using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;

namespace JEO3.Data.Extensions
{
    public static class DataColumnExtensions
    {
        #region Helpers

        public static List<DataColumn> ToList(this DataColumnCollection columns)
        {
            var columnsList = new List<DataColumn>();
            foreach (DataColumn column in columns)
            {
                columnsList.Add(column);
            }
            return columnsList;
        }

        #endregion
    }
}
