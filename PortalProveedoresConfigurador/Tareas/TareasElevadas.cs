using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;

namespace PortalProveedoresConfigurador.Tareas
{
    /// <summary>
    /// Implementación de las acciones que el dispatcher de Program.cs ejecuta
    /// cuando el Configurador se relanza a sí mismo con --task=&lt;nombre&gt; y
    /// verbo "runas". Cada método aquí asume que ya corre con privilegios de
    /// administrador (UAC ya aceptado por el usuario).
    ///
    /// Convención de retorno:
    ///   0 → éxito
    ///   1 → error (excepción capturada al margen, ver Program.cs)
    ///   2 → tarea desconocida (ver Program.cs)
    ///
    /// Convención de args: pares <c>--clave=valor</c> en cualquier orden,
    /// estilo POSIX largo. Parseo por <see cref="LeerArg(string[], string)"/>.
    /// </summary>
    internal static class TareasElevadas
    {
        private const string RutaRegistro = @"SOFTWARE\SOTI\Service Portal";

        /// <summary>
        /// Tarea "guardar-otros-hklm" — escribe MODE_TIMER y ENVIAR_CORREO_COMPRAS
        /// en HKLM. Se invoca desde la pestaña "Otros parámetros" del FormPrincipal.
        ///
        /// Args esperados:
        ///   --mode-timer=&lt;segundos&gt;
        ///   --enviar-correo-compras=True|False
        /// </summary>
        public static int GuardarOtrosHKLM(string[] argv)
        {
            var modeTimer    = LeerArg(argv, "--mode-timer");
            var enviarCorreo = LeerArg(argv, "--enviar-correo-compras");

            if (string.IsNullOrWhiteSpace(modeTimer) || string.IsNullOrWhiteSpace(enviarCorreo))
            {
                MessageBox.Show(
                    "Argumentos faltantes para guardar-otros-hklm.\n\n" +
                    "Esperado: --mode-timer=<seg> --enviar-correo-compras=True|False",
                    "Configurador (elevado)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return 1;
            }

            // Asegura la subclave HKLM existe (CrearLlaveRegistro es no-op si ya
            // está creada) y luego escribe los dos valores. Si la subclave no
            // existiera y no tuviéramos admin, EscribirRegistros falla; aquí
            // estamos elevados así que debería funcionar.
            var reg = new RegistrosWindows();
            string msg;
            reg.CrearLlaveRegistro(RutaRegistro, out msg);

            string m1, m2;
            var ok1 = reg.EscribirRegistros(RutaRegistro, "MODE_TIMER",            modeTimer,    out m1);
            var ok2 = reg.EscribirRegistros(RutaRegistro, "ENVIAR_CORREO_COMPRAS", enviarCorreo, out m2);

            if (!ok1 || !ok2)
            {
                MessageBox.Show(
                    "No se pudieron escribir todos los registros:\n\n" +
                    (ok1 ? "" : "- MODE_TIMER: " + m1 + "\n") +
                    (ok2 ? "" : "- ENVIAR_CORREO_COMPRAS: " + m2),
                    "Configurador (elevado)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Tarea "guardar-microsip" — escribe MICRO_SERV, MICRO_ROOT, MICRO_USER,
        /// MICRO_PASS en HKLM. Invocada desde la sección Microsip del FormPrincipal.
        ///
        /// Args esperados:
        ///   --micro-srv=&lt;servidor&gt;
        ///   --micro-root=&lt;ruta&gt;
        ///   --micro-user=&lt;usuario&gt;
        ///   --micro-pass=&lt;password&gt;
        /// </summary>
        public static int GuardarMicrosip(string[] argv)
        {
            var srv  = LeerArg(argv, "--micro-srv");
            var root = LeerArg(argv, "--micro-root");
            var user = LeerArg(argv, "--micro-user");
            var pass = LeerArg(argv, "--micro-pass");

            // pass puede legítimamente venir vacío en algunas instalaciones,
            // pero el resto son obligatorios para poder conectar.
            if (string.IsNullOrWhiteSpace(srv) || string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(user))
            {
                MessageBox.Show(
                    "Argumentos faltantes para guardar-microsip.",
                    "Configurador (elevado)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return 1;
            }

            var reg = new RegistrosWindows();
            string msg;
            reg.CrearLlaveRegistro(RutaRegistro, out msg);

            string m1, m2, m3, m4;
            var ok =
                reg.EscribirRegistros(RutaRegistro, "MICRO_SERV", srv,  out m1) &
                reg.EscribirRegistros(RutaRegistro, "MICRO_ROOT", root, out m2) &
                reg.EscribirRegistros(RutaRegistro, "MICRO_USER", user, out m3) &
                reg.EscribirRegistros(RutaRegistro, "MICRO_PASS", pass ?? "", out m4);

            if (!ok)
            {
                MessageBox.Show(
                    "No se pudieron escribir todos los registros:\n\n" +
                    string.Join("\n", new[] { m1, m2, m3, m4 }),
                    "Configurador (elevado)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Tarea "guardar-portal" — escribe PORTAL_BASE_URL y PORTAL_API_KEY en
        /// HKLM. Invocada desde la sección Portal Web del FormPrincipal.
        ///
        /// Args esperados:
        ///   --portal-url=&lt;url&gt;
        ///   --portal-api-key=&lt;key&gt;
        /// </summary>
        public static int GuardarPortal(string[] argv)
        {
            var url = LeerArg(argv, "--portal-url");
            var key = LeerArg(argv, "--portal-api-key");

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show(
                    "Argumentos faltantes para guardar-portal.",
                    "Configurador (elevado)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return 1;
            }

            var reg = new RegistrosWindows();
            string msg;
            reg.CrearLlaveRegistro(RutaRegistro, out msg);

            string m1, m2;
            var ok =
                reg.EscribirRegistros(RutaRegistro, "PORTAL_BASE_URL", url, out m1) &
                reg.EscribirRegistros(RutaRegistro, "PORTAL_API_KEY",  key, out m2);

            if (!ok)
            {
                MessageBox.Show(
                    "No se pudieron escribir los registros del portal:\n\n" +
                    string.Join("\n", new[] { m1, m2 }),
                    "Configurador (elevado)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return 1;
            }

            return 0;
        }

        // ====================================================================
        // SERVICIO WINDOWS
        // ====================================================================

        /// <summary>
        /// Tarea "guardar-servicio" — escribe SERVICE_NAME y RUTA_ARCHIVOS en HKLM.
        /// </summary>
        public static int GuardarServicio(string[] argv)
        {
            var nombre = LeerArg(argv, "--service-name");
            var ruta   = LeerArg(argv, "--ruta-archivos");

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarError("Falta --service-name.");
                return 1;
            }

            var reg = new RegistrosWindows();
            string msg;
            reg.CrearLlaveRegistro(RutaRegistro, out msg);

            string m1, m2;
            var ok =
                reg.EscribirRegistros(RutaRegistro, "SERVICE_NAME",   nombre,    out m1) &
                reg.EscribirRegistros(RutaRegistro, "RUTA_ARCHIVOS",  ruta ?? "", out m2);

            if (!ok)
            {
                MostrarError("No se pudieron escribir los registros:\n\n" +
                             string.Join("\n", new[] { m1, m2 }));
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Tarea "instalar-servicio" — registra el servicio Windows usando
        /// <c>sc.exe create</c>. La ruta del EXE del servicio se busca primero
        /// junto al Configurador (instalación normal), y como fallback en el
        /// proyecto hermano (escenario de desarrollo con F5).
        /// </summary>
        public static int InstalarServicio(string[] argv)
        {
            var nombre = LeerArg(argv, "--service-name");
            if (string.IsNullOrWhiteSpace(nombre)) { MostrarError("Falta --service-name."); return 1; }

            var exe = LocalizarServicioExe();
            if (exe == null)
            {
                MostrarError(
                    "No se encontró 'PortalProveedoresService.exe'.\n\n" +
                    "Busqué junto al Configurador y en la carpeta hermana del servicio. " +
                    "Copia el EXE del servicio al mismo directorio que el Configurador.");
                return 1;
            }

            // sc create requiere el formato 'binPath= "..."' con un espacio
            // después del '='. Comillas dobles obligatorias si el path tiene
            // espacios. El DisplayName sí puede llevar espacios.
            var binPath = "\"" + exe + "\"";
            var args = "create " + nombre +
                       " binPath= " + binPath +
                       " start= auto" +
                       " DisplayName= \"" + nombre + " (Portal Proveedores)\"";

            return EjecutarSc(args, "Instalar servicio");
        }

        /// <summary>Tarea "desinstalar-servicio" — detiene si está activo y luego sc delete.</summary>
        public static int DesinstalarServicio(string[] argv)
        {
            var nombre = LeerArg(argv, "--service-name");
            if (string.IsNullOrWhiteSpace(nombre)) { MostrarError("Falta --service-name."); return 1; }

            // Si está corriendo, intentamos detenerlo primero — sc delete solo
            // marca para borrado si está corriendo, y queda fantasma hasta el
            // próximo reinicio. Mejor detenerlo manualmente.
            try
            {
                using (var svc = new ServiceController(nombre))
                {
                    if (svc.Status == ServiceControllerStatus.Running ||
                        svc.Status == ServiceControllerStatus.StartPending)
                    {
                        svc.Stop();
                        svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }
            }
            catch { /* si no existía, sc delete devolverá error y se reporta */ }

            return EjecutarSc("delete " + nombre, "Desinstalar servicio");
        }

        public static int IniciarServicio(string[] argv)
        {
            var nombre = LeerArg(argv, "--service-name");
            if (string.IsNullOrWhiteSpace(nombre)) { MostrarError("Falta --service-name."); return 1; }

            try
            {
                using (var svc = new ServiceController(nombre))
                {
                    if (svc.Status != ServiceControllerStatus.Stopped)
                        return 0; // ya está corriendo o iniciando — éxito tácito
                    svc.Start();
                    svc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo iniciar el servicio:\n\n" + ex.Message);
                return 1;
            }
        }

        public static int DetenerServicio(string[] argv)
        {
            var nombre = LeerArg(argv, "--service-name");
            if (string.IsNullOrWhiteSpace(nombre)) { MostrarError("Falta --service-name."); return 1; }

            try
            {
                using (var svc = new ServiceController(nombre))
                {
                    if (svc.Status == ServiceControllerStatus.Stopped)
                        return 0;
                    svc.Stop();
                    svc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo detener el servicio:\n\n" + ex.Message);
                return 1;
            }
        }

        // ====================================================================
        // Helpers internos
        // ====================================================================

        /// <summary>
        /// Ejecuta <c>sc.exe</c> con los argumentos dados, captura stdout/stderr
        /// y muestra el resultado si fue error. Códigos de retorno != 0 son
        /// errores reales (sc.exe usa 0 = éxito).
        /// </summary>
        private static int EjecutarSc(string args, string operacion)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName               = "sc.exe",
                    Arguments              = args,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                };
                using (var p = Process.Start(psi))
                {
                    var stdout = p.StandardOutput.ReadToEnd();
                    var stderr = p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    if (p.ExitCode != 0)
                    {
                        MostrarError(
                            operacion + " falló (sc.exe exit " + p.ExitCode + "):\n\n" +
                            (string.IsNullOrWhiteSpace(stderr) ? stdout : stderr));
                        return 1;
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo invocar sc.exe:\n\n" + ex.Message);
                return 1;
            }
        }

        /// <summary>
        /// Devuelve la ruta al EXE del servicio Windows. Busca primero junto
        /// al Configurador, luego en el directorio hermano (escenario dev F5).
        /// </summary>
        private static string LocalizarServicioExe()
        {
            var dirConfig = Path.GetDirectoryName(Application.ExecutablePath);
            const string nombreExe = "PortalProveedoresService.exe";

            // 1) Misma carpeta — instalación de producción.
            var p1 = Path.Combine(dirConfig, nombreExe);
            if (File.Exists(p1)) return p1;

            // 2) Carpeta hermana del proyecto Service — desarrollo desde VS.
            //    Configurador\bin\Debug → ..\..\..\PortalProveedoresService\bin\<Config>
            try
            {
                var subir3 = Path.GetFullPath(Path.Combine(dirConfig, "..", "..", ".."));
                foreach (var conf in new[] { "Debug", "Release" })
                {
                    var p2 = Path.Combine(subir3, "PortalProveedoresService", "bin", conf, nombreExe);
                    if (File.Exists(p2)) return p2;
                }
            }
            catch { }

            return null;
        }

        private static void MostrarError(string mensaje)
        {
            MessageBox.Show(
                mensaje,
                "Configurador (elevado)",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        /// <summary>Lee el valor de un argumento estilo <c>--clave=valor</c>.</summary>
        private static string LeerArg(string[] argv, string nombre)
        {
            if (argv == null) return null;
            var prefijo = nombre + "=";
            foreach (var a in argv)
                if (a != null && a.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                    return a.Substring(prefijo.Length);
            return null;
        }
    }
}
