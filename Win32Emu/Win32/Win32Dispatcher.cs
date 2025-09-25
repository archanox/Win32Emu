using System.Reflection;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public class Win32Dispatcher
{
    private readonly Dictionary<string, IWin32ModuleUnsafe> _modules = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterModule(IWin32ModuleUnsafe module) => _modules[module.Name] = module;

    public bool TryInvoke(string dll, string export, ICpu cpu, VirtualMemory memory, out uint returnValue, out int stdcallArgBytes)
    {
        returnValue = 0;
        stdcallArgBytes = 0;
        if (!_modules.TryGetValue(dll, out var mod)) return false;

        if (!mod.TryInvokeUnsafe(export, cpu, memory, out var retUnsafe)) return false;

        returnValue = retUnsafe;
        cpu.SetRegister("EAX", retUnsafe);
        // Prefer generated meta if available
        stdcallArgBytes = StdCallMeta.GetArgBytes(dll, export);

        return true;
    }
}
