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