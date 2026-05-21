using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DBViewer.WPF.Models;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace DBViewer.WPF.Controls
{
    /// <summary>
    /// Interaction logic for DatabaseViewerControl.xaml
    /// </summary>
    public partial class DatabaseViewerControl : UserControl, ILoadableUserControl
    {
        #region Properties

        // Database Schema Information
        private List<DatabaseViewModel> _Columns = new List<DatabaseViewModel>();
        private List<TableColumnRelation> _Keys = new List<TableColumnRelation>();

        // Datagrid Selection Unit (Row vs Cell vs Both)
        private DataGridSelectionUnit _SelectionUnit = DataGridSelectionUnit.FullRow;

        // Expose the currently selected query result document (if any) so that the MainWindow can perform operations on it (e.g. export to Excel)
        public LayoutDocument CurrentQueryResultDocument
        {
            get
            {
                if (this.docPaneQueryResults.SelectedContentIndex == -1) { return null; }

                return (LayoutDocument)docPaneQueryResults.SelectedContent;
            }
        }

        #endregion

        #region Initialization

        public DatabaseViewerControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Load

        // Exposed function that is called from the MainWindow to Initialize the control and load necessary data.
        // This is where we should load the database schema information and any other necessary data, as well as initialize any UI elements (e.g. syntax highlighting in the query text box)
        public void Initialize()
        {
            this.LoadDbInformation();

            this.LoadTextEditorHighlighting();
        }

        private void LoadDbInformation()
        {
            List<DatabaseViewModel> columns = new List<DatabaseViewModel>();
            List<TableColumnRelation> keys = new List<TableColumnRelation>();

            var worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler((o, ea) =>
            {
                var directoryPath = System.IO.Directory.GetCurrentDirectory();
                var dir = new DirectoryInfo(directoryPath);
                var xmlFile = dir.GetFiles().FirstOrDefault(v => v.Name == Globals.WPF_XML_SCHEMA_FILENAME);

                if (xmlFile == null)
                {
                    columns = DatabaseViewModel.GetInstances();
                    keys = TableColumnRelation.GetInstances();

                    var data = new SchemaXmlData()
                    {
                        Columns = columns,
                        Keys = keys
                    };

                    var xml = XmlExtenstions.Serialize(data);
                    var xmlFilePath = Path.Combine(dir.FullName, Globals.WPF_XML_SCHEMA_FILENAME);

                    File.WriteAllText(xmlFilePath, xml);
                }
                else
                {
                    var xmlContent = File.ReadAllText(xmlFile.FullName);
                    var data = XmlExtenstions.Deserialize<SchemaXmlData>(xmlContent);
                    columns = data.Columns;
                    keys = data.Keys;
                }

                // Meant for SQL COLUMN_DEFAULT values that contain pipe characters which messes with the XML serialization.
                // Replace pipe characters with dashes and trim whitespace to mitigate this issue. This is not ideal but it is a very rare occurrence and it is only for default values which are not surfaced in the UI so it should be fine.
                foreach (var col in columns)
                {
                    if (col.PredefinedAcceptableValues == null || col.PredefinedAcceptableValues.Trim().Length == 0) { continue; }
                    col.PredefinedAcceptableValues = col.PredefinedAcceptableValues.Trim().Replace("|", "-");
                }
            });
            worker.RunWorkerCompleted += (o, ea) =>
            {
                this._Columns = columns;
                this._Keys = keys;

                var x = new ObservableCollection<DatabaseViewModel>(this._Columns);
                databaseGrid.ItemsSource = x;

                // Load Database Schema Table / Column Models
                var dataTables = new ObservableCollection<DatabaseViewModel>(
                    x
                    .GroupBy(d => new { d.DatabaseName, d.TableName, d.TableDescription, d.TablePrefix, d.Rows })
                    .Select(d => new DatabaseViewModel()
                    {
                        DatabaseName = d.Key.DatabaseName,
                        TableName = d.Key.TableName,
                        TableDescription = d.Key.TableDescription,
                        TablePrefix = d.Key.TablePrefix,
                        Rows = d.Key.Rows
                    }).Distinct().ToList());

                // Load Database Schema Table Relation Models
                var tableRelations = new ObservableCollection<TableColumnRelation>(
                    this._Keys.OrderBy(v => v.PrimaryTableName).ThenBy(v => v.PrimaryTableColumnName).ThenBy(v => v.ForeignTableName).ThenBy(v => v.ForeignTableColumnName));

                dataGridTables.ItemsSource = dataTables;
                databaseGridAllObjects.ItemsSource = tableRelations;
            };

            worker.RunWorkerAsync();
        }

        private void LoadTextEditorHighlighting()
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(Globals.WPF_AVALONEDIT_HIGHLIGHT_LANGUAGE_FILE))
            {
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    this.txtQuery.SyntaxHighlighting =
                        ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                        ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }
        }

        #endregion


        #region Buttons

        private void btnFindTablesContainingColumn_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (dataGridColumns.SelectedItem == null) { return; }

            var column = (DatabaseViewModel)this.dataGridColumns.SelectedItem;

            this.rbColumns.IsChecked = true;
            this.allObjectsSearchCtl.txtSearch.Text = column.ColumnName;
            this.docAllObjects.IsSelected = true;
        }

        private void btnGoToTable_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (databaseGrid.SelectedItem == null) { return; }

            var entry = (DatabaseViewModel)this.databaseGrid.SelectedItem;

            this.rbTables.IsChecked = true;
            this.allObjectsSearchCtl.txtSearch.Text = entry.TableName;

            this.docAllObjects.IsSelected = true;
        }

        private void btnQueryTable_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (dataGridTables.SelectedItem == null) { return; }

            var item = (DatabaseViewModel)this.dataGridTables.SelectedItem;
            var query = "SELECT TOP 1000 * FROM " + item.TableName;

            this.ExecuteQuery(query);
        }

        private void BtnExecuteQuery_Click(object sender, RoutedEventArgs e)
        {
            this.ExecuteQuery(this.txtQuery.Text);
        }

        #endregion

        #region DataGrids

        private void dataGridTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Validation
            if (dataGridTables.SelectedItem == null) { return; }

            const string RN = "\r\n";
            const string RNT = RN + "\t";

            var table = (DatabaseViewModel)this.dataGridTables.SelectedItem;
            var tableColumns = this._Columns.Where(r => r.TableName == table.TableName).Distinct().ToList();

            // Ident
            var col = this._Columns.Where(v => v.TableName == table.TableName && v.IsIdentity).FirstOrDefault();
            var data = this._Columns.Where(r => r.ColumnName == col?.ColumnName).Distinct().ToList();

            if (data == null) { return; }

            this.dataGridColumns.ItemsSource = new ObservableCollection<DatabaseViewModel>(data);

            var relatedTables = this._Keys
                .Where(v => v.PrimaryTableName == table.TableName && v.ForeignTableName != table.TableName)
                .GroupBy(v => new { v.ForeignTableName }).ToList();
            var relatedTableNames = relatedTables.Select(v => v.Key.ForeignTableName).Distinct().ToList();
            var relatedTableColumns = this._Columns.Where(v => relatedTableNames.Contains(v.TableName)).GroupBy(v => new { v.TableName }).ToList();

            var tableJoins = relatedTables.Select((v, i) => new
            {
                Clause = "\tLEFT JOIN " + v.Key.ForeignTableName + " X" + (i + 1) + " ON " + string.Join(" AND ", v.Select(x => "X" + (i + 1) + "." + x.ForeignTableColumnName + " = X." + col.ColumnName).Distinct())
            }).ToList();

            var relatedTableColumnsSelect = RN + string.Join("," + RN, relatedTableColumns.Select((v, i) => string.Join("," + "\r\n", v.OrderBy(x => x.OrdinalPosition).Select(x => "\t\t" + "X" + (i + 1) + "." + x.ColumnName)))).TrimEnd('\r').TrimEnd('\n').TrimEnd(',');

            columnDatabaseName.Visibility = Visibility.Hidden;
            columnTableName.Visibility = Visibility.Hidden;

            var query = "USE " + table.DatabaseName + RN + RN + RN;


            if (this.chkAutoGenerateQueryJoins.IsChecked == true)
            {
                query +=
                "\t" + "SELECT TOP 1000" + RN +
                String.Join("," + RN, tableColumns.OrderBy(v => v.OrdinalPosition).Select(v => "\t\t" + "X." + v.ColumnName).Distinct().ToList()) +
                (relatedTableColumnsSelect.Replace(RN, "").Length > 0 ? "," + relatedTableColumnsSelect.TrimEnd('\r').TrimEnd('\n').TrimEnd(',') : "") +
                RNT + "FROM " + table.TableName + " X" +
                RNT + string.Join(RNT, tableJoins.Select(v => v.Clause)) +
                RNT +
                RNT;
            }
            else
            {
                data = this._Columns.Where(r => r.TableName == table.TableName).Distinct().ToList();
                query +=
                "\t" + "SELECT TOP 1000" + RN +
                String.Join("," + RN, data.Where(v => v.ColumnName != null && v.ColumnName != string.Empty).OrderBy(v => v.OrdinalPosition).Select(v => "\t\t" + "X." + v.ColumnName)) +
                RNT + "FROM " + table.TableName + " X" +
                RNT + "" +
                RNT;
            }
            this.txtQuery.Text = query;

            query += RNT + RNT + RNT;

            columnDatabaseName.Visibility = Visibility.Hidden;
            columnTableName.Visibility = Visibility.Hidden;

            this.dataGridColumns.ItemsSource = new ObservableCollection<DatabaseViewModel>(tableColumns);

            this.txtQuery.Text = query;
            this.docColumns.Title = table.TableName;
        }

        #endregion

        #region Search

        private void allObjectsSearchCtl_TextChanged(object sender, EventArgs e)
        {
            // Validation
            if (this.databaseGrid.ItemsSource == null) { return; }

            var data = this._Columns
                .GroupBy(v => new { v.TableName, v.ColumnName }).Select(v => v.First()).Distinct()
                .OrderBy(v => v.TableName).ThenBy(v => v.ColumnName).ToList();
         
            this.databaseGrid.ItemsSource = data;

            ICollectionView view = CollectionViewSource.GetDefaultView(this.databaseGrid.ItemsSource);

            // Filter Facilities
            view.Filter = r =>
             {
                 DatabaseViewModel obj = r as DatabaseViewModel;
                 if (
                 (rbTablesAndColumns.IsChecked == true
                     && ((obj.TableName != null && obj.TableName.ToLower().Contains(this.allObjectsSearchCtl.SearchString.ToLower()))
                     || (obj.ColumnName != null && obj.ColumnName.ToLower().Contains(this.allObjectsSearchCtl.SearchString.ToLower())))
                 )
                 || (rbTables.IsChecked == true && (obj.TableName != null && obj.TableName.ToLower().Contains(this.allObjectsSearchCtl.SearchString.ToLower())))
                 || (rbColumns.IsChecked == true && obj.ColumnName != null && obj.ColumnName.ToLower().Contains(this.allObjectsSearchCtl.SearchString.ToLower())))
                 {
                     return true;
                 }
                 else
                 {
                     return false;
                 }
             };
        }

        private void tableSearchCtl_TextChanged(object sender, EventArgs e)
        {
            // Validation
            if (this.dataGridTables.ItemsSource == null) { return; }

            ICollectionView view = CollectionViewSource.GetDefaultView(this.dataGridTables.ItemsSource);

            // Filter Facilities
            view.Filter = r =>
            {
                DatabaseViewModel obj = r as DatabaseViewModel;
                if (
                (obj.TableName != null && (
                    obj.TableName.ToLower().Contains(this.tableSearchCtl.SearchString.ToLower())
                    || obj.TableDescription.ToLower().Contains(this.tableSearchCtl.SearchString.ToLower()))))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };
        }

        #endregion

        #region RadioButtons

        private void rbTables_Checked(object sender, RoutedEventArgs e)
        {
            this.allObjectsSearchCtl_TextChanged(null, null);
        }

        private void rbColumns_Checked(object sender, RoutedEventArgs e)
        {
            this.allObjectsSearchCtl_TextChanged(null, null);
        }

        private void rbTablesAndColumns_Checked(object sender, RoutedEventArgs e)
        {
            this.allObjectsSearchCtl_TextChanged(null, null);
        }

        #endregion

        #region Checkbox

        private void ChkAutoGenerateQueryJoins_Unchecked(object sender, RoutedEventArgs e)
        {
            dataGridTables_SelectionChanged(null, null);
        }

        private void ChkAutoGenerateQueryJoins_Checked(object sender, RoutedEventArgs e)
        {
            dataGridTables_SelectionChanged(null, null);
        }

        #endregion

        #region Exposed Events

        public void EnvironmentChanged()
        {
        }

        public void DataGridSelectionModeChanged(DataGridSelectionUnit unit)
        {
            if (docPaneQueryResults.Children.Count == 0) { return; }

            this._SelectionUnit = unit;

            try
            {
                for (var i = 0; i < docPaneQueryResults.Children.Count; i++)
                {
                    var doc = (LayoutDocument)docPaneQueryResults.Children[i];
                    var grid = (Grid)doc.Content;
                    var datagrid = grid.FindLogicalChildren<DataGrid>().FirstOrDefault();
                    if (datagrid != null)
                    {
                        datagrid.SelectionUnit = (DataGridSelectionUnit)unit;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region Functions

        private void ExecuteQuery(string query)
        {
            // Validation
            if (dataGridTables.SelectedItem == null) { return; }

            var item = (DatabaseViewModel)this.dataGridTables.SelectedItem;

            try
            {
                var table = DataUtility.GetDataTable(query);

                if (table == null) { return; }

                if (docPaneQueryResults.Children.Count == 1 && this.dataGridQueryResults.DataContext == null)
                {
                    this.dataGridQueryResults.DataContext = table.DefaultView;
                    docQueryResults.Title = table.Rows.Count + " - " + item.TableName;
                }
                else
                {
                    AddNewQueryResultSet(table, item.TableName, this.txtQuery.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void AddNewQueryResultSet(DataTable table, string tableName, string query)
        {
            LayoutDocumentQuery doc = new LayoutDocumentQuery(query) { Title = table.Rows.Count + " - " + tableName, ContentId = "document" + (docPaneQueryResults.Children.Count + 1), IconSource = docTables.IconSource };
            Grid grid = new Grid() { Width = Double.NaN, Height = Double.NaN, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            doc.IsSelectedChanged += new EventHandler((sender, e) =>
            {
                this.txtQuery.Text = doc.Query;
            });

            DataGrid dGrid = new DataGrid()
            {
                Width = Double.NaN,
                Height = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 0),
                CanUserAddRows = false,
                AutoGenerateColumns = true,
                IsReadOnly = true,
                AlternationCount = 2,
                AlternatingRowBackground = dataGridQueryResults.AlternatingRowBackground,
                Background = dataGridQueryResults.Background,
                SelectionMode = DataGridSelectionMode.Extended,
                SelectionUnit = DataGridSelectionUnit.FullRow,
                RowHeight = 22,
                FontSize = 12
            };

            dGrid.SelectionUnit = this._SelectionUnit;

            grid.Children.Add(dGrid);
            doc.Content = grid;
            docPaneQueryResults.Children.Add(doc);

            dGrid.ItemsSource = table.AsDataView();
            docPaneQueryResults.SelectedContentIndex = docPaneQueryResults.Children.Count - 1;
        }

        #endregion

    }
}