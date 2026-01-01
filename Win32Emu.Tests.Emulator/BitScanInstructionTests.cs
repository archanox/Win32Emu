using Xunit;

using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for BSF and BSR instructions with various operand sizes and edge cases
/// </summary>
public class BitScanInstructionTests
{
	[Theory]
	[InlineData(0x00000001u, 0u)]  // Bit 0 set
	[InlineData(0x00000002u, 1u)]  // Bit 1 set
	[InlineData(0x00000004u, 2u)]  // Bit 2 set
	[InlineData(0x00000008u, 3u)]  // Bit 3 set
	[InlineData(0x00000018u, 3u)]  // Bits 3 and 4 set (should return 3)
	[InlineData(0x80000000u, 31u)] // Bit 31 set
	[InlineData(0xFFFFFFFFu, 0u)]  // All bits set (should return 0)
	public void BSF_32Bit_JitCpu_ShouldFindFirstSetBit(uint source, uint expectedBit)
	{
		var memory = new VirtualMemory();
		// BSF EAX, EBX (0F BC C3)
		memory.Write8(0x1000, 0x0F);
		memory.Write8(0x1001, 0xBC);
		memory.Write8(0x1002, 0xC3);
		
		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xFFFFFFFF); // Initial value
		cpu.SetRegister("EBX", source);
		cpu.SetRegister("EFLAGS", 0x00000040); // ZF initially set
		
		cpu.SingleStep(memory);
		
		var result = cpu.GetRegister("EAX");
		var eflags = cpu.GetRegister("EFLAGS");
		var zf = (eflags & (1 << 6)) != 0;
		
		Assert.Equal(expectedBit, result);
		Assert.False(zf, "ZF should be clear when a bit is found");
	}
}
