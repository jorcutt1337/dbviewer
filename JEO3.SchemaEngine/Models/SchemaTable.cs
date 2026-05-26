using System;
using System.Collections.Generic;
using System.Text;

namespace JEO3.SchemaEngine.Models
{
    /// <summary>
    /// </summary>
    public sealed class SchemaTable
    {
        #region Properties
        public string DatabaseName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public int Rows { get; set; }

        public IReadOnlyList<SchemaColumn> Columns { get; init; }

        public IReadOnlyList<SchemaRelation> RelationsDown { get; init; }
        public IReadOnlyList<SchemaRelation> RelationsUp { get; init; }

        #endregion
        #region Initialization

        public SchemaTable()
        {

        }

        public SchemaTable(string databaseName, string tableName, List<SchemaColumn> columns, IReadOnlyList<SchemaRelation> relationsDown, IReadOnlyList<SchemaRelation> relationsUp)
        {
            DatabaseName = databaseName;
            TableName = tableName;
            Columns = columns;
            RelationsDown = relationsDown;
            RelationsUp = relationsUp;
        }

        #endregion
    }
}
