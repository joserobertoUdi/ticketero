/*==============================================================
    BASE DE DATOS : DBTicketero
    TABLA         : AreasFase
    AUTOR         : José Roberto Quiroga Salvador
    DESCRIPCIÓN   : Catálogo de áreas de la empresa.
                    Cada área puede tener uno o varios usuarios
                    y atender uno o varios tickets.
==============================================================*/

USE DBTicketero;
GO

/*==============================================================
    ELIMINAR TABLA SI EXISTE
==============================================================*/

IF OBJECT_ID('dbo.AreasFase', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.AreasFase;
END
GO

/*==============================================================
    CREACIÓN DE LA TABLA
==============================================================*/

CREATE TABLE dbo.AreasFase
(
    /*----------------------------------------------------------
        Identificador único del área
    ----------------------------------------------------------*/
    AreaId INT IDENTITY(1,1) NOT NULL,

    /*----------------------------------------------------------
        Nombre del área
    ----------------------------------------------------------*/
    Descripcion NVARCHAR(100) NOT NULL,

    /*----------------------------------------------------------
        Prefijo utilizado para códigos y reportes
    ----------------------------------------------------------*/
    Prefijo NVARCHAR(100) NOT NULL,

    /*----------------------------------------------------------
        Imagen o icono del área
        (Más adelante evaluaremos almacenar solo la ruta)
    ----------------------------------------------------------*/
    Icono IMAGE NULL,

    /*----------------------------------------------------------
        Estado lógico
        1 = Activo
        0 = Inactivo
    ----------------------------------------------------------*/
    Estado BIT NOT NULL
        CONSTRAINT DF_AreasFase_Estado
        DEFAULT (1),

    /*----------------------------------------------------------
        Fecha de creación
    ----------------------------------------------------------*/
    FechaReg DATETIME2(3) NOT NULL
        CONSTRAINT DF_AreasFase_FechaReg
        DEFAULT (SYSDATETIME()),

    /*----------------------------------------------------------
        Identificador global para auditoría
    ----------------------------------------------------------*/
    Ride UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_AreasFase_Ride
        DEFAULT (NEWID()),

    /*----------------------------------------------------------
        Clave Primaria
    ----------------------------------------------------------*/
    CONSTRAINT PK_AreasFase
        PRIMARY KEY CLUSTERED
        (
            AreaId ASC
        ),

    /*----------------------------------------------------------
        No permite nombres duplicados
    ----------------------------------------------------------*/
    CONSTRAINT UQ_AreasFase_Descripcion
        UNIQUE NONCLUSTERED
        (
            Descripcion ASC
        ),

    /*----------------------------------------------------------
        No permite prefijos duplicados
    ----------------------------------------------------------*/
    CONSTRAINT UQ_AreasFase_Prefijo
        UNIQUE NONCLUSTERED
        (
            Prefijo ASC
        )
);
GO

/*==============================================================
    ÍNDICE PARA CONSULTAS POR ESTADO
==============================================================*/

CREATE NONCLUSTERED INDEX IX_AreasFase_Estado
ON dbo.AreasFase (Estado);
GO

/*==============================================================
    PRUEBA DE INSERCIÓN
==============================================================*/

INSERT INTO dbo.AreasFase
(
    Descripcion,
    Prefijo,
    Icono
)
VALUES
(
    'Sistemas',
    'SIS',
    NULL
);
GO

/*==============================================================
    CONSULTA DE VALIDACIÓN
==============================================================*/

SELECT
    AreaId,
    Descripcion,
    Prefijo,
    Estado,
    FechaReg,
    Ride
FROM dbo.AreasFase;
GO

/*==============================================================
    VALIDAR CONFIGURACIÓN
==============================================================*/

EXEC sp_help 'dbo.AreasFase';
GO

EXEC sp_helpconstraint 'dbo.AreasFase';
GO

SELECT
    COLUMNPROPERTY
    (
        OBJECT_ID('dbo.AreasFase'),
        'AreaId',
        'IsIdentity'
    ) AS EsIdentity;
GO












PRINT '==pendiente revision=='


USE DBTicketero;
GO

EXEC sp_help 'dbo.AreasFase';
GO

SELECT 
  COLUMN_NAME,
  DATA_TYPE,
  CHARACTER_MAXIMUM_LENGTH,
  IS_NULLABLE

  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_NAME='AreasFase'
  ORDER BY ORDINAL_POSITION;

  SELECT *
  FROM dbo.AreasFase

  SELECT 
  COLUMNPROPERTY(
   OBJECT_ID('dbo.AreasFase'),
   'AreasId',
   'Isdentity'
   ) AS EsIdentity;
   
   ALTER TABLE dbo.AreasFase
      ADD CONSTRAINT UQ_AreasFase_Descripcion
      UNIQUE (Descripcion); 

     EXEC sp_helpconstraint 'dbo.AreasFase';
        GO

 ALTER TABLE dbo.AreasFase
 ADD CONSTRAINT DF_AreasFase_Estado
 DEFAULT(1) FOR Estado;

 /*==============================================================
    TABLA : AreasFase
    REGLA : No puede existir dos áreas con el mismo nombre
==============================================================*/

ALTER TABLE dbo.AreasFase
ADD CONSTRAINT UQ_AreasFase_Descripcion
UNIQUE (Descripcion);

/*==============================================================
    REGLA : El prefijo también debe ser único
==============================================================*/

ALTER TABLE dbo.AreasFase
ADD CONSTRAINT UQ_AreasFase_Prefijo
UNIQUE (Prefijo);
/*==============================================================
    REGLA : Toda área nueva inicia activa
==============================================================*/

ALTER TABLE dbo.AreasFase
ADD CONSTRAINT DF_AreasFase_Estado
DEFAULT (1) FOR Estado;
/*==============================================================
    REGLA : Registrar automáticamente la fecha de creación
==============================================================*/

ALTER TABLE dbo.AreasFase
ADD CONSTRAINT DF_AreasFase_FechaReg
DEFAULT (SYSDATETIME()) FOR FechaReg;
/*==============================================================
    REGLA : Generar automáticamente un identificador global
==============================================================*/

ALTER TABLE dbo.AreasFase
ADD CONSTRAINT DF_AreasFase_Ride
DEFAULT (NEWID()) FOR Ride;

INSERT INTO dbo.AreasFase
(
    Descripcion,
    Prefijo,
    Icono
)
VALUES
(
    'Sistemas',
    'SIS',
    NULL
);

INSERT INTO dbo.AreasFase
(
    Descripcion,
    Prefijo,
    Icono
)
VALUES
(
    'Sistemas',
    'SIS2',
    NULL
);

EXEC sp_help 'dbo.Usuarios';

/*============================================================
    COLUMNAS
============================================================*/

SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='Usuarios'
ORDER BY ORDINAL_POSITION;

EXEC sp_helpconstraint 'dbo.Usuarios';
GO
EXEC sp_helpconstraint 'dbo.AreasFase';
GO
SELECT
    COLUMNPROPERTY
    (
        OBJECT_ID('dbo.AreasFase'),
        'AreaId',
        'IsIdentity'
    ) AS EsIdentity;


