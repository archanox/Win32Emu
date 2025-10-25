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

	private uint _nextServiceHandle = 0xB0000000;
	private readonly Dictionary<uint, ServiceData> _services = new();
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
			case "REGCLOSEKEY":
				returnValue = RegCloseKey(a.UInt32(0));
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
	[DllModuleExport(36)]
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
	[DllModuleExport(24)]
	private uint RegSetValueExA(uint hKey, in LpcStr lpValueName, uint reserved, uint dwType, uint lpData, uint cbData)
	{
		var valueName = lpValueName.ToString() ?? string.Empty;
		
		_logger.LogInformation("[Advapi32] RegSetValueExA(hKey=0x{HKey:X8}, lpValueName=\"{ValueName}\", type=0x{DwType:X}, lpData=0x{LpData:X8}, cbData={CbData})",
			hKey, valueName, dwType, lpData, cbData);

		// For simplicity, just log the operation without actually storing the data
		// A full implementation would read the data from lpData and store it in the virtual registry
		
		if (lpData != 0 && cbData > 0)
		{
			// Could read the data here if needed for emulation
			// For now, just acknowledge the set operation
			_logger.LogInformation("[Advapi32] RegSetValueExA: Setting value (data not stored in emulation)");
		}

		// ERROR_SUCCESS
		return 0;
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
	[DllModuleExport(16)]
	private uint RegQueryValueA(uint hKey, in LpcStr lpSubKey, uint lpData, uint lpcbData)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegQueryValueA(hKey=0x{HKey:X8}, lpSubKey=\"{SubKey}\", lpData=0x{LpData:X8}, lpcbData=0x{LpcbData:X8})",
			hKey, subKey, lpData, lpcbData);

		// Return empty string (stub)
		if (lpcbData != 0)
		{
			_env.MemWrite32(lpcbData, 0);
		}

		return 2; // ERROR_FILE_NOT_FOUND
	}

	// Security functions
	[DllModuleExport(32)]
	private uint AccessCheck(uint pSecurityDescriptor, uint ClientToken, uint DesiredAccess, uint GenericMapping, uint PrivilegeSet, uint PrivilegeSetLength, uint GrantedAccess, uint AccessStatus)
	{
		_logger.LogInformation("[Advapi32] AccessCheck(stub)");
		if (AccessStatus != 0) _env.MemWrite32(AccessStatus, 1); // TRUE - access granted
		if (GrantedAccess != 0) _env.MemWrite32(GrantedAccess, DesiredAccess);
		return 1; // TRUE
	}

	[DllModuleExport(16)]
	private uint AddAccessAllowedAce(uint pAcl, uint dwAceRevision, uint AccessMask, uint pSid)
	{
		_logger.LogInformation("[Advapi32] AddAccessAllowedAce(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(24)]
	private uint AdjustTokenPrivileges(uint TokenHandle, uint DisableAllPrivileges, uint NewState, uint BufferLength, uint PreviousState, uint ReturnLength)
	{
		_logger.LogInformation("[Advapi32] AdjustTokenPrivileges(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(44)]
	private uint AllocateAndInitializeSid(uint pIdentifierAuthority, uint nSubAuthorityCount, uint nSubAuthority0, uint nSubAuthority1, uint nSubAuthority2, uint nSubAuthority3, uint nSubAuthority4, uint nSubAuthority5, uint nSubAuthority6, uint nSubAuthority7, uint pSid)
	{
		_logger.LogInformation("[Advapi32] AllocateAndInitializeSid(stub)");
		var sidHandle = _nextSidHandle++;
		if (pSid != 0) _env.MemWrite32(pSid, sidHandle);
		return 1; // TRUE
	}

	[DllModuleExport(4)]
	private uint FreeSid(uint pSid)
	{
		_logger.LogInformation("[Advapi32] FreeSid(pSid=0x{PSid:X8})", pSid);
		return 0; // NULL (void function returns NULL)
	}

	[DllModuleExport(4)]
	private uint GetLengthSid(uint pSid)
	{
		_logger.LogInformation("[Advapi32] GetLengthSid(pSid=0x{PSid:X8})", pSid);
		return 12; // Minimum SID size
	}

	[DllModuleExport(4)]
	private uint ImpersonateSelf(uint ImpersonationLevel)
	{
		_logger.LogInformation("[Advapi32] ImpersonateSelf(ImpersonationLevel={ImpersonationLevel})", ImpersonationLevel);
		return 1; // TRUE
	}

	[DllModuleExport(12)]
	private uint InitializeAcl(uint pAcl, uint nAclLength, uint dwAclRevision)
	{
		_logger.LogInformation("[Advapi32] InitializeAcl(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(8)]
	private uint InitializeSecurityDescriptor(uint pSecurityDescriptor, uint dwRevision)
	{
		_logger.LogInformation("[Advapi32] InitializeSecurityDescriptor(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(4)]
	private uint IsValidSecurityDescriptor(uint pSecurityDescriptor)
	{
		_logger.LogInformation("[Advapi32] IsValidSecurityDescriptor(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(12)]
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

	[DllModuleExport(12)]
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

	[DllModuleExport(16)]
	private uint OpenThreadToken(uint ThreadHandle, uint DesiredAccess, uint OpenAsSelf, uint TokenHandle)
	{
		_logger.LogInformation("[Advapi32] OpenThreadToken(stub)");
		if (TokenHandle != 0)
		{
			_env.MemWrite32(TokenHandle, 0xC0000001); // Pseudo-handle for token
		}
		return 1; // TRUE
	}

	[DllModuleExport(0)]
	private uint RevertToSelf()
	{
		_logger.LogInformation("[Advapi32] RevertToSelf()");
		return 1; // TRUE
	}

	[DllModuleExport(16)]
	private uint SetSecurityDescriptorDacl(uint pSecurityDescriptor, uint bDaclPresent, uint pDacl, uint bDaclDefaulted)
	{
		_logger.LogInformation("[Advapi32] SetSecurityDescriptorDacl(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(12)]
	private uint SetSecurityDescriptorGroup(uint pSecurityDescriptor, uint pGroup, uint bGroupDefaulted)
	{
		_logger.LogInformation("[Advapi32] SetSecurityDescriptorGroup(stub)");
		return 1; // TRUE
	}

	[DllModuleExport(12)]
	private uint SetSecurityDescriptorOwner(uint pSecurityDescriptor, uint pOwner, uint bOwnerDefaulted)
	{
		_logger.LogInformation("[Advapi32] SetSecurityDescriptorOwner(stub)");
		return 1; // TRUE
	}

	// Service functions
	[DllModuleExport(12)]
	private uint OpenSCManagerA(in LpcStr lpMachineName, in LpcStr lpDatabaseName, uint dwDesiredAccess)
	{
		var machineName = lpMachineName.ToString() ?? string.Empty;
		var databaseName = lpDatabaseName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] OpenSCManagerA(lpMachineName=\"{MachineName}\", lpDatabaseName=\"{DatabaseName}\", dwDesiredAccess=0x{DwDesiredAccess:X})",
			machineName, databaseName, dwDesiredAccess);
		
		return _nextServiceHandle++; // Return pseudo-handle
	}

	[DllModuleExport(12)]
	private uint OpenServiceA(uint hSCManager, in LpcStr lpServiceName, uint dwDesiredAccess)
	{
		var serviceName = lpServiceName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] OpenServiceA(hSCManager=0x{HSCManager:X8}, lpServiceName=\"{ServiceName}\", dwDesiredAccess=0x{DwDesiredAccess:X})",
			hSCManager, serviceName, dwDesiredAccess);
		
		return _nextServiceHandle++; // Return pseudo-handle
	}

	[DllModuleExport(4)]
	private uint CloseServiceHandle(uint hSCObject)
	{
		_logger.LogInformation("[Advapi32] CloseServiceHandle(hSCObject=0x{HSCObject:X8})", hSCObject);
		return 1; // TRUE
	}

	[DllModuleExport(52)]
	private uint CreateServiceW(uint hSCManager, uint lpServiceName, uint lpDisplayName, uint dwDesiredAccess, uint dwServiceType, uint dwStartType, uint dwErrorControl, uint lpBinaryPathName, uint lpLoadOrderGroup, uint lpdwTagId, uint lpDependencies, uint lpServiceStartName, uint lpPassword)
	{
		_logger.LogInformation("[Advapi32] CreateServiceW(stub)");
		return _nextServiceHandle++; // Return pseudo-handle
	}

	[DllModuleExport(4)]
	private uint DeleteService(uint hService)
	{
		_logger.LogInformation("[Advapi32] DeleteService(hService=0x{HService:X8})", hService);
		return 1; // TRUE
	}

	[DllModuleExport(12)]
	private uint StartServiceA(uint hService, uint dwNumServiceArgs, uint lpServiceArgVectors)
	{
		_logger.LogInformation("[Advapi32] StartServiceA(hService=0x{HService:X8}, dwNumServiceArgs={DwNumServiceArgs})",
			hService, dwNumServiceArgs);
		return 1; // TRUE
	}

	[DllModuleExport(12)]
	private uint ControlService(uint hService, uint dwControl, uint lpServiceStatus)
	{
		_logger.LogInformation("[Advapi32] ControlService(hService=0x{HService:X8}, dwControl={DwControl}, lpServiceStatus=0x{LpServiceStatus:X8})",
			hService, dwControl, lpServiceStatus);
		return 1; // TRUE
	}

	private uint RegOpenKeyA(uint hKey, in LpcStr lpSubKey, uint phkResult)
	{
		// RegOpenKeyA is equivalent to RegOpenKeyExA with samDesired = KEY_ALL_ACCESS
		return RegOpenKeyExA(hKey, lpSubKey, 0, 0xF003F, phkResult);
	}

	private uint RegQueryInfoKeyA(uint hKey, in LpStr lpClass, uint lpcchClass, uint lpReserved,
		uint lpcSubKeys, uint lpcchMaxSubKeyLen, uint lpcchMaxClassLen, uint lpcValues,
		uint lpcchMaxValueNameLen, uint lpcbMaxValueLen, uint lpcbSecurityDescriptor, uint lpftLastWriteTime)
	{
		_logger.LogInformation("[Advapi32] RegQueryInfoKeyA(hKey=0x{HKey:X8})", hKey);
		
		// Stub implementation - return zeros for all counts
		if (lpcSubKeys != 0)
			_env.MemWrite32(lpcSubKeys, 0);
		if (lpcValues != 0)
			_env.MemWrite32(lpcValues, 0);
		if (lpcchMaxSubKeyLen != 0)
			_env.MemWrite32(lpcchMaxSubKeyLen, 0);
		if (lpcchMaxValueNameLen != 0)
			_env.MemWrite32(lpcchMaxValueNameLen, 0);
		if (lpcbMaxValueLen != 0)
			_env.MemWrite32(lpcbMaxValueLen, 0);
		
		return 0; // ERROR_SUCCESS
	}

	private uint RegEnumKeyA(uint hKey, uint dwIndex, in LpStr lpName, uint cchName)
	{
		_logger.LogInformation("[Advapi32] RegEnumKeyA(hKey=0x{HKey:X8}, dwIndex={DwIndex})", hKey, dwIndex);
		
		// Stub implementation - no keys to enumerate
		return 259; // ERROR_NO_MORE_ITEMS
	}

	private uint RegEnumKeyExA(uint hKey, uint dwIndex, in LpStr lpName, uint lpcchName, uint lpReserved,
		in LpStr lpClass, uint lpcchClass, uint lpftLastWriteTime)
	{
		_logger.LogInformation("[Advapi32] RegEnumKeyExA(hKey=0x{HKey:X8}, dwIndex={DwIndex})", hKey, dwIndex);
		
		// Stub implementation - no keys to enumerate
		return 259; // ERROR_NO_MORE_ITEMS
	}

	private uint RegEnumValueA(uint hKey, uint dwIndex, in LpStr lpValueName, uint lpcchValueName,
		uint lpReserved, uint lpType, uint lpData, uint lpcbData)
	{
		_logger.LogInformation("[Advapi32] RegEnumValueA(hKey=0x{HKey:X8}, dwIndex={DwIndex})", hKey, dwIndex);
		
		// Stub implementation - no values to enumerate
		return 259; // ERROR_NO_MORE_ITEMS
	}

	private uint RegDeleteKeyA(uint hKey, in LpcStr lpSubKey)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegDeleteKeyA(hKey=0x{HKey:X8}, lpSubKey=\"{SubKey}\")", hKey, subKey);
		
		// Stub implementation - report success
		return 0; // ERROR_SUCCESS
	}

	private uint RegDeleteValueA(uint hKey, in LpcStr lpValueName)
	{
		var valueName = lpValueName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegDeleteValueA(hKey=0x{HKey:X8}, lpValueName=\"{ValueName}\")", hKey, valueName);
		
		// Stub implementation - report success
		return 0; // ERROR_SUCCESS
	}

	[DllModuleExport(12)]
	private uint RegCreateKeyA(uint hKey, in LpcStr lpSubKey, uint phkResult)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegCreateKeyA(hKey=0x{HKey:X8}, lpSubKey=\"{SubKey}\", phkResult=0x{PhkResult:X8})", hKey, subKey, phkResult);
		
		// Create a dummy handle
		uint handle = 0xABCD0000 | (uint)(subKey.GetHashCode() & 0xFFFF);
		if (phkResult != 0)
		{
			_env.MemWrite32(phkResult, handle);
		}
		
		return 0; // ERROR_SUCCESS
	}

	[DllModuleExport(16)]
	private uint RegSetValueA(uint hKey, in LpcStr lpSubKey, uint dwType, uint lpData, uint cbData)
	{
		var subKey = lpSubKey.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] RegSetValueA(hKey=0x{HKey:X8}, lpSubKey=\"{SubKey}\", dwType={DwType}, lpData=0x{LpData:X8}, cbData={CbData})",
			hKey, subKey, dwType, lpData, cbData);
		
		// Stub implementation - report success
		return 0; // ERROR_SUCCESS
	}

	[DllModuleExport(20)]
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

	[DllModuleExport(12)]
	private uint SetFileSecurityA(in LpcStr lpFileName, uint SecurityInformation, uint pSecurityDescriptor)
	{
		var fileName = lpFileName.ToString() ?? string.Empty;
		_logger.LogInformation("[Advapi32] SetFileSecurityA(lpFileName=\"{FileName}\", SecurityInformation=0x{SecurityInformation:X}, pSecurityDescriptor=0x{PSecurityDescriptor:X8})",
			fileName, SecurityInformation, pSecurityDescriptor);
		
		// Stub implementation - report success
		return 1; // TRUE
	}

	private class ServiceData
	{
		public string Name { get; set; } = string.Empty;
		public uint Handle { get; set; }
	}
}
