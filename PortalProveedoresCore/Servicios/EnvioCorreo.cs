using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresCore.Servicios
{
    /// <summary>
    /// Wrapper sobre <see cref="SmtpClient"/> que envía un correo de rechazo
    /// usando la configuración SMTP del portal (tabla MAIL, traída por
    /// <c>IPortalApi.ObtenerCorreoConfigAsync</c>).
    ///
    /// Réplica funcional del helper SOAP <c>F_ENVIAR_RECHAZO.EnviarMensaje</c>:
    /// SMTP con SSL, credenciales explícitas (NO UseDefaultCredentials),
    /// HTML soportado para el body.
    /// </summary>
    public sealed class EnvioCorreo
    {
        /// <summary>
        /// Envía un correo a uno o varios destinatarios. Lista separada por
        /// <c>;</c> igual que el SOAP (rtDest.Text.Split(';')). Devuelve true
        /// si todos los destinatarios válidos recibieron el correo; false si
        /// el envío falló.
        /// </summary>
        public Task<EnvioCorreoResultado> EnviarAsync(
            CorreoConfig config,
            string destinatarios,
            string asunto,
            string mensaje,
            bool   esHtml)
        {
            return Task.Run(() => EnviarSinc(config, destinatarios, asunto, mensaje, esHtml));
        }

        private static EnvioCorreoResultado EnviarSinc(
            CorreoConfig config,
            string destinatarios,
            string asunto,
            string mensaje,
            bool   esHtml)
        {
            var r = new EnvioCorreoResultado();

            if (config == null
                || string.IsNullOrWhiteSpace(config.smtp)
                || string.IsNullOrWhiteSpace(config.from)
                || config.port <= 0)
            {
                r.Mensaje = "Configuración SMTP del portal incompleta.";
                return r;
            }
            if (string.IsNullOrWhiteSpace(destinatarios))
            {
                r.Mensaje = "Sin destinatarios.";
                return r;
            }
            if (string.IsNullOrWhiteSpace(asunto))
            {
                r.Mensaje = "Asunto vacío.";
                return r;
            }

            try
            {
                using (var email = new MailMessage())
                using (var smtp  = new SmtpClient(config.smtp, config.port))
                {
                    var nombreRemitente = string.IsNullOrEmpty(config.name)
                        ? "Portal de Proveedores"
                        : config.name;
                    email.From       = new MailAddress(config.from, nombreRemitente);
                    email.Subject    = asunto;
                    email.Body       = mensaje ?? "";
                    email.IsBodyHtml = esHtml;
                    email.Priority   = MailPriority.Normal;

                    int valid = 0;
                    foreach (var dest in destinatarios.Split(';'))
                    {
                        var limpio = (dest ?? "").Trim();
                        if (limpio.Length == 0) continue;
                        try
                        {
                            email.To.Add(limpio);
                            valid++;
                        }
                        catch
                        {
                            // dirección mal formada — la ignoramos para no
                            // tumbar el envío entero.
                        }
                    }

                    if (valid == 0)
                    {
                        r.Mensaje = "Ninguna dirección de correo válida.";
                        return r;
                    }

                    smtp.EnableSsl             = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.DeliveryMethod        = SmtpDeliveryMethod.Network;
                    smtp.Credentials           = new NetworkCredential(config.from, config.pass);
                    smtp.Send(email);

                    r.Exito          = true;
                    r.Destinatarios  = valid;
                    r.Mensaje        = "Correo enviado a " + valid + " destinatario(s).";
                }
            }
            catch (Exception ex)
            {
                r.Exito    = false;
                r.Mensaje  = "Error al enviar correo: " + ex.Message;
            }

            return r;
        }
    }

    public sealed class EnvioCorreoResultado
    {
        public bool   Exito         { get; set; }
        public int    Destinatarios { get; set; }
        public string Mensaje       { get; set; } = "";
    }
}
