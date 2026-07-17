using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Repositorios;
using PortalProveedoresCore.Servicios;
using PortalProveedoresService.Repositorios;

namespace PortalProveedoresService.Sincronizacion
{
    /// <summary>
    /// Hito final del ciclo: detecta facturas pendientes y, según APLICA_DIR,
    /// las aplica en Microsip (real, con COMMIT) o las deja para que el
    /// operador las procese manualmente desde la app de escritorio.
    ///
    /// Sub-fase 2.4 — APLICACIÓN REAL:
    ///   - APLICA_DIR=FALSE: solo loguea las pendientes.
    ///   - APLICA_DIR=TRUE: por cada factura pendiente, baja XML+adjuntos
    ///     del portal y llama a <see cref="IAplicacionRepository.AplicarFacturaAsync"/>,
    ///     que hace los bloques 1-15 dentro de una transacción Firebird y
    ///     hace COMMIT real si todo pasó. Si cualquier bloque truena (o el
    ///     portal rechaza el marcado), ROLLBACK automático — los dos lados
    ///     quedan consistentes.
    /// </summary>
    public sealed class SincronizadorAplicacion : ISincronizador
    {
        private readonly ICacheEmpresasAutorizadas _cacheEmpresas;
        private readonly IPortalApi                _api;
        private readonly IResolutorEmpresaMicrosip _resolutorEmpresa;
        private readonly IAplicacionRepository     _repo;
        private readonly NotificadorAplicacion     _notificador;

        public SincronizadorAplicacion(
            ICacheEmpresasAutorizadas cacheEmpresas,
            IPortalApi                api,
            IResolutorEmpresaMicrosip resolutorEmpresa,
            IAplicacionRepository     repo)
        {
            _cacheEmpresas    = cacheEmpresas;
            _api              = api;
            _resolutorEmpresa = resolutorEmpresa;
            _repo             = repo;
            _notificador      = new NotificadorAplicacion(api);
        }

        public string Nombre { get { return "Aplicación"; } }

