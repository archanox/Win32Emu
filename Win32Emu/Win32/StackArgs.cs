using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public readonly ref struct StackArgs(ICpu cpu, VirtualMemory mem)
{
	private readonly uint esp = cpu.GetRegister("ESP");

	public uint UInt32(int index) => mem.Read32(esp + (uint)((index + 1) * 4));
	public NativeTypes.HModule HModule(int index) => new NativeTypes.HModule(UInt32(index));

	// Unsafe-style helpers if needed
	public unsafe void* Ptr(int index) => (void*)UInt32(index);
	public unsafe sbyte* Lpstr(int index) => (sbyte*)UInt32(index);
}