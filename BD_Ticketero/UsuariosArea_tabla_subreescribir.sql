EXEC sp_helpconstraint 'dbo.UsuariosAreaFase';
GO

/*==============================================================
    BASE DE DATOS : DBTicketero
    TABLA         : UsuariosAreaFase
    DESCRIPCIÓN   : Relaciona usuarios con las áreas donde
                    pueden trabajar.
==============================================================*/

USE DBTicketero;
GO

/*==============================================================
    ELIMINAR TABLA
==============================================================*/

IF OBJECT_ID('dbo.UsuariosAreaFase','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.UsuariosAreaFase;
END
GO

/*==============================================================
    CREACIÓN
==============================================================*/

CREATE TABLE dbo.UsuariosAreaFase
(

    /*-----------------------------------------
        Identificador técnico
    -----------------------------------------*/

    UsuarioAreaId INT IDENTITY(1,1) NOT NULL,

    /*-----------------------------------------
        Usuario
    -----------------------------------------*/

    UsuarioId INT NOT NULL,

    /*-----------------------------------------
        Área
    -----------------------------------------*/

    AreaId INT NOT NULL,

    /*-----------------------------------------
        Estado lógico
    -----------------------------------------*/

    Estado BIT NOT NULL
        CONSTRAINT DF_UsuariosAreaFase_Estado
        DEFAULT(1),

    /*-----------------------------------------
        Fecha de registro
    -----------------------------------------*/

    FechaReg DATETIME2(3) NOT NULL
        CONSTRAINT DF_UsuariosAreaFase_FechaReg
        DEFAULT(SYSDATETIME()),

    /*-----------------------------------------
        Identificador global
    -----------------------------------------*/

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_UsuariosAreaFase_Ride
        DEFAULT(NEWID()),

    /*-----------------------------------------
        PRIMARY KEY
    -----------------------------------------*/

    CONSTRAINT PK_UsuariosAreaFase
    PRIMARY KEY CLUSTERED
    (
        UsuarioAreaId ASC
    ),

    /*-----------------------------------------
        No permitir duplicados
    -----------------------------------------*/

    CONSTRAINT UQ_UsuariosAreaFase
    UNIQUE
    (
        UsuarioId,
        AreaId
    ),

    /*-----------------------------------------
        Usuario
    -----------------------------------------*/

    CONSTRAINT FK_UsuariosAreaFase_Usuarios
    FOREIGN KEY
    (
        UsuarioId
    )

    REFERENCES dbo.Usuarios
    (
        UsuarioId
    )

    ON UPDATE NO ACTION
    ON DELETE NO ACTION,

    /*-----------------------------------------
        Área
    -----------------------------------------*/

    CONSTRAINT FK_UsuariosAreaFase_Areas
    FOREIGN KEY
    (
        AreaId
    )

    REFERENCES dbo.Areas
    (
        AreaId
    )

    ON UPDATE NO ACTION
    ON DELETE NO ACTION

);
GO


SELECT
COLUMNPROPERTY
(
OBJECT_ID('dbo.UsuariosAreaFase'),
'UsuarioAreaId',
'IsIdentity'
) AS EsIdentity;
GO

EXEC sp_helpindex 'dbo.UsuariosAreaFase';
GO
SELECT *
FROM dbo.UsuariosAreaFase;

CREATE NONCLUSTERED INDEX IX_UsuariosAreaFase_Usuario
ON dbo.UsuariosAreaFase (UsuarioId);
GO

CREATE NONCLUSTERED INDEX IX_UsuariosAreaFase_Area
ON dbo.UsuariosAreaFase (AreaId);
GO

CREATE NONCLUSTERED INDEX IX_UsuariosAreaFase_Estado
ON dbo.UsuariosAreaFase (Estado);
GO