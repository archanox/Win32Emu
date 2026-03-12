using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;
using Win32Emu.Threading;
using Win32Emu.Win32.Input;

namespace Win32Emu.Win32.Modules
{
	public class DInputModule : IWin32ModuleAsync
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		// Temporary storage for CPU and memory during callbacks
		private ICpu? _currentCpu;
		private VirtualMemory? _currentMemory;

		public DInputModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "DINPUT.DLL";

		// DirectInput object handles
		private readonly Dictionary<uint, DirectInputObject> _dinputObjects = new();
		private readonly Dictionary<uint, DirectInputDevice> _devices = new();
		private uint _nextDInputHandle = 0x90000000;
		private uint _nextDeviceHandle = 0x91000000;

		// DirectInput constants
		private const uint DIDEVICEOBJECTDATA_SIZE = 16; // sizeof(DIDEVICEOBJECTDATA)
		private const uint DI_OK = 0; // Success return value
		private const uint DIERR_INVALIDPARAM = 0x80070057; // Invalid parameter error (E_INVALIDARG)
		private const uint DIERR_NOTACQUIRED = 0x8007001E; // Device not acquired error
		private const uint DIPROPDWORD_DATA_SIZE = 4; // Size of dwData field in DIPROPDWORD
		
		// DirectInput keyboard constants
		private const int DIKEYBOARD_MAX_KEYS = 256; // Number of keys in DirectInput keyboard
		
		// DirectInput mouse constants
		private const int DIMOUSE_MAX_BUTTONS = 4; // Number of mouse buttons supported
		private const uint DIMOUSE_X_OFFSET = 0; // X axis offset in DIMOUSESTATE
		private const uint DIMOUSE_Y_OFFSET = 4; // Y axis offset in DIMOUSESTATE
		private const uint DIMOUSE_Z_OFFSET = 8; // Z axis (wheel) offset in DIMOUSESTATE
		private const uint DIMOUSE_BUTTON_OFFSET = 12; // First button offset in DIMOUSESTATE
		
		// DirectInput key/button state constants
		private const uint DIKEY_PRESSED = 0x80; // Key/button is pressed
		private const uint DIKEY_RELEASED = 0x00; // Key/button is released
		
