using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// SHELL32.DLL module - provides Windows Shell functions including file operations and special folders.
/// </summary>
public class Shell32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Shell32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "SHELL32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "SHBROWSEFORFOLDERA":
				returnValue = SHBrowseForFolderA(a.UInt32(0));
				return true;

			case "SHCHANGENOTIFY":
				returnValue = SHChangeNotify(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "SHFILEOPERATIONA":
				returnValue = SHFileOperationA(a.UInt32(0));
				return true;

			case "SHGETMALLOC":
				returnValue = SHGetMalloc(a.UInt32(0));
				return true;

			case "SHGETPATHFROMIDLISTA":
				returnValue = SHGetPathFromIDListA(a.UInt32(0), a.LpStr(1));
				return true;

			case "SHGETSPECIALFOLDERLOCATION":
				returnValue = SHGetSpecialFolderLocation(a.UInt32(0), a.Int32(1), a.UInt32(2));
				return true;

			case "SHELLEXECUTEA":
				returnValue = ShellExecuteA(a.UInt32(0), a.LpcStr(1), a.LpcStr(2), a.LpcStr(3), a.LpcStr(4), a.Int32(5));
				return true;

			default:
				_logger.LogInformation("[Shell32] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Displays a dialog box that enables the user to select a Shell folder.
	/// PIDLIST_ABSOLUTE SHBrowseForFolderA([in] LPBROWSEINFOA lpbi);
	/// </summary>
	[DllModuleExport(8)]
	private uint SHBrowseForFolderA(uint lpbi)
	{
		_logger.LogInformation("[Shell32] SHBrowseForFolderA(lpbi=0x{Lpbi:X8})", lpbi);

		// Stub - return NULL (user canceled)
		return 0;
	}

	/// <summary>
	/// Notifies the system of an event that an application has performed.
	/// void SHChangeNotify(LONG wEventId, UINT uFlags, LPCVOID dwItem1, LPCVOID dwItem2);
	/// </summary>
	[DllModuleExport(16)]
	private uint SHChangeNotify(uint wEventId, uint uFlags, uint dwItem1, uint dwItem2)
	{
		_logger.LogInformation("[Shell32] SHChangeNotify(wEventId=0x{WEventId:X}, uFlags=0x{UFlags:X}, dwItem1=0x{DwItem1:X8}, dwItem2=0x{DwItem2:X8})",
			wEventId, uFlags, dwItem1, dwItem2);

		// Stub - just log the notification, no actual effect
		return 0; // void function
	}

	/// <summary>
	/// Copies, moves, renames, or deletes a file system object.
	/// int SHFileOperationA([in, out] LPSHFILEOPSTRUCTA lpFileOp);
	/// </summary>
	[DllModuleExport(8)]
	private uint SHFileOperationA(uint lpFileOp)
	{
		_logger.LogInformation("[Shell32] SHFileOperationA(lpFileOp=0x{LpFileOp:X8})", lpFileOp);

		// Stub - return success without performing operation
		return 0; // Success
	}

	/// <summary>
	/// Retrieves a pointer to the shell's IMalloc interface.
	/// HRESULT SHGetMalloc([out] LPMALLOC *ppMalloc);
	/// </summary>
	[DllModuleExport(8)]
	private uint SHGetMalloc(uint ppMalloc)
	{
		_logger.LogInformation("[Shell32] SHGetMalloc(ppMalloc=0x{PpMalloc:X8})", ppMalloc);

		// Stub - return E_NOTIMPL
		if (ppMalloc != 0)
		{
			_env.MemWrite32(ppMalloc, 0);
		}
		
		return 0x80004001; // E_NOTIMPL
	}

	/// <summary>
	/// Converts an item identifier list to a file system path.
	/// BOOL SHGetPathFromIDListA([in] PCIDLIST_ABSOLUTE pidl, [out] LPSTR pszPath);
	/// </summary>
	[DllModuleExport(8)]
	private uint SHGetPathFromIDListA(uint pidl, in LpStr pszPath)
	{
		_logger.LogInformation("[Shell32] SHGetPathFromIDListA(pidl=0x{Pidl:X8}, pszPath=0x{PszPath:X8})", 
			pidl, pszPath.Address);

		// Stub - return FALSE (conversion failed)
		return 0; // FALSE
	}

	/// <summary>
	/// Retrieves the path of a special folder, identified by its CSIDL.
	/// HRESULT SHGetSpecialFolderLocation(HWND hwnd, int csidl, PIDLIST_ABSOLUTE *ppidl);
	/// </summary>
	[DllModuleExport(12)]
	private uint SHGetSpecialFolderLocation(uint hwnd, int csidl, uint ppidl)
	{
		_logger.LogInformation("[Shell32] SHGetSpecialFolderLocation(hwnd=0x{Hwnd:X8}, csidl={Csidl}, ppidl=0x{Ppidl:X8})", 
			hwnd, csidl, ppidl);

		// Stub - return E_NOTIMPL
		if (ppidl != 0)
		{
			_env.MemWrite32(ppidl, 0);
		}

		return 0x80004001; // E_NOTIMPL
	}

	/// <summary>
	/// Performs an operation on a specified file.
	/// HINSTANCE ShellExecuteA(
	///   [in, optional] HWND   hwnd,
	///   [in, optional] LPCSTR lpOperation,
	///   [in]           LPCSTR lpFile,
	///   [in, optional] LPCSTR lpParameters,
	///   [in, optional] LPCSTR lpDirectory,
	///   [in]           INT    nShowCmd
	/// );
	/// </summary>
	[DllModuleExport(24)]
	private uint ShellExecuteA(uint hwnd, in LpcStr lpOperation, in LpcStr lpFile, in LpcStr lpParameters, in LpcStr lpDirectory, int nShowCmd)
	{
		var operation = lpOperation.ToString() ?? string.Empty;
		var file = lpFile.ToString() ?? string.Empty;
		var parameters = lpParameters.ToString() ?? string.Empty;
		var directory = lpDirectory.ToString() ?? string.Empty;
		
		_logger.LogInformation("[Shell32] ShellExecuteA(hwnd=0x{Hwnd:X8}, lpOperation=\"{Operation}\", lpFile=\"{File}\", lpParameters=\"{Parameters}\", lpDirectory=\"{Directory}\", nShowCmd={NShowCmd})",
			hwnd, operation, file, parameters, directory, nShowCmd);

		// Stub - return value > 32 indicates success
		return 33; // Success
	}
}
