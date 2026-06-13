using System.Threading;
using System.Threading.Tasks;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Resuelve el NOMBRE_CORTO actual de una empresa Microsip a partir de su
    /// EMPRESA_ID (= EMP_ID_MSP en el portal). La fuente de verdad es siempre
    /// la tabla EMPRESAS del archivo Microsip CONFIG.FDB, NO el snapshot
    /// almacenado en MySQL como EMP_NOMBRE — el usuario puede haber renombrado
    /// la empresa en Microsip después del último sync de empresas.
    ///
    /// Implementación esperada: caché interno cargado una sola vez por ciclo,
    /// con <see cref="Invalidar"/> al inicio del próximo. Así abrimos CONFIG.FDB
    /// una vez y no N (empresas) × M (sincronizadores) por ciclo.
    /// </summary>
    public interface IResolutorEmpresaMicrosip
    {
        /// <summary>
        /// Devuelve el NOMBRE_CORTO de la empresa, o <c>null</c> si no aparece
        /// en CONFIG.FDB (empresa borrada en Microsip pero aún viva en MySQL).
        /// </summary>
        Task<string> ObtenerNombreCortoAsync(int empresaId, CancellationToken ct);

        /// <summary>
        /// Marca el caché como obsoleto para que la próxima llamada relea
        /// CONFIG.FDB. Service1 lo invoca al inicio de cada ciclo, así un
        /// rename en Microsip se respeta en el siguiente ciclo.
        /// </summary>
        void Invalidar();
    }
}