		// DIDEVICEOBJECTDATA structure field offsets
		private const uint DIDEVICEOBJECTDATA_DWOFS_OFFSET = 0;
		private const uint DIDEVICEOBJECTDATA_DWDATA_OFFSET = 4;
		private const uint DIDEVICEOBJECTDATA_DWTIMESTAMP_OFFSET = 8;
		private const uint DIDEVICEOBJECTDATA_DWSEQUENCE_OFFSET = 12;

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			_currentCpu = cpu;
			_currentMemory = memory;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "DIRECTINPUTCREATE":
					returnValue = DirectInputCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "DIRECTINPUTCREATEA":
					returnValue = DirectInputCreateA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "DIRECTINPUTCREATEEX":
					returnValue = DirectInputCreateEx(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				default:
					_logger.LogInformation("[DInput] Unimplemented export: {Export}", export);
					return false;
			}
		}

		/// <summary>
		/// Async implementation for Win32 APIs that may call back into emulated code.
		/// Routes APIs through async paths to avoid blocking calls that fail on WASM.
		/// </summary>
		public async Task<(bool success, uint returnValue)> TryInvokeAsync(
			string export,
			ICpu cpu,
			VirtualMemory memory,
			CancellationToken cancellationToken = default)
		{
			_currentCpu = cpu;
			_currentMemory = memory;
			var a = new StackArgs(cpu, memory);

			// Route APIs through async paths to avoid .GetAwaiter().GetResult()
			// which throws PlatformNotSupportedException on WASM
			switch (export.ToUpperInvariant())
			{
				case "DIRECTINPUTCREATEA":
				case "DIRECTINPUTCREATE":
					return (true, await DirectInputCreateAAsync(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3)).ConfigureAwait(false));
			}
			
			// For all other APIs, use synchronous implementation
			if (TryInvokeUnsafe(export, cpu, memory, out var syncReturnValue))
			{
				return (true, syncReturnValue);
			}

			// No async work performed; return failure immediately
			return (false, 0);
		}

		[DllModuleExport(1, entryPoint: 0x0000B006, Version = "4.90.0.3000")]
		[DllModuleExport(1, entryPoint: 0x0000B126, Version = "5.1.2600.6532")]
		private uint DirectInputCreateA(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
		{
			// Sync wrapper for non-WASM runtimes that support .GetAwaiter().GetResult()
			// On WASM, TryInvokeAsync routes directly to DirectInputCreateAAsync, bypassing this method
			if (PlatformHelpers.IsWasm)
			{
				_logger.LogError("[DInput] DirectInputCreateA called on WASM - should use async path");
				return DIERR_INVALIDPARAM;
			}
			
			return DirectInputCreateAAsync(hinst, dwVersion, lplpDirectInput, pUnkOuter).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Async implementation of DirectInputCreateA.
		/// </summary>
		private async Task<uint> DirectInputCreateAAsync(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
		{
			// Fixed: Parameter order now matches MSDN documentation
			// Win32 API: DirectInputCreate(HINSTANCE hinst, DWORD dwVersion, LPDIRECTINPUT *lplpDirectInput, LPUNKNOWN punkOuter)
			_logger.LogInformation("[DInput] DirectInputCreateA(hinst=0x{Hinst:X8}, dwVersion=0x{DwVersion:X8}, lplpDirectInput=0x{LplpDirectInput:X8}, pUnkOuter=0x{PUnkOuter:X8})", hinst, dwVersion, lplpDirectInput, pUnkOuter);

			// Validate output pointer parameter
			if (lplpDirectInput == 0)
			{
				_logger.LogError("[DInput] DirectInputCreateA: lplpDirectInput is NULL");
				return DIERR_INVALIDPARAM;
			}

			// Detect if lplpDirectInput looks like a stack pointer (potential parameter handling bug)
			// Check against actual stack range from PE headers
			if (lplpDirectInput >= _env.StackLimit && lplpDirectInput < _env.StackBase)
			{
				_logger.LogWarning("[DInput] DirectInputCreateA: lplpDirectInput=0x{LplpDirectInput:X8} appears to be a stack address (stack range: 0x{StackLimit:X8}-0x{StackBase:X8}) - this might indicate a parameter handling issue", 
					lplpDirectInput, _env.StackLimit, _env.StackBase);
			}

			// Create DirectInput object with COM vtable
			var dinputHandle = _nextDInputHandle++;
			var dinputObj = new DirectInputObject
			{
				Handle = dinputHandle,
				Version = dwVersion
			};
			_dinputObjects[dinputHandle] = dinputObj;

			// Initialize input backend if not already done
			if (_env.InputBackend == null && _env.BackendFactory != null)
			{
				_env.InputBackend = _env.BackendFactory.CreateInputBackend(_logger);
				var success = await _env.InputBackend.InitializeAsync();
				if (!success)
				{
					_logger.LogError("[DInput] Failed to initialize input backend");
					return 1; // DIERR_GENERIC
				}
				_logger.LogInformation("[DInput] Input backend initialized successfully");
			}

// Create COM vtable for IDirectInput interface
			var vtableMethods = new List<KeyValuePair<string, Win32.COM.ComMethodInfo>>
			{
				new("QueryInterface", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))),
				new("AddRef", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.AddRef>((cpu, mem) => ComAddRef(cpu, mem))),
				new("Release", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.Release>((cpu, mem) => ComRelease(cpu, mem))),
				new("CreateDevice", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.CreateDevice>((cpu, mem) => DInput_CreateDevice(cpu, mem, dinputHandle))),
				new("EnumDevices", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.EnumDevices>((cpu, mem) => DInput_EnumDevices(cpu, mem))),
				new("GetDeviceStatus", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.GetDeviceStatus>((cpu, mem) => DInput_GetDeviceStatus(cpu, mem))),
				new("RunControlPanel", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.RunControlPanel>((cpu, mem) => DInput_RunControlPanel(cpu, mem))),
				new("Initialize", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.Initialize>((cpu, mem) => DInput_Initialize(cpu, mem)))
			};

// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectInput", vtableMethods);

			// Write COM object pointer to output parameter with verification
			_logger.LogInformation("[DInput] Writing COM object 0x{ComObjectAddr:X8} to address 0x{Addr:X8}", comObjectAddr, lplpDirectInput);
			_env.MemWrite32(lplpDirectInput, comObjectAddr);
			
			// Verify the write succeeded by reading back
			var verification = _env.MemRead32(lplpDirectInput);
			if (verification != comObjectAddr)
			{
				_logger.LogError("[DInput] Verification failed! Wrote 0x{Expected:X8} but read back 0x{Actual:X8} from address 0x{Addr:X8}", 
					comObjectAddr, verification, lplpDirectInput);
				return 1; // DIERR_GENERIC
			}
			_logger.LogInformation("[DInput] Verification: Read back 0x{Value:X8} from 0x{Addr:X8} - SUCCESS", verification, lplpDirectInput);

			_logger.LogInformation("[DInput] Created IDirectInput COM object at 0x{ComObjectAddr:X8}", comObjectAddr);
			return 0; // DI_OK
		}

		[DllModuleExport(1)]
		private uint DirectInputCreate(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
		{
			// Sync wrapper for non-WASM runtimes that support .GetAwaiter().GetResult()
			// On WASM, TryInvokeAsync routes directly to DirectInputCreateAAsync, bypassing this method
			if (PlatformHelpers.IsWasm)
			{
				_logger.LogError("[DInput] DirectInputCreate called on WASM - should use async path");
				return DIERR_INVALIDPARAM;
			}
			
			// DirectInputCreate and DirectInputCreateA are identical, so reuse the async implementation
			return DirectInputCreateAAsync(hinst, dwVersion, lplpDirectInput, pUnkOuter).GetAwaiter().GetResult();
		}

		[DllModuleExport(2, entryPoint: 0x0000B060, Version = "4.90.0.3000")]
		[DllModuleExport(2, entryPoint: 0x0000B18E, Version = "5.1.2600.6532")]
		private uint DirectInputCreateEx(uint hinst, uint dwVersion, uint riidltf, uint lplpDirectInput, uint pUnkOuter)
		{
			_logger.LogInformation("[DInput] DirectInputCreateEx(hinst=0x{Hinst:X8}, dwVersion=0x{DwVersion:X8}, riidltf=0x{Riidltf:X8}, lplpDirectInput=0x{LplpDirectInput:X8}, pUnkOuter=0x{PUnkOuter:X8})", hinst, dwVersion, riidltf, lplpDirectInput, pUnkOuter);

			// DirectInputCreateEx is similar to DirectInputCreate but with riidltf parameter
			// The riidltf parameter specifies the desired interface (e.g., IID_IDirectInput7)
			// For now, we ignore the specific interface and create a standard IDirectInput object
			return DirectInputCreate(hinst, dwVersion, lplpDirectInput, pUnkOuter);
		}

		private sealed class DirectInputObject
		{
			public uint Handle { get; set; }
			public uint Version { get; set; }
		}

		private sealed class DirectInputDevice
		{
			public uint Handle { get; set; }
			public uint BackendDeviceId { get; set; }
			public string Name { get; set; } = string.Empty;
			public IInputBackend.DeviceType DeviceType { get; set; }
			public bool IsAcquired { get; set; } // Whether device is acquired
			public uint DataFormat { get; set; } // Pointer to DIDATAFORMAT structure
			public uint DataFormatSize { get; set; } // Size of data format in bytes
			public uint CooperativeHwnd { get; set; } // Window handle for cooperative level
			public uint CooperativeFlags { get; set; } // Cooperative level flags
			public Dictionary<uint, uint> Properties { get; set; } = new(); // Device properties (GUID -> value)
			
			// Previous state for detecting changes (needed for buffered events)
			public Dictionary<int, bool> PreviousKeyStates { get; set; } = new();
			public Dictionary<int, bool> PreviousMouseButtons { get; set; } = new();
			public int PreviousMouseX { get; set; }
			public int PreviousMouseY { get; set; }
			public int PreviousMouseZ { get; set; }
			
			// Buffered input events queue
			public Queue<DeviceObjectData> EventQueue { get; set; } = new();
			public uint EventSequence { get; set; } // Sequence counter for events
		}
		
		// Structure representing a buffered input event (DIDEVICEOBJECTDATA)
		private struct DeviceObjectData
		{
			public uint dwOfs;       // +0: Offset in data format
			public uint dwData;      // +4: Value (key code, button state, etc.)
			public uint dwTimeStamp; // +8: Timestamp
			public uint dwSequence;  // +12: Sequence number
		}

		// COM interface methods for IDirectInput
		private uint ComQueryInterface(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var riid = args.UInt32(1);
			var ppvObject = args.UInt32(2);

			_logger.LogInformation("[DInput COM] IUnknown::QueryInterface(this=0x{ThisPtr:X8}, riid=0x{Riid:X8}, ppvObject=0x{PpvObject:X8})", thisPtr, riid, ppvObject);

			// E_NOINTERFACE = 0x80004002
			return 0x80004002;
		}

		private uint ComAddRef(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DInput COM] IUnknown::AddRef(this=0x{ThisPtr:X8})", thisPtr);
			return 1; // Reference count
		}

		private uint ComRelease(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DInput COM] IUnknown::Release(this=0x{ThisPtr:X8})", thisPtr);
			return 0; // Reference count after release
		}

		private uint DInput_CreateDevice(ICpu cpu, VirtualMemory memory, uint dinputHandle)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var rguid = args.UInt32(1);
			var lplpDirectInputDevice = args.UInt32(2);
			var pUnkOuter = args.UInt32(3);

			_logger.LogInformation("[DInput COM] IDirectInput::CreateDevice(this=0x{ThisPtr:X8}, rguid=0x{Rguid:X8}, lplpDevice=0x{LplpDirectInputDevice:X8}, pUnkOuter=0x{PUnkOuter:X8})", thisPtr, rguid, lplpDirectInputDevice, pUnkOuter);

			// Determine device type from GUID
			// Read GUID structure (16 bytes) from memory
			var deviceType = IInputBackend.DeviceType.Joystick; // Default to joystick
			var deviceName = "Emulated Device";
			uint backendDeviceId = 0;

			if (rguid != 0)
			{
				// Read GUID (Data1, Data2, Data3, Data4[8])
				var guidData1 = _env.MemRead32(rguid);

				// Common DirectInput device GUIDs
				// GUID_SysKeyboard = {6F1D2B61-D5A0-11CF-BFC7-444553540000}
				// GUID_SysMouse = {6F1D2B60-D5A0-11CF-BFC7-444553540000}
				// GUID_Joystick = {6F1D2B70-D5A0-11CF-BFC7-444553540000}

				if (guidData1 == 0x6F1D2B61)
				{
					deviceType = IInputBackend.DeviceType.Keyboard;
					deviceName = "Keyboard";
					// Find keyboard device in backend
					if (_env.InputBackend != null)
					{
						var devices = _env.InputBackend.GetDevices();
						var kbDevice = devices.FirstOrDefault(d => d.Type == IInputBackend.DeviceType.Keyboard);
						backendDeviceId = kbDevice.DeviceId;
					}
				}
				else if (guidData1 == 0x6F1D2B60)
				{
					deviceType = IInputBackend.DeviceType.Mouse;
					deviceName = "Mouse";
					// Find mouse device in backend
					if (_env.InputBackend != null)
					{
						var devices = _env.InputBackend.GetDevices();
						var mouseDevice = devices.FirstOrDefault(d => d.Type == IInputBackend.DeviceType.Mouse);
						backendDeviceId = mouseDevice.DeviceId;
					}
				}
				else
				{
					// Try to find a joystick device
					deviceType = IInputBackend.DeviceType.Joystick;
					if (_env.InputBackend != null)
					{
						var devices = _env.InputBackend.GetDevices();
						var joystickDevice = devices.FirstOrDefault(d => d.Type == IInputBackend.DeviceType.Joystick);
						backendDeviceId = joystickDevice.DeviceId;
						if (backendDeviceId != 0)
						{
							deviceName = joystickDevice.Name;
						}
					}
				}
			}

			// Create a device COM object with its own vtable
			var deviceHandle = _nextDeviceHandle++;
			var deviceObj = new DirectInputDevice
			{
				Handle = deviceHandle,
				Name = deviceName,
				DeviceType = deviceType,
				BackendDeviceId = backendDeviceId,
				IsAcquired = false
			};
			_devices[deviceHandle] = deviceObj;

			// Create COM vtable for IDirectInputDevice interface
			var deviceMethods = new List<KeyValuePair<string, Win32.COM.ComMethodInfo>>
			{
				new("QueryInterface", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem))),
				new("AddRef", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.AddRef>((cpu, mem) => ComAddRef(cpu, mem))),
				new("Release", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.Release>((cpu, mem) => ComRelease(cpu, mem))),
				new("GetCapabilities", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetCapabilities>((cpu, mem) => DInputDevice_GetCapabilities(cpu, mem))),
				new("EnumObjects", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.EnumObjects>((cpu, mem) => DInputDevice_EnumObjects(cpu, mem))),
				new("GetProperty", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetProperty>((cpu, mem) => DInputDevice_GetProperty(cpu, mem))),
				new("SetProperty", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.SetProperty>((cpu, mem) => DInputDevice_SetProperty(cpu, mem))),
				new("Acquire", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.Acquire>((cpu, mem) => DInputDevice_Acquire(cpu, mem))),
				new("Unacquire", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.Unacquire>((cpu, mem) => DInputDevice_Unacquire(cpu, mem))),
				new("GetDeviceState", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetDeviceState>((cpu, mem) => DInputDevice_GetDeviceState(cpu, mem))),
				new("GetDeviceData", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetDeviceData>((cpu, mem) => DInputDevice_GetDeviceData(cpu, mem))),
				new("SetDataFormat", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.SetDataFormat>((cpu, mem) => DInputDevice_SetDataFormat(cpu, mem))),
				new("SetEventNotification", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.SetEventNotification>((cpu, mem) => DInputDevice_SetEventNotification(cpu, mem))),
				new("SetCooperativeLevel", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.SetCooperativeLevel>((cpu, mem) => DInputDevice_SetCooperativeLevel(cpu, mem))),
				new("GetObjectInfo", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetObjectInfo>((cpu, mem) => DInputDevice_GetObjectInfo(cpu, mem))),
				new("GetDeviceInfo", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetDeviceInfo>((cpu, mem) => DInputDevice_GetDeviceInfo(cpu, mem))),
				new("RunControlPanel", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.RunControlPanel>((cpu, mem) => DInputDevice_RunControlPanel(cpu, mem))),
				new("Initialize", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.Initialize>((cpu, mem) => DInputDevice_Initialize(cpu, mem)))
			};

			var deviceComAddr = _env.ComDispatcher.CreateComObjectOrdered("IDirectInputDevice", deviceMethods);

			if (lplpDirectInputDevice != 0)
			{
				_env.MemWrite32(lplpDirectInputDevice, deviceComAddr);
			}

			_logger.LogInformation("[DInput COM] Created IDirectInputDevice COM object at 0x{DeviceComAddr:X8}", deviceComAddr);
			return 0; // DI_OK
		}

		private uint DInput_EnumDevices(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInput::EnumDevices() - stub");
			return 0; // DI_OK
		}

		private uint DInput_GetDeviceStatus(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInput::GetDeviceStatus() - stub");
			return 0; // DI_OK
		}

		private uint DInput_RunControlPanel(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInput::RunControlPanel() - stub");
			return 0; // DI_OK
		}

		private uint DInput_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInput::Initialize() - stub");
			return 0; // DI_OK
		}

		// IDirectInputDevice COM methods
		private uint DInputDevice_GetCapabilities(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::GetCapabilities() - stub");
			return 0; // DI_OK
		}

		private uint DInputDevice_EnumObjects(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::EnumObjects() - stub");
			return 0; // DI_OK
		}

		private uint DInputDevice_GetProperty(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::GetProperty() - stub");
			return 0; // DI_OK
		}

		private uint DInputDevice_SetProperty(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var rguidProp = args.UInt32(1);
			var pdiph = args.UInt32(2);

			_logger.LogInformation("[DInput COM] IDirectInputDevice::SetProperty(this=0x{ThisPtr:X8}, rguidProp=0x{RguidProp:X8}, pdiph=0x{Pdiph:X8})", thisPtr, rguidProp, pdiph);

			// Common DirectInput property GUIDs (predefined values):
			// DIPROP_BUFFERSIZE      = 1  // Set buffer size
			// DIPROP_AXISMODE        = 2  // Set axis mode (absolute/relative)
			// DIPROP_GRANULARITY     = 3  // Get granularity
			// DIPROP_RANGE           = 4  // Set/get range
			// DIPROP_DEADZONE        = 5  // Set/get deadzone
			// DIPROP_SATURATION      = 6  // Set/get saturation
			// DIPROP_FFGAIN          = 7  // Set/get force feedback gain
			// DIPROP_FFLOAD          = 8  // Get force feedback load
			// DIPROP_AUTOCENTER      = 9  // Set/get auto-center
			// DIPROP_CALIBRATIONMODE = 10 // Set/get calibration mode

			// Validate pdiph pointer
			if (pdiph == 0)
			{
				_logger.LogError("[DInput COM] SetProperty: pdiph is NULL");
				return DIERR_INVALIDPARAM;
			}

			try
			{
				// typedef struct DIPROPHEADER {
				//   DWORD dwSize;      // +0: Size of enclosing structure
				//   DWORD dwHeaderSize;// +4: Size of DIPROPHEADER (16 or 20 bytes)
				//   DWORD dwObj;       // +8: Object ID or 0 for device
				//   DWORD dwHow;       // +12: DIPH_DEVICE, DIPH_BYOFFSET, DIPH_BYID, etc.
				// } DIPROPHEADER;

				var diph = new DiPropHeaderRef(_env.Memory, pdiph);

				_logger.LogInformation("[DInput COM]   DIPROPHEADER: size={DwSize}, headerSize={DwHeaderSize}, obj={DwObj}, how={DwHow}",
					diph.dwSize, diph.dwHeaderSize, diph.dwObj, diph.dwHow);

				// For properties like DIPROP_BUFFERSIZE, there's additional data after the header
				// DIPROPDWORD contains: DIPROPHEADER diph; DWORD dwData;
				if (diph.dwSize >= diph.dwHeaderSize + DIPROPDWORD_DATA_SIZE)
				{
					var dwData = _env.MemRead32(pdiph + diph.dwHeaderSize);
					_logger.LogInformation("[DInput COM]   Property value: {DwData}", dwData);

					// Store the property
					DirectInputDevice? device = null;
					foreach (var dev in _devices.Values)
					{
						device = dev;
						break; // For now, use the first device
					}

					if (device != null)
					{
						device.Properties[rguidProp] = dwData;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DInput COM] SetProperty: Failed to read DIPROPHEADER structure at 0x{Pdiph:X8}", pdiph);
				return DIERR_INVALIDPARAM;
			}

			return 0; // DI_OK
		}

		private uint DInputDevice_Acquire(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DInput COM] IDirectInputDevice::Acquire(this=0x{ThisPtr:X8})", thisPtr);

			// Find the device associated with this COM object
			var device = _devices.Values.FirstOrDefault(d => true); // TODO: Map thisPtr to device

			if (device != null)
			{
				device.IsAcquired = true;
				_logger.LogInformation("[DInput COM]   Device acquired successfully");
			}

			return 0; // DI_OK
		}

		private uint DInputDevice_Unacquire(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);

			_logger.LogInformation("[DInput COM] IDirectInputDevice::Unacquire(this=0x{ThisPtr:X8})", thisPtr);

			// Find the device and mark it as not acquired
			DirectInputDevice? device = null;
			foreach (var dev in _devices.Values)
			{
				device = dev;
				break; // For now, use the first device
			}

			if (device != null)
			{
				device.IsAcquired = false;
				_logger.LogInformation("[DInput COM]   Device unacquired successfully");
			}

			return 0; // DI_OK
		}

		private uint DInputDevice_GetDeviceState(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var cbData = args.UInt32(1);
			var lpvData = args.UInt32(2);

			_logger.LogInformation("[DInput COM] IDirectInputDevice::GetDeviceState(this=0x{ThisPtr:X8}, cbData={CbData}, lpvData=0x{LpvData:X8})", thisPtr, cbData, lpvData);

			// Find the device associated with this COM object
			if (!_devices.TryGetValue(thisPtr, out var device) || !device.IsAcquired)
			{
				_logger.LogWarning("[DInput COM] Device not acquired or not found");
				return DIERR_NOTACQUIRED;
			}

			// Zero out the buffer first
			if (lpvData != 0 && cbData > 0)
			{
				_env.MemZero(lpvData, cbData);
			}

			// Poll input from backend if available
			if (_env.InputBackend != null && device.BackendDeviceId != 0)
			{
				if (_env.InputBackend.PollDevice(device.BackendDeviceId, out var state) && state != null)
				{
					// Convert backend state to DirectInput format
					switch (device.DeviceType)
					{
						case IInputBackend.DeviceType.Keyboard:
							// DirectInput keyboard format: 256 bytes indexed by DIK scan code.
							// The backend stores keys by Win32 VK code, so convert VK→DIK here.
							// KeyCodeMapper returns 0 for unmapped keys; DIK 0 is reserved/invalid.
							if (cbData >= 256 && lpvData != 0)
							{
								foreach (var (vk, pressed) in state.KeyStates)
								{
									var dik = KeyCodeMapper.VkToDik(vk);
									if (dik > 0 && dik < 256)
									{
										_env.Memory.Write8(lpvData + (uint)dik, (byte)(pressed ? 0x80 : 0x00));
									}
								}
							}

							break;

						case IInputBackend.DeviceType.Mouse:
							// DirectInput mouse format: DIMOUSESTATE structure
							// struct { LONG lX; LONG lY; LONG lZ; BYTE rgbButtons[4]; }
							if (cbData >= 16 && lpvData != 0)
							{
								_env.Memory.Write32(lpvData + 0, (uint)state.MouseX); // lX
								_env.Memory.Write32(lpvData + 4, (uint)state.MouseY); // lY
								_env.Memory.Write32(lpvData + 8, (uint)state.MouseZ); // lZ (wheel)
								for (var i = 0; i < 4; i++)
								{
									var isPressed = state.MouseButtons.TryGetValue(i, out var pressed) && pressed;
									_env.Memory.Write8(lpvData + 12 + (uint)i, (byte)(isPressed ? 0x80 : 0x00));
								}
							}

							break;

						case IInputBackend.DeviceType.Joystick:
							// DirectInput joystick format: DIJOYSTATE structure
							// Simplified: axes (4 LONGs) + POV (DWORD) + buttons (32 BYTEs)
							if (lpvData != 0)
							{
								var offset = 0u;
								// Axes (X, Y, Z, Rx)
								for (var i = 0; i < 4 && offset < cbData; i++)
								{
									var axisValue = state.Axes.TryGetValue(i, out var val) ? val : (short)0;
									// Convert -32768..32767 to 0..65535 for DirectInput
									var dinputValue = (uint)(axisValue + 32768);
									_env.Memory.Write32(lpvData + offset, dinputValue);
									offset += 4;
								}

								// POV hat
								if (offset + 4 <= cbData)
								{
									_env.Memory.Write32(lpvData + offset, (uint)state.PovHat);
									offset += 4;
								}

								// Buttons
								for (var i = 0; i < 32 && offset < cbData; i++)
								{
									var isPressed = state.Buttons.TryGetValue(i, out var pressed) && pressed;
									_env.Memory.Write8(lpvData + offset, (byte)(isPressed ? 0x80 : 0x00));
									offset++;
								}
							}

							break;
					}
				}
			}

			return 0; // DI_OK
		}

		private uint DInputDevice_GetDeviceData(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var cbObjectData = args.UInt32(1);
			var rgdod = args.UInt32(2);
			var pdwInOut = args.UInt32(3);
			var dwFlags = args.UInt32(4);

			_logger.LogInformation("[DInput COM] IDirectInputDevice::GetDeviceData(this=0x{ThisPtr:X8}, cbObjectData={CbObjectData}, rgdod=0x{Rgdod:X8}, pdwInOut=0x{PdwInOut:X8}, dwFlags=0x{DwFlags:X8})",
				thisPtr, cbObjectData, rgdod, pdwInOut, dwFlags);

			// Find the device associated with this COM object
			DirectInputDevice? device = null;
			foreach (var dev in _devices.Values)
			{
				device = dev;
				break; // For now, use the first device
			}

			if (device == null || !device.IsAcquired)
			{
				_logger.LogWarning("[DInput COM] Device not acquired or not found");
				return DIERR_NOTACQUIRED;
			}

			// Read the number of elements requested
			uint requestedElementCount = 0;
			if (pdwInOut != 0)
			{
				requestedElementCount = _env.MemRead32(pdwInOut);
			}

			_logger.LogInformation("[DInput COM]   Requested elements: {RequestedElementCount}", requestedElementCount);

			// Validate cbObjectData parameter
			if (cbObjectData != DIDEVICEOBJECTDATA_SIZE)
			{
				_logger.LogWarning("[DInput COM] Invalid cbObjectData: {CbObjectData}, expected {Expected}", cbObjectData, DIDEVICEOBJECTDATA_SIZE);
				return 0x80070057; // E_INVALIDARG
			}

			// Poll input from backend and generate buffered events
			if (_env.InputBackend != null && device.BackendDeviceId != 0 &&
			    _env.InputBackend.PollDevice(device.BackendDeviceId, out var state) && state != null)
			{
				var timestamp = (uint)Environment.TickCount;
				
				// Generate events based on device type
				switch (device.DeviceType)
				{
					case IInputBackend.DeviceType.Keyboard:
						// The backend stores key state by Win32 VK code.
						// DirectInput events use DIK scan codes as dwOfs, so convert VK→DIK.
						// Iterate current state to detect changes vs the previous poll.
						foreach (var (vk, isPressed) in state.KeyStates)
						{
							var dik = KeyCodeMapper.VkToDik(vk);
							if (dik == 0 || dik >= DIKEYBOARD_MAX_KEYS)
							{
								continue;
							}

							var wasPressed = device.PreviousKeyStates.TryGetValue(vk, out var prevPressed) && prevPressed;

							if (isPressed != wasPressed)
							{
								// Key state changed, add event
								device.EventQueue.Enqueue(new DeviceObjectData
								{
									dwOfs = (uint)dik, // DirectInput scan code (DIK_*)
									dwData = isPressed ? DIKEY_PRESSED : DIKEY_RELEASED,
									dwTimeStamp = timestamp,
									dwSequence = device.EventSequence++
								});

								// Track previous state by VK code
								device.PreviousKeyStates[vk] = isPressed;
							}
						}

						break;

					case IInputBackend.DeviceType.Mouse:
						// Check for mouse button changes
						for (var i = 0; i < DIMOUSE_MAX_BUTTONS; i++)
						{
							var isPressed = state.MouseButtons.TryGetValue(i, out var pressed) && pressed;
							var wasPressed = device.PreviousMouseButtons.TryGetValue(i, out var prevPressed) && prevPressed;
							
							if (isPressed != wasPressed)
							{
								device.EventQueue.Enqueue(new DeviceObjectData
								{
									dwOfs = DIMOUSE_BUTTON_OFFSET + (uint)i,
									dwData = isPressed ? DIKEY_PRESSED : DIKEY_RELEASED,
									dwTimeStamp = timestamp,
									dwSequence = device.EventSequence++
								});
								
								device.PreviousMouseButtons[i] = isPressed;
							}
						}
						
						// Check for mouse movement (X axis) - use relative delta
						var deltaX = state.MouseX - device.PreviousMouseX;
						if (deltaX != 0)
						{
							device.EventQueue.Enqueue(new DeviceObjectData
							{
								dwOfs = DIMOUSE_X_OFFSET,
								dwData = (uint)deltaX,
								dwTimeStamp = timestamp,
								dwSequence = device.EventSequence++
							});
							device.PreviousMouseX = state.MouseX;
						}
						
						// Check for mouse movement (Y axis) - use relative delta
						var deltaY = state.MouseY - device.PreviousMouseY;
						if (deltaY != 0)
						{
							device.EventQueue.Enqueue(new DeviceObjectData
							{
								dwOfs = DIMOUSE_Y_OFFSET,
								dwData = (uint)deltaY,
								dwTimeStamp = timestamp,
								dwSequence = device.EventSequence++
							});
							device.PreviousMouseY = state.MouseY;
						}
						
						// Check for mouse wheel (Z axis) - use relative delta
						var deltaZ = state.MouseZ - device.PreviousMouseZ;
						if (deltaZ != 0)
						{
							device.EventQueue.Enqueue(new DeviceObjectData
							{
								dwOfs = DIMOUSE_Z_OFFSET,
								dwData = (uint)deltaZ,
								dwTimeStamp = timestamp,
								dwSequence = device.EventSequence++
							});
							device.PreviousMouseZ = state.MouseZ;
						}
						break;
				}
			}

			// Determine how many events to return
			var eventsToReturn = Math.Min(requestedElementCount, (uint)device.EventQueue.Count);
			
			// If rgdod is NULL, just return the count
			if (rgdod == 0)
			{
				if (pdwInOut != 0)
				{
					_env.MemWrite32(pdwInOut, (uint)device.EventQueue.Count);
				}
				_logger.LogInformation("[DInput COM]   Returning event count: {Count}", device.EventQueue.Count);
				return DI_OK;
			}
			
			// Write events to output buffer
			for (var i = 0u; i < eventsToReturn; i++)
			{
				var evt = device.EventQueue.Dequeue();
				var offset = rgdod + (i * DIDEVICEOBJECTDATA_SIZE);
				
				// Write DIDEVICEOBJECTDATA structure
				_env.MemWrite32(offset + DIDEVICEOBJECTDATA_DWOFS_OFFSET, evt.dwOfs);
				_env.MemWrite32(offset + DIDEVICEOBJECTDATA_DWDATA_OFFSET, evt.dwData);
				_env.MemWrite32(offset + DIDEVICEOBJECTDATA_DWTIMESTAMP_OFFSET, evt.dwTimeStamp);
				_env.MemWrite32(offset + DIDEVICEOBJECTDATA_DWSEQUENCE_OFFSET, evt.dwSequence);
			}

			// Update output count
			if (pdwInOut != 0)
			{
				_env.MemWrite32(pdwInOut, eventsToReturn);
			}
			
			_logger.LogInformation("[DInput COM]   Returned {EventsReturned} events, {EventsRemaining} remaining", eventsToReturn, device.EventQueue.Count);

			return DI_OK;
		}

		private uint DInputDevice_SetDataFormat(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpdf = args.UInt32(1);

			_logger.LogInformation("[DInput COM] IDirectInputDevice::SetDataFormat(this=0x{ThisPtr:X8}, lpdf=0x{Lpdf:X8})", thisPtr, lpdf);

			// Validate lpdf pointer
			if (lpdf == 0)
			{
				_logger.LogError("[DInput COM] SetDataFormat: lpdf is NULL");
				return DIERR_INVALIDPARAM;
			}

			// Find the device object based on this pointer
			// In COM, we need to look up the device by the COM object address
			DirectInputDevice? device = null;
			foreach (var dev in _devices.Values)
			{
				device = dev;
				break; // For now, use the first device - in production would need proper COM object lookup
			}

			if (device == null)
			{
				_logger.LogError("[DInput COM] SetDataFormat: Device not found for this=0x{ThisPtr:X8}", thisPtr);
				return DIERR_INVALIDPARAM;
			}

			try
			{
				// Parse DIDATAFORMAT structure
				// typedef struct DIDATAFORMAT {
				//   DWORD dwSize;        // +0: Size of this structure
				//   DWORD dwObjSize;     // +4: Size of DIOBJECTDATAFORMAT
				//   DWORD dwFlags;       // +8: DIDF_ABSAXIS or DIDF_RELAXIS
				//   DWORD dwDataSize;    // +12: Size of device data
				//   DWORD dwNumObjs;     // +16: Number of objects
				//   LPDIOBJECTDATAFORMAT rgodf; // +20: Array of object formats
				// } DIDATAFORMAT;

				var df = new DiDataFormatRef(_env.Memory, lpdf);

				_logger.LogInformation("[DInput COM]   DIDATAFORMAT: size={DwSize}, objSize={DwObjSize}, flags=0x{DwFlags:X}, dataSize={DwDataSize}, numObjs={DwNumObjs}, rgodf=0x{Rgodf:X8}",
					df.dwSize, df.dwObjSize, df.dwFlags, df.dwDataSize, df.dwNumObjs, df.rgodf);

				// Store the data format information
				device.DataFormat = lpdf;
				device.DataFormatSize = df.dwDataSize;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[DInput COM] SetDataFormat: Failed to read DIDATAFORMAT structure at 0x{Lpdf:X8}", lpdf);
				return DIERR_INVALIDPARAM;
			}

			return 0; // DI_OK
		}

		private uint DInputDevice_SetEventNotification(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::SetEventNotification() - stub");
			return 0; // DI_OK
		}

		private uint DInputDevice_SetCooperativeLevel(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var hwnd = args.UInt32(1);
			var dwFlags = args.UInt32(2);

			_logger.LogInformation("[DInput COM] IDirectInputDevice::SetCooperativeLevel(this=0x{ThisPtr:X8}, hwnd=0x{Hwnd:X8}, flags=0x{DwFlags:X8})", thisPtr, hwnd, dwFlags);

			// DirectInput Cooperative Level Flags:
			// DISCL_EXCLUSIVE    = 0x00000001  // Exclusive access
			// DISCL_NONEXCLUSIVE = 0x00000002  // Non-exclusive access
			// DISCL_FOREGROUND   = 0x00000004  // Foreground access
			// DISCL_BACKGROUND   = 0x00000008  // Background access
			// DISCL_NOWINKEY     = 0x00000010  // Disable Windows key

			var flagNames = new List<string>();
			if ((dwFlags & 0x01) != 0)
			{
				flagNames.Add("DISCL_EXCLUSIVE");
			}

			if ((dwFlags & 0x02) != 0)
			{
				flagNames.Add("DISCL_NONEXCLUSIVE");
			}

			if ((dwFlags & 0x04) != 0)
			{
				flagNames.Add("DISCL_FOREGROUND");
			}

			if ((dwFlags & 0x08) != 0)
			{
				flagNames.Add("DISCL_BACKGROUND");
			}

			if ((dwFlags & 0x10) != 0)
			{
				flagNames.Add("DISCL_NOWINKEY");
			}

			if (flagNames.Count > 0)
			{
				_logger.LogInformation("[DInput COM]   Flags: {FlagNames}", string.Join(" | ", flagNames));
			}

			// Find the device and store the cooperative level settings
			DirectInputDevice? device = null;
			foreach (var dev in _devices.Values)
			{
				device = dev;
				break; // For now, use the first device
			}

			if (device != null)
			{
				device.CooperativeHwnd = hwnd;
				device.CooperativeFlags = dwFlags;
			}

			return 0; // DI_OK
		}

		private uint DInputDevice_GetObjectInfo(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::GetObjectInfo() - stub");
			return 0; // DI_OK
		}

		private uint DInputDevice_GetDeviceInfo(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::GetDeviceInfo() - stub");
			return 0; // DI_OK
		}

		private uint DInputDevice_RunControlPanel(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::RunControlPanel() - stub");
			return 0; // DI_OK
		}

		private uint DInputDevice_Initialize(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::Initialize() - stub");
			return 0; // DI_OK
		}


		[DllModuleExport(3, entryPoint: 0x0000B033, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(3, entryPoint: 0x0000B15A, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectInputCreateW(uint hinst, uint dwVersion, uint ppDI, uint punkOuter)
		{
			_logger.LogWarning("[dinput] DirectInputCreateW: hinst={hinst}, dwVersion=0x{dwVersion:X8}, ppDI=0x{ppDI:X8}, punkOuter={punkOuter}", hinst, dwVersion, ppDI, punkOuter);
			// TODO: Implement DirectInputCreateW
			return 0; // DWORD default
		}

		[DllModuleExport(4, entryPoint: 0x0000AE2C, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(4, entryPoint: 0x0000AE99, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllCanUnloadNow()
		{
			_logger.LogWarning("[dinput] DllCanUnloadNow called (stub)");
			// TODO: Implement DllCanUnloadNow
			return 0; // DWORD default
		}

		[DllModuleExport(5, entryPoint: 0x0000ADC1, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(5, entryPoint: 0x0000AE27, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllGetClassObject()
		{
			_logger.LogWarning("[dinput] DllGetClassObject called (stub)");
			// TODO: Implement DllGetClassObject
			return 0; // DWORD default
		}

		[DllModuleExport(6, entryPoint: 0x00014C35, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(6, entryPoint: 0x00015E55, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllRegisterServer()
		{
			_logger.LogWarning("[dinput] DllRegisterServer called (stub)");
			// TODO: Implement DllRegisterServer
			return 0; // DWORD default
		}

		[DllModuleExport(7, entryPoint: 0x00014C40, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(7, entryPoint: 0x00015E65, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllUnregisterServer()
		{
			_logger.LogWarning("[dinput] DllUnregisterServer called (stub)");
			// TODO: Implement DllUnregisterServer
			return 0; // DWORD default
		}
	}
}