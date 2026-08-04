# Guía de Despliegue — Ticketero API

## Requisitos previos

- .NET SDK 10.0
- SQL Server (local o remoto)
- Redis (opcional, para caché distribuida y backplane de SignalR)

---

## Estructura del proyecto

```
Ticketero/
├── src/
│   ├── Ticketero.Api              ← API REST (controladores, middleware, SignalR)
│   ├── Ticketero.Application      ← Casos de uso, DTOs, interfaces
│   ├── Ticketero.Domain           ← Entidades, enums, value objects (0 dependencias)
│   └── Ticketero.Infrastructure   ← EF Core, DbContext, migraciones, repositorios
├── tests/
│   └── Ticketero.Tests            ← Pruebas unitarias
├── scripts/                       ← Scripts SQL auxiliares
├── docs/                          ← Documentación técnica
├── docker-compose.yml             ← Orquestación local (SQL Server + Redis + API)
├── Dockerfile                     ← Imagen Docker multi-etapa
└── Ticketero.slnx                 ← Solution file
```

---

## Variables de entorno requeridas

La API obtiene su configuración desde variables de entorno (ámbito **Machine** en Windows). Los `appsettings.json` tienen valores vacíos como fallback.

### 1. Conexión a base de datos

```
Nombre:   ConnectionStrings__DefaultConnection
Valor:    Server=localhost\SQLEXPRESS;Database=DBTicketero;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true
```

**En producción** usar autenticación SQL Server:

```
Server=IP_O_HOST;Database=DBTicketero;User=sa;Password=CONTRA;TrustServerCertificate=true;MultipleActiveResultSets=true
```

### 2. Llave JWT

```
Nombre:   Jwt__Key
Valor:    TicketeroDevKey2026!@#$%^&*()MinLength32
```

Mínimo 32 caracteres. En producción usar una clave distinta y segura.

### 3. Configurar en Windows (PowerShell como Administrador)

```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
    "Server=localhost\SQLEXPRESS;Database=DBTicketero;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Machine")

[Environment]::SetEnvironmentVariable("Jwt__Key",
    "TicketeroDevKey2026!@#$%^&*()MinLength32",
    "Machine")
```

> **Importante:** Abrir una **nueva terminal** después de crearlas para que los cambios surtan efecto.

### 4. Verificar

```powershell
[Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Machine")
[Environment]::GetEnvironmentVariable("Jwt__Key", "Machine")
```

---

## Configuración de la base de datos

### Crear BD y aplicar migraciones

```powershell
cd C:\Users\becadesarrollo\Desktop\Proyectos\ApiTiketero\Ticketero
dotnet ef database update --project src\Ticketero.Infrastructure --startup-project src\Ticketero.Api
```

### Resetear BD desde cero

```powershell
dotnet ef database drop --project src\Ticketero.Infrastructure --startup-project src\Ticketero.Api --force
dotnet ef database update --project src\Ticketero.Infrastructure --startup-project src\Ticketero.Api
```

### Seed de datos iniciales

El seed se ejecuta automáticamente al iniciar la API si `SeedDatabase = true` en `appsettings.Development.json`. Puebla:

- **Roles:** Administrador, Supervisor, Operador, Llamador
- **EstadosTicket:** Nuevo, Asignado, En Proceso, En Espera, Resuelto, Cerrado, Cancelado, Llamado
- **Prioridades:** Crítica, Alta, Media, Baja
- **TiposTicket:** Incidente, Solicitud, Problema, Cambio, Consulta
- **Áreas:** Caja, Informes, Atención al Cliente
- **Servicios:** PAGO, INFORMACION, CONSULTA, RETIRO
- **Ubicaciones:** Caja General
- **Kioskos:** Kiosko Principal, Kiosko Secundario
- **Puestos:** Caja 1, Caja 2
- **Usuarios:** admin, jperez, mgarcia, rtorres (contraseña: `Admin123!`)

---

## Cómo ejecutar la API

### Local

```powershell
cd C:\Users\becadesarrollo\Desktop\Proyectos\ApiTiketero\Ticketero
dotnet run --project src\Ticketero.Api
```

La API se levanta en `http://localhost:5000`.

### Con Docker Compose

```powershell
docker-compose up -d
```

Levanta SQL Server (puerto 1433), Redis (puerto 6379) y la API (puerto 5000).

---

