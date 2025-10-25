using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for specific JitCpu instruction implementations
/// </summary>
public class JitCpuInstructionTests
{
	[Fact]
	public void CallWithMemoryOperand_ShouldJumpToAddressInMemory()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x10000);
		
		// Write target address to memory location
		mem.Write32(0x2000, 0x3000); // Target address is 0x3000
		
		// CALL [0x2000] = FF 15 00 20 00 00
		mem.Write8(0x1000, 0xFF); // CALL opcode
		mem.Write8(0x1001, 0x15); // ModRM byte for [disp32]
		mem.Write32(0x1002, 0x2000); // Address 0x2000
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x3000u, cpu.GetEip()); // Should jump to address stored in memory
		Assert.Equal(0xFFFCu, cpu.GetRegister("ESP")); // Stack should be decremented by 4
		Assert.Equal(0x1006u, mem.Read32(0xFFFC)); // Return address should be on stack
	}

	[Fact]
	public void CallWithRegisterOperand_ShouldJumpToAddressInRegister()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x10000);
		cpu.SetRegister("EAX", 0x3000); // Target address in EAX
		
		// CALL EAX = FF D0
		mem.Write8(0x1000, 0xFF); // CALL opcode
		mem.Write8(0x1001, 0xD0); // ModRM byte for register EAX
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x3000u, cpu.GetEip()); // Should jump to address in EAX
		Assert.Equal(0xFFFCu, cpu.GetRegister("ESP")); // Stack should be decremented by 4
		Assert.Equal(0x1002u, mem.Read32(0xFFFC)); // Return address should be on stack
	}

	[Fact]
	public void InInstruction_ShouldReadFromPort()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xFFFFFFFF); // Set EAX to all 1s
		
		// IN AL, 0x60 = E4 60
		mem.Write8(0x1000, 0xE4); // IN opcode
		mem.Write8(0x1001, 0x60); // Port 0x60
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0xFFFFFF00u, cpu.GetRegister("EAX")); // AL should be set to 0
		Assert.Equal(0x1002u, cpu.GetEip()); // EIP should advance
	}

	[Fact]
	public void InInstruction_FromDX_ShouldReadFromPortInDX()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xFFFFFFFF); // Set EAX to all 1s
		cpu.SetRegister("EDX", 0x3F8); // Port number in DX
		
		// IN AL, DX = EC
		mem.Write8(0x1000, 0xEC); // IN opcode
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0xFFFFFF00u, cpu.GetRegister("EAX")); // AL should be set to 0
		Assert.Equal(0x1001u, cpu.GetEip()); // EIP should advance
	}

	[Fact]
	public void LoopInstruction_ShouldDecrementECXAndJump()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ECX", 5); // Loop counter
		
		// LOOP -10 (0xFF F6) - jump back 10 bytes
		mem.Write8(0x1000, 0xE2); // LOOP opcode
		mem.Write8(0x1001, 0xF6); // Relative offset -10
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(4u, cpu.GetRegister("ECX")); // ECX should be decremented
		Assert.Equal(0x0FF8u, cpu.GetEip()); // Should jump back (0x1002 - 10)
	}

	[Fact]
	public void LoopInstruction_WhenECXIsOne_ShouldNotJump()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ECX", 1); // Loop counter = 1
		
		// LOOP -10
		mem.Write8(0x1000, 0xE2); // LOOP opcode
		mem.Write8(0x1001, 0xF6); // Relative offset -10
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0u, cpu.GetRegister("ECX")); // ECX should be decremented to 0
		Assert.Equal(0x1002u, cpu.GetEip()); // Should NOT jump, just continue
	}

	[Fact]
	public void LoopneInstruction_WhenZeroFlagClear_ShouldJump()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ECX", 5); // Loop counter
		cpu.SetRegister("EFLAGS", 0); // Clear all flags, including ZF
		
		// LOOPNE -10
		mem.Write8(0x1000, 0xE0); // LOOPNE opcode
		mem.Write8(0x1001, 0xF6); // Relative offset -10
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(4u, cpu.GetRegister("ECX")); // ECX should be decremented
		Assert.Equal(0x0FF8u, cpu.GetEip()); // Should jump back
	}

	[Fact]
	public void LoopneInstruction_WhenZeroFlagSet_ShouldNotJump()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ECX", 5); // Loop counter
		cpu.SetRegister("EFLAGS", 0x40); // Set ZF (bit 6)
		
		// LOOPNE -10
		mem.Write8(0x1000, 0xE0); // LOOPNE opcode
		mem.Write8(0x1001, 0xF6); // Relative offset -10
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(4u, cpu.GetRegister("ECX")); // ECX should be decremented
		Assert.Equal(0x1002u, cpu.GetEip()); // Should NOT jump due to ZF=1
	}

	[Fact]
	public void LoopeInstruction_WhenZeroFlagSet_ShouldJump()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ECX", 5); // Loop counter
		cpu.SetRegister("EFLAGS", 0x40); // Set ZF (bit 6)
		
		// LOOPE -10
		mem.Write8(0x1000, 0xE1); // LOOPE opcode
		mem.Write8(0x1001, 0xF6); // Relative offset -10
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(4u, cpu.GetRegister("ECX")); // ECX should be decremented
		Assert.Equal(0x0FF8u, cpu.GetEip()); // Should jump back
	}

	[Fact]
	public void LoopeInstruction_WhenZeroFlagClear_ShouldNotJump()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ECX", 5); // Loop counter
		cpu.SetRegister("EFLAGS", 0); // Clear all flags, including ZF
		
		// LOOPE -10
		mem.Write8(0x1000, 0xE1); // LOOPE opcode
		mem.Write8(0x1001, 0xF6); // Relative offset -10
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(4u, cpu.GetRegister("ECX")); // ECX should be decremented
		Assert.Equal(0x1002u, cpu.GetEip()); // Should NOT jump due to ZF=0
	}
}
