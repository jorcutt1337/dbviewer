namespace DBViewer.WPF.Models
{
    public class SchemaViewModel
    {
        #region Properties

        public string DatabaseName { get; set; }

		public string TableFullName => string.Join(".", TablePrefix, TableName);
        public string TableName { get; set; }
        public string TablePrefix { get; set; }

        public string ColumnName { get; set; }
        public int OrdinalPosition { get; set; }
        public string DataType { get; set; }
        public int? MaximumLength { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }
        public string ColumnDefault { get; set; }

        public long Rows { get; set; }

        #endregion
    }
}
