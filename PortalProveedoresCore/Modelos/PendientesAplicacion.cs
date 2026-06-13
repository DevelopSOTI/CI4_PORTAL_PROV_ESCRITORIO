namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Resumen de qué hay pendiente de aplicar a Microsip para UNA empresa
    /// específica. Lo devuelve el endpoint <c>GET /api/aplicacion/pendientes</c>.
    /// El servicio Windows lo consume una vez por empresa por ciclo para
    /// decidir si tiene trabajo y, junto con APLICA_DIR, si debe aplicarlo
    /// o solo loguear.
    /// </summary>
    public sealed class PendientesAplicacion
    {
        public int emp_id_msp              { get; set; }
        public int facturas_pendientes     { get; set; }
        public int complementos_pendientes { get; set; }

        /// <summary>Lista breve de facturas pendientes (folio, recepción, UUID, proveedor, fecha).</summary>
        public FacturaPendiente[] facturas { get; set; }

        /// <summary>Lista breve de complementos pendientes (folio, credito_fk, UUID, proveedor, fecha).</summary>
        public ComplementoPendiente[] complementos { get; set; }
    }

    /// <summary>
    /// Factura pendiente de aplicar en Microsip. Patrón del Delphi:
    /// <c>ESTATUS='S'</c> (subida por el proveedor) y <c>RECEPCION_ID != 0</c>
    /// (asociada a una recepción Microsip).
    /// </summary>
    public sealed class FacturaPendiente
    {
        public string FOLIO         { get; set; }
        public int    RECEPCION_ID  { get; set; }
        public string UUID          { get; set; }
        public int    PROVEEDOR_ID  { get; set; }
        public string FECHA_PAGO    { get; set; }
    }

    /// <summary>
    /// Complemento de pago pendiente de aplicar en Microsip. Patrón del
    /// Delphi: <c>ESTATUS='S'</c> y crédito NO cancelado.
    /// </summary>
    public sealed class ComplementoPendiente
    {
        public string FOLIO         { get; set; }
        public int    CREDITO_FK    { get; set; }
        public string UUID          { get; set; }
        public int    PROVEEDOR_ID  { get; set; }
        public string FECHA_PAGO    { get; set; }
    }
}
