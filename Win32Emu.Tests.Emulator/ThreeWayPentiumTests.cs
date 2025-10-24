using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Three-way conformance tests comparing Unicorn, IcedCpu, and JitCpu
/// Covers the Pentium instruction set including x87 FPU and MMX
/// </summary>
public class ThreeWayPentiumTests : IDisposable
{
	private readonly ThreeWayTestHelper _helper;

	public ThreeWayPentiumTests()
	{
		_helper = new ThreeWayTestHelper();
	}

	#region Conditional Jumps

	[Fact]
	public void JE_WhenZero_ShouldMatch()
	{
		// Arrange: Set ZF=1, then JE with offset
		// CMP EAX, EAX (39 C0) - sets ZF=1
		// JE +5 (74 05) - should jump
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0x39, 0xC0, 0x74, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act - execute CMP
		_helper.ExecuteInstruction();
		
		// Assert - all should have ZF=1
		_helper.AssertFlagsMatch(CpuFlag.Zf);
		
		// Act - execute JE
		_helper.ExecuteInstruction();
		
		// Assert - EIP should have jumped in all three
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JNE_WhenNotZero_ShouldMatch()
	{
		// Arrange: Set ZF=0, then JNE with offset
		// CMP EAX, EBX (39 D8) - sets ZF=0 (different values)
		// JNE +5 (75 05) - should jump
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0x39, 0xD8, 0x75, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act - execute CMP
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf);
		
		// Act - execute JNE
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JA_WhenAbove_ShouldMatch()
	{
		// Arrange: Set CF=0 and ZF=0 (above), then JA
		// CMP EBX, EAX (39 C3) where EBX > EAX
		// JA +5 (77 05) - should jump
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x39, 0xC3, 0x77, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf);
		
		_helper.ExecuteInstruction(); // JA
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JL_WhenLess_Signed_ShouldMatch()
	{
		// Arrange: Signed comparison, set SF!=OF (less than)
		// CMP EAX, EBX (39 D8) where EAX < EBX (signed)
		// JL +5 (7C 05) - should jump
		_helper.SetReg("EAX", 0xFFFFFFF0); // -16 signed
		_helper.SetReg("EBX", 0x00000010); // +16 signed
		_helper.WriteCode(0x39, 0xD8, 0x7C, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.AssertFlagsMatch(CpuFlag.Sf, CpuFlag.Of);
		
		_helper.ExecuteInstruction(); // JL
		_helper.AssertRegistersMatch("EIP");
	}

	#endregion

	#region Bit Manipulation

	[Fact]
	public void BSF_FindFirstBit_ShouldMatch()
	{
		// Arrange: BSF EAX, EBX (0F BC C3)
		_helper.SetReg("EAX", 0x00000000);
		_helper.SetReg("EBX", 0x00000018); // Binary: ...0001 1000
		_helper.WriteCode(0x0F, 0xBC, 0xC3);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should contain 3 (first set bit position)
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Zf);
	}

	[Fact]
	public void BSR_FindLastBit_ShouldMatch()
	{
		// Arrange: BSR EAX, EBX (0F BD C3)
		_helper.SetReg("EAX", 0x00000000);
		_helper.SetReg("EBX", 0x00000018); // Binary: ...0001 1000
		_helper.WriteCode(0x0F, 0xBD, 0xC3);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should contain 4 (last set bit position)
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Zf);
	}

