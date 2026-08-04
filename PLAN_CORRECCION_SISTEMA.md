# Plan de Corrección Integral - Sistema Ticketero

---

## 1. CHECKLIST DE VERIFICACIÓN

### 1.1 Sincronización de Hora y Generación de Tickets

| # | Verificación | ¿Cumple? | Evidencia / Nota |
|---|---|---|---|
| V1.1 | ¿Existe endpoint `GET /api/time` para sincronización? | ❌ No | No implementado |
| V1.2 | ¿El servidor usa `DateTime.UtcNow` consistentemente? | ✅ Sí | En todos los UseCases y EntityBase |
| V1.3 | ¿La BD usa `SYSDATETIME()` por defecto? | ✅ Sí | En Ticket_tabla.sql y otras |
| V1.4 | ¿El correlativo de ticket se resetea correctamente por día y área? | ✅ Sí | `CrearTicketUseCase` filtra por `FechaCreacion >= hoy` |
| V1.5 | ¿El kiosko Flutter sincroniza su hora con el servidor al inicio? | ❌ No | Usa `DateTime.now()` local sin ajuste |
| V1.6 | ¿Hay protección contra race conditions en la creación de tickets? | ❌ No | Dos kioskos simultáneos podrían generar mismo correlativo |
| V1.7 | ¿El formato `{PREFIJO}-{CORRELATIVO:D3}` es único por área+día? | ✅ Sí | Unique index en `NumeroTicket` |

### 1.2 Carga y Actualización de Multimedia

| # | Verificación | ¿Cumple? | Evidencia / Nota |
|---|---|---|---|
| V2.1 | ¿La subida de archivos valida tipo MIME? | ❌ No | `ConfiguracionMultimediaController.cs` no valida |
| V2.2 | ¿La subida valida tamaño máximo? | ❌ No | Sin límite configurado |
| V2.3 | ¿Los archivos se almacenan en SharePoint? | ❌ No | Se guardan en `wwwroot/uploads/multimedia/` local |
| V2.4 | ¿Los archivos persisten ante redeploy del servidor? | ❌ No | Volátil en sistema de archivos local |
| V2.5 | ¿Hay asociación correcta kiosko-multimedia via FK? | ✅ Sí | `ConfiguracionMultimedia.KioskoId` con FK |
| V2.6 | ¿El campo `KioskoMediaId` en Kiosko tiene uso real? | ❌ No | Campo huérfano, sin FK ni lógica |
| V2.7 | ¿Los `logoUrl`/`videoUrl` se sirven correctamente en el frontend? | ✅ Sí | Flutter los consume desde `KioskoFisicoResponse` |

### 1.3 Usuario Asignado a Múltiples Áreas

| # | Verificación | ¿Cumple? | Evidencia / Nota |
|---|---|---|---|
| V3.1 | ¿`UsuarioArea` (N:N) está correctamente modelada? | ✅ Sí | Tabla `UsuariosAreaFase` con unique(UsuarioId, AreaId) |
| V3.2 | ¿`AtenderTicketUseCase` valida que el usuario pertenezca al área? | ❌ No | No verifica contra `UsuarioArea` |
| V3.3 | ¿La sesión del operador (`SesionOperador`) permite múltiples áreas? | ✅ Sí | La sesión se asocia a un puesto, y el puesto a un área; pero el usuario puede cambiar de puesto |
| V3.4 | ¿El frontend filtra tickets por las áreas asignadas al usuario? | ✅ Sí | `auth.user?.areasAtencion` en Flutter |
| V3.5 | ¿El endpoint `POST /api/tickets/{id}/start-attention` verifica área? | ❌ No | Omite validación de pertenencia |
| V3.6 | ¿Se puede reabrir sesión en un área diferente? | ✅ Sí | `AbrirSesionUseCase` cierra sesión anterior y abre nueva |

### 1.4 Derivación de Áreas

