using Microsoft.Extensions.Logging;
using Silk.NET.Input;

namespace Win32Emu.Rendering;

/// <summary>
/// Silk.NET-based input backend for DirectInput operations
/// </summary>
public class SilkInputBackend : IInputBackend
{
    private readonly ILogger _logger;
    private bool _initialized;
    private readonly object _lock = new();
    private readonly Dictionary<uint, InputDevice> _devices = new();
    private uint _nextDeviceId = 1;

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

    public SilkInputBackend(ILogger logger)
    {
        _logger = logger;
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

            // Return current state of the device.
            // This is a stub implementation that returns empty state.
            // A full implementation would:
            // - Query Silk.NET.Input's IInputContext for actual keyboard/mouse/gamepad state
            // - Requires integration with a windowing system (SDL, GLFW) to get input context
            // - Update device.State with current button presses, axis positions, etc.
            state = device.State;
            return true;
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
