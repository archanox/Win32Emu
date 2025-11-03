using Microsoft.Extensions.Logging;
using Win32Emu.Memory;

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
	private const uint HEAP_BASE = 0x01000000;
	private const uint HEAP_LIMIT = 0x70000000;

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
	public static bool IsEbpValid(uint ebp, ulong memorySize)
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
	public static void RestoreCalleeSavedRegisters(ICpu cpu, SavedCalleeSavedRegisters saved, bool skipInvalidEbp = false, ulong memorySize = 0)
	{
		if (skipInvalidEbp && memorySize == 0)
		{
			throw new ArgumentException("memorySize must be provided and nonzero when skipInvalidEbp is true.", nameof(memorySize));
		}
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

	/// <summary>
	/// Attempts to restore EBP from the stack after an emulated API call.
	/// This handles cases where the calling code used EBP to hold the function pointer for an indirect call.
	/// </summary>
	/// <param name="cpu">The CPU instance</param>
	/// <param name="memory">The virtual memory instance</param>
	/// <param name="esp">Current stack pointer value</param>
	/// <param name="logger">Logger for diagnostic messages (optional)</param>
	/// <param name="logPrefix">Prefix for log messages (default: "CpuHelpers")</param>
	public static void RestoreEbpFromStack(ICpu cpu, VirtualMemory memory, uint esp, ILogger? logger = null, string logPrefix = "CpuHelpers")
	{
		try
		{
			var ebpFromStack = memory.Read32(esp);
			var currentEbp = cpu.GetRegister("EBP");

			// Define plausible stack region (for example, 1MB stack)
			// Assume stack grows down, so stack base is the highest address, stack limit is lowest
			// Here, we use current ESP as the top of the stack, and allow up to 1MB below
			const uint STACK_SIZE = 0x100000; // 1MB
			var stackBottom = (esp > STACK_SIZE) ? (esp - STACK_SIZE) : 0x00100000; // Don't go below 1MB

			var inStackRegion = (ebpFromStack >= stackBottom) && (ebpFromStack <= esp);
			var isAligned = (ebpFromStack & 0x3) == 0;

			// Optionally, check that the memory at ebpFromStack is readable and contains a plausible saved EBP
			var savedEbpValid = false;
			if (inStackRegion && isAligned)
			{
				try
				{
					var savedEbp = memory.Read32(ebpFromStack);
					// Check that savedEbp is also within stack region (optional, but plausible)
					savedEbpValid = (savedEbp >= stackBottom) && (savedEbp <= esp);
				}
				catch (Exception ex)
				{
					logger?.LogTrace(ex, "[{LogPrefix}] Exception while probing saved EBP at 0x{EbpFromStack:X8}", logPrefix, ebpFromStack);
					savedEbpValid = false;
				}
			}

			// Check if current EBP looks like an import hook address first
			var isImportHook = (currentEbp >= IMPORT_HOOK_BASE && currentEbp < IMPORT_HOOK_LIMIT);
			
			// If EBP is an import hook address, we must restore it from the stack
			// This happens when code uses patterns like: MOV EBP, [IAT_Entry]; CALL EBP
			// After the call returns, EBP still contains the import hook address and needs restoration
			if (isImportHook)
			{
				if (inStackRegion && isAligned)
				{
					cpu.SetRegister("EBP", ebpFromStack);
					logger?.LogTrace("[{LogPrefix}] Forcibly restored EBP from stack (was import hook 0x{OldEBP:X8}): 0x{EBP:X8}", logPrefix, currentEbp, ebpFromStack);
				}
				else
				{
					// EBP contains an import hook address (0x0F000000-0x10000000) but can't be restored from stack
					// This occurs when calling code uses EBP for indirect calls (e.g., MOV EBP, [IAT_Entry]; CALL EBP)
					// and the stack contains invalid or unaligned data
					// Reset EBP to ESP as a safe fallback to prevent subsequent memory access errors
					// when the program tries to use EBP for stack frame access (e.g., MOV EAX, [EBP+offset])
					cpu.SetRegister("EBP", esp);
					logger?.LogTrace("[{LogPrefix}] Reset EBP to ESP (was import hook 0x{OldEBP:X8}, stack restoration failed)", logPrefix, currentEbp);
				}
			}
			else if (inStackRegion && isAligned && savedEbpValid)
			{
				cpu.SetRegister("EBP", ebpFromStack);
				logger?.LogTrace("[{LogPrefix}] Restored EBP from stack: 0x{EBP:X8}", logPrefix, ebpFromStack);
			}
			else
			{
				// If we can't restore EBP from stack, check if current EBP is valid
				// Allow 4KB of slack above ESP to account for minor stack pointer adjustments (e.g., function prologues/epilogues, local allocations)
				const uint StackSlackBytes = 0x1000; // 4KB slack above ESP for plausible stack frame pointers
				var currentEbpInStackRegion = (currentEbp >= stackBottom) && (currentEbp <= esp + StackSlackBytes);
				
				// Check if current EBP looks like a COM vtable or object pointer
				// COM objects are typically allocated in heap regions (0x01000000-0x70000000)
				var isLikelyComPointer = (currentEbp >= HEAP_BASE && currentEbp < HEAP_LIMIT) && !currentEbpInStackRegion;
				
				// Check if current EBP is properly aligned (should be 4-byte aligned on x86)
				// Unaligned EBP can cause address calculation overflow issues
				var isUnaligned = (currentEbp & 0x3) != 0;
				
				if (isLikelyComPointer || isUnaligned)
				{
					// EBP contains a non-frame-pointer or special-purpose value (COM pointer, or unaligned); leave unchanged to respect calling conventions
					// Don't modify EBP - the calling code will manage it
					// Setting EBP=ESP here would break the caller's frame pointer assumptions
					
					if (isLikelyComPointer)
					{
						logger?.LogTrace("[{LogPrefix}] Skipped EBP restoration: current EBP 0x{CurrentEBP:X8} is likely a COM/heap pointer, leaving unchanged", logPrefix, currentEbp);
					}
					else if (isUnaligned)
					{
						logger?.LogTrace("[{LogPrefix}] Skipped EBP restoration: current EBP 0x{CurrentEBP:X8} is unaligned, leaving unchanged", logPrefix, currentEbp);
					}
				}
				else if (!currentEbpInStackRegion)
				{
					// EBP is out of stack region but not obviously wrong (aligned, not a hook/pointer)
					// This might be a valid heap pointer or global variable address used intentionally
					// Don't modify it
					logger?.LogTrace("[{LogPrefix}] Skipped EBP restoration: current EBP 0x{CurrentEBP:X8} is out of stack region but looks intentional, leaving unchanged", logPrefix, currentEbp);
				}
				else
				{
					logger?.LogTrace("[{LogPrefix}] Skipped restoring EBP from stack: 0x{EBP:X8} (not a valid frame pointer), current EBP 0x{CurrentEBP:X8} looks valid", logPrefix, ebpFromStack, currentEbp);
				}
			}
		}
		catch (Exception ex)
		{
			// Do not catch critical exceptions that should not be handled
			if (ex is StackOverflowException || ex is OutOfMemoryException || ex is ThreadAbortException)
				throw;
			logger?.LogTrace(ex, "[{LogPrefix}] Failed to restore EBP from stack", logPrefix);
		}
	}
}
