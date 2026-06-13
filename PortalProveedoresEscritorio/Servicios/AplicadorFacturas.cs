using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Repositorios;
using PortalProveedoresCore.Servicios;

namespace PortalProveedoresEscritorio.Servicios
{
    /// <summary>
    /// Orquesta la aplicación manual de UNA factura desde el escritorio:
    /// baja XML + adjuntos del portal y delega los bloques 1-15 al
    /// <see cref="IAplicacionRepository"/> compartido con el Service.
    ///
    /// Es la versión "single shot" del <c>SincronizadorAplicacion</c> del
    /// Service — misma cadena de pasos, mismo callback de marcado al portal
    /// dentro de la transacción Firebird, misma atomicidad (rollback si
    /// cualquier paso falla). La única diferencia: aquí el operador es quien
    /// dispara cada aplicación desde la UI, una a la vez.
    ///
    /// Reporta el avance vía <see cref="IProgress{T}"/> para que el modal
    /// pueda mostrar texto en vivo ("Descargando 2 adjuntos...", etc.) sin
    /// acoplarse al hilo de UI.
    /// </summary>
    public sealed class AplicadorFacturas
    {
        private readonly IPortalApi             _api;
        private readonly IAplicacionRepository  _repo;
        private readonly NotificadorAplicacion  _notificador;

        public AplicadorFacturas(IPortalApi api, IAplicacionRepository repo, NotificadorAplicacion notificador = null)
        {
            _api         = api  ?? throw new ArgumentNullException(nameof(api));
            _repo        = repo ?? throw new ArgumentNullException(nameof(repo));
            // El notificador es opcional para no romper consumidores existentes
            // (tests, escenarios sin red). Si no se pasa, se arma uno por defecto
            // con el mismo IPortalApi — es lo que quiere el flujo manual.
            _notificador = notificador ?? new NotificadorAplicacion(api);
        }

