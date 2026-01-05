using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Helper class for handling import calls during callback execution.
/// Provides shared logic for dispatching Win32 API import calls and managing CPU state.
/// </summary>
public static class ImportCallHelper
{
	/// <summary>
	/// Handles an import call by dispatching it through the Win32Dispatcher and managing CPU state.
	/// This method encapsulates the common logic for calling imported Win32 API functions during callback execution.
	/// </summary>
	/// <param name="dll">The DLL name (e.g., "KERNEL32.DLL")</param>
	/// <param name="name">The function name (e.g., "GetModuleHandleA")</param>
	/// <param name="cpu">The CPU instance</param>
	/// <param name="memory">The virtual memory instance</param>
	/// <param name="dispatcher">The Win32 dispatcher for invoking the function</param>
	/// <param name="image">The loaded image for return address validation (optional)</param>
	/// <param name="logger">The logger instance</param>
	/// <param name="logContext">Context string for logging (e.g., "msvcrt", "User32")</param>
	/// <param name="isValidReturnAddressFunc">Function to validate return addresses (optional)</param>
	/// <param name="shouldBreak">Output parameter indicating if execution should stop</param>
	/// <returns>True if the import call was successfully handled, false otherwise</returns>
	public static bool HandleImportCall(
		string dll,
		string name,
		ICpu cpu,
		VirtualMemory memory,
		Win32Dispatcher? dispatcher,
		LoadedImage? image,
		ILogger logger,
		string logContext,
		System.Func<uint, bool>? isValidReturnAddressFunc,
		out bool shouldBreak)
	{
		shouldBreak = false;

		// Save callee-saved registers (EBX, ESI, EDI, EBP)
		var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);

		if (dispatcher != null && dispatcher.TryInvoke(dll, name, cpu, memory, out var ret, out var argBytes))
		{
			logger.LogDebug("[{Context}] Import {Dll}!{Name} returned 0x{Ret:X8}", logContext, dll, name, ret);

			var currentEsp = cpu.GetRegister("ESP");
			var retEip = memory.Read32(currentEsp);

			// Validate return address before jumping
			if (isValidReturnAddressFunc != null && !isValidReturnAddressFunc(retEip))
			{
				logger.LogError("[{Context}] Invalid return address 0x{RetEip:X8} from import {Dll}!{Name}", logContext, retEip, dll, name);
				shouldBreak = true;
				return true;
			}

			currentEsp += 4 + (uint)argBytes;

			cpu.SetRegister("ESP", currentEsp);
			cpu.SetRegister("EAX", ret);
			cpu.SetEip(retEip);

			// Restore callee-saved registers, skipping invalid EBP values
			CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved, skipInvalidEbp: true, memorySize: memory.Size);
			return true;
		}
		else
		{
			// Import function not implemented - try to get arg bytes from metadata and simulate return
			var simulatedArgBytes = 0;
			try
			{
				simulatedArgBytes = StdCallMeta.GetArgBytes(dll, name);
				logger.LogWarning("[{Context}] Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes={ArgBytes}", logContext, dll, name, simulatedArgBytes);
			}
			catch (System.Exception ex)
			{
				logger.LogError(ex, "[{Context}] Unimplemented import {Dll}!{Name}, simulating return with 0, argBytes unknown (assuming 0)", logContext, dll, name);
			}

			var currentEsp = cpu.GetRegister("ESP");
			var retEip = memory.Read32(currentEsp);

			// Validate return address before jumping
			if (isValidReturnAddressFunc != null && !isValidReturnAddressFunc(retEip))
			{
				logger.LogError("[{Context}] Invalid return address 0x{RetEip:X8} from unimplemented import {Dll}!{Name}", logContext, retEip, dll, name);
				shouldBreak = true;
				return true;
			}

			// Pop return address + parameters (stdcall convention - callee cleans)
			currentEsp += 4 + (uint)simulatedArgBytes;

			cpu.SetRegister("ESP", currentEsp);
			cpu.SetRegister("EAX", 0); // Return 0 as default
			cpu.SetEip(retEip);

			// Restore callee-saved registers, skipping invalid EBP values
			CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved, skipInvalidEbp: true, memorySize: memory.Size);
			return true;
		}
	}
}
