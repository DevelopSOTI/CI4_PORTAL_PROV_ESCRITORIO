namespace PortalProveedoresCore.Modelos
{
    /// <summary>
    /// Día de la semana en que el portal acepta recepciones (tabla DIAS, fija a 7
    /// filas). El Configurador la lista y togglea DIA_RECIBE; no hay altas/bajas.
    /// </summary>
    public sealed class DiaRecepcion
    {
        public int    numero { get; set; }  // 1 = LUNES ... 7 = DOMINGO
        public string nombre { get; set; }
        public bool   recibe { get; set; }
    }
}
