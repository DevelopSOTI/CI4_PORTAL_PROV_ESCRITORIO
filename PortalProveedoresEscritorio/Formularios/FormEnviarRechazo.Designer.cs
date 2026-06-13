namespace PortalProveedoresEscritorio.Formularios
{
    partial class FormEnviarRechazo
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel       panelTitleBar;
        private System.Windows.Forms.Label       lblTitulo;
        private System.Windows.Forms.Label       btnMinimizar;
        private System.Windows.Forms.Label       btnCerrar;

        private System.Windows.Forms.Panel       panelBody;
        private System.Windows.Forms.Panel       sec1Card;
        private System.Windows.Forms.Label       sec1Titulo;
        private System.Windows.Forms.Label       sec1Hint;
        private System.Windows.Forms.Label       lbl_Folio;
        private System.Windows.Forms.TextBox     txtFolio;
        private System.Windows.Forms.Label       lbl_Proveedor;
        private System.Windows.Forms.TextBox     txtProveedor;
        private System.Windows.Forms.Label       lbl_Asunto;
        private System.Windows.Forms.TextBox     txtAsunto;
        private System.Windows.Forms.Label       lbl_Destinatarios;
        private System.Windows.Forms.TextBox     txtDestinatarios;
        private System.Windows.Forms.Label       lblHintDest;
        private System.Windows.Forms.Label       lbl_Motivo;
        private System.Windows.Forms.TextBox     txtMotivo;

        private System.Windows.Forms.Panel       panelEstado;
        private System.Windows.Forms.Label       lblEstado;
        private System.Windows.Forms.ProgressBar barProgreso;

        private System.Windows.Forms.Panel       panelBotones;
        private System.Windows.Forms.Button      btnRechazar;
        private System.Windows.Forms.Button      btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components       = new System.ComponentModel.Container();
            this.panelTitleBar    = new System.Windows.Forms.Panel();
            this.lblTitulo        = new System.Windows.Forms.Label();
            this.btnMinimizar     = new System.Windows.Forms.Label();
            this.btnCerrar        = new System.Windows.Forms.Label();

            this.panelBody        = new System.Windows.Forms.Panel();
            this.sec1Card         = new System.Windows.Forms.Panel();
            this.sec1Titulo       = new System.Windows.Forms.Label();
            this.sec1Hint         = new System.Windows.Forms.Label();
            this.lbl_Folio        = new System.Windows.Forms.Label();
            this.txtFolio         = new System.Windows.Forms.TextBox();
            this.lbl_Proveedor    = new System.Windows.Forms.Label();
            this.txtProveedor     = new System.Windows.Forms.TextBox();
            this.lbl_Asunto       = new System.Windows.Forms.Label();
            this.txtAsunto        = new System.Windows.Forms.TextBox();
            this.lbl_Destinatarios= new System.Windows.Forms.Label();
            this.txtDestinatarios = new System.Windows.Forms.TextBox();
            this.lblHintDest      = new System.Windows.Forms.Label();
            this.lbl_Motivo       = new System.Windows.Forms.Label();
            this.txtMotivo        = new System.Windows.Forms.TextBox();

            this.panelEstado      = new System.Windows.Forms.Panel();
            this.lblEstado        = new System.Windows.Forms.Label();
            this.barProgreso      = new System.Windows.Forms.ProgressBar();

            this.panelBotones     = new System.Windows.Forms.Panel();
            this.btnRechazar      = new System.Windows.Forms.Button();
            this.btnCancelar      = new System.Windows.Forms.Button();

            this.panelTitleBar.SuspendLayout();
            this.panelBody.SuspendLayout();
            this.sec1Card.SuspendLayout();
            this.panelEstado.SuspendLayout();
            this.panelBotones.SuspendLayout();
            this.SuspendLayout();

            // ============ Form ============
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor           = System.Drawing.Color.FromArgb(241, 245, 249);
            this.ClientSize          = new System.Drawing.Size(700, 660);
            this.MinimumSize         = new System.Drawing.Size(700, 660);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Rechazar y enviar correo";
            this.Font                = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ShowInTaskbar       = false;

            // ============ Title bar ============
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(239, 68, 68); // rojo — acción destructiva
            this.panelTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Size      = new System.Drawing.Size(700, 44);
            this.panelTitleBar.Controls.Add(this.lblTitulo);
            this.panelTitleBar.Controls.Add(this.btnMinimizar);
            this.panelTitleBar.Controls.Add(this.btnCerrar);

            this.lblTitulo.Location  = new System.Drawing.Point(16, 0);
            this.lblTitulo.Size      = new System.Drawing.Size(550, 44);
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitulo.Text      = "Rechazar factura y enviar correo al proveedor";

            this.btnMinimizar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimizar.Location  = new System.Drawing.Point(628, 0);
            this.btnMinimizar.Size      = new System.Drawing.Size(36, 44);
            this.btnMinimizar.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnMinimizar.ForeColor = System.Drawing.Color.White;
            this.btnMinimizar.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimizar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnMinimizar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnMinimizar.Text      = "─";

            this.btnCerrar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location  = new System.Drawing.Point(664, 0);
            this.btnCerrar.Size      = new System.Drawing.Size(36, 44);
            this.btnCerrar.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.BackColor = System.Drawing.Color.Transparent;
            this.btnCerrar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnCerrar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.Text      = "✕";

            // ============ Body ============
            this.panelBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.panelBody.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelBody.Padding   = new System.Windows.Forms.Padding(20);

            // ============ Card ============
            this.sec1Card.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.sec1Card.BackColor = System.Drawing.Color.White;
            this.sec1Card.Padding   = new System.Windows.Forms.Padding(24, 16, 24, 16);
            this.sec1Card.Controls.Add(this.txtMotivo);
            this.sec1Card.Controls.Add(this.lbl_Motivo);
            this.sec1Card.Controls.Add(this.lblHintDest);
            this.sec1Card.Controls.Add(this.txtDestinatarios);
            this.sec1Card.Controls.Add(this.lbl_Destinatarios);
            this.sec1Card.Controls.Add(this.txtAsunto);
            this.sec1Card.Controls.Add(this.lbl_Asunto);
            this.sec1Card.Controls.Add(this.txtProveedor);
            this.sec1Card.Controls.Add(this.lbl_Proveedor);
            this.sec1Card.Controls.Add(this.txtFolio);
            this.sec1Card.Controls.Add(this.lbl_Folio);
            this.sec1Card.Controls.Add(this.sec1Hint);
            this.sec1Card.Controls.Add(this.sec1Titulo);

            this.sec1Titulo.Location  = new System.Drawing.Point(24, 16);
            this.sec1Titulo.Size      = new System.Drawing.Size(600, 22);
            this.sec1Titulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.sec1Titulo.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.sec1Titulo.Text      = "Datos del rechazo";

            this.sec1Hint.Location  = new System.Drawing.Point(24, 40);
            this.sec1Hint.Size      = new System.Drawing.Size(610, 32);
            this.sec1Hint.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.sec1Hint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.sec1Hint.Text      = "Esta acción marca el documento como RECHAZADO en el portal y le envía un correo al proveedor con el motivo. No se aplica nada en Microsip.";

            // --- Folio ---
            this.lbl_Folio.Location  = new System.Drawing.Point(24, 84);
            this.lbl_Folio.Size      = new System.Drawing.Size(120, 22);
            this.lbl_Folio.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Folio.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_Folio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Folio.Text      = "Folio";
            this.txtFolio.Location    = new System.Drawing.Point(144, 80);
            this.txtFolio.Size        = new System.Drawing.Size(490, 25);
            this.txtFolio.Font        = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.txtFolio.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtFolio.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtFolio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFolio.ReadOnly    = true;
            this.txtFolio.Text        = "";

            // --- Proveedor ---
            this.lbl_Proveedor.Location  = new System.Drawing.Point(24, 116);
            this.lbl_Proveedor.Size      = new System.Drawing.Size(120, 22);
            this.lbl_Proveedor.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Proveedor.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_Proveedor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Proveedor.Text      = "Proveedor";
            this.txtProveedor.Location    = new System.Drawing.Point(144, 112);
            this.txtProveedor.Size        = new System.Drawing.Size(490, 25);
            this.txtProveedor.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProveedor.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtProveedor.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtProveedor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtProveedor.ReadOnly    = true;
            this.txtProveedor.Text        = "";

            // --- Asunto ---
            this.lbl_Asunto.Location  = new System.Drawing.Point(24, 156);
            this.lbl_Asunto.Size      = new System.Drawing.Size(120, 22);
            this.lbl_Asunto.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Asunto.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lbl_Asunto.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Asunto.Text      = "Asunto*";
            this.txtAsunto.Location    = new System.Drawing.Point(144, 152);
            this.txtAsunto.Size        = new System.Drawing.Size(490, 25);
            this.txtAsunto.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtAsunto.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtAsunto.BackColor   = System.Drawing.Color.White;
            this.txtAsunto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAsunto.Text        = "";

            // --- Destinatarios ---
            this.lbl_Destinatarios.Location  = new System.Drawing.Point(24, 196);
            this.lbl_Destinatarios.Size      = new System.Drawing.Size(120, 22);
            this.lbl_Destinatarios.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Destinatarios.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lbl_Destinatarios.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Destinatarios.Text      = "Destinatarios*";
            this.txtDestinatarios.Location    = new System.Drawing.Point(144, 192);
            this.txtDestinatarios.Size        = new System.Drawing.Size(490, 25);
            this.txtDestinatarios.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtDestinatarios.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtDestinatarios.BackColor   = System.Drawing.Color.White;
            this.txtDestinatarios.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDestinatarios.Text        = "";

            this.lblHintDest.Location  = new System.Drawing.Point(144, 220);
            this.lblHintDest.Size      = new System.Drawing.Size(490, 18);
            this.lblHintDest.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblHintDest.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblHintDest.Text      = "Separa varios correos con ;";

            // --- Motivo ---
            this.lbl_Motivo.Location  = new System.Drawing.Point(24, 252);
            this.lbl_Motivo.Size      = new System.Drawing.Size(120, 22);
            this.lbl_Motivo.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Motivo.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lbl_Motivo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Motivo.Text      = "Motivo*";
            this.txtMotivo.Location    = new System.Drawing.Point(144, 248);
            this.txtMotivo.Size        = new System.Drawing.Size(490, 220);
            this.txtMotivo.Font        = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtMotivo.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtMotivo.BackColor   = System.Drawing.Color.White;
            this.txtMotivo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMotivo.Multiline   = true;
            this.txtMotivo.ScrollBars  = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMotivo.Text        = "";

            // ============ Panel estado ============
            this.panelEstado.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelEstado.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.panelEstado.Size      = new System.Drawing.Size(700, 56);
            this.panelEstado.Padding   = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.panelEstado.Controls.Add(this.lblEstado);
            this.panelEstado.Controls.Add(this.barProgreso);

            this.lblEstado.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblEstado.Height    = 28;
            this.lblEstado.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblEstado.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblEstado.Text      = "Listo para rechazar.";

            this.barProgreso.Dock     = System.Windows.Forms.DockStyle.Top;
            this.barProgreso.Height   = 8;
            this.barProgreso.Style    = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.barProgreso.Visible  = false;

            // ============ Botones ============
            this.panelBotones.BackColor = System.Drawing.Color.White;
            this.panelBotones.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotones.Size      = new System.Drawing.Size(700, 68);
            this.panelBotones.Padding   = new System.Windows.Forms.Padding(20, 14, 20, 14);
            this.panelBotones.Controls.Add(this.btnRechazar);
            this.panelBotones.Controls.Add(this.btnCancelar);

            this.btnCancelar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location  = new System.Drawing.Point(370, 14);
            this.btnCancelar.Size      = new System.Drawing.Size(120, 40);
            this.btnCancelar.BackColor = System.Drawing.Color.White;
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnCancelar.FlatAppearance.BorderSize  = 1;
            this.btnCancelar.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancelar.Text      = "Cancelar";
            this.btnCancelar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click    += new System.EventHandler(this.btnCancelar_Click);

            this.btnRechazar.Anchor     = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRechazar.Location   = new System.Drawing.Point(498, 14);
            this.btnRechazar.Size       = new System.Drawing.Size(182, 40);
            this.btnRechazar.BackColor  = System.Drawing.Color.FromArgb(239, 68, 68);
            this.btnRechazar.ForeColor  = System.Drawing.Color.White;
            this.btnRechazar.FlatStyle  = System.Windows.Forms.FlatStyle.Flat;
            this.btnRechazar.FlatAppearance.BorderSize = 0;
            this.btnRechazar.Font       = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnRechazar.Text       = "✉  Rechazar y enviar";
            this.btnRechazar.Cursor     = System.Windows.Forms.Cursors.Hand;
            this.btnRechazar.UseVisualStyleBackColor = false;
            this.btnRechazar.Click     += new System.EventHandler(this.btnRechazar_Click);

            // ============ Compose ============
            this.panelBody.Controls.Add(this.sec1Card);

            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelEstado);
            this.Controls.Add(this.panelBotones);
            this.Controls.Add(this.panelTitleBar);

            this.AcceptButton = this.btnRechazar;
            this.CancelButton = this.btnCancelar;

            this.panelTitleBar.ResumeLayout(false);
            this.panelBody.ResumeLayout(false);
            this.sec1Card.ResumeLayout(false);
            this.sec1Card.PerformLayout();
            this.panelEstado.ResumeLayout(false);
            this.panelBotones.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
