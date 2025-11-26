using Microsoft.Extensions.Logging;
using Silk.NET.Input;

namespace Win32Emu.Gui.Backends;
using Win32Emu.Rendering;
/// <summary>
/// Silk.NET-based input backend for DirectInput operations
/// </summary>
public class SilkInputBackend(ILogger logger) : IInputBackend
{
    private readonly ILogger _logger = logger;
    private bool _initialized;
    private readonly Lock _lock = new();
    private readonly Dictionary<uint, InputDevice> _devices = new();
    private uint _nextDeviceId = 1;

    // Shared state for keyboard and mouse (updated by SilkGlfwRenderingBackend)
    private static readonly Dictionary<int, bool> _sharedKeyboardState = new();
    private static readonly Dictionary<int, bool> _sharedMouseButtons = new();
    private static int _sharedMouseX = 0;
    private static int _sharedMouseY = 0;
    private static int _sharedMouseZ = 0;
    private static readonly Lock _sharedStateLock = new();

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// </summary>
    public event EventHandler<UIEventArgs>? UIEvent;

    private class InputDevice
    {
        public uint Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public IInputBackend.DeviceType Type { get; set; }
        public IInputBackend.InputState State { get; set; } = new();
    }


    public bool Initialize()
    {
        lock (_lock)
        {
            if (_initialized)
            {
                return true;
            }

            _initialized = true;
            _logger.LogInformation("[SilkInput] Input subsystem initialized");
            return true;
        }
    }

    public List<(uint DeviceId, string Name, IInputBackend.DeviceType Type)> GetDevices()
    {
        lock (_lock)
        {
            var result = new List<(uint, string, IInputBackend.DeviceType)>();

            // Always add keyboard and mouse as virtual devices
            result.Add((0x1000, "Keyboard", IInputBackend.DeviceType.Keyboard));
            result.Add((0x2000, "Mouse", IInputBackend.DeviceType.Mouse));

            // Note: Silk.NET.Input requires a window context to enumerate actual devices
            // For now, we'll provide basic keyboard/mouse support
            // TODO: Add gamepad/joystick support when integrated with windowing. Track in issue tracker. 

            return result;
        }
    }

    public uint OpenDevice(uint deviceId, IInputBackend.DeviceType type)
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                _logger.LogError("[SilkInput] Not initialized");
                return 0;
            }

            var internalId = _nextDeviceId++;
            var device = new InputDevice
            {
                Id = internalId,
                Type = type,
                Name = type switch
                {
                    IInputBackend.DeviceType.Keyboard => "Keyboard",
                    IInputBackend.DeviceType.Mouse => "Mouse",
                    IInputBackend.DeviceType.Gamepad => $"Gamepad {deviceId}",
                    IInputBackend.DeviceType.Joystick => $"Joystick {deviceId}",
                    _ => "Unknown Device"
                }
            };

            _devices[internalId] = device;
            _logger.LogInformation("[SilkInput] Opened device {InternalId}: {DeviceName} ({DeviceType})", 
                                  internalId, device.Name, type);
            return internalId;
        }
    }

    public bool CloseDevice(uint deviceId)
    {
        lock (_lock)
        {
            if (!_devices.TryGetValue(deviceId, out var device))
            {
                return false;
            }

            _devices.Remove(deviceId);
            _logger.LogInformation("[SilkInput] Closed device {DeviceId}", deviceId);
            return true;
        }
    }

    public bool PollDevice(uint deviceId, out IInputBackend.InputState? state)
    {
        lock (_lock)
        {
            state = null;

            if (!_devices.TryGetValue(deviceId, out var device))
            {
                return false;
            }

            // For keyboard and mouse, return state from shared state
            if (device.Type == IInputBackend.DeviceType.Keyboard)
            {
                state = new IInputBackend.InputState();
                lock (_sharedStateLock)
                {
                    state.KeyStates = new Dictionary<int, bool>(_sharedKeyboardState);
                }
                return true;
            }
            else if (device.Type == IInputBackend.DeviceType.Mouse)
            {
                state = new IInputBackend.InputState();
                lock (_sharedStateLock)
                {
                    state.MouseX = _sharedMouseX;
                    state.MouseY = _sharedMouseY;
                    state.MouseZ = _sharedMouseZ;
                    state.MouseButtons = new Dictionary<int, bool>(_sharedMouseButtons);
                }
                return true;
            }

            // For other device types, return cached state
            state = device.State;
            return true;
        }
    }

    /// <summary>
    /// Update device state (called by rendering backends that have window context)
    /// </summary>
    public void UpdateDeviceState(uint deviceId, IInputBackend.InputState newState)
    {
        lock (_lock)
        {
            if (_devices.TryGetValue(deviceId, out var device))
            {
                device.State = newState;
            }
        }
    }

    /// <summary>
    /// Update keyboard state from GLFW events (called by SilkGlfwRenderingBackend)
    /// </summary>
    public static void UpdateKeyState(int keyCode, bool pressed)
    {
        lock (_sharedStateLock)
        {
            _sharedKeyboardState[keyCode] = pressed;
        }
    }

    /// <summary>
    /// Update mouse button state from GLFW events (called by SilkGlfwRenderingBackend)
    /// </summary>
    public static void UpdateMouseButton(int button, bool pressed)
    {
        lock (_sharedStateLock)
        {
            _sharedMouseButtons[button] = pressed;
        }
    }

    /// <summary>
    /// Update mouse position from GLFW events (called by SilkGlfwRenderingBackend)
    /// </summary>
    public static void UpdateMousePosition(int x, int y)
    {
        lock (_sharedStateLock)
        {
            _sharedMouseX = x;
            _sharedMouseY = y;
        }
    }

    /// <summary>
    /// Update mouse wheel from GLFW events (called by SilkGlfwRenderingBackend)
    /// </summary>
    public static void UpdateMouseWheel(int delta)
    {
        lock (_sharedStateLock)
        {
            _sharedMouseZ += delta;
        }
    }

    public void ProcessEvents()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            // Process input events and raise UIEvent for each input change.
            // This is a stub implementation that does nothing.
            // A full implementation would:
            // - Poll the windowing system (SDL, GLFW) for pending events
            // - Convert each event (key press, mouse move, etc.) to UIEventArgs
            // - Call OnUIEvent(args) to notify subscribers
            // 
            // Note: Actual input handling is typically done by the rendering backend
            // (SilkSdlRenderingBackend, SilkGlfwRenderingBackend) which has window context.
        }
    }

    /// <summary>
    /// Helper method for subclasses to raise UI events
    /// </summary>
    protected virtual void OnUIEvent(UIEventArgs e)
    {
        UIEvent?.Invoke(this, e);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            // Close all open devices
            foreach (var device in _devices.Values.ToList())
            {
                CloseDevice(device.Id);
            }

            _devices.Clear();
            _initialized = false;
            _logger.LogInformation("[SilkInput] Input subsystem disposed");
        }
    }

    public bool IsInitialized => _initialized;
    public int DeviceCount => _devices.Count;
}
