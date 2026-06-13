using System;

namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Filtros que envía la UI al endpoint <c>/api/escritorio/complementos-pendientes</c>.
    /// Todos opcionales — al construir el query string se omiten los que
    /// tengan valor por defecto. Mismo patrón que <see cref="FiltroFacturasEscritorio"/>.
    /// </summary>
    public sealed class FiltroComplementosEscritorio
    {
        public int       EmpIdMsp     { get; set; }
        public int       ProveedorId  { get; set; }   // 0 = todos
        public int       AlmacenId    { get; set; }   // 0 = todos
        public DateTime? Desde        { get; set; }
        public DateTime? Hasta        { get; set; }
        public bool      SoloPorVencer{ get; set; }
        public int       Limit        { get; set; } = 100;
    }

    /// <summary>
    /// Vista completa de un complemento de pago del portal listo para asociar
    /// a un crédito Microsip. La devuelve
    /// <c>GET /api/aplicacion/complementos-aplicar?emp_id_msp=N</c>.
    ///
    /// Réplica del shape del SELECT del Delphi (Func_Calcula.pas:792-815).
    /// Nombres en MAYÚSCULAS para coincidir con el JSON del portal y con la
    /// nomenclatura del Pascal — facilita el grep cruzado entre Delphi y C#.
    ///
    /// A diferencia de una factura, el complemento NO crea un nuevo
    /// DOCTOS_CP en Microsip — se asocia al crédito existente cuyo
    /// CREDITO_ID coincide con <see cref="CREDITO_FK"/>.
    /// </summary>
    public sealed class ComplementoAplicar
    {
        public int    DOCTO_CP_ID        { get; set; }   // F.DOCTO_CP_ID — id en MySQL del complemento
        public string SERIE              { get; set; }
        public string FOLIO_PAGO         { get; set; }   // F.FOLIO — folio del complemento en el portal
        public decimal MONTO             { get; set; }
        public string MONEDA_PAGO        { get; set; }
        public int    CREDITO_FK         { get; set; }   // = CREDITOS.CREDITO_ID — el crédito al que se asocia
        public string FOLIO_CREDITO      { get; set; }   // C.FOLIO — folio del crédito Microsip

        /// <summary>
        /// C.CONCEPTO_CP_ID de CREDITOS (MySQL). El legacy lo bajaba con el
        /// SOAP <c>CargarCredito</c> (services/creditos.php:382) y lo usaba
        /// en el lookup de DOCTOS_CP en Firebird
        /// (F_APLICAR_COMPLEMENTO.cs:649-653: FOLIO + NATURALEZA_CONCEPTO='R'
        /// + PROVEEDOR_ID + CONCEPTO_CP_ID) para no asociar el pago al
        /// documento CxP equivocado cuando un proveedor repite folios entre
        /// conceptos. 0 = portal viejo sin la columna en la respuesta (el
        /// lookup omite el filtro en ese caso).
        /// </summary>
        public int    CONCEPTO_CP_ID     { get; set; }
        public string FECHA_PAGO         { get; set; }
        public string FECHA_COMPLEMENTO  { get; set; }   // F.FECHA del complemento
        public int    PROVEEDOR_ID       { get; set; }
        public string RFC                { get; set; }   // F.EMISOR_RFC
        public string NOMBRE             { get; set; }   // P.NOMBRE del proveedor
        public string UUID               { get; set; }
        public string USO_CFDI           { get; set; }

        /// <summary>
        /// F.VERSION_PAGO de COMPLEMENTO_ENCABEZADO (MySQL). El SOAP
        /// F_APLICAR_COMPLEMENTO.cs:436-446 cambia el título del modal y
        /// el texto del botón según este campo:
        /// <list type="bullet">
        ///   <item>"0" — nota de crédito (botón "Asociar nota de crédito a Microsip").</item>
        ///   <item>cualquier otro — complemento de pago (botón "Asociar complemento a Microsip").</item>
        /// </list>
        /// </summary>
        public string VERSION_PAGO       { get; set; }
    }
}
