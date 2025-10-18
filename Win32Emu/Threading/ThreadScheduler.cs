using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Threading;

/// <summary>
/// Manages thread scheduling and context switching for emulated threads
/// </summary>
public class ThreadScheduler
{
	private readonly ILogger _logger;
	private readonly Dictionary<uint, EmulatedThread> _threads = new();
	private EmulatedThread? _currentThread;
	private uint _nextThreadId = 1;
	private uint _nextHandle = 0x1000; // Start handles at 0x1000
	private readonly object _lock = new();

	// Thread quantum (instructions per context switch)
	private const int DefaultQuantum = 1000;
	private int _currentQuantum = DefaultQuantum;
	private int _instructionsExecuted = 0;

	public ThreadScheduler(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
	}

	/// <summary>
	/// Get the currently executing thread
	/// </summary>
	public EmulatedThread? CurrentThread
	{
		get
		{
			lock (_lock)
			{
				return _currentThread;
			}
		}
	}

	/// <summary>
	/// Get thread by ID
	/// </summary>
	public EmulatedThread? GetThread(uint threadId)
	{
		lock (_lock)
		{
			return _threads.TryGetValue(threadId, out var thread) ? thread : null;
		}
	}

	/// <summary>
	/// Get thread by handle
	/// </summary>
	public EmulatedThread? GetThreadByHandle(uint handle)
	{
		lock (_lock)
		{
			return _threads.Values.FirstOrDefault(t => t.Handle == handle);
		}
	}

	/// <summary>
	/// Create a new thread
	/// </summary>
	public EmulatedThread CreateThread(uint entryPoint, uint parameter, uint stackSize, VirtualMemory memory, bool suspended = false)
	{
		lock (_lock)
		{
			var threadId = _nextThreadId++;
			var handle = _nextHandle++;

			// Allocate stack memory (align to 4KB boundary)
			var actualStackSize = (stackSize + 0xFFF) & ~0xFFFu;
			var stackBase = AllocateStack(memory, actualStackSize);

			var thread = new EmulatedThread(threadId, handle, entryPoint, parameter, stackBase, actualStackSize);
			thread.Initialize(memory);

			if (!suspended)
			{
				thread.State = ThreadState.Running;
			}

			_threads[threadId] = thread;

			// If this is the first thread (main thread), set it as current
			if (_currentThread == null)
			{
				_currentThread = thread;
			}

			_logger.LogInformation("[ThreadScheduler] Created thread {ThreadId} (handle=0x{Handle:X8}) entry=0x{EntryPoint:X8} stack=0x{StackBase:X8}-0x{StackTop:X8}",
				threadId, handle, entryPoint, stackBase - actualStackSize, stackBase);

			return thread;
		}
	}

	/// <summary>
	/// Initialize the main thread (thread ID 1)
	/// </summary>
	public EmulatedThread InitializeMainThread(ICpu cpu, VirtualMemory memory)
	{
		lock (_lock)
		{
			var threadId = 1u;
			var handle = _nextHandle++;

			// Main thread uses the existing stack
			var stackBase = cpu.GetRegister("ESP") + 0x100000; // Assume 1MB stack
			var stackSize = 0x100000u;

			var thread = new EmulatedThread(threadId, handle, cpu.GetEip(), 0, stackBase, stackSize);
			
			// Save current CPU state as the thread context
			thread.Context.SaveFrom(cpu);
			thread.State = ThreadState.Running;

			_threads[threadId] = thread;
			_currentThread = thread;
			_nextThreadId = 2; // Next thread ID will be 2

			_logger.LogInformation("[ThreadScheduler] Initialized main thread {ThreadId} (handle=0x{Handle:X8})",
				threadId, handle);

			return thread;
		}
	}

	/// <summary>
	/// Allocate stack memory for a new thread
	/// </summary>
	private uint AllocateStack(VirtualMemory memory, uint size)
	{
		// Allocate stack in high memory region (starting from 0x10000000 downward)
		// Each stack is placed 1MB apart to allow for growth
		const uint stackRegionBase = 0x10000000;
		const uint stackSpacing = 0x100000; // 1MB spacing

		var stackCount = (uint)(_threads.Count + 1);
		var stackTop = stackRegionBase - (stackCount * stackSpacing);

		// Allocate and zero the stack memory
		for (uint i = 0; i < size; i += 4)
		{
			memory.Write32(stackTop - size + i, 0);
		}

		return stackTop;
	}

	/// <summary>
	/// Resume a suspended thread
	/// </summary>
	public void ResumeThread(uint threadId)
	{
		lock (_lock)
		{
			if (_threads.TryGetValue(threadId, out var thread) && thread.State == ThreadState.Suspended)
			{
				thread.State = ThreadState.Running;
				_logger.LogInformation("[ThreadScheduler] Resumed thread {ThreadId}", threadId);
			}
		}
	}

