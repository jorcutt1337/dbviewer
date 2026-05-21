namespace DBViewer.WPF.Models
{
    public class SchemaXmlData
    {
        public List<DatabaseViewModel> Columns { get; set; }
        public List<TableColumnRelation> Keys { get; set; }
    }
}
