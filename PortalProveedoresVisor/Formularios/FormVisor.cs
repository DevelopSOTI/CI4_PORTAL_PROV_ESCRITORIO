using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Pipes;

namespace PortalProveedoresVisor.Formularios
{
    /// <summary>
    /// Ventana principal del Visor. Diseño dark estilo dashboard/terminal con
    /// header colorable desde PORTAL_CONFIG (Tema): logo del cliente, nombre
    /// del portal, indicador de estado del servicio y barra de progreso.
    ///
    /// Comportamiento:
    ///   - Vive en la bandeja del sistema. X o minimizar la esconden, NO la
    ///     cierran. Salir explícito desde menú del tray.
    ///   - Tray icon dinámico: verde=ok, azul=ciclo en curso, ámbar=pausado,
    ///     rojo=desconectado.
    ///   - Toast (ShowBalloonTip) en eventos críticos: arranque/cierre del
    ///     servicio, fallas de ciclo, fallas de paso.
    ///   - Menú tray con comandos: Forzar ciclo, Pausar, Reanudar.
    /// </summary>
    public partial class FormVisor : Form
    {
        // -------- Comportamiento de ventana --------
        private readonly bool _arrancaEnTray;
        private bool _suprimirShowInicial;
        private bool _cierreReal;

        // -------- Conexión con el servicio --------
        private ClientePipe _cliente;
        private bool _logVacio = true;

        // Para separar visualmente los pasos del ciclo: insertamos una línea
        // en blanco antes de cada "→ Paso: X", EXCEPTO el primero del ciclo
        // (que va pegado al header "▶ Ciclo #N iniciado"). Sin esto, los pasos
        // se ven amontonados y cuesta distinguir dónde termina uno y empieza otro.
        private bool _esPrimerPasoDelCiclo = true;

        // -------- Tray icons cacheados (Bitmap → Icon → cleanup en Dispose) --
        private Icon _iconoConectado;
        private Icon _iconoSincronizando;
        private Icon _iconoPausado;
        private Icon _iconoDesconectado;

        // -------- Estado actual del servicio (para decidir colores/iconos) --
        private string _ultimoEstado = "";
        private bool   _ultimaConexion;

        // -------- Paleta del log (semántica, no de marca) --------------------
        private static readonly Color ColorTimestamp = Color.FromArgb(100, 116, 139);
        private static readonly Color ColorFuente    = Color.FromArgb(148, 163, 184);
        private static readonly Color ColorMensaje   = Color.FromArgb(226, 232, 240);
        private static readonly Color ColorInfo      = Color.FromArgb( 96, 165, 250);
        private static readonly Color ColorWarning   = Color.FromArgb(251, 191,  36);
        private static readonly Color ColorError     = Color.FromArgb(248, 113, 113);
        private static readonly Color ColorSuccess   = Color.FromArgb( 74, 222, 128);

        /// <summary>
        /// Color del separador horizontal entre pasos del ciclo.
        /// Gris azulado tenue para que la línea se note sin competir con el contenido.
        /// </summary>
        private static readonly Color ColorSeparador = Color.FromArgb( 71,  85, 105);

        // Win32: DestroyIcon para liberar los handles que generamos con Bitmap.GetHicon().
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        public FormVisor(bool arrancaEnTray)
        {
            _arrancaEnTray       = arrancaEnTray;
            _suprimirShowInicial = arrancaEnTray;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AplicarTema();
            ConstruirIconosTray();

            // Estado inicial del header y tray.
            lblEstadoLinea.Text      = "Estado: conectando al servicio…";
            lblEstadoLinea.ForeColor = ColorFuente;
            ActualizarIconoTray("desconectado");
            ActualizarHabilitacionComandos(estadoConocido: false);

            // Suscripción al pipe.
            _cliente = new ClientePipe();
            _cliente.MensajeRecibido += Cliente_MensajeRecibido;
            _cliente.ConexionCambio  += Cliente_ConexionCambio;
            _cliente.Iniciar();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_cliente != null)
            {
                try
                {
                    _cliente.MensajeRecibido -= Cliente_MensajeRecibido;
                    _cliente.ConexionCambio  -= Cliente_ConexionCambio;
                    _cliente.Dispose();
                }
                catch { }
                _cliente = null;
            }

