namespace Win32Emu.Debugging
{
	/// <summary>
	/// Represents a watchpoint in the debugger
	/// </summary>
	public class Watchpoint
	{
		public uint Id { get; init; }
		public uint Address { get; init; }
		public WatchpointType Type { get; init; }
		public uint Length { get; init; }
		public string Description { get; init; } = "";
		public bool Enabled { get; set; }
		public int HitCount { get; set; }
	}
}