using Microsoft.Extensions.Logging;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Diagnostic tests to isolate the import call infinite loop issue.
/// These tests simulate the exact sequence that causes ign_teas.exe to hang.
/// </summary>
public class ImportCallDiagnosticTests
{
    private readonly ITestOutputHelper _output;

    public ImportCallDiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SimpleCallReturn_ShouldExecuteCorrectly()
    {
        // This test simulates a simple CALL/RET sequence without import handling
        // to verify basic CPU behavior is correct

        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);

        // Setup code at 0x00400000
        var codeBase = 0x00400000u;
        cpu.SetEip(codeBase);

        // Setup stack at 0x00100000
        var stackBase = 0x00100000u;
        cpu.SetRegister("ESP", stackBase);
        cpu.SetRegister("EBP", stackBase);

        // Write code:
        // 00400000: CALL 0x00400010  (E8 0B 00 00 00)
        // 00400005: NOP               (90)
        // 00400006: HLT               (F4) - we'll check we reach here
        // ...
        // 00400010: RET               (C3) - function to call
        
        memory.Write8(codeBase + 0, 0xE8);  // CALL rel32
        memory.Write8(codeBase + 1, 0x0B);
        memory.Write8(codeBase + 2, 0x00);
        memory.Write8(codeBase + 3, 0x00);
        memory.Write8(codeBase + 4, 0x00);
        memory.Write8(codeBase + 5, 0x90);  // NOP
        memory.Write8(codeBase + 6, 0xF4);  // HLT
        memory.Write8(codeBase + 0x10, 0xC3);  // RET

        // Execute CALL
        var step1 = cpu.SingleStep(memory);
        Assert.True(step1.IsCall);
        Assert.Equal(0x00400010u, step1.CallTarget);
        Assert.Equal(0x00400010u, cpu.GetEip());
        
        // Verify return address was pushed
        var esp = cpu.GetRegister("ESP");
        var returnAddr = memory.Read32(esp);
        _output.WriteLine($"After CALL: ESP=0x{esp:X8}, Return addr=0x{returnAddr:X8}, EIP=0x{cpu.GetEip():X8}");
        Assert.Equal(0x00400005u, returnAddr);

        // Execute RET
        var step2 = cpu.SingleStep(memory);
        Assert.False(step2.IsCall);
        
        // Verify we returned to correct address
        var eipAfterRet = cpu.GetEip();
        _output.WriteLine($"After RET: EIP=0x{eipAfterRet:X8}, ESP=0x{cpu.GetRegister("ESP"):X8}");
        Assert.Equal(0x00400005u, eipAfterRet);

