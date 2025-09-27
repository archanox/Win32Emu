using Win32Emu.Cpu.IcedImpl;
using Win32Emu.Memory;

namespace Win32Emu.Debugging;

/// <summary>
/// Enhanced CPU debugger that provides detailed logging and error detection
/// </summary>
public class EnhancedCpuDebugger
{
    private readonly IcedCpu _cpu;
    private readonly VirtualMemory _memory;
    private readonly List<CpuState> _executionTrace = new();
    private CpuState _lastState;
    
    public bool EnableSuspiciousRegisterDetection { get; set; } = true;
    public bool LogAllInstructions { get; set; } = false;
    public bool LogToConsole { get; set; } = true;
    public uint SuspiciousThreshold { get; set; } = 0x1000;
    
    public EnhancedCpuDebugger(IcedCpu cpu, VirtualMemory memory)
    {
        _cpu = cpu;
        _memory = memory;
    }
    
    /// <summary>
    /// Check registers for suspicious values before executing an instruction
    /// </summary>
    public List<string> CheckRegistersBeforeStep()
    {
        var warnings = new List<string>();
        var state = CaptureCurrentState();
        _lastState = state;
        
        if (EnableSuspiciousRegisterDetection)
        {
            if (state.EBP <= SuspiciousThreshold && state.EBP != 0)
            {
                var warning = $"WARNING: EBP suspiciously small: 0x{state.EBP:X8} at EIP=0x{state.EIP:X8}";
                warnings.Add(warning);
                if (LogToConsole) Console.WriteLine(warning);
            }
            
            if (state.ESP <= SuspiciousThreshold && state.ESP != 0)
            {
                var warning = $"WARNING: ESP suspiciously small: 0x{state.ESP:X8} at EIP=0x{state.EIP:X8}";
                warnings.Add(warning);
                if (LogToConsole) Console.WriteLine(warning);
            }
            
            // Check for frame pointer corruption (EBP should generally be >= ESP)
            if (state.EBP != 0 && state.ESP != 0 && state.EBP < state.ESP - 0x10000)
            {
                var warning = $"WARNING: EBP (0x{state.EBP:X8}) much smaller than ESP (0x{state.ESP:X8})";
                warnings.Add(warning);
                if (LogToConsole) Console.WriteLine(warning);
            }
        }
        
        return warnings;
    }
    
    /// <summary>
    /// Execute a single step with enhanced error handling and logging
    /// </summary>
    public void SafeSingleStep()
    {
        var stateBefore = CaptureCurrentState();
        
        if (LogAllInstructions)
        {
            LogInstruction(stateBefore);
        }
        
        try
        {
            _cpu.SingleStep(_memory);
            
            // Add to execution trace
            _executionTrace.Add(stateBefore);
            
            // Keep trace size manageable
            if (_executionTrace.Count > 1000)
            {
                _executionTrace.RemoveAt(0);
            }
        }
        catch (IndexOutOfRangeException ex) when (ex.Message.Contains("0xFFFFFFFD") || 
                                                  ex.Message.Contains("0xFFFFFFFF") ||
                                                  ex.Message.Contains("0xFFFFFFF"))
        {
            // Enhanced error logging for memory access violations
            if (LogToConsole)
            {
                Console.WriteLine("*** MEMORY ACCESS VIOLATION DETECTED ***");
                Console.WriteLine($"EIP: 0x{stateBefore.EIP:X8}");
                Console.WriteLine($"Instruction: {stateBefore.InstructionBytes}");
                Console.WriteLine("Register state:");
                Console.WriteLine($"  EAX=0x{stateBefore.EAX:X8} EBX=0x{stateBefore.EBX:X8} ECX=0x{stateBefore.ECX:X8} EDX=0x{stateBefore.EDX:X8}");
                Console.WriteLine($"  ESI=0x{stateBefore.ESI:X8} EDI=0x{stateBefore.EDI:X8} EBP=0x{stateBefore.EBP:X8} ESP=0x{stateBefore.ESP:X8}");
                Console.WriteLine($"Original exception: {ex.Message}");
                
                // Analyze the likely cause
                AnalyzeMemoryViolationCause(stateBefore);
            }
            
            throw; // Re-throw with original stack trace
        }
    }
    
    /// <summary>
    /// Analyze and explain the likely cause of a memory violation
    /// </summary>
    private void AnalyzeMemoryViolationCause(CpuState state)
    {
        Console.WriteLine("\n*** LIKELY CAUSE ANALYSIS ***");
        
        if (state.EBP <= 0x10)
        {
            Console.WriteLine("→ CAUSE: Uninitialized or corrupted frame pointer (EBP)");
            Console.WriteLine("  SOLUTION: Check that function prologues properly execute 'PUSH EBP; MOV EBP, ESP'");
        }
        else if (state.EBP <= SuspiciousThreshold)
        {
            Console.WriteLine("→ CAUSE: Frame pointer (EBP) corrupted to small value");
            Console.WriteLine("  SOLUTION: Check for buffer overflows or stack corruption in previous functions");
        }
        
        if (state.ESP <= SuspiciousThreshold)
        {
            Console.WriteLine("→ CAUSE: Stack pointer (ESP) corrupted or stack overflow");
            Console.WriteLine("  SOLUTION: Check for infinite recursion or excessive local variable allocation");
        }
        
        // Check recent execution history for patterns
        if (_executionTrace.Count > 10)
        {
            var recentCalls = _executionTrace.TakeLast(10).Count(t => t.InstructionBytes.StartsWith("E8")); // CALL instructions
            if (recentCalls > 5)
            {
                Console.WriteLine("→ PATTERN: Many recent function calls detected - possible stack overflow");
            }
        }
        
        Console.WriteLine("\nTo debug further:");
        Console.WriteLine("1. Add breakpoint just before this EIP");
        Console.WriteLine("2. Examine the call stack to see how EBP got corrupted");
        Console.WriteLine("3. Check the previous 10-20 instructions for buffer operations");
        Console.WriteLine("4. Verify calling conventions match between caller/callee");
    }
    
