/*=============================================================
BASE DE DATOS : DBTicketero
TABLA         : Kiosko
DESCRIPCIÓN   : Equipos físicos donde se generan tickets
=============================================================*/

IF OBJECT_ID('dbo.Kiosko','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Kiosko;
END;
GO

CREATE TABLE dbo.Kiosko
(
    KioskoId INT IDENTITY(1,1) NOT NULL,

    Descripcion NVARCHAR(100) NOT NULL,

    Ubicacion NVARCHAR(150) NOT NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_Kiosko_Estado
        DEFAULT(1),

    FechaReg DATETIME2(3) NOT NULL
        CONSTRAINT DF_Kiosko_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Kiosko_Ride
        DEFAULT(NEWID()),

    CONSTRAINT PK_Kiosko
    PRIMARY KEY CLUSTERED
    (
        KioskoId
    )
);
GO

ALTER TABLE dbo.Kiosko
ADD CONSTRAINT UQ_Kiosko_Descripcion
UNIQUE
(
    Descripcion
);
GO

CREATE NONCLUSTERED INDEX IX_Kiosko_Descripcion
ON dbo.Kiosko
(
    Descripcion
);
GO

CREATE NONCLUSTERED INDEX IX_Kiosko_Ubicacion
ON dbo.Kiosko
(
    Ubicacion
);
GO

INSERT INTO dbo.Kiosko
(
    Descripcion,
    Ubicacion
)
VALUES
('Kiosko Principal','Planta Baja'),
('Kiosko Norte','Primer Piso'),
('Kiosko Sur','Segundo Piso');
GO

SELECT *
FROM dbo.Kiosko;
GO

EXEC sp_help 'dbo.Kiosko';
GO

EXEC sp_helpconstraint 'dbo.Kiosko';
GO

EXEC sp_helpindex 'dbo.Kiosko';
GO

SELECT
COLUMNPROPERTY
(
OBJECT_ID('dbo.Kiosko'),
'KioskoId',
'IsIdentity'
) AS EsIdentity;
GO


IF COL_LENGTH('dbo.Kiosko', 'UbicacionId') IS NULL
BEGIN
    ALTER TABLE dbo.Kiosko
    ADD UbicacionId INT NOT NULL
        CONSTRAINT DF_Kiosko_Ubicacion DEFAULT(1);
END
GO

ALTER TABLE dbo.Kiosko
ADD CONSTRAINT FK_Kiosko_Ubicaciones
FOREIGN KEY (UbicacionId)
REFERENCES dbo.Ubicaciones(UbicacionId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

CREATE INDEX IX_Kiosko_Ubicacion
ON dbo.Kiosko(UbicacionId);
GO

ALTER TABLE dbo.Kiosko
DROP CONSTRAINT DF_Kiosko_Ubicacion;
GO

EXEC sp_help 'dbo.Kiosko';
GO

EXEC sp_helpconstraint 'dbo.Kiosko';
GO

EXEC sp_helpindex 'dbo.Kiosko';
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
WHERE OBJECT_NAME(fk.parent_object_id)='Kiosko';
GO

