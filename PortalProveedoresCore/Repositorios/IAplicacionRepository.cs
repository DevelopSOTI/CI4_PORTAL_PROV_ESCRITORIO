using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresCore.Repositorios
{
    /// <summary>
    /// Aplica una factura del portal a Microsip (Firebird, BD por empresa).
    /// Replica <c>APLICAR_MICROSIP_33</c> del Delphi
    /// (Func_Facturas_3_3.pas:348-1262).
    ///
    /// IMPLEMENTACIÓN POR SUB-FASES:
    /// <list type="bullet">
    ///   <item>
    ///     <b>2.2 (este archivo)</b>: bloques 1-8 (validaciones + DOCTOS_CM
    ///     + LIGAS + DET + IMPUESTOS). Se ejecuta en modo DRY-RUN — al final
    ///     de la transacción se hace ROLLBACK siempre, para poder validar
    ///     contra producción sin riesgo.
    ///   </item>
    ///   <item>
    ///     <b>2.3</b>: bloques 9-12 (CFDI repo + CFD_RECIBIDOS + vencimientos
    ///     + stored proc GENERA_DOCTO_CP_CM).
    ///   </item>
    ///   <item>
    ///     <b>2.4</b>: bloques 13-17 (adjuntos + UPDATE recepción + UPDATE
    ///     portal + COMMIT + correo opcional). Se elimina el ROLLBACK forzado.
    ///   </item>
    /// </list>
    /// </summary>
    public interface IAplicacionRepository
    {
        /// <summary>
        /// Modo VALIDACIÓN: ejecuta los bloques 1-11 contra Firebird y al
        /// final hace ROLLBACK SIEMPRE. Útil cuando se quiere probar el flujo
        /// con datos reales sin dejar basura. NO procesa adjuntos (bloque 12)
        /// ni actualiza el portal (bloque 14).
        ///
        /// <paramref name="cfdi"/> trae el XML del CFDI tal como vino del
        /// portal (UTF-8) más USO_CFDI y LUGAR_EXPEDICION.
        /// </summary>
        Task<ResultadoAplicacionDryRun> AplicarFacturaDryRunAsync(
            string nombreEmpresaMicrosip,
            FacturaAplicar factura,
            CfdiXmlMicrosip cfdi,
            CancellationToken ct);

        /// <summary>
        /// Modo PRODUCCIÓN: ejecuta los bloques 1-15 contra Firebird y
        /// hace COMMIT real al final. La factura queda aplicada en Microsip
        /// (DOCTOS_CM, REPOSITORIO_CFDI, CFD_RECIBIDOS, vencimientos, cargo
        /// CP) y el portal queda marcado (FACTURA_PROVEEDOR_33.ESTATUS='R',
        /// RECEPCIONES.ESTATUS='R').
        ///
        /// Si CUALQUIER bloque falla, hace ROLLBACK automático — la
        /// transacción Firebird es atómica. La llamada al portal (bloque 14)
        /// va DENTRO de la transacción Firebird; si el portal falla, también
        /// rollbackeamos para no quedar desincronizados.
        ///
        /// <paramref name="cfdi"/> y <paramref name="adjuntos"/> se bajan en
        /// el SincronizadorAplicacion ANTES de abrir la transacción Firebird
        /// (para que cualquier fallo de red NO bloquee la transacción).
        ///
        /// <paramref name="marcarPortalAsync"/> es un callback que el
        /// repository invoca dentro del bloque 14 (después de tener el
        /// folio definitivo y el DOCTO_CM_ID nuevo). El sincronizador hace
        /// la llamada HTTP real con esos datos.
        ///
        /// <paramref name="sincronizarPortalYaAplicadaAsync"/> es un callback
        /// alternativo que se invoca SOLO cuando la recepción Microsip ya
        /// está facturada (ESTATUS='F') y se encontró la compra ya ligada.
        /// Recibe (compraId, folioCompra) reales de Microsip y llama al
        /// endpoint <c>factura-ya-aplicada-sincronizar</c> (UPDATE por
        /// DOCTO_CM_ID del portal, NO por RECEP_ID). Réplica del SOAP
        /// <c>ACTUALIZAR_FACTURA_PORTAL_ESCT</c>. Si es null se trata el
        /// caso como saltado, igual que antes.
        /// </summary>
        Task<ResultadoAplicacion> AplicarFacturaAsync(
            string nombreEmpresaMicrosip,
            FacturaAplicar factura,
            CfdiXmlMicrosip cfdi,
            AdjuntoDescargado[] adjuntos,
            System.Func<int, string, System.Threading.Tasks.Task<bool>> marcarPortalAsync,
            System.Func<int, string, System.Threading.Tasks.Task<bool>> sincronizarPortalYaAplicadaAsync,
            CancellationToken ct);

        /// <summary>
        /// Modo PRODUCCIÓN para facturas SIN recepción (RECEP_ID = 0). El
        /// operador eligió en el modal del Escritorio un artículo NO
        /// almacenable y una condición de pago — el repositorio crea la
        /// compra directamente, sin ligarla a una recepción.
        ///
        /// Réplica de <c>APLICAR_SIN_RECEPCION</c> del SOAP legacy
        /// (F_APLICAR_FACTURA.cs:1007-1689):
        /// <list type="number">
        ///   <item>Valida que el artículo NO sea almacenable.</item>
        ///   <item>Resuelve la condición de pago (COND_PAGO_ID).</item>
        ///   <item>Resuelve la sucursal "Matriz" y la CLAVE_PROV del proveedor.</item>
        ///   <item>Crea DOCTOS_CM tipo 'C', detalle de 1 línea genérica,
        ///         impuestos del artículo, REPOSITORIO_CFDI + CFD_RECIBIDOS,
        ///         cargo CP via GENERA_DOCTO_CP_CM y los vencimientos.</item>
        ///   <item>Copia los adjuntos del portal a ARCHIVOS_ADJUNTOS.</item>
        ///   <item>Marca la factura como aplicada en el portal (callback).</item>
        ///   <item>COMMIT atómico — rollback si cualquier paso falla.</item>
        /// </list>
        /// </summary>
        Task<ResultadoAplicacion> AplicarFacturaSinRecepcionAsync(
            string nombreEmpresaMicrosip,
            FacturaAplicar factura,
            string articuloNombre,
            string condicionPagoNombre,
            CfdiXmlMicrosip cfdi,
            AdjuntoDescargado[] adjuntos,
            System.Func<int, string, System.Threading.Tasks.Task<bool>> marcarPortalAsync,
            CancellationToken ct);

        /// <summary>
        /// Asocia un complemento de pago a un crédito Microsip existente.
        /// Réplica de Func_Complementos.pas (el bloque incompleto del Delphi)
        /// CON los pasos que faltaban: INSERT CFD_RECIBIDOS, UPDATE DOCTOS_CP
        /// TIENE_CFD, marcar portal, COMMIT.
        ///
        /// A diferencia de las facturas, NO crea un nuevo documento — solo
        /// asocia el CFDI al crédito que ya está en DOCTOS_CP.
        ///
        /// <paramref name="marcarPortalAsync"/> recibe el CREDITO_FK y hace
        /// la llamada HTTP a marcar-complemento-aplicado.
        /// </summary>
        Task<ResultadoAplicacion> AplicarComplementoAsync(
            string nombreEmpresaMicrosip,
            ComplementoAplicar complemento,
            CfdiXmlMicrosip cfdi,
            AdjuntoDescargado[] adjuntos,
            System.Func<int, System.Threading.Tasks.Task<bool>> marcarPortalAsync,
            CancellationToken ct);

        /// <summary>
        /// Réplica de la rama <c>TIENE_CFDI=='S'</c> del SOAP
        /// <c>F_APLICAR_COMPLEMENTO.cs:794-819</c>. Se invoca SOLO cuando
        /// <see cref="AplicarComplementoAsync"/> ya devolvió
        /// <see cref="ResultadoAplicacionTipo.CreditoYaConCfdi"/> — el crédito
        /// origen en Microsip ya tenía un CFDI asociado y el operador del
        /// Escritorio quiere igualmente:
        /// <list type="number">
        ///   <item>Actualizar <c>REPOSITORIO_CFDI.TIPO_DOCTO_MSP = 'Pago'</c>
        ///         en el CFDI que ya existía (réplica de
        ///         <c>ActualizaTIPO_DOCTO_MSP</c> en el SOAP).</item>
        ///   <item>Insertar los adjuntos del complemento en
        ///         <c>ARCHIVOS_ADJUNTOS</c> con <c>NOM_TABLA='DOCTOS_CP'</c>.</item>
        ///   <item>Marcar el complemento como aplicado en el portal MySQL via
        ///         el mismo callback <paramref name="marcarPortalAsync"/>.</item>
        ///   <item>COMMIT atómico — rollback si cualquier paso falla.</item>
        /// </list>
        ///
        /// El flujo es <b>opt-in por el llamador</b>: el Service NO lo invoca
        /// (se queda con el <see cref="ResultadoAplicacionTipo.CreditoYaConCfdi"/>
        /// como saltado). El Escritorio sí lo invoca como segundo paso cuando
        /// quiere igualmente asociar los adjuntos extras de un complemento
        /// cuyo crédito ya tenía CFDI viejo en Microsip.
        /// </summary>
        Task<ResultadoAplicacion> AsociarComplementoYaConCfdiAsync(
            string nombreEmpresaMicrosip,
            ComplementoAplicar complemento,
            AdjuntoDescargado[] adjuntos,
            System.Func<int, System.Threading.Tasks.Task<bool>> marcarPortalAsync,
            CancellationToken ct);
    }

    /// <summary>
    /// Adjunto que ya fue descargado del portal por el sincronizador. Trae el
    /// binario crudo en memoria — el repository se encarga de comprimirlo en
    /// ZIP e insertarlo en ARCHIVOS_ADJUNTOS.
    /// </summary>
    public sealed class AdjuntoDescargado
    {
        public int    Id              { get; set; }
        public string NombreOriginal  { get; set; }
        public byte[] Contenido       { get; set; }
    }

    /// <summary>
    /// Resultado de una aplicación real (NO dry-run). Incluye los IDs
    /// definitivos en Microsip y la cantidad de adjuntos efectivamente
    /// insertados.
    /// </summary>
    public sealed class ResultadoAplicacion
    {
        public ResultadoAplicacionTipo tipo { get; set; }
        public int    ultimoBloque       { get; set; }
        public string folioFinalGenerado { get; set; }
        public int    nuevoDoctoCmId     { get; set; }
        public int    renglonesDetalle   { get; set; }
        public int    filasImpuestos     { get; set; }
        public bool   cfdiCreado         { get; set; }
        public int    adjuntosInsertados { get; set; }
        public int    adjuntosOmitidos   { get; set; }
        public bool   portalMarcado      { get; set; }
        public string mensaje            { get; set; }

        /// <summary>
        /// Texto extra que el Escritorio agrega al banner de éxito (sin
        /// convertirlo en error). Lo usa <c>AplicadorFacturas</c> para
        /// reportar el resultado del correo de notificación al proveedor
        /// (réplica del SOAP <c>PROCESO_ENVIAR</c> de
        /// F_APLICAR_FACTURA.cs:985-990). El Service NO lo setea — queda
        /// null para sincronizaciones automáticas y otros consumidores.
        /// </summary>
        public string mensajeAdicionalEscritorio { get; set; }
    }

    /// <summary>
    /// Resultado de un dry-run de aplicación. Indica qué bloque fue el último
    /// en ejecutarse y, si hubo error, el mensaje y la categoría.
    /// </summary>
    public sealed class ResultadoAplicacionDryRun
    {
        /// <summary>Categoría del resultado.</summary>
        public ResultadoAplicacionTipo tipo { get; set; }

        /// <summary>Último bloque ejecutado (0-8). 0 = ni siquiera abrió la conexión.</summary>
        public int ultimoBloque { get; set; }

        /// <summary>FOLIO_FINAL ('WEB000123') que asignaría — solo informativo, se descarta con el rollback.</summary>
        public string folioFinalGenerado { get; set; }

        /// <summary>Número de renglones de DOCTOS_CM_DET que insertaría.</summary>
        public int renglonesDetalle { get; set; }

        /// <summary>Número de filas de IMPUESTOS_DOCTOS_CM que insertaría.</summary>
        public int filasImpuestos { get; set; }

        /// <summary>True si el REPOSITORIO_CFDI tuvo que crearse (UUID nuevo); false si ya existía.</summary>
        public bool cfdiCreado { get; set; }

        /// <summary>Mensaje legible (OK o motivo del error).</summary>
        public string mensaje { get; set; }
    }

    public enum ResultadoAplicacionTipo
    {
        /// <summary>Bloques 1-8 OK contra producción. ROLLBACK aplicado.</summary>
        OkDryRun,

        /// <summary>La recepción origen no existe en Microsip — la factura no se puede aplicar.</summary>
        RecepcionNoExiste,

        /// <summary>La recepción ya está facturada (ESTATUS='F') — habría que solo marcar el portal.</summary>
        RecepcionYaFacturada,

        /// <summary>El FOLIO_COMPRA del proveedor ya está registrado en otra compra del mismo proveedor.</summary>
        FolioCompraDuplicado,

        /// <summary>La serie WEB no está registrada en FOLIOS_COMPRAS de Microsip.</summary>
        SerieWebNoConfigurada,

        /// <summary>Excepción de Firebird en algún bloque.</summary>
        Error,

        /// <summary>No se pudo abrir la conexión Firebird de esa empresa.</summary>
        ErrorConexion,

        // === Fase 3 — complementos ===

        /// <summary>El crédito (DOCTOS_CP) origen no existe en Microsip.</summary>
        CreditoNoExiste,

        /// <summary>El crédito ya tiene un CFDI asociado (TIENE_CFD='S') — no se puede asociar otro.</summary>
        CreditoYaConCfdi,

        // === Aplicación SIN recepción (réplica SOAP APLICAR_SIN_RECEPCION) ===

        /// <summary>El artículo elegido en el modal no existe en ARTICULOS de Microsip.</summary>
        ArticuloNoExiste,

        /// <summary>El artículo elegido es almacenable; debe ser NO almacenable para este flujo.</summary>
        ArticuloEsAlmacenable,

        /// <summary>La condición de pago elegida no existe en CONDICIONES_PAGO_CP.</summary>
        CondicionPagoNoExiste,

        /// <summary>El proveedor del CFDI no existe en PROVEEDORES de Microsip (CLAVE_PROV principal).</summary>
        ProveedorNoExisteMicrosip,

        /// <summary>La sucursal "Matriz" no existe en SUCURSALES — Microsip mal configurado.</summary>
        SucursalMatrizNoExiste,

        /// <summary>El CFDI no trae UUID — no se puede aplicar.</summary>
        UuidVacio,

        /// <summary>
        /// La recepción origen en Microsip está cancelada (ESTATUS='C') — la
        /// factura debe marcarse como rechazada en el portal (réplica del
        /// flujo SOAP FACTURA_RECEP_CANCELADO en F_APLICAR_FACTURA.cs:155-186).
        /// </summary>
        RecepcionCancelada,

        /// <summary>
        /// La recepción origen en Microsip ya está facturada (ESTATUS='F') y
        /// hay una compra ya ligada vía DOCTOS_CM_LIGAS. En vez de fallar como
        /// <see cref="RecepcionYaFacturada"/>, el orquestador sincroniza el
        /// portal con la compra que YA existe — réplica LITERAL del SOAP
        /// <c>ACTUALIZAR_FACTURA_PORTAL_ESCT</c> (F_APLICAR_FACTURA.cs:190-231).
        /// Es un caso de ÉXITO semánticamente: la factura ya estaba aplicada,
        /// solo faltaba reflejarlo en el portal. El repository convierte este
        /// resultado a <see cref="OkDryRun"/> tras llamar al callback de
        /// sincronización del portal.
        /// </summary>
        RecepcionYaFacturadaSincronizar,
    }
}
