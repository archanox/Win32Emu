using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// WAVMIX32.DLL module - provides wave audio mixing functionality.
/// This was a middleware library used in older games for mixing multiple wave sounds.
/// </summary>
public class Wavmix32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Wavmix32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "WAVMIX32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "WAVEMIXACTIVATE":
				returnValue = WaveMixActivate(a.UInt32(0), a.UInt32(1));
				return true;
			case "WAVEMIXCLOSECHANNEL":
				returnValue = WaveMixCloseChannel(a.UInt32(0), a.Int32(1));
				return true;
			case "WAVEMIXCLOSESESSION":
				returnValue = WaveMixCloseSession(a.UInt32(0));
				return true;
			case "WAVEMIXFLUSHCHANNEL":
				returnValue = WaveMixFlushChannel(a.UInt32(0), a.Int32(1), a.UInt32(2));
				return true;
			case "WAVEMIXFREEWAVE":
				returnValue = WaveMixFreeWave(a.UInt32(0), a.UInt32(1));
				return true;
			case "WAVEMIXINIT":
				returnValue = WaveMixInit(a.UInt32(0));
				return true;
			case "WAVEMIXOPENCHANNEL":
				returnValue = WaveMixOpenChannel(a.UInt32(0), a.Int32(1), a.UInt32(2));
				return true;
			case "WAVEMIXOPENWAVE":
				returnValue = WaveMixOpenWave(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "WAVEMIXPLAY":
				returnValue = WaveMixPlay(a.UInt32(0));
				return true;
			case "WAVEMIXPUMP":
				returnValue = WaveMixPump();
				return true;

			default:
				_logger.LogInformation("[Wavmix32] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Activates or deactivates a WaveMix session.
	/// UINT WaveMixActivate(HANDLE hMixSession, BOOL fActivate);
	/// </summary>
	[DllModuleExport(8)]
	private uint WaveMixActivate(uint hMixSession, uint fActivate)
	{
		_logger.LogInformation("[Wavmix32] WaveMixActivate(hMixSession=0x{HMixSession:X8}, fActivate={FActivate})", hMixSession, fActivate);
		return 0; // Success
	}

	/// <summary>
	/// Closes a channel in a WaveMix session.
	/// UINT WaveMixCloseChannel(HANDLE hMixSession, int iChannel);
	/// </summary>
	[DllModuleExport(8)]
	private uint WaveMixCloseChannel(uint hMixSession, int iChannel)
	{
		_logger.LogInformation("[Wavmix32] WaveMixCloseChannel(hMixSession=0x{HMixSession:X8}, iChannel={IChannel})", hMixSession, iChannel);
		return 0; // Success
	}

	/// <summary>
	/// Closes a WaveMix session and releases all associated resources.
	/// UINT WaveMixCloseSession(HANDLE hMixSession);
	/// </summary>
	[DllModuleExport(4)]
	private uint WaveMixCloseSession(uint hMixSession)
	{
		_logger.LogInformation("[Wavmix32] WaveMixCloseSession(hMixSession=0x{HMixSession:X8})", hMixSession);
		return 0; // Success
	}

	/// <summary>
	/// Flushes all pending sounds from a channel.
	/// UINT WaveMixFlushChannel(HANDLE hMixSession, int iChannel, DWORD dwFlags);
	/// </summary>
	[DllModuleExport(12)]
	private uint WaveMixFlushChannel(uint hMixSession, int iChannel, uint dwFlags)
	{
		_logger.LogInformation("[Wavmix32] WaveMixFlushChannel(hMixSession=0x{HMixSession:X8}, iChannel={IChannel}, dwFlags=0x{DwFlags:X})",
			hMixSession, iChannel, dwFlags);
		return 0; // Success
	}

	/// <summary>
	/// Frees a wave sound loaded with WaveMixOpenWave.
	/// UINT WaveMixFreeWave(HANDLE hMixSession, LPWAVE lpWave);
	/// </summary>
	[DllModuleExport(8)]
	private uint WaveMixFreeWave(uint hMixSession, uint lpWave)
	{
		_logger.LogInformation("[Wavmix32] WaveMixFreeWave(hMixSession=0x{HMixSession:X8}, lpWave=0x{LpWave:X8})", hMixSession, lpWave);
		return 0; // Success
	}

	/// <summary>
	/// Initializes a WaveMix session.
	/// HANDLE WaveMixInit(LPWAVEMIXINFO lpMixInfo);
	/// </summary>
	[DllModuleExport(4)]
	private uint WaveMixInit(uint lpMixInfo)
	{
		_logger.LogInformation("[Wavmix32] WaveMixInit(lpMixInfo=0x{LpMixInfo:X8})", lpMixInfo);
		// Return a dummy session handle
		return 0x0AE00001; // Dummy session handle
	}

	/// <summary>
	/// Opens a playback channel in a WaveMix session.
	/// UINT WaveMixOpenChannel(HANDLE hMixSession, int iChannel, DWORD dwFlags);
	/// </summary>
	[DllModuleExport(12)]
	private uint WaveMixOpenChannel(uint hMixSession, int iChannel, uint dwFlags)
	{
		_logger.LogInformation("[Wavmix32] WaveMixOpenChannel(hMixSession=0x{HMixSession:X8}, iChannel={IChannel}, dwFlags=0x{DwFlags:X})",
			hMixSession, iChannel, dwFlags);
		return 0; // Success
	}

	/// <summary>
	/// Opens and loads a wave file for playback.
	/// LPWAVE WaveMixOpenWave(HANDLE hMixSession, LPCSTR lpszWaveFilename, HINSTANCE hInst, DWORD dwFlags);
	/// </summary>
	[DllModuleExport(16)]
	private uint WaveMixOpenWave(uint hMixSession, in LpcStr lpszWaveFilename, uint hInst, uint dwFlags)
	{
		var waveFilename = lpszWaveFilename.ToString() ?? string.Empty;
		_logger.LogInformation("[Wavmix32] WaveMixOpenWave(hMixSession=0x{HMixSession:X8}, lpszWaveFilename=\"{WaveFilename}\", hInst=0x{HInst:X8}, dwFlags=0x{DwFlags:X})",
			hMixSession, waveFilename, hInst, dwFlags);
		// Return a dummy wave handle
		return 0x00010000 | (uint)(waveFilename.GetHashCode() & 0xFFFF);
	}

	/// <summary>
	/// Plays a wave sound on a channel.
	/// UINT WaveMixPlay(LPWAVEMIXPLAYPARAMS lpPlayParams);
	/// </summary>
	[DllModuleExport(4)]
	private uint WaveMixPlay(uint lpPlayParams)
	{
		_logger.LogInformation("[Wavmix32] WaveMixPlay(lpPlayParams=0x{LpPlayParams:X8})", lpPlayParams);
		return 0; // Success
	}

	/// <summary>
	/// Processes pending wave mixing operations.
	/// UINT WaveMixPump(void);
	/// </summary>
	[DllModuleExport(0)]
	private uint WaveMixPump()
	{
		_logger.LogInformation("[Wavmix32] WaveMixPump()");
		return 0; // Success
	}
}
