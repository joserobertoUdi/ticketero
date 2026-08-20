# REGISTRO DE MODIFICACIONES

> Comparativa entre rama `main` (base original remota) y rama `dev` (cambios locales aplicados).
> Fecha: Julio 2026

---

## ACTUALIZACIÓN (Agosto 2026) — Flujo de atención y regla de tiempo máximo

### 1. Corrección: persistencia de cambios en atenciones activas

**Contexto:** Al cerrar la pestaña de la aplicación mientras un operador atendía un ticket,
`CerrarSesionUseCase` y `AbrirSesionUseCase` cargaban las atenciones activas con
`Atenciones.FindAsync(...)`, las modificaban (`FechaFin`, `EstadoTicketId`, etc.) y luego
llamaban `SaveChangesAsync`. Sin embargo, `GenericRepository.FindAsync` usaba `AsNoTracking()`,
por lo que las entidades quedaban **desprendidas** del `DbContext` y los cambios **no se
persistían** (el ticket quedaba "a la deriva", la cola bloqueada y `FechaFin == null`).

**Cambios:**
- `Repositories/GenericRepository.cs` — `FindAsync()` dejó de usar `AsNoTracking()`: ahora
  devuelve entidades rastreadas para que las modificaciones posteriores se persistan.
- `UseCases/LlamarTicketUseCase.cs` — al avanzar al siguiente ticket pendiente del área, ahora
  re-consulta el ticket con `GetByIdAsync` (rastreado) antes de modificar su `EstadoTicketId`,
  garantizando que el cambio se guarde.

### 2. Nueva regla: finalización automática de atenciones vencidas (20 minutos)

**Regla:** Toda atención activa (`FechaFin == null`) con más de **20 minutos** de duración se
finaliza automáticamente en todo el flujo.

**Cambios:**
- `Domain/Enums/ReglasAtencion.cs` **(NUEVO)** — Constante `MaximoMinutosAtencion = 20`.
- `Application/Interfaces/IUnitOfWork.cs` — Nueva firma
  `FinalizarAtencionesVencidasAsync(int minutosMaximo, CancellationToken = default)`.
- `Infrastructure/Repositories/UnitOfWork.cs` — Implementación: localiza atenciones activas con
  `FechaInicio <= ahora - N minutos`, les asigna `FechaFin`, `TiempoAtencionSegundos`,
  `EstadoTicketId = Cerrado` y observación automática; además cierra el ticket
  (`Cerrado` + `FechaCierre`). Persiste si encontró vencidas.
- La regla se invoca al inicio de **todos** los casos de uso del flujo:
  `LlamarTicketUseCase`, `AtenderTicketUseCase`, `CerrarTicketUseCase`, `DerivarTicketUseCase`,
  `CerrarSesionUseCase` y `AbrirSesionUseCase`.
- **Al cerrar la pestaña:** la regla corre antes del cierre normal de atenciones, de modo que si
  una atención ya superó el límite queda finalizada por la regla y el cierre de sesión procesa
  el resto.

### 3. Pruebas actualizadas y nuevas

**Corrección en pruebas de integración (`TicketFlujoIntegracionTests.cs`):**
- Se eliminó el uso de `ServicioId = 1` / `TipoTicketId = 1` "fantasma": ahora se siembran filas
  reales de `Servicio`, `TipoTicket` y las 8 de `EstadoTicket` (IDs 1-8 acorde al enum).
- Motivo: `IncludeAll()` usa navegaciones **requeridas** que EF traduce a `INNER JOIN`; al no
  existir las filas referenciadas, la consulta descartaba todos los tickets (cola vacía).

**Tests nuevos:**
| Test | Qué valida |
|------|------------|
| `CerrarSesion_FinalizaAtencionVencida_Automaticamente` | Atención de 25 min finalizada al cerrar la pestaña |
| `Llamar_AlInicioDelFlujo_FinalizaAtencionesVencidas` | La regla se aplica al llamar tickets: atención vencida se cierra y ticket queda `Cerrado` |

