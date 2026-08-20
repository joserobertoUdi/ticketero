<#
.SYNOPSIS
    Prepara y levanta la API Ticketero en un entorno LOCAL de desarrollo.

.DESCRIPTION
    Pasos que ejecuta:
      1. Verifica requisitos (.NET SDK 10, SQL Server accesible).
      2. Restaura la base desde respaldo.bak  (SOLO con -Restaurar).
      3. Define las variables de entorno (ambito User, no requiere admin).
      4. Valida la conexion y contrasta las tablas contra el modelo EF Core.
      5. Levanta la API y comprueba /healthz y /ready.

    Autenticacion: intenta autenticacion de Windows. Si falla, pide credenciales
    SQL (usuario sa). Se puede forzar con -Usuario sa.

.EXAMPLE
    .\Levantar-Local.ps1
    Flujo normal: variables + validacion + levantar la API. NO toca la base de datos.

.EXAMPLE
    .\Levantar-Local.ps1 -Usuario sa
    Igual, pero pide directamente la contrasena de sa (sin probar Windows Auth).

.EXAMPLE
    .\Levantar-Local.ps1 -SoloValidar
    Solo define variables y valida la conexion, sin levantar la API.

.EXAMPLE
    .\Levantar-Local.ps1 -Restaurar -BaseDatos DBTicketero
    Restaura respaldo.bak SOBRESCRIBIENDO la base indicada, y luego levanta la API.
#>

[CmdletBinding()]
param(
    [string] $Instancia   = 'localhost',
    [string] $BaseDatos   = 'DBTicketero_Dev',
    [string] $Usuario     = '',
    [string] $ArchivoBak,
    [string] $JwtKey      = 'TicketeroDevLocal2026!@#$%^&*()MinLength32Chars',
    [int]    $Puerto      = 5000,
    [switch] $Restaurar,
    [switch] $SoloValidar
)

$ErrorActionPreference = 'Stop'

# --------------------------------------------------------------------------
# Rutas
# --------------------------------------------------------------------------
$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path   # ...\Ticketero\scripts
$RaizApi     = Split-Path -Parent $ScriptDir                     # ...\Ticketero
$ProyectoApi = Join-Path $RaizApi 'src\Ticketero.Api'
if (-not $ArchivoBak) {
    $ArchivoBak = Join-Path (Split-Path -Parent $RaizApi) 'respaldo.bak'   # ...\ApiTiketero\respaldo.bak
}

# --------------------------------------------------------------------------
# Salida
# --------------------------------------------------------------------------
function Write-Paso  { param($m) Write-Host "`n==> $m" -ForegroundColor Cyan }
function Write-Ok    { param($m) Write-Host "    [OK] $m" -ForegroundColor Green }
function Write-Info  { param($m) Write-Host "    $m"     -ForegroundColor Gray }
function Write-Aviso { param($m) Write-Host "    [!] $m" -ForegroundColor Yellow }

# --------------------------------------------------------------------------
# Cliente SQL: .NET Framework (Windows PowerShell 5.1) o Microsoft.Data.SqlClient (PS7 + modulo SqlServer)
# --------------------------------------------------------------------------
$TipoConexion = $null
foreach ($t in @('System.Data.SqlClient.SqlConnection', 'Microsoft.Data.SqlClient.SqlConnection')) {
    if ($t -as [type]) { $TipoConexion = ($t -as [type]).FullName; break }
}
if (-not $TipoConexion -and (Get-Module -ListAvailable -Name SqlServer)) {
    Import-Module SqlServer -ErrorAction SilentlyContinue
    if ('Microsoft.Data.SqlClient.SqlConnection' -as [type]) { $TipoConexion = 'Microsoft.Data.SqlClient.SqlConnection' }
}
if (-not $TipoConexion) {
    throw "No hay cliente SQL disponible. Ejecuta desde Windows PowerShell 5.1 (powershell.exe) o instala el modulo SqlServer: Install-Module SqlServer -Scope CurrentUser"
}
$TipoBuilder = $TipoConexion -replace 'SqlConnection$', 'SqlConnectionStringBuilder'

function New-CadenaConexion {
    param(
        [string] $Base,
        [System.Management.Automation.PSCredential] $Credencial,
        [int] $Timeout = 15
    )
    $b = New-Object $script:TipoBuilder
    $b['Data Source']     = $script:Instancia
    $b['Initial Catalog'] = $Base
    $b['Connect Timeout'] = $Timeout
    $b['TrustServerCertificate']    = $true
    $b['MultipleActiveResultSets']  = $true
    if ($Credencial) {
        $b['Integrated Security'] = $false
        $b['User ID']  = $Credencial.UserName
        $b['Password'] = $Credencial.GetNetworkCredential().Password
    }
    else {
        $b['Integrated Security'] = $true
    }
    return $b.ConnectionString
}

