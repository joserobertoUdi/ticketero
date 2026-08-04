-- =====================================================
-- 01_Cleanup.sql
-- Limpia TODAS las tablas para empezar desde cero.
-- No modifica estructura, solo datos.
-- Orden de borrado respeta dependencias de FK.
-- =====================================================
SET QUOTED_IDENTIFIER ON;
GO

USE [DBTicketero_Dev];
GO

PRINT 'Limpiando todas las tablas (orden inverso a FKs)...';
GO

DELETE FROM [dbo].[ConfiguracionRed];
DELETE FROM [dbo].[ConfiguracionMultimedia];
DELETE FROM [dbo].[ConfiguracionImpresora];
DELETE FROM [dbo].[ActivosFijos];
DELETE FROM [dbo].[Marcacion];
DELETE FROM [dbo].[Atencion];
DELETE FROM [dbo].[SesionOperador];
DELETE FROM [dbo].[Tickets];
DELETE FROM [dbo].[UsuariosAreaFase];
DELETE FROM [dbo].[KioskoAreas];
DELETE FROM [dbo].[Usuarios];
DELETE FROM [dbo].[Puestos];
DELETE FROM [dbo].[Kiosko];
DELETE FROM [dbo].[Ubicaciones];
DELETE FROM [dbo].[Servicios];
DELETE FROM [dbo].[AreasFase];
DELETE FROM [dbo].[TiposTicket];
DELETE FROM [dbo].[Prioridades];
DELETE FROM [dbo].[EstadosTicket];
DELETE FROM [dbo].[Roles];
DELETE FROM [dbo].[Configuraciones];
PRINT '  Todas las tablas limpiadas.';

-- Resetear identidades
DBCC CHECKIDENT ('[dbo].[ConfiguracionRed]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ConfiguracionMultimedia]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ConfiguracionImpresora]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ActivosFijos]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Marcacion]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Atencion]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[SesionOperador]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Tickets]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[UsuariosAreaFase]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[KioskoAreas]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Usuarios]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Puestos]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Kiosko]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Ubicaciones]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Servicios]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[AreasFase]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[TiposTicket]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Prioridades]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[EstadosTicket]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Roles]', RESEED, 0);
PRINT '  Identidades reseteadas.';

PRINT '';
PRINT '====================================================';
PRINT '  LIMPIEZA COMPLETADA - BD lista para reseed';
PRINT '====================================================';
GO
