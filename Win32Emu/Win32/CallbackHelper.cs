using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Helper class for invoking stdcall callbacks in emulated code.
/// This allows native (host) code to call back into Win32 emulated code.
/// </summary>
public class CallbackHelper
{
	private readonly ICpu _cpu;
	private readonly VirtualMemory _memory;
	private readonly ILogger _logger;

	public CallbackHelper(ICpu cpu, VirtualMemory memory, ILogger logger)
	{
		_cpu = cpu;
		_memory = memory;
		_logger = logger;
	}

	/// <summary>
	/// Invokes a stdcall callback function in emulated code.
	/// The stdcall convention means:
	/// - Arguments pushed right-to-left on stack
	/// - Callee cleans up stack
	/// - Return value in EAX
	/// </summary>
	/// <param name="callbackAddress">Address of the callback function in emulated memory</param>
	/// <param name="parameters">Parameters to pass to the callback (pushed right-to-left)</param>
	/// <param name="maxInstructions">Maximum number of instructions to execute (safety limit)</param>
	/// <returns>Return value from EAX, or null if callback failed</returns>
	public uint? InvokeStdcallCallback(uint callbackAddress, uint[] parameters, int maxInstructions = 10000)
	{
		if (callbackAddress == 0)
		{
			_logger.LogError("[CallbackHelper] Invalid callback address: 0x00000000");
			return null;
		}

		// Save current CPU state
		var savedEip = _cpu.GetEip();
		var savedEsp = _cpu.GetRegister("ESP");
		var savedEbp = _cpu.GetRegister("EBP");
		var savedEax = _cpu.GetRegister("EAX");
		var savedEcx = _cpu.GetRegister("ECX");
		var savedEdx = _cpu.GetRegister("EDX");
		var savedEbx = _cpu.GetRegister("EBX");
		var savedEsi = _cpu.GetRegister("ESI");
		var savedEdi = _cpu.GetRegister("EDI");
		var savedEflags = _cpu.GetRegister("EFLAGS");

		try
		{
			// Allocate temporary stack space for callback
			var tempStackSize = (uint)((parameters.Length + 2) * 4 + 64); // Extra space for safety
			var tempStackTop = savedEsp - tempStackSize;
			
			// Align stack to 16 bytes (modern calling convention preference)
			tempStackTop &= 0xFFFFFFF0;
			
			// Set up stack pointer
			_cpu.SetRegister("ESP", tempStackTop);
			var esp = tempStackTop;

			// Push return address (special marker value to detect when callback returns)
			const uint CALLBACK_RETURN_MARKER = 0xDEADBEEF;
			esp -= 4;
			_memory.Write32(esp, CALLBACK_RETURN_MARKER);

			// Push parameters in reverse order (right-to-left for stdcall)
			for (int i = parameters.Length - 1; i >= 0; i--)
			{
				esp -= 4;
				_memory.Write32(esp, parameters[i]);
			}

			// Update ESP to point to first parameter
			_cpu.SetRegister("ESP", esp);

			// Set EIP to callback address
			_cpu.SetEip(callbackAddress);

			_logger.LogDebug("[CallbackHelper] Invoking callback at 0x{Address:X8} with {Count} parameters",
				callbackAddress, parameters.Length);

			// Execute callback until it returns (EIP reaches marker or max instructions)
			int instructionsExecuted = 0;
			while (instructionsExecuted < maxInstructions)
			{
				var currentEip = _cpu.GetEip();

				// Check if we've returned (EIP at the return marker address)
				// In reality, the RET instruction will try to jump to the marker value
				// We detect this by checking if EIP is the marker value
				if (currentEip == CALLBACK_RETURN_MARKER)
				{
					_logger.LogDebug("[CallbackHelper] Callback returned after {Count} instructions",
						instructionsExecuted);
					
					// Get return value from EAX
					var returnValue = _cpu.GetRegister("EAX");
					
					return returnValue;
				}

				// Execute one instruction
				try
				{
					_cpu.SingleStep(_memory);
					instructionsExecuted++;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[CallbackHelper] Exception during callback execution at EIP=0x{Eip:X8}",
						currentEip);
					return null;
				}
			}

			_logger.LogWarning("[CallbackHelper] Callback at 0x{Address:X8} exceeded max instructions ({Max})",
				callbackAddress, maxInstructions);
			return null;
		}
		finally
		{
			// Restore CPU state
			_cpu.SetEip(savedEip);
			_cpu.SetRegister("ESP", savedEsp);
			_cpu.SetRegister("EBP", savedEbp);
			_cpu.SetRegister("EAX", savedEax);
			_cpu.SetRegister("ECX", savedEcx);
			_cpu.SetRegister("EDX", savedEdx);
			_cpu.SetRegister("EBX", savedEbx);
			_cpu.SetRegister("ESI", savedEsi);
			_cpu.SetRegister("EDI", savedEdi);
			_cpu.SetRegister("EFLAGS", savedEflags);
		}
	}
}
