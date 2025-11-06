namespace Win32Emu.Threading
{
	/// <summary>
	/// Represents a Win32 event synchronization object
	/// </summary>
	public class EmulatedEvent
	{
		public uint Handle { get; }
		public string? Name { get; }
		public bool ManualReset { get; }
		public bool Signaled { get; set; }
		public Queue<uint> WaitingThreads { get; } = new();

		public EmulatedEvent(uint handle, string? name, bool manualReset, bool initialState)
		{
			Handle = handle;
			Name = name;
			ManualReset = manualReset;
			Signaled = initialState;
		}
	}
}