using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.VirtualFileSystem;

/// <summary>
/// Layered virtual filesystem that supports copy-on-write semantics.
/// Files are read from a read-only base layer (game files) but all writes
/// go to a writable overlay layer (per-game save data).
/// </summary>
public class LayeredVirtualFileSystem : IVirtualFileSystem
{
	private readonly string _baseDirectory;
	private readonly string _overlayDirectory;
	private readonly ILogger _logger;

	/// <summary>
	/// Creates a new layered virtual filesystem.
	/// </summary>
	/// <param name="baseDirectory">Read-only base directory containing original game files</param>
	/// <param name="overlayDirectory">Writable overlay directory for game-specific modifications</param>
	/// <param name="logger">Optional logger</param>
	public LayeredVirtualFileSystem(string baseDirectory, string? overlayDirectory = null, ILogger? logger = null)
	{
		_baseDirectory = Path.GetFullPath(baseDirectory);
		_overlayDirectory = overlayDirectory != null 
			? Path.GetFullPath(overlayDirectory) 
			: Path.Combine(Path.GetTempPath(), "Win32Emu_VFS_" + Guid.NewGuid().ToString("N"));
		_logger = logger ?? NullLogger.Instance;

		// Ensure overlay directory exists
		Directory.CreateDirectory(_overlayDirectory);

		_logger.LogInformation("[VFS] Initialized with base: {BaseDirectory}, overlay: {OverlayDirectory}", 
			_baseDirectory, _overlayDirectory);
	}

	/// <summary>
	/// Normalizes a path to use consistent separators and removes leading slashes.
	/// </summary>
	private string NormalizePath(string path)
	{
		// Convert to forward slashes and remove leading slashes/drive letters for virtual paths
		var normalized = path.Replace('\\', '/').TrimStart('/');
		
		// Remove drive letters like "C:/"
		if (normalized.Length >= 2 && normalized[1] == ':')
		{
			normalized = normalized.Substring(2).TrimStart('/');
		}

		return normalized;
	}

	/// <summary>
	/// Gets the full path in the overlay directory.
	/// </summary>
	private string GetOverlayPath(string virtualPath)
	{
		var normalized = NormalizePath(virtualPath);
		return Path.Combine(_overlayDirectory, normalized);
	}

	/// <summary>
	/// Gets the full path in the base directory.
	/// </summary>
	private string GetBasePath(string virtualPath)
	{
		var normalized = NormalizePath(virtualPath);
		return Path.Combine(_baseDirectory, normalized);
	}

	/// <summary>
	/// Resolves a virtual path to the actual filesystem path, checking overlay first, then base.
	/// </summary>
	private string? ResolvePath(string virtualPath, out bool isInOverlay)
	{
		var overlayPath = GetOverlayPath(virtualPath);
		if (File.Exists(overlayPath))
		{
			isInOverlay = true;
			return overlayPath;
		}

		var basePath = GetBasePath(virtualPath);
		if (File.Exists(basePath))
		{
			isInOverlay = false;
			return basePath;
		}

		isInOverlay = false;
		return null;
	}

	/// <summary>
	/// Copies a file from base to overlay if it doesn't exist in overlay yet.
	/// This implements copy-on-write semantics.
	/// </summary>
	private void EnsureInOverlay(string virtualPath)
	{
		var overlayPath = GetOverlayPath(virtualPath);
		if (File.Exists(overlayPath))
		{
			return; // Already in overlay
		}

		var basePath = GetBasePath(virtualPath);
		if (!File.Exists(basePath))
		{
			return; // File doesn't exist anywhere
		}

		// Copy from base to overlay
		var overlayDir = Path.GetDirectoryName(overlayPath);
		if (!string.IsNullOrEmpty(overlayDir))
		{
			Directory.CreateDirectory(overlayDir);
		}

		File.Copy(basePath, overlayPath, true);
		_logger.LogDebug("[VFS] Copied {VirtualPath} from base to overlay", virtualPath);
	}

