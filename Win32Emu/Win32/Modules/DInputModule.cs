using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;

namespace Win32Emu.Win32.Modules
{
	public class DInputModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

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

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
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

		[DllModuleExport(1, entryPoint: 0x0000B006, Version = "4.90.0.3000")]
		[DllModuleExport(1, entryPoint: 0x0000B126, Version = "5.1.2600.6532")]
		private uint DirectInputCreateA(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
		{
			_logger.LogInformation("[DInput] DirectInputCreateA(hinst=0x{Hinst:X8}, dwVersion=0x{DwVersion:X8}, lplpDirectInput=0x{LplpDirectInput:X8}, pUnkOuter=0x{PUnkOuter:X8})", hinst, dwVersion, lplpDirectInput, pUnkOuter);

			// Create DirectInput object with COM vtable
			var dinputHandle = _nextDInputHandle++;
			var dinputObj = new DirectInputObject
			{
				Handle = dinputHandle,
				Version = dwVersion
			};
			_dinputObjects[dinputHandle] = dinputObj;

			// Initialize input backend if not already done
			if (_env.InputBackend == null)
			{
				_env.InputBackend = Rendering.BackendFactory.CreateInputBackend(_logger);
				_env.InputBackend.Initialize();
			}

// Create COM vtable for IDirectInput interface
			var vtableMethods = new Dictionary<string, Win32.COM.ComMethodInfo>
			{
				{ "QueryInterface", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem)) },
				{ "AddRef", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.AddRef>((cpu, mem) => ComAddRef(cpu, mem)) },
				{ "Release", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.Release>((cpu, mem) => ComRelease(cpu, mem)) },
				{ "CreateDevice", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.CreateDevice>((cpu, mem) => DInput_CreateDevice(cpu, mem, dinputHandle)) },
				{ "EnumDevices", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.EnumDevices>((cpu, mem) => DInput_EnumDevices(cpu, mem)) },
				{ "GetDeviceStatus", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.GetDeviceStatus>((cpu, mem) => DInput_GetDeviceStatus(cpu, mem)) },
				{ "RunControlPanel", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.RunControlPanel>((cpu, mem) => DInput_RunControlPanel(cpu, mem)) },
				{ "Initialize", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.Initialize>((cpu, mem) => DInput_Initialize(cpu, mem)) }
			};

// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObject("IDirectInput", vtableMethods);

// Write COM object pointer to output parameter
			if (lplpDirectInput != 0)
			{
				_env.MemWrite32(lplpDirectInput, comObjectAddr);
			}

