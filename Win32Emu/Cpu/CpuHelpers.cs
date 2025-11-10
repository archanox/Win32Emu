using Microsoft.Extensions.Logging;
using Win32Emu.Memory;

namespace Win32Emu.Cpu;

/// <summary>
/// Helper methods for CPU register management and async execution
/// </summary>
public static class CpuHelpers
{
	/// <summary>
	/// Execute CPU instruction(s) asynchronously, using ExecuteBlockAsync for JIT-enabled CPUs
	/// or SingleStepAsync for interpreter CPUs. This provides optimal performance while maintaining
	/// compatibility across all CPU backends.
	/// </summary>
	/// <param name="cpu">The CPU instance</param>
	/// <param name="memory">Virtual memory instance</param>
	/// <returns>Result of the CPU step/block execution</returns>
	public static async Task<CpuStepResult> ExecuteAsync(ICpu cpu, VirtualMemory memory)
	{
		if (cpu is IAsyncCpu asyncCpu)
		{
			// For JIT-enabled CPUs, use ExecuteBlockAsync for better performance
			if (asyncCpu.SupportsJit)
			{
				return await asyncCpu.ExecuteBlockAsync(memory);
			}
			
			// For interpreter CPUs, use SingleStepAsync
			return await asyncCpu.SingleStepAsync(memory);
		}
		
		// Fallback to synchronous execution for non-async CPUs
		// Wrap in Task.FromResult to maintain async signature
		return await Task.FromResult(cpu.SingleStep(memory));
	}
	
	/// <summary>
	/// Execute CPU instruction(s) synchronously, with automatic backend selection
	/// </summary>
	/// <param name="cpu">The CPU instance</param>
	/// <param name="memory">Virtual memory instance</param>
	/// <returns>Result of the CPU step/block execution</returns>
	public static CpuStepResult Execute(ICpu cpu, VirtualMemory memory)
	{
		// For now, always use SingleStep for synchronous execution
		// In the future, we could add synchronous block execution if needed
		return cpu.SingleStep(memory);
	}
	
	/// <summary>
	/// Suspend CPU execution by saving complete state. Use this before async await points
	/// to ensure CPU state is preserved across async boundaries.
	/// </summary>
	/// <param name="cpu">The CPU instance</param>
	/// <returns>Saved CPU state that can be restored later, or null if CPU doesn't support state management</returns>
	public static CpuState? SuspendExecution(ICpu cpu)
	{
		if (cpu is IAsyncCpu asyncCpu)
		{
			return asyncCpu.SaveState();
		}
		return null;
	}
	
	/// <summary>
	/// Resume CPU execution by restoring saved state. Use this after async await points
	/// to restore CPU state that was saved before the async operation.
	/// </summary>
	/// <param name="cpu">The CPU instance</param>
	/// <param name="state">Previously saved CPU state</param>
	public static void ResumeExecution(ICpu cpu, CpuState? state)
	{
		if (state != null && cpu is IAsyncCpu asyncCpu)
		{
			asyncCpu.RestoreState(state);
		}
	}
	
	// Constants for EBP validation
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
		if (MemoryRegions.IsInImportHookRange(ebp)) return false;
		if (ebp >= memorySize) return false;
		
