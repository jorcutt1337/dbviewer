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

        // Singleton for global environment variable - May delete later
        public static EnvironmentType Environment = EnvironmentType.PROD;

        // App.config key name for connection string
        public static string DbConnectionKeyName => "DB_" + Environment;

        // Name of file to save db schema information in on 1st load. Read from file on subsequent app launches. Delete file to refresh schema.
        public const string XmlSchemaFilename = "DB_SCHEMA.xml";

        // Name of AvalonEdit highlighting language file
        public const string AvalonEditHiglightLanguageFilename = "DBViewer.WPF.Resources.AvalonEdit.SQL.xshd";
    }
}