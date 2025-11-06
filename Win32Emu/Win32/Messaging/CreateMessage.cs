namespace Win32Emu.Win32.Messaging;

/// <summary>
/// WM_CREATE message
/// </summary>
public record CreateMessage(uint Hwnd, uint WParam, uint LParam) 
	: Win32Message(Hwnd, (uint)WM.CREATE, WParam, LParam);