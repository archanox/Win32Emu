namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_TIMER message
	/// </summary>
	public record TimerMessage(uint Hwnd, uint WParam, uint LParam) 
		: Win32Message(Hwnd, (uint)WM.TIMER, WParam, LParam)
	{
		/// <summary>
		/// Timer identifier
		/// </summary>
		public uint TimerId => WParam;
	
		/// <summary>
		/// Timer procedure address (optional)
		/// </summary>
		public uint TimerProc => LParam;
	}
}