using System;
using System.Windows.Forms;
using PortalProveedoresCore.Servicios;

namespace PortalProveedoresConfigurador.Formularios
{
    /// <summary>
    /// Modal que pregunta al operador desde qué fecha/hora se sincronizarán los
    /// documentos (recepciones, créditos, facturas) de una empresa autorizada.
    ///
    /// Comportamiento:
    ///   - DateTimePicker con casilla de verificación: si el operador la
    ///     desmarca, el resultado es <see cref="ValorSincDesde.SincToda"/>
    ///     (NULL en BD = sincronizar toda la historia).
    ///   - Si la deja marcada, el resultado es <see cref="ValorSincDesde.Desde(DateTime)"/>.
    ///   - Si cancela, el llamador debe revertir el cambio de estatus.
    ///
    /// Default sugerido: 1 de enero del año en curso a las 00:00 — patrón muy
    /// común en clientes ("traete del año entrante para acá").
    /// </summary>
    public partial class FormSincDesde : Form
    {
        /// <summary>
        /// Resultado válido solo si <see cref="Form.ShowDialog"/> devolvió OK.
        /// </summary>
        public ValorSincDesde Resultado { get; private set; } = ValorSincDesde.NoTocar;

        public FormSincDesde(string nombreEmpresa, DateTime? fechaActual)
        {
            InitializeComponent();
            lblEmpresa.Text = "Empresa: " + (nombreEmpresa ?? "—");

            // Default: 1 de enero del año en curso a las 00:00. Si la empresa
            // ya tenía una fecha asignada, la cargamos para que el operador la
            // pueda ajustar en lugar de empezar desde cero.
            var anioActual = DateTime.Now.Year;
            var sugerido = new DateTime(anioActual, 1, 1, 0, 0, 0);

            dtpDesde.MinDate = new DateTime(2000, 1, 1);
            dtpDesde.MaxDate = DateTime.Now.AddYears(1);

            if (fechaActual.HasValue)
            {
                dtpDesde.Value   = fechaActual.Value;
                dtpDesde.Checked = true;
            }
            else
            {
                dtpDesde.Value   = sugerido;
                // Cuando es vista por primera vez (sin fecha previa), la dejamos
                // marcada con el sugerido — la mayoría de clientes va a querer
                // un filtro y no "toda la historia".
                dtpDesde.Checked = true;
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            Resultado = dtpDesde.Checked
                ? ValorSincDesde.Desde(dtpDesde.Value)
                : ValorSincDesde.SincToda;
        }
    }
}
