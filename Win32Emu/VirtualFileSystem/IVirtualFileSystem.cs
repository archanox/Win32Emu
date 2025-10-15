namespace Win32Emu.VirtualFileSystem;

/// <summary>
/// Virtual File System interface that provides a layer between Win32 file I/O APIs
/// and the actual filesystem. This allows for copy-on-write semantics and game-specific
/// virtual filesystems.
/// </summary>
public interface IVirtualFileSystem
{
	/// <summary>
	/// Opens or creates a file with the specified access mode and creation disposition.
	/// </summary>
	IVirtualFileHandle? OpenFile(string path, VfsFileMode mode, VfsFileAccess access);

	/// <summary>
	/// Deletes a file from the virtual filesystem.
	/// </summary>
	bool DeleteFile(string path);

	/// <summary>
	/// Moves/renames a file in the virtual filesystem.
	/// </summary>
	bool MoveFile(string existingPath, string newPath);

	/// <summary>
	/// Checks if a file exists in the virtual filesystem.
	/// </summary>
	bool FileExists(string path);

	/// <summary>
	/// Gets files matching a search pattern.
	/// </summary>
	string[] GetFiles(string directory, string pattern);
}

/// <summary>
/// Represents a handle to a file in the virtual filesystem.
/// </summary>
public interface IVirtualFileHandle : IDisposable
{
	/// <summary>
	/// Reads data from the file at the current position.
	/// </summary>
	int Read(byte[] buffer, int offset, int count);

	/// <summary>
	/// Writes data to the file at the current position.
	/// </summary>
	void Write(byte[] buffer, int offset, int count);

	/// <summary>
	/// Sets the position within the file.
	/// </summary>
	long Seek(long offset, SeekOrigin origin);

	/// <summary>
	/// Gets the current position within the file.
	/// </summary>
	long Position { get; }

	/// <summary>
	/// Sets the length of the file.
	/// </summary>
	void SetLength(long length);

	/// <summary>
	/// Flushes any buffered data to the underlying storage.
	/// </summary>
	void Flush();
}

/// <summary>
/// File access mode for VFS operations.
/// </summary>
public enum VfsFileAccess
{
	Read,
	Write,
	ReadWrite
}

/// <summary>
/// File mode for VFS operations.
/// </summary>
public enum VfsFileMode
{
	CreateNew,
	Create,
	Open,
	OpenOrCreate,
	Truncate
}
