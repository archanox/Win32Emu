using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
namespace Win32Emu.Win32.Modules;

/// <summary>
/// KERNELBASE.DLL module - used for testing forwarded exports.
/// In real Windows, many KERNEL32.DLL functions forward to KERNELBASE.DLL.
/// </summary>
public class KernelBaseModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public KernelBaseModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}
	public string Name => "KERNELBASE.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;

		switch (export.ToUpperInvariant())
		{
			case "GETVERSIONEX":
				returnValue = GetVersionEx();
				return true;

			default:
				_logger.LogInformation($"[KernelBase] Unimplemented export: {export}");
				return false;
		}
	}

	[DllModuleExport(1)]
	private uint GetVersionEx()
	{
		// Simplified implementation for testing
		_logger.LogInformation("[KernelBase] GetVersionEx called (forwarded from KERNEL32)");
		return 1; // TRUE
	}

	public Dictionary<string, uint> GetExportOrdinals()
	{
		// Auto-generated from [DllModuleExport] attributes
		return DllModuleExportInfo.GetAllExports("KERNELBASE.DLL");
	}
}
