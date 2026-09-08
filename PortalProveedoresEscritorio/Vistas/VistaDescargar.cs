using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Vista del tab "Descargar" del FormPrincipal. Réplica funcional de
    /// <c>F_DESCARGAR</c> del SOAP — el operador filtra facturas y baja
    /// XML/PDF/Adjuntos en una sola operación (por fila o por todas).
    ///
    /// Reusa el endpoint <c>/api/escritorio/facturas-pendientes</c> con
    /// <c>todos_estatus=1</c> — réplica del SOAP <c>SelectDescargar</c>
    /// (services/facturas.php:356-410) que NO filtraba por ESTATUS: el tab
    /// servía para auditar facturas YA aplicadas/rechazadas (por eso muestra
    /// NUM_POLIZA) y filtraba el rango de fechas por FECHA_FACTURA
    /// (F_DESCARGAR.cs:152-153).
    ///
    /// Click derecho en una fila o en el toolbar: opciones de descarga.
    /// Construcción 100% en código.
    /// </summary>
    public sealed class VistaDescargar : UserControl
    {
        private readonly IPortalApi        _api;
        private readonly EmpresaEscritorio _empresa;

        // ---- Toolbar ----
        private Panel          panelToolbar;
        private Label          lblDesde;
        private DateTimePicker dtpDesde;
        private Label          lblHasta;
        private DateTimePicker dtpHasta;
        private Label          lblLimite;
        private NumericUpDown  numLimite;
        private Label          lblProveedor;
        private ComboBox       cbBuscarProveedor;
        private Button         btnConsultar;
        private Button         btnDescargarTodas;
        private Label          lblContador;

        // ---- Grid + menú contextual ----
        private DataGridView    dgv;
        private ContextMenuStrip ctxFila;
        private ToolStripMenuItem miXml;
        private ToolStripMenuItem miPdf;
        private ToolStripMenuItem miAdjuntos;
        private ToolStripMenuItem miTodo;

        public VistaDescargar(IPortalApi api, EmpresaEscritorio empresa)
        {
            _api     = api     ?? throw new ArgumentNullException(nameof(api));
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));

            ConstruirUI();
            this.Load += async (s, e) => { await CargarCatalogosAsync(); await ConsultarAsync(); };
        }

        private void ConstruirUI()
        {
            this.Dock      = DockStyle.Fill;
            this.BackColor = Color.FromArgb(247, 249, 252);

            // ============ Toolbar ============
            panelToolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 104,
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

            lblDesde = new Label
            {
                Location  = new Point(20, 22), Size = new Size(48, 22),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text      = "Desde",
            };
            dtpDesde = new DateTimePicker
            {
                Location = new Point(68, 18), Size = new Size(128, 25),
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today.AddYears(-1),
            };
            lblHasta = new Label
            {
                Location  = new Point(208, 22), Size = new Size(44, 22),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text      = "Hasta",
            };
            dtpHasta = new DateTimePicker
            {
                Location = new Point(252, 18), Size = new Size(128, 25),
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today.AddYears(1),
            };
            lblLimite = new Label
            {
                Location  = new Point(394, 22), Size = new Size(48, 22),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text      = "Límite",
            };
            numLimite = new NumericUpDown
            {
                Location = new Point(442, 18), Size = new Size(70, 25),
                Minimum  = 1, Maximum = 9999, Value = 100,
            };
            // 2ª fila: filtro por proveedor — para no descargar TODOS. Combo
            // vacío = todos los proveedores; elegir uno filtra. Réplica del
            // filtro de proveedor de F_DESCARGAR del SOAP.
            lblProveedor = new Label
            {
                Location  = new Point(20, 66), Size = new Size(66, 22),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text      = "Proveedor",
            };
            cbBuscarProveedor = new ComboBox
            {
                Location      = new Point(86, 62), Size = new Size(430, 25),
                DropDownStyle = ComboBoxStyle.DropDown,
            };
            btnConsultar = new Button
            {
                Location  = new Point(526, 16), Size = new Size(108, 30),
                BackColor = Tema.Primary, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9F),
                Text      = "Consultar", Cursor = Cursors.Hand,
            };
            btnConsultar.FlatAppearance.BorderSize = 0;
            btnConsultar.Click += async (s, e) => await ConsultarAsync();

            btnDescargarTodas = new Button
            {
                Location  = new Point(644, 16), Size = new Size(180, 30),
                BackColor = Color.FromArgb(34, 197, 94), // verde
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9F),
                Text      = "📥  Descargar TODO de TODAS",
                Cursor    = Cursors.Hand,
            };
            btnDescargarTodas.FlatAppearance.BorderSize = 0;
            btnDescargarTodas.Click += async (s, e) => await DescargarLoteAsync(soloFilaSeleccionada: false, tipoDescarga: TipoDescarga.Todo);

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

            panelToolbar.Controls.Add(lblDesde);
            panelToolbar.Controls.Add(dtpDesde);
            panelToolbar.Controls.Add(lblHasta);
            panelToolbar.Controls.Add(dtpHasta);
            panelToolbar.Controls.Add(lblLimite);
            panelToolbar.Controls.Add(numLimite);
            panelToolbar.Controls.Add(lblProveedor);
            panelToolbar.Controls.Add(cbBuscarProveedor);
            panelToolbar.Controls.Add(btnConsultar);
            panelToolbar.Controls.Add(btnDescargarTodas);
            panelToolbar.Controls.Add(lblContador);

            // ============ Grid ============
            dgv = new DataGridView
            {
                Dock                      = DockStyle.Fill,
                BackgroundColor           = Color.White,
                BorderStyle               = BorderStyle.None,
                AllowUserToAddRows        = false,
                AllowUserToDeleteRows     = false,
                ReadOnly                  = true,
                MultiSelect               = false,
                SelectionMode             = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode       = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible         = false,
                AutoGenerateColumns       = false,
                EnableHeadersVisualStyles = false,
                CellBorderStyle           = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor                 = Color.FromArgb(241, 245, 249),
                ColumnHeadersBorderStyle  = DataGridViewHeaderBorderStyle.None,
                RowTemplate               = { Height = 32 },
            };
            dgv.DefaultCellStyle.Font                           = new Font("Segoe UI", 9F);
            dgv.ColumnHeadersDefaultCellStyle.Font              = new Font("Segoe UI Semibold", 9F);
            dgv.ColumnHeadersDefaultCellStyle.BackColor         = Color.FromArgb(247, 249, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor         = Color.FromArgb(71, 85, 105);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor= Color.FromArgb(247, 249, 252);
            dgv.AlternatingRowsDefaultCellStyle.BackColor       = Color.FromArgb(252, 252, 254);

            // Checkbox column — réplica F_DESCARGAR (Media 3): permite
            // que las acciones de descarga procesen solo las filas marcadas.
            var colSeleccionar = new DataGridViewCheckBoxColumn
            {
                Name       = "colSeleccionar",
                HeaderText = "",
                Width      = 30,
                FillWeight = 4,
                ReadOnly   = false,
                Resizable  = DataGridViewTriState.False,
            };
            dgv.Columns.Add(colSeleccionar);

            dgv.Columns.Add(Col("UUID",           "UUID",           260, mono: true, hidden: true));
            dgv.Columns.Add(Col("DOCTO_CM_ID",    "DoctoCM",        0,   hidden: true));
            // PROVEEDOR_ID es necesario para la query de NUM_POLIZA / FECHA_POLIZA
            // (F_DESCARGAR.cs:221 — `fac.PROVEEDOR_ID = @PROVEEDOR`).
            dgv.Columns.Add(Col("PROVEEDOR_ID",   "ProveedorID",    0,   hidden: true));
            dgv.Columns.Add(Col("PROVEEDOR_NOMBRE","Proveedor",     260));
            dgv.Columns.Add(Col("RFC",            "RFC",            120, mono: true));
            dgv.Columns.Add(Col("FOLIO_PROV",     "Folio",          100));
            dgv.Columns.Add(Col("FECHA_FACTURA",  "Fecha factura",  110));
            dgv.Columns.Add(Col("ESTATUS",        "Estatus",        70));
            // Réplica F_DESCARGAR.cs:170-173 — pólizas leídas de Firebird.
            dgv.Columns.Add(Col("NUM_POLIZA",     "Num. Póliza",    100));
            dgv.Columns.Add(Col("FECHA_POLIZA",   "Fecha Póliza",   110));

            // El grid es ReadOnly por default pero la columna checkbox debe
            // permitir click — el resto sigue read-only.
            dgv.ReadOnly = false;
            foreach (DataGridViewColumn col in dgv.Columns)
                if (col.Name != "colSeleccionar") col.ReadOnly = true;

            ctxFila = ConstruirContextMenu();
            dgv.ContextMenuStrip = ctxFila;
            dgv.CellMouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                {
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;
                }
            };

            this.Controls.Add(dgv);          // Fill
            this.Controls.Add(panelToolbar); // Top
        }

        private static DataGridViewTextBoxColumn Col(string name, string header, int width,
            bool mono = false, bool hidden = false)
        {
            var c = new DataGridViewTextBoxColumn
            {
                Name       = name,
                HeaderText = header,
                ReadOnly   = true,
                Visible    = !hidden,
                FillWeight = hidden ? 1 : Math.Max(width, 50),
            };
            if (mono) c.DefaultCellStyle.Font = new Font("Consolas", 9F);
            return c;
        }

        /// <summary>
        /// Menú contextual con 4 acciones de descarga sobre la fila
        /// seleccionada. La acción "Descargar TODO" del toolbar superior
        /// usa el mismo método pero sin filtrar por selección.
        /// </summary>
        private ContextMenuStrip ConstruirContextMenu()
        {
            var menu = new ContextMenuStrip();
            miXml      = new ToolStripMenuItem("📑  Descargar XML");
            miPdf      = new ToolStripMenuItem("📄  Descargar PDF");
            miAdjuntos = new ToolStripMenuItem("📎  Descargar adjuntos");
            miTodo     = new ToolStripMenuItem("📥  Descargar TODO");
            miXml.Click      += async (s, e) => await DescargarLoteAsync(true, TipoDescarga.Xml);
            miPdf.Click      += async (s, e) => await DescargarLoteAsync(true, TipoDescarga.Pdf);
            miAdjuntos.Click += async (s, e) => await DescargarLoteAsync(true, TipoDescarga.Adjuntos);
            miTodo.Click     += async (s, e) => await DescargarLoteAsync(true, TipoDescarga.Todo);
            menu.Items.Add(miXml);
            menu.Items.Add(miPdf);
            menu.Items.Add(miAdjuntos);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(miTodo);
            return menu;
        }

        // ====================================================================
        // Consulta
        // ====================================================================

        /// <summary>
        /// Llena el combo de proveedores para poder filtrar la descarga por un
        /// proveedor (y no traer TODOS). Mismo patrón que VistaFacturas
        /// (ObtenerCatalogosFiltrosAsync). Combo vacío = todos los proveedores.
        /// </summary>
        private async Task CargarCatalogosAsync()
        {
            try
            {
                bool aplicaDir = false;
                try { aplicaDir = await _api.ObtenerAplicaDirAsync(CancellationToken.None).ConfigureAwait(true); }
                catch { /* default false */ }

                var cat = await _api.ObtenerCatalogosFiltrosAsync(
                    _empresa.Id, aplicaDir, "facturas", CancellationToken.None
                ).ConfigureAwait(true);

                cbBuscarProveedor.Items.Clear();
                if (cat != null && cat.proveedores != null)
                    foreach (var p in cat.proveedores) cbBuscarProveedor.Items.Add(p);
            }
            catch
            {
                // Silencioso — sin catálogo el combo queda vacío (= todos).
            }
        }

        private static int IdSeleccionado(ComboBox cb)
        {
            if (cb == null) return 0;
            var item = cb.SelectedItem as PortalProveedoresCore.Modelos.CatalogoFiltroItem;
            return item != null ? item.id : 0;
        }

        private static string TextoLibre(ComboBox cb)
        {
            if (cb == null) return "";
            // Si el texto coincide con el item elegido, usamos el id (no texto libre).
            if (cb.SelectedItem != null
                && string.Equals(cb.SelectedItem.ToString(), cb.Text, StringComparison.OrdinalIgnoreCase))
                return "";
            return (cb.Text ?? "").Trim();
        }

        private async Task ConsultarAsync()
        {
            btnConsultar.Enabled = false;
            lblContador.Text     = "Cargando…";

            try
            {
                var filtro = new FiltroFacturasEscritorio
                {
                    EmpIdMsp = _empresa.Id,
                    Limit    = (int) numLimite.Value,
                    Desde    = dtpDesde.Value.Date,
                    Hasta    = dtpHasta.Value.Date,
                    // Filtro por proveedor: id si se eligió del combo, o texto
                    // libre (LIKE) si el operador tecleó. Vacío = todos.
                    ProveedorId     = IdSeleccionado(cbBuscarProveedor),
                    NombreProveedor = TextoLibre(cbBuscarProveedor),
                    // Réplica SelectDescargar (facturas.php:356-410): el tab
                    // Descargar legacy incluía facturas ya aplicadas y
                    // rechazadas — sin este flag solo se verían las ESTATUS='S'.
                    TodosEstatus = true,
                };
                var resp = await _api.ObtenerFacturasPendientesEscritorioAsync(filtro, CancellationToken.None)
                    .ConfigureAwait(true);

                dgv.Rows.Clear();
                int n = 0;
                if (resp != null && resp.facturas != null)
                {
                    foreach (var f in resp.facturas)
                    {
                        // Orden: checkbox, UUID, DOCTO_CM_ID, PROVEEDOR_ID,
                        // proveedor, RFC, folio, fecha, estatus, NUM_POLIZA,
                        // FECHA_POLIZA.
                        dgv.Rows.Add(false, f.UUID ?? "", f.DOCTO_CM_ID, f.PROVEEDOR_ID,
                                     f.PROVEEDOR_NOMBRE ?? "", f.RFC ?? "",
                                     f.FOLIO_PROV ?? "", FormatearFecha(f.FECHA_FACTURA),
                                     f.ESTATUS ?? "", "", "");
                        n++;
                    }
                }
                lblContador.Text = n == 0 ? "Sin facturas en el rango" : (n + " factura(s)");

                // Enriquecer con NUM_POLIZA / FECHA_POLIZA desde Firebird —
                // réplica F_DESCARGAR.cs:195-231. Si la conexión falla, se
                // queda vacío.
                await EnriquecerConPolizasAsync(CancellationToken.None).ConfigureAwait(true);
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

        /// <summary>
        /// Devuelve las filas con checkbox marcado. Si ninguna está marcada,
        /// la lista viene vacía.
        /// </summary>
        private List<DataGridViewRow> ObtenerFilasChequeadas()
        {
            var resultado = new List<DataGridViewRow>();
            foreach (DataGridViewRow r in dgv.Rows)
            {
                var celda = r.Cells["colSeleccionar"].Value;
                bool marcada = celda is bool ? (bool)celda : false;
                if (marcada) resultado.Add(r);
            }
            return resultado;
        }

        /// <summary>
        /// Recorre las filas del grid y consulta Firebird en bloque para
        /// NUM_POLIZA / FECHA_POLIZA. Si la consulta falla o devuelve vacío,
        /// las columnas se quedan en cadena vacía. Réplica F_DESCARGAR.cs:195-231.
        /// </summary>
        private async Task EnriquecerConPolizasAsync(CancellationToken ct)
        {
            if (dgv.Rows.Count == 0) return;

            var pares = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                string folio = (dgv.Rows[i].Cells["FOLIO_PROV"].Value ?? "").ToString();
                int prov     = ParseInt(dgv.Rows[i].Cells["PROVEEDOR_ID"].Value);
                if (!string.IsNullOrEmpty(folio) && prov > 0)
                    pares.Add(new KeyValuePair<string, int>(folio, prov));
            }
            if (pares.Count == 0) return;

            try
            {
                var svc  = new Servicios.PolizasMicrosip();
                var dict = await svc.ObtenerPolizasAsync(_empresa.NombreCorto, pares, ct)
                    .ConfigureAwait(true);
                if (dict == null || dict.Count == 0) return;

                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    string folio = (dgv.Rows[i].Cells["FOLIO_PROV"].Value ?? "").ToString();
                    int prov     = ParseInt(dgv.Rows[i].Cells["PROVEEDOR_ID"].Value);
                    Servicios.PolizasMicrosip.DatoPoliza p;
                    if (dict.TryGetValue(Servicios.PolizasMicrosip.ClaveDic(folio, prov), out p) && p != null)
                    {
                        dgv.Rows[i].Cells["NUM_POLIZA"].Value   = p.NumPoliza   ?? "";
                        dgv.Rows[i].Cells["FECHA_POLIZA"].Value = p.FechaPoliza ?? "";
                    }
                }
            }
            catch
            {
                // Sin pólizas — no se considera error.
            }
        }

        // ====================================================================
        // Descarga por lote
        // ====================================================================

        private enum TipoDescarga { Xml, Pdf, Adjuntos, Todo }

        /// <summary>
        /// Descarga los archivos seleccionados a una carpeta elegida por el
        /// operador. Si <paramref name="soloFilaSeleccionada"/> es false,
        /// procesa todas las filas del grid. Crea una subcarpeta por UUID
        /// para mantener orden cuando hay muchas facturas.
        /// </summary>
        private async Task DescargarLoteAsync(bool soloFilaSeleccionada, TipoDescarga tipoDescarga)
        {
            var filas = new List<DataGridViewRow>();

            // Réplica F_DESCARGAR.cs:386-566 — el SOAP procesa las filas
            // con checkbox SELECT marcado cuando hay alguna. Si no, mantiene
            // el comportamiento legacy (fila seleccionada o todas).
            var chequeadas = ObtenerFilasChequeadas();
            if (chequeadas.Count > 0)
            {
                filas.AddRange(chequeadas);
            }
            else if (soloFilaSeleccionada)
            {
                if (dgv.SelectedRows.Count == 0) return;
                filas.Add(dgv.SelectedRows[0]);
            }
            else
            {
                if (dgv.Rows.Count == 0)
                {
                    MessageBox.Show("Sin filas en el grid.", "Nada que descargar",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                foreach (DataGridViewRow r in dgv.Rows) filas.Add(r);
            }

            string carpetaRaiz;
            using (var fbd = new FolderBrowserDialog
            {
                Description  = "Selecciona la carpeta donde guardar los archivos",
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            })
            {
                if (fbd.ShowDialog(this.FindForm()) != DialogResult.OK) return;
                carpetaRaiz = fbd.SelectedPath;
            }

            btnConsultar.Enabled      = false;
            btnDescargarTodas.Enabled = false;
            int xmlOk = 0, pdfOk = 0, adjOk = 0, errores = 0;
            var ct = CancellationToken.None;

            try
            {
                int idx = 0;
                foreach (var row in filas)
                {
                    idx++;
                    string uuid       = (row.Cells["UUID"].Value ?? "").ToString();
                    int    doctoCmId  = ParseInt(row.Cells["DOCTO_CM_ID"].Value);
                    string proveedor  = (row.Cells["PROVEEDOR_NOMBRE"].Value ?? "").ToString().Trim();
                    if (string.IsNullOrEmpty(uuid)) continue;

                    lblContador.Text = "Descargando " + idx + " / " + filas.Count + "…";

                    // Réplica EXACTA de F_DESCARGAR del SOAP nuevo (D:):
                    //  - "Descargar TODO" (XML+PDF+adjuntos juntos) → AGRUPA por
                    //    cliente: <raíz>/<proveedor>/<uuid>/ (GuardarArchivos CON
                    //    'proveedor', F_DESCARGAR.cs:673-700 de D:).
                    //  - "solo XML" / "solo PDF" individuales → SUELTOS en la raíz
                    //    elegida (GuardarArchivos SIN 'proveedor', :463-610).
                    // Los adjuntos individuales se agrupan igual que TODO (D: solo
                    // baja adjuntos dentro de "TODO"; además sus nombres no son
                    // únicos → agrupar evita que se pisen entre facturas).
                    bool agrupar = tipoDescarga == TipoDescarga.Todo
                                || tipoDescarga == TipoDescarga.Adjuntos;
                    var sub = (agrupar && !string.IsNullOrEmpty(proveedor))
                        ? Path.Combine(carpetaRaiz, LimpiarNombre(proveedor), LimpiarNombre(uuid))
                        : carpetaRaiz;
                    Directory.CreateDirectory(sub);

                    if (tipoDescarga == TipoDescarga.Xml || tipoDescarga == TipoDescarga.Todo)
                    {
                        try
                        {
                            var cfdi = await _api.ObtenerCfdiXmlAsync(uuid, "F", ct).ConfigureAwait(true);
                            if (cfdi != null && !string.IsNullOrEmpty(cfdi.xml))
                            {
                                File.WriteAllText(Path.Combine(sub, uuid + ".XML"),
                                    cfdi.xml, new UTF8Encoding(false));
                                xmlOk++;
                            }
                        }
                        catch { errores++; }
                    }

                    if (tipoDescarga == TipoDescarga.Pdf || tipoDescarga == TipoDescarga.Todo)
                    {
                        try
                        {
                            var pdf = await _api.ObtenerCfdiPdfAsync(uuid, "F", ct).ConfigureAwait(true);
                            if (pdf != null && pdf.Length > 0)
                            {
                                File.WriteAllBytes(Path.Combine(sub, uuid + ".pdf"), pdf);
                                pdfOk++;
                            }
                        }
                        catch { errores++; }
                    }

                    if (tipoDescarga == TipoDescarga.Adjuntos || tipoDescarga == TipoDescarga.Todo)
                    {
                        try
                        {
                            var lista = await _api.ListarAdjuntosAsync(doctoCmId, _empresa.Id, "F", ct)
                                .ConfigureAwait(true);
                            if (lista != null)
                            {
                                foreach (var a in lista)
                                {
                                    var bin = await _api.DescargarAdjuntoAsync(a.id, ct).ConfigureAwait(true);
                                    if (bin == null || bin.Length == 0) continue;
                                    var nombre = string.IsNullOrEmpty(a.nombre_original)
                                                    ? a.nombre_archivo
                                                    : a.nombre_original;
                                    File.WriteAllBytes(Path.Combine(sub, LimpiarNombre(nombre)), bin);
                                    adjOk++;
                                }
                            }
                        }
                        catch { errores++; }
                    }
                }

                lblContador.Text = filas.Count + " factura(s) procesada(s)";

                var resumen = "Descarga terminada en " + carpetaRaiz + "\n\n"
                            + "XML:      " + xmlOk + "\n"
                            + "PDF:      " + pdfOk + "\n"
                            + "Adjuntos: " + adjOk + "\n"
                            + (errores > 0 ? "Errores:  " + errores : "");
                MessageBox.Show(resumen, "Descarga completada",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error inesperado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnConsultar.Enabled      = true;
                btnDescargarTodas.Enabled = true;
            }
        }

        // ====================================================================
        // Helpers
        // ====================================================================

        private static int ParseInt(object v)
        {
            int x;
            return int.TryParse((v ?? "").ToString(), NumberStyles.Any,
                                CultureInfo.InvariantCulture, out x) ? x : 0;
        }

        private static string FormatearFecha(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            DateTime d;
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out d))
                return d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            return raw;
        }

        private static string LimpiarNombre(string s)
        {
            if (string.IsNullOrEmpty(s)) return "archivo";
            var inv = Path.GetInvalidFileNameChars();
            var sb  = new StringBuilder(s.Length);
            foreach (var ch in s) sb.Append(Array.IndexOf(inv, ch) >= 0 ? '_' : ch);
            return sb.ToString();
        }
    }
}
