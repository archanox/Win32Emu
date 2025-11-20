using Avalonia.Controls;
using Win32Emu.Gui.Controls;

namespace Win32Emu.Gui.Views;

public partial class EmulatorWindow : Window
{
    public EmulatorWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Get the terminal control for direct access from ViewModel
    /// </summary>
    public TerminalControl? GetTerminalControl()
    {
        return this.FindControl<TerminalControl>("TerminalControl");
    }
}