## Endpoints principales

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | Público | Iniciar sesión |
| POST | `/api/auth/refresh` | Público | Refrescar token |
| GET | `/api/auth/me` | Autenticado | Obtener usuario actual |
| POST | `/api/auth/logout` | Autenticado | Cerrar sesión |
| GET | `/healthz` | Público | Health check (liveness) |
| GET | `/ready` | Público | Health check (readiness) |
| GET/POST/PUT/DELETE | `/api/Tickets` | Variable | CRUD de tickets |
| GET/POST/PUT/DELETE | `/api/Usuarios` | Variable | CRUD de usuarios |
| GET/POST/PUT/DELETE | `/api/Areas` | Variable | CRUD de áreas |
| GET/POST/PUT/DELETE | `/api/Servicios` | Variable | CRUD de servicios |
| GET/POST/PUT/DELETE | `/api/Kioskos` | Variable | CRUD de kioskos |
| GET/POST/PUT/DELETE | `/api/Puestos` | Variable | CRUD de puestos |
| GET | `/api/Dashboard` | Autenticado | Datos del dashboard |
| WebSocket | `/ticketHub` | Autenticado | SignalR (tiempo real) |

---

## Arquitectura de Clean Architecture

```
Ticketero.Api        ← Controladores, Middleware, Swagger, SignalR
       ↓
Ticketero.Application ← Casos de uso, DTOs, interfaces de repositorios
       ↓
Ticketero.Domain     ← Entidades, Enums, Value Objects (sin dependencias externas)
       ↑
Ticketero.Infrastructure ← EF Core, DbContext, Repositorios, Migraciones
```

## Tecnologías principales

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 10.0 | Framework base |
| Entity Framework Core | 10.0 | ORM y migraciones |
| SQL Server | 2022 Express | Base de datos |
| Redis | 7 Alpine | Caché distribuida y backplane SignalR |
| SignalR | - | Notificaciones en tiempo real |
| JWT Bearer | - | Autenticación |
| Serilog | 10.0 | Logging estructurado |
| Swashbuckle | 10.0 | Documentación Swagger |
| QuestPDF | 2026.7 | Generación de PDFs |
| BCrypt.Net | 4.2 | Hashing de contraseñas |



## Paso 1: Iniciar el Backend

```powershell
# Desde la raíz del proyecto
cd C:\Users\becadesarrollo\Desktop\Proyectos\ApiTiketero\Ticketero

# Opción A: Desarrollo (con hot reload)
dotnet watch run --project src\Ticketero.Api\Ticketero.Api.csproj --urls http://localhost:5000

# Opción B: Producción (sin watch)
dotnet run --project src\Ticketero.Api\Ticketero.Api.csproj --urls http://localhost:5000 --configuration Release

# Opción C: Publicar y ejecutar (recomendado para producción)
dotnet publish src\Ticketero.Api\Ticketero.Api.csproj -c Release -o .\publish
cd .\publish
.\Ticketero.Api.exe --urls http://0.0.0.0:5000
```

---

## Paso 2: Verificar que el Backend Arrancó Correctamente

```powershell
# 1. Verificar que el proceso está corriendo
netstat -ano | Select-String ":5000.*LISTEN"

# 2. Verificar health checks
Invoke-RestMethod -Uri "http://localhost:5000/healthz" -Method Get
# → Debe responder con "Healthy"

Invoke-RestMethod -Uri "http://localhost:5000/ready" -Method Get
# → Debe responder con "Healthy" (incluye verificación de BD)

# 3. Verificar login
$body = @{correo="admin@empresa.com"; password="Admin123!"} | ConvertTo-Json
$login = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $body -ContentType "application/json"
$login.token.Substring(0, 20)
# → Debe mostrar los primeros 20 caracteres del JWT
```

---

## Paso 3: Verificar el Flujo Completo

```powershell
# Login
$headers = @{"Content-Type"="application/json"}
$login = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Headers $headers -Body '{"correo":"admin@empresa.com","password":"Admin123!"}' -ErrorAction Stop
$token = $login.token
$userId = $login.usuario.id
$headers["Authorization"] = "Bearer $token"
Write-Host "1. Login OK - User: $($login.usuario.nombreUsuario) ($userId)"

# Obtener áreas y puestos
$areas = Invoke-RestMethod -Uri "http://localhost:5000/api/areas" -Method Get -Headers $headers
Write-Host "2. Areas: $($areas.Count) cargadas"

$puestos = Invoke-RestMethod -Uri "http://localhost:5000/api/areas/$($areas[0].id)/puestos" -Method Get -Headers $headers
Write-Host "3. Puestos en $($areas[0].nombre): $($puestos.Count) disponibles"
$puestoId = $puestos[0].id

# Crear sesión
$sesion = Invoke-RestMethod -Uri "http://localhost:5000/api/usuarios/$userId/sesion" -Method Post -Headers $headers -Body "{""usuarioId"":$userId,""puestoId"":$puestoId}"
Write-Host "4. Sesion creada: ID=$($sesion.sesionOperadorId) Activa=$($sesion.estaActiva)"

# Cerrar sesión
$cierre = Invoke-RestMethod -Uri "http://localhost:5000/api/usuarios/sesion/$($sesion.sesionOperadorId)/cerrar" -Method Post -Headers $headers
Write-Host "5. Sesion cerrada: Fin=$($cierre.fechaFin) Activa=$($cierre.estaActiva)"

Write-Host "`nTODO OK"
```

### Salida esperada
```
1. Login OK - User: admin (1)
2. Areas: 3 cargadas
3. Puestos en Caja: 2 disponibles
4. Sesion creada: ID=1 Activa=True
5. Sesion cerrada: Fin=2026-07-16T... Activa=False
TODO OK
```

---

## Paso 4: Verificar en Base de Datos

```sql
-- Conectar a SQL Server y ejecutar:
SELECT 
    s.SesionOperadorId,
    u.NombreUsuario AS Usuario,
    p.Descripcion AS Puesto,
    s.FechaInicio,
    s.FechaFin,
    CASE WHEN s.FechaFin IS NULL THEN 'ACTIVA' ELSE 'CERRADA' END AS Estado
