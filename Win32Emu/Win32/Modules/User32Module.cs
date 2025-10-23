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
		private ICpu? _cpu;
		private VirtualMemory? _memory;
		private Win32Dispatcher? _dispatcher;
		private LoadedImage? _image;
		private PeResourceReader? _resourceReader;
		private IEmulatorHost? _host;
		
		// State tracking for cursor and focus
		private uint _currentCursor;
		private uint _focusWindow;

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

		public void SetResourceReader(PeResourceReader resourceReader)
		{
			_resourceReader = resourceReader;
		}

		public void SetHost(IEmulatorHost? host)
		{
			_host = host;
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

				case "REGISTERWINDOWMESSAGEA":
					returnValue = RegisterWindowMessageA(a.Lpstr(0));
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

				case "EXITWINDOWSEX":
					returnValue = ExitWindowsEx(a.UInt32(0), a.UInt32(1));
					return true;

				case "GETWINDOWTEXTA":
					returnValue = GetWindowTextA(a.UInt32(0), a.LpStr(1), a.Int32(2));
					return true;

				case "SETWINDOWTEXTA":
					returnValue = SetWindowTextA(a.UInt32(0), a.LpcStr(1));
					return true;

				case "LOADIMAGEA":
					returnValue = LoadImageA(a.UInt32(0), a.LpStr(1), a.UInt32(2), a.Int32(3), a.Int32(4), a.UInt32(5));
					return true;

				case "LOADSTRINGA":
					returnValue = LoadStringA(a.UInt32(0), a.UInt32(1), a.LpStr(2), a.Int32(3));
					return true;

				case "WSPRINTFA":
					returnValue = WsprintfA(a.LpStr(0), a.LpcStr(1), a);
					return true;

				case "WVSPRINTFA":
					returnValue = WvsprintfA(a.LpStr(0), a.LpcStr(1), a.UInt32(2));
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

		/// <summary>
		/// Defines a new window message that is guaranteed to be unique throughout the system.
		/// The message value can be used in calls to SendMessage or PostMessage.
		/// </summary>
		/// <param name="lpString">
		/// A pointer to a null-terminated string that specifies the message to be registered.
		/// </param>
		/// <returns>
		/// If the message is successfully registered, the return value is a message identifier in the range 0xC000 through 0xFFFF.
		/// If the function fails, the return value is zero. To get extended error information, call GetLastError.
		/// </returns>
		/// <remarks>
		/// The RegisterWindowMessage function is typically used to register messages for communicating between two cooperating applications.
		/// If two different applications register the same message string, the applications return the same message value.
		/// The message remains registered until the session ends.
		/// Only use RegisterWindowMessage when more than one application must process the same message.
		/// For sending private messages within a window class, an application can use any integer in the range WM_USER through 0x7FFF.
		/// (Messages in this range are private to a window class, not to an application. For example, predefined control classes
		/// such as BUTTON, EDIT, LISTBOX, and COMBOBOX may already be using values in this range.)
		/// </remarks>
		[DllModuleExport(18)]
		private unsafe uint RegisterWindowMessageA(sbyte* lpString)
		{
			// Validate the input pointer
			if (lpString == null)
			{
				_logger.LogWarning("[User32] RegisterWindowMessageA: NULL string pointer");
				return 0;
			}

			var lpStringPtr = (uint)(nint)lpString;
			
			// Read the message string from memory
			var messageString = _env.ReadAnsiString(lpStringPtr);

			if (string.IsNullOrEmpty(messageString))
			{
				_logger.LogWarning("[User32] RegisterWindowMessageA: Empty message string");
				return 0;
			}

			// Register the message in the process environment
			var messageId = _env.RegisterWindowMessage(messageString);

			_logger.LogInformation("[User32] RegisterWindowMessageA: '{MessageString}' -> 0x{MessageId:X4}", messageString, messageId);
			
			return messageId;
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

			// Get the current window to check if it exists and get previous visibility
			var window = _env.GetWindow(hwnd);
			if (window == null)
			{
				_logger.LogWarning("[User32] ShowWindow: Invalid HWND=0x{Hwnd:X8}", hwnd);
				return 0; // Window was not previously visible
			}

			// Check if window was previously visible (has WS_VISIBLE style)
			var wasPreviouslyVisible = (window.Value.Style & NativeTypes.WindowStyle.WS_VISIBLE) != 0;

			// Update visibility based on nCmdShow
			// SW_HIDE = 0, SW_SHOWNORMAL = 1, SW_SHOWMINIMIZED = 2, SW_SHOWMAXIMIZED = 3,
			// SW_MAXIMIZE = 3, SW_SHOWNOACTIVATE = 4, SW_SHOW = 5, SW_MINIMIZE = 6,
			// SW_SHOWMINNOACTIVE = 7, SW_SHOWNA = 8, SW_RESTORE = 9
			bool shouldBeVisible = nCmdShow != 0; // SW_HIDE = 0, all others show the window

			// Get current style from window properties (which may have been modified)
			var currentStyle = _env.GetWindowProperty(hwnd, NativeTypes.WindowLong.GWL_STYLE);
			if (currentStyle == 0)
			{
				// No custom style set, use the window's original style
				currentStyle = window.Value.Style;
			}

			// Update the WS_VISIBLE flag
			if (shouldBeVisible)
			{
				currentStyle |= NativeTypes.WindowStyle.WS_VISIBLE;
				_logger.LogInformation("[User32] ShowWindow: Window 0x{Hwnd:X8} is now visible", hwnd);
				
				// Send WM_ACTIVATEAPP message when window becomes visible
				// WM_ACTIVATEAPP = 0x001C, wParam = TRUE (1) for activation, lParam = 0 (thread ID)
				if (!wasPreviouslyVisible)
				{
					_env.SendMessageToWindow(hwnd, 0x001C, 1, 0);
					_logger.LogDebug("[User32] ShowWindow: Sent WM_ACTIVATEAPP to window 0x{Hwnd:X8}", hwnd);
				}
			}
			else
			{
				currentStyle &= ~NativeTypes.WindowStyle.WS_VISIBLE;
				_logger.LogInformation("[User32] ShowWindow: Window 0x{Hwnd:X8} is now hidden", hwnd);
				
				// Send WM_ACTIVATEAPP message when window becomes hidden
				// WM_ACTIVATEAPP = 0x001C, wParam = FALSE (0) for deactivation, lParam = 0 (thread ID)
				if (wasPreviouslyVisible)
				{
					_env.SendMessageToWindow(hwnd, 0x001C, 0, 0);
					_logger.LogDebug("[User32] ShowWindow: Sent WM_ACTIVATEAPP (deactivate) to window 0x{Hwnd:X8}", hwnd);
				}
			}

			// Store the updated style in window properties
			_env.SetWindowProperty(hwnd, NativeTypes.WindowLong.GWL_STYLE, currentStyle);

			// Return non-zero if window was previously visible, zero if it was previously hidden
			return wasPreviouslyVisible ? 1u : 0u;
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

				var result = CallWindowProcedure(_cpu!, _memory!, wndProc.Value, hwnd, message, wParam, lParam);
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

			// Push parameters (right-to-left for stdcall)
			esp -= 4;
			memory.Write32(esp, lParam);

			esp -= 4;
			memory.Write32(esp, wParam);

			esp -= 4;
			memory.Write32(esp, message);

			esp -= 4;
			memory.Write32(esp, hwnd);

			// Push return address (we'll use a special marker address)
			// This must be pushed AFTER parameters so it's on top of the stack
			const uint RETURN_ADDRESS = 0xDEADBEEF;
			esp -= 4;
			memory.Write32(esp, RETURN_ADDRESS);

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
			var executionSuccessful = true;

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

					// Check for invalid EIP (NULL pointer execution)
					if (eip == 0x00000000)
					{
						_logger.LogWarning("[User32] CallWindowProcedure: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting");
						executionSuccessful = false;
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
								executionSuccessful = false;
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
					var step = cpu.SingleStep(memory);

					// Check for COM vtable method calls
					if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
					{
						_logger.LogDebug("[User32] CallWindowProcedure: COM vtable call at 0x{CallTarget:X8}", step.CallTarget);

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
					// Check for import calls - these need to be dispatched to emulated Win32 functions
					else if (step.IsCall && _image != null && _image.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
					{
						var dll = imp.dll.ToUpperInvariant();
						var name = imp.name;
						_logger.LogDebug("[User32] CallWindowProcedure: Import call {Dll}!{Name} at 0x{CallTarget:X8}", dll, name, step.CallTarget);

						// Save callee-saved registers (EBX, ESI, EDI)
						var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);

						if (_dispatcher != null && _dispatcher.TryInvoke(dll, name, cpu, memory, out var ret, out var argBytes))
						{
							_logger.LogDebug("[User32] CallWindowProcedure: Import {Dll}!{Name} returned 0x{Ret:X8}", dll, name, ret);
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
						else
						{
							// Import function not implemented - try to get arg bytes from metadata and simulate return
							var simulatedArgBytes = 0;
							try
							{
								simulatedArgBytes = StdCallMeta.GetArgBytes(dll, name);
								_logger.LogWarning("[User32] CallWindowProcedure: Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes={ArgBytes}", dll, name, simulatedArgBytes);
							}
							catch
							{
								_logger.LogWarning("[User32] CallWindowProcedure: Unimplemented import {Dll}!{Name}, no metadata available, simulating return with 0, argBytes=0", dll, name);
							}

							var currentEsp = cpu.GetRegister("ESP");
							var retEip = memory.Read32(currentEsp);

							currentEsp += 4 + (uint)simulatedArgBytes;

							cpu.SetRegister("ESP", currentEsp);
							cpu.SetRegister("EAX", 0);
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
				_logger.LogWarning(ex, "[User32] CallWindowProcedure: Exception during execution: {ExMessage}", ex.Message);
				executionSuccessful = false;
			}

			if (steps >= MAX_STEPS)
			{
				_logger.LogWarning("[User32] CallWindowProcedure: Exceeded max steps ({MaxSteps}), aborting - WndProc may be in infinite loop", MAX_STEPS);
				executionSuccessful = false;
			}

			// Get return value from EAX, but only if execution was successful
			// Otherwise return 0 as a safe default value
			var returnValue = executionSuccessful ? cpu.GetRegister("EAX") : 0u;

			// If execution was not successful, we need to clean up the stack memory
			// to prevent corruption that could affect subsequent calls
			if (!executionSuccessful)
			{
				// Clear the stack memory region that was used for the call
				// This includes the return address and parameters (5 dwords = 20 bytes)
				var stackDataSize = 20u; // Return address (4) + hwnd (4) + message (4) + wParam (4) + lParam (4)
				// Use a single bulk write for efficiency
				memory.WriteBytes(savedEsp - stackDataSize, new byte[stackDataSize]);
				_logger.LogDebug("[User32] CallWindowProcedure: Cleaned up {Size} bytes of stack memory after failed execution", stackDataSize);
			}

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
				var result = CallWindowProcedure(_cpu!, _memory!, wndProc.Value, hwnd, msg, wParam, lParam);
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
			// The API contract says we should write back the coordinates, but for now
			// we'll skip this to avoid potential stack corruption issues
			// TODO: Implement proper coordinate conversion and write-back
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
			switch (nIndex)
			{
				case 0:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning SM_CXSCREEN (0): 1920");
					return 1920; // SM_CXSCREEN - Screen width
				case 1:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning SM_CYSCREEN (1): 1080");
					return 1080; // SM_CYSCREEN - Screen height
				case 4:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning SM_CXSCREEN (4): 640");
					return 640; // SM_CXMIN - Minimum window width
				case 5:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning SM_CXSCREEN (5): 480");
					return 480; // SM_CYMIN - Minimum window height
				default:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning {SystemMetric} ({SystemMetricValue}): 0", ((SystemMetric)nIndex).ToString(), nIndex);
					return 0;
			}
		}

		enum SystemMetric
		{
			/// <summary>
			/// The flags that specify how the system arranged minimized windows. For more information, see the Remarks section in this topic.
			/// </summary>
			SM_ARRANGE = 56,
			/// <summary>
			/// The value that specifies how the system is started:
			/// 0 Normal boot
			/// 1 Fail-safe boot
			/// 2 Fail-safe with network boot
			/// A fail-safe boot (also called SafeBoot, Safe Mode, or Clean Boot) bypasses the user startup files.
			/// </summary>
			SM_CLEANBOOT = 67,
			/// <summary>
			/// The number of display monitors on a desktop. For more information, see the Remarks section in this topic.
			/// </summary>
			SM_CMONITORS = 80,
			/// <summary>
			/// The number of buttons on a mouse, or zero if no mouse is installed.
			/// </summary>
			SM_CMOUSEBUTTONS = 43,
			/// <summary>
			/// Reflects the state of the laptop or slate mode, 0 for Slate Mode and non-zero otherwise. When this system metric changes, the system sends a broadcast message via WM_SETTINGCHANGE with "ConvertibleSlateMode" in the LPARAM. Note that this system metric doesn't apply to desktop PCs. In that case, use GetAutoRotationState.
			/// </summary>
			SM_CONVERTIBLESLATEMODE = 0x2003,
			/// <summary>
			/// The width of a window border, in pixels. This is equivalent to the SM_CXEDGE value for windows with the 3-D look.
			/// </summary>
			SM_CXBORDER = 5,
			/// <summary>
			/// The nominal width of a cursor, in pixels.
			/// </summary>
			SM_CXCURSOR = 13,
			/// <summary>
			/// This value is the same as SM_CXFIXEDFRAME.
			/// </summary>
			SM_CXDLGFRAME = 7,
			/// <summary>
			/// The width of the rectangle around the location of a first click in a double-click sequence, in pixels. The second click must occur within the rectangle that is defined by SM_CXDOUBLECLK and SM_CYDOUBLECLK for the system to consider the two clicks a double-click. The two clicks must also occur within a specified time.
			/// To set the width of the double-click rectangle, call SystemParametersInfo with SPI_SETDOUBLECLKWIDTH.
			/// </summary>
			SM_CXDOUBLECLK = 36,
			/// <summary>
			/// The number of pixels on either side of a mouse-down point that the mouse pointer can move before a drag operation begins. This allows the user to click and release the mouse button easily without unintentionally starting a drag operation. If this value is negative, it is subtracted from the left of the mouse-down point and added to the right of it.
			/// </summary>
			SM_CXDRAG = 68,
			/// <summary>
			/// The width of a 3-D border, in pixels. This metric is the 3-D counterpart of SM_CXBORDER.
			/// </summary>
			SM_CXEDGE = 45,
			/// <summary>
			/// The thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels. SM_CXFIXEDFRAME is the height of the horizontal border, and SM_CYFIXEDFRAME is the width of the vertical border.
			/// This value is the same as SM_CXDLGFRAME.
			/// </summary>
			SM_CXFIXEDFRAME = 7,
			/// <summary>
			/// The width of the left and right edges of the focus rectangle that the DrawFocusRect draws. This value is in pixels.
			/// Windows 2000:  This value is not supported.
			/// </summary>
			SM_CXFOCUSBORDER = 83,
			/// <summary>
			/// This value is the same as SM_CXSIZEFRAME.
			/// </summary>
			SM_CXFRAME = 32,
			/// <summary>
			/// The width of the client area for a full-screen window on the primary display monitor, in pixels. To get the coordinates of the portion of the screen that is not obscured by the system taskbar or by application desktop toolbars, call the SystemParametersInfo function with the SPI_GETWORKAREA value.
			/// </summary>
			SM_CXFULLSCREEN = 16,
			/// <summary>
			/// The width of the arrow bitmap on a horizontal scroll bar, in pixels.
			/// </summary>
			SM_CXHSCROLL = 21,
			/// <summary>
			/// The width of the thumb box in a horizontal scroll bar, in pixels.
			/// </summary>
			SM_CXHTHUMB = 10,
			/// <summary>
			/// The system large width of an icon, in pixels. The LoadIcon function can load only icons with the dimensions that SM_CXICON and SM_CYICON specifies. See Icon Sizes for more info.
			/// </summary>
			SM_CXICON = 11,
			/// <summary>
			/// The width of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size SM_CXICONSPACING by SM_CYICONSPACING when arranged. This value is always greater than or equal to SM_CXICON.
			/// </summary>
			SM_CXICONSPACING = 38,
			/// <summary>
			/// The default width, in pixels, of a maximized top-level window on the primary display monitor.
			/// </summary>
			SM_CXMAXIMIZED = 61,
			/// <summary>
			/// The default maximum width of a window that has a caption and sizing borders, in pixels. This metric refers to the entire desktop. The user cannot drag the window frame to a size larger than these dimensions. A window can override this value by processing the WM_GETMINMAXINFO message.
			/// </summary>
			SM_CXMAXTRACK = 59,
			/// <summary>
			/// The width of the default menu check-mark bitmap, in pixels.
			/// </summary>
			SM_CXMENUCHECK = 71,
			/// <summary>
			/// The width of menu bar buttons, such as the child window close button that is used in the multiple document interface, in pixels.
			/// </summary>
			SM_CXMENUSIZE = 54,
			/// <summary>
			/// The minimum width of a window, in pixels.
			/// </summary>
			SM_CXMIN = 28,
			/// <summary>
			/// The width of a minimized window, in pixels.
			/// </summary>
			SM_CXMINIMIZED = 57,
			/// <summary>
			/// The width of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged. This value is always greater than or equal to SM_CXMINIMIZED.
			/// </summary>
			SM_CXMINSPACING = 47,
			/// <summary>
			/// The minimum tracking width of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions. A window can override this value by processing the WM_GETMINMAXINFO message.
			/// </summary>
			SM_CXMINTRACK = 34,
			/// <summary>
			/// The amount of border padding for captioned windows, in pixels.
			/// Windows XP/2000:  This value is not supported.
			/// </summary>
			SM_CXPADDEDBORDER = 92,
			/// <summary>
			/// The width of the screen of the primary display monitor, in pixels. This is the same value obtained by calling GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, HORZRES).
			/// </summary>
			SM_CXSCREEN = 0,
			/// <summary>
			/// The width of a button in a window caption or title bar, in pixels.
			/// </summary>
			SM_CXSIZE = 30,
			/// <summary>
			/// The thickness of the sizing border around the perimeter of a window that can be resized, in pixels. SM_CXSIZEFRAME is the width of the horizontal border, and SM_CYSIZEFRAME is the height of the vertical border.
			/// This value is the same as <see cref="SM_CXFRAME"/>.
			/// </summary>
			SM_CXSIZEFRAME = 32,
			/// <summary>
			/// The system small width of an icon, in pixels. Small icons typically appear in window captions and in small icon view. See Icon Sizes for more info.
			/// </summary>
			SM_CXSMICON = 49,
			/// <summary>
			/// The width of small caption buttons, in pixels.
			/// </summary>
			SM_CXSMSIZE = 52,
			/// <summary>
			/// The width of the virtual screen, in pixels. The virtual screen is the bounding rectangle of all display monitors. The SM_XVIRTUALSCREEN metric is the coordinates for the left side of the virtual screen.
			/// </summary>
			SM_CXVIRTUALSCREEN = 78,
			/// <summary>
			/// The width of a vertical scroll bar, in pixels.
			/// </summary>
			SM_CXVSCROLL = 2,
			/// <summary>
			/// The height of a window border, in pixels. This is equivalent to the SM_CYEDGE value for windows with the 3-D look.
			/// </summary>
			SM_CYBORDER = 6,
			/// <summary>
			/// The height of a caption area, in pixels.
			/// </summary>
			SM_CYCAPTION = 4,
			/// <summary>
			/// The nominal height of a cursor, in pixels.
			/// </summary>
			SM_CYCURSOR = 14,
			/// <summary>
			/// This value is the same as SM_CYFIXEDFRAME.
			/// </summary>
			SM_CYDLGFRAME = 8,
			/// <summary>
			/// The height of the rectangle around the location of a first click in a double-click sequence, in pixels. The second click must occur within the rectangle defined by SM_CXDOUBLECLK and SM_CYDOUBLECLK for the system to consider the two clicks a double-click. The two clicks must also occur within a specified time.
			/// To set the height of the double-click rectangle, call SystemParametersInfo with SPI_SETDOUBLECLKHEIGHT.
			/// </summary>
			SM_CYDOUBLECLK = 37,
			/// <summary>
			/// The number of pixels above and below a mouse-down point that the mouse pointer can move before a drag operation begins. This allows the user to click and release the mouse button easily without unintentionally starting a drag operation. If this value is negative, it is subtracted from above the mouse-down point and added below it.
			/// </summary>
			SM_CYDRAG = 69,
			/// <summary>
			/// The height of a 3-D border, in pixels. This is the 3-D counterpart of SM_CYBORDER.
			/// </summary>
			SM_CYEDGE = 46,
			/// <summary>
			/// The thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels. SM_CXFIXEDFRAME is the height of the horizontal border, and SM_CYFIXEDFRAME is the width of the vertical border.
			/// This value is the same as SM_CYDLGFRAME.
			/// </summary>
			SM_CYFIXEDFRAME = 8,
			/// <summary>
			/// The height of the top and bottom edges of the focus rectangle drawn by DrawFocusRect. This value is in pixels.
			/// Windows 2000:  This value is not supported.
			/// </summary>
			SM_CYFOCUSBORDER = 84,
			/// <summary>
			/// This value is the same as SM_CYSIZEFRAME.
			/// </summary>
			SM_CYFRAME = 33,
			/// <summary>
			/// The height of the client area for a full-screen window on the primary display monitor, in pixels. To get the coordinates of the portion of the screen not obscured by the system taskbar or by application desktop toolbars, call the SystemParametersInfo function with the SPI_GETWORKAREA value.
			/// </summary>
			SM_CYFULLSCREEN = 17,
			/// <summary>
			/// The height of a horizontal scroll bar, in pixels.
			/// </summary>
			SM_CYHSCROLL = 3,
			/// <summary>
			/// The system large height of an icon, in pixels. The LoadIcon function can load only icons with the dimensions that SM_CXICON and SM_CYICON specifies. See Icon Sizes for more info.
			/// </summary>
			SM_CYICON = 12,
			/// <summary>
			/// The height of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size SM_CXICONSPACING by SM_CYICONSPACING when arranged. This value is always greater than or equal to SM_CYICON.
			/// </summary>
			SM_CYICONSPACING = 39,
			/// <summary>
			/// For double byte character set versions of the system, this is the height of the Kanji window at the bottom of the screen, in pixels.
			/// </summary>
			SM_CYKANJIWINDOW = 18,
			/// <summary>
			/// The default height, in pixels, of a maximized top-level window on the primary display monitor.
			/// </summary>
			SM_CYMAXIMIZED = 62,
			/// <summary>
			/// The default maximum height of a window that has a caption and sizing borders, in pixels. This metric refers to the entire desktop. The user cannot drag the window frame to a size larger than these dimensions. A window can override this value by processing the WM_GETMINMAXINFO message.
			/// </summary>
			SM_CYMAXTRACK = 60,
			/// <summary>
			/// The height of a single-line menu bar, in pixels.
			/// </summary>
			SM_CYMENU = 15,
			/// <summary>
			/// The height of the default menu check-mark bitmap, in pixels.
			/// </summary>
			SM_CYMENUCHECK = 72,
			/// <summary>
			/// The height of menu bar buttons, such as the child window close button that is used in the multiple document interface, in pixels.
			/// </summary>
			SM_CYMENUSIZE = 55,
			/// <summary>
			/// The minimum height of a window, in pixels.
			/// </summary>
			SM_CYMIN = 29,
			/// <summary>
			/// The height of a minimized window, in pixels.
			/// </summary>
			SM_CYMINIMIZED = 58,
			/// <summary>
			/// The height of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged. This value is always greater than or equal to SM_CYMINIMIZED.
			/// </summary>
			SM_CYMINSPACING = 48,
			/// <summary>
			/// The minimum tracking height of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions. A window can override this value by processing the WM_GETMINMAXINFO message.
			/// </summary>
			SM_CYMINTRACK = 35,
			/// <summary>
			/// The height of the screen of the primary display monitor, in pixels. This is the same value obtained by calling GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, VERTRES).
			/// </summary>
			SM_CYSCREEN = 1,
			/// <summary>
			/// The height of a button in a window caption or title bar, in pixels.
			/// </summary>
			SM_CYSIZE = 31,
			/// <summary>
			/// The thickness of the sizing border around the perimeter of a window that can be resized, in pixels. SM_CXSIZEFRAME is the width of the horizontal border, and SM_CYSIZEFRAME is the height of the vertical border.
			/// This value is the same as SM_CYFRAME.
			/// </summary>
			SM_CYSIZEFRAME = 33,
			/// <summary>
			/// The height of a small caption, in pixels.
			/// </summary>
			SM_CYSMCAPTION = 51,
			/// <summary>
			/// The system small height of an icon, in pixels. Small icons typically appear in window captions and in small icon view. See Icon Sizes for more info.
			/// </summary>
			SM_CYSMICON = 50,
			/// <summary>
			/// The height of small caption buttons, in pixels.
			/// </summary>
			SM_CYSMSIZE = 53,
			/// <summary>
			/// The height of the virtual screen, in pixels. The virtual screen is the bounding rectangle of all display monitors. The SM_YVIRTUALSCREEN metric is the coordinates for the top of the virtual screen.
			/// </summary>
			SM_CYVIRTUALSCREEN = 79,
			/// <summary>
			/// The height of the arrow bitmap on a vertical scroll bar, in pixels.
			/// </summary>
			SM_CYVSCROLL = 20,
			/// <summary>
			/// The height of the thumb box in a vertical scroll bar, in pixels.
			/// </summary>
			SM_CYVTHUMB = 9,
			/// <summary>
			/// Nonzero if User32.dll supports DBCS; otherwise, 0.
			/// </summary>
			SM_DBCSENABLED = 42,
			/// <summary>
			/// Nonzero if the debug version of User.exe is installed; otherwise, 0.
			/// </summary>
			SM_DEBUG = 22,
			/// <summary>
			/// Nonzero if the current operating system is Windows 7 or Windows Server 2008 R2 and the Tablet PC Input service is started; otherwise, 0. The return value is a bitmask that specifies the type of digitizer input supported by the device. For more information, see Remarks.
			/// Windows Server 2008, Windows Vista and Windows XP/2000:  This value is not supported.
			/// </summary>
			SM_DIGITIZER = 94,
			/// <summary>
			/// Nonzero if Input Method Manager/Input Method Editor features are enabled; otherwise, 0.
			/// SM_IMMENABLED indicates whether the system is ready to use a Unicode-based IME on a Unicode application. To ensure that a language-dependent IME works, check SM_DBCSENABLED and the system ANSI code page. Otherwise the ANSI-to-Unicode conversion may not be performed correctly, or some components like fonts or registry settings may not be present.
			/// </summary>
			SM_IMMENABLED = 82,
			/// <summary>
			/// Nonzero if there are digitizers in the system; otherwise, 0.
			/// SM_MAXIMUMTOUCHES returns the aggregate maximum of the maximum number of contacts supported by every digitizer in the system. If the system has only single-touch digitizers, the return value is 1. If the system has multi-touch digitizers, the return value is the number of simultaneous contacts the hardware can provide.
			/// Windows Server 2008, Windows Vista and Windows XP/2000:  This value is not supported.
			/// </summary>
			SM_MAXIMUMTOUCHES = 95,
			/// <summary>
			/// Nonzero if the current operating system is the Windows XP, Media Center Edition, 0 if not.
			/// </summary>
			SM_MEDIACENTER = 87,
			/// <summary>
			/// Nonzero if drop-down menus are right-aligned with the corresponding menu-bar item; 0 if the menus are left-aligned.
			/// </summary>
			SM_MENUDROPALIGNMENT = 40,
			/// <summary>
			/// Nonzero if the system is enabled for Hebrew and Arabic languages, 0 if not.
			/// </summary>
			SM_MIDEASTENABLED = 74,
			/// <summary>
			/// Nonzero if a mouse is installed; otherwise, 0. This value is rarely zero, because of support for virtual mice and because some systems detect the presence of the port instead of the presence of a mouse.
			/// </summary>
			SM_MOUSEPRESENT = 19,
			/// <summary>
			/// Nonzero if a mouse with a horizontal scroll wheel is installed; otherwise 0.
			/// </summary>
			SM_MOUSEHORIZONTALWHEELPRESENT = 91,
			/// <summary>
			/// Nonzero if a mouse with a vertical scroll wheel is installed; otherwise 0.
			/// </summary>
			SM_MOUSEWHEELPRESENT = 75,
			/// <summary>
			/// The least significant bit is set if a network is present; otherwise, it is cleared. The other bits are reserved for future use.
			/// </summary>
			SM_NETWORK = 63,
			/// <summary>
			/// Nonzero if the Microsoft Windows for Pen computing extensions are installed; zero otherwise.
			/// </summary>
			SM_PENWINDOWS = 41,
			/// <summary>
			/// This system metric is used in a Terminal Services environment to determine if the current Terminal Server session is being remotely controlled. Its value is nonzero if the current session is remotely controlled; otherwise, 0.
			/// You can use terminal services management tools such as Terminal Services Manager (tsadmin.msc) and shadow.exe to control a remote session. When a session is being remotely controlled, another user can view the contents of that session and potentially interact with it.
			/// </summary>
			SM_REMOTECONTROL = 0x2001,
			/// <summary>
			/// This system metric is used in a Terminal Services environment. If the calling process is associated with a Terminal Services client session, the return value is nonzero. If the calling process is associated with the Terminal Services console session, the return value is 0. Windows Server 2003 and Windows XP:  The console session is not necessarily the physical console. For more information, see WTSGetActiveConsoleSessionId.
			/// </summary>
			SM_REMOTESESSION = 0x1000,
			/// <summary>
			/// Nonzero if all the display monitors have the same color format, otherwise, 0. Two displays can have the same bit depth, but different color formats. For example, the red, green, and blue pixels can be encoded with different numbers of bits, or those bits can be located in different places in a pixel color value.
			/// </summary>
			SM_SAMEDISPLAYFORMAT = 81,
			/// <summary>
			/// This system metric should be ignored; it always returns 0.
			/// </summary>
			SM_SECURE = 44,
			/// <summary>
			/// The build number if the system is Windows Server 2003 R2; otherwise, 0.
			/// </summary>
			SM_SERVERR2 = 89,
			/// <summary>
			/// Nonzero if the user requires an application to present information visually in situations where it would otherwise present the information only in audible form; otherwise, 0.
			/// </summary>
			SM_SHOWSOUNDS = 70,
			/// <summary>
			/// Nonzero if the current session is shutting down; otherwise, 0.
			/// Windows 2000:  This value is not supported.
			/// </summary>
			SM_SHUTTINGDOWN = 0x2000,
			/// <summary>
			/// Nonzero if the computer has a low-end (slow) processor; otherwise, 0.
			/// </summary>
			SM_SLOWMACHINE = 73,
			/// <summary>
			/// Nonzero if the current operating system is Windows 7 Starter Edition, Windows Vista Starter, or Windows XP Starter Edition; otherwise, 0.
			/// </summary>
			SM_STARTER = 88,
			/// <summary>
			/// Nonzero if the meanings of the left and right mouse buttons are swapped; otherwise, 0.
			/// </summary>
			SM_SWAPBUTTON = 23,
			/// <summary>
			/// Reflects the state of the docking mode, 0 for Undocked Mode and non-zero otherwise. When this system metric changes, the system sends a broadcast message via WM_SETTINGCHANGE with "SystemDockMode" in the LPARAM.
			/// </summary>
			SM_SYSTEMDOCKED = 0x2004,
			/// <summary>
			/// Nonzero if the current operating system is the Windows XP Tablet PC edition or if the current operating system is Windows Vista or Windows 7 and the Tablet PC Input service is started; otherwise, 0. The SM_DIGITIZER setting indicates the type of digitizer input supported by a device running Windows 7 or Windows Server 2008 R2. For more information, see Remarks.
			/// </summary>
			SM_TABLETPC = 86,
			/// <summary>
			/// The coordinates for the left side of the virtual screen. The virtual screen is the bounding rectangle of all display monitors. The SM_CXVIRTUALSCREEN metric is the width of the virtual screen.
			/// </summary>
			SM_XVIRTUALSCREEN = 76,
			/// <summary>
			/// The coordinates for the top of the virtual screen. The virtual screen is the bounding rectangle of all display monitors. The SM_CYVIRTUALSCREEN metric is the height of the virtual screen.
			/// </summary>
			SM_YVIRTUALSCREEN = 77,
		}

		[DllModuleExport(1)]
		private uint LoadIconA(uint hInstance, uint lpIconName)
		{
			_logger.LogInformation("[User32] LoadIconA: hInstance=0x{HInstance:X8} lpIconName=0x{LpIconName:X8}", hInstance, lpIconName);
			// Return a unique icon handle
			// For standard system icons (when hInstance is NULL), return predefined handles
			if (hInstance == 0 && lpIconName <= 0xFFFF)
			{
				// Standard icon IDs (IDI_APPLICATION = 32512 = 0x7F00)
				// Return a handle that includes the icon ID for debugging
				return 0x00010000 | (lpIconName & 0xFFFF);
			}
			return _env.RegisterHandle(new object()); // Custom icon object
		}

		[DllModuleExport(1)]
		private uint LoadCursorA(uint hInstance, uint lpCursorName)
		{
			_logger.LogInformation("[User32] LoadCursorA: hInstance=0x{HInstance:X8} lpCursorName=0x{LpCursorName:X8}", hInstance, lpCursorName);
			// Return a unique cursor handle
			// For standard system cursors (when hInstance is NULL), return predefined handles
			if (hInstance == 0 && lpCursorName <= 0xFFFF)
			{
				// Standard cursor IDs (IDC_ARROW = 32512 = 0x7F00)
				// Return a handle that includes the cursor ID for debugging
				return 0x00010000 | (lpCursorName & 0xFFFF);
			}
			return _env.RegisterHandle(new object()); // Custom cursor object
		}

		[DllModuleExport(1)]
		private uint SetCursor(uint hCursor)
		{
			_logger.LogInformation("[User32] SetCursor: hCursor=0x{HCursor:X8}", hCursor);
			// Store and return previous cursor handle
			var previousCursor = _currentCursor;
			_currentCursor = hCursor;
			return previousCursor;
		}

		[DllModuleExport(1, IsStub = true)]
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
			// Store and return previous focus window handle
			var previousFocus = _focusWindow;
			_focusWindow = hwnd;
			return previousFocus;
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
			
			// If a host is available, show the message box through it
			if (_host != null)
			{
				try
				{
					var msgBoxInfo = new MessageBoxInfo
					{
						ParentHandle = hwnd,
						Text = text,
						Caption = caption,
						Type = uType
					};
					
					var result = _host.OnMessageBox(msgBoxInfo);
					_logger.LogInformation("[User32] MessageBoxA: Host returned result {Result}", result);
					return (uint)result;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[User32] MessageBoxA: Exception calling host");
				}
			}
			
			// Fallback: return IDOK (1) if no host available
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

			// Load the dialog template from resources
			DialogTemplate? template = null;
			if (_resourceReader != null && _memory != null)
			{
				try
				{
					// Find the dialog resource (RT_DIALOG = 5)
					const uint RT_DIALOG = 5;
					_logger.LogInformation("[User32] DialogBoxParamAsync: Loading dialog resource from 0x{LpTemplateName:X8}", lpTemplateName);

					var hResInfo = _resourceReader.FindResource(RT_DIALOG, lpTemplateName, 0);
					if (hResInfo != 0)
					{
						_logger.LogInformation("[User32] DialogBoxParamAsync: Found dialog resource, hResInfo=0x{HResInfo:X8}", hResInfo);

						var hResData = _resourceReader.LoadResource(hInstance, hResInfo);
						if (hResData != 0)
						{
							_logger.LogInformation("[User32] DialogBoxParamAsync: Loaded dialog resource data at 0x{HResData:X8}", hResData);

							var lpData = _resourceReader.LockResource(hResData);
							if (lpData != 0)
							{
								_logger.LogInformation("[User32] DialogBoxParamAsync: Locked dialog resource at 0x{LpData:X8}", lpData);

								// Parse the dialog template
								var parser = new DialogTemplateParser(_memory);
								template = parser.Parse(lpData);

								_logger.LogInformation("[User32] DialogBoxParamAsync: Parsed dialog template - Title='{Title}', Items={ItemCount}, Size=({Width}x{Height})",
									template.Title, template.ItemCount, template.Width, template.Height);

								// Log control information
								foreach (var item in template.Items)
								{
									_logger.LogInformation("[User32] DialogBoxParamAsync: Control - ID={Id}, Class={Class}, Title='{Title}', Pos=({X},{Y}), Size=({Width}x{Height})",
										item.Id, item.WindowClass, item.Title, item.X, item.Y, item.Width, item.Height);
								}
							}
							else
							{
								_logger.LogWarning("[User32] DialogBoxParamAsync: Failed to lock dialog resource");
							}
						}
						else
						{
							_logger.LogWarning("[User32] DialogBoxParamAsync: Failed to load dialog resource");
						}
					}
					else
					{
						_logger.LogWarning("[User32] DialogBoxParamAsync: Dialog resource not found for template 0x{LpTemplateName:X8}", lpTemplateName);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[User32] DialogBoxParamAsync: Exception loading dialog template");
				}
			}
			else
			{
				_logger.LogWarning("[User32] DialogBoxParamAsync: Resource reader or memory not available");
			}

			// TODO: Show the dialog using Avalonia UI
			// For now, we'll fall back to the existing behavior but at least we've loaded and parsed the template
			if (template != null)
			{
				_logger.LogInformation("[User32] DialogBoxParamAsync: Dialog template loaded successfully but Avalonia UI integration not yet implemented");
				_logger.LogInformation("[User32] DialogBoxParamAsync: Would create dialog window with title '{Title}' and {ItemCount} controls", template.Title, template.ItemCount);

				// Create a dialog window handle
				var hDlg = _env.RegisterHandle(new object()); // Dialog handle
				_logger.LogInformation("[User32] DialogBoxParamAsync: Created dialog handle=0x{HDlg:X8}", hDlg);

				// Initialize dialog state for proper message loop handling
				_env.InitializeDialogState(hDlg);

				// If we have a host, show the dialog through Avalonia (non-blocking)
				if (_host != null)
				{
					_logger.LogInformation("[User32] DialogBoxParamAsync: Showing dialog through Avalonia UI with message loop");

					try
					{
						var dialogInfo = new DialogCreateInfo
						{
							Handle = hDlg,
							Template = template,
							ParentHandle = hWndParent,
							DialogProcAddress = lpDialogFunc,
							InitParam = dwInitParam
						};

						// Show the dialog non-blocking
						// OnDialogCreate will create and show the window, then return immediately
						// The window will stay open while we process messages below
						_ = _host.OnDialogCreate(dialogInfo);
						
						_logger.LogInformation("[User32] DialogBoxParamAsync: Dialog window shown, proceeding to message loop");
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "[User32] DialogBoxParamAsync: Exception showing dialog through host");
						_env.CleanupDialogState(hDlg);
						_env.CloseHandle(hDlg);
						return 0;
					}
				}
				else
				{
					_logger.LogWarning("[User32] DialogBoxParamAsync: No host available to show Avalonia dialog");
				}

				// Call the dialog procedure with WM_INITDIALOG (continues for both Avalonia and non-Avalonia paths)
				
				// Call the dialog procedure with WM_INITDIALOG (0x0110)
				// WM_INITDIALOG signature: BOOL CALLBACK DialogProc(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
				// wParam = hWndParent (or 0 if no focus control)
				// lParam = dwInitParam
				const uint WM_INITDIALOG = 0x0110;
				var dialogProcTimedOut = false;
				var dialogProcCancelled = false;
				var dialogProcFailed = false;

				if (lpDialogFunc != 0)
				{
					_logger.LogInformation("[User32] DialogBoxParamAsync: Calling dialog procedure with WM_INITDIALOG");
					var (initResult, timedOut, cancelled, failed) = await CallDialogProcedureAsync(_cpu!, _memory!, lpDialogFunc, hDlg, WM_INITDIALOG, 0, dwInitParam, cancellationToken);
					_logger.LogInformation("[User32] DialogBoxParamAsync: WM_INITDIALOG returned {InitResult}", initResult);
					dialogProcTimedOut = timedOut;
					dialogProcCancelled = cancelled;
					dialogProcFailed = failed;
				}
				else
				{
					_logger.LogWarning("[User32] DialogBoxParamAsync: No dialog procedure specified");
				}

				// If the dialog procedure timed out, was cancelled, or failed during initialization, end the dialog immediately
				if (dialogProcTimedOut || dialogProcCancelled || dialogProcFailed)
				{
					var status = dialogProcFailed ? "failed" : (dialogProcCancelled ? "cancelled" : "timed out");
					_logger.LogWarning("[User32] DialogBoxParamAsync: Dialog procedure {Status}, ending dialog with result 0", status);
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
							var (result, timedOut, cancelled, failed) = await CallDialogProcedureAsync(_cpu!, _memory!, lpDialogFunc, hDlg, msg.Message, msg.WParam, msg.LParam, cancellationToken);
							_logger.LogDebug("[User32] DialogBoxParamAsync: Dialog procedure returned {Result} for MSG=0x{Message:X4}", result, msg.Message);

							// If dialog procedure times out, is cancelled, or fails, force end the dialog
							if (timedOut || cancelled || failed)
							{
								var status = failed ? "failed" : (cancelled ? "cancelled" : "timed out");
								_logger.LogWarning("[User32] DialogBoxParamAsync: Dialog procedure {Status} during message processing, forcing dialog end", status);
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

					// If we've had too many empty iterations and the dialog proc timed out or failed, force end
					if ((dialogProcTimedOut || dialogProcFailed) && consecutiveEmptyIterations >= MAX_EMPTY_ITERATIONS)
					{
						var status = dialogProcFailed ? "failed" : "timed out";
						_logger.LogWarning("[User32] DialogBoxParamAsync: No messages and dialog procedure {Status}, forcing dialog end", status);
						_env.SetDialogResult(hDlg, 0);
					}

					// Yield to avoid tight loop without introducing artificial delay
					await Task.Yield();
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
		else
		{
			_logger.LogWarning("[User32] DialogBoxParamAsync: Failed to load dialog template");
			return 0;
		}
		}

		/// <summary>
		/// Call a dialog procedure by setting up CPU state and executing the callback.
		/// DialogProc signature: INT_PTR CALLBACK DialogProc(HWND hwndDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
		/// Uses stdcall calling convention (callee cleans stack, parameters pushed right-to-left)
		/// Returns a tuple of (returnValue, timedOut) where timedOut indicates if the procedure exceeded max steps.
		/// </summary>
		private (uint returnValue, bool timedOut, bool failed) CallDialogProcedureWithTimeout(ICpu cpu, VirtualMemory memory, uint dialogProcAddress, uint hwndDlg, uint message, uint wParam, uint lParam)
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
			var failed = false;
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

					// Check for invalid EIP (NULL pointer execution)
					if (eip == 0x00000000)
					{
						_logger.LogError("[User32] CallDialogProcedure: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting");
						failed = true;
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
						else
						{
							// Import function not implemented - try to get arg bytes from metadata and simulate return
							var simulatedArgBytes = 0;
							try
							{
								simulatedArgBytes = StdCallMeta.GetArgBytes(dll, name);
								_logger.LogWarning("[User32] CallDialogProcedure: Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes={ArgBytes}", dll, name, simulatedArgBytes);
							}
							catch
							{
								_logger.LogWarning("[User32] CallDialogProcedure: Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes unknown (assuming 0)", dll, name);
							}
							
							var currentEsp = cpu.GetRegister("ESP");
							var retEip = memory.Read32(currentEsp);
							
							// Pop return address + parameters (stdcall convention - callee cleans)
							currentEsp += 4 + (uint)simulatedArgBytes;
							
							cpu.SetRegister("ESP", currentEsp);
							cpu.SetRegister("EAX", 0); // Return 0 as default
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
				_logger.LogWarning(ex, "[User32] CallDialogProcedure: Exception during execution: {ExMessage}", ex.Message);
				failed = true;
			}

			if (steps >= MAX_STEPS)
			{
				_logger.LogWarning("[User32] CallDialogProcedure: Exceeded max steps ({MaxSteps}), aborting - DialogProc may be in infinite loop", MAX_STEPS);
				timedOut = true;
			}

			// Get return value from EAX, but only if execution was successful
			// Otherwise return 0 as a safe default value
			var returnValue = (timedOut || failed) ? 0u : cpu.GetRegister("EAX");

			// Restore CPU state
			cpu.SetEip(savedEip);
			cpu.SetRegister("ESP", savedEsp);
			cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] CallDialogProcedure: Completed with return value 0x{ReturnValue:X8}, timedOut={TimedOut}, failed={Failed}",
				returnValue, timedOut, failed);

			return (returnValue, timedOut, failed);
		}

		/// <summary>
		/// Async version of CallDialogProcedureWithTimeout with cancellation token support.
		/// Allows cooperative cancellation during long-running dialog procedure execution.
		/// </summary>
		private async Task<(uint returnValue, bool timedOut, bool cancelled, bool failed)> CallDialogProcedureAsync(
			ICpu cpu, VirtualMemory memory, uint dialogProcAddress, uint hwndDlg, uint message, uint wParam, uint lParam, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("[User32] CallDialogProcedureAsync: Calling 0x{DialogProcAddress:X8} with HWND=0x{HwndDlg:X8} MSG=0x{Message:X4}", dialogProcAddress, hwndDlg, message);

			// Validate dialog procedure address - reject NULL or obviously invalid addresses
			if (dialogProcAddress == 0)
			{
				_logger.LogWarning("[User32] CallDialogProcedureAsync: Dialog procedure address is NULL (0x00000000), aborting");
				return (0, true, false, true); // Mark as failed since the procedure address is invalid
			}

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
			var failed = false;
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

					// Log first 20 instructions to help debug if we jump to NULL
					if (steps < 20)
					{
						_logger.LogInformation("[User32] CallDialogProcedureAsync: Step {Steps}: EIP=0x{Eip:X8}", steps, eip);
					}

					// Check if we've returned to our marker address
					if (eip == RETURN_ADDRESS)
					{
						break;
					}

					// Check for invalid EIP (NULL pointer execution)
					if (eip == 0x00000000)
					{
						_logger.LogError("[User32] CallDialogProcedureAsync: Execution jumped to NULL address (0x00000000) at step {Steps}", steps);
						_logger.LogError("[User32] CallDialogProcedureAsync: This typically means the code called a NULL function pointer");
						_logger.LogError("[User32] CallDialogProcedureAsync: ESP=0x{Esp:X8} EBP=0x{Ebp:X8}",
							cpu.GetRegister("ESP"), cpu.GetRegister("EBP"));
						// Log stack contents
						try
						{
							var stackPtr = cpu.GetRegister("ESP");
							_logger.LogError("[User32] CallDialogProcedureAsync: Stack: {Stack}",
								string.Join(" ", Enumerable.Range(0, 8).Select(i => $"0x{memory.Read32(stackPtr + (uint)(i * 4)):X8}")));
						}
						catch
						{
						}

						failed = true;
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
						_logger.LogInformation("[User32] CallDialogProcedureAsync: COM vtable call at 0x{CallTarget:X8}", step.CallTarget);

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
						_logger.LogInformation("[User32] CallDialogProcedureAsync: Import call {Dll}!{Name} at 0x{CallTarget:X8}", dll, name, step.CallTarget);

						// Save callee-saved registers (EBX, ESI, EDI)
						var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);

						if (_dispatcher != null && _dispatcher.TryInvoke(dll, name, cpu, memory, out var ret, out var argBytes))
						{
							_logger.LogInformation("[User32] CallDialogProcedureAsync: Import {Dll}!{Name} returned 0x{Ret:X8}", dll, name, ret);
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
						else
						{
							// Import function not implemented - try to get arg bytes from metadata and simulate return
							var simulatedArgBytes = 0;
							try
							{
								simulatedArgBytes = StdCallMeta.GetArgBytes(dll, name);
								_logger.LogWarning("[User32] CallDialogProcedureAsync: Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes={ArgBytes}", dll, name, simulatedArgBytes);
							}
							catch
							{
								_logger.LogWarning("[User32] CallDialogProcedureAsync: Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes unknown (assuming 0)", dll, name);
							}
							
							var currentEsp = cpu.GetRegister("ESP");
							var retEip = memory.Read32(currentEsp);
							
							// Pop return address + parameters (stdcall convention - callee cleans)
							currentEsp += 4 + (uint)simulatedArgBytes;
							
							cpu.SetRegister("ESP", currentEsp);
							cpu.SetRegister("EAX", 0); // Return 0 as default
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
								_logger.LogInformation("[User32] CallDialogProcedureAsync: Cooperative yield at {Steps} steps", steps);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "[User32] CallDialogProcedureAsync: Exception during execution: {ExMessage}", ex.Message);
				failed = true;
			}

			if (steps >= MAX_STEPS)
			{
				_logger.LogWarning("[User32] CallDialogProcedureAsync: Exceeded max steps ({MaxSteps}), aborting - DialogProc may be in infinite loop", MAX_STEPS);
				timedOut = true;
			}

			// Get return value from EAX, but only if execution was successful
			// Otherwise return 0 as a safe default value
			var returnValue = (timedOut || cancelled || failed) ? 0u : cpu.GetRegister("EAX");

			// Restore CPU state
			cpu.SetEip(savedEip);
			cpu.SetRegister("ESP", savedEsp);
			cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] CallDialogProcedureAsync: Completed with return value 0x{ReturnValue:X8}, timedOut={TimedOut}, cancelled={Cancelled}, failed={Failed}",
				returnValue, timedOut, cancelled, failed);

			return (returnValue, timedOut, cancelled, failed);
		}


		[DllModuleExport(1)]
		private uint EndDialog(uint hDlg, uint nResult)
		{
			// EndDialog closes a modal dialog box and sets its result
			_logger.LogInformation("[User32] EndDialog: hDlg=0x{HDlg:X8} nResult={NResult}", hDlg, nResult);

			// Set the dialog result in the process environment
			// This will signal DialogBoxParamA to exit its message loop
			var success = _env.SetDialogResult(hDlg, nResult);

			// If we have a host, close the Avalonia dialog window
			if (_host != null && success)
			{
				try
				{
					// Notify host to close the dialog window
					_host.OnDialogEnd(hDlg, (int)nResult);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[User32] EndDialog: Exception closing Avalonia dialog");
				}
			}

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
				var stackBottom = (esp > STACK_SIZE) ? (esp - STACK_SIZE) : 0x00100000; // Don't go below 1MB

				var inStackRegion = (ebpFromStack >= stackBottom) && (ebpFromStack <= esp);
				var isAligned = (ebpFromStack & 0x3) == 0;

				// Optionally, check that the memory at ebpFromStack is readable and contains a plausible saved EBP
				var savedEbpValid = false;
				if (inStackRegion && isAligned)
				{
					try
					{
						var savedEbp = memory.Read32(ebpFromStack);
						// Check that savedEbp is also within stack region (optional, but plausible)
						savedEbpValid = (savedEbp >= stackBottom) && (savedEbp <= esp);
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
					_logger.LogDebug("[User32] Skipped restoring EBP from stack: 0x{EBP:X8} (not a valid frame pointer)", ebpFromStack);
				}
			}
			catch (Exception ex)
			{
				_logger.LogDebug(ex, "[User32] Failed to restore EBP from stack");
			}
		}

	[DllModuleExport(8)]
	private uint ExitWindowsEx(uint uFlags, uint dwReason)
	{
		_logger.LogInformation("[User32] ExitWindowsEx(uFlags=0x{UFlags:X}, dwReason=0x{DwReason:X})", uFlags, dwReason);
		// Stub - just log the call, don't actually shut down
		return 1; // TRUE
	}

	[DllModuleExport(12)]
	private uint GetWindowTextA(uint hWnd, in LpStr lpString, int nMaxCount)
	{
		_logger.LogInformation("[User32] GetWindowTextA(hWnd=0x{HWnd:X8}, lpString=0x{LpString:X8}, nMaxCount={NMaxCount})", hWnd, lpString.Address, nMaxCount);

		// Stub - return empty string
		var title = string.Empty;
		
		// Write to buffer
		if (nMaxCount > 0)
		{
			lpString.Write(_env.Memory, title, true);
		}

		return (uint)title.Length;
	}

	[DllModuleExport(8)]
	private uint SetWindowTextA(uint hWnd, in LpcStr lpString)
	{
		var text = lpString.ToString() ?? string.Empty;
		_logger.LogInformation("[User32] SetWindowTextA(hWnd=0x{HWnd:X8}, lpString=\"{Text}\")", hWnd, text);

		// Stub - just log the operation
		return 1; // TRUE
	}

	[DllModuleExport(20)]
	private uint LoadImageA(uint hInst, in LpStr name, uint type, int cx, int cy, uint fuLoad)
	{
		var imageName = name.Read(_env.Memory);
		_logger.LogInformation("[User32] LoadImageA(hInst=0x{HInst:X8}, name=\"{ImageName}\", type={Type}, cx={Cx}, cy={Cy}, fuLoad=0x{FuLoad:X})", 
			hInst, imageName, type, cx, cy, fuLoad);

		// Stub - return a dummy handle
		// Type: 0=IMAGE_BITMAP, 1=IMAGE_ICON, 2=IMAGE_CURSOR
		var handle = 0x90000000 + (uint)imageName.GetHashCode();
		_logger.LogInformation("[User32] LoadImageA: Returning stub handle 0x{Handle:X8}", handle);
		return handle;
	}

	[DllModuleExport(16)]
	private uint LoadStringA(uint hInstance, uint uID, in LpStr lpBuffer, int cchBufferMax)
	{
		_logger.LogInformation("[User32] LoadStringA(hInstance=0x{HInstance:X8}, uID={UID}, lpBuffer=0x{LpBuffer:X8}, cchBufferMax={CchBufferMax})", 
			hInstance, uID, lpBuffer.Address, cchBufferMax);

		// Stub - string resources not yet implemented, return empty
		if (cchBufferMax > 0)
		{
			lpBuffer.Write(_env.Memory, string.Empty, true);
		}
		return 0;
	}

	[DllModuleExport(20)]
	private uint WsprintfA(in LpStr output, in LpcStr format, StackArgs args)
	{
		var formatStr = format.ToString() ?? string.Empty;
		_logger.LogInformation("[User32] WsprintfA(output=0x{Output:X8}, format=\"{FormatStr}\")", output.Address, formatStr);

		// Simple sprintf implementation - just copy format string for now
		// A full implementation would parse format string and substitute arguments
		output.Write(_env.Memory, formatStr, true);
		
		return (uint)formatStr.Length;
	}

	[DllModuleExport(12)]
	private uint WvsprintfA(in LpStr output, in LpcStr format, uint arglist)
	{
		var formatStr = format.ToString() ?? string.Empty;
		_logger.LogInformation("[User32] WvsprintfA(output=0x{Output:X8}, format=\"{FormatStr}\", arglist=0x{Arglist:X8})", 
			output.Address, formatStr, arglist);

		// Simple vsprintf implementation - just copy format string for now
		// A full implementation would parse format string and substitute arguments from va_list
		output.Write(_env.Memory, formatStr, true);
		
		return (uint)formatStr.Length;
	}

	}
}