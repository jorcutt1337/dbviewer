using System.ComponentModel;
using System.IO;

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

        public const string DefaultSqlAlias = "X";

        private static List<string> _DefaultColumnsToIgnore = null;
        public static List<string> DefaultColumnsToIgnore
        {
            get
            {
                if (_DefaultColumnsToIgnore == null)
                {
                    _DefaultColumnsToIgnore = new List<string>();
                    string columnsToIgnoreConfig = System.Configuration.ConfigurationManager.AppSettings["DefaultColumnsToIgnore"];
                    if (!string.IsNullOrEmpty(columnsToIgnoreConfig))
                    {
                        _DefaultColumnsToIgnore = columnsToIgnoreConfig.Split(',').Select(c => c.Trim())
                            .Distinct().Order().ToList();
                    }
                }
                return _DefaultColumnsToIgnore;
            }
        }


        public static bool RefreshSchemaOnEveryStart =>  bool.Parse(System.Configuration.ConfigurationManager.AppSettings["RefreshSchemaOnEveryStart"] ?? "false");  

        // Name of AvalonEdit highlighting language file
        public static string AvalonEditHiglightLanguageFilename => "DBViewer.WPF.Resources.AvalonEdit.SQL.xshd";
    }
}