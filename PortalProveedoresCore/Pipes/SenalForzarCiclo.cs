using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace PortalProveedoresCore.Pipes
{
    /// <summary>
    /// Evento con nombre global para pedirle al Servicio un ciclo inmediato
    /// ("Sincronizar ahora") desde cualquier proceso de la máquina.
    ///
    /// Por qué no el pipe: <see cref="ServidorPipe"/> admite UNA sola conexión
    /// (<c>maxNumberOfServerInstances: 1</c>) y el Visor, que vive en la
    /// bandeja, normalmente la ocupa. El evento es independiente del pipe: no
    /// compite por el slot y es fire-and-forget.
    ///
    /// El Servicio lo CREA (con ACL) y lo escucha; los clientes solo lo ABREN y
    /// hacen <c>Set()</c>. AutoReset: cada <c>Set()</c> es un pulso.
    /// </summary>
    public static class SenalForzarCiclo
    {
        /// <summary>
        /// "Global\" para que lo vean las sesiones de usuario aunque el
        /// Servicio corra en la sesión 0 (LocalSystem).
        /// </summary>
        public const string Nombre = @"Global\PortalProveedoresService_ForzarCiclo";

        /// <summary>
        /// Lado Servicio: crea el evento con ACL que deja al usuario interactivo
        /// y a los administradores señalarlo (sin ACL, un evento creado por
        /// LocalSystem no lo puede abrir un usuario normal). Mismo criterio de
        /// identidades que el ACL del pipe.
        /// </summary>
        public static EventWaitHandle Crear()
        {
            var seguridad = new EventWaitHandleSecurity();

            seguridad.AddAccessRule(new EventWaitHandleAccessRule(
                new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                EventWaitHandleRights.FullControl,
                AccessControlType.Allow));

            seguridad.AddAccessRule(new EventWaitHandleAccessRule(
                new SecurityIdentifier(WellKnownSidType.InteractiveSid, null),
                EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize,
                AccessControlType.Allow));

            seguridad.AddAccessRule(new EventWaitHandleAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize,
                AccessControlType.Allow));

            bool creado;
            return new EventWaitHandle(false, EventResetMode.AutoReset, Nombre, out creado, seguridad);
        }

        /// <summary>
        /// Lado cliente (Configurador): señala al Servicio. Devuelve false si
        /// el evento no existe, es decir, el Servicio no está en ejecución.
        /// No lanza por eso; sí propaga <see cref="UnauthorizedAccessException"/>
        /// si el ACL no permite al usuario actual.
        /// </summary>
        public static bool Solicitar()
        {
            EventWaitHandle evento;
            if (!EventWaitHandle.TryOpenExisting(Nombre, EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize, out evento))
                return false;

            using (evento)
            {
                evento.Set();
            }
            return true;
        }
    }
}
