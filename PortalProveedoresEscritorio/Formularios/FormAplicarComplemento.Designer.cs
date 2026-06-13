namespace PortalProveedoresEscritorio.Formularios
{
    partial class FormAplicarComplemento
    {
        private System.ComponentModel.IContainer components = null;

        // --- Title bar ---
        private System.Windows.Forms.Panel       panelTitleBar;
        private System.Windows.Forms.Label       lblTitulo;
        private System.Windows.Forms.Label       btnMinimizar;
        private System.Windows.Forms.Label       btnCerrar;

        // --- Body (split izq/der) ---
        private System.Windows.Forms.Panel       panelBody;
        private System.Windows.Forms.Panel       panelIzquierdo;

        // --- Izquierda · Datos del complemento ---
        private System.Windows.Forms.Panel       sec1Card;
        private System.Windows.Forms.Label       sec1Titulo;
        private System.Windows.Forms.Label       lbl_NombreProv;
        private System.Windows.Forms.TextBox     txtProveedor;
        private System.Windows.Forms.Label       lbl_FolioPago;
        private System.Windows.Forms.TextBox     txtFolioPago;
        private System.Windows.Forms.Label       lbl_FechaPago;
        private System.Windows.Forms.TextBox     txtFechaPago;
        private System.Windows.Forms.Label       lbl_FechaComp;
        private System.Windows.Forms.TextBox     txtFechaComp;
        private System.Windows.Forms.Label       lbl_UsoCfdi;
        private System.Windows.Forms.TextBox     txtUsoCfdi;
        private System.Windows.Forms.Label       lbl_Monto;
        private System.Windows.Forms.TextBox     txtMonto;
        private System.Windows.Forms.Label       lbl_UUID;
        private System.Windows.Forms.TextBox     txtUUID;

        // --- Izquierda · Crédito asociado en Microsip ---
        private System.Windows.Forms.Panel       sec2Card;
        private System.Windows.Forms.Label       sec2Titulo;
        private System.Windows.Forms.Label       sec2Hint;
        private System.Windows.Forms.Label       lbl_FolioCredito;
        private System.Windows.Forms.TextBox     txtFolioCredito;
        private System.Windows.Forms.Label       lbl_CreditoFk;
        private System.Windows.Forms.TextBox     txtCreditoFk;

        // --- Derecha · Factura del proveedor (preview inline con WebView2) ---
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
            this.components            = new System.ComponentModel.Container();
            this.panelTitleBar         = new System.Windows.Forms.Panel();
            this.lblTitulo             = new System.Windows.Forms.Label();
            this.btnMinimizar          = new System.Windows.Forms.Label();
            this.btnCerrar             = new System.Windows.Forms.Label();

            this.panelBody             = new System.Windows.Forms.Panel();
            this.panelIzquierdo        = new System.Windows.Forms.Panel();

            this.sec1Card              = new System.Windows.Forms.Panel();
            this.sec1Titulo            = new System.Windows.Forms.Label();
            this.lbl_NombreProv        = new System.Windows.Forms.Label();
            this.txtProveedor          = new System.Windows.Forms.TextBox();
            this.lbl_FolioPago         = new System.Windows.Forms.Label();
            this.txtFolioPago          = new System.Windows.Forms.TextBox();
            this.lbl_FechaPago         = new System.Windows.Forms.Label();
            this.txtFechaPago          = new System.Windows.Forms.TextBox();
            this.lbl_FechaComp         = new System.Windows.Forms.Label();
            this.txtFechaComp          = new System.Windows.Forms.TextBox();
            this.lbl_UsoCfdi           = new System.Windows.Forms.Label();
            this.txtUsoCfdi            = new System.Windows.Forms.TextBox();
            this.lbl_Monto             = new System.Windows.Forms.Label();
            this.txtMonto              = new System.Windows.Forms.TextBox();
            this.lbl_UUID              = new System.Windows.Forms.Label();
            this.txtUUID               = new System.Windows.Forms.TextBox();

            this.sec2Card              = new System.Windows.Forms.Panel();
            this.sec2Titulo            = new System.Windows.Forms.Label();
            this.sec2Hint              = new System.Windows.Forms.Label();
            this.lbl_FolioCredito      = new System.Windows.Forms.Label();
            this.txtFolioCredito       = new System.Windows.Forms.TextBox();
            this.lbl_CreditoFk         = new System.Windows.Forms.Label();
            this.txtCreditoFk          = new System.Windows.Forms.TextBox();

            this.sec3Card              = new System.Windows.Forms.Panel();
            this.sec3Header            = new System.Windows.Forms.Panel();
            this.sec3Titulo            = new System.Windows.Forms.Label();
            this.sec3Subtitulo         = new System.Windows.Forms.Label();
            this.sec3Tabs              = new System.Windows.Forms.Panel();
            this.btnTabPdf             = new System.Windows.Forms.Button();
            this.btnTabXml             = new System.Windows.Forms.Button();
            this.btnTabAdjuntos        = new System.Windows.Forms.Button();
            this.btnAbrirExterno       = new System.Windows.Forms.Button();
            this.sec3VistaContenedor   = new System.Windows.Forms.Panel();
            this.webView               = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.txtVistaXml           = new System.Windows.Forms.TextBox();
            this.dgvAdjuntos           = new System.Windows.Forms.DataGridView();
            this.colAdjNombre          = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAdjTamano          = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAdjDescargar       = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colAdjId              = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblVistaCargando      = new System.Windows.Forms.Label();

            this.panelEstado           = new System.Windows.Forms.Panel();
            this.lblEstado             = new System.Windows.Forms.Label();
            this.barProgreso           = new System.Windows.Forms.ProgressBar();

            this.panelBotones          = new System.Windows.Forms.Panel();
            this.btnAplicar            = new System.Windows.Forms.Button();
            this.btnCancelar           = new System.Windows.Forms.Button();

            this.panelTitleBar.SuspendLayout();
            this.panelBody.SuspendLayout();
            this.panelIzquierdo.SuspendLayout();
            this.sec1Card.SuspendLayout();
            this.sec2Card.SuspendLayout();
            this.sec3Card.SuspendLayout();
            this.sec3Header.SuspendLayout();
            this.sec3Tabs.SuspendLayout();
            this.sec3VistaContenedor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdjuntos)).BeginInit();
            this.panelEstado.SuspendLayout();
            this.panelBotones.SuspendLayout();
            this.SuspendLayout();

            // ============ Form ============
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor           = System.Drawing.Color.FromArgb(241, 245, 249);
            this.ClientSize          = new System.Drawing.Size(1250, 760);
            this.MinimumSize         = new System.Drawing.Size(1100, 700);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Asociar CFDI en Microsip";
            this.Font                = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ShowInTaskbar       = false;

            // ============ Title bar ============
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.panelTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Size      = new System.Drawing.Size(1250, 44);
            this.panelTitleBar.Controls.Add(this.lblTitulo);
            this.panelTitleBar.Controls.Add(this.btnMinimizar);
            this.panelTitleBar.Controls.Add(this.btnCerrar);

            this.lblTitulo.Location  = new System.Drawing.Point(16, 0);
            this.lblTitulo.Size      = new System.Drawing.Size(1100, 44);
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitulo.Text      = "Asociar CFDI del complemento al crédito de cuentas por pagar";

            this.btnMinimizar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimizar.Location  = new System.Drawing.Point(1178, 0);
            this.btnMinimizar.Size      = new System.Drawing.Size(36, 44);
            this.btnMinimizar.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnMinimizar.ForeColor = System.Drawing.Color.White;
            this.btnMinimizar.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimizar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnMinimizar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnMinimizar.Text      = "─";

            this.btnCerrar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location  = new System.Drawing.Point(1214, 0);
            this.btnCerrar.Size      = new System.Drawing.Size(36, 44);
            this.btnCerrar.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.BackColor = System.Drawing.Color.Transparent;
            this.btnCerrar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnCerrar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.Text      = "✕";

            // ============ Body + izquierdo ============
            this.panelBody.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.panelBody.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelBody.Padding   = new System.Windows.Forms.Padding(16, 16, 16, 16);

            this.panelIzquierdo.Dock      = System.Windows.Forms.DockStyle.Left;
            this.panelIzquierdo.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelIzquierdo.Size      = new System.Drawing.Size(520, 580);
            this.panelIzquierdo.Padding   = new System.Windows.Forms.Padding(0, 0, 16, 0);

            // ============ Sec 1: Datos del complemento ============
            this.sec1Card.BackColor = System.Drawing.Color.White;
            this.sec1Card.Dock      = System.Windows.Forms.DockStyle.Top;
            this.sec1Card.Size      = new System.Drawing.Size(504, 296);
            this.sec1Card.Padding   = new System.Windows.Forms.Padding(20, 12, 20, 12);
            this.sec1Card.Controls.Add(this.sec1Titulo);
            this.sec1Card.Controls.Add(this.lbl_NombreProv); this.sec1Card.Controls.Add(this.txtProveedor);
            this.sec1Card.Controls.Add(this.lbl_FolioPago);  this.sec1Card.Controls.Add(this.txtFolioPago);
            this.sec1Card.Controls.Add(this.lbl_FechaPago);  this.sec1Card.Controls.Add(this.txtFechaPago);
            this.sec1Card.Controls.Add(this.lbl_FechaComp);  this.sec1Card.Controls.Add(this.txtFechaComp);
            this.sec1Card.Controls.Add(this.lbl_UsoCfdi);    this.sec1Card.Controls.Add(this.txtUsoCfdi);
            this.sec1Card.Controls.Add(this.lbl_Monto);      this.sec1Card.Controls.Add(this.txtMonto);
            this.sec1Card.Controls.Add(this.lbl_UUID);       this.sec1Card.Controls.Add(this.txtUUID);

            this.sec1Titulo.Location  = new System.Drawing.Point(20, 8);
            this.sec1Titulo.Size      = new System.Drawing.Size(400, 22);
            this.sec1Titulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.sec1Titulo.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.sec1Titulo.Text      = "Datos del complemento de pago";

            // --- Nombre del proveedor ---
            this.lbl_NombreProv.Location  = new System.Drawing.Point(20, 44);
            this.lbl_NombreProv.Size      = new System.Drawing.Size(140, 22);
            this.lbl_NombreProv.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_NombreProv.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_NombreProv.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_NombreProv.Text      = "Proveedor";
            this.txtProveedor.Location    = new System.Drawing.Point(160, 40);
            this.txtProveedor.Size        = new System.Drawing.Size(320, 25);
            this.txtProveedor.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProveedor.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtProveedor.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtProveedor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtProveedor.ReadOnly    = true;
            this.txtProveedor.Text        = "";

            // --- Folio del pago ---
            this.lbl_FolioPago.Location  = new System.Drawing.Point(20, 76);
            this.lbl_FolioPago.Size      = new System.Drawing.Size(140, 22);
            this.lbl_FolioPago.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FolioPago.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_FolioPago.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_FolioPago.Text      = "Folio del pago";
            this.txtFolioPago.Location    = new System.Drawing.Point(160, 72);
            this.txtFolioPago.Size        = new System.Drawing.Size(320, 25);
            this.txtFolioPago.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFolioPago.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtFolioPago.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtFolioPago.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFolioPago.ReadOnly    = true;
            this.txtFolioPago.Text        = "";

            // --- Fecha del pago ---
            this.lbl_FechaPago.Location  = new System.Drawing.Point(20, 108);
            this.lbl_FechaPago.Size      = new System.Drawing.Size(140, 22);
            this.lbl_FechaPago.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FechaPago.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_FechaPago.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_FechaPago.Text      = "Fecha del pago";
            this.txtFechaPago.Location    = new System.Drawing.Point(160, 104);
            this.txtFechaPago.Size        = new System.Drawing.Size(320, 25);
            this.txtFechaPago.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFechaPago.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtFechaPago.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtFechaPago.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFechaPago.ReadOnly    = true;
            this.txtFechaPago.Text        = "";

            // --- Fecha del complemento ---
            this.lbl_FechaComp.Location  = new System.Drawing.Point(20, 140);
            this.lbl_FechaComp.Size      = new System.Drawing.Size(140, 22);
            this.lbl_FechaComp.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FechaComp.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_FechaComp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_FechaComp.Text      = "Fecha complemento";
            this.txtFechaComp.Location    = new System.Drawing.Point(160, 136);
            this.txtFechaComp.Size        = new System.Drawing.Size(320, 25);
            this.txtFechaComp.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFechaComp.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtFechaComp.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtFechaComp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFechaComp.ReadOnly    = true;
            this.txtFechaComp.Text        = "";

            // --- Uso CFDI ---
            this.lbl_UsoCfdi.Location  = new System.Drawing.Point(20, 172);
            this.lbl_UsoCfdi.Size      = new System.Drawing.Size(140, 22);
            this.lbl_UsoCfdi.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_UsoCfdi.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_UsoCfdi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_UsoCfdi.Text      = "Uso CFDI";
            this.txtUsoCfdi.Location    = new System.Drawing.Point(160, 168);
            this.txtUsoCfdi.Size        = new System.Drawing.Size(320, 25);
            this.txtUsoCfdi.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtUsoCfdi.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtUsoCfdi.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtUsoCfdi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsoCfdi.ReadOnly    = true;
            this.txtUsoCfdi.Text        = "";

            // --- Monto (destacado, semibold, azul) ---
            this.lbl_Monto.Location  = new System.Drawing.Point(20, 208);
            this.lbl_Monto.Size      = new System.Drawing.Size(140, 22);
            this.lbl_Monto.Font      = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lbl_Monto.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.lbl_Monto.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Monto.Text      = "Monto";
            this.txtMonto.Location    = new System.Drawing.Point(160, 204);
            this.txtMonto.Size        = new System.Drawing.Size(320, 27);
            this.txtMonto.Font        = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.txtMonto.ForeColor   = System.Drawing.Color.FromArgb(37, 99, 235);
            this.txtMonto.BackColor   = System.Drawing.Color.FromArgb(239, 246, 255);
            this.txtMonto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMonto.ReadOnly    = true;
            this.txtMonto.Text        = "";

            // --- UUID monoespacio ---
            this.lbl_UUID.Location  = new System.Drawing.Point(20, 244);
            this.lbl_UUID.Size      = new System.Drawing.Size(140, 22);
            this.lbl_UUID.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_UUID.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_UUID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_UUID.Text      = "UUID";
            this.txtUUID.Location    = new System.Drawing.Point(160, 240);
            this.txtUUID.Size        = new System.Drawing.Size(320, 25);
            this.txtUUID.Font        = new System.Drawing.Font("Consolas", 8.5F);
            this.txtUUID.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtUUID.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtUUID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUUID.ReadOnly    = true;
            this.txtUUID.Text        = "";

            // ============ Sec 2: Crédito asociado ============
            this.sec2Card.BackColor = System.Drawing.Color.White;
            this.sec2Card.Dock      = System.Windows.Forms.DockStyle.Top;
            this.sec2Card.Size      = new System.Drawing.Size(504, 152);
            this.sec2Card.Padding   = new System.Windows.Forms.Padding(20, 12, 20, 12);
            this.sec2Card.Controls.Add(this.sec2Hint);
            this.sec2Card.Controls.Add(this.sec2Titulo);
            this.sec2Card.Controls.Add(this.lbl_FolioCredito); this.sec2Card.Controls.Add(this.txtFolioCredito);
            this.sec2Card.Controls.Add(this.lbl_CreditoFk);    this.sec2Card.Controls.Add(this.txtCreditoFk);

            this.sec2Titulo.Location  = new System.Drawing.Point(20, 8);
            this.sec2Titulo.Size      = new System.Drawing.Size(400, 22);
            this.sec2Titulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.sec2Titulo.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.sec2Titulo.Text      = "Crédito Microsip asociado";

            this.sec2Hint.Location  = new System.Drawing.Point(20, 32);
            this.sec2Hint.Size      = new System.Drawing.Size(460, 32);
            this.sec2Hint.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.sec2Hint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.sec2Hint.Text      = "El complemento se asocia al crédito existente. NO se crea un documento nuevo en Microsip — solo se agrega el CFDI al CFD_RECIBIDOS y se marca TIENE_CFD='S' en DOCTOS_CP.";

            this.lbl_FolioCredito.Location  = new System.Drawing.Point(20, 72);
            this.lbl_FolioCredito.Size      = new System.Drawing.Size(140, 22);
            this.lbl_FolioCredito.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FolioCredito.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_FolioCredito.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_FolioCredito.Text      = "Folio crédito";
            this.txtFolioCredito.Location    = new System.Drawing.Point(160, 68);
            this.txtFolioCredito.Size        = new System.Drawing.Size(320, 25);
            this.txtFolioCredito.Font        = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.txtFolioCredito.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtFolioCredito.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtFolioCredito.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFolioCredito.ReadOnly    = true;
            this.txtFolioCredito.Text        = "";

            this.lbl_CreditoFk.Location  = new System.Drawing.Point(20, 104);
            this.lbl_CreditoFk.Size      = new System.Drawing.Size(140, 22);
            this.lbl_CreditoFk.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_CreditoFk.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_CreditoFk.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_CreditoFk.Text      = "CREDITO_ID";
            this.txtCreditoFk.Location    = new System.Drawing.Point(160, 100);
            this.txtCreditoFk.Size        = new System.Drawing.Size(320, 25);
            this.txtCreditoFk.Font        = new System.Drawing.Font("Consolas", 9F);
            this.txtCreditoFk.ForeColor   = System.Drawing.Color.FromArgb(100, 116, 139);
            this.txtCreditoFk.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtCreditoFk.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCreditoFk.ReadOnly    = true;
            this.txtCreditoFk.Text        = "";

            // ============ Sec 3: Factura del proveedor (derecha) ============
            this.sec3Card.BackColor = System.Drawing.Color.White;
            this.sec3Card.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.sec3Card.Padding   = new System.Windows.Forms.Padding(0);
            this.sec3Card.Controls.Add(this.sec3VistaContenedor);
            this.sec3Card.Controls.Add(this.sec3Tabs);
            this.sec3Card.Controls.Add(this.sec3Header);

            this.sec3Header.Dock      = System.Windows.Forms.DockStyle.Top;
            this.sec3Header.Size      = new System.Drawing.Size(694, 56);
            this.sec3Header.BackColor = System.Drawing.Color.White;
            this.sec3Header.Padding   = new System.Windows.Forms.Padding(20, 12, 20, 4);
            this.sec3Header.Controls.Add(this.sec3Subtitulo);
            this.sec3Header.Controls.Add(this.sec3Titulo);

            this.sec3Titulo.Dock      = System.Windows.Forms.DockStyle.Top;
            this.sec3Titulo.Height    = 24;
            this.sec3Titulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.sec3Titulo.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.sec3Titulo.Text      = "Complemento del proveedor";

            this.sec3Subtitulo.Dock      = System.Windows.Forms.DockStyle.Top;
            this.sec3Subtitulo.Height    = 18;
            this.sec3Subtitulo.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.sec3Subtitulo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.sec3Subtitulo.Text      = "Cargando CFDI del portal…";

            // Tabs PDF / XML / Adjuntos.
            this.sec3Tabs.Dock      = System.Windows.Forms.DockStyle.Top;
            this.sec3Tabs.Size      = new System.Drawing.Size(694, 48);
            this.sec3Tabs.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.sec3Tabs.Padding   = new System.Windows.Forms.Padding(20, 8, 20, 8);
            this.sec3Tabs.Controls.Add(this.btnAbrirExterno);
            this.sec3Tabs.Controls.Add(this.btnTabAdjuntos);
            this.sec3Tabs.Controls.Add(this.btnTabXml);
            this.sec3Tabs.Controls.Add(this.btnTabPdf);

            this.btnTabPdf.Location  = new System.Drawing.Point(20, 8);
            this.btnTabPdf.Size      = new System.Drawing.Size(110, 32);
            this.btnTabPdf.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnTabPdf.ForeColor = System.Drawing.Color.White;
            this.btnTabPdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabPdf.FlatAppearance.BorderSize = 0;
            this.btnTabPdf.Font      = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabPdf.Text      = "📄  PDF";
            this.btnTabPdf.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnTabPdf.Enabled   = false;
            this.btnTabPdf.UseVisualStyleBackColor = false;
            this.btnTabPdf.Click    += new System.EventHandler(this.btnTabPdf_Click);

            this.btnTabXml.Location  = new System.Drawing.Point(135, 8);
            this.btnTabXml.Size      = new System.Drawing.Size(110, 32);
            this.btnTabXml.BackColor = System.Drawing.Color.White;
            this.btnTabXml.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnTabXml.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabXml.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnTabXml.FlatAppearance.BorderSize  = 1;
            this.btnTabXml.Font      = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabXml.Text      = "📑  XML";
            this.btnTabXml.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnTabXml.Enabled   = false;
            this.btnTabXml.UseVisualStyleBackColor = false;
            this.btnTabXml.Click    += new System.EventHandler(this.btnTabXml_Click);

            this.btnTabAdjuntos.Location  = new System.Drawing.Point(250, 8);
            this.btnTabAdjuntos.Size      = new System.Drawing.Size(160, 32);
            this.btnTabAdjuntos.BackColor = System.Drawing.Color.White;
            this.btnTabAdjuntos.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnTabAdjuntos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabAdjuntos.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnTabAdjuntos.FlatAppearance.BorderSize  = 1;
            this.btnTabAdjuntos.Font      = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabAdjuntos.Text      = "📎  Adjuntos";
            this.btnTabAdjuntos.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnTabAdjuntos.Enabled   = false;
            this.btnTabAdjuntos.UseVisualStyleBackColor = false;
            this.btnTabAdjuntos.Click    += new System.EventHandler(this.btnTabAdjuntos_Click);

            this.btnAbrirExterno.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbrirExterno.Location  = new System.Drawing.Point(520, 8);
            this.btnAbrirExterno.Size      = new System.Drawing.Size(154, 32);
            this.btnAbrirExterno.BackColor = System.Drawing.Color.White;
            this.btnAbrirExterno.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnAbrirExterno.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbrirExterno.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnAbrirExterno.FlatAppearance.BorderSize  = 1;
            this.btnAbrirExterno.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnAbrirExterno.Text      = "↗  Abrir externo";
            this.btnAbrirExterno.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnAbrirExterno.Enabled   = false;
            this.btnAbrirExterno.UseVisualStyleBackColor = false;
            this.btnAbrirExterno.Click    += new System.EventHandler(this.btnAbrirExterno_Click);

            // Contenedor de la vista (PDF inline / XML / grid de adjuntos).
            this.sec3VistaContenedor.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.sec3VistaContenedor.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.sec3VistaContenedor.Controls.Add(this.dgvAdjuntos);
            this.sec3VistaContenedor.Controls.Add(this.txtVistaXml);
            this.sec3VistaContenedor.Controls.Add(this.webView);
            this.sec3VistaContenedor.Controls.Add(this.lblVistaCargando);

            this.lblVistaCargando.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblVistaCargando.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.lblVistaCargando.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblVistaCargando.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblVistaCargando.Text      = "Cargando vista previa…";

            this.webView.Dock              = System.Windows.Forms.DockStyle.Fill;
            this.webView.AllowExternalDrop = false;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.webView.ZoomFactor        = 1D;
            this.webView.Visible           = false;

            this.txtVistaXml.Dock        = System.Windows.Forms.DockStyle.Fill;
            this.txtVistaXml.Multiline   = true;
            this.txtVistaXml.ScrollBars  = System.Windows.Forms.ScrollBars.Both;
            this.txtVistaXml.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtVistaXml.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtVistaXml.ForeColor   = System.Drawing.Color.FromArgb(30, 41, 59);
            this.txtVistaXml.Font        = new System.Drawing.Font("Consolas", 9.5F);
            this.txtVistaXml.ReadOnly    = true;
            this.txtVistaXml.WordWrap    = false;
            this.txtVistaXml.Visible     = false;

            // Grid de adjuntos — oculto al inicio, se muestra al togglear a Adjuntos.
            this.dgvAdjuntos.Dock                       = System.Windows.Forms.DockStyle.Fill;
            this.dgvAdjuntos.BackgroundColor            = System.Drawing.Color.White;
            this.dgvAdjuntos.BorderStyle                = System.Windows.Forms.BorderStyle.None;
            this.dgvAdjuntos.AllowUserToAddRows         = false;
            this.dgvAdjuntos.AllowUserToDeleteRows      = false;
            this.dgvAdjuntos.AllowUserToResizeRows      = false;
            this.dgvAdjuntos.ReadOnly                   = false;
            this.dgvAdjuntos.MultiSelect                = false;
            this.dgvAdjuntos.SelectionMode              = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAdjuntos.AutoSizeColumnsMode        = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAdjuntos.RowHeadersVisible          = false;
            this.dgvAdjuntos.AutoGenerateColumns        = false;
            this.dgvAdjuntos.EnableHeadersVisualStyles  = false;
            this.dgvAdjuntos.CellBorderStyle            = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvAdjuntos.GridColor                  = System.Drawing.Color.FromArgb(241, 245, 249);
            this.dgvAdjuntos.RowTemplate.Height         = 40;
            this.dgvAdjuntos.ColumnHeadersHeight        = 36;
            this.dgvAdjuntos.ColumnHeadersBorderStyle   = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvAdjuntos.Visible                    = false;
            this.dgvAdjuntos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colAdjNombre,
                this.colAdjTamano,
                this.colAdjDescargar,
                this.colAdjId});

            this.colAdjNombre.Name        = "colAdjNombre";
            this.colAdjNombre.HeaderText  = "Archivo";
            this.colAdjNombre.ReadOnly    = true;
            this.colAdjNombre.FillWeight  = 60F;

            this.colAdjTamano.Name        = "colAdjTamano";
            this.colAdjTamano.HeaderText  = "Tamaño";
            this.colAdjTamano.ReadOnly    = true;
            this.colAdjTamano.FillWeight  = 18F;

            this.colAdjDescargar.Name        = "colAdjDescargar";
            this.colAdjDescargar.HeaderText  = "";
            this.colAdjDescargar.Text        = "📥 Descargar";
            this.colAdjDescargar.UseColumnTextForButtonValue = true;
            this.colAdjDescargar.FillWeight  = 22F;
            this.colAdjDescargar.FlatStyle   = System.Windows.Forms.FlatStyle.Flat;

            this.colAdjId.Name        = "colAdjId";
            this.colAdjId.HeaderText  = "Id";
            this.colAdjId.ReadOnly    = true;
            this.colAdjId.Visible     = false;

            // ============ Panel estado ============
            this.panelEstado.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelEstado.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.panelEstado.Size      = new System.Drawing.Size(1250, 56);
            this.panelEstado.Padding   = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.panelEstado.Controls.Add(this.lblEstado);
            this.panelEstado.Controls.Add(this.barProgreso);

            this.lblEstado.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblEstado.Height    = 28;
            this.lblEstado.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblEstado.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblEstado.Text      = "Listo para asociar.";

            this.barProgreso.Dock     = System.Windows.Forms.DockStyle.Top;
            this.barProgreso.Height   = 8;
            this.barProgreso.Minimum  = 0;
            this.barProgreso.Maximum  = 100;
            this.barProgreso.Value    = 0;
            this.barProgreso.Style    = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.barProgreso.Visible  = false;

            // ============ Panel botones ============
            this.panelBotones.BackColor = System.Drawing.Color.White;
            this.panelBotones.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotones.Size      = new System.Drawing.Size(1250, 68);
            this.panelBotones.Padding   = new System.Windows.Forms.Padding(20, 14, 20, 14);
            this.panelBotones.Controls.Add(this.btnAplicar);
            this.panelBotones.Controls.Add(this.btnCancelar);

            this.btnCancelar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Location  = new System.Drawing.Point(826, 14);
            this.btnCancelar.Size      = new System.Drawing.Size(212, 40);
            this.btnCancelar.BackColor = System.Drawing.Color.White;
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnCancelar.FlatAppearance.BorderSize  = 1;
            this.btnCancelar.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancelar.Text      = "Cancelar aplicación a Microsip";
            this.btnCancelar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click    += new System.EventHandler(this.btnCancelar_Click);

            this.btnAplicar.Anchor     = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAplicar.Location   = new System.Drawing.Point(1044, 14);
            this.btnAplicar.Size       = new System.Drawing.Size(186, 40);
            this.btnAplicar.BackColor  = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnAplicar.ForeColor  = System.Drawing.Color.White;
            this.btnAplicar.FlatStyle  = System.Windows.Forms.FlatStyle.Flat;
            this.btnAplicar.FlatAppearance.BorderSize = 0;
            this.btnAplicar.Font       = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnAplicar.Text       = "Asociar CFDI a Microsip";
            this.btnAplicar.Cursor     = System.Windows.Forms.Cursors.Hand;
            this.btnAplicar.UseVisualStyleBackColor = false;
            this.btnAplicar.Click     += new System.EventHandler(this.btnAplicar_Click);

            // ============ Compose ============
            this.panelIzquierdo.Controls.Add(this.sec2Card);
            this.panelIzquierdo.Controls.Add(this.sec1Card);

            this.panelBody.Controls.Add(this.sec3Card);
            this.panelBody.Controls.Add(this.panelIzquierdo);

            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelEstado);
            this.Controls.Add(this.panelBotones);
            this.Controls.Add(this.panelTitleBar);

            this.AcceptButton = this.btnAplicar;
            this.CancelButton = this.btnCancelar;

            this.panelTitleBar.ResumeLayout(false);
            this.panelBody.ResumeLayout(false);
            this.panelIzquierdo.ResumeLayout(false);
            this.sec1Card.ResumeLayout(false);
            this.sec1Card.PerformLayout();
            this.sec2Card.ResumeLayout(false);
            this.sec2Card.PerformLayout();
            this.sec3Card.ResumeLayout(false);
            this.sec3Card.PerformLayout();
            this.sec3Header.ResumeLayout(false);
            this.sec3Tabs.ResumeLayout(false);
            this.sec3VistaContenedor.ResumeLayout(false);
            this.sec3VistaContenedor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdjuntos)).EndInit();
            this.panelEstado.ResumeLayout(false);
            this.panelBotones.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
