using DiscUtils;
using DiscUtils.Fat;
using DiscUtils.Iso9660;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VhdDisk = DiscUtils.Vhd.Disk;
using VhdxDisk = DiscUtils.Vhdx.Disk;
using VmdkDisk = DiscUtils.Vmdk.Disk;

namespace Win32Emu.VirtualFileSystem;

/// <summary>
/// Virtual file system that uses DiscUtils to provide a virtual disk (VMDK/VHD/VHDX/ISO).
/// This allows for a complete C: drive emulation with FAT filesystem support.
/// </summary>
public class DiskVirtualFileSystem : IVirtualFileSystem, IDisposable
{
	private readonly ILogger _logger;
	private readonly VirtualDisk? _disk;
	private readonly DiscFileSystem _fileSystem;
	
	/// <summary>
	/// Gets whether this disk is read-only
	/// </summary>
	public bool IsReadOnly { get; }
	private readonly Dictionary<string, Stream> _openFiles = new();

	/// <summary>
	/// Opens an existing virtual disk or ISO file.
	/// </summary>
	/// <param name="diskPath">Path to the disk file (VMDK/VHD/VHDX/ISO)</param>
	/// <param name="logger">Optional logger</param>
	public DiskVirtualFileSystem(string diskPath, ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
		var extension = Path.GetExtension(diskPath).ToLowerInvariant();

		try
		{
			switch (extension)
			{
				case ".iso":
					var isoStream = File.Open(diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
					_fileSystem = new CDReader(isoStream, true);
					IsReadOnly = true;
					_logger.LogInformation("[DiskVFS] Mounted ISO: {DiskPath}", diskPath);
					break;

				case ".vmdk":
					try
					{
						_disk = new VmdkDisk(diskPath, FileAccess.ReadWrite);
					}
					catch (UnauthorizedAccessException)
					{
						_disk = new VmdkDisk(diskPath, FileAccess.Read);
						IsReadOnly = true;
					}
					_fileSystem = GetFileSystemFromDisk(_disk);
					_logger.LogInformation("[DiskVFS] Mounted VMDK: {DiskPath} ({Mode})", diskPath, IsReadOnly ? "Read-Only" : "Read-Write");
					break;

				case ".vhd":
					try
					{
						_disk = new VhdDisk(diskPath, FileAccess.ReadWrite);
					}
					catch (UnauthorizedAccessException)
					{
						_disk = new VhdDisk(diskPath, FileAccess.Read);
						IsReadOnly = true;
					}
					_fileSystem = GetFileSystemFromDisk(_disk);
					_logger.LogInformation("[DiskVFS] Mounted VHD: {DiskPath} ({Mode})", diskPath, IsReadOnly ? "Read-Only" : "Read-Write");
					break;

				case ".vhdx":
					try
					{
						_disk = new VhdxDisk(diskPath, FileAccess.ReadWrite);
					}
					catch (UnauthorizedAccessException)
					{
						_disk = new VhdxDisk(diskPath, FileAccess.Read);
						IsReadOnly = true;
					}
					_fileSystem = GetFileSystemFromDisk(_disk);
					_logger.LogInformation("[DiskVFS] Mounted VHDX: {DiskPath} ({Mode})", diskPath, IsReadOnly ? "Read-Only" : "Read-Write");
					break;

				default:
					throw new NotSupportedException($"Unsupported disk format: {extension}");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[DiskVFS] Failed to mount disk: {DiskPath}", diskPath);
			throw;
		}
	}

	/// <summary>
	/// Creates a new virtual disk with the specified format and size, pre-formatted with FAT32.
	/// </summary>
	/// <param name="diskPath">Path where the disk file will be created</param>
	/// <param name="format">Disk format (VMDK/VHD/VHDX)</param>
	/// <param name="sizeBytes">Size of the disk in bytes</param>
	/// <param name="logger">Optional logger</param>
	public static DiskVirtualFileSystem Create(string diskPath, DiskFormat format, long sizeBytes, ILogger? logger = null)
	{
		logger ??= NullLogger.Instance;

		try
		{
			// Ensure parent directory exists
			var directory = Path.GetDirectoryName(diskPath);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// Create the disk using DiscUtils based on format
			switch (format)
			{
				case DiskFormat.Vhd:
					CreateVhdDisk(diskPath, sizeBytes, logger);
					break;
				
				case DiskFormat.Vhdx:
					CreateVhdxDisk(diskPath, sizeBytes, logger);
					break;
				
				case DiskFormat.Vmdk:
					CreateVmdkDisk(diskPath, sizeBytes, logger);
					break;
				
				default:
					throw new NotSupportedException($"Unsupported disk format: {format}");
			}

			logger.LogInformation("[DiskVFS] Created and formatted {Format} disk: {DiskPath} ({SizeBytes} bytes)", 
				format, diskPath, sizeBytes);

			// Return a new instance that opens the created disk
			return new DiskVirtualFileSystem(diskPath, logger);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "[DiskVFS] Failed to create disk: {DiskPath}", diskPath);
			throw;
		}
	}

	private static void CreateVhdDisk(string diskPath, long sizeBytes, ILogger logger)
	{
		using (Stream vhdStream = File.Create(diskPath))
		{
			// Default block size for dynamic VHDs is 2MB
			long blockSize = 2 * 1024 * 1024;
			VhdDisk.InitializeDynamic(vhdStream, Ownership.None, sizeBytes, blockSize);
			
			// Re-open the disk to format it
			using (var disk = new VhdDisk(diskPath, FileAccess.ReadWrite))
			{
				BiosPartitionTable.Initialize(disk, WellKnownPartitionType.WindowsFat);
				
				using (FatFileSystem.FormatPartition(disk, 0, null))
				{
					// Disk is now formatted and ready to use
					logger.LogDebug("[DiskVFS] VHD disk formatted with FAT32");
				}
			}
		}
	}

	private static void CreateVhdxDisk(string diskPath, long sizeBytes, ILogger logger)
	{
		using (Stream vhdxStream = File.Create(diskPath))
		{
			// VHDX uses 1MB block size by default
			long blockSize = 1 * 1024 * 1024;
			VhdxDisk.InitializeDynamic(vhdxStream, Ownership.None, sizeBytes, blockSize);
			
			// Re-open the disk to format it
			using (var disk = new VhdxDisk(diskPath, FileAccess.ReadWrite))
			{
				BiosPartitionTable.Initialize(disk, WellKnownPartitionType.WindowsFat);
				
				using (FatFileSystem.FormatPartition(disk, 0, null))
				{
					// Disk is now formatted and ready to use
					logger.LogDebug("[DiskVFS] VHDX disk formatted with FAT32");
				}
			}
		}
	}

	private static void CreateVmdkDisk(string diskPath, long sizeBytes, ILogger logger)
	{
		throw new NotSupportedException("VMDK disk creation is not supported.");
	}

	/// <summary>
	/// Copies a directory and all its contents into the virtual disk.
	/// </summary>
	/// <param name="sourcePath">Source directory path on the host filesystem</param>
	/// <param name="targetPath">Target path in the virtual disk (e.g., "/" or "/games")</param>
	public void CopyDirectoryIn(string sourcePath, string targetPath = "/")
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException("Cannot copy files into a read-only disk (ISO)");
		}