FROM SesionOperador s
INNER JOIN Usuarios u ON s.UsuarioId = u.UsuarioId
INNER JOIN Puestos p ON s.PuestoId = p.PuestoId
ORDER BY s.SesionOperadorId DESC;
```

### Validaciones de integridad
```sql
-- 1. No deben haber dos sesiones activas para el mismo usuario
SELECT UsuarioId, COUNT(*) AS SesionesActivas
FROM SesionOperador
WHERE FechaFin IS NULL
GROUP BY UsuarioId
HAVING COUNT(*) > 1;
-- → Debe retornar 0 filas

-- 2. Todas las FK deben ser válidas
SELECT s.SesionOperadorId
FROM SesionOperador s
LEFT JOIN Usuarios u ON s.UsuarioId = u.UsuarioId
LEFT JOIN Puestos p ON s.PuestoId = p.PuestoId
WHERE u.UsuarioId IS NULL OR p.PuestoId IS NULL;
-- → Debe retornar 0 filas

-- 3. Toda sesión debe tener FechaInicio
SELECT COUNT(*) AS SinFechaInicio
FROM SesionOperador
WHERE FechaInicio IS NULL;
-- → Debe retornar 0
```

---

## Solución de Problemas

### Error: `Jwt:Key no está configurada`

**Causa:** La variable de entorno `Jwt__Key` no está seteada.

**Verificar:**
```powershell
# PowerShell
$env:Jwt__Key
# → Si retorna vacío, no está configurada
```

**Solución:**
```powershell
$env:Jwt__Key="<una-clave-de-al-menos-32-caracteres>"
# Luego reiniciar la app
```

---

### Error: `Cannot open database 'DBTicketero'`

**Causa:** La connection string no está configurada o la BD no existe.

**Verificar:**
```powershell
# PowerShell (con la connection string correcta)
$conn = New-Object System.Data.SqlClient.SqlConnection("Server=localhost\SQLEXPRESS;Database=DBTicketero;Trusted_Connection=true;TrustServerCertificate=true")
$conn.Open()
$conn.Close()
Write-Host "Conexion OK"
```

**Soluciones:**
- **Opción A:** Configurar `ConnectionStrings__DefaultConnection` como variable de entorno
- **Opción B:** Editarla directamente en `appsettings.Production.json`
- **Opción C:** Verificar que SQL Server esté corriendo: `Get-Service MSSQLSERVER` o `Get-Service SQLEXPRESS`

---

### Error: `HTTP 401 Unauthorized` en todas las requests

**Causa:** La `Jwt__Key` cambió respecto a la sesión anterior, invalidando tokens existentes.

**Solución:** Hacer login nuevamente para obtener un nuevo token:
```powershell
$login = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Headers @{"Content-Type"="application/json"} -Body '{"correo":"admin@empresa.com","password":"Admin123!"}'
$nuevoToken = $login.token
# Usar $nuevoToken en todas las requests
```

---

### Error: `El puerto 5000 ya está en uso`

**Verificar qué proceso ocupa el puerto:**
```powershell
netstat -ano | Select-String ":5000"
# El último número es el PID
```

**Solución Opción A — Matar el proceso:**
```powershell
Stop-Process -Id <PID> -Force
```

**Solución Opción B — Usar otro puerto:**
```powershell
dotnet run --project src\Ticketero.Api\Ticketero.Api.csproj --urls http://localhost:5001
# Recordar actualizar ApiConstants.baseUrl en el frontend
```

---

## Checklist de Pre-Despliegue

- [ ] `Jwt__Key` seteada con clave de ≥ 32 caracteres
- [ ] `ConnectionStrings__DefaultConnection` apunta a la BD correcta
- [ ] La BD existe y tiene las tablas creadas (migrations ejecutadas)
- [ ] Puerto 5000 libre (`netstat -ano | Select-String ":5000"`)
- [ ] Frontend compila (`dart analyze` sin errores)
- [ ] Firewall permite conexiones al puerto 5000
- [ ] CORS configurado con los orígenes del frontend (`appsettings.json` → `Cors.Origins`)