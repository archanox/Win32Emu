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

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "DIRECTINPUTCREATEA":
				case "DIRECTINPUTCREATE":
					returnValue = DirectInputCreateA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				// TODO: DIRECTINPUT8CREATE needs to move over to DINPUT8.DLL
				case "DIRECTINPUT8CREATE":
					returnValue = DirectInput8Create(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				default:
					_logger.LogInformation("[DInput] Unimplemented export: {Export}", export);
					return false;
			}
		}

		[DllModuleExport(1)]
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

// Create COM vtable for IDirectInput interface
			var vtableMethods = new Dictionary<string, Win32.COM.ComMethodInfo>
			{
				{ "QueryInterface", new Win32.COM.ComMethodInfo((cpu, mem) => ComQueryInterface(cpu, mem), ArgBytes: 12) }, // this + riid + ppvObject
				{ "AddRef", new Win32.COM.ComMethodInfo((cpu, mem) => ComAddRef(cpu, mem), ArgBytes: 4) }, // this only
				{ "Release", new Win32.COM.ComMethodInfo((cpu, mem) => ComRelease(cpu, mem), ArgBytes: 4) }, // this only
				{ "CreateDevice", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_CreateDevice(cpu, mem, dinputHandle), ArgBytes: 16) }, // this + rguid + lplpDevice + pUnkOuter
				{ "EnumDevices", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_EnumDevices(cpu, mem), ArgBytes: 20) }, // this + dwDevType + lpCallback + pvRef + dwFlags
				{ "GetDeviceStatus", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_GetDeviceStatus(cpu, mem), ArgBytes: 8) }, // this + rguidInstance
				{ "RunControlPanel", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_RunControlPanel(cpu, mem), ArgBytes: 8) }, // this + hwndOwner
				{ "Initialize", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_Initialize(cpu, mem), ArgBytes: 8) } // this + hinst
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
				{ "QueryInterface", new Win32.COM.ComMethodInfo((cpu, mem) => ComQueryInterface(cpu, mem), ArgBytes: 12) }, // this + riid + ppvObject
				{ "AddRef", new Win32.COM.ComMethodInfo((cpu, mem) => ComAddRef(cpu, mem), ArgBytes: 4) }, // this only
				{ "Release", new Win32.COM.ComMethodInfo((cpu, mem) => ComRelease(cpu, mem), ArgBytes: 4) }, // this only
				{ "CreateDevice", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_CreateDevice(cpu, mem, dinputHandle), ArgBytes: 16) }, // this + rguid + lplpDevice + pUnkOuter
				{ "EnumDevices", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_EnumDevices(cpu, mem), ArgBytes: 20) }, // this + dwDevType + lpCallback + pvRef + dwFlags
				{ "GetDeviceStatus", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_GetDeviceStatus(cpu, mem), ArgBytes: 8) }, // this + rguidInstance
				{ "RunControlPanel", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_RunControlPanel(cpu, mem), ArgBytes: 8) }, // this + hwndOwner
				{ "Initialize", new Win32.COM.ComMethodInfo((cpu, mem) => DInput_Initialize(cpu, mem), ArgBytes: 8) } // this + hinst
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

		[DllModuleExport(2)]
		private uint DirectInput8Create(uint hinst, uint dwVersion, uint riidltf, uint lplpDirectInput, uint pUnkOuter)
		{
			_logger.LogInformation("[DInput] DirectInput8Create(hinst=0x{Hinst:X8}, dwVersion=0x{DwVersion:X8}, riidltf=0x{Riidltf:X8})", hinst, dwVersion, riidltf);

			// DirectInput8 is similar to DirectInputCreate but with additional parameters
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
			public uint DataFormat { get; set; } // Pointer to DIDATAFORMAT structure
			public uint DataFormatSize { get; set; } // Size of data format in bytes
			public uint CooperativeHwnd { get; set; } // Window handle for cooperative level
			public uint CooperativeFlags { get; set; } // Cooperative level flags
			public bool IsAcquired { get; set; } // Whether device is acquired
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

			// Create a device COM object with its own vtable
			var deviceHandle = _nextDeviceHandle++;
			var deviceObj = new DirectInputDevice
			{
				Handle = deviceHandle,
				Name = "Emulated Device"
			};
			_devices[deviceHandle] = deviceObj;

			// Create COM vtable for IDirectInputDevice interface
			var deviceMethods = new Dictionary<string, Win32.COM.ComMethodInfo>
			{
				{ "QueryInterface", new Win32.COM.ComMethodInfo((cpu, mem) => ComQueryInterface(cpu, mem), ArgBytes: 12) }, // this + riid + ppvObject
				{ "AddRef", new Win32.COM.ComMethodInfo((cpu, mem) => ComAddRef(cpu, mem), ArgBytes: 4) }, // this only
				{ "Release", new Win32.COM.ComMethodInfo((cpu, mem) => ComRelease(cpu, mem), ArgBytes: 4) }, // this only
				{ "GetCapabilities", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_GetCapabilities(cpu, mem), ArgBytes: 8) }, // this + lpDIDevCaps
				{ "EnumObjects", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_EnumObjects(cpu, mem), ArgBytes: 16) }, // this + lpCallback + pvRef + dwFlags
				{ "GetProperty", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_GetProperty(cpu, mem), ArgBytes: 12) }, // this + rguidProp + pdiph
				{ "SetProperty", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_SetProperty(cpu, mem), ArgBytes: 12) }, // this + rguidProp + lpdiph
				{ "Acquire", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_Acquire(cpu, mem), ArgBytes: 4) }, // this only
				{ "Unacquire", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_Unacquire(cpu, mem), ArgBytes: 4) }, // this only
				{ "GetDeviceState", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_GetDeviceState(cpu, mem), ArgBytes: 12) }, // this + cbData + lpvData
				{ "GetDeviceData", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_GetDeviceData(cpu, mem), ArgBytes: 20) }, // this + cbObjectData + rgdod + pdwInOut + dwFlags
				{ "SetDataFormat", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_SetDataFormat(cpu, mem), ArgBytes: 8) }, // this + lpdf
				{ "SetEventNotification", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_SetEventNotification(cpu, mem), ArgBytes: 8) }, // this + hEvent
				{ "SetCooperativeLevel", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_SetCooperativeLevel(cpu, mem), ArgBytes: 12) }, // this + hwnd + dwFlags
				{ "GetObjectInfo", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_GetObjectInfo(cpu, mem), ArgBytes: 12) }, // this + pdidoi + dwObj + dwHow
				{ "GetDeviceInfo", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_GetDeviceInfo(cpu, mem), ArgBytes: 8) }, // this + pdidi
				{ "RunControlPanel", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_RunControlPanel(cpu, mem), ArgBytes: 12) }, // this + hwndOwner + dwFlags
				{ "Initialize", new Win32.COM.ComMethodInfo((cpu, mem) => DInputDevice_Initialize(cpu, mem), ArgBytes: 16) } // this + hinst + dwVersion + rguid
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

				var dwSize = _env.MemRead32(pdiph);
				var dwHeaderSize = _env.MemRead32(pdiph + 4);
				var dwObj = _env.MemRead32(pdiph + 8);
				var dwHow = _env.MemRead32(pdiph + 12);

				_logger.LogInformation("[DInput COM]   DIPROPHEADER: size={DwSize}, headerSize={DwHeaderSize}, obj={DwObj}, how={DwHow}",
					dwSize, dwHeaderSize, dwObj, dwHow);

				// For properties like DIPROP_BUFFERSIZE, there's additional data after the header
				// DIPROPDWORD contains: DIPROPHEADER diph; DWORD dwData;
				if (dwSize >= dwHeaderSize + 4)
				{
					var dwData = _env.MemRead32(pdiph + dwHeaderSize);
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

			// Find the device and mark it as acquired
			DirectInputDevice? device = null;
			foreach (var dev in _devices.Values)
			{
				device = dev;
				break; // For now, use the first device
			}

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

			_logger.LogInformation("[DInput COM] IDirectInputDevice::GetDeviceState(this=0x{ThisPtr:X8}, cbData={CbData}, lpvData=0x{LpvData:X8}) - stub", thisPtr, cbData, lpvData);

			// Zero out the device state buffer
			if (lpvData != 0 && cbData > 0)
			{
				_env.MemZero(lpvData, cbData);
			}

			return 0; // DI_OK
		}

		private uint DInputDevice_GetDeviceData(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::GetDeviceData() - stub");
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

				var dwSize = _env.MemRead32(lpdf);
				var dwObjSize = _env.MemRead32(lpdf + 4);
				var dwFlags = _env.MemRead32(lpdf + 8);
				var dwDataSize = _env.MemRead32(lpdf + 12);
				var dwNumObjs = _env.MemRead32(lpdf + 16);
				var rgodf = _env.MemRead32(lpdf + 20);

				_logger.LogInformation("[DInput COM]   DIDATAFORMAT: size={DwSize}, objSize={DwObjSize}, flags=0x{DwFlags:X}, dataSize={DwDataSize}, numObjs={DwNumObjs}, rgodf=0x{Rgodf:X8}",
					dwSize, dwObjSize, dwFlags, dwDataSize, dwNumObjs, rgodf);

				// Store the data format information
				device.DataFormat = lpdf;
				device.DataFormatSize = dwDataSize;
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
	}
}