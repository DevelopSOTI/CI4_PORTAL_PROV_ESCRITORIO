using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PortalProveedoresConfigurador.Configuracion;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Servicios;
// Configuracion.Tema vive en el mismo namespace que UAC (PortalProveedoresConfigurador.Configuracion).

namespace PortalProveedoresConfigurador.Formularios
{
    /// <summary>
    /// Ventana principal del Configurador con sidebar navigation
    /// (estilo Discord / VS Code / Settings de Windows 11). Las pestañas son
    /// paneles Visible=true/false controlados por los botones del sidebar.
    ///
    /// Operaciones que requieren admin (escribir HKLM, instalar/detener servicio)
    /// se delegan a <see cref="UAC.EjecutarTareaElevada(string, string[])"/>, que
    /// relanza este EXE con --task=&lt;nombre&gt;. Ver Program.cs para el dispatcher.
    /// </summary>
    public partial class FormPrincipal : Form
    {
        private const string RutaRegistro = @"SOFTWARE\SOTI\Service Portal";

        /// <summary>Sección actualmente visible. Se usa para evitar trabajo redundante.</summary>
        private string _seccionActual = "microsip";

        /// <summary>Snapshot del último GET /api/parametros para detectar deltas en el guardado.</summary>
        private List<ParametroPortal> _parametrosOriginales;

        /// <summary>Si true, los eventos del grid de empresas se ignoran. Lo
        /// usamos durante la carga inicial para que la asignación programática
        /// de celdas no dispare PATCH falsos al portal.</summary>
        private bool _cargandoEmpresas;

        /// <summary>Snapshot de la última carga de empresas, para filtrar
        /// client-side por nombre/RFC sin re-pedir al portal y para el contador.</summary>
        private List<EmpresaConfig> _empresasActuales = new List<EmpresaConfig>();

        public FormPrincipal()
        {
            InitializeComponent();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            ConfigurarTooltips();
            AplicarTema();
            CargarMicrosipDesdeRegistro();
            CargarPortalDesdeRegistro();
            CargarServicioDesdeRegistro();
            CargarHKLMTimerYCorreo();
            ActualizarBotonesNav();
            // No precargamos parámetros del portal hasta que el usuario abra
            // esa sección — evita pegar al portal en cada apertura del Configurador.
        }

        // ====================================================================
        // TEMA — repinta todo el form con la paleta cargada desde PORTAL_CONFIG
        // ====================================================================

        private void AplicarTema()
        {
            // -- Marca (sidebar header) -------------------------------------
            pnlSidebar.BackColor   = Tema.Secondary;
            pnlMarca.BackColor     = Tema.Secondary;
            pbLogo.BackColor       = Tema.Secondary;

            if (Tema.Logo != null)
            {
                pbLogo.Image   = Tema.Logo;
                pbLogo.Visible = true;
                lblMarca.Visible = false;   // el logo reemplaza al nombre como marca
            }
            else
            {
                lblMarca.Text    = Tema.NombreApp;
                lblMarca.Visible = true;
                pbLogo.Visible   = false;
            }

            // -- Botones del sidebar (inactivos + hover) --------------------
            var navs = new[] { btnNavMicrosip, btnNavPortal, btnNavServicio,
                               btnNavOtros, btnNavEmpresas, btnNavDias };
            foreach (var b in navs)
            {
                b.BackColor = Tema.Secondary;
                b.FlatAppearance.MouseOverBackColor = Tema.SecondaryHover;
            }
            ActualizarBotonesNav(); // pinta el activo con Tema.Primary

            // -- Botones primarios -------------------------------------------
            var primarios = new[] { btnGuardarMicrosip, btnGuardarPortal, btnGuardarHKLM,
                                    btnGuardarParametros, btnGuardarDias,
                                    btnGuardarServicio, btnIniciarServicio };
            // (Empresas no tiene botón Guardar: cambios commit en cada celda.)
            foreach (var b in primarios)
            {
                b.BackColor = Tema.Primary;
                b.FlatAppearance.MouseOverBackColor = Tema.PrimaryHover;
            }

            // -- Selección del grid en color primary tenue --------------------
            dgvParametros.DefaultCellStyle.SelectionBackColor = Tema.Aclarar(Tema.Primary, 80);

            // -- Título del form ---------------------------------------------
            this.Text = Tema.NombreApp + " — Configurador";

            // -- StatusStrip: indicador modo offline -------------------------
            if (Tema.ModoOffline)
            {
                lblEstadoPortal.Text = "Modo offline";
                lblEstadoPortal.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            }
            else
            {
                lblEstadoPortal.Text = "Portal: conectado";
                lblEstadoPortal.ForeColor = System.Drawing.Color.FromArgb(22, 163, 74);
            }
        }

        // ====================================================================
        // NAVEGACIÓN ENTRE SECCIONES
        // ====================================================================

        private void btnNavMicrosip_Click(object sender, EventArgs e) { MostrarSeccion("microsip"); }
        private void btnNavPortal_Click(object sender, EventArgs e)   { MostrarSeccion("portal"); }
        private void btnNavServicio_Click(object sender, EventArgs e) { MostrarSeccion("servicio"); }
        private void btnNavOtros_Click(object sender, EventArgs e)    { MostrarSeccion("otros"); }
        private void btnNavEmpresas_Click(object sender, EventArgs e) { MostrarSeccion("empresas"); }
        private void btnNavDias_Click(object sender, EventArgs e)     { MostrarSeccion("dias"); }

        private void MostrarSeccion(string nombre)
        {
            _seccionActual = nombre;
            pnlSeccionMicrosip.Visible = nombre == "microsip";
            pnlSeccionPortal.Visible   = nombre == "portal";
            pnlSeccionServicio.Visible = nombre == "servicio";
            pnlSeccionOtros.Visible    = nombre == "otros";
            pnlSeccionEmpresas.Visible = nombre == "empresas";
            pnlSeccionDias.Visible     = nombre == "dias";

            ActualizarBotonesNav();

            // Carga perezosa: cada vez que el usuario abre una sección que
            // necesita datos del portal, pegamos. No usamos botones de Recargar.
            if (nombre == "otros")    { var fnf = new Func<Task>(CargarParametrosAsync); fnf.Invoke(); }
            if (nombre == "dias")     { var fnf = new Func<Task>(CargarDiasAsync);       fnf.Invoke(); }
            if (nombre == "empresas") { var fnf = new Func<Task>(CargarEmpresasAsync);   fnf.Invoke(); }
            if (nombre == "servicio") { RefrescarEstadoServicio(); }
        }

        /// <summary>
        /// Resalta el botón de la sección activa con el color de acento del
        /// sidebar, para que el usuario sepa siempre dónde está.
        /// </summary>
        private void ActualizarBotonesNav()
        {
            var nav = new Dictionary<string, Button>
            {
                { "microsip", btnNavMicrosip },
                { "portal",   btnNavPortal },
                { "servicio", btnNavServicio },
                { "otros",    btnNavOtros },
                { "empresas", btnNavEmpresas },
                { "dias",     btnNavDias },
            };

            foreach (var kv in nav)
            {
                var esActivo = kv.Key == _seccionActual;
                kv.Value.BackColor = esActivo ? Tema.Primary : Tema.Secondary;
                kv.Value.FlatAppearance.MouseOverBackColor =
                    esActivo ? Tema.Primary : Tema.SecondaryHover;
            }
        }

        // ====================================================================
        // SECCIÓN MICROSIP
        // ====================================================================

        private void CargarMicrosipDesdeRegistro()
        {
            try
            {
                var reg = new RegistrosWindows();
                reg.LeerRegistros(RutaRegistro);
                txtMicSrv.Text  = reg.MICRO_SERVER ?? "";
                txtMicRoot.Text = reg.MICRO_ROOT   ?? "";
                txtMicUser.Text = reg.MICRO_USER   ?? "";
                txtMicPass.Text = reg.MICRO_PASS   ?? "";
            }
            catch (Exception ex)
            {
                AvisoWarning("No se pudieron leer los registros de Microsip:\n\n" + ex.Message);
            }
        }