        public async Task<bool> EjecutarAsync(CancellationToken ct)
        {
            // 1) ¿APLICA_DIR? Una sola llamada por ciclo — es global.
            bool aplicaDir;
            try
            {
                aplicaDir = await _api.ObtenerAplicaDirAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                EventoLog.Error("Aplicación: no se pudo leer APLICA_DIR del portal: " + ex.Message
                    + " — asumiendo FALSE (default seguro).");
                aplicaDir = false;
            }

            EventoLog.Info("Aplicación: APLICA_DIR = " + (aplicaDir ? "TRUE" : "FALSE")
                + (aplicaDir
                    ? " (modo PRODUCCIÓN: las facturas se aplicarán en Microsip con commit)"
                    : " (las pendientes deberán aplicarse manualmente desde la app de escritorio)"));

            // 2) Listamos pendientes por empresa autorizada.
            var empresas = await _cacheEmpresas.ObtenerAsync(ct).ConfigureAwait(false);
            if (empresas.Count == 0)
            {
                EventoLog.Info("Aplicación: no hay empresas autorizadas; nada que evaluar.");
                return true;
            }

            int totalFacturasPend     = 0;
            int totalComplementosPend = 0;
            int totalAplicadas        = 0;
            int totalSaltadas         = 0;
            int totalErrores          = 0;
            int empresasConPendientes = 0;
            int empresasConError      = 0;

            foreach (var emp in empresas)
            {
                ct.ThrowIfCancellationRequested();

                var nombreHumano = (!string.IsNullOrEmpty(emp.nombre_largo) ? emp.nombre_largo
                                  : !string.IsNullOrEmpty(emp.nombre)       ? emp.nombre
                                  : "(sin nombre)")
                                  + " [emp_id_msp=" + emp.emp_id_msp + "]";

                try
                {
                    var p = await _api.ObtenerPendientesAsync(emp.emp_id_msp, ct).ConfigureAwait(false);

                    if (p.facturas_pendientes == 0 && p.complementos_pendientes == 0)
                    {
                        EventoLog.Info("Aplicación · " + nombreHumano + ": sin pendientes.");
                        continue;
                    }

                    empresasConPendientes++;
                    totalFacturasPend     += p.facturas_pendientes;
                    totalComplementosPend += p.complementos_pendientes;

                    var mensaje = "Aplicación · " + nombreHumano + ": "
                                + p.facturas_pendientes     + " factura(s) y "
                                + p.complementos_pendientes + " complemento(s) pendiente(s).";

                    if (aplicaDir)
                        EventoLog.Info(mensaje + " (procediendo a aplicar)");
                    else
                        EventoLog.Warning(mensaje + " APLICA_DIR=FALSE — aplicar manualmente.");

                    LogearMuestra(nombreHumano, "Factura", p.facturas,
                        f => "folio=" + f.FOLIO + ", recepcion_id=" + f.RECEPCION_ID + ", uuid=" + f.UUID);
                    LogearMuestra(nombreHumano, "Complemento", p.complementos,
                        c => "folio=" + c.FOLIO + ", credito_fk=" + c.CREDITO_FK + ", uuid=" + c.UUID);

                    // Si APLICA_DIR=TRUE y hay facturas pendientes, aplicar.
                    if (aplicaDir && p.facturas_pendientes > 0)
                    {
                        var resumen = await AplicarFacturasEmpresaAsync(emp, ct).ConfigureAwait(false);
                        totalAplicadas += resumen.Ok;
                        totalSaltadas  += resumen.Saltadas;
                        totalErrores   += resumen.Errores;
                    }

                    // Fase 3: complementos. El Delphi histórico nunca los
                    // procesó (bloque comentado + función incompleta), pero
                    // aquí sí los aplicamos: asocian un CFDI a un crédito
                    // existente sin crear documento nuevo.
                    if (aplicaDir && p.complementos_pendientes > 0)
                    {
                        var resumen = await AplicarComplementosEmpresaAsync(emp, ct).ConfigureAwait(false);
                        totalAplicadas += resumen.Ok;
                        totalSaltadas  += resumen.Saltadas;
                        totalErrores   += resumen.Errores;
                    }
                }
                catch (Exception ex)
                {
                    EventoLog.Error("Aplicación · " + nombreHumano + ": " + ex.Message);
                    empresasConError++;
                }
            }

            // Resumen final.
            var resumenTxt = "Aplicación: " + empresasConPendientes + " empresa(s) con pendientes"
                + " (" + totalFacturasPend + " factura(s) y " + totalComplementosPend + " complemento(s) en total)";
            if (empresasConError > 0) resumenTxt += "; " + empresasConError + " con error";
            resumenTxt += " · APLICA_DIR=" + (aplicaDir ? "TRUE" : "FALSE") + ".";
            EventoLog.Info(resumenTxt);

            if (aplicaDir && (totalAplicadas + totalSaltadas + totalErrores) > 0)
            {
                EventoLog.Info("Aplicación: resumen → "
                    + totalAplicadas + " aplicada(s), "
                    + totalSaltadas  + " saltada(s), "
                    + totalErrores   + " con error.");
            }

            return true;
        }

