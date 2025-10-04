using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for POP instruction implementation including POP to memory
/// </summary>
public class CpuPopInstructionTests
{
    [Fact]
    public void Pop_ToRegister_ShouldWork()
    {
        // Arrange
        var memory = new VirtualMemory(2 * 1024 * 1024); // 2MB to accommodate addresses
        var cpu = new IcedCpu(memory);
        
        // Set up stack with a value
        cpu.SetRegister("ESP", 0x00100000);
        memory.Write32(0x00100000, 0x12345678);
        cpu.SetEip(0x00001000);
        
        // POP EAX (opcode 0x58)
        var testCode = new byte[] { 0x58 };
        memory.WriteBytes(0x00001000, testCode);
        
        // Act
        cpu.SingleStep(memory);
        
        // Assert
        Assert.Equal(0x12345678u, cpu.GetRegister("EAX"));
        Assert.Equal(0x00100004u, cpu.GetRegister("ESP")); // ESP should increment by 4
    }
    
    [Fact]
    public void Pop_ToMemory_ShouldWork()
    {
        // Arrange
        var memory = new VirtualMemory(4 * 1024 * 1024); // 4MB to accommodate addresses
        var cpu = new IcedCpu(memory);
        
        // Set up stack with a value to pop
        cpu.SetRegister("ESP", 0x00100000);
        memory.Write32(0x00100000, 0xAABBCCDD); // Value on stack
        
        // Set EBP to point to a valid memory location where we'll pop the value
        cpu.SetRegister("EBP", 0x00200000);
        cpu.SetEip(0x00001000);
        
        // POP [EBP-4] - This pops from stack to memory location [EBP-4]
        // Opcode: 0x8F 0x45 0xFC (POP DWORD PTR [EBP-4])
        var testCode = new byte[] { 0x8F, 0x45, 0xFC };
        memory.WriteBytes(0x00001000, testCode);
        
        // Act
        cpu.SingleStep(memory);
        
        // Assert
        // The value should be popped from stack to memory location [EBP-4]
        Assert.Equal(0xAABBCCDDu, memory.Read32(0x00200000 - 4));
        // ESP should increment by 4 after pop
        Assert.Equal(0x00100004u, cpu.GetRegister("ESP"));
    }
    
    [Fact]
    public void Pop_ToMemoryWithESI_ShouldWork()
    {
        // Arrange
        var memory = new VirtualMemory(2 * 1024 * 1024); // 2MB to accommodate addresses
        var cpu = new IcedCpu(memory);
        
        // Set up stack with a value to pop
        cpu.SetRegister("ESP", 0x00100000);
        memory.Write32(0x00100000, 0x11223344); // Value on stack
        
        // Set ESI to point to a valid memory location
        cpu.SetRegister("ESI", 0x00150000);
        cpu.SetEip(0x00001000);
        
        // POP [ESI] - Opcode: 0x8F 0x06
        var testCode = new byte[] { 0x8F, 0x06 };
        memory.WriteBytes(0x00001000, testCode);
        
        // Act
        cpu.SingleStep(memory);
        
        // Assert
        // The value should be popped from stack to memory location [ESI]
        Assert.Equal(0x11223344u, memory.Read32(0x00150000));
        // ESP should increment by 4 after pop
        Assert.Equal(0x00100004u, cpu.GetRegister("ESP"));
    }
    
    [Fact]
    public void Pop_MultipleOperations_ShouldMaintainStackIntegrity()
    {
        // Arrange
        var memory = new VirtualMemory(4 * 1024 * 1024); // 4MB to accommodate addresses
        var cpu = new IcedCpu(memory);
        
        // Set up a program with push and pop operations
        cpu.SetRegister("ESP", 0x00100000);
        cpu.SetRegister("EBP", 0x00200000);
        cpu.SetRegister("EAX", 0xDEADBEEF);
        cpu.SetEip(0x00001000);
        
        // Program:
        // PUSH EAX         (0x50)
        // POP [EBP-4]      (0x8F 0x45 0xFC)
        var testCode = new byte[] 
        { 
            0x50,               // PUSH EAX
            0x8F, 0x45, 0xFC    // POP [EBP-4]
        };
        memory.WriteBytes(0x00001000, testCode);
        
        // Act
        cpu.SingleStep(memory); // PUSH EAX
        cpu.SingleStep(memory); // POP [EBP-4]
        
        // Assert
        // Value should be in memory at [EBP-4]
        Assert.Equal(0xDEADBEEFu, memory.Read32(0x00200000 - 4));
        // ESP should be back to original value after push then pop
        Assert.Equal(0x00100000u, cpu.GetRegister("ESP"));
    }
}
