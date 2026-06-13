using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Acceso al catálogo de monedas en Microsip (Firebird, BD por empresa).
    /// Una responsabilidad: leer la tabla <c>MONEDAS</c> de una empresa,
    /// con filtro incremental opcional por fecha.
    /// </summary>
    public interface IMonedasRepository
    {
        /// <summary>
        /// Lista las monedas de la empresa cuyo Firebird vive en
        /// <c>MICRO_ROOT\{nombreEmpresa}.FDB</c>.
        ///
        /// <paramref name="desde"/> aplica el patrón Delphi histórico
        /// (Func_Calcula.pas:245-251): si viene una fecha, se filtra por
        /// <c>FECHA_HORA_CREACION &gt; desde OR FECHA_HORA_ULT_MODIF &gt; desde</c>
        /// para traerse solo las monedas nuevas o modificadas desde la
        /// última sincronización. Si es <c>null</c>, se traen todas
        /// (carga inicial).
        /// </summary>
        Task<IReadOnlyList<MonedaMicrosip>> ListarAsync(string nombreEmpresa, DateTime? desde, CancellationToken ct);
    }
}
