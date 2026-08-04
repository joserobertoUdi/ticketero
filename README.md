<div align="center">
  <h1>🎫 Sistema Ticketero</h1>
  <p><strong>Sistema integral de gestión de turnos para atención al cliente</strong></p>
  <p>
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet" alt=".NET 8">
    <img src="https://img.shields.io/badge/Flutter-3.22-02569B?logo=flutter" alt="Flutter 3.22">
    <img src="https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver" alt="SQL Server 2022">
    <img src="https://img.shields.io/badge/Redis-7-red?logo=redis" alt="Redis 7">
    <img src="https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker" alt="Docker Compose">
  </p>
</div>

---

## 📋 Descripción

**Ticketero** es un sistema de gestión de turnos diseñado para optimizar la atención al cliente en organizaciones con múltiples áreas y servicios. Permite a los clientes tomar un ticket desde un kiosko físico o virtual, y a los operadores atenderlos en orden, con soporte en tiempo real mediante WebSockets.

---

## ✨ Características

- **Toma de turnos** desde kioskos físicos o pantallas táctiles
- **Atención en tiempo real** con actualizaciones vía SignalR (WebSockets)
- **Monitor de puestos** para visualizar el estado de atención
- **Pantalla de llamadores** para exhibir turnos en pantallas grandes
- **Panel de administración** con gestión de usuarios, áreas, servicios y configuraciones
- **Dashboard interactivo** con estadísticas y gráficos (fl_chart)
- **Configuración multimedia** (video/imagen) por área en kioskos
- **Configuración de impresoras** para tickets físicos
- **Autenticación JWT** con roles y permisos
- **Rate limiting** para protección contra abusos
- **Redis** como caché distribuida y backplane de SignalR
- **Docker Compose** para despliegue completo
- **Arquitectura limpia (Clean Architecture)** en backend y frontend

---

## 🚀 Tecnologías

### Backend — `ApiTiketero/Ticketero/`

| Tecnología | Propósito |
|---|---|
| **.NET 8** | Framework principal |
| **ASP.NET Core Web API** | API RESTful |
| **Entity Framework Core** | ORM para SQL Server |
| **SignalR** | Comunicación en tiempo real (WebSockets) |
| **JWT Bearer** | Autenticación y autorización |
| **Redis** | Caché distribuida y backplane de SignalR |
| **Serilog** | Logging estructurado |
| **QuestPDF** | Generación de reportes PDF |
| **Swagger / OpenAPI** | Documentación de la API |
| **Docker** | Contenedorización |
| **Health Checks** | Monitoreo de estado |

### Frontend — `SistemaTicket/sistema_ticketero/`

| Tecnología | Propósito |
|---|---|
| **Flutter** | Framework de UI multiplataforma |
| **Dart** | Lenguaje de programación |
| **Provider** | Manejo de estado |
| **Dio** | Cliente HTTP |
| **SignalR NetCore** | Comunicación en tiempo real |
| **fl_chart** | Gráficos del dashboard |
| **flutter_secure_storage** | Almacenamiento seguro de tokens |
| **window_manager** | Control de ventana en escritorio |
| **desktop_multi_window** | Múltiples ventanas |
| **video_player** | Reproducción multimedia en kioskos |

### Base de Datos — `BD_Ticketero/`

| Tecnología | Propósito |
|---|---|
| **SQL Server 2022** | Motor de base de datos |
| **T-SQL** | Scripts de esquema y datos |
| **Índices no agrupados** | Optimización de consultas |
| **Claves foráneas y únicas** | Integridad referencial |

---

## 🧱 Estructura del Proyecto

