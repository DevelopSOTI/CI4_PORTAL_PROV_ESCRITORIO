using System;
using System.Drawing;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Modal "Nuevo folio" — réplica funcional de <c>F_NUEVO_FOLIO</c> del
    /// SOAP. Se abre cuando al aplicar una factura el FOLIO_PROV ya existe
    /// en Microsip (DOCTOS_CM duplicado por proveedor) y el operador
    /// necesita elegir entre tres opciones (SOAP F_APLICAR_FACTURA.cs:1132-1155):
    /// <list type="bullet">
    ///   <item><b>Insertar con nuevo folio</b> (RESULTADO=1): el operador
    ///   teclea una serie + folio distinto al del CFDI; el flujo continúa
    ///   con ese folio.</item>
    ///   <item><b>Actualizar nuevo folio en portal</b> (RESULTADO=2):
    ///   marca la factura del portal como aplicada (ESTATUS='R') sin tocar
    ///   Microsip — el SOAP llama <c>ws.ACTUALIZA_NUEVO_FOLIO</c>.</item>
    ///   <item><b>No hacer nada</b> (RESULTADO=3): cancelar todo.</item>
    /// </list>
    ///
    /// Validaciones literales del SOAP (F_NUEVO_FOLIO.cs:37, 47):
    /// <list type="bullet">
    ///   <item>El folio nuevo debe ser distinto al del CFDI.</item>
    ///   <item>Serie y folio son obligatorios.</item>
    /// </list>
    ///
    /// El folio resultante se pad-rellena con ceros hasta 9 caracteres
    /// totales (F_NUEVO_FOLIO.cs:32-34).
    /// </summary>
    public sealed class FormNuevoFolio : Form
    {
        /// <summary>
        /// Outcome del modal — mismos enteros que el SOAP F_NUEVO_FOLIO:
        /// 1 = Insertar con nuevo folio (el operador tecleó serie+folio).
        /// 2 = Actualizar nuevo folio en portal (cambiar ESTATUS='R').
        /// 3 = No hacer nada (cancelar). Es el default cuando se cierra
        ///     sin elegir explícitamente.
        /// </summary>
        public enum OpcionFolio
        {
            InsertarConNuevoFolio   = 1,
            ActualizarFolioEnPortal = 2,
            Cancelar                = 3,
        }

        private readonly string _folioCfdi;

        /// <summary>Outcome elegido por el operador. Default = Cancelar.</summary>
        public OpcionFolio Resultado { get; private set; }

        /// <summary>Folio nuevo (solo válido cuando <see cref="Resultado"/>==InsertarConNuevoFolio).</summary>
        public string FolioNuevo { get; private set; }

        private Panel   panelTitleBar;
        private Label   lblTitulo;
        private Label   btnCerrar;
        private Panel   panelBody;
        private Label   lblHint;
        private Label   lblFolioCfdi;
        private TextBox txtFolioCfdi;
        private Label   lblSerie;
        private TextBox txtSerie;
        private Label   lblFolio;
        private TextBox txtFolio;
        private Panel   panelBotones;
        private Button  btnInsertar;
        private Button  btnActualizar;
        private Button  btnNada;

        public FormNuevoFolio(string folioCfdi)
        {
            _folioCfdi = folioCfdi ?? "";
            Resultado  = OpcionFolio.Cancelar; // default réplica F_NUEVO_FOLIO.cs:22
            ConstruirUI();
        }

        private void ConstruirUI()
        {
            this.Text            = "Asignar nuevo folio";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.ClientSize      = new Size(600, 380);
            this.BackColor       = Color.FromArgb(241, 245, 249);
            this.Font            = new Font("Segoe UI", 9.5F);
            this.ShowInTaskbar   = false;

            // --- Title bar ---
            panelTitleBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = Tema.Primary,
            };
            panelTitleBar.Width = this.ClientSize.Width;

            lblTitulo = new Label
            {
                Location  = new Point(16, 0),
                Size      = new Size(510, 44),
                Font      = new Font("Segoe UI Semibold", 11F),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "Asignar Nuevo Folio",
            };
            btnCerrar = new Label
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(panelTitleBar.Width - 36, 0),
                Size      = new Size(36, 44),
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor    = Cursors.Hand,
                Text      = "X",
            };
            panelTitleBar.Controls.Add(lblTitulo);
            panelTitleBar.Controls.Add(btnCerrar);
            UiHelpers.EngancharDragNativo(panelTitleBar, this);
            UiHelpers.EngancharDragNativo(lblTitulo,     this);
            UiHelpers.ConfigurarBotonCerrar(btnCerrar, Color.FromArgb(220, 230, 245),
                () => { Resultado = OpcionFolio.Cancelar; this.DialogResult = DialogResult.Cancel; this.Close(); });

            // --- Body ---
            panelBody = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
                Padding   = new Padding(28, 20, 28, 20),
            };

            lblHint = new Label
            {
                Location  = new Point(28, 60),
                Size      = new Size(544, 60),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text      = "El folio del proveedor ya existe en Microsip para este proveedor. " +
                            "Puedes insertar la factura con una serie + folio distinto, o solo " +
                            "marcarla como aplicada en el portal sin tocar Microsip.",
            };

            lblFolioCfdi = new Label
            {
                Location  = new Point(28, 130),
                Size      = new Size(150, 25),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text      = "Folio en CFDI",
            };
            txtFolioCfdi = new TextBox
            {
                Location    = new Point(180, 126),
                Size        = new Size(390, 25),
                BackColor   = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly    = true,
                Font        = new Font("Consolas", 9F),
                Text        = _folioCfdi,
            };

            lblSerie = new Label
            {
                Location  = new Point(28, 172),
                Size      = new Size(150, 25),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text      = "Nueva serie",
            };
            txtSerie = new TextBox
            {
                Location    = new Point(180, 168),
                Size        = new Size(140, 25),
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Consolas", 10F),
                CharacterCasing = CharacterCasing.Upper,
                MaxLength   = 4,
            };
            txtSerie.KeyPress += (s, e) =>
            {
                // Solo letras (réplica F_NUEVO_FOLIO.cs:85).
                if (!char.IsLetter(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                    e.Handled = true;
            };

            lblFolio = new Label
            {
                Location  = new Point(28, 210),
                Size      = new Size(150, 25),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text      = "Nuevo folio",
            };
            txtFolio = new TextBox
            {
                Location    = new Point(180, 206),
                Size        = new Size(140, 25),
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Consolas", 10F),
                MaxLength   = 9,
            };
            // Solo dígitos (réplica F_NUEVO_FOLIO.cs:67-80).
            txtFolio.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };

            panelBody.Controls.Add(lblHint);
            panelBody.Controls.Add(lblFolioCfdi); panelBody.Controls.Add(txtFolioCfdi);
            panelBody.Controls.Add(lblSerie);     panelBody.Controls.Add(txtSerie);
            panelBody.Controls.Add(lblFolio);     panelBody.Controls.Add(txtFolio);

            // --- Botones (tres, mismo orden que el SOAP) ---
            panelBotones = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 64,
                BackColor = Color.White,
                Padding   = new Padding(20, 12, 20, 12),
            };
            panelBotones.Width = this.ClientSize.Width;
            panelBotones.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, 0, panelBotones.Width, 0);
            };

            // Botón Insertar (RESULTADO=1) — primario.
            btnInsertar = new Button
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(panelBotones.Width - 192, 12),
                Size      = new Size(172, 36),
                BackColor = Tema.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9.5F),
                Text      = "Insertar con nuevo folio",
                Cursor    = Cursors.Hand,
            };
            btnInsertar.FlatAppearance.BorderSize = 0;
            btnInsertar.Click += BtnInsertar_Click;

            // Botón Actualizar (RESULTADO=2) — secundario.
            btnActualizar = new Button
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(panelBotones.Width - 392, 12),
                Size      = new Size(192, 36),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5F),
                Text      = "Actualizar nuevo folio en portal",
                Cursor    = Cursors.Hand,
            };
            btnActualizar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnActualizar.FlatAppearance.BorderSize  = 1;
            btnActualizar.Click += BtnActualizar_Click;

            // Botón Nada (RESULTADO=3) — cancelar.
            btnNada = new Button
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(panelBotones.Width - 502, 12),
                Size      = new Size(102, 36),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5F),
                Text      = "No hacer nada",
                Cursor    = Cursors.Hand,
            };
            btnNada.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnNada.FlatAppearance.BorderSize  = 1;
            btnNada.Click += BtnNada_Click;

            panelBotones.Controls.Add(btnNada);
            panelBotones.Controls.Add(btnActualizar);
            panelBotones.Controls.Add(btnInsertar);

            this.Controls.Add(panelBody);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelTitleBar);

            this.AcceptButton = btnInsertar;
            this.CancelButton = btnNada;

            UiHelpers.AplicarEsquinasRedondeadas(this, 10);
        }

        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            string serie = (txtSerie.Text ?? "").Trim();
            string folio = (txtFolio.Text ?? "").Trim();

            if (serie.Length == 0 || folio.Length == 0)
            {
                // Texto literal del SOAP F_NUEVO_FOLIO.cs:47.
                MessageBox.Show(
                    "Necesita proporcionar la serie y el folio del nuevo folio con el que desea insertar la factura",
                    "Mensaje de la Aplicación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string serieUpper = serie.ToUpper();
            // Padding con ceros hasta 9 chars totales (F_NUEVO_FOLIO.cs:32-34).
            while ((serieUpper.Length + folio.Length) < 9)
                serieUpper = serieUpper + "0";
            string nuevo = serieUpper + folio;

            // Réplica de la validación F_NUEVO_FOLIO.cs:35-39.
            if (nuevo == _folioCfdi)
            {
                MessageBox.Show(
                    "El folio tiene que ser diferente al que viene en el XML",
                    "Mensaje de la Aplicación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Resultado         = OpcionFolio.InsertarConNuevoFolio;
            FolioNuevo        = nuevo;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            Resultado         = OpcionFolio.ActualizarFolioEnPortal;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnNada_Click(object sender, EventArgs e)
        {
            Resultado         = OpcionFolio.Cancelar;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
