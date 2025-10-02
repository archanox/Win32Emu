using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public interface IWin32ModuleUnsafe
{
	string Name { get; }

	// Optional extension: modules can implement pointer-based stubs for selected exports.
	bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue);
}

public class User32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "USER32.DLL";

	public unsafe bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "REGISTERCLASSA":
				returnValue = RegisterClassA(a.UInt32(0));
				return true;

			case "CREATEWINDOWEXA":
				returnValue = CreateWindowExA(
					a.UInt32(0),  // dwExStyle
					a.Lpstr(1),   // lpClassName
					a.Lpstr(2),   // lpWindowName
					a.UInt32(3),  // dwStyle
					a.Int32(4),   // x
					a.Int32(5),   // y
					a.Int32(6),   // nWidth
					a.Int32(7),   // nHeight
					a.UInt32(8),  // hWndParent
					a.UInt32(9),  // hMenu
					a.UInt32(10), // hInstance
					a.UInt32(11)  // lpParam
				);
				return true;

			case "SHOWWINDOW":
				returnValue = ShowWindow(a.UInt32(0), a.Int32(1));
				return true;

			case "GETMESSAGEA":
				returnValue = GetMessageA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "TRANSLATEMESSAGE":
				returnValue = TranslateMessage(a.UInt32(0));
				return true;

			case "DISPATCHMESSAGEA":
				returnValue = DispatchMessageA(a.UInt32(0));
				return true;

			case "DEFWINDOWPROCA":
				returnValue = DefWindowProcA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "POSTQUITMESSAGE":
				PostQuitMessage(a.Int32(0));
				returnValue = 0;
				return true;

			case "SENDMESSAGEA":
				returnValue = SendMessageA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			default:
				Console.WriteLine($"[User32] Unimplemented export: {export}");
				return false;
		}
	}

	private uint RegisterClassA(uint lpWndClass)
	{
		if (lpWndClass == 0)
		{
			Console.WriteLine("[User32] RegisterClassA: NULL WNDCLASS pointer");
			return 0;
		}

		// WNDCLASSA structure layout:
		// UINT      style;         // 0
		// WNDPROC   lpfnWndProc;   // 4
		// int       cbClsExtra;    // 8
		// int       cbWndExtra;    // 12
		// HINSTANCE hInstance;     // 16
		// HICON     hIcon;         // 20
		// HCURSOR   hCursor;       // 24
		// HBRUSH    hbrBackground; // 28
		// LPCSTR    lpszMenuName;  // 32
		// LPCSTR    lpszClassName; // 36

		var style = env.MemRead32(lpWndClass + 0);
		var wndProc = env.MemRead32(lpWndClass + 4);
		var clsExtra = (int)env.MemRead32(lpWndClass + 8);
		var wndExtra = (int)env.MemRead32(lpWndClass + 12);
		var hInstance = env.MemRead32(lpWndClass + 16);
		var hIcon = env.MemRead32(lpWndClass + 20);
		var hCursor = env.MemRead32(lpWndClass + 24);
		var hbrBackground = env.MemRead32(lpWndClass + 28);
		var menuNamePtr = env.MemRead32(lpWndClass + 32);
		var classNamePtr = env.MemRead32(lpWndClass + 36);

		if (classNamePtr == 0)
		{
			Console.WriteLine("[User32] RegisterClassA: NULL class name");
			return 0;
		}

		var className = env.ReadAnsiString(classNamePtr);
		var menuName = menuNamePtr != 0 ? env.ReadAnsiString(menuNamePtr) : null;

		var classInfo = new ProcessEnvironment.WindowClassInfo(
			className, style, wndProc, clsExtra, wndExtra,
			hInstance, hIcon, hCursor, hbrBackground, menuName
		);

		if (env.RegisterWindowClass(className, classInfo))
		{
			// Return an ATOM (non-zero value) on success
			// Windows uses atoms (16-bit values) for class registration
			// We'll return a simple non-zero value to indicate success
			var atom = (uint)(className.GetHashCode() & 0xFFFF);
			if (atom == 0) atom = 1;
			Console.WriteLine($"[User32] RegisterClassA: '{className}' -> atom 0x{atom:X4}");
			return atom;
		}

		Console.WriteLine($"[User32] RegisterClassA: Failed to register '{className}'");
		return 0;
	}

	private unsafe uint CreateWindowExA(
		uint dwExStyle,
		sbyte* lpClassName,
		sbyte* lpWindowName,
		uint dwStyle,
		int x,
		int y,
		int nWidth,
		int nHeight,
		uint hWndParent,
		uint hMenu,
		uint hInstance,
		uint lpParam)
	{
		var classNamePtr = (uint)(nint)lpClassName;
		var windowNamePtr = (uint)(nint)lpWindowName;

		if (classNamePtr == 0)
		{
			Console.WriteLine("[User32] CreateWindowExA: NULL class name");
			return 0;
		}

		var className = env.ReadAnsiString(classNamePtr);
		var windowName = windowNamePtr != 0 ? env.ReadAnsiString(windowNamePtr) : "";

		// Check if window class is registered
		if (!env.IsWindowClassRegistered(className))
		{
			Console.WriteLine($"[User32] CreateWindowExA: Window class '{className}' not registered");
			return 0;
		}

		// Handle CW_USEDEFAULT for position and size
		const int CW_USEDEFAULT = unchecked((int)0x80000000);
		if (x == CW_USEDEFAULT) x = 100;
		if (y == CW_USEDEFAULT) y = 100;
		if (nWidth == CW_USEDEFAULT) nWidth = 640;
		if (nHeight == CW_USEDEFAULT) nHeight = 480;

		var hwnd = env.CreateWindow(
			className, windowName, dwStyle, dwExStyle,
			x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam
		);

		if (hwnd != 0)
		{
			Console.WriteLine($"[User32] CreateWindowExA: Created HWND=0x{hwnd:X8} Class='{className}' Title='{windowName}'");
		}
		else
		{
			Console.WriteLine($"[User32] CreateWindowExA: Failed to create window");
		}

		return hwnd;
	}

	private uint ShowWindow(uint hwnd, int nCmdShow)
	{
		// SW_HIDE = 0, SW_NORMAL = 1, SW_SHOWMINIMIZED = 2, SW_SHOWMAXIMIZED = 3, etc.
		Console.WriteLine($"[User32] ShowWindow: HWND=0x{hwnd:X8} nCmdShow={nCmdShow}");
		
		// For now, just log and return TRUE (non-zero)
		// In a full implementation, this would interact with the Avalonia window
		return 1;
	}

	private uint GetMessageA(uint lpMsg, uint hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
	{
		// MSG structure layout (28 bytes):
		// HWND   hwnd;      // 0
		// UINT   message;   // 4
		// WPARAM wParam;    // 8
		// LPARAM lParam;    // 12
		// DWORD  time;      // 16
		// POINT  pt;        // 20 (x, y each 4 bytes)

		if (lpMsg == 0)
		{
			Console.WriteLine("[User32] GetMessageA: NULL MSG pointer");
			return 0;
		}

		// Check if there's a quit message
		if (env.HasQuitMessage())
		{
			var exitCode = env.GetQuitExitCode();
			Console.WriteLine($"[User32] GetMessageA: WM_QUIT (exitCode={exitCode})");
			
			// Fill MSG structure with WM_QUIT
			env.MemWrite32(lpMsg + 0, 0);      // hwnd = NULL
			env.MemWrite32(lpMsg + 4, 0x0012); // WM_QUIT = 0x0012
			env.MemWrite32(lpMsg + 8, (uint)exitCode); // wParam = exit code
			env.MemWrite32(lpMsg + 12, 0);     // lParam = 0
			env.MemWrite32(lpMsg + 16, 0);     // time = 0
			env.MemWrite32(lpMsg + 20, 0);     // pt.x = 0
			env.MemWrite32(lpMsg + 24, 0);     // pt.y = 0
			
			return 0; // GetMessage returns 0 for WM_QUIT
		}

		// For now, simulate no messages (would block in real implementation)
		// In a real implementation, this would wait for messages or return pending messages
		Console.WriteLine("[User32] GetMessageA: No messages, simulating empty queue");
		
		// Return a dummy message (WM_NULL)
		env.MemWrite32(lpMsg + 0, hWnd);   // hwnd
		env.MemWrite32(lpMsg + 4, 0);      // WM_NULL = 0
		env.MemWrite32(lpMsg + 8, 0);      // wParam = 0
		env.MemWrite32(lpMsg + 12, 0);     // lParam = 0
		env.MemWrite32(lpMsg + 16, 0);     // time = 0
		env.MemWrite32(lpMsg + 20, 0);     // pt.x = 0
		env.MemWrite32(lpMsg + 24, 0);     // pt.y = 0
		
		return 1; // GetMessage returns non-zero for all messages except WM_QUIT
	}

	private uint TranslateMessage(uint lpMsg)
	{
		// TranslateMessage translates virtual-key messages into character messages
		// For now, just log and return FALSE (no translation occurred)
		Console.WriteLine("[User32] TranslateMessage: Called");
		return 0;
	}

	private uint DispatchMessageA(uint lpMsg)
	{
		if (lpMsg == 0)
		{
			Console.WriteLine("[User32] DispatchMessageA: NULL MSG pointer");
			return 0;
		}

		// Read MSG structure
		var hwnd = env.MemRead32(lpMsg + 0);
		var message = env.MemRead32(lpMsg + 4);
		var wParam = env.MemRead32(lpMsg + 8);
		var lParam = env.MemRead32(lpMsg + 12);

		Console.WriteLine($"[User32] DispatchMessageA: HWND=0x{hwnd:X8} MSG=0x{message:X4} wParam=0x{wParam:X8} lParam=0x{lParam:X8}");

		// In a full implementation, this would call the window procedure
		// For now, just return 0 (message processed)
		return 0;
	}

	private uint DefWindowProcA(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		Console.WriteLine($"[User32] DefWindowProcA: HWND=0x{hwnd:X8} MSG=0x{msg:X4} wParam=0x{wParam:X8} lParam=0x{lParam:X8}");
		
		// DefWindowProc provides default processing for window messages
		// For now, just return 0 (message processed)
		return 0;
	}

	private void PostQuitMessage(int nExitCode)
	{
		Console.WriteLine($"[User32] PostQuitMessage: exitCode={nExitCode}");
		env.PostQuitMessage(nExitCode);
	}

	private uint SendMessageA(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		Console.WriteLine($"[User32] SendMessageA: HWND=0x{hwnd:X8} MSG=0x{msg:X4} wParam=0x{wParam:X8} lParam=0x{lParam:X8}");
		
		// SendMessage sends a message to the window procedure
		// For now, just log and return 0 (message processed)
		return 0;
	}
}

