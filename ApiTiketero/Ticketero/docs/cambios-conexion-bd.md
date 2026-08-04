# Cambios realizados para la conexión a BD mediante variable de entorno

## Contexto

Originalmente la API obtenía el connection string exclusivamente desde los archivos `appsettings.json`. Para producción se requiere que la conexión a la base de datos se configure mediante una **variable de entorno de Windows a nivel Machine**, de modo que:

- Las credenciales no queden硬codeadas en archivos del repositorio.
- La conexión sea configurable por entorno (desarrollo, producción) sin modificar el código.
- Sea independiente del usuario que ejecuta la API.

.NET ya soporta variables de entorno en runtime mediante `WebApplication.CreateBuilder()`, pero el **design-time factory** (`TicketeroDbContextFactory.cs`) usado por `dotnet ef` para migraciones no las incluía.

---

## Archivos modificados

### 1. `src/Ticketero.Infrastructure/Data/TicketeroDbContextFactory.cs`

**Cambio:** Se agregó `.AddEnvironmentVariables()` al `ConfigurationBuilder`.

```diff
 public TicketeroDbContext CreateDbContext(string[] args)
 {
     var configuration = new ConfigurationBuilder()
         .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Ticketero.Api"))
         .AddJsonFile("appsettings.json")
+        .AddEnvironmentVariables()
         .Build();

     var optionsBuilder = new DbContextOptionsBuilder<TicketeroDbContext>();
     optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

     return new TicketeroDbContext(optionsBuilder.Options);
 }
```

**Propósito:** Sin esta línea, al ejecutar `dotnet ef database update`, el factory solo leía `appsettings.json` e ignoraba la variable de entorno. Con el cambio, el factory consulta ambas fuentes, respetando la precedencia de variables de entorno sobre el JSON.

---

### 2. `src/Ticketero.Infrastructure/Ticketero.Infrastructure.csproj`

**Cambio:** Se agregó el paquete NuGet `Microsoft.Extensions.Configuration.EnvironmentVariables`.

```diff
 <ItemGroup>
+    <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="10.0.9" />
     <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.9" />
     ...
 </ItemGroup>
```

**Propósito:** El método de extensión `.AddEnvironmentVariables()` pertenece a este paquete. Sin él, el código no compila porque `IConfigurationBuilder` no reconoce el método.

---

### 3. `src/Ticketero.Api/appsettings.Development.json`

**Cambio:** Se vació el connection string para evitar que actúe como fallback y enmascare la ausencia de la variable de entorno.

```diff
 "ConnectionStrings": {
-    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=DBTicketero_Dev;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
+    "DefaultConnection": ""
 },
```

**Propósito:** En la jerarquía de configuración de .NET, `appsettings.Development.json` tiene menor prioridad que las variables de entorno. Sin embargo, si la variable de entorno no existe, .NET caía a este archivo y conectaba a `DBTicketero_Dev` silenciosamente, dando la falsa impresión de que la variable funcionaba. Al dejarlo vacío, la ausencia de la variable provoca un error claro (`ConnectionString no inicializado`).

---

## Archivos NO modificados (funcionan igual)

| Archivo | Motivo |
|---|---|
| `src/Ticketero.Api/Program.cs` | `WebApplication.CreateBuilder(args)` ya incluye `AddEnvironmentVariables()` por defecto. No requiere cambios. |
| `src/Ticketero.Infrastructure/Extensions/InfrastructureServiceRegistration.cs` | Usa `configuration.GetConnectionString("DefaultConnection")` que ya consulta todas las fuentes configuradas. |
| `src/Ticketero.Api/appsettings.json` | Ya tenía `"DefaultConnection": ""` como fallback genérico. |

---

## Flujo de resolución del connection string

```
dotnet run / dotnet ef database update
         │
         ▼
IConfiguration.GetConnectionString("DefaultConnection")
         │
         ├── 1. Variable de entorno (Machine) ──── Si existe → 🏆 Gana
         │
         ├── 2. appsettings.Development.json ───── Si está vacío → se ignora
         │
         └── 3. appsettings.json ───────────────── Si está vacío → se ignora
                                                          │
                                                          ▼
                                              Error: "ConnectionString no inicializado"
```

---

## Requisito externo

El usuario debe crear la variable de entorno en Windows antes de ejecutar la API:

```
Nombre:   ConnectionStrings__DefaultConnection
Valor:    Server=localhost\SQLEXPRESS;Database=DBTicketero;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true
Ámbito:   Machine
```

El doble guion bajo `__` es el separador universal que .NET usa para representar `:` en nombres jerárquicos de configuración (`ConnectionStrings:DefaultConnection`).
