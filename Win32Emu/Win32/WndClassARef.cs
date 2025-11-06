using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Ref struct wrapper for WNDCLASSA that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct WndClassARef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public WndClassARef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public uint style
	{
		get => _memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, value);
	}

	public uint lpfnWndProc
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	public int cbClsExtra
	{
		get => (int)_memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, (uint)value);
	}

	public int cbWndExtra
	{
		get => (int)_memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, (uint)value);
	}

	public uint hInstance
	{
		get => _memory.Read32(_address + 16);
		set => _memory.Write32(_address + 16, value);
	}

	public uint hIcon
	{
		get => _memory.Read32(_address + 20);
		set => _memory.Write32(_address + 20, value);
	}

	public uint hCursor
	{
		get => _memory.Read32(_address + 24);
		set => _memory.Write32(_address + 24, value);
	}

	public uint hbrBackground
	{
		get => _memory.Read32(_address + 28);
		set => _memory.Write32(_address + 28, value);
	}

	public uint lpszMenuName
	{
		get => _memory.Read32(_address + 32);
		set => _memory.Write32(_address + 32, value);
	}

	public uint lpszClassName
	{
		get => _memory.Read32(_address + 36);
		set => _memory.Write32(_address + 36, value);
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.WNDCLASSA ToStruct()
	{
		return new NativeTypes.WNDCLASSA
		{
			style = style,
			lpfnWndProc = lpfnWndProc,
			cbClsExtra = cbClsExtra,
			cbWndExtra = cbWndExtra,
			hInstance = hInstance,
			hIcon = hIcon,
			hCursor = hCursor,
			hbrBackground = hbrBackground,
			lpszMenuName = lpszMenuName,
			lpszClassName = lpszClassName
		};
	}

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.WNDCLASSA(WndClassARef refStruct) => refStruct.ToStruct();
}