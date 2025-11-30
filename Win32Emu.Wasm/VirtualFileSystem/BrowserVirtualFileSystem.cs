using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.VirtualFileSystem;

namespace Win32Emu.Wasm.VirtualFileSystem;

/// <summary>
/// Browser-based Virtual File System for WASM emulator.
/// Provides an in-memory file system that can be populated from browser file uploads.
/// Supports case-insensitive file access like Windows and implements copy-on-write semantics.
/// </summary>
public class BrowserVirtualFileSystem : IVirtualFileSystem, IDisposable
{
	private readonly ILogger _logger;
	private readonly Dictionary<string, byte[]> _files = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> _openFiles = new(StringComparer.OrdinalIgnoreCase);
	private readonly object _lock = new();
	private bool _disposed;

	/// <summary>
	/// Creates a new browser-based virtual file system.
	/// </summary>
	/// <param name="logger">Optional logger for diagnostics</param>
	public BrowserVirtualFileSystem(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
		
		// Initialize root directory
		_directories.Add("\\");
		_directories.Add("C:\\");
		
		_logger.LogInformation("[BrowserVFS] Initialized browser-based virtual file system");
	}

	/// <summary>
	/// Adds files to the virtual file system from a dictionary.
	/// File paths should be relative (e.g., "myFolder/game.exe" or "readme.txt").
	/// </summary>
	/// <param name="files">Dictionary mapping relative paths to file contents</param>
	public void AddFiles(Dictionary<string, byte[]> files)
	{
		lock (_lock)
		{
			foreach (var kvp in files)
			{
				AddFile(kvp.Key, kvp.Value);
			}
		}
		
		_logger.LogInformation("[BrowserVFS] Added {Count} files to virtual file system", files.Count);
	}

	/// <summary>
	/// Adds a single file to the virtual file system.
	/// </summary>
	/// <param name="relativePath">Relative path of the file (e.g., "myFolder/game.exe")</param>
	/// <param name="data">File contents</param>
	public void AddFile(string relativePath, byte[] data)
	{
		var normalizedPath = NormalizePath(relativePath);
		
		lock (_lock)
		{
			_files[normalizedPath] = data;
			
			// Ensure parent directories exist
			EnsureDirectoriesForPath(normalizedPath);
		}
		
		_logger.LogDebug("[BrowserVFS] Added file: {Path} ({Size} bytes)", normalizedPath, data.Length);
	}

	/// <summary>
	/// Creates all parent directories for a given file path.
	/// </summary>
	private void EnsureDirectoriesForPath(string normalizedPath)
	{
		var lastSep = normalizedPath.LastIndexOf('\\');
		if (lastSep <= 0)
		{
			return;
		}
		
		var directory = normalizedPath.Substring(0, lastSep);
		
		// Create all parent directories
		var parts = directory.Split('\\', StringSplitOptions.RemoveEmptyEntries);
		var currentPath = "\\";
		
		foreach (var part in parts)
		{
			currentPath = currentPath.TrimEnd('\\') + "\\" + part;
			_directories.Add(currentPath);
		}
	}

	/// <summary>
	/// Updates the data for an existing file. Used internally by BrowserFileHandle.
	/// </summary>
	internal void UpdateFileData(string normalizedPath, byte[] data)
	{
		lock (_lock)
		{
			_files[normalizedPath] = data;
		}
		_logger.LogDebug("[BrowserVFS] Updated file data: {Path} ({Size} bytes)", normalizedPath, data.Length);
	}

	/// <summary>
	/// Marks a file as closed. Used internally by BrowserFileHandle.
	/// </summary>
	internal void CloseFile(string normalizedPath)
	{
		lock (_lock)
		{
			_openFiles.Remove(normalizedPath);
		}
		_logger.LogDebug("[BrowserVFS] Closed file: {Path}", normalizedPath);
	}

	/// <summary>
	/// Gets the total number of files in the virtual file system.
	/// </summary>
	public int FileCount
	{
		get
		{
			lock (_lock)
			{
				return _files.Count;
			}
		}
	}

