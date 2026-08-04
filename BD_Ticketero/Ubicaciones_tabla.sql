/*****************************************************************
 TABLA: Ubicaciones
 DESCRIPCIÓN:
 Representa una ubicación física donde puede instalarse uno o más
 kioskos dentro de la institución.
******************************************************************/

CREATE TABLE dbo.Ubicaciones
(
    UbicacionId INT IDENTITY(1,1) NOT NULL,

    Descripcion NVARCHAR(100) NOT NULL,

    Edificio NVARCHAR(100) NOT NULL,

    Piso NVARCHAR(50) NOT NULL,

    Sector NVARCHAR(100) NOT NULL,

    Referencia NVARCHAR(250) NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_Ubicaciones_Estado
        DEFAULT(1),

    FechaReg DATETIME2 NOT NULL
        CONSTRAINT DF_Ubicaciones_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Ubicaciones_Ride
        DEFAULT(NEWID()),

    CONSTRAINT PK_Ubicaciones
        PRIMARY KEY CLUSTERED (UbicacionId),

    CONSTRAINT UQ_Ubicaciones
        UNIQUE
        (
            Edificio,
            Piso,
            Sector,
            Descripcion
        )
);
GO

CREATE INDEX IX_Ubicaciones_Edificio
ON dbo.Ubicaciones(Edificio);
GO

CREATE INDEX IX_Ubicaciones_Piso
ON dbo.Ubicaciones(Piso);
GO

CREATE INDEX IX_Ubicaciones_Sector
ON dbo.Ubicaciones(Sector);
GO

CREATE INDEX IX_Ubicaciones_Estado
ON dbo.Ubicaciones(Estado);
GO

ALTER TABLE dbo.Ubicaciones
ADD CONSTRAINT CK_Ubicaciones_Descripcion
CHECK (LEN(LTRIM(RTRIM(Descripcion))) > 0);
GO

ALTER TABLE dbo.Ubicaciones
ADD CONSTRAINT CK_Ubicaciones_Edificio
CHECK (LEN(LTRIM(RTRIM(Edificio))) > 0);
GO

ALTER TABLE dbo.Ubicaciones
ADD CONSTRAINT CK_Ubicaciones_Piso
CHECK (LEN(LTRIM(RTRIM(Piso))) > 0);
GO

ALTER TABLE dbo.Ubicaciones
ADD CONSTRAINT CK_Ubicaciones_Sector
CHECK (LEN(LTRIM(RTRIM(Sector))) > 0);
GO

EXEC sp_helpconstraint 'dbo.Ubicaciones';
SELECT
name
FROM sys.default_constraints
WHERE parent_object_id=OBJECT_ID('dbo.Ubicaciones');

SELECT
name
FROM sys.check_constraints
WHERE parent_object_id=OBJECT_ID('dbo.Ubicaciones');

SELECT
i.name,
i.type_desc
FROM sys.indexes i
WHERE object_id=OBJECT_ID('dbo.Ubicaciones');

SELECT
name,
is_identity
FROM sys.columns
WHERE object_id=OBJECT_ID('dbo.Ubicaciones');

INSERT INTO dbo.Ubicaciones
(
Descripcion,
Edificio,
Piso,
Sector
)
VALUES
(
'Caja General',
'Principal',
'Planta Baja',
'Caja'
);