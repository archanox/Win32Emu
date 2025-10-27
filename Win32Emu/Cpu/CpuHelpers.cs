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
	// Constants for EBP validation (matching Emulator.cs)
	private const uint IMPORT_HOOK_BASE = 0x0F000000;
	private const uint IMPORT_HOOK_LIMIT = 0x10000000;
	private const uint MIN_VALID_EBP = 0x1000;

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
	/// Check if an EBP value is obviously invalid (0, import hook address, etc.)
	/// </summary>
	public static bool IsEbpValid(uint ebp, uint memorySize)
	{
		// Check for obviously invalid values
		if (ebp == 0) return false;
		if (ebp < MIN_VALID_EBP) return false;
		if (ebp >= IMPORT_HOOK_BASE && ebp < IMPORT_HOOK_LIMIT) return false;
		if (ebp >= memorySize) return false;
		
		return true;
	}

	/// <summary>
	/// Restore callee-saved registers (EBX, ESI, EDI, EBP) that were previously saved.
	/// Optionally skip restoring EBP if it was invalid when saved (prevents corruption cycle).
	/// </summary>
	public static void RestoreCalleeSavedRegisters(ICpu cpu, SavedCalleeSavedRegisters saved, bool skipInvalidEbp = false, uint memorySize = 0)
	{
		cpu.SetRegister("EBX", saved.Ebx);
		cpu.SetRegister("ESI", saved.Esi);
		cpu.SetRegister("EDI", saved.Edi);
		
		// If skipInvalidEbp is true, only restore EBP if it was valid when saved
		if (skipInvalidEbp && memorySize > 0)
		{
			if (IsEbpValid(saved.Ebp, memorySize))
			{
				cpu.SetRegister("EBP", saved.Ebp);
			}
			// Otherwise, leave EBP as-is (likely corrected by ValidateAndFixEbp)
		}
		else
		{
			cpu.SetRegister("EBP", saved.Ebp);
		}
	}
}
