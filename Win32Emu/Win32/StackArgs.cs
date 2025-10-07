using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public readonly ref struct StackArgs(ICpu cpu, VirtualMemory mem)
{
	private readonly uint _esp = cpu.GetRegister("ESP");

	public uint UInt32(int index) => mem.Read32(_esp + (uint)((index + 1) * 4));
	public int Int32(int index) => (int)mem.Read32(_esp + (uint)((index + 1) * 4));
	public NativeTypes.HModule HModule(int index) => new NativeTypes.HModule(UInt32(index));

	// Unsafe-style helpers if needed
	public unsafe void* Ptr(int index) => (void*)UInt32(index);
	
	public unsafe sbyte* Lpstr(int index) => (sbyte*)UInt32(index);
	
	public unsafe char* Lpcstr(int index) => (char*)UInt32(index);
	
	public LpcStr LpcStr(int index) => new LpcStr(UInt32(index));
	
	public LpWStr LpWStr(int index) => new LpWStr(UInt32(index));
}