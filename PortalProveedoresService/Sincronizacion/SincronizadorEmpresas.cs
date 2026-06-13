using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Servicios;
using PortalProveedoresService.Repositorios;

namespace PortalProveedoresService.Sincronizacion
{
    /// <summary>
    /// Hito 1 del ciclo: lee empresas de Microsip y las POSTea al portal.
    /// </summary>
    public sealed class SincronizadorEmpresas : ISincronizador
    {
        private readonly IEmpresasRepository _repo;
        private readonly IPortalApi _api;

        public SincronizadorEmpresas(IEmpresasRepository repo, IPortalApi api)
        {
            _repo = repo;
            _api  = api;
        }

        public string Nombre { get { return "Empresas"; } }

        public async Task<bool> EjecutarAsync(CancellationToken ct)
        {
            EventoLog.Info("Sincronizando empresas: leyendo Microsip...");
            var empresas = await _repo.ListarAsync(ct).ConfigureAwait(false);

            if (empresas.Count == 0)
            {
                EventoLog.Info("Sincronizando empresas: 0 registros, nada que enviar.");
                return true;
            }

            EventoLog.Info("Sincronizando empresas: enviando " + empresas.Count + " al portal...");
            try
            {
                var r = await _api.SincronizarEmpresasAsync(empresas, ct).ConfigureAwait(false);
                EventoLog.Info("Empresas sincronizadas: inserted=" + r.inserted + " updated=" + r.updated + " unchanged=" + r.unchanged + " errors=" + (r.errors == null ? 0 : r.errors.Length));
                return true;
            }
            catch (PortalApiException ex)
            {
                EventoLog.Error("Sincronizando empresas: " + ex.Message + " | body=" + ex.Cuerpo);
                return false;
            }
        }
    }
}
