using System.Windows.Controls;

namespace DBViewer.WPF.Controls
{
	public interface ILoadableUserControl
	{
		void Initialize(string key, string connectionString);
		void ResetDisplay();
		void RefreshSchema();
        void DataGridSelectionModeChanged(DataGridSelectionUnit unit);
    }
}
