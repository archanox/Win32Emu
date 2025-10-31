using Avalonia.Input;

namespace Win32Emu.Gui.Utilities;

/// <summary>
/// Maps Avalonia input events to Win32 virtual key codes and mouse messages
/// </summary>
public static class Win32InputMapper
{
	/// <summary>
	/// Maps Avalonia Key to Win32 Virtual Key code
	/// Based on Windows Virtual-Key Codes: https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
	/// </summary>
	public static byte MapKeyToVirtualKeyCode(Key key)
	{
		return key switch
		{
			// Letters A-Z
			Key.A => 0x41,
			Key.B => 0x42,
			Key.C => 0x43,
			Key.D => 0x44,
			Key.E => 0x45,
			Key.F => 0x46,
			Key.G => 0x47,
			Key.H => 0x48,
			Key.I => 0x49,
			Key.J => 0x4A,
			Key.K => 0x4B,
			Key.L => 0x4C,
			Key.M => 0x4D,
			Key.N => 0x4E,
			Key.O => 0x4F,
			Key.P => 0x50,
			Key.Q => 0x51,
			Key.R => 0x52,
			Key.S => 0x53,
			Key.T => 0x54,
			Key.U => 0x55,
			Key.V => 0x56,
			Key.W => 0x57,
			Key.X => 0x58,
			Key.Y => 0x59,
			Key.Z => 0x5A,
			
			// Numbers 0-9
			Key.D0 => 0x30,
			Key.D1 => 0x31,
			Key.D2 => 0x32,
			Key.D3 => 0x33,
			Key.D4 => 0x34,
			Key.D5 => 0x35,
			Key.D6 => 0x36,
			Key.D7 => 0x37,
			Key.D8 => 0x38,
			Key.D9 => 0x39,
			
			// Numpad
			Key.NumPad0 => 0x60,
			Key.NumPad1 => 0x61,
			Key.NumPad2 => 0x62,
			Key.NumPad3 => 0x63,
			Key.NumPad4 => 0x64,
			Key.NumPad5 => 0x65,
			Key.NumPad6 => 0x66,
			Key.NumPad7 => 0x67,
			Key.NumPad8 => 0x68,
			Key.NumPad9 => 0x69,
			
			// Function keys
			Key.F1 => 0x70,
			Key.F2 => 0x71,
			Key.F3 => 0x72,
			Key.F4 => 0x73,
			Key.F5 => 0x74,
			Key.F6 => 0x75,
			Key.F7 => 0x76,
			Key.F8 => 0x77,
			Key.F9 => 0x78,
			Key.F10 => 0x79,
			Key.F11 => 0x7A,
			Key.F12 => 0x7B,
			
			// Special keys
			Key.Back => 0x08,      // VK_BACK
			Key.Tab => 0x09,       // VK_TAB
			Key.Enter => 0x0D,     // VK_RETURN
			Key.LeftShift => 0xA0, // VK_LSHIFT
			Key.RightShift => 0xA1,// VK_RSHIFT
			Key.LeftCtrl => 0xA2,  // VK_LCONTROL
			Key.RightCtrl => 0xA3, // VK_RCONTROL
			Key.LeftAlt => 0xA4,   // VK_LMENU
			Key.RightAlt => 0xA5,  // VK_RMENU
			Key.Pause => 0x13,     // VK_PAUSE
			Key.CapsLock => 0x14,  // VK_CAPITAL
			Key.Escape => 0x1B,    // VK_ESCAPE
			Key.Space => 0x20,     // VK_SPACE
			Key.PageUp => 0x21,    // VK_PRIOR
			Key.PageDown => 0x22,  // VK_NEXT
			Key.End => 0x23,       // VK_END
			Key.Home => 0x24,      // VK_HOME
			Key.Left => 0x25,      // VK_LEFT
			Key.Up => 0x26,        // VK_UP
			Key.Right => 0x27,     // VK_RIGHT
			Key.Down => 0x28,      // VK_DOWN
			Key.Insert => 0x2D,    // VK_INSERT
			Key.Delete => 0x2E,    // VK_DELETE
			
			// Numpad operations
			Key.Multiply => 0x6A,  // VK_MULTIPLY
			Key.Add => 0x6B,       // VK_ADD
			Key.Subtract => 0x6D,  // VK_SUBTRACT
			Key.Decimal => 0x6E,   // VK_DECIMAL
			Key.Divide => 0x6F,    // VK_DIVIDE
			
			// OEM keys
			Key.OemSemicolon => 0xBA,  // VK_OEM_1 (;:)
			Key.OemPlus => 0xBB,       // VK_OEM_PLUS (=+)
			Key.OemComma => 0xBC,      // VK_OEM_COMMA (,<)
			Key.OemMinus => 0xBD,      // VK_OEM_MINUS (-_)
			Key.OemPeriod => 0xBE,     // VK_OEM_PERIOD (.>)
			Key.OemQuestion => 0xBF,   // VK_OEM_2 (/?)
			Key.OemTilde => 0xC0,      // VK_OEM_3 (`~)
			Key.OemOpenBrackets => 0xDB, // VK_OEM_4 ([{)
			Key.OemPipe => 0xDC,       // VK_OEM_5 (\|)
			Key.OemCloseBrackets => 0xDD, // VK_OEM_6 (]})
			Key.OemQuotes => 0xDE,     // VK_OEM_7 ('")
			
			// Default for unmapped keys
			_ => 0x00
		};
	}
	
	/// <summary>
	/// Gets the modifier key state for Win32 messages
	/// </summary>
	public static uint GetKeyModifiers(KeyModifiers modifiers)
	{
		uint result = 0;
		if ((modifiers & KeyModifiers.Shift) != 0)
			result |= 0x0004; // MK_SHIFT
		if ((modifiers & KeyModifiers.Control) != 0)
			result |= 0x0008; // MK_CONTROL
		
		return result;
	}
	
	/// <summary>
	/// Gets the mouse button state for Win32 messages
	/// </summary>
	/// <param name="properties">Pointer point properties from Avalonia containing button states</param>
	/// <returns>Win32 button state flags (MK_LBUTTON, MK_RBUTTON, MK_MBUTTON)</returns>
	public static uint GetMouseButtonState(PointerPointProperties properties)
	{
		uint result = 0;
		if (properties.IsLeftButtonPressed)
			result |= 0x0001; // MK_LBUTTON
		if (properties.IsRightButtonPressed)
			result |= 0x0002; // MK_RBUTTON
		if (properties.IsMiddleButtonPressed)
			result |= 0x0010; // MK_MBUTTON
		
		return result;
	}
	
	/// <summary>
	/// Creates lParam for mouse messages (position in window coordinates)
	/// </summary>
	public static uint MakeMouseLParam(double x, double y)
	{
		// Clamp to valid range for 16-bit signed coordinates
		short xPos = (short)Math.Clamp(x, short.MinValue, short.MaxValue);
		short yPos = (short)Math.Clamp(y, short.MinValue, short.MaxValue);
		
		// Pack into lParam: LOWORD = x, HIWORD = y
		return (uint)(((ushort)yPos << 16) | ((ushort)xPos & 0xFFFF));
	}
}
