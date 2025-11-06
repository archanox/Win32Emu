namespace Win32Emu.VirtualFileSystem
{
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
}