namespace PortalProveedoresEscritorio.Formularios
{
    partial class FormAplicarFactura
    {
        private System.ComponentModel.IContainer components = null;

        // --- Title bar ---
        private System.Windows.Forms.Panel       panelTitleBar;
        private System.Windows.Forms.Label       lblTitulo;
        private System.Windows.Forms.Label       btnMinimizar;
        private System.Windows.Forms.Label       btnMaximizar;
        private System.Windows.Forms.Label       btnCerrar;

        // --- Body (split izq/der) ---
        private System.Windows.Forms.Panel       panelBody;
        private System.Windows.Forms.Panel       panelIzquierdo;

        // --- Izquierda · Datos del proveedor ---
        private System.Windows.Forms.Panel       sec1Card;
        private System.Windows.Forms.Label       sec1Titulo;
        private System.Windows.Forms.Label       lbl_NombreProv;
        private System.Windows.Forms.TextBox     txtProveedor;
        private System.Windows.Forms.Label       lbl_FolioFac;
        private System.Windows.Forms.TextBox     txtFolioFac;
        private System.Windows.Forms.Label       lbl_FechaFac;
        private System.Windows.Forms.DateTimePicker dtpFechaFac1;
        private System.Windows.Forms.Label       lbl_Atraso;
        private System.Windows.Forms.TextBox     txtAtraso;
        private System.Windows.Forms.Label       lbl_FechaSubio;
        private System.Windows.Forms.TextBox     txtFechaSubio;
        private System.Windows.Forms.Label       lbl_Sugerida;
        private System.Windows.Forms.TextBox     txtSugerida;
        private System.Windows.Forms.Label       lbl_Total;
        private System.Windows.Forms.TextBox     txtTotal;
        private System.Windows.Forms.Label       lbl_UUID;
        private System.Windows.Forms.TextBox     txtUUID;

        // --- Izquierda · Descripción de la compra ---
        private System.Windows.Forms.Panel       sec2Card;
        private System.Windows.Forms.Label       sec2Titulo;
        private System.Windows.Forms.Label       lbl_Serie;
        private System.Windows.Forms.ComboBox    cbSerie;
        private System.Windows.Forms.Label       lbl_Condiciones;
        private System.Windows.Forms.ComboBox    cbCondiciones;
        private System.Windows.Forms.Label       lbl_Articulo;
        private System.Windows.Forms.ComboBox    cbArticulo;
        private System.Windows.Forms.Button      btnBuscarArticulo;
        private System.Windows.Forms.Label       lbl_DescCompra;
        private System.Windows.Forms.TextBox     rtDesc;

        // --- Derecha · Factura del proveedor (preview grande inline) ---
        private System.Windows.Forms.Panel       sec3Card;
        private System.Windows.Forms.Panel       sec3Header;
        private System.Windows.Forms.Label       sec3Titulo;
        private System.Windows.Forms.Label       sec3Subtitulo;
        private System.Windows.Forms.Panel       sec3Tabs;
        private System.Windows.Forms.Button      btnTabPdf;
        private System.Windows.Forms.Button      btnTabXml;
        private System.Windows.Forms.Button      btnTabAdjuntos;
        private System.Windows.Forms.Button      btnAbrirExterno;
        private System.Windows.Forms.Panel       sec3VistaContenedor;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.TextBox     txtVistaXml;
        private System.Windows.Forms.DataGridView dgvAdjuntos;
        private System.Windows.Forms.DataGridViewTextBoxColumn   colAdjNombre;
        private System.Windows.Forms.DataGridViewTextBoxColumn   colAdjTamano;
        private System.Windows.Forms.DataGridViewButtonColumn    colAdjDescargar;
        private System.Windows.Forms.DataGridViewTextBoxColumn   colAdjId;
        private System.Windows.Forms.Label       lblVistaCargando;

        // --- Estado + progreso ---
        private System.Windows.Forms.Panel       panelEstado;
        private System.Windows.Forms.Label       lblEstado;
        private System.Windows.Forms.ProgressBar barProgreso;

