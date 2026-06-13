using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace PortalProveedoresCore.Pipes
{
    /// <summary>
    /// Lado servidor del Named Pipe en el proceso del Servicio Windows.
    /// Acepta una conexión del Visor a la vez, drena los eventos del
    /// <see cref="CanalEventos"/> hacia el stream, y lee comandos del Visor
    /// para despacharlos al handler que le pase el llamador.
    ///
    /// Si la conexión se rompe (Visor cerrado o crash), el bucle externo
    /// vuelve a abrir el pipe y espera otra conexión — sin reiniciar el
    /// servicio. El servicio sigue publicando eventos al canal aunque nadie
    /// esté conectado; el canal aplica drop-oldest para no crecer sin límite.
    ///
    /// ACL del pipe (por defecto):
    ///   - LocalSystem            : FullControl
    ///   - Usuarios interactivos  : ReadWrite + Synchronize
    ///   - Administradores        : FullControl
    /// Cualquier visor que corra como usuario logueado o como admin puede
    /// conectar; servicios de otros usuarios no.
    /// </summary>
    public sealed class ServidorPipe : IDisposable
    {
        private readonly CanalEventos _canal;
        private readonly Func<MensajeBase, Task> _onComando;
        private readonly CancellationTokenSource _ctsServidor = new CancellationTokenSource();
        private Task _tareaAceptacion;

        /// <param name="canal">Cola de eventos a drenar hacia el visor conectado.</param>
        /// <param name="onComando">
        /// Handler para los comandos que el visor envíe (CMD_PING, CMD_PAUSAR, etc.).
        /// Se invoca en el thread del lector del pipe; no bloquees mucho aquí.
        /// </param>
        public ServidorPipe(CanalEventos canal, Func<MensajeBase, Task> onComando)
        {
            if (canal == null)     throw new ArgumentNullException("canal");
            if (onComando == null) throw new ArgumentNullException("onComando");
            _canal     = canal;
            _onComando = onComando;
        }

        /// <summary>Arranca el bucle de aceptación de conexiones en background.</summary>
        public void Iniciar()
        {
            if (_tareaAceptacion != null) return;
            _tareaAceptacion = Task.Run(() => LoopAceptarConexionesAsync(_ctsServidor.Token));
        }

        /// <summary>
        /// Detiene el servidor. Cierra el canal (los productores ya no se
        /// pueden publicar), cancela la aceptación de conexiones, y espera
        /// hasta 5 segundos a que las tareas terminen.
        /// </summary>
        public void Detener()
        {
            _canal.CerrarParaEscritura();
            _ctsServidor.Cancel();
            try { if (_tareaAceptacion != null) _tareaAceptacion.Wait(TimeSpan.FromSeconds(5)); }
            catch { }
        }

        public void Dispose()
        {
            Detener();
            try { _ctsServidor.Dispose(); } catch { }
        }

        // ====================================================================
        // Loop de aceptación
        // ====================================================================

        private async Task LoopAceptarConexionesAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                NamedPipeServerStream server = null;
                try
                {
                    server = CrearPipeServer();
                    await server.WaitForConnectionAsync(ct).ConfigureAwait(false);
                    await AtenderConexionAsync(server, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break; // detención normal
                }
                catch (Exception)
                {
                    // Esperamos 2 segundos antes de reintentar para no spammear
                    // si hay un error sistémico (permisos, registro, etc.).
                    try { await Task.Delay(TimeSpan.FromSeconds(2), ct).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }
                }
                finally
                {
                    if (server != null)
                    {
                        try { server.Dispose(); } catch { }
                    }
                }
            }
        }

        private NamedPipeServerStream CrearPipeServer()
        {
            var seguridad = ConstruirACL();

            return new NamedPipeServerStream(
                pipeName: ConstantesPipe.NombrePipe,
                direction: PipeDirection.InOut,
                maxNumberOfServerInstances: 1, // solo un visor a la vez
                transmissionMode: PipeTransmissionMode.Byte,
                options: PipeOptions.Asynchronous,
                inBufferSize: 4096,
                outBufferSize: 16384,
                pipeSecurity: seguridad);
        }

        /// <summary>
        /// ACL del pipe. Sin esto, .NET asignaría un ACL por defecto que
        /// excluye al usuario interactivo cuando el servicio corre como
        /// LocalSystem — y entonces el Visor no podría conectar.
        /// </summary>
        private static PipeSecurity ConstruirACL()
        {
            var sec = new PipeSecurity();

            sec.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Allow));

            sec.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.InteractiveSid, null),
                PipeAccessRights.ReadWrite | PipeAccessRights.Synchronize,
                AccessControlType.Allow));

            sec.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Allow));

            return sec;
        }

        // ====================================================================
        // Atención de una conexión activa
        // ====================================================================

        /// <summary>
        /// Lanza dos tareas en paralelo: una drena el canal hacia el writer del
        /// pipe (eventos hacia el Visor), otra lee del reader del pipe
        /// (comandos del Visor). Si cualquiera termina (pipe roto), cancela
        /// la otra y regresa para que el loop externo acepte otra conexión.
        /// </summary>
        private async Task AtenderConexionAsync(NamedPipeServerStream server, CancellationToken ctServidor)
        {
            using (var ctsConexion = CancellationTokenSource.CreateLinkedTokenSource(ctServidor))
            using (var reader = new StreamReader(server))
            using (var writer = new StreamWriter(server) { AutoFlush = true })
            {
                var tWrite = LoopEscribirEventosAsync(writer, ctsConexion.Token);
                var tRead  = LoopLeerComandosAsync(reader, ctsConexion.Token);

                await Task.WhenAny(tWrite, tRead).ConfigureAwait(false);
                ctsConexion.Cancel();

                // Damos un instante para que la otra tarea termine limpiamente.
                try { await Task.WhenAll(tWrite, tRead).ConfigureAwait(false); }
                catch { /* esperado: pipe roto o cancelado */ }
            }
        }

        private async Task LoopEscribirEventosAsync(StreamWriter writer, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var msg = await _canal.EsperarSiguienteAsync(ct).ConfigureAwait(false);
                if (msg == null) return; // canal cerrado

                var linea = SerializadorMensajes.Serializar(msg);

                try
                {
                    await writer.WriteLineAsync(linea).ConfigureAwait(false);
                }
                catch (IOException)
                {
                    return; // pipe roto
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }

        private async Task LoopLeerComandosAsync(StreamReader reader, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                string linea;
                try
                {
                    linea = await reader.ReadLineAsync().ConfigureAwait(false);
                }
                catch (IOException)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                if (linea == null) return; // pipe cerrado

                var msg = SerializadorMensajes.Deserializar(linea);
                if (msg == null) continue; // tipo desconocido — ignorar

                try { await _onComando(msg).ConfigureAwait(false); }
                catch { /* fallos del handler no rompen el lector */ }
            }
        }
    }
}
