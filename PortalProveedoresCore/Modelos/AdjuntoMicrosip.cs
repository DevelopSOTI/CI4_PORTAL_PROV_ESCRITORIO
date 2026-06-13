namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Adjunto extra que el proveedor subió desde la web junto con la factura
    /// o el complemento (PDF, OC, etc.). El servicio Windows lo descarga,
    /// lo comprime en ZIP de una sola entrada y lo inserta en
    /// <c>ARCHIVOS_ADJUNTOS</c> de Microsip (NOM_TABLA='DOCTOS_CM').
    ///
    /// Es el shape que devuelve <c>GET /api/adjuntos?docto_id=&amp;emp=&amp;tipo=</c>
    /// del portal CI4 (controller <c>Api\Adjuntos::index</c>).
    ///
    /// Replica el record <c>TAdjuntoInfo</c> del Delphi
    /// (Func_Facturas_3_3.pas:30-35, Func_Complementos.pas:30-35).
    /// </summary>
    public sealed class AdjuntoMicrosip
    {
        public int    id               { get; set; }
        public string tipo             { get; set; }   // 'F' factura, 'C' complemento
        public int    docto_id         { get; set; }   // DOCTO_CM_ID en MySQL (no en Microsip)
        public int    proveedor_id_msp { get; set; }
        public int    emp_fk           { get; set; }
        public string nombre_original  { get; set; }
        public string nombre_archivo   { get; set; }
        public string mime             { get; set; }
        public int    tamano           { get; set; }   // bytes
    }
}