        /// <summary>
        /// Para cada factura pendiente de la empresa: baja XML + adjuntos del
        /// portal y aplica realmente (COMMIT en Microsip + UPDATE en portal).
        /// </summary>
        private async Task<ResumenAplicacion> AplicarFacturasEmpresaAsync(EmpresaConfig emp, CancellationToken ct)
        {
            var resumen = new ResumenAplicacion();

            string nombreCorto = await _resolutorEmpresa.ObtenerNombreCortoAsync(emp.emp_id_msp, ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(nombreCorto))
            {
                EventoLog.Warning("Aplicación · emp_id_msp=" + emp.emp_id_msp
                    + ": no se pudo resolver NOMBRE_CORTO desde CONFIG.FDB. Saltando.");
                return resumen;
            }

            FacturaAplicar[] facturas;
            try
            {
                facturas = await _api.ObtenerFacturasAplicarAsync(emp.emp_id_msp, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                EventoLog.Error("Aplicación · " + nombreCorto + ": no se pudo obtener detalle de facturas: " + ex.Message);
                resumen.Errores++;
                return resumen;
            }

            if (facturas.Length == 0)
            {
                EventoLog.Info("Aplicación · " + nombreCorto + ": sin facturas con detalle completo "
                    + "(el conteo de pendientes era > 0 pero el SELECT detallado no devolvió ninguna).");
                return resumen;
            }

            EventoLog.Info("Aplicación · " + nombreCorto + ": aplicando "
                + facturas.Length + " factura(s) a Microsip...");

            // Réplica del Delphi Func_Facturas_3_3.pas:1248-1252 (y 3_2:683):
            // tras la aplicación automática exitosa, si el registro Windows
            // MAILS_SEND='True', se manda el correo "Contra recibo electronico"
            // al proveedor (PROCESO_ENVIAR, Func.pas:453-504). Se lee una vez
            // por empresa — el Delphi lo leía una vez al arrancar (D.MAILS_SEND).
            bool mailsSend = LeerMailsSend();

            foreach (var f in facturas)
            {
                ct.ThrowIfCancellationRequested();

                string etiqueta = "factura folio_compra=" + (string.IsNullOrEmpty(f.FOLIO_COMPRA) ? "(sin folio)" : f.FOLIO_COMPRA)
                                + ", recepcion=" + (string.IsNullOrEmpty(f.FOLIO_RECEPCION) ? "?" : f.FOLIO_RECEPCION)
                                + ", uuid=" + (string.IsNullOrEmpty(f.UUID) ? "?" : f.UUID.Substring(0, Math.Min(8, f.UUID.Length)) + "...");

                // Las facturas SIN recepción (RECEP_ID=0) requieren que el
                // operador elija un artículo NO almacenable y una condición
                // de pago en el modal del Escritorio — el Service no tiene
                // esa información, así que las omite y deja que el humano
                // decida desde la app de escritorio.
                if (f.RECEP_ID == 0)
                {
                    resumen.Saltadas++;
                    EventoLog.Info("Aplicación · " + nombreCorto + " · " + etiqueta
                        + ": sin recepción ligada — se omite del Service "
                        + "(debe aplicarse manualmente desde el Escritorio).");
                    continue;
                }

                // 1) Bajar XML del CFDI.
                CfdiXmlMicrosip cfdi = null;
                try
                {
                    cfdi = await _api.ObtenerCfdiXmlAsync(f.UUID, "F", ct).ConfigureAwait(false);
                }
                catch (Exception exCfdi)
                {
                    EventoLog.Warning("Aplicación · " + nombreCorto + " · " + etiqueta
                        + ": no se pudo bajar XML del portal: " + exCfdi.Message
                        + ". Se intentará igual (servirá si REPOSITORIO_CFDI ya lo tiene).");
                }

                // 2) Bajar lista de adjuntos + binarios.
                var adjuntosDescargados = await DescargarAdjuntosAsync(f, emp.emp_id_msp, ct).ConfigureAwait(false);

                // 3) Aplicar con COMMIT real. El callback de marcar el portal
                //    se invoca DENTRO de la transacción Firebird — si el portal
                //    falla, rollback de Firebird (atomicidad de los dos lados).
                int recepIdLocal        = f.RECEP_ID;
                int facturaMysqlIdLocal = f.DOCTO_CM_ID;
                Func<int, string, Task<bool>> marcarPortal = async (compraId, folioMsp) =>
                {
                    try
                    {
                        return await _api.MarcarFacturaAplicadaAsync(recepIdLocal, folioMsp, compraId, ct).ConfigureAwait(false);
                    }
                    catch (Exception exMarcar)
                    {
                        EventoLog.Error("Aplicación · " + nombreCorto + " · " + etiqueta
                            + ": marcarFacturaAplicada falló: " + exMarcar.Message);
                        return false;
                    }
                };

                // Callback alternativo: sincroniza el portal cuando la
                // recepción Microsip ya está facturada (ESTATUS='F') y se
                // encontró la compra ligada. Réplica del SOAP
                // ACTUALIZAR_FACTURA_PORTAL_ESCT (UPDATE WHERE DOCTO_CM_ID).
                Func<int, string, Task<bool>> sincronizarPortalYaAplicada = async (compraId, folioCompra) =>
                {
                    try
                    {
                        return await _api.SincronizarFacturaYaAplicadaAsync(
                            facturaMysqlIdLocal, recepIdLocal, folioCompra, compraId, ct
                        ).ConfigureAwait(false);
                    }
                    catch (Exception exSinc)
                    {
                        EventoLog.Error("Aplicación · " + nombreCorto + " · " + etiqueta
                            + ": SincronizarFacturaYaAplicada falló: " + exSinc.Message);
                        return false;
                    }
                };

                var r = await _repo.AplicarFacturaAsync(nombreCorto, f, cfdi, adjuntosDescargados,
                    marcarPortal, sincronizarPortalYaAplicada, ct).ConfigureAwait(false);

                switch (r.tipo)
                {
                    case ResultadoAplicacionTipo.OkDryRun: // semánticamente OK
                        resumen.Ok++;
                        EventoLog.Info("Aplicación · " + nombreCorto + " · " + etiqueta + " · ✓ " + r.mensaje);

                        // Correo "Contra recibo electronico" al proveedor —
                        // réplica del Delphi Func_Facturas_3_3.pas:1248-1252:
                        // tras COMMIT + portal actualizado, si MAILS_SEND='True',
                        // PROCESO_ENVIAR (Func.pas:453-504). Se excluye el caso
                        // "ya estaba en Microsip" (la factura no se aplicó AHORA;
                        // el correo se mandó cuando se aplicó originalmente).
                        // El Delphi NO manda este correo para complementos
                        // (PROCESO_ENVIAR solo se invoca desde Func_Facturas_3_2/3_3)
                        // — aquí tampoco. Best-effort: jamás rompe el ciclo.
                        if (mailsSend
                            && r.portalMarcado
                            && !(r.mensaje ?? "").Contains("ya estaba en Microsip"))
                        {
                            await NotificarProveedorAsync(nombreCorto, etiqueta, f, emp, ct).ConfigureAwait(false);
                        }
                        break;

                    case ResultadoAplicacionTipo.RecepcionNoExiste:
                    case ResultadoAplicacionTipo.RecepcionYaFacturada:
                    case ResultadoAplicacionTipo.FolioCompraDuplicado:
                    case ResultadoAplicacionTipo.SerieWebNoConfigurada:
                        resumen.Saltadas++;
                        EventoLog.Warning("Aplicación · " + nombreCorto + " · " + etiqueta + ": " + r.mensaje);
                        break;

                    case ResultadoAplicacionTipo.RecepcionYaFacturadaSincronizar:
                        // El repository solo deja este tipo si NO pudo
                        // sincronizar el portal (compra Microsip encontrada
                        // pero el endpoint del portal falló). Si lo logró,
                        // ya vino como OkDryRun arriba.
                        resumen.Saltadas++;
                        EventoLog.Warning("Aplicación · " + nombreCorto + " · " + etiqueta + ": " + r.mensaje);
                        break;

                    case ResultadoAplicacionTipo.RecepcionCancelada:
                        // Réplica del SOAP F_APLICAR_FACTURA.cs:155-186 — la
                        // recepción origen está cancelada en Microsip. Hay
                        // que marcar la factura como rechazada en el portal
                        // (POST /api/aplicacion/factura-recep-cancelada).
                        bool marcadoRecepCanc = false;
                        try
                        {
                            marcadoRecepCanc = await _api
                                .MarcarFacturaRecepCanceladaAsync(f.DOCTO_CM_ID, "SERVICIO", ct)
                                .ConfigureAwait(false);
                        }
                        catch (Exception exMarcarRecepCanc)
                        {
                            EventoLog.Error("Aplicación · " + nombreCorto + " · " + etiqueta
                                + ": MarcarFacturaRecepCancelada falló: " + exMarcarRecepCanc.Message);
                        }
                        resumen.Saltadas++;
                        EventoLog.Warning("Aplicación · " + nombreCorto + " · " + etiqueta + ": " + r.mensaje
                            + (marcadoRecepCanc
                                ? " Factura marcada como rechazada en el portal."
                                : " AVISO: no se pudo marcar en el portal."));
                        break;

                    case ResultadoAplicacionTipo.Error:
                    case ResultadoAplicacionTipo.ErrorConexion:
                    default:
                        resumen.Errores++;
                        EventoLog.Error("Aplicación · " + nombreCorto + " · " + etiqueta
                            + ": error en bloque " + r.ultimoBloque + " — " + r.mensaje);
                        break;
                }
            }

            return resumen;
        }

        /// <summary>
        /// Análogo a <see cref="AplicarFacturasEmpresaAsync"/> pero para
        /// complementos: baja XML+adjuntos del portal y los asocia al crédito
        /// Microsip correspondiente (sin crear documento nuevo).
        /// </summary>
        private async Task<ResumenAplicacion> AplicarComplementosEmpresaAsync(EmpresaConfig emp, CancellationToken ct)
        {
            var resumen = new ResumenAplicacion();

            string nombreCorto = await _resolutorEmpresa.ObtenerNombreCortoAsync(emp.emp_id_msp, ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(nombreCorto))
            {
                EventoLog.Warning("Aplicación · emp_id_msp=" + emp.emp_id_msp
                    + ": no se pudo resolver NOMBRE_CORTO para complementos. Saltando.");
                return resumen;
            }

            ComplementoAplicar[] complementos;
            try
            {
                complementos = await _api.ObtenerComplementosAplicarAsync(emp.emp_id_msp, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                EventoLog.Error("Aplicación · " + nombreCorto + ": no se pudo obtener detalle de complementos: " + ex.Message);
                resumen.Errores++;
                return resumen;
            }

            if (complementos.Length == 0)
            {
                EventoLog.Info("Aplicación · " + nombreCorto + ": sin complementos con detalle completo.");
                return resumen;
            }

            EventoLog.Info("Aplicación · " + nombreCorto + ": asociando "
                + complementos.Length + " complemento(s) a créditos de Microsip...");

            foreach (var c in complementos)
            {
                ct.ThrowIfCancellationRequested();

                string etiqueta = "complemento folio_pago=" + (string.IsNullOrEmpty(c.FOLIO_PAGO) ? "(sin folio)" : c.FOLIO_PAGO)
                                + ", credito=" + (string.IsNullOrEmpty(c.FOLIO_CREDITO) ? "?" : c.FOLIO_CREDITO)
                                + ", uuid=" + (string.IsNullOrEmpty(c.UUID) ? "?" : c.UUID.Substring(0, Math.Min(8, c.UUID.Length)) + "...");

                CfdiXmlMicrosip cfdi = null;
                try
                {
                    cfdi = await _api.ObtenerCfdiXmlAsync(c.UUID, "C", ct).ConfigureAwait(false);
                }
                catch (Exception exCfdi)
                {
                    EventoLog.Warning("Aplicación · " + nombreCorto + " · " + etiqueta
                        + ": no se pudo bajar XML del portal: " + exCfdi.Message
                        + ". Se intentará igual (servirá si REPOSITORIO_CFDI ya lo tiene).");
                }

                // Adjuntos de complemento (tipo='C').
                var adjuntosDescargados = await DescargarAdjuntosComplementoAsync(c, emp.emp_id_msp, ct).ConfigureAwait(false);

                int creditoFkLocal = c.CREDITO_FK;
                Func<int, Task<bool>> marcarPortal = async (cfk) =>
                {
                    try
                    {
                        return await _api.MarcarComplementoAplicadoAsync(cfk, ct).ConfigureAwait(false);
                    }
                    catch (Exception exMarcar)
                    {
                        EventoLog.Error("Aplicación · " + nombreCorto + " · " + etiqueta
                            + ": marcarComplementoAplicado falló: " + exMarcar.Message);
                        return false;
                    }
                };

                var r = await _repo.AplicarComplementoAsync(nombreCorto, c, cfdi, adjuntosDescargados, marcarPortal, ct).ConfigureAwait(false);

                switch (r.tipo)
                {
                    case ResultadoAplicacionTipo.OkDryRun: // semánticamente OK
                        resumen.Ok++;
                        EventoLog.Info("Aplicación · " + nombreCorto + " · " + etiqueta + " · ✓ " + r.mensaje);
                        break;

                    case ResultadoAplicacionTipo.CreditoNoExiste:
                    case ResultadoAplicacionTipo.CreditoYaConCfdi:
                        resumen.Saltadas++;
                        EventoLog.Warning("Aplicación · " + nombreCorto + " · " + etiqueta + ": " + r.mensaje);
                        break;

                    case ResultadoAplicacionTipo.Error:
                    case ResultadoAplicacionTipo.ErrorConexion:
                    default:
                        resumen.Errores++;
                        EventoLog.Error("Aplicación · " + nombreCorto + " · " + etiqueta
                            + ": error en bloque " + r.ultimoBloque + " — " + r.mensaje);
                        break;
                }
            }

            return resumen;
        }

        /// <summary>
        /// Descarga adjuntos de un complemento (tipo='C'). Mismo flujo que
        /// para facturas, solo cambia el doctoId (DOCTO_CP_ID) y el tipo.
        /// </summary>
        private async Task<AdjuntoDescargado[]> DescargarAdjuntosComplementoAsync(ComplementoAplicar c, int empIdMsp, CancellationToken ct)
        {
            AdjuntoMicrosip[] lista;
            try
            {
                lista = await _api.ListarAdjuntosAsync(c.DOCTO_CP_ID, empIdMsp, "C", ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                EventoLog.Warning("Aplicación · no se pudo listar adjuntos del complemento DOCTO_CP_ID="
                    + c.DOCTO_CP_ID + " emp=" + empIdMsp + ": " + ex.Message);
                return new AdjuntoDescargado[0];
            }

            if (lista == null || lista.Length == 0) return new AdjuntoDescargado[0];

            var resultados = new List<AdjuntoDescargado>(lista.Length);
            foreach (var a in lista)
            {
                ct.ThrowIfCancellationRequested();
                byte[] bytes = null;
                try { bytes = await _api.DescargarAdjuntoAsync(a.id, ct).ConfigureAwait(false); }
                catch (Exception ex) { EventoLog.Warning("Aplicación · adjunto id=" + a.id + " no se pudo descargar: " + ex.Message); }
                if (bytes == null || bytes.Length == 0)
                {
                    EventoLog.Warning("Aplicación · adjunto complemento id=" + a.id + " vacío — omitido.");
                    continue;
                }
                resultados.Add(new AdjuntoDescargado
                {
                    Id             = a.id,
                    NombreOriginal = a.nombre_original ?? ("adjunto_" + a.id + ".bin"),
                    Contenido      = bytes,
                });
            }
            return resultados.ToArray();
        }

        /// <summary>
        /// Lista los adjuntos del portal para esta factura y descarga el
        /// binario de cada uno. Si alguno falla la descarga, lo omite del
        /// array (el repository tampoco lo intentará insertar).
        /// </summary>
        private async Task<AdjuntoDescargado[]> DescargarAdjuntosAsync(FacturaAplicar f, int empIdMsp, CancellationToken ct)
        {
            AdjuntoMicrosip[] lista;
            try
            {
                lista = await _api.ListarAdjuntosAsync(f.DOCTO_CM_ID, empIdMsp, "F", ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                EventoLog.Warning("Aplicación · no se pudo listar adjuntos de la factura DOCTO_CM_ID="
                    + f.DOCTO_CM_ID + " emp=" + empIdMsp + ": " + ex.Message);
                return new AdjuntoDescargado[0];
            }

            if (lista == null || lista.Length == 0) return new AdjuntoDescargado[0];

            var resultados = new List<AdjuntoDescargado>(lista.Length);
            foreach (var a in lista)
            {
                ct.ThrowIfCancellationRequested();
                byte[] bytes = null;
                try
                {
                    bytes = await _api.DescargarAdjuntoAsync(a.id, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    EventoLog.Warning("Aplicación · adjunto id=" + a.id + " no se pudo descargar: " + ex.Message);
                }
                if (bytes == null || bytes.Length == 0)
                {
                    EventoLog.Warning("Aplicación · adjunto id=" + a.id + " vacío o sin contenido — omitido.");
                    continue;
                }
                resultados.Add(new AdjuntoDescargado
                {
                    Id             = a.id,
                    NombreOriginal = a.nombre_original ?? ("adjunto_" + a.id + ".bin"),
                    Contenido      = bytes,
                });
            }
            return resultados.ToArray();
        }

        /// <summary>
        /// Envía el correo "Contra recibo electronico" al proveedor tras la
        /// aplicación automática exitosa. Réplica del Delphi
        /// Func_Facturas_3_3.pas:1248-1252 → PROCESO_ENVIAR (Func.pas:453-504).
        /// Best-effort: cualquier falla se loguea como Warning y el ciclo sigue.
        /// </summary>
        private async Task NotificarProveedorAsync(string nombreCorto, string etiqueta, FacturaAplicar f, EmpresaConfig emp, CancellationToken ct)
        {
            try
            {
                var rn = await _notificador.NotificarFacturaAplicadaAsync(
                    f.PROVEEDOR_ID,
                    emp.emp_id_msp,
                    f.FOLIO_COMPRA,
                    f.FECHA_FACTURA,
                    f.FECHA_PAGO,        // fecha estimada de pago (no la de recepción)
                    ct
                ).ConfigureAwait(false);

                if (rn != null && rn.Enviado)
                {
                    EventoLog.Info("Aplicación · " + nombreCorto + " · " + etiqueta
                        + ": notificación enviada al proveedor"
                        + (string.IsNullOrEmpty(rn.Destino) ? "." : " (" + rn.Destino + ")."));
                }
                else
                {
                    EventoLog.Warning("Aplicación · " + nombreCorto + " · " + etiqueta
                        + ": no se pudo notificar al proveedor por correo"
                        + (rn != null && !string.IsNullOrWhiteSpace(rn.Mensaje) ? " — " + rn.Mensaje : "."));
                }
            }
            catch (Exception ex)
            {
                EventoLog.Warning("Aplicación · " + nombreCorto + " · " + etiqueta
                    + ": error inesperado al notificar al proveedor: " + ex.Message);
            }
        }

        /// <summary>
        /// Lee MAILS_SEND del registro Windows (HKLM) — mismo flag que el
        /// Delphi cargaba en D.MAILS_SEND y comparaba contra 'True' en
        /// Func_Facturas_3_3.pas:1249. Comparación EXACTA ("True"), igual que
        /// el '=' de Delphi (case-sensitive). Si el registro no se puede leer,
        /// se asume False: no se manda correo.
        /// </summary>
        private static bool LeerMailsSend()
        {
            try
            {
                var reg = new RegistrosWindows();
                if (!reg.LeerRegistros(false)) return false;
                return (reg.MAILS_SEND ?? "") == "True";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Vuelca al log las primeras 3 entradas del array para que el operador
        /// vea folios y UUIDs concretos sin necesidad de ir a la app web.
        /// </summary>
        private static void LogearMuestra<T>(string nombreHumano, string tipo, T[] items, Func<T, string> describir)
        {
            if (items == null || items.Length == 0) return;
            int max = Math.Min(3, items.Length);
            for (int i = 0; i < max; i++)
                EventoLog.Info("Aplicación · " + nombreHumano + " · " + tipo + " #" + (i + 1) + ": " + describir(items[i]));
            if (items.Length > max)
                EventoLog.Info("Aplicación · " + nombreHumano + " · ... y " + (items.Length - max) + " " + tipo.ToLower() + "(s) más.");
        }

        private sealed class ResumenAplicacion
        {
            public int Ok       { get; set; }
            public int Saltadas { get; set; }
            public int Errores  { get; set; }
        }
    }
}
