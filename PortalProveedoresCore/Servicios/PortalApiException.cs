using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;

namespace PortalProveedoresCore.Servicios
{
    /// <summary>
    /// Falla en una llamada al portal CI4. Incluye el status code para que el
    /// llamador pueda decidir reintento (5xx) vs error de configuración (4xx).
    /// </summary>
    public sealed class PortalApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string Cuerpo { get; }

        public PortalApiException(string mensaje, HttpStatusCode statusCode, string cuerpo)
            : base(mensaje)
        {
            StatusCode = statusCode;
            Cuerpo     = cuerpo;
        }

        /// <summary>
        /// Mensaje legible que mandó el portal en el cuerpo JSON (clave
        /// "mensaje", o "error" si no hay), para mostrarlo tal cual al
        /// operador. null si el cuerpo no es JSON o no trae ninguna.
        /// </summary>
        public string MensajePortal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Cuerpo)) return null;
                try
                {
                    var d = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(Cuerpo);
                    object v;
                    if (d != null && d.TryGetValue("mensaje", out v) && v is string && ((string) v).Length > 0) return (string) v;
                    if (d != null && d.TryGetValue("error", out v) && v is string && ((string) v).Length > 0) return (string) v;
                }
                catch { }
                return null;
            }
        }
    }
}
