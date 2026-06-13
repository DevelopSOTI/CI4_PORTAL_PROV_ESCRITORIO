# Portal Proveedores — Reglas de oro del proyecto

## Contexto multi-proyecto

Cuatro proyectos coexisten y CADA UNO tiene un rol específico. **No mezclar lógica entre ellos.**

| Proyecto | Ruta | Rol |
|---|---|---|
| **Delphi (legacy Embarcadero)** | `C:\Users\Desarrollo\source\repos\PortalProveedoresServicioEmbarcadero\ServicioPortalProveedoresEmbarcadero\` | Referencia de **consultas SQL** y **nombres exactos de tablas MySQL**. Archivos clave: `Func.pas`, `Func_Catalogos.pas`, `Func_Facturas_3_3.pas`, `Func_Complementos.pas`, `Func_Recepciones.pas`, `Func_Creditos.pas`, `Func_Notas.pas`, `Func_Calcula.pas`, `Data.pas`. Solo lectura. |
| **C# con SOAP (legacy)** | `C:\Users\Desarrollo\source\repos\PortalProveedores\` (`Servicio Portal\`, `Configurador\`, `ConfiguradorLocal\`) | Referencia del **flujo funcional** que está corriendo hoy. Solo lectura — NO se modifica. |
| **Portal web CodeIgniter 4** | `C:\wamp\www\PortalProveedores\` (deploy en `https://proveedores.soti.com.mx`) | Aquí se definen y exponen las **APIs REST** que consume el servicio nuevo. |
| **Servicio nuevo C# (ESTE proyecto)** | `C:\Users\Desarrollo\source\repos\PortalProveedoresServicioEmbarcadero\Nuevo\PortalProveedoresService\` | Reescritura del servicio aplicando la lógica como **nueva y mejorada con mejores prácticas**. Consume el portal CI4 vía REST con `X-API-Key`. |

## Reglas innegociables

1. **No mezclar código viejo en el proyecto nuevo.** Los proyectos legacy son guía de referencia, no fuente de copy-paste. La lógica se reimplementa limpia.
2. **El sistema se vende miles de veces.** Cada instalación apunta a un portal distinto. Por eso:
   - **Ninguna URL base, API key, ni credencial va hardcodeada en el código.**
   - Todo eso vive en el **registro de Windows** (`HKLM\SOFTWARE\SOTI\Service Portal`).
3. **Firebird (Microsip)** se conecta con usuario, password y carpeta de datos leídos del registro — **mismo patrón que la app de escritorio**, porque probablemente compartirán los mismos registros en la misma máquina.
4. **Sin SOAP en el proyecto nuevo.** Solo REST contra `/api/...` del portal CI4, autenticado con header `X-API-Key`.
5. **El portal CI4 es el único que toca MySQL.** El servicio nuevo nunca abre conexión directa al MySQL del hosting (las llaves `MYSQL_*` del registro legacy NO se usan en el proyecto nuevo).

## Registros de Windows (HKLM\SOFTWARE\SOTI\Service Portal)

Archivo listo para importar: `ServicePortal_Registros.reg` (raíz de este proyecto).

Llaves que el proyecto nuevo SÍ debe leer (ver `Configuracion/RegistrosWindows.cs`):

| Llave | Ejemplo | Uso |
|---|---|---|
| `MICRO_SERV` | `localhost` | Datasource Firebird |
| `MICRO_ROOT` | `C:\Microsip datos` | Carpeta raíz de Microsip (sin `\System`) |
| `MICRO_USER` | `SYSDBA` | Usuario Firebird |
| `MICRO_PASS` | `masterkey` | Password Firebird |
| `PORTAL_BASE_URL` | `https://proveedores.soti.com.mx` | URL base del portal CI4 — **clave para multi-cliente** |
| `PORTAL_API_KEY` | `acf2c85a…` | API key para el header `X-API-Key` |
| `MODE_TIMER` | `60` | Segundos entre ciclos completos de sincronización |
| `SERVICE_NAME` | `WSPortal` | Nombre del servicio Windows |
| `RUTA_ARCHIVOS` | `C:\prueba` | Carpeta predefinida de adjuntos locales |
| `MAILS_SEND` | `True` | Flag general de envío de correos del servicio |
| `ENVIAR_CORREO_COMPRAS` | `False` | Si True, el servicio manda correo al proveedor al registrar una compra desde una de sus facturas (cada cliente decide independiente) |

