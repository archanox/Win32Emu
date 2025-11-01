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
}

/// <summary>
/// Ref struct wrapper for WNDCLASSEXA that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct WndClassExARef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public WndClassExARef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public uint cbSize
	{
		get => _memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, value);
	}

	public uint style
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	public uint lpfnWndProc
	{
		get => _memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, value);
	}

	public int cbClsExtra
	{
		get => (int)_memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, (uint)value);
	}

	public int cbWndExtra
	{
		get => (int)_memory.Read32(_address + 16);
		set => _memory.Write32(_address + 16, (uint)value);
	}

	public uint hInstance
	{
		get => _memory.Read32(_address + 20);
		set => _memory.Write32(_address + 20, value);
	}

	public uint hIcon
	{
		get => _memory.Read32(_address + 24);
		set => _memory.Write32(_address + 24, value);
	}

	public uint hCursor
	{
		get => _memory.Read32(_address + 28);
		set => _memory.Write32(_address + 28, value);
	}

	public uint hbrBackground
	{
		get => _memory.Read32(_address + 32);
		set => _memory.Write32(_address + 32, value);
	}

	public uint lpszMenuName
	{
		get => _memory.Read32(_address + 36);
		set => _memory.Write32(_address + 36, value);
	}

	public uint lpszClassName
	{
		get => _memory.Read32(_address + 40);
		set => _memory.Write32(_address + 40, value);
	}

	public uint hIconSm
	{
		get => _memory.Read32(_address + 44);
		set => _memory.Write32(_address + 44, value);
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.WNDCLASSEXA ToStruct()
	{
		return new NativeTypes.WNDCLASSEXA
		{
			cbSize = cbSize,
			style = style,
			lpfnWndProc = lpfnWndProc,
			cbClsExtra = cbClsExtra,
			cbWndExtra = cbWndExtra,
			hInstance = hInstance,
			hIcon = hIcon,
			hCursor = hCursor,
			hbrBackground = hbrBackground,
			lpszMenuName = lpszMenuName,
			lpszClassName = lpszClassName,
			hIconSm = hIconSm
		};
	}
}

/// <summary>
/// Ref struct wrapper for MSG that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct MsgRef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public MsgRef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public uint hwnd
	{
		get => _memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, value);
	}

	public uint message
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	public uint wParam
	{
		get => _memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, value);
	}

	public uint lParam
	{
		get => _memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, value);
	}

	public uint time
	{
		get => _memory.Read32(_address + 16);
		set => _memory.Write32(_address + 16, value);
	}

	public int ptX
	{
		get => (int)_memory.Read32(_address + 20);
		set => _memory.Write32(_address + 20, unchecked((uint)value));
	}

	public int ptY
	{
		get => (int)_memory.Read32(_address + 24);
		set => _memory.Write32(_address + 24, unchecked((uint)value));
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.MSG ToStruct()
	{
		return new NativeTypes.MSG
		{
			hwnd = hwnd,
			message = message,
			wParam = wParam,
			lParam = lParam,
			time = time,
			ptX = ptX,
			ptY = ptY
		};
	}
}

/// <summary>
/// Ref struct wrapper for RECT that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct RectRef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public RectRef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public int left
	{
		get => (int)_memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, unchecked((uint)value));
	}

	public int top
	{
		get => (int)_memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, unchecked((uint)value));
	}

	public int right
	{
		get => (int)_memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, unchecked((uint)value));
	}

	public int bottom
	{
		get => (int)_memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, unchecked((uint)value));
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.RECT ToStruct()
	{
		return new NativeTypes.RECT
		{
			left = left,
			top = top,
			right = right,
			bottom = bottom
		};
	}
}

/// <summary>
/// Ref struct wrapper for POINT that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct PointRef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public PointRef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public int x
	{
		get => (int)_memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, unchecked((uint)value));
	}

	public int y
	{
		get => (int)_memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, unchecked((uint)value));
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.POINT ToStruct()
	{
		return new NativeTypes.POINT
		{
			x = x,
			y = y
		};
	}
}

/// <summary>
/// Ref struct wrapper for DOCINFOA that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct DocInfoARef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public DocInfoARef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public int cbSize
	{
		get => (int)_memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, (uint)value);
	}

	public uint lpszDocName
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	public uint lpszOutput
	{
		get => _memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, value);
	}

	public uint lpszDatatype
	{
		get => _memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, value);
	}

	public uint fwType
	{
		get => _memory.Read32(_address + 16);
		set => _memory.Write32(_address + 16, value);
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.DOCINFOA ToStruct()
	{
		return new NativeTypes.DOCINFOA
		{
			cbSize = cbSize,
			lpszDocName = lpszDocName,
			lpszOutput = lpszOutput,
			lpszDatatype = lpszDatatype,
			fwType = fwType
		};
	}
}
