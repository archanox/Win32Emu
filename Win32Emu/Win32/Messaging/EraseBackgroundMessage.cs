namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_ERASEBKGND message
	/// </summary>
	public record EraseBackgroundMessage(uint Hwnd, uint WParam) 
		: Win32Message(Hwnd, (uint)WM.ERASEBKGND, WParam, 0)
	{
		/// <summary>
		/// Device context handle
		/// </summary>
		public uint HDC => WParam;
	}
}