-- =====================================================
-- 02_SeedData.sql
-- Datos maestros y transaccionales estandarizados.
-- Flujos normalizados: Nuevo(1)->Llamado(8)->En Proceso(3)
--   ->Cerrado(6) | Derivado(2)->(ciclo en nueva área)
--   ->Cancelado(7)
-- =====================================================
SET QUOTED_IDENTIFIER ON;
GO

USE [DBTicketero_Dev];
GO

-- =====================================================
-- 1. CATÁLOGOS BASE (solo si están vacíos)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles])
BEGIN
    SET IDENTITY_INSERT [dbo].[Roles] ON;
    INSERT INTO [dbo].[Roles] ([RolId], [Descripcion], [Estado], [FechaReg], [Ride], [Logs])
    VALUES
        (1, 'Administrador', 1, SYSDATETIME(), NEWID(), 0),
        (2, 'Supervisor',    1, SYSDATETIME(), NEWID(), 0),
        (3, 'Operador',      1, SYSDATETIME(), NEWID(), 0),
        (4, 'Llamador',      1, SYSDATETIME(), NEWID(), 0);
    SET IDENTITY_INSERT [dbo].[Roles] OFF;
    PRINT 'Roles insertados.';
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadosTicket])
BEGIN
    SET IDENTITY_INSERT [dbo].[EstadosTicket] ON;
    INSERT INTO [dbo].[EstadosTicket] ([EstadoTicketId], [Descripcion], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 'Nuevo',       1, SYSDATETIME(), NEWID()),
        (2, 'Asignado',    1, SYSDATETIME(), NEWID()),  -- Derivado a nueva área
        (3, 'En Proceso',  1, SYSDATETIME(), NEWID()),
        (4, 'En Espera',   1, SYSDATETIME(), NEWID()),
        (5, 'Resuelto',    1, SYSDATETIME(), NEWID()),
        (6, 'Cerrado',     1, SYSDATETIME(), NEWID()),
        (7, 'Cancelado',   1, SYSDATETIME(), NEWID()),
        (8, 'Llamado',     1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[EstadosTicket] OFF;
    PRINT 'EstadosTicket insertados.';
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Prioridades])
BEGIN
    SET IDENTITY_INSERT [dbo].[Prioridades] ON;
    INSERT INTO [dbo].[Prioridades] ([PrioridadId], [Descripcion], [Nivel], [Color], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 'Critica', 1, 'Rojo',    1, SYSDATETIME(), NEWID()),
        (2, 'Alta',    2, 'Naranja', 1, SYSDATETIME(), NEWID()),
        (3, 'Media',   3, 'Amarillo',1, SYSDATETIME(), NEWID()),
        (4, 'Baja',    4, 'Verde',   1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[Prioridades] OFF;
    PRINT 'Prioridades insertadas.';
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[TiposTicket])
BEGIN
    SET IDENTITY_INSERT [dbo].[TiposTicket] ON;
    INSERT INTO [dbo].[TiposTicket] ([TipoTicketId], [Descripcion], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 'Incidente', 1, SYSDATETIME(), NEWID()),
        (2, 'Solicitud', 1, SYSDATETIME(), NEWID()),
        (3, 'Problema',  1, SYSDATETIME(), NEWID()),
        (4, 'Cambio',    1, SYSDATETIME(), NEWID()),
        (5, 'Consulta',  1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[TiposTicket] OFF;
    PRINT 'TiposTicket insertados.';
END
GO

-- =====================================================
-- 2. ÁREAS Y SERVICIOS
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[AreasFase])
BEGIN
    SET IDENTITY_INSERT [dbo].[AreasFase] ON;
    INSERT INTO [dbo].[AreasFase] ([AreaId], [Descripcion], [Prefijo], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 'Caja',                  'CAJ', 1, SYSDATETIME(), NEWID()),
        (2, 'Informes',              'INF', 1, SYSDATETIME(), NEWID()),
        (3, 'Atención al Cliente',   'ATE', 1, SYSDATETIME(), NEWID()),
        (4, 'Servicios Generales',   'SGE', 1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[AreasFase] OFF;
    PRINT 'Áreas insertadas.';
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Servicios])
BEGIN
    SET IDENTITY_INSERT [dbo].[Servicios] ON;
    INSERT INTO [dbo].[Servicios] ([ServicioId], [Descripcion], [AreaId], [Estado], [FechaReg], [Ride])
    VALUES
        -- Caja (AreaId=1)
        (1, 'PAGO',         1, 1, SYSDATETIME(), NEWID()),
        (2, 'INFORMACION',  1, 1, SYSDATETIME(), NEWID()),
        (3, 'CONSULTA',     1, 1, SYSDATETIME(), NEWID()),
        (4, 'RETIRO',       1, 1, SYSDATETIME(), NEWID()),
        -- Informes (AreaId=2)
        (5, 'REPORTES',     2, 1, SYSDATETIME(), NEWID()),
        (6, 'CERTIFICADOS', 2, 1, SYSDATETIME(), NEWID()),
        -- Atención al Cliente (AreaId=3)
        (7, 'RECLAMOS',     3, 1, SYSDATETIME(), NEWID()),
        (8, 'SUGERENCIAS',  3, 1, SYSDATETIME(), NEWID()),
        -- Servicios Generales (AreaId=4)
        (9, 'MANTENIMIENTO',4, 1, SYSDATETIME(), NEWID()),
        (10, 'LIMPIEZA',    4, 1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[Servicios] OFF;
    PRINT 'Servicios insertados.';
END
GO

-- =====================================================
-- 3. UBICACIONES
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Ubicaciones])
BEGIN
    SET IDENTITY_INSERT [dbo].[Ubicaciones] ON;
    INSERT INTO [dbo].[Ubicaciones] ([UbicacionId], [Descripcion], [Edificio], [Piso], [Sector], [Referencia], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 'Caja General',    'Principal',   'Planta Baja', 'Caja',    NULL, 1, SYSDATETIME(), NEWID()),
        (2, 'Módulo Informes', 'Principal',   'Planta Baja', 'Informes',NULL, 1, SYSDATETIME(), NEWID()),
        (3, 'Oficinas ATE',    'Principal',   '1er Piso',    'Atención al Cliente', 'Al lado de escaleras', 1, SYSDATETIME(), NEWID()),
        (4, 'Entrada Principal','Principal',  'Planta Baja', 'Acceso',  'Entrada principal del edificio', 1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[Ubicaciones] OFF;
    PRINT 'Ubicaciones insertadas.';
END
GO

-- =====================================================
-- 4. KIOSKOS Y CONFIGURACIÓN
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Kiosko])
BEGIN
    SET IDENTITY_INSERT [dbo].[Kiosko] ON;
    INSERT INTO [dbo].[Kiosko] ([KioskoId], [Descripcion], [Ubicacion], [UbicacionId], [Estado], [FechaReg], [Ride], [IpKiosko], [IpEquipo], [MascaraRedEquipo], [GatewayEquipo], [DnsEquipo])
    VALUES
        (1, 'Kiosko Principal',  'Entrada Principal',  4, 1, SYSDATETIME(), NEWID(), '192.168.1.100', '192.168.1.50',  '255.255.255.0', '192.168.1.1',   '8.8.8.8'),
        (2, 'Kiosko Secundario', 'Pasillo Central',    1, 1, SYSDATETIME(), NEWID(), '192.168.1.101', '192.168.1.51',  '255.255.255.0', '192.168.1.1',   '8.8.8.8'),
        (3, 'Kiosko Caja',       'Área de Caja',       1, 1, SYSDATETIME(), NEWID(), '192.168.1.102', '192.168.1.52',  '255.255.255.0', '192.168.1.1',   '8.8.8.8');
    SET IDENTITY_INSERT [dbo].[Kiosko] OFF;
    PRINT 'Kioskos insertados.';
