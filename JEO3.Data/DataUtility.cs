namespace JEO3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Text;
    using JEO3.Data.Extensions;
    using Microsoft.Data.SqlClient;
    using Microsoft.SqlServer.Types;

    public static class DataUtility
    {
        #region Data

        public static bool TestConnection(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Return State Open
                    return connection.State == ConnectionState.Open;
                }
                catch (SqlException)
                {
                    // SQL Related Errors - Timeouts, Login Failures Etc.
                    throw;
                }
                catch (Exception)
                {
                    // Other Potential Issues (e.g. Invalid Connection String Format)
                    throw;
                }
            }
        }

        public static async Task<DataTable> GetDataTable(string query, string connectionString)
        {
            // Connection
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Command
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Reader
                    conn.Open();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        // Create DataTable Dyanmically Instead Of Using DataAdapter To Avoid Issues With SQL UDTs
                        DataTable dt = new DataTable();

                        // Get ColumnNames
                        var columnNames = GetUniqueColumnNames(reader);

                        // Loop Fields
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string typeName = reader.GetDataTypeName(i);
                            Type columnType;

                            // Route SQL UDTs to specific CLR types
                            switch (typeName.ToLower())
                            {
                                case "hierarchyid":
                                    columnType = typeof(SqlHierarchyId);
                                    break;
                                case "geometry":
                                    columnType = typeof(SqlGeometry);
                                    break;
                                case "geography":
                                    columnType = typeof(SqlGeography);
                                    break;
                                default:
                                    // Check Binary - String, Otherwise Default
                                    Type type = reader.GetFieldType(i);
                                    columnType = type == typeof(byte[]) ? typeof(string) : type ?? typeof(object);
                                    break;
                            }


                            dt.Columns.Add(columnNames[i], columnType);
                        }

                        // Populate Rows
                        while (reader.Read())
                        {
                            // Create Row
                            DataRow row = dt.NewRow();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string typeName = reader.GetDataTypeName(i);

                                // Handle Null Values
                                if (reader.IsDBNull(i))
                                {
                                    row[i] = DBNull.Value;
                                    continue;
                                }

                                // Route SQL UDTs to specific CLR types
                                switch (typeName.ToLower())
                                {
                                    case "hierarchyid":
                                        row[i] = reader.GetFieldValue<SqlHierarchyId>(i);
                                        break;
                                    case "geometry":
                                        row[i] = reader.GetFieldValue<SqlGeometry>(i);
                                        break;
                                    case "geography":
                                        row[i] = reader.GetFieldValue<SqlGeography>(i);
                                        break;
                                    default:
                                        Type type = reader.GetFieldType(i);

                                        // Check Binary - String, Otherwise Default
                                        if (type == typeof(byte[]))
                                        {
                                            var data = (byte[])reader.GetValue(i);
                                            row[i] = Encoding.ASCII.GetString(data);
                                        }
                                        else
                                        {
                                            row[i] = reader.GetValue(i);
                                        }
                                        break;
                                }

                            }
                            dt.Rows.Add(row);
                        }
                        return dt;
                    }
                }
            }
        }

        private static List<string> GetUniqueColumnNames(SqlDataReader reader)
        {
            var result = new List<string>();
            var occurrences = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);

                if (occurrences.TryGetValue(columnName, out int count))
                {
                    count++;
                    occurrences[columnName] = count;

                    columnName = $"{columnName}_{count}";
                }
                else
                {
                    occurrences[columnName] = 0;
                }

                result.Add(columnName);
            }

            return result;
        }

        #endregion

        #region Reflection

        public static async Task<IReadOnlyList<T>> GetInstances<T>(string query, string connString)
        {
            var startTime = DateTime.Now;

            // Create List Of Type T
            List<T> list = (List<T>)Activator.CreateInstance(typeof(List<T>));

            // Get DataTable From Query
            var table = await GetDataTable(query, connString);

            // Convert DataTable To List Of Type T
            list = table.ToInstanceList<T>();

            // Logging
            Debug.WriteLine("Time To Load Type - " + typeof(T).Name + " - " + (decimal)Math.Round((DateTime.Now - startTime).TotalSeconds, 3) + " seconds");

            return list;
        }

        #endregion
    }
}