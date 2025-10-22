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
			public IInputBackend.DeviceType DeviceType { get; set; }
			public bool IsAcquired { get; set; }
			public byte[]? DataFormat { get; set; }
			public int DataFormatSize { get; set; }
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
			_logger.LogInformation("[DInput COM] IDirectInputDevice::SetProperty() - stub");
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
			}

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

			_logger.LogInformation("[DInput COM] IDirectInputDevice::GetDeviceState(this=0x{ThisPtr:X8}, cbData={CbData}, lpvData=0x{LpvData:X8})", thisPtr, cbData, lpvData);

			// Find the device associated with this COM object
			var device = _devices.Values.FirstOrDefault(d => true); // TODO: Map thisPtr to device
			if (device == null || !device.IsAcquired)
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