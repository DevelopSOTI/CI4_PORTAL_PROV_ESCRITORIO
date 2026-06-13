namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Almacén leído del Firebird de una empresa Microsip. Los nombres de las
    /// propiedades respetan el JSON que espera el endpoint CI4 (snake_case),
    /// no la nomenclatura interna de Microsip — así no hay que mapear con
    /// atributos en la serialización.
    /// </summary>
    public sealed class AlmacenMicrosip
    {
        public int    almacen_id           { get; set; }
        public string nombre               { get; set; }
        public string nombre_abrev         { get; set; }
        public string fecha_hora_ult_modif { get; set; }
    }
}
