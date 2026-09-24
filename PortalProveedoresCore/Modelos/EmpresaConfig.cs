namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Vista de configuración remota de una empresa, expuesta por el portal CI4
    /// en GET /api/empresas. La consumen:
    ///  - El <b>Configurador</b>, para listar y autorizar/bloquear empresas
    ///    (sin <c>?solo_autorizadas</c>). No usa <see cref="checkpoints"/>.
    ///  - El <b>Servicio Windows</b>, con <c>?solo_autorizadas=1</c>, para
    ///    iterar empresas habilitadas en cada hito (Almacenes, Monedas, ...).
    ///    Lee <see cref="checkpoints"/> para decidir el filtro incremental
    ///    por catálogo y por empresa sin requests adicionales.
    ///
    /// Los nombres de las propiedades son snake_case para que JavaScriptSerializer
    /// las mapee 1:1 contra el JSON que devuelve el endpoint, sin atributos.
    /// </summary>
    public sealed class EmpresaConfig
    {
        public int    emp_id_msp   { get; set; }
        public string nombre       { get; set; }
        public string nombre_largo { get; set; }
        public string rfc          { get; set; }
        public string estatus      { get; set; }  // "Bloqueada" | "Autorizada"
        public string diferencia   { get; set; }  // "S" | "N"
        public string ult_sinc     { get; set; }  // datetime "YYYY-MM-DD HH:MM:SS" o null. Sello del ciclo COMPLETO, NO usar para filtros por catálogo.
        public string sinc_desde   { get; set; }  // datetime o null. null = sincronizar toda la historia. Aplica a DOCUMENTOS (recepciones, facturas), no catálogos.

        /// <summary>
        /// High-water-mark por catálogo: la última FECHA_HORA_ULT_MODIF que el
        /// portal tiene registrada de cada catálogo MSP para esta empresa.
        /// Cada sincronizador (Almacenes, Monedas, ...) lee SU PROPIO checkpoint
        /// y arma su filtro Firebird <c>WHERE FECHA_HORA_ULT_MODIF &gt; checkpoint</c>.
        /// <c>null</c> en un catálogo = portal no tiene nada todavía para esa
        /// empresa = carga inicial (traer TODO).
        /// Solo viene poblado cuando se invoca <c>ListarEmpresasAutorizadasAsync</c>.
        /// </summary>
        public CheckpointsCatalogos checkpoints { get; set; }

        /// <summary>
        /// Override de un solo uso por módulo (tabla SINC_OVERRIDE del portal):
        /// fecha desde la que el operador pidió re-jalar ese módulo, o null.
        /// Viene en los dos listados (Configurador y Servicio). El Servicio lo
        /// aplica una vez y lo borra (DELETE) tras el ack del portal.
        /// <c>null</c> completo = portal viejo sin la función.
        /// </summary>
        public OverridesSinc overrides { get; set; }
    }

    /// <summary>
    /// Bloque <c>overrides</c> de GET /api/empresas. Mismos nombres que
    /// <see cref="ModulosSinc"/>; cada valor es "YYYY-MM-DD HH:MM:SS" o null.
    /// </summary>
    public sealed class OverridesSinc
    {
        public string almacenes   { get; set; }
        public string monedas     { get; set; }
        public string proveedores { get; set; }
        public string recepciones { get; set; }
        public string creditos    { get; set; }
        public string notas       { get; set; }

        /// <summary>Valor crudo del override de un módulo (null si no hay).</summary>
        public string De(string modulo)
        {
            switch (modulo)
            {
                case ModulosSinc.Almacenes:   return almacenes;
                case ModulosSinc.Monedas:     return monedas;
                case ModulosSinc.Proveedores: return proveedores;
                case ModulosSinc.Recepciones: return recepciones;
                case ModulosSinc.Creditos:    return creditos;
                case ModulosSinc.Notas:       return notas;
                default:                      return null;
            }
        }
    }

    /// <summary>
    /// Nombres de módulo que acepta /api/empresas/{id}/override. Uno por
    /// sincronizador: créditos y notas comparten el MAX (tabla CREDITOS) pero
    /// cada uno tiene su propio override.
    /// </summary>
    public static class ModulosSinc
    {
        public const string Almacenes   = "almacenes";
        public const string Monedas     = "monedas";
        public const string Proveedores = "proveedores";
        public const string Recepciones = "recepciones";
        public const string Creditos    = "creditos";
        public const string Notas       = "notas";

        public static readonly string[] Todos = { Almacenes, Monedas, Proveedores, Recepciones, Creditos, Notas };
    }

    /// <summary>
    /// Respuesta de GET/PUT /api/empresas/{id}/override: overrides pendientes
    /// y el MAX actual de cada módulo (tope que un override no puede rebasar).
    /// </summary>
    public sealed class EstadoOverrideSinc
    {
        public int                  emp_id_msp { get; set; }
        public bool                 disponible { get; set; }  // false = falta la tabla en el portal
        public OverridesSinc        overrides  { get; set; }
        public CheckpointsCatalogos maximos    { get; set; }
    }

    /// <summary>
    /// Bloque <c>checkpoints</c> dentro de la respuesta de GET /api/empresas?solo_autorizadas=1.
    /// Cada propiedad es la última fecha (string ISO) que el portal tiene de ese
    /// catálogo MSP para esa empresa, o <c>null</c> si aún no hay datos.
    /// </summary>
    public sealed class CheckpointsCatalogos
    {
        public string almacenes   { get; set; }
        public string monedas     { get; set; }
        public string proveedores { get; set; }
        public string recepciones { get; set; }
        public string creditos    { get; set; }
        public string notas       { get; set; }
        // public string facturas { get; set; }

        /// <summary>Valor crudo del checkpoint de un módulo (ver <see cref="ModulosSinc"/>).</summary>
        public string De(string modulo)
        {
            switch (modulo)
            {
                case ModulosSinc.Almacenes:   return almacenes;
                case ModulosSinc.Monedas:     return monedas;
                case ModulosSinc.Proveedores: return proveedores;
                case ModulosSinc.Recepciones: return recepciones;
                case ModulosSinc.Creditos:    return creditos;
                case ModulosSinc.Notas:       return notas;
                default:                      return null;
            }
        }
    }
}
