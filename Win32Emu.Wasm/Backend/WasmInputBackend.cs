using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Win32Emu.Rendering;
using static Win32Emu.Rendering.IInputBackend;

namespace Win32Emu.Wasm.Backend;

/// <summary>
/// WASM-compatible input backend using JavaScript interop for keyboard/mouse events
/// </summary>
public class WasmInputBackend : IInputBackend
{
	private readonly IJSRuntime _jsRuntime;
	private readonly ILogger<WasmInputBackend> _logger;
	private bool _initialized;
	private readonly Dictionary<uint, InputState> _deviceStates = new();
	private uint _nextDeviceId = 1;

	// Current input state (updated via JS interop)
	private readonly InputState _keyboardState = new();
	private readonly InputState _mouseState = new();

	public event EventHandler<UIEventArgs>? UIEvent;

	public bool IsInitialized => _initialized;
	public int DeviceCount => _deviceStates.Count;

	public WasmInputBackend(IJSRuntime jsRuntime, ILogger<WasmInputBackend> logger)
	{
		_jsRuntime = jsRuntime;
		_logger = logger;
	}

	public bool Initialize()
	{
		if (_initialized)
		{
			return true;
		}

		try
		{
			_logger.LogInformation("[WASM] Initializing input backend");
			
			// Input events will be handled via JavaScript event listeners
			// registered in index.html and forwarded to this backend
			
			_initialized = true;
			_logger.LogInformation("[WASM] Input backend initialized successfully");
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[WASM] Failed to initialize input backend");
			return false;
		}
	}

	public List<(uint DeviceId, string Name, DeviceType Type)> GetDevices()
	{
		// In WASM, we always have keyboard and mouse available
		return new List<(uint, string, DeviceType)>
		{
			(1, "Browser Keyboard", DeviceType.Keyboard),
			(2, "Browser Mouse", DeviceType.Mouse)
		};
	}

	public uint OpenDevice(uint deviceId, DeviceType type)
	{
		if (!_initialized)
		{
			_logger.LogWarning("[WASM] Cannot open device: backend not initialized");
			return 0;
		}

		var newId = _nextDeviceId++;
		var state = type switch
		{
			DeviceType.Keyboard => _keyboardState,
			DeviceType.Mouse => _mouseState,
			_ => new InputState()
		};
		
		_deviceStates[newId] = state;
		_logger.LogInformation("[WASM] Opened input device {DeviceId} ({Type})", newId, type);
		
		return newId;
	}

	public bool CloseDevice(uint deviceId)
	{
		if (_deviceStates.Remove(deviceId))
		{
			_logger.LogInformation("[WASM] Closed input device {DeviceId}", deviceId);
			return true;
		}
		return false;
	}

	public bool PollDevice(uint deviceId, out InputState? state)
	{
		if (_deviceStates.TryGetValue(deviceId, out state))
		{
			return true;
		}
		
		state = null;
		return false;
	}

	public void ProcessEvents()
	{
		// In WASM, events are processed asynchronously via JS callbacks
		// This method could trigger a JS call to poll for pending events
	}

	/// <summary>
	/// Called from JavaScript to update key state
	/// </summary>
	[JSInvokable]
	public void OnKeyDown(int keyCode)
	{
		_keyboardState.KeyStates[keyCode] = true;
		UIEvent?.Invoke(this, new UIEventArgs
		{
			EventType = UIEventType.KeyDown,
			KeyCode = keyCode
		});
	}

	/// <summary>
	/// Called from JavaScript to update key state
	/// </summary>
	[JSInvokable]
	public void OnKeyUp(int keyCode)
	{
		_keyboardState.KeyStates[keyCode] = false;
		UIEvent?.Invoke(this, new UIEventArgs
		{
			EventType = UIEventType.KeyUp,
			KeyCode = keyCode
		});
	}

	/// <summary>
	/// Called from JavaScript to update mouse position
	/// </summary>
	[JSInvokable]
	public void OnMouseMove(int x, int y)
	{
		_mouseState.MouseX = x;
		_mouseState.MouseY = y;
		UIEvent?.Invoke(this, new UIEventArgs
		{
			EventType = UIEventType.MouseMove,
			MouseX = x,
			MouseY = y
		});
	}

	/// <summary>
	/// Called from JavaScript to update mouse button state
	/// </summary>
	[JSInvokable]
	public void OnMouseDown(int button, int x, int y)
	{
		_mouseState.MouseButtons[button] = true;
		_mouseState.MouseX = x;
		_mouseState.MouseY = y;
		UIEvent?.Invoke(this, new UIEventArgs
		{
			EventType = UIEventType.MouseButtonDown,
			MouseX = x,
			MouseY = y,
			WParam = (uint)button // Use WParam to pass button number
		});
	}

	/// <summary>
	/// Called from JavaScript to update mouse button state
	/// </summary>
	[JSInvokable]
	public void OnMouseUp(int button, int x, int y)
	{
		_mouseState.MouseButtons[button] = false;
		_mouseState.MouseX = x;
		_mouseState.MouseY = y;
		UIEvent?.Invoke(this, new UIEventArgs
		{
			EventType = UIEventType.MouseButtonUp,
			MouseX = x,
			MouseY = y,
			WParam = (uint)button // Use WParam to pass button number
		});
	}

	public void Dispose()
	{
		if (_initialized)
		{
			_logger.LogInformation("[WASM] Disposing input backend");
			_deviceStates.Clear();
			_initialized = false;
		}
	}
}