| # | Verificación | ¿Cumple? | Evidencia / Nota |
|---|---|---|---|
| V4.1 | ¿Se valida que área destino sea diferente de la actual? | ✅ Sí | `DerivarTicketUseCase.cs:29` |
| V4.2 | ¿Se valida que área destino exista? | ✅ Sí | `DerivarTicketUseCase.cs:32` |
| V4.3 | ¿El `DerivadoDeNombre` en `TicketResponse` muestra el área ORIGEN? | ❌ No | **BUG:** Muestra `AreaDestino?.Descripcion` (destino) en lugar de `Area?.Descripcion` (origen) |
| V4.4 | ¿Se registra el tiempo de atención antes de derivar? | ✅ Sí | `atencion.TiempoAtencionSegundos = (int)(UtcNow - FechaInicio).TotalSeconds` |
| V4.5 | ¿Se guarda observación en la derivación? | ✅ Sí | `atencion.Observacion = request.Observacion` |
| V4.6 | ¿Hay límite de derivaciones por ticket? | ❌ No | Puede derivarse N veces sin control |
| V4.7 | ¿El ticket derivado aparece en la cola del área destino? | ✅ Sí | `Ticket.AreaActualId` y `EstadoTicketId = Asignado` se actualizan |
| V4.8 | ¿El operador que deriva está autorizado (pertenece al área)? | ❌ No | No se valida autorización |

### 1.5 Observaciones al Finalizar Ticket

| # | Verificación | ¿Cumple? | Evidencia / Nota |
|---|---|---|---|
| V5.1 | ¿La observación se guarda al cerrar ticket? | ✅ Sí | `CerrarTicketUseCase.cs:24` |
| V5.2 | ¿La observación se guarda al derivar? | ✅ Sí | `DerivarTicketUseCase.cs:41` |
| V5.3 | ¿La observación se guarda al cancelar? | ✅ Sí | `TicketsController.cs:189` |
| V5.4 | ¿El `TicketResponse` incluye la observación? | ✅ Sí | `MappingService.cs:94` |
| V5.5 | ¿Admin/Supervisor puede ver el historial COMPLETO de observaciones? | ❌ No | Solo ve la última atención (`ultimaAtencion?.Observacion`) |
| V5.6 | ¿Hay un endpoint `GET /api/tickets/{id}/history` con todas las atenciones? | ❌ No | No existe endpoint específico |
| V5.7 | ¿El frontend Flutter muestra observaciones históricas? | ❌ No | `AttentionLog` tiene campo `observacion` pero no hay vista histórica |
| V5.8 | ¿El Dashboard/PdfExport incluye observaciones? | ❌ No | `DashboardController` no expone observaciones |

### 1.6 Activos Fijos Asociados a Kiosko

| # | Verificación | ¿Cumple? | Evidencia / Nota |
|---|---|---|---|
| V6.1 | ¿Existe script SQL para `ActivosFijos` en `BD_Ticketero/`? | ❌ No | Solo existe via EF Migration |
| V6.2 | ¿`ActivoFijo.TipoActivo` usa un catálogo/enum? | ❌ No | Es string libre (Monitor, PC, Impresora...) |
| V6.3 | ¿`NumeroActivo` es único globalmente? | ❌ No | Unique index solo por `(KioskoId, NumeroActivo)` |
| V6.4 | ¿Los activos se incluyen en la respuesta del kiosko? | ✅ Sí | `KioskoFisicoResponse.ActivosFijos` |
| V6.5 | ¿Hay auditoría de cambios en activos fijos? | ❌ No | No hay log de modificaciones |
| V6.6 | ¿Se puede crear/editar/eliminar activos desde el frontend? | ❌ No | No hay UI para gestión de activos |

### 1.7 Relación Kiosko-Área e Identificación

| # | Verificación | ¿Cumple? | Evidencia / Nota |
|---|---|---|---|
| V7.1 | ¿`KioskoAreas` tabla puente está normalizada? | ✅ Sí | `(KioskoId, AreaId)` unique |
| V7.2 | ¿El kiosko se identifica por IP en auto-registro? | ✅ Sí | `POST /api/kioskos-fisicos/auto-registrar` |
| V7.3 | ¿`Kiosko.IpKiosko` e `IpEquipo` se actualizan dinámicamente? | ✅ Sí | Sí, en `auto-registrar` y `detectar-ip` |
| V7.4 | ¿Se puede deshabilitar un área en un kiosko sin borrar? | ✅ Sí | `KioskoArea.Estado` (EntityBase) |
| V7.5 | ¿`KioskoMediaId` tiene FK real y utilidad? | ❌ No | Campo huérfano sin propósito definido |
| V7.6 | ¿El kiosko huérfano (sin áreas) se detecta? | ✅ Sí | `KioskoFisicoResponse.EsHuerfano` |
| V7.7 | ¿Hay validación que exija al menos un área por kiosko? | ❌ No | Se puede crear kiosko sin áreas |

---

## 2. RUTA DE VERIFICACIÓN (Pruebas de Integración)

A continuación, la secuencia de pruebas para verificar el correcto funcionamiento del sistema, cubriendo todos los flujos críticos:

