using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Acceso al catálogo de proveedores en Microsip (Firebird, BD por empresa).
    /// JOIN triple: proveedores + claves_proveedores + libres_proveedor — el
    /// proveedor "completo" para el portal vive en estas tres tablas.
    /// </summary>
    public interface IProveedoresRepository
    {
        /// <summary>
        /// Lista los proveedores de la empresa cuyo Firebird vive en
        /// <c>MICRO_ROOT\{nombreEmpresa}.FDB</c>.
        ///
        /// <paramref name="desde"/> aplica el patrón Delphi histórico
        /// (Func_Calcula.pas:288-313) con dos correcciones documentadas en
        /// <see cref="ProveedoresRepository"/>: si viene fecha, filtra por
        /// <c>p.FECHA_HORA_CREACION &gt; desde OR p.FECHA_HORA_ULT_MODIF &gt; desde</c>.
        /// Si es <c>null</c>, se traen todos (carga inicial).
        /// </summary>
        Task<IReadOnlyList<ProveedorMicrosip>> ListarAsync(string nombreEmpresa, DateTime? desde, CancellationToken ct);
    }
}
