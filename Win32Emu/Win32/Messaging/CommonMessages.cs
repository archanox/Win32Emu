namespace Win32Emu.Win32.Messaging;

/// <summary>
/// Common Win32 message identifiers
/// </summary>
public static class WM
{
	public const uint NULL = 0x0000;
	public const uint CREATE = 0x0001;
	public const uint DESTROY = 0x0002;
	public const uint MOVE = 0x0003;
	public const uint SIZE = 0x0005;
	public const uint ACTIVATE = 0x0006;
	public const uint SETFOCUS = 0x0007;
	public const uint KILLFOCUS = 0x0008;
	public const uint ENABLE = 0x000A;
	public const uint PAINT = 0x000F;
	public const uint CLOSE = 0x0010;
	public const uint QUIT = 0x0012;
	public const uint ERASEBKGND = 0x0014;
	public const uint ACTIVATEAPP = 0x001C;
	
	// Keyboard messages
	public const uint KEYDOWN = 0x0100;
	public const uint KEYUP = 0x0101;
	public const uint CHAR = 0x0102;
	public const uint SYSKEYDOWN = 0x0104;
	public const uint SYSKEYUP = 0x0105;
	
	// Mouse messages
	public const uint MOUSEMOVE = 0x0200;
	public const uint LBUTTONDOWN = 0x0201;
	public const uint LBUTTONUP = 0x0202;
	public const uint RBUTTONDOWN = 0x0204;
	public const uint RBUTTONUP = 0x0205;
	public const uint MBUTTONDOWN = 0x0207;
	public const uint MBUTTONUP = 0x0208;
	
	// Control messages
	public const uint COMMAND = 0x0111;
	public const uint SYSCOMMAND = 0x0112;
	public const uint TIMER = 0x0113;
	
	// User messages start at 0x0400
	public const uint USER = 0x0400;
}

/// <summary>
/// WM_CREATE message
/// </summary>
public record CreateMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, WM.CREATE, WParam, LParam);

/// <summary>
/// WM_DESTROY message
/// </summary>
public record DestroyMessage(uint Hwnd) 
	: Win32Message(Hwnd, WM.DESTROY, 0, 0);

/// <summary>
/// WM_PAINT message
/// </summary>
public record PaintMessage(uint Hwnd) 
	: Win32Message(Hwnd, WM.PAINT, 0, 0);

/// <summary>
/// WM_CLOSE message
/// </summary>
public record CloseMessage(uint Hwnd) 
	: Win32Message(Hwnd, WM.CLOSE, 0, 0);

/// <summary>
/// WM_COMMAND message
/// </summary>
public record CommandMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, WM.COMMAND, WParam, LParam)
{
	/// <summary>
	/// Control ID (LOWORD of wParam)
	/// </summary>
	public uint ControlId => WParam & 0xFFFF;
	
	/// <summary>
	/// Notification code (HIWORD of wParam)
	/// </summary>
	public uint NotificationCode => (WParam >> 16) & 0xFFFF;
	
	/// <summary>
	/// Control window handle (lParam)
	/// </summary>
	public uint ControlHandle => LParam;
}

/// <summary>
/// WM_LBUTTONDOWN message
/// </summary>
public record LButtonDownMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, WM.LBUTTONDOWN, WParam, LParam)
{
	/// <summary>
	/// X coordinate (LOWORD of lParam)
	/// </summary>
	public int X => (short)(LParam & 0xFFFF);
	
	/// <summary>
	/// Y coordinate (HIWORD of lParam)
	/// </summary>
	public int Y => (short)((LParam >> 16) & 0xFFFF);
}

/// <summary>
/// WM_LBUTTONUP message
/// </summary>
public record LButtonUpMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, WM.LBUTTONUP, WParam, LParam)
{
	/// <summary>
	/// X coordinate (LOWORD of lParam)
	/// </summary>
	public int X => (short)(LParam & 0xFFFF);
	
	/// <summary>
	/// Y coordinate (HIWORD of lParam)
	/// </summary>
	public int Y => (short)((LParam >> 16) & 0xFFFF);
}

/// <summary>
/// WM_KEYDOWN message
/// </summary>
public record KeyDownMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, WM.KEYDOWN, WParam, LParam)
{
	/// <summary>
	/// Virtual key code
	/// </summary>
	public uint VirtualKeyCode => WParam;
	
	/// <summary>
	/// Repeat count (bits 0-15 of lParam)
	/// </summary>
	public uint RepeatCount => LParam & 0xFFFF;
	
	/// <summary>
	/// Scan code (bits 16-23 of lParam)
	/// </summary>
	public uint ScanCode => (LParam >> 16) & 0xFF;
}

/// <summary>
/// WM_KEYUP message
/// </summary>
public record KeyUpMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, WM.KEYUP, WParam, LParam)
{
	/// <summary>
	/// Virtual key code
	/// </summary>
	public uint VirtualKeyCode => WParam;
}