	/// <summary>
	/// Suspend a thread
	/// </summary>
	public void SuspendThread(uint threadId)
	{
		lock (_lock)
		{
			if (_threads.TryGetValue(threadId, out var thread) && thread.State == ThreadState.Running)
			{
				thread.State = ThreadState.Suspended;
				_logger.LogInformation("[ThreadScheduler] Suspended thread {ThreadId}", threadId);
			}
		}
	}

	/// <summary>
	/// Terminate a thread
	/// </summary>
	public void TerminateThread(uint threadId, uint exitCode)
	{
		lock (_lock)
		{
			if (_threads.TryGetValue(threadId, out var thread))
			{
				thread.State = ThreadState.Terminated;
				thread.ExitCode = exitCode;
				_logger.LogInformation("[ThreadScheduler] Terminated thread {ThreadId} with exit code {ExitCode}", threadId, exitCode);

				// If this was the current thread, schedule another
				if (_currentThread == thread)
				{
					_currentThread = null;
				}
			}
		}
	}

	/// <summary>
	/// Record an instruction execution and check if context switch is needed
	/// </summary>
	public bool ShouldContextSwitch()
	{
		lock (_lock)
		{
			_instructionsExecuted++;
			
			// Only switch if we have multiple runnable threads
			if (_threads.Count(t => t.Value.State == ThreadState.Running) <= 1)
			{
				return false;
			}

			return _instructionsExecuted >= _currentQuantum;
		}
	}

	/// <summary>
	/// Perform a context switch to the next runnable thread
	/// </summary>
	public EmulatedThread? ContextSwitch(ICpu cpu)
	{
		lock (_lock)
		{
			// Save current thread's context
			if (_currentThread != null && _currentThread.State != ThreadState.Terminated)
			{
				_currentThread.Context.SaveFrom(cpu);
			}

			// Reset quantum counter
			_instructionsExecuted = 0;

			// Find next runnable thread (round-robin)
			var runnableThreads = _threads.Values
				.Where(t => t.State == ThreadState.Running)
				.OrderBy(t => t.ThreadId)
				.ToList();

			if (runnableThreads.Count == 0)
			{
				_currentThread = null;
				return null;
			}

			// Find next thread after current
			var currentIndex = _currentThread != null ? runnableThreads.FindIndex(t => t.ThreadId == _currentThread.ThreadId) : -1;
			var nextIndex = (currentIndex + 1) % runnableThreads.Count;
			var nextThread = runnableThreads[nextIndex];

			if (nextThread != _currentThread)
			{
				_logger.LogDebug("[ThreadScheduler] Context switch: {OldThreadId} -> {NewThreadId}",
					_currentThread?.ThreadId ?? 0, nextThread.ThreadId);
			}

			_currentThread = nextThread;

			// Restore next thread's context
			_currentThread.Context.RestoreTo(cpu);

			return _currentThread;
		}
	}

	/// <summary>
	/// Get all threads
	/// </summary>
	public IReadOnlyList<EmulatedThread> GetAllThreads()
	{
		lock (_lock)
		{
			return _threads.Values.ToList();
		}
	}

	/// <summary>
	/// Check if any threads are still running
	/// </summary>
	public bool HasRunningThreads()
	{
		lock (_lock)
		{
			return _threads.Values.Any(t => t.State == ThreadState.Running);
		}
	}

	/// <summary>
	/// Mark thread as waiting on a synchronization object
	/// </summary>
	public void SetThreadWaiting(uint threadId, object syncObject, uint timeoutMs)
	{
		lock (_lock)
		{
			if (_threads.TryGetValue(threadId, out var thread))
			{
				thread.State = ThreadState.Waiting;
				thread.WaitingOn = syncObject;
				
				if (timeoutMs != 0xFFFFFFFF) // INFINITE
				{
					thread.WaitTimeout = DateTime.UtcNow.AddMilliseconds(timeoutMs);
				}
				else
				{
					thread.WaitTimeout = null;
				}

				_logger.LogDebug("[ThreadScheduler] Thread {ThreadId} waiting on {SyncObject}", threadId, syncObject);
			}
		}
	}

	/// <summary>
	/// Wake thread from waiting state
	/// </summary>
	public void WakeThread(uint threadId)
	{
		lock (_lock)
		{
			if (_threads.TryGetValue(threadId, out var thread) && thread.State == ThreadState.Waiting)
			{
				thread.State = ThreadState.Running;
				thread.WaitingOn = null;
				thread.WaitTimeout = null;
				_logger.LogDebug("[ThreadScheduler] Thread {ThreadId} woken up", threadId);
			}
		}
	}

	/// <summary>
	/// Check for threads with expired wait timeouts
	/// </summary>
	public void ProcessWaitTimeouts()
	{
		lock (_lock)
		{
			var now = DateTime.UtcNow;
			foreach (var thread in _threads.Values.Where(t => t.State == ThreadState.Waiting && t.WaitTimeout.HasValue))
			{
				if (now >= thread.WaitTimeout!.Value)
				{
					_logger.LogDebug("[ThreadScheduler] Thread {ThreadId} wait timeout expired", thread.ThreadId);
					WakeThread(thread.ThreadId);
				}
			}
		}
	}
}
