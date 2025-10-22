using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// DSETUP.DLL module - provides DirectX setup functions.
/// </summary>
public class DsetupModule : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public DsetupModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "DSETUP.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "DIRECTXSETUPA":
				returnValue = DirectXSetupA(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
				return true;

			default:
				_logger.LogInformation("[Dsetup] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Installs DirectX.
	/// INT DirectXSetupA(HWND hWnd, LPSTR lpszRootPath, DWORD dwFlags);
	/// </summary>
	[DllModuleExport(12)]
	private uint DirectXSetupA(uint hWnd, in LpcStr lpszRootPath, uint dwFlags)
	{
		var rootPath = lpszRootPath.ToString() ?? string.Empty;
		_logger.LogInformation("[Dsetup] DirectXSetupA(hWnd=0x{HWnd:X8}, lpszRootPath=\"{RootPath}\", dwFlags=0x{DwFlags:X})",
			hWnd, rootPath, dwFlags);

		// Stub - return success (DirectX already installed)
		const int DSETUPERR_SUCCESS = 0;
		return (uint)DSETUPERR_SUCCESS;
	}
}
