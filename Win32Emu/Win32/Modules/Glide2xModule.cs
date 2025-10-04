using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class Glide2XModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public Glide2XModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "GLIDE2X.DLL";

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			//var a = new StackArgs(cpu, memory);

			_logger.LogInformation($"[Glide2x] Unimplemented export: {export}");
			return false;
		}

		public Dictionary<string, uint> GetExportOrdinals()
		{
			// Export ordinals for Glide2x - currently no implemented exports
			var exports = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
			{
				{"_ConvertAndDownloadRle@64", 1}
			};
			return exports;
		}
	}
}