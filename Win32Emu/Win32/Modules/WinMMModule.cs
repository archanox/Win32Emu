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

				case "TIMESETEVENT":
					returnValue = TimeSetEvent(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				case "JOYGETPOSEX":
					returnValue = JoyGetPosEx(a.UInt32(0), a.UInt32(1));
					return true;

				case "JOYGETDEVCAPSA":
					returnValue = JoyGetDevCapsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MCISENDSTRINGA":
					returnValue = MciSendStringA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
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

		private unsafe uint TimeSetEvent(uint uDelay, uint uResolution, uint lpTimeProc, uint dwUser, uint fuEvent)
		{
			// TimeSetEvent sets a timer event
			// Returns a timer identifier or 0 if it failed
			_logger.LogInformation($"[WinMM] timeSetEvent(delay={uDelay}, resolution={uResolution}, callback=0x{lpTimeProc:X8})");
			
			// Return a synthetic timer ID
			return 0x1000 + uDelay; // Simple unique ID based on delay
		}

		private unsafe uint JoyGetPosEx(uint uJoyID, uint pji)
		{
			// JoyGetPosEx queries the position and button status of a joystick
			_logger.LogInformation($"[WinMM] joyGetPosEx(uJoyID={uJoyID}, pji=0x{pji:X8})");
			
			if (pji == 0)
			{
				return 165; // JOYERR_PARMS
			}

			// Return JOYERR_UNPLUGGED to indicate no joystick is connected
			return 167; // JOYERR_UNPLUGGED
		}

		private unsafe uint JoyGetDevCapsA(uint uJoyID, uint pjc, uint cbjc)
		{
			// JoyGetDevCapsA queries the capabilities of a joystick
			_logger.LogInformation($"[WinMM] joyGetDevCapsA(uJoyID={uJoyID}, pjc=0x{pjc:X8}, cbjc={cbjc})");
			
			if (pjc == 0)
			{
				return 165; // JOYERR_PARMS
			}

			// Return JOYERR_UNPLUGGED to indicate no joystick is connected
			return 167; // JOYERR_UNPLUGGED
		}

		private unsafe uint MciSendStringA(uint lpszCommand, uint lpszReturnString, uint cchReturn, uint hwndCallback)
		{
			// MciSendStringA sends a command string to an MCI device
			var command = lpszCommand != 0 ? _env.ReadAnsiString(lpszCommand) : "";
			_logger.LogInformation($"[WinMM] mciSendStringA: \"{command}\"");
			
			// For now, just return success
			return 0; // MMSYSERR_NOERROR
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