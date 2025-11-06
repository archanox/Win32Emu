namespace Win32Emu.Threading
{
	/// <summary>
	/// Represents a Win32 semaphore synchronization object
	/// </summary>
	public class EmulatedSemaphore
	{
		public uint Handle { get; }
		public string? Name { get; }
		public uint CurrentCount { get; set; }
		public uint MaximumCount { get; }
		public Queue<uint> WaitingThreads { get; } = new();

		public EmulatedSemaphore(uint handle, string? name, uint initialCount, uint maximumCount)
		{
			Handle = handle;
			Name = name;
			CurrentCount = initialCount;
			MaximumCount = maximumCount;
		}

		public bool IsSignaled => CurrentCount > 0;
	}
}