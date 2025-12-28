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
        // Constants for x86 instruction opcodes and operands
        const uint TEST_FILE_SIZE = 0x100000; // 1MB test file
        const uint BLOCK_SIZE_MASK = 0xFFFF; // 64KB - 1
        const byte BLOCK_SHIFT = 0x10; // Shift right by 16 bits
        const byte OPCODE_ADD_EAX_IMM32 = 0x05; // ADD EAX, imm32
        const byte OPCODE_SHR_RM32_IMM8 = 0xC1; // SHR r/m32, imm8
        const byte MODRM_SHR_EAX = 0xE8; // ModR/M byte for SHR EAX
        const uint CODE_BASE_ADDRESS = 0x400000;
        const uint MEMORY_SIZE = 0x1000000; // 16MB
        const uint STACK_LIMIT = 0;
        const uint STACK_BASE = 0x100000;
        
        // Test with actual CPU emulation to see if instructions execute correctly
        var vm = new VirtualMemory(MEMORY_SIZE);
        var cpu = new IcedCpu(vm, NullLogger<IcedCpu>.Instance, Iced.Intel.DecoderOptions.None, false, CODE_BASE_ADDRESS, STACK_LIMIT, STACK_BASE);
        
        // Set up: EAX = file size (1MB = 0x100000)
        cpu.SetRegister("EAX", TEST_FILE_SIZE);
        
        // Emulate: ADD EAX, 0xFFFF
        vm.Write8(CODE_BASE_ADDRESS, OPCODE_ADD_EAX_IMM32);
        vm.Write32(CODE_BASE_ADDRESS + 1, BLOCK_SIZE_MASK);
        
        // Execute ADD
        cpu.SingleStep(vm);
        var afterAdd = cpu.GetRegister("EAX");
        
        // Should be 0x10FFFF
        Assert.Equal(0x10FFFFu, afterAdd);
        
        // Emulate: SHR EAX, 0x10
        vm.Write8(CODE_BASE_ADDRESS + 5, OPCODE_SHR_RM32_IMM8);
        vm.Write8(CODE_BASE_ADDRESS + 6, MODRM_SHR_EAX);
        vm.Write8(CODE_BASE_ADDRESS + 7, BLOCK_SHIFT);
        
        // Execute SHR
        cpu.SingleStep(vm);
        var result = cpu.GetRegister("EAX");
        
        // Should be 16 (0x10FFFF >> 16 = 0x10)
        Assert.Equal(16u, result);
    }
}
