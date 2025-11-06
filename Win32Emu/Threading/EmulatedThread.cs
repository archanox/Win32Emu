using Win32Emu.Memory;

namespace Win32Emu.Threading;

/// <summary>
/// Represents an emulated Win32 thread with its own CPU context and stack
/// </summary>
public class EmulatedThread
{
	/// <summary>
	/// Unique thread identifier
	/// </summary>
	public uint ThreadId { get; }

	/// <summary>
	/// Thread handle (for Win32 APIs)
	/// </summary>
	public uint Handle { get; }

	/// <summary>
	/// Current state of the thread
	/// </summary>
	public ThreadState State { get; set; }

	/// <summary>
	/// CPU context (registers, EIP, EFLAGS)
	/// </summary>
	public CpuContext Context { get; set; }

	/// <summary>
	/// Thread entry point address
	/// </summary>
	public uint EntryPoint { get; }

	/// <summary>
	/// Parameter passed to thread entry point
	/// </summary>
	public uint Parameter { get; }

	/// <summary>
	/// Stack base address (top of stack)
	/// </summary>
	public uint StackBase { get; }

	/// <summary>
	/// Stack size in bytes
	/// </summary>
	public uint StackSize { get; }

	/// <summary>
	/// Thread exit code (set when terminated)
	/// </summary>
	public uint ExitCode { get; set; }

	/// <summary>
	/// Thread-local storage for this thread
	/// </summary>
	public Dictionary<uint, uint> ThreadLocalStorage { get; } = new();

	/// <summary>
	/// Synchronization object this thread is waiting on (if State == Waiting)
	/// </summary>
	public object? WaitingOn { get; set; }

	/// <summary>
	/// Timeout for wait operation (if applicable)
	/// </summary>
	public DateTime? WaitTimeout { get; set; }

	/// <summary>
	/// Thread priority (Win32 priority value)
	/// </summary>
	public int Priority { get; set; }

	public EmulatedThread(uint threadId, uint handle, uint entryPoint, uint parameter, uint stackBase, uint stackSize)
	{
		ThreadId = threadId;
		Handle = handle;
		EntryPoint = entryPoint;
		Parameter = parameter;
		StackBase = stackBase;
		StackSize = stackSize;
		State = ThreadState.Suspended;
		Context = new CpuContext();
		Priority = 0; // THREAD_PRIORITY_NORMAL
	}

	/// <summary>
	/// Initialize thread context for first execution
	/// </summary>
	public void Initialize(VirtualMemory memory)
	{
		// Set up initial stack pointer (stack grows downward)
		Context.ESP = StackBase - 4; // Leave space for return address

		// Set entry point
		Context.EIP = EntryPoint;

		// Push return address (0xFFFFFFFF to detect thread exit)
		memory.Write32(Context.ESP, 0xFFFFFFFF);
		Context.ESP -= 4;

		// Push thread parameter
		memory.Write32(Context.ESP, Parameter);

		// Set initial register state
		Context.EBP = StackBase;
		Context.EAX = 0;
		Context.EBX = 0;
		Context.ECX = 0;
		Context.EDX = 0;
		Context.ESI = 0;
		Context.EDI = 0;
		Context.EFLAGS = 0x202; // Standard initial EFLAGS (interrupts enabled)
	}
}