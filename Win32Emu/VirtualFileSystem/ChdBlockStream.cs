using System.IO;
using System.Text;
using CHDSharpLib;
using CHDSharpLib.Utils;
using Microsoft.Extensions.Logging;

namespace Win32Emu.VirtualFileSystem;

/// <summary>
/// Stream wrapper that provides read access to decompressed CHD data blocks.
/// This allows reading data from CHD files block-by-block without decompressing the entire file.
/// </summary>
internal class ChdBlockStream : Stream
{
	private readonly Stream _chdFile;
	private readonly CHDHeader _header;
	private readonly CHDCodec _codec;
	private readonly ArrayPool _arrayPool;
	private readonly ILogger _logger;
	private long _position;
	private readonly byte[] _currentBlock;
	private int _currentBlockIndex = -1;
	private bool _disposed;

	public ChdBlockStream(Stream chdFile, CHDHeader header, ILogger logger)
	{
		_chdFile = chdFile ?? throw new ArgumentNullException(nameof(chdFile));
		_header = header ?? throw new ArgumentNullException(nameof(header));
		_logger = logger;
		_position = 0;
		_currentBlock = new byte[header.blocksize];
		_codec = new CHDCodec();
		_arrayPool = new ArrayPool(header.blocksize);
		
		// Initialize block readers
		CHDBlockRead.FindBlockReaders(_header);
		CHDBlockRead.FindRepeatedBlocks(_header, null);
	}

	public override bool CanRead => !_disposed;
	public override bool CanSeek => !_disposed;
	public override bool CanWrite => false;
	public override long Length => (long)_header.totalbytes;
	public override long Position 
	{ 
		get => _position; 
		set => Seek(value, SeekOrigin.Begin); 
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_disposed) throw new ObjectDisposedException(nameof(ChdBlockStream));
		if (buffer == null) throw new ArgumentNullException(nameof(buffer));
		if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		if (offset + count > buffer.Length) throw new ArgumentException("Buffer too small");

		if (_position >= Length) return 0;

		int totalRead = 0;
		while (count > 0 && _position < Length)
		{
			// Calculate which block we need
			int blockIndex = (int)(_position / _header.blocksize);
			int blockOffset = (int)(_position % _header.blocksize);
			
			// Load the block if it's not already loaded
			if (_currentBlockIndex != blockIndex)
			{
				LoadBlock(blockIndex);
			}

			// Copy data from current block
			int bytesToCopy = Math.Min(count, (int)_header.blocksize - blockOffset);
			bytesToCopy = Math.Min(bytesToCopy, (int)(Length - _position));
			
			Array.Copy(_currentBlock, blockOffset, buffer, offset, bytesToCopy);
			
			_position += bytesToCopy;
			offset += bytesToCopy;
			count -= bytesToCopy;
			totalRead += bytesToCopy;
		}

		return totalRead;
	}

	private void LoadBlock(int blockIndex)
	{
		if (blockIndex < 0 || blockIndex >= _header.totalblocks)
		{
			throw new ArgumentOutOfRangeException(nameof(blockIndex));
		}

		var mapEntry = _header.map[blockIndex];
		
		// Read the compressed block data if needed
		if (mapEntry.buffIn == null && mapEntry.comptype != compression_type.COMPRESSION_SELF && 
		    mapEntry.comptype != compression_type.COMPRESSION_MINI)
		{
			mapEntry.buffIn = _arrayPool.Rent();
			_chdFile.Seek((long)mapEntry.offset, SeekOrigin.Begin);
			int bytesRead = _chdFile.Read(mapEntry.buffIn, 0, (int)mapEntry.length);
			if (bytesRead != mapEntry.length)
			{
				throw new IOException($"Failed to read CHD block {blockIndex}: expected {mapEntry.length} bytes, got {bytesRead}");
			}
		}

		// Decompress the block
		var error = CHDBlockRead.ReadBlock(mapEntry, _arrayPool, _header.chdReader, _codec, _currentBlock, (int)_header.blocksize);
		if (error != chd_error.CHDERR_NONE)
		{
			throw new IOException($"Failed to decompress CHD block {blockIndex}: {error}");
		}

		_currentBlockIndex = blockIndex;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (_disposed) throw new ObjectDisposedException(nameof(ChdBlockStream));

		long newPosition = origin switch
		{
			SeekOrigin.Begin => offset,
			SeekOrigin.Current => _position + offset,
			SeekOrigin.End => Length + offset,
			_ => throw new ArgumentException("Invalid seek origin", nameof(origin))
		};

		if (newPosition < 0) throw new IOException("Seek before beginning of stream");
		if (newPosition > Length) newPosition = Length;

		_position = newPosition;
		return _position;
	}

	public override void Flush()
	{
		// No-op for read-only stream
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException("Cannot set length on CHD stream");
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException("Cannot write to CHD stream");
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				// Clean up block buffers
				if (_header?.map != null)
				{
					foreach (var mapEntry in _header.map)
					{
						if (mapEntry.buffIn != null)
						{
							_arrayPool.Return(mapEntry.buffIn);
							mapEntry.buffIn = null;
						}
						if (mapEntry.buffOutCache != null)
						{
							_arrayPool.Return(mapEntry.buffOutCache);
							mapEntry.buffOutCache = null;
						}
					}
				}
			}
			_disposed = true;
		}
		base.Dispose(disposing);
	}
}
