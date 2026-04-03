using Xunit;
using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for BMI/BMI2 bit manipulation instructions (LZCNT, TZCNT, POPCNT, ANDN, BEXTR, BLSI, BLSMSK, BLSR)
/// </summary>
public class BmiInstructionTests
{
	#region LZCNT Tests

	[Theory]
	[InlineData(0x80000000u, 0u)]   // High bit set -> 0 leading zeros
	[InlineData(0x40000000u, 1u)]   // Bit 30 set -> 1 leading zero
	[InlineData(0x00000001u, 31u)]  // Bit 0 set -> 31 leading zeros
	[InlineData(0x00000008u, 28u)]  // Bit 3 set -> 28 leading zeros
	[InlineData(0xFFFFFFFFu, 0u)]   // All bits set -> 0 leading zeros
	[InlineData(0x00000000u, 32u)]  // No bits set -> 32 leading zeros
	public void LZCNT_32Bit_ShouldCountLeadingZeros(uint source, uint expectedCount)
	{
		var memory = new VirtualMemory();
		// LZCNT EAX, EBX (F3 0F BD C3)
		memory.Write8(0x1000, 0xF3);
		memory.Write8(0x1001, 0x0F);
		memory.Write8(0x1002, 0xBD);
		memory.Write8(0x1003, 0xC3);

		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xFFFFFFFF); // Initial value
		cpu.SetRegister("EBX", source);

		cpu.SingleStep(memory);

		var result = cpu.GetRegister("EAX");
		var eflags = cpu.GetRegister("EFLAGS");
		var zf = (eflags & (1 << 6)) != 0; // ZF bit
		var cf = (eflags & (1 << 0)) != 0; // CF bit

