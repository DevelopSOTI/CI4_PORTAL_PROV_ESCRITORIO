namespace PortalProveedoresCore.Pipes
{
    // Mensajes que el Visor envía hacia el Servicio. El servicio los procesa
    // de inmediato. Si la acción provoca eventos secundarios (ej. el comando
    // FORZAR_CICLO arranca un ciclo), esos eventos viajan de vuelta por el
    // canal normal de eventos — no hay un "response" del comando como tal.

    /// <summary>
    /// Heartbeat. El servicio responde con un EVT_LOG nivel "info" diciendo
    /// "pong" — útil para que el Visor sepa que el pipe está vivo.
    /// </summary>
    public sealed class ComandoPing : MensajeBase
    {
        public ComandoPing() { tipo = TiposMensaje.CMD_PING; }
    }

    /// <summary>
    /// Pide al servicio que arranque un ciclo ya, sin esperar al MODE_TIMER.
    /// Si ya hay un ciclo corriendo, el servicio ignora el comando y emite
    /// un EVT_LOG nivel "warning" explicando.
    /// </summary>
    public sealed class ComandoForzarCiclo : MensajeBase
    {
        public ComandoForzarCiclo() { tipo = TiposMensaje.CMD_FORZAR_CICLO; }
    }

    /// <summary>
    /// Pausa el servicio: el ciclo en curso (si hay) termina, pero no se
    /// dispara el siguiente. Persiste hasta CMD_REANUDAR o reinicio del
    /// proceso.
    /// </summary>
    public sealed class ComandoPausar : MensajeBase
    {
        public ComandoPausar() { tipo = TiposMensaje.CMD_PAUSAR; }
    }

    /// <summary>Sale de pausa y agenda el siguiente ciclo con el timer normal.</summary>
    public sealed class ComandoReanudar : MensajeBase
    {
        public ComandoReanudar() { tipo = TiposMensaje.CMD_REANUDAR; }
    }

    /// <summary>
    /// Pide al servicio que reenvíe un EVT_SNAPSHOT. Útil cuando el Visor
    /// se reconecta tras un drop o quiere refrescar la cabecera sin esperar
    /// al siguiente ciclo.
    /// </summary>
    public sealed class ComandoSolicitarSnapshot : MensajeBase
    {
        public ComandoSolicitarSnapshot() { tipo = TiposMensaje.CMD_SOLICITAR_SNAPSHOT; }
    }
}