			_logger.LogInformation("[DInput] Created IDirectInput COM object at 0x{ComObjectAddr:X8}", comObjectAddr);
			return 0; // DI_OK
		}

		[DllModuleExport(1)]
		private uint DirectInputCreate(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
		{
			_logger.LogInformation("[DInput] DirectInputCreate(hinst=0x{Hinst:X8}, dwVersion=0x{DwVersion:X8}, lplpDirectInput=0x{LplpDirectInput:X8}, pUnkOuter=0x{PUnkOuter:X8})", hinst, dwVersion, lplpDirectInput, pUnkOuter);

			// Create DirectInput object with COM vtable (same as DirectInputCreateA)
			var dinputHandle = _nextDInputHandle++;
			var dinputObj = new DirectInputObject
			{
				Handle = dinputHandle,
				Version = dwVersion
			};
			_dinputObjects[dinputHandle] = dinputObj;

			// Initialize input backend if not already done
			if (_env.InputBackend == null)
			{
				_env.InputBackend = Rendering.BackendFactory.CreateInputBackend(_logger);
				_env.InputBackend.Initialize();
			}

			// Create COM vtable for IDirectInput interface
			var vtableMethods = new Dictionary<string, Win32.COM.ComMethodInfo>
			{
				{ "QueryInterface", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem)) },
				{ "AddRef", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.AddRef>((cpu, mem) => ComAddRef(cpu, mem)) },
				{ "Release", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.Release>((cpu, mem) => ComRelease(cpu, mem)) },
				{ "CreateDevice", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.CreateDevice>((cpu, mem) => DInput_CreateDevice(cpu, mem, dinputHandle)) },
				{ "EnumDevices", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.EnumDevices>((cpu, mem) => DInput_EnumDevices(cpu, mem)) },
				{ "GetDeviceStatus", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.GetDeviceStatus>((cpu, mem) => DInput_GetDeviceStatus(cpu, mem)) },
				{ "RunControlPanel", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.RunControlPanel>((cpu, mem) => DInput_RunControlPanel(cpu, mem)) },
				{ "Initialize", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInput.Initialize>((cpu, mem) => DInput_Initialize(cpu, mem)) }
			};

			// Create the COM object with vtable
			var comObjectAddr = _env.ComDispatcher.CreateComObject("IDirectInput", vtableMethods);

			// Write COM object pointer to output parameter
			if (lplpDirectInput != 0)
			{
				_env.MemWrite32(lplpDirectInput, comObjectAddr);
			}

			_logger.LogInformation("[DInput] Created IDirectInput COM object at 0x{ComObjectAddr:X8}", comObjectAddr);
			return 0; // DI_OK
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
			var deviceMethods = new Dictionary<string, Win32.COM.ComMethodInfo>
			{
				{ "QueryInterface", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.QueryInterface>((cpu, mem) => ComQueryInterface(cpu, mem)) },
				{ "AddRef", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.AddRef>((cpu, mem) => ComAddRef(cpu, mem)) },
				{ "Release", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.Release>((cpu, mem) => ComRelease(cpu, mem)) },
				{ "GetCapabilities", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetCapabilities>((cpu, mem) => DInputDevice_GetCapabilities(cpu, mem)) },
				{ "EnumObjects", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.EnumObjects>((cpu, mem) => DInputDevice_EnumObjects(cpu, mem)) },
				{ "GetProperty", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetProperty>((cpu, mem) => DInputDevice_GetProperty(cpu, mem)) },
				{ "SetProperty", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.SetProperty>((cpu, mem) => DInputDevice_SetProperty(cpu, mem)) },
				{ "Acquire", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.Acquire>((cpu, mem) => DInputDevice_Acquire(cpu, mem)) },
				{ "Unacquire", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.Unacquire>((cpu, mem) => DInputDevice_Unacquire(cpu, mem)) },
				{ "GetDeviceState", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetDeviceState>((cpu, mem) => DInputDevice_GetDeviceState(cpu, mem)) },
				{ "GetDeviceData", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetDeviceData>((cpu, mem) => DInputDevice_GetDeviceData(cpu, mem)) },
				{ "SetDataFormat", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.SetDataFormat>((cpu, mem) => DInputDevice_SetDataFormat(cpu, mem)) },
				{ "SetEventNotification", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.SetEventNotification>((cpu, mem) => DInputDevice_SetEventNotification(cpu, mem)) },
				{ "SetCooperativeLevel", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.SetCooperativeLevel>((cpu, mem) => DInputDevice_SetCooperativeLevel(cpu, mem)) },
				{ "GetObjectInfo", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetObjectInfo>((cpu, mem) => DInputDevice_GetObjectInfo(cpu, mem)) },
				{ "GetDeviceInfo", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.GetDeviceInfo>((cpu, mem) => DInputDevice_GetDeviceInfo(cpu, mem)) },
				{ "RunControlPanel", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.RunControlPanel>((cpu, mem) => DInputDevice_RunControlPanel(cpu, mem)) },
				{ "Initialize", Win32.COM.ComVtableDispatcher.FromDelegate<Win32.COM.IDirectInputDevice.Initialize>((cpu, mem) => DInputDevice_Initialize(cpu, mem)) }
			};

			var deviceComAddr = _env.ComDispatcher.CreateComObject("IDirectInputDevice", deviceMethods);

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

			// Parse DIPROPHEADER structure if pdiph is valid
			if (pdiph != 0)
			{
				// typedef struct DIPROPHEADER {
				//   DWORD dwSize;      // +0: Size of enclosing structure
				//   DWORD dwHeaderSize;// +4: Size of DIPROPHEADER (16 or 20 bytes)
				//   DWORD dwObj;       // +8: Object ID or 0 for device
				//   DWORD dwHow;       // +12: DIPH_DEVICE, DIPH_BYOFFSET, DIPH_BYID, etc.
				// } DIPROPHEADER;

				var diph = new DiPropHeaderRef(_env.Memory, pdiph);
				// Removed: using diph.dwHeaderSize
				// Removed: using diph.dwObj
				// Removed: using diph.dwHow

				_logger.LogInformation("[DInput COM]   DIPROPHEADER: size={DwSize}, headerSize={DwHeaderSize}, obj={DwObj}, how={DwHow}",
					diph.dwSize, diph.dwHeaderSize, diph.dwObj, diph.dwHow);

				// For properties like DIPROP_BUFFERSIZE, there's additional data after the header
				// DIPROPDWORD contains: DIPROPHEADER diph; DWORD dwData;
			if (diph.dwSize >= diph.dwHeaderSize + 4)
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
				return 0x8007001E; // DIERR_NOTACQUIRED
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
							// DirectInput keyboard format: 256 bytes, one per key
							if (cbData >= 256 && lpvData != 0)
							{
								for (var i = 0; i < 256; i++)
								{
									var isPressed = state.KeyStates.TryGetValue(i, out var pressed) && pressed;
									_env.Memory.Write8(lpvData + (uint)i, (byte)(isPressed ? 0x80 : 0x00));
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
				return 0x8007001E; // DIERR_NOTACQUIRED
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

			// For now, return 0 elements (no buffered events)
			// In a full implementation, we would:
			// 1. Check if there are buffered input events in the device's event queue
			// 2. Validate that rgdod buffer size (requestedElementCount * DIDEVICEOBJECTDATA_SIZE) is valid
			// 3. Fill the rgdod buffer with DIDEVICEOBJECTDATA structures:
			//    - DWORD dwOfs       // +0: Offset in data format
			//    - DWORD dwData      // +4: Value (key code, button state, etc.)
			//    - DWORD dwTimeStamp // +8: Timestamp
			//    - DWORD dwSequence  // +12: Sequence number
			// 4. Update pdwInOut with the actual number of elements returned

			// Return 0 elements for now
			if (pdwInOut != 0)
			{
				_env.MemWrite32(pdwInOut, 0);
			}

			return 0; // DI_OK
		}

		private uint DInputDevice_SetDataFormat(ICpu cpu, VirtualMemory memory)
		{
			var args = new StackArgs(cpu, memory);
			var thisPtr = args.UInt32(0);
			var lpdf = args.UInt32(1);

			_logger.LogInformation("[DInput COM] IDirectInputDevice::SetDataFormat(this=0x{ThisPtr:X8}, lpdf=0x{Lpdf:X8})", thisPtr, lpdf);

			// Find the device object based on this pointer
			// In COM, we need to look up the device by the COM object address
			DirectInputDevice? device = null;
			foreach (var dev in _devices.Values)
			{
				device = dev;
				break; // For now, use the first device - in production would need proper COM object lookup
			}

			if (device != null && lpdf != 0)
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
			if ((dwFlags & 0x01) != 0) flagNames.Add("DISCL_EXCLUSIVE");
			if ((dwFlags & 0x02) != 0) flagNames.Add("DISCL_NONEXCLUSIVE");
			if ((dwFlags & 0x04) != 0) flagNames.Add("DISCL_FOREGROUND");
			if ((dwFlags & 0x08) != 0) flagNames.Add("DISCL_BACKGROUND");
			if ((dwFlags & 0x10) != 0) flagNames.Add("DISCL_NOWINKEY");

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