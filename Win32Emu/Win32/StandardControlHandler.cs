using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32;

/// <summary>
/// Handles window messages for standard Win32 controls (BUTTON, EDIT, etc.)
/// Routes messages to the Avalonia frontend via the host
/// </summary>
public class StandardControlHandler
{
	private readonly ProcessEnvironment _env;
	private readonly IEmulatorHost? _host;
	private readonly ILogger _logger;

	public StandardControlHandler(ProcessEnvironment env, IEmulatorHost? host = null, ILogger? logger = null)
	{
		_env = env;
		_host = host;
		_logger = logger ?? NullLogger.Instance;
	}

	/// <summary>
	/// Handle a message for a standard control
	/// </summary>
	public uint HandleMessage(uint hwnd, uint msg, uint wParam, uint lParam, string className)
	{
		_logger.LogDebug("[StandardControlHandler] HWND=0x{Hwnd:X8} Class='{ClassName}' MSG=0x{Msg:X4}", hwnd, msg, className);

		return className.ToUpperInvariant() switch
		{
			"BUTTON" => HandleButtonMessage(hwnd, (WindowNotifications)msg, wParam, lParam),
			"EDIT" => HandleEditMessage(hwnd, msg, wParam, lParam),
			"STATIC" => HandleStaticMessage(hwnd, msg, wParam, lParam),
			"LISTBOX" => HandleListBoxMessage(hwnd, msg, wParam, lParam),
			"COMBOBOX" => HandleComboBoxMessage(hwnd, msg, wParam, lParam),
			"SCROLLBAR" => HandleScrollBarMessage(hwnd, msg, wParam, lParam),
			_ => throw new NotImplementedException($"Unknown Control Type {className}") // Unknown control type
		};
	}

	enum WindowNotifications : uint
	{
		WM_CREATE = 0x0001,
		WM_DESTROY = 0x0002,
		WM_PAINT = 0x000F,
		WM_COMMAND = 0x0111,
		WM_ERASEBKGND = 0x0014,

		WM_ACTIVATEAPP = 0x001C,
		WM_CANCELMODE = 0x001F,
		WM_CHILDACTIVATE = 0x0022,
		WM_CLOSE = 0x0010,
		WM_COMPACTING = 0x0041,
		WM_ENABLE = 0x000A,
		WM_ENTERSIZEMOVE = 0x0231,
		WM_EXITSIZEMOVE = 0x0232,
		WM_GETICON = 0x007F,
		WM_GETMINMAXINFO = 0x0024,
		WM_INPUTLANGCHANGE = 0x0051,
		WM_INPUTLANGCHANGEREQUEST = 0x0050,
		WM_MOVE = 0x0003,
		WM_MOVING = 0x0216,
		WM_NCACTIVATE = 0x0086,
		WM_NCCALCSIZE = 0x0083,
		WM_NCCREATE = 0x0081,
		WM_NCDESTROY = 0x0082,
		WM_NULL = 0x0000,
		WM_QUERYDRAGICON = 0x0037,
		WM_QUERYOPEN = 0x0013,
		WM_QUIT = 0x0012,
		WM_SHOWWINDOW = 0x0018,
		WM_SIZE = 0x0005,
		WM_SIZING = 0x0214,
		WM_STYLECHANGED = 0x007D,
		WM_STYLECHANGING = 0x007C,
		WM_THEMECHANGED = 0x031A,
		WM_USERCHANGED = 0x0054,
		WM_WINDOWPOSCHANGED = 0x0047,
		WM_WINDOWPOSCHANGING = 0x0046,
		WM_NCPAINT = 0x0085,

