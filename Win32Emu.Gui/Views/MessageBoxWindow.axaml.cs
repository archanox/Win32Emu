using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Win32Emu.Gui.Views;

/// <summary>
/// Avalonia window that displays Win32 MessageBox dialogs
/// </summary>
public partial class MessageBoxWindow : Window
{
    private readonly TaskCompletionSource<int> _resultTcs = new();
    private int _result = 1; // Default to IDOK

    // Win32 MessageBox button constants
    private const int IDOK = 1;
    private const int IDCANCEL = 2;
    private const int IDABORT = 3;
    private const int IDRETRY = 4;
    private const int IDIGNORE = 5;
    private const int IDYES = 6;
    private const int IDNO = 7;

    // Win32 MessageBox type constants
    private const uint MB_OK = 0x00000000;
    private const uint MB_OKCANCEL = 0x00000001;
    private const uint MB_ABORTRETRYIGNORE = 0x00000002;
    private const uint MB_YESNOCANCEL = 0x00000003;
    private const uint MB_YESNO = 0x00000004;
    private const uint MB_RETRYCANCEL = 0x00000005;

    // Icon constants
    private const uint MB_ICONERROR = 0x00000010;
    private const uint MB_ICONQUESTION = 0x00000020;
    private const uint MB_ICONWARNING = 0x00000030;
    private const uint MB_ICONINFORMATION = 0x00000040;

    public MessageBoxWindow()
    {
        InitializeComponent();
    }

    public MessageBoxWindow(string caption, string text, uint type)
    {
        InitializeComponent();
        
        // Set window title
        Title = caption;
        
        // Set message text
        var messageText = this.FindControl<TextBlock>("MessageText");
        if (messageText != null)
        {
            messageText.Text = text;
        }
        
        // Set icon based on type
        SetIcon(type);
        
        // Create buttons based on type
        CreateButtons(type);
    }

    private void SetIcon(uint type)
    {
        var iconText = this.FindControl<TextBlock>("IconText");
        if (iconText == null) return;

        var iconType = type & 0x000000F0;
        
        switch (iconType)
        {
            case MB_ICONERROR:
                iconText.Text = "❌"; // Error/Stop icon
                iconText.Foreground = Brushes.Red;
                iconText.IsVisible = true;
                break;
            case MB_ICONQUESTION:
                iconText.Text = "❓"; // Question icon
                iconText.Foreground = Brushes.Blue;
                iconText.IsVisible = true;
                break;
            case MB_ICONWARNING:
                iconText.Text = "⚠️"; // Warning icon
                iconText.Foreground = Brushes.Orange;
                iconText.IsVisible = true;
                break;
            case MB_ICONINFORMATION:
                iconText.Text = "ℹ️"; // Information icon
                iconText.Foreground = Brushes.Blue;
                iconText.IsVisible = true;
                break;
            default:
                iconText.IsVisible = false;
                break;
        }
    }

    private void CreateButtons(uint type)
    {
        var buttonPanel = this.GetVisualDescendants()
            .OfType<StackPanel>()
            .FirstOrDefault(sp => sp.Orientation == Orientation.Horizontal);
        
        if (buttonPanel == null) return;

        buttonPanel.Children.Clear();

        var buttonType = type & 0x0000000F;

        switch (buttonType)
        {
            case MB_OK:
                AddButton(buttonPanel, "OK", IDOK, isDefault: true);
                break;
            case MB_OKCANCEL:
                AddButton(buttonPanel, "OK", IDOK, isDefault: true);
                AddButton(buttonPanel, "Cancel", IDCANCEL, isCancel: true);
                break;
            case MB_ABORTRETRYIGNORE:
                AddButton(buttonPanel, "Abort", IDABORT);
                AddButton(buttonPanel, "Retry", IDRETRY, isDefault: true);
                AddButton(buttonPanel, "Ignore", IDIGNORE);
                break;
            case MB_YESNOCANCEL:
                AddButton(buttonPanel, "Yes", IDYES, isDefault: true);
                AddButton(buttonPanel, "No", IDNO);
                AddButton(buttonPanel, "Cancel", IDCANCEL, isCancel: true);
                break;
            case MB_YESNO:
                AddButton(buttonPanel, "Yes", IDYES, isDefault: true);
                AddButton(buttonPanel, "No", IDNO);
                break;
            case MB_RETRYCANCEL:
                AddButton(buttonPanel, "Retry", IDRETRY, isDefault: true);
                AddButton(buttonPanel, "Cancel", IDCANCEL, isCancel: true);
                break;
            default:
                AddButton(buttonPanel, "OK", IDOK, isDefault: true);
                break;
        }
    }

    private void AddButton(StackPanel panel, string text, int result, bool isDefault = false, bool isCancel = false)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 75,
            Padding = new Thickness(15, 5),
            IsDefault = isDefault,
            IsCancel = isCancel
        };

        button.Click += (sender, e) =>
        {
            _result = result;
            Close();
        };

        panel.Children.Add(button);
    }

    /// <summary>
    /// Shows the message box modally and returns the button result
    /// </summary>
    public async Task<int> ShowMessageBoxAsync(Window? owner)
    {
        if (owner != null)
        {
            await ShowDialog<int>(owner);
        }
        else
        {
            Show();
            await _resultTcs.Task;
        }

        return _result;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _resultTcs.TrySetResult(_result);
    }
}
