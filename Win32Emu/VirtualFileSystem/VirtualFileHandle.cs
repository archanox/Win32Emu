namespace Win32Emu.VirtualFileSystem
{
	/// <summary>
	/// Implementation of IVirtualFileHandle that wraps a FileStream.
	/// </summary>
	internal class VirtualFileHandle : IVirtualFileHandle
	{
		private readonly FileStream _stream;

		public VirtualFileHandle(FileStream stream)
		{
			_stream = stream;
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
			_stream.Flush(true);
		}

		public void Dispose()
		{
			_stream.Dispose();
		}
	}
}