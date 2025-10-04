using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class User32Module : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public User32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

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
						a.UInt32(0), // dwExStyle
						a.Lpstr(1), // lpClassName
						a.Lpstr(2), // lpWindowName
						a.UInt32(3), // dwStyle
						a.Int32(4), // x
						a.Int32(5), // y
						a.Int32(6), // nWidth
						a.Int32(7), // nHeight
						a.UInt32(8), // hWndParent
						a.UInt32(9), // hMenu
						a.UInt32(10), // hInstance
						a.UInt32(11) // lpParam
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
					returnValue = GetDc(a.UInt32(0));
					return true;

				case "RELEASEDC":
					returnValue = ReleaseDc(a.UInt32(0), a.UInt32(1));
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
					_logger.LogInformation($"[User32] Unimplemented export: {export}");
					return false;
			}
		}

		[DllModuleExport(20)]
	private unsafe uint RegisterClassA(uint lpWndClass)
		{
			if (lpWndClass == 0)
			{
				_logger.LogInformation("[User32] RegisterClassA: NULL WNDCLASS pointer");
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

			var style = _env.MemRead32(lpWndClass + 0);
			var wndProc = _env.MemRead32(lpWndClass + 4);
			var clsExtra = (int)_env.MemRead32(lpWndClass + 8);
			var wndExtra = (int)_env.MemRead32(lpWndClass + 12);
			var hInstance = _env.MemRead32(lpWndClass + 16);
			var hIcon = _env.MemRead32(lpWndClass + 20);
			var hCursor = _env.MemRead32(lpWndClass + 24);
			var hbrBackground = _env.MemRead32(lpWndClass + 28);
			var menuNamePtr = _env.MemRead32(lpWndClass + 32);
			var classNamePtr = _env.MemRead32(lpWndClass + 36);

			if (classNamePtr == 0)
			{
				_logger.LogInformation("[User32] RegisterClassA: NULL class name");
				return 0;
			}

			var className = _env.ReadAnsiString(classNamePtr);
			var menuName = menuNamePtr != 0 ? _env.ReadAnsiString(menuNamePtr) : null;

			var classInfo = new ProcessEnvironment.WindowClassInfo(
				className, style, wndProc, clsExtra, wndExtra,
				hInstance, hIcon, hCursor, hbrBackground, menuName
			);

			if (_env.RegisterWindowClass(className, classInfo))
			{
				// Return an ATOM (non-zero value) on success
				// Windows uses atoms (16-bit values) for class registration
				// We'll return a simple non-zero value to indicate success
				var atom = (uint)(className.GetHashCode() & 0xFFFF);
				if (atom == 0)
				{
					atom = 1;
				}

				_logger.LogInformation($"[User32] RegisterClassA: '{className}' -> atom 0x{atom:X4}");
				return atom;
			}

			_logger.LogInformation($"[User32] RegisterClassA: Failed to register '{className}'");
			return 0;
		}

	[DllModuleExport(3)]
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
				_logger.LogInformation("[User32] CreateWindowExA: NULL class name");
				return 0;
			}

			var className = _env.ReadAnsiString(classNamePtr);
			var windowName = windowNamePtr != 0 ? _env.ReadAnsiString(windowNamePtr) : "";

			// Check if window class is registered
			if (!_env.IsWindowClassRegistered(className))
			{
				_logger.LogInformation($"[User32] CreateWindowExA: Window class '{className}' not registered");
				return 0;
			}

			// Handle CW_USEDEFAULT for position and size
			const int cwUsedefault = unchecked((int)0x80000000);
			if (x == cwUsedefault)
			{
				x = 100;
			}

			if (y == cwUsedefault)
			{
				y = 100;
			}

			if (nWidth == cwUsedefault)
			{
				nWidth = 640;
			}

			if (nHeight == cwUsedefault)
			{
				nHeight = 480;
			}

			var hwnd = _env.CreateWindow(
				className, windowName, dwStyle, dwExStyle,
				x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam
			);

			if (hwnd != 0)
			{
				_logger.LogInformation($"[User32] CreateWindowExA: Created HWND=0x{hwnd:X8} Class='{className}' Title='{windowName}'");
			}
			else
			{
				_logger.LogInformation("[User32] CreateWindowExA: Failed to create window");
			}

			return hwnd;
		}

		[DllModuleExport(28)]
	private unsafe uint ShowWindow(uint hwnd, int nCmdShow)
		{
			// SW_HIDE = 0, SW_NORMAL = 1, SW_SHOWMINIMIZED = 2, SW_SHOWMAXIMIZED = 3, etc.
			_logger.LogInformation($"[User32] ShowWindow: HWND=0x{hwnd:X8} nCmdShow={nCmdShow}");

			// For now, just log and return TRUE (non-zero)
			// In a full implementation, this would interact with the Avalonia window
			return 1;
		}

		[DllModuleExport(10)]
	private unsafe uint GetMessageA(uint lpMsg, uint hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
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
				_logger.LogInformation("[User32] GetMessageA: NULL MSG pointer");
				return 0;
			}

			// Check if there's a quit message
			if (_env.HasQuitMessage())
			{
				var exitCode = _env.GetQuitExitCode();
				_logger.LogInformation($"[User32] GetMessageA: WM_QUIT (exitCode={exitCode})");

				// Fill MSG structure with WM_QUIT
				_env.MemWrite32(lpMsg + 0, 0); // hwnd = NULL
				_env.MemWrite32(lpMsg + 4, 0x0012); // WM_QUIT = 0x0012
				_env.MemWrite32(lpMsg + 8, (uint)exitCode); // wParam = exit code
				_env.MemWrite32(lpMsg + 12, 0); // lParam = 0
				_env.MemWrite32(lpMsg + 16, 0); // time = 0
				_env.MemWrite32(lpMsg + 20, 0); // pt.x = 0
				_env.MemWrite32(lpMsg + 24, 0); // pt.y = 0

				return 0; // GetMessage returns 0 for WM_QUIT
			}

			// For now, simulate no messages (would block in real implementation)
			// In a real implementation, this would wait for messages or return pending messages
			_logger.LogInformation("[User32] GetMessageA: No messages, simulating empty queue");

			// Return a dummy message (WM_NULL)
			_env.MemWrite32(lpMsg + 0, hWnd); // hwnd
			_env.MemWrite32(lpMsg + 4, 0); // WM_NULL = 0
			_env.MemWrite32(lpMsg + 8, 0); // wParam = 0
			_env.MemWrite32(lpMsg + 12, 0); // lParam = 0
			_env.MemWrite32(lpMsg + 16, 0); // time = 0
			_env.MemWrite32(lpMsg + 20, 0); // pt.x = 0
			_env.MemWrite32(lpMsg + 24, 0); // pt.y = 0

			return 1; // GetMessage returns non-zero for all messages except WM_QUIT
		}

		[DllModuleExport(30)]
	private unsafe uint TranslateMessage(uint lpMsg)
		{
			// TranslateMessage translates virtual-key messages into character messages
			// For now, just log and return FALSE (no translation occurred)
			_logger.LogInformation("[User32] TranslateMessage: Called");
			return 0;
		}

		[DllModuleExport(6)]
	private unsafe uint DispatchMessageA(uint lpMsg)
		{
			if (lpMsg == 0)
			{
				_logger.LogInformation("[User32] DispatchMessageA: NULL MSG pointer");
				return 0;
			}

			// Read MSG structure
			var hwnd = _env.MemRead32(lpMsg + 0);
			var message = _env.MemRead32(lpMsg + 4);
			var wParam = _env.MemRead32(lpMsg + 8);
			var lParam = _env.MemRead32(lpMsg + 12);

			_logger.LogInformation($"[User32] DispatchMessageA: HWND=0x{hwnd:X8} MSG=0x{message:X4} wParam=0x{wParam:X8} lParam=0x{lParam:X8}");

			// In a full implementation, this would call the window procedure
			// For now, just return 0 (message processed)
			return 0;
		}

		private unsafe uint DefWindowProcA(uint hwnd, uint msg, uint wParam, uint lParam)
		{
			_logger.LogInformation($"[User32] DefWindowProcA: HWND=0x{hwnd:X8} MSG=0x{msg:X4} wParam=0x{wParam:X8} lParam=0x{lParam:X8}");

			// DefWindowProc provides default processing for window messages
			// For now, just return 0 (message processed)
			return 0;
		}

	[DllModuleExport(19)]
		private unsafe void PostQuitMessage(int nExitCode)
		{
			_logger.LogInformation($"[User32] PostQuitMessage: exitCode={nExitCode}");
			_env.PostQuitMessage(nExitCode);
		}

		private unsafe uint SendMessageA(uint hwnd, uint msg, uint wParam, uint lParam)
		{
			_logger.LogInformation($"[User32] SendMessageA: HWND=0x{hwnd:X8} MSG=0x{msg:X4} wParam=0x{wParam:X8} lParam=0x{lParam:X8}");

			// SendMessage sends a message to the window procedure
			// For now, just log and return 0 (message processed)
			return 0;
		}

		private unsafe uint ClientToScreen(uint hwnd, uint lpPoint)
		{
			if (lpPoint == 0)
			{
				return 0;
			}

			// POINT structure: LONG x, LONG y (8 bytes)
			var x = (int)_env.MemRead32(lpPoint);
			var y = (int)_env.MemRead32(lpPoint + 4);

			_logger.LogInformation($"[User32] ClientToScreen: HWND=0x{hwnd:X8} Point=({x},{y})");

			// For now, treat client coordinates same as screen coordinates (no offset)
			// In a real implementation, this would add window position to client coords
			return 1; // TRUE
		}

		private unsafe uint SetRect(uint lpRect, int left, int top, int right, int bottom)
		{
			if (lpRect == 0)
			{
				return 0;
			}

			_logger.LogInformation($"[User32] SetRect: lpRect=0x{lpRect:X8} ({left},{top},{right},{bottom})");

			// RECT structure: LONG left, top, right, bottom (16 bytes)
			_env.MemWrite32(lpRect, (uint)left);
			_env.MemWrite32(lpRect + 4, (uint)top);
			_env.MemWrite32(lpRect + 8, (uint)right);
			_env.MemWrite32(lpRect + 12, (uint)bottom);

			return 1; // TRUE
		}

		private unsafe uint GetClientRect(uint hwnd, uint lpRect)
		{
			if (lpRect == 0)
			{
				return 0;
			}

			_logger.LogInformation($"[User32] GetClientRect: HWND=0x{hwnd:X8}");

			// Return a default client rect (0, 0, 640, 480)
			_env.MemWrite32(lpRect, 0); // left
			_env.MemWrite32(lpRect + 4, 0); // top
			_env.MemWrite32(lpRect + 8, 640); // right
			_env.MemWrite32(lpRect + 12, 480); // bottom

			return 1; // TRUE
		}

		private unsafe uint GetWindowRect(uint hwnd, uint lpRect)
		{
			if (lpRect == 0)
			{
				return 0;
			}

			_logger.LogInformation($"[User32] GetWindowRect: HWND=0x{hwnd:X8}");

			// Return a default window rect (100, 100, 740, 580)
			_env.MemWrite32(lpRect, 100); // left
			_env.MemWrite32(lpRect + 4, 100); // top
			_env.MemWrite32(lpRect + 8, 740); // right
			_env.MemWrite32(lpRect + 12, 580); // bottom

			return 1; // TRUE
		}

		private unsafe uint AdjustWindowRectEx(uint lpRect, uint dwStyle, int bMenu, uint dwExStyle)
		{
			if (lpRect == 0)
			{
				return 0;
			}

			var left = (int)_env.MemRead32(lpRect);
			var top = (int)_env.MemRead32(lpRect + 4);
			var right = (int)_env.MemRead32(lpRect + 8);
			var bottom = (int)_env.MemRead32(lpRect + 12);

			_logger.LogInformation($"[User32] AdjustWindowRectEx: rect=({left},{top},{right},{bottom}) style=0x{dwStyle:X8}");

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
			{
				top -= menuHeight;
			}

			_env.MemWrite32(lpRect, (uint)left);
			_env.MemWrite32(lpRect + 4, (uint)top);
			_env.MemWrite32(lpRect + 8, (uint)right);
			_env.MemWrite32(lpRect + 12, (uint)bottom);

			return 1; // TRUE
		}

		private unsafe uint GetDc(uint hwnd)
		{
			// Create a device context handle
			var hdc = _env.RegisterHandle(new object()); // Dummy DC object
			_logger.LogInformation($"[User32] GetDC: HWND=0x{hwnd:X8} -> HDC=0x{hdc:X8}");
			return hdc;
		}

		private unsafe uint ReleaseDc(uint hwnd, uint hdc)
		{
			_logger.LogInformation($"[User32] ReleaseDC: HWND=0x{hwnd:X8} HDC=0x{hdc:X8}");
			_env.CloseHandle(hdc);
			return 1; // Success
		}

		private unsafe uint UpdateWindow(uint hwnd)
		{
			_logger.LogInformation($"[User32] UpdateWindow: HWND=0x{hwnd:X8}");
			// Trigger immediate repaint - for now just log
			return 1; // TRUE
		}

		private unsafe uint DestroyWindow(uint hwnd)
		{
			_logger.LogInformation($"[User32] DestroyWindow: HWND=0x{hwnd:X8}");

			// Remove window from tracking
			if (_env.DestroyWindow(hwnd))
			{
				return 1; // TRUE
			}

			return 0; // FALSE
		}

		private unsafe uint SetWindowPos(uint hwnd, uint hwndInsertAfter, int x, int y, int cx, int cy, uint flags)
		{
			_logger.LogInformation($"[User32] SetWindowPos: HWND=0x{hwnd:X8} pos=({x},{y}) size=({cx},{cy}) flags=0x{flags:X8}");
			// For now just log
			return 1; // TRUE
		}

		[DllModuleExport(11)]
		private unsafe int GetSystemMetrics(int nIndex)
		{
			_logger.LogInformation($"[User32] GetSystemMetrics: nIndex={nIndex}");

			// Return common system metrics
			return nIndex switch
			{
				0 => 1920, // SM_CXSCREEN - Screen width
				1 => 1080, // SM_CYSCREEN - Screen height
				4 => 640, // SM_CXMIN - Minimum window width
				5 => 480, // SM_CYMIN - Minimum window height
				_ => 0
			};
		}

		private unsafe uint LoadIconA(uint hInstance, uint lpIconName)
		{
			_logger.LogInformation($"[User32] LoadIconA: hInstance=0x{hInstance:X8} lpIconName=0x{lpIconName:X8}");
			// Return a dummy icon handle
			return _env.RegisterHandle(new object()); // Dummy icon object
		}

		private unsafe uint LoadCursorA(uint hInstance, uint lpCursorName)
		{
			_logger.LogInformation($"[User32] LoadCursorA: hInstance=0x{hInstance:X8} lpCursorName=0x{lpCursorName:X8}");
			// Return a dummy cursor handle
			return _env.RegisterHandle(new object()); // Dummy cursor object
		}

		private unsafe uint SetCursor(uint hCursor)
		{
			_logger.LogInformation($"[User32] SetCursor: hCursor=0x{hCursor:X8}");
			// Return previous cursor handle (dummy)
			return 0x00000001;
		}

		private unsafe uint SetFocus(uint hwnd)
		{
			_logger.LogInformation($"[User32] SetFocus: HWND=0x{hwnd:X8}");
			// Return previous focus window handle
			return 0; // NULL means no previous focus
		}

		private unsafe uint GetMenu(uint hwnd)
		{
			_logger.LogInformation($"[User32] GetMenu: HWND=0x{hwnd:X8}");
			// Return menu handle (NULL if no menu)
			return 0;
		}

		private unsafe uint SetWindowLongA(uint hwnd, int nIndex, uint dwNewLong)
		{
			_logger.LogInformation($"[User32] SetWindowLongA: HWND=0x{hwnd:X8} nIndex={nIndex} dwNewLong=0x{dwNewLong:X8}");
			// Return previous value (for now return 0)
			return 0;
		}

		private unsafe uint GetWindowLongA(uint hwnd, int nIndex)
		{
			_logger.LogInformation($"[User32] GetWindowLongA: HWND=0x{hwnd:X8} nIndex={nIndex}");
			// Return window data (for now return 0)
			return 0;
		}

		private unsafe uint MessageBoxA(uint hwnd, uint lpText, uint lpCaption, uint uType)
		{
			var text = lpText != 0 ? _env.ReadAnsiString(lpText) : "";
			var caption = lpCaption != 0 ? _env.ReadAnsiString(lpCaption) : "";
			_logger.LogInformation($"[User32] MessageBoxA: \"{caption}\" - \"{text}\" type=0x{uType:X8}");
			// Return IDOK (1)
			return 1;
		}

		private unsafe uint SystemParametersInfoA(uint uiAction, uint uiParam, uint pvParam, uint fWinIni)
		{
			_logger.LogInformation($"[User32] SystemParametersInfoA: action=0x{uiAction:X8} param={uiParam}");
			// For now just return success
			return 1; // TRUE
		}

		private unsafe uint PeekMessageA(uint lpMsg, uint hwnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
		{
			// PeekMessage returns immediately with message availability
			// Return 0 for no message (non-blocking)
			_logger.LogInformation($"[User32] PeekMessageA: lpMsg=0x{lpMsg:X8} HWND=0x{hwnd:X8}");
			return 0; // No message available
		}

		private unsafe uint PostMessageA(uint hwnd, uint msg, uint wParam, uint lParam)
		{
			_logger.LogInformation($"[User32] PostMessageA: HWND=0x{hwnd:X8} MSG=0x{msg:X4} wParam=0x{wParam:X8} lParam=0x{lParam:X8}");
			// Post message to queue - for now just log
			return 1; // TRUE
		}

		public Dictionary<string, uint> GetExportOrdinals()
		{
			// Export ordinals for User32 - alphabetically ordered
			var exports = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
			{
				{ "ADJUSTWINDOWRECTEX", 1 },
				{ "CLIENTTOSCREEN", 2 },
				{ "CREATEWINDOWEXA", 3 },
				{ "DEFWINDOWPROCA", 4 },
				{ "DESTROYWINDOW", 5 },
				{ "DISPATCHMESSAGEA", 6 },
				{ "GETCLIENTRECT", 7 },
				{ "GETDC", 8 },
				{ "GETMENU", 9 },
				{ "GETMESSAGEA", 10 },
				{ "GETSYSTEMMETRICS", 11 },
				{ "GETWINDOWLONGA", 12 },
				{ "GETWINDOWRECT", 13 },
				{ "LOADCURSORA", 14 },
				{ "LOADICONA", 15 },
				{ "MESSAGEBOXA", 16 },
				{ "PEEKMESSAGEA", 17 },
				{ "POSTMESSAGEA", 18 },
				{ "POSTQUITMESSAGE", 19 },
				{ "REGISTERCLASSA", 20 },
				{ "RELEASEDC", 21 },
				{ "SENDMESSAGEA", 22 },
				{ "SETCURSOR", 23 },
				{ "SETFOCUS", 24 },
				{ "SETRECT", 25 },
				{ "SETWINDOWLONGA", 26 },
				{ "SETWINDOWPOS", 27 },
				{ "SHOWWINDOW", 28 },
				{ "SYSTEMPARAMETERSINFOA", 29 },
				{ "TRANSLATEMESSAGE", 30 },
				{ "UPDATEWINDOW", 31 }
			};
			return exports;
		}
	}
}