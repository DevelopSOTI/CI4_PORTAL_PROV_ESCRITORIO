; =============================================================================
;  Portal Proveedores SOTI - Instalador unico (Inno Setup 6)
; =============================================================================
;
;  Sistema: 4 apps .NET Framework 4.8 WinForms + DLL Core compartida.
;    - PortalProveedoresCore.dll          (compartida, va SIEMPRE)
;    - PortalProveedoresConfigurador.exe  (siempre)
;    - PortalProveedoresEscritorio.exe    (cliente)
;    - PortalProveedoresVisor.exe         (servidor)
;    - PortalProveedoresService.exe       (servidor, servicio Windows)
;
;  Destino unico: C:\SOTI\PORTAL_PROVEEDORES (todas las apps en la misma carpeta,
;  por eso una sola copia de cada DLL compartida basta).
;
;  Vista de registro: HKLM nativo de 64 bits. Las apps leen
;  HKLM\SOFTWARE\SOTI\Service Portal con RegistryView.Registry64, asi que el
;  instalador DEBE escribir en la hive nativa (no WOW6432Node). Se logra con
;  ArchitecturesInstallIn64BitMode=x64compatible -> las escrituras [Registry]
;  HKLM caen en la vista de 64 bits.
;
;  --- DECISION: Auto-arranque del Visor (investigado en el codigo fuente) ---
;  El Visor (PortalProveedoresVisor\Program.cs) ES single-instance: usa un
;  Mutex con nombre "PortalProveedoresVisor_SingleInstance"; una 2da instancia
;  sale en silencio. ADEMAS, en cada arranque se auto-registra (idempotente) en
;  HKCU\...\Run con la bandera --tray mediante AutoArranque.AsegurarRegistroEnHKCU().
;  El propio codigo (AutoArranque.cs) documenta que el auto-arranque es por
;  diseno PER-USER via HKCU y que HKLM NO es deseado (requeriria admin y
;  aplicaria a todos los usuarios, comportamiento no buscado para una app de
;  monitoreo personal).
;  Por tanto el instalador NO agrega una entrada HKLM\...\Run. La estrategia es:
;  para servidor/completo se ofrece al final un checkbox "Iniciar el Visor ahora";
;  al lanzarlo, el Visor se auto-registra en HKCU para los siguientes inicios de
;  sesion de Windows. Asi arranca solo al iniciar sesion, SIN doble lanzamiento y
;  respetando el diseno per-user de la app. (Un servicio en Session 0 no puede
;  abrir UI; por eso el Visor va por arranque de sesion de usuario, no lanzado
;  por el servicio.)
; =============================================================================

#define MyAppName        "Portal Proveedores SOTI"
#define MyAppPublisher   "SOTI"
#define MyAppVersion     "1.0.0"
#define MyInstallDir     "C:\SOTI\PORTAL_PROVEEDORES"
#define RegKeyPath       "SOFTWARE\SOTI\Service Portal"