public class Gdi32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "GDI32.DLL";

	// Stock object handles - these are pseudo-handles that don't require cleanup
	private readonly Dictionary<int, uint> _stockObjects = new();
	private uint _nextStockObjectHandle = 0x80000000; // Start with high address to distinguish from regular handles

	// Device contexts
	private readonly Dictionary<uint, DeviceContext> _deviceContexts = new();
	private uint _nextDCHandle = 0x81000000;

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "GETSTOCKOBJECT":
				returnValue = GetStockObject(a.Int32(0));
				return true;

			case "BEGINPAINT":
				returnValue = BeginPaint(a.UInt32(0), a.UInt32(1));
				return true;

			case "ENDPAINT":
				returnValue = EndPaint(a.UInt32(0), a.UInt32(1));
				return true;

			case "FILLRECT":
				returnValue = FillRect(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "TEXTOUT":
			case "TEXTOUTA":
				returnValue = TextOutA(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.Int32(4));
				return true;

			case "SETBKMODE":
				returnValue = SetBkMode(a.UInt32(0), a.Int32(1));
				return true;

			case "SETTEXTCOLOR":
				returnValue = SetTextColor(a.UInt32(0), a.UInt32(1));
				return true;

			default:
				Console.WriteLine($"[Gdi32] Unimplemented export: {export}");
				return false;
		}
	}

	private uint GetStockObject(int stockObjectId)
	{
		// Validate stock object ID
		if (stockObjectId < NativeTypes.StockObject.WHITE_BRUSH || 
		    stockObjectId > NativeTypes.StockObject.DC_PEN)
		{
			Console.WriteLine($"[Gdi32] GetStockObject: Invalid stock object ID {stockObjectId}");
			return 0;
		}

		// Return cached handle or create a new one
		if (_stockObjects.TryGetValue(stockObjectId, out var handle))
		{
			return handle;
		}

		// Create a pseudo-handle for this stock object
		handle = _nextStockObjectHandle++;
		_stockObjects[stockObjectId] = handle;

		Console.WriteLine($"[Gdi32] GetStockObject({stockObjectId}) -> 0x{handle:X8}");
		return handle;
	}

	private uint BeginPaint(uint hwnd, uint lpPaint)
	{
		Console.WriteLine($"[Gdi32] BeginPaint(HWND=0x{hwnd:X8}, lpPaint=0x{lpPaint:X8})");

		// Create a device context for this paint session
		var hdc = _nextDCHandle++;
		var dc = new DeviceContext
		{
			Handle = hdc,
			WindowHandle = hwnd
		};
		_deviceContexts[hdc] = dc;

		// Fill PAINTSTRUCT if provided
		if (lpPaint != 0)
		{
			// PAINTSTRUCT layout:
			// HDC hdc
			// BOOL fErase
			// RECT rcPaint
			// BOOL fRestore
			// BOOL fIncUpdate
			// BYTE rgbReserved[32]
			env.MemWrite32(lpPaint, hdc);      // hdc
			env.MemWrite32(lpPaint + 4, 1);    // fErase = TRUE
			env.MemWrite32(lpPaint + 8, 0);    // rcPaint.left
			env.MemWrite32(lpPaint + 12, 0);   // rcPaint.top
			env.MemWrite32(lpPaint + 16, 640); // rcPaint.right
			env.MemWrite32(lpPaint + 20, 480); // rcPaint.bottom
		}

		return hdc;
	}

	private uint EndPaint(uint hwnd, uint lpPaint)
	{
		if (lpPaint != 0)
		{
			var hdc = env.MemRead32(lpPaint);
			Console.WriteLine($"[Gdi32] EndPaint(HWND=0x{hwnd:X8}, HDC=0x{hdc:X8})");

			// Remove the device context
			_deviceContexts.Remove(hdc);
		}

		return 1; // TRUE
	}

	private uint FillRect(uint hdc, uint lpRect, uint hBrush)
	{
		if (lpRect != 0)
		{
			var left = env.MemRead32(lpRect);
			var top = env.MemRead32(lpRect + 4);
			var right = env.MemRead32(lpRect + 8);
			var bottom = env.MemRead32(lpRect + 12);
			Console.WriteLine($"[Gdi32] FillRect(HDC=0x{hdc:X8}, rect=({left},{top},{right},{bottom}), hBrush=0x{hBrush:X8})");
		}

		return 1; // Non-zero on success
	}

	private uint TextOutA(uint hdc, int x, int y, uint lpString, int cbString)
	{
		if (lpString != 0 && cbString > 0)
		{
			var text = env.ReadAnsiString(lpString, cbString);
			Console.WriteLine($"[Gdi32] TextOutA(HDC=0x{hdc:X8}, x={x}, y={y}, text=\"{text}\")");
		}

		return 1; // TRUE
	}

	private uint SetBkMode(uint hdc, int mode)
	{
		Console.WriteLine($"[Gdi32] SetBkMode(HDC=0x{hdc:X8}, mode={mode})");
		if (_deviceContexts.TryGetValue(hdc, out var dc))
		{
			int previous = dc.BkMode;
			dc.BkMode = mode;
			return (uint)previous;
		}
		return 0; // Default: TRANSPARENT
	}

	private uint SetTextColor(uint hdc, uint color)
	{
		Console.WriteLine($"[Gdi32] SetTextColor(HDC=0x{hdc:X8}, color=0x{color:X8})");
		return 0x00000000; // Previous color (black)
	}

	private class DeviceContext
	{
		public uint Handle { get; set; }
		public uint WindowHandle { get; set; }
		public int BkMode { get; set; } = 2; // OPAQUE
		public uint TextColor { get; set; } = 0x00000000; // Black
	}
}

