/*=============================================================
BASE DE DATOS : DBTicketero
TABLA         : Atencion
DESCRIPCIÓN   : Registro de las atenciones realizadas a un ticket
=============================================================*/

IF OBJECT_ID('dbo.Atencion','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Atencion;
END;
GO

CREATE TABLE dbo.Atencion
(
    AtencionId INT IDENTITY(1,1) NOT NULL,

    TicketId INT NOT NULL,

    UsuarioId INT NOT NULL,

    AreaId INT NOT NULL,

    ServicioId INT NOT NULL,

    EstadoTicketId INT NOT NULL,

    FechaInicio DATETIME2(3) NOT NULL,

    FechaFin DATETIME2(3) NULL,

    TiempoAtencionSegundos INT NULL,

    FueDerivado BIT NOT NULL
        CONSTRAINT DF_Atencion_FueDerivado
        DEFAULT(0),

    AreaDestinoId INT NULL,

    Observacion NVARCHAR(500) NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_Atencion_Estado
        DEFAULT(1),

    FechaReg DATETIME2(3) NOT NULL
        CONSTRAINT DF_Atencion_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Atencion_Ride
        DEFAULT(NEWID()),

    CONSTRAINT PK_Atencion
        PRIMARY KEY CLUSTERED
        (
            AtencionId
        )
);
GO

/* Usuario que atendió */

ALTER TABLE dbo.Atencion
ADD CONSTRAINT FK_Atencion_Usuarios
FOREIGN KEY (UsuarioId)
REFERENCES dbo.Usuarios(UsuarioId);
GO

/* Área donde se atendió */

ALTER TABLE dbo.Atencion
ADD CONSTRAINT FK_Atencion_Areas
FOREIGN KEY (AreaId)
REFERENCES dbo.Areas(AreaId);
GO

/* Servicio realizado */

ALTER TABLE dbo.Atencion
ADD CONSTRAINT FK_Atencion_Servicios
FOREIGN KEY (ServicioId)
REFERENCES dbo.Servicios(ServicioId);
GO

/* Estado del ticket durante la atención */

ALTER TABLE dbo.Atencion
ADD CONSTRAINT FK_Atencion_EstadosTicket
FOREIGN KEY (EstadoTicketId)
REFERENCES dbo.EstadosTicket(EstadoTicketId);
GO

/* Área destino en caso de derivación */

ALTER TABLE dbo.Atencion
ADD CONSTRAINT FK_Atencion_AreaDestino
FOREIGN KEY (AreaDestinoId)
REFERENCES dbo.Areas(AreaId);
GO

ALTER TABLE dbo.Atencion
ADD CONSTRAINT CK_Atencion_Tiempo
CHECK
(
    TiempoAtencionSegundos IS NULL
    OR TiempoAtencionSegundos >= 0
);
GO

CREATE NONCLUSTERED INDEX IX_Atencion_Ticket
ON dbo.Atencion(TicketId);
GO

CREATE NONCLUSTERED INDEX IX_Atencion_Usuario
ON dbo.Atencion(UsuarioId);
GO

CREATE NONCLUSTERED INDEX IX_Atencion_Area
ON dbo.Atencion(AreaId);
GO

CREATE NONCLUSTERED INDEX IX_Atencion_FechaInicio
ON dbo.Atencion(FechaInicio);
GO

CREATE NONCLUSTERED INDEX IX_Atencion_EstadoTicket
ON dbo.Atencion(EstadoTicketId);
GO

EXEC sp_help 'dbo.Atencion';
GO
EXEC sp_helpconstraint 'dbo.Atencion';
GO
EXEC sp_helpindex 'dbo.Atencion';
GO
SELECT
COLUMNPROPERTY
(
OBJECT_ID('dbo.Atencion'),
'AtencionId',
'IsIdentity'
) AS EsIdentity;
GO
EXEC sp_fkeys 'Atencion';
GO
SELECT *
FROM dbo.Atencion;
GO

CREATE NONCLUSTERED INDEX IX_Atencion_Area_Fecha
ON dbo.Atencion
(
    AreaId,
    FechaInicio
);
GO

CREATE NONCLUSTERED INDEX IX_Atencion_Usuario_Fecha
ON dbo.Atencion
(
    UsuarioId,
    FechaInicio
);
GO

SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='Atencion'
AND COLUMN_NAME='PuestoId';
GO

ALTER TABLE dbo.Atencion
ADD PuestoId INT NOT NULL
CONSTRAINT DF_Atencion_Puesto DEFAULT(1);
GO

ALTER TABLE dbo.Atencion
ADD CONSTRAINT FK_Atencion_Puestos
FOREIGN KEY (PuestoId)
REFERENCES dbo.Puestos(PuestoId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

CREATE INDEX IX_Atencion_Puesto
ON dbo.Atencion(PuestoId);
GO

ALTER TABLE dbo.Atencion
DROP CONSTRAINT DF_Atencion_Puesto;
GO

EXEC sp_help 'dbo.Atencion';
GO

EXEC sp_helpconstraint 'dbo.Atencion';
GO

EXEC sp_helpindex 'dbo.Atencion';
GO

SELECT
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TablaHija,
    c1.name AS Columna,
    OBJECT_NAME(fk.referenced_object_id) AS TablaPadre
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc
    ON fk.object_id=fkc.constraint_object_id
JOIN sys.columns c1
    ON c1.object_id=fkc.parent_object_id
   AND c1.column_id=fkc.parent_column_id
WHERE OBJECT_NAME(fk.parent_object_id)='Atencion';
GO

SELECT
    name,
    delete_referential_action_desc,
    update_referential_action_desc
FROM sys.foreign_keys
WHERE name='FK_Atencion_Puestos';
GO

IF COL_LENGTH('dbo.Atencion', 'SesionOperadorId') IS NULL
BEGIN
    ALTER TABLE dbo.Atencion
    ADD SesionOperadorId INT NULL;
END
GO

ALTER TABLE dbo.Atencion
ADD CONSTRAINT FK_Atencion_SesionOperador
FOREIGN KEY (SesionOperadorId)
REFERENCES dbo.SesionOperador(SesionOperadorId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

CREATE INDEX IX_Atencion_SesionOperador
ON dbo.Atencion(SesionOperadorId);
GO

DROP INDEX IX_Atencion_Puesto
ON dbo.Atencion;
GO

ALTER TABLE dbo.Atencion
DROP CONSTRAINT FK_Atencion_Puestos;
GO

ALTER TABLE dbo.Atencion
DROP COLUMN PuestoId;
GO