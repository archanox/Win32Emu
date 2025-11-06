namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_CLOSE message
	/// </summary>
	public record CloseMessage(uint Hwnd) 
		: Win32Message(Hwnd, (uint)WM.CLOSE, 0, 0);
}