using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Threading;

/// <summary>
/// Manages Win32 synchronization objects
/// </summary>
public class SynchronizationManager(ILogger? logger = null)
{
	private readonly ILogger _logger = logger ?? NullLogger.Instance;
	private readonly Dictionary<uint, EmulatedMutex> _mutexes = new();
	private readonly Dictionary<uint, EmulatedEvent> _events = new();
	private readonly Dictionary<uint, EmulatedSemaphore> _semaphores = new();
	private readonly Dictionary<string, uint> _namedObjects = new(); // name -> handle
	private uint _nextHandle = 0x2000;
	private readonly Lock _lock = new();

	#region Mutex Operations

	/// <summary>
	/// Create a mutex
	/// </summary>
	public uint CreateMutex(bool initialOwner, string? name, uint currentThreadId, out bool alreadyExists)
	{
		lock (_lock)
		{
			alreadyExists = false;

			// Check if named mutex already exists
			if (!string.IsNullOrEmpty(name) && _namedObjects.TryGetValue(name, out var existingHandle))
			{
				alreadyExists = true;
				_logger.LogInformation("[SyncMgr] Opened existing mutex '{Name}' (handle=0x{Handle:X8})", name, existingHandle);
				return existingHandle;
			}

			var handle = _nextHandle++;
			var mutex = new EmulatedMutex(handle, name);

			if (initialOwner)
			{
				mutex.OwningThreadId = currentThreadId;
				mutex.RecursionCount = 1;
			}

			_mutexes[handle] = mutex;

			if (!string.IsNullOrEmpty(name))
			{
				_namedObjects[name] = handle;
			}

			_logger.LogInformation("[SyncMgr] Created mutex '{Name}' (handle=0x{Handle:X8}, initialOwner={InitialOwner})",
				name ?? "<unnamed>", handle, initialOwner);

			return handle;
		}
	}

	/// <summary>
	/// Acquire a mutex (returns true if acquired, false if needs to wait)
	/// </summary>
	public bool AcquireMutex(uint handle, uint threadId)
	{
		lock (_lock)
		{
			if (!_mutexes.TryGetValue(handle, out var mutex))
			{
				_logger.LogWarning("[SyncMgr] AcquireMutex: invalid handle 0x{Handle:X8}", handle);
				return false;
			}

			// Already owned by this thread - recursive acquisition
			if (mutex.OwningThreadId == threadId)
			{
				mutex.RecursionCount++;
				_logger.LogDebug("[SyncMgr] Mutex 0x{Handle:X8} recursively acquired (count={Count})", handle, mutex.RecursionCount);
				return true;
			}

			// Not owned - acquire it
			if (!mutex.IsOwned)
			{
				mutex.OwningThreadId = threadId;
				mutex.RecursionCount = 1;
				_logger.LogDebug("[SyncMgr] Mutex 0x{Handle:X8} acquired by thread {ThreadId}", handle, threadId);
				return true;
			}

			// Owned by another thread - need to wait
			if (!mutex.WaitingThreads.Contains(threadId))
			{
				mutex.WaitingThreads.Enqueue(threadId);
				_logger.LogDebug("[SyncMgr] Thread {ThreadId} queued for mutex 0x{Handle:X8}", threadId, handle);
			}
			return false;
		}
	}

	/// <summary>
	/// Check if a mutex can be acquired by the specified thread (without actually acquiring it)
	/// </summary>
	public bool CanAcquireMutex(uint handle, uint threadId)
	{
		lock (_lock)
		{
			if (!_mutexes.TryGetValue(handle, out var mutex))
			{
				return false;
			}

			// Already owned by this thread - can recursively acquire
			if (mutex.OwningThreadId == threadId)
			{
				return true;
			}

			// Not owned - can acquire it
			return !mutex.IsOwned;
		}
	}

	/// <summary>
	/// Release a mutex
	/// </summary>
	public bool ReleaseMutex(uint handle, uint threadId)
	{
		lock (_lock)
		{
			if (!_mutexes.TryGetValue(handle, out var mutex))
			{
				_logger.LogWarning("[SyncMgr] ReleaseMutex: invalid handle 0x{Handle:X8}", handle);
				return false;
			}

			if (mutex.OwningThreadId != threadId)
			{
				_logger.LogWarning("[SyncMgr] ReleaseMutex: thread {ThreadId} does not own mutex 0x{Handle:X8}", threadId, handle);
				return false;
			}

			mutex.RecursionCount--;

			if (mutex.RecursionCount == 0)
			{
				mutex.OwningThreadId = 0;
				_logger.LogDebug("[SyncMgr] Mutex 0x{Handle:X8} released by thread {ThreadId}", handle, threadId);
			}
			else
			{
				_logger.LogDebug("[SyncMgr] Mutex 0x{Handle:X8} recursion count decreased to {Count}", handle, mutex.RecursionCount);
			}

			return true;
		}
	}

