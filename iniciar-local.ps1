<#
.SYNOPSIS
    Inicia en local la base de datos, la API Ticketero y la aplicación Flutter.

.DESCRIPTION
    - Usa SQL Server local instalado por defecto (sin contenedores).
    - Opcionalmente levanta SQL Server y Redis mediante Docker Compose.
    - Aplica las migraciones de Entity Framework a DBTicketero.
    - Abre la API y Flutter (Windows por defecto) en ventanas de PowerShell independientes.

.EXAMPLE
    .\iniciar-local.ps1

.EXAMPLE
    .\iniciar-local.ps1 -DatabaseMode Docker -Device chrome
##>
[CmdletBinding()]
param(
    [string]$Device = 'windows',
    [ValidateSet('LocalSqlServer', 'Docker')]
    [string]$DatabaseMode = 'LocalSqlServer',
    [string]$SqlServerInstance = '.\SQLEXPRESS',
    [switch]$SkipMigrations
)

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$backend = Join-Path $root 'ApiTiketero\Ticketero'
$apiProject = Join-Path $backend 'src\Ticketero.Api\Ticketero.Api.csproj'
$infrastructureProject = Join-Path $backend 'src\Ticketero.Infrastructure\Ticketero.Infrastructure.csproj'
$frontend = Join-Path $root 'SistemaTicket\sistema_ticketero'
$composeFile = Join-Path $backend 'docker-compose.yml'

function Assert-Command {
    param([Parameter(Mandatory)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "No se encontró '$Name' en PATH. Instálalo y vuelve a ejecutar este script."
    }
}

function Wait-TcpPort {
    param(
        [Parameter(Mandatory)][int]$Port,
        [int]$TimeoutSeconds = 90
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-NetConnection -ComputerName 'localhost' -Port $Port -InformationLevel Quiet -WarningAction SilentlyContinue) {
            return
        }
        Start-Sleep -Seconds 2
    }

    throw "El puerto $Port no quedó disponible en $TimeoutSeconds segundos. Revisa Docker Desktop y los registros de SQL Server."
}

function ConvertTo-EncodedPowerShellCommand {
    param([Parameter(Mandatory)][string]$Command)

    return [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($Command))
}

Assert-Command dotnet
Assert-Command flutter

# Estas variables existen únicamente en los procesos creados por este lanzador.
$connectionString = "Server=$SqlServerInstance;Database=DBTicketero;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
$redisConnection = ''

if ($DatabaseMode -eq 'Docker') {
    Assert-Command docker
    Write-Host 'Iniciando SQL Server y Redis mediante Docker...' -ForegroundColor Cyan
    & docker compose -f $composeFile up -d sqlserver redis
    if ($LASTEXITCODE -ne 0) {
        throw 'Docker Compose no pudo iniciar SQL Server y Redis.'
    }

    Write-Host 'Esperando a que SQL Server acepte conexiones...' -ForegroundColor Cyan
    Wait-TcpPort -Port 1433
    $connectionString = 'Server=localhost,1433;Database=DBTicketero;User Id=sa;Password=TicketeroProd2026!;TrustServerCertificate=true;MultipleActiveResultSets=true'
    $redisConnection = 'localhost:6379'
}
$databaseDescription = if ($DatabaseMode -eq 'Docker') { 'SQL Server Docker en localhost,1433' } else { "SQL Server local ($SqlServerInstance)" }
$jwtKey = 'TicketeroDevKey2026!@#$%^&*()MinLength32'

if (-not $SkipMigrations) {
    Write-Host 'Aplicando migraciones de la base de datos...' -ForegroundColor Cyan
    $env:ConnectionStrings__DefaultConnection = $connectionString
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    & dotnet ef database update --project $infrastructureProject --startup-project $apiProject
    if ($LASTEXITCODE -ne 0) {
        throw 'No se pudieron aplicar las migraciones. Ejecuta de nuevo el script cuando SQL Server esté completamente inicializado.'
    }
}

Write-Host 'Restaurando dependencias de Flutter...' -ForegroundColor Cyan
Push-Location $frontend
try {
    & flutter pub get
    if ($LASTEXITCODE -ne 0) {
        throw 'Flutter no pudo restaurar las dependencias.'
    }
}
finally {
    Pop-Location
}

$apiCommand = @"
`$env:ASPNETCORE_ENVIRONMENT = 'Development'
`$env:ConnectionStrings__DefaultConnection = '$connectionString'
`$env:ConnectionStrings__Redis = '$redisConnection'
`$env:Jwt__Key = '$jwtKey'
Set-Location -LiteralPath '$backend'
dotnet run --project '$apiProject' --urls http://localhost:5000
"@

$flutterCommand = @"
Set-Location -LiteralPath '$frontend'
flutter run -d '$Device'
"@

Write-Host 'Abriendo API en http://localhost:5000...' -ForegroundColor Green
Start-Process -FilePath 'powershell.exe' -ArgumentList @(
    '-NoExit',
    '-EncodedCommand',
    (ConvertTo-EncodedPowerShellCommand -Command $apiCommand)
)

Write-Host "Abriendo Flutter en el dispositivo '$Device'..." -ForegroundColor Green
Start-Process -FilePath 'powershell.exe' -ArgumentList @(
    '-NoExit',
    '-EncodedCommand',
    (ConvertTo-EncodedPowerShellCommand -Command $flutterCommand)
)

Write-Host ''
Write-Host 'Servicios iniciados:' -ForegroundColor Green
Write-Host "  Base de datos: $databaseDescription (DBTicketero)"
if ($DatabaseMode -eq 'Docker') {
    Write-Host '  Redis: localhost,6379'
}
Write-Host '  API: http://localhost:5000 (Swagger: http://localhost:5000/swagger)'
