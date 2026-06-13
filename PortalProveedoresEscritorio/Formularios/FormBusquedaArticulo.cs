using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Modelos;
using PortalProveedoresEscritorio.Servicios;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Modal "Buscar artículo" — réplica funcional de <c>F_BUSQUEDA</c> del
    /// SOAP. Carga los artículos NO almacenables del Microsip de la empresa
    /// y permite filtrar por nombre (LIKE %X%). Doble-click o Aceptar
    /// devuelve el seleccionado.
    ///
    /// Construido 100% en código.
    /// </summary>
    public sealed class FormBusquedaArticulo : Form
    {
        private readonly string _nombreEmpresa;
        private readonly CatalogosMicrosip _catalogos = new CatalogosMicrosip();

        public ArticuloMicrosip ArticuloSeleccionado { get; private set; }

        private Panel        panelTitleBar;
        private Label        lblTitulo;
        private Label        btnCerrar;
        private Panel        panelToolbar;
        private TextBox      txtBuscar;
        private Button       btnBuscar;
        private Label        lblContador;
        private DataGridView dgv;
        private Panel        panelBotones;
        private Button       btnAceptar;
        private Button       btnCancelar;

        public FormBusquedaArticulo(string nombreEmpresa)
        {
            _nombreEmpresa = nombreEmpresa ?? throw new ArgumentNullException(nameof(nombreEmpresa));

            ConstruirUI();
            this.Shown += async (s, e) => await BuscarAsync("");
        }

        private void ConstruirUI()
        {
            this.Text            = "Buscar artículo en Microsip";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.ClientSize      = new Size(800, 600);
            this.MinimumSize     = new Size(700, 500);
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
                Size      = new Size(700, 44),
                Font      = new Font("Segoe UI Semibold", 11F),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "🔍  Buscar artículo en Microsip — " + _nombreEmpresa,
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
                Text      = "✕",
            };
            panelTitleBar.Controls.Add(lblTitulo);
            panelTitleBar.Controls.Add(btnCerrar);
            UiHelpers.EngancharDragNativo(panelTitleBar, this);
            UiHelpers.EngancharDragNativo(lblTitulo,     this);
            UiHelpers.ConfigurarBotonCerrar(btnCerrar, Color.FromArgb(220, 230, 245),
                () => { this.DialogResult = DialogResult.Cancel; this.Close(); });

            // --- Toolbar ---
            panelToolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = Color.White,
                Padding   = new Padding(20, 12, 20, 12),
            };
            panelToolbar.Width = this.ClientSize.Width;
            panelToolbar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, panelToolbar.Height - 1,
                                              panelToolbar.Width, panelToolbar.Height - 1);
            };

            txtBuscar = new TextBox
            {
                Location    = new Point(20, 16),
                Size        = new Size(500, 25),
                Font        = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
            };
            txtBuscar.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await BuscarAsync(txtBuscar.Text);
                }
            };

            btnBuscar = new Button
            {
                Location  = new Point(530, 14),
                Size      = new Size(108, 30),
                BackColor = Tema.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9F),
                Text      = "🔍  Buscar",
                Cursor    = Cursors.Hand,
            };
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Click += async (s, e) => await BuscarAsync(txtBuscar.Text);

            lblContador = new Label
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(panelToolbar.Width - 160, 20),
                Size      = new Size(140, 22),
                Font      = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.MiddleRight,
                Text      = "",
            };
            panelToolbar.Controls.Add(txtBuscar);
            panelToolbar.Controls.Add(btnBuscar);
            panelToolbar.Controls.Add(lblContador);

            // --- Grid ---
            dgv = new DataGridView
            {
                Dock                       = DockStyle.Fill,
                BackgroundColor            = Color.White,
                BorderStyle                = BorderStyle.None,
                AllowUserToAddRows         = false,
                AllowUserToDeleteRows      = false,
                ReadOnly                   = true,
                MultiSelect                = false,
                SelectionMode              = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode        = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible          = false,
                AutoGenerateColumns        = false,
                EnableHeadersVisualStyles  = false,
                CellBorderStyle            = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor                  = Color.FromArgb(241, 245, 249),
                ColumnHeadersBorderStyle   = DataGridViewHeaderBorderStyle.None,
                RowTemplate                = { Height = 32 },
            };
            dgv.DefaultCellStyle.Font                       = new Font("Segoe UI", 9.5F);
            dgv.ColumnHeadersDefaultCellStyle.Font          = new Font("Segoe UI Semibold", 9F);
            dgv.ColumnHeadersDefaultCellStyle.BackColor     = Color.FromArgb(247, 249, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor     = Color.FromArgb(71, 85, 105);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 249, 252);
            dgv.AlternatingRowsDefaultCellStyle.BackColor   = Color.FromArgb(252, 252, 254);

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ID", HeaderText = "Id", ReadOnly = true, Visible = false,
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CLAVE", HeaderText = "Clave", ReadOnly = true, FillWeight = 20,
                DefaultCellStyle = { Font = new Font("Consolas", 9F) },
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NOMBRE", HeaderText = "Nombre del artículo", ReadOnly = true, FillWeight = 80,
            });
            dgv.CellDoubleClick += dgv_CellDoubleClick;

            // --- Botones ---
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

            btnCancelar = new Button
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(panelBotones.Width - 252, 12),
                Size      = new Size(112, 36),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5F),
                Text      = "Cancelar",
                Cursor    = Cursors.Hand,
            };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCancelar.FlatAppearance.BorderSize  = 1;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnAceptar = new Button
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(panelBotones.Width - 132, 12),
                Size      = new Size(112, 36),
                BackColor = Tema.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9.5F),
                Text      = "Aceptar",
                Cursor    = Cursors.Hand,
            };
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += BtnAceptar_Click;

            panelBotones.Controls.Add(btnCancelar);
            panelBotones.Controls.Add(btnAceptar);

            // --- Compose ---
            this.Controls.Add(dgv);
            this.Controls.Add(panelToolbar);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelTitleBar);

            this.AcceptButton = btnAceptar;
            this.CancelButton = btnCancelar;

            UiHelpers.AplicarEsquinasRedondeadas(this, 10);

            // BUG 4 — este buscador se abre encima de FormAplicarFactura, que
            // también tiene fondos blancos. Sin un borde, los dos forms blancos
            // se funden y no se distingue dónde empieza este. Le damos una
            // identidad visual: un borde de 2px en Tema.Primary (el mismo
            // color de su barra de título) siguiendo las esquinas redondeadas,
            // para que se vea claramente como una ventana separada y moderna.
            this.Padding = new Padding(2);
            this.BackColor = Tema.Primary;          // el "borde" = fondo del form que asoma 2px
            this.Paint += FormBusquedaArticulo_Paint;
        }

        /// <summary>
        /// Dibuja un borde redondeado de 2px en <see cref="Tema.Primary"/>
        /// alrededor del form para separarlo visualmente del padre blanco.
        /// El relleno interno (toolbar/grid/botones) ya es blanco, así que
        /// solo el contorno de 2px del fondo Tema.Primary queda visible.
        /// </summary>
        private void FormBusquedaArticulo_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (var path = UiHelpers.CrearPathRedondeado(rect, 10))
            using (var pen = new Pen(Tema.Primary, 2))
                e.Graphics.DrawPath(pen, path);
        }

        private async Task BuscarAsync(string filtro)
        {
            btnBuscar.Enabled = false;
            lblContador.Text  = "Buscando…";
            try
            {
                var resultados = await _catalogos
                    .BuscarArticulosAsync(_nombreEmpresa, filtro, 500, CancellationToken.None)
                    .ConfigureAwait(true);

                dgv.Rows.Clear();
                foreach (var a in resultados)
                    dgv.Rows.Add(a.Id, a.Clave ?? "", a.Nombre ?? "");

                lblContador.Text = resultados.Length == 0
                    ? "Sin resultados"
                    : resultados.Length + " artículo(s)";
            }
            catch (Exception ex)
            {
                lblContador.Text = "";
                MessageBox.Show(ex.Message, "Error inesperado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnBuscar.Enabled = true;
            }
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            // Réplica F_BUSQUEDA.cs:81-82 — pregunta de confirmación.
            if (MessageBox.Show("¿Seleccionar este articulo?",
                "Mensaje de la aplicación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            SeleccionarFila(e.RowIndex);
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un artículo de la lista.",
                    "Sin selección", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SeleccionarFila(dgv.SelectedRows[0].Index);
        }

        private void SeleccionarFila(int index)
        {
            var row = dgv.Rows[index];
            int id;
            int.TryParse((row.Cells["ID"].Value ?? "").ToString(),
                out id);
            ArticuloSeleccionado = new ArticuloMicrosip
            {
                Id     = id,
                Clave  = (row.Cells["CLAVE"].Value  ?? "").ToString(),
                Nombre = (row.Cells["NOMBRE"].Value ?? "").ToString(),
            };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
