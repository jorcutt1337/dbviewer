using System;
using System.Collections.Generic;
using System.Text;
using JEO3.SchemaEngine.Models;

namespace JEO3.SchemaEngine.Extensions
{
    internal static class SchemaExtensions
    {
        private const string TB = "\t";
        private const string RN = "\r\n";
        private const string RNT = RN + TB;
        private const string FROM = "FROM ";

        internal static IReadOnlyList<SchemaColumn> OtherTableColumns(this SchemaColumn column)
        {
            if (column == null) { throw new Exception(nameof(OtherTableColumns) + " - Cannot find table columns."); }

            return column.Table?.Columns?.Where(c => c.ColumnName != column.ColumnName).ToList() ?? new List<SchemaColumn>();
        }


        //internal static string QuerySelects()
        //{
        //    RN + SLTT + string.Join("," + (isSingleLine == false ? RN + SLTT : " "), relatedTableColumns.OrderBy(x => x.OrdinalPosition).Select(x => (isSingleLine == false ? TB + TB : "") + "[" + key.ForeignTableName + "]." + x.ColumnName)) + ",";
        //}

        internal static string QueryFrom(this SchemaTable table, bool generateTableJoins, string alias, string? extraJoins = null)
        {
            var from = generateTableJoins ? RNT + FROM + table.TableName + " " + alias : RNT + FROM + table.TableName + " " + alias
                + (extraJoins != null ? RNT + string.Join(RNT, extraJoins) : string.Empty) + RNT + RNT;
            return from;
        }

        internal static string Query(this IEnumerable<SchemaColumn> columns, bool isSingleLine, string alias)
        {
            var SLTT = (isSingleLine == false ? string.Empty : TB + TB);

            var selectColumns = columns
                .OrderBy(v => v.OrdinalPosition)
                .Select(v => (isSingleLine == false ? TB + TB : " ") + alias + "." + v.ColumnName).Distinct().ToList();

            var select = SLTT + String.Join("," + (isSingleLine == false ? RN : " "), selectColumns);

            return select;
        }
    }
}
