namespace Win32Emu.Memory;

/// <summary>
/// Very early simplistic linear virtual memory model for 32-bit address space.
/// Backed by a single byte[] region. No paging yet.
/// </summary>
public class VirtualMemory(ulong size = VirtualMemory.DefaultSize)
{
	private const ulong DefaultSize = 512 * 1024 * 1024; // 256 MB = 268,435,456
	private readonly byte[] _mem = new byte[size];

    public ulong Size => (ulong)_mem.LongLength;

    public byte Read8(ulong addr) => _mem[addr];
    public ushort Read16(ulong addr) => (ushort)(Read8(addr) | (Read8(addr + 1) << 8));
    public uint Read32(ulong addr) => (uint)(Read16(addr) | (Read16(addr + 2) << 16));

    public void Write8(ulong addr, byte value) => _mem[addr] = value;
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

    public void WriteBytes(ulong addr, ReadOnlySpan<byte> data)
    {
        data.CopyTo(new Span<byte>(_mem, (int)addr, data.Length));
    }

    public byte[] GetSpan(ulong addr, int length) => _mem.AsSpan((int)addr, length).ToArray();
}
