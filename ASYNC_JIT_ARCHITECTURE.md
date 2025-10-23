# Async JIT x86 Emulator Architecture

## Overview

The Win32Emu emulator now supports asynchronous execution through a JIT (Just-In-Time) compiler that translates x86 machine code to .NET CIL (Common Intermediate Language). This enables true async/await patterns throughout the emulation pipeline, particularly for Win32 API calls, message processing, and UI operations.

## Key Components

### 1. IAsyncCpu Interface

The `IAsyncCpu` interface extends `ICpu` with asynchronous execution capabilities:

```csharp
public interface IAsyncCpu : ICpu
{
    Task<CpuStepResult> SingleStepAsync(VirtualMemory mem);
    Task<CpuStepResult> ExecuteBlockAsync(VirtualMemory mem, int maxInstructions = 0);
    bool SupportsJit { get; }
    CpuState SaveState();
    void RestoreState(CpuState state);
}
```

**Key Methods:**
- `SingleStepAsync`: Execute a single instruction asynchronously
- `ExecuteBlockAsync`: Execute a block of instructions (JIT-compiled for performance)
- `SaveState`/`RestoreState`: Suspend and resume CPU state across async boundaries
- `SupportsJit`: Indicates whether the backend uses JIT compilation

### 2. CPU Backends

#### IcedCpu (Interpreter-based)

The existing interpreter-based CPU now implements `IAsyncCpu` for backward compatibility:
- `SupportsJit = false` (pure interpreter)
- Async methods wrap synchronous execution with `Task.FromResult`
- No breaking changes to existing code
- Supports all x86 instructions currently implemented

**Usage:**
```csharp
var cpu = new IcedCpu(memory, logger);
// Both sync and async work
var syncResult = cpu.SingleStep(memory);
var asyncResult = await cpu.SingleStepAsync(memory);
```

#### JitCpu (JIT-based)

New JIT-based CPU that compiles x86 code blocks to .NET CIL:
- `SupportsJit = true` 
- Compiles frequently-executed code blocks for performance
- Native async/await support throughout
- Maintains compiled block cache for reuse

**Usage:**
```csharp
var cpu = new JitCpu(memory, logger);
// Preferred async execution
var result = await cpu.ExecuteBlockAsync(memory, maxInstructions: 100);
```

**Loading with JIT:**
```csharp
emulator.LoadExecutable("game.exe", useJitCpu: true);
```

### 3. Async Dispatcher

The `Win32Dispatcher` now supports asynchronous import calls:

```csharp
// Synchronous (backward compatible)
bool success = dispatcher.TryInvoke(dll, export, cpu, memory, out uint returnValue, out int argBytes);

// Asynchronous (new)
var (success, returnValue, argBytes) = await dispatcher.TryInvokeAsync(dll, export, cpu, memory);
```

This enables Win32 API implementations to use async patterns:
- `await GetMessageAsync()` for non-blocking message queue operations
- `await Task.Delay()` for sleep operations
- Async I/O operations
- Async COM method invocations

### 4. CPU State Management

The `CpuState` class captures complete CPU state for async suspension/resumption:

```csharp
public class CpuState
{
    public uint Eax, Ebx, Ecx, Edx;
    public uint Esi, Edi, Ebp, Esp, Eip, Eflags;
    public double[]? FpuStack;
    public int FpuTop;
    public ushort FpuControlWord, FpuStatusWord;
}
```

**Example - Suspending for async operation:**
```csharp
// Save state before async operation
var savedState = cpu.SaveState();

// Perform async operation (may yield control)
await SomeLongRunningOperation();

// Restore state after resuming
cpu.RestoreState(savedState);
```

## Architecture Diagrams

### Synchronous Execution Flow (Legacy)

```
Main Loop
  ↓
SingleStep (synchronous)
  ↓
Instruction Execution
  ↓
Import Call Detection
  ↓
Win32Dispatcher.TryInvoke (blocks)
  ↓
Win32 API Implementation (blocks)
  ↓
Return to Main Loop
```

### Asynchronous Execution Flow (New)

```
Main Loop (async)
  ↓
await SingleStepAsync
  ↓
Instruction Execution
  ↓
Import Call Detection
  ↓
await Win32Dispatcher.TryInvokeAsync
  ↓
await Win32 API Implementation
  │
  ├─ await GetMessageAsync (yields)
  ├─ await Task.Delay (yields)
  ├─ await FileReadAsync (yields)
  └─ Other async operations
  ↓
Return to Main Loop
```

### JIT Compilation Flow

```
ExecuteBlockAsync
  ↓
Check Compiled Block Cache
  │
  ├─ Cache Hit → Execute Compiled CIL
  │
  └─ Cache Miss
      ↓
    Analyze x86 Block
      ↓
    Emit CIL Instructions
      ↓
    Store in Cache
      ↓
    Execute Compiled CIL
```

## Benefits

### 1. Non-Blocking Execution

**Before (Synchronous):**
```csharp
// Blocks entire emulator
var message = GetMessageBlocking(hwnd, 0, 0, timeout: 100);
```

**After (Asynchronous):**
```csharp
// Yields control, allows other operations
var message = await GetMessageAsync(hwnd, 0, 0, timeout: 100);
```

### 2. Better UI Responsiveness

Async execution prevents the emulator from blocking during:
- Window message processing
- Dialog procedures
- COM method calls
- Long-running Win32 API calls

### 3. Event-Driven Model

Supports true event-driven UI where messages can be dispatched without polling:
```csharp
// Process UI events asynchronously
_env.ProcessAllBackendEvents(); // Non-blocking

// Handle messages as they arrive
var msg = await GetMessageAsync(); // Yields until message available
```

### 4. Performance (JIT)

