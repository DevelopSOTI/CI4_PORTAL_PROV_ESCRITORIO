namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Empresa Microsip leída del DAO. Forma de transporte hacia el portal:
    /// los nombres de las propiedades respetan el JSON que espera el endpoint
    /// CI4 (snake_case), no la nomenclatura interna de Microsip.
    /// </summary>
    public sealed class EmpresaMicrosip
    {
        public int empresa_id { get; set; }
        public string nombre_corto { get; set; }
        public string nombre { get; set; }
        public string rfc { get; set; }
        public string fecha_hora_ult_modif { get; set; }
    }
}
