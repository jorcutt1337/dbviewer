using System.Windows.Controls;

namespace JEO3.WPF.Controls
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchControl : UserControl
    {
        #region Properties

        public string SearchString
        {
            get
            {
                return this.txtSearch.Text;
            }
        }

        public event EventHandler TextChanged;

        #endregion

        #region Initialization

        public SearchControl()
        {
            InitializeComponent();
        }

        #endregion
        
        #region Events
        
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Validation
            if (TextChanged != null)
            {
                TextChanged(this, null);
            }
        }

        #endregion

        #region Functions

        public void ClearSearch()
        {
            this.txtSearch.Text = string.Empty;
        }

        #endregion
    }
}
