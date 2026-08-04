IF OBJECT_ID('dbo.Roles','U') IS NOT NULL
    DROP TABLE dbo.Roles;
GO

CREATE TABLE dbo.Roles
(
    RolId INT IDENTITY(1,1) NOT NULL,

    Descripcion NVARCHAR(100) NOT NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_Roles_Estado
        DEFAULT(1),

    FechaReg DATETIME2 NOT NULL
        CONSTRAINT DF_Roles_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Roles_Ride
        DEFAULT(NEWID()),

    Logs INT NOT NULL
        DEFAULT(0),

    CONSTRAINT PK_Roles
        PRIMARY KEY CLUSTERED(RolId)
);
GO

ALTER TABLE dbo.Roles
ADD CONSTRAINT UQ_Roles_Descripcion
UNIQUE(Descripcion);
GO

CREATE INDEX IX_Roles_Descripcion
ON dbo.Roles(Descripcion);
GO

SELECT *
FROM dbo.Roles;
GO

EXEC sp_help 'dbo.Roles';
GO

EXEC sp_helpconstraint 'dbo.Roles';
GO

EXEC sp_helpindex 'dbo.Roles';
GO

SELECT
COLUMNPROPERTY(
OBJECT_ID('dbo.Roles'),
'RolId',
'IsIdentity')
AS EsIdentity;
GO  

INSERT INTO dbo.Roles
(
Descripcion
)
VALUES
('Administrador'),
('Supervisor'),
('Operador'),
('Recepcion'),
('Consulta');
GO