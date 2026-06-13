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
    }
}
