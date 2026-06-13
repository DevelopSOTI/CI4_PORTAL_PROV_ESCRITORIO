using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Implementación del DAO de recepciones contra Firebird. Replica los dos
    /// SELECTs del Delphi histórico (cabecera en Func_Calcula.pas:374-406,
    /// detalle en Func_Recepciones.pas:50-53) y los une en memoria para
    /// poder enviar cabecera + detalle al portal en una sola request.
    ///
    /// OPTIMIZACIONES respecto al Delphi:
    ///
    ///  1. <b>DIAS_PLAZO inline</b>. El Delphi hace una query extra por
    ///     recepción (<c>GET_PLAZO(docto_cm_id)</c> en Func.pas:303-325) =
    ///     N+1 queries. Aquí lo trae el mismo SELECT principal con una
    ///     subconsulta <c>FIRST 1</c> correlacionada (misma semántica que
    ///     GET_PLAZO: primera fila de PLAZOS_COND_PAG_CP, sin ORDER BY).
    ///
    ///  2. <b>Detalle batch</b>. El Delphi consulta el detalle por FOLIO
    ///     dentro del loop = otra N+1. Aquí leo TODO el detalle del ciclo
    ///     con un único <c>WHERE docto_cm_id IN (...)</c> y agrupo en C#.
    ///
    ///  3. <b>Filtro de fecha con COALESCE</b>. Mismo patrón que catálogos
    ///     para esquivar el bug "fila con FECHA_HORA_CREACION pero
    ///     FECHA_HORA_ULT_MODIF nulo" (ver ProveedoresRepository).
    /// </summary>
    public sealed class RecepcionesRepository : IRecepcionesRepository
    {
        public async Task<IReadOnlyList<RecepcionMicrosip>> ListarAsync(string nombreEmpresa, DateTime? desde, CancellationToken ct)
        {
            var resultado = new List<RecepcionMicrosip>();

            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreEmpresa))
            {
                EventoLog.Warning("RecepcionesRepository: no se pudo abrir BD de '" + nombreEmpresa + "'.");
                return resultado;
            }

            try
            {
                // Detectar columnas opcionales del schema:
                //  - LIBRES_REC_CM.USO_CFDI puede no existir si el operador
                //    nunca autorizó la empresa por el nuevo Configurador (caso
                //    de empresas heredadas del SOAP). Sin esta detección, el
                //    LEFT JOIN truena con "Column unknown LC.USO_CFDI" (-206).
                //  - PLAZOS_COND_PAG_CP es estándar en Microsip pero si la
                //    tabla no existe se ignora con COALESCE.
                var tieneUsoCfdi = await TablaTieneColumnaAsync(con.FBC, "LIBRES_REC_CM", "USO_CFDI", ct).ConfigureAwait(false);

                // ---- Cabecera ----
                resultado = await LeerCabecerasAsync(con.FBC, desde, ct, tieneUsoCfdi).ConfigureAwait(false);
                if (resultado.Count == 0) return resultado;

                // ---- Detalle (batch único) ----
                var doctoIds = new List<int>(resultado.Count);
                foreach (var r in resultado) doctoIds.Add(r.docto_cm_id);

                var detallesPorDocto = await LeerDetalleBatchAsync(con.FBC, doctoIds, ct).ConfigureAwait(false);

                // Hidratar cada cabecera con su detalle.
                foreach (var r in resultado)
                {
                    List<RecepcionDetalleMicrosip> dets;
                    r.detalle = detallesPorDocto.TryGetValue(r.docto_cm_id, out dets)
                        ? dets
                        : new List<RecepcionDetalleMicrosip>();
                }
            }
            catch (Exception ex)
            {
                EventoLog.Error("RecepcionesRepository('" + nombreEmpresa + "'): " + ex.Message);
            }
            finally
            {
                con.Desconectar();
            }

            return resultado;
        }

        // ====================================================================
        // Cabecera — SELECT con todos los JOINs (monedas, libres_rec_cm,
        // listas_atributos, condiciones_pago_cp, plazos_cond_pag_cp)
        // ====================================================================

        private static async Task<List<RecepcionMicrosip>> LeerCabecerasAsync(FbConnection con, DateTime? desde, CancellationToken ct, bool tieneUsoCfdi)
        {
            var lista = new List<RecepcionMicrosip>();

            var sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("  dc.DOCTO_CM_ID, dc.FOLIO, dc.FECHA, dc.CLAVE_PROV, dc.PROVEEDOR_ID, ");
            sql.Append("  dc.MONEDA_ID, dc.IMPORTE_NETO, dc.TOTAL_IMPUESTOS, dc.TOTAL_RETENCIONES, ");
            sql.Append("  COALESCE(dc.FECHA_HORA_ULT_MODIF, dc.FECHA_HORA_CREACION) AS FECHA_HORA_ULT_MODIF, ");
            sql.Append("  dc.ALMACEN_ID, dc.ESTATUS, ");
            sql.Append("  m.NOMBRE AS MONEDA_NOMBRE, m.CLAVE_FISCAL AS MONEDA_SIMBOLO, ");
            // USO_CFDI: solo se incluye si la columna existe en LIBRES_REC_CM.
            // Si no existe (empresa heredada antes del nuevo Configurador),
            // devolvemos string vacío como fallback.
            if (tieneUsoCfdi)
                sql.Append("  COALESCE(la.VALOR_DESPLEGADO, '') AS USO_CFDI, ");
            else
                sql.Append("  '' AS USO_CFDI, ");
            // Plazo: réplica de GET_PLAZO del Delphi (Func.pas:303-325) —
            // consulta aparte por recepción que une doctos_cm →
            // condiciones_pago_cp → plazos_cond_pag_cp y toma la PRIMERA fila
            // del resultado (sin ORDER BY; orden natural). Aquí lo resolvemos
            // como subconsulta FIRST 1 correlacionada para no duplicar
            // cabeceras: un LEFT JOIN directo a PLAZOS_COND_PAG_CP producía N
            // filas por recepción cuando la condición de pago tiene N plazos
            // (mismo FOLIO upserteado N veces). Sin ORDER BY igual que el
            // Delphi — el orden natural/PK de Firebird es lo más cercano al
            // comportamiento original.
            sql.Append("  COALESCE((SELECT FIRST 1 p.DIAS_PLAZO ");
            sql.Append("              FROM CONDICIONES_PAGO_CP c ");
            sql.Append("              JOIN PLAZOS_COND_PAG_CP p ON (p.COND_PAGO_ID = c.COND_PAGO_ID) ");
            sql.Append("             WHERE c.COND_PAGO_ID = dc.COND_PAGO_ID), 0) AS DIAS_PLAZO ");
            sql.Append("FROM DOCTOS_CM dc ");
            sql.Append("JOIN MONEDAS m ON (dc.MONEDA_ID = m.MONEDA_ID) ");
            if (tieneUsoCfdi)
            {
                sql.Append("LEFT JOIN LIBRES_REC_CM lc ON (lc.DOCTO_CM_ID = dc.DOCTO_CM_ID) ");
                sql.Append("LEFT JOIN LISTAS_ATRIBUTOS la ON (lc.USO_CFDI = la.LISTA_ATRIB_ID) ");
            }
            sql.Append("WHERE dc.TIPO_DOCTO = 'R' ");

            if (desde.HasValue)
            {
                sql.Append("AND COALESCE(dc.FECHA_HORA_ULT_MODIF, dc.FECHA_HORA_CREACION) > @desde ");
            }
            else
            {
                // Fallback: si nunca se ha sincronizado y EMP_SINC_DESDE es null,
                // traemos los últimos 90 días para no descargar AÑOS de historia
                // en la carga inicial. El operador puede cambiar EMP_SINC_DESDE
                // si quiere ir más atrás.
                sql.Append("AND dc.FECHA >= DATEADD(-90 DAY TO CURRENT_DATE) ");
            }

            sql.Append("ORDER BY dc.DOCTO_CM_ID");

            using (var cmd = new FbCommand(sql.ToString(), con))
            {
                if (desde.HasValue)
                    cmd.Parameters.Add("@desde", FbDbType.TimeStamp).Value = desde.Value;

                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var fecha          = rd.IsDBNull(rd.GetOrdinal("FECHA"))                  ? (DateTime?) null : (DateTime?) Convert.ToDateTime(rd["FECHA"]);
                        var fechaUltModif  = rd.IsDBNull(rd.GetOrdinal("FECHA_HORA_ULT_MODIF"))   ? (DateTime?) null : (DateTime?) Convert.ToDateTime(rd["FECHA_HORA_ULT_MODIF"]);

                        lista.Add(new RecepcionMicrosip
                        {
                            docto_cm_id          = Convert.ToInt32(rd["DOCTO_CM_ID"]),
                            folio                = (Convert.ToString(rd["FOLIO"])      ?? "").Trim(),
                            fecha                = fecha.HasValue        ? fecha.Value.ToString("yyyy-MM-dd HH:mm:ss")        : "",
                            clave_prov           = (Convert.ToString(rd["CLAVE_PROV"]) ?? "").Trim(),
                            proveedor_id         = rd.IsDBNull(rd.GetOrdinal("PROVEEDOR_ID")) ? 0 : Convert.ToInt32(rd["PROVEEDOR_ID"]),
                            moneda_id            = rd.IsDBNull(rd.GetOrdinal("MONEDA_ID"))    ? 0 : Convert.ToInt32(rd["MONEDA_ID"]),
                            moneda_nombre        = (Convert.ToString(rd["MONEDA_NOMBRE"])  ?? "").Trim(),
                            moneda_simbolo       = (Convert.ToString(rd["MONEDA_SIMBOLO"]) ?? "").Trim(),
                            importe_neto         = rd.IsDBNull(rd.GetOrdinal("IMPORTE_NETO"))      ? 0m : Convert.ToDecimal(rd["IMPORTE_NETO"]),
                            total_impuestos      = rd.IsDBNull(rd.GetOrdinal("TOTAL_IMPUESTOS"))   ? 0m : Convert.ToDecimal(rd["TOTAL_IMPUESTOS"]),
                            total_retenciones    = rd.IsDBNull(rd.GetOrdinal("TOTAL_RETENCIONES")) ? 0m : Convert.ToDecimal(rd["TOTAL_RETENCIONES"]),
                            fecha_hora_ult_modif = fechaUltModif.HasValue ? fechaUltModif.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                            almacen_id           = rd.IsDBNull(rd.GetOrdinal("ALMACEN_ID")) ? 0 : Convert.ToInt32(rd["ALMACEN_ID"]),
                            estatus              = (Convert.ToString(rd["ESTATUS"]) ?? "").Trim(),
                            dias_plazo           = rd.IsDBNull(rd.GetOrdinal("DIAS_PLAZO")) ? 0 : Convert.ToInt32(rd["DIAS_PLAZO"]),
                            uso_cfdi             = ExtraerClaveSat((Convert.ToString(rd["USO_CFDI"]) ?? "").Trim()),
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Extrae el código SAT (G01, G03, P01, etc.) del valor desplegado de
        /// LISTAS_ATRIBUTOS, que viene como "G01 - Adquisición de mercancías".
        /// Réplica EXACTA de GET_USO_CLAVE del Delphi (Func.pas:381-392):
        ///   position := Pos('-', uso_cfdi) - 1;
        ///   Result := Trim(Copy(uso_cfdi, 0, position));
        /// Es decir: corta en el primer GUION ('-') y aplica Trim. Si la cadena
        /// NO contiene guion, Pos devuelve 0 → Copy con count -1 devuelve ''
        /// (cadena vacía), NO el string completo.
        /// </summary>
        private static string ExtraerClaveSat(string valorDesplegado)
        {
            if (string.IsNullOrEmpty(valorDesplegado)) return "";
            int idxGuion = valorDesplegado.IndexOf('-');
            if (idxGuion < 0) return "";   // sin '-': el Delphi devuelve vacío
            return valorDesplegado.Substring(0, idxGuion).Trim();
        }

        // ====================================================================
        // Metadata Firebird — detectar si una columna existe
        // ====================================================================

        /// <summary>
        /// True si la columna existe en la tabla. Usa el catálogo del sistema
        /// (<c>RDB$RELATION_FIELDS</c>) para responder en una sola query
        /// metadata muy barata.
        /// </summary>
        private static async Task<bool> TablaTieneColumnaAsync(FbConnection con, string tabla, string columna, CancellationToken ct)
        {
            const string sql =
                "SELECT FIRST 1 1 FROM RDB$RELATION_FIELDS " +
                "WHERE RDB$RELATION_NAME = @t AND RDB$FIELD_NAME = @c";
            using (var cmd = new FbCommand(sql, con))
            {
                cmd.Parameters.Add("@t", FbDbType.VarChar).Value = tabla;
                cmd.Parameters.Add("@c", FbDbType.VarChar).Value = columna;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    return await rd.ReadAsync(ct).ConfigureAwait(false);
                }
            }
        }

        // ====================================================================
        // Detalle — batch con WHERE docto_cm_id IN (...)
        // ====================================================================

        private static async Task<Dictionary<int, List<RecepcionDetalleMicrosip>>> LeerDetalleBatchAsync(
            FbConnection con, List<int> doctoIds, CancellationToken ct)
        {
            var porDocto = new Dictionary<int, List<RecepcionDetalleMicrosip>>(doctoIds.Count);
            if (doctoIds.Count == 0) return porDocto;

            // Firebird permite IN con muchos elementos pero tiene un límite
            // (~1500 parámetros por query). Si excedemos, lo hacemos por chunks.
            const int chunkSize = 1000;
            for (int offset = 0; offset < doctoIds.Count; offset += chunkSize)
            {
                ct.ThrowIfCancellationRequested();
                int count = Math.Min(chunkSize, doctoIds.Count - offset);
                await LeerDetalleChunkAsync(con, doctoIds, offset, count, porDocto, ct).ConfigureAwait(false);
            }

            return porDocto;
        }

        private static async Task LeerDetalleChunkAsync(
            FbConnection con,
            List<int> doctoIds, int offset, int count,
            Dictionary<int, List<RecepcionDetalleMicrosip>> destino,
            CancellationToken ct)
        {
            // Construimos el "IN (?,?,?,...)" con N parámetros — Firebird los
            // necesita posicionales en cantidad exacta.
            var sb = new StringBuilder();
            sb.Append("SELECT dcd.DOCTO_CM_DET_ID, dcd.DOCTO_CM_ID, art.NOMBRE, ");
            sb.Append("       dcd.UNIDADES, dcd.PRECIO_UNITARIO, dcd.PCTJE_DSCTO, ");
            sb.Append("       dcd.PRECIO_TOTAL_NETO, dcd.NOTAS, dcd.POSICION ");
            sb.Append("FROM DOCTOS_CM_DET dcd ");
            sb.Append("JOIN ARTICULOS art ON (dcd.ARTICULO_ID = art.ARTICULO_ID) ");
            sb.Append("WHERE dcd.DOCTO_CM_ID IN (");
            for (int i = 0; i < count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append('@').Append("d").Append(i);
            }
            sb.Append(") ORDER BY dcd.DOCTO_CM_ID, dcd.POSICION");

            using (var cmd = new FbCommand(sb.ToString(), con))
            {
                for (int i = 0; i < count; i++)
                    cmd.Parameters.Add("@d" + i, FbDbType.Integer).Value = doctoIds[offset + i];

                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var doctoId = Convert.ToInt32(rd["DOCTO_CM_ID"]);
                        List<RecepcionDetalleMicrosip> lista;
                        if (!destino.TryGetValue(doctoId, out lista))
                        {
                            lista = new List<RecepcionDetalleMicrosip>();
                            destino[doctoId] = lista;
                        }

                        lista.Add(new RecepcionDetalleMicrosip
                        {
                            docto_cm_det_id   = Convert.ToInt32(rd["DOCTO_CM_DET_ID"]),
                            nombre            = (Convert.ToString(rd["NOMBRE"]) ?? "").Trim(),
                            unidades          = rd.IsDBNull(rd.GetOrdinal("UNIDADES"))          ? 0m : Convert.ToDecimal(rd["UNIDADES"]),
                            precio_unitario   = rd.IsDBNull(rd.GetOrdinal("PRECIO_UNITARIO"))   ? 0m : Convert.ToDecimal(rd["PRECIO_UNITARIO"]),
                            pctje_dscto       = rd.IsDBNull(rd.GetOrdinal("PCTJE_DSCTO"))       ? 0m : Convert.ToDecimal(rd["PCTJE_DSCTO"]),
                            precio_total_neto = rd.IsDBNull(rd.GetOrdinal("PRECIO_TOTAL_NETO")) ? 0m : Convert.ToDecimal(rd["PRECIO_TOTAL_NETO"]),
                            notas             = (Convert.ToString(rd["NOTAS"]) ?? "").Trim(),
                            posicion          = rd.IsDBNull(rd.GetOrdinal("POSICION")) ? 0 : Convert.ToInt32(rd["POSICION"]),
                        });
                    }
                }
            }
        }
    }
}
