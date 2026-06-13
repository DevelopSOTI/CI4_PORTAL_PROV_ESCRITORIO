using System;
using System.Diagnostics;
using System.IO;

namespace PortalProveedoresCore.Logging
{
    /// <summary>
    /// Logging centralizado. Escribe al EventLog de Windows (categoría
    /// Application, source PortalProveedoresService) y, en paralelo, persiste a
    /// disco SOLO los warnings y errores en una estructura de carpetas fechada
    /// {baseDir}\yyyy\MM\dd\eventos.log para depuración rápida sin abrir el
    /// Visor de Eventos. Los Information NO se persisten a disco (siguen yendo
    /// al EventLog de Windows y al Visor en vivo) para no llenar el servidor.
    ///
    /// El registro de Source en EventLog requiere admin, así que si no existe
    /// y no se puede crear, se cae elegantemente al archivo solamente — la app
    /// nunca se cuelga por no poder loguear.
    /// </summary>
    public static class EventoLog
    {
        private const string Source = "PortalProveedoresService";
        private const string LogName = "Application";

        private static readonly object _candado = new object();

        public static void Info   (string msg) { Escribir(msg, EventLogEntryType.Information); }
        public static void Warning(string msg) { Escribir(msg, EventLogEntryType.Warning); }
        public static void Error  (string msg) { Escribir(msg, EventLogEntryType.Error); }

        /// <summary>
        /// Hook opcional para que el Servicio (y solo él) reciba cada log y lo
        /// reenvíe al pipe del Visor. La firma es <c>(nivel, mensaje)</c> con
        /// nivel ∈ {info, warning, error}. Lo deja como event para que múltiples
        /// suscriptores convivan sin pisarse y para que asignar a null lo
        /// desactive limpio.
        ///
        /// Quién lo conecta: Service1.OnStart engancha a CanalEventos.Publicar.
        /// El Configurador y los unit tests no lo tocan — se queda en null y
        /// el comportamiento es idéntico al original.
        /// </summary>
        public static event Action<string, string> Publicador;

        private static void Escribir(string msg, EventLogEntryType tipo)
        {
            EscribirEventLog(msg, tipo);
            EscribirArchivo(msg, tipo);

            // Snapshot del delegate para que un unsubscribe en otro thread no
            // truene esta llamada con NullReferenceException.
            var pub = Publicador;
            if (pub != null)
            {
                try { pub(NivelDeTipo(tipo), msg); }
                catch { /* el logging nunca debe tirar el servicio */ }
            }
        }

        private static string NivelDeTipo(EventLogEntryType tipo)
        {
            switch (tipo)
            {
                case EventLogEntryType.Information: return "info";
                case EventLogEntryType.Warning:     return "warning";
                case EventLogEntryType.Error:       return "error";
                default:                             return "info";
            }
        }

        private static void EscribirEventLog(string msg, EventLogEntryType tipo)
        {
            try
            {
                if (!EventLog.SourceExists(Source))
                    EventLog.CreateEventSource(Source, LogName);
                EventLog.WriteEntry(Source, msg, tipo);
            }
            catch
            {
                // No-op: si no podemos escribir al EventLog (sin admin para
                // crear el Source la primera vez), seguimos con el archivo.
            }
        }

        private static void EscribirArchivo(string msg, EventLogEntryType tipo)
        {
            // SOLO persistimos warnings y errores a disco. Los Information se
            // omiten aquí (llenaban el servidor de basura); siguen yendo al
            // EventLog de Windows y al Visor en vivo.
            if (tipo != EventLogEntryType.Warning && tipo != EventLogEntryType.Error)
                return;

            try
            {
                // Carpeta fechada NUMÉRICA: {baseDir}\yyyy\MM\dd\eventos.log
                // (en producción baseDir resuelve a C:\SOTI\PORTAL_PROVEEDORES).
                var now   = DateTime.Now;
                var year  = now.ToString("yyyy");
                var month = now.ToString("MM");
                var day   = now.ToString("dd");

                var carpeta = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, year, month, day);

                // Crea toda la cadena de subdirectorios si no existe.
                Directory.CreateDirectory(carpeta);

                var archivo = Path.Combine(carpeta, "eventos.log");
                var linea   = now.ToString("HH:mm:ss") + " [" + tipo + "] " + msg + Environment.NewLine;

                lock (_candado)
                {
                    File.AppendAllText(archivo, linea);
                }
            }
            catch
            {
                // No-op: si ni siquiera podemos escribir a disco, no hay donde
                // reportarlo y el servicio no debe morir por logging.
            }
        }
    }
}
