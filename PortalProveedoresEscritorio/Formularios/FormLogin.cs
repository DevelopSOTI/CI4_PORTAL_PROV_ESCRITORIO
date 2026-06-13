using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Servicios;
using PortalProveedoresEscritorio.Servicios;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Pantalla de login. Autentica al usuario contra CONFIG.FDB de
    /// Microsip y, si la autenticación es exitosa, pide la empresa antes
    /// de abrir <see cref="FormPrincipal"/>.
    ///
    /// El <c>.Designer.cs</c> mantiene solo valores literales y handlers
    /// nombrados (compatibles con el diseñador de Visual Studio). Toda la
    /// lógica que depende del <see cref="Tema"/> del portal o que requiere
    /// expresiones complejas vive en este archivo, en
    /// <see cref="AplicarTemaYHandlers"/>.
    /// </summary>
    public partial class FormLogin : Form
    {
        private RegistrosWindows _reg;

        public FormLogin()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Load   += FormLogin_Load;
            this.Shown  += FormLogin_Shown;
            this.Resize += FormLogin_Resize;

            AplicarTemaYHandlers();
        }

        /// <summary>
        /// Se ejecuta después de <c>InitializeComponent</c>. Sobrescribe los
        /// colores literales del Designer con los del Tema del portal,
        /// engancha drag nativo, configura hovers de los botones de title
        /// bar y aplica los redondeos.
        /// </summary>
        private void AplicarTemaYHandlers()
        {
            // Paleta del tema del portal
            Color fondoForm  = Tema.Secondary;
            Color fondoTitle = Tema.SecondaryHover;
            Color fondoInput = Tema.Aclarar(Tema.Secondary, 28);

            this.BackColor              = fondoForm;
            this.panelInputUsuario.BackColor  = fondoInput;
            this.txtUsuario.BackColor   = fondoInput;
            this.panelInputPassword.BackColor = fondoInput;
            this.txtPassword.BackColor  = fondoInput;
            this.btnAceptar.BackColor   = Tema.Primary;
            this.btnAceptar.FlatAppearance.MouseOverBackColor = Tema.PrimaryHover;
            this.btnAceptar.FlatAppearance.MouseDownBackColor = Tema.PrimaryHover;
            this.linkConfigurador.LinkColor       = Tema.Primary;
            this.linkConfigurador.ActiveLinkColor = Tema.PrimaryHover;
            this.lblNombrePortal.Text   = Tema.NombreApp;

            // Logo: real si vino del portal, placeholder con paint si no
            if (Tema.Logo != null)
                this.pictureBoxLogo.Image = Tema.Logo;
            else
                this.pictureBoxLogo.Paint += pictureBoxLogo_Paint;

            // Drag nativo del form arrastrando la title bar
            UiHelpers.EngancharDragNativo(this.panelTitleBar, this);

            // Botones de title bar (hover y click)
            Color textoTenue = Color.FromArgb(160, 180, 200);
            UiHelpers.ConfigurarBotonMinimizar(this.btnMinimizar, textoTenue, this);
            UiHelpers.ConfigurarBotonCerrar   (this.btnCerrar,    textoTenue, this.Close);

            // Botón principal con esquinas redondeadas
            this.btnAceptar.Paint += (s, e) => UiHelpers.DibujarBordePill(this.btnAceptar, 12);

            // Form + inputs con esquinas redondeadas (necesario también en Resize)
            ActualizarRegiones();
        }

        private void ActualizarRegiones()
        {
            UiHelpers.AplicarEsquinasRedondeadas(this, 16);
            UiHelpers.AplicarEsquinasRedondeadas(this.panelInputUsuario,  10);
            UiHelpers.AplicarEsquinasRedondeadas(this.panelInputPassword, 10);
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            _reg = new RegistrosWindows();

            if (!_reg.LeerRegistros(false))
            {
                if (MessageBox.Show(
                    "No hay configuración del Portal en HKLM. ¿Abrir el Configurador?",
                    "Configuración faltante",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    LanzarConfigurador();
                }
                return;
            }

            txtUsuario.Text = _reg.MICRO_USER1 ?? "";
            if (!string.IsNullOrEmpty(_reg.MICRO_PASS1))
            {
                txtPassword.Text = _reg.MICRO_PASS1;
                chkRecordar.Checked = true;
            }
        }

        private void FormLogin_Shown(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtUsuario.Text)) txtPassword.Focus();
            else                                       txtUsuario.Focus();
        }

        private void FormLogin_Resize(object sender, EventArgs e) => ActualizarRegiones();

        // ====================================================================
        // Validaciones + auth
        // ====================================================================

        private void txtUsuario_TextChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            errorProviderUsuario.Clear();
            btnAceptar.Enabled = !string.Equals(
                txtUsuario.Text.Trim(), "SYSDBA", StringComparison.OrdinalIgnoreCase);
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            errorProviderPassword.Clear();

            if (e.KeyCode == Keys.Enter
                && string.Equals(txtUsuario.Text.Trim(), "SYSDBA", StringComparison.OrdinalIgnoreCase)
                && txtPassword.Text == "masterkey")
            {
                linkConfigurador.Visible = true;
                e.SuppressKeyPress = true;
            }
        }

        private async void btnAceptar_Click(object sender, EventArgs e)
        {
            var usuario  = txtUsuario.Text.Trim().ToUpperInvariant();
            var password = txtPassword.Text;

            if (string.IsNullOrEmpty(usuario))
            {
                errorProviderUsuario.SetError(txtUsuario, "No puede estar vacio este campo");
                txtUsuario.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                errorProviderPassword.SetError(txtPassword, "No puede estar vacio este campo");
                txtPassword.Focus();
                return;
            }

            btnAceptar.Enabled = false;
            lblStatus.ForeColor = Color.FromArgb(200, 220, 255);
            lblStatus.Text = "Conectando a Microsip...";
            Application.DoEvents();

            var con = new ConexionMicrosip();
            string mensaje;
            if (!con.ConectarConfigPrueba(_reg.MICRO_SERVER, _reg.MICRO_ROOT, usuario, password, out mensaje))
            {
                // Texto y caption replicados literalmente del SOAP legacy
                // (F_Login.cs:205) — "Usuario inexistente en Microsip."
                lblStatus.ForeColor = Color.FromArgb(255, 180, 180);
                lblStatus.Text     = "Usuario inexistente en Microsip.";
                MessageBox.Show(this,
                    "Usuario inexistente en Microsip.",
                    "No fue posible iniciar sesión",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnAceptar.Enabled = true;
                con.Desconectar();
                return;
            }

            try
            {
                _reg.EscribirRegistros("MICRO_USER1", usuario, false);
                _reg.EscribirRegistros("MICRO_PASS1", chkRecordar.Checked ? password : "", false);
            }
            catch { }

            lblStatus.Text = "Cargando empresas...";
            Application.DoEvents();

            try
            {
                var api = new PortalApi(_reg.PORTAL_BASE_URL, _reg.PORTAL_API_KEY);
                var svc = new ServicioEmpresasMicrosip(api);
                var empresas = await svc.ObtenerAutorizadasAsync(con.FBC, usuario, CancellationToken.None).ConfigureAwait(true);

                con.Desconectar();

                if (empresas == null || empresas.Count == 0)
                {
                    lblStatus.ForeColor = Color.FromArgb(255, 180, 180);
                    lblStatus.Text = "Sin empresas autorizadas";
                    btnAceptar.Enabled = true;
                    return;
                }

                EmpresaEscritorio empresaElegida;
                this.Hide();
                using (var selector = new FormSelectorEmpresa(empresas))
                {
                    var r = selector.ShowDialog(this);
                    if (r != DialogResult.OK || selector.EmpresaSeleccionada == null)
                    {
                        this.Show();
                        lblStatus.Text = "";
                        btnAceptar.Enabled = true;
                        return;
                    }
                    empresaElegida = selector.EmpresaSeleccionada;
                }

                using (var principal = new FormPrincipal(usuario, password, empresas, empresaElegida, api))
                {
                    principal.ShowDialog(this);
                }
                this.Show();
                lblStatus.Text = "";
                btnAceptar.Enabled = true;
            }
            catch (Exception ex)
            {
                con.Desconectar();
                lblStatus.ForeColor = Color.FromArgb(255, 180, 180);
                lblStatus.Text = "Error: " + ex.Message;
                btnAceptar.Enabled = true;
            }
        }

        private void linkConfigurador_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LanzarConfigurador();
        }

        /// <summary>
        /// Logo placeholder cuando el portal no devolvió un logo. Dibuja un
        /// círculo translúcido con la inicial del nombre del portal.
        /// </summary>
        private void pictureBoxLogo_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, pictureBoxLogo.Width, pictureBoxLogo.Height);
            using (var brush = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                g.FillEllipse(brush, rect);
            using (var pen = new Pen(Color.FromArgb(120, 255, 255, 255), 2f))
                g.DrawEllipse(pen, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);

            var nombre = string.IsNullOrEmpty(Tema.NombreApp) ? "P" : Tema.NombreApp;
            var inicial = char.ToUpper(nombre[0]).ToString();

            using (var f = new Font("Segoe UI Semibold", 28F, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(inicial, f, Brushes.White, rect, sf);
        }

        private static void LanzarConfigurador()
        {
            try
            {
                var exe = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "PortalProveedoresConfigurador.exe");

                if (!System.IO.File.Exists(exe))
                {
                    MessageBox.Show(
                        "No se encontró 'PortalProveedoresConfigurador.exe' al lado del ejecutable.",
                        "Configurador no disponible",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                Process.Start(exe);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el Configurador: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
