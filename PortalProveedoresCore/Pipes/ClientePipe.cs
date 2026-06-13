using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PortalProveedoresCore.Pipes
{
    /// <summary>
    /// Lado cliente del Named Pipe en el proceso del Visor. Mantiene una conexión
    /// persistente con el Servicio: si se rompe (Servicio cae, restart, etc.),
    /// el bucle interno espera 2 segundos y vuelve a intentar — el Visor no
    /// tiene que hacer nada para reconectarse.
    ///
    /// Uso:
    /// <code>
    /// var cli = new ClientePipe();
    /// cli.MensajeRecibido += msg => { ... };
    /// cli.ConexionCambio  += conectado => { ... };
    /// cli.Iniciar();
    /// ...
    /// await cli.EnviarAsync(new ComandoForzarCiclo());
    /// ...
    /// cli.Dispose();
    /// </code>
    ///
    /// Threading:
    ///   - <see cref="MensajeRecibido"/> y <see cref="ConexionCambio"/> se
    ///     disparan en el thread del lector del pipe; el suscriptor (UI WinForms)
    ///     es responsable de hacer Marshal a su hilo si va a tocar controles.
    ///   - <see cref="EnviarAsync"/> es thread-safe y serializa internamente
    ///     las escrituras con un semáforo.
    /// </summary>
    public sealed class ClientePipe : IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly SemaphoreSlim _semaforoEscribir = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _timeoutConexion = TimeSpan.FromSeconds(2);
        private readonly TimeSpan _backoffReconexion = TimeSpan.FromSeconds(2);

        private Task _loopConexion;
        private volatile StreamWriter _writer; // null cuando no hay conexión
        private volatile bool _conectado;

        /// <summary>Se dispara cada vez que llega un mensaje del Servicio.</summary>
        public event Action<MensajeBase> MensajeRecibido;

        /// <summary>
        /// True/false según el estado de la conexión. Se dispara cuando cambia
        /// (no en cada intento). Útil para refrescar el indicador de la UI.
        /// </summary>
        public event Action<bool> ConexionCambio;

        public bool Conectado { get { return _conectado; } }

        /// <summary>Arranca el bucle de conexión en background. No bloquea.</summary>
        public void Iniciar()
        {
            if (_loopConexion != null) return;
            _loopConexion = Task.Run(() => LoopAsync(_cts.Token));
        }

        /// <summary>
        /// Encola un comando hacia el Servicio. Si no hay conexión activa,
        /// lanza <see cref="InvalidOperationException"/> — el llamador decide
        /// si lo reporta al usuario o lo retiene para cuando reconecte.
        /// </summary>
        public async Task EnviarAsync(MensajeBase mensaje)
        {
            if (mensaje == null) throw new ArgumentNullException("mensaje");
            var w = _writer;
            if (w == null) throw new InvalidOperationException("Pipe no conectado.");

            var linea = SerializadorMensajes.Serializar(mensaje);

            await _semaforoEscribir.WaitAsync().ConfigureAwait(false);
            try
            {
                await w.WriteLineAsync(linea).ConfigureAwait(false);
            }
            finally
            {
                _semaforoEscribir.Release();
            }
        }

        public void Dispose()
        {
            try { _cts.Cancel(); } catch { }
            try { if (_loopConexion != null) _loopConexion.Wait(TimeSpan.FromSeconds(3)); } catch { }
            try { _cts.Dispose(); } catch { }
            try { _semaforoEscribir.Dispose(); } catch { }
        }

        // ====================================================================
        // Loop de conexión + atención
        // ====================================================================

        private async Task LoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await ConectarYServirAsync(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // cualquier error: backoff y reintentar
                }

                NotificarCambioConexion(false);

                try { await Task.Delay(_backoffReconexion, ct).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task ConectarYServirAsync(CancellationToken ct)
        {
            using (var pipe = new NamedPipeClientStream(
                serverName: ".",
                pipeName: ConstantesPipe.NombrePipe,
                direction: PipeDirection.InOut,
                options: PipeOptions.Asynchronous))
            {
                // ConnectAsync admite int milisegundos en .NET Framework. Si el
                // servicio aún no abrió el pipe (en arranque), expira y el loop
                // externo nos manda de vuelta tras el backoff.
                await pipe.ConnectAsync((int) _timeoutConexion.TotalMilliseconds, ct).ConfigureAwait(false);

                NotificarCambioConexion(true);

                using (var reader = new StreamReader(pipe))
                using (var writer = new StreamWriter(pipe) { AutoFlush = true })
                {
                    _writer = writer;
                    try
                    {
                        // Al conectar, pedimos al servicio que nos mande snapshot
                        // del estado actual — para no tener que esperar al
                        // siguiente ciclo para mostrar algo en el header.
                        try { await EnviarAsync(new ComandoSolicitarSnapshot()).ConfigureAwait(false); }
                        catch { /* la conexión recién subió; un fallo aquí es benigno */ }

                        await LoopLeerAsync(reader, ct).ConfigureAwait(false);
                    }
                    finally
                    {
                        _writer = null;
                    }
                }
            }
        }

        private async Task LoopLeerAsync(StreamReader reader, CancellationToken ct)
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
                    return; // pipe roto
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                if (linea == null) return; // EOF — pipe cerrado del otro lado

                var msg = SerializadorMensajes.Deserializar(linea);
                if (msg == null) continue; // tipo desconocido — ignorar

                var h = MensajeRecibido;
                if (h != null)
                {
                    try { h(msg); }
                    catch { /* los problemas del suscriptor no rompen el lector */ }
                }
            }
        }

        private void NotificarCambioConexion(bool conectado)
        {
            if (_conectado == conectado) return;
            _conectado = conectado;
            var h = ConexionCambio;
            if (h != null) { try { h(conectado); } catch { } }
        }
    }
}