END
GO

-- KioskoAreas: qué áreas atiende cada kiosko
IF NOT EXISTS (SELECT 1 FROM [dbo].[KioskoAreas])
BEGIN
    INSERT INTO [dbo].[KioskoAreas] ([KioskoId], [AreaId], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 1, 1, SYSDATETIME(), NEWID()),  -- Kiosko Principal → Caja
        (1, 2, 1, SYSDATETIME(), NEWID()),  -- Kiosko Principal → Informes
        (2, 3, 1, SYSDATETIME(), NEWID()),  -- Kiosko Secundario → ATE
        (3, 1, 1, SYSDATETIME(), NEWID());  -- Kiosko Caja → Caja
    PRINT 'KioskoAreas insertadas.';
END
GO

-- Configuración de impresoras
IF NOT EXISTS (SELECT 1 FROM [dbo].[ConfiguracionImpresora])
BEGIN
    SET IDENTITY_INSERT [dbo].[ConfiguracionImpresora] ON;
    INSERT INTO [dbo].[ConfiguracionImpresora] ([ConfiguracionImpresoraId], [KioskoId], [NombreImpresora], [Puerto], [DireccionIP], [TipoConexion], [AnchoPapelMM], [Copias], [ImpresionAutomatica], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 1, 'Epson TM-T20',    'USB001', '192.168.1.200', 'USB',  80, 1, 1, 1, SYSDATETIME(), NEWID()),
        (2, 2, 'Epson TM-T88VI',  'USB002', '192.168.1.201', 'USB',  80, 1, 1, 1, SYSDATETIME(), NEWID()),
        (3, 3, 'Bematech MP-4200','USB003', '192.168.1.202', 'USB',  80, 1, 1, 1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[ConfiguracionImpresora] OFF;
    PRINT 'Impresoras insertadas.';
END
GO

-- Configuración de multimedia (video)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ConfiguracionMultimedia])
BEGIN
    SET IDENTITY_INSERT [dbo].[ConfiguracionMultimedia] ON;
    INSERT INTO [dbo].[ConfiguracionMultimedia] ([ConfiguracionMultimediaId], [KioskoId], [NombreContenido], [TipoContenido], [RutaArchivo], [DuracionSegundos], [Orden], [Repetir], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 1, 'Bienvenida.mp4',    'video', 'C:\Media\Bienvenida.mp4',    30, 1, 1, 1, SYSDATETIME(), NEWID()),
        (2, 1, 'Promociones.mp4',   'video', 'C:\Media\Promociones.mp4',   45, 2, 1, 1, SYSDATETIME(), NEWID()),
        (3, 2, 'AtencionCliente.mp4','video','C:\Media\AtencionCliente.mp4',60, 1, 1, 1, SYSDATETIME(), NEWID()),
        (4, 3, 'CajaRapida.mp4',    'video', 'C:\Media\CajaRapida.mp4',    20, 1, 1, 1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[ConfiguracionMultimedia] OFF;
    PRINT 'Multimedia insertada.';
END
GO

-- Configuración de red por kiosko
IF NOT EXISTS (SELECT 1 FROM [dbo].[ConfiguracionRed])
BEGIN
    SET IDENTITY_INSERT [dbo].[ConfiguracionRed] ON;
    INSERT INTO [dbo].[ConfiguracionRed] ([ConfiguracionRedId], [KioskoId], [TipoConexion], [DHCP], [DireccionIP], [MascaraSubred], [Gateway], [DNSPrimario], [DNSSecundario], [Puerto], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 1, 'Ethernet', 0, '192.168.1.50',  '255.255.255.0', '192.168.1.1', '8.8.8.8', '8.8.4.4', 8080, 1, SYSDATETIME(), NEWID()),
        (2, 2, 'Ethernet', 0, '192.168.1.51',  '255.255.255.0', '192.168.1.1', '8.8.8.8', '8.8.4.4', 8080, 1, SYSDATETIME(), NEWID()),
        (3, 3, 'WiFi',     1, '192.168.1.52',  '255.255.255.0', '192.168.1.1', '8.8.8.8', '8.8.4.4', 8080, 1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[ConfiguracionRed] OFF;
    PRINT 'Configuración de red insertada.';
END
GO

-- Activos fijos por kiosko
IF NOT EXISTS (SELECT 1 FROM [dbo].[ActivosFijos])
BEGIN
    SET IDENTITY_INSERT [dbo].[ActivosFijos] ON;
    INSERT INTO [dbo].[ActivosFijos] ([ActivoFijoId], [KioskoId], [TipoActivo], [NumeroActivo], [Descripcion], [Marca], [Modelo], [Serie], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 1, 'Monitor',    'ACT-001', 'Monitor táctil 22"',  'Dell',  'P2214T',  'SN-DELL-001', 1, SYSDATETIME(), NEWID()),
        (2, 1, 'PC',         'ACT-002', 'Mini PC Kiosko',      'HP',    'EliteDesk', 'SN-HP-001', 1, SYSDATETIME(), NEWID()),
        (3, 1, 'Impresora',  'ACT-003', 'Impresora tickets',   'Epson', 'TM-T20',   'SN-EPS-001', 1, SYSDATETIME(), NEWID()),
        (4, 2, 'Monitor',    'ACT-004', 'Monitor táctil 22"',  'Lenovo','ThinkVision','SN-LEN-001',1, SYSDATETIME(), NEWID()),
        (5, 2, 'PC',         'ACT-005', 'Mini PC Kiosko',      'HP',    'EliteDesk', 'SN-HP-002', 1, SYSDATETIME(), NEWID()),
        (6, 3, 'Monitor',    'ACT-006', 'Monitor 19"',         'Samsung','S19F350', 'SN-SAM-001', 1, SYSDATETIME(), NEWID()),
        (7, 3, 'PC',         'ACT-007', 'PC Escritorio',       'HP',    'ProDesk', 'SN-HP-003', 1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[ActivosFijos] OFF;
    PRINT 'Activos fijos insertados.';
END
GO

-- =====================================================
-- 5. PUESTOS (por área)
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Puestos])
BEGIN
    SET IDENTITY_INSERT [dbo].[Puestos] ON;
    INSERT INTO [dbo].[Puestos] ([PuestoId], [AreaId], [Descripcion], [Estado], [FechaReg], [Ride])
    VALUES
        (1, 1, 'Caja 1',        1, SYSDATETIME(), NEWID()),
        (2, 1, 'Caja 2',        1, SYSDATETIME(), NEWID()),
        (3, 1, 'Caja 3',        1, SYSDATETIME(), NEWID()),
        (4, 2, 'Informes 1',    1, SYSDATETIME(), NEWID()),
        (5, 2, 'Informes 2',    1, SYSDATETIME(), NEWID()),
        (6, 3, 'ATE 1',         1, SYSDATETIME(), NEWID()),
        (7, 3, 'ATE 2',         1, SYSDATETIME(), NEWID()),
        (8, 4, 'Servicios Gral',1, SYSDATETIME(), NEWID());
    SET IDENTITY_INSERT [dbo].[Puestos] OFF;
    PRINT 'Puestos insertados.';
END
GO

-- =====================================================
-- 6. USUARIOS
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Usuarios])
BEGIN
    SET IDENTITY_INSERT [dbo].[Usuarios] ON;
    INSERT INTO [dbo].[Usuarios] ([UsuarioId], [NombreUsuario], [Nombre], [Apellido], [Correo], [PasswordHash], [CodigoSistema], [RolId], [Estado], [FechaReg], [Ride], [Logs], [FechaUltimoAcceso])
    VALUES
        (1, 'admin',      'Administrador', 'Sistema',  'admin@empresa.com',     '$2a$11$WXlqreMW/E0R74IV8b54DOZKsakYnl.eOvm64nhkbSosQFKwIXdt2', 'ADM001', 1, 1, SYSDATETIME(), NEWID(), 0, NULL),
        (2, 'jperez',     'Juan',          'Perez',    'jperez@empresa.com',    '$2a$11$WXlqreMW/E0R74IV8b54DOZKsakYnl.eOvm64nhkbSosQFKwIXdt2', 'SPV001', 2, 1, SYSDATETIME(), NEWID(), 0, NULL),
        (3, 'mgarcia',    'Maria',         'Garcia',   'mgarcia@empresa.com',   '$2a$11$WXlqreMW/E0R74IV8b54DOZKsakYnl.eOvm64nhkbSosQFKwIXdt2', 'OPE001', 3, 1, SYSDATETIME(), NEWID(), 0, NULL),
        (4, 'rtorres',    'Roberto',       'Torres',   'rtorres@empresa.com',   '$2a$11$WXlqreMW/E0R74IV8b54DOZKsakYnl.eOvm64nhkbSosQFKwIXdt2', 'LLA001', 4, 1, SYSDATETIME(), NEWID(), 0, NULL),
        (5, 'lgonzalez',  'Laura',         'Gonzalez', 'lgonzalez@empresa.com', '$2a$11$WXlqreMW/E0R74IV8b54DOZKsakYnl.eOvm64nhkbSosQFKwIXdt2', 'OPE002', 3, 1, SYSDATETIME(), NEWID(), 0, NULL),
        (6, 'arodriguez', 'Ana',           'Rodriguez','arodriguez@empresa.com','$2a$11$WXlqreMW/E0R74IV8b54DOZKsakYnl.eOvm64nhkbSosQFKwIXdt2', 'OPE003', 3, 1, SYSDATETIME(), NEWID(), 0, NULL),
        (7, 'mvelez',     'Mario',         'Velez',    'mvelez@empresa.com',    '$2a$11$WXlqreMW/E0R74IV8b54DOZKsakYnl.eOvm64nhkbSosQFKwIXdt2', 'SPV002', 2, 1, SYSDATETIME(), NEWID(), 0, NULL),
        (8, 'smartinez',  'Sofia',         'Martinez', 'smartinez@empresa.com', '$2a$11$WXlqreMW/E0R74IV8b54DOZKsakYnl.eOvm64nhkbSosQFKwIXdt2', 'LLA002', 4, 1, SYSDATETIME(), NEWID(), 0, NULL);
    SET IDENTITY_INSERT [dbo].[Usuarios] OFF;
    PRINT 'Usuarios insertados.';
