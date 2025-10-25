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
			case "ORDINAL_2":
				returnValue = Ordinal_2(a.UInt32(0));
				return true;

			case "ORDINAL_4":
				returnValue = Ordinal_4(a.UInt32(0));
				return true;

			case "ORDINAL_6":
				returnValue = Ordinal_6(a.UInt32(0), a.UInt32(1));
				return true;

			case "ORDINAL_7":
				returnValue = Ordinal_7(a.UInt32(0), a.UInt32(1));
				return true;

			case "ORDINAL_8":
				returnValue = Ordinal_8(a.UInt32(0), a.UInt32(1));
				return true;

			case "ORDINAL_9":
				returnValue = Ordinal_9(a.UInt32(0), a.UInt32(1));
				return true;

			case "ORDINAL_10":
				returnValue = Ordinal_10(a.UInt32(0), a.UInt32(1));
				return true;

			case "ORDINAL_12":
				returnValue = Ordinal_12(a.UInt32(0));
				return true;

			case "ORDINAL_149":
				returnValue = Ordinal_149(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "ORDINAL_150":
				returnValue = Ordinal_150(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "ORDINAL_161":
				returnValue = Ordinal_161(a.UInt32(0));
				return true;

			case "ORDINAL_185":
				returnValue = Ordinal_185(a.UInt32(0), a.UInt32(1), a.UInt32(2));
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

	/// <summary>
	/// Ordinal 2 - SysAllocStringLen or similar function.
	/// BSTR SysAllocStringLen(const OLECHAR *psz, UINT len);
	/// </summary>
	[DllModuleExport(4)]
	private uint Ordinal_2(uint psz)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_2(psz=0x{Psz:X8})", psz);
		// Stub: Return the input pointer as-is
		return psz;
	}

	/// <summary>
	/// Ordinal 4 - SysAllocStringByteLen or similar function.
	/// BSTR SysAllocStringByteLen(LPCSTR psz, UINT len);
	/// </summary>
	[DllModuleExport(4)]
	private uint Ordinal_4(uint psz)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_4(psz=0x{Psz:X8})", psz);
		// Stub: Return the input pointer as-is
		return psz;
	}

	/// <summary>
	/// Ordinal 6 - SysReAllocStringLen or similar function.
	/// INT SysReAllocStringLen(BSTR *pbstr, const OLECHAR *psz, UINT len);
	/// </summary>
	[DllModuleExport(8)]
	private uint Ordinal_6(uint pbstr, uint psz)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_6(pbstr=0x{Pbstr:X8}, psz=0x{Psz:X8})", pbstr, psz);
		return 1; // TRUE
	}

	/// <summary>
	/// Ordinal 10 - VariantInit or similar function.
	/// void VariantInit(VARIANTARG *pvarg);
	/// </summary>
	[DllModuleExport(8)]
	private uint Ordinal_10(uint pvarg, uint param2)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_10(pvarg=0x{Pvarg:X8}, param2=0x{Param2:X8})", pvarg, param2);

		// Initialize VARIANT to VT_EMPTY
		if (pvarg != 0)
		{
			// VARIANT is 16 bytes, initialize to zeros
			for (int i = 0; i < 16; i++)
			{
				_env.MemWrite8(pvarg + (uint)i, 0);
			}
		}

		return 0; // void function
	}

	/// <summary>
	/// Ordinal 12 - VariantClear or similar function.
	/// HRESULT VariantClear(VARIANTARG *pvarg);
	/// </summary>
	[DllModuleExport(4)]
	private uint Ordinal_12(uint pvarg)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_12(pvarg=0x{Pvarg:X8})", pvarg);

		// Clear VARIANT
		if (pvarg != 0)
		{
			for (int i = 0; i < 16; i++)
			{
				_env.MemWrite8(pvarg + (uint)i, 0);
			}
		}

		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Ordinal 149 - VariantChangeType or similar function.
	/// HRESULT VariantChangeType(VARIANTARG *pvargDest, const VARIANTARG *pvarSrc, USHORT wFlags, VARTYPE vt);
	/// </summary>
	[DllModuleExport(12)]
	private uint Ordinal_149(uint pvargDest, uint pvarSrc, uint wFlags)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_149(pvargDest=0x{PvargDest:X8}, pvarSrc=0x{PvarSrc:X8}, wFlags=0x{WFlags:X})",
			pvargDest, pvarSrc, wFlags);

		// Stub: Return E_NOTIMPL
		return 0x80004001; // E_NOTIMPL
	}

	/// <summary>
	/// Ordinal 150 - VariantCopy or similar function.
	/// HRESULT VariantCopy(VARIANTARG *pvargDest, const VARIANTARG *pvargSrc);
	/// </summary>
	[DllModuleExport(16)]
	private uint Ordinal_150(uint pvargDest, uint pvargSrc, uint param3, uint param4)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_150(pvargDest=0x{PvargDest:X8}, pvargSrc=0x{PvargSrc:X8})",
			pvargDest, pvargSrc);

		// Copy VARIANT (16 bytes)
		if (pvargDest != 0 && pvargSrc != 0)
		{
			for (int i = 0; i < 16; i++)
			{
				var b = _env.MemRead8(pvargSrc + (uint)i);
				_env.MemWrite8(pvargDest + (uint)i, b);
			}
		}

		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Ordinal 161 - SafeArrayDestroy or similar function.
	/// HRESULT SafeArrayDestroy(SAFEARRAY *psa);
	/// </summary>
	[DllModuleExport(4)]
	private uint Ordinal_161(uint psa)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_161(psa=0x{Psa:X8})", psa);
		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Ordinal 185 - SafeArrayCreate or similar function.
	/// SAFEARRAY *SafeArrayCreate(VARTYPE vt, UINT cDims, SAFEARRAYBOUND *rgsabound);
	/// </summary>
	[DllModuleExport(12)]
	private uint Ordinal_185(uint vt, uint cDims, uint rgsabound)
	{
		_logger.LogInformation("[Oleaut32] Ordinal_185(vt=0x{Vt:X}, cDims={CDims}, rgsabound=0x{Rgsabound:X8})",
			vt, cDims, rgsabound);

		// Stub: Return NULL (failed to create)
		return 0;
	}
}
