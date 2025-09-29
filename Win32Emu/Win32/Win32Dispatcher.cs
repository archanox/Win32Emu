using System.Reflection;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public class Win32Dispatcher
{
    private readonly Dictionary<string, IWin32ModuleUnsafe> _modules = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _dynamicallyLoadedDlls = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _unknownFunctionCalls = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterModule(IWin32ModuleUnsafe module) => _modules[module.Name] = module;
    
    public void RegisterDynamicallyLoadedDll(string dllName)
    {
        _dynamicallyLoadedDlls.Add(dllName);
        Console.WriteLine($"[Dispatcher] Registered dynamically loaded DLL: {dllName}");
    }

    public bool TryInvoke(string dll, string export, ICpu cpu, VirtualMemory memory, out uint returnValue, out int stdcallArgBytes)
    {
        returnValue = 0;
        stdcallArgBytes = 0;
        
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
                    // Default to 0 for unknown functions in known modules
                    stdcallArgBytes = 0;
                    Console.WriteLine($"[Dispatcher] Warning: No arg bytes metadata for {dll}!{export}, using 0");
                }
                
                return true;
            }
            else
            {
                // Known module but unknown export - log this
                Console.WriteLine($"[Dispatcher] Unimplemented function in known module: {dll}!{export}");
                LogUnknownFunctionCall(dll, export);
                
                // Return success with default behavior
                returnValue = 0;
                stdcallArgBytes = 0; // Default for unknown functions
                cpu.SetRegister("EAX", returnValue);
                return true;
            }
        }
        
        // Handle unknown DLLs - this is the main enhancement
        Console.WriteLine($"[Dispatcher] Unknown DLL function call: {dll}!{export}");
        LogUnknownFunctionCall(dll, export);
        
        // Check if this DLL was dynamically loaded
        bool isDynamicallyLoaded = _dynamicallyLoadedDlls.Contains(dll);
        if (isDynamicallyLoaded)
        {
            Console.WriteLine($"[Dispatcher] Note: {dll} was dynamically loaded via LoadLibrary");
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
            Console.WriteLine($"[Dispatcher] New unimplemented function: {dll}!{export} (total for {dll}: {functions.Count})");
        }
    }
    
    public void PrintUnknownFunctionsSummary()
    {
        if (_unknownFunctionCalls.Count == 0)
        {
            Console.WriteLine("[Dispatcher] No unknown function calls recorded.");
            return;
        }
        
        Console.WriteLine($"[Dispatcher] Summary of unknown function calls ({_unknownFunctionCalls.Count} DLLs):");
        foreach (var (dll, functions) in _unknownFunctionCalls.OrderBy(kvp => kvp.Key))
        {
            Console.WriteLine($"[Dispatcher]   {dll}: {functions.Count} functions");
            foreach (var func in functions.OrderBy(f => f))
            {
                Console.WriteLine($"[Dispatcher]     - {func}");
            }
        }
    }
}
