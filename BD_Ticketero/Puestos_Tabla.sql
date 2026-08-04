/*==========================================================
 TABLA: Puestos
==========================================================*/

IF OBJECT_ID('dbo.Puestos','U') IS NOT NULL
    DROP TABLE dbo.Puestos;
GO

CREATE TABLE dbo.Puestos
(
    PuestoId INT IDENTITY(1,1) NOT NULL,

    AreaId INT NOT NULL,

    Descripcion NVARCHAR(100) NOT NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_Puestos_Estado
        DEFAULT(1),

    FechaReg DATETIME2 NOT NULL
        CONSTRAINT DF_Puestos_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Puestos_Ride
        DEFAULT(NEWSEQUENTIALID()),

    CONSTRAINT PK_Puestos
        PRIMARY KEY(PuestoId),

    CONSTRAINT UQ_Puestos_Descripcion
        UNIQUE(Descripcion),

    CONSTRAINT FK_Puestos_Areas
        FOREIGN KEY(AreaId)
        REFERENCES Areas(AreaId)
);
GO

CREATE INDEX IX_Puestos_Area
ON Puestos(AreaId);
GO

CREATE INDEX IX_Puestos_Estado
ON Puestos(Estado);
GO

CREATE INDEX IX_Puestos_Descripcion
ON Puestos(Descripcion);
GO

INSERT INTO Puestos
(
AreaId,
Descripcion
)
VALUES
(1,'Caja 1'),
(1,'Caja 2'),
(1,'Caja 3');

EXEC sp_helpconstraint 'dbo.Puestos';

ALTER TABLE dbo.Puestos
DROP CONSTRAINT UQ_Puestos_Descripcion;
GO

ALTER TABLE dbo.Puestos
ADD CONSTRAINT UQ_Puestos_Area_Descripcion
UNIQUE
(
    AreaId,
    Descripcion
);
GO

EXEC sp_help Puestos;

EXEC sp_helpconstraint 'Puestos';

EXEC sp_helpindex 'Puestos';

SELECT * FROM Puestos;