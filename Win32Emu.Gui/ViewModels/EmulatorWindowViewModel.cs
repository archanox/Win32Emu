using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Services;
using Win32Emu.Gui.Utilities;
using Win32Emu.Win32.Messaging;

namespace Win32Emu.Gui.ViewModels;

public partial class EmulatorWindowViewModel : ViewModelBase, IGuiEmulatorHost
{
    private readonly EmulatorService? _emulatorService;
    private GuiMessageDispatcherIntegration? _messageDispatcherIntegration;

    [ObservableProperty]
    private ObservableCollection<DebugMessage> _debugMessages = [];

    [ObservableProperty]
    private string _stdOutput = "";

    [ObservableProperty]
    private EmulatorState _currentState = EmulatorState.Stopped;

    [ObservableProperty]
    private DebugLevel _minimumDebugLevel = DebugLevel.Info;

    [ObservableProperty]
    private bool _showDebugPanel = true;

    [ObservableProperty]
    private bool _showStdOutputPanel = true;

    [ObservableProperty]
    private WriteableBitmap? _displayBitmap;

    [ObservableProperty]
    private bool _hasDisplay;

    // Track created windows - maps Win32 HWND to Avalonia Window
    private readonly Dictionary<uint, Window> _createdWindows = new();
    
    // Track created dialogs - maps Win32 HWND to DialogWindow
    private readonly Dictionary<uint, Views.DialogWindow> _createdDialogs = new();
    
    // Track created controls - maps Win32 HWND to Avalonia Control
    private readonly Dictionary<uint, Control> _createdControls = new();
    
    // Track control parent relationships - maps child HWND to parent HWND
    private readonly Dictionary<uint, uint> _controlParents = new();
    
    // Track control IDs - maps child HWND to control ID (from hMenu parameter)
    private readonly Dictionary<uint, uint> _controlIds = new();
    
    // Reference to the owner window for showing child windows
    private Window? _ownerWindow;
    
    // Track if we've resized the window to match the display
    private bool _hasResizedForDisplay;

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

    /// <summary>
    /// Initialize MessageDispatcher integration when emulator is ready
    /// </summary>
    public void InitializeMessageDispatcher()
    {
        if (_emulatorService?.CurrentEmulator?.Environment != null)
        {
            _messageDispatcherIntegration = new GuiMessageDispatcherIntegration(
                _emulatorService.CurrentEmulator.Environment,
                this
            );
            _messageDispatcherIntegration.RegisterDefaultHandlers();
            OnDebugOutput("MessageDispatcher integration initialized with async handlers", DebugLevel.Info);
        }
    }

