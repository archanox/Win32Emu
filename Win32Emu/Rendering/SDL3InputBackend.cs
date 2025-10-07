using Microsoft.Extensions.Logging;
using SDL3;

namespace Win32Emu.Rendering;

/// <summary>
/// SDL3-based input backend for DirectInput operations
/// </summary>
public class Sdl3InputBackend(ILogger logger) : IDisposable
{
    private bool _initialized;
    private readonly object _lock = new();
    private readonly Dictionary<uint, InputDevice> _devices = new();
    private readonly List<uint> _gamepadIds = new();
    private readonly List<uint> _joystickIds = new();
    private uint _nextDeviceId = 1;

    private class InputDevice
    {
        public uint Id { get; set; }
        public uint SdlJoystickId { get; set; }
        public IntPtr JoystickHandle { get; set; }
        public IntPtr GamepadHandle { get; set; }
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }
        public InputState State { get; set; } = new();
    }

    public class InputState
    {
        public Dictionary<int, bool> Buttons { get; set; } = new();
        public Dictionary<int, short> Axes { get; set; } = new();
        public int PovHat { get; set; }
    }

    public enum DeviceType
    {
        Keyboard,
        Mouse,
        Gamepad,
        Joystick
    }

    /// <summary>
    /// Initialize SDL3 input subsystem
    /// </summary>
    public bool Initialize()
    {
        lock (_lock)
        {
            if (_initialized)
            {
	            return true;
            }

            // Initialize SDL3 gamepad and joystick subsystems
            if (!SDL.Init(SDL.InitFlags.Gamepad | SDL.InitFlags.Joystick))
            {
                logger.LogError($"[SDL3Input] Failed to initialize: {SDL.GetError()}");
                return false;
            }

            _initialized = true;
            logger.LogInformation("[SDL3Input] Input subsystem initialized");
            
            // Enumerate connected devices
            EnumerateDevices();
            
            return true;
        }
    }

    /// <summary>
    /// Enumerate all connected input devices
    /// </summary>
    private void EnumerateDevices()
    {
        // Get gamepad count
        int gamepadCount;
        var gamepadIds = SDL.GetGamepads(out gamepadCount);
        if (gamepadIds != null)
        {
            for (var i = 0; i < gamepadCount; i++)
            {
                _gamepadIds.Add(gamepadIds[i]);
                logger.LogInformation($"[SDL3Input] Found gamepad: {gamepadIds[i]}");
            }
        }

        // Get joystick count  
        var joystickIds = SDL.GetJoysticks(out var joystickCount);
        if (joystickIds != null)
        {
            for (var i = 0; i < joystickCount; i++)
            {
                if (!_gamepadIds.Contains(joystickIds[i]))
                {
                    _joystickIds.Add(joystickIds[i]);
                    logger.LogInformation($"[SDL3Input] Found joystick: {joystickIds[i]}");
                }
            }
        }
    }

    /// <summary>
    /// Get list of available input devices
    /// </summary>
    public List<(uint DeviceId, string Name, DeviceType Type)> GetDevices()
    {
        lock (_lock)
        {
            var result = new List<(uint, string, DeviceType)>();

            // Always add keyboard and mouse
            result.Add((0x1000, "Keyboard", DeviceType.Keyboard));
            result.Add((0x2000, "Mouse", DeviceType.Mouse));

            // Add gamepads
            for (var i = 0; i < _gamepadIds.Count; i++)
            {
                var gamepadId = _gamepadIds[i];
                var gamepad = SDL.OpenGamepad(gamepadId);
                var name = gamepad != IntPtr.Zero ? SDL.GetGamepadName(gamepad) : $"Gamepad {i}";
                result.Add(((uint)(0x3000 + i), name ?? $"Gamepad {i}", DeviceType.Gamepad));
                if (gamepad != IntPtr.Zero)
                {
                    SDL.CloseGamepad(gamepad);
                }
            }

            // Add joysticks
            for (var i = 0; i < _joystickIds.Count; i++)
            {
                var joystickId = _joystickIds[i];
                var joystick = SDL.OpenJoystick(joystickId);
                var name = joystick != IntPtr.Zero ? SDL.GetJoystickName(joystick) : $"Joystick {i}";
                result.Add(((uint)(0x4000 + i), name ?? $"Joystick {i}", DeviceType.Joystick));
                if (joystick != IntPtr.Zero)
                {
                    SDL.CloseJoystick(joystick);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Open an input device for reading
    /// </summary>
    public uint OpenDevice(uint deviceId, DeviceType type)
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                logger.LogError("[SDL3Input] Not initialized");
                return 0;
            }

            var internalId = _nextDeviceId++;
            var device = new InputDevice
            {
                Id = internalId,
                Type = type,
                Name = $"Device {deviceId}"
            };

            // Open the appropriate SDL device
            if (type == DeviceType.Gamepad)
            {
                var index = (int)(deviceId - 0x3000);
                if (index >= 0 && index < _gamepadIds.Count)
                {
                    device.SdlJoystickId = _gamepadIds[index];
                    device.GamepadHandle = SDL.OpenGamepad(device.SdlJoystickId);
                    if (device.GamepadHandle != IntPtr.Zero)
                    {
                        device.Name = SDL.GetGamepadName(device.GamepadHandle) ?? device.Name;
                    }
                }
            }
            else if (type == DeviceType.Joystick)
            {
                var index = (int)(deviceId - 0x4000);
                if (index >= 0 && index < _joystickIds.Count)
                {
                    device.SdlJoystickId = _joystickIds[index];
                    device.JoystickHandle = SDL.OpenJoystick(device.SdlJoystickId);
                    if (device.JoystickHandle != IntPtr.Zero)
                    {
                        device.Name = SDL.GetJoystickName(device.JoystickHandle) ?? device.Name;
                    }
                }
            }

            _devices[internalId] = device;
            logger.LogInformation($"[SDL3Input] Opened device {internalId}: {device.Name} ({type})");
            return internalId;
        }
    }

    /// <summary>
    /// Close an input device
    /// </summary>
    public bool CloseDevice(uint deviceId)
    {
        lock (_lock)
        {
            if (!_devices.TryGetValue(deviceId, out var device))
            {
	            return false;
            }

            if (device.GamepadHandle != IntPtr.Zero)
            {
                SDL.CloseGamepad(device.GamepadHandle);
            }

            if (device.JoystickHandle != IntPtr.Zero)
            {
                SDL.CloseJoystick(device.JoystickHandle);
            }

            _devices.Remove(deviceId);
            logger.LogInformation($"[SDL3Input] Closed device {deviceId}");
            return true;
        }
    }

    /// <summary>
    /// Poll input state from a device
    /// </summary>
    public bool PollDevice(uint deviceId, out InputState? state)
    {
        lock (_lock)
        {
            state = null;

            if (!_devices.TryGetValue(deviceId, out var device))
            {
	            return false;
            }

            state = new InputState();

            // Update gamepad state
            if (device.GamepadHandle != IntPtr.Zero)
            {
                // Poll buttons
                for (var i = 0; i < 16; i++)
                {
                    var buttonState = SDL.GetGamepadButton(device.GamepadHandle, (SDL.GamepadButton)i);
                    state.Buttons[i] = buttonState;
                }

                // Poll axes
                for (var i = 0; i < 6; i++)
                {
                    var axisValue = SDL.GetGamepadAxis(device.GamepadHandle, (SDL.GamepadAxis)i);
                    state.Axes[i] = axisValue;
                }
            }
            // Update joystick state
            else if (device.JoystickHandle != IntPtr.Zero)
            {
                var numButtons = SDL.GetNumJoystickButtons(device.JoystickHandle);
                for (var i = 0; i < numButtons; i++)
                {
                    var buttonState = SDL.GetJoystickButton(device.JoystickHandle, i);
                    state.Buttons[i] = buttonState;
                }

                var numAxes = SDL.GetNumJoystickAxes(device.JoystickHandle);
                for (var i = 0; i < numAxes; i++)
                {
                    var axisValue = SDL.GetJoystickAxis(device.JoystickHandle, i);
                    state.Axes[i] = axisValue;
                }

                var numHats = SDL.GetNumJoystickHats(device.JoystickHandle);
                if (numHats > 0)
                {
                    state.PovHat = (int)SDL.GetJoystickHat(device.JoystickHandle, 0);
                }
            }

            device.State = state;
            return true;
        }
    }

    /// <summary>
    /// Process SDL input events
    /// </summary>
    public void ProcessEvents()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
	            return;
            }

            SDL.Event evt;
            while (SDL.PollEvent(out evt))
            {
                switch ((SDL.EventType)evt.Type)
                {
                    case SDL.EventType.GamepadAdded:
                        logger.LogInformation("[SDL3Input] Gamepad added");
                        EnumerateDevices();
                        break;
                    case SDL.EventType.GamepadRemoved:
	                    logger.LogInformation("[SDL3Input] Gamepad removed");
                        EnumerateDevices();
                        break;
                    case SDL.EventType.JoystickAdded:
	                    logger.LogInformation("[SDL3Input] Joystick added");
                        EnumerateDevices();
                        break;
                    case SDL.EventType.JoystickRemoved:
	                    logger.LogInformation("[SDL3Input] Joystick removed");
                        EnumerateDevices();
                        break;
                }
            }
        }
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
            _gamepadIds.Clear();
            _joystickIds.Clear();

            // Quit SDL input subsystems
            SDL.QuitSubSystem(SDL.InitFlags.Gamepad | SDL.InitFlags.Joystick);
            _initialized = false;
            logger.LogInformation("[SDL3Input] Input subsystem disposed");
        }
    }

    public bool IsInitialized => _initialized;
    public int DeviceCount => _devices.Count;
}
