using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Servicios;
using PortalProveedoresEscritorio.Formularios;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio
{
    /// <summary>
    /// Entry point del Escritorio. Antes de mostrar la UI, intenta cargar
    /// el <see cref="Tema"/> del portal (PORTAL_CONFIG: paleta, nombre y
    /// logo) con un timeout corto. Si el portal no responde, deja los
    /// defaults — la app funciona igual, solo se ve sin la marca del cliente.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Handlers globales (igual que en el Visor).
            Application.ThreadException += (s, e) =>
                MostrarError("Error inesperado", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                MostrarError("Error fatal", e.ExceptionObject as Exception);
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                MostrarError("Tarea en background falló", e.Exception);
                e.SetObserved();
            };

            // Cargar tema del portal: paleta + nombre + logo del cliente.
            // Si HKLM no tiene URL/API key o el portal está offline, deja
            // los defaults (azul Tailwind). La UI se construye después.
            CargarTemaSeguro();

            Application.Run(new FormLogin());
        }

        // Archivo hermano de los XML de preferencias donde se cachea el logo
        // del portal para que aparezca aunque no haya internet.
        private const string ArchivoLogoCache = "logo_portal.png";

        private static void CargarTemaSeguro()
        {
            // MEJORA 1 — el operador puede pedir que la app ignore los
            // colores del portal (paleta default azul/slate). El logo y el
            // nombre del portal SÍ se siguen usando: el toggle es solo de
            // colores.
            bool respetarColores = PreferenciasUsuario.LeerBool(
                PreferenciasUsuario.SubseccionTema,
                PreferenciasUsuario.ClaveRespetarColores,
                valorDefault: true);

            // Paleta default de Tema.cs capturada ANTES de aplicar nada del
            // portal — es a lo que regresamos si respetarColores = false.
            Color defPrimary        = Tema.Primary;
            Color defPrimaryHover   = Tema.PrimaryHover;
            Color defSecondary      = Tema.Secondary;
            Color defSecondaryHover = Tema.SecondaryHover;
            Color defAccent         = Tema.Accent;

            bool online = false;
            try
            {
                var reg = new RegistrosWindows();
                if (reg.LeerRegistros(false)
                    && !string.IsNullOrWhiteSpace(reg.PORTAL_BASE_URL)
                    && !string.IsNullOrWhiteSpace(reg.PORTAL_API_KEY))
                {
                    var api = new PortalApi(reg.PORTAL_BASE_URL, reg.PORTAL_API_KEY);
                    // .GetAwaiter().GetResult() bloquea el hilo principal hasta
                    // 3 segundos; aceptable en Main() porque NO hay UI todavía.
                    Tema.CargarConTimeoutAsync(api, TimeSpan.FromSeconds(3))
                        .GetAwaiter().GetResult();
                    online = !Tema.ModoOffline;
                }
            }
            catch
            {
                // Cualquier excepción (red, DPAPI, HKLM faltante) deja
                // Tema en defaults y modo offline. La app sigue.
            }

            // MEJORA 3 — caché offline en %LocalAppData%: tras una descarga
            // exitosa guardamos colores + nombre (Tema.xml) y logo
            // (logo_portal.png); si el portal no respondió, usamos lo último
            // cacheado para que la app abra con la misma apariencia.
            if (online) GuardarTemaEnCache();
            else        AplicarTemaDesdeCache();

            if (!respetarColores)
            {
                // Revertimos SOLO los colores a la paleta default propia.
                // NombreApp y Logo (del portal o del caché) se conservan.
                Tema.Primary        = defPrimary;
                Tema.PrimaryHover   = defPrimaryHover;
                Tema.Secondary      = defSecondary;
                Tema.SecondaryHover = defSecondaryHover;
                Tema.Accent         = defAccent;
            }
        }

        /// <summary>
        /// Persiste el tema recién descargado del portal (colores + nombre en
        /// el XML de preferencias, logo como PNG) para usarlo como fallback
        /// cuando no haya internet. Best-effort: cualquier fallo de disco se
        /// ignora — el caché es una comodidad, no un requisito.
        /// </summary>
        private static void GuardarTemaEnCache()
        {
            try
            {
                var s = PreferenciasUsuario.SubseccionTema;
                PreferenciasUsuario.EscribirString(s, "CacheNombre",    Tema.NombreApp ?? "");
                PreferenciasUsuario.EscribirString(s, "CachePrimary",   ColorAHex(Tema.Primary));
                PreferenciasUsuario.EscribirString(s, "CachePrimaryHover", ColorAHex(Tema.PrimaryHover));
                PreferenciasUsuario.EscribirString(s, "CacheSecondary", ColorAHex(Tema.Secondary));
                PreferenciasUsuario.EscribirString(s, "CacheAccent",    ColorAHex(Tema.Accent));
            }
            catch { }

            try
            {
                if (Tema.Logo != null)
                {
                    var ruta = Path.Combine(
                        PreferenciasUsuario.ObtenerCarpetaBase(), ArchivoLogoCache);
                    // Clon antes de Save: evita "GDI+ generic error" si el
                    // Image original quedó atado a un stream ya cerrado.
                    using (var copia = new Bitmap(Tema.Logo))
                        copia.Save(ruta, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch { }
        }

        /// <summary>
        /// Fallback offline: restaura el último tema descargado (nombre +
        /// colores desde Tema.xml, logo desde logo_portal.png). Si no hay
        /// caché o está corrupto, deja los defaults — mismo comportamiento
        /// de siempre. Nunca lanza.
        /// </summary>
        private static void AplicarTemaDesdeCache()
        {
            try
            {
                var s = PreferenciasUsuario.SubseccionTema;

                var nombre = PreferenciasUsuario.LeerString(s, "CacheNombre", "");
                if (!string.IsNullOrWhiteSpace(nombre)) Tema.NombreApp = nombre;

                Tema.Primary      = Tema.HexAColor(PreferenciasUsuario.LeerString(s, "CachePrimary",      ""), Tema.Primary);
                Tema.PrimaryHover = Tema.HexAColor(PreferenciasUsuario.LeerString(s, "CachePrimaryHover", ""), Tema.PrimaryHover);
                Tema.Secondary    = Tema.HexAColor(PreferenciasUsuario.LeerString(s, "CacheSecondary",    ""), Tema.Secondary);
                Tema.Accent       = Tema.HexAColor(PreferenciasUsuario.LeerString(s, "CacheAccent",       ""), Tema.Accent);
                Tema.SecondaryHover = Tema.Aclarar(Tema.Secondary, 15);
            }
            catch { }

            try
            {
                var ruta = Path.Combine(
                    PreferenciasUsuario.ObtenerCarpetaBase(), ArchivoLogoCache);
                if (File.Exists(ruta))
                {
                    // Leemos bytes y clonamos para no dejar el archivo
                    // bloqueado (Image.FromFile mantiene el handle abierto).
                    var bytes = File.ReadAllBytes(ruta);
                    using (var ms = new MemoryStream(bytes))
                    using (var tmp = Image.FromStream(ms))
                        Tema.Logo = new Bitmap(tmp);
                }
            }
            catch { }
        }

        private static string ColorAHex(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private static void MostrarError(string titulo, Exception ex)
        {
            if (ex == null) return;
            try
            {
                MessageBox.Show(
                    ex.GetType().Name + ": " + ex.Message + "\n\n" + ex.StackTrace,
                    titulo,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch { }
        }
    }
}
