using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// OLEPRO32.DLL module - provides OLE property support.
/// </summary>
public partial class Olepro32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Olepro32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "OLEPRO32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "ORDINAL_253":
				returnValue = Ordinal_253(a.UInt32(0), a.UInt32(1));
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Ordinal 253 - Unknown function, possibly related to OLE property management.
	/// </summary>
	[DllModuleExport(8)]
	private uint Ordinal_253(uint param1, uint param2)
	{
		LogOrdinal_253(param1, param2);

		// Stub: Return success
		return 0x00000000; // S_OK
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Olepro32] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Olepro32] Ordinal_253(param1=0x{Param1:X8}, param2=0x{Param2:X8})")]
	partial void LogOrdinal_253(uint param1, uint param2);
}
