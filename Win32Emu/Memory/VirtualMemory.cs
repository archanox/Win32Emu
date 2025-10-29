using System.Collections.Concurrent;

namespace Win32Emu.Memory;

/// <summary>
/// Sparse virtual memory model for 32-bit address space.
/// Uses page-based allocation to support full 4GB address space without allocating all memory upfront.
/// Pages are allocated on-demand when accessed.
/// </summary>
public class VirtualMemory
{
	private const ulong DefaultSize = 512 * 1024 * 1024; // Default tracked size for compatibility
	private const int PageSizeBits = 12; // 4KB pages (2^12 = 4096)
	private const int PageSize = 1 << PageSizeBits; // 4096 bytes
	private const uint PageMask = PageSize - 1; // 0xFFF
	private const ulong MaxAddress = 0x100000000; // 4GB for 32-bit address space
	
	private readonly ConcurrentDictionary<uint, byte[]> _pages;
	private readonly ulong _trackedSize; // For compatibility with existing code
	
	public VirtualMemory(ulong size = DefaultSize)
	{
		_pages = new ConcurrentDictionary<uint, byte[]>();
		_trackedSize = size;
	}

    public ulong Size => MaxAddress; // Report full 32-bit address space

    private void EnsureRange(ulong addr, ulong length = 1)
    {
        if (length == 0)
        {
	        return;
        }

        // Check for overflow in address calculation
        if (addr > ulong.MaxValue - length + 1)
        {
            Diagnostics.Diagnostics.LogMemoryEnsureFailure(addr, length, MaxAddress);
            throw new IndexOutOfRangeException($"Memory access causes address overflow: addr=0x{addr:X}, len={length}, size=0x{MaxAddress:X}");
        }
            
        // Check if the entire range is within 32-bit address space
        if (addr + length > MaxAddress)
        {
            Diagnostics.Diagnostics.LogMemoryEnsureFailure(addr, length, MaxAddress);
            throw new IndexOutOfRangeException($"Memory access out of range: addr=0x{addr:X}, len={length}, size=0x{MaxAddress:X}");
        }
    }
    
    private byte[] GetOrCreatePage(uint pageIndex)
    {
        return _pages.GetOrAdd(pageIndex, _ => new byte[PageSize]);
    }
    
    private byte ReadByteInternal(ulong addr)
    {
        uint pageIndex = (uint)(addr >> PageSizeBits);
        uint offset = (uint)(addr & PageMask);
        
        if (_pages.TryGetValue(pageIndex, out var page))
        {
            return page[offset];
        }
        
        // Unallocated pages return zero
        return 0;
    }
    
    private void WriteByteInternal(ulong addr, byte value)
    {
        uint pageIndex = (uint)(addr >> PageSizeBits);
        uint offset = (uint)(addr & PageMask);
        
        var page = GetOrCreatePage(pageIndex);
        page[offset] = value;
    }

    public byte Read8(ulong addr)
    {
        EnsureRange(addr);
        return ReadByteInternal(addr);
    }

    public ushort Read16(ulong addr)
    {
        EnsureRange(addr, 2);
        return (ushort)(Read8(addr) | (Read8(addr + 1) << 8));
    }

    public uint Read32(ulong addr)
    {
        EnsureRange(addr, 4);
        return (uint)(Read16(addr) | (Read16(addr + 2) << 16));
    }

    public void Write8(ulong addr, byte value)
    {
        EnsureRange(addr);
        WriteByteInternal(addr, value);
    }

    public void Write16(ulong addr, ushort value)
    {
        Write8(addr, (byte)(value & 0xFF));
        Write8(addr + 1, (byte)(value >> 8));
    }

    public void Write32(ulong addr, uint value)
    {
        Write16(addr, (ushort)(value & 0xFFFF));
        Write16(addr + 2, (ushort)(value >> 16));
    }

    public ulong Read64(ulong addr)
    {
        EnsureRange(addr, 8);
        return Read32(addr) | ((ulong)Read32(addr + 4) << 32);
    }

    public void Write64(ulong addr, ulong value)
    {
        Write32(addr, (uint)(value & 0xFFFFFFFF));
        Write32(addr + 4, (uint)(value >> 32));
    }

    public void WriteBytes(ulong addr, ReadOnlySpan<byte> data)
    {
        EnsureRange(addr, (ulong)data.Length);
        
        // Optimized write for page-aligned or single-page writes
        uint startPage = (uint)(addr >> PageSizeBits);
        uint endPage = (uint)((addr + (ulong)data.Length - 1) >> PageSizeBits);
        
        if (startPage == endPage)
        {
            // Single page write
            var page = GetOrCreatePage(startPage);
            uint offset = (uint)(addr & PageMask);
            data.CopyTo(new Span<byte>(page, (int)offset, data.Length));
        }
        else
        {
            // Multi-page write - write byte by byte for simplicity
            for (int i = 0; i < data.Length; i++)
            {
                WriteByteInternal(addr + (ulong)i, data[i]);
            }
        }
    }

    public byte[] GetSpan(ulong addr, int length)
    {
        EnsureRange(addr, (ulong)length);
        
        byte[] result = new byte[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = ReadByteInternal(addr + (ulong)i);
        }
        return result;
    }
    
    /// <summary>
    /// Gets statistics about memory usage
    /// </summary>
    public (int AllocatedPages, long AllocatedBytes) GetMemoryStats()
    {
        int pageCount = _pages.Count;
        long bytes = pageCount * PageSize;
        return (pageCount, bytes);
    }
}
