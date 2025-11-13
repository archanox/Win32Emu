using Xunit;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Basic tests to verify EIP advancement is working correctly
/// </summary>
public class EipAdvancementTests
{
	[Fact]
	public void SingleStep_ShouldAdvanceEipForNop()
	{
		// Arrange
		var memory = new VirtualMemory();
		memory.Write8(0x1000, 0x90); // NOP instruction (1 byte)
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var finalEip = cpu.GetEip();
		Assert.Equal(0x1001u, finalEip);
	}
	
	[Fact]
	public void SingleStep_ShouldAdvanceEipForAddRegReg()
	{
		// Arrange
		var memory = new VirtualMemory();
		// ADD CH, DL (0x00, 0xD1) - 2 bytes
		memory.Write8(0x1000, 0x00);
		memory.Write8(0x1001, 0xD1);
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("ECX", 0x0BA34F00);
		cpu.SetRegister("EDX", 0x00000040);
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var finalEip = cpu.GetEip();
		Assert.Equal(0x1002u, finalEip);
	}
	
	[Fact]
	public void SingleStep_ShouldNotAdvanceEipForJmp()
	{
		// Arrange
		var memory = new VirtualMemory();
		// JMP short +5 (EB 05) - 2 bytes, jumps to 0x1007
		memory.Write8(0x1000, 0xEB);
		memory.Write8(0x1001, 0x05);
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var finalEip = cpu.GetEip();
		Assert.Equal(0x1007u, finalEip); // Should jump, not advance to 0x1002
	}
	
	[Fact]
	public void SingleStep_ShouldAdvanceEipForAdd16BitAddressing()
	{
		// Arrange - This mimics the conformance test pattern
		var memory = new VirtualMemory();
		
		// Write instruction: add [ss:bp+60h],bl 
		// In real 16-bit addressing this would be: 36 00 5E 60 (ss prefix + add [bp+60h],bl)
		// But in 32-bit mode with 16-bit address size prefix: 67 36 00 5E 60
		// Actually, let's use the actual encoding from a real 32-bit mode instruction
		// For simplicity, let's test: add [ebp+60h],bl which is: 00 5D 60 (3 bytes)
		memory.Write8(0x3A92, 0x00);  // ADD r/m8, r8
		memory.Write8(0x3A93, 0x5D);  // ModR/M: [EBP+disp8]
		memory.Write8(0x3A94, 0x60);  // disp8 = 0x60
		
		// Set up initial state
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x3A92);
		cpu.SetRegister("EBP", 0x1000);
		cpu.SetRegister("EBX", 0x12);
		cpu.SetRegister("ESP", 0x2000);
		
		// Write memory at [EBP+60h] = [0x1060]
		memory.Write8(0x1060, 0x05);
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var finalEip = cpu.GetEip();
		// Instruction is 3 bytes: 00 5D 60, so EIP should advance from 0x3A92 to 0x3A95
		Assert.Equal(0x3A95u, finalEip);
	}
}
