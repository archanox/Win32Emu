using Win32Emu.Cpu.Iced;
using Win32Emu.Debugging;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Enhanced CPU debugging with actual implementation of debugging features
/// </summary>
public class CpuDebuggingTests
{
    [Fact]
    public void TestEnhancedCpuDebugger()
    {
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        var debugger = new EnhancedCpuDebugger(cpu, memory);
        
        // Test the debugging functionality
        cpu.SetRegister("EBP", 0x00000001); // Problematic value
        cpu.SetRegister("ESP", 0x00200000);
        cpu.SetEip(0x00401000);
        
        // Create instruction that will cause the 0xFFFFFFFD issue
        var testCode = new byte[] { 0x8B, 0x45, 0xFC }; // MOV EAX, [EBP-4]
        memory.WriteBytes(0x00401000, testCode);
        
        // Enable debugging
        debugger.EnableSuspiciousRegisterDetection = true;
        debugger.LogAllInstructions = false; // Don't spam, just log problems
        debugger.LogToConsole = false; // Don't spam during tests
        
        // This should detect the suspicious EBP value and log it
        var warnings = debugger.CheckRegistersBeforeStep();
        Assert.NotEmpty(warnings);
        Assert.Contains("EBP suspiciously small", warnings[0]);
        
        // This should catch and log the exception with full context
        var exception = Assert.Throws<IndexOutOfRangeException>(() => 
            debugger.SafeSingleStep());
        
        Assert.Contains("0xFFFFFFFD", exception.Message);
        
        // Verify that debugging info was captured
        var lastState = debugger.GetLastRegisterState();
        Assert.Equal(0x00000001u, lastState.Ebp);
        Assert.Equal(0x00401000u, lastState.Eip);
    }
    
    [Fact]
    public void TestInstructionTracing()
    {
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        var debugger = new EnhancedCpuDebugger(cpu, memory);
        
        // Set up a sequence of instructions to trace
        cpu.SetRegister("EBP", 0x00100000); // Valid frame pointer
        cpu.SetRegister("ESP", 0x00100000);
        cpu.SetEip(0x00401000);
        
        // Initialize memory at ESP for the PUSH instruction
        memory.Write32(0x00100000 - 4, 0); // Space for PUSH EBP
        
        // Create a small program
        var program = new byte[]
        {
            0x55,               // PUSH EBP
            0x89, 0xE5,         // MOV EBP, ESP
            0x8B, 0x45, 0x08,   // MOV EAX, [EBP+8] - this will fail but we'll stop before it
            0x5D,               // POP EBP
            0xC3                // RET
        };
        memory.WriteBytes(0x00401000, program);
        
        // Enable instruction tracing
        debugger.LogAllInstructions = false; // Don't spam console during tests
        debugger.LogToConsole = false; // Silent for tests
        debugger.EnableSuspiciousRegisterDetection = true;
        
        // Execute the first two instructions (safe ones)
        debugger.SafeSingleStep(); // PUSH EBP
        debugger.SafeSingleStep(); // MOV EBP, ESP
        
        // Verify we captured the execution trace
        var trace = debugger.GetExecutionTrace();
        Assert.True(trace.Count >= 2);
        
        // First instruction should be PUSH EBP at 0x00401000
        Assert.Equal(0x00401000u, trace[0].Eip);
        Assert.Contains("55", trace[0].InstructionBytes); // PUSH EBP opcode
    }
    
    [Fact]
    public void TestProblematicEipDetection()
    {
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        var debugger = new EnhancedCpuDebugger(cpu, memory);
        
        // Set the CPU to the problematic EIP from the original error
        cpu.SetEip(0x0F000512);
        cpu.SetRegister("EBP", 0x00000000); // Problematic register state
        
        // Test problematic EIP detection
        Assert.True(debugger.IsProblematicEip());
        Assert.False(debugger.IsProblematicEip(0x0F000513));
        
        // Test that it doesn't crash when handling the problematic EIP
        debugger.LogToConsole = false; // Silent for test
        debugger.HandleProblematicEip();
        
        // First capture state to ensure _lastState is set  
        debugger.CheckRegistersBeforeStep();
        
        // Verify state was captured correctly
        var state = debugger.GetLastRegisterState();
        Assert.NotNull(state);
        Assert.Equal(0x0F000512u, state.Eip);
        Assert.Equal(0x00000000u, state.Ebp);
    }
    
