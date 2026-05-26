using System;
using System.Collections.Generic;
using System.Text;

namespace JEO3.SchemaEngine
{
    public static class SqlQueries
    {
        public const string SelectTop1000 = "SELECT TOP 1000";

        internal const string SchemaQuery = @"

	SELECT DISTINCT
		DB_NAME() AS DatabaseName,
		TB.TABLE_SCHEMA AS SchemaName,
		TB.TABLE_NAME AS TableShortName,
		(CASE LOWER(TB.TABLE_SCHEMA) WHEN 'dbo' THEN '' ELSE TB.TABLE_SCHEMA + '.' END) + TB.TABLE_NAME AS TableName,
		COL.COLUMN_NAME AS ColumnName,
		C.column_id AS OrdinalPosition,
		(CASE COL.IS_NULLABLE WHEN 'YES' THEN 1 ELSE 0 END) AS IsNullable,
		--COALESCE(I.is_primary_key, 0) AS IsIdentity,
		COALESCE(
			(SELECT TOP 1 IX.is_primary_key
			FROM sys.indexes IX 
				LEFT JOIN sys.index_columns ICX ON ICX.object_id = IX.object_id AND ICX.index_id = IX.index_id
			WHERE
				IX.object_id = C.object_id AND ICX.column_id = C.column_id
				), 0) AS IsIdentity,
		IIF(T.Name = 'int' AND LEN(COL.COLUMN_DEFAULT) > 0, 'bit', T.Name) AS DataType,
		(CASE WHEN COL.CHARACTER_MAXIMUM_LENGTH IS NULL THEN C.max_length ELSE COL.CHARACTER_MAXIMUM_LENGTH END) AS MaximumLength, 
		C.precision AS Precision,
		C.scale AS Scale,
		REPLACE(REPLACE(COALESCE(COL.COLUMN_DEFAULT, ''), '(', ''), ')', '') AS ColumnDefault,
		P.Rows AS Rows
	FROM sys.columns C
		INNER JOIN sys.objects O ON O.object_id = C.object_id
		INNER JOIN sys.types T ON C.user_type_id = T.user_type_id
		INNER JOIN sys.tables ST ON ST.object_id = C.object_id
		INNER JOIN  INFORMATION_SCHEMA.TABLES TB ON TB.TABLE_NAME = ST.name
		INNER JOIN information_schema.COLUMNS COL ON COL.table_name = O.Name AND COL.column_name = C.Name
		INNER JOIN 
		(
			SELECT 
				T1.name AS TableName, 
				MAX(P.Rows) AS Rows 
			FROM sys.tables T1 
				INNER JOIN sys.partitions P ON P.object_id = T1.object_id
			GROUP BY
				T1.name
		) P ON P.TableName = O.Name
	WHERE
		O.type = 'U'
	ORDER BY
		(CASE LOWER(TB.TABLE_SCHEMA) WHEN 'dbo' THEN '' ELSE TB.TABLE_SCHEMA + '.' END) + TB.TABLE_NAME,
		C.column_id";

        internal const string TableRelationsQuery = @"
SELECT DISTINCT
    DB_NAME() AS DatabaseName,
    SCHP.name AS PrimaryTableSchema,
    TBLP.name AS PrimaryTableShortName,
    SCHF.name AS ForeignTableSchema,
    TBLF.name AS ForeignTableShortName,
    (CASE LOWER(TBP.TABLE_SCHEMA) WHEN 'dbo' THEN '' ELSE TBP.TABLE_SCHEMA + '.' END) + TBP.TABLE_NAME AS PrimaryTableName,
    --LTRIM(SCHP.name + '.' + TBLP.name, '.') AS PrimaryTableName,
    COLP.name AS PrimaryColumnName,
    (CASE LOWER(TBF.TABLE_SCHEMA) WHEN 'dbo' THEN '' ELSE TBF.TABLE_SCHEMA + '.' END) + TBF.TABLE_NAME AS ForeignTableName,
    --LTRIM(SCHF.name + '.' + TBLF.name, '.') AS ForeignTableName,
    COLF.name AS ForeignColumnName,
    IIF(TP.Name = 'int' AND LEN(SCOLP.COLUMN_DEFAULT) > 0, 'bit', TP.Name) AS PrimaryColumnDataType,
    IIF(TF.Name = 'int' AND LEN(SCOLF.COLUMN_DEFAULT) > 0, 'bit', TF.Name) AS ForeignColumnDataType,
    FK.name AS FK_Name,
    COLF.is_nullable AS ForeignIsNullable,
    CASE 
        WHEN (SELECT COUNT(*) FROM sys.index_columns ic 
              JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
              WHERE i.is_primary_key = 1 AND ic.object_id = TBLP.object_id) > 1 
        THEN 1 --FK.name
        ELSE 0
    END AS IsCompositeKey
    
FROM sys.foreign_key_columns FKC
    INNER JOIN sys.foreign_keys FK ON FK.object_id = FKC.constraint_object_id
    
    INNER JOIN sys.columns COLF ON COLF.object_id = FKC.parent_object_id AND COLF.column_id = FKC.parent_column_id
    INNER JOIN sys.columns COLP ON COLP.object_id = FKC.referenced_object_id AND COLP.column_id = FKC.referenced_column_id
    
    INNER JOIN sys.tables TBLF ON TBLF.object_id = FKC.parent_object_id
    INNER JOIN sys.tables TBLP ON TBLP.object_id = FKC.referenced_object_id
    
    INNER JOIN sys.schemas SCHF ON FK.schema_id = SCHF.schema_id
    INNER JOIN sys.schemas SCHP ON TBLP.schema_id = SCHP.schema_id
    
	INNER JOIN  INFORMATION_SCHEMA.TABLES TBP ON TBP.TABLE_NAME = TBLP.name
	INNER JOIN  INFORMATION_SCHEMA.TABLES TBF ON TBF.TABLE_NAME = TBLF.name

    INNER JOIN sys.types TP ON COLP.user_type_id = TP.user_type_id
    INNER JOIN sys.types TF ON COLF.user_type_id = TF.user_type_id
    
	INNER JOIN sys.objects OP ON OP.object_id = COLP.object_id
	INNER JOIN sys.objects OFR ON OFR.object_id = COLF.object_id
	INNER JOIN INFORMATION_SCHEMA.COLUMNS SCOLP ON SCOLP.table_name = OP.Name AND SCOLP.column_name = COLP.Name
	LEFT JOIN INFORMATION_SCHEMA.COLUMNS SCOLF ON SCOLF.table_name = OFR.Name AND SCOLP.column_name = COLF.Name
WHERE
    IIF(TP.Name = 'int' AND LEN(SCOLP.COLUMN_DEFAULT) > 0, 'bit', TP.Name) <> 'bit' 
    AND IIF(TF.Name = 'int' AND LEN(SCOLF.COLUMN_DEFAULT) > 0, 'bit', TF.Name) <> 'bit'
ORDER BY 
    PrimaryTableName,
    PrimaryColumnName,
    ForeignTableName,
    ForeignColumnName";
    }
}
