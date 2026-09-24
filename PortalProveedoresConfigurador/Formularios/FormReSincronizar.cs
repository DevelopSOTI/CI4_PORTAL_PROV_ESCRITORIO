using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Servicios;

namespace PortalProveedoresConfigurador.Formularios
{
    /// <summary>
    /// Modal para pedir una re-sincronización de un solo uso por módulo
    /// (tabla SINC_OVERRIDE del portal). El operador marca solo los módulos
    /// que quiere volver a traer y la fecha de cada uno; el servicio los re-jala
    /// una vez en su siguiente ciclo y borra el pendiente.
    ///
    /// Reglas:
    ///   - La fecha nunca puede ser posterior al último dato que ya tiene el
    ///     portal de ese módulo (el DateTimePicker se topa ahí y el portal
    ///     además lo valida con 422).
    ///   - Desmarcar un módulo que tenía un pendiente lo cancela (DELETE).
    ///
    /// Habla con el portal por sí mismo (GET al abrir, PUT/DELETE al guardar)
    /// para poder mostrar el error de cada módulo sin cerrarse.
    /// </summary>
    public partial class FormReSincronizar : Form
    {
        private sealed class Fila
        {
            public string         Modulo;
            public string         Nombre;
            public CheckBox       Chk;
            public DateTimePicker Dtp;
            public Label          LblMax;
            public DateTime?      Maximo;     // MAX actual del portal (null = sin datos)
            public DateTime?      Pendiente;  // override ya armado (null = ninguno)
        }

        private static readonly DateTime FechaMinima = new DateTime(2000, 1, 1);

        private readonly IPortalApi _api;
        private readonly int _idMsp;
        private readonly Fila[] _filas;

        /// <summary>True mientras corre el guardado (PUT/DELETE al portal).</summary>
        private bool _guardando;

        /// <summary>
        /// Overrides pendientes tal como quedaron en el portal la última vez que
        /// se consultaron (al abrir o tras guardar). null si nunca se pudo
        /// consultar. El llamador lo usa para refrescar la columna del grid,
        /// aunque el operador haya cerrado con Cancelar tras un guardado parcial.
        /// </summary>
        public OverridesSinc Resultado { get; private set; }

        public FormReSincronizar(IPortalApi api, int idMsp, string nombreEmpresa)
        {
            InitializeComponent();

            _api   = api;
            _idMsp = idMsp;
            lblEmpresa.Text = "Empresa: " + (string.IsNullOrWhiteSpace(nombreEmpresa) ? "—" : nombreEmpresa);

            _filas = new[]
            {
                new Fila { Modulo = ModulosSinc.Almacenes,   Nombre = "Almacenes",        Chk = chkAlmacenes,   Dtp = dtpAlmacenes,   LblMax = lblMaxAlmacenes },
                new Fila { Modulo = ModulosSinc.Monedas,     Nombre = "Monedas",          Chk = chkMonedas,     Dtp = dtpMonedas,     LblMax = lblMaxMonedas },
                new Fila { Modulo = ModulosSinc.Proveedores, Nombre = "Proveedores",      Chk = chkProveedores, Dtp = dtpProveedores, LblMax = lblMaxProveedores },
                new Fila { Modulo = ModulosSinc.Recepciones, Nombre = "Recepciones",      Chk = chkRecepciones, Dtp = dtpRecepciones, LblMax = lblMaxRecepciones },
                new Fila { Modulo = ModulosSinc.Creditos,    Nombre = "Créditos",         Chk = chkCreditos,    Dtp = dtpCreditos,    LblMax = lblMaxCreditos },
                new Fila { Modulo = ModulosSinc.Notas,       Nombre = "Notas de crédito", Chk = chkNotas,       Dtp = dtpNotas,       LblMax = lblMaxNotas },
            };

            foreach (var f in _filas) f.Chk.Enabled = false;   // hasta que llegue el estado del portal
        }

        private async void FormReSincronizar_Load(object sender, EventArgs e)
        {
            MarcarEstado("Consultando al portal...", null);
            try
            {
                var estado = await _api.ObtenerOverridesAsync(_idMsp, CancellationToken.None);
                if (IsDisposed) return;   // lo cerraron mientras consultaba
                if (estado == null || !estado.disponible)
                {
                    MarcarEstado("El portal todavía no tiene esta función (falta la tabla SINC_OVERRIDE).", false);
                    return;
                }

                Cargar(estado);
                btnGuardar.Enabled = true;
            }
            catch (PortalApiException px)
            {
                MarcarEstado("Error " + (int) px.StatusCode + " del portal: " + (px.MensajePortal ?? px.Message), false);
            }
            catch (Exception ex)
            {
                MarcarEstado("No se pudo consultar al portal: " + ex.Message, false);
            }
        }

