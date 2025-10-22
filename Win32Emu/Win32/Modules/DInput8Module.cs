using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;

namespace Win32Emu.Win32.Modules
{
	public class DInput8Module : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public DInput8Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "DINPUT8.DLL";

		// DirectInput object handles
		private uint _nextDInputHandle = 0x90000000;
		private uint _nextDeviceHandle = 0x91000000;

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "DIRECTINPUT8CREATE":
					returnValue = DirectInput8Create(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				default:
					_logger.LogInformation("[DInput8] Unimplemented export: {Export}", export);
					return false;
			}
		}

		[DllModuleExport(1, entryPoint: 0x0000DDD9, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(1, entryPoint: 0x0000D926, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectInput8Create(uint hinst, uint dwVersion, uint riidltf, uint ppvOut, uint punkOuter)
		{
			_logger.LogWarning("[dinput8] DirectInput8Create: hinst={hinst}, dwVersion=0x{dwVersion:X8}, riidltf={riidltf}, ppvOut=0x{ppvOut:X8}, punkOuter={punkOuter}", hinst, dwVersion, riidltf, ppvOut, punkOuter);
			// TODO: Implement DirectInput8Create
			return 0; // DWORD default
		}

		[DllModuleExport(2, entryPoint: 0x0000DBC7, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(2, entryPoint: 0x0000D6A4, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllCanUnloadNow()
		{
			_logger.LogWarning("[dinput8] DllCanUnloadNow called (stub)");
			// TODO: Implement DllCanUnloadNow
			return 0; // DWORD default
		}

		[DllModuleExport(3, entryPoint: 0x0000DB5C, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(3, entryPoint: 0x0000D632, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllGetClassObject()
		{
			_logger.LogWarning("[dinput8] DllGetClassObject called (stub)");
			// TODO: Implement DllGetClassObject
			return 0; // DWORD default
		}

		[DllModuleExport(4, entryPoint: 0x000199D4, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(4, entryPoint: 0x0001A900, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllRegisterServer()
		{
			_logger.LogWarning("[dinput8] DllRegisterServer called (stub)");
			// TODO: Implement DllRegisterServer
			return 0; // DWORD default
		}

		[DllModuleExport(5, entryPoint: 0x000199DF, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(5, entryPoint: 0x0001A910, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllUnregisterServer()
		{
			_logger.LogWarning("[dinput8] DllUnregisterServer called (stub)");
			// TODO: Implement DllUnregisterServer
			return 0; // DWORD default
		}

	}
}