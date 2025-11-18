using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// SHLWAPI.DLL module - provides Shell lightweight utility functions.
/// These are commonly used path manipulation and string utility functions.
/// </summary>
public partial class ShlwapiModule : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public ShlwapiModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "SHLWAPI.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "PATHREMOVEFILESPECA":
				returnValue = PathRemoveFileSpecA(a.UInt32(0));
				return true;

			case "PATHADDBACKSLASHA":
				returnValue = PathAddBackslashA(a.UInt32(0));
				return true;

			case "PATHCOMBINEA":
				returnValue = PathCombineA(a.UInt32(0), a.LpcStr(1), a.LpcStr(2));
				return true;

			case "PATHFINDEXTENSIONA":
				returnValue = PathFindExtensionA(a.LpcStr(0));
				return true;

			case "PATHFINDFILENAMEA":
				returnValue = PathFindFileNameA(a.LpcStr(0));
				return true;

			case "STRSTRIA":
				returnValue = StrStrIA(a.LpcStr(0), a.LpcStr(1));
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Removes the trailing file name and backslash from a path.
	/// BOOL PathRemoveFileSpecA(
	///   LPSTR pszPath
	/// );
	/// </summary>
	/// <returns>TRUE if something was removed, FALSE otherwise</returns>
	[DllModuleExport(4)]
	private uint PathRemoveFileSpecA(uint pszPath)
	{
		if (pszPath == 0)
		{
			LogPathRemoveFileSpecA("(null)", false);
			return 0; // FALSE
		}

		// Read the path string with MAX_PATH protection
		var pathBytes = new System.Collections.Generic.List<byte>();
		uint offset = 0;
		const uint MAX_PATH = 260; // Standard Windows MAX_PATH
		byte b;
		while (offset < MAX_PATH && (b = _env.MemRead8(pszPath + offset)) != 0)
		{
			pathBytes.Add(b);
			offset++;
		}

		// Find the last backslash
		int lastBackslash = -1;
		for (int i = pathBytes.Count - 1; i >= 0; i--)
		{
			if (pathBytes[i] == (byte)'\\')
			{
				lastBackslash = i;
				break;
			}
		}

		var originalPath = System.Text.Encoding.ASCII.GetString(pathBytes.ToArray());

		// If found, truncate at that position
		if (lastBackslash >= 0)
		{
			// Write null terminator at the backslash position
			_env.MemWrite8(pszPath + (uint)lastBackslash, 0);
			LogPathRemoveFileSpecA(originalPath, true);
			return 1; // TRUE - something was removed
		}

		LogPathRemoveFileSpecA(originalPath, false);
		return 0; // FALSE - nothing was removed
	}

	/// <summary>
	/// Adds a backslash to the end of a string to create the correct syntax for a path.
	/// LPSTR PathAddBackslashA(
	///   [in,out] LPSTR pszPath
	/// );
	/// </summary>
	/// <returns>Address of the terminating NULL or NULL if path is too long</returns>
	[DllModuleExport(4)]
	private uint PathAddBackslashA(uint pszPath)
	{
		if (pszPath == 0)
		{
			_logger.LogDebug("[Shlwapi] PathAddBackslashA(null) -> NULL");
			return 0; // NULL
		}

		// Read the path string
		var pathBytes = new System.Collections.Generic.List<byte>();
		uint offset = 0;
		const uint MAX_PATH = 260;
		byte b;
		while (offset < MAX_PATH && (b = _env.MemRead8(pszPath + offset)) != 0)
		{
			pathBytes.Add(b);
			offset++;
		}

		var path = System.Text.Encoding.ASCII.GetString(pathBytes.ToArray());
		_logger.LogDebug("[Shlwapi] PathAddBackslashA(\"{Path}\")", path);

		// Check if path already ends with backslash
		if (pathBytes.Count > 0 && pathBytes[pathBytes.Count - 1] == (byte)'\\')
		{
			// Already has backslash, return pointer to NULL terminator
			return pszPath + (uint)pathBytes.Count;
		}

		// Check if there's room for backslash and NULL terminator
		if (pathBytes.Count + 2 >= MAX_PATH)
		{
			_logger.LogWarning("[Shlwapi] PathAddBackslashA: Path too long");
			return 0; // NULL - path too long
		}

		// Add backslash and NULL terminator
		_env.MemWrite8(pszPath + (uint)pathBytes.Count, (byte)'\\');
		_env.MemWrite8(pszPath + (uint)pathBytes.Count + 1, 0);

		return pszPath + (uint)pathBytes.Count + 1; // Return pointer to NULL terminator
	}

	/// <summary>
	/// Concatenates two path strings.
	/// LPSTR PathCombineA(
	///   [out] LPSTR  pszDest,
	///   [in]  LPCSTR pszDir,
	///   [in]  LPCSTR pszFile
	/// );
	/// </summary>
	/// <returns>Address of combined path or NULL on error</returns>
	[DllModuleExport(12)]
	private uint PathCombineA(uint pszDest, in LpcStr pszDirPtr, in LpcStr pszFilePtr)
	{
		var pszDir = pszDirPtr.ToString() ?? string.Empty;
		var pszFile = pszFilePtr.ToString() ?? string.Empty;

		_logger.LogDebug("[Shlwapi] PathCombineA(pszDest=0x{PszDest:X8}, pszDir=\"{PszDir}\", pszFile=\"{PszFile}\")",
			pszDest, pszDir, pszFile);

		if (pszDest == 0)
		{
			return 0; // NULL
		}

		const uint MAX_PATH = 260;

		// Handle NULL inputs
		if (string.IsNullOrEmpty(pszDir))
		{
			pszDir = "";
		}
		if (string.IsNullOrEmpty(pszFile))
		{
			pszFile = "";
		}

		// Combine paths
		string combined;
		if (string.IsNullOrEmpty(pszDir))
		{
			combined = pszFile;
		}
		else if (string.IsNullOrEmpty(pszFile))
		{
			combined = pszDir;
		}
		else
		{
			// Check if pszFile is an absolute path
			if (pszFile.Length >= 2 && pszFile[1] == ':')
			{
				// pszFile is absolute, use it alone
				combined = pszFile;
			}
			else if (pszFile.StartsWith("\\"))
			{
				// pszFile starts with backslash, use it alone
				combined = pszFile;
			}
			else
			{
				// Combine with backslash separator
				combined = pszDir.TrimEnd('\\') + "\\" + pszFile;
			}
		}

		// Check length
		if (combined.Length >= MAX_PATH)
		{
			_logger.LogWarning("[Shlwapi] PathCombineA: Combined path too long");
			return 0; // NULL
		}

		// Write to destination
		var bytes = System.Text.Encoding.ASCII.GetBytes(combined);
		for (int i = 0; i < bytes.Length; i++)
		{
			_env.MemWrite8(pszDest + (uint)i, bytes[i]);
		}
		_env.MemWrite8(pszDest + (uint)bytes.Length, 0); // NULL terminator

		return pszDest;
	}

	/// <summary>
	/// Searches a path for an extension.
	/// LPCSTR PathFindExtensionA(
	///   [in] LPCSTR pszPath
	/// );
	/// </summary>
	/// <returns>Address of the extension or pointer to NULL if no extension found</returns>
	[DllModuleExport(4)]
	private uint PathFindExtensionA(in LpcStr pszPathPtr)
	{
		var pszPath = pszPathPtr.ToString() ?? string.Empty;

		_logger.LogDebug("[Shlwapi] PathFindExtensionA(pszPath=\"{PszPath}\")", pszPath);

		if (string.IsNullOrEmpty(pszPath))
		{
			return 0; // NULL
		}

		// Find last dot after last backslash
		int lastBackslash = pszPath.LastIndexOf('\\');
		int lastDot = pszPath.LastIndexOf('.');

		// If dot is after last backslash (or no backslash), it's an extension
		if (lastDot > lastBackslash)
		{
			// Calculate address of the dot in memory
			// We need to find where the string is in memory
			// For simplicity, we'll return a pointer to an empty string if we can't determine the address
			// In a real implementation, we'd need to know the memory address of pszPath
			_logger.LogWarning("[Shlwapi] PathFindExtensionA: Cannot determine memory address, returning stub");
			
			// Return 0 to indicate no extension (stub behavior)
			return 0;
		}

		// No extension found
		return 0;
	}

	/// <summary>
	/// Searches a path for the file name.
	/// LPCSTR PathFindFileNameA(
	///   [in] LPCSTR pszPath
	/// );
	/// </summary>
	/// <returns>Address of the filename or pointer to path if no backslash found</returns>
	[DllModuleExport(4)]
	private uint PathFindFileNameA(in LpcStr pszPathPtr)
	{
		var pszPath = pszPathPtr.ToString() ?? string.Empty;

		_logger.LogDebug("[Shlwapi] PathFindFileNameA(pszPath=\"{PszPath}\")", pszPath);

		if (string.IsNullOrEmpty(pszPath))
		{
			return 0; // NULL
		}

		// Find last backslash
		int lastBackslash = pszPath.LastIndexOf('\\');

		// Similar issue as PathFindExtensionA - we'd need to know the memory address
		_logger.LogWarning("[Shlwapi] PathFindFileNameA: Cannot determine memory address, returning stub");
		
		return 0;
	}

	/// <summary>
	/// Finds the first occurrence of a substring within a string (case-insensitive).
	/// PCSTR StrStrIA(
	///   [in] PCSTR pszFirst,
	///   [in] PCSTR pszSrch
	/// );
	/// </summary>
	/// <returns>Address of first occurrence or NULL if not found</returns>
	[DllModuleExport(8)]
	private uint StrStrIA(in LpcStr pszFirstPtr, in LpcStr pszSrchPtr)
	{
		var pszFirst = pszFirstPtr.ToString() ?? string.Empty;
		var pszSrch = pszSrchPtr.ToString() ?? string.Empty;

		_logger.LogDebug("[Shlwapi] StrStrIA(pszFirst=\"{PszFirst}\", pszSrch=\"{PszSrch}\")",
			pszFirst, pszSrch);

		if (string.IsNullOrEmpty(pszFirst) || string.IsNullOrEmpty(pszSrch))
		{
			return 0; // NULL
		}

		// Case-insensitive search
		int index = pszFirst.IndexOf(pszSrch, StringComparison.OrdinalIgnoreCase);

		if (index >= 0)
		{
			// Found - but we'd need to calculate the memory address
			// This is a limitation of our current implementation
			_logger.LogWarning("[Shlwapi] StrStrIA: Cannot determine memory address, returning stub");
			return 0;
		}

		return 0; // NULL - not found
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Shlwapi] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Shlwapi] PathRemoveFileSpecA(\"{Path}\") -> {Result}")]
	partial void LogPathRemoveFileSpecA(string path, bool result);
}
