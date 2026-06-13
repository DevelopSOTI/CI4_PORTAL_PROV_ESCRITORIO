using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresCore.Servicios
{
    /// <summary>
    /// Cliente del portal CI4. Una operación por endpoint.
    ///
    /// Operaciones de servicio (cron):  Sincronizar*, Sellar*.
    /// Operaciones de configurador:     Listar*, Actualizar*.
    /// </summary>
    public interface IPortalApi
    {
        // --- Servicio Windows (ciclo de sincronización) ---
        Task<ResumenSync> SincronizarEmpresasAsync(IEnumerable<EmpresaMicrosip> empresas, CancellationToken ct);
        Task SellarUltimaSincronizacionAsync(string timestamp, CancellationToken ct);

        /// <summary>
        /// POST /api/almacenes/sync — sube el catálogo de almacenes de UNA
        /// empresa al portal. El servicio lo invoca una vez por empresa
        /// autorizada por ciclo.
        /// </summary>
        Task<ResumenSync> SincronizarAlmacenesAsync(int empIdMsp, IEnumerable<AlmacenMicrosip> almacenes, CancellationToken ct);

        /// <summary>
        /// POST /api/monedas/sync — sube el catálogo de monedas de UNA
        /// empresa al portal. Misma forma que almacenes: el servicio lo
        /// invoca una vez por empresa autorizada por ciclo.
        /// </summary>
        Task<ResumenSync> SincronizarMonedasAsync(int empIdMsp, IEnumerable<MonedaMicrosip> monedas, CancellationToken ct);

        /// <summary>
        /// POST /api/recepciones/sync — sube las recepciones (cabecera + detalle)
        /// de UNA empresa al portal. A diferencia de los catálogos, cada item
        /// del payload incluye su array <c>detalle</c>; el portal hace UPSERT
        /// de cabecera por (FOLIO, EMP_FK) y DELETE+INSERT del detalle completo.
        /// </summary>
        Task<ResumenSync> SincronizarRecepcionesAsync(int empIdMsp, IEnumerable<RecepcionMicrosip> recepciones, CancellationToken ct);

        /// <summary>
        /// POST /api/creditos/sync — sube los créditos (pagos PPD) de UNA empresa
        /// al portal. Solo cabecera; el detalle (CREDITOS_DET con XML del CFDI)
        /// se sincroniza en una fase posterior. UPSERT por (FOLIO, CONCEPTO_CP_ID,
        /// PROVEEDOR_ID, EMP_FK).
        /// </summary>
        Task<ResumenSync> SincronizarCreditosAsync(int empIdMsp, IEnumerable<DoctoCpMicrosip> creditos, CancellationToken ct);

        /// <summary>
        /// POST /api/notas/sync — sube las NOTAS de UNA empresa al portal.
        /// Mismo shape que créditos; cambia solo el filtro Microsip
        /// (<c>cc.tipo='R'</c> vs <c>'P'</c> para créditos). El significado
        /// semántico de 'R' lo define la configuración del cliente en
        /// Microsip — el cliente replica el filtro tal cual.
        /// </summary>
        Task<ResumenSync> SincronizarNotasAsync(int empIdMsp, IEnumerable<DoctoCpMicrosip> notas, CancellationToken ct);

        // ===== APLICACIÓN (Portal → Microsip) ===================================
        //
        // A diferencia de los catálogos y documentos anteriores (push de Microsip
        // al portal), este flujo es INVERSO: el proveedor sube su factura/complemento
        // al portal vía web, y el servicio Windows luego la aplica en Microsip.
        // El interruptor maestro es APLICA_DIR (PARAMETROS) — si está en TRUE,
        // el servicio aplica automáticamente; si FALSE, sólo loguea las pendientes
        // para que el operador las procese desde la app de escritorio.
        // Mismo patrón que el Delphi histórico (Func.pas:214-252).

        /// <summary>
        /// GET /api/aplicacion/aplica-dir — devuelve el valor del parámetro
        /// APLICA_DIR del portal (true = aplicación automática habilitada).
        /// Default seguro: false si el parámetro no existe.
        /// </summary>
        Task<bool> ObtenerAplicaDirAsync(CancellationToken ct);

        /// <summary>
        /// Prueba ligera de conectividad + autenticación contra el portal.
        /// Hace un GET autenticado a un endpoint barato y devuelve true si el
        /// portal respondió 2xx (URL alcanzable + API key válida). Cualquier
        /// fallo de red, DNS, 401/403 o 5xx devuelve false — NO lanza.
        ///
        /// Pensado para el botón "Probar conexión" del Configurador y para el
        /// modo headless <c>--probar-portal</c> que invoca el instalador.
        /// </summary>
        Task<bool> ProbarConexionAsync(CancellationToken ct);

        /// <summary>
        /// GET /api/aplicacion/pendientes?emp_id_msp=N — devuelve los conteos
        /// y listas breves de facturas y complementos pendientes de aplicar
        /// en Microsip para una empresa autorizada específica.
        /// </summary>
        Task<PendientesAplicacion> ObtenerPendientesAsync(int empIdMsp, CancellationToken ct);

        /// <summary>
        /// GET /api/aplicacion/facturas-aplicar?emp_id_msp=N — devuelve la
        /// lista completa de facturas listas para aplicar en Microsip
        /// (cabeceras + datos de proveedor + recepción). Mismo shape exacto
        /// que el SELECT del Delphi en <c>Func_Calcula.pas:685-720</c>.
        /// </summary>
        Task<FacturaAplicar[]> ObtenerFacturasAplicarAsync(int empIdMsp, CancellationToken ct);

        /// <summary>
        /// GET /api/escritorio/facturas-pendientes — listado con filtros
        /// server-side para la UI del Escritorio. A diferencia de
        /// <see cref="ObtenerFacturasAplicarAsync"/>, este endpoint acepta
        /// filtros (proveedor, almacén, rango fechas, por vencer, límite)
        /// y respeta APLICA_DIR: en modo automático devuelve SOLO las
        /// facturas sin recepción (las otras las aplica el servicio sin
        /// intervención manual).
        ///
        /// Devuelve la respuesta completa para que la UI pueda mostrar el
        /// modo automático/manual con un mensaje claro al operador.
        /// </summary>
        Task<RespuestaFacturasEscritorio> ObtenerFacturasPendientesEscritorioAsync(
            FiltroFacturasEscritorio filtro, CancellationToken ct);

        /// <summary>
        /// GET /api/aplicacion/complementos-aplicar?emp_id_msp=N — análogo
        /// para complementos de pago. Mismo shape que
        /// <c>Func_Calcula.pas:792-815</c>.
        /// </summary>
        Task<ComplementoAplicar[]> ObtenerComplementosAplicarAsync(int empIdMsp, CancellationToken ct);

        /// <summary>
        /// GET /api/escritorio/complementos-pendientes?... — endpoint NUEVO
        /// con filtros server-side (proveedor, almacén, rango fechas, por
        /// vencer, limit). Réplica del SOAP F_COMPLEMENTO.btnConsultar_Click
        /// (F_COMPLEMENTO.cs:151-200) que enviaba esos filtros al CARGAR_COMPLEMENTOS.
        /// El Service sigue usando <see cref="ObtenerComplementosAplicarAsync"/>
        /// (sin filtros) — son endpoints independientes.
        /// </summary>
        Task<ComplementoAplicar[]> ObtenerComplementosPendientesEscritorioAsync(
            FiltroComplementosEscritorio filtro, CancellationToken ct);

        /// <summary>
        /// POST /api/aplicacion/marcar-complemento-aplicado — análogo a
        /// marcar-factura-aplicada pero para complementos:
        ///
        ///   UPDATE COMPLEMENTO_ENCABEZADO SET ESTATUS='R',
        ///          USUARIO_ASOCIO_COBRO='SYSDBA', FECHA_ASOCIO_COBRO=NOW()
        ///    WHERE CREDITO_FK=?
        ///   UPDATE CREDITOS SET ESTATUS='R' WHERE CREDITO_ID=?
        ///
        /// El parámetro es el CREDITO_FK del complemento (= CREDITOS.CREDITO_ID).
        /// </summary>
        Task<bool> MarcarComplementoAplicadoAsync(int creditoFk, CancellationToken ct);

        /// <summary>
        /// Overload con los parámetros ADITIVOS del endpoint (el Service
        /// sigue usando el overload corto — mismo wire-format que siempre):
        /// <list type="bullet">
        ///   <item><paramref name="doctoCpId"/> &gt; 0 → el UPDATE marca SOLO
        ///     ese complemento (<c>WHERE CREDITO_FK=? AND DOCTO_CP_ID=?</c>) —
        ///     réplica del SOAP ACTUALIZAR_COMPLEMENTO_PORTAL rama 'R'
        ///     (services/Complementos.php:96: <c>WHERE DOCTO_CP_ID=?</c>,
        ///     el id MySQL del complemento específico).</item>
        ///   <item><paramref name="usuario"/> no vacío → se sella en
        ///     USUARIO_ASOCIO_COBRO (F_APLICAR_COMPLEMENTO.cs:672 pasaba el
        ///     operador Microsip real, no 'SYSDBA').</item>
        /// </list>
        /// Con doctoCpId=0 y usuario null/vacío equivale al overload corto.
        /// </summary>
        Task<bool> MarcarComplementoAplicadoAsync(int creditoFk, int doctoCpId, string usuario, CancellationToken ct);

        /// <summary>
        /// GET /api/adjuntos?docto_id=N&amp;emp=N&amp;tipo=F — lista los archivos
        /// adjuntos extra que el proveedor subió al portal junto con su factura
        /// (o complemento). El servicio los descarga uno por uno y los inserta
        /// en ARCHIVOS_ADJUNTOS de Microsip. Si no hay adjuntos devuelve array
        /// vacío (HTTP 200, no 404).
        ///
        /// <paramref name="tipo"/>: 'F' = factura, 'C' = complemento.
        /// </summary>
        Task<AdjuntoMicrosip[]> ListarAdjuntosAsync(int doctoId, int empIdMsp, string tipo, CancellationToken ct);

        /// <summary>
        /// GET /api/adjuntos/{id} — descarga el binario crudo del adjunto.
        /// El servicio luego lo comprime en un ZIP de una sola entrada
        /// (es el formato que exige Microsip para FILE_STREAM en
        /// ARCHIVOS_ADJUNTOS).
        ///
        /// Devuelve null si el adjunto no se pudo descargar (404 u otro error),
        /// para que el sincronizador pueda registrar la incidencia y continuar
        /// con los demás adjuntos sin abortar toda la aplicación de la factura.
        /// </summary>
        Task<byte[]> DescargarAdjuntoAsync(int id, CancellationToken ct);

        /// <summary>
        /// GET /api/aplicacion/cfdi-xml?uuid=...&amp;tipo=F — devuelve el XML
        /// del CFDI tal como lo guardó el proveedor en el portal, más los
        /// campos USO_CFDI y LUGAR_EXPEDICION que Microsip necesita para
        /// REPOSITORIO_CFDI. El servicio aplica la transformación de encoding
        /// (UTF-8 → ISO-8859-1 → ASCII) antes de insertarlo.
        /// </summary>
        Task<CfdiXmlMicrosip> ObtenerCfdiXmlAsync(string uuid, string tipo, CancellationToken ct);

        /// <summary>
        /// GET /api/aplicacion/cfdi-pdf?uuid=...&amp;tipo=F — devuelve el PDF
        /// binario que el proveedor subió al portal junto con su factura
        /// (o complemento). Sale de la columna <c>PDF</c> de la misma tabla
        /// auxiliar que el XML.
        ///
        /// Devuelve null si el proveedor subió solo XML sin PDF (caso
        /// común — el CFDI por sí mismo es suficiente para Microsip, el
        /// PDF es solo para validación visual del operador).
        /// </summary>
        Task<byte[]> ObtenerCfdiPdfAsync(string uuid, string tipo, CancellationToken ct);

        // ===== E.4 — rechazo + correo + descarte ===============================

        /// <summary>
        /// GET /api/aplicacion/correo-config — config SMTP del portal (tabla
        /// MAIL). El Escritorio la usa para enviar el correo de rechazo al
        /// proveedor desde su propio cliente SMTP.
        /// </summary>
        Task<CorreoConfig> ObtenerCorreoConfigAsync(CancellationToken ct);

        /// <summary>
        /// GET /api/aplicacion/proveedor-correo?proveedor_id=N&amp;emp_id_msp=M —
        /// devuelve el correo registrado del proveedor en PROVEEDORES.MAIL.
        /// Devuelve cadena vacía si no hay correo registrado.
        /// </summary>
        Task<string> ObtenerCorreoProveedorAsync(int proveedorId, int empIdMsp, CancellationToken ct);

        /// <summary>
        /// GET /api/aplicacion/proveedor-correo-por-rfc?rfc=...&amp;emp_id_msp=M
        /// — réplica funcional del SOAP <c>BuscarProveedorXRFC</c>
        /// (F_RECHAZA_ENVIA_CORREO.cs:97-136). Busca el correo del proveedor
        /// haciendo JOIN ACCESO + PROVEEDORES_MSP por RFC normalizado.
        /// Útil para el fallback de rechazo de complementos cuando el
        /// PROVEEDOR_ID del complemento no apunta a un ACCESO en el portal.
        /// </summary>
        Task<string> ObtenerCorreoProveedorPorRfcAsync(string rfc, int empIdMsp, CancellationToken ct);

        /// <summary>
        /// POST /api/aplicacion/rechazar-factura — marca la factura como
        /// rechazada en MySQL. Réplica LITERAL del SOAP
        /// <c>ws.RECHAZA_FACTURA(DOCTO_CM, USUARIO, FECHA, MOTIVO)</c>
        /// (services/facturas.php:272-290): ESTATUS='X' + sello
        /// USUARIO_RECH_FACTURA/FECHA_RECH_FACTURA + MOTIVO_RECHAZO,
        /// WHERE DOCTO_CM_ID.
        ///
        /// <paramref name="doctoCmId"/> es el
        /// <c>FACTURA_PROVEEDOR_33.DOCTO_CM_ID</c> (id MySQL de la factura,
        /// columna "DOCTO_CM_ID" del grid) — NO el RECEP_ID, que no es único
        /// y vale 0 en facturas sin recepción.
        /// </summary>
        Task<bool> RechazarFacturaAsync(int doctoCmId, string usuario, string motivo, CancellationToken ct);

        /// <summary>
        /// POST /api/aplicacion/rechazar-complemento — marca el complemento
        /// como rechazado en MySQL. Mismas columnas que facturas pero con
        /// sufijo "_COBRO" (USUARIO_RECHAZO_COBRO, etc.).
        /// </summary>
        Task<bool> RechazarComplementoAsync(int doctoCpId, string usuario, string motivo, CancellationToken ct);

        /// <summary>
        /// POST /api/aplicacion/descartar-factura — DEPRECADO por paridad
        /// SOAP: el menú "Descartar factura" del legacy (F_FACTURAS.cs:455)
        /// llamaba <c>ws.ACTUALIZA_NUEVO_FOLIO(DOCTO_CM_ID)</c> (ESTATUS='R'),
        /// no este endpoint (ESTATUS='C' por RECEP_ID — el proveedor vería
        /// "Cancelada" en rojo). Usar <see cref="ActualizarNuevoFolioAsync"/>.
        /// Sin call-sites; se conserva solo por compatibilidad del contrato.
        /// </summary>
        Task<bool> DescartarFacturaAsync(int recepId, string usuario, CancellationToken ct);

        /// <summary>
        /// POST /api/aplicacion/actualizar-nuevo-folio — pone ESTATUS='R'
        /// en FACTURA_PROVEEDOR_33 por DOCTO_CM_ID. Réplica LITERAL del
        /// SOAP <c>ws.ACTUALIZA_NUEVO_FOLIO(DOCTO_CM_IDFACSQL)</c>
        /// (services/facturas.php:236-270). Lo usa el Escritorio cuando el
        /// operador, ante un folio duplicado, elige la opción "Actualizar
        /// nuevo folio en portal" del modal F_NUEVO_FOLIO (RESULTADO=2).
        /// </summary>
        Task<bool> ActualizarNuevoFolioAsync(int doctoCmId, CancellationToken ct);

        /// <summary>
        /// POST /api/aplicacion/factura-recep-cancelada — marca la factura
        /// como rechazada en el portal con ESTATUS='W' y motivo fijo "Se
        /// subio una factura a una recepción cancelada". Réplica LITERAL
        /// del SOAP <c>FACTURA_RECEP_CANCELADO</c> del legacy
        /// (services/facturas.php:413-453).
        ///
        /// Se invoca cuando el bloque 1 de <c>AplicacionRepository</c> detecta
        /// que la recepción origen en Microsip tiene ESTATUS='C' (cancelada).
        ///
        /// <paramref name="facturaMysqlId"/> es el
        /// <c>FACTURA_PROVEEDOR_33.DOCTO_CM_ID</c> (NO el RECEP_ID).
        /// </summary>
        Task<bool> MarcarFacturaRecepCanceladaAsync(int facturaMysqlId, string usuario, CancellationToken ct);

        /// <summary>
        /// POST /api/aplicacion/factura-ya-aplicada-sincronizar — sincroniza
        /// el portal con una compra que YA existe en Microsip. Réplica LITERAL
        /// del SOAP <c>ACTUALIZAR_FACTURA_PORTAL_ESCT</c> del legacy
        /// (services/facturas.php:172-234).
        ///
        /// Se invoca cuando el bloque 1 de <c>AplicacionRepository</c> detecta
        /// que la recepción origen en Microsip ya tiene ESTATUS='F' y encuentra
        /// la compra ya ligada vía DOCTOS_CM_LIGAS. El portal hace:
        ///
        ///   UPDATE FACTURA_PROVEEDOR_33 SET FOLIO_MSP=?, COMPRA_ID=?, ESTATUS='R',
        ///          USUARIO_CONV_COMPRA='SYSDBA', FECHA_CONV_COMPRA=NOW()
        ///    WHERE DOCTO_CM_ID=?   -- DOCTO_CM_ID del portal (FACTURA_PROVEEDOR_33)
        ///   UPDATE RECEPCIONES SET ESTATUS='R' WHERE RECEP_ID=?
        ///
        /// Diferencia clave con <see cref="MarcarFacturaAplicadaAsync"/>: el
        /// WHERE es por <c>DOCTO_CM_ID</c> del portal (id MySQL de la factura),
        /// no por <c>RECEP_ID</c>.
        ///
        /// También lo usa el flujo "aplicar SIN recepción" del Escritorio con
        /// <c>recepId=0</c> (el UPDATE a RECEPCIONES no afecta filas — mismo
        /// no-op que el SOAP con $DOCTO_CM_IDmsql=0), porque
        /// marcar-factura-aplicada marca por RECEP_ID y no sirve cuando la
        /// factura no tiene recepción ligada.
        ///
        /// <paramref name="facturaMysqlId"/>: <c>FACTURA_PROVEEDOR_33.DOCTO_CM_ID</c>.
        /// <paramref name="recepId"/>: <c>RECEPCIONES.RECEP_ID</c> del portal (0 = sin recepción).
        /// <paramref name="folioCompra"/>: FOLIO de la compra ya existente en Microsip.
        /// <paramref name="compraId"/>: DOCTO_CM_ID de la compra ya existente en Microsip.
        /// </summary>
        Task<bool> SincronizarFacturaYaAplicadaAsync(
            int facturaMysqlId, int recepId, string folioCompra, int compraId, CancellationToken ct);

        // ===== E.5 — catálogo de proveedores =================================

        /// <summary>
        /// GET /api/escritorio/proveedores-registrados?emp_id_msp=N
        /// — lista de proveedores con cuenta de acceso al portal (sale de
        /// la tabla <c>ACCESO</c> filtrando por <c>EMP_FK</c>). Réplica del
        /// SOAP <c>ws.GETProveedoresRegistrados</c>.
        /// </summary>
        Task<RespuestaProveedoresRegistrados> ObtenerProveedoresRegistradosAsync(
            int empIdMsp, CancellationToken ct);

        /// <summary>
        /// GET /api/escritorio/catalogos-filtros?emp_id_msp=N — catálogos
        /// de proveedores y almacenes para alimentar los ComboBox
        /// AutoComplete de las vistas (sustituye los TextBox planos que
        /// había en E.5).
        /// </summary>
        Task<RespuestaCatalogosFiltros> ObtenerCatalogosFiltrosAsync(
            int empIdMsp, CancellationToken ct);

        /// <summary>
        /// GET /api/escritorio/catalogos-filtros?emp_id_msp=N&amp;aplica_dir=1&amp;entidad=facturas
        /// — variante con filtros del SOAP. Réplica del flujo
        /// F_FACTURAS.cs:55-82 + C_FUNCIONES.LIST_PROVEEDORES: cuando
        /// <paramref name="aplicaDir"/>=true y la entidad es 'facturas',
        /// el catálogo de proveedores devuelve SOLO aquellos con al menos
        /// una factura pendiente sin recepción ligada (RECEPCION_ID=0)
        /// — proveedores.php:322-325.
        /// </summary>
        Task<RespuestaCatalogosFiltros> ObtenerCatalogosFiltrosAsync(
            int empIdMsp, bool aplicaDir, string entidad, CancellationToken ct);

        /// <summary>
        /// POST /api/aplicacion/marcar-factura-aplicada — el servicio invoca
        /// este endpoint DESPUÉS del COMMIT de Firebird, para reflejar en MySQL
        /// que la factura ya pasó a compras de Microsip:
        ///
        ///   UPDATE FACTURA_PROVEEDOR_33 SET FOLIO_MSP=?, COMPRA_ID=?, ESTATUS='R',
        ///          USUARIO_CONV_COMPRA='SYSDBA', FECHA_CONV_COMPRA=NOW()
        ///    WHERE RECEP_ID=?
        ///   UPDATE RECEPCIONES SET ESTATUS='R' WHERE RECEP_ID=?
        ///
        /// Devuelve true si la transacción del portal fue OK (incluso con 0 filas
        /// afectadas, que sería un caso anómalo: la factura ya estaba marcada o
        /// el RECEP_ID no existe en el portal).
        /// </summary>
        Task<bool> MarcarFacturaAplicadaAsync(int recepId, string folioMsp, int compraId, CancellationToken ct);

        /// <summary>
        /// POST /api/proveedores/sync — sube el catálogo de proveedores de
        /// UNA empresa al portal. El servicio lee Microsip con JOIN triple
        /// (proveedores + claves_proveedores + libres_proveedor) y envía aquí.
        /// </summary>
        Task<ResumenSync> SincronizarProveedoresAsync(int empIdMsp, IEnumerable<ProveedorMicrosip> proveedores, CancellationToken ct);

        // --- Configurador (administración remota) ---

        /// <summary>
        /// GET /api/empresas — lista TODAS las empresas con sus flags.
        /// Lo consume el Configurador para llenar el grid de autorizar/bloquear.
        /// </summary>
        Task<List<EmpresaConfig>> ListarEmpresasAsync(CancellationToken ct);

        /// <summary>
        /// GET /api/empresas?solo_autorizadas=1 — solo las empresas con
        /// EMP_ESTATUS='Autorizada'. Lo consume el Servicio para iterar los
        /// sub-pasos (proveedores, recepciones, facturas) únicamente en las
        /// empresas habilitadas — mismo patrón que el Delphi legacy.
        /// </summary>
        Task<List<EmpresaConfig>> ListarEmpresasAutorizadasAsync(CancellationToken ct);

        /// <summary>
        /// PATCH /api/empresas/{id_msp} — cambia estatus, diferencia y/o sinc_desde.
        ///
        /// Para los string (estatus, diferencia): <c>null</c> = no tocar.
        /// Para sinc_desde se usa <see cref="ValorSincDesde"/> porque <c>null</c>
        /// SÍ es un valor válido (significa "sincronizar toda la historia").
        /// </summary>
        Task<EmpresaConfig> ActualizarEmpresaAsync(int idMsp, string estatus, string diferencia, ValorSincDesde sincDesde, CancellationToken ct);

        /// <summary>GET /api/dias — lista los 7 días de la semana con su flag DIA_RECIBE.</summary>
        Task<List<DiaRecepcion>> ListarDiasAsync(CancellationToken ct);

        /// <summary>
        /// PATCH /api/dias — actualización batch parcial. Los días que no incluyas
        /// quedan intactos. Devuelve los 7 días tras la actualización.
        /// </summary>
        Task<List<DiaRecepcion>> ActualizarDiasAsync(IEnumerable<DiaRecepcion> cambios, CancellationToken ct);

        /// <summary>
        /// GET /api/parametros — lista los parámetros de negocio del portal
        /// (DIAS_LIMITE, TOLERANCIA, APLICA_DIR, etc.) con su descripción.
        /// </summary>
        Task<List<ParametroPortal>> ListarParametrosAsync(CancellationToken ct);

        /// <summary>
        /// PATCH /api/parametros — actualización batch parcial. Solo modifica
        /// PARAM_VALOR de filas existentes. Claves auto-administradas (LAST_UPDATE)
        /// se ignoran silenciosamente del lado server. Devuelve el listado completo
        /// tras la actualización + el resumen (cuántos aplicaron, cuáles se ignoraron).
        /// </summary>
        Task<ResultadoActualizacionParametros> ActualizarParametrosAsync(IEnumerable<ParametroPortal> cambios, CancellationToken ct);

        /// <summary>
        /// GET /api/portal-config — tema visual del portal (paleta + nombre + logo).
        /// Lo consumen las apps de escritorio al arrancar para pintarse con la
        /// marca del cliente. Si el portal está offline, se usan los defaults
        /// hardcodeados en el cliente.
        /// </summary>
        Task<TemaPortal> ObtenerTemaAsync(CancellationToken ct);
    }

    /// <summary>
    /// Respuesta combinada del PATCH /api/parametros: lista actualizada + resumen
    /// de qué pasó con cada clave enviada. La separamos para que el Configurador
    /// pueda mostrar un MessageBox del tipo "Se actualizaron 3 parámetros; LAST_UPDATE
    /// se ignoró por estar protegido".
    /// </summary>
    public sealed class ResultadoActualizacionParametros
    {
        public ParametroPortal[]              parametros { get; set; }
        public ResumenActualizacionParametros resumen    { get; set; }
    }

    /// <summary>
    /// Tri-estado para el campo sinc_desde en PATCH /api/empresas/{id}:
    /// <list type="bullet">
    ///   <item><see cref="NoTocar"/> — el PATCH no incluye sinc_desde en el body.</item>
    ///   <item><see cref="SincToda"/> — pone NULL en la BD (sincronizar toda la historia).</item>
    ///   <item><see cref="Desde(DateTime)"/> — pone una fecha específica.</item>
    /// </list>
    /// La distinción importa porque <c>null</c> tiene significado de negocio
    /// (sincronizar todo), distinto de "no quiero tocar este campo".
    /// </summary>
    public sealed class ValorSincDesde
    {
        public bool Toca   { get; private set; }
        public bool EsNull { get; private set; }
        public DateTime Fecha { get; private set; }

        private ValorSincDesde() { }

        public static readonly ValorSincDesde NoTocar  = new ValorSincDesde { Toca = false };
        public static readonly ValorSincDesde SincToda = new ValorSincDesde { Toca = true, EsNull = true };
        public static ValorSincDesde Desde(DateTime fecha) =>
            new ValorSincDesde { Toca = true, EsNull = false, Fecha = fecha };
    }

    /// <summary>
    /// Resumen que devuelve el endpoint /api/empresas/sync.
    /// </summary>
    public sealed class ResumenSync
    {
        public int inserted  { get; set; }
        public int updated   { get; set; }
        public int unchanged { get; set; }
        public ErrorSync[] errors { get; set; }
    }

    /// <summary>
    /// Item de error que devuelve cualquier endpoint /sync. Como cada endpoint
    /// usa un identificador distinto (empresa_id, almacen_id, moneda_id,
    /// proveedor_id, folio para documentos), todas las propiedades son
    /// opcionales y solo viene poblada la que aplique al tipo de sync.
    /// JavaScriptSerializer ignora silenciosamente las claves que no están en
    /// el JSON, así que esta clase compartida funciona para los siete endpoints.
    /// Cada sincronizador usa <see cref="DescribirItem"/> para mostrar la
    /// clave correcta en el log.
    /// </summary>
    public sealed class ErrorSync
    {
        public int    empresa_id   { get; set; }
        public int    almacen_id   { get; set; }
        public int    moneda_id    { get; set; }
        public int    proveedor_id { get; set; }
        public string folio        { get; set; }
        public string msg          { get; set; }

        /// <summary>
        /// Devuelve una etiqueta legible del item que falló, eligiendo la
        /// primera propiedad poblada. Útil para el log: por ejemplo
        /// <c>"folio=RM7230"</c> o <c>"empresa_id=1294"</c>.
        /// </summary>
        public string DescribirItem()
        {
            if (!string.IsNullOrEmpty(folio)) return "folio=" + folio;
            if (proveedor_id > 0) return "proveedor_id=" + proveedor_id;
            if (almacen_id   > 0) return "almacen_id="   + almacen_id;
            if (moneda_id    > 0) return "moneda_id="    + moneda_id;
            if (empresa_id   > 0) return "empresa_id="   + empresa_id;
            return "(sin id)";
        }
    }
}
