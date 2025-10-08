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

		public User32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
			_standardControlHandler = new StandardControlHandler(env, null, _logger);
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
					returnValue = DispatchMessageAInternal(a.UInt32(0), cpu, memory);
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
					returnValue = SendMessageA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), cpu, memory);
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

				case "SENDDLGITEMMESSAGEA":
					returnValue = SendDlgItemMessageA(a.UInt32(0), a.Int32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				case "ENABLEWINDOW":
					returnValue = EnableWindow(a.UInt32(0), a.UInt32(1));
					return true;

				default:
					_logger.LogInformation("[User32] Unimplemented export: {Export}", export);
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

		[DllModuleExport(28)]
		private unsafe uint ShowWindow(uint hwnd, int nCmdShow)
		{
			// SW_HIDE = 0, SW_NORMAL = 1, SW_SHOWMINIMIZED = 2, SW_SHOWMAXIMIZED = 3, etc.
			_logger.LogInformation("[User32] ShowWindow: HWND=0x{Hwnd:X8} nCmdShow={NCmdShow}", hwnd, nCmdShow);

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
			var queuedMsg = _env.GetMessageBlocking(hWnd, wMsgFilterMin, wMsgFilterMax, timeoutMs: 100);
			if (queuedMsg.HasValue)
			{
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

			// No messages available after timeout - return WM_NULL
			// Note: Real Windows GetMessage would block indefinitely, but we timeout to avoid hanging
			_logger.LogInformation("[User32] GetMessageA: No messages after timeout, returning WM_NULL");

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
			return DispatchMessageAInternal(lpMsg, null, null);
		}

		[DllModuleExport(1)]
		private unsafe uint DispatchMessageAInternal(uint lpMsg, ICpu? cpu, VirtualMemory? memory)
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

				// If CPU is available, call the window procedure
				if (cpu != null && memory != null)
				{
					var result = CallWindowProcedure(cpu, memory, wndProc.Value, hwnd, message, wParam, lParam);
					_logger.LogInformation("[User32] DispatchMessageA: WndProc returned 0x{Result:X8}", result);
					return result;
				}
				else
				{
					_logger.LogInformation($"[User32] DispatchMessageA: CPU not available, cannot call WndProc");
				}
			}
			else
			{
				_logger.LogInformation("[User32] DispatchMessageA: No WndProc found for HWND=0x{Hwnd:X8}", hwnd);
			}

			// For now, just return 0 (message processed)
			return 0;
		}

		[DllModuleExport(1)]
		private unsafe uint DefWindowProcA(uint hwnd, uint msg, uint wParam, uint lParam)
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
		private unsafe void PostQuitMessage(int nExitCode)
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
			const int MAX_STEPS = 100000; // Safety limit
			int steps = 0;

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

					// Execute one instruction
					cpu.SingleStep(memory);

					steps++;
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning("[User32] CallWindowProcedure: Exception during execution: {ExMessage}", ex.Message);
			}

			if (steps >= MAX_STEPS)
			{
				_logger.LogWarning("[User32] CallWindowProcedure: Exceeded max steps ({MaxSteps}), aborting", MAX_STEPS);
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
		
		[DllModuleExport(1)]
		private unsafe uint SendMessageA(uint hwnd, uint msg, uint wParam, uint lParam, ICpu? cpu, VirtualMemory? memory)
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

				// If CPU is available, call the window procedure
				if (cpu != null && memory != null)
				{
					var result = CallWindowProcedure(cpu, memory, wndProc.Value, hwnd, msg, wParam, lParam);
					_logger.LogInformation("[User32] SendMessageA: WndProc returned 0x{Result:X8}", result);
					return result;
				}
				else
				{
					_logger.LogInformation($"[User32] SendMessageA: CPU not available, cannot call WndProc");
				}
			}
			else
			{
				_logger.LogInformation("[User32] SendMessageA: No WndProc found for HWND=0x{Hwnd:X8}", hwnd);
			}

			// For now, return 0 (message processed)
			return 0;
		}

		[DllModuleExport(1)]
		private unsafe uint ClientToScreen(uint hwnd, uint lpPoint)
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
		private unsafe uint SetRect(uint lpRect, int left, int top, int right, int bottom)
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
		private unsafe uint GetClientRect(uint hwnd, uint lpRect)
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
		private unsafe uint GetWindowRect(uint hwnd, uint lpRect)
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
		private unsafe uint GetDc(uint hwnd)
		{
			// Create a device context handle
			var hdc = _env.RegisterHandle(new object()); // Dummy DC object
			_logger.LogInformation("[User32] GetDC: HWND=0x{Hwnd:X8} -> HDC=0x{Hdc:X8}", hwnd, hdc);
			return hdc;
		}

		[DllModuleExport(1)]
		private unsafe uint ReleaseDc(uint hwnd, uint hdc)
		{
			_logger.LogInformation("[User32] ReleaseDC: HWND=0x{Hwnd:X8} HDC=0x{Hdc:X8}", hwnd, hdc);
			_env.CloseHandle(hdc);
			return 1; // Success
		}

		[DllModuleExport(1)]
		private unsafe uint UpdateWindow(uint hwnd)
		{
			_logger.LogInformation("[User32] UpdateWindow: HWND=0x{Hwnd:X8}", hwnd);
			// Trigger immediate repaint - for now just log
			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private unsafe uint DestroyWindow(uint hwnd)
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
		private unsafe uint SetWindowPos(uint hwnd, uint hwndInsertAfter, int x, int y, int cx, int cy, uint flags)
		{
			_logger.LogInformation("[User32] SetWindowPos: HWND=0x{Hwnd:X8} pos=({I},{I1}) size=({Cx},{Cy}) flags=0x{Flags:X8}", hwnd, x, y, cx, cy, flags);
			// For now just log
			return 1; // TRUE
		}

		[DllModuleExport(11)]
		private unsafe int GetSystemMetrics(int nIndex)
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
		private unsafe uint LoadIconA(uint hInstance, uint lpIconName)
		{
			_logger.LogInformation("[User32] LoadIconA: hInstance=0x{HInstance:X8} lpIconName=0x{LpIconName:X8}", hInstance, lpIconName);
			// Return a dummy icon handle
			return _env.RegisterHandle(new object()); // Dummy icon object
		}

		[DllModuleExport(1)]
		private unsafe uint LoadCursorA(uint hInstance, uint lpCursorName)
		{
			_logger.LogInformation("[User32] LoadCursorA: hInstance=0x{HInstance:X8} lpCursorName=0x{LpCursorName:X8}", hInstance, lpCursorName);
			// Return a dummy cursor handle
			return _env.RegisterHandle(new object()); // Dummy cursor object
		}

		[DllModuleExport(1)]
		private unsafe uint SetCursor(uint hCursor)
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
		private unsafe uint SetFocus(uint hwnd)
		{
			_logger.LogInformation("[User32] SetFocus: HWND=0x{Hwnd:X8}", hwnd);
			// Return previous focus window handle
			return 0; // NULL means no previous focus
		}

		[DllModuleExport(1)]
		private unsafe uint GetMenu(uint hwnd)
		{
			_logger.LogInformation("[User32] GetMenu: HWND=0x{Hwnd:X8}", hwnd);
			// Return menu handle (NULL if no menu)
			return 0;
		}

		[DllModuleExport(1)]
		private unsafe uint SetWindowLongA(uint hwnd, int nIndex, uint dwNewLong)
		{
			_logger.LogInformation("[User32] SetWindowLongA: HWND=0x{Hwnd:X8} nIndex={NIndex} dwNewLong=0x{DwNewLong:X8}", hwnd, nIndex, dwNewLong);
			// Return previous value (for now return 0)
			return 0;
		}

		[DllModuleExport(1)]
		private unsafe uint GetWindowLongA(uint hwnd, int nIndex)
		{
			_logger.LogInformation("[User32] GetWindowLongA: HWND=0x{Hwnd:X8} nIndex={NIndex}", hwnd, nIndex);
			// Return window data (for now return 0)
			return 0;
		}

		[DllModuleExport(1)]
		private unsafe uint MessageBoxA(uint hwnd, uint lpText, uint lpCaption, uint uType)
		{
			var text = lpText != 0 ? _env.ReadAnsiString(lpText) : "";
			var caption = lpCaption != 0 ? _env.ReadAnsiString(lpCaption) : "";
			_logger.LogInformation("[User32] MessageBoxA: \"{Caption}\" - \"{Text}\" type=0x{UType:X8}", caption, text, uType);
			// Return IDOK (1)
			return 1;
		}

		[DllModuleExport(1)]
		private unsafe uint SystemParametersInfoA(uint uiAction, uint uiParam, uint pvParam, uint fWinIni)
		{
			_logger.LogInformation("[User32] SystemParametersInfoA: action=0x{UiAction:X8} param={UiParam}", uiAction, uiParam);
			// For now just return success
			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private unsafe uint PeekMessageA(uint lpMsg, uint hwnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
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
		private unsafe uint PostMessageA(uint hwnd, uint msg, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] PostMessageA: HWND=0x{Hwnd:X8} MSG=0x{Msg:X4} wParam=0x{WParam:X8} lParam=0x{LParam:X8}", hwnd, msg, wParam, lParam);

			// Post message to the queue
			var success = _env.PostMessage(hwnd, msg, wParam, lParam);
			return success ? 1u : 0u; // TRUE : FALSE
		}

		[DllModuleExport(1)]
		private unsafe uint DialogBoxParamA(uint hInstance, uint lpTemplateName, uint hWndParent, uint lpDialogFunc, uint dwInitParam)
		{
			// DialogBoxParamA creates a modal dialog box
			// For now, we'll just log and return a default value
			_logger.LogInformation("[User32] DialogBoxParamA: hInstance=0x{HInstance:X8} lpTemplateName=0x{LpTemplateName:X8} lpDialogFunc=0x{LpDialogFunc:X8}", hInstance, lpTemplateName, lpDialogFunc);

			// Return IDOK (1) to indicate the dialog was closed with OK
			return 1;
		}

		[DllModuleExport(1)]
		private unsafe uint EndDialog(uint hDlg, uint nResult)
		{
			// EndDialog closes a modal dialog box
			_logger.LogInformation("[User32] EndDialog: hDlg=0x{HDlg:X8} nResult={NResult}", hDlg, nResult);
			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private unsafe uint GetDlgItem(uint hDlg, int nIDDlgItem)
		{
			// GetDlgItem retrieves a handle to a control in a dialog box
			_logger.LogInformation("[User32] GetDlgItem: hDlg=0x{HDlg:X8} nIDDlgItem={NIdDlgItem}", hDlg, nIDDlgItem);

			// Return a synthetic handle (dialog handle + control ID)
			return hDlg + (uint)nIDDlgItem;
		}

		[DllModuleExport(1)]
		private unsafe uint GetDlgItemTextA(uint hDlg, int nIDDlgItem, uint lpString, int cchMax)
		{
			// GetDlgItemTextA retrieves the text of a control in a dialog box
			_logger.LogInformation("[User32] GetDlgItemTextA: hDlg=0x{HDlg:X8} nIDDlgItem={NIdDlgItem} cchMax={CchMax}", hDlg, nIDDlgItem, cchMax);

			if (lpString == 0 || cchMax <= 0)
			{
				return 0;
			}

			// Return empty string for now
			_env.MemWriteBytes(lpString, new byte[] { 0 });
			return 0;
		}

		[DllModuleExport(1)]
		private unsafe uint SendDlgItemMessageA(uint hDlg, int nIDDlgItem, uint msg, uint wParam, uint lParam)
		{
			// SendDlgItemMessageA sends a message to a control in a dialog box
			_logger.LogInformation("[User32] SendDlgItemMessageA: hDlg=0x{HDlg:X8} nIDDlgItem={NIdDlgItem} msg=0x{Msg:X4}", hDlg, nIDDlgItem, msg);

			// Return 0 (default message handling result)
			return 0;
		}

		[DllModuleExport(1)]
		private unsafe uint EnableWindow(uint hwnd, uint bEnable)
		{
			// EnableWindow enables or disables mouse and keyboard input to a window
			// Returns the previous enable state: nonzero if previously disabled, zero if previously enabled
			_logger.LogInformation("[User32] EnableWindow: HWND=0x{Hwnd:X8} bEnable={BEnable}", hwnd, bEnable);

			// Get the previous state (default to enabled if not tracked)
			bool wasEnabled = _windowEnabledState.GetValueOrDefault(hwnd, true);

			// Update the state
			_windowEnabledState[hwnd] = bEnable != 0;

			// Return previous state: return 0 if was enabled, non-zero if was disabled
			return wasEnabled ? 0u : 1u;
		}
	}
}