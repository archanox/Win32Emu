namespace Win32Emu.Debugging
{
	/// <summary>
	/// Type of watchpoint
	/// </summary>
	public enum WatchpointType
	{
		Write,    // Break on write
		Read,     // Break on read
		Access    // Break on read or write
	}
}