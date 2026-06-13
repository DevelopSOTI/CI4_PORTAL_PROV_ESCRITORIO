namespace PortalProveedoresCore.Pipes
{
    /// <summary>
    /// Constantes del canal Named Pipe entre el Servicio Windows y el Visor.
    /// Comparte literales que deben coincidir 1:1 entre cliente y servidor para
    /// que la conexión funcione.
    /// </summary>
    public static class ConstantesPipe
    {
        /// <summary>
        /// Nombre del pipe (sin el prefijo "\\.\pipe\"). Sirve tanto para
        /// <c>NamedPipeServerStream</c> como <c>NamedPipeClientStream</c>.
        /// </summary>
        public const string NombrePipe = "PortalProveedoresService";

        /// <summary>
        /// Versión del protocolo de mensajes. Si el servicio escribe versión
        /// mayor que la que entiende el visor, el visor puede mostrar un
        /// aviso "actualízate". Se incrementa cuando cambia la forma de un
        /// mensaje o se quita un tipo.
        /// </summary>
        public const int VersionProtocolo = 1;
    }

    /// <summary>
    /// Niveles de severidad para <see cref="Eventos.EventoLog"/>. Coinciden
    /// con la paleta de colores del Visor (info → azul, warning → ámbar,
    /// error → rojo, success → verde).
    /// </summary>
    public static class NivelLog
    {
        public const string Info    = "info";
        public const string Warning = "warning";
        public const string Error   = "error";
        public const string Success = "success";
    }

    /// <summary>
    /// Identificadores estables de cada paso del ciclo de sincronización.
    /// El Visor los usa para agrupar logs y mostrar progreso por paso.
    /// El orden aquí refleja el orden de ejecución que dicta el doc maestro.
    /// </summary>
    public static class PasoSincronizacion
    {
        public const string Empresas          = "empresas";
        public const string Almacenes         = "almacenes";
        public const string Monedas           = "monedas";
        public const string Proveedores       = "proveedores";
        public const string CamposLibres      = "campos_libres";
        public const string Recepciones       = "recepciones";
        public const string Notas             = "notas";
        public const string Creditos          = "creditos";
        public const string Facturas          = "facturas";
        public const string ComplementosPago  = "complementos_pago";
        public const string Sellado           = "sellado";
    }

    /// <summary>
    /// Estados que reporta el servicio en <see cref="Eventos.EventoSnapshot"/>
    /// para que el Visor sepa qué pintar al conectarse.
    /// </summary>
    public static class EstadoServicio
    {
        public const string Iniciando       = "iniciando";
        public const string Ocioso          = "ocioso";          // esperando siguiente ciclo
        public const string EjecutandoCiclo = "ejecutando_ciclo";
        public const string Pausado         = "pausado";         // ciclos no se disparan
        public const string Deteniendo      = "deteniendo";
    }
}
