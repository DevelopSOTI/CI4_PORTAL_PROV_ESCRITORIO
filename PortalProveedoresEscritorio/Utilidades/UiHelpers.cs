using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PortalProveedoresEscritorio.Utilidades
{
    /// <summary>
    /// Helpers de UI reusables por todos los forms de la app. Centralizar
    /// aquí evita duplicación entre <c>FormLogin</c>, <c>FormSelectorEmpresa</c>
    /// y <c>FormPrincipal</c>, y mantiene los <c>.Designer.cs</c> limpios
    /// (sin lambdas ni operaciones complejas que rompen el diseñador de VS).
    /// </summary>
    public static class UiHelpers
    {
        // ---- P/Invoke para drag nativo de ventanas sin borde ----
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION       = 0x2;

        /// <summary>
        /// Engancha un control (típicamente una title bar custom) para que al
        /// arrastrarlo Windows mueva el form padre como si fuera la caption
        /// nativa. No mantiene estado entre eventos — el SO se encarga.
        /// </summary>
        public static void EngancharDragNativo(Control dragHandle, Form formAMover)
        {
            if (dragHandle == null || formAMover == null) return;
            dragHandle.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                ReleaseCapture();
                SendMessage(formAMover.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            };
        }

        /// <summary>
        /// Aplica una región redondeada al control (usado para forms sin
        /// borde y para inputs/botones tipo "pill"). Llámalo en el Load
        /// del form y en Resize para que se mantenga al redimensionar.
        /// </summary>
        public static void AplicarEsquinasRedondeadas(Control c, int radio)
        {
            if (c == null || c.Width <= 0 || c.Height <= 0) return;
            using (var path = CrearPathRedondeado(c.ClientRectangle, radio))
                c.Region = new Region(path);
        }

        /// <summary>
        /// Genera un GraphicsPath con esquinas redondeadas para un rectángulo.
        /// El llamador es responsable de hacerle <c>Dispose</c>.
        /// </summary>
        public static GraphicsPath CrearPathRedondeado(Rectangle rect, int radio)
        {
            int d = radio * 2;
            var path = new GraphicsPath();
            if (d <= 0) { path.AddRectangle(rect); return path; }
            path.AddArc(rect.X,         rect.Y,         d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y,         d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d,d, d,   0, 90);
            path.AddArc(rect.X,         rect.Bottom - d,d, d,  90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Aplica el efecto "pill" (esquinas redondeadas) a un botón.
        /// Conéctalo al evento Paint del botón. Se usa una región — el
        /// hover/click siguen funcionando porque la región solo recorta
        /// visualmente, no captura.
        /// </summary>
        public static void DibujarBordePill(Button b, int radio)
        {
            if (b == null || b.Width <= 0 || b.Height <= 0) return;
            using (var path = CrearPathRedondeado(b.ClientRectangle, radio))
                b.Region = new Region(path);
        }

        // ---- Hovers para los botones "cerrar" y "minimizar" de title bars custom ----

        /// <summary>
        /// Configura un Label que actúa como botón "cerrar" (X). Al hover,
        /// fondo rojo Windows + texto blanco. Al click invoca <paramref name="onClose"/>.
        /// </summary>
        public static void ConfigurarBotonCerrar(Label btn, Color colorNormalText, Action onClose)
        {
            if (btn == null) return;
            Color rojoWindows = Color.FromArgb(232, 17, 35);
            btn.Cursor    = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.ForeColor = colorNormalText;
            btn.BackColor = Color.Transparent;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = rojoWindows;
                btn.ForeColor = Color.White;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = colorNormalText;
            };
            if (onClose != null)
                btn.Click += (s, e) => onClose();
        }

        /// <summary>
        /// Configura un Label que actúa como botón "minimizar" (─). Al hover,
        /// fondo gris translúcido + texto blanco.
        /// </summary>
        public static void ConfigurarBotonMinimizar(Label btn, Color colorNormalText, Form formAMinimizar)
        {
            if (btn == null) return;
            btn.Cursor    = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.ForeColor = colorNormalText;
            btn.BackColor = Color.Transparent;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(40, 255, 255, 255);
                btn.ForeColor = Color.White;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = colorNormalText;
            };
            if (formAMinimizar != null)
                btn.Click += (s, e) => { formAMinimizar.WindowState = FormWindowState.Minimized; };
        }

        /// <summary>
        /// Configura un Label que actúa como botón "maximizar/restaurar" (□ / ❐).
        /// Al hover, fondo gris translúcido + texto blanco. Al click invoca
        /// <paramref name="onToggle"/>, que el form usa para alternar
        /// <c>WindowState</c> y manejar la región redondeada.
        /// </summary>
        public static void ConfigurarBotonMaximizar(Label btn, Color colorNormalText, Action onToggle)
        {
            if (btn == null) return;
            btn.Cursor    = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.ForeColor = colorNormalText;
            btn.BackColor = Color.Transparent;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(40, 255, 255, 255);
                btn.ForeColor = Color.White;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = colorNormalText;
            };
            if (onToggle != null)
                btn.Click += (s, e) => onToggle();
        }
    }
}
