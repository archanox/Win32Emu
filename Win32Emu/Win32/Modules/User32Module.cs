using System.Diagnostics;
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
		private readonly Dictionary<uint, bool> _windowEnabledState = new();
		private readonly StandardControlHandler _standardControlHandler;
		private ICpu _cpu;
		private VirtualMemory _memory;
		private Win32Dispatcher? _dispatcher;
		private LoadedImage? _image;
		
		// Constants for procedure execution monitoring
		private const int INFINITE_LOOP_CHECK_INTERVAL = 100000; // Check for infinite loops every 100K steps
		private const int STUCK_COUNTER_THRESHOLD = 3; // Number of consecutive checks at same EIP to consider it stuck
		private const int CANCELLATION_CHECK_INTERVAL = 1000; // Check cancellation token every 1K steps

		public User32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
			_standardControlHandler = new StandardControlHandler(env, null, _logger);
		}

		public void SetDispatcher(Win32Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void SetLoadedImage(LoadedImage image)
		{
			_image = image;
		}

		public string Name => "USER32.DLL";

		public unsafe bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			_cpu = cpu;
			_memory = memory;
			
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			Debug.Assert(export != null, nameof(export) + " != null");
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

				case "SHOWCURSOR":
					returnValue = (uint)ShowCursor(a.Int32(0));
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

				case "DIALOGBOXPARAMA":
					returnValue = DialogBoxParamA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				case "ENDDIALOG":
					returnValue = EndDialog(a.UInt32(0), a.UInt32(1));
					return true;

				case "GETDLGITEM":
					returnValue = GetDlgItem(a.UInt32(0), a.Int32(1));
					return true;

				case "GETDLGITEMTEXTA":
					returnValue = GetDlgItemTextA(a.UInt32(0), a.Int32(1), a.UInt32(2), a.Int32(3));
					return true;

				case "SETDLGITEMTEXTA":
					returnValue = SetDlgItemTextA(a.UInt32(0), a.Int32(1), a.Lpstr(2));
					return true;

				case "SENDDLGITEMMESSAGEA":
					returnValue = SendDlgItemMessageA(a.UInt32(0), a.Int32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				case "ENABLEWINDOW":
					returnValue = EnableWindow(a.UInt32(0), a.UInt32(1));
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

				case "CHARNEXTA":
					returnValue = CharNextA(a.UInt32(0));
					return true;

				default:
					_logger.LogInformation("[User32] Unimplemented export: {Export}", export);
					return false;
			}
		}

		/// <summary>
		/// Registers a window class for subsequent use in calls to the CreateWindow or CreateWindowEx function.
		/// </summary>
		/// <param name="lpWndClass">
		/// A pointer to a WNDCLASS structure. You must fill the structure with the appropriate class attributes before passing it to the function.
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is a class atom that uniquely identifies the class being registered.
		/// This atom can only be used by CreateWindow, CreateWindowEx, GetClassInfo, GetClassInfoEx, FindWindow, FindWindowEx, and UnregisterClass functions.
		/// If the function fails, the return value is zero. To get extended error information, call GetLastError.
		/// </returns>
		/// <remarks>
		/// If you register the window class by using RegisterClassA, the application tells the system that the windows of the created class expect messages with text or character parameters to use the ANSI character set.
		/// All window classes that an application registers are unregistered when it terminates.
		/// No window classes registered by a DLL are unregistered when the DLL is unloaded. A DLL must explicitly unregister its classes when it is unloaded.
		/// </remarks>
		[DllModuleExport(20)]
		private uint RegisterClassA(uint lpWndClass)
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

				// Register the atom-to-classname mapping
				_env.RegisterAtom(atom, className);

				_logger.LogInformation("[User32] RegisterClassA: '{ClassName}' -> atom 0x{Atom:X4}", className, atom);
				return atom;
			}

			_logger.LogInformation("[User32] RegisterClassA: Failed to register '{ClassName}'", className);
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

			string className;

			// Check if lpClassName is an atom (HIWORD is 0) or a string pointer
			if (classNamePtr != 0 && (classNamePtr & 0xFFFF0000) == 0)
			{
				// It's an atom - look up the class name
				var atomClassName = _env.GetClassNameFromAtom(classNamePtr);
				if (atomClassName == null)
				{
					_logger.LogInformation("[User32] CreateWindowExA: Unknown atom 0x{ClassNamePtr:X4}", classNamePtr);
					return 0;
				}

				className = atomClassName;
			}
			else if (classNamePtr == 0)
			{
				_logger.LogInformation("[User32] CreateWindowExA: NULL class name");
				return 0;
			}
			else
			{
				// It's a string pointer
				className = _env.ReadAnsiString(classNamePtr);
			}

			var windowName = windowNamePtr != 0 ? _env.ReadAnsiString(windowNamePtr) : "";

			// Check if window class is registered
			if (!_env.IsWindowClassRegistered(className))
			{
				_logger.LogInformation("[User32] CreateWindowExA: Window class '{ClassName}' not registered", className);
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
				_logger.LogInformation("[User32] CreateWindowExA: Created HWND=0x{Hwnd:X8} Class='{ClassName}' Title='{WindowName}'", hwnd, className, windowName);
			}
			else
			{
				_logger.LogInformation("[User32] CreateWindowExA: Failed to create window");
			}

			return hwnd;
		}

		/// <summary>
		/// Sets the specified window's show state.
		/// </summary>
		/// <param name="hwnd">
		/// A handle to the window.
		/// </param>
		/// <param name="nCmdShow">
		/// Controls how the window is to be shown. This parameter can be one of the SW_ constants such as:
		/// SW_HIDE, SW_SHOWNORMAL, SW_SHOWMINIMIZED, SW_SHOWMAXIMIZED, SW_SHOWNOACTIVATE, SW_SHOW,
		/// SW_MINIMIZE, SW_SHOWMINNOACTIVE, SW_SHOWNA, SW_RESTORE, SW_SHOWDEFAULT, or SW_FORCEMINIMIZE.
		/// </param>
		/// <returns>
		/// If the window was previously visible, the return value is nonzero.
		/// If the window was previously hidden, the return value is zero.
		/// </returns>
		/// <remarks>
		/// To perform certain special effects when showing or hiding a window, use AnimateWindow.
		/// The first time an application calls ShowWindow, it should use the WinMain function's nCmdShow parameter as its nCmdShow parameter.
		/// Subsequent calls to ShowWindow must use one of the values in the given list, instead of the one specified by the WinMain function's nCmdShow parameter.
		/// </remarks>
		[DllModuleExport(28)]
		private uint ShowWindow(uint hwnd, int nCmdShow)
		{
			// SW_HIDE = 0, SW_NORMAL = 1, SW_SHOWMINIMIZED = 2, SW_SHOWMAXIMIZED = 3, etc.
			_logger.LogInformation("[User32] ShowWindow: HWND=0x{Hwnd:X8} nCmdShow={NCmdShow}", hwnd, nCmdShow);

			// For now, just log and return TRUE (non-zero)
			// In a full implementation, this would interact with the Avalonia window
			return 1;
		}

		[DllModuleExport(10)]
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
				_logger.LogInformation("[User32] GetMessageA: NULL MSG pointer");
				return 0xFFFFFFFF; // -1 for error
			}

			while (true)
			{
				// Check if there's a quit message
				if (_env.HasQuitMessage())
				{
					var exitCode = _env.GetQuitExitCode();
					_logger.LogInformation("[User32] GetMessageA: WM_QUIT (exitCode={ExitCode})", exitCode);

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

				// Try to get a message from the queue (with short timeout to simulate blocking)
				// Real Windows GetMessage blocks indefinitely, but we use a timeout to avoid hanging the emulator
				var queuedMsg = _env.GetMessageBlocking(hWnd, wMsgFilterMin, wMsgFilterMax, timeoutMs: -1);
				if (queuedMsg.HasValue)
				{
					if (queuedMsg.Value.Message == 0x0012)
					{
						// WM_QUIT
						_env.PostQuitMessage((int)queuedMsg.Value.WParam);
						continue; // Loop again to handle quit message
					}
					
					_logger.LogInformation("[User32] GetMessageA: retrieved MSG=0x{ValueMessage:X4} HWND=0x{ValueHwnd:X8}", queuedMsg.Value.Message, queuedMsg.Value.Hwnd);

					// Fill MSG structure
					_env.MemWrite32(lpMsg + 0, queuedMsg.Value.Hwnd);
					_env.MemWrite32(lpMsg + 4, queuedMsg.Value.Message);
					_env.MemWrite32(lpMsg + 8, queuedMsg.Value.WParam);
					_env.MemWrite32(lpMsg + 12, queuedMsg.Value.LParam);
					_env.MemWrite32(lpMsg + 16, queuedMsg.Value.Time);
					_env.MemWrite32(lpMsg + 20, queuedMsg.Value.PtX);
					_env.MemWrite32(lpMsg + 24, queuedMsg.Value.PtY);

					return 1; // GetMessage returns non-zero for all messages except WM_QUIT
				}
			}
		}

		[DllModuleExport(30)]
		private uint TranslateMessage(uint lpMsg)
		{
			// TranslateMessage translates virtual-key messages into character messages
			
			if (lpMsg != 0)
			{
				var hwnd = _env.MemRead32(lpMsg + 0);
				var message = _env.MemRead32(lpMsg + 4);
				var wParam = _env.MemRead32(lpMsg + 8);
				var lParam = _env.MemRead32(lpMsg + 12);
				_logger.LogInformation(
					"[User32] TranslateMessage: HWND=0x{Hwnd:X8} MSG=0x{Message:X4} wParam=0x{WParam:X8} lParam=0x{LParam:X8}",
					hwnd, message, wParam, lParam);
			}
			else
			{
				_logger.LogInformation("[User32] TranslateMessage: Called with null lpMsg");
			}
			
			return NativeTypes.Win32Bool.FALSE;
		}

		[DllModuleExport(6)]
		private uint DispatchMessageA(uint lpMsg)
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

			_logger.LogInformation("[User32] DispatchMessageA: HWND=0x{Hwnd:X8} MSG=0x{Message:X4} wParam=0x{WParam:X8} lParam=0x{LParam:X8}", hwnd, message, wParam, lParam);

			// Check if this is a standard control first
			var windowInfo = _env.GetWindow(hwnd);
			if (windowInfo.HasValue && StandardControlHandler.IsStandardControl(windowInfo.Value.ClassName))
			{
				_logger.LogInformation("[User32] DispatchMessageA: Routing to standard control handler for class '{ClassName}'", windowInfo.Value.ClassName);
				return _standardControlHandler.HandleMessage(hwnd, message, wParam, lParam, windowInfo.Value.ClassName);
			}

			// Try to get the window procedure for this window
			var wndProc = _env.GetWindowProc(hwnd);
			if (wndProc.HasValue && wndProc.Value != 0)
			{
				_logger.LogInformation("[User32] DispatchMessageA: Found WndProc=0x{WndProc:X8} for HWND=0x{Hwnd:X8}", wndProc.Value, hwnd);
				
				var result = CallWindowProcedure(_cpu, _memory, wndProc.Value, hwnd, message, wParam, lParam);
				_logger.LogInformation("[User32] DispatchMessageA: WndProc returned 0x{Result:X8}", result);
				return result;
			}

			_logger.LogInformation("[User32] DispatchMessageA: No WndProc found for HWND=0x{Hwnd:X8}", hwnd);

			// For now, just return 0 (message processed)
			return 0;
		}

		[DllModuleExport(1)]
		private uint DefWindowProcA(uint hwnd, uint msg, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] DefWindowProcA: HWND=0x{Hwnd:X8} MSG=0x{Msg:X4} wParam=0x{WParam:X8} lParam=0x{LParam:X8}", hwnd, msg, wParam, lParam);

			// DefWindowProc provides default processing for window messages
			// Implement some common default behaviors
			switch (msg)
			{
				case 0x0001: // WM_CREATE
					_logger.LogInformation($"[User32] DefWindowProcA: WM_CREATE");
					return 0; // Continue creation

				case 0x0002: // WM_DESTROY
					_logger.LogInformation($"[User32] DefWindowProcA: WM_DESTROY");
					return 0;

				case 0x0010: // WM_CLOSE
					_logger.LogInformation($"[User32] DefWindowProcA: WM_CLOSE - destroying window");
					// Default action is to destroy the window
					_env.DestroyWindow(hwnd);
					return 0;

				case 0x000F: // WM_PAINT
					_logger.LogInformation($"[User32] DefWindowProcA: WM_PAINT");
					return 0;

				case 0x0014: // WM_ERASEBKGND
					_logger.LogInformation($"[User32] DefWindowProcA: WM_ERASEBKGND");
					return 1; // Background erased

				default:
					// For all other messages, just return 0
					return 0;
			}
		}

		[DllModuleExport(19)]
		private void PostQuitMessage(int nExitCode)
		{
			_logger.LogInformation("[User32] PostQuitMessage: exitCode={NExitCode}", nExitCode);
			_env.PostQuitMessage(nExitCode);
		}

		/// <summary>
		/// Call a window procedure by setting up CPU state and executing the callback.
		/// WndProc signature: LRESULT CALLBACK WndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
		/// Uses stdcall calling convention (callee cleans stack, parameters pushed right-to-left)
		/// </summary>
		private uint CallWindowProcedure(ICpu cpu, VirtualMemory memory, uint wndProcAddress, uint hwnd, uint message, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] CallWindowProcedure: Calling 0x{WndProcAddress:X8} with HWND=0x{Hwnd:X8} MSG=0x{Message:X4}", wndProcAddress, hwnd, message);

			// Save current CPU state
			var savedEip = cpu.GetEip();
			var savedEsp = cpu.GetRegister("ESP");
			var savedEbp = cpu.GetRegister("EBP");

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			var esp = savedEsp;

			// Push return address (we'll use a special marker address)
			const uint RETURN_ADDRESS = 0xDEADBEEF;
			esp -= 4;
			memory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			esp -= 4;
			memory.Write32(esp, lParam);

			esp -= 4;
			memory.Write32(esp, wParam);

			esp -= 4;
			memory.Write32(esp, message);

			esp -= 4;
			memory.Write32(esp, hwnd);

			// Update CPU registers
			cpu.SetRegister("ESP", esp);
			cpu.SetEip(wndProcAddress);

			// Execute until we hit the return address
			// Use unlimited steps for window procedures to support complex UI operations.
			// The procedure will naturally terminate when it returns (hits RETURN_ADDRESS).
			// To prevent true infinite loops, we track progress and detect stuck execution.
			const int MAX_STEPS = int.MaxValue; // No artificial limit
			// YIELD_INTERVAL: Check for context switches every 10K instructions
			// Rationale: 10K provides good balance between:
			// - Responsiveness: Allows context switches ~50 times during max execution
			// - Performance: Low overhead (~0.001% for scheduler checks)
			// - Granularity: Fine enough for cooperative multitasking
			const int YIELD_INTERVAL = 10000;
			var steps = 0;
			var lastCheckEip = cpu.GetEip();
			var stuckCounter = 0;

			try
			{
				while (steps < MAX_STEPS)
				{
					var eip = cpu.GetEip();

					// Check if we've returned to our marker address
					if (eip == RETURN_ADDRESS)
					{
						break;
					}

					// Detect potential infinite loops by checking if we're making progress
					if (steps > 0 && steps % INFINITE_LOOP_CHECK_INTERVAL == 0)
					{
						var currentEip = cpu.GetEip();
						if (currentEip == lastCheckEip)
						{
							stuckCounter++;
							if (stuckCounter >= STUCK_COUNTER_THRESHOLD)
							{
								// We've been at the same instruction for multiple check intervals - likely an infinite loop
								_logger.LogWarning("[User32] CallWindowProcedure: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting", currentEip, stuckCounter);
								break;
							}
						}
						else
						{
							stuckCounter = 0;
							lastCheckEip = currentEip;
						}
					}

					// Execute one instruction
					cpu.SingleStep(memory);

					steps++;
					
					// Periodically check if we should yield to other threads
					if (steps % YIELD_INTERVAL == 0)
					{
						var scheduler = _env.ThreadScheduler;
						if (scheduler != null)
						{
							// Process any waiting thread timeouts
							scheduler.ProcessWaitTimeouts();
							
							// Check if there are other threads that need CPU time
							if (scheduler.ShouldContextSwitch())
							{
								_logger.LogDebug("[User32] CallWindowProcedure: Cooperative yield at {Steps} steps", steps);
								// Note: We can't actually context switch here since we're mid-call
								// But we log it for diagnostics. In a future enhancement, we could
								// save state and resume the call later.
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning("[User32] CallWindowProcedure: Exception during execution: {ExMessage}", ex.Message);
			}

			if (steps >= MAX_STEPS)
			{
				_logger.LogWarning("[User32] CallWindowProcedure: Exceeded max steps ({MaxSteps}), aborting - WndProc may be in infinite loop", MAX_STEPS);
			}

			// Get return value from EAX
			var returnValue = cpu.GetRegister("EAX");

			// Restore CPU state
			cpu.SetEip(savedEip);
			cpu.SetRegister("ESP", savedEsp);
			cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] CallWindowProcedure: Completed with return value 0x{ReturnValue:X8}", returnValue);

			return returnValue;
		}
		
		/// <summary>
		/// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window and does not return until the window procedure has processed the message.
		/// </summary>
		/// <param name="hwnd">
		/// A handle to the window whose window procedure will receive the message.
		/// If this parameter is HWND_BROADCAST (0xFFFF), the message is sent to all top-level windows in the system.
		/// </param>
		/// <param name="msg">
		/// The message to be sent. For lists of the system-provided messages, see System-Defined Messages.
		/// </param>
		/// <param name="wParam">
		/// Additional message-specific information.
		/// </param>
		/// <param name="lParam">
		/// Additional message-specific information.
		/// </param>
		/// <returns>
		/// The return value specifies the result of the message processing; it depends on the message sent.
		/// </returns>
		/// <remarks>
		/// The system only does marshalling for system messages (those in the range 0 to WM_USER-1). To send other messages to another process, you must do custom marshalling.
		/// If the specified window was created by the calling thread, the window procedure is called immediately as a subroutine.
		/// If the specified window was created by a different thread, the system switches to that thread and calls the appropriate window procedure.
		/// Messages sent between threads are processed only when the receiving thread executes message retrieval code.
		/// </remarks>
		[DllModuleExport(1)]
		private uint SendMessageA(uint hwnd, uint msg, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] SendMessageA: HWND=0x{Hwnd:X8} MSG=0x{Msg:X4} wParam=0x{WParam:X8} lParam=0x{LParam:X8}", hwnd, msg, wParam, lParam);

			// Check if this is a standard control first
			var windowInfo = _env.GetWindow(hwnd);
			if (windowInfo.HasValue && StandardControlHandler.IsStandardControl(windowInfo.Value.ClassName))
			{
				_logger.LogInformation("[User32] SendMessageA: Routing to standard control handler for class '{ClassName}'", windowInfo.Value.ClassName);
				return _standardControlHandler.HandleMessage(hwnd, msg, wParam, lParam, windowInfo.Value.ClassName);
			}

			// SendMessage sends a message directly to the window procedure (synchronous)
			// Try to get the window procedure for this window
			var wndProc = _env.GetWindowProc(hwnd);
			if (wndProc.HasValue && wndProc.Value != 0)
			{
				_logger.LogInformation("[User32] SendMessageA: Found WndProc=0x{WndProc:X8} for HWND=0x{Hwnd:X8}", wndProc.Value, hwnd);
				var result = CallWindowProcedure(_cpu, _memory, wndProc.Value, hwnd, msg, wParam, lParam);
				_logger.LogInformation("[User32] SendMessageA: WndProc returned 0x{Result:X8}", result);
				return result;
			}

			_logger.LogInformation("[User32] SendMessageA: No WndProc found for HWND=0x{Hwnd:X8}", hwnd);

			// For now, return 0 (message processed)
			return 0;
		}

		[DllModuleExport(1)]
		private uint ClientToScreen(uint hwnd, uint lpPoint)
		{
			if (lpPoint == 0)
			{
				return 0;
			}

			// POINT structure: LONG x, LONG y (8 bytes)
			var x = (int)_env.MemRead32(lpPoint);
			var y = (int)_env.MemRead32(lpPoint + 4);

			_logger.LogInformation("[User32] ClientToScreen: HWND=0x{Hwnd:X8} Point=({I},{I1})", hwnd, x, y);

			// For now, treat client coordinates same as screen coordinates (no offset)
			// In a real implementation, this would add window position to client coords
			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint SetRect(uint lpRect, int left, int top, int right, int bottom)
		{
			if (lpRect == 0)
			{
				return 0;
			}

			_logger.LogInformation("[User32] SetRect: lpRect=0x{LpRect:X8} ({Left},{Top},{Right},{Bottom})", lpRect, left, top, right, bottom);

			// RECT structure: LONG left, top, right, bottom (16 bytes)
			_env.MemWrite32(lpRect, (uint)left);
			_env.MemWrite32(lpRect + 4, (uint)top);
			_env.MemWrite32(lpRect + 8, (uint)right);
			_env.MemWrite32(lpRect + 12, (uint)bottom);

			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint GetClientRect(uint hwnd, uint lpRect)
		{
			if (lpRect == 0)
			{
				return 0;
			}

			_logger.LogInformation("[User32] GetClientRect: HWND=0x{Hwnd:X8}", hwnd);

			// Return a default client rect (0, 0, 640, 480)
			_env.MemWrite32(lpRect, 0); // left
			_env.MemWrite32(lpRect + 4, 0); // top
			_env.MemWrite32(lpRect + 8, 640); // right
			_env.MemWrite32(lpRect + 12, 480); // bottom

			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint GetWindowRect(uint hwnd, uint lpRect)
		{
			if (lpRect == 0)
			{
				return 0;
			}

			_logger.LogInformation("[User32] GetWindowRect: HWND=0x{Hwnd:X8}", hwnd);

			// Return a default window rect (100, 100, 740, 580)
			_env.MemWrite32(lpRect, 100); // left
			_env.MemWrite32(lpRect + 4, 100); // top
			_env.MemWrite32(lpRect + 8, 740); // right
			_env.MemWrite32(lpRect + 12, 580); // bottom

			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint AdjustWindowRectEx(uint lpRect, uint dwStyle, int bMenu, uint dwExStyle)
		{
			if (lpRect == 0)
			{
				return 0;
			}

			var left = (int)_env.MemRead32(lpRect);
			var top = (int)_env.MemRead32(lpRect + 4);
			var right = (int)_env.MemRead32(lpRect + 8);
			var bottom = (int)_env.MemRead32(lpRect + 12);

			_logger.LogInformation("[User32] AdjustWindowRectEx: rect=({Left},{Top},{Right},{Bottom}) style=0x{DwStyle:X8}", left, top, right, bottom, dwStyle);

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

		[DllModuleExport(1)]
		private uint GetDc(uint hwnd)
		{
			// Create a device context handle
			var hdc = _env.RegisterHandle(new object()); // Dummy DC object
			_logger.LogInformation("[User32] GetDC: HWND=0x{Hwnd:X8} -> HDC=0x{Hdc:X8}", hwnd, hdc);
			return hdc;
		}

		[DllModuleExport(1)]
		private uint ReleaseDc(uint hwnd, uint hdc)
		{
			_logger.LogInformation("[User32] ReleaseDC: HWND=0x{Hwnd:X8} HDC=0x{Hdc:X8}", hwnd, hdc);
			_env.CloseHandle(hdc);
			return 1; // Success
		}

		[DllModuleExport(1)]
		private uint UpdateWindow(uint hwnd)
		{
			_logger.LogInformation("[User32] UpdateWindow: HWND=0x{Hwnd:X8}", hwnd);
			
			// UpdateWindow sends WM_PAINT directly if the window has an update region
			// This is synchronous - it calls the window procedure directly
			const uint WM_PAINT = 0x000F;
			
			// Check if window exists
			var windowInfo = _env.GetWindow(hwnd);
			if (!windowInfo.HasValue)
			{
				_logger.LogWarning("[User32] UpdateWindow: Window 0x{Hwnd:X8} not found", hwnd);
				return 0; // FALSE
			}
			
			// Send WM_PAINT message to the window
			// In a real implementation, this would only send if the window has an update region
			_logger.LogInformation("[User32] UpdateWindow: Sending WM_PAINT to HWND=0x{Hwnd:X8}", hwnd);
			SendMessageA(hwnd, WM_PAINT, 0, 0);
			
			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint DestroyWindow(uint hwnd)
		{
			_logger.LogInformation("[User32] DestroyWindow: HWND=0x{Hwnd:X8}", hwnd);

			// Remove window from tracking
			if (_env.DestroyWindow(hwnd))
			{
				return 1; // TRUE
			}

			return 0; // FALSE
		}

		[DllModuleExport(1)]
		private uint SetWindowPos(uint hwnd, uint hwndInsertAfter, int x, int y, int cx, int cy, uint flags)
		{
			_logger.LogInformation("[User32] SetWindowPos: HWND=0x{Hwnd:X8} pos=({I},{I1}) size=({Cx},{Cy}) flags=0x{Flags:X8}", hwnd, x, y, cx, cy, flags);
			// For now just log
			return 1; // TRUE
		}

		[DllModuleExport(11)]
		private int GetSystemMetrics(int nIndex)
		{
			_logger.LogInformation("[User32] GetSystemMetrics: nIndex={NIndex}", nIndex);

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

		[DllModuleExport(1)]
		private uint LoadIconA(uint hInstance, uint lpIconName)
		{
			_logger.LogInformation("[User32] LoadIconA: hInstance=0x{HInstance:X8} lpIconName=0x{LpIconName:X8}", hInstance, lpIconName);
			// Return a dummy icon handle
			return _env.RegisterHandle(new object()); // Dummy icon object
		}

		[DllModuleExport(1)]
		private uint LoadCursorA(uint hInstance, uint lpCursorName)
		{
			_logger.LogInformation("[User32] LoadCursorA: hInstance=0x{HInstance:X8} lpCursorName=0x{LpCursorName:X8}", hInstance, lpCursorName);
			// Return a dummy cursor handle
			return _env.RegisterHandle(new object()); // Dummy cursor object
		}

		[DllModuleExport(1)]
		private uint SetCursor(uint hCursor)
		{
			_logger.LogInformation("[User32] SetCursor: hCursor=0x{HCursor:X8}", hCursor);
			// Return previous cursor handle (dummy)
			return 0x00000001;
		}

		[DllModuleExport(1)]
		private int ShowCursor(int bShow)
		{
			_logger.LogInformation("[User32] ShowCursor: bShow={BShow}", bShow);
			// ShowCursor increments/decrements an internal display count
			// Returns the new display count after the operation
			// For now, return a simple value indicating cursor is visible
			return bShow != 0 ? 1 : 0;
		}

		[DllModuleExport(1)]
		private uint SetFocus(uint hwnd)
		{
			_logger.LogInformation("[User32] SetFocus: HWND=0x{Hwnd:X8}", hwnd);
			// Return previous focus window handle
			return 0; // NULL means no previous focus
		}

		[DllModuleExport(1)]
		private uint GetMenu(uint hwnd)
		{
			_logger.LogInformation("[User32] GetMenu: HWND=0x{Hwnd:X8}", hwnd);
			// Return menu handle (NULL if no menu)
			return 0;
		}

		[DllModuleExport(1)]
		private uint SetWindowLongA(uint hwnd, int nIndex, uint dwNewLong)
		{
			_logger.LogInformation("[User32] SetWindowLongA: HWND=0x{Hwnd:X8} nIndex={NIndex} dwNewLong=0x{DwNewLong:X8}", hwnd, nIndex, dwNewLong);
			
			// Get the previous value before setting
			var previousValue = _env.GetWindowProperty(hwnd, nIndex);
			
			// Set the new value
			_env.SetWindowProperty(hwnd, nIndex, dwNewLong);
			
			// Return the previous value
			return previousValue;
		}

		[DllModuleExport(1)]
		private uint GetWindowLongA(uint hwnd, int nIndex)
		{
			_logger.LogInformation("[User32] GetWindowLongA: HWND=0x{Hwnd:X8} nIndex={NIndex}", hwnd, nIndex);
			
			// Get the window property value
			var value = _env.GetWindowProperty(hwnd, nIndex);
			
			return value;
		}

		[DllModuleExport(16)]
		private uint MessageBoxA(uint hwnd, uint lpText, uint lpCaption, uint uType)
		{
			var text = lpText != 0 ? _env.ReadAnsiString(lpText) : "";
			var caption = lpCaption != 0 ? _env.ReadAnsiString(lpCaption) : "";
			_logger.LogInformation("[User32] MessageBoxA: \"{Caption}\" - \"{Text}\" type=0x{UType:X8}", caption, text, uType);
			// Return IDOK (1)
			return 1;
		}

		[DllModuleExport(1)]
		private uint SystemParametersInfoA(uint uiAction, uint uiParam, uint pvParam, uint fWinIni)
		{
			_logger.LogInformation("[User32] SystemParametersInfoA: action=0x{UiAction:X8} param={UiParam}", uiAction, uiParam);
			// For now just return success
			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint PeekMessageA(uint lpMsg, uint hwnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
		{
			// PeekMessage returns immediately with message availability
			_logger.LogInformation("[User32] PeekMessageA: lpMsg=0x{LpMsg:X8} HWND=0x{Hwnd:X8}", lpMsg, hwnd);

			if (lpMsg == 0)
			{
				return 0; // No message available
			}

			// PM_REMOVE = 0x0001, PM_NOREMOVE = 0x0000
			var remove = (wRemoveMsg & 0x0001) != 0;

			// Try to peek at a message from the queue
			if (_env.TryPeekMessage(out var queuedMsg, hwnd, wMsgFilterMin, wMsgFilterMax, remove))
			{
				// Fill MSG structure
				_env.MemWrite32(lpMsg + 0, queuedMsg.Hwnd);
				_env.MemWrite32(lpMsg + 4, queuedMsg.Message);
				_env.MemWrite32(lpMsg + 8, queuedMsg.WParam);
				_env.MemWrite32(lpMsg + 12, queuedMsg.LParam);
				_env.MemWrite32(lpMsg + 16, queuedMsg.Time);
				_env.MemWrite32(lpMsg + 20, queuedMsg.PtX);
				_env.MemWrite32(lpMsg + 24, queuedMsg.PtY);

				_logger.LogInformation("[User32] PeekMessageA: found MSG=0x{QueuedMsgMessage:X4}", queuedMsg.Message);
				return 1; // Message available
			}

			return 0; // No message available
		}

		[DllModuleExport(1)]
		private uint PostMessageA(uint hwnd, uint msg, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] PostMessageA: HWND=0x{Hwnd:X8} MSG=0x{Msg:X4} wParam=0x{WParam:X8} lParam=0x{LParam:X8}", hwnd, msg, wParam, lParam);

			// Post message to the queue
			var success = _env.PostMessage(hwnd, msg, wParam, lParam);
			return success ? 1u : 0u; // TRUE : FALSE
		}

		[DllModuleExport(1)]
		private uint DialogBoxParamA(uint hInstance, uint lpTemplateName, uint hWndParent, uint lpDialogFunc, uint dwInitParam)
		{
			// Synchronous wrapper around async implementation
			return DialogBoxParamAsync(hInstance, lpTemplateName, hWndParent, lpDialogFunc, dwInitParam, CancellationToken.None).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Async version of DialogBoxParamA with cancellation token support.
		/// Creates a modal dialog box with proper async message loop and cooperative cancellation.
		/// </summary>
		private async Task<uint> DialogBoxParamAsync(uint hInstance, uint lpTemplateName, uint hWndParent, uint lpDialogFunc, uint dwInitParam, CancellationToken cancellationToken = default)
		{
			// DialogBoxParamA creates a modal dialog box
			_logger.LogInformation("[User32] DialogBoxParamAsync: hInstance=0x{HInstance:X8} lpTemplateName=0x{LpTemplateName:X8} lpDialogFunc=0x{LpDialogFunc:X8}", hInstance, lpTemplateName, lpDialogFunc);

			// Create a dialog window handle
			// For now, we create a synthetic dialog handle without parsing the template
			var hDlg = _env.RegisterHandle(new object()); // Dialog handle
			_logger.LogInformation("[User32] DialogBoxParamAsync: Created dialog handle=0x{HDlg:X8}", hDlg);

			// Initialize dialog state
			_env.InitializeDialogState(hDlg);

			// Call the dialog procedure with WM_INITDIALOG (0x0110)
			// WM_INITDIALOG signature: BOOL CALLBACK DialogProc(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
			// wParam = hWndParent (or 0 if no focus control)
			// lParam = dwInitParam
			const uint WM_INITDIALOG = 0x0110;
			var dialogProcTimedOut = false;
			var dialogProcCancelled = false;
			
			if (lpDialogFunc != 0)
			{
				_logger.LogInformation("[User32] DialogBoxParamAsync: Calling dialog procedure with WM_INITDIALOG");
				var (initResult, timedOut, cancelled) = await CallDialogProcedureAsync(_cpu, _memory, lpDialogFunc, hDlg, WM_INITDIALOG, 0, dwInitParam, cancellationToken);
				_logger.LogInformation("[User32] DialogBoxParamAsync: WM_INITDIALOG returned {InitResult}", initResult);
				dialogProcTimedOut = timedOut;
				dialogProcCancelled = cancelled;
			}
			else
			{
				_logger.LogWarning("[User32] DialogBoxParamAsync: No dialog procedure specified");
			}

			// If the dialog procedure timed out or was cancelled during initialization, end the dialog immediately
			if (dialogProcTimedOut || dialogProcCancelled)
			{
				_logger.LogWarning("[User32] DialogBoxParamAsync: Dialog procedure {Status}, ending dialog with result 0", 
					dialogProcCancelled ? "cancelled" : "timed out");
				_env.SetDialogResult(hDlg, 0);
			}

			// Run modal message loop until EndDialog is called
			_logger.LogInformation("[User32] DialogBoxParamAsync: Entering modal message loop");
			
			const int MAX_ITERATIONS = 10000; // Safety limit to prevent infinite loops
			var iterations = 0;
			var consecutiveEmptyIterations = 0;
			const int MAX_EMPTY_ITERATIONS = 100; // Exit if no messages for 100 iterations
			
			while (!_env.IsDialogEnded(hDlg) && iterations < MAX_ITERATIONS && !cancellationToken.IsCancellationRequested)
			{
				iterations++;
				
				// Check for quit message
				if (_env.HasQuitMessage())
				{
					_logger.LogInformation("[User32] DialogBoxParamAsync: Quit message received, breaking modal loop");
					break;
				}

				// Try to get a message (with short timeout to avoid blocking indefinitely)
				// Use async version for better cooperative multitasking
				var queuedMsg = await _env.GetMessageAsync(0, 0, 0, timeoutMs: 10);
				
				if (queuedMsg.HasValue)
				{
					consecutiveEmptyIterations = 0;
					var msg = queuedMsg.Value;
					_logger.LogDebug("[User32] DialogBoxParamAsync: Processing message MSG=0x{Message:X4} HWND=0x{Hwnd:X8}", msg.Message, msg.Hwnd);
					
					// Dispatch the message to the dialog procedure if it's for our dialog
					if (msg.Hwnd == hDlg || msg.Hwnd == 0)
					{
						if (lpDialogFunc != 0)
						{
							var (result, timedOut, cancelled) = await CallDialogProcedureAsync(_cpu, _memory, lpDialogFunc, hDlg, msg.Message, msg.WParam, msg.LParam, cancellationToken);
							_logger.LogDebug("[User32] DialogBoxParamAsync: Dialog procedure returned {Result} for MSG=0x{Message:X4}", result, msg.Message);
							
							// If dialog procedure times out or is cancelled, force end the dialog
							if (timedOut || cancelled)
							{
								_logger.LogWarning("[User32] DialogBoxParamAsync: Dialog procedure {Status} during message processing, forcing dialog end",
									cancelled ? "cancelled" : "timed out");
								_env.SetDialogResult(hDlg, 0);
							}
						}
					}
					else
					{
						// Message for a different window - requeue it
						_env.PostMessage(msg.Hwnd, msg.Message, msg.WParam, msg.LParam);
					}
				}
				else
				{
					consecutiveEmptyIterations++;
					
					// If we've had too many empty iterations and the dialog proc timed out, force end
					if (dialogProcTimedOut && consecutiveEmptyIterations >= MAX_EMPTY_ITERATIONS)
					{
						_logger.LogWarning("[User32] DialogBoxParamAsync: No messages and dialog procedure timed out, forcing dialog end");
						_env.SetDialogResult(hDlg, 0);
					}
					
					// Small delay to avoid tight loop
					await Task.Delay(1, cancellationToken);
				}
			}

			if (iterations >= MAX_ITERATIONS)
			{
				_logger.LogWarning("[User32] DialogBoxParamAsync: Exceeded max iterations, forcing dialog end");
			}

			if (cancellationToken.IsCancellationRequested)
			{
				_logger.LogInformation("[User32] DialogBoxParamAsync: Cancellation requested, ending dialog");
			}

			// Get the result from EndDialog
			var dialogResult = _env.GetDialogResult(hDlg);
			
			// Clean up dialog state
			_env.CleanupDialogState(hDlg);
			_env.CloseHandle(hDlg);
			
			_logger.LogInformation("[User32] DialogBoxParamAsync: Returning result={DialogResult}", dialogResult);
			return dialogResult;
		}

		/// <summary>
		/// Call a dialog procedure by setting up CPU state and executing the callback.
		/// DialogProc signature: INT_PTR CALLBACK DialogProc(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
		/// Uses stdcall calling convention (callee cleans stack, parameters pushed right-to-left)
		/// Returns a tuple of (returnValue, timedOut) where timedOut indicates if the procedure exceeded max steps.
		/// </summary>
		private (uint returnValue, bool timedOut) CallDialogProcedureWithTimeout(ICpu cpu, VirtualMemory memory, uint dialogProcAddress, uint hwndDlg, uint message, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] CallDialogProcedure: Calling 0x{DialogProcAddress:X8} with HWND=0x{HwndDlg:X8} MSG=0x{Message:X4}", dialogProcAddress, hwndDlg, message);

			// Save current CPU state
			var savedEip = cpu.GetEip();
			var savedEsp = cpu.GetRegister("ESP");
			var savedEbp = cpu.GetRegister("EBP");

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			var esp = savedEsp;

			// Push return address (we'll use a special marker address)
			const uint RETURN_ADDRESS = 0xDEADBEEF;
			esp -= 4;
			memory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			esp -= 4;
			memory.Write32(esp, lParam);

			esp -= 4;
			memory.Write32(esp, wParam);

			esp -= 4;
			memory.Write32(esp, message);

			esp -= 4;
			memory.Write32(esp, hwndDlg);

			// Update CPU registers
			cpu.SetRegister("ESP", esp);
			cpu.SetEip(dialogProcAddress);

			// Execute until we hit the return address
			// Use unlimited steps for dialog procedures to support complex UI operations
			// that may involve extensive initialization, layout calculations, or event processing.
			// The procedure will naturally terminate when it returns (hits RETURN_ADDRESS).
			// To prevent true infinite loops, we track progress via:
			// - Detecting when EIP stops changing (infinite loop detection)
			// - Monitoring for repeated execution at the same address
			const int MAX_STEPS = int.MaxValue; // No artificial limit
			// YIELD_INTERVAL: Check for context switches every 10K instructions
			// Rationale: 10K provides good balance between:
			// - Responsiveness: Allows context switches ~50 times during max execution
			// - Performance: Low overhead (~0.001% for scheduler checks)
			// - Granularity: Fine enough for cooperative multitasking
			const int YIELD_INTERVAL = 10000;
			var steps = 0;
			var timedOut = false;
			var lastCheckEip = cpu.GetEip();
			var stuckCounter = 0;

			try
			{
				while (steps < MAX_STEPS)
				{
					var eip = cpu.GetEip();

					// Check if we've returned to our marker address
					if (eip == RETURN_ADDRESS)
					{
						break;
					}

					// Detect potential infinite loops by checking if we're making progress
					if (steps > 0 && steps % INFINITE_LOOP_CHECK_INTERVAL == 0)
					{
						var currentEip = cpu.GetEip();
						if (currentEip == lastCheckEip)
						{
							stuckCounter++;
							if (stuckCounter >= STUCK_COUNTER_THRESHOLD)
							{
								// We've been at the same instruction for multiple check intervals - likely an infinite loop
								_logger.LogWarning("[User32] CallDialogProcedure: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting", currentEip, stuckCounter);
								timedOut = true;
								break;
							}
						}
						else
						{
							stuckCounter = 0;
							lastCheckEip = currentEip;
						}
					}

					// Execute one instruction and check for import calls
					var step = cpu.SingleStep(memory);
					
					// Check for COM vtable method calls
					if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
					{
						_logger.LogDebug("[User32] CallDialogProcedure: COM vtable call at 0x{CallTarget:X8}", step.CallTarget);
						
						// Save callee-saved registers (EBX, ESI, EDI)
						var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);
						
						if (_env.ComDispatcher.TryInvoke(step.CallTarget, cpu, memory, out var comRet, out var comArgBytes))
						{
							var currentEsp = cpu.GetRegister("ESP");
							var retEip = memory.Read32(currentEsp);
							currentEsp += 4 + (uint)comArgBytes; // Pop return address + arguments
							cpu.SetRegister("ESP", currentEsp);
							cpu.SetRegister("EAX", comRet);
							cpu.SetEip(retEip);
							
							// Restore callee-saved registers
							CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved);
							
							RestoreEbpFromStack(cpu, memory, currentEsp);
						}
					}
					// Check for import calls
					else if (step.IsCall && _image != null && _image.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
					{
						var dll = imp.dll.ToUpperInvariant();
						var name = imp.name;
						_logger.LogDebug("[User32] CallDialogProcedure: Import call {Dll}!{Name} at 0x{CallTarget:X8}", dll, name, step.CallTarget);
						
						// Save callee-saved registers (EBX, ESI, EDI)
						var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);
						
						if (_dispatcher != null && _dispatcher.TryInvoke(dll, name, cpu, memory, out var ret, out var argBytes))
						{
							_logger.LogDebug("[User32] CallDialogProcedure: Import {Dll}!{Name} returned 0x{Ret:X8}", dll, name, ret);
							var currentEsp = cpu.GetRegister("ESP");
							var retEip = memory.Read32(currentEsp);
							
							currentEsp += 4 + (uint)argBytes;
							
							cpu.SetRegister("ESP", currentEsp);
							cpu.SetRegister("EAX", ret);
							cpu.SetEip(retEip);
							
							// Restore callee-saved registers
							CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved);
							
							RestoreEbpFromStack(cpu, memory, currentEsp);
						}
					}

					steps++;
					
					// Periodically check if we should yield to other threads
					if (steps % YIELD_INTERVAL == 0)
					{
						var scheduler = _env.ThreadScheduler;
						if (scheduler != null)
						{
							// Process any waiting thread timeouts
							scheduler.ProcessWaitTimeouts();
							
							// Check if there are other threads that need CPU time
							if (scheduler.ShouldContextSwitch())
							{
								_logger.LogDebug("[User32] CallDialogProcedure: Cooperative yield at {Steps} steps", steps);
								// Note: We can't actually context switch here since we're mid-call
								// But we log it for diagnostics. In a future enhancement, we could
								// save state and resume the call later.
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning("[User32] CallDialogProcedure: Exception during execution: {ExMessage}", ex.Message);
			}

			if (steps >= MAX_STEPS)
			{
				_logger.LogWarning("[User32] CallDialogProcedure: Exceeded max steps ({MaxSteps}), aborting - DialogProc may be in infinite loop", MAX_STEPS);
				timedOut = true;
			}

			// Get return value from EAX
			var returnValue = cpu.GetRegister("EAX");

			// Restore CPU state
			cpu.SetEip(savedEip);
			cpu.SetRegister("ESP", savedEsp);
			cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] CallDialogProcedure: Completed with return value 0x{ReturnValue:X8}", returnValue);

			return (returnValue, timedOut);
		}

		/// <summary>
		/// Async version of CallDialogProcedureWithTimeout with cancellation token support.
		/// Allows cooperative cancellation during long-running dialog procedure execution.
		/// </summary>
		private async Task<(uint returnValue, bool timedOut, bool cancelled)> CallDialogProcedureAsync(
			ICpu cpu, VirtualMemory memory, uint dialogProcAddress, uint hwndDlg, uint message, uint wParam, uint lParam, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("[User32] CallDialogProcedureAsync: Calling 0x{DialogProcAddress:X8} with HWND=0x{HwndDlg:X8} MSG=0x{Message:X4}", dialogProcAddress, hwndDlg, message);

			// Save current CPU state
			var savedEip = cpu.GetEip();
			var savedEsp = cpu.GetRegister("ESP");
			var savedEbp = cpu.GetRegister("EBP");

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			var esp = savedEsp;

			// Push return address (we'll use a special marker address)
			const uint RETURN_ADDRESS = 0xDEADBEEF;
			esp -= 4;
			memory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			esp -= 4;
			memory.Write32(esp, lParam);

			esp -= 4;
			memory.Write32(esp, wParam);

			esp -= 4;
			memory.Write32(esp, message);

			esp -= 4;
			memory.Write32(esp, hwndDlg);

			// Update CPU registers
			cpu.SetRegister("ESP", esp);
			cpu.SetEip(dialogProcAddress);

			// Execute until we hit the return address with cancellation support
			const int MAX_STEPS = int.MaxValue; // No artificial limit
			const int YIELD_INTERVAL = 10000;
			var steps = 0;
			var timedOut = false;
			var cancelled = false;
			var lastCheckEip = cpu.GetEip();
			var stuckCounter = 0;

			try
			{
				while (steps < MAX_STEPS)
				{
					// Check for cancellation at regular intervals
					if (steps % CANCELLATION_CHECK_INTERVAL == 0)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							_logger.LogInformation("[User32] CallDialogProcedureAsync: Cancellation requested at step {Steps}", steps);
							cancelled = true;
							break;
						}
						
						// Yield to allow other async operations to proceed
						await Task.Yield();
					}

					var eip = cpu.GetEip();

					// Check if we've returned to our marker address
					if (eip == RETURN_ADDRESS)
					{
						break;
					}

					// Detect potential infinite loops by checking if we're making progress
					if (steps > 0 && steps % INFINITE_LOOP_CHECK_INTERVAL == 0)
					{
						var currentEip = cpu.GetEip();
						if (currentEip == lastCheckEip)
						{
							stuckCounter++;
							if (stuckCounter >= STUCK_COUNTER_THRESHOLD)
							{
								// We've been at the same instruction for multiple check intervals - likely an infinite loop
								_logger.LogWarning("[User32] CallDialogProcedureAsync: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting", currentEip, stuckCounter);
								timedOut = true;
								break;
							}
						}
						else
						{
							stuckCounter = 0;
							lastCheckEip = currentEip;
						}
					}

					// Execute one instruction and check for import calls
					var step = cpu.SingleStep(memory);
					
					// Check for COM vtable method calls
					if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
					{
						_logger.LogDebug("[User32] CallDialogProcedureAsync: COM vtable call at 0x{CallTarget:X8}", step.CallTarget);
						
						// Save callee-saved registers (EBX, ESI, EDI)
						var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);
						
						if (_env.ComDispatcher.TryInvoke(step.CallTarget, cpu, memory, out var comRet, out var comArgBytes))
						{
							var currentEsp = cpu.GetRegister("ESP");
							var retEip = memory.Read32(currentEsp);
							currentEsp += 4 + (uint)comArgBytes; // Pop return address + arguments
							cpu.SetRegister("ESP", currentEsp);
							cpu.SetRegister("EAX", comRet);
							cpu.SetEip(retEip);
							
							// Restore callee-saved registers
							CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved);
							
							RestoreEbpFromStack(cpu, memory, currentEsp);
						}
					}
					// Check for import calls
					else if (step.IsCall && _image != null && _image.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
					{
						var dll = imp.dll.ToUpperInvariant();
						var name = imp.name;
						_logger.LogDebug("[User32] CallDialogProcedureAsync: Import call {Dll}!{Name} at 0x{CallTarget:X8}", dll, name, step.CallTarget);
						
						// Save callee-saved registers (EBX, ESI, EDI)
						var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);
						
						if (_dispatcher != null && _dispatcher.TryInvoke(dll, name, cpu, memory, out var ret, out var argBytes))
						{
							_logger.LogDebug("[User32] CallDialogProcedureAsync: Import {Dll}!{Name} returned 0x{Ret:X8}", dll, name, ret);
							var currentEsp = cpu.GetRegister("ESP");
							var retEip = memory.Read32(currentEsp);
							
							currentEsp += 4 + (uint)argBytes;
							
							cpu.SetRegister("ESP", currentEsp);
							cpu.SetRegister("EAX", ret);
							cpu.SetEip(retEip);
							
							// Restore callee-saved registers
							CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved);
							
							RestoreEbpFromStack(cpu, memory, currentEsp);
						}
					}

					steps++;
					
					// Periodically check if we should yield to other threads
					if (steps % YIELD_INTERVAL == 0)
					{
						var scheduler = _env.ThreadScheduler;
						if (scheduler != null)
						{
							// Process any waiting thread timeouts
							scheduler.ProcessWaitTimeouts();
							
							// Check if there are other threads that need CPU time
							if (scheduler.ShouldContextSwitch())
							{
								_logger.LogDebug("[User32] CallDialogProcedureAsync: Cooperative yield at {Steps} steps", steps);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning("[User32] CallDialogProcedureAsync: Exception during execution: {ExMessage}", ex.Message);
			}

			if (steps >= MAX_STEPS)
			{
				_logger.LogWarning("[User32] CallDialogProcedureAsync: Exceeded max steps ({MaxSteps}), aborting - DialogProc may be in infinite loop", MAX_STEPS);
				timedOut = true;
			}

			// Get return value from EAX
			var returnValue = cpu.GetRegister("EAX");

			// Restore CPU state
			cpu.SetEip(savedEip);
			cpu.SetRegister("ESP", savedEsp);
			cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] CallDialogProcedureAsync: Completed with return value 0x{ReturnValue:X8}, timedOut={TimedOut}, cancelled={Cancelled}", 
				returnValue, timedOut, cancelled);

			return (returnValue, timedOut, cancelled);
		}


		[DllModuleExport(1)]
		private uint EndDialog(uint hDlg, uint nResult)
		{
			// EndDialog closes a modal dialog box and sets its result
			_logger.LogInformation("[User32] EndDialog: hDlg=0x{HDlg:X8} nResult={NResult}", hDlg, nResult);
			
			// Set the dialog result in the process environment
			// This will signal DialogBoxParamA to exit its message loop
			var success = _env.SetDialogResult(hDlg, nResult);
			
			return success ? 1u : 0u; // TRUE if successful, FALSE otherwise
		}

		[DllModuleExport(1)]
		private uint GetDlgItem(uint hDlg, int nIDDlgItem)
		{
			// GetDlgItem retrieves a handle to a control in a dialog box
			_logger.LogInformation("[User32] GetDlgItem: hDlg=0x{HDlg:X8} nIDDlgItem={NIdDlgItem}", hDlg, nIDDlgItem);

			// Return a synthetic handle (dialog handle + control ID)
			return hDlg + (uint)nIDDlgItem;
		}

		[DllModuleExport(1)]
		private uint GetDlgItemTextA(uint hDlg, int nIDDlgItem, uint lpString, int cchMax)
		{
			// GetDlgItemTextA retrieves the text of a control in a dialog box
			_logger.LogInformation("[User32] GetDlgItemTextA: hDlg=0x{HDlg:X8} nIDDlgItem={NIdDlgItem} cchMax={CchMax}", hDlg, nIDDlgItem, cchMax);

			if (lpString == 0 || cchMax <= 0)
			{
				return 0;
			}

			// Get the text from the dialog control
			var text = _env.GetDialogControlText(hDlg, nIDDlgItem);
			
			if (string.IsNullOrEmpty(text))
			{
				// Return empty string
				_env.MemWriteBytes(lpString, new byte[] { 0 });
				return 0;
			}

			// Truncate text if it exceeds buffer size (including null terminator)
			// Note: This uses simple string truncation which is appropriate for Win32 ANSI APIs
			// that work with single-byte character sets. For DBCS (double-byte character sets),
			// Win32 would use functions like IsDBCSLeadByte to handle multibyte sequences.
			var maxLength = cchMax - 1; // Leave room for null terminator
			if (text.Length > maxLength)
			{
				text = text.Substring(0, maxLength);
			}

			// Write the text to memory using ASCII encoding (Win32 ANSI API)
			var bytes = System.Text.Encoding.ASCII.GetBytes(text + '\0');
			_env.MemWriteBytes(lpString, bytes);
			
			// Return the number of characters copied (excluding null terminator)
			return (uint)text.Length;
		}

		[DllModuleExport(1)]
		private unsafe uint SetDlgItemTextA(uint hDlg, int nIDDlgItem, sbyte* lpString)
		{
			// SetDlgItemTextA sets the text of a control in a dialog box
			_logger.LogInformation("[User32] SetDlgItemTextA: hDlg=0x{HDlg:X8} nIDDlgItem={NIdDlgItem}", hDlg, nIDDlgItem);

			string text;
			if (lpString == null)
			{
				text = string.Empty;
			}
			else
			{
				var lpStringPtr = (uint)(nint)lpString;
				text = _env.ReadAnsiString(lpStringPtr);
			}
			
			// Store the text in the dialog control
			_env.SetDialogControlText(hDlg, nIDDlgItem, text);
			
			_logger.LogInformation("[User32] SetDlgItemTextA: Set text '{Text}' for control {NIdDlgItem}", text, nIDDlgItem);
			
			return 1; // TRUE on success
		}

		[DllModuleExport(1)]
		private uint SendDlgItemMessageA(uint hDlg, int nIDDlgItem, uint msg, uint wParam, uint lParam)
		{
			// SendDlgItemMessageA sends a message to a control in a dialog box
			_logger.LogInformation("[User32] SendDlgItemMessageA: hDlg=0x{HDlg:X8} nIDDlgItem={NIdDlgItem} msg=0x{Msg:X4}", hDlg, nIDDlgItem, msg);

			// Return 0 (default message handling result)
			return 0;
		}

		[DllModuleExport(1)]
		private uint EnableWindow(uint hwnd, uint bEnable)
		{
			// EnableWindow enables or disables mouse and keyboard input to a window
			// Returns the previous enable state: nonzero if previously disabled, zero if previously enabled
			_logger.LogInformation("[User32] EnableWindow: HWND=0x{Hwnd:X8} bEnable={BEnable}", hwnd, bEnable);

			// Get the previous state (default to enabled if not tracked)
			var wasEnabled = _windowEnabledState.GetValueOrDefault(hwnd, true);

			// Update the state
			_windowEnabledState[hwnd] = bEnable != 0;

			// Return previous state: return 0 if was enabled, non-zero if was disabled
			return wasEnabled ? 0u : 1u;
		}

		[DllModuleExport(1)]
		private uint BeginPaint(uint hwnd, uint lpPaint)
		{
			_logger.LogInformation("[User32] BeginPaint: HWND=0x{Hwnd:X8} lpPaint=0x{LpPaint:X8}", hwnd, lpPaint);

			if (lpPaint == 0)
			{
				return 0;
			}

			// Get a device context
			var hdc = GetDc(hwnd);

			// Fill the PAINTSTRUCT structure (64 bytes)
			// HDC  hdc;         // 0
			// BOOL fErase;      // 4
			// RECT rcPaint;     // 8
			// BOOL fRestore;    // 24
			// BOOL fIncUpdate;  // 28
			// BYTE rgbReserved[32]; // 32

			_env.MemWrite32(lpPaint + 0, hdc);
			_env.MemWrite32(lpPaint + 4, 1); // fErase = TRUE

			// Get the client rectangle for rcPaint
			GetClientRect(hwnd, lpPaint + 8);

			// Zero out fRestore, fIncUpdate, and rgbReserved
			for (uint i = 24; i < 64; i++)
			{
				_env.MemWrite8(lpPaint + i, 0);
			}

			return hdc;
		}

		[DllModuleExport(1)]
		private uint EndPaint(uint hwnd, uint lpPaint)
		{
			_logger.LogInformation("[User32] EndPaint: HWND=0x{Hwnd:X8} lpPaint=0x{LpPaint:X8}", hwnd, lpPaint);

			if (lpPaint != 0)
			{
				var hdc = _env.MemRead32(lpPaint + 0);
				ReleaseDc(hwnd, hdc);
			}

			return 1; // Always returns non-zero
		}

		[DllModuleExport(1)]
		private uint FillRect(uint hdc, uint lprc, uint hbr)
		{
			_logger.LogInformation("[User32] FillRect: hdc=0x{Hdc:X8} lprc=0x{Lprc:X8} hbr=0x{Hbr:X8}", hdc, lprc, hbr);

			if (lprc != 0)
			{
				var left = (int)_env.MemRead32(lprc);
				var top = (int)_env.MemRead32(lprc + 4);
				var right = (int)_env.MemRead32(lprc + 8);
				var bottom = (int)_env.MemRead32(lprc + 12);
				_logger.LogInformation("[User32] FillRect: rect=({Left},{Top},{Right},{Bottom})", left, top, right, bottom);
			}

			// For now, we don't do any actual drawing.
			// Just return success.
			return 1;
		}

		[DllModuleExport(4)]
		private uint CharNextA(uint lpsz)
		{
			if (lpsz == 0)
			{
				_logger.LogInformation("[User32] CharNextA: NULL pointer");
				return 0;
			}

			// Read the byte at the current position
			var currentByte = _env.MemRead8(lpsz);

			// If we're at the null terminator, return the same pointer
			if (currentByte == 0)
			{
				_logger.LogInformation("[User32] CharNextA: At null terminator, returning same pointer 0x{Lpsz:X8}", lpsz);
				return lpsz;
			}

			// For single-byte character sets (ASCII), just advance by 1 byte
			// A full implementation would need to check for multi-byte character sequences
			// and use IsDBCSLeadByte to determine if this is a lead byte in a DBCS encoding
			var nextPtr = lpsz + 1;

			_logger.LogDebug("[User32] CharNextA: lpsz=0x{Lpsz:X8} currentChar={CurrentChar} currentByte=0x{CurrentByte:X2} -> next=0x{NextPtr:X8}", lpsz, (char)currentByte, currentByte, nextPtr);

			return nextPtr;
		}

		/// <summary>
		/// Attempts to restore EBP from the stack after an emulated API call.
		/// This handles cases where the calling code used EBP to hold the function pointer for an indirect call.
		/// </summary>
		private void RestoreEbpFromStack(ICpu cpu, VirtualMemory memory, uint esp)
		{
			try
			{
				var ebpFromStack = memory.Read32(esp);

				// Define plausible stack region (for example, 1MB stack)
				// Assume stack grows down, so stack base is the highest address, stack limit is lowest
				// Here, we use current ESP as the top of the stack, and allow up to 1MB below
				const uint STACK_SIZE = 0x100000; // 1MB
				uint stackTop = esp;
				uint stackBottom = (esp > STACK_SIZE) ? (esp - STACK_SIZE) : 0x00100000; // Don't go below 1MB

				bool inStackRegion = (ebpFromStack >= stackBottom) && (ebpFromStack <= stackTop);
				bool isAligned = (ebpFromStack & 0x3) == 0;

				// Optionally, check that the memory at ebpFromStack is readable and contains a plausible saved EBP
				bool savedEbpValid = false;
				if (inStackRegion && isAligned)
				{
					try
					{
						var savedEbp = memory.Read32(ebpFromStack);
						// Check that savedEbp is also within stack region (optional, but plausible)
						savedEbpValid = (savedEbp >= stackBottom) && (savedEbp <= stackTop);
					}
					catch
					{
						savedEbpValid = false;
					}
				}

				if (inStackRegion && isAligned && savedEbpValid)
				{
					cpu.SetRegister("EBP", ebpFromStack);
					_logger.LogDebug("[User32] Restored EBP from stack: 0x{EBP:X8}", ebpFromStack);
				}
				else
				{
					_logger.LogWarning("[User32] Skipped restoring EBP from stack: 0x{EBP:X8} (invalid frame pointer)", ebpFromStack);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "[User32] Failed to restore EBP from stack");
			}
		}
	}
}
