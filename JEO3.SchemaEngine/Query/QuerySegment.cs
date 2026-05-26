using System;
using System.Collections.Generic;
using System.Text;

namespace JEO3.SchemaEngine
{
    public class QuerySegment
    {
        #region Properties

        public string PrimaryTableName { get; set; }
        public string ForeignTableName { get; set; }
        public string PrimaryColumnName { get; set; }
        public string ForeignColumnName { get; set; }
        public string Selects { get; set; }
        public string Joins { get; set; }

        #endregion

        #region Initialization

        public QuerySegment(string primaryTableName, string foreignTableName, string selects, string joins)
        {
            PrimaryTableName = primaryTableName;
            ForeignTableName = foreignTableName;
            Selects = selects;
            Joins = joins;
        }

        #endregion
    }
}