        // Execute NOP
        var step3 = cpu.SingleStep(memory);
        Assert.Equal(0x00400006u, cpu.GetEip());
    }

    [Fact]
    public void CallWithManualStackManipulation_ShouldWork()
    {
        // This test simulates what happens during import handling:
        // 1. CALL instruction pushes return address
        // 2. We manually adjust ESP and EIP (like import handler does)
        // 3. Execution should continue normally

        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);

        var codeBase = 0x00400000u;
        var stackBase = 0x00100000u;
        cpu.SetEip(codeBase);
        cpu.SetRegister("ESP", stackBase);

        // Write code:
        // 00400000: CALL 0x0F000000  (E8 FB FF BF 0E) - call to import stub
        // 00400005: NOP               (90)
        // 00400006: NOP               (90)
        // 00400007: HLT               (F4)

        memory.Write8(codeBase + 0, 0xE8);
        memory.Write32(codeBase + 1, 0x0EBFFFFB);  // relative offset to 0x0F000000
        memory.Write8(codeBase + 5, 0x90);  // NOP
        memory.Write8(codeBase + 6, 0x90);  // NOP
        memory.Write8(codeBase + 7, 0xF4);  // HLT

        // Execute CALL - this will try to jump to 0x0F000000
        var step1 = cpu.SingleStep(memory);
        
        _output.WriteLine($"After CALL: IsCall={step1.IsCall}, CallTarget=0x{step1.CallTarget:X8}, EIP=0x{cpu.GetEip():X8}");
        Assert.True(step1.IsCall);
        Assert.Equal(0x0F000000u, step1.CallTarget);

        // Now simulate what the import handler does:
        // 1. Read return address from stack
        var esp = cpu.GetRegister("ESP");
        var retAddr = memory.Read32(esp);
        _output.WriteLine($"Return address from stack: 0x{retAddr:X8}, ESP=0x{esp:X8}");
        Assert.Equal(0x00400005u, retAddr);

        // 2. Clean up stack (return address + args)
        var argBytes = 8;  // simulate LoadCursorA with 2 uint args
        esp += 4 + (uint)argBytes;
        cpu.SetRegister("ESP", esp);

        // 3. Set EIP to return address
        cpu.SetEip(retAddr);
        cpu.SetRegister("EAX", 0x00017F00);  // simulate return value

        _output.WriteLine($"After simulated return: EIP=0x{cpu.GetEip():X8}, ESP=0x{esp:X8}");

        // 4. Execute next instruction (should be NOP at 0x00400005)
        var step2 = cpu.SingleStep(memory);
        _output.WriteLine($"After next instruction: EIP=0x{cpu.GetEip():X8}");
        Assert.Equal(0x00400006u, cpu.GetEip());

        // 5. Execute another NOP
        var step3 = cpu.SingleStep(memory);
        Assert.Equal(0x00400007u, cpu.GetEip());
    }

    [Fact]
    public void LoopAfterManualReturn_DetectsInfiniteLoop()
    {
        // This test checks if there's something about the specific instruction sequence
        // after LoadCursorA that causes the infinite loop

        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);

        var codeBase = 0x00400000u;
        var stackBase = 0x00100000u;
        cpu.SetEip(codeBase);
        cpu.SetRegister("ESP", stackBase);

        // Try to execute many instructions after a manual return
        // Write code:
        // 00400000: CALL 0x0F000000
        // 00400005: MOV [0x43c790], EAX  (A3 90 C7 43 00) - actual instruction after LoadCursorA
        // 0040000A: NOP
        // 0040000B: HLT

        memory.Write8(codeBase + 0, 0xE8);
        memory.Write32(codeBase + 1, 0x0EBFFFFB);
        memory.Write8(codeBase + 5, 0xA3);  // MOV [mem], EAX
        memory.Write32(codeBase + 6, 0x0043C790);
        memory.Write8(codeBase + 10, 0x90);  // NOP
        memory.Write8(codeBase + 11, 0xF4);  // HLT

        // Execute CALL
        var step1 = cpu.SingleStep(memory);

        // Simulate import return
        var esp = cpu.GetRegister("ESP");
        var retAddr = memory.Read32(esp);
        esp += 4 + 8;  // clean up return address + 2 args
        cpu.SetRegister("ESP", esp);
        cpu.SetEip(retAddr);
        cpu.SetRegister("EAX", 0x00017F00);

        _output.WriteLine($"Simulated return to: EIP=0x{cpu.GetEip():X8}");

        // Try to execute next 1000 instructions and detect if we're stuck
        var maxInstructions = 1000;
        var lastEip = cpu.GetEip();
        var stuckCount = 0;

        for (int i = 0; i < maxInstructions; i++)
        {
            var currentEip = cpu.GetEip();
            
            if (currentEip == 0x00400007 || currentEip == 0x0040000B)
            {
                // Reached HLT - success!
                _output.WriteLine($"SUCCESS: Reached HLT at instruction {i}, EIP=0x{currentEip:X8}");
                Assert.True(i < 10, "Should reach HLT quickly, not after many iterations");
                return;
            }

            if (currentEip == lastEip)
            {
                stuckCount++;
                if (stuckCount > 10)
                {
                    Assert.Fail($"INFINITE LOOP DETECTED: EIP stuck at 0x{currentEip:X8} for {stuckCount} iterations");
                }
            }
            else
            {
                stuckCount = 0;
            }

            lastEip = currentEip;

            try
            {
                var step = cpu.SingleStep(memory);
                if (i % 100 == 0)
                {
                    _output.WriteLine($"Instruction {i}: EIP=0x{cpu.GetEip():X8}");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Exception at instruction {i}, EIP=0x{currentEip:X8}: {ex.Message}");
                throw;
            }
        }

        Assert.Fail($"Did not reach HLT after {maxInstructions} instructions. Last EIP: 0x{cpu.GetEip():X8}");
    }

    [Fact]
    public void InvalidInstruction_ShouldThrowException()
    {
        // This test verifies that INVALID instructions halt execution by throwing an exception
        // This prevents the cascading stack corruption described in the issue

        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);

        var codeBase = 0x00400000u;
        var stackBase = 0x00100000u;
        cpu.SetEip(codeBase);
        cpu.SetRegister("ESP", stackBase);

        // Write invalid instruction bytes (0xFF 0xFF is INVALID)
        memory.Write8(codeBase + 0, 0xFF);
        memory.Write8(codeBase + 1, 0xFF);

        // Attempting to execute the INVALID instruction should throw an exception
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            cpu.SingleStep(memory);
        });

        _output.WriteLine($"Exception message: {exception.Message}");
        Assert.Contains("INVALID instruction", exception.Message);
        Assert.Contains($"0x{codeBase:X8}", exception.Message);
    }

    [Fact]
    public void InvalidInstruction_PreventsStackCorruption()
    {
        // This test verifies that execution halts on the FIRST INVALID instruction
        // and doesn't continue to cause cascading corruption

        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);

        var codeBase = 0x00400000u;
        var stackBase = 0x00100000u;
        cpu.SetEip(codeBase);
        cpu.SetRegister("ESP", stackBase);

        // Write a sequence of invalid instruction bytes
        for (uint i = 0; i < 100; i++)
        {
            memory.Write8(codeBase + i, 0xFF);
        }

        // First execution should throw
        Assert.Throws<InvalidOperationException>(() =>
        {
            cpu.SingleStep(memory);
        });

        // ESP should still be at its original value - not corrupted
        var espAfter = cpu.GetRegister("ESP");
        Assert.Equal(stackBase, espAfter);

        // EIP should have been advanced by the decoder, but execution should have halted
        // The important thing is that we didn't continue executing dozens of INVALID instructions
        _output.WriteLine($"ESP after INVALID: 0x{espAfter:X8} (expected 0x{stackBase:X8})");
        _output.WriteLine($"EIP after INVALID: 0x{cpu.GetEip():X8}");
    }
}
