using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Services;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.Generic;

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
    private Win32Emu.DebugLevel _minimumDebugLevel = Win32Emu.DebugLevel.Info;

    [ObservableProperty]
    private bool _showDebugPanel = true;

    [ObservableProperty]
    private bool _showStdOutputPanel = true;

    // Track created windows - maps Win32 HWND to Avalonia Window
    private readonly Dictionary<uint, Window> _createdWindows = new();
    
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

    public void OnDebugOutput(string message, Win32Emu.DebugLevel level)
    {
        if (level >= MinimumDebugLevel)
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
        }
    }

    public void OnStdOutput(string output)
    {
        StdOutput.Add(output);

        // Keep only last 1000 lines
        while (StdOutput.Count > 1000)
        {
            StdOutput.RemoveAt(0);
        }
    }

    public void OnWindowCreate(Win32Emu.WindowCreateInfo info)
    {
        // Phase 2: Create actual Avalonia windows for User32/GDI32 operations
        OnDebugOutput($"Creating Avalonia window for HWND=0x{info.Handle:X8}: {info.Title} ({info.Width}x{info.Height})", Win32Emu.DebugLevel.Info);
        
        // Create the window on the UI thread
        Dispatcher.UIThread.Post(() =>
        {
            try
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
                if (info.X >= 0 && info.X < 10000 && info.Y >= 0 && info.Y < 10000)
                {
                    window.Position = new Avalonia.PixelPoint(info.X, info.Y);
                }

                // Store the window mapping
                _createdWindows[info.Handle] = window;

                // Handle window closing
                window.Closing += (s, e) =>
                {
                    _createdWindows.Remove(info.Handle);
                    OnDebugOutput($"Avalonia window closed for HWND=0x{info.Handle:X8}", Win32Emu.DebugLevel.Info);
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
                
                OnDebugOutput($"Avalonia window shown for HWND=0x{info.Handle:X8}", Win32Emu.DebugLevel.Info);
            }
            catch (Exception ex)
            {
                OnDebugOutput($"Failed to create Avalonia window: {ex.Message}", Win32Emu.DebugLevel.Error);
            }
        });
    }

    public void OnDisplayUpdate(DisplayUpdateInfo info)
    {
        // TODO: Update SDL3 display rendering
        OnDebugOutput($"Display updated: {info.Width}x{info.Height}", Win32Emu.DebugLevel.Debug);
    }

    public void OnStateChanged(EmulatorState state)
    {
        CurrentState = state;
        OnDebugOutput($"Emulator state changed: {state}", Win32Emu.DebugLevel.Info);
    }

    [RelayCommand]
    private void StopEmulation()
    {
        if (_emulatorService?.CurrentEmulator != null)
        {
            _emulatorService.CurrentEmulator.Stop();
            OnDebugOutput("Stop requested", Win32Emu.DebugLevel.Info);
        }
    }

    [RelayCommand]
    private void PauseResumeEmulation()
    {
        if (_emulatorService?.CurrentEmulator != null)
        {
            if (_emulatorService.CurrentEmulator.IsPaused)
            {
                _emulatorService.CurrentEmulator.Resume();
                OnDebugOutput("Resume requested", Win32Emu.DebugLevel.Info);
            }
            else
            {
                _emulatorService.CurrentEmulator.Pause();
                OnDebugOutput("Pause requested", Win32Emu.DebugLevel.Info);
            }
        }
    }
}

public class DebugMessage
{
    public DateTime Timestamp { get; init; }
    public Win32Emu.DebugLevel Level { get; init; }
    public required string Message { get; init; }
}
