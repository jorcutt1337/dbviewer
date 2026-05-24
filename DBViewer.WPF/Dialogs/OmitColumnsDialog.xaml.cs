using System.Windows;

namespace DBViewer.WPF.Dialogs
{
    /// <summary>
    /// Interaction logic for OmitColumnsDialog.xaml
    /// </summary>
    public partial class OmitColumnsDialog : Window
	{
		#region Properties

		public List<string> ColumnsToOmit
		{
			get
			{
				return txtColumnsNames.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
					.Select(c => c.Trim()).Distinct().Order().ToList();
			}
        }

		#endregion

		#region Initialization

		public OmitColumnsDialog(List<string> columnsToOmit)
		{
			InitializeComponent();

			this.LoadTextEditorHighlighting();

			this.Title = "Omit Columns";

			this.txtColumnsNames.Text = string.Join("\r\n", columnsToOmit);
		}

		#endregion

		#region Functions

		private void LoadTextEditorHighlighting()
		{
			using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(Globals.AvalonEditHiglightLanguageFilename))
			{
				using (var reader = new System.Xml.XmlTextReader(stream))
				{
					this.txtColumnsNames.SyntaxHighlighting =
						ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
						ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
				}
			}
		}

		#endregion

		#region Button

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{			
			this.DialogResult = true;
			this.Close();
		}
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		#endregion
	}
}
