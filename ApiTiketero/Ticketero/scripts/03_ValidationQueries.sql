-- =====================================================
-- 03_ValidationQueries.sql
-- Consultas de validación visual de datos.
-- Cada sección responde una pregunta de negocio.
-- =====================================================
SET QUOTED_IDENTIFIER ON;
GO

USE [DBTicketero_Dev];
GO

-- =======================================================================
-- 1. TICKET: hora creaci?n, kiosko, tiempo de espera
--    ?A qu? hora se cre? el ticket, en qu? kiosko se cre?,
--    cu?nto tiempo esper? para ser atendido?
-- =======================================================================
PRINT '=== 1. DETALLE COMPLETO DE TICKETS ===';
SELECT
    t.NumeroTicket,
    t.FechaCreacion                     AS 'Creaci?n',
    FORMAT(t.FechaCreacion, 'HH:mm:ss') AS 'HoraCreaci?n',
    k.Descripcion                       AS 'KioskoCreaci?n',
    a.Descripcion                       AS '?reaDestino',
    s.Descripcion                       AS 'Servicio',
    et.Descripcion                      AS 'EstadoActual',
    -- Tiempo de espera: desde creaci?n hasta que inicia primera atenci?n
    ISNULL(
        CONVERT(VARCHAR(8),
            DATEADD(SECOND,
                DATEDIFF(SECOND, t.FechaCreacion, 
                    (SELECT MIN(ax.FechaInicio) FROM [dbo].[Atencion] ax WHERE ax.TicketId = t.TicketId)),
                0), 108),
        'Pendiente')                    AS 'Espera(hh:mm:ss)',
    CASE
        WHEN EXISTS (SELECT 1 FROM [dbo].[Atencion] ax WHERE ax.TicketId = t.TicketId)
            THEN CONVERT(VARCHAR(10),
                DATEDIFF(MINUTE, t.FechaCreacion,
                    (SELECT MIN(ax.FechaInicio) FROM [dbo].[Atencion] ax WHERE ax.TicketId = t.TicketId))
                ) + ' min'
        ELSE 'A?n no atendido'
    END                                 AS 'EsperaAprox'
FROM [dbo].[Tickets] t
LEFT JOIN [dbo].[Marcacion]   m  ON m.TicketId = t.TicketId AND m.MarcacionId = (SELECT MIN(mx.MarcacionId) FROM [dbo].[Marcacion] mx WHERE mx.TicketId = t.TicketId)
LEFT JOIN [dbo].[Kiosko]      k  ON k.KioskoId = m.KioskoId
LEFT JOIN [dbo].[AreasFase]   a  ON a.AreaId = t.AreaActualId
LEFT JOIN [dbo].[Servicios]   s  ON s.ServicioId = t.ServicioId
LEFT JOIN [dbo].[EstadosTicket] et ON et.EstadoTicketId = t.EstadoTicketId
ORDER BY t.FechaCreacion;
GO

-- =======================================================================
-- 2. ATENCI?N DE UN TICKET ESPEC?FICO
--    ?Qui?n atendi? tal ticket, cu?nto dur? la atenci?n,
--    fue derivado o no, y qu? servicio estaba consultando?
-- =======================================================================
PRINT '=== 2. ATENCIONES POR TICKET (reemplazar NumeroTicket) ===';
-- Cambiar 'CAJ-002' por el ticket deseado
DECLARE @TicketBuscado VARCHAR(30) = 'CAJ-002';

SELECT
    t.NumeroTicket,
    u.Nombre + ' ' + u.Apellido         AS 'AtendidoPor',
    r.Descripcion                       AS 'RolOperador',
    a.Descripcion                       AS '?reaAtenci?n',
    s.Descripcion                       AS 'ServicioConsultado',
    atencion.FechaInicio                AS 'InicioAtenci?n',
    atencion.FechaFin                   AS 'FinAtenci?n',
    CASE
        WHEN atencion.FechaFin IS NOT NULL THEN
            CONVERT(VARCHAR(8), DATEADD(SECOND,
                DATEDIFF(SECOND, atencion.FechaInicio, atencion.FechaFin), 0), 108)
        ELSE 'En curso'
    END                                 AS 'Duraci?n(hh:mm:ss)',
    atencion.TiempoAtencionSegundos     AS 'Duraci?nSegundos',
    CASE WHEN atencion.FueDerivado = 1 THEN 'S?' ELSE 'No' END AS 'FueDerivado',
    ad.Descripcion                      AS '?reaDestinoDerivaci?n',
    atencion.Observacion                AS 'Observaci?n',
    et.Descripcion                      AS 'EstadoFinalAtenci?n'
