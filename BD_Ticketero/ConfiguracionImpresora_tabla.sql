CREATE TABLE dbo.ConfiguracionImpresora
(
    ConfiguracionImpresoraId INT IDENTITY(1,1) NOT NULL,

    KioskoId INT NOT NULL,

    NombreImpresora NVARCHAR(150) NOT NULL,

    Puerto NVARCHAR(50) NULL,

    DireccionIP NVARCHAR(50) NULL,

    TipoConexion NVARCHAR(20) NOT NULL,

    AnchoPapelMM INT NOT NULL,

    Copias TINYINT NOT NULL,

    ImpresionAutomatica BIT NOT NULL,

    Estado BIT NOT NULL,

    FechaReg DATETIME2 NOT NULL,

    Ride UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT PK_ConfiguracionImpresora
        PRIMARY KEY CLUSTERED
    (
        ConfiguracionImpresoraId
    )
);
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT FK_ConfiguracionImpresora_Kioskos
FOREIGN KEY (KioskoId)
REFERENCES dbo.Kiosko(KioskoId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT DF_ConfiguracionImpresora_Estado
DEFAULT(1) FOR Estado;
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT DF_ConfiguracionImpresora_FechaReg
DEFAULT(SYSDATETIME()) FOR FechaReg;
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT DF_ConfiguracionImpresora_Ride
DEFAULT(NEWID()) FOR Ride;
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT DF_ConfiguracionImpresora_Copias
DEFAULT(1) FOR Copias;
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT DF_ConfiguracionImpresora_ImpresionAutomatica
DEFAULT(1) FOR ImpresionAutomatica;
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT CK_ConfiguracionImpresora_Nombre
CHECK (LEN(LTRIM(RTRIM(NombreImpresora)))>0);
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT CK_ConfiguracionImpresora_TipoConexion
CHECK
(
    TipoConexion IN
    (
        'USB',
        'RED'
    )
);
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT CK_ConfiguracionImpresora_Ancho
CHECK
(
    AnchoPapelMM IN (58,80)
);
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT CK_ConfiguracionImpresora_Copias
CHECK
(
    Copias>=1
    AND Copias<=5
);
GO

CREATE INDEX IX_ConfiguracionImpresora_Kiosko
ON dbo.ConfiguracionImpresora(KioskoId);
GO

CREATE INDEX IX_ConfiguracionImpresora_Estado
ON dbo.ConfiguracionImpresora(Estado);
GO

CREATE INDEX IX_ConfiguracionImpresora_Nombre
ON dbo.ConfiguracionImpresora(NombreImpresora);
GO

ALTER TABLE dbo.ConfiguracionImpresora
ADD CONSTRAINT UQ_ConfiguracionImpresora
UNIQUE
(
    KioskoId,
    NombreImpresora
);
GO

EXEC sp_help 'dbo.ConfiguracionImpresora';
GO

EXEC sp_helpconstraint 'dbo.ConfiguracionImpresora';
GO

EXEC sp_helpindex 'dbo.ConfiguracionImpresora';
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
WHERE OBJECT_NAME(fk.parent_object_id) = 'ConfiguracionImpresora';
GO

SELECT *
FROM dbo.ConfiguracionImpresora;
GO