namespace Win32Emu.Gui.Services;

/// <summary>
/// Interface for receiving emulator output and events
/// </summary>
public interface IEmulatorHost
{
    /// <summary>
    /// Called when the emulator outputs debug information
    /// </summary>
    void OnDebugOutput(string message, DebugLevel level);
    
    /// <summary>
    /// Called when the emulated program writes to stdout
    /// </summary>
    void OnStdOutput(string output);
    
    /// <summary>
    /// Called when the emulator needs to create a window
    /// </summary>
    void OnWindowCreate(WindowCreateInfo info);
    
    /// <summary>
    /// Called when the emulator updates display output
    /// </summary>
    void OnDisplayUpdate(DisplayUpdateInfo info);
    
    /// <summary>
    /// Called when the emulator state changes
    /// </summary>
    void OnStateChanged(EmulatorState state);
}

public enum DebugLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error
}

public enum EmulatorState
{
    Stopped,
    Running,
    Paused,
    Error
}

public class WindowCreateInfo
{
    public required string Title { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}

public class DisplayUpdateInfo
{
    public required byte[] FrameBuffer { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Stride { get; init; }
}
