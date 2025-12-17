using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Threading;
using static Win32Emu.Win32.NativeTypes;

namespace Win32Emu.Win32.Modules
{
	internal class WinMmModule : IWin32ModuleAsync
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;
		private ICpu? _cpu;
		private VirtualMemory? _memory;

		// Constants for async callback execution
		private const int INFINITE_LOOP_CHECK_INTERVAL = 100000; // Check for infinite loops every 100K steps
		private const int STUCK_COUNTER_THRESHOLD = 3; // Number of consecutive checks at same EIP to consider it stuck
		private const int CANCELLATION_CHECK_INTERVAL = 1000; // Check cancellation token every 1K steps
		private const uint MINIMUM_VALID_EIP = 0x00001000; // Minimum valid instruction pointer (4KB)

		// MIDI header structure constants
		private const uint MIDIHDR_FLAGS_OFFSET = 16; // Offset of dwFlags in MIDIHDR structure

		// MIDI stream handle allocation base (chosen to avoid conflicts with other handle types)
		private const uint MIDI_STREAM_HANDLE_BASE = 0x70000000;

		// Timer API result codes
		private enum TimerResult : uint
		{
			TIMERR_NOERROR = 0,   // No error
			TIMERR_STRUCT = 96,   // Invalid structure size
		}

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

		// Timer tracking for timeSetEvent implementation
		private readonly ConcurrentDictionary<uint, MultimediaTimerInfo> _multimediaTimers = new();
		private uint _nextMultimediaTimerId = 0x1000;

		// Multimedia timer information structure
		private record struct MultimediaTimerInfo(
			uint TimerId,
			uint Delay,
			uint Resolution,
			uint TimeProc,
			uint DwUser,
			uint FuEvent
		);

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			_cpu = cpu;
			_memory = memory;
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "TIMEGETTIME":
					returnValue = TimeGetTime();
					return true;

				case "TIMEGETDEVCAPS":
					returnValue = TimeGetDevCaps(a.UInt32(0), a.UInt32(1));
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

