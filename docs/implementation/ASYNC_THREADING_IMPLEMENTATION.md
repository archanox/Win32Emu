# Async/Await Threading Implementation

This document describes the async/await and cooperative threading enhancements made to Win32Emu to fix blocking issues in ign_teas.exe and other multi-threaded applications.

## Overview

The emulator now uses cooperative multitasking throughout the Win32 API layer, allowing complex window procedures, COM calls, and synchronization operations to execute without blocking other threads or the main emulation loop.

## Key Changes

### 1. User32Module: Window Message Processing

#### CallWindowProcedure
- **Increased safety limit**: MAX_STEPS raised from 100,000 to 500,000 instructions
- **Cooperative yielding**: Checks for thread context switches every 10,000 instructions
- **ThreadScheduler integration**: Processes wait timeouts and checks for context switch needs
- **Better diagnostics**: Enhanced logging to identify infinite loops in WndProcs

**Impact**: Window procedures that previously timed out (causing "WndProc returned 0xDEADBEEF" errors) can now complete successfully while yielding to other threads.

#### CallDialogProcedureWithTimeout
- Same improvements as CallWindowProcedure
- Handles dialog procedures that need extended execution time
- Cooperates with COM vtable calls within dialog procedures

### 2. ProcessEnvironment: Message Queue Operations

#### GetMessageAsync (NEW)
```csharp
public async Task<QueuedMessage?> GetMessageAsync(
    uint hwnd, uint msgFilterMin, uint msgFilterMax, int timeoutMs = 100)
```

- Properly async method using `await` for channel operations
- Uses `CancellationToken` for timeout handling
- Yields control during waiting, enabling true async/await patterns
- Preferred method for new code

#### GetMessageBlocking (REFACTORED)
- Now wraps GetMessageAsync for backward compatibility
- Uses `GetAwaiter().GetResult()` for synchronous contexts
- Maintains existing API contract while benefiting from async implementation

**Impact**: Message queue operations no longer block the emulation loop, allowing cooperative multitasking between message processing and CPU execution.

### 3. Kernel32Module: Threading Primitives

#### Sleep
```csharp
private uint Sleep(uint dwMilliseconds)
```

**Enhancements**:
- **Sleep(0)**: Cooperates with ThreadScheduler instead of just `Thread.Yield()`
- **Sleep(INFINITE)**: Marks thread as waiting instead of blocking indefinitely
- **Timed sleeps**: Uses `ThreadScheduler.SetThreadWaiting()` for cooperative multitasking
- **Minimal blocking**: Only does short native sleeps (1ms) to prevent busy-waiting

**Impact**: Sleep calls no longer block the emulator. Threads properly yield control and can be context-switched.

#### WaitForSingleObject
```csharp
private uint WaitForSingleObject(uint hHandle, uint dwMilliseconds)
```

**Enhancements**:
- **Polling loop**: Implements proper blocking behavior by polling until object is signaled or timeout expires
- **Timeout handling**: Correctly handles zero timeout (immediate return) and INFINITE timeout
- **Cooperative yielding**: Uses `Thread.Sleep(1)` and ProcessWaitTimeouts() to prevent busy-waiting
- **Detailed logging**: Tracks synchronization object states (mutex, event, semaphore)
- **Win32 conformance**: Now properly blocks and waits, conforming to Win32 API specifications

**Impact**: Synchronization operations now correctly block until signaled or timeout, matching Win32 behavior. Applications using WaitForSingleObject will work correctly without needing retry loops.

### 4. Ole32Module: COM Initialization

#### CoInitialize
- Added comprehensive documentation about threading model
- Clarified that initialization is synchronous but non-blocking
- Explained integration with emulator's threading system

### 5. COM Vtable Dispatcher

#### Threading Model Documentation
Added clear documentation explaining:
- COM methods should be synchronous but non-blocking
- Handlers should complete quickly
- Guidelines for operations that need to wait
- Cooperative threading patterns to use

## Threading Model

### Cooperative Multitasking

The emulator uses cooperative multitasking where:

1. **Long-running operations yield periodically**: Window procedures and dialog procedures check for context switches every 10K instructions
2. **Blocking operations mark threads as waiting**: Sleep and WaitForSingleObject use ThreadScheduler instead of blocking
3. **Message queue operations use async patterns**: GetMessageAsync properly yields during waits
4. **Main execution loop remains responsive**: No single operation can monopolize CPU time

### Integration Points

```
Main Execution Loop (RunNormalAsync)
    ↓
    ├─→ CPU SingleStep execution
    │   ├─→ Import calls (Win32 APIs)
    │   │   ├─→ SendMessageA
    │   │   │   └─→ CallWindowProcedure (yields every 10K steps)
    │   │   ├─→ Sleep
    │   │   │   └─→ ThreadScheduler.SetThreadWaiting
    │   │   └─→ WaitForSingleObject
    │   │       └─→ ThreadScheduler.SetThreadWaiting
    │   └─→ COM vtable calls
    │       └─→ Non-blocking handlers
    ↓
    ├─→ ThreadScheduler.ShouldContextSwitch
    │   └─→ Context switch if quantum expired
    ↓
    ├─→ ThreadScheduler.ProcessWaitTimeouts
    │   └─→ Wake threads whose timeouts expired
    └─→ await Task.Delay(1) if no runnable threads
```

## Migration Guide

### For Existing Code

1. **No changes required**: All Win32 APIs maintain backward compatibility
2. **GetMessageBlocking still works**: Internally uses async implementation
3. **Transparent improvements**: Existing code benefits from cooperative threading automatically

### For New Code

1. **Prefer GetMessageAsync**: Use async version where possible
2. **Understand yielding**: Long computations should periodically check ThreadScheduler
3. **Use cooperative primitives**: Prefer ThreadScheduler methods over blocking operations

## Performance Impact

### Improvements
- **Reduced blocking**: Long-running WndProcs no longer monopolize CPU
- **Better responsiveness**: Other threads get CPU time during waits
- **Timeout handling**: Proper timeout tracking and wakeup
- **Fewer busy-waits**: Sleep and WaitForSingleObject properly yield

### Tradeoffs
- **Slight overhead**: Periodic context switch checks (every 10K instructions)
- **Cooperative nature**: Threads must explicitly yield (can't be preempted mid-instruction)
- **Timeout approximations**: Sleep/Wait timeouts are approximate due to quantum-based scheduling

## Testing Recommendations

1. **Test with ign_teas.exe**: Verify no more "WndProc returned 0xDEADBEEF" errors
2. **Test multi-threaded apps**: Ensure proper thread scheduling and no deadlocks
3. **Test message-heavy apps**: Verify message queue operations don't block
4. **Test synchronization**: Confirm mutexes, events, and semaphores work correctly
5. **Monitor performance**: Check for any regressions in single-threaded scenarios

## Future Enhancements

1. **True async execution**: Consider making CallWindowProcedure truly async by saving/resuming state
2. **Preemptive scheduling**: Add optional preemptive mode for even better responsiveness
3. **Async COM methods**: Support for async COM interfaces if needed
4. **Wait optimization**: Smarter polling/event-driven wait handling
5. **Profiling integration**: Track time spent in yielding vs execution

## Related Files

- `Win32Emu/Win32/Modules/User32Module.cs` - Window message processing
- `Win32Emu/Win32/Modules/Kernel32Module.cs` - Threading primitives
- `Win32Emu/Win32/Modules/Ole32Module.cs` - COM initialization
- `Win32Emu/Win32/ProcessEnvironment.cs` - Message queue and environment
- `Win32Emu/Win32/COM/ComVtableDispatcher.cs` - COM method dispatch
- `Win32Emu/Threading/ThreadScheduler.cs` - Thread scheduling and context switching
- `Win32Emu/Threading/EmulatedThread.cs` - Thread state and context
- `Win32Emu/Emulator.cs` - Main execution loop

## Summary

These changes transform Win32Emu from using blocking synchronous operations to a fully cooperative multitasking model with async/await patterns. This fixes issues where applications get stuck during window procedure execution, message processing, or synchronization operations, while maintaining full backward compatibility with existing code.