        /// <summary>
        /// Aplica la factura indicada del portal en Microsip de la empresa.
        /// El resultado refleja el último bloque ejecutado y el folio que
        /// quedó asignado en Microsip (si llegó al bloque final = COMMIT).
        ///
        /// Si la factura tiene <c>RECEP_ID = 0</c> (sin recepción), <b>se
        /// requieren</b> <paramref name="articuloNoAlmacenable"/> y
        /// <paramref name="condicionPago"/> — el operador los eligió en los
        /// combos del modal y el flujo SOAP <c>APLICAR_SIN_RECEPCION</c>
        /// los necesita. Para facturas con recepción esos parámetros se
        /// ignoran (el detalle se copia de la recepción origen).
        /// </summary>
        public async Task<ResultadoAplicacion> AplicarAsync(
            EmpresaEscritorio          empresa,
            FacturaPendienteEscritorio facturaUi,
            string                     articuloNoAlmacenable,
            string                     condicionPago,
            string                     usuarioMicrosip,
            IProgress<string>          progreso,
            CancellationToken          ct)
        {
            if (empresa   == null) throw new ArgumentNullException(nameof(empresa));
            if (facturaUi == null) throw new ArgumentNullException(nameof(facturaUi));

            Reportar(progreso, "Buscando datos completos de la factura…");

            // 1) Resolver el shape FacturaAplicar — el grid solo tiene un
            //    DTO de UI; el repositorio necesita el shape exacto del
            //    SELECT (FacturaAplicar). El endpoint devuelve la lista
            //    completa y filtramos por DOCTO_CM_ID (único en MySQL).
            //    Usar RECEP_ID rompía cuando había varias facturas sin
            //    recepción — todas comparten RECEP_ID=0.
            FacturaAplicar[] todas;
            try
            {
                todas = await _api.ObtenerFacturasAplicarAsync(empresa.Id, ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Error(2, "No se pudo obtener la lista de facturas a aplicar: " + ex.Message);
            }

            var fa = (todas ?? new FacturaAplicar[0])
                .FirstOrDefault(x => x.DOCTO_CM_ID == facturaUi.DOCTO_CM_ID);

            if (fa == null)
            {
                // La factura ya no es elegible — probablemente otro operador
                // la aplicó/rechazó entre el listado y este click.
                return Error(1, "La factura ya no aparece como pendiente. " +
                                "Recarga la lista para ver el estado actual.");
            }

            bool sinRecepcion = (fa.RECEP_ID == 0);
            if (sinRecepcion)
            {
                if (string.IsNullOrWhiteSpace(articuloNoAlmacenable))
                    return Error(1, "Para facturas sin recepción se requiere elegir un "
                                    + "artículo NO almacenable en el modal.");
                if (string.IsNullOrWhiteSpace(condicionPago))
                    return Error(1, "Para facturas sin recepción se requiere elegir una "
                                    + "condición de pago en el modal.");
            }

            // 2) Bajar XML del CFDI. Si falla, seguimos: el bloque 9 del
            //    repositorio detecta si el REPOSITORIO_CFDI ya tiene el UUID
            //    de un ciclo previo. Misma política que el SincronizadorAplicacion.
            Reportar(progreso, "Descargando CFDI del portal…");
            CfdiXmlMicrosip cfdi = null;
            try
            {
                cfdi = await _api.ObtenerCfdiXmlAsync(facturaUi.UUID, "F", ct)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Silencioso por diseño — el bloque 9 lo manejará. Si tampoco
                // existe en Microsip, el repositorio devuelve el error claro
                // y ahí lo ve el operador.
            }

            // 3) Listar y descargar los adjuntos extra del portal (PDFs, OC).
            Reportar(progreso, "Buscando archivos adjuntos…");
            AdjuntoMicrosip[] listaAdj;
            try
            {
                listaAdj = await _api.ListarAdjuntosAsync(fa.DOCTO_CM_ID, empresa.Id, "F", ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Error(3, "No se pudo listar adjuntos del portal: " + ex.Message);
            }

            var adjuntos = new List<AdjuntoDescargado>();
            if (listaAdj != null && listaAdj.Length > 0)
            {
                int idx = 0;
                foreach (var a in listaAdj)
                {
                    ct.ThrowIfCancellationRequested();
                    idx++;
                    Reportar(progreso, "Descargando adjunto " + idx + " de " + listaAdj.Length + "…");

                    byte[] bin = null;
                    try
                    {
                        bin = await _api.DescargarAdjuntoAsync(a.id, ct).ConfigureAwait(false);
                    }
                    catch
                    {
                        // bin queda null → se omite, se reporta como "omitido"
                        // en el resultado del repositorio.
                    }

                    if (bin == null) continue;
                    adjuntos.Add(new AdjuntoDescargado
                    {
                        Id             = a.id,
                        NombreOriginal = string.IsNullOrEmpty(a.nombre_original)
                                            ? a.nombre_archivo
                                            : a.nombre_original,
                        Contenido      = bin,
                    });
                }
            }

            // 4) Callback que marca la factura como aplicada en el portal.
            //    Se invoca DENTRO de la transacción Firebird, en el bloque 14
            //    del repositorio. Si falla, rollback de Firebird — los dos
            //    lados quedan consistentes.
            int recepIdLocal       = fa.RECEP_ID;
            int facturaMysqlIdLocal = fa.DOCTO_CM_ID;
            Func<int, string, Task<bool>> marcarPortal = async (compraId, folioMsp) =>
            {
                try
                {
                    return await _api
                        .MarcarFacturaAplicadaAsync(recepIdLocal, folioMsp, compraId, ct)
                        .ConfigureAwait(false);
                }
                catch
                {
                    return false;
                }
            };

            // Callback alternativo: sincroniza el portal con una compra YA
            // existente en Microsip cuando la recepción ya tiene ESTATUS='F'.
            // Réplica del SOAP ACTUALIZAR_FACTURA_PORTAL_ESCT — UPDATE por
            // DOCTO_CM_ID del portal (NO por RECEP_ID). Solo aplica al flujo
            // CON recepción (el flujo "sin recepción" nunca entra a este caso).
            Func<int, string, Task<bool>> sincronizarPortalYaAplicada = async (compraId, folioCompra) =>
            {
                try
                {
                    return await _api
                        .SincronizarFacturaYaAplicadaAsync(
                            facturaMysqlIdLocal, recepIdLocal, folioCompra, compraId, ct)
                        .ConfigureAwait(false);
                }
                catch
                {
                    return false;
                }
            };

            // Callback de marcado para el flujo SIN recepción. El SOAP usaba
            // ACTUALIZAR_FACTURA_PORTAL_ESCT (services/facturas.php:172-234):
            // UPDATE FACTURA_PROVEEDOR_33 ... WHERE DOCTO_CM_ID (id MySQL de
            // la factura) + UPDATE RECEPCIONES WHERE RECEP_ID=0 (no-op). No
            // puede usarse marcar-factura-aplicada porque ese endpoint marca
            // por RECEP_ID y rechaza recep_id<=0 — con RECEP_ID=0 el flujo
            // jamás podría commitear. El primer argumento del callback es el
            // DOCTO_CM_ID Microsip de la compra recién creada (lo escribe el
            // portal en COMPRA_ID).
            Func<int, string, Task<bool>> marcarPortalSinRecepcion = async (compraId, folioMsp) =>
            {
                try
                {
                    return await _api
                        .SincronizarFacturaYaAplicadaAsync(
                            facturaMysqlIdLocal, 0, folioMsp, compraId, ct)
                        .ConfigureAwait(false);
                }
                catch
                {
                    return false;
                }
            };

            // 5) Aplicación real. Esto abre Firebird de la empresa
            //    (NOMBRE_CORTO), corre los bloques con COMMIT si todo va
            //    bien o ROLLBACK ante cualquier fallo. El flujo SIN
            //    recepción es una réplica de APLICAR_SIN_RECEPCION del SOAP
            //    (F_APLICAR_FACTURA.cs:1007-1689) — crea el DOCTOS_CM tipo
            //    'C' con 1 línea genérica del artículo no almacenable elegido.
            Reportar(progreso, sinRecepcion
                ? "Aplicando en Microsip (sin recepción)…"
                : "Aplicando en Microsip…");

            ResultadoAplicacion resultado;
            if (sinRecepcion)
            {
                resultado = await _repo.AplicarFacturaSinRecepcionAsync(
                    empresa.NombreCorto, fa, articuloNoAlmacenable, condicionPago,
                    cfdi, adjuntos.ToArray(), marcarPortalSinRecepcion, ct
                ).ConfigureAwait(false);
            }
            else
            {
                resultado = await _repo.AplicarFacturaAsync(
                    empresa.NombreCorto, fa, cfdi, adjuntos.ToArray(),
                    marcarPortal, sincronizarPortalYaAplicada, ct
                ).ConfigureAwait(false);
            }

            // Réplica del SOAP F_APLICAR_FACTURA.cs:155-186 — cuando la
            // recepción origen está cancelada en Microsip, el repositorio
            // sale en bloque 1 con tipo=RecepcionCancelada y NO toca Firebird.
            // Aquí marcamos la factura como rechazada en el portal MySQL
            // (POST /api/aplicacion/factura-recep-cancelada) para que el
            // proveedor la vea rechazada y la vuelva a subir contra otra
            // recepción. El usuario Microsip se sella en USUARIO_RECH_FACTURA.
            if (resultado != null && resultado.tipo == ResultadoAplicacionTipo.RecepcionCancelada)
            {
                Reportar(progreso, "Marcando factura como rechazada en el portal…");
                string usuarioParaSello = string.IsNullOrWhiteSpace(usuarioMicrosip)
                                            ? "OPERADOR-ESCRITORIO"
                                            : usuarioMicrosip;
                bool marcado = false;
                try
                {
                    marcado = await _api
                        .MarcarFacturaRecepCanceladaAsync(fa.DOCTO_CM_ID, usuarioParaSello, ct)
                        .ConfigureAwait(false);
                }
                catch
                {
                    marcado = false;
                }

                resultado.portalMarcado = marcado;
                resultado.mensaje = (resultado.mensaje ?? "")
                    + (marcado
                        ? " La factura ya fue marcada como rechazada en el portal."
                        : " AVISO: NO se pudo marcar la factura como rechazada en el portal — revísela manualmente.");
            }

            // Réplica del SOAP F_APLICAR_FACTURA.cs:985-990 (con recepción) y
            // F_APLICAR_FACTURA.cs:1673-1677 (sin recepción): tras un COMMIT
            // exitoso, mandar el correo de "factura recibida → pendiente de pago"
            // al proveedor. Best-effort: si falla SMTP o no hay correo, la
            // aplicación sigue siendo exitosa.
            //
            // Excluimos RecepcionYaFacturadaSincronizar porque ahí la factura
            // YA estaba aplicada en Microsip — el proveedor ya recibió su
            // correo cuando se aplicó originalmente. Mandar uno nuevo sería
            // un duplicado confuso.
            if (resultado != null
                && resultado.portalMarcado
                && resultado.tipo == ResultadoAplicacionTipo.OkDryRun
                && !(resultado.mensaje ?? "").Contains("ya estaba en Microsip"))
            {
                Reportar(progreso, "Notificando al proveedor por correo…");
                NotificadorAplicacion.ResultadoNotificacion rn = null;
                try
                {
                    rn = await _notificador.NotificarFacturaAplicadaAsync(
                        fa.PROVEEDOR_ID,
                        empresa.Id,
                        fa.FOLIO_COMPRA,
                        fa.FECHA_FACTURA,
                        fa.FECHA_RECEPCION,
                        ct
                    ).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Best-effort: cualquier excepción se traga.
                    rn = new NotificadorAplicacion.ResultadoNotificacion
                    {
                        Enviado = false,
                        Mensaje = "Error inesperado al notificar al proveedor: " + ex.Message,
                    };
                }

                if (rn != null && rn.Enviado)
                {
                    resultado.mensajeAdicionalEscritorio =
                        "Notificación enviada al proveedor"
                        + (string.IsNullOrEmpty(rn.Destino) ? "." : (" (" + rn.Destino + ")."));
                }
                else
                {
                    string detalle = (rn != null && !string.IsNullOrWhiteSpace(rn.Mensaje))
                        ? " — " + rn.Mensaje
                        : "";
                    resultado.mensajeAdicionalEscritorio =
                        "No se pudo notificar al proveedor por correo (sin destinatario o SMTP no configurado)"
                        + detalle;
                }
            }

            return resultado;
        }

        private static void Reportar(IProgress<string> p, string msg)
        {
            if (p != null) p.Report(msg);
        }

        private static ResultadoAplicacion Error(int bloque, string mensaje)
        {
            return new ResultadoAplicacion
            {
                tipo         = ResultadoAplicacionTipo.Error,
                ultimoBloque = bloque,
                mensaje      = mensaje,
            };
        }
    }
}
