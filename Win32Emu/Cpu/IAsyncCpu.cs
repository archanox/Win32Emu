using Win32Emu.Memory;

namespace Win32Emu.Cpu;

/// <summary>
/// Extended CPU interface that supports async execution for JIT-compiled code
/// </summary>
public interface IAsyncCpu : ICpu
{
	/// <summary>
	/// Execute a single instruction asynchronously. This allows for await-based async operations
	/// during import calls, COM method invocations, and message processing.
	/// </summary>
	/// <param name="mem">Virtual memory instance</param>
	/// <returns>Result of the CPU step including call information</returns>
	Task<CpuStepResult> SingleStepAsync(VirtualMemory mem);
	
	/// <summary>
	/// Execute multiple instructions asynchronously until a breakpoint, call, or limit is reached.
	/// This is the primary method for JIT-compiled execution blocks.
	/// </summary>
	/// <param name="mem">Virtual memory instance</param>
	/// <param name="maxInstructions">Maximum number of instructions to execute (0 = no limit)</param>
	/// <returns>Result of the execution block including call information</returns>
	Task<CpuStepResult> ExecuteBlockAsync(VirtualMemory mem, int maxInstructions = 0);
	
	/// <summary>
	/// Check if this CPU backend supports JIT compilation
	/// </summary>
	bool SupportsJit { get; }
	
	/// <summary>
	/// Save the complete CPU state for suspension across async boundaries
	/// </summary>
	/// <returns>Serialized CPU state that can be restored later</returns>
	CpuState SaveState();
	
	/// <summary>
	/// Restore the complete CPU state after resuming from async operation
	/// </summary>
	/// <param name="state">Previously saved CPU state</param>
	void RestoreState(CpuState state);
}

/// <summary>
/// Represents a complete snapshot of CPU state for async suspension/resumption
/// </summary>
public class CpuState
{
	public uint Eax { get; set; }
	public uint Ebx { get; set; }
	public uint Ecx { get; set; }
	public uint Edx { get; set; }
	public uint Esi { get; set; }
	public uint Edi { get; set; }
	public uint Ebp { get; set; }
	public uint Esp { get; set; }
	public uint Eip { get; set; }
	public uint Eflags { get; set; }
	
	// FPU state (optional, can be extended as needed)
	public double[]? FpuStack { get; set; }
	public int FpuTop { get; set; }
	public ushort FpuControlWord { get; set; }
	public ushort FpuStatusWord { get; set; }
}
