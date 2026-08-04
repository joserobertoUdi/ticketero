CREATE TABLE dbo.Configuraciones
(
    ConfiguracionId INT IDENTITY(1,1) NOT NULL,

    Clave NVARCHAR(100) NOT NULL,

    Valor NVARCHAR(500) NOT NULL,

    Descripcion NVARCHAR(300) NULL,

    Estado BIT NOT NULL
        CONSTRAINT DF_Configuraciones_Estado DEFAULT(1),

    FechaReg DATETIME2 NOT NULL
        CONSTRAINT DF_Configuraciones_FechaReg DEFAULT(SYSDATETIME()),

    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_Configuraciones_Ride DEFAULT(NEWID()),

    CONSTRAINT PK_Configuraciones
        PRIMARY KEY(ConfiguracionId),

    CONSTRAINT UQ_Configuraciones_Clave
        UNIQUE(Clave)
);
GO

CREATE INDEX IX_Configuraciones_Estado
ON dbo.Configuraciones(Estado);
GO

CREATE INDEX IX_Configuraciones_Clave
ON dbo.Configuraciones(Clave);
GO

INSERT INTO dbo.Configuraciones
(
    Clave,
    Valor,
    Descripcion
)
VALUES
('VersionSistema','1.0.0','Versión instalada'),

('TiempoEspera','300','Tiempo máximo de espera'),

('TiempoRecall','60','Tiempo para volver a llamar'),

('MostrarPublicidad','1','Habilita multimedia'),

('universidadUdi','Innovando en conocimeintos','Caja sector sur');
GO