namespace PortalProveedoresVisor.Formularios
{
    partial class FormVisor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        // REGLAS (mismas que en el Configurador, ver CLAUDE.md):
        //  - Sin lambdas.
        //  - Sin llamadas a métodos propios dentro de InitializeComponent.
        //  - Todos los colores inline para que el Diseñador renderice.

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.lblTituloApp = new System.Windows.Forms.Label();
            this.lblEstadoLinea = new System.Windows.Forms.Label();
            this.progresoCiclo = new System.Windows.Forms.ProgressBar();

            this.pnlBody = new System.Windows.Forms.Panel();
            this.rtbLog = new System.Windows.Forms.RichTextBox();

            this.pnlFooter = new System.Windows.Forms.Panel();
            this.chkAutoScroll = new System.Windows.Forms.CheckBox();
            this.lblFiltro = new System.Windows.Forms.Label();
            this.txtFiltro = new System.Windows.Forms.TextBox();
            this.lblNivel = new System.Windows.Forms.Label();
            this.cmbNivel = new System.Windows.Forms.ComboBox();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();

            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuTray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniAbrir = new System.Windows.Forms.ToolStripMenuItem();
            this.mniSeparator0 = new System.Windows.Forms.ToolStripSeparator();
            this.mniForzarCiclo = new System.Windows.Forms.ToolStripMenuItem();
            this.mniPausar = new System.Windows.Forms.ToolStripMenuItem();
            this.mniReanudar = new System.Windows.Forms.ToolStripMenuItem();
            this.mniSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mniMantenerArriba = new System.Windows.Forms.ToolStripMenuItem();
            this.mniSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mniSalir = new System.Windows.Forms.ToolStripMenuItem();

            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();

            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.pnlBody.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.menuTray.SuspendLayout();
            this.SuspendLayout();

