using System;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Servicios;
using PortalProveedoresService.Repositorios;

namespace PortalProveedoresService.Sincronizacion
{
    /// <summary>
    /// Hito per-empresa: actualiza el catálogo de monedas en el portal.
    /// Itera ÚNICAMENTE las empresas <c>Autorizada</c> en el portal y, por
    /// cada una, abre su Firebird local y POSTea las monedas nuevas/modificadas.
    ///
    /// Filtro incremental por catálogo: cada empresa trae en
    /// <c>EmpresaConfig.checkpoints.monedas</c> la última
    /// <c>FECHA_HORA_ULT_MODIF</c> que el portal ya tiene de ESTE catálogo
    /// para esta empresa (high-water-mark). Si es <c>null</c>, es la primera
    /// vez que se sincroniza monedas para esa empresa → carga completa
    /// (sin filtro de fecha), aunque Almacenes ya haya corrido antes y haya
    /// poblado <c>EMP_ULT_SINC</c> — los catálogos son independientes.
    ///
    /// Mensajes hacia el visor: TODOS por nombre de empresa, NUNCA por ID
    /// (regla del proyecto, ver CLAUDE.md).
    /// </summary>
    public sealed class SincronizadorMonedas : ISincronizador
    {
        private readonly IResolutorEmpresaMicrosip _resolutor;
        private readonly ICacheEmpresasAutorizadas _cacheEmpresas;
        private readonly IMonedasRepository _repo;
        private readonly IPortalApi _api;

        public SincronizadorMonedas(
            IResolutorEmpresaMicrosip resolutor,
            ICacheEmpresasAutorizadas cacheEmpresas,
            IMonedasRepository repo,
            IPortalApi api)
        {
            _resolutor     = resolutor;
            _cacheEmpresas = cacheEmpresas;
            _repo          = repo;
            _api           = api;
        }

        public string Nombre { get { return "Monedas"; } }

        public async Task<bool> EjecutarAsync(CancellationToken ct)
        {
            // Lista compartida del ciclo — la primera vez que algún paso la
            // pide se hace la GET al portal; aquí ya viene de memoria si
            // Almacenes corrió antes (que es lo normal por orden de pasos).
            var empresas = await _cacheEmpresas.ObtenerAsync(ct).ConfigureAwait(false);
            if (empresas.Count == 0)
            {
                EventoLog.Info("Sincronizando monedas: no hay empresas autorizadas; nada que hacer.");
                return true;
            }

            int totalProcesadas  = 0;
            int empresasConError = 0;
            int empresasOmitidas = 0;

            foreach (var emp in empresas)
            {
                ct.ThrowIfCancellationRequested();

                // Nombre humano para los logs — usamos el largo de MySQL para
                // mostrar al operador. Para abrir el .FDB usamos el NOMBRE_CORTO
                // resuelto desde CONFIG.FDB (fuente de verdad).
                // Incluimos emp_id_msp para diagnóstico en multi-empresa.
                var nombreHumano = (!string.IsNullOrEmpty(emp.nombre_largo) ? emp.nombre_largo
                                  : !string.IsNullOrEmpty(emp.nombre)       ? emp.nombre
                                  : "(sin nombre)")
                                  + " [emp_id_msp=" + emp.emp_id_msp + "]";

                try
                {
                    // SIEMPRE resolver el NOMBRE_CORTO actual desde CONFIG.FDB
                    // por EMPRESA_ID. Si el usuario renombró la empresa en
                    // Microsip después del último sync de empresas, MySQL tiene
                    // el nombre viejo; CONFIG.FDB tiene el actual.
                    var nombreCorto = await _resolutor.ObtenerNombreCortoAsync(emp.emp_id_msp, ct).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(nombreCorto))
                    {
                        EventoLog.Warning("Monedas · " + nombreHumano +
                            ": no se encontró en CONFIG.FDB (¿borrada en Microsip?); se omite.");
                        empresasOmitidas++;
                        continue;
                    }

                    var desde = ParsearCheckpoint(emp.checkpoints != null ? emp.checkpoints.monedas : null);

                    EventoLog.Info("Monedas · " + nombreHumano + ": leyendo Microsip"
                        + (desde.HasValue ? " (desde " + desde.Value.ToString("yyyy-MM-dd HH:mm") + ")" : " (sincronización inicial)")
                        + "...");

                    var monedas = await _repo.ListarAsync(nombreCorto, desde, ct).ConfigureAwait(false);
                    if (monedas.Count == 0)
                    {
                        EventoLog.Info("Monedas · " + nombreHumano + ": sin cambios.");
                        totalProcesadas++;   // se procesó OK aunque no había nada que enviar
                        continue;
                    }

                    EventoLog.Info("Monedas · " + nombreHumano + ": enviando " + monedas.Count + " al portal...");

                    var r = await _api.SincronizarMonedasAsync(emp.emp_id_msp, monedas, ct).ConfigureAwait(false);

                    EventoLog.Info("Monedas · " + nombreHumano + ": OK (nuevas=" + r.inserted
                        + ", actualizadas=" + r.updated
                        + ", sin cambios=" + r.unchanged
                        + ", errores=" + (r.errors == null ? 0 : r.errors.Length) + ")");

                    totalProcesadas++;
                }
                catch (Exception ex)
                {
                    EventoLog.Error("Monedas · " + nombreHumano + ": " + ex.Message);
                    empresasConError++;
                }
            }

            // Resumen condicional: solo menciona omitidas/errores si las hubo,
            // así el caso "todo limpio" se lee corto y claro.
            var resumen = "Sincronizando monedas: terminó (" + totalProcesadas + " OK";
            if (empresasConError > 0) resumen += ", " + empresasConError + " con error";
            if (empresasOmitidas > 0) resumen += ", " + empresasOmitidas + " omitidas";
            resumen += " de " + empresas.Count + " autorizadas).";
            EventoLog.Info(resumen);

            // Devuelve true mientras al menos una empresa se haya procesado
            // sin error fatal. Errores aislados por empresa no abortan el ciclo
            // global — eso lo decide el orquestador (Service1).
            return true;
        }

        /// <summary>
        /// Convierte el checkpoint ISO del portal en un <see cref="DateTime"/>
        /// listo para el filtro Firebird. <c>null</c>/vacío/inválido → <c>null</c>
        /// (sincronización inicial). Suma 1 segundo para esquivar el bug de
        /// fracciones sub-segundo de Firebird vs. MySQL truncado — ver doc en
        /// SincronizadorAlmacenes.ParsearCheckpoint.
        /// </summary>
        private static DateTime? ParsearCheckpoint(string checkpointIso)
        {
            if (string.IsNullOrWhiteSpace(checkpointIso)) return null;
            DateTime dt;
            if (!DateTime.TryParse(checkpointIso, out dt)) return null;
            return dt.AddSeconds(1);
        }
    }
}
