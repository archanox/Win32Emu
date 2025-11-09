using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

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
			_cpu = cpu;
			_memory = memory;
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

				case "MCISENDCOMMANDA":
					returnValue = MciSendCommandA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
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

				case "MIXERSETCONTROLDETAILS":
					returnValue = MixerSetControlDetails(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "WAVEOUTGETNUMDEVS":
					returnValue = WaveOutGetNumDevs();
					return true;

				case "WAVEOUTGETDEVCAPSA":
					returnValue = WaveOutGetDevCapsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
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

				case "SNDPLAYSOUND":
					returnValue = SndPlaySound(a.LpcStr(0), a.UInt32(1));
					return true;

				case "SNDPLAYSOUNDA":
					returnValue = SndPlaySoundA(a.LpcStr(0), a.UInt32(1));
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
			return 0; // TIMERR_NOERROR
		}

		[DllModuleExport(1)]
		private uint TimeSetEvent(uint uDelay, uint uResolution, uint lpTimeProc, uint dwUser, uint fuEvent)
		{
			// TimeSetEvent sets a timer event
			// Returns a timer identifier or 0 if it failed
			_logger.LogInformation("[WinMM] timeSetEvent(delay={UDelay}, resolution={UResolution}, callback=0x{LpTimeProc:X8})", uDelay, uResolution, lpTimeProc);

			// Return a synthetic timer ID
			return 0x1000 + uDelay; // Simple unique ID based on delay
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
			if (_env.AudioBackend == null)
			{
				_env.AudioBackend = Rendering.BackendFactory.CreateAudioBackend(_logger);
				_env.AudioBackend.Initialize();
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

			// For now, most APIs use synchronous implementation
			// TODO: When timer callbacks are fully implemented, use async version
			if (TryInvokeUnsafe(export, cpu, memory, out var syncReturnValue))
			{
				return (true, syncReturnValue);
			}

			// No async work performed; return failure immediately
			return (false, 0);
		}

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

			// Execute until we hit the return address with cancellation support
			const int YIELD_INTERVAL = 10000;
			var steps = 0;
			var executionSuccessful = true;
			var lastCheckEip = _cpu.GetEip();
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
							_logger.LogInformation("[WinMM] CallTimeProcAsync: Cancellation requested at step {Steps}", steps);
							executionSuccessful = false;
							break;
						}

						// Yield to allow other async operations to proceed
						await Task.Yield();
					}

					var eip = _cpu.GetEip();

					// Check if we've returned to our marker address
					if (eip == RETURN_ADDRESS)
					{
						break;
					}

					// Check for invalid EIP (NULL pointer execution)
					if (eip == 0x00000000)
					{
						_logger.LogWarning("[WinMM] CallTimeProcAsync: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting");
						executionSuccessful = false;
						break;
					}

					// Check for other invalid low addresses
					if (eip < MINIMUM_VALID_EIP && eip != RETURN_ADDRESS)
					{
						_logger.LogError("[WinMM] CallTimeProcAsync: Execution jumped to invalid low address 0x{Eip:X8}", eip);
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
								_logger.LogWarning("[WinMM] CallTimeProcAsync: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting",
									currentEip, stuckCounter);
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

					// Execute one instruction
					_cpu.SingleStep(_memory);
					steps++;

					// Periodically yield for cooperative multitasking
					if (steps % YIELD_INTERVAL == 0)
					{
						await Task.Yield();
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[WinMM] CallTimeProcAsync: Exception during execution: {ExMessage}", ex.Message);
				executionSuccessful = false;
			}

			// Restore CPU state
			_cpu.SetEip(savedEip);
			_cpu.SetRegister("ESP", savedEsp);
			_cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[WinMM] CallTimeProcAsync: Completed {Status}", executionSuccessful ? "successfully" : "with errors");
		}

		#endregion
	}
}