		Assert.Equal(expectedCount, result);
		Assert.Equal(source == 0, zf); // ZF set if source is zero
		Assert.Equal(source == 0, cf); // CF set if source is zero
	}

	[Theory]
	[InlineData(0x8000u, 0u)]    // High bit set in 16-bit -> 0 leading zeros
	[InlineData(0x4000u, 1u)]    // Bit 14 set -> 1 leading zero
	[InlineData(0x0001u, 15u)]   // Bit 0 set -> 15 leading zeros
	[InlineData(0x0000u, 16u)]   // No bits set -> 16 leading zeros
	public void LZCNT_16Bit_ShouldCountLeadingZeros(uint source, uint expectedCount)
	{
		var memory = new VirtualMemory();
		// LZCNT AX, BX (66 F3 0F BD C3)
		memory.Write8(0x1000, 0x66); // Operand size override prefix
		memory.Write8(0x1001, 0xF3);
		memory.Write8(0x1002, 0x0F);
		memory.Write8(0x1003, 0xBD);
		memory.Write8(0x1004, 0xC3);

		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xFFFFFFFF); // Initial value
		cpu.SetRegister("EBX", source);

		cpu.SingleStep(memory);

		var result = cpu.GetRegister("EAX") & 0xFFFF;
		Assert.Equal(expectedCount, result);
	}

	#endregion

	#region TZCNT Tests

	[Theory]
	[InlineData(0x00000001u, 0u)]   // Bit 0 set -> 0 trailing zeros
	[InlineData(0x00000002u, 1u)]   // Bit 1 set -> 1 trailing zero
	[InlineData(0x00000008u, 3u)]   // Bit 3 set -> 3 trailing zeros
	[InlineData(0x80000000u, 31u)]  // Bit 31 set -> 31 trailing zeros
	[InlineData(0xFFFFFFFFu, 0u)]   // All bits set -> 0 trailing zeros
	[InlineData(0x00000000u, 32u)]  // No bits set -> 32 trailing zeros
	public void TZCNT_32Bit_ShouldCountTrailingZeros(uint source, uint expectedCount)
	{
		var memory = new VirtualMemory();
		// TZCNT EAX, EBX (F3 0F BC C3)
		memory.Write8(0x1000, 0xF3);
		memory.Write8(0x1001, 0x0F);
		memory.Write8(0x1002, 0xBC);
		memory.Write8(0x1003, 0xC3);

		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xFFFFFFFF); // Initial value
		cpu.SetRegister("EBX", source);

		cpu.SingleStep(memory);

		var result = cpu.GetRegister("EAX");
		var eflags = cpu.GetRegister("EFLAGS");
		var zf = (eflags & (1 << 6)) != 0;
		var cf = (eflags & (1 << 0)) != 0;

		Assert.Equal(expectedCount, result);
		Assert.Equal(source == 0, zf);
		Assert.Equal(source == 0, cf); // CF set if source is zero, otherwise clear for TZCNT
	}

	#endregion

	#region POPCNT Tests

	[Theory]
	[InlineData(0x00000000u, 0u)]   // No bits set
	[InlineData(0x00000001u, 1u)]   // 1 bit set
	[InlineData(0x00000003u, 2u)]   // 2 bits set
	[InlineData(0x0000000Fu, 4u)]   // 4 bits set
	[InlineData(0xFFFFFFFFu, 32u)]  // All 32 bits set
	[InlineData(0x80000001u, 2u)]   // 2 bits set (bit 0 and 31)
	[InlineData(0x55555555u, 16u)]  // Alternating pattern (16 bits)
	[InlineData(0xAAAAAAAAu, 16u)]  // Alternating pattern (16 bits)
	public void POPCNT_32Bit_ShouldCountSetBits(uint source, uint expectedCount)
	{
		var memory = new VirtualMemory();
		// POPCNT EAX, EBX (F3 0F B8 C3)
		memory.Write8(0x1000, 0xF3);
		memory.Write8(0x1001, 0x0F);
		memory.Write8(0x1002, 0xB8);
		memory.Write8(0x1003, 0xC3);

		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 0xFFFFFFFF); // Initial value
		cpu.SetRegister("EBX", source);

		cpu.SingleStep(memory);

		var result = cpu.GetRegister("EAX");
		var eflags = cpu.GetRegister("EFLAGS");
		var zf = (eflags & (1 << 6)) != 0;
		var cf = (eflags & (1 << 0)) != 0;

		Assert.Equal(expectedCount, result);
		Assert.Equal(result == 0, zf); // ZF set if result is zero
		Assert.False(cf); // CF always clear for POPCNT
	}

	#endregion

	#region ANDN Tests

	[Theory]
	[InlineData(0x00000000u, 0x00000000u, 0x00000000u)]  // ~0 & 0 = 0
	[InlineData(0xFFFFFFFFu, 0xFFFFFFFFu, 0x00000000u)]  // ~FFFF & FFFF = 0
	[InlineData(0x00000000u, 0xFFFFFFFFu, 0xFFFFFFFFu)]  // ~0 & FFFF = FFFF
	[InlineData(0x0000FFFFu, 0xFFFF0000u, 0xFFFF0000u)]  // ~0000FFFF & FFFF0000 = FFFF0000
	[InlineData(0xF0F0F0F0u, 0x0F0F0F0Fu, 0x0F0F0F0Fu)]  // ~F0F0F0F0 & 0F0F0F0F = 0F0F0F0F
	public void ANDN_32Bit_ShouldComputeAndNot(uint src1, uint src2, uint expected)
	{
		var memory = new VirtualMemory();
		// ANDN EAX, EBX, ECX (C4 E2 60 F2 C1) - VEX-encoded
		memory.Write8(0x1000, 0xC4);
		memory.Write8(0x1001, 0xE2);
		memory.Write8(0x1002, 0x60);
		memory.Write8(0x1003, 0xF2);
		memory.Write8(0x1004, 0xC1);

		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBX", src1);
		cpu.SetRegister("ECX", src2);

		cpu.SingleStep(memory);

		var result = cpu.GetRegister("EAX");
		var eflags = cpu.GetRegister("EFLAGS");
		var zf = (eflags & (1 << 6)) != 0;
		var sf = (eflags & (1 << 7)) != 0;
		var cf = (eflags & (1 << 0)) != 0;
		var of = (eflags & (1 << 11)) != 0;

		Assert.Equal(expected, result);
		Assert.Equal(expected == 0, zf); // ZF set if result is zero
		Assert.Equal((expected & 0x80000000u) != 0, sf); // SF set if sign bit set
		Assert.False(cf); // CF always clear for ANDN
		Assert.False(of); // OF always clear for ANDN
	}

	#endregion

	#region BEXTR Tests

	[Theory]
	[InlineData(0xFFFFFFFFu, 0x0000u, 0x00000000u)] // Extract from bit 0, length 0 -> 0
	[InlineData(0xFFFFFFFFu, 0x0800u, 0xFFu)]       // Extract from bit 0, length 8 -> FF
	[InlineData(0xFFFFFFFFu, 0x1008u, 0xFFFFu)]     // Extract from bit 8, length 16 -> FFFF
	[InlineData(0x12345678u, 0x0808u, 0x56u)]       // Extract byte at position 8
	[InlineData(0x12345678u, 0x1000u, 0x5678u)]     // Extract lower 16 bits
	[InlineData(0x12345678u, 0x1010u, 0x1234u)]     // Extract upper 16 bits
	public void BEXTR_32Bit_ShouldExtractBitField(uint source, uint control, uint expected)
	{
		var memory = new VirtualMemory();
		// BEXTR EAX, EBX, ECX (C4 E2 60 F7 C3) - VEX-encoded
		memory.Write8(0x1000, 0xC4);
		memory.Write8(0x1001, 0xE2);
		memory.Write8(0x1002, 0x60);
		memory.Write8(0x1003, 0xF7);
		memory.Write8(0x1004, 0xC3);

		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBX", source);
		cpu.SetRegister("ECX", control);

		cpu.SingleStep(memory);

		var result = cpu.GetRegister("EAX");
		var eflags = cpu.GetRegister("EFLAGS");
		var zf = (eflags & (1 << 6)) != 0;

		Assert.Equal(expected, result);
		Assert.Equal(result == 0, zf); // ZF set if result is zero
	}

	#endregion

	#region BLSI Tests

	[Theory]
	[InlineData(0x00000001u, 0x00000001u)]  // Lowest bit at position 0
	[InlineData(0x00000002u, 0x00000002u)]  // Lowest bit at position 1
	[InlineData(0x00000008u, 0x00000008u)]  // Lowest bit at position 3
	[InlineData(0x00000018u, 0x00000008u)]  // Lowest set bit (bit 3)
	[InlineData(0x80000000u, 0x80000000u)]  // Lowest bit at position 31
	[InlineData(0x00000000u, 0x00000000u)]  // No bits set -> 0
	public void BLSI_32Bit_ShouldIsolateLowestSetBit(uint source, uint expected)
	{
		var memory = new VirtualMemory();
		// BLSI EAX, EBX (C4 E2 60 F3 DB) - VEX-encoded
		memory.Write8(0x1000, 0xC4);
		memory.Write8(0x1001, 0xE2);
		memory.Write8(0x1002, 0x60);
		memory.Write8(0x1003, 0xF3);
		memory.Write8(0x1004, 0xDB);

		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBX", source);

		cpu.SingleStep(memory);

		var result = cpu.GetRegister("EAX");
		var eflags = cpu.GetRegister("EFLAGS");
		var zf = (eflags & (1 << 6)) != 0;
		var cf = (eflags & (1 << 0)) != 0;

		Assert.Equal(expected, result);
		Assert.Equal(source == 0, zf); // ZF set if source is zero
		Assert.Equal(source != 0, cf); // CF set if source is nonzero
	}

	#endregion

	#region BLSMSK Tests

	[Theory]
	[InlineData(0x00000001u, 0x00000001u)]  // Mask up to bit 0 -> 1
	[InlineData(0x00000002u, 0x00000003u)]  // Mask up to bit 1 -> 3
	[InlineData(0x00000008u, 0x0000000Fu)]  // Mask up to bit 3 -> F
	[InlineData(0x00000018u, 0x0000000Fu)]  // Mask up to lowest (bit 3) -> F
	[InlineData(0x80000000u, 0xFFFFFFFFu)]  // Mask up to bit 31 -> FFFFFFFF
	[InlineData(0x00000000u, 0xFFFFFFFFu)]  // No bits set -> FFFFFFFF
	public void BLSMSK_32Bit_ShouldCreateMaskToLowestSetBit(uint source, uint expected)
	{
		var memory = new VirtualMemory();
		// BLSMSK EAX, EBX (C4 E2 60 F3 D3) - VEX-encoded
		memory.Write8(0x1000, 0xC4);
		memory.Write8(0x1001, 0xE2);
		memory.Write8(0x1002, 0x60);
		memory.Write8(0x1003, 0xF3);
		memory.Write8(0x1004, 0xD3);

		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBX", source);

		cpu.SingleStep(memory);

		var result = cpu.GetRegister("EAX");
		var eflags = cpu.GetRegister("EFLAGS");
		var cf = (eflags & (1 << 0)) != 0;

		Assert.Equal(expected, result);
		Assert.Equal(source == 0, cf); // CF set if source is zero
	}

	#endregion

	#region BLSR Tests

	[Theory]
	[InlineData(0x00000001u, 0x00000000u)]  // Reset bit 0 -> 0
	[InlineData(0x00000003u, 0x00000002u)]  // Reset bit 0 -> 2
	[InlineData(0x00000008u, 0x00000000u)]  // Reset bit 3 -> 0
	[InlineData(0x00000018u, 0x00000010u)]  // Reset lowest (bit 3) -> 10
	[InlineData(0x80000000u, 0x00000000u)]  // Reset bit 31 -> 0
	[InlineData(0x00000000u, 0x00000000u)]  // No bits set -> 0
	public void BLSR_32Bit_ShouldResetLowestSetBit(uint source, uint expected)
	{
		var memory = new VirtualMemory();
		// BLSR EAX, EBX (C4 E2 60 F3 CB) - VEX-encoded
		memory.Write8(0x1000, 0xC4);
		memory.Write8(0x1001, 0xE2);
		memory.Write8(0x1002, 0x60);
		memory.Write8(0x1003, 0xF3);
		memory.Write8(0x1004, 0xCB);

		var cpu = new JitCpu(memory);
		cpu.SetEip(0x1000);
		cpu.SetRegister("EBX", source);

		cpu.SingleStep(memory);

		var result = cpu.GetRegister("EAX");
		var eflags = cpu.GetRegister("EFLAGS");
		var zf = (eflags & (1 << 6)) != 0;
		var cf = (eflags & (1 << 0)) != 0;

		Assert.Equal(expected, result);
		Assert.Equal(result == 0, zf); // ZF set if result is zero
		Assert.Equal(source == 0, cf); // CF set if source is zero
	}

	#endregion
}
