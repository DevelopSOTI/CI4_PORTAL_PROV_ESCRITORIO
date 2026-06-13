using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PortalProveedoresCore.Pipes
{
    /// <summary>
    /// Cola productor-consumidor asíncrona, thread-safe. Múltiples productores
    /// (el worker del servicio, los hooks de logging, etc.) llaman
    /// <see cref="Publicar"/> sin bloquear; UN consumidor (el escritor del
    /// pipe) consume con <see cref="EsperarSiguienteAsync"/>.
    ///
    /// Bounded con política <b>drop-oldest</b>: si nadie consume durante mucho
    /// rato (Visor desconectado, por ejemplo), la cola descarta los mensajes
    /// más viejos en lugar de crecer sin límite. Eso evita memory leak en
    /// instalaciones donde el operador nunca abre el Visor.
    ///
    /// Implementación: <see cref="ConcurrentQueue{T}"/> + <see cref="SemaphoreSlim"/>.
    /// La razón de NO usar <c>BlockingCollection</c> es que ésta combinación
    /// ofrece <c>WaitAsync</c> nativo sin tener que tirar un thread del pool
    /// por consumidor (BlockingCollection.Take es bloqueante).
    /// </summary>
    public sealed class CanalEventos : IDisposable
    {
        private readonly ConcurrentQueue<MensajeBase> _cola = new ConcurrentQueue<MensajeBase>();
        private readonly SemaphoreSlim _disponibles = new SemaphoreSlim(0);
        private readonly int _maxCapacidad;
        private volatile bool _cerrado;

        /// <param name="maxCapacidad">Tamaño máximo de la cola. Cuando se llena,
        /// los mensajes más viejos se descartan al publicar uno nuevo. 5000
        /// alcanza para horas de actividad densa.</param>
        public CanalEventos(int maxCapacidad = 5000)
        {
            if (maxCapacidad <= 0) throw new ArgumentOutOfRangeException("maxCapacidad");
            _maxCapacidad = maxCapacidad;
        }

        /// <summary>
        /// Encola un mensaje. No bloquea. Si la cola está cerrada o el mensaje
        /// es null, no hace nada. Si está llena, descarta uno viejo para hacer
        /// espacio (drop-oldest).
        /// </summary>
        public void Publicar(MensajeBase mensaje)
        {
            if (_cerrado || mensaje == null) return;

            // Drop-oldest: dejar espacio si está lleno.
            while (_cola.Count >= _maxCapacidad)
            {
                MensajeBase descartado;
                if (!_cola.TryDequeue(out descartado)) break;
                // No tocamos el semáforo aquí. Si había wake-ups pendientes,
                // EsperarSiguienteAsync los maneja con un loop tolerante a
                // wake-ups espurios (cuando despierta pero la cola está vacía).
            }

            _cola.Enqueue(mensaje);
            _disponibles.Release();
        }

        /// <summary>
        /// Marca la cola como cerrada. Las futuras llamadas a Publicar son
        /// no-op. El consumidor desbloqueado leerá lo que queda y luego null.
        /// </summary>
        public void CerrarParaEscritura()
        {
            _cerrado = true;
            _disponibles.Release(); // despierta al consumidor para que vea _cerrado
        }

        /// <summary>
        /// Espera el siguiente mensaje. Devuelve <c>null</c> cuando la cola se
        /// cerró y no queda más por consumir, o cuando el token se cancela.
        /// </summary>
        public async Task<MensajeBase> EsperarSiguienteAsync(CancellationToken ct)
        {
            while (true)
            {
                try
                {
                    await _disponibles.WaitAsync(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }

                MensajeBase msg;
                if (_cola.TryDequeue(out msg))
                    return msg;

                // Wake-up espurio: la cola estaba vacía. Si además ya cerramos,
                // terminamos. Si no, vuelve a esperar.
                if (_cerrado) return null;
            }
        }

        public void Dispose()
        {
            _cerrado = true;
            try { _disponibles.Dispose(); } catch { }
        }
    }
}
