using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// IMM32.DLL module - provides Input Method Manager functionality.
/// </summary>
public class Imm32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Imm32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "IMM32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "IMMGETDEFAULTIMEWND":
				returnValue = ImmGetDefaultIMEWnd(a.UInt32(0));
				return true;

			default:
				_logger.LogInformation("[Imm32] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Retrieves the default window handle to the IME class.
	/// HWND ImmGetDefaultIMEWnd(
	///   [in] HWND hWnd
	/// );
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint ImmGetDefaultIMEWnd(uint hWnd)
	{
		_logger.LogInformation("[Imm32] ImmGetDefaultIMEWnd(hWnd=0x{HWnd:X8})", hWnd);
		// Return NULL (0) - no IME window is available in this emulation environment
		return 0;
	}
}
