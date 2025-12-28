using Xunit;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for arithmetic operations to verify WASM vs native behavior.
/// This specifically tests the ign_teas loop counter calculation issue.
/// </summary>
public class ArithmeticOperationTests
{
    [Theory]
    [InlineData(0x00100000, 16)]   // 1MB file = 16 blocks of 64KB
    [InlineData(0x00060000, 6)]    // 384KB file = 6 blocks
    [InlineData(0x00010000, 1)]    // 64KB file = 1 block
    [InlineData(0x00001000, 1)]    // 4KB file = 1 block (rounds up)
    [InlineData(0x00000001, 1)]    // 1 byte file = 1 block (rounds up)
    public void TestBlockCountCalculation_ShouldMatchExpected(uint fileSize, uint expectedBlocks)
    {
        // This tests the calculation: uVar8 = (sVar3 + 0xFFFF) >> 0x10
        // Which should calculate the number of 64KB blocks needed
        
        // Simulate the calculation
        uint blockCount = (fileSize + 0xFFFF) >> 0x10;
        
        Assert.Equal(expectedBlocks, blockCount);
    }
    
    [Fact]
    public void TestBlockCountCalculation_WithCpuEmulation()
    {
        // Test with actual CPU emulation to see if instructions execute correctly
        var vm = new VirtualMemory(0x1000000);
        var cpu = new IcedCpu(vm, NullLogger<IcedCpu>.Instance, Iced.Intel.DecoderOptions.None, false, 0x400000, 0, 0x100000);
        
        // Set up: EAX = file size (1MB = 0x100000)
        cpu.SetRegister("EAX", 0x100000);
        
        // Emulate: ADD EAX, 0xFFFF
        vm.Write8(0x400000, 0x05);  // ADD EAX, imm32
        vm.Write32(0x400001, 0xFFFF);
        
        // Execute ADD
        cpu.SingleStep(vm);
        var afterAdd = cpu.GetRegister("EAX");
        
        // Should be 0x10FFFF
        Assert.Equal(0x10FFFFu, afterAdd);
        
        // Emulate: SHR EAX, 0x10
        vm.Write8(0x400005, 0xC1);  // SHR EAX, imm8
        vm.Write8(0x400006, 0xE8);  // ModRM: EAX
        vm.Write8(0x400007, 0x10);  // shift count = 16
        
        // Execute SHR
        cpu.SingleStep(vm);
        var result = cpu.GetRegister("EAX");
        
        // Should be 16 (0x10FFFF >> 16 = 0x10)
        Assert.Equal(16u, result);
    }
}
