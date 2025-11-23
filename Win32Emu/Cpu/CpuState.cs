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

		// Control Registers (CR0-CR4)
		public uint Cr0 { get; set; }
		public uint Cr2 { get; set; }
		public uint Cr3 { get; set; }
		public uint Cr4 { get; set; }

		// Debug Registers (DR0-DR7)
		public uint Dr0 { get; set; }
		public uint Dr1 { get; set; }
		public uint Dr2 { get; set; }
		public uint Dr3 { get; set; }
		public uint Dr6 { get; set; }
		public uint Dr7 { get; set; }
	}
}