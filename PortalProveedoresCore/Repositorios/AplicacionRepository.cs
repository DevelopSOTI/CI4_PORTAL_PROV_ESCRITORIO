using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Configuracion;
using PortalProveedoresCore.Logging;
using PortalProveedoresCore.Modelos;

namespace PortalProveedoresCore.Repositorios
{
    /// <summary>
    /// Implementación de <see cref="IAplicacionRepository"/>. Réplica de
    /// <c>APLICAR_MICROSIP_33</c> del Delphi (Func_Facturas_3_3.pas:348-1262).
    ///
    /// En sub-fase 2.2 solo ejecuta los bloques 1-8 y al final hace ROLLBACK
    /// — es un dry-run que NO modifica nada en Microsip, pero sí valida que
    /// los SQLs son aceptados con datos reales de producción.
    ///
    /// Bloques implementados:
    /// <list type="number">
    ///   <item>SELECT DOCTOS_CM origen (recepción). Valida existencia y ESTATUS.</item>
    ///   <item>Verifica que FOLIO_COMPRA del proveedor no exista ya en DOCTOS_CM tipo 'C'.</item>
    ///   <item>SIGUIENTE_FOLIO('WEB'): lee FOLIOS_COMPRAS, incrementa, hace UPDATE.</item>
    ///   <item>GEN_DOCTO_ID + INSERT DOCTOS_CM (encabezado tipo 'C').</item>
    ///   <item>GEN_DOCTO_ID + INSERT DOCTOS_CM_LIGAS (liga recepción → compra).</item>
    ///   <item>Loop DOCTOS_CM_DET: GEN_DOCTO_ID + INSERT DOCTOS_CM_DET + INSERT DOCTOS_CM_LIGAS_DET.</item>
    ///   <item>Loop IMPUESTOS_DOCTOS_CM: INSERT IMPUESTOS_DOCTOS_CM.</item>
    ///   <item>(Implícito) — al final del bloque 7 se forzaría el rollback en dry-run.</item>
    /// </list>
    /// </summary>
    public sealed class AplicacionRepository : IAplicacionRepository
    {
        // ================================================================
        // Método principal — MODO PRODUCCIÓN (sub-fase 2.4)
        // ================================================================

        public async Task<ResultadoAplicacion> AplicarFacturaAsync(
            string nombreEmpresaMicrosip, FacturaAplicar factura, CfdiXmlMicrosip cfdi,
            AdjuntoDescargado[] adjuntos,
            Func<int, string, Task<bool>> marcarPortalAsync,
            Func<int, string, Task<bool>> sincronizarPortalYaAplicadaAsync,
            CancellationToken ct)
        {
            var resultado = new ResultadoAplicacion
            {
                tipo         = ResultadoAplicacionTipo.Error,
                ultimoBloque = 0,
                mensaje      = "no se pudo arrancar",
            };

            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreEmpresaMicrosip))
            {
                resultado.tipo    = ResultadoAplicacionTipo.ErrorConexion;
                resultado.mensaje = "No se pudo abrir Firebird de '" + nombreEmpresaMicrosip + "'.";
                return resultado;
            }

            FbTransaction tx = null;
            bool commitHecho = false;

