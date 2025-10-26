namespace Win32Emu.Cpu;

/// <summary>
/// Holds saved callee-saved register values (EBX, ESI, EDI, EBP) per x86 calling convention
/// </summary>
public readonly struct SavedCalleeSavedRegisters
{
	public uint Ebx { get; init; }
	public uint Esi { get; init; }
	public uint Edi { get; init; }
	public uint Ebp { get; init; }
}

/// <summary>
/// Helper methods for CPU register management
/// </summary>
public static class CpuHelpers
{
	/// <summary>
	/// Save callee-saved registers (EBX, ESI, EDI, EBP) per x86 calling convention.
	/// Per x86 stdcall/cdecl conventions, these registers must be preserved by the callee.
	/// Even if EBP contains a function pointer or other value at call time, we preserve it
	/// as the calling code is responsible for managing EBP according to its needs.
	/// </summary>
	public static SavedCalleeSavedRegisters SaveCalleeSavedRegisters(ICpu cpu)
	{
		return new SavedCalleeSavedRegisters
		{
			Ebx = cpu.GetRegister("EBX"),
			Esi = cpu.GetRegister("ESI"),
			Edi = cpu.GetRegister("EDI"),
			Ebp = cpu.GetRegister("EBP")
		};
	}

	/// <summary>
	/// Restore callee-saved registers (EBX, ESI, EDI, EBP) that were previously saved
	/// </summary>
	public static void RestoreCalleeSavedRegisters(ICpu cpu, SavedCalleeSavedRegisters saved)
	{
		cpu.SetRegister("EBX", saved.Ebx);
		cpu.SetRegister("ESI", saved.Esi);
		cpu.SetRegister("EDI", saved.Edi);
		cpu.SetRegister("EBP", saved.Ebp);
	}
}
