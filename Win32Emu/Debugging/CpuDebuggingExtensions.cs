using Microsoft.Extensions.Logging;
using Win32Emu.Cpu.Iced;
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
    /// Note: Only checks ESP (stack pointer) since EBP can legally be used as a general-purpose register.
    /// Many programs use EBP for loop counters, temporary values, or other purposes.
    /// </summary>
    public static bool HasSuspiciousRegisters(this IcedCpu cpu, uint threshold = 0x1000)
    {
        var esp = cpu.GetRegister("ESP");
        
        // Only check ESP - it should always point to valid stack memory
        // Do NOT check EBP - it can legally be used as a general-purpose register
        return esp <= threshold;
    }
    
    /// <summary>
    /// Log current register state to console
    /// </summary>
    public static void LogRegisters(this IcedCpu cpu, ILogger logger, string prefix = "")
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
        
        logger.LogDebug("{Prefix}EIP=0x{Eip:X8} | EAX=0x{Eax:X8} EBX=0x{Ebx:X8} ECX=0x{Ecx:X8} EDX=0x{Edx:X8} ESP=0x{Esp:X8} EBP=0x{Ebp:X8} | ESI=0x{Esi:X8} EDI=0x{Edi:X8}", prefix, eip, eax, ebx, ecx, edx, esp, ebp, esi, edi);
    }
}