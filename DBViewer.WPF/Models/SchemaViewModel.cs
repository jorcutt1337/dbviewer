using System.Printing;
using System.Xml.Serialization;

namespace DBViewer.WPF.Models
{
    public class SchemaViewModel
    {
        #region Properties

        public string DatabaseName { get; set; }

		public string TableFullName => string.Join(".", TablePrefix, TableName);
        public string TableName { get; set; }
        public string TablePrefix { get; set; }

        private string _ColumnName = string.Empty;
        public string ColumnName
        {
            get
            {
                return _ColumnName;
            }
            set
            {
                _ColumnName = SqlConstants.SqlReservedKeywordsAll.Any(v => v == value.ToUpper()) || value.Trim().Contains(" ") ? string.Format("[{0}]", value) : value;
            }
        }
        
        public int OrdinalPosition { get; set; }
        public string DataType { get; set; }
        public int? MaximumLength { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }
        public string ColumnDefault { get; set; }

        public long Rows { get; set; }

        [XmlIgnore]
        public List<SchemaViewModel> OtherTableColumns { get; set; } = new List<SchemaViewModel>();

        #endregion
    }
}