**Resultado:** Suite completa en verde — **97 pruebas, 0 errores**.

---

---

## RESUMEN GENERAL

| Indicador | Valor |
|-----------|-------|
| Archivos modificados | 29 |
| Archivos nuevos | 5 |
| Líneas añadidas | 2,617 |
| Líneas eliminadas | 327 |
| Commits en dev desde main | 6 (5 originales + 1 nuevo) |

---

## BACKEND - ApiTiketero (`backend/main` → `dev`)

### 1. CAPA API (Controllers, Hubs, Middleware, Mapping, Program)

#### `Controllers/DashboardController.cs`
- **Contexto:** El controlador generaba PDF inline usando QuestPDF directamente en el método de reporte.
- **Cambio:** Se extrajo la lógica de generación de PDF a un servicio dedicado `IPdfExportService` / `PdfExportService`.
- **Mejora:** Separación de responsabilidades, el controlador ahora solo prepara datos (`PdfExportData`) y delega la generación del PDF. Código más mantenible y testeable.

#### `Controllers/KioskosFisicosController.cs`
- **Contexto:** El controlador creaba/actualizaba activos fijos sin registro de auditoría y usaba `KioskoMediaId`.
- **Cambios:**
  - Eliminado `KioskoMediaId` del `CreateKioskoRequest` y `UpdateKioskoRequest`.
  - Agregado `CreadoPor` en nuevos activos fijos.
  - Agregada **auditoría completa** (`ActivoFijoAuditoria`) en operaciones CREAR, ACTUALIZAR y ELIMINAR de activos fijos.
  - Sistema de **upsert** (update/insert) en reemplazo de delete+insert masivo: ahora detecta cambios y solo actualiza o elimina lo necesario.
  - `GetCurrentUserName()` ahora también busca `ClaimTypes.Email` como fallback.
- **Mejora:** Trazabilidad completa de cambios en activos fijos; rendimiento al evitar delete masivo.

#### `Controllers/TicketsController.cs`
- **Contexto:** Las notificaciones SignalR se enviaban a `Clients.All`.
- **Cambio:** Cambiado a `Clients.Group($"area_{AreaActualId}")`.
- **Mejora:** Las notificaciones de tickets solo llegan a los clientes conectados al área correspondiente, no a todos.

#### `Controllers/UsersController.cs`
- **Contexto:** `GetById` hacía una consulta innecesaria (`GetByCorreoAsync("")`) y luego filtraba en memoria.
- **Cambio:** Simplificado a `GetByIdWithIncludesAsync(id)` directo.
- **Mejora:** Una sola consulta eficiente a la base de datos.

#### `Hubs/TicketHub.cs`
- **Contexto:** Tenía métodos redundantes `NotifyTicketCreated`, `NotifyTicketCalled`, `NotifyTicketStarted`, `NotifyTicketCompleted`, `NotifyTicketCancelled`, `NotifyQueueUpdated`.
- **Cambio:** Eliminados todos. Las notificaciones ahora se envían desde los controladores via `IHubContext`.
- **Mejora:** Eliminación de duplicación. Los controladores ya emiten eventos; los métodos del Hub eran redundantes.

#### `Mapping/MappingService.cs`
- **Contexto:** El mapping incluía `KioskoMediaId` y no exponía campos de auditoría de activos fijos.
- **Cambios:**
  - Eliminado `KioskoMediaId` del `KioskoFisicoResponse`.
  - Agregados `CreadoPor`, `UltimaModificacionPor`, `FechaModificacion` a `ActivoFijoResponse`.

#### `Middleware/ErrorHandlingMiddleware.cs`
- **Contexto:** Solo manejaba `InvalidOperationException`, `UnauthorizedAccessException` y genéricas.
- **Cambios:** Agregados catches para:
  - `DbUpdateConcurrencyException` → HTTP 409 Conflict.
  - `DbUpdateException` → HTTP 400 con detalle del error InnerException.
- **Mejora:** Manejo robusto de errores de base de datos y concurrencia.

