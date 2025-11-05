using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for the specific issue reported in the problem statement:
/// Indirect CALL through EBP when EBP contains a stack address (0x001FEF10)
/// </summary>
public class IndirectCallStackAddressTests
{
	[Fact]
	public void CALL_EBP_WithStackAddress_ShouldThrowClearException()
	{
		// Arrange: This reproduces the exact scenario from the problem statement
		// At 0x0040319A, code executes "call ebp" where EBP=0x001FEF10 (stack address)
		var memory = new VirtualMemory();
		var imageBase = 0x00400000u; // Typical Win32 executable image base
		var cpu = new IcedCpu(memory, null, imageBase: imageBase);
		
		cpu.SetEip(0x0040319A);
		cpu.SetRegister("ESP", 0x001FEF0C);
		cpu.SetRegister("EBP", 0x001FEF10); // Stack address from the problem statement
		
		// Write CALL EBP instruction (2 bytes: FF D5)
		memory.Write8(0x0040319A, 0xFF); // CALL r/m32
		memory.Write8(0x0040319B, 0xD5); // ModRM: 11 010 101 = register EBP
		
		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => cpu.SingleStep(memory));
		
		// Verify exception message contains all the important details
		Assert.Contains("CALL", exception.Message);
		Assert.Contains("0x001FEF10", exception.Message);
		Assert.Contains("stack", exception.Message);
		Assert.Contains("EBP", exception.Message);
		Assert.Contains("function pointer", exception.Message);
		
		// Verify exception message mentions common causes
		Assert.Contains("Uninitialized function pointer", exception.Message);
	}
	
	[Fact]
	public void CALL_Memory_WithStackAddress_ShouldThrowClearException()
	{
		// Arrange: This tests the pattern "call [memory]" where memory contains a stack address
		// This simulates the scenario where [0x004552F8] contains 0x001FEF10
		var memory = new VirtualMemory();
		var imageBase = 0x00400000u; // Typical Win32 executable image base
		var cpu = new IcedCpu(memory, null, imageBase: imageBase);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("ESP", 0x001FEF0C);
		cpu.SetRegister("EBX", 0x004552F8); // Address that holds the function pointer
		
		// Write stack address to the memory location (simulating uninitialized IAT entry)
		memory.Write32(0x004552F8, 0x001FEF10);
		
		// Write CALL [EBX] instruction (2 bytes: FF 13)
		memory.Write8(0x00400000, 0xFF); // CALL r/m32
		memory.Write8(0x00400001, 0x13); // ModRM: 00 010 011 = memory indirect through EBX
		
		// Act & Assert
		var exception = Assert.Throws<InvalidOperationException>(() => cpu.SingleStep(memory));
		
		// Verify exception message contains important details
		Assert.Contains("CALL", exception.Message);
		Assert.Contains("0x001FEF10", exception.Message);
		Assert.Contains("stack", exception.Message);
		
		// Should mention it came from memory (not a register)
		Assert.Contains("from memory", exception.Message);
	}
	
	[Fact]
	public void CALL_Register_WithValidCodeAddress_ShouldNotThrow()
	{
		// Arrange: CALL with valid code address (>= image base)
		var memory = new VirtualMemory();
		var imageBase = 0x00400000u; // Typical Win32 executable image base
		var cpu = new IcedCpu(memory, null, imageBase: imageBase);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("ESP", 0x00100000);
		cpu.SetRegister("EAX", 0x00401000); // Valid code address (above image base)
		
		// Write CALL EAX instruction
		memory.Write8(0x00400000, 0xFF);
		memory.Write8(0x00400001, 0xD0);
		
		// Act - should not throw
		cpu.SingleStep(memory);
		
		// Assert
		Assert.Equal(0x00401000u, cpu.GetEip());
	}
	
	[Fact]
	public void CALL_Register_WithImportStubAddress_ShouldNotThrow()
	{
		// Arrange: CALL with import stub address (0x0F000000 - 0x0FFFFFFF)
		var memory = new VirtualMemory();
		var imageBase = 0x00400000u; // Typical Win32 executable image base
		var cpu = new IcedCpu(memory, null, imageBase: imageBase);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("ESP", 0x00100000);
		cpu.SetRegister("EAX", 0x0F000040); // Import stub address
		
		// Write CALL EAX instruction
		memory.Write8(0x00400000, 0xFF);
		memory.Write8(0x00400001, 0xD0);
		
		// Act - should not throw (import stubs are valid)
		cpu.SingleStep(memory);
		
		// Assert
		Assert.Equal(0x0F000040u, cpu.GetEip());
	}
}
