using Avalonia.Controls;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Gui.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
        
			// Set the storage provider when the window is loaded
			Opened += (_, _) =>
			{
				if (DataContext is MainWindowViewModel viewModel)
				{
					viewModel.SetStorageProvider(StorageProvider);
				}
			};
		}
	}
}