using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Modelos;
using PortalProveedoresCore.Servicios;

namespace PortalProveedoresService.Sincronizacion
{
    /// <summary>
    /// Override de sincronización de un solo uso (tabla SINC_OVERRIDE del
    /// portal), compartido por los 6 sincronizadores per-empresa.
    ///
    /// Prioridad del "desde" con override:
    ///   1) override (un solo uso) — gana sobre todo lo demás, PERO nunca puede
    ///      ser posterior al MAX del módulo: las filas entre el MAX y esa fecha
    ///      se perderían para siempre (el MAX saltaría por encima). Si lo es,
    ///      se ignora y se consume igual, con aviso.
    ///   2) lo de siempre: checkpoint (MAX) → sinc_desde → fallback del módulo.
    ///
    /// El override se usa tal cual, sin el +1s del checkpoint: la intención del
    /// operador es "desde exactamente aquí".
    ///
    /// Auto-consumo: el sincronizador llama <see cref="ConsumirAsync"/> SOLO
    /// cuando el portal confirmó el envío (o no había nada que enviar). Si el
    /// ciclo truena antes, el override sobrevive y se reintenta.
    /// </summary>
    internal sealed class OverrideSincronizacion
    {
        private readonly EmpresaConfig _emp;
        private readonly string _modulo;
        private readonly string _etiqueta;
        private readonly string _nombreHumano;

        /// <summary>Hay fila de override para este módulo (usada o ignorada): hay que consumirla.</summary>
        public bool Pendiente { get; private set; }

        /// <summary>El override se aplicó como fecha "desde" de esta vuelta.</summary>
        public bool Aplicado { get; private set; }

        /// <summary>
        /// Resuelve el override de <paramref name="modulo"/> para la empresa.
        /// <paramref name="etiqueta"/> y <paramref name="nombreHumano"/> son
        /// solo para los mensajes del Visor (por nombre, nunca por ID).
        /// </summary>
        public OverrideSincronizacion(EmpresaConfig emp, string modulo, string etiqueta, string nombreHumano)
        {
            _emp          = emp;
            _modulo       = modulo;
            _etiqueta     = etiqueta;
            _nombreHumano = nombreHumano;
        }

        /// <summary>
        /// Devuelve la fecha "desde" final: el override si aplica, o
        /// <paramref name="desdeNormal"/> (lo que el sincronizador calculó con
        /// checkpoint/sinc_desde) si no hay override o se ignoró.
        /// </summary>
        public DateTime? ResolverDesde(DateTime? desdeNormal)
        {
            var raw = _emp.overrides != null ? _emp.overrides.De(_modulo) : null;
            if (string.IsNullOrWhiteSpace(raw)) return desdeNormal;

            Pendiente = true;

            DateTime fecha;
            if (!DateTime.TryParseExact(raw, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                EventoLog.Warning(_etiqueta + " · " + _nombreHumano + ": re-sincronización con fecha ilegible ("
                    + raw + "); se descarta y se sigue normal.");
                return desdeNormal;
            }

            // Tope: el MAX crudo del portal (sin +1s). Sin MAX el módulo aún no
            // tiene datos en el portal y no hay nada que perder.
            DateTime maximo;
            var rawMax = _emp.checkpoints != null ? _emp.checkpoints.De(_modulo) : null;
            if (!string.IsNullOrWhiteSpace(rawMax) && DateTime.TryParse(rawMax, out maximo) && fecha > maximo)
            {
                EventoLog.Warning(_etiqueta + " · " + _nombreHumano + ": la re-sincronización pedida desde "
                    + fecha.ToString("dd/MM/yyyy HH:mm") + " es posterior al último dato del portal ("
                    + maximo.ToString("dd/MM/yyyy HH:mm") + "); se ignora para no dejar huecos y se sigue normal.");
                return desdeNormal;
            }

            Aplicado = true;
            EventoLog.Info(_etiqueta + " · " + _nombreHumano + ": re-sincronización solicitada desde "
                + fecha.ToString("dd/MM/yyyy HH:mm") + " (un solo uso).");
            return fecha;
        }

        /// <summary>
        /// Borra el override en el portal. Llamar solo tras el ack del envío
        /// (o si no había nada que enviar). <paramref name="erroresEnvio"/> =
        /// errores por fila que reportó el portal: el override se consume
        /// igual (decisión del proyecto), pero se avisa.
        /// Nunca lanza: si el DELETE falla, el override se reaplica el
        /// siguiente ciclo (re-jalado idempotente, sin pérdida).
        /// </summary>
        public async Task ConsumirAsync(IPortalApi api, int erroresEnvio, CancellationToken ct)
        {
            if (!Pendiente) return;

            if (Aplicado && erroresEnvio > 0)
                EventoLog.Warning(_etiqueta + " · " + _nombreHumano + ": la re-sincronización terminó con "
                    + erroresEnvio + " error(es) del portal; se da por consumida. Revisa los errores de arriba.");

            try
            {
                await api.BorrarOverrideAsync(_emp.emp_id_msp, _modulo, ct).ConfigureAwait(false);
                if (Aplicado)
                    EventoLog.Info(_etiqueta + " · " + _nombreHumano + ": re-sincronización completada; se vuelve al incremental normal.");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                EventoLog.Warning(_etiqueta + " · " + _nombreHumano + ": no se pudo quitar la re-sincronización ("
                    + ex.Message + "); se repetirá el próximo ciclo.");
            }
        }
    }
}
