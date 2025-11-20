using DiscUtils;
using DiscUtils.Fat;
using DiscUtils.Iso9660;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VhdDisk = DiscUtils.Vhd.Disk;
using VhdxDisk = DiscUtils.Vhdx.Disk;

namespace Win32Emu.VirtualFileSystem;

/// <summary>
/// Virtual file system that uses LTRData.DiscUtils to provide a virtual disk (VHD/VHDX/ISO).
/// This allows for a complete C: drive emulation with FAT filesystem support, including long filenames.
/// </summary>
public class DiskVirtualFileSystem : IVirtualFileSystem, IDisposable
{
	private readonly ILogger _logger;
	private readonly VirtualDisk? _disk;
	private readonly DiscFileSystem _fileSystem;
	private readonly Stream? _underlyingStream; // Keep stream alive for ISO/CHD files
	
	/// <summary>
	/// Gets whether this disk is read-only
	/// </summary>
	public bool IsReadOnly { get; }
	private readonly Dictionary<string, Stream> _openFiles = new();

	/// <summary>
	/// Opens an existing virtual disk or ISO file.
	/// </summary>
	/// <param name="diskPath">Path to the disk file (VHD/VHDX/ISO/CHD)</param>
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
					_underlyingStream = File.Open(diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
					_fileSystem = new CDReader(_underlyingStream, true);
					IsReadOnly = true;
					_logger.LogInformation("[DiskVFS] Mounted ISO: {DiskPath}", diskPath);
					break;

				case ".chd":
					// CHD (Compressed Hunks of Data) format used by MAME for disc images
					// CHD files are always read-only
					var chdReader = new ChdDiscReader(diskPath, _logger);
					if (!chdReader.IsValid)
					{
						chdReader.Dispose();
						throw new InvalidOperationException($"Invalid or unsupported CHD file: {diskPath}");
					}
					
					// Try to extract ISO filesystem from CHD
					var chdIsoFs = chdReader.TryGetIsoFileSystem();
					if (chdIsoFs != null)
					{
						_fileSystem = chdIsoFs;
						_logger.LogInformation("[DiskVFS] Mounted CHD with ISO filesystem: {DiskPath} (Version: {Version}, TOC: {Tracks} tracks)", 
							diskPath, chdReader.Version, chdReader.Toc?.Tracks.Count ?? 0);
					}
					else
					{
						// CHD detected but no ISO filesystem found
						_logger.LogWarning("[DiskVFS] CHD file opened but no ISO filesystem detected: {DiskPath}", diskPath);
						chdReader.Dispose();
						throw new NotSupportedException($"CHD file opened successfully but no ISO 9660 filesystem detected: {diskPath}");
					}
					IsReadOnly = true;
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
	/// <param name="format">Disk format (VHD/VHDX)</param>
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
		// Create and initialize the VHD file
		using (Stream vhdStream = File.Create(diskPath))
		{
			VhdDisk.InitializeDynamic(vhdStream, Ownership.None, sizeBytes);
		}
		
		// Re-open the disk to format it (must be done after stream is closed)
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

	private static void CreateVhdxDisk(string diskPath, long sizeBytes, ILogger logger)
	{
		// Create and initialize the VHDX file
		using (Stream vhdxStream = File.Create(diskPath))
		{
			VhdxDisk.InitializeDynamic(vhdxStream, Ownership.None, sizeBytes);
		}
		
		// Re-open the disk to format it (must be done after stream is closed)
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

	/// <summary>
	/// Copies a directory and all its contents into the virtual disk.
	/// </summary>
	/// <param name="sourcePath">Source directory path on the host filesystem</param>
	/// <param name="targetPath">Target path in the virtual disk (e.g., "/" or "/games")</param>
	public void CopyDirectoryIn(string sourcePath, string targetPath = "/")
	{
		CopyDirectoryIn(sourcePath, targetPath, null);
	}

	/// <summary>
	/// Copy a directory from the host filesystem into the virtual disk with progress reporting
	/// </summary>
	/// <param name="sourcePath">Source directory on host filesystem</param>
	/// <param name="targetPath">Target path in virtual filesystem</param>
	/// <param name="progress">Optional progress reporter for file copy operations</param>
	public void CopyDirectoryIn(string sourcePath, string targetPath, IProgress<(string fileName, int filesCopied, int totalFiles, long bytesCopied, long totalBytes)>? progress)
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
		// NormalizePath already ensures leading separator, no need to check again

		// Ensure target directory exists
		if (!_fileSystem.DirectoryExists(targetPath))
		{
			_fileSystem.CreateDirectory(targetPath);
		}

		_logger.LogInformation("[DiskVFS] Starting copy: {SourcePath} -> {TargetPath}", sourcePath, targetPath);
		
		// Count files and calculate total size if progress reporting is enabled
		var fileCount = 0;
		var totalBytes = 0L;
		if (progress != null)
		{
			var allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
			fileCount = allFiles.Length;
			totalBytes = allFiles.Sum(f => new FileInfo(f).Length);
			_logger.LogInformation("[DiskVFS] Found {FileCount} files ({TotalBytes} bytes) to copy", fileCount, totalBytes);
		}
		
		var state = new CopyProgressState { TotalFiles = fileCount, TotalBytes = totalBytes };
		CopyDirectoryRecursive(sourcePath, targetPath, progress, state);
		
		_logger.LogInformation("[DiskVFS] Successfully completed copying directory to virtual disk: {TargetPath}", targetPath);
	}

	/// <summary>
	/// Internal state class for tracking copy progress across recursive calls.
	/// Note: This class is not thread-safe. Instances should only be used within a single copy operation
	/// and should not be shared across threads.
	/// </summary>
	private class CopyProgressState
	{
		public int FilesCopied { get; set; }
		public long BytesCopied { get; set; }
		public int TotalFiles { get; set; }
		public long TotalBytes { get; set; }
	}

	private void CopyDirectoryRecursive(string sourceDir, string targetDir)
	{
		CopyDirectoryRecursive(sourceDir, targetDir, null, new CopyProgressState());
	}

	private void CopyDirectoryRecursive(string sourceDir, string targetDir, IProgress<(string fileName, int filesCopied, int totalFiles, long bytesCopied, long totalBytes)>? progress, CopyProgressState state)
	{
		// Copy all files in current directory
		foreach (var file in Directory.GetFiles(sourceDir))
		{
			var fileName = Path.GetFileName(file);
			var targetPath = CombinePaths(targetDir, fileName);

			using var sourceStream = File.OpenRead(file);
			var fileSize = sourceStream.Length;
			using var targetStream = _fileSystem.OpenFile(targetPath, FileMode.Create, FileAccess.Write);
			sourceStream.CopyTo(targetStream);

			state.FilesCopied++;
			state.BytesCopied += fileSize;
			
			_logger.LogDebug("[DiskVFS] Copied file: {FileName} -> {TargetPath} ({Size} bytes) [{FilesCopied}/{TotalFiles}]", 
				fileName, targetPath, fileSize, state.FilesCopied, state.TotalFiles);
			
			// Report progress
			progress?.Report((fileName, state.FilesCopied, state.TotalFiles, state.BytesCopied, state.TotalBytes));
		}

		// Recursively copy subdirectories
		foreach (var dir in Directory.GetDirectories(sourceDir))
		{
			var dirName = Path.GetFileName(dir);
			var targetPath = CombinePaths(targetDir, dirName);

			_fileSystem.CreateDirectory(targetPath);
			_logger.LogInformation("[DiskVFS] Created directory: {TargetPath}", targetPath);
			CopyDirectoryRecursive(dir, targetPath, progress, state);
		}
	}

	private static DiscFileSystem GetFileSystemFromDisk(VirtualDisk disk)
	{
		// Check if the disk has partitions
		if (disk.IsPartitioned)
		{
			var partitionTable = disk.Partitions;
			if (partitionTable.Count > 0)
			{
				// Use the first partition
				var partition = partitionTable[0];
				Stream? partitionStream = partition.Open();
				
				try
				{
					// Try to detect filesystem on the partition
					if (FatFileSystem.Detect(partitionStream))
					{
						// Reset stream position before creating filesystem
						partitionStream.Position = 0;
						var fs = new FatFileSystem(partitionStream);
						partitionStream = null; // Ownership transferred to FatFileSystem
						return fs;
					}
				}
				finally
				{
					// Dispose stream only if ownership was not transferred
					if (partitionStream != null)
					{
						partitionStream.Dispose();
					}
				}
			}
		}
		else
		{
			// Try raw disk (no partition table)
			var diskStream = disk.Content;
			
			// Try to detect filesystem
			if (FatFileSystem.Detect(diskStream))
			{
				// Reset stream position before creating filesystem
				diskStream.Position = 0;
				return new FatFileSystem(diskStream);
			}
		}

		throw new InvalidOperationException("No supported filesystem found on disk. Use Create() to format a new disk.");
	}

	private string NormalizePath(string path)
	{
		// DiscUtils requires backslashes for all filesystem types (FAT, ISO9660, etc.)
		// Convert any forward slashes to backslashes
		var normalized = path.Replace('/', '\\');

		// Remove drive letters (e.g., "C:\path" becomes "\path")
		if (normalized.Length >= 2 && normalized[1] == ':')
		{
			normalized = normalized.Substring(2);
		}

		// Ensure leading backslash
		if (!normalized.StartsWith('\\'))
		{
			normalized = "\\" + normalized;
		}

		// Remove double backslashes
		normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\\+", "\\");

		return normalized;
	}

	private string CombinePaths(string basePath, string childPath)
	{
		// DiscUtils uses backslashes for all filesystem types
		basePath = basePath.TrimEnd('/', '\\');
		childPath = childPath.TrimStart('/', '\\');
		var combined = basePath + "\\" + childPath;
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

	public bool DirectoryExists(string path)
	{
		try
		{
			var normalizedPath = NormalizePath(path);
			return _fileSystem.DirectoryExists(normalizedPath);
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "[DiskVFS] Failed to check if directory exists: {Path}", path);
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
	
	/// <summary>
	/// Creates a directory in the virtual filesystem.
	/// This is an internal helper method for components that need to create directory structures.
	/// </summary>
	/// <param name="path">The path of the directory to create</param>
	public void CreateDirectory(string path)
	{
		if (IsReadOnly)
		{
			_logger.LogWarning("[DiskVFS] Cannot create directory on read-only disk: {Path}", path);
			return;
		}
		
		try
		{
			var normalizedPath = NormalizePath(path);
			
			// Create directory if it doesn't exist
			// Check existence with error handling for FAT filesystem corruption
			bool directoryExists = false;
			try
			{
				directoryExists = _fileSystem.DirectoryExists(normalizedPath);
			}
			catch (ArgumentException ex) when (ex.Message.Contains("An item with the same key has already been added"))
			{
				// FAT filesystem has duplicate entries - assume directory exists to avoid corruption
				_logger.LogWarning(ex, "[DiskVFS] FAT filesystem corruption detected for path: {Path}. Assuming directory exists.", normalizedPath);
				directoryExists = true;
			}
			
			if (!directoryExists)
			{
				_fileSystem.CreateDirectory(normalizedPath);
				_logger.LogDebug("[DiskVFS] Created directory: {Path}", normalizedPath);
			}
		}
		catch (ArgumentException ex) when (ex.Message.Contains("An item with the same key has already been added"))
		{
			// FAT filesystem corruption - log and continue
			_logger.LogWarning(ex, "[DiskVFS] FAT filesystem corruption while creating directory: {Path}. Continuing anyway.", path);
		}
		catch (IOException ex)
		{
			_logger.LogWarning(ex, "[DiskVFS] Failed to create directory due to IO error: {Path}", path);
		}
		catch (UnauthorizedAccessException ex)
		{
			_logger.LogWarning(ex, "[DiskVFS] Failed to create directory due to access error: {Path}", path);
		}
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
		_underlyingStream?.Dispose();

		_logger.LogInformation("[DiskVFS] Disposed");
	}
}