END
GO

-- UsuarioArea: asignación de operadores a áreas
IF NOT EXISTS (SELECT 1 FROM [dbo].[UsuariosAreaFase])
BEGIN
    INSERT INTO [dbo].[UsuariosAreaFase] ([UsuarioId], [AreaId], [Estado], [FechaReg], [Ride])
    VALUES
        (2, 1, 1, SYSDATETIME(), NEWID()),  -- jperez (Supervisor) → Caja
        (2, 2, 1, SYSDATETIME(), NEWID()),  -- jperez (Supervisor) → Informes
        (3, 1, 1, SYSDATETIME(), NEWID()),  -- mgarcia (Operador) → Caja
        (4, 1, 1, SYSDATETIME(), NEWID()),  -- rtorres (Llamador) → Caja
        (4, 2, 1, SYSDATETIME(), NEWID()),  -- rtorres (Llamador) → Informes
        (5, 2, 1, SYSDATETIME(), NEWID()),  -- lgonzalez (Operador) → Informes
        (6, 3, 1, SYSDATETIME(), NEWID()),  -- arodriguez (Operador) → ATE
        (7, 3, 1, SYSDATETIME(), NEWID()),  -- mvelez (Supervisor) → ATE
        (8, 3, 1, SYSDATETIME(), NEWID()),  -- smartinez (Llamador) → ATE
        (8, 4, 1, SYSDATETIME(), NEWID());  -- smartinez (Llamador) → Servicios Generales
    PRINT 'UsuariosAreaFase insertadas.';
