using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Servicios;
using PortalProveedoresVisor.Configuracion;
using PortalProveedoresVisor.Formularios;

namespace PortalProveedoresVisor
{
    /// <summary>
    /// Punto de entrada del Visor. Dos modos según los argumentos:
    ///   - <c>(sin args)</c>           — abre la ventana visible y enfocada.
    ///   - <c>--tray</c>               — arranca oculto en la bandeja del sistema.
    ///                                   Se usa al iniciar sesión (HKCU\…\Run lo
    ///                                   manda con esta bandera para no molestar
    ///                                   al usuario que acaba de entrar).
    ///
    /// En cada arranque se auto-registra (idempotente) en HKCU\…\Run para que
    /// el siguiente login también lo prenda. Si el usuario desactiva el
    /// auto-arranque (futuro: checkbox en preferencias), se llama a
    /// <see cref="AutoArranque.Desregistrar"/>.
    ///
    /// Single-instance: usamos un Mutex con nombre simple (per-sesión por
    /// default en Windows actual). Si ya hay un visor corriendo en esta sesión,
    /// la segunda invocación sale en silencio — evita ventanas duplicadas y
    /// múltiples clientes peleándose por la única conexión que admite el pipe.
    /// </summary>
    internal static class Program
    {
        private const string ArgTray    = "--tray";
        private const string NombreMutex = "PortalProveedoresVisor_SingleInstance";

        [STAThread]
        static void Main(string[] argv)
        {
            // Handlers globales de excepciones no controladas: ANTES de tocar
            // WinForms para que apliquen incluso durante la inicialización.
            // Sin esto, una excepción cualquiera en una continuación de
            // BeginInvoke (callback del pipe llegando, snapshot inválido, NRE
            // en el header durante una desconexión, etc.) termina el proceso
            // silenciosamente y el Visor "desaparece" de la bandeja.
            //
            // Filosofía: el Visor está diseñado para sobrevivir a fallos
            // transientes (servicio detenido, pipe roto, snapshot incompleto).
            // Tragamos la excepción, la registramos al EventLog de Windows
            // para diagnóstico y dejamos al Visor seguir corriendo.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                try { EventoLog.Error("Visor (UI ThreadException): " + e.Exception); } catch { }
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try { EventoLog.Error("Visor (UnhandledException): " + e.ExceptionObject); } catch { }
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try { EventoLog.Error("Visor (UnobservedTaskException): " + e.Exception); } catch { }
                // Marcamos como observada para que el finalizer NO termine el proceso
                // (comportamiento por default en .NET Framework 4 antes de 4.5 — aún
                // así lo marcamos por defensa en profundidad).
                try { e.SetObserved(); } catch { }
            };

            using (var mutex = new Mutex(initiallyOwned: false, name: NombreMutex))
            {
                bool tomado = false;
                try
                {
                    try
                    {
                        tomado = mutex.WaitOne(0, exitContext: false);
                    }
                    catch (AbandonedMutexException)
                    {
                        // La instancia anterior terminó sin liberar el mutex.
                        // Lo heredamos limpiamente y continuamos.
                        tomado = true;
                    }

                    if (!tomado)
                    {
                        // Ya hay un visor corriendo en esta sesión. Salimos sin
                        // ruido — no abrimos ventanas, no mostramos mensajes.
                        return;
                    }

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    // Auto-registro idempotente en HKCU\...\Run con --tray.
                    AutoArranque.AsegurarRegistroEnHKCU();

                    // Precarga del tema (PORTAL_CONFIG): colores, nombre, logo.
                    // Falla silenciosamente si no hay HKLM / portal — el Visor
                    // queda con paleta default (blue-600 / slate-900).
                    PrecargarTema();

                    var arrancaEnTray = ArgPresente(argv, ArgTray);
                    Application.Run(new FormVisor(arrancaEnTray));
                }
                finally
                {
                    if (tomado)
                    {
                        try { mutex.ReleaseMutex(); } catch { }
                    }
                }
            }
        }

        private static bool ArgPresente(string[] argv, string arg)
        {
            if (argv == null) return false;
            foreach (var a in argv)
                if (string.Equals(a, arg, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>
        /// Lee URL/API key del registro y, si ambos están presentes, intenta
        /// cargar el tema con timeout 3s. Cualquier error se silencia — los
        /// defaults son suficientes para usar la app.
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
                Tema.CargarConTimeoutAsync(api, TimeSpan.FromSeconds(3))
                    .GetAwaiter().GetResult();
            }
            catch
            {
                // No-op: Tema queda con defaults + ModoOffline = true.
            }
        }
    }
}
