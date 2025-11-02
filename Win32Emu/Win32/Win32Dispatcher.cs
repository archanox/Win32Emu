using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Diagnostics;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public class Win32Dispatcher(ILogger logger)
{
	private readonly Dictionary<string, IWin32ModuleUnsafe> _modules = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> _dynamicallyLoadedDlls = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, HashSet<string>> _unknownFunctionCalls = new(StringComparer.OrdinalIgnoreCase);
	private ApiCallTracer? _apiCallTracer;
	
	/// <summary>
	/// Sets the API call tracer for this dispatcher
	/// </summary>
	public void SetApiCallTracer(ApiCallTracer? tracer)
	{
		_apiCallTracer = tracer;
	}

	public void RegisterModule(IWin32ModuleUnsafe module) => _modules[module.Name] = module;

	public void RegisterDynamicallyLoadedDll(string dllName)
	{
		_dynamicallyLoadedDlls.Add(dllName);
		logger.LogInformation("[Dispatcher] Registered dynamically loaded DLL: {DllName}", dllName);
	}

	public bool TryGetModule(string dllName, out IWin32ModuleUnsafe? module)
	{
		return _modules.TryGetValue(dllName, out module);
	}

	public bool TryInvoke(string dll, string export, ICpu cpu, VirtualMemory memory, out uint returnValue, out int stdcallArgBytes)
	{
		returnValue = 0;
		stdcallArgBytes = 0;

		var eip = cpu.GetEip();
		var esp = cpu.GetRegister("ESP");
		byte[]? stackSnippet = null;
		try
		{
			stackSnippet = memory.GetSpan(esp, 16);
		}
		catch
		{
		}

		logger.LogInformation("Dispatching {Dll}!{Export} at EIP=0x{GetEip:X8} ESP=0x{Esp:X8} stack={Unreadable}", dll, export, eip, esp, stackSnippet == null ? "<unreadable>" : BitConverter.ToString(stackSnippet).Replace('-', ' '));

		// Try to invoke with known modules first
		if (_modules.TryGetValue(dll, out var mod))
		{
			if (mod.TryInvokeUnsafe(export, cpu, memory, out var retUnsafe))
			{
				returnValue = retUnsafe;
				cpu.SetRegister("EAX", retUnsafe);

				// Try to get arg bytes from metadata
				if (StdCallMeta.TryGetArgBytes(dll, export, out stdcallArgBytes))
				{
					logger.LogInformation("[Dispatcher] {Dll}!{Export} returned 0x{ReturnValue:X8}, argBytes={StdcallArgBytes}", dll, export, returnValue, stdcallArgBytes);
				}
				else
				{
					// Function is missing [DllModuleExport] attribute
					logger.LogError("[Dispatcher] {Dll}!{Export} is missing [DllModuleExport] attribute - cannot determine stack cleanup bytes", dll, export);
					stdcallArgBytes = 0; // Default to 0, but this may cause stack corruption
					logger.LogInformation("[Dispatcher] {Dll}!{Export} returned 0x{ReturnValue:X8}, argBytes={StdcallArgBytes} (MISSING METADATA)", dll, export, returnValue, stdcallArgBytes);
				}

				// Log to API tracer if enabled
				// TODO(enhancement): Parse parameters from stack using metadata from [DllModuleExport]
				// This would provide detailed parameter values in the trace for better diagnostics.
				// See issue: (create issue to track this enhancement)
				_apiCallTracer?.LogApiCall(
					moduleName: dll,
					functionName: export,
					parameters: null, // Parameters not yet parsed - would require stack walking
					returnValue: returnValue,
					eip: eip);

				return true;
			}

			// Known module but unknown export - log this
			logger.LogError("Unimplemented function in known module: {Dll}!{Export}", dll, export);
			LogUnknownFunctionCall(dll, export);

			// Return success with default behavior
			returnValue = 0;
			stdcallArgBytes = 0; // Default for unknown functions
			cpu.SetRegister("EAX", returnValue);
			return true;
		}

		// Handle unknown DLLs - this is the main enhancement
		logger.LogError("Unknown DLL function call: {Dll}!{Export}", dll, export);
		LogUnknownFunctionCall(dll, export);

		// Check if this DLL was dynamically loaded
		var isDynamicallyLoaded = _dynamicallyLoadedDlls.Contains(dll);
		if (isDynamicallyLoaded)
		{
			logger.LogInformation("Note: {Dll} was dynamically loaded via LoadLibrary", dll);
		}

		// Provide default behavior for unknown DLL calls
		returnValue = 0; // Default return value
		stdcallArgBytes = 0; // Default arg bytes (let caller handle stack cleanup)
		cpu.SetRegister("EAX", returnValue);

		return true; // Always return true now - we handle all calls
	}

	/// <summary>
	/// Async version of TryInvoke for async-aware CPU backends and Win32 modules
	/// </summary>
	public Task<(bool success, uint returnValue, int stdcallArgBytes)> TryInvokeAsync(string dll, string export, ICpu cpu, VirtualMemory memory)
	{
		// For now, wrap the synchronous version
		// Future: implement IWin32ModuleAsync interface for modules that need async operations
		var success = TryInvoke(dll, export, cpu, memory, out var returnValue, out var stdcallArgBytes);
		return Task.FromResult((success, returnValue, stdcallArgBytes));
	}

	private void LogUnknownFunctionCall(string dll, string export)
	{
		if (!_unknownFunctionCalls.TryGetValue(dll, out var functions))
		{
			functions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			_unknownFunctionCalls[dll] = functions;
		}

		if (functions.Add(export))
		{
			logger.LogInformation("New unimplemented function: {Dll}!{Export} (total for {S}: {FunctionsCount})", dll, export, dll, functions.Count);
		}
	}

	public void PrintUnknownFunctionsSummary()
	{
		if (_unknownFunctionCalls.Count == 0)
		{
			logger.LogInformation("No unimplemented functions found!");
			return;
		}

		Console.WriteLine($"Summary of unknown function calls ({_unknownFunctionCalls.Count} DLLs):");
		foreach (var (dll, functions) in _unknownFunctionCalls.OrderBy(kvp => kvp.Key))
		{
			Console.WriteLine($"  {dll}: {functions.Count} functions");
			foreach (var func in functions.OrderBy(f => f))
			{
				Console.WriteLine($"    - {func}");
			}
		}
	}
}