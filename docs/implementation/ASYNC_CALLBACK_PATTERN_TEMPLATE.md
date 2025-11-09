# Async Callback Pattern Template

This template provides a starting point for implementing async callback execution in Win32 modules.

## Prerequisites

Add these constants to your module class:

```csharp
// Constants for async callback execution
private const int INFINITE_LOOP_CHECK_INTERVAL = 100000; // Check for infinite loops every 100K steps
private const int STUCK_COUNTER_THRESHOLD = 3; // Number of consecutive checks at same EIP to consider it stuck
private const int CANCELLATION_CHECK_INTERVAL = 1000; // Check cancellation token every 1K steps
private const uint MINIMUM_VALID_EIP = 0x00001000; // Minimum valid instruction pointer (4KB)
```

## Template: Async Callback Method

Replace placeholders with your specific implementation details:

```csharp
/// <summary>
/// Async version of [YourCallbackName] that eliminates the need for STACK_SAFETY_MARGIN.
/// Uses async/await pattern for clean separation of host (C#) and guest (x86) execution stacks.
/// </summary>
/// <param name="callbackAddress">Address of the callback function in emulated memory</param>
/// <param name="param1">First callback parameter</param>
/// <param name="param2">Second callback parameter</param>
/// <param name="cancellationToken">Optional cancellation token</param>
/// <returns>Return value from the callback (typically from EAX register)</returns>
private async Task<uint> [YourCallbackName]Async(
	uint callbackAddress,
	uint param1,
	uint param2,
	// Add more parameters as needed
	CancellationToken cancellationToken = default)
{
	if (_cpu == null || _memory == null)
	{
		_logger.LogWarning("[Module] [YourCallbackName]Async: CPU or Memory not available");
		return 0; // or appropriate default
	}

	_logger.LogInformation("[Module] [YourCallbackName]Async: Calling 0x{CallbackAddress:X8}", callbackAddress);

	// Validate callback address
	if (callbackAddress == 0)
	{
		_logger.LogWarning("[Module] [YourCallbackName]Async: Callback address is NULL (0x00000000), aborting");
		return 0; // or appropriate default
	}

	// Save current CPU state
	var savedEip = _cpu.GetEip();
	var savedEsp = _cpu.GetRegister("ESP");
	var savedEbp = _cpu.GetRegister("EBP");

	// Define return address marker
	const uint RETURN_ADDRESS = 0xDEADBEEF;

	// Set up stack for stdcall convention (parameters pushed right-to-left)
	// NOTE: No STACK_SAFETY_MARGIN needed! The async architecture provides clean stack separation.
	var esp = savedEsp;

	// Push return address first
	esp -= 4;
	_memory.Write32(esp, RETURN_ADDRESS);

	// Push parameters (right-to-left for stdcall)
	// Last parameter first (rightmost in declaration)
	esp -= 4;
	_memory.Write32(esp, param2);

	// First parameter last (leftmost in declaration)
	esp -= 4;
	_memory.Write32(esp, param1);

	// Update CPU registers
	_cpu.SetRegister("ESP", esp);
	_cpu.SetEip(callbackAddress);

	// Execute until we hit the return address with cancellation support
	const int YIELD_INTERVAL = 10000;
	var steps = 0;
	var executionSuccessful = true;
	var lastCheckEip = _cpu.GetEip();
	var stuckCounter = 0;

	try
	{
		// Execute in unbounded loop with safeguards:
		// 1. Return detection: Break when EIP hits RETURN_ADDRESS marker
		// 2. Cancellation: Regular checks for cancellation requests
		// 3. Progress tracking: Detect stuck execution by monitoring EIP changes
		// 4. Yielding: Periodic Task.Yield() allows other async operations to proceed
		while (true)
		{
			// Check for cancellation at regular intervals
			if (steps % CANCELLATION_CHECK_INTERVAL == 0)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					_logger.LogInformation("[Module] [YourCallbackName]Async: Cancellation requested at step {Steps}", steps);
					executionSuccessful = false;
					break;
				}

				// Yield to allow other async operations to proceed
				await Task.Yield();
			}

			var eip = _cpu.GetEip();

			// Check if we've returned to our marker address
			if (eip == RETURN_ADDRESS)
			{
				break;
			}

			// Check for invalid EIP (NULL pointer execution)
			if (eip == 0x00000000)
			{
				_logger.LogWarning("[Module] [YourCallbackName]Async: Execution jumped to NULL address (0x00000000), likely due to invalid function pointer - aborting");
				executionSuccessful = false;
				break;
			}

			// Check for other invalid low addresses
			if (eip < MINIMUM_VALID_EIP && eip != RETURN_ADDRESS)
			{
				_logger.LogError("[Module] [YourCallbackName]Async: Execution jumped to invalid low address 0x{Eip:X8}", eip);
				executionSuccessful = false;
				break;
			}

			// Detect potential infinite loops
			if (steps > 0 && steps % INFINITE_LOOP_CHECK_INTERVAL == 0)
			{
				var currentEip = _cpu.GetEip();
				if (currentEip == lastCheckEip)
				{
					stuckCounter++;
					if (stuckCounter >= STUCK_COUNTER_THRESHOLD)
					{
						_logger.LogWarning("[Module] [YourCallbackName]Async: Detected infinite loop at EIP=0x{Eip:X8} after {Count} checks, aborting",
							currentEip, stuckCounter);
						executionSuccessful = false;
						break;
					}
				}
				else
				{
					stuckCounter = 0;
					lastCheckEip = currentEip;
				}
			}

			// Execute one instruction
			_cpu.SingleStep(_memory);
			steps++;

			// Periodically yield for cooperative multitasking
			if (steps % YIELD_INTERVAL == 0)
			{
				await Task.Yield();
			}
		}
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "[Module] [YourCallbackName]Async: Exception during execution: {ExMessage}", ex.Message);
		executionSuccessful = false;
	}

	// Get return value from EAX, but only if execution was successful
	var returnValue = executionSuccessful ? _cpu.GetRegister("EAX") : 0u;

	// Restore CPU state
	_cpu.SetEip(savedEip);
	_cpu.SetRegister("ESP", savedEsp);
	_cpu.SetRegister("EBP", savedEbp);

	_logger.LogInformation("[Module] [YourCallbackName]Async: Completed with return value 0x{ReturnValue:X8}", returnValue);

	return returnValue;
}
```

