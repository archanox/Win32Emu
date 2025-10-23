using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// MSACM32.DLL module - provides Audio Compression Manager (ACM) functions.
/// </summary>
public class Msacm32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Msacm32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "MSACM32.DLL";

	// ACM stream handle tracking
	private readonly Dictionary<uint, AcmStream> _acmStreams = new();
	private uint _nextAcmStreamHandle = 0x70000000;

	private class AcmStream
	{
		public uint Handle { get; set; }
		public uint SourceFormat { get; set; }
		public uint DestFormat { get; set; }
		public uint Filter { get; set; }
		public uint Callback { get; set; }
		public uint Instance { get; set; }
		public uint Flags { get; set; }
	}

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "ACMSTREAMOPEN":
				returnValue = AcmStreamOpen(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7));
				return true;

			case "ACMSTREAMCLOSE":
				returnValue = AcmStreamClose(a.UInt32(0), a.UInt32(1));
				return true;

			case "ACMSTREAMSIZE":
				returnValue = AcmStreamSize(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "ACMSTREAMCONVERT":
				returnValue = AcmStreamConvert(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "ACMSTREAMRESET":
				returnValue = AcmStreamReset(a.UInt32(0), a.UInt32(1));
				return true;

			case "ACMFORMATTAGDETAILSA":
				returnValue = AcmFormatTagDetailsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "ACMFORMATDETAILSA":
				returnValue = AcmFormatDetailsA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "ACMFORMATCHOOSEA":
				returnValue = AcmFormatChooseA(a.UInt32(0));
				return true;

			case "ACMFORMATENUMA":
				returnValue = AcmFormatEnumA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;

			case "ACMMETRICS":
				returnValue = AcmMetrics(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			default:
				_logger.LogInformation("[MSACM32] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Opens an Audio Compression Manager (ACM) conversion stream.
	/// MMRESULT acmStreamOpen(
	///   LPHACMSTREAM  phas,
	///   HACMDRIVER    had,
	///   LPWAVEFORMATEX pwfxSrc,
	///   LPWAVEFORMATEX pwfxDst,
	///   LPWAVEFILTER  pwfltr,
	///   DWORD_PTR     dwCallback,
	///   DWORD_PTR     dwInstance,
	///   DWORD         fdwOpen
	/// );
	/// </summary>
	[DllModuleExport(32)]
	private uint AcmStreamOpen(uint phas, uint had, uint pwfxSrc, uint pwfxDst, uint pwfltr, uint dwCallback, uint dwInstance, uint fdwOpen)
	{
		_logger.LogInformation("[MSACM32] acmStreamOpen(phas=0x{Phas:X8}, had=0x{Had:X8}, pwfxSrc=0x{PwfxSrc:X8}, pwfxDst=0x{PwfxDst:X8}, pwfltr=0x{Pwfltr:X8}, dwCallback=0x{DwCallback:X8}, dwInstance=0x{DwInstance:X8}, fdwOpen=0x{FdwOpen:X8})",
			phas, had, pwfxSrc, pwfxDst, pwfltr, dwCallback, dwInstance, fdwOpen);

		if (phas == 0)
		{
			_logger.LogWarning("[MSACM32] acmStreamOpen: NULL stream handle pointer");
			return 11; // MMSYSERR_INVALPARAM
		}

		// Create a handle for this ACM stream
		var handle = _nextAcmStreamHandle++;
		var stream = new AcmStream
		{
			Handle = handle,
			SourceFormat = pwfxSrc,
			DestFormat = pwfxDst,
			Filter = pwfltr,
			Callback = dwCallback,
			Instance = dwInstance,
			Flags = fdwOpen
		};

		_acmStreams[handle] = stream;

		// Write the handle to the output parameter
		_env.MemWrite32(phas, handle);

		_logger.LogInformation("[MSACM32] acmStreamOpen: Created stream handle 0x{Handle:X8}", handle);
		return 0; // MMSYSERR_NOERROR
	}

	/// <summary>
	/// Closes an ACM conversion stream.
	/// MMRESULT acmStreamClose(
	///   HACMSTREAM has,
	///   DWORD      fdwClose
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint AcmStreamClose(uint has, uint fdwClose)
	{
		_logger.LogInformation("[MSACM32] acmStreamClose(has=0x{Has:X8}, fdwClose=0x{FdwClose:X8})", has, fdwClose);

		if (_acmStreams.ContainsKey(has))
		{
			_acmStreams.Remove(has);
			_logger.LogInformation("[MSACM32] acmStreamClose: Closed stream handle 0x{Has:X8}", has);
			return 0; // MMSYSERR_NOERROR
		}

		_logger.LogWarning("[MSACM32] acmStreamClose: Invalid stream handle 0x{Has:X8}", has);
		return 6; // MMSYSERR_INVALHANDLE
	}

	/// <summary>
	/// Returns a recommended size for a source or destination buffer on an ACM stream.
	/// MMRESULT acmStreamSize(
	///   HACMSTREAM has,
	///   DWORD      cbInput,
	///   LPDWORD    pdwOutputBytes,
	///   DWORD      fdwSize
	/// );
	/// </summary>
	[DllModuleExport(16)]
	private uint AcmStreamSize(uint has, uint cbInput, uint pdwOutputBytes, uint fdwSize)
	{
		_logger.LogInformation("[MSACM32] acmStreamSize(has=0x{Has:X8}, cbInput={CbInput}, pdwOutputBytes=0x{PdwOutputBytes:X8}, fdwSize=0x{FdwSize:X8})",
			has, cbInput, pdwOutputBytes, fdwSize);

		if (!_acmStreams.ContainsKey(has))
		{
			_logger.LogWarning("[MSACM32] acmStreamSize: Invalid stream handle 0x{Has:X8}", has);
			return 6; // MMSYSERR_INVALHANDLE
		}

		// For stub implementation, assume 1:1 size ratio
		if (pdwOutputBytes != 0)
		{
			_env.MemWrite32(pdwOutputBytes, cbInput);
		}

		return 0; // MMSYSERR_NOERROR
	}

	/// <summary>
	/// Converts audio data from one format to another.
	/// MMRESULT acmStreamConvert(
	///   HACMSTREAM      has,
	///   LPACMSTREAMHEADER pash,
	///   DWORD           fdwConvert
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint AcmStreamConvert(uint has, uint pash, uint fdwConvert)
	{
		_logger.LogInformation("[MSACM32] acmStreamConvert(has=0x{Has:X8}, pash=0x{Pash:X8}, fdwConvert=0x{FdwConvert:X8})",
			has, pash, fdwConvert);

		if (!_acmStreams.ContainsKey(has))
		{
			_logger.LogWarning("[MSACM32] acmStreamConvert: Invalid stream handle 0x{Has:X8}", has);
			return 6; // MMSYSERR_INVALHANDLE
		}

		// For stub implementation, just mark as complete
		// A full implementation would actually convert the audio data
		return 0; // MMSYSERR_NOERROR
	}

	/// <summary>
	/// Resets an ACM stream.
	/// MMRESULT acmStreamReset(
	///   HACMSTREAM has,
	///   DWORD      fdwReset
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint AcmStreamReset(uint has, uint fdwReset)
	{
		_logger.LogInformation("[MSACM32] acmStreamReset(has=0x{Has:X8}, fdwReset=0x{FdwReset:X8})", has, fdwReset);

		if (!_acmStreams.ContainsKey(has))
		{
			_logger.LogWarning("[MSACM32] acmStreamReset: Invalid stream handle 0x{Has:X8}", has);
			return 6; // MMSYSERR_INVALHANDLE
		}

		return 0; // MMSYSERR_NOERROR
	}

	/// <summary>
	/// Retrieves details about a specific format tag.
	/// MMRESULT acmFormatTagDetailsA(
	///   HACMDRIVER          had,
	///   LPACMFORMATTAGDETAILS paftd,
	///   DWORD               fdwDetails
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint AcmFormatTagDetailsA(uint had, uint paftd, uint fdwDetails)
	{
		_logger.LogInformation("[MSACM32] acmFormatTagDetailsA(had=0x{Had:X8}, paftd=0x{Paftd:X8}, fdwDetails=0x{FdwDetails:X8})",
			had, paftd, fdwDetails);

		// For stub implementation, just return success
		return 0; // MMSYSERR_NOERROR
	}

	/// <summary>
	/// Retrieves details about a specific format.
	/// MMRESULT acmFormatDetailsA(
	///   HACMDRIVER        had,
	///   LPACMFORMATDETAILS pafd,
	///   DWORD             fdwDetails
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint AcmFormatDetailsA(uint had, uint pafd, uint fdwDetails)
	{
		_logger.LogInformation("[MSACM32] acmFormatDetailsA(had=0x{Had:X8}, pafd=0x{Pafd:X8}, fdwDetails=0x{FdwDetails:X8})",
			had, pafd, fdwDetails);

		// For stub implementation, just return success
		return 0; // MMSYSERR_NOERROR
	}

	/// <summary>
	/// Creates a dialog for selecting an audio format.
	/// MMRESULT acmFormatChooseA(
	///   LPACMFORMATCHOOSE pafmtc
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint AcmFormatChooseA(uint pafmtc)
	{
		_logger.LogInformation("[MSACM32] acmFormatChooseA(pafmtc=0x{Pafmtc:X8})", pafmtc);

		// For stub implementation, return user cancelled
		return 515; // ACMERR_CANCELED
	}

	/// <summary>
	/// Enumerates available audio formats.
	/// MMRESULT acmFormatEnumA(
	///   HACMDRIVER        had,
	///   LPACMFORMATDETAILS pafd,
	///   ACMFORMATENUMCB   fnCallback,
	///   DWORD_PTR         dwInstance,
	///   DWORD             fdwEnum
	/// );
	/// </summary>
	[DllModuleExport(20)]
	private uint AcmFormatEnumA(uint had, uint pafd, uint fnCallback, uint dwInstance, uint fdwEnum)
	{
		_logger.LogInformation("[MSACM32] acmFormatEnumA(had=0x{Had:X8}, pafd=0x{Pafd:X8}, fnCallback=0x{FnCallback:X8}, dwInstance=0x{DwInstance:X8}, fdwEnum=0x{FdwEnum:X8})",
			had, pafd, fnCallback, dwInstance, fdwEnum);

		// For stub implementation, don't enumerate any formats (just return success)
		return 0; // MMSYSERR_NOERROR
	}

	/// <summary>
	/// Retrieves ACM metrics.
	/// MMRESULT acmMetrics(
	///   HACMOBJ hao,
	///   UINT    uMetric,
	///   LPVOID  pMetric
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint AcmMetrics(uint hao, uint uMetric, uint pMetric)
	{
		_logger.LogInformation("[MSACM32] acmMetrics(hao=0x{Hao:X8}, uMetric={UMetric}, pMetric=0x{PMetric:X8})",
			hao, uMetric, pMetric);

		// For stub implementation, return 0 for all metrics
		if (pMetric != 0)
		{
			_env.MemWrite32(pMetric, 0);
		}

		return 0; // MMSYSERR_NOERROR
	}
}