; Raiz de los binarios ya compilados en Release.
#define BinRoot          "C:\Users\Desarrollo\source\repos\PortalProveedoresServicioEmbarcadero\Nuevo\PortalProveedoresService"
#define CoreBin          BinRoot + "\PortalProveedoresCore\bin\Release"
#define ConfigBin        BinRoot + "\PortalProveedoresConfigurador\bin\Release"
#define VisorBin         BinRoot + "\PortalProveedoresVisor\bin\Release"
#define ServiceBin       BinRoot + "\PortalProveedoresService\bin\Release"
#define EscritorioBin    BinRoot + "\PortalProveedoresEscritorio\bin\Release"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={#MyInstallDir}
DefaultGroupName=Portal Proveedores
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
OutputDir=Output
OutputBaseFilename=PortalProveedores-Setup
UninstallDisplayName={#MyAppName}

[Languages]
; En Inno Setup 6.7.x el archivo de espanol se llama Spanish.isl (antes
; existia SpanishStandard.isl). Si compilas con una version distinta que use
; el nombre antiguo, cambia esta linea a compiler:Languages\SpanishStandard.isl.
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"

; =============================================================================
;  TYPES (modos de instalacion) y COMPONENTS
;
;  cliente  -> core, configurador, escritorio
;  servidor -> core, configurador, visor, service
;  completo -> todos
; =============================================================================
[Types]
Name: "cliente";  Description: "Cliente (Escritorio + Configurador)"
Name: "servidor"; Description: "Servidor (Servicio + Visor + Configurador)"
Name: "completo"; Description: "Completo (Ambos)"

[Components]
; core: fijo en los 3 types (no se puede desmarcar)
Name: "core";         Description: "Componentes base (Core + Firebird)";        Types: cliente servidor completo; Flags: fixed
Name: "configurador"; Description: "Configurador";                              Types: cliente servidor completo
Name: "escritorio";   Description: "Portal Proveedores Escritorio";            Types: cliente completo
Name: "visor";        Description: "Visor de actividad del servicio";          Types: servidor completo
Name: "service";      Description: "Servicio Windows (sincronizacion)";        Types: servidor completo

[Files]
; --- Core compartido (SIEMPRE) ---
Source: "{#CoreBin}\PortalProveedoresCore.dll";              DestDir: "{app}"; Flags: ignoreversion; Components: core
Source: "{#CoreBin}\FirebirdSql.Data.FirebirdClient.dll";    DestDir: "{app}"; Flags: ignoreversion; Components: core

; --- Configurador ---
Source: "{#ConfigBin}\PortalProveedoresConfigurador.exe";        DestDir: "{app}"; Flags: ignoreversion; Components: configurador
Source: "{#ConfigBin}\PortalProveedoresConfigurador.exe.config"; DestDir: "{app}"; Flags: ignoreversion; Components: configurador

; --- Escritorio (cliente, completo) + WebView2 ---
Source: "{#EscritorioBin}\PortalProveedoresEscritorio.exe";        DestDir: "{app}"; Flags: ignoreversion; Components: escritorio
Source: "{#EscritorioBin}\PortalProveedoresEscritorio.exe.config"; DestDir: "{app}"; Flags: ignoreversion; Components: escritorio
Source: "{#EscritorioBin}\Microsoft.Web.WebView2.Core.dll";        DestDir: "{app}"; Flags: ignoreversion; Components: escritorio
Source: "{#EscritorioBin}\Microsoft.Web.WebView2.WinForms.dll";    DestDir: "{app}"; Flags: ignoreversion; Components: escritorio
Source: "{#EscritorioBin}\Microsoft.Web.WebView2.Wpf.dll";         DestDir: "{app}"; Flags: ignoreversion; Components: escritorio
; Loaders nativos WebView2 (carpeta runtimes\ con las 3 arquitecturas)
Source: "{#EscritorioBin}\runtimes\*"; DestDir: "{app}\runtimes"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: escritorio

; --- Visor (servidor, completo) ---
Source: "{#VisorBin}\PortalProveedoresVisor.exe";        DestDir: "{app}"; Flags: ignoreversion; Components: visor
Source: "{#VisorBin}\PortalProveedoresVisor.exe.config"; DestDir: "{app}"; Flags: ignoreversion; Components: visor

; --- Service (servidor, completo) ---
Source: "{#ServiceBin}\PortalProveedoresService.exe";        DestDir: "{app}"; Flags: ignoreversion; Components: service
Source: "{#ServiceBin}\PortalProveedoresService.exe.config"; DestDir: "{app}"; Flags: ignoreversion; Components: service

; --- dontcopy: binarios usados SOLO durante el wizard para "Probar conexion".
;     Se extraen a {tmp} con ExtractTemporaryFile antes de copiar la instalacion.
;     probar-microsip necesita FirebirdSql junto al exe; probar-portal solo Core.
;     Extraemos los 3 por si acaso. NO se instalan (flag dontcopy). ---
Source: "{#ConfigBin}\PortalProveedoresConfigurador.exe";     DestDir: "{tmp}"; Flags: dontcopy
Source: "{#ConfigBin}\PortalProveedoresCore.dll";             DestDir: "{tmp}"; Flags: dontcopy
Source: "{#ConfigBin}\FirebirdSql.Data.FirebirdClient.dll";   DestDir: "{tmp}"; Flags: dontcopy

[Icons]
; Accesos directos en el menu inicio.
Name: "{group}\Configurador Portal Proveedores"; Filename: "{app}\PortalProveedoresConfigurador.exe"; Components: configurador
Name: "{group}\Portal Proveedores Escritorio";   Filename: "{app}\PortalProveedoresEscritorio.exe";   Components: escritorio
Name: "{group}\Visor Portal Proveedores";        Filename: "{app}\PortalProveedoresVisor.exe";        Components: visor
Name: "{group}\Desinstalar Portal Proveedores";  Filename: "{uninstallexe}"

; =============================================================================
;  REGISTRO (HKLM, vista nativa 64-bit por ArchitecturesInstallIn64BitMode)
;  Nombres EXACTOS tomados de TareasElevadas.cs. Todos REG_SZ.
;  Los valores provienen de las paginas custom del wizard (ver [Code]).
; =============================================================================
[Registry]
; --- Siempre (cliente y servidor): Microsip + Portal ---
Root: HKLM; Subkey: "{#RegKeyPath}"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "{#RegKeyPath}"; ValueType: string; ValueName: "MICRO_SERV";      ValueData: "{code:GetMicroServ}";  Flags: uninsdeletevalue
Root: HKLM; Subkey: "{#RegKeyPath}"; ValueType: string; ValueName: "MICRO_ROOT";      ValueData: "{code:GetMicroRoot}";  Flags: uninsdeletevalue
Root: HKLM; Subkey: "{#RegKeyPath}"; ValueType: string; ValueName: "MICRO_USER";      ValueData: "{code:GetMicroUser}";  Flags: uninsdeletevalue
Root: HKLM; Subkey: "{#RegKeyPath}"; ValueType: string; ValueName: "MICRO_PASS";      ValueData: "{code:GetMicroPass}";  Flags: uninsdeletevalue
Root: HKLM; Subkey: "{#RegKeyPath}"; ValueType: string; ValueName: "PORTAL_BASE_URL"; ValueData: "{code:GetPortalUrl}";  Flags: uninsdeletevalue
Root: HKLM; Subkey: "{#RegKeyPath}"; ValueType: string; ValueName: "PORTAL_API_KEY";  ValueData: "{code:GetPortalKey}";  Flags: uninsdeletevalue

; --- Solo servidor/completo (component service) ---
Root: HKLM; Subkey: "{#RegKeyPath}"; ValueType: string; ValueName: "SERVICE_NAME";   ValueData: "{code:GetServiceName}";  Flags: uninsdeletevalue; Components: service
Root: HKLM; Subkey: "{#RegKeyPath}"; ValueType: string; ValueName: "RUTA_ARCHIVOS";  ValueData: "{code:GetRutaArchivos}"; Flags: uninsdeletevalue; Components: service
Root: HKLM; Subkey: "{#RegKeyPath}"; ValueType: string; ValueName: "MODE_TIMER";     ValueData: "{code:GetModeTimer}";    Flags: uninsdeletevalue; Components: service

; =============================================================================
;  RUN (post-instalacion)
; =============================================================================
[Run]
; --- Servicio Windows: crear + iniciar (solo servidor/completo) ---
; Replica de TareasElevadas.InstalarServicio: 'binPath= "..."' (espacio tras '=').
Filename: "{sys}\sc.exe"; \
  Parameters: "create ""{code:GetServiceName}"" binPath= ""{app}\PortalProveedoresService.exe"" start= auto DisplayName= ""{code:GetServiceName} (Portal Proveedores)"""; \
  Flags: runhidden waituntilterminated; \
  StatusMsg: "Registrando el servicio Windows..."; \
  Components: service
Filename: "{sys}\sc.exe"; \
  Parameters: "start ""{code:GetServiceName}"""; \
  Flags: runhidden waituntilterminated; \
  StatusMsg: "Iniciando el servicio Windows..."; \
  Components: service

; --- Checkbox final (servidor/completo): abrir el Configurador ---
; El servicio sincroniza primero las empresas; el operador las autoriza despues
; en el Configurador. NO se selecciona empresas en el wizard.
Filename: "{app}\PortalProveedoresConfigurador.exe"; \
  Description: "Abrir el Configurador para autorizar empresas y dias"; \
  Flags: postinstall nowait skipifsilent; \
  Components: configurador; \
  Check: EsServidorOCompleto

; --- Checkbox final (servidor/completo): iniciar el Visor ahora ---
; Al lanzarlo, el Visor se auto-registra en HKCU\...\Run (--tray) para los
; siguientes inicios de sesion. Ver decision en el encabezado de este archivo.
Filename: "{app}\PortalProveedoresVisor.exe"; \
  Description: "Iniciar el Visor ahora (se autoarrancara en los siguientes inicios de sesion)"; \
  Flags: postinstall nowait skipifsilent; \
  Components: visor

; --- Checkbox final (cliente): iniciar el Escritorio ---
Filename: "{app}\PortalProveedoresEscritorio.exe"; \
  Description: "Iniciar Portal Proveedores Escritorio"; \
  Flags: postinstall nowait skipifsilent; \
  Components: escritorio; \
  Check: EsCliente

; =============================================================================
;  UNINSTALL: detener y borrar el servicio antes de quitar archivos.
; =============================================================================
[UninstallRun]
Filename: "{sys}\sc.exe"; Parameters: "stop ""{code:GetServiceNameUninstall}"""; \
  Flags: runhidden waituntilterminated; RunOnceId: "StopSvc"; Components: service
Filename: "{sys}\sc.exe"; Parameters: "delete ""{code:GetServiceNameUninstall}"""; \
  Flags: runhidden waituntilterminated; RunOnceId: "DelSvc"; Components: service

; =============================================================================
;  CODE
; =============================================================================
[Code]
var
  PageMicrosip: TInputQueryWizardPage;
  PagePortal:   TInputQueryWizardPage;
  PageServicio: TInputQueryWizardPage;
  BtnProbarMicrosip: TNewButton;
  BtnProbarPortal:   TNewButton;

const
  NET48_RELEASE = 528040;

// ---------------------------------------------------------------------------
//  Prerrequisito .NET Framework 4.8 (lectura HKLM 64-bit).
// ---------------------------------------------------------------------------
function InitializeSetup(): Boolean;
var
  Release: Cardinal;
begin
  Result := True;
  if not RegQueryDWordValue(HKLM64,
      'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) then
  begin
    MsgBox('Este sistema requiere Microsoft .NET Framework 4.8 o superior, ' +
           'pero no se detecto ninguna instalacion de .NET Framework 4.' + #13#10 + #13#10 +
           'Instale .NET Framework 4.8 y vuelva a ejecutar este instalador.',
           mbCriticalError, MB_OK);
    Result := False;
    Exit;
  end;
  if Release < NET48_RELEASE then
  begin
    MsgBox('Este sistema requiere Microsoft .NET Framework 4.8 o superior.' + #13#10 + #13#10 +
           'La version instalada es anterior a 4.8 (Release=' + IntToStr(Release) + ').' + #13#10 +
           'Actualice a .NET Framework 4.8 y vuelva a ejecutar este instalador.',
           mbCriticalError, MB_OK);
    Result := False;
  end;
end;

// ---------------------------------------------------------------------------
//  Helpers de seleccion de modo.
// ---------------------------------------------------------------------------
function EsServidorOCompleto(): Boolean;
begin
  Result := WizardIsComponentSelected('service');
end;

function EsCliente(): Boolean;
begin
  // "Cliente puro": escritorio seleccionado y servicio NO.
  Result := WizardIsComponentSelected('escritorio') and (not WizardIsComponentSelected('service'));
end;

// ---------------------------------------------------------------------------
//  "Probar conexion": extrae el Configurador + DLLs a {tmp} y lo ejecuta
//  headless. Lee el ResultCode (0=OK, 1=fallo, 2=args). Es OPCIONAL (boton).
// ---------------------------------------------------------------------------
procedure ExtraerHerramientasPrueba();
begin
  // Idempotente: ExtractTemporaryFile vuelve a extraer sin problema.
  ExtractTemporaryFile('PortalProveedoresConfigurador.exe');
  ExtractTemporaryFile('PortalProveedoresCore.dll');
  ExtractTemporaryFile('FirebirdSql.Data.FirebirdClient.dll');
end;

procedure OnProbarPortal(Sender: TObject);
var
  ResultCode: Integer;
  Url, Key: string;
begin
  Url := Trim(PagePortal.Values[0]);
  Key := Trim(PagePortal.Values[1]);
  if (Url = '') or (Key = '') then
  begin
    MsgBox('Capture la URL del portal y la API key antes de probar.', mbError, MB_OK);
    Exit;
  end;
  ExtraerHerramientasPrueba();
  if not Exec(ExpandConstant('{tmp}\PortalProveedoresConfigurador.exe'),
        '--probar-portal "' + Url + '" "' + Key + '"',
        ExpandConstant('{tmp}'), SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('No se pudo ejecutar el Configurador para probar el portal.', mbError, MB_OK);
    Exit;
  end;
  case ResultCode of
    0: MsgBox('Conexion al portal correcta. La URL es alcanzable y la API key es valida.', mbInformation, MB_OK);
    2: MsgBox('Argumentos invalidos al probar el portal (faltan URL o API key).', mbError, MB_OK);
  else
    MsgBox('No se pudo conectar al portal. Verifique la URL y la API key.', mbError, MB_OK);
  end;
end;

procedure OnProbarMicrosip(Sender: TObject);
var
  ResultCode: Integer;
  Srv, Root, User, Pass: string;
begin
  Srv  := Trim(PageMicrosip.Values[0]);
  Root := Trim(PageMicrosip.Values[1]);
  User := Trim(PageMicrosip.Values[2]);
  Pass := PageMicrosip.Values[3];
  if (Srv = '') or (Root = '') or (User = '') then
  begin
    MsgBox('Capture servidor, ruta y usuario de Microsip antes de probar.', mbError, MB_OK);
    Exit;
  end;
  ExtraerHerramientasPrueba();
  if not Exec(ExpandConstant('{tmp}\PortalProveedoresConfigurador.exe'),
        '--probar-microsip "' + Srv + '" "' + Root + '" "' + User + '" "' + Pass + '"',
        ExpandConstant('{tmp}'), SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('No se pudo ejecutar el Configurador para probar Microsip.', mbError, MB_OK);
    Exit;
  end;
  case ResultCode of
    0: MsgBox('Conexion a Microsip correcta.', mbInformation, MB_OK);
    2: MsgBox('Argumentos invalidos al probar Microsip (faltan datos).', mbError, MB_OK);
  else
    MsgBox('No se pudo conectar a Microsip. Verifique servidor, ruta y credenciales.', mbError, MB_OK);
  end;
end;

// ---------------------------------------------------------------------------
//  Construccion de las paginas custom del wizard.
// ---------------------------------------------------------------------------
procedure InitializeWizard();
begin
  // --- Pagina Microsip ---
  PageMicrosip := CreateInputQueryPage(wpSelectComponents,
    'Conexion a Microsip',
    'Datos para conectar a la base de datos Microsip (Firebird).',
    'Estos valores se guardan en HKLM y los usan tanto el cliente como el servidor. ' +
    'Puede usar el boton "Probar Microsip" para verificar (opcional, recomendado).');
  PageMicrosip.Add('Servidor Microsip (p.ej. localhost):', False);
  PageMicrosip.Add('Ruta raiz Microsip (carpeta "Microsip datos"):', False);
  PageMicrosip.Add('Usuario:', False);
  PageMicrosip.Add('Password:', True);  // True => PasswordChar
  PageMicrosip.Values[0] := 'localhost';
  PageMicrosip.Values[2] := 'SYSDBA';
  PageMicrosip.Values[3] := 'masterkey';

  BtnProbarMicrosip := TNewButton.Create(PageMicrosip);
  BtnProbarMicrosip.Parent := PageMicrosip.Surface;
  BtnProbarMicrosip.Caption := 'Probar Microsip';
  BtnProbarMicrosip.Width := ScaleX(120);
  BtnProbarMicrosip.Height := ScaleY(25);
  BtnProbarMicrosip.Left := PageMicrosip.SurfaceWidth - BtnProbarMicrosip.Width;
  BtnProbarMicrosip.Top := PageMicrosip.SurfaceHeight - BtnProbarMicrosip.Height;
  BtnProbarMicrosip.OnClick := @OnProbarMicrosip;

  // --- Pagina Portal ---
  PagePortal := CreateInputQueryPage(PageMicrosip.ID,
    'Portal Web',
    'Datos de conexion al portal de proveedores (REST).',
    'Estos valores se guardan en HKLM. Use "Probar portal" para verificar la URL y ' +
    'la API key (opcional, recomendado).');
  PagePortal.Add('URL base del portal (p.ej. https://proveedores.soti.com.mx):', False);
  PagePortal.Add('API key:', False);

  BtnProbarPortal := TNewButton.Create(PagePortal);
  BtnProbarPortal.Parent := PagePortal.Surface;
  BtnProbarPortal.Caption := 'Probar portal';
  BtnProbarPortal.Width := ScaleX(120);
  BtnProbarPortal.Height := ScaleY(25);
  BtnProbarPortal.Left := PagePortal.SurfaceWidth - BtnProbarPortal.Width;
  BtnProbarPortal.Top := PagePortal.SurfaceHeight - BtnProbarPortal.Height;
  BtnProbarPortal.OnClick := @OnProbarPortal;

  // --- Pagina Servicio (solo se muestra si el component service esta activo;
  //     ver ShouldSkipPage). ---
  PageServicio := CreateInputQueryPage(PagePortal.ID,
    'Servicio Windows',
    'Configuracion del servicio de sincronizacion (solo servidor).',
    'El intervalo se mide en SEGUNDOS y lo lee el servicio desde HKLM (MODE_TIMER).');
  PageServicio.Add('Nombre del servicio:', False);
  PageServicio.Add('Intervalo de sincronizacion (segundos):', False);
  PageServicio.Values[0] := 'PortalProveedoresService';
  PageServicio.Values[1] := '300';
end;

// La pagina de Servicio solo aplica si el Type incluye el component "service".
function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if (PageID = PageServicio.ID) then
    Result := not WizardIsComponentSelected('service');
end;

// ---------------------------------------------------------------------------
//  Validacion: no avanzar si faltan campos obligatorios. Probar conexion es
//  opcional (boton), no obligatorio para avanzar.
// ---------------------------------------------------------------------------
function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;

  if CurPageID = PageMicrosip.ID then
  begin
    if (Trim(PageMicrosip.Values[0]) = '') or
       (Trim(PageMicrosip.Values[1]) = '') or
       (Trim(PageMicrosip.Values[2]) = '') then
    begin
      MsgBox('Capture el servidor, la ruta raiz y el usuario de Microsip.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
  end;

  if CurPageID = PagePortal.ID then
  begin
    if (Trim(PagePortal.Values[0]) = '') or (Trim(PagePortal.Values[1]) = '') then
    begin
      MsgBox('Capture la URL del portal y la API key.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
  end;

  if (CurPageID = PageServicio.ID) and WizardIsComponentSelected('service') then
  begin
    if Trim(PageServicio.Values[0]) = '' then
    begin
      MsgBox('Capture el nombre del servicio.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
    if StrToIntDef(Trim(PageServicio.Values[1]), -1) <= 0 then
    begin
      MsgBox('El intervalo del servicio debe ser un numero de segundos mayor que cero.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
  end;
end;

// ---------------------------------------------------------------------------
//  Getters usados por [Registry], [Run] y [UninstallRun] via {code:...}.
// ---------------------------------------------------------------------------
function GetMicroServ(Param: string): string; begin Result := Trim(PageMicrosip.Values[0]); end;
function GetMicroRoot(Param: string): string; begin Result := Trim(PageMicrosip.Values[1]); end;
function GetMicroUser(Param: string): string; begin Result := Trim(PageMicrosip.Values[2]); end;
function GetMicroPass(Param: string): string; begin Result := PageMicrosip.Values[3]; end;
function GetPortalUrl(Param: string): string;  begin Result := Trim(PagePortal.Values[0]); end;
function GetPortalKey(Param: string): string;  begin Result := Trim(PagePortal.Values[1]); end;

function GetServiceName(Param: string): string;
begin
  Result := Trim(PageServicio.Values[0]);
  if Result = '' then Result := 'PortalProveedoresService';
end;

function GetModeTimer(Param: string): string;
begin
  Result := Trim(PageServicio.Values[1]);
  if Result = '' then Result := '300';
end;

// RUTA_ARCHIVOS: por default la carpeta de instalacion ({app}).
function GetRutaArchivos(Param: string): string;
begin
  Result := ExpandConstant('{app}');
end;

// Para el desinstalador: lee SERVICE_NAME desde HKLM (las paginas del wizard
// no existen durante la desinstalacion).
function GetServiceNameUninstall(Param: string): string;
begin
  if not RegQueryStringValue(HKLM64, '{#RegKeyPath}', 'SERVICE_NAME', Result) then
    Result := 'PortalProveedoresService';
  if Trim(Result) = '' then
    Result := 'PortalProveedoresService';
end;