END
GO

-- =====================================================
-- 7. DATOS TRANSACCIONALES
-- Día actual: 2026-07-09
-- =====================================================
DECLARE @Hoy DATE = '2026-07-09';
DECLARE @Ayer DATE = '2026-07-08';
-- Función inline para combinar fecha + hora (SQL Server DATETIME addition trick)
DECLARE @HoyDT DATETIME = CAST(@Hoy AS DATETIME);
DECLARE @AyerDT DATETIME = CAST(@Ayer AS DATETIME);

-- =====================================================
-- 7a. SESIONES DE OPERADOR
-- =====================================================
INSERT INTO [dbo].[SesionOperador] ([UsuarioId], [PuestoId], [FechaInicio], [FechaFin], [Estado], [FechaReg], [Ride])
VALUES
    -- Maria Garcia (mgarcia, UsuarioId=3) - Sesión de hoy
    (3, 1, CAST(@HoyDT + CAST('08:00:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('09:30:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    -- Laura Gonzalez (lgonzalez, UsuarioId=5) - Sesión de hoy (aún abierta)
    (5, 4, CAST(@HoyDT + CAST('08:30:00' AS DATETIME) AS DATETIME2), NULL,             1, SYSDATETIME(), NEWID()),
    -- Ana Rodriguez (arodriguez, UsuarioId=6) - Sesión de hoy (aún abierta)
    (6, 6, CAST(@HoyDT + CAST('09:00:00' AS DATETIME) AS DATETIME2), NULL,             1, SYSDATETIME(), NEWID()),
    -- Maria Garcia (mgarcia) - Sesión de ayer
    (3, 1, CAST(@AyerDT + CAST('07:30:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('12:00:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    -- Laura Gonzalez (lgonzalez) - Sesión de ayer
    (5, 4, CAST(@AyerDT + CAST('08:00:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('11:30:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID());
PRINT 'SesionesOperador insertadas.';

-- =====================================================
-- 7b. TICKETS
-- =====================================================
-- Obtener IDs de sesiones (dependen del orden de inserción)
DECLARE @SesionMariaHoy INT, @SesionMariaAyer INT, @SesionLauraHoy INT, @SesionLauraAyer INT, @SesionAnaHoy INT;
SELECT @SesionMariaHoy = SesionOperadorId FROM [dbo].[SesionOperador] WHERE UsuarioId=3 AND PuestoId=1 AND FechaFin IS NOT NULL AND CAST(FechaInicio AS DATE)=@Hoy;
SELECT @SesionMariaAyer = SesionOperadorId FROM [dbo].[SesionOperador] WHERE UsuarioId=3 AND PuestoId=1 AND CAST(FechaInicio AS DATE)=@Ayer;
SELECT @SesionLauraHoy = SesionOperadorId FROM [dbo].[SesionOperador] WHERE UsuarioId=5 AND PuestoId=4 AND FechaFin IS NULL;
SELECT @SesionLauraAyer = SesionOperadorId FROM [dbo].[SesionOperador] WHERE UsuarioId=5 AND PuestoId=4 AND CAST(FechaInicio AS DATE)=@Ayer;
SELECT @SesionAnaHoy = SesionOperadorId FROM [dbo].[SesionOperador] WHERE UsuarioId=6 AND PuestoId=6 AND FechaFin IS NULL;

PRINT 'IDs de sesiones obtenidos.';

-- Tickets de HOY
INSERT INTO [dbo].[Tickets] ([NumeroTicket], [ServicioId], [TipoTicketId], [PrioridadId], [EstadoTicketId], [AreaActualId], [Descripcion], [FechaCreacion], [FechaCierre], [Estado], [FechaReg], [Ride])
VALUES
    -- CAJ-001: Creado → Llamado → En Proceso → Cerrado (flujo completo)
    ('CAJ-001', 1, 1, 3, 6, 1, 'Pago de servicios básicos',          CAST(@HoyDT + CAST('07:55:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('08:15:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    -- CAJ-002: Creado → Llamado → En Proceso → Derivado a Informes
    ('CAJ-002', 3, 5, 3, 2, 2, 'Consulta de saldo y derivado a Informes', CAST(@HoyDT + CAST('08:00:00' AS DATETIME) AS DATETIME2), NULL, 1, SYSDATETIME(), NEWID()),
    -- CAJ-003: Creado → Llamado → En Proceso → Cerrado
    ('CAJ-003', 4, 2, 3, 6, 1, 'Retiro de efectivo',                 CAST(@HoyDT + CAST('08:45:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('09:20:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    -- CAJ-004: Creado → Llamado (esperando atención)
    ('CAJ-004', 2, 5, 3, 8, 1, 'Solicitud de información de productos', CAST(@HoyDT + CAST('09:00:00' AS DATETIME) AS DATETIME2), NULL, 1, SYSDATETIME(), NEWID()),
    -- CAJ-005: Creado (no llamado aún)
    ('CAJ-005', 1, 2, 4, 1, 1, 'Pago de impuesto municipal',        CAST(@HoyDT + CAST('09:15:00' AS DATETIME) AS DATETIME2), NULL, 1, SYSDATETIME(), NEWID()),
    -- INF-001: Creado en Informes (producto de derivación de CAJ-002, pero también es un ticket independiente de Informes)
    ('INF-001', 5, 5, 3, 6, 2, 'Solicitud de reporte de crédito',   CAST(@HoyDT + CAST('08:30:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('09:00:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    -- INF-002: Creado → Llamado → Cancelado
    ('INF-002', 6, 2, 4, 7, 2, 'Certificado de ingresos',           CAST(@HoyDT + CAST('08:35:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('08:50:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    -- INF-003: Creado → Llamado → En Proceso (actualmente en atención)
    ('INF-003', 5, 1, 3, 3, 2, 'Reporte de historial de pagos',     CAST(@HoyDT + CAST('08:55:00' AS DATETIME) AS DATETIME2), NULL, 1, SYSDATETIME(), NEWID()),
    -- ATE-001: Creado → Llamado → En Proceso (actualmente en atención)
    ('ATE-001', 7, 3, 2, 3, 3, 'Reclamo por cobro indebido',        CAST(@HoyDT + CAST('09:10:00' AS DATETIME) AS DATETIME2), NULL, 1, SYSDATETIME(), NEWID()),
    -- SGE-001: Creado (no llamado aún)
    ('SGE-001', 9, 2, 4, 1, 4, 'Solicitud de mantenimiento de A/C', CAST(@HoyDT + CAST('09:30:00' AS DATETIME) AS DATETIME2), NULL, 1, SYSDATETIME(), NEWID());

PRINT 'Tickets de HOY insertados.';

-- Tickets de AYER (historial) - numeros unicos (101+ para evitar conflicto UQ)
INSERT INTO [dbo].[Tickets] ([NumeroTicket], [ServicioId], [TipoTicketId], [PrioridadId], [EstadoTicketId], [AreaActualId], [Descripcion], [FechaCreacion], [FechaCierre], [Estado], [FechaReg], [Ride])
VALUES
    ('CAJ-101', 2, 5, 3, 6, 1, 'Consulta de productos ayer',              CAST(@AyerDT + CAST('08:00:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('08:20:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    ('CAJ-102', 1, 1, 2, 6, 1, 'Pago urgente de servicios',               CAST(@AyerDT + CAST('08:30:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('09:00:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    ('INF-101', 6, 2, 3, 6, 2, 'Certificado de estudio',                  CAST(@AyerDT + CAST('09:00:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('09:15:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    ('ATE-101', 7, 3, 1, 6, 3, 'Reclamo prioritario de facturación',      CAST(@AyerDT + CAST('09:30:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('10:00:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    ('CAJ-103', 4, 2, 3, 6, 1, 'Retiro de ahorros',                       CAST(@AyerDT + CAST('10:00:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('10:25:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    ('ATE-102', 8, 4, 4, 6, 3, 'Sugerencia de mejora en atención',        CAST(@AyerDT + CAST('11:00:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('11:10:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID()),
    ('CAJ-104', 2, 5, 3, 7, 1, 'Consulta cancelada por usuario',          CAST(@AyerDT + CAST('11:30:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('11:35:00' AS DATETIME) AS DATETIME2), 1, SYSDATETIME(), NEWID());
PRINT 'Tickets de AYER insertados.';

-- =====================================================
-- 7c. ATENCIONES (con FK a sesiones)
-- =====================================================
-- Obtener IDs de tickets (por NumeroTicket y FechaCreacion)
DECLARE @T_CAJ001_HOY INT, @T_CAJ002_HOY INT, @T_CAJ003_HOY INT, @T_CAJ004_HOY INT, @T_CAJ005_HOY INT;
DECLARE @T_INF001_HOY INT, @T_INF002_HOY INT, @T_INF003_HOY INT, @T_ATE001_HOY INT, @T_SGE001_HOY INT;
DECLARE @T_CAJ101_AYER INT, @T_CAJ102_AYER INT, @T_CAJ103_AYER INT, @T_CAJ104_AYER INT;
DECLARE @T_INF101_AYER INT, @T_ATE101_AYER INT, @T_ATE102_AYER INT;

SELECT @T_CAJ001_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='CAJ-001' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_CAJ002_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='CAJ-002' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_CAJ003_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='CAJ-003' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_CAJ004_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='CAJ-004' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_CAJ005_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='CAJ-005' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_INF001_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='INF-001' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_INF002_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='INF-002' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_INF003_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='INF-003' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_ATE001_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='ATE-001' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_SGE001_HOY = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='SGE-001' AND CAST(FechaCreacion AS DATE)=@Hoy;
SELECT @T_CAJ101_AYER = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='CAJ-101' AND CAST(FechaCreacion AS DATE)=@Ayer;
SELECT @T_CAJ102_AYER = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='CAJ-102' AND CAST(FechaCreacion AS DATE)=@Ayer;
SELECT @T_CAJ103_AYER = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='CAJ-103' AND CAST(FechaCreacion AS DATE)=@Ayer;
SELECT @T_CAJ104_AYER = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='CAJ-104' AND CAST(FechaCreacion AS DATE)=@Ayer;
SELECT @T_INF101_AYER = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='INF-101' AND CAST(FechaCreacion AS DATE)=@Ayer;
SELECT @T_ATE101_AYER = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='ATE-101' AND CAST(FechaCreacion AS DATE)=@Ayer;
SELECT @T_ATE102_AYER = TicketId FROM [dbo].[Tickets] WHERE NumeroTicket='ATE-102' AND CAST(FechaCreacion AS DATE)=@Ayer;

PRINT 'IDs de tickets obtenidos.';

-- Atenciones de HOY
INSERT INTO [dbo].[Atencion] ([TicketId], [UsuarioId], [AreaId], [ServicioId], [EstadoTicketId], [SesionOperadorId], [FechaInicio], [FechaFin], [TiempoAtencionSegundos], [FueDerivado], [AreaDestinoId], [Observacion], [Estado], [FechaReg], [Ride])
VALUES
    -- CAJ-001: Maria Garcia atiende y completa
    (@T_CAJ001_HOY, 3, 1, 1, 6, @SesionMariaHoy, CAST(@HoyDT + CAST('08:05:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('08:15:00' AS DATETIME) AS DATETIME2), 600, 0, NULL, 'Pago recibido correctamente', 1, SYSDATETIME(), NEWID()),
    -- CAJ-002: Maria Garcia atiende y deriva a Informes
    (@T_CAJ002_HOY, 3, 1, 3, 2, @SesionMariaHoy, CAST(@HoyDT + CAST('08:10:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('08:30:00' AS DATETIME) AS DATETIME2), 1200, 1, 2, 'Derivado a Informes para reporte detallado', 1, SYSDATETIME(), NEWID()),
    -- INF-001: Laura Gonzalez atiende y completa (ticket propio de Informes)
    (@T_INF001_HOY, 5, 2, 5, 6, @SesionLauraHoy, CAST(@HoyDT + CAST('08:40:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('09:00:00' AS DATETIME) AS DATETIME2), 1200, 0, NULL, 'Reporte entregado al cliente', 1, SYSDATETIME(), NEWID()),
    -- INF-002: Laura Gonzalez atiende y cancela
    (@T_INF002_HOY, 5, 2, 6, 7, @SesionLauraHoy, CAST(@HoyDT + CAST('08:45:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('08:50:00' AS DATETIME) AS DATETIME2), 300, 0, NULL, 'Cliente no presentó documentos requeridos', 1, SYSDATETIME(), NEWID()),
    -- CAJ-003: Maria Garcia atiende y completa
    (@T_CAJ003_HOY, 3, 1, 4, 6, @SesionMariaHoy, CAST(@HoyDT + CAST('09:00:00' AS DATETIME) AS DATETIME2), CAST(@HoyDT + CAST('09:20:00' AS DATETIME) AS DATETIME2), 1200, 0, NULL, 'Retiro procesado exitosamente', 1, SYSDATETIME(), NEWID()),
    -- INF-003: Laura Gonzalez atiende actualmente (sin FechaFin)
    (@T_INF003_HOY, 5, 2, 5, 3, @SesionLauraHoy, CAST(@HoyDT + CAST('09:10:00' AS DATETIME) AS DATETIME2), NULL, NULL, 0, NULL, NULL, 1, SYSDATETIME(), NEWID()),
    -- ATE-001: Ana Rodriguez atiende actualmente (sin FechaFin)
    (@T_ATE001_HOY, 6, 3, 7, 3, @SesionAnaHoy, CAST(@HoyDT + CAST('09:20:00' AS DATETIME) AS DATETIME2), NULL, NULL, 0, NULL, NULL, 1, SYSDATETIME(), NEWID());
PRINT 'Atenciones de HOY insertadas.';

-- Atenciones de AYER
INSERT INTO [dbo].[Atencion] ([TicketId], [UsuarioId], [AreaId], [ServicioId], [EstadoTicketId], [SesionOperadorId], [FechaInicio], [FechaFin], [TiempoAtencionSegundos], [FueDerivado], [AreaDestinoId], [Observacion], [Estado], [FechaReg], [Ride])
VALUES
    (@T_CAJ101_AYER, 3, 1, 2, 6, @SesionMariaAyer, CAST(@AyerDT + CAST('08:05:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('08:20:00' AS DATETIME) AS DATETIME2), 900, 0, NULL, 'Cliente informado sobre productos', 1, SYSDATETIME(), NEWID()),
    (@T_CAJ102_AYER, 3, 1, 1, 6, @SesionMariaAyer, CAST(@AyerDT + CAST('08:35:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('09:00:00' AS DATETIME) AS DATETIME2), 1500, 0, NULL, 'Pago procesado', 1, SYSDATETIME(), NEWID()),
    (@T_CAJ103_AYER, 3, 1, 4, 6, @SesionMariaAyer, CAST(@AyerDT + CAST('10:05:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('10:25:00' AS DATETIME) AS DATETIME2), 1200, 0, NULL, 'Retiro autorizado', 1, SYSDATETIME(), NEWID()),
    (@T_CAJ104_AYER, 3, 1, 2, 7, @SesionMariaAyer, CAST(@AyerDT + CAST('11:32:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('11:35:00' AS DATETIME) AS DATETIME2), 180, 0, NULL, 'Cliente canceló antes de iniciar', 1, SYSDATETIME(), NEWID()),
    (@T_INF101_AYER, 5, 2, 6, 6, @SesionLauraAyer, CAST(@AyerDT + CAST('09:05:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('09:15:00' AS DATETIME) AS DATETIME2), 600, 0, NULL, 'Certificado entregado', 1, SYSDATETIME(), NEWID()),
    (@T_ATE101_AYER, 6, 3, 7, 6, NULL, CAST(@AyerDT + CAST('09:35:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('10:00:00' AS DATETIME) AS DATETIME2), 1500, 0, NULL, 'Reclamo registrado en sistema', 1, SYSDATETIME(), NEWID()),
    (@T_ATE102_AYER, 6, 3, 8, 6, NULL, CAST(@AyerDT + CAST('11:05:00' AS DATETIME) AS DATETIME2), CAST(@AyerDT + CAST('11:10:00' AS DATETIME) AS DATETIME2), 300, 0, NULL, 'Sugerencia documentada', 1, SYSDATETIME(), NEWID());
PRINT 'Atenciones de AYER insertadas.';

-- =====================================================
-- 7d. MARCA CIONES (llamados por el Llamador)
-- =====================================================
-- rtorres (UsuarioId=4) llama para Caja
-- smartinez (UsuarioId=8) llama para ATE
INSERT INTO [dbo].[Marcacion] ([TicketId], [UsuarioId], [KioskoId], [NumeroLlamado], [FechaMarcacion], [Respondio], [Estado], [FechaReg], [Ride])
VALUES
    -- CAJ-001: Llamado desde Kiosko Principal (KioskoId=1) a las 08:00, respondido
    (@T_CAJ001_HOY, 4, 1, 1, CAST(@HoyDT + CAST('08:00:00' AS DATETIME) AS DATETIME2), 1, 1, SYSDATETIME(), NEWID()),
    -- CAJ-002: Llamado desde Kiosko Caja (KioskoId=3) a las 08:05, respondido
    (@T_CAJ002_HOY, 4, 3, 2, CAST(@HoyDT + CAST('08:05:00' AS DATETIME) AS DATETIME2), 1, 1, SYSDATETIME(), NEWID()),
    -- CAJ-003: Llamado desde Kiosko Principal a las 08:50, respondido
    (@T_CAJ003_HOY, 4, 1, 3, CAST(@HoyDT + CAST('08:50:00' AS DATETIME) AS DATETIME2), 1, 1, SYSDATETIME(), NEWID()),
    -- CAJ-004: Llamado desde Kiosko Caja a las 09:05, NO respondido aún
    (@T_CAJ004_HOY, 4, 3, 4, CAST(@HoyDT + CAST('09:05:00' AS DATETIME) AS DATETIME2), 0, 1, SYSDATETIME(), NEWID()),
    -- INF-001: Llamado desde Kiosko Principal a las 08:35, respondido
    (@T_INF001_HOY, 4, 1, 5, CAST(@HoyDT + CAST('08:35:00' AS DATETIME) AS DATETIME2), 1, 1, SYSDATETIME(), NEWID()),
    -- INF-002: Llamado desde Kiosko Principal a las 08:40, respondido
    (@T_INF002_HOY, 4, 1, 6, CAST(@HoyDT + CAST('08:40:00' AS DATETIME) AS DATETIME2), 1, 1, SYSDATETIME(), NEWID()),
    -- INF-003: Llamado desde Kiosko Principal a las 09:00, respondido
    (@T_INF003_HOY, 4, 1, 7, CAST(@HoyDT + CAST('09:00:00' AS DATETIME) AS DATETIME2), 1, 1, SYSDATETIME(), NEWID()),
    -- ATE-001: Llamado desde Kiosko Secundario (KioskoId=2) por smartinez a las 09:15, respondido
    (@T_ATE001_HOY, 8, 2, 1, CAST(@HoyDT + CAST('09:15:00' AS DATETIME) AS DATETIME2), 1, 1, SYSDATETIME(), NEWID());
PRINT 'Marcaciones de HOY insertadas.';

PRINT '';
PRINT '====================================================';
PRINT '  SEED COMPLETADO EXITOSAMENTE';
PRINT '====================================================';
GO

