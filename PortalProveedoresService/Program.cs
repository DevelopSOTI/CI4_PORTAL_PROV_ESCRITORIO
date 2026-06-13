using System;
using System.ServiceProcess;

namespace PortalProveedoresService
{
    /// <summary>
    /// Punto de entrada doble:
    ///  - Bajo el Service Control Manager (servicio instalado): ServiceBase.Run.
    ///  - En consola interactiva (F5 desde Visual Studio): arranca el ciclo
    ///    directo para poder poner breakpoints y ver salida en tiempo real.
    ///
    /// `Environment.UserInteractive` es true cuando NO corre como servicio.
    /// </summary>
    internal static class Program
    {
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                EjecutarComoConsola();
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { new WSPortal() });
            }
        }

        private static void EjecutarComoConsola()
        {
            Console.Title = "PortalProveedoresService (modo consola — debug)";
            Console.WriteLine("======================================================");
            Console.WriteLine("  PortalProveedoresService — MODO CONSOLA (debug)");
            Console.WriteLine("======================================================");
            Console.WriteLine();
            Console.WriteLine("Requisitos antes de continuar:");
            Console.WriteLine("  - HKLM\\SOFTWARE\\SOTI\\Service Portal con:");
            Console.WriteLine("       PORTAL_BASE_URL, PORTAL_API_KEY,");
            Console.WriteLine("       MICRO_SERV, MICRO_ROOT, MICRO_USER, MICRO_PASS,");
            Console.WriteLine("       MODE_TIMER (segundos entre ciclos).");
            Console.WriteLine("  - Portal CI4 corriendo con portal.apiKey en .env.");
            Console.WriteLine();
            Console.WriteLine("Para correr COMO servicio Windows real (no debug), instala");
            Console.WriteLine("con sc.exe / InstallUtil y arranca desde services.msc.");
            Console.WriteLine();

            var svc = new WSPortal();
            svc.IniciarManual();
            Console.WriteLine("[OK] Ciclo arrancado. Logs en EventLog Windows y en .\\EventLog\\.");
            Console.WriteLine();
            Console.WriteLine("Presiona ENTER para detener.");
            Console.WriteLine("------------------------------------------------------");
            Console.ReadLine();

            Console.WriteLine("Deteniendo...");
            svc.DetenerManual();
            Console.WriteLine("[OK] Detenido. Bye.");
        }
    }
}
