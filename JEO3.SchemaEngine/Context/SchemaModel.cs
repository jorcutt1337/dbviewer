using System.Data.Common;
using System.Xml.Serialization;

namespace JEO3.SchemaEngine.Models
{
    /// <summary>
    /// </summary>
    public sealed class SchemaModel
    {
        public IReadOnlyList<SchemaColumn> Columns { get; set; }
        public IReadOnlyList<SchemaRelation> Relations { get; set; }

        [XmlIgnore]
        public IReadOnlyList<SchemaTable> Tables { get; set; }

        public SchemaModel(IReadOnlyList<SchemaColumn> columns, IReadOnlyList<SchemaRelation> relations)
        {
            Columns = columns;
            Relations = relations;

            // Populate Tables
            Tables = columns.GroupBy(v => new { v.DatabaseName, v.TableName, v.SchemaName, v.Rows })
                        .Select(v => new SchemaTable(
                            v.Key.DatabaseName, 
                            v.Key.TableName,
                            v.ToList(), 
                            relations.Where(k => k.PrimaryTableName == v.Key.TableName).ToList(), 
                            relations.Where(k => k.ForeignTableName == v.Key.TableName).ToList())).Distinct().ToList();

            // Set Table Navigation Property For Each Column
            var tableLookup = Tables.ToDictionary(v => (v.DatabaseName, v.TableName));
            foreach (var column in columns)
            {
                // Validation
                if (!tableLookup.TryGetValue((column.DatabaseName, column.TableName), out var table))
                {
                    throw new Exception($"Table not found for {column.DatabaseName}.{column.TableName}.{column.ColumnName}");
                }

                column.Table = table;
            }

            // Set Table & Column Navigation Properties For Each Column
            var columnLookup = Columns.ToDictionary(v => (v.DatabaseName, v.TableName, v.ColumnName, v.DataType));
            foreach (var relation in relations)
            {
                // Validation - Bombs Away :)
                if (!tableLookup.TryGetValue((relation.DatabaseName, relation.ForeignTableName), out var fTable))
                {
                    throw new Exception($"Foreign Table not found for {relation.DatabaseName}.{relation.ForeignTableName}");
                }
                if (!tableLookup.TryGetValue((relation.DatabaseName, relation.PrimaryTableName), out var pTable))
                {
                    throw new Exception($"Primary Table not found for {relation.DatabaseName}.{relation.PrimaryTableName}");
                }
                if (!columnLookup.TryGetValue((relation.DatabaseName, relation.PrimaryTableName, relation.PrimaryColumnName, relation.PrimaryColumnDataType), out var pColumn))
                {
                    throw new Exception($"Primary Table Column not found for {relation.DatabaseName}.{relation.PrimaryTableName}.{relation.PrimaryColumnName}");
                }
                if (!columnLookup.TryGetValue((relation.DatabaseName, relation.ForeignTableName, relation.ForeignColumnName, relation.ForeignColumnDataType), out var fColumn))
                {
                    throw new Exception($"Foreign Table Column not found for {relation.DatabaseName}.{relation.ForeignTableName}.{relation.ForeignColumnName}");
                }

                relation.PrimaryTable = pTable;
                relation.PrimaryColumn = pColumn;
                relation.ForeignTable = fTable;
                relation.ForeignColumn = fColumn;
            }
        }
    }
}
