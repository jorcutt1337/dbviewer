namespace DBViewer.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Data.SqlClient;

    public static class DataUtility
    {
        #region Sql Type Code Mappings

        private static readonly List<KeyValuePair<Type, TypeCode>> TYPE_CODE_MAPPINGS = new List<KeyValuePair<Type, TypeCode>>()
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

        #region SQL

        // Only Place SQL Connections and Queries Are Made
        public static DataTable GetDataTable(string query)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[Globals.DB_CONNECTION].ConnectionString))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    // Open, fetch data, and close connection automatically
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        #endregion

        #region Reflection

        public static List<T> GetInstances<T>(string query)
        {
            var startTime = DateTime.Now;

            try
            {
                // Create List Of Type T
                List<T> list = (List<T>)Activator.CreateInstance(typeof(List<T>));

                // Get DataTable From Query
                var table = GetDataTable(query);

                // Convert DataTable To List Of Type T
                list = DataUtility.GetInstanceListFromDataTable<T>(table);

                Trace.WriteLine("Total Time To Load Type - " + typeof(T).Name + " - " + (int)Math.Round((DateTime.Now - startTime).TotalSeconds, 1) + " seconds");

                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        private static List<T> GetInstanceListFromDataTable<T>(DataTable dt)
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
                T item = GetInstanceFromRow<T>(dr, columnNames, properties);

                // Add Instance To List
                list.Add(item);
            }

            return list;
        }

        private static T GetInstanceFromRow<T>(DataRow dr, List<string> columnNames, List<PropertyInfo> properties)
        {
            // Create New Instance Of Type T
            T item = (T)Activator.CreateInstance(typeof(T));

            // Loop Type Properties
            foreach (var property in properties)
            {
                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                // If Table Has No Matching Column Name Or Property Value Type Is Not In Type Mapping List - Continue
                if (columnNames.Any(name => name.ToUpper() == property.Name.ToUpper()) == false
                    || dr[property.Name.ToUpper()] == null
                    || dr[property.Name.ToUpper()] == DBNull.Value
                    || TYPE_CODE_MAPPINGS.Any(kvp => kvp.Key.Name == underlyingType.Name) == false)
                {
                    continue;
                }

                // Get Property Type TypeCode
                TypeCode code = TYPE_CODE_MAPPINGS.First(kvp => kvp.Key == underlyingType).Value;

                // Validation
                if (dr[property.Name.ToUpper()] == null || dr[property.Name.ToUpper()] == DBNull.Value)
                {
                    continue;
                }

                try
                {
                    // Get Cell Value
                    var value = dr[property.Name.ToUpper()] == null ? "" : dr[property.Name.ToUpper()].ToString().Trim();

                    // Set Property Value
                    SetPropertyValue(item, property, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return item;
        }

        private static bool SetPropertyValue(object item, PropertyInfo property, string value)
        {
            try
            {
                // Can this be done with generics and type constraints?
                // Maybe. But we will need to implement custom parsing logic for non-primitive types like Flag, HierarchyId, Geography, Geometry, etc.

                // Temp vars
                Int32 int32Tmp = 0; Int64 int64Tmp = 0; double dblTmp = 0; decimal decTmp = 0; long lngTmp = 0; DateTime dtTemp; float fltTmp = 0;

                // Get Generic Type Arguments For Nullable Property Typez
                var args = property.PropertyType.GenericTypeArguments;

                // Check Property Type And Set Value. TODO: This Condition List Is Quite Small..... There Will Be Outliers Like DateTime or DateTime2
                if (property.PropertyType == typeof(double) || (args.Count() > 0 && args[0].Name.ToLower() == "double"))
                {
                    property.SetValue(item, double.TryParse(value, out dblTmp) ? double.Parse(value) : 0);
                }
                else if (property.PropertyType == typeof(double?))
                {
                    property.SetValue(item, double.TryParse(value, out dblTmp) ? double.Parse(value) : default(double?));
                }
                else if (property.PropertyType == typeof(Int64) || (args.Count() > 0 && args[0].Name.ToLower() == "int64"))
                {
                    property.SetValue(item, Int64.TryParse(value, out int64Tmp) ? Int64.Parse(value) : 0);
                }
                else if (property.PropertyType == typeof(Int64?))
                {
                    property.SetValue(item, Int64.TryParse(value, out int64Tmp) ? Int64.Parse(value) : default(Int64?));
                }
                else if (property.PropertyType == typeof(Int32) || (args.Count() > 0 && (args[0].Name.ToLower().StartsWith("int"))))
                {
                    property.SetValue(item, Int32.TryParse(value, out int32Tmp) ? Int32.Parse(value) : 0);
                }
                else if (property.PropertyType == typeof(Int32?))
                {
                    property.SetValue(item, Int32.TryParse(value, out int32Tmp) ? Int32.Parse(value) : default(Int32?));
                }
                else if (property.PropertyType == typeof(decimal) || (args.Count() > 0 && args[0].Name.ToLower() == "decimal"))
                {
                    property.SetValue(item, decimal.TryParse(value, out decTmp) ? decimal.Parse(value) : 0);
                }
                else if (property.PropertyType == typeof(decimal?))
                {
                    property.SetValue(item, decimal.TryParse(value, out decTmp) ? decimal.Parse(value) : default(decimal?));
                }
                else if (property.PropertyType == typeof(float) || (args.Count() > 0 && args[0].Name.ToLower() == "float"))
                {
                    property.SetValue(item, float.TryParse(value, out fltTmp) ? float.Parse(value) : 0);
                }
                else if (property.PropertyType == typeof(float?))
                {
                    property.SetValue(item, float.TryParse(value, out fltTmp) ? float.Parse(value) : default(float?));
                }
                else if (property.PropertyType == typeof(long) || (args.Count() > 0 && args[0].Name.ToLower() == "long"))
                {
                    property.SetValue(item, long.TryParse(value, out lngTmp) ? long.Parse(value) : 0);
                }
                else if (property.PropertyType == typeof(long?))
                {
                    property.SetValue(item, long.TryParse(value, out lngTmp) ? long.Parse(value) : default(long?));
                }
                else if (property.PropertyType == typeof(DateTime) || (args.Count() > 0 && args[0].Name.ToLower() == "datetime"))
                {
                    property.SetValue(item, DateTime.TryParse(value, out dtTemp) ? DateTime.Parse(value) : DateTime.MinValue);
                }
                else if (property.PropertyType == typeof(bool) || (args.Count() > 0 && args[0].Name.ToLower() == "bool"))
                {
                    var bVal = value.ToUpper() == "TRUE" || value.ToUpper() == "Y" || value.ToUpper() == "1" ? true : false;
                    property.SetValue(item, bVal);
                }
                else if (property.PropertyType == typeof(bool?))
                {
                    var bVal = value.ToUpper() == "TRUE" || value.ToUpper() == "Y" || value.ToUpper() == "1" ? true : false;
                    property.SetValue(item, bVal);
                }
                else if ((property.PropertyType == typeof(TimeSpan) || property.PropertyType == typeof(TimeSpan?)) && value.Length > 0)
                {
                    var pct24Hours = double.TryParse(value, out dblTmp) ? double.Parse(value) : 0;
                    var hours = pct24Hours > 0 ? Math.Round((double)24 * pct24Hours, 0) : 0;

                    // Since TimeSpan.Parse does not support parsing hours greater than 23,
                    // we will just need to create a DateTime object and then convert it to a TimeSpan
                    // Year 2000 Is Arbitrary. We Just Need A Valid Date To Parse The Time From.
                    var val =  "01/01/2000 " + hours + ":00:00";
                    var date = DateTime.TryParse(val, out dtTemp) ? DateTime.Parse(val) : DateTime.MinValue;
                    if (date > DateTime.MinValue)
                    {
                        property.SetValue(item, new TimeSpan(date.Hour, date.Minute, date.Second));
                    }
                }
                else
                {
                    property.SetValue(item, value);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Other

        public static string ToCSV(this DataTable dt)
        {
            var rows = dt.AsEnumerable().Select(row => string.Join(",", row.ItemArray.ToList()));
            var csv = string.Join("\r\n", rows);

            return csv;
        }

        #endregion
    }
}