            try
            {
                tx = con.FBC.BeginTransaction(IsolationLevel.ReadCommitted);

                // Bloques 1-11: misma lógica que el dry-run.
                var ejecucion = await EjecutarBloques1A11Async(
                    con.FBC, tx, factura, cfdi, ct
                ).ConfigureAwait(false);

                resultado.ultimoBloque       = ejecucion.UltimoBloque;
                resultado.folioFinalGenerado = ejecucion.FolioFinal;
                resultado.nuevoDoctoCmId     = ejecucion.NuevoDoctoCmId;
                resultado.renglonesDetalle   = ejecucion.RenglonesDetalle;
                resultado.filasImpuestos     = ejecucion.FilasImpuestos;
                resultado.cfdiCreado         = ejecucion.CfdiCreado;

                // Réplica del SOAP F_APLICAR_FACTURA.cs:190-231 — la recepción
                // ya está facturada en Microsip y se encontró la compra ligada.
                // Sincronizar el portal con esa compra (NO crear nada en
                // Firebird) y salir como ÉXITO. El callback dedicado va al
                // endpoint factura-ya-aplicada-sincronizar (UPDATE por
                // DOCTO_CM_ID del portal, no por RECEP_ID).
                if (ejecucion.Tipo == ResultadoAplicacionTipo.RecepcionYaFacturadaSincronizar)
                {
                    bool sincOk = false;
                    if (sincronizarPortalYaAplicadaAsync != null)
                    {
                        sincOk = await sincronizarPortalYaAplicadaAsync(
                            ejecucion.NuevoDoctoCmId, ejecucion.FolioFinal
                        ).ConfigureAwait(false);
                    }
                    resultado.portalMarcado = sincOk;

                    // No hicimos INSERT/UPDATE en Firebird — rollback por
                    // consistencia. (commitHecho queda false → finally lo hace.)
                    if (sincOk)
                    {
                        resultado.tipo    = ResultadoAplicacionTipo.OkDryRun; // semánticamente OK
                        resultado.mensaje = "Se actualizo solo en el portal la recepcion "
                                          + factura.FOLIO_RECEPCION
                                          + " correctamente, porque la factura ya estaba en Microsip"
                                          + ". Folio Microsip: " + ejecucion.FolioFinal
                                          + ", DOCTO_CM_ID compra: " + ejecucion.NuevoDoctoCmId + ".";
                    }
                    else
                    {
                        resultado.tipo    = ResultadoAplicacionTipo.RecepcionYaFacturadaSincronizar;
                        resultado.mensaje = "La factura ya estaba en Microsip "
                                          + "(compra DOCTO_CM_ID=" + ejecucion.NuevoDoctoCmId
                                          + ", FOLIO=" + ejecucion.FolioFinal
                                          + ") pero NO se pudo sincronizar el portal — revisar manualmente.";
                    }
                    return resultado;
                }

                if (ejecucion.Tipo != ResultadoAplicacionTipo.OkDryRun)
                {
                    // Cualquier saltada / error: rollback y salida.
                    resultado.tipo    = ejecucion.Tipo;
                    resultado.mensaje = ejecucion.Mensaje;
                    return resultado;
                }

                // === BLOQUE 12: adjuntos del portal → ZIP → ARCHIVOS_ADJUNTOS ===
                resultado.ultimoBloque = 12;
                var resAdj = await InsertarAdjuntosAsync(
                    con.FBC, tx, ejecucion.NuevoDoctoCmId, adjuntos, ct
                ).ConfigureAwait(false);
                resultado.adjuntosInsertados = resAdj.Insertados;
                resultado.adjuntosOmitidos   = resAdj.Omitidos;

                // === BLOQUE 13: UPDATE DOCTOS_CM ESTATUS='F' en la recepción ====
                resultado.ultimoBloque = 13;
                await MarcarRecepcionFacturadaAsync(
                    con.FBC, tx, factura.FOLIO_RECEPCION, ct
                ).ConfigureAwait(false);

                // === BLOQUE 14: llamada al portal — UPDATE FACTURA + RECEPCIONES ===
                // El contrato del callback es (compraId, folioMsp): el primer
                // argumento es el DOCTO_CM_ID de la compra Microsip recién
                // creada, que el portal escribe en COMPRA_ID — réplica del
                // SOAP ACTUALIZAR_FACTURA_PORTAL_33 (services/facturas.php:16-22:
                // COMPRA_ID = $DoctoCmID). El RECEP_ID lo captura cada consumidor
                // (AplicadorFacturas / SincronizadorAplicacion) en su lambda.
                resultado.ultimoBloque = 14;
                bool portalOk = false;
                if (marcarPortalAsync != null)
                {
                    portalOk = await marcarPortalAsync(
                        ejecucion.NuevoDoctoCmId, ejecucion.FolioFinal
                    ).ConfigureAwait(false);
                }
                resultado.portalMarcado = portalOk;

                if (!portalOk)
                {
                    // El portal NO se pudo marcar — rollback Firebird para
                    // mantener los dos lados sincronizados.
                    resultado.tipo    = ResultadoAplicacionTipo.Error;
                    resultado.mensaje = "El portal rechazó marcar la factura como aplicada — rollback de Firebird.";
                    return resultado;
                }

                // === BLOQUE 15: COMMIT ==========================================
                resultado.ultimoBloque = 15;
                tx.Commit();
                commitHecho = true;

                resultado.tipo    = ResultadoAplicacionTipo.OkDryRun; // semánticamente "OK"
                resultado.mensaje = "APLICADA. Folio Microsip: " + ejecucion.FolioFinal
                                  + ". DOCTO_CM_ID nuevo: " + ejecucion.NuevoDoctoCmId
                                  + ". Renglones: " + ejecucion.RenglonesDetalle
                                  + ". Impuestos: " + ejecucion.FilasImpuestos
                                  + ". Adjuntos insertados: " + resAdj.Insertados
                                  + (resAdj.Omitidos > 0 ? " (omitidos: " + resAdj.Omitidos + ")" : "")
                                  + ". CFDI: " + (ejecucion.CfdiCreado ? "creado" : "ya existía") + ".";
                return resultado;
            }
            catch (Exception ex)
            {
                resultado.tipo    = ResultadoAplicacionTipo.Error;
                resultado.mensaje = "Excepción en bloque " + resultado.ultimoBloque + ": "
                                  + ex.GetType().Name + ": " + ex.Message;
                EventoLog.Error("AplicacionRepository (real): bloque " + resultado.ultimoBloque + " — " + ex);
                return resultado;
            }
            finally
            {
                if (tx != null)
                {
                    if (!commitHecho)
                    {
                        try { tx.Rollback(); } catch { }
                    }
                    tx.Dispose();
                }
                con.Desconectar();
            }
        }

        // ================================================================
        // Método principal — SIN RECEPCIÓN (réplica APLICAR_SIN_RECEPCION)
        // ================================================================

        /// <summary>
        /// Réplica del SOAP <c>APLICAR_SIN_RECEPCION</c>
        /// (F_APLICAR_FACTURA.cs:1007-1689). Aplica una factura del portal
        /// que NO tiene recepción ligada (RECEP_ID = 0): el operador elige
        /// un artículo NO almacenable y una condición de pago en el modal
        /// del Escritorio, y se crea el DOCTOS_CM tipo 'C' directamente,
        /// sin DOCTOS_CM_LIGAS.
        /// </summary>
        public async Task<ResultadoAplicacion> AplicarFacturaSinRecepcionAsync(
            string nombreEmpresaMicrosip, FacturaAplicar factura,
            string articuloNombre, string condicionPagoNombre,
            CfdiXmlMicrosip cfdi, AdjuntoDescargado[] adjuntos,
            Func<int, string, Task<bool>> marcarPortalAsync,
            CancellationToken ct)
        {
            var resultado = new ResultadoAplicacion
            {
                tipo         = ResultadoAplicacionTipo.Error,
                ultimoBloque = 0,
                mensaje      = "no se pudo arrancar",
            };

            if (string.IsNullOrWhiteSpace(articuloNombre))
            {
                resultado.tipo    = ResultadoAplicacionTipo.ArticuloNoExiste;
                resultado.mensaje = "No se seleccionó un artículo en el modal.";
                return resultado;
            }
            if (string.IsNullOrWhiteSpace(condicionPagoNombre))
            {
                resultado.tipo    = ResultadoAplicacionTipo.CondicionPagoNoExiste;
                resultado.mensaje = "No se seleccionó una condición de pago en el modal.";
                return resultado;
            }
            if (string.IsNullOrWhiteSpace(factura.UUID))
            {
                resultado.tipo    = ResultadoAplicacionTipo.UuidVacio;
                resultado.mensaje = "La factura no tiene UUID — no se puede aplicar.";
                return resultado;
            }

            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreEmpresaMicrosip))
            {
                resultado.tipo    = ResultadoAplicacionTipo.ErrorConexion;
                resultado.mensaje = "No se pudo abrir Firebird de '" + nombreEmpresaMicrosip + "'.";
                return resultado;
            }

            FbTransaction tx = null;
            bool commitHecho = false;

            try
            {
                tx = con.FBC.BeginTransaction(IsolationLevel.ReadCommitted);

                // === BLOQUE 1: artículo NO ALMACENABLE ===========================
                resultado.ultimoBloque = 1;
                var art = await LeerArticuloAsync(con.FBC, tx, articuloNombre, ct)
                    .ConfigureAwait(false);
                if (art == null)
                {
                    resultado.tipo    = ResultadoAplicacionTipo.ArticuloNoExiste;
                    resultado.mensaje = "El artículo '" + articuloNombre + "' no existe en Microsip.";
                    return resultado;
                }
                if (string.Equals(art.EsAlmacenable, "S", StringComparison.OrdinalIgnoreCase))
                {
                    resultado.tipo    = ResultadoAplicacionTipo.ArticuloEsAlmacenable;
                    resultado.mensaje = "El artículo '" + articuloNombre + "' es ALMACENABLE; para "
                                      + "facturas sin recepción se requiere uno NO almacenable.";
                    return resultado;
                }

                // === BLOQUE 2: condición de pago =================================
                resultado.ultimoBloque = 2;
                int condPagoId = await LeerCondPagoIdAsync(con.FBC, tx, condicionPagoNombre, ct)
                    .ConfigureAwait(false);
                if (condPagoId == 0)
                {
                    resultado.tipo    = ResultadoAplicacionTipo.CondicionPagoNoExiste;
                    resultado.mensaje = "La condición de pago '" + condicionPagoNombre
                                      + "' no existe en Microsip.";
                    return resultado;
                }

                // === BLOQUE 3: sucursal Matriz ===================================
                resultado.ultimoBloque = 3;
                int sucursalId = await LeerSucursalMatrizIdAsync(con.FBC, tx, ct)
                    .ConfigureAwait(false);
                if (sucursalId == 0)
                {
                    resultado.tipo    = ResultadoAplicacionTipo.SucursalMatrizNoExiste;
                    resultado.mensaje = "La sucursal 'Matriz' no existe en Microsip.";
                    return resultado;
                }

                // === BLOQUE 4: proveedor (CLAVE_PROV) ============================
                resultado.ultimoBloque = 4;
                var prov = await LeerProveedorPrincipalAsync(
                    con.FBC, tx, factura.PROVEEDOR_ID, ct
                ).ConfigureAwait(false);
                if (prov == null)
                {
                    resultado.tipo    = ResultadoAplicacionTipo.ProveedorNoExisteMicrosip;
                    resultado.mensaje = "El proveedor " + factura.PROVEEDOR_ID
                                      + " no existe en Microsip o no tiene clave principal.";
                    return resultado;
                }

                // === BLOQUE 5: FOLIO_PROV duplicado ==============================
                resultado.ultimoBloque = 5;
                if (!string.IsNullOrEmpty(factura.FOLIO_COMPRA)
                    && factura.FOLIO_COMPRA != "000000000")
                {
                    if (await ExisteCompraConFolioProvAsync(
                            con.FBC, tx, factura.FOLIO_COMPRA, prov.ProveedorId, ct
                        ).ConfigureAwait(false))
                    {
                        resultado.tipo    = ResultadoAplicacionTipo.FolioCompraDuplicado;
                        resultado.mensaje = "Ya hay una compra con FOLIO_PROV='" + factura.FOLIO_COMPRA
                                          + "' del proveedor " + prov.ProveedorId + ".";
                        return resultado;
                    }
                }

                // === BLOQUE 6: SIGUIENTE_FOLIO('WEB') ============================
                resultado.ultimoBloque = 6;
                var folioFinal = await SiguienteFolioWebAsync(con.FBC, tx, ct).ConfigureAwait(false);
                if (string.IsNullOrEmpty(folioFinal))
                {
                    resultado.tipo    = ResultadoAplicacionTipo.SerieWebNoConfigurada;
                    resultado.mensaje = "La serie 'WEB' no está registrada en FOLIOS_COMPRAS.";
                    return resultado;
                }
                resultado.folioFinalGenerado = folioFinal;

                string folioCompra = factura.FOLIO_COMPRA;
                string folioXml;
                if (string.IsNullOrEmpty(folioCompra) || folioCompra == "000000000")
                {
                    folioCompra = folioFinal;
                    folioXml    = "";
                }
                else
                {
                    folioXml = folioCompra;
                }

                // === BLOQUE 7: resolver MONEDA_ID ================================
                // Réplica del SOAP F_APLICAR_FACTURA.cs:1229 — el INSERT en
                // DOCTOS_CM usa el MONEDA_ID que viene del PORTAL
                // (C_FUNCIONES.cs:318 facturas[i].MONEDA_ID → F_FACTURAS.cs:360),
                // NO se resuelve por símbolo. Resolver por símbolo devolvía 0
                // cuando no había match (ej. MONEDA_SIMBOLO vacío) e insertar
                // MONEDA_ID=0 violaba el FOREIGN KEY MONEDAS_A_DOCTOS_CM.
                resultado.ultimoBloque = 7;
                int monedaId = factura.MONEDA_ID;
                if (monedaId <= 0)
                {
                    // Filas viejas/incompletas del portal traen MONEDA_ID=0.
                    // Caemos a la moneda nacional de Firebird como fallback
                    // seguro — NUNCA insertamos 0 (violaría el FK). El SOAP
                    // nunca consulta MONEDAS (siempre confía en el portal), así
                    // que no hay query legacy del flag de moneda local que
                    // replicar; en Microsip el peso nacional es siempre el
                    // MONEDA_ID más bajo (=1), por lo que SELECT FIRST 1 ...
                    // ORDER BY MONEDA_ID lo resuelve de forma determinista.
                    monedaId = await LeerMonedaNacionalIdAsync(con.FBC, tx, ct)
                        .ConfigureAwait(false);
                    if (monedaId <= 0)
                    {
                        resultado.tipo    = ResultadoAplicacionTipo.Error;
                        resultado.mensaje = "La factura no trae MONEDA_ID y no se pudo "
                                          + "resolver la moneda nacional en Microsip (tabla MONEDAS vacía).";
                        return resultado;
                    }
                }

                // === BLOQUE 8: INSERT DOCTOS_CM (sin recepción) ==================
                resultado.ultimoBloque = 8;
                int nuevoDoctoCmId = await GenDoctoIdAsync(con.FBC, tx, ct).ConfigureAwait(false);
                resultado.nuevoDoctoCmId = nuevoDoctoCmId;

                decimal importeTotal = factura.IMPORTE_NETO + factura.TOTAL_IMPUESTOS
                                     - factura.TOTAL_RETENCIONES - factura.DESCUENTO_GLOBAL;

                await InsertarDoctosCmSinRecepcionAsync(
                    con.FBC, tx, nuevoDoctoCmId, sucursalId, folioFinal, factura,
                    prov, factura.ALMACEN_FK_MSP, monedaId, condPagoId,
                    folioCompra, importeTotal, ct
                ).ConfigureAwait(false);

                // === BLOQUE 9: INSERT DOCTOS_CM_DET (1 línea genérica) ===========
                resultado.ultimoBloque = 9;
                int nuevoDetId = await GenDoctoIdAsync(con.FBC, tx, ct).ConfigureAwait(false);
                await InsertarDoctosCmDetSinRecepcionAsync(
                    con.FBC, tx, nuevoDetId, nuevoDoctoCmId, art, importeTotal, ct
                ).ConfigureAwait(false);
                resultado.renglonesDetalle = 1;

                // === BLOQUE 10: INSERT IMPUESTOS_DOCTOS_CM (del artículo) ========
                resultado.ultimoBloque = 10;
                resultado.filasImpuestos = await InsertarImpuestosDelArticuloAsync(
                    con.FBC, tx, nuevoDoctoCmId, articuloNombre, importeTotal, ct
                ).ConfigureAwait(false);

                // === BLOQUE 11: REPOSITORIO_CFDI + CFD_RECIBIDOS =================
                resultado.ultimoBloque = 11;
                var cfdiResult = await BuscarOInsertarCfdiAsync(
                    con.FBC, tx, factura, cfdi, folioCompra, folioXml, ct
                ).ConfigureAwait(false);
                resultado.cfdiCreado = cfdiResult.FueCreado;

                await InsertarCfdRecibidoAsync(
                    con.FBC, tx, nuevoDoctoCmId, cfdiResult.CfdiId,
                    ParsearFecha(factura.FECHA_FACTURA), cfdiResult.XmlPreparado, ct
                ).ConfigureAwait(false);

                // === BLOQUE 12: INSERT VENCIMIENTOS_CARGOS_CM ====================
                resultado.ultimoBloque = 12;
                await InsertarVencimientoCargoCmAsync(
                    con.FBC, tx, nuevoDoctoCmId, ParsearFecha(factura.FECHA_PAGO), ct
                ).ConfigureAwait(false);

                // === BLOQUE 13: EXEC GENERA_DOCTO_CP_CM ==========================
                resultado.ultimoBloque = 13;
                await EjecutarGeneraDoctoCpCmAsync(con.FBC, tx, nuevoDoctoCmId, ct)
                    .ConfigureAwait(false);

                // === BLOQUE 14: adjuntos =========================================
                resultado.ultimoBloque = 14;
                var resAdj = await InsertarAdjuntosAsync(
                    con.FBC, tx, nuevoDoctoCmId, adjuntos, ct
                ).ConfigureAwait(false);
                resultado.adjuntosInsertados = resAdj.Insertados;
                resultado.adjuntosOmitidos   = resAdj.Omitidos;

                // === BLOQUE 15: marcar portal ====================================
                // Contrato del callback: (compraId, folioMsp) — el primer
                // argumento es el DOCTO_CM_ID Microsip de la compra recién
                // creada. El SOAP para sin-recepción usaba
                // ACTUALIZAR_FACTURA_PORTAL_ESCT (services/facturas.php:172-234):
                // UPDATE FACTURA_PROVEEDOR_33 SET ... COMPRA_ID=$DoctoCmID ...
                // WHERE DOCTO_CM_ID=$DOCTO_CM_IDFACSQL (id MySQL de la factura,
                // que el Escritorio captura en su lambda) + UPDATE RECEPCIONES
                // WHERE RECEP_ID=0 (no-op). Pasar factura.RECEP_ID (=0) aquí
                // hacía que el endpoint rechazara la llamada y el flujo nunca
                // pudiera commitear.
                resultado.ultimoBloque = 15;
                bool portalOk = false;
                if (marcarPortalAsync != null)
                {
                    portalOk = await marcarPortalAsync(nuevoDoctoCmId, folioFinal)
                        .ConfigureAwait(false);
                }
                resultado.portalMarcado = portalOk;

                if (!portalOk)
                {
                    resultado.tipo    = ResultadoAplicacionTipo.Error;
                    resultado.mensaje = "El portal rechazó marcar la factura como aplicada — rollback de Firebird.";
                    return resultado;
                }

                // === BLOQUE 16: COMMIT ===========================================
                resultado.ultimoBloque = 16;
                tx.Commit();
                commitHecho = true;

                resultado.tipo    = ResultadoAplicacionTipo.OkDryRun;
                resultado.mensaje = "APLICADA SIN RECEPCIÓN. Folio Microsip: " + folioFinal
                                  + ". DOCTO_CM_ID nuevo: " + nuevoDoctoCmId
                                  + ". Renglones: 1"
                                  + ". Impuestos: " + resultado.filasImpuestos
                                  + ". Adjuntos insertados: " + resAdj.Insertados
                                  + (resAdj.Omitidos > 0 ? " (omitidos: " + resAdj.Omitidos + ")" : "")
                                  + ". CFDI: " + (cfdiResult.FueCreado ? "creado" : "ya existía") + ".";
                return resultado;
            }
            catch (Exception ex)
            {
                resultado.tipo    = ResultadoAplicacionTipo.Error;
                resultado.mensaje = "Excepción en bloque " + resultado.ultimoBloque + ": "
                                  + ex.GetType().Name + ": " + ex.Message;
                EventoLog.Error("AplicacionRepository (sin recepción): bloque "
                              + resultado.ultimoBloque + " — " + ex);
                return resultado;
            }
            finally
            {
                if (tx != null)
                {
                    if (!commitHecho)
                    {
                        try { tx.Rollback(); } catch { }
                    }
                    tx.Dispose();
                }
                con.Desconectar();
            }
        }

        // ================================================================
        // Método principal — COMPLEMENTOS (Fase 3)
        // ================================================================

        public async Task<ResultadoAplicacion> AplicarComplementoAsync(
            string nombreEmpresaMicrosip, ComplementoAplicar c, CfdiXmlMicrosip cfdi,
            AdjuntoDescargado[] adjuntos,
            Func<int, Task<bool>> marcarPortalAsync,
            CancellationToken ct)
        {
            var resultado = new ResultadoAplicacion
            {
                tipo         = ResultadoAplicacionTipo.Error,
                ultimoBloque = 0,
                mensaje      = "no se pudo arrancar",
            };

            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreEmpresaMicrosip))
            {
                resultado.tipo    = ResultadoAplicacionTipo.ErrorConexion;
                resultado.mensaje = "No se pudo abrir Firebird de '" + nombreEmpresaMicrosip + "'.";
                return resultado;
            }

            FbTransaction tx = null;
            bool commitHecho = false;

            try
            {
                tx = con.FBC.BeginTransaction(IsolationLevel.ReadCommitted);

                // === BLOQUE 1: SELECT DOCTOS_CP por FOLIO + NATURALEZA 'R' +
                //     PROVEEDOR_ID + CONCEPTO_CP_ID (F_APLICAR_COMPLEMENTO.cs:649-653)
                resultado.ultimoBloque = 1;
                var credito = await LeerCreditoOrigenAsync(
                    con.FBC, tx, c.FOLIO_CREDITO, c.PROVEEDOR_ID, c.CONCEPTO_CP_ID, ct
                ).ConfigureAwait(false);

                if (credito == null)
                {
                    resultado.tipo    = ResultadoAplicacionTipo.CreditoNoExiste;
                    resultado.mensaje = "No existe el crédito " + c.FOLIO_CREDITO
                                      + " del proveedor " + c.PROVEEDOR_ID + " en Microsip.";
                    return resultado;
                }

                resultado.nuevoDoctoCmId = credito.DoctoCpId; // reusamos el campo

                if (string.Equals(credito.TieneCfd, "S", StringComparison.OrdinalIgnoreCase))
                {
                    // Ya hay un CFDI asociado a este crédito — marcar portal y salir.
                    // (Mismo criterio del Delphi Func_Complementos.pas:307-333: si
                    //  TIENE_CFD='S' solo actualiza el portal y se sale).
                    bool yaMarcado = false;
                    if (marcarPortalAsync != null)
                    {
                        try { yaMarcado = await marcarPortalAsync(c.CREDITO_FK).ConfigureAwait(false); }
                        catch { yaMarcado = false; }
                    }
                    resultado.portalMarcado = yaMarcado;

                    resultado.tipo    = ResultadoAplicacionTipo.CreditoYaConCfdi;
                    resultado.mensaje = "El crédito " + c.FOLIO_CREDITO
                                      + " ya tiene CFDI asociado (TIENE_CFD='S')."
                                      + (yaMarcado ? " Portal marcado." : " Portal no se pudo marcar.");
                    if (yaMarcado) { tx.Commit(); commitHecho = true; }
                    return resultado;
                }

                // === BLOQUE 2: SELECT/INSERT REPOSITORIO_CFDI ====================
                resultado.ultimoBloque = 2;
                var cfdiResult = await BuscarOInsertarCfdiComplementoAsync(
                    con.FBC, tx, c, cfdi, ct
                ).ConfigureAwait(false);
                resultado.cfdiCreado = cfdiResult.FueCreado;

                // === BLOQUE 3: INSERT CFD_RECIBIDOS (CLAVE_SISTEMA='CP') =========
                resultado.ultimoBloque = 3;
                await InsertarCfdRecibidoCpAsync(
                    con.FBC, tx, credito.DoctoCpId, cfdiResult.CfdiId,
                    ParsearFecha(c.FECHA_COMPLEMENTO), cfdiResult.XmlPreparado, ct
                ).ConfigureAwait(false);

                // === BLOQUE 4: UPDATE DOCTOS_CP SET TIENE_CFD='S' ================
                resultado.ultimoBloque = 4;
                await MarcarCreditoConCfdiAsync(con.FBC, tx, credito.DoctoCpId, ct).ConfigureAwait(false);

                // === BLOQUE 5: adjuntos del complemento (opcional) ===============
                resultado.ultimoBloque = 5;
                var resAdj = await InsertarAdjuntosComplementoAsync(
                    con.FBC, tx, credito.DoctoCpId, adjuntos, ct
                ).ConfigureAwait(false);
                resultado.adjuntosInsertados = resAdj.Insertados;
                resultado.adjuntosOmitidos   = resAdj.Omitidos;

                // === BLOQUE 6: marcar portal (UPDATE COMPLEMENTO + CREDITOS) =====
                resultado.ultimoBloque = 6;
                bool portalOk = false;
                if (marcarPortalAsync != null)
                {
                    portalOk = await marcarPortalAsync(c.CREDITO_FK).ConfigureAwait(false);
                }
                resultado.portalMarcado = portalOk;

                if (!portalOk)
                {
                    resultado.tipo    = ResultadoAplicacionTipo.Error;
                    resultado.mensaje = "El portal rechazó marcar el complemento como aplicado — rollback de Firebird.";
                    return resultado;
                }

                // === BLOQUE 7: COMMIT ============================================
                resultado.ultimoBloque = 7;
                tx.Commit();
                commitHecho = true;

                resultado.tipo    = ResultadoAplicacionTipo.OkDryRun;
                resultado.mensaje = "ASOCIADO. DOCTO_CP_ID: " + credito.DoctoCpId
                                  + ". Adjuntos insertados: " + resAdj.Insertados
                                  + (resAdj.Omitidos > 0 ? " (omitidos: " + resAdj.Omitidos + ")" : "")
                                  + ". CFDI: " + (cfdiResult.FueCreado ? "creado" : "ya existía") + ".";
                return resultado;
            }
            catch (Exception ex)
            {
                resultado.tipo    = ResultadoAplicacionTipo.Error;
                resultado.mensaje = "Excepción en bloque " + resultado.ultimoBloque + ": "
                                  + ex.GetType().Name + ": " + ex.Message;
                EventoLog.Error("AplicacionRepository (complemento): bloque " + resultado.ultimoBloque + " — " + ex);
                return resultado;
            }
            finally
            {
                if (tx != null)
                {
                    if (!commitHecho)
                    {
                        try { tx.Rollback(); } catch { }
                    }
                    tx.Dispose();
                }
                con.Desconectar();
            }
        }

        // ================================================================
        // Método principal — COMPLEMENTOS rama "TIENE_CFD='S'" (Fase 3 fix #4)
        // ================================================================

        /// <summary>
        /// Réplica de la rama <c>TIENE_CFDI=='S'</c> del SOAP
        /// <c>F_APLICAR_COMPLEMENTO.cs:794-819</c>:
        /// <code>
        /// else if (TIENE_CFDI == "S") {
        ///   CFDI_ID = _repCFDI.ExisteRrepositorio(UUID, con, tran);
        ///   if (CFDI_ID > 0) {
        ///     AsociaDocumentosAdjuntos(DOCTO_CP_ID_MSP, con, tran, out msg);
        ///     _repCFDI.ActualizaTIPO_DOCTO_MSP(CFDI_ID, con, tran, tipo_docto_msp, out msg);
        ///     ws.ACTUALIZAR_COMPLEMENTO_PORTAL(...);
        ///   }
        ///   msg += "Ya tiene un CFD asociado\n\r";
        /// }
        /// </code>
        ///
        /// El Escritorio invoca este método DESPUÉS de recibir
        /// <see cref="ResultadoAplicacionTipo.CreditoYaConCfdi"/> de
        /// <see cref="AplicarComplementoAsync"/>. Reabre una nueva
        /// transacción Firebird (la del primer intento ya hizo commit
        /// solo del marcado del portal) y ejecuta los pasos faltantes.
        /// </summary>
        public async Task<ResultadoAplicacion> AsociarComplementoYaConCfdiAsync(
            string nombreEmpresaMicrosip, ComplementoAplicar c,
            AdjuntoDescargado[] adjuntos,
            Func<int, Task<bool>> marcarPortalAsync,
            CancellationToken ct)
        {
            var resultado = new ResultadoAplicacion
            {
                tipo         = ResultadoAplicacionTipo.Error,
                ultimoBloque = 0,
                mensaje      = "no se pudo arrancar",
            };

            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreEmpresaMicrosip))
            {
                resultado.tipo    = ResultadoAplicacionTipo.ErrorConexion;
                resultado.mensaje = "No se pudo abrir Firebird de '" + nombreEmpresaMicrosip + "'.";
                return resultado;
            }

            FbTransaction tx = null;
            bool commitHecho = false;

            try
            {
                tx = con.FBC.BeginTransaction(IsolationLevel.ReadCommitted);

                // === BLOQUE 1: re-leer crédito (mismo SELECT que el flujo normal) ===
                resultado.ultimoBloque = 1;
                var credito = await LeerCreditoOrigenAsync(
                    con.FBC, tx, c.FOLIO_CREDITO, c.PROVEEDOR_ID, c.CONCEPTO_CP_ID, ct
                ).ConfigureAwait(false);

                if (credito == null)
                {
                    resultado.tipo    = ResultadoAplicacionTipo.CreditoNoExiste;
                    resultado.mensaje = "No existe el crédito " + c.FOLIO_CREDITO
                                      + " del proveedor " + c.PROVEEDOR_ID + " en Microsip.";
                    return resultado;
                }

                resultado.nuevoDoctoCmId = credito.DoctoCpId;

                // Este método solo aplica cuando el crédito YA tiene CFDI. Si
                // por alguna razón ya no lo tiene (otro operador limpió la
                // marca?), regresamos a la ruta normal con un error claro.
                if (!string.Equals(credito.TieneCfd, "S", StringComparison.OrdinalIgnoreCase))
                {
                    resultado.tipo    = ResultadoAplicacionTipo.Error;
                    resultado.mensaje = "El crédito " + c.FOLIO_CREDITO
                                      + " ya no tiene TIENE_CFD='S' — usar AplicarComplementoAsync.";
                    return resultado;
                }

                // === BLOQUE 2: buscar CFDI_ID por UUID del complemento ===========
                // Réplica del SOAP F_APLICAR_COMPLEMENTO.cs:797:
                //   CFDI_ID = _repCFDI.ExisteRrepositorio(UUID, con, tran);
                resultado.ultimoBloque = 2;
                int cfdiId = await BuscarCfdiIdPorUuidAsync(con.FBC, tx, c.UUID, ct)
                    .ConfigureAwait(false);

                if (cfdiId <= 0)
                {
                    // El SOAP también condiciona los siguientes pasos a (CFDI_ID > 0),
                    // pero NO falla; solo agrega "Ya tiene un CFD asociado". Aquí
                    // sí lo reportamos como error para que el Escritorio sepa que
                    // los adjuntos NO se asociaron — no es invisible.
                    resultado.tipo    = ResultadoAplicacionTipo.Error;
                    resultado.mensaje = "El UUID '" + c.UUID + "' no está en REPOSITORIO_CFDI; "
                                      + "no se pueden asociar los adjuntos extras.";
                    return resultado;
                }

                // === BLOQUE 3: adjuntos (réplica AsociaDocumentosAdjuntos) =======
                // Reusamos el helper InsertarAdjuntosComplementoAsync que ya
                // hace el INSERT con NOM_TABLA='DOCTOS_CP'.
                resultado.ultimoBloque = 3;
                var resAdj = await InsertarAdjuntosComplementoAsync(
                    con.FBC, tx, credito.DoctoCpId, adjuntos, ct
                ).ConfigureAwait(false);
                resultado.adjuntosInsertados = resAdj.Insertados;
                resultado.adjuntosOmitidos   = resAdj.Omitidos;

                // === BLOQUE 4: UPDATE REPOSITORIO_CFDI.TIPO_DOCTO_MSP ============
                // Réplica LITERAL del SOAP F_APLICAR_COMPLEMENTO.cs:251-253:
                //   UPDATE REPOSITORIO_CFDI SET TIPO_DOCTO_MSP = 'Pago'
                //    WHERE CFDI_ID = @CFDI_ID
                // El SOAP usa 'Pago' o 'Nota de crédito' según VERSION_PAGO, pero
                // el portal solo emite complementos de pago (tipo='C'), por lo
                // que el valor literal es siempre 'Pago' aquí (mismo valor que
                // BuscarOInsertarCfdiComplementoAsync ya escribe al insertar).
                resultado.ultimoBloque = 4;
                await ActualizarTipoDoctoMspAsync(con.FBC, tx, cfdiId, "Pago", ct)
                    .ConfigureAwait(false);

                // === BLOQUE 5: marcar portal (ACTUALIZAR_COMPLEMENTO_PORTAL) =====
                // Mismo callback que el flujo normal — el endpoint PHP es
                // idempotente (UPDATE ... SET ESTATUS='R' WHERE DOCTO_CP_ID=?),
                // así que volver a llamarlo no hace daño aunque AplicarComplementoAsync
                // ya lo haya marcado en el primer intento.
                resultado.ultimoBloque = 5;
                bool portalOk = false;
                if (marcarPortalAsync != null)
                {
                    portalOk = await marcarPortalAsync(c.CREDITO_FK).ConfigureAwait(false);
                }
                resultado.portalMarcado = portalOk;

                if (!portalOk)
                {
                    resultado.tipo    = ResultadoAplicacionTipo.Error;
                    resultado.mensaje = "El portal rechazó marcar el complemento como aplicado — rollback de Firebird.";
                    return resultado;
                }

                // === BLOQUE 6: COMMIT ============================================
                resultado.ultimoBloque = 6;
                tx.Commit();
                commitHecho = true;

                resultado.tipo    = ResultadoAplicacionTipo.OkDryRun; // semánticamente "OK"
                resultado.mensaje = "El crédito ya tenía CFDI asociado; se vincularon los adjuntos del complemento."
                                  + " DOCTO_CP_ID: " + credito.DoctoCpId
                                  + ". Adjuntos insertados: " + resAdj.Insertados
                                  + (resAdj.Omitidos > 0 ? " (omitidos: " + resAdj.Omitidos + ")" : "")
                                  + ". (Ya tiene un CFD asociado)";
                return resultado;
            }
            catch (Exception ex)
            {
                resultado.tipo    = ResultadoAplicacionTipo.Error;
                resultado.mensaje = "Excepción en bloque " + resultado.ultimoBloque + ": "
                                  + ex.GetType().Name + ": " + ex.Message;
                EventoLog.Error("AplicacionRepository (asociar ya con CFDI): bloque "
                              + resultado.ultimoBloque + " — " + ex);
                return resultado;
            }
            finally
            {
                if (tx != null)
                {
                    if (!commitHecho)
                    {
                        try { tx.Rollback(); } catch { }
                    }
                    tx.Dispose();
                }
                con.Desconectar();
            }
        }

        // ================================================================
        // Helpers para AsociarComplementoYaConCfdiAsync
        // ================================================================

        /// <summary>
        /// Réplica del SOAP <c>REPOSITORIO_CFDI.ExisteRrepositorio(UUID, ...)</c>
        /// (F_APLICAR_COMPLEMENTO.cs:155-160 / referenced :797): SELECT CFDI_ID
        /// FROM REPOSITORIO_CFDI WHERE UUID=? — devuelve 0 si no existe, -1 si
        /// hay error de SQL.
        /// </summary>
        private static async Task<int> BuscarCfdiIdPorUuidAsync(
            FbConnection con, FbTransaction tx, string uuid, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(uuid)) return 0;
            const string sql = "SELECT CFDI_ID FROM REPOSITORIO_CFDI WHERE UUID = @uuid";
            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@uuid", FbDbType.VarChar).Value = uuid;
                var obj = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                if (obj == null || obj == DBNull.Value) return 0;
                return Convert.ToInt32(obj);
            }
        }

        /// <summary>
        /// Réplica del SOAP <c>REPOSITORIO_CFDI.ActualizaTIPO_DOCTO_MSP</c>
        /// (F_APLICAR_COMPLEMENTO.cs:229-281): UPDATE REPOSITORIO_CFDI
        /// SET TIPO_DOCTO_MSP=? WHERE CFDI_ID=?. El SOAP además valida que
        /// el TIPO_DOCTO_MSP esté vacío antes de actualizar — aquí hacemos
        /// el UPDATE incondicional porque para complementos el valor es
        /// siempre el mismo ('Pago') y es idempotente.
        /// </summary>
        private static async Task ActualizarTipoDoctoMspAsync(
            FbConnection con, FbTransaction tx, int cfdiId, string tipoDoctoMsp, CancellationToken ct)
        {
            const string sql = "UPDATE REPOSITORIO_CFDI SET TIPO_DOCTO_MSP = @tipo WHERE CFDI_ID = @id";
            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@tipo", FbDbType.VarChar).Value = tipoDoctoMsp ?? "";
                cmd.Parameters.Add("@id",   FbDbType.Integer).Value = cfdiId;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // BLOQUE 1 (Complementos) — leer crédito DOCTOS_CP
        // ================================================================

        private static async Task<CreditoOrigen> LeerCreditoOrigenAsync(
            FbConnection con, FbTransaction tx, string folioCredito, int proveedorId,
            int conceptoCpId, CancellationToken ct)
        {
            // Réplica del WHERE completo del SOAP (F_APLICAR_COMPLEMENTO.cs:649-653):
            //   SELECT * FROM DOCTOS_CP
            //    WHERE FOLIO = @CreditoFolio
            //      AND NATURALEZA_CONCEPTO = 'R'
            //      AND PROVEEDOR_ID = @ProveedorID
            //      AND CONCEPTO_CP_ID = @ConceptoCpId
            // (NATURALEZA_CONCEPTO es columna directa de DOCTOS_CP — el legacy
            // no necesitaba JOIN a CONCEPTOS_CP). Sin estos filtros, un
            // proveedor con folios repetidos entre conceptos podía asociar el
            // pago al documento CxP equivocado. CONCEPTO_CP_ID viene del
            // portal (CREDITOS.CONCEPTO_CP_ID — el SOAP lo bajaba con
            // CargarCredito, services/creditos.php:382); si llega 0 (portal
            // sin la columna en la respuesta todavía) se omite ese filtro y
            // queda al menos NATURALEZA_CONCEPTO='R' como red de seguridad.
            string sql =
                "SELECT DOCTO_CP_ID, CLAVE_PROV, PROVEEDOR_ID, TIPO_CAMBIO, " +
                "       DESCRIPCION, COND_PAGO_ID, CONCEPTO_CP_ID, TIENE_CFD " +
                "  FROM DOCTOS_CP " +
                " WHERE FOLIO = @folio " +
                "   AND NATURALEZA_CONCEPTO = 'R' " +
                "   AND PROVEEDOR_ID = @prov";
            if (conceptoCpId > 0)
                sql += " AND CONCEPTO_CP_ID = @concepto";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@folio", FbDbType.VarChar).Value = folioCredito;
                cmd.Parameters.Add("@prov",  FbDbType.Integer).Value = proveedorId;
                if (conceptoCpId > 0)
                    cmd.Parameters.Add("@concepto", FbDbType.Integer).Value = conceptoCpId;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (!await rd.ReadAsync(ct).ConfigureAwait(false))
                        return null;

                    return new CreditoOrigen
                    {
                        DoctoCpId    = Convert.ToInt32(rd["DOCTO_CP_ID"]),
                        ClaveProv    = (Convert.ToString(rd["CLAVE_PROV"]) ?? "").Trim(),
                        ProveedorId  = ToInt(rd["PROVEEDOR_ID"]),
                        TipoCambio   = ToDecimal(rd["TIPO_CAMBIO"]),
                        Descripcion  = Convert.ToString(rd["DESCRIPCION"]) ?? "",
                        CondPagoId   = ToInt(rd["COND_PAGO_ID"]),
                        ConceptoCpId = ToInt(rd["CONCEPTO_CP_ID"]),
                        TieneCfd     = (Convert.ToString(rd["TIENE_CFD"]) ?? "").Trim(),
                    };
                }
            }
        }

        // ================================================================
        // BLOQUE 2 (Complementos) — buscar/insertar REPOSITORIO_CFDI
        // ================================================================

        /// <summary>
        /// Análogo a <see cref="BuscarOInsertarCfdiAsync"/> pero para
        /// complementos. Diferencias respecto a facturas:
        /// <list type="bullet">
        ///   <item><c>TIPO_COMPROBANTE = 'P'</c> (pago), no 'I' como Delphi
        ///     puso por copia-pega de facturas. Es lo correcto del SAT.</item>
        ///   <item><c>IMPORTE = MONTO</c> del complemento (no la suma compleja
        ///     de la factura).</item>
        ///   <item>No hay <c>FOLIO_XML</c> ni <c>FOLIO_COMPRA</c> separado;
        ///     usamos <c>FOLIO_PAGO</c> del complemento.</item>
        /// </list>
        /// </summary>
        private static async Task<ResultadoCfdi> BuscarOInsertarCfdiComplementoAsync(
            FbConnection con, FbTransaction tx,
            ComplementoAplicar c, CfdiXmlMicrosip cfdi, CancellationToken ct)
        {
            using (var cmd = new FbCommand(
                "SELECT CFDI_ID, XML FROM REPOSITORIO_CFDI WHERE UUID = @uuid",
                con, tx))
            {
                cmd.Parameters.Add("@uuid", FbDbType.VarChar).Value = c.UUID;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        return new ResultadoCfdi
                        {
                            CfdiId       = Convert.ToInt32(rd["CFDI_ID"]),
                            XmlPreparado = Convert.ToString(rd["XML"]) ?? "",
                            FueCreado    = false,
                        };
                    }
                }
            }

            if (cfdi == null || string.IsNullOrEmpty(cfdi.xml))
                throw new InvalidOperationException(
                    "REPOSITORIO_CFDI no tiene la UUID '" + c.UUID + "' pero el portal " +
                    "no devolvió el XML — no se puede insertar el CFDI del complemento.");

            string xmlPreparado = EncoderXmlMicrosip.PrepararParaMicrosip(cfdi.xml);
            int nuevoCfdiId = await GenDoctoIdAsync(con, tx, ct).ConfigureAwait(false);

            const string sqlInsert =
                "INSERT INTO REPOSITORIO_CFDI (" +
                "  CFDI_ID, MODALIDAD_FACTURACION, VERSION, UUID, NATURALEZA, TIPO_COMPROBANTE, " +
                "  TIPO_DOCTO_MSP, FOLIO, FECHA, RFC, NOMBRE, IMPORTE, MONEDA, TIPO_CAMBIO, " +
                "  ES_PARCIALIDAD, NOM_ARCH, XML, REFER_GRUPO, SELLO_VALIDADO, ES_SUSTITUTO, " +
                "  USUARIO_CREADOR, FECHA_HORA_CREACION, LUGAR_EXPEDICION, USO_CFDI" +
                ") VALUES (" +
                // 'P' (Pago) en lugar de 'I' (Ingreso) — diferimos del Delphi
                // que ponía 'I' por copia-pega de facturas. 'P' es lo correcto
                // del SAT para complementos.
                "  @id, 'CFDI', '4.0', @uuid, 'R', 'P', " +
                "  'Pago', @folio, @fecha, @rfc, @nombre, @importe, @moneda, 1, " +
                "  'N', @nomArch, @xml, @refGrupo, 'M', 'N', " +
                "  'SISTEMAWEB', @fechaCrea, '', @uso" +
                ")";

            using (var cmd = new FbCommand(sqlInsert, con, tx))
            {
                cmd.Parameters.Add("@id",        FbDbType.Integer).Value   = nuevoCfdiId;
                cmd.Parameters.Add("@uuid",      FbDbType.VarChar).Value   = c.UUID ?? "";
                cmd.Parameters.Add("@folio",     FbDbType.VarChar).Value   = c.FOLIO_PAGO ?? "";
                cmd.Parameters.Add("@fecha",     FbDbType.TimeStamp).Value = ParsearFecha(c.FECHA_COMPLEMENTO);
                cmd.Parameters.Add("@rfc",       FbDbType.VarChar).Value   = c.RFC ?? "";
                cmd.Parameters.Add("@nombre",    FbDbType.VarChar).Value   = c.NOMBRE ?? "";
                cmd.Parameters.Add("@importe",   FbDbType.Double).Value    = (double) c.MONTO;
                cmd.Parameters.Add("@moneda",    FbDbType.VarChar).Value   = c.MONEDA_PAGO ?? "";
                cmd.Parameters.Add("@nomArch",   FbDbType.VarChar).Value   = (c.RFC ?? "") + "_" + (c.FOLIO_PAGO ?? "") + ".xml";
                cmd.Parameters.Add("@xml",       FbDbType.Text).Value      = xmlPreparado ?? "";
                cmd.Parameters.Add("@refGrupo",  FbDbType.VarChar).Value   = c.FOLIO_PAGO ?? "";
                cmd.Parameters.Add("@fechaCrea", FbDbType.TimeStamp).Value = DateTime.Now;
                cmd.Parameters.Add("@uso",       FbDbType.VarChar).Value   = (object) (c.USO_CFDI ?? cfdi.uso_cfdi ?? "") ?? DBNull.Value;

                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            return new ResultadoCfdi
            {
                CfdiId       = nuevoCfdiId,
                XmlPreparado = xmlPreparado,
                FueCreado    = true,
            };
        }

        // ================================================================
        // BLOQUE 3 (Complementos) — INSERT CFD_RECIBIDOS con CLAVE_SISTEMA='CP'
        // ================================================================

        private static async Task InsertarCfdRecibidoCpAsync(
            FbConnection con, FbTransaction tx,
            int doctoCpId, int cfdiId, DateTime fecha, string xml,
            CancellationToken ct)
        {
            int cfdRecibidoId = await GenDoctoIdAsync(con, tx, ct).ConfigureAwait(false);

            const string sql =
                "INSERT INTO CFD_RECIBIDOS (CFD_RECIBIDO_ID, CLAVE_SISTEMA, DOCTO_ID, FECHA, XML, CFDI_ID) " +
                "VALUES (@id, 'CP', @docto, @fecha, @xml, @cfdi)";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@id",    FbDbType.Integer).Value   = cfdRecibidoId;
                cmd.Parameters.Add("@docto", FbDbType.Integer).Value   = doctoCpId;
                cmd.Parameters.Add("@fecha", FbDbType.TimeStamp).Value = fecha;
                cmd.Parameters.Add("@xml",   FbDbType.Text).Value      = xml ?? "";
                cmd.Parameters.Add("@cfdi",  FbDbType.Integer).Value   = cfdiId;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // BLOQUE 4 (Complementos) — UPDATE DOCTOS_CP SET TIENE_CFD='S'
        // ================================================================

        private static async Task MarcarCreditoConCfdiAsync(
            FbConnection con, FbTransaction tx, int doctoCpId, CancellationToken ct)
        {
            const string sql = "UPDATE DOCTOS_CP SET TIENE_CFD = 'S' WHERE DOCTO_CP_ID = @id";
            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@id", FbDbType.Integer).Value = doctoCpId;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // BLOQUE 5 (Complementos) — adjuntos con NOM_TABLA='DOCTOS_CP'
        // ================================================================

        private static async Task<ResultadoAdjuntos> InsertarAdjuntosComplementoAsync(
            FbConnection con, FbTransaction tx,
            int doctoCpId, AdjuntoDescargado[] adjuntos, CancellationToken ct)
        {
            var resumen = new ResultadoAdjuntos();
            if (adjuntos == null || adjuntos.Length == 0) return resumen;

            foreach (var adj in adjuntos)
            {
                ct.ThrowIfCancellationRequested();
                if (adj == null || adj.Contenido == null || adj.Contenido.Length == 0)
                {
                    resumen.Omitidos++;
                    continue;
                }

                try
                {
                    var zipBytes = ComprimirEnZip(adj.NombreOriginal, adj.Contenido);
                    int idAdjunto = await GenDoctoIdAsync(con, tx, ct).ConfigureAwait(false);

                    const string sql =
                        "INSERT INTO ARCHIVOS_ADJUNTOS (" +
                        "  ARCHIVO_ADJUNTO_ID, NOM_TABLA, ELEM_ID, FILE_NAME, FILE_SIZE, FILE_DATE, FILE_STREAM" +
                        ") VALUES (" +
                        "  @id, 'DOCTOS_CP', @elem, @name, @size, @fecha, @stream" +
                        ")";

                    using (var cmd = new FbCommand(sql, con, tx))
                    {
                        cmd.Parameters.Add("@id",     FbDbType.Integer).Value   = idAdjunto;
                        cmd.Parameters.Add("@elem",   FbDbType.Integer).Value   = doctoCpId;
                        cmd.Parameters.Add("@name",   FbDbType.VarChar).Value   = TruncarA(adj.NombreOriginal ?? "", 100);
                        cmd.Parameters.Add("@size",   FbDbType.Integer).Value   = adj.Contenido.Length / 1024;
                        cmd.Parameters.Add("@fecha",  FbDbType.TimeStamp).Value = DateTime.Now;
                        cmd.Parameters.Add("@stream", FbDbType.Binary).Value    = zipBytes;
                        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    }

                    resumen.Insertados++;
                }
                catch (Exception ex)
                {
                    EventoLog.Warning("Aplicación · adjunto complemento id=" + adj.Id
                        + " omitido: " + ex.GetType().Name + ": " + ex.Message);
                    resumen.Omitidos++;
                }
            }

            return resumen;
        }

        private sealed class CreditoOrigen
        {
            public int     DoctoCpId    { get; set; }
            public string  ClaveProv    { get; set; }
            public int     ProveedorId  { get; set; }
            public decimal TipoCambio   { get; set; }
            public string  Descripcion  { get; set; }
            public int     CondPagoId   { get; set; }
            public int     ConceptoCpId { get; set; }
            public string  TieneCfd     { get; set; }
        }

        // ================================================================
        // Método de validación — MODO DRY-RUN (mantiene compat con 2.2/2.3)
        // ================================================================

        public async Task<ResultadoAplicacionDryRun> AplicarFacturaDryRunAsync(
            string nombreEmpresaMicrosip, FacturaAplicar factura, CfdiXmlMicrosip cfdi, CancellationToken ct)
        {
            var resultado = new ResultadoAplicacionDryRun
            {
                tipo         = ResultadoAplicacionTipo.Error,
                ultimoBloque = 0,
                mensaje      = "no se pudo arrancar",
            };

            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreEmpresaMicrosip))
            {
                resultado.tipo    = ResultadoAplicacionTipo.ErrorConexion;
                resultado.mensaje = "No se pudo abrir Firebird de '" + nombreEmpresaMicrosip + "'.";
                return resultado;
            }

            FbTransaction tx = null;

            try
            {
                tx = con.FBC.BeginTransaction(IsolationLevel.ReadCommitted);

                var ejecucion = await EjecutarBloques1A11Async(
                    con.FBC, tx, factura, cfdi, ct
                ).ConfigureAwait(false);

                resultado.ultimoBloque        = ejecucion.UltimoBloque;
                resultado.folioFinalGenerado  = ejecucion.FolioFinal;
                resultado.renglonesDetalle    = ejecucion.RenglonesDetalle;
                resultado.filasImpuestos      = ejecucion.FilasImpuestos;
                resultado.cfdiCreado          = ejecucion.CfdiCreado;
                resultado.tipo                = ejecucion.Tipo;
                resultado.mensaje             = ejecucion.Tipo == ResultadoAplicacionTipo.OkDryRun
                    ? "OK (dry-run): bloques 1-11 ejecutados sin error. " +
                      "Folio que se asignaría: " + ejecucion.FolioFinal + ". " +
                      "Renglones detalle: " + ejecucion.RenglonesDetalle + ". " +
                      "Filas impuestos: " + ejecucion.FilasImpuestos + ". " +
                      "CFDI: " + (ejecucion.CfdiCreado ? "creado" : "ya existía") + "."
                    : ejecucion.Mensaje;

                return resultado;
            }
            catch (Exception ex)
            {
                resultado.tipo    = ResultadoAplicacionTipo.Error;
                resultado.mensaje = "Excepción en bloque " + resultado.ultimoBloque + ": "
                                  + ex.GetType().Name + ": " + ex.Message;
                EventoLog.Error("AplicacionRepository (dry-run): bloque " + resultado.ultimoBloque + " — " + ex);
                return resultado;
            }
            finally
            {
                // DRY-RUN: rollback siempre, no importa si todo salió bien.
                if (tx != null)
                {
                    try { tx.Rollback(); } catch { }
                    tx.Dispose();
                }
                con.Desconectar();
            }
        }

        // ================================================================
        // Núcleo compartido — bloques 1-11
        // ================================================================

        /// <summary>
        /// Núcleo común a dry-run y producción. Ejecuta los 11 bloques de
        /// preparación dentro de la transacción ya abierta, sin hacer commit
        /// ni rollback (eso lo decide el llamador).
        /// </summary>
        private static async Task<EjecucionBloques> EjecutarBloques1A11Async(
            FbConnection con, FbTransaction tx,
            FacturaAplicar factura, CfdiXmlMicrosip cfdi, CancellationToken ct)
        {
            var e = new EjecucionBloques();

            // === BLOQUE 1: SELECT DOCTOS_CM origen ============================
            e.UltimoBloque = 1;
            var recepcionOrigen = await LeerRecepcionOrigenAsync(
                con, tx, factura.FOLIO_RECEPCION, factura.PROVEEDOR_ID, ct
            ).ConfigureAwait(false);

            if (recepcionOrigen == null)
            {
                e.Tipo    = ResultadoAplicacionTipo.RecepcionNoExiste;
                e.Mensaje = "No existe la recepción " + factura.FOLIO_RECEPCION
                          + " del proveedor " + factura.PROVEEDOR_ID + " en Microsip.";
                return e;
            }

            // Réplica del SOAP F_APLICAR_FACTURA.cs:155-186 — si la recepción
            // origen en Microsip está cancelada (ESTATUS='C'), el orquestador
            // deberá marcar la factura como rechazada en el portal y avisar
            // al operador para que el proveedor la suba de nuevo.
            if (string.Equals(recepcionOrigen.Estatus, "C", StringComparison.OrdinalIgnoreCase))
            {
                e.Tipo    = ResultadoAplicacionTipo.RecepcionCancelada;
                e.Mensaje = "La recepción " + factura.FOLIO_RECEPCION
                          + " está CANCELADA en Microsip (ESTATUS='C') — la factura debe ser rechazada en el portal.";
                return e;
            }

            if (string.Equals(recepcionOrigen.Estatus, "F", StringComparison.OrdinalIgnoreCase))
            {
                // Réplica del SOAP F_APLICAR_FACTURA.cs:190-231 — la recepción
                // ya tiene su compra hecha en Microsip; el portal estaba
                // desincronizado. Buscamos la compra ya ligada vía
                // DOCTOS_CM_LIGAS (FTE = recepción, DEST = compra) y, si la
                // encontramos, devolvemos RecepcionYaFacturadaSincronizar
                // con folio + DOCTO_CM_ID poblados para que el orquestador
                // llame al endpoint factura-ya-aplicada-sincronizar.
                var compraLigada = await BuscarCompraLigadaAsync(
                    con, tx, recepcionOrigen.DoctoCmId, ct
                ).ConfigureAwait(false);

                if (compraLigada != null)
                {
                    e.NuevoDoctoCmId = compraLigada.Item1; // DOCTO_CM_ID de la compra existente
                    e.FolioFinal     = compraLigada.Item2; // FOLIO de la compra existente
                    e.Tipo    = ResultadoAplicacionTipo.RecepcionYaFacturadaSincronizar;
                    e.Mensaje = "La recepción " + factura.FOLIO_RECEPCION
                              + " ya está facturada en Microsip (ESTATUS='F'); "
                              + "compra ligada DOCTO_CM_ID=" + compraLigada.Item1
                              + ", FOLIO=" + compraLigada.Item2 + ".";
                    return e;
                }

                // Caso raro de data corrupta: ESTATUS='F' pero no encontramos
                // la liga. Mantener el comportamiento original.
                e.Tipo    = ResultadoAplicacionTipo.RecepcionYaFacturada;
                e.Mensaje = "La recepción " + factura.FOLIO_RECEPCION
                          + " ya está facturada (ESTATUS='F') pero no se encontró "
                          + "la compra ligada en DOCTOS_CM_LIGAS — revisar manualmente.";
                return e;
            }

            // === BLOQUE 2: validar FOLIO_COMPRA duplicado =====================
            e.UltimoBloque = 2;
            if (!string.IsNullOrEmpty(factura.FOLIO_COMPRA)
                && factura.FOLIO_COMPRA != "000000000")
            {
                if (await ExisteCompraConFolioProvAsync(
                    con, tx, factura.FOLIO_COMPRA, factura.PROVEEDOR_ID, ct
                ).ConfigureAwait(false))
                {
                    e.Tipo    = ResultadoAplicacionTipo.FolioCompraDuplicado;
                    e.Mensaje = "Ya hay una compra registrada con FOLIO_PROV='" + factura.FOLIO_COMPRA
                              + "' del proveedor " + factura.PROVEEDOR_ID + ".";
                    return e;
                }
            }

            // === BLOQUE 3: SIGUIENTE_FOLIO('WEB') ==============================
            e.UltimoBloque = 3;
            var folioFinal = await SiguienteFolioWebAsync(con, tx, ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(folioFinal))
            {
                e.Tipo    = ResultadoAplicacionTipo.SerieWebNoConfigurada;
                e.Mensaje = "La serie 'WEB' no está registrada en FOLIOS_COMPRAS de esta empresa.";
                return e;
            }
            e.FolioFinal = folioFinal;

            // FOLIO_COMPRA == '000000000' → el proveedor no tenía folio propio,
            // se reusa el folio WEB y el folio del CFDI queda vacío.
            string folioXml;
            string folioCompra = factura.FOLIO_COMPRA;
            if (string.IsNullOrEmpty(folioCompra) || folioCompra == "000000000")
            {
                folioCompra = folioFinal;
                folioXml    = "";
            }
            else
            {
                folioXml = folioCompra;
            }

            // === BLOQUE 4: INSERT DOCTOS_CM ====================================
            e.UltimoBloque = 4;
            int nuevoDoctoCmId = await GenDoctoIdAsync(con, tx, ct).ConfigureAwait(false);
            e.NuevoDoctoCmId = nuevoDoctoCmId;

            await InsertarDoctosCmAsync(
                con, tx, nuevoDoctoCmId, recepcionOrigen, factura,
                folioFinal, folioCompra, ct
            ).ConfigureAwait(false);

            // === BLOQUE 5: INSERT DOCTOS_CM_LIGAS =============================
            e.UltimoBloque = 5;
            int nuevoLigaId = await GenDoctoIdAsync(con, tx, ct).ConfigureAwait(false);
            await InsertarDoctosCmLigasAsync(
                con, tx, nuevoLigaId, recepcionOrigen.DoctoCmId, nuevoDoctoCmId, ct
            ).ConfigureAwait(false);

            // === BLOQUE 6: loop DOCTOS_CM_DET + DOCTOS_CM_LIGAS_DET ===========
            e.UltimoBloque = 6;
            e.RenglonesDetalle = await CopiarDetalleAsync(
                con, tx, recepcionOrigen.DoctoCmId, nuevoDoctoCmId, nuevoLigaId, ct
            ).ConfigureAwait(false);

            // === BLOQUE 7: loop IMPUESTOS_DOCTOS_CM ===========================
            e.UltimoBloque = 7;
            e.FilasImpuestos = await CopiarImpuestosAsync(
                con, tx, recepcionOrigen.DoctoCmId, nuevoDoctoCmId, ct
            ).ConfigureAwait(false);

            // === BLOQUE 8: SELECT/INSERT REPOSITORIO_CFDI ====================
            e.UltimoBloque = 8;
            var cfdiResult = await BuscarOInsertarCfdiAsync(
                con, tx, factura, cfdi, folioCompra, folioXml, ct
            ).ConfigureAwait(false);
            e.CfdiCreado = cfdiResult.FueCreado;

            // === BLOQUE 9: INSERT CFD_RECIBIDOS ==============================
            e.UltimoBloque = 9;
            await InsertarCfdRecibidoAsync(
                con, tx, nuevoDoctoCmId, cfdiResult.CfdiId,
                ParsearFecha(factura.FECHA_FACTURA), cfdiResult.XmlPreparado, ct
            ).ConfigureAwait(false);

            // === BLOQUE 10: INSERT VENCIMIENTOS_CARGOS_CM ====================
            e.UltimoBloque = 10;
            await InsertarVencimientoCargoCmAsync(
                con, tx, nuevoDoctoCmId, ParsearFecha(factura.FECHA_PAGO), ct
            ).ConfigureAwait(false);

            // === BLOQUE 11: EXEC stored proc GENERA_DOCTO_CP_CM ==============
            e.UltimoBloque = 11;
            await EjecutarGeneraDoctoCpCmAsync(con, tx, nuevoDoctoCmId, ct).ConfigureAwait(false);

            e.Tipo = ResultadoAplicacionTipo.OkDryRun;
            return e;
        }

        private sealed class EjecucionBloques
        {
            public ResultadoAplicacionTipo Tipo { get; set; } = ResultadoAplicacionTipo.Error;
            public int    UltimoBloque     { get; set; }
            public string FolioFinal       { get; set; }
            public int    NuevoDoctoCmId   { get; set; }
            public int    RenglonesDetalle { get; set; }
            public int    FilasImpuestos   { get; set; }
            public bool   CfdiCreado       { get; set; }
            public string Mensaje          { get; set; }
        }

        // ================================================================
        // BLOQUE 1 — leer recepción origen
        // ================================================================

        /// <summary>
        /// Lee la cabecera de la recepción Microsip por FOLIO + PROVEEDOR + TIPO='R'.
        /// Devuelve null si no existe.
        /// </summary>
        private static async Task<RecepcionOrigen> LeerRecepcionOrigenAsync(
            FbConnection con, FbTransaction tx, string folioRecepcion, int proveedorId, CancellationToken ct)
        {
            const string sql =
                "SELECT DOCTO_CM_ID, ESTATUS, SUBTIPO_DOCTO, SUCURSAL_ID, CLAVE_PROV, PROVEEDOR_ID, " +
                "       ALMACEN_ID, MONEDA_ID, TIPO_CAMBIO, TIPO_DSCTO, DSCTO_PCTJE, DSCTO_IMPORTE, " +
                "       DESCRIPCION, IMPORTE_NETO, FLETES, OTROS_CARGOS, TOTAL_IMPUESTOS, TOTAL_RETENCIONES, " +
                "       GASTOS_ADUANALES, OTROS_GASTOS, COND_PAGO_ID, CARGAR_SUN " +
                "  FROM DOCTOS_CM " +
                " WHERE FOLIO = @folio " +
                "   AND PROVEEDOR_ID = @prov " +
                "   AND TIPO_DOCTO = 'R'";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@folio", FbDbType.VarChar).Value = folioRecepcion;
                cmd.Parameters.Add("@prov",  FbDbType.Integer).Value = proveedorId;

                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (!await rd.ReadAsync(ct).ConfigureAwait(false))
                        return null;

                    return new RecepcionOrigen
                    {
                        DoctoCmId         = Convert.ToInt32(rd["DOCTO_CM_ID"]),
                        Estatus           = (Convert.ToString(rd["ESTATUS"]) ?? "").Trim(),
                        SubtipoDocto      = (Convert.ToString(rd["SUBTIPO_DOCTO"]) ?? "").Trim(),
                        SucursalId        = ToInt(rd["SUCURSAL_ID"]),
                        ClaveProv         = (Convert.ToString(rd["CLAVE_PROV"]) ?? "").Trim(),
                        ProveedorId       = ToInt(rd["PROVEEDOR_ID"]),
                        AlmacenId         = ToInt(rd["ALMACEN_ID"]),
                        MonedaId          = ToInt(rd["MONEDA_ID"]),
                        TipoCambio        = ToDecimal(rd["TIPO_CAMBIO"]),
                        TipoDscto         = (Convert.ToString(rd["TIPO_DSCTO"]) ?? "").Trim(),
                        DsctoPctje        = ToDecimal(rd["DSCTO_PCTJE"]),
                        DsctoImporte      = ToDecimal(rd["DSCTO_IMPORTE"]),
                        Descripcion       = Convert.ToString(rd["DESCRIPCION"]) ?? "",
                        ImporteNeto       = ToDecimal(rd["IMPORTE_NETO"]),
                        Fletes            = ToDecimal(rd["FLETES"]),
                        OtrosCargos       = ToDecimal(rd["OTROS_CARGOS"]),
                        TotalImpuestos    = ToDecimal(rd["TOTAL_IMPUESTOS"]),
                        TotalRetenciones  = ToDecimal(rd["TOTAL_RETENCIONES"]),
                        GastosAduanales   = ToDecimal(rd["GASTOS_ADUANALES"]),
                        OtrosGastos       = ToDecimal(rd["OTROS_GASTOS"]),
                        CondPagoId        = ToInt(rd["COND_PAGO_ID"]),
                        CargarSun         = (Convert.ToString(rd["CARGAR_SUN"]) ?? "").Trim(),
                    };
                }
            }
        }

        // ================================================================
        // BLOQUE 1 (bis) — buscar la compra ya ligada cuando ESTATUS='F'
        // ================================================================

        /// <summary>
        /// Réplica LITERAL del SELECT del SOAP F_APLICAR_FACTURA.cs:196-198
        /// (legacy):
        ///
        ///   SELECT * FROM doctos_cm_ligas d
        ///     JOIN doctos_cm dc ON ( d.docto_cm_dest_id = dc.docto_cm_id )
        ///    WHERE d.docto_cm_fte_id = &lt;recepcion_docto_cm_id&gt;
        ///
        /// La liga tiene FTE = recepción (origen) y DEST = compra (destino).
        /// Devuelve (DOCTO_CM_ID, FOLIO) de la compra ya creada, o null si no
        /// existe la liga (caso raro de data corrupta cuando ESTATUS='F' pero
        /// la liga no se materializó).
        /// </summary>
        private static async Task<Tuple<int, string>> BuscarCompraLigadaAsync(
            FbConnection con, FbTransaction tx, int doctoCmRecepcionId, CancellationToken ct)
        {
            const string sql =
                "SELECT dc.DOCTO_CM_ID, dc.FOLIO " +
                "  FROM DOCTOS_CM_LIGAS d " +
                "  JOIN DOCTOS_CM dc ON ( d.DOCTO_CM_DEST_ID = dc.DOCTO_CM_ID ) " +
                " WHERE d.DOCTO_CM_FTE_ID = @fte";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@fte", FbDbType.Integer).Value = doctoCmRecepcionId;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (!await rd.ReadAsync(ct).ConfigureAwait(false))
                        return null;

                    int    doctoCmId   = Convert.ToInt32(rd["DOCTO_CM_ID"]);
                    string folioCompra = (Convert.ToString(rd["FOLIO"]) ?? "").Trim();
                    return Tuple.Create(doctoCmId, folioCompra);
                }
            }
        }

        // ================================================================
        // BLOQUE 2 — validar FOLIO_PROV duplicado
        // ================================================================

        private static async Task<bool> ExisteCompraConFolioProvAsync(
            FbConnection con, FbTransaction tx, string folioCompra, int proveedorId, CancellationToken ct)
        {
            const string sql =
                "SELECT FIRST 1 DOCTO_CM_ID FROM DOCTOS_CM " +
                " WHERE FOLIO_PROV = @folio " +
                "   AND PROVEEDOR_ID = @prov " +
                "   AND TIPO_DOCTO = 'C'";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@folio", FbDbType.VarChar).Value = folioCompra;
                cmd.Parameters.Add("@prov",  FbDbType.Integer).Value = proveedorId;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                    return await rd.ReadAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // BLOQUE 3 — SIGUIENTE_FOLIO('WEB')
        // ================================================================

        /// <summary>
        /// Réplica de <c>SIGUIENTE_FOLIO('WEB')</c> de Func.pas:329-362.
        /// Lee CONSECUTIVO de FOLIOS_COMPRAS, lo incrementa, hace UPDATE, y
        /// devuelve "WEB" + pad-left 6 (ej. "WEB000123"). Si no existe la
        /// serie 'WEB' devuelve null.
        /// </summary>
        private static async Task<string> SiguienteFolioWebAsync(
            FbConnection con, FbTransaction tx, CancellationToken ct)
        {
            int consecutivo;
            using (var cmd = new FbCommand(
                "SELECT CONSECUTIVO FROM FOLIOS_COMPRAS WHERE SERIE = 'WEB'",
                con, tx))
            {
                var raw = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                if (raw == null || raw == DBNull.Value) return null;
                consecutivo = Convert.ToInt32(raw);
            }

            consecutivo++;

            using (var cmd = new FbCommand(
                "UPDATE FOLIOS_COMPRAS SET CONSECUTIVO = @c WHERE SERIE = 'WEB'",
                con, tx))
            {
                cmd.Parameters.Add("@c", FbDbType.Integer).Value = consecutivo;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            return "WEB" + consecutivo.ToString("D6");
        }

        // ================================================================
        // GEN_DOCTO_ID — stored procedure de Microsip
        // ================================================================

        /// <summary>
        /// Invoca el stored proc <c>GEN_DOCTO_ID</c> de Microsip y devuelve el
        /// nuevo ID. Es lo que usa el Delphi para asignar ID a DOCTOS_CM,
        /// DOCTOS_CM_DET, DOCTOS_CM_LIGAS, REPOSITORIO_CFDI, etc.
        /// </summary>
        private static async Task<int> GenDoctoIdAsync(FbConnection con, FbTransaction tx, CancellationToken ct)
        {
            using (var cmd = new FbCommand("GEN_DOCTO_ID", con, tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var raw = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                if (raw == null || raw == DBNull.Value)
                    throw new InvalidOperationException("GEN_DOCTO_ID devolvió null.");
                return Convert.ToInt32(raw);
            }
        }

        // ================================================================
        // BLOQUE 4 — INSERT DOCTOS_CM (encabezado tipo 'C')
        // ================================================================

        /// <summary>
        /// INSERT en DOCTOS_CM con TIPO_DOCTO='C'. Réplica literal del Delphi
        /// (Func_Facturas_3_3.pas:509-580). Campos hardcodeados:
        /// <list type="bullet">
        ///   <item><c>ESTATUS='N'</c> (Normal, no facturada)</item>
        ///   <item><c>APLICADO='S'</c></item>
        ///   <item><c>SISTEMA_ORIGEN='CM'</c></item>
        ///   <item><c>TIENE_CFD='S'</c></item>
        ///   <item><c>USUARIO_CREADOR='SISTEMAWEB'</c> + USUARIO_AUT_*='SYSDBA'/'SISTEMAWEB'</item>
        /// </list>
        /// </summary>
        private static async Task InsertarDoctosCmAsync(
            FbConnection con, FbTransaction tx,
            int nuevoDoctoCmId, RecepcionOrigen origen, FacturaAplicar factura,
            string folioFinal, string folioCompra, CancellationToken ct)
        {
            const string sql =
                "INSERT INTO DOCTOS_CM (" +
                "  DOCTO_CM_ID, TIPO_DOCTO, SUBTIPO_DOCTO, SUCURSAL_ID, FOLIO, FECHA, CLAVE_PROV, " +
                "  PROVEEDOR_ID, FOLIO_PROV, FACTURA_DEV, ALMACEN_ID, MONEDA_ID, TIPO_CAMBIO, " +
                "  TIPO_DSCTO, DSCTO_PCTJE, DSCTO_IMPORTE, ESTATUS, APLICADO, DESCRIPCION, " +
                "  IMPORTE_NETO, FLETES, OTROS_CARGOS, TOTAL_IMPUESTOS, TOTAL_RETENCIONES, " +
                "  GASTOS_ADUANALES, OTROS_GASTOS, FORMA_EMITIDA, CONTABILIZADO, " +
                "  ACREDITAR_CXP, SISTEMA_ORIGEN, COND_PAGO_ID, PCTJE_DSCTO_PPAG, CARGAR_SUN, ENVIADO, " +
                "  TIENE_CFD, USUARIO_CREADOR, USUARIO_AUT_CREACION, USUARIO_ULT_MODIF, USUARIO_AUT_MODIF" +
                ") VALUES (" +
                "  @docto, 'C', @subtipo, @sucursal, @folio, @fecha, @claveProv, " +
                "  @proveedor, @folioProv, '', @almacen, @moneda, @tipoCambio, " +
                "  @tipoDscto, @dsctoPctje, @dsctoImporte, 'N', 'S', @descripcion, " +
                "  @importeNeto, @fletes, @otrosCargos, @totalImp, @totalRet, " +
                "  @gastosAd, @otrosGastos, 'N', 'N', " +
                "  'N', 'CM', @condPago, 0, @cargarSun, 'N', " +
                "  'S', 'SISTEMAWEB', 'SYSDBA', 'SISTEMAWEB', 'SISTEMAWEB'" +
                ")";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@docto",        FbDbType.Integer).Value   = nuevoDoctoCmId;
                cmd.Parameters.Add("@subtipo",      FbDbType.VarChar).Value   = (object) origen.SubtipoDocto ?? DBNull.Value;
                cmd.Parameters.Add("@sucursal",     FbDbType.Integer).Value   = origen.SucursalId;
                cmd.Parameters.Add("@folio",        FbDbType.VarChar).Value   = folioFinal;
                cmd.Parameters.Add("@fecha",        FbDbType.TimeStamp).Value = ParsearFecha(factura.FECHA);
                cmd.Parameters.Add("@claveProv",    FbDbType.VarChar).Value   = (object) origen.ClaveProv ?? DBNull.Value;
                cmd.Parameters.Add("@proveedor",    FbDbType.Integer).Value   = origen.ProveedorId;
                cmd.Parameters.Add("@folioProv",    FbDbType.VarChar).Value   = folioCompra ?? "";
                cmd.Parameters.Add("@almacen",      FbDbType.Integer).Value   = origen.AlmacenId;
                cmd.Parameters.Add("@moneda",       FbDbType.Integer).Value   = origen.MonedaId;
                cmd.Parameters.Add("@tipoCambio",   FbDbType.Double).Value    = (double) origen.TipoCambio;
                cmd.Parameters.Add("@tipoDscto",    FbDbType.VarChar).Value   = (object) origen.TipoDscto ?? DBNull.Value;
                cmd.Parameters.Add("@dsctoPctje",   FbDbType.Double).Value    = (double) origen.DsctoPctje;
                cmd.Parameters.Add("@dsctoImporte", FbDbType.Double).Value    = (double) origen.DsctoImporte;
                cmd.Parameters.Add("@descripcion",  FbDbType.VarChar).Value   = (object) origen.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@importeNeto",  FbDbType.Double).Value    = (double) origen.ImporteNeto;
                cmd.Parameters.Add("@fletes",       FbDbType.Double).Value    = (double) origen.Fletes;
                cmd.Parameters.Add("@otrosCargos",  FbDbType.Double).Value    = (double) origen.OtrosCargos;
                cmd.Parameters.Add("@totalImp",     FbDbType.Double).Value    = (double) origen.TotalImpuestos;
                cmd.Parameters.Add("@totalRet",     FbDbType.Double).Value    = (double) origen.TotalRetenciones;
                cmd.Parameters.Add("@gastosAd",     FbDbType.Double).Value    = (double) origen.GastosAduanales;
                cmd.Parameters.Add("@otrosGastos",  FbDbType.Double).Value    = (double) origen.OtrosGastos;
                cmd.Parameters.Add("@condPago",     FbDbType.Integer).Value   = origen.CondPagoId;
                cmd.Parameters.Add("@cargarSun",    FbDbType.VarChar).Value   = (object) origen.CargarSun ?? DBNull.Value;

                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // BLOQUE 5 — INSERT DOCTOS_CM_LIGAS
        // ================================================================

        private static async Task InsertarDoctosCmLigasAsync(
            FbConnection con, FbTransaction tx,
            int doctoCmLigaId, int fteId, int destId, CancellationToken ct)
        {
            const string sql =
                "INSERT INTO DOCTOS_CM_LIGAS (DOCTO_CM_LIGA_ID, DOCTO_CM_FTE_ID, DOCTO_CM_DEST_ID) " +
                "VALUES (@liga, @fte, @dest)";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@liga", FbDbType.Integer).Value = doctoCmLigaId;
                cmd.Parameters.Add("@fte",  FbDbType.Integer).Value = fteId;
                cmd.Parameters.Add("@dest", FbDbType.Integer).Value = destId;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // BLOQUE 6 — copia DOCTOS_CM_DET + DOCTOS_CM_LIGAS_DET
        // ================================================================

        /// <summary>
        /// Por cada renglón del detalle de la recepción origen:
        ///   1. GEN_DOCTO_ID para nuevo DOCTO_CM_DET_ID
        ///   2. INSERT DOCTOS_CM_DET con datos copiados
        ///   3. INSERT DOCTOS_CM_LIGAS_DET (liga detalle origen → destino)
        /// </summary>
        private static async Task<int> CopiarDetalleAsync(
            FbConnection con, FbTransaction tx,
            int doctoCmOrigenId, int doctoCmDestinoId, int doctoCmLigaId,
            CancellationToken ct)
        {
            // 1) Leemos TODO el detalle origen a memoria — eso libera el reader
            // antes de invocar GEN_DOCTO_ID (no podemos tener 2 readers abiertos
            // simultáneamente en la misma conexión Firebird).
            var renglones = new System.Collections.Generic.List<RenglonDetalleOrigen>();
            const string sqlLeer =
                "SELECT DOCTO_CM_DET_ID, CLAVE_ARTICULO, ARTICULO_ID, UNIDADES, UNIDADES_REC_DEV, UNIDADES_A_REC, " +
                "       UMED, CONTENIDO_UMED, PRECIO_UNITARIO, PCTJE_DSCTO, PCTJE_DSCTO_PRO, PCTJE_DSCTO_VOL, " +
                "       PCTJE_DSCTO_PROMO, DSCTO_ART, DSCTO_EXTRA, PRECIO_TOTAL_NETO, PCTJE_ARANCEL, NOTAS, POSICION " +
                "  FROM DOCTOS_CM_DET " +
                " WHERE DOCTO_CM_ID = @docto " +
                " ORDER BY POSICION";

            using (var cmd = new FbCommand(sqlLeer, con, tx))
            {
                cmd.Parameters.Add("@docto", FbDbType.Integer).Value = doctoCmOrigenId;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        renglones.Add(new RenglonDetalleOrigen
                        {
                            DoctoCmDetId    = Convert.ToInt32(rd["DOCTO_CM_DET_ID"]),
                            ClaveArticulo   = (Convert.ToString(rd["CLAVE_ARTICULO"]) ?? "").Trim(),
                            ArticuloId      = ToInt(rd["ARTICULO_ID"]),
                            Unidades        = ToDecimal(rd["UNIDADES"]),
                            UnidadesRecDev  = ToDecimal(rd["UNIDADES_REC_DEV"]),
                            UnidadesARec    = ToDecimal(rd["UNIDADES_A_REC"]),
                            Umed            = (Convert.ToString(rd["UMED"]) ?? "").Trim(),
                            ContenidoUmed   = ToDecimal(rd["CONTENIDO_UMED"]),
                            PrecioUnitario  = ToDecimal(rd["PRECIO_UNITARIO"]),
                            PctjeDscto      = ToDecimal(rd["PCTJE_DSCTO"]),
                            PctjeDsctoPro   = ToDecimal(rd["PCTJE_DSCTO_PRO"]),
                            PctjeDsctoVol   = ToDecimal(rd["PCTJE_DSCTO_VOL"]),
                            PctjeDsctoPromo = ToDecimal(rd["PCTJE_DSCTO_PROMO"]),
                            DsctoArt        = ToDecimal(rd["DSCTO_ART"]),
                            DsctoExtra      = ToDecimal(rd["DSCTO_EXTRA"]),
                            PrecioTotalNeto = ToDecimal(rd["PRECIO_TOTAL_NETO"]),
                            PctjeArancel    = ToDecimal(rd["PCTJE_ARANCEL"]),
                            Notas           = Convert.ToString(rd["NOTAS"]) ?? "",
                            Posicion        = ToInt(rd["POSICION"]),
                        });
                    }
                }
            }

            // 2) Por cada renglón, generamos IDs e insertamos.
            const string sqlInsDet =
                "INSERT INTO DOCTOS_CM_DET (" +
                "  DOCTO_CM_DET_ID, DOCTO_CM_ID, CLAVE_ARTICULO, ARTICULO_ID, UNIDADES, UNIDADES_REC_DEV, UNIDADES_A_REC, " +
                "  UMED, CONTENIDO_UMED, PRECIO_UNITARIO, PCTJE_DSCTO, PCTJE_DSCTO_PRO, PCTJE_DSCTO_VOL, " +
                "  PCTJE_DSCTO_PROMO, DSCTO_ART, DSCTO_EXTRA, PRECIO_TOTAL_NETO, PCTJE_ARANCEL, NOTAS, POSICION" +
                ") VALUES (" +
                "  @det, @docto, @clave, @art, @un, @recDev, @aRec, " +
                "  @umed, @contUmed, @precio, @pdsc, @pdscPro, @pdscVol, " +
                "  @pdscPromo, @dsctoArt, @dsctoExtra, @neto, @pArancel, @notas, @posicion" +
                ")";

            const string sqlInsLiga =
                "INSERT INTO DOCTOS_CM_LIGAS_DET (DOCTO_CM_LIGA_ID, DOCTO_CM_DET_FTE_ID, DOCTO_CM_DET_DEST_ID) " +
                "VALUES (@liga, @fte, @dest)";

            foreach (var r in renglones)
            {
                ct.ThrowIfCancellationRequested();

                int nuevoDetId = await GenDoctoIdAsync(con, tx, ct).ConfigureAwait(false);

                using (var cmd = new FbCommand(sqlInsDet, con, tx))
                {
                    cmd.Parameters.Add("@det",        FbDbType.Integer).Value = nuevoDetId;
                    cmd.Parameters.Add("@docto",      FbDbType.Integer).Value = doctoCmDestinoId;
                    cmd.Parameters.Add("@clave",      FbDbType.VarChar).Value = (object) r.ClaveArticulo ?? DBNull.Value;
                    cmd.Parameters.Add("@art",        FbDbType.Integer).Value = r.ArticuloId;
                    cmd.Parameters.Add("@un",         FbDbType.Double).Value  = (double) r.Unidades;
                    cmd.Parameters.Add("@recDev",     FbDbType.Double).Value  = (double) r.UnidadesRecDev;
                    cmd.Parameters.Add("@aRec",       FbDbType.Double).Value  = (double) r.UnidadesARec;
                    cmd.Parameters.Add("@umed",       FbDbType.VarChar).Value = (object) r.Umed ?? DBNull.Value;
                    cmd.Parameters.Add("@contUmed",   FbDbType.Double).Value  = (double) r.ContenidoUmed;
                    cmd.Parameters.Add("@precio",     FbDbType.Double).Value  = (double) r.PrecioUnitario;
                    cmd.Parameters.Add("@pdsc",       FbDbType.Double).Value  = (double) r.PctjeDscto;
                    cmd.Parameters.Add("@pdscPro",    FbDbType.Double).Value  = (double) r.PctjeDsctoPro;
                    cmd.Parameters.Add("@pdscVol",    FbDbType.Double).Value  = (double) r.PctjeDsctoVol;
                    cmd.Parameters.Add("@pdscPromo",  FbDbType.Double).Value  = (double) r.PctjeDsctoPromo;
                    cmd.Parameters.Add("@dsctoArt",   FbDbType.Double).Value  = (double) r.DsctoArt;
                    cmd.Parameters.Add("@dsctoExtra", FbDbType.Double).Value  = (double) r.DsctoExtra;
                    cmd.Parameters.Add("@neto",       FbDbType.Double).Value  = (double) r.PrecioTotalNeto;
                    cmd.Parameters.Add("@pArancel",   FbDbType.Double).Value  = (double) r.PctjeArancel;
                    cmd.Parameters.Add("@notas",      FbDbType.VarChar).Value = (object) r.Notas ?? DBNull.Value;
                    cmd.Parameters.Add("@posicion",   FbDbType.Integer).Value = r.Posicion;
                    await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using (var cmd = new FbCommand(sqlInsLiga, con, tx))
                {
                    cmd.Parameters.Add("@liga", FbDbType.Integer).Value = doctoCmLigaId;
                    cmd.Parameters.Add("@fte",  FbDbType.Integer).Value = r.DoctoCmDetId;
                    cmd.Parameters.Add("@dest", FbDbType.Integer).Value = nuevoDetId;
                    await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
            }

            return renglones.Count;
        }

        // ================================================================
        // BLOQUE 7 — copia IMPUESTOS_DOCTOS_CM
        // ================================================================

        private static async Task<int> CopiarImpuestosAsync(
            FbConnection con, FbTransaction tx,
            int doctoCmOrigenId, int doctoCmDestinoId,
            CancellationToken ct)
        {
            // Mismo patrón que detalle: leer todo a memoria primero, luego loop de inserts.
            var filas = new System.Collections.Generic.List<FilaImpuesto>();
            const string sqlLeer =
                "SELECT IMPUESTO_ID, COMPRA_NETA, OTROS_IMPUESTOS, PCTJE_IMPUESTO, " +
                "       IMPORTE_IMPUESTO, UNIDADES_IMPUESTO, IMPORTE_UNITARIO_IMPUESTO " +
                "  FROM IMPUESTOS_DOCTOS_CM " +
                " WHERE DOCTO_CM_ID = @docto";

            using (var cmd = new FbCommand(sqlLeer, con, tx))
            {
                cmd.Parameters.Add("@docto", FbDbType.Integer).Value = doctoCmOrigenId;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        filas.Add(new FilaImpuesto
                        {
                            ImpuestoId               = ToInt(rd["IMPUESTO_ID"]),
                            CompraNeta               = ToDecimal(rd["COMPRA_NETA"]),
                            OtrosImpuestos           = ToDecimal(rd["OTROS_IMPUESTOS"]),
                            PctjeImpuesto            = ToDecimal(rd["PCTJE_IMPUESTO"]),
                            ImporteImpuesto          = ToDecimal(rd["IMPORTE_IMPUESTO"]),
                            UnidadesImpuesto         = ToDecimal(rd["UNIDADES_IMPUESTO"]),
                            ImporteUnitarioImpuesto  = ToDecimal(rd["IMPORTE_UNITARIO_IMPUESTO"]),
                        });
                    }
                }
            }

            const string sqlIns =
                "INSERT INTO IMPUESTOS_DOCTOS_CM (" +
                "  DOCTO_CM_ID, IMPUESTO_ID, COMPRA_NETA, OTROS_IMPUESTOS, PCTJE_IMPUESTO, " +
                "  IMPORTE_IMPUESTO, UNIDADES_IMPUESTO, IMPORTE_UNITARIO_IMPUESTO" +
                ") VALUES (" +
                "  @docto, @imp, @neta, @otros, @pctje, @importe, @unidades, @unitario" +
                ")";

            foreach (var f in filas)
            {
                ct.ThrowIfCancellationRequested();

                using (var cmd = new FbCommand(sqlIns, con, tx))
                {
                    cmd.Parameters.Add("@docto",    FbDbType.Integer).Value = doctoCmDestinoId;
                    cmd.Parameters.Add("@imp",      FbDbType.Integer).Value = f.ImpuestoId;
                    cmd.Parameters.Add("@neta",     FbDbType.Double).Value  = (double) f.CompraNeta;
                    cmd.Parameters.Add("@otros",    FbDbType.Double).Value  = (double) f.OtrosImpuestos;
                    cmd.Parameters.Add("@pctje",    FbDbType.Double).Value  = (double) f.PctjeImpuesto;
                    cmd.Parameters.Add("@importe",  FbDbType.Double).Value  = (double) f.ImporteImpuesto;
                    cmd.Parameters.Add("@unidades", FbDbType.Double).Value  = (double) f.UnidadesImpuesto;
                    cmd.Parameters.Add("@unitario", FbDbType.Double).Value  = (double) f.ImporteUnitarioImpuesto;
                    await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
            }

            return filas.Count;
        }

        // ================================================================
        // BLOQUE 8 — buscar o insertar REPOSITORIO_CFDI
        // ================================================================

        /// <summary>
        /// Si ya existe una fila en <c>REPOSITORIO_CFDI</c> con la UUID,
        /// devuelve su <c>CFDI_ID</c> y el XML almacenado. Si no, inserta una
        /// nueva con el XML del portal (pasado por
        /// <see cref="EncoderXmlMicrosip.PrepararParaMicrosip"/>) y devuelve
        /// el nuevo ID.
        ///
        /// Réplica de Func_Facturas_3_3.pas:858-1042. Hardcodeados igual que
        /// el Delphi: VERSION='4.0', MODALIDAD='CFDI', NATURALEZA='R',
        /// TIPO_COMPROBANTE='I', TIPO_DOCTO_MSP='Compra', SELLO_VALIDADO='M',
        /// ES_PARCIALIDAD='N', ES_SUSTITUTO='N', USUARIO_CREADOR='SISTEMAWEB'.
        ///
        /// IMPORTE se calcula como en el Delphi:
        ///   <c>IMPORTE_NETO + TOTAL_IMPUESTOS - TOTAL_RETENCIONES - DESCUENTO_GLOBAL</c>
        /// </summary>
        private static async Task<ResultadoCfdi> BuscarOInsertarCfdiAsync(
            FbConnection con, FbTransaction tx,
            FacturaAplicar factura, CfdiXmlMicrosip cfdi,
            string folioCompra, string folioXml, CancellationToken ct)
        {
            // 1) ¿Ya existe?
            using (var cmd = new FbCommand(
                "SELECT CFDI_ID, XML FROM REPOSITORIO_CFDI WHERE UUID = @uuid",
                con, tx))
            {
                cmd.Parameters.Add("@uuid", FbDbType.VarChar).Value = factura.UUID;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        return new ResultadoCfdi
                        {
                            CfdiId       = Convert.ToInt32(rd["CFDI_ID"]),
                            XmlPreparado = Convert.ToString(rd["XML"]) ?? "",
                            FueCreado    = false,
                        };
                    }
                }
            }

            // 2) No existe — preparamos el XML y lo insertamos.
            if (cfdi == null || string.IsNullOrEmpty(cfdi.xml))
                throw new InvalidOperationException(
                    "REPOSITORIO_CFDI no tiene la UUID '" + factura.UUID + "' pero el portal " +
                    "no devolvió el XML — no se puede insertar el CFDI.");

            // El Delphi hace: UTF-8 → ISO-8859-1 → ASCII (Func_Facturas_3_3.pas:931-934).
            // Replicamos paridad bit a bit con EncoderXmlMicrosip.
            string xmlPreparado = EncoderXmlMicrosip.PrepararParaMicrosip(cfdi.xml);

            int nuevoCfdiId = await GenDoctoIdAsync(con, tx, ct).ConfigureAwait(false);

            const string sqlInsert =
                "INSERT INTO REPOSITORIO_CFDI (" +
                "  CFDI_ID, MODALIDAD_FACTURACION, VERSION, UUID, NATURALEZA, TIPO_COMPROBANTE, " +
                "  TIPO_DOCTO_MSP, FOLIO, FECHA, RFC, NOMBRE, IMPORTE, MONEDA, TIPO_CAMBIO, " +
                "  ES_PARCIALIDAD, NOM_ARCH, XML, REFER_GRUPO, SELLO_VALIDADO, ES_SUSTITUTO, " +
                "  USUARIO_CREADOR, FECHA_HORA_CREACION, LUGAR_EXPEDICION, USO_CFDI" +
                ") VALUES (" +
                "  @id, 'CFDI', '4.0', @uuid, 'R', 'I', " +
                "  'Compra', @folio, @fecha, @rfc, @nombre, @importe, @moneda, @tc, " +
                "  'N', @nomArch, @xml, @refGrupo, 'M', 'N', " +
                "  'SISTEMAWEB', @fechaCrea, @lugar, @uso" +
                ")";

            decimal importe = factura.IMPORTE_NETO
                            + factura.TOTAL_IMPUESTOS
                            - factura.TOTAL_RETENCIONES
                            - factura.DESCUENTO_GLOBAL;

            using (var cmd = new FbCommand(sqlInsert, con, tx))
            {
                cmd.Parameters.Add("@id",        FbDbType.Integer).Value   = nuevoCfdiId;
                cmd.Parameters.Add("@uuid",      FbDbType.VarChar).Value   = factura.UUID ?? "";
                cmd.Parameters.Add("@folio",     FbDbType.VarChar).Value   = folioXml ?? "";
                cmd.Parameters.Add("@fecha",     FbDbType.TimeStamp).Value = ParsearFecha(factura.FECHA_FACTURA);
                cmd.Parameters.Add("@rfc",       FbDbType.VarChar).Value   = factura.RFC ?? "";
                cmd.Parameters.Add("@nombre",    FbDbType.VarChar).Value   = factura.NOMBRE ?? "";
                cmd.Parameters.Add("@importe",   FbDbType.Double).Value    = (double) importe;
                cmd.Parameters.Add("@moneda",    FbDbType.VarChar).Value   = factura.MONEDA_SIMBOLO ?? "";
                cmd.Parameters.Add("@tc",        FbDbType.Double).Value    = (double) factura.TIPO_CAMBIO;
                cmd.Parameters.Add("@nomArch",   FbDbType.VarChar).Value   = (factura.RFC ?? "") + "_" + (folioCompra ?? "") + ".xml";
                cmd.Parameters.Add("@xml",       FbDbType.Text).Value      = xmlPreparado ?? "";
                cmd.Parameters.Add("@refGrupo",  FbDbType.VarChar).Value   = folioCompra ?? "";
                cmd.Parameters.Add("@fechaCrea", FbDbType.TimeStamp).Value = DateTime.Now;
                cmd.Parameters.Add("@lugar",     FbDbType.VarChar).Value   = (object) (cfdi.lugar_expedicion ?? "") ?? DBNull.Value;
                cmd.Parameters.Add("@uso",       FbDbType.VarChar).Value   = (object) (cfdi.uso_cfdi ?? "") ?? DBNull.Value;

                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            return new ResultadoCfdi
            {
                CfdiId       = nuevoCfdiId,
                XmlPreparado = xmlPreparado,
                FueCreado    = true,
            };
        }

        // ================================================================
        // BLOQUE 9 — INSERT CFD_RECIBIDOS
        // ================================================================

        /// <summary>
        /// Liga la compra (DOCTOS_CM) con el CFDI almacenado en REPOSITORIO_CFDI.
        /// Réplica de Func_Facturas_3_3.pas:1053-1066.
        /// </summary>
        private static async Task InsertarCfdRecibidoAsync(
            FbConnection con, FbTransaction tx,
            int doctoCmId, int cfdiId, DateTime fechaFactura, string xml,
            CancellationToken ct)
        {
            int cfdRecibidoId = await GenDoctoIdAsync(con, tx, ct).ConfigureAwait(false);

            const string sql =
                "INSERT INTO CFD_RECIBIDOS (CFD_RECIBIDO_ID, CLAVE_SISTEMA, DOCTO_ID, FECHA, XML, CFDI_ID) " +
                "VALUES (@id, 'CM', @docto, @fecha, @xml, @cfdi)";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@id",    FbDbType.Integer).Value   = cfdRecibidoId;
                cmd.Parameters.Add("@docto", FbDbType.Integer).Value   = doctoCmId;
                cmd.Parameters.Add("@fecha", FbDbType.TimeStamp).Value = fechaFactura;
                cmd.Parameters.Add("@xml",   FbDbType.Text).Value      = xml ?? "";
                cmd.Parameters.Add("@cfdi",  FbDbType.Integer).Value   = cfdiId;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // BLOQUE 10 — INSERT VENCIMIENTOS_CARGOS_CM
        // ================================================================

        /// <summary>
        /// Crea el vencimiento al 100% en la fecha de pago. Réplica de
        /// Func_Facturas_3_3.pas:1086-1095. Microsip usa esta tabla para
        /// calcular el saldo y la antigüedad de cuentas por pagar.
        /// </summary>
        private static async Task InsertarVencimientoCargoCmAsync(
            FbConnection con, FbTransaction tx,
            int doctoCmId, DateTime fechaPago, CancellationToken ct)
        {
            const string sql =
                "INSERT INTO VENCIMIENTOS_CARGOS_CM (DOCTO_CM_ID, FECHA_VENCIMIENTO, PCTJE_VEN) " +
                "VALUES (@docto, @fecha, 100)";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@docto", FbDbType.Integer).Value   = doctoCmId;
                cmd.Parameters.Add("@fecha", FbDbType.TimeStamp).Value = fechaPago;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // BLOQUE 11 — EXEC stored proc GENERA_DOCTO_CP_CM
        // ================================================================

        /// <summary>
        /// Invoca el stored proc <c>GENERA_DOCTO_CP_CM(V_DOCTO_CM_ID)</c> que
        /// genera el cargo correspondiente en cuentas por pagar (DOCTOS_CP).
        /// Réplica de Func_Facturas_3_3.pas:1108-1119.
        ///
        /// El stored proc internamente crea: DOCTOS_CP, IMPUESTOS_DOCTO_CP,
        /// VENCIMIENTOS_CARGOS_CP, DOCTOS_ENTRE_SIS (liga CM↔CP), etc. Toda
        /// esa lógica vive en Microsip y solo nos consume el ID generado.
        /// </summary>
        private static async Task EjecutarGeneraDoctoCpCmAsync(
            FbConnection con, FbTransaction tx, int doctoCmId, CancellationToken ct)
        {
            using (var cmd = new FbCommand("GENERA_DOCTO_CP_CM", con, tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@V_DOCTO_CM_ID", FbDbType.Integer).Value = doctoCmId;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // BLOQUE 12 — adjuntos del portal → ZIP → ARCHIVOS_ADJUNTOS
        // ================================================================

        /// <summary>
        /// Por cada adjunto descargado: comprime en ZIP de una sola entrada
        /// (el formato que exige Microsip) y lo inserta en ARCHIVOS_ADJUNTOS
        /// con NOM_TABLA='DOCTOS_CM' y ELEM_ID=DOCTO_CM nuevo.
        ///
        /// Si un adjunto falla (tamaño 0, ZIP corrupto, etc.) se omite y
        /// continúa con los demás — la transacción NO se aborta porque
        /// perder un PDF no debe bloquear toda la aplicación de la factura.
        /// Mismo criterio que el Delphi (Func_Facturas_3_3.pas:1203-1208).
        /// </summary>
        private static async Task<ResultadoAdjuntos> InsertarAdjuntosAsync(
            FbConnection con, FbTransaction tx,
            int doctoCmId, AdjuntoDescargado[] adjuntos, CancellationToken ct)
        {
            var resumen = new ResultadoAdjuntos();
            if (adjuntos == null || adjuntos.Length == 0) return resumen;

            foreach (var adj in adjuntos)
            {
                ct.ThrowIfCancellationRequested();
                if (adj == null || adj.Contenido == null || adj.Contenido.Length == 0)
                {
                    resumen.Omitidos++;
                    continue;
                }

                try
                {
                    var zipBytes = ComprimirEnZip(adj.NombreOriginal, adj.Contenido);
                    int idAdjunto = await GenDoctoIdAsync(con, tx, ct).ConfigureAwait(false);

                    const string sql =
                        "INSERT INTO ARCHIVOS_ADJUNTOS (" +
                        "  ARCHIVO_ADJUNTO_ID, NOM_TABLA, ELEM_ID, FILE_NAME, FILE_SIZE, FILE_DATE, FILE_STREAM" +
                        ") VALUES (" +
                        "  @id, 'DOCTOS_CM', @elem, @name, @size, @fecha, @stream" +
                        ")";

                    using (var cmd = new FbCommand(sql, con, tx))
                    {
                        cmd.Parameters.Add("@id",     FbDbType.Integer).Value   = idAdjunto;
                        cmd.Parameters.Add("@elem",   FbDbType.Integer).Value   = doctoCmId;
                        cmd.Parameters.Add("@name",   FbDbType.VarChar).Value   = TruncarA(adj.NombreOriginal ?? "", 100);
                        // Delphi guarda el tamaño en KB: bytes / 1024.
                        cmd.Parameters.Add("@size",   FbDbType.Integer).Value   = adj.Contenido.Length / 1024;
                        cmd.Parameters.Add("@fecha",  FbDbType.TimeStamp).Value = DateTime.Now;
                        cmd.Parameters.Add("@stream", FbDbType.Binary).Value    = zipBytes;
                        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    }

                    resumen.Insertados++;
                }
                catch (Exception ex)
                {
                    EventoLog.Warning("Aplicación · adjunto id=" + adj.Id
                        + " omitido: " + ex.GetType().Name + ": " + ex.Message);
                    resumen.Omitidos++;
                }
            }

            return resumen;
        }

        private sealed class ResultadoAdjuntos
        {
            public int Insertados { get; set; }
            public int Omitidos   { get; set; }
        }

        private static string TruncarA(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= maxLen ? s : s.Substring(0, maxLen);
        }

        /// <summary>
        /// Comprime los bytes en un ZIP de UNA sola entrada con el nombre
        /// original — el formato exacto que pide Microsip para FILE_STREAM.
        /// Réplica de COMPRIMIR_EN_ZIP en Func_Facturas_3_3.pas:132-157.
        /// </summary>
        private static byte[] ComprimirEnZip(string nombreArchivo, byte[] contenido)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                using (var zip = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    var entry = zip.CreateEntry(nombreArchivo ?? "archivo.bin", System.IO.Compression.CompressionLevel.Optimal);
                    using (var es = entry.Open())
                    {
                        es.Write(contenido, 0, contenido.Length);
                    }
                }
                return ms.ToArray();
            }
        }

        // ================================================================
        // BLOQUE 13 — UPDATE DOCTOS_CM SET ESTATUS='F' en la recepción
        // ================================================================

        /// <summary>
        /// Marca la recepción origen como facturada. Réplica de
        /// Func_Facturas_3_3.pas:1226-1230.
        /// </summary>
        private static async Task MarcarRecepcionFacturadaAsync(
            FbConnection con, FbTransaction tx, string folioRecepcion, CancellationToken ct)
        {
            const string sql =
                "UPDATE DOCTOS_CM SET ESTATUS = 'F' " +
                " WHERE FOLIO = @folio AND TIPO_DOCTO = 'R'";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@folio", FbDbType.VarChar).Value = folioRecepcion ?? "";
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        // ================================================================
        // Helpers de conversión
        // ================================================================

        private static int     ToInt(object v)     => v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);
        private static decimal ToDecimal(object v) => v == null || v == DBNull.Value ? 0m : Convert.ToDecimal(v);

        /// <summary>
        /// Parsea "YYYY-MM-DD HH:MM:SS" o "DD/MM/YYYY HH:MM:SS" según venga del
        /// portal. Si el string es vacío usa <c>Now</c> como fallback seguro
        /// (Microsip exige NOT NULL en FECHA).
        /// </summary>
        private static DateTime ParsearFecha(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return DateTime.Now;

            DateTime dt;
            // MySQL devuelve YYYY-MM-DD HH:MM:SS (estándar ISO).
            if (DateTime.TryParseExact(s, "yyyy-MM-dd HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dt))
                return dt;

            // Por si el portal cambia el formato a DD/MM/YYYY (como Delphi histórico).
            if (DateTime.TryParseExact(s, "dd/MM/yyyy HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dt))
                return dt;

            // Último recurso: parseo libre invariante.
            if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out dt))
                return dt;

            return DateTime.Now;
        }

        // ================================================================
        // DTOs internos (solo para esta clase)
        // ================================================================

        private sealed class RecepcionOrigen
        {
            public int      DoctoCmId         { get; set; }
            public string   Estatus           { get; set; }
            public string   SubtipoDocto      { get; set; }
            public int      SucursalId        { get; set; }
            public string   ClaveProv         { get; set; }
            public int      ProveedorId       { get; set; }
            public int      AlmacenId         { get; set; }
            public int      MonedaId          { get; set; }
            public decimal  TipoCambio        { get; set; }
            public string   TipoDscto         { get; set; }
            public decimal  DsctoPctje        { get; set; }
            public decimal  DsctoImporte      { get; set; }
            public string   Descripcion       { get; set; }
            public decimal  ImporteNeto       { get; set; }
            public decimal  Fletes            { get; set; }
            public decimal  OtrosCargos       { get; set; }
            public decimal  TotalImpuestos    { get; set; }
            public decimal  TotalRetenciones  { get; set; }
            public decimal  GastosAduanales   { get; set; }
            public decimal  OtrosGastos       { get; set; }
            public int      CondPagoId        { get; set; }
            public string   CargarSun         { get; set; }
        }

        private sealed class RenglonDetalleOrigen
        {
            public int     DoctoCmDetId    { get; set; }
            public string  ClaveArticulo   { get; set; }
            public int     ArticuloId      { get; set; }
            public decimal Unidades        { get; set; }
            public decimal UnidadesRecDev  { get; set; }
            public decimal UnidadesARec    { get; set; }
            public string  Umed            { get; set; }
            public decimal ContenidoUmed   { get; set; }
            public decimal PrecioUnitario  { get; set; }
            public decimal PctjeDscto      { get; set; }
            public decimal PctjeDsctoPro   { get; set; }
            public decimal PctjeDsctoVol   { get; set; }
            public decimal PctjeDsctoPromo { get; set; }
            public decimal DsctoArt        { get; set; }
            public decimal DsctoExtra      { get; set; }
            public decimal PrecioTotalNeto { get; set; }
            public decimal PctjeArancel    { get; set; }
            public string  Notas           { get; set; }
            public int     Posicion        { get; set; }
        }

        private sealed class FilaImpuesto
        {
            public int      ImpuestoId              { get; set; }
            public decimal  CompraNeta              { get; set; }
            public decimal  OtrosImpuestos          { get; set; }
            public decimal  PctjeImpuesto           { get; set; }
            public decimal  ImporteImpuesto         { get; set; }
            public decimal  UnidadesImpuesto        { get; set; }
            public decimal  ImporteUnitarioImpuesto { get; set; }
        }

        /// <summary>
        /// Resultado de buscar/insertar el CFDI en REPOSITORIO_CFDI. Devuelve
        /// el ID que el bloque 9 (CFD_RECIBIDOS) necesita, el XML almacenado
        /// (para no leerlo dos veces si ya existía) y si fue creado en este
        /// ciclo.
        /// </summary>
        private sealed class ResultadoCfdi
        {
            public int    CfdiId       { get; set; }
            public string XmlPreparado { get; set; }
            public bool   FueCreado    { get; set; }
        }

        // ================================================================
        // Helpers para APLICAR_SIN_RECEPCION
        // ================================================================

        private sealed class ArticuloFb
        {
            public int    ArticuloId     { get; set; }
            public string ClaveArticulo  { get; set; }
            public string EsAlmacenable  { get; set; }
        }

        private sealed class ProveedorPrincipalFb
        {
            public int    ProveedorId { get; set; }
            public string ClaveProv   { get; set; }
            public string Nombre      { get; set; }
        }

        /// <summary>
        /// Réplica F_APLICAR_FACTURA.cs:1028-1049 — busca el primer artículo
        /// con ese nombre y devuelve <c>ES_ALMACENABLE</c> para que el caller
        /// decida si rechazarlo.
        /// </summary>
        private static async Task<ArticuloFb> LeerArticuloAsync(
            FbConnection con, FbTransaction tx, string nombre, CancellationToken ct)
        {
            const string sql =
                "SELECT FIRST 1 A.ARTICULO_ID, C.CLAVE_ARTICULO, A.ES_ALMACENABLE " +
                "  FROM ARTICULOS A " +
                "  LEFT JOIN CLAVES_ARTICULOS C ON (A.ARTICULO_ID = C.ARTICULO_ID) " +
                " WHERE A.NOMBRE = @nombre";

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@nombre", FbDbType.VarChar).Value = nombre ?? "";
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (!await rd.ReadAsync(ct).ConfigureAwait(false)) return null;
                    return new ArticuloFb
                    {
                        ArticuloId    = ToInt(rd["ARTICULO_ID"]),
                        ClaveArticulo = (Convert.ToString(rd["CLAVE_ARTICULO"]) ?? "").Trim(),
                        EsAlmacenable = (Convert.ToString(rd["ES_ALMACENABLE"]) ?? "").Trim(),
                    };
                }
            }
        }

        /// <summary>Réplica F_APLICAR_FACTURA.cs:1072-1082.</summary>
        private static async Task<int> LeerCondPagoIdAsync(
            FbConnection con, FbTransaction tx, string nombre, CancellationToken ct)
        {
            const string sql =
                "SELECT FIRST 1 C.COND_PAGO_ID " +
                "  FROM CONDICIONES_PAGO_CP C " +
                "  INNER JOIN PLAZOS_COND_PAG_CP P ON (C.COND_PAGO_ID = P.COND_PAGO_ID) " +
                " WHERE C.NOMBRE = @nombre";
            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@nombre", FbDbType.VarChar).Value = nombre ?? "";
                var raw = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return raw == null || raw == DBNull.Value ? 0 : Convert.ToInt32(raw);
            }
        }

        /// <summary>Réplica F_APLICAR_FACTURA.cs:1085-1092.</summary>
        private static async Task<int> LeerSucursalMatrizIdAsync(
            FbConnection con, FbTransaction tx, CancellationToken ct)
        {
            const string sql = "SELECT FIRST 1 SUCURSAL_ID FROM SUCURSALES WHERE NOMBRE = 'Matriz'";
            using (var cmd = new FbCommand(sql, con, tx))
            {
                var raw = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return raw == null || raw == DBNull.Value ? 0 : Convert.ToInt32(raw);
            }
        }

        /// <summary>
        /// Réplica F_APLICAR_FACTURA.cs:1094-1114. Busca el proveedor por
        /// PROVEEDOR_ID con su CLAVE_PROV principal (ES_PPAL='S').
        /// </summary>
        private static async Task<ProveedorPrincipalFb> LeerProveedorPrincipalAsync(
            FbConnection con, FbTransaction tx, int proveedorId, CancellationToken ct)
        {
            const string sql =
                "SELECT FIRST 1 P.PROVEEDOR_ID, P.NOMBRE, C.CLAVE_PROV " +
                "  FROM PROVEEDORES P " +
                "  INNER JOIN CLAVES_PROVEEDORES C ON (C.PROVEEDOR_ID = P.PROVEEDOR_ID) " +
                "  INNER JOIN ROLES_CLAVES_PROVEEDORES R ON (R.ROL_CLAVE_PROV_ID = C.ROL_CLAVE_PROV_ID) " +
                " WHERE R.ES_PPAL = 'S' " +
                "   AND P.PROVEEDOR_ID = @prov";
            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@prov", FbDbType.Integer).Value = proveedorId;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (!await rd.ReadAsync(ct).ConfigureAwait(false)) return null;
                    return new ProveedorPrincipalFb
                    {
                        ProveedorId = ToInt(rd["PROVEEDOR_ID"]),
                        Nombre      = (Convert.ToString(rd["NOMBRE"]) ?? "").Trim(),
                        ClaveProv   = (Convert.ToString(rd["CLAVE_PROV"]) ?? "").Trim(),
                    };
                }
            }
        }

        /// <summary>
        /// Fallback de moneda nacional para cuando la factura del portal trae
        /// <c>MONEDA_ID = 0</c> (filas viejas/incompletas). El SOAP nunca
        /// consulta MONEDAS — siempre usa el MONEDA_ID que mandó el portal
        /// (F_APLICAR_FACTURA.cs:1229) — así que no hay query legacy del flag
        /// de moneda local que replicar. En Microsip el peso nacional es
        /// siempre el <c>MONEDA_ID</c> más bajo (=1, el primero que crea el
        /// instalador), por lo que <c>SELECT FIRST 1 MONEDA_ID ... ORDER BY
        /// MONEDA_ID</c> lo resuelve de forma determinista sin depender del
        /// nombre del flag (que varía entre versiones de Microsip). Devuelve 0
        /// solo si la tabla MONEDAS está vacía (imposible en una empresa real);
        /// en ese caso el llamador aborta con error claro — NUNCA inserta 0.
        /// </summary>
        private static async Task<int> LeerMonedaNacionalIdAsync(
            FbConnection con, FbTransaction tx, CancellationToken ct)
        {
            const string sql = "SELECT FIRST 1 MONEDA_ID FROM MONEDAS ORDER BY MONEDA_ID";
            using (var cmd = new FbCommand(sql, con, tx))
            {
                var raw = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return raw == null || raw == DBNull.Value ? 0 : Convert.ToInt32(raw);
            }
        }

        /// <summary>
        /// INSERT en DOCTOS_CM para flujo SIN recepción. Réplica de
        /// F_APLICAR_FACTURA.cs:1195-1269. Diferencias respecto al flujo CON
        /// recepción:
        /// <list type="bullet">
        ///   <item><c>SUBTIPO_DOCTO='N'</c> (normal, no proviene de remisión).</item>
        ///   <item><c>TIPO_DSCTO='I'</c> (importe) — siempre.</item>
        ///   <item><c>FECHA</c> = HOY (el SOAP usa <c>FECHA_DTP</c> del DateTimePicker;
        ///         como aquí no hay UI editable, usamos hoy igual que el SOAP cuando
        ///         el operador no toca el control).</item>
        ///   <item>Los <c>DSCTO_PCTJE/DSCTO_IMPORTE</c> se calculan a partir de
        ///         <c>DESCUENTO_GLOBAL</c> de la factura del portal.</item>
        ///   <item><c>IMPORTE_NETO</c> = importe / 1.16 (aproximación del subtotal,
        ///         igual que el SOAP).</item>
        /// </list>
        /// </summary>
        private static async Task InsertarDoctosCmSinRecepcionAsync(
            FbConnection con, FbTransaction tx,
            int doctoCmId, int sucursalId, string folioFinal,
            FacturaAplicar factura, ProveedorPrincipalFb prov,
            int almacenId, int monedaId, int condPagoId,
            string folioCompra, decimal importeTotal,
            CancellationToken ct)
        {
            const string sql =
                "INSERT INTO DOCTOS_CM (" +
                "  DOCTO_CM_ID, TIPO_DOCTO, SUBTIPO_DOCTO, SUCURSAL_ID, FOLIO, FECHA, CLAVE_PROV, " +
                "  PROVEEDOR_ID, FOLIO_PROV, FACTURA_DEV, ALMACEN_ID, MONEDA_ID, TIPO_CAMBIO, " +
                "  TIPO_DSCTO, DSCTO_PCTJE, DSCTO_IMPORTE, ESTATUS, APLICADO, DESCRIPCION, " +
                "  IMPORTE_NETO, FLETES, OTROS_CARGOS, TOTAL_IMPUESTOS, TOTAL_RETENCIONES, " +
                "  GASTOS_ADUANALES, OTROS_GASTOS, FORMA_EMITIDA, CONTABILIZADO, ACREDITAR_CXP, " +
                "  SISTEMA_ORIGEN, COND_PAGO_ID, PCTJE_DSCTO_PPAG, CARGAR_SUN, ENVIADO, TIENE_CFD, " +
                "  USUARIO_CREADOR, USUARIO_AUT_CREACION, USUARIO_ULT_MODIF, USUARIO_AUT_MODIF" +
                ") VALUES (" +
                "  @docto, 'C', 'N', @sucursal, @folio, @fecha, @claveProv, " +
                "  @proveedor, @folioProv, '', @almacen, @moneda, @tipoCambio, " +
                "  'I', @dsctoPctje, @dsctoImporte, 'N', 'S', @descripcion, " +
                "  @importeNeto, 0, 0, @totalImp, @totalRet, " +
                "  0, 0, 'N', 'N', 'N', " +
                "  'CM', @condPago, 0, 'S', 'N', 'S', " +
                "  'SISTEMAWEB', 'SYSDBA', 'SISTEMAWEB', 'SISTEMAWEB'" +
                ")";

            decimal dsctoPctje = 0;
            decimal dsctoImporte = 0;
            if (factura.DESCUENTO_GLOBAL > 0 && importeTotal > 0)
            {
                dsctoPctje   = (factura.DESCUENTO_GLOBAL * 100m) / importeTotal;
                dsctoImporte = factura.DESCUENTO_GLOBAL;
            }

            // El SOAP usa importe/1.16 como aproximación del subtotal cuando
            // no tiene los renglones reales — la única línea es genérica.
            decimal importeNetoAprox = importeTotal / 1.16m;
            decimal totalImpAprox    = importeTotal - importeNetoAprox;

            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@docto",        FbDbType.Integer).Value   = doctoCmId;
                cmd.Parameters.Add("@sucursal",     FbDbType.Integer).Value   = sucursalId;
                cmd.Parameters.Add("@folio",        FbDbType.VarChar).Value   = folioFinal;
                cmd.Parameters.Add("@fecha",        FbDbType.TimeStamp).Value = DateTime.Today;
                cmd.Parameters.Add("@claveProv",    FbDbType.VarChar).Value   = (object) prov.ClaveProv ?? DBNull.Value;
                cmd.Parameters.Add("@proveedor",    FbDbType.Integer).Value   = prov.ProveedorId;
                cmd.Parameters.Add("@folioProv",    FbDbType.VarChar).Value   = folioCompra ?? "";
                cmd.Parameters.Add("@almacen",      FbDbType.Integer).Value   = almacenId;
                cmd.Parameters.Add("@moneda",       FbDbType.Integer).Value   = monedaId;
                cmd.Parameters.Add("@tipoCambio",   FbDbType.Double).Value    = (double) factura.TIPO_CAMBIO;
                cmd.Parameters.Add("@dsctoPctje",   FbDbType.Double).Value    = (double) dsctoPctje;
                cmd.Parameters.Add("@dsctoImporte", FbDbType.Double).Value    = (double) dsctoImporte;
                cmd.Parameters.Add("@descripcion",  FbDbType.VarChar).Value   = "Compra factura "
                                                                                + (factura.FOLIO_COMPRA ?? "")
                                                                                + " del proveedor "
                                                                                + (prov.Nombre ?? "");
                cmd.Parameters.Add("@importeNeto",  FbDbType.Double).Value    = (double) importeNetoAprox;
                cmd.Parameters.Add("@totalImp",     FbDbType.Double).Value    = (double) totalImpAprox;
                cmd.Parameters.Add("@totalRet",     FbDbType.Double).Value    = (double) factura.TOTAL_RETENCIONES;
                cmd.Parameters.Add("@condPago",     FbDbType.Integer).Value   = condPagoId;

                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// INSERT 1 línea genérica de DOCTOS_CM_DET. Réplica de
        /// F_APLICAR_FACTURA.cs:1283-1318: UNIDADES=1, UMED='NA',
        /// CONTENIDO_UMED=1, POSICION=1, PRECIO_UNITARIO=importe total.
        /// </summary>
        private static async Task InsertarDoctosCmDetSinRecepcionAsync(
            FbConnection con, FbTransaction tx,
            int detId, int doctoCmId, ArticuloFb art, decimal importeTotal,
            CancellationToken ct)
        {
            const string sql =
                "INSERT INTO DOCTOS_CM_DET (" +
                "  DOCTO_CM_DET_ID, DOCTO_CM_ID, CLAVE_ARTICULO, ARTICULO_ID, UNIDADES, " +
                "  UNIDADES_REC_DEV, UNIDADES_A_REC, UMED, CONTENIDO_UMED, PRECIO_UNITARIO, " +
                "  PCTJE_DSCTO, PCTJE_DSCTO_PRO, PCTJE_DSCTO_VOL, PCTJE_DSCTO_PROMO, " +
                "  DSCTO_ART, DSCTO_EXTRA, PRECIO_TOTAL_NETO, PCTJE_ARANCEL, NOTAS, POSICION" +
                ") VALUES (" +
                "  @det, @docto, @clave, @art, 1, 0, 0, 'NA', 1, @precio, " +
                "  0, 0, 0, 0, 0, 0, @neto, 0, '', 1" +
                ")";
            using (var cmd = new FbCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@det",    FbDbType.Integer).Value = detId;
                cmd.Parameters.Add("@docto",  FbDbType.Integer).Value = doctoCmId;
                cmd.Parameters.Add("@clave",  FbDbType.VarChar).Value = (object) art.ClaveArticulo ?? DBNull.Value;
                cmd.Parameters.Add("@art",    FbDbType.Integer).Value = art.ArticuloId;
                cmd.Parameters.Add("@precio", FbDbType.Double).Value  = (double) importeTotal;
                cmd.Parameters.Add("@neto",   FbDbType.Double).Value  = (double) importeTotal;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Loop por los impuestos definidos para el artículo y los inserta en
        /// IMPUESTOS_DOCTOS_CM. Réplica de F_APLICAR_FACTURA.cs:1331-1361.
        /// Devuelve la cantidad de filas insertadas.
        /// </summary>
        private static async Task<int> InsertarImpuestosDelArticuloAsync(
            FbConnection con, FbTransaction tx,
            int doctoCmId, string articuloNombre, decimal importeTotal, CancellationToken ct)
        {
            var impuestos = new System.Collections.Generic.List<ImpuestoArticulo>();

            const string sqlLeer =
                "SELECT M.IMPUESTO_ID, M.PCTJE_IMPUESTO " +
                "  FROM ARTICULOS A " +
                "  INNER JOIN IMPUESTOS_ARTICULOS I ON (A.ARTICULO_ID = I.ARTICULO_ID) " +
                "  INNER JOIN IMPUESTOS M           ON (M.IMPUESTO_ID = I.IMPUESTO_ID) " +
                " WHERE A.NOMBRE = @nombre";

            using (var cmd = new FbCommand(sqlLeer, con, tx))
            {
                cmd.Parameters.Add("@nombre", FbDbType.VarChar).Value = articuloNombre ?? "";
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        impuestos.Add(new ImpuestoArticulo
                        {
                            ImpuestoId = ToInt(rd["IMPUESTO_ID"]),
                            Pctje      = ToDecimal(rd["PCTJE_IMPUESTO"]),
                        });
                    }
                }
            }

            const string sqlIns =
                "INSERT INTO IMPUESTOS_DOCTOS_CM (" +
                "  DOCTO_CM_ID, IMPUESTO_ID, COMPRA_NETA, OTROS_IMPUESTOS, PCTJE_IMPUESTO, " +
                "  IMPORTE_IMPUESTO, UNIDADES_IMPUESTO, IMPORTE_UNITARIO_IMPUESTO" +
                ") VALUES (" +
                "  @docto, @imp, @neta, 0, @pctje, @importe, 0, 0" +
                ")";

            int n = 0;
            foreach (var imp in impuestos)
            {
                ct.ThrowIfCancellationRequested();
                decimal importe = importeTotal * (imp.Pctje / 100m);
                using (var cmd = new FbCommand(sqlIns, con, tx))
                {
                    cmd.Parameters.Add("@docto",   FbDbType.Integer).Value = doctoCmId;
                    cmd.Parameters.Add("@imp",     FbDbType.Integer).Value = imp.ImpuestoId;
                    cmd.Parameters.Add("@neta",    FbDbType.Double).Value  = (double) importeTotal;
                    cmd.Parameters.Add("@pctje",   FbDbType.Double).Value  = (double) imp.Pctje;
                    cmd.Parameters.Add("@importe", FbDbType.Double).Value  = (double) importe;
                    await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
                n++;
            }
            return n;
        }

        private sealed class ImpuestoArticulo
        {
            public int     ImpuestoId { get; set; }
            public decimal Pctje      { get; set; }
        }
    }
}
