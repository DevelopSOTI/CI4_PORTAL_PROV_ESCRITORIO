using System;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Servicios;
using PortalProveedoresService.Repositorios;

namespace PortalProveedoresService.Sincronizacion
{
    /// <summary>
    /// Hito per-empresa: actualiza el catálogo de almacenes en el portal.
    /// Itera ÚNICAMENTE las empresas <c>Autorizada</c> en el portal y, por
    /// cada una, abre su Firebird local y POSTea los almacenes nuevos/modificados.
    ///
    /// Filtro incremental por catálogo: cada empresa trae en
    /// <c>EmpresaConfig.checkpoints.almacenes</c> la última
    /// <c>FECHA_HORA_ULT_MODIF</c> que el portal ya tiene de ESTE catálogo
    /// para esta empresa (high-water-mark). Si es <c>null</c>, es la primera
    /// vez que se sincroniza almacenes para esa empresa → carga completa
    /// (sin filtro de fecha). Esto es independiente de <c>EMP_ULT_SINC</c>,
    /// que es el sello del ciclo COMPLETO (puede estar poblado por otro
    /// catálogo aunque éste sea su primera vez).
    ///
    /// Mensajes hacia el visor: TODOS por nombre de empresa, NUNCA por ID
    /// (regla del proyecto, ver CLAUDE.md).
    /// </summary>
    public sealed class SincronizadorAlmacenes : ISincronizador
    {
        private readonly IResolutorEmpresaMicrosip _resolutor;
        private readonly ICacheEmpresasAutorizadas _cacheEmpresas;
        private readonly IAlmacenesRepository _repo;
        private readonly IPortalApi _api;

        public SincronizadorAlmacenes(
            IResolutorEmpresaMicrosip resolutor,
            ICacheEmpresasAutorizadas cacheEmpresas,
            IAlmacenesRepository repo,
            IPortalApi api)
        {
            _resolutor     = resolutor;
            _cacheEmpresas = cacheEmpresas;
            _repo          = repo;
            _api           = api;
        }

        public string Nombre { get { return "Almacenes"; } }

        public async Task<bool> EjecutarAsync(CancellationToken ct)
        {
            // Lista compartida del ciclo — el cache hace la GET solo la primera
            // vez que algún sincronizador la pide; las llamadas posteriores
            // (Monedas, Proveedores, ...) reusan la lista de memoria.
            var empresas = await _cacheEmpresas.ObtenerAsync(ct).ConfigureAwait(false);
            if (empresas.Count == 0)
            {
                EventoLog.Info("Sincronizando almacenes: no hay empresas autorizadas; nada que hacer.");
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
                // resuelto desde CONFIG.FDB (fuente de verdad, ver más abajo).
                // Incluimos emp_id_msp para diagnóstico en multi-empresa: con eso
                // el operador identifica EXACTAMENTE qué cliente y qué .FDB de
                // Microsip es el que falló, aunque hayan dos empresas con
                // nombres parecidos.
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
                        EventoLog.Warning("Almacenes · " + nombreHumano +
                            ": no se encontró en CONFIG.FDB (¿borrada en Microsip?); se omite.");
                        empresasOmitidas++;
                        continue;
                    }

                    var desde = ParsearCheckpoint(emp.checkpoints != null ? emp.checkpoints.almacenes : null);

                    EventoLog.Info("Almacenes · " + nombreHumano + ": leyendo Microsip"
                        + (desde.HasValue ? " (desde " + desde.Value.ToString("yyyy-MM-dd HH:mm") + ")" : " (sincronización inicial)")
                        + "...");

                    var almacenes = await _repo.ListarAsync(nombreCorto, desde, ct).ConfigureAwait(false);
                    if (almacenes.Count == 0)
                    {
                        EventoLog.Info("Almacenes · " + nombreHumano + ": sin cambios.");
                        totalProcesadas++;   // se procesó OK aunque no había nada que enviar
                        continue;
                    }

                    EventoLog.Info("Almacenes · " + nombreHumano + ": enviando " + almacenes.Count + " al portal...");

                    var r = await _api.SincronizarAlmacenesAsync(emp.emp_id_msp, almacenes, ct).ConfigureAwait(false);

                    EventoLog.Info("Almacenes · " + nombreHumano + ": OK (nuevos=" + r.inserted
                        + ", actualizados=" + r.updated
                        + ", sin cambios=" + r.unchanged
                        + ", errores=" + (r.errors == null ? 0 : r.errors.Length) + ")");

                    totalProcesadas++;
                }
                catch (Exception ex)
                {
                    EventoLog.Error("Almacenes · " + nombreHumano + ": " + ex.Message);
                    empresasConError++;
                }
            }

            // Resumen condicional: solo menciona omitidas/errores si las hubo,
            // así el caso "todo limpio" se lee corto y claro.
            var resumen = "Sincronizando almacenes: terminó (" + totalProcesadas + " OK";
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
        /// (sincronización inicial, traer todo).
        ///
        /// Sumamos 1 segundo para esquivar el problema de precisión:
        /// Firebird tiene precisión sub-segundo en FECHA_HORA_ULT_MODIF, pero
        /// MySQL guarda solo hasta segundo. El portal devuelve <c>MAX(FECHA)</c>
        /// truncado. Si filtramos <c>&gt; MAX exacto</c>, Firebird devuelve filas
        /// con fracciones dentro del mismo segundo (X.fff &gt; X.000); el servicio
        /// las envía con la fecha truncada; el portal compara y dice "unchanged";
        /// el MAX nunca avanza → loop infinito de 1 fila por ciclo.
        ///
        /// El Delphi histórico evitaba esto restando 1 día como buffer paranoico
        /// (ver Func_Calcula.pas:173: <c>IncDay(EMP_ULT_SINC, -1)</c>). Aquí
        /// 1 segundo basta: brinca la ventana del segundo MAX donde están las
        /// fracciones perdidas, sin traer 24h de ruido cada ciclo.
        ///
        /// Edge case aceptado: una fila nueva insertada en Microsip
        /// EXACTAMENTE en el mismo segundo del MAX se perdería. Probabilidad
        /// muy baja (requiere que el ciclo termine y la fila se cree en el
        /// mismo segundo) y se recupera al ciclo siguiente si esa fila vuelve
        /// a tocarse.
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