### RUTA 1: Ciclo de Vida Completo de un Ticket

```
1. CREAR TICKET (kiosko)
   POST /api/tickets (AllowAnonymous)
   → Verificar: NumeroTicket formato correcto, Estado=Nuevo, FechaCreacion=UTC

2. VERIFICAR COLA PENDIENTES
   GET /api/tickets/pending?areaId=X
   → Verificar: Ticket aparece en la cola, tiempo de espera en 0

3. LLAMAR TICKET (operador)
   POST /api/tickets/{id}/call
   → Verificar: Estado=Asignado, Marcacion creada, NumeroLlamado=1

4. INICIAR ATENCIÓN
   POST /api/tickets/{id}/start-attention
   → Verificar: Estado=EnAtencion, Atencion creada, FechaInicio=UTC
   → Validar CRÍTICO: usuario debe pertenecer al área del ticket (V3.2)

5. DERIVAR A OTRA ÁREA
   POST /api/tickets/{id}/derivar
   → Verificar: Atencion.FueDerivado=true, AreaDestinoId correcto
   → Verificar: Ticket.AreaActualId=AreaDestino, Estado=Asignado (V4.3)
   → Validar CRÍTICO: DerivadoDe debe ser ORIGEN, no destino

6. CERRAR TICKET
   POST /api/tickets/{id}/complete
   → Verificar: Estado=Cerrado, Atencion.FechaFin!=null
   → Verificar: TiempoAtencionSegundos calculado
   → Verificar: Observacion guardada en Atencion (V5.1)

7. VERIFICAR HISTORIAL
   GET /api/tickets/{id} (con includes)
   → Verificar: Atenciones[], Marcaciones[], Observacion visible
```

### RUTA 2: Gestión de Kiosko y Multimedia

```
1. CREAR KIOSKO CON ACTIVOS
   POST /api/kioskos-fisicos
   → Verificar: Kiosko creado con AreaIds, ActivosFijos, Impresora, Red

2. AUTO-REGISTRO POR IP
   POST /api/kioskos-fisicos/auto-registrar
   → Verificar: IP detectada, Kiosko creado o actualizado

3. SUBIR MULTIMEDIA
   POST /api/ConfiguracionMultimedia/upload
   → Verificar: Archivo guardado, RutaArchivo correcta
   → Validar: Tipo MIME y tamaño (V2.1, V2.2)

4. VER CONFIGURACIÓN COMPLETA
   GET /api/kioskos-fisicos/{id}
   → Verificar: ActivosFijos[], logoUrl, videoUrl, areaIds

5. SINCRONIZAR HORA
   GET /api/time (propuesto)
   → Verificar: ServerTime en UTC, offset calculable
```

### RUTA 3: Usuario Multíárea y Sesiones

```
1. CREAR USUARIO CON MÚLTIPLES ÁREAS
   POST /api/users
   → Verificar: AreasAtencion = [area1, area2, area3]

2. ABRIR SESIÓN EN ÁREA 1
   POST /api/sesiones/abrir (Puesto del Área 1)
   → Verificar: SesionOperador activa, FechaInicio=UTC

3. ATENDER TICKET DEL ÁREA 1
   POST /api/tickets/{id}/start-attention
   → Validar: Operador tiene UsuarioArea para área 1 (V3.2)

4. CERRAR SESIÓN Y ABRIR EN ÁREA 2
   POST /api/sesiones/cerrar
   POST /api/sesiones/abrir (Puesto del Área 2)
   → Verificar: Sesión anterior cerrada, nueva activa

5. ATENDER TICKET DEL ÁREA 2
   POST /api/tickets/{id}/start-attention
   → Validar: Operador tiene UsuarioArea para área 2

6. INTENTAR ATENDER ÁREA NO ASIGNADA
   → Validar: Debe rechazar con error de autorización
```

### RUTA 4: Dashboard y Reportes

```
1. VER RESUMEN
   GET /api/dashboard/summary?fechaInicio=X&fechaFin=Y
   → Verificar: TotalTickets, Atendidos, Pendientes, TiempoPromedio

2. VER ESTADÍSTICAS POR ÁREA
   GET /api/dashboard/by-area
   → Verificar: Datos agrupados por área

3. EXPORTAR PDF
   GET /api/dashboard/export-pdf
   → Verificar: PDF generado con datos correctos

4. VER ESTADO DE PUESTOS
   GET /api/dashboard/puestos-status
   → Verificar: Status por puesto (libre/atendiendo/inhabilitado)
```

