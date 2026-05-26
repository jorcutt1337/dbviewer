using System.Xml.Serialization;
using JEO3.Data;

namespace JEO3.SchemaEngine.Models
{
    /// <summary>
    /// </summary>
    public sealed class SchemaColumn
    {
        #region Properties

        [XmlIgnore]
        public SchemaTable Table { get; set; } = default!;

        public string DatabaseName { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public string TableShortName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string TableFullName => string.Join(".", SchemaName, TableName);
        private string _ColumnName = string.Empty;
        public string ColumnName
        {
            get
            {
                return _ColumnName;
            }
            set
            {
                // Validation
                if (value == null || value == string.Empty || _ColumnName != string.Empty) { return; }

                // Check If Column Name Is A Reserved Keyword Or Contains Spaces, If So Wrap In Brackets - This Applies To Every Usage Of This Property Everywhere
                var val = value.Replace("[", string.Empty).Replace("]", string.Empty).Trim();
                _ColumnName = SqlConstants.ReservedKeywordsAll.Any(v => v == val.ToUpper()) || val.Contains(" ") ? string.Format("[{0}]", val) : val;
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

        #endregion

        #region Initialization

        #endregion
    }
}
