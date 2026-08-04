/*==============================================================
    BASE DE DATOS : DBTicketero
    TABLA         : Prioridades
    DESCRIPCIÓN   : Catálogo de prioridades de los tickets
==============================================================*/

IF OBJECT_ID('dbo.Prioridades','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Prioridades;
END;
GO

CREATE TABLE dbo.Prioridades
(
    PrioridadId INT IDENTITY(1,1) NOT NULL,

    Descripcion NVARCHAR(100) NOT NULL,

    Nivel TINYINT NOT NULL,

    Color NVARCHAR(20) NOT NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_Prioridades_Estado
        DEFAULT(1),

    FechaReg DATETIME2(3) NOT NULL
        CONSTRAINT DF_Prioridades_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Prioridades_Ride
        DEFAULT(NEWID()),

    CONSTRAINT PK_Prioridades
        PRIMARY KEY CLUSTERED
        (
            PrioridadId
        )
);
GO
ALTER TABLE dbo.Prioridades
ADD CONSTRAINT UQ_Prioridades_Descripcion
UNIQUE (Descripcion);
GO
ALTER TABLE dbo.Prioridades
ADD CONSTRAINT UQ_Prioridades_Nivel
UNIQUE (Nivel);
GO

CREATE NONCLUSTERED INDEX IX_Prioridades_Descripcion
ON dbo.Prioridades (Descripcion);
GO

CREATE NONCLUSTERED INDEX IX_Prioridades_Nivel
ON dbo.Prioridades (Nivel);
GO

INSERT INTO dbo.Prioridades
(
    Descripcion,
    Nivel,
    Color
)
VALUES
('Critica',1,'Rojo'),
('Alta',2,'Naranja'),
('Media',3,'Amarillo'),
('Baja',4,'Verde');
GO

SELECT *
FROM dbo.Prioridades;
GO
EXEC sp_help 'dbo.Prioridades';
GO

EXEC sp_helpconstraint 'dbo.Prioridades';
GO

EXEC sp_helpindex 'dbo.Prioridades';
GO

SELECT
    COLUMNPROPERTY
    (
        OBJECT_ID('dbo.Prioridades'),
        'PrioridadId',
        'IsIdentity'
    ) AS EsIdentity;
GO