	/// <summary>
	/// Gets all files in the virtual file system.
	/// </summary>
	public IReadOnlyDictionary<string, byte[]> Files
	{
		get
		{
			lock (_lock)
			{
				return new Dictionary<string, byte[]>(_files, StringComparer.OrdinalIgnoreCase);
			}
		}
	}

	/// <summary>
	/// Normalizes a path to use backslashes and Windows-style formatting.
	/// </summary>
	private string NormalizePath(string path)
	{
		// Convert forward slashes to backslashes
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
		
		// Remove trailing backslash (except for root)
		if (normalized.Length > 1 && normalized.EndsWith('\\'))
		{
			normalized = normalized.TrimEnd('\\');
		}
		
		// Remove double backslashes
		while (normalized.Contains("\\\\"))
		{
			normalized = normalized.Replace("\\\\", "\\");
		}
		
		return normalized;
	}

	/// <summary>
	/// Finds a file case-insensitively.
	/// </summary>
	private string? FindFileCaseInsensitive(string normalizedPath)
	{
		lock (_lock)
		{
			// Direct lookup works due to case-insensitive comparer
			if (_files.ContainsKey(normalizedPath))
			{
				// Return the actual key from the dictionary (preserving original case)
				foreach (var key in _files.Keys)
				{
					if (string.Equals(key, normalizedPath, StringComparison.OrdinalIgnoreCase))
					{
						return key;
					}
				}
			}
		}
		return null;
	}

	public IVirtualFileHandle? OpenFile(string path, VfsFileMode mode, VfsFileAccess access)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		
		var normalizedPath = NormalizePath(path);
		var canWrite = access == VfsFileAccess.Write || access == VfsFileAccess.ReadWrite;
		
		_logger.LogDebug("[BrowserVFS] OpenFile: {Path}, Mode: {Mode}, Access: {Access}", normalizedPath, mode, access);
		
