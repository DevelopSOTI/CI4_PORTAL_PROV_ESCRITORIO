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
    /// Hito per-empresa: actualiza las NOTAS en el portal. Filtro Microsip:
    /// <c>cc.tipo='R'</c> en doctos_cp con <c>naturaleza_concepto='R'</c>
    /// (mismo filtro que el Delphi histórico en Func_Calcula.pas:562). El
    /// significado exacto de 'R' lo define la configuración del cliente en
    /// Microsip — el sincronizador replica el filtro tal cual sin asumir.
    ///
    /// Mismo shape que Créditos — solo cambia el tipo del concepto. Ambos
    /// se persisten en la misma tabla CREDITOS del portal (patrón heredado
    /// del SOAP legacy: Func_Notas.pas hace INSERT INTO CREDITOS).
    /// </summary>
    public sealed class SincronizadorNotas : ISincronizador
    {
        private readonly IResolutorEmpresaMicrosip _resolutor;
        private readonly ICacheEmpresasAutorizadas _cacheEmpresas;
        private readonly IDoctosCpRepository _repo;
        private readonly IPortalApi _api;

        public SincronizadorNotas(
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

        public string Nombre { get { return "Notas"; } }

        public async Task<bool> EjecutarAsync(CancellationToken ct)
        {
            var empresas = await _cacheEmpresas.ObtenerAsync(ct).ConfigureAwait(false);
            if (empresas.Count == 0)
            {
                EventoLog.Info("Sincronizando notas: no hay empresas autorizadas; nada que hacer.");
                return true;
            }

            int totalProcesadas  = 0;
            int empresasConError = 0;
            int empresasOmitidas = 0;

            foreach (var emp in empresas)
            {
                ct.ThrowIfCancellationRequested();

                // Incluimos emp_id_msp en el nombre para diagnóstico en multi-empresa:
                // si dos clientes tienen nombres parecidos o si la sincronización
                // falla, el operador necesita saber EXACTAMENTE qué empresa de
                // Microsip es (el emp_id_msp es el lookup directo a su .FDB).
                var nombreHumano = (!string.IsNullOrEmpty(emp.nombre_largo) ? emp.nombre_largo
                                  : !string.IsNullOrEmpty(emp.nombre)       ? emp.nombre
                                  : "(sin nombre)")
                                  + " [emp_id_msp=" + emp.emp_id_msp + "]";

                try
                {
                    var nombreCorto = await _resolutor.ObtenerNombreCortoAsync(emp.emp_id_msp, ct).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(nombreCorto))
                    {
                        EventoLog.Warning("Notas · " + nombreHumano + ": no se encontró en CONFIG.FDB; se omite.");
                        empresasOmitidas++;
                        continue;
                    }

                    var desde = CalcularDesde(emp);
                    EventoLog.Info("Notas · " + nombreHumano + ": leyendo Microsip"
                        + (desde.HasValue ? " (desde " + desde.Value.ToString("yyyy-MM-dd HH:mm") + ")" : " (últimos 90 días)")
                        + "...");

                    var notas = await _repo.ListarAsync(nombreCorto, "R", desde, ct).ConfigureAwait(false);
                    if (notas.Count == 0)
                    {
                        EventoLog.Info("Notas · " + nombreHumano + ": sin cambios.");
                        totalProcesadas++;
                        continue;
                    }

                    EventoLog.Info("Notas · " + nombreHumano + ": enviando " + notas.Count + " al portal...");
                    var r = await _api.SincronizarNotasAsync(emp.emp_id_msp, notas, ct).ConfigureAwait(false);
                    int nErrores = r.errors == null ? 0 : r.errors.Length;
                    EventoLog.Info("Notas · " + nombreHumano + ": OK (nuevas=" + r.inserted
                        + ", actualizadas=" + r.updated
                        + ", sin cambios=" + r.unchanged
                        + ", errores=" + nErrores + ")");
                    // Log detallado de los primeros errores para diagnosticar
                    // sin tener que ir al log del portal CI4.
                    if (nErrores > 0)
                    {
                        int max = Math.Min(5, r.errors.Length);
                        for (int i = 0; i < max; i++)
                            EventoLog.Warning("Notas · " + nombreHumano + " · error #" + (i+1)
                                + " (" + r.errors[i].DescribirItem() + "): " + r.errors[i].msg);
                        if (r.errors.Length > max)
                            EventoLog.Warning("Notas · " + nombreHumano + " · ... y " + (r.errors.Length - max) + " errores más.");
                    }

                    totalProcesadas++;
                }
                catch (Exception ex)
                {
                    EventoLog.Error("Notas · " + nombreHumano + ": " + ex.Message);
                    empresasConError++;
                }
            }

            var resumen = "Sincronizando notas: terminó (" + totalProcesadas + " OK";
            if (empresasConError > 0) resumen += ", " + empresasConError + " con error";
            if (empresasOmitidas > 0) resumen += ", " + empresasOmitidas + " omitidas";
            resumen += " de " + empresas.Count + " autorizadas).";
            EventoLog.Info(resumen);

            return true;
        }

        private static DateTime? CalcularDesde(EmpresaConfig emp)
        {
            if (emp.checkpoints != null && !string.IsNullOrWhiteSpace(emp.checkpoints.notas))
            {
                DateTime dt;
                if (DateTime.TryParse(emp.checkpoints.notas, out dt)) return dt.AddSeconds(1);
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
