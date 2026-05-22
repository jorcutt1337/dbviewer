using System.Xml.Serialization;

namespace DBViewer.WPF.Models
{
	public class RelationViewModel
	{
		#region Properties

		public string PrimaryTableName { get; set; }
		public string PrimaryTableColumnName { get; set; }
		public string ForeignTableName { get; set; }
		public string ForeignTableColumnName { get; set; }

		[XmlIgnore]
		public SchemaViewModel PrimaryColumn { get; set; } = new();

        [XmlIgnore]
        public SchemaViewModel ForeignColumn { get; set; } = new();

        #endregion
    }
}
