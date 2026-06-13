using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Servicios;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Implementación del cache de empresas autorizadas con lazy load.
    /// Thread-safety: los sincronizadores corren en serie dentro del ciclo
    /// (ver Service1.EjecutarCicloAsync), así que NO hay carrera. Si en el
    /// futuro un paso decide leer en paralelo, hay que proteger el lazy
    /// con SemaphoreSlim — por ahora sería overhead innecesario.
    /// </summary>
    public sealed class CacheEmpresasAutorizadas : ICacheEmpresasAutorizadas
    {
        private readonly IPortalApi _api;
        private IReadOnlyList<EmpresaConfig> _cache;

        public CacheEmpresasAutorizadas(IPortalApi api)
        {
            _api = api;
        }

        public async Task<IReadOnlyList<EmpresaConfig>> ObtenerAsync(CancellationToken ct)
        {
            if (_cache != null) return _cache;

            EventoLog.Info("Consultando empresas autorizadas al portal...");
            var lista = await _api.ListarEmpresasAutorizadasAsync(ct).ConfigureAwait(false);
            EventoLog.Info("Empresas autorizadas: " + lista.Count + ".");
            _cache = lista;
            return _cache;
        }
    }
}
