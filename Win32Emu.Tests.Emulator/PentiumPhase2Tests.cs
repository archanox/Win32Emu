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

	[Fact]
	public void JitCpu_FNCLEX_ShouldClearExceptionFlags()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// Set FPU status word with exception flags set (bits 0-5 and 7)
		// We'll set it via reflection since there's no public setter
		var fpuStatusField = typeof(JitCpu).GetField("_fpuStatusWord", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		fpuStatusField!.SetValue(cpu, (ushort)0xFFFF); // All bits set
		
		// FNCLEX (0xDB 0xE2) - Clear FPU exceptions
		mem.Write8(0x1000, 0xDB);
		mem.Write8(0x1001, 0xE2);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert
		// Exception flags (bits 0-5, 7) should be cleared
		// Other bits (6, 8-15) should be preserved
		ushort fpuStatus = (ushort)fpuStatusField!.GetValue(cpu)!;
		
		// Bits 0-5 and 7 should be 0
		Assert.Equal(0, fpuStatus & 0x3F); // Bits 0-5
		Assert.Equal(0, fpuStatus & 0x80); // Bit 7
		
		// Bits 8-14 (condition codes, TOP) should be preserved.
		// Bits 0-7 and 15 should be cleared.
		// The preserved bits from 0xFFFF are 0x7F00.
		Assert.Equal(0x7F00, fpuStatus & 0x7F00);

		// Explicitly check that other bits are cleared
		Assert.Equal(0, fpuStatus & 0x80FF);
		
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_FSTSW_AX_ShouldStoreStatusWordToAX()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0x12345678);
		
		// Set FPU status word to a known value
		var fpuStatusField = typeof(JitCpu).GetField("_fpuStatusWord", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		fpuStatusField!.SetValue(cpu, (ushort)0xABCD);
		
		// FSTSW AX (0x9B 0xDF 0xE0) - Store status word to AX
		// Note: 0x9B is FWAIT prefix, but FSTSW AX encoding is 0xDF 0xE0
		mem.Write8(0x1000, 0xDF);
		mem.Write8(0x1001, 0xE0);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert
		// Lower 16 bits of EAX should contain FPU status word
		// Upper 16 bits should be preserved
		uint eax = cpu.GetRegister("EAX");
		Assert.Equal(0x1234ABCDu, eax);
		Assert.Equal(0x1002u, cpu.GetEip());
	}

	[Fact]
	public void JitCpu_FSTSW_Memory_ShouldStoreStatusWordToMemory()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		
		// Set FPU status word to a known value
		var fpuStatusField = typeof(JitCpu).GetField("_fpuStatusWord", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		fpuStatusField!.SetValue(cpu, (ushort)0x5678);
		
		// Clear destination memory
		mem.Write16(0x2000, 0x0000);
		
		// FSTSW [0x2000] (0xDD 0x3D + 4-byte displacement)
		// ModRM: 00 111 101 = 0x3D (MEM with disp32, reg=7)
		mem.Write8(0x1000, 0xDD);
		mem.Write8(0x1001, 0x3D);
		mem.Write32(0x1002, 0x2000);
		
		// Act
		var result = cpu.SingleStep(mem);
		
		// Assert
		// Memory at 0x2000 should contain FPU status word
		ushort stored = mem.Read16(0x2000);
		Assert.Equal((ushort)0x5678, stored);
		Assert.Equal(0x1006u, cpu.GetEip());
	}
}
