using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Acceso a las recepciones de mercancía en Microsip (Firebird, BD por
    /// empresa). Lee cabecera + detalle de manera batch para evitar N+1.
    /// </summary>
    public interface IRecepcionesRepository
    {
        /// <summary>
        /// Lista las recepciones de la empresa cuyo Firebird vive en
        /// <c>MICRO_ROOT\{nombreEmpresa}.FDB</c>, con su detalle hidratado.
        ///
        /// <paramref name="desde"/> aplica el patrón Delphi histórico
        /// (Func_Calcula.pas:397-404): si viene una fecha, filtra por
        /// <c>COALESCE(FECHA_HORA_ULT_MODIF, FECHA_HORA_CREACION) &gt; desde</c>.
        /// Si es <c>null</c>, la carga inicial usa el fallback hardcodeado
        /// del Delphi (<c>fecha &gt; '01.03.2025'</c>) — el operador puede
        /// adelantarlo configurando <c>EMP_SINC_DESDE</c> en el modal.
        /// </summary>
        Task<IReadOnlyList<RecepcionMicrosip>> ListarAsync(string nombreEmpresa, DateTime? desde, CancellationToken ct);
    }
}
