namespace Win32Emu.Win32.Messaging;

/// <summary>
/// Common Win32 message identifiers
/// </summary>
public enum WM : uint
{
	NULL = 0x0000,
	CREATE = 0x0001,
	DESTROY = 0x0002,
	MOVE = 0x0003,
	SIZE = 0x0005,
	ACTIVATE = 0x0006,
	SETFOCUS = 0x0007,
	KILLFOCUS = 0x0008,
	ENABLE = 0x000A,
	PAINT = 0x000F,
	CLOSE = 0x0010,
	QUIT = 0x0012,
	ERASEBKGND = 0x0014,
	ACTIVATEAPP = 0x001C,
	
	// Keyboard messages
	KEYDOWN = 0x0100,
	KEYUP = 0x0101,
	CHAR = 0x0102,
	SYSKEYDOWN = 0x0104,
	SYSKEYUP = 0x0105,
	
	// Mouse messages
	MOUSEMOVE = 0x0200,
	LBUTTONDOWN = 0x0201,
	LBUTTONUP = 0x0202,
	RBUTTONDOWN = 0x0204,
	RBUTTONUP = 0x0205,
	MBUTTONDOWN = 0x0207,
	MBUTTONUP = 0x0208,
	
	// Control messages
	COMMAND = 0x0111,
	SYSCOMMAND = 0x0112,
	TIMER = 0x0113,
	
	// User messages start at 0x0400
	USER = 0x0400
}

/// <summary>
/// WM_CREATE message
/// </summary>
public record CreateMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.CREATE, WParam, LParam);

/// <summary>
/// WM_DESTROY message
/// </summary>
public record DestroyMessage(uint Hwnd) 
	: Win32Message(Hwnd, (uint)WM.DESTROY, 0, 0);

/// <summary>
/// WM_PAINT message
/// </summary>
public record PaintMessage(uint Hwnd) 
	: Win32Message(Hwnd, (uint)WM.PAINT, 0, 0);

/// <summary>
/// WM_CLOSE message
/// </summary>
public record CloseMessage(uint Hwnd) 
	: Win32Message(Hwnd, (uint)WM.CLOSE, 0, 0);

/// <summary>
/// WM_COMMAND message
/// </summary>
public record CommandMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.COMMAND, WParam, LParam)
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
	: Win32Message(Hwnd, (uint)WM.LBUTTONDOWN, WParam, LParam)
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
	: Win32Message(Hwnd, (uint)WM.LBUTTONUP, WParam, LParam)
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
	: Win32Message(Hwnd, (uint)WM.KEYDOWN, WParam, LParam)
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
	: Win32Message(Hwnd, (uint)WM.KEYUP, WParam, LParam)
{
	/// <summary>
	/// Virtual key code
	/// </summary>
	public uint VirtualKeyCode => WParam;
}

/// <summary>
/// WM_MOVE message
/// </summary>
public record MoveMessage(uint Hwnd, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.MOVE, 0, LParam)
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
/// WM_SIZE message
/// </summary>
public record SizeMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.SIZE, WParam, LParam)
{
	/// <summary>
	/// Width (LOWORD of lParam)
	/// </summary>
	public ushort Width => (ushort)(LParam & 0xFFFF);
	
	/// <summary>
	/// Height (HIWORD of lParam)
	/// </summary>
	public ushort Height => (ushort)((LParam >> 16) & 0xFFFF);
	
	/// <summary>
	/// Type of resizing (SIZE_RESTORED = 0, SIZE_MINIMIZED = 1, SIZE_MAXIMIZED = 2, etc.)
	/// </summary>
	public uint SizeType => WParam;
}

/// <summary>
/// WM_ACTIVATE message
/// </summary>
public record ActivateMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.ACTIVATE, WParam, LParam)
{
	/// <summary>
	/// Active flag (LOWORD of wParam)
	/// </summary>
	public uint ActiveFlag => WParam & 0xFFFF;
	
	/// <summary>
	/// Minimized flag (HIWORD of wParam)
	/// </summary>
	public bool IsMinimized => ((WParam >> 16) & 0xFFFF) != 0;
	
	/// <summary>
	/// Handle of window being activated/deactivated
	/// </summary>
	public uint OtherWindow => LParam;
}

/// <summary>
/// WM_MOUSEMOVE message
/// </summary>
public record MouseMoveMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.MOUSEMOVE, WParam, LParam)
{
	/// <summary>
	/// X coordinate (LOWORD of lParam)
	/// </summary>
	public int X => (short)(LParam & 0xFFFF);
	
	/// <summary>
	/// Y coordinate (HIWORD of lParam)
	/// </summary>
	public int Y => (short)((LParam >> 16) & 0xFFFF);
	
	/// <summary>
	/// Key state flags
	/// </summary>
	public uint KeyFlags => WParam;
}

/// <summary>
/// WM_RBUTTONDOWN message
/// </summary>
public record RButtonDownMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.RBUTTONDOWN, WParam, LParam)
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
/// WM_RBUTTONUP message
/// </summary>
public record RButtonUpMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.RBUTTONUP, WParam, LParam)
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
/// WM_CHAR message
/// </summary>
public record CharMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.CHAR, WParam, LParam)
{
	/// <summary>
	/// Character code
	/// </summary>
	public uint CharCode => WParam;
	
	/// <summary>
	/// Repeat count
	/// </summary>
	public uint RepeatCount => LParam & 0xFFFF;
}

/// <summary>
/// WM_TIMER message
/// </summary>
public record TimerMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.TIMER, WParam, LParam)
{
	/// <summary>
	/// Timer identifier
	/// </summary>
	public uint TimerId => WParam;
	
	/// <summary>
	/// Timer procedure address (optional)
	/// </summary>
	public uint TimerProc => LParam;
}

/// <summary>
/// WM_ERASEBKGND message
/// </summary>
public record EraseBackgroundMessage(uint Hwnd, uint WParam) 
	: Win32Message(Hwnd, (uint)WM.ERASEBKGND, WParam, 0)
{
	/// <summary>
	/// Device context handle
	/// </summary>
	public uint HDC => WParam;
}

/// <summary>
/// WM_QUIT message
/// </summary>
public record QuitMessage(uint ExitCode) 
	: Win32Message(0, (uint)WM.QUIT, ExitCode, 0)
{
	/// <summary>
	/// Exit code for the application
	/// </summary>
	public uint ExitCode => WParam;
}
