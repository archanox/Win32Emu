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

	// Registry data type constants
	private const uint REG_SZ = 1;          // String
	private const uint REG_EXPAND_SZ = 2;   // Expandable string  
	private const uint REG_BINARY = 3;      // Binary data
	private const uint REG_DWORD = 4;       // 32-bit number

	private uint _nextServiceHandle = 0xB0000000;
	private uint _nextSidHandle = 0xB1000000;

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			// Registry functions
			case "REGOPENKEYEXA":
				returnValue = RegOpenKeyExA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "REGOPENKEYA":
				returnValue = RegOpenKeyA(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
				return true;
			case "REGCREATEKEYEXA":
				returnValue = RegCreateKeyExA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.LpcStr(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7), a.UInt32(8));
				return true;
			case "REGCREATEKEYA":
				returnValue = RegCreateKeyA(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
				return true;
			case "REGSETVALUEEXA":
				returnValue = RegSetValueExA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
				return true;
			case "REGSETVALUEA":
				returnValue = RegSetValueA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "REGQUERYVALUEEXA":
				returnValue = RegQueryValueExA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
				return true;
			case "REGQUERYVALUEA":
				returnValue = RegQueryValueA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "REGQUERYINFOKEYA":
				returnValue = RegQueryInfoKeyA(a.UInt32(0), a.LpStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7), a.UInt32(8), a.UInt32(9), a.UInt32(10), a.UInt32(11));
				return true;
			case "REGENUMKEYA":
				returnValue = RegEnumKeyA(a.UInt32(0), a.UInt32(1), a.LpStr(2), a.UInt32(3));
				return true;
			case "REGENUMKEYEXA":
				returnValue = RegEnumKeyExA(a.UInt32(0), a.UInt32(1), a.LpStr(2), a.UInt32(3), a.UInt32(4), a.LpStr(5), a.UInt32(6), a.UInt32(7));
				return true;
			case "REGENUMVALUEA":
				returnValue = RegEnumValueA(a.UInt32(0), a.UInt32(1), a.LpStr(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7));
				return true;
			case "REGDELETEKEYA":
				returnValue = RegDeleteKeyA(a.UInt32(0), a.LpcStr(1));
				return true;
			case "REGDELETEVALUEA":
				returnValue = RegDeleteValueA(a.UInt32(0), a.LpcStr(1));
				return true;
			case "REGCONNECTREGISTRYA":
				returnValue = RegConnectRegistryA(a.LpcStr(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "REGCLOSEKEY":
				returnValue = RegCloseKey(a.UInt32(0));
				return true;
			case "REGFLUSHKEY":
				returnValue = RegFlushKey(a.UInt32(0));
				return true;
			case "REGLOADKEYA":
				returnValue = RegLoadKeyA(a.UInt32(0), a.LpcStr(1), a.LpcStr(2));
				return true;
			case "REGUNLOADKEYA":
				returnValue = RegUnLoadKeyA(a.UInt32(0), a.LpcStr(1));
				return true;

			// Security functions
			case "ACCESSCHECK":
				returnValue = AccessCheck(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7));
				return true;
			case "ADDACCESSALLOWEDACE":
				returnValue = AddAccessAllowedAce(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "ADJUSTTOKENPRIVILEGES":
				returnValue = AdjustTokenPrivileges(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
				return true;
			case "ALLOCATEANDINITIALIZESID":
				returnValue = AllocateAndInitializeSid(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7), a.UInt32(8), a.UInt32(9), a.UInt32(10));
				return true;
			case "FREESID":
				returnValue = FreeSid(a.UInt32(0));
				return true;
			case "GETLENGTHSID":
				returnValue = GetLengthSid(a.UInt32(0));
				return true;
			case "IMPERSONATESELF":
				returnValue = ImpersonateSelf(a.UInt32(0));
				return true;
			case "INITIALIZEACL":
				returnValue = InitializeAcl(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "INITIALIZESECURITYDESCRIPTOR":
				returnValue = InitializeSecurityDescriptor(a.UInt32(0), a.UInt32(1));
				return true;
			case "ISVALIDSECURITYDESCRIPTOR":
				returnValue = IsValidSecurityDescriptor(a.UInt32(0));
				return true;
			case "LOOKUPPRIVILEGEVALUEA":
				returnValue = LookupPrivilegeValueA(a.LpcStr(0), a.LpcStr(1), a.UInt32(2));
				return true;
			case "OPENPROCESSTOKEN":
				returnValue = OpenProcessToken(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "OPENTHREADTOKEN":
				returnValue = OpenThreadToken(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "REVERTTOSELF":
				returnValue = RevertToSelf();
				return true;
			case "SETSECURITYDESCRIPTORDACL":
				returnValue = SetSecurityDescriptorDacl(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "SETSECURITYDESCRIPTORGROUP":
				returnValue = SetSecurityDescriptorGroup(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "SETSECURITYDESCRIPTOROWNER":
				returnValue = SetSecurityDescriptorOwner(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "GETFILESECURITYA":
				returnValue = GetFileSecurityA(a.LpcStr(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "SETFILESECURITYA":
				returnValue = SetFileSecurityA(a.LpcStr(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "GETUSERNAMEA":
				returnValue = GetUserNameA(a.UInt32(0), a.UInt32(1));
				return true;

			// Service functions
			case "OPENSCMANAGERA":
				returnValue = OpenSCManagerA(a.LpcStr(0), a.LpcStr(1), a.UInt32(2));
				return true;
			case "OPENSERVICEA":
				returnValue = OpenServiceA(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
				return true;
			case "CLOSESERVICEHANDLE":
				returnValue = CloseServiceHandle(a.UInt32(0));
				return true;
			case "CREATESERVICEW":
				returnValue = CreateServiceW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7), a.UInt32(8), a.UInt32(9), a.UInt32(10), a.UInt32(11), a.UInt32(12));
				return true;
			case "DELETESERVICE":
				returnValue = DeleteService(a.UInt32(0));
				return true;
			case "STARTSERVICEA":
				returnValue = StartServiceA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "CONTROLSERVICE":
				returnValue = ControlService(a.UInt32(0), a.UInt32(1), a.UInt32(2));
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
	[DllModuleExport(240, Version = "4.90.0.3000")]
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
	[DllModuleExport(248, Version = "4.90.0.3000")]
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

		// Determine the data type and size
		uint dataType;
		byte[] dataBytes;

		if (value is string str)
		{
			dataType = REG_SZ;
			dataBytes = System.Text.Encoding.ASCII.GetBytes(str + '\0'); // Null-terminated
		}
		else if (value is uint dwordVal)
		{
			dataType = REG_DWORD;
			dataBytes = BitConverter.GetBytes(dwordVal);
		}
		else if (value is int intVal)
		{
			dataType = REG_DWORD;
			dataBytes = BitConverter.GetBytes((uint)intVal);
		}
		else if (value is byte[] binaryVal)
		{
			dataType = REG_BINARY;
			dataBytes = binaryVal;
		}
		else
		{
			// Unknown type - convert to string
			dataType = REG_SZ;
			var fallbackStr = value?.ToString() ?? string.Empty;
			dataBytes = System.Text.Encoding.ASCII.GetBytes(fallbackStr + '\0');
		}

		// Write the data type if requested
		if (lpType != 0)
		{
			_env.MemWrite32(lpType, dataType);
		}

		// Check buffer size
		uint requiredSize = (uint)dataBytes.Length;
		uint providedSize = 0;

		if (lpcbData != 0)
		{
			providedSize = _env.MemRead32(lpcbData);
			_env.MemWrite32(lpcbData, requiredSize);
		}

		// If no buffer or buffer too small, return ERROR_MORE_DATA
		if (lpData == 0 || (lpcbData != 0 && providedSize < requiredSize))
		{
			_logger.LogInformation("[Advapi32] RegQueryValueExA: Buffer too small or null (required={RequiredSize}, provided={ProvidedSize})",
				requiredSize, providedSize);
			return 234; // ERROR_MORE_DATA
		}

		// Write the data to the buffer
		for (uint i = 0; i < requiredSize && i < providedSize; i++)
		{
			_env.MemWrite8(lpData + i, dataBytes[i]);
		}

		_logger.LogInformation("[Advapi32] RegQueryValueExA: Returned {Size} bytes, type={Type}", requiredSize, dataType);

		// ERROR_SUCCESS
		return 0;
	}

	/// <summary>
	/// Closes a handle to the specified registry key.
	/// LSTATUS RegCloseKey(
	///   [in] HKEY hKey
	/// );
	/// </summary>
	[DllModuleExport(217, Version = "4.90.0.3000")]
	private uint RegCloseKey(uint hKey)
	{
		_logger.LogInformation("[Advapi32] RegCloseKey(hKey=0x{HKey:X8})", hKey);

		// Close the virtual registry key
		_env.RegCloseKey(hKey);

		// ERROR_SUCCESS
		return 0;
	}

	/// <summary>
	/// Writes all the attributes of the specified open registry key into the registry.
	/// LSTATUS RegFlushKey(
	///   [in] HKEY hKey
	/// );
	/// </summary>
	[DllModuleExport(234, Version = "4.90.0.3000")]
	private uint RegFlushKey(uint hKey)
	{
		_logger.LogInformation("[Advapi32] RegFlushKey(hKey=0x{HKey:X8})", hKey);

		// In a real system, this would flush the key to disk
		// For emulation purposes, we just acknowledge the call

		// ERROR_SUCCESS
		return 0;
	}

	/// <summary>
	/// Creates the specified registry key. If the key already exists, the function opens it.
	/// LSTATUS RegCreateKeyExA(
	///   [in]            HKEY                        hKey,
	///   [in]            LPCSTR                      lpSubKey,
	///   [in]            DWORD                       Reserved,
	///   [in, optional]  LPSTR                       lpClass,
	///   [in]            DWORD                       dwOptions,
	///   [in]            REGSAM                      samDesired,
	///   [in, optional]  const LPSECURITY_ATTRIBUTES lpSecurityAttributes,
	///   [out]           PHKEY                       phkResult,
	///   [out, optional] LPDWORD                     lpdwDisposition
	/// );
	/// </summary>
	[DllModuleExport(221, Version = "4.90.0.3000")]
	private uint RegCreateKeyExA(uint hKey, in LpcStr lpSubKey, uint reserved, in LpcStr lpClass, uint dwOptions, uint samDesired, uint lpSecurityAttributes, uint phkResult, uint lpdwDisposition)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		var className = lpClass.ToString() ?? string.Empty;

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

		_logger.LogInformation("[Advapi32] RegCreateKeyExA(hKey={HKeyName}, lpSubKey=\"{SubKey}\", class=\"{ClassName}\", options=0x{DwOptions:X}, access=0x{SamDesired:X}, phkResult=0x{PhkResult:X8})",
			hKeyName, subKey, className, dwOptions, samDesired, phkResult);

		// Create or open the virtual registry key
		var handle = _env.RegOpenKey(fullPath);
		var wasCreated = true; // For simplicity, always report as created

		// Write the handle to the output parameter
		if (phkResult != 0)
		{
			_env.MemWrite32(phkResult, handle);
		}

		// Write disposition (created or opened)
		if (lpdwDisposition != 0)
		{
			const uint REG_CREATED_NEW_KEY = 0x00000001;
			const uint REG_OPENED_EXISTING_KEY = 0x00000002;
			_env.MemWrite32(lpdwDisposition, wasCreated ? REG_CREATED_NEW_KEY : REG_OPENED_EXISTING_KEY);
		}

		// ERROR_SUCCESS
		return 0;
	}

	/// <summary>
	/// Sets the data and type of a specified value under a registry key.
	/// LSTATUS RegSetValueExA(
	///   [in]           HKEY       hKey,
	///   [in, optional] LPCSTR     lpValueName,
	///   [in]           DWORD      Reserved,
	///   [in]           DWORD      dwType,
	///   [in]           const BYTE *lpData,
	///   [in]           DWORD      cbData
	/// );
	/// </summary>
	[DllModuleExport(260, Version = "4.90.0.3000")]
	private uint RegSetValueExA(uint hKey, in LpcStr lpValueName, uint reserved, uint dwType, uint lpData, uint cbData)
	{
		var valueName = lpValueName.ToString() ?? string.Empty;

		_logger.LogInformation("[Advapi32] RegSetValueExA(hKey=0x{HKey:X8}, lpValueName=\"{ValueName}\", type=0x{DwType:X}, lpData=0x{LpData:X8}, cbData={CbData})",
			hKey, valueName, dwType, lpData, cbData);

		if (lpData == 0 || cbData == 0)
		{
			_logger.LogWarning("[Advapi32] RegSetValueExA: Invalid data pointer or size");
			return 0; // ERROR_SUCCESS (be lenient for now)
		}

		try
		{
			// Read the data from memory
			var data = new byte[cbData];
			for (uint i = 0; i < cbData; i++)
			{
				data[i] = _env.MemRead8(lpData + i);
			}

			// Convert data based on type
			object value;
			DiscUtils.Registry.RegistryValueType regType;

			switch (dwType)
			{
				case REG_SZ:
					// Null-terminated string
					var strLen = Array.IndexOf(data, (byte)0);
					if (strLen < 0) strLen = data.Length;
					value = System.Text.Encoding.ASCII.GetString(data, 0, strLen);
					regType = DiscUtils.Registry.RegistryValueType.String;
					break;

				case REG_EXPAND_SZ:
					// Expandable string
					strLen = Array.IndexOf(data, (byte)0);
					if (strLen < 0) strLen = data.Length;
					value = System.Text.Encoding.ASCII.GetString(data, 0, strLen);
					regType = DiscUtils.Registry.RegistryValueType.ExpandString;
					break;

				case REG_DWORD:
					if (cbData >= 4)
					{
						value = BitConverter.ToUInt32(data, 0);
						regType = DiscUtils.Registry.RegistryValueType.Dword;
					}
					else
					{
						value = data;
						regType = DiscUtils.Registry.RegistryValueType.Binary;
					}
					break;

				case REG_BINARY:
				default:
					value = data;
					regType = DiscUtils.Registry.RegistryValueType.Binary;
					break;
			}

			// Store in virtual registry
			if (_env.RegSetValue(hKey, valueName, value, regType))
			{
				_logger.LogInformation("[Advapi32] RegSetValueExA: Set value \"{ValueName}\"={Value} (type={Type})", valueName, value, regType);
				return 0; // ERROR_SUCCESS
			}
			else
			{
				_logger.LogError("[Advapi32] RegSetValueExA: Failed to set value \"{ValueName}\"", valueName);
				return 5; // ERROR_ACCESS_DENIED
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegSetValueExA: Failed to set value");
			return 5; // ERROR_ACCESS_DENIED
		}
	}

	/// <summary>
	/// Retrieves data associated with the default or unnamed value of a registry key.
	/// LSTATUS RegQueryValueA(
	///   HKEY   hKey,
	///   LPCSTR lpSubKey,
	///   LPSTR  lpData,
	///   PLONG  lpcbData
	/// );
	/// </summary>
	[DllModuleExport(247, Version = "4.90.0.3000")]
	private uint RegQueryValueA(uint hKey, in LpcStr lpSubKey, uint lpData, uint lpcbData)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegQueryValueA(hKey=0x{HKey:X8}, lpSubKey=\"{SubKey}\", lpData=0x{LpData:X8}, lpcbData=0x{LpcbData:X8})",
			hKey, subKey, lpData, lpcbData);

		try
		{
			// RegQueryValueA queries the default (unnamed) value of a subkey
			// If lpSubKey is not null/empty, we need to open the subkey first
			uint targetHandle = hKey;
			bool closeHandle = false;

			if (!string.IsNullOrEmpty(subKey))
			{
				// Get the parent key path
				var keyPath = _env.RegistryHive?.GetKeyPath(hKey);
				if (string.IsNullOrEmpty(keyPath))
				{
					// hKey might be a predefined key
					const uint HKEY_CLASSES_ROOT = 0x80000000;
					const uint HKEY_CURRENT_USER = 0x80000001;
					const uint HKEY_LOCAL_MACHINE = 0x80000002;
					const uint HKEY_USERS = 0x80000003;

					keyPath = hKey switch
					{
						HKEY_CLASSES_ROOT => "HKEY_CLASSES_ROOT",
						HKEY_CURRENT_USER => "HKEY_CURRENT_USER",
						HKEY_LOCAL_MACHINE => "HKEY_LOCAL_MACHINE",
						HKEY_USERS => "HKEY_USERS",
						_ => null
					};
				}

				if (!string.IsNullOrEmpty(keyPath))
				{
					var fullPath = $"{keyPath}\\{subKey}";
					targetHandle = _env.RegOpenKey(fullPath);
					if (targetHandle == 0)
					{
						_logger.LogInformation("[Advapi32] RegQueryValueA: Subkey not found");
						return 2; // ERROR_FILE_NOT_FOUND
					}
					closeHandle = true;
				}
				else
				{
					_logger.LogWarning("[Advapi32] RegQueryValueA: Invalid key handle");
					return 2; // ERROR_FILE_NOT_FOUND
				}
			}

			// Query the default (unnamed) value
			if (!_env.RegQueryValue(targetHandle, "", out var value))
			{
				if (closeHandle) _env.RegCloseKey(targetHandle);
				_logger.LogInformation("[Advapi32] RegQueryValueA: Value not found");
				return 2; // ERROR_FILE_NOT_FOUND
			}

			// Convert value to string (RegQueryValueA only returns REG_SZ)
			var valueStr = value?.ToString() ?? string.Empty;
			var valueBytes = System.Text.Encoding.ASCII.GetBytes(valueStr + '\0'); // Null-terminated

			// Check buffer size
			uint requiredSize = (uint)valueBytes.Length;
			uint providedSize = 0;

			if (lpcbData != 0)
			{
				providedSize = _env.MemRead32(lpcbData);
				_env.MemWrite32(lpcbData, requiredSize);
			}

			// If no buffer or buffer too small, return ERROR_MORE_DATA
			if (lpData == 0 || (lpcbData != 0 && providedSize < requiredSize))
			{
				if (closeHandle) _env.RegCloseKey(targetHandle);
				_logger.LogInformation("[Advapi32] RegQueryValueA: Buffer too small (required={RequiredSize}, provided={ProvidedSize})",
					requiredSize, providedSize);
				return 234; // ERROR_MORE_DATA
			}

			// Write the data to the buffer
			for (uint i = 0; i < requiredSize && i < providedSize; i++)
			{
				_env.MemWrite8(lpData + i, valueBytes[i]);
			}

			if (closeHandle) _env.RegCloseKey(targetHandle);

			_logger.LogInformation("[Advapi32] RegQueryValueA: Returned {Size} bytes", requiredSize);
			return 0; // ERROR_SUCCESS
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegQueryValueA: Failed to query value");
			return 2; // ERROR_FILE_NOT_FOUND
		}
	}

	// Security functions
	[DllModuleExport(3, Version = "4.90.0.3000", IsStub = true)]
	private uint AccessCheck(uint pSecurityDescriptor, uint ClientToken, uint DesiredAccess, uint GenericMapping, uint PrivilegeSet, uint PrivilegeSetLength, uint GrantedAccess, uint AccessStatus)
	{
		_logger.LogInformation("[Advapi32] AccessCheck(stub)");
		if (AccessStatus != 0) _env.MemWrite32(AccessStatus, 1); // TRUE - access granted
		if (GrantedAccess != 0) _env.MemWrite32(GrantedAccess, DesiredAccess);
		return 1; // TRUE
	}

	[DllModuleExport(6, Version = "4.90.0.3000", IsStub = true)]
	private uint AddAccessAllowedAce(uint pAcl, uint dwAceRevision, uint AccessMask, uint pSid)
	{
		_logger.LogInformation("[Advapi32] AddAccessAllowedAce(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(11, Version = "4.90.0.3000", IsStub = true)]
	private uint AdjustTokenPrivileges(uint TokenHandle, uint DisableAllPrivileges, uint NewState, uint BufferLength, uint PreviousState, uint ReturnLength)
	{
		_logger.LogInformation("[Advapi32] AdjustTokenPrivileges(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(12, Version = "4.90.0.3000", IsStub = true)]
	private uint AllocateAndInitializeSid(uint pIdentifierAuthority, uint nSubAuthorityCount, uint nSubAuthority0, uint nSubAuthority1, uint nSubAuthority2, uint nSubAuthority3, uint nSubAuthority4, uint nSubAuthority5, uint nSubAuthority6, uint nSubAuthority7, uint pSid)
	{
		_logger.LogInformation("[Advapi32] AllocateAndInitializeSid(stub)");
		var sidHandle = _nextSidHandle++;
		if (pSid != 0) _env.MemWrite32(pSid, sidHandle);
		return 1; // TRUE
	}

	[DllModuleExport(101, Version = "4.90.0.3000")]
	private uint FreeSid(uint pSid)
	{
		_logger.LogInformation("[Advapi32] FreeSid(pSid=0x{PSid:X8})", pSid);
		return 0; // NULL (void function returns NULL)
	}

	[DllModuleExport(119, Version = "4.90.0.3000")]
	private uint GetLengthSid(uint pSid)
	{
		_logger.LogInformation("[Advapi32] GetLengthSid(pSid=0x{PSid:X8})", pSid);
		return 12; // Minimum SID size
	}

	[DllModuleExport(158, Version = "4.90.0.3000")]
	private uint ImpersonateSelf(uint ImpersonationLevel)
	{
		_logger.LogInformation("[Advapi32] ImpersonateSelf(ImpersonationLevel={ImpersonationLevel})", ImpersonationLevel);
		return 1; // TRUE
	}

	[DllModuleExport(159, Version = "4.90.0.3000", IsStub = true)]
	private uint InitializeAcl(uint pAcl, uint nAclLength, uint dwAclRevision)
	{
		_logger.LogInformation("[Advapi32] InitializeAcl(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(160, Version = "4.90.0.3000", IsStub = true)]
	private uint InitializeSecurityDescriptor(uint pSecurityDescriptor, uint dwRevision)
	{
		_logger.LogInformation("[Advapi32] InitializeSecurityDescriptor(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(166, Version = "4.90.0.3000", IsStub = true)]
	private uint IsValidSecurityDescriptor(uint pSecurityDescriptor)
	{
		_logger.LogInformation("[Advapi32] IsValidSecurityDescriptor(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(179, Version = "4.90.0.3000")]
	private uint LookupPrivilegeValueA(in LpcStr lpSystemName, in LpcStr lpName, uint lpLuid)
	{
		var systemName = lpSystemName.ToString() ?? string.Empty;
		var name = lpName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] LookupPrivilegeValueA(lpSystemName=\"{SystemName}\", lpName=\"{Name}\", lpLuid=0x{LpLuid:X8})",
			systemName, name, lpLuid);

		if (lpLuid != 0)
		{
			_env.MemWrite32(lpLuid, 1); // LUID low part
			_env.MemWrite32(lpLuid + 4, 0); // LUID high part
		}
		return 1; // TRUE
	}

	[DllModuleExport(200, Version = "4.90.0.3000")]
	private uint OpenProcessToken(uint ProcessHandle, uint DesiredAccess, uint TokenHandle)
	{
		_logger.LogInformation("[Advapi32] OpenProcessToken(ProcessHandle=0x{ProcessHandle:X8}, DesiredAccess=0x{DesiredAccess:X}, TokenHandle=0x{TokenHandle:X8})",
			ProcessHandle, DesiredAccess, TokenHandle);

		if (TokenHandle != 0)
		{
			_env.MemWrite32(TokenHandle, 0xC0000000); // Pseudo-handle for token
		}
		return 1; // TRUE
	}

	[DllModuleExport(205, Version = "4.90.0.3000", IsStub = true)]
	private uint OpenThreadToken(uint ThreadHandle, uint DesiredAccess, uint OpenAsSelf, uint TokenHandle)
	{
		_logger.LogInformation("[Advapi32] OpenThreadToken(stub)");
		if (TokenHandle != 0)
		{
			_env.MemWrite32(TokenHandle, 0xC0000001); // Pseudo-handle for token
		}
		return 1; // TRUE
	}

	[DllModuleExport(271, Version = "4.90.0.3000")]
	private uint RevertToSelf()
	{
		_logger.LogInformation("[Advapi32] RevertToSelf()");
		return 1; // TRUE
	}

	[DllModuleExport(281, Version = "4.90.0.3000", IsStub = true)]
	private uint SetSecurityDescriptorDacl(uint pSecurityDescriptor, uint bDaclPresent, uint pDacl, uint bDaclDefaulted)
	{
		_logger.LogInformation("[Advapi32] SetSecurityDescriptorDacl(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(288, Version = "4.90.0.3000", IsStub = true)]
	private uint SetSecurityDescriptorGroup(uint pSecurityDescriptor, uint pGroup, uint bGroupDefaulted)
	{
		_logger.LogInformation("[Advapi32] SetSecurityDescriptorGroup(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(289, Version = "4.90.0.3000", IsStub = true)]
	private uint SetSecurityDescriptorOwner(uint pSecurityDescriptor, uint pOwner, uint bOwnerDefaulted)
	{
		_logger.LogInformation("[Advapi32] SetSecurityDescriptorOwner(stub)");
		return 1; // TRUE
	}

	// Service functions
	[DllModuleExport(201, Version = "4.90.0.3000")]
	private uint OpenSCManagerA(in LpcStr lpMachineName, in LpcStr lpDatabaseName, uint dwDesiredAccess)
	{
		var machineName = lpMachineName.ToString() ?? string.Empty;
		var databaseName = lpDatabaseName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] OpenSCManagerA(lpMachineName=\"{MachineName}\", lpDatabaseName=\"{DatabaseName}\", dwDesiredAccess=0x{DwDesiredAccess:X})",
			machineName, databaseName, dwDesiredAccess);

		return _nextServiceHandle++; // Return pseudo-handle
	}

	[DllModuleExport(203, Version = "4.90.0.3000")]
	private uint OpenServiceA(uint hSCManager, in LpcStr lpServiceName, uint dwDesiredAccess)
	{
		var serviceName = lpServiceName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] OpenServiceA(hSCManager=0x{HSCManager:X8}, lpServiceName=\"{ServiceName}\", dwDesiredAccess=0x{DwDesiredAccess:X})",
			hSCManager, serviceName, dwDesiredAccess);

		return _nextServiceHandle++; // Return pseudo-handle
	}

	[DllModuleExport(36, Version = "4.90.0.3000")]
	private uint CloseServiceHandle(uint hSCObject)
	{
		_logger.LogInformation("[Advapi32] CloseServiceHandle(hSCObject=0x{HSCObject:X8})", hSCObject);
		return 1; // TRUE
	}

	[DllModuleExport(49, Version = "4.90.0.3000")]
	private uint CreateServiceW(uint hSCManager, uint lpServiceName, uint lpDisplayName, uint dwDesiredAccess, uint dwServiceType, uint dwStartType, uint dwErrorControl, uint lpBinaryPathName, uint lpLoadOrderGroup, uint lpdwTagId, uint lpDependencies, uint lpServiceStartName, uint lpPassword)
	{
		_logger.LogInformation("[Advapi32] CreateServiceW(stub)");
		return _nextServiceHandle++; // Return pseudo-handle
	}

	[DllModuleExport(91, Version = "4.90.0.3000")]
	private uint DeleteService(uint hService)
	{
		_logger.LogInformation("[Advapi32] DeleteService(hService=0x{HService:X8})", hService);
		return 1; // TRUE
	}

	[DllModuleExport(299, Version = "4.90.0.3000")]
	private uint StartServiceA(uint hService, uint dwNumServiceArgs, uint lpServiceArgVectors)
	{
		_logger.LogInformation("[Advapi32] StartServiceA(hService=0x{HService:X8}, dwNumServiceArgs={DwNumServiceArgs})",
			hService, dwNumServiceArgs);
		return 1; // TRUE
	}

	[DllModuleExport(37, Version = "4.90.0.3000")]
	private uint ControlService(uint hService, uint dwControl, uint lpServiceStatus)
	{
		_logger.LogInformation("[Advapi32] ControlService(hService=0x{HService:X8}, dwControl={DwControl}, lpServiceStatus=0x{LpServiceStatus:X8})",
			hService, dwControl, lpServiceStatus);
		return 1; // TRUE
	}

	[DllModuleExport(239, Version = "4.90.0.3000")]
	private uint RegOpenKeyA(uint hKey, in LpcStr lpSubKey, uint phkResult)
	{
		// RegOpenKeyA is equivalent to RegOpenKeyExA with samDesired = KEY_ALL_ACCESS
		return RegOpenKeyExA(hKey, lpSubKey, 0, 0xF003F, phkResult);
	}

	[DllModuleExport(243, Version = "4.90.0.3000")]
	private uint RegQueryInfoKeyA(uint hKey, in LpStr lpClass, uint lpcchClass, uint lpReserved,
		uint lpcSubKeys, uint lpcchMaxSubKeyLen, uint lpcchMaxClassLen, uint lpcValues,
		uint lpcchMaxValueNameLen, uint lpcbMaxValueLen, uint lpcbSecurityDescriptor, uint lpftLastWriteTime)
	{
		_logger.LogInformation("[Advapi32] RegQueryInfoKeyA(hKey=0x{HKey:X8})", hKey);

		try
		{
			// Get subkey information
			var subKeyNames = _env.RegEnumerateSubKeys(hKey);
			var valueNames = _env.RegEnumerateValues(hKey);
			
			// Calculate max lengths
			uint maxSubKeyLen = 0;
			foreach (var name in subKeyNames)
			{
				if (name.Length > maxSubKeyLen)
					maxSubKeyLen = (uint)name.Length;
			}
			
			uint maxValueNameLen = 0;
			uint maxValueLen = 0;
			foreach (var name in valueNames)
			{
				if (name.Length > maxValueNameLen)
					maxValueNameLen = (uint)name.Length;
				
				// Try to get value to determine max data length
				if (_env.RegQueryValue(hKey, name, out var value))
				{
					uint valueLen = 0;
					if (value is string str)
					{
						valueLen = (uint)(str.Length + 1); // Include null terminator
					}
					else if (value is byte[] bytes)
					{
						valueLen = (uint)bytes.Length;
					}
					else if (value is uint || value is int)
					{
						valueLen = 4;
					}
					
					if (valueLen > maxValueLen)
						maxValueLen = valueLen;
				}
			}
			
			// Write results
			if (lpcSubKeys != 0)
				_env.MemWrite32(lpcSubKeys, (uint)subKeyNames.Length);
			if (lpcValues != 0)
				_env.MemWrite32(lpcValues, (uint)valueNames.Length);
			if (lpcchMaxSubKeyLen != 0)
				_env.MemWrite32(lpcchMaxSubKeyLen, maxSubKeyLen);
			if (lpcchMaxValueNameLen != 0)
				_env.MemWrite32(lpcchMaxValueNameLen, maxValueNameLen);
			if (lpcbMaxValueLen != 0)
				_env.MemWrite32(lpcbMaxValueLen, maxValueLen);

			return 0; // ERROR_SUCCESS
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegQueryInfoKeyA: Failed to query info");
			return 2; // ERROR_FILE_NOT_FOUND
		}
	}

	[DllModuleExport(228, Version = "4.90.0.3000")]
	private uint RegEnumKeyA(uint hKey, uint dwIndex, in LpStr lpName, uint cchName)
	{
		_logger.LogInformation("[Advapi32] RegEnumKeyA(hKey=0x{HKey:X8}, dwIndex={DwIndex})", hKey, dwIndex);

		try
		{
			var subKeyNames = _env.RegEnumerateSubKeys(hKey);
			
			if (dwIndex >= subKeyNames.Length)
			{
				return 259; // ERROR_NO_MORE_ITEMS
			}
			
			var keyName = subKeyNames[dwIndex];
			
			// Write the key name to the buffer
			var nameBytes = System.Text.Encoding.ASCII.GetBytes(keyName);
			var namePtr = lpName.Address;
			
			if (namePtr != 0)
			{
				// Copy name to buffer with null terminator
				for (int i = 0; i < nameBytes.Length && i < cchName - 1; i++)
				{
					_env.MemWrite8(namePtr + (uint)i, nameBytes[i]);
				}
				_env.MemWrite8(namePtr + (uint)Math.Min(nameBytes.Length, cchName - 1), 0); // Null terminator
			}
			
			return 0; // ERROR_SUCCESS
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegEnumKeyA: Failed to enumerate key");
			return 259; // ERROR_NO_MORE_ITEMS
		}
	}

	[DllModuleExport(229, Version = "4.90.0.3000")]
	private uint RegEnumKeyExA(uint hKey, uint dwIndex, in LpStr lpName, uint lpcchName, uint lpReserved,
		in LpStr lpClass, uint lpcchClass, uint lpftLastWriteTime)
	{
		_logger.LogInformation("[Advapi32] RegEnumKeyExA(hKey=0x{HKey:X8}, dwIndex={DwIndex})", hKey, dwIndex);

		try
		{
			var subKeyNames = _env.RegEnumerateSubKeys(hKey);
			
			if (dwIndex >= subKeyNames.Length)
			{
				return 259; // ERROR_NO_MORE_ITEMS
			}
			
			var keyName = subKeyNames[dwIndex];
			
			// Read the buffer size
			uint bufferSize = 0;
			if (lpcchName != 0)
			{
				bufferSize = _env.MemRead32(lpcchName);
			}
			
			// Write the key name to the buffer
			var nameBytes = System.Text.Encoding.ASCII.GetBytes(keyName);
			var namePtr = lpName.Address;
			
			if (namePtr != 0 && bufferSize > 0)
			{
				// Copy name to buffer with null terminator
				uint copyLen = Math.Min((uint)nameBytes.Length, bufferSize - 1);
				for (uint i = 0; i < copyLen; i++)
				{
					_env.MemWrite8(namePtr + i, nameBytes[i]);
				}
				_env.MemWrite8(namePtr + copyLen, 0); // Null terminator
			}
			
			// Update the size (excluding null terminator)
			if (lpcchName != 0)
			{
				_env.MemWrite32(lpcchName, (uint)keyName.Length);
			}
			
			// Class name is typically not used, leave it empty
			if (lpcchClass != 0)
			{
				_env.MemWrite32(lpcchClass, 0);
			}
			
			return 0; // ERROR_SUCCESS
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegEnumKeyExA: Failed to enumerate key");
			return 259; // ERROR_NO_MORE_ITEMS
		}
	}

	[DllModuleExport(232, Version = "4.90.0.3000")]
	private uint RegEnumValueA(uint hKey, uint dwIndex, in LpStr lpValueName, uint lpcchValueName,
		uint lpReserved, uint lpType, uint lpData, uint lpcbData)
	{
		_logger.LogInformation("[Advapi32] RegEnumValueA(hKey=0x{HKey:X8}, dwIndex={DwIndex})", hKey, dwIndex);

		try
		{
			var valueNames = _env.RegEnumerateValues(hKey);
			
			if (dwIndex >= valueNames.Length)
			{
				return 259; // ERROR_NO_MORE_ITEMS
			}
			
			var valueName = valueNames[dwIndex];
			
			// Read the buffer sizes
			uint nameBufferSize = 0;
			uint dataBufferSize = 0;
			if (lpcchValueName != 0)
			{
				nameBufferSize = _env.MemRead32(lpcchValueName);
			}
			if (lpcbData != 0)
			{
				dataBufferSize = _env.MemRead32(lpcbData);
			}
			
			// Write the value name to the buffer
			var nameBytes = System.Text.Encoding.ASCII.GetBytes(valueName);
			var namePtr = lpValueName.Address;
			
			if (namePtr != 0 && nameBufferSize > 0)
			{
				// Copy name to buffer with null terminator
				uint copyLen = Math.Min((uint)nameBytes.Length, nameBufferSize - 1);
				for (uint i = 0; i < copyLen; i++)
				{
					_env.MemWrite8(namePtr + i, nameBytes[i]);
				}
				_env.MemWrite8(namePtr + copyLen, 0); // Null terminator
			}
			
			// Update the name size (excluding null terminator)
			if (lpcchValueName != 0)
			{
				_env.MemWrite32(lpcchValueName, (uint)valueName.Length);
			}
			
			// Get the value data
			if (_env.RegQueryValue(hKey, valueName, out var value))
			{
				// Determine the data type and size
				uint dataType;
				byte[] dataBytes;

				if (value is string str)
				{
					dataType = REG_SZ;
					dataBytes = System.Text.Encoding.ASCII.GetBytes(str + '\0'); // Null-terminated
				}
				else if (value is uint dwordVal)
				{
					dataType = REG_DWORD;
					dataBytes = BitConverter.GetBytes(dwordVal);
				}
				else if (value is int intVal)
				{
					dataType = REG_DWORD;
					dataBytes = BitConverter.GetBytes((uint)intVal);
				}
				else if (value is byte[] binaryVal)
				{
					dataType = REG_BINARY;
					dataBytes = binaryVal;
				}
				else
				{
					// Unknown type - convert to string
					dataType = REG_SZ;
					var fallbackStr = value?.ToString() ?? string.Empty;
					dataBytes = System.Text.Encoding.ASCII.GetBytes(fallbackStr + '\0');
				}
				
				// Write the data type
				if (lpType != 0)
				{
					_env.MemWrite32(lpType, dataType);
				}
				
				// Write the data if buffer provided
				if (lpData != 0 && dataBufferSize >= dataBytes.Length)
				{
					for (uint i = 0; i < dataBytes.Length; i++)
					{
						_env.MemWrite8(lpData + i, dataBytes[i]);
					}
				}
				
				// Update data size
				if (lpcbData != 0)
				{
					_env.MemWrite32(lpcbData, (uint)dataBytes.Length);
				}
			}
			
			return 0; // ERROR_SUCCESS
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegEnumValueA: Failed to enumerate value");
			return 259; // ERROR_NO_MORE_ITEMS
		}
	}

	[DllModuleExport(224, Version = "4.90.0.3000")]
	private uint RegDeleteKeyA(uint hKey, in LpcStr lpSubKey)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegDeleteKeyA(hKey=0x{HKey:X8}, lpSubKey=\"{SubKey}\")", hKey, subKey);

		try
		{
			// Get the path of the parent key
			var keyPath = _env.RegistryHive?.GetKeyPath(hKey);
			if (string.IsNullOrEmpty(keyPath))
			{
				_logger.LogWarning("[Advapi32] RegDeleteKeyA: Invalid key handle");
				return 2; // ERROR_FILE_NOT_FOUND
			}
			
			// Build the full path to the subkey
			var fullPath = string.IsNullOrEmpty(subKey) ? keyPath : $"{keyPath}\\{subKey}";
			
			// Delete the subkey
			if (_env.RegDeleteKey(fullPath))
			{
				return 0; // ERROR_SUCCESS
			}
			else
			{
				return 2; // ERROR_FILE_NOT_FOUND
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegDeleteKeyA: Failed to delete key");
			return 5; // ERROR_ACCESS_DENIED
		}
	}

	[DllModuleExport(226, Version = "4.90.0.3000")]
	private uint RegDeleteValueA(uint hKey, in LpcStr lpValueName)
	{
		var valueName = lpValueName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegDeleteValueA(hKey=0x{HKey:X8}, lpValueName=\"{ValueName}\")", hKey, valueName);

		try
		{
			if (_env.RegDeleteValue(hKey, valueName))
			{
				return 0; // ERROR_SUCCESS
			}
			else
			{
				return 2; // ERROR_FILE_NOT_FOUND
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegDeleteValueA: Failed to delete value");
			return 5; // ERROR_ACCESS_DENIED
		}
	}

	[DllModuleExport(220, Version = "4.90.0.3000")]
	private uint RegCreateKeyA(uint hKey, in LpcStr lpSubKey, uint phkResult)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegCreateKeyA(hKey=0x{HKey:X8}, lpSubKey=\"{SubKey}\", phkResult=0x{PhkResult:X8})", hKey, subKey, phkResult);

		try
		{
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
				_ => _env.RegistryHive?.GetKeyPath(hKey) ?? $"0x{hKey:X8}"
			};

			var fullPath = string.IsNullOrEmpty(subKey) ? hKeyName : $"{hKeyName}\\{subKey}";

			// Create or open the virtual registry key
			var handle = _env.RegCreateKey(fullPath);

			// Write the handle to the output parameter
			if (phkResult != 0)
			{
				_env.MemWrite32(phkResult, handle);
			}

			return 0; // ERROR_SUCCESS
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegCreateKeyA: Failed to create key");
			return 5; // ERROR_ACCESS_DENIED
		}
	}

	[DllModuleExport(259, Version = "4.90.0.3000")]
	private uint RegSetValueA(uint hKey, in LpcStr lpSubKey, uint dwType, uint lpData, uint cbData)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegSetValueA(hKey=0x{HKey:X8}, lpSubKey=\"{SubKey}\", dwType={DwType}, lpData=0x{LpData:X8}, cbData={CbData})",
			hKey, subKey, dwType, lpData, cbData);

		try
		{
			// RegSetValueA sets the default (unnamed) value of a subkey
			// If lpSubKey is not null/empty, we need to open/create the subkey first
			uint targetHandle = hKey;
			bool closeHandle = false;

			if (!string.IsNullOrEmpty(subKey))
			{
				// Get the parent key path
				var keyPath = _env.RegistryHive?.GetKeyPath(hKey);
				if (string.IsNullOrEmpty(keyPath))
				{
					// hKey might be a predefined key
					const uint HKEY_CLASSES_ROOT = 0x80000000;
					const uint HKEY_CURRENT_USER = 0x80000001;
					const uint HKEY_LOCAL_MACHINE = 0x80000002;
					const uint HKEY_USERS = 0x80000003;

					keyPath = hKey switch
					{
						HKEY_CLASSES_ROOT => "HKEY_CLASSES_ROOT",
						HKEY_CURRENT_USER => "HKEY_CURRENT_USER",
						HKEY_LOCAL_MACHINE => "HKEY_LOCAL_MACHINE",
						HKEY_USERS => "HKEY_USERS",
						_ => null
					};
				}

				if (!string.IsNullOrEmpty(keyPath))
				{
					var fullPath = $"{keyPath}\\{subKey}";
					targetHandle = _env.RegCreateKey(fullPath);
					closeHandle = true;
				}
				else
				{
					_logger.LogWarning("[Advapi32] RegSetValueA: Invalid key handle");
					return 2; // ERROR_FILE_NOT_FOUND
				}
			}

			// Read the data from memory
			if (lpData == 0 || cbData == 0)
			{
				_logger.LogWarning("[Advapi32] RegSetValueA: Invalid data pointer or size");
				if (closeHandle) _env.RegCloseKey(targetHandle);
				return 87; // ERROR_INVALID_PARAMETER
			}

			var data = new byte[cbData];
			for (uint i = 0; i < cbData; i++)
			{
				data[i] = _env.MemRead8(lpData + i);
			}

			// RegSetValueA always sets a REG_SZ value
			var strLen = Array.IndexOf(data, (byte)0);
			if (strLen < 0) strLen = data.Length;
			var value = System.Text.Encoding.ASCII.GetString(data, 0, strLen);

			// Set the default (unnamed) value
			var success = _env.RegSetValue(targetHandle, "", value, DiscUtils.Registry.RegistryValueType.String);

			if (closeHandle) _env.RegCloseKey(targetHandle);

			return success ? 0u : 5u; // ERROR_SUCCESS or ERROR_ACCESS_DENIED
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Advapi32] RegSetValueA: Failed to set value");
			return 5; // ERROR_ACCESS_DENIED
		}
	}

	[DllModuleExport(116, Version = "4.90.0.3000", IsStub = true)]
	private uint GetFileSecurityA(in LpcStr lpFileName, uint RequestedInformation, uint pSecurityDescriptor, uint nLength, uint lpnLengthNeeded)
	{
		var fileName = lpFileName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] GetFileSecurityA(lpFileName=\"{FileName}\", RequestedInformation=0x{RequestedInformation:X}, nLength={NLength})",
			fileName, RequestedInformation, nLength);

		// Stub implementation - report insufficient buffer
		if (lpnLengthNeeded != 0)
		{
			_env.MemWrite32(lpnLengthNeeded, 20); // Minimal security descriptor size
		}

		return 0; // FALSE (insufficient buffer)
	}

	[DllModuleExport(279, Version = "4.90.0.3000", IsStub = true)]
	private uint SetFileSecurityA(in LpcStr lpFileName, uint SecurityInformation, uint pSecurityDescriptor)
	{
		var fileName = lpFileName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] SetFileSecurityA(lpFileName=\"{FileName}\", SecurityInformation=0x{SecurityInformation:X}, pSecurityDescriptor=0x{PSecurityDescriptor:X8})",
			fileName, SecurityInformation, pSecurityDescriptor);

		// Stub implementation - report success
		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves the name of the user associated with the current thread.
	/// BOOL GetUserNameA(
	///   [out]     LPSTR   lpBuffer,
	///   [in, out] LPDWORD pcbBuffer
	/// );
	/// </summary>
	[DllModuleExport(154, Version = "4.90.0.3000")]
	private uint GetUserNameA(uint lpBuffer, uint pcbBuffer)
	{
		_logger.LogInformation("[Advapi32] GetUserNameA(lpBuffer=0x{LpBuffer:X8}, pcbBuffer=0x{PcbBuffer:X8})",
			lpBuffer, pcbBuffer);

		// Default username for emulation
		const string username = "Player";
		var usernameBytes = System.Text.Encoding.ASCII.GetBytes(username);
		var requiredSize = (uint)(usernameBytes.Length + 1); // +1 for null terminator

		// Read the buffer size
		uint bufferSize = 0;
		if (pcbBuffer != 0)
		{
			bufferSize = _env.MemRead32(pcbBuffer);
		}

		// Check if buffer is large enough
		if (bufferSize < requiredSize)
		{
			// Write required size
			if (pcbBuffer != 0)
			{
				_env.MemWrite32(pcbBuffer, requiredSize);
			}
			// ERROR_INSUFFICIENT_BUFFER would normally be set via SetLastError
			// but Advapi32Module doesn't have direct access to that
			return 0; // FALSE
		}

		// Write username to buffer
		if (lpBuffer != 0)
		{
			for (int i = 0; i < usernameBytes.Length; i++)
			{
				_env.MemWrite8(lpBuffer + (uint)i, usernameBytes[i]);
			}
			// Write null terminator
			_env.MemWrite8(lpBuffer + (uint)usernameBytes.Length, 0);
		}

		// Write actual size (including null terminator)
		if (pcbBuffer != 0)
		{
			_env.MemWrite32(pcbBuffer, requiredSize);
		}

		return 1; // TRUE
	}

	/// <summary>
	/// Loads a registry hive from a file into the registry.
	/// LSTATUS RegLoadKeyA(
	///   [in] HKEY   hKey,
	///   [in] LPCSTR lpSubKey,
	///   [in] LPCSTR lpFile
	/// );
	/// </summary>
	[DllModuleExport(236, Version = "4.90.0.3000", IsStub = true)]
	private uint RegLoadKeyA(uint hKey, in LpcStr lpSubKey, in LpcStr lpFile)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		var file = lpFile.ToString() ?? string.Empty;

		_logger.LogInformation("[Advapi32] RegLoadKeyA(hKey=0x{HKey:X8}, lpSubKey=\"{LpSubKey}\", lpFile=\"{LpFile}\")",
			hKey, subKey, file);

		// RegLoadKey requires SE_RESTORE_NAME privilege
		// This is a privileged operation typically used by system utilities
		// For emulation purposes, we'll return ERROR_ACCESS_DENIED

		_logger.LogWarning("[Advapi32] RegLoadKeyA: Privileged operation not supported in emulator");
		return (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
	}

	/// <summary>
	/// Unloads a registry hive from the registry.
	/// LSTATUS RegUnLoadKeyA(
	///   [in] HKEY   hKey,
	///   [in] LPCSTR lpSubKey
	/// );
	/// </summary>
	[DllModuleExport(263, Version = "4.90.0.3000")]
	private uint RegUnLoadKeyA(uint hKey, in LpcStr lpSubKey)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;

		_logger.LogInformation("[Advapi32] RegUnLoadKeyA(hKey=0x{HKey:X8}, lpSubKey=\"{LpSubKey}\")",
			hKey, subKey);

		// RegUnLoadKey requires SE_RESTORE_NAME privilege
		// This is a privileged operation typically used by system utilities
		// For emulation purposes, we'll return ERROR_ACCESS_DENIED

		_logger.LogWarning("[Advapi32] RegUnLoadKeyA: Privileged operation not supported in emulator");
		return (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
	}

	/// <summary>
	/// Establishes a connection to a predefined registry key on another computer.
	/// LONG RegConnectRegistryA(LPCSTR lpMachineName, HKEY hKey, PHKEY phkResult);
	/// </summary>
	[DllModuleExport(218, Version = "4.90.0.3000")]
	private uint RegConnectRegistryA(in LpcStr lpMachineName, uint hKey, uint phkResult)
	{
		var machineName = lpMachineName.Read(_env.Memory) ?? "";
		_logger.LogInformation("[Advapi32] RegConnectRegistryA(lpMachineName='{LpMachineName}', hKey=0x{HKey:X8}, phkResult=0x{PhkResult:X8})",
			machineName, hKey, phkResult);

		// For local machine or null, just duplicate the key handle
		if (string.IsNullOrEmpty(machineName) || machineName == "." || machineName.StartsWith("\\\\."))
		{
			if (phkResult != 0)
			{
				_env.MemWrite32(phkResult, hKey);
			}
			return 0; // ERROR_SUCCESS
		}

		// Remote registry not supported
		return 53; // ERROR_BAD_NETPATH
	}
}
