using System;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresCore.Servicios
{
    /// <summary>
    /// Envía el correo de "factura recibida, pasa a pendiente de pago" al
    /// proveedor justo después de que la factura se aplicó con éxito en
    /// Microsip. Réplica del SOAP <c>C_FUNCIONES.PROCESO_ENVIAR</c> de
    /// <c>C:\Users\Desarrollo\source\repos\PortalProveedores\PortalProveedores\C_FUNCIONES.cs:607-656</c>,
    /// invocado por <c>F_APLICAR_FACTURA.cs:985-990</c> (flujo con recepción)
    /// y <c>F_APLICAR_FACTURA.cs:1673-1677</c> (flujo sin recepción) cuando
    /// <c>reg.MAILS_SEND == "True"</c>.
    ///
    /// El servicio Delphi tenía el mismo flujo en PROCESO_ENVIAR
    /// (Func.pas:453-504), disparado tras la aplicación automática exitosa en
    /// Func_Facturas_3_3.pas:1248-1252 si el registro Windows MAILS_SEND='True'.
    /// Mismo asunto ('Contra recibo electronico') y mismo texto del cuerpo
    /// (Func.pas:481-483); solo difieren los saltos de línea (#13#13/#13 en
    /// Delphi vs " \r\n\n"/" \r\n" del SOAP, que es el que replicamos).
    /// Datos: FolioFac=FOLIO_COMPRA, FechaFac=FECHA_FACTURA dd/mm/yyyy,
    /// FechaProv=FECHA_RECEPCION dd/mm/yyyy (Func_Facturas_3_3.pas:497-499).
    /// El Delphi NO enviaba este correo para complementos (PROCESO_ENVIAR solo
    /// se invoca desde Func_Facturas_3_2/3_3) — aquí tampoco.
    ///
    /// Decisiones de portabilidad respecto al SOAP:
    /// <list type="bullet">
    ///   <item>SMTP: el SOAP lo lee con <c>ws.SelectCargarCorreo()</c> (tabla MAIL del
    ///         portal). El Escritorio nuevo usa exactamente la misma fuente vía
    ///         <see cref="IPortalApi.ObtenerCorreoConfigAsync"/>.</item>
    ///   <item>Correo del proveedor: el SOAP lo lee de Firebird
    ///         (<c>proveedores.EMAIL</c> y <c>libres_proveedor.CORREO_CXC</c>),
    ///         prefiriendo <c>CORREO_CXC</c>. En el portal nuevo el correo
    ///         autoritativo vive en <c>ACCESO.CORREO</c> (MySQL) y lo expone
    ///         <see cref="IPortalApi.ObtenerCorreoProveedorAsync"/>. Es la única
    ///         dirección que el operador del portal mantiene actualizada — si
    ///         el proveedor la cambió, el legacy mandaba al correo viejo de Microsip.</item>
    ///   <item>Best-effort: el SOAP envuelve el SMTP en try/catch y nunca aborta el
    ///         flujo de aplicación. Replicamos exactamente esa política — si falla,
    ///         la factura sigue aplicada y solo se reporta en la UI.</item>
    /// </list>
    /// </summary>
    public sealed class NotificadorAplicacion
    {
        private readonly IPortalApi _api;
        private readonly EnvioCorreo _envio;

        public NotificadorAplicacion(IPortalApi api, EnvioCorreo envio = null)
        {
            _api   = api   ?? throw new ArgumentNullException(nameof(api));
            _envio = envio ?? new EnvioCorreo();
        }

        /// <summary>
        /// Resultado del intento de notificación. <c>Mensaje</c> describe qué
        /// pasó en lenguaje natural (para mostrar al operador en el banner).
        /// </summary>
        public sealed class ResultadoNotificacion
        {
            public bool   Enviado    { get; set; }
            public string Destino    { get; set; }
            public string Mensaje    { get; set; }
        }

        /// <summary>
        /// Envía el correo. Si no hay destinatario o no hay SMTP, devuelve
        /// <c>Enviado=false</c> con <c>Mensaje</c> explicativo — NO lanza.
        /// </summary>
        /// <param name="proveedorId">PROVEEDOR_ID del portal (= FacturaAplicar.PROVEEDOR_ID).</param>
        /// <param name="empresaId">EMP_ID_MSP — la empresa autorizada.</param>
        /// <param name="folioCompra">Folio del CFDI del proveedor (FacturaAplicar.FOLIO_COMPRA).</param>
        /// <param name="fechaFactura">Fecha del CFDI (string ISO, como lo entrega el portal).</param>
        /// <param name="fechaRecepcion">Fecha estimada de pago a mostrar al proveedor.</param>
        public async Task<ResultadoNotificacion> NotificarFacturaAplicadaAsync(
            int proveedorId,
            int empresaId,
            string folioCompra,
            string fechaFactura,
            string fechaRecepcion,
            CancellationToken ct)
        {
            var r = new ResultadoNotificacion();

            // 1) Config SMTP del portal (misma fuente que ws.SelectCargarCorreo() del SOAP).
            CorreoConfig cfg = null;
            try
            {
                cfg = await _api.ObtenerCorreoConfigAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                r.Mensaje = "No se pudo obtener la configuración SMTP del portal: " + ex.Message;
                return r;
            }

            if (cfg == null
                || string.IsNullOrWhiteSpace(cfg.smtp)
                || string.IsNullOrWhiteSpace(cfg.from)
                || cfg.port <= 0)
            {
                r.Mensaje = "Configuración SMTP del portal incompleta.";
                return r;
            }

            // 2) Correo del proveedor (ACCESO.CORREO del portal).
            string destino = null;
            try
            {
                destino = await _api
                    .ObtenerCorreoProveedorAsync(proveedorId, empresaId, ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                r.Mensaje = "No se pudo obtener el correo del proveedor: " + ex.Message;
                return r;
            }

            if (string.IsNullOrWhiteSpace(destino))
            {
                r.Mensaje = "El proveedor no tiene correo registrado en el portal.";
                return r;
            }
            r.Destino = destino.Trim();

            // 3) Asunto y cuerpo LITERALES del SOAP
            //    (C_FUNCIONES.cs:636-640 + C_FUNCIONES.cs:645/650 para el subject).
            //    Texto plano (no HTML) — el SOAP usa email.Body sin IsBodyHtml=true.
            const string asunto = "Contra recibo electronico";

            string fechaFacCorta   = FormatearFechaCorta(fechaFactura);
            string fechaRecepCorta = FormatearFechaCorta(fechaRecepcion);

            // Cadena LITERAL del SOAP C_FUNCIONES.cs:636-640:
            //   string mensaje = "Estimado proveedor \r\n\n";
            //   mensaje += "Le notificamos que la factura " + folioFac + " con fecha " + fechaFac.ToShortDateString();
            //   mensaje += " fue recibida y paso a pendiente de pago, la fecha estimada de pago seria el dia ";
            //   mensaje += fechaProv + " \r\n";
            //   mensaje += "Favor de verificar el estatus de la factura en el portal de proveedores.";
            var mensaje =
                "Estimado proveedor \r\n\n"
                + "Le notificamos que la factura " + (folioCompra ?? "") + " con fecha " + fechaFacCorta
                + " fue recibida y paso a pendiente de pago, la fecha estimada de pago seria el dia "
                + fechaRecepCorta + " \r\n"
                + "Favor de verificar el estatus de la factura en el portal de proveedores.";

            // 4) Envío SMTP — best-effort, captura cualquier excepción.
            try
            {
                var env = await _envio
                    .EnviarAsync(cfg, r.Destino, asunto, mensaje, esHtml: false)
                    .ConfigureAwait(false);

                r.Enviado = env != null && env.Exito;
                r.Mensaje = env != null ? env.Mensaje : "Sin respuesta del envío SMTP.";
            }
            catch (Exception ex)
            {
                r.Enviado = false;
                r.Mensaje = "Error al enviar correo: " + ex.Message;
            }

            return r;
        }

        /// <summary>
        /// Mejor esfuerzo para emular <c>DateTime.ToShortDateString()</c> con el
        /// string ISO que entrega el portal. Si no parsea, devuelve el string
        /// tal cual — el SOAP también imprimía sin formato (concat directo del
        /// objeto DateTime para <c>fechaProv</c>).
        /// </summary>
        private static string FormatearFechaCorta(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha)) return "";
            DateTime dt;
            if (DateTime.TryParse(fecha, out dt))
                return dt.ToShortDateString();
            return fecha;
        }
    }
}
