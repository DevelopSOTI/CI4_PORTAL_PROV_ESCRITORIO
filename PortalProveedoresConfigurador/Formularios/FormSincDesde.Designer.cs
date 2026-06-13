namespace PortalProveedoresConfigurador.Formularios
{
    partial class FormSincDesde
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblEmpresa = new System.Windows.Forms.Label();
            this.lblAyuda = new System.Windows.Forms.Label();
            this.lblFecha = new System.Windows.Forms.Label();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.lblNullHint = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // === lblTitulo ======================================================
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 13F);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTitulo.Location = new System.Drawing.Point(28, 22);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Text = "Sincronizar documentos desde";

            // === lblEmpresa =====================================================
            this.lblEmpresa.AutoSize = true;
            this.lblEmpresa.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblEmpresa.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblEmpresa.Location = new System.Drawing.Point(28, 52);
            this.lblEmpresa.Name = "lblEmpresa";
            this.lblEmpresa.Text = "Empresa: —";

            // === lblAyuda =======================================================
            this.lblAyuda.AutoSize = false;
            this.lblAyuda.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAyuda.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblAyuda.Location = new System.Drawing.Point(28, 84);
            this.lblAyuda.Name = "lblAyuda";
            this.lblAyuda.Size = new System.Drawing.Size(430, 50);
            this.lblAyuda.Text = "Las recepciones, créditos y facturas de proveedores con FECHA_HORA_CREACION mayor o igual a la fecha elegida se sincronizarán al portal.";

            // === lblFecha =======================================================
            this.lblFecha.AutoSize = true;
            this.lblFecha.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFecha.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblFecha.Location = new System.Drawing.Point(28, 148);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Text = "Fecha y hora";

            // === dtpDesde =======================================================
            this.dtpDesde.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.dtpDesde.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDesde.Location = new System.Drawing.Point(28, 170);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.ShowCheckBox = true;
            this.dtpDesde.Size = new System.Drawing.Size(220, 26);

            // === lblNullHint ====================================================
            this.lblNullHint.AutoSize = false;
            this.lblNullHint.Font = new System.Drawing.Font("Segoe UI Italic", 8.5F);
            this.lblNullHint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblNullHint.Location = new System.Drawing.Point(28, 204);
            this.lblNullHint.Name = "lblNullHint";
            this.lblNullHint.Size = new System.Drawing.Size(430, 32);
            this.lblNullHint.Text = "Desmarca la casilla para sincronizar TODA la historia disponible (sin filtro). Recomendado para empresas con poca actividad histórica.";

            // === btnCancelar ====================================================
            this.btnCancelar.BackColor = System.Drawing.Color.White;
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnCancelar.FlatAppearance.BorderSize = 1;
            this.btnCancelar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnCancelar.Location = new System.Drawing.Point(218, 260);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(120, 36);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;

            // === btnAceptar =====================================================
            this.btnAceptar.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnAceptar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAceptar.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAceptar.FlatAppearance.BorderSize = 0;
            this.btnAceptar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnAceptar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAceptar.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnAceptar.ForeColor = System.Drawing.Color.White;
            this.btnAceptar.Location = new System.Drawing.Point(346, 260);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(120, 36);
            this.btnAceptar.Text = "Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = false;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);

            // === Form ===========================================================
            this.AcceptButton = this.btnAceptar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(494, 320);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.lblNullHint);
            this.Controls.Add(this.dtpDesde);
            this.Controls.Add(this.lblFecha);
            this.Controls.Add(this.lblAyuda);
            this.Controls.Add(this.lblEmpresa);
            this.Controls.Add(this.lblTitulo);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSincDesde";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configurador";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblEmpresa;
        private System.Windows.Forms.Label lblAyuda;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.Label lblNullHint;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnAceptar;
    }
}
