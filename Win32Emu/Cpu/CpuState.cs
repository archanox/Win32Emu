namespace Win32Emu.Cpu
{
	/// <summary>
	/// Represents a complete snapshot of CPU state for async suspension/resumption
	/// </summary>
	public class CpuState
	{
		public uint Eax { get; set; }
		public uint Ebx { get; set; }
		public uint Ecx { get; set; }
		public uint Edx { get; set; }
		public uint Esi { get; set; }
		public uint Edi { get; set; }
		public uint Ebp { get; set; }
		public uint Esp { get; set; }
		public uint Eip { get; set; }
		public uint Eflags { get; set; }
	
		// FPU state (optional, can be extended as needed)
		public double[]? FpuStack { get; set; }
		public int FpuTop { get; set; }
		public ushort FpuControlWord { get; set; }
		public ushort FpuStatusWord { get; set; }
		public ushort FpuTagWord { get; set; }
	}
}