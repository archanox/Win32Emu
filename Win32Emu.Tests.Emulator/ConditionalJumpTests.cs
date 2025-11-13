using Xunit;
using Xunit.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests to verify conditional jumps and LOOP instructions advance EIP correctly
/// when branches are not taken
/// </summary>
public class ConditionalJumpTests
{
	private readonly ITestOutputHelper _output;
	
	public ConditionalJumpTests(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void ConditionalJump_ShouldAdvanceEip_WhenNotTaken()
	{
		// Arrange
		var memory = new VirtualMemory();
		// JZ (jump if zero) with ZF=0 (not taken)
		// Opcode: 74 05 (JZ +5)
		memory.Write8(0x1000, 0x74);
		memory.Write8(0x1001, 0x05);
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EFLAGS", 0x00000000); // ZF=0, jump should not be taken
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var eip = cpu.GetEip();
		_output.WriteLine($"EIP after JZ (not taken): 0x{eip:X8}");
		
		// Should advance by 2 bytes (instruction length), not jump
		Assert.Equal(0x1002u, eip);
	}
	
	[Fact]
	public void ConditionalJump_ShouldJump_WhenTaken()
	{
		// Arrange
		var memory = new VirtualMemory();
		// JZ (jump if zero) with ZF=1 (taken)
		// Opcode: 74 05 (JZ +5)
		memory.Write8(0x1000, 0x74);
		memory.Write8(0x1001, 0x05);
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EFLAGS", 0x00000040); // ZF=1, jump should be taken
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var eip = cpu.GetEip();
		_output.WriteLine($"EIP after JZ (taken): 0x{eip:X8}");
		
		// Should jump to 0x1002 + 5 = 0x1007
		Assert.Equal(0x1007u, eip);
	}
	
	[Fact]
	public void Loop_ShouldAdvanceEip_WhenEcxIsZero()
	{
		// Arrange
		var memory = new VirtualMemory();
		// LOOP -10 (E2 F6)
		memory.Write8(0x1000, 0xE2);
		memory.Write8(0x1001, 0xF6); // -10 in signed byte
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("ECX", 0x00000001); // Will become 0 after decrement
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var eip = cpu.GetEip();
		var ecx = cpu.GetRegister("ECX");
		_output.WriteLine($"ECX: {ecx}, EIP after LOOP (not taken): 0x{eip:X8}");
		
		// ECX should be 0, EIP should advance by 2
		Assert.Equal(0x00000000u, ecx);
		Assert.Equal(0x1002u, eip);
	}
	
	[Fact]
	public void Loop_ShouldJump_WhenEcxIsNotZero()
	{
		// Arrange
		var memory = new VirtualMemory();
		// LOOP -10 (E2 F6)
		memory.Write8(0x1000, 0xE2);
		memory.Write8(0x1001, 0xF6); // -10 in signed byte
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("ECX", 0x00000002); // Will become 1 after decrement
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var eip = cpu.GetEip();
		var ecx = cpu.GetRegister("ECX");
		_output.WriteLine($"ECX: {ecx}, EIP after LOOP (taken): 0x{eip:X8}");
		
		// ECX should be 1, EIP should jump back by 10
		Assert.Equal(0x00000001u, ecx);
		Assert.Equal(0x0FF8u, eip); // 0x1002 - 10 = 0x0FF8
	}
}
