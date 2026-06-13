using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Pipes;
using PortalProveedoresCore.Repositorios;
using PortalProveedoresCore.Servicios;
using PortalProveedoresService.Repositorios;
using PortalProveedoresService.Sincronizacion;

namespace PortalProveedoresService
{
    /// <summary>
    /// Windows Service: orquesta los sincronizadores en orden, con un loop
    /// async cancelable, y publica eventos en vivo al Visor a través de un
    /// Named Pipe (ver <see cref="ServidorPipe"/>).
    ///
    /// Diseño del bucle:
    ///   - Un solo Task que va: ciclo → esperar (timer o wakeup) → repeat.
    ///   - Si OnStop dispara el CancellationToken, el ciclo termina en su
    ///     próximo punto de espera.
    ///   - Si llega <c>cmd:forzar_ciclo</c> desde el visor, despertamos al
    ///     loop antes de que cumpla el timer.
    ///   - Si llega <c>cmd:pausar</c>, marcamos un flag y saltamos los ciclos
    ///     hasta recibir <c>cmd:reanudar</c>.
    ///
    /// El intervalo vive en HKLM\...\MODE_TIMER (segundos).
    /// </summary>
    public partial class WSPortal : ServiceBase
    {
        private CancellationTokenSource _cts;
        private Task _ciclo;

        // Pipe / canal de eventos hacia el Visor
        private CanalEventos  _canal;
        private ServidorPipe  _servidor;
        private Action<string, string> _hookLog; // referencia para poder unsubscribe

        // Wakeup para CMD_FORZAR_CICLO. Se reasigna después de cada uso.
        private volatile TaskCompletionSource<bool> _wakeup;

        // Resolutor EMPRESA_ID → NOMBRE_CORTO desde CONFIG.FDB. Vive por la
        // vida del proceso; se invalida al inicio de cada ciclo para que
        // renames en Microsip se reflejen en el siguiente.
        private readonly IResolutorEmpresaMicrosip _resolutorEmpresa = new ResolutorEmpresaMicrosip();

        // Estado compartido para Snapshot. Protegido por _estadoLock.
        private readonly object _estadoLock = new object();
        private string  _estado = EstadoServicio.Iniciando;
        private int     _cicloContador;
        private int?    _cicloActualId;
        private string  _pasoActual;
        private DateTime? _inicioCicloActual;
        private DateTime? _ultimoCicloTerminado;
        private bool?   _ultimoCicloOk;
        private bool    _pausado;

        // Progreso del ciclo actual: pasos definidos en esta vuelta y cuántos
        // ya terminaron. El Visor lo usa para la ProgressBar determinada.
        private int?    _cicloPasosTotal;
        private int     _cicloPasosCompletados;

        public WSPortal()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)  { IniciarCiclo(); }
        protected override void OnStop()                 { DetenerCiclo(); }

        /// <summary>
        /// Wrappers internos para que Program.cs pueda iniciar/detener el ciclo
        /// en modo consola (F5 desde Visual Studio) sin pasar por el SCM.
        /// </summary>
        internal void IniciarManual() { IniciarCiclo(); }
        internal void DetenerManual() { DetenerCiclo(); }

        private void IniciarCiclo()
        {
            // 1) Canal + pipe (deben estar listos ANTES de publicar el primer evento).
            _canal    = new CanalEventos();
            _servidor = new ServidorPipe(_canal, ManejarComandoAsync);
            _servidor.Iniciar();

            // 2) Hook de EventoLog → canal, para que cada log se vea en el visor.
            _hookLog = (nivel, msg) => _canal.Publicar(new EventoBitacora { nivel = nivel, mensaje = msg });
            EventoLog.Publicador += _hookLog;

            _wakeup = NuevoWakeup();
            _cts    = new CancellationTokenSource();

            // 3) Anuncia "servicio iniciado" y arranca el loop.
            _canal.Publicar(new EventoServicioIniciado
            {
                nombre_servicio  = "PortalProveedoresService",
                version_servicio = "1.0",
                pid              = System.Diagnostics.Process.GetCurrentProcess().Id,
            });

            lock (_estadoLock) { _estado = EstadoServicio.Ocioso; }

            _ciclo = Task.Run(() => Ejecutar(_cts.Token));
            EventoLog.Info("Servicio iniciado.");
        }

