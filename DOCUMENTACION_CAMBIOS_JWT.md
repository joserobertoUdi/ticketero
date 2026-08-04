# Documentación de Cambios Realizados

## Resumen General

Se corrigió el flujo completo de control de puestos (apertura/cierre de sesión), se movió la clave JWT a variable de entorno, y se agregó cierre automático de sesión al cerrar la aplicación.

---

## 1. Backend — Flujo de Puestos y Sesiones

### 1.1 `StartAttentionRequest` — Eliminar default PuestoId
**Archivo:** `src/Ticketero.Application/DTOs/FrontendDtos.cs`
```diff
 public class StartAttentionRequest
 {
     public int UserId { get; set; }
-    public int PuestoId { get; set; } = 1;
+    public int PuestoId { get; set; }
 }
```
**Motivo:** El valor por defecto `1` provocaba que si el frontend no enviaba `puestoId`, siempre se usara "Caja 1", ignorando el puesto realmente seleccionado.

### 1.2 `TicketsController.StartAttention` — Validación reordenada
**Archivo:** `src/Ticketero.Api/Controllers/TicketsController.cs`

**Antes:** Validaba `PuestoId` primero (línea 158), luego buscaba sesión activa. Si el `PuestoId` era `0` (porque el frontend no lo envió), fallaba con "El puesto especificado no existe" incluso si el usuario ya tenía sesión activa.

**Después:** Primero busca sesión activa. Si existe, la reusa sin necesitar `PuestoId`. Si no existe, entonces valida `PuestoId > 0` y que el puesto exista en BD.

**Flujo resultante:**
1. ¿Hay sesión activa para el usuario? → Sí → la usa (ignora `PuestoId`)
2. ¿Hay sesión activa? → No → ¿`PuestoId > 0`? → Sí → valida que exista y crea sesión
3. No hay sesión y `PuestoId <= 0` → `"Debe seleccionar un puesto antes de iniciar atención"`

### 1.3 `AbrirSesionUseCase` — Auto-cierre de sesión previa + validaciones
**Archivo:** `src/Ticketero.Application/UseCases/AbrirSesionUseCase.cs`

**Antes:** Si el usuario ya tenía una sesión activa, lanzaba `InvalidOperationException("El usuario ya tiene una sesión activa")`.

**Después:**
- Valida `PuestoId > 0` → `"Debe seleccionar un puesto válido"`
- Valida que el puesto exista y esté activo → `"El puesto seleccionado no existe o no está disponible"`
- Si hay sesión activa previa:
  - Verifica que no tenga atenciones activas sin finalizar
  - Si tiene atenciones activas → rechaza con mensaje claro
  - Si no tiene → cierra la sesión anterior (`FechaFin = UtcNow`) y crea la nueva

### 1.4 `UsuariosController` — Transacción en endpoints de sesión
**Archivo:** `src/Ticketero.Api/Controllers/UsuariosController.cs`

Se agregó el helper `EjecutarConTransaccion` que envuelve `AbrirSesion` y `CerrarSesion` en transacciones de BD para atomicidad. Si falla, hace rollback automático.

---

## 2. Backend — JWT a Variable de Entorno

### 2.1 `appsettings.Development.json` — Eliminar clave hardcodeada
```diff
  "Jwt": {
-    "Key": "TicketeroDevKey2026!@#$%^&*()MinLength32",
     "Issuer": "TicketeroApiDev",
     "Audience": "TicketeroClientDev",
     "ExpiryHours": 8
  },
```

### 2.2 `Program.cs` — Validación de Jwt:Key al startup
**Archivo:** `src/Ticketero.Api/Program.cs`

```csharp
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key no está configurada. " +
        "Configure la variable de entorno Jwt__Key con una clave de al menos 32 caracteres.");
}
```

**Comportamiento:**
- Sin `Jwt__Key`: la app no arranca, muestra error claro con instrucciones
- Con `Jwt__Key` válida: arranque normal, la clave se usa para firmar/validar tokens

---

## 3. Frontend — Flujo de Puestos y Sesiones

### 3.1 `api_constants.dart` — Endpoints de sesión
Se agregaron:
```dart
static const String sessionBase = '$apiPrefix/usuarios';
static String openSession(int usuarioId) => '$sessionBase/$usuarioId/sesion';
static String closeSession(int sesionOperadorId) => '$sessionBase/sesion/$sesionOperadorId/cerrar';
```

### 3.2 `auth_remote_datasource.dart` — Llamadas API
- `openSession(usuarioId, puestoId)`: `POST /api/usuarios/{id}/sesion`
- `closeSession(sesionOperadorId)`: `POST /api/usuarios/sesion/{sesionId}/cerrar`

