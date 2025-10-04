using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Test that verifies EBP initialization prevents address wraparound
/// </summary>
public class EbpInitializationTests
{
    [Fact]
    public void IcedCpu_WithInitializedEBP_ShouldAllowFrameRelativeAccess()
    {
        // Arrange
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        
        // Simulate the fix: initialize EBP to same value as ESP
        cpu.SetRegister("ESP", 0x00200000);
        cpu.SetRegister("EBP", 0x00200000);
        cpu.SetEip(0x00401000);
        
        // Write some test data on the stack
        // [EBP-11] = 0x00200000 - 11 = 0x001FFFF5
        memory.Write32(0x001FFFF5, 0x12345678);
        
        // Test instruction: ADD EAX, [EBP-11]
        // This is the type of instruction that was failing in the issue
        var testCode = new byte[]
        {
            0x03, 0x45, 0xF5  // ADD EAX, [EBP-11]  (F5 = -11)
        };
        
        memory.WriteBytes(0x00401000, testCode);
        
        // Act - this should NOT throw
        cpu.SingleStep(memory);
        
        // Assert - the instruction should execute successfully
        Assert.Equal(0x12345678u, cpu.GetRegister("EAX"));
    }
    
    [Fact]
    public void IcedCpu_WithUninitializedEBP_ShouldThrowOnFrameRelativeAccess()
    {
        // Arrange
        var memory = new VirtualMemory();
        var cpu = new IcedCpu(memory);
        
        // Simulate the bug: EBP is 0 (uninitialized)
        cpu.SetRegister("ESP", 0x00200000);
        cpu.SetRegister("EBP", 0x00000000); // This is the problematic state
        cpu.SetEip(0x00401000);
        
        // Test instruction: ADD EAX, [EBP-11]
        // With EBP=0, this calculates address 0xFFFFFFF5 which is out of range
        var testCode = new byte[]
        {
            0x03, 0x45, 0xF5  // ADD EAX, [EBP-11]  (F5 = -11)
        };
        
        memory.WriteBytes(0x00401000, testCode);
        
        // Act & Assert - this should throw the exact error from the issue
        var exception = Assert.Throws<IndexOutOfRangeException>(() => cpu.SingleStep(memory));
        Assert.Contains("0xFFFFFFF5", exception.Message);
    }
}
