using System.Windows.Controls;

namespace DBViewer.WPF.Controls
{
	public interface ILoadableUserControl
	{
		void Initialize();
		void EnvironmentChanged();
        void DataGridSelectionModeChanged(DataGridSelectionUnit unit);
    }
}
