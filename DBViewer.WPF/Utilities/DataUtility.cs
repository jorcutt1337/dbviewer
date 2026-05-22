namespace DBViewer.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using DBViewer.WPF.Extensions;
    using Microsoft.Data.SqlClient;

    public static class DataUtility
    {
        // Only Place SQL Connections and Queries Are Made
        public static DataTable GetDataTable(string query)
        {
            var startTime = DateTime.Now;

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[Globals.DbConnectionKeyName].ConnectionString))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    adapter.Fill(dt);
                }
            }

            Debug.WriteLine("Time To Execute SQL - " + (int)Math.Round((DateTime.Now - startTime).TotalSeconds, 3) + " seconds");

            return dt;
        }

        public static List<T> GetInstances<T>(string query)
        {
            try
            {
                // Create List Of Type T
                List<T> list = (List<T>)Activator.CreateInstance(typeof(List<T>));

                // Get DataTable From Query
                var table = GetDataTable(query);

                var startTime = DateTime.Now;

                // Convert DataTable To List Of Type T
                list = table.GetInstanceList<T>();

                Debug.WriteLine("Time To Load Type - " + typeof(T).Name + " - " + (int)Math.Round((DateTime.Now - startTime).TotalSeconds, 3) + " seconds");

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