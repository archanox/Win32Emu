using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Win32Emu.Gui.Services;

namespace Win32Emu.Gui.ViewModels;

public partial class EmulatorWindowViewModel : ViewModelBase, IEmulatorHost
{
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

    public void OnDebugOutput(string message, DebugLevel level)
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

    public void OnWindowCreate(WindowCreateInfo info)
    {
        // TODO: Create Avalonia window for User32/GDI32 operations
        OnDebugOutput($"Window created: {info.Title} ({info.Width}x{info.Height})", DebugLevel.Info);
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
}

public class DebugMessage
{
    public DateTime Timestamp { get; init; }
    public DebugLevel Level { get; init; }
    public required string Message { get; init; }
}
