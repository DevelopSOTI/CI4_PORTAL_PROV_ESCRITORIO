using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PortalProveedoresEscritorio.Utilidades
{
    /// <summary>
    /// Persiste las preferencias del operador (qué columnas mostrar, anchos,
    /// orden, etc.) en archivos XML bajo
    /// <c>%LocalAppData%\SOTI\PortalProveedoresEscritorio\</c>.
    ///
    /// %LocalAppData% se resuelve a algo como
    /// <c>C:\Users\USUARIO_LOGUEADO\AppData\Local</c> — un path por usuario
    /// que NO requiere permisos elevados y donde cada operador de Windows
    /// tiene su propio juego de preferencias automáticamente.
    ///
    /// Estructura típica:
    /// <code>
    /// C:\Users\Operador1\AppData\Local\SOTI\PortalProveedoresEscritorio\
    ///   Vistas\VistaFacturas\Columnas.xml
    ///     &lt;preferencias&gt;
    ///       &lt;pref name="FOLIO_PROV"&gt;1&lt;/pref&gt;
    ///       &lt;pref name="UUID"&gt;0&lt;/pref&gt;     ← el operador la ocultó
    ///       ...
    ///     &lt;/preferencias&gt;
    /// </code>
    ///
    /// La API es <c>LeerBool</c> / <c>EscribirBool</c> con una "subsección"
    /// que se interpreta como path relativo. Por ejemplo
    /// <c>Vistas\VistaFacturas\Columnas</c> guarda en
    /// <c>Vistas\VistaFacturas\Columnas.xml</c>.
    ///
    /// Hay un cache en memoria por archivo para evitar reabrir el XML en
    /// cada toggle. Nunca lanza: en caso de error (disco lleno, antivirus,
    /// archivo corrupto) devuelve el default y sigue.
    /// </summary>
    public static class PreferenciasUsuario
    {
        private const string CarpetaRaiz   = "SOTI";
        private const string CarpetaApp    = "PortalProveedoresEscritorio";

        // ---- Subsección "Tema" (toggle de colores + caché offline) --------
        // Vive en %LocalAppData%\SOTI\PortalProveedoresEscritorio\Tema.xml,
        // el mismo esquema XML que las vistas de columnas.
        public const string SubseccionTema           = "Tema";
        public const string ClaveRespetarColores     = "RespetarColoresPortal";

        // Cache <subsection, dict-de-claves>. Cada subsection corresponde a
        // un archivo XML. Una vez cargado, las lecturas y escrituras pasan
        // por el dict sin re-leer el archivo.
        private static readonly Dictionary<string, Dictionary<string, string>> _cache
            = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        // Sincronización porque varios controles pueden tocar prefs al mismo
        // tiempo (ej. múltiples toggle rápidos en el menú).
        private static readonly object _lock = new object();

        /// <summary>
        /// Lee una preferencia booleana. Si no existe el archivo o la clave
        /// (primera vez que el operador toca la vista), devuelve
        /// <paramref name="valorDefault"/>.
        /// </summary>
        public static bool LeerBool(string subsection, string nombre, bool valorDefault)
        {
            try
            {
                lock (_lock)
                {
                    var dict = ObtenerDict(subsection);
                    string val;
                    if (dict.TryGetValue(nombre, out val))
                        return val == "1";
                    return valorDefault;
                }
            }
            catch
            {
                return valorDefault;
            }
        }

        /// <summary>
        /// Escribe una preferencia booleana y persiste el archivo XML
        /// completo. La carpeta se crea si no existe.
        /// </summary>
        public static void EscribirBool(string subsection, string nombre, bool valor)
        {
            try
            {
                lock (_lock)
                {
                    var dict = ObtenerDict(subsection);
                    dict[nombre] = valor ? "1" : "0";
                    Persistir(subsection, dict);
                }
            }
            catch
            {
                // Best effort — sin permisos, antivirus, disco lleno → ignoramos.
            }
        }

        /// <summary>
        /// Lee una preferencia de texto. Si no existe el archivo o la clave,
        /// devuelve <paramref name="valorDefault"/>. Nunca lanza.
        /// </summary>
        public static string LeerString(string subsection, string nombre, string valorDefault)
        {
            try
            {
                lock (_lock)
                {
                    var dict = ObtenerDict(subsection);
                    string val;
                    if (dict.TryGetValue(nombre, out val))
                        return val;
                    return valorDefault;
                }
            }
            catch
            {
                return valorDefault;
            }
        }

        /// <summary>
        /// Escribe una preferencia de texto y persiste el archivo XML
        /// completo. Best-effort — nunca lanza.
        /// </summary>
        public static void EscribirString(string subsection, string nombre, string valor)
        {
            try
            {
                lock (_lock)
                {
                    var dict = ObtenerDict(subsection);
                    dict[nombre] = valor ?? "";
                    Persistir(subsection, dict);
                }
            }
            catch
            {
                // Best effort.
            }
        }

        /// <summary>
        /// Carpeta raíz donde viven los XML de preferencias
        /// (<c>%LocalAppData%\SOTI\PortalProveedoresEscritorio</c>). La crea
        /// si no existe. Útil para guardar archivos hermanos de los XML
        /// (ej. el logo del portal cacheado para modo offline).
        /// </summary>
        public static string ObtenerCarpetaBase()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var carpeta = Path.Combine(local, CarpetaRaiz, CarpetaApp);
            Directory.CreateDirectory(carpeta);
            return carpeta;
        }

        // ====================================================================
        // Internals
        // ====================================================================

        private static Dictionary<string, string> ObtenerDict(string subsection)
        {
            Dictionary<string, string> d;
            if (_cache.TryGetValue(subsection, out d)) return d;
            d = Cargar(subsection);
            _cache[subsection] = d;
            return d;
        }

        /// <summary>
        /// Resuelve la ruta absoluta del archivo XML correspondiente a una
        /// subsección. <c>"Vistas\VistaFacturas\Columnas"</c> →
        /// <c>%LocalAppData%\SOTI\PortalProveedoresEscritorio\Vistas\VistaFacturas\Columnas.xml</c>.
        /// La carpeta padre se crea con <c>CreateDirectory</c> (idempotente).
        /// </summary>
        private static string ResolverRuta(string subsection)
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var subsecNorm = (subsection ?? "default").Trim();

            // Separamos carpetas vs nombre del archivo: "a\b\c" → carpeta="a\b", archivo="c.xml".
            var carpetaRelativa = Path.GetDirectoryName(subsecNorm) ?? "";
            var archivo         = Path.GetFileName(subsecNorm);
            if (string.IsNullOrEmpty(archivo)) archivo = "default";

            var carpetaAbs = Path.Combine(local, CarpetaRaiz, CarpetaApp, carpetaRelativa);
            Directory.CreateDirectory(carpetaAbs);
            return Path.Combine(carpetaAbs, archivo + ".xml");
        }

        private static Dictionary<string, string> Cargar(string subsection)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var ruta = ResolverRuta(subsection);
            if (!File.Exists(ruta)) return dict;

            try
            {
                var doc = XDocument.Load(ruta);
                foreach (var el in doc.Descendants("pref"))
                {
                    var attr = el.Attribute("name");
                    if (attr == null || string.IsNullOrEmpty(attr.Value)) continue;
                    dict[attr.Value] = el.Value ?? "";
                }
            }
            catch
            {
                // Archivo corrupto / XML inválido → diccionario vacío. La
                // próxima escritura lo sobreescribirá.
            }
            return dict;
        }

        private static void Persistir(string subsection, Dictionary<string, string> dict)
        {
            var ruta = ResolverRuta(subsection);
            var doc  = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("preferencias",
                    dict.OrderBy(kv => kv.Key)
                        .Select(kv =>
                            new XElement("pref",
                                new XAttribute("name", kv.Key),
                                kv.Value))));

            // Escritura atómica: a un .tmp y luego Move sobre el final. Si
            // el proceso muere a mitad, el archivo viejo no queda corrupto.
            var rutaTmp = ruta + ".tmp";
            doc.Save(rutaTmp);
            if (File.Exists(ruta)) File.Delete(ruta);
            File.Move(rutaTmp, ruta);
        }
    }
}
