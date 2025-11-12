namespace Win32Emu.VirtualFileSystem;

/// <summary>
/// A Stream wrapper around IVirtualFileHandle to enable Stream-based APIs
/// to work with the virtual file system.
/// </summary>
public class VfsStream : Stream
{
	private readonly IVirtualFileHandle _handle;
	private readonly bool _canRead;
	private readonly bool _canWrite;
	private bool _disposed;

	public VfsStream(IVirtualFileHandle handle, bool canRead, bool canWrite)
	{
		_handle = handle ?? throw new ArgumentNullException(nameof(handle));
		_canRead = canRead;
		_canWrite = canWrite;
	}

	public override bool CanRead => _canRead && !_disposed;
	public override bool CanSeek => !_disposed;
	public override bool CanWrite => _canWrite && !_disposed;

	public override long Length
	{
		get
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(VfsStream));

			var currentPos = _handle.Position;
			try
			{
				_handle.Seek(0, SeekOrigin.End);
				return _handle.Position;
			}
			finally
			{
				_handle.Seek(currentPos, SeekOrigin.Begin);
			}
		}
	}

	public override long Position
	{
		get
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(VfsStream));
			return _handle.Position;
		}
		set
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(VfsStream));
			_handle.Seek(value, SeekOrigin.Begin);
		}
	}

	public override void Flush()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(VfsStream));
		_handle.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(VfsStream));
		if (!_canRead)
			throw new NotSupportedException("Stream does not support reading");
		
		return _handle.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(VfsStream));
		return _handle.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(VfsStream));
		_handle.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(VfsStream));
		if (!_canWrite)
			throw new NotSupportedException("Stream does not support writing");
		
		_handle.Write(buffer, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_handle.Dispose();
			}
			_disposed = true;
		}
		base.Dispose(disposing);
	}
}
