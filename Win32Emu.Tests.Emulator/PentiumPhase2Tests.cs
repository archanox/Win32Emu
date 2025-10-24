using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for Phase 2 implemented Pentium instructions in JitCpu
/// </summary>
public class PentiumPhase2Tests
{
	[Fact]
	public void JitCpu_CMOVAE_ShouldMoveWhenCarryClear()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF01);
		cpu.SetRegister("EFLAGS", 0x00); // CF=0
		
		// CMOVAE EAX, EBX (0x0F 0x43 0xC3) - move if above or equal (CF=0)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x43);
		mem.Write8(0x1002, 0xC3);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - EAX should be updated to EBX value
		Assert.Equal(0xABCDEF01u, cpu.GetRegister("EAX"));
		Assert.Equal(0x1003u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_CMOVAE_ShouldNotMoveWhenCarrySet()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF01);
		cpu.SetRegister("EFLAGS", 0x01); // CF=1
		
		// CMOVAE EAX, EBX (0x0F 0x43 0xC3)
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x43);
		mem.Write8(0x1002, 0xC3);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - EAX should remain unchanged
		Assert.Equal(0x12345678u, cpu.GetRegister("EAX"));
		Assert.Equal(0x1003u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_CMOVO_ShouldMoveWhenOverflowSet()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x00000000);
		cpu.SetRegister("EBX", 0xFFFFFFFF);
		cpu.SetRegister("EFLAGS", 0x800); // OF=1 (bit 11)
		
		// CMOVO EAX, EBX (0x0F 0x40 0xC3) - move if overflow
		mem.Write8(0x1000, 0x0F);
		mem.Write8(0x1001, 0x40);
		mem.Write8(0x1002, 0xC3);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - EAX should be updated
		Assert.Equal(0xFFFFFFFFu, cpu.GetRegister("EAX"));
		Assert.Equal(0x1003u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_LODSW_ShouldLoadWordAndIncrementESI()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("ESI", 0x2000);
		cpu.SetRegister("EFLAGS", 0x00); // DF=0 (forward)
		
		// Write test data at ESI
		mem.Write16(0x2000, 0xABCD);
		
		// LODSW (0x66 0xAD)
		mem.Write8(0x1000, 0x66);
		mem.Write8(0x1001, 0xAD);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - AX should contain loaded word, ESI incremented
		Assert.Equal(0x1234ABCDu, cpu.GetRegister("EAX")); // Lower 16 bits changed
		Assert.Equal(0x2002u, cpu.GetRegister("ESI")); // ESI += 2
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_LODSW_ShouldLoadWordAndDecrementESI_WhenDFSet()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("ESI", 0x2000);
		cpu.SetRegister("EFLAGS", 0x400); // DF=1 (backward, bit 10)
		
		// Write test data at ESI
		mem.Write16(0x2000, 0xABCD);
		
		// LODSW (0x66 0xAD)
		mem.Write8(0x1000, 0x66);
		mem.Write8(0x1001, 0xAD);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - AX should contain loaded word, ESI decremented
		Assert.Equal(0x1234ABCDu, cpu.GetRegister("EAX"));
		Assert.Equal(0x1FFEu, cpu.GetRegister("ESI")); // ESI -= 2
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_RETF_ShouldPopEIPAndCS()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x2000);
		
		// Setup stack with return address and CS
		mem.Write32(0x2000, 0x00003000); // Return EIP
		mem.Write32(0x2004, 0x00000023); // Return CS (ignored in flat memory)
		
		// RETF (0xCB)
		mem.Write8(0x1000, 0xCB);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP restored, ESP adjusted for both pops
		Assert.Equal(0x3000u, cpu.GetEip());
		Assert.Equal(0x2008u, cpu.GetRegister("ESP")); // ESP += 8 (4 for EIP + 4 for CS)
	}

	[Fact]
	public void JitCpu_INTO_ShouldExecuteWhenOverflowSet()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EFLAGS", 0x800); // OF=1
		
		// INTO (0xCE)
		mem.Write8(0x1000, 0xCE);
		
		// Act - should not crash
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP advances
		Assert.Equal(0x1001u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_HLT_ShouldExecute()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// HLT (0xF4)
		mem.Write8(0x1000, 0xF4);
		
		// Act - should not crash
		var result = cpu.SingleStep(mem);
		
		// Assert - EIP advances
		Assert.Equal(0x1001u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_ENTER_ShouldCreateStackFrame()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x3000);
		cpu.SetRegister("EBP", 0x2500);
		
		// ENTER 16, 0 (0xC8 0x10 0x00 0x00) - allocate 16 bytes, nesting level 0
		mem.Write8(0x1000, 0xC8);
		mem.Write8(0x1001, 0x10);
		mem.Write8(0x1002, 0x00);
		mem.Write8(0x1003, 0x00);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert
		// EBP should be pushed, then updated to frame pointer
		// ESP should be decremented by 4 (push) + 16 (alloc)
		uint newEbp = cpu.GetRegister("EBP");
		uint newEsp = cpu.GetRegister("ESP");
		
		Assert.Equal(0x2FFCu, newEbp); // Frame pointer
		Assert.Equal(0x2FECu, newEsp); // ESP = frame - 16
		Assert.Equal(0x1004u, cpu.GetEip());
	}
}