            // Liberar handles de los iconos generados.
            LiberarIcono(ref _iconoConectado);
            LiberarIcono(ref _iconoSincronizando);
            LiberarIcono(ref _iconoPausado);
            LiberarIcono(ref _iconoDesconectado);

            base.OnFormClosed(e);
        }

        // ====================================================================
        // TEMA — aplicar paleta y logo desde PORTAL_CONFIG
        // ====================================================================

        private void AplicarTema()
        {
            // Header & footer toman el "secondary" del tema (slate-900 por defecto).
            pnlHeader.BackColor = Tema.Secondary;
            pnlFooter.BackColor = Tema.Secondary;
            pbLogo.BackColor    = Tema.Secondary;

            // Texto del título = nombre del portal cargado del tema.
            lblTituloApp.Text = "Visor · " + Tema.NombreApp;
            this.Text         = "Visor · " + Tema.NombreApp;
            notifyIcon.Text   = "Visor · " + Tema.NombreApp;

            // Logo: si lo hay, lo mostramos y movemos el título a la derecha.
            if (Tema.Logo != null)
            {
                pbLogo.Image   = Tema.Logo;
                pbLogo.Visible = true;
                lblTituloApp.Location  = new Point(84, 18);
                lblEstadoLinea.Location = new Point(84, 48);
            }
            else
            {
                pbLogo.Visible = false;
                lblTituloApp.Location  = new Point(24, 18);
                lblEstadoLinea.Location = new Point(24, 48);
            }

            // Botón Guardar usa el primary del tema.
            btnGuardar.BackColor = Tema.Primary;
            btnGuardar.FlatAppearance.MouseOverBackColor = Tema.PrimaryHover;
        }

        // ====================================================================
        // ICONOS DINÁMICOS DEL TRAY
        // ====================================================================

        private void ConstruirIconosTray()
        {
            // 4 iconos pequeños circulares en colores semánticos.
            _iconoConectado     = CrearIconoCirculo(Color.FromArgb( 74, 222, 128));  // verde
            _iconoSincronizando = CrearIconoCirculo(Tema.Primary);                    // azul (de marca)
            _iconoPausado       = CrearIconoCirculo(Color.FromArgb(251, 191,  36));  // ámbar
            _iconoDesconectado  = CrearIconoCirculo(Color.FromArgb(248, 113, 113));  // rojo
        }

        private static Icon CrearIconoCirculo(Color color)
        {
            // 16×16 con un círculo bordeado para que se vea nítido en cualquier
            // tema de Windows (claro u oscuro de la bandeja).
            using (var bmp = new Bitmap(16, 16))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    // Sombra sutil para que destaque
                    using (var sombra = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                        g.FillEllipse(sombra, 2, 3, 13, 13);

                    using (var br = new SolidBrush(color))
                        g.FillEllipse(br, 1, 1, 14, 14);

                    using (var bordePen = new Pen(Color.FromArgb(200, Color.White), 1f))
                        g.DrawEllipse(bordePen, 1, 1, 13, 13);
                }
                var hIcon = bmp.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }

        private void LiberarIcono(ref Icon icono)
        {
            if (icono == null) return;
            try { DestroyIcon(icono.Handle); } catch { }
            try { icono.Dispose(); } catch { }
            icono = null;
        }

        private void ActualizarIconoTray(string estado)
        {
            Icon dst;
            if (!_ultimaConexion)              dst = _iconoDesconectado;
            else if (estado == EstadoServicio.EjecutandoCiclo) dst = _iconoSincronizando;
            else if (estado == EstadoServicio.Pausado)         dst = _iconoPausado;
            else                                                dst = _iconoConectado;

            if (dst != null) notifyIcon.Icon = dst;
        }

