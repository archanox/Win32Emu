namespace Win32Emu.VirtualFileSystem
{
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
}