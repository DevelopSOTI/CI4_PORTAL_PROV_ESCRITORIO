namespace PortalProveedoresEscritorio.Formularios
{
    partial class FormSelectorEmpresa
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel    panelTitleBar;
        private System.Windows.Forms.Label    btnCerrar;
        private System.Windows.Forms.Label    lblTitulo;
        private System.Windows.Forms.Label    lblSubtitulo;
        private System.Windows.Forms.ListBox  listEmpresas;
        private System.Windows.Forms.Button   btnAceptar;
        private System.Windows.Forms.Button   btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.btnCerrar     = new System.Windows.Forms.Label();
            this.lblTitulo     = new System.Windows.Forms.Label();
            this.lblSubtitulo  = new System.Windows.Forms.Label();
            this.listEmpresas  = new System.Windows.Forms.ListBox();
            this.btnAceptar    = new System.Windows.Forms.Button();
            this.btnCancelar   = new System.Windows.Forms.Button();
            this.panelTitleBar.SuspendLayout();
            this.SuspendLayout();

            // ---- Form ----
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor           = System.Drawing.Color.FromArgb(15, 23, 42);
            this.ClientSize          = new System.Drawing.Size(440, 480);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Seleccionar empresa";
            this.Font                = new System.Drawing.Font("Segoe UI", 9.5F);

            // ---- Title bar ----
            this.panelTitleBar.BackColor = System.Drawing.Color.Transparent;
            this.panelTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Size      = new System.Drawing.Size(440, 36);
            this.panelTitleBar.Controls.Add(this.btnCerrar);

            this.btnCerrar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location  = new System.Drawing.Point(404, 0);
            this.btnCerrar.Size      = new System.Drawing.Size(36, 36);
            this.btnCerrar.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(160, 180, 200);
            this.btnCerrar.Text      = "✕";

            // ---- Título + subtítulo ----
            this.lblTitulo.Location  = new System.Drawing.Point(32, 50);
            this.lblTitulo.Size      = new System.Drawing.Size(376, 32);
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 15F);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Text      = "Selecciona una empresa";

            this.lblSubtitulo.Location  = new System.Drawing.Point(32, 85);
            this.lblSubtitulo.Size      = new System.Drawing.Size(376, 20);
            this.lblSubtitulo.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(160, 180, 200);
            this.lblSubtitulo.Text      = "Confirma con qué empresa vas a trabajar";

            // ---- ListBox ----
            this.listEmpresas.Location    = new System.Drawing.Point(32, 125);
            this.listEmpresas.Size        = new System.Drawing.Size(376, 240);
            this.listEmpresas.BackColor   = System.Drawing.Color.FromArgb(45, 55, 75);
            this.listEmpresas.ForeColor   = System.Drawing.Color.White;
            this.listEmpresas.Font        = new System.Drawing.Font("Segoe UI", 10.5F);
            this.listEmpresas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listEmpresas.IntegralHeight = false;
            this.listEmpresas.ItemHeight  = 28;
            this.listEmpresas.TabIndex    = 0;
            this.listEmpresas.DoubleClick += new System.EventHandler(this.listEmpresas_DoubleClick);

            // ---- Botones ----
            this.btnCancelar.Location  = new System.Drawing.Point(32, 395);
            this.btnCancelar.Size      = new System.Drawing.Size(180, 44);
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(45, 55, 75);
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(180, 200, 220);
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.Font      = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.btnCancelar.Text      = "Cancelar";
            this.btnCancelar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.TabIndex  = 2;
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click    += new System.EventHandler(this.btnCancelar_Click);

            this.btnAceptar.Location  = new System.Drawing.Point(228, 395);
            this.btnAceptar.Size      = new System.Drawing.Size(180, 44);
            this.btnAceptar.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnAceptar.ForeColor = System.Drawing.Color.White;
            this.btnAceptar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAceptar.FlatAppearance.BorderSize = 0;
            this.btnAceptar.Font      = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.btnAceptar.Text      = "Aceptar";
            this.btnAceptar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAceptar.TabIndex  = 1;
            this.btnAceptar.UseVisualStyleBackColor = false;
            this.btnAceptar.Click    += new System.EventHandler(this.btnAceptar_Click);

            // ---- Compose ----
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.listEmpresas);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.panelTitleBar);

            this.AcceptButton = this.btnAceptar;
            this.CancelButton = this.btnCancelar;

            this.panelTitleBar.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
