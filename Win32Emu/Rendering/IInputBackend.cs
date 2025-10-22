namespace Win32Emu.Rendering;

/// <summary>
/// Interface for input backends
/// </summary>
public interface IInputBackend : IDisposable
{
    /// <summary>
    /// Device type enumeration
    /// </summary>
    public enum DeviceType
    {
        Keyboard,
        Mouse,
        Gamepad,
        Joystick
    }

    /// <summary>
    /// Input state for a device
    /// </summary>
    public class InputState
    {
        // Joystick/Gamepad state
        public Dictionary<int, bool> Buttons { get; set; } = new();
        public Dictionary<int, short> Axes { get; set; } = new();
        public int PovHat { get; set; }

        // Keyboard state (256 keys for DirectInput compatibility)
        public Dictionary<int, bool> KeyStates { get; set; } = new();

        // Mouse state
        public int MouseX { get; set; }
        public int MouseY { get; set; }
        public int MouseZ { get; set; } // Scroll wheel
        public Dictionary<int, bool> MouseButtons { get; set; } = new();
    }

    /// <summary>
    /// Initialize the input backend
    /// </summary>
    bool Initialize();

    /// <summary>
    /// Get list of available input devices
    /// </summary>
    List<(uint DeviceId, string Name, DeviceType Type)> GetDevices();

    /// <summary>
    /// Open an input device for reading
    /// </summary>
    uint OpenDevice(uint deviceId, DeviceType type);

    /// <summary>
    /// Close an input device
    /// </summary>
    bool CloseDevice(uint deviceId);

    /// <summary>
    /// Poll input state from a device
    /// </summary>
    bool PollDevice(uint deviceId, out InputState? state);

    /// <summary>
    /// Process input events
    /// </summary>
    void ProcessEvents();

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// </summary>
    event EventHandler<UIEventArgs>? UIEvent;

    /// <summary>
    /// Gets whether the backend is initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets the number of open devices
    /// </summary>
    int DeviceCount { get; }
}
