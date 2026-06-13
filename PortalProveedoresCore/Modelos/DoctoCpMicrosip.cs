namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Documento de cuentas por pagar leído del Firebird de una empresa
    /// Microsip. Sirve para CRÉDITOS (<c>cc.tipo='P'</c>) y NOTAS
    /// (<c>cc.tipo='R'</c>) — ambos comparten estructura idéntica y el
    /// mismo SELECT salvo el filtro de tipo. La semántica de 'P' y 'R'
    /// la define la configuración del cliente en Microsip; el cliente
    /// solo respeta lo que hace el Delphi histórico.
    ///
    /// Mismas columnas que el SELECT del Delphi histórico
    /// (Func_Calcula.pas:454-491 para créditos, 540-577 para notas).
    /// </summary>
    public sealed class DoctoCpMicrosip
    {
        public int    docto_cp_id          { get; set; }
        public int    concepto_cp_id       { get; set; }
        public string concepto_cp          { get; set; }   // nombre del concepto (cc.nombre)
        public string folio                { get; set; }
        public string fecha                { get; set; }   // "yyyy-MM-dd HH:mm:ss"
        public string clave_prov           { get; set; }
        public int    proveedor_id         { get; set; }
        public string descripcion          { get; set; }
        public string fecha_hora_ult_modif { get; set; }
        /// <summary>'S' o 'N' — viene de dc.cancelado.</summary>
        public string cancelado            { get; set; }
        /// <summary>'S' o 'N' — viene de dc.aplicado.</summary>
        public string aplicado             { get; set; }
        /// <summary>'S' o 'N' — viene de dc.tiene_cfd. El portal lo usa para derivar ESTATUS='F' cuando 'S'.</summary>
        public string tiene_cfd            { get; set; }

        /// <summary>
        /// Renglones del cobro: las facturas/cargos del proveedor que se están
        /// pagando con este crédito, junto con su CFDI. SOLO incluye renglones
        /// cuyo CFDI tiene MetodoPago='PPD'. El portal hace DELETE+INSERT por
        /// cabecera de su tabla CREDITOS_DET.
        /// </summary>
        public CreditoDetMicrosip[] detalle { get; set; }
    }
}
