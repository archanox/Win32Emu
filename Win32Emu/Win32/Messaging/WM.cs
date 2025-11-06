namespace Win32Emu.Win32.Messaging
{
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
}