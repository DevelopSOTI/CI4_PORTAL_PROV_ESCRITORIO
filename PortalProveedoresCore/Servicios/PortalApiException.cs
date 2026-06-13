using System;
using System.Net;

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
    }
}