## Template: Public API with Sync Wrapper

```csharp
/// <summary>
/// [Original Win32 API documentation]
/// </summary>
[DllModuleExport(N)]
private uint [YourApiName](uint param1, uint param2)
{
	_logger.LogInformation("[Module] [YourApiName](param1=0x{Param1:X8}, param2=0x{Param2:X8})", param1, param2);

	// Validate parameters
	if (param1 == 0)
	{
		_logger.LogInformation("[Module] [YourApiName]: Invalid parameter");
		return ERROR_INVALID_PARAMETER;
	}

	// Use async implementation internally for better stack separation
	return [YourApiName]Async(param1, param2).GetAwaiter().GetResult();
}

/// <summary>
/// Async implementation of [YourApiName].
/// </summary>
private async Task<uint> [YourApiName]Async(
	uint param1, 
	uint param2, 
	CancellationToken cancellationToken = default)
{
	// Your implementation that calls the async callback
	var result = await [YourCallbackName]Async(
		callbackAddress, 
		callbackParam1, 
		callbackParam2,
		cancellationToken).ConfigureAwait(false);

	// Process result and return
	return result;
}
```

## Checklist for Implementation

- [ ] Add async callback constants to module class
- [ ] Create async callback method following template
- [ ] Validate callback address (check for NULL)
- [ ] Save CPU state before execution
- [ ] Set up stack WITHOUT STACK_SAFETY_MARGIN
- [ ] Push return address marker (0xDEADBEEF)
- [ ] Push parameters in correct order (right-to-left for stdcall)
- [ ] Implement execution loop with safeguards:
  - [ ] Cancellation checking
  - [ ] Return address detection
  - [ ] NULL pointer detection
  - [ ] Invalid EIP detection
  - [ ] Infinite loop detection
  - [ ] Periodic yielding
- [ ] Get return value from EAX
- [ ] Restore CPU state
- [ ] Create sync wrapper for exported API
- [ ] Create async version of public API
- [ ] Update tests (if any)
- [ ] Build and verify no compilation errors
- [ ] Run tests to ensure backward compatibility
- [ ] Document in ASYNC_CALLBACK_MIGRATION.md

## Common Pitfalls to Avoid

1. **Wrong parameter order** - Remember stdcall pushes right-to-left
2. **Forgetting to restore CPU state** - Always restore in finally block or at end
3. **Using wrong return address** - Use 0xDEADBEEF consistently
4. **Missing validation** - Always check for NULL callback address
5. **Incorrect EAX reading** - Only read EAX if executionSuccessful is true
6. **Missing ConfigureAwait(false)** - Use when calling async methods in library code
7. **Wrong calling convention** - Most Win32 callbacks use stdcall (caller cleans stack)

## Testing

After implementation:

```bash
# Build the project
dotnet build --configuration Release

# Run relevant tests
dotnet test [YourTestProject] --configuration Release --no-build

# Verify backward compatibility
dotnet test --configuration Release --no-build
```

## References

- Pattern Documentation: `docs/implementation/ASYNC_CALLBACK_MIGRATION.md`
- Working Examples:
  - `Win32Emu/Win32/Modules/User32Module.cs` - CallWindowProcedureAsync
  - `Win32Emu/Win32/Modules/DSoundModule.cs` - CallEnumerationCallbackAsync
