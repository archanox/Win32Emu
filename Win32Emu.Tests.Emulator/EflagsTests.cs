using Xunit;
using Xunit.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests to verify EFLAGS are calculated correctly
/// </summary>
public class EflagsTests
{
	private readonly ITestOutputHelper _output;
	
	public EflagsTests(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void ADD_ShouldSetCarryFlag_WhenOverflow()
	{
		// Arrange
		var memory = new VirtualMemory();
		// ADD EAX, EBX where result overflows
		memory.Write8(0x1000, 0x01); // ADD EAX, EBX (01 D8)
		memory.Write8(0x1001, 0xD8);
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xFFFFFFFF);
		cpu.SetRegister("EBX", 0x00000001);
		cpu.SetRegister("EFLAGS", 0x00000000); // Clear all flags
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var eflags = cpu.GetRegister("EFLAGS");
		var cf = (eflags & (1 << 0)) != 0; // Carry flag
		var zf = (eflags & (1 << 6)) != 0; // Zero flag
		var sf = (eflags & (1 << 7)) != 0; // Sign flag
		var of = (eflags & (1 << 11)) != 0; // Overflow flag
		var pf = (eflags & (1 << 2)) != 0; // Parity flag
		
		_output.WriteLine($"EAX result: 0x{cpu.GetRegister("EAX"):X8}");
		_output.WriteLine($"EFLAGS: 0x{eflags:X8}");
		_output.WriteLine($"CF={cf}, ZF={zf}, SF={sf}, OF={of}, PF={pf}");
		
		// 0xFFFFFFFF + 0x00000001 = 0x00000000 (with carry)
		Assert.Equal(0x00000000u, cpu.GetRegister("EAX"));
		Assert.True(cf, "Carry flag should be set");
		Assert.True(zf, "Zero flag should be set"); 
		Assert.False(sf, "Sign flag should be clear");
		// PF should be set (even parity - all bits are 0)
		Assert.True(pf, "Parity flag should be set");
	}
	
	[Fact]
	public void ADD_ShouldSetSignFlag_WhenResultNegative()
	{
		// Arrange
		var memory = new VirtualMemory();
		// ADD EAX, EBX
		memory.Write8(0x1000, 0x01); // ADD EAX, EBX (01 D8)
		memory.Write8(0x1001, 0xD8);
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x7FFFFFFF);
		cpu.SetRegister("EBX", 0x00000001);
		cpu.SetRegister("EFLAGS", 0x00000000);
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var eflags = cpu.GetRegister("EFLAGS");
		var sf = (eflags & (1 << 7)) != 0; // Sign flag
		var of = (eflags & (1 << 11)) != 0; // Overflow flag
		
		_output.WriteLine($"EAX result: 0x{cpu.GetRegister("EAX"):X8}");
		_output.WriteLine($"EFLAGS: 0x{eflags:X8}");
		_output.WriteLine($"SF={sf}, OF={of}");
		
		// 0x7FFFFFFF + 0x00000001 = 0x80000000 (negative in signed interpretation)
		Assert.Equal(0x80000000u, cpu.GetRegister("EAX"));
		Assert.True(sf, "Sign flag should be set (MSB=1)");
		Assert.True(of, "Overflow flag should be set (signed overflow)");
	}
	
	[Fact]
	public void NOP_ShouldNotModifyEflags()
	{
		// Arrange
		var memory = new VirtualMemory();
		memory.Write8(0x1000, 0x90); // NOP
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EFLAGS", 0xFFFC0846); // Set some flags
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var eflags = cpu.GetRegister("EFLAGS");
		_output.WriteLine($"EFLAGS: 0x{eflags:X8}");
		
		// NOP should not modify EFLAGS
		Assert.Equal(0xFFFC0846u, eflags);
	}
	
	[Fact]
	public void ADD_ShouldPreserveReservedBits_WhenInitializedWithHighBits()
	{
		// Arrange - This mimics conformance test pattern
		var memory = new VirtualMemory();
		// ADD EAX, EBX - simple instruction
		memory.Write8(0x1000, 0x01);
		memory.Write8(0x1001, 0xD8);
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x00000001);
		cpu.SetRegister("EBX", 0x00000001);
		// Set EFLAGS with high bits set (like in conformance tests)
		cpu.SetRegister("EFLAGS", 0xFFFC0000); // High bits set, low flags clear
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var eflags = cpu.GetRegister("EFLAGS");
		_output.WriteLine($"Initial EFLAGS: 0xFFFC0000");
		_output.WriteLine($"Final EFLAGS:   0x{eflags:X8}");
		
		// The high bits (0xFFFC0000) should be preserved
		// Low bits should have: no CF, no ZF, no SF, no OF, PF depends on result
		var highBits = eflags & 0xFFFF0000;
		Assert.Equal(0xFFFC0000u, highBits);
	}
}
