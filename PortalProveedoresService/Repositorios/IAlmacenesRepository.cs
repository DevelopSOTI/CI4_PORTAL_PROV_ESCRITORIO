using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Acceso al catálogo de almacenes en Microsip (Firebird, BD por empresa).
    /// Una responsabilidad: leer la tabla <c>ALMACENES</c> de una empresa,
    /// con filtro incremental opcional por fecha.
    /// </summary>
    public interface IAlmacenesRepository
    {
        /// <summary>
        /// Lista los almacenes de la empresa cuyo Firebird vive en
        /// <c>MICRO_ROOT\{nombreEmpresa}.FDB</c>.
        ///
        /// <paramref name="desde"/> aplica el patrón Delphi histórico
        /// (Func_Calcula.pas:201-204): si viene una fecha, se filtra por
        /// <c>FECHA_HORA_CREACION &gt; desde OR FECHA_HORA_ULT_MODIF &gt; desde</c>
        /// para traerse solo los almacenes nuevos o modificados desde la
        /// última sincronización. Si es <c>null</c>, se traen todos.
        /// </summary>
        Task<IReadOnlyList<AlmacenMicrosip>> ListarAsync(string nombreEmpresa, DateTime? desde, CancellationToken ct);
    }
}
