namespace Win32Emu.Win32.Input;

/// <summary>
/// Maps between Win32 virtual key codes (VK_*), DirectInput scan codes (DIK_*),
/// and SDL3 hardware scan codes.
///
/// DirectInput keyboard offsets are the same as PC XT / PS2 keyboard scan codes (Set 1).
/// These differ from both Win32 virtual key codes and SDL3 USB-HID-based scancodes.
/// </summary>
public static class KeyCodeMapper
{
	/// <summary>
	/// Maps Win32 virtual key code (VK_*) to DirectInput scan code (DIK_*).
	/// Returns 0 for unmapped keys.
	/// </summary>
	private static readonly int[] VkToDikTable = BuildVkToDikTable();

	/// <summary>
	/// Maps SDL3 scancode (USB-HID based) to Win32 virtual key code (VK_*).
	/// SDL3 scancodes go up to 512; the common ones are 4–231.
	/// Returns 0 for unmapped scancodes.
	/// </summary>
	private static readonly int[] SdlToVkTable = BuildSdlToVkTable();

	/// <summary>
	/// Converts a Win32 virtual key code (VK_*) to a DirectInput scan code (DIK_*).
	/// </summary>
	/// <param name="vk">Win32 virtual key code (0–255).</param>
	/// <returns>DirectInput scan code, or 0 if the key has no DIK equivalent.</returns>
	public static int VkToDik(int vk)
	{
		if ((uint)vk >= (uint)VkToDikTable.Length)
		{
			return 0;
		}

		return VkToDikTable[vk];
	}

	/// <summary>
	/// Converts an SDL3 scancode (USB-HID based) to a Win32 virtual key code (VK_*).
	/// </summary>
	/// <param name="sdlScancode">SDL3 scancode value.</param>
	/// <returns>Win32 virtual key code, or 0 if the scancode has no VK equivalent.</returns>
	public static int SdlScancodeToVk(int sdlScancode)
	{
		if ((uint)sdlScancode >= (uint)SdlToVkTable.Length)
		{
			return 0;
		}

		return SdlToVkTable[sdlScancode];
	}

