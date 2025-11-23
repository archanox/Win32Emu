using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Diagnostics;
using Win32Emu.Memory;
using Win32Emu.Loader;
using System.IO;

namespace Win32Emu.Win32;

public class Win32Dispatcher(ILogger logger)
{
	private readonly Dictionary<string, IWin32ModuleUnsafe> _modules = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> _dynamicallyLoadedDlls = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, HashSet<string>> _unknownFunctionCalls = new(StringComparer.OrdinalIgnoreCase);
	private ApiCallTracer? _apiCallTracer;
	private ProcessEnvironment? _env;
	
	/// <summary>
	/// Sets the process environment for querying loaded images
	/// </summary>
	public void SetProcessEnvironment(ProcessEnvironment? env)
	{
		_env = env;
	}
	
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
		return TryInvoke(dll, export, cpu, memory, out returnValue, out stdcallArgBytes, out _);
	}

	public bool TryInvoke(string dll, string export, ICpu cpu, VirtualMemory memory, out uint returnValue, out int stdcallArgBytes, out Loader.CallingConvention? callingConvention)
	{
		returnValue = 0;
		stdcallArgBytes = 0;
		callingConvention = null;

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

		// Resolve ordinal-based export names (e.g., "ORDINAL_4") to actual method names
		// This allows modules to implement functions by their actual names without handling ordinals
		var originalExport = export;
		export = DllModuleExportInfo.ResolveOrdinalExport(dll, export);
		if (originalExport != export)
		{
			logger.LogDebug("[Dispatcher] Resolved ordinal export {OriginalExport} to {Export} for {Dll}", originalExport, export, dll);
		}

		// Try to invoke with known modules first
		if (_modules.TryGetValue(dll, out var mod))
		{
			if (mod.TryInvokeUnsafe(export, cpu, memory, out var retUnsafe))
			{
				returnValue = retUnsafe;
				// Set EAX with return value per x86 stdcall convention
				// NOTE: This is REQUIRED for debugger modes (interactive debugger, GDB server)
				// which call TryInvoke directly and rely on the dispatcher to set EAX.
				// For the HandleSyscall code path, this is redundant (that path also sets EAX),
				// but keeping it here ensures all callers get consistent behavior.
				cpu.SetRegister("EAX", retUnsafe);

				// Try to get arg bytes from metadata (use resolved export name)
				if (StdCallMeta.TryGetArgBytes(dll, export, out stdcallArgBytes))
				{
					// For Win32 API modules, always use stdcall convention
					callingConvention = Loader.CallingConvention.Stdcall;
					logger.LogInformation("[Dispatcher] {Dll}!{Export} returned 0x{ReturnValue:X8}, argBytes={StdcallArgBytes}", dll, originalExport, returnValue, stdcallArgBytes);
				}
				else
				{
					// Function is missing [DllModuleExport] attribute
					logger.LogError("[Dispatcher] {Dll}!{Export} is missing [DllModuleExport] attribute - cannot determine stack cleanup bytes", dll, export);
					stdcallArgBytes = 0; // Default to 0, but this may cause stack corruption
					callingConvention = Loader.CallingConvention.Stdcall; // Assume stdcall
					logger.LogInformation("[Dispatcher] {Dll}!{Export} returned 0x{ReturnValue:X8}, argBytes={StdcallArgBytes} (MISSING METADATA)", dll, originalExport, returnValue, stdcallArgBytes);
				}

				// Log to API tracer if enabled (use original export for logging)
				// TODO(enhancement): Parse parameters from stack using metadata from [DllModuleExport]
				// This would provide detailed parameter values in the trace for better diagnostics.
				// See issue: (create issue to track this enhancement)
				_apiCallTracer?.LogApiCall(
					moduleName: dll,
					functionName: originalExport,
					parameters: null, // Parameters not yet parsed - would require stack walking
					returnValue: returnValue,
					eip: eip);

				return true;
			}

			// Known module but unknown export - log this (use original export name for logging)
			logger.LogError("Unimplemented function in known module: {Dll}!{Export}", dll, originalExport);
			LogUnknownFunctionCall(dll, originalExport);

			// Return success with default behavior
			returnValue = 0;
			stdcallArgBytes = 0; // Default for unknown functions
			// Set EAX for consistency (see comment above about debugger modes)
			cpu.SetRegister("EAX", returnValue);
			return true;
		}

		// Handle unknown DLLs - this is the main enhancement (use original export name for logging)
		logger.LogError("Unknown DLL function call: {Dll}!{Export}", dll, originalExport);
		LogUnknownFunctionCall(dll, originalExport);

		// Check if this DLL was dynamically loaded as a PE image
		var isDynamicallyLoaded = _dynamicallyLoadedDlls.Contains(dll);
		if (isDynamicallyLoaded && _env != null)
		{
			// Try to find the loaded PE image for this DLL
			var loadedImages = _env.GetAllLoadedImages();
			foreach (var (moduleName, image) in loadedImages)
			{
				// Match by DLL name (case-insensitive)
				var imageName = Path.GetFileName(image.FilePath);
				if (string.Equals(imageName, dll, StringComparison.OrdinalIgnoreCase))
				{
					// Check if the export has metadata
					// Try resolved export name first, then original export name
					if (image.ExportMetadata.TryGetValue(export, out var exportMeta) ||
					    image.ExportMetadata.TryGetValue(originalExport, out exportMeta))
					{
						logger.LogInformation("[Dispatcher] Found PE export metadata for {Dll}!{Export}: Convention={Convention}, StackArgBytes={StackArgBytes}",
							dll, export, exportMeta.Convention, exportMeta.StackArgBytes);
						
						stdcallArgBytes = exportMeta.StackArgBytes;
						callingConvention = exportMeta.Convention;
						
						// For cdecl, stdcallArgBytes is 0, but downstream code MUST check callingConvention
						// to emit plain RET (0xC3) instead of RET imm16 (0xC2). Do not rely solely on argBytes.
						if (exportMeta.Convention == Loader.CallingConvention.Cdecl)
						{
							stdcallArgBytes = 0; // Caller cleans, not callee
						}
						
						break;
					}
				}
			}
		}
		
		if (isDynamicallyLoaded)
		{
			logger.LogInformation("Note: {Dll} was dynamically loaded via LoadLibrary", dll);
		}

		// Provide default behavior for unknown DLL calls
		returnValue = 0; // Default return value
		stdcallArgBytes = 0; // Default arg bytes (let caller handle stack cleanup)
		// Set EAX for consistency (see comment above about debugger modes)
		cpu.SetRegister("EAX", returnValue);

		return true; // Always return true now - we handle all calls
	}

	/// <summary>
	/// Async version of TryInvoke for async-aware CPU backends and Win32 modules.
	/// This version supports modules that implement IWin32ModuleAsync for proper async execution.
	/// </summary>
	public async Task<(bool success, uint returnValue, int stdcallArgBytes, Loader.CallingConvention? callingConvention)> TryInvokeAsync(
		string dll, 
		string export, 
		ICpu cpu, 
		VirtualMemory memory, 
		CancellationToken cancellationToken = default)
	{
		var eip = cpu.GetEip();
		var esp = cpu.GetRegister("ESP");
		byte[]? stackSnippet = null;
		try
		{
			stackSnippet = memory.GetSpan(esp, 16);
		}
		catch (Exception ex)
		{
			logger.LogDebug(ex, "Failed to read stack snippet at ESP=0x{Esp:X8} in TryInvokeAsync", esp);
		}

		logger.LogInformation("Dispatching async {Dll}!{Export} at EIP=0x{GetEip:X8} ESP=0x{Esp:X8} stack={Unreadable}", 
			dll, export, eip, esp, stackSnippet == null ? "<unreadable>" : BitConverter.ToString(stackSnippet).Replace('-', ' '));

		// Resolve ordinal-based export names (e.g., "ORDINAL_4") to actual method names
		var originalExport = export;
		export = DllModuleExportInfo.ResolveOrdinalExport(dll, export);
		if (originalExport != export)
		{
			logger.LogDebug("[Dispatcher] Resolved ordinal export {OriginalExport} to {Export} for {Dll}", originalExport, export, dll);
		}

		// Try async-aware modules first
		if (_modules.TryGetValue(dll, out var mod) && mod is IWin32ModuleAsync asyncMod)
		{
			var (success, returnValue) = await asyncMod.TryInvokeAsync(export, cpu, memory, cancellationToken).ConfigureAwait(false);
			if (success)
			{
				// Set EAX with return value per x86 stdcall convention
				cpu.SetRegister("EAX", returnValue);

				// Try to get arg bytes from metadata (use resolved export name)
				if (StdCallMeta.TryGetArgBytes(dll, export, out var stdcallArgBytes))
				{
					logger.LogInformation("[Dispatcher] Async {Dll}!{Export} returned 0x{ReturnValue:X8}, argBytes={StdcallArgBytes}", 
						dll, originalExport, returnValue, stdcallArgBytes);
				}
				else
				{
					// Function is missing [DllModuleExport] attribute
					logger.LogError("[Dispatcher] Async {Dll}!{Export} is missing [DllModuleExport] attribute - cannot determine stack cleanup bytes", 
						dll, export);
					stdcallArgBytes = 0;
					logger.LogInformation("[Dispatcher] Async {Dll}!{Export} returned 0x{ReturnValue:X8}, argBytes={StdcallArgBytes} (MISSING METADATA)", 
						dll, originalExport, returnValue, stdcallArgBytes);
				}

				// Log to API tracer if enabled (use original export for logging)
				_apiCallTracer?.LogApiCall(
					moduleName: dll,
					functionName: originalExport,
					parameters: null,
					returnValue: returnValue,
					eip: eip);

				return (true, returnValue, stdcallArgBytes, Loader.CallingConvention.Stdcall);
			}
		}

		// Fall back to synchronous version for non-async modules
		var syncSuccess = TryInvoke(dll, export, cpu, memory, out var syncReturnValue, out var syncStdcallArgBytes, out var syncCallingConvention);
		return (syncSuccess, syncReturnValue, syncStdcallArgBytes, syncCallingConvention);
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

	/// <summary>
	/// Public method to track unknown function calls from other components (e.g., GetProcAddress failures)
	/// </summary>
	public void TrackUnknownFunction(string dll, string export)
	{
		LogUnknownFunctionCall(dll, export);
	}

	public void PrintUnknownFunctionsSummary()
	{
		if (_unknownFunctionCalls.Count == 0)
		{
			logger.LogInformation("No unimplemented functions found!");
			return;
		}

		logger.LogWarning("Summary of unknown function calls ({Count} DLLs):", _unknownFunctionCalls.Count);
		foreach (var (dll, functions) in _unknownFunctionCalls.OrderBy(kvp => kvp.Key))
		{
			logger.LogWarning("  {Dll}: {Count} functions", dll, functions.Count);
			foreach (var func in functions.OrderBy(f => f))
			{
				logger.LogWarning("    - {Function}", func);
			}
		}
	}
}