#### `Program.cs`
- **Contexto:** No había registro del servicio de PDF.
- **Cambio:** Registrado `IPdfExportService` / `PdfExportService` como Scoped en DI.

#### `Services/PdfExportService.cs` **(NUEVO)**
- Servicio que encapsula la generación de PDF con QuestPDF.
- Recibe `PdfExportData` con estadísticas y genera el reporte completo.

---

### 2. CAPA APPLICATION (DTOs, Interfaces, UseCases)

#### `DTOs/FrontendDtos.cs`
- **Cambios:**
  - `TicketResponse`: Agregado `AtencionId` (int?).
  - `KioskoFisicoResponse`: Eliminado `KioskoMediaId`.
  - `ActivoFijoResponse`: Agregados `CreadoPor`, `UltimaModificacionPor`, `FechaModificacion`.
  - `KioskoFisicoCreateRequest`: Eliminado `KioskoMediaId`.
  - `KioskoFisicoUpdateRequest`: Eliminado `KioskoMediaId`.

#### `DTOs/PagedResponse.cs`
- **Contexto:** `PageSize` default era 20.
- **Cambio:** Cambiado a `0` (sin default). La lógica de `ApplyPaging` en `QueryableExtensions` clampea a mínimo 1.
- **Mejora:** El pageSize ahora es explícito; si no se envía, el backend usa el mínimo.

#### `Interfaces/IUnitOfWork.cs`
- **Cambio:** Agregada propiedad `IGenericRepository<ActivoFijoAuditoria> ActivosFijosAuditoria`.

#### `UseCases/CrearTicketUseCase.cs`
- **Contexto:** La generación de código de ticket usaba filtro `FechaCreacion >= hoy` para reiniciar secuencia diaria.
- **Cambio:** Eliminado filtro de fecha. La secuencia ahora es global (nunca se reinicia).
- **Mejora:** Código de ticket único a nivel global, no solo diario.

---

### 3. CAPA DOMAIN (Entities)

#### `Entities/ActivoFijo.cs`
- **Cambios:**
  - Agregado `CreadoPor` (string?).
  - Agregada navegación `ICollection<ActivoFijoAuditoria> Auditorias`.

#### `Entities/ActivoFijoAuditoria.cs` **(NUEVO)**
- Nueva entidad para auditoría de activos fijos.
- Propiedades: `Id`, `ActivoFijoId`, `KioskoId`, `UsuarioNombre`, `Accion` (CREAR/ACTUALIZAR/ELIMINAR), `CambioResumen`, `FechaCambio`.
- Navegaciones: `ActivoFijo`, `Kiosko`.

#### `Entities/Kiosko.cs`
- **Cambio:** Eliminado `KioskoMediaId` (int?).

---

### 4. CAPA INFRASTRUCTURE (Data, Migrations, Repositories)

#### `Data/Configurations/EntityConfigurations.cs`
- **Cambios:**
  - `KioskoConfiguration`: Eliminada propiedad `KioskoMediaId`.
  - `ActivoFijoConfiguration`: Agregados `CreadoPor` (max 100), `UltimaModificacionPor` (max 100), navegación `Auditorias` con `OnDelete(DeleteBehavior.SetNull)`.
  - **Nueva `ActivoFijoAuditoriaConfiguration`**: Define tabla `ActivosFijosAuditoria`, PK `AuditoriaId`, FK a `ActivoFijo` y `Kiosko`, índice por `FechaCambio`, `FechaCambio` con default `SYSDATETIME()`.

#### `Data/TicketeroDbContext.cs`
- **Cambio:** Agregado `DbSet<ActivoFijoAuditoria> ActivosFijosAuditoria` y su configuración.

#### `Migrations/20260722141409_AddActivoFijoAuditoriaAndRemoveKioskoMediaId.cs` **(NUEVO)**
- Migración que crea la tabla `ActivosFijosAuditoria` y elimina columna `KioskoMediaId` de `Kioskos`.

#### `Migrations/TicketeroDbContextModelSnapshot.cs`
- **Cambio:** Snapshot actualizado reflejando la nueva tabla y columnas agregadas/eliminadas.

