/*==============================================================
    BASE DE DATOS : DBTicketero
    TABLA         : Tickets
    DESCRIPCIÓN   : Tabla principal del sistema.
                    Almacena la información general de cada ticket.
==============================================================*/

CREATE TABLE dbo.Tickets
(
    -----------------------------------------------------------------
    -- Llave primaria
    -----------------------------------------------------------------
    TicketId INT IDENTITY(1,1) NOT NULL,

    -----------------------------------------------------------------
    -- Número visible del ticket
    -----------------------------------------------------------------
    NumeroTicket NVARCHAR(30) NOT NULL,

    -----------------------------------------------------------------
    -- Catálogos
    -----------------------------------------------------------------
    ServicioId INT NOT NULL,
    TipoTicketId INT NOT NULL,
    PrioridadId INT NOT NULL,

    -----------------------------------------------------------------
    -- Estado actual
    -----------------------------------------------------------------
    EstadoTicketId INT NOT NULL,

    -----------------------------------------------------------------
    -- Área donde actualmente se encuentra
    -----------------------------------------------------------------
    AreaActualId INT NOT NULL,

    -----------------------------------------------------------------
    -- Descripción inicial
    -----------------------------------------------------------------
    Descripcion NVARCHAR(500) NOT NULL,

    -----------------------------------------------------------------
    -- Fecha de creación
    -----------------------------------------------------------------
    FechaCreacion DATETIME2(3) NOT NULL,

    -----------------------------------------------------------------
    -- Fecha de cierre
    -----------------------------------------------------------------
    FechaCierre DATETIME2(3) NULL,

    -----------------------------------------------------------------
    -- Baja lógica
    -----------------------------------------------------------------
    Estado BIT NOT NULL,

    -----------------------------------------------------------------
    -- Identificador único
    -----------------------------------------------------------------
    Ride UNIQUEIDENTIFIER NOT NULL,

    -------------------------------------------------------------
    -- PRIMARY KEY
    -------------------------------------------------------------
    CONSTRAINT PK_Tickets
        PRIMARY KEY CLUSTERED
    (
        TicketId
    )
);
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT DF_Tickets_FechaCreacion
DEFAULT (SYSDATETIME())
FOR FechaCreacion;
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT DF_Tickets_Estado
DEFAULT (1)
FOR Estado;
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT DF_Tickets_Ride
DEFAULT (NEWID())
FOR Ride;
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_Servicios
FOREIGN KEY
(
    ServicioId
)
REFERENCES dbo.Servicios
(
    ServicioId
);
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_TiposTicket
FOREIGN KEY
(
    TipoTicketId
)
REFERENCES dbo.TiposTicket
(
    TipoTicketId
);
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_Prioridades
FOREIGN KEY
(
    PrioridadId
)
REFERENCES dbo.Prioridades
(
    PrioridadId
);
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_EstadosTicket
FOREIGN KEY
(
    EstadoTicketId
)
REFERENCES dbo.EstadosTicket
(
    EstadoTicketId
);
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_Areas
FOREIGN KEY
(
    AreaActualId
)
REFERENCES dbo.Areas
(
    AreaId
);
GO

ALTER TABLE dbo.Tickets
ADD CONSTRAINT UQ_Tickets_NumeroTicket
UNIQUE
(
    NumeroTicket
);
GO

CREATE NONCLUSTERED INDEX IX_Tickets_Estado
ON dbo.Tickets
(
    EstadoTicketId
);
GO

CREATE NONCLUSTERED INDEX IX_Tickets_Area
ON dbo.Tickets
(
    AreaActualId
);
GO

CREATE NONCLUSTERED INDEX IX_Tickets_Servicio
ON dbo.Tickets
(
    ServicioId
);
GO

CREATE NONCLUSTERED INDEX IX_Tickets_Fecha
ON dbo.Tickets
(
    FechaCreacion
);
GO

CREATE NONCLUSTERED INDEX IX_Tickets_Prioridad
ON dbo.Tickets
(
    PrioridadId
);
GO

INSERT INTO dbo.Tickets
(
    NumeroTicket,
    ServicioId,
    TipoTicketId,
    PrioridadId,
    EstadoTicketId,
    AreaActualId,
    Descripcion
)
VALUES
(
    'SIS-000001',
    1,
    1,
    2,
    1,
    1,
    'Prueba del sistema'
);
GO

SELECT *
FROM dbo.Tickets;
GO

EXEC sp_help 'dbo.Tickets';
GO

EXEC sp_helpconstraint 'dbo.Tickets';
GO

EXEC sp_helpindex 'dbo.Tickets';
GO

SELECT
COLUMNPROPERTY
(
    OBJECT_ID('dbo.Tickets'),
    'TicketId',
    'IsIdentity'
) AS EsIdentity;
GO

EXEC sp_fkeys 'Tickets';
GO