	private static int[] BuildVkToDikTable()
	{
		var table = new int[256];

		// Backspace, Tab
		table[0x08] = 0x0E; // VK_BACK       → DIK_BACK
		table[0x09] = 0x0F; // VK_TAB        → DIK_TAB

		// Return, Pause, CapsLock, Escape, Space
		table[0x0D] = 0x1C; // VK_RETURN     → DIK_RETURN
		table[0x13] = 0xC5; // VK_PAUSE      → DIK_PAUSE
		table[0x14] = 0x3A; // VK_CAPITAL    → DIK_CAPITAL
		table[0x1B] = 0x01; // VK_ESCAPE     → DIK_ESCAPE
		table[0x20] = 0x39; // VK_SPACE      → DIK_SPACE

		// Navigation (extended scan codes)
		table[0x21] = 0xC9; // VK_PRIOR      → DIK_PRIOR (Page Up)
		table[0x22] = 0xD1; // VK_NEXT       → DIK_NEXT  (Page Down)
		table[0x23] = 0xCF; // VK_END        → DIK_END
		table[0x24] = 0xC7; // VK_HOME       → DIK_HOME
		table[0x25] = 0xCB; // VK_LEFT       → DIK_LEFT
		table[0x26] = 0xC8; // VK_UP         → DIK_UP
		table[0x27] = 0xCD; // VK_RIGHT      → DIK_RIGHT
		table[0x28] = 0xD0; // VK_DOWN       → DIK_DOWN
		table[0x2D] = 0xD2; // VK_INSERT     → DIK_INSERT
		table[0x2E] = 0xD3; // VK_DELETE     → DIK_DELETE

		// Digits 0-9
		table[0x30] = 0x0B; // VK_0 → DIK_0
		table[0x31] = 0x02; // VK_1 → DIK_1
		table[0x32] = 0x03; // VK_2 → DIK_2
		table[0x33] = 0x04; // VK_3 → DIK_3
		table[0x34] = 0x05; // VK_4 → DIK_4
		table[0x35] = 0x06; // VK_5 → DIK_5
		table[0x36] = 0x07; // VK_6 → DIK_6
		table[0x37] = 0x08; // VK_7 → DIK_7
		table[0x38] = 0x09; // VK_8 → DIK_8
		table[0x39] = 0x0A; // VK_9 → DIK_9

		// Letters A-Z
		table[0x41] = 0x1E; // VK_A → DIK_A
		table[0x42] = 0x30; // VK_B → DIK_B
		table[0x43] = 0x2E; // VK_C → DIK_C
		table[0x44] = 0x20; // VK_D → DIK_D
		table[0x45] = 0x12; // VK_E → DIK_E
		table[0x46] = 0x21; // VK_F → DIK_F
		table[0x47] = 0x22; // VK_G → DIK_G
		table[0x48] = 0x23; // VK_H → DIK_H
		table[0x49] = 0x17; // VK_I → DIK_I
		table[0x4A] = 0x24; // VK_J → DIK_J
		table[0x4B] = 0x25; // VK_K → DIK_K
		table[0x4C] = 0x26; // VK_L → DIK_L
		table[0x4D] = 0x32; // VK_M → DIK_M
		table[0x4E] = 0x31; // VK_N → DIK_N
		table[0x4F] = 0x18; // VK_O → DIK_O
		table[0x50] = 0x19; // VK_P → DIK_P
		table[0x51] = 0x10; // VK_Q → DIK_Q
		table[0x52] = 0x13; // VK_R → DIK_R
		table[0x53] = 0x1F; // VK_S → DIK_S
		table[0x54] = 0x14; // VK_T → DIK_T
		table[0x55] = 0x16; // VK_U → DIK_U
		table[0x56] = 0x2F; // VK_V → DIK_V
		table[0x57] = 0x11; // VK_W → DIK_W
		table[0x58] = 0x2D; // VK_X → DIK_X
		table[0x59] = 0x15; // VK_Y → DIK_Y
		table[0x5A] = 0x2C; // VK_Z → DIK_Z

		// Numpad 0-9
		table[0x60] = 0x52; // VK_NUMPAD0  → DIK_NUMPAD0
		table[0x61] = 0x4F; // VK_NUMPAD1  → DIK_NUMPAD1
		table[0x62] = 0x50; // VK_NUMPAD2  → DIK_NUMPAD2
		table[0x63] = 0x51; // VK_NUMPAD3  → DIK_NUMPAD3
		table[0x64] = 0x4B; // VK_NUMPAD4  → DIK_NUMPAD4
		table[0x65] = 0x4C; // VK_NUMPAD5  → DIK_NUMPAD5
		table[0x66] = 0x4D; // VK_NUMPAD6  → DIK_NUMPAD6
		table[0x67] = 0x47; // VK_NUMPAD7  → DIK_NUMPAD7
		table[0x68] = 0x48; // VK_NUMPAD8  → DIK_NUMPAD8
		table[0x69] = 0x49; // VK_NUMPAD9  → DIK_NUMPAD9

		// Numpad operators
		table[0x6A] = 0x37; // VK_MULTIPLY → DIK_MULTIPLY
		table[0x6B] = 0x4E; // VK_ADD      → DIK_ADD
		table[0x6D] = 0x4A; // VK_SUBTRACT → DIK_SUBTRACT
		table[0x6E] = 0x53; // VK_DECIMAL  → DIK_DECIMAL
		table[0x6F] = 0xB5; // VK_DIVIDE   → DIK_DIVIDE (extended)

		// Function keys F1-F12
		table[0x70] = 0x3B; // VK_F1  → DIK_F1
		table[0x71] = 0x3C; // VK_F2  → DIK_F2
		table[0x72] = 0x3D; // VK_F3  → DIK_F3
		table[0x73] = 0x3E; // VK_F4  → DIK_F4
		table[0x74] = 0x3F; // VK_F5  → DIK_F5
		table[0x75] = 0x40; // VK_F6  → DIK_F6
		table[0x76] = 0x41; // VK_F7  → DIK_F7
		table[0x77] = 0x42; // VK_F8  → DIK_F8
		table[0x78] = 0x43; // VK_F9  → DIK_F9
		table[0x79] = 0x44; // VK_F10 → DIK_F10
		table[0x7A] = 0x57; // VK_F11 → DIK_F11
		table[0x7B] = 0x58; // VK_F12 → DIK_F12

		// Lock keys
		table[0x90] = 0x45; // VK_NUMLOCK → DIK_NUMLOCK
		table[0x91] = 0x46; // VK_SCROLL  → DIK_SCROLL

		// Modifier keys (left and right)
		table[0xA0] = 0x2A; // VK_LSHIFT    → DIK_LSHIFT
		table[0xA1] = 0x36; // VK_RSHIFT    → DIK_RSHIFT
		table[0xA2] = 0x1D; // VK_LCONTROL  → DIK_LCONTROL
		table[0xA3] = 0x9D; // VK_RCONTROL  → DIK_RCONTROL (extended)
		table[0xA4] = 0x38; // VK_LMENU     → DIK_LMENU
		table[0xA5] = 0xB8; // VK_RMENU     → DIK_RMENU (extended)

		// Generic modifier fallbacks (some apps / WASM may send generic VK_SHIFT etc.)
		table[0x10] = 0x2A; // VK_SHIFT   → DIK_LSHIFT
		table[0x11] = 0x1D; // VK_CONTROL → DIK_LCONTROL
		table[0x12] = 0x38; // VK_MENU    → DIK_LMENU

		// OEM / punctuation keys
		table[0xBA] = 0x27; // VK_OEM_1         (;:)  → DIK_SEMICOLON
		table[0xBB] = 0x0D; // VK_OEM_PLUS      (=+)  → DIK_EQUALS
		table[0xBC] = 0x33; // VK_OEM_COMMA     (,<)  → DIK_COMMA
		table[0xBD] = 0x0C; // VK_OEM_MINUS     (-_)  → DIK_MINUS
		table[0xBE] = 0x34; // VK_OEM_PERIOD    (.>)  → DIK_PERIOD
		table[0xBF] = 0x35; // VK_OEM_2         (/?)  → DIK_SLASH
		table[0xC0] = 0x29; // VK_OEM_3         (`~)  → DIK_GRAVE
		table[0xDB] = 0x1A; // VK_OEM_4         ([{)  → DIK_LBRACKET
		table[0xDC] = 0x2B; // VK_OEM_5         (\|)  → DIK_BACKSLASH
		table[0xDD] = 0x1B; // VK_OEM_6         (]})  → DIK_RBRACKET
		table[0xDE] = 0x28; // VK_OEM_7         ('")  → DIK_APOSTROPHE

		return table;
	}

