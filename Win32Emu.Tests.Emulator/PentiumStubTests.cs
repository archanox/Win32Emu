using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests to verify that all Pentium CPU mnemonics are at least recognized/stubbed in JitCpu
/// </summary>
public class PentiumStubTests
{
	private readonly ITestOutputHelper _output;

	public PentiumStubTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void JitCpu_ShouldRecognizeConditionalJumps()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// JE (0x74 0x10 - jump if equal, relative offset 0x10)
		mem.Write8(0x1000, 0x74);
		mem.Write8(0x1001, 0x10);
		
		// Act - should not throw, just log
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP should advance past the instruction
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ShouldRecognizeBitManipulation()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// BSF EAX, EBX (0x0F 0xBC 0xC3)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0xBC);
		mem.Write8(0x1002, 0xC3);
		
		// Act - should not throw, just log
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP should advance past the instruction
		Assert.Equal(0x1003u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ShouldRecognizeMMXInstructions()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// EMMS (0x0F 0x77 - Empty MMX state)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x77);
		
		// Act - should not throw, just log
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP should advance past the instruction
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ShouldRecognizeFPUInstructions()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// FNINIT (0xDB 0xE3 - Initialize FPU without checking exceptions)
		mem.Write8(0x1000, 0xDB);
		mem.Write8(0x1001, 0xE3);
		
		// Act - should not throw, just log
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP should advance past the instruction
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ShouldRecognizeSystemInstructions()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// HLT (0xF4 - Halt)
		mem.Write8(0x1000, 0xF4);
		
		// Act - should not throw, just log
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP should advance past the instruction
		Assert.Equal(0x1001u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ShouldRecognizeShiftDouble()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// SHLD EAX, EBX, 1 (0x0F 0xA4 0xC3 0x01)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0xA4);
		mem.Write8(0x1002, 0xC3);
		mem.Write8(0x1003, 0x01);
		
		// Act - should not throw, just log
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP should advance past the instruction
		Assert.Equal(0x1004u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ShouldRecognizeBCDInstructions()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// AAA (0x37 - ASCII Adjust After Addition)
		mem.Write8(0x1000, 0x37);
		
		// Act - should not throw, just log
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP should advance past the instruction
		Assert.Equal(0x1001u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ShouldRecognizeConditionalMoves()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// CMOVAE EAX, EBX (0x0F 0x43 0xC3 - conditional move if above or equal)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x43);
		mem.Write8(0x1002, 0xC3);
		
		// Act - should not throw, just log
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP should advance past the instruction
		Assert.Equal(0x1003u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_BasicInstructionsStillWork()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x2000);
		
		// NOP (0x90)
		mem.Write8(0x1000, 0x90);
		
		// Act
		var result1 = cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x1001u, cpu.GetEip());
		Assert.False(result1.IsCall);
		
		// Test INT3 (0xCC)
		mem.Write8(0x1001, 0xCC);
		var result2 = cpu.SingleStep(mem);
		
		Assert.Equal(0x1002u, cpu.GetEip());
		Assert.False(result2.IsCall);
	}

	[Fact]
	public void JitCpu_CallAndRetStillWork()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x2000);
		
		// CALL rel32 (0xE8 + 4-byte offset)
		// Call to 0x1010 (offset = 0x1010 - 0x1005 = 0x0B)
		mem.Write8(0x1000, 0xE8);
		mem.Write32(0x1001, 0x0B); // offset
		
		// Act - execute CALL
		var result1 = cpu.SingleStep(mem);
		
		// Assert CALL
		Assert.True(result1.IsCall);
		Assert.Equal(0x1010u, cpu.GetEip()); // Should jump to target
		Assert.Equal(0x1FFCu, cpu.GetRegister("ESP")); // Stack should grow
		Assert.Equal(0x1005u, mem.Read32(0x1FFC)); // Return address pushed
		
		// Now test RET
		cpu.SetEip(0x1010);
		mem.Write8(0x1010, 0xC3); // RET
		
		var result2 = cpu.SingleStep(mem);
		
		// Assert RET
		Assert.False(result2.IsCall);
		Assert.Equal(0x1005u, cpu.GetEip()); // Should return to saved address
		Assert.Equal(0x2000u, cpu.GetRegister("ESP")); // Stack should shrink back
	}
}
