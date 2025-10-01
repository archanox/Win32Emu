using Avalonia.Controls;
using Avalonia.Input;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Gui.Views;

public partial class EmulatorWindow : Window
{
    public EmulatorWindow()
    {
        InitializeComponent();
        
        // Handle F12 key to toggle debug panel
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F12 && DataContext is EmulatorWindowViewModel viewModel)
        {
            viewModel.ShowDebugPanel = !viewModel.ShowDebugPanel;
            e.Handled = true;
        }
    }
}
