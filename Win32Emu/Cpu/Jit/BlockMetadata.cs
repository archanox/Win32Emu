namespace Win32Emu.Cpu.Jit
{
	/// <summary>
	/// Metadata about a compiled x86 code block
	/// </summary>
	public class BlockMetadata
	{
		/// <summary>
		/// Starting address (EIP) of the block
		/// </summary>
		public uint StartAddress { get; set; }
	
		/// <summary>
		/// Number of instructions in the block
		/// </summary>
		public int InstructionCount { get; set; }
	
		/// <summary>
		/// Length of the block in bytes
		/// </summary>
		public int ByteLength { get; set; }
	
		/// <summary>
		/// SHA256 hash of the x86 code bytes
		/// </summary>
		public string CodeHash { get; set; } = string.Empty;
	
		/// <summary>
		/// Timestamp when this block was first compiled
		/// </summary>
		public DateTime FirstCompiled { get; set; }
	
		/// <summary>
		/// Number of times this block has been executed
		/// </summary>
		public long ExecutionCount { get; set; }
	
		/// <summary>
		/// Whether this block ends with a call instruction
		/// </summary>
		public bool EndsWithCall { get; set; }
	
		/// <summary>
		/// Whether this block ends with a return instruction
		/// </summary>
		public bool EndsWithReturn { get; set; }
	
		/// <summary>
		/// Target address if this block ends with a direct jump/call
		/// </summary>
		public uint? DirectTarget { get; set; }
	}
}