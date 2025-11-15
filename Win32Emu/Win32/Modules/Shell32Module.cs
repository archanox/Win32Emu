using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// SHELL32.DLL module - provides Windows Shell functions including file operations and special folders.
/// </summary>
public partial class Shell32Module : IWin32ModuleUnsafe
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
			case "DRAGFINISH":
				returnValue = DragFinish(a.UInt32(0));
				return true;
			case "DRAGQUERYFILEA":
				returnValue = DragQueryFileA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "SHELLEXECUTEEXA":
				returnValue = ShellExecuteExA(a.UInt32(0));
				return true;
			case "EXTRACTICONA":
				returnValue = ExtractIconA(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
				return true;
			case "SHGETFILEINFOA":
				returnValue = SHGetFileInfoA(a.LpcStr(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "SHELLABOUTA":
				returnValue = ShellAboutA(a.UInt32(0), a.LpcStr(1), a.LpcStr(2), a.UInt32(3));
				return true;

			default:
				LogUnimplementedExport(export);
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
		LogSHBrowseForFolderA(lpbi);

		if (lpbi == 0)
		{
			_logger.LogWarning("[Shell32] SHBrowseForFolderA: NULL BROWSEINFO pointer");
			return 0;
		}

		// Read BROWSEINFO structure
		// typedef struct _browseinfoA {
		//   HWND        hwndOwner;       // +0
		//   LPCITEMIDLIST pidlRoot;      // +4
		//   LPSTR       pszDisplayName;  // +8
		//   LPCSTR      lpszTitle;       // +12
		//   UINT        ulFlags;         // +16
		//   BFFCALLBACK lpfn;            // +20
		//   LPARAM      lParam;          // +24
		//   int         iImage;          // +28
		// } BROWSEINFOA;
		
		var hwndOwner = _env.MemRead32(lpbi + 0);
		var pidlRoot = _env.MemRead32(lpbi + 4);
		var pszDisplayName = _env.MemRead32(lpbi + 8);
		var lpszTitle = _env.MemRead32(lpbi + 12);
		var ulFlags = _env.MemRead32(lpbi + 16);
		
		_logger.LogInformation("[Shell32] SHBrowseForFolderA: hwndOwner=0x{HwndOwner:X8}, flags=0x{Flags:X}, title=0x{Title:X8}",
			hwndOwner, ulFlags, lpszTitle);

		// Allocate a fake PIDL - we'll use a simple 2-byte structure (size=0, terminator)
		// Real PIDLs are complex, but we just need something to pass around
		var pidlAddr = _env.SimpleAlloc(16); // Small allocation for fake PIDL
		_env.MemWrite16(pidlAddr, 0); // cb = 0 (size of this item)
		
		// Store a default installation path in the PIDL metadata area (for our use)
		// We'll use a magic marker so SHGetPathFromIDListA knows it's our fake PIDL
		_env.MemWrite32(pidlAddr + 2, 0x504C4946); // "FILP" marker (PIDL reversed)
		
		// Provide a default folder name if pszDisplayName buffer exists
		if (pszDisplayName != 0)
		{
			var defaultName = "Program Files";
			var nameBytes = System.Text.Encoding.ASCII.GetBytes(defaultName + '\0');
			for (int i = 0; i < nameBytes.Length && i < 260; i++)
			{
				_env.MemWrite8(pszDisplayName + (uint)i, nameBytes[i]);
			}
		}
		
		_logger.LogInformation("[Shell32] SHBrowseForFolderA: Returning fake PIDL at 0x{Pidl:X8}", pidlAddr);
		
		return pidlAddr;
	}

	/// <summary>
	/// Notifies the system of an event that an application has performed.
	/// void SHChangeNotify(LONG wEventId, UINT uFlags, LPCVOID dwItem1, LPCVOID dwItem2);
	/// </summary>
	[DllModuleExport(16)]
	private uint SHChangeNotify(uint wEventId, uint uFlags, uint dwItem1, uint dwItem2)
	{
		LogSHChangeNotify(wEventId, uFlags, dwItem1, dwItem2);

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
		LogSHFileOperationA(lpFileOp);

		if (lpFileOp == 0)
		{
			_logger.LogWarning("[Shell32] SHFileOperationA: NULL SHFILEOPSTRUCT pointer");
			return 0x80070057; // E_INVALIDARG
		}

		// Read SHFILEOPSTRUCT structure
		// typedef struct _SHFILEOPSTRUCTA {
		//   HWND         hwnd;              // +0
		//   UINT         wFunc;             // +4
		//   LPCSTR       pFrom;             // +8
		//   LPCSTR       pTo;               // +12
		//   FILEOP_FLAGS fFlags;            // +16
		//   BOOL         fAnyOperationsAborted; // +20
		//   LPVOID       hNameMappings;     // +24
		//   LPCSTR       lpszProgressTitle; // +28
		// } SHFILEOPSTRUCTA;
		
		var hwnd = _env.MemRead32(lpFileOp + 0);
		var wFunc = _env.MemRead32(lpFileOp + 4);
		var pFrom = _env.MemRead32(lpFileOp + 8);
		var pTo = _env.MemRead32(lpFileOp + 12);
		var fFlags = _env.MemRead32(lpFileOp + 16);
		
		_logger.LogInformation("[Shell32] SHFileOperationA: wFunc={Func}, fFlags=0x{Flags:X}, pFrom=0x{From:X8}, pTo=0x{To:X8}",
			wFunc, fFlags, pFrom, pTo);
		
		try
		{
			// Read source path(s) - can be multiple null-terminated strings
			var sourceFiles = ReadMultipleStrings(pFrom);
			var destFiles = pTo != 0 ? ReadMultipleStrings(pTo) : new List<string>();
			
			_logger.LogInformation("[Shell32] SHFileOperationA: Source files: {Sources}", string.Join(", ", sourceFiles));
			if (destFiles.Count > 0)
			{
				_logger.LogInformation("[Shell32] SHFileOperationA: Dest files: {Dests}", string.Join(", ", destFiles));
			}
			
			// Perform the operation
			const uint FO_MOVE = 1;
			const uint FO_COPY = 2;
			const uint FO_DELETE = 3;
			const uint FO_RENAME = 4;
			
			switch (wFunc)
			{
				case FO_MOVE:
					return PerformMoveOperation(sourceFiles, destFiles, fFlags);
				case FO_COPY:
					return PerformCopyOperation(sourceFiles, destFiles, fFlags);
				case FO_DELETE:
					return PerformDeleteOperation(sourceFiles, fFlags);
				case FO_RENAME:
					return PerformRenameOperation(sourceFiles, destFiles, fFlags);
				default:
					_logger.LogWarning("[Shell32] SHFileOperationA: Unknown wFunc={Func}", wFunc);
					return 0x71; // ERROR_BAD_FUNCTION
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Shell32] SHFileOperationA: Operation failed");
			return 0x02; // ERROR_FILE_NOT_FOUND (generic error)
		}
	}
	
	/// <summary>
	/// Reads multiple null-terminated strings from memory (double-null terminated list).
	/// </summary>
	private List<string> ReadMultipleStrings(uint address)
	{
		var result = new List<string>();
		if (address == 0)
			return result;
		
		uint offset = 0;
		while (true)
		{
			var str = ReadNullTerminatedString(address + offset);
			if (string.IsNullOrEmpty(str))
				break;
			
			result.Add(str);
			offset += (uint)(str.Length + 1); // +1 for null terminator
		}
		
		return result;
	}
	
	/// <summary>
	/// Reads a single null-terminated string from memory.
	/// </summary>
	private string ReadNullTerminatedString(uint address)
	{
		var bytes = new List<byte>();
		uint offset = 0;
		
		while (true)
		{
			var b = _env.MemRead8(address + offset);
			if (b == 0)
				break;
			bytes.Add(b);
			offset++;
			
			if (offset > 4096) // Safety limit
				break;
		}
		
		return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
	}
	
	private uint PerformCopyOperation(List<string> sources, List<string> dests, uint flags)
	{
		const uint FOF_NOCONFIRMMKDIR = 0x0200;
		
		try
		{
			for (int i = 0; i < sources.Count; i++)
			{
				var source = sources[i];
				var dest = i < dests.Count ? dests[i] : dests.LastOrDefault() ?? string.Empty;
				
				if (string.IsNullOrEmpty(dest))
				{
					_logger.LogWarning("[Shell32] SHFileOperationA COPY: No destination for {Source}", source);
					continue;
				}
				
				_logger.LogInformation("[Shell32] SHFileOperationA COPY: {Source} -> {Dest}", source, dest);
				
				// Use paths directly - VFS integration happens at file handle level in Kernel32
				var resolvedSource = source;
				var resolvedDest = dest;
				
				// Create destination directory if needed
				var destDir = System.IO.Path.GetDirectoryName(resolvedDest);
				if (!string.IsNullOrEmpty(destDir) && !System.IO.Directory.Exists(destDir))
				{
					if ((flags & FOF_NOCONFIRMMKDIR) != 0)
					{
						try
						{
							System.IO.Directory.CreateDirectory(destDir);
							_logger.LogInformation("[Shell32] SHFileOperationA: Created directory {Dir}", destDir);
						}
						catch (Exception ex)
						{
							_logger.LogWarning(ex, "[Shell32] SHFileOperationA: Could not create directory {Dir}", destDir);
						}
					}
				}
				
				// Perform copy
				try
				{
					if (System.IO.File.Exists(resolvedSource))
					{
						System.IO.File.Copy(resolvedSource, resolvedDest, overwrite: true);
						_logger.LogInformation("[Shell32] SHFileOperationA: Copied {Source} to {Dest}", resolvedSource, resolvedDest);
					}
					else if (System.IO.Directory.Exists(resolvedSource))
					{
						CopyDirectory(resolvedSource, resolvedDest);
						_logger.LogInformation("[Shell32] SHFileOperationA: Copied directory {Source} to {Dest}", resolvedSource, resolvedDest);
					}
					else
					{
						_logger.LogWarning("[Shell32] SHFileOperationA COPY: Source not found: {Source}", resolvedSource);
						// Don't return error immediately - continue with other files
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "[Shell32] SHFileOperationA COPY: Failed to copy {Source} to {Dest}", resolvedSource, resolvedDest);
					// Continue with remaining files
				}
			}
			
			return 0; // Success (best effort)
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Shell32] SHFileOperationA COPY failed");
			return 0x02; // ERROR_FILE_NOT_FOUND
		}
	}
	
	private uint PerformDeleteOperation(List<string> sources, uint flags)
	{
		try
		{
			foreach (var source in sources)
			{
				_logger.LogInformation("[Shell32] SHFileOperationA DELETE: {Source}", source);
				
				var resolvedSource = source;
				
				try
				{
					if (System.IO.File.Exists(resolvedSource))
					{
						System.IO.File.Delete(resolvedSource);
						_logger.LogInformation("[Shell32] SHFileOperationA: Deleted file {Source}", resolvedSource);
					}
					else if (System.IO.Directory.Exists(resolvedSource))
					{
						System.IO.Directory.Delete(resolvedSource, recursive: true);
						_logger.LogInformation("[Shell32] SHFileOperationA: Deleted directory {Source}", resolvedSource);
					}
					else
					{
						_logger.LogWarning("[Shell32] SHFileOperationA DELETE: Source not found: {Source}", resolvedSource);
						// Continue with other files even if one is not found
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "[Shell32] SHFileOperationA DELETE: Failed to delete {Source}", resolvedSource);
				}
			}
			
			return 0; // Success
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Shell32] SHFileOperationA DELETE failed");
			return 0x02; // ERROR_FILE_NOT_FOUND
		}
	}
	
	private uint PerformMoveOperation(List<string> sources, List<string> dests, uint flags)
	{
		try
		{
			for (int i = 0; i < sources.Count; i++)
			{
				var source = sources[i];
				var dest = i < dests.Count ? dests[i] : dests.LastOrDefault() ?? string.Empty;
				
				if (string.IsNullOrEmpty(dest))
				{
					_logger.LogWarning("[Shell32] SHFileOperationA MOVE: No destination for {Source}", source);
					continue;
				}
				
				_logger.LogInformation("[Shell32] SHFileOperationA MOVE: {Source} -> {Dest}", source, dest);
				
				var resolvedSource = source;
				var resolvedDest = dest;
				
				try
				{
					// Create destination directory if needed
					var destDir = System.IO.Path.GetDirectoryName(resolvedDest);
					if (!string.IsNullOrEmpty(destDir) && !System.IO.Directory.Exists(destDir))
					{
						System.IO.Directory.CreateDirectory(destDir);
					}
					
					if (System.IO.File.Exists(resolvedSource))
					{
						System.IO.File.Move(resolvedSource, resolvedDest, overwrite: true);
						_logger.LogInformation("[Shell32] SHFileOperationA: Moved {Source} to {Dest}", resolvedSource, resolvedDest);
					}
					else if (System.IO.Directory.Exists(resolvedSource))
					{
						System.IO.Directory.Move(resolvedSource, resolvedDest);
						_logger.LogInformation("[Shell32] SHFileOperationA: Moved directory {Source} to {Dest}", resolvedSource, resolvedDest);
					}
					else
					{
						_logger.LogWarning("[Shell32] SHFileOperationA MOVE: Source not found: {Source}", resolvedSource);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "[Shell32] SHFileOperationA MOVE: Failed to move {Source} to {Dest}", resolvedSource, resolvedDest);
				}
			}
			
			return 0; // Success
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Shell32] SHFileOperationA MOVE failed");
			return 0x02;
		}
	}
	
	private uint PerformRenameOperation(List<string> sources, List<string> dests, uint flags)
	{
		// Rename is similar to move
		return PerformMoveOperation(sources, dests, flags);
	}
	
	private void CopyDirectory(string sourceDir, string destDir)
	{
		// Create destination directory
		System.IO.Directory.CreateDirectory(destDir);
		
		// Copy files
		foreach (var file in System.IO.Directory.GetFiles(sourceDir))
		{
			var fileName = System.IO.Path.GetFileName(file);
			var destFile = System.IO.Path.Combine(destDir, fileName);
			System.IO.File.Copy(file, destFile, overwrite: true);
		}
		
		// Copy subdirectories recursively
		foreach (var dir in System.IO.Directory.GetDirectories(sourceDir))
		{
			var dirName = System.IO.Path.GetFileName(dir);
			var destSubDir = System.IO.Path.Combine(destDir, dirName);
			CopyDirectory(dir, destSubDir);
		}
	}

	/// <summary>
	/// Retrieves a pointer to the shell's IMalloc interface.
	/// HRESULT SHGetMalloc([out] LPMALLOC *ppMalloc);
	/// </summary>
	[DllModuleExport(8)]
	private uint SHGetMalloc(uint ppMalloc)
	{
		LogSHGetMalloc(ppMalloc);

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
		LogSHGetPathFromIDListA(pidl, pszPath.Address);

		if (pidl == 0 || pszPath.Address == 0)
		{
			_logger.LogWarning("[Shell32] SHGetPathFromIDListA: NULL pointer");
			return 0; // FALSE
		}

		// Check if this is our fake PIDL (has magic marker at offset +2)
		var marker = _env.MemRead32(pidl + 2);
		if (marker == 0x504C4946) // "FILP" marker
		{
			// This is our fake PIDL from SHBrowseForFolderA
			// Return a default installation path
			var defaultPath = @"C:\Program Files\Ignition";
			
			_logger.LogInformation("[Shell32] SHGetPathFromIDListA: Converting fake PIDL to path: {Path}", defaultPath);
			
			var pathBytes = System.Text.Encoding.ASCII.GetBytes(defaultPath + '\0');
			for (int i = 0; i < pathBytes.Length && i < 260; i++)
			{
				_env.MemWrite8(pszPath.Address + (uint)i, pathBytes[i]);
			}
			
			return 1; // TRUE - success
		}

		// If it's not our fake PIDL, it might be from SHGetSpecialFolderLocation
		// Check for special folder marker
		var specialMarker = _env.MemRead32(pidl + 2);
		if ((specialMarker & 0xFFFF0000) == 0x43530000) // "CS" marker (CSIDL)
		{
			var csidl = specialMarker & 0xFFFF;
			var specialPath = GetSpecialFolderPath((int)csidl);
			
			_logger.LogInformation("[Shell32] SHGetPathFromIDListA: Converting special folder PIDL (CSIDL={Csidl}) to path: {Path}", 
				csidl, specialPath);
			
			var pathBytes = System.Text.Encoding.ASCII.GetBytes(specialPath + '\0');
			for (int i = 0; i < pathBytes.Length && i < 260; i++)
			{
				_env.MemWrite8(pszPath.Address + (uint)i, pathBytes[i]);
			}
			
			return 1; // TRUE - success
		}
		
		_logger.LogWarning("[Shell32] SHGetPathFromIDListA: Unknown PIDL format");
		return 0; // FALSE - conversion failed
	}

	/// <summary>
	/// Retrieves the path of a special folder, identified by its CSIDL.
	/// HRESULT SHGetSpecialFolderLocation(HWND hwnd, int csidl, PIDLIST_ABSOLUTE *ppidl);
	/// </summary>
	[DllModuleExport(12)]
	private uint SHGetSpecialFolderLocation(uint hwnd, int csidl, uint ppidl)
	{
		LogSHGetSpecialFolderLocation(hwnd, csidl, ppidl);

		if (ppidl == 0)
		{
			_logger.LogWarning("[Shell32] SHGetSpecialFolderLocation: NULL ppidl pointer");
			return 0x80070057; // E_INVALIDARG
		}

		// Allocate a fake PIDL for the special folder
		var pidlAddr = _env.SimpleAlloc(16);
		_env.MemWrite16(pidlAddr, 0); // cb = 0
		
		// Store a marker indicating this is a special folder PIDL
		// Format: 0x43530000 | csidl  (CS = CSIDL marker)
		_env.MemWrite32(pidlAddr + 2, (uint)(0x43530000 | (csidl & 0xFFFF)));
		
		// Write the PIDL address to the output pointer
		_env.MemWrite32(ppidl, pidlAddr);
		
		_logger.LogInformation("[Shell32] SHGetSpecialFolderLocation: Allocated PIDL at 0x{Pidl:X8} for CSIDL {Csidl}",
			pidlAddr, csidl);

		return 0; // S_OK
	}

	/// <summary>
	/// Helper method to get the path for a special folder based on CSIDL.
	/// </summary>
	private string GetSpecialFolderPath(int csidl)
	{
		// Common CSIDL values:
		// CSIDL_DESKTOP = 0x0000
		// CSIDL_PROGRAMS = 0x0002
		// CSIDL_PERSONAL = 0x0005 (My Documents)
		// CSIDL_APPDATA = 0x001a
		// CSIDL_PROGRAM_FILES = 0x0026
		// CSIDL_COMMON_APPDATA = 0x0023
		
		return csidl switch
		{
			0x0000 => @"C:\Users\Public\Desktop",
			0x0002 => @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
			0x0005 => @"C:\Users\Public\Documents",
			0x001a => @"C:\Users\Default\AppData\Roaming",
			0x0023 => @"C:\ProgramData",
			0x0026 => @"C:\Program Files",
			_ => @"C:\Windows\System32" // Fallback
		};
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
		
		LogShellExecuteA(hwnd, operation, file, parameters, directory, nShowCmd);

		// Stub - return value > 32 indicates success
		return 33; // Success
	}

	/// <summary>
	/// Releases memory that the system allocated for use in transferring file names to the application.
	/// void DragFinish(HDROP hDrop);
	/// </summary>
	[DllModuleExport(4)]
	private uint DragFinish(uint hDrop)
	{
		LogDragFinish(hDrop);
		return 0; // void function
	}

	/// <summary>
	/// Retrieves the names of dropped files that result from a successful drag-and-drop operation.
	/// UINT DragQueryFileA(HDROP hDrop, UINT iFile, LPSTR lpszFile, UINT cch);
	/// </summary>
	[DllModuleExport(16)]
	private uint DragQueryFileA(uint hDrop, uint iFile, uint lpszFile, uint cch)
	{
		LogDragQueryFileA(hDrop, iFile, lpszFile, cch);

		// Stub - return 0 (no files dropped)
		return 0;
	}

	[DllModuleExport(1)]
	private uint ShellExecuteExA(uint lpExecInfo)
	{
		LogShellExecuteExA(lpExecInfo);
		
		// SHELLEXECUTEINFO structure
		// Read fields from the structure if needed
		// For now, just return success
		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves a handle to an icon from the specified executable file, DLL, or icon file.
	/// HICON ExtractIconA(HINSTANCE hInst, LPCSTR pszExeFileName, UINT nIconIndex);
	/// </summary>
	[DllModuleExport(12)]
	private uint ExtractIconA(uint hInst, in LpcStr pszExeFileName, uint nIconIndex)
	{
		var fileName = pszExeFileName.ToString() ?? string.Empty;
		LogExtractIconA(hInst, fileName, nIconIndex);
		
		// Stub - return NULL (no icon)
		return 0;
	}

	/// <summary>
	/// Retrieves information about an object in the file system, such as a file, folder, directory, or drive root.
	/// DWORD_PTR SHGetFileInfoA(LPCSTR pszPath, DWORD dwFileAttributes, SHFILEINFOA *psfi, UINT cbFileInfo, UINT uFlags);
	/// </summary>
	[DllModuleExport(20)]
	private uint SHGetFileInfoA(in LpcStr pszPath, uint dwFileAttributes, uint psfi, uint cbFileInfo, uint uFlags)
	{
		var path = pszPath.ToString() ?? string.Empty;
		LogSHGetFileInfoA(path, dwFileAttributes, uFlags);
		
		// Stub - return 0 (failure)
		return 0;
	}

	/// <summary>
	/// Displays a ShellAbout dialog box.
	/// INT ShellAboutA(HWND hWnd, LPCSTR szApp, LPCSTR szOtherStuff, HICON hIcon);
	/// </summary>
	[DllModuleExport(16)]
	private uint ShellAboutA(uint hWnd, in LpcStr szApp, in LpcStr szOtherStuff, uint hIcon)
	{
		var app = szApp.ToString() ?? string.Empty;
		var otherStuff = szOtherStuff.ToString() ?? string.Empty;
		LogShellAboutA(hWnd, app, otherStuff, hIcon);
		
		// Stub - return TRUE (success)
		return 1;
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] SHBrowseForFolderA(lpbi=0x{Lpbi:X8})")]
	partial void LogSHBrowseForFolderA(uint lpbi);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] SHChangeNotify(wEventId=0x{WEventId:X}, uFlags=0x{UFlags:X}, dwItem1=0x{DwItem1:X8}, dwItem2=0x{DwItem2:X8})")]
	partial void LogSHChangeNotify(uint wEventId, uint uFlags, uint dwItem1, uint dwItem2);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] SHFileOperationA(lpFileOp=0x{LpFileOp:X8})")]
	partial void LogSHFileOperationA(uint lpFileOp);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] SHGetMalloc(ppMalloc=0x{PpMalloc:X8})")]
	partial void LogSHGetMalloc(uint ppMalloc);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] SHGetPathFromIDListA(pidl=0x{Pidl:X8}, pszPath=0x{PszPath:X8})")]
	partial void LogSHGetPathFromIDListA(uint pidl, uint pszPath);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] SHGetSpecialFolderLocation(hwnd=0x{Hwnd:X8}, csidl={Csidl}, ppidl=0x{Ppidl:X8})")]
	partial void LogSHGetSpecialFolderLocation(uint hwnd, int csidl, uint ppidl);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] ShellExecuteA(hwnd=0x{Hwnd:X8}, lpOperation=\"{Operation}\", lpFile=\"{File}\", lpParameters=\"{Parameters}\", lpDirectory=\"{Directory}\", nShowCmd={NShowCmd})")]
	partial void LogShellExecuteA(uint hwnd, string operation, string file, string parameters, string directory, int nShowCmd);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] DragFinish(hDrop=0x{HDrop:X8})")]
	partial void LogDragFinish(uint hDrop);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] DragQueryFileA(hDrop=0x{HDrop:X8}, iFile={IFile}, lpszFile=0x{LpszFile:X8}, cch={Cch})")]
	partial void LogDragQueryFileA(uint hDrop, uint iFile, uint lpszFile, uint cch);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] ShellExecuteExA(lpExecInfo=0x{LpExecInfo:X8})")]
	partial void LogShellExecuteExA(uint lpExecInfo);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] ExtractIconA(hInst=0x{HInst:X8}, pszExeFileName=\"{FileName}\", nIconIndex={NIconIndex})")]
	partial void LogExtractIconA(uint hInst, string fileName, uint nIconIndex);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] SHGetFileInfoA(pszPath=\"{Path}\", dwFileAttributes=0x{DwFileAttributes:X}, uFlags=0x{UFlags:X})")]
	partial void LogSHGetFileInfoA(string path, uint dwFileAttributes, uint uFlags);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Shell32] ShellAboutA(hWnd=0x{HWnd:X8}, szApp=\"{App}\", szOtherStuff=\"{OtherStuff}\", hIcon=0x{HIcon:X8})")]
	partial void LogShellAboutA(uint hWnd, string app, string otherStuff, uint hIcon);
}
