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

		// Read the path string
		var pathBytes = new System.Collections.Generic.List<byte>();
		uint offset = 0;
		byte b;
		while ((b = _env.MemRead8(pszPath + offset)) != 0)
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

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Shlwapi] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Shlwapi] PathRemoveFileSpecA(\"{Path}\") -> {Result}")]
	partial void LogPathRemoveFileSpecA(string path, bool result);
}