        private void DetenerCiclo()
        {
            EventoLog.Info("Servicio deteniéndose...");

            lock (_estadoLock) { _estado = EstadoServicio.Deteniendo; }

            // Anuncia "deteniendo" antes de cortar nada: queremos que el visor
            // vea el evento mientras el pipe sigue vivo.
            if (_canal != null) _canal.Publicar(new EventoServicioDeteniendo { razon = "OnStop del SCM" });

            try
            {
                if (_cts != null) _cts.Cancel();
                if (_ciclo != null) _ciclo.Wait(TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                EventoLog.Error("DetenerCiclo: " + ex.Message);
            }

            // Desconectar el hook antes de cerrar el canal para evitar publicar
            // post-mortem (que se descartarían pero ensucian el flujo).
            if (_hookLog != null) { EventoLog.Publicador -= _hookLog; _hookLog = null; }

            if (_servidor != null) { try { _servidor.Detener(); _servidor.Dispose(); } catch { } _servidor = null; }
            if (_canal    != null) { try { _canal.Dispose(); }    catch { } _canal    = null; }

            EventoLog.Info("Servicio detenido.");
        }

        // ====================================================================
        // Bucle principal: ciclo + espera (timer o wakeup)
        // ====================================================================

        private async Task Ejecutar(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // Saltar si estamos en pausa — pero seguimos esperando el
                // timer para no quemar CPU. Wakeup (reanudar / forzar) nos saca.
                bool enPausa;
                lock (_estadoLock) { enPausa = _pausado; }

                if (!enPausa)
                {
                    try
                    {
                        await EjecutarCicloAsync(ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { /* shutdown limpio */ }
                    catch (Exception ex)
                    {
                        EventoLog.Error("Ciclo: " + ex.Message);
                    }
                }

                if (ct.IsCancellationRequested) return;

                // Espera el timer O un wakeup (cmd:forzar_ciclo, cmd:reanudar),
                // lo que ocurra primero.
                var espera = LeerIntervalo();
                await EsperarConWakeupAsync(espera, ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Un ciclo completo: lee config, construye dependencias, corre cada
        /// sincronizador en orden. Publica EVT_CICLO_INICIADO / EVT_CICLO_TERMINADO
        /// envolviendo al loop legacy de pasos.
        /// </summary>
        private async Task EjecutarCicloAsync(CancellationToken ct)
        {
            var reg = new RegistrosWindows();
            if (!reg.LeerRegistros(false))
            {
                reg.CrearRegistros(false);
            }
            if (string.IsNullOrWhiteSpace(reg.PORTAL_BASE_URL) || string.IsNullOrWhiteSpace(reg.PORTAL_API_KEY))
            {
                EventoLog.Warning("Ciclo abortado: PORTAL_BASE_URL o PORTAL_API_KEY no configurados.");
                return;
            }

            IPortalApi api = new PortalApi(reg.PORTAL_BASE_URL, reg.PORTAL_API_KEY);

            // Invalidamos el caché de CONFIG.FDB al inicio de cada ciclo: si el
            // usuario renombró una empresa en Microsip, la siguiente vuelta lo
            // recoge sin reiniciar el servicio.
            _resolutorEmpresa.Invalidar();

            // Cache de empresas autorizadas vivo SOLO durante este ciclo. La
            // primera vez que un sincronizador (Almacenes, Monedas, ...) la pida,
            // se hace UNA GET al portal; los pasos restantes reusan la lista de
            // memoria. El próximo ciclo crea otra instancia y vuelve a consultar
            // (así se reflejan empresas autorizadas/desautorizadas entre ciclos).
            ICacheEmpresasAutorizadas cacheEmpresas = new CacheEmpresasAutorizadas(api);

            var pasos = new List<ISincronizador>
            {
                new SincronizadorEmpresas(new EmpresasRepository(), api),
                // Nota: la configuración de campos libres en Firebird (BANCO,
                // PCTJE_RECHAZO, USO_CFDI, folio WEB, etc.) NO se hace aquí.
                // Se dispara desde el Configurador en el momento exacto en que
                // el operador autoriza la empresa (transición Bloqueada →
                // Autorizada), igual que el SOAP legacy. Ver
                // PortalProveedoresCore.Configuracion.ConfiguradorCamposLibres.
                new SincronizadorAlmacenes(_resolutorEmpresa, cacheEmpresas, new AlmacenesRepository(), api),
                new SincronizadorMonedas(_resolutorEmpresa, cacheEmpresas, new MonedasRepository(), api),
                new SincronizadorProveedores(_resolutorEmpresa, cacheEmpresas, new ProveedoresRepository(), api),
                new SincronizadorRecepciones(_resolutorEmpresa, cacheEmpresas, new RecepcionesRepository(), api),
                new SincronizadorCreditos   (_resolutorEmpresa, cacheEmpresas, new DoctosCpRepository(),    api),
                new SincronizadorNotas      (_resolutorEmpresa, cacheEmpresas, new DoctosCpRepository(),    api),
                // Aplicación (Portal → Microsip): detecta facturas y complementos
                // que el proveedor subió y que el servicio debe aplicar a Microsip.
                // Por ahora solo detecta y loguea (fase 1); la aplicación real
                // viene cuando se replique APLICAR_MICROSIP_33 del Delphi.
                new SincronizadorAplicacion (cacheEmpresas, api, _resolutorEmpresa, new AplicacionRepository()),
                // Próximos hitos se enchufan aquí en el orden del documento maestro:
                // new SincronizadorCamposLibres(_resolutorEmpresa, cacheEmpresas, ...),
                // new SincronizadorRecepciones(_resolutorEmpresa, cacheEmpresas, ...),
                // new SincronizadorNotas(_resolutorEmpresa, cacheEmpresas, ...),
                // new SincronizadorCreditos(_resolutorEmpresa, cacheEmpresas, ...),
                // new SincronizadorFacturas33(_resolutorEmpresa, cacheEmpresas, ...),
            };

            // Anuncia inicio del ciclo (estado compartido + evento al pipe).
            int cicloId;
            DateTime inicio = DateTime.Now;
            lock (_estadoLock)
            {
                _cicloContador++;
                cicloId                 = _cicloContador;
                _cicloActualId          = cicloId;
                _inicioCicloActual      = inicio;
                _pasoActual             = null;
                _estado                 = EstadoServicio.EjecutandoCiclo;
                _cicloPasosTotal        = pasos.Count;
                _cicloPasosCompletados  = 0;
            }
            _canal.Publicar(new EventoCicloIniciado
            {
                ciclo_id    = cicloId,
                total_pasos = pasos.Count,
            });
            EventoLog.Info("Iniciando ciclo de sincronización.");

            int pasosOk = 0, pasosFalla = 0;
            bool cicloOk = true;

            try
            {
                foreach (var paso in pasos)
                {
                    ct.ThrowIfCancellationRequested();

                    lock (_estadoLock) { _pasoActual = paso.Nombre; }
                    // EVT_PASO_INICIADO ya hace que el Visor pinte el header del paso
                    // ("→ Paso: X"). Un EventoLog.Info("Paso: X") aquí duplicaría la
                    // línea — el Visor lo recibiría por el hook EventoLog→canal y
                    // lo pintaría debajo del header. Por eso lo quitamos.
                    _canal.Publicar(new EventoPasoIniciado { paso = paso.Nombre });

                    bool ok = false;
                    try
                    {
                        ok = await paso.EjecutarAsync(ct).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        EventoLog.Error("Paso '" + paso.Nombre + "' lanzó excepción: " + ex.Message);
                    }

                    // Contamos el paso como completado tanto si terminó OK
                    // como si falló — para la barra de progreso es lo mismo
                    // (ya no se va a re-ejecutar en este ciclo).
                    lock (_estadoLock) { _cicloPasosCompletados++; }

                    _canal.Publicar(new EventoPasoTerminado
                    {
                        paso              = paso.Nombre,
                        ok                = ok,
                        items_procesados  = 0,    // los pasos en su versión actual no reportan conteo
                        mensaje_resumen   = ok ? "OK" : "Falló",
                    });

                    if (!ok)
                    {
                        EventoLog.Warning("Paso '" + paso.Nombre + "' falló: se aborta el resto del ciclo.");
                        pasosFalla++;
                        cicloOk = false;
                        break;
                    }
                    pasosOk++;
                }

                if (cicloOk)
                {
                    try
                    {
                        await api.SellarUltimaSincronizacionAsync(inicio.ToString("yyyy-MM-dd HH:mm:ss"), ct).ConfigureAwait(false);
                        EventoLog.Info("Ciclo terminado y sellado.");
                    }
                    catch (Exception ex)
                    {
                        EventoLog.Error("No se pudo sellar la última sincronización: " + ex.Message);
                    }
                }
            }
            finally
            {
                var fin       = DateTime.Now;
                var duracion  = (fin - inicio).TotalSeconds;

                lock (_estadoLock)
                {
                    _ultimoCicloTerminado    = fin;
                    _ultimoCicloOk           = cicloOk;
                    _cicloActualId           = null;
                    _pasoActual              = null;
                    _inicioCicloActual       = null;
                    _cicloPasosTotal         = null;
                    _cicloPasosCompletados   = 0;
                    _estado                  = _pausado ? EstadoServicio.Pausado : EstadoServicio.Ocioso;
                }

                _canal.Publicar(new EventoCicloTerminado
                {
                    ciclo_id     = cicloId,
                    ok           = cicloOk,
                    duracion_seg = duracion,
                    pasos_ok     = pasosOk,
                    pasos_falla  = pasosFalla,
                });
            }
        }

        private TimeSpan LeerIntervalo()
        {
            try
            {
                var reg = new RegistrosWindows();
                if (reg.LeerRegistros(false) && int.TryParse(reg.MODE_TIMER, out var seg) && seg > 0)
                    return TimeSpan.FromSeconds(seg);
            }
            catch { }
            return TimeSpan.FromMinutes(1);
        }

        // ====================================================================
        // Wakeup: timer + cmd:forzar_ciclo / cmd:reanudar
        // ====================================================================

        private static TaskCompletionSource<bool> NuevoWakeup()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private async Task EsperarConWakeupAsync(TimeSpan espera, CancellationToken ct)
        {
            // Snapshot del TCS para que un reset concurrente no pierda señal.
            var wake = _wakeup.Task;
            Task delay;
            try { delay = Task.Delay(espera, ct); }
            catch (OperationCanceledException) { return; }

            var primero = await Task.WhenAny(delay, wake).ConfigureAwait(false);
            if (primero == wake)
            {
                _wakeup = NuevoWakeup(); // recargar para el próximo ciclo
            }
        }

        private void Despertar()
        {
            var tcs = _wakeup;
            tcs.TrySetResult(true);
        }

        // ====================================================================
        // Handler de comandos entrantes del Visor
        // ====================================================================

        private async Task ManejarComandoAsync(MensajeBase msg)
        {
            switch (msg.tipo)
            {
                case TiposMensaje.CMD_PING:
                    _canal.Publicar(new EventoBitacora { nivel = NivelLog.Info, mensaje = "pong", fuente = "ServidorPipe" });
                    break;

                case TiposMensaje.CMD_FORZAR_CICLO:
                    lock (_estadoLock)
                    {
                        if (_estado == EstadoServicio.EjecutandoCiclo)
                        {
                            _canal.Publicar(new EventoBitacora { nivel = NivelLog.Warning, mensaje = "Ya hay un ciclo en curso; ignorando cmd:forzar_ciclo.", fuente = "ServidorPipe" });
                            return;
                        }
                    }
                    EventoLog.Info("Visor solicitó forzar ciclo.");
                    Despertar();
                    break;

                case TiposMensaje.CMD_PAUSAR:
                    lock (_estadoLock) { _pausado = true; if (_estado != EstadoServicio.EjecutandoCiclo) _estado = EstadoServicio.Pausado; }
                    EventoLog.Info("Visor solicitó pausa.");
                    PublicarSnapshot();
                    break;

                case TiposMensaje.CMD_REANUDAR:
                    lock (_estadoLock) { _pausado = false; if (_estado == EstadoServicio.Pausado) _estado = EstadoServicio.Ocioso; }
                    EventoLog.Info("Visor solicitó reanudar.");
                    Despertar();
                    PublicarSnapshot();
                    break;

                case TiposMensaje.CMD_SOLICITAR_SNAPSHOT:
                    PublicarSnapshot();
                    break;
            }

            // 'await' formal para satisfacer la firma async; los comandos actuales
            // todos son no-async (publicar al canal es no-bloqueante).
            await Task.FromResult(0).ConfigureAwait(false);
        }

        private void PublicarSnapshot()
        {
            if (_canal == null) return;

            var snap = new EventoSnapshot();
            lock (_estadoLock)
            {
                snap.estado                   = _estado;
                snap.ciclo_actual_id          = _cicloActualId;
                snap.paso_actual              = _pasoActual;
                snap.inicio_ciclo_actual      = _inicioCicloActual;
                snap.ultimo_ciclo_terminado   = _ultimoCicloTerminado;
                snap.ultimo_ciclo_ok          = _ultimoCicloOk;
                snap.ciclo_pasos_total        = _cicloPasosTotal;
                snap.ciclo_pasos_completados  = _cicloPasosTotal.HasValue ? (int?) _cicloPasosCompletados : null;
            }
            snap.timer_segundos = (int) LeerIntervalo().TotalSeconds;

            _canal.Publicar(snap);
        }
    }
}
