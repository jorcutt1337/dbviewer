namespace DBViewer.WPF.Models
{
    /// <summary>
    /// Model Used To Serialize / Deserialize Schema Information To / From XML
    /// </summary>
    public class SchemaXmlModel
    {
        public List<SchemaViewModel> Columns { get; set; } = new();
        public List<RelationViewModel> Keys { get; set; } = new();
    }
}
