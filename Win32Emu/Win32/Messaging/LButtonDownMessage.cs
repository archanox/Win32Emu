namespace Win32Emu.Win32.Messaging
{
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
}