using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace JEO3.Data.Extensions
{
    /// <summary>
    /// DataRow Extensions
    /// </summary>
    public static class DataRowExtensions
    {
        #region Reflection

        public static T ToInstance<T>(this DataRow dr, List<string> columnNames, List<PropertyInfo> properties)
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
                    || SqlConstants.TypeCodes.Any(kvp => kvp.Key.Name == underlyingType.Name) == false)
                {
                    continue;
                }

                // Get Property Type TypeCode
                TypeCode code = SqlConstants.TypeCodes.First(kvp => kvp.Key == underlyingType).Value;

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
                    var val = "01/01/2000 " + hours + ":00:00";
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

    }
}
