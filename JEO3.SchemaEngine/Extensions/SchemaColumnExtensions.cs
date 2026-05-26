using System;
using System.Collections.Generic;
using System.Text;
using JEO3.SchemaEngine.Models;

namespace JEO3.SchemaEngine.Extensions
{
    internal static class SchemaColumnExtensions
    {
        internal static IReadOnlyList<SchemaColumn> OtherTableColumns(this SchemaColumn column)
        {
            if (column == null) { throw new Exception(nameof(OtherTableColumns) + " - Cannot find table columns."); }

            return column.Table?.Columns?.Where(c => c.ColumnName != column.ColumnName).ToList() ?? new List<SchemaColumn>();
        }
    }
}
