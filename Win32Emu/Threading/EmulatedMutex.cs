namespace Win32Emu.Threading
{
	/// <summary>
	/// Represents a Win32 mutex synchronization object
	/// </summary>
	public class EmulatedMutex
	{
		public uint Handle { get; }
		public string? Name { get; }
		public uint OwningThreadId { get; set; }
		public int RecursionCount { get; set; }
		public Queue<uint> WaitingThreads { get; } = new();

		public EmulatedMutex(uint handle, string? name)
		{
			Handle = handle;
			Name = name;
			OwningThreadId = 0;
			RecursionCount = 0;
		}

		public bool IsOwned => OwningThreadId != 0;
	}
}