### 3.3 `auth_repository.dart` (dominio) — Nuevos métodos en interfaz
```dart
Future<Either<Failure, int>> openSession(int usuarioId, int puestoId);
Future<Either<Failure, Unit>> closeSession(int sesionOperadorId);
```

### 3.4 `auth_repository_impl.dart` — Implementación
`openSession` retorna `sesionOperadorId` (int). `closeSession` retorna `Unit`.

### 3.5 `auth_provider.dart` — Gestión de sesión
Nuevos campos:
- `_sesionOperadorId` — ID de la sesión activa en backend
- `_fechaIngreso` — momento en que se abrió la sesión

Nuevos métodos:
- `openSession(usuarioId, puestoId)` → llama API, almacena `_sesionOperadorId`
- `closeSession()` → llama API de cierre, limpia estado local

`logout()` actualizado: ahora cierra la sesión en backend ANTES de limpiar estado local.

Getters expuestos: `puestoId`, `sesionOperadorId`, `fechaIngreso`.

### 3.6 `puesto_selection_screen.dart` — Confirmar puesto
**Antes:** `_confirm()` solo guardaba el puesto en memoria local y navegaba a `/attention`.

**Después:** 
- Llama `auth.openSession(userId, puestoId)` para crear la sesión en backend
- Muestra SnackBar si hay error (no bloquea la navegación)
- Se eliminó el botón "Continuar sin puesto" → si no hay puestos, solo permite "Cerrar sesión"

### 3.7 `attention_screen.dart` — Pasar puestoId real
**Antes:** `startAttention(ticketId, userId)` sin `puestoId` → se usaba `puestoId=1` por defecto.

**Después:** `startAttention(ticketId, userId, puestoId: auth.puestoId ?? 1)` → envía el puesto realmente seleccionado.

### 3.8 `ticket_provider.dart` — Aceptar puestoId
Se agregó parámetro `puestoId` en `startAttention()` y se pasa al repositorio.

### 3.9 `main.dart` — Cierre automático al cerrar ventana
Se agregó `WindowCloseHandler` con callback a `authProvider.logout()`:
```dart
windowManager.addListener(WindowCloseHandler(
  onClose: () => authProvider.logout(),
));
```
Cuando el usuario cierra la ventana (click en X, Alt+F4), se ejecuta `logout()` que llama `closeSession()` en backend antes de destruir la ventana.

---

## 4. Base de Datos

No se modificó el esquema (`SesionOperador`, `Puestos`, `Usuarios`, `Atencion`). Todos los cambios son solo de código.

### Tablas validadas
| Tabla | Columnas clave | Normalización |
|-------|----------------|--------------|
| `SesionOperador` | `SesionOperadorId` (PK), `UsuarioId` (FK→Usuarios), `PuestoId` (FK→Puestos), `FechaInicio`, `FechaFin`, `Estado`, `FechaReg`, `Ride` | 3FN |
| `Puestos` | `PuestoId` (PK), `AreaId` (FK→Areas), `Descripcion`, `Estado` | 3FN |
| `Usuarios` | `UsuarioId` (PK), FK a Roles | 3FN |
| `Atencion` | FK a `SesionOperador.SesionOperadorId` (nullable) | 3FN |

### Datos registrados
- `SesionOperador.FechaInicio` → hora de ingreso (cuando el usuario confirma el puesto)
- `Atencion.FechaInicio` → hora de primera atención (primer `start-attention` de la sesión)
- `SesionOperador.FechaFin` → hora de salida (logout o cierre de app)

---

## 5. Pruebas Realizadas

| Escenario | Resultado |
|-----------|-----------|
| Login con credenciales correctas | JWT emitido |
| Crear sesión con puesto válido | Sesión creada, `FechaInicio` set |
| Crear sesión con puesto ya ocupado por el mismo usuario | Auto-cierra la anterior, crea nueva |
| Crear sesión con puesto ocupado por otro usuario con atenciones activas | Rechaza con mensaje claro |
| Crear sesión con `PuestoId=0` | `"Debe seleccionar un puesto válido"` |
| Crear sesión con `PuestoId=999` (inexistente) | `"El puesto seleccionado no existe o no está disponible"` |
| Cerrar sesión | `FechaFin` seteado |
| start-attention sin sesión y sin `PuestoId` | `"Debe seleccionar un puesto antes de iniciar atención"` |
| start-attention con sesión activa, sin `PuestoId` | Reusa la sesión existente |
| start-attention con sesión activa y `PuestoId` | Reusa la sesión existente (ignora `PuestoId`) |
| JWT sin `Jwt__Key` en env vars | App no arranca, error descriptivo |
| JWT con `Jwt__Key` en env vars | Login y todos los endpoints OK |
