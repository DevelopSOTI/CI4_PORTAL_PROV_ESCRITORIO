using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Servicios;

namespace PortalProveedoresCore.Configuracion
{
    /// <summary>
    /// Paleta visual y marca del Configurador. Se inicia con los defaults del
    /// proyecto (azul / slate) y, si el portal está accesible, se sobrescribe
    /// con los valores de PORTAL_CONFIG al arrancar la app.
    ///
    /// Patrón: las propiedades son <c>static</c> y mutables. Antes de abrir el
    /// form, <see cref="CargarConTimeoutAsync"/> intenta refrescarlas; si falla,
    /// la app sigue con los defaults y queda en <see cref="ModoOffline"/> = true
    /// para que la UI muestre el indicador correspondiente.
    /// </summary>
    public static class Tema
    {
        // ---- Defaults (paleta Tailwind: blue-600 / slate-900) -----------------
        public static Color  Primary       { get; set; } = Color.FromArgb( 37,  99, 235);
        public static Color  PrimaryHover  { get; set; } = Color.FromArgb( 29,  78, 216);
        public static Color  Secondary     { get; set; } = Color.FromArgb( 15,  23,  42);
        public static Color  SecondaryHover{ get; set; } = Color.FromArgb( 30,  41,  59);
        public static Color  Accent        { get; set; } = Color.FromArgb(245, 158,  11);
        public static string NombreApp     { get; set; } = "Portal de Proveedores";
        public static Image  Logo          { get; set; } = null;

        /// <summary>True si la carga del tema falló y estamos usando defaults.</summary>
        public static bool ModoOffline { get; private set; } = true;

        /// <summary>
        /// Intenta cargar el tema desde <c>GET /api/portal-config</c> y, si
        /// hay <c>logo_url</c>, descarga la imagen. Tiene un timeout global —
        /// si el portal no responde en N segundos, deja los defaults y marca
        /// modo offline. Nunca lanza: silencia errores y los reporta vía la
        /// bandera <see cref="ModoOffline"/>.
        /// </summary>
        public static async Task CargarConTimeoutAsync(IPortalApi api, TimeSpan timeout)
        {
            if (api == null) { ModoOffline = true; return; }

            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    var tema = await api.ObtenerTemaAsync(cts.Token).ConfigureAwait(false);
                    if (tema == null) { ModoOffline = true; return; }

                    if (!string.IsNullOrWhiteSpace(tema.nombre))         NombreApp    = tema.nombre;
                    if (!string.IsNullOrWhiteSpace(tema.color_primary))       Primary       = HexAColor(tema.color_primary,       Primary);
                    if (!string.IsNullOrWhiteSpace(tema.color_primary_hover)) PrimaryHover  = HexAColor(tema.color_primary_hover, PrimaryHover);
                    if (!string.IsNullOrWhiteSpace(tema.color_secondary))     Secondary     = HexAColor(tema.color_secondary,     Secondary);
                    if (!string.IsNullOrWhiteSpace(tema.color_accent))        Accent        = HexAColor(tema.color_accent,        Accent);
                    SecondaryHover = Aclarar(Secondary, 15);

                    if (!string.IsNullOrWhiteSpace(tema.logo_url))
                        Logo = await DescargarImagenAsync(tema.logo_url, cts.Token).ConfigureAwait(false);

                    ModoOffline = false;
                }
                catch
                {
                    // Cualquier fallo (red, timeout, JSON inválido, API key mala)
                    // deja los defaults. La UI sigue siendo usable.
                    ModoOffline = true;
                }
            }
        }

        /// <summary>
        /// Convierte <c>#RRGGBB</c> (o <c>#RGB</c>) a <see cref="Color"/>. Si
        /// el formato no es reconocible, regresa el fallback — nunca lanza.
        /// </summary>
        public static Color HexAColor(string hex, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(hex)) return fallback;
            hex = hex.Trim().TrimStart('#');

            // Expande #RGB → #RRGGBB
            if (hex.Length == 3)
                hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);

            if (hex.Length != 6 && hex.Length != 8) return fallback;

            try
            {
                if (hex.Length == 6)
                {
                    var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    var b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    return Color.FromArgb(r, g, b);
                }
                else
                {
                    var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    var b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    var a = Convert.ToInt32(hex.Substring(6, 2), 16);
                    return Color.FromArgb(a, r, g, b);
                }
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>
        /// Devuelve un Color un poco más claro que el dado (para hover sobre
        /// botones oscuros). Si el componente ya está cerca de 255 se queda
        /// en 255 — sin overflow.
        /// </summary>
        public static Color Aclarar(Color c, int delta)
        {
            return Color.FromArgb(
                Math.Min(255, c.R + delta),
                Math.Min(255, c.G + delta),
                Math.Min(255, c.B + delta));
        }

        /// <summary>
        /// Descarga una imagen (JPG/PNG) y la deserializa a <see cref="Image"/>.
        /// Devuelve null si falla; nunca lanza. Usa su propio HttpClient
        /// pequeño porque el logo suele estar en la raíz del portal sin
        /// requerir X-API-Key (sirve como asset estático de Apache).
        /// </summary>
        private static async Task<Image> DescargarImagenAsync(string url, CancellationToken ct)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(5);
                    var bytes = await http.GetByteArrayAsync(url).ConfigureAwait(false);
                    using (var ms = new MemoryStream(bytes))
                    {
                        // Image.FromStream requiere que el stream siga vivo el
                        // tiempo que el Image exista. Lo evitamos clonando.
                        using (var tmp = Image.FromStream(ms))
                            return new Bitmap(tmp);
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
