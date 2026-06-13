using System.Threading;
using System.Threading.Tasks;

namespace PortalProveedoresService.Sincronizacion
{
    /// <summary>
    /// Una unidad de sincronización (empresas, almacenes, monedas, ...). El
    /// servicio invoca cada sincronizador en orden; añadir un hito futuro =
    /// agregar una nueva implementación y registrarla en Service1.
    /// </summary>
    public interface ISincronizador
    {
        /// <summary>Nombre legible para logs.</summary>
        string Nombre { get; }

        /// <summary>
        /// Ejecuta la sincronización. Devuelve true si terminó sin bloqueos
        /// críticos. Si devuelve false, el servicio aborta el resto del ciclo
        /// (mismo contrato que el Func.INSERT_UPDATE legacy: si EMPRESAS falla
        /// no se sincroniza nada más).
        /// </summary>
        Task<bool> EjecutarAsync(CancellationToken ct);
    }
}