function Invoke-Sql {
    param(
        [Parameter(Mandatory)] [string] $Query,
        [Parameter(Mandatory)] [string] $CadenaConexion,
        [int] $TimeoutSegundos = 900
    )
    $cn = New-Object $script:TipoConexion $CadenaConexion
    try {
        $cn.Open()
        $cmd = $cn.CreateCommand()
        $cmd.CommandText    = $Query
        $cmd.CommandTimeout = $TimeoutSegundos
        $dt = New-Object System.Data.DataTable
        $rd = $cmd.ExecuteReader()
        try { if (-not $rd.IsClosed -and $rd.FieldCount -gt 0) { $dt.Load($rd) } }
        finally { $rd.Dispose() }
        if ($dt.Rows.Count -eq 0) { return $null }
        return $dt
    }
    finally { $cn.Dispose() }
}

Write-Host "`n===========================================" -ForegroundColor White
Write-Host " Ticketero - Entorno LOCAL de desarrollo"     -ForegroundColor White
Write-Host "===========================================" -ForegroundColor White
Write-Info "Instancia SQL : $Instancia"
Write-Info "Base de datos : $BaseDatos"
Write-Info "Proyecto API  : $ProyectoApi"

# --------------------------------------------------------------------------
# 1. Requisitos y autenticacion
# --------------------------------------------------------------------------
Write-Paso '1/5  Verificando requisitos'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "No se encontro 'dotnet'. Instala el SDK de .NET 10: https://dotnet.microsoft.com/download"
}
$sdks  = & dotnet --list-sdks
$sdk10 = $sdks | Where-Object { $_ -match '^10\.' }
if (-not $sdk10) {
    Write-Aviso 'El proyecto tiene TargetFramework net10.0 y no se detecto un SDK 10.x.'
    Write-Info  'SDKs instalados:'
    $sdks | ForEach-Object { Write-Info "  $_" }
    throw 'Instala el SDK de .NET 10 antes de continuar.'
}
Write-Ok ".NET SDK: $($sdk10 | Select-Object -First 1)"

# Resolver credenciales
$Credencial = $null
$conectado  = $false

if (-not $Usuario) {
    try {
        $probe = Invoke-Sql -Query 'SELECT 1 AS x' -CadenaConexion (New-CadenaConexion -Base 'master') -TimeoutSegundos 15
        $conectado = $true
        Write-Ok 'Autenticacion de Windows aceptada.'
    }
    catch {
        Write-Aviso 'Autenticacion de Windows rechazada; se usara autenticacion SQL.'
        $Usuario = 'sa'
    }
}

if (-not $conectado) {
    $Credencial = Get-Credential -UserName $Usuario -Message "Contrasena de SQL Server para '$Usuario' en $Instancia"
    if (-not $Credencial) { throw 'Se cancelo la solicitud de credenciales.' }
    $null = Invoke-Sql -Query 'SELECT 1 AS x' -CadenaConexion (New-CadenaConexion -Base 'master' -Credencial $Credencial) -TimeoutSegundos 15
    Write-Ok "Autenticacion SQL aceptada para '$($Credencial.UserName)'."
}

$ConnMaster = New-CadenaConexion -Base 'master'    -Credencial $Credencial
$ConnApi    = New-CadenaConexion -Base $BaseDatos  -Credencial $Credencial -Timeout 30

$ver = Invoke-Sql -Query "SELECT SERVERPROPERTY('ProductVersion') AS v, SERVERPROPERTY('Edition') AS e" -CadenaConexion $ConnMaster -TimeoutSegundos 30
Write-Ok "SQL Server: $($ver.v) / $($ver.e)"