    /// <summary>
    /// Capture the current CPU and relevant memory state
    /// </summary>
    private CpuState CaptureCurrentState()
    {
        var eip = _cpu.GetEip();
        var instructionBytes = "";
        
        try
        {
            var bytes = _memory.GetSpan(eip, 8);
            instructionBytes = Convert.ToHexString(bytes);
        }
        catch
        {
            instructionBytes = "INVALID";
        }
        
        return new CpuState
        {
            EIP = eip,
            EAX = _cpu.GetRegister("EAX"),
            EBX = _cpu.GetRegister("EBX"),
            ECX = _cpu.GetRegister("ECX"),
            EDX = _cpu.GetRegister("EDX"),
            ESI = _cpu.GetRegister("ESI"),
            EDI = _cpu.GetRegister("EDI"),
            EBP = _cpu.GetRegister("EBP"),
            ESP = _cpu.GetRegister("ESP"),
            EFLAGS = _cpu.GetRegister("EFLAGS"),
            InstructionBytes = instructionBytes
        };
    }
    
    /// <summary>
    /// Log instruction execution with full context
    /// </summary>
    private void LogInstruction(CpuState state)
    {
        if (!LogToConsole) return;
        
        Console.WriteLine($"[0x{state.EIP:X8}] {state.InstructionBytes} | " +
                         $"EAX=0x{state.EAX:X8} EBP=0x{state.EBP:X8} ESP=0x{state.ESP:X8}");
    }
    
    /// <summary>
    /// Get the last captured register state
    /// </summary>
    public CpuState GetLastRegisterState() => _lastState;
    
    /// <summary>
    /// Get the execution trace (last 1000 instructions)
    /// </summary>
    public List<CpuState> GetExecutionTrace() => new(_executionTrace);
    
    /// <summary>
    /// Clear the execution trace
    /// </summary>
    public void ClearTrace() => _executionTrace.Clear();
    
    /// <summary>
    /// Find all occurrences of suspicious register values in the trace
    /// </summary>
    public List<CpuState> FindSuspiciousStates()
    {
        return _executionTrace.Where(state => 
            state.EBP <= SuspiciousThreshold || 
            state.ESP <= SuspiciousThreshold).ToList();
    }
    
    /// <summary>
    /// Check if current CPU state should trigger the problematic EIP breakpoint
    /// </summary>
    public bool IsProblematicEip(uint targetEip = 0x0F000512)
    {
        return _cpu.GetEip() == targetEip;
    }
    
    /// <summary>
    /// Print detailed state when problematic EIP is reached
    /// </summary>
    public void HandleProblematicEip(uint targetEip = 0x0F000512)
    {
        if (!IsProblematicEip(targetEip)) return;
        
        var state = CaptureCurrentState();
        
        Console.WriteLine($"*** FOUND PROBLEMATIC EIP! ***");
        Console.WriteLine($"EIP=0x{state.EIP:X8} EBP=0x{state.EBP:X8} ESP=0x{state.ESP:X8}");
        Console.WriteLine($"EAX=0x{state.EAX:X8} EBX=0x{state.EBX:X8} ECX=0x{state.ECX:X8} EDX=0x{state.EDX:X8}");
        Console.WriteLine($"ESI=0x{state.ESI:X8} EDI=0x{state.EDI:X8}");
        Console.WriteLine($"Instruction bytes: {state.InstructionBytes}");
        Console.WriteLine("*** STOPPING BEFORE CRASH ***");
        
        // Analyze what would happen
        if (state.EBP <= 0x10)
        {
            Console.WriteLine($"ANALYSIS: EBP=0x{state.EBP:X8} is extremely small - any negative displacement will wrap around!");
            Console.WriteLine("This will likely cause the 0xFFFFFFFD error.");
        }
    }
}

/// <summary>
/// Snapshot of CPU state at a point in time
/// </summary>
public record CpuState
{
    public uint EIP { get; init; }
    public uint EAX { get; init; }
    public uint EBX { get; init; }
    public uint ECX { get; init; }
    public uint EDX { get; init; }
    public uint ESI { get; init; }
    public uint EDI { get; init; }
    public uint EBP { get; init; }
    public uint ESP { get; init; }
    public uint EFLAGS { get; init; }
    public string InstructionBytes { get; init; } = "";
}