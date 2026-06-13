using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Vista cacheada de la lista de empresas autorizadas del portal,
    /// con tiempo de vida de UN ciclo de sincronización.
    ///
    /// Por qué existe: cada sincronizador per-empresa (Almacenes, Monedas,
    /// Proveedores, Recepciones, ...) necesita la misma lista para iterar.
    /// Sin este cache, cada paso haría su propia GET /api/empresas?solo_autorizadas=1
    /// — N+1 requests HTTP por ciclo, todas devolviendo lo mismo. Con el cache
    /// se hace UNA sola request por ciclo y se reusa.
    ///
    /// Lifetime: una instancia nueva por <c>EjecutarCicloAsync</c>. NO se invalida
    /// dentro del mismo ciclo (sería absurdo: si el operador autoriza una empresa
    /// nueva mientras corre Almacenes, no queremos que Monedas la incluya y
    /// Proveedores no — los pasos deben verla igual). El siguiente ciclo crea
    /// otro cache y vuelve a consultar al portal.
    /// </summary>
    public interface ICacheEmpresasAutorizadas
    {
        /// <summary>
        /// Devuelve la lista de empresas autorizadas. Primera llamada → HTTP
        /// al portal; llamadas subsiguientes → memoria.
        /// </summary>
        Task<IReadOnlyList<EmpresaConfig>> ObtenerAsync(CancellationToken ct);
    }
}
