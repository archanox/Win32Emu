namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_ACTIVATE message
	/// </summary>
	public record ActivateMessage(uint Hwnd, uint WParam, uint LParam) 
		: Win32Message(Hwnd, (uint)WM.ACTIVATE, WParam, LParam)
	{
		/// <summary>
		/// Active flag (LOWORD of wParam)
		/// </summary>
		public uint ActiveFlag => WParam & 0xFFFF;
	
		/// <summary>
		/// Minimized flag (HIWORD of wParam)
		/// </summary>
		public bool IsMinimized => ((WParam >> 16) & 0xFFFF) != 0;
	
		/// <summary>
		/// Handle of window being activated/deactivated
		/// </summary>
		public uint OtherWindow => LParam;
	}
}