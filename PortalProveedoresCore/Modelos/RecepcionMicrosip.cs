using System.Collections.Generic;

namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Recepción de mercancía leída del Firebird de una empresa Microsip.
    /// Los nombres de las propiedades respetan el JSON que espera el endpoint
    /// CI4 (snake_case).
    ///
    /// Trae también el array <see cref="detalle"/> con las partidas. La cabecera
    /// y el detalle se envían juntos al portal (POST /api/recepciones/sync)
    /// para que el UPSERT mantenga la consistencia entre ambas tablas.
    ///
    /// Mismos campos que el INSERT del Delphi histórico
    /// (Func_Recepciones.pas → ACTUALIZA_RECEPCIONES).
    /// </summary>
    public sealed class RecepcionMicrosip
    {
        public int    docto_cm_id          { get; set; }
        public string folio                { get; set; }
        public string fecha                { get; set; }   // "yyyy-MM-dd HH:mm:ss"
        public string clave_prov           { get; set; }
        public int    proveedor_id         { get; set; }
        public int    moneda_id            { get; set; }
        public string moneda_nombre        { get; set; }
        public string moneda_simbolo       { get; set; }   // CLAVE_FISCAL de la moneda
        public decimal importe_neto        { get; set; }
        public decimal total_impuestos     { get; set; }
        public decimal total_retenciones   { get; set; }
        public string fecha_hora_ult_modif { get; set; }
        public int    almacen_id           { get; set; }
        /// <summary>Estatus Microsip: 'P' Pendiente, 'F' Facturada, 'C' Cancelada.</summary>
        public string estatus              { get; set; }
        public int    dias_plazo           { get; set; }
        /// <summary>Clave SAT (G01, G03, P01, etc.) resuelta desde LISTAS_ATRIBUTOS.</summary>
        public string uso_cfdi             { get; set; }

        /// <summary>Partidas de la recepción. Puede venir vacío si la recepción no tiene líneas.</summary>
        public List<RecepcionDetalleMicrosip> detalle { get; set; }
    }

    /// <summary>
    /// Partida de una recepción. Mismas columnas que el INSERT del Delphi
    /// histórico en RECEPCIONES_DET (Func_Recepciones.pas:86-114).
    /// </summary>
    public sealed class RecepcionDetalleMicrosip
    {
        public int    docto_cm_det_id  { get; set; }
        public string nombre           { get; set; }   // nombre del artículo
        public decimal unidades        { get; set; }
        public decimal precio_unitario { get; set; }
        public decimal pctje_dscto     { get; set; }
        public decimal precio_total_neto { get; set; }
        public string notas            { get; set; }
        public int    posicion         { get; set; }
    }
}
