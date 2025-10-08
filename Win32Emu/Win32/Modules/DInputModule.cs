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

		private unsafe uint DirectInputCreateA(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
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
			var vtableMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
			{
				{ "QueryInterface", (cpu, mem) => ComQueryInterface(cpu, mem) },
				{ "AddRef", (cpu, mem) => ComAddRef(cpu, mem) },
				{ "Release", (cpu, mem) => ComRelease(cpu, mem) },
				{ "CreateDevice", (cpu, mem) => DInput_CreateDevice(cpu, mem, dinputHandle) },
				{ "EnumDevices", (cpu, mem) => DInput_EnumDevices(cpu, mem) },
				{ "GetDeviceStatus", (cpu, mem) => DInput_GetDeviceStatus(cpu, mem) },
				{ "RunControlPanel", (cpu, mem) => DInput_RunControlPanel(cpu, mem) },
				{ "Initialize", (cpu, mem) => DInput_Initialize(cpu, mem) }
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
		private unsafe uint DirectInputCreate(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
		{
			_logger.LogInformation("[DInput] DirectInputCreate(hinst=0x{Hinst:X8}, dwVersion=0x{DwVersion:X8}, lplpDirectInput=0x{LplpDirectInput:X8}, pUnkOuter=0x{PUnkOuter:X8})", hinst, dwVersion, lplpDirectInput, pUnkOuter);

			// Create DirectInput object
			var diHandle = _nextDInputHandle++;
			var diObj = new DirectInputObject
			{
				Handle = diHandle,
				Version = dwVersion
			};
			_dinputObjects[diHandle] = diObj;

			// Initialize input backend if not already done
			if (_env.InputBackend == null)
			{
				_env.InputBackend = new Sdl3InputBackend(_logger);
				_env.InputBackend.Initialize();
			}

			if (lplpDirectInput != 0)
			{
				_env.MemWrite32(lplpDirectInput, diHandle);
			}

			return 0; // DI_OK
		}

		[DllModuleExport(2)]
		private unsafe uint DirectInput8Create(uint hinst, uint dwVersion, uint riidltf, uint lplpDirectInput, uint pUnkOuter)
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
			var deviceMethods = new Dictionary<string, Func<ICpu, VirtualMemory, uint>>
			{
				{ "QueryInterface", (cpu, mem) => ComQueryInterface(cpu, mem) },
				{ "AddRef", (cpu, mem) => ComAddRef(cpu, mem) },
				{ "Release", (cpu, mem) => ComRelease(cpu, mem) },
				{ "GetCapabilities", (cpu, mem) => DInputDevice_GetCapabilities(cpu, mem) },
				{ "EnumObjects", (cpu, mem) => DInputDevice_EnumObjects(cpu, mem) },
				{ "GetProperty", (cpu, mem) => DInputDevice_GetProperty(cpu, mem) },
				{ "SetProperty", (cpu, mem) => DInputDevice_SetProperty(cpu, mem) },
				{ "Acquire", (cpu, mem) => DInputDevice_Acquire(cpu, mem) },
				{ "Unacquire", (cpu, mem) => DInputDevice_Unacquire(cpu, mem) },
				{ "GetDeviceState", (cpu, mem) => DInputDevice_GetDeviceState(cpu, mem) },
				{ "GetDeviceData", (cpu, mem) => DInputDevice_GetDeviceData(cpu, mem) },
				{ "SetDataFormat", (cpu, mem) => DInputDevice_SetDataFormat(cpu, mem) },
				{ "SetEventNotification", (cpu, mem) => DInputDevice_SetEventNotification(cpu, mem) },
				{ "SetCooperativeLevel", (cpu, mem) => DInputDevice_SetCooperativeLevel(cpu, mem) },
				{ "GetObjectInfo", (cpu, mem) => DInputDevice_GetObjectInfo(cpu, mem) },
				{ "GetDeviceInfo", (cpu, mem) => DInputDevice_GetDeviceInfo(cpu, mem) },
				{ "RunControlPanel", (cpu, mem) => DInputDevice_RunControlPanel(cpu, mem) },
				{ "Initialize", (cpu, mem) => DInputDevice_Initialize(cpu, mem) }
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
			_logger.LogInformation("[DInput COM] IDirectInputDevice::SetProperty() - stub");
			return 0; // DI_OK
		}

		private uint DInputDevice_Acquire(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::Acquire() - stub");
			return 0; // DI_OK
		}

		private uint DInputDevice_Unacquire(ICpu cpu, VirtualMemory memory)
		{
			_logger.LogInformation("[DInput COM] IDirectInputDevice::Unacquire() - stub");
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

			_logger.LogInformation("[DInput COM] IDirectInputDevice::SetDataFormat(this=0x{ThisPtr:X8}, lpdf=0x{Lpdf:X8}) - stub", thisPtr, lpdf);
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

			_logger.LogInformation("[DInput COM] IDirectInputDevice::SetCooperativeLevel(this=0x{ThisPtr:X8}, hwnd=0x{Hwnd:X8}, flags=0x{DwFlags:X8}) - stub", thisPtr, hwnd, dwFlags);
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