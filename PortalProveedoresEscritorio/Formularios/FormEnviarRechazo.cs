using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Servicios;
using PortalProveedoresEscritorio.Servicios;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Modal "Rechazar factura/complemento y enviar correo". Réplica
    /// funcional del SOAP <c>F_ENVIAR_RECHAZO</c>:
    /// <list type="number">
    ///   <item>El operador captura asunto + destinatarios + motivo.</item>
    ///   <item>Se llama al endpoint REST (factura o complemento) que marca
    ///         el documento como rechazado en MySQL.</item>
    ///   <item>Se envía el correo al proveedor con la config SMTP del
    ///         portal (tabla MAIL).</item>
    /// </list>
    /// Si el correo falla DESPUÉS de marcar el rechazo, se notifica al
    /// operador pero el rechazo se mantiene — mismo patrón que el SOAP
    /// (F_ENVIAR_RECHAZO.cs:82-86).
    /// </summary>
    public partial class FormEnviarRechazo : Form
    {
        public enum TipoDocumento { Factura, Complemento }

        private readonly IPortalApi    _api;
        private readonly EnvioCorreo   _envio = new EnvioCorreo();
        private readonly TipoDocumento _tipo;
        private readonly int           _docId;        // DOCTO_CM_ID (id MySQL de la factura) para facturas, DOCTO_CP_ID para complementos — réplica F_ENVIAR_RECHAZO.cs:69 (RECHAZA_FACTURA WHERE DOCTO_CM_ID)
        private readonly string        _usuario;
        private readonly string        _folio;
        private readonly string        _proveedor;
        private readonly string        _correoSugerido; // de PROVEEDORES.MAIL

        public bool   Rechazado  { get; private set; }
        public string MotivoUsado{ get; private set; }

        public FormEnviarRechazo(IPortalApi api,
                                 TipoDocumento tipo,
                                 int docId,
                                 string usuario,
                                 string folio,
                                 string proveedor,
                                 string correoSugerido)
        {
            _api            = api ?? throw new ArgumentNullException(nameof(api));
            _tipo           = tipo;
            _docId          = docId;
            _usuario        = usuario   ?? "";
            _folio          = folio     ?? "";
            _proveedor      = proveedor ?? "";
            _correoSugerido = correoSugerido ?? "";

            InitializeComponent();
            AplicarTemaYHandlers();
            LlenarValoresIniciales();
        }

        private void AplicarTemaYHandlers()
        {
            // Title bar y botón Rechazar quedan en rojo (acción destructiva).
            // Los botones cerrar/minimizar con texto blanco — sobre fondo
            // rojo contrastan bien.
            UiHelpers.AplicarEsquinasRedondeadas(this, 10);
            UiHelpers.EngancharDragNativo(this.panelTitleBar, this);
            UiHelpers.EngancharDragNativo(this.lblTitulo,     this);

            Color iconoClaro = Color.FromArgb(245, 230, 230);
            UiHelpers.ConfigurarBotonCerrar(
                this.btnCerrar, iconoClaro, () => CancelarYCerrar());
            UiHelpers.ConfigurarBotonMinimizar(this.btnMinimizar, iconoClaro, this);

            // Borde fino del card.
            this.sec1Card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0,
                        this.sec1Card.Width - 1, this.sec1Card.Height - 1);
            };
            this.panelEstado.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, 0, this.panelEstado.Width, 0);
            };
            this.panelBotones.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, 0, this.panelBotones.Width, 0);
            };
        }

        private void LlenarValoresIniciales()
        {
            this.lblTitulo.Text = _tipo == TipoDocumento.Complemento
                ? "Rechazar CFDI del complemento y enviar correo"
                : "Rechazar factura y enviar correo al proveedor";

            this.txtFolio.Text        = _folio;
            this.txtProveedor.Text    = _proveedor;
            this.txtDestinatarios.Text= _correoSugerido;

            // Asunto y motivo con un valor inicial razonable que el operador
            // puede ajustar. Inspirado en el patrón típico del SOAP.
            this.txtAsunto.Text = _tipo == TipoDocumento.Complemento
                ? "Rechazo de complemento de pago — " + _folio
                : "Rechazo de factura — " + _folio;

            this.txtMotivo.Text = "Estimado proveedor,\r\n\r\n"
                + "Le informamos que se ha rechazado su "
                + (_tipo == TipoDocumento.Complemento ? "complemento de pago" : "factura")
                + " con folio " + _folio + ".\r\n\r\n"
                + "Motivo del rechazo:\r\n[Describe aquí el motivo]\r\n\r\n"
                + "Saludos.";
        }

        // ====================================================================
        // Rechazar
        // ====================================================================

        private async void btnRechazar_Click(object sender, EventArgs e)
        {
            // Validaciones literales del SOAP F_ENVIAR_RECHAZO.cs:37-48.
            if (string.IsNullOrWhiteSpace(this.txtAsunto.Text))
            {
                MessageBox.Show(
                    "Debe contener Asunto\r\nintentelo de nuevo",
                    "Mensaje de la aplicación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtAsunto.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(this.txtMotivo.Text))
            {
                MessageBox.Show(
                    "El campo de motivo de rechazo debe contener información.",
                    "Mensaje de la aplicación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtMotivo.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(this.txtDestinatarios.Text))
            {
                MessageBox.Show(
                    "Debe indicar al menos un destinatario para el correo.",
                    "Mensaje de la aplicación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtDestinatarios.Focus();
                return;
            }

            btnRechazar.Enabled = false;
            btnCancelar.Enabled = false;
            barProgreso.Visible = true;
            barProgreso.Style   = ProgressBarStyle.Marquee;
            barProgreso.MarqueeAnimationSpeed = 30;

            var ct = CancellationToken.None;

            // === Paso 1: marcar rechazo en MySQL ===
            MostrarEstado("Marcando rechazo en el portal…", EstadoTipo.Trabajando);
            bool rechazoOk;
            try
            {
                if (_tipo == TipoDocumento.Factura)
                    rechazoOk = await _api.RechazarFacturaAsync(_docId, _usuario, this.txtMotivo.Text, ct)
                        .ConfigureAwait(true);
                else
                    rechazoOk = await _api.RechazarComplementoAsync(_docId, _usuario, this.txtMotivo.Text, ct)
                        .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                rechazoOk = false;
                MostrarEstado("Error al marcar el rechazo: " + ex.Message, EstadoTipo.Error);
            }

            if (!rechazoOk)
            {
                // Texto literal del SOAP F_ENVIAR_RECHAZO.cs:92.
                MessageBox.Show(
                    "Hubo un error al rechazar la factura\r\nFavor de contactar con el adminsitrador del sistema",
                    "Error al rechazar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RestaurarBotones();
                return;
            }

            Rechazado   = true;
            MotivoUsado = this.txtMotivo.Text;

            // === Paso 2: enviar correo ===
            MostrarEstado("Enviando correo al proveedor…", EstadoTipo.Trabajando);

            CorreoConfig conf = null;
            try
            {
                conf = await _api.ObtenerCorreoConfigAsync(ct).ConfigureAwait(true);
            }
            catch (Exception)
            {
                // sin conf, no podemos enviar — manejamos abajo.
            }

            EnvioCorreoResultado envio = new EnvioCorreoResultado
            {
                Exito   = false,
                Mensaje = "No se pudo obtener la configuración SMTP del portal.",
            };
            if (conf != null)
            {
                envio = await _envio.EnviarAsync(
                    conf,
                    this.txtDestinatarios.Text,
                    this.txtAsunto.Text,
                    this.txtMotivo.Text,
                    esHtml: false
                ).ConfigureAwait(true);
            }

            // Réplica del SOAP F_ENVIAR_RECHAZO.cs:77-86: el rechazo en el
            // portal SE MANTIENE aunque el correo falle. Se notifica al
            // operador con MessageBox distinto.
            if (envio.Exito)
            {
                MostrarEstado(
                    "Rechazado correctamente. " + envio.Mensaje,
                    EstadoTipo.Exito);
                MessageBox.Show(
                    "Mensaje de rechazo enviado al proveedor\r\nFactura "
                    + _folio + " rechazada con exito",
                    "Rechazado",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MostrarEstado(
                    "Rechazado, pero el correo falló: " + envio.Mensaje,
                    EstadoTipo.Error);
                MessageBox.Show(
                    "Se rechazo con exito la factura " + _folio
                    + " pero hubo un error al notificar por correo",
                    "Rechada exitosamente",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void RestaurarBotones()
        {
            btnRechazar.Enabled = true;
            btnCancelar.Enabled = true;
            barProgreso.Visible = false;
        }

        // ====================================================================
        // Cerrar / cancelar
        // ====================================================================

        private void btnCancelar_Click(object sender, EventArgs e) => CancelarYCerrar();

        private void CancelarYCerrar()
        {
            this.DialogResult = Rechazado ? DialogResult.OK : DialogResult.Cancel;
            this.Close();
        }

        // ====================================================================
        // Helpers
        // ====================================================================

        private enum EstadoTipo { Trabajando, Exito, Error }

        private void MostrarEstado(string mensaje, EstadoTipo tipo)
        {
            this.lblEstado.Text = mensaje;
            switch (tipo)
            {
                case EstadoTipo.Trabajando:
                    this.panelEstado.BackColor = Color.FromArgb(241, 245, 249);
                    this.lblEstado.ForeColor   = Color.FromArgb(51, 65, 85);
                    break;
                case EstadoTipo.Exito:
                    this.panelEstado.BackColor = Color.FromArgb(220, 252, 231);
                    this.lblEstado.ForeColor   = Color.FromArgb(22, 101, 52);
                    break;
                case EstadoTipo.Error:
                    this.panelEstado.BackColor = Color.FromArgb(254, 226, 226);
                    this.lblEstado.ForeColor   = Color.FromArgb(153, 27, 27);
                    break;
            }
        }
    }
}
