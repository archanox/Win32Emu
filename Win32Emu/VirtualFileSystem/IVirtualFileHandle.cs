namespace Win32Emu.VirtualFileSystem
{
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
}