```
Proyectos/
├── ApiTiketero/                     # Backend (.NET 8)
│   └── Ticketero/
│       ├── src/
│       │   ├── Ticketero.Api/       # Capa de presentación (Controllers, Hubs)
│       │   ├── Ticketero.Application/ # Casos de uso y DTOs
│       │   ├── Ticketero.Domain/    # Entidades y Value Objects
│       │   └── Ticketero.Infrastructure/ # EF Core, Repositorios, Servicios
│       ├── tests/                   # Pruebas unitarias e integración
│       ├── docker-compose.yml       # Orquestación con SQL Server + Redis + API
│       └── Dockerfile               # Imagen de la API
│
├── SistemaTicket/                   # Frontend (Flutter)
│   └── sistema_ticketero/
│       ├── lib/
│       │   ├── core/                # Utilidades, tema, red
│       │   ├── data/                # Fuentes de datos (remote/local), modelos, repositorios
│       │   ├── domain/              # Entidades, repositorios abstractos, casos de uso
│       │   └── presentation/        # Providers, screens, widgets
│       ├── backend/                 # Backend embebido (opcional)
│       └── database/                # Scripts de BD adicionales
│
└── BD_Ticketero/                    # Base de datos (SQL Server)
    ├── Tablas: Areas, Tickets, Usuarios, Servicios, Kioskos, etc.
    ├── Relaciones y constraints
    └── Validaciones/
```

---

## 🗄️ Modelo de Datos

La base de datos `DBTicketero` está compuesta por **21 tablas** principales:

| Tabla | Descripción |
|---|---|
| `Tickets` | Tabla principal del sistema |
| `Areas` | Áreas de atención |
| `Usuarios` | Operadores y administradores |
| `Servicios` | Tipos de servicios ofrecidos |
| `Kioskos` | Puntos de toma de turno |
| `KioskoAreas` | Relación kiosko-área |
| `Puestos` | Puestos de atención |
| `EstadosTicket` | Catálogo de estados |
| `Prioridades` | Niveles de prioridad |
| `TiposTicket` | Categorías de tickets |
| `Atencion` | Bitácora de atenciones |
| `Marcacion` | Registro de marcaciones |
| `Configuraciones` | Configuración general del sistema |
| `ConfiguracionMultimedia` | Multimedia por área |
| `ConfiguracionImpresora` | Configuración de impresión |
| `ConfiguracionRed` | Configuración de red de kioskos |
| `Roles` | Roles de usuario |
| `Ubicaciones` | Ubicaciones físicas |
| `SesionOperador` | Sesiones activas |
| `ActivoFijo` | Activos fijos asociados |
| `UsuarioArea` | Asignación usuario-área |

---

## ⚙️ Cómo funciona

### Flujo principal

1. **Cliente** llega a un kiosko y selecciona un servicio/área
2. **Kiosko** genera un ticket con número único y lo imprime
3. **Ticket** aparece en la pantalla de llamadores y en el monitor de puestos
4. **Operador** llama al siguiente ticket desde su puesto de atención
5. **Cliente** es atendido y el ticket se marca como completado
6. **Dashboard** refleja las estadísticas en tiempo real

### Comunicación en tiempo real

Todos los eventos (nuevo ticket, llamado, atención, finalización) se transmiten instantáneamente mediante **SignalR** a todos los clientes conectados.

---

## 🐳 Despliegue con Docker

```bash
# Iniciar todos los servicios
docker-compose up -d

# Servicios:
# - SQL Server 2022 (puerto 1433)
# - Redis 7 (puerto 6379)
# - API Ticketero (puerto 5000)
```

---

## 🛠️ Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Flutter 3.22+](https://flutter.dev)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional)
- [SQL Server 2022](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (local o Docker)

---

## 📦 Instalación local

### Backend

```bash
cd ApiTiketero/Ticketero
dotnet restore
dotnet run --project src/Ticketero.Api
```

### Frontend

```bash
cd SistemaTicket/sistema_ticketero
flutter pub get
flutter run
```

### Base de Datos

Ejecuta los scripts SQL en `BD_Ticketero/` en orden alfabético sobre una instancia de SQL Server.

---

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un issue primero para discutir los cambios propuestos.

---

## 📄 Licencia

Este proyecto es de uso privado/educativo. Todos los derechos reservados.

---

<div align="center">
  <p>Hecho con ❤️ para mejorar la experiencia de atención al cliente</p>
</div>
