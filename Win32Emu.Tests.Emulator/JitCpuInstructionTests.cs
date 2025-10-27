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

	[Fact]
	public void MovsbInstruction_ShouldCopyByteAndIncrementRegisters()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESI", 0x2000); // Source address
		cpu.SetRegister("EDI", 0x3000); // Destination address
		cpu.SetRegister("EFLAGS", 0); // Clear DF (direction flag)
		
		// Write test value at source
		mem.Write8(0x2000, 0x42); // Test byte value
		
		// MOVSB = A4
		mem.Write8(0x1000, 0xA4);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x42, mem.Read8(0x3000)); // Byte should be copied to destination
		Assert.Equal(0x2001u, cpu.GetRegister("ESI")); // ESI should be incremented
		Assert.Equal(0x3001u, cpu.GetRegister("EDI")); // EDI should be incremented
		Assert.Equal(0x1001u, cpu.GetEip()); // EIP should advance
	}

	[Fact]
	public void MovsbInstruction_WithDirectionFlag_ShouldDecrementRegisters()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESI", 0x2000); // Source address
		cpu.SetRegister("EDI", 0x3000); // Destination address
		cpu.SetRegister("EFLAGS", 0x400); // Set DF (direction flag, bit 10)
		
		// Write test value at source
		mem.Write8(0x2000, 0x55); // Test byte value
		
		// MOVSB = A4
		mem.Write8(0x1000, 0xA4);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x55, mem.Read8(0x3000)); // Byte should be copied to destination
		Assert.Equal(0x1FFFu, cpu.GetRegister("ESI")); // ESI should be decremented
		Assert.Equal(0x2FFFu, cpu.GetRegister("EDI")); // EDI should be decremented
		Assert.Equal(0x1001u, cpu.GetEip()); // EIP should advance
	}

	[Fact]
	public void StosdInstruction_ShouldStoreEaxAndIncrementEdi()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678); // Value to store
		cpu.SetRegister("EDI", 0x3000); // Destination address
		cpu.SetRegister("EFLAGS", 0); // Clear DF (direction flag)
		
		// STOSD = AB
		mem.Write8(0x1000, 0xAB);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x12345678u, mem.Read32(0x3000)); // EAX value should be stored
		Assert.Equal(0x3004u, cpu.GetRegister("EDI")); // EDI should be incremented by 4
		Assert.Equal(0x1001u, cpu.GetEip()); // EIP should advance
	}

	[Fact]
	public void StosdInstruction_WithDirectionFlag_ShouldDecrementEdi()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xDEADBEEF); // Value to store
		cpu.SetRegister("EDI", 0x3000); // Destination address
		cpu.SetRegister("EFLAGS", 0x400); // Set DF (direction flag, bit 10)
		
		// STOSD = AB
		mem.Write8(0x1000, 0xAB);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0xDEADBEEFu, mem.Read32(0x3000)); // EAX value should be stored
		Assert.Equal(0x2FFCu, cpu.GetRegister("EDI")); // EDI should be decremented by 4
		Assert.Equal(0x1001u, cpu.GetEip()); // EIP should advance
	}

	[Fact]
	public void RclWithNegativeESPDisplacement_ShouldAccessCorrectMemory()
	{
		// Arrange - This test reproduces the bug from the issue
		// RCL dword [ESP-0x44], CL should calculate address as ESP+displacement (0x001FFFB4+0xFFFFFFBC), not use displacement alone (0xFFFFFFBC)
		const int memorySize = 16 * 1024 * 1024; // 16MB like in the error
		var mem = new VirtualMemory(memorySize);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x001FFFB4); // ESP from the error log
		cpu.SetRegister("ECX", 1); // CL = 1 (rotate count)
		
		// Write a test value at [ESP-0x44]
		uint targetAddr = cpu.GetRegister("ESP") - 0x44;
		mem.Write32(targetAddr, 0x80000001); // Value with bit 31 set
		
		// RCL dword [ESP-0x44], CL = D3 94 24 BC FF FF FF
		mem.Write8(0x1000, 0xD3); // RCL opcode
		mem.Write8(0x1001, 0x94); // ModR/M (Mod=10, Reg=010, R/M=100)
		mem.Write8(0x1002, 0x24); // SIB (Scale=00, Index=100, Base=100)
		mem.Write32(0x1003, 0xFFFFFFBC); // Displacement (-0x44)
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x1007u, cpu.GetEip()); // EIP should advance past 7-byte instruction
		// Verify the memory was actually modified (not an out-of-bounds access)
		uint valueAfter = mem.Read32(targetAddr);
		Assert.NotEqual(0x80000001u, valueAfter); // Value should have been rotated
	}

	[Fact]
	public void MemoryAccess_WithNegativeDisplacement_ShouldCalculateCorrectAddress()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBP", 0x10000); // Base pointer
		
		// Write a value at [EBP-68] (0x10000 - 0x44 = 0xFFBC)
		uint expectedAddr = 0x10000 - 0x44;
		mem.Write32(expectedAddr, 0x12345678);
		
		// MOV EAX, [EBP-68] = 8B 45 BC
		// This uses a signed displacement of -68 (0xFFFFFFBC as uint)
		mem.Write8(0x1000, 0x8B); // MOV opcode
		mem.Write8(0x1001, 0x45); // ModRM byte: [EBP+disp8]
		mem.Write8(0x1002, 0xBC); // Displacement -68 (as signed byte)
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x12345678u, cpu.GetRegister("EAX")); // Should read value from [EBP-68]
		Assert.Equal(0x1003u, cpu.GetEip()); // EIP should advance
	}

	[Fact]
	public void RclInstruction_WithNegativeDisplacement_ShouldNotCrash()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBP", 0x10000); // Base pointer
		
		// Write a value at [EBP-68]
		uint targetAddr = 0x10000 - 0x44;
		mem.Write32(targetAddr, 0x80000001);
		
		// RCL DWORD PTR [EBP-68], 1
		// This instruction uses a negative displacement
		mem.Write8(0x1000, 0xD1); // RCL opcode
		mem.Write8(0x1001, 0x55); // ModRM byte: [EBP+disp8], reg=2 (RCL)
		mem.Write8(0x1002, 0xBC); // Displacement -68 (as signed byte)
		
		// Act & Assert - should not throw
		cpu.SingleStep(mem);
		
		Assert.Equal(0x1003u, cpu.GetEip()); // EIP should advance
	}

	[Fact]
	public void MemoryAccess_WithNegativeDisplacement32_ShouldCalculateCorrectAddress()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBP", 0x100); // Small base pointer to test wraparound
		
		// This test checks if large negative displacements (as uint) are handled correctly
		// MOV EAX, [EBP+0xFFFFFFBC] should be interpreted as [EBP-68]
		// If EBP=0x100, the address should be 0x100 + 0xFFFFFFBC = 0xBC (wrapping around)
		// But this is incorrect! It should be 0x100 - 68 = 0xBC (same result but by accident)
		
		// Let's use a value where the bug is more obvious
		cpu.SetRegister("EBP", 0x200); // Base pointer
		uint expectedAddr = 0x200 - 0x44; // = 0x1BC
		mem.Write32(expectedAddr, 0xDEADBEEF);
		
		// MOV EAX, [EBP-68] using 32-bit displacement
		// This would be encoded as: 8B 85 BC FF FF FF
		mem.Write8(0x1000, 0x8B); // MOV opcode
		mem.Write8(0x1001, 0x85); // ModRM byte: [EBP+disp32]
		mem.Write32(0x1002, 0xFFFFFFBC); // Displacement -68 as 32-bit value
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0xDEADBEEFu, cpu.GetRegister("EAX")); // Should read value from [EBP-68]
		Assert.Equal(0x1006u, cpu.GetEip()); // EIP should advance
	}

	[Fact]
	public void MemoryAccess_WithNegativeDisplacementAndZeroBase_ShouldThrow()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBP", 0); // Base pointer is 0!
		
		// MOV EAX, [EBP-68] using 32-bit displacement
		// With EBP=0, this becomes [0xFFFFFFBC] which is out of range
		mem.Write8(0x1000, 0x8B); // MOV opcode
		mem.Write8(0x1001, 0x85); // ModRM byte: [EBP+disp32]
		mem.Write32(0x1002, 0xFFFFFFBC); // Displacement -68 as 32-bit value
		
		// Act & Assert
		var ex = Assert.Throws<IndexOutOfRangeException>(() => cpu.SingleStep(mem));
		Assert.Contains("0xFFFFFFBC", ex.Message);
	}
}