    /// <summary>
    /// Post a message asynchronously using the MessageDispatcher
    /// </summary>
    private async Task PostMessageAsync(uint hwnd, uint message, uint wParam, uint lParam)
    {
        if (_messageDispatcherIntegration != null)
        {
            await _messageDispatcherIntegration.PostMessageAsync(hwnd, message, wParam, lParam);
        }
        else
        {
            // Fallback to synchronous PostMessage
            _emulatorService?.CurrentEmulator?.PostMessage(hwnd, message, wParam, lParam);
        }
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
            StdOutput += output;
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

    public async Task<int> OnDialogCreate(DialogCreateInfo info)
    {
        OnDebugOutput($"Creating Avalonia dialog for HWND=0x{info.Handle:X8}: {info.Template.Title} ({info.Template.Width}x{info.Template.Height})", DebugLevel.Info);
        OnDebugOutput($"EmulatorService is {(_emulatorService != null ? "NOT NULL" : "NULL")}", DebugLevel.Info);
        OnDebugOutput($"CurrentEmulator is {(_emulatorService?.CurrentEmulator != null ? "NOT NULL" : "NULL")}", DebugLevel.Info);
        
        // Show the dialog on the UI thread (non-blocking)
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                // Create message callback that posts messages to the emulator
                // Always create the callback even if emulator is not available yet.
                // The runtime check handles race conditions where the emulator might be 
                // stopped or replaced between callback creation and execution.
                Action<uint, uint, uint, uint> messageCallback = (hwnd, msg, wParam, lParam) =>
                {
                    if (_emulatorService?.CurrentEmulator != null)
                    {
                        OnDebugOutput($"Dialog HWND=0x{hwnd:X8} posting message MSG=0x{msg:X4} wParam=0x{wParam:X8} lParam=0x{lParam:X8}", DebugLevel.Info);
                        _emulatorService.CurrentEmulator.PostMessage(hwnd, msg, wParam, lParam);
                    }
                    else
                    {
                        OnDebugOutput($"ERROR: Cannot post message from dialog - emulator service or current emulator is null at execution time", DebugLevel.Error);
                    }
                };
                
                if (_emulatorService?.CurrentEmulator != null)
                {
                    OnDebugOutput($"Dialog message callback created successfully for dialog HWND=0x{info.Handle:X8}", DebugLevel.Info);
                }
                else
                {
                    OnDebugOutput($"WARNING: Created message callback but emulator service={(_emulatorService != null ? "NOT NULL" : "NULL")}, current emulator={(_emulatorService?.CurrentEmulator != null ? "NOT NULL" : "NULL")}", DebugLevel.Warning);
                }
                
                // Create debug callback that uses OnDebugOutput
                Action<string, DebugLevel> debugCallback = (message, level) =>
                {
                    OnDebugOutput(message, level);
                };
                
                OnDebugOutput($"About to create DialogWindow with messageCallback=NOT NULL, debugCallback=NOT NULL", DebugLevel.Info);
                
                // Create DialogWindow from the template with dialog handle, control handles, message callback, and debug callback
                var dialogWindow = new Views.DialogWindow(info.Template, info.Handle, info.ControlHandles, messageCallback, debugCallback);
                
                // Track the dialog so we can close it later via EndDialog
                _createdDialogs[info.Handle] = dialogWindow;
                
                // Find parent window if specified
                Window? parentWindow = null;
                if (info.ParentHandle != 0 && _createdWindows.TryGetValue(info.ParentHandle, out var parent))
                {
                    parentWindow = parent;
                }
                else
                {
                    // Use owner window as fallback
                    parentWindow = _ownerWindow;
                }
                
                // Show the dialog non-modally
                // Note: We can't use ShowDialog() because that would block
                // Instead, show it as a regular window
                dialogWindow.Show();
                
                OnDebugOutput($"Dialog window shown for HWND=0x{info.Handle:X8}, message loop will handle interactions", DebugLevel.Info);
            }
            catch (Exception ex)
            {
                OnDebugOutput($"Failed to create/show Avalonia dialog: {ex.Message}", DebugLevel.Error);
            }
        });
        
        // Return immediately - the message loop will handle the dialog lifecycle
        // The actual result will come from EndDialog
        return 0;
    }

    public void OnDialogEnd(uint dialogHandle, int result)
    {
        OnDebugOutput($"Closing Avalonia dialog for HWND=0x{dialogHandle:X8} with result={result}", DebugLevel.Info);
        
        // Close the dialog window on the UI thread
        Dispatcher.UIThread.Post(() =>
        {
            if (_createdDialogs.TryGetValue(dialogHandle, out var dialogWindow))
            {
                try
                {
                    // End the dialog with the specified result
                    dialogWindow.EndDialog(result);
                    
                    // Remove from tracking
                    _createdDialogs.Remove(dialogHandle);
                    
                    OnDebugOutput($"Dialog closed for HWND=0x{dialogHandle:X8}", DebugLevel.Info);
                }
                catch (Exception ex)
                {
                    OnDebugOutput($"Error closing dialog for HWND=0x{dialogHandle:X8}: {ex.Message}", DebugLevel.Error);
                }
            }
            else
            {
                OnDebugOutput($"Dialog HWND=0x{dialogHandle:X8} not found in tracking dictionary", DebugLevel.Warning);
            }
        });
    }

    public int OnMessageBox(MessageBoxInfo info)
    {
        OnDebugOutput($"MessageBox: \"{info.Caption}\" - \"{info.Text}\" (type=0x{info.Type:X8})", DebugLevel.Error);
        
        // Show message box on UI thread and wait for result
        var result = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                var messageBox = new Views.MessageBoxWindow(info.Caption, info.Text, info.Type);
                var buttonResult = await messageBox.ShowMessageBoxAsync(_ownerWindow);
                
                OnDebugOutput($"MessageBox returned: {buttonResult}", DebugLevel.Info);
                return buttonResult;
            }
            catch (Exception ex)
            {
                OnDebugOutput($"Error showing message box: {ex.Message}", DebugLevel.Error);
                return 1; // IDOK as fallback
            }
        }).Result;
        
        return result;
    }

    public void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text)
    {
        // Find the dialog window and update the control
        if (_createdDialogs.TryGetValue(dialogHandle, out var dialog))
        {
            dialog.SetControlText((ushort)controlId, text);
        }
    }

    public void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData)
    {
        OnDebugOutput($"Dialog 0x{dialogHandle:X8} control {controlId} bitmap changed ({bitmapData.Length} bytes)", DebugLevel.Info);
        
        // Find the dialog window and update the control with bitmap
        if (_createdDialogs.TryGetValue(dialogHandle, out var dialog))
        {
            dialog.SetControlBitmap((ushort)controlId, bitmapData);
            OnDebugOutput($"Dialog 0x{dialogHandle:X8} control {controlId} bitmap updated successfully", DebugLevel.Debug);
        }
        else
        {
            OnDebugOutput($"Dialog 0x{dialogHandle:X8} not found in _createdDialogs", DebugLevel.Warning);
        }
    }

    public void OnDialogControlEnabledChanged(uint dialogHandle, int controlId, bool enabled)
    {
        // Find the dialog window and enable/disable the control
        if (_createdDialogs.TryGetValue(dialogHandle, out var dialog))
        {
            dialog.SetControlEnabled((ushort)controlId, enabled);
            OnDebugOutput($"Dialog 0x{dialogHandle:X8} control {controlId} {(enabled ? "enabled" : "disabled")}", DebugLevel.Debug);
        }
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

        // Hook window lifecycle events to send Win32 messages
        window.Opened += async (s, e) =>
        {
            OnDebugOutput($"Avalonia window opened for HWND=0x{info.Handle:X8}, sending WM_SHOWWINDOW", DebugLevel.Debug);
            // WM_SHOWWINDOW = 0x0018, wParam = TRUE (showing)
            await PostMessageAsync(info.Handle, 0x0018, 1, 0);
        };

        window.Activated += async (s, e) =>
        {
            OnDebugOutput($"Avalonia window activated for HWND=0x{info.Handle:X8}, sending WM_ACTIVATEAPP", DebugLevel.Debug);
            // WM_ACTIVATEAPP = 0x001C, wParam = TRUE (activating)
            await PostMessageAsync(info.Handle, 0x001C, 1, 0);
        };

        window.Deactivated += async (s, e) =>
        {
            OnDebugOutput($"Avalonia window deactivated for HWND=0x{info.Handle:X8}, sending WM_ACTIVATEAPP", DebugLevel.Debug);
            // WM_ACTIVATEAPP = 0x001C, wParam = FALSE (deactivating)
            await PostMessageAsync(info.Handle, 0x001C, 0, 0);
        };

        window.PositionChanged += async (s, e) =>
        {
            if (s is Window w)
            {
                var pos = w.Position;
                OnDebugOutput($"Avalonia window moved for HWND=0x{info.Handle:X8} to ({pos.X}, {pos.Y}), sending WM_MOVE", DebugLevel.Debug);
                // WM_MOVE = 0x0003, wParam = 0, lParam = MAKELONG(x, y)
                uint lParam = ((uint)pos.Y << 16) | ((uint)pos.X & 0xFFFF);
                await PostMessageAsync(info.Handle, 0x0003, 0, lParam);
            }
        };

        window.Resized += async (s, e) =>
        {
            if (s is Window w)
            {
                var size = w.ClientSize;
                OnDebugOutput($"Avalonia window resized for HWND=0x{info.Handle:X8} to {size.Width}x{size.Height}, sending WM_SIZE", DebugLevel.Debug);
                // WM_SIZE = 0x0005, wParam = SIZE_RESTORED (0), lParam = MAKELONG(width, height)
                ushort width = (ushort)Math.Clamp(size.Width, 0, ushort.MaxValue);
                ushort height = (ushort)Math.Clamp(size.Height, 0, ushort.MaxValue);
                uint lParam = ((uint)height << 16) | ((uint)width & 0xFFFF);
                await PostMessageAsync(info.Handle, 0x0005, 0, lParam);
            }
        };

        // Hook keyboard events
        window.KeyDown += async (s, e) =>
        {
            var virtualKey = Win32InputMapper.MapKeyToVirtualKeyCode(e.Key);
            if (virtualKey != 0)
            {
                OnDebugOutput($"Avalonia window key down for HWND=0x{info.Handle:X8}: Key={e.Key} VK=0x{virtualKey:X2}, sending WM_KEYDOWN", DebugLevel.Debug);
                // WM_KEYDOWN = 0x0100, wParam = virtual key code, lParam = key data (repeat count, scan code, etc.)
                // For simplicity, we use a basic lParam with repeat count = 1
                uint lParam = 0x00000001; // Repeat count = 1, scan code = 0, extended key = 0, context code = 0, previous state = 0, transition state = 0
                await PostMessageAsync(info.Handle, 0x0100, virtualKey, lParam);
                
                // Also send WM_CHAR for character input (simplified - real implementation should check if it's a character key)
                if ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.Space)
                {
                    // For now, just send the virtual key as char code (simplified)
                    // A proper implementation would translate based on keyboard layout and shift state
                    char charCode = (e.Key >= Key.A && e.Key <= Key.Z)
                        ? (char)('A' + (e.Key - Key.A))
                        : (e.Key >= Key.D0 && e.Key <= Key.D9)
                            ? (char)('0' + (e.Key - Key.D0))
                            : ' ';
                    
                    // Apply shift modifier for letters
                    if ((e.KeyModifiers & KeyModifiers.Shift) == 0 && charCode >= 'A' && charCode <= 'Z')
                    {
                        charCode = (char)(charCode + 32); // Convert to lowercase
                    }
                    
                    OnDebugOutput($"Avalonia window char for HWND=0x{info.Handle:X8}: Char='{charCode}' (0x{(uint)charCode:X2}), sending WM_CHAR", DebugLevel.Debug);
                    // WM_CHAR = 0x0102
                    await PostMessageAsync(info.Handle, 0x0102, (uint)charCode, lParam);
                }
            }
        };

        window.KeyUp += async (s, e) =>
        {
            var virtualKey = Win32InputMapper.MapKeyToVirtualKeyCode(e.Key);
            if (virtualKey != 0)
            {
                OnDebugOutput($"Avalonia window key up for HWND=0x{info.Handle:X8}: Key={e.Key} VK=0x{virtualKey:X2}, sending WM_KEYUP", DebugLevel.Debug);
                // WM_KEYUP = 0x0101, wParam = virtual key code, lParam = key data
                uint lParam = 0xC0000001; // Repeat count = 1, previous state = 1, transition state = 1 (key being released)
                await PostMessageAsync(info.Handle, 0x0101, virtualKey, lParam);
            }
        };

        // Hook mouse events
        window.PointerPressed += async (s, e) =>
        {
            var point = e.GetCurrentPoint(window);
            var pos = point.Position;
            var properties = point.Properties;
            
            uint wParam = Win32InputMapper.GetMouseButtonState(properties);
            uint lParam = Win32InputMapper.MakeMouseLParam(pos.X, pos.Y);
            
            if (properties.IsLeftButtonPressed)
            {
                OnDebugOutput($"Avalonia window left button down at ({pos.X:F0}, {pos.Y:F0}) for HWND=0x{info.Handle:X8}, sending WM_LBUTTONDOWN", DebugLevel.Debug);
                // WM_LBUTTONDOWN = 0x0201
                await PostMessageAsync(info.Handle, 0x0201, wParam, lParam);
            }
            else if (properties.IsRightButtonPressed)
            {
                OnDebugOutput($"Avalonia window right button down at ({pos.X:F0}, {pos.Y:F0}) for HWND=0x{info.Handle:X8}, sending WM_RBUTTONDOWN", DebugLevel.Debug);
                // WM_RBUTTONDOWN = 0x0204
                await PostMessageAsync(info.Handle, 0x0204, wParam, lParam);
            }
            else if (properties.IsMiddleButtonPressed)
            {
                OnDebugOutput($"Avalonia window middle button down at ({pos.X:F0}, {pos.Y:F0}) for HWND=0x{info.Handle:X8}, sending WM_MBUTTONDOWN", DebugLevel.Debug);
                // WM_MBUTTONDOWN = 0x0207
                await PostMessageAsync(info.Handle, 0x0207, wParam, lParam);
            }
        };

        window.PointerReleased += async (s, e) =>
        {
            var point = e.GetCurrentPoint(window);
            var pos = point.Position;
            var properties = point.Properties;
            
            uint wParam = Win32InputMapper.GetMouseButtonState(properties);
            uint lParam = Win32InputMapper.MakeMouseLParam(pos.X, pos.Y);
            
            // Determine which button was released based on the button type
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                OnDebugOutput($"Avalonia window left button up at ({pos.X:F0}, {pos.Y:F0}) for HWND=0x{info.Handle:X8}, sending WM_LBUTTONUP", DebugLevel.Debug);
                // WM_LBUTTONUP = 0x0202
                await PostMessageAsync(info.Handle, 0x0202, wParam, lParam);
            }
            else if (e.InitialPressMouseButton == MouseButton.Right)
            {
                OnDebugOutput($"Avalonia window right button up at ({pos.X:F0}, {pos.Y:F0}) for HWND=0x{info.Handle:X8}, sending WM_RBUTTONUP", DebugLevel.Debug);
                // WM_RBUTTONUP = 0x0205
                await PostMessageAsync(info.Handle, 0x0205, wParam, lParam);
            }
            else if (e.InitialPressMouseButton == MouseButton.Middle)
            {
                OnDebugOutput($"Avalonia window middle button up at ({pos.X:F0}, {pos.Y:F0}) for HWND=0x{info.Handle:X8}, sending WM_MBUTTONUP", DebugLevel.Debug);
                // WM_MBUTTONUP = 0x0208
                await PostMessageAsync(info.Handle, 0x0208, wParam, lParam);
            }
        };

        window.PointerMoved += async (s, e) =>
        {
            var point = e.GetCurrentPoint(window);
            var pos = point.Position;
            var properties = point.Properties;
            
            uint wParam = Win32InputMapper.GetMouseButtonState(properties);
            uint lParam = Win32InputMapper.MakeMouseLParam(pos.X, pos.Y);
            
            // WM_MOUSEMOVE = 0x0200
            // Only log mouse move at Debug level to avoid spam
            OnDebugOutput($"Avalonia window mouse move at ({pos.X:F0}, {pos.Y:F0}) for HWND=0x{info.Handle:X8}, sending WM_MOUSEMOVE", DebugLevel.Trace);
            await PostMessageAsync(info.Handle, 0x0200, wParam, lParam);
        };

        window.PointerWheelChanged += async (s, e) =>
        {
            var point = e.GetCurrentPoint(window);
            var pos = point.Position;
            var properties = point.Properties;
            var delta = e.Delta.Y; // Vertical scroll delta
            
            // Win32 wheel delta is in units of WHEEL_DELTA (120)
            short wheelDelta = (short)(delta * 120);
            uint wParam = ((uint)(ushort)wheelDelta << 16) | Win32InputMapper.GetMouseButtonState(properties);
            uint lParam = Win32InputMapper.MakeMouseLParam(pos.X, pos.Y);
            
            OnDebugOutput($"Avalonia window mouse wheel at ({pos.X:F0}, {pos.Y:F0}) delta={delta} for HWND=0x{info.Handle:X8}, sending WM_MOUSEWHEEL", DebugLevel.Debug);
            // WM_MOUSEWHEEL = 0x020A
            await PostMessageAsync(info.Handle, 0x020A, wParam, lParam);
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
        
        OnDebugOutput($"Avalonia window shown for HWND=0x{info.Handle:X8} with keyboard and mouse input routing enabled", DebugLevel.Info);
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
                    button.Click += async (s, e) =>
                    {
                        OnDebugOutput($"Button 0x{hwnd:X8} clicked", DebugLevel.Debug);
                        // Send WM_LBUTTONDOWN and WM_LBUTTONUP to simulate a button click
                        // This will allow the StandardControlHandler to handle the click and send WM_COMMAND
                        await SendMouseClickToButton(hwnd);
                    };
                }
                break;

            case "EDIT":
                if (control is TextBox textBox)
                {
                    textBox.TextChanged += async (s, e) =>
                    {
                        OnDebugOutput($"Edit 0x{hwnd:X8} text changed", DebugLevel.Debug);
                        await SendWmCommand(hwnd, 0x0300); // EN_CHANGE = 0x0300
                    };
                }
                break;

            // Add more control types as needed
        }
    }
    
    /// <summary>
    /// Send mouse click messages to a button control
    /// </summary>
    private async Task SendMouseClickToButton(uint buttonHwnd)
    {
        if (_emulatorService?.CurrentEmulator != null)
        {
            // Send WM_LBUTTONDOWN (0x0201) async
            await PostMessageAsync(buttonHwnd, 0x0201, 0x0001, 0);
            
            // Send WM_LBUTTONUP (0x0202) async
            await PostMessageAsync(buttonHwnd, 0x0202, 0, 0);
            
            OnDebugOutput($"Sent mouse click messages to button 0x{buttonHwnd:X8}", DebugLevel.Debug);
        }
        else
        {
            OnDebugOutput($"Cannot send mouse click: emulator not running", DebugLevel.Warning);
        }
    }
    
    /// <summary>
    /// Send WM_COMMAND message to the parent window
    /// </summary>
    private async Task SendWmCommand(uint controlHwnd, uint notificationCode)
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
        
        // Post WM_COMMAND (0x0111) to parent async
        if (_emulatorService?.CurrentEmulator != null)
        {
            try
            {
                await PostMessageAsync(parentHwnd, 0x0111, wParam, lParam);
                OnDebugOutput($"Sent WM_COMMAND to parent 0x{parentHwnd:X8}: controlId={controlId}, notification=0x{notificationCode:X4}", DebugLevel.Debug);
            }
            catch (Exception ex)
            {
                OnDebugOutput($"Failed to send WM_COMMAND to parent 0x{parentHwnd:X8}: {ex.Message}", DebugLevel.Error);
            }
        }
        else
        {
            OnDebugOutput($"Cannot send WM_COMMAND: emulator not running", DebugLevel.Warning);
        }
    }

    public void OnDisplayUpdate(DisplayUpdateInfo info)
    {
        // Update the Avalonia display with the frame buffer
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                // Create or resize the bitmap if needed
                if (DisplayBitmap == null || DisplayBitmap.PixelSize.Width != info.Width || DisplayBitmap.PixelSize.Height != info.Height)
                {
                    // Dispose old bitmap if it exists
                    DisplayBitmap?.Dispose();
                    
                    DisplayBitmap = new WriteableBitmap(
                        new PixelSize(info.Width, info.Height),
                        new Vector(96, 96),
                        PixelFormat.Rgba8888,
                        AlphaFormat.Premul);
                    
                    HasDisplay = true;
                    OnDebugOutput($"Created display bitmap: {info.Width}x{info.Height}", DebugLevel.Info);
                    
                    // Resize the window to match the display size (first time only)
                    if (!_hasResizedForDisplay && _ownerWindow != null)
                    {
                        ResizeWindowForDisplay(info.Width, info.Height);
                        _hasResizedForDisplay = true;
                    }
                }

                // Update the bitmap with the new frame buffer data
                using (var framebuffer = DisplayBitmap.Lock())
                {
                    // Calculate the actual framebuffer size accounting for stride/row padding
                    var framebufferBytes = framebuffer.RowBytes * framebuffer.Size.Height;
                    
                    // Ensure we don't copy more data than available or than the framebuffer can hold
                    var maxCopy = Math.Min(framebufferBytes, info.Width * info.Height * 4);
                    var bytesToCopy = Math.Min(info.FrameBuffer.Length, maxCopy);
                    
                    if (bytesToCopy != info.FrameBuffer.Length)
                    {
                        OnDebugOutput($"Frame buffer size mismatch: provided={info.FrameBuffer.Length}, framebuffer={framebufferBytes}, copying={bytesToCopy}", DebugLevel.Warning);
                    }
                    
                    // Copy using Marshal for safety - bounds are validated above
                    System.Runtime.InteropServices.Marshal.Copy(
                        info.FrameBuffer,
                        0,
                        framebuffer.Address,
                        bytesToCopy);
                }

                OnDebugOutput($"Display updated: {info.Width}x{info.Height}, stride={info.Stride}", DebugLevel.Trace);
            }
            catch (ArgumentException ex)
            {
                OnDebugOutput($"Argument error updating display: {ex.Message}", DebugLevel.Error);
            }
            catch (InvalidOperationException ex)
            {
                OnDebugOutput($"Invalid operation updating display: {ex.Message}", DebugLevel.Error);
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                OnDebugOutput($"Interop error updating display: {ex.Message}", DebugLevel.Error);
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException) && !(ex is StackOverflowException) && !(ex is System.Threading.ThreadAbortException))
            {
                OnDebugOutput($"Unexpected error updating display: {ex.Message}", DebugLevel.Error);
            }
        });
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

    [RelayCommand]
    private async Task OpenRegistryViewer()
    {
        if (_emulatorService?.CurrentEmulator?.Environment != null)
        {
            var registryWindow = new Views.RegistryViewerWindow
            {
                DataContext = new RegistryViewerViewModel(_emulatorService.CurrentEmulator.Environment)
            };
            
            if (_ownerWindow != null)
            {
                await registryWindow.ShowDialog(_ownerWindow);
            }
            else
            {
                registryWindow.Show();
            }
        }
        else
        {
            OnDebugOutput("Cannot open registry viewer: Emulator not running", DebugLevel.Warning);
        }
    }
    
    /// <summary>
    /// Resize the EmulatorWindow to match the game's display resolution
    /// </summary>
    private void ResizeWindowForDisplay(int displayWidth, int displayHeight)
    {
        if (_ownerWindow == null)
        {
            return;
        }
        
        try
        {
            // Calculate window size accounting for UI chrome (borders, title bar, status bar, debug panel if visible)
            // Status bar is approximately 40px, window chrome is approximately 40px
            const int chromeHeight = 80;
            
            // If debug panel is shown, we keep the window width as is (debug panel is on the side)
            // Otherwise, we resize to match the display width
            int targetWidth = displayWidth;
            int targetHeight = displayHeight + chromeHeight;
            
            // Add extra width if debug panel is visible (approximately 400px)
            if (ShowDebugPanel)
            {
                targetWidth += 400;
            }
            
            // Ensure minimum window size
            targetWidth = Math.Max(targetWidth, 640);
            targetHeight = Math.Max(targetHeight, 480);
            
            // Ensure window fits on screen (with some margin)
            if (_ownerWindow.Screens?.Primary != null)
            {
                var screenBounds = _ownerWindow.Screens.Primary.WorkingArea;
                targetWidth = Math.Min(targetWidth, (int)(screenBounds.Width * 0.9));
                targetHeight = Math.Min(targetHeight, (int)(screenBounds.Height * 0.9));
            }
            
            _ownerWindow.Width = targetWidth;
            _ownerWindow.Height = targetHeight;
            
            OnDebugOutput($"Resized EmulatorWindow to {targetWidth}x{targetHeight} for display {displayWidth}x{displayHeight}", DebugLevel.Info);
        }
        catch (Exception ex)
        {
            OnDebugOutput($"Failed to resize window: {ex.Message}", DebugLevel.Warning);
        }
    }
}

public class DebugMessage
{
    public DateTime Timestamp { get; init; }
    public DebugLevel Level { get; init; }
    public required string Message { get; init; }
}
