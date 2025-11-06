namespace Win32Emu.Win32.Messaging
{
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
}