FROM [dbo].[Tickets] t
JOIN [dbo].[Atencion] atencion ON atencion.TicketId = t.TicketId
JOIN [dbo].[Usuarios] u        ON u.UsuarioId = atencion.UsuarioId
JOIN [dbo].[Roles] r           ON r.RolId = u.RolId
JOIN [dbo].[AreasFase] a       ON a.AreaId = atencion.AreaId
JOIN [dbo].[Servicios] s       ON s.ServicioId = atencion.ServicioId
JOIN [dbo].[EstadosTicket] et  ON et.EstadoTicketId = atencion.EstadoTicketId
LEFT JOIN [dbo].[AreasFase] ad ON ad.AreaId = atencion.AreaDestinoId
WHERE t.NumeroTicket = @TicketBuscado
ORDER BY atencion.FechaInicio;
GO

-- =======================================================================
-- 3. SESI?N DEL OPERADOR: login, primer ticket, ?ltima atenci?n, logout
--    ?A qu? hora inici? sesi?n el operador, a qu? hora fue su
--    primer ticket, hasta qu? hora fue su ?ltima atenci?n,
--    a qu? hora cerr? sesi?n?
-- =======================================================================
PRINT '=== 3. SESIONES DE OPERADOR CON DETALLE DE ATENCIONES ===';
SELECT
    u.Nombre + ' ' + u.Apellido         AS 'Operador',
    u.NombreUsuario                     AS 'Usuario',
    so.SesionOperadorId                 AS 'SesionId',
    so.FechaInicio                      AS 'InicioSesi?n',
    FORMAT(so.FechaInicio, 'HH:mm:ss')  AS 'HoraLogin',
    so.FechaFin                         AS 'FinSesi?n',
    FORMAT(so.FechaFin, 'HH:mm:ss')     AS 'HoraLogout',
    ISNULL(
        CONVERT(VARCHAR(8), DATEADD(SECOND,
            DATEDIFF(SECOND, so.FechaInicio, ISNULL(so.FechaFin, SYSDATETIME())), 0), 108),
        'En curso')                     AS 'DuracionSesion(hh:mm:ss)',
    -- Primer ticket de la sesi?n
    (SELECT MIN(ax.FechaInicio) FROM [dbo].[Atencion] ax WHERE ax.SesionOperadorId = so.SesionOperadorId) AS 'PrimerTicket',
    FORMAT(
        (SELECT MIN(ax.FechaInicio) FROM [dbo].[Atencion] ax WHERE ax.SesionOperadorId = so.SesionOperadorId),
        'HH:mm:ss')                     AS 'HoraPrimerTicket',
    -- ?ltima atenci?n de la sesi?n
    (SELECT MAX(ax.FechaFin) FROM [dbo].[Atencion] ax WHERE ax.SesionOperadorId = so.SesionOperadorId) AS 'UltimaAtencionFin',
    FORMAT(
        (SELECT MAX(ax.FechaFin) FROM [dbo].[Atencion] ax WHERE ax.SesionOperadorId = so.SesionOperadorId),
        'HH:mm:ss')                     AS 'HoraUltimaAtencion',
    -- Total de tickets atendidos en esta sesi?n
    (SELECT COUNT(*) FROM [dbo].[Atencion] ax WHERE ax.SesionOperadorId = so.SesionOperadorId) AS 'TicketsAtendidos'
FROM [dbo].[SesionOperador] so
JOIN [dbo].[Usuarios] u ON u.UsuarioId = so.UsuarioId
ORDER BY so.FechaInicio DESC;
GO

