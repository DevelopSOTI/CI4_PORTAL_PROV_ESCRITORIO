using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace PortalProveedoresConfigurador.Configuracion
{
    /// <summary>
    /// Helper de elevación de permisos (UAC).
    ///
    /// Patrón "self-relaunch elevado por tarea": el Configurador abre SIN
    /// pedir UAC. Cuando un botón requiere privilegios (escribir HKLM,
    /// instalar/iniciar/detener servicio Windows, etc.), la UI llama a
    /// <see cref="EjecutarTareaElevada(string, string[])"/>, que arranca una
    /// segunda instancia de este mismo EXE con <c>--task=&lt;nombre&gt;</c> y
    /// verbo <c>runas</c> — eso es lo que dispara el prompt de UAC. La
    /// instancia elevada hace UNA cosa, devuelve un ExitCode y termina; la
    /// instancia normal sigue corriendo con su UI intacta.
    ///
    /// Beneficios:
    ///   - El usuario solo ve UAC cuando realmente lo necesita (no al abrir).
    ///   - Cada acción elevada es atómica y aislada del resto de la app.
    ///   - Funciona con el modelo de seguridad estándar de Windows; nada raro.
    /// </summary>
    public static class UAC
    {
        /// <summary>
        /// True si el proceso actual ya está corriendo con privilegios de admin.
        /// Útil para evitar relanzarse cuando ya estás elevado (p.ej. el usuario
        /// abrió el Configurador "Como administrador" desde el menú contextual).
        /// </summary>
        public static bool EsAdministrador()
        {
            using (var id = WindowsIdentity.GetCurrent())
                return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Relanza este mismo EXE con argumentos extra solicitando UAC. Espera
        /// a que termine y devuelve el ExitCode. Convenciones:
        ///   <list type="bullet">
        ///     <item>0  → la tarea elevada terminó OK.</item>
        ///     <item>1  → la tarea elevada terminó con error (lanzó excepción).</item>
        ///     <item>2  → tarea desconocida (mismatch entre llamador y dispatcher).</item>
        ///     <item>-1 → el usuario canceló el prompt de UAC, o no se pudo
        ///                arrancar el proceso.</item>
        ///   </list>
        ///
        /// Nota: con Verb=runas es OBLIGATORIO UseShellExecute=true, lo que
        /// implica que NO podemos capturar stdout/stderr. Para devolver
        /// resultados ricos hay que usar archivos temporales o el ExitCode.
        /// </summary>
        public static int EjecutarTareaElevada(string tarea, params string[] argsExtra)
        {
            if (string.IsNullOrWhiteSpace(tarea))
                throw new ArgumentException("tarea es obligatoria.", "tarea");

            var argumentos = "--task=" + tarea;
            if (argsExtra != null && argsExtra.Length > 0)
                argumentos += " " + string.Join(" ", argsExtra);

            var psi = new ProcessStartInfo
            {
                FileName        = Application.ExecutablePath,
                Arguments       = argumentos,
                UseShellExecute = true,    // requerido para Verb=runas
                Verb            = "runas", // dispara el prompt UAC
                CreateNoWindow  = false,
            };

            try
            {
                using (var p = Process.Start(psi))
                {
                    if (p == null) return -1;
                    p.WaitForExit();
                    return p.ExitCode;
                }
            }
            catch (Win32Exception)
            {
                // Usuario canceló UAC (1223) o Windows no pudo arrancar el proceso.
                return -1;
            }
        }
    }
}
