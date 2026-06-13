using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Configuracion;

namespace PortalProveedoresEscritorio.Servicios
{
    /// <summary>
    /// Consulta NUM_POLIZA / FECHA_POLIZA directamente desde Firebird de la
    /// empresa para una lista de FOLIO_COMPRA + PROVEEDOR_ID. Réplica
    /// LITERAL del SQL del SOAP F_DESCARGAR.cs:196-216 — el SOAP itera fila
    /// por fila y hace la misma query parametrizada por (FOLIO, PROVEEDOR).
    /// Aquí emulamos ese loop con una sola conexión (un solo
    /// <see cref="ConexionMicrosip.ConectarMicrosip"/>) que prepara la query
    /// y la reusa N veces — el coste real es la red, no el roundtrip de SQL.
    ///
    /// Por qué no se hace IN(...): el SQL del SOAP filtra TANTO por FOLIO
    /// COMO por PROVEEDOR_ID. Un único <c>FOLIO IN (...)</c> mezcla
    /// proveedores y devolvería pólizas equivocadas. El loop por fila es
    /// estructuralmente más simple y replica el SOAP exactamente.
    /// </summary>
    public sealed class PolizasMicrosip
    {
        /// <summary>
        /// Tuple-like simple — evitamos ValueTuple para que el build cumpla
        /// con el toolchain ya en uso (no requerir referencia a System.ValueTuple).
        /// </summary>
        public sealed class DatoPoliza
        {
            public string NumPoliza   { get; set; }
            public string FechaPoliza { get; set; } // dd/MM/yyyy
        }

        /// <summary>
        /// Devuelve un diccionario con clave "FOLIO|PROVEEDOR_ID" → poliza.
        /// Las filas sin póliza encontrada simplemente no aparecen en el
        /// diccionario (el caller debe pintar "" en esos casos). Si la
        /// conexión a Firebird falla, devuelve diccionario vacío sin lanzar.
        /// </summary>
        public async Task<Dictionary<string, DatoPoliza>> ObtenerPolizasAsync(
            string nombreEmpresa,
            IEnumerable<KeyValuePair<string, int>> folioYProveedor,
            CancellationToken ct)
        {
            var resultado = new Dictionary<string, DatoPoliza>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(nombreEmpresa) || folioYProveedor == null)
                return resultado;

            try
            {
                var con = new ConexionMicrosip();
                if (!con.ConectarMicrosip(nombreEmpresa)) return resultado;
                try
                {
                    // Réplica LITERAL del SOAP F_DESCARGAR.cs:196-216.
                    const string sql = @"
SELECT
    fac.FOLIO AS FACTURA,
    dco.POLIZA AS NUM_POLIZA,
    dco.FECHA AS FECHA_POLIZA
FROM DOCTOS_CP fac
INNER JOIN IMPORTES_DOCTOS_CP imp
    ON imp.DOCTO_CP_ACR_ID = fac.DOCTO_CP_ID
   AND imp.TIPO_IMPTE = 'R'
INNER JOIN DOCTOS_CP pag
    ON pag.DOCTO_CP_ID = imp.DOCTO_CP_ID
INNER JOIN DOCTOS_ENTRE_SIS des1
    ON des1.DOCTO_FTE_ID = pag.DOCTO_CP_ID
INNER JOIN DOCTOS_BA dba
    ON dba.DOCTO_BA_ID = des1.DOCTO_DEST_ID
INNER JOIN DOCTOS_ENTRE_SIS des2
    ON des2.DOCTO_FTE_ID = dba.DOCTO_BA_ID
INNER JOIN DOCTOS_CO dco
    ON dco.DOCTO_CO_ID = des2.DOCTO_DEST_ID
WHERE fac.FOLIO = @FOLIO
  AND fac.PROVEEDOR_ID = @PROVEEDOR";

                    foreach (var par in folioYProveedor)
                    {
                        if (ct.IsCancellationRequested) break;
                        string folio = par.Key ?? "";
                        int    prov  = par.Value;
                        if (string.IsNullOrEmpty(folio) || prov <= 0) continue;

                        try
                        {
                            using (var cmd = new FbCommand(sql, con.FBC))
                            {
                                cmd.Parameters.AddWithValue("@FOLIO", folio);
                                cmd.Parameters.AddWithValue("@PROVEEDOR", prov);
                                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                                {
                                    if (await rd.ReadAsync(ct).ConfigureAwait(false))
                                    {
                                        string num   = Convert.ToString(rd["NUM_POLIZA"]) ?? "";
                                        string fecha = "";
                                        var raw      = rd["FECHA_POLIZA"];
                                        if (raw != DBNull.Value)
                                        {
                                            fecha = Convert.ToDateTime(raw).ToString("dd/MM/yyyy");
                                        }
                                        resultado[ClaveDic(folio, prov)] = new DatoPoliza
                                        {
                                            NumPoliza   = num,
                                            FechaPoliza = fecha,
                                        };
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Una fila que falle no debe matar el lote.
                        }
                    }
                }
                finally { con.Desconectar(); }
            }
            catch
            {
                // Sin conexión, el grid se queda sin columnas de póliza —
                // mejor eso que matar el flujo de la vista.
            }
            return resultado;
        }

        public static string ClaveDic(string folio, int prov)
        {
            return (folio ?? "") + "|" + prov;
        }
    }
}
