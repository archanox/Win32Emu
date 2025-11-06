using Win32Emu.Cpu;

namespace Win32Emu.Threading
{
	/// <summary>
	/// Represents a saved CPU context for thread switching
	/// </summary>
	public class CpuContext
	{
		public uint EAX { get; set; }
		public uint EBX { get; set; }
		public uint ECX { get; set; }
		public uint EDX { get; set; }
		public uint ESI { get; set; }
		public uint EDI { get; set; }
		public uint EBP { get; set; }
		public uint ESP { get; set; }
		public uint EIP { get; set; }
		public uint EFLAGS { get; set; }

		/// <summary>
		/// Save CPU state from ICpu
		/// </summary>
		public void SaveFrom(ICpu cpu)
		{
			EAX = cpu.GetRegister("EAX");
			EBX = cpu.GetRegister("EBX");
			ECX = cpu.GetRegister("ECX");
			EDX = cpu.GetRegister("EDX");
			ESI = cpu.GetRegister("ESI");
			EDI = cpu.GetRegister("EDI");
			EBP = cpu.GetRegister("EBP");
			ESP = cpu.GetRegister("ESP");
			EIP = cpu.GetEip();
			EFLAGS = cpu.GetRegister("EFLAGS");
		}

		/// <summary>
		/// Restore CPU state to ICpu
		/// </summary>
		public void RestoreTo(ICpu cpu)
		{
			cpu.SetRegister("EAX", EAX);
			cpu.SetRegister("EBX", EBX);
			cpu.SetRegister("ECX", ECX);
			cpu.SetRegister("EDX", EDX);
			cpu.SetRegister("ESI", ESI);
			cpu.SetRegister("EDI", EDI);
			cpu.SetRegister("EBP", EBP);
			cpu.SetRegister("ESP", ESP);
			cpu.SetEip(EIP);
			cpu.SetRegister("EFLAGS", EFLAGS);
		}

		/// <summary>
		/// Create a copy of this context
		/// </summary>
		public CpuContext Clone()
		{
			return new CpuContext
			{
				EAX = EAX,
				EBX = EBX,
				ECX = ECX,
				EDX = EDX,
				ESI = ESI,
				EDI = EDI,
				EBP = EBP,
				ESP = ESP,
				EIP = EIP,
				EFLAGS = EFLAGS
			};
		}
	}
}