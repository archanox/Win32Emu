using Win32Emu.Cpu.IcedImpl;
using Win32Emu.Memory;

namespace Win32Emu.Debugging;

/// <summary>
/// Extension methods to easily add enhanced debugging to your main program
/// </summary>
public static class CpuDebuggingExtensions
{
    /// <summary>
    /// Create an enhanced debugger wrapper for your CPU
    /// </summary>
    public static EnhancedCpuDebugger CreateDebugger(this IcedCpu cpu, VirtualMemory memory)
    {
        return new EnhancedCpuDebugger(cpu, memory);
    }
    
    /// <summary>
    /// Execute a single step with automatic error detection and logging
    /// Usage: Replace cpu.SingleStep(vm) with cpu.DebugStep(vm)
    /// </summary>
    public static void DebugStep(this IcedCpu cpu, VirtualMemory memory, 
        bool logSuspiciousRegisters = true, 
        bool logAllInstructions = false,
        uint suspiciousThreshold = 0x1000)
    {
        var debugger = new EnhancedCpuDebugger(cpu, memory)
        {
            EnableSuspiciousRegisterDetection = logSuspiciousRegisters,
            LogAllInstructions = logAllInstructions,
            LogToConsole = true,
            SuspiciousThreshold = suspiciousThreshold
        };
        
        debugger.CheckRegistersBeforeStep();
        debugger.SafeSingleStep();
    }
    
    /// <summary>
    /// Check if the current register state looks suspicious
    /// </summary>
    public static bool HasSuspiciousRegisters(this IcedCpu cpu, uint threshold = 0x1000)
    {
        var ebp = cpu.GetRegister("EBP");
        var esp = cpu.GetRegister("ESP");
        
        return ebp <= threshold || esp <= threshold || 
               (ebp != 0 && esp != 0 && ebp < esp - 0x10000);
    }
    
    /// <summary>
    /// Log current register state to console
    /// </summary>
    public static void LogRegisters(this IcedCpu cpu, string prefix = "")
    {
        var eip = cpu.GetEip();
        var eax = cpu.GetRegister("EAX");
        var ebx = cpu.GetRegister("EBX");
        var ecx = cpu.GetRegister("ECX");
        var edx = cpu.GetRegister("EDX");
        var esi = cpu.GetRegister("ESI");
        var edi = cpu.GetRegister("EDI");
        var ebp = cpu.GetRegister("EBP");
        var esp = cpu.GetRegister("ESP");
        
        Console.WriteLine($"{prefix}EIP=0x{eip:X8} | EAX=0x{eax:X8} EBX=0x{ebx:X8} ECX=0x{ecx:X8} EDX=0x{edx:X8}");
        Console.WriteLine($"{prefix}ESP=0x{esp:X8} EBP=0x{ebp:X8} | ESI=0x{esi:X8} EDI=0x{edi:X8}");
    }
}