#### `Repositories/GenericRepository.cs`
- **Cambios:**
  - `GetAllAsync()`: Agregado `AsNoTracking()`.
  - `FindAsync()`: Agregado `AsNoTracking()`.
  - `ExistsAsync()`: Optimizado de `GetByIdAsync` + null check a `AnyAsync`.
- **Mejora:** Reducción de tráfico de datos y seguimiento de entidades en consultas de solo lectura.

#### `Repositories/Repositories.cs`
- **Cambios:**
  - Agregado método privado `IncludeAllNoTracking()` en `TicketRepository`.
  - Varias queries cambiadas de `IncludeAll()` a `IncludeAllNoTracking()` (GetTicketsPorAreaAsync, GetTicketsPendientesPorAreaAsync, GetTicketsPendientesAsync, GetTicketsByDateRangeAsync).
  - `GetTicketsByDateRangeAsync`: Condición `<= fin` cambiada a `< fin` para evitar solapamiento.
  - `GetPuestosPorAreaAsync`: Agregado `Include(p => p.Sesiones)`.
- **Mejora:** Rendimiento en consultas de solo lectura; inclusión de sesiones en puestos.

#### `Repositories/UnitOfWork.cs`
- **Cambio:** Agregado `ActivosFijosAuditoria` con su lazy initialization.

---

### 5. TESTS (nuevos)

**Archivos nuevos en `tests/Ticketero.Tests/`:**

| Archivo | Descripción |
|---------|-------------|
| `Ticketero.Tests.csproj` | Proyecto de tests con xUnit, Moq, Shouldly |
| `AtenderTicketUseCaseTests.cs` | Tests: validar que usuario pertenezca al área antes de atender ticket |
| `DerivarTicketUseCaseTests.cs` | Tests: validar que usuario pertenezca al área antes de derivar ticket |
| `MappingServiceTests.cs` | Tests: mapeo de roles (BD→Frontend y Frontend→BD), estados, formateo de tiempo |
| `MappingServiceDerivadoDeTests.cs` | Tests: campo `DerivadoDe` en tickets derivados vs no derivados |
| `PagedResponseTests.cs` | Tests: TotalPages, HasPreviousPage, HasNextPage, defaults de PagedRequest |
| `QueryableExtensionsTests.cs` | Tests: ApplyPaging y ApplySorting con diferentes configuraciones |
| `TipoActivoValidatorTests.cs` | Tests: validación de tipos de activo (case-insensitive, valores válidos/inválidos) |

**Archivo modificado:**
- `PagedResponseTests.cs`: Test `PagedRequest_ShouldHaveDefaultPageSizeOfTwenty` actualizado a esperar `0` en vez de `20`.

---

## FRONTEND - SistemaTicket (`frontend/main` → `dev`)

### Cambios en Flutter/Dart

#### `lib/core/utils/time_sync_service.dart`
- **Cambio:** `_maxDrift` aumentado de 2 segundos a 5 segundos.
- **Motivo:** Mayor tolerancia a desviaciones de reloj entre dispositivos de red.

#### `lib/data/models/kiosko_fisico_model.dart`
- **Cambio:** Eliminado campo `kioskoMediaId` del modelo, constructor, `fromJson`, `toJson` y `copyWith`.

#### `lib/domain/entities/kiosko_fisico.dart`
- **Cambio:** Eliminado campo `kioskoMediaId` de la entidad, constructor, `fromJson`, `toJson` y `copyWith`.

#### `lib/presentation/providers/settings_provider.dart`
- **Cambio:** Eliminado `_selectedKioskoMediaId`, `_keySelectedKioskoMedia` y todos los métodos getter/setter asociados.

#### `lib/presentation/screens/kiosko_selection/kiosko_selection_screen.dart`
- **Cambios:**
  - Agregada inicialización de `TimeSyncService` para sincronización horaria.
  - Mejorada detección de red: ahora usa PowerShell (`Get-NetIPAddress`, `Get-NetRoute`, `Get-DnsClientServerAddress`) en lugar de métodos básicos.
  - Eliminado método `_findMedia()` y todas las referencias a `KioskoMedia`.
  - Agregada advertencia visual cuando se detecta desviación horaria > 5s.