				case "JOYGETDEVCAPSW":
					returnValue = JoyGetDevCapsW(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "JOYGETPOS":
					returnValue = JoyGetPos(a.UInt32(0), a.UInt32(1));
					return true;

				case "JOYGETTHRESHOLD":
					returnValue = JoyGetThreshold(a.UInt32(0), a.UInt32(1));
					return true;

				case "JOYRELEASECAPTURE":
					returnValue = JoyReleaseCapture(a.UInt32(0));
					return true;

				case "JOYSETCAPTURE":
					returnValue = JoySetCapture(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "JOYSETTHRESHOLD":
					returnValue = JoySetThreshold(a.UInt32(0), a.UInt32(1));
					return true;

				case "MCISENDSTRINGA":
					returnValue = MciSendStringA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "MCISENDCOMMANDA":
					returnValue = MciSendCommandA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "MCIGETDEVICEIDA":
					returnValue = MciGetDeviceIDA(a.LpcStr(0));
					return true;

				case "MCISENDCOMMANDW":
					returnValue = MciSendCommandW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "MMIOOPENA":
					returnValue = MmioOpenA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MMIOCLOSE":
					returnValue = MmioClose(a.UInt32(0), a.UInt32(1));
					return true;

				case "MMIOREAD":
					returnValue = MmioRead(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MMIOSEEK":
					returnValue = MmioSeek(a.UInt32(0), a.Int32(1), a.Int32(2));
					return true;

				case "MMIOGETINFO":
					returnValue = MmioGetInfo(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MMIOSETINFO":
					returnValue = MmioSetInfo(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MMIODESCEND":
					returnValue = MmioDescend(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "MMIOASCEND":
					returnValue = MmioAscend(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MMIOADVANCE":
					returnValue = MmioAdvance(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MMIOCREATECHUNK":
					returnValue = MmioCreateChunk(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MMIOWRITE":
					returnValue = MmioWrite(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MMIOINSTALLIOPROCA":
					returnValue = MmioInstallIOProcA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MMIOSETBUFFER":
					returnValue = MmioSetBuffer(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "MIXEROPEN":
					returnValue = MixerOpen(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				case "MIXERCLOSE":
					returnValue = MixerClose(a.UInt32(0));
					return true;

				case "MIXERGETCONTROLDETAILS":
					returnValue = MixerGetControlDetails(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIXERGETCONTROLDETAILSA":
					returnValue = MixerGetControlDetailsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIXERGETCONTROLDETAILSW":
					returnValue = MixerGetControlDetailsW(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIXERGETDEVCAPSA":
					returnValue = MixerGetDevCapsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIXERGETDEVCAPSW":
					returnValue = MixerGetDevCapsW(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIXERGETLINECONTROLSW":
					returnValue = MixerGetLineControlsW(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIXERGETLINEINFOW":
					returnValue = MixerGetLineInfoW(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIXERGETNUMDEVS":
					returnValue = MixerGetNumDevs();
					return true;

				case "MIXERSETCONTROLDETAILS":
					returnValue = MixerSetControlDetails(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEOUTGETNUMDEVS":
					returnValue = WaveOutGetNumDevs();
					return true;

				case "WAVEOUTGETDEVCAPSA":
					returnValue = WaveOutGetDevCapsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEOUTGETDEVCAPSW":
					returnValue = WaveOutGetDevCapsW(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEOUTGETERRORTEXTA":
					returnValue = WaveOutGetErrorTextA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEOUTMESSAGE":
					returnValue = WaveOutMessage(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "WAVEOUTGETVOLUME":
					returnValue = WaveOutGetVolume(a.UInt32(0), a.UInt32(1));
					return true;

				case "WAVEOUTSETVOLUME":
					returnValue = WaveOutSetVolume(a.UInt32(0), a.UInt32(1));
					return true;

				case "WAVEOUTOPEN":
					returnValue = WaveOutOpen(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
					return true;

				case "WAVEOUTCLOSE":
					returnValue = WaveOutClose(a.UInt32(0));
					return true;

				case "WAVEOUTPREPAREHEADER":
					returnValue = WaveOutPrepareHeader(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEOUTUNPREPAREHEADER":
					returnValue = WaveOutUnprepareHeader(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEOUTWRITE":
					returnValue = WaveOutWrite(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEOUTPAUSE":
					returnValue = WaveOutPause(a.UInt32(0));
					return true;

				case "WAVEOUTRESTART":
					returnValue = WaveOutRestart(a.UInt32(0));
					return true;

				case "WAVEOUTRESET":
					returnValue = WaveOutReset(a.UInt32(0));
					return true;

				case "WAVEOUTGETPOSITION":
					returnValue = WaveOutGetPosition(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEINGETNUMDEVS":
					returnValue = WaveInGetNumDevs();
					return true;

				case "WAVEINGETDEVCAPSA":
					returnValue = WaveInGetDevCapsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEINGETDEVCAPSW":
					returnValue = WaveInGetDevCapsW(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEINGETERRORTEXTA":
					returnValue = WaveInGetErrorTextA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEINMESSAGE":
					returnValue = WaveInMessage(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "WAVEINOPEN":
					returnValue = WaveInOpen(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
					return true;

				case "WAVEINCLOSE":
					returnValue = WaveInClose(a.UInt32(0));
					return true;

				case "WAVEINPREPAREHEADER":
					returnValue = WaveInPrepareHeader(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEINUNPREPAREHEADER":
					returnValue = WaveInUnprepareHeader(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEINADDBUFFER":
					returnValue = WaveInAddBuffer(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEINSTART":
					returnValue = WaveInStart(a.UInt32(0));
					return true;

				case "WAVEINSTOP":
					returnValue = WaveInStop(a.UInt32(0));
					return true;

				case "WAVEINRESET":
					returnValue = WaveInReset(a.UInt32(0));
					return true;

				case "WAVEINGETPOSITION":
					returnValue = WaveInGetPosition(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIXERGETLINEINFOA":
					returnValue = MixerGetLineInfoA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIXERGETLINECONTROLSA":
					returnValue = MixerGetLineControlsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "AUXGETNUMDEVS":
					returnValue = AuxGetNumDevs();
					return true;

				case "AUXGETDEVCAPSA":
					returnValue = AuxGetDevCapsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "AUXGETVOLUME":
					returnValue = AuxGetVolume(a.UInt32(0), a.UInt32(1));
					return true;

				case "AUXSETVOLUME":
					returnValue = AuxSetVolume(a.UInt32(0), a.UInt32(1));
					return true;

				case "MIDIOUTGETNUMDEVS":
					returnValue = MidiOutGetNumDevs();
					return true;

				case "MIDIOUTGETDEVCAPSA":
					returnValue = MidiOutGetDevCapsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDIOUTOPEN":
					returnValue = MidiOutOpen(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				case "MIDIOUTCLOSE":
					returnValue = MidiOutClose(a.UInt32(0));
					return true;

				case "MIDIOUTGETVOLUME":
					returnValue = MidiOutGetVolume(a.UInt32(0), a.UInt32(1));
					return true;

				case "MIDIOUTLONGMSG":
					returnValue = MidiOutLongMsg(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDIOUTGETID":
					returnValue = MidiOutGetID(a.UInt32(0), a.UInt32(1));
					return true;

				case "SNDPLAYSOUND":
					returnValue = SndPlaySound(a.LpcStr(0), a.UInt32(1));
					return true;

				case "SNDPLAYSOUNDA":
					returnValue = SndPlaySoundA(a.LpcStr(0), a.UInt32(1));
					return true;

				case "SNDPLAYSOUNDW":
					returnValue = SndPlaySoundW(a.LpcWStr(0), a.UInt32(1));
					return true;

				case "PLAYSOUND":
					returnValue = PlaySound(a.LpcStr(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "PLAYSOUNDA":
					returnValue = PlaySoundA(a.LpcStr(0), a.UInt32(1), a.UInt32(2));
					return true;

				// Joystick functions
				case "JOYCONFIGCHANGED":
					returnValue = JoyConfigChanged(a.UInt32(0));
					return true;

				case "JOYGETNUMDEVS":
					returnValue = JoyGetNumDevs();
					return true;

				// MIDI Output functions
				case "MIDIOUTGETERRORTEXTA":
					returnValue = MidiOutGetErrorTextA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDIOUTPREPAREHEADER":
					returnValue = MidiOutPrepareHeader(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDIOUTRESET":
					returnValue = MidiOutReset(a.UInt32(0));
					return true;

				case "MIDIOUTSETVOLUME":
					returnValue = MidiOutSetVolume(a.UInt32(0), a.UInt32(1));
					return true;

				case "MIDIOUTSHORTMSG":
					returnValue = MidiOutShortMsg(a.UInt32(0), a.UInt32(1));
					return true;

				case "MIDIOUTUNPREPAREHEADER":
					returnValue = MidiOutUnprepareHeader(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				// MIDI Stream functions
				case "MIDISTREAMCLOSE":
					returnValue = MidiStreamClose(a.UInt32(0));
					return true;

				case "MIDISTREAMOPEN":
					returnValue = MidiStreamOpen(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
					return true;

				case "MIDISTREAMOUT":
					returnValue = MidiStreamOut(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDISTREAMPAUSE":
					returnValue = MidiStreamPause(a.UInt32(0));
					return true;

				case "MIDISTREAMPROPERTY":
					returnValue = MidiStreamProperty(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDISTREAMRESTART":
					returnValue = MidiStreamRestart(a.UInt32(0));
					return true;

				case "MIDISTREAMSTOP":
					returnValue = MidiStreamStop(a.UInt32(0));
					return true;

				case "MIDISTREAMPOSITION":
					returnValue = MidiStreamPosition(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				// MIDI Input functions
				case "MIDIINGETNUMDEVS":
					returnValue = MidiInGetNumDevs();
					return true;

				case "MIDIINGETDEVCAPSA":
					returnValue = MidiInGetDevCapsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDIINOPEN":
					returnValue = MidiInOpen(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				case "MIDIINCLOSE":
					returnValue = MidiInClose(a.UInt32(0));
					return true;

				case "MIDIINPREPAREHEADER":
					returnValue = MidiInPrepareHeader(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDIINUNPREPAREHEADER":
					returnValue = MidiInUnprepareHeader(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDIINADDBUFFER":
					returnValue = MidiInAddBuffer(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "MIDIINRESET":
					returnValue = MidiInReset(a.UInt32(0));
					return true;

				default:
					_logger.LogInformation("[WinMM] Unimplemented export: {Export}", export);
					return false;
			}
		}

		[DllModuleExport(1)]
		private uint TimeGetTime()
		{
			// Return time in milliseconds since start
			var time = (uint)_stopwatch.ElapsedMilliseconds;
			return time;
		}

		/// <summary>
		/// Queries the timer device capabilities.
		/// MMRESULT timeGetDevCaps(
		///   [out] LPTIMECAPS ptc,
		///   [in]  UINT       cbtc
		/// );
		/// </summary>
		[DllModuleExport(1)]
		private uint TimeGetDevCaps(uint lpTimeCaps, uint cbTimeCaps)
		{
			_logger.LogInformation("[WinMM] timeGetDevCaps(lpTimeCaps=0x{LpTimeCaps:X8}, cbTimeCaps={CbTimeCaps})",
				lpTimeCaps, cbTimeCaps);

			// Size check
			uint timecapsSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeTypes.TIMECAPS>();
			if (cbTimeCaps < timecapsSize)
			{
				_logger.LogWarning("[WinMM] timeGetDevCaps: Buffer too small");
				return (uint)TimerResult.TIMERR_STRUCT;
			}

			// Create and write TIMECAPS structure
			var timecaps = new NativeTypes.TIMECAPS
			{
				wPeriodMin = 1,       // Minimum timer resolution: 1 ms
				wPeriodMax = 1000000  // Maximum timer resolution: 1000000 ms
			};

			_env.MemWrite32(lpTimeCaps, timecaps.wPeriodMin);
			_env.MemWrite32(lpTimeCaps + 4, timecaps.wPeriodMax);

			return (uint)TimerResult.TIMERR_NOERROR;
		}

		[DllModuleExport(1)]
		private uint TimeBeginPeriod(uint uPeriod)
		{
			_logger.LogInformation("[WinMM] timeBeginPeriod({UPeriod})", uPeriod);
			_timerPeriod = uPeriod;
			return 0; // TIMERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint TimeEndPeriod(uint uPeriod)
		{
			_logger.LogInformation("[WinMM] timeEndPeriod({UPeriod})", uPeriod);
			_timerPeriod = 0;
			return 0; // TIMERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint TimeKillEvent(uint uTimerId)
		{
			_logger.LogInformation("[WinMM] timeKillEvent({UTimerId})", uTimerId);
			
			// Remove the timer from tracking if it exists
			if (_multimediaTimers.TryRemove(uTimerId, out _))
			{
				_logger.LogInformation("[WinMM] timeKillEvent: Removed timer {TimerId}", uTimerId);
			}
			else
			{
				_logger.LogDebug("[WinMM] timeKillEvent: Timer {TimerId} not found (may have already been killed)", uTimerId);
			}
			
			// Always return success for simplicity (matching original stub behavior)
			// Real Windows API may return error codes, but for emulation we're lenient
			return 0; // TIMERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint TimeSetEvent(uint uDelay, uint uResolution, uint lpTimeProc, uint dwUser, uint fuEvent)
		{
			// TimeSetEvent sets a timer event
			// Returns a timer identifier or 0 if it failed
			_logger.LogInformation("[WinMM] timeSetEvent(delay={UDelay}, resolution={UResolution}, callback=0x{LpTimeProc:X8}, dwUser=0x{DwUser:X8}, fuEvent=0x{FuEvent:X})",
				uDelay, uResolution, lpTimeProc, dwUser, fuEvent);

			// Validate parameters
			if (lpTimeProc == 0)
			{
				_logger.LogWarning("[WinMM] timeSetEvent: Callback address is NULL");
				return 0; // NULL - failure
			}

			// Generate a unique timer ID using thread-safe increment
			var timerId = Interlocked.Increment(ref _nextMultimediaTimerId) - 1;

			// Create timer info and store it
			var timerInfo = new MultimediaTimerInfo(
				TimerId: timerId,
				Delay: uDelay,
				Resolution: uResolution,
				TimeProc: lpTimeProc,
				DwUser: dwUser,
				FuEvent: fuEvent
			);

			_multimediaTimers[timerId] = timerInfo;

			_logger.LogInformation("[WinMM] timeSetEvent: Created timer ID=0x{TimerId:X}, callback=0x{Callback:X8}",
				timerId, lpTimeProc);

			// Note: The timer is now registered but won't fire automatically without a timer scheduler.
			// The CallTimeProcAsync method is ready to be invoked when the timer fires.

			return timerId;
		}

		/// <summary>
		/// Public method to manually trigger a multimedia timer callback.
		/// This can be called by a timer scheduler or for testing purposes.
		/// </summary>
		public async Task FireMultimediaTimerAsync(uint timerId, CancellationToken cancellationToken = default)
		{
			if (!_multimediaTimers.TryGetValue(timerId, out var timerInfo))
			{
				_logger.LogWarning("[WinMM] FireMultimediaTimerAsync: Timer 0x{TimerId:X} not found", timerId);
				return;
			}

			// Call the timer callback using the async pattern
			// void CALLBACK TimeProc(UINT uTimerID, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2)
			await CallTimeProcAsync(
				timerInfo.TimeProc,
				timerId,
				0, // uMsg (not used for timeSetEvent callbacks)
				timerInfo.DwUser,
				0, // dw1 (reserved, not used)
				0, // dw2 (reserved, not used)
				cancellationToken
			).ConfigureAwait(false);
		}

		[DllModuleExport(1)]
		private uint JoyGetPosEx(uint uJoyID, uint pji)
		{
			// JoyGetPosEx queries the position and button status of a joystick
			_logger.LogInformation("[WinMM] joyGetPosEx(uJoyID={UJoyId}, pji=0x{Pji:X8})", uJoyID, pji);

			if (pji == 0)
			{
				return 165; // JOYERR_PARMS
			}

			// Return JOYERR_UNPLUGGED to indicate no joystick is connected
			return 167; // JOYERR_UNPLUGGED
		}

		[DllModuleExport(1)]
		private uint JoyGetDevCapsA(uint uJoyID, uint pjc, uint cbjc)
		{
			// JoyGetDevCapsA queries the capabilities of a joystick
			_logger.LogInformation("[WinMM] joyGetDevCapsA(uJoyID={UJoyId}, pjc=0x{Pjc:X8}, cbjc={Cbjc})", uJoyID, pjc, cbjc);

			if (pjc == 0)
			{
				return 165; // JOYERR_PARMS
			}

			// Return JOYERR_UNPLUGGED to indicate no joystick is connected
			return 167; // JOYERR_UNPLUGGED
		}

		[DllModuleExport(1)]
		private uint MciSendStringA(uint lpszCommand, uint lpszReturnString, uint cchReturn, uint hwndCallback)
		{
			// MciSendStringA sends a command string to an MCI device
			var command = lpszCommand != 0 ? _env.ReadAnsiString(lpszCommand) : "";
			_logger.LogInformation("[WinMM] mciSendStringA: \"{Command}\"", command);

			// For now, just return success
			return 0; // MMSYSERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint MciSendCommandA(uint wDeviceID, uint uMsg, uint dwParam1, uint dwParam2)
		{
			// MciSendCommandA sends a command message to an MCI device
			_logger.LogInformation("[WinMM] mciSendCommandA(wDeviceID={WDeviceId}, uMsg=0x{UMsg:X8}, dwParam1=0x{DwParam1:X8}, dwParam2=0x{DwParam2:X8})",
				wDeviceID, uMsg, dwParam1, dwParam2);

			// For now, just return success
			return 0; // MMSYSERR_NOERROR
		}

		// MMIO handle tracking
		private readonly Dictionary<uint, MmioFile> _mmioFiles = new();
		private uint _nextMmioHandle = 0x50000000;

		private class MmioFile
		{
			public uint Handle { get; set; }
			public string? FileName { get; set; }
			public uint Flags { get; set; }
			public uint Position { get; set; }
			public byte[]? Buffer { get; set; }
			public uint BufferSize { get; set; }
		}

		[DllModuleExport(1)]
		private uint MmioOpenA(uint lpszFileName, uint lpmmioinfo, uint dwOpenFlags)
		{
			// MmioOpenA opens a file for multimedia I/O
			var fileName = lpszFileName != 0 ? _env.ReadAnsiString(lpszFileName) : "";
			_logger.LogInformation("[WinMM] mmioOpenA(\"{FileName}\", lpmmioinfo=0x{Lpmmioinfo:X8}, dwOpenFlags=0x{DwOpenFlags:X8})",
				fileName, lpmmioinfo, dwOpenFlags);

			// Create a handle for this MMIO file
			var handle = _nextMmioHandle++;
			var mmioFile = new MmioFile
			{
				Handle = handle,
				FileName = fileName,
				Flags = dwOpenFlags,
				Position = 0,
				Buffer = new byte[4096], // Default buffer size
				BufferSize = 4096
			};

			_mmioFiles[handle] = mmioFile;

			_logger.LogInformation("[WinMM] mmioOpenA: Created handle 0x{Handle:X8} for \"{FileName}\"", handle, fileName);
			return handle;
		}

		[DllModuleExport(1)]
		private uint MmioClose(uint hmmio, uint wFlags)
		{
			// MmioClose closes a file opened with mmioOpen
			_logger.LogInformation("[WinMM] mmioClose(hmmio=0x{Hmmio:X8}, wFlags=0x{WFlags:X8})", hmmio, wFlags);

			if (_mmioFiles.ContainsKey(hmmio))
			{
				_mmioFiles.Remove(hmmio);
				_logger.LogInformation("[WinMM] mmioClose: Closed handle 0x{Hmmio:X8}", hmmio);
				return 0; // MMSYSERR_NOERROR
			}

			_logger.LogWarning("[WinMM] mmioClose: Invalid handle 0x{Hmmio:X8}", hmmio);
			return 256; // MMIOERR_INVALIDHANDLE
		}

		[DllModuleExport(1)]
		private uint MmioRead(uint hmmio, uint pch, uint cch)
		{
			// MmioRead reads data from a file opened with mmioOpen
			_logger.LogInformation("[WinMM] mmioRead(hmmio=0x{Hmmio:X8}, pch=0x{Pch:X8}, cch={Cch})", hmmio, pch, cch);

			if (!_mmioFiles.ContainsKey(hmmio))
			{
				_logger.LogWarning("[WinMM] mmioRead: Invalid handle 0x{Hmmio:X8}", hmmio);
				return 0xFFFFFFFF; // -1 indicates error
			}

			// For stub implementation, return 0 bytes read
			// A full implementation would read from the actual file
			_logger.LogInformation("[WinMM] mmioRead: Stub returning 0 bytes");
			return 0;
		}

		[DllModuleExport(1)]
		private uint MmioSeek(uint hmmio, int lOffset, int iOrigin)
		{
			// MmioSeek moves the file pointer in a file opened with mmioOpen
			_logger.LogInformation("[WinMM] mmioSeek(hmmio=0x{Hmmio:X8}, lOffset={LOffset}, iOrigin={IOrigin})", hmmio, lOffset, iOrigin);

			if (!_mmioFiles.TryGetValue(hmmio, out var mmioFile))
			{
				_logger.LogWarning("[WinMM] mmioSeek: Invalid handle 0x{Hmmio:X8}", hmmio);
				return 0xFFFFFFFF; // -1 indicates error
			}

			// Calculate new position based on origin
			// SEEK_SET = 0, SEEK_CUR = 1, SEEK_END = 2
			uint newPosition = iOrigin switch
			{
				0 => (uint)lOffset, // SEEK_SET
				1 => (uint)((int)mmioFile.Position + lOffset), // SEEK_CUR
				2 => 0, // SEEK_END - would need file size
				_ => mmioFile.Position
			};

			mmioFile.Position = newPosition;
			_logger.LogInformation("[WinMM] mmioSeek: New position {NewPosition}", newPosition);
			return newPosition;
		}

		[DllModuleExport(1)]
		private uint MmioGetInfo(uint hmmio, uint lpmmioinfo, uint wFlags)
		{
			// MmioGetInfo retrieves information about a file opened with mmioOpen
			_logger.LogInformation("[WinMM] mmioGetInfo(hmmio=0x{Hmmio:X8}, lpmmioinfo=0x{Lpmmioinfo:X8}, wFlags=0x{WFlags:X8})",
				hmmio, lpmmioinfo, wFlags);

			if (!_mmioFiles.ContainsKey(hmmio))
			{
				_logger.LogWarning("[WinMM] mmioGetInfo: Invalid handle 0x{Hmmio:X8}", hmmio);
				return 256; // MMIOERR_INVALIDHANDLE
			}

			// For stub implementation, just return success
			// A full implementation would fill in the MMIOINFO structure
			return 0; // MMSYSERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint MmioSetInfo(uint hmmio, uint lpmmioinfo, uint wFlags)
		{
			// MmioSetInfo sets information about a file opened with mmioOpen
			_logger.LogInformation("[WinMM] mmioSetInfo(hmmio=0x{Hmmio:X8}, lpmmioinfo=0x{Lpmmioinfo:X8}, wFlags=0x{WFlags:X8})",
				hmmio, lpmmioinfo, wFlags);

			if (!_mmioFiles.ContainsKey(hmmio))
			{
				_logger.LogWarning("[WinMM] mmioSetInfo: Invalid handle 0x{Hmmio:X8}", hmmio);
				return 256; // MMIOERR_INVALIDHANDLE
			}

			// For stub implementation, just return success
			return 0; // MMSYSERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint MmioDescend(uint hmmio, uint lpck, uint lpckParent, uint wFlags)
		{
			// MmioDescend descends into a RIFF chunk
			_logger.LogInformation("[WinMM] mmioDescend(hmmio=0x{Hmmio:X8}, lpck=0x{Lpck:X8}, lpckParent=0x{LpckParent:X8}, wFlags=0x{WFlags:X8})",
				hmmio, lpck, lpckParent, wFlags);

			if (!_mmioFiles.ContainsKey(hmmio))
			{
				_logger.LogWarning("[WinMM] mmioDescend: Invalid handle 0x{Hmmio:X8}", hmmio);
				return 256; // MMIOERR_INVALIDHANDLE
			}

			// For stub implementation, just return success
			// A full implementation would parse RIFF chunks
			return 0; // MMSYSERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint MmioAscend(uint hmmio, uint lpck, uint wFlags)
		{
			// MmioAscend ascends out of a RIFF chunk
			_logger.LogInformation("[WinMM] mmioAscend(hmmio=0x{Hmmio:X8}, lpck=0x{Lpck:X8}, wFlags=0x{WFlags:X8})",
				hmmio, lpck, wFlags);

			if (!_mmioFiles.ContainsKey(hmmio))
			{
				_logger.LogWarning("[WinMM] mmioAscend: Invalid handle 0x{Hmmio:X8}", hmmio);
				return 256; // MMIOERR_INVALIDHANDLE
			}

			// For stub implementation, just return success
			return 0; // MMSYSERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint MmioAdvance(uint hmmio, uint lpmmioinfo, uint wFlags)
		{
			// MmioAdvance advances the I/O buffer of a file opened with mmioOpen
			_logger.LogInformation("[WinMM] mmioAdvance(hmmio=0x{Hmmio:X8}, lpmmioinfo=0x{Lpmmioinfo:X8}, wFlags=0x{WFlags:X8})",
				hmmio, lpmmioinfo, wFlags);

			if (!_mmioFiles.ContainsKey(hmmio))
			{
				_logger.LogWarning("[WinMM] mmioAdvance: Invalid handle 0x{Hmmio:X8}", hmmio);
				return 256; // MMIOERR_INVALIDHANDLE
			}

			// For stub implementation, just return success
			return 0; // MMSYSERR_NOERROR
		}

		/// <summary>
		/// Creates a chunk in a RIFF file opened with mmioOpen.
		/// MMRESULT mmioCreateChunk(
		///   [in]      HMMIO    hmmio,
		///   [in, out] LPMMCKINFO pmmcki,
		///   [in]      UINT     fuCreate
		/// );
		/// </summary>
		[DllModuleExport(1)]
		private uint MmioCreateChunk(uint hmmio, uint pmmcki, uint fuCreate)
		{
			_logger.LogInformation("[WinMM] mmioCreateChunk(hmmio=0x{Hmmio:X8}, pmmcki=0x{Pmmcki:X8}, fuCreate=0x{FuCreate:X8})",
				hmmio, pmmcki, fuCreate);

			if (!_mmioFiles.ContainsKey(hmmio))
			{
				_logger.LogWarning("[WinMM] mmioCreateChunk: Invalid handle 0x{Hmmio:X8}", hmmio);
				return (uint)NativeTypes.MMIOError.MMIOERR_BASE;
			}

			// fuCreate flags:
			// MMIO_CREATELIST (0x0040) - Creates a LIST chunk
			// MMIO_CREATERIFF (0x0020) - Creates a RIFF chunk

			// For stub implementation, just return success
			// A full implementation would:
			// 1. Write the chunk header to the file
			// 2. Update the MMCKINFO structure with chunk offset info
			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Writes data to a file opened with mmioOpen.
		/// LONG mmioWrite(
		///   [in] HMMIO  hmmio,
		///   [in] const char *pch,
		///   [in] LONG   cch
		/// );
		/// </summary>
		[DllModuleExport(1)]
		private uint MmioWrite(uint hmmio, uint pch, uint cch)
		{
			_logger.LogInformation("[WinMM] mmioWrite(hmmio=0x{Hmmio:X8}, pch=0x{Pch:X8}, cch={Cch})",
				hmmio, pch, cch);

			if (!_mmioFiles.ContainsKey(hmmio))
			{
				_logger.LogWarning("[WinMM] mmioWrite: Invalid handle 0x{Hmmio:X8}", hmmio);
				return 0xFFFFFFFF; // -1 indicates error
			}

			// For stub implementation, return the number of bytes "written"
			// A full implementation would actually write the data to the file
			return cch;
		}

		// Mixer handle tracking
		private readonly Dictionary<uint, MixerDevice> _mixerDevices = new();
		private uint _nextMixerHandle = 0x60000000;

		private class MixerDevice
		{
			public uint Handle { get; set; }
			public uint DeviceId { get; set; }
			public uint Callback { get; set; }
			public uint Instance { get; set; }
			public uint Flags { get; set; }
			public float Volume { get; set; } = 1.0f; // Default full volume
			public float Balance { get; set; } = 0.0f; // Default centered
		}

		[DllModuleExport(1)]
		private uint MixerOpen(uint phmx, uint uMxId, uint dwCallback, uint dwInstance, uint fdwOpen)
		{
			// MixerOpen opens an audio mixer device
			_logger.LogInformation("[WinMM] mixerOpen(phmx=0x{Phmx:X8}, uMxId={UMxId}, dwCallback=0x{DwCallback:X8}, dwInstance=0x{DwInstance:X8}, fdwOpen=0x{FdwOpen:X8})",
				phmx, uMxId, dwCallback, dwInstance, fdwOpen);

			if (phmx == 0)
			{
				_logger.LogWarning("[WinMM] mixerOpen: NULL handle pointer");
				return 11; // MMSYSERR_INVALPARAM
			}

			// Initialize audio backend if not already done
			if (_env.AudioBackend == null && _env.BackendFactory != null)
			{
				_env.AudioBackend = _env.BackendFactory.CreateAudioBackend(_logger);
				// In WASM mode, we cannot block on async operations (Monitor.Wait is not supported).
				// Fire-and-forget the initialization - the backend will self-mark as initialized.
				if (PlatformHelpers.IsWasm)
				{
					// In WASM, continuations run on the synchronization context, so we don't specify TaskScheduler
					_ = _env.AudioBackend.InitializeAsync()
						.ContinueWith(t =>
						{
							if (t.IsFaulted)
							{
								_logger.LogError(t.Exception?.GetBaseException(), "[WinMM] Audio backend initialization failed (WASM mode)");
							}
							else if (t.Result)
							{
								_logger.LogInformation("[WinMM] Audio backend initialized successfully (WASM mode)");
							}
							else
							{
								_logger.LogWarning("[WinMM] Audio backend initialization returned false (WASM mode)");
							}
						});
					_logger.LogInformation("[WinMM] Audio backend initialization started asynchronously (WASM mode)");
				}
				else
				{
					_env.AudioBackend.InitializeAsync().GetAwaiter().GetResult();
				}
			}

			// Create a handle for this mixer device
			var handle = _nextMixerHandle++;
			var mixer = new MixerDevice
			{
				Handle = handle,
				DeviceId = uMxId,
				Callback = dwCallback,
				Instance = dwInstance,
				Flags = fdwOpen,
				Volume = 1.0f,
				Balance = 0.0f
			};

			_mixerDevices[handle] = mixer;

			// Write the handle to the output parameter
			_env.MemWrite32(phmx, handle);

			_logger.LogInformation("[WinMM] mixerOpen: Created handle 0x{Handle:X8} for device {UMxId} with audio backend support", handle, uMxId);
			return 0; // MMSYSERR_NOERROR
		}

		[DllModuleExport(11)]
		private uint MixerClose(uint hmx)
		{
			_logger.LogInformation("[WinMM] mixerClose(hmx=0x{Hmx:X8})", hmx);

			if (_mixerDevices.ContainsKey(hmx))
			{
				_mixerDevices.Remove(hmx);
				_logger.LogInformation("[WinMM] mixerClose: Closed mixer handle 0x{Hmx:X8}", hmx);
				return 0; // MMSYSERR_NOERROR
			}

			_logger.LogWarning("[WinMM] mixerClose: Invalid mixer handle 0x{Hmx:X8}", hmx);
			return 6; // MMSYSERR_INVALHANDLE
		}

		[DllModuleExport(12)]
		private uint MixerGetControlDetailsA(uint hmxobj, uint pmxcd, uint fdwDetails)
		{
			_logger.LogInformation("[WinMM] mixerGetControlDetailsA(hmxobj=0x{Hmxobj:X8}, pmxcd=0x{Pmxcd:X8}, fdwDetails=0x{FdwDetails:X8})",
				hmxobj, pmxcd, fdwDetails);

			// For stub implementation, return success
			// A full implementation would read mixer control values
			return 0; // MMSYSERR_NOERROR
		}
		[DllModuleExport(12)]
		private uint MixerGetControlDetails(uint hmxobj, uint pmxcd, uint fdwDetails)
		{
			_logger.LogInformation("[WinMM] mixerGetControlDetails(hmxobj=0x{Hmxobj:X8}, pmxcd=0x{Pmxcd:X8}, fdwDetails=0x{FdwDetails:X8})",
				hmxobj, pmxcd, fdwDetails);

			// For stub implementation, return success
			// A full implementation would read mixer control values
			return 0; // MMSYSERR_NOERROR
		}

		[DllModuleExport(13)]
		private uint MixerSetControlDetails(uint hmxobj, uint pmxcd, uint fdwDetails)
		{
			_logger.LogInformation("[WinMM] mixerSetControlDetails(hmxobj=0x{Hmxobj:X8}, pmxcd=0x{Pmxcd:X8}, fdwDetails=0x{FdwDetails:X8})",
				hmxobj, pmxcd, fdwDetails);

			// For stub implementation, return success
			// A full implementation would set mixer control values (volume, balance, etc.)
			return 0; // MMSYSERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint WaveOutGetNumDevs()
		{
			_logger.LogInformation("[WinMM] waveOutGetNumDevs()");
			// Return 1 device available
			return 1;
		}

		[DllModuleExport(1)]
		private uint WaveOutGetDevCapsA(uint uDeviceID, uint pwoc, uint cbwoc)
		{
			_logger.LogInformation("[WinMM] waveOutGetDevCapsA(uDeviceID={UDeviceID}, pwoc=0x{Pwoc:X8}, cbwoc={Cbwoc})",
				uDeviceID, pwoc, cbwoc);

			// Fill in WAVEOUTCAPS structure if pwoc is valid
			if (pwoc != 0 && cbwoc > 0)
			{
				// For simplicity, just zero out the structure
				// A full implementation would fill in device capabilities
				for (uint i = 0; i < cbwoc; i++)
				{
					_env.MemWrite8(pwoc + i, 0);
				}
			}

			return 0; // MMSYSERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint WaveOutMessage(uint hwo, uint uMsg, uint dw1, uint dw2)
		{
			_logger.LogInformation("[WinMM] waveOutMessage(hwo=0x{Hwo:X8}, uMsg={UMsg}, dw1=0x{Dw1:X8}, dw2=0x{Dw2:X8})",
				hwo, uMsg, dw1, dw2);

			// Stub implementation - return 0 (success)
			return 0;
		}

		/// <summary>
		/// Retrieves the current volume setting of the specified waveform-audio output device.
		/// MMRESULT waveOutGetVolume(
		///   [in]  HWAVEOUT hwo,
		///   [out] LPDWORD  pdwVolume
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint WaveOutGetVolume(uint hwo, uint pdwVolume)
		{
			_logger.LogInformation("[WinMM] waveOutGetVolume(hwo=0x{Hwo:X8}, pdwVolume=0x{PdwVolume:X8})",
				hwo, pdwVolume);

			// Return full volume (0xFFFFFFFF = max volume for both left and right channels)
			if (pdwVolume != 0)
			{
				_env.MemWrite32(pdwVolume, 0xFFFFFFFF);
			}

			return 0; // MMSYSERR_NOERROR
		}

		/// <summary>
		/// Sets the volume level of the specified waveform-audio output device.
		/// MMRESULT waveOutSetVolume(
		///   [in] HWAVEOUT hwo,
		///   [in] DWORD    dwVolume
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint WaveOutSetVolume(uint hwo, uint dwVolume)
		{
			_logger.LogInformation("[WinMM] waveOutSetVolume(hwo=0x{Hwo:X8}, dwVolume=0x{DwVolume:X8})",
				hwo, dwVolume);

			// Accept the volume setting (but don't actually change anything)
			return 0; // MMSYSERR_NOERROR
		}

		// Wave handle tracking
		private readonly Dictionary<uint, WaveDeviceInfo> _waveOutDevices = new();
		private readonly Dictionary<uint, WaveDeviceInfo> _waveInDevices = new();
		private uint _nextWaveOutHandle = 0x1000;
		private uint _nextWaveInHandle = 0x2000;

		private class WaveDeviceInfo
		{
			public uint Handle { get; set; }
			public uint Callback { get; set; }
			public uint CallbackInstance { get; set; }
			public uint Flags { get; set; }
		}

		/// <summary>
		/// Opens the specified waveform-audio output device for playback.
		/// </summary>
		[DllModuleExport(3)]
		private uint WaveOutOpen(uint phwo, uint uDeviceID, uint pwfx, uint dwCallback, uint dwInstance, uint fdwOpen)
		{
			_logger.LogInformation("[WinMM] waveOutOpen(phwo=0x{Phwo:X8}, uDeviceID={UDeviceID}, pwfx=0x{Pwfx:X8}, dwCallback=0x{DwCallback:X8}, dwInstance=0x{DwInstance:X8}, fdwOpen=0x{FdwOpen:X8})",
				phwo, uDeviceID, pwfx, dwCallback, dwInstance, fdwOpen);

			// Create a wave output device handle
			var handle = _nextWaveOutHandle++;
			_waveOutDevices[handle] = new WaveDeviceInfo
			{
				Handle = handle,
				Callback = dwCallback,
				CallbackInstance = dwInstance,
				Flags = fdwOpen
			};

			// Write the handle to the output parameter
			if (phwo != 0)
			{
				_env.MemWrite32(phwo, handle);
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Closes the given waveform-audio output device.
		/// </summary>
		[DllModuleExport(4)]
		private uint WaveOutClose(uint hwo)
		{
			_logger.LogInformation("[WinMM] waveOutClose(hwo=0x{Hwo:X8})", hwo);

			if (_waveOutDevices.Remove(hwo))
			{
				return (uint)MMSysError.MMSYSERR_NOERROR;
			}

			return (uint)MMSysError.MMSYSERR_INVALHANDLE;
		}

		/// <summary>
		/// Prepares a waveform-audio data block for playback.
		/// </summary>
		[DllModuleExport(5)]
		private uint WaveOutPrepareHeader(uint hwo, uint pwh, uint cbwh)
		{
			_logger.LogInformation("[WinMM] waveOutPrepareHeader(hwo=0x{Hwo:X8}, pwh=0x{Pwh:X8}, cbwh={Cbwh})",
				hwo, pwh, cbwh);

			if (pwh != 0)
			{
				// Set WHDR_PREPARED flag in dwFlags field
				var flagsOffset = (uint)Marshal.OffsetOf<WAVEHDR>(nameof(WAVEHDR.dwFlags));
				var flags = _env.MemRead32(pwh + flagsOffset);
				_env.MemWrite32(pwh + flagsOffset, flags | (uint)WaveHdrFlags.WHDR_PREPARED);
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Cleans up the preparation performed by waveOutPrepareHeader.
		/// </summary>
		[DllModuleExport(6)]
		private uint WaveOutUnprepareHeader(uint hwo, uint pwh, uint cbwh)
		{
			_logger.LogInformation("[WinMM] waveOutUnprepareHeader(hwo=0x{Hwo:X8}, pwh=0x{Pwh:X8}, cbwh={Cbwh})",
				hwo, pwh, cbwh);

			if (pwh != 0)
			{
				// Clear WHDR_PREPARED flag and set WHDR_DONE flag in dwFlags field
				var flagsOffset = (uint)Marshal.OffsetOf<WAVEHDR>(nameof(WAVEHDR.dwFlags));
				var flags = _env.MemRead32(pwh + flagsOffset);
				_env.MemWrite32(pwh + flagsOffset, (flags & ~(uint)WaveHdrFlags.WHDR_PREPARED) | (uint)WaveHdrFlags.WHDR_DONE);
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Sends a data block to the given waveform-audio output device.
		/// </summary>
		[DllModuleExport(7)]
		private uint WaveOutWrite(uint hwo, uint pwh, uint cbwh)
		{
			_logger.LogInformation("[WinMM] waveOutWrite(hwo=0x{Hwo:X8}, pwh=0x{Pwh:X8}, cbwh={Cbwh})",
				hwo, pwh, cbwh);

			if (pwh != 0)
			{
				// Mark buffer as done (set WHDR_DONE flag)
				var flagsOffset = (uint)Marshal.OffsetOf<WAVEHDR>(nameof(WAVEHDR.dwFlags));
				var flags = _env.MemRead32(pwh + flagsOffset);
				_env.MemWrite32(pwh + flagsOffset, flags | (uint)WaveHdrFlags.WHDR_DONE);
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Pauses playback on the given waveform-audio output device.
		/// </summary>
		[DllModuleExport(9)]
		private uint WaveOutPause(uint hwo)
		{
			_logger.LogInformation("[WinMM] waveOutPause(hwo=0x{Hwo:X8})", hwo);
			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Restarts a paused waveform-audio output device.
		/// </summary>
		[DllModuleExport(10)]
		private uint WaveOutRestart(uint hwo)
		{
			_logger.LogInformation("[WinMM] waveOutRestart(hwo=0x{Hwo:X8})", hwo);
			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Stops playback on the given waveform-audio output device and resets the current position to zero.
		/// </summary>
		[DllModuleExport(100)]
		private uint WaveOutReset(uint hwo)
		{
			_logger.LogInformation("[WinMM] waveOutReset(hwo=0x{Hwo:X8})", hwo);
			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Retrieves the current playback position of the given waveform-audio output device.
		/// </summary>
		[DllModuleExport(101)]
		private uint WaveOutGetPosition(uint hwo, uint pmmt, uint cbmmt)
		{
			_logger.LogInformation("[WinMM] waveOutGetPosition(hwo=0x{Hwo:X8}, pmmt=0x{Pmmt:X8}, cbmmt={Cbmmt})",
				hwo, pmmt, cbmmt);

			var mmtimeSize = (uint)Marshal.SizeOf<MMTIME>();
			if (pmmt != 0 && cbmmt >= mmtimeSize)
			{
				// Return time in samples
				var wTypeOffset = (uint)Marshal.OffsetOf<MMTIME>(nameof(MMTIME.wType));
				var uOffset = (uint)Marshal.OffsetOf<MMTIME>(nameof(MMTIME.u));
				var paddingOffset = (uint)Marshal.OffsetOf<MMTIME>(nameof(MMTIME.padding));
				
				_env.MemWrite32(pmmt + wTypeOffset, (uint)MMTimeType.TIME_SAMPLES);
				_env.MemWrite32(pmmt + uOffset, 0); // sample = 0
				_env.MemWrite32(pmmt + paddingOffset, 0); // padding
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Retrieves the number of waveform-audio input devices present in the system.
		/// </summary>
		[DllModuleExport(13)]
		private uint WaveInGetNumDevs()
		{
			_logger.LogInformation("[WinMM] waveInGetNumDevs()");
			return 1; // Return 1 device available
		}

		/// <summary>
		/// Retrieves the capabilities of a given waveform-audio input device.
		/// </summary>
		[DllModuleExport(14)]
		private uint WaveInGetDevCapsA(uint uDeviceID, uint pwic, uint cbwic)
		{
			_logger.LogInformation("[WinMM] waveInGetDevCapsA(uDeviceID={UDeviceID}, pwic=0x{Pwic:X8}, cbwic={Cbwic})",
				uDeviceID, pwic, cbwic);

			// Fill in WAVEINCAPS structure if pwic is valid
			if (pwic != 0 && cbwic > 0)
			{
				// Zero out the structure
				for (uint i = 0; i < cbwic; i++)
				{
					_env.MemWrite8(pwic + i, 0);
				}
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Sends messages to the waveform-audio input device drivers.
		/// </summary>
		[DllModuleExport(15)]
		private uint WaveInMessage(uint hwi, uint uMsg, uint dw1, uint dw2)
		{
			_logger.LogInformation("[WinMM] waveInMessage(hwi=0x{Hwi:X8}, uMsg={UMsg}, dw1=0x{Dw1:X8}, dw2=0x{Dw2:X8})",
				hwi, uMsg, dw1, dw2);
			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Opens the given waveform-audio input device for recording.
		/// </summary>
		[DllModuleExport(16)]
		private uint WaveInOpen(uint phwi, uint uDeviceID, uint pwfx, uint dwCallback, uint dwInstance, uint fdwOpen)
		{
			_logger.LogInformation("[WinMM] waveInOpen(phwi=0x{Phwi:X8}, uDeviceID={UDeviceID}, pwfx=0x{Pwfx:X8}, dwCallback=0x{DwCallback:X8}, dwInstance=0x{DwInstance:X8}, fdwOpen=0x{FdwOpen:X8})",
				phwi, uDeviceID, pwfx, dwCallback, dwInstance, fdwOpen);

			// Create a wave input device handle
			var handle = _nextWaveInHandle++;
			_waveInDevices[handle] = new WaveDeviceInfo
			{
				Handle = handle,
				Callback = dwCallback,
				CallbackInstance = dwInstance,
				Flags = fdwOpen
			};

			// Write the handle to the output parameter
			if (phwi != 0)
			{
				_env.MemWrite32(phwi, handle);
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Closes the given waveform-audio input device.
		/// </summary>
		[DllModuleExport(17)]
		private uint WaveInClose(uint hwi)
		{
			_logger.LogInformation("[WinMM] waveInClose(hwi=0x{Hwi:X8})", hwi);

			if (_waveInDevices.Remove(hwi))
			{
				return (uint)MMSysError.MMSYSERR_NOERROR;
			}

			return (uint)MMSysError.MMSYSERR_INVALHANDLE;
		}

		/// <summary>
		/// Prepares a buffer for waveform-audio input.
		/// </summary>
		[DllModuleExport(18)]
		private uint WaveInPrepareHeader(uint hwi, uint pwh, uint cbwh)
		{
			_logger.LogInformation("[WinMM] waveInPrepareHeader(hwi=0x{Hwi:X8}, pwh=0x{Pwh:X8}, cbwh={Cbwh})",
				hwi, pwh, cbwh);

			if (pwh != 0)
			{
				// Set WHDR_PREPARED flag in dwFlags field
				var flagsOffset = (uint)Marshal.OffsetOf<WAVEHDR>(nameof(WAVEHDR.dwFlags));
				var flags = _env.MemRead32(pwh + flagsOffset);
				_env.MemWrite32(pwh + flagsOffset, flags | (uint)WaveHdrFlags.WHDR_PREPARED);
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Cleans up the preparation performed by waveInPrepareHeader.
		/// </summary>
		[DllModuleExport(19)]
		private uint WaveInUnprepareHeader(uint hwi, uint pwh, uint cbwh)
		{
			_logger.LogInformation("[WinMM] waveInUnprepareHeader(hwi=0x{Hwi:X8}, pwh=0x{Pwh:X8}, cbwh={Cbwh})",
				hwi, pwh, cbwh);

			if (pwh != 0)
			{
				// Clear WHDR_PREPARED flag and set WHDR_DONE flag
				var flagsOffset = (uint)Marshal.OffsetOf<WAVEHDR>(nameof(WAVEHDR.dwFlags));
				var flags = _env.MemRead32(pwh + flagsOffset);
				_env.MemWrite32(pwh + flagsOffset, (flags & ~(uint)WaveHdrFlags.WHDR_PREPARED) | (uint)WaveHdrFlags.WHDR_DONE);
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Sends an input buffer to the given waveform-audio input device.
		/// </summary>
		[DllModuleExport(20)]
		private uint WaveInAddBuffer(uint hwi, uint pwh, uint cbwh)
		{
			_logger.LogInformation("[WinMM] waveInAddBuffer(hwi=0x{Hwi:X8}, pwh=0x{Pwh:X8}, cbwh={Cbwh})",
				hwi, pwh, cbwh);

			if (pwh != 0)
			{
				// Mark buffer as done immediately (for stub implementation)
				var flagsOffset = (uint)Marshal.OffsetOf<WAVEHDR>(nameof(WAVEHDR.dwFlags));
				var flags = _env.MemRead32(pwh + flagsOffset);
				_env.MemWrite32(pwh + flagsOffset, flags | (uint)WaveHdrFlags.WHDR_DONE);
				
				// Set dwBytesRecorded to 0
				var bytesRecordedOffset = (uint)Marshal.OffsetOf<WAVEHDR>(nameof(WAVEHDR.dwBytesRecorded));
				_env.MemWrite32(pwh + bytesRecordedOffset, 0);
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Starts input on the given waveform-audio input device.
		/// </summary>
		[DllModuleExport(21)]
		private uint WaveInStart(uint hwi)
		{
			_logger.LogInformation("[WinMM] waveInStart(hwi=0x{Hwi:X8})", hwi);
			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Stops waveform-audio input.
		/// </summary>
		[DllModuleExport(22)]
		private uint WaveInStop(uint hwi)
		{
			_logger.LogInformation("[WinMM] waveInStop(hwi=0x{Hwi:X8})", hwi);
			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Stops input on the given waveform-audio input device and resets the current position to zero.
		/// </summary>
		[DllModuleExport(23)]
		private uint WaveInReset(uint hwi)
		{
			_logger.LogInformation("[WinMM] waveInReset(hwi=0x{Hwi:X8})", hwi);
			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Retrieves the current input position of the given waveform-audio input device.
		/// </summary>
		[DllModuleExport(24)]
		private uint WaveInGetPosition(uint hwi, uint pmmt, uint cbmmt)
		{
			_logger.LogInformation("[WinMM] waveInGetPosition(hwi=0x{Hwi:X8}, pmmt=0x{Pmmt:X8}, cbmmt={Cbmmt})",
				hwi, pmmt, cbmmt);

			var mmtimeSize = (uint)Marshal.SizeOf<MMTIME>();
			if (pmmt != 0 && cbmmt >= mmtimeSize)
			{
				// Return time in samples
				var wTypeOffset = (uint)Marshal.OffsetOf<MMTIME>(nameof(MMTIME.wType));
				var uOffset = (uint)Marshal.OffsetOf<MMTIME>(nameof(MMTIME.u));
				var paddingOffset = (uint)Marshal.OffsetOf<MMTIME>(nameof(MMTIME.padding));
				
				_env.MemWrite32(pmmt + wTypeOffset, (uint)MMTimeType.TIME_SAMPLES);
				_env.MemWrite32(pmmt + uOffset, 0); // sample = 0
				_env.MemWrite32(pmmt + paddingOffset, 0); // padding
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Retrieves information about a specific line of a mixer device.
		/// </summary>
		[DllModuleExport(25)]
		private uint MixerGetLineInfoA(uint hmxobj, uint pmxl, uint fdwInfo)
		{
			_logger.LogInformation("[WinMM] mixerGetLineInfoA(hmxobj=0x{Hmxobj:X8}, pmxl=0x{Pmxl:X8}, fdwInfo=0x{FdwInfo:X8})",
				hmxobj, pmxl, fdwInfo);

			// Stub implementation - just zero out the structure
			if (pmxl != 0)
			{
				// Read cbStruct field to determine structure size
				var cbStructOffset = (uint)Marshal.OffsetOf<MIXERLINEA>(nameof(MIXERLINEA.cbStruct));
				var cbStruct = _env.MemRead32(pmxl + cbStructOffset);
				if (cbStruct > 0)
				{
					// Zero out the entire structure except cbStruct
					for (uint i = cbStructOffset + sizeof(uint); i < cbStruct; i++)
					{
						_env.MemWrite8(pmxl + i, 0);
					}
				}
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Retrieves one or more controls associated with an audio line.
		/// </summary>
		[DllModuleExport(26)]
		private uint MixerGetLineControlsA(uint hmxobj, uint pmxlc, uint fdwControls)
		{
			_logger.LogInformation("[WinMM] mixerGetLineControlsA(hmxobj=0x{Hmxobj:X8}, pmxlc=0x{Pmxlc:X8}, fdwControls=0x{FdwControls:X8})",
				hmxobj, pmxlc, fdwControls);

			// Stub implementation - indicate no controls
			if (pmxlc != 0)
			{
				// Set cControls to 0
				var cControlsOffset = (uint)Marshal.OffsetOf<MIXERLINECONTROLSA>(nameof(MIXERLINECONTROLSA.cControls));
				_env.MemWrite32(pmxlc + cControlsOffset, 0);
			}

			return (uint)MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Retrieves the number of auxiliary output devices present in the system.
		/// UINT auxGetNumDevs();
		/// </summary>
		[DllModuleExport(0)]
		private uint AuxGetNumDevs()
		{
			_logger.LogInformation("[WinMM] auxGetNumDevs()");

			// Return 1 device for compatibility
			return 1;
		}

		/// <summary>
		/// Retrieves the capabilities of a given auxiliary output device.
		/// MMRESULT auxGetDevCapsA(
		///   [in]  UINT_PTR  uDeviceID,
		///   [out] LPAUXCAPS pac,
		///   [in]  UINT      cbac
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint AuxGetDevCapsA(uint uDeviceID, uint pac, uint cbac)
		{
			_logger.LogInformation("[WinMM] auxGetDevCapsA(uDeviceID={UDeviceID}, pac=0x{Pac:X8}, cbac={Cbac})",
				uDeviceID, pac, cbac);

			// Fill in AUXCAPS structure if pac is valid
			if (pac != 0 && cbac > 0)
			{
				// For simplicity, just zero out the structure
				for (uint i = 0; i < cbac; i++)
				{
					_env.MemWrite8(pac + i, 0);
				}
			}

			return 0; // MMSYSERR_NOERROR
		}

		/// <summary>
		/// Retrieves the current volume setting of the specified auxiliary output device.
		/// MMRESULT auxGetVolume(
		///   [in]  UINT    uDeviceID,
		///   [out] LPDWORD pdwVolume
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint AuxGetVolume(uint uDeviceID, uint pdwVolume)
		{
			_logger.LogInformation("[WinMM] auxGetVolume(uDeviceID={UDeviceID}, pdwVolume=0x{PdwVolume:X8})",
				uDeviceID, pdwVolume);

			// Return full volume (0xFFFFFFFF = max volume for both left and right channels)
			if (pdwVolume != 0)
			{
				_env.MemWrite32(pdwVolume, 0xFFFFFFFF);
			}

			return 0; // MMSYSERR_NOERROR
		}

		/// <summary>
		/// Sets the volume of the specified auxiliary output device.
		/// MMRESULT auxSetVolume(
		///   [in] UINT  uDeviceID,
		///   [in] DWORD dwVolume
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint AuxSetVolume(uint uDeviceID, uint dwVolume)
		{
			_logger.LogInformation("[WinMM] auxSetVolume(uDeviceID={UDeviceID}, dwVolume=0x{DwVolume:X8})",
				uDeviceID, dwVolume);

			// Accept the volume setting (but don't actually change anything)
			return 0; // MMSYSERR_NOERROR
		}

		/// <summary>
		/// Retrieves the number of MIDI output devices present in the system.
		/// UINT midiOutGetNumDevs();
		/// </summary>
		[DllModuleExport(0)]
		private uint MidiOutGetNumDevs()
		{
			_logger.LogInformation("[WinMM] midiOutGetNumDevs()");

			// Return 1 device for compatibility
			// A full implementation would enumerate actual MIDI devices
			return 1;
		}

		/// <summary>
		/// Retrieves the number of joystick devices supported by the current driver.
		/// UINT joyGetNumDevs();
		/// </summary>
		[DllModuleExport(0)]
		private uint JoyGetNumDevs()
		{
			_logger.LogInformation("[WinMM] joyGetNumDevs()");

			// Return 0 joysticks for now (no joystick support)
			return 0;
		}

		/// <summary>
		/// Retrieves a textual description of an error identified by the specified error code.
		/// MMRESULT midiOutGetErrorTextA(
		///   [in]  MMRESULT wError,
		///   [out] LPSTR    lpText,
		///   [in]  UINT     cchText
		/// );
		/// </summary>
		[DllModuleExport(12, IsStub = true)]
		private uint MidiOutGetErrorTextA(uint wError, uint lpText, uint cchText)
		{
			_logger.LogInformation("[WinMM] midiOutGetErrorTextA(wError={WError}, lpText=0x{LpText:X8}, cchText={CchText})",
				wError, lpText, cchText);

			if (lpText != 0 && cchText > 0)
			{
				// Write a generic error message
				var errorMsg = "MIDI error";
				var len = Math.Min(errorMsg.Length, (int)cchText - 1);
				for (int i = 0; i < len; i++)
				{
					_env.MemWrite8(lpText + (uint)i, (byte)errorMsg[i]);
				}
				_env.MemWrite8(lpText + (uint)len, 0); // Null terminator
			}

			return 0; // MMSYSERR_NOERROR
		}

		/// <summary>
		/// Prepares a MIDI system-exclusive or stream buffer for output.
		/// MMRESULT midiOutPrepareHeader(
		///   [in] HMIDIOUT  hmo,
		///   [in] LPMIDIHDR pmh,
		///   [in] UINT      cbmh
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint MidiOutPrepareHeader(uint hmo, uint pmh, uint cbmh)
		{
			_logger.LogInformation("[WinMM] midiOutPrepareHeader(hmo=0x{Hmo:X8}, pmh=0x{Pmh:X8}, cbmh={Cbmh})",
				hmo, pmh, cbmh);

			// Set MHDR_PREPARED flag in MIDIHDR.dwFlags
			if (pmh != 0)
			{
				var flags = _env.MemRead32(pmh + MIDIHDR_FLAGS_OFFSET);
				_env.MemWrite32(pmh + MIDIHDR_FLAGS_OFFSET, flags | (uint)NativeTypes.MidiHdrFlags.MHDR_PREPARED);
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Turns off all notes on all MIDI channels for the specified MIDI output device.
		/// MMRESULT midiOutReset(
		///   [in] HMIDIOUT hmo
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint MidiOutReset(uint hmo)
		{
			_logger.LogInformation("[WinMM] midiOutReset(hmo=0x{Hmo:X8})", hmo);
			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Sets the volume of a MIDI output device.
		/// MMRESULT midiOutSetVolume(
		///   [in] HMIDIOUT hmo,
		///   [in] DWORD    dwVolume
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint MidiOutSetVolume(uint hmo, uint dwVolume)
		{
			_logger.LogInformation("[WinMM] midiOutSetVolume(hmo=0x{Hmo:X8}, dwVolume=0x{DwVolume:X8})",
				hmo, dwVolume);
			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Sends a short MIDI message to the specified MIDI output device.
		/// MMRESULT midiOutShortMsg(
		///   [in] HMIDIOUT hmo,
		///   [in] DWORD    dwMsg
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint MidiOutShortMsg(uint hmo, uint dwMsg)
		{
			_logger.LogInformation("[WinMM] midiOutShortMsg(hmo=0x{Hmo:X8}, dwMsg=0x{DwMsg:X8})",
				hmo, dwMsg);
			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Cleans up the preparation performed by midiOutPrepareHeader.
		/// MMRESULT midiOutUnprepareHeader(
		///   [in] HMIDIOUT  hmo,
		///   [in] LPMIDIHDR pmh,
		///   [in] UINT      cbmh
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint MidiOutUnprepareHeader(uint hmo, uint pmh, uint cbmh)
		{
			_logger.LogInformation("[WinMM] midiOutUnprepareHeader(hmo=0x{Hmo:X8}, pmh=0x{Pmh:X8}, cbmh={Cbmh})",
				hmo, pmh, cbmh);

			// Clear MHDR_PREPARED flag and set MHDR_DONE in MIDIHDR.dwFlags
			if (pmh != 0)
			{
				var flags = _env.MemRead32(pmh + MIDIHDR_FLAGS_OFFSET);
				_env.MemWrite32(pmh + MIDIHDR_FLAGS_OFFSET, (flags & ~(uint)NativeTypes.MidiHdrFlags.MHDR_PREPARED) | (uint)NativeTypes.MidiHdrFlags.MHDR_DONE);
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		// MIDI stream handles
		private readonly Dictionary<uint, MidiStreamInfo> _midiStreams = new();
		private uint _nextMidiStreamHandle = MIDI_STREAM_HANDLE_BASE;

		private class MidiStreamInfo
		{
			public uint Handle { get; set; }
			public uint DeviceID { get; set; }
			public uint Callback { get; set; }
			public uint Instance { get; set; }
			public uint Flags { get; set; }
			public bool IsPaused { get; set; }
		}

		/// <summary>
		/// Closes an open MIDI stream.
		/// MMRESULT midiStreamClose(
		///   [in] HMIDISTRM hms
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint MidiStreamClose(uint hms)
		{
			_logger.LogInformation("[WinMM] midiStreamClose(hms=0x{Hms:X8})", hms);

			if (_midiStreams.Remove(hms))
			{
				return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		/// <summary>
		/// Opens a MIDI stream for output.
		/// MMRESULT midiStreamOpen(
		///   [out] LPHMIDISTRM phms,
		///   [in]  LPUINT      puDeviceID,
		///   [in]  DWORD       cMidi,
		///   [in]  DWORD_PTR   dwCallback,
		///   [in]  DWORD_PTR   dwInstance,
		///   [in]  DWORD       fdwOpen
		/// );
		/// </summary>
		[DllModuleExport(24)]
		private uint MidiStreamOpen(uint phms, uint puDeviceID, uint cMidi, uint dwCallback, uint dwInstance, uint fdwOpen)
		{
			_logger.LogInformation("[WinMM] midiStreamOpen(phms=0x{Phms:X8}, puDeviceID=0x{PuDeviceID:X8}, cMidi={CMidi}, dwCallback=0x{DwCallback:X8}, dwInstance=0x{DwInstance:X8}, fdwOpen=0x{FdwOpen:X8})",
				phms, puDeviceID, cMidi, dwCallback, dwInstance, fdwOpen);

			if (phms == 0 || puDeviceID == 0)
			{
				return (uint)NativeTypes.MMSysError.MMSYSERR_INVALPARAM;
			}

			var deviceId = _env.MemRead32(puDeviceID);
			var handle = _nextMidiStreamHandle++;

			_midiStreams[handle] = new MidiStreamInfo
			{
				Handle = handle,
				DeviceID = deviceId,
				Callback = dwCallback,
				Instance = dwInstance,
				Flags = fdwOpen,
				IsPaused = false
			};

			_env.MemWrite32(phms, handle);

			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Plays or queues a stream of MIDI data.
		/// MMRESULT midiStreamOut(
		///   [in] HMIDISTRM hms,
		///   [in] LPMIDIHDR pmh,
		///   [in] UINT      cbmh
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint MidiStreamOut(uint hms, uint pmh, uint cbmh)
		{
			_logger.LogInformation("[WinMM] midiStreamOut(hms=0x{Hms:X8}, pmh=0x{Pmh:X8}, cbmh={Cbmh})",
				hms, pmh, cbmh);

			if (!_midiStreams.ContainsKey(hms))
			{
				return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
			}

			// Mark buffer as done immediately (stub behavior)
			if (pmh != 0)
			{
				var flags = _env.MemRead32(pmh + MIDIHDR_FLAGS_OFFSET);
				_env.MemWrite32(pmh + MIDIHDR_FLAGS_OFFSET, flags | (uint)NativeTypes.MidiHdrFlags.MHDR_DONE);
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Pauses playback of a specified MIDI stream.
		/// MMRESULT midiStreamPause(
		///   [in] HMIDISTRM hms
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint MidiStreamPause(uint hms)
		{
			_logger.LogInformation("[WinMM] midiStreamPause(hms=0x{Hms:X8})", hms);

			if (_midiStreams.TryGetValue(hms, out var stream))
			{
				stream.IsPaused = true;
				return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		/// <summary>
		/// Sets or retrieves properties of a MIDI data stream.
		/// MMRESULT midiStreamProperty(
		///   [in]      HMIDISTRM hms,
		///   [in, out] LPBYTE    lppropdata,
		///   [in]      DWORD     dwProperty
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint MidiStreamProperty(uint hms, uint lppropdata, uint dwProperty)
		{
			_logger.LogInformation("[WinMM] midiStreamProperty(hms=0x{Hms:X8}, lppropdata=0x{Lppropdata:X8}, dwProperty=0x{DwProperty:X8})",
				hms, lppropdata, dwProperty);

			if (!_midiStreams.ContainsKey(hms))
			{
				return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
			}

			// For stub, just return success
			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Restarts a paused MIDI stream.
		/// MMRESULT midiStreamRestart(
		///   [in] HMIDISTRM hms
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint MidiStreamRestart(uint hms)
		{
			_logger.LogInformation("[WinMM] midiStreamRestart(hms=0x{Hms:X8})", hms);

			if (_midiStreams.TryGetValue(hms, out var stream))
			{
				stream.IsPaused = false;
				return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		/// <summary>
		/// Stops playback of a specified MIDI stream.
		/// MMRESULT midiStreamStop(
		///   [in] HMIDISTRM hms
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint MidiStreamStop(uint hms)
		{
			_logger.LogInformation("[WinMM] midiStreamStop(hms=0x{Hms:X8})", hms);

			if (!_midiStreams.ContainsKey(hms))
			{
				return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		/// <summary>
		/// Plays a waveform sound specified by a filename.
		/// BOOL sndPlaySound(
		///   [in] LPCSTR pszSound,
		///   [in] UINT   fuSound
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint SndPlaySound(in LpcStr pszSound, uint fuSound)
		{
			var soundName = pszSound.ToString() ?? string.Empty;
			_logger.LogInformation("[WinMM] sndPlaySound(pszSound=\"{SoundName}\", fuSound=0x{FuSound:X8})",
				soundName, fuSound);

			// sndPlaySound plays a sound file
			// fuSound flags include:
			// SND_SYNC (0x0000) - Play synchronously
			// SND_ASYNC (0x0001) - Play asynchronously
			// SND_NODEFAULT (0x0002) - Don't play default sound if file not found
			// SND_MEMORY (0x0004) - pszSound is a memory image
			// SND_LOOP (0x0008) - Loop the sound until called again
			// SND_NOSTOP (0x0010) - Don't stop currently playing sound

			// For stub implementation, we just log and return success
			// A full implementation would actually play the sound file

			if (string.IsNullOrEmpty(soundName))
			{
				_logger.LogInformation("[WinMM] sndPlaySound: NULL sound name, stopping sound");
				return 1; // TRUE - stopping sound
			}

			_logger.LogInformation("[WinMM] sndPlaySound: Stub - would play sound \"{SoundName}\"", soundName);
			return 1; // TRUE - success
		}

		/// <summary>
		/// Plays a waveform sound specified by a filename.
		/// BOOL sndPlaySoundA(
		///   [in] LPCSTR pszSound,
		///   [in] UINT   fuSound
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint SndPlaySoundA(in LpcStr pszSound, uint fuSound)
		{
			var soundName = pszSound.ToString() ?? string.Empty;
			_logger.LogInformation("[WinMM] sndPlaySoundA(pszSound=\"{SoundName}\", fuSound=0x{FuSound:X8})",
				soundName, fuSound);

			// sndPlaySound plays a sound file
			// fuSound flags include:
			// SND_SYNC (0x0000) - Play synchronously
			// SND_ASYNC (0x0001) - Play asynchronously
			// SND_NODEFAULT (0x0002) - Don't play default sound if file not found
			// SND_MEMORY (0x0004) - pszSound is a memory image
			// SND_LOOP (0x0008) - Loop the sound until called again
			// SND_NOSTOP (0x0010) - Don't stop currently playing sound

			// For stub implementation, we just log and return success
			// A full implementation would actually play the sound file

			if (string.IsNullOrEmpty(soundName))
			{
				_logger.LogInformation("[WinMM] sndPlaySoundA: NULL sound name, stopping sound");
				return 1; // TRUE - stopping sound
			}

			_logger.LogInformation("[WinMM] sndPlaySoundA: Stub - would play sound \"{SoundName}\"", soundName);
			return 1; // TRUE - success
		}

		/// <summary>
		/// Plays a waveform sound specified by a filename or resource.
		/// BOOL PlaySoundA(
		///   [in] LPCSTR pszSound,
		///   [in] HMODULE hmod,
		///   [in] DWORD fdwSound
		/// );
		/// </summary>
		[DllModuleExport(1)]
		private uint PlaySoundA(in LpcStr pszSound, uint hmod, uint fdwSound)
		{
			var soundName = pszSound.ToString() ?? string.Empty;
			_logger.LogInformation("[WinMM] PlaySoundA(pszSound=\"{SoundName}\", hmod=0x{Hmod:X8}, fdwSound=0x{FdwSound:X8})",
				soundName, hmod, fdwSound);

			// PlaySound plays a sound file or resource
			// fdwSound flags include:
			// SND_SYNC (0x0000) - Play synchronously
			// SND_ASYNC (0x0001) - Play asynchronously
			// SND_NODEFAULT (0x0002) - Don't play default sound if file not found
			// SND_MEMORY (0x0004) - pszSound is a memory image
			// SND_LOOP (0x0008) - Loop the sound until called again
			// SND_NOSTOP (0x0010) - Don't stop currently playing sound
			// SND_APPLICATION (0x0080) - Use application-specific association
			// SND_ALIAS (0x00010000) - pszSound is a system event alias
			// SND_FILENAME (0x00020000) - pszSound is a filename
			// SND_RESOURCE (0x00040000) - pszSound is a resource identifier; hmod identifies the module
			// SND_PURGE (0x0040) - Stop all sounds
			// SND_NOWAIT (0x00002000) - Don't wait if driver is busy
			// SND_ALIAS_ID (0x00110000) - pszSound is a predefined sound identifier

			// For stub implementation, we just log and return success
			// A full implementation would actually play the sound file or resource

			if (string.IsNullOrEmpty(soundName))
			{
				_logger.LogInformation("[WinMM] PlaySoundA: NULL sound name, stopping sound");
				return 1; // TRUE - stopping sound
			}

			_logger.LogInformation("[WinMM] PlaySoundA: Stub - would play sound \"{SoundName}\" from module 0x{Hmod:X8}", soundName, hmod);
			return 1; // TRUE - success
		}

		/// <summary>
		/// Plays a waveform sound specified by a filename or resource.
		/// BOOL PlaySound(
		///   [in] LPCSTR pszSound,
		///   [in] HMODULE hmod,
		///   [in] DWORD fdwSound
		/// );
		/// </summary>
		[DllModuleExport(1)]
		private uint PlaySound(in LpcStr pszSound, uint hmod, uint fdwSound)
		{
			var soundName = pszSound.ToString() ?? string.Empty;
			_logger.LogInformation("[WinMM] PlaySound(pszSound=\"{SoundName}\", hmod=0x{Hmod:X8}, fdwSound=0x{FdwSound:X8})",
				soundName, hmod, fdwSound);

			// PlaySound plays a sound file or resource
			// fdwSound flags include:
			// SND_SYNC (0x0000) - Play synchronously
			// SND_ASYNC (0x0001) - Play asynchronously
			// SND_NODEFAULT (0x0002) - Don't play default sound if file not found
			// SND_MEMORY (0x0004) - pszSound is a memory image
			// SND_LOOP (0x0008) - Loop the sound until called again
			// SND_NOSTOP (0x0010) - Don't stop currently playing sound
			// SND_APPLICATION (0x0080) - Use application-specific association
			// SND_ALIAS (0x00010000) - pszSound is a system event alias
			// SND_FILENAME (0x00020000) - pszSound is a filename
			// SND_RESOURCE (0x00040000) - pszSound is a resource identifier; hmod identifies the module
			// SND_PURGE (0x0040) - Stop all sounds
			// SND_NOWAIT (0x00002000) - Don't wait if driver is busy
			// SND_ALIAS_ID (0x00110000) - pszSound is a predefined sound identifier

			// For stub implementation, we just log and return success
			// A full implementation would actually play the sound file or resource

			if (string.IsNullOrEmpty(soundName))
			{
				_logger.LogInformation("[WinMM] PlaySound: NULL sound name, stopping sound");
				return 1; // TRUE - stopping sound
			}

			_logger.LogInformation("[WinMM] PlaySound: Stub - would play sound \"{SoundName}\" from module 0x{Hmod:X8}", soundName, hmod);
			return 1; // TRUE - success
		}

		/// <summary>
		/// Notifies the system that joystick configuration has changed.
		/// MMRESULT joyConfigChanged(DWORD dwFlags);
		/// </summary>
		[DllModuleExport(0)]
		private uint JoyConfigChanged(uint dwFlags)
		{
			_logger.LogInformation("[WinMM] JoyConfigChanged(dwFlags=0x{DwFlags:X8})", dwFlags);
			return 0; // JOYERR_NOERROR
		}

		public async Task<(bool success, uint returnValue)> TryInvokeAsync(
			string export,
			ICpu cpu,
			VirtualMemory memory,
			CancellationToken cancellationToken = default)
		{
			_cpu = cpu;
			_memory = memory;

		    // TODO: Route timer callback exports (timeSetEvent) to async execution
		    // when callback functionality is fully implemented. For now, all APIs
		    // use synchronous stubs that don't invoke callbacks.
		    if (TryInvokeUnsafe(export, cpu, memory, out var syncReturnValue))
			{
				return (true, syncReturnValue);
			}

			// No async work performed; return failure immediately
			return (false, 0);
		}

		#region Async Callback Execution Helper

		/// <summary>
		/// Executes emulated guest code asynchronously with comprehensive safeguards.
		/// This helper method contains the common execution loop logic used by async callback methods,
		/// eliminating code duplication while ensuring consistent behavior.
		/// </summary>
		/// <param name="returnAddress">Marker address (0xDEADBEEF) indicating callback return</param>
		/// <param name="logContext">Context string for logging (e.g., "CallTimeProcAsync")</param>
		/// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
		/// <returns>True if execution completed successfully, false if aborted or failed</returns>
		private async Task<bool> ExecuteCallbackAsync(
			uint returnAddress,
			string logContext,
			CancellationToken cancellationToken = default)
		{
			const int YIELD_INTERVAL = 10000;
			var steps = 0;
			var executionSuccessful = true;
			var lastCheckEip = _cpu!.GetEip();
			var stuckCounter = 0;

			try
			{
				while (true)
				{
					// Check for cancellation at regular intervals
					if (steps % CANCELLATION_CHECK_INTERVAL == 0)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							_logger.LogInformation("[WinMM] {LogContext}: Cancellation requested at step {Steps}", logContext, steps);
							executionSuccessful = false;
							break;
						}

						// Suspend execution to preserve CPU state across async boundary
						var cpuState = CpuHelpers.SuspendExecution(_cpu);
						
						// Yield to allow other async operations to proceed
						await Task.Yield();
						
						// Resume execution with preserved state
						CpuHelpers.ResumeExecution(_cpu, cpuState);
					}

					var eip = _cpu.GetEip();

					// Check if we've returned to our marker address
					if (eip == returnAddress)
					{
						break;
					}

					// Check for invalid EIP (NULL pointer execution)
					if (eip == 0x00000000)
					{
						_logger.LogWarning("[WinMM] {LogContext}: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting", logContext);
						executionSuccessful = false;
						break;
					}

					// Check for other invalid low addresses
					if (eip < MINIMUM_VALID_EIP && eip != returnAddress)
					{
						_logger.LogError("[WinMM] {LogContext}: Execution jumped to invalid low address 0x{Eip:X8}", logContext, eip);
						executionSuccessful = false;
						break;
					}

					// Detect potential infinite loops
					if (steps > 0 && steps % INFINITE_LOOP_CHECK_INTERVAL == 0)
					{
						var currentEip = _cpu.GetEip();
						if (currentEip == lastCheckEip)
						{
							stuckCounter++;
							if (stuckCounter >= STUCK_COUNTER_THRESHOLD)
							{
								_logger.LogWarning("[WinMM] {LogContext}: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting",
									logContext, currentEip, stuckCounter);
								executionSuccessful = false;
								break;
							}
						}
						else
						{
							stuckCounter = 0;
							lastCheckEip = currentEip;
						}
					}

					// Execute instruction(s) - uses ExecuteBlockAsync for JIT CPUs, SingleStepAsync for interpreters
					await CpuHelpers.ExecuteAsync(_cpu, _memory!);
					steps++;

					// Periodically check if we should yield to other threads
					if (steps % YIELD_INTERVAL == 0)
					{
						var scheduler = _env.ThreadScheduler;
						if (scheduler != null)
						{
							scheduler.ProcessWaitTimeouts();
							if (scheduler.ShouldContextSwitch())
							{
								_logger.LogDebug("[WinMM] {LogContext}: Cooperative yield at {Steps} steps", logContext, steps);
							}
						}

						await Task.Yield();
					}
				}
			}
			catch (Exception ex)
			{
				// Rethrow critical exceptions that should not be caught
				if (ex is OutOfMemoryException || ex is StackOverflowException)
				{
					throw;
				}

				_logger.LogError(ex, "[WinMM] {LogContext}: Exception during execution: {ExMessage}", logContext, ex.Message);
				executionSuccessful = false;
			}

			return executionSuccessful;
		}

		#endregion

		#region Async Callback Methods

		/// <summary>
		/// Async version of timer event callback execution that eliminates the need for STACK_SAFETY_MARGIN.
		/// Uses async/await pattern for clean separation of host (C#) and guest (x86) execution stacks.
		/// </summary>
		/// <param name="timeProc">Address of the timer callback in emulated memory</param>
		/// <param name="uTimerID">Timer identifier</param>
		/// <param name="uMsg">Reserved (not used)</param>
		/// <param name="dwUser">User-supplied data</param>
		/// <param name="dw1">Reserved (not used)</param>
		/// <param name="dw2">Reserved (not used)</param>
		/// <param name="cancellationToken">Optional cancellation token</param>
		/// <returns>Task that completes when callback execution finishes</returns>
		private async Task CallTimeProcAsync(
			uint timeProc,
			uint uTimerID,
			uint uMsg,
			uint dwUser,
			uint dw1,
			uint dw2,
			CancellationToken cancellationToken = default)
		{
			if (_cpu == null || _memory == null)
			{
				_logger.LogWarning("[WinMM] CallTimeProcAsync: CPU or Memory not available");
				return;
			}

			_logger.LogInformation("[WinMM] CallTimeProcAsync: Calling 0x{TimeProc:X8} for timer {TimerID}", timeProc, uTimerID);

			// Validate callback address
			if (timeProc == 0)
			{
				_logger.LogWarning("[WinMM] CallTimeProcAsync: Timer callback address is NULL (0x00000000), aborting");
				return;
			}

			// Save current CPU state
			var savedEip = _cpu.GetEip();
			var savedEsp = _cpu.GetRegister("ESP");
			var savedEbp = _cpu.GetRegister("EBP");

			// Define return address marker
			const uint RETURN_ADDRESS = 0xDEADBEEF;

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			// NOTE: No STACK_SAFETY_MARGIN needed! The async architecture provides clean stack separation.
			var esp = savedEsp;

			// Push return address first
			esp -= 4;
			_memory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			// void CALLBACK TimeProc(UINT uTimerID, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2)
			esp -= 4;
			_memory.Write32(esp, dw2);

			esp -= 4;
			_memory.Write32(esp, dw1);

			esp -= 4;
			_memory.Write32(esp, dwUser);

			esp -= 4;
			_memory.Write32(esp, uMsg);

			esp -= 4;
			_memory.Write32(esp, uTimerID);

			// Update CPU registers
			_cpu.SetRegister("ESP", esp);
			_cpu.SetEip(timeProc);

			// Execute callback using the common helper method
			var executionSuccessful = await ExecuteCallbackAsync(RETURN_ADDRESS, "CallTimeProcAsync", cancellationToken).ConfigureAwait(false);

			// Restore CPU state
			_cpu.SetEip(savedEip);
			_cpu.SetRegister("ESP", savedEsp);
			_cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[WinMM] CallTimeProcAsync: Completed {Status}", executionSuccessful ? "successfully" : "with errors");
		}

		// ============================================================================
		// Joystick Functions
		// ============================================================================

		[DllModuleExport(12)]
		private uint JoyGetDevCapsW(uint uJoyID, uint pjc, uint cbjc)
		{
			_logger.LogInformation("[WinMM] joyGetDevCapsW(uJoyID={UJoyID}, pjc=0x{Pjc:X8}, cbjc={Cbjc})",
				uJoyID, pjc, cbjc);
			// Return error - no joystick present
			return (uint)NativeTypes.MMSysError.MMSYSERR_NODRIVER;
		}

		[DllModuleExport(8)]
		private uint JoyGetPos(uint uJoyID, uint pji)
		{
			_logger.LogInformation("[WinMM] joyGetPos(uJoyID={UJoyID}, pji=0x{Pji:X8})", uJoyID, pji);
			// Return error - no joystick present
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(8)]
		private uint JoyGetThreshold(uint uJoyID, uint puThreshold)
		{
			_logger.LogInformation("[WinMM] joyGetThreshold(uJoyID={UJoyID}, puThreshold=0x{PuThreshold:X8})",
				uJoyID, puThreshold);
			// Return error - no joystick present
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(4)]
		private uint JoyReleaseCapture(uint uJoyID)
		{
			_logger.LogInformation("[WinMM] joyReleaseCapture(uJoyID={UJoyID})", uJoyID);
			// Return success - stub
			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		[DllModuleExport(16)]
		private uint JoySetCapture(uint hwnd, uint uJoyID, uint uPeriod, uint fChanged)
		{
			_logger.LogInformation("[WinMM] joySetCapture(hwnd=0x{Hwnd:X8}, uJoyID={UJoyID}, uPeriod={UPeriod}, fChanged={FChanged})",
				hwnd, uJoyID, uPeriod, fChanged);
			// Return error - no joystick present
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(8)]
		private uint JoySetThreshold(uint uJoyID, uint uThreshold)
		{
			_logger.LogInformation("[WinMM] joySetThreshold(uJoyID={UJoyID}, uThreshold={UThreshold})",
				uJoyID, uThreshold);
			// Return error - no joystick present
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		// ============================================================================
		// MCI Functions
		// ============================================================================

		[DllModuleExport(4)]
		private uint MciGetDeviceIDA(in LpcStr pszDevice)
		{
			var deviceName = pszDevice.ToString() ?? string.Empty;
			_logger.LogInformation("[WinMM] mciGetDeviceIDA(pszDevice=\"{DeviceName}\")", deviceName);
			// Return 0 (no device)
			return 0;
		}

		[DllModuleExport(16)]
		private uint MciSendCommandW(uint mciId, uint uMsg, uint dwParam1, uint dwParam2)
		{
			_logger.LogInformation("[WinMM] mciSendCommandW(mciId={MciId}, uMsg=0x{UMsg:X8}, dwParam1=0x{DwParam1:X8}, dwParam2=0x{DwParam2:X8})",
				mciId, uMsg, dwParam1, dwParam2);
			// Return error - not implemented
			return (uint)NativeTypes.MMSysError.MMSYSERR_NODRIVER;
		}

		// ============================================================================
		// MIDI In Functions
		// ============================================================================

		[DllModuleExport(0)]
		private uint MidiInGetNumDevs()
		{
			_logger.LogInformation("[WinMM] midiInGetNumDevs()");
			// Return 0 devices
			return 0;
		}

		[DllModuleExport(12)]
		private uint MidiInGetDevCapsA(uint uDeviceID, uint pmic, uint cbmic)
		{
			_logger.LogInformation("[WinMM] midiInGetDevCapsA(uDeviceID={UDeviceID}, pmic=0x{Pmic:X8}, cbmic={Cbmic})",
				uDeviceID, pmic, cbmic);
			// Return error - no devices
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(20)]
		private uint MidiInOpen(uint phmi, uint uDeviceID, uint dwCallback, uint dwInstance, uint fdwOpen)
		{
			_logger.LogInformation("[WinMM] midiInOpen(phmi=0x{Phmi:X8}, uDeviceID={UDeviceID}, dwCallback=0x{DwCallback:X8}, dwInstance=0x{DwInstance:X8}, fdwOpen=0x{FdwOpen:X8})",
				phmi, uDeviceID, dwCallback, dwInstance, fdwOpen);
			// Return error - no devices
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(4)]
		private uint MidiInClose(uint hmi)
		{
			_logger.LogInformation("[WinMM] midiInClose(hmi=0x{Hmi:X8})", hmi);
			// Return error - invalid handle
			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		[DllModuleExport(12)]
		private uint MidiInPrepareHeader(uint hmi, uint pmh, uint cbmh)
		{
			_logger.LogInformation("[WinMM] midiInPrepareHeader(hmi=0x{Hmi:X8}, pmh=0x{Pmh:X8}, cbmh={Cbmh})",
				hmi, pmh, cbmh);
			// Return error - invalid handle
			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		[DllModuleExport(12)]
		private uint MidiInUnprepareHeader(uint hmi, uint pmh, uint cbmh)
		{
			_logger.LogInformation("[WinMM] midiInUnprepareHeader(hmi=0x{Hmi:X8}, pmh=0x{Pmh:X8}, cbmh={Cbmh})",
				hmi, pmh, cbmh);
			// Return error - invalid handle
			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		[DllModuleExport(12)]
		private uint MidiInAddBuffer(uint hmi, uint pmh, uint cbmh)
		{
			_logger.LogInformation("[WinMM] midiInAddBuffer(hmi=0x{Hmi:X8}, pmh=0x{Pmh:X8}, cbmh={Cbmh})",
				hmi, pmh, cbmh);
			// Return error - invalid handle
			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		[DllModuleExport(4)]
		private uint MidiInReset(uint hmi)
		{
			_logger.LogInformation("[WinMM] midiInReset(hmi=0x{Hmi:X8})", hmi);
			// Return error - invalid handle
			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		// ============================================================================
		// MIDI Out Functions (additional)
		// ============================================================================

		[DllModuleExport(12)]
		private uint MidiOutGetDevCapsA(uint uDeviceID, uint pmoc, uint cbmoc)
		{
			_logger.LogInformation("[WinMM] midiOutGetDevCapsA(uDeviceID={UDeviceID}, pmoc=0x{Pmoc:X8}, cbmoc={Cbmoc})",
				uDeviceID, pmoc, cbmoc);
			// Return error - no devices
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(20)]
		private uint MidiOutOpen(uint phmo, uint uDeviceID, uint dwCallback, uint dwInstance, uint fdwOpen)
		{
			_logger.LogInformation("[WinMM] midiOutOpen(phmo=0x{Phmo:X8}, uDeviceID={UDeviceID}, dwCallback=0x{DwCallback:X8}, dwInstance=0x{DwInstance:X8}, fdwOpen=0x{FdwOpen:X8})",
				phmo, uDeviceID, dwCallback, dwInstance, fdwOpen);
			// Return error - no devices
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(4)]
		private uint MidiOutClose(uint hmo)
		{
			_logger.LogInformation("[WinMM] midiOutClose(hmo=0x{Hmo:X8})", hmo);
			// Return error - invalid handle
			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		[DllModuleExport(8)]
		private uint MidiOutGetVolume(uint hmo, uint pdwVolume)
		{
			_logger.LogInformation("[WinMM] midiOutGetVolume(hmo=0x{Hmo:X8}, pdwVolume=0x{PdwVolume:X8})",
				hmo, pdwVolume);
			// Return full volume
			if (pdwVolume != 0)
			{
				_env.MemWrite32(pdwVolume, 0xFFFFFFFF);
			}
			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		[DllModuleExport(12)]
		private uint MidiOutLongMsg(uint hmo, uint pmh, uint cbmh)
		{
			_logger.LogInformation("[WinMM] midiOutLongMsg(hmo=0x{Hmo:X8}, pmh=0x{Pmh:X8}, cbmh={Cbmh})",
				hmo, pmh, cbmh);
			// Return error - invalid handle
			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		[DllModuleExport(8)]
		private uint MidiOutGetID(uint hmo, uint puDeviceID)
		{
			_logger.LogInformation("[WinMM] midiOutGetID(hmo=0x{Hmo:X8}, puDeviceID=0x{PuDeviceID:X8})",
				hmo, puDeviceID);
			// Return error - invalid handle
			return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
		}

		// ============================================================================
		// MIDI Stream Functions (additional)
		// ============================================================================

		[DllModuleExport(12)]
		private uint MidiStreamPosition(uint hms, uint pmmt, uint cbmmt)
		{
			_logger.LogInformation("[WinMM] midiStreamPosition(hms=0x{Hms:X8}, pmmt=0x{Pmmt:X8}, cbmmt={Cbmmt})",
				hms, pmmt, cbmmt);

			if (!_midiStreams.ContainsKey(hms))
			{
				return (uint)NativeTypes.MMSysError.MMSYSERR_INVALHANDLE;
			}

			// Return position 0 (stub)
			if (pmmt != 0 && cbmmt >= 8)
			{
				_env.MemWrite32(pmmt, 0); // ms
				_env.MemWrite32(pmmt + 4, 0); // u.ticks or other union member
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		// ============================================================================
		// Mixer Functions (additional)
		// ============================================================================

		[DllModuleExport(12)]
		private uint MixerGetControlDetailsW(uint hmxobj, uint pmxcd, uint fdwDetails)
		{
			_logger.LogInformation("[WinMM] mixerGetControlDetailsW(hmxobj=0x{Hmxobj:X8}, pmxcd=0x{Pmxcd:X8}, fdwDetails=0x{FdwDetails:X8})",
				hmxobj, pmxcd, fdwDetails);
			// Return error - not implemented
			return (uint)NativeTypes.MMSysError.MMSYSERR_NODRIVER;
		}

		[DllModuleExport(12)]
		private uint MixerGetDevCapsA(uint uMxId, uint pmxcaps, uint cbmxcaps)
		{
			_logger.LogInformation("[WinMM] mixerGetDevCapsA(uMxId={UMxId}, pmxcaps=0x{Pmxcaps:X8}, cbmxcaps={Cbmxcaps})",
				uMxId, pmxcaps, cbmxcaps);
			// Return error - no devices
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(12)]
		private uint MixerGetDevCapsW(uint uMxId, uint pmxcaps, uint cbmxcaps)
		{
			_logger.LogInformation("[WinMM] mixerGetDevCapsW(uMxId={UMxId}, pmxcaps=0x{Pmxcaps:X8}, cbmxcaps={Cbmxcaps})",
				uMxId, pmxcaps, cbmxcaps);
			// Return error - no devices
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(12)]
		private uint MixerGetLineControlsW(uint hmxobj, uint pmxlc, uint fdwControls)
		{
			_logger.LogInformation("[WinMM] mixerGetLineControlsW(hmxobj=0x{Hmxobj:X8}, pmxlc=0x{Pmxlc:X8}, fdwControls=0x{FdwControls:X8})",
				hmxobj, pmxlc, fdwControls);
			// Return error - not implemented
			return (uint)NativeTypes.MMSysError.MMSYSERR_NODRIVER;
		}

		[DllModuleExport(12)]
		private uint MixerGetLineInfoW(uint hmxobj, uint pmxl, uint fdwInfo)
		{
			_logger.LogInformation("[WinMM] mixerGetLineInfoW(hmxobj=0x{Hmxobj:X8}, pmxl=0x{Pmxl:X8}, fdwInfo=0x{FdwInfo:X8})",
				hmxobj, pmxl, fdwInfo);
			// Return error - not implemented
			return (uint)NativeTypes.MMSysError.MMSYSERR_NODRIVER;
		}

		[DllModuleExport(0)]
		private uint MixerGetNumDevs()
		{
			_logger.LogInformation("[WinMM] mixerGetNumDevs()");
			// Return 0 devices
			return 0;
		}

		// ============================================================================
		// MMIO Functions (additional)
		// ============================================================================

		[DllModuleExport(12)]
		private uint MmioInstallIOProcA(uint fccIOProc, uint pIOProc, uint dwFlags)
		{
			_logger.LogInformation("[WinMM] mmioInstallIOProcA(fccIOProc=0x{FccIOProc:X8}, pIOProc=0x{PIOProc:X8}, dwFlags=0x{DwFlags:X8})",
				fccIOProc, pIOProc, dwFlags);
			// Return NULL - not implemented
			return 0;
		}

		[DllModuleExport(16)]
		private uint MmioSetBuffer(uint hmmio, uint pchBuffer, uint cchBuffer, uint fuBuffer)
		{
			_logger.LogInformation("[WinMM] mmioSetBuffer(hmmio=0x{Hmmio:X8}, pchBuffer=0x{PchBuffer:X8}, cchBuffer={CchBuffer}, fuBuffer=0x{FuBuffer:X8})",
				hmmio, pchBuffer, cchBuffer, fuBuffer);

			if (!_mmioFiles.ContainsKey(hmmio))
			{
				_logger.LogWarning("[WinMM] mmioSetBuffer: Invalid handle 0x{Hmmio:X8}", hmmio);
				return (uint)NativeTypes.MMIOError.MMIOERR_BASE;
			}

			// Return success - stub
			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		// ============================================================================
		// Sound Functions (additional)
		// ============================================================================

		[DllModuleExport(8)]
		private uint SndPlaySoundW(in LpcWStr pszSound, uint fuSound)
		{
			var soundName = pszSound.ToString() ?? string.Empty;
			_logger.LogInformation("[WinMM] sndPlaySoundW(pszSound=\"{SoundName}\", fuSound=0x{FuSound:X8})",
				soundName, fuSound);
			// Return success without playing - stub
			return 1; // TRUE
		}

		// ============================================================================
		// Wave Functions (additional)
		// ============================================================================

		[DllModuleExport(12)]
		private uint WaveInGetDevCapsW(uint uDeviceID, uint pwic, uint cbwic)
		{
			_logger.LogInformation("[WinMM] waveInGetDevCapsW(uDeviceID={UDeviceID}, pwic=0x{Pwic:X8}, cbwic={Cbwic})",
				uDeviceID, pwic, cbwic);
			// Return error - no devices
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(12)]
		private uint WaveInGetErrorTextA(uint mmrError, uint pszText, uint cchText)
		{
			_logger.LogInformation("[WinMM] waveInGetErrorTextA(mmrError={MmrError}, pszText=0x{PszText:X8}, cchText={CchText})",
				mmrError, pszText, cchText);

			if (pszText != 0 && cchText > 0)
			{
				var errorText = "Unknown error";
				_env.WriteAnsiStringAt(pszText, errorText);
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		[DllModuleExport(12)]
		private uint WaveOutGetDevCapsW(uint uDeviceID, uint pwoc, uint cbwoc)
		{
			_logger.LogInformation("[WinMM] waveOutGetDevCapsW(uDeviceID={UDeviceID}, pwoc=0x{Pwoc:X8}, cbwoc={Cbwoc})",
				uDeviceID, pwoc, cbwoc);
			// Return error - no devices
			return (uint)NativeTypes.MMSysError.MMSYSERR_BADDEVICEID;
		}

		[DllModuleExport(12)]
		private uint WaveOutGetErrorTextA(uint mmrError, uint pszText, uint cchText)
		{
			_logger.LogInformation("[WinMM] waveOutGetErrorTextA(mmrError={MmrError}, pszText=0x{PszText:X8}, cchText={CchText})",
				mmrError, pszText, cchText);

			if (pszText != 0 && cchText > 0)
			{
				var errorText = "Unknown error";
				_env.WriteAnsiStringAt(pszText, errorText);
			}

			return (uint)NativeTypes.MMSysError.MMSYSERR_NOERROR;
		}

		#endregion
	}
}
