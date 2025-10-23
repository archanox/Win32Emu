using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// OLEAUT32.DLL module - provides OLE Automation functionality.
/// </summary>
public class Oleaut32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Oleaut32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "OLEAUT32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "ORDINAL_7":
				returnValue = Ordinal_7(a.UInt32(0), a.UInt32(1));
				return true;

			case "ORDINAL_8":
				returnValue = Ordinal_8(a.UInt32(0), a.UInt32(1));
				return true;

			case "ORDINAL_9":
				returnValue = Ordinal_9(a.UInt32(0), a.UInt32(1));
				return true;

			default:
				_logger.LogInformation("[Oleaut32] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Ordinal 7 - SysAllocString or similar function.
	/// BSTR SysAllocString(const OLECHAR *psz);
	/// </summary>
	[DllModuleExport(4)]
	private uint Ordinal_7(uint psz, uint param2)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_7(psz=0x{Psz:X8}, param2=0x{Param2:X8})", psz, param2);

		if (psz == 0)
		{
			return 0; // NULL
		}

		// For SysAllocString, we would allocate a BSTR
		// For now, return a stub handle
		return psz; // Return the input pointer as-is (stub)
	}

	/// <summary>
	/// Ordinal 8 - SysFreeString or similar function.
	/// void SysFreeString(BSTR bstrString);
	/// </summary>
	[DllModuleExport(4)]
	private uint Ordinal_8(uint bstrString, uint param2)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_8(bstrString=0x{BstrString:X8}, param2=0x{Param2:X8})", bstrString, param2);

		// For SysFreeString, we would free the BSTR
		// For now, just acknowledge the call
		return 0; // void function
	}

	/// <summary>
	/// Ordinal 9 - SysReAllocString or similar function.
	/// INT SysReAllocString(BSTR *pbstr, const OLECHAR *psz);
	/// </summary>
	[DllModuleExport(4)]
	private uint Ordinal_9(uint pbstr, uint psz)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_9(pbstr=0x{Pbstr:X8}, psz=0x{Psz:X8})", pbstr, psz);

		// For SysReAllocString, we would reallocate a BSTR
		// For now, return success
		return 1; // TRUE
	}
}