#### `lib/presentation/screens/settings/settings_screen.dart`
- **Cambio:** Eliminado `KioskoMediaPanel` de la lista de paneles de configuración.

#### `lib/presentation/screens/settings/widgets/kioskos_administracion_panel.dart`
- **Cambios:**
  - Eliminado `DropdownButtonFormField` para seleccionar `KioskoMedia`.
  - Agregados `TextField` directos para `logoUrl` y `videoUrl`.
  - Payload de creación/actualización ahora envía `logoUrl`/`videoUrl` directamente.
  - Health check endpoint cambiado de `/api/health` a `/healthz`.

#### `lib/window/caller_app.dart`
- **Cambios:**
  - Agregado `TimeSyncService.initialize()` en `_initWindow()`.
  - Agregado `Provider<TimeSyncService>` en la lista de providers.
  - Eliminado `kioskoMediaId` en las llamadas a la API.

---

## BASE DE DATOS

### Cambios en esquema (vía Migrations EF Core, NO en scripts SQL)

| Cambio | Detalle |
|--------|---------|
| **Tabla nueva** | `ActivosFijosAuditoria` (dbo) |
| Columnas | `AuditoriaId` (PK, int, identity), `ActivoFijoId` (int, FK → ActivosFijos), `KioskoId` (int, FK → Kioskos), `UsuarioNombre` (nvarchar(100)), `Accion` (nvarchar(20)), `CambioResumen` (nvarchar(500)), `FechaCambio` (datetime2, default SYSDATETIME()) |
| FK | `ActivoFijoId` → ActivosFijos (SET NULL), `KioskoId` → Kioskos (SET NULL) |
| Índices | `IX_ActivosFijosAuditoria_Fecha` sobre `FechaCambio` |
| **Columna eliminada** | `KioskoMediaId` de tabla `Kioskos` |
| **Columnas agregadas** | `CreadoPor` (nvarchar(100)), `UltimaModificacionPor` (nvarchar(100)), `FechaModificacion` (datetime2) en tabla `ActivosFijos` |

### Scripts SQL (BD_Ticketero/)
- **Sin cambios.** Los 20 scripts `.sql` originales se mantienen intactos.
- La nueva tabla y columnas se aplican exclusivamente mediante Migrations de Entity Framework Core.

---

## LISTA COMPLETA DE ARCHIVOS MODIFICADOS vs `main`

### Backend (ApiTiketero/) - 26 archivos

