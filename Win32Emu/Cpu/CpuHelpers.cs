namespace Win32Emu.Cpu;

/// <summary>
/// Holds saved callee-saved register values (EBX, ESI, EDI) per x86 calling convention
/// </summary>
public readonly struct SavedCalleeSavedRegisters
{
	public uint Ebx { get; init; }
	public uint Esi { get; init; }
	public uint Edi { get; init; }
}

/// <summary>
/// Helper methods for CPU register management
/// </summary>
public static class CpuHelpers
{
	/// <summary>
	/// Save callee-saved registers (EBX, ESI, EDI) per x86 calling convention.
	/// Note: We do NOT save EBP here because some calling code uses EBP to hold the function
	/// pointer for indirect calls (e.g., MOV EBP, [IAT_Entry]; CALL EBP). If we preserve
	/// the EBP value at the time of the call, we'll restore the function pointer value
	/// instead of the original frame pointer, causing crashes.
	/// </summary>
	public static SavedCalleeSavedRegisters SaveCalleeSavedRegisters(ICpu cpu)
	{
		return new SavedCalleeSavedRegisters
		{
			Ebx = cpu.GetRegister("EBX"),
			Esi = cpu.GetRegister("ESI"),
			Edi = cpu.GetRegister("EDI")
		};
	}

	/// <summary>
	/// Restore callee-saved registers (EBX, ESI, EDI) that were previously saved
	/// </summary>
	public static void RestoreCalleeSavedRegisters(ICpu cpu, SavedCalleeSavedRegisters saved)
	{
		cpu.SetRegister("EBX", saved.Ebx);
		cpu.SetRegister("ESI", saved.Esi);
		cpu.SetRegister("EDI", saved.Edi);
	}
}
