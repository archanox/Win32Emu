using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

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
	private readonly ulong _configuredSize; // Configured size from settings (not enforced, for logging/stats only)
	
	// IAT protection: maps IAT VA -> expected synthetic address for runtime verification
	private Dictionary<uint, uint>? _iatEntryMap;
	private readonly ILogger? _logger;
	
	public VirtualMemory(ulong size = DefaultSize, ILogger? logger = null)
	{
		_pages = new ConcurrentDictionary<uint, byte[]>();
		_configuredSize = size;
		_logger = logger;
	}
	
	/// <summary>
	/// Registers IAT entries for runtime verification and auto-fix.
	/// When memory is read from these addresses, the value is verified and corrected if corrupted.
	/// </summary>
	public void RegisterIatEntries(Dictionary<uint, uint> iatEntryMap)
	{
		_iatEntryMap = iatEntryMap;
	}

    public ulong Size => MaxAddress; // Report full 32-bit address space
    
    /// <summary>
    /// Gets the configured memory size from settings (not enforced in sparse model)
    /// </summary>
    public ulong ConfiguredSize => _configuredSize;

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
        var value = (uint)(Read16(addr) | (Read16(addr + 2) << 16));
        
        // IAT protection: verify and fix corrupted entries
        if (_iatEntryMap != null && _iatEntryMap.TryGetValue((uint)addr, out var expectedValue))
        {
            if (value != expectedValue)
            {
                // IAT entry was corrupted - log and fix it automatically
                _logger?.LogWarning("[VirtualMemory] IAT corruption detected and auto-fixed at 0x{Addr:X8}: was 0x{Value:X8}, expected 0x{Expected:X8}", 
                    addr, value, expectedValue);
                Write32(addr, expectedValue);
                return expectedValue;
            }
        }
        
        return value;
    }

    public void Write8(ulong addr, byte value)
    {
        EnsureRange(addr);
        WriteByteInternal(addr, value);
    }

    public void Write16(ulong addr, ushort value)
    {
        EnsureRange(addr, 2);
        Write8(addr, (byte)(value & 0xFF));
        Write8(addr + 1, (byte)(value >> 8));
    }

    public void Write32(ulong addr, uint value)
    {
        EnsureRange(addr, 4);
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
        EnsureRange(addr, 8);
        Write32(addr, (uint)(value & 0xFFFFFFFF));
        Write32(addr + 4, (uint)(value >> 32));
    }

    public void WriteBytes(ulong addr, ReadOnlySpan<byte> data)
    {
        EnsureRange(addr, (ulong)data.Length);
        
        // Early return for empty data to avoid underflow in endPage calculation
        if (data.Length == 0)
        {
            return;
        }
        
        uint startPage = (uint)(addr >> PageSizeBits);
        uint endPage = (uint)((addr + (ulong)data.Length - 1) >> PageSizeBits);

        int dataOffset = 0;
        int bytesRemaining = data.Length;

        // Handle first partial page
        uint firstPageOffset = (uint)(addr & PageMask);
        if (firstPageOffset != 0)
        {
            int bytesInFirstPage = Math.Min(PageSize - (int)firstPageOffset, bytesRemaining);
            var page = GetOrCreatePage(startPage);
            data.Slice(dataOffset, bytesInFirstPage).CopyTo(new Span<byte>(page, (int)firstPageOffset, bytesInFirstPage));
            dataOffset += bytesInFirstPage;
            bytesRemaining -= bytesInFirstPage;
            startPage++;
        }

        // Handle full pages in the middle
        while (bytesRemaining >= PageSize)
        {
            var page = GetOrCreatePage(startPage);
            data.Slice(dataOffset, PageSize).CopyTo(new Span<byte>(page, 0, PageSize));
            dataOffset += PageSize;
            bytesRemaining -= PageSize;
            startPage++;
        }

        // Handle last partial page
        if (bytesRemaining > 0)
        {
            var page = GetOrCreatePage(startPage);
            data.Slice(dataOffset, bytesRemaining).CopyTo(new Span<byte>(page, 0, bytesRemaining));
        }
    }

    /// <summary>
    /// Reads bytes from memory into a newly allocated array.
    /// For better performance, consider using ReadBytes(ulong, Span&lt;byte&gt;) to avoid allocations.
    /// </summary>
    public byte[] GetSpan(ulong addr, int length)
    {
        EnsureRange(addr, (ulong)length);
        
        byte[] result = new byte[length];
        ReadBytes(addr, result);
        return result;
    }
    
    /// <summary>
    /// Reads bytes from memory into the provided span.
    /// This is more efficient than GetSpan as it avoids allocations.
    /// </summary>
    public void ReadBytes(ulong addr, Span<byte> destination)
    {
        EnsureRange(addr, (ulong)destination.Length);
        
        if (destination.Length == 0)
        {
            return;
        }
        
        uint startPage = (uint)(addr >> PageSizeBits);
        uint endPage = (uint)((addr + (ulong)destination.Length - 1) >> PageSizeBits);
        
        int destOffset = 0;
        int bytesRemaining = destination.Length;
        
        // Handle first partial page
        uint firstPageOffset = (uint)(addr & PageMask);
        if (firstPageOffset != 0)
        {
            int bytesInFirstPage = Math.Min(PageSize - (int)firstPageOffset, bytesRemaining);
            if (_pages.TryGetValue(startPage, out var page))
            {
                new ReadOnlySpan<byte>(page, (int)firstPageOffset, bytesInFirstPage).CopyTo(destination.Slice(destOffset, bytesInFirstPage));
            }
            else
            {
                // Unallocated pages read as zeros
                destination.Slice(destOffset, bytesInFirstPage).Clear();
            }
            destOffset += bytesInFirstPage;
            bytesRemaining -= bytesInFirstPage;
            startPage++;
        }
        
        // Handle full pages in the middle
        while (bytesRemaining >= PageSize)
        {
            if (_pages.TryGetValue(startPage, out var page))
            {
                new ReadOnlySpan<byte>(page, 0, PageSize).CopyTo(destination.Slice(destOffset, PageSize));
            }
            else
            {
                // Unallocated pages read as zeros
                destination.Slice(destOffset, PageSize).Clear();
            }
            destOffset += PageSize;
            bytesRemaining -= PageSize;
            startPage++;
        }
        
        // Handle last partial page
        if (bytesRemaining > 0)
        {
            if (_pages.TryGetValue(startPage, out var page))
            {
                new ReadOnlySpan<byte>(page, 0, bytesRemaining).CopyTo(destination.Slice(destOffset, bytesRemaining));
            }
            else
            {
                // Unallocated pages read as zeros
                destination.Slice(destOffset, bytesRemaining).Clear();
            }
        }
    }
    
    /// <summary>
    /// Gets a Memory&lt;byte&gt; view of the specified memory region.
    /// Note: This allocates a new array. For zero-copy access, use TryGetPageMemory when possible.
    /// </summary>
    public Memory<byte> GetMemory(ulong addr, int length)
    {
        if (TryGetPageMemory(addr, length, out var memory))
        {
            return memory;
        }
        return new Memory<byte>(GetSpan(addr, length));
    }
    
    /// <summary>
    /// Tries to get a Memory&lt;byte&gt; view of a single page without copying.
    /// This is only possible if the requested region fits within a single page boundary.
    /// Returns false if the region spans multiple pages or requires allocation.
    /// </summary>
    public bool TryGetPageMemory(ulong addr, int length, out Memory<byte> memory)
    {
    if (length < 0 || length > PageSize)
    {
        memory = Memory<byte>.Empty;
        return false;
    }
        
    if (length == 0)
    {
        memory = Memory<byte>.Empty;
        return true;
    }
        
    uint pageIndex = (uint)(addr >> PageSizeBits);
    uint offset = (uint)(addr & PageMask);
        
        // Check if the request spans multiple pages (use long to prevent overflow)
        if ((long)offset + length > PageSize)
        {
            memory = Memory<byte>.Empty;
            return false;
        }
        
        if (_pages.TryGetValue(pageIndex, out var page))
        {
            memory = new Memory<byte>(page, (int)offset, length);
            return true;
        }
        
        // Page not allocated yet
        memory = Memory<byte>.Empty;
        return false;
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
