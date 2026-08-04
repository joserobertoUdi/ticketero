PRINT '========== TABLAS ==========';

SELECT
    t.name AS Tabla,
    t.create_date,
    t.modify_date
FROM sys.tables t
ORDER BY t.name;
GO

PRINT '========== COLUMNAS ==========';

SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
ORDER BY
TABLE_NAME,
ORDINAL_POSITION;
GO

PRINT '========== PRIMARY KEYS ==========';

SELECT
KU.TABLE_NAME,
KU.COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KU
ON TC.CONSTRAINT_NAME=KU.CONSTRAINT_NAME
WHERE TC.CONSTRAINT_TYPE='PRIMARY KEY'
ORDER BY KU.TABLE_NAME;
GO

PRINT '========== FOREIGN KEYS ==========';

SELECT

fk.name AS ForeignKey,

OBJECT_NAME(fk.parent_object_id) TablaHija,

COL_NAME(fkc.parent_object_id,fkc.parent_column_id) Columna,

OBJECT_NAME(fk.referenced_object_id) TablaPadre

FROM sys.foreign_keys fk

INNER JOIN sys.foreign_key_columns fkc

ON fk.object_id=fkc.constraint_object_id

ORDER BY TablaHija;
GO

PRINT '========== DEFAULTS ==========';

SELECT

OBJECT_NAME(parent_object_id) Tabla,

name ConstraintName

FROM sys.default_constraints

ORDER BY Tabla;
GO

PRINT '========== CHECK ==========';

SELECT

OBJECT_NAME(parent_object_id) Tabla,

name ConstraintName

FROM sys.check_constraints

ORDER BY Tabla;
GO

PRINT '========== UNIQUE ==========';

SELECT

OBJECT_NAME(parent_object_id) Tabla,

name ConstraintName

FROM sys.key_constraints

WHERE type='UQ'

ORDER BY Tabla;
GO  

PRINT '========== INDICES ==========';

SELECT

OBJECT_NAME(object_id) Tabla,

name Indice,

type_desc

FROM sys.indexes

WHERE index_id>0

ORDER BY Tabla;
GO

PRINT '========== IDENTITY ==========';

SELECT

OBJECT_NAME(object_id) Tabla,

name Columna,

is_identity

FROM sys.columns

WHERE is_identity=1;
GO  

PRINT '========== REGISTROS ==========';

EXEC sp_MSforeachtable
'
SELECT
''?''
AS Tabla,
COUNT(*)
AS Registros
FROM ?;
';
GO

PRINT '========== ESPACIO ==========';

EXEC sp_MSforeachtable
'
EXEC sp_spaceused ''?'';
';
GO

PRINT '========== OBJETOS DUPLICADOS ==========';

SELECT

name,

type_desc

FROM sys.objects

ORDER BY name;
GO

PRINT '========== DEPENDENCIAS ==========';

SELECT

OBJECT_NAME(referencing_id) Objeto,

referenced_entity_name Referencia

FROM sys.sql_expression_dependencies

ORDER BY Objeto;
GO

PRINT '========== TABLAS SIN PK ==========';

SELECT

t.name

FROM sys.tables t

WHERE NOT EXISTS
(
SELECT *

FROM sys.key_constraints kc

WHERE kc.parent_object_id=t.object_id

AND kc.type='PK'
);
GO

PRINT '========== TABLAS SIN FK ==========';

SELECT

t.name

FROM sys.tables t

WHERE NOT EXISTS
(
SELECT *

FROM sys.foreign_keys fk

WHERE fk.parent_object_id=t.object_id
)

ORDER BY t.name;
GO