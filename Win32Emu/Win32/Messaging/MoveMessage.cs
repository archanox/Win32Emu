namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_MOVE message
	/// </summary>
	public record MoveMessage(uint Hwnd, uint LParam) 
		: Win32Message(Hwnd, (uint)WM.MOVE, 0, LParam)
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