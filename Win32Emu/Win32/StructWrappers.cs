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

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.WNDCLASSEXA(WndClassExARef refStruct) => refStruct.ToStruct();
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

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.MSG(MsgRef refStruct) => refStruct.ToStruct();
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

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.RECT(RectRef refStruct) => refStruct.ToStruct();
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

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.POINT(PointRef refStruct) => refStruct.ToStruct();
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

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.DOCINFOA(DocInfoARef refStruct) => refStruct.ToStruct();
}

/// <summary>
/// Ref struct wrapper for SCROLLINFO that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct ScrollInfoRef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public ScrollInfoRef(VirtualMemory memory, uint address)
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

	public uint fMask
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	public int nMin
	{
		get => (int)_memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, unchecked((uint)value));
	}

	public int nMax
	{
		get => (int)_memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, unchecked((uint)value));
	}

	public uint nPage
	{
		get => _memory.Read32(_address + 16);
		set => _memory.Write32(_address + 16, value);
	}

	public int nPos
	{
		get => (int)_memory.Read32(_address + 20);
		set => _memory.Write32(_address + 20, unchecked((uint)value));
	}

	public int nTrackPos
	{
		get => (int)_memory.Read32(_address + 24);
		set => _memory.Write32(_address + 24, unchecked((uint)value));
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.SCROLLINFO ToStruct()
	{
		return new NativeTypes.SCROLLINFO
		{
			cbSize = cbSize,
			fMask = fMask,
			nMin = nMin,
			nMax = nMax,
			nPage = nPage,
			nPos = nPos,
			nTrackPos = nTrackPos
		};
	}

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.SCROLLINFO(ScrollInfoRef refStruct) => refStruct.ToStruct();
}

/// <summary>
/// Ref struct wrapper for PAINTSTRUCT that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// Note: rcPaint is represented as a RectRef at offset 8.
/// </summary>
public readonly ref struct PaintStructRef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public PaintStructRef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public uint hdc
	{
		get => _memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, value);
	}

	public uint fErase
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	/// <summary>
	/// Gets a RectRef for the rcPaint field (at offset 8).
	/// </summary>
	public RectRef rcPaint => new RectRef(_memory, _address + 8);

	public uint fRestore
	{
		get => _memory.Read32(_address + 24);
		set => _memory.Write32(_address + 24, value);
	}

	public uint fIncUpdate
	{
		get => _memory.Read32(_address + 28);
		set => _memory.Write32(_address + 28, value);
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.PAINTSTRUCT ToStruct()
	{
		var rect = rcPaint;
		return new NativeTypes.PAINTSTRUCT
		{
			hdc = hdc,
			fErase = fErase,
			rcPaintLeft = rect.left,
			rcPaintTop = rect.top,
			rcPaintRight = rect.right,
			rcPaintBottom = rect.bottom,
			fRestore = fRestore,
			fIncUpdate = fIncUpdate
		};
	}

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.PAINTSTRUCT(PaintStructRef refStruct) => refStruct.ToStruct();
}

/// <summary>
/// Ref struct wrapper for DDSURFACEDESC that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct DDSurfaceDescRef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public DDSurfaceDescRef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public uint dwSize
	{
		get => _memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, value);
	}

	public uint dwFlags
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	public uint dwWidth
	{
		get => _memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, value);
	}

	public uint dwHeight
	{
		get => _memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, value);
	}

	public uint lPitch
	{
		get => _memory.Read32(_address + 16);
		set => _memory.Write32(_address + 16, value);
	}

	public uint dwBackBufferCount
	{
		get => _memory.Read32(_address + 20);
		set => _memory.Write32(_address + 20, value);
	}

	public uint dwSurfaceCaps
	{
		get => _memory.Read32(_address + 108);
		set => _memory.Write32(_address + 108, value);
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.DDSURFACEDESC ToStruct()
	{
		return new NativeTypes.DDSURFACEDESC
		{
			dwSize = dwSize,
			dwFlags = dwFlags,
			dwWidth = dwWidth,
			dwHeight = dwHeight,
			lPitch = lPitch,
			dwBackBufferCount = dwBackBufferCount
		};
	}

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.DDSURFACEDESC(DDSurfaceDescRef refStruct) => refStruct.ToStruct();
}

/// <summary>
/// Ref struct wrapper for DIPROPHEADER that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct DiPropHeaderRef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public DiPropHeaderRef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public uint dwSize
	{
		get => _memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, value);
	}

	public uint dwHeaderSize
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	public uint dwObj
	{
		get => _memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, value);
	}

	public uint dwHow
	{
		get => _memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, value);
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.DIPROPHEADER ToStruct()
	{
		return new NativeTypes.DIPROPHEADER
		{
			dwSize = dwSize,
			dwHeaderSize = dwHeaderSize,
			dwObj = dwObj,
			dwHow = dwHow
		};
	}

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.DIPROPHEADER(DiPropHeaderRef refStruct) => refStruct.ToStruct();
}

/// <summary>
/// Ref struct wrapper for DIDATAFORMAT that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct DiDataFormatRef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public DiDataFormatRef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	public uint dwSize
	{
		get => _memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, value);
	}

	public uint dwObjSize
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	public uint dwFlags
	{
		get => _memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, value);
	}

	public uint dwDataSize
	{
		get => _memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, value);
	}

	public uint dwNumObjs
	{
		get => _memory.Read32(_address + 16);
		set => _memory.Write32(_address + 16, value);
	}

	public uint rgodf
	{
		get => _memory.Read32(_address + 20);
		set => _memory.Write32(_address + 20, value);
	}

	/// <summary>
	/// Converts this ref struct to a value struct snapshot.
	/// </summary>
	public NativeTypes.DIDATAFORMAT ToStruct()
	{
		return new NativeTypes.DIDATAFORMAT
		{
			dwSize = dwSize,
			dwObjSize = dwObjSize,
			dwFlags = dwFlags,
			dwDataSize = dwDataSize,
			dwNumObjs = dwNumObjs,
			rgodf = rgodf
		};
	}

	/// <summary>
	/// Implicit conversion to the underlying value struct.
	/// </summary>
	public static implicit operator NativeTypes.DIDATAFORMAT(DiDataFormatRef refStruct) => refStruct.ToStruct();
}