		return true;
	}

	/// <summary>
	/// Restore callee-saved registers (EBX, ESI, EDI, EBP) that were previously saved.
	/// Optionally skip restoring EBP if it was invalid when saved (prevents corruption cycle).
	/// 
	/// EBP Validation Strategy:
	/// - Per x86 calling conventions, EBP must be preserved by callees
	/// - However, some real-world code uses EBP for non-standard purposes (e.g., holding function pointers)
	/// - When skipInvalidEbp=true, we detect obviously invalid EBP values (import hooks, null, etc.)
	/// - Invalid EBP values are NOT restored, preventing corruption cycles
	/// - This handles edge cases like: MOV EBP, [IAT_Entry]; CALL EBP
	/// - After such calls, EBP contains an import hook address that should not be restored
	/// 
	/// Current Implementation (as of Issue #583):
	/// - All code paths in Emulator.cs use skipInvalidEbp=true for consistency
	/// - This provides defensive protection against EBP corruption without breaking standard code
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
			var isImportHook = MemoryRegions.IsInImportHookRange(currentEbp);
			
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
					// EBP contains an import hook address but can't be restored from stack
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

	/// <summary>
	/// Validates and logs the state of callee-saved registers before/after API calls.
	/// Helps diagnose register corruption issues.
	/// </summary>
	/// <param name="cpu">The CPU instance</param>
	/// <param name="saved">Previously saved register values</param>
	/// <param name="memorySize">Total memory size for validation</param>
	/// <param name="logger">Logger for diagnostic output</param>
	/// <param name="context">Context string for log messages (e.g., "COM call", "Syscall")</param>
	/// <param name="logLevel">Minimum log level for validation messages (default: Debug)</param>
	public static void ValidateRegisterState(
		ICpu cpu,
		SavedCalleeSavedRegisters saved,
		ulong memorySize,
		ILogger? logger = null,
		string context = "API call",
		LogLevel logLevel = LogLevel.Debug)
	{
		if (logger == null || !logger.IsEnabled(logLevel))
			return;

		var currentEbx = cpu.GetRegister("EBX");
		var currentEsi = cpu.GetRegister("ESI");
		var currentEdi = cpu.GetRegister("EDI");
		var currentEbp = cpu.GetRegister("EBP");
		var currentEsp = cpu.GetRegister("ESP");

		// Check if callee-saved registers were preserved
		var ebxPreserved = currentEbx == saved.Ebx;
		var esiPreserved = currentEsi == saved.Esi;
		var ediPreserved = currentEdi == saved.Edi;
		var ebpPreserved = currentEbp == saved.Ebp;

		// Check if EBP is valid (not corrupted)
		var ebpValid = IsEbpValid(currentEbp, memorySize);
		var savedEbpValid = IsEbpValid(saved.Ebp, memorySize);

		// Log detailed register state
		logger.Log(logLevel,
			"[RegisterValidation] After {Context}: EBX={Ebx:X8} (saved={SavedEbx:X8}, preserved={EbxPreserved}), " +
			"ESI={Esi:X8} (saved={SavedEsi:X8}, preserved={EsiPreserved}), " +
			"EDI={Edi:X8} (saved={SavedEdi:X8}, preserved={EdiPreserved}), " +
			"EBP={Ebp:X8} (saved={SavedEbp:X8}, preserved={EbpPreserved}, valid={EbpValid}, savedValid={SavedEbpValid}), " +
			"ESP={Esp:X8}",
			context,
			currentEbx, saved.Ebx, ebxPreserved,
			currentEsi, saved.Esi, esiPreserved,
			currentEdi, saved.Edi, ediPreserved,
			currentEbp, saved.Ebp, ebpPreserved, ebpValid, savedEbpValid,
			currentEsp);

		// Warn if registers were not preserved (violation of calling convention)
		if (!ebxPreserved || !esiPreserved || !ediPreserved)
		{
			logger.LogWarning(
				"[RegisterValidation] Callee-saved registers NOT preserved after {Context}: " +
				"EBX changed={EbxChanged}, ESI changed={EsiChanged}, EDI changed={EdiChanged}",
				context, !ebxPreserved, !esiPreserved, !ediPreserved);
		}

		// Warn if EBP was corrupted during the call
		if (savedEbpValid && !ebpValid && !ebpPreserved)
		{
			logger.LogWarning(
				"[RegisterValidation] EBP corrupted during {Context}: was valid 0x{SavedEbp:X8}, now invalid 0x{CurrentEbp:X8}",
				context, saved.Ebp, currentEbp);
		}
	}

	/// <summary>
	/// Consolidated helper to handle stdcall function invocation with register preservation.
	/// This reduces code duplication across different call paths in Emulator.cs.
	/// 
	/// Handles the complete flow:
	/// 1. Save callee-saved registers
	/// 2. Invoke the function
	/// 3. Set return value in EAX
	/// 4. Clean up stack (pop return address + arguments)
	/// 5. Set EIP to return address
	/// 6. Restore callee-saved registers
	/// 
	/// This function implements the x86 stdcall calling convention where:
	/// - Callee cleans up arguments from stack
	/// - Return value is in EAX
	/// - EBX, ESI, EDI, EBP must be preserved
	/// </summary>
	/// <param name="cpu">The CPU instance</param>
	/// <param name="memory">Virtual memory instance</param>
	/// <param name="invokeFunc">Function to invoke that returns (success, returnValue, argBytes)</param>
	/// <param name="memorySize">Total memory size for EBP validation</param>
	/// <param name="logger">Optional logger for diagnostics</param>
	/// <param name="context">Context string for logging (e.g., "COM call", "Import")</param>
	/// <returns>True if invocation succeeded, false otherwise</returns>
	public static bool InvokeWithRegisterPreservation(
		ICpu cpu,
		VirtualMemory memory,
		Func<(bool success, uint returnValue, int argBytes)> invokeFunc,
		ulong memorySize,
		ILogger? logger = null,
		string context = "API call")
	{
		// Save callee-saved registers
		var saved = SaveCalleeSavedRegisters(cpu);
		
		// Invoke the function
		var (success, returnValue, argBytes) = invokeFunc();
		
		if (success)
		{
			// Set return value in EAX (stdcall convention)
			cpu.SetRegister("EAX", returnValue);
			
			// Get current stack pointer and return address
			var esp = cpu.GetRegister("ESP");
			var retEip = memory.Read32(esp);
			
			// Dump stack contents for debugging
			if (logger != null && logger.IsEnabled(LogLevel.Information))
			{
				var stackDump = new System.Text.StringBuilder();
				stackDump.AppendLine($"[{context}] Stack state before cleanup:");
				try
				{
					for (int i = 0; i < 8; i++)
					{
						var addr = esp + (uint)(i * 4);
						var val = memory.Read32(addr);
						var label = i == 0 ? " (return addr)" : i <= (argBytes / 4) ? $" (arg{i})" : "";
						stackDump.AppendLine($"  [ESP+{i * 4:D2}] = 0x{addr:X8}: 0x{val:X8}{label}");
					}
				}
				catch
				{
					stackDump.AppendLine("  (error reading stack)");
				}
				logger.LogInformation(stackDump.ToString());
			}
			
			// Log detailed stack cleanup information
			logger?.LogInformation("[{Context}] Stack cleanup: ESP=0x{Esp:X8}, retEIP=0x{RetEip:X8}, argBytes={ArgBytes}, new ESP=0x{NewEsp:X8}",
				context, esp, retEip, argBytes, esp + 4 + (uint)argBytes);
			
			// Clean up stack: pop return address + arguments (stdcall convention)
			esp += 4 + (uint)argBytes;
			cpu.SetRegister("ESP", esp);
			
			// Set EIP to return address
			cpu.SetEip(retEip);
			
			// Restore callee-saved registers with EBP validation
			RestoreCalleeSavedRegisters(cpu, saved, skipInvalidEbp: true, memorySize: memorySize);
			
			// Optionally validate register state for diagnostics
			if (logger != null && logger.IsEnabled(LogLevel.Debug))
			{
				ValidateRegisterState(cpu, saved, memorySize, logger, context, LogLevel.Debug);
			}
			
			return true;
		}
		else
		{
			// Restore registers even on failure
			RestoreCalleeSavedRegisters(cpu, saved, skipInvalidEbp: true, memorySize: memorySize);
			
			// Simulate error return
			var esp = cpu.GetRegister("ESP");
			var retEip = memory.Read32(esp);
			esp += 4 + (uint)argBytes; // Pop return address and arguments to maintain stack alignment
			cpu.SetRegister("ESP", esp);
			cpu.SetRegister("EAX", 0); // Return 0 as error
			cpu.SetEip(retEip);
			
			return false;
		}
	}
}
