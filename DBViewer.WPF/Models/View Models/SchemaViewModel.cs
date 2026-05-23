using System.Printing;
using System.Xml.Serialization;

namespace DBViewer.WPF.Models
{
    /// <summary>
    /// ViewModel For UI Binding Of Table Schema Information
    /// </summary>
    public class SchemaViewModel
    {
        #region Properties

        public string DatabaseName { get; set; } = string.Empty;

        public string TableFullName => string.Join(".", TablePrefix, TableName);
        public string TableName { get; set; } = string.Empty;
        public string TablePrefix { get; set; } = string.Empty;

        private string _ColumnName = string.Empty;
        public string ColumnName
        {
            get
            {
                return _ColumnName;
            }
            set
            {
                var val = value.Replace("[", string.Empty).Replace("]", string.Empty).Trim();
                _ColumnName = SqlConstants.SqlReservedKeywordsAll.Any(v => v == val.ToUpper()) || val.Contains(" ") ? string.Format("[{0}]", val) : val;
            }
        }
        
        public int OrdinalPosition { get; set; }
        public string DataType { get; set; } = string.Empty;
        public int? MaximumLength { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public string Precision { get; set; } = string.Empty;
        public string Scale { get; set; } = string.Empty;
        public string ColumnDefault { get; set; } = string.Empty;

        public long Rows { get; set; }

        [XmlIgnore]
        public List<SchemaViewModel> OtherTableColumns { get; set; } = new List<SchemaViewModel>();

        #endregion
    }
}
