using System;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Servicios;
using PortalProveedoresService.Repositorios;

namespace PortalProveedoresService.Sincronizacion
{
    /// <summary>
    /// Hito per-empresa: actualiza las recepciones de mercancía en el portal.
    /// Primer sincronizador de DOCUMENTOS (no catálogo), con cabecera + detalle.
    ///
    /// Filtro temporal (HÍBRIDO):
    ///  - Si la empresa ya tiene checkpoint del catálogo (high-water-mark del
    ///    portal): usar ese. Es siempre el más reciente y nos da incremental.
    ///  - Si no hay checkpoint (primera vez): usar <c>EMP_SINC_DESDE</c>
    ///    (lo que el operador configura en el modal del Configurador).
    ///  - Si tampoco hay sinc_desde: el repository hace fallback a "últimos
    ///    90 días" para no descargar años de historia en la carga inicial.
    ///
    /// Sumamos 1 segundo al desde (igual patrón que catálogos) para esquivar
    /// el bug de fracciones sub-segundo de Firebird vs. MySQL truncado.
    ///
    /// Mensajes hacia el visor: TODOS por nombre de empresa, NUNCA por ID.
    /// </summary>
    public sealed class SincronizadorRecepciones : ISincronizador
    {
        private readonly IResolutorEmpresaMicrosip _resolutor;
        private readonly ICacheEmpresasAutorizadas _cacheEmpresas;
        private readonly IRecepcionesRepository _repo;
        private readonly IPortalApi _api;

        public SincronizadorRecepciones(
            IResolutorEmpresaMicrosip resolutor,
            ICacheEmpresasAutorizadas cacheEmpresas,
            IRecepcionesRepository repo,
            IPortalApi api)
        {
            _resolutor     = resolutor;
            _cacheEmpresas = cacheEmpresas;
            _repo          = repo;
            _api           = api;
        }

        public string Nombre { get { return "Recepciones"; } }

        public async Task<bool> EjecutarAsync(CancellationToken ct)
        {
            var empresas = await _cacheEmpresas.ObtenerAsync(ct).ConfigureAwait(false);
            if (empresas.Count == 0)
            {
                EventoLog.Info("Sincronizando recepciones: no hay empresas autorizadas; nada que hacer.");
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
                        EventoLog.Warning("Recepciones · " + nombreHumano +
                            ": no se encontró en CONFIG.FDB (¿borrada en Microsip?); se omite.");
                        empresasOmitidas++;
                        continue;
                    }

                    var desde = CalcularDesde(emp);

                    EventoLog.Info("Recepciones · " + nombreHumano + ": leyendo Microsip"
                        + (desde.HasValue ? " (desde " + desde.Value.ToString("yyyy-MM-dd HH:mm") + ")" : " (últimos 90 días)")
                        + "...");

                    var recepciones = await _repo.ListarAsync(nombreCorto, desde, ct).ConfigureAwait(false);
                    if (recepciones.Count == 0)
                    {
                        EventoLog.Info("Recepciones · " + nombreHumano + ": sin cambios.");
                        totalProcesadas++;
                        continue;
                    }

                    EventoLog.Info("Recepciones · " + nombreHumano + ": enviando " + recepciones.Count + " al portal...");

                    var r = await _api.SincronizarRecepcionesAsync(emp.emp_id_msp, recepciones, ct).ConfigureAwait(false);

                    int nErrores = r.errors == null ? 0 : r.errors.Length;
                    EventoLog.Info("Recepciones · " + nombreHumano + ": OK (nuevas=" + r.inserted
                        + ", actualizadas=" + r.updated
                        + ", sin cambios=" + r.unchanged
                        + ", errores=" + nErrores + ")");
                    if (nErrores > 0)
                    {
                        int max = Math.Min(5, r.errors.Length);
                        for (int i = 0; i < max; i++)
                            EventoLog.Warning("Recepciones · " + nombreHumano + " · error #" + (i+1)
                                + " (" + r.errors[i].DescribirItem() + "): " + r.errors[i].msg);
                        if (r.errors.Length > max)
                            EventoLog.Warning("Recepciones · " + nombreHumano + " · ... y " + (r.errors.Length - max) + " errores más.");
                    }

                    totalProcesadas++;
                }
                catch (Exception ex)
                {
                    EventoLog.Error("Recepciones · " + nombreHumano + ": " + ex.Message);
                    empresasConError++;
                }
            }

            var resumen = "Sincronizando recepciones: terminó (" + totalProcesadas + " OK";
            if (empresasConError > 0) resumen += ", " + empresasConError + " con error";
            if (empresasOmitidas > 0) resumen += ", " + empresasOmitidas + " omitidas";
            resumen += " de " + empresas.Count + " autorizadas).";
            EventoLog.Info(resumen);

            return true;
        }

        /// <summary>
        /// Resuelve la fecha "desde" para el filtro Firebird priorizando:
        ///   1) Checkpoint del portal (más reciente, da incremental).
        ///   2) EMP_SINC_DESDE (lo que el operador configuró en el modal).
        ///   3) null → el repository hace fallback a "últimos 90 días".
        ///
        /// Si se elige checkpoint o sinc_desde, sumamos 1 segundo para
        /// esquivar el bug de fracciones sub-segundo de Firebird (ver
        /// SincronizadorAlmacenes.ParsearCheckpoint).
        /// </summary>
        private static DateTime? CalcularDesde(PortalProveedoresCore.Modelos.EmpresaConfig emp)
        {
            // 1) Checkpoint específico de recepciones (el más reciente).
            if (emp.checkpoints != null && !string.IsNullOrWhiteSpace(emp.checkpoints.recepciones))
            {
                DateTime dt;
                if (DateTime.TryParse(emp.checkpoints.recepciones, out dt))
                    return dt.AddSeconds(1);
            }

            // 2) EMP_SINC_DESDE (modal del Configurador).
            if (!string.IsNullOrWhiteSpace(emp.sinc_desde))
            {
                DateTime dt;
                if (DateTime.TryParse(emp.sinc_desde, out dt))
                    return dt.AddSeconds(1);
            }

            // 3) null → fallback en el repository.
            return null;
        }
    }
}
