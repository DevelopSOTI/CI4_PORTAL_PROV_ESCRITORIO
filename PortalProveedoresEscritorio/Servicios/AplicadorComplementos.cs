using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Repositorios;
using PortalProveedoresCore.Servicios;

namespace PortalProveedoresEscritorio.Servicios
{
    /// <summary>
    /// Orquesta la asociación manual de UN complemento de pago desde el
    /// escritorio: baja XML + adjuntos del portal y delega los bloques a
    /// <see cref="IAplicacionRepository.AplicarComplementoAsync"/>
    /// compartido con el Service.
    ///
    /// Gemelo de <see cref="AplicadorFacturas"/>: mismo patrón (single-shot
    /// del SincronizadorAplicacion automático), misma atomicidad. La
    /// diferencia clave: el complemento NO crea un nuevo documento en
    /// Microsip — se ASOCIA al crédito existente cuyo
    /// <c>CREDITO_ID = CREDITO_FK</c>.
    /// </summary>
    public sealed class AplicadorComplementos
    {
        private readonly IPortalApi            _api;
        private readonly IAplicacionRepository _repo;

        public AplicadorComplementos(IPortalApi api, IAplicacionRepository repo)
        {
            _api  = api  ?? throw new ArgumentNullException(nameof(api));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// <paramref name="usuarioMicrosip"/>: operador Microsip real que
        /// ejecuta la asociación — se sella en USUARIO_ASOCIO_COBRO del
        /// portal (réplica F_APLICAR_COMPLEMENTO.cs:672, ws_usuario=USUARIO).
        /// Mismo patrón que <see cref="AplicadorFacturas.AplicarAsync"/>.
        /// </summary>
        public async Task<ResultadoAplicacion> AplicarAsync(
            EmpresaEscritorio  empresa,
            ComplementoAplicar comp,
            string             usuarioMicrosip,
            IProgress<string>  progreso,
            CancellationToken  ct)
        {
            if (empresa == null) throw new ArgumentNullException(nameof(empresa));
            if (comp    == null) throw new ArgumentNullException(nameof(comp));

            // 1) Bajar XML del CFDI del complemento. Si falla, seguimos: el
            //    repositorio se las arregla si el REPOSITORIO_CFDI ya tiene
            //    el UUID de un ciclo previo.
            Reportar(progreso, "Descargando CFDI del portal…");
            CfdiXmlMicrosip cfdi = null;
            try
            {
                cfdi = await _api.ObtenerCfdiXmlAsync(comp.UUID, "C", ct).ConfigureAwait(false);
            }
            catch
            {
                // Silencioso por diseño.
            }

            // 2) Listar y descargar adjuntos extra del portal (PDF, OC).
            Reportar(progreso, "Buscando archivos adjuntos…");
            AdjuntoMicrosip[] listaAdj;
            try
            {
                listaAdj = await _api.ListarAdjuntosAsync(comp.DOCTO_CP_ID, empresa.Id, "C", ct)
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
                    try { bin = await _api.DescargarAdjuntoAsync(a.id, ct).ConfigureAwait(false); }
                    catch { }
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

            // 3) Callback que marca el complemento como aplicado en el portal.
            //    Se invoca DENTRO de la transacción Firebird. Recibe el
            //    CREDITO_FK (no compraId, no folio — el complemento se
            //    asocia a un crédito existente). Se pasa además el
            //    DOCTO_CP_ID del complemento específico y el operador real
            //    — réplica del SOAP ACTUALIZAR_COMPLEMENTO_PORTAL rama 'R'
            //    (F_APLICAR_COMPLEMENTO.cs:672-677: ws_usuario=USUARIO,
            //    ws_docto_cp_id=DOCTO_CP_ID), en vez de marcar TODOS los
            //    complementos del crédito como 'SYSDBA'.
            int creditoFkLocal = comp.CREDITO_FK;
            int doctoCpIdLocal = comp.DOCTO_CP_ID;
            string usuarioLocal = usuarioMicrosip ?? "";
            Func<int, Task<bool>> marcarPortal = async (creditoFk) =>
            {
                try
                {
                    return await _api.MarcarComplementoAplicadoAsync(
                            creditoFkLocal, doctoCpIdLocal, usuarioLocal, ct)
                        .ConfigureAwait(false);
                }
                catch
                {
                    return false;
                }
            };

            // 4) Aplicación real. AplicarComplementoAsync corre los bloques
            //    de asociación CFDI + actualización TIENE_CFD en DOCTOS_CP +
            //    marcación al portal + COMMIT atómico.
            Reportar(progreso, "Asociando CFDI en Microsip…");
            var adjuntosArr = adjuntos.ToArray();
            var resultado = await _repo.AplicarComplementoAsync(
                empresa.NombreCorto, comp, cfdi, adjuntosArr, marcarPortal, ct
            ).ConfigureAwait(false);

            // 5) Réplica de la rama TIENE_CFDI='S' del SOAP
            //    F_APLICAR_COMPLEMENTO.cs:794-819. Si el crédito ya tenía CFDI
            //    en Microsip, AplicarComplementoAsync devolvió CreditoYaConCfdi
            //    sin tocar adjuntos ni TIPO_DOCTO_MSP. Aquí ejecutamos el
            //    segundo paso para asociar los adjuntos extras del complemento
            //    y reflejar el destino del CFDI (texto del banner: "Ya tiene un
            //    CFD asociado", igual que el SOAP).
            //
            //    Solo el ESCRITORIO ejecuta este segundo paso — el Service deja
            //    el complemento como Saltadas (regla operacional).
            if (resultado != null && resultado.tipo == ResultadoAplicacionTipo.CreditoYaConCfdi)
            {
                Reportar(progreso, "El crédito ya tenía CFDI — asociando adjuntos extra…");
                var resultado2 = await _repo.AsociarComplementoYaConCfdiAsync(
                    empresa.NombreCorto, comp, adjuntosArr, marcarPortal, ct
                ).ConfigureAwait(false);

                return resultado2;
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
