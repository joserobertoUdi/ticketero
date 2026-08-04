CREATE TABLE dbo.ConfiguracionMultimedia
(
    ConfiguracionMultimediaId INT IDENTITY(1,1) NOT NULL,

    KioskoId INT NOT NULL,

    NombreContenido NVARCHAR(200) NOT NULL,

    TipoContenido NVARCHAR(20) NOT NULL,

    RutaArchivo NVARCHAR(500) NOT NULL,

    DuracionSegundos INT NULL,

    Orden INT NOT NULL,

    Repetir BIT NOT NULL,

    Estado BIT NOT NULL,

    FechaReg DATETIME2 NOT NULL,

    Ride UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT PK_ConfiguracionMultimedia
        PRIMARY KEY (ConfiguracionMultimediaId)
);
GO
ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT FK_ConfiguracionMultimedia_Kioskos
FOREIGN KEY (KioskoId)
REFERENCES dbo.Kiosko(KioskoId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT DF_ConfiguracionMultimedia_Repetir
DEFAULT(1) FOR Repetir;
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT DF_ConfiguracionMultimedia_Estado
DEFAULT(1) FOR Estado;
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT DF_ConfiguracionMultimedia_FechaReg
DEFAULT(SYSDATETIME()) FOR FechaReg;
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT DF_ConfiguracionMultimedia_Ride
DEFAULT(NEWID()) FOR Ride;
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT DF_ConfiguracionMultimedia_Orden
DEFAULT(1) FOR Orden;
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT CK_ConfiguracionMultimedia_Nombre
CHECK(LEN(LTRIM(RTRIM(NombreContenido)))>0);
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT CK_ConfiguracionMultimedia_Ruta
CHECK(LEN(LTRIM(RTRIM(RutaArchivo)))>0);
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT CK_ConfiguracionMultimedia_Duracion
CHECK
(
DuracionSegundos IS NULL
OR DuracionSegundos>=0
);
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT CK_ConfiguracionMultimedia_Orden
CHECK(Orden>=1);
GO

ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT CK_ConfiguracionMultimedia_Tipo
CHECK
(
TipoContenido IN
(
'VIDEO',
'IMAGEN',
'PDF',
'HTML'
)
);
GO
CREATE INDEX IX_ConfiguracionMultimedia_Kiosko
ON dbo.ConfiguracionMultimedia(KioskoId);
GO

CREATE INDEX IX_ConfiguracionMultimedia_Estado
ON dbo.ConfiguracionMultimedia(Estado);
GO

CREATE INDEX IX_ConfiguracionMultimedia_Orden
ON dbo.ConfiguracionMultimedia(Orden);
GO

CREATE INDEX IX_ConfiguracionMultimedia_Tipo
ON dbo.ConfiguracionMultimedia(TipoContenido);
GO
ALTER TABLE dbo.ConfiguracionMultimedia
ADD CONSTRAINT UQ_ConfiguracionMultimedia
UNIQUE
(
KioskoId,
NombreContenido
);
GO

EXEC sp_help 'dbo.ConfiguracionMultimedia';
GO

EXEC sp_helpconstraint 'dbo.ConfiguracionMultimedia';
GO

EXEC sp_helpindex 'dbo.ConfiguracionMultimedia';
GO
SELECT
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TablaHija,
    c1.name AS Columna,
    OBJECT_NAME(fk.referenced_object_id) AS TablaPadre
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc
    ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c1
    ON c1.object_id = fkc.parent_object_id
   AND c1.column_id = fkc.parent_column_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'ConfiguracionMultimedia';
GO