using SDL3;

namespace Win32Emu.Rendering;

/// <summary>
/// SDL3-based input backend for DirectInput operations
/// </summary>
public class SDL3InputBackend : IDisposable
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
                return true;

            // Initialize SDL3 gamepad and joystick subsystems
            if (!SDL.Init(SDL.InitFlags.Gamepad | SDL.InitFlags.Joystick))
            {
                Console.WriteLine($"[SDL3Input] Failed to initialize: {SDL.GetError()}");
                return false;
            }

            _initialized = true;
            Console.WriteLine("[SDL3Input] Input subsystem initialized");
            
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
            for (int i = 0; i < gamepadCount; i++)
            {
                _gamepadIds.Add(gamepadIds[i]);
                Console.WriteLine($"[SDL3Input] Found gamepad: {gamepadIds[i]}");
            }
        }

        // Get joystick count  
        int joystickCount;
        var joystickIds = SDL.GetJoysticks(out joystickCount);
        if (joystickIds != null)
        {
            for (int i = 0; i < joystickCount; i++)
            {
                if (!_gamepadIds.Contains(joystickIds[i]))
                {
                    _joystickIds.Add(joystickIds[i]);
                    Console.WriteLine($"[SDL3Input] Found joystick: {joystickIds[i]}");
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
            for (int i = 0; i < _gamepadIds.Count; i++)
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
            for (int i = 0; i < _joystickIds.Count; i++)
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
                Console.WriteLine("[SDL3Input] Not initialized");
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
                int index = (int)(deviceId - 0x3000);
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
                int index = (int)(deviceId - 0x4000);
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
            Console.WriteLine($"[SDL3Input] Opened device {internalId}: {device.Name} ({type})");
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
                return false;

            if (device.GamepadHandle != IntPtr.Zero)
            {
                SDL.CloseGamepad(device.GamepadHandle);
            }

            if (device.JoystickHandle != IntPtr.Zero)
            {
                SDL.CloseJoystick(device.JoystickHandle);
            }

            _devices.Remove(deviceId);
            Console.WriteLine($"[SDL3Input] Closed device {deviceId}");
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
                return false;

            state = new InputState();

            // Update gamepad state
            if (device.GamepadHandle != IntPtr.Zero)
            {
                // Poll buttons
                for (int i = 0; i < 16; i++)
                {
                    var buttonState = SDL.GetGamepadButton(device.GamepadHandle, (SDL.GamepadButton)i);
                    state.Buttons[i] = buttonState;
                }

                // Poll axes
                for (int i = 0; i < 6; i++)
                {
                    var axisValue = SDL.GetGamepadAxis(device.GamepadHandle, (SDL.GamepadAxis)i);
                    state.Axes[i] = axisValue;
                }
            }
            // Update joystick state
            else if (device.JoystickHandle != IntPtr.Zero)
            {
                int numButtons = SDL.GetNumJoystickButtons(device.JoystickHandle);
                for (int i = 0; i < numButtons; i++)
                {
                    var buttonState = SDL.GetJoystickButton(device.JoystickHandle, i);
                    state.Buttons[i] = buttonState;
                }

                int numAxes = SDL.GetNumJoystickAxes(device.JoystickHandle);
                for (int i = 0; i < numAxes; i++)
                {
                    var axisValue = SDL.GetJoystickAxis(device.JoystickHandle, i);
                    state.Axes[i] = axisValue;
                }

                int numHats = SDL.GetNumJoystickHats(device.JoystickHandle);
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
                return;

            SDL.Event evt;
            while (SDL.PollEvent(out evt))
            {
                switch ((SDL.EventType)evt.Type)
                {
                    case SDL.EventType.GamepadAdded:
                        Console.WriteLine($"[SDL3Input] Gamepad added");
                        EnumerateDevices();
                        break;
                    case SDL.EventType.GamepadRemoved:
                        Console.WriteLine($"[SDL3Input] Gamepad removed");
                        EnumerateDevices();
                        break;
                    case SDL.EventType.JoystickAdded:
                        Console.WriteLine($"[SDL3Input] Joystick added");
                        EnumerateDevices();
                        break;
                    case SDL.EventType.JoystickRemoved:
                        Console.WriteLine($"[SDL3Input] Joystick removed");
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
                return;

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
            Console.WriteLine("[SDL3Input] Input subsystem disposed");
        }
    }

    public bool IsInitialized => _initialized;
    public int DeviceCount => _devices.Count;
}
