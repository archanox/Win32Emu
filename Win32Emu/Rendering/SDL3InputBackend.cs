using Microsoft.Extensions.Logging;
using SDL3;
using System.Collections.Concurrent;

namespace Win32Emu.Rendering;

/// <summary>
/// SDL3 input backend for DirectInput operations
/// </summary>
public class Sdl3InputBackend : IInputBackend
{
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<int, DeviceInfo> _devices = new();
    private bool _initialized;
    private readonly object _lock = new();
    private int _nextDeviceId = 1;

    private class DeviceInfo
    {
        public int Id { get; set; }
        public IInputBackend.DeviceType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public IntPtr JoystickHandle { get; set; }
    }

    public Sdl3InputBackend(ILogger logger)
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

            try
            {
                _logger.LogInformation("[SDL3Input] Initializing SDL3 input backend...");

                // Critical: Set app metadata before any SDL initialization
                Sdl3Initializer.EnsureAppMetadataSet();

                // Initialize SDL joystick subsystem
                if (!SDL.Init(SDL_InitFlags.Joystick | SDL_InitFlags.Gamepad))
                {
                    _logger.LogError("[SDL3Input] Failed to initialize SDL input: {Error}", SDL.GetError());
                    return false;
                }

                // Add keyboard device
                _devices[_nextDeviceId] = new DeviceInfo
                {
                    Id = _nextDeviceId++,
                    Type = IInputBackend.DeviceType.Keyboard,
                    Name = "Keyboard"
                };

                // Add mouse device
                _devices[_nextDeviceId] = new DeviceInfo
                {
                    Id = _nextDeviceId++,
                    Type = IInputBackend.DeviceType.Mouse,
                    Name = "Mouse"
                };

                // Enumerate joysticks
                var joystickIds = SDL.GetJoysticks();
                if (joystickIds != null)
                {
                    for (var i = 0; i < joystickIds.Length; i++)
                    {
                        var joystickId = joystickIds[i];
                        var joystick = SDL.OpenJoystick(joystickId);
                        
                        if (joystick != IntPtr.Zero)
                        {
                            var name = SDL.GetJoystickName(joystick);
                            _devices[_nextDeviceId] = new DeviceInfo
                            {
                                Id = _nextDeviceId++,
                                Type = IInputBackend.DeviceType.Joystick,
                                Name = name ?? $"Joystick {i}",
                                JoystickHandle = joystick
                            };

                            _logger.LogInformation("[SDL3Input] Found joystick: {Name}", name ?? $"Joystick {i}");
                        }
                    }
                }

                _initialized = true;
                _logger.LogInformation("[SDL3Input] Input backend initialized with {DeviceCount} devices", _devices.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SDL3Input] Failed to initialize input backend");
                return false;
            }
        }
    }

    public List<(uint DeviceId, string Name, IInputBackend.DeviceType Type)> GetDevices()
    {
        if (!_initialized)
        {
            return new List<(uint DeviceId, string Name, IInputBackend.DeviceType Type)>();
        }

        lock (_lock)
        {
            return _devices.Values.Select(d => ((uint)d.Id, d.Name, d.Type)).ToList();
        }
    }

    public uint OpenDevice(uint deviceId, IInputBackend.DeviceType type)
    {
        // Devices are already opened during initialization
        // This is a no-op for SDL3 backend
        return deviceId;
    }

    public bool CloseDevice(uint deviceId)
    {
        // Devices will be closed during dispose
        // This is a no-op for SDL3 backend
        return true;
    }

    public bool PollDevice(uint deviceId, out IInputBackend.InputState? state)
    {
        state = null;

        if (!_initialized)
        {
            return false;
        }

        if (!_devices.TryGetValue((int)deviceId, out var device))
        {
            return false;
        }

        state = new IInputBackend.InputState();

        if (device.Type == IInputBackend.DeviceType.Joystick && device.JoystickHandle != IntPtr.Zero)
        {
            var numAxes = SDL.GetNumJoystickAxes(device.JoystickHandle);
            var numButtons = SDL.GetNumJoystickButtons(device.JoystickHandle);

            // Get axis values
            for (var i = 0; i < numAxes; i++)
            {
                var value = SDL.GetJoystickAxis(device.JoystickHandle, i);
                state.Axes[i] = value;
            }

            // Get button states
            for (var i = 0; i < numButtons; i++)
            {
                state.Buttons[i] = SDL.GetJoystickButton(device.JoystickHandle, i);
            }

            // Get POV hat state (if available)
            if (SDL.GetNumJoystickHats(device.JoystickHandle) > 0)
            {
                state.PovHat = SDL.GetJoystickHat(device.JoystickHandle, 0);
            }

            return true;
        }

        return false;
    }

    public void ProcessEvents()
    {
        // SDL event processing is handled by the rendering backend
        // This is a no-op for SDL3 input backend
    }

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// </summary>
    public event EventHandler<UIEventArgs>? UIEvent;

    public bool IsInitialized => _initialized;

    public int DeviceCount => _devices.Count;

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            // Close all joysticks
            foreach (var device in _devices.Values)
            {
                if (device.JoystickHandle != IntPtr.Zero)
                {
                    SDL.CloseJoystick(device.JoystickHandle);
                }
            }

            _devices.Clear();
            SDL.Quit();
            _initialized = false;
            _logger.LogInformation("[SDL3Input] Input backend disposed");
        }

        GC.SuppressFinalize(this);
    }
}
