namespace Win32Emu.Win32.Messaging
{
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
}