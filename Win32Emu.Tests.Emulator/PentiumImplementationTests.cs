using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for fully implemented Pentium instructions in JitCpu
/// </summary>
public class PentiumImplementationTests
{
	[Fact]
	public void JitCpu_ConditionalJump_JE_ShouldJumpWhenZero()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EFLAGS", 0x40); // Set ZF (Zero Flag)
		
		// JE (0x74 0x10 - jump if equal, relative offset 0x10)
		// This should jump to 0x1012 (0x1000 + 2 for instruction length + 0x10)
		mem.Write8(0x1000, 0x74);
		mem.Write8(0x1001, 0x10);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - Should have jumped
		Assert.Equal(0x1012u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ConditionalJump_JE_ShouldNotJumpWhenNotZero()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EFLAGS", 0x00); // Clear ZF
		
		// JE (0x74 0x10)
		mem.Write8(0x1000, 0x74);
		mem.Write8(0x1001, 0x10);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - Should not have jumped, EIP advances to next instruction
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ConditionalJump_JNE_ShouldJumpWhenNotZero()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EFLAGS", 0x00); // Clear ZF
		
		// JNE (0x75 0x10 - jump if not equal)
		mem.Write8(0x1000, 0x75);
		mem.Write8(0x1001, 0x10);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - Should have jumped
		Assert.Equal(0x1012u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ConditionalJump_JA_ShouldJumpWhenAbove()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EFLAGS", 0x00); // CF=0, ZF=0 (above)
		
		// JA (0x77 0x10 - jump if above, CF=0 and ZF=0)
		mem.Write8(0x1000, 0x77);
		mem.Write8(0x1001, 0x10);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - Should have jumped
		Assert.Equal(0x1012u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_BIT_BSF_ShouldFindFirstSetBit()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBX", 0x00000018); // Binary: ...0001 1000, first set bit at position 3
		
		// BSF EAX, EBX (0x0F 0xBC 0xC3)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0xBC);
		mem.Write8(0x1002, 0xC3);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - EAX should contain 3 (first set bit position)
		Assert.Equal(3u, cpu.GetRegister("EAX"));
		Assert.Equal(0x1003u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_BIT_BSR_ShouldFindLastSetBit()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBX", 0x00000018); // Binary: ...0001 1000, last set bit at position 4
		
		// BSR EAX, EBX (0x0F 0xBD 0xC3)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0xBD);
		mem.Write8(0x1002, 0xC3);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - EAX should contain 4 (last set bit position)
		Assert.Equal(4u, cpu.GetRegister("EAX"));
		Assert.Equal(0x1003u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_BIT_BTS_ShouldSetBit()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x00000000);
		cpu.SetRegister("ECX", 5); // Set bit 5
		
		// BTS EAX, ECX (0x0F 0xAB 0xC1)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0xAB);
		mem.Write8(0x1002, 0xC1);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - Bit 5 should be set in EAX
		Assert.Equal(0x00000020u, cpu.GetRegister("EAX")); // 2^5 = 32 = 0x20
		Assert.Equal(0x1003u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_BCD_CBW_ShouldConvertByteToWord()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345680); // AL = 0x80 (signed byte = -128)
		
		// CBW (0x66 0x98 - Convert Byte to Word)
		mem.Write8(0x1000, 0x66);
		mem.Write8(0x1001, 0x98);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - AX should contain sign-extended value (0xFF80)
		uint expectedEax = 0x1234FF80;
		Assert.Equal(expectedEax, cpu.GetRegister("EAX"));
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_BCD_CWDE_ShouldConvertWordToDword()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12348000); // AX = 0x8000 (signed word = -32768)
		
		// CWDE (0x98 - Convert Word to Doubleword Extended)
		mem.Write8(0x1000, 0x98);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - EAX should contain sign-extended value (0xFFFF8000)
		Assert.Equal(0xFFFF8000u, cpu.GetRegister("EAX"));
		Assert.Equal(0x1001u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_SHLD_ShouldShiftLeftDouble()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF01);
		
		// SHLD EAX, EBX, 4 (0x0F 0xA4 0xC3 0x04)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0xA4);
		mem.Write8(0x1002, 0xC3);
		mem.Write8(0x1003, 0x04);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - EAX should be shifted left by 4, filling with high bits of EBX
		// 0x12345678 << 4 = 0x23456780, fill with 0xA from EBX high bits
		Assert.Equal(0x2345678Au, cpu.GetRegister("EAX"));
		Assert.Equal(0x1004u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_SHRD_ShouldShiftRightDouble()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF01);
		
		// SHRD EAX, EBX, 4 (0x0F 0xAC 0xC3 0x04)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0xAC);
		mem.Write8(0x1002, 0xC3);
		mem.Write8(0x1003, 0x04);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - EAX should be shifted right by 4, filling with low bits of EBX
		// 0x12345678 >> 4 = 0x01234567, fill with 0x1 from EBX low bits  
		Assert.Equal(0x11234567u, cpu.GetRegister("EAX"));
		Assert.Equal(0x1004u, cpu.GetEip());
	}
}
