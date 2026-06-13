using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace PortalProveedoresCore.Pipes
{
    /// <summary>
    /// Convierte mensajes del pipe a NDJSON (una línea por mensaje) y de
    /// regreso. El round-trip preserva la subclase concreta gracias al
    /// discriminador <see cref="MensajeBase.tipo"/>.
    ///
    /// Reusa <see cref="JavaScriptSerializer"/> (System.Web.Extensions) — el
    /// mismo que usa <c>PortalApi</c> — para no agregar dependencias externas.
    /// Es suficiente para POCOs planos y maneja DateTime / nullables OK.
    ///
    /// Patrón de uso:
    /// <code>
    /// // Emisor:
    /// var ev = new EventoCicloIniciado { ciclo_id = 5 };
    /// string linea = SerializadorMensajes.Serializar(ev);
    /// await writer.WriteLineAsync(linea);
    ///
    /// // Receptor:
    /// string linea = await reader.ReadLineAsync();
    /// var msg = SerializadorMensajes.Deserializar(linea);
    /// if (msg is EventoCicloIniciado ev) { ... }
    /// </code>
    ///
    /// Si el receptor recibe un <c>tipo</c> desconocido (cliente desactualizado
    /// frente a un servidor más nuevo, o ruido en el pipe), <see cref="Deserializar"/>
    /// regresa null en lugar de tronar — la línea simplemente se ignora.
    /// </summary>
    public static class SerializadorMensajes
    {
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        /// <summary>
        /// Tabla de despacho tipo→clase para resolver la deserialización.
        /// Cualquier mensaje nuevo se registra aquí — si se olvida, el receptor
        /// lo ignorará silenciosamente (regresará null) sin tronar el pipe.
        /// </summary>
        private static readonly Dictionary<string, Type> _mapeo = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            // Eventos -----------------------------------------------------
            { TiposMensaje.EVT_SERVICIO_INICIADO,   typeof(EventoServicioIniciado) },
            { TiposMensaje.EVT_SERVICIO_DETENIENDO, typeof(EventoServicioDeteniendo) },
            { TiposMensaje.EVT_SNAPSHOT,            typeof(EventoSnapshot) },
            { TiposMensaje.EVT_CICLO_INICIADO,      typeof(EventoCicloIniciado) },
            { TiposMensaje.EVT_CICLO_TERMINADO,     typeof(EventoCicloTerminado) },
            { TiposMensaje.EVT_PASO_INICIADO,       typeof(EventoPasoIniciado) },
            { TiposMensaje.EVT_PASO_TERMINADO,      typeof(EventoPasoTerminado) },
            { TiposMensaje.EVT_PROGRESO,            typeof(EventoProgreso) },
            { TiposMensaje.EVT_LOG,                 typeof(EventoBitacora) },

            // Comandos ----------------------------------------------------
            { TiposMensaje.CMD_PING,                typeof(ComandoPing) },
            { TiposMensaje.CMD_FORZAR_CICLO,        typeof(ComandoForzarCiclo) },
            { TiposMensaje.CMD_PAUSAR,              typeof(ComandoPausar) },
            { TiposMensaje.CMD_REANUDAR,            typeof(ComandoReanudar) },
            { TiposMensaje.CMD_SOLICITAR_SNAPSHOT,  typeof(ComandoSolicitarSnapshot) },
        };

        /// <summary>
        /// Convierte un mensaje a una línea JSON. NO incluye salto de línea
        /// al final — eso lo agrega <c>StreamWriter.WriteLine</c>. Garantiza
        /// que el resultado no contenga saltos de línea internos (en JSON
        /// los strings con \n se escapan a "\\n" automáticamente, así que
        /// una línea por mensaje queda asegurada).
        /// </summary>
        public static string Serializar(MensajeBase mensaje)
        {
            if (mensaje == null) throw new ArgumentNullException("mensaje");
            return _json.Serialize(mensaje);
        }

        /// <summary>
        /// Deserializa una línea NDJSON al subtipo concreto de <see cref="MensajeBase"/>.
        ///
        /// Regresa <c>null</c> si:
        ///   - la línea es vacía o no es JSON válido
        ///   - falta el campo <c>tipo</c>
        ///   - el <c>tipo</c> no está registrado (mensaje desconocido)
        ///   - el JSON es válido pero no encaja en el subtipo esperado
        ///
        /// El receptor debe estar preparado para null y descartarlo — esto
        /// permite forward-compat: un servidor más nuevo puede mandar tipos
        /// que el cliente viejo no entiende y el pipe sigue funcionando.
        /// </summary>
        public static MensajeBase Deserializar(string lineaJson)
        {
            if (string.IsNullOrWhiteSpace(lineaJson)) return null;

            // Primera pasada: leer solo el discriminador sin construir el objeto final.
            Dictionary<string, object> dict;
            try
            {
                dict = _json.Deserialize<Dictionary<string, object>>(lineaJson);
            }
            catch
            {
                return null; // no era JSON válido
            }
            if (dict == null) return null;

            object tipoObj;
            if (!dict.TryGetValue("tipo", out tipoObj)) return null;
            var tipo = tipoObj as string;
            if (string.IsNullOrEmpty(tipo)) return null;

            Type clrType;
            if (!_mapeo.TryGetValue(tipo, out clrType)) return null;

            // Segunda pasada: deserializar al subtipo concreto.
            try
            {
                return (MensajeBase) _json.Deserialize(lineaJson, clrType);
            }
            catch
            {
                return null;
            }
        }
    }
}
