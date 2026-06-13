using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Servicios;
using PortalProveedoresEscritorio.Servicios;

namespace PortalProveedoresEscritorio.Vistas
{
    /// <summary>
    /// Vista del tab "Proveedores" del FormPrincipal. Réplica funcional de
    /// <c>F_PROVEEDORES</c> del SOAP — lista los proveedores con cuenta de
    /// acceso al portal (sale de la tabla <c>ACCESO</c>).
    ///
    /// Columnas literales del SOAP F_PROVEEDORES.cs:42-45:
    /// <c>PROVEEDOR_ID</c> / <c>NOMBRE</c> (USUARIO) /
    /// <c>RFC</c> (RAZON_SOCIAL) / <c>CORREO</c>.
    /// </summary>
    public sealed class VistaProveedores : UserControl
    {
        private readonly IPortalApi        _api;
        private readonly EmpresaEscritorio _empresa;

        private Panel        panelToolbar;
        private Button       btnConsultar;
        private Label        lblContador;
        private DataGridView dgvProv;

        public VistaProveedores(IPortalApi api, EmpresaEscritorio empresa)
        {
            _api     = api     ?? throw new ArgumentNullException(nameof(api));
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));

            ConstruirUI();
            this.Load += async (s, e) => await ConsultarAsync();
        }

        private void ConstruirUI()
        {
            this.Dock      = DockStyle.Fill;
            this.BackColor = Color.FromArgb(247, 249, 252);

            // ---- Toolbar ----
            panelToolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 64,
                BackColor = Color.White,
                Padding   = new Padding(20, 10, 20, 10),
            };
            panelToolbar.Width = this.Width;
            panelToolbar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Tema.Primary, 2))
                    e.Graphics.DrawLine(pen, 0, panelToolbar.Height - 1,
                                              panelToolbar.Width, panelToolbar.Height - 1);
            };

            btnConsultar = new Button
            {
                Location  = new Point(20, 16),
                Size      = new Size(108, 30),
                BackColor = Tema.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9F),
                Text      = "Consultar",
                Cursor    = Cursors.Hand,
            };
            btnConsultar.FlatAppearance.BorderSize = 0;
            btnConsultar.Click += async (s, e) => await ConsultarAsync();

            lblContador = new Label
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(panelToolbar.Width - 280, 22),
                Size      = new Size(260, 22),
                Font      = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.MiddleRight,
                Text      = "",
            };

            panelToolbar.Controls.Add(btnConsultar);
            panelToolbar.Controls.Add(lblContador);

            // ---- Grid ----
            dgvProv = new DataGridView
            {
                Dock                       = DockStyle.Fill,
                BackgroundColor            = Color.White,
                BorderStyle                = BorderStyle.None,
                AllowUserToAddRows         = false,
                AllowUserToDeleteRows      = false,
                AllowUserToResizeRows      = false,
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
                RowTemplate                = { Height = 36 },
            };
            dgvProv.ColumnHeadersHeight                          = 36;
            dgvProv.DefaultCellStyle.Font                        = new Font("Segoe UI", 9.5F);
            dgvProv.ColumnHeadersDefaultCellStyle.BackColor      = Color.FromArgb(247, 249, 252);
            dgvProv.ColumnHeadersDefaultCellStyle.ForeColor      = Color.FromArgb(71, 85, 105);
            dgvProv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 249, 252);
            dgvProv.ColumnHeadersDefaultCellStyle.Font           = new Font("Segoe UI Semibold", 9F);
            dgvProv.AlternatingRowsDefaultCellStyle.BackColor    = Color.FromArgb(252, 252, 254);

            // === Columnas literales del SOAP F_PROVEEDORES.cs:42-45 ===
            // El PROVEEDOR_ID se carga pero queda oculto (es de uso interno;
            // el operador no lo necesita ver).
            dgvProv.Columns.Add(Col("PROVEEDOR_ID", "ID",          80, monoespacio: true, hidden: true));
            dgvProv.Columns.Add(Col("NOMBRE",       "Usuario",     180));
            dgvProv.Columns.Add(Col("RFC",          "Razón social",340));
            dgvProv.Columns.Add(Col("CORREO",       "Correo",      280));
            dgvProv.Columns.Add(Col("ESTATUS",      "Estatus",     80));

            this.Controls.Add(dgvProv);     // Fill
            this.Controls.Add(panelToolbar);// Top
        }

        private static DataGridViewTextBoxColumn Col(string name, string header, int width,
            bool monoespacio = false, bool hidden = false)
        {
            var c = new DataGridViewTextBoxColumn
            {
                Name       = name,
                HeaderText = header,
                ReadOnly   = true,
                Visible    = !hidden,
                FillWeight = hidden ? 1 : width,
            };
            if (monoespacio)
                c.DefaultCellStyle.Font = new Font("Consolas", 9F);
            return c;
        }

        private async Task ConsultarAsync()
        {
            btnConsultar.Enabled = false;
            lblContador.Text     = "Cargando…";

            try
            {
                var resp = await _api.ObtenerProveedoresRegistradosAsync(
                    _empresa.Id, CancellationToken.None
                ).ConfigureAwait(true);

                dgvProv.Rows.Clear();
                int n = 0;
                if (resp != null && resp.proveedores != null)
                {
                    foreach (var p in resp.proveedores)
                    {
                        // El SOAP llena las columnas NOMBRE/RFC con USUARIO/
                        // RAZON_SOCIAL (F_PROVEEDORES.cs:42-44).
                        dgvProv.Rows.Add(
                            p.proveedor_id,
                            p.usuario      ?? "",
                            p.razon_social ?? "",
                            p.correo       ?? "",
                            EstatusLegible(p.estatus));
                        n++;
                    }
                }

                lblContador.Text = n == 0
                    ? "Sin proveedores con acceso al portal"
                    : (n + " proveedor" + (n == 1 ? "" : "es") + " registrado" + (n == 1 ? "" : "s"));
            }
            catch (Exception ex)
            {
                lblContador.Text = "";
                MessageBox.Show(ex.Message, "Error inesperado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnConsultar.Enabled = true;
            }
        }

        private static string EstatusLegible(string raw)
        {
            switch ((raw ?? "").ToUpperInvariant())
            {
                case "A": return "Activo";
                case "B": return "Bloqueado";
                case "N": return "Nuevo";
                default:  return raw ?? "";
            }
        }
    }
}
