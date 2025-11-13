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
	
	[Theory]
	[InlineData(0x1000, 0x0BA34F40, 0x7EEA40C1, 0xFFFC0443, 0x0BA31040u, 0xFFFC0413u, true, "conformance test")]
	[InlineData(0x1000, 0x0BA34F00, 0x000000C1, 0xFFFC0000, 0x0BA31000u, 0xFFFC0011u, false, "manual test")]
	public void ADD_8Bit_ShouldCalculateFlagsCorrectly(uint eip, uint initialEcx, uint initialEdx, uint initialEflags, 
		uint expectedEcx, uint expectedEflags, bool includeSegmentOverride, string testName)
	{
		// Test: add ch,dl
		// This validates both conformance test case (test 4 from 00.MOO.gz) and manual test case
		var memory = new VirtualMemory();
		
		if (includeSegmentOverride)
		{
			// Instruction bytes: 3E 00 D5 F4 (DS segment override + ADD CH,DL + HLT)
			memory.Write8(eip, 0x3E); // DS segment override
			memory.Write8(eip + 1, 0x00); // ADD opcode
			memory.Write8(eip + 2, 0xD5); // ModR/M byte (CH, DL)
			memory.Write8(eip + 3, 0xF4); // HLT (next instruction?)
		}
		else
		{
			// Instruction bytes: 00 D5 (ADD CH,DL)
			memory.Write8(eip, 0x00); // ADD opcode
			memory.Write8(eip + 1, 0xD5); // ModR/M byte (CH, DL)
		}
		
		var cpu = new IcedCpu(memory);
		cpu.SetEip(eip);
		cpu.SetRegister("ECX", initialEcx);
		cpu.SetRegister("EDX", initialEdx);
		cpu.SetRegister("EFLAGS", initialEflags);
		
		// Act
		cpu.SingleStep(memory);
		
		// Assert
		var ecx = cpu.GetRegister("ECX");
		var ch = (ecx >> 8) & 0xFF;
		var eflags = cpu.GetRegister("EFLAGS");
		
		_output.WriteLine($"Test: {testName}");
		_output.WriteLine($"ECX: 0x{ecx:X8}, CH: 0x{ch:X2}");
		_output.WriteLine($"Expected ECX: 0x{expectedEcx:X8}");
		_output.WriteLine($"EFLAGS: 0x{eflags:X8}");
		_output.WriteLine($"Expected EFLAGS: 0x{expectedEflags:X8}");
		
		// CH should be 0x10 (0x4F + 0xC1 = 0x110, truncated to 0x10)
		Assert.Equal(0x10u, ch);
		Assert.Equal(expectedEcx, ecx);
		Assert.Equal(expectedEflags, eflags);
		
		// Verify individual flags for documentation
		var cf = (eflags & (1 << 0)) != 0;
		var pf = (eflags & (1 << 2)) != 0;
		var af = (eflags & (1 << 4)) != 0;
		var zf = (eflags & (1 << 6)) != 0;
		var sf = (eflags & (1 << 7)) != 0;
		var of = (eflags & (1 << 11)) != 0;
		
		_output.WriteLine($"CF={cf}, PF={pf}, AF={af}, ZF={zf}, SF={sf}, OF={of}");
		
		// For 0x4F + 0xC1 = 0x110:
		// CF should be 1 (overflow from bit 7)
		// PF should be 0 (result 0x10 has odd parity: 1 bit set)
		// AF should be 1 (carry from bit 3: low nibbles 0xF (CH) + 0x1 (DL) = 0x10)
		// ZF should be 0 (result is not zero)
		// SF should be 0 (bit 7 of result is 0)
		// OF should be 0 (no signed overflow: positive + negative = positive)
		
		Assert.True(cf, "Carry flag should be set (unsigned overflow)");
		Assert.False(pf, "Parity flag should be clear (odd parity)");
		Assert.True(af, "Auxiliary flag should be set (carry from bit 3)");
		Assert.False(zf, "Zero flag should be clear (result is not zero)");
		Assert.False(sf, "Sign flag should be clear (bit 7 is 0)");
		Assert.False(of, "Overflow flag should be clear (no signed overflow)");
	}
	
	[Fact]
	public void DumpConformanceTestCase()
	{
		var testFile = System.IO.Path.Combine("TestData", "SingleStepTests", "00.MOO.gz");
		if (!System.IO.File.Exists(testFile))
		{
			_output.WriteLine("Test file not found, skipping");
			return;
		}
		
		var mooFile = SingleStepTests.MooFileParser.Parse(testFile);
		var test = mooFile.Tests[4]; // Test 4

		_output.WriteLine($"Test: {test.Name}");
		_output.WriteLine($"Initial EIP: 0x{test.InitialState.Registers.Eip:X8}");
		_output.WriteLine($"Initial ECX: 0x{test.InitialState.Registers.Ecx:X8}");
		_output.WriteLine($"Initial EDX: 0x{test.InitialState.Registers.Edx:X8}");
		_output.WriteLine($"Initial EFLAGS: 0x{test.InitialState.Registers.Eflags:X8}");
		_output.WriteLine($"Expected EIP: 0x{test.FinalState.Registers.Eip:X8}");
		_output.WriteLine($"Expected ECX: 0x{test.FinalState.Registers.Ecx:X8}");
		_output.WriteLine($"Expected EFLAGS: 0x{test.FinalState.Registers.Eflags:X8}");
		
		_output.WriteLine($"\nInstruction bytes ({test.InstructionBytes.Length}):");
		_output.WriteLine($"  {BitConverter.ToString(test.InstructionBytes)}");
		
		_output.WriteLine($"\nMemory entries:");
		foreach (var mem in test.InitialState.Memory.OrderBy(m => m.Address))
		{
			_output.WriteLine($"  [{mem.Address:X8}] = 0x{mem.Value:X2}");
		}

		var ch_init = (test.InitialState.Registers.Ecx >> 8) & 0xFF;
		var dl_init = test.InitialState.Registers.Edx & 0xFF;
		var ch_final = (test.FinalState.Registers.Ecx >> 8) & 0xFF;

		_output.WriteLine($"\nCH (initial): 0x{ch_init:X2}");
		_output.WriteLine($"DL (initial): 0x{dl_init:X2}");
		_output.WriteLine($"CH + DL = 0x{ch_init + dl_init:X3}");
		_output.WriteLine($"CH (expected): 0x{ch_final:X2}");
		
		var expectedInstrLen = test.FinalState.Registers.Eip - test.InitialState.Registers.Eip;
		_output.WriteLine($"\nExpected instruction length: {expectedInstrLen} bytes");
	}
}