        // --- Botones ---
        private System.Windows.Forms.Panel       panelBotones;
        private System.Windows.Forms.Button      btnAplicar;
        private System.Windows.Forms.Button      btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.btnMinimizar = new System.Windows.Forms.Label();
            this.btnMaximizar = new System.Windows.Forms.Label();
            this.btnCerrar = new System.Windows.Forms.Label();
            this.panelBody = new System.Windows.Forms.Panel();
            this.sec3Card = new System.Windows.Forms.Panel();
            this.sec3VistaContenedor = new System.Windows.Forms.Panel();
            this.dgvAdjuntos = new System.Windows.Forms.DataGridView();
            this.colAdjNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAdjTamano = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAdjDescargar = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colAdjId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtVistaXml = new System.Windows.Forms.TextBox();
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.lblVistaCargando = new System.Windows.Forms.Label();
            this.sec3Tabs = new System.Windows.Forms.Panel();
            this.btnAbrirExterno = new System.Windows.Forms.Button();
            this.btnTabAdjuntos = new System.Windows.Forms.Button();
            this.btnTabXml = new System.Windows.Forms.Button();
            this.btnTabPdf = new System.Windows.Forms.Button();
            this.sec3Header = new System.Windows.Forms.Panel();
            this.sec3Subtitulo = new System.Windows.Forms.Label();
            this.sec3Titulo = new System.Windows.Forms.Label();
            this.panelIzquierdo = new System.Windows.Forms.Panel();
            this.sec2Card = new System.Windows.Forms.Panel();
            this.sec2Titulo = new System.Windows.Forms.Label();
            this.lbl_Serie = new System.Windows.Forms.Label();
            this.cbSerie = new System.Windows.Forms.ComboBox();
            this.lbl_Condiciones = new System.Windows.Forms.Label();
            this.cbCondiciones = new System.Windows.Forms.ComboBox();
            this.lbl_Articulo = new System.Windows.Forms.Label();
            this.cbArticulo = new System.Windows.Forms.ComboBox();
            this.btnBuscarArticulo = new System.Windows.Forms.Button();
            this.lbl_DescCompra = new System.Windows.Forms.Label();
            this.rtDesc = new System.Windows.Forms.TextBox();
            this.sec1Card = new System.Windows.Forms.Panel();
            this.sec1Titulo = new System.Windows.Forms.Label();
            this.lbl_NombreProv = new System.Windows.Forms.Label();
            this.txtProveedor = new System.Windows.Forms.TextBox();
            this.lbl_FolioFac = new System.Windows.Forms.Label();
            this.txtFolioFac = new System.Windows.Forms.TextBox();
            this.lbl_FechaFac = new System.Windows.Forms.Label();
            this.dtpFechaFac1 = new System.Windows.Forms.DateTimePicker();
            this.lbl_Atraso = new System.Windows.Forms.Label();
            this.txtAtraso = new System.Windows.Forms.TextBox();
            this.lbl_FechaSubio = new System.Windows.Forms.Label();
            this.txtFechaSubio = new System.Windows.Forms.TextBox();
            this.lbl_Sugerida = new System.Windows.Forms.Label();
            this.txtSugerida = new System.Windows.Forms.TextBox();
            this.lbl_Total = new System.Windows.Forms.Label();
            this.txtTotal = new System.Windows.Forms.TextBox();
            this.lbl_UUID = new System.Windows.Forms.Label();
            this.txtUUID = new System.Windows.Forms.TextBox();
            this.panelEstado = new System.Windows.Forms.Panel();
            this.lblEstado = new System.Windows.Forms.Label();
            this.barProgreso = new System.Windows.Forms.ProgressBar();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.btnAplicar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.panelTitleBar.SuspendLayout();
            this.panelBody.SuspendLayout();
            this.sec3Card.SuspendLayout();
            this.sec3VistaContenedor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdjuntos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.sec3Tabs.SuspendLayout();
            this.sec3Header.SuspendLayout();
            this.panelIzquierdo.SuspendLayout();
            this.sec2Card.SuspendLayout();
            this.sec1Card.SuspendLayout();
            this.panelEstado.SuspendLayout();
            this.panelBotones.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.panelTitleBar.Controls.Add(this.lblTitulo);
            this.panelTitleBar.Controls.Add(this.btnMinimizar);
            this.panelTitleBar.Controls.Add(this.btnMaximizar);
            this.panelTitleBar.Controls.Add(this.btnCerrar);
            this.panelTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Location = new System.Drawing.Point(0, 0);
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.Size = new System.Drawing.Size(1220, 44);
            this.panelTitleBar.TabIndex = 3;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(16, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(1100, 44);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Aplicación de la factura al módulo de compras y cuentas por cobrar";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnMinimizar
            // 
            this.btnMinimizar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimizar.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimizar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMinimizar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnMinimizar.ForeColor = System.Drawing.Color.White;
            this.btnMinimizar.Location = new System.Drawing.Point(1112, 0);
            this.btnMinimizar.Name = "btnMinimizar";
            this.btnMinimizar.Size = new System.Drawing.Size(36, 44);
            this.btnMinimizar.TabIndex = 1;
            this.btnMinimizar.Text = "─";
            this.btnMinimizar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnMaximizar
            // 
            this.btnMaximizar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMaximizar.BackColor = System.Drawing.Color.Transparent;
            this.btnMaximizar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMaximizar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnMaximizar.ForeColor = System.Drawing.Color.White;
            this.btnMaximizar.Location = new System.Drawing.Point(1148, 0);
            this.btnMaximizar.Name = "btnMaximizar";
            this.btnMaximizar.Size = new System.Drawing.Size(36, 44);
            this.btnMaximizar.TabIndex = 2;
            this.btnMaximizar.Text = "□";
            this.btnMaximizar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.BackColor = System.Drawing.Color.Transparent;
            this.btnCerrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(1184, 0);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(36, 44);
            this.btnCerrar.TabIndex = 3;
            this.btnCerrar.Text = "✕";
            this.btnCerrar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelBody
            // 
            this.panelBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.panelBody.Controls.Add(this.sec3Card);
            this.panelBody.Controls.Add(this.panelIzquierdo);
            this.panelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBody.Location = new System.Drawing.Point(0, 44);
            this.panelBody.Name = "panelBody";
            this.panelBody.Padding = new System.Windows.Forms.Padding(16);
            this.panelBody.Size = new System.Drawing.Size(1220, 592);
            this.panelBody.TabIndex = 0;
            // 
            // sec3Card
            // 
            this.sec3Card.BackColor = System.Drawing.Color.White;
            this.sec3Card.Controls.Add(this.sec3VistaContenedor);
            this.sec3Card.Controls.Add(this.sec3Tabs);
            this.sec3Card.Controls.Add(this.sec3Header);
            this.sec3Card.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sec3Card.Location = new System.Drawing.Point(536, 16);
            this.sec3Card.Name = "sec3Card";
            this.sec3Card.Size = new System.Drawing.Size(668, 560);
            this.sec3Card.TabIndex = 0;
            // 
            // sec3VistaContenedor
            // 
            this.sec3VistaContenedor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.sec3VistaContenedor.Controls.Add(this.dgvAdjuntos);
            this.sec3VistaContenedor.Controls.Add(this.txtVistaXml);
            this.sec3VistaContenedor.Controls.Add(this.webView);
            this.sec3VistaContenedor.Controls.Add(this.lblVistaCargando);
            this.sec3VistaContenedor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sec3VistaContenedor.Location = new System.Drawing.Point(0, 104);
            this.sec3VistaContenedor.Name = "sec3VistaContenedor";
            this.sec3VistaContenedor.Size = new System.Drawing.Size(668, 456);
            this.sec3VistaContenedor.TabIndex = 0;
            // 
            // dgvAdjuntos
            // 
            this.dgvAdjuntos.AllowUserToAddRows = false;
            this.dgvAdjuntos.AllowUserToDeleteRows = false;
            this.dgvAdjuntos.AllowUserToResizeRows = false;
            this.dgvAdjuntos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAdjuntos.BackgroundColor = System.Drawing.Color.White;
            this.dgvAdjuntos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvAdjuntos.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvAdjuntos.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvAdjuntos.ColumnHeadersHeight = 36;
            this.dgvAdjuntos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAdjNombre,
            this.colAdjTamano,
            this.colAdjDescargar,
            this.colAdjId});
            this.dgvAdjuntos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAdjuntos.EnableHeadersVisualStyles = false;
            this.dgvAdjuntos.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.dgvAdjuntos.Location = new System.Drawing.Point(0, 0);
            this.dgvAdjuntos.MultiSelect = false;
            this.dgvAdjuntos.Name = "dgvAdjuntos";
            this.dgvAdjuntos.RowHeadersVisible = false;
            this.dgvAdjuntos.RowTemplate.Height = 40;
            this.dgvAdjuntos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAdjuntos.Size = new System.Drawing.Size(668, 456);
            this.dgvAdjuntos.TabIndex = 0;
            this.dgvAdjuntos.Visible = false;
            // 
            // colAdjNombre
            // 
            this.colAdjNombre.FillWeight = 60F;
            this.colAdjNombre.HeaderText = "Archivo";
            this.colAdjNombre.Name = "colAdjNombre";
            this.colAdjNombre.ReadOnly = true;
            // 
            // colAdjTamano
            // 
            this.colAdjTamano.FillWeight = 18F;
            this.colAdjTamano.HeaderText = "Tamaño";
            this.colAdjTamano.Name = "colAdjTamano";
            this.colAdjTamano.ReadOnly = true;
            // 
            // colAdjDescargar
            // 
            this.colAdjDescargar.FillWeight = 22F;
            this.colAdjDescargar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colAdjDescargar.HeaderText = "";
            this.colAdjDescargar.Name = "colAdjDescargar";
            this.colAdjDescargar.Text = "📥 Descargar";
            this.colAdjDescargar.UseColumnTextForButtonValue = true;
            // 
            // colAdjId
            // 
            this.colAdjId.HeaderText = "Id";
            this.colAdjId.Name = "colAdjId";
            this.colAdjId.ReadOnly = true;
            this.colAdjId.Visible = false;
            // 
            // txtVistaXml
            // 
            this.txtVistaXml.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtVistaXml.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtVistaXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVistaXml.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.txtVistaXml.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtVistaXml.Location = new System.Drawing.Point(0, 0);
            this.txtVistaXml.Multiline = true;
            this.txtVistaXml.Name = "txtVistaXml";
            this.txtVistaXml.ReadOnly = true;
            this.txtVistaXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtVistaXml.Size = new System.Drawing.Size(668, 456);
            this.txtVistaXml.TabIndex = 1;
            this.txtVistaXml.Visible = false;
            this.txtVistaXml.WordWrap = false;
            // 
            // webView
            // 
            this.webView.AllowExternalDrop = false;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 0);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(668, 456);
            this.webView.TabIndex = 2;
            this.webView.Visible = false;
            this.webView.ZoomFactor = 1D;
            // 
            // lblVistaCargando
            // 
            this.lblVistaCargando.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVistaCargando.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblVistaCargando.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblVistaCargando.Location = new System.Drawing.Point(0, 0);
            this.lblVistaCargando.Name = "lblVistaCargando";
            this.lblVistaCargando.Size = new System.Drawing.Size(668, 456);
            this.lblVistaCargando.TabIndex = 3;
            this.lblVistaCargando.Text = "Cargando vista previa…";
            this.lblVistaCargando.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sec3Tabs
            // 
            this.sec3Tabs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.sec3Tabs.Controls.Add(this.btnAbrirExterno);
            this.sec3Tabs.Controls.Add(this.btnTabAdjuntos);
            this.sec3Tabs.Controls.Add(this.btnTabXml);
            this.sec3Tabs.Controls.Add(this.btnTabPdf);
            this.sec3Tabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.sec3Tabs.Location = new System.Drawing.Point(0, 56);
            this.sec3Tabs.Name = "sec3Tabs";
            this.sec3Tabs.Padding = new System.Windows.Forms.Padding(20, 8, 20, 8);
            this.sec3Tabs.Size = new System.Drawing.Size(668, 48);
            this.sec3Tabs.TabIndex = 1;
            // 
            // btnAbrirExterno
            // 
            this.btnAbrirExterno.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbrirExterno.BackColor = System.Drawing.Color.White;
            this.btnAbrirExterno.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAbrirExterno.Enabled = false;
            this.btnAbrirExterno.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.btnAbrirExterno.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbrirExterno.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnAbrirExterno.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.btnAbrirExterno.Location = new System.Drawing.Point(142, 8);
            this.btnAbrirExterno.Name = "btnAbrirExterno";
            this.btnAbrirExterno.Size = new System.Drawing.Size(154, 32);
            this.btnAbrirExterno.TabIndex = 0;
            this.btnAbrirExterno.Text = "↗  Abrir externo";
            this.btnAbrirExterno.UseVisualStyleBackColor = false;
            this.btnAbrirExterno.Click += new System.EventHandler(this.btnAbrirExterno_Click);
            // 
            // btnTabAdjuntos
            // 
            this.btnTabAdjuntos.BackColor = System.Drawing.Color.White;
            this.btnTabAdjuntos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabAdjuntos.Enabled = false;
            this.btnTabAdjuntos.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.btnTabAdjuntos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabAdjuntos.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabAdjuntos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.btnTabAdjuntos.Location = new System.Drawing.Point(250, 8);
            this.btnTabAdjuntos.Name = "btnTabAdjuntos";
            this.btnTabAdjuntos.Size = new System.Drawing.Size(160, 32);
            this.btnTabAdjuntos.TabIndex = 1;
            this.btnTabAdjuntos.Text = "📎  Adjuntos";
            this.btnTabAdjuntos.UseVisualStyleBackColor = false;
            this.btnTabAdjuntos.Click += new System.EventHandler(this.btnTabAdjuntos_Click);
            // 
            // btnTabXml
            // 
            this.btnTabXml.BackColor = System.Drawing.Color.White;
            this.btnTabXml.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabXml.Enabled = false;
            this.btnTabXml.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.btnTabXml.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabXml.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabXml.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.btnTabXml.Location = new System.Drawing.Point(135, 8);
            this.btnTabXml.Name = "btnTabXml";
            this.btnTabXml.Size = new System.Drawing.Size(110, 32);
            this.btnTabXml.TabIndex = 2;
            this.btnTabXml.Text = "📑  XML";
            this.btnTabXml.UseVisualStyleBackColor = false;
            this.btnTabXml.Click += new System.EventHandler(this.btnTabXml_Click);
            // 
            // btnTabPdf
            // 
            this.btnTabPdf.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnTabPdf.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabPdf.Enabled = false;
            this.btnTabPdf.FlatAppearance.BorderSize = 0;
            this.btnTabPdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabPdf.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabPdf.ForeColor = System.Drawing.Color.White;
            this.btnTabPdf.Location = new System.Drawing.Point(20, 8);
            this.btnTabPdf.Name = "btnTabPdf";
            this.btnTabPdf.Size = new System.Drawing.Size(110, 32);
            this.btnTabPdf.TabIndex = 3;
            this.btnTabPdf.Text = "📄  PDF";
            this.btnTabPdf.UseVisualStyleBackColor = false;
            this.btnTabPdf.Click += new System.EventHandler(this.btnTabPdf_Click);
            // 
            // sec3Header
            // 
            this.sec3Header.BackColor = System.Drawing.Color.White;
            this.sec3Header.Controls.Add(this.sec3Subtitulo);
            this.sec3Header.Controls.Add(this.sec3Titulo);
            this.sec3Header.Dock = System.Windows.Forms.DockStyle.Top;
            this.sec3Header.Location = new System.Drawing.Point(0, 0);
            this.sec3Header.Name = "sec3Header";
            this.sec3Header.Padding = new System.Windows.Forms.Padding(20, 12, 20, 4);
            this.sec3Header.Size = new System.Drawing.Size(668, 56);
            this.sec3Header.TabIndex = 2;
            // 
            // sec3Subtitulo
            // 
            this.sec3Subtitulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.sec3Subtitulo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.sec3Subtitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.sec3Subtitulo.Location = new System.Drawing.Point(20, 36);
            this.sec3Subtitulo.Name = "sec3Subtitulo";
            this.sec3Subtitulo.Size = new System.Drawing.Size(628, 18);
            this.sec3Subtitulo.TabIndex = 0;
            this.sec3Subtitulo.Text = "Cargando CFDI del portal…";
            // 
            // sec3Titulo
            // 
            this.sec3Titulo.Dock = System.Windows.Forms.DockStyle.Top;
            this.sec3Titulo.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.sec3Titulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.sec3Titulo.Location = new System.Drawing.Point(20, 12);
            this.sec3Titulo.Name = "sec3Titulo";
            this.sec3Titulo.Size = new System.Drawing.Size(628, 24);
            this.sec3Titulo.TabIndex = 1;
            this.sec3Titulo.Text = "Factura del proveedor";
            // 
            // panelIzquierdo
            // 
            this.panelIzquierdo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.panelIzquierdo.Controls.Add(this.sec2Card);
            this.panelIzquierdo.Controls.Add(this.sec1Card);
            this.panelIzquierdo.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelIzquierdo.Location = new System.Drawing.Point(16, 16);
            this.panelIzquierdo.Name = "panelIzquierdo";
            this.panelIzquierdo.Padding = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.panelIzquierdo.Size = new System.Drawing.Size(520, 560);
            this.panelIzquierdo.TabIndex = 1;
            // 
            // sec2Card
            // 
            this.sec2Card.BackColor = System.Drawing.Color.White;
            this.sec2Card.Controls.Add(this.sec2Titulo);
            this.sec2Card.Controls.Add(this.lbl_Serie);
            this.sec2Card.Controls.Add(this.cbSerie);
            this.sec2Card.Controls.Add(this.lbl_Condiciones);
            this.sec2Card.Controls.Add(this.cbCondiciones);
            this.sec2Card.Controls.Add(this.lbl_Articulo);
            this.sec2Card.Controls.Add(this.cbArticulo);
            this.sec2Card.Controls.Add(this.btnBuscarArticulo);
            this.sec2Card.Controls.Add(this.lbl_DescCompra);
            this.sec2Card.Controls.Add(this.rtDesc);
            this.sec2Card.Dock = System.Windows.Forms.DockStyle.Top;
            this.sec2Card.Location = new System.Drawing.Point(0, 320);
            this.sec2Card.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.sec2Card.Name = "sec2Card";
            this.sec2Card.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);
            this.sec2Card.Size = new System.Drawing.Size(504, 230);
            this.sec2Card.TabIndex = 0;
            // 
            // sec2Titulo
            // 
            this.sec2Titulo.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.sec2Titulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.sec2Titulo.Location = new System.Drawing.Point(20, 8);
            this.sec2Titulo.Name = "sec2Titulo";
            this.sec2Titulo.Size = new System.Drawing.Size(400, 22);
            this.sec2Titulo.TabIndex = 0;
            this.sec2Titulo.Text = "Descripción de la compra";
            //
            // lbl_Serie
            //
            this.lbl_Serie.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Serie.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_Serie.Location = new System.Drawing.Point(20, 42);
            this.lbl_Serie.Name = "lbl_Serie";
            this.lbl_Serie.Size = new System.Drawing.Size(160, 22);
            this.lbl_Serie.TabIndex = 20;
            this.lbl_Serie.Text = "Serie del folio";
            this.lbl_Serie.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cbSerie
            //
            this.cbSerie.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSerie.Enabled = false;
            this.cbSerie.Location = new System.Drawing.Point(180, 40);
            this.cbSerie.Name = "cbSerie";
            this.cbSerie.Size = new System.Drawing.Size(300, 25);
            this.cbSerie.TabIndex = 21;
            //
            // lbl_Condiciones
            //
            this.lbl_Condiciones.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Condiciones.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_Condiciones.Location = new System.Drawing.Point(20, 74);
            this.lbl_Condiciones.Name = "lbl_Condiciones";
            this.lbl_Condiciones.Size = new System.Drawing.Size(160, 22);
            this.lbl_Condiciones.TabIndex = 1;
            this.lbl_Condiciones.Text = "Condiciones de pago";
            this.lbl_Condiciones.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cbCondiciones
            //
            this.cbCondiciones.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCondiciones.Enabled = false;
            this.cbCondiciones.Location = new System.Drawing.Point(180, 72);
            this.cbCondiciones.Name = "cbCondiciones";
            this.cbCondiciones.Size = new System.Drawing.Size(300, 25);
            this.cbCondiciones.TabIndex = 2;
            // 
            // lbl_Articulo
            // 
            this.lbl_Articulo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Articulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_Articulo.Location = new System.Drawing.Point(20, 106);
            this.lbl_Articulo.Name = "lbl_Articulo";
            this.lbl_Articulo.Size = new System.Drawing.Size(160, 22);
            this.lbl_Articulo.TabIndex = 3;
            this.lbl_Articulo.Text = "Artículo general";
            this.lbl_Articulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbArticulo
            // 
            this.cbArticulo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbArticulo.Enabled = false;
            this.cbArticulo.Location = new System.Drawing.Point(180, 104);
            this.cbArticulo.Name = "cbArticulo";
            this.cbArticulo.Size = new System.Drawing.Size(160, 25);
            this.cbArticulo.TabIndex = 4;
            // 
            // btnBuscarArticulo
            // 
            this.btnBuscarArticulo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.btnBuscarArticulo.Enabled = false;
            this.btnBuscarArticulo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.btnBuscarArticulo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscarArticulo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBuscarArticulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.btnBuscarArticulo.Location = new System.Drawing.Point(345, 104);
            this.btnBuscarArticulo.Name = "btnBuscarArticulo";
            this.btnBuscarArticulo.Size = new System.Drawing.Size(135, 26);
            this.btnBuscarArticulo.TabIndex = 5;
            this.btnBuscarArticulo.Text = "🔍  Buscar Articulo";
            this.btnBuscarArticulo.UseVisualStyleBackColor = false;
            // 
            // lbl_DescCompra
            // 
            this.lbl_DescCompra.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_DescCompra.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_DescCompra.Location = new System.Drawing.Point(20, 138);
            this.lbl_DescCompra.Name = "lbl_DescCompra";
            this.lbl_DescCompra.Size = new System.Drawing.Size(160, 22);
            this.lbl_DescCompra.TabIndex = 6;
            this.lbl_DescCompra.Text = "Descripción de la compra";
            this.lbl_DescCompra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rtDesc
            // 
            this.rtDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtDesc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rtDesc.Location = new System.Drawing.Point(180, 138);
            this.rtDesc.Multiline = true;
            this.rtDesc.Name = "rtDesc";
            this.rtDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.rtDesc.Size = new System.Drawing.Size(300, 68);
            this.rtDesc.TabIndex = 7;
            // 
            // sec1Card
            // 
            this.sec1Card.BackColor = System.Drawing.Color.White;
            this.sec1Card.Controls.Add(this.sec1Titulo);
            this.sec1Card.Controls.Add(this.lbl_NombreProv);
            this.sec1Card.Controls.Add(this.txtProveedor);
            this.sec1Card.Controls.Add(this.lbl_FolioFac);
            this.sec1Card.Controls.Add(this.txtFolioFac);
            this.sec1Card.Controls.Add(this.lbl_FechaFac);
            this.sec1Card.Controls.Add(this.dtpFechaFac1);
            this.sec1Card.Controls.Add(this.lbl_Atraso);
            this.sec1Card.Controls.Add(this.txtAtraso);
            this.sec1Card.Controls.Add(this.lbl_FechaSubio);
            this.sec1Card.Controls.Add(this.txtFechaSubio);
            this.sec1Card.Controls.Add(this.lbl_Sugerida);
            this.sec1Card.Controls.Add(this.txtSugerida);
            this.sec1Card.Controls.Add(this.lbl_Total);
            this.sec1Card.Controls.Add(this.txtTotal);
            this.sec1Card.Controls.Add(this.lbl_UUID);
            this.sec1Card.Controls.Add(this.txtUUID);
            this.sec1Card.Dock = System.Windows.Forms.DockStyle.Top;
            this.sec1Card.Location = new System.Drawing.Point(0, 0);
            this.sec1Card.Name = "sec1Card";
            this.sec1Card.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);
            this.sec1Card.Size = new System.Drawing.Size(504, 320);
            this.sec1Card.TabIndex = 1;
            // 
            // sec1Titulo
            // 
            this.sec1Titulo.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.sec1Titulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.sec1Titulo.Location = new System.Drawing.Point(20, 8);
            this.sec1Titulo.Name = "sec1Titulo";
            this.sec1Titulo.Size = new System.Drawing.Size(400, 22);
            this.sec1Titulo.TabIndex = 0;
            this.sec1Titulo.Text = "Datos del proveedor";
            // 
            // lbl_NombreProv
            // 
            this.lbl_NombreProv.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_NombreProv.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_NombreProv.Location = new System.Drawing.Point(20, 44);
            this.lbl_NombreProv.Name = "lbl_NombreProv";
            this.lbl_NombreProv.Size = new System.Drawing.Size(160, 22);
            this.lbl_NombreProv.TabIndex = 1;
            this.lbl_NombreProv.Text = "Nombre del proveedor:";
            this.lbl_NombreProv.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtProveedor
            // 
            this.txtProveedor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtProveedor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtProveedor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProveedor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtProveedor.Location = new System.Drawing.Point(180, 40);
            this.txtProveedor.Name = "txtProveedor";
            this.txtProveedor.ReadOnly = true;
            this.txtProveedor.Size = new System.Drawing.Size(300, 23);
            this.txtProveedor.TabIndex = 2;
            // 
            // lbl_FolioFac
            // 
            this.lbl_FolioFac.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FolioFac.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_FolioFac.Location = new System.Drawing.Point(20, 76);
            this.lbl_FolioFac.Name = "lbl_FolioFac";
            this.lbl_FolioFac.Size = new System.Drawing.Size(160, 22);
            this.lbl_FolioFac.TabIndex = 3;
            this.lbl_FolioFac.Text = "Folio factura";
            this.lbl_FolioFac.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtFolioFac
            // 
            this.txtFolioFac.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtFolioFac.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFolioFac.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFolioFac.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtFolioFac.Location = new System.Drawing.Point(180, 72);
            this.txtFolioFac.Name = "txtFolioFac";
            this.txtFolioFac.ReadOnly = true;
            this.txtFolioFac.Size = new System.Drawing.Size(300, 23);
            this.txtFolioFac.TabIndex = 4;
            // 
            // lbl_FechaFac
            // 
            this.lbl_FechaFac.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FechaFac.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_FechaFac.Location = new System.Drawing.Point(20, 108);
            this.lbl_FechaFac.Name = "lbl_FechaFac";
            this.lbl_FechaFac.Size = new System.Drawing.Size(160, 22);
            this.lbl_FechaFac.TabIndex = 5;
            this.lbl_FechaFac.Text = "Fecha de la factura";
            this.lbl_FechaFac.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dtpFechaFac1
            // 
            this.dtpFechaFac1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpFechaFac1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaFac1.Location = new System.Drawing.Point(180, 104);
            this.dtpFechaFac1.Name = "dtpFechaFac1";
            this.dtpFechaFac1.Size = new System.Drawing.Size(300, 23);
            this.dtpFechaFac1.TabIndex = 6;
            // 
            // lbl_Atraso
            // 
            this.lbl_Atraso.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Atraso.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_Atraso.Location = new System.Drawing.Point(20, 140);
            this.lbl_Atraso.Name = "lbl_Atraso";
            this.lbl_Atraso.Size = new System.Drawing.Size(160, 22);
            this.lbl_Atraso.TabIndex = 7;
            this.lbl_Atraso.Text = "Atraso (Dias)";
            this.lbl_Atraso.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtAtraso
            // 
            this.txtAtraso.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtAtraso.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAtraso.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtAtraso.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtAtraso.Location = new System.Drawing.Point(180, 136);
            this.txtAtraso.Name = "txtAtraso";
            this.txtAtraso.ReadOnly = true;
            this.txtAtraso.Size = new System.Drawing.Size(300, 23);
            this.txtAtraso.TabIndex = 8;
            // 
            // lbl_FechaSubio
            // 
            this.lbl_FechaSubio.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FechaSubio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_FechaSubio.Location = new System.Drawing.Point(20, 172);
            this.lbl_FechaSubio.Name = "lbl_FechaSubio";
            this.lbl_FechaSubio.Size = new System.Drawing.Size(160, 22);
            this.lbl_FechaSubio.TabIndex = 9;
            this.lbl_FechaSubio.Text = "Fecha subio al portal";
            this.lbl_FechaSubio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtFechaSubio
            // 
            this.txtFechaSubio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtFechaSubio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFechaSubio.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFechaSubio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtFechaSubio.Location = new System.Drawing.Point(180, 168);
            this.txtFechaSubio.Name = "txtFechaSubio";
            this.txtFechaSubio.ReadOnly = true;
            this.txtFechaSubio.Size = new System.Drawing.Size(300, 23);
            this.txtFechaSubio.TabIndex = 10;
            // 
            // lbl_Sugerida
            // 
            this.lbl_Sugerida.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Sugerida.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_Sugerida.Location = new System.Drawing.Point(20, 204);
            this.lbl_Sugerida.Name = "lbl_Sugerida";
            this.lbl_Sugerida.Size = new System.Drawing.Size(160, 22);
            this.lbl_Sugerida.TabIndex = 11;
            this.lbl_Sugerida.Text = "Fecha sugerida pago";
            this.lbl_Sugerida.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSugerida
            // 
            this.txtSugerida.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtSugerida.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSugerida.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSugerida.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtSugerida.Location = new System.Drawing.Point(180, 200);
            this.txtSugerida.Name = "txtSugerida";
            this.txtSugerida.ReadOnly = true;
            this.txtSugerida.Size = new System.Drawing.Size(300, 23);
            this.txtSugerida.TabIndex = 12;
            // 
            // lbl_Total
            // 
            this.lbl_Total.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lbl_Total.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lbl_Total.Location = new System.Drawing.Point(20, 240);
            this.lbl_Total.Name = "lbl_Total";
            this.lbl_Total.Size = new System.Drawing.Size(160, 22);
            this.lbl_Total.TabIndex = 13;
            this.lbl_Total.Text = "Total";
            this.lbl_Total.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtTotal
            // 
            this.txtTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.txtTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.txtTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.txtTotal.Location = new System.Drawing.Point(180, 236);
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.ReadOnly = true;
            this.txtTotal.Size = new System.Drawing.Size(300, 27);
            this.txtTotal.TabIndex = 14;
            // 
            // lbl_UUID
            // 
            this.lbl_UUID.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_UUID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lbl_UUID.Location = new System.Drawing.Point(20, 276);
            this.lbl_UUID.Name = "lbl_UUID";
            this.lbl_UUID.Size = new System.Drawing.Size(160, 22);
            this.lbl_UUID.TabIndex = 15;
            this.lbl_UUID.Text = "UUID";
            this.lbl_UUID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtUUID
            // 
            this.txtUUID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtUUID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUUID.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.txtUUID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtUUID.Location = new System.Drawing.Point(180, 272);
            this.txtUUID.Name = "txtUUID";
            this.txtUUID.ReadOnly = true;
            this.txtUUID.Size = new System.Drawing.Size(300, 21);
            this.txtUUID.TabIndex = 16;
            // 
            // panelEstado
            // 
            this.panelEstado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.panelEstado.Controls.Add(this.lblEstado);
            this.panelEstado.Controls.Add(this.barProgreso);
            this.panelEstado.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEstado.Location = new System.Drawing.Point(0, 636);
            this.panelEstado.Name = "panelEstado";
            this.panelEstado.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.panelEstado.Size = new System.Drawing.Size(1220, 56);
            this.panelEstado.TabIndex = 1;
            // 
            // lblEstado
            // 
            this.lblEstado.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblEstado.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblEstado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblEstado.Location = new System.Drawing.Point(20, 18);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(1180, 28);
            this.lblEstado.TabIndex = 0;
            this.lblEstado.Text = "Listo para aplicar.";
            // 
            // barProgreso
            // 
            this.barProgreso.Dock = System.Windows.Forms.DockStyle.Top;
            this.barProgreso.Location = new System.Drawing.Point(20, 10);
            this.barProgreso.Name = "barProgreso";
            this.barProgreso.Size = new System.Drawing.Size(1180, 8);
            this.barProgreso.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.barProgreso.TabIndex = 1;
            this.barProgreso.Visible = false;
            // 
            // panelBotones
            // 
            this.panelBotones.BackColor = System.Drawing.Color.White;
            this.panelBotones.Controls.Add(this.btnAplicar);
            this.panelBotones.Controls.Add(this.btnCancelar);
            this.panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotones.Location = new System.Drawing.Point(0, 692);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Padding = new System.Windows.Forms.Padding(20, 14, 20, 14);
            this.panelBotones.Size = new System.Drawing.Size(1220, 68);
            this.panelBotones.TabIndex = 2;
            // 
            // btnAplicar
            // 
            this.btnAplicar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAplicar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnAplicar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAplicar.FlatAppearance.BorderSize = 0;
            this.btnAplicar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAplicar.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnAplicar.ForeColor = System.Drawing.Color.White;
            this.btnAplicar.Location = new System.Drawing.Point(1014, 14);
            this.btnAplicar.Name = "btnAplicar";
            this.btnAplicar.Size = new System.Drawing.Size(186, 40);
            this.btnAplicar.TabIndex = 0;
            this.btnAplicar.Text = "Aplicar Factura a Microsip";
            this.btnAplicar.UseVisualStyleBackColor = false;
            this.btnAplicar.Click += new System.EventHandler(this.btnAplicar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.BackColor = System.Drawing.Color.White;
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnCancelar.Location = new System.Drawing.Point(796, 14);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(212, 40);
            this.btnCancelar.TabIndex = 1;
            this.btnCancelar.Text = "Cancelar aplicación a Microsip";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // FormAplicarFactura
            // 
            this.AcceptButton = this.btnAplicar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(1220, 760);
            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelEstado);
            this.Controls.Add(this.panelBotones);
            this.Controls.Add(this.panelTitleBar);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(1100, 700);
            this.Name = "FormAplicarFactura";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Aplicar compra en Microsip";
            this.panelTitleBar.ResumeLayout(false);
            this.panelBody.ResumeLayout(false);
            this.sec3Card.ResumeLayout(false);
            this.sec3VistaContenedor.ResumeLayout(false);
            this.sec3VistaContenedor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdjuntos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.sec3Tabs.ResumeLayout(false);
            this.sec3Header.ResumeLayout(false);
            this.panelIzquierdo.ResumeLayout(false);
            this.sec2Card.ResumeLayout(false);
            this.sec2Card.PerformLayout();
            this.sec1Card.ResumeLayout(false);
            this.sec1Card.PerformLayout();
            this.panelEstado.ResumeLayout(false);
            this.panelBotones.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
