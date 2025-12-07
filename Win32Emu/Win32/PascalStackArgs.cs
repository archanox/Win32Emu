using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Stack argument reader for Pascal calling convention (Win16).
/// Pascal convention pushes arguments left-to-right, opposite of stdcall's right-to-left.
/// This means the first parameter is at the highest stack offset, last parameter at the lowest.
/// </summary>
/// <remarks>
/// For a function with N parameters in Pascal convention:
/// - param[0] is at ESP + (N * 4) - first pushed, highest offset
/// - param[1] is at ESP + ((N-1) * 4)
/// - ...
/// - param[N-1] is at ESP + 4 - last pushed, lowest offset
/// 
/// This is the opposite of stdcall where param[0] is at ESP+4 (lowest offset).
/// </remarks>
public readonly ref struct PascalStackArgs(ICpu cpu, VirtualMemory mem, int paramCount, uint baseOffset = 0)
{
	private readonly uint _esp = cpu.GetRegister("ESP") + baseOffset;
	private readonly int _paramCount = paramCount;

	/// <summary>
	/// Read a uint32 parameter at the given logical index (0-based).
	/// The index is reversed to account for left-to-right push order.
	/// </summary>
	public uint UInt32(int index) => mem.Read32(_esp + (uint)((_paramCount - index) * 4));
	
	/// <summary>
	/// Read an int32 parameter at the given logical index (0-based).
	/// The index is reversed to account for left-to-right push order.
	/// </summary>
	public int Int32(int index) => (int)mem.Read32(_esp + (uint)((_paramCount - index) * 4));
	
	public NativeTypes.HModule HModule(int index) => new NativeTypes.HModule(UInt32(index));

	// Managed helper methods - return uint addresses instead of pointers
	public uint Ptr(int index) => UInt32(index);
	
	public uint Lpstr(int index) => UInt32(index);
	
	public uint Lpcstr(int index) => UInt32(index);
	
	public LpStr LpStr(int index) => new LpStr(UInt32(index));
	
	public LpcStr LpcStr(int index) => new LpcStr(UInt32(index), mem);
	
	public LpWStr LpWStr(int index) => new LpWStr(UInt32(index));
	
	public LpcWStr LpcWStr(int index) => new LpcWStr(UInt32(index), mem);
	
	public uint Lpcpinfo(int index) => UInt32(index);
	
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
	
	public DDPixelFormatRef DDPixelFormat(int index) => new DDPixelFormatRef(mem, UInt32(index));
	
	public StartupInfoARef StartupInfoA(int index) => new StartupInfoARef(mem, UInt32(index));
	
	public ExceptionPointersRef ExceptionPointers(int index) => new ExceptionPointersRef(mem, UInt32(index));
	
	public ExceptionRecordRef ExceptionRecord(int index) => new ExceptionRecordRef(mem, UInt32(index));
}
