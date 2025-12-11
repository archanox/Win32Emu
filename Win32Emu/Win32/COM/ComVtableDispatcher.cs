using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.COM;

/// <summary>
/// Dispatcher for COM vtable method calls
/// Handles calls to COM interface methods in the COM vtable memory range
/// 
/// Threading Model:
/// COM method invocations are synchronous but designed to be non-blocking.
/// All handlers should complete quickly without long-running operations.
/// For operations that need to wait or perform I/O, handlers should:
/// 1. Return immediately with appropriate status codes
/// 2. Use cooperative threading patterns (e.g., ThreadScheduler.SetThreadWaiting)
/// 3. Avoid blocking the emulation loop
/// </summary>
public class ComVtableDispatcher
{
	private readonly ProcessEnvironment _env;
	private readonly ILogger _logger;
	
	// Map of vtable stub addresses to synchronous handler functions
	private readonly Dictionary<uint, Func<ICpu, VirtualMemory, uint>> _vtableHandlers = new();
	
	// Map of vtable stub addresses to asynchronous handler functions
	private readonly Dictionary<uint, Func<ICpu, VirtualMemory, Task<uint>>> _vtableAsyncHandlers = new();
	
	// Map of vtable stub addresses to method names for debugging
	private readonly Dictionary<uint, string> _vtableMethodNames = new();
	
	// Map of vtable stub addresses to argument byte counts (for stdcall stack cleanup)
	private readonly Dictionary<uint, int> _vtableArgBytes = new();
	
	// Track allocated COM objects
	private readonly Dictionary<uint, ComObjectInfo> _comObjects = new();
	private uint _nextObjectId = 1;
	
	public ComVtableDispatcher(ProcessEnvironment env, ILogger? logger = null)
	{
		_env = env;
		_logger = logger ?? NullLogger.Instance;
	}
	
	/// <summary>
	/// Check if an address is in the COM vtable range
	/// </summary>
	public bool IsComVtableAddress(uint address)
	{
		return MemoryRegions.IsInComVtableRange(address);
	}
	
	/// <summary>
	/// Try to invoke a COM vtable method
	/// </summary>
	public bool TryInvoke(uint address, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		return TryInvoke(address, cpu, memory, out returnValue, out _);
	}
	
