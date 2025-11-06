namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_CHAR message
	/// </summary>
	public record CharMessage(uint Hwnd, uint WParam, uint LParam) 
		: Win32Message(Hwnd, (uint)WM.CHAR, WParam, LParam)
	{
		/// <summary>
		/// Character code
		/// </summary>
		public uint CharCode => WParam;
	
		/// <summary>
		/// Repeat count
		/// </summary>
		public uint RepeatCount => LParam & 0xFFFF;
	}
}