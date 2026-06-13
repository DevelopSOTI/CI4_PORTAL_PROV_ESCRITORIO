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
        private System.Windows.Forms.TextBox     txtFechaFac1;
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
            this.components            = new System.ComponentModel.Container();
            this.panelTitleBar         = new System.Windows.Forms.Panel();
            this.lblTitulo             = new System.Windows.Forms.Label();
            this.btnMinimizar          = new System.Windows.Forms.Label();
            this.btnMaximizar          = new System.Windows.Forms.Label();
            this.btnCerrar             = new System.Windows.Forms.Label();

            this.panelBody             = new System.Windows.Forms.Panel();
            this.panelIzquierdo        = new System.Windows.Forms.Panel();

            this.sec1Card              = new System.Windows.Forms.Panel();
            this.sec1Titulo            = new System.Windows.Forms.Label();
            this.lbl_NombreProv        = new System.Windows.Forms.Label();
            this.txtProveedor          = new System.Windows.Forms.TextBox();
            this.lbl_FolioFac          = new System.Windows.Forms.Label();
            this.txtFolioFac           = new System.Windows.Forms.TextBox();
            this.lbl_FechaFac          = new System.Windows.Forms.Label();
            this.txtFechaFac1          = new System.Windows.Forms.TextBox();
            this.lbl_Atraso            = new System.Windows.Forms.Label();
            this.txtAtraso             = new System.Windows.Forms.TextBox();
            this.lbl_FechaSubio        = new System.Windows.Forms.Label();
            this.txtFechaSubio         = new System.Windows.Forms.TextBox();
            this.lbl_Sugerida          = new System.Windows.Forms.Label();
            this.txtSugerida           = new System.Windows.Forms.TextBox();
            this.lbl_Total             = new System.Windows.Forms.Label();
            this.txtTotal              = new System.Windows.Forms.TextBox();
            this.lbl_UUID              = new System.Windows.Forms.Label();
            this.txtUUID               = new System.Windows.Forms.TextBox();

            this.sec2Card              = new System.Windows.Forms.Panel();
            this.sec2Titulo            = new System.Windows.Forms.Label();
            this.lbl_Condiciones       = new System.Windows.Forms.Label();
            this.cbCondiciones         = new System.Windows.Forms.ComboBox();
            this.lbl_Articulo          = new System.Windows.Forms.Label();
            this.cbArticulo            = new System.Windows.Forms.ComboBox();
            this.btnBuscarArticulo     = new System.Windows.Forms.Button();
            this.lbl_DescCompra        = new System.Windows.Forms.Label();
            this.rtDesc                = new System.Windows.Forms.TextBox();

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
            this.Text                = "Aplicar compra en Microsip";
            this.Font                = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ShowInTaskbar       = false;

            // ============ Title bar ============
            this.panelTitleBar.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.panelTitleBar.Dock      = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Size      = new System.Drawing.Size(1250, 44);
            this.panelTitleBar.Controls.Add(this.lblTitulo);
            this.panelTitleBar.Controls.Add(this.btnMinimizar);
            this.panelTitleBar.Controls.Add(this.btnMaximizar);
            this.panelTitleBar.Controls.Add(this.btnCerrar);

            this.lblTitulo.Location  = new System.Drawing.Point(16, 0);
            this.lblTitulo.Size      = new System.Drawing.Size(1100, 44);
            this.lblTitulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitulo.Text      = "Aplicación de la factura al módulo de compras y cuentas por cobrar";

            this.btnMinimizar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimizar.Location  = new System.Drawing.Point(1142, 0);
            this.btnMinimizar.Size      = new System.Drawing.Size(36, 44);
            this.btnMinimizar.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnMinimizar.ForeColor = System.Drawing.Color.White;
            this.btnMinimizar.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimizar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnMinimizar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnMinimizar.Text      = "─";

            this.btnMaximizar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMaximizar.Location  = new System.Drawing.Point(1178, 0);
            this.btnMaximizar.Size      = new System.Drawing.Size(36, 44);
            this.btnMaximizar.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnMaximizar.ForeColor = System.Drawing.Color.White;
            this.btnMaximizar.BackColor = System.Drawing.Color.Transparent;
            this.btnMaximizar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnMaximizar.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnMaximizar.Text      = "□";

            this.btnCerrar.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Location  = new System.Drawing.Point(1214, 0);
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
            this.panelBody.Padding   = new System.Windows.Forms.Padding(16, 16, 16, 16);

            // ============ Panel izquierdo (Datos + Descripción) ============
            this.panelIzquierdo.Dock      = System.Windows.Forms.DockStyle.Left;
            this.panelIzquierdo.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.panelIzquierdo.Size      = new System.Drawing.Size(520, 580);
            this.panelIzquierdo.Padding   = new System.Windows.Forms.Padding(0, 0, 16, 0);

            // ============ Sec 1: Datos del proveedor ============
            this.sec1Card.BackColor = System.Drawing.Color.White;
            this.sec1Card.Dock      = System.Windows.Forms.DockStyle.Top;
            this.sec1Card.Size      = new System.Drawing.Size(504, 320);
            this.sec1Card.Padding   = new System.Windows.Forms.Padding(20, 12, 20, 12);
            this.sec1Card.Controls.Add(this.sec1Titulo);
            this.sec1Card.Controls.Add(this.lbl_NombreProv); this.sec1Card.Controls.Add(this.txtProveedor);
            this.sec1Card.Controls.Add(this.lbl_FolioFac);   this.sec1Card.Controls.Add(this.txtFolioFac);
            this.sec1Card.Controls.Add(this.lbl_FechaFac);   this.sec1Card.Controls.Add(this.txtFechaFac1);
            this.sec1Card.Controls.Add(this.lbl_Atraso);     this.sec1Card.Controls.Add(this.txtAtraso);
            this.sec1Card.Controls.Add(this.lbl_FechaSubio); this.sec1Card.Controls.Add(this.txtFechaSubio);
            this.sec1Card.Controls.Add(this.lbl_Sugerida);   this.sec1Card.Controls.Add(this.txtSugerida);
            this.sec1Card.Controls.Add(this.lbl_Total);      this.sec1Card.Controls.Add(this.txtTotal);
            this.sec1Card.Controls.Add(this.lbl_UUID);       this.sec1Card.Controls.Add(this.txtUUID);

            this.sec1Titulo.Location  = new System.Drawing.Point(20, 8);
            this.sec1Titulo.Size      = new System.Drawing.Size(400, 22);
            this.sec1Titulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.sec1Titulo.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.sec1Titulo.Text      = "Datos del proveedor";

            // 8 pares label/textbox en columna única (no en 2 columnas — el panel
            // izquierdo es angosto). Posiciones literales para Designer.

            // --- Nombre del proveedor ---
            this.lbl_NombreProv.Location  = new System.Drawing.Point(20, 44);
            this.lbl_NombreProv.Size      = new System.Drawing.Size(160, 22);
            this.lbl_NombreProv.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_NombreProv.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_NombreProv.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_NombreProv.Text      = "Nombre del proveedor:";
            this.txtProveedor.Location    = new System.Drawing.Point(180, 40);
            this.txtProveedor.Size        = new System.Drawing.Size(300, 25);
            this.txtProveedor.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtProveedor.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtProveedor.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtProveedor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtProveedor.ReadOnly    = true;
            this.txtProveedor.Text        = "";

            // --- Folio factura ---
            this.lbl_FolioFac.Location  = new System.Drawing.Point(20, 76);
            this.lbl_FolioFac.Size      = new System.Drawing.Size(160, 22);
            this.lbl_FolioFac.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FolioFac.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_FolioFac.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_FolioFac.Text      = "Folio factura";
            this.txtFolioFac.Location    = new System.Drawing.Point(180, 72);
            this.txtFolioFac.Size        = new System.Drawing.Size(300, 25);
            this.txtFolioFac.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFolioFac.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtFolioFac.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtFolioFac.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFolioFac.ReadOnly    = true;
            this.txtFolioFac.Text        = "";

            // --- Fecha de la factura ---
            this.lbl_FechaFac.Location  = new System.Drawing.Point(20, 108);
            this.lbl_FechaFac.Size      = new System.Drawing.Size(160, 22);
            this.lbl_FechaFac.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FechaFac.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_FechaFac.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_FechaFac.Text      = "Fecha de la factura";
            this.txtFechaFac1.Location    = new System.Drawing.Point(180, 104);
            this.txtFechaFac1.Size        = new System.Drawing.Size(300, 25);
            this.txtFechaFac1.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFechaFac1.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtFechaFac1.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtFechaFac1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFechaFac1.ReadOnly    = true;
            this.txtFechaFac1.Text        = "";

            // --- Atraso (Dias) ---
            this.lbl_Atraso.Location  = new System.Drawing.Point(20, 140);
            this.lbl_Atraso.Size      = new System.Drawing.Size(160, 22);
            this.lbl_Atraso.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Atraso.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_Atraso.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Atraso.Text      = "Atraso (Dias)";
            this.txtAtraso.Location    = new System.Drawing.Point(180, 136);
            this.txtAtraso.Size        = new System.Drawing.Size(300, 25);
            this.txtAtraso.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtAtraso.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtAtraso.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtAtraso.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAtraso.ReadOnly    = true;
            this.txtAtraso.Text        = "";

            // --- Fecha subio al portal ---
            this.lbl_FechaSubio.Location  = new System.Drawing.Point(20, 172);
            this.lbl_FechaSubio.Size      = new System.Drawing.Size(160, 22);
            this.lbl_FechaSubio.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_FechaSubio.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_FechaSubio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_FechaSubio.Text      = "Fecha subio al portal";
            this.txtFechaSubio.Location    = new System.Drawing.Point(180, 168);
            this.txtFechaSubio.Size        = new System.Drawing.Size(300, 25);
            this.txtFechaSubio.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtFechaSubio.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtFechaSubio.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtFechaSubio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFechaSubio.ReadOnly    = true;
            this.txtFechaSubio.Text        = "";

            // --- Fecha sugerida pago ---
            this.lbl_Sugerida.Location  = new System.Drawing.Point(20, 204);
            this.lbl_Sugerida.Size      = new System.Drawing.Size(160, 22);
            this.lbl_Sugerida.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Sugerida.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_Sugerida.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Sugerida.Text      = "Fecha sugerida pago";
            this.txtSugerida.Location    = new System.Drawing.Point(180, 200);
            this.txtSugerida.Size        = new System.Drawing.Size(300, 25);
            this.txtSugerida.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSugerida.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtSugerida.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtSugerida.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSugerida.ReadOnly    = true;
            this.txtSugerida.Text        = "";

            // --- Total (semibold, destacado) ---
            this.lbl_Total.Location  = new System.Drawing.Point(20, 240);
            this.lbl_Total.Size      = new System.Drawing.Size(160, 22);
            this.lbl_Total.Font      = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lbl_Total.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.lbl_Total.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Total.Text      = "Total";
            this.txtTotal.Location    = new System.Drawing.Point(180, 236);
            this.txtTotal.Size        = new System.Drawing.Size(300, 27);
            this.txtTotal.Font        = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.txtTotal.ForeColor   = System.Drawing.Color.FromArgb(37, 99, 235);
            this.txtTotal.BackColor   = System.Drawing.Color.FromArgb(239, 246, 255);
            this.txtTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTotal.ReadOnly    = true;
            this.txtTotal.Text        = "";

            // --- UUID (Consolas, monoespacio) ---
            this.lbl_UUID.Location  = new System.Drawing.Point(20, 276);
            this.lbl_UUID.Size      = new System.Drawing.Size(160, 22);
            this.lbl_UUID.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_UUID.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_UUID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_UUID.Text      = "UUID";
            this.txtUUID.Location    = new System.Drawing.Point(180, 272);
            this.txtUUID.Size        = new System.Drawing.Size(300, 25);
            this.txtUUID.Font        = new System.Drawing.Font("Consolas", 8.5F);
            this.txtUUID.ForeColor   = System.Drawing.Color.FromArgb(15, 23, 42);
            this.txtUUID.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            this.txtUUID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUUID.ReadOnly    = true;
            this.txtUUID.Text        = "";

            // ============ Sec 2: Descripción de la compra ============
            this.sec2Card.BackColor = System.Drawing.Color.White;
            this.sec2Card.Dock      = System.Windows.Forms.DockStyle.Top;
            this.sec2Card.Size      = new System.Drawing.Size(504, 230);
            this.sec2Card.Padding   = new System.Windows.Forms.Padding(20, 12, 20, 12);
            this.sec2Card.Margin    = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.sec2Card.Controls.Add(this.sec2Titulo);
            this.sec2Card.Controls.Add(this.lbl_Condiciones);  this.sec2Card.Controls.Add(this.cbCondiciones);
            this.sec2Card.Controls.Add(this.lbl_Articulo);     this.sec2Card.Controls.Add(this.cbArticulo);
            this.sec2Card.Controls.Add(this.btnBuscarArticulo);
            this.sec2Card.Controls.Add(this.lbl_DescCompra);   this.sec2Card.Controls.Add(this.rtDesc);

            this.sec2Titulo.Location  = new System.Drawing.Point(20, 8);
            this.sec2Titulo.Size      = new System.Drawing.Size(400, 22);
            this.sec2Titulo.Font      = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.sec2Titulo.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.sec2Titulo.Text      = "Descripción de la compra";

            this.lbl_Condiciones.Location  = new System.Drawing.Point(20, 42);
            this.lbl_Condiciones.Size      = new System.Drawing.Size(160, 22);
            this.lbl_Condiciones.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Condiciones.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_Condiciones.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Condiciones.Text      = "Condiciones de pago";

            this.cbCondiciones.Location      = new System.Drawing.Point(180, 40);
            this.cbCondiciones.Size          = new System.Drawing.Size(300, 25);
            this.cbCondiciones.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCondiciones.Enabled       = false;

            this.lbl_Articulo.Location  = new System.Drawing.Point(20, 74);
            this.lbl_Articulo.Size      = new System.Drawing.Size(160, 22);
            this.lbl_Articulo.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_Articulo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_Articulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_Articulo.Text      = "Artículo general";

            this.cbArticulo.Location      = new System.Drawing.Point(180, 72);
            this.cbArticulo.Size          = new System.Drawing.Size(160, 25);
            this.cbArticulo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbArticulo.Enabled       = false;

            this.btnBuscarArticulo.Location  = new System.Drawing.Point(345, 72);
            this.btnBuscarArticulo.Size      = new System.Drawing.Size(135, 26);
            this.btnBuscarArticulo.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnBuscarArticulo.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnBuscarArticulo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscarArticulo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnBuscarArticulo.FlatAppearance.BorderSize  = 1;
            this.btnBuscarArticulo.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBuscarArticulo.Text      = "🔍  Buscar Articulo";
            this.btnBuscarArticulo.Enabled   = false;
            this.btnBuscarArticulo.UseVisualStyleBackColor = false;

            this.lbl_DescCompra.Location  = new System.Drawing.Point(20, 110);
            this.lbl_DescCompra.Size      = new System.Drawing.Size(160, 22);
            this.lbl_DescCompra.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_DescCompra.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lbl_DescCompra.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_DescCompra.Text      = "Descripción de la compra";

            this.rtDesc.Location    = new System.Drawing.Point(180, 110);
            this.rtDesc.Size        = new System.Drawing.Size(300, 96);
            this.rtDesc.Multiline   = true;
            this.rtDesc.ScrollBars  = System.Windows.Forms.ScrollBars.Vertical;
            this.rtDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtDesc.Font        = new System.Drawing.Font("Segoe UI", 9F);

            // ============ Sec 3: Factura del proveedor (derecha, GRANDE) ============
            // Layout: header (titulo+subtitulo) → tabs (PDF/XML/Abrir externo)
            // → contenedor con WebView2 (PDF) + TextBox (XML) que se toggle.
            this.sec3Card.BackColor = System.Drawing.Color.White;
            this.sec3Card.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.sec3Card.Padding   = new System.Windows.Forms.Padding(0);
            this.sec3Card.Controls.Add(this.sec3VistaContenedor);
            this.sec3Card.Controls.Add(this.sec3Tabs);
            this.sec3Card.Controls.Add(this.sec3Header);

            // -- Header del card derecho --
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
            this.sec3Titulo.Text      = "Factura del proveedor";

            this.sec3Subtitulo.Dock      = System.Windows.Forms.DockStyle.Top;
            this.sec3Subtitulo.Height    = 18;
            this.sec3Subtitulo.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.sec3Subtitulo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.sec3Subtitulo.Text      = "Cargando CFDI del portal…";

            // -- Tabs: PDF / XML / Adjuntos + botón abrir externo --
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

            // -- Contenedor de la vista (PDF inline, XML o lista de adjuntos) --
            this.sec3VistaContenedor.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.sec3VistaContenedor.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.sec3VistaContenedor.Controls.Add(this.dgvAdjuntos);
            this.sec3VistaContenedor.Controls.Add(this.txtVistaXml);
            this.sec3VistaContenedor.Controls.Add(this.webView);
            this.sec3VistaContenedor.Controls.Add(this.lblVistaCargando);

            // Label de fondo "Cargando…" — visible solo mientras la vista está vacía.
            this.lblVistaCargando.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblVistaCargando.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.lblVistaCargando.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblVistaCargando.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblVistaCargando.Text      = "Cargando vista previa…";

            // WebView2 — Dock=Fill, oculto al inicio hasta que el PDF cargue.
            this.webView.Dock              = System.Windows.Forms.DockStyle.Fill;
            this.webView.AllowExternalDrop = false;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.webView.ZoomFactor        = 1D;
            this.webView.Visible           = false;

            // TextBox XML — oculto al inicio, se muestra al togglear a XML.
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
            this.lblEstado.Text      = "Listo para aplicar.";

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
            this.btnAplicar.Text       = "Aplicar Factura a Microsip";
            this.btnAplicar.Cursor     = System.Windows.Forms.Cursors.Hand;
            this.btnAplicar.UseVisualStyleBackColor = false;
            this.btnAplicar.Click     += new System.EventHandler(this.btnAplicar_Click);

            // ============ Compose ============
            // Izquierda apila Datos arriba y Descripción abajo (Dock=Top).
            this.panelIzquierdo.Controls.Add(this.sec2Card);
            this.panelIzquierdo.Controls.Add(this.sec1Card);

            // Body = izquierda + derecha (sec3 con Dock=Fill ocupa lo demás).
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
