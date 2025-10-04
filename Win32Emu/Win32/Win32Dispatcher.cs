using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	public class Win32Dispatcher
	{
		private readonly Dictionary<string, IWin32ModuleUnsafe> _modules = new(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> _dynamicallyLoadedDlls = new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, HashSet<string>> _unknownFunctionCalls = new(StringComparer.OrdinalIgnoreCase);

		public void RegisterModule(IWin32ModuleUnsafe module) => _modules[module.Name] = module;
    
		public void RegisterDynamicallyLoadedDll(string dllName)
		{
			_dynamicallyLoadedDlls.Add(dllName);
			Diagnostics.Diagnostics.LogInfo($"[Dispatcher] Registered dynamically loaded DLL: {dllName}");
		}

		public bool TryGetModule(string dllName, out IWin32ModuleUnsafe? module)
		{
			return _modules.TryGetValue(dllName, out module);
		}

		public bool TryInvoke(string dll, string export, ICpu cpu, VirtualMemory memory, out uint returnValue, out int stdcallArgBytes)
		{
			returnValue = 0;
			stdcallArgBytes = 0;

			var esp = cpu.GetRegister("ESP");
			byte[] stackSnippet = null;
			try { stackSnippet = memory.GetSpan(esp, 16); } catch { }
			Diagnostics.Diagnostics.LogInfo($"Dispatching {dll}!{export} at EIP=0x{cpu.GetEip():X8} ESP=0x{esp:X8} stack={ (stackSnippet==null?"<unreadable>":BitConverter.ToString(stackSnippet).Replace('-', ' ')) }");
        
			// Try to invoke with known modules first
			if (_modules.TryGetValue(dll, out var mod))
			{
				if (mod.TryInvokeUnsafe(export, cpu, memory, out var retUnsafe))
				{
					returnValue = retUnsafe;
					cpu.SetRegister("EAX", retUnsafe);
                
					// Try to get arg bytes, but don't fail if not available
					try
					{
						stdcallArgBytes = StdCallMeta.GetArgBytes(dll, export);
					}
					catch (InvalidOperationException)
					{
						// Hardcoded fixes for functions with missing metadata (temporary workaround)
						var dllUpper = dll.ToUpperInvariant();
						var exportUpper = export.ToUpperInvariant();
						stdcallArgBytes = (dllUpper, exportUpper) switch
						{
							("KERNEL32.DLL", "GETACP") => 0,        // UINT GetACP(void)
							("KERNEL32.DLL", "GETCPINFO") => 8,     // BOOL GetCPInfo(UINT, LPCPINFO)
							_ => 0
						};
                    
						if (stdcallArgBytes > 0)
						{
							Diagnostics.Diagnostics.LogWarn($"Using hardcoded arg bytes for {dll}!{export}: {stdcallArgBytes}");
						}
						else
						{
							Diagnostics.Diagnostics.LogWarn($"No arg bytes metadata for {dll}!{export}, using 0");
						}
					}
                
					return true;
				}

				// Known module but unknown export - log this
				Diagnostics.Diagnostics.LogWarn($"Unimplemented function in known module: {dll}!{export}");
				LogUnknownFunctionCall(dll, export);
                
				// Return success with default behavior
				returnValue = 0;
				stdcallArgBytes = 0; // Default for unknown functions
				cpu.SetRegister("EAX", returnValue);
				return true;
			}
        
			// Handle unknown DLLs - this is the main enhancement
			Diagnostics.Diagnostics.LogWarn($"Unknown DLL function call: {dll}!{export}");
			LogUnknownFunctionCall(dll, export);
        
			// Check if this DLL was dynamically loaded
			var isDynamicallyLoaded = _dynamicallyLoadedDlls.Contains(dll);
			if (isDynamicallyLoaded)
			{
				Diagnostics.Diagnostics.LogInfo($"Note: {dll} was dynamically loaded via LoadLibrary");
			}
        
			// Provide default behavior for unknown DLL calls
			returnValue = 0; // Default return value
			stdcallArgBytes = 0; // Default arg bytes (let caller handle stack cleanup)
			cpu.SetRegister("EAX", returnValue);
        
			return true; // Always return true now - we handle all calls
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
				Diagnostics.Diagnostics.LogInfo($"New unimplemented function: {dll}!{export} (total for {dll}: {functions.Count})");
			}
		}
    
		public void PrintUnknownFunctionsSummary()
		{
			if (_unknownFunctionCalls.Count == 0)
			{
				Diagnostics.Diagnostics.LogInfo("No unknown function calls recorded.");
				return;
			}
        
			Diagnostics.Diagnostics.LogInfo($"Summary of unknown function calls ({_unknownFunctionCalls.Count} DLLs):");
			foreach (var (dll, functions) in _unknownFunctionCalls.OrderBy(kvp => kvp.Key))
			{
				Diagnostics.Diagnostics.LogInfo($"  {dll}: {functions.Count} functions");
				foreach (var func in functions.OrderBy(f => f))
				{
					Diagnostics.Diagnostics.LogInfo($"    - {func}");
				}
			}
		}
	}
}
