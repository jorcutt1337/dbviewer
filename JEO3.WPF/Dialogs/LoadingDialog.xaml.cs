using System.Windows;

namespace JEO3.WPF.Dialogs
{
    public partial class LoadingDialog : Window
    {
        public LoadingDialog()
        {
            InitializeComponent();
        }

        public void SetProgress(int percent, string? message = null)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = percent;
                PercentTextBlock.Text = $"{percent}%";

                if (!string.IsNullOrWhiteSpace(message))
                {
                    MessageTextBlock.Text = message;
                }
            });
        }

        public void SetIndeterminate(bool isIndeterminate)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.IsIndeterminate = isIndeterminate;
            });
        }
    }
}