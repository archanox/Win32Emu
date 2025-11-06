namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_PAINT message
	/// </summary>
	public record PaintMessage(uint Hwnd) 
		: Win32Message(Hwnd, (uint)WM.PAINT, 0, 0);
}