```
MODIFICADOS (21):
  ApiTiketero/Ticketero/src/Ticketero.Api/Controllers/DashboardController.cs
  ApiTiketero/Ticketero/src/Ticketero.Api/Controllers/KioskosFisicosController.cs
  ApiTiketero/Ticketero/src/Ticketero.Api/Controllers/TicketsController.cs
  ApiTiketero/Ticketero/src/Ticketero.Api/Controllers/UsersController.cs
  ApiTiketero/Ticketero/src/Ticketero.Api/Hubs/TicketHub.cs
  ApiTiketero/Ticketero/src/Ticketero.Api/Mapping/MappingService.cs
  ApiTiketero/Ticketero/src/Ticketero.Api/Middleware/ErrorHandlingMiddleware.cs
  ApiTiketero/Ticketero/src/Ticketero.Api/Program.cs
  ApiTiketero/Ticketero/src/Ticketero.Application/DTOs/FrontendDtos.cs
  ApiTiketero/Ticketero/src/Ticketero.Application/DTOs/PagedResponse.cs
  ApiTiketero/Ticketero/src/Ticketero.Application/Interfaces/IUnitOfWork.cs
  ApiTiketero/Ticketero/src/Ticketero.Application/UseCases/CrearTicketUseCase.cs
  ApiTiketero/Ticketero/src/Ticketero.Domain/Entities/ActivoFijo.cs
  ApiTiketero/Ticketero/src/Ticketero.Domain/Entities/Kiosko.cs
  ApiTiketero/Ticketero/src/Ticketero.Infrastructure/Data/Configurations/EntityConfigurations.cs
  ApiTiketero/Ticketero/src/Ticketero.Infrastructure/Data/TicketeroDbContext.cs
  ApiTiketero/Ticketero/src/Ticketero.Infrastructure/Migrations/TicketeroDbContextModelSnapshot.cs
  ApiTiketero/Ticketero/src/Ticketero.Infrastructure/Repositories/GenericRepository.cs
  ApiTiketero/Ticketero/src/Ticketero.Infrastructure/Repositories/Repositories.cs
  ApiTiketero/Ticketero/src/Ticketero.Infrastructure/Repositories/UnitOfWork.cs
  ApiTiketero/Ticketero/tests/Ticketero.Tests/PagedResponseTests.cs

NUEVOS (5):
  ApiTiketero/Ticketero/src/Ticketero.Api/Services/PdfExportService.cs
  ApiTiketero/Ticketero/src/Ticketero.Domain/Entities/ActivoFijoAuditoria.cs
  ApiTiketero/Ticketero/src/Ticketero.Infrastructure/Migrations/20260722141409_AddActivoFijoAuditoriaAndRemoveKioskoMediaId.cs
  ApiTiketero/Ticketero/src/Ticketero.Infrastructure/Migrations/20260722141409_AddActivoFijoAuditoriaAndRemoveKioskoMediaId.Designer.cs
  ApiTiketero/Ticketero/tests/Ticketero.Tests/ (carpeta completa: 8 archivos)
```

### Frontend (SistemaTicket/) - 7 archivos

```
MODIFICADOS (7):
  SistemaTicket/sistema_ticketero/lib/core/utils/time_sync_service.dart
  SistemaTicket/sistema_ticketero/lib/data/models/kiosko_fisico_model.dart
  SistemaTicket/sistema_ticketero/lib/domain/entities/kiosko_fisico.dart
  SistemaTicket/sistema_ticketero/lib/presentation/providers/settings_provider.dart
  SistemaTicket/sistema_ticketero/lib/presentation/screens/kiosko_selection/kiosko_selection_screen.dart
  SistemaTicket/sistema_ticketero/lib/presentation/screens/settings/settings_screen.dart
  SistemaTicket/sistema_ticketero/lib/presentation/screens/settings/widgets/kioskos_administracion_panel.dart
  SistemaTicket/sistema_ticketero/lib/window/caller_app.dart
```

### Documentación - 1 archivo nuevo

```
NUEVOS (1):
  GAPS_DETALLADOS.md
```

---

## MEJORAS CLAVE POR CATEGORÍA

| Categoría | Mejora |
|-----------|--------|
| **Eliminación KioskoMedia** | Se eliminó la entidad KioskoMedia de backend y frontend. `logoUrl`/`videoUrl` ahora se asignan directamente al kiosko físico. |
| **Auditoría** | Nuevo sistema de auditoría para Activos Fijos con trazabilidad completa (CREAR, ACTUALIZAR, ELIMINAR) incluyendo usuario y resumen de cambios. |
| **Rendimiento BD** | `AsNoTracking()` en consultas de solo lectura; `AnyAsync` para exists; secuencia de tickets global sin filtro fecha. |
| **Notificaciones SignalR** | De `Clients.All` a `Clients.Group` por área; eliminación de métodos redundantes en Hub. |
| **Manejo de errores** | Captura de `DbUpdateException` y `DbUpdateConcurrencyException` con códigos HTTP apropiados. |
| **PDF Export** | Refactorizado a servicio dedicado con inyección de dependencias. |
| **Tests unitarios** | Suite completa de tests con xUnit + Moq + Shouldly para casos de uso, mapping, paginación y validación. |
| **Sincronización horaria** | TimeSyncService con detección de desviación > 5s; detección de red mejorada via PowerShell. |
| **Health check** | Endpoint cambiado de `/api/health` a `/healthz` en frontend. |
