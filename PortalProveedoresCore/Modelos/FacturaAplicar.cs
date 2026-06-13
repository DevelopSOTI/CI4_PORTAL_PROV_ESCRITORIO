namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Vista completa de una factura del portal lista para aplicar en
    /// Microsip. La devuelve <c>GET /api/aplicacion/facturas-aplicar?emp_id_msp=N</c>
    /// y representa el shape exacto del SELECT del Delphi
    /// (Func_Calcula.pas:685-720):
    ///
    /// <code>
    /// SELECT F.DOCTO_CM_ID, F.FOLIO AS FOLIO_COMPRA, F.IMPORTE_NETO, F.TOTAL_IMPUESTOS,
    ///        F.TOTAL_RETENCIONES, F.DESCUENTO_GLOBAL, F.MONEDA_SIMBOLO, F.TIPO_CAMBIO,
    ///        F.RECEPCION_ID, F.RECEP_ID, C.FOLIO AS FOLIO_RECEPCION,
    ///        F.FECHA_PAGO, F.FECHA_FACTURA, F.FECHA_RECEPCION, C.FECHA,
    ///        F.PROVEEDOR_FK AS PROVEEDOR_ID, F.RFC, P.NOMBRE, F.UUID
    ///   FROM FACTURA_PROVEEDOR_33 F
    ///  INNER JOIN PROVEEDORES_MSP P ON ...
    ///  INNER JOIN ALMACENES_MSP   A ON ...
    ///  INNER JOIN RECEPCIONES     C ON ...
    ///  WHERE F.EMP_FK = ? AND F.ESTATUS = 'S' AND C.ESTATUS &lt;&gt; 'C' AND F.RECEPCION_ID &lt;&gt; 0
    ///  ORDER BY P.NOMBRE, F.FECHA_PAGO ASC
    /// </code>
    ///
    /// Las fechas viajan como string <c>YYYY-MM-DD HH:MM:SS</c> tal como salen
    /// de MySQL; el sincronizador las parsea con <see cref="System.DateTime.Parse"/>
    /// invariante para insertar en Firebird.
    ///
    /// NOTA: los nombres de propiedad están en MAYÚSCULAS para que coincidan
    /// con el JSON exacto que devuelve el portal y con la nomenclatura del
    /// Delphi (que usa MAYÚSCULAS en variables locales). Esto facilita
    /// rastrear bug reports entre los dos sistemas — un grep por DOCTO_CM_ID
    /// encuentra la línea tanto en Pascal como en C#.
    /// </summary>
    public sealed class FacturaAplicar
    {
        // === Identidad MySQL ===
        public int    DOCTO_CM_ID       { get; set; }   // F.DOCTO_CM_ID — id en MySQL (no en Microsip)
        public int    RECEP_ID          { get; set; }   // F.RECEP_ID — id de la recepción en MySQL
        public int    RECEPCION_ID      { get; set; }   // F.RECEPCION_ID — id de la recepción en Microsip
        public string UUID              { get; set; }   // CFDI UUID

        // === Folios ===
        public string FOLIO_COMPRA      { get; set; }   // F.FOLIO — folio que asignó el proveedor a su factura
        public string FOLIO_RECEPCION   { get; set; }   // C.FOLIO — folio de la recepción Microsip (ej. RM7230)

        // === Almacén (para flujo SIN recepción — el DOCTOS_CM lo necesita) ===
        public int    ALMACEN_FK_MSP    { get; set; }   // F.ALMACEN_FK_MSP — ALMACEN_ID en Microsip

        // === Totales (la suma efectiva del CFDI) ===
        public decimal IMPORTE_NETO       { get; set; }
        public decimal TOTAL_IMPUESTOS    { get; set; }
        public decimal TOTAL_RETENCIONES  { get; set; }
        public decimal DESCUENTO_GLOBAL   { get; set; }
        public string  MONEDA_SIMBOLO     { get; set; }   // ej. "MXN", "USD"
        public int     MONEDA_ID          { get; set; }   // F.MONEDA_ID — MONEDA_ID Microsip que el portal ya resolvió
        public decimal TIPO_CAMBIO        { get; set; }

        // === Fechas (todas en formato "YYYY-MM-DD HH:MM:SS") ===
        public string FECHA_PAGO          { get; set; }   // F.FECHA_PAGO
        public string FECHA_FACTURA       { get; set; }   // F.FECHA_FACTURA — fecha del CFDI
        public string FECHA_RECEPCION     { get; set; }   // F.FECHA_RECEPCION — cuando se recibió la mercancía
        public string FECHA               { get; set; }   // C.FECHA de la recepción Microsip

        // === Proveedor (datos del CFDI) ===
        public int    PROVEEDOR_ID        { get; set; }   // F.PROVEEDOR_FK = P.PROVEEDOR_ID_MSP
        public string RFC                 { get; set; }   // F.RFC
        public string NOMBRE              { get; set; }   // P.NOMBRE del proveedor
    }
}
