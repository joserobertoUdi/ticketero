IF OBJECT_ID('dbo.SesionOperador','U') IS NOT NULL
BEGIN
    DROP TABLE dbo.SesionOperador;
END
GO

CREATE TABLE dbo.SesionOperador
(
    SesionOperadorId INT IDENTITY(1,1) NOT NULL,

    UsuarioId INT NOT NULL,

    PuestoId INT NOT NULL,

    FechaInicio DATETIME2(7) NOT NULL,

    FechaFin DATETIME2(7) NULL,

    Estado BIT NOT NULL,

    FechaReg DATETIME2(7) NOT NULL,

    Ride UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT PK_SesionOperador
        PRIMARY KEY CLUSTERED
        (
            SesionOperadorId
        )
);
GO

ALTER TABLE dbo.SesionOperador
ADD CONSTRAINT DF_SesionOperador_FechaInicio
DEFAULT(SYSDATETIME()) FOR FechaInicio;
GO

ALTER TABLE dbo.SesionOperador
ADD CONSTRAINT DF_SesionOperador_Estado
DEFAULT(1) FOR Estado;
GO

ALTER TABLE dbo.SesionOperador
ADD CONSTRAINT DF_SesionOperador_FechaReg
DEFAULT(SYSDATETIME()) FOR FechaReg;
GO

ALTER TABLE dbo.SesionOperador
ADD CONSTRAINT DF_SesionOperador_Ride
DEFAULT(NEWID()) FOR Ride;
GO

ALTER TABLE dbo.SesionOperador
ADD CONSTRAINT FK_SesionOperador_Usuarios
FOREIGN KEY
(
    UsuarioId
)
REFERENCES dbo.Usuarios
(
    UsuarioId
)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

ALTER TABLE dbo.SesionOperador
ADD CONSTRAINT FK_SesionOperador_Puestos
FOREIGN KEY
(
    PuestoId
)
REFERENCES dbo.Puestos
(
    PuestoId
)
ON DELETE NO ACTION
ON UPDATE NO ACTION;
GO

ALTER TABLE dbo.SesionOperador
ADD CONSTRAINT CK_SesionOperador_Fechas
CHECK
(
    FechaFin IS NULL
    OR FechaFin>=FechaInicio
);
GO

CREATE INDEX IX_SesionOperador_Usuario
ON dbo.SesionOperador
(
    UsuarioId
);
GO

CREATE INDEX IX_SesionOperador_Puesto
ON dbo.SesionOperador
(
    PuestoId
);
GO

CREATE INDEX IX_SesionOperador_FechaInicio
ON dbo.SesionOperador
(
    FechaInicio
);
GO

CREATE INDEX IX_SesionOperador_FechaFin
ON dbo.SesionOperador
(
    FechaFin
);
GO

CREATE INDEX IX_SesionOperador_Usuario_Fecha
ON dbo.SesionOperador
(
    UsuarioId,
    FechaInicio
);
GO

CREATE INDEX IX_SesionOperador_Puesto_Fecha
ON dbo.SesionOperador
(
    PuestoId,
    FechaInicio
);
GO

CREATE UNIQUE INDEX UX_SesionOperador_Usuario_Activa
ON dbo.SesionOperador
(
    UsuarioId
)
WHERE FechaFin IS NULL;
GO

CREATE UNIQUE INDEX UX_SesionOperador_Puesto_Activo
ON dbo.SesionOperador
(
    PuestoId
)
WHERE FechaFin IS NULL;
GO

INSERT INTO dbo.SesionOperador
(
    UsuarioId,
    PuestoId
)
VALUES
(1,1);
GO

EXEC sp_help 'SesionOperador';
GO
EXEC sp_helpconstraint 'SesionOperador';
GO
EXEC sp_helpindex 'SesionOperador';
GO
SELECT

    fk.name AS ForeignKey,

    OBJECT_NAME(fk.parent_object_id) TablaHija,

    c1.name Columna,

    OBJECT_NAME(fk.referenced_object_id) TablaPadre

FROM sys.foreign_keys fk

INNER JOIN sys.foreign_key_columns fkc

ON fk.object_id=fkc.constraint_object_id

INNER JOIN sys.columns c1

ON c1.object_id=fkc.parent_object_id
AND c1.column_id=fkc.parent_column_id

WHERE OBJECT_NAME(fk.parent_object_id)='SesionOperador';
GO

SELECT *
FROM dbo.SesionOperador;
GO