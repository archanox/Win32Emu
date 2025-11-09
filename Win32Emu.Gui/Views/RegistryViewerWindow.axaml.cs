using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Gui.Views;

public partial class RegistryViewerWindow : Window
{
	public RegistryViewerWindow()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
