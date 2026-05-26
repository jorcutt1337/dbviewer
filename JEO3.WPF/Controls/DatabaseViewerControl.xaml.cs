using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using JEO3.Data;
using JEO3.SchemaEngine;
using JEO3.SchemaEngine.Models;
using JEO3.WPF.Dialogs;
using JEO3.WPF.Extensions;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace JEO3.WPF.Controls
{
    /// <summary>
    /// Interaction logic for DatabaseViewerControl.xaml
    /// </summary>
    public partial class DatabaseViewerControl : UserControl, ILoadableUserControl
    {
        #region Properties

        private string _AppSettingsKeyName = string.Empty;
        private string _ConnectionString = string.Empty;
        private string _DatabaseName = string.Empty;

        // Database Schema Information
        private SchemaContext _Context { get; set; }

        private List<string> _ColumnsToOmit = Globals.DefaultColumnsToIgnore;

        // Datagrid Selection Unit (Row vs Cell vs Both)
        private DataGridSelectionUnit _SelectionUnit = DataGridSelectionUnit.FullRow;

        // Expose the currently selected query result document (if any) so that the MainWindow can perform operations on it (e.g. export to Excel)
        internal LayoutDocument CurrentQueryResultDocument
        {
            get
            {
                if (this.pnQueryResults.SelectedContentIndex == -1) { return null; }

                return (LayoutDocument)pnQueryResults.SelectedContent;
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

        private async void LoadDbInformation()
        {
            try
            {
                // Forget Saving & Reloading Schema To/From File For Now. Revisit Later
                // Just Moved Everything Into JEO3.Data and JEO3.SchemaEngine
                var directoryPath = System.IO.Directory.GetCurrentDirectory();
                var dir = new DirectoryInfo(directoryPath);
                var xmlFile = dir.GetFiles().FirstOrDefault(v => v.Name == _AppSettingsKeyName);
                //if (xmlFile == null || Globals.RefreshSchemaOnEveryStart)
                //{
                    this._Context = new SchemaEngine.SchemaContext(_ConnectionString);
                    await _Context.LoadSchema();

                    var xml = XmlExtenstions.Serialize(this._Context.Schema);
                    var xmlFilePath = Path.Combine(dir.FullName, _AppSettingsKeyName);

                    File.WriteAllText(xmlFilePath, xml);
                //}
                //else
                //{
                //    var xmlContent = File.ReadAllText(xmlFile.FullName);
                //    var data = XmlExtenstions.Deserialize<SchemaModel>(xmlContent);

                //    this._Context = new SchemaEngine.SchemaContext(_ConnectionString, data);
                //}

                gridTables.ItemsSource = new ObservableCollection<SchemaTable>(_Context.Schema.Tables);
                    gridAllObjects.ItemsSource = new ObservableCollection<SchemaColumn>(_Context.Schema.Columns);
                    gridTableRelations.ItemsSource = new ObservableCollection<SchemaRelation>(_Context.Schema.Relations);

                    this.AutoSizeLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Application '{0}' failed to initialize. Error: '{1}'", nameof(JEO3), ex.ToString()));
                throw;
            }
        }
        private void AutoSizeLayout()
        {
            double maxWidth = 0;

            foreach (SchemaTable item in gridTables.ItemsSource)
            {
                double width = DisplayHelper.MeasureTextWidth(
                    item.TableName,
                    gridTables.FontFamily,
                    gridTables.FontSize,
                    FontStyles.Normal,
                    FontWeights.SemiBold,
                    FontStretches.Normal);

                if (width > maxWidth)
                {
                    maxWidth = width;
                }
            }

            // Add padding for cell margins/sort glyph/etc.
            maxWidth += 30;

            this.pnTables.DockWidth = new GridLength(maxWidth);
            lpRight.DockWidth = new GridLength(750);
            lpLeftSide.DockWidth = new GridLength(1300);
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
                Debug.WriteLine(string.Format("Application '{0}' failed to load AvalonDock highlighting file '{1}'. Error: '{2}'", nameof(JEO3), Globals.AvalonEditHiglightLanguageFilename, ex.ToString()));
            }
        }

        #endregion

        #region Exposed Events

        // Exposed function that is called from the MainWindow to Initialize the control and load necessary data.
        // This is where we should load the database schema information and any other necessary data, as well as initialize any UI elements (e.g. syntax highlighting in the query text box)
        public void Initialize(string key, string connectionString)
        {
            _AppSettingsKeyName = key;
            _ConnectionString = connectionString;
            this.Name = _AppSettingsKeyName;
            this.ToolTip = _ConnectionString;

            this.LoadDbInformation();

            this.LoadTextEditorHighlighting();
        }

        public void ResetDisplay()
        {
            lpLeftSide.DockWidth = new GridLength(1300);
        }

        public void RefreshSchema()
        {
            // ConnectionStrings
        }

        public void DataGridSelectionModeChanged(DataGridSelectionUnit unit)
        {
            if (pnQueryResults.Children.Count == 0) { return; }

            this._SelectionUnit = unit;

            try
            {
                for (var i = 0; i < pnQueryResults.Children.Count; i++)
                {
                    var doc = (LayoutDocument)pnQueryResults.Children[i];
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

            var entry = (SchemaColumn)gridColumns.SelectedItem;
            this.rbColumns.IsChecked = true;
            this.allObjectsSearchCtl.txtSearch.Text = entry.ColumnName;
            this.docAllObjects.IsSelected = true;
        }

        private void btnGoToTable_Click(object sender, RoutedEventArgs e)
        {
            if (gridAllObjects.SelectedItem == null) { return; }

            var entry = (SchemaColumn)gridAllObjects.SelectedItem;
            this.rbTables.IsChecked = true;
            this.allObjectsSearchCtl.txtSearch.Text = entry.TableName;
            this.docAllObjects.IsSelected = true;
        }

        private void btnOpenOmitColumnsDialog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OmitColumnsDialog(this._ColumnsToOmit);
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value == true && dialog.ColumnsToOmit != null)
            {
                this._ColumnsToOmit = dialog.ColumnsToOmit.Order().ToList();

                 // Trigger Query Refresh
                 SelectedTableChanged();
            }
        }

        private async void btnQueryTable_Click(object sender, RoutedEventArgs e)
        {
            if (gridTables.SelectedItem == null) { return; }

            var entry = (SchemaTable)gridTables.SelectedItem;
            var query = SqlQueries.SelectTop1000 + " * FROM " + entry.TableName;

            this.ExecuteQuery(query);
        }

        private async void BtnExecuteQuery_Click(object sender, RoutedEventArgs e)
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

            ApplySearchFilter<SchemaColumn>(
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

            var data = this._Context.Schema.Columns
                .GroupBy(v => new { v.TableName, v.ColumnName }).Select(v => v.First()).Distinct()
                .OrderBy(v => v.TableName).ThenBy(v => v.ColumnName).ToList();

            gridAllObjects.ItemsSource = data;

            ICollectionView view = CollectionViewSource.GetDefaultView(gridAllObjects.ItemsSource);

            ApplySearchFilter<SchemaColumn>(
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

        private async void SelectedTableChanged()
        {
            if (gridTables.SelectedItem == null) { return; }

            var startTime = DateTime.Now;

            // Selected Table
            var table = (SchemaTable)this.gridTables.SelectedItem;

            // Generate Query
            var query = await this._Context.GenerateQuery(table.TableName, new QueryOptions(chkAutoGenerateQueryJoins.IsChecked.GetValueOrDefault(),
                chkInclJoinSelects.IsChecked.GetValueOrDefault(),
                chkAutoGenerateQueryJoins.IsChecked.GetValueOrDefault(),
                nudDefaultRelationsipsDown.Value.GetValueOrDefault(),
                nudDefaultRelationsipsUp.Value.GetValueOrDefault()));

            txtQuery.Text = query;
            docColumns.Title = table.TableName;
            gridColumns.ItemsSource = new ObservableCollection<SchemaColumn>(table.Columns);

            Debug.WriteLine("Time To Generate Query - " + (decimal)Math.Round((DateTime.Now - startTime).TotalSeconds, 3) + " seconds");
        }

        record QueryGen(string PrimaryTableName, string ForeignTableName, string Selects, string Joins);

        private async void ExecuteQuery(string query)
        {
            // Validation
            if (gridTables.SelectedItem == null) { return; }

            var item = (SchemaTable)gridTables.SelectedItem;

            try
            {
                var table = await DataUtility.GetDataTable(query, _ConnectionString);

                if (table == null) { return; }

                if (pnQueryResults.Children.Count == 1 && this.dataGridQueryResults.DataContext == null)
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
            LayoutDocumentQuery doc = new LayoutDocumentQuery(query) { Title = table.Rows.Count + " - " + tableName, ContentId = "document" + (pnQueryResults.Children.Count + 1), IconSource = docTables.IconSource };
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
            pnQueryResults.Children.Add(doc);

            dGrid.ItemsSource = table.AsDataView();
            pnQueryResults.SelectedContentIndex = pnQueryResults.Children.Count - 1;
        }

        #endregion
    }
}