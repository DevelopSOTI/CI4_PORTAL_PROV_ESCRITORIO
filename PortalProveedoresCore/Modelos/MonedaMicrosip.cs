namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Moneda leída del Firebird de una empresa Microsip. Los nombres de las
    /// propiedades respetan el JSON que espera el endpoint CI4 (snake_case),
    /// no la nomenclatura interna de Microsip — así no hay que mapear con
    /// atributos en la serialización.
    ///
    /// Mismas tres columnas que el Delphi histórico envía a MONEDAS_MSP:
    /// MONEDA_ID, NOMBRE, CLAVE_FISCAL, FECHA_HORA_ULT_MODIF
    /// (ver Func_Catalogos.pas → ACTUALIZA_MONEDAS).
    /// </summary>
    public sealed class MonedaMicrosip
    {
        public int    moneda_id            { get; set; }
        public string nombre               { get; set; }
        public string clave_fiscal         { get; set; }
        public string fecha_hora_ult_modif { get; set; }
    }
}