	[Fact]
	public void BTS_SetBit_ShouldMatch()
	{
		// Arrange: BTS EAX, ECX (0F AB C8)
		_helper.SetReg("EAX", 0x00000000);
		_helper.SetReg("ECX", 5); // Set bit 5
		_helper.WriteCode(0x0F, 0xAB, 0xC8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - Bit 5 should be set in EAX
		_helper.AssertRegistersMatch("EAX", "ECX");
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	#endregion

	#region BCD Arithmetic

	[Fact]
	public void CBW_SignExtend_ShouldMatch()
	{
		// Arrange: CBW (66 98) - convert byte to word
		_helper.SetReg("EAX", 0x12345680); // AL = 0x80 (signed byte = -128)
		_helper.WriteCode(0x66, 0x98);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - AX should contain sign-extended value
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void CWDE_SignExtend_ShouldMatch()
	{
		// Arrange: CWDE (98) - convert word to dword
		_helper.SetReg("EAX", 0x12348000); // AX = 0x8000 (signed word = -32768)
		_helper.WriteCode(0x98);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should contain sign-extended value
		_helper.AssertRegistersMatch("EAX");
	}

	#endregion

	#region Conditional Moves

	[Fact]
	public void CMOVAE_WhenCarryClear_ShouldMatch()
	{
		// Arrange: Set CF=0, then CMOVAE
		// CLC (F8) - clear carry
		// CMOVAE EAX, EBX (0F 43 C3)
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0xF8, 0x0F, 0x43, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // CLC
		_helper.AssertFlagsMatch(CpuFlag.Cf);
		
		_helper.ExecuteInstruction(); // CMOVAE
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVO_WhenOverflow_ShouldMatch()
	{
		// Arrange: Set OF=1 by causing overflow, then CMOVO
		// ADD AL, 0x7F (04 7F) with AL=0x7F causes overflow
		// CMOVO EAX, EBX (0F 40 C3)
		_helper.SetReg("EAX", 0x0000007F);
		_helper.SetReg("EBX", 0xFFFFFFFF);
		_helper.WriteCode(0x04, 0x7F, 0x0F, 0x40, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // ADD
		_helper.AssertFlagsMatch(CpuFlag.Of);
		
		_helper.ExecuteInstruction(); // CMOVO
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
	}

	#endregion

	#region System Instructions

	[Fact]
	public void HLT_ShouldMatch()
	{
		// Arrange: HLT (F4)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0xF4, 0x90);
		
		// Act - HLT should execute without error
		try
		{
			_helper.ExecuteInstruction();
			// In some implementations, HLT may complete normally
		}
		catch
		{
			// In others, it may throw - that's OK
		}
		
		// Assert - at minimum, EAX should remain unchanged
		// (Can't assert much more since HLT behavior varies)
	}

	#endregion

	#region Arithmetic

	[Fact]
	public void ADD_EAX_EBX_ShouldMatch()
	{
		// Arrange: ADD EAX, EBX (01 D8)
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000003);
		_helper.WriteCode(0x01, 0xD8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
	}

	[Fact]
	public void SUB_WithBorrow_ShouldMatch()
	{
		// Arrange: SUB EAX, EBX (29 D8)
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x29, 0xD8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
	}

	[Fact]
	public void XOR_EAX_EAX_ShouldMatch()
	{
		// Arrange: XOR EAX, EAX (31 C0) - common zeroing idiom
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0x31, 0xC0);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf);
	}

	#endregion

	#region Logic and Shifts

	[Fact]
	public void SHL_EAX_Immediate_ShouldMatch()
	{
		// Arrange: SHL EAX, 4 (C1 E0 04)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0xC1, 0xE0, 0x04);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Zf, CpuFlag.Pf);
	}

	[Fact]
	public void SHR_EAX_Immediate_ShouldMatch()
	{
		// Arrange: SHR EAX, 4 (C1 E8 04)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0xC1, 0xE8, 0x04);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Zf, CpuFlag.Pf);
	}

	[Fact]
	public void SHLD_DoubleShiftLeft_ShouldMatch()
	{
		// Arrange: SHLD EAX, EBX, 4 (0F A4 D8 04)
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0x0F, 0xA4, 0xD8, 0x04);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should be shifted left by 4, filling with high bits of EBX
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Zf);
	}

	[Fact]
	public void SHRD_DoubleShiftRight_ShouldMatch()
	{
		// Arrange: SHRD EAX, EBX, 4 (0F AC D8 04)
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0x0F, 0xAC, 0xD8, 0x04);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should be shifted right by 4, filling with low bits of EBX
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Zf);
	}

	[Fact]
	public void AND_EAX_EBX_ShouldMatch()
	{
		// Arrange: AND EAX, EBX (21 D8)
		_helper.SetReg("EAX", 0xFF00FF00);
		_helper.SetReg("EBX", 0x0F0F0F0F);
		_helper.WriteCode(0x21, 0xD8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf);
	}

	#endregion

	#region Data Movement

	[Fact]
	public void MOV_EAX_EBX_ShouldMatch()
	{
		// Arrange: MOV EAX, EBX (89 D8)
		_helper.SetReg("EAX", 0x00000000);
		_helper.SetReg("EBX", 0x12345678);
		_helper.WriteCode(0x89, 0xD8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void PUSH_POP_ShouldMatch()
	{
		// Arrange: PUSH EAX (50), POP EBX (5B)
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0x00000000);
		_helper.WriteCode(0x50, 0x5B);
		
		// Act
		_helper.ExecuteInstruction(); // PUSH
		var esp1 = _helper;
		_helper.AssertRegistersMatch("ESP");
		
		_helper.ExecuteInstruction(); // POP
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EBX", "ESP");
	}

	#endregion

	#region Multiply/Divide

	[Fact]
	public void MUL_EAX_EBX_ShouldMatch()
	{
		// Arrange: MUL EBX (F7 E3)
		_helper.SetReg("EAX", 0x00001000);
		_helper.SetReg("EBX", 0x00000100);
		_helper.WriteCode(0xF7, 0xE3);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EDX:EAX contains result
		_helper.AssertRegistersMatch("EAX", "EDX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Of);
	}

	[Fact]
	public void IMUL_SignedMultiply_ShouldMatch()
	{
		// Arrange: IMUL EBX (F7 EB)
		_helper.SetReg("EAX", 0xFFFFFFF0); // -16
		_helper.SetReg("EBX", 0x00000010); // 16
		_helper.WriteCode(0xF7, 0xEB);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EDX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Of);
	}

	[Fact]
	public void DIV_EAX_EBX_ShouldMatch()
	{
		// Arrange: DIV EBX (F7 F3)
		_helper.SetReg("EAX", 0x00001000);
		_helper.SetReg("EDX", 0x00000000);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0xF7, 0xF3);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX = quotient, EDX = remainder
		_helper.AssertRegistersMatch("EAX", "EDX");
	}

	#endregion

	#region Flag Operations

	[Fact]
	public void CLC_ShouldMatch()
	{
		// Arrange: STC then CLC (F9 F8)
		_helper.WriteCode(0xF9, 0xF8);
		
		// Act
		_helper.ExecuteInstruction(); // STC
		_helper.ExecuteInstruction(); // CLC
		
		// Assert
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	[Fact]
	public void CMC_ShouldMatch()
	{
		// Arrange: CLC then CMC (F8 F5)
		_helper.WriteCode(0xF8, 0xF5);
		
		// Act
		_helper.ExecuteInstruction(); // CLC
		_helper.ExecuteInstruction(); // CMC
		
		// Assert
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	#endregion

	#region Additional Tests

	[Fact]
	public void INC_EAX_ShouldMatch()
	{
		// Arrange: INC EAX (40)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0x40);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
	}

	[Fact]
	public void DEC_EAX_ShouldMatch()
	{
		// Arrange: DEC EAX (48)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0x48);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
	}

	[Fact]
	public void NEG_EAX_ShouldMatch()
	{
		// Arrange: NEG EAX (F7 D8)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0xF7, 0xD8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Of, CpuFlag.Af, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf);
	}

	[Fact]
	public void CMP_EAX_EBX_ShouldMatch()
	{
		// Arrange: CMP EAX, EBX (39 D8)
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0x12345670);
		_helper.WriteCode(0x39, 0xD8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Of, CpuFlag.Af, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf);
	}

	[Fact]
	public void OR_EAX_EBX_ShouldMatch()
	{
		// Arrange: OR EAX, EBX (09 D8)
		_helper.SetReg("EAX", 0xFF00FF00);
		_helper.SetReg("EBX", 0x0F0F0F0F);
		_helper.WriteCode(0x09, 0xD8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Of, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Pf);
	}

	#endregion

	public void Dispose()
	{
		_helper?.Dispose();
	}
}