Llaves del modelo viejo que el proyecto nuevo **ignora**:
- `MYSQL_*`, `URL_WEBSERVICES` — modelo SOAP retirado.
- `MODE_APPLI` — su rol (Auto/Semi-automático del servicio) lo cumple ahora `PARAMETROS.APLICA_DIR` en MySQL. El servicio nuevo NO la lee de HKLM.

## Regla de oro de la sincronización por empresa

**El paso "Empresas" sincroniza TODAS las empresas** desde Microsip al portal (nuevas entran como `'Bloqueada'`). Los siguientes pasos (Almacenes, Proveedores, Recepciones, Créditos, Facturas, etc.) iteran ÚNICAMENTE las empresas que están como `EMP_ESTATUS='Autorizada'` en `EMPRESAS_MSP`.

Este patrón viene del Delphi histórico (`Func_Calcula.pas:159` — `SELECT * FROM EMPRESAS_MSP WHERE EMP_ESTATUS = 'Autorizada'`) y del SOAP C# legacy (`selectEmpAutorizadas()` web service). El nuevo proyecto lo respeta:

- **Endpoint REST**: `GET /api/empresas?solo_autorizadas=1` aplica el filtro server-side.
- **Core**: `IPortalApi.ListarEmpresasAutorizadasAsync(ct)` consume ese endpoint para el Servicio. `ListarEmpresasAsync` sin filtro sigue existiendo y la usa el Configurador para el grid completo.
- **Service**: cuando se implementen `SincronizadorProveedores`, `SincronizadorRecepciones`, etc., cada uno itera el resultado de `ListarEmpresasAutorizadasAsync`, y por cada empresa abre su `MICRO_ROOT\<EMP_NOMBRE>.FDB`.

### NOMBRE_CORTO: siempre desde CONFIG.FDB, nunca desde MySQL

Para abrir el Firebird de una empresa (`MICRO_ROOT\<NOMBRE_CORTO>.FDB`), el **NOMBRE_CORTO siempre se resuelve desde Microsip CONFIG.FDB por EMPRESA_ID**, NO desde el snapshot `EMP_NOMBRE` que vive en MySQL.

Razón: el usuario puede renombrar la empresa en Microsip después de que el portal recibió el último sync. MySQL queda con el nombre viejo; CONFIG.FDB tiene el actual. Si abrimos el .FDB con el nombre viejo, falla.

Mecanismo implementado:
- `IResolutorEmpresaMicrosip.ObtenerNombreCortoAsync(empresaId, ct)` lee `SELECT EMPRESA_ID, NOMBRE_CORTO FROM EMPRESAS` de CONFIG.FDB.
- Caché en memoria — una lectura por ciclo, no por sincronizador.
- Service1 llama `_resolutorEmpresa.Invalidar()` al inicio de cada ciclo, así un rename se refleja en el siguiente.
- **Cualquier sincronizador per-empresa (Almacenes, Monedas, Proveedores, etc.) debe usar este resolutor** antes de abrir el FDB.

### Mensajes hacia el Visor: solo nombres, nunca IDs

El operador no conoce ni le interesan los IDs de base de datos (`EMP_ID`, `EMP_ID_MSP`, ni el PID del proceso Windows). El Visor muestra todo por **nombre de empresa** (`EMP_NOMBRE`).

| Aceptable | Inaceptable |
|---|---|
| "Sincronizando empresa **SOTI**" | "Sincronizando empresa **id 1294**" |
| "▲ Servicio iniciado · WSPortal v1.0" | "▲ Servicio iniciado (PID 4856)" |
| "Paso Proveedores: 87 procesados" | "Paso Proveedores (paso_id=4): 87 procesados" |