public class DDrawModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "DDRAW.DLL";

	// DirectDraw object handles
	private readonly Dictionary<uint, DirectDrawObject> _ddrawObjects = new();
	private readonly Dictionary<uint, DirectDrawSurface> _surfaces = new();
	private uint _nextDDrawHandle = 0x70000000;
	private uint _nextSurfaceHandle = 0x71000000;

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "DIRECTDRAWCREATE":
				returnValue = DirectDrawCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "DIRECTDRAWCREATEEX":
				returnValue = DirectDrawCreateEx(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			default:
				Console.WriteLine($"[DDraw] Unimplemented export: {export}");
				return false;
		}
	}

	private uint DirectDrawCreate(uint lpGuid, uint lplpDD, uint pUnkOuter)
	{
		Console.WriteLine($"[DDraw] DirectDrawCreate(lpGuid=0x{lpGuid:X8}, lplpDD=0x{lplpDD:X8}, pUnkOuter=0x{pUnkOuter:X8})");

		// Create DirectDraw object
		var ddrawHandle = _nextDDrawHandle++;
		var ddrawObj = new DirectDrawObject
		{
			Handle = ddrawHandle
		};
		_ddrawObjects[ddrawHandle] = ddrawObj;

		// Write vtable pointer back to caller
		if (lplpDD != 0)
		{
			env.MemWrite32(lplpDD, ddrawHandle);
		}

		Console.WriteLine($"[DDraw] Created DirectDraw object: 0x{ddrawHandle:X8}");
		return 0; // DD_OK
	}

	private uint DirectDrawCreateEx(uint lpGuid, uint lplpDD, uint iid, uint pUnkOuter)
	{
		Console.WriteLine($"[DDraw] DirectDrawCreateEx(lpGuid=0x{lpGuid:X8}, lplpDD=0x{lplpDD:X8}, iid=0x{iid:X8}, pUnkOuter=0x{pUnkOuter:X8})");

		// Similar to DirectDrawCreate but with interface ID
		var ddrawHandle = _nextDDrawHandle++;
		var ddrawObj = new DirectDrawObject
		{
			Handle = ddrawHandle
		};
		_ddrawObjects[ddrawHandle] = ddrawObj;

		if (lplpDD != 0)
		{
			env.MemWrite32(lplpDD, ddrawHandle);
		}

		Console.WriteLine($"[DDraw] Created DirectDraw object (Ex): 0x{ddrawHandle:X8}");
		return 0; // DD_OK
	}

	private class DirectDrawObject
	{
		public uint Handle { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int BitsPerPixel { get; set; }
	}

	private class DirectDrawSurface
	{
		public uint Handle { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Pitch { get; set; }
		public byte[]? Bits { get; set; }
		public bool IsPrimary { get; set; }
	}
}

public class DSoundModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "DSOUND.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[DSound] Unimplemented export: {export}");
		return false;
	}
}


public class DInputModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "DINPUT.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[DInput] Unimplemented export: {export}");
		return false;
	}
}

public class WinMMModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "WINMM.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[WinMM] Unimplemented export: {export}");
		return false;
	}
}

public class Glide2xModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "GLIDE2X.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		//var a = new StackArgs(cpu, memory);

		Console.WriteLine($"[Glide2x] Unimplemented export: {export}");
		return false;
	}
}