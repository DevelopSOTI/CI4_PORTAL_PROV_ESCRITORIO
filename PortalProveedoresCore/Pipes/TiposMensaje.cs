namespace PortalProveedoresCore.Pipes
{
    /// <summary>
    /// Discriminador de los mensajes que viajan por el pipe.
    ///
    /// Convención:
    ///   - <c>"evt:..."</c> → mensaje enviado del Servicio hacia el Visor.
    ///   - <c>"cmd:..."</c> → mensaje enviado del Visor hacia el Servicio.
    ///
    /// Es campo de cada mensaje (ver <see cref="MensajeBase.tipo"/>), y el
    /// <see cref="SerializadorMensajes"/> lo usa para resolver la clase CLR
    /// correcta al deserializar.
    /// </summary>
    public static class TiposMensaje
    {
        // ===== Eventos (Servicio → Visor) ====================================

        /// <summary>El servicio acaba de arrancar. Se manda una sola vez por proceso.</summary>
        public const string EVT_SERVICIO_INICIADO   = "evt:servicio_iniciado";

        /// <summary>El servicio está a punto de terminar (OnStop o Ctrl+C).</summary>
        public const string EVT_SERVICIO_DETENIENDO = "evt:servicio_deteniendo";

        /// <summary>Foto del estado del servicio. Se manda apenas conecta un visor.</summary>
        public const string EVT_SNAPSHOT            = "evt:snapshot";

        /// <summary>Arrancó un ciclo de sincronización.</summary>
        public const string EVT_CICLO_INICIADO      = "evt:ciclo_iniciado";

        /// <summary>Terminó un ciclo, con éxito o falla.</summary>
        public const string EVT_CICLO_TERMINADO     = "evt:ciclo_terminado";

        /// <summary>Arrancó un paso dentro del ciclo (Empresas, Proveedores, etc.).</summary>
        public const string EVT_PASO_INICIADO       = "evt:paso_iniciado";

        /// <summary>Terminó un paso, con conteo de items y resumen.</summary>
        public const string EVT_PASO_TERMINADO      = "evt:paso_terminado";

        /// <summary>Progreso intermedio de un paso (ej. "8 de 47 empresas").</summary>
        public const string EVT_PROGRESO            = "evt:progreso";

        /// <summary>Log genérico (info/warning/error/success) — para todo lo que no encaja en los demás.</summary>
        public const string EVT_LOG                 = "evt:log";

        // ===== Comandos (Visor → Servicio) ===================================

        /// <summary>Heartbeat. El servicio responde con un EVT_LOG nivel info.</summary>
        public const string CMD_PING                = "cmd:ping";

        /// <summary>Pide al servicio que arranque un ciclo ahora, sin esperar al timer.</summary>
        public const string CMD_FORZAR_CICLO        = "cmd:forzar_ciclo";

        /// <summary>Pone al servicio en pausa: no dispara ciclos nuevos hasta CMD_REANUDAR.</summary>
        public const string CMD_PAUSAR              = "cmd:pausar";

        /// <summary>Sale de pausa y agenda el siguiente ciclo según el timer normal.</summary>
        public const string CMD_REANUDAR            = "cmd:reanudar";

        /// <summary>Pide al servicio que (re)envíe el EVT_SNAPSHOT. Útil tras reconectar.</summary>
        public const string CMD_SOLICITAR_SNAPSHOT  = "cmd:solicitar_snapshot";
    }
}
