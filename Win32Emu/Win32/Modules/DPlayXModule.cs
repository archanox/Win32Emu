using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Modules
{
	public class DPlayXModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public DPlayXModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "DPLAYX.DLL";

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "DIRECTPLAYCREATE":
					returnValue = DirectPlayCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "DIRECTPLAYENUMERATEA":
					returnValue = DirectPlayEnumerateA(a.UInt32(0), a.UInt32(1));
					return true;
				case "DIRECTPLAYLOBBYCREATEA":
					returnValue = DirectPlayLobbyCreateA();
					return true;
				default:
					_logger.LogInformation("[DPlayX] Unimplemented export: {Export}", export);
					return false;
			}
		}

		[DllModuleExport(2, entryPoint: 0x00006C19, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(2, entryPoint: 0x00006C19, Version = "5.1.2600.6532", IsStub = true)]
		private uint DirectPlayEnumerateA(uint pCallback, uint pContext)
		{
			// TODO: Implement DirectPlayEnumerateA
			_logger.LogInformation("[DPlayX] DirectPlayEnumerateA({PCallbackName}=0x{PCallback:X8}, {PContextName}=0x{PContext:X8})", nameof(pCallback), pCallback, nameof(pContext), pContext);
			return 0;
		}

		[DllModuleExport(1, entryPoint: 0x000073B9, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(1, entryPoint: 0x000073B9, Version = "5.1.2600.6532", IsStub = true)]
		private uint DirectPlayCreate(uint lpGUID, uint lplpDP, uint pUnkOuter)
		{
			// TODO: Implement DirectPlayCreate
			_logger.LogInformation("[DPlayX] DirectPlayCreate({LpGuidName}=0x{LpGuid:X8}, {LplpDpName}=0x{LplpDp:X8}, {PUnkOuterName}=0x{PUnkOuter:X8})", nameof(lpGUID), lpGUID, nameof(lplpDP), lplpDP, nameof(pUnkOuter), pUnkOuter);
			return 0;
		}

		[DllModuleExport(3, entryPoint: 0x00006B7D, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(3, entryPoint: 0x00006B7D, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectPlayEnumerateW()
		{
			_logger.LogWarning("[dplayx] DirectPlayEnumerateW called (stub)");
			// TODO: Implement DirectPlayEnumerateW
			return 0; // DWORD default
		}

		[DllModuleExport(4, entryPoint: 0x00020E78, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(4, entryPoint: 0x00020E7E, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectPlayLobbyCreateA()
		{
			_logger.LogWarning("[dplayx] DirectPlayLobbyCreateA called (stub)");
			// TODO: Implement DirectPlayLobbyCreateA
			return 0; // DWORD default
		}

		[DllModuleExport(5, entryPoint: 0x00020E36, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(5, entryPoint: 0x00020E3C, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectPlayLobbyCreateW()
		{
			_logger.LogWarning("[dplayx] DirectPlayLobbyCreateW called (stub)");
			// TODO: Implement DirectPlayLobbyCreateW
			return 0; // DWORD default
		}

		[DllModuleExport(6, entryPoint: 0x0001BE76, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(6, entryPoint: 0x0001BE76, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllCanUnloadNow()
		{
			_logger.LogWarning("[dplayx] DllCanUnloadNow called (stub)");
			// TODO: Implement DllCanUnloadNow
			return 0; // DWORD default
		}

		[DllModuleExport(7, entryPoint: 0x0001BD1D, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(7, entryPoint: 0x0001BD1D, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllGetClassObject()
		{
			_logger.LogWarning("[dplayx] DllGetClassObject called (stub)");
			// TODO: Implement DllGetClassObject
			return 0; // DWORD default
		}

		[DllModuleExport(8, entryPoint: 0x0001344A, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(8, entryPoint: 0x0001344A, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllRegisterServer()
		{
			_logger.LogWarning("[dplayx] DllRegisterServer called (stub)");
			// TODO: Implement DllRegisterServer
			return 0; // DWORD default
		}

		[DllModuleExport(9, entryPoint: 0x00006D04, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(9, entryPoint: 0x00006D04, Version = "5.1.2600.6532", IsStub = true)]
		public uint DirectPlayEnumerate()
		{
			_logger.LogWarning("[dplayx] DirectPlayEnumerate called (stub)");
			// TODO: Implement DirectPlayEnumerate
			return 0; // DWORD default
		}

		[DllModuleExport(10, entryPoint: 0x00013461, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(10, entryPoint: 0x00013461, Version = "5.1.2600.6532", IsStub = true)]
		public uint DllUnregisterServer()
		{
			_logger.LogWarning("[dplayx] DllUnregisterServer called (stub)");
			// TODO: Implement DllUnregisterServer
			return 0; // DWORD default
		}

		[DllModuleExport(11, entryPoint: 0x00036800, Version = "4.90.0.3000", IsStub = true)]
		[DllModuleExport(11, entryPoint: 0x00036800, Version = "5.1.2600.6532", IsStub = true)]
		public uint gdwDPlaySPRefCount()
		{
			_logger.LogWarning("[dplayx] gdwDPlaySPRefCount called (stub)");
			// TODO: Implement gdwDPlaySPRefCount
			return 0; // DWORD default
		}
	}
}