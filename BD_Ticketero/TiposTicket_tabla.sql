/*==============================================================
    BASE DE DATOS : DBTicketero
    TABLA         : TiposTicket
    DESCRIPCIÓN   : Catálogo de tipos de ticket
==============================================================*/

IF OBJECT_ID('dbo.TiposTicket','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.TiposTicket;
END;
GO

CREATE TABLE dbo.TiposTicket
(
    TipoTicketId INT IDENTITY(1,1) NOT NULL,

    Descripcion NVARCHAR(100) NOT NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_TiposTicket_Estado
        DEFAULT(1),

    FechaReg DATETIME2(3) NOT NULL
        CONSTRAINT DF_TiposTicket_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_TiposTicket_Ride
        DEFAULT(NEWID()),

    CONSTRAINT PK_TiposTicket
        PRIMARY KEY CLUSTERED
        (
            TipoTicketId
        )
);
GO

ALTER TABLE dbo.TiposTicket
ADD CONSTRAINT UQ_TiposTicket_Descripcion
UNIQUE(Descripcion);
GO

CREATE NONCLUSTERED INDEX IX_TiposTicket_Descripcion
ON dbo.TiposTicket
(
    Descripcion
);
GO

INSERT INTO dbo.TiposTicket
(
    Descripcion
)
VALUES
('Incidente'),
('Solicitud'),
('Problema'),
('Cambio'),
('Consulta');
GO

SELECT *
FROM dbo.TiposTicket;
GO

EXEC sp_help 'dbo.TiposTicket';
GO

EXEC sp_helpconstraint 'dbo.TiposTicket';
GO

EXEC sp_helpindex 'dbo.TiposTicket';
GO

SELECT
    COLUMNPROPERTY
    (
        OBJECT_ID('dbo.TiposTicket'),
        'TipoTicketId',
        'IsIdentity'
    ) AS EsIdentity;
GO