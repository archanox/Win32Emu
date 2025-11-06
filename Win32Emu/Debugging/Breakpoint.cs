namespace Win32Emu.Debugging
{
	/// <summary>
	/// Represents a breakpoint in the debugger
	/// </summary>
	public class Breakpoint
	{
		public uint Id { get; init; }
		public uint Address { get; init; }
		public string Description { get; init; } = "";
		public bool Enabled { get; set; }
		public int HitCount { get; set; }
	}
}