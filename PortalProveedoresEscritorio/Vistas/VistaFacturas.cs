using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Repositorios;
using PortalProveedoresCore.Servicios;
using PortalProveedoresEscritorio.Formularios;
using PortalProveedoresEscritorio.Servicios;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio.Vistas
{
    /// <summary>
    /// Vista del tab "Facturas" del FormPrincipal.
    ///
    /// **UI**: moderna, paleta del Tema dinámico. Toolbar horizontal en la
    /// parte superior con filtros (Desde / Hasta / Solo por vencer / Límite
    /// / Consultar) más contador a la derecha. Click derecho en la toolbar
    /// abre "Personalizar vista" para mostrar/ocultar columnas, con
    /// persistencia por usuario en XML bajo <c>%LocalAppData%</c>.
    ///
    /// **Behavior**: pixel-near al SOAP legacy (F_FACTURAS.cs/.Designer.cs):
    /// <list type="bullet">
    ///   <item>27 columnas con los mismos <c>Name</c> y <c>HeaderText</c>
    ///         literales del SOAP (incluyendo "Folio Recepcion" y "Dias de
    ///         atraso" sin tildes).</item>
    ///   <item>8 visibles por default — el operador puede mostrar/ocultar
    ///         las que no sean IDs internos.</item>
    ///   <item>Menú contextual con 6 items en el mismo orden y mismos
    ///         textos (con ampersands de teclado) que el SOAP, item 6 oculto
    ///         por default.</item>
    ///   <item>Validación de permiso Microsip <c>831</c> antes de abrir
    ///         FormAplicarFactura (réplica F_FACTURAS.cs:311).</item>
    ///   <item>Textos literales del SOAP en los MessageBox de
    ///         Descargar/Descartar.</item>
    /// </list>
    ///
    /// El proyecto NUNCA usa los web services SOAP — los catálogos, listados
    /// y marcaciones pasan por la API REST CI4 vía <see cref="IPortalApi"/>.
    /// </summary>
    public sealed class VistaFacturas : UserControl
    {
        // --- Inyectado ---
        private readonly IPortalApi        _api;
        private readonly EmpresaEscritorio _empresa;
        private readonly string            _usuario;
        private readonly string            _password;
        private readonly PermisosMicrosip  _permisos;

        // --- Toolbar (moderno) ---
        private Panel          panelToolbar;
        private Label          lblToolbarDesde;
        private DateTimePicker dtpDesde;
        private Label          lblToolbarHasta;
        private DateTimePicker dtpHasta;
        private CheckBox       chkPorVencer;
        private Label          lblToolbarLimite;
        private NumericUpDown  numLimite;
        private Label          lblToolbarProveedor;
        private ComboBox       cbBuscarProveedor;
        private Label          lblToolbarAlmacen;
        private ComboBox       cbBuscarAlmacen;
        private Button         btnConsultar;
        private Label          lblContador;

        // --- Click derecho en toolbar → Personalizar vista ---
        private ContextMenuStrip ctxToolbar;

        // --- Banner contextual (APLICA_DIR) ---
        private Panel panelBanner;
        private Label lblBanner;

        // --- Grid + menú contextual (estilo SOAP) ---
        private DataGridView     dgvFacturas;
        private ContextMenuStrip ctxGrid;
        private ToolStripMenuItem miAplicar;
        private ToolStripMenuItem miVistaPrevia;
        private ToolStripMenuItem miDescarga;
        private ToolStripMenuItem miRechaza;
        private ToolStripMenuItem miDescartar;
        private ToolStripMenuItem miSoloCambiar;

        // Path en %LocalAppData% donde se guardan las preferencias del operador.
        private const string PrefsSubsection = @"Vistas\VistaFacturas\Columnas";

        public VistaFacturas(IPortalApi api, EmpresaEscritorio empresa,
                             string usuario, string password)
        {
            _api      = api      ?? throw new ArgumentNullException(nameof(api));
            _empresa  = empresa  ?? throw new ArgumentNullException(nameof(empresa));
            _usuario  = usuario  ?? "";
            _password = password ?? "";
            _permisos = new PermisosMicrosip();

            ConstruirUI();
            this.Load += async (s, e) =>
            {
                await CargarCatalogosAsync();
                await ConsultarAsync();
            };
        }

        /// <summary>
        /// Carga la lista de proveedores y almacenes para alimentar los
        /// ComboBox AutoComplete. Best-effort: si falla, los combos quedan
        /// vacíos y el operador puede escribir texto libre (filtro LIKE).
        /// </summary>
        private async Task CargarCatalogosAsync()
        {
            try
            {
                // Réplica F_FACTURAS.cs:55-82 — el SOAP llamaba
                // GET_APLICA_FACTURAS antes de LIST_PROVEEDORES para que
                // el catálogo de proveedores se filtre según el modo.
                bool aplicaDir = false;
                try
                {
                    aplicaDir = await _api.ObtenerAplicaDirAsync(CancellationToken.None)
                        .ConfigureAwait(true);
                }
                catch { /* default false */ }

                var cat = await _api.ObtenerCatalogosFiltrosAsync(
                    _empresa.Id, aplicaDir, "facturas", CancellationToken.None
                ).ConfigureAwait(true);

                cbBuscarProveedor.Items.Clear();
                if (cat.proveedores != null)
                    foreach (var p in cat.proveedores) cbBuscarProveedor.Items.Add(p);

                cbBuscarAlmacen.Items.Clear();
                if (cat.almacenes != null)
                    foreach (var a in cat.almacenes) cbBuscarAlmacen.Items.Add(a);
            }
            catch
            {
                // Silencioso — sin catálogos los combos quedan vacíos.
            }
        }

        // ====================================================================
        // Layout
        // ====================================================================

        private void ConstruirUI()
        {
            this.Dock      = DockStyle.Fill;
            this.BackColor = Color.FromArgb(247, 249, 252);
            this.Padding   = new Padding(0);

            dgvFacturas  = ConstruirGrid();
            AplicarPreferenciasColumnas();
            panelBanner  = ConstruirBanner();
            panelToolbar = ConstruirToolbar();
            ctxGrid      = ConstruirMenuContextualGrid();
            ctxToolbar   = ConstruirMenuContextualToolbar();

            dgvFacturas.ContextMenuStrip = ctxGrid;
            dgvFacturas.CellMouseDown   += dgvFacturas_CellMouseDown;
            dgvFacturas.CellDoubleClick += dgvFacturas_CellDoubleClick;
            panelToolbar.ContextMenuStrip= ctxToolbar;

            this.Controls.Add(dgvFacturas);    // Fill
            this.Controls.Add(panelBanner);    // Top
            this.Controls.Add(panelToolbar);   // Top
        }

        /// <summary>
        /// Después de configurar las columnas, restaura la visibilidad
        /// guardada en XML por usuario. Las columnas internas (IDs, PDF, XML,
        /// etc.) se quedan siempre invisibles (el operador NO las puede
        /// mostrar — son data hidden para que los handlers del menú las lean).
        /// </summary>
        private void AplicarPreferenciasColumnas()
        {
            foreach (DataGridViewColumn col in dgvFacturas.Columns)
            {
                if (!col.Visible) continue;
                col.Visible = PreferenciasUsuario.LeerBool(
                    PrefsSubsection, col.Name, valorDefault: true);
            }
        }

        /// <summary>
        /// Toolbar moderna horizontal: filtros + Consultar + contador.
        /// Fondo claro, paleta del Tema (border-bottom Primary). Sin
        /// GroupBoxes pesados — la versión clásica SOAP queda solo en el
        /// behavior (textos, validaciones, menú contextual).
        /// </summary>
        private Panel ConstruirToolbar()
        {
            // Toolbar de 2 filas (alta 104px), grupos lógicos con espaciado
            // consistente (4px label→control, 24px entre grupos):
            //   Fila 1: fechas (Desde+Hasta) · Solo por vencer · Límite,
            //           contador anclado a la derecha.
            //   Fila 2: combos (Proveedor ancho + Almacén) · botón Consultar
            //           al final.
            // IMPORTANTE: p.Width se fija ANTES de agregar hijos con
            // Anchor=Right — bug conocido de Anchor+Dock (el offset derecho
            // se calcula con el Width vigente al momento del Add).
            var p = new Panel
            {
                Dock      = DockStyle.Top,
                Width     = 1200,
                Height    = 104,
                BackColor = Color.White,
                Padding   = new Padding(20, 10, 20, 10),
            };
            p.Paint += (s, e) =>
            {
                using (var pen = new Pen(Tema.Primary, 2))
                    e.Graphics.DrawLine(pen, 0, p.Height - 1, p.Width, p.Height - 1);
            };

            // ============ Fila 1: fechas + por vencer + límite ============
            lblToolbarDesde = new Label
            {
                Location  = new Point(20, 20),
                Size      = new Size(48, 22),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "Desde",
            };
            dtpDesde = new DateTimePicker
            {
                Location = new Point(70, 18),
                Size     = new Size(130, 25),
                Format   = DateTimePickerFormat.Short,
                // Default amplio (un año atrás) para no esconder pendientes
                // históricos. Una factura puede quedar pendiente meses si
                // no se aplica al ciclo.
                Value    = DateTime.Today.AddYears(-1),
            };

            lblToolbarHasta = new Label
            {
                Location  = new Point(224, 20),
                Size      = new Size(44, 22),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "Hasta",
            };
            dtpHasta = new DateTimePicker
            {
                Location = new Point(270, 18),
                Size     = new Size(130, 25),
                Format   = DateTimePickerFormat.Short,
                // Un año adelante por si la FECHA_PAGO sugerida es futura.
                Value    = DateTime.Today.AddYears(1),
            };

            chkPorVencer = new CheckBox
            {
                Location  = new Point(424, 19),
                Size      = new Size(140, 24),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(51, 65, 85),
                Text      = "Solo por vencer",
                Checked   = false,
            };

            lblToolbarLimite = new Label
            {
                Location  = new Point(588, 20),
                Size      = new Size(48, 22),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "Límite",
            };
            numLimite = new NumericUpDown
            {
                Location  = new Point(638, 18),
                Size      = new Size(64, 25),
                Minimum   = 1,
                Maximum   = 9999,
                Value     = 100,
            };

            // Contador: arranca DESPUÉS del último filtro de la fila 1 y se
            // estira hasta el borde derecho (Anchor Left+Right) — así nunca
            // se encima con el Límite aunque la ventana sea angosta.
            lblContador = new Label
            {
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location  = new Point(712, 20),
                Size      = new Size(p.Width - 712 - 20, 22),
                Font      = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.MiddleRight,
                Text      = "",
            };

            // ============ Fila 2: Proveedor + Almacén + Consultar ============
            // ComboBox con AutoCompleteMode=SuggestAppend: el operador
            // escribe y se filtra la lista del catálogo. Equivale al
            // cbProveedor/cbUnidadNegocio del SOAP pero con búsqueda
            // incremental (funciona aunque haya miles de proveedores).
            lblToolbarProveedor = new Label
            {
                Location  = new Point(20, 62),
                Size      = new Size(72, 22),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "Proveedor",
            };
            cbBuscarProveedor = new ComboBox
            {
                // 360px — las razones sociales completas son largas y antes
                // (280px) se cortaban.
                Location           = new Point(94, 60),
                Size               = new Size(360, 25),
                Font               = new Font("Segoe UI", 9.5F),
                DropDownStyle      = ComboBoxStyle.DropDown,
                AutoCompleteMode   = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                FlatStyle          = FlatStyle.Flat,
            };

            lblToolbarAlmacen = new Label
            {
                Location  = new Point(478, 62),
                Size      = new Size(64, 22),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "Almacén",
            };
            cbBuscarAlmacen = new ComboBox
            {
                Location           = new Point(544, 60),
                Size               = new Size(230, 25),
                Font               = new Font("Segoe UI", 9.5F),
                DropDownStyle      = ComboBoxStyle.DropDown,
                AutoCompleteMode   = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                FlatStyle          = FlatStyle.Flat,
            };

            btnConsultar = new Button
            {
                Location  = new Point(798, 58),
                Size      = new Size(130, 30),
                BackColor = Tema.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9F),
                Text      = "🔍  Consultar",
                Cursor    = Cursors.Hand,
            };
            btnConsultar.FlatAppearance.BorderSize = 0;
            btnConsultar.Click += async (s, e) => await ConsultarAsync();

            p.Controls.Add(lblToolbarDesde);
            p.Controls.Add(dtpDesde);
            p.Controls.Add(lblToolbarHasta);
            p.Controls.Add(dtpHasta);
            p.Controls.Add(chkPorVencer);
            p.Controls.Add(lblToolbarLimite);
            p.Controls.Add(numLimite);
            p.Controls.Add(lblToolbarProveedor);
            p.Controls.Add(cbBuscarProveedor);
            p.Controls.Add(lblToolbarAlmacen);
            p.Controls.Add(cbBuscarAlmacen);
            p.Controls.Add(btnConsultar);
            p.Controls.Add(lblContador);
            return p;
        }

        /// <summary>
        /// Devuelve el ID seleccionado de un ComboBox de catálogo. 0 si el
        /// operador escribió texto libre sin elegir un item de la lista
        /// (caso en el que el filtro server-side lo recibe por nombre LIKE).
        /// </summary>
        private static int IdSeleccionado(ComboBox cb)
        {
            if (cb == null) return 0;
            var item = cb.SelectedItem as PortalProveedoresCore.Modelos.CatalogoFiltroItem;
            return item != null ? item.id : 0;
        }

        /// <summary>
        /// Devuelve el texto crudo del ComboBox para usarse como filtro
        /// LIKE %X% cuando el operador no eligió un item de la lista.
        /// </summary>
        private static string TextoLibre(ComboBox cb)
        {
            if (cb == null) return "";
            // Si el item seleccionado coincide con el texto, NO mandamos
            // texto libre — usamos el id.
            if (cb.SelectedItem != null
                && string.Equals(cb.SelectedItem.ToString(), cb.Text,
                                 StringComparison.OrdinalIgnoreCase))
                return "";
            return (cb.Text ?? "").Trim();
        }

        private Panel ConstruirBanner()
        {
            var p = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 0,
                BackColor = Color.FromArgb(254, 243, 199),
                Padding   = new Padding(16, 10, 16, 10),
                Visible   = false,
            };
            lblBanner = new Label
            {
                Dock      = DockStyle.Fill,
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(146, 64, 14),
                TextAlign = ContentAlignment.MiddleLeft,
                Text      = "",
            };
            p.Controls.Add(lblBanner);
            return p;
        }

        /// <summary>
        /// 27 columnas en el orden exacto del SOAP (F_FACTURAS.Designer:511-704).
        /// Mismos <c>Name</c> y <c>HeaderText</c>. Por default 8 visibles —
        /// el operador puede mostrar/ocultar el resto desde "Personalizar
        /// vista". Las columnas internas (IDs, PDF, XML, UUID, etc.) se
        /// quedan siempre invisibles porque solo sirven para que los
        /// handlers del menú las lean (igual que el SOAP).
        /// </summary>
        private DataGridView ConstruirGrid()
        {
            var dgv = new DataGridView
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
                AutoSizeColumnsMode        = DataGridViewAutoSizeColumnsMode.None,
                ScrollBars                 = ScrollBars.Both,
                RowHeadersVisible          = false,
                AutoGenerateColumns        = false,
                ColumnHeadersHeightSizeMode= DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                EnableHeadersVisualStyles  = false,
            };
            dgv.DefaultCellStyle.Font          = new Font("Segoe UI", 9F);
            dgv.ColumnHeadersDefaultCellStyle.Font           = new Font("Segoe UI Semibold", 9F);
            dgv.ColumnHeadersDefaultCellStyle.BackColor      = Color.FromArgb(247, 249, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor      = Color.FromArgb(51, 65, 85);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 249, 252);
            dgv.AlternatingRowsDefaultCellStyle.BackColor    = Color.FromArgb(252, 252, 254);
            dgv.GridColor                                    = Color.FromArgb(230, 234, 240);

            // === 27 columnas, mismo orden y mismos Name/HeaderText que el SOAP ===
            // Internas (siempre ocultas):
            dgv.Columns.Add(Col("DOCTO_CM_IDSQL",   "DOCTO_CM_IDSQL",   0,   visible: false));
            dgv.Columns.Add(Col("MONEDA_ID",       "MONEDA_ID",        0,   visible: false));
            dgv.Columns.Add(Col("ALMACEN_FK_MSP",  "ALMACEN_FK_MSP",   0,   visible: false));
            dgv.Columns.Add(Col("GLOBAL",          "GLOBAL",           0,   visible: false));
            dgv.Columns.Add(Col("Docto_cm_id_rec", "Docto_cm_id_rec",  0,   visible: false));
            dgv.Columns.Add(Col("RECEPCION_ID_MS", "RECEPCION_ID_MS",  0,   visible: false));
            dgv.Columns.Add(Col("DOCTO_CM_ID",     "DOCTO_CM_ID",      0,   visible: false));
            dgv.Columns.Add(Col("PROVEEDOR_FK",    "PROVEEDOR FK",     0,   visible: false));
            dgv.Columns.Add(Col("FOLIO_RECEPCION", "Folio Recepcion",  0,   visible: false));
            dgv.Columns.Add(Col("RECEP",           "RECEP",            0,   visible: false));
            dgv.Columns.Add(Col("VERSION",         "Version",          0,   visible: false));
            dgv.Columns.Add(Col("FECHA_FACTURA",   "FECHA_FACTURA",    0,   visible: false));
            dgv.Columns.Add(Col("PDF",             "PDF",              0,   visible: false));
            dgv.Columns.Add(Col("XML",             "XML",              0,   visible: false));
            dgv.Columns.Add(Col("TIPO_CAMBIO",     "TIPO_CAMBIO",      0,   visible: false));

            // Visibles por default + togglables vía "Personalizar vista":
            dgv.Columns.Add(Col("NOMBRE",          "Nombre",            260));
            dgv.Columns.Add(Col("FOLIO_PROVEEDOR", "Factura",           110));
            dgv.Columns.Add(Col("FOLIO_RECEPCION1","Recepción",         110));
            dgv.Columns.Add(Col("RECIBIDA",        "Recibida",          110));
            dgv.Columns.Add(Col("FECHA",           "Fecha estimada de pago", 150));
            dgv.Columns.Add(Col("ATRASO",          "Dias de atraso",    110));
            dgv.Columns.Add(ColImporte("IMPORTE",  "Importe",           130));
            dgv.Columns.Add(Col("UNIDAD_DE_NEGOCIO","Unidad de negocio",200));

            // Visibles por default pero con valor del DTO (no del SOAP), togglables:
            dgv.Columns.Add(Col("RFC",             "RFC",               110));
            dgv.Columns.Add(Col("MONEDA_SIMBOLO",  "Moneda",            80));
            dgv.Columns.Add(Col("UUID",            "UUID",              260));
            dgv.Columns.Add(ColImporte("RETENCIONES","Retenciones",     110));

            return dgv;
        }

        private static DataGridViewTextBoxColumn Col(string name, string header, int width, bool visible = true)
        {
            return new DataGridViewTextBoxColumn
            {
                Name         = name,
                HeaderText   = header,
                ReadOnly     = true,
                Visible      = visible,
                Width        = visible ? width : 5,
                MinimumWidth = 5,
            };
        }

        private static DataGridViewTextBoxColumn ColImporte(string name, string header, int width)
        {
            var c = Col(name, header, width);
            c.DefaultCellStyle.Format    = "N2";
            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            return c;
        }

        /// <summary>
        /// Réplica funcional del menú contextual del SOAP
        /// (F_FACTURAS.Designer:457-509): 6 items con sus ampersands de
        /// teclado, item 6 oculto por default, item 1 deshabilitado hasta
        /// que haya filas (F_FACTURAS.cs:297-306).
        /// </summary>
        private ContextMenuStrip ConstruirMenuContextualGrid()
        {
            var menu = new ContextMenuStrip();

            // Emojis decorativos (modernos) + textos literales SOAP con sus
            // ampersands de teclado (Alt+P/V/D/R/E/S).
            miAplicar     = new ToolStripMenuItem("⚙  A&plicar compra en Microsip") { Enabled = false };
            miVistaPrevia = new ToolStripMenuItem("👁  &Vista previa de archivos");
            miDescarga    = new ToolStripMenuItem("📥  &Descarga archivos");
            miRechaza     = new ToolStripMenuItem("✉  &Rechaza factura y envía correo");
            miDescartar   = new ToolStripMenuItem("🗑  D&escartar factura");
            miSoloCambiar = new ToolStripMenuItem("🔄  &Solo cambiar estatus portal") { Visible = false };

            miAplicar.Click     += accion_AplicarMicrosip;
            miVistaPrevia.Click += accion_VistaPrevia;
            miDescarga.Click    += accion_DescargarArchivos;
            miRechaza.Click     += accion_RechazarConCorreo;
            miDescartar.Click   += accion_Descartar;
            miSoloCambiar.Click += accion_SoloCambiarEstatus;

            menu.Items.Add(miAplicar);
            menu.Items.Add(miVistaPrevia);
            menu.Items.Add(miDescarga);
            menu.Items.Add(miRechaza);
            menu.Items.Add(miDescartar);
            menu.Items.Add(miSoloCambiar);

            menu.Opened += (s, e) =>
            {
                bool hay = dgvFacturas.Rows.Count > 0;
                miAplicar.Enabled     = hay;
                miVistaPrevia.Enabled = hay;
                miDescarga.Enabled    = hay;
                miRechaza.Enabled     = hay;
                miDescartar.Enabled   = hay;
            };

            return menu;
        }

        /// <summary>
        /// Click derecho en la toolbar → submenú "Personalizar vista" con
        /// un toggle por columna togglable. Las columnas internas (IDs,
        /// PDF, XML, etc) no aparecen aquí — son data hidden permanente.
        ///
        /// Se persisten en XML por usuario en
        /// <c>%LocalAppData%\SOTI\PortalProveedoresEscritorio\Vistas\VistaFacturas\Columnas.xml</c>.
        /// </summary>
        private ContextMenuStrip ConstruirMenuContextualToolbar()
        {
            var menu = new ContextMenuStrip();
            var miPersonalizar = new ToolStripMenuItem("⚙  Personalizar vista");

            // Mantener el submenú abierto al togglear items — el operador
            // suele cambiar varias columnas en una sola interacción.
            miPersonalizar.DropDown.Closing += (s, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true;
            };

            foreach (DataGridViewColumn col in dgvFacturas.Columns)
            {
                if (EsColumnaInterna(col.Name)) continue;

                var item = new ToolStripMenuItem(col.HeaderText)
                {
                    CheckOnClick = true,
                    Checked      = col.Visible,
                    Tag          = col.Name,
                };
                item.CheckedChanged += MenuPersonalizar_ItemToggled;
                miPersonalizar.DropDownItems.Add(item);
            }

            menu.Items.Add(miPersonalizar);
            return menu;
        }

        /// <summary>
        /// Devuelve true si la columna es de uso interno (IDs, fechas crudas,
        /// PDF/XML/UUID en algunos casos) — esas no se exponen en el menú
        /// porque solo sirven para alimentar los handlers de las acciones.
        /// </summary>
        private static bool EsColumnaInterna(string nombre)
        {
            switch (nombre)
            {
                case "DOCTO_CM_IDSQL":
                case "MONEDA_ID":
                case "ALMACEN_FK_MSP":
                case "GLOBAL":
                case "Docto_cm_id_rec":
                case "RECEPCION_ID_MS":
                case "DOCTO_CM_ID":
                case "PROVEEDOR_FK":
                case "FOLIO_RECEPCION":
                case "RECEP":
                case "VERSION":
                case "FECHA_FACTURA":
                case "PDF":
                case "XML":
                case "TIPO_CAMBIO":
                    return true;
                default:
                    return false;
            }
        }

        private void MenuPersonalizar_ItemToggled(object sender, EventArgs e)
        {
            var mi = (ToolStripMenuItem) sender;
            var nombreColumna = (string) mi.Tag;
            if (!dgvFacturas.Columns.Contains(nombreColumna)) return;
            dgvFacturas.Columns[nombreColumna].Visible = mi.Checked;
            PreferenciasUsuario.EscribirBool(PrefsSubsection, nombreColumna, mi.Checked);
        }

        // ====================================================================
        // Consulta
        // ====================================================================

        private async Task ConsultarAsync()
        {
            btnConsultar.Enabled = false;
            lblContador.Text     = "Cargando…";

            try
            {
                var filtro = new FiltroFacturasEscritorio
                {
                    EmpIdMsp        = _empresa.Id,
                    Limit           = (int) numLimite.Value,
                    Desde           = dtpDesde.Value.Date,
                    Hasta           = dtpHasta.Value.Date,
                    SoloPorVencer   = chkPorVencer.Checked,
                    ProveedorId     = IdSeleccionado(cbBuscarProveedor),
                    AlmacenId       = IdSeleccionado(cbBuscarAlmacen),
                    NombreProveedor = TextoLibre(cbBuscarProveedor),
                    NombreAlmacen   = TextoLibre(cbBuscarAlmacen),
                };

                var resp = await _api
                    .ObtenerFacturasPendientesEscritorioAsync(filtro, CancellationToken.None)
                    .ConfigureAwait(true);

                MostrarBannerAplicaDir(resp != null && resp.aplica_dir);

                dgvFacturas.Rows.Clear();
                int n = 0;
                if (resp != null && resp.facturas != null)
                {
                    foreach (var f in resp.facturas)
                    {
                        dgvFacturas.Rows.Add(MapearAUiRow(f));
                        n++;
                    }
                }

                lblContador.Text = n == 0
                    ? "Sin facturas pendientes"
                    : (n + " factura" + (n == 1 ? "" : "s") + " pendiente" + (n == 1 ? "" : "s"));
            }
            catch (Exception ex)
            {
                lblContador.Text = "";
                MessageBox.Show(ex.Message, "Hubo un error inesperado",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConsultar.Enabled = true;
            }
        }

        private void MostrarBannerAplicaDir(bool aplicaDir)
        {
            if (aplicaDir)
            {
                lblBanner.Text =
                    "Modo automático activo (APLICA_DIR=TRUE) — solo se muestran las "
                    + "facturas SIN recepción ligada. Las demás las aplica el servicio "
                    + "Windows automáticamente.";
                panelBanner.Height  = 48;
                panelBanner.Visible = true;
            }
            else
            {
                panelBanner.Visible = false;
                panelBanner.Height  = 0;
            }
        }

        /// <summary>
        /// Convierte el DTO REST a una fila de 27 valores en el orden EXACTO
        /// de las columnas (mismo orden que las declaramos en ConstruirGrid).
        /// Calcula los campos derivados (ATRASO desde FECHA_PAGO,
        /// FOLIO_RECEPCION1 sin ceros) que el SOAP traía calculados del
        /// server.
        /// </summary>
        private static object[] MapearAUiRow(FacturaPendienteEscritorio f)
        {
            return new object[]
            {
                // === Internas (orden SOAP) ===
                0,                                              // DOCTO_CM_IDSQL
                f.ALMACEN_ID,                                   // MONEDA_ID (placeholder)
                f.ALMACEN_ID,                                   // ALMACEN_FK_MSP
                (double) f.DESCUENTO_GLOBAL,                    // GLOBAL
                f.RECEP_ID,                                     // Docto_cm_id_rec
                f.RECEPCION_ID,                                 // RECEPCION_ID_MS
                f.DOCTO_CM_ID,                                  // DOCTO_CM_ID
                f.PROVEEDOR_ID,                                 // PROVEEDOR_FK
                f.FOLIO_RECEPCION ?? "",                        // FOLIO_RECEPCION (con ceros)
                f.FOLIO_RECEPCION ?? "",                        // RECEP
                "",                                             // VERSION
                f.FECHA_FACTURA ?? "",                          // FECHA_FACTURA crudo
                "",                                             // PDF — placeholder hasta endpoint REST
                "",                                             // XML — placeholder hasta endpoint REST
                (double) f.TIPO_CAMBIO,                         // TIPO_CAMBIO

                // === Visibles por default (estilo SOAP) ===
                f.PROVEEDOR_NOMBRE ?? "",                       // NOMBRE
                f.FOLIO_PROV ?? "",                             // FOLIO_PROVEEDOR
                LimpiarFolio(f.FOLIO_RECEPCION),                // FOLIO_RECEPCION1 ("RM7230")
                FormatearFecha(f.FECHA_RECEPCION),              // RECIBIDA
                FormatearFecha(f.FECHA_PAGO),                   // FECHA (estimada de pago)
                CalcularAtraso(f.FECHA_PAGO),                   // ATRASO (días)
                (double) f.TOTAL,                               // IMPORTE
                f.ALMACEN_NOMBRE ?? "",                         // UNIDAD_DE_NEGOCIO

                // === Visibles por default (extras del DTO REST) ===
                f.RFC ?? "",                                    // RFC
                f.MONEDA_SIMBOLO ?? "",                         // MONEDA_SIMBOLO
                f.UUID ?? "",                                   // UUID
                (double) f.TOTAL_RETENCIONES,                   // RETENCIONES
            };
        }

        private static string LimpiarFolio(string folioConCeros)
        {
            if (string.IsNullOrEmpty(folioConCeros)) return "";
            // "RM00007230" → "RM7230". Conserva la serie alfa y quita los
            // ceros a la izquierda del bloque numérico final.
            var serie = "";
            var i = 0;
            while (i < folioConCeros.Length && !char.IsDigit(folioConCeros[i]))
            {
                serie += folioConCeros[i];
                i++;
            }
            var numero = folioConCeros.Substring(i).TrimStart('0');
            if (numero.Length == 0) numero = "0";
            return serie + numero;
        }

        private static string CalcularAtraso(string fechaPago)
        {
            if (string.IsNullOrEmpty(fechaPago)) return "";
            DateTime d;
            if (!DateTime.TryParse(fechaPago, CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out d))
                return "";
            int dias = (int) Math.Floor((DateTime.Today - d.Date).TotalDays);
            return dias > 0 ? dias.ToString(CultureInfo.InvariantCulture) : "0";
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

        // ====================================================================
        // Selección + acciones del menú
        // ====================================================================

        private void dgvFacturas_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            if (e.RowIndex < 0) return;
            dgvFacturas.ClearSelection();
            dgvFacturas.Rows[e.RowIndex].Selected = true;
            dgvFacturas.CurrentCell = dgvFacturas.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
        }

        /// <summary>
        /// MEJORA 4 — doble clic en un renglón = acción principal del menú
        /// ("Aplicar compra en Microsip"). Se ignora el doble clic en el
        /// header (RowIndex &lt; 0). La fila se selecciona ANTES de invocar
        /// el handler porque éste lee <see cref="FilaSeleccionada"/>.
        /// </summary>
        private void dgvFacturas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            dgvFacturas.ClearSelection();
            dgvFacturas.Rows[e.RowIndex].Selected = true;
            dgvFacturas.CurrentCell = dgvFacturas.Rows[e.RowIndex].Cells[e.ColumnIndex];
            accion_AplicarMicrosip(sender, EventArgs.Empty);
        }

        private DataGridViewRow FilaSeleccionada()
        {
            if (dgvFacturas.SelectedRows.Count == 0) return null;
            return dgvFacturas.SelectedRows[0];
        }

        private string GetCell(DataGridViewRow row, string nombreCol)
        {
            if (row == null || !dgvFacturas.Columns.Contains(nombreCol)) return "";
            var v = row.Cells[nombreCol].Value;
            return v == null ? "" : v.ToString();
        }

        private int GetCellInt(DataGridViewRow row, string nombreCol)
        {
            int x;
            return int.TryParse(GetCell(row, nombreCol), NumberStyles.Any,
                                CultureInfo.InvariantCulture, out x) ? x : 0;
        }

        /// <summary>
        /// Réplica F_FACTURAS.cs:308-375. Antes de abrir el modal valida el
        /// permiso Microsip 831 (crear nueva compra). Si el operador no lo
        /// tiene, MessageBox literal del SOAP y aborta.
        ///
        /// Las facturas SIN recepción (RECEP_ID=0) NO se bloquean — el modal
        /// pide al operador un artículo NO almacenable y una condición de
        /// pago, y el repositorio dispara el flujo SOAP
        /// <c>APLICAR_SIN_RECEPCION</c> (F_APLICAR_FACTURA.cs:1007-1689) en
        /// vez del flujo normal con recepción.
        /// </summary>
        private async void accion_AplicarMicrosip(object sender, EventArgs e)
        {
            var row = FilaSeleccionada();
            if (row == null) return;

            bool tienePermiso = await _permisos
                .TienePermisoAsync(_usuario, _password, "831", CancellationToken.None)
                .ConfigureAwait(true);

            if (!tienePermiso)
            {
                MessageBox.Show(
                    "Usted no tiene el permiso para hacer compras en Microsip",
                    "No se aplico factura",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var f = new FacturaPendienteEscritorio
            {
                DOCTO_CM_ID       = GetCellInt(row, "DOCTO_CM_ID"),
                FOLIO_PROV        = GetCell(row, "FOLIO_PROVEEDOR"),
                UUID              = GetCell(row, "UUID"),
                RFC               = GetCell(row, "RFC"),
                PROVEEDOR_NOMBRE  = GetCell(row, "NOMBRE"),
                PROVEEDOR_ID      = GetCellInt(row, "PROVEEDOR_FK"),
                TOTAL             = ParseDecimal(GetCell(row, "IMPORTE")),
                MONEDA_SIMBOLO    = GetCell(row, "MONEDA_SIMBOLO"),
                TIPO_CAMBIO       = ParseDecimal(GetCell(row, "TIPO_CAMBIO")),
                FECHA_FACTURA     = GetCell(row, "FECHA_FACTURA"),
                FECHA_RECEPCION   = GetCell(row, "RECIBIDA"),
                FECHA_PAGO        = GetCell(row, "FECHA"),
                ALMACEN_ID        = GetCellInt(row, "ALMACEN_FK_MSP"),
                ALMACEN_NOMBRE    = GetCell(row, "UNIDAD_DE_NEGOCIO"),
                RECEP_ID          = GetCellInt(row, "Docto_cm_id_rec"),
                FOLIO_RECEPCION   = GetCell(row, "FOLIO_RECEPCION"),
                RECEPCION_ID      = GetCellInt(row, "RECEPCION_ID_MS"),
            };

            try
            {
                var aplicador = new AplicadorFacturas(_api, new AplicacionRepository());
                using (var dlg = new FormAplicarFactura(_empresa, f, aplicador, _api, _usuario))
                {
                    var r = dlg.ShowDialog(this.FindForm());
                    if (r == DialogResult.OK && dlg.FacturaAplicada)
                    {
                        // Réplica F_FACTURAS.cs:366-369 — quitamos la fila
                        // aplicada del listado.
                        dgvFacturas.Rows.Remove(row);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensaje de aplicación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void accion_VistaPrevia(object sender, EventArgs e)
        {
            var row = FilaSeleccionada();
            if (row == null) return;
            using (var dlg = new FormVistaPrevia(
                _api,
                GetCell(row, "UUID"),
                "F",
                GetCell(row, "FOLIO_PROVEEDOR"),
                GetCell(row, "NOMBRE"),
                GetCellInt(row, "DOCTO_CM_ID"),
                _empresa.Id))
            {
                dlg.ShowDialog(this.FindForm());
            }
        }

        /// <summary>
        /// Réplica F_FACTURAS.cs:386-410 — pregunta literal del SOAP y luego
        /// descarga PDF + XML del CFDI vía REST y los escribe a disco con
        /// nombre <c>UUID.pdf</c> / <c>UUID.XML</c>.
        /// </summary>
        private async void accion_DescargarArchivos(object sender, EventArgs e)
        {
            var row = FilaSeleccionada();
            if (row == null) return;

            string uuid = GetCell(row, "UUID");
            if (string.IsNullOrEmpty(uuid))
            {
                MessageBox.Show("Sin UUID en la factura, no se pueden bajar archivos.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string rutaPred;
            try
            {
                var reg = new RegistrosWindows();
                reg.LeerRegistros(false);
                rutaPred = reg.RUTA_ARCHIVOS ?? "";
            }
            catch { rutaPred = ""; }

            string rutaDestino = null;
            var r = MessageBox.Show(
                "¿Desea guardar los archivos en la ruta predefinida?\r\n\n''" + rutaPred
                + "''\r\n\n SI: Continuar \n NO: Cambiar ruta",
                "Guardar Archivos",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (r == DialogResult.Yes)
            {
                if (string.IsNullOrEmpty(rutaPred) || !Directory.Exists(rutaPred))
                {
                    MessageBox.Show("La ruta predefinida no existe o no está configurada.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                rutaDestino = rutaPred;
            }
            else
            {
                using (var fbd = new FolderBrowserDialog
                {
                    Description = "Selecciona la carpeta donde guardar los archivos del CFDI",
                    SelectedPath = Directory.Exists(rutaPred) ? rutaPred : Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                })
                {
                    if (fbd.ShowDialog(this.FindForm()) != DialogResult.OK) return;
                    rutaDestino = fbd.SelectedPath;
                }
            }

            await DescargarCfdiAsync(uuid, "F", rutaDestino).ConfigureAwait(true);
        }

        /// <summary>
        /// Baja PDF y XML del CFDI vía REST y los escribe a la carpeta dada.
        /// Réplica funcional de F_FACTURAS.GuardarArchivos del SOAP.
        /// </summary>
        private async Task DescargarCfdiAsync(string uuid, string tipo, string carpeta)
        {
            int guardados = 0;

            try
            {
                var ct = CancellationToken.None;

                // PDF
                var pdf = await _api.ObtenerCfdiPdfAsync(uuid, tipo, ct).ConfigureAwait(true);
                if (pdf != null && pdf.Length > 0)
                {
                    File.WriteAllBytes(Path.Combine(carpeta, uuid + ".pdf"), pdf);
                    guardados++;
                }

                // XML
                var cfdi = await _api.ObtenerCfdiXmlAsync(uuid, tipo, ct).ConfigureAwait(true);
                if (cfdi != null && !string.IsNullOrEmpty(cfdi.xml))
                {
                    File.WriteAllText(Path.Combine(carpeta, uuid + ".XML"),
                        cfdi.xml, new System.Text.UTF8Encoding(false));
                    guardados++;
                }

                if (guardados == 0)
                {
                    MessageBox.Show("No hay archivos disponibles para este UUID en el portal.",
                        "Sin archivos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Texto literal del SOAP F_FACTURAS.cs:144.
                MessageBox.Show("Se han guardado los archivos con exito en la ruta " + carpeta,
                    "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar archivos",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void accion_RechazarConCorreo(object sender, EventArgs e)
        {
            var row = FilaSeleccionada();
            if (row == null) return;

            // El rechazo va por DOCTO_CM_ID (id MySQL de la factura) — réplica
            // del SOAP F_FACTURAS.cs:421 (f.DOCTO_CM = grid "Docto_cm_id_rec",
            // que en el legacy contenía facturas[i].DOCTO_CM_ID — ver
            // C_FUNCIONES.cs:268) y RECHAZA_FACTURA WHERE DOCTO_CM_ID
            // (services/facturas.php:285). En NUESTRO grid esa columna se
            // llama "DOCTO_CM_ID"; "Docto_cm_id_rec" aquí guarda el RECEP_ID
            // (MapearAUiRow) y NO sirve: no es único y vale 0 sin recepción.
            int doctoCmId    = GetCellInt(row, "DOCTO_CM_ID");
            int proveedorId  = GetCellInt(row, "PROVEEDOR_FK");
            string folio     = GetCell(row, "FOLIO_PROVEEDOR");
            string proveedor = GetCell(row, "NOMBRE");

            // Sugerir el correo registrado del proveedor (puede venir vacío;
            // el operador igual lo puede tipear).
            string correoSug = "";
            try
            {
                correoSug = await _api.ObtenerCorreoProveedorAsync(
                    proveedorId, _empresa.Id, CancellationToken.None
                ).ConfigureAwait(true);
            }
            catch { /* silencioso */ }

            using (var dlg = new FormEnviarRechazo(
                _api, FormEnviarRechazo.TipoDocumento.Factura,
                doctoCmId, _usuario, folio, proveedor, correoSug))
            {
                var r = dlg.ShowDialog(this.FindForm());
                if (r == DialogResult.OK && dlg.Rechazado)
                {
                    // Misma política que aplicar — la factura rechazada deja
                    // de aparecer como pendiente.
                    dgvFacturas.Rows.Remove(row);
                }
            }
        }

        /// <summary>
        /// Réplica F_FACTURAS.cs:440-462 — texto literal del SOAP en el
        /// MessageBox de confirmación + la MISMA llamada del legacy:
        /// <c>ws.ACTUALIZA_NUEVO_FOLIO(Docto_cm_id_rec)</c> (F_FACTURAS.cs:455,
        /// services/facturas.php:236-270 → UPDATE FACTURA_PROVEEDOR_33 SET
        /// ESTATUS='R' WHERE DOCTO_CM_ID=?). ESTATUS='R' = "en proceso de
        /// pago", congruente con el texto del modal; el endpoint
        /// descartar-factura (ESTATUS='C' por RECEP_ID) quedó deprecado por
        /// paridad SOAP — ya no se llama desde aquí.
        /// </summary>
        private async void accion_Descartar(object sender, EventArgs e)
        {
            var row = FilaSeleccionada();
            if (row == null) return;

            // En el legacy "Docto_cm_id_rec" contenía el DOCTO_CM_ID MySQL de
            // la factura (C_FUNCIONES.cs:268); en nuestro grid esa data vive
            // en la columna "DOCTO_CM_ID" (MapearAUiRow).
            int doctoCmId = GetCellInt(row, "DOCTO_CM_ID");

            string msj = "Este proceso ignora la factura, es decir, no la aplicara a Microsip, ";
            msj +=       "dejara de aparecerle en el portal al proveedor como pendiente, ";
            msj +=       "pasara a proceso de pago para el proveedor, ";
            msj +=       "sin generar ningun documento en Microsip.";

            var r = MessageBox.Show(
                msj + "\n\n" + "¿Esta seguro que desea continuar con la modificación?",
                "Mensaje de la aplicación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (r != DialogResult.Yes) return;

            try
            {
                bool ok = await _api.ActualizarNuevoFolioAsync(doctoCmId, CancellationToken.None)
                    .ConfigureAwait(true);
                if (ok)
                {
                    // Texto literal del SOAP F_FACTURAS.cs:457.
                    MessageBox.Show("Factura ignorada con exito", "Exito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dgvFacturas.Rows.Remove(row);
                }
                else
                {
                    MessageBox.Show("El portal rechazó la operación.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error inesperado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void accion_SoloCambiarEstatus(object sender, EventArgs e)
        {
            MessageBox.Show("Solo cambiar estatus portal — pendiente fase E.4.",
                "Pendiente", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static decimal ParseDecimal(string s)
        {
            decimal d;
            return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d)
                ? d : 0m;
        }
    }
}
