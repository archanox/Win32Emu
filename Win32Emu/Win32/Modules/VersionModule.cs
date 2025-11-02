using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// VERSION.DLL module - provides version information functions.
/// </summary>
public partial class VersionModule : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public VersionModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "VERSION.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "GETFILEVERSIONINFOSIZEA":
				returnValue = GetFileVersionInfoSizeA(a.LpcStr(0), a.UInt32(1));
				return true;

			case "GETFILEVERSIONINFOA":
				returnValue = GetFileVersionInfoA(a.LpcStr(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "VERQUERYVALUEA":
				returnValue = VerQueryValueA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3));
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Determines the size of the file version information structure.
	/// DWORD GetFileVersionInfoSizeA(
	///   LPCSTR lptstrFilename,
	///   LPDWORD lpdwHandle
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint GetFileVersionInfoSizeA(in LpcStr lptstrFilename, uint lpdwHandle)
	{
		var filename = lptstrFilename.ToString() ?? string.Empty;
		LogGetFileVersionInfoSizeA(filename, lpdwHandle);

		// Clear the handle output parameter
		if (lpdwHandle != 0)
		{
			_env.MemWrite32(lpdwHandle, 0);
		}

		// Return a dummy size for version info (stub)
		return 1024; // Return a reasonable buffer size
	}

	/// <summary>
	/// Retrieves version information for the specified file.
	/// BOOL GetFileVersionInfoA(
	///   LPCSTR lptstrFilename,
	///   DWORD  dwHandle,
	///   DWORD  dwLen,
	///   LPVOID lpData
	/// );
	/// </summary>
	[DllModuleExport(16)]
	private uint GetFileVersionInfoA(in LpcStr lptstrFilename, uint dwHandle, uint dwLen, uint lpData)
	{
		var filename = lptstrFilename.ToString() ?? string.Empty;
		LogGetFileVersionInfoA(filename, dwHandle, dwLen, lpData);

		// For a stub implementation, just zero out the buffer
		if (lpData != 0 && dwLen > 0)
		{
			for (uint i = 0; i < dwLen; i++)
			{
				_env.MemWrite8(lpData + i, 0);
			}
		}

		return 1; // TRUE - success
	}

	/// <summary>
	/// Retrieves specified version information from the specified version-information resource.
	/// BOOL VerQueryValueA(
	///   LPCVOID pBlock,
	///   LPCSTR  lpSubBlock,
	///   LPVOID  *lplpBuffer,
	///   PUINT   puLen
	/// );
	/// </summary>
	[DllModuleExport(16)]
	private uint VerQueryValueA(uint pBlock, in LpcStr lpSubBlock, uint lplpBuffer, uint puLen)
	{
		var subBlock = lpSubBlock.ToString() ?? string.Empty;
		LogVerQueryValueA(pBlock, subBlock, lplpBuffer, puLen);

		// Return NULL pointer and 0 length (not found)
		if (lplpBuffer != 0)
		{
			_env.MemWrite32(lplpBuffer, 0);
		}
		if (puLen != 0)
		{
			_env.MemWrite32(puLen, 0);
		}

		return 0; // FALSE - not found (stub)
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Version] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Version] GetFileVersionInfoSizeA(filename=\"{Filename}\", lpdwHandle=0x{LpdwHandle:X8})")]
	partial void LogGetFileVersionInfoSizeA(string filename, uint lpdwHandle);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Version] GetFileVersionInfoA(filename=\"{Filename}\", dwHandle=0x{DwHandle:X}, dwLen={DwLen}, lpData=0x{LpData:X8})")]
	partial void LogGetFileVersionInfoA(string filename, uint dwHandle, uint dwLen, uint lpData);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Version] VerQueryValueA(pBlock=0x{PBlock:X8}, lpSubBlock=\"{SubBlock}\", lplpBuffer=0x{LplpBuffer:X8}, puLen=0x{PuLen:X8})")]
	partial void LogVerQueryValueA(uint pBlock, string subBlock, uint lplpBuffer, uint puLen);
}