### RUTA 5: Observaciones y Auditoría

```
1. CREAR TICKET → ATENDER → DERIVAR → ATENDER → CERRAR
   (Ciclo con derivación intermedia)

2. VERIFICAR HISTORIAL DE ATENCIONES
   GET /api/tickets/{id}/history (PROPUESTO)
   → Verificar: 2 atenciones, cada una con observación
   → Validar: Observación de derivación visible

3. CONSULTAR COMO ADMIN
   GET /api/tickets/{id}?
   → Verificar: Observacion = última observación (mejorable)
```

---

## 3. PLAN DE CORRECCIÓN

### PRIORIDAD: Crítica (P0) — Debe corregirse antes de producción

---

#### C1: Corregir `DerivadoDeNombre` en MappingService

| Campo | Detalle |
|---|---|
| **Problema** | `TicketResponse.DerivadoDeNombre` muestra el área **destino** en lugar del área **origen** (BUG de concepto) |
| **Archivo** | `ApiTiketero/Ticketero/src/Ticketero.Api/Mapping/MappingService.cs:92` |
| **Código actual** | `DerivadoDeNombre = ultimaAtencion?.AreaDestino?.Descripcion` |
| **Código correcto** | `DerivadoDeNombre = ultimaAtencion?.Area?.Descripcion` |
| **Objetivo** | Que el frontend muestre correctamente "Derivado de [Área Origen] → [Área Actual]" |
| **Validación** | Prueba Ruta 1 paso 5 de la ruta de verificación |
| **Consideraciones** | También revisar `DerivadoDe` (es `AreaDestinoId.ToString()` → debería ser `AreaId.ToString()`). Ajustar ambos. |

---

#### C2: Validar pertenencia del usuario al área en `AtenderTicketUseCase`

| Campo | Detalle |
|---|---|
| **Problema** | Un operador puede atender tickets de cualquier área sin restricción |
| **Archivo** | `ApiTiketero/Ticketero/src/Ticketero.Application/UseCases/AtenderTicketUseCase.cs` |
| **Objetivo** | Validar que `request.UsuarioId` tenga un registro `UsuarioArea` activo para `request.AreaId` |
| **Implementación** | ```
var usuarioArea = await _unitOfWork.UsuarioArea
    .FindAsync(ua => ua.UsuarioId == request.UsuarioId && ua.AreaId == request.AreaId && ua.Estado);
if (!usuarioArea.Any())
    throw new InvalidOperationException("Usuario no asignado al área seleccionada");
``` |
| **Validación** | Prueba Ruta 3 paso 6 |
| **Consideraciones** | Agregar `UsuarioArea` al `IUnitOfWork` (repositorio). También validar en `DerivarTicketUseCase` que el operador que deriva pertenezca al área actual del ticket. |

---

#### C3: Endpoint `GET /api/time` para sincronización horaria

| Campo | Detalle |
|---|---|
| **Problema** | El kiosko Flutter no tiene referencia de hora del servidor; puede haber desfase |
| **Archivo** | Nuevo controller o endpoint en `ConfiguracionesController.cs` |
| **Objetivo** | Proveer endpoint público para que el kiosko sincronice su reloj |
| **Implementación** | ```
[AllowAnonymous]
[HttpGet("api/time")]
public IActionResult GetServerTime()
{
    return Ok(new {
        serverTime = DateTime.UtcNow,
        serverTimeLocal = DateTime.Now,
        timezone = TimeZoneInfo.Local.Id,
        offsetUtc = TimeZoneInfo.Local.BaseUtcOffset.ToString()
    });
}
``` |
| **Validación** | Prueba Ruta 2 paso 5 |
| **Consideraciones** | En Flutter, al iniciar el kiosko, llamar a este endpoint y calcular el delta con `DateTime.now()` local para mostrar hora ajustada. |

---

### PRIORIDAD: Alta (P1) — Impacto significativo

---

#### C4: Validación de tipo MIME y tamaño en subida de multimedia

