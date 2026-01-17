using Microsoft.Extensions.Logging;

namespace Win32Emu.Gui.Backends;
using Win32Emu.Rendering;

/// <summary>
/// Null input backend that provides no input events.
/// Used for headless mode or when input is not available.
/// </summary>
public class NullInputBackend : IInputBackend
{
	private readonly ILogger _logger;
	private bool _isInitialized;
	private readonly Dictionary<uint, (string Name, IInputBackend.DeviceType Type)> _devices = new();
	private uint _nextDeviceId = 1;

	/// <summary>
	/// Event fired when a UI event occurs (never fired in null backend)
	/// </summary>
	public event EventHandler<UIEventArgs>? UIEvent;

	public bool IsInitialized => _isInitialized;
	public int DeviceCount => _devices.Count;

	public NullInputBackend(ILogger logger)
	{
		_logger = logger;
	}

	public Task<bool> InitializeAsync()
	{
		_logger.LogInformation("[NullInput] Input backend initialized (no input devices)");
		_isInitialized = true;
		return Task.FromResult(true);
	}

	public List<(uint DeviceId, string Name, IInputBackend.DeviceType Type)> GetDevices()
	{
		// Return empty list - no devices in headless mode
		return new List<(uint, string, IInputBackend.DeviceType)>();
	}

	public uint OpenDevice(uint deviceId, IInputBackend.DeviceType type)
	{
		// Create a virtual device
		var virtualId = _nextDeviceId++;
		_devices[virtualId] = ($"Null {type} Device", type);
		_logger.LogDebug("[NullInput] OpenDevice(deviceId={DeviceId}, type={Type}) -> {VirtualId}", 
			deviceId, type, virtualId);
		return virtualId;
	}

	public bool CloseDevice(uint deviceId)
	{
		_logger.LogDebug("[NullInput] CloseDevice(deviceId={DeviceId})", deviceId);
		return _devices.Remove(deviceId);
	}

	public bool PollDevice(uint deviceId, out IInputBackend.InputState? state)
	{
		// Return empty state - no input in headless mode
		state = new IInputBackend.InputState();
		return true;
	}

	public void ProcessEvents()
	{
		// No-op - no events in headless mode
	}

	public void Dispose()
	{
		_logger.LogInformation("[NullInput] Input backend disposed");
		_devices.Clear();
		_isInitialized = false;
	}
}
