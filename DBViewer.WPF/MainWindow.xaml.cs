using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DBViewer.WPF.Controls;
using Microsoft.Win32;

namespace DBViewer.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties

        // Collection of UI Tabs / UserControls that need to be updated when the environment or DataGridSelectionUnit changes
        private List<ILoadableUserControl> _UserControls;

        #endregion

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();

            this.LoadControls();
        }

        private void LoadControls()
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                // Environment ComboBox
                this.cboEnvironment.Items.Add(Globals.EnvironmentType.DEV);
                this.cboEnvironment.Items.Add(Globals.EnvironmentType.PROD);
                this.cboEnvironment.SelectedItem = Globals.EnvironmentType.PROD;

                // DataGridSelectionUnit ComboBox
                this.cboSelectionUnit.Items.Add(DataGridSelectionUnit.FullRow);
                this.cboSelectionUnit.Items.Add(DataGridSelectionUnit.Cell);
                this.cboSelectionUnit.Items.Add(DataGridSelectionUnit.CellOrRowHeader);
                this.cboSelectionUnit.SelectedItem = DataGridSelectionUnit.FullRow;

                // UserControls
                this._UserControls = new List<ILoadableUserControl>() { this.ucDatabaseViewer };
                foreach (var control in this._UserControls)
                {
                    control.Initialize();
                }
            }
        }

        #endregion

        #region ComboBox

        private void CboEnvironment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboEnvironment.SelectedIndex == -1) { return; }
            Globals.Environment = (Globals.EnvironmentType)cboEnvironment.SelectedItem;

            if (this._UserControls == null) { return; }

            foreach (var control in this._UserControls)
            {
                control.EnvironmentChanged();
            }
        }

        private void cboSelectionUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboSelectionUnit.SelectedIndex == -1) { return; }
            DataGridSelectionUnit unit = (DataGridSelectionUnit)cboSelectionUnit.SelectedItem;

            if (this._UserControls == null) { return; }

            foreach (var control in this._UserControls)
            {
                control.DataGridSelectionModeChanged(unit);
            }
        }

        #endregion

        #region Menu

        private void MenuFile_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuFile_ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            var doc = this.ucDatabaseViewer.CurrentQueryResultDocument;
            if (doc == null) { return; }

            var grid = (Grid)doc.Content;
            foreach (var child in grid.Children)
            {
                if (child.GetType() == typeof(DataGrid))
                {
                    var dGrid = (DataGrid)child;
                    if (dGrid.ItemsSource == null)
                    {
                        // Validation
                        return;
                    }

                    var table = ((DataView)dGrid.ItemsSource).ToTable();
                    var csv = DataUtility.ToCSV(table);

                    SaveFileDialog saveFileDialog = new SaveFileDialog() { DefaultExt = ".csv", Filter = "CSV|*.csv" };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllText(saveFileDialog.FileName, csv);
                    }

                    return;
                }
            }
        }

        #endregion
    }
}