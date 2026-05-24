namespace DBViewer.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.PortableExecutable;
    using System.Text;
    using DBViewer.WPF.Extensions;
    using Microsoft.Data.SqlClient;
    using Microsoft.SqlServer.Types;

    public static class DataUtility
    {
        public static DataTable GetDataTable(string query, string connectionString)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();

                        // 1. Build DataTable schema dynamically
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string typeName = reader.GetDataTypeName(i).ToLower();
                            Type type = reader.GetFieldType(i);
                            Type columnType;

                            // Route SQL UDTs to specific CLR types
                            switch (typeName)
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
                                    // Use default types for standard columns
                                    columnType = reader.GetFieldType(i) ?? typeof(object);
                                    break;
                            }

                            if (type == typeof(byte[]))
                            {
                                columnType = typeof(string); // Map binary data to string for easier display
                            }

                            dt.Columns.Add(reader.GetName(i), columnType);
                        }

                        // 2. Populate DataTable rows
                        while (reader.Read())
                        {
                            DataRow row = dt.NewRow();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string typeName = reader.GetDataTypeName(i).ToLower();
                                Type type = reader.GetFieldType(i);

                                // Prevent casting exceptions by mapping null values
                                if (reader.IsDBNull(i))
                                {
                                    row[i] = DBNull.Value;
                                    continue;
                                }

                                // Read explicit types mapped above
                                switch (typeName)
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
                                        row[i] = reader.GetValue(i);
                                        break;
                                }

                                if (type == typeof(byte[]))
                                {
                                    var data = (byte[])reader.GetValue(i);
                                    row[i] = Encoding.ASCII.GetString(data); // Map binary data to string for easier display
                                }
                            }
                            dt.Rows.Add(row);
                        }
                        return dt;
                    }
                }
            }

            return null;
        }

        public static List<T> GetInstances<T>(string query, string connString)
        {
            try
            {
                // Create List Of Type T
                List<T> list = (List<T>)Activator.CreateInstance(typeof(List<T>));

                // Get DataTable From Query
                var table = GetDataTable(query, connString);

                var startTime = DateTime.Now;

                // Convert DataTable To List Of Type T
                list = table.GetInstanceList<T>();

                Debug.WriteLine("Time To Load Type - " + typeof(T).Name + " - " + (decimal)Math.Round((DateTime.Now - startTime).TotalSeconds, 3) + " seconds");

                return list;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return null;
        }
    }
}