namespace Win32Emu.Gui.Services;

/// <summary>
/// Extended interface for receiving emulator output and events in the GUI
/// </summary>
public interface IGuiEmulatorHost : IEmulatorHost
{
    // OnWindowCreate and OnDisplayUpdate are now in the base IEmulatorHost interface
    
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
