using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Web.WebView2.Core;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Repositorios;
using PortalProveedoresCore.Servicios;
using PortalProveedoresEscritorio.Servicios;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Modal "Aplicar compra en Microsip" — réplica funcional del SOAP
    /// con estética moderna en layout split (40/60):
    /// <list type="bullet">
    ///   <item><b>Izquierda</b>: "Datos del proveedor" + "Descripción de la
    ///         compra" — campos compactos en columna única.</item>
    ///   <item><b>Derecha (grande)</b>: "Factura del proveedor" — botones
    ///         para abrir el PDF y XML del CFDI en el visor predeterminado
    ///         del sistema (Acrobat, Edge, etc.) + preview del XML
    ///         formateado debajo.</item>
    /// </list>
    ///
    /// Los binarios se descargan vía REST a <c>%TEMP%</c> al abrir el modal
    /// y se borran en <see cref="OnFormClosed"/>. La descarga del PDF
    /// busca dentro de los adjuntos del portal el primer archivo con
    /// extensión <c>.pdf</c>.
    /// </summary>
    public partial class FormAplicarFactura : Form
    {
        private readonly EmpresaEscritorio          _empresa;
        private readonly FacturaPendienteEscritorio _factura;
        private readonly AplicadorFacturas          _aplicador;
        private readonly IPortalApi                 _api;
        private readonly string                     _usuarioMicrosip;

        private CancellationTokenSource _cts;
        private string _pdfTempPath;
        private string _xmlTempPath;
        private readonly List<string> _adjuntosTempPaths = new List<string>();
        private bool   _webViewListo;
        private enum   PestañaActiva { Pdf, Xml, Adjuntos }
        private PestañaActiva _pestaña = PestañaActiva.Pdf;

        public bool   FacturaAplicada { get; private set; }
        public string FolioMicrosip   { get; private set; }

        public FormAplicarFactura(EmpresaEscritorio empresa,
                                  FacturaPendienteEscritorio factura,
                                  AplicadorFacturas aplicador,
                                  IPortalApi api,
                                  string usuarioMicrosip)
        {
            _empresa         = empresa   ?? throw new ArgumentNullException(nameof(empresa));
            _factura         = factura   ?? throw new ArgumentNullException(nameof(factura));
            _aplicador       = aplicador ?? throw new ArgumentNullException(nameof(aplicador));
            _api             = api       ?? throw new ArgumentNullException(nameof(api));
            _usuarioMicrosip = usuarioMicrosip ?? "";

            InitializeComponent();
            AplicarTemaYHandlers();
            LlenarDatos();

            this.Shown       += async (s, e) => { await InicializarWebViewAsync(); await CargarAsync(); };
            this.FormClosing += FormAplicarFactura_FormClosing;
            this.FormClosed  += FormAplicarFactura_FormClosed;
        }

        /// <summary>
        /// Inicializa el WebView2 con su carpeta de datos en %LOCALAPPDATA%.
        /// Sin <c>EnsureCoreWebView2Async</c> el control no funciona —
        /// es el handshake con el runtime Edge Chromium del sistema.
        /// Si el runtime no está instalado, captura la excepción y deja
        /// el TextBox del XML como vista única.
        /// </summary>
        private async Task InicializarWebViewAsync()
        {
            try
            {
                var carpetaUsuario = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SOTI", "PortalProveedoresEscritorio", "WebView2");
                Directory.CreateDirectory(carpetaUsuario);

                var env = await CoreWebView2Environment.CreateAsync(
                    null, carpetaUsuario, new CoreWebView2EnvironmentOptions());
                await this.webView.EnsureCoreWebView2Async(env);

                // Sin barra de Edge ni menú contextual de devtools.
                this.webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                this.webView.CoreWebView2.Settings.AreDevToolsEnabled            = false;
                this.webView.CoreWebView2.Settings.IsStatusBarEnabled            = false;

                _webViewListo = true;

                // Si el PDF ya se descargó mientras el WebView se inicializaba,
                // mostrarlo ya. Si no, la pestaña queda con el lblCargando.
                if (!string.IsNullOrEmpty(_pdfTempPath) && File.Exists(_pdfTempPath))
                    MostrarPestaña(PestañaActiva.Pdf);
            }
            catch (Exception ex)
            {
                _webViewListo = false;
                this.sec3Subtitulo.Text = "WebView2 no disponible (" + ex.Message
                    + "). Mostrando solo XML.";
                // Forzamos al XML como vista única — el PDF inline no es posible.
                MostrarPestaña(PestañaActiva.Xml);
            }
        }

        private void AplicarTemaYHandlers()
        {
            this.panelTitleBar.BackColor = Tema.Primary;
            this.btnAplicar.BackColor    = Tema.Primary;

            // Título con folio de la recepción + nombre de la empresa para
            // que el operador identifique con qué documento está trabajando
            // sin tener que ver el contenido del modal.
            var folioRec = string.IsNullOrEmpty(_factura.FOLIO_RECEPCION)
                ? "(sin recepción)"
                : _factura.FOLIO_RECEPCION;
            this.lblTitulo.Text = "Aplicar Compra en Microsip · Recepción "
                                + folioRec + "  ·  " + _empresa.NombreCorto;

            UiHelpers.AplicarEsquinasRedondeadas(this, 10);
            UiHelpers.EngancharDragNativo(this.panelTitleBar, this);
            UiHelpers.EngancharDragNativo(this.lblTitulo,     this);

            // Color claro contrastante — los íconos del SOAP se veían
            // invisibles porque les pasaba Tema.Primary (morado/morado).
            Color iconoClaro = Color.FromArgb(220, 230, 245);
            UiHelpers.ConfigurarBotonCerrar(
                this.btnCerrar, iconoClaro, () => CancelarYCerrar());
            UiHelpers.ConfigurarBotonMinimizar(this.btnMinimizar, iconoClaro, this);
            UiHelpers.ConfigurarBotonMaximizar(this.btnMaximizar, iconoClaro,
                () => TogglearMaximizar());

            // Doble clic en la barra de título maximiza/restaura, igual que
            // una ventana nativa de Windows (consistencia).
            this.panelTitleBar.DoubleClick += (s, e) => TogglearMaximizar();
            this.lblTitulo.DoubleClick     += (s, e) => TogglearMaximizar();

            // Bordes finos en cada card.
            DibujarBordeCard(this.sec1Card);
            DibujarBordeCard(this.sec2Card);
            DibujarBordeCard(this.sec3Card);

            // Línea separadora arriba de los tabs del card derecho.
            this.sec3Tabs.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, 0, this.sec3Tabs.Width, 0);
            };

            // Estilo del grid de adjuntos (paleta moderna, no Win98).
            this.dgvAdjuntos.ColumnHeadersDefaultCellStyle.BackColor          = Color.FromArgb(247, 249, 252);
            this.dgvAdjuntos.ColumnHeadersDefaultCellStyle.ForeColor          = Color.FromArgb(71, 85, 105);
            this.dgvAdjuntos.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 249, 252);
            this.dgvAdjuntos.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI Semibold", 9F);
            this.dgvAdjuntos.DefaultCellStyle.Font                            = new Font("Segoe UI", 9.5F);
            this.dgvAdjuntos.DefaultCellStyle.SelectionBackColor              = Color.FromArgb(241, 245, 249);
            this.dgvAdjuntos.DefaultCellStyle.SelectionForeColor              = Color.FromArgb(15, 23, 42);
            this.colAdjDescargar.DefaultCellStyle.BackColor                   = Color.FromArgb(241, 245, 249);
            this.colAdjDescargar.DefaultCellStyle.ForeColor                   = Color.FromArgb(37, 99, 235);
            this.colAdjDescargar.DefaultCellStyle.SelectionBackColor          = Color.FromArgb(226, 232, 240);
            this.colAdjDescargar.DefaultCellStyle.SelectionForeColor          = Color.FromArgb(37, 99, 235);
            this.dgvAdjuntos.CellContentClick += dgvAdjuntos_CellContentClick;

            // Pinta el estado inicial de los tabs (PDF activo como default).
            EstilarTabs();

            // Separadores arriba de los panels inferiores.
            this.panelEstado.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, 0, this.panelEstado.Width, 0);
            };
            this.panelBotones.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, 0, this.panelBotones.Width, 0);
            };
        }

        /// <summary>
        /// Alterna el form entre Normal y Maximized. Como el form usa chrome
        /// custom (FormBorderStyle.None + región redondeada), hay que manejar
        /// dos detalles que Windows haría solo con un borde nativo:
        /// <list type="number">
        ///   <item>Al maximizar, se quita la región redondeada
        ///         (<c>Region = null</c>) para que el form llene el rectángulo
        ///         completo sin esquinas cortadas; al restaurar se re-aplica
        ///         con <see cref="UiHelpers.AplicarEsquinasRedondeadas"/>.</item>
        ///   <item>Se fija <c>MaximizedBounds</c> al área de trabajo de la
        ///         pantalla para no tapar la barra de tareas — un form sin
        ///         borde maximizado, por defecto, cubre toda la pantalla.</item>
        /// </list>
        /// </summary>
        private void TogglearMaximizar()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                // Restaurar: volver a tamaño normal y re-aplicar esquinas.
                this.WindowState = FormWindowState.Normal;
                UiHelpers.AplicarEsquinasRedondeadas(this, 10);
                this.btnMaximizar.Text = "□";
            }
            else
            {
                // Maximizar respetando el área de trabajo (sin tapar la
                // barra de tareas) y sin la región redondeada.
                this.MaximizedBounds = Screen.FromControl(this).WorkingArea;
                this.Region          = null;
                this.WindowState     = FormWindowState.Maximized;
                this.btnMaximizar.Text = "❐";
            }
        }

        private static void DibujarBordeCard(Panel card)
        {
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
        }

        private void LlenarDatos()
        {
            this.txtProveedor.Text  = SafeString(_factura.PROVEEDOR_NOMBRE);
            this.txtFolioFac.Text   = SafeString(_factura.FOLIO_PROV);
            this.dtpFechaFac1.Value = ParsearFechaODefault(_factura.FECHA_FACTURA);
            this.txtAtraso.Text     = CalcularAtraso(_factura.FECHA_PAGO);
            this.txtFechaSubio.Text = FormatearFecha(_factura.FECHA_RECEPCION);
            this.txtSugerida.Text   = FormatearFecha(_factura.FECHA_PAGO);
            this.txtTotal.Text      = FormatearTotal(_factura.TOTAL, _factura.MONEDA_SIMBOLO);
            this.txtUUID.Text       = SafeString(_factura.UUID);

            this.rtDesc.Text = "Compra factura " + (_factura.FOLIO_PROV ?? "")
                            + " del proveedor "  + (_factura.PROVEEDOR_NOMBRE ?? "");

            // Habilitamos los combos y el botón de búsqueda — se llenan al
            // hacer Shown (carga asíncrona desde Firebird de la empresa).
            this.cbCondiciones.Items.Clear();
            this.cbCondiciones.Items.Add("Cargando…");
            this.cbCondiciones.SelectedIndex = 0;
            this.cbArticulo.Items.Clear();
            this.cbArticulo.Items.Add("(usa el buscador)");
            this.cbArticulo.SelectedIndex = 0;
            this.btnBuscarArticulo.Enabled = true;
            this.btnBuscarArticulo.Click  += BtnBuscarArticulo_Click;
        }

        // ====================================================================
        // E.6 — catálogos Microsip + buscador de artículo
        // ====================================================================

        private readonly CatalogosMicrosip _catalogos = new CatalogosMicrosip();

        private async Task CargarCondicionesPagoAsync()
        {
            try
            {
                var cond = await _catalogos
                    .ListarCondicionesPagoAsync(_empresa.NombreCorto, CancellationToken.None)
                    .ConfigureAwait(true);

                this.cbCondiciones.Items.Clear();
                if (cond.Length == 0)
                {
                    this.cbCondiciones.Items.Add("(sin catálogo)");
                    this.cbCondiciones.SelectedIndex = 0;
                    this.cbCondiciones.Enabled = false;
                    return;
                }
                foreach (var c in cond) this.cbCondiciones.Items.Add(c);
                this.cbCondiciones.SelectedIndex = 0;
                this.cbCondiciones.Enabled = true;
            }
            catch
            {
                this.cbCondiciones.Items.Clear();
                this.cbCondiciones.Items.Add("(error al leer)");
                this.cbCondiciones.SelectedIndex = 0;
                this.cbCondiciones.Enabled = false;
            }
        }

        private void BtnBuscarArticulo_Click(object sender, EventArgs e)
        {
            using (var dlg = new FormBusquedaArticulo(_empresa.NombreCorto))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ArticuloSeleccionado != null)
                {
                    var art = dlg.ArticuloSeleccionado;
                    this.cbArticulo.Items.Clear();
                    this.cbArticulo.Items.Add(art);
                    this.cbArticulo.SelectedIndex = 0;
                    this.cbArticulo.Enabled = true;
                }
            }
        }

        // ====================================================================
        // Carga de XML + PDF (REAL contra REST)
        // ====================================================================

        private async Task CargarAsync()
        {
            var ct = CancellationToken.None;
            // En paralelo: bajar XML del CFDI, descargar PDF del CFDI, listar
            // adjuntos extras del proveedor, y cargar el catálogo de
            // condiciones de pago desde Firebird de la empresa.
            await Task.WhenAll(
                CargarXmlAsync(ct),
                CargarPdfAsync(ct),
                CargarAdjuntosAsync(ct),
                CargarCondicionesPagoAsync()
            ).ConfigureAwait(true);
        }

        /// <summary>
        /// Lista los adjuntos extras del portal (PDFs/imágenes/OCs que el
        /// proveedor subió aparte del CFDI) y los pinta en el grid del tab
        /// "📎 Adjuntos". El tab queda deshabilitado si no hay nada.
        /// </summary>
        private async Task CargarAdjuntosAsync(CancellationToken ct)
        {
            this.dgvAdjuntos.Rows.Clear();
            try
            {
                var lista = await _api
                    .ListarAdjuntosAsync(_factura.DOCTO_CM_ID, _empresa.Id, "F", ct)
                    .ConfigureAwait(true);

                if (lista == null || lista.Length == 0)
                {
                    this.btnTabAdjuntos.Text    = "📎  Adjuntos (0)";
                    this.btnTabAdjuntos.Enabled = false;
                    return;
                }

                foreach (var a in lista)
                {
                    var nombre = string.IsNullOrEmpty(a.nombre_original)
                        ? a.nombre_archivo
                        : a.nombre_original;
                    this.dgvAdjuntos.Rows.Add(
                        nombre,
                        FormatearTamano(a.tamano),
                        "📥 Descargar",
                        a.id);
                }

                this.btnTabAdjuntos.Text    = "📎  Adjuntos (" + lista.Length + ")";
                this.btnTabAdjuntos.Enabled = true;
            }
            catch
            {
                // Silencioso — sin adjuntos el operador igual puede aplicar.
                this.btnTabAdjuntos.Enabled = false;
            }
        }

        private async void dgvAdjuntos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (this.dgvAdjuntos.Columns[e.ColumnIndex].Name != "colAdjDescargar") return;

            var row = this.dgvAdjuntos.Rows[e.RowIndex];
            int id;
            if (!int.TryParse((row.Cells["colAdjId"].Value ?? "").ToString(),
                              NumberStyles.Any, CultureInfo.InvariantCulture, out id))
                return;
            var nombre = (row.Cells["colAdjNombre"].Value ?? "adjunto").ToString();

            try
            {
                var binario = await _api.DescargarAdjuntoAsync(id, CancellationToken.None)
                    .ConfigureAwait(true);
                if (binario == null || binario.Length == 0)
                {
                    MessageBox.Show("El portal no devolvió contenido para este adjunto.",
                        "No disponible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Guardar con el nombre original para que el operador lo
                // reconozca al abrirlo (y para que la app del sistema use la
                // extensión correcta).
                var path = Path.Combine(Path.GetTempPath(),
                    "Adjunto_" + LimpiarParaNombreArchivo(_factura.UUID) + "_"
                    + LimpiarParaNombreArchivo(nombre));
                File.WriteAllBytes(path, binario);
                _adjuntosTempPaths.Add(path);

                Process.Start(new ProcessStartInfo
                {
                    FileName        = path,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo descargar el adjunto:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string FormatearTamano(int bytes)
        {
            if (bytes <= 0) return "—";
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("N1") + " KB";
            return (bytes / (1024.0 * 1024)).ToString("N1") + " MB";
        }

        private async Task CargarXmlAsync(CancellationToken ct)
        {
            try
            {
                var cfdi = await _api.ObtenerCfdiXmlAsync(_factura.UUID, "F", ct)
                    .ConfigureAwait(true);

                if (cfdi == null || string.IsNullOrEmpty(cfdi.xml))
                {
                    this.sec3Subtitulo.Text = "El portal no devolvió XML para este UUID.";
                    this.txtVistaXml.Text   = "";
                    return;
                }

                this.sec3Subtitulo.Text = "UUID " + (_factura.UUID ?? "")
                                       + "  ·  Uso CFDI: " + (cfdi.uso_cfdi ?? "—")
                                       + "  ·  Lugar exp.: " + (cfdi.lugar_expedicion ?? "—");

                var formateado = FormatearXml(cfdi.xml);
                this.txtVistaXml.Text = formateado;
                this.btnTabXml.Enabled = true;
                ActualizarHabilitadoExterno();

                // Guardar XML a %TEMP% para que "Abrir externo" pueda
                // lanzarlo con la app predeterminada del sistema.
                try
                {
                    _xmlTempPath = Path.Combine(Path.GetTempPath(),
                        "Factura_" + LimpiarParaNombreArchivo(_factura.UUID) + ".xml");
                    File.WriteAllText(_xmlTempPath, formateado, new UTF8Encoding(false));
                }
                catch
                {
                    _xmlTempPath = null;
                }

                // Si el PDF no llegó (todavía o nunca), mostrar el XML por defecto.
                if (string.IsNullOrEmpty(_pdfTempPath))
                    MostrarPestaña(PestañaActiva.Xml);
            }
            catch (Exception ex)
            {
                this.sec3Subtitulo.Text = "No se pudo obtener el CFDI: " + ex.Message;
                this.txtVistaXml.Text   = "";
            }
        }

        /// <summary>
        /// Descarga el PDF del CFDI vía el endpoint REST dedicado
        /// (<c>GET /api/aplicacion/cfdi-pdf?uuid=...&amp;tipo=F</c>) que sale
        /// de la columna <c>PDF</c> de <c>ARCHIVOS_FACTURA_PROVEEDOR_33</c>
        /// — el mismo lugar donde el proveedor subió el archivo cuando creó
        /// la factura en el portal web. Lo guarda en <c>%TEMP%</c> y habilita
        /// el botón "Abrir PDF".
        ///
        /// Si el endpoint devuelve null (proveedor subió solo XML), el botón
        /// queda deshabilitado — no es un error.
        /// </summary>
        private async Task CargarPdfAsync(CancellationToken ct)
        {
            try
            {
                var binario = await _api.ObtenerCfdiPdfAsync(_factura.UUID, "F", ct)
                    .ConfigureAwait(true);

                if (binario == null || binario.Length == 0) return;

                _pdfTempPath = Path.Combine(Path.GetTempPath(),
                    "Factura_" + LimpiarParaNombreArchivo(_factura.UUID) + ".pdf");
                File.WriteAllBytes(_pdfTempPath, binario);
                this.btnTabPdf.Enabled = true;
                ActualizarHabilitadoExterno();

                // Si el WebView2 ya está listo, navegar al PDF y mostrarlo
                // por defecto. Si todavía no (init en curso), se hace al
                // terminar Inicializar.
                if (_webViewListo)
                    MostrarPestaña(PestañaActiva.Pdf);
            }
            catch
            {
                // Silencioso — sin PDF el operador igual puede aplicar.
                _pdfTempPath = null;
            }
        }

        // ====================================================================
        // Tabs PDF / XML + abrir externo
        // ====================================================================

        private void btnTabPdf_Click(object sender, EventArgs e)      => MostrarPestaña(PestañaActiva.Pdf);
        private void btnTabXml_Click(object sender, EventArgs e)      => MostrarPestaña(PestañaActiva.Xml);
        private void btnTabAdjuntos_Click(object sender, EventArgs e) => MostrarPestaña(PestañaActiva.Adjuntos);

        private void MostrarPestaña(PestañaActiva p)
        {
            _pestaña = p;
            EstilarTabs();

            // El label "Cargando…" oculta el contenido vacío inicial. Lo
            // ocultamos en cuanto hay algo que mostrar.
            this.lblVistaCargando.Visible = false;

            bool esPdf = (p == PestañaActiva.Pdf);
            bool esXml = (p == PestañaActiva.Xml);
            bool esAdj = (p == PestañaActiva.Adjuntos);

            this.webView.Visible     = esPdf;
            this.txtVistaXml.Visible = esXml;
            this.dgvAdjuntos.Visible = esAdj;

            if (esPdf && _webViewListo
                && !string.IsNullOrEmpty(_pdfTempPath) && File.Exists(_pdfTempPath))
            {
                try
                {
                    // file:/// con el path del temp PDF — Edge muestra
                    // su visor de PDF inline con zoom + scroll + búsqueda.
                    this.webView.CoreWebView2.Navigate(new Uri(_pdfTempPath).AbsoluteUri);
                }
                catch { }
            }

            ActualizarHabilitadoExterno();
        }

        /// <summary>
        /// El tab activo se ve azul sólido; los inactivos se ven blancos
        /// con borde gris claro.
        /// </summary>
        private void EstilarTabs()
        {
            EstilarTabIndividual(this.btnTabPdf,      _pestaña == PestañaActiva.Pdf);
            EstilarTabIndividual(this.btnTabXml,      _pestaña == PestañaActiva.Xml);
            EstilarTabIndividual(this.btnTabAdjuntos, _pestaña == PestañaActiva.Adjuntos);
        }

        private void EstilarTabIndividual(Button btn, bool activo)
        {
            btn.BackColor = activo ? Tema.Primary       : Color.White;
            btn.ForeColor = activo ? Color.White        : Color.FromArgb(71, 85, 105);
            btn.FlatAppearance.BorderSize = activo ? 0 : 1;
        }

        private void ActualizarHabilitadoExterno()
        {
            // "Abrir externo" abre lo que la pestaña activa esté mostrando.
            // En el tab "Adjuntos" cada fila tiene su propio botón → el
            // global no aplica.
            switch (_pestaña)
            {
                case PestañaActiva.Pdf:
                    this.btnAbrirExterno.Enabled = !string.IsNullOrEmpty(_pdfTempPath);
                    this.btnAbrirExterno.Visible = true;
                    break;
                case PestañaActiva.Xml:
                    this.btnAbrirExterno.Enabled = !string.IsNullOrEmpty(_xmlTempPath);
                    this.btnAbrirExterno.Visible = true;
                    break;
                default: // Adjuntos
                    this.btnAbrirExterno.Enabled = false;
                    this.btnAbrirExterno.Visible = false;
                    break;
            }
        }

        private void btnAbrirExterno_Click(object sender, EventArgs e)
        {
            if (_pestaña == PestañaActiva.Pdf)
                AbrirArchivoExterno(_pdfTempPath, "PDF");
            else if (_pestaña == PestañaActiva.Xml)
                AbrirArchivoExterno(_xmlTempPath, "XML");
        }

        private void AbrirArchivoExterno(string path, string tipo)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                MessageBox.Show(
                    "El archivo " + tipo + " no está disponible.",
                    "No disponible",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName        = path,
                    UseShellExecute = true,    // app predeterminada del sistema
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo abrir el " + tipo + ":\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ====================================================================
        // Aplicar
        // ====================================================================

        private async void btnAplicar_Click(object sender, EventArgs e)
        {
            // Para facturas SIN recepción exigimos que el operador haya
            // elegido un artículo NO almacenable y una condición de pago en
            // los combos — son los parámetros que APLICAR_SIN_RECEPCION del
            // SOAP necesita para crear la línea genérica de DOCTOS_CM_DET.
            string articulo      = ExtraerNombreArticulo();
            string condicionPago = ExtraerNombreCondicion();

            if (_factura.RECEP_ID == 0)
            {
                if (string.IsNullOrEmpty(articulo))
                {
                    MostrarEstado("Elige un artículo NO almacenable (botón Buscar) "
                                + "antes de aplicar — la factura no tiene recepción ligada.",
                                  EstadoTipo.Error);
                    return;
                }
                if (string.IsNullOrEmpty(condicionPago))
                {
                    MostrarEstado("Elige una condición de pago antes de aplicar.",
                                  EstadoTipo.Error);
                    return;
                }
            }

            btnAplicar.Enabled  = false;
            btnCancelar.Enabled = false;
            barProgreso.Visible = true;
            barProgreso.Style   = ProgressBarStyle.Marquee;
            barProgreso.MarqueeAnimationSpeed = 30;

            _cts = new CancellationTokenSource();
            var progreso = new Progress<string>(msg => MostrarEstado(msg, EstadoTipo.Trabajando));

            // Captura la fecha elegida por el operador en el hilo de UI (el
            // DateTimePicker no puede leerse desde el Task.Run). Réplica del
            // SOAP: FECHA_DTP → DOCTOS_CM.FECHA (F_APLICAR_FACTURA.cs:387,1222).
            var fechaCompra = this.dtpFechaFac1.Value;

            ResultadoAplicacion r;
            try
            {
                r = await Task.Run(
                    () => _aplicador.AplicarAsync(
                        _empresa, _factura, articulo, condicionPago,
                        _usuarioMicrosip, fechaCompra, progreso, _cts.Token),
                    _cts.Token);
            }
            catch (OperationCanceledException)
            {
                MostrarEstado("Aplicación cancelada por el operador.", EstadoTipo.Error);
                RestaurarBotonesParaReintentar();
                return;
            }
            catch (Exception ex)
            {
                MostrarEstado("Error inesperado: " + ex.Message, EstadoTipo.Error);
                RestaurarBotonesParaReintentar();
                return;
            }
            finally
            {
                barProgreso.Style   = ProgressBarStyle.Continuous;
                barProgreso.Value   = 100;
                if (_cts != null) { _cts.Dispose(); _cts = null; }
            }

            ProcesarResultado(r);
        }

        private void ProcesarResultado(ResultadoAplicacion r)
        {
            // Réplica del SOAP F_APLICAR_FACTURA.cs:190-231 — cuando la
            // recepción origen YA estaba facturada en Microsip y se
            // sincronizó el portal con la compra existente, mostramos el
            // mensaje LITERAL del legacy. El repositorio devuelve este
            // caso como OkDryRun + portalMarcado=true + mensaje con la
            // marca "ya estaba en Microsip" para distinguirlo del éxito
            // normal (que sí creó compra nueva). La fila desaparece del
            // grid igual que el éxito normal porque la factura ya no
            // tiene sentido seguir intentándola.
            if (r != null
                && r.portalMarcado
                && r.mensaje != null
                && r.mensaje.IndexOf("ya estaba en Microsip", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string folioRec = string.IsNullOrEmpty(_factura.FOLIO_RECEPCION)
                    ? "(sin folio)"
                    : _factura.FOLIO_RECEPCION;

                // Mensaje LITERAL del SOAP F_APLICAR_FACTURA.cs:213.
                string msgSinc = "Se actualizo solo en el portal la recepcion "
                               + folioRec
                               + " correctamente, porque la factura ya estaba en Microsip";

                MessageBox.Show(this, msgSinc, "Mensaje de la Aplicación",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                FacturaAplicada   = true;
                FolioMicrosip     = r.folioFinalGenerado ?? "";
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }

            // Réplica del SOAP F_APLICAR_FACTURA.cs:155-186 — cuando la
            // recepción origen está cancelada en Microsip mostramos el
            // mensaje LITERAL del legacy. La factura ya quedó rechazada
            // en el portal MySQL (o se intentó marcar), así que tratamos
            // este caso como "ya terminamos" — cerramos el modal y se
            // quita la fila del grid de la vista padre.
            if (r != null && r.tipo == ResultadoAplicacionTipo.RecepcionCancelada)
            {
                string msg = "La recepción a la que se relacionó esta factura, "
                           + "actualmente se encuentra cancelada en Microsip. "
                           + "Favor de solicitar al proveedor que vuelva a subir la factura.";
                if (r.portalMarcado)
                    msg += Environment.NewLine + Environment.NewLine
                        + "La factura ya fue marcada como rechazada en el portal.";
                else
                    msg += Environment.NewLine + Environment.NewLine
                        + "AVISO: NO se pudo marcar la factura como rechazada en el portal — revísela manualmente.";

                MessageBox.Show(this, msg, "Mensaje de la Aplicación",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // La fila debe desaparecer del grid (igual que cuando se
                // aplica con éxito) porque la factura ya no tiene sentido
                // seguir intentándola — el portal la dejó rechazada.
                FacturaAplicada   = true;
                FolioMicrosip     = "";
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }

            // Réplica del SOAP F_APLICAR_FACTURA.cs:1132-1155 — folio
            // duplicado: abrir F_NUEVO_FOLIO y procesar la decisión del
            // operador. RESULTADO=2 dispara ws.ACTUALIZA_NUEVO_FOLIO.
            // RESULTADO=1 cierra el modal para que el operador relance
            // manualmente con el folio modificado (igual que el SOAP —
            // no hay reintento automático con el nuevo folio porque eso
            // requeriría re-llamar al backend del repositorio entero;
            // el SOAP también deja al operador volver a iniciar el flujo).
            // RESULTADO=3 simplemente cierra y deja la factura para
            // procesar después.
            if (r != null && r.tipo == ResultadoAplicacionTipo.FolioCompraDuplicado)
            {
                ManejarFolioDuplicado();
                return;
            }

            bool exito = EsResultadoExitoso(r);

            if (exito)
            {
                FacturaAplicada = true;
                FolioMicrosip   = r.folioFinalGenerado;

                // Texto adicional opcional del notificador de correo
                // (réplica del SOAP PROCESO_ENVIAR). Es informativo: aunque
                // diga "no se pudo notificar", la aplicación sigue siendo
                // exitosa — el legacy también enviaba best-effort.
                string textoCorreo = string.IsNullOrWhiteSpace(r.mensajeAdicionalEscritorio)
                    ? ""
                    : ("  ·  " + r.mensajeAdicionalEscritorio);

                MostrarEstado(
                    "Factura aplicada. Folio Microsip: " + r.folioFinalGenerado
                    + "  ·  Adjuntos: " + r.adjuntosInsertados
                    + (r.adjuntosOmitidos > 0 ? " (omitidos: " + r.adjuntosOmitidos + ")" : "")
                    + textoCorreo,
                    EstadoTipo.Exito);

                this.btnCancelar.Text    = "Cerrar";
                this.btnCancelar.Enabled = true;
                this.AcceptButton        = this.btnCancelar;
                this.btnAplicar.Visible  = false;
            }
            else
            {
                MostrarEstado(
                    "No se pudo aplicar (bloque " + r.ultimoBloque + "): " + r.mensaje,
                    EstadoTipo.Error);
                RestaurarBotonesParaReintentar();
            }
        }

        /// <summary>
        /// El éxito se decide por <c>tipo</c> + <c>portalMarcado</c>, NO por
        /// el número del bloque. El bloque final puede cambiar si el flujo
        /// del repositorio se reorganiza (en complementos por ejemplo es 7,
        /// no 15). El bloque solo se usa para el mensaje de error cuando
        /// algo falla.
        /// </summary>
        private static bool EsResultadoExitoso(ResultadoAplicacion r)
        {
            if (r == null) return false;
            switch (r.tipo)
            {
                case ResultadoAplicacionTipo.Error:
                case ResultadoAplicacionTipo.ErrorConexion:
                case ResultadoAplicacionTipo.RecepcionNoExiste:
                case ResultadoAplicacionTipo.RecepcionYaFacturada:
                case ResultadoAplicacionTipo.RecepcionYaFacturadaSincronizar:
                case ResultadoAplicacionTipo.RecepcionCancelada:
                case ResultadoAplicacionTipo.FolioCompraDuplicado:
                case ResultadoAplicacionTipo.SerieWebNoConfigurada:
                case ResultadoAplicacionTipo.CreditoNoExiste:
                case ResultadoAplicacionTipo.CreditoYaConCfdi:
                    return false;
                default:
                    return r.portalMarcado;
            }
        }

        /// <summary>
        /// Abre el modal <see cref="FormNuevoFolio"/> y procesa la decisión
        /// del operador ante un folio duplicado del proveedor. Réplica del
        /// SOAP F_APLICAR_FACTURA.cs:1132-1155.
        /// </summary>
        private async void ManejarFolioDuplicado()
        {
            using (var dlg = new FormNuevoFolio(_factura.FOLIO_PROV ?? ""))
            {
                dlg.ShowDialog(this);

                if (dlg.Resultado == FormNuevoFolio.OpcionFolio.ActualizarFolioEnPortal)
                {
                    // Réplica F_APLICAR_FACTURA.cs:1145-1152 — el operador
                    // eligió marcar la factura como aplicada en el portal
                    // sin tocar Microsip. Llamamos al endpoint
                    // /api/aplicacion/actualizar-nuevo-folio que pone
                    // ESTATUS='R' en FACTURA_PROVEEDOR_33.
                    try
                    {
                        bool ok = await _api.ActualizarNuevoFolioAsync(
                            _factura.DOCTO_CM_ID, CancellationToken.None).ConfigureAwait(true);
                        if (!ok)
                        {
                            // Texto literal del SOAP F_APLICAR_FACTURA.cs:1149.
                            MessageBox.Show(this,
                                "No se pudo actualizar el estatus de la recepción en portal",
                                "Mensaje de la Aplicación",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            RestaurarBotonesParaReintentar();
                            return;
                        }

                        MostrarEstado(
                            "Factura marcada como aplicada en el portal (sin tocar Microsip).",
                            EstadoTipo.Exito);
                        FacturaAplicada   = true;
                        FolioMicrosip     = "";
                        this.btnCancelar.Text    = "Cerrar";
                        this.btnCancelar.Enabled = true;
                        this.AcceptButton        = this.btnCancelar;
                        this.btnAplicar.Visible  = false;
                        // Conviene que el grid padre quite la fila —
                        // marcamos DialogResult.OK al cerrar.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this,
                            "Error al actualizar la factura en el portal:\n" + ex.Message,
                            "Mensaje de la Aplicación",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        RestaurarBotonesParaReintentar();
                    }
                    return;
                }

                if (dlg.Resultado == FormNuevoFolio.OpcionFolio.InsertarConNuevoFolio)
                {
                    // Réplica F_APLICAR_FACTURA.cs:1141-1144 — el SOAP no
                    // reintenta automático con el folio nuevo (el bloque
                    // 1141-1144 está vacío). Le decimos al operador qué
                    // folio escribió y dejamos que vuelva a Aplicar
                    // manualmente. El folio nuevo lo registra el SOAP en
                    // DOCTOS_CM/DOCTOS_CP más adelante; para el Escritorio
                    // nuevo el operador debe corregir el FOLIO_PROV en el
                    // portal y volver a aplicar (el repositorio relee el
                    // FOLIO_PROV en cada intento).
                    MostrarEstado(
                        "Folio nuevo asignado: " + dlg.FolioNuevo
                        + ". Corrige el folio en el portal y vuelve a aplicar.",
                        EstadoTipo.Trabajando);
                    RestaurarBotonesParaReintentar();
                    return;
                }

                // RESULTADO=3 (Cancelar) — F_APLICAR_FACTURA.cs:1153-1154
                // (return false). Simplemente restaurar UI para que el
                // operador pueda cerrar o reintentar.
                MostrarEstado(
                    "Aplicación cancelada por el operador (folio duplicado).",
                    EstadoTipo.Error);
                RestaurarBotonesParaReintentar();
            }
        }

        private void RestaurarBotonesParaReintentar()
        {
            this.btnAplicar.Enabled  = true;
            this.btnCancelar.Enabled = true;
            this.barProgreso.Visible = false;
        }

        // ====================================================================
        // Cerrar / cancelar / cleanup
        // ====================================================================

        private void btnCancelar_Click(object sender, EventArgs e) => CancelarYCerrar();

        private void CancelarYCerrar()
        {
            if (_cts != null) { try { _cts.Cancel(); } catch { } }
            this.DialogResult = FacturaAplicada ? DialogResult.OK : DialogResult.Cancel;
            this.Close();
        }

        private void FormAplicarFactura_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cts != null) { try { _cts.Cancel(); } catch { } }
        }

        private void FormAplicarFactura_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Limpia archivos temporales (PDF/XML + adjuntos descargados) —
            // son potencialmente grandes y contienen datos del CFDI.
            BorrarArchivoSilencioso(_pdfTempPath);
            BorrarArchivoSilencioso(_xmlTempPath);
            foreach (var path in _adjuntosTempPaths)
                BorrarArchivoSilencioso(path);
        }

        private static void BorrarArchivoSilencioso(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }

        // ====================================================================
        // Helpers
        // ====================================================================

        private enum EstadoTipo { Trabajando, Exito, Error }

        private void MostrarEstado(string mensaje, EstadoTipo tipo)
        {
            this.lblEstado.Text = mensaje;
            switch (tipo)
            {
                case EstadoTipo.Trabajando:
                    this.panelEstado.BackColor = Color.FromArgb(241, 245, 249);
                    this.lblEstado.ForeColor   = Color.FromArgb(51, 65, 85);
                    break;
                case EstadoTipo.Exito:
                    this.panelEstado.BackColor = Color.FromArgb(220, 252, 231);
                    this.lblEstado.ForeColor   = Color.FromArgb(22, 101, 52);
                    break;
                case EstadoTipo.Error:
                    this.panelEstado.BackColor = Color.FromArgb(254, 226, 226);
                    this.lblEstado.ForeColor   = Color.FromArgb(153, 27, 27);
                    break;
            }
        }

        private static string SafeString(string s)
        {
            return string.IsNullOrEmpty(s) ? "" : s;
        }

        private static string FormatearFecha(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            DateTime d;
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out d))
                return d.ToString("dd / MMM / yyyy", new CultureInfo("es-MX"));
            return raw;
        }

        /// <summary>Parsea la fecha del CFDI para el DateTimePicker; si no
        /// parsea usa hoy (el control no admite valores inválidos).</summary>
        private static DateTime ParsearFechaODefault(string raw)
        {
            DateTime d;
            if (!string.IsNullOrEmpty(raw) &&
                DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                return d;
            return DateTime.Today;
        }

        private static string FormatearTotal(decimal total, string moneda)
        {
            string m = string.IsNullOrEmpty(moneda) ? "MXN" : moneda;
            return total.ToString("N2", CultureInfo.InvariantCulture) + " " + m;
        }

        private static string CalcularAtraso(string fechaPago)
        {
            if (string.IsNullOrEmpty(fechaPago)) return "0";
            DateTime d;
            if (!DateTime.TryParse(fechaPago, CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out d))
                return "0";
            int dias = (int) Math.Floor((DateTime.Today - d.Date).TotalDays);
            return dias > 0 ? dias.ToString(CultureInfo.InvariantCulture) : "0";
        }

        private static string FormatearXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return "";
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                var sb = new StringBuilder();
                var settings = new XmlWriterSettings
                {
                    Indent              = true,
                    IndentChars         = "  ",
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration  = false,
                };
                using (var writer = XmlWriter.Create(sb, settings))
                {
                    doc.Save(writer);
                }
                return sb.ToString();
            }
            catch
            {
                return xml;
            }
        }

        /// <summary>
        /// Devuelve el nombre del artículo elegido en el combo. Devuelve ""
        /// si el ítem actual es un placeholder ("(usa el buscador)", "Cargando…")
        /// o si no hay selección. El SOAP usa el NOMBRE textual del artículo
        /// para buscarlo en Firebird por <c>ARTICULOS.NOMBRE</c>.
        /// </summary>
        private string ExtraerNombreArticulo()
        {
            var item = this.cbArticulo.SelectedItem;
            if (item == null) return "";
            var art = item as ArticuloMicrosip;
            if (art != null) return art.Nombre ?? "";
            // Placeholder o texto manual — ignorar.
            return "";
        }

        private string ExtraerNombreCondicion()
        {
            var item = this.cbCondiciones.SelectedItem;
            if (item == null) return "";
            var cp = item as CondicionPagoMicrosip;
            if (cp != null) return cp.Nombre ?? "";
            return "";
        }

        private static string LimpiarParaNombreArchivo(string s)
        {
            if (string.IsNullOrEmpty(s)) return "cfdi";
            var invalidos = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
                sb.Append(Array.IndexOf(invalidos, ch) >= 0 ? '_' : ch);
            return sb.ToString();
        }
    }
}
