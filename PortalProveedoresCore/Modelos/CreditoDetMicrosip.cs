namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Renglón del detalle de un crédito (cobro PPD) sincronizado desde
    /// Microsip al portal. Representa UNA factura/cargo del proveedor que
    /// está siendo pagada por el crédito, junto con su CFDI asociado.
    ///
    /// Se construye a partir de:
    /// <list type="bullet">
    ///   <item><c>IMPORTES_DOCTOS_CP</c> en Microsip (los renglones del cobro)</item>
    ///   <item><c>DOCTOS_CP</c> del documento acreditado (FOLIO, DESCRIPCION, SISTEMA_ORIGEN)</item>
    ///   <item><c>CFD_RECIBIDOS</c> + <c>REPOSITORIO_CFDI</c> (XML + UUID + FECHA del CFDI)</item>
    /// </list>
    ///
    /// El portal SOLO recibe renglones cuyo CFDI tiene <c>MetodoPago='PPD'</c>
    /// (parcialidades), réplica del filtro del Delphi en
    /// <c>Func_Creditos.pas:240</c>. Renglones con PUE u otros métodos no se
    /// envían porque no aplica el concepto de complemento de pago.
    /// </summary>
    public sealed class CreditoDetMicrosip
    {
        /// <summary>id.IMPTE_DOCTO_CP_ID — PK de IMPORTES_DOCTOS_CP en Microsip.</summary>
        public int     impte_docto_cp_id { get; set; }

        /// <summary>id.DOCTO_CP_ID — el del crédito (mismo que la cabecera DoctoCpMicrosip.docto_cp_id).</summary>
        public int     docto_cp_id       { get; set; }

        /// <summary>id.DOCTO_CP_ACR_ID — el documento acreditado (cargo en CP del proveedor).</summary>
        public int     docto_cp_acr_id   { get; set; }

        public decimal importe           { get; set; }
        public decimal impuesto          { get; set; }
        public decimal iva_retenido      { get; set; }
        public decimal isr_retenido      { get; set; }

        /// <summary>dc.FOLIO del documento acreditado.</summary>
        public string  folio_acr         { get; set; }
        public string  descripcion       { get; set; }

        /// <summary>UUID del CFDI del documento acreditado.</summary>
        public string  uuid              { get; set; }

        /// <summary>Fecha del CFDI ("yyyy-MM-dd").</summary>
        public string  fecha             { get; set; }
    }
}