		WM_CAPTURECHANGED = 0x0215,
		WM_LBUTTONDBLCLK = 0x0203,
		WM_LBUTTONDOWN = 0x0201,
		WM_LBUTTONUP = 0x0202,
		WM_MBUTTONDBLCLK = 0x0209,
		WM_MBUTTONDOWN = 0x0207,
		WM_MBUTTONUP = 0x0208,
		WM_MOUSEACTIVATE = 0x0021,
		WM_MOUSEHOVER = 0x02A1,
		WM_MOUSEHWHEEL = 0x020E,
		WM_MOUSELEAVE = 0x02A3,
		WM_MOUSEMOVE = 0x0200,
		WM_MOUSEWHEEL = 0x020A,
		WM_NCHITTEST = 0x0084,
		WM_NCLBUTTONDBLCLK = 0x00A3,
		WM_NCLBUTTONDOWN = 0x00A1,
		WM_NCLBUTTONUP = 0x00A2,
		WM_NCMBUTTONDBLCLK = 0x00A9,
		WM_NCMBUTTONDOWN = 0x00A7,
		WM_NCMBUTTONUP = 0x00A8,
		WM_NCMOUSEHOVER = 0x02A0,
		WM_NCMOUSELEAVE = 0x02A2,
		WM_NCMOUSEMOVE = 0x00A0,
		WM_NCRBUTTONDBLCLK = 0x00A6,
		WM_NCRBUTTONDOWN = 0x00A4,
		WM_NCRBUTTONUP = 0x00A5,
		WM_NCXBUTTONDBLCLK = 0x00AD,
		WM_NCXBUTTONDOWN = 0x00AB,
		WM_NCXBUTTONUP = 0x00AC,
		WM_RBUTTONDBLCLK = 0x0206,
		WM_RBUTTONDOWN = 0x0204,
		WM_RBUTTONUP = 0x0205,
		WM_XBUTTONDBLCLK = 0x020D,
		WM_XBUTTONDOWN = 0x020B,
		WM_XBUTTONUP = 0x020C,

		// Button control messages
		BM_CLICK = 0x00F1
	}

	private uint HandleButtonMessage(uint hwnd, WindowNotifications msg, uint wParam, uint lParam)
	{
		switch (msg)
		{
			case WindowNotifications.WM_CREATE: // WM_CREATE
				_logger.LogDebug("[Button] WM_CREATE");
				return 0;

			case WindowNotifications.WM_PAINT: // WM_PAINT
				_logger.LogDebug("[Button] WM_PAINT");
				// Let Avalonia handle the painting
				return 0;

			case WindowNotifications.WM_ERASEBKGND: // WM_ERASEBKGND
				return 1; // Background erased

			case WindowNotifications.WM_NCPAINT: // WM_NCPAINT
				return 0;

			case WindowNotifications.WM_LBUTTONDOWN:
				_logger.LogDebug("[Button] WM_LBUTTONDOWN");
				// No mouse capture or state change implemented
				return 0;

			case WindowNotifications.WM_LBUTTONUP:
				_logger.LogDebug("[Button] WM_LBUTTONUP");
				// Send BN_CLICKED notification to parent (no mouse capture or state change implemented)
				SendButtonNotification(hwnd, NotificationCode.BN_CLICKED); // BN_CLICKED = 0
				return 0;

			case WindowNotifications.BM_CLICK:
				_logger.LogDebug("[Button] BM_CLICK - simulating button click");
				// BM_CLICK simulates a button click by sending the notification directly
				SendButtonNotification(hwnd, NotificationCode.BN_CLICKED);
				return 0;

			default:
				_logger.LogDebug("[Button] Unhandled message {Msg}(0x{MsgValue:X4})", msg, (uint)msg);
				return 0;
		}
	}

	enum NotificationCode : uint
	{
		BN_CLICKED = 0,
		BN_PAINT = 1,
		BN_HILITE = 2,
		BN_UNHILITE = 3,
		BN_DISABLE = 4,
		BN_DOUBLECLICKED = 5,
		BN_PUSHED = BN_HILITE,
		BN_UNPUSHED = BN_UNHILITE,
		BN_DBLCLK = BN_DOUBLECLICKED,
		BN_SETFOCUS = 6,
		BN_KILLFOCUS = 7
	}