JIT compilation improves performance for:
- Frequently executed code blocks
- Tight loops
- Performance-critical sections

**Performance comparison:**
- Interpreter: ~1M instructions/sec
- JIT: ~5-10M instructions/sec (estimated)

## Usage Examples

### Example 1: Creating Emulator with JIT

```csharp
var emulator = new Emulator(host, logger);
emulator.LoadExecutable(
    "application.exe",
    debugMode: false,
    useJitCpu: true  // Enable JIT compilation
);

await emulator.RunAsync();
```

### Example 2: Async Import Call

```csharp
// In a Win32 module implementation
[DllModuleExport("USER32.DLL", "GetMessageA")]
public async Task<uint> GetMessageA_Async(ICpu cpu, VirtualMemory memory)
{
    var lpMsg = cpu.GetRegister("ESP") + 4;
    var hwnd = memory.Read32(lpMsg + 4);
    
    // Non-blocking message retrieval
    var message = await _env.GetMessageAsync(hwnd, 0, 0);
    
    if (message != null)
    {
        // Write message to memory
        memory.Write32(lpMsg, message.Hwnd);
        memory.Write32(lpMsg + 4, message.Message);
        // ... etc
        return 1;
    }
    
    return 0;
}
```

### Example 3: State Save/Restore

```csharp
// Before calling async operation
var cpuState = cpu.SaveState();

try
{
    // Async operation that may modify CPU state
    await ProcessWindowProcedureAsync();
}
finally
{
    // Restore original state
    cpu.RestoreState(cpuState);
}
```

## Migration Guide

### For Existing Code

**No changes required!** The async infrastructure is backward compatible:
- `IcedCpu` implements `IAsyncCpu` with synchronous wrappers
- All existing synchronous APIs continue to work
- `ICpu` interface unchanged

### For New Code

1. **Use IAsyncCpu interface:**
   ```csharp
   // Instead of: IcedCpu cpu
   IAsyncCpu cpu = new JitCpu(memory, logger);
   ```

2. **Prefer async methods:**
   ```csharp
   // Instead of: cpu.SingleStep(memory)
   await cpu.SingleStepAsync(memory);
   ```

3. **Use async dispatcher:**
   ```csharp
   // Instead of: dispatcher.TryInvoke(...)
   var (success, retVal, argBytes) = await dispatcher.TryInvokeAsync(...);
   ```

## Implementation Status

### Completed ✅

- [x] `IAsyncCpu` interface definition
- [x] `IcedCpu` async compatibility layer
- [x] `JitCpu` basic infrastructure
- [x] JIT compilation framework (CIL emission)
- [x] `Win32Dispatcher.TryInvokeAsync`
- [x] CPU state save/restore
- [x] Comprehensive unit tests (11 tests)
- [x] Emulator integration (useJitCpu parameter)

### In Progress 🚧

- [ ] Full x86 instruction set in JIT compiler
- [ ] Async-aware Win32 API implementations
- [ ] Message queue full async support
- [ ] Performance optimizations

### Future Enhancements 🔮

- [ ] Adaptive JIT (profile-guided optimization)
- [ ] SIMD instruction support in JIT
- [ ] Multi-threaded JIT compilation
- [ ] JIT compilation statistics/debugging
- [ ] Async COM vtable methods

## Performance Considerations

### When to Use JIT

**Use JIT when:**
- Performance is critical
- Application has tight loops
- Long-running applications
- Async operations are needed

**Use Interpreter when:**
- Debugging (better error messages)
- Single-step debugging required
- Legacy instruction support needed
- Memory constrained

### JIT Compilation Overhead

- **First execution**: Slower (compilation time)
- **Subsequent executions**: Much faster (cached)
- **Memory**: Additional memory for compiled blocks
- **Warmup**: JIT improves over time as cache grows

## Testing

Run async JIT tests:
```bash
dotnet test --filter "FullyQualifiedName~AsyncJitCpuTests"
```

Current test coverage:
- ✅ Interface implementation
- ✅ JIT support detection
- ✅ Async single step
- ✅ Async block execution
- ✅ CPU state save/restore
- ✅ Backward compatibility

## Troubleshooting

### JIT Compilation Fails

**Symptom:** Exception during `ExecuteBlockAsync`

**Solution:** Fall back to interpreter:
```csharp
try
{
    result = await cpu.ExecuteBlockAsync(memory, 100);
}
catch (Exception)
{
    // Fall back to single-step interpretation
    result = await cpu.SingleStepAsync(memory);
}
```

### Async Deadlock

**Symptom:** Emulator hangs during async operation

**Solution:** Ensure proper async/await usage:
```csharp
// ❌ Wrong - blocks
var result = asyncOperation.Result;

// ✅ Correct - yields
var result = await asyncOperation;
```

### State Corruption

**Symptom:** Registers have unexpected values after async call

**Solution:** Verify state save/restore:
```csharp
var state = cpu.SaveState();
await AsyncOperation();
cpu.RestoreState(state); // Don't forget this!
```

## Related Files

- `Win32Emu/Cpu/IAsyncCpu.cs` - Async CPU interface
- `Win32Emu/Cpu/Iced/IcedCpu.cs` - Interpreter with async support
- `Win32Emu/Cpu/Jit/JitCpu.cs` - JIT compiler backend
- `Win32Emu/Win32/Win32Dispatcher.cs` - Async dispatcher
- `Win32Emu/Emulator.cs` - Main emulator with JIT support
- `Win32Emu.Tests.Emulator/AsyncJitCpuTests.cs` - Unit tests

## References

- [.NET Reflection.Emit Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit)
- [Async/Await Best Practices](https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [IcedX86 Decoder](https://github.com/icedland/iced) - x86 instruction decoder used
