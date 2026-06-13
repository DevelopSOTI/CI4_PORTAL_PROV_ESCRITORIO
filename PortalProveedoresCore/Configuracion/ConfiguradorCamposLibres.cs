using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PortalProveedoresCore.Logging;

namespace PortalProveedoresCore.Configuracion
{
    /// <summary>
    /// Configura la base Firebird de una empresa Microsip para que sea usable
    /// por el portal: agrega los campos libres del proveedor, crea la fila
    /// FOLIOS_COMPRAS SERIE='WEB' y configura USO_CFDI (cabecera ATRIBUTOS +
    /// 25 opciones SAT + columna en LIBRES_REC_CM).
    ///
    /// CUÁNDO se invoca:
    ///   Una sola vez por empresa, en el momento en que el operador la
    ///   autoriza (transición EMP_ESTATUS = Bloqueada → Autorizada) desde el
    ///   Configurador. NO se llama desde el ciclo recurrente del servicio
    ///   Windows — eso sería desperdicio porque la operación es idempotente
    ///   pero costosa (abre Firebird de la empresa, hace metadata queries).
    ///
    ///   Replica fielmente el flujo del SOAP legacy
    ///   (<c>Configurador.C_FUNCIONES_MSP.CrearCamposParticulares</c>) que
    ///   también se disparaba desde el Configurador legacy al autorizar.
    ///
    /// IDEMPOTENCIA:
    ///   - Si los campos / opciones / columnas ya existen → no hace nada.
    ///   - Si la empresa se vuelve a autorizar tras haber sido bloqueada,
    ///     se re-ejecuta y ve "todo en orden". El bloqueo NO borra nada
    ///     (regla del proyecto).
    /// </summary>
    public sealed class ConfiguradorCamposLibres
    {
        // Las 25 opciones SAT del USO_CFDI, EXACTAMENTE como Microsip las almacena
        // en VALOR_DESPLEGADO (NOMBRE_MEDIO, truncadas a 50 chars). Estos strings
        // se copiaron del Firebird de una empresa funcional para garantizar
        // congruencia con la fuente — incluso los typos ("Mobilario") y los
        // truncados. El portal solo necesita el prefijo SAT ("G01", "P01"…),
        // el resto del texto es para la UI de Microsip.
        //
        // Si el SAT publica nuevos códigos, agregar aquí y bumpar la lista.
        private static readonly string[] UsoCfdiOpciones = new[]
        {
            "G01 - Adquisición de mercancias",
            "G02 - Devoluciones, descuentos o bonificaciones",
            "G03 - Gastos en general",
            "I01 - Construcciones",
            "I02 - Mobilario y equipo de oficina por inversione",
            "I03 - Equipo de transporte",
            "I04 - Equipo de computo y accesorios",
            "I05 - Dados, troqueles, moldes, matrices y herrame",
            "I06 - Comunicaciones telefónicas",
            "I07 - Comunicaciones satelitales",
            "I08 - Otra maquinaria y equipo",
            "D01 - Honorarios médicos, dentales y gastos hospit",
            "D02 - Gastos médicos por incapacidad o discapacida",
            "D03 - Gastos funerales.",
            "D04 - Donativos.",
            "D05 - Intereses reales efectivamente pagados por c",
            "D06 - Aportaciones voluntarias al SAR.",
            "D07 - Primas por seguros de gastos médicos.",
            "D08 - Gastos de transportación escolar obligatoria",
            "D09 - Depósitos en cuentas para el ahorro, primas",
            "D10 - Pagos por servicios educativos (colegiaturas",
            "P01 - Por definir",
            "S01 - Sin efectos fiscales",
            "CP01 - Pagos",
            "CN01 - Nómina",
        };

        // Definición completa de cada campo libre del proveedor:
        //  - SqlTipo: lo que va al ALTER TABLE (tipo Firebird).
        //  - Atributo*: lo que va a la tabla ATRIBUTOS (la cabecera que hace
        //    que el campo aparezca en la pestaña "Datos particulares" del
        //    proveedor en Microsip). Mapeo confirmado contra una empresa
        //    funcional del cliente:
        //      TIPO='C' = Caracter (VARCHAR), LONGITUD = tamaño
        //      TIPO='N' = Numérico, LONGITUD = parte entera, ESCALA = decimales
        //      TIPO='S' = Sí/No (booleano, dominio SI_NO_N)
        //      REQUERIDO = 'S' obliga al usuario a llenarlo, 'N' es opcional
        //
        // Las 7 primeras filas son las del SOAP legacy; REFERENCIA y
        // ADJUNTAR_ARCHIVOS son las que Microsip moderno espera para el
        // portal nuevo.
        private static readonly DefinicionCampoLibre[] CamposEsperados = new[]
        {
            //                    nombre                              tipoSql        T  Lon  Esc  Req  DefCar  DefNum
            new DefinicionCampoLibre("BANCO",                         "VARCHAR(20)",  "C", 20,   null, "N", null, null),
            new DefinicionCampoLibre("SUCURSAL",                      "VARCHAR(10)",  "C", 10,   null, "N", null, null),
            new DefinicionCampoLibre("CLABE",                         "VARCHAR(50)",  "C", 50,   null, "N", null, null),
            new DefinicionCampoLibre("CTA_TRANSFERENCIA_ELECTRONICA", "VARCHAR(20)",  "C", 20,   null, "N", null, null),
            new DefinicionCampoLibre("CORREO_CXC",                    "VARCHAR(99)",  "C", 99,   null, "N", null, null),
            // PCTJE_RECHAZO: default 0 — el portal interpreta NULL distinto a "no
            // rechaza nada"; con 0 explícito el comportamiento es inequívoco.
            new DefinicionCampoLibre("PCTJE_RECHAZO",                 "NUMERIC(6,3)", "N", 3,    3,    "S", null, 0m),
            // Para los campos tipo "S" (Sí/No) ponemos 'N' como valor por default,
            // así Microsip prepopula el dropdown con "NO" en cada proveedor nuevo
            // y el operador solo lo cambia cuando aplica — UX más cómoda.
            new DefinicionCampoLibre("PERMITIR_SIN_RECEPCION",        "SI_NO_N",      "S", null, null, "S", "N",  null),
            new DefinicionCampoLibre("REFERENCIA",                    "VARCHAR(50)",  "C", 50,   null, "N", null, null),
            new DefinicionCampoLibre("ADJUNTAR_ARCHIVOS",             "SI_NO_N",      "S", null, null, "N", "N",  null),
        };

