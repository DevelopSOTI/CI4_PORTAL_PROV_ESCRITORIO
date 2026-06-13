using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Implementación del DAO de proveedores contra el Firebird de cada empresa.
    /// Replica el SELECT triple-JOIN de <c>ACTUALIZA_PROVEEDORES</c> del Delphi
    /// histórico (Func_Calcula.pas:288-313), parametrizado y async.
    ///
    /// CORRECCIONES respecto al Delphi original (errores aritméticos del SQL
    /// histórico que aquí se subsanan; comportamiento sigue siendo equivalente
    /// — se obtienen los mismos proveedores, pero ahora también en carga inicial
    /// y sin duplicados por OR mal parseado):
    ///
    ///  1. <b>rol_clave_prov_id = 49 movido al JOIN ON</b>. En el Delphi se
    ///     agregaba como <c>AND c.rol_clave_prov_id = 49</c> dentro del WHERE
    ///     solo cuando había filtro de fecha. Eso provocaba dos problemas:
    ///     (a) en carga inicial no se aplicaba → proveedores con múltiples
    ///     claves se duplicaban en el resultado;
    ///     (b) por precedencia AND/OR el filtro solo afectaba a la rama
    ///     <c>FECHA_HORA_ULT_MODIF &gt; X</c>, no a <c>FECHA_HORA_CREACION &gt; X</c>.
    ///     Solución: poner la condición en el ON del JOIN — aplica siempre.
    ///
    ///  2. <b>Paréntesis en el OR del WHERE</b>. <c>(creacion &gt; X OR modif &gt; X)</c>
    ///     en lugar de <c>creacion &gt; X OR modif &gt; X</c> suelto. Aunque
    ///     ahora el rol_clave_prov_id no está en el WHERE, dejamos los paréntesis
    ///     como defensa en profundidad para cuando se sumen más condiciones.
    ///
    ///  3. <b>Detección dinámica de columnas de LIBRES_PROVEEDOR</b>. En
    ///     Microsip los "campos libres" (PERMITIR_SIN_RECEPCION, PCTJE_RECHAZO,
    ///     REFERENCIA, ADJUNTAR_ARCHIVOS) son configurables por instalación:
    ///     cada cliente puede no tenerlos, tenerlos en LIBRES_PROVEEDOR o
    ///     tenerlos en la propia tabla PROVEEDORES (Microsip moderno). El
    ///     Delphi asumía que estaban en LIBRES_PROVEEDOR y reventaba con
    ///     "Column unknown" si no era el caso. Aquí inspeccionamos
    ///     RDB$RELATION_FIELDS para resolver cada campo: primero en
    ///     LIBRES_PROVEEDOR, luego en PROVEEDORES, y si no está en ninguna,
    ///     usamos un literal default en el SELECT.
    /// </summary>
    public sealed class ProveedoresRepository : IProveedoresRepository
    {
        public async Task<IReadOnlyList<ProveedorMicrosip>> ListarAsync(string nombreEmpresa, DateTime? desde, CancellationToken ct)
        {
            var lista = new List<ProveedorMicrosip>();

            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreEmpresa))
            {
                EventoLog.Warning("ProveedoresRepository: no se pudo abrir BD de '" + nombreEmpresa + "'.");
                return lista;
            }

            try
            {
                // Detectar dónde viven los campos libres en ESTA instalación
                // de Microsip (ver doc de la clase, corrección #3).
                var colsLibres = await ObtenerColumnasTablaAsync(con.FBC, "LIBRES_PROVEEDOR", ct).ConfigureAwait(false);
                var colsProv   = await ObtenerColumnasTablaAsync(con.FBC, "PROVEEDORES",      ct).ConfigureAwait(false);

                // Usamos COALESCE(ULT_MODIF, CREACION) tanto en el SELECT como
                // en el WHERE para que:
                //   1) En el WHERE, una fila con ULT_MODIF=NULL pero CREACION
                //      reciente igual califique como "modificada".
                //   2) En el SELECT, la "fecha efectiva" que viaja al portal
                //      siempre tenga valor. Sin esto, el portal guardaría NULL
                //      en FECHA_ULT_MODIF, su checkpoint MAX(ULT_MODIF) no
                //      avanzaría, y la misma fila seguiría siendo "modificada"
                //      en cada ciclo (loop infinito eficiente, pero ruidoso).
                string sqlBase =
                    "SELECT" +
                    "  p.PROVEEDOR_ID," +
                    "  p.NOMBRE," +
                    "  p.ESTATUS," +
                    "  COALESCE(p.FECHA_HORA_ULT_MODIF, p.FECHA_HORA_CREACION) AS FECHA_HORA_ULT_MODIF," +
                    "  c.CLAVE_PROV," +
                    "  p.RFC_CURP," +
                    "  " + ResolverColumna(colsLibres, colsProv, "PERMITIR_SIN_RECEPCION", "'N'") + " AS PERMITIR_SIN_RECEPCION," +
                    "  " + ResolverColumna(colsLibres, colsProv, "PCTJE_RECHAZO",          "0")   + " AS PCTJE_RECHAZO," +
                    "  " + ResolverColumna(colsLibres, colsProv, "REFERENCIA",             "''")  + " AS REFERENCIA," +
                    "  " + ResolverColumna(colsLibres, colsProv, "ADJUNTAR_ARCHIVOS",      "'N'") + " AS ADJUNTAR_ARCHIVOS " +
                    "FROM PROVEEDORES p " +
                    "JOIN CLAVES_PROVEEDORES c ON (p.PROVEEDOR_ID = c.PROVEEDOR_ID AND c.ROL_CLAVE_PROV_ID = 49) " +
                    "JOIN LIBRES_PROVEEDOR l ON (p.PROVEEDOR_ID = l.PROVEEDOR_ID)";

                string sql;
                if (desde.HasValue)
                {
                    sql = sqlBase +
                          " WHERE COALESCE(p.FECHA_HORA_ULT_MODIF, p.FECHA_HORA_CREACION) > @desde" +
                          " ORDER BY p.PROVEEDOR_ID";
                }
                else
                {
                    sql = sqlBase + " ORDER BY p.PROVEEDOR_ID";
                }

                using (var cmd = new FbCommand(sql, con.FBC))
                {
                    if (desde.HasValue)
                        cmd.Parameters.Add("@desde", FbDbType.TimeStamp).Value = desde.Value;

                    using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                    {
                        while (await rd.ReadAsync(ct).ConfigureAwait(false))
                        {
                            var fechaUltModif = rd.IsDBNull(3)
                                ? (DateTime?) null
                                : (DateTime?) Convert.ToDateTime(rd[3]);

                            lista.Add(new ProveedorMicrosip
                            {
                                proveedor_id          = Convert.ToInt32(rd["PROVEEDOR_ID"]),
                                nombre                = (Convert.ToString(rd["NOMBRE"])       ?? "").Trim(),
                                estatus               = (Convert.ToString(rd["ESTATUS"])      ?? "").Trim(),
                                clave_prov            = (Convert.ToString(rd["CLAVE_PROV"])   ?? "").Trim(),
                                fecha_hora_ult_modif  = fechaUltModif.HasValue
                                    ? fechaUltModif.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                    : "",
                                rfc                   = (Convert.ToString(rd["RFC_CURP"])     ?? "").Trim(),
                                prov_priv             = SiNo(Convert.ToString(rd["PERMITIR_SIN_RECEPCION"])),
                                pctje_rechazo         = rd.IsDBNull(rd.GetOrdinal("PCTJE_RECHAZO"))
                                    ? 0m
                                    : Convert.ToDecimal(rd["PCTJE_RECHAZO"]),
                                referencia            = (Convert.ToString(rd["REFERENCIA"])   ?? "").Trim(),
                                adjuntar_archivos     = SiNo(Convert.ToString(rd["ADJUNTAR_ARCHIVOS"])),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventoLog.Error("ProveedoresRepository('" + nombreEmpresa + "'): " + ex.Message);
            }
            finally
            {
                con.Desconectar();
            }

            return lista;
        }

        /// <summary>
        /// Conversión 'S'/'N' → "SI"/"NO" — herencia del Delphi histórico,
        /// que es lo que la tabla MySQL PROVEEDORES_MSP espera en PROV_PRIV y
        /// ADJUNTAR_ARCHIVOS (varchar(2)). Cualquier valor distinto de 'S'
        /// se mapea a "NO" (incluye null, vacío, espacios).
        /// </summary>
        private static string SiNo(string valorFirebird)
        {
            if (string.IsNullOrWhiteSpace(valorFirebird)) return "NO";
            return valorFirebird.Trim().ToUpperInvariant() == "S" ? "SI" : "NO";
        }

        /// <summary>
        /// Construye la expresión SQL para una columna "libre" del proveedor.
        /// Preferencia: LIBRES_PROVEEDOR &gt; PROVEEDORES &gt; literal default.
        /// La preferencia por LIBRES_PROVEEDOR replica el comportamiento del
        /// Delphi original; el fallback a PROVEEDORES y al literal es nuevo,
        /// para soportar Microsip modernos y clientes con esquema reducido.
        /// </summary>
        private static string ResolverColumna(HashSet<string> colsLibres, HashSet<string> colsProv, string nombre, string literalDefault)
        {
            if (colsLibres.Contains(nombre)) return "l." + nombre;
            if (colsProv  .Contains(nombre)) return "p." + nombre;
            return literalDefault;
        }

        /// <summary>
        /// Lista los nombres (trimmed, comparación case-insensitive) de las
        /// columnas de una tabla Firebird, leyendo de RDB$RELATION_FIELDS.
        /// Las columnas Firebird se almacenan en CHAR(31) con padding a la
        /// derecha, por eso el TRIM.
        /// </summary>
        private static async Task<HashSet<string>> ObtenerColumnasTablaAsync(FbConnection con, string nombreTabla, CancellationToken ct)
        {
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var cmd = new FbCommand(
                "SELECT RDB$FIELD_NAME FROM RDB$RELATION_FIELDS WHERE RDB$RELATION_NAME = @t", con))
            {
                cmd.Parameters.Add("@t", FbDbType.VarChar).Value = nombreTabla;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var nombreCol = Convert.ToString(rd[0]);
                        if (!string.IsNullOrEmpty(nombreCol)) cols.Add(nombreCol.Trim());
                    }
                }
            }

            return cols;
        }
    }
}