		if (!Directory.Exists(sourcePath))
		{
			throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");
		}

		// Normalize target path
		targetPath = NormalizePath(targetPath);
		if (!targetPath.StartsWith('/'))
		{
			targetPath = "/" + targetPath;
		}

		// Ensure target directory exists
		if (!_fileSystem.DirectoryExists(targetPath))
		{
			_fileSystem.CreateDirectory(targetPath);
		}

		_logger.LogInformation("[DiskVFS] Copying directory {SourcePath} to {TargetPath}", sourcePath, targetPath);
		CopyDirectoryRecursive(sourcePath, targetPath);
	}

	private void CopyDirectoryRecursive(string sourceDir, string targetDir)
	{
		// Copy all files in current directory
		foreach (var file in Directory.GetFiles(sourceDir))
		{
			var fileName = Path.GetFileName(file);
			var targetPath = CombinePaths(targetDir, fileName);

			using var sourceStream = File.OpenRead(file);
			using var targetStream = _fileSystem.OpenFile(targetPath, FileMode.Create, FileAccess.Write);
			sourceStream.CopyTo(targetStream);

			_logger.LogDebug("[DiskVFS] Copied file: {TargetPath}", targetPath);
		}

		// Recursively copy subdirectories
		foreach (var dir in Directory.GetDirectories(sourceDir))
		{
			var dirName = Path.GetFileName(dir);
			var targetPath = CombinePaths(targetDir, dirName);

			_fileSystem.CreateDirectory(targetPath);
			CopyDirectoryRecursive(dir, targetPath);
		}
	}

	private static DiscFileSystem GetFileSystemFromDisk(VirtualDisk disk)
	{
		// Use the disk's content stream directly
		var diskStream = disk.Content;

		// Try to detect filesystem
		if (FatFileSystem.Detect(diskStream))
		{
			return new FatFileSystem(diskStream);
		}

		throw new InvalidOperationException("No supported filesystem found on disk. Use Create() to format a new disk.");
	}

	private string NormalizePath(string path)
	{
		// Convert Windows-style paths to Unix-style
		var normalized = path.Replace('\\', '/');

		// Remove drive letters
		if (normalized.Length >= 2 && normalized[1] == ':')
		{
			normalized = normalized.Substring(2);
		}

		// Ensure leading slash
		if (!normalized.StartsWith('/'))
		{
			normalized = "/" + normalized;
		}

		// Remove double slashes efficiently using regex
		normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "/+", "/");

		return normalized;
	}

	private string CombinePaths(string basePath, string childPath)
	{
		basePath = basePath.TrimEnd('/');
		childPath = childPath.TrimStart('/');
		var combined = basePath + "/" + childPath;
		return NormalizePath(combined);
	}

	public IVirtualFileHandle? OpenFile(string path, VfsFileMode mode, VfsFileAccess access)
	{
		if (IsReadOnly && (access == VfsFileAccess.Write || access == VfsFileAccess.ReadWrite))
		{
			_logger.LogDebug("[DiskVFS] Cannot write to read-only disk: {Path}", path);
			return null;
		}

		try
		{
			var normalizedPath = NormalizePath(path);
			var fileMode = ConvertFileMode(mode);
			var fileAccess = ConvertFileAccess(access);

			if (!_fileSystem.FileExists(normalizedPath) && fileMode == FileMode.Open)
			{
				_logger.LogDebug("[DiskVFS] File not found: {Path}", path);
				return null;
			}

			var stream = _fileSystem.OpenFile(normalizedPath, fileMode, fileAccess);
			_openFiles[normalizedPath] = stream;
			_logger.LogDebug("[DiskVFS] Opened file: {Path}", path);
			return new DiskFileHandle(stream, normalizedPath, this);
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[DiskVFS] Failed to open file: {Path}", path);
			return null;
		}
	}

	public bool DeleteFile(string path)
	{
		if (IsReadOnly)
		{
			_logger.LogDebug("[DiskVFS] Cannot delete from read-only disk: {Path}", path);
			return false;
		}

		try
		{
			var normalizedPath = NormalizePath(path);
			if (!_fileSystem.FileExists(normalizedPath))
			{
				return false;
			}

			_fileSystem.DeleteFile(normalizedPath);
			_logger.LogDebug("[DiskVFS] Deleted file: {Path}", path);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[DiskVFS] Failed to delete file: {Path}", path);
			return false;
		}
	}

	public bool MoveFile(string existingPath, string newPath)
	{
		if (IsReadOnly)
		{
			_logger.LogDebug("[DiskVFS] Cannot move files on read-only disk");
			return false;
		}

		try
		{
			var normalizedExisting = NormalizePath(existingPath);
			var normalizedNew = NormalizePath(newPath);

			if (!_fileSystem.FileExists(normalizedExisting))
			{
				return false;
			}

			_fileSystem.MoveFile(normalizedExisting, normalizedNew);
			_logger.LogDebug("[DiskVFS] Moved file: {ExistingPath} -> {NewPath}", existingPath, newPath);
			return true;
		}
		catch (UnauthorizedAccessException ex)
		{
			_logger.LogDebug(ex, "[DiskVFS] Failed to move file (unauthorized): {ExistingPath} -> {NewPath}", existingPath, newPath);
			return false;
		}
		catch (IOException ex)
		{
			_logger.LogDebug(ex, "[DiskVFS] Failed to move file (IO error): {ExistingPath} -> {NewPath}", existingPath, newPath);
			return false;
		}
		catch (ArgumentException ex)
		{
			_logger.LogDebug(ex, "[DiskVFS] Failed to move file (argument error): {ExistingPath} -> {NewPath}", existingPath, newPath);
			return false;
		}
		catch (NotSupportedException ex)
		{
			_logger.LogDebug(ex, "[DiskVFS] Failed to move file (not supported): {ExistingPath} -> {NewPath}", existingPath, newPath);
			return false;
		}
	}

	public bool FileExists(string path)
	{
		try
		{
			var normalizedPath = NormalizePath(path);
			return _fileSystem.FileExists(normalizedPath);
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[DiskVFS] Failed to check if file exists: {Path}", path);
			return false;
		}
	}

	public string[] GetFiles(string directory, string pattern)
	{
		try
		{
			var normalizedDir = NormalizePath(directory);
			if (!_fileSystem.DirectoryExists(normalizedDir))
			{
				return [];
			}

			var files = _fileSystem.GetFiles(normalizedDir, pattern, SearchOption.TopDirectoryOnly);
			var fileNames = files.Select(f => Path.GetFileName(f)).ToArray();

			_logger.LogDebug("[DiskVFS] Found {Count} files in {Directory} matching {Pattern}", 
				fileNames.Length, directory, pattern);
			return fileNames;
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[DiskVFS] Failed to get files in {Directory}", directory);
			return [];
		}
	}

	public string ToWindowsPath(string realPath)
	{
		// For disk-based VFS, paths are already virtual
		// Just ensure they have C: prefix
		var normalized = NormalizePath(realPath);
		return "C:" + normalized.Replace('/', '\\');
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

	internal void CloseFile(string normalizedPath)
	{
		_openFiles.Remove(normalizedPath);
	}

	public void Dispose()
	{
		// Close all open files
		foreach (var stream in _openFiles.Values)
		{
			stream.Dispose();
		}
		_openFiles.Clear();

		// Dispose filesystem and disk
		_fileSystem?.Dispose();
		_disk?.Dispose();

		_logger.LogInformation("[DiskVFS] Disposed");
	}
}

/// <summary>
/// Disk format for virtual disk creation.
/// </summary>
public enum DiskFormat
{
	Vmdk,
	Vhd,
	Vhdx
}

/// <summary>
/// File handle for disk-based virtual filesystem.
/// </summary>
internal class DiskFileHandle : IVirtualFileHandle
{
	private readonly Stream _stream;
	private readonly string _normalizedPath;
	private readonly DiskVirtualFileSystem _owner;

	public DiskFileHandle(Stream stream, string normalizedPath, DiskVirtualFileSystem owner)
	{
		_stream = stream;
		_normalizedPath = normalizedPath;
		_owner = owner;
	}

	public int Read(byte[] buffer, int offset, int count)
	{
		return _stream.Read(buffer, offset, count);
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		_stream.Write(buffer, offset, count);
	}

	public long Seek(long offset, SeekOrigin origin)
	{
		return _stream.Seek(offset, origin);
	}

	public long Position => _stream.Position;

	public void SetLength(long length)
	{
		_stream.SetLength(length);
	}

	public void Flush()
	{
		_stream.Flush();
	}

	public void Dispose()
	{
		_stream.Dispose();
		_owner.CloseFile(_normalizedPath);
	}
}
