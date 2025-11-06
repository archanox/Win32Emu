namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_KEYDOWN message
	/// </summary>
	public record KeyDownMessage(uint Hwnd, uint WParam, uint LParam) 
		: Win32Message(Hwnd, (uint)WM.KEYDOWN, WParam, LParam)
	{
		/// <summary>
		/// Virtual key code
		/// </summary>
		public uint VirtualKeyCode => WParam;
	
		/// <summary>
		/// Repeat count (bits 0-15 of lParam)
		/// </summary>
		public uint RepeatCount => LParam & 0xFFFF;
	
		/// <summary>
		/// Scan code (bits 16-23 of lParam)
		/// </summary>
		public uint ScanCode => (LParam >> 16) & 0xFF;
	}
}