	/// <summary>
	/// Send a button notification (WM_COMMAND) to the parent window
	/// </summary>
	private void SendButtonNotification(uint buttonHwnd, NotificationCode notificationCode)
	{
		// Get the button's window info to find parent
		var windowInfo = _env.GetWindow(buttonHwnd);
		if (!windowInfo.HasValue)
		{
			_logger.LogWarning("[Button] Cannot send notification: window info not found for HWND=0x{ButtonHwnd:X8}", buttonHwnd);
			return;
		}

		var parentHwnd = windowInfo.Value.Parent;
		if (parentHwnd == 0)
		{
			_logger.LogWarning("[Button] Cannot send notification: no parent for button HWND=0x{ButtonHwnd:X8}", buttonHwnd);
			return;
		}

		// Get control ID from the button's menu field (for child windows, this is the control ID)
		var controlId = windowInfo.Value.Menu;

		// Build WM_COMMAND wParam: HIWORD = notification code, LOWORD = control ID
		var wParam = (((uint)notificationCode & 0xFFFF) << 16) | (controlId & 0xFFFF);

		// Post WM_COMMAND to parent window
		_logger.LogInformation("[Button] Sending WM_COMMAND to parent 0x{ParentHwnd:X8}: controlId={ControlId}, notification={NotificationCode}(0x{NotificationCodeValue:X4})", parentHwnd, controlId, notificationCode, (uint)notificationCode);
		_env.PostMessage(parentHwnd, (uint)WindowNotifications.WM_COMMAND, wParam, buttonHwnd);
	}

	private uint HandleEditMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		switch (msg)
		{
			case 0x0001: // WM_CREATE
				_logger.LogDebug("[Edit] WM_CREATE");
				return 0;

			case 0x000C: // WM_SETTEXT
				_logger.LogDebug("[Edit] WM_SETTEXT");
				// TODO: Notify Avalonia to update text
				return 1; // TRUE

			case 0x000D: // WM_GETTEXT
				_logger.LogDebug("[Edit] WM_GETTEXT");
				// TODO: Get text from Avalonia control
				return 0;

			case 0x000E: // WM_GETTEXTLENGTH
				_logger.LogDebug("[Edit] WM_GETTEXTLENGTH");
				// TODO: Get text length from Avalonia control
				return 0;

			case 0x000F: // WM_PAINT
				_logger.LogDebug("[Edit] WM_PAINT");
				return 0;

			default:
				_logger.LogDebug("[Edit] Unhandled message 0x{Msg:X4}", msg);
				return 0;
		}
	}

	private uint HandleStaticMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		switch (msg)
		{
			case 0x0001: // WM_CREATE
				_logger.LogDebug("[Static] WM_CREATE");
				return 0;

			case 0x000C: // WM_SETTEXT
				_logger.LogDebug("[Static] WM_SETTEXT");
				// TODO: Notify Avalonia to update text
				return 1; // TRUE

			case 0x000F: // WM_PAINT
				_logger.LogDebug("[Static] WM_PAINT");
				return 0;

			default:
				_logger.LogDebug("[Static] Unhandled message 0x{Msg:X4}", msg);
				return 0;
		}
	}

	private uint HandleListBoxMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		_logger.LogDebug("[ListBox] Message 0x{Msg:X4}", msg);
		return 0;
	}

	private uint HandleComboBoxMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		_logger.LogDebug("[ComboBox] Message 0x{Msg:X4}", msg);
		return 0;
	}

	private uint HandleScrollBarMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		_logger.LogDebug("[ScrollBar] Message 0x{Msg:X4}", msg);
		return 0;
	}

	/// <summary>
	/// Check if a class name is a standard control
	/// </summary>
	public static bool IsStandardControl(string className)
	{
		return className.ToUpperInvariant() switch
		{
			"BUTTON" or "EDIT" or "STATIC" or "LISTBOX" or "COMBOBOX" or "SCROLLBAR" => true,
			_ => false
		};
	}
}
