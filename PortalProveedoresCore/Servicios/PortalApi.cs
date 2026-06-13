using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresCore.Servicios
{
    /// <summary>
    /// Cliente HTTP del portal CI4. HttpClient singleton (no se hace new por
    /// llamada — el patrón correcto en .NET Framework). Autentica con X-API-Key.
    ///
    /// Serializa con JavaScriptSerializer (System.Web.Extensions) para no
    /// agregar NuGet. Suficiente para POCOs planos.
    /// </summary>
    public sealed class PortalApi : IPortalApi
    {
        private static readonly HttpClient _http = ConstruirHttpClient();
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        // HttpMethod.Patch no existe en .NET Framework 4.6 — definimos uno propio.
        private static readonly HttpMethod _patch = new HttpMethod("PATCH");

        private readonly string _baseUrl;
        private readonly string _apiKey;

        public PortalApi(string baseUrl, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("baseUrl vacío. Configurar HKLM\\...\\PORTAL_BASE_URL.", "baseUrl");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("apiKey vacío. Configurar HKLM\\...\\PORTAL_API_KEY.", "apiKey");

            _baseUrl = baseUrl.TrimEnd('/');
            _apiKey  = apiKey;
        }

        public async Task<ResumenSync> SincronizarEmpresasAsync(IEnumerable<EmpresaMicrosip> empresas, CancellationToken ct)
        {
            var lista = new List<EmpresaMicrosip>(empresas);
            var cuerpo = _json.Serialize(new { empresas = lista });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/empresas/sync", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/empresas/sync devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<ResumenSync>(body);
            }
        }

        public async Task SellarUltimaSincronizacionAsync(string timestamp, CancellationToken ct)
        {
            var cuerpo = _json.Serialize(new { timestamp = timestamp });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/empresas/sellar", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/empresas/sellar devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
            }
        }

        public async Task<ResumenSync> SincronizarAlmacenesAsync(int empIdMsp, IEnumerable<AlmacenMicrosip> almacenes, CancellationToken ct)
        {
            var lista  = new List<AlmacenMicrosip>(almacenes);
            var cuerpo = _json.Serialize(new { emp_id_msp = empIdMsp, almacenes = lista });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/almacenes/sync", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/almacenes/sync devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<ResumenSync>(body);
            }
        }

        public async Task<ResumenSync> SincronizarMonedasAsync(int empIdMsp, IEnumerable<MonedaMicrosip> monedas, CancellationToken ct)
        {
            var lista  = new List<MonedaMicrosip>(monedas);
            var cuerpo = _json.Serialize(new { emp_id_msp = empIdMsp, monedas = lista });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/monedas/sync", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/monedas/sync devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<ResumenSync>(body);
            }
        }

        public async Task<ResumenSync> SincronizarRecepcionesAsync(int empIdMsp, IEnumerable<RecepcionMicrosip> recepciones, CancellationToken ct)
        {
            var lista  = new List<RecepcionMicrosip>(recepciones);
            var cuerpo = _json.Serialize(new { emp_id_msp = empIdMsp, recepciones = lista });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/recepciones/sync", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/recepciones/sync devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<ResumenSync>(body);
            }
        }

        public async Task<ResumenSync> SincronizarCreditosAsync(int empIdMsp, IEnumerable<DoctoCpMicrosip> creditos, CancellationToken ct)
        {
            var lista  = new List<DoctoCpMicrosip>(creditos);
            var cuerpo = _json.Serialize(new { emp_id_msp = empIdMsp, creditos = lista });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/creditos/sync", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/creditos/sync devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<ResumenSync>(body);
            }
        }

        public async Task<ResumenSync> SincronizarNotasAsync(int empIdMsp, IEnumerable<DoctoCpMicrosip> notas, CancellationToken ct)
        {
            var lista  = new List<DoctoCpMicrosip>(notas);
            var cuerpo = _json.Serialize(new { emp_id_msp = empIdMsp, notas = lista });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/notas/sync", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/notas/sync devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<ResumenSync>(body);
            }
        }

        public async Task<bool> ObtenerAplicaDirAsync(CancellationToken ct)
        {
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, "/api/aplicacion/aplica-dir"))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET /api/aplicacion/aplica-dir devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaAplicaDir>(body);
                return env != null && env.aplica_dir;
            }
        }

        public async Task<bool> ProbarConexionAsync(CancellationToken ct)
        {
            // Endpoint barato y autenticado: si la URL es alcanzable y la API
            // key es válida, el filtro deja pasar y responde 2xx. Bad key →
            // 401/403; URL mala → excepción de red. Cualquier no-2xx o
            // excepción cuenta como "no conectó".
            try
            {
                using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, "/api/aplicacion/aplica-dir"))
                using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
                {
                    return resp.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<PendientesAplicacion> ObtenerPendientesAsync(int empIdMsp, CancellationToken ct)
        {
            var ruta = "/api/aplicacion/pendientes?emp_id_msp=" + empIdMsp;
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET " + ruta + " devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<PendientesAplicacion>(body);
            }
        }

        public async Task<FacturaAplicar[]> ObtenerFacturasAplicarAsync(int empIdMsp, CancellationToken ct)
        {
            var ruta = "/api/aplicacion/facturas-aplicar?emp_id_msp=" + empIdMsp;
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET " + ruta + " devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaFacturasAplicar>(body);
                return env != null && env.facturas != null ? env.facturas : new FacturaAplicar[0];
            }
        }

        public async Task<RespuestaFacturasEscritorio> ObtenerFacturasPendientesEscritorioAsync(
            FiltroFacturasEscritorio f, CancellationToken ct)
        {
            if (f == null) throw new ArgumentNullException("filtro");

            var qs = new System.Text.StringBuilder();
            qs.Append("emp_id_msp=").Append(f.EmpIdMsp);
            if (f.ProveedorId > 0) qs.Append("&proveedor_id=").Append(f.ProveedorId);
            if (f.AlmacenId   > 0) qs.Append("&almacen_id=").Append(f.AlmacenId);
            if (!string.IsNullOrWhiteSpace(f.NombreProveedor))
                qs.Append("&nombre_proveedor=").Append(Uri.EscapeDataString(f.NombreProveedor.Trim()));
            if (!string.IsNullOrWhiteSpace(f.NombreAlmacen))
                qs.Append("&nombre_almacen=").Append(Uri.EscapeDataString(f.NombreAlmacen.Trim()));
            if (f.Desde.HasValue)  qs.Append("&desde=").Append(f.Desde.Value.ToString("yyyy-MM-dd"));
            if (f.Hasta.HasValue)  qs.Append("&hasta=").Append(f.Hasta.Value.ToString("yyyy-MM-dd"));
            if (f.SoloPorVencer)   qs.Append("&por_vencer=1");
            if (f.Limit > 0)       qs.Append("&limit=").Append(f.Limit);
            // Tab Descargar — réplica SelectDescargar (sin filtro de ESTATUS).
            if (f.TodosEstatus)    qs.Append("&todos_estatus=1");

            var ruta = "/api/escritorio/facturas-pendientes?" + qs.ToString();
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET " + ruta + " devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaFacturasEscritorio>(body);
                if (env == null)
                    return new RespuestaFacturasEscritorio { facturas = new FacturaPendienteEscritorio[0] };
                if (env.facturas == null) env.facturas = new FacturaPendienteEscritorio[0];
                return env;
            }
        }

        public async Task<ComplementoAplicar[]> ObtenerComplementosAplicarAsync(int empIdMsp, CancellationToken ct)
        {
            var ruta = "/api/aplicacion/complementos-aplicar?emp_id_msp=" + empIdMsp;
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET " + ruta + " devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaComplementosAplicar>(body);
                return env != null && env.complementos != null ? env.complementos : new ComplementoAplicar[0];
            }
        }

        public async Task<ComplementoAplicar[]> ObtenerComplementosPendientesEscritorioAsync(
            FiltroComplementosEscritorio filtro, CancellationToken ct)
        {
            if (filtro == null) throw new ArgumentNullException("filtro");
            // Construcción manual del query string para mantenernos congruentes
            // con ObtenerFacturasPendientesEscritorioAsync (no usamos
            // HttpUtility para no jalar System.Web).
            var sb = new System.Text.StringBuilder();
            sb.Append("/api/escritorio/complementos-pendientes?emp_id_msp=").Append(filtro.EmpIdMsp);
            if (filtro.ProveedorId > 0)   sb.Append("&proveedor_id=").Append(filtro.ProveedorId);
            if (filtro.AlmacenId   > 0)   sb.Append("&almacen_id=").Append(filtro.AlmacenId);
            if (filtro.Desde.HasValue)    sb.Append("&desde=").Append(filtro.Desde.Value.ToString("yyyy-MM-dd"));
            if (filtro.Hasta.HasValue)    sb.Append("&hasta=").Append(filtro.Hasta.Value.ToString("yyyy-MM-dd"));
            if (filtro.SoloPorVencer)     sb.Append("&por_vencer=1");
            if (filtro.Limit > 0)         sb.Append("&limit=").Append(filtro.Limit);

            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, sb.ToString()))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET complementos-pendientes devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaComplementosAplicar>(body);
                return env != null && env.complementos != null ? env.complementos : new ComplementoAplicar[0];
            }
        }

        public Task<bool> MarcarComplementoAplicadoAsync(int creditoFk, CancellationToken ct)
        {
            // Overload histórico (lo usa el Service) — delega con defaults
            // que reproducen el comportamiento de siempre.
            return MarcarComplementoAplicadoAsync(creditoFk, 0, null, ct);
        }

        public async Task<bool> MarcarComplementoAplicadoAsync(int creditoFk, int doctoCpId, string usuario, CancellationToken ct)
        {
            // Los parámetros aditivos solo viajan cuando traen valor — el
            // body del Service queda byte a byte igual que antes
            // ({"credito_fk":N}) y un PHP viejo tampoco se entera.
            string cuerpo;
            if (doctoCpId > 0 && !string.IsNullOrEmpty(usuario))
                cuerpo = _json.Serialize(new { credito_fk = creditoFk, docto_cp_id = doctoCpId, usuario = usuario });
            else if (doctoCpId > 0)
                cuerpo = _json.Serialize(new { credito_fk = creditoFk, docto_cp_id = doctoCpId });
            else if (!string.IsNullOrEmpty(usuario))
                cuerpo = _json.Serialize(new { credito_fk = creditoFk, usuario = usuario });
            else
                cuerpo = _json.Serialize(new { credito_fk = creditoFk });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/aplicacion/marcar-complemento-aplicado", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/aplicacion/marcar-complemento-aplicado devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaMarcarComplemento>(body);
                return env != null && env.ok;
            }
        }

        public async Task<AdjuntoMicrosip[]> ListarAdjuntosAsync(int doctoId, int empIdMsp, string tipo, CancellationToken ct)
        {
            var tipoLimpio = string.IsNullOrEmpty(tipo) ? "F" : tipo.ToUpperInvariant();
            if (tipoLimpio != "F" && tipoLimpio != "C") tipoLimpio = "F";

            var ruta = "/api/adjuntos?docto_id=" + doctoId
                     + "&emp=" + empIdMsp
                     + "&tipo=" + tipoLimpio;

            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET " + ruta + " devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaAdjuntos>(body);
                return env != null && env.adjuntos != null ? env.adjuntos : new AdjuntoMicrosip[0];
            }
        }

        public async Task<byte[]> DescargarAdjuntoAsync(int id, CancellationToken ct)
        {
            var ruta = "/api/adjuntos/" + id;

            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                if (!resp.IsSuccessStatusCode)
                {
                    // No lanzamos — el sincronizador trata "no se pudo descargar"
                    // como una incidencia recuperable: registra el id y sigue con
                    // los demás adjuntos. El Delphi hace exactamente lo mismo
                    // (Func_Facturas_3_3.pas:1203-1208).
                    return null;
                }

                return await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
        }

        public async Task<CfdiXmlMicrosip> ObtenerCfdiXmlAsync(string uuid, string tipo, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                throw new ArgumentException("uuid vacío.", "uuid");

            var tipoLimpio = string.IsNullOrEmpty(tipo) ? "F" : tipo.ToUpperInvariant();
            if (tipoLimpio != "F" && tipoLimpio != "C") tipoLimpio = "F";

            var ruta = "/api/aplicacion/cfdi-xml?uuid=" + Uri.EscapeDataString(uuid)
                     + "&tipo=" + tipoLimpio;

            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET " + ruta + " devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<CfdiXmlMicrosip>(body);
            }
        }

        public async Task<byte[]> ObtenerCfdiPdfAsync(string uuid, string tipo, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                throw new ArgumentException("uuid vacío.", "uuid");

            var tipoLimpio = string.IsNullOrEmpty(tipo) ? "F" : tipo.ToUpperInvariant();
            if (tipoLimpio != "F" && tipoLimpio != "C") tipoLimpio = "F";

            var ruta = "/api/aplicacion/cfdi-pdf?uuid=" + Uri.EscapeDataString(uuid)
                     + "&tipo=" + tipoLimpio;

            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                // 404 = el proveedor subió XML sin PDF (caso común). Devolvemos
                // null para que el cliente deshabilite el botón "Abrir PDF"
                // sin tratarlo como error de comunicación.
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new PortalApiException("GET " + ruta + " devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                }

                return await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
        }

        // ===== E.4 — rechazo + correo + descarte ============================

        public async Task<CorreoConfig> ObtenerCorreoConfigAsync(CancellationToken ct)
        {
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, "/api/aplicacion/correo-config"))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET correo-config devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                return _json.Deserialize<CorreoConfig>(body);
            }
        }

        public async Task<string> ObtenerCorreoProveedorAsync(int proveedorId, int empIdMsp, CancellationToken ct)
        {
            var ruta = "/api/aplicacion/proveedor-correo?proveedor_id=" + proveedorId
                     + "&emp_id_msp=" + empIdMsp;
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET proveedor-correo devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                var resp2 = _json.Deserialize<RespuestaCorreoProveedor>(body);
                return resp2 == null ? "" : (resp2.correo ?? "");
            }
        }

        public async Task<string> ObtenerCorreoProveedorPorRfcAsync(string rfc, int empIdMsp, CancellationToken ct)
        {
            // Réplica funcional del SOAP BuscarProveedorXRFC
            // (F_RECHAZA_ENVIA_CORREO.cs:97-136). El endpoint del portal
            // devuelve los correos separados por ';' (igual semántica que
            // el SOAP). Lo usa el Escritorio como fallback al rechazar
            // complementos cuando el PROVEEDOR_ID no apunta a un ACCESO.
            string rfcEnc = System.Net.WebUtility.UrlEncode(rfc ?? "");
            var ruta = "/api/aplicacion/proveedor-correo-por-rfc?rfc=" + rfcEnc
                     + "&emp_id_msp=" + empIdMsp;
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET proveedor-correo-por-rfc devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                var resp2 = _json.Deserialize<RespuestaCorreoProveedor>(body);
                return resp2 == null ? "" : (resp2.correo ?? "");
            }
        }

        public async Task<bool> RechazarFacturaAsync(int doctoCmId, string usuario, string motivo, CancellationToken ct)
        {
            // Réplica del SOAP ws.RECHAZA_FACTURA(DOCTO_CM, USUARIO, FECHA, MOTIVO)
            // (F_ENVIAR_RECHAZO.cs:69 + services/facturas.php:272-290): la llave
            // es el DOCTO_CM_ID MySQL de la factura (NO el RECEP_ID) y la fecha
            // de rechazo la sella el cliente, igual que el legacy con
            // DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").
            var cuerpo = _json.Serialize(new
            {
                docto_cm_id = doctoCmId,
                usuario     = usuario,
                motivo      = motivo,
                fecha       = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            });
            using (var req = NuevaRequest(HttpMethod.Post, "/api/aplicacion/rechazar-factura", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST rechazar-factura devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                var r = _json.Deserialize<RespuestaSimpleOk>(body);
                return r != null && r.ok;
            }
        }

        public async Task<bool> RechazarComplementoAsync(int doctoCpId, string usuario, string motivo, CancellationToken ct)
        {
            var cuerpo = _json.Serialize(new
            {
                docto_cp_id = doctoCpId,
                usuario     = usuario,
                motivo      = motivo,
            });
            using (var req = NuevaRequest(HttpMethod.Post, "/api/aplicacion/rechazar-complemento", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST rechazar-complemento devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                var r = _json.Deserialize<RespuestaSimpleOk>(body);
                return r != null && r.ok;
            }
        }

        public async Task<bool> DescartarFacturaAsync(int recepId, string usuario, CancellationToken ct)
        {
            var cuerpo = _json.Serialize(new
            {
                recep_id = recepId,
                usuario  = usuario,
            });
            using (var req = NuevaRequest(HttpMethod.Post, "/api/aplicacion/descartar-factura", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST descartar-factura devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                var r = _json.Deserialize<RespuestaSimpleOk>(body);
                return r != null && r.ok;
            }
        }

        public async Task<bool> ActualizarNuevoFolioAsync(int doctoCmId, CancellationToken ct)
        {
            // Réplica del SOAP ws.ACTUALIZA_NUEVO_FOLIO(DOCTO_CM_IDFACSQL)
            // (services/facturas.php:236-270). UPDATE FACTURA_PROVEEDOR_33
            // SET ESTATUS='R' WHERE DOCTO_CM_ID = ?.
            var cuerpo = _json.Serialize(new
            {
                docto_cm_id = doctoCmId,
            });
            using (var req = NuevaRequest(HttpMethod.Post, "/api/aplicacion/actualizar-nuevo-folio", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST actualizar-nuevo-folio devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                var r = _json.Deserialize<RespuestaSimpleOk>(body);
                return r != null && r.ok;
            }
        }

        public async Task<bool> MarcarFacturaRecepCanceladaAsync(int facturaMysqlId, string usuario, CancellationToken ct)
        {
            // El campo del body se llama recep_id por congruencia con el resto
            // de los endpoints de Aplicacion.php, pero el valor enviado es el
            // DOCTO_CM_ID de FACTURA_PROVEEDOR_33 (mismo nombre que el param
            // DOCTO_CM_IDFACSQL del SOAP legacy FACTURA_RECEP_CANCELADO).
            var cuerpo = _json.Serialize(new
            {
                recep_id = facturaMysqlId,
                usuario  = usuario,
                fecha    = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            });
            using (var req = NuevaRequest(HttpMethod.Post, "/api/aplicacion/factura-recep-cancelada", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST factura-recep-cancelada devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                var r = _json.Deserialize<RespuestaSimpleOk>(body);
                return r != null && r.ok;
            }
        }

        public async Task<bool> SincronizarFacturaYaAplicadaAsync(
            int facturaMysqlId, int recepId, string folioCompra, int compraId, CancellationToken ct)
        {
            // Réplica del SOAP ACTUALIZAR_FACTURA_PORTAL_ESCT
            // (services/facturas.php:172-234). El endpoint del portal hace dos
            // UPDATEs: FACTURA_PROVEEDOR_33 (WHERE DOCTO_CM_ID = factura_mysql_id)
            // y RECEPCIONES (WHERE RECEP_ID = recep_id).
            var cuerpo = _json.Serialize(new
            {
                factura_mysql_id = facturaMysqlId,
                recep_id         = recepId,
                folio_compra     = folioCompra ?? "",
                compra_id        = compraId,
            });
            using (var req = NuevaRequest(HttpMethod.Post, "/api/aplicacion/factura-ya-aplicada-sincronizar", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST factura-ya-aplicada-sincronizar devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                var r = _json.Deserialize<RespuestaSimpleOk>(body);
                return r != null && r.ok;
            }
        }

        private sealed class RespuestaCorreoProveedor { public string correo { get; set; } }
        private sealed class RespuestaSimpleOk        { public bool   ok     { get; set; } public int filas { get; set; } }

        public Task<RespuestaCatalogosFiltros> ObtenerCatalogosFiltrosAsync(
            int empIdMsp, CancellationToken ct)
        {
            // Compatibilidad — defaults: no APLICA_DIR, entidad facturas.
            return ObtenerCatalogosFiltrosAsync(empIdMsp, false, "facturas", ct);
        }

        public async Task<RespuestaCatalogosFiltros> ObtenerCatalogosFiltrosAsync(
            int empIdMsp, bool aplicaDir, string entidad, CancellationToken ct)
        {
            var ruta = "/api/escritorio/catalogos-filtros?emp_id_msp=" + empIdMsp;
            if (aplicaDir) ruta += "&aplica_dir=1";
            if (!string.IsNullOrEmpty(entidad))
                ruta += "&entidad=" + System.Net.WebUtility.UrlEncode(entidad);

            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET catalogos-filtros devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                var r = _json.Deserialize<RespuestaCatalogosFiltros>(body);
                if (r == null) r = new RespuestaCatalogosFiltros();
                if (r.proveedores == null) r.proveedores = new CatalogoFiltroItem[0];
                if (r.almacenes   == null) r.almacenes   = new CatalogoFiltroItem[0];
                return r;
            }
        }

        public async Task<RespuestaProveedoresRegistrados> ObtenerProveedoresRegistradosAsync(
            int empIdMsp, CancellationToken ct)
        {
            var ruta = "/api/escritorio/proveedores-registrados?emp_id_msp=" + empIdMsp;
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET proveedores-registrados devolvió " + (int)resp.StatusCode, resp.StatusCode, body);
                return _json.Deserialize<RespuestaProveedoresRegistrados>(body);
            }
        }

        public async Task<bool> MarcarFacturaAplicadaAsync(int recepId, string folioMsp, int compraId, CancellationToken ct)
        {
            var cuerpo = _json.Serialize(new
            {
                recep_id  = recepId,
                folio_msp = folioMsp,
                compra_id = compraId,
            });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/aplicacion/marcar-factura-aplicada", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/aplicacion/marcar-factura-aplicada devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaMarcarFactura>(body);
                return env != null && env.ok;
            }
        }

        public async Task<ResumenSync> SincronizarProveedoresAsync(int empIdMsp, IEnumerable<ProveedorMicrosip> proveedores, CancellationToken ct)
        {
            var lista  = new List<ProveedorMicrosip>(proveedores);
            var cuerpo = _json.Serialize(new { emp_id_msp = empIdMsp, proveedores = lista });

            using (var req = NuevaRequest(HttpMethod.Post, "/api/proveedores/sync", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("POST /api/proveedores/sync devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<ResumenSync>(body);
            }
        }

        // ===== Administración remota (consumido por el Configurador) ============

        public Task<List<EmpresaConfig>> ListarEmpresasAsync(CancellationToken ct)
        {
            return ListarEmpresasInternoAsync(soloAutorizadas: false, ct);
        }

        public Task<List<EmpresaConfig>> ListarEmpresasAutorizadasAsync(CancellationToken ct)
        {
            return ListarEmpresasInternoAsync(soloAutorizadas: true, ct);
        }

        private async Task<List<EmpresaConfig>> ListarEmpresasInternoAsync(bool soloAutorizadas, CancellationToken ct)
        {
            var ruta = soloAutorizadas ? "/api/empresas?solo_autorizadas=1" : "/api/empresas";

            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, ruta))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET " + ruta + " devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaEmpresas>(body);
                return new List<EmpresaConfig>(env != null && env.empresas != null ? env.empresas : new EmpresaConfig[0]);
            }
        }

        public async Task<EmpresaConfig> ActualizarEmpresaAsync(int idMsp, string estatus, string diferencia, ValorSincDesde sincDesde, CancellationToken ct)
        {
            if (sincDesde == null) sincDesde = ValorSincDesde.NoTocar;

            if (estatus == null && diferencia == null && !sincDesde.Toca)
                throw new ArgumentException("Al menos uno de estatus/diferencia/sincDesde debe venir distinto de null.");

            // Dictionary<string, object> en lugar de <string, string> porque
            // sinc_desde puede ser literalmente null en el JSON (y JavaScriptSerializer
            // mapea null → JSON null correctamente con object).
            var dict = new Dictionary<string, object>();
            if (estatus    != null) dict["estatus"]    = estatus;
            if (diferencia != null) dict["diferencia"] = diferencia;
            if (sincDesde.Toca)
            {
                dict["sinc_desde"] = sincDesde.EsNull
                    ? (object) null
                    : sincDesde.Fecha.ToString("yyyy-MM-dd HH:mm:ss");
            }

            var cuerpo = _json.Serialize(dict);

            using (var req = NuevaRequest(_patch, "/api/empresas/" + idMsp, cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("PATCH /api/empresas/" + idMsp + " devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaEmpresa>(body);
                return env != null ? env.empresa : null;
            }
        }

        public async Task<List<DiaRecepcion>> ListarDiasAsync(CancellationToken ct)
        {
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, "/api/dias"))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET /api/dias devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaDias>(body);
                return new List<DiaRecepcion>(env != null && env.dias != null ? env.dias : new DiaRecepcion[0]);
            }
        }

        public async Task<List<DiaRecepcion>> ActualizarDiasAsync(IEnumerable<DiaRecepcion> cambios, CancellationToken ct)
        {
            // Proyectamos a un shape minimal {numero, recibe} — el portal valida y
            // los nombres son redundantes en el wire. Mantenemos DiaRecepcion como
            // el modelo "rico" para la UI pero no lo enviamos completo.
            var lista = new List<object>();
            foreach (var c in cambios)
                lista.Add(new { numero = c.numero, recibe = c.recibe });

            var cuerpo = _json.Serialize(new { dias = lista });

            using (var req = NuevaRequest(_patch, "/api/dias", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("PATCH /api/dias devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaDias>(body);
                return new List<DiaRecepcion>(env != null && env.dias != null ? env.dias : new DiaRecepcion[0]);
            }
        }

        public async Task<List<ParametroPortal>> ListarParametrosAsync(CancellationToken ct)
        {
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, "/api/parametros"))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET /api/parametros devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                var env = _json.Deserialize<RespuestaParametros>(body);
                return new List<ParametroPortal>(env != null && env.parametros != null ? env.parametros : new ParametroPortal[0]);
            }
        }

        public async Task<TemaPortal> ObtenerTemaAsync(CancellationToken ct)
        {
            using (var req = NuevaRequestSinCuerpo(HttpMethod.Get, "/api/portal-config"))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("GET /api/portal-config devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<TemaPortal>(body);
            }
        }

        public async Task<ResultadoActualizacionParametros> ActualizarParametrosAsync(IEnumerable<ParametroPortal> cambios, CancellationToken ct)
        {
            // Solo enviamos clave/valor en el wire — la descripcion la pone el server.
            var lista = new List<object>();
            foreach (var c in cambios)
                lista.Add(new { clave = c.clave, valor = c.valor });

            var cuerpo = _json.Serialize(new { cambios = lista });

            using (var req = NuevaRequest(_patch, "/api/parametros", cuerpo))
            using (var resp = await _http.SendAsync(req, ct).ConfigureAwait(false))
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                    throw new PortalApiException("PATCH /api/parametros devolvió " + (int)resp.StatusCode, resp.StatusCode, body);

                return _json.Deserialize<ResultadoActualizacionParametros>(body);
            }
        }

        // ===== Helpers internos =================================================

        private HttpRequestMessage NuevaRequest(HttpMethod metodo, string ruta, string cuerpoJson)
        {
            var req = new HttpRequestMessage(metodo, _baseUrl + ruta);
            req.Headers.Add("X-API-Key", _apiKey);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            req.Content = new StringContent(cuerpoJson, Encoding.UTF8, "application/json");
            return req;
        }

        /// <summary>Variante para GET — sin Content, mismo header X-API-Key.</summary>
        private HttpRequestMessage NuevaRequestSinCuerpo(HttpMethod metodo, string ruta)
        {
            var req = new HttpRequestMessage(metodo, _baseUrl + ruta);
            req.Headers.Add("X-API-Key", _apiKey);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return req;
        }

        // ===== DTOs de envoltura (forma del JSON que devuelve el portal) =======

        private sealed class RespuestaEmpresas       { public EmpresaConfig[]   empresas    { get; set; } }
        private sealed class RespuestaEmpresa        { public EmpresaConfig     empresa     { get; set; } }
        private sealed class RespuestaDias           { public DiaRecepcion[]    dias        { get; set; } }
        private sealed class RespuestaParametros     { public ParametroPortal[] parametros  { get; set; } }
        private sealed class RespuestaAplicaDir      { public bool              aplica_dir  { get; set; } }
        private sealed class RespuestaAdjuntos             { public AdjuntoMicrosip[]    adjuntos     { get; set; } }
        private sealed class RespuestaMarcarFactura        { public bool                 ok           { get; set; } }
        private sealed class RespuestaFacturasAplicar      { public FacturaAplicar[]     facturas     { get; set; } }
        private sealed class RespuestaComplementosAplicar  { public ComplementoAplicar[] complementos { get; set; } }
        private sealed class RespuestaMarcarComplemento    { public bool                 ok           { get; set; } }

        private static HttpClient ConstruirHttpClient()
        {
            var c = new HttpClient();
            c.Timeout = TimeSpan.FromSeconds(60);
            return c;
        }
    }
}
