using Avalonia.Controls;
using Avalonia.Input.Platform;
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

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        // Set up clipboard access for the view model
        if (DataContext is GameInfoViewModel viewModel)
        {
            // Generate the GameDB stub when window opens
            viewModel.CopyGameDbStubCommand.Execute(null);
        }
    }

    private async void CopyGameDbStub_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is GameInfoViewModel viewModel && !string.IsNullOrEmpty(viewModel.GameDbStubJson))
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(viewModel.GameDbStubJson);
                
                // Show a temporary message that the text was copied
                if (sender is Button button)
                {
                    var originalContent = button.Content;
                    button.Content = "✓ Copied!";
                    await Task.Delay(2000);
                    button.Content = originalContent;
                }
            }
        }
    }
}
