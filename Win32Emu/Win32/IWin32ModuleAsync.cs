using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Extended Win32 module interface that supports async execution for window procedures and message handling.
/// This enables clean separation between host (C#) and guest (x86) stacks, eliminating the need for
/// STACK_SAFETY_MARGIN and preventing stack corruption in nested calls.
/// </summary>
public interface IWin32ModuleAsync : IWin32ModuleUnsafe
{
	/// <summary>
	/// Attempts to invoke a Win32 API export asynchronously.
	/// This is used for APIs that may call back into emulated code (e.g., window procedures, dialog procedures).
	/// </summary>
	/// <param name="export">Name of the exported function</param>
	/// <param name="cpu">CPU instance (must implement IAsyncCpu for async execution)</param>
	/// <param name="memory">Virtual memory instance</param>
	/// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
	/// <returns>Task with tuple containing success flag and return value</returns>
	Task<(bool success, uint returnValue)> TryInvokeAsync(string export, ICpu cpu, VirtualMemory memory, CancellationToken cancellationToken = default);
}
