CREATE TABLE dbo.KioskoAreas
(
    KioskoAreaId INT IDENTITY(1,1) NOT NULL,

    KioskoId INT NOT NULL,

    AreaId INT NOT NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_KioskoAreas_Estado DEFAULT(1),

    FechaReg DATETIME2 NOT NULL
        CONSTRAINT DF_KioskoAreas_FechaReg DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_KioskoAreas_Ride DEFAULT(NEWID()),

    CONSTRAINT PK_KioskoAreas
        PRIMARY KEY(KioskoAreaId)
);
GO

ALTER TABLE dbo.KioskoAreas
ADD CONSTRAINT FK_KioskoAreas_Kioskos
FOREIGN KEY(KioskoId)
REFERENCES dbo.Kiosko(KioskoId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

ALTER TABLE dbo.KioskoAreas
ADD CONSTRAINT FK_KioskoAreas_Areas
FOREIGN KEY(AreaId)
REFERENCES dbo.Areas(AreaId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

CREATE INDEX IX_KioskoAreas_Kiosko
ON dbo.KioskoAreas(KioskoId);
GO

CREATE INDEX IX_KioskoAreas_Area
ON dbo.KioskoAreas(AreaId);
GO

CREATE INDEX IX_KioskoAreas_Estado
ON dbo.KioskoAreas(Estado);
GO

ALTER TABLE dbo.KioskoAreas
ADD CONSTRAINT UQ_KioskoAreas
UNIQUE
(
    KioskoId,
    AreaId
);
GO

ALTER TABLE dbo.KioskoAreas
DROP CONSTRAINT DF_KioskoAreas_Estado;

ALTER TABLE dbo.KioskoAreas
DROP CONSTRAINT DF_KioskoAreas_FechaReg;

ALTER TABLE dbo.KioskoAreas
DROP CONSTRAINT DF_KioskoAreas_Ride;
GO

EXEC sp_help 'dbo.KioskoAreas';
GO

EXEC sp_helpconstraint 'dbo.KioskoAreas';
GO

EXEC sp_helpindex 'dbo.KioskoAreas';
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
WHERE OBJECT_NAME(fk.parent_object_id)='KioskoAreas';
GO

SELECT *
FROM dbo.KioskoAreas;
GO