            // === pnlHeader (cabecera con logo + título + estado + progress) =====
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.pnlHeader.Controls.Add(this.progresoCiclo);
            this.pnlHeader.Controls.Add(this.lblEstadoLinea);
            this.pnlHeader.Controls.Add(this.lblTituloApp);
            this.pnlHeader.Controls.Add(this.pbLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(24, 18, 24, 16);
            this.pnlHeader.Size = new System.Drawing.Size(980, 96);

            this.pbLogo.BackColor = System.Drawing.Color.Transparent;
            this.pbLogo.Location = new System.Drawing.Point(24, 18);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(48, 48);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLogo.TabStop = false;
            this.pbLogo.Visible = false; // se enciende en AplicarTema cuando hay logo

            this.lblTituloApp.AutoSize = true;
            this.lblTituloApp.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.lblTituloApp.ForeColor = System.Drawing.Color.White;
            this.lblTituloApp.Location = new System.Drawing.Point(24, 18);
            this.lblTituloApp.Name = "lblTituloApp";
            this.lblTituloApp.Text = "Visor del Portal de Proveedores";

            this.lblEstadoLinea.AutoSize = true;
            this.lblEstadoLinea.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblEstadoLinea.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblEstadoLinea.Location = new System.Drawing.Point(24, 48);
            this.lblEstadoLinea.Name = "lblEstadoLinea";
            this.lblEstadoLinea.Text = "Estado: conectando al servicio…";

            this.progresoCiclo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.progresoCiclo.Location = new System.Drawing.Point(640, 38);
            this.progresoCiclo.MarqueeAnimationSpeed = 36;
            this.progresoCiclo.Maximum = 100;
            this.progresoCiclo.Minimum = 0;
            this.progresoCiclo.Name = "progresoCiclo";
            this.progresoCiclo.Size = new System.Drawing.Size(312, 16);
            // Continuous por default; en runtime cambiamos a Marquee si el
            // servicio no nos manda total_pasos (versiones viejas del Service).
            this.progresoCiclo.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progresoCiclo.Visible = false; // solo se ve mientras dura un ciclo

            // === pnlBody (área del log) =========================================
            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(2, 6, 23);
            this.pnlBody.Controls.Add(this.rtbLog);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 96);
            this.pnlBody.Name = "pnlBody";
            // Padding generoso: el texto no debe pegarse a los bordes del panel.
            this.pnlBody.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);
            this.pnlBody.Size = new System.Drawing.Size(980, 494);

            this.rtbLog.BackColor = System.Drawing.Color.FromArgb(2, 6, 23);
            this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Font = new System.Drawing.Font("Cascadia Mono", 9.5F);
            this.rtbLog.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.rtbLog.HideSelection = false;
            this.rtbLog.Location = new System.Drawing.Point(20, 12);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbLog.Size = new System.Drawing.Size(940, 470);
            this.rtbLog.Text = "Esperando eventos del servicio...";
            // Word wrap activado: líneas largas envuelven en lugar de salirse de
            // pantalla. Las continuaciones tienen hanging indent para alinearse
            // con la columna del mensaje (ver UI_AgregarLineaLog).
            this.rtbLog.WordWrap = true;

            // === pnlFooter (controles inferiores) ==============================
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.pnlFooter.Controls.Add(this.btnGuardar);
            this.pnlFooter.Controls.Add(this.btnLimpiar);
            this.pnlFooter.Controls.Add(this.cmbNivel);
            this.pnlFooter.Controls.Add(this.lblNivel);
            this.pnlFooter.Controls.Add(this.txtFiltro);
            this.pnlFooter.Controls.Add(this.lblFiltro);
            this.pnlFooter.Controls.Add(this.chkAutoScroll);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 590);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(24, 12, 24, 12);
            this.pnlFooter.Size = new System.Drawing.Size(980, 50);

            this.chkAutoScroll.AutoSize = true;
            this.chkAutoScroll.Checked = true;
            this.chkAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoScroll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkAutoScroll.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkAutoScroll.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.chkAutoScroll.Location = new System.Drawing.Point(24, 15);
            this.chkAutoScroll.Name = "chkAutoScroll";
            this.chkAutoScroll.Text = "Auto-scroll";
            this.chkAutoScroll.UseVisualStyleBackColor = false;

            this.lblFiltro.AutoSize = true;
            this.lblFiltro.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFiltro.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblFiltro.Location = new System.Drawing.Point(140, 16);
            this.lblFiltro.Name = "lblFiltro";
            this.lblFiltro.Text = "Filtro:";

            this.txtFiltro.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.txtFiltro.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFiltro.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.txtFiltro.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.txtFiltro.Location = new System.Drawing.Point(180, 13);
            this.txtFiltro.Name = "txtFiltro";
            this.txtFiltro.Size = new System.Drawing.Size(220, 24);

            this.lblNivel.AutoSize = true;
            this.lblNivel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblNivel.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblNivel.Location = new System.Drawing.Point(420, 16);
            this.lblNivel.Name = "lblNivel";
            this.lblNivel.Text = "Nivel:";

            this.cmbNivel.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.cmbNivel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNivel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbNivel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbNivel.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.cmbNivel.Items.AddRange(new object[] { "Info", "Warning", "Error", "Solo errores" });
            this.cmbNivel.Location = new System.Drawing.Point(460, 13);
            this.cmbNivel.Name = "cmbNivel";
            this.cmbNivel.Size = new System.Drawing.Size(120, 26);

            this.btnLimpiar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLimpiar.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.btnLimpiar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLimpiar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnLimpiar.FlatAppearance.BorderSize = 1;
            this.btnLimpiar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnLimpiar.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnLimpiar.Location = new System.Drawing.Point(720, 11);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(108, 28);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.UseVisualStyleBackColor = false;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);

            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(835, 11);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(120, 28);
            this.btnGuardar.Text = "Guardar log…";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);

            // === NotifyIcon + Menú del tray =====================================
            this.notifyIcon.ContextMenuStrip = this.menuTray;
            this.notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            this.notifyIcon.Text = "Visor del Portal de Proveedores";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);

            this.menuTray.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.menuTray.ForeColor = System.Drawing.Color.White;
            this.menuTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mniAbrir,
                this.mniSeparator0,
                this.mniForzarCiclo,
                this.mniPausar,
                this.mniReanudar,
                this.mniSeparator1,
                this.mniMantenerArriba,
                this.mniSeparator2,
                this.mniSalir});
            this.menuTray.Name = "menuTray";
            this.menuTray.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;

            this.mniAbrir.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.mniAbrir.ForeColor = System.Drawing.Color.White;
            this.mniAbrir.Name = "mniAbrir";
            this.mniAbrir.Text = "Abrir visor";
            this.mniAbrir.Click += new System.EventHandler(this.mniAbrir_Click);

            this.mniSeparator0.Name = "mniSeparator0";

            this.mniForzarCiclo.ForeColor = System.Drawing.Color.White;
            this.mniForzarCiclo.Name = "mniForzarCiclo";
            this.mniForzarCiclo.Text = "Forzar ciclo ahora";
            this.mniForzarCiclo.Click += new System.EventHandler(this.mniForzarCiclo_Click);

            this.mniPausar.ForeColor = System.Drawing.Color.White;
            this.mniPausar.Name = "mniPausar";
            this.mniPausar.Text = "Pausar servicio";
            this.mniPausar.Click += new System.EventHandler(this.mniPausar_Click);

            this.mniReanudar.ForeColor = System.Drawing.Color.White;
            this.mniReanudar.Name = "mniReanudar";
            this.mniReanudar.Text = "Reanudar servicio";
            this.mniReanudar.Click += new System.EventHandler(this.mniReanudar_Click);

            this.mniSeparator1.Name = "mniSeparator1";

            this.mniMantenerArriba.CheckOnClick = true;
            this.mniMantenerArriba.ForeColor = System.Drawing.Color.White;
            this.mniMantenerArriba.Name = "mniMantenerArriba";
            this.mniMantenerArriba.Text = "Mantener al frente";
            this.mniMantenerArriba.Click += new System.EventHandler(this.mniMantenerArriba_Click);

            this.mniSeparator2.Name = "mniSeparator2";

            this.mniSalir.ForeColor = System.Drawing.Color.White;
            this.mniSalir.Name = "mniSalir";
            this.mniSalir.Text = "Salir";
            this.mniSalir.Click += new System.EventHandler(this.mniSalir_Click);

            // === SaveFileDialog (para Guardar log) ==============================
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.Filter = "Texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";

            // === Form ===========================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(2, 6, 23);
            this.ClientSize = new System.Drawing.Size(980, 640);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Icon = System.Drawing.SystemIcons.Application;
            this.MinimumSize = new System.Drawing.Size(820, 520);
            this.Name = "FormVisor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Visor del Portal de Proveedores";

            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.pnlBody.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.menuTray.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        // === Campos =========================================================

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Label lblTituloApp;
        private System.Windows.Forms.Label lblEstadoLinea;
        private System.Windows.Forms.ProgressBar progresoCiclo;

        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.RichTextBox rtbLog;

        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.CheckBox chkAutoScroll;
        private System.Windows.Forms.Label lblFiltro;
        private System.Windows.Forms.TextBox txtFiltro;
        private System.Windows.Forms.Label lblNivel;
        private System.Windows.Forms.ComboBox cmbNivel;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnGuardar;

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip menuTray;
        private System.Windows.Forms.ToolStripMenuItem mniAbrir;
        private System.Windows.Forms.ToolStripSeparator mniSeparator0;
        private System.Windows.Forms.ToolStripMenuItem mniForzarCiclo;
        private System.Windows.Forms.ToolStripMenuItem mniPausar;
        private System.Windows.Forms.ToolStripMenuItem mniReanudar;
        private System.Windows.Forms.ToolStripSeparator mniSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mniMantenerArriba;
        private System.Windows.Forms.ToolStripSeparator mniSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mniSalir;

        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}