	/// <summary>
	/// Try to invoke a COM vtable method and return argument byte count for stack cleanup
	/// </summary>
	public bool TryInvoke(uint address, ICpu cpu, VirtualMemory memory, out uint returnValue, out int argBytes)
	{
		returnValue = 0;
		argBytes = 0;
		
		if (!IsComVtableAddress(address))
		{
			return false;
		}
		
		// Try to get the method name for better logging
		var methodName = _vtableMethodNames.GetValueOrDefault(address, "Unknown");
		
		if (_vtableHandlers.TryGetValue(address, out var handler))
		{
			// Capture register state before method invocation for debugging
			var eipBefore = cpu.GetEip();
			var espBefore = cpu.GetRegister("ESP");
			var ebpBefore = cpu.GetRegister("EBP");
			var ebxBefore = cpu.GetRegister("EBX");
			var esiBefore = cpu.GetRegister("ESI");
			var ediBefore = cpu.GetRegister("EDI");
			
			_logger.LogInformation("[COM] Invoking vtable method: {MethodName} at address 0x{Address:X8}", methodName, address);
			_logger.LogDebug("[COM] Register state before {MethodName}: EIP=0x{Eip:X8}, ESP=0x{Esp:X8}, EBP=0x{Ebp:X8}, EBX=0x{Ebx:X8}, ESI=0x{Esi:X8}, EDI=0x{Edi:X8}", 
				methodName, eipBefore, espBefore, ebpBefore, ebxBefore, esiBefore, ediBefore);
			
			// Invoke the method with exception handling
			try
			{
				returnValue = handler(cpu, memory);
				_logger.LogDebug("[COM] {MethodName} handler completed successfully", methodName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[COM] Exception in {MethodName} handler", methodName);
				returnValue = 0x80004005; // E_FAIL
				// Continue with logging and cleanup
			}
			
			// Capture register state after method invocation
			var eipAfter = cpu.GetEip();
			var espAfter = cpu.GetRegister("ESP");
			var ebpAfter = cpu.GetRegister("EBP");
			var ebxAfter = cpu.GetRegister("EBX");
			var esiAfter = cpu.GetRegister("ESI");
			var ediAfter = cpu.GetRegister("EDI");
			
			// Get argument byte count for stack cleanup (0 if not registered)
			argBytes = _vtableArgBytes.GetValueOrDefault(address, 0);
			
			// Log return value and register state after
			_logger.LogInformation("[COM] {MethodName} returned 0x{ReturnValue:X8} (argBytes={ArgBytes})", methodName, returnValue, argBytes);
			_logger.LogDebug("[COM] Register state after {MethodName}: EIP=0x{Eip:X8}, ESP=0x{Esp:X8}, EBP=0x{Ebp:X8}, EBX=0x{Ebx:X8}, ESI=0x{Esi:X8}, EDI=0x{Edi:X8}", 
				methodName, eipAfter, espAfter, ebpAfter, ebxAfter, esiAfter, ediAfter);
			
			// Verify callee-saved registers are preserved (EBX, ESI, EDI, EBP)
			if (ebxBefore != ebxAfter)
			{
				_logger.LogWarning("[COM] {MethodName} corrupted EBX: before=0x{Before:X8}, after=0x{After:X8}", methodName, ebxBefore, ebxAfter);
			}
			if (esiBefore != esiAfter)
			{
				_logger.LogWarning("[COM] {MethodName} corrupted ESI: before=0x{Before:X8}, after=0x{After:X8}", methodName, esiBefore, esiAfter);
			}
			if (ediBefore != ediAfter)
			{
				_logger.LogWarning("[COM] {MethodName} corrupted EDI: before=0x{Before:X8}, after=0x{After:X8}", methodName, ediBefore, ediAfter);
			}
			if (ebpBefore != ebpAfter)
			{
				_logger.LogWarning("[COM] {MethodName} corrupted EBP: before=0x{Before:X8}, after=0x{After:X8}", methodName, ebpBefore, ebpAfter);
			}
			
			// Verify ESP is unchanged by the handler
			// NOTE: COM methods are invoked through INT3 traps, not regular CALL instructions.
			// The handler itself should NOT modify ESP - all stack cleanup is handled by
			// InvokeWithRegisterPreservation in the emulator after the handler returns.
			// For normal stdcall functions, the callee cleans the stack, but INT3 handlers don't follow that convention.
			if (espAfter != espBefore)
			{
				_logger.LogWarning("[COM] {MethodName} unexpectedly modified ESP: before=0x{Before:X8}, after=0x{After:X8}, delta={Delta} bytes", 
					methodName, espBefore, espAfter, (int)espAfter - (int)espBefore);
			}
			
			return true;
		}
		
		_logger.LogWarning("[COM] Unhandled COM vtable call at 0x{Address:X8} (method: {MethodName})", address, methodName);
		return false;
	}
	
	/// <summary>
	/// Try to invoke a COM vtable method asynchronously
	/// </summary>
	public async Task<(bool success, uint returnValue, int argBytes)> TryInvokeAsync(
		uint address, 
		ICpu cpu, 
		VirtualMemory memory,
		CancellationToken cancellationToken = default)
	{
		if (!IsComVtableAddress(address))
		{
			return (false, 0, 0);
		}
		
		// Try to get the method name for better logging
		var methodName = _vtableMethodNames.GetValueOrDefault(address, "Unknown");
		
		// Check for async handler first
		if (_vtableAsyncHandlers.TryGetValue(address, out var asyncHandler))
		{
			// Capture register state before method invocation for debugging
			var eipBefore = cpu.GetEip();
			var espBefore = cpu.GetRegister("ESP");
			var ebpBefore = cpu.GetRegister("EBP");
			var ebxBefore = cpu.GetRegister("EBX");
			var esiBefore = cpu.GetRegister("ESI");
			var ediBefore = cpu.GetRegister("EDI");
			
			_logger.LogInformation("[COM] Invoking async vtable method: {MethodName} at address 0x{Address:X8}", methodName, address);
			_logger.LogDebug("[COM] Register state before {MethodName}: EIP=0x{Eip:X8}, ESP=0x{Esp:X8}, EBP=0x{Ebp:X8}, EBX=0x{Ebx:X8}, ESI=0x{Esi:X8}, EDI=0x{Edi:X8}", 
				methodName, eipBefore, espBefore, ebpBefore, ebxBefore, esiBefore, ediBefore);
			
			uint returnValue;
			try
			{
				// Save CPU state before async operation
				var cpuState = CpuHelpers.SuspendExecution(cpu);
				
				// Check for cancellation before invoking handler
				cancellationToken.ThrowIfCancellationRequested();
				
				// Invoke async handler
				returnValue = await asyncHandler(cpu, memory);
				
				// Restore CPU state after async operation
				CpuHelpers.ResumeExecution(cpu, cpuState);
				
				_logger.LogDebug("[COM] {MethodName} async handler completed successfully", methodName);
			}
			catch (Exception ex)
			{
				// Rethrow critical exceptions
				if (ex is OutOfMemoryException || ex is StackOverflowException || ex is ThreadAbortException)
				{
					throw;
				}

				_logger.LogError(ex, "[COM] Exception in {MethodName} async handler", methodName);
				returnValue = 0x80004005; // E_FAIL
			}
			
			// Capture register state after method invocation
			var eipAfter = cpu.GetEip();
			var espAfter = cpu.GetRegister("ESP");
			var ebpAfter = cpu.GetRegister("EBP");
			var ebxAfter = cpu.GetRegister("EBX");
			var esiAfter = cpu.GetRegister("ESI");
			var ediAfter = cpu.GetRegister("EDI");
			
			// Get argument byte count for stack cleanup
			var argBytes = _vtableArgBytes.GetValueOrDefault(address, 0);
			
			// Log return value and register state after
			_logger.LogInformation("[COM] {MethodName} returned 0x{ReturnValue:X8} (argBytes={ArgBytes})", methodName, returnValue, argBytes);
			_logger.LogDebug("[COM] Register state after {MethodName}: EIP=0x{Eip:X8}, ESP=0x{Esp:X8}, EBP=0x{Ebp:X8}, EBX=0x{Ebx:X8}, ESI=0x{Esi:X8}, EDI=0x{Edi:X8}", 
				methodName, eipAfter, espAfter, ebpAfter, ebxAfter, esiAfter, ediAfter);
			
			// Verify callee-saved registers are preserved (EBX, ESI, EDI, EBP)
			if (ebxBefore != ebxAfter)
			{
				_logger.LogWarning("[COM] {MethodName} corrupted EBX: before=0x{Before:X8}, after=0x{After:X8}", methodName, ebxBefore, ebxAfter);
			}
			if (esiBefore != esiAfter)
			{
				_logger.LogWarning("[COM] {MethodName} corrupted ESI: before=0x{Before:X8}, after=0x{After:X8}", methodName, esiBefore, esiAfter);
			}
			if (ediBefore != ediAfter)
			{
				_logger.LogWarning("[COM] {MethodName} corrupted EDI: before=0x{Before:X8}, after=0x{After:X8}", methodName, ediBefore, ediAfter);
			}
			if (ebpBefore != ebpAfter)
			{
				_logger.LogWarning("[COM] {MethodName} corrupted EBP: before=0x{Before:X8}, after=0x{After:X8}", methodName, ebpBefore, ebpAfter);
			}
			
			// Verify ESP is unchanged by the handler
			if (espAfter != espBefore)
			{
				_logger.LogWarning("[COM] {MethodName} unexpectedly modified ESP: before=0x{Before:X8}, after=0x{After:X8}, delta={Delta} bytes", 
					methodName, espBefore, espAfter, (int)espAfter - (int)espBefore);
			}
			
			return (true, returnValue, argBytes);
		}
		
		// Fall back to synchronous handler if no async handler exists
		if (_vtableHandlers.TryGetValue(address, out var syncHandler))
		{
			_logger.LogInformation("[COM] Invoking vtable method (async fallback to sync): {MethodName} at address 0x{Address:X8}", methodName, address);
			
			var cpuState = CpuHelpers.SuspendExecution(cpu);
			uint returnValue;
			try
			{
				returnValue = syncHandler(cpu, memory);
				_logger.LogDebug("[COM] {MethodName} handler completed successfully", methodName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[COM] Exception in {MethodName} handler (async fallback)", methodName);
				returnValue = 0x80004005; // E_FAIL
				// Continue with logging and cleanup
			}
			finally
			{
				CpuHelpers.ResumeExecution(cpu, cpuState);
			}
			
			var argBytes = _vtableArgBytes.GetValueOrDefault(address, 0);
			_logger.LogInformation("[COM] {MethodName} returned 0x{ReturnValue:X8} (argBytes={ArgBytes})", methodName, returnValue, argBytes);
			return (true, returnValue, argBytes);
		}
		
		_logger.LogWarning("[COM] Unhandled COM vtable call at 0x{Address:X8} (method: {MethodName})", address, methodName);
		return (false, 0, 0);
	}
	
	/// <summary>
	/// Internal helper to create a COM object with a vtable, using a generic handler registration action
	/// </summary>
	/// <remarks>
	/// IMPORTANT: The methods collection MUST maintain insertion order to match the COM interface vtable layout.
	/// Methods must be provided in the exact order defined by the COM interface specification 
	/// (e.g., QueryInterface, AddRef, Release must be first 3 methods).
	/// </remarks>
	private uint CreateComObjectInternal<TMethodInfo>(
		string interfaceName,
		IEnumerable<KeyValuePair<string, TMethodInfo>> methods,
		Func<TMethodInfo, Func<ICpu, VirtualMemory, uint>?> getSyncHandler,
		Func<TMethodInfo, Func<ICpu, VirtualMemory, Task<uint>>?> getAsyncHandler,
		Func<TMethodInfo, int> getArgBytes,
		bool isAsync)
	{
		var objectId = _nextObjectId++;
		
		// Allocate memory for the COM object structure
		// COM object layout: [vtable pointer][object data...]
		var objectAddr = _env.SimpleAlloc(8); // 4 bytes for vtable ptr + 4 bytes for object data
		
		// Zero the COM object memory to ensure clean state
		_env.MemZero(objectAddr, 8);
		
		// Convert to list to get count and allow indexed access
		var methodsList = methods.ToList();
		
		// Allocate memory for the vtable
		var vtableSize = (uint)(methodsList.Count * 4); // 4 bytes per method pointer
		var vtableAddr = _env.SimpleAlloc(vtableSize);
		
		// CRITICAL: Zero the vtable memory to prevent garbage data in unused slots
		// SimpleAlloc doesn't zero memory, so we must do it explicitly
		_env.MemZero(vtableAddr, vtableSize);
		
		// Write vtable pointer to object
		_env.MemWrite32(objectAddr, vtableAddr);
		
		// Create vtable stubs and write function pointers
		// CRITICAL: Methods are iterated in the exact order provided
		// This ensures vtable methods are at the correct offsets as defined by the COM interface
		var stubAddr = MemoryRegions.ComVtableBase + (objectId * 0x1000); // Each object gets 4KB of address space
		uint methodIndex = 0;
		
		foreach (var kvp in methodsList)
		{
			var methodName = kvp.Key;
			var methodInfo = kvp.Value;
			
			// Calculate stub address for this method
			var methodStubAddr = stubAddr + (methodIndex * 0x10); // 16 bytes per stub
			
			// Write function pointer to vtable
			_env.MemWrite32(vtableAddr + (methodIndex * 4), methodStubAddr);
			
			// Create INT3 stub at the method address
			var stub = new byte[] 
			{ 
				0xCC, // INT3 - breakpoint instruction
				0x90, 0x90, 0x90, // NOP padding
				0x90, 0x90, 0x90, 0x90,
				0x90, 0x90, 0x90, 0x90,
				0x90, 0x90, 0x90, 0x90
			};
			_env.MemWriteBytes(methodStubAddr, stub);
			
			// Register the handler (sync or async)
			var syncHandler = getSyncHandler(methodInfo);
			if (syncHandler != null)
			{
				_vtableHandlers[methodStubAddr] = syncHandler;
			}
			
			var asyncHandler = getAsyncHandler(methodInfo);
			if (asyncHandler != null)
			{
				_vtableAsyncHandlers[methodStubAddr] = asyncHandler;
			}
			
			// Register the method name for debugging
			_vtableMethodNames[methodStubAddr] = $"{interfaceName}::{methodName}";
			
			// Register argument byte count for stack cleanup
			_vtableArgBytes[methodStubAddr] = getArgBytes(methodInfo);
			
			var asyncLabel = isAsync ? "async, " : "";
			_logger.LogDebug("[COM] {InterfaceName}::{MethodName} -> 0x{MethodStubAddr:X8} ({AsyncLabel}argBytes={ArgBytes})", 
				interfaceName, methodName, methodStubAddr, asyncLabel, getArgBytes(methodInfo));
			
			methodIndex++;
		}
		
		var objInfo = new ComObjectInfo
		{
			ObjectId = objectId,
			ObjectAddress = objectAddr,
			VtableAddress = vtableAddr,
			InterfaceName = interfaceName
		};
		
		_comObjects[objectAddr] = objInfo;
		
		var asyncPrefix = isAsync ? "async " : "";
		_logger.LogInformation("[COM] Created {AsyncPrefix}{InterfaceName} object at 0x{ObjectAddr:X8} (vtable at 0x{VtableAddr:X8})", 
			asyncPrefix, interfaceName, objectAddr, vtableAddr);
		
		// Verify vtable contents immediately after creation
		_logger.LogInformation("[COM] Verifying vtable at 0x{VtableAddr:X8} with {Count} methods:", vtableAddr, methodsList.Count);
		for (uint i = 0; i < methodsList.Count && i < 15; i++) // Log first 15 methods
		{
			var entryAddr = vtableAddr + (i * 4);
			var methodStubAddr = _env.MemRead32(entryAddr);
			var methodName = methodsList[(int)i].Key;
			_logger.LogInformation("[COM]   [{Index:D2}] vtable[0x{EntryAddr:X8}] = 0x{StubAddr:X8} ({MethodName})", 
				i, entryAddr, methodStubAddr, methodName);
		}
		
		return objectAddr;
	}
	
	/// <summary>
	/// Create a COM object with a vtable, with argument byte metadata for proper stack cleanup
	/// </summary>
	public uint CreateComObject(string interfaceName, Dictionary<string, ComMethodInfo> methods)
	{
		return CreateComObjectInternal(
			interfaceName,
			methods,
			info => info.Handler,
			info => null,
			info => info.ArgBytes,
			isAsync: false);
	}
	
	/// <summary>
	/// Create a COM object with a vtable using an ordered list to ensure correct method order.
	/// This is the REQUIRED way to create COM objects to ensure vtable methods are in correct order.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: This method MUST be used when vtable method order matters, which is ALWAYS for COM interfaces.
	/// COM interfaces require methods to be at specific offsets in the vtable. Incorrect ordering will cause
	/// crashes when programs call methods at the wrong offsets.
	/// 
	/// The old CreateComObject(Dictionary) method should NOT be used for new code as Dictionary iteration
	/// order cannot be relied upon for vtable construction.
	/// 
	/// Example:
	/// <code>
	/// var methods = new List&lt;KeyValuePair&lt;string, ComMethodInfo&gt;&gt;
	/// {
	///     new("QueryInterface", ...),  // Must be first (offset 0x00)
	///     new("AddRef", ...),          // Must be second (offset 0x04)
	///     new("Release", ...),         // Must be third (offset 0x08)
	///     new("Flip", ...),           // At specific offset (e.g., 0x2C for IDirectDrawSurface)
	///     // ... other methods in exact COM interface order
	/// };
	/// var comAddr = dispatcher.CreateComObjectOrdered("IDirectDrawSurface", methods);
	/// </code>
	/// </remarks>
	/// <param name="interfaceName">Name of the COM interface</param>
	/// <param name="methods">Ordered list of method name/info pairs in exact COM interface order</param>
	public uint CreateComObjectOrdered(string interfaceName, List<KeyValuePair<string, ComMethodInfo>> methods)
	{
		// Pass the list directly - no conversion to dictionary to preserve order
		return CreateComObjectInternal(
			interfaceName,
			methods,
			info => info.Handler,
			info => null,
			info => info.ArgBytes,
			isAsync: false);
	}
	
	/// <summary>
	/// Helper to create ComMethodInfo from a delegate type.
	/// Automatically calculates argBytes from the delegate signature.
	/// </summary>
	/// <typeparam name="TDelegate">Delegate type with [UnmanagedFunctionPointer(CallingConvention.StdCall)]</typeparam>
	/// <param name="handler">Handler function that implements the delegate logic</param>
	/// <returns>ComMethodInfo with automatically calculated argBytes</returns>
	public static ComMethodInfo FromDelegate<TDelegate>(Func<ICpu, VirtualMemory, uint> handler) where TDelegate : Delegate
	{
		var delegateType = typeof(TDelegate);
		
		// Verify the delegate has the correct attribute
		if (!ComDelegateHelper.HasStdCallConvention(delegateType))
		{
			throw new InvalidOperationException($"Delegate type {delegateType.Name} must have [UnmanagedFunctionPointer(CallingConvention.StdCall)] attribute");
		}
		
		// Calculate argument bytes from delegate signature
		var argBytes = ComDelegateHelper.GetArgBytes(delegateType);
		
		return new ComMethodInfo(handler, argBytes);
	}
	
	/// <summary>
	/// Helper to create ComAsyncMethodInfo from a delegate type for async COM method handlers.
	/// Automatically calculates argBytes from the delegate signature.
	/// This enables proper async/await throughout the COM call chain for operations that need to yield
	/// to the browser event loop in WASM or perform other async operations.
	/// </summary>
	/// <typeparam name="TDelegate">Delegate type with [UnmanagedFunctionPointer(CallingConvention.StdCall)]</typeparam>
	/// <param name="asyncHandler">Async handler function that implements the delegate logic</param>
	/// <returns>ComAsyncMethodInfo with automatically calculated argBytes</returns>
	public static ComAsyncMethodInfo FromAsyncDelegate<TDelegate>(Func<ICpu, VirtualMemory, Task<uint>> asyncHandler) where TDelegate : Delegate
	{
		if (asyncHandler == null)
		{
			throw new ArgumentNullException(nameof(asyncHandler));
		}
		
		var delegateType = typeof(TDelegate);
		
		// Verify the delegate has the correct attribute
		if (!ComDelegateHelper.HasStdCallConvention(delegateType))
		{
			throw new InvalidOperationException($"Delegate type {delegateType.Name} must have [UnmanagedFunctionPointer(CallingConvention.StdCall)] attribute");
		}
		
		// Calculate argument bytes from delegate signature
		var argBytes = ComDelegateHelper.GetArgBytes(delegateType);
		
		return new ComAsyncMethodInfo(asyncHandler, argBytes);
	}
	
	/// <summary>
	/// Create a COM object with async vtable handlers
	/// </summary>
	public uint CreateComObjectAsync(string interfaceName, Dictionary<string, ComAsyncMethodInfo> methods)
	{
		return CreateComObjectInternal(
			interfaceName,
			methods,
			info => null,
			info => info.AsyncHandler,
			info => info.ArgBytes,
			isAsync: true);
	}
	
	/// <summary>
	/// Create a COM object with async vtable handlers using an ordered list to ensure correct method order.
	/// This is the REQUIRED way to create async COM objects to ensure vtable methods are in correct order.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: This method MUST be used when vtable method order matters, which is ALWAYS for COM interfaces.
	/// COM interfaces require methods to be at specific offsets in the vtable. Incorrect ordering will cause
	/// crashes when programs call methods at the wrong offsets.
	/// 
	/// The async variant allows proper async/await throughout the COM call chain for operations that need to
	/// yield to the browser event loop in WASM or perform other async operations.
	/// 
	/// Example:
	/// <code>
	/// var methods = new List&lt;KeyValuePair&lt;string, ComAsyncMethodInfo&gt;&gt;
	/// {
	///     new("QueryInterface", ComVtableDispatcher.FromAsyncDelegate&lt;IDirectDraw.QueryInterface&gt;(async (cpu, mem) => await ComQueryInterfaceAsync(cpu, mem))),
	///     new("AddRef", ComVtableDispatcher.FromAsyncDelegate&lt;IDirectDraw.AddRef&gt;(async (cpu, mem) => await ComAddRefAsync(cpu, mem))),
	///     new("Release", ComVtableDispatcher.FromAsyncDelegate&lt;IDirectDraw.Release&gt;(async (cpu, mem) => await ComReleaseAsync(cpu, mem))),
	///     new("SetDisplayMode", ComVtableDispatcher.FromAsyncDelegate&lt;IDirectDraw.SetDisplayMode&gt;(async (cpu, mem) => await DDraw_SetDisplayModeAsync(cpu, mem, ddrawHandle))),
	///     // ... other methods in exact COM interface order
	/// };
	/// var comAddr = dispatcher.CreateComObjectAsyncOrdered("IDirectDraw", methods);
	/// </code>
	/// </remarks>
	/// <param name="interfaceName">Name of the COM interface</param>
	/// <param name="methods">Ordered list of method name/async info pairs in exact COM interface order</param>
	public uint CreateComObjectAsyncOrdered(string interfaceName, List<KeyValuePair<string, ComAsyncMethodInfo>> methods)
	{
		// Pass the list directly - no conversion to dictionary to preserve order
		return CreateComObjectInternal(
			interfaceName,
			methods,
			info => null,
			info => info.AsyncHandler,
			info => info.ArgBytes,
			isAsync: true);
	}
	
	private sealed class ComObjectInfo
	{
		public uint ObjectId { get; set; }
		public uint ObjectAddress { get; set; }
		public uint VtableAddress { get; set; }
		public string InterfaceName { get; set; } = string.Empty;
	}
}