        // ====================================================================
        // TRAY / minimize / close
        // ====================================================================

        protected override void SetVisibleCore(bool value)
        {
            if (_suprimirShowInicial)
            {
                _suprimirShowInicial = false;
                ShowInTaskbar = false;
                base.SetVisibleCore(false);
                return;
            }
            base.SetVisibleCore(value);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized) { ShowInTaskbar = false; Hide(); }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_cierreReal)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                Hide();
                return;
            }
            try { notifyIcon.Visible = false; } catch { }
            base.OnFormClosing(e);
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e) { MostrarVentana(); }
        private void mniAbrir_Click(object sender, EventArgs e)         { MostrarVentana(); }

        private void mniMantenerArriba_Click(object sender, EventArgs e)
        {
            TopMost = mniMantenerArriba.Checked;
        }

        private void mniSalir_Click(object sender, EventArgs e)
        {
            _cierreReal = true;
            try { notifyIcon.Visible = false; } catch { }
            Close();
            Application.Exit();
        }

        private void MostrarVentana()
        {
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            Show();
            BringToFront();
            Activate();
        }

        // ====================================================================
        // COMANDOS HACIA EL SERVICIO (menú tray)
        // ====================================================================

        private async void mniForzarCiclo_Click(object sender, EventArgs e)
        {
            await EnviarComandoSeguroAsync(new ComandoForzarCiclo(), "Forzar ciclo");
        }

        private async void mniPausar_Click(object sender, EventArgs e)
        {
            await EnviarComandoSeguroAsync(new ComandoPausar(), "Pausar servicio");
        }

        private async void mniReanudar_Click(object sender, EventArgs e)
        {
            await EnviarComandoSeguroAsync(new ComandoReanudar(), "Reanudar servicio");
        }

        private async System.Threading.Tasks.Task EnviarComandoSeguroAsync(MensajeBase cmd, string descripcion)
        {
            if (_cliente == null || !_cliente.Conectado)
            {
                MostrarToast(descripcion, "No hay conexión con el servicio.", ToolTipIcon.Warning);
                return;
            }
            try { await _cliente.EnviarAsync(cmd); }
            catch (Exception ex) { MostrarToast(descripcion, ex.Message, ToolTipIcon.Error); }
        }

        /// <summary>
        /// Habilita o deshabilita las acciones de comando según el estado.
        /// Sin conexión: todas desactivadas. Pausado: Reanudar activo, Pausar no.
        /// Ejecutando ciclo: Forzar no tiene sentido (ya hay uno corriendo).
        /// </summary>
        private void ActualizarHabilitacionComandos(bool estadoConocido)
        {
            mniForzarCiclo.Enabled = _ultimaConexion && estadoConocido && _ultimoEstado != EstadoServicio.EjecutandoCiclo && _ultimoEstado != EstadoServicio.Pausado;
            mniPausar.Enabled      = _ultimaConexion && estadoConocido && _ultimoEstado != EstadoServicio.Pausado && _ultimoEstado != EstadoServicio.Deteniendo;
            mniReanudar.Enabled    = _ultimaConexion && _ultimoEstado == EstadoServicio.Pausado;
        }

        // ====================================================================
        // BOTONES Limpiar / Guardar
        // ====================================================================

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            var resp = MessageBox.Show(
                this,
                "¿Limpiar el log en pantalla?\n\nEsto no borra el log histórico en disco del servicio — solo limpia lo que ves aquí.",
                "Limpiar log",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (resp != DialogResult.Yes) return;
            LimpiarLog();
        }

        /// <summary>
        /// Vacía el log en pantalla y resetea el estado asociado al render
        /// (<see cref="_logVacio"/> y <see cref="_esPrimerPasoDelCiclo"/>) para
        /// que la próxima línea se pinte limpia, sin separadores ni indentación
        /// heredados de lo que había antes.
        ///
        /// Es la lógica única de "limpiar": la usa el botón Limpiar y también el
        /// auto-limpiado al iniciar un ciclo (EventoCicloIniciado).
        /// </summary>
        private void LimpiarLog()
        {
            rtbLog.Clear();
            _logVacio             = true;
            _esPrimerPasoDelCiclo = true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = "visor_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
            if (saveFileDialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                File.WriteAllText(saveFileDialog.FileName, rtbLog.Text);
                MostrarToast("Log guardado", "Archivo: " + Path.GetFileName(saveFileDialog.FileName), ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo guardar:\n\n" + ex.Message,
                    "Guardar log", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====================================================================
        // TOAST helper
        // ====================================================================

        private void MostrarToast(string titulo, string mensaje, ToolTipIcon icono)
        {
            try { notifyIcon.ShowBalloonTip(4000, titulo, mensaje, icono); }
            catch { /* sin overlay disponible (sesión RDP rara, etc.) */ }
        }

        // ====================================================================
        // CALLBACKS del ClientePipe — corren en otro thread, marshal a UI.
        // ====================================================================

        private void Cliente_ConexionCambio(bool conectado)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired) { try { BeginInvoke(new Action<bool>(Cliente_ConexionCambio), conectado); } catch { } return; }

            _ultimaConexion = conectado;

            if (conectado)
            {
                lblEstadoLinea.Text      = "Servicio: conectado · esperando snapshot…";
                lblEstadoLinea.ForeColor = ColorSuccess;
            }
            else
            {
                lblEstadoLinea.Text      = "Servicio: desconectado · reintentando…";
                lblEstadoLinea.ForeColor = ColorError;
                _ultimoEstado = "";
                progresoCiclo.Visible = false;
            }

            ActualizarIconoTray(_ultimoEstado);
            ActualizarHabilitacionComandos(conectado && !string.IsNullOrEmpty(_ultimoEstado));
        }

        private void Cliente_MensajeRecibido(MensajeBase msg)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired) { try { BeginInvoke(new Action<MensajeBase>(Cliente_MensajeRecibido), msg); } catch { } return; }
            UI_DespacharMensaje(msg);
        }

        private void UI_DespacharMensaje(MensajeBase msg)
        {
            var snap = msg as EventoSnapshot;
            if (snap != null) { UI_ActualizarHeaderSnapshot(snap); return; }

            var bit = msg as EventoBitacora;
            if (bit != null) { UI_AgregarLineaLog(bit.nivel, bit.ts, bit.mensaje, bit.fuente); return; }

            var si = msg as EventoServicioIniciado;
            if (si != null)
            {
                // El visor habla en lenguaje humano: nombre del servicio y
                // versión, sin PID (irrelevante para el operador).
                UI_AgregarLineaLog("success", si.ts,
                    "▲ Servicio iniciado · " + si.nombre_servicio + " v" + si.version_servicio, null);
                MostrarToast("Servicio iniciado", si.nombre_servicio, ToolTipIcon.Info);
                return;
            }

            var sd = msg as EventoServicioDeteniendo;
            if (sd != null)
            {
                UI_AgregarLineaLog("warning", sd.ts, "▼ Servicio deteniéndose: " + (sd.razon ?? "(sin razón)"), null);
                MostrarToast("Servicio deteniéndose", sd.razon ?? "", ToolTipIcon.Warning);
                return;
            }

            var ci = msg as EventoCicloIniciado;
            if (ci != null)
            {
                // Cada nueva vuelta del servicio arranca con el log en blanco:
                // así no se acumula basura entre ciclos. Limpiamos PRIMERO y
                // dejamos que el propio "▶ Ciclo #N iniciado" sea el primer
                // renglón de la vuelta. Estamos ya en el hilo de UI (este
                // método se invoca desde Cliente_MensajeRecibido vía BeginInvoke).
                //
                // OJO: solo aquí. NO en EventoSnapshot (estado inicial al
                // conectar) ni en cada EventoBitacora.
                LimpiarLog();

                UI_AgregarLineaLog("info", ci.ts,
                    "▶ Ciclo #" + ci.ciclo_id + " iniciado"
                    + (ci.total_pasos > 0 ? " (" + ci.total_pasos + " pasos)" : ""),
                    null);
                UI_ArrancarProgresoCiclo(ci.total_pasos);
                _ultimoEstado = EstadoServicio.EjecutandoCiclo;
                _esPrimerPasoDelCiclo = true;   // reset para que el 1er paso del nuevo ciclo NO lleve separador antes
                ActualizarIconoTray(_ultimoEstado);
                ActualizarHabilitacionComandos(estadoConocido: true);
                return;
            }

            var ct = msg as EventoCicloTerminado;
            if (ct != null)
            {
                var nivel   = ct.ok ? "success" : "error";
                var simbolo = ct.ok ? "■" : "✗";
                UI_AgregarLineaLog(nivel, ct.ts,
                    simbolo + " Ciclo #" + ct.ciclo_id + " terminó en " + ct.duracion_seg.ToString("F1") + "s " +
                    "(pasos: " + ct.pasos_ok + " ok, " + ct.pasos_falla + " falló)", null);

                UI_OcultarProgresoCiclo();
                _ultimoEstado = EstadoServicio.Ocioso;
                ActualizarIconoTray(_ultimoEstado);
                ActualizarHabilitacionComandos(estadoConocido: true);

                if (!ct.ok)
                    MostrarToast("Ciclo falló", "Ciclo #" + ct.ciclo_id + ": " + ct.pasos_falla + " paso(s) con error.", ToolTipIcon.Error);
                return;
            }

            var pi = msg as EventoPasoIniciado;
            if (pi != null)
            {
                // Separador visual entre pasos: línea en blanco antes del header
                // del paso (excepto el primero del ciclo, que ya va pegado al
                // "▶ Ciclo #N iniciado").
                if (!_esPrimerPasoDelCiclo) UI_InsertarSeparadorEntrePasos();
                _esPrimerPasoDelCiclo = false;

                var totalSuffix = pi.items_total.HasValue ? " (" + pi.items_total.Value + " ítems)" : "";
                UI_AgregarLineaLog("info", pi.ts, "  → Paso: " + pi.paso + totalSuffix, null);
                return;
            }

            var pt = msg as EventoPasoTerminado;
            if (pt != null)
            {
                var nivel = pt.ok ? "success" : "error";
                var simbolo = pt.ok ? "✓" : "✗";
                var resumen = string.IsNullOrEmpty(pt.mensaje_resumen) ? (pt.ok ? "OK" : "FAIL") : pt.mensaje_resumen;
                UI_AgregarLineaLog(nivel, pt.ts, "  " + simbolo + " Paso " + pt.paso + ": " + resumen, null);

                UI_AvanzarProgresoCiclo();

                if (!pt.ok) MostrarToast("Paso falló", pt.paso + " — " + resumen, ToolTipIcon.Error);
                return;
            }

            var pr = msg as EventoProgreso;
            if (pr != null)
            {
                var progreso = pr.items_total.HasValue ? pr.items_completados + "/" + pr.items_total.Value : pr.items_completados.ToString();
                UI_AgregarLineaLog("info", pr.ts, "  · " + pr.paso + " " + progreso +
                    (string.IsNullOrEmpty(pr.mensaje) ? "" : " — " + pr.mensaje), null);
                return;
            }
        }

        /// <summary>
        /// Inserta un separador visual entre dos pasos del ciclo: una línea
        /// horizontal en gris azulado tenue precedida y seguida por respiros.
        /// Resetea el hanging indent para que el separador quede pegado al
        /// margen izquierdo (no sangrado como continuación de la línea previa).
        /// </summary>
        private void UI_InsertarSeparadorEntrePasos()
        {
            if (_logVacio) return; // el log está vacío — no hay nada a qué separar

            rtbLog.SelectionStart         = rtbLog.TextLength;
            rtbLog.SelectionLength        = 0;
            rtbLog.SelectionHangingIndent = 0;

            // Pequeño respiro arriba para destacar la separación.
            rtbLog.AppendText(Environment.NewLine);

            // Línea horizontal: 90 caracteres "─" (U+2500). En un RichTextBox
            // con fuente monoespaciada cubre cómodamente el ancho típico del
            // panel sin saltar de línea (si la ventana es chica, WordWrap lo
            // corta — no rompe nada, solo se envuelve).
            rtbLog.SelectionStart  = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor  = ColorSeparador;
            rtbLog.AppendText(new string('─', 90) + Environment.NewLine);

            if (chkAutoScroll.Checked)
            {
                rtbLog.SelectionStart = rtbLog.TextLength;
                rtbLog.ScrollToCaret();
            }
        }

        private void UI_AgregarLineaLog(string nivel, DateTime ts, string mensaje, string fuente)
        {
            if (_logVacio) { rtbLog.Clear(); _logVacio = false; }

            var timestamp = ts.ToString("HH:mm:ss");
            var nivelStr  = (nivel ?? "info").ToUpperInvariant().PadRight(7);
            var color     = ColorParaNivel(nivel);

            rtbLog.SelectionStart  = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;

            // Hanging indent: cuando la línea se envuelve por WordWrap, la
            // continuación se sangra para alinearse con la columna del mensaje
            // (~145px = timestamp 65px + nivel 80px). Se ve mucho mejor que
            // continuar pegado al borde izquierdo.
            rtbLog.SelectionHangingIndent = 145;

            rtbLog.SelectionColor = ColorTimestamp;
            rtbLog.AppendText(timestamp + "  ");

            rtbLog.SelectionColor = color;
            rtbLog.AppendText(nivelStr + "  ");

            if (!string.IsNullOrEmpty(fuente))
            {
                rtbLog.SelectionColor = ColorFuente;
                rtbLog.AppendText("[" + fuente + "] ");
            }

            rtbLog.SelectionColor = ColorMensaje;
            rtbLog.AppendText((mensaje ?? "") + Environment.NewLine);

            // Cap: si supera 200KB, recorta los primeros 50KB.
            if (rtbLog.TextLength > 200000)
            {
                rtbLog.Select(0, 50000);
                rtbLog.SelectedText = "";
            }

            if (chkAutoScroll.Checked)
            {
                rtbLog.SelectionStart = rtbLog.TextLength;
                rtbLog.ScrollToCaret();
            }
        }

        private void UI_ActualizarHeaderSnapshot(EventoSnapshot snap)
        {
            _ultimoEstado = snap.estado ?? "";

            var partes = new List<string>();
            partes.Add("Servicio: " + DescribirEstado(snap.estado));

            if (snap.ciclo_actual_id.HasValue)
            {
                var enPaso = string.IsNullOrEmpty(snap.paso_actual) ? "" : " · paso " + snap.paso_actual;
                partes.Add("Ciclo #" + snap.ciclo_actual_id.Value + enPaso);
            }
            else if (snap.ultimo_ciclo_terminado.HasValue)
            {
                var marca = (snap.ultimo_ciclo_ok == true) ? " ✓" : " ✗";
                partes.Add("Último ciclo: " + snap.ultimo_ciclo_terminado.Value.ToString("HH:mm:ss") + marca);
            }

            if (snap.timer_segundos > 0)
                partes.Add("Cada " + FormatosHumanos.DuracionCorta(snap.timer_segundos));

            lblEstadoLinea.Text      = string.Join("  ·  ", partes.ToArray());
            lblEstadoLinea.ForeColor = ColorParaEstado(snap.estado);

            UI_AplicarProgresoDesdeSnapshot(snap);
            ActualizarIconoTray(_ultimoEstado);
            ActualizarHabilitacionComandos(estadoConocido: true);
        }

        // ====================================================================
        // PROGRESS BAR — paso del Marquee a Continuous y de vuelta
        // ====================================================================

        private void UI_ArrancarProgresoCiclo(int totalPasos)
        {
            if (totalPasos > 0)
            {
                progresoCiclo.Style   = ProgressBarStyle.Continuous;
                progresoCiclo.Minimum = 0;
                progresoCiclo.Maximum = totalPasos;
                progresoCiclo.Value   = 0;
            }
            else
            {
                // Servicio viejo o sin info — fallback al Marquee.
                progresoCiclo.Style = ProgressBarStyle.Marquee;
            }
            progresoCiclo.Visible = true;
        }

        private void UI_AvanzarProgresoCiclo()
        {
            if (progresoCiclo.Style != ProgressBarStyle.Continuous) return;
            if (progresoCiclo.Value < progresoCiclo.Maximum)
                progresoCiclo.Value = progresoCiclo.Value + 1;
        }

        private void UI_OcultarProgresoCiclo()
        {
            progresoCiclo.Visible = false;
            progresoCiclo.Value   = 0;
        }

        /// <summary>
        /// Si el Visor se reconectó a mitad de un ciclo, restaura la posición
        /// de la barra desde lo que reportó el snapshot. Sin esto, la barra
        /// arrancaría en 0 hasta el siguiente EVT_PASO_TERMINADO y desorientaría
        /// al operador.
        /// </summary>
        private void UI_AplicarProgresoDesdeSnapshot(EventoSnapshot snap)
        {
            if (snap.estado != EstadoServicio.EjecutandoCiclo)
            {
                UI_OcultarProgresoCiclo();
                return;
            }

            if (snap.ciclo_pasos_total.HasValue && snap.ciclo_pasos_total.Value > 0)
            {
                progresoCiclo.Style   = ProgressBarStyle.Continuous;
                progresoCiclo.Minimum = 0;
                progresoCiclo.Maximum = snap.ciclo_pasos_total.Value;
                var pos = snap.ciclo_pasos_completados ?? 0;
                progresoCiclo.Value   = Math.Max(0, Math.Min(progresoCiclo.Maximum, pos));
            }
            else
            {
                progresoCiclo.Style = ProgressBarStyle.Marquee;
            }
            progresoCiclo.Visible = true;
        }

        // ====================================================================
        // Helpers de presentación
        // ====================================================================

        private static Color ColorParaNivel(string nivel)
        {
            if (string.Equals(nivel, NivelLog.Warning, StringComparison.Ordinal)) return ColorWarning;
            if (string.Equals(nivel, NivelLog.Error,   StringComparison.Ordinal)) return ColorError;
            if (string.Equals(nivel, NivelLog.Success, StringComparison.Ordinal)) return ColorSuccess;
            return ColorInfo;
        }

        private static Color ColorParaEstado(string estado)
        {
            if (string.Equals(estado, EstadoServicio.Ocioso,           StringComparison.Ordinal) ||
                string.Equals(estado, EstadoServicio.EjecutandoCiclo,  StringComparison.Ordinal))
                return ColorSuccess;
            if (string.Equals(estado, EstadoServicio.Pausado,    StringComparison.Ordinal)) return ColorWarning;
            if (string.Equals(estado, EstadoServicio.Deteniendo, StringComparison.Ordinal)) return ColorError;
            return ColorFuente;
        }

        private static string DescribirEstado(string estado)
        {
            if (string.Equals(estado, EstadoServicio.Iniciando,       StringComparison.Ordinal)) return "Iniciando";
            if (string.Equals(estado, EstadoServicio.Ocioso,          StringComparison.Ordinal)) return "Activo";
            if (string.Equals(estado, EstadoServicio.EjecutandoCiclo, StringComparison.Ordinal)) return "Sincronizando";
            if (string.Equals(estado, EstadoServicio.Pausado,         StringComparison.Ordinal)) return "Pausado";
            if (string.Equals(estado, EstadoServicio.Deteniendo,      StringComparison.Ordinal)) return "Deteniéndose";
            return estado ?? "—";
        }
    }
}
