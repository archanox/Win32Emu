namespace Win32Emu.Cpu.Iced
{
	/// <summary>
	/// Contains detailed analysis of an instruction.
	/// </summary>
	public class InstructionAnalysis
	{
		public string FormattedInstruction { get; set; } = "";
		public ulong Address { get; set; }
		public int Length { get; set; }
		public string Mnemonic { get; set; } = "";
		public string OpCodeString { get; set; } = "";
	
		public List<string> ReadRegisters { get; set; } = [];
		public List<string> WrittenRegisters { get; set; } = [];
		public List<MemoryAccess> MemoryAccesses { get; set; } = [];
	}
}