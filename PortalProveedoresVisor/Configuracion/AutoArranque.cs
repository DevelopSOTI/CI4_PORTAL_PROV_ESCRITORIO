using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PortalProveedoresVisor.Configuracion
{
    /// <summary>
    /// Maneja la entrada del Visor en
    /// <c>HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run</c>
    /// para que se autoarranque en cada inicio de sesión del usuario, oculto
    /// en la bandeja (argumento <c>--tray</c>).
    ///
    /// Por qué HKCU y no HKLM: HKCU es per-user (no requiere admin), cada
    /// usuario que inicie sesión en la máquina tendrá su propio Visor. HKLM
    /// requeriría UAC al primer arranque y aplicaría a todos los usuarios,
    /// lo cual no es lo deseado para una app de monitoreo personal.
    ///
    /// Idempotente: <see cref="AsegurarRegistroEnHKCU"/> revisa si el valor ya
    /// está correcto y solo escribe si difiere — no genera tráfico de registro
    /// innecesario en cada arranque.
    /// </summary>
    internal static class AutoArranque
    {
        private const string RutaRun = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string Clave   = "PortalProveedoresVisor";

        /// <summary>
        /// Garantiza que la entrada apunta al EXE actual con <c>--tray</c>.
        /// Si ya está correcta, no hace nada. Falla silenciosamente si no
        /// se puede escribir (políticas de grupo, etc.) — el Visor sigue
        /// corriendo, solo no se autoarranca en el próximo login.
        /// </summary>
        public static void AsegurarRegistroEnHKCU()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RutaRun, writable: true))
                {
                    if (key == null) return;

                    var deseado = "\"" + Application.ExecutablePath + "\" --tray";
                    var actual  = key.GetValue(Clave) as string;

                    if (!string.Equals(actual, deseado, StringComparison.Ordinal))
                        key.SetValue(Clave, deseado, RegistryValueKind.String);
                }
            }
            catch
            {
                // Sin permisos o política de grupo bloquea el Run. No es fatal.
            }
        }

        /// <summary>True si la clave existe (sin importar su valor).</summary>
        public static bool EstaRegistrado()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RutaRun, writable: false))
                {
                    return key != null && key.GetValue(Clave) != null;
                }
            }
            catch { return false; }
        }

        /// <summary>Quita la clave del auto-arranque. Para opción "no iniciar al iniciar sesión".</summary>
        public static void Desregistrar()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RutaRun, writable: true))
                {
                    if (key != null && key.GetValue(Clave) != null)
                        key.DeleteValue(Clave);
                }
            }
            catch { }
        }
    }
}
