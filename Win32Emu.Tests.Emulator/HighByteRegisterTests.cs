using Xunit;
using Xunit.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for 8-bit register operations, especially high-byte registers
/// </summary>
public class HighByteRegisterTests
{
	private readonly ITestOutputHelper _output;
	
	public HighByteRegisterTests(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void ADD_CH_DL_ShouldWorkCorrectly()
	{
		// Arrange
		var memory = new VirtualMemory();
		// ADD CH, DL - need correct encoding
		// Opcode 0x00 = ADD r/m8, r8
		// ModR/M: Mod=11 (register), Reg=010 (DL), R/M=101 (CH)
		// ModR/M = 11010101 = 0xD5
		memory.Write8(0x3A92, 0x00);
		memory.Write8(0x3A93, 0xD5);
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x3A92);
		// Initial values from conformance test
		cpu.SetRegister("ECX", 0x0BA34F00); // CH = 0x4F, CL = 0x00
		cpu.SetRegister("EDX", 0x00000040); // DL = 0x40
		cpu.SetRegister("EFLAGS", 0xFFFC0486);
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var ecx = cpu.GetRegister("ECX");
		var edx = cpu.GetRegister("EDX");
		var eip = cpu.GetEip();
		var eflags = cpu.GetRegister("EFLAGS");
		
		_output.WriteLine($"Initial: ECX=0x{0x0BA34F00:X8} (CH=0x4F), EDX=0x{0x00000040:X8} (DL=0x40)");
		_output.WriteLine($"Final:   ECX=0x{ecx:X8}, EDX=0x{edx:X8}");
		_output.WriteLine($"EIP: 0x{eip:X8} (expected 0x00003A94)");
		_output.WriteLine($"EFLAGS: 0x{eflags:X8}");
		
		var ch = (ecx >> 8) & 0xFF;
		var cl = ecx & 0xFF;
		_output.WriteLine($"CH: 0x{ch:X2}, CL: 0x{cl:X2}");
		
		// Expected: 0x4F + 0x40 = 0x8F (no carry in 8-bit)
		// ECX should be 0x0BA38F00
		Assert.Equal(0x0BA38F00u, ecx);
		Assert.Equal(0x00003A94u, eip);
	}
}
