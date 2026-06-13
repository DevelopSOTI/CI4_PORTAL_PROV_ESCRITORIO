using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Acceso al catálogo de empresas en Microsip (Firebird). Una sola
    /// responsabilidad: leer; nadie más conoce el SQL de Firebird.
    /// </summary>
    public interface IEmpresasRepository
    {
        Task<IReadOnlyList<EmpresaMicrosip>> ListarAsync(CancellationToken ct);
    }
}
