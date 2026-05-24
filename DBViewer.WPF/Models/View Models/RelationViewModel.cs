using System.Xml.Serialization;

namespace DBViewer.WPF.Models
{
    /// <summary>
    /// ViewModel For UI Binding Of Table Relations / Keys
    /// </summary>
    public class RelationViewModel
	{
        #region Properties

        public string PrimaryTableSchemaName { get; set; } = string.Empty;
        public string PrimaryTableShortName { get; set; } = string.Empty;
        public string ForeignTableSchemaName { get; set; } = string.Empty;
        public string ForeignTableShortName { get; set; } = string.Empty;

        // ---

        public string PrimaryTableName { get; set; } = string.Empty;
        public string PrimaryTableColumnName { get; set; } = string.Empty;
        public string ForeignTableName { get; set; } = string.Empty;
        public string ForeignTableColumnName { get; set; } = string.Empty;
        
        // ---
        
        public string ForeignKeyName { get; set; } = string.Empty;
        public bool IsCompositeKey { get; set; }
        public bool PrimaryIsNullable { get; set; }
        public bool ForeignIsNullable { get; set; }

        // ---

        [XmlIgnore]
		public SchemaViewModel PrimaryColumn { get; set; } = new();

        [XmlIgnore]
        public SchemaViewModel ForeignColumn { get; set; } = new();

        #endregion
    }
}