	/// <summary>
	/// Get the next waiting thread for a mutex (if any)
	/// </summary>
	public uint? GetNextMutexWaiter(uint handle)
	{
		lock (_lock)
		{
			if (_mutexes.TryGetValue(handle, out var mutex) && mutex.WaitingThreads.Count > 0)
			{
				return mutex.WaitingThreads.Dequeue();
			}
			return null;
		}
	}

	#endregion

	#region Event Operations

	/// <summary>
	/// Create an event
	/// </summary>
	public uint CreateEvent(bool manualReset, bool initialState, string? name, out bool alreadyExists)
	{
		lock (_lock)
		{
			alreadyExists = false;

			// Check if named event already exists
			if (!string.IsNullOrEmpty(name) && _namedObjects.TryGetValue(name, out var existingHandle))
			{
				alreadyExists = true;
				_logger.LogInformation("[SyncMgr] Opened existing event '{Name}' (handle=0x{Handle:X8})", name, existingHandle);
				return existingHandle;
			}

			var handle = _nextHandle++;
			var evt = new EmulatedEvent(handle, name, manualReset, initialState);

			_events[handle] = evt;

			if (!string.IsNullOrEmpty(name))
			{
				_namedObjects[name] = handle;
			}

			_logger.LogInformation("[SyncMgr] Created event '{Name}' (handle=0x{Handle:X8}, manual={Manual}, initial={Initial})",
				name ?? "<unnamed>", handle, manualReset, initialState);

			return handle;
		}
	}

	/// <summary>
	/// Set an event to signaled state
	/// </summary>
	public bool SetEvent(uint handle)
	{
		lock (_lock)
		{
			if (!_events.TryGetValue(handle, out var evt))
			{
				_logger.LogWarning("[SyncMgr] SetEvent: invalid handle 0x{Handle:X8}", handle);
				return false;
			}

			evt.Signaled = true;
			_logger.LogDebug("[SyncMgr] Event 0x{Handle:X8} set to signaled", handle);
			return true;
		}
	}

	/// <summary>
	/// Reset an event to non-signaled state
	/// </summary>
	public bool ResetEvent(uint handle)
	{
		lock (_lock)
		{
			if (!_events.TryGetValue(handle, out var evt))
			{
				_logger.LogWarning("[SyncMgr] ResetEvent: invalid handle 0x{Handle:X8}", handle);
				return false;
			}

			evt.Signaled = false;
			_logger.LogDebug("[SyncMgr] Event 0x{Handle:X8} reset to non-signaled", handle);
			return true;
		}
	}

	/// <summary>
	/// Opens an existing named event object
	/// </summary>
	public uint OpenEvent(string name, uint dwDesiredAccess)
	{
		lock (_lock)
		{
			const uint NULL_HANDLE = 0;
			
			// Check if named event exists
			if (string.IsNullOrEmpty(name) || !_namedObjects.TryGetValue(name, out var existingHandle))
			{
				_logger.LogWarning("[SyncMgr] OpenEvent: event '{Name}' not found", name);
				return NULL_HANDLE;
			}

			// Verify it's actually an event (not a mutex or semaphore)
			if (!_events.ContainsKey(existingHandle))
			{
				_logger.LogWarning("[SyncMgr] OpenEvent: handle 0x{Handle:X8} is not an event", existingHandle);
				return NULL_HANDLE;
			}

			_logger.LogInformation("[SyncMgr] Opened existing event '{Name}' (handle=0x{Handle:X8})", name, existingHandle);
			return existingHandle;
		}
	}

	/// <summary>
	/// Check if an event is signaled
	/// </summary>
	public bool IsEventSignaled(uint handle)
	{
		lock (_lock)
		{
			return _events.TryGetValue(handle, out var evt) && evt.Signaled;
		}
	}

	/// <summary>
	/// Wait on an event (returns true if signaled)
	/// </summary>
	public bool WaitOnEvent(uint handle, uint threadId)
	{
		lock (_lock)
		{
			if (!_events.TryGetValue(handle, out var evt))
			{
				return false;
			}

			if (evt.Signaled)
			{
				// For auto-reset events, reset after successful wait
				if (!evt.ManualReset)
				{
					evt.Signaled = false;
					_logger.LogDebug("[SyncMgr] Event 0x{Handle:X8} auto-reset", handle);
				}
				return true;
			}

			// Not signaled, add to wait queue
			if (!evt.WaitingThreads.Contains(threadId))
			{
				evt.WaitingThreads.Enqueue(threadId);
			}
			return false;
		}
	}

	/// <summary>
	/// Get all threads waiting on an event
	/// </summary>
	public List<uint> GetEventWaiters(uint handle)
	{
		lock (_lock)
		{
			if (_events.TryGetValue(handle, out var evt))
			{
				return evt.WaitingThreads.ToList();
			}
			return new List<uint>();
		}
	}

	#endregion

	#region Semaphore Operations

