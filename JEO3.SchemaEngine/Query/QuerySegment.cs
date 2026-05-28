using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using JEO3.SchemaEngine.Models;

namespace JEO3.SchemaEngine
{
    public class QuerySegment
    {
        #region Properties

        //public string PrimaryAlias { get; set; }
        //public string ForeignAlias { get; set; }
        //public List<SchemaRelation> Relations { get; set; }
        //public SchemaTable Primary { get; set; }
        //public SchemaTable Foreign { get; set; }
        //public List<SchemaColumn> SelectColumns { get; set; } = new List<SchemaColumn>();
        // --------------------------------------------------
        public string PrimaryTable { get; set; }
        public string ForeignTable { get; set; }
        public string Selects { get; set; }
        public string Joins { get; set; }

        #endregion

        #region Initialization

        public QuerySegment(string primaryTableName, string foreignTableName, string selects, string joins) //, SchemaTable primary, List<SchemaColumn> selectColumns, SchemaTable foreign, List<SchemaRelation> relations)
        {
            PrimaryTable = primaryTableName;
            ForeignTable = foreignTableName;
            //Primary = primary;
            //Foreign = foreign;
            //Relations = relations;
            //SelectColumns = selectColumns;
            Selects = selects;
            Joins = joins;
        }

        #endregion
    }
}
