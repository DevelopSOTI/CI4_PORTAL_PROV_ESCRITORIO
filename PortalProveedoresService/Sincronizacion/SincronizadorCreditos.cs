using System;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Servicios;
using PortalProveedoresService.Repositorios;

namespace PortalProveedoresService.Sincronizacion
{
    /// <summary>
    /// Hito per-empresa: actualiza los CRÉDITOS (pagos PPD) en el portal.
    /// Filtro Microsip: <c>cc.tipo='P'</c> en doctos_cp con
    /// naturaleza='R'.
    ///
    /// Solo cabecera; el detalle (CREDITOS_DET con XML del CFDI) se sumará
    /// en una fase posterior. Itera empresas autorizadas con resolutor +
    /// cache compartido (mismo patrón que Recepciones).
    /// </summary>
    public sealed class SincronizadorCreditos : ISincronizador
    {
        private readonly IResolutorEmpresaMicrosip _resolutor;
        private readonly ICacheEmpresasAutorizadas _cacheEmpresas;
        private readonly IDoctosCpRepository _repo;
        private readonly IPortalApi _api;

        public SincronizadorCreditos(
            IResolutorEmpresaMicrosip resolutor,
            ICacheEmpresasAutorizadas cacheEmpresas,
            IDoctosCpRepository repo,
            IPortalApi api)
        {
            _resolutor     = resolutor;
            _cacheEmpresas = cacheEmpresas;
            _repo          = repo;
            _api           = api;
        }

        public string Nombre { get { return "Créditos"; } }

        public async Task<bool> EjecutarAsync(CancellationToken ct)
        {
            var empresas = await _cacheEmpresas.ObtenerAsync(ct).ConfigureAwait(false);
            if (empresas.Count == 0)
            {
                EventoLog.Info("Sincronizando créditos: no hay empresas autorizadas; nada que hacer.");
                return true;
            }

            int totalProcesadas  = 0;
            int empresasConError = 0;
            int empresasOmitidas = 0;

            foreach (var emp in empresas)
            {
                ct.ThrowIfCancellationRequested();

                // Incluimos emp_id_msp para diagnóstico en multi-empresa.
                var nombreHumano = (!string.IsNullOrEmpty(emp.nombre_largo) ? emp.nombre_largo
                                  : !string.IsNullOrEmpty(emp.nombre)       ? emp.nombre
                                  : "(sin nombre)")
                                  + " [emp_id_msp=" + emp.emp_id_msp + "]";

                try
                {
                    var nombreCorto = await _resolutor.ObtenerNombreCortoAsync(emp.emp_id_msp, ct).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(nombreCorto))
                    {
                        EventoLog.Warning("Créditos · " + nombreHumano + ": no se encontró en CONFIG.FDB; se omite.");
                        empresasOmitidas++;
                        continue;
                    }

                    var desde = CalcularDesde(emp);
                    EventoLog.Info("Créditos · " + nombreHumano + ": leyendo Microsip"
                        + (desde.HasValue ? " (desde " + desde.Value.ToString("yyyy-MM-dd HH:mm") + ")" : " (últimos 90 días)")
                        + "...");

                    var creditos = await _repo.ListarAsync(nombreCorto, "P", desde, ct).ConfigureAwait(false);
                    if (creditos.Count == 0)
                    {
                        EventoLog.Info("Créditos · " + nombreHumano + ": sin cambios.");
                        totalProcesadas++;
                        continue;
                    }

                    EventoLog.Info("Créditos · " + nombreHumano + ": enviando " + creditos.Count + " al portal...");
                    var r = await _api.SincronizarCreditosAsync(emp.emp_id_msp, creditos, ct).ConfigureAwait(false);
                    int nErrores = r.errors == null ? 0 : r.errors.Length;
                    EventoLog.Info("Créditos · " + nombreHumano + ": OK (nuevos=" + r.inserted
                        + ", actualizados=" + r.updated
                        + ", sin cambios=" + r.unchanged
                        + ", errores=" + nErrores + ")");
                    if (nErrores > 0)
                    {
                        int max = Math.Min(5, r.errors.Length);
                        for (int i = 0; i < max; i++)
                            EventoLog.Warning("Créditos · " + nombreHumano + " · error #" + (i+1)
                                + " (" + r.errors[i].DescribirItem() + "): " + r.errors[i].msg);
                        if (r.errors.Length > max)
                            EventoLog.Warning("Créditos · " + nombreHumano + " · ... y " + (r.errors.Length - max) + " errores más.");
                    }

                    totalProcesadas++;
                }
                catch (Exception ex)
                {
                    EventoLog.Error("Créditos · " + nombreHumano + ": " + ex.Message);
                    empresasConError++;
                }
            }

            var resumen = "Sincronizando créditos: terminó (" + totalProcesadas + " OK";
            if (empresasConError > 0) resumen += ", " + empresasConError + " con error";
            if (empresasOmitidas > 0) resumen += ", " + empresasOmitidas + " omitidas";
            resumen += " de " + empresas.Count + " autorizadas).";
            EventoLog.Info(resumen);

            return true;
        }

        /// <summary>
        /// Filtro híbrido: checkpoint del catálogo (más reciente) → EMP_SINC_DESDE
        /// (modal del Configurador) → null (fallback 90 días en repository).
        /// Suma 1 segundo para esquivar fracciones sub-segundo de Firebird.
        /// </summary>
        private static DateTime? CalcularDesde(EmpresaConfig emp)
        {
            if (emp.checkpoints != null && !string.IsNullOrWhiteSpace(emp.checkpoints.creditos))
            {
                DateTime dt;
                if (DateTime.TryParse(emp.checkpoints.creditos, out dt)) return dt.AddSeconds(1);
            }
            if (!string.IsNullOrWhiteSpace(emp.sinc_desde))
            {
                DateTime dt;
                if (DateTime.TryParse(emp.sinc_desde, out dt)) return dt.AddSeconds(1);
            }
            return null;
        }
    }
}
