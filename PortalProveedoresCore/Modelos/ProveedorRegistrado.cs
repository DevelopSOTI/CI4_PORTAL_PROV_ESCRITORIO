namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Fila del catálogo "Proveedores registrados" que devuelve
    /// <c>GET /api/escritorio/proveedores-registrados</c>. Réplica del DTO
    /// que el SOAP legacy <c>F_PROVEEDORES</c> traía vía
    /// <c>ws.GETProveedoresRegistrados</c> contra la tabla <c>ACCESO</c>.
    ///
    /// Campos:
    /// <list type="bullet">
    ///   <item><c>proveedor_id</c> — PK del proveedor en el portal (ACCESO.PROVEEDOR_ID).</item>
    ///   <item><c>proveedor_id_msp</c> — referencia al PROVEEDOR_ID_MSP de Microsip.</item>
    ///   <item><c>usuario</c> — login del proveedor en el portal.</item>
    ///   <item><c>razon_social</c> — nombre legal del proveedor.</item>
    ///   <item><c>correo</c> — correo registrado para notificaciones.</item>
    ///   <item><c>estatus</c> — 'A' activo, 'B' bloqueado, etc.</item>
    /// </list>
    /// </summary>
    public sealed class ProveedorRegistrado
    {
        public int    proveedor_id     { get; set; }
        public int    proveedor_id_msp { get; set; }
        public string usuario          { get; set; }
        public string razon_social     { get; set; }
        public string correo           { get; set; }
        public string estatus          { get; set; }
    }

    public sealed class RespuestaProveedoresRegistrados
    {
        public int                    emp_id_msp  { get; set; }
        public int                    total       { get; set; }
        public ProveedorRegistrado[]  proveedores { get; set; }
    }
}
