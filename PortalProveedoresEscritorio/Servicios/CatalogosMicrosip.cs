using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresEscritorio.Servicios
{
    /// <summary>
    /// Lee catálogos de Microsip directamente desde la base Firebird de la
    /// empresa seleccionada (mismo patrón que <see cref="PermisosMicrosip"/>:
    /// reconecta por demanda usando el <c>NOMBRE_CORTO</c> de la empresa).
    ///
    /// El escritorio NO sincroniza estos catálogos al portal — se leen al
    /// momento en que se abre un modal que los necesita (ej. condiciones de
    /// pago al abrir FormAplicarFactura, artículos al abrir el buscador).
    ///
    /// Réplica funcional de las queries del SOAP:
    /// <list type="bullet">
    ///   <item><c>CONDICIONES_PAGO_CP</c> INNER JOIN <c>PLAZOS_COND_PAG_CP</c>
    ///         → combo cbCondPago en F_APLICAR_FACTURA (compras/CxP).</item>
    ///   <item><c>ARTICULOS</c> con <c>ES_ALMACENABLE='N'</c> → grid F_BUSQUEDA.</item>
    /// </list>
    /// </summary>
    public sealed class CatalogosMicrosip
    {
        /// <summary>
        /// Lista las condiciones de pago de Microsip de la empresa. Réplica
        /// LITERAL del SOAP F_APLICAR_FACTURA.cs:1706-1718 (método INICIAR):
        /// <code>
        /// query  = "SELECT * FROM condiciones_pago_cp c ";
        /// query += "INNER JOIN plazos_cond_pag_cp p ON ( c.cond_pago_id = p.cond_pago_id ) ";
        /// query += "ORDER BY p.dias_plazo";
        /// // ... while(read) cbCondPago.Items.Add(fdr["NOMBRE"]);
        /// </code>
        /// Las condiciones de pago de COMPRAS/cuentas por pagar viven en
        /// <c>CONDICIONES_PAGO_CP</c> (NO en <c>CONDICIONES_PAGO</c>, que es la
        /// tabla de VENTAS). Leer la tabla de ventas dejaba el combo vacío
        /// cuando la empresa no vendía con esas condiciones, y parecía un
        /// problema de conexión.
        ///
        /// El JOIN con <c>PLAZOS_COND_PAG_CP</c> puede producir varias filas
        /// por la misma condición (una por plazo). El combo del SOAP añadía
        /// cada fila tal cual; aquí deduplicamos por <c>COND_PAGO_ID</c>
        /// quedándonos con la primera (la del menor <c>DIAS_PLAZO</c>, por el
        /// ORDER BY) para no mostrar la misma condición repetida.
        /// </summary>
        public async Task<CondicionPagoMicrosip[]> ListarCondicionesPagoAsync(
            string nombreEmpresa, CancellationToken ct)
        {
            var lista = new List<CondicionPagoMicrosip>();
            if (string.IsNullOrEmpty(nombreEmpresa)) return lista.ToArray();

            var vistos = new HashSet<int>();

            try
            {
                var con = new ConexionMicrosip();
                if (!con.ConectarMicrosip(nombreEmpresa)) return lista.ToArray();
                try
                {
                    // Réplica del SOAP F_APLICAR_FACTURA.cs:1707-1709.
                    const string sql =
                        "SELECT C.COND_PAGO_ID, C.NOMBRE " +
                        "  FROM CONDICIONES_PAGO_CP C " +
                        "  INNER JOIN PLAZOS_COND_PAG_CP P ON (C.COND_PAGO_ID = P.COND_PAGO_ID) " +
                        " ORDER BY P.DIAS_PLAZO";
                    using (var cmd = new FbCommand(sql, con.FBC))
                    using (var rd  = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                    {
                        while (await rd.ReadAsync(ct).ConfigureAwait(false))
                        {
                            int id = Convert.ToInt32(rd["COND_PAGO_ID"]);
                            // Dedup por COND_PAGO_ID — mantenemos la primera
                            // ocurrencia (igual que añadir cada fila pero sin
                            // duplicar la misma condición con varios plazos).
                            if (!vistos.Add(id)) continue;
                            lista.Add(new CondicionPagoMicrosip
                            {
                                Id     = id,
                                Nombre = (Convert.ToString(rd["NOMBRE"]) ?? "").Trim(),
                            });
                        }
                    }
                }
                finally { con.Desconectar(); }
            }
            catch
            {
                // Best-effort — sin conexión devolvemos lista vacía y la UI
                // muestra un placeholder. Mejor eso que romper el modal.
            }
            return lista.ToArray();
        }

        /// <summary>
        /// Lista las SERIES de folios de COMPRAS de Microsip de la empresa,
        /// para el combo de serie del F_APLICAR_FACTURA. Réplica de la consulta
        /// del SOAP nuevo (F_APLICAR_FACTURA.cs:1722-1723):
        /// <code>SELECT SERIE FROM folios_compras WHERE TIPO_DOCTO = 'C' ORDER BY SERIE</code>
        /// Devuelve los <c>SERIE</c> (string, trimmeados) — el folio interno de
        /// Microsip se arma como SERIE + consecutivo. TIPO_DOCTO='C' = compras
        /// (no ventas ni otros doctos). Best-effort: sin conexión devuelve vacío.
        /// </summary>
        public async Task<string[]> ListarSeriesFoliosComprasAsync(
            string nombreEmpresa, CancellationToken ct)
        {
            var lista = new List<string>();
            if (string.IsNullOrEmpty(nombreEmpresa)) return lista.ToArray();

            try
            {
                var con = new ConexionMicrosip();
                if (!con.ConectarMicrosip(nombreEmpresa)) return lista.ToArray();
                try
                {
                    const string sql =
                        "SELECT SERIE FROM FOLIOS_COMPRAS " +
                        " WHERE TIPO_DOCTO = 'C' ORDER BY SERIE";
                    using (var cmd = new FbCommand(sql, con.FBC))
                    using (var rd  = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                    {
                        while (await rd.ReadAsync(ct).ConfigureAwait(false))
                        {
                            var serie = (Convert.ToString(rd["SERIE"]) ?? "").Trim();
                            if (serie.Length > 0) lista.Add(serie);
                        }
                    }
                }
                finally { con.Desconectar(); }
            }
            catch
            {
                // Best-effort — sin conexión la UI muestra un placeholder.
            }
            return lista.ToArray();
        }

        /// <summary>
        /// Busca artículos NO almacenables (los "generales" usados para
        /// aplicar facturas del portal). Si <paramref name="filtro"/> está
        /// vacío, devuelve toda la lista (limitada). Réplica del SOAP
        /// F_BUSQUEDA + F_APLICAR_FACTURA línea 1028.
        ///
        /// El filtro hace LIKE %X% en <c>ARTICULOS.NOMBRE</c>.
        /// </summary>
        public async Task<ArticuloMicrosip[]> BuscarArticulosAsync(
            string nombreEmpresa, string filtro, int limite, CancellationToken ct)
        {
            var lista = new List<ArticuloMicrosip>();
            if (string.IsNullOrEmpty(nombreEmpresa)) return lista.ToArray();
            if (limite <= 0 || limite > 5000) limite = 500;
            var filtroLimpio = (filtro ?? "").Trim();

            try
            {
                var con = new ConexionMicrosip();
                if (!con.ConectarMicrosip(nombreEmpresa)) return lista.ToArray();
                try
                {
                    // Réplica de F_APLICAR_FACTURA.cs:1028 — usa LEFT JOIN
                    // con CLAVES_ARTICULOS para tener la clave del artículo.
                    var sql =
                        "SELECT FIRST " + limite + " " +
                        "       A.ARTICULO_ID, A.NOMBRE, A.ES_ALMACENABLE, " +
                        "       C.CLAVE_ARTICULO " +
                        "  FROM ARTICULOS A " +
                        "  LEFT JOIN CLAVES_ARTICULOS C ON (A.ARTICULO_ID = C.ARTICULO_ID) " +
                        " WHERE A.ES_ALMACENABLE = 'N' ";

                    bool conFiltro = filtroLimpio.Length > 0;
                    if (conFiltro)
                        sql += " AND UPPER(A.NOMBRE) LIKE @filtro ";

                    sql += " ORDER BY A.NOMBRE";

                    using (var cmd = new FbCommand(sql, con.FBC))
                    {
                        if (conFiltro)
                            cmd.Parameters.Add("@filtro", FbDbType.VarChar).Value
                                = "%" + filtroLimpio.ToUpperInvariant() + "%";

                        using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                        {
                            while (await rd.ReadAsync(ct).ConfigureAwait(false))
                            {
                                lista.Add(new ArticuloMicrosip
                                {
                                    Id            = Convert.ToInt32(rd["ARTICULO_ID"]),
                                    Nombre        = (Convert.ToString(rd["NOMBRE"]) ?? "").Trim(),
                                    Clave         = rd["CLAVE_ARTICULO"] == DBNull.Value
                                                        ? ""
                                                        : (Convert.ToString(rd["CLAVE_ARTICULO"]) ?? "").Trim(),
                                    EsAlmacenable = false,
                                });
                            }
                        }
                    }
                }
                finally { con.Desconectar(); }
            }
            catch
            {
                // Best-effort.
            }
            return lista.ToArray();
        }
    }
}
