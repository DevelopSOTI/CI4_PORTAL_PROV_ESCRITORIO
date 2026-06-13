namespace PortalProveedoresCore.Configuracion
{
    /// <summary>
    /// Helpers de presentación de valores numéricos hacia humanos. "1740
    /// segundos" se entiende mal a primera vista; "29 min" se lee al
    /// instante. Centralizado para que Configurador, Visor y futuros
    /// proyectos pinten la misma representación.
    /// </summary>
    public static class FormatosHumanos
    {
        /// <summary>
        /// Convierte segundos a la unidad humana más natural:
        ///   1740  → "29 min"
        ///   3600  → "1 h"
        ///   86400 → "1 d"
        ///   45    → "45 s"
        /// Si no es divisible exacto, se queda en segundos.
        /// </summary>
        public static string DuracionCorta(int segundos)
        {
            if (segundos <= 0) return "—";
            if (segundos % 86400 == 0) return (segundos / 86400) + " d";
            if (segundos % 3600  == 0) return (segundos / 3600)  + " h";
            if (segundos % 60    == 0) return (segundos / 60)    + " min";
            return segundos + " s";
        }
    }
}