# --------------------------------------------------------------------------
# 2. Restaurar respaldo.bak  (opt-in)
# --------------------------------------------------------------------------
if (-not $Restaurar -or $SoloValidar) {
    Write-Paso '2/5  Restauracion omitida (usa -Restaurar para sobrescribir la base)'
}
else {
    Write-Paso "2/5  Restaurando [$BaseDatos] desde respaldo.bak"

    if (-not (Test-Path $ArchivoBak)) { throw "No se encontro el respaldo: $ArchivoBak" }
    $bak = (Resolve-Path $ArchivoBak).Path
    Write-Info "Archivo: $bak ($([math]::Round((Get-Item $bak).Length / 1MB, 1)) MB)"
    Write-Aviso "Esto DESTRUYE el contenido actual de [$BaseDatos]."
    if ((Read-Host "Escribe SI para continuar") -ne 'SI') { throw 'Restauracion cancelada por el usuario.' }

    $bakEsc = $bak.Replace("'", "''")

    try {
        $fileList = Invoke-Sql -Query "RESTORE FILELISTONLY FROM DISK = N'$bakEsc'" -CadenaConexion $ConnMaster
    }
    catch {
        Write-Aviso 'SQL Server no pudo leer el archivo .bak.'
        Write-Info  'Causa habitual: la cuenta del servicio SQL no tiene permiso sobre la carpeta.'
        Write-Info  'Solucion (PowerShell como Administrador):'
        Write-Info  "  icacls '$(Split-Path -Parent $bak)' /grant 'NT SERVICE\MSSQLSERVER:(OI)(CI)RX'"
        throw $_
    }

    $rutas = Invoke-Sql -CadenaConexion $ConnMaster -Query @"
SELECT CONVERT(nvarchar(260), SERVERPROPERTY('InstanceDefaultDataPath')) AS DataPath,
       CONVERT(nvarchar(260), SERVERPROPERTY('InstanceDefaultLogPath'))  AS LogPath
"@
    $dataPath = [string] $rutas.DataPath
    $logPath  = [string] $rutas.LogPath
    if ([string]::IsNullOrWhiteSpace($dataPath)) {
        $mf = Invoke-Sql -CadenaConexion $ConnMaster `
              -Query 'SELECT TOP 1 physical_name FROM sys.master_files WHERE database_id = 1 AND type = 0'
        $dataPath = (Split-Path -Parent ([string]$mf.physical_name)) + '\'
    }
    if ([string]::IsNullOrWhiteSpace($logPath)) { $logPath = $dataPath }
    if (-not $dataPath.EndsWith('\')) { $dataPath += '\' }
    if (-not $logPath.EndsWith('\'))  { $logPath  += '\' }
    Write-Info "Destino datos: $dataPath"
    Write-Info "Destino log  : $logPath"

    $moves = @()
    foreach ($f in $fileList) {
        $destino = if ([string]$f.Type -eq 'L') { $logPath } else { $dataPath }
        $nombre  = [System.IO.Path]::GetFileName([string]$f.PhysicalName)
        $logico  = ([string]$f.LogicalName).Replace("'", "''")
        $rutaDst = ($destino + $nombre).Replace("'", "''")
        $moves  += "MOVE N'$logico' TO N'$rutaDst'"
        Write-Info "  $logico  ->  $destino$nombre"
    }

    $existe = Invoke-Sql -Query "SELECT database_id FROM sys.databases WHERE name = N'$BaseDatos'" -CadenaConexion $ConnMaster
    if ($null -ne $existe) {
        Invoke-Sql -Query "ALTER DATABASE [$BaseDatos] SET SINGLE_USER WITH ROLLBACK IMMEDIATE" -CadenaConexion $ConnMaster | Out-Null
    }

    $restore = "RESTORE DATABASE [$BaseDatos] FROM DISK = N'$bakEsc' WITH REPLACE, RECOVERY, STATS = 10, " + ($moves -join ', ')
    Invoke-Sql -Query $restore -CadenaConexion $ConnMaster | Out-Null
    Invoke-Sql -Query "IF DATABASEPROPERTYEX('$BaseDatos','UserAccess') <> 'MULTI_USER' ALTER DATABASE [$BaseDatos] SET MULTI_USER" -CadenaConexion $ConnMaster | Out-Null
    Write-Ok 'Base de datos restaurada.'
}

# --------------------------------------------------------------------------
# 3. Variables de entorno
# --------------------------------------------------------------------------
Write-Paso '3/5  Definiendo variables de entorno (ambito User)'

$vars = [ordered]@{
    'ConnectionStrings__DefaultConnection' = $ConnApi
    'Jwt__Key'                             = $JwtKey
    'Jwt__Issuer'                          = 'TicketeroApiDev'
    'Jwt__Audience'                        = 'TicketeroClientDev'
}

foreach ($k in $vars.Keys) {
    [Environment]::SetEnvironmentVariable($k, $vars[$k], 'User')   # persistente
    Set-Item -Path "Env:$k" -Value $vars[$k]                       # proceso actual

    $muestra = switch ($k) {
        'Jwt__Key' { '*' * 12 }
        'ConnectionStrings__DefaultConnection' { ($vars[$k] -replace '(?i)(Password=)[^;]*', '$1********') }
        default { $vars[$k] }
    }
    Write-Ok "$k = $muestra"
}

if ($Credencial) {
    Write-Aviso 'La cadena persistida contiene la contrasena de SQL en texto plano (ambito User).'
    Write-Info  'Para evitarlo, habilita autenticacion de Windows para tu usuario en SQL Server.'
}

# Redis: en local NO debe estar definido. Si lo esta, Program.cs registra
# AddStackExchangeRedisCache y cualquier operacion de cache (p.ej. el refresh
# token del login) falla con 500 tras 5s de timeout de conexion.
$esAdmin = ([Security.Principal.WindowsPrincipal] `
            [Security.Principal.WindowsIdentity]::GetCurrent()
           ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

foreach ($ambito in @('User', 'Machine')) {
    $redis = [Environment]::GetEnvironmentVariable('ConnectionStrings__Redis', $ambito)
    if ([string]::IsNullOrWhiteSpace($redis)) { continue }

    Write-Aviso "ConnectionStrings__Redis definida en ambito ${ambito}: '$redis'"

    if ($ambito -eq 'User' -or $esAdmin) {
        [Environment]::SetEnvironmentVariable('ConnectionStrings__Redis', $null, $ambito)
        Write-Ok "Eliminada del ambito $ambito."
    }
    else {
        Write-Info 'Requiere elevacion. Lanzando PowerShell como Administrador...'
        try {
            Start-Process powershell -Verb RunAs -Wait -WindowStyle Hidden -ArgumentList @(
                '-NoProfile', '-Command',
                "[Environment]::SetEnvironmentVariable('ConnectionStrings__Redis', `$null, 'Machine')"
            )
            $sigue = [Environment]::GetEnvironmentVariable('ConnectionStrings__Redis', 'Machine')
            if ([string]::IsNullOrWhiteSpace($sigue)) { Write-Ok 'Eliminada del ambito Machine.' }
            else { Write-Aviso 'Sigue presente en Machine; elimínala manualmente.' }
        }
        catch {
            Write-Aviso 'No se pudo elevar. Ejecuta como Administrador:'
            Write-Info  "  [Environment]::SetEnvironmentVariable('ConnectionStrings__Redis', `$null, 'Machine')"
        }
    }
}

# El proceso actual pudo heredarla antes de la limpieza; la API se lanza desde aqui.
Remove-Item Env:ConnectionStrings__Redis -ErrorAction SilentlyContinue
Write-Ok 'Cache: en memoria (sin Redis), correcto para desarrollo local.'

# La API corre en Development mediante el perfil 'http' de launchSettings.json
$env:ASPNETCORE_ENVIRONMENT = 'Development'
Write-Info 'ASPNETCORE_ENVIRONMENT = Development (solo en este proceso)'

# --------------------------------------------------------------------------
# 4. Validar la conexion
# --------------------------------------------------------------------------
Write-Paso '4/5  Validando conexion a la base de datos'

$info = Invoke-Sql -CadenaConexion $ConnApi -Query @"
SELECT DB_NAME() AS BaseActual,
       SUSER_SNAME() AS Login,
       (SELECT COUNT(*) FROM sys.tables) AS Tablas
"@
Write-Ok "Conectado a [$($info.BaseActual)] como '$($info.Login)'  -  $($info.Tablas) tablas"
if ([int]$info.Tablas -eq 0) { throw "La base '$BaseDatos' no tiene tablas." }

# Tablas declaradas en el modelo EF Core (ToTable) de Ticketero.Infrastructure
$esperadas = @(
    'ActivosFijos','ActivosFijosAuditoria','AreasFase','Atencion','ConfiguracionImpresora',
    'ConfiguracionMultimedia','ConfiguracionRed','Configuraciones','EstadosTicket','Kiosko',
    'KioskoAreas','Marcacion','Prioridades','Puestos','Roles','Servicios','SesionOperador',
    'Tickets','TiposTicket','Ubicaciones','Usuarios','UsuariosAreaFase'
)

$conteos = Invoke-Sql -CadenaConexion $ConnApi -Query @"
SELECT t.name AS Tabla, SUM(p.rows) AS Filas
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
GROUP BY t.name
ORDER BY t.name
"@
$presentes = @($conteos | ForEach-Object { $_.Tabla })

Write-Host ''
Write-Host ('    {0,-28} {1,10}' -f 'TABLA', 'FILAS') -ForegroundColor White
foreach ($row in $conteos) {
    Write-Host ('    {0,-28} {1,10}' -f $row.Tabla, $row.Filas) -ForegroundColor Gray
}
Write-Host ''

$faltantes = $esperadas | Where-Object { $presentes -notcontains $_ }
if ($faltantes) {
    Write-Aviso 'Tablas del modelo EF Core que NO existen en la base:'
    $faltantes | ForEach-Object { Write-Info "  - $_" }
    Write-Info  "La API fallara al consultarlas (Invalid object name 'dbo.X')."
}
else {
    Write-Ok "Las $($esperadas.Count) tablas del modelo EF Core estan presentes."
}

# Migraciones aplicadas
if ($presentes -contains '__EFMigrationsHistory') {
    $mig = Invoke-Sql -CadenaConexion $ConnApi -Query 'SELECT TOP 1 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC'
    if ($mig) { Write-Ok "Ultima migracion aplicada: $($mig.MigrationId)" }
}
else {
    Write-Aviso 'No existe __EFMigrationsHistory: la base no fue creada por migraciones EF Core.'
}

if ($SoloValidar) {
    Write-Host "`nValidacion completada. Para levantar la API:" -ForegroundColor Green
    Write-Host "  dotnet run --project `"$ProyectoApi`" --launch-profile http`n" -ForegroundColor White
    return
}

# --------------------------------------------------------------------------
# 5. Levantar la API
# --------------------------------------------------------------------------
Write-Paso '5/5  Levantando la API'

Push-Location $RaizApi
try {
    Write-Info 'dotnet build...'
    & dotnet build 'src\Ticketero.Api\Ticketero.Api.csproj' -c Debug --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw "La compilacion fallo (exit $LASTEXITCODE)." }
    Write-Ok 'Compilacion correcta.'

    $logDir = Join-Path $RaizApi 'logs'
    New-Item -ItemType Directory -Force -Path $logDir | Out-Null
    $logOut = Join-Path $logDir 'api-local.out.log'
    $logErr = Join-Path $logDir 'api-local.err.log'

    $proc = Start-Process -FilePath 'dotnet' `
        -ArgumentList @('run', '--project', "$ProyectoApi", '--launch-profile', 'http', '--no-build') `
        -WorkingDirectory $RaizApi -PassThru -NoNewWindow `
        -RedirectStandardOutput $logOut -RedirectStandardError $logErr

    Write-Info "Proceso dotnet PID $($proc.Id). Log: $logOut"

    $baseUrl = "http://localhost:$Puerto"
    $ready   = $false
    for ($i = 1; $i -le 40; $i++) {
        Start-Sleep -Seconds 2
        if ($proc.HasExited) {
            Write-Aviso "La API termino inesperadamente (exit $($proc.ExitCode)). Ultimas lineas:"
            Get-Content $logErr, $logOut -Tail 25 -ErrorAction SilentlyContinue | ForEach-Object { Write-Info $_ }
            throw 'La API no pudo iniciarse.'
        }
        try {
            if ((Invoke-WebRequest -Uri "$baseUrl/ready" -UseBasicParsing -TimeoutSec 5).StatusCode -eq 200) {
                $ready = $true; break
            }
        } catch { }
    }

    if (-not $ready) {
        Write-Aviso '/ready no respondio 200 en 80s. Ultimas lineas del log:'
        Get-Content $logOut -Tail 30 -ErrorAction SilentlyContinue | ForEach-Object { Write-Info $_ }
        throw 'Health check de base de datos fallido.'
    }

    Write-Ok "/healthz -> $((Invoke-WebRequest -Uri "$baseUrl/healthz" -UseBasicParsing -TimeoutSec 5).Content)"
    Write-Ok "/ready   -> Healthy (la API esta conectada a $BaseDatos)"

    Write-Host "`n-------------------------------------------" -ForegroundColor Green
    Write-Host ' API en marcha' -ForegroundColor Green
    Write-Host "-------------------------------------------" -ForegroundColor Green
    Write-Host "  Swagger        : $baseUrl/swagger"   -ForegroundColor White
    Write-Host "  SignalR        : $baseUrl/ticketHub" -ForegroundColor White
    Write-Host "  Liveness       : $baseUrl/healthz"   -ForegroundColor White
    Write-Host "  Readiness (BD) : $baseUrl/ready"     -ForegroundColor White
    Write-Host "`n  Ctrl+C detiene el seguimiento del log; para detener la API:" -ForegroundColor Gray
    Write-Host "    Stop-Process -Id $($proc.Id)`n" -ForegroundColor Gray

    Get-Content $logOut -Wait -Tail 20
}
finally {
    Pop-Location
}