        private void Cargar(EstadoOverrideSinc estado)
        {
            Resultado = estado.overrides;
            var pendientes = new List<string>();

            foreach (var f in _filas)
            {
                f.Maximo    = Parsear(estado.maximos   != null ? estado.maximos.De(f.Modulo)   : null);
                f.Pendiente = Parsear(estado.overrides != null ? estado.overrides.De(f.Modulo) : null);

                // Orden importa: MaxDate antes de Value, o el picker lanza.
                f.Dtp.MinDate = FechaMinima;
                f.Dtp.MaxDate = f.Maximo.HasValue && f.Maximo.Value > FechaMinima ? f.Maximo.Value : DateTime.Now;
                f.Dtp.Value   = Acotar(f.Dtp, f.Pendiente ?? Sugerida(f.Maximo));

                f.LblMax.Text = f.Maximo.HasValue ? f.Maximo.Value.ToString("dd/MM/yyyy HH:mm") : "Sin datos aún";

                f.Chk.Enabled = true;
                f.Chk.Checked = f.Pendiente.HasValue;
                f.Chk.Font    = new Font(f.Chk.Font, f.Pendiente.HasValue ? FontStyle.Bold : FontStyle.Regular);
                f.Dtp.Enabled = f.Chk.Checked;

                if (f.Pendiente.HasValue) pendientes.Add(f.Nombre);
            }

            MarcarEstado(pendientes.Count > 0
                ? "Pendientes para el siguiente ciclo: " + string.Join(", ", pendientes) + ". Desmarca uno para cancelarlo."
                : "", null);
        }

        /// <summary>
        /// Mientras se guarda no se deja cerrar con la X / Alt+F4: se perderían
        /// los errores por módulo y los PUT/DELETE seguirían corriendo sobre un
        /// form ya desechado, con la columna del grid desactualizada.
        /// </summary>
        private void FormReSincronizar_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_guardando && e.CloseReason == CloseReason.UserClosing) e.Cancel = true;
        }

        private void chkModulo_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var f in _filas)
                if (ReferenceEquals(f.Chk, sender)) f.Dtp.Enabled = f.Chk.Checked;
        }

        private async void btnGuardar_Click(object sender, EventArgs e)
        {
            _guardando          = true;
            btnGuardar.Enabled  = false;
            btnCancelar.Enabled = false;
            MarcarEstado("Guardando...", null);

            var errores = new List<string>();
            foreach (var f in _filas)
            {
                try
                {
                    if (!f.Chk.Checked)
                    {
                        if (f.Pendiente.HasValue)
                            await _api.BorrarOverrideAsync(_idMsp, f.Modulo, CancellationToken.None);
                        continue;
                    }

                    if (f.Pendiente.HasValue && f.Pendiente.Value == f.Dtp.Value) continue; // sin cambio

                    await _api.PonerOverrideAsync(_idMsp, f.Modulo, f.Dtp.Value, CancellationToken.None);
                }
                catch (PortalApiException px)
                {
                    errores.Add(f.Nombre + ": " + (px.MensajePortal ?? ("error " + (int) px.StatusCode + " del portal")));
                }
                catch (Exception ex)
                {
                    errores.Add(f.Nombre + ": " + ex.Message);
                }
            }

            // Estado real del portal tras guardar. Solo refresca los pendientes:
            // no pisa lo que el operador tiene en pantalla para los que fallaron.
            try
            {
                var estado = await _api.ObtenerOverridesAsync(_idMsp, CancellationToken.None);
                if (estado != null)
                {
                    Resultado = estado.overrides;
                    foreach (var f in _filas)
                        f.Pendiente = Parsear(estado.overrides != null ? estado.overrides.De(f.Modulo) : null);
                }
            }
            catch { /* el grid se queda con lo último conocido */ }

            _guardando = false;
            if (errores.Count == 0)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            MarcarEstado(string.Join(Environment.NewLine, errores), false);
            btnGuardar.Enabled  = true;
            btnCancelar.Enabled = true;
        }

        /// <summary>Default al marcar un módulo: día 1 del mes en curso, topado al MAX.</summary>
        private static DateTime Sugerida(DateTime? maximo)
        {
            var hoy = DateTime.Now;
            var sugerida = new DateTime(hoy.Year, hoy.Month, 1);
            return maximo.HasValue && maximo.Value < sugerida ? maximo.Value.Date : sugerida;
        }

        private static DateTime Acotar(DateTimePicker dtp, DateTime valor)
        {
            if (valor < dtp.MinDate) return dtp.MinDate;
            if (valor > dtp.MaxDate) return dtp.MaxDate;
            return valor;
        }

        private static DateTime? Parsear(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            DateTime dt;
            return DateTime.TryParseExact(raw, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                ? dt : (DateTime?) null;
        }

        private void MarcarEstado(string mensaje, bool? exito)
        {
            lblEstado.Text = mensaje;
            if (exito == false) lblEstado.ForeColor = Color.FromArgb(220, 38, 38);
            else                lblEstado.ForeColor = Color.FromArgb(100, 116, 139);
        }
    }
}
