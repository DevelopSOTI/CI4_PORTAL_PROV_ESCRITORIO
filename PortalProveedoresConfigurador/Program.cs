using System;
using System.Windows.Forms;
using PortalProveedoresConfigurador.Configuracion;
using PortalProveedoresConfigurador.Formularios;
using PortalProveedoresConfigurador.Tareas;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Servicios;

namespace PortalProveedoresConfigurador
{
    /// <summary>
    /// Entry point del Configurador. Dos modos según los argumentos:
    ///
    ///   1) Modo UI (sin args)        — abre el form principal, NO solicita admin.
    ///                                   Cuando un botón requiere elevación, la UI
    ///                                   llama a UAC.EjecutarTareaElevada("...") que
    ///                                   relanza este mismo EXE en "modo tarea".
    ///
    ///   2) Modo tarea (--task=...)   — ejecuta UNA tarea elevada y termina con
    ///                                   un ExitCode. No abre la UI; suele venir ya
    ///                                   elevado por UAC (Verb=runas).
    ///
    /// Convenciones del ExitCode (ver Configuracion/UAC.cs):
    ///   0 → OK · 1 → error · 2 → tarea desconocida.
    /// </summary>
    internal static class Program
    {
        private const string ArgTaskPrefix = "--task=";

        [STAThread]
        static int Main(string[] argv)
        {
            // Modos headless de "probar conexión" para el instalador. Devuelven
            // exit code (0=OK, 1=falló, 2=args inválidos) y NO muestran UI — el
            // instalador (Inno Setup) los invoca con Exec y lee el ResultCode.
            // Como el instalador ya corre elevado, lanzar este EXE
            // (requireAdministrator) NO dispara un UAC adicional.
            if (argv != null && argv.Length > 0)
            {
                var modo = (argv[0] ?? "").ToLowerInvariant();
                if (modo == "--probar-portal")    return ProbarPortalHeadless(argv);
                if (modo == "--probar-microsip")  return ProbarMicrosipHeadless(argv);
            }

            var tarea = ExtraerTarea(argv);
            if (tarea != null)
                return EjecutarTareaElevada(tarea, argv);

            // Modo UI normal.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Precarga del tema (PORTAL_CONFIG) antes de abrir el form. Si el
            // portal está offline o el HKLM no está configurado todavía, Tema
            // queda con sus defaults y el form muestra "Modo offline".
            PrecargarTema();

            // Migra a cifrado cualquier secreto que el instalador haya dejado en
            // texto plano. El Configurador corre elevado (requireAdministrator),
            // así que aquí la migración es confiable — cubre instalaciones cliente
            // que no tienen el Servicio. Best-effort: nunca bloquea la apertura.
            try { new PortalProveedoresCore.Configuracion.RegistrosWindows().MigrarSecretosACifrado(); } catch { }

            Application.Run(new FormPrincipal());
            return 0;
        }

        /// <summary>
        /// Lee URL/API key del registro y, si ambos están presentes, intenta
        /// cargar el tema con un timeout corto (3 segundos). Cualquier error
        /// se silencia — los defaults son suficientes para usar la app.
        /// </summary>
        private static void PrecargarTema()
        {
            try
            {
                var reg = new RegistrosWindows();
                if (!reg.LeerRegistros(@"SOFTWARE\SOTI\Service Portal")) return;
                if (string.IsNullOrWhiteSpace(reg.PORTAL_BASE_URL) ||
                    string.IsNullOrWhiteSpace(reg.PORTAL_API_KEY)) return;

                IPortalApi api = new PortalApi(reg.PORTAL_BASE_URL, reg.PORTAL_API_KEY);

                // Bloquea aquí (estamos antes de Application.Run, no hay UI
                // todavía). El timeout vive adentro de CargarConTimeoutAsync.
                Tema.CargarConTimeoutAsync(api, TimeSpan.FromSeconds(3))
                    .GetAwaiter().GetResult();
            }
            catch
            {
                // No-op: Tema queda con defaults + ModoOffline = true.
            }
        }

