using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Win32Emu.Gui.Browser;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
