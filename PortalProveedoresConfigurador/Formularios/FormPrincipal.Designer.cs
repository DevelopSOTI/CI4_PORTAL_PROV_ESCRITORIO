namespace PortalProveedoresConfigurador.Formularios
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        // REGLAS para que el Diseñador de Visual Studio funcione:
        //  - Sin lambdas.
        //  - Sin llamadas a métodos propios dentro de InitializeComponent.
        //  - Sin referencias a campos static de esta clase dentro de
        //    InitializeComponent (todos los colores se ponen literales).
        // Repintar dinámico (Tema desde PORTAL_CONFIG) vive en FormPrincipal.cs
        // dentro de AplicarTema(), que corre en el Form_Load y sobrescribe
        // los valores literales que pone el Diseñador.

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlMarca = new System.Windows.Forms.Panel();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.lblMarca = new System.Windows.Forms.Label();
            this.lblMarcaSub = new System.Windows.Forms.Label();
            this.btnNavMicrosip = new System.Windows.Forms.Button();
            this.btnNavPortal = new System.Windows.Forms.Button();
            this.btnNavServicio = new System.Windows.Forms.Button();
            this.btnNavOtros = new System.Windows.Forms.Button();
            this.btnNavEmpresas = new System.Windows.Forms.Button();
            this.btnNavDias = new System.Windows.Forms.Button();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlSeccionMicrosip = new System.Windows.Forms.Panel();
            this.pnlSeccionPortal = new System.Windows.Forms.Panel();
            this.pnlSeccionServicio = new System.Windows.Forms.Panel();
            this.pnlSeccionOtros = new System.Windows.Forms.Panel();
            this.pnlSeccionEmpresas = new System.Windows.Forms.Panel();
            this.pnlSeccionDias = new System.Windows.Forms.Panel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblEstadoServicio = new System.Windows.Forms.ToolStripStatusLabel();
            this.sepStatus1 = new System.Windows.Forms.ToolStripSeparator();
            this.lblEstadoPortal = new System.Windows.Forms.ToolStripStatusLabel();
            this.sepStatus2 = new System.Windows.Forms.ToolStripSeparator();
            this.lblEstadoSync = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();

            this.cardMicrosip = new System.Windows.Forms.Panel();
            this.lblTituloMicrosip = new System.Windows.Forms.Label();
            this.lblHelpMicrosip = new System.Windows.Forms.Label();
            this.lblMicSrv = new System.Windows.Forms.Label();
            this.txtMicSrv = new System.Windows.Forms.TextBox();
            this.lblMicRoot = new System.Windows.Forms.Label();
            this.txtMicRoot = new System.Windows.Forms.TextBox();
            this.btnMicExaminar = new System.Windows.Forms.Button();
            this.lblMicUser = new System.Windows.Forms.Label();
            this.txtMicUser = new System.Windows.Forms.TextBox();
            this.lblMicPass = new System.Windows.Forms.Label();
            this.txtMicPass = new System.Windows.Forms.TextBox();
            this.btnProbarMicrosip = new System.Windows.Forms.Button();
            this.btnGuardarMicrosip = new System.Windows.Forms.Button();
            this.lblEstadoConexionMic = new System.Windows.Forms.Label();

            this.cardComportamiento = new System.Windows.Forms.Panel();
            this.lblTituloComp = new System.Windows.Forms.Label();
            this.lblSincCada = new System.Windows.Forms.Label();
            this.nudTimer = new System.Windows.Forms.NumericUpDown();
            this.cmbUnidadTimer = new System.Windows.Forms.ComboBox();
            this.lblTimerHelper = new System.Windows.Forms.Label();
            this.chkEnviarCorreo = new System.Windows.Forms.CheckBox();
            this.lblEnviarCorreoHelp = new System.Windows.Forms.Label();
            this.btnGuardarHKLM = new System.Windows.Forms.Button();

            this.cardParametros = new System.Windows.Forms.Panel();
            this.lblTituloParam = new System.Windows.Forms.Label();
            this.dgvParametros = new System.Windows.Forms.DataGridView();
            this.colClave = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescripcion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnGuardarParametros = new System.Windows.Forms.Button();

            this.cardPortal = new System.Windows.Forms.Panel();
            this.lblTituloPortal = new System.Windows.Forms.Label();
            this.lblHelpPortal = new System.Windows.Forms.Label();
            this.lblPortalUrl = new System.Windows.Forms.Label();
            this.txtPortalUrl = new System.Windows.Forms.TextBox();
            this.lblPortalApiKey = new System.Windows.Forms.Label();
            this.txtPortalApiKey = new System.Windows.Forms.TextBox();
            this.btnPortalToggle = new System.Windows.Forms.Button();
            this.btnProbarPortal = new System.Windows.Forms.Button();
            this.btnGuardarPortal = new System.Windows.Forms.Button();
            this.lblEstadoConexionPortal = new System.Windows.Forms.Label();
            this.cardServicio = new System.Windows.Forms.Panel();
            this.lblTituloServicio = new System.Windows.Forms.Label();
            this.lblHelpServicio = new System.Windows.Forms.Label();
            this.lblServiceName = new System.Windows.Forms.Label();
            this.txtServiceName = new System.Windows.Forms.TextBox();
            this.lblRutaArchivos = new System.Windows.Forms.Label();
            this.txtRutaArchivos = new System.Windows.Forms.TextBox();
            this.btnExaminarRuta = new System.Windows.Forms.Button();
            this.lblEstadoActualLabel = new System.Windows.Forms.Label();
            this.lblEstadoActualValor = new System.Windows.Forms.Label();
            this.btnRefrescarEstado = new System.Windows.Forms.Button();
            this.btnGuardarServicio = new System.Windows.Forms.Button();
            this.btnInstalarServicio = new System.Windows.Forms.Button();
            this.btnDesinstalarServicio = new System.Windows.Forms.Button();
            this.btnIniciarServicio = new System.Windows.Forms.Button();
            this.btnDetenerServicio = new System.Windows.Forms.Button();
            this.lblEstadoServicioMensaje = new System.Windows.Forms.Label();
            this.cardEmpresas = new System.Windows.Forms.Panel();
            this.lblTituloEmpresas = new System.Windows.Forms.Label();
            this.lblHelpEmpresas = new System.Windows.Forms.Label();
            this.lblBuscar = new System.Windows.Forms.Label();
            this.txtBuscarEmpresa = new System.Windows.Forms.TextBox();
            this.lblContadorEmpresas = new System.Windows.Forms.Label();
            this.dgvEmpresas = new System.Windows.Forms.DataGridView();
            this.colEmpIdMsp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmpNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmpNombreLargo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmpRfc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmpEstatus = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colEmpDiferencia = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colEmpSincDesde = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmpUltSinc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblEstadoEmpresas = new System.Windows.Forms.Label();
            this.cardDias = new System.Windows.Forms.Panel();
            this.lblTituloDias = new System.Windows.Forms.Label();
            this.lblHelpDias = new System.Windows.Forms.Label();
            this.chkDia1 = new System.Windows.Forms.CheckBox();
            this.chkDia2 = new System.Windows.Forms.CheckBox();
            this.chkDia3 = new System.Windows.Forms.CheckBox();
            this.chkDia4 = new System.Windows.Forms.CheckBox();
            this.chkDia5 = new System.Windows.Forms.CheckBox();
            this.chkDia6 = new System.Windows.Forms.CheckBox();
            this.chkDia7 = new System.Windows.Forms.CheckBox();
            this.btnGuardarDias = new System.Windows.Forms.Button();
            this.lblEstadoDias = new System.Windows.Forms.Label();

            this.pnlSidebar.SuspendLayout();
            this.pnlMarca.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.pnlContent.SuspendLayout();
            this.pnlSeccionMicrosip.SuspendLayout();
            this.pnlSeccionPortal.SuspendLayout();
            this.cardPortal.SuspendLayout();
            this.pnlSeccionServicio.SuspendLayout();
            this.cardServicio.SuspendLayout();
            this.pnlSeccionOtros.SuspendLayout();
            this.pnlSeccionEmpresas.SuspendLayout();
            this.cardEmpresas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmpresas)).BeginInit();
            this.pnlSeccionDias.SuspendLayout();
            this.cardDias.SuspendLayout();
            this.cardMicrosip.SuspendLayout();
            this.cardComportamiento.SuspendLayout();
            this.cardParametros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParametros)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();

            // === pnlSidebar =====================================================
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.pnlSidebar.Controls.Add(this.btnNavDias);
            this.pnlSidebar.Controls.Add(this.btnNavEmpresas);
            this.pnlSidebar.Controls.Add(this.btnNavOtros);
            this.pnlSidebar.Controls.Add(this.btnNavServicio);
            this.pnlSidebar.Controls.Add(this.btnNavPortal);
            this.pnlSidebar.Controls.Add(this.btnNavMicrosip);
            this.pnlSidebar.Controls.Add(this.pnlMarca);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlSidebar.Size = new System.Drawing.Size(230, 618);
            this.pnlSidebar.TabIndex = 0;

            // === pnlMarca =======================================================
            this.pnlMarca.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.pnlMarca.Controls.Add(this.lblMarcaSub);
            this.pnlMarca.Controls.Add(this.lblMarca);
            this.pnlMarca.Controls.Add(this.pbLogo);
            this.pnlMarca.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMarca.Location = new System.Drawing.Point(0, 0);
            this.pnlMarca.Name = "pnlMarca";
            this.pnlMarca.Padding = new System.Windows.Forms.Padding(20, 18, 20, 14);
            this.pnlMarca.Size = new System.Drawing.Size(230, 96);
            this.pnlMarca.TabIndex = 0;

            // === pbLogo =========================================================
            this.pbLogo.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.pbLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pbLogo.Location = new System.Drawing.Point(20, 18);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(190, 42);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLogo.TabIndex = 2;
            this.pbLogo.TabStop = false;
            this.pbLogo.Visible = false;

            // === lblMarca =======================================================
            this.lblMarca.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMarca.Font = new System.Drawing.Font("Segoe UI Semibold", 13F);
            this.lblMarca.ForeColor = System.Drawing.Color.White;
            this.lblMarca.Location = new System.Drawing.Point(20, 18);
            this.lblMarca.Name = "lblMarca";
            this.lblMarca.Size = new System.Drawing.Size(190, 24);
            this.lblMarca.TabIndex = 0;
            this.lblMarca.Text = "Portal de Proveedores";

            // === lblMarcaSub ====================================================
            this.lblMarcaSub.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMarcaSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMarcaSub.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblMarcaSub.Location = new System.Drawing.Point(20, 42);
            this.lblMarcaSub.Name = "lblMarcaSub";
            this.lblMarcaSub.Size = new System.Drawing.Size(190, 18);
            this.lblMarcaSub.TabIndex = 1;
            this.lblMarcaSub.Text = "Configurador";

            // === Botones del sidebar ============================================
            this.btnNavDias.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnNavDias.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavDias.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNavDias.FlatAppearance.BorderSize = 0;
            this.btnNavDias.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.btnNavDias.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavDias.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnNavDias.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnNavDias.Location = new System.Drawing.Point(0, 316);
            this.btnNavDias.Name = "btnNavDias";
            this.btnNavDias.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavDias.Size = new System.Drawing.Size(230, 44);
            this.btnNavDias.TabIndex = 6;
            this.btnNavDias.Text = "Días";
            this.btnNavDias.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavDias.UseVisualStyleBackColor = false;
            this.btnNavDias.Click += new System.EventHandler(this.btnNavDias_Click);

            this.btnNavEmpresas.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnNavEmpresas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavEmpresas.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNavEmpresas.FlatAppearance.BorderSize = 0;
            this.btnNavEmpresas.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.btnNavEmpresas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavEmpresas.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnNavEmpresas.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnNavEmpresas.Location = new System.Drawing.Point(0, 272);
            this.btnNavEmpresas.Name = "btnNavEmpresas";
            this.btnNavEmpresas.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavEmpresas.Size = new System.Drawing.Size(230, 44);
            this.btnNavEmpresas.TabIndex = 5;
            this.btnNavEmpresas.Text = "Empresas";
            this.btnNavEmpresas.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavEmpresas.UseVisualStyleBackColor = false;
            this.btnNavEmpresas.Click += new System.EventHandler(this.btnNavEmpresas_Click);

            this.btnNavOtros.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnNavOtros.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavOtros.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNavOtros.FlatAppearance.BorderSize = 0;
            this.btnNavOtros.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.btnNavOtros.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavOtros.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnNavOtros.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnNavOtros.Location = new System.Drawing.Point(0, 228);
            this.btnNavOtros.Name = "btnNavOtros";
            this.btnNavOtros.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavOtros.Size = new System.Drawing.Size(230, 44);
            this.btnNavOtros.TabIndex = 4;
            this.btnNavOtros.Text = "Parámetros";
            this.btnNavOtros.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavOtros.UseVisualStyleBackColor = false;
            this.btnNavOtros.Click += new System.EventHandler(this.btnNavOtros_Click);

            this.btnNavServicio.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnNavServicio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavServicio.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNavServicio.FlatAppearance.BorderSize = 0;
            this.btnNavServicio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.btnNavServicio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavServicio.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnNavServicio.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnNavServicio.Location = new System.Drawing.Point(0, 184);
            this.btnNavServicio.Name = "btnNavServicio";
            this.btnNavServicio.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavServicio.Size = new System.Drawing.Size(230, 44);
            this.btnNavServicio.TabIndex = 3;
            this.btnNavServicio.Text = "Servicio";
            this.btnNavServicio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavServicio.UseVisualStyleBackColor = false;
            this.btnNavServicio.Click += new System.EventHandler(this.btnNavServicio_Click);

            this.btnNavPortal.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnNavPortal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavPortal.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNavPortal.FlatAppearance.BorderSize = 0;
            this.btnNavPortal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.btnNavPortal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavPortal.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnNavPortal.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnNavPortal.Location = new System.Drawing.Point(0, 140);
            this.btnNavPortal.Name = "btnNavPortal";
            this.btnNavPortal.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavPortal.Size = new System.Drawing.Size(230, 44);
            this.btnNavPortal.TabIndex = 2;
            this.btnNavPortal.Text = "Portal Web";
            this.btnNavPortal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavPortal.UseVisualStyleBackColor = false;
            this.btnNavPortal.Click += new System.EventHandler(this.btnNavPortal_Click);

            this.btnNavMicrosip.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnNavMicrosip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavMicrosip.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnNavMicrosip.FlatAppearance.BorderSize = 0;
            this.btnNavMicrosip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnNavMicrosip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavMicrosip.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnNavMicrosip.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnNavMicrosip.Location = new System.Drawing.Point(0, 96);
            this.btnNavMicrosip.Name = "btnNavMicrosip";
            this.btnNavMicrosip.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnNavMicrosip.Size = new System.Drawing.Size(230, 44);
            this.btnNavMicrosip.TabIndex = 1;
            this.btnNavMicrosip.Text = "Microsip";
            this.btnNavMicrosip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavMicrosip.UseVisualStyleBackColor = false;
            this.btnNavMicrosip.Click += new System.EventHandler(this.btnNavMicrosip_Click);

            // === pnlContent =====================================================
            this.pnlContent.BackColor = System.Drawing.Color.White;
            this.pnlContent.Controls.Add(this.pnlSeccionDias);
            this.pnlContent.Controls.Add(this.pnlSeccionEmpresas);
            this.pnlContent.Controls.Add(this.pnlSeccionOtros);
            this.pnlContent.Controls.Add(this.pnlSeccionServicio);
            this.pnlContent.Controls.Add(this.pnlSeccionPortal);
            this.pnlContent.Controls.Add(this.pnlSeccionMicrosip);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(230, 0);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(32, 28, 32, 16);
            this.pnlContent.Size = new System.Drawing.Size(850, 618);
            this.pnlContent.TabIndex = 1;

            // === pnlSeccionMicrosip =============================================
            this.pnlSeccionMicrosip.BackColor = System.Drawing.Color.White;
            this.pnlSeccionMicrosip.Controls.Add(this.cardMicrosip);
            this.pnlSeccionMicrosip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSeccionMicrosip.Location = new System.Drawing.Point(32, 28);
            this.pnlSeccionMicrosip.Name = "pnlSeccionMicrosip";
            this.pnlSeccionMicrosip.Size = new System.Drawing.Size(786, 574);
            this.pnlSeccionMicrosip.TabIndex = 0;

            // === cardMicrosip ===================================================
            this.cardMicrosip.BackColor = System.Drawing.Color.White;
            this.cardMicrosip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardMicrosip.Controls.Add(this.lblEstadoConexionMic);
            this.cardMicrosip.Controls.Add(this.btnGuardarMicrosip);
            this.cardMicrosip.Controls.Add(this.btnProbarMicrosip);
            this.cardMicrosip.Controls.Add(this.txtMicPass);
            this.cardMicrosip.Controls.Add(this.lblMicPass);
            this.cardMicrosip.Controls.Add(this.txtMicUser);
            this.cardMicrosip.Controls.Add(this.lblMicUser);
            this.cardMicrosip.Controls.Add(this.btnMicExaminar);
            this.cardMicrosip.Controls.Add(this.txtMicRoot);
            this.cardMicrosip.Controls.Add(this.lblMicRoot);
            this.cardMicrosip.Controls.Add(this.txtMicSrv);
            this.cardMicrosip.Controls.Add(this.lblMicSrv);
            this.cardMicrosip.Controls.Add(this.lblHelpMicrosip);
            this.cardMicrosip.Controls.Add(this.lblTituloMicrosip);
            this.cardMicrosip.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardMicrosip.Location = new System.Drawing.Point(0, 0);
            this.cardMicrosip.Name = "cardMicrosip";
            this.cardMicrosip.Padding = new System.Windows.Forms.Padding(28, 24, 28, 24);
            this.cardMicrosip.Size = new System.Drawing.Size(786, 360);
            this.cardMicrosip.TabIndex = 0;

            this.lblTituloMicrosip.AutoSize = true;
            this.lblTituloMicrosip.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.lblTituloMicrosip.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTituloMicrosip.Location = new System.Drawing.Point(28, 24);
            this.lblTituloMicrosip.Name = "lblTituloMicrosip";
            this.lblTituloMicrosip.Text = "Conexión con Microsip";

            this.lblHelpMicrosip.AutoSize = true;
            this.lblHelpMicrosip.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHelpMicrosip.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblHelpMicrosip.Location = new System.Drawing.Point(28, 56);
            this.lblHelpMicrosip.Name = "lblHelpMicrosip";
            this.lblHelpMicrosip.Text = "Credenciales y ruta de la base de datos Firebird de esta instalación.";

            this.lblMicSrv.AutoSize = true;
            this.lblMicSrv.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMicSrv.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblMicSrv.Location = new System.Drawing.Point(28, 100);
            this.lblMicSrv.Name = "lblMicSrv";
            this.lblMicSrv.Text = "Servidor";

            this.txtMicSrv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMicSrv.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMicSrv.Location = new System.Drawing.Point(28, 122);
            this.txtMicSrv.Name = "txtMicSrv";
            this.txtMicSrv.Size = new System.Drawing.Size(380, 26);

            this.lblMicRoot.AutoSize = true;
            this.lblMicRoot.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMicRoot.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblMicRoot.Location = new System.Drawing.Point(28, 158);
            this.lblMicRoot.Name = "lblMicRoot";
            this.lblMicRoot.Text = "Carpeta de datos";

            this.txtMicRoot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMicRoot.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMicRoot.Location = new System.Drawing.Point(28, 180);
            this.txtMicRoot.Name = "txtMicRoot";
            this.txtMicRoot.Size = new System.Drawing.Size(280, 26);

            this.btnMicExaminar.BackColor = System.Drawing.Color.White;
            this.btnMicExaminar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMicExaminar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnMicExaminar.FlatAppearance.BorderSize = 1;
            this.btnMicExaminar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnMicExaminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMicExaminar.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnMicExaminar.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnMicExaminar.Location = new System.Drawing.Point(316, 178);
            this.btnMicExaminar.Name = "btnMicExaminar";
            this.btnMicExaminar.Size = new System.Drawing.Size(92, 30);
            this.btnMicExaminar.Text = "Examinar…";
            this.btnMicExaminar.UseVisualStyleBackColor = false;
            this.btnMicExaminar.Click += new System.EventHandler(this.btnMicExaminar_Click);

            this.lblMicUser.AutoSize = true;
            this.lblMicUser.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMicUser.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblMicUser.Location = new System.Drawing.Point(28, 216);
            this.lblMicUser.Name = "lblMicUser";
            this.lblMicUser.Text = "Usuario";

            this.txtMicUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMicUser.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMicUser.Location = new System.Drawing.Point(28, 238);
            this.txtMicUser.Name = "txtMicUser";
            this.txtMicUser.Size = new System.Drawing.Size(180, 26);

            this.lblMicPass.AutoSize = true;
            this.lblMicPass.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMicPass.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblMicPass.Location = new System.Drawing.Point(228, 216);
            this.lblMicPass.Name = "lblMicPass";
            this.lblMicPass.Text = "Contraseña";

            this.txtMicPass.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMicPass.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMicPass.Location = new System.Drawing.Point(228, 238);
            this.txtMicPass.Name = "txtMicPass";
            this.txtMicPass.Size = new System.Drawing.Size(180, 26);
            this.txtMicPass.UseSystemPasswordChar = true;

            this.btnProbarMicrosip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnProbarMicrosip.BackColor = System.Drawing.Color.White;
            this.btnProbarMicrosip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnProbarMicrosip.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnProbarMicrosip.FlatAppearance.BorderSize = 1;
            this.btnProbarMicrosip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnProbarMicrosip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProbarMicrosip.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnProbarMicrosip.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnProbarMicrosip.Location = new System.Drawing.Point(28, 296);
            this.btnProbarMicrosip.Name = "btnProbarMicrosip";
            this.btnProbarMicrosip.Size = new System.Drawing.Size(160, 36);
            this.btnProbarMicrosip.Text = "Probar conexión";
            this.btnProbarMicrosip.UseVisualStyleBackColor = false;
            this.btnProbarMicrosip.Click += new System.EventHandler(this.btnProbarMicrosip_Click);

            this.lblEstadoConexionMic.AutoSize = true;
            this.lblEstadoConexionMic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstadoConexionMic.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadoConexionMic.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstadoConexionMic.Location = new System.Drawing.Point(200, 306);
            this.lblEstadoConexionMic.Name = "lblEstadoConexionMic";
            this.lblEstadoConexionMic.Text = "";

            this.btnGuardarMicrosip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardarMicrosip.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnGuardarMicrosip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardarMicrosip.FlatAppearance.BorderSize = 0;
            this.btnGuardarMicrosip.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnGuardarMicrosip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarMicrosip.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnGuardarMicrosip.ForeColor = System.Drawing.Color.White;
            this.btnGuardarMicrosip.Location = new System.Drawing.Point(598, 296);
            this.btnGuardarMicrosip.Name = "btnGuardarMicrosip";
            this.btnGuardarMicrosip.Size = new System.Drawing.Size(160, 36);
            this.btnGuardarMicrosip.Text = "Guardar";
            this.btnGuardarMicrosip.UseVisualStyleBackColor = false;
            this.btnGuardarMicrosip.Click += new System.EventHandler(this.btnGuardarMicrosip_Click);

            // === pnlSeccionPortal ===============================================
            this.pnlSeccionPortal.BackColor = System.Drawing.Color.White;
            this.pnlSeccionPortal.Controls.Add(this.cardPortal);
            this.pnlSeccionPortal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSeccionPortal.Location = new System.Drawing.Point(32, 28);
            this.pnlSeccionPortal.Name = "pnlSeccionPortal";
            this.pnlSeccionPortal.Size = new System.Drawing.Size(786, 574);
            this.pnlSeccionPortal.TabIndex = 1;
            this.pnlSeccionPortal.Visible = false;

            // === cardPortal =====================================================
            this.cardPortal.BackColor = System.Drawing.Color.White;
            this.cardPortal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardPortal.Controls.Add(this.lblEstadoConexionPortal);
            this.cardPortal.Controls.Add(this.btnGuardarPortal);
            this.cardPortal.Controls.Add(this.btnProbarPortal);
            this.cardPortal.Controls.Add(this.btnPortalToggle);
            this.cardPortal.Controls.Add(this.txtPortalApiKey);
            this.cardPortal.Controls.Add(this.lblPortalApiKey);
            this.cardPortal.Controls.Add(this.txtPortalUrl);
            this.cardPortal.Controls.Add(this.lblPortalUrl);
            this.cardPortal.Controls.Add(this.lblHelpPortal);
            this.cardPortal.Controls.Add(this.lblTituloPortal);
            this.cardPortal.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardPortal.Location = new System.Drawing.Point(0, 0);
            this.cardPortal.Name = "cardPortal";
            this.cardPortal.Padding = new System.Windows.Forms.Padding(28, 24, 28, 24);
            this.cardPortal.Size = new System.Drawing.Size(786, 300);
            this.cardPortal.TabIndex = 0;

            this.lblTituloPortal.AutoSize = true;
            this.lblTituloPortal.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.lblTituloPortal.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTituloPortal.Location = new System.Drawing.Point(28, 24);
            this.lblTituloPortal.Name = "lblTituloPortal";
            this.lblTituloPortal.Text = "Conexión con el portal";

            this.lblHelpPortal.AutoSize = true;
            this.lblHelpPortal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHelpPortal.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblHelpPortal.Location = new System.Drawing.Point(28, 56);
            this.lblHelpPortal.Name = "lblHelpPortal";
            this.lblHelpPortal.Text = "URL base del portal y API Key para autenticar al servicio. Cada cliente recibe su propia clave.";

            this.lblPortalUrl.AutoSize = true;
            this.lblPortalUrl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPortalUrl.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblPortalUrl.Location = new System.Drawing.Point(28, 100);
            this.lblPortalUrl.Name = "lblPortalUrl";
            this.lblPortalUrl.Text = "URL del portal";

            this.txtPortalUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPortalUrl.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPortalUrl.Location = new System.Drawing.Point(28, 122);
            this.txtPortalUrl.Name = "txtPortalUrl";
            this.txtPortalUrl.Size = new System.Drawing.Size(540, 26);

            this.lblPortalApiKey.AutoSize = true;
            this.lblPortalApiKey.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPortalApiKey.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblPortalApiKey.Location = new System.Drawing.Point(28, 158);
            this.lblPortalApiKey.Name = "lblPortalApiKey";
            this.lblPortalApiKey.Text = "API Key";

            this.txtPortalApiKey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPortalApiKey.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.txtPortalApiKey.Location = new System.Drawing.Point(28, 180);
            this.txtPortalApiKey.Name = "txtPortalApiKey";
            this.txtPortalApiKey.Size = new System.Drawing.Size(500, 26);
            this.txtPortalApiKey.UseSystemPasswordChar = true;

            this.btnPortalToggle.BackColor = System.Drawing.Color.White;
            this.btnPortalToggle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPortalToggle.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnPortalToggle.FlatAppearance.BorderSize = 1;
            this.btnPortalToggle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnPortalToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPortalToggle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPortalToggle.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnPortalToggle.Location = new System.Drawing.Point(534, 178);
            this.btnPortalToggle.Name = "btnPortalToggle";
            this.btnPortalToggle.Size = new System.Drawing.Size(80, 30);
            this.btnPortalToggle.Text = "Mostrar";
            this.btnPortalToggle.UseVisualStyleBackColor = false;
            this.btnPortalToggle.Click += new System.EventHandler(this.btnPortalToggle_Click);

            this.btnProbarPortal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnProbarPortal.BackColor = System.Drawing.Color.White;
            this.btnProbarPortal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnProbarPortal.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnProbarPortal.FlatAppearance.BorderSize = 1;
            this.btnProbarPortal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnProbarPortal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProbarPortal.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnProbarPortal.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnProbarPortal.Location = new System.Drawing.Point(28, 236);
            this.btnProbarPortal.Name = "btnProbarPortal";
            this.btnProbarPortal.Size = new System.Drawing.Size(160, 36);
            this.btnProbarPortal.Text = "Probar conexión";
            this.btnProbarPortal.UseVisualStyleBackColor = false;
            this.btnProbarPortal.Click += new System.EventHandler(this.btnProbarPortal_Click);

            this.lblEstadoConexionPortal.AutoSize = true;
            this.lblEstadoConexionPortal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstadoConexionPortal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadoConexionPortal.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstadoConexionPortal.Location = new System.Drawing.Point(200, 246);
            this.lblEstadoConexionPortal.Name = "lblEstadoConexionPortal";
            this.lblEstadoConexionPortal.Text = "";

            this.btnGuardarPortal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardarPortal.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnGuardarPortal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardarPortal.FlatAppearance.BorderSize = 0;
            this.btnGuardarPortal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnGuardarPortal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarPortal.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnGuardarPortal.ForeColor = System.Drawing.Color.White;
            this.btnGuardarPortal.Location = new System.Drawing.Point(598, 236);
            this.btnGuardarPortal.Name = "btnGuardarPortal";
            this.btnGuardarPortal.Size = new System.Drawing.Size(160, 36);
            this.btnGuardarPortal.Text = "Guardar";
            this.btnGuardarPortal.UseVisualStyleBackColor = false;
            this.btnGuardarPortal.Click += new System.EventHandler(this.btnGuardarPortal_Click);

            // === pnlSeccionServicio =============================================
            this.pnlSeccionServicio.BackColor = System.Drawing.Color.White;
            this.pnlSeccionServicio.Controls.Add(this.cardServicio);
            this.pnlSeccionServicio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSeccionServicio.Location = new System.Drawing.Point(32, 28);
            this.pnlSeccionServicio.Name = "pnlSeccionServicio";
            this.pnlSeccionServicio.Size = new System.Drawing.Size(786, 574);
            this.pnlSeccionServicio.TabIndex = 2;
            this.pnlSeccionServicio.Visible = false;

            // === cardServicio ===================================================
            this.cardServicio.BackColor = System.Drawing.Color.White;
            this.cardServicio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardServicio.Controls.Add(this.lblEstadoServicioMensaje);
            this.cardServicio.Controls.Add(this.btnDetenerServicio);
            this.cardServicio.Controls.Add(this.btnIniciarServicio);
            this.cardServicio.Controls.Add(this.btnDesinstalarServicio);
            this.cardServicio.Controls.Add(this.btnInstalarServicio);
            this.cardServicio.Controls.Add(this.btnGuardarServicio);
            this.cardServicio.Controls.Add(this.btnRefrescarEstado);
            this.cardServicio.Controls.Add(this.lblEstadoActualValor);
            this.cardServicio.Controls.Add(this.lblEstadoActualLabel);
            this.cardServicio.Controls.Add(this.btnExaminarRuta);
            this.cardServicio.Controls.Add(this.txtRutaArchivos);
            this.cardServicio.Controls.Add(this.lblRutaArchivos);
            this.cardServicio.Controls.Add(this.txtServiceName);
            this.cardServicio.Controls.Add(this.lblServiceName);
            this.cardServicio.Controls.Add(this.lblHelpServicio);
            this.cardServicio.Controls.Add(this.lblTituloServicio);
            this.cardServicio.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardServicio.Location = new System.Drawing.Point(0, 0);
            this.cardServicio.Name = "cardServicio";
            this.cardServicio.Padding = new System.Windows.Forms.Padding(28, 24, 28, 24);
            this.cardServicio.Size = new System.Drawing.Size(786, 430);
            this.cardServicio.TabIndex = 0;

            this.lblTituloServicio.AutoSize = true;
            this.lblTituloServicio.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.lblTituloServicio.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTituloServicio.Location = new System.Drawing.Point(28, 24);
            this.lblTituloServicio.Name = "lblTituloServicio";
            this.lblTituloServicio.Text = "Servicio de Windows";

            this.lblHelpServicio.AutoSize = true;
            this.lblHelpServicio.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHelpServicio.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblHelpServicio.Location = new System.Drawing.Point(28, 56);
            this.lblHelpServicio.Name = "lblHelpServicio";
            this.lblHelpServicio.Text = "Instala el servicio que sincroniza Microsip con el portal y controla su ejecución en esta máquina.";

            this.lblServiceName.AutoSize = true;
            this.lblServiceName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblServiceName.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblServiceName.Location = new System.Drawing.Point(28, 100);
            this.lblServiceName.Name = "lblServiceName";
            this.lblServiceName.Text = "Nombre del servicio";

            this.txtServiceName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServiceName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtServiceName.Location = new System.Drawing.Point(28, 122);
            this.txtServiceName.Name = "txtServiceName";
            this.txtServiceName.Size = new System.Drawing.Size(380, 26);

            this.lblRutaArchivos.AutoSize = true;
            this.lblRutaArchivos.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRutaArchivos.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblRutaArchivos.Location = new System.Drawing.Point(28, 158);
            this.lblRutaArchivos.Name = "lblRutaArchivos";
            this.lblRutaArchivos.Text = "Carpeta de archivos";

            this.txtRutaArchivos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRutaArchivos.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtRutaArchivos.Location = new System.Drawing.Point(28, 180);
            this.txtRutaArchivos.Name = "txtRutaArchivos";
            this.txtRutaArchivos.Size = new System.Drawing.Size(280, 26);

            this.btnExaminarRuta.BackColor = System.Drawing.Color.White;
            this.btnExaminarRuta.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExaminarRuta.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnExaminarRuta.FlatAppearance.BorderSize = 1;
            this.btnExaminarRuta.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnExaminarRuta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExaminarRuta.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnExaminarRuta.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnExaminarRuta.Location = new System.Drawing.Point(316, 178);
            this.btnExaminarRuta.Name = "btnExaminarRuta";
            this.btnExaminarRuta.Size = new System.Drawing.Size(92, 30);
            this.btnExaminarRuta.Text = "Examinar…";
            this.btnExaminarRuta.UseVisualStyleBackColor = false;
            this.btnExaminarRuta.Click += new System.EventHandler(this.btnExaminarRuta_Click);

            this.btnGuardarServicio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardarServicio.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnGuardarServicio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardarServicio.FlatAppearance.BorderSize = 0;
            this.btnGuardarServicio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnGuardarServicio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarServicio.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnGuardarServicio.ForeColor = System.Drawing.Color.White;
            this.btnGuardarServicio.Location = new System.Drawing.Point(598, 178);
            this.btnGuardarServicio.Name = "btnGuardarServicio";
            this.btnGuardarServicio.Size = new System.Drawing.Size(160, 36);
            this.btnGuardarServicio.Text = "Guardar";
            this.btnGuardarServicio.UseVisualStyleBackColor = false;
            this.btnGuardarServicio.Click += new System.EventHandler(this.btnGuardarServicio_Click);

            this.lblEstadoActualLabel.AutoSize = true;
            this.lblEstadoActualLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadoActualLabel.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblEstadoActualLabel.Location = new System.Drawing.Point(28, 240);
            this.lblEstadoActualLabel.Name = "lblEstadoActualLabel";
            this.lblEstadoActualLabel.Text = "Estado actual:";

            this.lblEstadoActualValor.AutoSize = true;
            this.lblEstadoActualValor.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.lblEstadoActualValor.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstadoActualValor.Location = new System.Drawing.Point(116, 239);
            this.lblEstadoActualValor.Name = "lblEstadoActualValor";
            this.lblEstadoActualValor.Text = "—";

            this.btnRefrescarEstado.BackColor = System.Drawing.Color.White;
            this.btnRefrescarEstado.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefrescarEstado.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnRefrescarEstado.FlatAppearance.BorderSize = 1;
            this.btnRefrescarEstado.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnRefrescarEstado.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefrescarEstado.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefrescarEstado.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnRefrescarEstado.Location = new System.Drawing.Point(260, 236);
            this.btnRefrescarEstado.Name = "btnRefrescarEstado";
            this.btnRefrescarEstado.Size = new System.Drawing.Size(80, 28);
            this.btnRefrescarEstado.Text = "Refrescar";
            this.btnRefrescarEstado.UseVisualStyleBackColor = false;
            this.btnRefrescarEstado.Click += new System.EventHandler(this.btnRefrescarEstado_Click);

            // Fila de 4 botones de acción del servicio. Todos requieren UAC; el
            // habilitar/deshabilitar de cada uno depende del estado en vivo.
            this.btnInstalarServicio.BackColor = System.Drawing.Color.White;
            this.btnInstalarServicio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnInstalarServicio.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnInstalarServicio.FlatAppearance.BorderSize = 1;
            this.btnInstalarServicio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnInstalarServicio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstalarServicio.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnInstalarServicio.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnInstalarServicio.Location = new System.Drawing.Point(28, 300);
            this.btnInstalarServicio.Name = "btnInstalarServicio";
            this.btnInstalarServicio.Size = new System.Drawing.Size(160, 38);
            this.btnInstalarServicio.Text = "Instalar";
            this.btnInstalarServicio.UseVisualStyleBackColor = false;
            this.btnInstalarServicio.Click += new System.EventHandler(this.btnInstalarServicio_Click);

            this.btnDesinstalarServicio.BackColor = System.Drawing.Color.White;
            this.btnDesinstalarServicio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDesinstalarServicio.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnDesinstalarServicio.FlatAppearance.BorderSize = 1;
            this.btnDesinstalarServicio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnDesinstalarServicio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesinstalarServicio.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnDesinstalarServicio.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.btnDesinstalarServicio.Location = new System.Drawing.Point(198, 300);
            this.btnDesinstalarServicio.Name = "btnDesinstalarServicio";
            this.btnDesinstalarServicio.Size = new System.Drawing.Size(160, 38);
            this.btnDesinstalarServicio.Text = "Desinstalar";
            this.btnDesinstalarServicio.UseVisualStyleBackColor = false;
            this.btnDesinstalarServicio.Click += new System.EventHandler(this.btnDesinstalarServicio_Click);

            this.btnIniciarServicio.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnIniciarServicio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIniciarServicio.FlatAppearance.BorderSize = 0;
            this.btnIniciarServicio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnIniciarServicio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIniciarServicio.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnIniciarServicio.ForeColor = System.Drawing.Color.White;
            this.btnIniciarServicio.Location = new System.Drawing.Point(368, 300);
            this.btnIniciarServicio.Name = "btnIniciarServicio";
            this.btnIniciarServicio.Size = new System.Drawing.Size(160, 38);
            this.btnIniciarServicio.Text = "Iniciar";
            this.btnIniciarServicio.UseVisualStyleBackColor = false;
            this.btnIniciarServicio.Click += new System.EventHandler(this.btnIniciarServicio_Click);

            this.btnDetenerServicio.BackColor = System.Drawing.Color.White;
            this.btnDetenerServicio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDetenerServicio.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.btnDetenerServicio.FlatAppearance.BorderSize = 1;
            this.btnDetenerServicio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnDetenerServicio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDetenerServicio.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnDetenerServicio.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnDetenerServicio.Location = new System.Drawing.Point(538, 300);
            this.btnDetenerServicio.Name = "btnDetenerServicio";
            this.btnDetenerServicio.Size = new System.Drawing.Size(160, 38);
            this.btnDetenerServicio.Text = "Detener";
            this.btnDetenerServicio.UseVisualStyleBackColor = false;
            this.btnDetenerServicio.Click += new System.EventHandler(this.btnDetenerServicio_Click);

            this.lblEstadoServicioMensaje.AutoSize = true;
            this.lblEstadoServicioMensaje.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstadoServicioMensaje.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadoServicioMensaje.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstadoServicioMensaje.Location = new System.Drawing.Point(28, 380);
            this.lblEstadoServicioMensaje.Name = "lblEstadoServicioMensaje";
            this.lblEstadoServicioMensaje.Text = "";

            // === pnlSeccionOtros ================================================
            this.pnlSeccionOtros.BackColor = System.Drawing.Color.White;
            this.pnlSeccionOtros.Controls.Add(this.cardParametros);
            this.pnlSeccionOtros.Controls.Add(this.cardComportamiento);
            this.pnlSeccionOtros.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSeccionOtros.Location = new System.Drawing.Point(32, 28);
            this.pnlSeccionOtros.Name = "pnlSeccionOtros";
            this.pnlSeccionOtros.Size = new System.Drawing.Size(786, 574);
            this.pnlSeccionOtros.TabIndex = 3;
            this.pnlSeccionOtros.Visible = false;

            // === cardComportamiento =============================================
            this.cardComportamiento.BackColor = System.Drawing.Color.White;
            this.cardComportamiento.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardComportamiento.Controls.Add(this.btnGuardarHKLM);
            this.cardComportamiento.Controls.Add(this.lblEnviarCorreoHelp);
            this.cardComportamiento.Controls.Add(this.chkEnviarCorreo);
            this.cardComportamiento.Controls.Add(this.lblTimerHelper);
            this.cardComportamiento.Controls.Add(this.cmbUnidadTimer);
            this.cardComportamiento.Controls.Add(this.nudTimer);
            this.cardComportamiento.Controls.Add(this.lblSincCada);
            this.cardComportamiento.Controls.Add(this.lblTituloComp);
            this.cardComportamiento.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardComportamiento.Location = new System.Drawing.Point(0, 0);
            this.cardComportamiento.Name = "cardComportamiento";
            this.cardComportamiento.Padding = new System.Windows.Forms.Padding(28, 22, 28, 22);
            this.cardComportamiento.Size = new System.Drawing.Size(786, 210);
            this.cardComportamiento.TabIndex = 0;

            this.lblTituloComp.AutoSize = true;
            this.lblTituloComp.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.lblTituloComp.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTituloComp.Location = new System.Drawing.Point(28, 22);
            this.lblTituloComp.Name = "lblTituloComp";
            this.lblTituloComp.Text = "Comportamiento del servicio";

            this.lblSincCada.AutoSize = true;
            this.lblSincCada.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSincCada.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblSincCada.Location = new System.Drawing.Point(28, 72);
            this.lblSincCada.Name = "lblSincCada";
            this.lblSincCada.Text = "Sincronizar cada";

            this.nudTimer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nudTimer.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.nudTimer.Location = new System.Drawing.Point(28, 94);
            this.nudTimer.Maximum = new decimal(new int[] {9999, 0, 0, 0});
            this.nudTimer.Minimum = new decimal(new int[] {1, 0, 0, 0});
            this.nudTimer.Name = "nudTimer";
            this.nudTimer.Size = new System.Drawing.Size(90, 26);
            this.nudTimer.Value = new decimal(new int[] {1, 0, 0, 0});

            this.cmbUnidadTimer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnidadTimer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbUnidadTimer.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbUnidadTimer.Items.AddRange(new object[] { "Segundos", "Minutos", "Horas", "Días" });
            this.cmbUnidadTimer.Location = new System.Drawing.Point(128, 94);
            this.cmbUnidadTimer.Name = "cmbUnidadTimer";
            this.cmbUnidadTimer.Size = new System.Drawing.Size(110, 28);

            this.lblTimerHelper.AutoSize = true;
            this.lblTimerHelper.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblTimerHelper.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblTimerHelper.Location = new System.Drawing.Point(250, 100);
            this.lblTimerHelper.Name = "lblTimerHelper";
            this.lblTimerHelper.Text = "(= — segundos)";

            this.chkEnviarCorreo.AutoSize = true;
            this.chkEnviarCorreo.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkEnviarCorreo.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkEnviarCorreo.Location = new System.Drawing.Point(28, 140);
            this.chkEnviarCorreo.Name = "chkEnviarCorreo";
            this.chkEnviarCorreo.Text = "Notificar por correo al proveedor al generar una compra";
            this.chkEnviarCorreo.UseVisualStyleBackColor = true;

            this.lblEnviarCorreoHelp.AutoSize = true;
            this.lblEnviarCorreoHelp.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblEnviarCorreoHelp.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEnviarCorreoHelp.Location = new System.Drawing.Point(50, 164);
            this.lblEnviarCorreoHelp.Name = "lblEnviarCorreoHelp";
            this.lblEnviarCorreoHelp.Text = "El proveedor recibe un aviso cuando su factura genera una compra en Microsip.";

            this.btnGuardarHKLM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardarHKLM.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnGuardarHKLM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardarHKLM.FlatAppearance.BorderSize = 0;
            this.btnGuardarHKLM.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnGuardarHKLM.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarHKLM.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnGuardarHKLM.ForeColor = System.Drawing.Color.White;
            this.btnGuardarHKLM.Location = new System.Drawing.Point(598, 150);
            this.btnGuardarHKLM.Name = "btnGuardarHKLM";
            this.btnGuardarHKLM.Size = new System.Drawing.Size(160, 36);
            this.btnGuardarHKLM.Text = "Guardar";
            this.btnGuardarHKLM.UseVisualStyleBackColor = false;
            this.btnGuardarHKLM.Click += new System.EventHandler(this.btnGuardarHKLM_Click);

            // === cardParametros =================================================
            this.cardParametros.BackColor = System.Drawing.Color.White;
            this.cardParametros.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardParametros.Controls.Add(this.btnGuardarParametros);
            this.cardParametros.Controls.Add(this.dgvParametros);
            this.cardParametros.Controls.Add(this.lblTituloParam);
            this.cardParametros.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardParametros.Location = new System.Drawing.Point(0, 210);
            this.cardParametros.Name = "cardParametros";
            this.cardParametros.Padding = new System.Windows.Forms.Padding(28, 22, 28, 22);
            this.cardParametros.Size = new System.Drawing.Size(786, 364);
            this.cardParametros.TabIndex = 1;

            this.lblTituloParam.AutoSize = true;
            this.lblTituloParam.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.lblTituloParam.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTituloParam.Location = new System.Drawing.Point(28, 22);
            this.lblTituloParam.Name = "lblTituloParam";
            this.lblTituloParam.Text = "Parámetros del portal";

            this.dgvParametros.AllowUserToAddRows = false;
            this.dgvParametros.AllowUserToDeleteRows = false;
            this.dgvParametros.AllowUserToResizeRows = false;
            this.dgvParametros.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvParametros.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvParametros.BackgroundColor = System.Drawing.Color.White;
            this.dgvParametros.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvParametros.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvParametros.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvParametros.ColumnHeadersHeight = 36;
            this.dgvParametros.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvParametros.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colClave, this.colDescripcion, this.colValor});
            this.dgvParametros.EnableHeadersVisualStyles = false;
            this.dgvParametros.GridColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.dgvParametros.Location = new System.Drawing.Point(28, 60);
            this.dgvParametros.Name = "dgvParametros";
            this.dgvParametros.RowHeadersVisible = false;
            this.dgvParametros.RowTemplate.Height = 32;
            this.dgvParametros.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvParametros.Size = new System.Drawing.Size(728, 250);
            this.dgvParametros.TabIndex = 1;

            this.colClave.HeaderText = "Clave";
            this.colClave.Name = "colClave";
            this.colClave.ReadOnly = true;
            this.colClave.FillWeight = 22F;

            this.colDescripcion.HeaderText = "Descripción";
            this.colDescripcion.Name = "colDescripcion";
            this.colDescripcion.ReadOnly = true;
            this.colDescripcion.FillWeight = 58F;

            this.colValor.HeaderText = "Valor";
            this.colValor.Name = "colValor";
            this.colValor.FillWeight = 20F;

            this.btnGuardarParametros.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardarParametros.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnGuardarParametros.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardarParametros.FlatAppearance.BorderSize = 0;
            this.btnGuardarParametros.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnGuardarParametros.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarParametros.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnGuardarParametros.ForeColor = System.Drawing.Color.White;
            this.btnGuardarParametros.Location = new System.Drawing.Point(596, 318);
            this.btnGuardarParametros.Name = "btnGuardarParametros";
            this.btnGuardarParametros.Size = new System.Drawing.Size(160, 36);
            this.btnGuardarParametros.Text = "Guardar cambios";
            this.btnGuardarParametros.UseVisualStyleBackColor = false;
            this.btnGuardarParametros.Click += new System.EventHandler(this.btnGuardarParametros_Click);

            // === pnlSeccionEmpresas =============================================
            this.pnlSeccionEmpresas.BackColor = System.Drawing.Color.White;
            this.pnlSeccionEmpresas.Controls.Add(this.cardEmpresas);
            this.pnlSeccionEmpresas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSeccionEmpresas.Location = new System.Drawing.Point(32, 28);
            this.pnlSeccionEmpresas.Name = "pnlSeccionEmpresas";
            this.pnlSeccionEmpresas.Size = new System.Drawing.Size(786, 574);
            this.pnlSeccionEmpresas.TabIndex = 4;
            this.pnlSeccionEmpresas.Visible = false;

            // === cardEmpresas ===================================================
            this.cardEmpresas.BackColor = System.Drawing.Color.White;
            this.cardEmpresas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardEmpresas.Controls.Add(this.lblEstadoEmpresas);
            this.cardEmpresas.Controls.Add(this.dgvEmpresas);
            this.cardEmpresas.Controls.Add(this.lblContadorEmpresas);
            this.cardEmpresas.Controls.Add(this.txtBuscarEmpresa);
            this.cardEmpresas.Controls.Add(this.lblBuscar);
            this.cardEmpresas.Controls.Add(this.lblHelpEmpresas);
            this.cardEmpresas.Controls.Add(this.lblTituloEmpresas);
            this.cardEmpresas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardEmpresas.Location = new System.Drawing.Point(0, 0);
            this.cardEmpresas.Name = "cardEmpresas";
            this.cardEmpresas.Padding = new System.Windows.Forms.Padding(28, 24, 28, 24);
            this.cardEmpresas.Size = new System.Drawing.Size(786, 574);
            this.cardEmpresas.TabIndex = 0;

            this.lblTituloEmpresas.AutoSize = true;
            this.lblTituloEmpresas.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.lblTituloEmpresas.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTituloEmpresas.Location = new System.Drawing.Point(28, 24);
            this.lblTituloEmpresas.Name = "lblTituloEmpresas";
            this.lblTituloEmpresas.Text = "Empresas";

            this.lblHelpEmpresas.AutoSize = true;
            this.lblHelpEmpresas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHelpEmpresas.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblHelpEmpresas.Location = new System.Drawing.Point(28, 56);
            this.lblHelpEmpresas.Name = "lblHelpEmpresas";
            this.lblHelpEmpresas.Text = "Autoriza el acceso al portal y activa la tolerancia de diferencias por empresa. Los cambios se guardan al instante.";

            this.lblBuscar.AutoSize = true;
            this.lblBuscar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBuscar.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblBuscar.Location = new System.Drawing.Point(28, 100);
            this.lblBuscar.Name = "lblBuscar";
            this.lblBuscar.Text = "Buscar";

            this.txtBuscarEmpresa.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBuscarEmpresa.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBuscarEmpresa.Location = new System.Drawing.Point(85, 96);
            this.txtBuscarEmpresa.Name = "txtBuscarEmpresa";
            this.txtBuscarEmpresa.Size = new System.Drawing.Size(320, 26);
            this.txtBuscarEmpresa.TextChanged += new System.EventHandler(this.txtBuscarEmpresa_TextChanged);

            this.lblContadorEmpresas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblContadorEmpresas.AutoSize = true;
            this.lblContadorEmpresas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblContadorEmpresas.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblContadorEmpresas.Location = new System.Drawing.Point(580, 100);
            this.lblContadorEmpresas.Name = "lblContadorEmpresas";
            this.lblContadorEmpresas.Text = "—";
            this.lblContadorEmpresas.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // === dgvEmpresas ===================================================
            this.dgvEmpresas.AllowUserToAddRows = false;
            this.dgvEmpresas.AllowUserToDeleteRows = false;
            this.dgvEmpresas.AllowUserToResizeRows = false;
            this.dgvEmpresas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvEmpresas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvEmpresas.BackgroundColor = System.Drawing.Color.White;
            this.dgvEmpresas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvEmpresas.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvEmpresas.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvEmpresas.ColumnHeadersHeight = 36;
            this.dgvEmpresas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvEmpresas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colEmpIdMsp, this.colEmpNombre, this.colEmpNombreLargo, this.colEmpRfc,
                this.colEmpEstatus, this.colEmpDiferencia, this.colEmpSincDesde, this.colEmpUltSinc});
            this.dgvEmpresas.EnableHeadersVisualStyles = false;
            this.dgvEmpresas.GridColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.dgvEmpresas.Location = new System.Drawing.Point(28, 140);
            this.dgvEmpresas.Name = "dgvEmpresas";
            this.dgvEmpresas.RowHeadersVisible = false;
            this.dgvEmpresas.RowTemplate.Height = 32;
            this.dgvEmpresas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            this.dgvEmpresas.Size = new System.Drawing.Size(700, 380);
            this.dgvEmpresas.TabIndex = 4;
            this.dgvEmpresas.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvEmpresas_CellValueChanged);
            this.dgvEmpresas.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvEmpresas_CurrentCellDirtyStateChanged);
            this.dgvEmpresas.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvEmpresas_CellDoubleClick);

            // EMP_ID_MSP (hidden): se usa para identificar la fila al hacer PATCH.
            this.colEmpIdMsp.HeaderText = "EMP_ID_MSP";
            this.colEmpIdMsp.Name = "colEmpIdMsp";
            this.colEmpIdMsp.ReadOnly = true;
            this.colEmpIdMsp.Visible = false;

            this.colEmpNombre.HeaderText = "Nombre";
            this.colEmpNombre.Name = "colEmpNombre";
            this.colEmpNombre.ReadOnly = true;
            this.colEmpNombre.FillWeight = 18F;

            this.colEmpNombreLargo.HeaderText = "Razón social";
            this.colEmpNombreLargo.Name = "colEmpNombreLargo";
            this.colEmpNombreLargo.ReadOnly = true;
            this.colEmpNombreLargo.FillWeight = 28F;

            this.colEmpRfc.HeaderText = "RFC";
            this.colEmpRfc.Name = "colEmpRfc";
            this.colEmpRfc.ReadOnly = true;
            this.colEmpRfc.FillWeight = 14F;

            this.colEmpEstatus.HeaderText = "Estatus";
            this.colEmpEstatus.Name = "colEmpEstatus";
            this.colEmpEstatus.Items.AddRange(new object[] { "Bloqueada", "Autorizada" });
            this.colEmpEstatus.FillWeight = 14F;
            this.colEmpEstatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colEmpEstatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

            this.colEmpDiferencia.HeaderText = "Permite dif.";
            this.colEmpDiferencia.Name = "colEmpDiferencia";
            this.colEmpDiferencia.FillWeight = 8F;
            this.colEmpDiferencia.ToolTipText = "Permite tolerancia entre el monto de la factura y el de la recepción.";

            this.colEmpSincDesde.HeaderText = "Sincronizar desde";
            this.colEmpSincDesde.Name = "colEmpSincDesde";
            this.colEmpSincDesde.ReadOnly = true;
            this.colEmpSincDesde.FillWeight = 14F;
            this.colEmpSincDesde.ToolTipText = "Doble-click para cambiar. Sin filtro = sincronizar toda la historia.";
            this.colEmpSincDesde.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(37, 99, 235);

            this.colEmpUltSinc.HeaderText = "Última sync";
            this.colEmpUltSinc.Name = "colEmpUltSinc";
            this.colEmpUltSinc.ReadOnly = true;
            this.colEmpUltSinc.FillWeight = 14F;

            this.lblEstadoEmpresas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstadoEmpresas.AutoSize = true;
            this.lblEstadoEmpresas.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadoEmpresas.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstadoEmpresas.Location = new System.Drawing.Point(28, 530);
            this.lblEstadoEmpresas.Name = "lblEstadoEmpresas";
            this.lblEstadoEmpresas.Text = "";

            // === pnlSeccionDias =================================================
            this.pnlSeccionDias.BackColor = System.Drawing.Color.White;
            this.pnlSeccionDias.Controls.Add(this.cardDias);
            this.pnlSeccionDias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSeccionDias.Location = new System.Drawing.Point(32, 28);
            this.pnlSeccionDias.Name = "pnlSeccionDias";
            this.pnlSeccionDias.Size = new System.Drawing.Size(786, 574);
            this.pnlSeccionDias.TabIndex = 5;
            this.pnlSeccionDias.Visible = false;

            // === cardDias =======================================================
            this.cardDias.BackColor = System.Drawing.Color.White;
            this.cardDias.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cardDias.Controls.Add(this.lblEstadoDias);
            this.cardDias.Controls.Add(this.btnGuardarDias);
            this.cardDias.Controls.Add(this.chkDia7);
            this.cardDias.Controls.Add(this.chkDia6);
            this.cardDias.Controls.Add(this.chkDia5);
            this.cardDias.Controls.Add(this.chkDia4);
            this.cardDias.Controls.Add(this.chkDia3);
            this.cardDias.Controls.Add(this.chkDia2);
            this.cardDias.Controls.Add(this.chkDia1);
            this.cardDias.Controls.Add(this.lblHelpDias);
            this.cardDias.Controls.Add(this.lblTituloDias);
            this.cardDias.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardDias.Location = new System.Drawing.Point(0, 0);
            this.cardDias.Name = "cardDias";
            this.cardDias.Padding = new System.Windows.Forms.Padding(28, 24, 28, 24);
            this.cardDias.Size = new System.Drawing.Size(786, 290);
            this.cardDias.TabIndex = 0;

            this.lblTituloDias.AutoSize = true;
            this.lblTituloDias.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
            this.lblTituloDias.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblTituloDias.Location = new System.Drawing.Point(28, 24);
            this.lblTituloDias.Name = "lblTituloDias";
            this.lblTituloDias.Text = "Días de recepción";

            this.lblHelpDias.AutoSize = true;
            this.lblHelpDias.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHelpDias.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblHelpDias.Location = new System.Drawing.Point(28, 56);
            this.lblHelpDias.Name = "lblHelpDias";
            this.lblHelpDias.Text = "Marca los días de la semana en que el portal acepta recepciones de mercancía de los proveedores.";

            // Fila 1: Lunes, Martes, Miércoles
            this.chkDia1.AutoSize = true;
            this.chkDia1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkDia1.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkDia1.Location = new System.Drawing.Point(28, 110);
            this.chkDia1.Name = "chkDia1";
            this.chkDia1.Tag = "1";
            this.chkDia1.Text = "Lunes";
            this.chkDia1.UseVisualStyleBackColor = true;

            this.chkDia2.AutoSize = true;
            this.chkDia2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkDia2.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkDia2.Location = new System.Drawing.Point(218, 110);
            this.chkDia2.Name = "chkDia2";
            this.chkDia2.Tag = "2";
            this.chkDia2.Text = "Martes";
            this.chkDia2.UseVisualStyleBackColor = true;

            this.chkDia3.AutoSize = true;
            this.chkDia3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkDia3.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkDia3.Location = new System.Drawing.Point(408, 110);
            this.chkDia3.Name = "chkDia3";
            this.chkDia3.Tag = "3";
            this.chkDia3.Text = "Miércoles";
            this.chkDia3.UseVisualStyleBackColor = true;

            // Fila 2: Jueves, Viernes, Sábado
            this.chkDia4.AutoSize = true;
            this.chkDia4.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkDia4.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkDia4.Location = new System.Drawing.Point(28, 150);
            this.chkDia4.Name = "chkDia4";
            this.chkDia4.Tag = "4";
            this.chkDia4.Text = "Jueves";
            this.chkDia4.UseVisualStyleBackColor = true;

            this.chkDia5.AutoSize = true;
            this.chkDia5.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkDia5.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkDia5.Location = new System.Drawing.Point(218, 150);
            this.chkDia5.Name = "chkDia5";
            this.chkDia5.Tag = "5";
            this.chkDia5.Text = "Viernes";
            this.chkDia5.UseVisualStyleBackColor = true;

            this.chkDia6.AutoSize = true;
            this.chkDia6.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkDia6.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkDia6.Location = new System.Drawing.Point(408, 150);
            this.chkDia6.Name = "chkDia6";
            this.chkDia6.Tag = "6";
            this.chkDia6.Text = "Sábado";
            this.chkDia6.UseVisualStyleBackColor = true;

            // Fila 3: Domingo
            this.chkDia7.AutoSize = true;
            this.chkDia7.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkDia7.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkDia7.Location = new System.Drawing.Point(28, 190);
            this.chkDia7.Name = "chkDia7";
            this.chkDia7.Tag = "7";
            this.chkDia7.Text = "Domingo";
            this.chkDia7.UseVisualStyleBackColor = true;

            this.lblEstadoDias.AutoSize = true;
            this.lblEstadoDias.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstadoDias.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEstadoDias.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstadoDias.Location = new System.Drawing.Point(28, 240);
            this.lblEstadoDias.Name = "lblEstadoDias";
            this.lblEstadoDias.Text = "";

            this.btnGuardarDias.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGuardarDias.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnGuardarDias.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardarDias.FlatAppearance.BorderSize = 0;
            this.btnGuardarDias.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnGuardarDias.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarDias.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnGuardarDias.ForeColor = System.Drawing.Color.White;
            this.btnGuardarDias.Location = new System.Drawing.Point(598, 230);
            this.btnGuardarDias.Name = "btnGuardarDias";
            this.btnGuardarDias.Size = new System.Drawing.Size(160, 36);
            this.btnGuardarDias.Text = "Guardar cambios";
            this.btnGuardarDias.UseVisualStyleBackColor = false;
            this.btnGuardarDias.Click += new System.EventHandler(this.btnGuardarDias_Click);

            // === StatusStrip ====================================================
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.lblEstadoServicio, this.sepStatus1, this.lblEstadoPortal, this.sepStatus2, this.lblEstadoSync});
            this.statusStrip.Location = new System.Drawing.Point(0, 618);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1080, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 2;

            this.lblEstadoServicio.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblEstadoServicio.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstadoServicio.Name = "lblEstadoServicio";
            this.lblEstadoServicio.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblEstadoServicio.Size = new System.Drawing.Size(72, 17);
            this.lblEstadoServicio.Text = "Servicio: —";

            this.sepStatus1.Name = "sepStatus1";

            this.lblEstadoPortal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblEstadoPortal.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstadoPortal.Name = "lblEstadoPortal";
            this.lblEstadoPortal.Size = new System.Drawing.Size(57, 17);
            this.lblEstadoPortal.Text = "Portal: —";

            this.sepStatus2.Name = "sepStatus2";

            this.lblEstadoSync.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblEstadoSync.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblEstadoSync.Name = "lblEstadoSync";
            this.lblEstadoSync.Size = new System.Drawing.Size(94, 17);
            this.lblEstadoSync.Text = "Última sync: —";

            // === Form ===========================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1080, 640);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlSidebar);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.MinimumSize = new System.Drawing.Size(960, 580);
            this.Name = "FormPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Configurador del Portal de Proveedores";
            this.Load += new System.EventHandler(this.FormPrincipal_Load);

            this.pnlSidebar.ResumeLayout(false);
            this.pnlMarca.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.pnlContent.ResumeLayout(false);
            this.pnlSeccionMicrosip.ResumeLayout(false);
            this.pnlSeccionPortal.ResumeLayout(false);
            this.cardPortal.ResumeLayout(false);
            this.cardPortal.PerformLayout();
            this.pnlSeccionServicio.ResumeLayout(false);
            this.cardServicio.ResumeLayout(false);
            this.cardServicio.PerformLayout();
            this.pnlSeccionOtros.ResumeLayout(false);
            this.pnlSeccionEmpresas.ResumeLayout(false);
            this.cardEmpresas.ResumeLayout(false);
            this.cardEmpresas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmpresas)).EndInit();
            this.pnlSeccionDias.ResumeLayout(false);
            this.cardDias.ResumeLayout(false);
            this.cardDias.PerformLayout();
            this.cardMicrosip.ResumeLayout(false);
            this.cardMicrosip.PerformLayout();
            this.cardComportamiento.ResumeLayout(false);
            this.cardComportamiento.PerformLayout();
            this.cardParametros.ResumeLayout(false);
            this.cardParametros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParametros)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // === Campos ==========================================================

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlMarca;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Label lblMarca;
        private System.Windows.Forms.Label lblMarcaSub;
        private System.Windows.Forms.Button btnNavMicrosip;
        private System.Windows.Forms.Button btnNavPortal;
        private System.Windows.Forms.Button btnNavServicio;
        private System.Windows.Forms.Button btnNavOtros;
        private System.Windows.Forms.Button btnNavEmpresas;
        private System.Windows.Forms.Button btnNavDias;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlSeccionMicrosip;
        private System.Windows.Forms.Panel pnlSeccionPortal;
        private System.Windows.Forms.Panel pnlSeccionServicio;
        private System.Windows.Forms.Panel pnlSeccionOtros;
        private System.Windows.Forms.Panel pnlSeccionEmpresas;
        private System.Windows.Forms.Panel pnlSeccionDias;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblEstadoServicio;
        private System.Windows.Forms.ToolStripSeparator sepStatus1;
        private System.Windows.Forms.ToolStripStatusLabel lblEstadoPortal;
        private System.Windows.Forms.ToolStripSeparator sepStatus2;
        private System.Windows.Forms.ToolStripStatusLabel lblEstadoSync;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;

        // Microsip
        private System.Windows.Forms.Panel cardMicrosip;
        private System.Windows.Forms.Label lblTituloMicrosip;
        private System.Windows.Forms.Label lblHelpMicrosip;
        private System.Windows.Forms.Label lblMicSrv;
        private System.Windows.Forms.TextBox txtMicSrv;
        private System.Windows.Forms.Label lblMicRoot;
        private System.Windows.Forms.TextBox txtMicRoot;
        private System.Windows.Forms.Button btnMicExaminar;
        private System.Windows.Forms.Label lblMicUser;
        private System.Windows.Forms.TextBox txtMicUser;
        private System.Windows.Forms.Label lblMicPass;
        private System.Windows.Forms.TextBox txtMicPass;
        private System.Windows.Forms.Button btnProbarMicrosip;
        private System.Windows.Forms.Button btnGuardarMicrosip;
        private System.Windows.Forms.Label lblEstadoConexionMic;

        // Otros - Comportamiento
        private System.Windows.Forms.Panel cardComportamiento;
        private System.Windows.Forms.Label lblTituloComp;
        private System.Windows.Forms.Label lblSincCada;
        private System.Windows.Forms.NumericUpDown nudTimer;
        private System.Windows.Forms.ComboBox cmbUnidadTimer;
        private System.Windows.Forms.Label lblTimerHelper;
        private System.Windows.Forms.CheckBox chkEnviarCorreo;
        private System.Windows.Forms.Label lblEnviarCorreoHelp;
        private System.Windows.Forms.Button btnGuardarHKLM;

        // Otros - Parámetros
        private System.Windows.Forms.Panel cardParametros;
        private System.Windows.Forms.Label lblTituloParam;
        private System.Windows.Forms.DataGridView dgvParametros;
        private System.Windows.Forms.DataGridViewTextBoxColumn colClave;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescripcion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValor;
        private System.Windows.Forms.Button btnGuardarParametros;

        // Portal Web
        private System.Windows.Forms.Panel cardPortal;
        private System.Windows.Forms.Label lblTituloPortal;
        private System.Windows.Forms.Label lblHelpPortal;
        private System.Windows.Forms.Label lblPortalUrl;
        private System.Windows.Forms.TextBox txtPortalUrl;
        private System.Windows.Forms.Label lblPortalApiKey;
        private System.Windows.Forms.TextBox txtPortalApiKey;
        private System.Windows.Forms.Button btnPortalToggle;
        private System.Windows.Forms.Button btnProbarPortal;
        private System.Windows.Forms.Button btnGuardarPortal;
        private System.Windows.Forms.Label lblEstadoConexionPortal;

        // Días de recepción
        private System.Windows.Forms.Panel cardDias;
        private System.Windows.Forms.Label lblTituloDias;
        private System.Windows.Forms.Label lblHelpDias;
        private System.Windows.Forms.CheckBox chkDia1;
        private System.Windows.Forms.CheckBox chkDia2;
        private System.Windows.Forms.CheckBox chkDia3;
        private System.Windows.Forms.CheckBox chkDia4;
        private System.Windows.Forms.CheckBox chkDia5;
        private System.Windows.Forms.CheckBox chkDia6;
        private System.Windows.Forms.CheckBox chkDia7;
        private System.Windows.Forms.Button btnGuardarDias;
        private System.Windows.Forms.Label lblEstadoDias;

        // Empresas
        private System.Windows.Forms.Panel cardEmpresas;
        private System.Windows.Forms.Label lblTituloEmpresas;
        private System.Windows.Forms.Label lblHelpEmpresas;
        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.TextBox txtBuscarEmpresa;
        private System.Windows.Forms.Label lblContadorEmpresas;
        private System.Windows.Forms.DataGridView dgvEmpresas;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmpIdMsp;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmpNombre;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmpNombreLargo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmpRfc;
        private System.Windows.Forms.DataGridViewComboBoxColumn colEmpEstatus;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colEmpDiferencia;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmpSincDesde;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmpUltSinc;
        private System.Windows.Forms.Label lblEstadoEmpresas;

        // Servicio Windows
        private System.Windows.Forms.Panel cardServicio;
        private System.Windows.Forms.Label lblTituloServicio;
        private System.Windows.Forms.Label lblHelpServicio;
        private System.Windows.Forms.Label lblServiceName;
        private System.Windows.Forms.TextBox txtServiceName;
        private System.Windows.Forms.Label lblRutaArchivos;
        private System.Windows.Forms.TextBox txtRutaArchivos;
        private System.Windows.Forms.Button btnExaminarRuta;
        private System.Windows.Forms.Label lblEstadoActualLabel;
        private System.Windows.Forms.Label lblEstadoActualValor;
        private System.Windows.Forms.Button btnRefrescarEstado;
        private System.Windows.Forms.Button btnGuardarServicio;
        private System.Windows.Forms.Button btnInstalarServicio;
        private System.Windows.Forms.Button btnDesinstalarServicio;
        private System.Windows.Forms.Button btnIniciarServicio;
        private System.Windows.Forms.Button btnDetenerServicio;
        private System.Windows.Forms.Label lblEstadoServicioMensaje;
    }
}
