# Guía para levantar el Sistema Ticketero localmente

Esta guía detalla cómo configurar las variables de entorno y demás configuraciones para ejecutar **Ticketero** completo en tu máquina local (desarrollo), incluyendo backend (.NET 8), base de datos (SQL Server), Redis y frontend (Flutter).

---

## 📋 Índice

1. [Requisitos previos](#-requisitos-previos)
2. [Opción A — Todo con Docker (recomendado)](#-opción-a--todo-con-docker-recomendado)
3. [Opción B — Manual en Windows](#-opción-b--manual-en-windows)
   - [B.1 Base de datos (SQL Server)](#b1-base-de-datos-sql-server)
   - [B.2 Redis](#b2-redis)
   - [B.3 Backend (API .NET)](#b3-backend-api-net)
   - [B.4 Frontend (Flutter)](#b4-frontend-flutter)
4. [Variables de entorno completas](#-variables-de-entorno-completas)
5. [Configuración de archivos](#-configuración-de-archivos)
6. [Usuarios por defecto](#-usuarios-por-defecto)
7. [Verificación del funcionamiento](#-verificación-del-funcionamiento)
8. [Solución de problemas](#-solución-de-problemas)

---

## 🧰 Requisitos previos

| Herramienta | Versión | Dónde descargar |
|---|---|---|
| **.NET SDK** | 8.x | https://dotnet.microsoft.com/download/dotnet/8.0 |
| **Flutter** | 3.22+ | https://flutter.dev |
| **Docker Desktop** | Última | https://www.docker.com/products/docker-desktop/ |
| **SQL Server** (opcional, sin Docker) | 2022 Express | https://www.microsoft.com/en-us/sql-server/sql-server-downloads |
| **Redis** (opcional, sin Docker) | 7.x | https://github.com/tporadowski/redis/releases |
| **Visual Studio** (recomendado) | 2022 | https://visualstudio.microsoft.com |

> **Nota:** Docker es la forma más rápida porque levanta SQL Server y Redis automáticamente sin instalarlos en el sistema.

---

## 🐳 Opción A — Todo con Docker (recomendado)

### 1. Levantar SQL Server + Redis + API

El `docker-compose.yml` ya trae todo configurado. Desde la carpeta del backend:

```powershell
cd ApiTiketero\Ticketero
docker-compose up -d
```

Esto levanta 3 servicios:

| Servicio | Imagen | Puerto |
|---|---|---|
| `sqlserver` | SQL Server 2022 | `1433` |
| `redis` | Redis 7 | `6379` |
| `api` | API Ticketero | `5000` |

### 2. Variables de entorno ya definidas en compose

No necesitas configurar nada manualmente; el compose define lo siguiente para la API:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ASPNETCORE_URLS=http://+:80
  - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=DBTicketero;User=sa;Password=TicketeroProd2026!;TrustServerCertificate=true;MultipleActiveResultSets=true
  - ConnectionStrings__Redis=redis:6379
  - Jwt__Key=TicketeroProdSecretKey2026!ChangeMeInProduction!
  - Jwt__Issuer=TicketeroApi
  - Jwt__Audience=TicketeroClient
  - Cors__Origins__0=http://localhost:4200
```

### 3. Ejecutar migraciones / crear base de datos

Con la API y SQL Server arriba, aplica las migraciones o ejecuta los scripts SQL:

```powershell
cd ApiTiketero\Ticketero
dotnet ef database update `
    --project src\Ticketero.Infrastructure `
    --startup-project src\Ticketero.Api
```

> Si no usas migraciones, ejecuta los scripts de `BD_Ticketero/*.sql` en orden alfabético y luego el seed de `ApiTiketero\Ticketero\scripts\02_SeedData.sql`.

### 4. Verificar la API

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/healthz"
# → Healthy
```

La API ya está corriendo dentro del contenedor. **El frontend Flutter se ejecuta fuera de Docker.**

---

## 🖥️ Opción B — Manual en Windows

Usa esta opción si prefieres ejecutar SQL Server y Redis nativamente sin Docker.

### B.1 Base de datos (SQL Server)

1. **Instala SQL Server 2022 Express** (o superior).

2. **Configura la variable de entorno de conexión** a nivel de máquina:

```powershell
[Environment]::SetEnvironmentVariable(
    "ConnectionStrings__DefaultConnection",
    "Server=localhost;Database=DBTicketero;User=sa;Password=TICKETERO_DEV_PASS;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Machine")
```

> Cambia `TICKETERO_DEV_PASS` por la contraseña real de tu usuario `sa` (o usa `;Trusted_Connection=true;` para autenticación de Windows).

3. **Crea la base de datos y las tablas** ejecutando los scripts de `BD_Ticketero/`:

```powershell
# Opción con sqlcmd (los scripts en orden alfabético)
Get-ChildItem "BD_Ticketero\*.sql" | Sort-Object Name | ForEach-Object {
    sqlcmd -S localhost -d DBTicketero -i $_.FullName
}

# O en SSMS: abre cada archivo y ejecútalo, o usa el script combinado
```

4. **Sembra los datos iniciales** (usuarios, áreas, servicios, catálogos):

```powershell
sqlcmd -S localhost -d DBTicketero -i ApiTiketero\Ticketero\scripts\02_SeedData.sql
```

### B.2 Redis

1. **Descarga e instala Redis for Windows** (o usa WSL / Docker solo para Redis).

2. **Verifica que corre en el puerto 6379:**

```powershell
Test-NetConnection localhost -Port 6379
```

> Si no instalas Redis, la API funcionará igual usando **MemoryCache** como fallback (la conexión `Redis` queda vacía).

### B.3 Backend (API .NET)

1. **Configura las variables de entorno** en la sesión PowerShell (ámbito Machine para persistencia):

```powershell
# Conexión a SQL Server
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
    "Server=localhost;Database=DBTicketero;User=sa;Password=TICKETERO_DEV_PASS;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Machine")

# Redis (dejar vacío si no quieres usar Redis)
[Environment]::SetEnvironmentVariable("ConnectionStrings__Redis", "localhost:6379", "Machine")

# Clave JWT (mínimo 32 caracteres)
[Environment]::SetEnvironmentVariable("Jwt__Key",
    "TicketeroDev!ClaveLocalSuperSeguraDe>=32Chars", "Machine")

# Episodio/entorno
[Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development", "Machine")
```

2. **Crea el archivo `appsettings.Development.json`** (si no existe) con:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DBTicketero;User=sa;Password=TICKETERO_DEV_PASS;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "TicketeroDev!ClaveLocalSuperSeguraDe>=32Chars",
    "Issuer": "TicketeroApiDev",
    "Audience": "TicketeroClientDev",
    "ExpiryHours": 8
  },
  "Cors": {
    "Origins": [
      "http://localhost:3000",
      "http://localhost:5000"
    ]
  },
  "SeedDatabase": true,
  "Seed": {
    "DefaultPassword": "Admin123!"
  }
}
```

> Puedes copiar la plantilla desde `appsettings.Development.example.json` (usa valores de ejemplo).

3. **Restaura los paquetes y ejecuta la API:**

```powershell
cd ApiTiketero\Ticketero
dotnet restore
dotnet run --project src\Ticketero.Api
```

La API quedará disponible en `http://localhost:5000` y Swagger en `http://localhost:5000/swagger`.

### B.4 Frontend (Flutter)

1. **Ubica la carpeta del frontend:**

```powershell
cd SistemaTicket\sistema_ticketero
```

2. **Revisa/ajusta la URL de la API** en `lib/core/constants/api_constants.dart`:

```dart
static const String baseUrl = 'http://localhost:5000';
static const String websocketUrl = 'http://localhost:5000/ticketHub';
```

3. **Instala dependencias y ejecuta:**

```powershell
flutter pub get
flutter run -d windows
```

> Para apuntar a otras URLs sin modificar el código, usa `--dart-define`:
> ```powershell
> flutter run -d windows --dart-define=API_URL=http://localhost:5000 --dart-define=WS_URL=http://localhost:5000/ticketHub
> ```

---

## 🔧 Variables de entorno completas

Estas son todas las variables que la API escala desde el entorno. Todas se pueden configurar como variables de entorno de Windows o agregarse al `appsettings.Development.json`.

### Obligatorias

| Variable | Descripción | Ejemplo local |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a SQL Server | `Server=localhost;Database=DBTicketero;User=sa;Password=TICKETERO_DEV_PASS;TrustServerCertificate=true;MultipleActiveResultSets=true` |
| `Jwt__Key` | Clave secreta JWT (≥32 caracteres) | `TicketeroDev!ClaveLocalSuperSeguraDe>=32Chars` |

### Opcionales

| Variable | Descripción | Defecto |
|---|---|---|
| `ConnectionStrings__Redis` | Cadena de conexión a Redis | `""` (usa MemoryCache) |
| `Jwt__Issuer` | Emisor del token | `TicketeroApi` |
| `Jwt__Audience` | Audiencia del token | `TicketeroClient` |
| `Jwt__ExpiryHours` | Horas de validez del token | `1` |
| `Cors__Origins__N` | Orígenes permitidos (configurado por variable) | `[]` |
| `SeedDatabase` | Sembrar datos al iniciar | `false` |
| `Seed__DefaultPassword` | Contraseña por defecto del seed | `""` |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Production` |

### Configurar todas a la vez en PowerShell (ámbito Machine)

```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
    "Server=localhost;Database=DBTicketero;User=sa;Password=TICKETERO_DEV_PASS;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Machine")

[Environment]::SetEnvironmentVariable("ConnectionStrings__Redis",
    "localhost:6379", "Machine")

[Environment]::SetEnvironmentVariable("Jwt__Key",
    "TicketeroDev!ClaveLocalSuperSeguraDe>=32Chars", "Machine")

[Environment]::SetEnvironmentVariable("Jwt__Issuer",
    "TicketeroApiDev", "Machine")

[Environment]::SetEnvironmentVariable("Jwt__Audience",
    "TicketeroClientDev", "Machine")

[Environment]::SetEnvironmentVariable("Jwt__ExpiryHours",
    "8", "Machine")

[Environment]::SetEnvironmentVariable("SeedDatabase",
    "true", "Machine")

[Environment]::SetEnvironmentVariable("Seed__DefaultPassword",
    "Admin123!", "Machine")

[Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT",
    "Development", "Machine")
```

> **Importante:** Las variables a nivel **Machine** requieren reiniciar la terminal (o la sesión) para heredarlas.

---

## ⚙️ Configuración de archivos

### Estructura del `appsettings`

| Archivo | Uso |
|---|---|
| `appsettings.json` | Base (valores por defecto, sin credenciales) |
| `appsettings.Development.json` | Solo se usa si `ASPNETCORE_ENVIRONMENT=Development` |
| `appsettings.Production.json` | Solo se usa si `ASPNETCORE_ENVIRONMENT=Production` |
| `appsettings.Development.example.json` | Plantilla de ejemplo (no tiene datos reales) |

> El orden de precedencia es: **variables de entorno > appsettings.{Environment}.json > appsettings.json**.

### Conexión en el frontend

El archivo clave del frontend es:
```
SistemaTicket/sistema_ticketero/lib/core/constants/api_constants.dart
```

Define `baseUrl` y `websocketUrl`. Para desarrollo local apunta a `http://localhost:5000`.

---

## 👥 Usuarios por defecto (seed)

| Usuario | Rol | Contraseña |
|---|---|---|
| `admin` | Administrador | `Admin123!` |
| `jperez` | Supervisor | `Admin123!` |
| `mgarcia` | Operador | `Admin123!` |
| `rtorres` | Llamador | `Admin123!` |

> Para el login de prueba la API espera el campo `correo`/`nombreUsuario` (según la versión). Revisa el endpoint `/api/auth/login`.

---

## ✅ Verificación del funcionamiento

### 1. Backend arriba

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/healthz"
# → Healthy
```

### 2. Swagger accesible

Abre en el navegador: `http://localhost:5000/swagger`

### 3. Login de prueba

```powershell
$login = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
    -Method Post -ContentType "application/json" `
    -Body '{"correo":"admin@empresa.com","password":"Admin123!"}'

Write-Host "Token: $($login.token.Substring(0, 20))..."
```

### 4. Catálogos cargan

```powershell
$token = $login.token
$headers = @{Authorization = "Bearer $token"}
$areas = Invoke-RestMethod -Uri "http://localhost:5000/api/areas" -Headers $headers
Write-Host "Áreas: $($areas.Count)"
```

### 5. Frontend conectado

- La app Flutter inicia y muestra la pantalla de login.
- El icono de **SignalR** (WebSocket) aparece en verde cuando conecta a `/ticketHub`.

---

## 🐛 Solución de problemas

### `Cannot open database 'DBTicketero'`

```powershell
# 1. La BD no existe → ejecuta los scripts de BD_Ticketero
# 2. La variable de entorno no está → revisa:
$env:ConnectionStrings__DefaultConnection
# 3. SQL Server no corre → revisa:
Get-Service MSSQLSERVER
```

### `Jwt:Key no configurada`

```powershell
$env:Jwt__Key
# Si vuelve vacío → configúrala como Machine y abre nueva terminal
[Environment]::SetEnvironmentVariable("Jwt__Key", "TicketeroDev!ClaveLocalSuperSeguraDe>=32Chars", "Machine")
```

### Puertos 5000 / 1433 / 6379 en uso

```powershell
# Ver qué ocupa el puerto 5000
netstat -ano | Select-String ":5000"
# Matar el proceso
Stop-Process -Id <PID> -Force
```

### El frontend no conecta con el backend

1. Verifica que `api_constants.dart` tenga `http://localhost:5000`.
2. Verifica que el backend esté corriendo: `Invoke-RestMethod http://localhost:5000/healthz`.
3. Verifica CORS: si tu app corre en un puerto distinto, agrégalo a `Cors:Origins`.

### SignalR / WebSocket no conecta

```powershell
# Verifica que la URL del WS esté bien (api_constants.dart)
# y que el hub esté mapeado en Program.cs: app.MapHub<TicketHub>("/ticketHub")
Test-NetConnection localhost -Port 5000
```

---

## 📚 Referencias

| Recurso | Ubicación |
|---|---|
| Docker Compose | `ApiTiketero\Ticketero\docker-compose.yml` |
| Plantilla config desarrollo | `ApiTiketero\Ticketero\src\Ticketero.Api\appsettings.Development.example.json` |
| Scripts de BD | `BD_Ticketero\*.sql` |
| Scripts de seed | `ApiTiketero\Ticketero\scripts\*.sql` |
| Constantes del frontend | `SistemaTicket\sistema_ticketero\lib\core\constants\api_constants.dart` |
| Guía de producción | `GUIA_DESPLIEGUE.md` |
| README principal | `README.md` |