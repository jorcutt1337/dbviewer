using System;
using System.Collections.Generic;
using System.Text;

namespace JEO3.Data
{
    public static class SqlConstants
    {
        #region Keywords

        public static readonly List<string> ReservedKeywordsAction = new List<string>()
        {
            "ADD", "ALTER", "CREATE", "DELETE", "DROP", "INSERT", "SELECT", "UPDATE"
        };

        public static readonly List<string> ReservedKeywordsStructural = new List<string>()
        {
            "COLUMN", "DATABASE", "INDEX", "SCHEMA", "TABLE", "VIEW"
        };

        public static readonly List<string> ReservedKeywordsLogical = new List<string>()
        {
            "ALL", "AND", "AS", "BY", "CASE", "FROM", "GROUP", "HAVING", "IN", "JOIN", "OR", "ORDER", "WHERE"
        };

        public static readonly List<string> ReservedKeywordsConstraints = new List<string>()
        {
            "CHECK", "CONSTRAINT", "DEFAULT", "FOREIGN", "PRIMARY", "REFERENCES", "UNIQUE"
        };

        public static readonly List<string> ReservedKeywordsMetadata = new List<string>()
        {
            "CURRENT_USER", "IDENTITY", "USER", "SESSION_USER"
        };

        public static readonly List<string> ReservedKeywordsAll = ReservedKeywordsAction
            .Concat(ReservedKeywordsStructural)
            .Concat(ReservedKeywordsLogical)
            .Concat(ReservedKeywordsConstraints)
            .Concat(ReservedKeywordsMetadata).ToList();

        #endregion

        #region Type Mappings

        /// <summary>
        /// List of SQL Type Codes for Model Hydration. Note: They do not affect the retrieval of a DataTable from a SQL query, only DataTable --> Hydration of a model of Type T
        /// For example, if model <T> has a property of type int, then this list will be used to determine that the TypeCode for int is Int32, 
        /// which will then be used in the SetPropertyValue function to set the value of the property correctly. UDT's: Flag, HierarchyId, Geography, Geometry
        /// 
        /// DO NOT REMOVE
        /// 
        /// </summary>
        public static readonly List<KeyValuePair<Type, TypeCode>> TypeCodes = new List<KeyValuePair<Type, TypeCode>>()
            {
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
    }
}
