using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public readonly ref struct StackArgs(ICpu cpu, VirtualMemory mem, uint baseOffset = 0)
{
	private readonly uint _esp = cpu.GetRegister("ESP") + baseOffset;

	public uint UInt32(int index) => mem.Read32(_esp + (uint)((index + 1) * 4));
	public int Int32(int index) => (int)mem.Read32(_esp + (uint)((index + 1) * 4));
	public NativeTypes.HModule HModule(int index) => new NativeTypes.HModule(UInt32(index));

	// Unsafe-style helpers if needed
	public unsafe void* Ptr(int index) => (void*)UInt32(index);
	
	public unsafe sbyte* Lpstr(int index) => (sbyte*)UInt32(index);
	
	public unsafe char* Lpcstr(int index) => (char*)UInt32(index);
	
	public LpStr LpStr(int index) => new LpStr(UInt32(index));
	
	public LpcStr LpcStr(int index) => new LpcStr(UInt32(index), mem);
	
	public LpWStr LpWStr(int index) => new LpWStr(UInt32(index));
	
	public unsafe NativeTypes.Lpcpinfo Lpcpinfo(int index) => (NativeTypes.Cpinfo*)UInt32(index);
	
	// Ref struct wrappers for Win32 structures with automatic memory read/write
	public WndClassARef WndClassA(int index) => new WndClassARef(mem, UInt32(index));
	
	public WndClassExARef WndClassExA(int index) => new WndClassExARef(mem, UInt32(index));
	
	public MsgRef Msg(int index) => new MsgRef(mem, UInt32(index));
	
	public RectRef Rect(int index) => new RectRef(mem, UInt32(index));
	
	public PointRef Point(int index) => new PointRef(mem, UInt32(index));
	
	public DocInfoARef DocInfoA(int index) => new DocInfoARef(mem, UInt32(index));
	
	public ScrollInfoRef ScrollInfo(int index) => new ScrollInfoRef(mem, UInt32(index));
	
	public PaintStructRef PaintStruct(int index) => new PaintStructRef(mem, UInt32(index));
	
	public DDSurfaceDescRef DDSurfaceDesc(int index) => new DDSurfaceDescRef(mem, UInt32(index));
	
	public DiPropHeaderRef DiPropHeader(int index) => new DiPropHeaderRef(mem, UInt32(index));
	
	public DiDataFormatRef DiDataFormat(int index) => new DiDataFormatRef(mem, UInt32(index));
	
	public FileTimeRef FileTime(int index) => new FileTimeRef(mem, UInt32(index));
	
	public SystemTimeRef SystemTime(int index) => new SystemTimeRef(mem, UInt32(index));
	
	public WaveFormatExRef WaveFormatEx(int index) => new WaveFormatExRef(mem, UInt32(index));
	
	public DDColorKeyRef DDColorKey(int index) => new DDColorKeyRef(mem, UInt32(index));
	
	public AcmStreamHeaderRef AcmStreamHeader(int index) => new AcmStreamHeaderRef(mem, UInt32(index));
}