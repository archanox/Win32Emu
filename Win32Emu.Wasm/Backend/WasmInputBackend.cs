using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Win32Emu.Rendering;
using static Win32Emu.Rendering.IInputBackend;

namespace Win32Emu.Wasm.Backend;

/// <summary>
/// WASM-compatible input backend using JavaScript interop for keyboard/mouse events.
/// This class is exposed to JavaScript via DotNetObjectReference for event callbacks.
/// </summary>
public class WasmInputBackend : IInputBackend
{
	private readonly IJSRuntime _jsRuntime;
	private readonly ILogger<WasmInputBackend> _logger;
	private bool _initialized;
	private readonly Dictionary<uint, InputState> _deviceStates = new();
	private uint _nextDeviceId = 1;
	private DotNetObjectReference<WasmInputBackend>? _dotNetRef;

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

	/// <summary>
	/// Initialize the input backend and register event listeners in JavaScript.
	/// </summary>
	public async Task<bool> InitializeAsync()
	{
		if (_initialized)
		{
			return true;
		}

		try
		{
			_logger.LogInformation("[WASM] Initializing input backend");
			
			// Create a DotNetObjectReference to pass to JavaScript
			_dotNetRef = DotNetObjectReference.Create(this);
			
			// Initialize input system in JavaScript
			var success = await _jsRuntime.InvokeAsync<bool>("initializeInput", "emulatorCanvas", _dotNetRef);
			
			if (success)
			{
				_initialized = true;
				_logger.LogInformation("[WASM] Input backend initialized successfully");
			}
			else
			{
				_logger.LogWarning("[WASM] Failed to initialize input backend");
			}
			
			return success;
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

	/// <summary>
	/// Opens an input device for reading.
	/// Note: Multiple devices of the same type share the same underlying InputState.
	/// This is intentional because in WASM, there's only one physical keyboard and mouse,
	/// and all "devices" should reflect the same browser input state.
	/// </summary>
	public uint OpenDevice(uint deviceId, DeviceType type)
	{
		if (!_initialized)
		{
			_logger.LogWarning("[WASM] Cannot open device: backend not initialized");
			return 0;
		}

		var newId = _nextDeviceId++;
		// All devices of the same type share state - this is intentional for WASM
		// since there's only one physical keyboard/mouse in the browser
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
		// Dictionary indexer will add the key if it doesn't exist
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
		// Dictionary indexer will add the key if it doesn't exist
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
		// Dictionary indexer will add the key if it doesn't exist
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
		// Dictionary indexer will add the key if it doesn't exist
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
			_dotNetRef?.Dispose();
			_dotNetRef = null;
			_initialized = false;
		}
	}
}
