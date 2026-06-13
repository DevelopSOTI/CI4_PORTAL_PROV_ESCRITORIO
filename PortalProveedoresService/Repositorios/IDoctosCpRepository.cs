using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Acceso a documentos de cuentas por pagar (<c>doctos_cp</c>) en
    /// Microsip. Sirve para CRÉDITOS y NOTAS — ambos comparten la misma
    /// estructura y solo difieren en el filtro <c>cc.tipo</c> (P=pago,
    /// R=devolución). Por eso el repository toma <paramref name="ccTipo"/>.
    /// </summary>
    public interface IDoctosCpRepository
    {
        /// <summary>
        /// Lista los documentos <c>cc.tipo = ccTipo</c> (junto a la convención
        /// <c>naturaleza_concepto = 'R'</c>) de la empresa con filtro
        /// incremental por <c>COALESCE(ULT_MODIF, CREACION) &gt; desde</c>.
        /// Si <paramref name="desde"/> es null, fallback a últimos 90 días.
        /// </summary>
        Task<IReadOnlyList<DoctoCpMicrosip>> ListarAsync(string nombreEmpresa, string ccTipo, DateTime? desde, CancellationToken ct);
    }
}