        private void btnMicExaminar_Click(object sender, EventArgs e)
        {
            folderBrowser.Description = "Selecciona la carpeta 'Microsip datos'";
            if (!string.IsNullOrWhiteSpace(txtMicRoot.Text) && Directory.Exists(txtMicRoot.Text))
                folderBrowser.SelectedPath = txtMicRoot.Text;

            if (folderBrowser.ShowDialog(this) == DialogResult.OK)
                txtMicRoot.Text = folderBrowser.SelectedPath;
        }

        private void btnProbarMicrosip_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMicSrv.Text)
             || string.IsNullOrWhiteSpace(txtMicRoot.Text)
             || string.IsNullOrWhiteSpace(txtMicUser.Text))
            {
                MarcarEstadoMicrosip("Completa servidor, carpeta y usuario.", false);
                return;
            }

            MarcarEstadoMicrosip("Probando...", null);
            UseWaitCursor = true;
            try
            {
                var cn = new ConexionMicrosip();
                string mensaje;
                var ok = cn.ConectarConfigPrueba(
                    txtMicSrv.Text.Trim(),
                    txtMicRoot.Text.Trim(),
                    txtMicUser.Text.Trim(),
                    txtMicPass.Text,
                    out mensaje);

                if (ok)
                {
                    cn.Desconectar();
                    MarcarEstadoMicrosip("Conectado a System\\CONFIG.FDB.", true);
                }
                else
                {
                    MarcarEstadoMicrosip("No conectó: " + mensaje, false);
                }
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void btnGuardarMicrosip_Click(object sender, EventArgs e)
        {
            int codigo = UAC.EjecutarTareaElevada(
                "guardar-microsip",
                "--micro-srv="  + EscaparArg(txtMicSrv.Text),
                "--micro-root=" + EscaparArg(txtMicRoot.Text),
                "--micro-user=" + EscaparArg(txtMicUser.Text),
                "--micro-pass=" + EscaparArg(txtMicPass.Text));

            ManejarResultadoElevado(codigo, "Conexión a Microsip guardada.");
        }

        private void MarcarEstadoMicrosip(string mensaje, bool? exito)
        {
            lblEstadoConexionMic.Text = mensaje;
            if (exito == true)       lblEstadoConexionMic.ForeColor = Color.FromArgb(22, 163, 74);   // green-600
            else if (exito == false) lblEstadoConexionMic.ForeColor = Color.FromArgb(220, 38, 38);   // red-600
            else                     lblEstadoConexionMic.ForeColor = Color.FromArgb(100, 116, 139); // slate-500
        }

        // ====================================================================
        // SECCIÓN PORTAL WEB
        // ====================================================================

        private void CargarPortalDesdeRegistro()
        {
            try
            {
                var reg = new RegistrosWindows();
                reg.LeerRegistros(RutaRegistro);
                txtPortalUrl.Text    = reg.PORTAL_BASE_URL ?? "";
                txtPortalApiKey.Text = reg.PORTAL_API_KEY  ?? "";
            }
            catch (Exception ex)
            {
                AvisoWarning("No se pudieron leer los registros del portal:\n\n" + ex.Message);
            }
        }

        /// <summary>
        /// Toggle del ojo: alterna ocultar/mostrar el contenido del API Key.
        /// El API Key se ve por defecto oculto (UseSystemPasswordChar=true) para
        /// que no quede a la vista de cualquiera que mire la pantalla.
        /// </summary>
        private void btnPortalToggle_Click(object sender, EventArgs e)
        {
            txtPortalApiKey.UseSystemPasswordChar = !txtPortalApiKey.UseSystemPasswordChar;
            btnPortalToggle.Text = txtPortalApiKey.UseSystemPasswordChar ? "Mostrar" : "Ocultar";
        }

        private async void btnProbarPortal_Click(object sender, EventArgs e)
        {
            var url = (txtPortalUrl.Text ?? "").Trim();
            var key = (txtPortalApiKey.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
            {
                MarcarEstadoPortal("Completa URL y API Key.", false);
                return;
            }

            MarcarEstadoPortal("Probando...", null);
            btnProbarPortal.Enabled = false;
            UseWaitCursor = true;

            try
            {
                IPortalApi api;
                try { api = new PortalApi(url, key); }
                catch (Exception ex)
                {
                    MarcarEstadoPortal("URL o API Key inválidos: " + ex.Message, false);
                    return;
                }

                // Usamos GET /api/portal-config como ping: si responde, sabemos
                // que la URL llega al portal Y que la API key es válida; además
                // recibimos el nombre del portal para mostrarlo al operador.
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8)))
                {
                    var tema = await api.ObtenerTemaAsync(cts.Token).ConfigureAwait(true);
                    MarcarEstadoPortal("Conectado a " + tema.nombre + ".", true);
                    ActualizarEstadoPortalStatusStrip(true);
                }
            }
            catch (PortalApiException px)
            {
                if (px.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    MarcarEstadoPortal("API Key inválida (401).", false);
                else
                    MarcarEstadoPortal("Error " + (int)px.StatusCode + " del portal.", false);
            }
            catch (TaskCanceledException)
            {
                MarcarEstadoPortal("Timeout: el portal no respondió en 8 seg.", false);
            }
            catch (Exception ex)
            {
                MarcarEstadoPortal("No conectó: " + ex.Message, false);
            }
            finally
            {
                btnProbarPortal.Enabled = true;
                UseWaitCursor = false;
            }
        }

        private void btnGuardarPortal_Click(object sender, EventArgs e)
        {
            var url = (txtPortalUrl.Text ?? "").Trim().TrimEnd('/');
            var key = (txtPortalApiKey.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
            {
                AvisoWarning("URL y API Key son obligatorios.");
                return;
            }

            int codigo = UAC.EjecutarTareaElevada(
                "guardar-portal",
                "--portal-url="     + EscaparArg(url),
                "--portal-api-key=" + EscaparArg(key));

            ManejarResultadoElevado(codigo, "Conexión al portal guardada.");
        }

        private void MarcarEstadoPortal(string mensaje, bool? exito)
        {
            lblEstadoConexionPortal.Text = mensaje;
            if (exito == true)       lblEstadoConexionPortal.ForeColor = Color.FromArgb(22, 163, 74);
            else if (exito == false) lblEstadoConexionPortal.ForeColor = Color.FromArgb(220, 38, 38);
            else                     lblEstadoConexionPortal.ForeColor = Color.FromArgb(100, 116, 139);
        }

        /// <summary>
        /// Refresca el indicador "Portal: ..." de la barra inferior cuando el
        /// operador acaba de probar la conexión. Útil porque al abrir la app
        /// pudimos haber estado offline; tras configurar correctamente, el
        /// estado debe actualizarse sin reiniciar.
        /// </summary>
        private void ActualizarEstadoPortalStatusStrip(bool conectado)
        {
            if (conectado)
            {
                lblEstadoPortal.Text = "Portal: conectado";
                lblEstadoPortal.ForeColor = Color.FromArgb(22, 163, 74);
            }
            else
            {
                lblEstadoPortal.Text = "Modo offline";
                lblEstadoPortal.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

        // ====================================================================
        // SECCIÓN DÍAS DE RECEPCIÓN
        // ====================================================================

        private CheckBox[] CajitasDias()
        {
            return new[] { chkDia1, chkDia2, chkDia3, chkDia4, chkDia5, chkDia6, chkDia7 };
        }

        private async Task CargarDiasAsync()
        {
            HabilitarGuardarDias(false);
            MarcarEstadoDias("Cargando...", null);

            try
            {
                var api = ConstruirApi();
                if (api == null) return;

                var lista = await api.ListarDiasAsync(CancellationToken.None);

                // Mapear numero → checkbox. Los nombres del backend (LUNES,
                // MARTES, ...) son CAPS sin acento; los del UI están en formato
                // humano. Por eso usamos el numero como llave, no el nombre.
                var cajitas = CajitasDias();
                foreach (var d in lista)
                {
                    if (d.numero >= 1 && d.numero <= 7)
                        cajitas[d.numero - 1].Checked = d.recibe;
                }

                MarcarEstadoDias("", null); // limpia el "Cargando..."
            }
            catch (PortalApiException px)
            {
                MarcarEstadoDias("Error " + (int)px.StatusCode + " del portal.", false);
            }
            catch (Exception ex)
            {
                MarcarEstadoDias("No se pudo cargar: " + ex.Message, false);
            }
            finally
            {
                HabilitarGuardarDias(true);
            }
        }

        private async void btnGuardarDias_Click(object sender, EventArgs e)
        {
            HabilitarGuardarDias(false);
            MarcarEstadoDias("Guardando...", null);

            try
            {
                var api = ConstruirApi();
                if (api == null) return;

                // Mandamos los 7 días en el batch — el PATCH es parcial pero
                // también funciona como reemplazo completo, y nos da consistencia
                // sin tener que recordar el snapshot anterior.
                var cajitas = CajitasDias();
                var cambios = new List<DiaRecepcion>();
                for (int i = 0; i < 7; i++)
                    cambios.Add(new DiaRecepcion { numero = i + 1, recibe = cajitas[i].Checked });

                await api.ActualizarDiasAsync(cambios, CancellationToken.None);
                MarcarEstadoDias("Cambios guardados.", true);
            }
            catch (PortalApiException px)
            {
                MarcarEstadoDias("Error " + (int)px.StatusCode + " del portal.", false);
            }
            catch (Exception ex)
            {
                MarcarEstadoDias("No se pudo guardar: " + ex.Message, false);
            }
            finally
            {
                HabilitarGuardarDias(true);
            }
        }

        private void HabilitarGuardarDias(bool habilitado)
        {
            btnGuardarDias.Enabled = habilitado;
            UseWaitCursor = !habilitado;
        }

        private void MarcarEstadoDias(string mensaje, bool? exito)
        {
            lblEstadoDias.Text = mensaje;
            if (exito == true)       lblEstadoDias.ForeColor = Color.FromArgb(22, 163, 74);
            else if (exito == false) lblEstadoDias.ForeColor = Color.FromArgb(220, 38, 38);
            else                     lblEstadoDias.ForeColor = Color.FromArgb(100, 116, 139);
        }

        // ====================================================================
        // SECCIÓN EMPRESAS
        // ====================================================================

        private async Task CargarEmpresasAsync()
        {
            MarcarEstadoEmpresas("Cargando...", null);
            try
            {
                var api = ConstruirApi();
                if (api == null) return;

                var lista = await api.ListarEmpresasAsync(CancellationToken.None);
                _empresasActuales = lista;

                _cargandoEmpresas = true;
                try
                {
                    dgvEmpresas.Rows.Clear();
                    foreach (var e in lista) AgregarFilaEmpresa(e);
                    AplicarFiltroEmpresas();
                }
                finally
                {
                    _cargandoEmpresas = false;
                }

                ActualizarContadorEmpresas();
                MarcarEstadoEmpresas("", null);
            }
            catch (PortalApiException px)
            {
                MarcarEstadoEmpresas("Error " + (int)px.StatusCode + " del portal.", false);
            }
            catch (Exception ex)
            {
                MarcarEstadoEmpresas("No se pudo cargar: " + ex.Message, false);
            }
        }

        private void AgregarFilaEmpresa(EmpresaConfig e)
        {
            var estatus = NormalizarEstatus(e.estatus);
            int idx = dgvEmpresas.Rows.Add(
                e.emp_id_msp,
                e.nombre ?? "",
                e.nombre_largo ?? "",
                e.rfc ?? "",
                estatus,
                DiferenciaABool(e.diferencia),
                FormatearSincDesde(e.sinc_desde),
                FormatearUltSinc(e.ult_sinc));

            // Si el estatus que vino del backend no coincide con los items del
            // combobox, DataGridView lanza error visual. NormalizarEstatus lo
            // mapea a Bloqueada como fallback seguro.

            PintarFilaSegunEstatus(dgvEmpresas.Rows[idx], estatus);
        }

        /// <summary>
        /// Verde muy claro para las filas de empresas <b>Autorizada</b>, blanco
        /// (default) para las demás. Es un indicador visual rápido para que el
        /// operador distinga de un vistazo qué empresas ya están habilitadas
        /// sin tener que leer la columna Estatus.
        /// </summary>
        private static readonly Color BackAutorizada = Color.FromArgb(240, 253, 244);

        /// <summary>
        /// Aplica el color de fondo de la fila según el estatus de la empresa.
        /// Centralizado para que el grid quede consistente al cargar, al
        /// refrescar post-PATCH y al revertir un cambio cancelado.
        /// </summary>
        private static void PintarFilaSegunEstatus(DataGridViewRow row, string estatus)
        {
            if (row == null) return;
            if (string.Equals(estatus, "Autorizada", StringComparison.Ordinal))
                row.DefaultCellStyle.BackColor = BackAutorizada;
            else
                row.DefaultCellStyle.BackColor = Color.Empty; // vuelve al default del grid
        }

        /// <summary>
        /// Formato humano para la columna "Sincronizar desde": fecha legible o
        /// "Sin filtro" cuando el backend manda null (que significa
        /// "sincronizar toda la historia").
        /// </summary>
        private static string FormatearSincDesde(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Sin filtro";
            DateTime dt;
            if (DateTime.TryParse(raw, out dt)) return dt.ToString("dd/MM/yyyy HH:mm");
            return raw;
        }

        /// <summary>
        /// Parse inverso: lee la celda "Sincronizar desde" y la convierte a
        /// DateTime? para que podamos pre-cargar el DateTimePicker del modal con
        /// la fecha que el operador ya tenía configurada.
        /// </summary>
        private static DateTime? ParsearSincDesde(string formateado)
        {
            if (string.IsNullOrWhiteSpace(formateado) || formateado == "Sin filtro") return null;
            DateTime dt;
            return DateTime.TryParse(formateado, out dt) ? dt : (DateTime?) null;
        }

        /// <summary>
        /// Asegura que el valor enviado al ComboBoxColumn sea uno de los items
        /// permitidos. Si llega algo raro, regresa "Bloqueada" — la decisión
        /// segura para una empresa sin estatus conocido.
        /// </summary>
        private static string NormalizarEstatus(string s)
        {
            if (string.Equals(s, "Autorizada", StringComparison.Ordinal)) return "Autorizada";
            return "Bloqueada";
        }

        private static bool DiferenciaABool(string s)
        {
            return string.Equals(s, "S", StringComparison.OrdinalIgnoreCase);
        }

        private static string BoolADiferencia(bool b)
        {
            return b ? "S" : "N";
        }

        private static string FormatearUltSinc(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Nunca";
            DateTime dt;
            // Backend manda "YYYY-MM-DD HH:MM:SS" (datetime de MySQL serializado).
            if (DateTime.TryParse(raw, out dt))
                return dt.ToString("dd/MM/yyyy HH:mm");
            return raw;
        }

        /// <summary>
        /// Se llama tras cualquier cambio de celda. Distingue commits humanos
        /// (Estatus/Diferencia → PATCH al portal) de la repoblación programática
        /// durante CargarEmpresasAsync (ignorar). El flag <see cref="_cargandoEmpresas"/>
        /// distingue uno del otro.
        ///
        /// Caso especial: cuando el operador cambia Estatus de Bloqueada a
        /// Autorizada y la empresa todavía no tiene "Sincronizar desde", abrimos
        /// el modal para preguntar antes de mandar el PATCH. Si el operador
        /// cancela el modal, revertimos el cambio de estatus en la celda.
        /// </summary>
        private async void dgvEmpresas_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_cargandoEmpresas) return;
            if (e.RowIndex < 0)    return;

            var row = dgvEmpresas.Rows[e.RowIndex];
            var idMsp = ToInt(row.Cells[colEmpIdMsp.Index].Value);
            if (idMsp <= 0) return;

            if (e.ColumnIndex == colEmpEstatus.Index)
            {
                var nuevoEstatus = row.Cells[colEmpEstatus.Index].Value as string;

                // Promoción Bloqueada → Autorizada Y la empresa no tenía fecha:
                // preguntamos por la fecha desde la cual queremos sincronizar.
                if (nuevoEstatus == "Autorizada")
                {
                    var original = EmpresaOriginal(idMsp);
                    bool yaTeniaSincDesde = original != null && !string.IsNullOrWhiteSpace(original.sinc_desde);
                    bool eraBloqueada = original == null || original.estatus != "Autorizada";

                    if (eraBloqueada && !yaTeniaSincDesde)
                    {
                        var nombre = (row.Cells[colEmpNombreLargo.Index].Value as string) ?? (row.Cells[colEmpNombre.Index].Value as string) ?? "";
                        var sincDesde = AbrirModalSincDesde(nombre, null);
                        if (sincDesde == null)
                        {
                            // Operador canceló — revertimos el estatus al original.
                            RevertirEstatusEnFila(row, original != null ? original.estatus : "Bloqueada");
                            return;
                        }
                        await PatchEmpresaAsync(idMsp, "Autorizada", null, sincDesde);
                        return;
                    }
                }

                await PatchEmpresaAsync(idMsp, nuevoEstatus, null, ValorSincDesde.NoTocar);
                return;
            }

            if (e.ColumnIndex == colEmpDiferencia.Index)
            {
                var nuevaDiferencia = BoolADiferencia(row.Cells[colEmpDiferencia.Index].Value as bool? ?? false);
                await PatchEmpresaAsync(idMsp, null, nuevaDiferencia, ValorSincDesde.NoTocar);
                return;
            }

            // Otras columnas — no nos importan.
        }

        /// <summary>
        /// Doble-click en la columna "Sincronizar desde" abre el modal para
        /// cambiar la fecha. Funciona para empresas Autorizadas y Bloqueadas
        /// por igual (el filtro solo tiene efecto cuando está Autorizada y el
        /// servicio empieza a sincronizar documentos).
        /// </summary>
        private async void dgvEmpresas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_cargandoEmpresas) return;
            if (e.RowIndex < 0)    return;
            if (e.ColumnIndex != colEmpSincDesde.Index) return;

            var row = dgvEmpresas.Rows[e.RowIndex];
            var idMsp = ToInt(row.Cells[colEmpIdMsp.Index].Value);
            if (idMsp <= 0) return;

            var nombre = (row.Cells[colEmpNombreLargo.Index].Value as string)
                      ?? (row.Cells[colEmpNombre.Index].Value as string) ?? "";
            var actual = ParsearSincDesde(row.Cells[colEmpSincDesde.Index].Value as string);

            var elegido = AbrirModalSincDesde(nombre, actual);
            if (elegido == null) return; // cancelado

            await PatchEmpresaAsync(idMsp, null, null, elegido);
        }

        /// <summary>
        /// Despliega el modal modal-of-modals y devuelve el resultado o null si
        /// el usuario cancela. Centralizado para que CellValueChanged y
        /// CellDoubleClick lo invoquen del mismo lugar.
        /// </summary>
        private ValorSincDesde AbrirModalSincDesde(string nombreEmpresa, DateTime? fechaActual)
        {
            using (var dlg = new FormSincDesde(nombreEmpresa, fechaActual))
            {
                return dlg.ShowDialog(this) == DialogResult.OK ? dlg.Resultado : null;
            }
        }

        /// <summary>
        /// Restaura el ComboBox de estatus al valor original sin disparar el
        /// CellValueChanged (que entraría en bucle infinito si no lo silenciamos).
        /// </summary>
        private void RevertirEstatusEnFila(DataGridViewRow row, string original)
        {
            _cargandoEmpresas = true;
            try
            {
                row.Cells[colEmpEstatus.Index].Value = original;
                PintarFilaSegunEstatus(row, original);
            }
            finally { _cargandoEmpresas = false; }
        }

        private EmpresaConfig EmpresaOriginal(int idMsp)
        {
            foreach (var e in _empresasActuales)
                if (e.emp_id_msp == idMsp) return e;
            return null;
        }

        /// <summary>
        /// Las celdas CheckBox y ComboBox normalmente NO disparan CellValueChanged
        /// hasta perder el foco. Forzamos el commit en cuanto el usuario toca la
        /// celda — así el PATCH viaja al instante.
        /// </summary>
        private void dgvEmpresas_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvEmpresas.IsCurrentCellDirty)
                dgvEmpresas.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private async Task PatchEmpresaAsync(int idMsp, string nuevoEstatus, string nuevaDiferencia, ValorSincDesde sincDesde)
        {
            // Detectamos transición Bloqueada→Autorizada ANTES de cualquier
            // operación, para replicar el orden del SOAP legacy
            // (FEmpresas.cs:262-284): primero configurar Microsip, después
            // autorizar en el portal. Si Microsip falla, NO autorizamos en
            // el portal — así no queda estado inconsistente.
            var original = EmpresaOriginal(idMsp);
            bool eraBloqueada = original == null
                             || !string.Equals(original.estatus, "Autorizada", StringComparison.Ordinal);
            bool esAutorizacionInicial = eraBloqueada
                                       && string.Equals(nuevoEstatus, "Autorizada", StringComparison.Ordinal);

            // Paso 0 (solo en autorización inicial): configurar Microsip.
            if (esAutorizacionInicial)
            {
                var nombreVisualPre = NombreVisualEmpresa(idMsp);
                MarcarEstadoEmpresas("Configurando " + nombreVisualPre + " en Microsip...", null);

                ResumenConfiguracionEmpresa r;
                try
                {
                    var configurador = new ConfiguradorCamposLibres();
                    r = await configurador.AsegurarAsync(idMsp, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    MarcarEstadoEmpresas("Microsip falló — autorización cancelada.", false);
                    RevertirEstatusEnFila(BuscarFilaEmpresa(idMsp), original != null ? original.estatus : "Bloqueada");
                    MessageBox.Show(this,
                        "No se pudo configurar Microsip para " + nombreVisualPre + ":\n\n" + ex.Message
                        + "\n\nLa empresa NO fue autorizada en el portal. Verifique el problema y vuelva a intentar.",
                        "Configuración de Microsip",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Si la configuración tuvo problemas graves, abortamos también.
                // Casos: empresa no está en CONFIG.FDB, conexión al FDB falló,
                // LIBRES_PROVEEDOR no existe. Mostramos el detalle al operador.
                if (!ConfiguracionMicrosipFueExitosa(r, nombreVisualPre))
                {
                    RevertirEstatusEnFila(BuscarFilaEmpresa(idMsp), original != null ? original.estatus : "Bloqueada");
                    return;
                }

                // Microsip OK — mostramos resumen pero NO bloquea el PATCH.
                MostrarResumenConfiguracionMicrosip(nombreVisualPre, r);
            }

            // Paso 1: PATCH al portal. Solo llega aquí si Microsip estaba OK
            // (o si no era una autorización inicial — diferencia/sinc_desde).
            MarcarEstadoEmpresas("Guardando...", null);
            try
            {
                var api = ConstruirApi();
                if (api == null) return;

                var actualizada = await api.ActualizarEmpresaAsync(idMsp, nuevoEstatus, nuevaDiferencia, sincDesde ?? ValorSincDesde.NoTocar, CancellationToken.None);
                if (actualizada != null)
                {
                    for (int i = 0; i < _empresasActuales.Count; i++)
                        if (_empresasActuales[i].emp_id_msp == actualizada.emp_id_msp)
                            _empresasActuales[i] = actualizada;
                }

                if (actualizada != null) RefrescarFilaEmpresa(actualizada);

                MarcarEstadoEmpresas("Guardado.", true);
                ActualizarContadorEmpresas();
            }
            catch (PortalApiException px)
            {
                MarcarEstadoEmpresas("Error " + (int)px.StatusCode + " del portal.", false);
                await CargarEmpresasAsync();
            }
            catch (Exception ex)
            {
                MarcarEstadoEmpresas("No se pudo guardar: " + ex.Message, false);
                await CargarEmpresasAsync();
            }
        }

        /// <summary>
        /// Localiza la fila del grid para una empresa por su EMP_ID_MSP.
        /// Devuelve null si no la encuentra (caso raro durante recarga).
        /// </summary>
        private DataGridViewRow BuscarFilaEmpresa(int idMsp)
        {
            foreach (DataGridViewRow r in dgvEmpresas.Rows)
            {
                if (ToInt(r.Cells[colEmpIdMsp.Index].Value) == idMsp) return r;
            }
            return null;
        }

        /// <summary>
        /// Nombre visual de una empresa por su EMP_ID_MSP (prefiriendo el largo).
        /// Lo extraemos del snapshot en memoria que ya está hidratado.
        /// </summary>
        private string NombreVisualEmpresa(int idMsp)
        {
            var emp = EmpresaOriginal(idMsp);
            if (emp == null) return "empresa " + idMsp;
            return !string.IsNullOrEmpty(emp.nombre_largo) ? emp.nombre_largo
                 : !string.IsNullOrEmpty(emp.nombre)       ? emp.nombre
                 : "empresa " + idMsp;
        }

        /// <summary>
        /// Decide si la configuración de Microsip fue lo suficientemente
        /// exitosa como para proceder con el PATCH al portal. Muestra al
        /// operador el diálogo apropiado en los casos de error y devuelve
        /// false para abortar la autorización.
        /// </summary>
        private bool ConfiguracionMicrosipFueExitosa(ResumenConfiguracionEmpresa r, string nombreVisual)
        {
            if (r.EmpresaNoEnConfigFdb)
            {
                MessageBox.Show(this,
                    nombreVisual + " no se encuentra en Microsip (CONFIG.FDB).\n"
                    + "¿Fue renombrada o eliminada? La autorización en el portal fue cancelada.",
                    "Configuración de Microsip",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                MarcarEstadoEmpresas(nombreVisual + ": no está en Microsip.", false);
                return false;
            }
            if (r.ConexionFallo)
            {
                MessageBox.Show(this,
                    "No se pudo conectar a la base Firebird de " + nombreVisual + ".\n"
                    + "Verifique que Microsip esté arriba y que las credenciales del registro sean correctas.\n"
                    + "La autorización en el portal fue cancelada.",
                    "Configuración de Microsip",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                MarcarEstadoEmpresas(nombreVisual + ": no se pudo abrir su base.", false);
                return false;
            }
            if (r.TablaLibresProveedorAusente)
            {
                MessageBox.Show(this,
                    "LIBRES_PROVEEDOR no existe en " + nombreVisual + ".\n"
                    + "Defina al menos un campo libre de proveedor desde Microsip antes de autorizarla en el portal.",
                    "Configuración de Microsip",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                MarcarEstadoEmpresas(nombreVisual + ": LIBRES_PROVEEDOR ausente.", false);
                return false;
            }
            if (!string.IsNullOrEmpty(r.ErrorGeneral))
            {
                MessageBox.Show(this,
                    "Hubo un error inesperado configurando " + nombreVisual + ":\n\n"
                    + r.ErrorGeneral + "\n\nLa autorización en el portal fue cancelada.",
                    "Configuración de Microsip",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                MarcarEstadoEmpresas(nombreVisual + ": error.", false);
                return false;
            }

            // Si la verificación post-ALTER detectó que ningún campo persistió,
            // también abortamos — la BD quedó como antes y nada cambió.
            if (r.CamposConError.Count > 0 && r.CamposCreados.Count == 0 && r.CamposYaExistian.Count == 0)
            {
                MessageBox.Show(this,
                    "Ningún campo libre se pudo crear en " + nombreVisual + ":\n\n"
                    + string.Join("\n", r.CamposConError)
                    + "\n\nLa autorización en el portal fue cancelada.",
                    "Configuración de Microsip",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                MarcarEstadoEmpresas(nombreVisual + ": ALTER TABLE no surtió efecto.", false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Muestra al operador qué cambió en Microsip durante la configuración
        /// de una empresa recién autorizada. Llamado SOLO después de que
        /// <see cref="ConfiguracionMicrosipFueExitosa"/> regresó true — los
        /// casos fatales (sin conexión, tabla ausente, error general) los
        /// muestra esa otra función con diálogos de error y aborta el flujo.
        /// </summary>
        private void MostrarResumenConfiguracionMicrosip(string nombreVisual, ResumenConfiguracionEmpresa r)
        {
            // Armar resumen de lo que se hizo (campos verificados post-ALTER).
            var partes = new List<string>();
            if (r.CamposCreados.Count > 0)
                partes.Add(r.CamposCreados.Count + " campo(s) en LIBRES_PROVEEDOR");
            if (r.AtributosProveedorCreados > 0)
                partes.Add(r.AtributosProveedorCreados + " cabecera(s) en ATRIBUTOS (PROVEEDOR)");
            if (r.FilasActualizadasConDefaults > 0)
                partes.Add(r.FilasActualizadasConDefaults + " fila(s) actualizada(s) con defaults");
            if (r.FolioWebCreado)
                partes.Add("folio WEB en FOLIOS_COMPRAS");
            bool usoCfdiNew = r.UsoCfdiAtributoCreado || r.UsoCfdiOpcionesCreadas > 0 || r.UsoCfdiColumnaCreada;
            if (usoCfdiNew)
            {
                var detalle = new List<string>();
                if (r.UsoCfdiAtributoCreado)      detalle.Add("cabecera ATRIBUTOS");
                if (r.UsoCfdiOpcionesCreadas > 0) detalle.Add(r.UsoCfdiOpcionesCreadas + " opciones SAT");
                if (r.UsoCfdiColumnaCreada)       detalle.Add("columna LIBRES_REC_CM.USO_CFDI");
                partes.Add("USO_CFDI: " + string.Join(" + ", detalle));
            }

            // Advertencias no fatales (campos individuales que fallaron, folio
            // WEB con problema, USO_CFDI parcial, etc.).
            var warnings = new List<string>();
            foreach (var err in r.CamposConError)             warnings.Add("• " + err);
            foreach (var err in r.AtributosProveedorConError) warnings.Add("• ATRIBUTOS " + err);
            if (!string.IsNullOrEmpty(r.DefaultsRetroactivosError))
                warnings.Add("• defaults retroactivos: " + r.DefaultsRetroactivosError);
            if (!string.IsNullOrEmpty(r.FolioWebError))       warnings.Add("• folio WEB: " + r.FolioWebError);
            if (!string.IsNullOrEmpty(r.UsoCfdiError))        warnings.Add("• USO_CFDI: " + r.UsoCfdiError);
            if (r.UsoCfdiTablaAusente)
                warnings.Add("• LIBRES_REC_CM no existe — defina primero un campo libre de recepción en Microsip.");

            // Caso "todo idempotente, nada que hacer": no molestar con diálogo.
            if (partes.Count == 0 && warnings.Count == 0)
            {
                MarcarEstadoEmpresas(nombreVisual + ": Microsip ya estaba listo.", true);
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Microsip listo para " + nombreVisual + ".");
            sb.AppendLine();
            if (partes.Count > 0)
            {
                sb.AppendLine("Se configuró en Microsip:");
                foreach (var p in partes) sb.AppendLine("  • " + p);
                sb.AppendLine();
                sb.AppendLine("Reinicie Microsip para que los cambios se vean en la pestaña \"Datos particulares\".");
            }
            else
            {
                sb.AppendLine("Microsip ya tenía todo configurado (no fue necesario crear nada).");
            }
            if (warnings.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Advertencias:");
                foreach (var w in warnings) sb.AppendLine("  " + w);
            }

            var icono = warnings.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information;
            MarcarEstadoEmpresas(nombreVisual + ": Microsip configurado.", warnings.Count == 0);
            MessageBox.Show(this, sb.ToString().TrimEnd(),
                "Configuración de Microsip",
                MessageBoxButtons.OK, icono);
        }

        private void txtBuscarEmpresa_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltroEmpresas();
        }

        /// <summary>
        /// Filtra las filas del grid según el texto en la caja de búsqueda.
        /// Coincide por nombre, razón social o RFC (case-insensitive, contains).
        /// </summary>
        private void AplicarFiltroEmpresas()
        {
            var q = (txtBuscarEmpresa.Text ?? "").Trim();
            if (q.Length == 0)
            {
                foreach (DataGridViewRow r in dgvEmpresas.Rows)
                    r.Visible = true;
                ActualizarContadorEmpresas();
                return;
            }

            // CurrentCell tiene que estar en una fila visible, si no DataGridView
            // se queja al ocultar la actual. Lo despinamos primero.
            dgvEmpresas.CurrentCell = null;

            var qLower = q.ToLowerInvariant();
            int visibles = 0;
            foreach (DataGridViewRow r in dgvEmpresas.Rows)
            {
                var nombre       = (r.Cells[colEmpNombre.Index].Value as string)       ?? "";
                var nombreLargo  = (r.Cells[colEmpNombreLargo.Index].Value as string)  ?? "";
                var rfc          = (r.Cells[colEmpRfc.Index].Value as string)          ?? "";

                bool match = nombre.ToLowerInvariant().Contains(qLower)
                          || nombreLargo.ToLowerInvariant().Contains(qLower)
                          || rfc.ToLowerInvariant().Contains(qLower);
                r.Visible = match;
                if (match) visibles++;
            }

            int autorizadas = ContarAutorizadasVisibles();
            lblContadorEmpresas.Text = visibles + " visibles · " + autorizadas + " autorizada" + (autorizadas == 1 ? "" : "s");
        }

        private void ActualizarContadorEmpresas()
        {
            int total = _empresasActuales.Count;
            int autorizadas = 0;
            foreach (var e in _empresasActuales)
                if (string.Equals(e.estatus, "Autorizada", StringComparison.Ordinal))
                    autorizadas++;

            lblContadorEmpresas.Text = total + " empresa" + (total == 1 ? "" : "s")
                + " · " + autorizadas + " autorizada" + (autorizadas == 1 ? "" : "s");
        }

        private int ContarAutorizadasVisibles()
        {
            int n = 0;
            foreach (DataGridViewRow r in dgvEmpresas.Rows)
                if (r.Visible && string.Equals(r.Cells[colEmpEstatus.Index].Value as string, "Autorizada", StringComparison.Ordinal))
                    n++;
            return n;
        }

        private static int ToInt(object o)
        {
            if (o == null) return 0;
            int n;
            return int.TryParse(o.ToString(), out n) ? n : 0;
        }

        /// <summary>
        /// Actualiza las celdas visibles de una fila con los datos frescos que
        /// regresó el server tras un PATCH. Especialmente útil para que la
        /// columna "Sincronizar desde" muestre la nueva fecha sin recargar todo
        /// el grid.
        /// </summary>
        private void RefrescarFilaEmpresa(EmpresaConfig e)
        {
            _cargandoEmpresas = true;
            try
            {
                foreach (DataGridViewRow row in dgvEmpresas.Rows)
                {
                    if (ToInt(row.Cells[colEmpIdMsp.Index].Value) != e.emp_id_msp) continue;

                    var estatus = NormalizarEstatus(e.estatus);
                    row.Cells[colEmpEstatus.Index].Value     = estatus;
                    row.Cells[colEmpDiferencia.Index].Value  = DiferenciaABool(e.diferencia);
                    row.Cells[colEmpSincDesde.Index].Value   = FormatearSincDesde(e.sinc_desde);
                    row.Cells[colEmpUltSinc.Index].Value     = FormatearUltSinc(e.ult_sinc);
                    PintarFilaSegunEstatus(row, estatus);
                    return;
                }
            }
            finally { _cargandoEmpresas = false; }
        }

        private void MarcarEstadoEmpresas(string mensaje, bool? exito)
        {
            lblEstadoEmpresas.Text = mensaje;
            if (exito == true)       lblEstadoEmpresas.ForeColor = Color.FromArgb(22, 163, 74);
            else if (exito == false) lblEstadoEmpresas.ForeColor = Color.FromArgb(220, 38, 38);
            else                     lblEstadoEmpresas.ForeColor = Color.FromArgb(100, 116, 139);
        }

        // ====================================================================
        // SECCIÓN SERVICIO WINDOWS
        // ====================================================================

        private void CargarServicioDesdeRegistro()
        {
            try
            {
                var reg = new RegistrosWindows();
                reg.LeerRegistros(RutaRegistro);
                txtServiceName.Text  = reg.SERVICE_NAME  ?? "";
                txtRutaArchivos.Text = reg.RUTA_ARCHIVOS ?? "";
            }
            catch (Exception ex)
            {
                AvisoWarning("No se pudieron leer los registros del servicio:\n\n" + ex.Message);
            }
        }

        private void btnExaminarRuta_Click(object sender, EventArgs e)
        {
            folderBrowser.Description = "Carpeta para archivos del servicio";
            if (!string.IsNullOrWhiteSpace(txtRutaArchivos.Text) && Directory.Exists(txtRutaArchivos.Text))
                folderBrowser.SelectedPath = txtRutaArchivos.Text;
            if (folderBrowser.ShowDialog(this) == DialogResult.OK)
                txtRutaArchivos.Text = folderBrowser.SelectedPath;
        }

        private void btnGuardarServicio_Click(object sender, EventArgs e)
        {
            var nombre = (txtServiceName.Text ?? "").Trim();
            var ruta   = (txtRutaArchivos.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                AvisoWarning("El nombre del servicio es obligatorio.");
                return;
            }

            int codigo = UAC.EjecutarTareaElevada(
                "guardar-servicio",
                "--service-name="  + EscaparArg(nombre),
                "--ruta-archivos=" + EscaparArg(ruta));

            ManejarResultadoElevado(codigo, "Configuración del servicio guardada.");
            RefrescarEstadoServicio();
        }

        private void btnRefrescarEstado_Click(object sender, EventArgs e)
        {
            RefrescarEstadoServicio();
        }

        /// <summary>
        /// Consulta el SCM por el servicio cuyo nombre está en <c>txtServiceName</c>
        /// y refleja el resultado en el label de estado + habilitación de los
        /// 4 botones de acción. No requiere admin (consulta es lectura).
        /// </summary>
        private void RefrescarEstadoServicio()
        {
            var nombre = (txtServiceName.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ReflejarEstado(EstadoServicio.SinNombre, "Especifica un nombre.");
                return;
            }

            try
            {
                using (var sc = new ServiceController(nombre))
                {
                    var status = sc.Status;
                    switch (status)
                    {
                        case ServiceControllerStatus.Running:
                            ReflejarEstado(EstadoServicio.Corriendo, "Corriendo");
                            break;
                        case ServiceControllerStatus.Stopped:
                            ReflejarEstado(EstadoServicio.Detenido, "Detenido");
                            break;
                        case ServiceControllerStatus.StartPending:
                            ReflejarEstado(EstadoServicio.Pendiente, "Iniciando…");
                            break;
                        case ServiceControllerStatus.StopPending:
                            ReflejarEstado(EstadoServicio.Pendiente, "Deteniendo…");
                            break;
                        case ServiceControllerStatus.Paused:
                            ReflejarEstado(EstadoServicio.Pendiente, "Pausado");
                            break;
                        default:
                            ReflejarEstado(EstadoServicio.Pendiente, status.ToString());
                            break;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // El SCM lanza esto cuando el servicio no existe.
                ReflejarEstado(EstadoServicio.NoInstalado, "No instalado");
            }
            catch (Exception ex)
            {
                ReflejarEstado(EstadoServicio.SinNombre, "Error: " + ex.Message);
            }
        }

        private enum EstadoServicio { SinNombre, NoInstalado, Detenido, Corriendo, Pendiente }

        private void ReflejarEstado(EstadoServicio estado, string textoValor)
        {
            lblEstadoActualValor.Text = "● " + textoValor;
            switch (estado)
            {
                case EstadoServicio.Corriendo:
                    lblEstadoActualValor.ForeColor = Color.FromArgb(22, 163, 74);
                    break;
                case EstadoServicio.Detenido:
                    lblEstadoActualValor.ForeColor = Color.FromArgb(220, 38, 38);
                    break;
                case EstadoServicio.NoInstalado:
                    lblEstadoActualValor.ForeColor = Color.FromArgb(100, 116, 139);
                    break;
                case EstadoServicio.Pendiente:
                    lblEstadoActualValor.ForeColor = Color.FromArgb(245, 158, 11);
                    break;
                default:
                    lblEstadoActualValor.ForeColor = Color.FromArgb(100, 116, 139);
                    break;
            }

            // Habilitación de botones según el estado real del servicio.
            // Pending → todo deshabilitado para evitar dobles acciones mientras el SCM trabaja.
            switch (estado)
            {
                case EstadoServicio.NoInstalado:
                    btnInstalarServicio.Enabled    = true;
                    btnDesinstalarServicio.Enabled = false;
                    btnIniciarServicio.Enabled     = false;
                    btnDetenerServicio.Enabled     = false;
                    break;
                case EstadoServicio.Detenido:
                    btnInstalarServicio.Enabled    = false;
                    btnDesinstalarServicio.Enabled = true;
                    btnIniciarServicio.Enabled     = true;
                    btnDetenerServicio.Enabled     = false;
                    break;
                case EstadoServicio.Corriendo:
                    btnInstalarServicio.Enabled    = false;
                    btnDesinstalarServicio.Enabled = true;
                    btnIniciarServicio.Enabled     = false;
                    btnDetenerServicio.Enabled     = true;
                    break;
                case EstadoServicio.Pendiente:
                case EstadoServicio.SinNombre:
                default:
                    btnInstalarServicio.Enabled    = false;
                    btnDesinstalarServicio.Enabled = false;
                    btnIniciarServicio.Enabled     = false;
                    btnDetenerServicio.Enabled     = false;
                    break;
            }
        }

        private void btnInstalarServicio_Click(object sender, EventArgs e)
        {
            var nombre = (txtServiceName.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre)) { AvisoWarning("Falta el nombre del servicio."); return; }

            int codigo = UAC.EjecutarTareaElevada("instalar-servicio", "--service-name=" + EscaparArg(nombre));
            ManejarResultadoElevado(codigo, "Servicio instalado correctamente.");
            RefrescarEstadoServicio();
        }

        private void btnDesinstalarServicio_Click(object sender, EventArgs e)
        {
            var nombre = (txtServiceName.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre)) { AvisoWarning("Falta el nombre del servicio."); return; }

            var conf = MessageBox.Show(
                "¿Desinstalar el servicio \"" + nombre + "\"?\n\n" +
                "Si está corriendo será detenido primero.",
                "Configurador",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (conf != DialogResult.Yes) return;

            int codigo = UAC.EjecutarTareaElevada("desinstalar-servicio", "--service-name=" + EscaparArg(nombre));
            ManejarResultadoElevado(codigo, "Servicio desinstalado.");
            RefrescarEstadoServicio();
        }

        private void btnIniciarServicio_Click(object sender, EventArgs e)
        {
            var nombre = (txtServiceName.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre)) { AvisoWarning("Falta el nombre del servicio."); return; }

            int codigo = UAC.EjecutarTareaElevada("iniciar-servicio", "--service-name=" + EscaparArg(nombre));
            ManejarResultadoElevado(codigo, "Servicio iniciado.");
            RefrescarEstadoServicio();
        }

        private void btnDetenerServicio_Click(object sender, EventArgs e)
        {
            var nombre = (txtServiceName.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre)) { AvisoWarning("Falta el nombre del servicio."); return; }

            int codigo = UAC.EjecutarTareaElevada("detener-servicio", "--service-name=" + EscaparArg(nombre));
            ManejarResultadoElevado(codigo, "Servicio detenido.");
            RefrescarEstadoServicio();
        }

        // ====================================================================
        // OTROS — Comportamiento (HKLM)
        // ====================================================================

        private void CargarHKLMTimerYCorreo()
        {
            try
            {
                var reg = new RegistrosWindows();
                reg.LeerRegistros(RutaRegistro);

                int totalSeg;
                if (!int.TryParse(reg.MODE_TIMER, out totalSeg) || totalSeg <= 0)
                    totalSeg = 60;

                int valor; string unidad;
                SegundosAUnidad(totalSeg, out valor, out unidad);
                cmbUnidadTimer.SelectedItem = unidad;
                if (cmbUnidadTimer.SelectedIndex < 0) cmbUnidadTimer.SelectedIndex = 0;
                nudTimer.Value = Math.Max(nudTimer.Minimum, Math.Min(nudTimer.Maximum, valor));

                chkEnviarCorreo.Checked = string.Equals(
                    reg.ENVIAR_CORREO_COMPRAS, "True", StringComparison.OrdinalIgnoreCase);

                ActualizarHelperTimer();
                nudTimer.ValueChanged    += new EventHandler(this.nudTimer_ValueChanged);
                cmbUnidadTimer.SelectedIndexChanged += new EventHandler(this.cmbUnidadTimer_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                AvisoWarning("No se pudieron leer los registros HKLM:\n\n" + ex.Message);
            }
        }

        private void nudTimer_ValueChanged(object sender, EventArgs e)            { ActualizarHelperTimer(); }
        private void cmbUnidadTimer_SelectedIndexChanged(object sender, EventArgs e) { ActualizarHelperTimer(); }

        private void ActualizarHelperTimer()
        {
            var total = UnidadASegundos((int)nudTimer.Value, cmbUnidadTimer.SelectedItem as string);
            lblTimerHelper.Text = "(= " + total.ToString("N0") + " segundos)";
        }

        private void btnGuardarHKLM_Click(object sender, EventArgs e)
        {
            var segundos     = UnidadASegundos((int)nudTimer.Value, cmbUnidadTimer.SelectedItem as string);
            var enviarCorreo = chkEnviarCorreo.Checked ? "True" : "False";

            int codigo = UAC.EjecutarTareaElevada(
                "guardar-otros-hklm",
                "--mode-timer=" + segundos,
                "--enviar-correo-compras=" + enviarCorreo);

            ManejarResultadoElevado(codigo, "Comportamiento del servicio guardado.");
        }

        // ====================================================================
        // OTROS — Parámetros del portal (REST)
        // ====================================================================

        private async Task CargarParametrosAsync()
        {
            HabilitarBotonGuardarParametros(false);
            try
            {
                var api = ConstruirApi();
                if (api == null) return;

                var lista = await api.ListarParametrosAsync(CancellationToken.None);
                _parametrosOriginales = lista;

                dgvParametros.Rows.Clear();
                foreach (var p in lista)
                {
                    int idx = dgvParametros.Rows.Add(p.clave, p.descripcion, p.valor);
                    if (EsAutoManaged(p.clave))
                    {
                        dgvParametros.Rows[idx].ReadOnly = true;
                        dgvParametros.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                        dgvParametros.Rows[idx].DefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139);
                    }
                }
            }
            catch (PortalApiException px)
            {
                AvisoError("El portal respondió con error:\n\n" + px.Message + "\n\n" + px.Cuerpo);
            }
            catch (Exception ex)
            {
                AvisoError("No se pudieron cargar los parámetros:\n\n" + ex.Message);
            }
            finally
            {
                HabilitarBotonGuardarParametros(true);
            }
        }

        private async void btnGuardarParametros_Click(object sender, EventArgs e)
        {
            if (_parametrosOriginales == null) return;

            var cambios = ColectarCambios();
            if (cambios.Count == 0)
            {
                AvisoInfo("No hay cambios para guardar.");
                return;
            }

            HabilitarBotonGuardarParametros(false);
            try
            {
                var api = ConstruirApi();
                if (api == null) return;

                var resultado = await api.ActualizarParametrosAsync(cambios, CancellationToken.None);
                _parametrosOriginales = new List<ParametroPortal>(resultado.parametros);

                var msg = "Cambios aplicados: " + resultado.resumen.aplicados;
                if (resultado.resumen.ignorados_auto != null && resultado.resumen.ignorados_auto.Length > 0)
                    msg += "\nProtegidos (no se tocan): " + string.Join(", ", resultado.resumen.ignorados_auto);
                if (resultado.resumen.no_encontrados != null && resultado.resumen.no_encontrados.Length > 0)
                    msg += "\nNo encontrados: " + string.Join(", ", resultado.resumen.no_encontrados);
                AvisoInfo(msg);

                await CargarParametrosAsync();
            }
            catch (PortalApiException px)
            {
                AvisoError("El portal respondió con error:\n\n" + px.Message + "\n\n" + px.Cuerpo);
            }
            catch (Exception ex)
            {
                AvisoError("No se pudieron guardar los parámetros:\n\n" + ex.Message);
            }
            finally
            {
                HabilitarBotonGuardarParametros(true);
            }
        }

        private List<ParametroPortal> ColectarCambios()
        {
            var cambios = new List<ParametroPortal>();
            foreach (DataGridViewRow row in dgvParametros.Rows)
            {
                var clave = row.Cells[colClave.Index].Value as string;
                var valor = row.Cells[colValor.Index].Value as string;
                if (string.IsNullOrEmpty(clave)) continue;
                if (EsAutoManaged(clave))         continue;

                ParametroPortal original = null;
                foreach (var p in _parametrosOriginales)
                {
                    if (p.clave == clave) { original = p; break; }
                }
                if (original == null) continue;
                if (string.Equals(original.valor ?? "", valor ?? "", StringComparison.Ordinal)) continue;

                cambios.Add(new ParametroPortal { clave = clave, valor = valor ?? "" });
            }
            return cambios;
        }

        // ====================================================================
        // Helpers
        // ====================================================================

        private IPortalApi ConstruirApi()
        {
            var reg = new RegistrosWindows();
            reg.LeerRegistros(RutaRegistro);

            if (string.IsNullOrWhiteSpace(reg.PORTAL_BASE_URL) ||
                string.IsNullOrWhiteSpace(reg.PORTAL_API_KEY))
            {
                AvisoWarning("Configura la URL y el API Key del portal en la sección 'Portal Web' antes de usar esta operación.");
                return null;
            }

            return new PortalApi(reg.PORTAL_BASE_URL, reg.PORTAL_API_KEY);
        }

        private void HabilitarBotonGuardarParametros(bool habilitado)
        {
            btnGuardarParametros.Enabled = habilitado;
            UseWaitCursor = !habilitado;
        }

        private static bool EsAutoManaged(string clave)
        {
            return string.Equals(clave, "LAST_UPDATE", StringComparison.Ordinal);
        }

        private void ManejarResultadoElevado(int codigo, string mensajeExito)
        {
            switch (codigo)
            {
                case 0:  AvisoInfo(mensajeExito); break;
                case -1: /* el usuario canceló UAC — sin ruido */ break;
                default: AvisoError("La operación elevada terminó con código " + codigo + "."); break;
            }
        }

        private static string EscaparArg(string v)
        {
            // Los args van por linea de comandos: si contienen espacios, comillas
            // o &, el shell los rompería. Encerramos con comillas y escapamos
            // las comillas internas.
            if (v == null) return "\"\"";
            var contieneEspecial =
                v.IndexOfAny(new[] { ' ', '\t', '"', '&', '|', '<', '>' }) >= 0;
            if (!contieneEspecial) return v;
            return "\"" + v.Replace("\"", "\\\"") + "\"";
        }

        // === Conversión seg ↔ unidad humana ==================================

        private static void SegundosAUnidad(int totalSegundos, out int valor, out string unidad)
        {
            if (totalSegundos > 0 && totalSegundos % 86400 == 0) { valor = totalSegundos / 86400; unidad = "Días";    return; }
            if (totalSegundos > 0 && totalSegundos % 3600  == 0) { valor = totalSegundos / 3600;  unidad = "Horas";   return; }
            if (totalSegundos > 0 && totalSegundos % 60    == 0) { valor = totalSegundos / 60;    unidad = "Minutos"; return; }
            valor = totalSegundos;
            unidad = "Segundos";
        }

        private static int UnidadASegundos(int valor, string unidad)
        {
            switch (unidad)
            {
                case "Días":    return valor * 86400;
                case "Horas":   return valor * 3600;
                case "Minutos": return valor * 60;
                default:        return valor;
            }
        }

        // === Tooltips y avisos ===============================================

        private void ConfigurarTooltips()
        {
            toolTip.SetToolTip(this.btnProbarMicrosip,
                "Intenta abrir la base CONFIG.FDB con los valores escritos en este formulario (sin guardar).");
            toolTip.SetToolTip(this.btnGuardarMicrosip,
                "Guarda estos valores en el registro de Windows. Solicita permisos de administrador.");
            toolTip.SetToolTip(this.btnProbarPortal,
                "Llama GET /api/portal-config con los valores escritos en este formulario (sin guardar) y reporta si conectó.");
            toolTip.SetToolTip(this.btnGuardarPortal,
                "Guarda la URL y el API Key en el registro de Windows. Solicita permisos de administrador.");
            toolTip.SetToolTip(this.btnPortalToggle,
                "Alterna entre mostrar y ocultar el API Key.");
            toolTip.SetToolTip(this.btnRefrescarEstado,
                "Vuelve a consultar el estado del servicio en Windows.");
            toolTip.SetToolTip(this.btnInstalarServicio,
                "Registra el servicio en Windows con sc.exe create. Solicita permisos de administrador.");
            toolTip.SetToolTip(this.btnDesinstalarServicio,
                "Elimina el servicio del registro de Windows. Solicita permisos de administrador.");
            toolTip.SetToolTip(this.btnIniciarServicio,
                "Arranca el servicio Windows. Solicita permisos de administrador.");
            toolTip.SetToolTip(this.btnDetenerServicio,
                "Detiene el servicio Windows. Solicita permisos de administrador.");
            toolTip.SetToolTip(this.btnGuardarHKLM,
                "Guarda el comportamiento del servicio en el registro de Windows. Solicita permisos de administrador.");
            toolTip.SetToolTip(this.btnGuardarParametros,
                "Envía solo las filas modificadas al portal. Las filas protegidas (en gris) no se mandan.");
        }

        private void AvisoInfo(string msg)
        {
            MessageBox.Show(msg, "Configurador", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AvisoWarning(string msg)
        {
            MessageBox.Show(msg, "Configurador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void AvisoError(string msg)
        {
            MessageBox.Show(msg, "Configurador", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
