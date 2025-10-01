namespace Win32Emu.Gui.Services;

/// <summary>
/// Extended interface for receiving emulator output and events in the GUI
/// </summary>
public interface IGuiEmulatorHost : Win32Emu.IEmulatorHost
{
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
