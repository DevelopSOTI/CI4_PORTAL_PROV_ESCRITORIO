namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Catálogos de proveedores y almacenes de una empresa, usados por la
    /// UI del Escritorio para alimentar los ComboBox AutoComplete de
    /// "Buscar por proveedor / almacén" en las vistas de Facturas y
    /// Complementos. Lo devuelve
    /// <c>GET /api/escritorio/catalogos-filtros?emp_id_msp=N</c>.
    /// </summary>
    public sealed class CatalogoFiltroItem
    {
        public int    id     { get; set; }
        public string nombre { get; set; }

        public override string ToString() => nombre ?? "";
    }

    public sealed class RespuestaCatalogosFiltros
    {
        public int                   emp_id_msp  { get; set; }
        public CatalogoFiltroItem[]  proveedores { get; set; }
        public CatalogoFiltroItem[]  almacenes   { get; set; }
    }
}
