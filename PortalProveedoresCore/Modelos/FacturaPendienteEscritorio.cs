using System;

namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Vista de una factura del portal para mostrar en el listado del
    /// Escritorio. Mismas filas que <see cref="FacturaAplicar"/> pero con
    /// nombres de proveedor y almacén ya resueltos (joins en el endpoint)
    /// para no hacer N+1 desde la UI.
    ///
    /// La devuelve <c>GET /api/escritorio/facturas-pendientes</c>.
    /// </summary>
    public sealed class FacturaPendienteEscritorio
    {
        public int    DOCTO_CM_ID       { get; set; }
        public string FOLIO_PROV        { get; set; }
        public string UUID              { get; set; }
        public string RFC               { get; set; }
        public string PROVEEDOR_NOMBRE  { get; set; }
        public int    PROVEEDOR_ID      { get; set; }
        public decimal IMPORTE_NETO     { get; set; }
        public decimal TOTAL_IMPUESTOS  { get; set; }
        public decimal TOTAL_RETENCIONES{ get; set; }
        public decimal DESCUENTO_GLOBAL { get; set; }
        public decimal TOTAL            { get; set; }
        public string MONEDA_SIMBOLO    { get; set; }
        public decimal TIPO_CAMBIO      { get; set; }
        public string FECHA_FACTURA     { get; set; }
        public string FECHA_RECEPCION   { get; set; }
        public string FECHA_PAGO        { get; set; }
        public int    ALMACEN_ID        { get; set; }
        public string ALMACEN_NOMBRE    { get; set; }
        public int    RECEP_ID          { get; set; }
        public string FOLIO_RECEPCION   { get; set; }
        public int    RECEPCION_ID      { get; set; }
        public string ESTATUS           { get; set; }
    }

    /// <summary>
    /// Filtros que envía la UI al endpoint. Todos opcionales — al construir
    /// el query string se omiten los que tengan valor por defecto.
    /// </summary>
    public sealed class FiltroFacturasEscritorio
    {
        public int       EmpIdMsp     { get; set; }
        public int       ProveedorId  { get; set; }     // 0 = todos
        public int       AlmacenId    { get; set; }     // 0 = todos
        public string    NombreProveedor { get; set; } // LIKE %X% — vacío = sin filtro
        public string    NombreAlmacen   { get; set; } // LIKE %X% — vacío = sin filtro
        public DateTime? Desde        { get; set; }     // null = sin límite inferior
        public DateTime? Hasta        { get; set; }     // null = sin límite superior
        public bool      SoloPorVencer{ get; set; }     // FECHA_PAGO ≤ HOY + 7 días
        public int       Limit        { get; set; } = 100;

        /// <summary>
        /// true → el endpoint NO filtra por ESTATUS (incluye aplicadas,
        /// rechazadas y canceladas) y el rango Desde/Hasta aplica sobre
        /// FECHA_FACTURA — réplica del SOAP <c>SelectDescargar</c>
        /// (services/facturas.php:356-410) que usaba el tab Descargar
        /// (F_DESCARGAR.cs:152-165) para auditar lo ya procesado.
        /// false (default) → solo pendientes ESTATUS='S' (comportamiento
        /// histórico del endpoint).
        /// </summary>
        public bool      TodosEstatus { get; set; }
    }

    /// <summary>
    /// Respuesta completa del endpoint <c>facturas-pendientes</c>. Incluye el
    /// listado, el total que matchearon (sin LIMIT) y el flag
    /// <see cref="aplica_dir"/> del portal — la UI lo usa para mostrar un
    /// mensaje claro al operador explicando por qué solo ve cierto subset
    /// (en modo automático no aparecen las que el servicio aplicará solo).
    /// </summary>
    public sealed class RespuestaFacturasEscritorio
    {
        public int                          emp_id_msp { get; set; }
        public int                          total      { get; set; }
        public bool                         aplica_dir { get; set; }
        public FacturaPendienteEscritorio[] facturas   { get; set; }
    }
}
