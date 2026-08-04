/*==============================================================
    BASE DE DATOS : DBTicketero
    TABLA         : EstadosTicket
    DESCRIPCIÓN   : Catálogo de estados que puede tener un ticket.
==============================================================*/

IF OBJECT_ID('dbo.EstadosTicket','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.EstadosTicket;
END;
GO

CREATE TABLE dbo.EstadosTicket
(
    EstadoTicketId INT IDENTITY(1,1) NOT NULL,

    Descripcion NVARCHAR(100) NOT NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_EstadosTicket_Estado
        DEFAULT(1),

    FechaReg DATETIME2(3) NOT NULL
        CONSTRAINT DF_EstadosTicket_FechaReg
        DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_EstadosTicket_Ride
        DEFAULT(NEWID()),

    CONSTRAINT PK_EstadosTicket
        PRIMARY KEY CLUSTERED
        (
            EstadoTicketId
        )
);
GO

ALTER TABLE dbo.EstadosTicket
ADD CONSTRAINT UQ_EstadosTicket_Descripcion
UNIQUE (Descripcion);
GO

CREATE NONCLUSTERED INDEX IX_EstadosTicket_Descripcion
ON dbo.EstadosTicket(Descripcion);
GO

INSERT INTO dbo.EstadosTicket
(
    Descripcion
)
VALUES
('Nuevo'),
('Asignado'),
('En Proceso'),
('En Espera'),
('Resuelto'),
('Cerrado'),
('Cancelado');
GO

SELECT *
FROM dbo.EstadosTicket;
GO

EXEC sp_help 'dbo.EstadosTicket';
GO

EXEC sp_helpconstraint 'dbo.EstadosTicket';
GO

EXEC sp_helpindex 'dbo.EstadosTicket';
GO

SELECT
COLUMNPROPERTY
(
    OBJECT_ID('dbo.EstadosTicket'),
    'EstadoTicketId',
    'IsIdentity'
) AS EsIdentity;
GO