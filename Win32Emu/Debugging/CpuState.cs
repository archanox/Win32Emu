namespace Win32Emu.Debugging
{
	/// <summary>
	/// Snapshot of CPU state at a point in time
	/// </summary>
	public record CpuState
	{
		public uint Eip { get; init; }
		public uint Eax { get; init; }
		public uint Ebx { get; init; }
		public uint Ecx { get; init; }
		public uint Edx { get; init; }
		public uint Esi { get; init; }
		public uint Edi { get; init; }
		public uint Ebp { get; init; }
		public uint Esp { get; init; }
		public uint Eflags { get; init; }
		public string InstructionBytes { get; init; } = "";
	}
}