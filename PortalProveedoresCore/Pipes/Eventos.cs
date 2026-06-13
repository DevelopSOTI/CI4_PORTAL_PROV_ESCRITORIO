using System;

namespace PortalProveedoresCore.Pipes
{
    // Mensajes que viajan del Servicio Windows hacia el Visor (push, no poll).
    // El servicio los pone en su Channel<MensajeBase> interno; el ServidorPipe
    // los serializa y los escribe al stream del pipe línea por línea (NDJSON).
    //
    // Convención: todos los campos en snake_case (mismo estilo que el JSON
    // del resto del proyecto). Constructores asignan tipo y arrastran
    // base.ts/base.version desde MensajeBase.

    // ====================================================================
    // Lifecycle del servicio
    // ====================================================================

    /// <summary>El servicio acaba de arrancar. Se envía exactamente una vez por proceso.</summary>
    public sealed class EventoServicioIniciado : MensajeBase
    {
        public EventoServicioIniciado() { tipo = TiposMensaje.EVT_SERVICIO_INICIADO; }

        public string nombre_servicio { get; set; }   // nombre Windows del servicio
        public string version_servicio { get; set; }  // versión del EXE
        public int    pid              { get; set; }  // PID del proceso del servicio
    }

    /// <summary>El servicio está a punto de detenerse (OnStop del SCM o cierre interactivo).</summary>
    public sealed class EventoServicioDeteniendo : MensajeBase
    {
        public EventoServicioDeteniendo() { tipo = TiposMensaje.EVT_SERVICIO_DETENIENDO; }

        public string razon { get; set; }   // libre: "OnStop del SCM", "Ctrl+C", etc.
    }

    /// <summary>
    /// "Foto" del estado del servicio. El servicio lo manda cuando un Visor
    /// se conecta (o cuando el Visor lo pide con CMD_SOLICITAR_SNAPSHOT) para
    /// que el Visor pueda pintar la cabecera sin esperar al siguiente ciclo.
    /// </summary>
    public sealed class EventoSnapshot : MensajeBase
    {
        public EventoSnapshot() { tipo = TiposMensaje.EVT_SNAPSHOT; }

        public string  estado                    { get; set; }   // ver EstadoServicio
        public int?    ciclo_actual_id           { get; set; }   // null si no está en ciclo
        public string  paso_actual               { get; set; }   // null si no está en paso
        public DateTime? inicio_ciclo_actual     { get; set; }
        public DateTime? ultimo_ciclo_terminado  { get; set; }
        public bool?   ultimo_ciclo_ok           { get; set; }
        public int     timer_segundos            { get; set; }   // MODE_TIMER vigente

        // Progreso del ciclo en curso (null si no hay ciclo). Le permite al
        // Visor pintar la ProgressBar al reconectarse sin esperar al siguiente
        // EVT_CICLO_INICIADO.
        public int?    ciclo_pasos_total         { get; set; }
        public int?    ciclo_pasos_completados   { get; set; }
    }

    // ====================================================================
    // Ciclo de sincronización
    // ====================================================================

    /// <summary>Arrancó un ciclo. id es un contador local del servicio (1, 2, 3...).</summary>
    public sealed class EventoCicloIniciado : MensajeBase
    {
        public EventoCicloIniciado() { tipo = TiposMensaje.EVT_CICLO_INICIADO; }

        public int ciclo_id { get; set; }

        /// <summary>
        /// Cantidad total de pasos que ejecutará este ciclo. El Visor lo usa
        /// para configurar la ProgressBar como determinada (Continuous) en
        /// lugar de Marquee. Si es 0 o menor, el Visor cae a Marquee.
        /// </summary>
        public int total_pasos { get; set; }
    }

    /// <summary>Terminó un ciclo. duracion en segundos para mostrar humano.</summary>
    public sealed class EventoCicloTerminado : MensajeBase
    {
        public EventoCicloTerminado() { tipo = TiposMensaje.EVT_CICLO_TERMINADO; }

        public int    ciclo_id      { get; set; }
        public bool   ok            { get; set; }
        public double duracion_seg  { get; set; }
        public int    pasos_ok      { get; set; }
        public int    pasos_falla   { get; set; }
    }

    /// <summary>Arrancó un paso (Empresas, Proveedores, etc.). Ver PasoSincronizacion.</summary>
    public sealed class EventoPasoIniciado : MensajeBase
    {
        public EventoPasoIniciado() { tipo = TiposMensaje.EVT_PASO_INICIADO; }

        public string paso          { get; set; }   // ver PasoSincronizacion
        public int?   items_total   { get; set; }   // null si no se conoce de antemano
    }

    /// <summary>
    /// Terminó un paso con éxito o fracaso. Si ok=false, el ciclo se aborta
    /// (mismo contrato que ISincronizador.EjecutarAsync).
    /// </summary>
    public sealed class EventoPasoTerminado : MensajeBase
    {
        public EventoPasoTerminado() { tipo = TiposMensaje.EVT_PASO_TERMINADO; }

        public string paso              { get; set; }
        public bool   ok                { get; set; }
        public int    items_procesados  { get; set; }
        public string mensaje_resumen   { get; set; }   // ej. "15 empresas (14 nuevas, 1 actualizada)"
    }

    /// <summary>
    /// Progreso intermedio. Útil cuando un paso lleva un rato y queremos
    /// que el Visor muestre "8 de 47 empresas" en la barra de progreso.
    /// </summary>
    public sealed class EventoProgreso : MensajeBase
    {
        public EventoProgreso() { tipo = TiposMensaje.EVT_PROGRESO; }

        public string paso             { get; set; }
        public int    items_completados { get; set; }
        public int?   items_total      { get; set; }
        public string mensaje          { get; set; }   // libre, ej. "Sincronizando SOTI..."
    }

    // ====================================================================
    // Logs generales (catch-all)
    // ====================================================================

    /// <summary>
    /// Log que no encaja en los eventos estructurados de arriba. Para info
    /// suelta como "Leyendo registros", "Portal: conectado", warnings de DB
    /// que no pertenecen a un paso específico, etc.
    ///
    /// Nombre intencional: "EventoBitacora" en lugar de "EventoLog" para no
    /// chocar con la clase <c>PortalProveedoresCore.Logging.EventoLog</c>
    /// cuando ambos namespaces se usan en el mismo archivo (típicamente Service1).
    /// El wire format sigue siendo "evt:log".
    /// </summary>
    public sealed class EventoBitacora : MensajeBase
    {
        public EventoBitacora() { tipo = TiposMensaje.EVT_LOG; }

        public string nivel   { get; set; }   // ver NivelLog
        public string mensaje { get; set; }
        public string fuente  { get; set; }   // opcional: "EmpresasRepository", "PortalApi", etc.
    }
}