        /// <summary>
        /// Configura el Firebird de la empresa identificada por
        /// <paramref name="empIdMsp"/>. Resuelve el NOMBRE_CORTO de la empresa
        /// desde CONFIG.FDB internamente (el llamador no necesita conocerlo).
        ///
        /// El método es safe-to-call repetidamente; solo crea lo que falta.
        /// </summary>
        public async Task<ResumenConfiguracionEmpresa> AsegurarAsync(int empIdMsp, CancellationToken ct)
        {
            var resumen = new ResumenConfiguracionEmpresa
            {
                CamposCreados              = new List<string>(),
                CamposYaExistian           = new List<string>(),
                CamposConError             = new List<string>(),
                AtributosProveedorConError = new List<string>(),
            };

            // 1. Resolver NOMBRE_CORTO desde CONFIG.FDB. Es la fuente de verdad
            //    (vs. EMPRESAS_MSP que puede tener el nombre viejo si renombran
            //    en Microsip después de autorizar). Si la empresa fue borrada
            //    en Microsip → null → no podemos seguir.
            var nombreCorto = await ResolverNombreCortoAsync(empIdMsp, ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(nombreCorto))
            {
                resumen.EmpresaNoEnConfigFdb = true;
                return resumen;
            }
            resumen.NombreCorto = nombreCorto;

            // 2. Abrir el FDB de la empresa y configurar.
            var con = new ConexionMicrosip();
            if (!con.ConectarMicrosip(nombreCorto))
            {
                resumen.ConexionFallo = true;
                return resumen;
            }

            try
            {
                var tablas = await ObtenerTablasAsync(con.FBC, ct).ConfigureAwait(false);
                if (!tablas.Contains("LIBRES_PROVEEDOR"))
                {
                    resumen.TablaLibresProveedorAusente = true;
                    return resumen;
                }

                // Campos libres del proveedor — SIN transacción explícita.
                // DDL en Firebird funciona mejor con auto-commit por comando:
                // si un ALTER falla, los demás siguen aplicándose y los exitosos
                // persisten. Después releemos metadata para verificar que cada
                // columna que dimos por "creada" REALMENTE aparezca (trust-but-verify).
                var colsAntes = await ObtenerColumnasTablaAsync(con.FBC, "LIBRES_PROVEEDOR", ct).ConfigureAwait(false);
                await AsegurarCamposProveedorAsync(con.FBC, colsAntes, resumen, ct).ConfigureAwait(false);

                // Verificación post-ALTER: si un nombre quedó en CamposCreados
                // pero al releer el schema NO está, lo movemos a CamposConError.
                // Esto cubre el caso histórico donde el ALTER reportaba éxito al
                // driver pero el commit silencioso fallaba y nadie se enteraba.
                var colsDespues = await ObtenerColumnasTablaAsync(con.FBC, "LIBRES_PROVEEDOR", ct).ConfigureAwait(false);
                VerificarCamposCreados(resumen, colsDespues);

                // Registrar la CABECERA de cada campo en la tabla ATRIBUTOS
                // (CLAVE_OBJETO='PROVEEDOR'). Esto es lo que hace que Microsip
                // muestre los campos en la pestaña "Datos particulares" del
                // proveedor. El SOAP legacy NO lo hacía — solo creaba columnas
                // y dejaba la cabecera para crearse manualmente desde Microsip.
                // Ese fue el "bug" reportado por el usuario.
                if (tablas.Contains("ATRIBUTOS"))
                {
                    await AsegurarAtributosProveedorAsync(con.FBC, resumen, ct).ConfigureAwait(false);
                }

                // FASE 2: aplicar defaults RETROACTIVAMENTE a las filas existentes.
                // VALOR_DEFAULT_* en ATRIBUTOS solo aplica a registros que Microsip
                // cree DESPUÉS; los proveedores que ya existen quedan con NULL.
                // Para que el portal interprete cada campo inequívocamente
                // (PCTJE_RECHAZO=0 ≠ NULL, PERMITIR_SIN_RECEPCION='N' ≠ NULL),
                // llenamos esos NULLs con un UPDATE.
                //
                // Obligatorio en TRANSACCIÓN APARTE: Firebird no permite usar
                // una columna recién creada en la misma transacción del ALTER
                // (error SQL -206). Aquí los ALTER ya fueron autocommit, pero
                // aún así separamos para aislar fallos.
                await AplicarDefaultsRetroactivosAsync(con.FBC, resumen, ct).ConfigureAwait(false);

                // Folio WEB — bloque aislado para no contaminar lo anterior si falla.
                await AsegurarFolioWebAsync(con.FBC, resumen, ct).ConfigureAwait(false);

                // USO_CFDI — bloque aislado por la misma razón.
                await AsegurarUsoCfdiAsync(con.FBC, tablas, resumen, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                EventoLog.Error("ConfiguradorCamposLibres('" + nombreCorto + "'): " + ex.Message);
                resumen.ErrorGeneral = ex.Message.Trim();
            }
            finally
            {
                con.Desconectar();
            }

            return resumen;
        }

        // ====================================================================
        // Resolver NOMBRE_CORTO desde CONFIG.FDB (lookup por EMPRESA_ID)
        // ====================================================================

        private static async Task<string> ResolverNombreCortoAsync(int empIdMsp, CancellationToken ct)
        {
            var con = new ConexionMicrosip();
            if (!con.ConectarConfigMicrosip()) return null;

            try
            {
                using (var cmd = new FbCommand(
                    "SELECT NOMBRE_CORTO FROM EMPRESAS WHERE EMPRESA_ID = @id", con.FBC))
                {
                    cmd.Parameters.Add("@id", FbDbType.Integer).Value = empIdMsp;
                    using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                    {
                        if (await rd.ReadAsync(ct).ConfigureAwait(false))
                        {
                            var raw = Convert.ToString(rd[0]);
                            return string.IsNullOrEmpty(raw) ? null : raw.Trim();
                        }
                    }
                }
            }
            finally
            {
                con.Desconectar();
            }

            return null;
        }

        // ====================================================================
        // LIBRES_PROVEEDOR — los 9 campos esperados
        // ====================================================================

        private async Task AsegurarCamposProveedorAsync(
            FbConnection con,
            HashSet<string> colsExistentes,
            ResumenConfiguracionEmpresa resumen,
            CancellationToken ct)
        {
            var creados   = (List<string>) resumen.CamposCreados;
            var existian  = (List<string>) resumen.CamposYaExistian;
            var conError  = (List<string>) resumen.CamposConError;

            foreach (var campo in CamposEsperados)
            {
                ct.ThrowIfCancellationRequested();

                if (colsExistentes.Contains(campo.Nombre))
                {
                    existian.Add(campo.Nombre);
                    continue;
                }

                // SIN transacción explícita — Firebird hace auto-commit por
                // comando DDL. Esto evita que un commit silencioso al final
                // descarte cambios que el código creía haber hecho.
                var sqlPrincipal = "ALTER TABLE LIBRES_PROVEEDOR ADD " + campo.Nombre + " " + campo.TipoSql;
                try
                {
                    using (var cmd = new FbCommand(sqlPrincipal, con))
                        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    creados.Add(campo.Nombre);
                    continue;
                }
                catch (Exception exPrincipal)
                {
                    if (campo.TipoSql != "SI_NO_N")
                    {
                        conError.Add(campo.Nombre + " (" + exPrincipal.Message.Trim() + ")");
                        continue;
                    }

                    var sqlFallback = "ALTER TABLE LIBRES_PROVEEDOR ADD " + campo.Nombre + " CHAR(1) DEFAULT 'N'";
                    try
                    {
                        using (var cmd = new FbCommand(sqlFallback, con))
                            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                        creados.Add(campo.Nombre + " (CHAR(1))");
                    }
                    catch (Exception exFallback)
                    {
                        conError.Add(campo.Nombre + " (" + exFallback.Message.Trim() + ")");
                    }
                }
            }
        }

        /// <summary>
        /// "Trust but verify": releemos metadata y movemos de
        /// <see cref="ResumenConfiguracionEmpresa.CamposCreados"/> a
        /// <see cref="ResumenConfiguracionEmpresa.CamposConError"/> cualquier
        /// nombre que el código dijo haber creado pero que no aparece en el
        /// schema actualizado. Sin esto, el operador podía ver "creados 9"
        /// en el MessageBox cuando en realidad nada se aplicó.
        /// </summary>
        private static void VerificarCamposCreados(ResumenConfiguracionEmpresa resumen, HashSet<string> colsDespues)
        {
            var creadosReportados = (List<string>) resumen.CamposCreados;
            var conError          = (List<string>) resumen.CamposConError;

            var creadosVerificados = new List<string>();
            foreach (var etiqueta in creadosReportados)
            {
                // La etiqueta puede ser "BANCO" o "BANCO (CHAR(1))" (fallback).
                // El nombre real es la primera palabra.
                var nombre = etiqueta.Split(' ')[0];
                if (colsDespues.Contains(nombre))
                {
                    creadosVerificados.Add(etiqueta);
                }
                else
                {
                    conError.Add(nombre + " (ALTER reportó éxito pero la columna no aparece en el schema — el cambio no persistió)");
                }
            }

            // Reemplazamos la lista en el resumen: ahora solo contiene los que
            // PASARON la verificación post-ALTER.
            resumen.CamposCreados = creadosVerificados;
        }

        // ====================================================================
        // ATRIBUTOS — cabecera que hace visibles los campos en Microsip
        // ====================================================================
        //
        // Para que un campo en LIBRES_PROVEEDOR aparezca en la pestaña "Datos
        // particulares" del proveedor de Microsip, hay que registrar su
        // cabecera en la tabla ATRIBUTOS con:
        //   CLAVE_OBJETO = 'PROVEEDOR'
        //   NOMBRE_COLUMNA = nombre de la columna en LIBRES_PROVEEDOR
        //   TIPO = 'C' Caracter / 'N' Numérico / 'S' Sí-No / 'L' Lista
        //   LONGITUD / ESCALA según corresponda
        //   POSICION = orden de display (consecutivo dentro de CLAVE_OBJETO)
        //
        // Sin esta cabecera, la columna existe en la BD pero Microsip no la ve.
        // El legacy SOAP NO lo hacía — ese era el bug reportado.

        private async Task AsegurarAtributosProveedorAsync(
            FbConnection con,
            ResumenConfiguracionEmpresa resumen,
            CancellationToken ct)
        {
            var conError = (List<string>) resumen.AtributosProveedorConError;

            // Una sola transacción DML — los INSERTs son atómicos. Si falla
            // alguno, no quedan cabeceras a medias en ATRIBUTOS.
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    foreach (var campo in CamposEsperados)
                    {
                        ct.ThrowIfCancellationRequested();

                        // ¿Ya existe la cabecera?
                        bool yaExiste = false;
                        using (var cmd = new FbCommand(
                            "SELECT FIRST 1 ATRIBUTO_ID FROM ATRIBUTOS " +
                            "WHERE CLAVE_OBJETO = 'PROVEEDOR' AND NOMBRE_COLUMNA = @c", con, tx))
                        {
                            cmd.Parameters.Add("@c", FbDbType.VarChar).Value = campo.Nombre;
                            using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                                yaExiste = await rd.ReadAsync(ct).ConfigureAwait(false);
                        }

                        if (yaExiste)
                        {
                            resumen.AtributosProveedorYaExistian++;
                            continue;
                        }

                        try
                        {
                            int newId = await GenerarCatalogoIdAsync(con, tx, ct).ConfigureAwait(false);
                            if (newId == 0)
                            {
                                conError.Add(campo.Nombre + " (GEN_CATALOGO_ID devolvió 0)");
                                continue;
                            }

                            short posicion = 1;
                            using (var cmd = new FbCommand(
                                "SELECT COALESCE(MAX(POSICION), 0) + 1 FROM ATRIBUTOS WHERE CLAVE_OBJETO = 'PROVEEDOR'", con, tx))
                            using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                            {
                                if (await rd.ReadAsync(ct).ConfigureAwait(false))
                                    posicion = Convert.ToInt16(rd[0]);
                            }

                            // Construimos el INSERT DINÁMICAMENTE — Firebird de
                            // Microsip tiene VALOR_DEFAULT_NUMERICO con NOT NULL
                            // (y posiblemente otras columnas), así que en lugar
                            // de pasar @param = DBNull.Value (que viola NOT NULL),
                            // OMITIMOS la columna cuando no tenemos valor. Firebird
                            // entonces aplica el default propio de la columna.
                            var columnas = new List<string> { "ATRIBUTO_ID", "NOMBRE", "NOMBRE_COLUMNA",
                                                              "CLAVE_OBJETO", "POSICION", "TIPO", "REQUERIDO" };
                            var valores  = new List<string> { "@id", "@nom", "@col",
                                                              "'PROVEEDOR'", "@pos", "@tip", "@req" };

                            if (campo.Longitud.HasValue)                     { columnas.Add("LONGITUD");               valores.Add("@lon"); }
                            if (campo.Escala.HasValue)                       { columnas.Add("ESCALA");                 valores.Add("@esc"); }
                            if (!string.IsNullOrEmpty(campo.ValorDefaultCaracter)) { columnas.Add("VALOR_DEFAULT_CARACTER"); valores.Add("@defC"); }
                            if (campo.ValorDefaultNumerico.HasValue)         { columnas.Add("VALOR_DEFAULT_NUMERICO"); valores.Add("@defN"); }

                            var sqlInsert = "INSERT INTO ATRIBUTOS (" + string.Join(", ", columnas)
                                          + ") VALUES (" + string.Join(", ", valores) + ")";

                            using (var cmd = new FbCommand(sqlInsert, con, tx))
                            {
                                cmd.Parameters.Add("@id",  FbDbType.Integer).Value  = newId;
                                cmd.Parameters.Add("@nom", FbDbType.VarChar).Value  = campo.Nombre;
                                cmd.Parameters.Add("@col", FbDbType.VarChar).Value  = campo.Nombre;
                                cmd.Parameters.Add("@pos", FbDbType.SmallInt).Value = posicion;
                                cmd.Parameters.Add("@tip", FbDbType.VarChar).Value  = campo.AtributoTipo;
                                cmd.Parameters.Add("@req", FbDbType.VarChar).Value  = campo.Requerido;

                                if (campo.Longitud.HasValue)
                                    cmd.Parameters.Add("@lon", FbDbType.SmallInt).Value = (short) campo.Longitud.Value;
                                if (campo.Escala.HasValue)
                                    cmd.Parameters.Add("@esc", FbDbType.SmallInt).Value = (short) campo.Escala.Value;
                                if (!string.IsNullOrEmpty(campo.ValorDefaultCaracter))
                                    cmd.Parameters.Add("@defC", FbDbType.VarChar).Value = campo.ValorDefaultCaracter;
                                if (campo.ValorDefaultNumerico.HasValue)
                                    cmd.Parameters.Add("@defN", FbDbType.Decimal).Value = campo.ValorDefaultNumerico.Value;

                                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                            }

                            resumen.AtributosProveedorCreados++;
                        }
                        catch (Exception ex)
                        {
                            conError.Add(campo.Nombre + " (" + ex.Message.Trim() + ")");
                        }
                    }

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { /* swallow */ }
                    throw;
                }
            }
        }