El "Ciclo #5" sí se queda — es un contador lógico de ejecuciones del ciclo, no un identificador de base de datos. Ayuda a referenciar una ejecución específica en una conversación de soporte ("el ciclo 5 falló").

## Ciclo de sincronización del servicio

El servicio corre un ciclo completo cada `MODE_TIMER` segundos. El ciclo abarca:

1. Empresas (catálogo Microsip → `EMPRESAS_MSP`)
2. Proveedores
3. Monedas
4. Recepciones
5. Complementos de pago
6. (otros catálogos según se vayan integrando)
7. **Sellar** al final → `POST /api/empresas/sellar` actualiza `EMP_ULT_SINC` en todas las empresas `Autorizada`.

**Importante:** `EMP_ULT_SINC` representa "última vez que TODO el ciclo del cliente terminó OK", NO la última sync de empresas. Por eso el sello se pone una sola vez al cierre del ciclo, solo si todos los pasos previos terminaron sin error fatal.

## Configurador (UI Windows Forms del cliente)

El **Configurador** legacy (referencia: `C:\Users\Desarrollo\source\repos\PortalProveedores\Configurador\`) tiene esta estructura, que el nuevo Configurador debe replicar (con mejores prácticas):

**Pestañas principales:**
- **Microsip** — servidor, carpeta de datos, usuario, contraseña, "Probar conexión"
- **Portal Web** — dominio, usuario/contraseña/nombre de BD MySQL, puerto, "Probar Conexión", "Guardar Conexión", "Crear BD en Portal" (futuro: con MigrationRunner versionado, ver pendiente)
- **Servicio** — URL del servicio web, nombre del servicio, "Instalar", "Desinstalar", "Iniciar", "Detener", ubicación predefinida de archivos
- **Otros Parámetros** — "Sincronizar cada" (timer), "Días límite" (numericUpDown, habilitado por checkbox), "Cerrar al terminar", "Enviar correo a proveedores al generar compras", "¿Servicio registra compras de forma automática?"

**Menú Herramientas:**
- Configurar empresas autorizadas y bloqueadas
- Configurar correo de la página
- Configurar días de recepción

**Modos del servicio:**
- **Automático** — el servicio registra automáticamente las compras vinculadas a facturas desde el portal.
- **Semi-Automático** — una app de escritorio aparte trae la info de facturas de proveedores y las asigna manualmente a Microsip.

**ConfiguradorLocal** — **eliminado del alcance del proyecto nuevo.** El legacy era una versión simplificada (un solo form) pero terminó siendo redundante: configura los mismos registros que el Configurador grande. El nuevo Configurador hace todo y solicita elevación (UAC) únicamente cuando la operación lo requiere (escribir en HKLM, instalar/desinstalar servicio Windows).

## División de responsabilidades: Configurador vs Portal admin

Regla derivada de la regla de oro #4 + decisión del proyecto:

**El Configurador** sigue manejando empresas (autorizadas/bloqueadas, permite diferencias) y días de recepción, **pero NO abre MySQL directo**. Llama al portal CI4 vía REST con `X-API-Key`. Es la misma arquitectura del legacy SOAP, solo cambia el transporte.

**Empresas** (tabla `EMPRESAS_MSP`):
- Columnas relevantes: `EMP_ID` (PK auto interna del portal), `EMP_ID_MSP` (id natural de Microsip, el que usa el Configurador), `EMP_NOMBRE` (varchar 30), `EMP_NOMBRE_LARGO` (varchar 100), `EMP_RFC` (varchar 20, puede estar vacío), `EMP_ESTATUS` (varchar 10: `'Bloqueada'`/`'Autorizada'`), `EMP_DIFERENCIA` (char 1, `'S'`/`'N'`, default `'N'`), `EMP_ULT_SINC` (datetime nullable), `EMP_FECHA_ULT_MODIF` (datetime).
- **`EMP_DIFERENCIA` es bandera del portal web, NO del servicio.** Cuando un proveedor sube una factura desde el portal, si la empresa tiene `'S'` el portal permite tolerancia en la validación (diferencias factura↔recepción). El servicio Windows **ignora** este campo — el Configurador solo lo escribe vía REST y el portal lo lee al validar facturas.
- Operaciones desde el Configurador: listar, cambiar estatus, cambiar permite-diferencias.
- Endpoints REST a crear en el portal CI4:
  - `GET /api/empresas` → lista todas con sus flags.
  - `PATCH /api/empresas/{id}` body `{ estatus?: 'Bloqueada'|'Autorizada', diferencia?: 'S'|'N' }`.

**Días de recepción** (tabla `DIAS`):
- Columnas: `DIA_NUMERO` (1-7), `DIA_RECIBE` (1/0).
- Operaciones desde el Configurador: marcar qué días de la semana acepta recepciones.
- Endpoints REST a crear:
  - `GET /api/dias` → lista los 7 días con su flag.
  - `PATCH /api/dias` body `{ dias: [{numero, recibe}, ...] }` (batch).

**Lo que NO va al Configurador, se queda solo en el portal admin web** (ya implementado):
- `Admin\Correo` (config SMTP del portal — no se duplica)
- `Admin\Proveedores` (alta/bloqueo, permisos, reset password)
- `Admin\Tema`, `Admin\Usuarios`
- Login de proveedores y vistas del proveedor

## Pendientes capturados para más adelante

- **Creación de BD en hosting MySQL con MigrationRunner versionado**: hoy `C_CREAR_BD_PORTAL.cs` arma el schema con SQL crudo. Reemplazar por un sistema de migraciones (CodeIgniter ya las soporta nativo) para que las nuevas instalaciones y las actualizaciones del schema se manejen igual y en orden. Posponer hasta cerrar el ciclo de sincronización.

- **Visor en tiempo real (cuarto EXE del solution)**: app que se autoarranca cuando un usuario inicia sesión en Windows y muestra en pantalla, estilizada, todo lo que el servicio está haciendo en vivo (logs, ciclos, sincronizaciones, errores). Decisiones de arquitectura (alineadas con el Documento Maestro v2 del proyecto):
  - **Auto-arranque**: entrada en `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` con argumento `--tray`. Sin Scheduled Task ni `CreateProcessAsUser`. El Visor se auto-registra al primer arranque.
  - **IPC**: Named Pipes bidireccionales (`System.IO.Pipes`, `PipeDirection.InOut`). Servidor en el Servicio, cliente en el Visor. Empuje, no sondeo. Protocolo NDJSON. El Servicio internamente usa `Channel<T>` para canalizar eventos del worker hacia el pipe writer. Por el mismo pipe viajan eventos del servicio (status, progress, errors) Y comandos del visor (pausar, reanudar, detener, etc.).
  - **Pipe ACL**: solo el usuario interactivo + LocalSystem.
  - **Es una app aparte del Configurador** (decisión del proyecto, distinta a lo que sugiere el doc maestro que unifica config + monitoreo en una sola app).

## Discrepancias conocidas respecto al Documento Maestro v2

El doc maestro establece varias cosas que NO se cumplen en la implementación actual. Están vivas y se irán alineando incrementalmente:

| Discrepancia | Doc dice | Implementación actual | Decisión |
|---|---|---|---|
| Helper elevado | Helper.exe separado con manifest `requireAdministrator` | `TareasElevadas` dentro del Configurador con self-relaunch UAC | **Se queda como está** — funciona igual y simplifica deploy |
| .NET target | .NET 8 Worker Service + `AddWindowsService()` + `IHostedService` | .NET Framework 4.6 con `ServiceBase` | **Migrar a 4.8** (para soporte de SO modernos sin saltar al runtime nuevo) |
| Cifrado de `MICRO_PASS` | DPAPI `ProtectedData.Protect` en HKLM | Texto plano | Pendiente — postergado |
| Single app (config + visor) | Una sola "App gráfica WinForms" | Configurador y Visor separados | **Se queda separado** — single responsibility |

## Tabla MySQL `PARAMETROS` — configuración de negocio global

NO es HKLM. Vive en la BD del portal y aplica a todos los proveedores del cliente. El Configurador la edita vía REST al portal CI4 (a crear: `GET /api/parametros`, `PATCH /api/parametros`).

**Schema:** `PARAM_ID` (PK auto), `PARAM_CLAVE` (varchar 20, único en la práctica), `PARAM_VALOR` (varchar 20 nullable), `PARAM_DESCRIPCION` (text NOT NULL). La descripción la sirve el backend al Configurador (no se hardcodea en C#).

| PARAM_CLAVE | Ejemplo | Significado |
|---|---|---|
| `TOLERANCIA` | `0.0000` | Tolerancia en monto que tiene el proveedor para subir una factura con diferencia al total de la recepción |
| `APLICA_DIR` | `TRUE` / `FALSE` | Comportamiento del servicio. `TRUE` = inserta solo las compras que tienen una factura relacionada a la recepción del proveedor. `FALSE` = no inserta nada y la app de escritorio (semi-automática) lee este flag para mostrar todas las facturas con o sin recepción para revisión manual del operador. |
| `LAST_UPDATE` | `02/06/2026 13:47:12` | Marca de la última sincronización completa (escrita por el servicio). **Read-only desde el Configurador**: el endpoint PATCH la ignora silenciosamente si llega en el body. |
| `DIAS_LIMITE` | `28` | Días máximos para que el proveedor suba su factura. `0` = sin límite |
| `DIAS_COMPLEMENTO` | `5` | Días máximos para subir el complemento de pago |
| `DIAS_LIMITE_PUE` | `15` | Días máximos para facturas PUE (pago en una exhibición) |

**Endpoints del portal CI4:**
- `GET /api/parametros` → `{"parametros":[{clave, valor, descripcion},...]}`
- `PATCH /api/parametros` → body `{"cambios":[{clave, valor},...]}` — solo modifica filas existentes (no inserta nuevas). Devuelve la lista completa tras la actualización + un `resumen` con `aplicados`, `ignorados_auto` (claves protegidas), `no_encontrados`.

**Para agregar un nuevo parámetro al negocio:** inserción manual / migración en MySQL con su descripción. Aparece automáticamente en el grid del Configurador en su siguiente carga, sin recompilar nada.

## Caminos a archivos clave

### Proyecto nuevo (ESTE)
- **Lectura de registros:** `PortalProveedoresService/Configuracion/RegistrosWindows.cs`
- **Conexión Firebird:** `PortalProveedoresService/Configuracion/ConexionMicrosip.cs`
- **Cliente REST al portal:** `PortalProveedoresService/Servicios/PortalApi.cs` (interfaz `IPortalApi.cs`)
- **Sincronizador de empresas:** `PortalProveedoresService/Sincronizacion/SincronizadorEmpresas.cs`

### Portal CI4 (referencia para APIs)
- **Filtro X-API-Key:** `app/Filters/ApiKeyFilter.php`
- **Rutas API:** `app/Config/Routes.php` (grupo `api`)
- **Controladores API:** `app/Controllers/Api/Adjuntos.php`, `app/Controllers/Api/Empresas.php`
- **`.env`:** raíz del portal (`portal.apiKey`, `database.*`)

### Delphi (referencia para SQL/tablas)
- **Conexión y queries base:** `Func.pas`, `Data.pas`
- **Catálogos (empresas, almacenes, proveedores):** `Func_Catalogos.pas`
- **Facturas:** `Func_Facturas_3_2.pas`, `Func_Facturas_3_3.pas`
- **Complementos de pago:** `Func_Complementos.pas`
- **Recepciones:** `Func_Recepciones.pas`
- **Créditos / notas:** `Func_Creditos.pas`, `Func_Notas.pas`

### C# SOAP (referencia de flujo)
- **Servicio Windows:** `Servicio Portal/Service_Portal.cs`, `C_EMPRESAS.cs`, `C_PROVEEDORES.cs`, `C_FACTURA33.cs`, `C_RECEPCIONES.cs`
- **Cadena Firebird de referencia:** `Servicio Portal/C_ConexionFirebird.cs`
