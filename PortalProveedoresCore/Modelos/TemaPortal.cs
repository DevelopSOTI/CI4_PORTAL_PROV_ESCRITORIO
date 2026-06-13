namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Configuración visual del portal (tabla PORTAL_CONFIG), expuesta para
    /// que las apps de escritorio se pinten con la marca de cada cliente:
    /// paleta de colores en formato CSS hexadecimal, nombre del portal y
    /// URL absoluta del logo.
    ///
    /// Los nombres en snake_case coinciden 1:1 con el JSON que devuelve
    /// GET /api/portal-config.
    /// </summary>
    public sealed class TemaPortal
    {
        public string nombre              { get; set; }
        public string color_primary       { get; set; }
        public string color_primary_hover { get; set; }
        public string color_secondary     { get; set; }
        public string color_accent        { get; set; }
        public string logo_url            { get; set; }
    }
}
