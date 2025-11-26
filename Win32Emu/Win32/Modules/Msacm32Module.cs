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

	// WAVEFORMATEX structure constants
	private const ushort WAVE_FORMAT_PCM = 0x0001;
	private const ushort WAVE_FORMAT_ADPCM = 0x0002;
	private const ushort WAVE_FORMAT_ALAW = 0x0006;
	private const ushort WAVE_FORMAT_MULAW = 0x0007;

	private class AcmStream
	{
		public uint Handle { get; set; }
		public uint SourceFormat { get; set; }
		public uint DestFormat { get; set; }
		public uint Filter { get; set; }
		public uint Callback { get; set; }
		public uint Instance { get; set; }
		public uint Flags { get; set; }
		public WaveFormat? SourceWaveFormat { get; set; }
		public WaveFormat? DestWaveFormat { get; set; }
	}

	private class WaveFormat
	{
		public ushort FormatTag { get; set; }
		public ushort Channels { get; set; }
		public uint SamplesPerSec { get; set; }
		public uint AvgBytesPerSec { get; set; }
		public ushort BlockAlign { get; set; }
		public ushort BitsPerSample { get; set; }
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
	[DllModuleExport(1)]
	private uint AcmStreamOpen(uint phas, uint had, uint pwfxSrc, uint pwfxDst, uint pwfltr, uint dwCallback, uint dwInstance, uint fdwOpen)
	{
		_logger.LogInformation("[MSACM32] acmStreamOpen(phas=0x{Phas:X8}, had=0x{Had:X8}, pwfxSrc=0x{PwfxSrc:X8}, pwfxDst=0x{PwfxDst:X8}, pwfltr=0x{Pwfltr:X8}, dwCallback=0x{DwCallback:X8}, dwInstance=0x{DwInstance:X8}, fdwOpen=0x{FdwOpen:X8})",
			phas, had, pwfxSrc, pwfxDst, pwfltr, dwCallback, dwInstance, fdwOpen);

		if (phas == 0)
		{
			_logger.LogWarning("[MSACM32] acmStreamOpen: NULL stream handle pointer");
			return 11; // MMSYSERR_INVALPARAM
		}

		// Initialize audio backend if not already done
		if (_env.AudioBackend == null && _env.BackendFactory != null)
		{
			_env.AudioBackend = _env.BackendFactory.CreateAudioBackend(_logger);
			_env.AudioBackend.Initialize();
		}

		// Parse source wave format (WAVEFORMATEX structure)
		WaveFormat? srcFormat = null;
		if (pwfxSrc != 0)
		{
			srcFormat = ParseWaveFormat(pwfxSrc);
			_logger.LogInformation("[MSACM32] Source format: {FormatTag}, {Channels}ch, {SamplesPerSec}Hz, {BitsPerSample}bit",
				srcFormat.FormatTag, srcFormat.Channels, srcFormat.SamplesPerSec, srcFormat.BitsPerSample);
		}

		// Parse destination wave format
		WaveFormat? dstFormat = null;
		if (pwfxDst != 0)
		{
			dstFormat = ParseWaveFormat(pwfxDst);
			_logger.LogInformation("[MSACM32] Dest format: {FormatTag}, {Channels}ch, {SamplesPerSec}Hz, {BitsPerSample}bit",
				dstFormat.FormatTag, dstFormat.Channels, dstFormat.SamplesPerSec, dstFormat.BitsPerSample);
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
			Flags = fdwOpen,
			SourceWaveFormat = srcFormat,
			DestWaveFormat = dstFormat
		};

		_acmStreams[handle] = stream;

		// Write the handle to the output parameter
		_env.MemWrite32(phas, handle);

		_logger.LogInformation("[MSACM32] acmStreamOpen: Created stream handle 0x{Handle:X8} with audio backend support", handle);
		return 0; // MMSYSERR_NOERROR
	}

	/// <summary>
	/// Parses a WAVEFORMATEX structure from memory
	/// </summary>
	private WaveFormat ParseWaveFormat(uint address)
	{
		var waveFormat = new WaveFormatExRef(_env.Memory, address);
		return new WaveFormat
		{
			FormatTag = waveFormat.wFormatTag,
			Channels = waveFormat.nChannels,
			SamplesPerSec = waveFormat.nSamplesPerSec,
			AvgBytesPerSec = waveFormat.nAvgBytesPerSec,
			BlockAlign = waveFormat.nBlockAlign,
			BitsPerSample = waveFormat.wBitsPerSample
		};
	}

	/// <summary>
	/// Closes an ACM conversion stream.
	/// MMRESULT acmStreamClose(
	///   HACMSTREAM has,
	///   DWORD      fdwClose
	/// );
	/// </summary>
	[DllModuleExport(2)]
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
	[DllModuleExport(3)]
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
	[DllModuleExport(4)]
	private uint AcmStreamConvert(uint has, uint pash, uint fdwConvert)
	{
		_logger.LogInformation("[MSACM32] acmStreamConvert(has=0x{Has:X8}, pash=0x{Pash:X8}, fdwConvert=0x{FdwConvert:X8})",
			has, pash, fdwConvert);

		if (!_acmStreams.TryGetValue(has, out var stream))
		{
			_logger.LogWarning("[MSACM32] acmStreamConvert: Invalid stream handle 0x{Has:X8}", has);
			return 6; // MMSYSERR_INVALHANDLE
		}

		if (pash == 0)
		{
			_logger.LogWarning("[MSACM32] acmStreamConvert: NULL stream header pointer");
			return 11; // MMSYSERR_INVALPARAM
		}

		try
		{
			// Parse ACMSTREAMHEADER structure
			// typedef struct {
			//   DWORD cbStruct;          // +0
			//   DWORD fdwStatus;         // +4
			//   DWORD_PTR dwUser;        // +8
			//   LPBYTE pbSrc;            // +12
			//   DWORD cbSrcLength;       // +16
			//   DWORD cbSrcLengthUsed;   // +20
			//   DWORD_PTR dwSrcUser;     // +24
			//   LPBYTE pbDst;            // +28
			//   DWORD cbDstLength;       // +32
			//   DWORD cbDstLengthUsed;   // +36
			//   ...
			// } ACMSTREAMHEADER;

			var header = new AcmStreamHeaderRef(_env.Memory, pash);

			_logger.LogInformation("[MSACM32] Converting {CbSrcLength} bytes from 0x{PbSrc:X8} to 0x{PbDst:X8} (max {CbDstLength} bytes)",
				header.cbSrcLength, header.pbSrc, header.pbDst, header.cbDstLength);

			// Perform format conversion based on source and dest formats
			uint bytesConverted = 0;
			if (stream.SourceWaveFormat != null && stream.DestWaveFormat != null)
			{
				bytesConverted = ConvertAudioData(
					header.pbSrc, header.cbSrcLength,
					header.pbDst, header.cbDstLength,
					stream.SourceWaveFormat,
					stream.DestWaveFormat);
			}
			else
			{
				// Fallback: copy data directly (no conversion)
				bytesConverted = Math.Min(header.cbSrcLength, header.cbDstLength);
				for (uint i = 0; i < bytesConverted; i++)
				{
					_env.MemWrite8(header.pbDst + i, _env.MemRead8(header.pbSrc + i));
				}
			}

			// Update the stream header with bytes used
			header.cbSrcLengthUsed = header.cbSrcLength;
			header.cbDstLengthUsed = bytesConverted;

			_logger.LogInformation("[MSACM32] Converted {BytesConverted} bytes", bytesConverted);
			return 0; // MMSYSERR_NOERROR
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[MSACM32] acmStreamConvert: Conversion failed");
			return 1; // MMSYSERR_ERROR
		}
	}

	/// <summary>
	/// Converts audio data between formats (basic PCM conversion)
	/// </summary>
	private uint ConvertAudioData(uint srcAddr, uint srcLen, uint dstAddr, uint dstLen,
		WaveFormat srcFormat, WaveFormat dstFormat)
	{
		// Only support PCM to PCM conversion for now
		if (srcFormat.FormatTag != WAVE_FORMAT_PCM || dstFormat.FormatTag != WAVE_FORMAT_PCM)
		{
			_logger.LogWarning("[MSACM32] Unsupported format conversion: {SrcFormatTag} -> {DstFormatTag}",
				srcFormat.FormatTag, dstFormat.FormatTag);
			// Fallback: copy data directly
			var copyLen = Math.Min(srcLen, dstLen);
			for (uint i = 0; i < copyLen; i++)
			{
				_env.MemWrite8(dstAddr + i, _env.MemRead8(srcAddr + i));
			}
			return copyLen;
		}

		// Calculate conversion parameters
		var srcBytesPerSample = srcFormat.BitsPerSample / 8;
		var dstBytesPerSample = dstFormat.BitsPerSample / 8;
		var srcSampleCount = srcLen / (uint)(srcBytesPerSample * srcFormat.Channels);

		// For simplicity, assume same sample rate (no resampling)
		// In production, you'd need proper resampling for different sample rates
		var dstSampleCount = srcSampleCount;
		var dstBytesNeeded = dstSampleCount * (uint)(dstBytesPerSample * dstFormat.Channels);

		if (dstBytesNeeded > dstLen)
		{
			_logger.LogWarning("[MSACM32] Destination buffer too small: need {DstBytesNeeded}, have {DstLen}",
				dstBytesNeeded, dstLen);
			dstSampleCount = dstLen / (uint)(dstBytesPerSample * dstFormat.Channels);
			dstBytesNeeded = dstSampleCount * (uint)(dstBytesPerSample * dstFormat.Channels);
		}

		// Perform simple PCM conversion (bit depth and channel conversion)
		for (uint i = 0; i < dstSampleCount; i++)
		{
			for (var ch = 0; ch < Math.Min(srcFormat.Channels, dstFormat.Channels); ch++)
			{
				var srcOffset = srcAddr + (i * (uint)(srcBytesPerSample * srcFormat.Channels)) + (uint)(ch * srcBytesPerSample);
				var dstOffset = dstAddr + (i * (uint)(dstBytesPerSample * dstFormat.Channels)) + (uint)(ch * dstBytesPerSample);

				// Read source sample
				int sample = 0;
				if (srcFormat.BitsPerSample == 8)
				{
					sample = _env.MemRead8(srcOffset) - 128; // 8-bit is unsigned, convert to signed
					sample <<= 8; // Scale to 16-bit range
				}
				else if (srcFormat.BitsPerSample == 16)
				{
					sample = (short)_env.MemRead16(srcOffset);
				}

				// Write destination sample
				if (dstFormat.BitsPerSample == 8)
				{
					var val = (byte)((sample >> 8) + 128); // Scale to 8-bit unsigned
					_env.MemWrite8(dstOffset, val);
				}
				else if (dstFormat.BitsPerSample == 16)
				{
					_env.MemWrite16(dstOffset, (ushort)sample);
				}
			}
		}

		_logger.LogInformation("[MSACM32] Converted {SrcSampleCount} samples ({SrcFormat} -> {DstFormat})",
			dstSampleCount, $"{srcFormat.BitsPerSample}bit/{srcFormat.Channels}ch", $"{dstFormat.BitsPerSample}bit/{dstFormat.Channels}ch");

		return dstBytesNeeded;
	}

	/// <summary>
	/// Resets an ACM stream.
	/// MMRESULT acmStreamReset(
	///   HACMSTREAM has,
	///   DWORD      fdwReset
	/// );
	/// </summary>
	[DllModuleExport(5)]
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
	[DllModuleExport(6)]
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
	[DllModuleExport(7)]
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
	[DllModuleExport(8)]
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
	[DllModuleExport(9)]
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
	[DllModuleExport(10)]
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
