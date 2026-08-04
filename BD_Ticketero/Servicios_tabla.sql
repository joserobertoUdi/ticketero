/*=============================================================
TABLA : Servicios
DESCRIPCION : Catálogo de servicios disponibles
=============================================================*/

IF OBJECT_ID('dbo.Servicios','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Servicios;
END;
GO

CREATE TABLE dbo.Servicios
(
    ServicioId INT IDENTITY(1,1) NOT NULL,

    Descripcion NVARCHAR(100) NOT NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_Servicios_Estado
        DEFAULT(1),

    FechaReg DATETIME2(3) NOT NULL
        CONSTRAINT DF_Servicios_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Servicios_Ride
        DEFAULT(NEWID()),

    CONSTRAINT PK_Servicios
        PRIMARY KEY CLUSTERED
        (
            ServicioId
        )
);
GO

ALTER TABLE dbo.Servicios
ADD CONSTRAINT UQ_Servicios_Descripcion
UNIQUE
(
    Descripcion
);
GO

CREATE NONCLUSTERED INDEX IX_Servicios_Descripcion
ON dbo.Servicios
(
    Descripcion
);
GO

INSERT INTO dbo.Servicios
(
    Descripcion
)
VALUES
('PAGO'),
('INFORMACION'),
('CONSULTA'),
('RETIRO');
GO

SELECT *
FROM dbo.Servicios;
GO

EXEC sp_help 'dbo.Servicios';
GO

EXEC sp_helpconstraint 'dbo.Servicios';
GO

EXEC sp_helpindex 'dbo.Servicios';
GO
SELECT
COLUMNPROPERTY
(
OBJECT_ID('dbo.Servicios'),
'ServicioId',
'IsIdentity'
) AS EsIdentity;
GO

---------------------------------------------------------
-- Agregar AreaId a Servicios
---------------------------------------------------------

IF COL_LENGTH('dbo.Servicios','AreaId') IS NULL
BEGIN

    ALTER TABLE dbo.Servicios
    ADD AreaId INT NOT NULL
        CONSTRAINT DF_Servicios_Area DEFAULT(1);

END
GO

ALTER TABLE dbo.Servicios
ADD CONSTRAINT FK_Servicios_Areas
FOREIGN KEY(AreaId)
REFERENCES dbo.Areas(AreaId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

CREATE INDEX IX_Servicios_Area
ON dbo.Servicios(AreaId);
GO

CREATE INDEX IX_Servicios_Area_Estado
ON dbo.Servicios(AreaId,Estado);
GO

DROP INDEX IF EXISTS UQ_Servicios_Descripcion
ON dbo.Servicios;
GO

CREATE UNIQUE INDEX UQ_Servicios_Area_Descripcion
ON dbo.Servicios
(
    AreaId,
    Descripcion
);
GO




EXEC sp_help 'Servicios';

EXEC sp_helpconstraint 'Servicios';

EXEC sp_helpindex 'Servicios';

SELECT *
FROM Servicios;


IF COL_LENGTH('dbo.Servicios','AreaId') IS NULL
BEGIN
    ALTER TABLE dbo.Servicios
    ADD AreaId INT NOT NULL
        CONSTRAINT DF_Servicios_Area DEFAULT(1);
END
GO

ALTER TABLE dbo.Servicios
ADD CONSTRAINT FK_Servicios_Areas
FOREIGN KEY (AreaId)
REFERENCES dbo.Areas(AreaId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

CREATE INDEX IX_Servicios_Area
ON dbo.Servicios(AreaId);
GO

ALTER TABLE dbo.Servicios
DROP CONSTRAINT DF_Servicios_Area;
GO

EXEC sp_help 'dbo.Servicios';
GO

EXEC sp_helpconstraint 'dbo.Servicios';
GO

EXEC sp_helpindex 'dbo.Servicios';
GO

SELECT
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TablaHija,
    c1.name AS Columna,
    OBJECT_NAME(fk.referenced_object_id) AS TablaPadre
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc
    ON fk.object_id=fkc.constraint_object_id
INNER JOIN sys.columns c1
    ON c1.object_id=fkc.parent_object_id
   AND c1.column_id=fkc.parent_column_id
WHERE OBJECT_NAME(fk.parent_object_id)='Servicios';
GO