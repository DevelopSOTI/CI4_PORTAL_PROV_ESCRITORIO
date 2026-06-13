using System;
using System.Drawing;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Modal pequeño que pide la contraseña de SYSDBA de Firebird antes de
    /// abrir el Configurador desde el Escritorio. No valida nada por sí mismo:
    /// solo captura la contraseña y la expone en <see cref="Password"/>. La
    /// validación (conectar a CONFIG.FDB) la hace el llamador (FormPrincipal).
    ///
    /// Construido todo en código (sin Designer) a propósito, para no chocar con
    /// la regla del Designer estricto del proyecto.
    /// </summary>
    public sealed class FormPasswordSysdba : Form
    {
        private TextBox _txtPassword;

        /// <summary>Contraseña capturada. Solo es significativa si el diálogo cerró con OK.</summary>
        public string Password { get; private set; } = "";

        public FormPasswordSysdba()
        {
            ConstruirUi();
        }

        private void ConstruirUi()
        {
            // --- Form ---
            this.Text            = "Acceso al Configurador";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.ShowInTaskbar   = false;
            this.ClientSize      = new Size(420, 170);
            this.Font            = new Font("Segoe UI", 9F);

            // --- Label de instrucción ---
            var lbl = new Label
            {
                AutoSize  = false,
                Location  = new Point(18, 18),
                Size      = new Size(384, 44),
                Text      = "Ingrese la contraseña de SYSDBA de Firebird para abrir el Configurador.",
            };

            // --- TextBox de contraseña ---
            _txtPassword = new TextBox
            {
                Location              = new Point(18, 70),
                Size                  = new Size(384, 24),
                UseSystemPasswordChar = true,
            };

            // --- Botones ---
            var btnAceptar = new Button
            {
                Text         = "Aceptar",
                DialogResult = DialogResult.OK,
                Location     = new Point(246, 118),
                Size         = new Size(75, 28),
                BackColor    = Tema.Primary,
                ForeColor    = Color.White,
                FlatStyle    = FlatStyle.Flat,
            };
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += BtnAceptar_Click;

            var btnCancelar = new Button
            {
                Text         = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location     = new Point(327, 118),
                Size         = new Size(75, 28),
                FlatStyle    = FlatStyle.Flat,
            };

            this.Controls.Add(lbl);
            this.Controls.Add(_txtPassword);
            this.Controls.Add(btnAceptar);
            this.Controls.Add(btnCancelar);

            // Enter = Aceptar, Esc = Cancelar.
            this.AcceptButton = btnAceptar;
            this.CancelButton = btnCancelar;

            this.Shown += (s, e) => _txtPassword.Focus();
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            // Capturamos la contraseña antes de que el diálogo se cierre con OK.
            Password = _txtPassword.Text ?? "";
        }
    }
}
