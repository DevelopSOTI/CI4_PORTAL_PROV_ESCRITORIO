namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Parámetro de negocio del portal (tabla PARAMETROS). El Configurador la
    /// administra a través de GET /api/parametros y PATCH /api/parametros.
    ///
    /// La descripción la sirve el backend (columna PARAM_DESCRIPCION) — no es
    /// un diccionario hardcoded en el cliente. Eso permite agregar parámetros
    /// nuevos sin tener que actualizar el Configurador.
    /// </summary>
    public sealed class ParametroPortal
    {
        public string clave       { get; set; }
        public string valor       { get; set; }
        public string descripcion { get; set; }
    }

    /// <summary>
    /// Resumen que devuelve el endpoint PATCH /api/parametros: cuántos cambios
    /// se aplicaron y cuáles claves se omitieron por auto-managed (LAST_UPDATE)
    /// o por no existir. Útil para que el Configurador muestre un mensaje claro
    /// en lugar de "se guardó todo" cuando no fue así.
    /// </summary>
    public sealed class ResumenActualizacionParametros
    {
        public int      aplicados      { get; set; }
        public string[] ignorados_auto { get; set; }
        public string[] no_encontrados { get; set; }
    }
}
