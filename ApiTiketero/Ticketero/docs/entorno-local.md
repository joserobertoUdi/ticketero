# Entorno local de desarrollo

Guía para levantar la API Ticketero en una máquina de desarrollo, con la base de datos
restaurada desde `ApiTiketero/respaldo.bak`.

> Para producción, ver `docs/guia-despliegue.md` y `docs/conexion-bd-variable-entorno.md`.

---

## Entorno de referencia

| Componente | Valor |
|---|---|
| Instancia SQL | `localhost` (instancia por defecto, SQL Server 2025 — `17.0.1000.7`) |
| Base de datos | `DBTicketero_Dev`, creada por migraciones EF Core (`__EFMigrationsHistory` presente) |
| Autenticación | El script prueba Windows Auth; si falla, pide credenciales SQL (`sa`) |
| SDK | **.NET 10** — los `.csproj` declaran `net10.0`. El README dice .NET 8 y está desactualizado |
| PowerShell | Windows PowerShell 5.1, o PowerShell 7 con el módulo `SqlServer` |

---

## Uso rápido

```powershell
cd C:\proyectos\activeRepository\ticketero\ApiTiketero\Ticketero\scripts
.\Levantar-Local.ps1
```

El script ejecuta cinco pasos y se detiene con un mensaje claro si alguno falla:

1. **Requisitos** — comprueba el SDK 10.x y resuelve la autenticación contra `localhost`.
2. **Restauración** — *omitida por defecto*. Solo se ejecuta con `-Restaurar`, y aun así pide
   confirmación escrita antes de sobrescribir.
3. **Variables de entorno** — ámbito `User`, no requiere permisos de administrador.
4. **Validación de conexión** — se conecta a `DBTicketero_Dev`, lista todas las tablas con su
   número de filas, contrasta contra las 22 tablas del modelo EF Core y muestra la última
   migración aplicada.
5. **Arranque** — compila, ejecuta la API y comprueba `/healthz` y `/ready`
   (`/ready` incluye el health check de SQL Server, así que un 200 confirma la conexión real).

### Variantes

```powershell
.\Levantar-Local.ps1 -Usuario sa                        # ir directo a autenticación SQL
.\Levantar-Local.ps1 -SoloValidar                       # variables + validación, sin levantar la API
.\Levantar-Local.ps1 -BaseDatos DBTicketero             # apuntar a otra base
.\Levantar-Local.ps1 -Restaurar -BaseDatos DBTicketero  # restaurar respaldo.bak (destructivo)
```

> `-Restaurar` es opt-in a propósito: `DBTicketero_Dev` ya está poblada, y una restauración
> desde `respaldo.bak` la sobrescribiría por completo.

---

## Variables de entorno que define

Ámbito `User` (persistentes, visibles en nuevas terminales):

| Variable | Valor local |
|---|---|
| `ConnectionStrings__DefaultConnection` | `Data Source=localhost;Initial Catalog=DBTicketero_Dev;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True` |
| `Jwt__Key` | clave de desarrollo de 47 caracteres (el mínimo que exige `Program.cs` es 32) |
| `Jwt__Issuer` | `TicketeroApiDev` |
| `Jwt__Audience` | `TicketeroClientDev` |

Si se usa autenticación SQL, la cadena lleva `User ID=sa;Password=...` **en texto plano** en la
variable de usuario. El script lo advierte. Para evitarlo, dale a tu usuario de Windows un login
en SQL Server con permisos sobre `DBTicketero_Dev` y el script usará Windows Auth.

`ASPNETCORE_ENVIRONMENT=Development` no se persiste: lo aporta el perfil `http` de
`launchSettings.json`, y persistirlo afectaría a cualquier otra app ASP.NET del usuario.

**`ConnectionStrings__Redis` se deja sin definir a propósito.** `Program.cs` cae entonces a
`AddDistributedMemoryCache()` y a SignalR sin backplane, que es lo correcto en local.

Verificar en una terminal nueva:

```powershell
[Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection", "User")
```

---

## `appsettings.Development.json`

El archivo está en `.gitignore` y se generó junto con este script. Mantiene
`"DefaultConnection": ""` a propósito: según `docs/cambios-conexion-bd.md`, un valor de
respaldo aquí enmascararía la ausencia de la variable de entorno y conectaría en silencio a
otra base. Con el campo vacío, el fallo es explícito (`ConnectionString no inicializado`).

También deja `"Cors": { "Origins": [] }` — con la lista vacía `Program.cs` aplica
`AllowAnyOrigin()`, cómodo para el cliente Flutter en local.

---

## Endpoints tras el arranque

| URL | Qué es |
|---|---|
| `http://localhost:5000/swagger` | Swagger UI (solo en Development) |
| `http://localhost:5000/healthz` | Liveness |
| `http://localhost:5000/ready` | Readiness, **incluye** el health check de SQL Server |
| `http://localhost:5000/ticketHub` | Hub de SignalR |

Logs: `Ticketero/logs/api-local.out.log` (salida del proceso) y
`Ticketero/logs/ticketero-{fecha}.log` (Serilog).

Detener la API: `Stop-Process -Id <PID>` — el script imprime el PID al arrancar.

---

## Problemas frecuentes

**`Operating system error 5 (Access is denied)` al restaurar**
La cuenta del servicio SQL no puede leer `respaldo.bak`. En PowerShell como Administrador:

```powershell
icacls 'C:\proyectos\activeRepository\ticketero\ApiTiketero' /grant 'NT SERVICE\MSSQLSERVER:(OI)(CI)RX'
```

**`Login failed for user`**
Windows Auth no está habilitado para tu usuario. Ejecuta con `-Usuario sa` y el script pedirá la
contraseña de forma segura (`Get-Credential`, sin quedar en el historial).

**`Invalid object name 'dbo.X'`**
Falta una tabla del modelo EF Core. El paso 4 del script lista exactamente cuáles antes de que la
API arranque. Si aparece, aplica migraciones:
`dotnet ef database update --project src\Ticketero.Infrastructure --startup-project src\Ticketero.Api`

**`Jwt:Key no está configurada`**
La terminal se abrió antes de crear las variables. Abre una nueva o ejecuta el script de nuevo
(también las define en el proceso actual).

**No se detecta el SDK 10**
`dotnet --list-sdks`. El proyecto no compila con .NET 8 pese a lo que indica el README.
