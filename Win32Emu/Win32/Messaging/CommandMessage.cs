namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_COMMAND message
	/// </summary>
	public record CommandMessage(uint Hwnd, uint WParam, uint LParam) 
		: Win32Message(Hwnd, (uint)WM.COMMAND, WParam, LParam)
	{
		/// <summary>
		/// Control ID (LOWORD of wParam)
		/// </summary>
		public uint ControlId => WParam & 0xFFFF;
	
		/// <summary>
		/// Notification code (HIWORD of wParam)
		/// </summary>
		public uint NotificationCode => (WParam >> 16) & 0xFFFF;
	
		/// <summary>
		/// Control window handle (lParam)
		/// </summary>
		public uint ControlHandle => LParam;
	}
}