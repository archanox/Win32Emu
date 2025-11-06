namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_SIZE message
	/// </summary>
	public record SizeMessage(uint Hwnd, uint WParam, uint LParam) 
		: Win32Message(Hwnd, (uint)WM.SIZE, WParam, LParam)
	{
		/// <summary>
		/// Width (LOWORD of lParam)
		/// </summary>
		public ushort Width => (ushort)(LParam & 0xFFFF);
	
		/// <summary>
		/// Height (HIWORD of lParam)
		/// </summary>
		public ushort Height => (ushort)((LParam >> 16) & 0xFFFF);
	
		/// <summary>
		/// Type of resizing (SIZE_RESTORED = 0, SIZE_MINIMIZED = 1, SIZE_MAXIMIZED = 2, etc.)
		/// </summary>
		public uint SizeType => WParam;
	}
}