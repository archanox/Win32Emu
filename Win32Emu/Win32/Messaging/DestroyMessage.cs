namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_DESTROY message
	/// </summary>
	public record DestroyMessage(uint Hwnd) 
		: Win32Message(Hwnd, (uint)WM.DESTROY, 0, 0);
}