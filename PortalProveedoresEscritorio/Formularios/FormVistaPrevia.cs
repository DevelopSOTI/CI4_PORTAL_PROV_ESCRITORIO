using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Servicios;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Modal "Vista previa de archivos" — muestra el PDF, el XML y la lista
    /// de adjuntos del CFDI sin necesidad de aplicar el documento. Réplica
    /// funcional del SOAP <c>F_VISTA_PREVIA</c>, pero con WebView2 inline
    /// (PDF renderizado por Edge Chromium) y reutilizando los endpoints REST
    /// que ya tenemos (<c>cfdi-xml</c>, <c>cfdi-pdf</c>, <c>/adjuntos</c>).
    ///
    /// UI construida en código (sin .Designer.cs) — el form es 100% un
    /// visor sin estado: tres tabs (PDF / XML / Adjuntos) más un botón
    /// "Cerrar".
    /// </summary>
    public sealed class FormVistaPrevia : Form
    {
        private readonly IPortalApi _api;
        private readonly string     _uuid;
        private readonly string     _tipo;       // "F" o "C"
        private readonly string     _folio;
        private readonly string     _proveedor;
        private readonly int        _doctoId;    // DOCTO_CM_ID factura o DOCTO_CP_ID complemento
        private readonly int        _empIdMsp;

        // --- Controles ---
        private Panel          panelTitleBar;
        private Label          lblTitulo;
        private Label          btnMinimizar;
        private Label          btnCerrar;

        private Panel          panelHeader;
        private Label          lblHeader;
        private Label          lblHeaderSub;

        private Panel          panelTabs;
        private Button         btnTabPdf;
        private Button         btnTabXml;
        private Button         btnTabAdjuntos;
        private Button         btnAbrirExterno;

        private Panel          panelVista;
        private WebView2       webView;
        private TextBox        txtVistaXml;
        private DataGridView   dgvAdjuntos;
        private Label          lblCargando;

        private Panel          panelBotones;
        private Button         btnCerrarAbajo;

        // --- Estado ---
        private string _pdfTempPath;
        private string _xmlTempPath;
        private readonly List<string> _adjuntosTempPaths = new List<string>();
        private bool _webViewListo;
        private enum Pestaña { Pdf, Xml, Adjuntos }
        private Pestaña _pestaña = Pestaña.Pdf;

        public FormVistaPrevia(IPortalApi api, string uuid, string tipo,
                               string folio, string proveedor,
                               int doctoId, int empIdMsp)
        {
            _api       = api  ?? throw new ArgumentNullException(nameof(api));
            _uuid      = uuid ?? "";
            _tipo      = (tipo == "C") ? "C" : "F";
            _folio     = folio     ?? "";
            _proveedor = proveedor ?? "";
            _doctoId   = doctoId;
            _empIdMsp  = empIdMsp;

            ConstruirUI();
            this.Shown      += async (s, e) => { await InicializarWebViewAsync(); await CargarAsync(); };
            this.FormClosed += FormVistaPrevia_FormClosed;
        }

        // ====================================================================
        // Layout
        // ====================================================================

        private void ConstruirUI()
        {
            this.Text            = "Vista previa de archivos";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.ClientSize      = new Size(1100, 720);
            this.MinimumSize     = new Size(900, 600);
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
            // FIX: forzar el Width ANTES de agregar hijos con Anchor=Right.
            // El Dock=Top ajustaría el Width al agregar al form, pero los
            // Anchor de los hijos ya habrían calculado sus offsets mal.
            panelTitleBar.Width = this.ClientSize.Width;
            lblTitulo = new Label
            {
                Location  = new Point(16, 0),
                Size      = new Size(900, 44),
                Font      = new Font("Segoe UI Semibold", 11F),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = _tipo == "C"
                          ? "Vista previa del complemento " + _folio
                          : "Vista previa de la factura "   + _folio,
            };
            btnMinimizar = new Label
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(this.ClientSize.Width - 72, 0),
                Size      = new Size(36, 44),
                Font      = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor    = Cursors.Hand,
                Text      = "─",
            };
            btnCerrar = new Label
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(this.ClientSize.Width - 36, 0),
                Size      = new Size(36, 44),
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor    = Cursors.Hand,
                Text      = "✕",
            };
            panelTitleBar.Controls.Add(lblTitulo);
            panelTitleBar.Controls.Add(btnMinimizar);
            panelTitleBar.Controls.Add(btnCerrar);
            UiHelpers.EngancharDragNativo(panelTitleBar, this);
            UiHelpers.EngancharDragNativo(lblTitulo,     this);
            Color iconoClaro = Color.FromArgb(220, 230, 245);
            UiHelpers.ConfigurarBotonCerrar(btnCerrar, iconoClaro, () => this.Close());
            UiHelpers.ConfigurarBotonMinimizar(btnMinimizar, iconoClaro, this);

            // --- Header (proveedor + UUID) ---
            panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = Color.White,
                Padding   = new Padding(20, 10, 20, 4),
            };
            lblHeader = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 24,
                Font      = new Font("Segoe UI Semibold", 10.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                Text      = "Proveedor: " + _proveedor,
            };
            lblHeaderSub = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 20,
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text      = "UUID " + _uuid,
            };
            panelHeader.Controls.Add(lblHeaderSub);
            panelHeader.Controls.Add(lblHeader);

            // --- Tabs ---
            panelTabs = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding   = new Padding(20, 8, 20, 8),
            };
            panelTabs.Width = this.ClientSize.Width;
            btnTabPdf      = ConstruirTab("📄  PDF",       new Point(20, 8),  activo: true);
            btnTabXml      = ConstruirTab("📑  XML",       new Point(135, 8), activo: false);
            btnTabAdjuntos = ConstruirTab("📎  Adjuntos",  new Point(250, 8), activo: false, ancho: 160);
            btnTabPdf.Enabled      = false;
            btnTabXml.Enabled      = false;
            btnTabAdjuntos.Enabled = false;
            btnTabPdf.Click      += (s, e) => MostrarPestaña(Pestaña.Pdf);
            btnTabXml.Click      += (s, e) => MostrarPestaña(Pestaña.Xml);
            btnTabAdjuntos.Click += (s, e) => MostrarPestaña(Pestaña.Adjuntos);

            btnAbrirExterno = new Button
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(this.ClientSize.Width - 174, 8),
                Size      = new Size(154, 32),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9F),
                Text      = "↗  Abrir externo",
                Cursor    = Cursors.Hand,
                Enabled   = false,
            };
            btnAbrirExterno.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnAbrirExterno.FlatAppearance.BorderSize  = 1;
            btnAbrirExterno.Click += btnAbrirExterno_Click;

            panelTabs.Controls.Add(btnAbrirExterno);
            panelTabs.Controls.Add(btnTabAdjuntos);
            panelTabs.Controls.Add(btnTabXml);
            panelTabs.Controls.Add(btnTabPdf);

            // --- Vista (WebView2 / TextBox XML / grid Adjuntos) ---
            panelVista = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 245, 249),
            };
            lblCargando = new Label
            {
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleCenter,
                Text      = "Cargando vista previa…",
            };
            webView = new WebView2
            {
                Dock                  = DockStyle.Fill,
                DefaultBackgroundColor= Color.FromArgb(241, 245, 249),
                Visible               = false,
            };
            txtVistaXml = new TextBox
            {
                Dock        = DockStyle.Fill,
                Multiline   = true,
                ScrollBars  = ScrollBars.Both,
                BorderStyle = BorderStyle.None,
                BackColor   = Color.FromArgb(248, 250, 252),
                ForeColor   = Color.FromArgb(30, 41, 59),
                Font        = new Font("Consolas", 9.5F),
                ReadOnly    = true,
                WordWrap    = false,
                Visible     = false,
            };
            dgvAdjuntos = ConstruirGridAdjuntos();
            panelVista.Controls.Add(dgvAdjuntos);
            panelVista.Controls.Add(txtVistaXml);
            panelVista.Controls.Add(webView);
            panelVista.Controls.Add(lblCargando);

            // --- Botones inferiores ---
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
            btnCerrarAbajo = new Button
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(this.ClientSize.Width - 140, 12),
                Size      = new Size(120, 40),
                BackColor = Tema.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9.5F),
                Text      = "Cerrar",
                Cursor    = Cursors.Hand,
            };
            btnCerrarAbajo.FlatAppearance.BorderSize = 0;
            btnCerrarAbajo.Click += (s, e) => this.Close();
            panelBotones.Controls.Add(btnCerrarAbajo);

            // --- Compose ---
            this.Controls.Add(panelVista);
            this.Controls.Add(panelTabs);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBotones);
            this.Controls.Add(panelTitleBar);

            UiHelpers.AplicarEsquinasRedondeadas(this, 10);
        }

        private Button ConstruirTab(string texto, Point loc, bool activo, int ancho = 110)
        {
            var b = new Button
            {
                Location  = loc,
                Size      = new Size(ancho, 32),
                BackColor = activo ? Tema.Primary : Color.White,
                ForeColor = activo ? Color.White  : Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9.5F),
                Text      = texto,
                Cursor    = Cursors.Hand,
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            b.FlatAppearance.BorderSize  = activo ? 0 : 1;
            return b;
        }

        private DataGridView ConstruirGridAdjuntos()
        {
            var dgv = new DataGridView
            {
                Dock                        = DockStyle.Fill,
                BackgroundColor             = Color.White,
                BorderStyle                 = BorderStyle.None,
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                AllowUserToResizeRows       = false,
                ReadOnly                    = false,
                MultiSelect                 = false,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible           = false,
                AutoGenerateColumns         = false,
                EnableHeadersVisualStyles   = false,
                CellBorderStyle             = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor                   = Color.FromArgb(241, 245, 249),
                Visible                     = false,
                ColumnHeadersBorderStyle    = DataGridViewHeaderBorderStyle.None,
            };
            dgv.RowTemplate.Height  = 40;
            dgv.ColumnHeadersHeight = 36;
            dgv.ColumnHeadersDefaultCellStyle.BackColor          = Color.FromArgb(247, 249, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor          = Color.FromArgb(71, 85, 105);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 249, 252);
            dgv.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI Semibold", 9F);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colAdjNombre", HeaderText = "Archivo", ReadOnly = true, FillWeight = 60F,
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colAdjTamano", HeaderText = "Tamaño", ReadOnly = true, FillWeight = 18F,
            });
            var btn = new DataGridViewButtonColumn
            {
                Name       = "colAdjDescargar",
                HeaderText = "",
                Text       = "📥 Descargar",
                UseColumnTextForButtonValue = true,
                FillWeight = 22F,
                FlatStyle  = FlatStyle.Flat,
            };
            btn.DefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            btn.DefaultCellStyle.ForeColor = Color.FromArgb(37, 99, 235);
            dgv.Columns.Add(btn);
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colAdjId", HeaderText = "Id", ReadOnly = true, Visible = false,
            });

            dgv.CellContentClick += dgvAdjuntos_CellContentClick;
            return dgv;
        }

        // ====================================================================
        // WebView2 + carga
        // ====================================================================

        private async Task InicializarWebViewAsync()
        {
            try
            {
                var carpetaUsuario = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SOTI", "PortalProveedoresEscritorio", "WebView2");
                Directory.CreateDirectory(carpetaUsuario);
                var env = await CoreWebView2Environment.CreateAsync(null, carpetaUsuario, new CoreWebView2EnvironmentOptions());
                await webView.EnsureCoreWebView2Async(env);
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                webView.CoreWebView2.Settings.AreDevToolsEnabled            = false;
                webView.CoreWebView2.Settings.IsStatusBarEnabled            = false;
                _webViewListo = true;

                if (!string.IsNullOrEmpty(_pdfTempPath) && File.Exists(_pdfTempPath))
                    MostrarPestaña(Pestaña.Pdf);
            }
            catch
            {
                _webViewListo = false;
                MostrarPestaña(Pestaña.Xml);
            }
        }

        private async Task CargarAsync()
        {
            var ct = CancellationToken.None;
            await Task.WhenAll(CargarXmlAsync(ct), CargarPdfAsync(ct), CargarAdjuntosAsync(ct))
                .ConfigureAwait(true);
        }

        private async Task CargarXmlAsync(CancellationToken ct)
        {
            try
            {
                var cfdi = await _api.ObtenerCfdiXmlAsync(_uuid, _tipo, ct).ConfigureAwait(true);
                if (cfdi == null || string.IsNullOrEmpty(cfdi.xml)) return;

                var formateado = FormatearXml(cfdi.xml);
                txtVistaXml.Text = formateado;
                btnTabXml.Enabled = true;
                ActualizarHabilitadoExterno();

                try
                {
                    _xmlTempPath = Path.Combine(Path.GetTempPath(),
                        "Preview_" + LimpiarNombre(_uuid) + ".xml");
                    File.WriteAllText(_xmlTempPath, formateado, new UTF8Encoding(false));
                }
                catch { _xmlTempPath = null; }

                if (string.IsNullOrEmpty(_pdfTempPath))
                    MostrarPestaña(Pestaña.Xml);
            }
            catch { }
        }

        private async Task CargarPdfAsync(CancellationToken ct)
        {
            try
            {
                var bin = await _api.ObtenerCfdiPdfAsync(_uuid, _tipo, ct).ConfigureAwait(true);
                if (bin == null || bin.Length == 0) return;

                _pdfTempPath = Path.Combine(Path.GetTempPath(),
                    "Preview_" + LimpiarNombre(_uuid) + ".pdf");
                File.WriteAllBytes(_pdfTempPath, bin);
                btnTabPdf.Enabled = true;
                ActualizarHabilitadoExterno();

                if (_webViewListo) MostrarPestaña(Pestaña.Pdf);
            }
            catch { }
        }

        private async Task CargarAdjuntosAsync(CancellationToken ct)
        {
            dgvAdjuntos.Rows.Clear();
            try
            {
                var lista = await _api.ListarAdjuntosAsync(_doctoId, _empIdMsp, _tipo, ct)
                    .ConfigureAwait(true);
                if (lista == null || lista.Length == 0)
                {
                    btnTabAdjuntos.Text    = "📎  Adjuntos (0)";
                    btnTabAdjuntos.Enabled = false;
                    return;
                }
                foreach (var a in lista)
                {
                    var nombre = string.IsNullOrEmpty(a.nombre_original) ? a.nombre_archivo : a.nombre_original;
                    dgvAdjuntos.Rows.Add(nombre, FormatearTamano(a.tamano), "📥 Descargar", a.id);
                }
                btnTabAdjuntos.Text    = "📎  Adjuntos (" + lista.Length + ")";
                btnTabAdjuntos.Enabled = true;
            }
            catch { btnTabAdjuntos.Enabled = false; }
        }

        // ====================================================================
        // Tabs
        // ====================================================================

        private void MostrarPestaña(Pestaña p)
        {
            _pestaña = p;
            EstilarTab(btnTabPdf,      p == Pestaña.Pdf);
            EstilarTab(btnTabXml,      p == Pestaña.Xml);
            EstilarTab(btnTabAdjuntos, p == Pestaña.Adjuntos);

            lblCargando.Visible = false;
            webView.Visible     = p == Pestaña.Pdf;
            txtVistaXml.Visible = p == Pestaña.Xml;
            dgvAdjuntos.Visible = p == Pestaña.Adjuntos;

            if (p == Pestaña.Pdf && _webViewListo
                && !string.IsNullOrEmpty(_pdfTempPath) && File.Exists(_pdfTempPath))
            {
                try { webView.CoreWebView2.Navigate(new Uri(_pdfTempPath).AbsoluteUri); } catch { }
            }
            ActualizarHabilitadoExterno();
        }

        private void EstilarTab(Button btn, bool activo)
        {
            btn.BackColor = activo ? Tema.Primary : Color.White;
            btn.ForeColor = activo ? Color.White  : Color.FromArgb(71, 85, 105);
            btn.FlatAppearance.BorderSize = activo ? 0 : 1;
        }

        private void ActualizarHabilitadoExterno()
        {
            switch (_pestaña)
            {
                case Pestaña.Pdf:
                    btnAbrirExterno.Enabled = !string.IsNullOrEmpty(_pdfTempPath);
                    btnAbrirExterno.Visible = true; break;
                case Pestaña.Xml:
                    btnAbrirExterno.Enabled = !string.IsNullOrEmpty(_xmlTempPath);
                    btnAbrirExterno.Visible = true; break;
                default:
                    btnAbrirExterno.Visible = false; break;
            }
        }

        private void btnAbrirExterno_Click(object sender, EventArgs e)
        {
            var path = _pestaña == Pestaña.Pdf ? _pdfTempPath
                     : _pestaña == Pestaña.Xml ? _xmlTempPath
                     : null;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            try { Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void dgvAdjuntos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvAdjuntos.Columns[e.ColumnIndex].Name != "colAdjDescargar") return;

            var row = dgvAdjuntos.Rows[e.RowIndex];
            int id;
            if (!int.TryParse((row.Cells["colAdjId"].Value ?? "").ToString(),
                              NumberStyles.Any, CultureInfo.InvariantCulture, out id))
                return;
            var nombre = (row.Cells["colAdjNombre"].Value ?? "adjunto").ToString();

            try
            {
                var bin = await _api.DescargarAdjuntoAsync(id, CancellationToken.None).ConfigureAwait(true);
                if (bin == null) return;
                var path = Path.Combine(Path.GetTempPath(),
                    "Adjunto_" + LimpiarNombre(_uuid) + "_" + LimpiarNombre(nombre));
                File.WriteAllBytes(path, bin);
                _adjuntosTempPaths.Add(path);
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ====================================================================
        // Cleanup
        // ====================================================================

        private void FormVistaPrevia_FormClosed(object sender, FormClosedEventArgs e)
        {
            BorrarSilencioso(_pdfTempPath);
            BorrarSilencioso(_xmlTempPath);
            foreach (var p in _adjuntosTempPaths) BorrarSilencioso(p);
        }

        private static void BorrarSilencioso(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        private static string FormatearTamano(int bytes)
        {
            if (bytes <= 0) return "—";
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("N1") + " KB";
            return (bytes / (1024.0 * 1024)).ToString("N1") + " MB";
        }

        private static string FormatearXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return "";
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                var sb = new StringBuilder();
                var settings = new XmlWriterSettings { Indent = true, IndentChars = "  " };
                using (var w = XmlWriter.Create(sb, settings)) doc.Save(w);
                return sb.ToString();
            }
            catch { return xml; }
        }

        private static string LimpiarNombre(string s)
        {
            if (string.IsNullOrEmpty(s)) return "cfdi";
            var inv = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s) sb.Append(Array.IndexOf(inv, ch) >= 0 ? '_' : ch);
            return sb.ToString();
        }
    }
}
