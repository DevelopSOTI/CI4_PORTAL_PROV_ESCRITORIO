using System;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Servicios;
using PortalProveedoresService.Repositorios;

namespace PortalProveedoresService.Sincronizacion
{
    /// <summary>
    /// Hito per-empresa: actualiza el catálogo de proveedores en el portal.
    /// Itera ÚNICAMENTE las empresas <c>Autorizada</c> en el portal y, por
    /// cada una, abre su Firebird local, hace el JOIN triple
    /// (proveedores + claves_proveedores + libres_proveedor) y POSTea al
    /// portal los proveedores nuevos/modificados.
    ///
    /// Filtro incremental por catálogo: cada empresa trae en
    /// <c>EmpresaConfig.checkpoints.proveedores</c> la última
    /// <c>FECHA_ULT_MODIF</c> (nota: en PROVEEDORES_MSP la columna es
    /// FECHA_ULT_MODIF, no FECHA_HORA_ULT_MODIF como almacenes/monedas) que
    /// el portal ya tiene. Si es <c>null</c>, es la primera vez para esa
    /// empresa → carga completa.
    ///
    /// Mensajes hacia el visor: TODOS por nombre de empresa, NUNCA por ID
    /// (regla del proyecto, ver CLAUDE.md).
    /// </summary>
    public sealed class SincronizadorProveedores : ISincronizador
    {
        private readonly IResolutorEmpresaMicrosip _resolutor;
        private readonly ICacheEmpresasAutorizadas _cacheEmpresas;
        private readonly IProveedoresRepository _repo;
        private readonly IPortalApi _api;

        public SincronizadorProveedores(
            IResolutorEmpresaMicrosip resolutor,
            ICacheEmpresasAutorizadas cacheEmpresas,
            IProveedoresRepository repo,
            IPortalApi api)
        {
            _resolutor     = resolutor;
            _cacheEmpresas = cacheEmpresas;
            _repo          = repo;
            _api           = api;
        }

        public string Nombre { get { return "Proveedores"; } }

        public async Task<bool> EjecutarAsync(CancellationToken ct)
        {
            var empresas = await _cacheEmpresas.ObtenerAsync(ct).ConfigureAwait(false);
            if (empresas.Count == 0)
            {
                EventoLog.Info("Sincronizando proveedores: no hay empresas autorizadas; nada que hacer.");
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
                        EventoLog.Warning("Proveedores · " + nombreHumano +
                            ": no se encontró en CONFIG.FDB (¿borrada en Microsip?); se omite.");
                        empresasOmitidas++;
                        continue;
                    }

                    var desde = ParsearCheckpoint(emp.checkpoints != null ? emp.checkpoints.proveedores : null);

                    EventoLog.Info("Proveedores · " + nombreHumano + ": leyendo Microsip"
                        + (desde.HasValue ? " (desde " + desde.Value.ToString("yyyy-MM-dd HH:mm") + ")" : " (sincronización inicial)")
                        + "...");

                    var proveedores = await _repo.ListarAsync(nombreCorto, desde, ct).ConfigureAwait(false);
                    if (proveedores.Count == 0)
                    {
                        EventoLog.Info("Proveedores · " + nombreHumano + ": sin cambios.");
                        // La empresa SÍ se procesó (verificamos que no había nada
                        // nuevo); cuenta como OK para el resumen final.
                        totalProcesadas++;
                        continue;
                    }

                    EventoLog.Info("Proveedores · " + nombreHumano + ": enviando " + proveedores.Count + " al portal...");

                    var r = await _api.SincronizarProveedoresAsync(emp.emp_id_msp, proveedores, ct).ConfigureAwait(false);

                    EventoLog.Info("Proveedores · " + nombreHumano + ": OK (nuevos=" + r.inserted
                        + ", actualizados=" + r.updated
                        + ", sin cambios=" + r.unchanged
                        + ", errores=" + (r.errors == null ? 0 : r.errors.Length) + ")");

                    totalProcesadas++;
                }
                catch (Exception ex)
                {
                    EventoLog.Error("Proveedores · " + nombreHumano + ": " + ex.Message);
                    empresasConError++;
                }
            }

            var resumen = "Sincronizando proveedores: terminó (" + totalProcesadas + " OK";
            if (empresasConError > 0) resumen += ", " + empresasConError + " con error";
            if (empresasOmitidas > 0) resumen += ", " + empresasOmitidas + " omitidas";
            resumen += " de " + empresas.Count + " autorizadas).";
            EventoLog.Info(resumen);

            return true;
        }

        /// <summary>
        /// Convierte el checkpoint ISO del portal en un <see cref="DateTime"/>
        /// listo para el filtro Firebird. <c>null</c>/vacío/inválido → <c>null</c>
        /// (sincronización inicial, traer todo).
        /// </summary>
        private static DateTime? ParsearCheckpoint(string checkpointIso)
        {
            if (string.IsNullOrWhiteSpace(checkpointIso)) return null;
            DateTime dt;
            if (!DateTime.TryParse(checkpointIso, out dt)) return null;
            // +1 seg: esquivar fracciones sub-segundo de Firebird vs. MySQL
            // truncado. Ver SincronizadorAlmacenes.ParsearCheckpoint para el
            // razonamiento completo del bug y por qué 1 segundo basta.
            return dt.AddSeconds(1);
        }
    }
}
