# Mejora: override de sincronización de un solo uso + botón "Sincronizar ahora" en el Configurador

> **Estado:** IMPLEMENTADO 2026-09-22. PHP en producción (respaldo y reversa en
> `C:\SOTI\Respaldos\portal_soti_20260922_override\`); C# compilado en Debug, falta Release/instalador.
> Cambios respecto a este diseño: la clave JSON es `overrides` (va en los dos listados), el
> override nunca puede pasar del MAX (PHP 422 + el Service lo ignora y lo consume), la lógica del
> Service vive en `Sincronizacion/OverrideSincronizacion.cs`, y la UI es la columna
> "Re-sincronizar" + `FormReSincronizar` (casilla por módulo), no el modal de `sinc_desde`.
>
> **Fecha del diseño:** 2026-09-22.

## 0. Contexto y decisión de fondo

Hoy la "última fecha de sincronizado" de cada módulo del Service **no se persiste como cursor**:
el portal la calcula al vuelo como `MAX(fecha_últ_modif)` de lo que ya tiene guardado, por
empresa y por tabla, y la devuelve en el bloque `checkpoints` de `GET /api/empresas?solo_autorizadas=1`.
Cada sincronizador la usa (+1 segundo, para esquivar la diferencia de precisión Firebird
sub-segundo vs MySQL truncado al segundo) como filtro `WHERE FECHA_HORA_ULT_MODIF > checkpoint`.

- Catálogos (Almacenes, Monedas, Proveedores) → `ParsearCheckpoint(emp.checkpoints.X)` solamente.
- Movimientos (Recepciones, Créditos, Notas) → `CalcularDesde(emp)` con prioridad
  `checkpoint → sinc_desde → 90 días`.

**Se mantiene el `MAX`** (es auto-reparable y no puede desincronizarse: el cursor *es* el dato,
así que un insert fallido nunca "se salta" una fila). Lo que hoy **no existe** y sí se necesita:
poder **forzar un re-jalado desde una fecha anterior**, porque el `MAX` nunca va hacia atrás.

**Decisión:** agregar un **override de un solo uso, aditivo al `MAX`**, por empresa × módulo,
que **gana sobre el checkpoint** cuando está presente, se aplica **una vez** y **el propio
Service lo borra** tras confirmarse el envío (auto-consumo: el operador no tiene que acordarse
de quitarlo). Y un botón **"Sincronizar ahora"** en el Configurador que dispara un ciclo
inmediato (brinca el timer por única vez), equivalente al que ya tiene el Visor.

## 1. Reglas de oro que aplican (no romper)

- **APIs compartidas con el Service.** El cambio a `GET /api/empresas` debe ser **aditivo y
  retrocompatible** (agregar un bloque `override`, nunca renombrar/quitar campos existentes),
  porque ese endpoint lo consume el Service en cada ciclo. Endpoints nuevos: libres.
  El **usuario sube el PHP manual** — el asistente NO sube a hosting.
- **WinForms `.Designer.cs` estricto.** El botón nuevo en `FormPrincipal.Designer.cs` solo con
  literales + `new` + `Controls.Add`. Nada de helpers, lambdas, `if`, `Tema.X` en el Designer.
- **Veracidad del Service = Delphi**, pero esta funcionalidad es nueva; el "forzar ciclo" ya
  está espejeado del Visor (mismo comando interno).
- **Elevación del Configurador por `--task`/runas, sin manifest.** El botón nuevo NO requiere
  elevación (solo hace `Set()` de un evento con nombre y un `PATCH` HTTP).

## 2. Parte A — Override de un solo uso

### 2.1 Modelo de datos (MySQL, producción)

Tabla nueva y chica (aditiva; el `MAX` sigue siendo el default):

```sql
CREATE TABLE SINC_OVERRIDE (
  EMP_FK  INT          NOT NULL,   -- = EMPRESAS_MSP.EMP_ID_MSP
  MODULO  VARCHAR(20)  NOT NULL,   -- almacenes|monedas|proveedores|recepciones|creditos|notas
  DESDE   DATETIME     NOT NULL,   -- fecha/hora desde la que se quiere re-jalar
  CREADO  DATETIME     NOT NULL,
  PRIMARY KEY (EMP_FK, MODULO)
);
```

- **Presencia de fila = override pendiente.** Sin fila = comportamiento normal (`MAX`).
- `MODULO` es el nombre del **sincronizador**, granular. Ojo: **Créditos y Notas comparten el
  `MAX` (tabla `CREDITOS`) pero son sincronizadores distintos** → override independiente para
  cada uno (dos filas posibles: `creditos` y `notas`).

### 2.2 Cambios en el portal (PHP / CI4)

- `EmpresaConfig` (C#) y el serializador PHP: agregar bloque **`override`** paralelo a
  `checkpoints` en la respuesta de `GET /api/empresas?solo_autorizadas=1`
  (`app/Controllers/Api/Empresas.php::serializarEmpresa` + `EmpresaMspModel::listarTodas`
  con un `LEFT JOIN`/subselect a `SINC_OVERRIDE`). Un campo por módulo, `null` si no hay override.
- Endpoints nuevos (en un controller `Api\SincOverride` o dentro de `Api\Empresas`):
  - `PUT /api/empresas/{idMsp}/override` — body `{ "modulo": "...", "desde": "YYYY-MM-DD HH:MM:SS" }`.
    Lo usa el **Configurador** para poner el override. Valida `modulo` contra whitelist y `desde`
    con `strtotime`. `UPSERT` por `(EMP_FK, MODULO)`.
  - `DELETE /api/empresas/{idMsp}/override/{modulo}` — lo usa el **Service** para consumirlo.
  - `GET /api/empresas/{idMsp}/override` — opcional, para que el Configurador muestre los
    pendientes en el modal.
- **Migración CI4** para crear `SINC_OVERRIDE` (las tablas `*_MSP` son legacy y viven fuera de
  migraciones; esta tabla es nueva del portal, así que sí conviene migración versionada).

### 2.3 Cambios en el Service (C#)

**Modelos** (`PortalProveedoresCore/Modelos/EmpresaConfig.cs`):
- Nueva clase `OverridesSinc` con `almacenes/monedas/proveedores/recepciones/creditos/notas`
  (mismo patrón snake_case que `CheckpointsCatalogos`).
- `EmpresaConfig.override_sinc` (o similar; cuidar que el nombre JSON coincida con el PHP).

**Prioridad nueva en TODOS los sincronizadores** (los 6). El orden pasa a ser:

```
1) override (un solo uso)   ← NUEVO, gana sobre el MAX
2) checkpoint (MAX del portal)
3) sinc_desde (solo movimientos)
4) 90 días / traer todo (fallback según módulo)
```

- Catálogos (`SincronizadorAlmacenes/Monedas/Proveedores`): hoy solo tienen
  `ParsearCheckpoint`; **agregarles la rama de override** al inicio.
- Movimientos (`SincronizadorRecepciones/Creditos/Notas`): anteponer la rama de override en
  `CalcularDesde`.
- El override, igual que el checkpoint, entra por `ParsearCheckpoint`/`+1s`? **No**: el override
  es una fecha elegida por el operador; se usa **tal cual** (sin `+1s`), porque la intención es
  "desde exactamente aquí". Confirmar en implementación si conviene el `+1s` o no (probablemente
  NO, para no dejar fuera el segundo elegido).

**Auto-consumo (crítico — no perder datos):**
- El Service borra el override **solo después** de que el portal confirmó el envío de ese módulo
  para esa empresa (tras `_api.SincronizarXAsync(...)` OK, aunque `inserted/updated` sean 0).
  Llamada: `DELETE /api/empresas/{idMsp}/override/{modulo}`.
- Si el ciclo truena antes del ack, el override sobrevive y se reintenta al siguiente ciclo
  (comportamiento deseado).

### 2.4 Cambios en el Configurador (C#)

- En el **modal por empresa** (donde ya se edita `sinc_desde`), agregar por módulo la opción de
  fijar un override de un uso (fecha/hora + módulo) → `PUT /api/empresas/{id}/override`.
- **NO** meterlo en la sección global de "Parámetros": el dato es por-empresa × por-módulo y ahí
  se sentiría fuera de lugar.
- Mostrar los overrides pendientes (badge/etiqueta) para que se vea que hay uno armado.

## 3. Parte B — Botón "Sincronizar ahora" en el Configurador

### 3.1 Lo que ya existe (verificado)

- Comando interno `CMD_FORZAR_CICLO = "cmd:forzar_ciclo"` (`Core/Pipes/TiposMensaje.cs`).
- El Visor lo manda por el pipe: `FormVisor.cs` → `_cliente.EnviarAsync(new ComandoForzarCiclo())`.
- El Service lo atiende en `ManejarComandoAsync` (`Service1.cs`): si ya hay ciclo en curso lo
  ignora; si no, llama `Despertar()`, que hace `TrySetResult` sobre `_wakeup`, y el loop
  `EsperarConWakeupAsync` (que espera `Task.WhenAny(delay, _wakeup.Task)`) despierta antes del
  timer. El intervalo normal sigue después → "brinca por única vez".

### 3.2 El gotcha del pipe (por qué NO reusar el pipe)

`ServidorPipe.CrearPipeServer()` usa `maxNumberOfServerInstances: 1` ("solo un visor a la vez").
Si el Visor está abierto (lo normal, vive en la bandeja), **ocupa el único slot** y el
Configurador no podría conectar al pipe. El ACL sí permitiría al Configurador (Interactive +
Administrators), pero el límite de instancias es el problema.

### 3.3 Solución elegida: `EventWaitHandle` con nombre global

Decisión del usuario: *"el que funcione de mejor manera sin causar conflicto o crasheo"*. Se
elige un **evento con nombre global**, desacoplado del pipe (el pipe se queda para el streaming
de eventos al Visor):

**Core (compartido)** — nuevo helper para centralizar nombre + ACL, p. ej.
`Core/Pipes/SenalForzarCiclo.cs` (o ampliar `ConstantesPipe`):
- Nombre: `Global\PortalProveedoresService_ForzarCiclo`.
- `EventResetMode.AutoReset` (cada `Set()` = un pulso).
- ACL (`EventWaitHandleSecurity`) espejando el del pipe: `LocalSystem` FullControl,
  `Interactive` y `BuiltinAdministrators` con `Modify | Synchronize`. Sin esto, el evento creado
  por LocalSystem no lo puede señalar el usuario interactivo.

**Service** (`Service1.OnStart`):
- Crear el `EventWaitHandle` con ese nombre y ACL.
- `ThreadPool.RegisterWaitForSingleObject(evento, callback, ...)` una sola vez; el callback
  ejecuta **la misma lógica guardada que `CMD_FORZAR_CICLO`** (revisar `_estadoLock` /
  `EstadoServicio.EjecutandoCiclo`, log, y `Despertar()`). Extraer esa lógica a un método
  privado `SolicitarForzarCiclo(string origen)` y llamarlo tanto desde el `case CMD_FORZAR_CICLO`
  como desde el callback del evento (DRY).
- Liberar el evento y el registro en `OnStop`.

**Configurador** (botón nuevo en la sección de Servicio de `FormPrincipal`):
- Al hacer clic: `EventWaitHandle.TryOpenExisting(nombre, ...)`; si existe → `Set()`;
  mostrar toast/label "Solicitud enviada — míralo en el Visor".
- Si `TryOpenExisting` devuelve false (o lanza `WaitHandleCannotBeOpenedException`) → el servicio
  no está corriendo → avisar "El servicio no está en ejecución".
- Fire-and-forget: no necesita streaming de eventos (para eso está el Visor). No puede crashear
  ni bloquear aunque el Visor esté conectado.
- Recordatorio Designer estricto: el `Button` va en `FormPrincipal.Designer.cs` solo con
  literales + `new` + `Controls.Add`; la lógica del clic en `FormPrincipal.cs`.

## 4. Orden de implementación sugerido

1. **PHP primero** (el Service depende del contrato): migración `SINC_OVERRIDE`, endpoints
   PUT/DELETE/GET override, y bloque `override` aditivo en `GET /api/empresas?solo_autorizadas=1`.
   Probar con curl/Postman. (El usuario sube el PHP a hosting manualmente.)
2. **Core**: modelos `OverridesSinc` en `EmpresaConfig`; helper `SenalForzarCiclo` (nombre+ACL).
3. **Service**: rama de override en los 6 sincronizadores + auto-consumo (DELETE tras ack);
   `EventWaitHandle` en `OnStart`/`OnStop` + `SolicitarForzarCiclo` reutilizado.
4. **Configurador**: override por módulo en el modal de empresa; botón "Sincronizar ahora".
5. **Build Release** (matar Visor/Escritorio/Configurador antes) y, si aplica, regenerar instalador.

## 5. Pruebas / criterios de aceptación

- **Override 1 uso:** poner override en `recepciones` de una empresa con fecha vieja → el ciclo
  jala desde esa fecha; al terminar, la fila de `SINC_OVERRIDE` **desaparece**; el siguiente
  ciclo vuelve al `MAX` (no se queda pegado en la fecha vieja).
- **Override + crash:** si el envío al portal falla, el override **permanece** y se reintenta.
- **Créditos vs Notas:** override en `creditos` NO afecta a `notas` y viceversa, aunque compartan
  el `MAX`.
- **Botón con Visor abierto:** "Sincronizar ahora" dispara el ciclo aunque el Visor tenga el pipe
  ocupado; no crashea ni el Configurador ni el Service.
- **Botón sin servicio:** avisa "servicio no está corriendo", sin excepción sin manejar.
- **Ciclo en curso:** si ya hay un ciclo corriendo, el botón/evento se ignora con warning (igual
  que hoy `CMD_FORZAR_CICLO`).
- **Retrocompatibilidad API:** un Service viejo (sin conocer `override`) sigue funcionando contra
  el portal nuevo (campo ignorado).

## 6. Archivos que se tocan (referencia rápida)

**PHP** (`C:\wamp\www\PortalProveedores`):
- `app/Database/Migrations/*_CreateSincOverride.php` (nuevo)
- `app/Models/EmpresaMspModel.php` (subselect/join override en `listarTodas`)
- `app/Controllers/Api/Empresas.php` (`serializarEmpresa` + endpoints override) o `Api/SincOverride.php`
- `app/Config/Routes.php` (rutas nuevas)

**C#** (`...\Nuevo\PortalProveedoresService`):
- `PortalProveedoresCore/Modelos/EmpresaConfig.cs` (`OverridesSinc`)
- `PortalProveedoresCore/Pipes/SenalForzarCiclo.cs` (nuevo) o `ConstantesPipe.cs`
- `PortalProveedoresCore/Servicios/IPortalApi.cs` + `PortalApi.cs` (DELETE override; PUT si el Configurador lo usa vía Core)
- `PortalProveedoresService/Sincronizacion/Sincronizador{Almacenes,Monedas,Proveedores,Recepciones,Creditos,Notas}.cs`
- `PortalProveedoresService/Service1.cs` (`OnStart/OnStop`, `SolicitarForzarCiclo`)
- `PortalProveedoresConfigurador/Formularios/FormPrincipal.cs` + `FormPrincipal.Designer.cs` (botón; override en modal empresa)
