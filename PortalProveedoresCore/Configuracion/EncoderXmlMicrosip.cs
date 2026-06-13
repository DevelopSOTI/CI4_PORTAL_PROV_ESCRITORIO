using System.Text;

namespace PortalProveedoresCore.Configuracion
{
    /// <summary>
    /// Preparador de XML del CFDI para insertarlo en <c>REPOSITORIO_CFDI.XML</c>
    /// de Microsip. Replica BIT A BIT la transformación que hace el Delphi en
    /// <c>Func_Facturas_3_3.pas:931-934</c> (mismo bloque en
    /// <c>Func_Complementos.pas:423-424</c>):
    ///
    /// <code>
    /// Utf8Bytes := TEncoding.Convert(TEncoding.UTF8, TEncoding.GetEncoding(28591), TEncoding.UTF8.GetBytes(XML));
    /// XML := TEncoding.ASCII.GetString(Utf8Bytes);
    /// </code>
    ///
    /// Más una pasada previa de <c>CleanXMLText</c> que elimina el BOM
    /// (Func_Facturas_3_3.pas:261-272):
    ///
    /// <code>
    /// // Caso 1: BOM Unicode real (U+FEFF)
    /// if (Length(Result) > 0) and (Result[1] = #$FEFF) then Delete(Result, 1, 1);
    /// // Caso 2: BOM convertido a caracteres visibles "﻿" (EF BB BF interpretado como Latin1)
    /// if Result.StartsWith('﻿') then Delete(Result, 1, 3);
    /// </code>
    ///
    /// NOTA SOBRE LA PARIDAD CON DELPHI: la cadena resultante de
    /// <c>ASCII.GetString(latin1_bytes)</c> reemplaza los caracteres
    /// acentuados (bytes 128-255) por '?'. El Delphi tiene exactamente este
    /// comportamiento — es lo que hay en producción desde hace años y Microsip
    /// lo acepta, así que NO intentamos "mejorarlo". Replicamos paridad exacta
    /// hasta validar que la Fase 2 funciona; si después se descubre un problema
    /// con acentos lo discutimos con el cliente.
    /// </summary>
    public static class EncoderXmlMicrosip
    {
        private static readonly Encoding _latin1 = Encoding.GetEncoding(28591);

        /// <summary>
        /// Toma el XML UTF-16 (string .NET nativo) que vino del portal,
        /// limpia el BOM y aplica el doble encoding UTF-8 → ISO-8859-1 → ASCII.
        /// </summary>
        public static string PrepararParaMicrosip(string xmlUtf16)
        {
            if (string.IsNullOrEmpty(xmlUtf16)) return string.Empty;

            var limpio = LimpiarBom(xmlUtf16);

            // 1) UTF-16 → bytes UTF-8 (representación binaria del texto)
            var utf8Bytes = Encoding.UTF8.GetBytes(limpio);

            // 2) Convertir esos bytes a ISO-8859-1 (Latin1). El método
            //    Encoding.Convert sí reinterpreta el contenido: por cada
            //    runa UTF-8 produce su equivalente en Latin1 (con fallback
            //    '?' para los puntos de código fuera de 0-255).
            var latin1Bytes = Encoding.Convert(Encoding.UTF8, _latin1, utf8Bytes);

            // 3) ASCII.GetString de los bytes Latin1 — los bytes 128-255 caen
            //    en el fallback de ASCIIEncoding (replacement '?'). Mismo
            //    comportamiento que el Delphi en producción.
            return Encoding.ASCII.GetString(latin1Bytes);
        }

        /// <summary>
        /// Elimina el BOM del XML en sus dos variantes posibles: el real
        /// U+FEFF (cuando viene como Unicode bien formado) o el BOM corrupto
        /// EF BB BF interpretado como tres caracteres Latin1 visibles
        /// (caso típico cuando MySQL devuelve bytes UTF-8 como si fueran
        /// Latin1). Replica <c>CleanXMLText</c> del Delphi.
        /// </summary>
        public static string LimpiarBom(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return xml;

            // Caso 1: BOM Unicode real
            if (xml[0] == '﻿')
                xml = xml.Substring(1);

            // Caso 2: BOM EF BB BF interpretado como Latin1 — son los chars
            // 'ï' (0xEF), '»' (0xBB), '¿' (0xBF), o sus equivalentes en
            // distintas representaciones. El Delphi compara literalmente
            // con el string '﻿' (que en su encoding source es la secuencia
            // de 3 chars). En C# escribimos el string mismo:
            if (xml.Length >= 3
                && xml[0] == 'ï'
                && xml[1] == '»'
                && xml[2] == '¿')
            {
                xml = xml.Substring(3);
            }

            return xml;
        }
    }
}
