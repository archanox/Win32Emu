using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class WinMmModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public WinMmModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "WINMM.DLL";

		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private uint _timerPeriod;

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "TIMEGETTIME":
					returnValue = TimeGetTime();
					return true;

				case "TIMEBEGINPERIOD":
					returnValue = TimeBeginPeriod(a.UInt32(0));
					return true;

				case "TIMEENDPERIOD":
					returnValue = TimeEndPeriod(a.UInt32(0));
					return true;

				case "TIMEKILLEVENT":
					returnValue = TimeKillEvent(a.UInt32(0));
					return true;

				default:
					_logger.LogInformation($"[WinMM] Unimplemented export: {export}");
					return false;
			}
		}

		private unsafe uint TimeGetTime()
		{
			// Return time in milliseconds since start
			var time = (uint)_stopwatch.ElapsedMilliseconds;
			return time;
		}

		private unsafe uint TimeBeginPeriod(uint uPeriod)
		{
			_logger.LogInformation($"[WinMM] timeBeginPeriod({uPeriod})");
			_timerPeriod = uPeriod;
			return 0; // TIMERR_NOERROR
		}

		private unsafe uint TimeEndPeriod(uint uPeriod)
		{
			_logger.LogInformation($"[WinMM] timeEndPeriod({uPeriod})");
			_timerPeriod = 0;
			return 0; // TIMERR_NOERROR
		}

		private unsafe uint TimeKillEvent(uint uTimerId)
		{
			_logger.LogInformation($"[WinMM] timeKillEvent({uTimerId})");
			return 0; // TIMERR_NOERROR
		}

		public Dictionary<string, uint> GetExportOrdinals()
		{
			// Export ordinals for WinMM - alphabetically ordered
			var exports = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
			{
				{ "TIMEBEGINPERIOD", 1 },
				{ "TIMEENDPERIOD", 2 },
				{ "TIMEGETTIME", 3 },
				{ "TIMEKILLEVENT", 4 }
			};
			return exports;
		}
	}
}