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

			case "PEEKMESSAGEA":
				returnValue = PeekMessageA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
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

			case "POSTMESSAGEA":
				returnValue = PostMessageA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "SENDMESSAGEA":
				returnValue = SendMessageA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "CLIENTTOSCREEN":
				returnValue = ClientToScreen(a.UInt32(0), a.UInt32(1));
				return true;

			case "SETRECT":
				returnValue = SetRect(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4));
				return true;

			case "GETCLIENTRECT":
				returnValue = GetClientRect(a.UInt32(0), a.UInt32(1));
				return true;

			case "GETWINDOWRECT":
				returnValue = GetWindowRect(a.UInt32(0), a.UInt32(1));
				return true;

			case "ADJUSTWINDOWRECTEX":
				returnValue = AdjustWindowRectEx(a.UInt32(0), a.UInt32(1), a.Int32(2), a.UInt32(3));
				return true;

			case "GETDC":
				returnValue = GetDC(a.UInt32(0));
				return true;

			case "RELEASEDC":
				returnValue = ReleaseDC(a.UInt32(0), a.UInt32(1));
				return true;

			case "UPDATEWINDOW":
				returnValue = UpdateWindow(a.UInt32(0));
				return true;

			case "DESTROYWINDOW":
				returnValue = DestroyWindow(a.UInt32(0));
				return true;

			case "SETWINDOWPOS":
				returnValue = SetWindowPos(a.UInt32(0), a.UInt32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.Int32(5), a.UInt32(6));
				return true;

			case "GETSYSTEMMETRICS":
				returnValue = (uint)GetSystemMetrics(a.Int32(0));
				return true;

			case "LOADICONA":
				returnValue = LoadIconA(a.UInt32(0), a.UInt32(1));
				return true;

			case "LOADCURSORA":
				returnValue = LoadCursorA(a.UInt32(0), a.UInt32(1));
				return true;

			case "SETCURSOR":
				returnValue = SetCursor(a.UInt32(0));
				return true;

			case "SETFOCUS":
				returnValue = SetFocus(a.UInt32(0));
				return true;

			case "GETMENU":
				returnValue = GetMenu(a.UInt32(0));
				return true;

			case "SETWINDOWLONGA":
				returnValue = SetWindowLongA(a.UInt32(0), a.Int32(1), a.UInt32(2));
				return true;

			case "GETWINDOWLONGA":
				returnValue = GetWindowLongA(a.UInt32(0), a.Int32(1));
				return true;

			case "MESSAGEBOXA":
				returnValue = MessageBoxA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "SYSTEMPARAMETERSINFOA":
				returnValue = SystemParametersInfoA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
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

	private uint ClientToScreen(uint hwnd, uint lpPoint)
	{
		if (lpPoint == 0)
			return 0;

		// POINT structure: LONG x, LONG y (8 bytes)
		var x = (int)env.MemRead32(lpPoint);
		var y = (int)env.MemRead32(lpPoint + 4);

		Console.WriteLine($"[User32] ClientToScreen: HWND=0x{hwnd:X8} Point=({x},{y})");

		// For now, treat client coordinates same as screen coordinates (no offset)
		// In a real implementation, this would add window position to client coords
		return 1; // TRUE
	}

	private uint SetRect(uint lpRect, int left, int top, int right, int bottom)
	{
		if (lpRect == 0)
			return 0;

		Console.WriteLine($"[User32] SetRect: lpRect=0x{lpRect:X8} ({left},{top},{right},{bottom})");

		// RECT structure: LONG left, top, right, bottom (16 bytes)
		env.MemWrite32(lpRect, (uint)left);
		env.MemWrite32(lpRect + 4, (uint)top);
		env.MemWrite32(lpRect + 8, (uint)right);
		env.MemWrite32(lpRect + 12, (uint)bottom);

		return 1; // TRUE
	}

	private uint GetClientRect(uint hwnd, uint lpRect)
	{
		if (lpRect == 0)
			return 0;

		Console.WriteLine($"[User32] GetClientRect: HWND=0x{hwnd:X8}");

		// Return a default client rect (0, 0, 640, 480)
		env.MemWrite32(lpRect, 0);       // left
		env.MemWrite32(lpRect + 4, 0);   // top
		env.MemWrite32(lpRect + 8, 640); // right
		env.MemWrite32(lpRect + 12, 480); // bottom

		return 1; // TRUE
	}

	private uint GetWindowRect(uint hwnd, uint lpRect)
	{
		if (lpRect == 0)
			return 0;

		Console.WriteLine($"[User32] GetWindowRect: HWND=0x{hwnd:X8}");

		// Return a default window rect (100, 100, 740, 580)
		env.MemWrite32(lpRect, 100);     // left
		env.MemWrite32(lpRect + 4, 100); // top
		env.MemWrite32(lpRect + 8, 740); // right
		env.MemWrite32(lpRect + 12, 580); // bottom

		return 1; // TRUE
	}

	private uint AdjustWindowRectEx(uint lpRect, uint dwStyle, int bMenu, uint dwExStyle)
	{
		if (lpRect == 0)
			return 0;

		var left = (int)env.MemRead32(lpRect);
		var top = (int)env.MemRead32(lpRect + 4);
		var right = (int)env.MemRead32(lpRect + 8);
		var bottom = (int)env.MemRead32(lpRect + 12);

		Console.WriteLine($"[User32] AdjustWindowRectEx: rect=({left},{top},{right},{bottom}) style=0x{dwStyle:X8}");

		// Add window frame size (typical values)
		const int frameWidth = 8;
		const int frameHeight = 8;
		const int titleBarHeight = 32;
		const int menuHeight = 20;

		left -= frameWidth;
		top -= titleBarHeight;
		right += frameWidth;
		bottom += frameHeight;

		if (bMenu != 0)
			top -= menuHeight;

		env.MemWrite32(lpRect, (uint)left);
		env.MemWrite32(lpRect + 4, (uint)top);
		env.MemWrite32(lpRect + 8, (uint)right);
		env.MemWrite32(lpRect + 12, (uint)bottom);

		return 1; // TRUE
	}

	private uint GetDC(uint hwnd)
	{
		// Create a device context handle
		var hdc = env.RegisterHandle(new object()); // Dummy DC object
		Console.WriteLine($"[User32] GetDC: HWND=0x{hwnd:X8} -> HDC=0x{hdc:X8}");
		return hdc;
	}

	private uint ReleaseDC(uint hwnd, uint hdc)
	{
		Console.WriteLine($"[User32] ReleaseDC: HWND=0x{hwnd:X8} HDC=0x{hdc:X8}");
		env.CloseHandle(hdc);
		return 1; // Success
	}

	private uint UpdateWindow(uint hwnd)
	{
		Console.WriteLine($"[User32] UpdateWindow: HWND=0x{hwnd:X8}");
		// Trigger immediate repaint - for now just log
		return 1; // TRUE
	}

	private uint DestroyWindow(uint hwnd)
	{
		Console.WriteLine($"[User32] DestroyWindow: HWND=0x{hwnd:X8}");
		
		// Remove window from tracking
		if (env.DestroyWindow(hwnd))
		{
			return 1; // TRUE
		}
		return 0; // FALSE
	}

	private uint SetWindowPos(uint hwnd, uint hwndInsertAfter, int x, int y, int cx, int cy, uint flags)
	{
		Console.WriteLine($"[User32] SetWindowPos: HWND=0x{hwnd:X8} pos=({x},{y}) size=({cx},{cy}) flags=0x{flags:X8}");
		// For now just log
		return 1; // TRUE
	}

	private int GetSystemMetrics(int nIndex)
	{
		Console.WriteLine($"[User32] GetSystemMetrics: nIndex={nIndex}");
		
		// Return common system metrics
		return nIndex switch
		{
			0 => 1920,  // SM_CXSCREEN - Screen width
			1 => 1080,  // SM_CYSCREEN - Screen height
			4 => 640,   // SM_CXMIN - Minimum window width
			5 => 480,   // SM_CYMIN - Minimum window height
			_ => 0
		};
	}

	private uint LoadIconA(uint hInstance, uint lpIconName)
	{
		Console.WriteLine($"[User32] LoadIconA: hInstance=0x{hInstance:X8} lpIconName=0x{lpIconName:X8}");
		// Return a dummy icon handle
		return env.RegisterHandle(new object()); // Dummy icon object
	}

	private uint LoadCursorA(uint hInstance, uint lpCursorName)
	{
		Console.WriteLine($"[User32] LoadCursorA: hInstance=0x{hInstance:X8} lpCursorName=0x{lpCursorName:X8}");
		// Return a dummy cursor handle
		return env.RegisterHandle(new object()); // Dummy cursor object
	}

	private uint SetCursor(uint hCursor)
	{
		Console.WriteLine($"[User32] SetCursor: hCursor=0x{hCursor:X8}");
		// Return previous cursor handle (dummy)
		return 0x00000001;
	}

	private uint SetFocus(uint hwnd)
	{
		Console.WriteLine($"[User32] SetFocus: HWND=0x{hwnd:X8}");
		// Return previous focus window handle
		return 0; // NULL means no previous focus
	}

	private uint GetMenu(uint hwnd)
	{
		Console.WriteLine($"[User32] GetMenu: HWND=0x{hwnd:X8}");
		// Return menu handle (NULL if no menu)
		return 0;
	}

	private uint SetWindowLongA(uint hwnd, int nIndex, uint dwNewLong)
	{
		Console.WriteLine($"[User32] SetWindowLongA: HWND=0x{hwnd:X8} nIndex={nIndex} dwNewLong=0x{dwNewLong:X8}");
		// Return previous value (for now return 0)
		return 0;
	}

	private uint GetWindowLongA(uint hwnd, int nIndex)
	{
		Console.WriteLine($"[User32] GetWindowLongA: HWND=0x{hwnd:X8} nIndex={nIndex}");
		// Return window data (for now return 0)
		return 0;
	}

	private uint MessageBoxA(uint hwnd, uint lpText, uint lpCaption, uint uType)
	{
		var text = lpText != 0 ? env.ReadAnsiString(lpText) : "";
		var caption = lpCaption != 0 ? env.ReadAnsiString(lpCaption) : "";
		Console.WriteLine($"[User32] MessageBoxA: \"{caption}\" - \"{text}\" type=0x{uType:X8}");
		// Return IDOK (1)
		return 1;
	}

	private uint SystemParametersInfoA(uint uiAction, uint uiParam, uint pvParam, uint fWinIni)
	{
		Console.WriteLine($"[User32] SystemParametersInfoA: action=0x{uiAction:X8} param={uiParam}");
		// For now just return success
		return 1; // TRUE
	}

	private uint PeekMessageA(uint lpMsg, uint hwnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
	{
		// PeekMessage returns immediately with message availability
		// Return 0 for no message (non-blocking)
		Console.WriteLine($"[User32] PeekMessageA: lpMsg=0x{lpMsg:X8} HWND=0x{hwnd:X8}");
		return 0; // No message available
	}

	private uint PostMessageA(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		Console.WriteLine($"[User32] PostMessageA: HWND=0x{hwnd:X8} MSG=0x{msg:X4} wParam=0x{wParam:X8} lParam=0x{lParam:X8}");
		// Post message to queue - for now just log
		return 1; // TRUE
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

			case "GETDEVICECAPS":
				returnValue = (uint)GetDeviceCaps(a.UInt32(0), a.Int32(1));
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

	private int GetDeviceCaps(uint hdc, int nIndex)
	{
		Console.WriteLine($"[Gdi32] GetDeviceCaps(HDC=0x{hdc:X8}, nIndex={nIndex})");
		
		// Return common device capabilities
		return nIndex switch
		{
			8 => 1920,    // HORZRES - Horizontal resolution in pixels
			10 => 1080,   // VERTRES - Vertical resolution in pixels
			12 => 32,     // BITSPIXEL - Color bits per pixel
			88 => 96,     // LOGPIXELSX - Logical pixels/inch in X
			90 => 96,     // LOGPIXELSY - Logical pixels/inch in Y
			2 => 8,       // TECHNOLOGY - DT_RASDISPLAY (raster display)
			_ => 0
		};
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

	// DirectSound object handles
	private readonly Dictionary<uint, DirectSoundObject> _dsoundObjects = new();
	private readonly Dictionary<uint, DirectSoundBuffer> _buffers = new();
	private uint _nextDSoundHandle = 0x80000000;
	private uint _nextBufferHandle = 0x81000000;

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "DIRECTSOUNDCREATE":
				returnValue = DirectSoundCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "DIRECTSOUNDENUMERATEA":
				returnValue = DirectSoundEnumerateA(a.UInt32(0), a.UInt32(1));
				return true;
			default:
				Console.WriteLine($"[DSound] Unimplemented export: {export}");
				return false;
		}
	}

	private uint DirectSoundCreate(uint lpGuid, uint lplpDS, uint pUnkOuter)
	{
		Console.WriteLine($"[DSound] DirectSoundCreate(lpGuid=0x{lpGuid:X8}, lplpDS=0x{lplpDS:X8}, pUnkOuter=0x{pUnkOuter:X8})");

		// Create DirectSound object
		var dsHandle = _nextDSoundHandle++;
		var dsObj = new DirectSoundObject
		{
			Handle = dsHandle
		};
		_dsoundObjects[dsHandle] = dsObj;

		// Initialize audio backend if not already done
		if (env.AudioBackend == null)
		{
			env.AudioBackend = new Win32Emu.Rendering.SDL3AudioBackend();
			env.AudioBackend.Initialize();
		}

		if (lplpDS != 0)
		{
			env.MemWrite32(lplpDS, dsHandle);
		}

		return 0; // DS_OK
	}

	private uint DirectSoundEnumerateA(uint lpDSEnumCallback, uint lpContext)
	{
		Console.WriteLine($"[DSound] DirectSoundEnumerateA(lpDSEnumCallback=0x{lpDSEnumCallback:X8}, lpContext=0x{lpContext:X8})");
		
		// For now, just report no devices found (return DS_OK)
		// In a full implementation, we would enumerate audio devices and call the callback
		return 0; // DS_OK
	}

	private class DirectSoundObject
	{
		public uint Handle { get; set; }
		public int Frequency { get; set; } = 44100;
		public int BitsPerSample { get; set; } = 16;
		public int Channels { get; set; } = 2;
	}

	private class DirectSoundBuffer
	{
		public uint Handle { get; set; }
		public uint AudioStreamId { get; set; }
		public int Size { get; set; }
		public byte[]? Data { get; set; }
		public bool IsPrimary { get; set; }
	}
}