        // ====================================================================
        // FASE 2 — Defaults retroactivos para filas existentes
        // ====================================================================
        //
        // VALOR_DEFAULT_* en ATRIBUTOS solo aplica a registros que Microsip cree
        // DESPUÉS de definir la cabecera. Los proveedores que ya existían en la
        // BD quedan con la nueva columna en NULL. Para evitar la ambigüedad
        // "NULL vs valor por default" en el portal (PCTJE_RECHAZO=0 distinto a
        // PCTJE_RECHAZO IS NULL), llenamos esos NULLs con un UPDATE.
        //
        // Transacción aparte: además de la limitación Firebird -206 (no se
        // puede usar una columna recién creada en la misma transacción del
        // ALTER), aislar este bloque protege lo ya creado si el UPDATE falla.
        // Si una columna no existe (porque su ALTER falló antes), el catch
        // por columna lo reporta como warning sin abortar las demás.

        private async Task AplicarDefaultsRetroactivosAsync(
            FbConnection con,
            ResumenConfiguracionEmpresa resumen,
            CancellationToken ct)
        {
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    foreach (var campo in CamposEsperados)
                    {
                        ct.ThrowIfCancellationRequested();

                        // Solo aplicamos a campos con default declarado.
                        // VARCHAR sin default (BANCO, SUCURSAL, etc.) los dejamos
                        // en NULL — un string vacío no tiene mejor semántica que
                        // NULL para esos casos.
                        object valor = null;
                        FbDbType tipoDb = FbDbType.VarChar;

                        if (!string.IsNullOrEmpty(campo.ValorDefaultCaracter))
                        {
                            valor   = campo.ValorDefaultCaracter;
                            tipoDb  = FbDbType.Char;
                        }
                        else if (campo.ValorDefaultNumerico.HasValue)
                        {
                            valor   = campo.ValorDefaultNumerico.Value;
                            tipoDb  = FbDbType.Decimal;
                        }

                        if (valor == null) continue;

                        try
                        {
                            using (var cmd = new FbCommand(
                                "UPDATE LIBRES_PROVEEDOR SET " + campo.Nombre + " = @v " +
                                "WHERE " + campo.Nombre + " IS NULL", con, tx))
                            {
                                var p = cmd.Parameters.Add("@v", tipoDb);
                                p.Value = valor;
                                int filas = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                                resumen.FilasActualizadasConDefaults += filas;
                            }
                        }
                        catch (Exception exCampo)
                        {
                            // Un campo individual fallando no debe abortar los
                            // demás. Acumulamos el mensaje y seguimos.
                            var prefix = string.IsNullOrEmpty(resumen.DefaultsRetroactivosError)
                                ? "" : resumen.DefaultsRetroactivosError + "; ";
                            resumen.DefaultsRetroactivosError = prefix +
                                campo.Nombre + ": " + exCampo.Message.Trim();
                        }
                    }

                    tx.Commit();
                }
                catch (Exception ex)
                {
                    try { tx.Rollback(); } catch { /* swallow */ }
                    // No relanzamos: la estructura ya quedó creada (esos son los
                    // pasos anteriores). Esto es una operación de "limpieza" que
                    // puede fallar sin invalidar la autorización.
                    resumen.DefaultsRetroactivosError = ex.Message.Trim();
                }
            }
        }

        // ====================================================================
        // FOLIOS_COMPRAS — fila SERIE='WEB'
        // ====================================================================

        private async Task AsegurarFolioWebAsync(FbConnection con, ResumenConfiguracionEmpresa resumen, CancellationToken ct)
        {
            try
            {
                using (var cmd = new FbCommand(
                    "SELECT FOLIO_COMPRAS_ID FROM FOLIOS_COMPRAS WHERE SERIE = 'WEB'", con))
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        resumen.FolioWebYaExistia = true;
                        return;
                    }
                }

                int sucursalId = 0;
                using (var cmd = new FbCommand(
                    "SELECT SUCURSAL_ID FROM SUCURSALES WHERE NOMBRE = 'Matriz'", con))
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (await rd.ReadAsync(ct).ConfigureAwait(false))
                        sucursalId = Convert.ToInt32(rd[0]);
                }

                if (sucursalId == 0)
                {
                    resumen.FolioWebError = "no se encontró la sucursal 'Matriz'";
                    return;
                }

                int catalogoId = 0;
                using (var cmd = new FbCommand("EXECUTE PROCEDURE GEN_CATALOGO_ID", con))
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    if (await rd.ReadAsync(ct).ConfigureAwait(false))
                        catalogoId = Convert.ToInt32(rd[0]);
                }

                if (catalogoId == 0)
                {
                    resumen.FolioWebError = "GEN_CATALOGO_ID devolvió 0";
                    return;
                }

                var sqlInsert =
                    "INSERT INTO FOLIOS_COMPRAS (FOLIO_COMPRAS_ID, TIPO_DOCTO, SUCURSAL_ID, SERIE, CONSECUTIVO) " +
                    "VALUES (@id, 'C', @suc, 'WEB', 1)";
                using (var cmd = new FbCommand(sqlInsert, con))
                {
                    cmd.Parameters.Add("@id",  FbDbType.Integer).Value = catalogoId;
                    cmd.Parameters.Add("@suc", FbDbType.Integer).Value = sucursalId;
                    await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                resumen.FolioWebCreado = true;
            }
            catch (Exception ex)
            {
                resumen.FolioWebError = ex.Message.Trim();
            }
        }

        // ====================================================================
        // USO_CFDI: ATRIBUTOS + LISTAS_ATRIBUTOS + columna en LIBRES_REC_CM
        // ====================================================================
        //
        // Modelo descubierto del schema Microsip:
        //   ATRIBUTOS         — cabecera de "campos libres" (uno por columna libre):
        //                       CLAVE_OBJETO='REC_CM' identifica LIBRES_REC_CM;
        //                       NOMBRE_COLUMNA es el nombre de la columna;
        //                       TIPO='L' indica "lista desplegable".
        //   LISTAS_ATRIBUTOS  — opciones de cada lista, agrupadas por ATRIBUTO_ID;
        //                       VALOR_DESPLEGADO es lo que ve el usuario.
        //   LIBRES_REC_CM.USO_CFDI — columna que guarda el LISTA_ATRIB_ID elegido.
        //
        // No hay FK formal de LIBRES_REC_CM.USO_CFDI a LISTAS_ATRIBUTOS — Microsip
        // resuelve por convención vía (CLAVE_OBJETO, NOMBRE_COLUMNA). Por eso aquí
        // solo creamos los tres bloques sin agregar constraints extra.
        //
        // IDs: ATRIBUTOS y LISTAS_ATRIBUTOS comparten generador (GEN_CATALOGO_ID).

        private async Task AsegurarUsoCfdiAsync(
            FbConnection con,
            HashSet<string> tablas,
            ResumenConfiguracionEmpresa resumen,
            CancellationToken ct)
        {
            if (!tablas.Contains("LIBRES_REC_CM"))
            {
                resumen.UsoCfdiTablaAusente = true;
                return;
            }
            if (!tablas.Contains("ATRIBUTOS") || !tablas.Contains("LISTAS_ATRIBUTOS"))
            {
                resumen.UsoCfdiError = "tablas ATRIBUTOS/LISTAS_ATRIBUTOS ausentes";
                return;
            }

            try
            {
                int atributoId;
                bool atributoCreado = false;
                bool opcionesCreadas = false;

                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        atributoId = await ObtenerAtributoIdUsoCfdiAsync(con, tx, ct).ConfigureAwait(false);
                        if (atributoId == 0)
                        {
                            atributoId     = await CrearAtributoUsoCfdiAsync(con, tx, ct).ConfigureAwait(false);
                            atributoCreado = true;
                        }

                        int yaExistentes = await ContarOpcionesAsync(con, tx, atributoId, ct).ConfigureAwait(false);
                        if (yaExistentes == 0)
                        {
                            await CrearOpcionesUsoCfdiAsync(con, tx, atributoId, ct).ConfigureAwait(false);
                            opcionesCreadas = true;
                        }
                        else if (yaExistentes != UsoCfdiOpciones.Length)
                        {
                            resumen.UsoCfdiError = "lista parcial (" + yaExistentes
                                + "/" + UsoCfdiOpciones.Length + " opciones) — revisar manualmente en Microsip";
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { /* swallow */ }
                        throw;
                    }
                }

                resumen.UsoCfdiAtributoCreado  = atributoCreado;
                resumen.UsoCfdiOpcionesCreadas = opcionesCreadas ? UsoCfdiOpciones.Length : 0;

                var colsLibresRec = await ObtenerColumnasTablaAsync(con, "LIBRES_REC_CM", ct).ConfigureAwait(false);
                if (!colsLibresRec.Contains("USO_CFDI"))
                {
                    try
                    {
                        using (var cmd = new FbCommand("ALTER TABLE LIBRES_REC_CM ADD USO_CFDI ENTERO_ID", con))
                            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    }
                    catch
                    {
                        using (var cmd = new FbCommand("ALTER TABLE LIBRES_REC_CM ADD USO_CFDI INTEGER", con))
                            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    }
                    resumen.UsoCfdiColumnaCreada = true;
                }
            }
            catch (Exception ex)
            {
                resumen.UsoCfdiError = ex.Message.Trim();
            }
        }

        private static async Task<int> ObtenerAtributoIdUsoCfdiAsync(FbConnection con, FbTransaction tx, CancellationToken ct)
        {
            using (var cmd = new FbCommand(
                "SELECT ATRIBUTO_ID FROM ATRIBUTOS " +
                "WHERE CLAVE_OBJETO = 'REC_CM' AND NOMBRE_COLUMNA = 'USO_CFDI'", con, tx))
            using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                if (await rd.ReadAsync(ct).ConfigureAwait(false))
                    return Convert.ToInt32(rd[0]);
            }
            return 0;
        }

        private static async Task<int> CrearAtributoUsoCfdiAsync(FbConnection con, FbTransaction tx, CancellationToken ct)
        {
            int newId = await GenerarCatalogoIdAsync(con, tx, ct).ConfigureAwait(false);
            if (newId == 0) throw new Exception("GEN_CATALOGO_ID devolvió 0");

            short posicion = 1;
            using (var cmd = new FbCommand(
                "SELECT COALESCE(MAX(POSICION), 0) + 1 FROM ATRIBUTOS WHERE CLAVE_OBJETO = 'REC_CM'", con, tx))
            using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                if (await rd.ReadAsync(ct).ConfigureAwait(false))
                    posicion = Convert.ToInt16(rd[0]);
            }

            using (var cmd = new FbCommand(
                "INSERT INTO ATRIBUTOS " +
                "  (ATRIBUTO_ID, NOMBRE, NOMBRE_COLUMNA, CLAVE_OBJETO, POSICION, TIPO, ESCALA, REQUERIDO) " +
                "VALUES (@id, 'USO_CFDI', 'USO_CFDI', 'REC_CM', @pos, 'L', 0, 'N')", con, tx))
            {
                cmd.Parameters.Add("@id",  FbDbType.Integer).Value  = newId;
                cmd.Parameters.Add("@pos", FbDbType.SmallInt).Value = posicion;
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            return newId;
        }

        private static async Task<int> ContarOpcionesAsync(FbConnection con, FbTransaction tx, int atributoId, CancellationToken ct)
        {
            using (var cmd = new FbCommand(
                "SELECT COUNT(*) FROM LISTAS_ATRIBUTOS WHERE ATRIBUTO_ID = @aid", con, tx))
            {
                cmd.Parameters.Add("@aid", FbDbType.Integer).Value = atributoId;
                var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return Convert.ToInt32(result);
            }
        }

        private static async Task CrearOpcionesUsoCfdiAsync(FbConnection con, FbTransaction tx, int atributoId, CancellationToken ct)
        {
            for (int i = 0; i < UsoCfdiOpciones.Length; i++)
            {
                ct.ThrowIfCancellationRequested();

                int listaId = await GenerarCatalogoIdAsync(con, tx, ct).ConfigureAwait(false);
                if (listaId == 0) throw new Exception("GEN_CATALOGO_ID devolvió 0 en opción " + (i + 1));

                using (var cmd = new FbCommand(
                    "INSERT INTO LISTAS_ATRIBUTOS (LISTA_ATRIB_ID, ATRIBUTO_ID, VALOR_DESPLEGADO, POSICION) " +
                    "VALUES (@lid, @aid, @val, @pos)", con, tx))
                {
                    cmd.Parameters.Add("@lid", FbDbType.Integer).Value  = listaId;
                    cmd.Parameters.Add("@aid", FbDbType.Integer).Value  = atributoId;
                    cmd.Parameters.Add("@val", FbDbType.VarChar).Value  = UsoCfdiOpciones[i];
                    cmd.Parameters.Add("@pos", FbDbType.SmallInt).Value = (short)(i + 1);
                    await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
            }
        }

        private static async Task<int> GenerarCatalogoIdAsync(FbConnection con, FbTransaction tx, CancellationToken ct)
        {
            using (var cmd = new FbCommand("EXECUTE PROCEDURE GEN_CATALOGO_ID", con, tx))
            using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                if (await rd.ReadAsync(ct).ConfigureAwait(false))
                    return Convert.ToInt32(rd[0]);
            }
            return 0;
        }

        // ====================================================================
        // Metadata helpers (Firebird system tables)
        // ====================================================================

        private static async Task<HashSet<string>> ObtenerTablasAsync(FbConnection con, CancellationToken ct)
        {
            var tablas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new FbCommand(
                "SELECT RDB$RELATION_NAME FROM RDB$RELATIONS " +
                "WHERE RDB$VIEW_BLR IS NULL AND (RDB$SYSTEM_FLAG = 0 OR RDB$SYSTEM_FLAG IS NULL)", con))
            using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await rd.ReadAsync(ct).ConfigureAwait(false))
                {
                    var nombre = Convert.ToString(rd[0]);
                    if (!string.IsNullOrEmpty(nombre)) tablas.Add(nombre.Trim());
                }
            }
            return tablas;
        }

        private static async Task<HashSet<string>> ObtenerColumnasTablaAsync(FbConnection con, string nombreTabla, CancellationToken ct)
        {
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new FbCommand(
                "SELECT RDB$FIELD_NAME FROM RDB$RELATION_FIELDS WHERE RDB$RELATION_NAME = @t", con))
            {
                cmd.Parameters.Add("@t", FbDbType.VarChar).Value = nombreTabla;
                using (var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rd.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var nombre = Convert.ToString(rd[0]);
                        if (!string.IsNullOrEmpty(nombre)) cols.Add(nombre.Trim());
                    }
                }
            }
            return cols;
        }

        /// <summary>
        /// Definición declarativa de un campo libre del proveedor con su
        /// equivalente SQL (para el ALTER TABLE) y su metadata de ATRIBUTOS
        /// (para que aparezca en la pestaña "Datos particulares" de Microsip).
        /// </summary>
        private sealed class DefinicionCampoLibre
        {
            public string Nombre   { get; }
            /// <summary>Tipo Firebird para el ALTER TABLE (VARCHAR(N), NUMERIC(N,M), SI_NO_N).</summary>
            public string TipoSql  { get; }
            /// <summary>TIPO en ATRIBUTOS: 'C' Caracter, 'N' Numérico, 'S' Sí/No, 'L' Lista.</summary>
            public string AtributoTipo { get; }
            /// <summary>LONGITUD en ATRIBUTOS. Para 'C' es el tamaño; para 'N' es la parte entera; nulo para 'S'.</summary>
            public int?   Longitud { get; }
            /// <summary>ESCALA en ATRIBUTOS. Solo para 'N': cantidad de decimales.</summary>
            public int?   Escala   { get; }
            /// <summary>REQUERIDO en ATRIBUTOS: 'S' obliga al usuario a llenarlo en Microsip.</summary>
            public string Requerido { get; }
            /// <summary>
            /// VALOR_DEFAULT_CARACTER en ATRIBUTOS: lo que Microsip prepopula
            /// en el campo cuando crea un proveedor nuevo. Útil para campos
            /// tipo 'S' (Sí/No) donde casi siempre el operador escogería 'N'.
            /// Null = sin default (Microsip lo deja en blanco).
            /// </summary>
            public string ValorDefaultCaracter { get; }
            /// <summary>
            /// VALOR_DEFAULT_NUMERICO en ATRIBUTOS: equivalente para campos
            /// tipo 'N'. Null = sin default. También se usa como valor del
            /// UPDATE retroactivo en las filas existentes (fase 2).
            /// </summary>
            public decimal? ValorDefaultNumerico { get; }

            public DefinicionCampoLibre(
                string nombre, string tipoSql,
                string atributoTipo, int? longitud, int? escala, string requerido,
                string valorDefaultCaracter, decimal? valorDefaultNumerico)
            {
                Nombre = nombre;
                TipoSql = tipoSql;
                AtributoTipo = atributoTipo;
                Longitud = longitud;
                Escala = escala;
                Requerido = requerido;
                ValorDefaultCaracter = valorDefaultCaracter;
                ValorDefaultNumerico = valorDefaultNumerico;
            }
        }
    }

    /// <summary>
    /// Resultado de <see cref="ConfiguradorCamposLibres.AsegurarAsync"/>.
    /// Granular para que el llamador (Configurador C#) muestre al operador
    /// un resumen claro de qué se hizo / qué ya estaba.
    /// </summary>
    public sealed class ResumenConfiguracionEmpresa
    {
        /// <summary>NOMBRE_CORTO resuelto desde CONFIG.FDB para esa empresa (informativo).</summary>
        public string NombreCorto { get; set; }

        /// <summary>True si EMPRESA_ID no aparece en CONFIG.FDB (¿borrada en Microsip?).</summary>
        public bool EmpresaNoEnConfigFdb { get; set; }

        /// <summary>True si la conexión al FDB de la empresa falló.</summary>
        public bool ConexionFallo { get; set; }

        /// <summary>True si LIBRES_PROVEEDOR no existe en este Microsip.</summary>
        public bool TablaLibresProveedorAusente { get; set; }

        /// <summary>Error inesperado durante la operación (raro).</summary>
        public string ErrorGeneral { get; set; }

        /// <summary>Columnas creadas con ALTER TABLE en LIBRES_PROVEEDOR.</summary>
        public IReadOnlyList<string> CamposCreados { get; set; }

        /// <summary>Columnas que ya existían — no se tocaron.</summary>
        public IReadOnlyList<string> CamposYaExistian { get; set; }

        /// <summary>Errores no-fatales por campo (los demás siguen intentándose).</summary>
        public IReadOnlyList<string> CamposConError { get; set; }

        /// <summary>
        /// Cantidad de filas insertadas en ATRIBUTOS para los campos de
        /// LIBRES_PROVEEDOR. Crear el row aquí es lo que hace que Microsip
        /// muestre el campo en la pestaña "Datos particulares" del proveedor;
        /// sin esto, la columna existe en BD pero es invisible en la UI.
        /// </summary>
        public int AtributosProveedorCreados { get; set; }

        /// <summary>Cantidad de filas en ATRIBUTOS que ya existían — no se tocaron.</summary>
        public int AtributosProveedorYaExistian { get; set; }

        /// <summary>Errores no-fatales al registrar ATRIBUTOS de PROVEEDOR.</summary>
        public IReadOnlyList<string> AtributosProveedorConError { get; set; }

        /// <summary>
        /// Cantidad TOTAL de filas actualizadas por la fase 2 (UPDATE WHERE
        /// col IS NULL) sumando todas las columnas. Si hay 100 proveedores
        /// con PCTJE_RECHAZO NULL y se les setea 0, este contador suma 100.
        /// </summary>
        public int FilasActualizadasConDefaults { get; set; }

        /// <summary>Mensaje libre si la fase 2 falló. No invalida el resto.</summary>
        public string DefaultsRetroactivosError { get; set; }

        /// <summary>True si se insertó FOLIOS_COMPRAS SERIE='WEB' en este ciclo.</summary>
        public bool FolioWebCreado { get; set; }

        /// <summary>True si SERIE='WEB' ya existía — no se tocó.</summary>
        public bool FolioWebYaExistia { get; set; }

        /// <summary>Mensaje libre si el folio WEB no se pudo crear.</summary>
        public string FolioWebError { get; set; }

        /// <summary>True si LIBRES_REC_CM no existe en este Microsip — se omite USO_CFDI.</summary>
        public bool UsoCfdiTablaAusente { get; set; }

        /// <summary>True si la cabecera ATRIBUTOS de USO_CFDI se insertó.</summary>
        public bool UsoCfdiAtributoCreado { get; set; }

        /// <summary>Cantidad de opciones SAT insertadas en LISTAS_ATRIBUTOS.</summary>
        public int UsoCfdiOpcionesCreadas { get; set; }

        /// <summary>True si la columna USO_CFDI se agregó a LIBRES_REC_CM.</summary>
        public bool UsoCfdiColumnaCreada { get; set; }

        /// <summary>Mensaje libre si algo de USO_CFDI no se pudo.</summary>
        public string UsoCfdiError { get; set; }
    }
}