	/// <summary>
	/// Create a semaphore
	/// </summary>
	public uint CreateSemaphore(uint initialCount, uint maximumCount, string? name, out bool alreadyExists)
	{
		lock (_lock)
		{
			alreadyExists = false;

			// Check if named semaphore already exists
			if (!string.IsNullOrEmpty(name) && _namedObjects.TryGetValue(name, out var existingHandle))
			{
				alreadyExists = true;
				_logger.LogInformation("[SyncMgr] Opened existing semaphore '{Name}' (handle=0x{Handle:X8})", name, existingHandle);
				return existingHandle;
			}

			var handle = _nextHandle++;
			var semaphore = new EmulatedSemaphore(handle, name, initialCount, maximumCount);

			_semaphores[handle] = semaphore;

			if (!string.IsNullOrEmpty(name))
			{
				_namedObjects[name] = handle;
			}

			_logger.LogInformation("[SyncMgr] Created semaphore '{Name}' (handle=0x{Handle:X8}, initial={Initial}, max={Max})",
				name ?? "<unnamed>", handle, initialCount, maximumCount);

			return handle;
		}
	}

	/// <summary>
	/// Wait on a semaphore (returns true if acquired)
	/// </summary>
	public bool WaitOnSemaphore(uint handle, uint threadId)
	{
		lock (_lock)
		{
			if (!_semaphores.TryGetValue(handle, out var semaphore))
			{
				return false;
			}

			if (semaphore.CurrentCount > 0)
			{
				semaphore.CurrentCount--;
				_logger.LogDebug("[SyncMgr] Semaphore 0x{Handle:X8} acquired (count={Count})", handle, semaphore.CurrentCount);
				return true;
			}

			// No count available, add to wait queue
			if (!semaphore.WaitingThreads.Contains(threadId))
			{
				semaphore.WaitingThreads.Enqueue(threadId);
			}
			return false;
		}
	}

	/// <summary>
	/// Check if a semaphore is signaled (has available count)
	/// </summary>
	public bool IsSemaphoreSignaled(uint handle)
	{
		lock (_lock)
		{
			return _semaphores.TryGetValue(handle, out var semaphore) && semaphore.IsSignaled;
		}
	}

	/// <summary>
	/// Release a semaphore
	/// </summary>
	public bool ReleaseSemaphore(uint handle, uint releaseCount, out uint previousCount)
	{
		lock (_lock)
		{
			previousCount = 0;

			if (!_semaphores.TryGetValue(handle, out var semaphore))
			{
				return false;
			}

			previousCount = semaphore.CurrentCount;

			if (semaphore.CurrentCount + releaseCount > semaphore.MaximumCount)
			{
				_logger.LogWarning("[SyncMgr] ReleaseSemaphore: would exceed maximum count");
				return false;
			}

			semaphore.CurrentCount += releaseCount;
			_logger.LogDebug("[SyncMgr] Semaphore 0x{Handle:X8} released (count={Count})", handle, semaphore.CurrentCount);
			return true;
		}
	}

	/// <summary>
	/// Get the next waiting thread for a semaphore (if any)
	/// </summary>
	public uint? GetNextSemaphoreWaiter(uint handle)
	{
		lock (_lock)
		{
			if (_semaphores.TryGetValue(handle, out var semaphore) && semaphore.WaitingThreads.Count > 0)
			{
				return semaphore.WaitingThreads.Dequeue();
			}
			return null;
		}
	}

	#endregion

	#region General Operations

	/// <summary>
	/// Check if a handle is valid
	/// </summary>
	public bool IsValidHandle(uint handle)
	{
		lock (_lock)
		{
			return _mutexes.ContainsKey(handle) ||
			       _events.ContainsKey(handle) ||
			       _semaphores.ContainsKey(handle);
		}
	}

	/// <summary>
	/// Get the type of synchronization object
	/// </summary>
	public string? GetObjectType(uint handle)
	{
		lock (_lock)
		{
			if (_mutexes.ContainsKey(handle))
			{
				return "Mutex";
			}

			if (_events.ContainsKey(handle))
			{
				return "Event";
			}

			if (_semaphores.ContainsKey(handle))
			{
				return "Semaphore";
			}

			return null;
		}
	}

	/// <summary>
	/// Close a synchronization object handle
	/// </summary>
	public bool CloseHandle(uint handle)
	{
		lock (_lock)
		{
			if (_mutexes.Remove(handle, out var mutex))
			{
				if (!string.IsNullOrEmpty(mutex.Name))
				{
					_namedObjects.Remove(mutex.Name);
				}
				_logger.LogInformation("[SyncMgr] Closed mutex handle 0x{Handle:X8}", handle);
				return true;
			}

			if (_events.Remove(handle, out var evt))
			{
				if (!string.IsNullOrEmpty(evt.Name))
				{
					_namedObjects.Remove(evt.Name);
				}
				_logger.LogInformation("[SyncMgr] Closed event handle 0x{Handle:X8}", handle);
				return true;
			}

			if (_semaphores.Remove(handle, out var semaphore))
			{
				if (!string.IsNullOrEmpty(semaphore.Name))
				{
					_namedObjects.Remove(semaphore.Name);
				}
				_logger.LogInformation("[SyncMgr] Closed semaphore handle 0x{Handle:X8}", handle);
				return true;
			}

			return false;
		}
	}

	#endregion
}
