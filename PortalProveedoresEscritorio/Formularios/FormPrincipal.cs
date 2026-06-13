using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Servicios;
using PortalProveedoresEscritorio.Servicios;
using PortalProveedoresEscritorio.Utilidades;
using PortalProveedoresEscritorio.Vistas;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Ventana principal de la app de revisión. Estructura tipo "shell":
    /// header arriba, columna de tabs a la izquierda, panel de contenido al
    /// centro. Cada tab carga su <see cref="UserControl"/> lazy (la primera
    /// vez que el operador hace click).
    ///
    /// Réplica funcional de <c>F_PRINCIPAL.cs</c> del SOAP legacy, con
    /// estética modernizada (sin colores hardcoded a fragmentos de cyan;
    /// usamos paleta del Configurador).
    /// </summary>
    public partial class FormPrincipal : Form
    {
        private readonly string                       _usuario;
        private readonly string                       _password;
        private readonly IReadOnlyList<EmpresaEscritorio> _empresas;
        private readonly IPortalApi                   _api;

        private EmpresaEscritorio _empresaActual;

        // Cache de UserControls cargados — se crean lazy en el primer click
        // y se mantienen vivos para que la siguiente visita sea instantánea.
        private readonly Dictionary<Tab, UserControl> _vistas = new Dictionary<Tab, UserControl>();
        private Tab _tabActivo = Tab.Ninguno;

        private enum Tab { Ninguno, Facturas, Complementos, Descargas, Proveedores }

        public FormPrincipal(string usuario, string password,
                             IReadOnlyList<EmpresaEscritorio> empresas,
                             EmpresaEscritorio empresaInicial,
                             IPortalApi api)
        {
            _usuario       = usuario       ?? "";
            _password      = password      ?? "";
            _empresas      = empresas      ?? new List<EmpresaEscritorio>(0);
            _empresaActual = empresaInicial;
            _api           = api;

            InitializeComponent();
            AplicarTema();

            // MEJORA 1 — estado actual del toggle "Usar colores del portal"
            // (default true = comportamiento de siempre).
            mnuHerramientas_ColoresPortal.Checked = PreferenciasUsuario.LeerBool(
                PreferenciasUsuario.SubseccionTema,
                PreferenciasUsuario.ClaveRespetarColores,
                valorDefault: true);

            this.Load  += FormPrincipal_Load;
        }

        /// <summary>
        /// Sobrescribe los colores literales del Designer con los del Tema
        /// del portal. También asigna el logo del cliente si vino del portal.
        /// </summary>
        private void AplicarTema()
        {
            this.Text                  = Tema.NombreApp;
            this.menuPrincipal.BackColor = Tema.Secondary;
            this.panelHeader.BackColor   = Tema.Secondary;
            this.panelSidebar.BackColor  = Tema.Secondary;

            if (Tema.Logo != null)
                this.picHeaderLogo.Image = Tema.Logo;

            this.lblHeaderTitulo.Text = Tema.NombreApp;
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            ActualizarHeader();
            ActualizarHabilitadoTabs();
        }

        private void ActualizarHeader()
        {
            var empresaTxt = _empresaActual != null
                ? _empresaActual.NombreCorto
                : "(sin empresa)";
            lblHeaderEmpresa.Text = "Usuario: " + _usuario + "  ·  Empresa: " + empresaTxt;
        }

        private void ActualizarHabilitadoTabs()
        {
            bool hayEmpresa = _empresaActual != null;
            btnTabFacturas.Enabled     = hayEmpresa;
            btnTabComplementos.Enabled = hayEmpresa;
            btnTabDescargas.Enabled    = hayEmpresa;
            btnTabProveedores.Enabled  = hayEmpresa;
        }

        // ====================================================================
        // Selección de empresa
        // ====================================================================

        private void mnuArchivo_SeleccionarEmpresa_Click(object sender, EventArgs e)
        {
            PedirSeleccionEmpresa(obligatoria: false);
        }

        private void PedirSeleccionEmpresa(bool obligatoria)
        {
            using (var dlg = new FormSelectorEmpresa(_empresas))
            {
                var r = dlg.ShowDialog(this);
                if (r == DialogResult.OK && dlg.EmpresaSeleccionada != null)
                {
                    var anterior = _empresaActual;
                    _empresaActual = dlg.EmpresaSeleccionada;
                    ActualizarHeader();
                    ActualizarHabilitadoTabs();

                    // Si cambió la empresa, descartamos las vistas cacheadas
                    // (los datos son por empresa, no podemos reusar).
                    if (anterior == null || anterior.Id != _empresaActual.Id)
                    {
                        DescartarVistasCacheadas();
                    }
                    return;
                }

                if (obligatoria && _empresaActual == null)
                {
                    // Textos literales del SOAP legacy (F_PRINCIPAL.cs:70-83 y
                    // 290-302). Incluye la falta de tilde en "podra" y la
                    // frase "con la desea trabajar" tal como están en el
                    // original — son los mensajes a los que los operadores
                    // están acostumbrados.
                    if (MessageBox.Show(
                        "No ha seleccionado la empresa, el sistema no podra funcionar.\n\n"
                        + "¿Quiere seleccionar la empresa con la desea trabajar?",
                        "No hay selección",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        PedirSeleccionEmpresa(obligatoria: true);
                    }
                    else
                    {
                        MessageBox.Show(
                            "No ha seleccionado la empresa, el sistema no podra funcionar.\r\n",
                            "No hay selección",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ActualizarHabilitadoTabs();
                    }
                }
            }
        }

        private void DescartarVistasCacheadas()
        {
            foreach (var kv in _vistas)
                kv.Value.Dispose();
            _vistas.Clear();
            panelContenido.Controls.Clear();
            _tabActivo = Tab.Ninguno;
            ResaltarBotonTab();
        }

        // ====================================================================
        // Cambio de tab
        // ====================================================================

        private void btnTabFacturas_Click(object sender, EventArgs e)     => CambiarA(Tab.Facturas);
        private void btnTabComplementos_Click(object sender, EventArgs e) => CambiarA(Tab.Complementos);
        private void btnTabDescargas_Click(object sender, EventArgs e)    => CambiarA(Tab.Descargas);
        private void btnTabProveedores_Click(object sender, EventArgs e)  => CambiarA(Tab.Proveedores);

        private void CambiarA(Tab tab)
        {
            if (_empresaActual == null) return;
            if (_tabActivo == tab) return;

            UserControl vista;
            if (!_vistas.TryGetValue(tab, out vista))
            {
                vista = ConstruirVista(tab);
                vista.Dock = DockStyle.Fill;
                _vistas[tab] = vista;
            }

            panelContenido.Controls.Clear();
            panelContenido.Controls.Add(vista);
            _tabActivo = tab;
            ResaltarBotonTab();
        }

        /// <summary>
        /// Construye la UserControl real para cada tab. Las vistas todavía
        /// no implementadas devuelven un placeholder con texto.
        /// </summary>
        private UserControl ConstruirVista(Tab tab)
        {
            switch (tab)
            {
                case Tab.Facturas:
                    return new VistaFacturas(_api, _empresaActual, _usuario, _password);

                case Tab.Complementos:
                    return new VistaComplementos(_api, _empresaActual, _usuario, _password);

                case Tab.Proveedores:
                    return new VistaProveedores(_api, _empresaActual);

                case Tab.Descargas:
                    return new VistaDescargar(_api, _empresaActual);

                default:
                    var uc = new UserControl { Dock = DockStyle.Fill };
                    var lbl = new Label
                    {
                        Dock      = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.FromArgb(100, 120, 140),
                        Font      = new Font("Segoe UI", 14F),
                        Text      = "Vista \"" + tab + "\" — pendiente (fase E."
                                  + ((int) tab) + ")",
                    };
                    uc.Controls.Add(lbl);
                    return uc;
            }
        }

        private void ResaltarBotonTab()
        {
            // Activo: color primario del tema (con texto blanco para contraste).
            // Reposo: transparente sobre el sidebar oscuro, texto tenue.
            Color activoBg = Tema.Primary;
            Color activoFg = Color.White;
            Color reposoBg = Color.Transparent;
            Color reposoFg = Color.FromArgb(200, 215, 230);

            // Hover: tonos OSCUROS. El default de WinForms ACLARA el fondo al
            // pasar el mouse, y como el texto es casi blanco se perdía. Aquí el
            // hover se mantiene oscuro (un realce sutil sobre el sidebar /
            // primario) para que el texto siga legible.
            Color activoHover = Aclarar(Tema.Primary,   0.14);   // primario un poco más brillante
            Color reposoHover = Aclarar(Tema.Secondary, 0.14);   // realce sutil sobre el sidebar oscuro

            AplicarEstiloTab(btnTabFacturas,     _tabActivo == Tab.Facturas,     activoBg, activoFg, reposoBg, reposoFg, activoHover, reposoHover);
            AplicarEstiloTab(btnTabComplementos, _tabActivo == Tab.Complementos, activoBg, activoFg, reposoBg, reposoFg, activoHover, reposoHover);
            AplicarEstiloTab(btnTabDescargas,    _tabActivo == Tab.Descargas,    activoBg, activoFg, reposoBg, reposoFg, activoHover, reposoHover);
            AplicarEstiloTab(btnTabProveedores,  _tabActivo == Tab.Proveedores,  activoBg, activoFg, reposoBg, reposoFg, activoHover, reposoHover);
        }

        /// <summary>
        /// Aplica color de fondo/texto y, sobre todo, el color de hover/clic
        /// (FlatAppearance) a un botón del sidebar según esté activo o en reposo.
        /// </summary>
        private static void AplicarEstiloTab(Button b, bool activo,
                                             Color activoBg, Color activoFg,
                                             Color reposoBg, Color reposoFg,
                                             Color activoHover, Color reposoHover)
        {
            b.BackColor = activo ? activoBg : reposoBg;
            b.ForeColor = activo ? activoFg : reposoFg;
            b.FlatAppearance.MouseOverBackColor = activo ? activoHover : reposoHover;
            b.FlatAppearance.MouseDownBackColor = activo ? activoHover : reposoHover;
        }

        /// <summary>
        /// Mezcla un color hacia el blanco en <paramref name="factor"/> (0..1),
        /// subiendo el brillo sin cambiar mucho el tono. Para un color con alfa 0
        /// (transparente) devuelve el resultado opaco — el hover debe ser sólido.
        /// </summary>
        private static Color Aclarar(Color c, double factor)
        {
            int r = (int) (c.R + (255 - c.R) * factor);
            int g = (int) (c.G + (255 - c.G) * factor);
            int b = (int) (c.B + (255 - c.B) * factor);
            return Color.FromArgb(255, Clamp(r), Clamp(g), Clamp(b));
        }

        private static int Clamp(int v) { return v < 0 ? 0 : (v > 255 ? 255 : v); }

        // ====================================================================
        // Otros menús
        // ====================================================================

        /// <summary>
        /// Candado SYSDBA para abrir el Configurador desde el Escritorio:
        /// 1) pide la contraseña de SYSDBA de Firebird en un modal;
        /// 2) la valida conectando a CONFIG.FDB con usuario fijo "SYSDBA"
        ///    (ConectarConfigPrueba NO lee el registro — perfecto para validar
        ///    credenciales nuevas);
        /// 3) solo si valida, lanza el Configurador (UseShellExecute=true para
        ///    que respete su manifest / elevación por-tarea).
        ///
        /// El candado vive aquí, en el Escritorio. El Configurador en sí no
        /// cambia (sigue asInvoker + UAC por-tarea).
        /// </summary>
        private void mnuHerramientas_AbrirConfigurador_Click(object sender, EventArgs e)
        {
            // 1) Pedir la contraseña. Si cancela, no hacemos nada.
            string passwordIngresada;
            using (var dlg = new FormPasswordSysdba())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                passwordIngresada = dlg.Password ?? "";
            }

            // 2) Leer servidor/ruta de Firebird desde HKLM (usar las PROPIEDADES,
            //    no los nombres de valor del registro).
            var reg = new RegistrosWindows();
            reg.LeerRegistros(@"SOFTWARE\SOTI\Service Portal");

            // 3) Validar la contraseña conectando a CONFIG.FDB con usuario fijo SYSDBA.
            string msg;
            bool ok = new ConexionMicrosip().ConectarConfigPrueba(
                reg.MICRO_SERVER, reg.MICRO_ROOT, "SYSDBA", passwordIngresada, out msg);

            if (!ok)
            {
                MessageBox.Show(this,
                    "Contraseña de SYSDBA incorrecta o no se pudo conectar a Firebird.\n\n" + msg,
                    "Acceso al Configurador",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // 4) Validó: localizar y lanzar el Configurador.
            var exe = LocalizarConfiguradorExe();
            if (exe == null)
            {
                MessageBox.Show(this,
                    "No se encontró 'PortalProveedoresConfigurador.exe'.\n\n" +
                    "Busqué junto al ejecutable del Escritorio y en la carpeta hermana " +
                    "del proyecto Configurador (escenario de desarrollo).",
                    "Configurador no disponible",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // UseShellExecute=true: que respete el manifest y la elevación
                // por-tarea propia del Configurador.
                Process.Start(new ProcessStartInfo { FileName = exe, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el Configurador: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Devuelve la ruta al EXE del Configurador. Busca primero junto al
        /// ejecutable del Escritorio (instalación de producción) y, como
        /// fallback de desarrollo (F5 desde VS), en la carpeta hermana
        /// <c>..\..\..\PortalProveedoresConfigurador\bin\{Debug,Release}\</c>.
        /// Replica el patrón de TareasElevadas.LocalizarServicioExe.
        /// </summary>
        private static string LocalizarConfiguradorExe()
        {
            var dirEscritorio = Path.GetDirectoryName(Application.ExecutablePath);
            const string nombreExe = "PortalProveedoresConfigurador.exe";

            // 1) Misma carpeta — instalación de producción.
            var p1 = Path.Combine(dirEscritorio, nombreExe);
            if (File.Exists(p1)) return p1;

            // 2) Carpeta hermana del proyecto Configurador — desarrollo desde VS.
            //    Escritorio\bin\Debug → ..\..\..\PortalProveedoresConfigurador\bin\<Config>
            try
            {
                var subir3 = Path.GetFullPath(Path.Combine(dirEscritorio, "..", "..", ".."));
                foreach (var conf in new[] { "Debug", "Release" })
                {
                    var p2 = Path.Combine(subir3, "PortalProveedoresConfigurador", "bin", conf, nombreExe);
                    if (File.Exists(p2)) return p2;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// MEJORA 1 — toggle "Usar colores del portal". Cuando se desmarca,
        /// la app vuelve a su paleta default propia (azul/slate); el logo y
        /// el nombre del portal se conservan. El tema se aplica al construir
        /// cada Form/Vista, por lo que el cambio surte efecto al reiniciar
        /// (lo robusto y simple gana sobre re-tematizar la app en vivo).
        /// </summary>
        private void mnuHerramientas_ColoresPortal_Click(object sender, EventArgs e)
        {
            PreferenciasUsuario.EscribirBool(
                PreferenciasUsuario.SubseccionTema,
                PreferenciasUsuario.ClaveRespetarColores,
                mnuHerramientas_ColoresPortal.Checked);

            MessageBox.Show(this,
                "El cambio se aplicará al reiniciar la aplicación.",
                "Colores del portal",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void mnuArchivo_CerrarSesion_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK; // FormLogin lo reabre
            this.Close();
        }

        private void mnuArchivo_Salir_Click(object sender, EventArgs e) => Application.Exit();
    }
}
