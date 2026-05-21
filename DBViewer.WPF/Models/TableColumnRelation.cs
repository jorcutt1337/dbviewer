namespace DBViewer.WPF.Models
{
	public class TableColumnRelation
	{
		#region Properties

		public string PrimaryTableName { get; set; }
		public string PrimaryTableColumnName { get; set; }
		public string ForeignTableName { get; set; }
		public string ForeignTableColumnName { get; set; }

		#endregion

		#region Queries

		public const string RELATION_QUERY = @"
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

		#region Functions

		public static List<TableColumnRelation> GetInstances()
		{
			return DataUtility.GetInstances<TableColumnRelation>(RELATION_QUERY);
		}

		#endregion
	}
}