    [Fact]
    public void TestCpuDebuggingExtensions()
    {
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        
        // Initialize registers to known state first
        cpu.SetRegister("EBP", 0x00200000); // Set to valid value initially  
        cpu.SetRegister("ESP", 0x00200000);
        
        // Test extension methods
        Assert.False(cpu.HasSuspiciousRegisters()); // Should be fine initially
        
        cpu.SetRegister("EBP", 0x00000500); // Set to suspicious value
        Assert.True(cpu.HasSuspiciousRegisters()); // Should detect it
        
        // Test debugger creation
        var debugger = cpu.CreateDebugger(memory);
        Assert.NotNull(debugger);
        Assert.IsType<EnhancedCpuDebugger>(debugger);
    }
    
    [Fact]
    public void TestSyntheticImportAddressDetection()
    {
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        
        // Set up a synthetic import address with INT3 stub (like PeImageLoader creates)
        var syntheticAddress = 0x0F000512u; // This is the problematic address from the error log
        var int3Stub = new byte[] { 0xCC, 0x90, 0x90, 0x90 }; // INT3 + NOPs
        memory.WriteBytes(syntheticAddress, int3Stub);
        
        cpu.SetEip(syntheticAddress);
        cpu.SetRegister("ESP", 0x00200000);
        
        // Test that SingleStep correctly identifies this as a call to synthetic import
        var stepResult = cpu.SingleStep(memory);
        
        // The IcedCpu should recognize INT3 at synthetic address as a call
        Assert.True(stepResult.IsCall);
        Assert.Equal(syntheticAddress, stepResult.CallTarget);
    }

    [Fact] 
    public void ShowUsageExample()
    {
        // This test demonstrates how to use the enhanced debugging in your Program.cs
        Console.WriteLine("=== Enhanced Debugging Usage ===");
        Console.WriteLine();
        Console.WriteLine("To use the enhanced debugging in your Program.cs, replace your main loop with:");
        Console.WriteLine();
        Console.WriteLine("```csharp");
        Console.WriteLine("using Win32Emu.Debugging;");
        Console.WriteLine();
        Console.WriteLine("// Method 1: Simple replacement");
        Console.WriteLine("for (var i = 0; i < maxInstr && !env.ExitRequested; i++)");
        Console.WriteLine("{");
        Console.WriteLine("    try");
        Console.WriteLine("    {");
        Console.WriteLine("        // Replace: var step = cpu.SingleStep(vm);"); 
        Console.WriteLine("        // With:");
        Console.WriteLine("        cpu.DebugStep(vm, logSuspiciousRegisters: true, logAllInstructions: false);");
        Console.WriteLine("        var step = new CpuStepResult(false, 0); // You'll need to modify DebugStep to return this");
        Console.WriteLine("        ");
        Console.WriteLine("        // ... rest of your existing code");
        Console.WriteLine("    }");
        Console.WriteLine("    catch (IndexOutOfRangeException ex)");
        Console.WriteLine("    {");
        Console.WriteLine("        Console.WriteLine($\"Caught error at instruction {i}: {ex.Message}\");");
        Console.WriteLine("        cpu.LogRegisters(\"Final state: \");");
        Console.WriteLine("        throw;");
        Console.WriteLine("    }");
        Console.WriteLine("}");
        Console.WriteLine();
        Console.WriteLine("// Method 2: Full-featured debugging");
        Console.WriteLine("var debugger = cpu.CreateDebugger(vm);");
        Console.WriteLine("debugger.EnableSuspiciousRegisterDetection = true;");
        Console.WriteLine("debugger.LogToConsole = true;");
        Console.WriteLine();
        Console.WriteLine("for (var i = 0; i < maxInstr && !env.ExitRequested; i++)");
        Console.WriteLine("{");
        Console.WriteLine("    // Check for the specific problematic EIP");
        Console.WriteLine("    if (debugger.IsProblematicEip(0x0F000512))");
        Console.WriteLine("    {");
        Console.WriteLine("        debugger.HandleProblematicEip();");
        Console.WriteLine("        break; // Stop before crash");
        Console.WriteLine("    }");
        Console.WriteLine("    ");
        Console.WriteLine("    try"); 
        Console.WriteLine("    {");
        Console.WriteLine("        debugger.SafeSingleStep();");
        Console.WriteLine("        // ... handle your import calls etc.");
        Console.WriteLine("    }");
        Console.WriteLine("    catch (IndexOutOfRangeException ex)");
        Console.WriteLine("    {");
        Console.WriteLine("        // The debugger already logged detailed info");
        Console.WriteLine("        Console.WriteLine($\"Execution stopped at instruction {i}\");");
        Console.WriteLine("        var suspiciousStates = debugger.FindSuspiciousStates();");
        Console.WriteLine("        Console.WriteLine($\"Found {suspiciousStates.Count} suspicious register states in trace\");");
        Console.WriteLine("        throw;");
        Console.WriteLine("    }");
        Console.WriteLine("}");
        Console.WriteLine("```");
    }
}