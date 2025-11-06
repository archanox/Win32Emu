namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// WM_QUIT message
	/// </summary>
	public record QuitMessage(uint ExitCode) 
		: Win32Message(0, (uint)WM.QUIT, ExitCode, 0)
	{
		/// <summary>
		/// Exit code for the application
		/// </summary>
		public uint ExitCode => WParam;
	}
}