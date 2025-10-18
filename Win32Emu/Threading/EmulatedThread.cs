using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Threading;

/// <summary>
/// Represents the state of an emulated thread
/// </summary>
public enum ThreadState
{
	/// <summary>Thread is ready to run or currently running</summary>
	Running,
	/// <summary>Thread is suspended (not scheduled)</summary>
	Suspended,
	/// <summary>Thread is waiting on a synchronization object</summary>
	Waiting,
	/// <summary>Thread has terminated</summary>
	Terminated
}

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

/// <summary>
/// Represents a saved CPU context for thread switching
/// </summary>
public class CpuContext
{
	public uint EAX { get; set; }
	public uint EBX { get; set; }
	public uint ECX { get; set; }
	public uint EDX { get; set; }
	public uint ESI { get; set; }
	public uint EDI { get; set; }
	public uint EBP { get; set; }
	public uint ESP { get; set; }
	public uint EIP { get; set; }
	public uint EFLAGS { get; set; }

	/// <summary>
	/// Save CPU state from ICpu
	/// </summary>
	public void SaveFrom(ICpu cpu)
	{
		EAX = cpu.GetRegister("EAX");
		EBX = cpu.GetRegister("EBX");
		ECX = cpu.GetRegister("ECX");
		EDX = cpu.GetRegister("EDX");
		ESI = cpu.GetRegister("ESI");
		EDI = cpu.GetRegister("EDI");
		EBP = cpu.GetRegister("EBP");
		ESP = cpu.GetRegister("ESP");
		EIP = cpu.GetEip();
		EFLAGS = cpu.GetRegister("EFLAGS");
	}

	/// <summary>
	/// Restore CPU state to ICpu
	/// </summary>
	public void RestoreTo(ICpu cpu)
	{
		cpu.SetRegister("EAX", EAX);
		cpu.SetRegister("EBX", EBX);
		cpu.SetRegister("ECX", ECX);
		cpu.SetRegister("EDX", EDX);
		cpu.SetRegister("ESI", ESI);
		cpu.SetRegister("EDI", EDI);
		cpu.SetRegister("EBP", EBP);
		cpu.SetRegister("ESP", ESP);
		cpu.SetEip(EIP);
		cpu.SetRegister("EFLAGS", EFLAGS);
	}

	/// <summary>
	/// Create a copy of this context
	/// </summary>
	public CpuContext Clone()
	{
		return new CpuContext
		{
			EAX = EAX,
			EBX = EBX,
			ECX = ECX,
			EDX = EDX,
			ESI = ESI,
			EDI = EDI,
			EBP = EBP,
			ESP = ESP,
			EIP = EIP,
			EFLAGS = EFLAGS
		};
	}
}
