using System;
using System.Collections.Generic;
using System.Text;

namespace DBViewer.WPF
{
    internal class SqlConstants
    {
		#region Queries

		internal const string SelectTop1000 = "SELECT TOP 1000";

        internal const string SchemaQuery = @"

	SELECT DISTINCT
		DB_NAME() AS DatabaseName,
		TB.TABLE_SCHEMA AS TablePrefix,
		TB.TABLE_NAME AS TableShortName,
		(CASE LOWER(TB.TABLE_SCHEMA) WHEN 'dbo' THEN '' ELSE TB.TABLE_SCHEMA + '.' END) + TB.TABLE_NAME AS TableName,
		COL.COLUMN_NAME AS ColumnName,
		--(CASE SI.name WHEN NULL THEN '' ELSE 'Index' END) AS ColumnDescription,
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
		0 AS DataTypeId,
		IIF(T.Name = 'int' AND LEN(COL.COLUMN_DEFAULT) > 0, 'bit', T.Name) AS DataType,
		(CASE WHEN COL.CHARACTER_MAXIMUM_LENGTH IS NULL THEN C.max_length ELSE COL.CHARACTER_MAXIMUM_LENGTH END) AS MaximumLength, 
		C.precision AS Precision,
		C.scale AS Scale,
		REPLACE(REPLACE(COALESCE(COL.COLUMN_DEFAULT, ''), '(', ''), ')', '') AS ColumnDefault,
		P.Rows AS Rows
	FROM sys.columns C
		INNER JOIN sys.objects O ON O.object_id = C.object_id
		INNER JOIN sys.types T ON C.user_type_id = T.user_type_id
		LEFT JOIN sys.tables ST ON ST.object_id = C.object_id
		LEFT JOIN  INFORMATION_SCHEMA.TABLES TB ON TB.TABLE_NAME = ST.name
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
	SELECT
		TB.TABLE_SCHEMA + '.' + PT.Name AS PrimaryTableName,
		COALESCE(PI.is_primary_key, 0) AS IsIdentity,
		PC.Name AS PrimaryTableColumnName,
		TB2.TABLE_SCHEMA + '.' + FT.name as ForeignTableName, 
		--FK.constraint_column_id AS FK_PartNo, 
		FC.name as ForeignTableColumnName 
	FROM sys.foreign_key_columns AS FK
		INNER JOIN sys.tables AS FT ON FT.object_id = FK.parent_object_id
		INNER JOIN sys.columns AS FC on FK.parent_object_id = FC.object_id AND FK.parent_column_id = FC.column_id
		INNER JOIN sys.tables AS PT ON PT.object_id = FK.referenced_object_id
		LEFT OUTER JOIN sys.columns PC ON PC.object_id = PT.object_id AND PC.column_id = FK.constraint_column_id
		LEFT OUTER JOIN sys.index_columns PIC ON PIC.object_id = PC.object_id AND PIC.column_id = PC.column_id
		LEFT OUTER JOIN sys.indexes PI ON PI.object_id = PIC.object_id AND PI.index_id = PIC.index_id
		LEFT OUTER JOIN  INFORMATION_SCHEMA.TABLES TB ON TB.TABLE_NAME = PT.name
		LEFT OUTER JOIN  INFORMATION_SCHEMA.TABLES TB2 ON TB2.TABLE_NAME = FT.name
	WHERE
		PI.is_primary_key = 1
	ORDER BY
		PrimaryTableName, 
		ForeignTableName, 
		FK.constraint_column_id";

        #endregion

        #region Type Mappings

        /// <summary>
        /// List of SQL Type Codes for the app to work
        /// 
        /// DO NOT REMOVE
        /// 
        /// </summary>
        internal static readonly List<KeyValuePair<Type, TypeCode>> SqlTypeCodes = new List<KeyValuePair<Type, TypeCode>>()
            {
                // For Flag, HierarchyId, Geography, Geometry, and other non-primitive types, we will need to implement custom parsing logic in the SetPropertyValue function
                new KeyValuePair<Type, TypeCode>(typeof(Boolean), TypeCode.Boolean),
                new KeyValuePair<Type, TypeCode>(typeof(bool), TypeCode.Boolean),
                new KeyValuePair<Type, TypeCode>(typeof(byte), TypeCode.Byte),
                new KeyValuePair<Type, TypeCode>(typeof(char), TypeCode.Char),
                new KeyValuePair<Type, TypeCode>(typeof(Char), TypeCode.Char),
                new KeyValuePair<Type, TypeCode>(typeof(DateTime), TypeCode.DateTime),
                new KeyValuePair<Type, TypeCode>(typeof(decimal), TypeCode.Decimal),
                new KeyValuePair<Type, TypeCode>(typeof(Decimal), TypeCode.Decimal),
                new KeyValuePair<Type, TypeCode>(typeof(double), TypeCode.Double),
                new KeyValuePair<Type, TypeCode>(typeof(Double), TypeCode.Double),
                new KeyValuePair<Type, TypeCode>(typeof(short), TypeCode.Int16),
                new KeyValuePair<Type, TypeCode>(typeof(int), TypeCode.Int32),
                new KeyValuePair<Type, TypeCode>(typeof(long), TypeCode.Int64),
                new KeyValuePair<Type, TypeCode>(typeof(float), TypeCode.Single),
                new KeyValuePair<Type, TypeCode>(typeof(string), TypeCode.String),
                new KeyValuePair<Type, TypeCode>(typeof(String), TypeCode.String),
                new KeyValuePair<Type, TypeCode>(typeof(ushort), TypeCode.UInt16),
                new KeyValuePair<Type, TypeCode>(typeof(uint), TypeCode.UInt32),
                new KeyValuePair<Type, TypeCode>(typeof(Int16), TypeCode.Int16),
                new KeyValuePair<Type, TypeCode>(typeof(Int32), TypeCode.Int32),
                new KeyValuePair<Type, TypeCode>(typeof(Int64), TypeCode.Int64),
                new KeyValuePair<Type, TypeCode>(typeof(UInt16), TypeCode.UInt16),
                new KeyValuePair<Type, TypeCode>(typeof(UInt32), TypeCode.UInt32),
                new KeyValuePair<Type, TypeCode>(typeof(UInt64), TypeCode.UInt64)
            };

		#endregion
	}
}
