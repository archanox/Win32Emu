using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	internal class User32Module : IWin32ModuleAsync
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

		// ATOM generation counter for window classes
		private uint _nextAtom = 0xC000; // Start at 0xC000 (standard user atom range)

		// State tracking for cursor and focus
		private uint _currentCursor;
		private uint _focusWindow;
		private int _cursorDisplayCount = 1; // Tracks cursor visibility counter (starts visible in Windows)

		// Counter for generating unique bitmap handles
		private uint _nextBitmapHandle = 0;

		// Counter for generating unique menu handles
		private uint _nextMenuHandle = 0;

		// Timer tracking for SetTimer implementation
		private readonly ConcurrentDictionary<uint, TimerInfo> _timers = new();
		private uint _nextTimerId = 1;

		// Hook tracking for SetWindowsHookEx implementation
		private readonly ConcurrentDictionary<uint, HookInfo> _hooks = new();
		private uint _nextHookHandle = 0x00010001;

		// Timer information structure
		private record struct TimerInfo(
			uint TimerId,
			uint HWnd,
			uint Elapse,
			uint TimerProc
		);

		// Hook information structure
		private record struct HookInfo(
			uint HookHandle,
			int IdHook,
			uint HookProc,
			uint HMod,
			uint ThreadId
		);

		// Constants for procedure execution monitoring
		private const int INFINITE_LOOP_CHECK_INTERVAL = 100000; // Check for infinite loops every 100K steps
		private const int STUCK_COUNTER_THRESHOLD = 3; // Number of consecutive checks at same EIP to consider it stuck
		private const int CANCELLATION_CHECK_INTERVAL = 1000; // Check cancellation token every 1K steps
		private const uint MINIMUM_VALID_EIP = 0x00001000; // Minimum valid instruction pointer (4KB) - addresses below this indicate memory corruption
		
		// Resource ID constants
		// In Win32 API, resource IDs can be either integers or string pointers.
		// Integer resource IDs are stored as values < 0x10000 (65536), while string pointers are >= 0x10000.
		// This follows the Windows IS_INTRESOURCE macro convention.
		private const uint MAX_INTRESOURCE = 0x10000; // Maximum value for integer resource IDs (65536)

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

				case "REGISTERCLASSEXA":
					returnValue = RegisterClassExA(a.UInt32(0));
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

				case "WAITMESSAGE":
					returnValue = WaitMessage();
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

				case "OFFSETRECT":
					returnValue = OffsetRect(a.UInt32(0), a.Int32(1), a.Int32(2));
					return true;

				case "INFLATERECT":
					returnValue = InflateRect(a.UInt32(0), a.Int32(1), a.Int32(2));
					return true;

				case "INVALIDATERECT":
					returnValue = InvalidateRect(a.UInt32(0), a.UInt32(1), a.UInt32(2));
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
					returnValue = (uint)GetSystemMetrics((SystemMetric)a.Int32(0));
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

				case "SETCURSORPOS":
					returnValue = SetCursorPos(a.Int32(0), a.Int32(1));
					return true;

				case "CLIPCURSOR":
					returnValue = ClipCursor(a.UInt32(0));
					return true;

				case "SETTIMER":
					returnValue = SetTimer(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "SETFOCUS":
					returnValue = SetFocus(a.UInt32(0));
					return true;

				case "GETFOCUS":
					returnValue = GetFocus();
					return true;

				case "GETMENU":
					returnValue = GetMenu(a.UInt32(0));
					return true;

				case "CREATEPOPUPMENU":
					returnValue = CreatePopupMenu();
					return true;

				case "APPENDMENUA":
					returnValue = AppendMenuA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "TRACKPOPUPMENU":
					returnValue = TrackPopupMenu(a.UInt32(0), a.UInt32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.UInt32(5), a.UInt32(6));
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

				case "GETDLGITEMINT":
					returnValue = GetDlgItemInt(a.UInt32(0), a.Int32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "SETDLGITEMINT":
					returnValue = SetDlgItemInt(a.UInt32(0), a.Int32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "ISDLGBUTTONCHECKED":
					returnValue = IsDlgButtonChecked(a.UInt32(0), a.Int32(1));
					return true;

				case "CHECKRADIOBUTTON":
					returnValue = CheckRadioButton(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3));
					return true;

				case "CREATEDIALOGPARAMA":
					returnValue = CreateDialogParamA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
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

				case "CHARPREVA":
					returnValue = CharPrevA(a.UInt32(0), a.UInt32(1));
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
				case "GETDESKTOPWINDOW":
					returnValue = GetDesktopWindow();
					return true;
				case "CHECKDLGBUTTON":
					returnValue = CheckDlgButton(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;
				case "MSGWAITFORMULTIPLEOBJECTS":
					returnValue = MsgWaitForMultipleObjects(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				case "CHARLOWERBUFFA":
					returnValue = CharLowerBuffA(a.LpStr(0), a.UInt32(1));
					return true;

				case "GETKEYBOARDTYPE":
					returnValue = GetKeyboardType(a.Int32(0));
					return true;

				case "ENUMDISPLAYSETTINGSA":
					returnValue = EnumDisplaySettingsA(a.LpcStr(0), a.UInt32(1), a.UInt32(2));
					return true;

				// Additional window management functions
				case "BEGINDEFERWINDOWPOS":
					returnValue = BeginDeferWindowPos(a.Int32(0));
					return true;
				case "DEFERWINDOWPOS":
					returnValue = DeferWindowPos(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.Int32(4), a.Int32(5), a.Int32(6), a.UInt32(7));
					return true;
				case "ENDDEFERWINDOWPOS":
					returnValue = EndDeferWindowPos(a.UInt32(0));
					return true;
				case "BRINGWINDOWTOTOP":
					returnValue = BringWindowToTop(a.UInt32(0));
					return true;
				case "GETACTIVEWINDOW":
					returnValue = GetActiveWindow();
					return true;
				case "SETACTIVEWINDOW":
					returnValue = SetActiveWindow(a.UInt32(0));
					return true;
				case "GETFOREGROUNDWINDOW":
					returnValue = GetForegroundWindow();
					return true;
				case "SETFOREGROUNDWINDOW":
					returnValue = SetForegroundWindow(a.UInt32(0));
					return true;
				case "FINDWINDOWA":
					returnValue = FindWindowA(a.LpcStr(0), a.LpcStr(1));
					return true;
				case "ENUMWINDOWS":
					returnValue = EnumWindows(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETPARENT":
					returnValue = GetParent(a.UInt32(0));
					return true;
				case "GETTOPWINDOW":
					returnValue = GetTopWindow(a.UInt32(0));
					return true;
				case "GETWINDOW":
					returnValue = GetWindow(a.UInt32(0), a.UInt32(1));
					return true;
				case "ISCHILD":
					returnValue = IsChild(a.UInt32(0), a.UInt32(1));
					return true;
				case "ISWINDOW":
					returnValue = IsWindow(a.UInt32(0));
					return true;
				case "ISWINDOWENABLED":
					returnValue = IsWindowEnabled(a.UInt32(0));
					return true;
				case "ISWINDOWVISIBLE":
					returnValue = IsWindowVisible(a.UInt32(0));
					return true;
				case "ISICONIC":
					returnValue = IsIconic(a.UInt32(0));
					return true;
				case "MOVEWINDOW":
					returnValue = MoveWindow(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.UInt32(5));
					return true;
				case "SHOWOWNEDPOPUPS":
					returnValue = ShowOwnedPopups(a.UInt32(0), a.UInt32(1));
					return true;

				// String and character functions
				case "CHARUPPERA":
					returnValue = CharUpperA(a.UInt32(0));
					return true;
				case "ISCHARALPHAA":
					returnValue = IsCharAlphaA(a.UInt32(0));
					return true;
				case "DRAWTEXTA":
					returnValue = (uint)DrawTextA(a.UInt32(0), a.LpcStr(1), a.Int32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "GRAYSTRINGA":
					returnValue = GrayStringA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.Int32(4), a.Int32(5), a.Int32(6), a.Int32(7), a.Int32(8));
					return true;
				case "TABBEDTEXTOUTA":
					returnValue = TabbedTextOutA(a.UInt32(0), a.Int32(1), a.Int32(2), a.LpcStr(3), a.Int32(4), a.Int32(5), a.UInt32(6), a.Int32(7));
					return true;
				case "GETWINDOWTEXTLENGTHA":
					returnValue = (uint)GetWindowTextLengthA(a.UInt32(0));
					return true;

				// Menu functions
				case "CHECKMENUITEM":
					returnValue = CheckMenuItem(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "CHECKMENURADIOITEM":
					returnValue = CheckMenuRadioItem(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "ENABLEMENUITEM":
					returnValue = EnableMenuItem(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "GETMENUITEMCOUNT":
					returnValue = (uint)GetMenuItemCount(a.UInt32(0));
					return true;
				case "GETMENUITEMID":
					returnValue = GetMenuItemID(a.UInt32(0), a.Int32(1));
					return true;
				case "GETMENUSTATE":
					returnValue = GetMenuState(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "GETSUBMENU":
					returnValue = GetSubMenu(a.UInt32(0), a.Int32(1));
					return true;
				case "MODIFYMENUA":
					returnValue = ModifyMenuA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.LpcStr(4));
					return true;
				case "SETMENU":
					returnValue = SetMenu(a.UInt32(0), a.UInt32(1));
					return true;
				case "SETMENUITEMBITMAPS":
					returnValue = SetMenuItemBitmaps(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "DESTROYMENU":
					returnValue = DestroyMenu(a.UInt32(0));
					return true;
				case "LOADMENUA":
					returnValue = LoadMenuA(a.UInt32(0), a.LpcStr(1));
					return true;
				case "GETMENUCHECKMARKDIMENSIONS":
					returnValue = GetMenuCheckMarkDimensions();
					return true;
				case "REMOVEMENU":
					returnValue = RemoveMenu(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "DRAWMENUBAR":
					returnValue = DrawMenuBar(a.UInt32(0));
					return true;

				// Rectangle functions
				case "COPYRECT":
					returnValue = CopyRect(a.UInt32(0), a.UInt32(1));
					return true;
				case "EQUALRECT":
					returnValue = EqualRect(a.UInt32(0), a.UInt32(1));
					return true;
				case "PTINRECT":
					returnValue = PtInRect(a.UInt32(0), a.Int32(1), a.Int32(2));
					return true;
				case "SETRECTEMPTY":
					returnValue = SetRectEmpty(a.UInt32(0));
					return true;

				// Scrollbar functions
				case "GETSCROLLPOS":
					returnValue = (uint)GetScrollPos(a.UInt32(0), a.Int32(1));
					return true;
				case "GETSCROLLRANGE":
					returnValue = GetScrollRange(a.UInt32(0), a.Int32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "SETSCROLLPOS":
					returnValue = (uint)SetScrollPos(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "SETSCROLLRANGE":
					returnValue = SetScrollRange(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.UInt32(4));
					return true;
				case "SHOWSCROLLBAR":
					returnValue = ShowScrollBar(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;

				// Dialog functions
				case "CREATEDIALOGINDIRECTPARAMA":
					returnValue = CreateDialogIndirectParamA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "GETDLGCTRLID":
					returnValue = (uint)GetDlgCtrlID(a.UInt32(0));
					return true;
				case "GETNEXTDLGTABITEM":
					returnValue = GetNextDlgTabItem(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "ISDIALOGMESSAGEA":
					returnValue = IsDialogMessageA(a.UInt32(0), a.UInt32(1));
					return true;

				// Input and keyboard functions
				case "GETASYNCKEYSTATE":
					returnValue = (uint)GetAsyncKeyState(a.Int32(0));
					return true;
				case "GETKEYSTATE":
					returnValue = (uint)GetKeyState(a.Int32(0));
					return true;
				case "GETKEYBOARDSTATE":
					returnValue = GetKeyboardState(a.UInt32(0));
					return true;
				case "MAPVIRTUALKEYA":
					returnValue = MapVirtualKeyA(a.UInt32(0), a.UInt32(1));
					return true;
				case "TOASCII":
					returnValue = (uint)ToAscii(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "TOUNICODE":
					returnValue = (uint)ToUnicode(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.Int32(4), a.UInt32(5));
					return true;
				case "GETCAPTURE":
					returnValue = GetCapture();
					return true;
				case "SETCAPTURE":
					returnValue = SetCapture(a.UInt32(0));
					return true;
				case "RELEASECAPTURE":
					returnValue = ReleaseCapture();
					return true;
				case "GETCURSORPOS":
					returnValue = GetCursorPos(a.UInt32(0));
					return true;

				// Message functions
				case "GETMESSAGEPOS":
					returnValue = GetMessagePos();
					return true;
				case "GETMESSAGETIME":
					returnValue = (uint)GetMessageTime();
					return true;
				case "CALLWINDOWPROCA":
					returnValue = CallWindowProcA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				// Window property functions
				case "GETPROPA":
					returnValue = GetPropA(a.UInt32(0), a.LpcStr(1));
					return true;
				case "SETPROPA":
					returnValue = SetPropA(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
					return true;
				case "REMOVEPROPA":
					returnValue = RemovePropA(a.UInt32(0), a.LpcStr(1));
					return true;

				// Icon and cursor functions
				case "LOADBITMAPA":
					returnValue = LoadBitmapA(a.UInt32(0), a.LpcStr(1));
					return true;
				case "DRAWICON":
					returnValue = DrawIcon(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "DESTROYICON":
					returnValue = DestroyIcon(a.UInt32(0));
					return true;

				// Coordinate mapping
				case "MAPWINDOWPOINTS":
					returnValue = (uint)MapWindowPoints(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "SCREENTOCLIENT":
					returnValue = ScreenToClient(a.UInt32(0), a.UInt32(1));
					return true;
				case "WINDOWFROMPOINT":
					returnValue = WindowFromPoint(a.Int32(0), a.Int32(1));
					return true;

				// System functions
				case "GETSYSCOLOR":
					returnValue = GetSysColor(a.Int32(0));
					return true;
				case "GETSYSCOLORBRUSH":
					returnValue = GetSysColorBrush(a.Int32(0));
					return true;
				case "MESSAGEBEEP":
					returnValue = MessageBeep(a.UInt32(0));
					return true;

				// Class functions
				case "GETCLASSINFOA":
					returnValue = GetClassInfoA(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
					return true;
				case "GETCLASSNAMEA":
					returnValue = (uint)GetClassNameA(a.UInt32(0), a.LpStr(1), a.Int32(2));
					return true;
				case "UNREGISTERCLASSA":
					returnValue = UnregisterClassA(a.LpcStr(0), a.UInt32(1));
					return true;

				// Redraw functions
				case "REDRAWWINDOW":
					returnValue = RedrawWindow(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "VALIDATERECT":
					returnValue = ValidateRect(a.UInt32(0), a.UInt32(1));
					return true;

				// Accelerator and hook functions
				case "LOADACCELERATORSA":
					returnValue = LoadAcceleratorsA(a.UInt32(0), a.LpcStr(1));
					return true;
				case "TRANSLATEACCELERATORA":
					returnValue = (uint)TranslateAcceleratorA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "SETWINDOWSHOOKEXA":
					returnValue = SetWindowsHookExA(a.Int32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "UNHOOKWINDOWSHOOKEX":
					returnValue = UnhookWindowsHookEx(a.UInt32(0));
					return true;
				case "CALLNEXTHOOKEX":
					returnValue = CallNextHookEx(a.UInt32(0), a.Int32(1), a.UInt32(2), a.UInt32(3));
					return true;

				// DDE functions
				case "REUSEDDELPARAM":
					returnValue = ReuseDDElParam(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "UNPACKDDELPARAM":
					returnValue = UnpackDDElParam(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				// Timer functions
				case "KILLTIMER":
					returnValue = KillTimer(a.UInt32(0), a.UInt32(1));
					return true;

				// Window activity functions
				case "GETLASTACTIVEPOPUP":
					returnValue = GetLastActivePopup(a.UInt32(0));
					return true;

				// Help function
				case "WINHELPA":
					returnValue = WinHelpA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3));
					return true;

				// Caret functions
				case "SHOWCARET":
					returnValue = ShowCaret(a.UInt32(0));
					return true;
				case "HIDECARET":
					returnValue = HideCaret(a.UInt32(0));
					return true;

				// Drawing functions
				case "DRAWFOCUSRECT":
					returnValue = DrawFocusRect(a.UInt32(0), a.UInt32(1));
					return true;
				case "DRAWCAPTION":
					returnValue = DrawCaption(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "DRAWFRAMECONTROL":
					returnValue = DrawFrameControl(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "EXCLUDEUPDATERGN":
					returnValue = (uint)ExcludeUpdateRgn(a.UInt32(0), a.UInt32(1));
					return true;

				// Rectangle functions
				case "INTERSECTRECT":
					returnValue = IntersectRect(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				// Window DC functions
				case "GETWINDOWDC":
					returnValue = GetWindowDC(a.UInt32(0));
					return true;

				// Dialog functions
				case "DEFDLGPROCA":
					returnValue = DefDlgProcA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "GETNEXTDLGGROUPITEM":
					returnValue = GetNextDlgGroupItem(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "MAPDIALOGRECT":
					returnValue = MapDialogRect(a.UInt32(0), a.UInt32(1));
					return true;

				// Class functions
				case "GETCLASSLONGA":
					returnValue = GetClassLongA(a.UInt32(0), a.Int32(1));
					return true;

				// Window state functions
				case "GETWINDOWPLACEMENT":
					returnValue = GetWindowPlacement(a.UInt32(0), a.UInt32(1));
					return true;
				case "ISWINDOWUNICODE":
					returnValue = IsWindowUnicode(a.UInt32(0));
					return true;

				// Clipboard functions
				case "REGISTERCLIPBOARDFORMATA":
					returnValue = RegisterClipboardFormatA(a.LpcStr(0));
					return true;

				// Thread message function
				case "POSTTHREADMESSAGEA":
					returnValue = PostThreadMessageA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				// Accelerator function
				case "COPYACCELERATORTABLEA":
					returnValue = CopyAcceleratorTableA(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;

				// Window context help
				case "SETWINDOWCONTEXTHELPID":
					returnValue = SetWindowContextHelpId(a.UInt32(0), a.UInt32(1));
					return true;

				// Scrolling functions
				case "SCROLLWINDOW":
					returnValue = ScrollWindow(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "SETSCROLLINFO":
					returnValue = SetScrollInfo(a.UInt32(0), a.Int32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "GETSCROLLINFO":
					returnValue = GetScrollInfo(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;

				// DDE (Dynamic Data Exchange) functions
				case "DDEINITIALIZEA":
					returnValue = DdeInitializeA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "DDECONNECT":
					returnValue = DdeConnect(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "DDEDISCONNECT":
					returnValue = DdeDisconnect(a.UInt32(0));
					return true;
				case "DDECREATESTRINGHANDLEA":
					returnValue = DdeCreateStringHandleA(a.UInt32(0), a.LpcStr(1), a.Int32(2));
					return true;
				case "DDEFREESTRINGHANDLE":
					returnValue = DdeFreeStringHandle(a.UInt32(0), a.UInt32(1));
					return true;
				case "DDECLIENTTRANSACTION":
					returnValue = DdeClientTransaction(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7));
					return true;
				case "DDEGETDATA":
					returnValue = DdeGetData(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				// Window state functions
				case "ISZOOMED":
					returnValue = IsZoomed(a.UInt32(0));
					return true;

				// Hotkey functions
				case "REGISTERHOTKEY":
					returnValue = RegisterHotKey(a.UInt32(0), a.Int32(1), a.UInt32(2), a.UInt32(3));
					return true;

				// Dialog functions
				case "DIALOGBOXINDIRECTPARAMA":
					returnValue = DialogBoxIndirectParamA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;

				// Character conversion functions
				case "CHARTOOEMA":
					returnValue = CharToOemA(a.LpStr(0), a.LpStr(1));
					return true;

				// Accelerator functions
				case "CREATEACCELERATORTABLEA":
					returnValue = CreateAcceleratorTableA(a.UInt32(0), a.Int32(1));
					return true;
				case "DESTROYACCELERATORTABLE":
					returnValue = DestroyAcceleratorTable(a.UInt32(0));
					return true;

				// Window region functions
				case "GETUPDATERECT":
					returnValue = GetUpdateRect(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "GETUPDATERGN":
					returnValue = GetUpdateRgn(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "INVALIDATERGN":
					returnValue = InvalidateRgn(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "VALIDATERGN":
					returnValue = ValidateRgn(a.UInt32(0), a.UInt32(1));
					return true;

				// Cursor functions
				case "LOADCURSORFROMFILEA":
					returnValue = LoadCursorFromFileA(a.LpcStr(0));
					return true;

				// Window class functions
				case "SETCLASSLONGA":
					returnValue = SetClassLongA(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;

				case "CHILDWINDOWFROMPOINT":
					returnValue = ChildWindowFromPoint(a.UInt32(0), a.Int32(1), a.Int32(2));
					return true;

				case "CLOSECLIPBOARD":
					returnValue = CloseClipboard();
					return true;

				case "DRAWEDGE":
					returnValue = DrawEdge(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "GETCLIPBOARDDATA":
					returnValue = GetClipboardData(a.UInt32(0));
					return true;

				case "ISCLIPBOARDFORMATAVAILABLE":
					returnValue = IsClipboardFormatAvailable(a.UInt32(0));
					return true;

				case "OPENCLIPBOARD":
					returnValue = OpenClipboard(a.UInt32(0));
					return true;

				case "TRACKPOPUPMENUEX":
					returnValue = TrackPopupMenuEx(a.UInt32(0), a.UInt32(1), a.Int32(2), a.Int32(3), a.UInt32(4), a.UInt32(5));
					return true;

				case "CHARLOWERA":
					returnValue = CharLowerA(a.LpStr(0));
					return true;

				case "CHARUPPERBUFFA":
					returnValue = CharUpperBuffA(a.LpStr(0), a.UInt32(1));
					return true;

				case "CREATECARET":
					returnValue = CreateCaret(a.UInt32(0), a.UInt32(1), a.Int32(2), a.Int32(3));
					return true;

				case "DESTROYCARET":
					returnValue = DestroyCaret();
					return true;

				case "SETCARETPOS":
					returnValue = SetCaretPos(a.Int32(0), a.Int32(1));
					return true;

				case "GETDOUBLECLICKTIME":
					returnValue = GetDoubleClickTime();
					return true;

				case "DELETEMENU":
					returnValue = DeleteMenu(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "INSERTMENUA":
					returnValue = InsertMenuA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.LpcStr(4));
					return true;

				case "INSERTMENUITEMA":
					returnValue = InsertMenuItemA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "GETMENUITEMINFOA":
					returnValue = GetMenuItemInfoA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "SETMENUITEMINFOA":
					returnValue = SetMenuItemInfoA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "SETMENUDEFAULTITEM":
					returnValue = SetMenuDefaultItem(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "SCROLLWINDOWEX":
					returnValue = ScrollWindowEx(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7));
					return true;

				case "SETWINDOWPLACEMENT":
					returnValue = SetWindowPlacement(a.UInt32(0), a.UInt32(1));
					return true;

				case "DRAWANIMATEDRECTS":
					returnValue = DrawAnimatedRects(a.UInt32(0), a.Int32(1), a.UInt32(2), a.UInt32(3));
					return true;

				case "EMPTYCLIPBOARD":
					returnValue = EmptyClipboard();
					return true;

				case "SETCLIPBOARDDATA":
					returnValue = SetClipboardData(a.UInt32(0), a.UInt32(1));
					return true;

				// Missing functions from issue
				case "CHANGEDISPLAYSETTINGSA":
					returnValue = (uint)ChangeDisplaySettingsA(a.UInt32(0), a.UInt32(1));
					return true;
				case "UNIONRECT":
					returnValue = UnionRect(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "GETDCEX":
					returnValue = GetDCEx(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "GETWINDOWTHREADPROCESSID":
					returnValue = GetWindowThreadProcessId(a.UInt32(0), a.UInt32(1));
					return true;
				case "KEYBD_EVENT":
					keybd_event(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					returnValue = 0;
					return true;
				case "MESSAGEBOXINDIRECTA":
					returnValue = MessageBoxIndirectA(a.UInt32(0));
					return true;
				case "ISRECTEMPTY":
					returnValue = IsRectEmpty(a.UInt32(0));
					return true;
				case "MOUSE_EVENT":
					mouse_event(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					returnValue = 0;
					return true;

				default:
					_logger.LogInformation("[User32] Unimplemented export: {Export}", export);
					return false;
			}
		}

		/// <summary>
		/// Async implementation for Win32 APIs that may call back into emulated code.
		/// Currently delegates to synchronous version - will be enhanced as more APIs are converted to async.
		/// </summary>
		public async Task<(bool success, uint returnValue)> TryInvokeAsync(
			string export,
			ICpu cpu,
			VirtualMemory memory,
			CancellationToken cancellationToken = default)
		{
			_cpu = cpu;
			_memory = memory;

			// For now, most APIs use synchronous implementation
			// TODO: Convert message handling APIs to use async paths
			if (TryInvokeUnsafe(export, cpu, memory, out var syncReturnValue))
			{
				return (true, syncReturnValue);
			}

			// No async work performed; return failure immediately
			return (false, 0);
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

			// Use ref struct wrapper for automatic memory access
			var wndClass = new WndClassARef(_env.Memory, lpWndClass);

			if (wndClass.lpszClassName == 0)
			{
				_logger.LogInformation("[User32] RegisterClassA: NULL class name");
				return 0;
			}

			var className = _env.ReadAnsiString(wndClass.lpszClassName);
			var menuName = wndClass.lpszMenuName != 0 ? _env.ReadAnsiString(wndClass.lpszMenuName) : null;

			var classInfo = new ProcessEnvironment.WindowClassInfo(
				className, wndClass.style, wndClass.lpfnWndProc, wndClass.cbClsExtra, wndClass.cbWndExtra,
				wndClass.hInstance, wndClass.hIcon, wndClass.hCursor, wndClass.hbrBackground, menuName
			);

			if (_env.RegisterWindowClass(className, classInfo))
			{
				// Return an ATOM (non-zero value) on success
				// Windows uses atoms (16-bit values) for class registration
				// Use a counter to ensure uniqueness and avoid hash collisions
				var atom = _nextAtom++;

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
				// Check if this is a dialog control handle
				var controlInfo = _env.FindDialogControlByHandle(hwnd);
				if (controlInfo.HasValue)
				{
					// Handle control visibility - store in window properties
					var controlStyle = _env.GetWindowProperty(hwnd, (int)NativeTypes.WindowLong.GWL_STYLE);
					bool wasControlVisible = (controlStyle & (uint)NativeTypes.WindowStyle.WS_VISIBLE) != 0;
					
					bool controlShouldBeVisible = nCmdShow != 0; // SW_HIDE = 0, all others show the window
					
					if (controlShouldBeVisible)
					{
						controlStyle |= (uint)NativeTypes.WindowStyle.WS_VISIBLE;
						_logger.LogInformation("[User32] ShowWindow: Control 0x{Hwnd:X8} (ID={ControlId}) is now visible", hwnd, controlInfo.Value.ControlId);
						
						// Notify GUI to show the control
						_host?.OnControlVisibilityChanged(controlInfo.Value.DialogHandle, controlInfo.Value.ControlId, true);
					}
					else
					{
						controlStyle &= ~(uint)NativeTypes.WindowStyle.WS_VISIBLE;
						_logger.LogInformation("[User32] ShowWindow: Control 0x{Hwnd:X8} (ID={ControlId}) is now hidden", hwnd, controlInfo.Value.ControlId);
						
						// Notify GUI to hide the control
						_host?.OnControlVisibilityChanged(controlInfo.Value.DialogHandle, controlInfo.Value.ControlId, false);
					}
					
					_env.SetWindowProperty(hwnd, (int)NativeTypes.WindowLong.GWL_STYLE, controlStyle);
					return wasControlVisible ? 1u : 0u;
				}
				
				_logger.LogWarning("[User32] ShowWindow: Invalid HWND=0x{Hwnd:X8}", hwnd);
				return 0; // Window was not previously visible
			}

			// Check if window was previously visible (has WS_VISIBLE style)
			var wasPreviouslyVisible = (window.Value.Style & (uint)NativeTypes.WindowStyle.WS_VISIBLE) != 0;

			// Update visibility based on nCmdShow
			// SW_HIDE = 0, SW_SHOWNORMAL = 1, SW_SHOWMINIMIZED = 2, SW_SHOWMAXIMIZED = 3,
			// SW_MAXIMIZE = 3, SW_SHOWNOACTIVATE = 4, SW_SHOW = 5, SW_MINIMIZE = 6,
			// SW_SHOWMINNOACTIVE = 7, SW_SHOWNA = 8, SW_RESTORE = 9
			bool shouldBeVisible = nCmdShow != 0; // SW_HIDE = 0, all others show the window

			// Get current style from window properties (which may have been modified)
			var currentStyle = _env.GetWindowProperty(hwnd, (int)NativeTypes.WindowLong.GWL_STYLE);
			if (currentStyle == 0)
			{
				// No custom style set, use the window's original style
				currentStyle = window.Value.Style;
			}

			// Update the WS_VISIBLE flag
			if (shouldBeVisible)
			{
				currentStyle |= (uint)NativeTypes.WindowStyle.WS_VISIBLE;
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
				currentStyle &= ~(uint)NativeTypes.WindowStyle.WS_VISIBLE;
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
			_env.SetWindowProperty(hwnd, (int)NativeTypes.WindowLong.GWL_STYLE, currentStyle);

			// Return non-zero if window was previously visible, zero if it was previously hidden
			return wasPreviouslyVisible ? 1u : 0u;
		}

		[DllModuleExport(10)]
		private uint GetMessageA(uint lpMsg, uint hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
		{
			if (lpMsg == 0)
			{
				_logger.LogInformation("[User32] GetMessageA: NULL MSG pointer");
				return 0xFFFFFFFF; // -1 for error
			}

			// Use ref struct wrapper - writes happen automatically on property assignment
			var msg = new MsgRef(_env.Memory, lpMsg);

			// Check if there's a quit message
			if (_env.HasQuitMessage())
			{
				var exitCode = _env.GetQuitExitCode();
				_logger.LogInformation("[User32] GetMessageA: WM_QUIT (exitCode={ExitCode})", exitCode);

				// Direct property assignments automatically write to memory
				msg.hwnd = 0;
				msg.message = 0x0012; // WM_QUIT
				msg.wParam = (uint)exitCode;
				msg.lParam = 0;
				msg.time = 0;
				msg.ptX = 0;
				msg.ptY = 0;

				return 0; // GetMessage returns 0 for WM_QUIT
			}

			// Try to get a message from the queue without blocking first
			var queuedMsg = _env.TryGetMessageNonBlocking(hWnd, wMsgFilterMin, wMsgFilterMax);

			if (queuedMsg.HasValue)
			{
				// Message available immediately
				if (queuedMsg.Value.Message == 0x0012)
				{
					// WM_QUIT - already being processed from the queue
					msg.hwnd = 0;
					msg.message = 0x0012; // WM_QUIT
					msg.wParam = queuedMsg.Value.WParam;
					msg.lParam = 0;
					msg.time = queuedMsg.Value.Time;
					msg.ptX = (int)queuedMsg.Value.PtX;
					msg.ptY = (int)queuedMsg.Value.PtY;

					return 0; // GetMessage returns 0 for WM_QUIT
				}

				_logger.LogInformation("[User32] GetMessageA: retrieved MSG=0x{ValueMessage:X4} HWND=0x{ValueHwnd:X8}", queuedMsg.Value.Message, queuedMsg.Value.Hwnd);

				// Direct property assignments automatically write to memory
				msg.hwnd = queuedMsg.Value.Hwnd;
				msg.message = queuedMsg.Value.Message;
				msg.wParam = queuedMsg.Value.WParam;
				msg.lParam = queuedMsg.Value.LParam;
				msg.time = queuedMsg.Value.Time;
				msg.ptX = (int)queuedMsg.Value.PtX;
				msg.ptY = (int)queuedMsg.Value.PtY;

				return 1; // GetMessage returns non-zero for all messages except WM_QUIT
			}

			// No message available - block the thread until a message arrives
			// This integrates with the thread scheduler to properly suspend the thread
			// instead of busy-waiting in a loop
			var scheduler = _env.ThreadScheduler;
			// Only use thread suspension if we have a real emulator host that can handle thread switching
			// In test environments without a host, fall back to timeout behavior
			if (scheduler != null && _env.Host != null)
			{
				var currentThreadId = _env.GetCurrentThreadId();
				var messageQueueToken = _env.GetMessageQueueWaitToken();

				_logger.LogDebug("[User32] GetMessageA: No messages available, blocking thread {ThreadId}", currentThreadId);

				// Set thread to waiting state with INFINITE timeout (0xFFFFFFFF)
				// The thread will be woken when a message is posted via PostMessage
				scheduler.SetThreadWaiting(currentThreadId, messageQueueToken, 0xFFFFFFFF);

				// IMPORTANT: Thread is now suspended - execution does not continue past this point
				// The emulator will context switch to another thread. When PostMessage wakes this
				// thread, execution resumes at the BEGINNING of GetMessageA (not here), so this
				// return statement never actually returns a value to the caller.
				// The return value 0xFFFFFFFF is a sentinel that indicates "thread suspended" but
				// is never used since the thread state prevents normal return flow.
				return 0xFFFFFFFF; // Sentinel: thread suspended, this value is never used
			}
			else
			{
				// No thread scheduler available - fall back to old timeout behavior
				// This maintains compatibility with tests and scenarios without threading
				_logger.LogTrace("[User32] GetMessageA: No thread scheduler, using timeout fallback");
				queuedMsg = _env.GetMessageBlocking(hWnd, wMsgFilterMin, wMsgFilterMax, timeoutMs: 100);

				if (queuedMsg.HasValue)
				{
					if (queuedMsg.Value.Message == 0x0012)
					{
						msg.hwnd = 0;
						msg.message = 0x0012; // WM_QUIT
						msg.wParam = queuedMsg.Value.WParam;
						msg.lParam = 0;
						msg.time = queuedMsg.Value.Time;
						msg.ptX = (int)queuedMsg.Value.PtX;
						msg.ptY = (int)queuedMsg.Value.PtY;

						return 0; // GetMessage returns 0 for WM_QUIT
					}

					_logger.LogInformation("[User32] GetMessageA: retrieved MSG=0x{ValueMessage:X4} HWND=0x{ValueHwnd:X8}", queuedMsg.Value.Message, queuedMsg.Value.Hwnd);

					msg.hwnd = queuedMsg.Value.Hwnd;
					msg.message = queuedMsg.Value.Message;
					msg.wParam = queuedMsg.Value.WParam;
					msg.lParam = queuedMsg.Value.LParam;
					msg.time = queuedMsg.Value.Time;
					msg.ptX = (int)queuedMsg.Value.PtX;
					msg.ptY = (int)queuedMsg.Value.PtY;

					return 1; // GetMessage returns non-zero for all messages except WM_QUIT
				}

				// Timeout - return WM_NULL for compatibility
				_logger.LogTrace("[User32] GetMessageA: Timeout, returning WM_NULL");
				msg.hwnd = 0;
				msg.message = 0; // WM_NULL
				msg.wParam = 0;
				msg.lParam = 0;
				msg.time = (uint)Environment.TickCount;
				msg.ptX = 0;
				msg.ptY = 0;

				return 1; // GetMessage returns non-zero for WM_NULL (only 0 for WM_QUIT)
			}
		}

		[DllModuleExport(30)]
		private uint TranslateMessage(uint lpMsg)
		{
			// TranslateMessage translates virtual-key messages into character messages

			if (lpMsg != 0)
			{
				var msg = new MsgRef(_env.Memory, lpMsg);
				_logger.LogInformation(
					"[User32] TranslateMessage: HWND=0x{Hwnd:X8} MSG=0x{Message:X4} wParam=0x{WParam:X8} lParam=0x{LParam:X8}",
					msg.hwnd, msg.message, msg.wParam, msg.lParam);
			}
			else
			{
				_logger.LogInformation("[User32] TranslateMessage: Called with null lpMsg");
			}

			return (uint)NativeTypes.Win32Bool.FALSE;
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
			var msg = new MsgRef(_env.Memory, lpMsg);

			_logger.LogInformation("[User32] DispatchMessageA: HWND=0x{Hwnd:X8} MSG=0x{Message:X4} wParam=0x{WParam:X8} lParam=0x{LParam:X8}",
				msg.hwnd, msg.message, msg.wParam, msg.lParam);

			// First, try dispatching through MessageDispatcher asynchronously
			if (_env.MessageDispatcher.HasHandlers(msg.message))
			{
				_logger.LogDebug("[User32] DispatchMessageA: Dispatching through MessageDispatcher");
				var typedMessage = Messaging.MessageFactory.CreateMessage(msg.hwnd, msg.message, msg.wParam, msg.lParam);
				// Use async dispatch with synchronous wait (DispatchMessageA is called from emulated code)
				var dispatchResult = _env.MessageDispatcher.DispatchAsync(typedMessage).GetAwaiter().GetResult();
				_logger.LogDebug("[User32] DispatchMessageA: MessageDispatcher returned 0x{Result:X8}", dispatchResult);
				// Continue to window procedure for compatibility
			}

			// Check if this is a standard control first
			var windowInfo = _env.GetWindow(msg.hwnd);
			if (windowInfo.HasValue && StandardControlHandler.IsStandardControl(windowInfo.Value.ClassName))
			{
				_logger.LogInformation("[User32] DispatchMessageA: Routing to standard control handler for class '{ClassName}'", windowInfo.Value.ClassName);
				return _standardControlHandler.HandleMessage(msg.hwnd, msg.message, msg.wParam, msg.lParam, windowInfo.Value.ClassName);
			}

			// Try to get the window procedure for this window
			var wndProc = _env.GetWindowProc(msg.hwnd);
			if (wndProc.HasValue && wndProc.Value != 0)
			{
				_logger.LogInformation("[User32] DispatchMessageA: Found WndProc=0x{WndProc:X8} for HWND=0x{Hwnd:X8}", wndProc.Value, msg.hwnd);

				var result = CallWindowProcedureAsync(wndProc.Value, msg.hwnd, msg.message, msg.wParam, msg.lParam).GetAwaiter().GetResult();
				_logger.LogInformation("[User32] DispatchMessageA: WndProc returned 0x{Result:X8}", result);
				return result;
			}

			_logger.LogInformation("[User32] DispatchMessageA: No WndProc found for HWND=0x{Hwnd:X8}", msg.hwnd);

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
		private uint CallWindowProcedure(uint wndProcAddress, uint hwnd, uint message, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] CallWindowProcedure: Calling 0x{WndProcAddress:X8} with HWND=0x{Hwnd:X8} MSG=0x{Message:X4}", wndProcAddress, hwnd, message);

			// Check if this is a standard control window procedure marker
			// These are not actual code addresses, but markers to route through StandardControlHandler
			if (ProcessEnvironment.IsStandardControlWndProc(wndProcAddress))
			{
				_logger.LogInformation("[User32] CallWindowProcedure: Detected standard control WndProc marker at 0x{WndProcAddress:X8}, routing to StandardControlHandler", wndProcAddress);
				var windowInfo = _env.GetWindow(hwnd);
				if (windowInfo.HasValue && StandardControlHandler.IsStandardControl(windowInfo.Value.ClassName))
				{
					return _standardControlHandler.HandleMessage(hwnd, message, wParam, lParam, windowInfo.Value.ClassName);
				}
				else
				{
					_logger.LogWarning("[User32] CallWindowProcedure: Window 0x{Hwnd:X8} has standard control WndProc but is not a standard control class", hwnd);
					return 0;
				}
			}

			// Validate window procedure address
			if (wndProcAddress == 0)
			{
				_logger.LogWarning("[User32] CallWindowProcedure: Window procedure address is NULL (0x00000000), aborting");
				return 0;
			}

			// Use consolidated helper to execute the procedure
			// Parameters are pushed right-to-left: lParam, wParam, message, hwnd
			uint[] parameters = [lParam, wParam, message, hwnd];
			var (returnValue, _, _) = ExecuteStdCallProcedure(
				_cpu, _memory, wndProcAddress, parameters, "CallWindowProcedure");

			return returnValue;
		}

		/// <summary>
		/// Core helper method for executing stdcall procedures (window procs, dialog procs, etc.) synchronously.
		/// Consolidates common stack setup, execution loop, and cleanup logic.
		/// This is the synchronous version of ExecuteStdCallProcedureAsync.
		/// </summary>
		/// <param name="cpu">CPU instance to use for execution</param>
		/// <param name="memory">Memory instance to use for stack operations</param>
		/// <param name="procedureAddress">Address of the procedure to call</param>
		/// <param name="parameters">Parameters to push on stack (right-to-left order)</param>
		/// <param name="contextName">Name for logging context (e.g., "CallWindowProcedure")</param>
		/// <returns>Tuple of (returnValue, timedOut, failed)</returns>
		private (uint returnValue, bool timedOut, bool failed) ExecuteStdCallProcedure(
			ICpu cpu,
			VirtualMemory memory,
			uint procedureAddress,
			uint[] parameters,
			string contextName)
		{
			// Save current CPU state
			var savedEip = cpu.GetEip();
			var savedEsp = cpu.GetRegister("ESP");
			var savedEbp = cpu.GetRegister("EBP");

			// Define return address marker
			const uint RETURN_ADDRESS = 0xDEADBEEF;

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			// Reserve STACK_SAFETY_MARGIN to prevent the called function from overwriting critical data
			// on the stack (such as return addresses from previous calls). The called function may use
			// stack space for local variables, nested calls, etc., which could overwrite data above
			// the parameters we push if we don't leave adequate space.
			const uint STACK_SAFETY_MARGIN = 256;
			var esp = savedEsp - STACK_SAFETY_MARGIN;

			// Push parameters (already in right-to-left order)
			foreach (var param in parameters)
			{
				esp -= 4;
				memory.Write32(esp, param);
			}

			// Push return address last (it must be pushed AFTER parameters so it's on top of the stack)
			esp -= 4;
			memory.Write32(esp, RETURN_ADDRESS);

			// Update CPU registers
			cpu.SetRegister("ESP", esp);
			cpu.SetEip(procedureAddress);

			// Execute until we hit the return address
			const int YIELD_INTERVAL = 10000;
			var steps = 0;
			var timedOut = false;
			var failed = false;
			var lastCheckEip = cpu.GetEip();
			var stuckCounter = 0;

			try
			{
				while (true)
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
						_logger.LogWarning("[User32] {Context}: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting", contextName);
						failed = true;
						break;
					}

					// Check for other invalid low addresses
					if (eip < MINIMUM_VALID_EIP && eip != RETURN_ADDRESS)
					{
						_logger.LogError("[User32] {Context}: Execution jumped to invalid low address 0x{Eip:X8}", contextName, eip);
						failed = true;
						break;
					}

					// Detect potential infinite loops
					if (steps > 0 && steps % INFINITE_LOOP_CHECK_INTERVAL == 0)
					{
						var currentEip = cpu.GetEip();
						if (currentEip == lastCheckEip)
						{
							stuckCounter++;
							if (stuckCounter >= STUCK_COUNTER_THRESHOLD)
							{
								_logger.LogWarning("[User32] {Context}: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting",
									contextName, currentEip, stuckCounter);
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

					// Execute one instruction
					var step = cpu.SingleStep(memory);

					// Handle COM vtable and import calls
					if (HandleComAndImportCalls(step, cpu, memory, contextName, out var stepDesc, out var shouldBreak) && shouldBreak)
					{
						failed = true;
						break;
					}

					steps++;

					// Periodically check if we should yield to other threads
					if (steps % YIELD_INTERVAL == 0)
					{
						var scheduler = _env.ThreadScheduler;
						if (scheduler != null)
						{
							scheduler.ProcessWaitTimeouts();
							if (scheduler.ShouldContextSwitch())
							{
								_logger.LogDebug("[User32] {Context}: Cooperative yield at {Steps} steps", contextName, steps);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[User32] {Context}: Exception during execution: {ExMessage}", contextName, ex.Message);
				failed = true;
			}

			// Get return value from EAX, but only if execution was successful
			var returnValue = (timedOut || failed) ? 0u : cpu.GetRegister("EAX");

			// Restore CPU state
			cpu.SetEip(savedEip);
			cpu.SetRegister("ESP", savedEsp);
			cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] {Context}: Completed with return value 0x{ReturnValue:X8}, timedOut={TimedOut}, failed={Failed}",
				contextName, returnValue, timedOut, failed);

			return (returnValue, timedOut, failed);
		}

		/// <summary>
		/// Core helper method for executing stdcall procedures (window procs, dialog procs, etc.) asynchronously.
		/// Consolidates common stack setup, execution loop, and cleanup logic.
		/// </summary>
		/// <param name="cpu">CPU instance to use for execution</param>
		/// <param name="memory">Memory instance to use for stack operations</param>
		/// <param name="procedureAddress">Address of the procedure to call</param>
		/// <param name="parameters">Parameters to push on stack (right-to-left order)</param>
		/// <param name="contextName">Name for logging context (e.g., "CallWindowProcedureAsync")</param>
		/// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
		/// <returns>Tuple of (returnValue, timedOut, cancelled, failed)</returns>
		private async Task<(uint returnValue, bool timedOut, bool cancelled, bool failed)> ExecuteStdCallProcedureAsync(
			ICpu cpu,
			VirtualMemory memory,
			uint procedureAddress,
			uint[] parameters,
			string contextName,
			CancellationToken cancellationToken = default)
		{
			// Save current CPU state
			var savedEip = cpu.GetEip();
			var savedEsp = cpu.GetRegister("ESP");
			var savedEbp = cpu.GetRegister("EBP");

			// Define return address marker
			const uint RETURN_ADDRESS = 0xDEADBEEF;

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			// Reserve STACK_SAFETY_MARGIN to prevent the called function from overwriting critical data
			// on the stack (such as return addresses from previous calls). The called function may use
			// stack space for local variables, nested calls, etc., which could overwrite data above
			// the parameters we push if we don't leave adequate space.
			const uint STACK_SAFETY_MARGIN = 256;
			var esp = savedEsp - STACK_SAFETY_MARGIN;

			// Push parameters (already in right-to-left order)
			foreach (var param in parameters)
			{
				esp -= 4;
				memory.Write32(esp, param);
			}

			// Push return address last (it must be pushed AFTER parameters so it's on top of the stack)
			esp -= 4;
			memory.Write32(esp, RETURN_ADDRESS);

			// Update CPU registers
			cpu.SetRegister("ESP", esp);
			cpu.SetEip(procedureAddress);

			// Execute until we hit the return address with cancellation support
			const int YIELD_INTERVAL = 10000;
			var steps = 0;
			var timedOut = false;
			var cancelled = false;
			var failed = false;
			var lastCheckEip = cpu.GetEip();
			var stuckCounter = 0;

			try
			{
				while (true)
				{
					// Check for cancellation at regular intervals
					if (steps % CANCELLATION_CHECK_INTERVAL == 0)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							_logger.LogInformation("[User32] {Context}: Cancellation requested at step {Steps}", contextName, steps);
							cancelled = true;
							break;
						}

						// Suspend execution to preserve CPU state across async boundary
						var cpuState = CpuHelpers.SuspendExecution(cpu);

						// Yield to allow other async operations to proceed
						await Task.Yield();

						// Resume execution with preserved state
						CpuHelpers.ResumeExecution(cpu, cpuState);
					}

					var eip = cpu.GetEip();

					// Check if we've returned to our marker address
					if (eip == RETURN_ADDRESS)
					{
						break;
					}

					// Check for invalid EIP (NULL pointer execution)
					if (eip == 0x00000000)
					{
						_logger.LogWarning("[User32] {Context}: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting", contextName);
						failed = true;
						break;
					}

					// Check for other invalid low addresses
					if (eip < MINIMUM_VALID_EIP && eip != RETURN_ADDRESS)
					{
						_logger.LogError("[User32] {Context}: Execution jumped to invalid low address 0x{Eip:X8}", contextName, eip);
						failed = true;
						break;
					}

					// Detect potential infinite loops
					if (steps > 0 && steps % INFINITE_LOOP_CHECK_INTERVAL == 0)
					{
						var currentEip = cpu.GetEip();
						if (currentEip == lastCheckEip)
						{
							stuckCounter++;
							if (stuckCounter >= STUCK_COUNTER_THRESHOLD)
							{
								_logger.LogWarning("[User32] {Context}: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting",
									contextName, currentEip, stuckCounter);
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

					// Execute instruction(s) - uses ExecuteBlockAsync for JIT CPUs, SingleStepAsync for interpreters
					var step = await CpuHelpers.ExecuteAsync(cpu, memory).ConfigureAwait(false);

					// Handle COM vtable and import calls
					if (HandleComAndImportCalls(step, cpu, memory, contextName, out var stepDesc, out var shouldBreak))
					{
						if (shouldBreak)
						{
							failed = true;
							break;
						}
					}

					steps++;

					// Periodically check if we should yield to other threads
					if (steps % YIELD_INTERVAL == 0)
					{
						var scheduler = _env.ThreadScheduler;
						if (scheduler != null)
						{
							scheduler.ProcessWaitTimeouts();
							if (scheduler.ShouldContextSwitch())
							{
								_logger.LogDebug("[User32] {Context}: Cooperative yield at {Steps} steps", contextName, steps);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[User32] {Context}: Exception during execution: {ExMessage}", contextName, ex.Message);
				failed = true;
			}

			// Get return value from EAX, but only if execution was successful
			var returnValue = (timedOut || cancelled || failed) ? 0u : cpu.GetRegister("EAX");

			// Restore CPU state
			cpu.SetEip(savedEip);
			cpu.SetRegister("ESP", savedEsp);
			cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] {Context}: Completed with return value 0x{ReturnValue:X8}, timedOut={TimedOut}, cancelled={Cancelled}, failed={Failed}",
				contextName, returnValue, timedOut, cancelled, failed);

			return (returnValue, timedOut, cancelled, failed);
		}

		/// <summary>
		/// Async version of CallWindowProcedure with cancellation support.
		/// Uses proper async/await to cleanly separate host (C#) and guest (x86) execution stacks.
		/// Includes STACK_SAFETY_MARGIN to prevent stack corruption from nested calls.
		/// </summary>
		private async Task<uint> CallWindowProcedureAsync(
			uint wndProcAddress,
			uint hwnd,
			uint message,
			uint wParam,
			uint lParam,
			CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("[User32] CallWindowProcedureAsync: Calling 0x{WndProcAddress:X8} with HWND=0x{Hwnd:X8} MSG=0x{Message:X4}",
				wndProcAddress, hwnd, message);

			// Check if this is a standard control window procedure marker
			if (ProcessEnvironment.IsStandardControlWndProc(wndProcAddress))
			{
				_logger.LogInformation("[User32] CallWindowProcedureAsync: Detected standard control WndProc marker at 0x{WndProcAddress:X8}, routing to StandardControlHandler",
					wndProcAddress);
				var windowInfo = _env.GetWindow(hwnd);
				if (windowInfo.HasValue && StandardControlHandler.IsStandardControl(windowInfo.Value.ClassName))
				{
					return _standardControlHandler.HandleMessage(hwnd, message, wParam, lParam, windowInfo.Value.ClassName);
				}
				else
				{
					_logger.LogWarning("[User32] CallWindowProcedureAsync: Window 0x{Hwnd:X8} has standard control WndProc but is not a standard control class", hwnd);
					return 0;
				}
			}

			// Validate window procedure address
			if (wndProcAddress == 0)
			{
				_logger.LogWarning("[User32] CallWindowProcedureAsync: Window procedure address is NULL (0x00000000), aborting");
				return 0;
			}

			// Use consolidated helper to execute the procedure
			// Parameters are pushed right-to-left: lParam, wParam, message, hwnd
			uint[] parameters = [lParam, wParam, message, hwnd];
			var (returnValue, _, _, _) = await ExecuteStdCallProcedureAsync(
				_cpu, _memory, wndProcAddress, parameters, "CallWindowProcedureAsync", cancellationToken).ConfigureAwait(false);

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
				var result = CallWindowProcedureAsync(wndProc.Value, hwnd, msg, wParam, lParam).GetAwaiter().GetResult();
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

			var point = new PointRef(_env.Memory, lpPoint);

			_logger.LogInformation("[User32] ClientToScreen: HWND=0x{Hwnd:X8} Point=({X},{Y})", hwnd, point.x, point.y);

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

			var rect = new RectRef(_env.Memory, lpRect);
			rect.left = left;
			rect.top = top;
			rect.right = right;
			rect.bottom = bottom;

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
			var rect = new RectRef(_env.Memory, lpRect);
			rect.left = 0;
			rect.top = 0;
			rect.right = 640;
			rect.bottom = 480;

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
			var rect = new RectRef(_env.Memory, lpRect);
			rect.left = 100;
			rect.top = 100;
			rect.right = 740;
			rect.bottom = 580;

			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint AdjustWindowRectEx(uint lpRect, uint dwStyle, int bMenu, uint dwExStyle)
		{
			if (lpRect == 0)
			{
				return 0;
			}

			var rect = new RectRef(_env.Memory, lpRect);

			_logger.LogInformation("[User32] AdjustWindowRectEx: rect=({Left},{Top},{Right},{Bottom}) style=0x{DwStyle:X8}",
				rect.left, rect.top, rect.right, rect.bottom, dwStyle);

			// Add window frame size (typical values)
			const int frameWidth = 8;
			const int frameHeight = 8;
			const int titleBarHeight = 32;
			const int menuHeight = 20;

			rect.left -= frameWidth;
			rect.top -= titleBarHeight;
			rect.right += frameWidth;
			rect.bottom += frameHeight;

			if (bMenu != 0)
			{
				rect.top -= menuHeight;
			}

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

			// Check if window exists
			var window = _env.GetWindow(hwnd);
			if (!window.HasValue)
			{
				_logger.LogWarning("[User32] DestroyWindow: Window 0x{Hwnd:X8} not found", hwnd);
				return 0; // FALSE - window doesn't exist
			}

			// Send WM_DESTROY message first (0x0002)
			// This is sent before the window is removed from the screen
			const uint WM_DESTROY = 0x0002;
			_env.SendMessageToWindow(hwnd, WM_DESTROY, 0, 0);
			_logger.LogDebug("[User32] DestroyWindow: Sent WM_DESTROY to window 0x{Hwnd:X8}", hwnd);

			// Send WM_NCDESTROY message (0x0082)
			// This is sent after WM_DESTROY and after child windows are destroyed
			const uint WM_NCDESTROY = 0x0082;
			_env.SendMessageToWindow(hwnd, WM_NCDESTROY, 0, 0);
			_logger.LogDebug("[User32] DestroyWindow: Sent WM_NCDESTROY to window 0x{Hwnd:X8}", hwnd);

			// Remove window from tracking
			if (_env.DestroyWindow(hwnd))
			{
				_logger.LogInformation("[User32] DestroyWindow: Successfully destroyed window 0x{Hwnd:X8}", hwnd);
				return 1; // TRUE
			}

			_logger.LogWarning("[User32] DestroyWindow: Failed to destroy window 0x{Hwnd:X8}", hwnd);
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
		private int GetSystemMetrics(SystemMetric nIndex)
		{
			_logger.LogInformation("[User32] GetSystemMetrics: nIndex={NIndex}", nIndex);

			// Return common system metrics
			switch (nIndex)
			{
				case SystemMetric.SM_CXSCREEN: //0:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning SM_CXSCREEN (0): {Width}", _env.DisplayWidth);
					return _env.DisplayWidth; // SM_CXSCREEN - Screen width (use display mode width)
				case SystemMetric.SM_CYSCREEN://1:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning SM_CYSCREEN (1): {Height}", _env.DisplayHeight);
					return _env.DisplayHeight; // SM_CYSCREEN - Screen height (use display mode height)
				case SystemMetric.SM_CXMIN://4:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning SM_CXSCREEN (4): 640");
					return 640; // SM_CXMIN - Minimum window width
				case SystemMetric.SM_CYMIN://5:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning SM_CXSCREEN (5): 480");
					return 480; // SM_CYMIN - Minimum window height
				default:
					_logger.LogInformation("[User32] GetSystemMetrics: Returning {SystemMetric} ({SystemMetricValue}): 0", nIndex.ToString(), (int)nIndex);
					return 0;
			}
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

		[DllModuleExport(732, entryPoint: 0x000139D0, Version = "5.1.2600.6532")]
		[DllModuleExport(1)]
		private int ShowCursor(int bShow)
		{
			_logger.LogInformation("[User32] ShowCursor: bShow={BShow}", bShow);

			// ShowCursor increments/decrements an internal display count
			// The cursor is displayed when the count is >= 0
			// The cursor is hidden when the count is < 0
			// Returns the new display count after the operation

			if (bShow != 0)
			{
				// TRUE - increment the display count (show cursor)
				_cursorDisplayCount++;
				_logger.LogInformation("[User32] ShowCursor: Incremented cursor count to {CursorDisplayCount}", _cursorDisplayCount);
			}
			else
			{
				// FALSE - decrement the display count (hide cursor)
				_cursorDisplayCount--;
				_logger.LogInformation("[User32] ShowCursor: Decremented cursor count to {CursorDisplayCount}", _cursorDisplayCount);
			}

			// Return the new display count
			return _cursorDisplayCount;
		}

		/// <summary>
		/// Sets the cursor position in screen coordinates.
		/// BOOL SetCursorPos(
		///   [in] int X,
		///   [in] int Y
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint SetCursorPos(int x, int y)
		{
			_logger.LogInformation("[User32] SetCursorPos: X={X}, Y={Y}", x, y);

			// For emulation purposes, we accept the call but don't actually move the cursor
			// A full implementation would update the cursor position in the rendering backend

			return 1; // TRUE - success
		}

		/// <summary>
		/// Confines the cursor to a rectangular area on the screen.
		/// BOOL ClipCursor(
		///   [in] const RECT *lpRect
		/// );
		/// </summary>
		/// <param name="lpRect">
		/// A pointer to the structure that contains the screen coordinates of the upper-left and lower-right corners
		/// of the confining rectangle. If this parameter is NULL, the cursor is free to move anywhere on the screen.
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is nonzero.
		/// If the function fails, the return value is zero.
		/// </returns>
		[DllModuleExport(4)]
		private uint ClipCursor(uint lpRect)
		{
			if (lpRect == 0)
			{
				_logger.LogInformation("[User32] ClipCursor: NULL (releasing cursor clip)");
			}
			else
			{
				// Read RECT structure (left, top, right, bottom)
				var left = (int)_env.MemRead32(lpRect + 0);
				var top = (int)_env.MemRead32(lpRect + 4);
				var right = (int)_env.MemRead32(lpRect + 8);
				var bottom = (int)_env.MemRead32(lpRect + 12);

				_logger.LogInformation("[User32] ClipCursor: Clipping to rect ({Left}, {Top}, {Right}, {Bottom})",
					left, top, right, bottom);
			}

			// For emulation, we just log the request and return success
			// A full implementation would:
			// 1. Store the clipping rectangle
			// 2. Constrain cursor movement to within the rectangle
			// 3. Update the rendering backend to enforce the clip

			return 1; // TRUE (success)
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
				_env.MemWriteStruct(lpMsg, ref queuedMsg);

				_logger.LogInformation("[User32] PeekMessageA: found MSG=0x{QueuedMsgMessage:X4}", queuedMsg.Message);
				return 1; // Message available
			}

			return 0; // No message available
		}

		[DllModuleExport(738, entryPoint: 0x00013A10, Version = "5.1.2600.6532")]
		[DllModuleExport(1, IsStub = false)]
		private uint WaitMessage()
		{
			_logger.LogInformation("[User32] WaitMessage");

			// WaitMessage waits until a message is posted to the calling thread's message queue
			// Returns TRUE (non-zero) if a message is available
			// Returns FALSE (0) if an error occurs

			// In our emulator, we'll do a simple wait with a small sleep
			// to simulate waiting for a message without spinning
			// For a stub implementation, just yield briefly and return success

			System.Threading.Thread.Sleep(1); // Brief yield to prevent spinning

			_logger.LogInformation("[User32] WaitMessage: Returning after wait");
			return (uint)NativeTypes.Win32Bool.TRUE; // Always return success
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
				// Note: We use RegisterHandle() instead of CreateWindow() to avoid creating duplicate GUI windows.
				// The actual GUI window is created later via OnDialogCreate(), which adds it to _createdDialogs.
				// SetWindowText will work because we've extended it to check _dialogStates in ProcessEnvironment.
				var hDlg = _env.RegisterHandle(new object()); // Dialog handle
				_logger.LogInformation("[User32] DialogBoxParamAsync: Created dialog handle=0x{HDlg:X8}", hDlg);

				// Initialize dialog state for proper message loop handling
				_env.InitializeDialogState(hDlg);

				// Create window handles for each control in the dialog
				var controlHandles = new Dictionary<int, uint>();
				foreach (var item in template.Items)
				{
					var controlHandle = _env.RegisterHandle(new object());
					controlHandles[item.Id] = controlHandle;
					_logger.LogInformation("[User32] DialogBoxParamAsync: Created control handle=0x{ControlHandle:X8} for ID={Id} ({Class})",
						controlHandle, item.Id, item.WindowClass);

					// Store control info for later retrieval (e.g., for GetDlgItem)
					_env.StoreControlInfo(hDlg, item.Id, controlHandle, item);
				}

				// If we have a host, show the dialog through Avalonia
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
							InitParam = dwInitParam,
							ControlHandles = controlHandles
						};

						// Show the dialog and WAIT for it to be created
						// This ensures the Avalonia window is fully created before WM_INITDIALOG is sent
						// so that SetDlgItemTextA and other initialization calls will update the GUI
						await _host.OnDialogCreate(dialogInfo).ConfigureAwait(false);

						_logger.LogInformation("[User32] DialogBoxParamAsync: Dialog window created, proceeding to WM_INITDIALOG");
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
					var (initResult, timedOut, cancelled, failed) = await CallDialogProcedureAsync(_cpu!, _memory!, lpDialogFunc, hDlg, WM_INITDIALOG, 0, dwInitParam, cancellationToken).ConfigureAwait(false);
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


				while (!_env.IsDialogEnded(hDlg) && !cancellationToken.IsCancellationRequested)
				{
					// Check for quit message
					if (_env.HasQuitMessage())
					{
						_logger.LogInformation("[User32] DialogBoxParamAsync: Quit message received, breaking modal loop");
						break;
					}

					// Try to get a message (with short timeout to avoid blocking indefinitely)
					// Use async version for better cooperative multitasking
					var queuedMsg = await _env.GetMessageAsync(0, 0, 0, timeoutMs: 10).ConfigureAwait(false);

					if (queuedMsg.HasValue)
					{
						var msg = queuedMsg.Value;
						_logger.LogInformation("[User32] DialogBoxParamAsync: Processing message MSG=0x{Message:X4} HWND=0x{Hwnd:X8} wParam=0x{WParam:X8} lParam=0x{LParam:X8}",
							msg.Message, msg.Hwnd, msg.WParam, msg.LParam);

						// Dispatch the message to the dialog procedure if it's for our dialog
						if (msg.Hwnd == hDlg || msg.Hwnd == 0)
						{
							if (lpDialogFunc != 0)
							{
								var (result, timedOut, cancelled, failed) = await CallDialogProcedureAsync(_cpu!, _memory!, lpDialogFunc, hDlg, msg.Message, msg.WParam, msg.LParam, cancellationToken).ConfigureAwait(false);
								_logger.LogInformation("[User32] DialogBoxParamAsync: Dialog procedure returned {Result} for MSG=0x{Message:X4}", result, msg.Message);

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
							_logger.LogInformation("[User32] DialogBoxParamAsync: Message for different window 0x{OtherHwnd:X8}, requeuing", msg.Hwnd);
							_env.PostMessage(msg.Hwnd, msg.Message, msg.WParam, msg.LParam);
						}
					}
					else
					{
						// If we've had too many empty iterations and the dialog proc timed out or failed, force end
						if ((dialogProcTimedOut || dialogProcFailed))
						{
							var status = dialogProcFailed ? "failed" : "timed out";
							_logger.LogWarning("[User32] DialogBoxParamAsync: No messages and dialog procedure {Status}, forcing dialog end", status);
							_env.SetDialogResult(hDlg, 0);
						}

						// Yield to avoid tight loop without introducing artificial delay
						await Task.Yield();
					}
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

			// Use consolidated helper to execute the procedure
			// Parameters are pushed right-to-left: lParam, wParam, message, hwndDlg
			uint[] parameters = [lParam, wParam, message, hwndDlg];
			return await ExecuteStdCallProcedureAsync(
				cpu, memory, dialogProcAddress, parameters, "CallDialogProcedureAsync", cancellationToken).ConfigureAwait(false);
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

			// Try to get the actual control handle from the dialog state first
			var controlHandle = _env.GetDialogControlHandle(hDlg, nIDDlgItem);
			if (controlHandle != 0)
			{
				_logger.LogInformation("[User32] GetDlgItem: Returning actual control handle 0x{ControlHandle:X8}", controlHandle);
				return controlHandle;
			}

			// Search for a child window with the matching control ID
			// For child windows, the control ID is stored in the Menu field
			var windowHandles = _env.GetAllWindowHandles();
			foreach (var hwnd in windowHandles)
			{
				var window = _env.GetWindow(hwnd);
				if (window.HasValue && window.Value.Parent == hDlg && window.Value.Menu == (uint)nIDDlgItem)
				{
					_logger.LogInformation("[User32] GetDlgItem: Found child window 0x{ChildHwnd:X8} with control ID {NIdDlgItem}", hwnd, nIDDlgItem);
					return hwnd;
				}
			}

			// Fallback: Return a synthetic handle (dialog handle + control ID) for compatibility
			_logger.LogInformation("[User32] GetDlgItem: Control not found, returning synthetic handle");
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
				text = text[..maxLength];
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

			// Notify the host (GUI) to update the control
			_host?.OnDialogControlTextChanged(hDlg, nIDDlgItem, text);

			_logger.LogInformation("[User32] SetDlgItemTextA: Set text '{Text}' for control {NIdDlgItem}", text, nIDDlgItem);

			return 1; // TRUE on success
		}

		[DllModuleExport(1)]
		private uint SendDlgItemMessageA(uint hDlg, int nIDDlgItem, uint msg, uint wParam, uint lParam)
		{
			// SendDlgItemMessageA sends a message to a control in a dialog box
			_logger.LogInformation("[User32] SendDlgItemMessageA: hDlg=0x{HDlg:X8} nIDDlgItem={NIdDlgItem} msg=0x{Msg:X4} wParam=0x{WParam:X8} lParam=0x{LParam:X8}",
				hDlg, nIDDlgItem, msg, wParam, lParam);

			// Get the control handle using GetDlgItem
			var hwndControl = GetDlgItem(hDlg, nIDDlgItem);
			if (hwndControl == 0)
			{
				_logger.LogWarning("[User32] SendDlgItemMessageA: Control not found for ID {NIdDlgItem}", nIDDlgItem);
				return 0;
			}

			// Handle STM_SETIMAGE (0x0172) for static controls
			if (msg == (uint)StaticControlMessage.STM_SETIMAGE)
			{
				var imageType = (ImageType)wParam;
				var imageHandle = lParam;

				_logger.LogInformation("[User32] SendDlgItemMessageA: STM_SETIMAGE imageType={ImageType} imageHandle=0x{ImageHandle:X8}",
					imageType, imageHandle);

				if (imageType == ImageType.IMAGE_BITMAP && imageHandle != 0)
				{
					// Look up the bitmap data from LoadImageA
					if (_loadedBitmaps.TryGetValue(imageHandle, out var bitmap))
					{
						_logger.LogInformation("[User32] SendDlgItemMessageA: Found bitmap data ({Size} bytes), notifying host",
							bitmap.Data.Length);

						// Notify the host (GUI) to display the bitmap
						_host?.OnDialogControlBitmapChanged(hDlg, nIDDlgItem, bitmap.Data);

						return 0; // Return 0 to indicate success (previous image handle would be returned normally)
					}
					else
					{
						_logger.LogWarning("[User32] SendDlgItemMessageA: Bitmap handle 0x{ImageHandle:X8} not found in loaded bitmaps",
							imageHandle);
					}
				}
			}

			// Forward the message to the control using SendMessageA
			return SendMessageA(hwndControl, msg, wParam, lParam);
		}

		/// <summary>
		/// Static control messages (STM_*)
		/// </summary>
		private enum StaticControlMessage : uint
		{
			STM_SETICON = 0x0170,
			STM_GETICON = 0x0171,
			STM_SETIMAGE = 0x0172,
			STM_GETIMAGE = 0x0173
		}

		/// <summary>
		/// Button control messages
		/// </summary>
		private enum ButtonMessage : uint
		{
			BM_GETCHECK = 0x00F0,
			BM_SETCHECK = 0x00F1,
			BM_GETSTATE = 0x00F2,
			BM_SETSTATE = 0x00F3,
			BM_SETSTYLE = 0x00F4,
			BM_CLICK = 0x00F5
		}

		/// <summary>
		/// Button state constants
		/// </summary>
		private enum ButtonState : uint
		{
			BST_UNCHECKED = 0x0000,
			BST_CHECKED = 0x0001,
			BST_INDETERMINATE = 0x0002
		}

		/// <summary>
		/// Image types for LoadImage and STM_SETIMAGE
		/// </summary>
		private enum ImageType : uint
		{
			IMAGE_BITMAP = 0,
			IMAGE_ICON = 1,
			IMAGE_CURSOR = 2,
			IMAGE_ENHMETAFILE = 3
		}

		/// <summary>
		/// Flags for LoadImage function
		/// </summary>
		[Flags]
		private enum LoadImageFlags : uint
		{
			LR_DEFAULTCOLOR = 0x0000,      // Default behavior (no special flags)
			LR_MONOCHROME = 0x0001,        // Load monochrome image
			LR_COLOR = 0x0002,             // Default (ignored)
			LR_COPYRETURNORG = 0x0004,     // Return original handle
			LR_COPYDELETEORG = 0x0008,     // Delete original after copy
			LR_LOADFROMFILE = 0x0010,      // Load from file
			LR_LOADTRANSPARENT = 0x0020,   // Load with transparency
			LR_DEFAULTSIZE = 0x0040,       // Use default size
			LR_VGACOLOR = 0x0080,          // Use VGA colors
			LR_LOADMAP3DCOLORS = 0x1000,   // Map 3D colors
			LR_CREATEDIBSECTION = 0x2000,  // Create DIB section
			LR_COPYFROMRESOURCE = 0x4000,  // Copy from resource
			LR_SHARED = 0x8000             // Share image handle
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
			var isEnabled = bEnable != 0;
			_windowEnabledState[hwnd] = isEnabled;

			// Try to notify the host (GUI) if this is a dialog control
			// Check if this window is a control in any dialog
			var controlInfo = _env.FindDialogControlByHandle(hwnd);
			if (controlInfo.HasValue)
			{
				_logger.LogInformation("[User32] EnableWindow: Notifying GUI to {Action} control {ControlId} in dialog 0x{DialogHandle:X8}",
					isEnabled ? "enable" : "disable", controlInfo.Value.ControlId, controlInfo.Value.DialogHandle);
				_host?.OnDialogControlEnabledChanged(controlInfo.Value.DialogHandle, controlInfo.Value.ControlId, isEnabled);
			}

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

			var ps = new PaintStructRef(_env.Memory, lpPaint);
			ps.hdc = hdc;
			ps.fErase = 1; // TRUE

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
				var ps = new PaintStructRef(_env.Memory, lpPaint);
				ReleaseDc(hwnd, ps.hdc);
			}

			return 1; // Always returns non-zero
		}

		[DllModuleExport(1)]
		private uint FillRect(uint hdc, uint lprc, uint hbr)
		{
			_logger.LogInformation("[User32] FillRect: hdc=0x{Hdc:X8} lprc=0x{Lprc:X8} hbr=0x{Hbr:X8}", hdc, lprc, hbr);

			if (lprc != 0)
			{
				var rect = new RectRef(_env.Memory, lprc);
				_logger.LogInformation("[User32] FillRect: rect=({Left},{Top},{Right},{Bottom})",
					rect.left, rect.top, rect.right, rect.bottom);
			}

			// For now, we don't do any actual drawing.
			// Just return success.
			return 1;
		}

		/// <summary>
		/// Retrieves the previous character in a string.
		/// LPSTR CharPrevA(
		///   [in] LPCSTR lpszStart,
		///   [in] LPCSTR lpszCurrent
		/// );
		/// </summary>
		// [DllModuleExport(0)]
		private uint CharPrevA(uint lpszStart, uint lpszCurrent)
		{
			_logger.LogDebug("[User32] CharPrevA(lpszStart=0x{LpszStart:X8}, lpszCurrent=0x{LpszCurrent:X8})",
				lpszStart, lpszCurrent);

			// If at or before the start, return start pointer
			if (lpszCurrent <= lpszStart)
			{
				return lpszStart;
			}

			// For single-byte character sets (ASCII), just go back by 1 byte
			// A full implementation would need to check for multi-byte character sequences
			// in DBCS encodings and potentially scan backwards to find the character boundary
			return lpszCurrent - 1;
		}

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
		private void RestoreEbpFromStack(uint esp)
		{
			CpuHelpers.RestoreEbpFromStack(_cpu!, _memory!, esp, _logger, "User32");
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

			// Update the window title in ProcessEnvironment
			if (_env.SetWindowText(hWnd, text))
			{
				// Notify the GUI if a host is available
				_host?.OnWindowTitleChanged(hWnd, text);
				return 1; // TRUE - success
			}

			// Window not found
			_logger.LogWarning("[User32] SetWindowTextA: Window not found: HWND=0x{HWnd:X8}", hWnd);
			return 0; // FALSE - failure
		}

		[DllModuleExport(20)]
		private uint LoadImageA(uint hInst, in LpStr name, uint type, int cx, int cy, uint fuLoad)
		{
			var imageName = name.Read(_env.Memory);
			_logger.LogInformation("[User32] LoadImageA(hInst=0x{HInst:X8}, name=\"{ImageName}\", type={Type}, cx={Cx}, cy={Cy}, fuLoad=0x{FuLoad:X})",
				hInst, imageName, type, cx, cy, fuLoad);

			if (type == (uint)ImageType.IMAGE_BITMAP)
			{
				// Try to load bitmap resource or file
				byte[]? bitmapData = null;
				var loadedFromFile = false;

				// Check if LR_LOADFROMFILE flag is set
				var loadFromFile = (fuLoad & (uint)LoadImageFlags.LR_LOADFROMFILE) != 0;

				if (!loadFromFile && _resourceReader != null)
				{
					// Try to load from resources first
					// Check if name is an integer resource ID or string name
					// Win32 convention: integer resource IDs are < MAX_INTRESOURCE (0x10000)
					if (name.Address < MAX_INTRESOURCE)
					{
						// It's an integer resource ID
						var resourceId = name.Address;
						bitmapData = _resourceReader.LoadBitmap(resourceId);
						if (bitmapData != null)
						{
							_logger.LogInformation("[User32] LoadImageA: Loaded bitmap from resource ID {ResourceId}", resourceId);
						}
						else
						{
							_logger.LogDebug("[User32] LoadImageA: Bitmap resource ID {ResourceId} not found", resourceId);
						}
					}
					else
					{
						// It's a string name
						_logger.LogDebug("[User32] LoadImageA: Attempting to load bitmap by name \"{ImageName}\"", imageName);
						bitmapData = _resourceReader.LoadBitmapByName(imageName);
						if (bitmapData != null)
						{
							_logger.LogInformation("[User32] LoadImageA: Loaded bitmap from resource name \"{ImageName}\"", imageName);
						}
						else
						{
							_logger.LogDebug("[User32] LoadImageA: Bitmap resource name \"{ImageName}\" not found in resources", imageName);
						}
					}
				}
				else if (!loadFromFile)
				{
					_logger.LogDebug("[User32] LoadImageA: Resource reader is null, cannot load from resources");
				}

				// If resource loading failed or LR_LOADFROMFILE was set, try loading from file
				if (bitmapData == null && !string.IsNullOrEmpty(imageName))
				{
					_logger.LogInformation("[User32] LoadImageA: Attempting to load bitmap from file \"{ImageName}\"", imageName);
					try
					{
						// Determine which filenames to try based on whether imageName already has an extension
						var hasExtension = imageName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) || 
						                   imageName.EndsWith(".dib", StringComparison.OrdinalIgnoreCase);
						
						var filenamesToTry = hasExtension 
							? new[] { imageName }
							: new[] { imageName + ".bmp", imageName + ".dib", imageName };
						
						foreach (var filename in filenamesToTry)
						{
							// Try to read the file through VFS
							if (_env.VirtualFileSystem != null && _env.VirtualFileSystem.FileExists(filename))
							{
								using var fileHandle = _env.VirtualFileSystem.OpenFile(filename, VirtualFileSystem.VfsFileMode.Open, VirtualFileSystem.VfsFileAccess.Read);
								if (fileHandle != null)
								{
									// Get file size by seeking to end
									var fileSize = fileHandle.Seek(0, SeekOrigin.End);
									fileHandle.Seek(0, SeekOrigin.Begin); // Reset to beginning
									
									if (fileSize > 0 && fileSize < int.MaxValue)
									{
										bitmapData = new byte[(int)fileSize];
										var bytesRead = fileHandle.Read(bitmapData, 0, (int)fileSize);
										if (bytesRead == fileSize)
										{
											_logger.LogInformation("[User32] LoadImageA: Loaded bitmap from file \"{FileName}\" ({Size} bytes)", 
												filename, bytesRead);
											loadedFromFile = true;
											break;
										}
										else
										{
											_logger.LogWarning("[User32] LoadImageA: Incomplete read of bitmap file \"{FileName}\". Expected {ExpectedBytes} bytes, got {ActualBytes} bytes.", 
												filename, fileSize, bytesRead);
										}
									}
								}
							}
						}
					}
					catch (System.IO.FileNotFoundException ex)
					{
						_logger.LogWarning(ex, "[User32] LoadImageA: File not found \"{ImageName}\"", imageName);
					}
					catch (System.UnauthorizedAccessException ex)
					{
						_logger.LogWarning(ex, "[User32] LoadImageA: Access denied loading bitmap from file \"{ImageName}\"", imageName);
					}
					catch (System.IO.IOException ex)
					{
						_logger.LogWarning(ex, "[User32] LoadImageA: IO error loading bitmap from file \"{ImageName}\"", imageName);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "[User32] LoadImageA: Unexpected error loading bitmap from file \"{ImageName}\"", imageName);
					}
				}

				if (bitmapData != null && bitmapData.Length > 0)
				{
					// Store the bitmap data for later retrieval by the UI
					// Use a handle that starts at 0x90000000 to avoid conflicts
					var handle = 0x90000000u + _nextBitmapHandle++;
					_loadedBitmaps[handle] = new LoadedBitmap
					{
						Data = bitmapData,
						Name = imageName,
						Width = cx > 0 ? cx : 0,
						Height = cy > 0 ? cy : 0
					};

					_logger.LogInformation("[User32] LoadImageA: Successfully loaded bitmap \"{ImageName}\" with {Size} bytes from {Source}, handle 0x{Handle:X8}",
						imageName, bitmapData.Length, loadedFromFile ? "file" : "resource", handle);
					return handle;
				}

				_logger.LogWarning("[User32] LoadImageA: Bitmap \"{ImageName}\" not found in resources or files", imageName);
				// Error 1814 = ERROR_RESOURCE_NAME_NOT_FOUND
				return 0;
			}

			// Stub for icons and cursors - return a dummy handle
			var stubHandle = 0x90000000u + ((uint)imageName.GetHashCode() & 0x0FFFFFFF);
			_logger.LogInformation("[User32] LoadImageA: Returning stub handle 0x{Handle:X8} for type {Type}", stubHandle, type);
			return stubHandle;
		}

		private readonly Dictionary<uint, LoadedBitmap> _loadedBitmaps = new();

		private class LoadedBitmap
		{
			public byte[] Data { get; set; } = Array.Empty<byte>();
			public string Name { get; set; } = string.Empty;
			public int Width { get; set; }
			public int Height { get; set; }
		}

		/// <summary>
		/// Gets a loaded bitmap by handle. Used by the UI to display bitmaps.
		/// </summary>
		public byte[]? GetLoadedBitmapData(uint handle)
		{
			if (_loadedBitmaps.TryGetValue(handle, out var bitmap))
			{
				return bitmap.Data;
			}
			return null;
		}

		[DllModuleExport(16)]
		private uint LoadStringA(uint hInstance, uint uID, in LpStr lpBuffer, int cchBufferMax)
		{
			_logger.LogInformation("[User32] LoadStringA(hInstance=0x{HInstance:X8}, uID={UID}, lpBuffer=0x{LpBuffer:X8}, cchBufferMax={CchBufferMax})",
				hInstance, uID, lpBuffer.Address, cchBufferMax);

			// Try to load the string from resources
			if (_resourceReader != null)
			{
				var str = _resourceReader.LoadString(uID);
				if (str != null)
				{
					// Calculate the actual length to write (limited by buffer size)
					var writeLen = str.Length;
					if (cchBufferMax > 0 && str.Length >= cchBufferMax)
					{
						writeLen = cchBufferMax - 1;
						str = str.Substring(0, writeLen);
					}

					_logger.LogInformation("[User32] LoadStringA: Loaded string \"{String}\" (length {Length})", str, str.Length);

					if (cchBufferMax > 0)
					{
						lpBuffer.Write(_env.Memory, str, true);
					}

					return (uint)writeLen;
				}
			}

			_logger.LogWarning("[User32] LoadStringA: String resource {UID} not found", uID);

			// String not found - return empty
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

			// Format the string using stack arguments
			// Args 0 and 1 are output and format, so variadic args start at index 2
			var result = FormatString(formatStr, args, 2);
			output.Write(_env.Memory, result, true);

			return (uint)result.Length;
		}

		[DllModuleExport(12)]
		private uint WvsprintfA(in LpStr output, in LpcStr format, uint arglist)
		{
			var formatStr = format.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] WvsprintfA(output=0x{Output:X8}, format=\"{FormatStr}\", arglist=0x{Arglist:X8})",
				output.Address, formatStr, arglist);

			// Format the string using va_list (arglist is a pointer to variadic arguments)
			var result = FormatStringFromVaList(formatStr, arglist);
			output.Write(_env.Memory, result, true);

			return (uint)result.Length;
		}

		/// <summary>
		/// Formats a printf-style format string with arguments from the stack.
		/// Supports common format specifiers: %s (string), %d/%i (int), %u (uint), %x/%X (hex), %c (char), %% (literal %).
		/// </summary>
		private string FormatString(string format, StackArgs args, int startArgIndex)
		{
			var result = new System.Text.StringBuilder();
			int argIndex = startArgIndex;
			
			for (int i = 0; i < format.Length; i++)
			{
				if (format[i] == '%' && i + 1 < format.Length)
				{
					i++; // Skip the %
					
					// Handle %% (literal %)
					if (format[i] == '%')
					{
						result.Append('%');
						continue;
					}
					
					// Parse format specifier (simplified - doesn't handle width, precision, etc.)
					char specifier = format[i];
					
					switch (specifier)
					{
						case 's': // String pointer
							var strAddr = args.UInt32(argIndex++);
							if (strAddr != 0)
							{
								var str = new LpcStr(strAddr, _env.Memory).ToString() ?? string.Empty;
								result.Append(str);
							}
							else
							{
								result.Append("(null)");
							}
							break;
						
						case 'd': // Signed decimal integer
						case 'i':
							var intVal = args.Int32(argIndex++);
							result.Append(intVal);
							break;
						
						case 'u': // Unsigned decimal integer
							var uintVal = args.UInt32(argIndex++);
							result.Append(uintVal);
							break;
						
						case 'x': // Unsigned hexadecimal (lowercase)
							var hexVal = args.UInt32(argIndex++);
							result.Append(hexVal.ToString("x"));
							break;
						
						case 'X': // Unsigned hexadecimal (uppercase)
							var hexValUpper = args.UInt32(argIndex++);
							result.Append(hexValUpper.ToString("X"));
							break;
						
						case 'c': // Character
							var charVal = (char)args.UInt32(argIndex++);
							result.Append(charVal);
							break;
						
						default:
							// Unknown specifier - just append it as-is
							result.Append('%');
							result.Append(specifier);
							argIndex++; // Still consume an argument
							break;
					}
				}
				else
				{
					result.Append(format[i]);
				}
			}
			
			return result.ToString();
		}

		/// <summary>
		/// Formats a printf-style format string with arguments from a va_list pointer.
		/// </summary>
		private string FormatStringFromVaList(string format, uint vaListPtr)
		{
			var result = new System.Text.StringBuilder();
			uint currentArgPtr = vaListPtr;
			
			for (int i = 0; i < format.Length; i++)
			{
				if (format[i] == '%' && i + 1 < format.Length)
				{
					i++; // Skip the %
					
					// Handle %% (literal %)
					if (format[i] == '%')
					{
						result.Append('%');
						continue;
					}
					
					// Parse format specifier
					char specifier = format[i];
					
					switch (specifier)
					{
						case 's': // String pointer
							var strAddr = _env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							if (strAddr != 0)
							{
								var str = new LpcStr(strAddr, _env.Memory).ToString() ?? string.Empty;
								result.Append(str);
							}
							else
							{
								result.Append("(null)");
							}
							break;
						
						case 'd': // Signed decimal integer
						case 'i':
							var intVal = (int)_env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(intVal);
							break;
						
						case 'u': // Unsigned decimal integer
							var uintVal = _env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(uintVal);
							break;
						
						case 'x': // Unsigned hexadecimal (lowercase)
							var hexVal = _env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(hexVal.ToString("x"));
							break;
						
						case 'X': // Unsigned hexadecimal (uppercase)
							var hexValUpper = _env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(hexValUpper.ToString("X"));
							break;
						
						case 'c': // Character
							var charVal = (char)_env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(charVal);
							break;
						
						default:
							// Unknown specifier - just append it as-is
							result.Append('%');
							result.Append(specifier);
							currentArgPtr += 4; // Still consume an argument
							break;
					}
				}
				else
				{
					result.Append(format[i]);
				}
			}
			
			return result.ToString();
		}

		/// <summary>
		/// Check if a Win32 API function typically returns a handle or function pointer.
		/// Helps identify potential NULL pointer issues when these functions return 0.
		/// </summary>

		/// <summary>
		/// Handles COM vtable calls and import function calls during CPU emulation.
		/// This consolidates the duplicated logic from CallWindowProcedure and CallDialogProcedureAsync.
		/// </summary>
		/// <param name="step">The current CPU step result</param>
		/// <param name="cpu">The CPU instance</param>
		/// <param name="memory">The virtual memory instance</param>
		/// <param name="logContext">Context string for logging (e.g., "CallWindowProcedure", "CallDialogProcedureAsync")</param>
		/// <param name="stepDesc">Output parameter for step description</param>
		/// <returns>True if the step was handled (COM or import call), false if it should be processed normally</returns>
		private bool HandleComAndImportCalls(CpuStepResult step, ICpu cpu, VirtualMemory memory, string logContext, out string? stepDesc, out bool shouldBreak)
		{
			stepDesc = null;
			shouldBreak = false;

			// Check for COM vtable method calls
			if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
			{
				var logLevel = logContext.Contains("Dialog") ? LogLevel.Information : LogLevel.Debug;
				_logger.Log(logLevel, "[User32] {Context}: COM vtable call at 0x{CallTarget:X8}", logContext, step.CallTarget);

				// Save callee-saved registers (EBX, ESI, EDI, EBP)
				var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);

				if (_env.ComDispatcher.TryInvoke(step.CallTarget, cpu, memory, out var comRet, out var comArgBytes))
				{
					stepDesc = $"COM vtable call -> 0x{step.CallTarget:X8}";
					var currentEsp = cpu.GetRegister("ESP");
					var retEip = memory.Read32(currentEsp);

					// Validate return address before jumping
					if (!IsValidReturnAddress(retEip, _image))
					{
						_logger.LogError("[User32] {Context}: Invalid return address 0x{RetEip:X8} from COM call", logContext, retEip);
						shouldBreak = true;
						return true;
					}

					currentEsp += 4 + (uint)comArgBytes; // Pop return address + arguments
					cpu.SetRegister("ESP", currentEsp);
					cpu.SetRegister("EAX", comRet);
					cpu.SetEip(retEip);

					// Restore callee-saved registers, skipping invalid EBP values (e.g., import hooks)
					CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved, skipInvalidEbp: true, memorySize: memory.Size);
				}
				return true;
			}
			// Check for import calls
			else if (step.IsCall && _image != null && _image.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
			{
				var dll = imp.dll.ToUpperInvariant();
				var name = imp.name;
				var logLevel = logContext.Contains("Dialog") ? LogLevel.Information : LogLevel.Debug;
				_logger.Log(logLevel, "[User32] {Context}: Import call {Dll}!{Name} at 0x{CallTarget:X8}", logContext, dll, name, step.CallTarget);
				stepDesc = $"Import call {dll}!{name}";

				// Save callee-saved registers (EBX, ESI, EDI, EBP)
				var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);

				if (_dispatcher != null && _dispatcher.TryInvoke(dll, name, cpu, memory, out var ret, out var argBytes))
				{
					_logger.Log(logLevel, "[User32] {Context}: Import {Dll}!{Name} returned 0x{Ret:X8}", logContext, dll, name, ret);

					// Warn if a function that typically returns handles/pointers returns NULL
					if (ret == 0 && IsHandleReturningFunction(name))
					{
						_logger.LogWarning("[User32] {Context}: {Dll}!{Name} returned NULL (0) - this may cause NULL pointer dereference if used as function pointer or handle", logContext, dll, name);
					}

					var currentEsp = cpu.GetRegister("ESP");
					var retEip = memory.Read32(currentEsp);

					// Validate return address before jumping
					if (!IsValidReturnAddress(retEip, _image))
					{
						_logger.LogError("[User32] {Context}: Invalid return address 0x{RetEip:X8} from import {Dll}!{Name}", logContext, retEip, dll, name);
						shouldBreak = true;
						return true;
					}

					currentEsp += 4 + (uint)argBytes;

					cpu.SetRegister("ESP", currentEsp);
					cpu.SetRegister("EAX", ret);
					cpu.SetEip(retEip);

					// Restore callee-saved registers, skipping invalid EBP values (e.g., import hooks)
					CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved, skipInvalidEbp: true, memorySize: memory.Size);
				}
				else
				{
					// Import function not implemented - try to get arg bytes from metadata and simulate return
					var simulatedArgBytes = 0;
					try
					{
						simulatedArgBytes = StdCallMeta.GetArgBytes(dll, name);
						_logger.LogWarning("[User32] {Context}: Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes={ArgBytes}", logContext, dll, name, simulatedArgBytes);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "[User32] {Context}: Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes unknown (assuming 0)", logContext, dll, name);
					}

					var currentEsp = cpu.GetRegister("ESP");
					var retEip = memory.Read32(currentEsp);

					// Validate return address before jumping
					if (!IsValidReturnAddress(retEip, _image))
					{
						_logger.LogError("[User32] {Context}: Invalid return address 0x{RetEip:X8} from unimplemented import {Dll}!{Name}", logContext, retEip, dll, name);
						shouldBreak = true;
						return true;
					}

					// Pop return address + parameters (stdcall convention - callee cleans)
					currentEsp += 4 + (uint)simulatedArgBytes;

					cpu.SetRegister("ESP", currentEsp);
					cpu.SetRegister("EAX", 0); // Return 0 as default
					cpu.SetEip(retEip);

					// Restore callee-saved registers, skipping invalid EBP values (e.g., import hooks)
					CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved, skipInvalidEbp: true, memorySize: memory.Size);
				}
				return true;
			}

			return false;
		}

		/// <summary>
		/// Validates that a return address points to valid executable code and not to stack or invalid memory.
		/// Uses actual values from the PE header and process environment instead of hardcoded constants.
		/// </summary>
		/// <param name="address">The return address to validate</param>
		/// <param name="image">The loaded PE image (optional)</param>
		/// <returns>True if the address is valid for execution, false otherwise</returns>
		private bool IsValidReturnAddress(uint address, LoadedImage? image)
		{
			// Reject NULL addresses
			if (address == 0)
			{
				return false;
			}

			// Get actual stack boundaries from process environment
			var stackLimit = _env.StackLimit;
			var stackBase = _env.StackBase;

			// Reject addresses within the stack region
			if (address >= stackLimit && address <= stackBase)
			{
				return false;
			}

			// If we have image info, use IsAddressInCodeSection for proper validation
			if (image != null)
			{
				var isInCodeSection = image.IsAddressInCodeSection(address);

				// Also check if it's in imported DLL space (though we don't have full info)
				// For now, accept any address above the image base if not in code section
				// This handles DLLs that are loaded at different addresses
				if (!isInCodeSection && address >= image.BaseAddress)
				{
					// Could be in a DLL, allow it
					return true;
				}

				return isInCodeSection;
			}

			// Without image info, use conservative default (typical Win32 image base)
			// This is a fallback for cases where image info is not available
			const uint DEFAULT_MIN_CODE_ADDRESS = 0x00400000;
			return address >= DEFAULT_MIN_CODE_ADDRESS;
		}

		private static bool IsHandleReturningFunction(string functionName)
		{
			var upperName = functionName.ToUpperInvariant();

			// Window/control creation functions
			if (upperName.Contains("CREATE") || upperName.Contains("LOAD"))
			{
				return true;
			}

			// Functions that return window handles
			if (upperName.StartsWith("GET") && (upperName.Contains("WINDOW") || upperName.Contains("DLG")))
			{
				return true;
			}

			// Device context functions
			if (upperName.Contains("DC") || upperName.Contains("HDC"))
			{
				return true;
			}

			// Menu, icon, cursor functions
			if (upperName.Contains("MENU") || upperName.Contains("ICON") || upperName.Contains("CURSOR"))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Retrieves a handle to the desktop window.
		/// </summary>
		[DllModuleExport(0)]
		private uint GetDesktopWindow()
		{
			_logger.LogInformation("[User32] GetDesktopWindow()");
			return 0x00010001; // Fake desktop handle
		}

		/// <summary>
		/// Changes the check state of a button control.
		/// </summary>
		[DllModuleExport(12)]
		private uint CheckDlgButton(uint hDlg, int nIDButton, uint uCheck)
		{
			_logger.LogInformation("[User32] CheckDlgButton(hDlg=0x{HDlg:X8}, nIDButton={NIDButton}, uCheck={UCheck})",
				hDlg, nIDButton, uCheck);
			return 1; // TRUE
		}

		/// <summary>
		/// Adds a check mark to a specified radio button in a group.
		/// BOOL CheckRadioButton(
		///   [in] HWND hDlg,
		///   [in] int  nIDFirstButton,
		///   [in] int  nIDLastButton,
		///   [in] int  nIDCheckButton
		/// );
		/// </summary>
		[DllModuleExport(16)]
		private uint CheckRadioButton(uint hDlg, int nIDFirstButton, int nIDLastButton, int nIDCheckButton)
		{
			_logger.LogInformation("[User32] CheckRadioButton(hDlg=0x{HDlg:X8}, nIDFirstButton={NIDFirstButton}, nIDLastButton={NIDLastButton}, nIDCheckButton={NIDCheckButton})",
				hDlg, nIDFirstButton, nIDLastButton, nIDCheckButton);

			// Uncheck all radio buttons in the range
			for (int id = nIDFirstButton; id <= nIDLastButton; id++)
			{
				var hwndButton = GetDlgItem(hDlg, id);
				if (hwndButton != 0)
				{
					SendDlgItemMessageA(hDlg, id, (uint)ButtonMessage.BM_SETCHECK, (uint)ButtonState.BST_UNCHECKED, 0);
				}
			}

			// Check the specified button
			if (nIDCheckButton >= nIDFirstButton && nIDCheckButton <= nIDLastButton)
			{
				var hwndCheck = GetDlgItem(hDlg, nIDCheckButton);
				if (hwndCheck != 0)
				{
					SendDlgItemMessageA(hDlg, nIDCheckButton, (uint)ButtonMessage.BM_SETCHECK, (uint)ButtonState.BST_CHECKED, 0);
					return 1; // TRUE - success
				}
			}

			// Return FALSE if the check button wasn't in range or doesn't exist
			return 0;
		}

		/// <summary>
		/// Determines whether a button control has a check mark.
		/// UINT IsDlgButtonChecked(
		///   [in] HWND hDlg,
		///   [in] int  nIDButton
		/// );
		/// Returns: BST_UNCHECKED (0), BST_CHECKED (1), or BST_INDETERMINATE (2)
		/// </summary>
		[DllModuleExport(8)]
		private uint IsDlgButtonChecked(uint hDlg, int nIDButton)
		{
			_logger.LogInformation("[User32] IsDlgButtonChecked(hDlg=0x{HDlg:X8}, nIDButton={NIDButton})",
				hDlg, nIDButton);

			// Get the control handle
			var hwndButton = GetDlgItem(hDlg, nIDButton);
			if (hwndButton == 0)
			{
				_logger.LogWarning("[User32] IsDlgButtonChecked: Button not found for ID {NIDButton}", nIDButton);
				return (uint)ButtonState.BST_UNCHECKED;
			}

			// Send BM_GETCHECK message to get the check state
			var checkState = SendDlgItemMessageA(hDlg, nIDButton, (uint)ButtonMessage.BM_GETCHECK, 0, 0);
			_logger.LogInformation("[User32] IsDlgButtonChecked: Button {NIDButton} has state {CheckState}", nIDButton, checkState);
			return checkState;
		}

		/// <summary>
		/// Waits until objects are signaled or time-out interval elapses.
		/// </summary>
		[DllModuleExport(20, IsStub = true)]
		private uint MsgWaitForMultipleObjects(uint nCount, uint pHandles, uint fWaitAll, uint dwMilliseconds, uint dwWakeMask)
		{
			_logger.LogInformation("[User32] MsgWaitForMultipleObjects(nCount={NCount}, dwMilliseconds={DwMilliseconds})",
				nCount, dwMilliseconds);
			return 0; // WAIT_OBJECT_0
		}

		[DllModuleExport(0)]
		private uint GetFocus()
		{
			_logger.LogInformation("[User32] GetFocus()");
			return _focusWindow;
		}

		[DllModuleExport(0)]
		private uint GetDlgItemInt(uint hDlg, int nIDDlgItem, uint lpTranslated, uint bSigned)
		{
			_logger.LogInformation("[User32] GetDlgItemInt(hDlg=0x{HDlg:X8}, nIDDlgItem={NIDDlgItem})", hDlg, nIDDlgItem);

			// Stub implementation - return 0
			if (lpTranslated != 0)
			{
				_env.MemWrite32(lpTranslated, 1); // TRUE - translation successful
			}
			return 0;
		}

		[DllModuleExport(0)]
		private uint SetDlgItemInt(uint hDlg, int nIDDlgItem, uint uValue, uint bSigned)
		{
			_logger.LogInformation("[User32] SetDlgItemInt(hDlg=0x{HDlg:X8}, nIDDlgItem={NIDDlgItem}, uValue={UValue})",
				hDlg, nIDDlgItem, uValue);
			return 1; // TRUE
		}

		[DllModuleExport(0)]
		private uint CreateDialogParamA(uint hInstance, uint lpTemplate, uint hWndParent, uint lpDialogFunc, uint dwInitParam)
		{
			_logger.LogInformation("[User32] CreateDialogParamA(hInstance=0x{HInstance:X8}, lpTemplate=0x{LpTemplate:X8})",
				hInstance, lpTemplate);

			// Stub implementation - similar to DialogBoxParamA but modeless
			// Create a basic window for the dialog
			var hwnd = _env.CreateWindow(
				"#32770", // Dialog class
				"Dialog", // Title
				0x80000000, // WS_POPUP
				0, // No extended style
				100, 100, 300, 200, // Position and size
				hWndParent, 0, hInstance, 0
			);

			return hwnd;
		}

		[DllModuleExport(0)]
		private uint OffsetRect(uint lprc, int dx, int dy)
		{
			_logger.LogInformation("[User32] OffsetRect(lprc=0x{Lprc:X8}, dx={Dx}, dy={Dy})", lprc, dx, dy);

			if (lprc != 0)
			{
				var rect = new RectRef(_env.Memory, lprc);

				// Offset the rectangle
				rect.left += dx;
				rect.top += dy;
				rect.right += dx;
				rect.bottom += dy;


			}

			return 1; // TRUE
		}

		[DllModuleExport(0)]
		private uint InflateRect(uint lprc, int dx, int dy)
		{
			_logger.LogInformation("[User32] InflateRect(lprc=0x{Lprc:X8}, dx={Dx}, dy={Dy})", lprc, dx, dy);

			if (lprc != 0)
			{
				var rect = new RectRef(_env.Memory, lprc);

				// Inflate the rectangle
				rect.left -= dx;
				rect.top -= dy;
				rect.right += dx;
				rect.bottom += dy;
			}

			return 1; // TRUE
		}

		[DllModuleExport(0)]
		private uint InvalidateRect(uint hWnd, uint lpRect, uint bErase)
		{
			_logger.LogInformation("[User32] InvalidateRect(hWnd=0x{HWnd:X8}, lpRect=0x{LpRect:X8}, bErase={BErase})",
				hWnd, lpRect, bErase);
			// Stub - always succeed
			return 1; // TRUE
		}

		[DllModuleExport(0)]
		private uint SetTimer(uint hWnd, uint nIDEvent, uint uElapse, uint lpTimerFunc)
		{
			_logger.LogInformation("[User32] SetTimer(hWnd=0x{HWnd:X8}, nIDEvent={NIDEvent}, uElapse={UElapse}ms, lpTimerFunc=0x{LpTimerFunc:X8})",
				hWnd, nIDEvent, uElapse, lpTimerFunc);

			// Determine the timer ID
			uint timerId;
			if (nIDEvent != 0)
			{
				timerId = nIDEvent;
			}
			else
			{
				// If nIDEvent is zero, allocate a new unique timer ID using thread-safe increment
				do
				{
					timerId = Interlocked.Increment(ref _nextTimerId) - 1;
				} while (_timers.ContainsKey(timerId));
			}

			// Create timer info and store it
			var timerInfo = new TimerInfo(
				TimerId: timerId,
				HWnd: hWnd,
				Elapse: uElapse,
				TimerProc: lpTimerFunc
			);

			_timers[timerId] = timerInfo;

			_logger.LogInformation("[User32] SetTimer: Created timer ID={TimerId}, callback=0x{Callback:X8}",
				timerId, lpTimerFunc);

			// Note: The timer is now registered but won't fire automatically without a timer scheduler.
			// Applications can query or manually trigger timers through other mechanisms.
			// The CallTimerProcAsync method is ready to be invoked when the timer fires.

			return timerId;
		}

		/// <summary>
		/// Public method to manually trigger a timer callback.
		/// This can be called by a timer scheduler or for testing purposes.
		/// </summary>
		public async Task FireTimerAsync(uint timerId, CancellationToken cancellationToken = default)
		{
			if (!_timers.TryGetValue(timerId, out var timerInfo))
			{
				_logger.LogWarning("[User32] FireTimerAsync: Timer {TimerId} not found", timerId);
				return;
			}

			// If no callback is provided, generate a WM_TIMER message (0x0113) instead
			if (timerInfo.TimerProc == 0)
			{
				_logger.LogDebug("[User32] FireTimerAsync: Timer {TimerId} has no callback, would post WM_TIMER message", timerId);
				// In a full implementation, would call: PostMessageA(timerInfo.HWnd, 0x0113, timerId, 0);
				return;
			}

			// Get current time (in milliseconds since system start)
			var dwTime = (uint)Environment.TickCount;

			// Call the timer callback using the async pattern
			await CallTimerProcAsync(
				timerInfo.TimerProc,
				timerInfo.HWnd,
				0x0113, // WM_TIMER
				timerId,
				dwTime,
				cancellationToken
			).ConfigureAwait(false);
		}

		[DllModuleExport(0)]
		private uint CharLowerBuffA(in LpStr lpsz, uint cchLength)
		{
			_logger.LogInformation("[User32] CharLowerBuffA(lpsz=0x{Lpsz:X8}, cchLength={CchLength})", lpsz.Address, cchLength);

			if (lpsz.Address != 0 && cchLength > 0)
			{
				// Read the string
				var str = lpsz.Read(_env.Memory, (int)cchLength);
				// Convert to lowercase
				var lower = str.ToLowerInvariant();

				// Ensure the output is exactly cchLength characters
				if (lower.Length > cchLength)
				{
					lower = lower.Substring(0, (int)cchLength);
				}
				else if (lower.Length < cchLength)
				{
					lower = lower.PadRight((int)cchLength, '\0');
				}

				// Write back
				lpsz.Write(_env.Memory, lower, false);
			}

			return cchLength;
		}

		[DllModuleExport(0)]
		private uint GetKeyboardType(int nTypeFlag)
		{
			_logger.LogInformation("[User32] GetKeyboardType(nTypeFlag={NTypeFlag})", nTypeFlag);

			return nTypeFlag switch
			{
				0 => 4, // Keyboard type: Enhanced 101- or 102-key keyboards
				1 => 0, // Keyboard subtype
				2 => 12, // Number of function keys
				_ => 0
			};
		}

		[DllModuleExport(0)]
		private uint EnumDisplaySettingsA(in LpcStr lpszDeviceName, uint iModeNum, uint lpDevMode)
		{
			var deviceName = lpszDeviceName.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] EnumDisplaySettingsA(lpszDeviceName=\"{DeviceName}\", iModeNum={IModeNum})",
				deviceName, iModeNum);

			// DEVMODE constants
			const uint DM_BITSPERPEL = 0x00040000;
			const uint DM_PELSWIDTH = 0x00080000;
			const uint DM_PELSHEIGHT = 0x00100000;
			const uint ENUM_CURRENT_SETTINGS = 0xFFFFFFFF;

			// DEVMODE structure offsets
			const uint DEVMODE_OFFSET_DMFIELDS = 0x40;
			const uint DEVMODE_OFFSET_DMPELSWIDTH = 0x68;
			const uint DEVMODE_OFFSET_DMPELSHEIGHT = 0x6C;
			const uint DEVMODE_OFFSET_DMBITSPERPEL = 0x70;

			// Stub implementation - fill in basic DEVMODE structure
			if (lpDevMode != 0)
			{
				// Write some basic values
				_env.MemWrite32(lpDevMode + DEVMODE_OFFSET_DMPELSWIDTH, 640); // dmPelsWidth
				_env.MemWrite32(lpDevMode + DEVMODE_OFFSET_DMPELSHEIGHT, 480); // dmPelsHeight
				_env.MemWrite32(lpDevMode + DEVMODE_OFFSET_DMBITSPERPEL, 32); // dmBitsPerPel
				_env.MemWrite32(lpDevMode + DEVMODE_OFFSET_DMFIELDS, DM_PELSWIDTH | DM_PELSHEIGHT | DM_BITSPERPEL); // dmFields
			}

			// Return TRUE for mode 0 (current settings), FALSE for others
			return iModeNum == ENUM_CURRENT_SETTINGS || iModeNum == 0 ? 1u : 0u;
		}

		// Additional window management functions
		[DllModuleExport(4)]
		private uint BeginDeferWindowPos(int nNumWindows)
		{
			_logger.LogInformation("[User32] BeginDeferWindowPos(nNumWindows={NNumWindows})", nNumWindows);
			return 0xDEF00001; // Dummy handle
		}

		[DllModuleExport(32)]
		private uint DeferWindowPos(uint hWinPosInfo, uint hWnd, uint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags)
		{
			_logger.LogInformation("[User32] DeferWindowPos(hWinPosInfo=0x{HWinPosInfo:X8}, hWnd=0x{HWnd:X8})", hWinPosInfo, hWnd);
			return hWinPosInfo; // Return same handle
		}

		[DllModuleExport(4)]
		private uint EndDeferWindowPos(uint hWinPosInfo)
		{
			_logger.LogInformation("[User32] EndDeferWindowPos(hWinPosInfo=0x{HWinPosInfo:X8})", hWinPosInfo);
			return 1; // TRUE
		}

		[DllModuleExport(4)]
		private uint BringWindowToTop(uint hWnd)
		{
			_logger.LogInformation("[User32] BringWindowToTop(hWnd=0x{HWnd:X8})", hWnd);
			return 1; // TRUE
		}

		[DllModuleExport(0)]
		private uint GetActiveWindow()
		{
			_logger.LogInformation("[User32] GetActiveWindow()");
			return 0; // NULL
		}

		[DllModuleExport(4)]
		private uint SetActiveWindow(uint hWnd)
		{
			_logger.LogInformation("[User32] SetActiveWindow(hWnd=0x{HWnd:X8})", hWnd);
			return hWnd; // Return same window
		}

		[DllModuleExport(0)]
		private uint GetForegroundWindow()
		{
			_logger.LogInformation("[User32] GetForegroundWindow()");
			return 0; // NULL
		}

		[DllModuleExport(4)]
		private uint SetForegroundWindow(uint hWnd)
		{
			_logger.LogInformation("[User32] SetForegroundWindow(hWnd=0x{HWnd:X8})", hWnd);
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint FindWindowA(in LpcStr lpClassName, in LpcStr lpWindowName)
		{
			var className = lpClassName.ToString() ?? string.Empty;
			var windowName = lpWindowName.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] FindWindowA(lpClassName=\"{ClassName}\", lpWindowName=\"{WindowName}\")", className, windowName);
			return 0; // NULL
		}

		/// <summary>
		/// Enumerates all top-level windows by passing the handle to each window to a callback function.
		/// BOOL EnumWindows(
		///   [in] WNDENUMPROC lpEnumFunc,
		///   [in] LPARAM      lParam
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint EnumWindows(uint lpEnumFunc, uint lParam)
		{
			return EnumWindowsAsync(lpEnumFunc, lParam).GetAwaiter().GetResult();
		}

		private async Task<uint> EnumWindowsAsync(uint lpEnumFunc, uint lParam, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("[User32] EnumWindows(lpEnumFunc=0x{LpEnumFunc:X8}, lParam=0x{LParam:X8})",
				lpEnumFunc, lParam);

			// Validate callback address
			if (lpEnumFunc == 0)
			{
				_logger.LogWarning("[User32] EnumWindows: Callback address is NULL, returning success without enumeration");
				return 1; // TRUE - success (per Windows behavior)
			}

			// Get all window handles from the environment
			var windowHandles = _env.GetAllWindowHandles().ToList();

			if (windowHandles.Count == 0)
			{
				_logger.LogInformation("[User32] EnumWindows: No windows to enumerate, returning success");
				return 1; // TRUE - success
			}

			_logger.LogInformation("[User32] EnumWindows: Enumerating {Count} windows", windowHandles.Count);

			// Enumerate each window and call the callback
			foreach (var hwnd in windowHandles)
			{
				_logger.LogDebug("[User32] EnumWindows: Calling callback for window 0x{HWnd:X8}", hwnd);

				// Call the enumeration callback using the async pattern
				var result = await CallEnumWindowsProcAsync(lpEnumFunc, hwnd, lParam, cancellationToken).ConfigureAwait(false);

				// If callback returns FALSE (0), stop enumeration
				if (result == 0)
				{
					_logger.LogInformation("[User32] EnumWindows: Callback returned FALSE, stopping enumeration");
					return 1; // EnumWindows returns TRUE even if enumeration stopped early
				}
			}

			_logger.LogInformation("[User32] EnumWindows: Enumeration completed successfully");
			return 1; // TRUE - success
		}

		[DllModuleExport(4)]
		private uint GetParent(uint hWnd)
		{
			_logger.LogInformation("[User32] GetParent(hWnd=0x{HWnd:X8})", hWnd);
			return 0; // NULL
		}

		[DllModuleExport(4)]
		private uint GetTopWindow(uint hWnd)
		{
			_logger.LogInformation("[User32] GetTopWindow(hWnd=0x{HWnd:X8})", hWnd);
			return 0; // NULL
		}

		[DllModuleExport(8)]
		private uint GetWindow(uint hWnd, uint uCmd)
		{
			_logger.LogInformation("[User32] GetWindow(hWnd=0x{HWnd:X8}, uCmd={UCmd})", hWnd, uCmd);
			return 0; // NULL
		}

		[DllModuleExport(8)]
		private uint IsChild(uint hWndParent, uint hWnd)
		{
			_logger.LogInformation("[User32] IsChild(hWndParent=0x{HWndParent:X8}, hWnd=0x{HWnd:X8})", hWndParent, hWnd);
			return 0; // FALSE
		}

		[DllModuleExport(4)]
		private uint IsWindow(uint hWnd)
		{
			_logger.LogInformation("[User32] IsWindow(hWnd=0x{HWnd:X8})", hWnd);
			return hWnd != 0 ? 1u : 0u; // TRUE if non-zero
		}

		[DllModuleExport(4)]
		private uint IsWindowEnabled(uint hWnd)
		{
			_logger.LogInformation("[User32] IsWindowEnabled(hWnd=0x{HWnd:X8})", hWnd);
			return _windowEnabledState.TryGetValue(hWnd, out var enabled) && enabled ? 1u : 0u;
		}

		[DllModuleExport(4)]
		private uint IsWindowVisible(uint hWnd)
		{
			_logger.LogInformation("[User32] IsWindowVisible(hWnd=0x{HWnd:X8})", hWnd);
			return 1; // TRUE
		}

		[DllModuleExport(4)]
		private uint IsIconic(uint hWnd)
		{
			_logger.LogInformation("[User32] IsIconic(hWnd=0x{HWnd:X8})", hWnd);
			return 0; // FALSE
		}

		[DllModuleExport(24)]
		private uint MoveWindow(uint hWnd, int x, int y, int nWidth, int nHeight, uint bRepaint)
		{
			_logger.LogInformation("[User32] MoveWindow(hWnd=0x{HWnd:X8}, x={X}, y={Y}, nWidth={NWidth}, nHeight={NHeight})", hWnd, x, y, nWidth, nHeight);
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint ShowOwnedPopups(uint hWnd, uint fShow)
		{
			_logger.LogInformation("[User32] ShowOwnedPopups(hWnd=0x{HWnd:X8}, fShow={FShow})", hWnd, fShow);
			return 1; // TRUE
		}

		// String and character functions
		[DllModuleExport(4)]
		private uint CharUpperA(uint lpsz)
		{
			_logger.LogInformation("[User32] CharUpperA(lpsz=0x{Lpsz:X8})", lpsz);
			return lpsz; // Return as-is (stub)
		}

		/// <summary>
		/// Determines whether a character is an alphabetic character.
		/// BOOL IsCharAlphaA(
		///   [in] CHAR ch
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint IsCharAlphaA(uint ch)
		{
			_logger.LogInformation("[User32] IsCharAlphaA(ch=0x{Ch:X8})", ch);

			// Extract the character (lower byte)
			char c = (char)(ch & 0xFF);

			// Check if it's an alphabetic character
			if (char.IsLetter(c))
			{
				return 1; // TRUE
			}

			return 0; // FALSE
		}

		[DllModuleExport(20)]
		private int DrawTextA(uint hdc, in LpcStr lpchText, int cchText, uint lprc, uint format)
		{
			var text = lpchText.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] DrawTextA(hdc=0x{Hdc:X8}, lpchText=\"{Text}\", format=0x{Format:X})", hdc, text, format);
			return text.Length; // Return text length
		}

		[DllModuleExport(36)]
		private uint GrayStringA(uint hDC, uint hBrush, uint lpOutputFunc, uint lpData, int nCount, int X, int Y, int nWidth, int nHeight)
		{
			_logger.LogInformation("[User32] GrayStringA(hDC=0x{HDC:X8}, X={X}, Y={Y})", hDC, X, Y);
			return 1; // TRUE
		}

		[DllModuleExport(32)]
		private uint TabbedTextOutA(uint hdc, int x, int y, in LpcStr lpString, int chCount, int nTabPositions, uint lpnTabStopPositions, int nTabOrigin)
		{
			var str = lpString.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] TabbedTextOutA(hdc=0x{Hdc:X8}, x={X}, y={Y}, lpString=\"{Str}\")", hdc, x, y, str);
			return 0; // Return 0 (stub)
		}

		[DllModuleExport(4)]
		private int GetWindowTextLengthA(uint hWnd)
		{
			_logger.LogInformation("[User32] GetWindowTextLengthA(hWnd=0x{HWnd:X8})", hWnd);
			return 0;
		}

		// Menu functions
		[DllModuleExport(12)]
		private uint CheckMenuItem(uint hMenu, uint uIDCheckItem, uint uCheck)
		{
			_logger.LogInformation("[User32] CheckMenuItem(hMenu=0x{HMenu:X8}, uIDCheckItem={UIDCheckItem}, uCheck=0x{UCheck:X})", hMenu, uIDCheckItem, uCheck);
			return 0; // Return previous state (unchecked)
		}

		[DllModuleExport(12)]
		private uint EnableMenuItem(uint hMenu, uint uIDEnableItem, uint uEnable)
		{
			_logger.LogInformation("[User32] EnableMenuItem(hMenu=0x{HMenu:X8}, uIDEnableItem={UIDEnableItem}, uEnable=0x{UEnable:X})", hMenu, uIDEnableItem, uEnable);
			return 0; // Return previous state
		}

		[DllModuleExport(4)]
		private int GetMenuItemCount(uint hMenu)
		{
			_logger.LogInformation("[User32] GetMenuItemCount(hMenu=0x{HMenu:X8})", hMenu);
			return 0;
		}

		[DllModuleExport(8)]
		private uint GetMenuItemID(uint hMenu, int nPos)
		{
			_logger.LogInformation("[User32] GetMenuItemID(hMenu=0x{HMenu:X8}, nPos={NPos})", hMenu, nPos);
			return 0xFFFFFFFF; // Return -1 (error)
		}

		[DllModuleExport(12)]
		private uint GetMenuState(uint hMenu, uint uId, uint uFlags)
		{
			_logger.LogInformation("[User32] GetMenuState(hMenu=0x{HMenu:X8}, uId={UId}, uFlags=0x{UFlags:X})", hMenu, uId, uFlags);
			return 0xFFFFFFFF; // Return -1 (error)
		}

		[DllModuleExport(8)]
		private uint GetSubMenu(uint hMenu, int nPos)
		{
			_logger.LogInformation("[User32] GetSubMenu(hMenu=0x{HMenu:X8}, nPos={NPos})", hMenu, nPos);
			return 0; // NULL
		}

		[DllModuleExport(20)]
		private uint ModifyMenuA(uint hMnu, uint uPosition, uint uFlags, uint uIDNewItem, in LpcStr lpNewItem)
		{
			var newItem = lpNewItem.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] ModifyMenuA(hMnu=0x{HMnu:X8}, uPosition={UPosition}, uFlags=0x{UFlags:X})", hMnu, uPosition, uFlags);
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint SetMenu(uint hWnd, uint hMenu)
		{
			_logger.LogInformation("[User32] SetMenu(hWnd=0x{HWnd:X8}, hMenu=0x{HMenu:X8})", hWnd, hMenu);
			return 1; // TRUE
		}

		[DllModuleExport(20)]
		private uint SetMenuItemBitmaps(uint hMenu, uint uPosition, uint uFlags, uint hBitmapUnchecked, uint hBitmapChecked)
		{
			_logger.LogInformation("[User32] SetMenuItemBitmaps(hMenu=0x{HMenu:X8}, uPosition={UPosition})", hMenu, uPosition);
			return 1; // TRUE
		}

		[DllModuleExport(4)]
		private uint DestroyMenu(uint hMenu)
		{
			_logger.LogInformation("[User32] DestroyMenu(hMenu=0x{HMenu:X8})", hMenu);
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint LoadMenuA(uint hInstance, in LpcStr lpMenuName)
		{
			var menuName = lpMenuName.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] LoadMenuA(hInstance=0x{HInstance:X8}, lpMenuName=\"{MenuName}\")", hInstance, menuName);
			return 0xABCD0000; // Dummy menu handle
		}

		[DllModuleExport(0)]
		private uint GetMenuCheckMarkDimensions()
		{
			_logger.LogInformation("[User32] GetMenuCheckMarkDimensions()");
			return 0x000D000D; // 13x13 pixels (MAKELONG(13, 13))
		}

		/// <summary>
		/// Deletes a menu item or detaches a submenu from the specified menu.
		/// BOOL RemoveMenu(
		///   [in] HMENU hMenu,
		///   [in] UINT  uPosition,
		///   [in] UINT  uFlags
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint RemoveMenu(uint hMenu, uint uPosition, uint uFlags)
		{
			_logger.LogInformation("[User32] RemoveMenu(hMenu=0x{HMenu:X8}, uPosition={UPosition}, uFlags=0x{UFlags:X})",
				hMenu, uPosition, uFlags);

			// RemoveMenu removes a menu item from a menu
			// uFlags can be:
			// MF_BYCOMMAND (0x0000) - uPosition is menu item ID
			// MF_BYPOSITION (0x0400) - uPosition is zero-based position

			// For stub implementation, just acknowledge the removal
			return 1; // TRUE
		}

		/// <summary>
		/// Redraws the menu bar of the specified window.
		/// BOOL DrawMenuBar(
		///   [in] HWND hWnd
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint DrawMenuBar(uint hWnd)
		{
			_logger.LogInformation("[User32] DrawMenuBar(hWnd=0x{HWnd:X8})", hWnd);

			// DrawMenuBar redraws the menu bar after changes
			// For stub implementation, just acknowledge the redraw
			return 1; // TRUE
		}

		// Rectangle functions
		[DllModuleExport(8)]
		private uint CopyRect(uint lprcDst, uint lprcSrc)
		{
			_logger.LogInformation("[User32] CopyRect(lprcDst=0x{LprcDst:X8}, lprcSrc=0x{LprcSrc:X8})", lprcDst, lprcSrc);
			if (lprcDst != 0 && lprcSrc != 0)
			{
				for (int i = 0; i < 16; i += 4)
				{
					_env.MemWrite32(lprcDst + (uint)i, _env.MemRead32(lprcSrc + (uint)i));
				}
			}
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint EqualRect(uint lprc1, uint lprc2)
		{
			_logger.LogInformation("[User32] EqualRect(lprc1=0x{Lprc1:X8}, lprc2=0x{Lprc2:X8})", lprc1, lprc2);
			return 0; // FALSE (stub)
		}

		[DllModuleExport(12)]
		private uint PtInRect(uint lprc, int ptX, int ptY)
		{
			_logger.LogInformation("[User32] PtInRect(lprc=0x{Lprc:X8}, pt=({PtX}, {PtY}))", lprc, ptX, ptY);
			return 0; // FALSE (stub)
		}

		[DllModuleExport(4)]
		private uint SetRectEmpty(uint lprc)
		{
			_logger.LogInformation("[User32] SetRectEmpty(lprc=0x{Lprc:X8})", lprc);
			if (lprc != 0)
			{
				_env.MemWrite32(lprc, 0);      // left
				_env.MemWrite32(lprc + 4, 0);  // top
				_env.MemWrite32(lprc + 8, 0);  // right
				_env.MemWrite32(lprc + 12, 0); // bottom
			}
			return 1; // TRUE
		}

		// Scrollbar functions
		[DllModuleExport(8)]
		private int GetScrollPos(uint hWnd, int nBar)
		{
			_logger.LogInformation("[User32] GetScrollPos(hWnd=0x{HWnd:X8}, nBar={NBar})", hWnd, nBar);
			return 0;
		}

		[DllModuleExport(16)]
		private uint GetScrollRange(uint hWnd, int nBar, uint lpMinPos, uint lpMaxPos)
		{
			_logger.LogInformation("[User32] GetScrollRange(hWnd=0x{HWnd:X8}, nBar={NBar})", hWnd, nBar);
			if (lpMinPos != 0)
			{
				_env.MemWrite32(lpMinPos, 0);
			}

			if (lpMaxPos != 0)
			{
				_env.MemWrite32(lpMaxPos, 0);
			}

			return 1; // TRUE
		}

		[DllModuleExport(16)]
		private int SetScrollPos(uint hWnd, int nBar, int nPos, uint bRedraw)
		{
			_logger.LogInformation("[User32] SetScrollPos(hWnd=0x{HWnd:X8}, nBar={NBar}, nPos={NPos})", hWnd, nBar, nPos);
			return 0; // Return previous position
		}

		[DllModuleExport(20)]
		private uint SetScrollRange(uint hWnd, int nBar, int nMinPos, int nMaxPos, uint bRedraw)
		{
			_logger.LogInformation("[User32] SetScrollRange(hWnd=0x{HWnd:X8}, nBar={NBar}, nMinPos={NMinPos}, nMaxPos={NMaxPos})", hWnd, nBar, nMinPos, nMaxPos);
			return 1; // TRUE
		}

		[DllModuleExport(12)]
		private uint ShowScrollBar(uint hWnd, int wBar, uint bShow)
		{
			_logger.LogInformation("[User32] ShowScrollBar(hWnd=0x{HWnd:X8}, wBar={WBar}, bShow={BShow})", hWnd, wBar, bShow);
			return 1; // TRUE
		}

		// Dialog functions
		[DllModuleExport(20)]
		private uint CreateDialogIndirectParamA(uint hInstance, uint lpTemplate, uint hWndParent, uint lpDialogFunc, uint dwInitParam)
		{
			_logger.LogInformation("[User32] CreateDialogIndirectParamA(hInstance=0x{HInstance:X8}, hWndParent=0x{HWndParent:X8})", hInstance, hWndParent);
			return 0; // NULL (stub)
		}

		[DllModuleExport(4)]
		private int GetDlgCtrlID(uint hWnd)
		{
			_logger.LogInformation("[User32] GetDlgCtrlID(hWnd=0x{HWnd:X8})", hWnd);
			return 0;
		}

		[DllModuleExport(12)]
		private uint GetNextDlgTabItem(uint hDlg, uint hCtl, uint bPrevious)
		{
			_logger.LogInformation("[User32] GetNextDlgTabItem(hDlg=0x{HDlg:X8}, hCtl=0x{HCtl:X8}, bPrevious={BPrevious})", hDlg, hCtl, bPrevious);
			return 0; // NULL
		}

		[DllModuleExport(8)]
		private uint IsDialogMessageA(uint hDlg, uint lpMsg)
		{
			_logger.LogInformation("[User32] IsDialogMessageA(hDlg=0x{HDlg:X8}, lpMsg=0x{LpMsg:X8})", hDlg, lpMsg);
			return 0; // FALSE
		}

		// Input and keyboard functions
		[DllModuleExport(4)]
		private short GetAsyncKeyState(int vKey)
		{
			_logger.LogInformation("[User32] GetAsyncKeyState(vKey={VKey})", vKey);
			return 0; // Key not pressed
		}

		[DllModuleExport(4)]
		private short GetKeyState(int nVirtKey)
		{
			_logger.LogInformation("[User32] GetKeyState(nVirtKey={NVirtKey})", nVirtKey);
			return 0;
		}

		[DllModuleExport(4)]
		private uint GetKeyboardState(uint lpKeyState)
		{
			_logger.LogInformation("[User32] GetKeyboardState(lpKeyState=0x{LpKeyState:X8})", lpKeyState);

			if (lpKeyState == 0)
			{
				return 0; // FALSE
			}

			// Keyboard state is an array of 256 bytes, one for each virtual key
			// Clear all keys to "not pressed" state
			for (uint i = 0; i < 256; i++)
			{
				_env.MemWrite8(lpKeyState + i, 0);
			}

			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint MapVirtualKeyA(uint uCode, uint uMapType)
		{
			_logger.LogInformation("[User32] MapVirtualKeyA(uCode={UCode}, uMapType={UMapType})", uCode, uMapType);

			// MapVirtualKey maps virtual key codes to scan codes or vice versa
			// uMapType:
			// 0 = virtual key to scan code
			// 1 = scan code to virtual key
			// 2 = virtual key to unshifted character
			// 3 = scan code to virtual key (extended keys)

			// Stub: return 0 (unmapped)
			return 0;
		}

		[DllModuleExport(20)]
		private int ToAscii(uint uVirtKey, uint uScanCode, uint lpKeyState, uint lpChar, uint uFlags)
		{
			_logger.LogInformation("[User32] ToAscii(uVirtKey={UVirtKey}, uScanCode={UScanCode}, lpKeyState=0x{LpKeyState:X8}, lpChar=0x{LpChar:X8}, uFlags={UFlags})",
				uVirtKey, uScanCode, lpKeyState, lpChar, uFlags);

			// ToAscii translates a virtual key code and keyboard state to ASCII character(s)
			// Returns:
			// -1 = dead key
			// 0 = no translation
			// 1 = one character translated
			// 2 = two characters translated

			// Stub: return 0 (no translation)
			if (lpChar != 0)
			{
				_env.MemWrite16(lpChar, 0);
			}
			return 0;
		}

		[DllModuleExport(20)]
		private int ToUnicode(uint wVirtKey, uint wScanCode, uint lpKeyState, uint pwszBuff, int cchBuff, uint wFlags)
		{
			_logger.LogInformation("[User32] ToUnicode(wVirtKey={WVirtKey}, wScanCode={WScanCode}, lpKeyState=0x{LpKeyState:X8}, pwszBuff=0x{PwszBuff:X8}, cchBuff={CchBuff}, wFlags={WFlags})",
				wVirtKey, wScanCode, lpKeyState, pwszBuff, cchBuff, wFlags);

			// ToUnicode translates a virtual key code and keyboard state to Unicode character(s)
			// Returns:
			// -1 = dead key
			// 0 = no translation
			// >0 = number of characters translated

			// Stub: return 0 (no translation)
			if (pwszBuff != 0 && cchBuff > 0)
			{
				_env.MemWrite16(pwszBuff, 0);
			}
			return 0;
		}

		[DllModuleExport(0)]
		private uint GetCapture()
		{
			_logger.LogInformation("[User32] GetCapture()");
			return 0; // NULL
		}

		[DllModuleExport(4)]
		private uint SetCapture(uint hWnd)
		{
			_logger.LogInformation("[User32] SetCapture(hWnd=0x{HWnd:X8})", hWnd);
			return 0; // Return previous capture window (NULL)
		}

		[DllModuleExport(0)]
		private uint ReleaseCapture()
		{
			_logger.LogInformation("[User32] ReleaseCapture()");
			return 1; // TRUE
		}

		[DllModuleExport(4)]
		private uint GetCursorPos(uint lpPoint)
		{
			_logger.LogInformation("[User32] GetCursorPos(lpPoint=0x{LpPoint:X8})", lpPoint);
			if (lpPoint != 0)
			{
				_env.MemWrite32(lpPoint, 0);     // x
				_env.MemWrite32(lpPoint + 4, 0); // y
			}
			return 1; // TRUE
		}

		// Message functions
		[DllModuleExport(0)]
		private uint GetMessagePos()
		{
			_logger.LogInformation("[User32] GetMessagePos()");
			return 0; // MAKELONG(0, 0)
		}

		[DllModuleExport(0)]
		private int GetMessageTime()
		{
			_logger.LogInformation("[User32] GetMessageTime()");
			return (int)Environment.TickCount;
		}

		[DllModuleExport(20)]
		private uint CallWindowProcA(uint lpPrevWndFunc, uint hWnd, uint Msg, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] CallWindowProcA(lpPrevWndFunc=0x{LpPrevWndFunc:X8}, hWnd=0x{HWnd:X8}, Msg=0x{Msg:X}, wParam=0x{WParam:X8}, lParam=0x{LParam:X8})",
				lpPrevWndFunc, hWnd, Msg, wParam, lParam);
			return 0; // Default return value
		}

		// Window property functions
		[DllModuleExport(8)]
		private uint GetPropA(uint hWnd, in LpcStr lpString)
		{
			var str = lpString.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] GetPropA(hWnd=0x{HWnd:X8}, lpString=\"{Str}\")", hWnd, str);
			return 0; // NULL
		}

		[DllModuleExport(12)]
		private uint SetPropA(uint hWnd, in LpcStr lpString, uint hData)
		{
			var str = lpString.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] SetPropA(hWnd=0x{HWnd:X8}, lpString=\"{Str}\", hData=0x{HData:X8})", hWnd, str, hData);
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint RemovePropA(uint hWnd, in LpcStr lpString)
		{
			var str = lpString.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] RemovePropA(hWnd=0x{HWnd:X8}, lpString=\"{Str}\")", hWnd, str);
			return 0; // NULL
		}

		// Icon and cursor functions
		[DllModuleExport(8)]
		private uint LoadBitmapA(uint hInstance, in LpcStr lpBitmapName)
		{
			var bitmapName = lpBitmapName.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] LoadBitmapA(hInstance=0x{HInstance:X8}, lpBitmapName=\"{BitmapName}\")", hInstance, bitmapName);

			// LoadBitmapA is a legacy function that's essentially LoadImageA with IMAGE_BITMAP
			// and no special flags (default behavior)

			// Convert LpcStr to LpStr for LoadImageA
			var namePtr = new LpStr(lpBitmapName.Address);
			return LoadImageA(hInstance, namePtr, (uint)ImageType.IMAGE_BITMAP, 0, 0, (uint)LoadImageFlags.LR_DEFAULTCOLOR);
		}

		[DllModuleExport(16)]
		private uint DrawIcon(uint hDC, int X, int Y, uint hIcon)
		{
			_logger.LogInformation("[User32] DrawIcon(hDC=0x{HDC:X8}, X={X}, Y={Y}, hIcon=0x{HIcon:X8})", hDC, X, Y, hIcon);
			return 1; // TRUE
		}

		[DllModuleExport(4)]
		private uint DestroyIcon(uint hIcon)
		{
			_logger.LogInformation("[User32] DestroyIcon(hIcon=0x{HIcon:X8})", hIcon);
			return 1; // TRUE
		}

		// Coordinate mapping
		[DllModuleExport(16)]
		private int MapWindowPoints(uint hWndFrom, uint hWndTo, uint lpPoints, uint cPoints)
		{
			_logger.LogInformation("[User32] MapWindowPoints(hWndFrom=0x{HWndFrom:X8}, hWndTo=0x{HWndTo:X8}, cPoints={CPoints})", hWndFrom, hWndTo, cPoints);
			return 0; // Return 0 offset
		}

		[DllModuleExport(8)]
		private uint ScreenToClient(uint hWnd, uint lpPoint)
		{
			_logger.LogInformation("[User32] ScreenToClient(hWnd=0x{HWnd:X8}, lpPoint=0x{LpPoint:X8})", hWnd, lpPoint);
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint WindowFromPoint(int xPoint, int yPoint)
		{
			_logger.LogInformation("[User32] WindowFromPoint(x={XPoint}, y={YPoint})", xPoint, yPoint);
			return 0; // NULL
		}

		// System functions
		[DllModuleExport(4)]
		private uint GetSysColor(int nIndex)
		{
			_logger.LogInformation("[User32] GetSysColor(nIndex={NIndex})", nIndex);
			return 0xFFFFFFFF; // White color (stub)
		}

		[DllModuleExport(4)]
		private uint GetSysColorBrush(int nIndex)
		{
			_logger.LogInformation("[User32] GetSysColorBrush(nIndex={NIndex})", nIndex);
			return 0x0BF50000; // Dummy brush handle
		}

		[DllModuleExport(4)]
		private uint MessageBeep(uint uType)
		{
			_logger.LogInformation("[User32] MessageBeep(uType=0x{UType:X})", uType);
			return 1; // TRUE
		}

		// Class functions
		[DllModuleExport(12)]
		private uint GetClassInfoA(uint hInstance, in LpcStr lpClassName, uint lpWndClass)
		{
			var className = lpClassName.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] GetClassInfoA(hInstance=0x{HInstance:X8}, lpClassName=\"{ClassName}\")", hInstance, className);
			return 0; // FALSE
		}

		[DllModuleExport(12)]
		private int GetClassNameA(uint hWnd, in LpStr lpClassName, int nMaxCount)
		{
			_logger.LogInformation("[User32] GetClassNameA(hWnd=0x{HWnd:X8}, nMaxCount={NMaxCount})", hWnd, nMaxCount);
			if (lpClassName.Address != 0 && nMaxCount > 0)
			{
				lpClassName.Write(_env.Memory, "Window", true);
				return 6; // Length of "Window"
			}
			return 0;
		}

		[DllModuleExport(8)]
		private uint UnregisterClassA(in LpcStr lpClassName, uint hInstance)
		{
			var className = lpClassName.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] UnregisterClassA(lpClassName=\"{ClassName}\", hInstance=0x{HInstance:X8})", className, hInstance);
			return 1; // TRUE
		}

		// Redraw functions
		[DllModuleExport(16)]
		private uint RedrawWindow(uint hWnd, uint lprcUpdate, uint hrgnUpdate, uint flags)
		{
			_logger.LogInformation("[User32] RedrawWindow(hWnd=0x{HWnd:X8}, flags=0x{Flags:X})", hWnd, flags);
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint ValidateRect(uint hWnd, uint lpRect)
		{
			_logger.LogInformation("[User32] ValidateRect(hWnd=0x{HWnd:X8}, lpRect=0x{LpRect:X8})", hWnd, lpRect);
			return 1; // TRUE
		}

		// Accelerator and hook functions
		[DllModuleExport(8)]
		private uint LoadAcceleratorsA(uint hInstance, in LpcStr lpTableName)
		{
			var tableName = lpTableName.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] LoadAcceleratorsA(hInstance=0x{HInstance:X8}, lpTableName=\"{TableName}\")", hInstance, tableName);
			return 0; // NULL
		}

		[DllModuleExport(12)]
		private int TranslateAcceleratorA(uint hWnd, uint hAccTable, uint lpMsg)
		{
			_logger.LogInformation("[User32] TranslateAcceleratorA(hWnd=0x{HWnd:X8}, hAccTable=0x{HAccTable:X8})", hWnd, hAccTable);
			return 0; // No accelerator processed
		}

		[DllModuleExport(16)]
		private uint SetWindowsHookExA(int idHook, uint lpfn, uint hMod, uint dwThreadId)
		{
			_logger.LogInformation("[User32] SetWindowsHookExA(idHook={IdHook}, lpfn=0x{Lpfn:X8}, hMod=0x{HMod:X8}, dwThreadId={DwThreadId})",
				idHook, lpfn, hMod, dwThreadId);

			// Validate hook procedure address
			if (lpfn == 0)
			{
				_logger.LogWarning("[User32] SetWindowsHookExA: Hook procedure address is NULL");
				return 0; // NULL - failure
			}

			// Generate a unique hook handle using thread-safe increment
			var hookHandle = Interlocked.Increment(ref _nextHookHandle) - 1;

			// Create hook info and store it
			var hookInfo = new HookInfo(
				HookHandle: hookHandle,
				IdHook: idHook,
				HookProc: lpfn,
				HMod: hMod,
				ThreadId: dwThreadId
			);

			_hooks[hookHandle] = hookInfo;

			_logger.LogInformation("[User32] SetWindowsHookExA: Installed hook handle=0x{HookHandle:X8}, type={IdHook}, proc=0x{Proc:X8}",
				hookHandle, idHook, lpfn);

			// Note: The hook is now registered but won't be called automatically without integration into message processing.
			// The CallHookProcAsync method is ready to be invoked when the hook should fire.

			return hookHandle;
		}

		/// <summary>
		/// Public method to manually trigger a hook callback.
		/// This can be called during message processing or for testing purposes.
		/// </summary>
		public async Task<uint> CallHookAsync(uint hookHandle, int nCode, uint wParam, uint lParam, CancellationToken cancellationToken = default)
		{
			if (!_hooks.TryGetValue(hookHandle, out var hookInfo))
			{
				_logger.LogWarning("[User32] CallHookAsync: Hook 0x{HookHandle:X8} not found", hookHandle);
				return 0;
			}

			// Call the hook callback using the async pattern
			return await CallHookProcAsync(
				hookInfo.HookProc,
				nCode,
				wParam,
				lParam,
				cancellationToken
			).ConfigureAwait(false);
		}

		[DllModuleExport(4)]
		private uint UnhookWindowsHookEx(uint hhk)
		{
			_logger.LogInformation("[User32] UnhookWindowsHookEx(hhk=0x{Hhk:X8})", hhk);

			// Remove the hook from tracking if it exists
			if (_hooks.TryRemove(hhk, out _))
			{
				_logger.LogInformation("[User32] UnhookWindowsHookEx: Removed hook 0x{HookHandle:X8}", hhk);
			}
			else
			{
				_logger.LogDebug("[User32] UnhookWindowsHookEx: Hook 0x{HookHandle:X8} not found (may have already been removed)", hhk);
			}

			// Always return success for simplicity (matching Windows behavior of being lenient)
			return 1; // TRUE - success
		}

		[DllModuleExport(16)]
		private uint CallNextHookEx(uint hhk, int nCode, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] CallNextHookEx(hhk=0x{Hhk:X8}, nCode={NCode})", hhk, nCode);
			return 0;
		}

		// DDE functions
		[DllModuleExport(20)]
		private uint ReuseDDElParam(uint lParam, uint msgIn, uint msgOut, uint uiLo, uint uiHi)
		{
			_logger.LogInformation("[User32] ReuseDDElParam(lParam=0x{LParam:X8}, msgIn=0x{MsgIn:X}, msgOut=0x{MsgOut:X})", lParam, msgIn, msgOut);
			return 0; // Stub
		}

		[DllModuleExport(16)]
		private uint UnpackDDElParam(uint msg, uint lParam, uint puiLo, uint puiHi)
		{
			_logger.LogInformation("[User32] UnpackDDElParam(msg=0x{Msg:X}, lParam=0x{LParam:X8})", msg, lParam);
			if (puiLo != 0)
			{
				_env.MemWrite32(puiLo, 0);
			}

			if (puiHi != 0)
			{
				_env.MemWrite32(puiHi, 0);
			}

			return 1; // TRUE
		}

		// Timer functions
		[DllModuleExport(8)]
		private uint KillTimer(uint hWnd, uint uIDEvent)
		{
			_logger.LogInformation("[User32] KillTimer(hWnd=0x{HWnd:X8}, uIDEvent={UIDEvent})", hWnd, uIDEvent);

			// Remove the timer from tracking if it exists
			if (_timers.TryRemove(uIDEvent, out _))
			{
				_logger.LogInformation("[User32] KillTimer: Removed timer {TimerId}", uIDEvent);
			}
			else
			{
				_logger.LogDebug("[User32] KillTimer: Timer {TimerId} not found (may have already been killed)", uIDEvent);
			}

			// Always return success for simplicity (matching Windows behavior of being lenient)
			return 1; // TRUE - success
		}

		// Window activity functions
		[DllModuleExport(4)]
		private uint GetLastActivePopup(uint hWnd)
		{
			_logger.LogInformation("[User32] GetLastActivePopup(hWnd=0x{HWnd:X8})", hWnd);
			return hWnd; // Return same window
		}

		// Help function
		[DllModuleExport(16)]
		private uint WinHelpA(uint hWndMain, in LpcStr lpszHelp, uint uCommand, uint dwData)
		{
			var helpFile = lpszHelp.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] WinHelpA(hWndMain=0x{HWndMain:X8}, lpszHelp=\"{HelpFile}\", uCommand={UCommand})", hWndMain, helpFile, uCommand);
			return 1; // TRUE
		}

		// Missing functions for calc.exe

		[DllModuleExport(20)]
		private uint CheckMenuRadioItem(uint hMenu, uint idFirst, uint idLast, uint idCheck, uint uFlags)
		{
			_logger.LogInformation("[User32] CheckMenuRadioItem(hMenu=0x{HMenu:X8}, idFirst={IdFirst}, idLast={IdLast}, idCheck={IdCheck}, uFlags=0x{UFlags:X})",
				hMenu, idFirst, idLast, idCheck, uFlags);
			return 1; // TRUE (stub)
		}

		[DllModuleExport(12)]
		private uint ChildWindowFromPoint(uint hWndParent, int x, int y)
		{
			_logger.LogInformation("[User32] ChildWindowFromPoint(hWndParent=0x{HWndParent:X8}, x={X}, y={Y})",
				hWndParent, x, y);
			return 0; // NULL (stub - no child window found)
		}

		[DllModuleExport(0)]
		private uint CloseClipboard()
		{
			_logger.LogInformation("[User32] CloseClipboard()");
			return 1; // TRUE (stub)
		}

		[DllModuleExport(16)]
		private uint DrawEdge(uint hdc, uint qrc, uint edge, uint grfFlags)
		{
			_logger.LogInformation("[User32] DrawEdge(hdc=0x{Hdc:X8}, qrc=0x{Qrc:X8}, edge=0x{Edge:X}, grfFlags=0x{GrfFlags:X})",
				hdc, qrc, edge, grfFlags);
			return 1; // TRUE (stub)
		}

		[DllModuleExport(4)]
		private uint GetClipboardData(uint uFormat)
		{
			_logger.LogInformation("[User32] GetClipboardData(uFormat={UFormat})", uFormat);
			return 0; // NULL (stub - no data available)
		}

		[DllModuleExport(4)]
		private uint HideCaret(uint hWnd)
		{
			_logger.LogInformation("[User32] HideCaret(hWnd=0x{HWnd:X8})", hWnd);
			return 1; // TRUE (stub)
		}

		[DllModuleExport(4)]
		private uint ShowCaret(uint hWnd)
		{
			_logger.LogInformation("[User32] ShowCaret(hWnd=0x{HWnd:X8})", hWnd);
			return 1; // TRUE (stub)
		}

		[DllModuleExport(4)]
		private uint IsClipboardFormatAvailable(uint format)
		{
			_logger.LogInformation("[User32] IsClipboardFormatAvailable(format={Format})", format);
			return 0; // FALSE (stub - format not available)
		}

		[DllModuleExport(4)]
		private uint OpenClipboard(uint hWndNewOwner)
		{
			_logger.LogInformation("[User32] OpenClipboard(hWndNewOwner=0x{HWndNewOwner:X8})", hWndNewOwner);
			return 1; // TRUE (stub)
		}

		[DllModuleExport(4)]
		private uint RegisterClassExA(uint lpWndClassEx)
		{
			if (lpWndClassEx == 0)
			{
				_logger.LogInformation("[User32] RegisterClassExA: NULL WNDCLASSEX pointer");
				return 0;
			}

			// Use ref struct wrapper for automatic memory access
			var wndClassEx = new WndClassExARef(_env.Memory, lpWndClassEx);

			if (wndClassEx.lpszClassName == 0)
			{
				_logger.LogInformation("[User32] RegisterClassExA: NULL class name");
				return 0;
			}

			var className = _env.ReadAnsiString(wndClassEx.lpszClassName);
			var menuName = wndClassEx.lpszMenuName != 0 ? _env.ReadAnsiString(wndClassEx.lpszMenuName) : null;

			_logger.LogInformation("[User32] RegisterClassExA: cbSize={CbSize}, style=0x{Style:X}, wndProc=0x{WndProc:X8}, className='{ClassName}'",
				wndClassEx.cbSize, wndClassEx.style, wndClassEx.lpfnWndProc, className);

			var classInfo = new ProcessEnvironment.WindowClassInfo(
				className, wndClassEx.style, wndClassEx.lpfnWndProc, wndClassEx.cbClsExtra, wndClassEx.cbWndExtra,
				wndClassEx.hInstance, wndClassEx.hIcon, wndClassEx.hCursor, wndClassEx.hbrBackground, menuName
			);

			if (_env.RegisterWindowClass(className, classInfo))
			{
				// Return an ATOM (non-zero value) on success
				// Windows uses atoms (16-bit values) for class registration
				// Use a counter to ensure uniqueness and avoid hash collisions
				var atom = _nextAtom++;

				// Register the atom-to-classname mapping
				_env.RegisterAtom(atom, className);

				_logger.LogInformation("[User32] RegisterClassExA: '{ClassName}' -> atom 0x{Atom:X4}", className, atom);
				return atom;
			}

			_logger.LogInformation("[User32] RegisterClassExA: Failed to register '{ClassName}'", className);
			return 0;
		}

		[DllModuleExport(24)]
		private uint TrackPopupMenuEx(uint hMenu, uint uFlags, int x, int y, uint hWnd, uint lptpm)
		{
			_logger.LogInformation("[User32] TrackPopupMenuEx(hMenu=0x{HMenu:X8}, uFlags=0x{UFlags:X}, x={X}, y={Y}, hWnd=0x{HWnd:X8})",
				hMenu, uFlags, x, y, hWnd);
			return 0; // FALSE (stub - no menu item selected)
		}

		/// <summary>
		/// Draws a rectangle in the style used to indicate focus.
		/// BOOL DrawFocusRect(HDC hDC, const RECT *lprc);
		/// </summary>
		[DllModuleExport(8)]
		private uint DrawFocusRect(uint hDC, uint lprc)
		{
			_logger.LogInformation("[User32] DrawFocusRect(hDC=0x{HDC:X8}, lprc=0x{Lprc:X8})", hDC, lprc);
			// Stub: Return TRUE (success)
			return 1;
		}

		[DllModuleExport(16)]
		private uint DrawCaption(uint hwnd, uint hdc, uint lprect, uint flags)
		{
			_logger.LogInformation("[User32] DrawCaption(hwnd=0x{Hwnd:X8}, hdc=0x{Hdc:X8}, lprect=0x{Lprect:X8}, flags=0x{Flags:X8})",
				hwnd, hdc, lprect, flags);

			// DrawCaption draws a window caption in the specified device context
			// Flags can include:
			// DC_ACTIVE (0x0001) - Active window caption
			// DC_SMALLCAP (0x0002) - Small caption (tool window)
			// DC_ICON (0x0004) - Draw icon
			// DC_TEXT (0x0008) - Draw caption text
			// DC_INBUTTON (0x0010) - Draw in button style
			// DC_GRADIENT (0x0020) - Use gradient for caption background
			// DC_BUTTONS (0x1000) - Draw caption buttons

			// Stub: Return TRUE (success)
			return 1;
		}

		[DllModuleExport(16)]
		private uint DrawFrameControl(uint hdc, uint lprc, uint uType, uint uState)
		{
			_logger.LogInformation("[User32] DrawFrameControl(hdc=0x{Hdc:X8}, lprc=0x{Lprc:X8}, uType={UType}, uState=0x{UState:X8})",
				hdc, lprc, uType, uState);

			// DrawFrameControl draws a frame control of the specified type and in the specified state
			// uType can be:
			// DFC_CAPTION (1) - Title bar
			// DFC_MENU (2) - Menu
			// DFC_SCROLL (3) - Scroll bar
			// DFC_BUTTON (4) - Standard button
			// DFC_POPUPMENU (5) - Popup menu

			// Stub: Return TRUE (success)
			return 1;
		}


		/// <summary>
		/// Excludes the update region from the clipping region of the specified device context.
		/// int ExcludeUpdateRgn(HDC hDC, HWND hWnd);
		/// </summary>
		[DllModuleExport(8)]
		private int ExcludeUpdateRgn(uint hDC, uint hWnd)
		{
			_logger.LogInformation("[User32] ExcludeUpdateRgn(hDC=0x{HDC:X8}, hWnd=0x{HWnd:X8})", hDC, hWnd);
			// Stub: Return SIMPLEREGION
			return 2; // SIMPLEREGION
		}

		/// <summary>
		/// Calculates the intersection of two rectangles.
		/// BOOL IntersectRect(LPRECT lprcDst, const RECT *lprcSrc1, const RECT *lprcSrc2);
		/// </summary>
		[DllModuleExport(12)]
		private uint IntersectRect(uint lprcDst, uint lprcSrc1, uint lprcSrc2)
		{
			_logger.LogInformation("[User32] IntersectRect(lprcDst=0x{LprcDst:X8}, lprcSrc1=0x{LprcSrc1:X8}, lprcSrc2=0x{LprcSrc2:X8})",
				lprcDst, lprcSrc1, lprcSrc2);

			if (lprcSrc1 == 0 || lprcSrc2 == 0 || lprcDst == 0)
			{
				return 0; // FALSE
			}

			// Read both source rectangles
			var rect1 = new RectRef(_env.Memory, lprcSrc1);
			var rect2 = new RectRef(_env.Memory, lprcSrc2);

			// Calculate intersection
			var leftDst = Math.Max(rect1.left, rect2.left);
			var topDst = Math.Max(rect1.top, rect2.top);
			var rightDst = Math.Min(rect1.right, rect2.right);
			var bottomDst = Math.Min(rect1.bottom, rect2.bottom);

			// Check if rectangles intersect
			if (leftDst >= rightDst || topDst >= bottomDst)
			{
				// No intersection - set to empty rectangle
				var emptyRect = new RectRef(_env.Memory, lprcDst);
				emptyRect.left = 0;
				emptyRect.top = 0;
				emptyRect.right = 0;
				emptyRect.bottom = 0;
				return 0; // FALSE
			}

			// Write intersection rectangle
			var dstRect = new RectRef(_env.Memory, lprcDst);
			dstRect.left = leftDst;
			dstRect.top = topDst;
			dstRect.right = rightDst;
			dstRect.bottom = bottomDst;

			return 1; // TRUE
		}

		/// <summary>
		/// Retrieves a device context (DC) for the entire window, including title bar, menus, and scroll bars.
		/// HDC GetWindowDC(HWND hWnd);
		/// </summary>
		[DllModuleExport(4)]
		private uint GetWindowDC(uint hWnd)
		{
			_logger.LogInformation("[User32] GetWindowDC(hWnd=0x{HWnd:X8})", hWnd);
			// Return a fake DC handle
			return 0x10001000;
		}

		/// <summary>
		/// Calls the default dialog box window procedure.
		/// LRESULT DefDlgProcA(HWND hDlg, UINT Msg, WPARAM wParam, LPARAM lParam);
		/// </summary>
		[DllModuleExport(16)]
		private uint DefDlgProcA(uint hDlg, uint Msg, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] DefDlgProcA(hDlg=0x{HDlg:X8}, Msg=0x{Msg:X}, wParam=0x{WParam:X}, lParam=0x{LParam:X})",
				hDlg, Msg, wParam, lParam);
			// Stub: Return 0 (message processed)
			return 0;
		}

		/// <summary>
		/// Retrieves the identifier of the next (or previous) control in a group.
		/// HWND GetNextDlgGroupItem(HWND hDlg, HWND hCtl, BOOL bPrevious);
		/// </summary>
		[DllModuleExport(12)]
		private uint GetNextDlgGroupItem(uint hDlg, uint hCtl, uint bPrevious)
		{
			_logger.LogInformation("[User32] GetNextDlgGroupItem(hDlg=0x{HDlg:X8}, hCtl=0x{HCtl:X8}, bPrevious={BPrevious})",
				hDlg, hCtl, bPrevious);
			// Stub: Return NULL (no next item)
			return 0;
		}

		/// <summary>
		/// Converts dialog box units to pixels.
		/// BOOL MapDialogRect(HWND hDlg, LPRECT lpRect);
		/// </summary>
		[DllModuleExport(8)]
		private uint MapDialogRect(uint hDlg, uint lpRect)
		{
			_logger.LogInformation("[User32] MapDialogRect(hDlg=0x{HDlg:X8}, lpRect=0x{LpRect:X8})", hDlg, lpRect);

			// Stub: Dialog units to pixels conversion (typical: 1 DLU = 1 pixel for simplicity)
			// In reality, this depends on the dialog base units
			return 1; // TRUE
		}

		/// <summary>
		/// Retrieves the specified value from the WNDCLASSEX structure associated with the window.
		/// DWORD GetClassLongA(HWND hWnd, int nIndex);
		/// </summary>
		[DllModuleExport(8)]
		private uint GetClassLongA(uint hWnd, int nIndex)
		{
			_logger.LogInformation("[User32] GetClassLongA(hWnd=0x{HWnd:X8}, nIndex={NIndex})", hWnd, nIndex);

			// Common indices:
			// GCL_MENUNAME = -8, GCL_HBRBACKGROUND = -10, GCL_HCURSOR = -12, GCL_HICON = -14
			// GCL_HMODULE = -16, GCL_CBWNDEXTRA = -18, GCL_CBCLSEXTRA = -20, GCL_WNDPROC = -24
			// GCL_STYLE = -26, GCW_ATOM = -32, GCL_HICONSM = -34

			// Stub: Return 0 for all indices
			return 0;
		}

		/// <summary>
		/// Retrieves the placement of the specified window.
		/// BOOL GetWindowPlacement(HWND hWnd, WINDOWPLACEMENT *lpwndpl);
		/// </summary>
		[DllModuleExport(8)]
		private uint GetWindowPlacement(uint hWnd, uint lpwndpl)
		{
			_logger.LogInformation("[User32] GetWindowPlacement(hWnd=0x{HWnd:X8}, lpwndpl=0x{Lpwndpl:X8})", hWnd, lpwndpl);

			if (lpwndpl == 0)
			{
				return 0; // FALSE
			}

			// WINDOWPLACEMENT structure:
			// UINT length; UINT flags; UINT showCmd; POINT ptMinPosition; POINT ptMaxPosition; RECT rcNormalPosition;

			// Write a default placement
			_env.MemWrite32(lpwndpl + 0, 44);  // length (sizeof(WINDOWPLACEMENT))
			_env.MemWrite32(lpwndpl + 4, 0);   // flags
			_env.MemWrite32(lpwndpl + 8, 1);   // showCmd (SW_SHOWNORMAL)
			_env.MemWrite32(lpwndpl + 12, 0);  // ptMinPosition.x
			_env.MemWrite32(lpwndpl + 16, 0);  // ptMinPosition.y
			_env.MemWrite32(lpwndpl + 20, 0);  // ptMaxPosition.x
			_env.MemWrite32(lpwndpl + 24, 0);  // ptMaxPosition.y
			_env.MemWrite32(lpwndpl + 28, 0);  // rcNormalPosition.left
			_env.MemWrite32(lpwndpl + 32, 0);  // rcNormalPosition.top
			_env.MemWrite32(lpwndpl + 36, 640); // rcNormalPosition.right
			_env.MemWrite32(lpwndpl + 40, 480); // rcNormalPosition.bottom

			return 1; // TRUE
		}

		/// <summary>
		/// Determines whether the specified window is a native Unicode window.
		/// BOOL IsWindowUnicode(HWND hWnd);
		/// </summary>
		[DllModuleExport(4)]
		private uint IsWindowUnicode(uint hWnd)
		{
			_logger.LogInformation("[User32] IsWindowUnicode(hWnd=0x{HWnd:X8})", hWnd);
			// Stub: Return FALSE (ANSI window)
			return 0;
		}

		/// <summary>
		/// Registers a new clipboard format.
		/// UINT RegisterClipboardFormatA(LPCSTR lpszFormat);
		/// </summary>
		[DllModuleExport(4)]
		private uint RegisterClipboardFormatA(in LpcStr lpszFormat)
		{
			var format = lpszFormat.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] RegisterClipboardFormatA(lpszFormat=\"{Format}\")", format);

			// Return a fake clipboard format ID
			// Standard formats are < 0xC000, custom formats are >= 0xC000
			return 0xC001;
		}

		/// <summary>
		/// Posts a message to the thread's message queue.
		/// BOOL PostThreadMessageA(DWORD idThread, UINT Msg, WPARAM wParam, LPARAM lParam);
		/// </summary>
		[DllModuleExport(16)]
		private uint PostThreadMessageA(uint idThread, uint Msg, uint wParam, uint lParam)
		{
			_logger.LogInformation("[User32] PostThreadMessageA(idThread={IdThread}, Msg=0x{Msg:X}, wParam=0x{WParam:X}, lParam=0x{LParam:X})",
				idThread, Msg, wParam, lParam);

			// Stub: Return TRUE (success)
			return 1;
		}

		/// <summary>
		/// Scrolls the contents of the specified window's client area.
		/// BOOL ScrollWindow(
		///   [in] HWND       hWnd,
		///   [in] int        XAmount,
		///   [in] int        YAmount,
		///   [in] const RECT *lpRect,
		///   [in] const RECT *lpClipRect
		/// );
		/// </summary>
		[DllModuleExport(20)]
		private uint ScrollWindow(uint hWnd, int xAmount, int yAmount, uint lpRect, uint lpClipRect)
		{
			_logger.LogInformation("[User32] ScrollWindow(hWnd=0x{HWnd:X8}, xAmount={XAmount}, yAmount={YAmount}, lpRect=0x{LpRect:X8}, lpClipRect=0x{LpClipRect:X8})",
				hWnd, xAmount, yAmount, lpRect, lpClipRect);

			// ScrollWindow scrolls the contents of a window's client area
			// For a stub implementation, we just return success
			// A full implementation would:
			// 1. Scroll the window contents by the specified amount
			// 2. Invalidate the uncovered region
			// 3. Optionally clip to the specified rectangles

			return 1; // TRUE
		}

		/// <summary>
		/// Sets the parameters of a scroll bar.
		/// int SetScrollInfo(
		///   [in] HWND          hWnd,
		///   [in] int           nBar,
		///   [in] LPCSCROLLINFO lpsi,
		///   [in] BOOL          redraw
		/// );
		/// </summary>
		[DllModuleExport(16)]
		private uint SetScrollInfo(uint hWnd, int nBar, uint lpsi, uint redraw)
		{
			_logger.LogInformation("[User32] SetScrollInfo(hWnd=0x{HWnd:X8}, nBar={NBar}, lpsi=0x{Lpsi:X8}, redraw={Redraw})",
				hWnd, nBar, lpsi, redraw);

			// SetScrollInfo sets the parameters of a scroll bar
			// nBar can be SB_HORZ (0), SB_VERT (1), or SB_CTL (2)
			// lpsi points to a SCROLLINFO structure with new values

			if (lpsi != 0)
			{
				var si = new ScrollInfoRef(_env.Memory, lpsi);
				_logger.LogInformation("[User32] SetScrollInfo: nMin={NMin}, nMax={NMax}, nPage={NPage}, nPos={NPos}",
					si.nMin, si.nMax, si.nPage, si.nPos);
			}

			// Return the current position (stub)
			return 0;
		}

		/// <summary>
		/// Retrieves the parameters of a scroll bar.
		/// BOOL GetScrollInfo(
		///   [in]      HWND          hWnd,
		///   [in]      int           nBar,
		///   [in, out] LPSCROLLINFO  lpsi
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint GetScrollInfo(uint hWnd, int nBar, uint lpsi)
		{
			_logger.LogInformation("[User32] GetScrollInfo(hWnd=0x{HWnd:X8}, nBar={NBar}, lpsi=0x{Lpsi:X8})",
				hWnd, nBar, lpsi);

			// GetScrollInfo retrieves the parameters of a scroll bar
			// nBar can be SB_HORZ (0), SB_VERT (1), or SB_CTL (2)
			// lpsi points to a SCROLLINFO structure to receive values

			if (lpsi != 0)
			{
				// SCROLLINFO structure offsets
				const uint SCROLLINFO_CBSIZE_OFFSET = 0;
				const uint SCROLLINFO_FMASK_OFFSET = 4;
				const uint SCROLLINFO_NMIN_OFFSET = 8;
				const uint SCROLLINFO_NMAX_OFFSET = 12;
				const uint SCROLLINFO_NPAGE_OFFSET = 16;
				const uint SCROLLINFO_NPOS_OFFSET = 20;
				const uint SCROLLINFO_NTRACKPOS_OFFSET = 24;

				var fMask = _env.MemRead32(lpsi + SCROLLINFO_FMASK_OFFSET);

				// For stub implementation, fill in default values
				// SIF_RANGE (0x0001), SIF_PAGE (0x0002), SIF_POS (0x0004), SIF_TRACKPOS (0x0010)

				if ((fMask & 0x0001) != 0) // SIF_RANGE
				{
					_env.MemWrite32(lpsi + SCROLLINFO_NMIN_OFFSET, 0); // nMin = 0
					_env.MemWrite32(lpsi + SCROLLINFO_NMAX_OFFSET, 100); // nMax = 100
				}
				if ((fMask & 0x0002) != 0) // SIF_PAGE
				{
					_env.MemWrite32(lpsi + SCROLLINFO_NPAGE_OFFSET, 10); // nPage = 10
				}
				if ((fMask & 0x0004) != 0) // SIF_POS
				{
					_env.MemWrite32(lpsi + SCROLLINFO_NPOS_OFFSET, 0); // nPos = 0
				}
				if ((fMask & 0x0010) != 0) // SIF_TRACKPOS
				{
					_env.MemWrite32(lpsi + SCROLLINFO_NTRACKPOS_OFFSET, 0); // nTrackPos = 0
				}

				_logger.LogInformation("[User32] GetScrollInfo: Returning default scroll info");
			}

			return 1; // TRUE
		}

		// DDE (Dynamic Data Exchange) functions
		// DDE is a legacy IPC mechanism used by older Windows applications
		private uint _nextDdeInstance = 0x10000000;
		private uint _nextDdeString = 0x30000000;
		private readonly Dictionary<uint, string> _ddeStrings = new();

		/// <summary>
		/// Registers an application with the Dynamic Data Exchange (DDE) Management Library.
		/// UINT DdeInitializeA(
		///   [in]  LPDWORD      pidInst,
		///   [in]  PFNCALLBACK  pfnCallback,
		///   [in]  DWORD        afCmd,
		///   [in]  DWORD        ulRes
		/// );
		/// </summary>
		[DllModuleExport(16)]
		private uint DdeInitializeA(uint pidInst, uint pfnCallback, uint afCmd, uint ulRes)
		{
			_logger.LogInformation("[User32] DdeInitializeA(pidInst=0x{PidInst:X8}, pfnCallback=0x{PfnCallback:X8}, afCmd=0x{AfCmd:X8}, ulRes={UlRes})",
				pidInst, pfnCallback, afCmd, ulRes);

			// DdeInitialize registers an application with DDE
			// pidInst points to a DWORD that receives the instance identifier
			// pfnCallback is the callback function for DDE transactions
			// afCmd specifies the DDE filters

			if (pidInst != 0)
			{
				var instance = _nextDdeInstance++;
				_env.MemWrite32(pidInst, instance);
				_logger.LogInformation("[User32] DdeInitializeA: Created instance 0x{Instance:X8}", instance);
			}

			return 0; // DMLERR_NO_ERROR
		}

		/// <summary>
		/// Establishes a conversation with a server application.
		/// HCONV DdeConnect(
		///   [in] DWORD   idInst,
		///   [in] HSZ     hszService,
		///   [in] HSZ     hszTopic,
		///   [in] PCONVCONTEXT pCC
		/// );
		/// </summary>
		[DllModuleExport(16)]
		private uint DdeConnect(uint idInst, uint hszService, uint hszTopic, uint pCC)
		{
			_logger.LogInformation("[User32] DdeConnect(idInst=0x{IdInst:X8}, hszService=0x{HszService:X8}, hszTopic=0x{HszTopic:X8}, pCC=0x{PCC:X8})",
				idInst, hszService, hszTopic, pCC);

			// DdeConnect establishes a DDE conversation
			// hszService and hszTopic are string handles
			// Returns a conversation handle, or NULL on failure

			var serviceName = _ddeStrings.TryGetValue(hszService, out var svc) ? svc : "unknown";
			var topicName = _ddeStrings.TryGetValue(hszTopic, out var top) ? top : "unknown";

			_logger.LogInformation("[User32] DdeConnect: Connecting to service \"{ServiceName}\" topic \"{TopicName}\"",
				serviceName, topicName);

			// For stub, just return NULL to indicate no server available
			return 0; // NULL - connection failed
		}

		/// <summary>
		/// Terminates a DDE conversation.
		/// BOOL DdeDisconnect(
		///   [in] HCONV hConv
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint DdeDisconnect(uint hConv)
		{
			_logger.LogInformation("[User32] DdeDisconnect(hConv=0x{HConv:X8})", hConv);

			// DdeDisconnect terminates a DDE conversation
			// Returns TRUE on success, FALSE on failure

			return 1; // TRUE
		}

		/// <summary>
		/// Creates a DDE string handle.
		/// HSZ DdeCreateStringHandleA(
		///   [in] DWORD  idInst,
		///   [in] LPCSTR psz,
		///   [in] int    iCodePage
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint DdeCreateStringHandleA(uint idInst, in LpcStr psz, int iCodePage)
		{
			var str = psz.ToString() ?? string.Empty;
			_logger.LogInformation("[User32] DdeCreateStringHandleA(idInst=0x{IdInst:X8}, psz=\"{Str}\", iCodePage={ICodePage})",
				idInst, str, iCodePage);

			// DdeCreateStringHandle creates a string handle for DDE
			// The string handle can be used in DDE transactions

			if (string.IsNullOrEmpty(str))
			{
				return 0; // NULL
			}

			var handle = _nextDdeString++;
			_ddeStrings[handle] = str;

			_logger.LogInformation("[User32] DdeCreateStringHandleA: Created handle 0x{Handle:X8} for \"{Str}\"", handle, str);
			return handle;
		}

		/// <summary>
		/// Frees a DDE string handle.
		/// BOOL DdeFreeStringHandle(
		///   [in] DWORD idInst,
		///   [in] HSZ   hsz
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint DdeFreeStringHandle(uint idInst, uint hsz)
		{
			_logger.LogInformation("[User32] DdeFreeStringHandle(idInst=0x{IdInst:X8}, hsz=0x{Hsz:X8})",
				idInst, hsz);

			// DdeFreeStringHandle frees a string handle

			if (_ddeStrings.Remove(hsz))
			{
				_logger.LogInformation("[User32] DdeFreeStringHandle: Freed handle 0x{Hsz:X8}", hsz);
			}

			return 1; // TRUE
		}

		/// <summary>
		/// Begins a DDE transaction.
		/// HDDEDATA DdeClientTransaction(
		///   [in] LPBYTE   pData,
		///   [in] DWORD    cbData,
		///   [in] HCONV    hConv,
		///   [in] HSZ      hszItem,
		///   [in] UINT     wFmt,
		///   [in] UINT     wType,
		///   [in] DWORD    dwTimeout,
		///   [out] LPDWORD pdwResult
		/// );
		/// </summary>
		[DllModuleExport(32)]
		private uint DdeClientTransaction(uint pData, uint cbData, uint hConv, uint hszItem, uint wFmt, uint wType, uint dwTimeout, uint pdwResult)
		{
			_logger.LogInformation("[User32] DdeClientTransaction(pData=0x{PData:X8}, cbData={CbData}, hConv=0x{HConv:X8}, hszItem=0x{HszItem:X8}, wFmt={WFmt}, wType={WType}, dwTimeout={DwTimeout})",
				pData, cbData, hConv, hszItem, wFmt, wType, dwTimeout);

			// DdeClientTransaction performs a DDE transaction
			// wType specifies the transaction type (XTYP_EXECUTE, XTYP_REQUEST, etc.)
			// Returns a DDE data handle, or NULL on failure

			var itemName = _ddeStrings.TryGetValue(hszItem, out var item) ? item : "unknown";
			_logger.LogInformation("[User32] DdeClientTransaction: Transaction on item \"{ItemName}\"", itemName);

			// For stub, return NULL to indicate transaction failed
			return 0; // NULL
		}

		/// <summary>
		/// Retrieves data from a DDE data handle.
		/// DWORD DdeGetData(
		///   [in]  HDDEDATA hData,
		///   [out] LPBYTE   pDst,
		///   [in]  DWORD    cbMax,
		///   [in]  DWORD    cbOff
		/// );
		/// </summary>
		[DllModuleExport(16)]
		private uint DdeGetData(uint hData, uint pDst, uint cbMax, uint cbOff)
		{
			_logger.LogInformation("[User32] DdeGetData(hData=0x{HData:X8}, pDst=0x{PDst:X8}, cbMax={CbMax}, cbOff={CbOff})",
				hData, pDst, cbMax, cbOff);

			// DdeGetData retrieves data from a DDE data handle
			// Returns the number of bytes copied, or 0 on failure

			// For stub, return 0 (no data)
			return 0;
		}

		/// <summary>
		/// Copies an accelerator table.
		/// int CopyAcceleratorTableA(HACCEL hAccelSrc, LPACCEL lpAccelDst, int cAccelEntries);
		/// </summary>
		[DllModuleExport(12)]
		private uint CopyAcceleratorTableA(uint hAccelSrc, uint lpAccelDst, int cAccelEntries)
		{
			_logger.LogInformation("[User32] CopyAcceleratorTableA(hAccelSrc=0x{HAccelSrc:X8}, lpAccelDst=0x{LpAccelDst:X8}, cAccelEntries={CAccelEntries})",
				hAccelSrc, lpAccelDst, cAccelEntries);

			// If lpAccelDst is NULL, return the number of entries
			if (lpAccelDst == 0)
			{
				return 0; // No entries (stub)
			}

			// Stub: Return 0 (no entries copied)
			return 0;
		}

		/// <summary>
		/// Sets the Help context identifier for the window.
		/// BOOL SetWindowContextHelpId(HWND hWnd, DWORD dwContextHelpId);
		/// </summary>
		[DllModuleExport(8)]
		private uint SetWindowContextHelpId(uint hWnd, uint dwContextHelpId)
		{
			_logger.LogInformation("[User32] SetWindowContextHelpId(hWnd=0x{HWnd:X8}, dwContextHelpId=0x{DwContextHelpId:X})",
				hWnd, dwContextHelpId);

			// Stub: Return TRUE (success)
			return 1;
		}

		/// <summary>
		/// Determines whether a window is maximized.
		/// BOOL IsZoomed(
		///   [in] HWND hWnd
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint IsZoomed(uint hWnd)
		{
			_logger.LogInformation("[User32] IsZoomed(hWnd=0x{HWnd:X8})", hWnd);

			// IsZoomed checks if a window is maximized (has WS_MAXIMIZE style)
			// Get the window style
			var style = _env.GetWindowProperty(hWnd, (int)NativeTypes.WindowLong.GWL_STYLE);

			if (style == 0)
			{
				// Window not found or no custom style, check the window info
				var window = _env.GetWindow(hWnd);
				if (window.HasValue)
				{
					style = window.Value.Style;
				}
			}

			// Check for WS_MAXIMIZE (0x01000000)
			const uint WS_MAXIMIZE = 0x01000000;
			var isMaximized = (style & WS_MAXIMIZE) != 0;

			_logger.LogInformation("[User32] IsZoomed: Window 0x{HWnd:X8} is {State}",
				hWnd, isMaximized ? "maximized" : "not maximized");

			return isMaximized ? 1u : 0u;
		}

		/// <summary>
		/// Defines a system-wide hot key.
		/// BOOL RegisterHotKey(
		///   [in, optional] HWND hWnd,
		///   [in]           int  id,
		///   [in]           UINT fsModifiers,
		///   [in]           UINT vk
		/// );
		/// </summary>
		[DllModuleExport(16)]
		private uint RegisterHotKey(uint hWnd, int id, uint fsModifiers, uint vk)
		{
			_logger.LogInformation("[User32] RegisterHotKey(hWnd=0x{HWnd:X8}, id={Id}, fsModifiers=0x{FsModifiers:X}, vk=0x{Vk:X})",
				hWnd, id, fsModifiers, vk);

			// RegisterHotKey defines a system-wide hotkey
			// fsModifiers can be:
			// MOD_ALT = 0x0001, MOD_CONTROL = 0x0002, MOD_SHIFT = 0x0004, MOD_WIN = 0x0008
			// vk is a virtual key code

			// For a stub implementation, we just return success
			// A full implementation would:
			// 1. Register the hotkey globally
			// 2. Post WM_HOTKEY messages when the hotkey is pressed
			// 3. Track registered hotkeys to prevent duplicates

			_logger.LogInformation("[User32] RegisterHotKey: Registered hotkey id={Id} with modifiers=0x{FsModifiers:X} and key=0x{Vk:X}",
				id, fsModifiers, vk);

			return 1; // TRUE (success)
		}

		/// <summary>
		/// Creates a modal dialog box from a dialog box template in memory.
		/// INT_PTR DialogBoxIndirectParamA(
		///   [in, optional] HINSTANCE       hInstance,
		///   [in]           LPCDLGTEMPLATEA lpTemplate,
		///   [in, optional] HWND            hWndParent,
		///   [in, optional] DLGPROC         lpDialogFunc,
		///   [in]           LPARAM          dwInitParam
		/// );
		/// </summary>
		[DllModuleExport(20)]
		private uint DialogBoxIndirectParamA(uint hInstance, uint lpTemplate, uint hWndParent, uint lpDialogFunc, uint dwInitParam)
		{
			_logger.LogInformation("[User32] DialogBoxIndirectParamA(hInstance=0x{HInstance:X8}, lpTemplate=0x{LpTemplate:X8}, hWndParent=0x{HWndParent:X8}, lpDialogFunc=0x{LpDialogFunc:X8}, dwInitParam=0x{DwInitParam:X8})",
				hInstance, lpTemplate, hWndParent, lpDialogFunc, dwInitParam);

			// DialogBoxIndirectParamA creates a modal dialog from a template in memory
			// This is similar to DialogBoxParamA, but uses a template in memory instead of a resource

			if (lpTemplate == 0)
			{
				_logger.LogWarning("[User32] DialogBoxIndirectParamA: NULL template pointer");
				return unchecked((uint)-1); // Return -1 for error
			}

			if (lpDialogFunc == 0)
			{
				_logger.LogWarning("[User32] DialogBoxIndirectParamA: NULL dialog proc");
				return unchecked((uint)-1); // Return -1 for error
			}

			// For stub implementation, we return IDCANCEL (2)
			// A full implementation would:
			// 1. Parse the dialog template
			// 2. Create the dialog window and controls
			// 3. Enter a modal message loop
			// 4. Call the dialog procedure for messages
			// 5. Return the result from EndDialog

			_logger.LogInformation("[User32] DialogBoxIndirectParamA: Stub returning IDCANCEL");
			return 2; // IDCANCEL
		}

		/// <summary>
		/// Converts a string to OEM character set.
		/// </summary>
		[DllModuleExport(0)]
		private uint CharToOemA(in LpStr lpszSrc, in LpStr lpszDst)
		{
			_logger.LogInformation("[User32] CharToOemA(lpszSrc=0x{LpszSrc:X8}, lpszDst=0x{LpszDst:X8})", lpszSrc.Address, lpszDst.Address);
			// Stub: just copy the string as-is
			var src = lpszSrc.ToString();
			if (!string.IsNullOrEmpty(src))
			{
				lpszDst.Write(_memory!, src, true);
			}
			return 1; // TRUE
		}

		/// <summary>
		/// Creates an accelerator table.
		/// </summary>
		[DllModuleExport(0)]
		private uint CreateAcceleratorTableA(uint lpaccl, int cAccel)
		{
			_logger.LogInformation("[User32] CreateAcceleratorTableA(lpaccl=0x{Lpaccl:X8}, cAccel={CAccel})", lpaccl, cAccel);
			// Return a fake accelerator table handle
			return 0x80001000;
		}

		/// <summary>
		/// Destroys an accelerator table.
		/// </summary>
		[DllModuleExport(0)]
		private uint DestroyAcceleratorTable(uint hAccel)
		{
			_logger.LogInformation("[User32] DestroyAcceleratorTable(hAccel=0x{HAccel:X8})", hAccel);
			return 1; // TRUE
		}

		/// <summary>
		/// Retrieves the coordinates of the update region for a window.
		/// </summary>
		[DllModuleExport(0)]
		private uint GetUpdateRect(uint hWnd, uint lpRect, uint bErase)
		{
			_logger.LogInformation("[User32] GetUpdateRect(hWnd=0x{HWnd:X8}, lpRect=0x{LpRect:X8}, bErase={BErase})", hWnd, lpRect, bErase);
			// Stub: return FALSE (no update region)
			return 0;
		}

		/// <summary>
		/// Retrieves the update region for a window.
		/// </summary>
		[DllModuleExport(0)]
		private uint GetUpdateRgn(uint hWnd, uint hRgn, uint bErase)
		{
			_logger.LogInformation("[User32] GetUpdateRgn(hWnd=0x{HWnd:X8}, hRgn=0x{HRgn:X8}, bErase={BErase})", hWnd, hRgn, bErase);
			// Stub: return NULLREGION (no update region)
			return 1;
		}

		/// <summary>
		/// Invalidates a region in a window.
		/// </summary>
		[DllModuleExport(0)]
		private uint InvalidateRgn(uint hWnd, uint hRgn, uint bErase)
		{
			_logger.LogInformation("[User32] InvalidateRgn(hWnd=0x{HWnd:X8}, hRgn=0x{HRgn:X8}, bErase={BErase})", hWnd, hRgn, bErase);
			return 1; // TRUE
		}

		/// <summary>
		/// Validates a region in a window.
		/// </summary>
		[DllModuleExport(0)]
		private uint ValidateRgn(uint hWnd, uint hRgn)
		{
			_logger.LogInformation("[User32] ValidateRgn(hWnd=0x{HWnd:X8}, hRgn=0x{HRgn:X8})", hWnd, hRgn);
			return 1; // TRUE
		}

		/// <summary>
		/// Loads a cursor from a file.
		/// </summary>
		[DllModuleExport(0)]
		private uint LoadCursorFromFileA(in LpcStr lpFileName)
		{
			var fileName = lpFileName.ToString();
			_logger.LogInformation("[User32] LoadCursorFromFileA(lpFileName=\"{FileName}\")", fileName ?? "(null)");
			// Return a fake cursor handle
			return 0x80002000;
		}

		/// <summary>
		/// Sets a window class field.
		/// </summary>
		[DllModuleExport(0)]
		private uint SetClassLongA(uint hWnd, int nIndex, uint dwNewLong)
		{
			_logger.LogInformation("[User32] SetClassLongA(hWnd=0x{HWnd:X8}, nIndex={NIndex}, dwNewLong=0x{DwNewLong:X8})", hWnd, nIndex, dwNewLong);
			// Return previous value (stub: return 0)
			return 0;
		}

		#region Async Callback Execution Helper

		/// <summary>
		/// Executes emulated guest code asynchronously with comprehensive safeguards.
		/// This helper method contains the common execution loop logic used by all async callback methods,
		/// eliminating code duplication while ensuring consistent behavior.
		/// </summary>
		/// <param name="returnAddress">Marker address (0xDEADBEEF) indicating callback return</param>
		/// <param name="logContext">Context string for logging (e.g., "CallTimerProcAsync")</param>
		/// <param name="handleComAndImports">Whether to handle COM vtable and import calls</param>
		/// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
		/// <returns>True if execution completed successfully, false if aborted or failed</returns>
		private async Task<bool> ExecuteCallbackAsync(
			uint returnAddress,
			string logContext,
			bool handleComAndImports,
			CancellationToken cancellationToken = default)
		{
			const int YIELD_INTERVAL = 10000;
			var steps = 0;
			var executionSuccessful = true;
			var lastCheckEip = _cpu!.GetEip();
			var stuckCounter = 0;

			try
			{
				while (true)
				{
					// Check for cancellation at regular intervals
					if (steps % CANCELLATION_CHECK_INTERVAL == 0)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							_logger.LogInformation("[User32] {LogContext}: Cancellation requested at step {Steps}", logContext, steps);
							executionSuccessful = false;
							break;
						}

						// Yield to allow other async operations to proceed
						await Task.Yield();
					}

					var eip = _cpu.GetEip();

					// Check if we've returned to our marker address
					if (eip == returnAddress)
					{
						break;
					}

					// Check for invalid EIP (NULL pointer execution)
					if (eip == 0x00000000)
					{
						_logger.LogWarning("[User32] {LogContext}: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting", logContext);
						executionSuccessful = false;
						break;
					}

					// Check for other invalid low addresses
					if (eip < MINIMUM_VALID_EIP && eip != returnAddress)
					{
						_logger.LogError("[User32] {LogContext}: Execution jumped to invalid low address 0x{Eip:X8}", logContext, eip);
						executionSuccessful = false;
						break;
					}

					// Detect potential infinite loops
					if (steps > 0 && steps % INFINITE_LOOP_CHECK_INTERVAL == 0)
					{
						var currentEip = _cpu.GetEip();
						if (currentEip == lastCheckEip)
						{
							stuckCounter++;
							if (stuckCounter >= STUCK_COUNTER_THRESHOLD)
							{
								_logger.LogWarning("[User32] {LogContext}: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting",
									logContext, currentEip, stuckCounter);
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
					var step = _cpu.SingleStep(_memory!);

					// Handle COM vtable and import calls (if requested)
					if (handleComAndImports && HandleComAndImportCalls(step, _cpu, _memory!, logContext, out var stepDesc, out var shouldBreak) && shouldBreak)
					{
						executionSuccessful = false;
						break;
					}

					steps++;

					// Periodically check if we should yield to other threads
					if (steps % YIELD_INTERVAL == 0)
					{
						var scheduler = _env.ThreadScheduler;
						if (scheduler != null)
						{
							scheduler.ProcessWaitTimeouts();
							if (scheduler.ShouldContextSwitch())
							{
								_logger.LogDebug("[User32] {LogContext}: Cooperative yield at {Steps} steps", logContext, steps);
							}
						}

						await Task.Yield();
					}
				}
			}
			catch (Exception ex)
			{
				// Rethrow critical exceptions that should not be caught
				if (ex is OutOfMemoryException || ex is StackOverflowException)
				{
					throw;
				}

				_logger.LogError(ex, "[User32] {LogContext}: Exception during execution: {ExMessage}", logContext, ex.Message);
				executionSuccessful = false;
			}

			return executionSuccessful;
		}

		#endregion

		#region Async Callback Methods

		/// <summary>
		/// Async version of timer callback execution that eliminates the need for STACK_SAFETY_MARGIN.
		/// Uses async/await pattern for clean separation of host (C#) and guest (x86) execution stacks.
		/// </summary>
		/// <param name="timerProc">Address of the timer procedure in emulated memory</param>
		/// <param name="hWnd">Window handle</param>
		/// <param name="uMsg">Timer message (WM_TIMER)</param>
		/// <param name="idEvent">Timer identifier</param>
		/// <param name="dwTime">Current system time</param>
		/// <param name="cancellationToken">Optional cancellation token</param>
		/// <returns>Task that completes when callback execution finishes</returns>
		private async Task CallTimerProcAsync(
			uint timerProc,
			uint hWnd,
			uint uMsg,
			uint idEvent,
			uint dwTime,
			CancellationToken cancellationToken = default)
		{
			if (_cpu == null || _memory == null)
			{
				_logger.LogWarning("[User32] CallTimerProcAsync: CPU or Memory not available");
				return;
			}

			_logger.LogInformation("[User32] CallTimerProcAsync: Calling 0x{TimerProc:X8} for timer {IdEvent}", timerProc, idEvent);

			// Validate callback address
			if (timerProc == 0)
			{
				_logger.LogWarning("[User32] CallTimerProcAsync: Timer procedure address is NULL (0x00000000), aborting");
				return;
			}

			// Save current CPU state
			var savedEip = _cpu.GetEip();
			var savedEsp = _cpu.GetRegister("ESP");
			var savedEbp = _cpu.GetRegister("EBP");

			// Define return address marker
			const uint RETURN_ADDRESS = 0xDEADBEEF;

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			// NOTE: No STACK_SAFETY_MARGIN needed! The async architecture provides clean stack separation.
			var esp = savedEsp;

			// Push return address first
			esp -= 4;
			_memory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			// VOID CALLBACK TimerProc(HWND hwnd, UINT uMsg, UINT_PTR idEvent, DWORD dwTime)
			esp -= 4;
			_memory.Write32(esp, dwTime);

			esp -= 4;
			_memory.Write32(esp, idEvent);

			esp -= 4;
			_memory.Write32(esp, uMsg);

			esp -= 4;
			_memory.Write32(esp, hWnd);

			// Update CPU registers
			_cpu.SetRegister("ESP", esp);
			_cpu.SetEip(timerProc);

			// Execute callback using the common helper method
			// Note: handleComAndImports=true is required for User32 callbacks that may invoke COM objects
			var executionSuccessful = await ExecuteCallbackAsync(RETURN_ADDRESS, "CallTimerProcAsync", handleComAndImports: true, cancellationToken).ConfigureAwait(false);

			// Restore CPU state
			_cpu.SetEip(savedEip);
			_cpu.SetRegister("ESP", savedEsp);
			_cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] CallTimerProcAsync: Completed {Status}", executionSuccessful ? "successfully" : "with errors");
		}

		/// <summary>
		/// Async version of window enumeration callback execution that eliminates the need for STACK_SAFETY_MARGIN.
		/// Uses async/await pattern for clean separation of host (C#) and guest (x86) execution stacks.
		/// </summary>
		/// <param name="enumProc">Address of the enumeration callback in emulated memory</param>
		/// <param name="hWnd">Window handle to pass to callback</param>
		/// <param name="lParam">Application-defined value</param>
		/// <param name="cancellationToken">Optional cancellation token</param>
		/// <returns>Return value from the callback (TRUE to continue, FALSE to stop)</returns>
		private async Task<uint> CallEnumWindowsProcAsync(
			uint enumProc,
			uint hWnd,
			uint lParam,
			CancellationToken cancellationToken = default)
		{
			if (_cpu == null || _memory == null)
			{
				_logger.LogWarning("[User32] CallEnumWindowsProcAsync: CPU or Memory not available");
				return 0;
			}

			_logger.LogInformation("[User32] CallEnumWindowsProcAsync: Calling 0x{EnumProc:X8} for window 0x{HWnd:X8}", enumProc, hWnd);

			// Validate callback address
			if (enumProc == 0)
			{
				_logger.LogWarning("[User32] CallEnumWindowsProcAsync: Enumeration callback address is NULL (0x00000000), aborting");
				return 0;
			}

			// Save current CPU state
			var savedEip = _cpu.GetEip();
			var savedEsp = _cpu.GetRegister("ESP");
			var savedEbp = _cpu.GetRegister("EBP");

			// Define return address marker
			const uint RETURN_ADDRESS = 0xDEADBEEF;

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			// NOTE: No STACK_SAFETY_MARGIN needed! The async architecture provides clean stack separation.
			var esp = savedEsp;

			// Push return address first
			esp -= 4;
			_memory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			// BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam)
			esp -= 4;
			_memory.Write32(esp, lParam);

			esp -= 4;
			_memory.Write32(esp, hWnd);

			// Update CPU registers
			_cpu.SetRegister("ESP", esp);
			_cpu.SetEip(enumProc);

			// Execute callback using the common helper method
			var executionSuccessful = await ExecuteCallbackAsync(RETURN_ADDRESS, "CallEnumWindowsProcAsync", handleComAndImports: true, cancellationToken).ConfigureAwait(false);

			// Get return value from EAX, but only if execution was successful
			var returnValue = executionSuccessful ? _cpu.GetRegister("EAX") : 0u;

			// Restore CPU state
			_cpu.SetEip(savedEip);
			_cpu.SetRegister("ESP", savedEsp);
			_cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] CallEnumWindowsProcAsync: Completed with return value 0x{ReturnValue:X8}", returnValue);

			return returnValue;
		}

		/// <summary>
		/// Async version of hook procedure execution that eliminates the need for STACK_SAFETY_MARGIN.
		/// Uses async/await pattern for clean separation of host (C#) and guest (x86) execution stacks.
		/// </summary>
		/// <param name="hookProc">Address of the hook procedure in emulated memory</param>
		/// <param name="nCode">Hook code</param>
		/// <param name="wParam">Message parameter</param>
		/// <param name="lParam">Message parameter</param>
		/// <param name="cancellationToken">Optional cancellation token</param>
		/// <returns>Return value from the hook procedure</returns>
		private async Task<uint> CallHookProcAsync(
			uint hookProc,
			int nCode,
			uint wParam,
			uint lParam,
			CancellationToken cancellationToken = default)
		{
			if (_cpu == null || _memory == null)
			{
				_logger.LogWarning("[User32] CallHookProcAsync: CPU or Memory not available");
				return 0;
			}

			_logger.LogInformation("[User32] CallHookProcAsync: Calling 0x{HookProc:X8} with nCode={NCode}", hookProc, nCode);

			// Validate callback address
			if (hookProc == 0)
			{
				_logger.LogWarning("[User32] CallHookProcAsync: Hook procedure address is NULL (0x00000000), aborting");
				return 0;
			}

			// Save current CPU state
			var savedEip = _cpu.GetEip();
			var savedEsp = _cpu.GetRegister("ESP");
			var savedEbp = _cpu.GetRegister("EBP");

			// Define return address marker
			const uint RETURN_ADDRESS = 0xDEADBEEF;

			// Set up stack for stdcall convention (parameters pushed right-to-left)
			// NOTE: No STACK_SAFETY_MARGIN needed! The async architecture provides clean stack separation.
			var esp = savedEsp;

			// Push return address first
			esp -= 4;
			_memory.Write32(esp, RETURN_ADDRESS);

			// Push parameters (right-to-left for stdcall)
			// LRESULT CALLBACK HookProc(int nCode, WPARAM wParam, LPARAM lParam)
			esp -= 4;
			_memory.Write32(esp, lParam);

			esp -= 4;
			_memory.Write32(esp, wParam);

			esp -= 4;
			_memory.Write32(esp, (uint)nCode);

			// Update CPU registers
			_cpu.SetRegister("ESP", esp);
			_cpu.SetEip(hookProc);

			// Execute callback using the common helper method
			var executionSuccessful = await ExecuteCallbackAsync(RETURN_ADDRESS, "CallHookProcAsync", handleComAndImports: true, cancellationToken).ConfigureAwait(false);

			// Get return value from EAX, but only if execution was successful
			var returnValue = executionSuccessful ? _cpu.GetRegister("EAX") : 0u;

			// Restore CPU state
			_cpu.SetEip(savedEip);
			_cpu.SetRegister("ESP", savedEsp);
			_cpu.SetRegister("EBP", savedEbp);

			_logger.LogInformation("[User32] CallHookProcAsync: Completed with return value 0x{ReturnValue:X8}", returnValue);

			return returnValue;
		}

		#endregion

		#region Menu Functions

		/// <summary>
		/// Creates a popup menu.
		/// HMENU CreatePopupMenu();
		/// </summary>
		[DllModuleExport(0)]
		private uint CreatePopupMenu()
		{
			_logger.LogInformation("[User32] CreatePopupMenu()");

			// Generate a new unique menu handle (using distinct handle range)
			var menuHandle = 0xABCD0000u + _nextMenuHandle++;

			_logger.LogInformation("[User32] CreatePopupMenu: Created menu handle 0x{MenuHandle:X8}", menuHandle);

			return menuHandle;
		}

		/// <summary>
		/// Appends a new item to the end of the specified menu bar, drop-down menu, submenu, or shortcut menu.
		/// BOOL AppendMenuA(
		///   [in] HMENU   hMenu,
		///   [in] UINT    uFlags,
		///   [in] UINT_PTR uIDNewItem,
		///   [in] LPCSTR  lpNewItem
		/// );
		/// </summary>
		[DllModuleExport(16)]
		private uint AppendMenuA(uint hMenu, uint uFlags, uint uIDNewItem, uint lpNewItem)
		{
			string itemText = "";
			if (lpNewItem != 0)
			{
				itemText = _env.ReadAnsiString(lpNewItem);
			}

			_logger.LogInformation("[User32] AppendMenuA(hMenu=0x{HMenu:X8}, uFlags=0x{UFlags:X8}, uIDNewItem=0x{UIDNewItem:X8}, lpNewItem=\"{ItemText}\")",
				hMenu, uFlags, uIDNewItem, itemText);

			// For now, just return success
			// A full implementation would need to track menu items and structure
			return 1; // TRUE
		}

		/// <summary>
		/// Displays a shortcut menu at the specified location and tracks the selection of items.
		/// BOOL TrackPopupMenu(
		///   [in] HMENU  hMenu,
		///   [in] UINT   uFlags,
		///   [in] int    x,
		///   [in] int    y,
		///   [in] int    nReserved,
		///   [in] HWND   hWnd,
		///   [in] const RECT *prcRect
		/// );
		/// </summary>
		[DllModuleExport(28)]
		private uint TrackPopupMenu(uint hMenu, uint uFlags, int x, int y, int nReserved, uint hWnd, uint prcRect)
		{
			_logger.LogInformation("[User32] TrackPopupMenu(hMenu=0x{HMenu:X8}, uFlags=0x{UFlags:X8}, x={X}, y={Y}, nReserved={NReserved}, hWnd=0x{HWnd:X8}, prcRect=0x{PrcRect:X8})",
				hMenu, uFlags, x, y, nReserved, hWnd, prcRect);

			// For stub implementation, return 0 (no item selected or menu cancelled)
			// A full implementation would need to:
			// 1. Display the popup menu at (x, y)
			// 2. Track mouse/keyboard input
			// 3. Return the selected menu item ID or 0 if cancelled

			_logger.LogInformation("[User32] TrackPopupMenu: Stub - returning 0 (no selection)");
			return 0;
		}

		/// <summary>
		/// Converts a character string or a single character to lowercase.
		/// </summary>
		[DllModuleExport(4)]
		private uint CharLowerA(in LpStr lpsz)
		{
			var str = _env.ReadAnsiString(lpsz.Address) ?? "";
			_logger.LogInformation("[User32] CharLowerA(lpsz='{Lpsz}')", str);
			var lower = str.ToLowerInvariant();
			_env.WriteAnsiStringAt(lpsz.Address, lower);
			return lpsz.Address;
		}

		/// <summary>
		/// Converts a specified number of characters in a buffer to uppercase.
		/// </summary>
		[DllModuleExport(8)]
		private uint CharUpperBuffA(in LpStr lpsz, uint cchLength)
		{
			var str = _env.ReadAnsiString(lpsz.Address, (int)cchLength) ?? "";
			_logger.LogInformation("[User32] CharUpperBuffA(lpsz='{Lpsz}', cchLength={CchLength})", str, cchLength);
			var upper = str.ToUpperInvariant();
			_env.WriteAnsiStringAt(lpsz.Address, upper);
			return cchLength;
		}

		/// <summary>
		/// Creates a new shape for the system caret and assigns ownership of the caret to the specified window.
		/// </summary>
		[DllModuleExport(16, IsStub = true)]
		private uint CreateCaret(uint hWnd, uint hBitmap, int nWidth, int nHeight)
		{
			_logger.LogInformation("[User32] CreateCaret(hWnd=0x{HWnd:X8}, hBitmap=0x{HBitmap:X8}, nWidth={NWidth}, nHeight={NHeight})",
				hWnd, hBitmap, nWidth, nHeight);
			return 1;
		}

		/// <summary>
		/// Destroys the caret's current shape, frees the caret from the window, and removes the caret from the screen.
		/// </summary>
		[DllModuleExport(0, IsStub = true)]
		private uint DestroyCaret()
		{
			_logger.LogInformation("[User32] DestroyCaret()");
			return 1;
		}

		/// <summary>
		/// Moves the caret to the specified coordinates in the client area of a window.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint SetCaretPos(int x, int y)
		{
			_logger.LogInformation("[User32] SetCaretPos(x={X}, y={Y})", x, y);
			return 1;
		}

		/// <summary>
		/// Retrieves the current double-click time for the mouse.
		/// </summary>
		[DllModuleExport(0)]
		private uint GetDoubleClickTime()
		{
			_logger.LogInformation("[User32] GetDoubleClickTime()");
			return 500; // 500ms default
		}

		/// <summary>
		/// Deletes a menu item or detaches a submenu from the specified menu.
		/// </summary>
		[DllModuleExport(12, IsStub = true)]
		private uint DeleteMenu(uint hMenu, uint uPosition, uint uFlags)
		{
			_logger.LogInformation("[User32] DeleteMenu(hMenu=0x{HMenu:X8}, uPosition={UPosition}, uFlags=0x{UFlags:X8})",
				hMenu, uPosition, uFlags);
			return 1;
		}

		/// <summary>
		/// Inserts a new menu item into a menu, moving other items down the menu.
		/// </summary>
		[DllModuleExport(20, IsStub = true)]
		private uint InsertMenuA(uint hMenu, uint uPosition, uint uFlags, uint uIDNewItem, in LpcStr lpNewItem)
		{
			var itemName = lpNewItem.Read(_env.Memory) ?? "";
			_logger.LogInformation("[User32] InsertMenuA(hMenu=0x{HMenu:X8}, uPosition={UPosition}, uFlags=0x{UFlags:X8}, uIDNewItem={UIDNewItem}, lpNewItem='{LpNewItem}')",
				hMenu, uPosition, uFlags, uIDNewItem, itemName);
			return 1;
		}

		/// <summary>
		/// Inserts a new menu item at the specified position in a menu.
		/// </summary>
		[DllModuleExport(16, IsStub = true)]
		private uint InsertMenuItemA(uint hMenu, uint uItem, uint fByPosition, uint lpmii)
		{
			_logger.LogInformation("[User32] InsertMenuItemA(hMenu=0x{HMenu:X8}, uItem={UItem}, fByPosition={FByPosition}, lpmii=0x{Lpmii:X8})",
				hMenu, uItem, fByPosition, lpmii);
			return 1;
		}

		/// <summary>
		/// Retrieves information about a menu item.
		/// </summary>
		[DllModuleExport(16, IsStub = true)]
		private uint GetMenuItemInfoA(uint hMenu, uint uItem, uint fByPosition, uint lpmii)
		{
			_logger.LogInformation("[User32] GetMenuItemInfoA(hMenu=0x{HMenu:X8}, uItem={UItem}, fByPosition={FByPosition}, lpmii=0x{Lpmii:X8})",
				hMenu, uItem, fByPosition, lpmii);
			return 0; // Not found
		}

		/// <summary>
		/// Changes information about a menu item.
		/// </summary>
		[DllModuleExport(16, IsStub = true)]
		private uint SetMenuItemInfoA(uint hMenu, uint uItem, uint fByPosition, uint lpmii)
		{
			_logger.LogInformation("[User32] SetMenuItemInfoA(hMenu=0x{HMenu:X8}, uItem={UItem}, fByPosition={FByPosition}, lpmii=0x{Lpmii:X8})",
				hMenu, uItem, fByPosition, lpmii);
			return 1;
		}

		/// <summary>
		/// Sets the default menu item for the specified menu.
		/// </summary>
		[DllModuleExport(12, IsStub = true)]
		private uint SetMenuDefaultItem(uint hMenu, uint uItem, uint fByPos)
		{
			_logger.LogInformation("[User32] SetMenuDefaultItem(hMenu=0x{HMenu:X8}, uItem={UItem}, fByPos={FByPos})",
				hMenu, uItem, fByPos);
			return 1;
		}

		/// <summary>
		/// Scrolls the contents of the specified window's client area.
		/// </summary>
		[DllModuleExport(32, IsStub = true)]
		private uint ScrollWindowEx(uint hWnd, int dx, int dy, uint prcScroll, uint prcClip, uint hrgnUpdate, uint prcUpdate, uint flags)
		{
			_logger.LogInformation("[User32] ScrollWindowEx(hWnd=0x{HWnd:X8}, dx={Dx}, dy={Dy}, flags=0x{Flags:X8})",
				hWnd, dx, dy, flags);
			return 1;
		}

		/// <summary>
		/// Sets the show state and the restored, minimized, and maximized positions of the specified window.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint SetWindowPlacement(uint hWnd, uint lpwndpl)
		{
			_logger.LogInformation("[User32] SetWindowPlacement(hWnd=0x{HWnd:X8}, lpwndpl=0x{Lpwndpl:X8})",
				hWnd, lpwndpl);
			return 1;
		}

		/// <summary>
		/// Animates the caption of a window to indicate the opening of an icon or the minimizing or maximizing of a window.
		/// </summary>
		[DllModuleExport(16, IsStub = true)]
		private uint DrawAnimatedRects(uint hWnd, int idAni, uint lprcFrom, uint lprcTo)
		{
			_logger.LogInformation("[User32] DrawAnimatedRects(hWnd=0x{HWnd:X8}, idAni={IdAni})",
				hWnd, idAni);
			return 1;
		}

		/// <summary>
		/// Empties the clipboard and frees handles to data in the clipboard.
		/// </summary>
		[DllModuleExport(0)]
		private uint EmptyClipboard()
		{
			_logger.LogInformation("[User32] EmptyClipboard()");
			return 1;
		}

		/// <summary>
		/// Places data on the clipboard in a specified clipboard format.
		/// </summary>
		[DllModuleExport(8)]
		private uint SetClipboardData(uint uFormat, uint hMem)
		{
			_logger.LogInformation("[User32] SetClipboardData(uFormat={UFormat}, hMem=0x{HMem:X8})", uFormat, hMem);
			return hMem;
		}

		/// <summary>
		/// Changes the settings of the default display device.
		/// </summary>
		[DllModuleExport(4, IsStub = true)]
		private int ChangeDisplaySettingsA(uint lpDevMode, uint dwFlags)
		{
			_logger.LogInformation("[User32] ChangeDisplaySettingsA(lpDevMode=0x{LpDevMode:X8}, dwFlags=0x{DwFlags:X8})", lpDevMode, dwFlags);
			// DISP_CHANGE_SUCCESSFUL = 0, DISP_CHANGE_RESTART = 1, DISP_CHANGE_FAILED = -1
			// Stub: return success (no restart needed)
			return 0; // DISP_CHANGE_SUCCESSFUL
		}

		/// <summary>
		/// Creates the union of two rectangles.
		/// </summary>
		[DllModuleExport(12, IsStub = true)]
		private uint UnionRect(uint lprcDst, uint lprcSrc1, uint lprcSrc2)
		{
			_logger.LogInformation("[User32] UnionRect(lprcDst=0x{LprcDst:X8}, lprcSrc1=0x{LprcSrc1:X8}, lprcSrc2=0x{LprcSrc2:X8})",
				lprcDst, lprcSrc1, lprcSrc2);

			if (lprcDst == 0 || lprcSrc1 == 0 || lprcSrc2 == 0)
				return 0;

			// Read RECT structures
			var rect1 = _env.MemReadStruct<NativeTypes.RECT>(lprcSrc1);
			var rect2 = _env.MemReadStruct<NativeTypes.RECT>(lprcSrc2);

			// Compute union
			var result = new NativeTypes.RECT
			{
				left = Math.Min(rect1.left, rect2.left),
				top = Math.Min(rect1.top, rect2.top),
				right = Math.Max(rect1.right, rect2.right),
				bottom = Math.Max(rect1.bottom, rect2.bottom)
			};

			// Write result
			_env.MemWriteStruct(lprcDst, ref result);

			return 1; // TRUE
		}

		/// <summary>
		/// Retrieves a device context with extended options.
		/// </summary>
		[DllModuleExport(12, IsStub = true)]
		private uint GetDCEx(uint hWnd, uint hrgnClip, uint flags)
		{
			_logger.LogInformation("[User32] GetDCEx(hWnd=0x{HWnd:X8}, hrgnClip=0x{HrgnClip:X8}, flags=0x{Flags:X8})",
				hWnd, hrgnClip, flags);
			// Stub: return a fake DC handle
			return 0x12340000;
		}

		/// <summary>
		/// Retrieves the identifier of the thread that created the specified window.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint GetWindowThreadProcessId(uint hWnd, uint lpdwProcessId)
		{
			_logger.LogInformation("[User32] GetWindowThreadProcessId(hWnd=0x{HWnd:X8}, lpdwProcessId=0x{LpdwProcessId:X8})",
				hWnd, lpdwProcessId);

			// Return fake thread ID
			var threadId = 1u;
			var processId = 1000u;

			if (lpdwProcessId != 0)
			{
				_env.MemWrite32(lpdwProcessId, processId);
			}

			return threadId;
		}

		/// <summary>
		/// Synthesizes a keystroke.
		/// </summary>
		[DllModuleExport(16, IsStub = true)]
		private void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo)
		{
			_logger.LogInformation("[User32] keybd_event(bVk={BVk}, bScan={BScan}, dwFlags=0x{DwFlags:X8}, dwExtraInfo=0x{DwExtraInfo:X8})",
				bVk, bScan, dwFlags, dwExtraInfo);
			// Stub: no-op (keyboard input not simulated)
		}

		/// <summary>
		/// Creates, displays, and operates a message box from a MSGBOXPARAMS structure.
		/// </summary>
		[DllModuleExport(4, IsStub = true)]
		private uint MessageBoxIndirectA(uint lpMsgBoxParams)
		{
			_logger.LogInformation("[User32] MessageBoxIndirectA(lpMsgBoxParams=0x{LpMsgBoxParams:X8})", lpMsgBoxParams);

			if (lpMsgBoxParams == 0)
				return 0;

			var msgBoxParams = _env.MemReadStruct<NativeTypes.MSGBOXPARAMS>(lpMsgBoxParams);

			var text = msgBoxParams.lpszText != 0 ? _env.ReadAnsiString(msgBoxParams.lpszText) : "";
			var caption = msgBoxParams.lpszCaption != 0 ? _env.ReadAnsiString(msgBoxParams.lpszCaption) : "";

			_logger.LogInformation("[User32] MessageBoxIndirectA: '{Caption}' - '{Text}' (style=0x{Style:X8})",
				caption, text, msgBoxParams.dwStyle);

			// Return IDOK (1)
			return 1;
		}

		/// <summary>
		/// Determines whether the specified rectangle is empty.
		/// </summary>
		[DllModuleExport(4, IsStub = true)]
		private uint IsRectEmpty(uint lprc)
		{
			_logger.LogInformation("[User32] IsRectEmpty(lprc=0x{Lprc:X8})", lprc);

			if (lprc == 0)
				return 1; // TRUE - null pointer considered empty

			var rect = _env.MemReadStruct<NativeTypes.RECT>(lprc);

			// Rectangle is empty if width or height is <= 0
			return (rect.right <= rect.left || rect.bottom <= rect.top) ? 1u : 0u;
		}

		/// <summary>
		/// Synthesizes mouse motion and button clicks.
		/// </summary>
		[DllModuleExport(20, IsStub = true)]
		private void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo)
		{
			_logger.LogInformation("[User32] mouse_event(dwFlags=0x{DwFlags:X8}, dx={Dx}, dy={Dy}, dwData={DwData}, dwExtraInfo=0x{DwExtraInfo:X8})",
				dwFlags, dx, dy, dwData, dwExtraInfo);
			// Stub: no-op (mouse input not simulated)
		}

		#endregion
	}
}