	private static int[] BuildSdlToVkTable()
	{
		// SDL3 uses USB HID scan codes (same values as SDL2):
		// https://github.com/libsdl-org/SDL/blob/main/include/SDL3/SDL_scancode.h
		const int TableSize = 512;
		var table = new int[TableSize];

		// Letters A-Z (SDL: 4-29)
		table[4]  = 0x41; // SDL_SCANCODE_A → VK_A
		table[5]  = 0x42; // SDL_SCANCODE_B → VK_B
		table[6]  = 0x43; // SDL_SCANCODE_C → VK_C
		table[7]  = 0x44; // SDL_SCANCODE_D → VK_D
		table[8]  = 0x45; // SDL_SCANCODE_E → VK_E
		table[9]  = 0x46; // SDL_SCANCODE_F → VK_F
		table[10] = 0x47; // SDL_SCANCODE_G → VK_G
		table[11] = 0x48; // SDL_SCANCODE_H → VK_H
		table[12] = 0x49; // SDL_SCANCODE_I → VK_I
		table[13] = 0x4A; // SDL_SCANCODE_J → VK_J
		table[14] = 0x4B; // SDL_SCANCODE_K → VK_K
		table[15] = 0x4C; // SDL_SCANCODE_L → VK_L
		table[16] = 0x4D; // SDL_SCANCODE_M → VK_M
		table[17] = 0x4E; // SDL_SCANCODE_N → VK_N
		table[18] = 0x4F; // SDL_SCANCODE_O → VK_O
		table[19] = 0x50; // SDL_SCANCODE_P → VK_P
		table[20] = 0x51; // SDL_SCANCODE_Q → VK_Q
		table[21] = 0x52; // SDL_SCANCODE_R → VK_R
		table[22] = 0x53; // SDL_SCANCODE_S → VK_S
		table[23] = 0x54; // SDL_SCANCODE_T → VK_T
		table[24] = 0x55; // SDL_SCANCODE_U → VK_U
		table[25] = 0x56; // SDL_SCANCODE_V → VK_V
		table[26] = 0x57; // SDL_SCANCODE_W → VK_W
		table[27] = 0x58; // SDL_SCANCODE_X → VK_X
		table[28] = 0x59; // SDL_SCANCODE_Y → VK_Y
		table[29] = 0x5A; // SDL_SCANCODE_Z → VK_Z

		// Digits 1-9, then 0 (SDL: 30-39)
		table[30] = 0x31; // SDL_SCANCODE_1 → VK_1
		table[31] = 0x32; // SDL_SCANCODE_2 → VK_2
		table[32] = 0x33; // SDL_SCANCODE_3 → VK_3
		table[33] = 0x34; // SDL_SCANCODE_4 → VK_4
		table[34] = 0x35; // SDL_SCANCODE_5 → VK_5
		table[35] = 0x36; // SDL_SCANCODE_6 → VK_6
		table[36] = 0x37; // SDL_SCANCODE_7 → VK_7
		table[37] = 0x38; // SDL_SCANCODE_8 → VK_8
		table[38] = 0x39; // SDL_SCANCODE_9 → VK_9
		table[39] = 0x30; // SDL_SCANCODE_0 → VK_0

		// Enter, Escape, Backspace, Tab, Space (SDL: 40-44)
		table[40] = 0x0D; // SDL_SCANCODE_RETURN    → VK_RETURN
		table[41] = 0x1B; // SDL_SCANCODE_ESCAPE    → VK_ESCAPE
		table[42] = 0x08; // SDL_SCANCODE_BACKSPACE  → VK_BACK
		table[43] = 0x09; // SDL_SCANCODE_TAB       → VK_TAB
		table[44] = 0x20; // SDL_SCANCODE_SPACE     → VK_SPACE

		// Punctuation (SDL: 45-56)
		table[45] = 0xBD; // SDL_SCANCODE_MINUS         (-_)  → VK_OEM_MINUS
		table[46] = 0xBB; // SDL_SCANCODE_EQUALS        (=+)  → VK_OEM_PLUS
		table[47] = 0xDB; // SDL_SCANCODE_LEFTBRACKET   ([{)  → VK_OEM_4
		table[48] = 0xDD; // SDL_SCANCODE_RIGHTBRACKET  (]})  → VK_OEM_6
		table[49] = 0xDC; // SDL_SCANCODE_BACKSLASH     (\|)  → VK_OEM_5
		// 50 = SDL_SCANCODE_NONUSHASH (hash / # for non-US keyboards) - no standard VK mapping
		table[51] = 0xBA; // SDL_SCANCODE_SEMICOLON     (;:)  → VK_OEM_1
		table[52] = 0xDE; // SDL_SCANCODE_APOSTROPHE    ('")  → VK_OEM_7
		table[53] = 0xC0; // SDL_SCANCODE_GRAVE         (`~)  → VK_OEM_3
		table[54] = 0xBC; // SDL_SCANCODE_COMMA         (,<)  → VK_OEM_COMMA
		table[55] = 0xBE; // SDL_SCANCODE_PERIOD        (.>)  → VK_OEM_PERIOD
		table[56] = 0xBF; // SDL_SCANCODE_SLASH         (/?)  → VK_OEM_2

		// CapsLock (SDL: 57)
		table[57] = 0x14; // SDL_SCANCODE_CAPSLOCK → VK_CAPITAL

		// Function keys F1-F12 (SDL: 58-69)
		table[58] = 0x70; // SDL_SCANCODE_F1  → VK_F1
		table[59] = 0x71; // SDL_SCANCODE_F2  → VK_F2
		table[60] = 0x72; // SDL_SCANCODE_F3  → VK_F3
		table[61] = 0x73; // SDL_SCANCODE_F4  → VK_F4
		table[62] = 0x74; // SDL_SCANCODE_F5  → VK_F5
		table[63] = 0x75; // SDL_SCANCODE_F6  → VK_F6
		table[64] = 0x76; // SDL_SCANCODE_F7  → VK_F7
		table[65] = 0x77; // SDL_SCANCODE_F8  → VK_F8
		table[66] = 0x78; // SDL_SCANCODE_F9  → VK_F9
		table[67] = 0x79; // SDL_SCANCODE_F10 → VK_F10
		table[68] = 0x7A; // SDL_SCANCODE_F11 → VK_F11
		table[69] = 0x7B; // SDL_SCANCODE_F12 → VK_F12

		// System keys (SDL: 70-72)
		table[70] = 0x2C; // SDL_SCANCODE_PRINTSCREEN → VK_SNAPSHOT
		table[71] = 0x91; // SDL_SCANCODE_SCROLLLOCK  → VK_SCROLL
		table[72] = 0x13; // SDL_SCANCODE_PAUSE       → VK_PAUSE

		// Editing / navigation cluster (SDL: 73-82)
		table[73] = 0x2D; // SDL_SCANCODE_INSERT   → VK_INSERT
		table[74] = 0x24; // SDL_SCANCODE_HOME     → VK_HOME
		table[75] = 0x21; // SDL_SCANCODE_PAGEUP   → VK_PRIOR
		table[76] = 0x2E; // SDL_SCANCODE_DELETE   → VK_DELETE
		table[77] = 0x23; // SDL_SCANCODE_END      → VK_END
		table[78] = 0x22; // SDL_SCANCODE_PAGEDOWN → VK_NEXT
		table[79] = 0x27; // SDL_SCANCODE_RIGHT    → VK_RIGHT
		table[80] = 0x25; // SDL_SCANCODE_LEFT     → VK_LEFT
		table[81] = 0x28; // SDL_SCANCODE_DOWN     → VK_DOWN
		table[82] = 0x26; // SDL_SCANCODE_UP       → VK_UP

		// Numpad (SDL: 83-99)
		table[83] = 0x90; // SDL_SCANCODE_NUMLOCKCLEAR → VK_NUMLOCK
		table[84] = 0x6F; // SDL_SCANCODE_KP_DIVIDE   → VK_DIVIDE
		table[85] = 0x6A; // SDL_SCANCODE_KP_MULTIPLY → VK_MULTIPLY
		table[86] = 0x6D; // SDL_SCANCODE_KP_MINUS    → VK_SUBTRACT
		table[87] = 0x6B; // SDL_SCANCODE_KP_PLUS     → VK_ADD
		table[88] = 0x0D; // SDL_SCANCODE_KP_ENTER    → VK_RETURN
		table[89] = 0x61; // SDL_SCANCODE_KP_1        → VK_NUMPAD1
		table[90] = 0x62; // SDL_SCANCODE_KP_2        → VK_NUMPAD2
		table[91] = 0x63; // SDL_SCANCODE_KP_3        → VK_NUMPAD3
		table[92] = 0x64; // SDL_SCANCODE_KP_4        → VK_NUMPAD4
		table[93] = 0x65; // SDL_SCANCODE_KP_5        → VK_NUMPAD5
		table[94] = 0x66; // SDL_SCANCODE_KP_6        → VK_NUMPAD6
		table[95] = 0x67; // SDL_SCANCODE_KP_7        → VK_NUMPAD7
		table[96] = 0x68; // SDL_SCANCODE_KP_8        → VK_NUMPAD8
		table[97] = 0x69; // SDL_SCANCODE_KP_9        → VK_NUMPAD9
		table[98] = 0x60; // SDL_SCANCODE_KP_0        → VK_NUMPAD0
		table[99] = 0x6E; // SDL_SCANCODE_KP_PERIOD   → VK_DECIMAL

		// Modifier keys (SDL: 224-231)
		table[224] = 0xA2; // SDL_SCANCODE_LCTRL  → VK_LCONTROL
		table[225] = 0xA0; // SDL_SCANCODE_LSHIFT → VK_LSHIFT
		table[226] = 0xA4; // SDL_SCANCODE_LALT   → VK_LMENU
		table[227] = 0x5B; // SDL_SCANCODE_LGUI   → VK_LWIN
		table[228] = 0xA3; // SDL_SCANCODE_RCTRL  → VK_RCONTROL
		table[229] = 0xA1; // SDL_SCANCODE_RSHIFT → VK_RSHIFT
		table[230] = 0xA5; // SDL_SCANCODE_RALT   → VK_RMENU
		table[231] = 0x5C; // SDL_SCANCODE_RGUI   → VK_RWIN

		return table;
	}
}
