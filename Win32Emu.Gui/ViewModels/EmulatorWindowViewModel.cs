using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Services;

namespace Win32Emu.Gui.ViewModels;

public partial class EmulatorWindowViewModel : ViewModelBase, IGuiEmulatorHost
{
    private readonly EmulatorService? _emulatorService;

    [ObservableProperty]
    private ObservableCollection<DebugMessage> _debugMessages = [];

    [ObservableProperty]
    private ObservableCollection<string> _stdOutput = [];

    [ObservableProperty]
    private EmulatorState _currentState = EmulatorState.Stopped;

    [ObservableProperty]
    private DebugLevel _minimumDebugLevel = DebugLevel.Info;

    [ObservableProperty]
    private bool _showDebugPanel = true;

    [ObservableProperty]
    private bool _showStdOutputPanel = true;

    // Track created windows - maps Win32 HWND to Avalonia Window
    private readonly Dictionary<uint, Window> _createdWindows = new();
    
    // Track created controls - maps Win32 HWND to Avalonia Control
    private readonly Dictionary<uint, Control> _createdControls = new();
    
    // Track control parent relationships - maps child HWND to parent HWND
    private readonly Dictionary<uint, uint> _controlParents = new();
    
    // Track control IDs - maps child HWND to control ID (from hMenu parameter)
    private readonly Dictionary<uint, uint> _controlIds = new();
    
    // Reference to the owner window for showing child windows
    private Window? _ownerWindow;

    public void SetOwnerWindow(Window owner)
    {
        _ownerWindow = owner;
    }

    public EmulatorWindowViewModel()
    {
        // Default constructor for design-time
    }

    public EmulatorWindowViewModel(EmulatorService emulatorService)
    {
        _emulatorService = emulatorService;
    }