-- =======================================================================
-- 4. KIOSKO EN CAJA: IP, impresora, video
--    ?Qu? kiosko est? en el ?rea de caja, qu? IP tiene,
--    qu? impresora tiene y qu? video est? mostrando?
-- =======================================================================
PRINT '=== 4. KIOSKOS DEL ?REA DE CAJA CON EQUIPAMIENTO ===';
SELECT
    k.Descripcion                       AS 'Kiosko',
    k.Ubicacion                         AS 'Ubicaci?nF?sica',
    k.IpKiosko                          AS 'IPKiosko',
    k.IpEquipo                          AS 'IPEquipo',
    -- Configuraci?n de red
    cr.TipoConexion                     AS 'TipoRed',
    cr.DHCP                             AS 'DHCP',
    cr.DireccionIP                      AS 'IPConfigurada',
    cr.MascaraSubred                    AS 'M?scara',
    cr.Gateway                          AS 'Gateway',
    -- Impresora
    ci.NombreImpresora                  AS 'Impresora',
    ci.TipoConexion                     AS 'Conexi?nImpresora',
    ci.Puerto                           AS 'PuertoImpresora',
    ci.DireccionIP                      AS 'IPImpresora',
    ci.AnchoPapelMM                     AS 'AnchoPapel(mm)',
    ci.ImpresionAutomatica              AS 'Impresi?nAutom?tica',
    -- Multimedia (video)
    cm.NombreContenido                  AS 'VideoMostrando',
    cm.TipoContenido                    AS 'TipoVideo',
    cm.RutaArchivo                      AS 'RutaVideo',
    cm.DuracionSegundos                 AS 'Duraci?nVideo(s)',
    cm.Repetir                          AS 'RepetirVideo'
FROM [dbo].[Kiosko] k
JOIN [dbo].[KioskoAreas] ka ON ka.KioskoId = k.KioskoId AND ka.AreaId = 1  -- Caja = AreaId 1
LEFT JOIN [dbo].[ConfiguracionRed] cr ON cr.KioskoId = k.KioskoId
LEFT JOIN [dbo].[ConfiguracionImpresora] ci ON ci.KioskoId = k.KioskoId
LEFT JOIN [dbo].[ConfiguracionMultimedia] cm ON cm.KioskoId = k.KioskoId
ORDER BY k.Descripcion;
GO

-- =======================================================================
-- 5. USUARIO: creado por, fecha, primera ?rea, otras ?reas atendidas
--    ?Qui?n cre? tal usuario, cu?ndo, y cu?l fue su primera ?rea,
--    y si ha atendido otras ?reas?
-- NOTA: El sistema no tiene "CreadoPorUsuarioId" en la tabla Usuarios.
--       Se infiere del Ride (UUID ?nico) y el primer UsuarioArea.
--       Como todos los usuarios fueron creados por el Seeder (admin),
--       mostramos el admin como creador gen?rico y las ?reas asignadas.
-- =======================================================================
PRINT '=== 5. USUARIOS: CREACION, AREAS ASIGNADAS Y ATENDIDAS ===';
PRINT '  (cambiar WHERE u.NombreUsuario por el usuario deseado, ej: mgarcia)';
SELECT
    u.NombreUsuario                 AS 'Usuario',
    u.Nombre + ' ' + u.Apellido     AS 'NombreCompleto',
    r.Descripcion                   AS 'Rol',
    u.FechaReg                      AS 'FechaCreacion',
    'admin'                         AS 'CreadoPor',
    (SELECT TOP 1 a.Descripcion
     FROM [dbo].[UsuariosAreaFase] uaf
     JOIN [dbo].[AreasFase] a ON a.AreaId = uaf.AreaId
     WHERE uaf.UsuarioId = u.UsuarioId
     ORDER BY uaf.FechaReg ASC)     AS 'PrimeraAreaAsignada',
    (SELECT COUNT(*) FROM [dbo].[UsuariosAreaFase] uaf WHERE uaf.UsuarioId = u.UsuarioId) AS 'TotalAreasAsignadas',
    STUFF((SELECT ', ' + ad.Descripcion
           FROM [dbo].[Atencion] ax
           JOIN [dbo].[AreasFase] ad ON ad.AreaId = ax.AreaId
           WHERE ax.UsuarioId = u.UsuarioId
           FOR XML PATH('')), 1, 2, '') AS 'AreasQueHaAtendido'
FROM [dbo].[Usuarios] u
JOIN [dbo].[Roles] r ON r.RolId = u.RolId
WHERE u.NombreUsuario = 'mgarcia';
GO

-- Visi?n general de todos los usuarios con ?reas
PRINT '=== 5b. VISI?N GENERAL DE USUARIOS Y ?REAS ===';
SELECT
    u.NombreUsuario                     AS 'Usuario',
    u.Nombre + ' ' + u.Apellido         AS 'Nombre',
    r.Descripcion                       AS 'Rol',
    u.FechaReg                          AS 'FechaCreacion',
    STUFF((SELECT ', ' + a.Descripcion
           FROM [dbo].[UsuariosAreaFase] uaf
           JOIN [dbo].[AreasFase] a ON a.AreaId = uaf.AreaId
           WHERE uaf.UsuarioId = u.UsuarioId
           ORDER BY uaf.FechaReg
           FOR XML PATH('')), 1, 2, '') AS '?reasAsignadas',
    (SELECT COUNT(DISTINCT ax.AreaId) FROM [dbo].[Atencion] ax WHERE ax.UsuarioId = u.UsuarioId) AS '?reasDistintasAtendidas'
