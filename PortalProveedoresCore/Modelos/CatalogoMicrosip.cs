namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Condición de pago de Microsip de COMPRAS/cuentas por pagar (tabla
    /// CONDICIONES_PAGO_CP de Firebird). Se usa en el combo "Condiciones de
    /// pago" del modal de aplicar.
    /// </summary>
    public sealed class CondicionPagoMicrosip
    {
        public int    Id     { get; set; }
        public string Nombre { get; set; }

        public override string ToString() => Nombre ?? "";
    }

    /// <summary>
    /// Artículo del catálogo de Microsip (tabla ARTICULOS). Solo expone los
    /// campos que el operador necesita ver/elegir en F_BUSQUEDA: id, clave
    /// y nombre. <c>EsAlmacenable</c> se incluye para filtrar — los artículos
    /// "generales" usados en facturas del portal son NO almacenables.
    /// </summary>
    public sealed class ArticuloMicrosip
    {
        public int    Id            { get; set; }
        public string Clave         { get; set; }
        public string Nombre        { get; set; }
        public bool   EsAlmacenable { get; set; }

        public override string ToString() => Nombre ?? "";
    }
}
