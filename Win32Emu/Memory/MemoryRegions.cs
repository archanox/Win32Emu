namespace Win32Emu.Memory;

/// <summary>
/// Centralized definition of special memory address ranges used by the emulator infrastructure.
/// These ranges are reserved for emulator-specific purposes like COM vtables, syscall dispatchers, and import hooks.
/// </summary>
/// <remarks>
/// Memory layout:
/// - 0x0D000000 - 0x0DFFFFFF: COM vtables and standard control window procedures (16 MB)
/// - 0x0E000000 - 0x0EFFFFFF: Syscall dispatcher and synthetic exports (16 MB)
/// - 0x0F000000 - 0x0FFFFFFF: Import hooks and stubs (16 MB)
/// - 0x10000000+: Other uses (thread stacks, module handles, etc.)
/// </remarks>
public static class MemoryRegions
{
	// COM vtables and standard control window procedures range (0x0D000000 - 0x0DFFFFFF)
	// This range is used for:
	// - COM interface vtable method stubs that trigger emulation
	// - Standard control window procedure markers (e.g., BUTTON, EDIT, LISTBOX)
	public const uint ComVtableBase = 0x0D000000;
	public const uint ComVtableLimit = 0x0E000000;
	public const uint ComVtableSize = ComVtableLimit - ComVtableBase; // 16 MB
	
	// Syscall dispatcher and synthetic exports range (0x0E000000 - 0x0EFFFFFF)
	// This range is used for:
	// - The main syscall dispatcher entry point (at 0x0E000000)
	// - Synthetic exports created via GetProcAddress (starting at 0x0E000000+)
	public const uint SyscallDispatcherBase = 0x0E000000;
	public const uint SyscallDispatcherLimit = 0x0F000000;
	public const uint SyscallDispatcherSize = SyscallDispatcherLimit - SyscallDispatcherBase; // 16 MB
	
	// The exact address of the syscall dispatcher entry point
	public const uint SyscallDispatcherAddress = 0x0E000000;
	
	// Import hooks and stubs range (0x0F000000 - 0x0FFFFFFF)
	// This range is used for:
	// - Static import table hook stubs (starting at 0x0F000000)
	// - Dynamic synthetic export stubs (starting at 0x0F800000)
	public const uint ImportHookBase = 0x0F000000;
	public const uint ImportHookLimit = 0x10000000;
	public const uint ImportHookSize = ImportHookLimit - ImportHookBase; // 16 MB
	
	// Synthetic export stubs start at 0x0F800000 (midpoint of import hook range)
	// to avoid conflicts with static import stubs at 0x0F000000
	public const uint SyntheticExportBase = 0x0F800000;
	
	// Overall special range limit (end of all emulator infrastructure ranges)
	public const uint SpecialRangeLimit = 0x10000000;
	
	// Import stub alignment (16 bytes per stub)
	public const uint ImportStubAlignmentMask = 0xFFFFFFF0u;
	public const uint ImportStubSize = 0x10;
	
	/// <summary>
	/// Checks if an address is within any special emulator infrastructure range.
	/// </summary>
	/// <param name="address">The address to check</param>
	/// <returns>True if the address is in a special range, false otherwise</returns>
	public static bool IsInSpecialRange(uint address)
	{
		return address >= ComVtableBase && address < SpecialRangeLimit;
	}
	
	/// <summary>
	/// Checks if an address is within the COM vtable range.
	/// </summary>
	/// <param name="address">The address to check</param>
	/// <returns>True if the address is in the COM vtable range, false otherwise</returns>
	public static bool IsInComVtableRange(uint address)
	{
		return address >= ComVtableBase && address < ComVtableLimit;
	}
	
	/// <summary>
	/// Checks if an address is within the syscall dispatcher range.
	/// </summary>
	/// <param name="address">The address to check</param>
	/// <returns>True if the address is in the syscall dispatcher range, false otherwise</returns>
	public static bool IsInSyscallRange(uint address)
	{
		return address >= SyscallDispatcherBase && address < SyscallDispatcherLimit;
	}
	
	/// <summary>
	/// Checks if an address is within the import hook range.
	/// </summary>
	/// <param name="address">The address to check</param>
	/// <returns>True if the address is in the import hook range, false otherwise</returns>
	public static bool IsInImportHookRange(uint address)
	{
		return address >= ImportHookBase && address < ImportHookLimit;
	}
}
