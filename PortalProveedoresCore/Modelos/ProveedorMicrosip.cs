namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Proveedor leído del Firebird de una empresa Microsip, ya con las 3 tablas
    /// JOIN-eadas (proveedores + claves_proveedores + libres_proveedor). Los
    /// nombres de las propiedades respetan el JSON que espera el endpoint CI4
    /// (snake_case) y el contrato del Delphi histórico (ACTUALIZA_PROVEEDORES,
    /// Func_Catalogos.pas:268-411).
    ///
    /// Mapeo S/N → SI/NO:
    ///   Microsip almacena en libres_proveedor.{permitir_sin_recepcion,
    ///   adjuntar_archivos} un solo carácter 'S' o 'N'. La tabla MySQL
    ///   PROVEEDORES_MSP.{PROV_PRIV, ADJUNTAR_ARCHIVOS} es varchar(2) y guarda
    ///   "SI" o "NO" — herencia del Delphi original. El repositorio C# hace
    ///   esa conversión ANTES de poblar el DTO, así que aquí ya viene "SI"/"NO".
    /// </summary>
    public sealed class ProveedorMicrosip
    {
        public int     proveedor_id          { get; set; }
        public string  nombre                { get; set; }
        public string  estatus               { get; set; }   // un solo carácter, ej. "A"
        public string  clave_prov            { get; set; }
        public string  fecha_hora_ult_modif  { get; set; }   // ISO "YYYY-MM-DD HH:mm:ss"
        public string  rfc                   { get; set; }
        public string  prov_priv             { get; set; }   // "SI" | "NO"
        public decimal pctje_rechazo         { get; set; }
        public string  referencia            { get; set; }
        public string  adjuntar_archivos     { get; set; }   // "SI" | "NO"
    }
}
