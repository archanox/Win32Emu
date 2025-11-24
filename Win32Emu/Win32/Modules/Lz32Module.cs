using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// LZ32.DLL module - provides LZ compression/decompression functions.
/// </summary>
public partial class Lz32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Lz32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "LZ32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "LZOPENFILEA":
				returnValue = (uint)LZOpenFileA(a.LpcStr(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "LZCOPY":
				returnValue = (uint)LZCopy(a.Int32(0), a.Int32(1));
				return true;

			case "LZCLOSE":
				LZClose(a.Int32(0));
				returnValue = 0;
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Opens a file for compressed or uncompressed reading.
	/// LZFILE LZOpenFileA(
	///   [in]  LPSTR  lpFileName,
	///   [out] LPOFSTRUCT lpReOpenBuf,
	///   [in]  WORD   wStyle
	/// );
	/// Returns: Handle to the file if successful, or an error value less than zero otherwise.
	/// Error values: LZERROR_BADINHANDLE (-1), LZERROR_BADOUTHANDLE (-2), LZERROR_READ (-3), 
	/// LZERROR_WRITE (-4), LZERROR_GLOBALLOC (-5), LZERROR_GLOBLOCK (-6), LZERROR_BADVALUE (-7),
	/// LZERROR_UNKNOWNALG (-8)
	/// </summary>
	[DllModuleExport(12)]
	private int LZOpenFileA(in LpcStr lpFileName, uint lpReOpenBuf, uint wStyle)
	{
		var fileName = lpFileName.ToString() ?? string.Empty;
		LogLZOpenFileA(fileName, lpReOpenBuf, wStyle);

		// Stub implementation: return error (file not found)
		const int LZERROR_BADINHANDLE = -1;
		return LZERROR_BADINHANDLE;
	}

	/// <summary>
	/// Copies a source file to a destination file, decompressing if necessary.
	/// LONG LZCopy(
	///   [in] INT hfSource,
	///   [in] INT hfDest
	/// );
	/// Returns: The size of the destination file if successful, or an error value less than zero otherwise.
	/// </summary>
	[DllModuleExport(8)]
	private int LZCopy(int hfSource, int hfDest)
	{
		LogLZCopy(hfSource, hfDest);

		// Stub implementation: return error
		const int LZERROR_READ = -3;
		return LZERROR_READ;
	}

	/// <summary>
	/// Closes a file that was opened with LZOpenFile.
	/// void LZClose(
	///   [in] INT hFile
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private void LZClose(int hFile)
	{
		LogLZClose(hFile);
		// Stub implementation: no-op
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[LZ32] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Information, Message = "[LZ32] LZOpenFileA(lpFileName=\"{FileName}\", lpReOpenBuf=0x{LpReOpenBuf:X8}, wStyle=0x{WStyle:X})")]
	partial void LogLZOpenFileA(string fileName, uint lpReOpenBuf, uint wStyle);

	[LoggerMessage(Level = LogLevel.Information, Message = "[LZ32] LZCopy(hfSource={HfSource}, hfDest={HfDest})")]
	partial void LogLZCopy(int hfSource, int hfDest);

	[LoggerMessage(Level = LogLevel.Information, Message = "[LZ32] LZClose(hFile={HFile})")]
	partial void LogLZClose(int hFile);
}
