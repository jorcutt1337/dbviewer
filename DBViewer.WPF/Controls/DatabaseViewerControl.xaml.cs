using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        private List<SchemaViewModel> _Columns = new List<SchemaViewModel>();
        private List<RelationViewModel> _Keys = new List<RelationViewModel>();

        private List<SchemaModel> _Models = new List<SchemaModel>();



        // Datagrid Selection Unit (Row vs Cell vs Both)
        private DataGridSelectionUnit _SelectionUnit = DataGridSelectionUnit.FullRow;

        // Expose the currently selected query result document (if any) so that the MainWindow can perform operations on it (e.g. export to Excel)
        internal LayoutDocument CurrentQueryResultDocument
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

        private void LoadDbInformation()
        {
            try
            {
                List<SchemaViewModel> columns = new List<SchemaViewModel>();
                List<RelationViewModel> keys = new List<RelationViewModel>();

                var worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler((o, ea) =>
                {
                    var directoryPath = System.IO.Directory.GetCurrentDirectory();
                    var dir = new DirectoryInfo(directoryPath);
                    var xmlFile = dir.GetFiles().FirstOrDefault(v => v.Name == Globals.XmlSchemaFilename);

                    if (xmlFile == null)
                    {
                        columns = DataUtility.GetInstances<SchemaViewModel>(SqlConstants.SchemaQuery);
                        keys = DataUtility.GetInstances<RelationViewModel>(SqlConstants.TableRelationsQuery);

                        var data = new SchemaXmlData()
                        {
                            Columns = columns,
                            Keys = keys
                        };

                        var xml = XmlExtenstions.Serialize(data);
                        var xmlFilePath = Path.Combine(dir.FullName, Globals.XmlSchemaFilename);

                        File.WriteAllText(xmlFilePath, xml);
                    }
                    else
                    {
                        var xmlContent = File.ReadAllText(xmlFile.FullName);
                        var data = XmlExtenstions.Deserialize<SchemaXmlData>(xmlContent);
                        columns = data.Columns;
                        keys = data.Keys;
                    }
                });
                worker.RunWorkerCompleted += (o, ea) =>
                {
                    this._Columns = columns;
                    this._Keys = keys;

                    foreach (var column in this._Columns)
                    {
                        column.OtherTableColumns = this._Columns.Where(v => v.ColumnName != column.ColumnName && v.TableName == column.TableName).ToList();
                    }

                    foreach (var key in this._Keys)
                    {
                        var p = this._Columns.FirstOrDefault(v => v.TableName == key.PrimaryTableName && v.ColumnName == key.PrimaryTableColumnName);
                        var c = this._Columns.FirstOrDefault(v => v.TableName == key.ForeignTableName && v.ColumnName == key.ForeignTableColumnName);

                        key.PrimaryColumn = p ?? throw new Exception($"Primary column '{key.PrimaryTableName}.{key.PrimaryTableColumnName}' not found in columns list.");
                        key.ForeignColumn = c ?? throw new Exception($"Foreign column '{key.ForeignTableName}.{key.ForeignTableColumnName}' not found in columns list.");
                    }

                    this._Models = this._Columns.GroupBy(d => new { d.DatabaseName, d.TableName, d.TablePrefix, d.Rows })
                        .Select(d => new SchemaModel()
                        {
                            TableName = d.Key.TableName,
                            Columns = d.ToList(),
                            RelationsDown = this._Keys.Where(k => k.PrimaryTableName == d.Key.TableName).ToList(),
                            RelationsUp = this._Keys.Where(k => k.ForeignTableName == d.Key.TableName).ToList()
                        }).Distinct().ToList();

                    gridAllObjects.ItemsSource = new ObservableCollection<SchemaViewModel>(this._Columns);

                    // Load Database Schema Table / Column Models
                    var dataTables = new ObservableCollection<SchemaViewModel>(
                        this._Columns
                        .GroupBy(d => new { d.DatabaseName, d.TableName, d.TablePrefix, d.Rows })
                        .Select(d => new SchemaViewModel()
                        {
                            DatabaseName = d.Key.DatabaseName,
                            TableName = d.Key.TableName,
                            TablePrefix = d.Key.TablePrefix,
                            Rows = d.Key.Rows
                        }).Distinct().ToList());

                    // Load Database Schema Table Relation Models
                    var tableRelations = new ObservableCollection<RelationViewModel>(
                        this._Keys.OrderBy(v => v.PrimaryTableName).ThenBy(v => v.PrimaryTableColumnName).ThenBy(v => v.ForeignTableName).ThenBy(v => v.ForeignTableColumnName));

                    gridTables.ItemsSource = dataTables;
                    databaseGridAllObjects.ItemsSource = tableRelations;
                };

                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Application '{0}' failed to initialize. Error: '{1}'", nameof(DBViewer), ex.ToString()));
            }
        }

        private void LoadTextEditorHighlighting()
        {
            try
            {
                using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(Globals.AvalonEditHiglightLanguageFilename))
                {
                    using (var reader = new System.Xml.XmlTextReader(stream))
                    {
                        this.txtQuery.SyntaxHighlighting =
                            ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Application '{0}' failed to load AvalonDock highlighting file '{1}'. Error: '{2}'", nameof(DBViewer), Globals.AvalonEditHiglightLanguageFilename, ex.ToString()));
            }
        }

        #endregion

        #region Exposed Events

        // Exposed function that is called from the MainWindow to Initialize the control and load necessary data.
        // This is where we should load the database schema information and any other necessary data, as well as initialize any UI elements (e.g. syntax highlighting in the query text box)
        public void Initialize()
        {
            this.LoadDbInformation();

            this.LoadTextEditorHighlighting();
        }

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


        #region Buttons

        private void btnFindTablesContainingColumn_Click(object sender, RoutedEventArgs e)
        {
            if (gridColumns.SelectedItem == null) { return; }

            var entry = (SchemaViewModel)gridColumns.SelectedItem;
            this.rbColumns.IsChecked = true;
            this.allObjectsSearchCtl.txtSearch.Text = entry.ColumnName;
            this.docAllObjects.IsSelected = true;
        }

        private void btnGoToTable_Click(object sender, RoutedEventArgs e)
        {
            if (gridAllObjects.SelectedItem == null) { return; }

            var entry = (SchemaViewModel)gridAllObjects.SelectedItem;
            this.rbTables.IsChecked = true;
            this.allObjectsSearchCtl.txtSearch.Text = entry.TableName;
            this.docAllObjects.IsSelected = true;
        }

        private void btnQueryTable_Click(object sender, RoutedEventArgs e)
        {
            if (gridTables.SelectedItem == null) { return; }

            var entry = (SchemaViewModel)gridTables.SelectedItem;
            var query = SqlConstants.SelectTop1000 + " * FROM " + entry.TableName;

            this.ExecuteQuery(query);
        }

        private void BtnExecuteQuery_Click(object sender, RoutedEventArgs e)
        {
            this.ExecuteQuery(this.txtQuery.Text);
        }

        #endregion

        #region DataGrids

        private void gridTables_SelectionChanged(object sender, SelectionChangedEventArgs e) => SelectedTableChanged();

        #endregion

        #region Search

        private void tableSearchCtl_TextChanged(object sender, EventArgs e)
        {
            // Validation
            if (gridTables.ItemsSource == null) { return; }

            ApplySearchFilter<SchemaViewModel>(
                    gridAllObjects.ItemsSource,
                    obj =>
                    {
                        bool tableMatch = obj.TableName?.Contains(this.allObjectsSearchCtl.SearchString, StringComparison.OrdinalIgnoreCase) == true;
                        return tableMatch;
                    });
        }

        private void AllObjectsSearchTextChanged(object sender = null, EventArgs e = null)
        {
            // Validation
            if (gridAllObjects.ItemsSource == null) { return; }

            var data = this._Columns
                .GroupBy(v => new { v.TableName, v.ColumnName }).Select(v => v.First()).Distinct()
                .OrderBy(v => v.TableName).ThenBy(v => v.ColumnName).ToList();

            gridAllObjects.ItemsSource = data;

            ICollectionView view = CollectionViewSource.GetDefaultView(gridAllObjects.ItemsSource);

            ApplySearchFilter<SchemaViewModel>(
                    gridAllObjects.ItemsSource,
                    obj =>
                    {
                        bool tableMatch = obj.TableName?.Contains(this.allObjectsSearchCtl.SearchString, StringComparison.OrdinalIgnoreCase) == true;
                        bool columnMatch = obj.ColumnName?.Contains(this.allObjectsSearchCtl.SearchString, StringComparison.OrdinalIgnoreCase) == true;
                        return (rbTablesAndColumns.IsChecked == true && (tableMatch || columnMatch))
                            || (rbTables.IsChecked == true && tableMatch)
                            || (rbColumns.IsChecked == true && columnMatch);
                    });
        }

        private void ApplySearchFilter<T>(object itemsSource, Func<T, bool> predicate)
        {
            // Validation
            if (itemsSource == null) { return; }

            ICollectionView view = CollectionViewSource.GetDefaultView(itemsSource);

            view.Filter = r =>
            {
                if (r is not T obj)
                {
                    return false;
                }

                return predicate(obj);
            };
        }

        #endregion

        #region RadioButtons

        private void rbTables_Checked(object sender, RoutedEventArgs e) => AllObjectsSearchTextChanged();
        private void rbColumns_Checked(object sender, RoutedEventArgs e) => AllObjectsSearchTextChanged();
        private void rbTablesAndColumns_Checked(object sender, RoutedEventArgs e) => AllObjectsSearchTextChanged();

        #endregion

        #region Checkbox

        private void chkAutoGenerateQueryJoins_Unchecked(object sender, RoutedEventArgs e) => SelectedTableChanged();
        private void chkAutoGenerateQueryJoins_Checked(object sender, RoutedEventArgs e) => SelectedTableChanged();

        #endregion

        #region Functions

        private void SelectedTableChanged()
        {
            if (gridTables.SelectedItem == null) { return; }

            const string TB = "\t";
            const string RN = "\r\n";
            const string RNT = RN + TB;

            var singleLine = chkAutoGenerateSelects1Line.IsChecked == true;
            var SLTT = (singleLine == false ? string.Empty : TB + TB);

            var table = (SchemaViewModel)this.gridTables.SelectedItem;
            var tableModel = this._Models.FirstOrDefault(m => m.TableName == table.TableName);

            var relatedTableColumnsSelect = string.Empty;
            var relatedTableJoins = new List<string>();

            var idx = 2;

            if (tableModel == null) { throw new InvalidOperationException("Table model not found."); }

            foreach (var key in tableModel.RelationsUp)
            {
                var relatedTableColumns = new List<SchemaViewModel>() { key.PrimaryColumn }.Concat(key.PrimaryColumn.OtherTableColumns).Distinct().ToList();

                relatedTableJoins.Add(TB + "INNER JOIN " + key.PrimaryTableName + " X" + idx + " ON " + "X" + idx + "." + key.PrimaryTableColumnName + " = " + "X." + key.ForeignTableColumnName);

                relatedTableColumnsSelect += RN + SLTT + string.Join("," + (singleLine == false ? RN + SLTT : " "), relatedTableColumns.OrderBy(x => x.OrdinalPosition).Select(x => (singleLine == false ? TB + TB : "") + "X" + idx + "." + x.ColumnName));
                relatedTableColumnsSelect += ",";
                idx++;

                Console.WriteLine($"Primary: {key.ForeignTableName}.{key.ForeignTableColumnName} -> Foreign: {key.PrimaryTableName}.{key.PrimaryTableColumnName}");
            }

            foreach (var key in tableModel.RelationsDown)
            {
                var relatedTableColumns = new List<SchemaViewModel>() { key.ForeignColumn }.Concat(key.ForeignColumn.OtherTableColumns).Distinct().ToList();

                relatedTableJoins.Add(TB + "LEFT JOIN " + key.ForeignTableName + " X" + idx + " ON " + "X" + idx + "." + key.ForeignTableColumnName + " = " + "X." + key.PrimaryTableColumnName);

                relatedTableColumnsSelect += RN + SLTT + string.Join("," + (singleLine == false ? RN + SLTT : " "), relatedTableColumns.OrderBy(x => x.OrdinalPosition).Select(x => (singleLine == false ? TB + TB : "") + "X" + idx + "." + x.ColumnName));
                relatedTableColumnsSelect += ",";
                idx++;

                Console.WriteLine($"Primary: {key.PrimaryTableName}.{key.PrimaryTableColumnName} -> Foreign: {key.ForeignTableName}.{key.ForeignTableColumnName}");
            }

            relatedTableColumnsSelect = relatedTableColumnsSelect.TrimEnd(',');

            columnDatabaseName.Visibility = Visibility.Hidden;
            columnTableName.Visibility = Visibility.Hidden;

            var query = "USE " + table.DatabaseName + RN + RN + "\t" + SqlConstants.SelectTop1000 + RN;

            var primaryColumns = tableModel.Columns.OrderBy(v => v.OrdinalPosition).Select(v => (singleLine == false ? TB + TB : " ") + "X." + v.ColumnName).Distinct().ToList();

            if (this.chkAutoGenerateQueryJoins.IsChecked == true)
            {
                query += SLTT + String.Join("," + (singleLine == false ? RN : ""), primaryColumns) +
                (chkInclJoinSelects.IsChecked == false ? "" : (relatedTableColumnsSelect.Replace(RN, "").Length > 0 ? "," + relatedTableColumnsSelect.TrimEnd('\r').TrimEnd('\n').TrimEnd(',') : "")) +
                RNT + "FROM " + table.TableName + " X" +
                RNT + string.Join(RNT, relatedTableJoins);
            }
            else
            {
                query +=
                SLTT + String.Join("," + (singleLine == false ? RN : ""), primaryColumns) +
                RNT + "FROM " + table.TableName + " X";
            }

            this.txtQuery.Text = query + RNT + RNT;
            this.docColumns.Title = table.TableName;

            gridColumns.ItemsSource = new ObservableCollection<SchemaViewModel>(tableModel.Columns);
            columnDatabaseName.Visibility = Visibility.Hidden;
            columnTableName.Visibility = Visibility.Hidden;
        }

        private void ExecuteQuery(string query)
        {
            // Validation
            if (gridTables.SelectedItem == null) { return; }

            var item = (SchemaViewModel)gridTables.SelectedItem;

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