namespace PortalProveedoresEscritorio.Formularios
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.MenuStrip   menuPrincipal;
        private System.Windows.Forms.ToolStripMenuItem mnuHerramientas;
        private System.Windows.Forms.ToolStripMenuItem mnuHerramientas_AbrirConfigurador;
        private System.Windows.Forms.ToolStripMenuItem mnuHerramientas_ConfigurarConexiones;
        private System.Windows.Forms.ToolStripMenuItem mnuHerramientas_SeleccionarEmpresa;
        private System.Windows.Forms.ToolStripMenuItem mnuHerramientas_ColoresPortal;
        private System.Windows.Forms.ToolStripSeparator mnuHerramientas_Sep1;
        private System.Windows.Forms.ToolStripMenuItem mnuHerramientas_CerrarSesion;
        private System.Windows.Forms.ToolStripMenuItem mnuHerramientas_Salir;

        private System.Windows.Forms.Panel       panelHeader;
        private System.Windows.Forms.PictureBox  picHeaderLogo;
        private System.Windows.Forms.Label       lblHeaderTitulo;
        private System.Windows.Forms.Label       lblHeaderEmpresa;

        private System.Windows.Forms.Panel       panelSidebar;
        private System.Windows.Forms.Button      btnTabFacturas;
        private System.Windows.Forms.Button      btnTabComplementos;
        private System.Windows.Forms.Button      btnTabDescargas;
        private System.Windows.Forms.Button      btnTabProveedores;

        private System.Windows.Forms.Panel       panelContenido;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.menuPrincipal = new System.Windows.Forms.MenuStrip();
            this.mnuHerramientas = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHerramientas_AbrirConfigurador = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHerramientas_ConfigurarConexiones = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHerramientas_SeleccionarEmpresa = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHerramientas_ColoresPortal = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHerramientas_Sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuHerramientas_CerrarSesion = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHerramientas_Salir = new System.Windows.Forms.ToolStripMenuItem();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.picHeaderLogo = new System.Windows.Forms.PictureBox();
            this.lblHeaderTitulo = new System.Windows.Forms.Label();
            this.lblHeaderEmpresa = new System.Windows.Forms.Label();
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.btnTabProveedores = new System.Windows.Forms.Button();
            this.btnTabDescargas = new System.Windows.Forms.Button();
            this.btnTabComplementos = new System.Windows.Forms.Button();
            this.btnTabFacturas = new System.Windows.Forms.Button();
            this.panelContenido = new System.Windows.Forms.Panel();
            this.menuPrincipal.SuspendLayout();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHeaderLogo)).BeginInit();
            this.panelSidebar.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuPrincipal
            // 
            this.menuPrincipal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.menuPrincipal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.menuPrincipal.ForeColor = System.Drawing.Color.White;
            this.menuPrincipal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHerramientas});
            this.menuPrincipal.Location = new System.Drawing.Point(0, 0);
            this.menuPrincipal.Name = "menuPrincipal";
            this.menuPrincipal.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuPrincipal.Size = new System.Drawing.Size(1200, 24);
            this.menuPrincipal.TabIndex = 3;
            // 
            // mnuHerramientas
            // 
            this.mnuHerramientas.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHerramientas_AbrirConfigurador,
            this.mnuHerramientas_ConfigurarConexiones,
            this.mnuHerramientas_SeleccionarEmpresa,
            this.mnuHerramientas_ColoresPortal,
            this.mnuHerramientas_Sep1,
            this.mnuHerramientas_CerrarSesion,
            this.mnuHerramientas_Salir});
            this.mnuHerramientas.ForeColor = System.Drawing.Color.White;
            this.mnuHerramientas.Name = "mnuHerramientas";
            this.mnuHerramientas.Size = new System.Drawing.Size(90, 20);
            this.mnuHerramientas.Text = "&Herramientas";
            // 
            // mnuHerramientas_AbrirConfigurador
            // 
            this.mnuHerramientas_AbrirConfigurador.Name = "mnuHerramientas_AbrirConfigurador";
            this.mnuHerramientas_AbrirConfigurador.Size = new System.Drawing.Size(222, 22);
            this.mnuHerramientas_AbrirConfigurador.Text = "Abrir Configurador…";
            this.mnuHerramientas_AbrirConfigurador.Click += new System.EventHandler(this.mnuHerramientas_AbrirConfigurador_Click);
            // 
            // mnuHerramientas_ConfigurarConexiones
            // 
            this.mnuHerramientas_ConfigurarConexiones.Name = "mnuHerramientas_ConfigurarConexiones";
            this.mnuHerramientas_ConfigurarConexiones.Size = new System.Drawing.Size(222, 22);
            this.mnuHerramientas_ConfigurarConexiones.Text = "Configurar conexiones";
            this.mnuHerramientas_ConfigurarConexiones.Click += new System.EventHandler(this.mnuHerramientas_AbrirConfigurador_Click);
            // 
            // mnuHerramientas_SeleccionarEmpresa
            // 
            this.mnuHerramientas_SeleccionarEmpresa.Name = "mnuHerramientas_SeleccionarEmpresa";
            this.mnuHerramientas_SeleccionarEmpresa.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.mnuHerramientas_SeleccionarEmpresa.Size = new System.Drawing.Size(222, 22);
            this.mnuHerramientas_SeleccionarEmpresa.Text = "Seleccionar empresa";
            this.mnuHerramientas_SeleccionarEmpresa.Click += new System.EventHandler(this.mnuArchivo_SeleccionarEmpresa_Click);
            // 
            // mnuHerramientas_ColoresPortal
            // 
            this.mnuHerramientas_ColoresPortal.Checked = true;
            this.mnuHerramientas_ColoresPortal.CheckOnClick = true;
            this.mnuHerramientas_ColoresPortal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuHerramientas_ColoresPortal.Name = "mnuHerramientas_ColoresPortal";
            this.mnuHerramientas_ColoresPortal.Size = new System.Drawing.Size(222, 22);
            this.mnuHerramientas_ColoresPortal.Text = "Usar colores del portal";
            this.mnuHerramientas_ColoresPortal.Click += new System.EventHandler(this.mnuHerramientas_ColoresPortal_Click);
            // 
            // mnuHerramientas_Sep1
            // 
            this.mnuHerramientas_Sep1.Name = "mnuHerramientas_Sep1";
            this.mnuHerramientas_Sep1.Size = new System.Drawing.Size(219, 6);
            // 
            // mnuHerramientas_CerrarSesion
            // 
            this.mnuHerramientas_CerrarSesion.Name = "mnuHerramientas_CerrarSesion";
            this.mnuHerramientas_CerrarSesion.Size = new System.Drawing.Size(222, 22);
            this.mnuHerramientas_CerrarSesion.Text = "Cerrar sesión";
            this.mnuHerramientas_CerrarSesion.Click += new System.EventHandler(this.mnuArchivo_CerrarSesion_Click);
            // 
            // mnuHerramientas_Salir
            // 
            this.mnuHerramientas_Salir.Name = "mnuHerramientas_Salir";
            this.mnuHerramientas_Salir.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.mnuHerramientas_Salir.Size = new System.Drawing.Size(222, 22);
            this.mnuHerramientas_Salir.Text = "Salir";
            this.mnuHerramientas_Salir.Click += new System.EventHandler(this.mnuArchivo_Salir_Click);
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.panelHeader.Controls.Add(this.picHeaderLogo);
            this.panelHeader.Controls.Add(this.lblHeaderTitulo);
            this.panelHeader.Controls.Add(this.lblHeaderEmpresa);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 24);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.panelHeader.Size = new System.Drawing.Size(1200, 70);
            this.panelHeader.TabIndex = 2;
            // 
            // picHeaderLogo
            // 
            this.picHeaderLogo.Location = new System.Drawing.Point(20, 13);
            this.picHeaderLogo.Name = "picHeaderLogo";
            this.picHeaderLogo.Size = new System.Drawing.Size(44, 44);
            this.picHeaderLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHeaderLogo.TabIndex = 0;
            this.picHeaderLogo.TabStop = false;
            // 
            // lblHeaderTitulo
            // 
            this.lblHeaderTitulo.AutoSize = true;
            this.lblHeaderTitulo.Font = new System.Drawing.Font("Segoe UI Semibold", 15F);
            this.lblHeaderTitulo.ForeColor = System.Drawing.Color.White;
            this.lblHeaderTitulo.Location = new System.Drawing.Point(76, 12);
            this.lblHeaderTitulo.Name = "lblHeaderTitulo";
            this.lblHeaderTitulo.Size = new System.Drawing.Size(185, 28);
            this.lblHeaderTitulo.TabIndex = 1;
            this.lblHeaderTitulo.Text = "Portal Proveedores";
            // 
            // lblHeaderEmpresa
            // 
            this.lblHeaderEmpresa.AutoSize = true;
            this.lblHeaderEmpresa.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblHeaderEmpresa.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.lblHeaderEmpresa.Location = new System.Drawing.Point(76, 40);
            this.lblHeaderEmpresa.Name = "lblHeaderEmpresa";
            this.lblHeaderEmpresa.Size = new System.Drawing.Size(115, 17);
            this.lblHeaderEmpresa.TabIndex = 2;
            this.lblHeaderEmpresa.Text = "Usuario · Empresa";
            // 
            // panelSidebar
            // 
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.panelSidebar.Controls.Add(this.btnTabProveedores);
            this.panelSidebar.Controls.Add(this.btnTabDescargas);
            this.panelSidebar.Controls.Add(this.btnTabComplementos);
            this.panelSidebar.Controls.Add(this.btnTabFacturas);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Location = new System.Drawing.Point(0, 94);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Padding = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.panelSidebar.Size = new System.Drawing.Size(240, 626);
            this.panelSidebar.TabIndex = 1;
            // 
            // btnTabProveedores
            // 
            this.btnTabProveedores.BackColor = System.Drawing.Color.Transparent;
            this.btnTabProveedores.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabProveedores.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTabProveedores.FlatAppearance.BorderSize = 0;
            this.btnTabProveedores.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabProveedores.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.btnTabProveedores.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(215)))), ((int)(((byte)(230)))));
            this.btnTabProveedores.Location = new System.Drawing.Point(0, 188);
            this.btnTabProveedores.Name = "btnTabProveedores";
            this.btnTabProveedores.Padding = new System.Windows.Forms.Padding(24, 0, 0, 0);
            this.btnTabProveedores.Size = new System.Drawing.Size(240, 56);
            this.btnTabProveedores.TabIndex = 0;
            this.btnTabProveedores.Text = "Proveedores";
            this.btnTabProveedores.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabProveedores.UseVisualStyleBackColor = false;
            this.btnTabProveedores.Click += new System.EventHandler(this.btnTabProveedores_Click);
            // 
            // btnTabDescargas
            // 
            this.btnTabDescargas.BackColor = System.Drawing.Color.Transparent;
            this.btnTabDescargas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabDescargas.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTabDescargas.FlatAppearance.BorderSize = 0;
            this.btnTabDescargas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabDescargas.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.btnTabDescargas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(215)))), ((int)(((byte)(230)))));
            this.btnTabDescargas.Location = new System.Drawing.Point(0, 132);
            this.btnTabDescargas.Name = "btnTabDescargas";
            this.btnTabDescargas.Padding = new System.Windows.Forms.Padding(24, 0, 0, 0);
            this.btnTabDescargas.Size = new System.Drawing.Size(240, 56);
            this.btnTabDescargas.TabIndex = 1;
            this.btnTabDescargas.Text = "Descargar";
            this.btnTabDescargas.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabDescargas.UseVisualStyleBackColor = false;
            this.btnTabDescargas.Click += new System.EventHandler(this.btnTabDescargas_Click);
            // 
            // btnTabComplementos
            // 
            this.btnTabComplementos.BackColor = System.Drawing.Color.Transparent;
            this.btnTabComplementos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabComplementos.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTabComplementos.FlatAppearance.BorderSize = 0;
            this.btnTabComplementos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabComplementos.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.btnTabComplementos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(215)))), ((int)(((byte)(230)))));
            this.btnTabComplementos.Location = new System.Drawing.Point(0, 76);
            this.btnTabComplementos.Name = "btnTabComplementos";
            this.btnTabComplementos.Padding = new System.Windows.Forms.Padding(24, 0, 0, 0);
            this.btnTabComplementos.Size = new System.Drawing.Size(240, 56);
            this.btnTabComplementos.TabIndex = 2;
            this.btnTabComplementos.Text = "Cuentas por pagar";
            this.btnTabComplementos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabComplementos.UseVisualStyleBackColor = false;
            this.btnTabComplementos.Click += new System.EventHandler(this.btnTabComplementos_Click);
            // 
            // btnTabFacturas
            // 
            this.btnTabFacturas.BackColor = System.Drawing.Color.Transparent;
            this.btnTabFacturas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabFacturas.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTabFacturas.FlatAppearance.BorderSize = 0;
            this.btnTabFacturas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabFacturas.Font = new System.Drawing.Font("Segoe UI Semibold", 11F);
            this.btnTabFacturas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(215)))), ((int)(((byte)(230)))));
            this.btnTabFacturas.Location = new System.Drawing.Point(0, 20);
            this.btnTabFacturas.Name = "btnTabFacturas";
            this.btnTabFacturas.Padding = new System.Windows.Forms.Padding(24, 0, 0, 0);
            this.btnTabFacturas.Size = new System.Drawing.Size(240, 56);
            this.btnTabFacturas.TabIndex = 3;
            this.btnTabFacturas.Text = "Facturas";
            this.btnTabFacturas.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabFacturas.UseVisualStyleBackColor = false;
            this.btnTabFacturas.Click += new System.EventHandler(this.btnTabFacturas_Click);
            // 
            // panelContenido
            // 
            this.panelContenido.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(252)))));
            this.panelContenido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContenido.Location = new System.Drawing.Point(240, 94);
            this.panelContenido.Name = "panelContenido";
            this.panelContenido.Padding = new System.Windows.Forms.Padding(12);
            this.panelContenido.Size = new System.Drawing.Size(960, 626);
            this.panelContenido.TabIndex = 0;
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(249)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1200, 720);
            this.Controls.Add(this.panelContenido);
            this.Controls.Add(this.panelSidebar);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.menuPrincipal);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.MainMenuStrip = this.menuPrincipal;
            this.MinimumSize = new System.Drawing.Size(1024, 600);
            this.Name = "FormPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Portal Proveedores";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuPrincipal.ResumeLayout(false);
            this.menuPrincipal.PerformLayout();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHeaderLogo)).EndInit();
            this.panelSidebar.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