        /// <summary>
        /// <c>--probar-portal &lt;baseUrl&gt; &lt;apiKey&gt;</c> — prueba que la URL del
        /// portal sea alcanzable y la API key válida. Exit 0 = OK, 1 = no
        /// conectó, 2 = faltan argumentos.
        /// </summary>
        private static int ProbarPortalHeadless(string[] argv)
        {
            if (argv.Length < 3) return 2;
            var baseUrl = LimpiarArg(argv[1]);
            var apiKey  = LimpiarArg(argv[2]);
            if (baseUrl.Length == 0 || apiKey.Length == 0) return 2;
            try
            {
                IPortalApi api = new PortalApi(baseUrl, apiKey);
                bool ok = api.ProbarConexionAsync(System.Threading.CancellationToken.None)
                             .GetAwaiter().GetResult();
                return ok ? 0 : 1;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// <c>--probar-microsip &lt;servidor&gt; &lt;root&gt; &lt;usuario&gt; [pass]</c> — prueba
        /// la conexión a CONFIG.FDB de Microsip con esas credenciales (sin leer
        /// el registro). Exit 0 = OK, 1 = no conectó, 2 = faltan argumentos.
        ///
        /// El password es OPCIONAL (hay instalaciones con pass vacío — mismo
        /// criterio que TareasElevadas.GuardarMicrosip). Los argumentos se
        /// sanean con Trim de espacios y de comillas sueltas: si el llamador
        /// armó mal el quoting (p. ej. una ruta terminada en '\' que escapa la
        /// comilla de cierre y fusiona argumentos), preferimos limpiar lo
        /// recuperable antes que fallar con "faltan argumentos".
        /// </summary>
        private static int ProbarMicrosipHeadless(string[] argv)
        {
            if (argv.Length < 4) return 2;
            var servidor = LimpiarArg(argv[1]);
            var root     = LimpiarArg(argv[2]);
            var usuario  = LimpiarArg(argv[3]);
            var pass     = argv.Length >= 5 ? LimpiarArg(argv[4]) : "";

            if (servidor.Length == 0 || root.Length == 0 || usuario.Length == 0)
                return 2;

            try
            {
                string msg;
                var con = new ConexionMicrosip();
                bool ok = con.ConectarConfigPrueba(servidor, root, usuario, pass, out msg);
                con.Desconectar();
                return ok ? 0 : 1;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Sanea un argumento de línea de comandos: quita espacios y comillas
        /// dobles residuales de un quoting mal armado por el llamador.
        /// </summary>
        private static string LimpiarArg(string s)
        {
            return (s ?? "").Trim().Trim('"').Trim();
        }

        /// <summary>Busca el primer argumento con prefijo --task= y devuelve su valor, o null.</summary>
        private static string ExtraerTarea(string[] argv)
        {
            if (argv == null) return null;
            foreach (var a in argv)
                if (a != null && a.StartsWith(ArgTaskPrefix, StringComparison.OrdinalIgnoreCase))
                    return a.Substring(ArgTaskPrefix.Length);
            return null;
        }

        /// <summary>
        /// Dispatcher de tareas elevadas. Cada acción del Configurador que requiera
        /// admin tendrá su entrada en este switch. Hoy está vacío de implementaciones
        /// — los handlers se irán cableando junto con los botones que las invocan
        /// (guardar registros, instalar/desinstalar/iniciar/detener servicio, etc.).
        /// </summary>
        private static int EjecutarTareaElevada(string tarea, string[] argv)
        {
            try
            {
                switch ((tarea ?? string.Empty).ToLowerInvariant())
                {
                    case "guardar-otros-hklm":
                        return TareasElevadas.GuardarOtrosHKLM(argv);

                    case "guardar-microsip":
                        return TareasElevadas.GuardarMicrosip(argv);

                    case "guardar-portal":
                        return TareasElevadas.GuardarPortal(argv);

                    case "guardar-servicio":
                        return TareasElevadas.GuardarServicio(argv);

                    case "instalar-servicio":
                        return TareasElevadas.InstalarServicio(argv);

                    case "desinstalar-servicio":
                        return TareasElevadas.DesinstalarServicio(argv);

                    case "iniciar-servicio":
                        return TareasElevadas.IniciarServicio(argv);

                    case "detener-servicio":
                        return TareasElevadas.DetenerServicio(argv);

                    default:
                        MessageBox.Show(
                            "Tarea elevada desconocida: '" + tarea + "'.",
                            "PortalProveedoresConfigurador",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return 2;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "La tarea elevada '" + tarea + "' falló:\n\n" + ex.Message,
                    "PortalProveedoresConfigurador",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return 1;
            }
        }
    }
}
