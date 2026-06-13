namespace PortalProveedoresEscritorio.Formularios
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel       panelTitleBar;
        private System.Windows.Forms.Label       btnMinimizar;
        private System.Windows.Forms.Label       btnCerrar;
        private System.Windows.Forms.PictureBox  pictureBoxLogo;
        private System.Windows.Forms.Label       lblNombrePortal;
        private System.Windows.Forms.Label       lblSubtitulo;
        private System.Windows.Forms.Panel       panelInputUsuario;
        private System.Windows.Forms.Label       iconUsuario;
        private System.Windows.Forms.TextBox     txtUsuario;
        private System.Windows.Forms.Panel       panelInputPassword;
        private System.Windows.Forms.Label       iconPassword;
        private System.Windows.Forms.TextBox     txtPassword;
        private System.Windows.Forms.CheckBox    chkRecordar;
        private System.Windows.Forms.LinkLabel   linkConfigurador;
        private System.Windows.Forms.Button      btnAceptar;
        private System.Windows.Forms.Label       lblStatus;
        private System.Windows.Forms.ErrorProvider errorProviderUsuario;
        private System.Windows.Forms.ErrorProvider errorProviderPassword;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelTitleBar = new System.Windows.Forms.Panel();
            this.btnMinimizar = new System.Windows.Forms.Label();
            this.btnCerrar = new System.Windows.Forms.Label();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.lblNombrePortal = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            this.panelInputUsuario = new System.Windows.Forms.Panel();
            this.iconUsuario = new System.Windows.Forms.Label();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.panelInputPassword = new System.Windows.Forms.Panel();
            this.iconPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.chkRecordar = new System.Windows.Forms.CheckBox();
            this.linkConfigurador = new System.Windows.Forms.LinkLabel();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.errorProviderUsuario = new System.Windows.Forms.ErrorProvider(this.components);
            this.errorProviderPassword = new System.Windows.Forms.ErrorProvider(this.components);
            this.panelTitleBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.panelInputUsuario.SuspendLayout();
            this.panelInputPassword.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderUsuario)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderPassword)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTitleBar
            // 
            this.panelTitleBar.BackColor = System.Drawing.Color.Transparent;
            this.panelTitleBar.Controls.Add(this.btnMinimizar);
            this.panelTitleBar.Controls.Add(this.btnCerrar);
            this.panelTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitleBar.Location = new System.Drawing.Point(0, 0);
            this.panelTitleBar.Name = "panelTitleBar";
            this.panelTitleBar.Size = new System.Drawing.Size(439, 36);
            this.panelTitleBar.TabIndex = 10;
            // 
            // btnMinimizar
            // 
            this.btnMinimizar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimizar.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnMinimizar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(180)))), ((int)(((byte)(200)))));
            this.btnMinimizar.Location = new System.Drawing.Point(367, 0);
            this.btnMinimizar.Name = "btnMinimizar";
            this.btnMinimizar.Size = new System.Drawing.Size(36, 36);
            this.btnMinimizar.TabIndex = 0;
            this.btnMinimizar.Text = "─";
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(180)))), ((int)(((byte)(200)))));
            this.btnCerrar.Location = new System.Drawing.Point(403, 0);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(36, 36);
            this.btnCerrar.TabIndex = 1;
            this.btnCerrar.Text = "✕";
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Location = new System.Drawing.Point(176, 60);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(88, 88);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxLogo.TabIndex = 9;
            this.pictureBoxLogo.TabStop = false;
            // 
            // lblNombrePortal
            // 
            this.lblNombrePortal.Font = new System.Drawing.Font("Segoe UI Semibold", 16F);
            this.lblNombrePortal.ForeColor = System.Drawing.Color.White;
            this.lblNombrePortal.Location = new System.Drawing.Point(20, 162);
            this.lblNombrePortal.Name = "lblNombrePortal";
            this.lblNombrePortal.Size = new System.Drawing.Size(400, 28);
            this.lblNombrePortal.TabIndex = 8;
            this.lblNombrePortal.Text = "Portal de proveedores";
            this.lblNombrePortal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSubtitulo
            // 
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(180)))), ((int)(((byte)(200)))));
            this.lblSubtitulo.Location = new System.Drawing.Point(20, 194);
            this.lblSubtitulo.Name = "lblSubtitulo";
            this.lblSubtitulo.Size = new System.Drawing.Size(400, 18);
            this.lblSubtitulo.TabIndex = 7;
            this.lblSubtitulo.Text = "Inicia sesión para continuar";
            this.lblSubtitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelInputUsuario
            // 
            this.panelInputUsuario.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(75)))));
            this.panelInputUsuario.Controls.Add(this.iconUsuario);
            this.panelInputUsuario.Controls.Add(this.txtUsuario);
            this.panelInputUsuario.Location = new System.Drawing.Point(32, 240);
            this.panelInputUsuario.Name = "panelInputUsuario";
            this.panelInputUsuario.Size = new System.Drawing.Size(376, 48);
            this.panelInputUsuario.TabIndex = 6;
            // 
            // iconUsuario
            // 
            this.iconUsuario.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.iconUsuario.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(220)))));
            this.iconUsuario.Location = new System.Drawing.Point(12, 12);
            this.iconUsuario.Name = "iconUsuario";
            this.iconUsuario.Size = new System.Drawing.Size(28, 24);
            this.iconUsuario.TabIndex = 0;
            this.iconUsuario.Text = "👤";
            this.iconUsuario.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtUsuario
            // 
            this.txtUsuario.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(75)))));
            this.txtUsuario.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtUsuario.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtUsuario.ForeColor = System.Drawing.Color.White;
            this.txtUsuario.Location = new System.Drawing.Point(48, 13);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(316, 19);
            this.txtUsuario.TabIndex = 0;
            this.txtUsuario.TextChanged += new System.EventHandler(this.txtUsuario_TextChanged);
            // 
            // panelInputPassword
            // 
            this.panelInputPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(75)))));
            this.panelInputPassword.Controls.Add(this.iconPassword);
            this.panelInputPassword.Controls.Add(this.txtPassword);
            this.panelInputPassword.Location = new System.Drawing.Point(32, 304);
            this.panelInputPassword.Name = "panelInputPassword";
            this.panelInputPassword.Size = new System.Drawing.Size(376, 48);
            this.panelInputPassword.TabIndex = 5;
            // 
            // iconPassword
            // 
            this.iconPassword.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.iconPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(220)))));
            this.iconPassword.Location = new System.Drawing.Point(12, 12);
            this.iconPassword.Name = "iconPassword";
            this.iconPassword.Size = new System.Drawing.Size(28, 24);
            this.iconPassword.TabIndex = 0;
            this.iconPassword.Text = "🔒";
            this.iconPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(75)))));
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtPassword.ForeColor = System.Drawing.Color.White;
            this.txtPassword.Location = new System.Drawing.Point(48, 13);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(316, 19);
            this.txtPassword.TabIndex = 1;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            // 
            // chkRecordar
            // 
            this.chkRecordar.AutoSize = true;
            this.chkRecordar.BackColor = System.Drawing.Color.Transparent;
            this.chkRecordar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRecordar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(220)))));
            this.chkRecordar.Location = new System.Drawing.Point(32, 365);
            this.chkRecordar.Name = "chkRecordar";
            this.chkRecordar.Size = new System.Drawing.Size(134, 19);
            this.chkRecordar.TabIndex = 2;
            this.chkRecordar.Text = "Recordar contraseña";
            this.chkRecordar.UseVisualStyleBackColor = false;
            // 
            // linkConfigurador
            // 
            this.linkConfigurador.AutoSize = true;
            this.linkConfigurador.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.linkConfigurador.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.linkConfigurador.Location = new System.Drawing.Point(308, 366);
            this.linkConfigurador.Name = "linkConfigurador";
            this.linkConfigurador.Size = new System.Drawing.Size(83, 15);
            this.linkConfigurador.TabIndex = 4;
            this.linkConfigurador.TabStop = true;
            this.linkConfigurador.Text = "Configuración";
            this.linkConfigurador.Visible = false;
            this.linkConfigurador.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkConfigurador_LinkClicked);
            // 
            // btnAceptar
            // 
            this.btnAceptar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnAceptar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAceptar.FlatAppearance.BorderSize = 0;
            this.btnAceptar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAceptar.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.btnAceptar.ForeColor = System.Drawing.Color.White;
            this.btnAceptar.Location = new System.Drawing.Point(32, 408);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(376, 48);
            this.btnAceptar.TabIndex = 3;
            this.btnAceptar.Text = "Iniciar sesión";
            this.btnAceptar.UseVisualStyleBackColor = false;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.lblStatus.Location = new System.Drawing.Point(32, 466);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(376, 20);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // errorProviderUsuario
            // 
            this.errorProviderUsuario.ContainerControl = this;
            // 
            // errorProviderPassword
            // 
            this.errorProviderPassword.ContainerControl = this;
            // 
            // FormLogin
            // 
            this.AcceptButton = this.btnAceptar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(439, 546);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.linkConfigurador);
            this.Controls.Add(this.chkRecordar);
            this.Controls.Add(this.panelInputPassword);
            this.Controls.Add(this.panelInputUsuario);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.lblNombrePortal);
            this.Controls.Add(this.pictureBoxLogo);
            this.Controls.Add(this.panelTitleBar);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Portal de proveedores";
            this.panelTitleBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.panelInputUsuario.ResumeLayout(false);
            this.panelInputUsuario.PerformLayout();
            this.panelInputPassword.ResumeLayout(false);
            this.panelInputPassword.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderUsuario)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderPassword)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