public class DInputModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "DINPUT.DLL";

	// DirectInput object handles
	private readonly Dictionary<uint, DirectInputObject> _dinputObjects = new();
	private readonly Dictionary<uint, DirectInputDevice> _devices = new();
	private uint _nextDInputHandle = 0x90000000;
	private uint _nextDeviceHandle = 0x91000000;

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "DIRECTINPUTCREATEA":
			case "DIRECTINPUTCREATE":
        returnValue = DirectInputCreateA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
        return true;
			case "DIRECTINPUT8CREATE":
				returnValue = DirectInput8Create(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;

			default:
				Console.WriteLine($"[DInput] Unimplemented export: {export}");
				return false;
		}
	}

	private uint DirectInputCreateA(uint hinst, uint dwVersion, uint lplpDirectInput, uint punkOuter)
	{
		Console.WriteLine($"[DInput] DirectInputCreateA(hinst=0x{hinst:X8}, dwVersion=0x{dwVersion:X8}, lplpDirectInput=0x{lplpDirectInput:X8})");

		// Create DirectInput object
		var dinputHandle = _nextDInputHandle++;
		var dinputObj = new DirectInputObject
		{
			Handle = dinputHandle
		};
    
		_dinputObjects[dinputHandle] = dinputObj;

		// Write handle back to caller
		if (lplpDirectInput != 0)
		{
			env.MemWrite32(lplpDirectInput, dinputHandle);
		}

		Console.WriteLine($"[DInput] Created DirectInput object: 0x{dinputHandle:X8}");
		return 0; // DI_OK
	}

	private uint DirectInputCreate(uint hinst, uint dwVersion, uint lplpDirectInput, uint pUnkOuter)
	{
		Console.WriteLine($"[DInput] DirectInputCreate(hinst=0x{hinst:X8}, dwVersion=0x{dwVersion:X8}, lplpDirectInput=0x{lplpDirectInput:X8})");

		// Create DirectInput object
		var diHandle = _nextDInputHandle++;
		var diObj = new DirectInputObject
		{
			Handle = diHandle,
			Version = dwVersion
		};
		_dinputObjects[diHandle] = diObj;

		// Initialize input backend if not already done
		if (env.InputBackend == null)
		{
			env.InputBackend = new Win32Emu.Rendering.SDL3InputBackend();
			env.InputBackend.Initialize();
		}

		if (lplpDirectInput != 0)
		{
			env.MemWrite32(lplpDirectInput, diHandle);
		}

		return 0; // DI_OK
	}

	private uint DirectInput8Create(uint hinst, uint dwVersion, uint riidltf, uint lplpDirectInput, uint pUnkOuter)
	{
		Console.WriteLine($"[DInput] DirectInput8Create(hinst=0x{hinst:X8}, dwVersion=0x{dwVersion:X8}, riidltf=0x{riidltf:X8})");

		// DirectInput8 is similar to DirectInputCreate but with additional parameters
		return DirectInputCreate(hinst, dwVersion, lplpDirectInput, pUnkOuter);
	}

	private class DirectInputObject
	{
		public uint Handle { get; set; }
		public uint Version { get; set; }
	}

	private class DirectInputDevice
	{
		public uint Handle { get; set; }
		public uint BackendDeviceId { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}

public class WinMMModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
	public string Name => "WINMM.DLL";

	private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();
	private uint _timerPeriod = 0;

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "TIMEGETTIME":
				returnValue = TimeGetTime();
				return true;

			case "TIMEBEGINPERIOD":
				returnValue = TimeBeginPeriod(a.UInt32(0));
				return true;

			case "TIMEENDPERIOD":
				returnValue = TimeEndPeriod(a.UInt32(0));
				return true;

			case "TIMEKILLEVENT":
				returnValue = TimeKillEvent(a.UInt32(0));
				return true;

			default:
				Console.WriteLine($"[WinMM] Unimplemented export: {export}");
				return false;
		}
	}

	private uint TimeGetTime()
	{
		// Return time in milliseconds since start
		var time = (uint)_stopwatch.ElapsedMilliseconds;
		return time;
	}

	private uint TimeBeginPeriod(uint uPeriod)
	{
		Console.WriteLine($"[WinMM] timeBeginPeriod({uPeriod})");
		_timerPeriod = uPeriod;
		return 0; // TIMERR_NOERROR
	}

	private uint TimeEndPeriod(uint uPeriod)
	{
		Console.WriteLine($"[WinMM] timeEndPeriod({uPeriod})");
		_timerPeriod = 0;
		return 0; // TIMERR_NOERROR
	}

	private uint TimeKillEvent(uint uTimerID)
	{
		Console.WriteLine($"[WinMM] timeKillEvent({uTimerID})");
		return 0; // TIMERR_NOERROR
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