using System.Xml.Serialization;

namespace DBViewer.WPF.Models
{
    /// <summary>
    /// ViewModel For UI Binding Of Table Relations / Keys
    /// </summary>
    public class RelationViewModel
	{
		#region Properties

		public string PrimaryTableName { get; set; } = string.Empty;
        public string PrimaryTableColumnName { get; set; } = string.Empty;
        public string ForeignTableName { get; set; } = string.Empty;
        public string ForeignTableColumnName { get; set; } = string.Empty;

        [XmlIgnore]
		public SchemaViewModel PrimaryColumn { get; set; } = new();

        [XmlIgnore]
        public SchemaViewModel ForeignColumn { get; set; } = new();

        #endregion
    }
}
