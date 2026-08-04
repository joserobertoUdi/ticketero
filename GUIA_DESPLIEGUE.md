# Guía de Configuración para Producción — KiTicketero

## 📋 Índice

1. [Arquitectura de Producción](#-arquitectura-de-producción)
2. [Problemas Bloqueantes](#-problemas-bloqueantes)
3. [Servidor Windows](#-servidor-windows)
4. [Base de Datos](#-base-de-datos)
5. [Backend (.NET 8)](#-backend-net-8)
6. [Frontend (Flutter)](#-frontend-flutter)
7. [Kioskos](#-kioskos)
8. [Red del Kiosko](#-red-del-kiosko)
9. [Impresora de Tickets](#-impresora-de-tickets)
10. [Multimedia](#-multimedia)
11. [Usuarios y Roles](#-usuarios-y-roles)
12. [Dashboard y Monitoreo](#-dashboard-y-monitoreo)
13. [Seguridad](#-seguridad)
14. [Variables de Entorno](#-variables-de-entorno)
15. [Verificación Post-Despliegue](#-verificación-post-despliegue)
16. [Mantenimiento](#-mantenimiento)
17. [Checklist de Despliegue](#-checklist-de-despliegue)
18. [Solución de Problemas](#-solución-de-problemas)

---

## 🏗️ Arquitectura de Producción

```
                    ┌─────────────────────────────┐
                    │   Clientes Windows (Flutter) │
                    │   Puestos de atención        │
                    │   Kioskos                    │
                    │   Pantallas llamadoras       │
                    └──────────┬──────────────────┘
                               │ HTTPS (TLS)
                               ▼
                    ┌─────────────────────────────┐
                    │    Reverse Proxy             │
                    │   (NGINX / IIS / Caddy)      │
                    │   Terminación TLS            │
                    └──────┬──────────┬───────────┘
                           │          │
                  HTTP:5000│          │
                           ▼          ▼
                    ┌──────────┐ ┌──────────┐
                    │ API .NET │ │  Redis   │
                    │ (Backend)│ │  (Cache) │
                    └─────┬────┘ └──────────┘
                          │
                          ▼
                    ┌──────────┐
                    │SQL Server│
                    │  2022    │
                    └──────────┘
```

### Flujo de comunicación

1. **App Flutter** (puesto/kiosko) se conecta vía HTTPS al reverse proxy
2. **Reverse proxy** termina TLS y reenvía a la API .NET en HTTP interno
3. **API .NET** procesa request, consulta SQL Server y Redis
4. **SignalR** (WebSocket) notifica cambios en tiempo real a todos los clientes
5. **Redis** actúa como backplane de SignalR para escalar horizontalmente

---

## 🔴 Problemas Bloqueantes

### 1. Frontend tiene `localhost` hardcodeado

**Archivo:** `SistemaTicket/sistema_ticketero/lib/core/constants/api_constants.dart:4-6`

```dart
// ❌ ACTUAL (no sirve en producción):
static const String baseUrl = 'http://localhost:5000';
static const String websocketUrl = 'http://localhost:5000/ticketHub';
```

**Solución — Usar `--dart-define` al compilar:**

```dart
// ✅ CAMBIAR A:
static const String baseUrl = String.fromEnvironment(
    'API_URL',
    defaultValue: 'http://localhost:5000');
static const String websocketUrl = String.fromEnvironment(
    'WS_URL',
    defaultValue: 'http://localhost:5000/ticketHub');
```

**Compilar para producción:**
```powershell
flutter build windows `
    --dart-define=API_URL=https://ticketero.midominio.com `
    --dart-define=WS_URL=wss://ticketero.midominio.com/ticketHub
```

### 2. Sin HTTPS (todo en HTTP)

**Riesgo:** Credenciales y tokens viajan en texto plano.

**Solución A — Reverse proxy con TLS (recomendado):**

Configurar NGINX, IIS o Caddy como proxy inverso:
```
Cliente → HTTPS :443 → NGINX → HTTP :5000 → API .NET
         → wss://dominio.com/ticketHub
```

**Solución B — Kestrel con certificado directo:**

En `appsettings.Production.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "C:/ruta/certificado.pfx",
          "Password": "contraseña-certificado"
        }
      }
    }
  }
}
```

### 3. `attachSettings` no hace nada

**Archivo:** `SistemaTicket/sistema_ticketero/lib/presentation/providers/auth_provider.dart:26`

```dart
void attachSettings(SettingsProvider settings) {
    // ❌ ESTÁ VACÍO — nunca implementado
}
```

**Impacto:** El `SettingsProvider` permite guardar URL del servidor en `SharedPreferences`, pero el `AuthProvider` nunca la usa. Si se cambia la URL desde configuración, no tiene efecto.

---

## 🖥️ Servidor Windows

### Requisitos mínimos

| Recurso | Mínimo | Recomendado |
|---------|--------|-------------|
| CPU | 4 núcleos | 8 núcleos |
| RAM | 8 GB | 16 GB |
| Disco | 100 GB SSD | 250 GB SSD |
| SO | Windows Server 2019 | Windows Server 2022 |
| .NET Runtime | 10.0 | 10.0 |
| SQL Server | 2019 Express | 2022 Standard |

### Puertos necesarios

| Puerto | Servicio | Descripción |
|--------|----------|-------------|
| 443 | Reverse Proxy | HTTPS público para la app |
| 5000 | API .NET | HTTP interno (solo proxy) |
| 1433 | SQL Server | Base de datos (red interna) |
| 6379 | Redis | Caché distribuida (red interna) |
| 80 | Reverse Proxy | HTTP redirección a HTTPS |

---

## 🗄️ Base de Datos

### Opción A: SQL Server local (recomendado para producción)

**1. Instalar SQL Server 2022 Express o superior**

**2. Crear variable de entorno a nivel Machine:**
```powershell
[Environment]::SetEnvironmentVariable(
    "ConnectionStrings__DefaultConnection",
    "Server=localhost;Database=DBTicketero;User=sa;Password=TU_CONTRA_SEGURA;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Machine")
```

**3. Aplicar migraciones:**
```powershell
cd C:\Users\becadesarrollo\Desktop\Proyectos\ApiTiketero\Ticketero
dotnet ef database update `
    --project src\Ticketero.Infrastructure `
    --startup-project src\Ticketero.Api
```

**4. Sembrar datos iniciales:**
Ejecutar los scripts SQL en `BD_Ticketero/` en orden, o ejecutar el script completo:
```powershell
sqlcmd -S localhost -d DBTicketero -i ApiTiketero\Ticketero\scripts\02_SeedData.sql
```

**5. Verificar integridad:**
```powershell
sqlcmd -S localhost -d DBTicketero -i ApiTiketero\Ticketero\scripts\03_ValidationQueries.sql
```

### Opción B: Docker (recomendado para desarrollo/QA)

```bash
cd ApiTiketero/Ticketero
docker-compose up -d sqlserver redis
```

La API se configura automáticamente vía variables de entorno en el `docker-compose.yml`.

### Tablas principales del sistema

| Tabla | Propósito |
|-------|-----------|
| `Tickets` | Ticket principal con FK a Servicios, TiposTicket, Prioridades, EstadosTicket, Areas |
| `Areas` | Áreas de atención (Caja, Informes, etc.) con prefijo y logo |
| `Usuarios` | Operadores y administradores con FK a Roles |
| `Servicios` | Servicios ofrecidos por área (Pago, Consulta, Retiro, etc.) |
| `Kioskos` | Puntos de toma de turno con configuración IP |
| `KioskoAreas` | Relación muchos-a-muchos entre kiosko y áreas |
| `Puestos` | Estaciones de atención física por área |
| `ConfiguracionRed` | Configuración de red por kiosko (DHCP/IP estático) |
| `ConfiguracionImpresora` | Configuración de impresora térmica por kiosko |
| `ConfiguracionesMultimedia` | Contenido multimedia (video/imagen) por kiosko |
| `SesionOperador` | Sesiones activas de operadores en puestos |
| `Atencion` | Bitácora de atenciones realizadas a tickets |
| `Marcacion` | Registro de llamados de tickets |
| `Configuraciones` | Parámetros clave-valor del sistema |

---

## 🚀 Backend (.NET 8)

### Publicar la API

```powershell
cd C:\Users\becadesarrollo\Desktop\Proyectos\ApiTiketero\Ticketero

dotnet publish src\Ticketero.Api\Ticketero.Api.csproj `
    -c Release `
    -o C:\ticketero\api
```

### Configurar `appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "ticketero.midominio.com",
  "ConnectionStrings": {
    "DefaultConnection": "",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "",
    "Issuer": "TicketeroApi",
    "Audience": "TicketeroClient",
    "ExpiryHours": 1
  },
  "Cors": {
    "Origins": [
      "https://ticketero.midominio.com",
      "https://app.ticketero.midominio.com"
    ]
  },
  "SeedDatabase": false,
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://*:5000"
      }
    }
  }
}
```

### Configurar variables de entorno (ámbito Machine)

```powershell
# Conexión a base de datos
[Environment]::SetEnvironmentVariable(
    "ConnectionStrings__DefaultConnection",
    "Server=localhost;Database=DBTicketero;User=sa;Password=TU_CONTRA;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Machine")

# Clave JWT (mínimo 32 caracteres)
[Environment]::SetEnvironmentVariable(
    "Jwt__Key",
    "TicketeroProd2026!ClaveSuperSeguraDe32Chars",
    "Machine")

# Redis (opcional, si no se usa se cae a MemoryCache)
[Environment]::SetEnvironmentVariable(
    "ConnectionStrings__Redis",
    "localhost:6379",
    "Machine")
```

### Instalar como servicio Windows

```powershell
# Crear servicio
New-Service -Name "TicketeroApi" `
    -BinaryPathName "C:\ticketero\api\Ticketero.Api.exe --urls http://*:5000" `
    -StartupType Automatic `
    -Description "API del sistema de turnos Ticketero"

# Iniciar servicio
Start-Service TicketeroApi

# Verificar estado
Get-Service TicketeroApi
```

### Opción Docker (alternativa)

```bash
cd ApiTiketero/Ticketero
docker-compose up -d
```

---

## 🖥️ Frontend (Flutter)

### Compilar para producción

```powershell
cd C:\Users\becadesarrollo\Desktop\Proyectos\SistemaTicket\sistema_ticketero

flutter clean
flutter pub get

flutter build windows `
    --dart-define=API_URL=https://ticketero.midominio.com `
    --dart-define=WS_URL=wss://ticketero.midominio.com/ticketHub
```

El ejecutable se genera en:
```
build\windows\runner\Release\sistema_ticketero.exe
```

### Distribución por red

1. Compartir la carpeta `build\windows\runner\Release\` en un recurso de red
2. Cada puesto ejecuta el `.exe` directamente (no requiere instalación)
3. Opcional: crear acceso directo en el escritorio de cada equipo

### Modo multi-ventana

La app soporta una **ventana secundaria** ("caller") para pantallas llamadoras:
```powershell
# Desde el código se abre con:
# await DesktopMultiWindow.createWindow('caller');
```

---

## 🏪 Kioskos

### Tipos de kiosko

| Tipo | Descripción |
|------|-------------|
| **Kiosko Físico** | Hardware dedicado con pantalla táctil + impresora |
| **Kiosko Virtual** | App Flutter en modo kiosko (misma PC que puesto) |

### Tabla `Kiosko` — Campos clave

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| `Descripcion` | Nombre del kiosko | "Kiosco Principal" |
| `Ubicacion` | Ubicación física | "Entrada principal" |
| `UbicacionId` | FK a Ubicaciones | 1 |
| `KioskoMediaId` | Contenido multimedia asignado | 1 |
| `IpKiosko` | IP del kiosko | 192.168.1.100 |
| `IpEquipo` | IP del equipo asociado | 192.168.1.101 |
| `MascaraRedEquipo` | Máscara de red | 255.255.255.0 |
| `GatewayEquipo` | Gateway | 192.168.1.1 |
| `DnsEquipo` | DNS | 8.8.8.8 |

### Configurar kiosko vía API

```powershell
# Autenticarse
$login = Invoke-RestMethod -Uri "https://ticketero.dominio.com/api/auth/login" -Method Post -Body '{"correo":"admin@empresa.com","password":"Admin123!"}' -ContentType "application/json"
$token = $login.token
$headers = @{Authorization = "Bearer $token"}

# Crear kiosko
$body = @{
    descripcion = "Kiosco Principal"
    ubicacion = "Entrada"
    ubicacionId = 1
    ipKiosko = "192.168.1.100"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://ticketero.dominio.com/api/kioskos" -Method Post -Body $body -ContentType "application/json" -Headers $headers
```

---

## 🌐 Red del Kiosko

### Configuración por kiosko (tabla `ConfiguracionRed`)

| Campo | Descripción | Valores |
|-------|-------------|---------|
| `TipoConexion` | Tipo de conexión de red | `ETHERNET`, `WIFI` |
| `DHCP` | Usar DHCP o IP estática | `true`, `false` |
| `DireccionIP` | IP estática (si DHCP=false) | `192.168.1.100` |
| `MascaraSubred` | Máscara de subred | `255.255.255.0` |
| `Gateway` | Gateway predeterminado | `192.168.1.1` |
| `DNSPrimario` | DNS primario | `8.8.8.8` |
| `DNSSecundario` | DNS secundario | `8.8.4.4` |
| `Puerto` | Puerto de la API | `5000` |

### Topología de red recomendada

```
[Internet]
    │
    ▼
[Firewall empresarial]
    │
    ▼
[Switch principal]
    │
    ├── [Servidor API + BD + Redis]
    ├── [Puesto 1 - Atención]
    ├── [Puesto 2 - Atención]
    ├── [Kiosko 1 - Toma de turnos]
    ├── [Kiosko 2 - Toma de turnos]
    └── [Pantalla Llamadora]
```

### Requisitos de red

- **Latencia:** < 50ms entre cualquier cliente y el servidor
- **Ancho de banda mínimo:** 10 Mbps (por cada 50 clientes simultáneos)
- **Calidad:** Conexión ethernet para kioskos (WiFi solo para tablets)

---

## 🖨️ Impresora de Tickets

### Configuración (tabla `ConfiguracionImpresora`)

| Campo | Descripción | Valores |
|-------|-------------|---------|
| `KioskoId` | FK al kiosko | 1 |
| `NombreImpresora` | Nombre descriptivo | "Impresora Caja 1" |
| `Puerto` | Puerto (USB, COM) | "USB001", "COM3" |
| `DireccionIP` | IP (conexión RED) | "192.168.1.200" |
| `TipoConexion` | Tipo de conexión | `USB`, `RED`, `SERIAL` |
| `AnchoPapelMM` | Ancho del papel | `58` o `80` |
| `Copias` | Número de copias | `1` |
| `ImpresionAutomatica` | Imprimir al tomar turno | `true` |

### Impresoras compatibles

| Marca | Modelo | Conexión |
|-------|--------|----------|
| Epson | TM-T20, TM-T88, TM-m30 | USB, RED |
| Star | TSP100, SP700 | USB, SERIAL |
| Bematech | MP-4200 TH | USB, RED |
| Daruma | DR800, DR700 | USB, SERIAL |
| Cualquier impresora | ESC/POS compatible | USB, RED, SERIAL |

### Protocolos de impresión

El sistema soporta tres métodos de impresión:

1. **Network ESC/POS** — Impresora en red TCP/IP (puerto 9100)
2. **Serial ESC/POS** — Puerto COM serial
3. **Windows Raw** — Impresora compartida vía Windows (driver instalado)

### Verificar impresora

```powershell
# Probar impresión de prueba desde el sistema
# Desde la pantalla de configuración del sistema:
# Configuración → Impresora → "Imprimir prueba"
```

---

## 🎬 Multimedia

### Configuración (tabla `ConfiguracionesMultimedia`)

| Campo | Descripción | Valores |
|-------|-------------|---------|
| `KioskoId` | FK al kiosko | 1 |
| `NombreContenido` | Nombre del archivo | "video_promo.mp4" |
| `TipoContenido` | Tipo de contenido | `VIDEO`, `IMAGEN`, `HTML`, `PDF` |
| `RutaArchivo` | Ruta o URL del archivo | "/uploads/multimedia/video1.mp4" |
| `DuracionSegundos` | Duración en segundos | `30` |
| `Orden` | Orden de reproducción | `1`, `2`, `3`... |
| `Repetir` | Repetir contenido | `true` |

### Formatos soportados

| Tipo | Formatos |
|------|----------|
| VIDEO | MP4, AVI, MKV (codec H.264) |
| IMAGEN | PNG, JPG, GIF |
| HTML | HTML5 con recursos embebidos |

### Subir archivos multimedia

```powershell
# Subir vía API
$headers = @{Authorization = "Bearer $token"}
$form = @{file = Get-Item -Path "C:\videos\promo.mp4"}
Invoke-RestMethod -Uri "https://ticketero.dominio.com/api/configuracion-multimedia/upload" `
    -Method Post `
    -Headers $headers `
    -Form $form
```

**Almacenamiento:** Los archivos se guardan en `wwwroot/uploads/multimedia/`.

---

## 👥 Usuarios y Roles

### Roles del sistema

| Rol | Descripción | Permisos |
|-----|-------------|----------|
| **Administrador** | Acceso total | Gestión de usuarios, áreas, servicios, kioskos, configuraciones, reportes |
| **Supervisor** | Supervisión | Dashboard, monitoreo de puestos, reportes, gestión de tickets |
| **Operador** | Atención | Atender tickets, llamar siguiente, completar, derivar |
| **Llamador** | Pantalla llamadora | Ver y re-llamar tickets en pantalla grande |

### Seed de usuarios por defecto

| Usuario | Nombre | Rol | Contraseña |
|---------|--------|-----|------------|
| `admin` | Administrador | Administrador | `Admin123!` |
| `jperez` | Juan Perez | Supervisor | `Admin123!` |
| `mgarcia` | Maria Garcia | Operador | `Admin123!` |
| `rtorres` | Roberto Torres | Llamador | `Admin123!` |

> **⚠️ Importante:** Cambiar todas las contraseñas por defecto en producción.

### Crear usuario vía API

```powershell
$body = @{
    nombreUsuario = "nuevo.operador"
    nombre = "Carlos"
    apellido = "Lopez"
    correo = "carlos@empresa.com"
    password = "TempPass123!"
    rolId = 3
    codigoSistema = "OPE002"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://ticketero.dominio.com/api/usuarios" `
    -Method Post -Body $body -ContentType "application/json" -Headers $headers
```

---

## 📊 Dashboard y Monitoreo

### Dashboard en tiempo real

- **Gráfico de tickets por hora** (fl_chart)
- **Tickets en espera vs atendidos**
- **Tiempo promedio de atención**
- **Rendimiento por operador**
- **Estado de puestos** (libre/ocupado)

### Monitor de puestos

- Visualiza todos los puestos de atención
- Muestra estado actual de cada puesto
- Actualización en tiempo real vía SignalR

### Pantalla llamadora (Caller)

- Muestra el ticket actual siendo llamado
- Versión multi-ventana independiente
- Ideal para pantallas LED grandes

### Health Checks

| Endpoint | Tipo | Descripción |
|----------|------|-------------|
| `GET /healthz` | Liveness | Verifica que la API responde |
| `GET /ready` | Readiness | Verifica API + BD |

```powershell
# Verificar estado
Invoke-RestMethod -Uri "https://ticketero.dominio.com/healthz"
# → "Healthy"

Invoke-RestMethod -Uri "https://ticketero.dominio.com/ready"
# → "Healthy"
```

---

## 🔐 Seguridad

| Medida | Implementación |
|--------|---------------|
| **Autenticación** | JWT Bearer tokens con expiración |
| **Roles** | 4 roles con permisos diferenciados |
| **Rate Limiting** | 100 requests/minuto por IP |
| **CORS** | Orígenes restringidos en producción |
| **Contraseñas** | BCrypt hash con salt |
| **Conexión BD** | Variable de entorno Machine (no en archivos) |
| **Logs** | Serilog con rotación diaria, 30 días retención |
| **TLS** | Reverse proxy o Kestrel con certificado |

### Buenas prácticas

1. **Nunca** commitear `appsettings.Development.json` con credenciales reales
2. **Rotar** la clave JWT periódicamente
3. **Usar contraseñas** diferentes para cada entorno
4. **Restringir** acceso a los endpoints de administración por IP
5. **Monitorear** logs de errores y accesos no autorizados

---

## 🔧 Variables de Entorno

### Obligatorias

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a SQL Server | `Server=localhost;Database=DBTicketero;User=sa;Password=***;TrustServerCertificate=true` |
| `Jwt__Key` | Clave secreta JWT (≥32 caracteres) | `TicketeroProd2026!ClaveSuperSeguraDe32Chars` |

### Opcionales

| Variable | Descripción | Defecto |
|----------|-------------|---------|
| `ConnectionStrings__Redis` | Cadena de conexión a Redis | `""` (usa MemoryCache) |
| `Jwt__Issuer` | Emisor del token | `TicketeroApi` |
| `Jwt__Audience` | Audiencia del token | `TicketeroClient` |
| `Jwt__ExpiryHours` | Horas de validez del token | `1` |
| `SeedDatabase` | Ejecutar seed al iniciar | `false` |

### Configurar en Windows (ámbito Machine)

```powershell
# Obligatorias
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
    "Server=localhost;Database=DBTicketero;User=sa;Password=MiContraSegura2026!;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Machine")

[Environment]::SetEnvironmentVariable("Jwt__Key",
    "TicketeroProd2026!ClaveSuperSeguraDe32Chars",
    "Machine")

# Opcionales
[Environment]::SetEnvironmentVariable("ConnectionStrings__Redis",
    "localhost:6379",
    "Machine")

[Environment]::SetEnvironmentVariable("SeedDatabase",
    "false",
    "Machine")
```

> **Nota:** Las variables Machine requieren reiniciar la terminal para heredarlas, o crearlas antes de abrir la terminal.

---

## ✅ Verificación Post-Despliegue

### 1. Verificar que el servicio corre

```powershell
Get-Service TicketeroApi
# → Status: Running
```

### 2. Verificar health checks

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/healthz"
# → "Healthy"

Invoke-RestMethod -Uri "http://localhost:5000/ready"
# → "Healthy"
```

### 3. Verificar autenticación

```powershell
$login = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body '{"correo":"admin@empresa.com","password":"Admin123!"}'

Write-Host "Token: $($login.token.Substring(0, 20))..."
Write-Host "Usuario: $($login.usuario.nombreUsuario)"
# → Token: eyJhbGciOiJIUzI1...
# → Usuario: admin
```

### 4. Verificar flujo completo

```powershell
# Login
$token = (Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -ContentType "application/json" -Body '{"correo":"admin@empresa.com","password":"Admin123!"}').token
$headers = @{Authorization = "Bearer $token"}
Write-Host "✅ Login OK"

# Obtener áreas
$areas = Invoke-RestMethod -Uri "http://localhost:5000/api/areas" -Headers $headers
Write-Host "✅ $($areas.Count) áreas cargadas: $($areas.descripcion -join ', ')"

# Obtener catálogos
$tipos = Invoke-RestMethod -Uri "http://localhost:5000/api/catalogos/tipos-ticket" -Headers $headers
$estados = Invoke-RestMethod -Uri "http://localhost:5000/api/catalogos/estados-ticket" -Headers $headers
$prioridades = Invoke-RestMethod -Uri "http://localhost:5000/api/catalogos/prioridades" -Headers $headers
Write-Host "✅ Catálogos: $($tipos.Count) tipos, $($estados.Count) estados, $($prioridades.Count) prioridades"

Write-Host "`n🎫 Sistema listo para operar"
```

### 5. Verificar SignalR WebSocket

```
Conectarse vía navegador a: https://ticketero.dominio.com/ticketHub
O verificar desde la app Flutter que el icono de conexión aparece verde.

Endpoints del hub:
- TicketCreated
- TicketCalled
- TicketStarted
- TicketCompleted
- TicketCancelled
- QueueUpdated
```

---

## 🛠️ Mantenimiento

| Tarea | Frecuencia | Comando / Acción |
|-------|------------|------------------|
| **Respaldo BD** | Diario | `BACKUP DATABASE DBTicketero TO DISK='C:\backups\db_$(Get-Date -Format yyyyMMdd).bak'` |
| **Rotación de logs** | Diario (automático) | Serilog retiene 30 días (`retainedFileCountLimit: 30`) |
| **Actualizar API** | Según release | `dotnet publish` → reemplazar binarios → `Restart-Service TicketeroApi` |
| **Monitorear health** | Cada minuto | Script automatizado: `GET /healthz` → debe responder 200 |
| **Limpiar multimedia** | Mensual | Archivos huérfanos en `wwwroot/uploads/multimedia/` |
| **Auditar usuarios** | Mensual | Revisar usuarios inactivos, cambiar contraseñas |
| **Actualizar cert TLS** | Anual | Renovar certificado SSL antes de expiración |

### Script de respaldo automatizado

```powershell
# backup_ticketero.ps1 — Programar en Task Scheduler
$fecha = Get-Date -Format "yyyyMMdd_HHmm"
$archivo = "C:\backups\DBTicketero_$fecha.bak"

Invoke-Sqlcmd -Query "BACKUP DATABASE DBTicketero TO DISK='$archivo' WITH COMPRESSION" `
    -ServerInstance "localhost"

# Limpiar backups > 30 días
Get-ChildItem "C:\backups\*.bak" | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | Remove-Item
```

---

## ✅ Checklist de Despliegue

### Pre-despliegue

- [ ] **Frontend:** Cambiar `baseUrl` de `localhost` a producción (vía `--dart-define`)
- [ ] **Frontend:** Usar `https://` y `wss://` en URLs
- [ ] **Frontend:** Compilar y probar localmente antes de distribuir
- [ ] **Backend:** Configurar variable de entorno `Jwt__Key` (≥32 caracteres)
- [ ] **Backend:** Configurar variable de entorno `ConnectionStrings__DefaultConnection`
- [ ] **Backend:** Configurar `AllowedHosts` con el dominio real
- [ ] **Backend:** Configurar `Cors:Origins` con URLs reales del frontend
- [ ] **Backend:** Deshabilitar Swagger (`ASPNETCORE_ENVIRONMENT=Production`)
- [ ] **Backend:** Configurar TLS (directo o reverse proxy)
- [ ] **BD:** Ejecutar migraciones o scripts SQL completos
- [ ] **BD:** Cambiar contraseña SA en docker-compose.yml
- [ ] **BD:** Verificar integridad con `scripts/03_ValidationQueries.sql`
- [ ] **Redis:** Configurar si se usa caché distribuida

### Post-despliegue

- [ ] **Health check:** `GET /healthz` responde 200
- [ ] **Readiness:** `GET /ready` responde 200
- [ ] **Login:** Autenticación funciona con usuarios seed
- [ ] **Catálogos:** Áreas, servicios, estados, prioridades cargan correctamente
- [ ] **Kioskos:** Configuración de red, impresora y multimedia funcional
- [ ] **SignalR:** WebSocket conecta y recibe eventos
- [ ] **Dashboard:** Estadísticas y gráficos se renderizan
- [ ] **Pantalla llamadora:** Ventana secundaria funciona
- [ ] **Firewall:** Puertos necesarios abiertos
- [ ] **Servicio:** API configurada como servicio Windows con inicio automático

---

## 🐛 Solución de Problemas

### Error: `Jwt:Key no configurada`

```powershell
# Verificar variable
$env:Jwt__Key
# Si retorna vacío, no está configurada

# Solución temporal (sesión actual)
$env:Jwt__Key="TicketeroProd2026!ClaveSuperSeguraDe32Chars"

# Solución permanente
[Environment]::SetEnvironmentVariable("Jwt__Key", "TicketeroProd2026!ClaveSuperSeguraDe32Chars", "Machine")
```

### Error: `Cannot open database 'DBTicketero'`

```powershell
# Verificar conexión
$conn = New-Object System.Data.SqlClient.SqlConnection(
    "Server=localhost;Database=DBTicketero;Trusted_Connection=true;TrustServerCertificate=true")
$conn.Open()
$conn.Close()
Write-Host "✅ Conexión OK"

# Causas posibles:
# 1. Variable de entorno no configurada
# 2. SQL Server no está corriendo: Get-Service MSSQLSERVER
# 3. BD no existe: aplicar migraciones
# 4. Firewall bloqueando puerto 1433
```

### Error: `HTTP 401 Unauthorized`

```powershell
# Causa: La Jwt__Key cambió entre sesiones
# Los tokens anteriores quedaron inválidos

# Solución: Hacer login nuevamente
$login = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post `
    -ContentType "application/json" `
    -Body '{"correo":"admin@empresa.com","password":"Admin123!"}'
$nuevoToken = $login.token
```

### Error: `El puerto 5000 ya está en uso`

```powershell
# Encontrar proceso
$pid = (netstat -ano | Select-String ":5000").Split()[-1]
Stop-Process -Id $pid -Force

# O usar puerto alternativo
dotnet run --project src\Ticketero.Api --urls http://localhost:5001
```

### Error: Frontend no conecta al backend

```powershell
# 1. Verificar que la URL del frontend apunte al servidor correcto
#    En api_constants.dart o en --dart-define usado al compilar

# 2. Verificar conectividad de red
Test-NetConnection ticketero.dominio.com -Port 443

# 3. Verificar CORS
#    Si la app está en https://app.ticketero.com y la API en otro dominio,
#    debe estar en Cors:Origins del appsettings

# 4. Verificar firewall de Windows
#    El puerto 5000 (o 443) debe estar abierto
```

### Error: SignalR no conecta (WebSocket)

```powershell
# 1. Verificar que la URL del WebSocket sea correcta
#    wss://ticketero.dominio.com/ticketHub

# 2. Verificar que el servidor tiene WebSocket habilitado
#    En Program.cs se mapea: app.MapHub<TicketHub>("/ticketHub")

# 3. Verificar que el proxy soporta WebSocket (importante en NGINX)
```

---

## 📚 Referencias

| Recurso | Ubicación |
|---------|-----------|
| API Endpoints | `SistemaTicket/sistema_ticketero/docs/API_ENDPOINTS.md` |
| Arquitectura | `SistemaTicket/sistema_ticketero/docs/ARQUITECTURA.md` |
| Configuración de red | `SistemaTicket/sistema_ticketero/docs/CONFIGURACION_RED.md` |
| Referencia técnica | `SistemaTicket/sistema_ticketero/docs/REFERENCIA_TECNICA.md` |
| Análisis del sistema | `SistemaTicket/sistema_ticketero/docs/ANALISIS_SISTEMA.md` |
| Scripts BD | `BD_Ticketero/*.sql` |
| Scripts seed | `ApiTiketero/Ticketero/scripts/*.sql` |
| Docker Compose | `ApiTiketero/Ticketero/docker-compose.yml` |
| Conexión BD por variable | `ApiTiketero/Ticketero/docs/conexion-bd-variable-entorno.md` |
| Cambios conexión BD | `ApiTiketero/Ticketero/docs/cambios-conexion-bd.md` |
