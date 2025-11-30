namespace Win32Emu.Wasm.VirtualFileSystem;

/// <summary>
/// Implementation of IVirtualFileHandle that wraps a MemoryStream for browser-based VFS.
/// Provides in-memory file operations for the WASM emulator.
/// </summary>
internal class BrowserFileHandle : Win32Emu.VirtualFileSystem.IVirtualFileHandle
{
	private readonly MemoryStream _stream;
	private readonly string _path;
	private readonly BrowserVirtualFileSystem _vfs;
	private readonly bool _canWrite;
	private bool _disposed;

	public BrowserFileHandle(MemoryStream stream, string path, BrowserVirtualFileSystem vfs, bool canWrite)
	{
		_stream = stream;
		_path = path;
		_vfs = vfs;
		_canWrite = canWrite;
		_disposed = false;
	}

	public int Read(byte[] buffer, int offset, int count)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		return _stream.Read(buffer, offset, count);
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (!_canWrite)
		{
			throw new InvalidOperationException("Cannot write to a read-only file handle");
		}
		_stream.Write(buffer, offset, count);
	}

	public long Seek(long offset, SeekOrigin origin)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		return _stream.Seek(offset, origin);
	}

	public long Position
	{
		get
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _stream.Position;
		}
	}

	public void SetLength(long length)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (!_canWrite)
		{
			throw new InvalidOperationException("Cannot modify a read-only file handle");
		}
		_stream.SetLength(length);
	}

	public void Flush()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		_stream.Flush();
		
		// Persist the data back to VFS when flushed if writable
		if (_canWrite)
		{
			_vfs.UpdateFileData(_path, _stream.ToArray());
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;

		// If writable, persist the data back to the VFS before disposing
		if (_canWrite)
		{
			_vfs.UpdateFileData(_path, _stream.ToArray());
		}

		_vfs.CloseFile(_path);
		_stream.Dispose();
	}
}
