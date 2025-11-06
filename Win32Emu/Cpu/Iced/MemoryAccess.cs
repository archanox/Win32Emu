namespace Win32Emu.Cpu.Iced
{
	/// <summary>
	/// Represents a memory access by an instruction.
	/// </summary>
	public class MemoryAccess
	{
		public string Segment { get; set; } = "";
		public string Base { get; set; } = "";
		public string Index { get; set; } = "";
		public int Scale { get; set; }
		public ulong Displacement { get; set; }
		public string Access { get; set; } = "";
	}
}