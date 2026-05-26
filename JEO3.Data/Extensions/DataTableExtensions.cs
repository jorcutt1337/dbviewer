using System.Data;
using System.Reflection;

namespace JEO3.Data.Extensions
{
    public static class DataTableExtensions
    {
        #region Reflection

        public static List<T> ToInstanceList<T>(this DataTable dt)
        {
            // Validation
            if (dt == null || dt.Rows.Count == 0)
            {
                return new List<T>();
            }

            // Create List Of Type T
            List<T> list = (List<T>)Activator.CreateInstance(typeof(List<T>));

            // Get Type Properties
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList();

            // Get List Of Table Column Names
            List<string> columnNames = new List<string>();
            foreach (DataColumn column in dt.Columns)
            {
                columnNames.Add(column.ColumnName);
            }

            // Loop DataRows
            foreach (DataRow dr in dt.Rows)
            {
                // Create New Instance Of Type T
                T item = dr.ToInstance<T>(columnNames, properties);

                // Add Instance To List
                list.Add(item);
            }

            return list;
        }

        #endregion

        #region Helpers

        public static List<DataRow> ToList(this DataTable dt) => dt.AsEnumerable().ToList();

        #endregion

        #region Other

        public static string ToCSV(this DataTable dt, bool includeHeaders = true)
        {
            var csv = includeHeaders == false ? string.Empty : string.Join(",", dt.Columns.ToList().Select(v => v.ColumnName).ToList());

            var rows = dt.ToList().Select(row => string.Join(",", row.ItemArray.ToList()));
            csv += string.Join("\r\n", rows);

            return csv;
        }

        #endregion
    }
}
