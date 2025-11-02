using CHDSharpLib;
using DiscUtils;
using DiscUtils.Iso9660;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
	
	/// <summary>
	/// Gets whether this CHD file is valid and can be read.
	/// </summary>
	public bool IsValid => _isValid;
	
	/// <summary>
	/// Gets the CHD version of the file.
	/// </summary>
	public uint? Version => _chdVersion;
	
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
			
			// Validate the CHD file using CHDSharp
			var result = CHD.CheckFile(_chdStream, Path.GetFileName(chdPath), false, out _chdVersion, out _chdSHA1, out _chdMD5);
			
			if (result == chd_error.CHDERR_NONE)
			{
				_isValid = true;
				_logger.LogInformation("[ChdDiscReader] Successfully opened CHD file: {Path} (Version: {Version})", 
					chdPath, _chdVersion);
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
	/// Attempts to extract the underlying ISO filesystem from the CHD file.
	/// CHD files often contain ISO 9660 filesystems for CD-ROM games.
	/// </summary>
	/// <returns>A CDReader for the ISO filesystem, or null if not available</returns>
	public CDReader? TryGetIsoFileSystem()
	{
		if (!_isValid || _chdStream == null)
		{
			return null;
		}
		
		try
		{
			// CHD files for CD-ROM games typically contain an ISO 9660 filesystem
			// We can try to detect and open it directly from the CHD stream
			// Note: This is a simplified implementation. Full CHD support would require
			// proper block decompression and disc format handling
			
			_logger.LogInformation("[ChdDiscReader] Attempting to extract ISO filesystem from CHD");
			
			// For now, we log that this is a placeholder for full CHD support
			_logger.LogWarning("[ChdDiscReader] Full CHD decompression not yet implemented. " +
			                  "CHD files are detected but file access is limited.");
			
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[ChdDiscReader] Failed to extract ISO filesystem from CHD");
			return null;
		}
	}
	
	public void Dispose()
	{
		_chdStream?.Dispose();
		_logger.LogDebug("[ChdDiscReader] Disposed CHD reader for: {Path}", _chdPath);
	}
}
