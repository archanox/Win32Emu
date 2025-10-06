using Avalonia.Controls;
using Avalonia.Interactivity;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Gui.Views;

public partial class GameInfoWindow : Window
{
    public GameInfoWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
