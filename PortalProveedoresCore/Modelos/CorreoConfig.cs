namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Configuración SMTP del portal, tal como la devuelve
    /// <c>GET /api/aplicacion/correo-config</c> (tabla MAIL del portal CI4).
    /// La usa el Escritorio para enviar el correo de rechazo al proveedor.
    /// </summary>
    public sealed class CorreoConfig
    {
        public string smtp { get; set; }
        public int    port { get; set; }
        public string from { get; set; }
        public string pass { get; set; }
        public string name { get; set; }
    }
}
