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
    /// Vista del tab "Cuentas por pagar" del FormPrincipal. Réplica funcional
    /// de <c>F_COMPLEMENTO</c> del SOAP legacy
    /// (PortalProveedores\F_COMPLEMENTO.cs + Designer.cs).
    ///
    /// **UI**: moderna, paleta del Tema. Mismo toolbar que VistaFacturas
    /// (Desde/Hasta/Solo por vencer/Límite/Consultar) + "Personalizar
    /// vista" para mostrar/ocultar columnas.
    ///
    /// **Behavior pixel-near al SOAP**:
    /// <list type="bullet">
    ///   <item>Caption: "Complementos de pago pendientes de aplicar en Microsip {empresa}"</item>
    ///   <item>Columnas con los <c>Name</c> y <c>HeaderText</c> literales del SOAP:
    ///         <c>Nombre</c> / <c>Concepto</c> / <c>Version pago</c> /
    ///         <c>Folio proveedor</c> / <c>Folio credito</c> /
    ///         <c>Recibido</c> / <c>Importe</c>.</item>
    ///   <item>Menú contextual: "Asociar CFDI en Microsip", "Vista previa
    ///         de archivos", "Descarga archivos", "Rechaza CFDI y envía
    ///         correo" (4 items, sin "Descartar" — eso es solo para facturas).</item>
    ///   <item>Permiso Microsip <b>713</b> (Modificar en cuentas por pagar)
    ///         antes de abrir el modal — réplica F_COMPLEMENTO.cs:329.</item>
    ///   <item>Permiso <b>715</b> (Cancelar) antes de "Rechazar". Mismo
    ///         texto de error que el SOAP.</item>
    /// </list>
    ///
    /// El listado se baja vía <c>GET /api/aplicacion/complementos-aplicar</c>
    /// (REST) — no hay filtros server-side todavía, así que el filtrado por
    /// fecha/proveedor/almacén se hace client-side. La lista de pendientes
    /// nunca es masiva (decenas, no miles).
    /// </summary>
    public sealed class VistaComplementos : UserControl
    {
        private readonly IPortalApi        _api;
        private readonly EmpresaEscritorio _empresa;
        private readonly string            _usuario;
        private readonly string            _password;
        private readonly PermisosMicrosip  _permisos;

        // Cache de la última lista bajada del portal — para filtrar
        // client-side sin volver a llamar al endpoint.
        private ComplementoAplicar[] _ultimoListado = new ComplementoAplicar[0];

        // ---- Toolbar (moderno) ----
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
        private Button         btnConsultar;
        private Label          lblContador;

        // Click derecho toolbar → "Personalizar vista".
        private ContextMenuStrip ctxToolbar;

        // ---- Grid + menú contextual ----
        private DataGridView     dgvComplementos;
        private ContextMenuStrip ctxGrid;
        private ToolStripMenuItem miAsociar;
        private ToolStripMenuItem miVistaPrevia;
        private ToolStripMenuItem miDescarga;
        private ToolStripMenuItem miRechaza;

        private const string PrefsSubsection = @"Vistas\VistaComplementos\Columnas";

        public VistaComplementos(IPortalApi api, EmpresaEscritorio empresa,
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
                await CargarCatalogoProveedoresAsync();
                await ConsultarAsync();
            };
        }

        /// <summary>
        /// Llena el ComboBox AutoComplete con los proveedores del catálogo
        /// vía <c>GET /api/escritorio/catalogos-filtros</c>. Best-effort.
        /// </summary>
        private async Task CargarCatalogoProveedoresAsync()
        {
            try
            {
                // Para complementos el filtro APLICA_DIR no aplica
                // (CR PPD no tienen recepción). Entidad=complementos.
                var cat = await _api.ObtenerCatalogosFiltrosAsync(
                    _empresa.Id, false, "complementos", CancellationToken.None
                ).ConfigureAwait(true);
                cbBuscarProveedor.Items.Clear();
                if (cat != null && cat.proveedores != null)
                    foreach (var p in cat.proveedores) cbBuscarProveedor.Items.Add(p);
            }
            catch { }
        }

        // ====================================================================
        // Layout
        // ====================================================================

        private void ConstruirUI()
        {
            this.Dock      = DockStyle.Fill;
            this.BackColor = Color.FromArgb(247, 249, 252);
            this.Padding   = new Padding(0);

            dgvComplementos = ConstruirGrid();
            AplicarPreferenciasColumnas();
            panelToolbar    = ConstruirToolbar();
            ctxGrid         = ConstruirMenuContextualGrid();
            ctxToolbar      = ConstruirMenuContextualToolbar();

            dgvComplementos.ContextMenuStrip = ctxGrid;
            dgvComplementos.CellMouseDown   += dgvComplementos_CellMouseDown;
            dgvComplementos.CellDoubleClick += dgvComplementos_CellDoubleClick;
            panelToolbar.ContextMenuStrip    = ctxToolbar;

            this.Controls.Add(dgvComplementos); // Fill
            this.Controls.Add(panelToolbar);    // Top
        }

        private void AplicarPreferenciasColumnas()
        {
            foreach (DataGridViewColumn col in dgvComplementos.Columns)
            {
                if (!col.Visible) continue;
                col.Visible = PreferenciasUsuario.LeerBool(
                    PrefsSubsection, col.Name, valorDefault: true);
            }
        }

        private Panel ConstruirToolbar()
        {
            // Toolbar de 2 filas — mismo layout y espaciados que
            // VistaFacturas (fila 1: fechas + por vencer + límite + contador
            // anclado a la derecha; fila 2: Proveedor ancho + Consultar al
            // final). La unidad de negocio no aplica a complementos.
            // IMPORTANTE: p.Width se fija ANTES de agregar hijos con
            // Anchor=Right — bug conocido de Anchor+Dock.
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
                // históricos. Los complementos quedan pendientes hasta que
                // se aplican — pueden tener varios meses de antigüedad.
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
                // Un año adelante por si el proveedor pone una FECHA_PAGO
                // futura (anticipos, calendarios de pagos programados).
                Value    = DateTime.Today.AddYears(1),
            };

            chkPorVencer = new CheckBox
            {
                Location  = new Point(424, 19),
                Size      = new Size(140, 24),
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(51, 65, 85),
                Text      = "Solo por vencer",
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
                Location = new Point(638, 18),
                Size     = new Size(64, 25),
                Minimum  = 1,
                Maximum  = 9999,
                Value    = 100,
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

            // ============ Fila 2: Proveedor + Consultar ============
            // ComboBox con AutoCompleteMode — datos del catálogo real.
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
                // 360px — mismo ancho que en VistaFacturas; las razones
                // sociales completas son largas.
                Location           = new Point(94, 60),
                Size               = new Size(360, 25),
                Font               = new Font("Segoe UI", 9.5F),
                DropDownStyle      = ComboBoxStyle.DropDown,
                AutoCompleteMode   = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                FlatStyle          = FlatStyle.Flat,
            };

            btnConsultar = new Button
            {
                Location  = new Point(478, 58),
                Size      = new Size(130, 30),
                BackColor = Tema.Primary, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI Semibold", 9F),
                Text      = "🔍  Consultar", Cursor = Cursors.Hand,
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
            p.Controls.Add(btnConsultar);
            p.Controls.Add(lblContador);
            return p;
        }

        private static string TextoComboLibre(ComboBox cb)
        {
            if (cb == null) return "";
            if (cb.SelectedItem != null
                && string.Equals(cb.SelectedItem.ToString(), cb.Text,
                                 StringComparison.OrdinalIgnoreCase))
                return cb.SelectedItem.ToString();
            return (cb.Text ?? "").Trim();
        }

        /// <summary>
        /// Columnas con los <c>Name</c> y <c>HeaderText</c> literales del
        /// SOAP F_COMPLEMENTO.Designer.cs:488-634. Las internas (IDs,
        /// FECHA_RECEPCION cruda, PDF/XML embebidos del SOAP, etc.) quedan
        /// siempre ocultas — sirven para alimentar los handlers.
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
            dgv.DefaultCellStyle.Font                        = new Font("Segoe UI", 9F);
            dgv.ColumnHeadersDefaultCellStyle.Font           = new Font("Segoe UI Semibold", 9F);
            dgv.ColumnHeadersDefaultCellStyle.BackColor      = Color.FromArgb(247, 249, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor      = Color.FromArgb(51, 65, 85);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 249, 252);
            dgv.AlternatingRowsDefaultCellStyle.BackColor    = Color.FromArgb(252, 252, 254);
            dgv.GridColor                                    = Color.FromArgb(230, 234, 240);

            // === Internas (siempre ocultas) ===
            dgv.Columns.Add(Col("INDICE",            "Índice",            0, visible: false));
            dgv.Columns.Add(Col("FECHA_RECEPCION",   "FECHA_RECEPCION",   0, visible: false));
            dgv.Columns.Add(Col("DOCTO_CP_ID",       "DOCTO_CP_ID",       0, visible: false));
            dgv.Columns.Add(Col("CREDITO_ID",        "CREDITO_ID",        0, visible: false));
            dgv.Columns.Add(Col("DOCTO_CP_ID_MSP",   "DOCTO_CP_ID_MSP",   0, visible: false));
            dgv.Columns.Add(Col("VERSION",           "Version",           0, visible: false));
            dgv.Columns.Add(Col("SERIE",             "SERIE",             0, visible: false));
            dgv.Columns.Add(Col("EMISOR_RFC",        "EMISOR_RFC",        0, visible: false));
            dgv.Columns.Add(Col("MONEDA_PAGO",       "MONEDA_PAGO",       0, visible: false));
            dgv.Columns.Add(Col("TIPOCAMBIOP",       "TIPOCAMBIOP",       0, visible: false));
            dgv.Columns.Add(Col("SUBTOTAL",          "SUBTOTAL",          0, visible: false));

            // === Visibles por default (HeaderText literal SOAP) ===
            dgv.Columns.Add(Col("NOMBRE",            "Nombre",            260));
            dgv.Columns.Add(Col("C_CONCEPTO_CP",     "Concepto",          140));
            dgv.Columns.Add(Col("C_VERSION_PAGO",    "Version pago",      90));
            dgv.Columns.Add(Col("FOLIO_PROVEEDOR",   "Folio proveedor",   130));
            dgv.Columns.Add(Col("FOLIO_CREDITO",     "Folio credito",     130));
            dgv.Columns.Add(Col("RECIBIDA",          "Recibido",          110));
            dgv.Columns.Add(ColImporte("IMPORTE",    "Importe",           130));
            dgv.Columns.Add(Col("UUID",              "UUID",              260));

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
        /// Menú contextual del grid — 4 items del SOAP
        /// (F_COMPLEMENTO.Designer.cs:168-202). Decoración con emojis para
        /// alinear con la VistaFacturas.
        /// </summary>
        private ContextMenuStrip ConstruirMenuContextualGrid()
        {
            var menu = new ContextMenuStrip();

            miAsociar     = new ToolStripMenuItem("⚙  &Asociar CFDI en Microsip") { Enabled = false };
            miVistaPrevia = new ToolStripMenuItem("👁  &Vista previa de archivos");
            miDescarga    = new ToolStripMenuItem("📥  &Descarga archivos");
            miRechaza     = new ToolStripMenuItem("✉  &Rechaza CFDI y envía correo");

            miAsociar.Click     += accion_AsociarMicrosip;
            miVistaPrevia.Click += accion_VistaPrevia;
            miDescarga.Click    += accion_DescargarArchivos;
            miRechaza.Click     += accion_RechazarConCorreo;

            menu.Items.Add(miAsociar);
            menu.Items.Add(miVistaPrevia);
            menu.Items.Add(miDescarga);
            menu.Items.Add(miRechaza);

            menu.Opened += (s, e) =>
            {
                bool hay = dgvComplementos.Rows.Count > 0;
                miAsociar.Enabled     = hay;
                miVistaPrevia.Enabled = hay;
                miDescarga.Enabled    = hay;
                miRechaza.Enabled     = hay;
            };

            return menu;
        }

        private ContextMenuStrip ConstruirMenuContextualToolbar()
        {
            var menu = new ContextMenuStrip();
            var miPersonalizar = new ToolStripMenuItem("⚙  Personalizar vista");

            miPersonalizar.DropDown.Closing += (s, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true;
            };

            foreach (DataGridViewColumn col in dgvComplementos.Columns)
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

        private static bool EsColumnaInterna(string nombre)
        {
            switch (nombre)
            {
                case "INDICE":
                case "FECHA_RECEPCION":
                case "DOCTO_CP_ID":
                case "CREDITO_ID":
                case "DOCTO_CP_ID_MSP":
                case "VERSION":
                case "SERIE":
                case "EMISOR_RFC":
                case "MONEDA_PAGO":
                case "TIPOCAMBIOP":
                case "SUBTOTAL":
                    return true;
                default:
                    return false;
            }
        }

        private void MenuPersonalizar_ItemToggled(object sender, EventArgs e)
        {
            var mi = (ToolStripMenuItem) sender;
            var nombreColumna = (string) mi.Tag;
            if (!dgvComplementos.Columns.Contains(nombreColumna)) return;
            dgvComplementos.Columns[nombreColumna].Visible = mi.Checked;
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
                // Réplica F_COMPLEMENTO.cs:151-200 — filtros SERVER-SIDE.
                // El endpoint nuevo /api/escritorio/complementos-pendientes
                // acepta proveedor, almacén, desde/hasta, por_vencer, limit.
                // El proveedor del combo (si hay) lo enviamos como id;
                // el filtro por nombre LIKE %X% ya no se usa, porque el
                // proveedor seleccionado da el id exacto.
                var filtro = new FiltroComplementosEscritorio
                {
                    EmpIdMsp      = _empresa.Id,
                    ProveedorId   = ExtraerProveedorId(cbBuscarProveedor),
                    Desde         = dtpDesde.Value.Date,
                    Hasta         = dtpHasta.Value.Date,
                    SoloPorVencer = chkPorVencer.Checked,
                    Limit         = (int) numLimite.Value,
                };

                var lista = await _api
                    .ObtenerComplementosPendientesEscritorioAsync(filtro, CancellationToken.None)
                    .ConfigureAwait(true);

                _ultimoListado = lista ?? new ComplementoAplicar[0];

                dgvComplementos.Rows.Clear();
                int n = 0;
                foreach (var c in _ultimoListado)
                {
                    dgvComplementos.Rows.Add(MapearAUiRow(c, n + 1));
                    n++;
                }

                lblContador.Text = n == 0
                    ? "Sin complementos pendientes"
                    : (n + " complemento" + (n == 1 ? "" : "s") + " pendiente" + (n == 1 ? "" : "s"));
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

        /// <summary>
        /// Devuelve el PROVEEDOR_ID_MSP del item seleccionado en el combo
        /// (o 0 si no es un item del catálogo). Si el usuario escribió texto
        /// libre que no corresponde a un item, se ignora — el endpoint
        /// nuevo solo acepta id exacto.
        /// </summary>
        private static int ExtraerProveedorId(ComboBox combo)
        {
            if (combo == null || combo.SelectedItem == null) return 0;
            var prov = combo.SelectedItem as CatalogoFiltroItem;
            return prov != null ? prov.id : 0;
        }

        /// <summary>
        /// Conservado por compatibilidad — ya NO se usa desde ConsultarAsync
        /// (los filtros viven server-side). Si el endpoint nuevo no estuviera
        /// disponible, este método permite filtrar lo que viene del Service.
        /// </summary>
        private IEnumerable<ComplementoAplicar> FiltrarClientSide(ComplementoAplicar[] todos)
        {
            DateTime desde         = dtpDesde.Value.Date;
            DateTime hasta         = dtpHasta.Value.Date.AddDays(1).AddSeconds(-1);
            bool soloPorVencer     = chkPorVencer.Checked;
            DateTime topePorVencer = DateTime.Today.AddDays(7);
            int limite             = (int) numLimite.Value;
            string buscaProv       = TextoComboLibre(cbBuscarProveedor);

            int count = 0;
            foreach (var c in todos)
            {
                if (count >= limite) yield break;

                DateTime fechaPago;
                if (!DateTime.TryParse(c.FECHA_PAGO, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out fechaPago))
                    continue;

                if (fechaPago < desde || fechaPago > hasta) continue;
                if (soloPorVencer && fechaPago > topePorVencer) continue;

                // Filtro por nombre del proveedor (LIKE %X%, case-insensitive).
                // Server-side no es posible porque /complementos-aplicar no
                // expone filtros — todo el catálogo viene y se filtra acá.
                if (buscaProv.Length > 0)
                {
                    var nombre = c.NOMBRE ?? "";
                    if (nombre.IndexOf(buscaProv, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                yield return c;
                count++;
            }
        }

        /// <summary>
        /// Convierte el DTO REST a la fila tal como el SOAP la consume.
        /// Orden de valores idéntico al orden en que las columnas se
        /// declararon en ConstruirGrid (críticamente importante).
        /// </summary>
        private static object[] MapearAUiRow(ComplementoAplicar c, int indice)
        {
            return new object[]
            {
                // === Internas ===
                indice,                                     // INDICE
                FormatearFecha(c.FECHA_COMPLEMENTO),        // FECHA_RECEPCION
                c.DOCTO_CP_ID,                              // DOCTO_CP_ID
                c.CREDITO_FK,                               // CREDITO_ID
                0,                                          // DOCTO_CP_ID_MSP — no expuesto en el DTO REST
                "",                                         // VERSION (CFDI 4.0 vs 3.3 — no expuesto)
                c.SERIE ?? "",                              // SERIE
                c.RFC ?? "",                                // EMISOR_RFC
                c.MONEDA_PAGO ?? "",                        // MONEDA_PAGO
                "1",                                        // TIPOCAMBIOP — placeholder
                "0.00",                                     // SUBTOTAL — placeholder

                // === Visibles ===
                c.NOMBRE ?? "",                             // NOMBRE
                ConceptoMostrar(c),                         // C_CONCEPTO_CP
                c.VERSION_PAGO ?? "",                       // C_VERSION_PAGO — del endpoint
                c.FOLIO_PAGO ?? "",                         // FOLIO_PROVEEDOR
                c.FOLIO_CREDITO ?? "",                      // FOLIO_CREDITO
                FormatearFecha(c.FECHA_COMPLEMENTO),        // RECIBIDA
                (double) c.MONTO,                           // IMPORTE
                c.UUID ?? "",                               // UUID
            };
        }

        /// <summary>
        /// El SOAP muestra el "Concepto" del crédito (ej. "Anticipo a
        /// proveedor"). Hasta que el endpoint REST exponga ese campo,
        /// mostramos un texto derivado del UsoCFDI para que la columna
        /// no quede vacía.
        /// </summary>
        private static string ConceptoMostrar(ComplementoAplicar c)
        {
            if (!string.IsNullOrEmpty(c.USO_CFDI)) return c.USO_CFDI;
            return "Pago de complemento";
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

        private void dgvComplementos_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            if (e.RowIndex < 0) return;
            dgvComplementos.ClearSelection();
            dgvComplementos.Rows[e.RowIndex].Selected = true;
            dgvComplementos.CurrentCell = dgvComplementos.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
        }

        /// <summary>
        /// MEJORA 4 — doble clic en un renglón = acción principal del menú
        /// ("Asociar CFDI en Microsip"). Se ignora el doble clic en el
        /// header (RowIndex &lt; 0). La fila se selecciona ANTES de invocar
        /// el handler porque éste lee <see cref="FilaSeleccionada"/>.
        /// </summary>
        private void dgvComplementos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            dgvComplementos.ClearSelection();
            dgvComplementos.Rows[e.RowIndex].Selected = true;
            dgvComplementos.CurrentCell = dgvComplementos.Rows[e.RowIndex].Cells[e.ColumnIndex];
            accion_AsociarMicrosip(sender, EventArgs.Empty);
        }

        private DataGridViewRow FilaSeleccionada()
        {
            if (dgvComplementos.SelectedRows.Count == 0) return null;
            return dgvComplementos.SelectedRows[0];
        }

        private ComplementoAplicar BuscarPorDoctoCpId(int doctoCpId)
        {
            if (_ultimoListado == null) return null;
            foreach (var c in _ultimoListado)
                if (c.DOCTO_CP_ID == doctoCpId) return c;
            return null;
        }

        private int GetCellInt(DataGridViewRow row, string nombreCol)
        {
            if (row == null || !dgvComplementos.Columns.Contains(nombreCol)) return 0;
            var v = row.Cells[nombreCol].Value;
            int x;
            return int.TryParse(v == null ? "" : v.ToString(),
                                NumberStyles.Any, CultureInfo.InvariantCulture, out x) ? x : 0;
        }

        /// <summary>
        /// Réplica F_COMPLEMENTO.cs:327-382. Antes de abrir el modal valida
        /// el permiso Microsip 713 (Modificar en cuentas por pagar). Mismo
        /// texto literal que el SOAP.
        /// </summary>
        private async void accion_AsociarMicrosip(object sender, EventArgs e)
        {
            var row = FilaSeleccionada();
            if (row == null) return;

            int doctoCpId = GetCellInt(row, "DOCTO_CP_ID");
            var comp = BuscarPorDoctoCpId(doctoCpId);
            if (comp == null)
            {
                MessageBox.Show(
                    "No se encontró el complemento seleccionado. Recarga la lista.",
                    "Sin datos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Réplica F_COMPLEMENTO.cs:329-332.
            bool tienePermiso = await _permisos
                .TienePermisoAsync(_usuario, _password, "713", CancellationToken.None)
                .ConfigureAwait(true);

            if (!tienePermiso)
            {
                MessageBox.Show(
                    "Usted no tiene el permiso para hacer modificaciones en cuentas por pagar en Microsip",
                    "No se aplico factura",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var aplicador = new AplicadorComplementos(_api, new AplicacionRepository());
                // _usuario = operador Microsip — se sella en USUARIO_ASOCIO_COBRO
                // (réplica F_APLICAR_COMPLEMENTO.cs:672, ws_usuario=USUARIO).
                using (var dlg = new FormAplicarComplemento(_empresa, comp, aplicador, _api, _usuario))
                {
                    var r = dlg.ShowDialog(this.FindForm());
                    if (r == DialogResult.OK && dlg.ComplementoAplicado)
                    {
                        dgvComplementos.Rows.Remove(row);
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

            int doctoCpId = GetCellInt(row, "DOCTO_CP_ID");
            var comp = BuscarPorDoctoCpId(doctoCpId);
            if (comp == null) return;

            using (var dlg = new FormVistaPrevia(
                _api, comp.UUID ?? "", "C",
                comp.FOLIO_PAGO ?? "", comp.NOMBRE ?? "",
                comp.DOCTO_CP_ID, _empresa.Id))
            {
                dlg.ShowDialog(this.FindForm());
            }
        }

        private async void accion_DescargarArchivos(object sender, EventArgs e)
        {
            var row = FilaSeleccionada();
            if (row == null) return;

            int doctoCpId = GetCellInt(row, "DOCTO_CP_ID");
            var comp = BuscarPorDoctoCpId(doctoCpId);
            if (comp == null || string.IsNullOrEmpty(comp.UUID))
            {
                MessageBox.Show("Sin UUID en el complemento, no se pueden bajar archivos.",
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

            // Texto literal del SOAP F_COMPLEMENTO.cs:266.
            var r = MessageBox.Show(
                " ¿Desea guardar los archivos en la ruta predefinida?\n\r\n\r" + rutaPred,
                "Guardar Archivos",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            string rutaDestino;
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
                    Description  = "Selecciona la carpeta donde guardar los archivos del CFDI",
                    SelectedPath = Directory.Exists(rutaPred) ? rutaPred : Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                })
                {
                    if (fbd.ShowDialog(this.FindForm()) != DialogResult.OK) return;
                    rutaDestino = fbd.SelectedPath;
                }
            }

            await DescargarCfdiComplementoAsync(comp.UUID, rutaDestino).ConfigureAwait(true);
        }

        private async Task DescargarCfdiComplementoAsync(string uuid, string carpeta)
        {
            int guardados = 0;
            try
            {
                var ct = CancellationToken.None;
                var pdf = await _api.ObtenerCfdiPdfAsync(uuid, "C", ct).ConfigureAwait(true);
                if (pdf != null && pdf.Length > 0)
                {
                    File.WriteAllBytes(Path.Combine(carpeta, uuid + ".pdf"), pdf);
                    guardados++;
                }
                var cfdi = await _api.ObtenerCfdiXmlAsync(uuid, "C", ct).ConfigureAwait(true);
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
                MessageBox.Show("Se han guardado los archivos con exito en la ruta " + carpeta,
                    "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar archivos",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Antes de rechazar valida el permiso Microsip 715 (Cancelar en
        /// cuentas por pagar) — réplica F_COMPLEMENTO.cs:394.
        /// </summary>
        private async void accion_RechazarConCorreo(object sender, EventArgs e)
        {
            var row = FilaSeleccionada();
            if (row == null) return;

            bool tienePermiso = await _permisos
                .TienePermisoAsync(_usuario, _password, "715", CancellationToken.None)
                .ConfigureAwait(true);

            if (!tienePermiso)
            {
                MessageBox.Show(
                    "Usted no tiene el permiso para hacer cancelaciones en cuentas por pagar en Microsip",
                    "No se aplico factura",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int doctoCpId = GetCellInt(row, "DOCTO_CP_ID");
            var comp = BuscarPorDoctoCpId(doctoCpId);
            if (comp == null) return;

            // Réplica del SOAP F_RECHAZA_ENVIA_CORREO.cs:97-136 — primero
            // intentamos por PROVEEDOR_ID (caso normal, complemento ligado
            // a un ACCESO del portal). Si no hay correo, fallback por RFC
            // (caso del SOAP: el complemento viene de un proveedor sin
            // PROVEEDOR_ID resolvible en el portal). Best-effort: si los
            // dos fallan dejamos vacío para que el operador teclee a mano.
            string correoSug = "";
            try
            {
                correoSug = await _api.ObtenerCorreoProveedorAsync(
                    comp.PROVEEDOR_ID, _empresa.Id, CancellationToken.None
                ).ConfigureAwait(true);
            }
            catch { }

            if (string.IsNullOrEmpty(correoSug) && !string.IsNullOrEmpty(comp.RFC))
            {
                try
                {
                    correoSug = await _api.ObtenerCorreoProveedorPorRfcAsync(
                        comp.RFC, _empresa.Id, CancellationToken.None
                    ).ConfigureAwait(true);
                }
                catch { }
            }

            using (var dlg = new FormEnviarRechazo(
                _api, FormEnviarRechazo.TipoDocumento.Complemento,
                comp.DOCTO_CP_ID, _usuario, comp.FOLIO_PAGO ?? "",
                comp.NOMBRE ?? "", correoSug))
            {
                var r = dlg.ShowDialog(this.FindForm());
                if (r == DialogResult.OK && dlg.Rechazado)
                {
                    dgvComplementos.Rows.Remove(row);
                }
            }
        }
    }
}