    public void OnDebugOutput(string message, DebugLevel level)
    {
        if (level >= MinimumDebugLevel)
        {
            Dispatcher.UIThread.Post(() =>
            {
                DebugMessages.Add(new DebugMessage
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message
                });

                // Keep only last 1000 messages
                while (DebugMessages.Count > 1000)
                {
                    DebugMessages.RemoveAt(0);
                }
            });
        }
    }

    public void OnStdOutput(string output)
    {
        Dispatcher.UIThread.Post(() =>
        {
            StdOutput.Add(output);

            // Keep only last 1000 lines
            while (StdOutput.Count > 1000)
            {
                StdOutput.RemoveAt(0);
            }
        });
    }

    public void OnWindowCreate(WindowCreateInfo info)
    {
        // Phase 2: Create actual Avalonia windows for User32/GDI32 operations
        OnDebugOutput($"Creating Avalonia window for HWND=0x{info.Handle:X8}: {info.Title} ({info.Width}x{info.Height}), Class='{info.ClassName}', Parent=0x{info.Parent:X8}", DebugLevel.Info);
        
        // Create the window on the UI thread
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                // Check if this is a standard control (child window)
                if (info.Parent != 0 && Win32ControlFactory.IsStandardControl(info.ClassName))
                {
                    CreateChildControl(info);
                }
                else
                {
                    CreateTopLevelWindow(info);
                }
            }
            catch (Exception ex)
            {
                OnDebugOutput($"Failed to create Avalonia window: {ex.Message}", DebugLevel.Error);
            }
        });
    }

    private void CreateTopLevelWindow(WindowCreateInfo info)
    {
        var window = new Window
        {
            Title = string.IsNullOrEmpty(info.Title) ? $"Window 0x{info.Handle:X8}" : info.Title,
            Width = info.Width > 0 ? info.Width : 640,
            Height = info.Height > 0 ? info.Height : 480,
            CanResize = true,
            ShowInTaskbar = true
        };

        // Set position if specified (not CW_USEDEFAULT)
        if (info.X is >= 0 and < 10000 && info.Y is >= 0 and < 10000)
        {
            window.Position = new PixelPoint(info.X, info.Y);
        }

        // Create a canvas to host child controls
        var canvas = new Canvas();
        window.Content = canvas;

        // Store the window mapping
        _createdWindows[info.Handle] = window;

        // Handle window closing
        window.Closing += (s, e) =>
        {
            _createdWindows.Remove(info.Handle);
            OnDebugOutput($"Avalonia window closed for HWND=0x{info.Handle:X8}", DebugLevel.Info);
        };

        // Show the window with owner if available
        if (_ownerWindow != null)
        {
            window.Show(_ownerWindow);
        }
        else
        {
            window.Show();
        }
        
        OnDebugOutput($"Avalonia window shown for HWND=0x{info.Handle:X8}", DebugLevel.Info);
    }

    private void CreateChildControl(WindowCreateInfo info)
    {
        // Find parent window
        if (!_createdWindows.TryGetValue(info.Parent, out var parentWindow))
        {
            OnDebugOutput($"Parent window 0x{info.Parent:X8} not found for control 0x{info.Handle:X8}", DebugLevel.Warning);
            return;
        }

        // Create the appropriate Avalonia control
        var control = Win32ControlFactory.CreateControl(
            info.ClassName,
            info.Title,
            info.Style,
            info.Width,
            info.Height);

        if (control == null)
        {
            OnDebugOutput($"Failed to create control for class '{info.ClassName}'", DebugLevel.Warning);
            return;
        }

        // Set position on the canvas
        Canvas.SetLeft(control, info.X);
        Canvas.SetTop(control, info.Y);

        // Add to parent window's canvas
        if (parentWindow.Content is Canvas canvas)
        {
            canvas.Children.Add(control);
        }
        else
        {
            OnDebugOutput($"Parent window content is not a Canvas, cannot add control", DebugLevel.Warning);
            return;
        }

        // Store the control mapping
        _createdControls[info.Handle] = control;
        
        // Store the parent relationship
        _controlParents[info.Handle] = info.Parent;
        
        // Store the control ID (from hMenu parameter for child windows)
        _controlIds[info.Handle] = info.Menu;

        // Set up event handlers for the control
        SetupControlEventHandlers(info.Handle, control, info.ClassName);

        OnDebugOutput($"Created {info.ClassName} control at ({info.X}, {info.Y}) with ID={info.Menu}", DebugLevel.Info);
    }

    private void SetupControlEventHandlers(uint hwnd, Control control, string className)
    {
        switch (className.ToUpperInvariant())
        {
            case "BUTTON":
                if (control is Button button)
                {
                    button.Click += (s, e) =>
                    {
                        OnDebugOutput($"Button 0x{hwnd:X8} clicked", DebugLevel.Debug);
                        SendWmCommand(hwnd, 0); // BN_CLICKED = 0
                    };
                }
                break;

            case "EDIT":
                if (control is TextBox textBox)
                {
                    textBox.TextChanged += (s, e) =>
                    {
                        OnDebugOutput($"Edit 0x{hwnd:X8} text changed", DebugLevel.Debug);
                        SendWmCommand(hwnd, 0x0300); // EN_CHANGE = 0x0300
                    };
                }
                break;

            // Add more control types as needed
        }
    }
    
    /// <summary>
    /// Send WM_COMMAND message to the parent window
    /// </summary>
    private void SendWmCommand(uint controlHwnd, uint notificationCode)
    {
        // Get parent HWND
        if (!_controlParents.TryGetValue(controlHwnd, out var parentHwnd))
        {
            OnDebugOutput($"Cannot send WM_COMMAND: parent not found for control 0x{controlHwnd:X8}", DebugLevel.Warning);
            return;
        }
        
        // Get control ID
        if (!_controlIds.TryGetValue(controlHwnd, out var controlId))
        {
            OnDebugOutput($"Cannot send WM_COMMAND: control ID not found for control 0x{controlHwnd:X8}", DebugLevel.Warning);
            return;
        }
        
        // Build WM_COMMAND wParam: HIWORD = notification code, LOWORD = control ID
        uint wParam = (notificationCode << 16) | (controlId & 0xFFFF);
        uint lParam = controlHwnd;
        
        // Post WM_COMMAND (0x0111) to parent
        if (_emulatorService?.CurrentEmulator != null)
        {
            bool success = _emulatorService.CurrentEmulator.PostMessage(parentHwnd, 0x0111, wParam, lParam);
            OnDebugOutput($"Sent WM_COMMAND to parent 0x{parentHwnd:X8}: controlId={controlId}, notification=0x{notificationCode:X4}, success={success}", DebugLevel.Debug);
        }
        else
        {
            OnDebugOutput($"Cannot send WM_COMMAND: emulator not running", DebugLevel.Warning);
        }
    }

    public void OnDisplayUpdate(DisplayUpdateInfo info)
    {
        // TODO: Update SDL3 display rendering
        OnDebugOutput($"Display updated: {info.Width}x{info.Height}", DebugLevel.Debug);
    }

    public void OnStateChanged(EmulatorState state)
    {
        CurrentState = state;
        OnDebugOutput($"Emulator state changed: {state}", DebugLevel.Info);
    }

    [RelayCommand]
    private void StopEmulation()
    {
        _emulatorService?.StopEmulator();
        OnDebugOutput("Stop requested", DebugLevel.Info);
    }

    [RelayCommand]
    private void PauseResumeEmulation()
    {
        if (_emulatorService?.CurrentEmulator != null)
        {
            if (_emulatorService.CurrentEmulator.IsPaused)
            {
                _emulatorService.CurrentEmulator.Resume();
                OnDebugOutput("Resume requested", DebugLevel.Info);
            }
            else
            {
                _emulatorService.CurrentEmulator.Pause();
                OnDebugOutput("Pause requested", DebugLevel.Info);
            }
        }
    }

    [RelayCommand]
    private void ToggleDebugPanel()
    {
        ShowDebugPanel = !ShowDebugPanel;
    }
}

public class DebugMessage
{
    public DateTime Timestamp { get; init; }
    public DebugLevel Level { get; init; }
    public required string Message { get; init; }
}
