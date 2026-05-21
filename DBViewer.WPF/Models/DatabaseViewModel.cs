namespace DBViewer.WPF.Models
{
    public class DatabaseViewModel
    {
        #region Properties

        public string DatabaseName { get; set; }

        public string TableName { get; set; }
        public string TableCategory { get; set; }
        public string TablePrefix { get; set; }
        public string TableDescription { get; set; }
        public string TableType { get; set; }
        public string RecordType { get; set; }

        public string ColumnName { get; set; }
        public string ColumnDescription { get; set; }
        public int OrdinalPosition { get; set; }
        public string ColumnDefault { get; set; }
        public string DataTypeId { get; set; }
        public string DataType { get; set; }
        public int? CharacterMaximumLength { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }
        public string PredefinedAcceptableValues { get; set; }

        public long Rows { get; set; }
        public string KeyColumns { get; set; }

        #endregion


        #region Queries

        private const string SCHEMA_QUERY = @"

SELECT DISTINCT
		 DB_NAME() AS DatabaseName,
		TB.TABLE_SCHEMA + '.' + O.Name AS TableName,
		'' AS TableCategory,'' AS TablePrefix,'' AS TableDescription,'' AS TableType,'' AS RecordType,
		C.Name AS ColumnName,
		--(CASE SI.name WHEN NULL THEN '' ELSE 'Index' END) AS ColumnDescription,
		C.column_id AS OrdinalPosition, 
		'' AS ColumnDefault,
		C.is_nullable AS IsNullable,
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
		(CASE WHEN COL.CHARACTER_MAXIMUM_LENGTH IS NULL THEN C.max_length ELSE COL.CHARACTER_MAXIMUM_LENGTH END) AS CharacterMaximumLength, 
		C.precision AS Precision,
		C.scale AS Scale,
		REPLACE(REPLACE(COALESCE(COL.COLUMN_DEFAULT, ''), '(', ''), ')', '') AS PredefinedAcceptableValues,
		'' AS PredefinedAcceptableValueDescriptions,
		P.Rows AS Rows
	FROM sys.columns C
		INNER JOIN sys.objects O ON O.object_id = C.object_id
		INNER JOIN sys.types T ON C.user_type_id = T.user_type_id
		LEFT JOIN sys.tables ST ON ST.object_id = C.object_id
		LEFT JOIN  INFORMATION_SCHEMA.TABLES TB ON TB.TABLE_NAME = ST.name
		--LEFT OUTER JOIN sys.index_columns IC ON IC.object_id = C.object_id AND IC.column_id = C.column_id
		--LEFT OUTER JOIN sys.indexes I ON ic.object_id = i.object_id AND ic.index_id = i.index_id
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
		TB.TABLE_SCHEMA + '.' + O.Name,
		C.column_id";

        #endregion

        #region Functions

        public static List<DatabaseViewModel> GetInstances()
        {
            var models = DataUtility.GetInstances<DatabaseViewModel>(SCHEMA_QUERY);

            return models;
        }

        #endregion
    }
}
