namespace PortalProveedoresConfigurador.Formularios
{
    partial class FormReSincronizar
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
            this.lblHdrModulo = new System.Windows.Forms.Label();
            this.lblHdrDesde = new System.Windows.Forms.Label();
            this.lblHdrUltimo = new System.Windows.Forms.Label();
            this.chkAlmacenes = new System.Windows.Forms.CheckBox();
            this.dtpAlmacenes = new System.Windows.Forms.DateTimePicker();
            this.lblMaxAlmacenes = new System.Windows.Forms.Label();
            this.chkMonedas = new System.Windows.Forms.CheckBox();
            this.dtpMonedas = new System.Windows.Forms.DateTimePicker();
            this.lblMaxMonedas = new System.Windows.Forms.Label();
            this.chkProveedores = new System.Windows.Forms.CheckBox();
            this.dtpProveedores = new System.Windows.Forms.DateTimePicker();
            this.lblMaxProveedores = new System.Windows.Forms.Label();
            this.chkRecepciones = new System.Windows.Forms.CheckBox();
            this.dtpRecepciones = new System.Windows.Forms.DateTimePicker();
            this.lblMaxRecepciones = new System.Windows.Forms.Label();
            this.chkCreditos = new System.Windows.Forms.CheckBox();
            this.dtpCreditos = new System.Windows.Forms.DateTimePicker();
            this.lblMaxCreditos = new System.Windows.Forms.Label();
            this.chkNotas = new System.Windows.Forms.CheckBox();
            this.dtpNotas = new System.Windows.Forms.DateTimePicker();
            this.lblMaxNotas = new System.Windows.Forms.Label();
            this.lblEstado = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // === lblTitulo ======================================================
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 13F);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTitulo.Location = new System.Drawing.Point(28, 22);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Text = "Re-sincronizar desde una fecha";

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
            this.lblAyuda.Location = new System.Drawing.Point(28, 82);
            this.lblAyuda.Name = "lblAyuda";
            this.lblAyuda.Size = new System.Drawing.Size(520, 50);
            this.lblAyuda.Text = "Marca solo los módulos que quieres volver a traer de Microsip. El servicio los re-jala una sola vez en su siguiente ciclo y luego vuelve solo al incremental normal. La fecha no puede ser posterior al último dato que ya tiene el portal.";

            // === Encabezados ====================================================
            this.lblHdrModulo.AutoSize = true;
            this.lblHdrModulo.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.lblHdrModulo.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblHdrModulo.Location = new System.Drawing.Point(28, 142);
            this.lblHdrModulo.Name = "lblHdrModulo";
            this.lblHdrModulo.Text = "Módulo";

            this.lblHdrDesde.AutoSize = true;
            this.lblHdrDesde.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.lblHdrDesde.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblHdrDesde.Location = new System.Drawing.Point(190, 142);
            this.lblHdrDesde.Name = "lblHdrDesde";
            this.lblHdrDesde.Text = "Re-sincronizar desde";

            this.lblHdrUltimo.AutoSize = true;
            this.lblHdrUltimo.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.lblHdrUltimo.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblHdrUltimo.Location = new System.Drawing.Point(398, 142);
            this.lblHdrUltimo.Name = "lblHdrUltimo";
            this.lblHdrUltimo.Text = "Último dato en el portal";

            // === Almacenes ======================================================
            this.chkAlmacenes.AutoSize = false;
            this.chkAlmacenes.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkAlmacenes.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.chkAlmacenes.Location = new System.Drawing.Point(28, 169);
            this.chkAlmacenes.Name = "chkAlmacenes";
            this.chkAlmacenes.Size = new System.Drawing.Size(150, 24);
            this.chkAlmacenes.Text = "Almacenes";
            this.chkAlmacenes.CheckedChanged += new System.EventHandler(this.chkModulo_CheckedChanged);

            this.dtpAlmacenes.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.dtpAlmacenes.Enabled = false;
            this.dtpAlmacenes.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpAlmacenes.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpAlmacenes.Location = new System.Drawing.Point(190, 168);
            this.dtpAlmacenes.Name = "dtpAlmacenes";
            this.dtpAlmacenes.Size = new System.Drawing.Size(190, 26);

            this.lblMaxAlmacenes.AutoSize = false;
            this.lblMaxAlmacenes.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxAlmacenes.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblMaxAlmacenes.Location = new System.Drawing.Point(398, 173);
            this.lblMaxAlmacenes.Name = "lblMaxAlmacenes";
            this.lblMaxAlmacenes.Size = new System.Drawing.Size(160, 20);
            this.lblMaxAlmacenes.Text = "—";

            // === Monedas ========================================================
            this.chkMonedas.AutoSize = false;
            this.chkMonedas.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkMonedas.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.chkMonedas.Location = new System.Drawing.Point(28, 207);
            this.chkMonedas.Name = "chkMonedas";
            this.chkMonedas.Size = new System.Drawing.Size(150, 24);
            this.chkMonedas.Text = "Monedas";
            this.chkMonedas.CheckedChanged += new System.EventHandler(this.chkModulo_CheckedChanged);

            this.dtpMonedas.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.dtpMonedas.Enabled = false;
            this.dtpMonedas.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpMonedas.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpMonedas.Location = new System.Drawing.Point(190, 206);
            this.dtpMonedas.Name = "dtpMonedas";
            this.dtpMonedas.Size = new System.Drawing.Size(190, 26);

            this.lblMaxMonedas.AutoSize = false;
            this.lblMaxMonedas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxMonedas.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblMaxMonedas.Location = new System.Drawing.Point(398, 211);
            this.lblMaxMonedas.Name = "lblMaxMonedas";
            this.lblMaxMonedas.Size = new System.Drawing.Size(160, 20);
            this.lblMaxMonedas.Text = "—";

            // === Proveedores ====================================================
            this.chkProveedores.AutoSize = false;
            this.chkProveedores.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkProveedores.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.chkProveedores.Location = new System.Drawing.Point(28, 245);
            this.chkProveedores.Name = "chkProveedores";
            this.chkProveedores.Size = new System.Drawing.Size(150, 24);
            this.chkProveedores.Text = "Proveedores";
            this.chkProveedores.CheckedChanged += new System.EventHandler(this.chkModulo_CheckedChanged);

            this.dtpProveedores.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.dtpProveedores.Enabled = false;
            this.dtpProveedores.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpProveedores.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpProveedores.Location = new System.Drawing.Point(190, 244);
            this.dtpProveedores.Name = "dtpProveedores";
            this.dtpProveedores.Size = new System.Drawing.Size(190, 26);

            this.lblMaxProveedores.AutoSize = false;
            this.lblMaxProveedores.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxProveedores.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblMaxProveedores.Location = new System.Drawing.Point(398, 249);
            this.lblMaxProveedores.Name = "lblMaxProveedores";
            this.lblMaxProveedores.Size = new System.Drawing.Size(160, 20);
            this.lblMaxProveedores.Text = "—";

            // === Recepciones ====================================================
            this.chkRecepciones.AutoSize = false;
            this.chkRecepciones.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkRecepciones.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.chkRecepciones.Location = new System.Drawing.Point(28, 283);
            this.chkRecepciones.Name = "chkRecepciones";
            this.chkRecepciones.Size = new System.Drawing.Size(150, 24);
            this.chkRecepciones.Text = "Recepciones";
            this.chkRecepciones.CheckedChanged += new System.EventHandler(this.chkModulo_CheckedChanged);

            this.dtpRecepciones.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.dtpRecepciones.Enabled = false;
            this.dtpRecepciones.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpRecepciones.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpRecepciones.Location = new System.Drawing.Point(190, 282);
            this.dtpRecepciones.Name = "dtpRecepciones";
            this.dtpRecepciones.Size = new System.Drawing.Size(190, 26);

            this.lblMaxRecepciones.AutoSize = false;
            this.lblMaxRecepciones.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxRecepciones.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblMaxRecepciones.Location = new System.Drawing.Point(398, 287);
            this.lblMaxRecepciones.Name = "lblMaxRecepciones";
            this.lblMaxRecepciones.Size = new System.Drawing.Size(160, 20);
            this.lblMaxRecepciones.Text = "—";

            // === Créditos =======================================================
            this.chkCreditos.AutoSize = false;
            this.chkCreditos.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkCreditos.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.chkCreditos.Location = new System.Drawing.Point(28, 321);
            this.chkCreditos.Name = "chkCreditos";
            this.chkCreditos.Size = new System.Drawing.Size(150, 24);
            this.chkCreditos.Text = "Créditos";
            this.chkCreditos.CheckedChanged += new System.EventHandler(this.chkModulo_CheckedChanged);

            this.dtpCreditos.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.dtpCreditos.Enabled = false;
            this.dtpCreditos.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpCreditos.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpCreditos.Location = new System.Drawing.Point(190, 320);
            this.dtpCreditos.Name = "dtpCreditos";
            this.dtpCreditos.Size = new System.Drawing.Size(190, 26);

            this.lblMaxCreditos.AutoSize = false;
            this.lblMaxCreditos.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxCreditos.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblMaxCreditos.Location = new System.Drawing.Point(398, 325);
            this.lblMaxCreditos.Name = "lblMaxCreditos";
            this.lblMaxCreditos.Size = new System.Drawing.Size(160, 20);
            this.lblMaxCreditos.Text = "—";

            // === Notas ==========================================================
            this.chkNotas.AutoSize = false;
            this.chkNotas.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkNotas.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.chkNotas.Location = new System.Drawing.Point(28, 359);
            this.chkNotas.Name = "chkNotas";
            this.chkNotas.Size = new System.Drawing.Size(150, 24);
            this.chkNotas.Text = "Notas de crédito";
            this.chkNotas.CheckedChanged += new System.EventHandler(this.chkModulo_CheckedChanged);

            this.dtpNotas.CustomFormat = "dd/MM/yyyy  HH:mm";
            this.dtpNotas.Enabled = false;
            this.dtpNotas.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpNotas.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpNotas.Location = new System.Drawing.Point(190, 358);
            this.dtpNotas.Name = "dtpNotas";
            this.dtpNotas.Size = new System.Drawing.Size(190, 26);

            this.lblMaxNotas.AutoSize = false;
            this.lblMaxNotas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxNotas.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblMaxNotas.Location = new System.Drawing.Point(398, 363);
            this.lblMaxNotas.Name = "lblMaxNotas";
            this.lblMaxNotas.Size = new System.Drawing.Size(160, 20);
            this.lblMaxNotas.Text = "—";

            // === lblEstado ======================================================
            this.lblEstado.AutoSize = false;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstado.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstado.Location = new System.Drawing.Point(28, 400);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(520, 38);
            this.lblEstado.Text = "";

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
            this.btnCancelar.Location = new System.Drawing.Point(300, 448);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(120, 36);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;

            // === btnGuardar =====================================================
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.Enabled = false;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(428, 448);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(120, 36);
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);

            // === Form ===========================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(576, 506);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.lblEstado);
            this.Controls.Add(this.lblMaxNotas);
            this.Controls.Add(this.dtpNotas);
            this.Controls.Add(this.chkNotas);
            this.Controls.Add(this.lblMaxCreditos);
            this.Controls.Add(this.dtpCreditos);
            this.Controls.Add(this.chkCreditos);
            this.Controls.Add(this.lblMaxRecepciones);
            this.Controls.Add(this.dtpRecepciones);
            this.Controls.Add(this.chkRecepciones);
            this.Controls.Add(this.lblMaxProveedores);
            this.Controls.Add(this.dtpProveedores);
            this.Controls.Add(this.chkProveedores);
            this.Controls.Add(this.lblMaxMonedas);
            this.Controls.Add(this.dtpMonedas);
            this.Controls.Add(this.chkMonedas);
            this.Controls.Add(this.lblMaxAlmacenes);
            this.Controls.Add(this.dtpAlmacenes);
            this.Controls.Add(this.chkAlmacenes);
            this.Controls.Add(this.lblHdrUltimo);
            this.Controls.Add(this.lblHdrDesde);
            this.Controls.Add(this.lblHdrModulo);
            this.Controls.Add(this.lblAyuda);
            this.Controls.Add(this.lblEmpresa);
            this.Controls.Add(this.lblTitulo);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormReSincronizar";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configurador";
            this.Load += new System.EventHandler(this.FormReSincronizar_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormReSincronizar_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblEmpresa;
        private System.Windows.Forms.Label lblAyuda;
        private System.Windows.Forms.Label lblHdrModulo;
        private System.Windows.Forms.Label lblHdrDesde;
        private System.Windows.Forms.Label lblHdrUltimo;
        private System.Windows.Forms.CheckBox chkAlmacenes;
        private System.Windows.Forms.DateTimePicker dtpAlmacenes;
        private System.Windows.Forms.Label lblMaxAlmacenes;
        private System.Windows.Forms.CheckBox chkMonedas;
        private System.Windows.Forms.DateTimePicker dtpMonedas;
        private System.Windows.Forms.Label lblMaxMonedas;
        private System.Windows.Forms.CheckBox chkProveedores;
        private System.Windows.Forms.DateTimePicker dtpProveedores;
        private System.Windows.Forms.Label lblMaxProveedores;
        private System.Windows.Forms.CheckBox chkRecepciones;
        private System.Windows.Forms.DateTimePicker dtpRecepciones;
        private System.Windows.Forms.Label lblMaxRecepciones;
        private System.Windows.Forms.CheckBox chkCreditos;
        private System.Windows.Forms.DateTimePicker dtpCreditos;
        private System.Windows.Forms.Label lblMaxCreditos;
        private System.Windows.Forms.CheckBox chkNotas;
        private System.Windows.Forms.DateTimePicker dtpNotas;
        private System.Windows.Forms.Label lblMaxNotas;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnGuardar;
    }
}
