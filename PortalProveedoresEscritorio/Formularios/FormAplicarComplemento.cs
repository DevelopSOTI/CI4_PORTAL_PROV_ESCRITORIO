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
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Repositorios;
using PortalProveedoresCore.Servicios;
using PortalProveedoresEscritorio.Servicios;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Modal "Asociar CFDI en Microsip" — réplica funcional de
    /// <c>F_APLICAR_COMPLEMENTO</c> del SOAP. Gemelo de
    /// <see cref="FormAplicarFactura"/> pero para complementos PPD:
    /// <list type="bullet">
    ///   <item>Sec 1: Datos del complemento.</item>
    ///   <item>Sec 2: Crédito Microsip al que se asocia (informativo).</item>
    ///   <item>Sec 3: Vista previa PDF/XML inline (WebView2).</item>
    /// </list>
    /// El backend usa <see cref="IAplicacionRepository.AplicarComplementoAsync"/>
    /// que NO crea nuevo documento — solo agrega el CFDI al CFD_RECIBIDOS
    /// y marca <c>TIENE_CFD='S'</c> en el DOCTOS_CP del crédito.
    /// </summary>
    public partial class FormAplicarComplemento : Form
    {
        private readonly EmpresaEscritorio    _empresa;
        private readonly ComplementoAplicar   _comp;
        private readonly AplicadorComplementos _aplicador;
        private readonly IPortalApi           _api;
        // Operador Microsip real — se sella en USUARIO_ASOCIO_COBRO del
        // portal (réplica F_APLICAR_COMPLEMENTO.cs:672). Mismo patrón que
        // FormAplicarFactura._usuarioMicrosip.
        private readonly string               _usuarioMicrosip;

        private CancellationTokenSource _cts;
        private string _pdfTempPath;
        private string _xmlTempPath;
        private readonly List<string> _adjuntosTempPaths = new List<string>();
        private bool   _webViewListo;
        private enum   PestañaActiva { Pdf, Xml, Adjuntos }
        private PestañaActiva _pestaña = PestañaActiva.Pdf;

        public bool   ComplementoAplicado { get; private set; }
        public string ResultadoMensaje    { get; private set; }

        public FormAplicarComplemento(EmpresaEscritorio empresa,
                                      ComplementoAplicar comp,
                                      AplicadorComplementos aplicador,
                                      IPortalApi api,
                                      string usuarioMicrosip)
        {
            _empresa         = empresa   ?? throw new ArgumentNullException(nameof(empresa));
            _comp            = comp      ?? throw new ArgumentNullException(nameof(comp));
            _aplicador       = aplicador ?? throw new ArgumentNullException(nameof(aplicador));
            _api             = api       ?? throw new ArgumentNullException(nameof(api));
            _usuarioMicrosip = usuarioMicrosip ?? "";

            InitializeComponent();
            AplicarTemaYHandlers();
            LlenarDatos();

            this.Shown       += async (s, e) => { await InicializarWebViewAsync(); await CargarAsync(); };
            this.FormClosing += FormAplicarComplemento_FormClosing;
            this.FormClosed  += FormAplicarComplemento_FormClosed;
        }

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

                this.webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                this.webView.CoreWebView2.Settings.AreDevToolsEnabled            = false;
                this.webView.CoreWebView2.Settings.IsStatusBarEnabled            = false;

                _webViewListo = true;

                if (!string.IsNullOrEmpty(_pdfTempPath) && File.Exists(_pdfTempPath))
                    MostrarPestaña(PestañaActiva.Pdf);
            }
            catch (Exception ex)
            {
                _webViewListo = false;
                this.sec3Subtitulo.Text = "WebView2 no disponible (" + ex.Message + "). Mostrando solo XML.";
                MostrarPestaña(PestañaActiva.Xml);
            }
        }

        private void AplicarTemaYHandlers()
        {
            this.panelTitleBar.BackColor = Tema.Primary;
            this.btnAplicar.BackColor    = Tema.Primary;

            // Réplica F_APLICAR_COMPLEMENTO.cs:436-446 — el SOAP cambia el
            // título y el botón según VERSION_PAGO. "0" = nota de crédito
            // (sin complemento de pago real), cualquier otra cosa = complemento
            // de pago propiamente dicho. El texto literal se conserva del SOAP.
            bool esNotaCredito = (_comp.VERSION_PAGO ?? "").Trim() == "0";
            if (esNotaCredito)
            {
                // F_APLICAR_COMPLEMENTO.cs:438-439 — literal SOAP.
                this.lblTitulo.Text   = "Asociación de nota de crédito al módulo de cuentas por cobrar — "
                                      + _empresa.NombreCorto;
                this.btnAplicar.Text  = "Asociar nota de crédito a Microsip";
            }
            else
            {
                // F_APLICAR_COMPLEMENTO.cs:443-444 — literal SOAP.
                this.lblTitulo.Text   = "Asociación del complemento al módulo de cuentas por cobrar — "
                                      + _empresa.NombreCorto;
                this.btnAplicar.Text  = "Asociar complemento a Microsip";
            }

            UiHelpers.AplicarEsquinasRedondeadas(this, 10);
            UiHelpers.EngancharDragNativo(this.panelTitleBar, this);
            UiHelpers.EngancharDragNativo(this.lblTitulo,     this);

            Color iconoClaro = Color.FromArgb(220, 230, 245);
            UiHelpers.ConfigurarBotonCerrar(
                this.btnCerrar, iconoClaro, () => CancelarYCerrar());
            UiHelpers.ConfigurarBotonMinimizar(this.btnMinimizar, iconoClaro, this);

            DibujarBordeCard(this.sec1Card);
            DibujarBordeCard(this.sec2Card);
            DibujarBordeCard(this.sec3Card);

            this.sec3Tabs.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, 0, this.sec3Tabs.Width, 0);
            };
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

            // Estilo del grid de adjuntos (paleta moderna).
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

            EstilarTabs();
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
            this.txtProveedor.Text   = SafeString(_comp.NOMBRE);
            this.txtFolioPago.Text   = SafeString(_comp.FOLIO_PAGO);
            this.txtFechaPago.Text   = FormatearFecha(_comp.FECHA_PAGO);
            this.txtFechaComp.Text   = FormatearFecha(_comp.FECHA_COMPLEMENTO);
            this.txtUsoCfdi.Text     = SafeString(_comp.USO_CFDI);
            this.txtMonto.Text       = FormatearMonto(_comp.MONTO, _comp.MONEDA_PAGO);
            this.txtUUID.Text        = SafeString(_comp.UUID);

            this.txtFolioCredito.Text = SafeString(_comp.FOLIO_CREDITO);
            this.txtCreditoFk.Text    = _comp.CREDITO_FK.ToString(CultureInfo.InvariantCulture);
        }

        // ====================================================================
        // Carga XML + PDF (REST tipo='C')
        // ====================================================================

        private async Task CargarAsync()
        {
            var ct = CancellationToken.None;
            await Task.WhenAll(
                CargarXmlAsync(ct),
                CargarPdfAsync(ct),
                CargarAdjuntosAsync(ct)
            ).ConfigureAwait(true);
        }

        /// <summary>
        /// Lista los adjuntos extras del complemento (tipo='C') y los pinta
        /// en el grid del tab "📎 Adjuntos". Mismo patrón que en facturas
        /// pero con DOCTO_CP_ID en vez de DOCTO_CM_ID.
        /// </summary>
        private async Task CargarAdjuntosAsync(CancellationToken ct)
        {
            this.dgvAdjuntos.Rows.Clear();
            try
            {
                var lista = await _api
                    .ListarAdjuntosAsync(_comp.DOCTO_CP_ID, _empresa.Id, "C", ct)
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

                var path = Path.Combine(Path.GetTempPath(),
                    "Adjunto_" + LimpiarParaNombreArchivo(_comp.UUID) + "_"
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
                var cfdi = await _api.ObtenerCfdiXmlAsync(_comp.UUID, "C", ct).ConfigureAwait(true);
                if (cfdi == null || string.IsNullOrEmpty(cfdi.xml))
                {
                    this.sec3Subtitulo.Text = "El portal no devolvió XML para este UUID.";
                    return;
                }

                this.sec3Subtitulo.Text = "UUID " + (_comp.UUID ?? "")
                                       + "  ·  Uso CFDI: " + (cfdi.uso_cfdi ?? "—");

                var formateado = FormatearXml(cfdi.xml);
                this.txtVistaXml.Text = formateado;
                this.btnTabXml.Enabled = true;
                ActualizarHabilitadoExterno();

                try
                {
                    _xmlTempPath = Path.Combine(Path.GetTempPath(),
                        "Complemento_" + LimpiarParaNombreArchivo(_comp.UUID) + ".xml");
                    File.WriteAllText(_xmlTempPath, formateado, new UTF8Encoding(false));
                }
                catch { _xmlTempPath = null; }

                if (string.IsNullOrEmpty(_pdfTempPath))
                    MostrarPestaña(PestañaActiva.Xml);
            }
            catch (Exception ex)
            {
                this.sec3Subtitulo.Text = "No se pudo obtener el CFDI: " + ex.Message;
            }
        }

        private async Task CargarPdfAsync(CancellationToken ct)
        {
            try
            {
                var binario = await _api.ObtenerCfdiPdfAsync(_comp.UUID, "C", ct).ConfigureAwait(true);
                if (binario == null || binario.Length == 0) return;

                _pdfTempPath = Path.Combine(Path.GetTempPath(),
                    "Complemento_" + LimpiarParaNombreArchivo(_comp.UUID) + ".pdf");
                File.WriteAllBytes(_pdfTempPath, binario);
                this.btnTabPdf.Enabled = true;
                ActualizarHabilitadoExterno();

                if (_webViewListo)
                    MostrarPestaña(PestañaActiva.Pdf);
            }
            catch
            {
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
                try { this.webView.CoreWebView2.Navigate(new Uri(_pdfTempPath).AbsoluteUri); } catch { }
            }

            ActualizarHabilitadoExterno();
        }

        private void EstilarTabs()
        {
            EstilarTabIndividual(this.btnTabPdf,      _pestaña == PestañaActiva.Pdf);
            EstilarTabIndividual(this.btnTabXml,      _pestaña == PestañaActiva.Xml);
            EstilarTabIndividual(this.btnTabAdjuntos, _pestaña == PestañaActiva.Adjuntos);
        }

        private void EstilarTabIndividual(Button btn, bool activo)
        {
            btn.BackColor = activo ? Tema.Primary : Color.White;
            btn.ForeColor = activo ? Color.White  : Color.FromArgb(71, 85, 105);
            btn.FlatAppearance.BorderSize = activo ? 0 : 1;
        }

        private void ActualizarHabilitadoExterno()
        {
            // En el tab Adjuntos, cada fila tiene su propio botón de
            // descarga — el global no aplica.
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
            string path, tipo;
            if (_pestaña == PestañaActiva.Pdf)      { path = _pdfTempPath; tipo = "PDF"; }
            else if (_pestaña == PestañaActiva.Xml) { path = _xmlTempPath; tipo = "XML"; }
            else                                    { return; }

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                MessageBox.Show("El archivo " + tipo + " no está disponible.",
                    "No disponible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName        = path,
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el " + tipo + ":\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ====================================================================
        // Aplicar
        // ====================================================================

        private async void btnAplicar_Click(object sender, EventArgs e)
        {
            btnAplicar.Enabled  = false;
            btnCancelar.Enabled = false;
            barProgreso.Visible = true;
            barProgreso.Style   = ProgressBarStyle.Marquee;
            barProgreso.MarqueeAnimationSpeed = 30;

            _cts = new CancellationTokenSource();
            var progreso = new Progress<string>(msg => MostrarEstado(msg, EstadoTipo.Trabajando));

            ResultadoAplicacion r;
            try
            {
                r = await Task.Run(
                    () => _aplicador.AplicarAsync(_empresa, _comp, _usuarioMicrosip, progreso, _cts.Token),
                    _cts.Token);
            }
            catch (OperationCanceledException)
            {
                MostrarEstado("Asociación cancelada por el operador.", EstadoTipo.Error);
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
            bool exito = EsResultadoExitoso(r);

            if (exito)
            {
                ComplementoAplicado = true;
                ResultadoMensaje    = r.mensaje;

                MostrarEstado(
                    "Complemento asociado. " + (r.mensaje ?? ""),
                    EstadoTipo.Exito);

                this.btnCancelar.Text    = "Cerrar";
                this.btnCancelar.Enabled = true;
                this.AcceptButton        = this.btnCancelar;
                this.btnAplicar.Visible  = false;
            }
            else
            {
                MostrarEstado(
                    "No se pudo asociar (bloque " + r.ultimoBloque + "): " + r.mensaje,
                    EstadoTipo.Error);
                RestaurarBotonesParaReintentar();
            }
        }

        /// <summary>
        /// El flujo de complementos termina con COMMIT en el bloque 7 (no 15
        /// como facturas). Lo importante para decidir éxito es:
        ///   - El <c>tipo</c> NO es un código de error de los conocidos.
        ///   - El callback al portal regresó OK (<c>portalMarcado=true</c>).
        /// El número del bloque no se usa aquí — el repositorio lo va
        /// actualizando paso a paso y solo importa el bloque cuando hubo
        /// un fallo intermedio (lo lee el mensaje).
        /// </summary>
        private static bool EsResultadoExitoso(ResultadoAplicacion r)
        {
            if (r == null) return false;
            switch (r.tipo)
            {
                case ResultadoAplicacionTipo.Error:
                case ResultadoAplicacionTipo.ErrorConexion:
                case ResultadoAplicacionTipo.CreditoNoExiste:
                case ResultadoAplicacionTipo.CreditoYaConCfdi:
                    return false;
                default:
                    return r.portalMarcado;
            }
        }

        private void RestaurarBotonesParaReintentar()
        {
            this.btnAplicar.Enabled  = true;
            this.btnCancelar.Enabled = true;
            this.barProgreso.Visible = false;
        }

        // ====================================================================
        // Cerrar / cleanup
        // ====================================================================

        private void btnCancelar_Click(object sender, EventArgs e) => CancelarYCerrar();

        private void CancelarYCerrar()
        {
            if (_cts != null) { try { _cts.Cancel(); } catch { } }
            this.DialogResult = ComplementoAplicado ? DialogResult.OK : DialogResult.Cancel;
            this.Close();
        }

        private void FormAplicarComplemento_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cts != null) { try { _cts.Cancel(); } catch { } }
        }

        private void FormAplicarComplemento_FormClosed(object sender, FormClosedEventArgs e)
        {
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

        private static string SafeString(string s) { return string.IsNullOrEmpty(s) ? "" : s; }

        private static string FormatearFecha(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            DateTime d;
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                return d.ToString("dd / MMM / yyyy", new CultureInfo("es-MX"));
            return raw;
        }

        private static string FormatearMonto(decimal monto, string moneda)
        {
            string m = string.IsNullOrEmpty(moneda) ? "MXN" : moneda;
            return monto.ToString("N2", CultureInfo.InvariantCulture) + " " + m;
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
                    doc.Save(writer);
                return sb.ToString();
            }
            catch
            {
                return xml;
            }
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
