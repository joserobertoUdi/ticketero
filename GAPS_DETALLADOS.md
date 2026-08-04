# Análisis Detallado de Gaps — Sistema Ticketero

> Fecha: 22/07/2026
> Objetivo: Documentar gaps pendientes con análisis de código, impacto, y propuesta de solución.

---

## Índice

1. [C12 / V7.7 — Validar al menos un área por kiosko](#1-c12--v77--validar-al-menos-un-área-por-kiosko)
2. [V1.1 / V1.5 — Sincronización hora kiosko Flutter con servidor](#2-v11--v15--sincronización-hora-kiosko-flutter-con-servidor)
3. [C10 / V6.5 — Auditoría de cambios en activos fijos](#3-c10--v65--auditoría-de-cambios-en-activos-fijos)
4. [C13 / V2.6 / V7.5 — Campo KioskoMediaId huérfano sin FK](#4-c13--v26--v75--campo-kioskomediaid-huérfano-sin-fk)

---

## 1. C12 / V7.7 — Validar al menos un área por kiosko

### Estado actual

**✅ Verificado: El gap ya está resuelto.** Existen 3 capas de validación activas:

#### 1. Frontend (Flutter)

**Archivo:** `SistemaTicket/sistema_ticketero/lib/presentation/screens/settings/widgets/kioskos_administracion_panel.dart:337-347`

```dart
bool _validateAll(Map<String, String?> errors,
    {required String nombre, required String ubicacion, required List<int> areaIds}) {
  errors.clear();
  errors['nombre'] = _validateField('nombre', nombre, required: true, maxLength: 100);
  errors['ubicacion'] = _validateField('ubicacion', ubicacion, required: true, maxLength: 150);
  if (areaIds.isEmpty) {
    errors['areaIds'] = 'Debe seleccionar al menos un área';
  }
  errors.removeWhere((_, v) => v == null);
  return errors.isEmpty;
}
```

#### 2. DTO Data Annotations (Backend)

**Archivo:** `ApiTiketero/Ticketero/src/Ticketero.Application/DTOs/FrontendDtos.cs:231-296` (Create)

```csharp
[Required(ErrorMessage = "Debe seleccionar al menos un área")]
[MinLength(1, ErrorMessage = "Debe seleccionar al menos un área")]
public List<int> AreaIds { get; set; } = new();
```

El mismo `[MinLength(1)]` existe en `KioskoFisicoUpdateRequest` (línea 341-397).

#### 3. Controller explícito (Backend)

**Archivo:** `ApiTiketero/Ticketero/src/Ticketero.Api/Controllers/KioskosFisicosController.cs:153-154`

```csharp
if (request.AreaIds == null || request.AreaIds.Count == 0)
    return BadRequest(new { mensaje = "Debe seleccionar al menos un área para el kiosko" });
```

Y en `Actualizar` (líneas 296-297):

```csharp
if (request.AreaIds.Count == 0)
    return BadRequest(new { mensaje = "Debe seleccionar al menos un área para el kiosko" });
```

### Observación

En `Actualizar`, cuando `request.AreaIds` es `null` (no enviado en el JSON), el bloque de actualización de áreas se salta completamente (línea 294: `if (request.AreaIds != null)`). Esto es correcto para PATCH parcial. Sin embargo, si se envía `"areaIds": []`, la validación lo atrapa.

### Veredicto

**No requiere acción.** La validación funciona en frontend, DTO, y controller. El gap fue resuelto antes de que el plan de corrección lo documentara.

---

## 2. V1.1 / V1.5 — Sincronización hora kiosko Flutter con servidor

### Estado actual

**⚠️ Parcialmente implementado.** El backend tiene el endpoint y el Flutter tiene el servicio, pero hay deficiencias en la integración.

### Análisis

#### Backend: ✅ Implementado

**Archivo:** `ApiTiketero/Ticketero/src/Ticketero.Api/Controllers/TimeController.cs`

```csharp
[Route("api/time")]
[AllowAnonymous]
[HttpGet]
public IActionResult GetServerTime()
{
    var now = DateTime.UtcNow;
    return Ok(new {
        serverTime = now.ToString("o"),
        serverTimeUtcTicks = now.Ticks,
        unixTimestampSeconds = ...,
        timezone = TimeZoneInfo.Local.Id,
        timezoneUtcOffset = ...,
        isUtc = ...
    });
}
```

Retorna: hora UTC en ISO 8601, ticks, unix timestamp, zona horaria del servidor, offset UTC.

#### TimeSyncService (Flutter): ✅ Implementado

**Archivo:** `SistemaTicket/sistema_ticketero/lib/core/utils/time_sync_service.dart`

- Singleton, se inicializa en `main.dart:83-84`
- Hace `GET /api/time`, calcula offset restando latencia estimada
- Sincronización periódica cada 5 minutos
- Provee `serverNow()`, `localFromServer()`, `serverFromLocal()`
- Tiene `isDriftAcceptable` con tolerancia de 2 segundos

**Inicialización en main.dart:**

```dart
final timeSyncService = TimeSyncService();
unawaited(timeSyncService.initialize()); // línea 83-84
```

**Uso en providers:** TicketProvider (línea 104) y DashboardProvider (línea 115) reciben `TimeSyncService` por constructor y lo usan consistentemente.

#### Deficiencias detectadas

1. **CallerWindowApp no tiene TimeSyncService**
   - **Archivo:** `SistemaTicket/sistema_ticketero/lib/window/caller_app.dart`
   - La ventana secundaria "caller" (pantalla llamadora pública) no inicializa `TimeSyncService`. Usa `DateTime.now()` local sin ajuste.
   - **Impacto:** La pantalla llamadora muestra hora local del dispositivo, no hora sincronizada con el servidor.

2. **KioskoSelectionScreen no sincroniza explícitamente**
   - **Archivo:** `SistemaTicket/sistema_ticketero/lib/presentation/screens/kiosko_selection/kiosko_selection_screen.dart`
   - Aunque `TimeSyncService` se inicializa en `main.dart`, la pantalla de selección de kiosko no espera a que la sincronización inicial termine antes de proceder.
   - No hay indicador visual de "sincronizando hora..." en el kiosko.
   - **Impacto:** Los tickets creados en los primeros segundos después de abrir la app podrían tener timestamps locales.

3. **No hay indicador de drift en UI**
   - `TimeSyncService` calcula `isDriftAcceptable` pero ningún provider ni widget lo expone.
   - Si el offset supera 2 segundos, no hay alerta ni advertencia visual.

### Solución propuesta

| # | Acción | Archivo | Prioridad |
|---|--------|---------|-----------|
| 1 | Inicializar `TimeSyncService` en `CallerWindowApp` | `caller_app.dart` | Alta |
| 2 | Agregar `await timeSyncService.initialize()` con indicador de carga en kiosko | `kiosko_selection_screen.dart` | Alta |
| 3 | Exponer `isSynced` / `isDriftAcceptable` en un Provider y mostrar advertencia si hay drift excesivo | Nuevo provider o extender SettingsProvider | Media |
| 4 | Mostrar hora sincronizada en la UI del kiosko (ej. esquina superior derecha) | `kiosko_selection_screen.dart`, `TicketSelectionScreen` | Baja |

---

## 3. C10 / V6.5 — Auditoría de cambios en activos fijos

### Estado actual

**❌ No hay auditoría histórica.** Solo se conserva el último usuario que modificó, y se pierde al actualizar.

### Análisis

#### Estructura actual

**Entidad ActivoFijo:** `ApiTiketero/Ticketero/src/Ticketero.Domain/Entities/ActivoFijo.cs`

```csharp
public class ActivoFijo : EntityBase
{
    public int KioskoId { get; set; }
    public string TipoActivo { get; set; } = string.Empty;
    public string NumeroActivo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Serie { get; set; }
    public string? UltimaModificacionPor { get; set; }    // ← Solo el último
    public DateTime? FechaModificacion { get; set; }       // ← Solo la última fecha
    public Kiosko? Kiosko { get; set; }
}
```

**EntityBase:** `ApiTiketero/Ticketero/src/Ticketero.Domain/Entities/EntityBase.cs`

```csharp
public abstract class EntityBase
{
    public int Id { get; protected set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaReg { get; set; } = DateTime.UtcNow; // ← Fecha creación
    public Guid Ride { get; set; } = Guid.NewGuid();
}
```

**Problema principal en Actualizar:** `KioskosFisicosController.cs:433-452`

```csharp
// DELETE + RECREATE — destruye la auditoría
var existingActivos = await _unitOfWork.ActivosFijos.FindAsync(a => a.KioskoId == id);
foreach (var ea in existingActivos)
    await _unitOfWork.ActivosFijos.DeleteAsync(ea);  // ← Borra TODO

var userName = GetCurrentUserName();
foreach (var af in request.ActivosFijos)
{
    await _unitOfWork.ActivosFijos.AddAsync(new ActivoFijo
    {
        // ... se crean NUEVOS registros con nuevos Id, nueva FechaReg
        UltimaModificacionPor = userName,
        FechaModificacion = DateTime.UtcNow
    });
}
```

**Problemas específicos:**

1. **Pérdida de historial:** Al borrar y recrear, se pierde:
   - `FechaReg` original (cuándo se agregó el activo por primera vez)
   - `Id` original (cualquier referencia externa se rompe)
   - Quién creó el activo originalmente (no hay campo `CreadoPor`)
   - Historial de cambios intermedios

2. **Sin CreadoPor:** No existe campo `CreadoPor` en `EntityBase` ni en `ActivoFijo`. Solo se sabe quién modificó por última vez.

3. **Sin tabla de auditoría:** No hay un log histórico de cambios con (qué cambió, quién, cuándo, valor anterior, valor nuevo).

4. **Response DTO no expone auditoría:** `ActivoFijoResponse` (FrontendDtos.cs:214-223) no incluye `UltimaModificacionPor` ni `FechaModificacion`.

### Solución propuesta

#### Opción A (Recomendada): Tabla de auditoría dedicada

Crear `AuditoriaActivoFijo` para historial inmutable:

```sql
CREATE TABLE AuditoriaActivoFijo (
    Id INT IDENTITY PRIMARY KEY,
    ActivoFijoId INT NOT NULL,
    UsuarioId INT NOT NULL,
    Accion VARCHAR(20) NOT NULL, -- CREAR, ACTUALIZAR, ELIMINAR
    CambioResumen NVARCHAR(500), -- ej: "TipoActivo: Monitor → CPU, NumeroActivo: A001 → A002"
    FechaCambio DATETIME2 DEFAULT SYSDATETIME()
);
```

#### Opción B (Mínima): Mejorar campos existentes

1. Agregar `CreadoPor` (string o int UsuarioId) a `ActivoFijo` o `EntityBase`
2. Cambiar `Actualizar` para hacer UPDATE en lugar de DELETE+INSERT:
   - Actualizar `UltimaModificacionPor`, `FechaModificacion`
   - No perder `FechaReg`, `CreadoPor`
3. Exponer `UltimaModificacionPor` y `FechaModificacion` en `ActivoFijoResponse`

#### Cambios necesarios

| # | Archivo | Cambio |
|---|---------|--------|
| 1 | `ActivoFijo.cs` | Agregar `public string? CreadoPor { get; set; }` |
| 2 | `EntityBase.cs` | (Opcional) Agregar `CreadoPor` a nivel base |
| 3 | `KioskosFisicosController.cs:218-230` | Guardar `CreadoPor` al crear activos |
| 4 | `KioskosFisicosController.cs:433-452` | Cambiar DELETE+INSERT por UPSERT (actualizar existentes + agregar nuevos + eliminar ausentes) |
| 5 | `FrontendDtos.cs:214-223` | Agregar `UltimaModificacionPor`, `FechaModificacion` al DTO response |
| 6 | `AuditoriaActivoFijo.cs` (nuevo) | Entidad de auditoría (si se opta por Opción A) |

---

## 4. C13 / V2.6 / V7.5 — Campo KioskoMediaId huérfano sin FK

### Estado actual

**❌ Campo sin propósito real en el backend.** Es un campo fantasma que solo tiene sentido en el frontend.

### Análisis

#### Backend: Campo sin integridad referencial

**Entidad Kiosko:** `ApiTiketero/Ticketero/src/Ticketero.Domain/Entities/Kiosko.cs:15`

```csharp
public int? KioskoMediaId { get; set; }
```

**Configuration EF Core:** `EntityConfigurations.cs:215`

```csharp
builder.Property(k => k.KioskoMediaId);  // ← Sin .HasForeignKey(), sin .HasOne()
```

- **No hay FK** en la migración (`AddKioskoMediaId.cs:14`):
  ```csharp
  migrationBuilder.AddColumn<int>(name: "KioskoMediaId", schema: "dbo", table: "Kiosko", type: "int", nullable: true);
  ```
- **No hay navigation property** en `Kiosko.cs` (no existe `ConfiguracionMultimedia? KioskoMediaNavegacion`)
- **No hay relación** en `EntityConfigurations.cs`

**ConfiguracionMultimedia sí tiene FK a Kiosko** (la relación inversa):

```csharp
// ConfiguracionMultimedia.cs:7
public int KioskoId { get; set; }        // ← FK real a Kiosko
public Kiosko? Kiosko { get; set; }       // ← Navigation property
```

Pero `Kiosko.KioskoMediaId` no apunta a `ConfiguracionMultimedia` ni a ninguna otra tabla.

#### Uso en controlador (mapeo pasivo)

`KioskosFisicosController.cs:161` y `:288` — solo asigna el valor que llega del request, sin lógica de negocio:

```csharp
kiosko.KioskoMediaId = request.KioskoMediaId;  // solo almacena el int
```

`MappingService.cs:144` — lo pasa al response:

```csharp
KioskoMediaId = kiosko.KioskoMediaId,
```

#### Frontend: Lookup local (el uso real)

En el frontend, `KioskoMediaId` se usa como clave de lookup en una lista local de `KioskoMedia` almacenada en **SharedPreferences**:

**Flujo en Flutter:**

1. `SettingsProvider` mantiene `_kioskoLocations` (lista de `KioskoMedia`) persistida en SharedPreferences
2. `KioskoMediaId` se usa para buscar coincidencias visuales (logo, video) desde el caché local
3. `KioskoSelectionScreen:111-117` — `_findMedia()` busca en `sp.kioskoLocations` por `mediaId`
4. `kioskos_administracion_panel:122` — mismo lookup

**Los valores multimedia REALES** (`logoUrl`, `videoUrl`) vienen directamente del backend en `KioskoFisicoResponse`, no de `KioskoMediaId`. Por lo tanto, `KioskoMediaId` es un concepto puramente del frontend que no corresponde a ninguna entidad del backend.

### Solución propuesta

#### Opción A (Recomendada): Eliminar el campo del backend

1. **Backend:** Remover `KioskoMediaId` de:
   - `Kiosko.cs` (propiedad)
   - `EntityConfigurations.cs` (mapeo)
   - `FrontendDtos.cs` (CreateRequest, UpdateRequest, Response)
   - `MappingService.cs` (mapeo)
   - `KioskosFisicosController.cs` (asignación)
   - Migración: crear `RemoveKioskoMediaId` que dropee la columna

2. **Frontend:** Mantener solo como campo local en Flutter:
   - `KioskoFisico` entidad y modelo: mantener el campo pero documentar que es local
   - `SettingsProvider._selectedKioskoMediaId`: mantener en SharedPreferences
   - No enviarlo al backend en las peticiones

#### Opción B: Darle propósito real como FK a ConfiguracionMultimedia

1. Agregar navigation property en `Kiosko.cs`:
   ```csharp
   public ConfiguracionMultimedia? KioskoMediaNavegacion { get; set; }
   ```

2. Configurar FK en `EntityConfigurations.cs`:
   ```csharp
   builder.HasOne(k => k.KioskoMediaNavegacion)
       .WithMany()
       .HasForeignKey(k => k.KioskoMediaId)
       .OnDelete(DeleteBehavior.SetNull);
   ```

3. Cambiar lógica en controller para que `KioskoMediaId` apunte al registro `ConfiguracionMultimedia` principal del kiosko. Pero esto duplicaría la relación existente (`ConfiguracionMultimedia.KioskoId` ya es FK).

#### Opción B no recomendada porque:
- Ya existe `ConfiguracionMultimedia.KioskoId` como FK correcta
- Cada kiosko puede tener múltiples configuraciones multimedia (varios logos, varios videos)
- `KioskoMediaId` como FK a una sola fila sería inconsistente con el modelo 1:N existente

### Migración necesaria (Opción A)

```csharp
// Nueva migración: RemoveKioskoMediaId
migrationBuilder.DropColumn(
    name: "KioskoMediaId",
    schema: "dbo",
    table: "Kiosko");
```

### Cambios por archivo

| # | Archivo | Cambio |
|---|---------|--------|
| 1 | `Kiosko.cs:15` | Eliminar `public int? KioskoMediaId { get; set; }` |
| 2 | `EntityConfigurations.cs:215` | Eliminar `builder.Property(k => k.KioskoMediaId)` |
| 3 | `FrontendDtos.cs` | Eliminar `KioskoMediaId` de los 3 DTOs (Create, Update, Response) |
| 4 | `MappingService.cs:144` | Eliminar `KioskoMediaId = kiosko.KioskoMediaId` |
| 5 | `KioskosFisicosController.cs:161,288` | Eliminar asignaciones de `KioskoMediaId` |
| 6 | `KioskoFisicoModel.dart` | Mantener campo pero no enviar en API calls |
| 7 | Nueva migración | `RemoveKioskoMediaId` |

---

## Resumen de prioridad para implementación

| Gap | Prioridad | Esfuerzo | Dependencias | Riesgo |
|-----|-----------|----------|--------------|--------|
| **V1.1/V1.5** — TimeSync en caller + kiosko | **Alta** | Pequeño (2-3 archivos) | Ninguna | Bajo — cambios localizados en Flutter |
| **C10** — Auditoría activos fijos | **Media** | Medio (backend + BD) | Ninguna | Medio — requiere migración y cambio de lógica UPSERT |
| **C13** — Eliminar KioskoMediaId | **Baja** | Medio (backend + migración) | Ninguna | Medio — requiere migración y afecta DTOs |
| **C12/V7.7** | — | **Ya resuelto** | — | — |
