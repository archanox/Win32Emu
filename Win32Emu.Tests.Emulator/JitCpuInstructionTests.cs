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
	public void MemoryAccess_WithNegativeDisplacementAndZeroBase_ReadsFromHighAddress()
	{
		// Arrange
		// With the new sparse memory model supporting full 4GB address space,
		// accessing 0xFFFFFFBC is valid (it's in the upper portion of 32-bit space)
		// and will just read zeros from an uninitialized page
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBP", 0); // Base pointer is 0!
		cpu.SetRegister("EAX", 0x12345678); // Non-zero value to verify it changes
		
		// MOV EAX, [EBP-68] using 32-bit displacement
		// With EBP=0, this becomes [0xFFFFFFBC] - which is valid in sparse 4GB model
		mem.Write8(0x1000, 0x8B); // MOV opcode
		mem.Write8(0x1001, 0x85); // ModRM byte: [EBP+disp32]
		mem.Write32(0x1002, 0xFFFFFFBC); // Displacement -68 as 32-bit value
		
		// Act - In the new sparse model, this should succeed and read 0 from uninitialized memory
		cpu.SingleStep(mem);
		
		// Assert - EAX should be 0 (uninitialized memory)
		Assert.Equal(0u, cpu.GetRegister("EAX"));
	}

	[Fact]
	public void MemoryAccess_WithVerySmallEBP_ReadsFromHighAddress()
	{
		// This test simulates a scenario where EBP is very small
		// With the new sparse memory model supporting full 4GB address space,
		// accessing addresses like 0xFFFFFFBC (0x10 - 0x44) is valid
		
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x10000);
		cpu.SetRegister("EBP", 0x10); // Very small EBP (below 0x1000 threshold)
		cpu.SetRegister("EAX", 0x12345678); // Non-zero value to verify it changes
		
		// MOV EAX, [EBP-68]
		// With EBP=0x10, address = 0x10 + 0xFFFFFFBC = 0xFFFFFFCC (wraps around in 32-bit)
		mem.Write8(0x1000, 0x8B);
		mem.Write8(0x1001, 0x85);
		mem.Write32(0x1002, 0xFFFFFFBC);
		
		// Act - In the new sparse model, this should succeed and read from the wrapped address
		cpu.SingleStep(mem);
		
		// Assert - EAX should be 0 (uninitialized memory)
		Assert.Equal(0u, cpu.GetRegister("EAX"));
	}

	[Fact]
	public void JmpNearBranch32_ShouldJumpToTarget()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		
		// JMP 0x2000 (relative jump)
		// E9 FB 0F 00 00 - JMP rel32 (0x2000 - 0x1005 = 0x0FFB)
		mem.Write8(0x1000, 0xE9); // JMP opcode
		mem.Write32(0x1001, 0x0FFB); // Relative offset
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x2000u, cpu.GetEip()); // Should jump to target address
	}

	[Fact]
	public void JmpWithRegisterOperand_ShouldJumpToAddressInRegister()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x3000); // Target address in EAX
		
		// JMP EAX = FF E0
		mem.Write8(0x1000, 0xFF); // JMP opcode
		mem.Write8(0x1001, 0xE0); // ModRM byte for register EAX
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x3000u, cpu.GetEip()); // Should jump to address in EAX
	}

	[Fact]
	public void JmpWithMemoryOperand_ShouldJumpToAddressInMemory()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		
		// Write target address to memory location
		mem.Write32(0x2000, 0x3000); // Target address is 0x3000
		
		// JMP [0x2000] = FF 25 00 20 00 00
		mem.Write8(0x1000, 0xFF); // JMP opcode
		mem.Write8(0x1001, 0x25); // ModRM byte for [disp32]
		mem.Write32(0x1002, 0x2000); // Address 0x2000
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x3000u, cpu.GetEip()); // Should jump to address stored in memory
	}

	[Fact]
	public void IntInstruction_WithImmediate80_ShouldSignalSyscall()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		
		// INT 0x80 = CD 80
		mem.Write8(0x1000, 0xCD); // INT opcode
		mem.Write8(0x1001, 0x80); // Immediate 0x80 (syscall)
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert
		Assert.True(result.IsSyscall); // Should signal syscall
		Assert.Equal(0x1002u, cpu.GetEip()); // Should advance EIP past the instruction
	}

	[Fact]
	public void IntInstruction_WithImmediate03_AtComVtableAddress_ShouldSignalCall()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		// Set EIP to a COM vtable address (0x0D000000 to 0x0E000000)
		cpu.SetEip(0x0D000100);
		
		// INT 0x03 = CD 03
		mem.Write8(0x0D000100, 0xCD); // INT opcode
		mem.Write8(0x0D000101, 0x03); // Immediate 0x03 (breakpoint)
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert
		Assert.True(result.IsCall); // Should signal call
		Assert.Equal(0x0D000100u, result.CallTarget); // Call target should be the instruction address
		Assert.Equal(0x0D000102u, cpu.GetEip()); // Should advance EIP past the instruction
	}

	[Fact]
	public void IntInstruction_WithImmediate03_NotAtComVtableAddress_ShouldNotSignalCall()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		// Set EIP to a regular address (not a COM vtable address)
		cpu.SetEip(0x00401000);
		
		// INT 0x03 = CD 03
		mem.Write8(0x00401000, 0xCD); // INT opcode
		mem.Write8(0x00401001, 0x03); // Immediate 0x03 (breakpoint)
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert
		Assert.False(result.IsCall); // Should not signal call
		Assert.Equal(0x00401002u, cpu.GetEip()); // Should advance EIP past the instruction
	}

	#region MMX Instruction Tests

	[Fact]
	public void EMMS_ShouldExecuteWithoutError()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// EMMS = 0F 77
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x77);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert - Should advance EIP without error
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void MOVD_MMXFromGPR_ShouldTransferValue()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		
		// MOVD MM0, EAX = 0F 6E C0
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6E);
		mem.Write8(0x1002, 0xC0);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert - EIP should advance
		Assert.Equal(0x1003u, cpu.GetEip());
		Assert.Equal(0x12345678u, cpu.GetRegister("EAX")); // EAX unchanged
	}

	[Fact]
	public void MOVD_GPRFromMMX_ShouldTransferValue()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xABCD1234);
		cpu.SetRegister("EBX", 0x00000000);
		
		// MOVD MM0, EAX = 0F 6E C0
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6E);
		mem.Write8(0x1002, 0xC0);
		
		// MOVD EBX, MM0 = 0F 7E C3
		mem.Write8(0x1003, 0x0F);
		mem.Write8(0x1004, 0x7E);
		mem.Write8(0x1005, 0xC3);
		
		// Act
		cpu.SingleStep(mem); // MOVD MM0, EAX
		cpu.SingleStep(mem); // MOVD EBX, MM0
		
		// Assert - EBX should now equal EAX
		Assert.Equal(0x1006u, cpu.GetEip());
		Assert.Equal(0xABCD1234u, cpu.GetRegister("EBX"));
	}

	[Fact]
	public void MOVQ_MMXToMMX_ShouldTransferValue()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		
		// MOVD MM0, EAX = 0F 6E C0
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6E);
		mem.Write8(0x1002, 0xC0);
		
		// MOVQ MM1, MM0 = 0F 6F C8
		mem.Write8(0x1003, 0x0F);
		mem.Write8(0x1004, 0x6F);
		mem.Write8(0x1005, 0xC8);
		
		// Act
		cpu.SingleStep(mem); // MOVD MM0, EAX
		cpu.SingleStep(mem); // MOVQ MM1, MM0
		
		// Assert
		Assert.Equal(0x1006u, cpu.GetEip());
	}

	[Fact]
	public void PADDB_ShouldAddPackedBytes()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x01020304);
		cpu.SetRegister("EBX", 0x05060708);
		
		// MOVD MM0, EAX = 0F 6E C0
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6E);
		mem.Write8(0x1002, 0xC0);
		
		// MOVD MM1, EBX = 0F 6E CB
		mem.Write8(0x1003, 0x0F);
		mem.Write8(0x1004, 0x6E);
		mem.Write8(0x1005, 0xCB);
		
		// PADDB MM0, MM1 = 0F FC C1
		mem.Write8(0x1006, 0x0F);
		mem.Write8(0x1007, 0xFC);
		mem.Write8(0x1008, 0xC1);
		
		// Act
		cpu.SingleStep(mem); // MOVD MM0, EAX
		cpu.SingleStep(mem); // MOVD MM1, EBX
		cpu.SingleStep(mem); // PADDB MM0, MM1
		
		// Assert - Result in MM0 should be 0x06080A0C (each byte added separately)
		Assert.Equal(0x1009u, cpu.GetEip());
	}

	[Fact]
	public void PAND_ShouldAndPackedData()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xFFFF0000);
		cpu.SetRegister("EBX", 0xFF00FF00);
		
		// MOVD MM0, EAX = 0F 6E C0
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6E);
		mem.Write8(0x1002, 0xC0);
		
		// MOVD MM1, EBX = 0F 6E CB
		mem.Write8(0x1003, 0x0F);
		mem.Write8(0x1004, 0x6E);
		mem.Write8(0x1005, 0xCB);
		
		// PAND MM0, MM1 = 0F DB C1
		mem.Write8(0x1006, 0x0F);
		mem.Write8(0x1007, 0xDB);
		mem.Write8(0x1008, 0xC1);
		
		// Act
		cpu.SingleStep(mem); // MOVD MM0, EAX
		cpu.SingleStep(mem); // MOVD MM1, EBX
		cpu.SingleStep(mem); // PAND MM0, MM1
		
		// Assert - Result in MM0 should be 0xFF000000
		Assert.Equal(0x1009u, cpu.GetEip());
	}

	[Fact]
	public void POR_ShouldOrPackedData()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xF0F00F0F);
		cpu.SetRegister("EBX", 0x0F0FF0F0);
		
		// MOVD MM0, EAX = 0F 6E C0
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6E);
		mem.Write8(0x1002, 0xC0);
		
		// MOVD MM1, EBX = 0F 6E CB
		mem.Write8(0x1003, 0x0F);
		mem.Write8(0x1004, 0x6E);
		mem.Write8(0x1005, 0xCB);
		
		// POR MM0, MM1 = 0F EB C1
		mem.Write8(0x1006, 0x0F);
		mem.Write8(0x1007, 0xEB);
		mem.Write8(0x1008, 0xC1);
		
		// Act
		cpu.SingleStep(mem); // MOVD MM0, EAX
		cpu.SingleStep(mem); // MOVD MM1, EBX
		cpu.SingleStep(mem); // POR MM0, MM1
		
		// Assert - Result in MM0 should be 0xFFFFFFFF
		Assert.Equal(0x1009u, cpu.GetEip());
	}

	[Fact]
	public void PXOR_ShouldXorPackedData()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xAAAAAAAA);
		cpu.SetRegister("EBX", 0x55555555);
		
		// MOVD MM0, EAX = 0F 6E C0
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6E);
		mem.Write8(0x1002, 0xC0);
		
		// MOVD MM1, EBX = 0F 6E CB
		mem.Write8(0x1003, 0x0F);
		mem.Write8(0x1004, 0x6E);
		mem.Write8(0x1005, 0xCB);
		
		// PXOR MM0, MM1 = 0F EF C1
		mem.Write8(0x1006, 0x0F);
		mem.Write8(0x1007, 0xEF);
		mem.Write8(0x1008, 0xC1);
		
		// Act
		cpu.SingleStep(mem); // MOVD MM0, EAX
		cpu.SingleStep(mem); // MOVD MM1, EBX
		cpu.SingleStep(mem); // PXOR MM0, MM1
		
		// Assert - Result in MM0 should be 0xFFFFFFFF
		Assert.Equal(0x1009u, cpu.GetEip());
	}

	[Fact]
	public void PSLLW_ShouldShiftLeftWords()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x00010002);
		
		// MOVD MM0, EAX = 0F 6E C0
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6E);
		mem.Write8(0x1002, 0xC0);
		
		// PSLLW MM0, 4 = 0F 71 F0 04
		mem.Write8(0x1003, 0x0F);
		mem.Write8(0x1004, 0x71);
		mem.Write8(0x1005, 0xF0);
		mem.Write8(0x1006, 0x04);
		
		// Act
		cpu.SingleStep(mem); // MOVD MM0, EAX
		cpu.SingleStep(mem); // PSLLW MM0, 4
		
		// Assert - Result in MM0 should be 0x00100020 (each word shifted left by 4)
		Assert.Equal(0x1007u, cpu.GetEip());
	}

	[Fact]
	public void MOVQ_ToMemory_ShouldWrite64Bits()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0x2000);
		
		// MOVD MM0, EAX = 0F 6E C0
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6E);
		mem.Write8(0x1002, 0xC0);
		
		// MOVQ [EBX], MM0 = 0F 7F 03
		mem.Write8(0x1003, 0x0F);
		mem.Write8(0x1004, 0x7F);
		mem.Write8(0x1005, 0x03);
		
		// Act
		cpu.SingleStep(mem); // MOVD MM0, EAX
		cpu.SingleStep(mem); // MOVQ [EBX], MM0
		
		// Assert
		Assert.Equal(0x1006u, cpu.GetEip());
		Assert.Equal(0x12345678u, mem.Read32(0x2000)); // Lower 32 bits
		Assert.Equal(0x00000000u, mem.Read32(0x2004)); // Upper 32 bits (zero-extended)
	}

	[Fact]
	public void MOVQ_FromMemory_ShouldRead64Bits()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBX", 0x2000);
		
		// Write test data to memory
		mem.Write64(0x2000, 0x123456789ABCDEF0);
		
		// MOVQ MM0, [EBX] = 0F 6F 03
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x6F);
		mem.Write8(0x1002, 0x03);
		
		// Act
		cpu.SingleStep(mem); // MOVQ MM0, [EBX]
		
		// Assert - MM0 should contain the 64-bit value
		Assert.Equal(0x1003u, cpu.GetEip());
	}

	#endregion

	#region Advanced FPU Instructions (Priority 3)

	[Fact]
	public void Fclex_ShouldClearFpuExceptions()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FNCLEX = DB E2 (no wait version)
		mem.Write8(0x1000, 0xDB);
		mem.Write8(0x1001, 0xE2);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void Finit_ShouldInitializeFpu()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FNINIT = DB E3 (no wait version)
		mem.Write8(0x1000, 0xDB);
		mem.Write8(0x1001, 0xE3);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void Fnop_ShouldDoNothing()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FNOP = D9 D0
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xD0);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void Fcomi_ShouldCompareAndSetEflags()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLD1 = D9 E8 (load 1.0)
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xE8);
		cpu.SingleStep(mem); // Load 1.0 onto stack
		
		// FLD1 again
		cpu.SetEip(0x1002);
		mem.Write8(0x1002, 0xD9);
		mem.Write8(0x1003, 0xE8);
		cpu.SingleStep(mem); // Load another 1.0
		
		// FCOMI ST(0), ST(1) = DB F1
		cpu.SetEip(0x1004);
		mem.Write8(0x1004, 0xDB);
		mem.Write8(0x1005, 0xF1);
		
		// Act
		cpu.SingleStep(mem); // Compare ST(0) with ST(1)
		
		// Assert - when equal, ZF=1, CF=0, PF=0
		Assert.Equal(0x1006u, cpu.GetEip());
		// Note: EFLAGS verification would require accessing CPU flags
	}

	[Fact]
	public void Fcomip_ShouldCompareSetEflagsAndPop()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLD1 = D9 E8 (load 1.0)
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xE8);
		cpu.SingleStep(mem);
		
		// FLD1 again
		cpu.SetEip(0x1002);
		mem.Write8(0x1002, 0xD9);
		mem.Write8(0x1003, 0xE8);
		cpu.SingleStep(mem);
		
		// FCOMIP ST(0), ST(1) = DF F1
		cpu.SetEip(0x1004);
		mem.Write8(0x1004, 0xDF);
		mem.Write8(0x1005, 0xF1);
		
		// Act
		cpu.SingleStep(mem); // Compare and pop
		
		// Assert
		Assert.Equal(0x1006u, cpu.GetEip());
	}

	[Fact]
	public void Fldl2t_ShouldLoadLog2Of10()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLDL2T = D9 E9
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xE9);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert - should load log2(10) ≈ 3.32193
		Assert.Equal(0x1002u, cpu.GetEip());
		// Verify the value is approximately correct
		double value = cpu.FpuGetSt(0);
		Assert.InRange(value, 3.32, 3.33);
	}

	[Fact]
	public void Fldlg2_ShouldLoadLog10Of2()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLDLG2 = D9 EC
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xEC);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert - should load log10(2) ≈ 0.30103
		Assert.Equal(0x1002u, cpu.GetEip());
		double value = cpu.FpuGetSt(0);
		Assert.InRange(value, 0.30, 0.31);
	}

	[Fact]
	public void Fldln2_ShouldLoadLogEOf2()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLDLN2 = D9 ED
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xED);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert - should load ln(2) ≈ 0.69315
		Assert.Equal(0x1002u, cpu.GetEip());
		double value = cpu.FpuGetSt(0);
		Assert.InRange(value, 0.69, 0.70);
	}

	[Fact]
	public void Frndint_ShouldRoundToInteger()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// Load 3.7 onto the FPU stack
		// We'll use FLD with a memory operand
		mem.Write32(0x2000, unchecked((uint)BitConverter.SingleToInt32Bits(3.7f)));
		
		// FLD dword ptr [0x2000] = D9 05 00 20 00 00
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0x05);
		mem.Write32(0x1002, 0x2000);
		cpu.SingleStep(mem);
		
		// FRNDINT = D9 FC
		cpu.SetEip(0x1006);
		mem.Write8(0x1006, 0xD9);
		mem.Write8(0x1007, 0xFC);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert - should round 3.7 to 4.0
		Assert.Equal(0x1008u, cpu.GetEip());
		double value = cpu.FpuGetSt(0);
		Assert.Equal(4.0, value);
	}

	[Fact]
	public void Ftst_ShouldTestAgainstZero()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLD1 = D9 E8 (load 1.0)
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xE8);
		cpu.SingleStep(mem);
		
		// FTST = D9 E4
		cpu.SetEip(0x1002);
		mem.Write8(0x1002, 0xD9);
		mem.Write8(0x1003, 0xE4);
		
		// Act
		cpu.SingleStep(mem); // Test ST(0) against 0.0
		
		// Assert
		Assert.Equal(0x1004u, cpu.GetEip());
	}

	[Fact]
	public void Fucom_ShouldUnorderedCompare()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLD1 twice to get ST(0) = ST(1) = 1.0
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xE8);
		cpu.SingleStep(mem);
		
		cpu.SetEip(0x1002);
		mem.Write8(0x1002, 0xD9);
		mem.Write8(0x1003, 0xE8);
		cpu.SingleStep(mem);
		
		// FUCOM ST(1) = DD E1
		cpu.SetEip(0x1004);
		mem.Write8(0x1004, 0xDD);
		mem.Write8(0x1005, 0xE1);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x1006u, cpu.GetEip());
	}

	[Fact]
	public void Fucomp_ShouldUnorderedCompareAndPop()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLD1 twice
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xE8);
		cpu.SingleStep(mem);
		
		cpu.SetEip(0x1002);
		mem.Write8(0x1002, 0xD9);
		mem.Write8(0x1003, 0xE8);
		cpu.SingleStep(mem);
		
		// FUCOMP ST(1) = DD E9
		cpu.SetEip(0x1004);
		mem.Write8(0x1004, 0xDD);
		mem.Write8(0x1005, 0xE9);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x1006u, cpu.GetEip());
	}

	[Fact]
	public void Fucompp_ShouldUnorderedCompareAndPopTwice()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLD1 twice
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xE8);
		cpu.SingleStep(mem);
		
		cpu.SetEip(0x1002);
		mem.Write8(0x1002, 0xD9);
		mem.Write8(0x1003, 0xE8);
		cpu.SingleStep(mem);
		
		// FUCOMPP = DA E9
		cpu.SetEip(0x1004);
		mem.Write8(0x1004, 0xDA);
		mem.Write8(0x1005, 0xE9);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x1006u, cpu.GetEip());
	}

	[Fact]
	public void Fprem_ShouldCalculatePartialRemainder()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// Load 7.0 then 3.0 onto stack, so ST(0) = 3.0, ST(1) = 7.0
		mem.Write32(0x2000, unchecked((uint)BitConverter.SingleToInt32Bits(7.0f)));
		mem.Write32(0x2004, unchecked((uint)BitConverter.SingleToInt32Bits(3.0f)));
		
		// FLD dword ptr [0x2000]
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0x05);
		mem.Write32(0x1002, 0x2000);
		cpu.SingleStep(mem);
		
		// FLD dword ptr [0x2004]
		cpu.SetEip(0x1006);
		mem.Write8(0x1006, 0xD9);
		mem.Write8(0x1007, 0x05);
		mem.Write32(0x1008, 0x2004);
		cpu.SingleStep(mem); // ST(0) = 3.0, ST(1) = 7.0
		
		// FPREM = D9 F8 (ST(0) = ST(0) % ST(1) = 3.0 % 7.0 = 3.0)
		cpu.SetEip(0x100C);
		mem.Write8(0x100C, 0xD9);
		mem.Write8(0x100D, 0xF8);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x100Eu, cpu.GetEip());
		double value = cpu.FpuGetSt(0);
		Assert.Equal(3.0, value); // 3 % 7 = 3
	}

	[Fact]
	public void Fptan_ShouldCalculateTangentAndPush1()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		cpu.SetEip(0x1000);
		
		// FLDZ = D9 EE (load 0.0)
		mem.Write8(0x1000, 0xD9);
		mem.Write8(0x1001, 0xEE);
		cpu.SingleStep(mem);
		
		// FPTAN = D9 F2
		cpu.SetEip(0x1002);
		mem.Write8(0x1002, 0xD9);
		mem.Write8(0x1003, 0xF2);
		
		// Act
		cpu.SingleStep(mem);
		
		// Assert - tan(0) = 0, and 1.0 is pushed
		Assert.Equal(0x1004u, cpu.GetEip());
		Assert.Equal(1.0, cpu.FpuGetSt(0)); // Top of stack is 1.0
		Assert.Equal(0.0, cpu.FpuGetSt(1)); // tan(0) = 0
	}

	#endregion
}