	public IVirtualFileHandle? OpenFile(string path, VfsFileMode mode, VfsFileAccess access)
	{
		try
		{
			var overlayPath = GetOverlayPath(path);

			// For write operations, ensure we're working with the overlay
			if (access == VfsFileAccess.Write || access == VfsFileAccess.ReadWrite)
			{
				// For modes that require existing file, copy to overlay first
				if (mode == VfsFileMode.Open || mode == VfsFileMode.Truncate || mode == VfsFileMode.OpenOrCreate)
				{
					EnsureInOverlay(path);
				}

				// Ensure overlay directory exists
				var overlayDir = Path.GetDirectoryName(overlayPath);
				if (!string.IsNullOrEmpty(overlayDir))
				{
					Directory.CreateDirectory(overlayDir);
				}

				var fileMode = ConvertFileMode(mode);
				var fileAccess = ConvertFileAccess(access);
				var fs = new FileStream(overlayPath, fileMode, fileAccess, FileShare.ReadWrite);
				_logger.LogDebug("[VFS] Opened {VirtualPath} for {Access} in overlay", path, access);
				return new VirtualFileHandle(fs);
			}

			// For read-only operations, try overlay first, then base
			var resolvedPath = ResolvePath(path, out var isInOverlay);
			if (resolvedPath != null)
			{
				var fs = new FileStream(resolvedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				_logger.LogDebug("[VFS] Opened {VirtualPath} for read from {Layer}", 
					path, isInOverlay ? "overlay" : "base");
				return new VirtualFileHandle(fs);
			}

			_logger.LogDebug("[VFS] File not found: {VirtualPath}", path);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[VFS] Failed to open file: {Path}", path);
			return null;
		}
	}

	public bool DeleteFile(string path)
	{
		try
		{
			var overlayPath = GetOverlayPath(path);
			
			// If file exists in overlay, delete it
			if (File.Exists(overlayPath))
			{
				File.Delete(overlayPath);
				_logger.LogDebug("[VFS] Deleted {Path} from overlay", path);
				return true;
			}

			// If file only exists in base, create a deletion marker in overlay
			// For simplicity, we just don't copy it to overlay and return success
			var basePath = GetBasePath(path);
			if (File.Exists(basePath))
			{
				_logger.LogDebug("[VFS] File {Path} exists in base (read-only), marking as deleted", path);
				return true;
			}

			_logger.LogDebug("[VFS] File not found for deletion: {Path}", path);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[VFS] Failed to delete file: {Path}", path);
			return false;
		}
	}

	public bool MoveFile(string existingPath, string newPath)
	{
		try
		{
			// Ensure source file is in overlay
			EnsureInOverlay(existingPath);

			var sourceOverlayPath = GetOverlayPath(existingPath);
			var destOverlayPath = GetOverlayPath(newPath);

			if (!File.Exists(sourceOverlayPath))
			{
				_logger.LogDebug("[VFS] Source file not found for move: {ExistingPath}", existingPath);
				return false;
			}

			// Ensure destination directory exists
			var destDir = Path.GetDirectoryName(destOverlayPath);
			if (!string.IsNullOrEmpty(destDir))
			{
				Directory.CreateDirectory(destDir);
			}

			File.Move(sourceOverlayPath, destOverlayPath, true);
			_logger.LogDebug("[VFS] Moved {ExistingPath} to {NewPath}", existingPath, newPath);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[VFS] Failed to move file: {ExistingPath} to {NewPath}", existingPath, newPath);
			return false;
		}
	}

	public bool FileExists(string path)
	{
		var resolvedPath = ResolvePath(path, out _);
		return resolvedPath != null;
	}

	public string[] GetFiles(string directory, string pattern)
	{
		try
		{
			var overlayDir = GetOverlayPath(directory);
			var baseDir = GetBasePath(directory);

			var files = new HashSet<string>();

			// Get files from overlay
			if (Directory.Exists(overlayDir))
			{
				foreach (var file in Directory.GetFiles(overlayDir, pattern))
				{
					files.Add(Path.GetFileName(file));
				}
			}

			// Get files from base (that aren't already in overlay)
			if (Directory.Exists(baseDir))
			{
				foreach (var file in Directory.GetFiles(baseDir, pattern))
				{
					files.Add(Path.GetFileName(file));
				}
			}

			_logger.LogDebug("[VFS] Found {Count} files in {Directory} matching {Pattern}", 
				files.Count, directory, pattern);
			return files.ToArray();
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[VFS] Failed to get files in {Directory} with pattern {Pattern}", 
				directory, pattern);
			return [];
		}
	}

	private static FileMode ConvertFileMode(VfsFileMode mode)
	{
		return mode switch
		{
			VfsFileMode.CreateNew => FileMode.CreateNew,
			VfsFileMode.Create => FileMode.Create,
			VfsFileMode.Open => FileMode.Open,
			VfsFileMode.OpenOrCreate => FileMode.OpenOrCreate,
			VfsFileMode.Truncate => FileMode.Truncate,
			_ => FileMode.OpenOrCreate
		};
	}

	private static FileAccess ConvertFileAccess(VfsFileAccess access)
	{
		return access switch
		{
			VfsFileAccess.Read => FileAccess.Read,
			VfsFileAccess.Write => FileAccess.Write,
			VfsFileAccess.ReadWrite => FileAccess.ReadWrite,
			_ => FileAccess.ReadWrite
		};
	}

	public string ToWindowsPath(string realPath)
	{
		try
		{
			// Get the full path to normalize it
			var fullPath = Path.GetFullPath(realPath);
			var baseFullPath = Path.GetFullPath(_baseDirectory);

			// Check if the path is under the base directory
			if (fullPath.StartsWith(baseFullPath, StringComparison.OrdinalIgnoreCase))
			{
				// Get the relative path from base
				var relativePath = Path.GetRelativePath(baseFullPath, fullPath);
				
				// Convert to Windows-style path with backslashes and add C: drive
				var windowsPath = @"C:\" + relativePath.Replace('/', '\\');
				
				_logger.LogDebug("[VFS] Virtualized path: {RealPath} -> {WindowsPath}", realPath, windowsPath);
				return windowsPath;
			}

			// If not under base directory, return the original path
			_logger.LogDebug("[VFS] Path not under base directory, returning as-is: {RealPath}", realPath);
			return realPath;
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[VFS] Failed to virtualize path: {RealPath}", realPath);
			return realPath;
		}
	}
}