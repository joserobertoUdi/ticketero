# Conexión a Base de Datos mediante Variable de Entorno

## 1. Modificación al proyecto

Se agregó `.AddEnvironmentVariables()` en `TicketeroDbContextFactory.cs` (línea 14) para que el factory de diseño (migraciones) también lea las variables de entorno, igual que ya lo hace la API en runtime mediante `WebApplication.CreateBuilder()`.

```diff
 var configuration = new ConfigurationBuilder()
     .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Ticketero.Api"))
     .AddJsonFile("appsettings.json")
+    .AddEnvironmentVariables()
     .Build();
```

Además se agregó el paquete NuGet `Microsoft.Extensions.Configuration.EnvironmentVariables` a `Ticketero.Infrastructure.csproj`.

## 2. Jerarquía de configuración en .NET

1. **Variables de entorno** ← máxima prioridad
2. `appsettings.{Environment}.json` (ej: `appsettings.Development.json`)
3. `appsettings.json` (base)

El connection string se resuelve mediante `configuration.GetConnectionString("DefaultConnection")`, que busca en todas las fuentes en este orden.

## 3. Crear la variable de entorno (ámbito Machine)

Abrir **PowerShell como Administrador**:

```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
    "Server=localhost\SQLEXPRESS;Database=DBTicketero;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Machine")
```

> **Nota:** Si se ejecuta desde una terminal que ya estaba abierta antes de crear la variable, la terminal no la hereda automáticamente porque cargó su bloque de entorno al iniciarse. Soluciones:
> - Abrir una **nueva terminal**
> - Forzar la variable en el proceso actual:
>   ```powershell
>   $env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=DBTicketero;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
>   ```

## 4. Verificar que la variable existe

```powershell
[Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Machine")
```

Debería devolver el connection string completo.

## 5. Aplicar migraciones (crear BD y tablas)

```powershell
cd C:\Users\becadesarrollo\Desktop\Proyectos\ApiTiketero\Ticketero
dotnet ef database update --project src\Ticketero.Infrastructure --startup-project src\Ticketero.Api
```

Este comando crea la base de datos `DBTicketero` (si no existe) y aplica todas las migraciones.

## 6. Ejecutar la API

```powershell
dotnet run --project src\Ticketero.Api
```

La salida esperada incluye:

- Consultas EF Core exitosas a las tablas del esquema `dbo`
- `Ticketero API iniciada correctamente`
- `Now listening on: http://[::]:5000`

## 7. Cuadro de verificación

| Paso | Resultado esperado | Comprobación |
|---|---|---|
| Variable creada en Machine | `GetEnvironmentVariable` devuelve el connection string | ✅ |
| Sin variable de entorno | Error: `ConnectionString no inicializado` | ✅ |
| Variable presente + BD sin migraciones | Error: `Invalid object name 'dbo.Roles'` | ✅ |
| Migraciones aplicadas | `Applying migration... Done.` | ✅ |
| API ejecutándose | `Ticketero API iniciada correctamente` en consola | ✅ |
| Seed ejecutado | Roles, estados, prioridades, usuarios creados automáticamente | ✅ |
| API conectada a `DBTicketero` (no `DBTicketero_Dev`) | Usa la variable de entorno, no `appsettings.Development.json` | ✅ |

## 8. Dónde ver la variable en Windows (interfaz gráfica)

**Panel de Control → Sistema → Configuración avanzada del sistema → Variables de entorno → Variables del sistema**

Allí aparece listada como `ConnectionStrings__DefaultConnection`.

## 9. Uso en producción

Solo cambia el valor de la variable `Machine` con los datos del servidor de producción:

```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection",
    "Server=IP_O_HOST;Database=DBTicketero;User=sa;Password=CONTRA;TrustServerCertificate=true;MultipleActiveResultSets=true",
    "Machine")
```

El código de la API no requiere ningún cambio adicional.

## 10. Notas importantes

- Las variables de entorno en Windows **no distinguen mayúsculas/minúsculas** (case-insensitive).
- La variable debe crearse con ámbito **Machine** para que esté disponible independientemente del usuario que ejecute la API.
- La API solo necesita la variable en **runtime**. Las migraciones la necesitan en **design-time** (por eso se modificó el `DbContextFactory`).
- Los `appsettings.json` y `appsettings.Development.json` tienen `"DefaultConnection": ""` como fallback, forzando el uso de la variable de entorno.
