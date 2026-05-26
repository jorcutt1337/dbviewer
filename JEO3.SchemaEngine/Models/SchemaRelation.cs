using System.Xml.Serialization;

namespace JEO3.SchemaEngine.Models
{
    /// <summary>
    /// </summary>
    public sealed class SchemaRelation
	{
        #region Properties

        public string DatabaseName { get; set; } = string.Empty;
        public string PrimaryTableSchema { get; set; } = string.Empty;
        public string ForeignTableSchema { get; set; } = string.Empty;

        public string PrimaryTableShortName { get; set; } = string.Empty;
        public string ForeignTableShortName { get; set; } = string.Empty;

        public string PrimaryTableName { get; set; } = string.Empty;
        public string ForeignTableName { get; set; } = string.Empty;
        
        public string PrimaryColumnName { get; set; } = string.Empty;
        public string ForeignColumnName { get; set; } = string.Empty;
        public string PrimaryColumnDataType { get; set; } = string.Empty;
        public string ForeignColumnDataType { get; set; } = string.Empty;

        public string ForeignKeyName { get; set; } = string.Empty;
        public bool IsCompositeKey { get; set; }
        public bool ForeignIsNullable { get; set; }


        [XmlIgnore]
        public SchemaTable PrimaryTable { get; set; } = default!;

        [XmlIgnore]
        public SchemaTable ForeignTable { get; set; } = default!;

        [XmlIgnore]
		public SchemaColumn PrimaryColumn { get; set; } = new();

        [XmlIgnore]
        public SchemaColumn ForeignColumn { get; set; } = new();

        #endregion
    }
}
