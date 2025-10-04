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

		public bool TryInvokeUnsafe(string exp, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (exp.ToUpperInvariant())
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
					_logger.LogInformation($"[DInput] Unimplemented export: {exp}");
					return false;
			}
		}

		private uint DirectInputCreateA(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
		{
			_logger.LogInformation($"[DInput] DirectInputCreateA(hinst=0x{hinst:X8}, dwVersion=0x{dwVersion:X8}, lplpDirectInput=0x{lplpDirectInput:X8}, pUnkOuter=0x{pUnkOuter:X8})");

			// Create DirectInput object
			var dinputHandle = _nextDInputHandle++;
			var dinputObj = new DirectInputObject
			{
				Handle = dinputHandle
			};

			_dinputObjects[dinputHandle] = dinputObj;

			// Write handle back to caller
			if (lplpDirectInput != 0)
			{
				_env.MemWrite32(lplpDirectInput, dinputHandle);
			}

			_logger.LogInformation($"[DInput] Created DirectInput object: 0x{dinputHandle:X8}");
			return 0; // DI_OK
		}

		private uint DirectInputCreate(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
		{
			_logger.LogInformation($"[DInput] DirectInputCreate(hinst=0x{hinst:X8}, dwVersion=0x{dwVersion:X8}, lplpDirectInput=0x{lplpDirectInput:X8}, pUnkOuter=0x{pUnkOuter:X8})");

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
				_env.InputBackend = new Sdl3InputBackend();
				_env.InputBackend.Initialize();
			}

			if (lplpDirectInput != 0)
			{
				_env.MemWrite32(lplpDirectInput, diHandle);
			}

			return 0; // DI_OK
		}

		private uint DirectInput8Create(uint hinst, uint dwVersion, uint riidltf, uint lplpDirectInput, uint pUnkOuter)
		{
			_logger.LogInformation($"[DInput] DirectInput8Create(hinst=0x{hinst:X8}, dwVersion=0x{dwVersion:X8}, riidltf=0x{riidltf:X8})");

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
	}
}