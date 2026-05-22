namespace DBViewer.WPF.Models
{
    public class SchemaXmlData
    {
        public List<SchemaViewModel> Columns { get; set; }
        public List<RelationViewModel> Keys { get; set; }
    }

    public class SchemaModel
    {
        public string TableName { get; set; }

        public List<SchemaViewModel> Columns { get; set; } = new List<SchemaViewModel>();

        public List<RelationViewModel> RelationsDown { get; set; } = new List<RelationViewModel>();
        public List<RelationViewModel> RelationsUp { get; set; } = new List<RelationViewModel>();

        public SchemaModel() { }
    }
}