		lock (_lock)
		{
			var existingPath = FindFileCaseInsensitive(normalizedPath);
			var fileExists = existingPath != null;
			
			// Handle file mode
			switch (mode)
			{
				case VfsFileMode.Open:
					if (!fileExists)
					{
						_logger.LogDebug("[BrowserVFS] File not found: {Path}", normalizedPath);
						return null;
					}
					break;
					
				case VfsFileMode.CreateNew:
					if (fileExists)
					{
						_logger.LogDebug("[BrowserVFS] File already exists: {Path}", normalizedPath);
						return null;
					}
					// Create empty file
					_files[normalizedPath] = [];
					EnsureDirectoriesForPath(normalizedPath);
					break;
					
				case VfsFileMode.Create:
				case VfsFileMode.Truncate:
					// Create or truncate
					_files[normalizedPath] = [];
					if (!fileExists)
					{
						EnsureDirectoriesForPath(normalizedPath);
					}
					break;
					
				case VfsFileMode.OpenOrCreate:
					if (!fileExists)
					{
						_files[normalizedPath] = [];
						EnsureDirectoriesForPath(normalizedPath);
					}
					break;
			}
			
			// Get the actual path key (with proper casing)
			var actualPath = existingPath ?? normalizedPath;
			
			// Create memory stream with file data
			var data = _files.TryGetValue(actualPath, out var fileData) ? fileData : [];
			var stream = new MemoryStream();
			stream.Write(data, 0, data.Length);
			
			// Reset position based on access mode
			if (access == VfsFileAccess.Read || access == VfsFileAccess.ReadWrite)
			{
				stream.Position = 0;
			}
			// For write mode, position stays at end for append behavior (if needed)
			
			_openFiles.Add(actualPath);
			_logger.LogDebug("[BrowserVFS] Opened file: {Path} (writable: {Writable})", actualPath, canWrite);
			
			return new BrowserFileHandle(stream, actualPath, this, canWrite);
		}
	}

	public bool DeleteFile(string path)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		
		var normalizedPath = NormalizePath(path);
		
		lock (_lock)
		{
			var actualPath = FindFileCaseInsensitive(normalizedPath);
			if (actualPath == null)
			{
				_logger.LogDebug("[BrowserVFS] File not found for deletion: {Path}", normalizedPath);
				return false;
			}
			
			_files.Remove(actualPath);
			_logger.LogDebug("[BrowserVFS] Deleted file: {Path}", actualPath);
			return true;
		}
	}

	public bool MoveFile(string existingPath, string newPath)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		
		var normalizedExisting = NormalizePath(existingPath);
		var normalizedNew = NormalizePath(newPath);
		
		lock (_lock)
		{
			var actualPath = FindFileCaseInsensitive(normalizedExisting);
			if (actualPath == null)
			{
				_logger.LogDebug("[BrowserVFS] Source file not found for move: {Path}", normalizedExisting);
				return false;
			}
			
			if (FindFileCaseInsensitive(normalizedNew) != null)
			{
				// Destination exists, overwrite
				_files.Remove(normalizedNew);
			}
			
			// Move file data to new path
			var data = _files[actualPath];
			_files.Remove(actualPath);
			_files[normalizedNew] = data;
			EnsureDirectoriesForPath(normalizedNew);
			
			_logger.LogDebug("[BrowserVFS] Moved file: {Source} -> {Destination}", actualPath, normalizedNew);
			return true;
		}
	}

	public bool FileExists(string path)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		
		var normalizedPath = NormalizePath(path);
		
		lock (_lock)
		{
			return FindFileCaseInsensitive(normalizedPath) != null;
		}
	}

	public bool DirectoryExists(string path)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		
		var normalizedPath = NormalizePath(path);
		
		// Root always exists
		if (normalizedPath == "\\" || normalizedPath == "C:\\" || normalizedPath == "")
		{
			return true;
		}
		
		lock (_lock)
		{
			// Check explicit directories
			foreach (var dir in _directories)
			{
				if (string.Equals(dir, normalizedPath, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			
			// Check if any file exists in this directory (implicit directory)
			var pathWithSep = normalizedPath.TrimEnd('\\') + "\\";
			foreach (var filePath in _files.Keys)
			{
				if (filePath.StartsWith(pathWithSep, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		
		return false;
	}

	public string[] GetFiles(string directory, string pattern)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		
		var normalizedDir = NormalizePath(directory);
		var dirWithSep = normalizedDir.TrimEnd('\\') + "\\";
		
		// Convert pattern to simple wildcard matching
		var hasWildcard = pattern.Contains('*') || pattern.Contains('?');
		var searchPattern = pattern.Replace("*", "").Replace("?", "");
		var extension = pattern.StartsWith("*") ? pattern.Substring(1) : null;
		
		var results = new List<string>();
		
		lock (_lock)
		{
			foreach (var filePath in _files.Keys)
			{
				// Check if file is in the specified directory (not subdirectory)
				if (!filePath.StartsWith(dirWithSep, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				
				var relativePath = filePath.Substring(dirWithSep.Length);
				
				// Skip files in subdirectories
				if (relativePath.Contains('\\'))
				{
					continue;
				}
				
				// Apply pattern matching
				if (hasWildcard)
				{
					if (extension != null)
					{
						// Pattern like "*.exe"
						if (relativePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
						{
							results.Add(relativePath);
						}
					}
					else if (pattern == "*" || pattern == "*.*")
					{
						// Match all files
						results.Add(relativePath);
					}
				}
				else
				{
					// Exact match
					if (string.Equals(relativePath, pattern, StringComparison.OrdinalIgnoreCase))
					{
						results.Add(relativePath);
					}
				}
			}
		}
		
		_logger.LogDebug("[BrowserVFS] GetFiles({Directory}, {Pattern}) returned {Count} files", 
			directory, pattern, results.Count);
		
		return results.ToArray();
	}

	/// <summary>
	/// Clears all files from the virtual file system.
	/// </summary>
	public void Clear()
	{
		lock (_lock)
		{
			_files.Clear();
			_directories.Clear();
			_openFiles.Clear();
			
			// Re-initialize root directory
			_directories.Add("\\");
			_directories.Add("C:\\");
		}
		
		_logger.LogInformation("[BrowserVFS] Cleared virtual file system");
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		
		lock (_lock)
		{
			_files.Clear();
			_directories.Clear();
			_openFiles.Clear();
		}
		
		_logger.LogInformation("[BrowserVFS] Disposed");
	}
}
