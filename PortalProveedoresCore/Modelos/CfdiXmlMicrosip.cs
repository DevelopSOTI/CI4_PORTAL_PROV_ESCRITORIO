namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// XML del CFDI tal como lo guardó el proveedor en el portal, junto con
    /// los dos campos que Microsip necesita en <c>REPOSITORIO_CFDI</c>
    /// (LUGAR_EXPEDICION y USO_CFDI). Lo devuelve
    /// <c>GET /api/aplicacion/cfdi-xml?uuid=&amp;tipo=</c>.
    ///
    /// El XML viene aquí como texto UTF-8; antes de insertarlo en Microsip
    /// hay que pasarlo por <c>EncoderXmlMicrosip.PrepararParaMicrosip</c>
    /// (UTF-8 → ISO-8859-1 → ASCII), exactamente como hace el Delphi
    /// (Func_Facturas_3_3.pas:931-934).
    /// </summary>
    public sealed class CfdiXmlMicrosip
    {
        public string uuid             { get; set; }
        public string tipo             { get; set; }   // 'F' factura, 'C' complemento
        public string xml              { get; set; }
        public string uso_cfdi         { get; set; }
        public string lugar_expedicion { get; set; }
    }
}
