namespace Win32Emu.Threading
{
	/// <summary>
	/// Represents the state of an emulated thread
	/// </summary>
	public enum ThreadState
	{
		/// <summary>Thread is ready to run or currently running</summary>
		Running,
		/// <summary>Thread is suspended (not scheduled)</summary>
		Suspended,
		/// <summary>Thread is waiting on a synchronization object</summary>
		Waiting,
		/// <summary>Thread has terminated</summary>
		Terminated
	}
}