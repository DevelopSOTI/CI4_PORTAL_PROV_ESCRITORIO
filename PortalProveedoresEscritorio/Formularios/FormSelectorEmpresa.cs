using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresEscritorio.Servicios;
using PortalProveedoresEscritorio.Utilidades;

namespace PortalProveedoresEscritorio.Formularios
{
    /// <summary>
    /// Diálogo modal para escoger la empresa con la que el operador va a
    /// trabajar. Réplica funcional de <c>F_SELECT_EMP</c> del SOAP, con look
    /// consistente con <see cref="FormLogin"/>. La paleta y los hovers se
    /// aplican en <see cref="AplicarTemaYHandlers"/> tras
    /// <c>InitializeComponent</c>.
    /// </summary>
    public partial class FormSelectorEmpresa : Form
    {
        public EmpresaEscritorio EmpresaSeleccionada { get; private set; }

        public FormSelectorEmpresa(IEnumerable<EmpresaEscritorio> empresas)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Resize += (s, e) => UiHelpers.AplicarEsquinasRedondeadas(this, 16);

            if (empresas != null)
                foreach (var e in empresas)
                    listEmpresas.Items.Add(e);

            if (listEmpresas.Items.Count > 0)
                listEmpresas.SelectedIndex = 0;

            AplicarTemaYHandlers();
        }

        private void AplicarTemaYHandlers()
        {
            Color fondoForm  = Tema.Secondary;
            Color fondoInput = Tema.Aclarar(Tema.Secondary, 28);

            this.BackColor                = fondoForm;
            this.listEmpresas.BackColor   = fondoInput;
            this.btnCancelar.BackColor    = fondoInput;
            this.btnCancelar.FlatAppearance.MouseOverBackColor = Tema.Aclarar(Tema.Secondary, 50);
            this.btnAceptar.BackColor     = Tema.Primary;
            this.btnAceptar.FlatAppearance.MouseOverBackColor = Tema.PrimaryHover;
            this.btnAceptar.FlatAppearance.MouseDownBackColor = Tema.PrimaryHover;

            UiHelpers.EngancharDragNativo(this.panelTitleBar, this);

            Color textoTenue = Color.FromArgb(160, 180, 200);
            UiHelpers.ConfigurarBotonCerrar(this.btnCerrar, textoTenue, () => btnCancelar_Click(this, EventArgs.Empty));

            this.btnAceptar.Paint  += (s, e) => UiHelpers.DibujarBordePill(this.btnAceptar,  10);
            this.btnCancelar.Paint += (s, e) => UiHelpers.DibujarBordePill(this.btnCancelar, 10);

            UiHelpers.AplicarEsquinasRedondeadas(this, 16);
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (listEmpresas.Items.Count == 0)
            {
                // Mensaje literal del SOAP legacy (F_SELECT_EMP.cs:51).
                MessageBox.Show(
                    "No hay empresas para seleccionar, favor de verificar la conexión con Microsip",
                    "No hay empresas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            EmpresaSeleccionada = listEmpresas.SelectedItem as EmpresaEscritorio;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            EmpresaSeleccionada = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listEmpresas_DoubleClick(object sender, EventArgs e) => btnAceptar_Click(sender, e);
    }
}
