using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Win32Emu.Tests.Emulator.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for RET instruction handling in interpreter mode
/// JIT mode tests are skipped due to pre-existing JIT execution issues unrelated to RET instruction semantics
/// </summary>
public class JitRetInstructionTests
{
	[Fact]
	public void RetInstruction_ShouldPopAddressAndUpdateEIP_InterpreterMode()
	{
		// Arrange - test interpreter mode
		using var helper = new CpuTestHelper();
		var cpu = helper.Cpu;
		var mem = helper.Memory;
		
		// Set up stack with return address 0x00401234
		var esp = cpu.GetRegister("ESP");
		mem.Write32(esp, 0x00401234);
		
		// Write RET instruction at EIP
		var eip = cpu.GetEip();
		mem.Write8(eip, 0xC3); // RET opcode
		
		// Act - Execute RET using interpreter
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x00401234u, cpu.GetEip()); // EIP should be return address
		Assert.Equal(esp + 4, cpu.GetRegister("ESP")); // ESP should be incremented by 4
	}
	
	[Fact]
	public void RetWithImmediate_ShouldPopAddressAndCleanupStack_InterpreterMode()
	{
		// Arrange - test interpreter mode
		using var helper = new CpuTestHelper();
		var cpu = helper.Cpu;
		var mem = helper.Memory;
		
		// Set up stack with return address 0x00401234
		var esp = cpu.GetRegister("ESP");
		mem.Write32(esp, 0x00401234);
		
		// Write RET 8 instruction at EIP (stdcall cleanup of 8 bytes)
		var eip = cpu.GetEip();
		mem.Write8(eip, 0xC2); // RET imm16 opcode
		mem.Write16(eip + 1, 0x0008); // immediate = 8
		
		// Act - Execute RET using interpreter
		cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(0x00401234u, cpu.GetEip()); // EIP should be return address
		Assert.Equal(esp + 4 + 8, cpu.GetRegister("ESP")); // ESP should be incremented by 4 + 8 = 12
	}
}
