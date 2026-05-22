namespace DBViewer.WPF.Models
{
    public class SchemaXmlData
    {
        public List<SchemaViewModel> Columns { get; set; }
        public List<RelationViewModel> Keys { get; set; }
    }
}