FROM [dbo].[Usuarios] u
JOIN [dbo].[Roles] r ON r.RolId = u.RolId
ORDER BY u.FechaReg;
GO

-- =======================================================================
-- 6. TICKETS ATENDIDOS POR OPERADOR HOY
--    Todos los tickets que atendi? tal operador en el d?a de hoy,
--    cu?ntos normales y cu?ntos referidos (derivados),
--    de qu? ?rea y qui?n los deriv? (si aplica).
-- =======================================================================
PRINT '=== 6. TICKETS ATENDIDOS POR OPERADOR EL D?A DE HOY ===';
-- Cambiar 'mgarcia' por el operador deseado, y la fecha si es necesario
DECLARE @Operador VARCHAR(100) = 'mgarcia';
DECLARE @FechaConsulta DATE = '2026-07-09';
DECLARE @OperadorId INT = (SELECT UsuarioId FROM [dbo].[Usuarios] WHERE NombreUsuario = @Operador);

IF @OperadorId IS NOT NULL
BEGIN
    -- Detalle de cada ticket
    SELECT
        t.NumeroTicket                  AS 'Ticket',
        t.Descripcion                   AS 'Descripci?n',
        a.Descripcion                   AS '?reaAtenci?n',
        s.Descripcion                   AS 'Servicio',
        atencion.FechaInicio            AS 'Inicio',
        atencion.FechaFin               AS 'Fin',
        CONVERT(VARCHAR(8), DATEADD(SECOND,
            ISNULL(atencion.TiempoAtencionSegundos, DATEDIFF(SECOND, atencion.FechaInicio, ISNULL(atencion.FechaFin, SYSDATETIME()))),
            0), 108)                    AS 'Duraci?n',
        CASE WHEN EXISTS (
            SELECT 1 FROM [dbo].[Atencion] ant
            WHERE ant.TicketId = t.TicketId
              AND ant.FueDerivado = 1
              AND ant.AreaDestinoId = atencion.AreaId
        ) THEN 'Derivado' ELSE 'Normal' END AS 'Tipo',
        -- ?rea desde la que fue derivado (si aplica)
        ISNULL(
            (SELECT adi.Descripcion
             FROM [dbo].[Atencion] ant2
             JOIN [dbo].[AreasFase] adi ON adi.AreaId = ant2.AreaId
             WHERE ant2.TicketId = t.TicketId
               AND ant2.FueDerivado = 1
               AND ant2.AreaDestinoId = atencion.AreaId),
            'Original')                 AS 'DerivadoDesde',
        -- Qui?n deriv? este ticket hacia esta ?rea
        ISNULL(
            (SELECT uo.Nombre + ' ' + uo.Apellido
             FROM [dbo].[Atencion] ant3
             JOIN [dbo].[Usuarios] uo ON uo.UsuarioId = ant3.UsuarioId
             WHERE ant3.TicketId = t.TicketId
               AND ant3.FueDerivado = 1
               AND ant3.AreaDestinoId = atencion.AreaId),
            'N/A')                      AS 'DerivadoPor',
        et.Descripcion                  AS 'Estado'
    FROM [dbo].[Atencion] atencion
    JOIN [dbo].[Tickets] t         ON t.TicketId = atencion.TicketId
    JOIN [dbo].[AreasFase] a       ON a.AreaId = atencion.AreaId
    JOIN [dbo].[Servicios] s       ON s.ServicioId = atencion.ServicioId
    JOIN [dbo].[EstadosTicket] et  ON et.EstadoTicketId = atencion.EstadoTicketId
    LEFT JOIN [dbo].[AreasFase] ad ON ad.AreaId = atencion.AreaDestinoId
    WHERE atencion.UsuarioId = @OperadorId
      AND CAST(atencion.FechaInicio AS DATE) = @FechaConsulta
    ORDER BY atencion.FechaInicio;

    -- Resumen (usando LEFT JOIN en lugar de subquery dentro de SUM)
    SELECT
        @Operador                       AS 'Operador',
        COUNT(*)                        AS 'TotalTicketsHoy',
        COUNT(CASE WHEN ant_der.FueDerivado IS NULL THEN 1 END) AS 'Normales',
        COUNT(CASE WHEN ant_der.FueDerivado = 1 THEN 1 END)    AS 'Derivados',
        COUNT(DISTINCT atencion.AreaId) AS '?reasDistintas'
    FROM [dbo].[Atencion] atencion
    LEFT JOIN [dbo].[Atencion] ant_der
        ON ant_der.TicketId = atencion.TicketId
       AND ant_der.FueDerivado = 1
       AND ant_der.AreaDestinoId = atencion.AreaId
    WHERE atencion.UsuarioId = @OperadorId
      AND CAST(atencion.FechaInicio AS DATE) = @FechaConsulta;
