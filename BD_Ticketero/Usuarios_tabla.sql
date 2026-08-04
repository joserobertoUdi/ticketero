IF OBJECT_ID('dbo.Usuarios','U') IS NOT NULL
    DROP TABLE dbo.Usuarios;
GO

CREATE TABLE dbo.Usuarios
(
    UsuarioId INT IDENTITY(1,1) NOT NULL,

    Nombre NVARCHAR(100) NOT NULL,

    Apellido NVARCHAR(100) NOT NULL,

    Correo NVARCHAR(150) NOT NULL,

    PasswordHash NVARCHAR(100) NOT NULL,

    CodigoSistema NVARCHAR(100) NOT NULL,

    RolId INT NOT NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_Usuarios_Estado
        DEFAULT(1),

    FechaReg DATETIME2 NOT NULL
        CONSTRAINT DF_Usuarios_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Usuarios_Ride
        DEFAULT(NEWID()),

    FechaUltimoAcceso DATETIME2 NULL,

    Logs INT NOT NULL
        DEFAULT(0),

    CONSTRAINT PK_Usuarios
        PRIMARY KEY CLUSTERED(UsuarioId)
);
GO

ALTER TABLE dbo.Usuarios
ADD CONSTRAINT FK_Usuarios_Roles
FOREIGN KEY(RolId)
REFERENCES dbo.Roles(RolId);
GO

ALTER TABLE dbo.Usuarios
ADD CONSTRAINT UQ_Usuarios_Correo
UNIQUE(Correo);
GO

ALTER TABLE dbo.Usuarios
ADD CONSTRAINT UQ_Usuarios_CodigoSistema
UNIQUE(CodigoSistema);
GO

CREATE INDEX IX_Usuarios_Rol
ON dbo.Usuarios(RolId);
GO

CREATE INDEX IX_Usuarios_Nombre
ON dbo.Usuarios(Nombre,Apellido);
GO

CREATE INDEX IX_Usuarios_Estado
ON dbo.Usuarios(Estado);
GO

SELECT *
FROM dbo.Usuarios;
GO

EXEC sp_help 'dbo.Usuarios';
GO

EXEC sp_helpconstraint 'dbo.Usuarios';
GO

EXEC sp_helpindex 'dbo.Usuarios';
GO

SELECT
COLUMNPROPERTY(
OBJECT_ID('dbo.Usuarios'),
'UsuarioId',
'IsIdentity')
AS EsIdentity;
GO

EXEC sp_fkeys 'Usuarios';
GO

INSERT INTO dbo.Usuarios
(
Nombre,
Apellido,
Correo,
PasswordHash,
CodigoSistema,
RolId
)
VALUES
(
'Administrador',
'Sistema',
'admin@empresa.com',
'HASH',
'ADM001',
1
);
GO