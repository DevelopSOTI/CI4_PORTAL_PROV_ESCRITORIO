using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresService.Repositorios
{
    /// <summary>
    /// Implementación del DAO para documentos de cuentas por pagar. Mismo
    /// SELECT que el Delphi histórico (Func_Calcula.pas:454-491 para
    /// créditos, 540-577 para notas), parametrizado en <c>cc.tipo</c> para
    /// servir a ambos sincronizadores con un solo método.
    ///
    /// Validación "pago liberado en bancos" — SOLO para NOTAS: en la rama de
    /// notas del Delphi (Func_Calcula.pas:556-564) los JOINs a
    /// doctos_entre_sis/doctos_ba y los filtros clave_sis_dest/db.aplicado
    /// están VIVOS; en la rama de créditos (Func_Calcula.pas:470-478) esas
    /// mismas líneas están comentadas (solo queda vivo en créditos un
    /// LEFT JOIN suelto a doctos_entre_sis en la línea 473, sin filtro
    /// asociado — no lo replicamos porque sin condiciones no filtra nada y
    /// solo duplicaría cabeceras).
    /// </summary>
    public sealed class DoctosCpRepository : IDoctosCpRepository
    {
        public async Task<IReadOnlyList<DoctoCpMicrosip>> ListarAsync(string nombreEmpresa, string ccTipo, DateTime? desde, CancellationToken ct)
        {
            var lista = new List<DoctoCpMicrosip>();

            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreEmpresa))
            {
                EventoLog.Warning("DoctosCpRepository: no se pudo abrir BD de '" + nombreEmpresa + "'.");
                return lista;
            }

            try
            {
                bool esNotas = string.Equals(ccTipo, "R", StringComparison.OrdinalIgnoreCase);

                var sql = new StringBuilder();
                sql.Append("SELECT ");
                sql.Append("  dc.DOCTO_CP_ID, dc.CONCEPTO_CP_ID, cc.NOMBRE AS CONCEPTO_CP, ");
                sql.Append("  dc.FOLIO, dc.FECHA, dc.CLAVE_PROV, dc.PROVEEDOR_ID, ");
                sql.Append("  dc.DESCRIPCION, ");
                sql.Append("  COALESCE(dc.FECHA_HORA_ULT_MODIF, dc.FECHA_HORA_CREACION) AS FECHA_HORA_ULT_MODIF, ");
                sql.Append("  dc.CANCELADO, dc.APLICADO, dc.TIENE_CFD ");
                sql.Append("FROM DOCTOS_CP dc ");
                sql.Append("JOIN CONCEPTOS_CP cc ON (dc.CONCEPTO_CP_ID = cc.CONCEPTO_CP_ID) ");
                if (esNotas)
                {
                    // SOLO NOTAS — validación "pago liberado en bancos", líneas
                    // VIVAS del Delphi Func_Calcula.pas:559-560:
                    //   LEFT JOIN doctos_entre_sis de ON(dc.docto_cp_id = de.docto_fte_id)
                    //   LEFT JOIN doctos_ba db ON(de.docto_dest_id = db.docto_ba_id)
                    // (En créditos las equivalentes 473-474 están comentadas/sin filtro.)
                    sql.Append("LEFT JOIN doctos_entre_sis de ON(dc.docto_cp_id = de.docto_fte_id) ");
                    sql.Append("LEFT JOIN doctos_ba db ON(de.docto_dest_id = db.docto_ba_id) ");
                }
                sql.Append("WHERE dc.NATURALEZA_CONCEPTO = 'R' ");
                sql.Append("  AND cc.TIPO = @cctipo ");
                if (esNotas)
                {
                    // Filtros vivos del Delphi Func_Calcula.pas:563-564 — excluyen
                    // notas cuyo cheque/transferencia destino en bancos NO está
                    // aplicado (db.aplicado <> 'S').
                    sql.Append("  AND (de.clave_sis_dest = 'BA' OR de.clave_sis_dest IS NULL) ");
                    sql.Append("  AND (db.aplicado = 'S' OR db.aplicado IS NULL) ");
                }

                if (desde.HasValue)
                {
                    sql.Append("  AND COALESCE(dc.FECHA_HORA_ULT_MODIF, dc.FECHA_HORA_CREACION) > @desde ");
                }
                else
                {
                    // Fallback: últimos 90 días para no descargar años en la
                    // carga inicial. El operador puede ampliar con EMP_SINC_DESDE.
                    sql.Append("  AND dc.FECHA >= DATEADD(-90 DAY TO CURRENT_DATE) ");
                }

                sql.Append("ORDER BY dc.DOCTO_CP_ID");

                using (var cmd = new FbCommand(sql.ToString(), con.FBC))
                {
                    cmd.Parameters.Add("@cctipo", FbDbType.VarChar).Value = ccTipo;
                    if (desde.HasValue)
                        cmd.Parameters.Add("@desde", FbDbType.TimeStamp).Value = desde.Value;

                    using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                    {
                        while (await rd.ReadAsync(ct).ConfigureAwait(false))
                        {
                            var fecha          = rd.IsDBNull(rd.GetOrdinal("FECHA"))                ? (DateTime?) null : (DateTime?) Convert.ToDateTime(rd["FECHA"]);
                            var fechaUltModif  = rd.IsDBNull(rd.GetOrdinal("FECHA_HORA_ULT_MODIF")) ? (DateTime?) null : (DateTime?) Convert.ToDateTime(rd["FECHA_HORA_ULT_MODIF"]);

                            lista.Add(new DoctoCpMicrosip
                            {
                                docto_cp_id          = Convert.ToInt32(rd["DOCTO_CP_ID"]),
                                concepto_cp_id       = Convert.ToInt32(rd["CONCEPTO_CP_ID"]),
                                concepto_cp          = (Convert.ToString(rd["CONCEPTO_CP"]) ?? "").Trim(),
                                folio                = (Convert.ToString(rd["FOLIO"])       ?? "").Trim(),
                                fecha                = fecha.HasValue ? fecha.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                                clave_prov           = (Convert.ToString(rd["CLAVE_PROV"]) ?? "").Trim(),
                                proveedor_id         = rd.IsDBNull(rd.GetOrdinal("PROVEEDOR_ID")) ? 0 : Convert.ToInt32(rd["PROVEEDOR_ID"]),
                                descripcion          = NormalizarDescripcion(Convert.ToString(rd["DESCRIPCION"])),
                                fecha_hora_ult_modif = fechaUltModif.HasValue ? fechaUltModif.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                                cancelado            = (Convert.ToString(rd["CANCELADO"]) ?? "N").Trim(),
                                aplicado             = (Convert.ToString(rd["APLICADO"])  ?? "S").Trim(),
                                tiene_cfd            = (Convert.ToString(rd["TIENE_CFD"]) ?? "N").Trim(),
                            });
                        }
                    }
                }

                // Detalle (CREDITOS_DET) — para AMBOS tipos:
                //   - Créditos: ACTUALIZA_CREDITOS_DET (Func_Creditos.pas:135-307).
                //   - Notas:    ACTUALIZA_NOTAS_DET (Func_Notas.pas:135-307) — el
                //     Delphi TAMBIÉN insertaba el detalle para notas (invocado en
                //     Func_Notas.pas:437 y 466), con el MISMO SELECT de
                //     importes_doctos_cp y el MISMO filtro PPD versión 3.3/4.0
                //     (Func_Notas.pas:238-240).
                if (lista.Count > 0)
                {
                    await CargarDetalleAsync(con.FBC, lista, ct).ConfigureAwait(false);

                    // Cancelaciones (CANCELADO='S') y finalizaciones (TIENE_CFD='S'):
                    // en el Delphi son UPDATEs de cabecera que NO tocan el detalle
                    // (Func_Creditos.pas:479-507 y Func_Notas.pas:479-507, fuera
                    // del branch que llama ACTUALIZA_*_DET). detalle=null para que
                    // el endpoint PHP no haga DELETE+INSERT de CREDITOS_DET por
                    // estas filas (el modelo solo sincroniza detalle si el campo
                    // viene como array).
                    foreach (var c in lista)
                    {
                        if (EsCancelacionOFinalizacion(c)) c.detalle = null;
                    }

                    // VALIDA_COMPLEMENTO activado — SOLO créditos (cc.tipo='P'):
                    // descartamos cobros PENDIENTES sin detalle PPD. Es lo que el
                    // Delphi intentó hacer en Func_Creditos.pas:384 y 451 (la
                    // llamada a VALIDA_COMPLEMENTO está comentada en producción,
                    // pero semánticamente es esto: solo subir cobros que
                    // realmente requieren complemento de pago).
                    //
                    // IMPORTANTE: NO se descartan cancelados ni finalizados — su
                    // única misión es actualizar el ESTATUS en el portal y el
                    // Delphi mandaba esas transiciones sin condicionarlas al
                    // detalle (Func_Creditos.pas:479-507). Si se descartaran, un
                    // cobro cancelado cuyo IMPORTES_DOCTOS_CP quedó vacío jamás
                    // pasaría a ESTATUS='C' y quedaría pendiente eternamente.
                    if (string.Equals(ccTipo, "P", StringComparison.OrdinalIgnoreCase))
                    {
                        int antes = lista.Count;
                        lista.RemoveAll(c => !EsCancelacionOFinalizacion(c)
                                          && (c.detalle == null || c.detalle.Length == 0));
                        int omitidos = antes - lista.Count;
                        if (omitidos > 0)
                        {
                            EventoLog.Info("DoctosCpRepository('" + nombreEmpresa
                                + "'): " + omitidos + " cobro(s) pendiente(s) sin detalle PPD omitido(s) "
                                + "(probablemente PUE u otro método). Quedan "
                                + lista.Count + " para sincronizar.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventoLog.Error("DoctosCpRepository('" + nombreEmpresa + "', tipo=" + ccTipo + "): " + ex.Message);
            }
            finally
            {
                con.Desconectar();
            }

            return lista;
        }

        /// <summary>
        /// True si la fila representa una transición de cancelación
        /// (CANCELADO='S') o de finalización (TIENE_CFD='S'). En el Delphi
        /// estas transiciones son UPDATEs de cabecera independientes del
        /// detalle (Func_Creditos.pas:479-507 / Func_Notas.pas:479-507).
        /// </summary>
        private static bool EsCancelacionOFinalizacion(DoctoCpMicrosip c)
        {
            return string.Equals(c.cancelado, "S", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.tiene_cfd, "S", StringComparison.OrdinalIgnoreCase);
        }

        // ====================================================================
        // Detalle: IMPORTES_DOCTOS_CP + lookup CFDI por documento acreditado
        // ====================================================================

        /// <summary>
        /// Por cada documento (crédito o nota) carga su array
        /// <see cref="DoctoCpMicrosip.detalle"/> con los renglones
        /// (IMPORTES_DOCTOS_CP) y el CFDI asociado a cada documento acreditado.
        /// Solo conserva renglones cuyo CFDI tiene <c>MetodoPago='PPD'</c> —
        /// réplica del filtro del Delphi en Func_Creditos.pas:240 (créditos) y
        /// Func_Notas.pas:238-240 (notas; mismo filtro en ambos).
        /// </summary>
        private static async Task CargarDetalleAsync(FbConnection con, List<DoctoCpMicrosip> creditos, CancellationToken ct)
        {
            // 1) Leemos TODOS los importes de TODOS los créditos en un solo SELECT.
            //    Esto evita N+1 por crédito.
            var doctosIds = new List<int>(creditos.Count);
            foreach (var c in creditos) doctosIds.Add(c.docto_cp_id);

            var importesPorCredito = await LeerImportesBatchAsync(con, doctosIds, ct).ConfigureAwait(false);

            // 2) Para cada importe necesitamos su CFDI. La consulta varía según
            //    SISTEMA_ORIGEN del documento acreditado. Procesamos uno por uno
            //    porque el JOIN es distinto (es lo que hace el Delphi).
            foreach (var c in creditos)
            {
                List<RenglonImporte> renglones;
                if (!importesPorCredito.TryGetValue(c.docto_cp_id, out renglones))
                {
                    c.detalle = new CreditoDetMicrosip[0];
                    continue;
                }

                var detallesPpd = new List<CreditoDetMicrosip>(renglones.Count);
                foreach (var r in renglones)
                {
                    ct.ThrowIfCancellationRequested();

                    var cfdi = await LeerCfdiDocumentoAcreditadoAsync(con, r.DoctoCpAcrId, r.SistemaOrigen, ct).ConfigureAwait(false);
                    if (cfdi == null) continue;

                    if (!XmlEsPPD(cfdi.Xml)) continue;

                    detallesPpd.Add(new CreditoDetMicrosip
                    {
                        impte_docto_cp_id = r.ImpteDoctoCpId,
                        docto_cp_id       = r.DoctoCpId,
                        docto_cp_acr_id   = r.DoctoCpAcrId,
                        importe           = r.Importe,
                        impuesto          = r.Impuesto,
                        iva_retenido      = r.IvaRetenido,
                        isr_retenido      = r.IsrRetenido,
                        folio_acr         = r.FolioAcr,
                        descripcion       = r.Descripcion,
                        uuid              = cfdi.Uuid,
                        fecha             = cfdi.Fecha.ToString("yyyy-MM-dd"),
                    });
                }
                c.detalle = detallesPpd.ToArray();
            }
        }

        /// <summary>
        /// Lee en batch los IMPORTES_DOCTOS_CP de todos los créditos pasados.
        /// Trae también FOLIO_ACR, DESCRIPCION y SISTEMA_ORIGEN del documento
        /// acreditado para evitar un round-trip extra.
        /// </summary>
        private static async Task<Dictionary<int, List<RenglonImporte>>> LeerImportesBatchAsync(
            FbConnection con, List<int> doctosIds, CancellationToken ct)
        {
            var porCredito = new Dictionary<int, List<RenglonImporte>>(doctosIds.Count);
            if (doctosIds.Count == 0) return porCredito;

            const int chunkSize = 1000;
            for (int offset = 0; offset < doctosIds.Count; offset += chunkSize)
            {
                int count = Math.Min(chunkSize, doctosIds.Count - offset);
                await LeerImportesChunkAsync(con, doctosIds, offset, count, porCredito, ct).ConfigureAwait(false);
            }

            return porCredito;
        }

        private static async Task LeerImportesChunkAsync(
            FbConnection con,
            List<int> doctosIds, int offset, int count,
            Dictionary<int, List<RenglonImporte>> destino,
            CancellationToken ct)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT id.IMPTE_DOCTO_CP_ID, id.DOCTO_CP_ID, id.DOCTO_CP_ACR_ID, ");
            sb.Append("       id.IMPORTE, id.IMPUESTO, id.IVA_RETENIDO, id.ISR_RETENIDO, ");
            sb.Append("       dc.FOLIO AS FOLIO_ACR, dc.DESCRIPCION, dc.SISTEMA_ORIGEN ");
            sb.Append("FROM IMPORTES_DOCTOS_CP id ");
            sb.Append("JOIN DOCTOS_CP dc ON (id.DOCTO_CP_ACR_ID = dc.DOCTO_CP_ID) ");
            sb.Append("WHERE id.DOCTO_CP_ID IN (");
            for (int i = 0; i < count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append('@').Append('d').Append(i);
            }
            sb.Append(')');

            using (var cmd = new FbCommand(sb.ToString(), con))
            {
                for (int i = 0; i < count; i++)
                    cmd.Parameters.Add("@d" + i, FbDbType.Integer).Value = doctosIds[offset + i];

                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var doctoCpId = Convert.ToInt32(rd["DOCTO_CP_ID"]);
                        List<RenglonImporte> lista;
                        if (!destino.TryGetValue(doctoCpId, out lista))
                        {
                            lista = new List<RenglonImporte>();
                            destino[doctoCpId] = lista;
                        }

                        lista.Add(new RenglonImporte
                        {
                            ImpteDoctoCpId = Convert.ToInt32(rd["IMPTE_DOCTO_CP_ID"]),
                            DoctoCpId      = doctoCpId,
                            DoctoCpAcrId   = Convert.ToInt32(rd["DOCTO_CP_ACR_ID"]),
                            Importe        = rd.IsDBNull(rd.GetOrdinal("IMPORTE"))      ? 0m : Convert.ToDecimal(rd["IMPORTE"]),
                            Impuesto       = rd.IsDBNull(rd.GetOrdinal("IMPUESTO"))     ? 0m : Convert.ToDecimal(rd["IMPUESTO"]),
                            IvaRetenido    = rd.IsDBNull(rd.GetOrdinal("IVA_RETENIDO")) ? 0m : Convert.ToDecimal(rd["IVA_RETENIDO"]),
                            IsrRetenido    = rd.IsDBNull(rd.GetOrdinal("ISR_RETENIDO")) ? 0m : Convert.ToDecimal(rd["ISR_RETENIDO"]),
                            FolioAcr       = (Convert.ToString(rd["FOLIO_ACR"]) ?? "").Trim(),
                            Descripcion    = NormalizarDescripcion(Convert.ToString(rd["DESCRIPCION"])),
                            SistemaOrigen  = (Convert.ToString(rd["SISTEMA_ORIGEN"]) ?? "").Trim(),
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Lee el CFDI (XML + UUID + FECHA) del documento acreditado. El SELECT
        /// cambia según SISTEMA_ORIGEN (CM o cualquier otro), igual que el
        /// Delphi (Func_Creditos.pas:200-222).
        /// </summary>
        private static async Task<CfdiAcreditado> LeerCfdiDocumentoAcreditadoAsync(
            FbConnection con, int doctoCpAcrId, string sistemaOrigen, CancellationToken ct)
        {
            string sql;
            if (string.Equals(sistemaOrigen, "CM", StringComparison.OrdinalIgnoreCase))
            {
                // El documento acreditado es un cargo CP que vino de una compra
                // CM. El CFDI vive en CFD_RECIBIDOS del documento CM origen, que
                // se obtiene siguiendo DOCTOS_ENTRE_SIS (DEST=el CP, FTE=el CM).
                sql = "SELECT cr.XML, rc.UUID, rc.FECHA " +
                      "  FROM DOCTOS_ENTRE_SIS ds " +
                      "  JOIN CFD_RECIBIDOS cr ON (ds.DOCTO_FTE_ID = cr.DOCTO_ID) " +
                      "  JOIN REPOSITORIO_CFDI rc ON (cr.CFDI_ID = rc.CFDI_ID) " +
                      " WHERE ds.DOCTO_DEST_ID = @docto " +
                      "   AND cr.CLAVE_SISTEMA = @sis";
            }
            else
            {
                // El documento acreditado es un cargo CP directo (no vino de
                // una compra). El CFDI está en CFD_RECIBIDOS del propio
                // documento CP.
                sql = "SELECT cr.XML, rc.UUID, rc.FECHA " +
                      "  FROM CFD_RECIBIDOS cr " +
                      "  JOIN REPOSITORIO_CFDI rc ON (cr.CFDI_ID = rc.CFDI_ID) " +
                      " WHERE cr.DOCTO_ID = @docto " +
                      "   AND cr.CLAVE_SISTEMA = @sis";
            }

            using (var cmd = new FbCommand(sql, con))
            {
                cmd.Parameters.Add("@docto", FbDbType.Integer).Value = doctoCpAcrId;
                cmd.Parameters.Add("@sis",   FbDbType.VarChar).Value = sistemaOrigen ?? "";
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (!await rd.ReadAsync(ct).ConfigureAwait(false)) return null;

                    return new CfdiAcreditado
                    {
                        Xml   = Convert.ToString(rd["XML"]) ?? "",
                        Uuid  = (Convert.ToString(rd["UUID"]) ?? "").Trim(),
                        Fecha = rd.IsDBNull(rd.GetOrdinal("FECHA")) ? DateTime.MinValue : Convert.ToDateTime(rd["FECHA"]),
                    };
                }
            }
        }

        /// <summary>
        /// Parsea el XML del CFDI y devuelve true si <c>cfdi:Comprobante[@MetodoPago='PPD']</c>
        /// para versiones 3.3 o 4.0. Mismo criterio del Delphi
        /// (Func_Creditos.pas:238-242). Cualquier error de parseo se trata como
        /// "no es PPD" — el detalle no se incluye, no abortamos el sync.
        /// </summary>
        private static bool XmlEsPPD(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return false;

            try
            {
                // CleanXml: el XML almacenado puede traer BOM corrupto u otros
                // restos. XDocument.Parse es estricto, así que limpiamos antes.
                var limpio = EncoderXmlMicrosip.LimpiarBom(xml);

                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
                using (var sr = new System.IO.StringReader(limpio))
                using (var xr = XmlReader.Create(sr, settings))
                {
                    var doc = XDocument.Load(xr);
                    var root = doc.Root;
                    if (root == null) return false;

                    var version    = (string)root.Attribute("Version");
                    var metodoPago = (string)root.Attribute("MetodoPago");

                    if (string.IsNullOrEmpty(metodoPago)) return false;
                    if (!string.Equals(metodoPago, "PPD", StringComparison.OrdinalIgnoreCase)) return false;

                    // Versión 3.3 o 4.0 (mismas que filtra el Delphi).
                    if (string.IsNullOrEmpty(version)) return false;
                    return version == "3.3" || version == "4.0";
                }
            }
            catch
            {
                return false;
            }
        }

        private sealed class RenglonImporte
        {
            public int     ImpteDoctoCpId { get; set; }
            public int     DoctoCpId      { get; set; }
            public int     DoctoCpAcrId   { get; set; }
            public decimal Importe        { get; set; }
            public decimal Impuesto       { get; set; }
            public decimal IvaRetenido    { get; set; }
            public decimal IsrRetenido    { get; set; }
            public string  FolioAcr       { get; set; }
            public string  Descripcion    { get; set; }
            public string  SistemaOrigen  { get; set; }
        }

        private sealed class CfdiAcreditado
        {
            public string   Xml   { get; set; }
            public string   Uuid  { get; set; }
            public DateTime Fecha { get; set; }
        }

        /// <summary>
        /// Quita las comillas dobles de la descripción para que no rompan
        /// nada en JSON / SQL downstream — mismo patrón que el Delphi
        /// (StringReplace de '"' por '' en Func_Calcula.pas:497).
        /// </summary>
        private static string NormalizarDescripcion(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            return raw.Replace("\"", "").Trim();
        }
    }
}