END
ELSE
    PRINT 'Operador no encontrado: ' + @Operador;
GO

-- =======================================================================
-- 7. USUARIO CON M?S DERIVACIONES DEL D?A
--    ?Cu?l fue el usuario que m?s tickets derivados tuvo en el d?a de hoy?
-- =======================================================================
PRINT '=== 7. RANKING DE DERIVACIONES DEL D?A DE HOY ===';
SELECT TOP 5
    u.Nombre + ' ' + u.Apellido         AS 'Operador',
    u.NombreUsuario                     AS 'Usuario',
    COUNT(*)                            AS 'TicketsDerivados',
    -- ?reas a las que deriv?
    STUFF((SELECT ', ' + ad.Descripcion
           FROM (SELECT DISTINCT ad2.AreaDestinoId
                 FROM [dbo].[Atencion] ad2
                 WHERE ad2.UsuarioId = u.UsuarioId
                   AND ad2.FueDerivado = 1
                   AND CAST(ad2.FechaInicio AS DATE) = '2026-07-09') dest
           JOIN [dbo].[AreasFase] ad ON ad.AreaId = dest.AreaDestinoId
           FOR XML PATH('')), 1, 2, '') AS '?reasDestino',
    -- Tickets que deriv? (n?meros)
    STUFF((SELECT ', ' + t.NumeroTicket
           FROM [dbo].[Atencion] ax3
           JOIN [dbo].[Tickets] t ON t.TicketId = ax3.TicketId
           WHERE ax3.UsuarioId = u.UsuarioId
             AND ax3.FueDerivado = 1
             AND CAST(ax3.FechaInicio AS DATE) = '2026-07-09'
           FOR XML PATH('')), 1, 2, '') AS 'TicketsDerivados'
FROM [dbo].[Atencion] ax
JOIN [dbo].[Usuarios] u ON u.UsuarioId = ax.UsuarioId
WHERE ax.FueDerivado = 1
  AND CAST(ax.FechaInicio AS DATE) = '2026-07-09'
GROUP BY u.UsuarioId, u.Nombre, u.Apellido, u.NombreUsuario
ORDER BY COUNT(*) DESC;
GO

-- =======================================================================
-- 8. VALIDACI?N DE INTEGRIDAD (datos hu?rfanos o inconsistentes)
-- =======================================================================
PRINT '=== 8. VALIDACI?N DE INTEGRIDAD ===';

-- Tickets sin ninguna atenci?n
SELECT 'Tickets sin atenci?n' AS 'Problema',
       COUNT(*) AS 'Cantidad',
       STUFF((SELECT ', ' + t2.NumeroTicket
              FROM [dbo].[Tickets] t2
              WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Atencion] ax WHERE ax.TicketId = t2.TicketId)
              FOR XML PATH('')), 1, 2, '') AS 'Detalle'
FROM [dbo].[Tickets] t
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Atencion] ax WHERE ax.TicketId = t.TicketId)
UNION ALL
-- Tickets llamados pero sin marcaci?n
SELECT 'Ticket llamado sin marcaci?n' AS 'Problema',
       COUNT(*),
       STUFF((SELECT ', ' + t2.NumeroTicket
              FROM [dbo].[Tickets] t2
              WHERE t2.EstadoTicketId = 8
                AND NOT EXISTS (SELECT 1 FROM [dbo].[Marcacion] mx WHERE mx.TicketId = t2.TicketId)
              FOR XML PATH('')), 1, 2, '')
FROM [dbo].[Tickets] t
WHERE t.EstadoTicketId = 8
  AND NOT EXISTS (SELECT 1 FROM [dbo].[Marcacion] mx WHERE mx.TicketId = t.TicketId)
UNION ALL
-- Atenciones con estado "En Proceso" pero sin FechaInicio (no deber?a ocurrir)
SELECT 'Atenci?n En Proceso sin FechaInicio',
       COUNT(*), ''
