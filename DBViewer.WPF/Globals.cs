namespace DBViewer.WPF
{
    /// <summary>
    /// Global information class
    /// </summary>
    public static class Globals
    {
        public enum DatagridSelectionUnit
        {
            FullRow,
            Cell,
            CellOrRowHeader
        }

        public enum EnvironmentType
        {
            DEV,
            PROD
        }

        public static EnvironmentType Environment = EnvironmentType.PROD;

        public static string DB_CONNECTION
        {
            get
            {
                return "DB_" + Environment.ToString();
            }
        }

        public const string WPF_XML_SCHEMA_FILENAME = "DB_SCHEMA.xml";
        public const string WPF_AVALONEDIT_HIGHLIGHT_LANGUAGE_FILE = "DBViewer.WPF.Resources.AvalonEdit.SQL.xshd";
    }
}