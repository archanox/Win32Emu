using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// ADVAPI32.DLL module - provides advanced Windows API functions including registry access.
/// </summary>
public class Advapi32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Advapi32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "ADVAPI32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "REGOPENKEYEXA":
				returnValue = RegOpenKeyExA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;

			case "REGQUERYVALUEEXA":
				returnValue = RegQueryValueExA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
				return true;

			case "REGCLOSEKEY":
				returnValue = RegCloseKey(a.UInt32(0));
				return true;

			default:
				_logger.LogInformation("[Advapi32] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Opens the specified registry key.
	/// LSTATUS RegOpenKeyExA(
	///   [in]           HKEY   hKey,
	///   [in, optional] LPCSTR lpSubKey,
	///   [in]           DWORD  ulOptions,
	///   [in]           REGSAM samDesired,
	///   [out]          PHKEY  phkResult
	/// );
	/// </summary>
	[DllModuleExport(20)]
	private uint RegOpenKeyExA(uint hKey, in LpcStr lpSubKey, uint ulOptions, uint samDesired, uint phkResult)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		
		// Predefined registry key values
		const uint HKEY_CLASSES_ROOT = 0x80000000;
		const uint HKEY_CURRENT_USER = 0x80000001;
		const uint HKEY_LOCAL_MACHINE = 0x80000002;
		const uint HKEY_USERS = 0x80000003;

		// Map predefined keys to readable names
		var hKeyName = hKey switch
		{
			HKEY_CLASSES_ROOT => "HKEY_CLASSES_ROOT",
			HKEY_CURRENT_USER => "HKEY_CURRENT_USER",
			HKEY_LOCAL_MACHINE => "HKEY_LOCAL_MACHINE",
			HKEY_USERS => "HKEY_USERS",
			_ => $"0x{hKey:X8}"
		};

		var fullPath = string.IsNullOrEmpty(subKey) ? hKeyName : $"{hKeyName}\\{subKey}";
		
		_logger.LogInformation("[Advapi32] RegOpenKeyExA(hKey={HKeyName}, lpSubKey=\"{SubKey}\", options=0x{UlOptions:X}, access=0x{SamDesired:X}, phkResult=0x{PhkResult:X8})",
			hKeyName, subKey, ulOptions, samDesired, phkResult);

		// Open the virtual registry key
		var handle = _env.RegOpenKey(fullPath);

		// Write the handle to the output parameter
		if (phkResult != 0)
		{
			_env.MemWrite32(phkResult, handle);
		}

		// ERROR_SUCCESS
		return 0;
	}

	/// <summary>
	/// Retrieves the type and data for the specified value name associated with an open registry key.
	/// LSTATUS RegQueryValueExA(
	///   [in]                HKEY    hKey,
	///   [in, optional]      LPCSTR  lpValueName,
	///   [in, optional]      LPDWORD lpReserved,
	///   [out, optional]     LPDWORD lpType,
	///   [out, optional]     LPBYTE  lpData,
	///   [in, out, optional] LPDWORD lpcbData
	/// );
	/// </summary>
	[DllModuleExport(32)]
	private uint RegQueryValueExA(uint hKey, in LpcStr lpValueName, uint lpReserved, uint lpType, uint lpData, uint lpcbData)
	{
		var valueName = lpValueName.ToString() ?? string.Empty;
		
		_logger.LogInformation("[Advapi32] RegQueryValueExA(hKey=0x{HKey:X8}, lpValueName=\"{ValueName}\", lpType=0x{LpType:X8}, lpData=0x{LpData:X8}, lpcbData=0x{LpcbData:X8})",
			hKey, valueName, lpType, lpData, lpcbData);

		// Try to query the value from virtual registry
		if (!_env.RegQueryValue(hKey, valueName, out var value))
		{
			// Value not found - return ERROR_FILE_NOT_FOUND (commonly used for registry)
			_logger.LogInformation("[Advapi32] RegQueryValueExA: Value not found");
			return 2; // ERROR_FILE_NOT_FOUND
		}

		// For simplicity, return empty data for now
		// A full implementation would serialize the value and write it to lpData
		const uint REG_SZ = 1; // String type

		if (lpType != 0)
		{
			_env.MemWrite32(lpType, REG_SZ);
		}

		if (lpcbData != 0)
		{
			_env.MemWrite32(lpcbData, 0); // Empty data
		}

		_logger.LogInformation("[Advapi32] RegQueryValueExA: Returning empty data (not implemented)");
		
		// ERROR_SUCCESS
		return 0;
	}

	/// <summary>
	/// Closes a handle to the specified registry key.
	/// LSTATUS RegCloseKey(
	///   [in] HKEY hKey
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint RegCloseKey(uint hKey)
	{
		_logger.LogInformation("[Advapi32] RegCloseKey(hKey=0x{HKey:X8})", hKey);

		// Close the virtual registry key
		_env.RegCloseKey(hKey);

		// ERROR_SUCCESS
		return 0;
	}
}
