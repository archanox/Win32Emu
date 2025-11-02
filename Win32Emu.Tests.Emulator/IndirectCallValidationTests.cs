using Microsoft.Extensions.Logging;
using Win32Emu.Tests.Emulator.TestInfrastructure;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for ValidateIndirectTarget to ensure special memory ranges don't trigger false warnings
/// </summary>
public class IndirectCallValidationTests : IDisposable
{
    private readonly CpuTestHelper _helper;
    private readonly ITestOutputHelper _output;

    public IndirectCallValidationTests(ITestOutputHelper output)
    {
        _output = output;
        _helper = new CpuTestHelper();
    }

    [Fact]
    public void IndirectCall_ToImportStub_ShouldNotWarn()
    {
        // Arrange: Set up a CALL EBP where EBP points to import stub range (0x0F000000-0x10000000)
        // This should NOT generate a warning because import stubs are valid emulator infrastructure
        _helper.SetReg("EBP", 0x0F000070);
        
        // Write instruction: CALL EBP (FF D5)
        _helper.WriteCode(0xFF, 0xD5);
        
        // Write a RET at the target address so execution can return
        _helper.Memory.Write8(0x0F000070, 0xC3); // RET instruction
        
        // Act & Assert - should not throw or crash
        _helper.ExecuteInstruction(); // Execute CALL EBP
        
        // The call should have jumped to 0x0F000070
        Assert.Equal(0x0F000070u, _helper.Cpu.GetEip());
    }

    [Fact]
    public void IndirectCall_ToSyscallDispatcher_ShouldNotWarn()
    {
        // Arrange: Set up a CALL EBP where EBP points to syscall dispatcher range (0x0E000000-0x0F000000)
        // This should NOT generate a warning because syscall dispatcher is valid emulator infrastructure
        _helper.SetReg("EBP", 0x0E000002);
        
        // Write instruction: CALL EBP (FF D5)
        _helper.WriteCode(0xFF, 0xD5);
        
        // Write a RET at the target address so execution can return
        _helper.Memory.Write8(0x0E000002, 0xC3); // RET instruction
        
        // Act & Assert - should not throw or crash
        _helper.ExecuteInstruction(); // Execute CALL EBP
        
        // The call should have jumped to 0x0E000002
        Assert.Equal(0x0E000002u, _helper.Cpu.GetEip());
    }

    [Fact]
    public void IndirectCall_ToComVtable_ShouldNotWarn()
    {
        // Arrange: Set up a CALL EBP where EBP points to COM vtable range (0x0D000000-0x0E000000)
        // This should NOT generate a warning because COM vtables are valid emulator infrastructure
        _helper.SetReg("EBP", 0x0D000100);
        
        // Write instruction: CALL EBP (FF D5)
        _helper.WriteCode(0xFF, 0xD5);
        
        // Write a RET at the target address so execution can return
        _helper.Memory.Write8(0x0D000100, 0xC3); // RET instruction
        
        // Act & Assert - should not throw or crash
        _helper.ExecuteInstruction(); // Execute CALL EBP
        
        // The call should have jumped to 0x0D000100
        Assert.Equal(0x0D000100u, _helper.Cpu.GetEip());
    }

    [Fact]
    public void IndirectCall_ToNormalCodeAddress_ShouldWork()
    {
        // Arrange: Set up a CALL EBP where EBP points to a normal code address (>= 0x00400000)
        // This should NOT generate a warning because it's a normal code address
        _helper.SetReg("EBP", 0x00401000);
        
        // Write instruction: CALL EBP (FF D5)
        _helper.WriteCode(0xFF, 0xD5);
        
        // Write a RET at the target address so execution can return
        _helper.Memory.Write8(0x00401000, 0xC3); // RET instruction
        
        // Act & Assert - should not throw or crash
        _helper.ExecuteInstruction(); // Execute CALL EBP
        
        // The call should have jumped to 0x00401000
        Assert.Equal(0x00401000u, _helper.Cpu.GetEip());
    }

    public void Dispose()
    {
        _helper?.Dispose();
    }
}