| Campo | Detalle |
|---|---|
| **Problema** | No hay validación de tipo de archivo ni tamaño máximo; riesgo de seguridad y corrupción |
| **Archivo** | `ConfiguracionMultimediaController.cs:57` método `CargarArchivo` |
| **Objetivo** | Validar extensiones permitidas (jpg, png, mp4, pdf) y tamaño máximo (ej: 20MB) |
| **Implementación** | ```
var allowedTypes = new[] { "image/jpeg", "image/png", "video/mp4", "application/pdf" };
if (!allowedTypes.Contains(archivo.ContentType))
    return BadRequest("Tipo de archivo no permitido");
if (archivo.Length > 20 * 1024 * 1024)
    return BadRequest("El archivo excede el tamaño máximo de 20MB");
``` |
| **Validación** | Prueba Ruta 2 paso 3 |
| **Consideraciones** | Las extensiones y tamaños deberían estar en `appsettings.json` para configuración sin redeploy. |

---

#### C5: Integración con SharePoint para almacenamiento multimedia

| Campo | Detalle |
|---|---|
| **Problema** | Los archivos multimedia se pierden al redeployar el servidor; no hay persistencia real |
| **Archivo** | `ConfiguracionMultimediaController.cs` (modificar) + nuevo servicio `SharePointService` |
| **Objetivo** | Migrar de almacenamiento local a SharePoint (o Azure Blob Storage como alternativa) |
| **Implementación** | Opción A (SharePoint): Usar `Microsoft.Graph` SDK para subir a un drive de SharePoint y guardar `webUrl` en `RutaArchivo`. Opción B (Azure Blob): Usar `Azure.Storage.Blobs` y guardar URL pública/sas en `RutaArchivo`. |
| **Validación** | Prueba Ruta 2 paso 3: verificar que `RutaArchivo` sea una URL absoluta (https://...) |
| **Consideraciones** | Costo de licencias SharePoint. Alternativa: configurar `appsettings.json` con `StorageMode: "Local|SharePoint|AzureBlob"` para elegir en deploy. Si se queda en local, agregar respaldo periódico. |

---

#### C6: Endpoint de historial completo de atenciones con observaciones

| Campo | Detalle |
|---|---|
| **Problema** | Solo se puede ver la última observación; administrador/supervisor no tiene visibilidad histórica |
| **Archivo** | Nuevo endpoint en `TicketsController.cs` + posible nuevo método en `IAtencionRepository` |
| **Objetivo** | Crear `GET /api/tickets/{id}/attention-history` que devuelva todas las atenciones con observaciones |
| **Implementación** | ```
[AllowAnonymous]
[HttpGet("{id}/attention-history")]
public async Task<IActionResult> GetAttentionHistory(int id)
{
    var atenciones = await _unitOfWork.Atenciones.FindAsync(a => a.TicketId == id);
    var result = atenciones.Select(a => new {
        a.Id, a.FechaInicio, a.FechaFin, a.TiempoAtencionSegundos,
        a.FueDerivado, a.Observacion,
        Usuario = a.Usuario != null ? $"{a.Usuario.Nombre} {a.Usuario.Apellido}" : null,
        Area = a.Area?.Descripcion,
        AreaDestino = a.AreaDestino?.Descripcion,
        Estado = a.EstadoTicket?.Descripcion
    }).OrderBy(a => a.FechaInicio);
    return Ok(result);
}
``` |
| **Validación** | Prueba Ruta 5 paso 2 |
| **Consideraciones** | En el frontend, agregar pestaña "Historial" en el detalle del ticket (visible para Admin/Supervisor). |

---

#### C7: Agregar columna `DerivadoDe` correcta en `TicketResponse`

| Campo | Detalle |
|---|---|
| **Problema** | `DerivadoDe` usa `AreaDestinoId.ToString()` en lugar del área origen |
| **Archivo** | `MappingService.cs:91` |
| **Código actual** | `DerivadoDe = ultimaAtencion?.FueDerivado == true ? ultimaAtencion?.AreaDestinoId?.ToString() : null` |
| **Código correcto** | `DerivadoDe = ultimaAtencion?.FueDerivado == true ? ultimaAtencion?.AreaId.ToString() : null` |
| **Objetivo** | Que el frontend reciba correctamente el ID del área de origen en derivaciones |
| **Validación** | Prueba Ruta 1 paso 5 |
| **Consideraciones** | Este bug cambia el significado semántico del campo. Verificar frontend para asegurar que no dependa del comportamiento actual. |

---

### PRIORIDAD: Media (P2) — Mejora significativa

---

#### C8: Protección contra race conditions en creación de tickets

| Problema | Dos kioskos pueden generar el mismo correlativo simultáneamente |
|---|---|
| **Objetivo** | Usar serialización o lock transaccional |
| **Implementación** | Agregar `serializable` en el `BeginTransactionAsync()` o usar una tabla de secuencia `Correlativos` con `UPDATE ... OUTPUT` atómico |
| **Consideraciones** | El impacto es bajo porque hay unique index en `NumeroTicket`, pero causaría error 500 al segundo kiosko |

---

#### C9: Catálogo de `TipoActivo` para activos fijos

| Problema | `TipoActivo` es string libre sin validación |
|---|---|
| **Objetivo** | Crear enum `TipoActivoEnum` (Monitor, PC, Impresora, Tablet, Lector, Otro) y validar en API |
| **Consideraciones** | No romper API actual: aceptar string pero normalizar. Agregar migración si se desea tabla catálogo. |

---

#### C10: Auditoría de cambios en activos fijos

| Problema | No hay trazabilidad de quién/modificó activos |
|---|---|
| **Objetivo** | Agregar columna `UltimaModificacionPor` (UsuarioId) y `FechaModificacion` en `ActivoFijo`, o tabla de auditoría separada |
| **Consideraciones** | `EntityBase` tiene `Logs` (int) que podría incrementarse, pero no guarda quién. Evaluar si vale la pena tabla `AuditoriaActivoFijo`. |

---

#### C11: Límite de derivaciones por ticket

| Problema | Un ticket puede derivarse infinitamente |
|---|---|
| **Objetivo** | Agregar constante `MaxDerivaciones = 5` y contar atenciones con `FueDerivado = true` para el ticket |
| **Consideraciones** | Podría haber casos de negocio que requieran más derivaciones; hacer configurable en tabla `Configuraciones`. |

---

#### C12: Validación de al menos un área por kiosko

| Problema | Se pueden crear kioskos sin áreas asignadas (huérfanos) |
|---|---|
| **Objetivo** | Validar en `KioskosFisicosController.Crear` que `request.AreaIds` tenga al menos un elemento |
| **Consideraciones** | Ya existe validación con `[MinLength(1)]` en `KioskoFisicoCreateRequest`, pero verificar que se ejecute. |

---

### PRIORIDAD: Baja (P3) — Mejora cosmética/futura

---

| # | Corrección | Objetivo |
|---|---|---|
| C13 | Limpiar campo `KioskoMediaId` | Eliminar o darle propósito real con FK |
| C14 | Dashboard: incluir observaciones en export PDF | Enriquecer reportes |
| C15 | Frontend: pantalla de gestión de activos fijos | CRUD de activos desde Flutter |
| C16 | Frontend: vista de historial de observaciones (Admin) | Dar visibilidad completa a supervisores |
| C17 | Pruebas unitarias para validaciones de área | Cubrir casos de borde en `AtenderTicketUseCase` y `DerivarTicketUseCase` |

---

## 4. RESUMEN DE CARGA POR PRIORIDAD

| Prioridad | Cantidad | Correcciones |
|---|---|---|
| **P0 - Crítica** | 3 | C1, C2, C3 |
| **P1 - Alta** | 4 | C4, C5, C6, C7 |
| **P2 - Media** | 4 | C8, C9, C10, C11, C12 |
| **P3 - Baja** | 5 | C13, C14, C15, C16, C17 |
| **Total** | **16** | |

---

## 5. DEPENDENCIAS ENTRE CORRECCIONES

```
C1 (DerivadoDe) ──→ C7 (DerivadoDe origen) [mismo archivo, hacer juntos]
C2 (Validar área) ──→ C11 (límite derivaciones) [flujo de atención completo]
C4 (Validar upload) ──→ C5 (SharePoint) [C4 es prerequisito de seguridad para C5]
C6 (Historial obs) ──→ C16 (Frontend historial) [backend primero, luego frontend]
C9 (Catálogo activos) ──→ C15 (CRUD activos) [catálogo primero para validar datos]
```

---

## 6. RECOMENDACIÓN DE SPRINT

### Sprint 1: Correcciones Críticas (P0)
- C1 + C7: Corregir `DerivadoDe` y `DerivadoDeNombre` en MappingService
- C2: Validar pertenencia del usuario al área en atención y derivación
- C3: Endpoint de sincronización horaria

### Sprint 2: Correcciones de Impacto (P1)
- C4: Validación MIME y tamaño en upload
- C5: Integración con SharePoint/Almacenamiento externo
- C6: Endpoint historial de atenciones

### Sprint 3: Mejoras (P2)
- C8, C9, C10, C11, C12

### Sprint 4: Frontend y Pulido (P3)
- C13, C14, C15, C16, C17

---

*Documento generado el 21/07/2026 basado en análisis de código fuente del sistema Ticketero.*
