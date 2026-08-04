CREATE TABLE dbo.ConfiguracionRed
(
    ConfiguracionRedId INT IDENTITY(1,1) NOT NULL,

    KioskoId INT NOT NULL,

    TipoConexion NVARCHAR(15) NOT NULL,

    DHCP BIT NOT NULL,

    DireccionIP NVARCHAR(50) NULL,

    MascaraSubred NVARCHAR(50) NULL,

    Gateway NVARCHAR(50) NULL,

    DNSPrimario NVARCHAR(50) NULL,

    DNSSecundario NVARCHAR(50) NULL,

    Puerto INT NULL,

    Estado BIT NOT NULL,

    FechaReg DATETIME2 NOT NULL,

    Ride UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT PK_ConfiguracionRed
        PRIMARY KEY CLUSTERED (ConfiguracionRedId)
);
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT FK_ConfiguracionRed_Kioskos
FOREIGN KEY (KioskoId)
REFERENCES dbo.Kiosko(KioskoId)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT DF_ConfiguracionRed_DHCP
DEFAULT(1) FOR DHCP;
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT DF_ConfiguracionRed_Estado
DEFAULT(1) FOR Estado;
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT DF_ConfiguracionRed_FechaReg
DEFAULT(SYSDATETIME()) FOR FechaReg;
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT DF_ConfiguracionRed_Ride
DEFAULT(NEWID()) FOR Ride;
GO  

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT CK_ConfiguracionRed_TipoConexion
CHECK
(
    TipoConexion IN
    (
        'ETHERNET',
        'WIFI'
    )
);
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT CK_ConfiguracionRed_Puerto
CHECK
(
    Puerto IS NULL
    OR Puerto BETWEEN 1 AND 65535
);
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT CK_ConfiguracionRed_IP
CHECK
(
    DireccionIP IS NULL
    OR LEN(LTRIM(RTRIM(DireccionIP)))>0
);
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT CK_ConfiguracionRed_Mascara
CHECK
(
    MascaraSubred IS NULL
    OR LEN(LTRIM(RTRIM(MascaraSubred)))>0
);
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT CK_ConfiguracionRed_Gateway
CHECK
(
    Gateway IS NULL
    OR LEN(LTRIM(RTRIM(Gateway)))>0
);
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT CK_ConfiguracionRed_DNS1
CHECK
(
    DNSPrimario IS NULL
    OR LEN(LTRIM(RTRIM(DNSPrimario)))>0
);
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT CK_ConfiguracionRed_DNS2
CHECK
(
    DNSSecundario IS NULL
    OR LEN(LTRIM(RTRIM(DNSSecundario)))>0
);
GO

CREATE INDEX IX_ConfiguracionRed_Kiosko
ON dbo.ConfiguracionRed(KioskoId);
GO

CREATE INDEX IX_ConfiguracionRed_Estado
ON dbo.ConfiguracionRed(Estado);
GO

CREATE INDEX IX_ConfiguracionRed_TipoConexion
ON dbo.ConfiguracionRed(TipoConexion);
GO

CREATE INDEX IX_ConfiguracionRed_DHCP
ON dbo.ConfiguracionRed(DHCP);
GO

ALTER TABLE dbo.ConfiguracionRed
ADD CONSTRAINT UQ_ConfiguracionRed
UNIQUE
(
    KioskoId,
    DireccionIP
);
GO

EXEC sp_help 'dbo.ConfiguracionRed';
GO

EXEC sp_helpconstraint 'dbo.ConfiguracionRed';
GO

EXEC sp_helpindex 'dbo.ConfiguracionRed';
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
WHERE OBJECT_NAME(fk.parent_object_id) = 'ConfiguracionRed';
GO