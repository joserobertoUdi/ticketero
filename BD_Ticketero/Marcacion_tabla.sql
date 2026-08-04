/*==============================================================
    BASE DE DATOS : DBTicketero
    TABLA         : Marcacion
    DESCRIPCIÓN   : Registro histórico de todos los llamados
                    realizados a un ticket.
==============================================================*/

IF OBJECT_ID('dbo.Marcacion','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Marcacion;
END
GO

CREATE TABLE dbo.Marcacion
(
    ------------------------------------------------------------
    -- Llave primaria
    ------------------------------------------------------------
    MarcacionId INT IDENTITY(1,1) NOT NULL,

    ------------------------------------------------------------
    -- Ticket llamado
    ------------------------------------------------------------
    TicketId INT NOT NULL,

    ------------------------------------------------------------
    -- Usuario que realizó el llamado
    ------------------------------------------------------------
    UsuarioId INT NOT NULL,

    ------------------------------------------------------------
    -- Kiosco o caja
    ------------------------------------------------------------
    KioskoId INT NOT NULL,

    ------------------------------------------------------------
    -- Número de llamado
    ------------------------------------------------------------
    NumeroLlamado TINYINT NOT NULL,

    ------------------------------------------------------------
    -- Fecha y hora del llamado
    ------------------------------------------------------------
    FechaMarcacion DATETIME2(3) NOT NULL,

    ------------------------------------------------------------
    -- El cliente respondió al llamado
    ------------------------------------------------------------
    Respondio BIT NOT NULL,

    ------------------------------------------------------------
    -- Estado lógico
    ------------------------------------------------------------
    Estado BIT NOT NULL,

    ------------------------------------------------------------
    -- Auditoría
    ------------------------------------------------------------
    FechaReg DATETIME2(3) NOT NULL,

    Ride UNIQUEIDENTIFIER NOT NULL,

    ------------------------------------------------------------
    -- Primary Key
    ------------------------------------------------------------
    CONSTRAINT PK_Marcacion
    PRIMARY KEY CLUSTERED
    (
        MarcacionId
    )
);
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT DF_Marcacion_Respondio
DEFAULT(0)
FOR Respondio;
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT DF_Marcacion_Estado
DEFAULT(1)
FOR Estado;
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT DF_Marcacion_FechaReg
DEFAULT(SYSDATETIME())
FOR FechaReg;
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT DF_Marcacion_Ride
DEFAULT(NEWID())
FOR Ride;
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT DF_Marcacion_FechaMarcacion
DEFAULT(SYSDATETIME())
FOR FechaMarcacion;
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT CK_Marcacion_NumeroLlamado
CHECK
(
    NumeroLlamado >= 1
);
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT FK_Marcacion_Usuarios
FOREIGN KEY
(
    UsuarioId
)
REFERENCES dbo.Usuarios
(
    UsuarioId
);
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT FK_Marcacion_Kiosko
FOREIGN KEY
(
    KioskoId
)
REFERENCES dbo.Kiosko
(
    KioskoId
);
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT FK_Marcacion_Tickets
FOREIGN KEY
(
    TicketId
)
REFERENCES dbo.Tickets
(
    TicketId
);
GO

ALTER TABLE dbo.Marcacion
ADD CONSTRAINT UQ_Marcacion_Ticket_Numero
UNIQUE
(
    TicketId,
    NumeroLlamado
);
GO

CREATE NONCLUSTERED INDEX IX_Marcacion_Ticket
ON dbo.Marcacion
(
    TicketId
);
GO

CREATE NONCLUSTERED INDEX IX_Marcacion_Usuario
ON dbo.Marcacion
(
    UsuarioId
);
GO

CREATE NONCLUSTERED INDEX IX_Marcacion_Kiosko
ON dbo.Marcacion
(
    KioskoId
);
GO

CREATE NONCLUSTERED INDEX IX_Marcacion_Fecha
ON dbo.Marcacion
(
    FechaMarcacion
);
GO

CREATE NONCLUSTERED INDEX IX_Marcacion_Usuario_Fecha
ON dbo.Marcacion
(
    UsuarioId,
    FechaMarcacion
);
GO

EXEC sp_help 'dbo.Marcacion';
GO
EXEC sp_helpconstraint 'dbo.Marcacion';
GO
EXEC sp_helpindex 'dbo.Marcacion';
GO
SELECT
    COLUMNPROPERTY(
        OBJECT_ID('dbo.Marcacion'),
        'MarcacionId',
        'IsIdentity'
    ) AS EsIdentity;
GO
SELECT *
FROM dbo.Marcacion;
GO