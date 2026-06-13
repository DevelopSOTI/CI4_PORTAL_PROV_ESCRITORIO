using System;

namespace PortalProveedoresCore.Pipes
{
    /// <summary>
    /// Cabecera común de todos los mensajes del pipe. JavaScriptSerializer
    /// incluye estas propiedades en cada mensaje serializado; el campo
    /// <see cref="tipo"/> sirve de discriminador para que el receptor sepa
    /// a qué clase deserializar la línea NDJSON.
    ///
    /// Convención snake_case en los nombres para alinear con el JSON de las
    /// otras APIs del proyecto y evitar atributos de mapping.
    /// </summary>
    public abstract class MensajeBase
    {
        /// <summary>Discriminador. Ver <see cref="TiposMensaje"/>.</summary>
        public string tipo { get; set; }

        /// <summary>Timestamp local del momento en que el emisor creó el mensaje.</summary>
        public DateTime ts { get; set; }

        /// <summary>Versión del protocolo. Coincide con <see cref="ConstantesPipe.VersionProtocolo"/>.</summary>
        public int version { get; set; }

        /// <summary>
        /// Constructor que asigna automáticamente <see cref="ts"/> y
        /// <see cref="version"/>. Cada subclase llama base() y pone su propio
        /// <see cref="tipo"/> en su propio constructor.
        /// </summary>
        protected MensajeBase()
        {
            ts      = DateTime.Now;
            version = ConstantesPipe.VersionProtocolo;
        }
    }
}
