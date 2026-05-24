using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DBViewer.WPF.Controls;
using DBViewer.WPF.Dialogs;
using DBViewer.WPF.Extensions;
using Microsoft.Win32;

namespace DBViewer.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties

        // Collection of UI Tabs / UserControls
        private List<ILoadableUserControl> _UserControls = new();

        #endregion

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();

            this.LoadControls();
        }

        private async void LoadControls()
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                // DataGridSelectionUnit ComboBox
                this.cboSelectionUnit.Items.Add(DataGridSelectionUnit.FullRow);
                this.cboSelectionUnit.Items.Add(DataGridSelectionUnit.Cell);
                this.cboSelectionUnit.Items.Add(DataGridSelectionUnit.CellOrRowHeader);
                this.cboSelectionUnit.SelectedItem = DataGridSelectionUnit.FullRow;

                var loading = new LoadingDialog();
                loading.Show();

                try
                {
                    this._UserControls = new List<ILoadableUserControl>();

                    for (var i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
                    {
                        var key = ConfigurationManager.ConnectionStrings[i];

                        if (key.ConnectionString == "data source=.\\SQLEXPRESS;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|aspnetdb.mdf;User Instance=true") { continue; }

                        var control = new DatabaseViewerControl() { Name = key.Name };
                        this._UserControls.Add(control);

                        var headerPanel = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal
                        };

                        headerPanel.Children.Add(new Image()
                        {
                            Width = 17,
                            Height = 17,
                            Source = new BitmapImage(new Uri("pack://application:,,,/Images/VS/CPPSQLProject_16x.png")),
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 6, 0)
                        });

                        headerPanel.Children.Add(new TextBlock()
                        {
                            Text = key.Name,
                            FontWeight = FontWeights.SemiBold,
                            FontSize = 12,
                            VerticalAlignment = VerticalAlignment.Center
                        });

                        var tabItem = new TabItem()
                        {
                            IsEnabled = true,
                            Header = headerPanel,
                            TabIndex = i,
                            Content = control
                        };
                        this.tabControl.Items.Add(tabItem);

                        try
                        {
                            loading.SetProgress(i, $"Loading Connection " + key.Name + " - " + (i + 1) + " / " + (Math.Round((i + 1) * 1.00 / ConfigurationManager.ConnectionStrings.Count, 0)));
                            control.Initialize(key.Name, key.ConnectionString);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error loading connection string '{key}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.tabControl.Items.Remove(control);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading controls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    loading.Close();
                }


                await Task.Delay(500);
                foreach (var uc in this._UserControls)
                {
                    uc.ResetDisplay();
                }                
            }
        }

        #endregion

        #region TabControl

        private async void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (ILoadableUserControl)this.tabControl.SelectedContent;
            control.ResetDisplay();
        }

        #endregion

        #region ComboBox

        private void CboEnvironment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cboEnvironment.SelectedIndex == -1) { return; }
            //Globals.Environment = (Globals.EnvironmentType)cboEnvironment.SelectedItem;

            //if (this._UserControls == null) { return; }

            //foreach (var control in this._UserControls)
            //{
            //    control.EnvironmentChanged();
            //}
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
            if (this.tabControl.SelectedContent == null)
            {
                MessageBox.Show("Error");
                return;
            }

            var doc = ((DatabaseViewerControl)this.tabControl.SelectedContent).CurrentQueryResultDocument;
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
                    var csv = table.ToCSV();

                    SaveFileDialog saveFileDialog = new SaveFileDialog() { DefaultExt = ".csv", Filter = "CSV|*.csv" };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllText(saveFileDialog.FileName, csv);
                    }

                    return;
                }
            }
        }

        private void MenuTools_RefreshSchema_Click(object sender, RoutedEventArgs e)
        {
            if (this._UserControls == null) { return; }
            foreach (var control in this._UserControls)
            {
                control.RefreshSchema();
            }
        }

        #endregion
    }
}