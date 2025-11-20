using CHDSharpLib;
using CHDSharpLib.Utils;
using DiscUtils;
using DiscUtils.Iso9660;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace Win32Emu.VirtualFileSystem;

/// <summary>
/// CHD (Compressed Hunks of Data) disc image reader that provides access to CHD files.
/// CHD is a compressed disc image format developed for MAME that supports Redbook audio.
/// </summary>
public class ChdDiscReader : IDisposable
{
	private readonly ILogger _logger;
	private readonly Stream _chdStream;
	private readonly string _chdPath;
	private bool _isValid;
	private uint? _chdVersion;
	private byte[]? _chdSHA1;
	private byte[]? _chdMD5;
	private CHDHeader? _header;
	private CdToc? _toc;
	private ChdBlockStream? _blockStream;
	
	/// <summary>
	/// Gets whether this CHD file is valid and can be read.
	/// </summary>
	public bool IsValid => _isValid;
	
	/// <summary>
	/// Gets the CHD version of the file.
	/// </summary>
	public uint? Version => _chdVersion;
	
	/// <summary>
	/// Gets the table of contents for the CD-ROM disc, if available.
	/// </summary>
	public CdToc? Toc => _toc;
	
	/// <summary>
	/// Opens a CHD disc image file for reading.
	/// </summary>
	/// <param name="chdPath">Path to the CHD file</param>
	/// <param name="logger">Optional logger</param>
	public ChdDiscReader(string chdPath, ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
		_chdPath = chdPath;
		
		if (!File.Exists(chdPath))
		{
			throw new FileNotFoundException($"CHD file not found: {chdPath}");
		}
		
		try
		{
			// Open the CHD file stream
			_chdStream = File.Open(chdPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			
			// Validate the CHD file header
			var result = CHD.CheckFile(_chdStream, Path.GetFileName(chdPath), false, out _chdVersion, out _chdSHA1, out _chdMD5);
			
			if (result == chd_error.CHDERR_NONE)
			{
				_isValid = true;
				_logger.LogInformation("[ChdDiscReader] Successfully validated CHD file: {Path} (Version: {Version})", 
					chdPath, _chdVersion);
				
				// Read the full CHD header for block decompression
				_chdStream.Seek(0, SeekOrigin.Begin);
				result = ReadChdHeader(_chdStream, _chdVersion!.Value, out _header);
				
				if (result == chd_error.CHDERR_NONE && _header != null)
				{
					_logger.LogInformation("[ChdDiscReader] CHD Header: {BlockSize} byte blocks, {TotalBlocks} blocks, {TotalBytes} total bytes",
						_header.blocksize, _header.totalblocks, _header.totalbytes);
					
					// Read CD-ROM metadata if available
					ReadCdMetadata();
					
					// Create block stream for decompression
					_blockStream = new ChdBlockStream(_chdStream, _header, _logger);
				}
				else
				{
					_isValid = false;
					_logger.LogWarning("[ChdDiscReader] Failed to read CHD header: {Error}", result);
				}
			}
			else
			{
				_isValid = false;
				_logger.LogWarning("[ChdDiscReader] CHD file validation failed: {Path} (Error: {Error})", 
					chdPath, result);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[ChdDiscReader] Failed to open CHD file: {Path}", chdPath);
			_chdStream?.Dispose();
			throw;
		}
	}
	
	/// <summary>
	/// Reads the CHD header for the specified version.
	/// </summary>
	private chd_error ReadChdHeader(Stream stream, uint version, out CHDHeader? header)
	{
		header = null;
		stream.Seek(0, SeekOrigin.Begin);
		
		return version switch
		{
			1 => CHDHeaders.ReadHeaderV1(stream, out header),
			2 => CHDHeaders.ReadHeaderV2(stream, out header),
			3 => CHDHeaders.ReadHeaderV3(stream, out header),
			4 => CHDHeaders.ReadHeaderV4(stream, out header),
			5 => CHDHeaders.ReadHeaderV5(stream, out header),
			_ => chd_error.CHDERR_UNSUPPORTED_VERSION
		};
	}
	
	/// <summary>
	/// Reads CD-ROM metadata from the CHD file to build the table of contents.
	/// </summary>
	private void ReadCdMetadata()
	{
		if (_header == null || _header.metaoffset == 0)
		{
			_logger.LogDebug("[ChdDiscReader] No metadata found in CHD file");
			return;
		}

		try
		{
			_toc = new CdToc();
			ulong metaOffset = _header.metaoffset;
			
			using var br = new BinaryReader(_chdStream, Encoding.UTF8, true);
			
			while (metaOffset != 0)
			{
				_chdStream.Seek((long)metaOffset, SeekOrigin.Begin);
				
				uint metaTag = br.ReadUInt32BE();
				uint metaLength = br.ReadUInt32BE();
				ulong metaNext = br.ReadUInt64BE();
				uint metaFlags = metaLength >> 24;
				metaLength &= 0x00ffffff;
				
				byte[] metaData = new byte[metaLength];
				_chdStream.ReadExactly(metaData, 0, metaData.Length);
				
				// Convert tag to string for easier comparison
				string tagStr = $"{(char)((metaTag >> 24) & 0xFF)}{(char)((metaTag >> 16) & 0xFF)}{(char)((metaTag >> 8) & 0xFF)}{(char)(metaTag & 0xFF)}";
				
				_logger.LogDebug("[ChdDiscReader] Metadata tag: {Tag}, Length: {Length}", tagStr, metaLength);
				
				// Parse CD-ROM track metadata (GDDD = GD-ROM, CHTR = CD track)
				if (tagStr == "CHTR")
				{
					ParseCdTrackMetadata(metaData);
				}
				
				metaOffset = metaNext;
			}
			
			if (_toc.Tracks.Count > 0)
			{
				_logger.LogInformation("[ChdDiscReader] Parsed {Count} tracks from CHD metadata", _toc.Tracks.Count);
				_logger.LogDebug("[ChdDiscReader] TOC:\n{Toc}", _toc.ToString());
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "[ChdDiscReader] Failed to read CD metadata");
			_toc = null;
		}
	}
	
	/// <summary>
	/// Parses CD track metadata from CHD metadata block.
	/// Format: "TRACK:%d TYPE:%s SUBTYPE:%s FRAMES:%d"
	/// </summary>
	private void ParseCdTrackMetadata(byte[] metaData)
	{
		try
		{
			string metaStr = Encoding.ASCII.GetString(metaData).TrimEnd('\0');
			var parts = metaStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			
			var track = new CdTrackInfo();
			
			foreach (var part in parts)
			{
				var kv = part.Split(':', 2);
				if (kv.Length != 2)
				{
					continue;
				}

				switch (kv[0])
				{
					case "TRACK":
						track.TrackNumber = int.Parse(kv[1]);
						break;
					case "TYPE":
						track.TrackType = ParseTrackType(kv[1]);
						track.FrameSize = GetFrameSizeForTrackType(track.TrackType);
						break;
					case "SUBTYPE":
						track.SubType = ParseSubType(kv[1]);
						break;
					case "FRAMES":
						track.FrameCount = int.Parse(kv[1]);
						break;
					case "PREGAP":
						track.Pregap = int.Parse(kv[1]);
						break;
					case "POSTGAP":
						track.Postgap = int.Parse(kv[1]);
						break;
				}
			}
			
			// Calculate start frame based on previous tracks
			if (_toc!.Tracks.Count > 0)
			{
				var lastTrack = _toc.Tracks[^1];
				track.StartFrame = lastTrack.StartFrame + lastTrack.FrameCount;
			}
			else
			{
				track.StartFrame = track.Pregap; // First track starts after its pregap
			}
			
			_toc.Tracks.Add(track);
			_toc.TotalFrames = track.StartFrame + track.FrameCount;
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "[ChdDiscReader] Failed to parse track metadata");
		}
	}
	
	private static CdTrackType ParseTrackType(string type)
	{
		return type switch
		{
			"MODE1" => CdTrackType.Mode1,
			"MODE1_RAW" or "MODE1/2352" => CdTrackType.Mode1Raw,
			"MODE2" => CdTrackType.Mode2,
			"MODE2_FORMLESS" or "MODE2/2336" => CdTrackType.Mode2Formless,
			"MODE2_FORM1" or "MODE2/2048" => CdTrackType.Mode2Form1,
			"MODE2_FORM2" or "MODE2/2324" => CdTrackType.Mode2Form2,
			"MODE2_RAW" or "MODE2/2352" => CdTrackType.Mode2Raw,
			"AUDIO" => CdTrackType.Audio,
			_ => CdTrackType.Mode1
		};
	}
	
	private static CdTrackSubType ParseSubType(string subType)
	{
		return subType switch
		{
			"RW" or "RW_RAW" => CdTrackSubType.Raw,
			"COOKED" => CdTrackSubType.Cooked,
			_ => CdTrackSubType.None
		};
	}
	
	private static int GetFrameSizeForTrackType(CdTrackType trackType)
	{
		return trackType switch
		{
			CdTrackType.Mode1 => 2048,
			CdTrackType.Mode1Raw => 2352,
			CdTrackType.Mode2 => 2336,
			CdTrackType.Mode2Formless => 2336,
			CdTrackType.Mode2Form1 => 2048,
			CdTrackType.Mode2Form2 => 2324,
			CdTrackType.Mode2Raw => 2352,
			CdTrackType.Audio => 2352, // CD-DA audio: 2352 bytes per frame (44.1kHz stereo)
			_ => 2048
		};
	}
	
	/// <summary>
	/// Attempts to extract the underlying ISO filesystem from the CHD file.
	/// CHD files often contain ISO 9660 filesystems for CD-ROM games.
	/// </summary>
	/// <returns>A CDReader for the ISO filesystem, or null if not available</returns>
	public CDReader? TryGetIsoFileSystem()
	{
		if (!_isValid || _blockStream == null)
		{
			return null;
		}
		
		try
		{
			_logger.LogInformation("[ChdDiscReader] Extracting ISO filesystem from CHD");
			
			// Reset stream position
			_blockStream.Seek(0, SeekOrigin.Begin);
			
			// Try to detect and open ISO 9660 filesystem from the decompressed stream
			// The ISO filesystem should be in the first data track
			if (CDReader.Detect(_blockStream))
			{
				_blockStream.Seek(0, SeekOrigin.Begin);
				var cdReader = new CDReader(_blockStream, true, true);
				_logger.LogInformation("[ChdDiscReader] Successfully extracted ISO 9660 filesystem");
				return cdReader;
			}
			
			_logger.LogWarning("[ChdDiscReader] No ISO 9660 filesystem detected in CHD");
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[ChdDiscReader] Failed to extract ISO filesystem from CHD");
			return null;
		}
	}
	
	/// <summary>
	/// Gets a stream to read decompressed data from the CHD file.
	/// </summary>
	/// <returns>A stream for reading decompressed CHD data, or null if not available</returns>
	public Stream? GetDataStream()
	{
		return _blockStream;
	}
	
	/// <summary>
	/// Reads audio data from a specific CD track.
	/// </summary>
	/// <param name="trackNumber">Track number (1-based)</param>
	/// <param name="startFrame">Starting frame within the track</param>
	/// <param name="frameCount">Number of frames to read</param>
	/// <returns>Audio data in raw CD-DA format (2352 bytes per frame, 16-bit stereo PCM at 44.1kHz)</returns>
	public byte[]? ReadAudioTrack(int trackNumber, int startFrame, int frameCount)
	{
		if (_toc == null || _blockStream == null)
		{
			_logger.LogWarning("[ChdDiscReader] Cannot read audio: No TOC or block stream available");
			return null;
		}
		
		var track = _toc.Tracks.FirstOrDefault(t => t.TrackNumber == trackNumber);
		if (track == null)
		{
			_logger.LogWarning("[ChdDiscReader] Track {TrackNumber} not found", trackNumber);
			return null;
		}
		
		if (track.TrackType != CdTrackType.Audio)
		{
			_logger.LogWarning("[ChdDiscReader] Track {TrackNumber} is not an audio track", trackNumber);
			return null;
		}
		
		// Calculate absolute position in the stream
		long absoluteFrame = track.StartFrame + startFrame;
		long position = absoluteFrame * track.FrameSize;
		int bytesToRead = frameCount * track.FrameSize;
		
		try
		{
			_blockStream.Seek(position, SeekOrigin.Begin);
			byte[] audioData = new byte[bytesToRead];
			int bytesRead = _blockStream.Read(audioData, 0, bytesToRead);
			
			if (bytesRead < bytesToRead)
			{
				_logger.LogWarning("[ChdDiscReader] Only read {BytesRead} of {BytesToRead} bytes from track {TrackNumber}",
					bytesRead, bytesToRead, trackNumber);
				Array.Resize(ref audioData, bytesRead);
			}
			
			return audioData;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[ChdDiscReader] Failed to read audio from track {TrackNumber}", trackNumber);
			return null;
		}
	}
	
	public void Dispose()
	{
		_blockStream?.Dispose();
		_chdStream?.Dispose();
		_logger.LogDebug("[ChdDiscReader] Disposed CHD reader for: {Path}", _chdPath);
	}
}
