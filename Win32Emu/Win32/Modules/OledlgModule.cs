using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// OLEDLG.DLL module - provides OLE user interface dialogs.
/// </summary>
public partial class OledlgModule : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public OledlgModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "OLEDLG.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "ORDINAL_8":
				returnValue = Ordinal_8(a.UInt32(0));
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Ordinal 8 - OleUIAddVerbMenuA or similar function.
	/// BOOL OleUIAddVerbMenuA(LPOLEOBJECT lpOleObj, LPCSTR lpszShortType, HMENU hMenu, UINT uPos, UINT uIDVerbMin, UINT uIDVerbMax, BOOL bAddConvert, UINT idConvert, HMENU *lphMenu);
	/// </summary>
	[DllModuleExport(4)]
	private uint Ordinal_8(uint lpOleObj)
	{
		LogOrdinal_8(lpOleObj);

		// Stub: Return FALSE (no verbs added)
		return 0;
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Oledlg] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Information, Message = "[Oledlg] Ordinal_8(lpOleObj=0x{LpOleObj:X8})")]
	partial void LogOrdinal_8(uint lpOleObj);
}