FROM [dbo].[Atencion]
WHERE EstadoTicketId = 3 AND FechaInicio IS NULL
UNION ALL
-- Atenciones cerradas sin FechaFin
SELECT 'Atenci?n cerrada sin FechaFin',
       COUNT(*), ''
FROM [dbo].[Atencion]
WHERE EstadoTicketId IN (6, 7) AND FechaFin IS NULL
UNION ALL
-- Tickets cerrados sin FechaCierre
SELECT 'Ticket cerrado sin FechaCierre',
       COUNT(*), ''
FROM [dbo].[Tickets]
WHERE EstadoTicketId = 6 AND FechaCierre IS NULL
UNION ALL
-- Tickets cancelados sin FechaCierre
SELECT 'Ticket cancelado sin FechaCierre',
       COUNT(*), ''
FROM [dbo].[Tickets]
WHERE EstadoTicketId = 7 AND FechaCierre IS NULL
UNION ALL
-- Marcaciones con Respondio=0 pero el ticket ya est? siendo atendido
SELECT 'Marcaci?n no respondida pero ticket en atenci?n',
       COUNT(*),
       STUFF((SELECT ', ' + t2.NumeroTicket
              FROM [dbo].[Marcacion] mx2
              JOIN [dbo].[Tickets] t2 ON t2.TicketId = mx2.TicketId
              WHERE mx2.Respondio = 0 AND t2.EstadoTicketId = 3
              FOR XML PATH('')), 1, 2, '')
FROM [dbo].[Marcacion] mx
JOIN [dbo].[Tickets] t ON t.TicketId = mx.TicketId
WHERE mx.Respondio = 0 AND t.EstadoTicketId = 3;
GO

-- =======================================================================
-- 9. MAPA COMPLETO DE FLUJO (trazabilidad por ticket)
-- =======================================================================
PRINT '=== 9. TRAZABILIDAD COMPLETA POR TICKET ===';
SELECT
    t.NumeroTicket                      AS 'Ticket',
    FORMAT(t.FechaCreacion, 'yyyy-MM-dd HH:mm:ss') AS 'Creado',
    CASE t.EstadoTicketId
        WHEN 1 THEN 'Nuevo'
        WHEN 2 THEN 'Asignado/Derivado'
        WHEN 3 THEN 'En Proceso'
        WHEN 6 THEN 'Cerrado'
        WHEN 7 THEN 'Cancelado'
        WHEN 8 THEN 'Llamado'
        ELSE 'Desconocido'
    END                                 AS 'EstadoActual',
    -- ?reas por las que pas?
    STUFF((SELECT ' ? ' + ar.Descripcion
           FROM [dbo].[Atencion] ax2
           JOIN [dbo].[AreasFase] ar ON ar.AreaId = ax2.AreaId
           WHERE ax2.TicketId = t.TicketId
           ORDER BY ax2.FechaInicio
           FOR XML PATH('')), 1, 3, '') AS '?reasRecorridas',
    -- Operadores que lo atendieron
    STUFF((SELECT ' ? ' + uo.NombreUsuario
           FROM [dbo].[Atencion] ax3
           JOIN [dbo].[Usuarios] uo ON uo.UsuarioId = ax3.UsuarioId
           WHERE ax3.TicketId = t.TicketId
           ORDER BY ax3.FechaInicio
           FOR XML PATH('')), 1, 3, '') AS 'Operadores',
    -- Tiempo total desde creaci?n hasta cierre
    CASE
        WHEN t.FechaCierre IS NOT NULL THEN
            CONVERT(VARCHAR(8), DATEADD(SECOND,
                DATEDIFF(SECOND, t.FechaCreacion, t.FechaCierre), 0), 108)
        ELSE 'Abierto'
    END                                 AS 'CicloVida(hh:mm:ss)',
    ISNULL(
        CONVERT(VARCHAR(8), DATEADD(SECOND,
            (SELECT ISNULL(SUM(ax4.TiempoAtencionSegundos), 0)
             FROM [dbo].[Atencion] ax4 WHERE ax4.TicketId = t.TicketId)
            , 0), 108),
        '0')                            AS 'TiempoAtenci?nTotal'
FROM [dbo].[Tickets] t
WHERE CAST(t.FechaCreacion AS DATE) = '2026-07-09'
ORDER BY t.NumeroTicket;
GO

PRINT '';
PRINT '====================================================';
PRINT '  VALIDACIONES COMPLETADAS';
PRINT '====================================================';
GO
