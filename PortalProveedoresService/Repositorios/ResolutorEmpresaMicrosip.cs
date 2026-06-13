using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Logging;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Lee CONFIG.FDB una sola vez por ciclo y cachea el mapeo
    /// <c>EMPRESA_ID → NOMBRE_CORTO</c>. Cada sincronizador per-empresa
    /// consulta este caché en lugar de confiar en el snapshot de EMP_NOMBRE
    /// que vive en MySQL (que puede estar viejo si renombraron la empresa
    /// en Microsip después del último sync).
    ///
    /// El caché se invalida con <see cref="Invalidar"/> — Service1 lo llama
    /// al inicio de cada ciclo para que cambios en CONFIG.FDB se reflejen
    /// en la siguiente iteración sin reiniciar el servicio.
    ///
    /// Thread-safety: las lecturas usan double-checked locking; la carga es
    /// atómica respecto a otras lecturas (puede ejecutarse 2 veces en una
    /// race pero el resultado es idéntico).
    /// </summary>
    public sealed class ResolutorEmpresaMicrosip : IResolutorEmpresaMicrosip
    {
        private const string Sql =
            "SELECT EMPRESA_ID, NOMBRE_CORTO FROM EMPRESAS ORDER BY EMPRESA_ID";

        private readonly object _candado = new object();
        private Dictionary<int, string> _cache; // null = aún no cargado

        public async Task<string> ObtenerNombreCortoAsync(int empresaId, CancellationToken ct)
        {
            Dictionary<int, string> cache;
            lock (_candado) { cache = _cache; }

            if (cache == null)
            {
                cache = await CargarAsync(ct).ConfigureAwait(false);
                lock (_candado) { _cache = cache; }
            }

            string nombre;
            return cache.TryGetValue(empresaId, out nombre) ? nombre : null;
        }

        public void Invalidar()
        {
            lock (_candado) { _cache = null; }
        }

        private static async Task<Dictionary<int, string>> CargarAsync(CancellationToken ct)
        {
            var dict = new Dictionary<int, string>();

            var con = new ConexionMicrosip();
            if (!con.ConectarConfigMicrosip())
            {
                EventoLog.Warning("ResolutorEmpresa: no se pudo abrir CONFIG.FDB.");
                return dict;
            }

            try
            {
                using (var cmd = new FbCommand(Sql, con.FBC))
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var id = Convert.ToInt32(rd["EMPRESA_ID"]);
                        var nc = Convert.ToString(rd["NOMBRE_CORTO"]) ?? "";
                        dict[id] = nc;
                    }
                }
            }
            catch (Exception ex)
            {
                EventoLog.Error("ResolutorEmpresa.CargarAsync: " + ex.Message);
            }
            finally
            {
                con.Desconectar();
            }

            return dict;
        }
    }
}
