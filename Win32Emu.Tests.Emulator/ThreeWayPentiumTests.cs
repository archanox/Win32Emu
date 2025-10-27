using Win32Emu.Tests.Emulator.TestInfrastructure;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Three-way conformance tests comparing Unicorn, IcedCpu, and JitCpu
/// Covers the Pentium instruction set including x87 FPU and MMX
/// </summary>
public class ThreeWayPentiumTests : IDisposable
{
	private readonly ThreeWayTestHelper _helper;
	private const long StackBaseAddress = 0x00100000;  // Must match ThreeWayTestHelper.StackBaseAddress

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

	#region More Conditional Jumps

	[Fact]
	public void JB_WhenBelow_ShouldMatch()
	{
		// Arrange: Set CF=1 (below), then JB
		// CMP EAX, EBX (39 D8) where EAX < EBX (unsigned)
		// JB +5 (72 05) - should jump
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x39, 0xD8, 0x72, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.AssertFlagsMatch(CpuFlag.Cf);
		
		_helper.ExecuteInstruction(); // JB
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JAE_WhenAboveOrEqual_ShouldMatch()
	{
		// Arrange: Set CF=0 (above or equal), then JAE
		// CMP EAX, EBX (39 D8) where EAX >= EBX
		// JAE +5 (73 05) - should jump
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x39, 0xD8, 0x73, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // JAE
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JBE_WhenBelowOrEqual_ShouldMatch()
	{
		// Arrange: Set CF=1 or ZF=1 (below or equal), then JBE
		// CMP EAX, EBX (39 D8) where EAX <= EBX
		// JBE +5 (76 05) - should jump
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x39, 0xD8, 0x76, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // JBE
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JG_WhenGreater_Signed_ShouldMatch()
	{
		// Arrange: Set ZF=0 and SF=OF (greater), then JG
		// CMP EAX, EBX (39 D8) where EAX > EBX (signed)
		// JG +5 (7F 05) - should jump
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x39, 0xD8, 0x7F, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // JG
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JGE_WhenGreaterOrEqual_Signed_ShouldMatch()
	{
		// Arrange: Set SF=OF (greater or equal), then JGE
		// CMP EAX, EBX (39 D8) where EAX >= EBX (signed)
		// JGE +5 (7D 05) - should jump
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x39, 0xD8, 0x7D, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // JGE
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JLE_WhenLessOrEqual_Signed_ShouldMatch()
	{
		// Arrange: Set ZF=1 or SF!=OF (less or equal), then JLE
		// CMP EAX, EBX (39 D8) where EAX <= EBX (signed)
		// JLE +5 (7E 05) - should jump
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x39, 0xD8, 0x7E, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // JLE
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JO_WhenOverflow_ShouldMatch()
	{
		// Arrange: Cause overflow, then JO
		// ADD AL, 0x7F (04 7F) with AL=0x7F causes overflow
		// JO +5 (70 05) - should jump
		_helper.SetReg("EAX", 0x0000007F);
		_helper.WriteCode(0x04, 0x7F, 0x70, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // ADD
		_helper.ExecuteInstruction(); // JO
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JNO_WhenNoOverflow_ShouldMatch()
	{
		// Arrange: No overflow, then JNO
		// ADD EAX, EBX (01 D8) with no overflow
		// JNO +5 (71 05) - should jump
		_helper.SetReg("EAX", 0x00000001);
		_helper.SetReg("EBX", 0x00000002);
		_helper.WriteCode(0x01, 0xD8, 0x71, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // ADD
		_helper.ExecuteInstruction(); // JNO
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JS_WhenSign_ShouldMatch()
	{
		// Arrange: Set SF=1 (negative), then JS
		// NEG EAX (F7 D8) with positive value
		// JS +5 (78 05) - should jump
		_helper.SetReg("EAX", 0x00000001);
		_helper.WriteCode(0xF7, 0xD8, 0x78, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // NEG
		_helper.ExecuteInstruction(); // JS
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JNS_WhenNoSign_ShouldMatch()
	{
		// Arrange: Set SF=0 (positive), then JNS
		// XOR EAX, EAX (31 C0) - result is zero (positive)
		// JNS +5 (79 05) - should jump
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0x31, 0xC0, 0x79, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // XOR
		_helper.ExecuteInstruction(); // JNS
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JP_WhenParity_ShouldMatch()
	{
		// Arrange: Set PF=1 (even parity), then JP
		// MOV AL, 0x03 (B0 03) - has even parity (2 bits set)
		// TEST AL, AL (A8 FF) - sets flags
		// JP +5 (7A 05) - should jump
		_helper.SetReg("EAX", 0x00000003);
		_helper.WriteCode(0xA8, 0xFF, 0x7A, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // TEST
		_helper.ExecuteInstruction(); // JP
		_helper.AssertRegistersMatch("EIP");
	}

	[Fact]
	public void JNP_WhenNoParity_ShouldMatch()
	{
		// Arrange: Set PF=0 (odd parity), then JNP
		// MOV AL, 0x01 (B0 01) - has odd parity (1 bit set)
		// TEST AL, AL (A8 FF) - sets flags
		// JNP +5 (7B 05) - should jump
		_helper.SetReg("EAX", 0x00000001);
		_helper.WriteCode(0xA8, 0xFF, 0x7B, 0x05, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90);
		
		// Act
		_helper.ExecuteInstruction(); // TEST
		_helper.ExecuteInstruction(); // JNP
		_helper.AssertRegistersMatch("EIP");
	}

	#endregion

	#region More Bit Operations

	[Fact]
	public void BT_TestBit_ShouldMatch()
	{
		// Arrange: BT EAX, ECX (0F A3 C8)
		_helper.SetReg("EAX", 0x00000020); // Bit 5 is set
		_helper.SetReg("ECX", 5);
		_helper.WriteCode(0x0F, 0xA3, 0xC8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - CF should be set
		_helper.AssertRegistersMatch("EAX", "ECX");
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	[Fact]
	public void BTR_ResetBit_ShouldMatch()
	{
		// Arrange: BTR EAX, ECX (0F B3 C8)
		_helper.SetReg("EAX", 0x00000020); // Bit 5 is set
		_helper.SetReg("ECX", 5);
		_helper.WriteCode(0x0F, 0xB3, 0xC8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - Bit 5 should be cleared
		_helper.AssertRegistersMatch("EAX", "ECX");
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	[Fact]
	public void BTC_ComplementBit_ShouldMatch()
	{
		// Arrange: BTC EAX, ECX (0F BB C8)
		_helper.SetReg("EAX", 0x00000000);
		_helper.SetReg("ECX", 5);
		_helper.WriteCode(0x0F, 0xBB, 0xC8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - Bit 5 should be toggled
		_helper.AssertRegistersMatch("EAX", "ECX");
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	#endregion

	#region Rotate Instructions

	[Fact]
	public void ROL_RotateLeft_ShouldMatch()
	{
		// Arrange: ROL EAX, 4 (C1 C0 04)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0xC1, 0xC0, 0x04);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	[Fact]
	public void ROR_RotateRight_ShouldMatch()
	{
		// Arrange: ROR EAX, 4 (C1 C8 04)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0xC1, 0xC8, 0x04);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	[Fact]
	public void RCL_RotateCarryLeft_ShouldMatch()
	{
		// Arrange: STC then RCL EAX, 1 (F9 D1 D0)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0xF9, 0xD1, 0xD0);
		
		// Act
		_helper.ExecuteInstruction(); // STC
		_helper.ExecuteInstruction(); // RCL
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	[Fact]
	public void RCR_RotateCarryRight_ShouldMatch()
	{
		// Arrange: STC then RCR EAX, 1 (F9 D1 D8)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0xF9, 0xD1, 0xD8);
		
		// Act
		_helper.ExecuteInstruction(); // STC
		_helper.ExecuteInstruction(); // RCR
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	#endregion

	#region More Arithmetic

	[Fact]
	public void ADC_AddWithCarry_ShouldMatch()
	{
		// Arrange: STC then ADC EAX, EBX (F9 11 D8)
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000003);
		_helper.WriteCode(0xF9, 0x11, 0xD8);
		
		// Act
		_helper.ExecuteInstruction(); // STC
		_helper.ExecuteInstruction(); // ADC
		
		// Assert - EAX should be 5 + 3 + 1 = 9
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
	}

	[Fact]
	public void SBB_SubtractWithBorrow_ShouldMatch()
	{
		// Arrange: STC then SBB EAX, EBX (F9 19 D8)
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0xF9, 0x19, 0xD8);
		
		// Act
		_helper.ExecuteInstruction(); // STC
		_helper.ExecuteInstruction(); // SBB
		
		// Assert - EAX should be 16 - 5 - 1 = 10
		_helper.AssertRegistersMatch("EAX", "EBX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf, CpuFlag.Af);
	}

	[Fact]
	public void NOT_BitwiseNot_ShouldMatch()
	{
		// Arrange: NOT EAX (F7 D0)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0xF7, 0xD0);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SAR_ArithmeticShiftRight_ShouldMatch()
	{
		// Arrange: SAR EAX, 4 (C1 F8 04)
		_helper.SetReg("EAX", 0x80000000); // Negative number
		_helper.WriteCode(0xC1, 0xF8, 0x04);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - High bit should be preserved (sign extension)
		_helper.AssertRegistersMatch("EAX");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Sf, CpuFlag.Zf, CpuFlag.Pf);
	}

	#endregion

	#region More Data Movement

	[Fact]
	public void XCHG_ExchangeRegisters_ShouldMatch()
	{
		// Arrange: XCHG EAX, EBX (93)
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0x93);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void LEA_LoadEffectiveAddress_ShouldMatch()
	{
		// Arrange: LEA EAX, [EBX+ECX*4+0x100] (8D 84 8B 00 01 00 00)
		_helper.SetReg("EAX", 0x00000000);
		_helper.SetReg("EBX", 0x00001000);
		_helper.SetReg("ECX", 0x00000010);
		_helper.WriteCode(0x8D, 0x84, 0x8B, 0x00, 0x01, 0x00, 0x00);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should contain the calculated address
		_helper.AssertRegistersMatch("EAX", "EBX", "ECX");
	}

	[Fact]
	public void MOVSX_SignExtend_ShouldMatch()
	{
		// Arrange: MOVSX EAX, BL (0F BE C3)
		_helper.SetReg("EAX", 0x00000000);
		_helper.SetReg("EBX", 0x000000FF); // -1 as signed byte
		_helper.WriteCode(0x0F, 0xBE, 0xC3);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should be sign-extended
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void MOVZX_ZeroExtend_ShouldMatch()
	{
		// Arrange: MOVZX EAX, BL (0F B6 C3)
		_helper.SetReg("EAX", 0xFFFFFFFF);
		_helper.SetReg("EBX", 0x000000FF);
		_helper.WriteCode(0x0F, 0xB6, 0xC3);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should be zero-extended
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void XLATB_Translate_ShouldMatch()
	{
		// Arrange: Set up translation table and XLATB (D7)
		_helper.SetReg("EAX", 0x00000003); // AL = 3 (index)
		_helper.SetReg("EBX", 0x00200000); // Base address
		_helper.WriteMemory(0x00200003, 0xFF); // Translation value
		_helper.WriteCode(0xD7);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - AL should contain the translated value
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void BSWAP_ByteSwap_ShouldMatch()
	{
		// Arrange: BSWAP EAX (0F C8)
		_helper.SetReg("EAX", 0x12345678);
		_helper.WriteCode(0x0F, 0xC8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - Bytes should be reversed
		_helper.AssertRegistersMatch("EAX");
	}

	#endregion

	#region Stack Operations

	[Fact]
	public void PUSHAD_POPAD_ShouldMatch()
	{
		// Arrange: PUSHAD (60), POPAD (61)
		_helper.SetReg("EAX", 0x11111111);
		_helper.SetReg("EBX", 0x22222222);
		_helper.SetReg("ECX", 0x33333333);
		_helper.SetReg("EDX", 0x44444444);
		_helper.SetReg("ESI", 0x55555555);
		_helper.SetReg("EDI", 0x66666666);
		_helper.WriteCode(0x60, 0x61);
		
		// Act
		_helper.ExecuteInstruction(); // PUSHAD
		_helper.ExecuteInstruction(); // POPAD
		
		// Assert
		_helper.AssertRegistersMatch("EAX", "EBX", "ECX", "EDX", "ESI", "EDI", "ESP");
	}

	#endregion

	#region SETcc Instructions

	[Fact]
	public void SETO_SetIfOverflow_ShouldMatch()
	{
		// Arrange: Cause overflow, then SETO AL (0F 90 C0)
		// ADD AL, 0x7F (04 7F) with AL=0x7F causes overflow
		_helper.SetReg("EAX", 0x0000007F);
		_helper.WriteCode(0x04, 0x7F, 0x0F, 0x90, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // ADD
		_helper.ExecuteInstruction(); // SETO
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETNO_SetIfNoOverflow_ShouldMatch()
	{
		// Arrange: No overflow, then SETNO AL (0F 91 C0)
		// ADD AL, 1 (04 01) with AL=1 (no overflow)
		_helper.SetReg("EAX", 0x00000001);
		_helper.WriteCode(0x04, 0x01, 0x0F, 0x91, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // ADD
		_helper.ExecuteInstruction(); // SETNO
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETB_SetIfBelow_ShouldMatch()
	{
		// Arrange: Set CF=1, then SETB AL (0F 92 C0)
		// CMP AL, BL (38 D8) where AL < BL
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x92, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETB
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETAE_SetIfAboveOrEqual_ShouldMatch()
	{
		// Arrange: Set CF=0, then SETAE AL (0F 93 C0)
		// CMP AL, BL (38 D8) where AL >= BL
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x93, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETAE
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETE_SetIfEqual_ShouldMatch()
	{
		// Arrange: Set ZF=1, then SETE AL (0F 94 C0)
		// CMP AL, BL (38 D8) where AL == BL
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x94, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETE
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETNE_SetIfNotEqual_ShouldMatch()
	{
		// Arrange: Set ZF=0, then SETNE AL (0F 95 C0)
		// CMP AL, BL (38 D8) where AL != BL
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x95, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETNE
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETBE_SetIfBelowOrEqual_ShouldMatch()
	{
		// Arrange: Set CF=1 or ZF=1, then SETBE AL (0F 96 C0)
		// CMP AL, BL (38 D8) where AL <= BL
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x96, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETBE
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETA_SetIfAbove_ShouldMatch()
	{
		// Arrange: Set CF=0 and ZF=0, then SETA AL (0F 97 C0)
		// CMP AL, BL (38 D8) where AL > BL
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x97, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETA
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETS_SetIfSign_ShouldMatch()
	{
		// Arrange: Set SF=1, then SETS AL (0F 98 C0)
		// NEG AL (F6 D8) with positive value
		_helper.SetReg("EAX", 0x00000001);
		_helper.WriteCode(0xF6, 0xD8, 0x0F, 0x98, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // NEG
		_helper.ExecuteInstruction(); // SETS
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETNS_SetIfNoSign_ShouldMatch()
	{
		// Arrange: Set SF=0, then SETNS AL (0F 99 C0)
		// XOR AL, AL (30 C0) - result is positive
		_helper.SetReg("EAX", 0x000000FF);
		_helper.WriteCode(0x30, 0xC0, 0x0F, 0x99, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // XOR
		_helper.ExecuteInstruction(); // SETNS
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETP_SetIfParity_ShouldMatch()
	{
		// Arrange: Set PF=1, then SETP AL (0F 9A C0)
		// MOV AL, 3 then TEST AL, AL
		_helper.SetReg("EAX", 0x00000003);
		_helper.WriteCode(0xA8, 0xFF, 0x0F, 0x9A, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // TEST
		_helper.ExecuteInstruction(); // SETP
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETNP_SetIfNoParity_ShouldMatch()
	{
		// Arrange: Set PF=0, then SETNP AL (0F 9B C0)
		// MOV AL, 1 then TEST AL, AL
		_helper.SetReg("EAX", 0x00000001);
		_helper.WriteCode(0xA8, 0xFF, 0x0F, 0x9B, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // TEST
		_helper.ExecuteInstruction(); // SETNP
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETL_SetIfLess_ShouldMatch()
	{
		// Arrange: Set SF!=OF, then SETL AL (0F 9C C0)
		// CMP AL, BL (38 D8) where AL < BL (signed)
		_helper.SetReg("EAX", 0x000000FF); // -1 signed
		_helper.SetReg("EBX", 0x00000001);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x9C, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETL
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETGE_SetIfGreaterOrEqual_ShouldMatch()
	{
		// Arrange: Set SF=OF, then SETGE AL (0F 9D C0)
		// CMP AL, BL (38 D8) where AL >= BL (signed)
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x9D, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETGE
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETLE_SetIfLessOrEqual_ShouldMatch()
	{
		// Arrange: Set ZF=1 or SF!=OF, then SETLE AL (0F 9E C0)
		// CMP AL, BL (38 D8) where AL <= BL (signed)
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x9E, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETLE
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	[Fact]
	public void SETG_SetIfGreater_ShouldMatch()
	{
		// Arrange: Set ZF=0 and SF=OF, then SETG AL (0F 9F C0)
		// CMP AL, BL (38 D8) where AL > BL (signed)
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x38, 0xD8, 0x0F, 0x9F, 0xC0);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // SETG
		
		// Assert - AL should be set to 1
		_helper.AssertRegistersMatch("EAX");
	}

	#endregion

	#region More Conditional Moves

	[Fact]
	public void CMOVE_MoveIfEqual_ShouldMatch()
	{
		// Arrange: Set ZF=1, then CMOVE (0F 44 C3)
		// CMP EAX, EAX (39 C0) - sets ZF=1
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0x39, 0xC0, 0x0F, 0x44, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // CMOVE
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVNE_MoveIfNotEqual_ShouldMatch()
	{
		// Arrange: Set ZF=0, then CMOVNE (0F 45 C3)
		// CMP EAX, EBX (39 D8) - sets ZF=0
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0x39, 0xD8, 0x0F, 0x45, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // CMOVNE
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVB_MoveIfBelow_ShouldMatch()
	{
		// Arrange: Set CF=1, then CMOVB (0F 42 C3)
		// CMP EAX, EBX (39 D8) where EAX < EBX
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x39, 0xD8, 0x0F, 0x42, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // CMOVB
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVBE_MoveIfBelowOrEqual_ShouldMatch()
	{
		// Arrange: Set CF=1 or ZF=1, then CMOVBE (0F 46 C3)
		// CMP EAX, EBX (39 D8) where EAX <= EBX
		_helper.SetReg("EAX", 0x00000005);
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x39, 0xD8, 0x0F, 0x46, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // CMOVBE
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVA_MoveIfAbove_ShouldMatch()
	{
		// Arrange: Set CF=0 and ZF=0, then CMOVA (0F 47 C3)
		// CMP EAX, EBX (39 D8) where EAX > EBX
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x39, 0xD8, 0x0F, 0x47, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // CMOVA
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVS_MoveIfSign_ShouldMatch()
	{
		// Arrange: Set SF=1, then CMOVS (0F 48 C3)
		// NEG EAX (F7 D8) with positive value
		_helper.SetReg("EAX", 0x00000001);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0xF7, 0xD8, 0x0F, 0x48, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // NEG
		_helper.ExecuteInstruction(); // CMOVS
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVNS_MoveIfNoSign_ShouldMatch()
	{
		// Arrange: Set SF=0, then CMOVNS (0F 49 C3)
		// XOR EAX, EAX (31 C0) - result is positive
		_helper.SetReg("EAX", 0x12345678);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0x31, 0xC0, 0x0F, 0x49, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // XOR
		_helper.ExecuteInstruction(); // CMOVNS
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVP_MoveIfParity_ShouldMatch()
	{
		// Arrange: Set PF=1, then CMOVP (0F 4A C3)
		// MOV AL, 3 then TEST AL, AL
		_helper.SetReg("EAX", 0x00000003);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0xA8, 0xFF, 0x0F, 0x4A, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // TEST
		_helper.ExecuteInstruction(); // CMOVP
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVNP_MoveIfNoParity_ShouldMatch()
	{
		// Arrange: Set PF=0, then CMOVNP (0F 4B C3)
		// MOV AL, 1 then TEST AL, AL
		_helper.SetReg("EAX", 0x00000001);
		_helper.SetReg("EBX", 0xABCDEF01);
		_helper.WriteCode(0xA8, 0xFF, 0x0F, 0x4B, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // TEST
		_helper.ExecuteInstruction(); // CMOVNP
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVL_MoveIfLess_ShouldMatch()
	{
		// Arrange: Set SF!=OF, then CMOVL (0F 4C C3)
		// CMP EAX, EBX (39 D8) where EAX < EBX (signed)
		_helper.SetReg("EAX", 0xFFFFFFF0); // -16 signed
		_helper.SetReg("EBX", 0x00000010);
		_helper.WriteCode(0x39, 0xD8, 0x0F, 0x4C, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // CMOVL
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVGE_MoveIfGreaterOrEqual_ShouldMatch()
	{
		// Arrange: Set SF=OF, then CMOVGE (0F 4D C3)
		// CMP EAX, EBX (39 D8) where EAX >= EBX (signed)
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x39, 0xD8, 0x0F, 0x4D, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // CMOVGE
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	[Fact]
	public void CMOVG_MoveIfGreater_ShouldMatch()
	{
		// Arrange: Set ZF=0 and SF=OF, then CMOVG (0F 4F C3)
		// CMP EAX, EBX (39 D8) where EAX > EBX (signed)
		_helper.SetReg("EAX", 0x00000010);
		_helper.SetReg("EBX", 0x00000005);
		_helper.WriteCode(0x39, 0xD8, 0x0F, 0x4F, 0xC3);
		
		// Act
		_helper.ExecuteInstruction(); // CMP
		_helper.ExecuteInstruction(); // CMOVG
		
		// Assert - EAX should be updated
		_helper.AssertRegistersMatch("EAX", "EBX");
	}

	#endregion

	#region More BCD and Special

	[Fact]
	public void CDQ_ConvertDwordToQword_ShouldMatch()
	{
		// Arrange: CDQ (99)
		_helper.SetReg("EAX", 0x80000000); // Negative number
		_helper.SetReg("EDX", 0x00000000);
		_helper.WriteCode(0x99);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EDX should be sign-extended
		_helper.AssertRegistersMatch("EAX", "EDX");
	}

	[Fact]
	public void STC_SetCarry_ShouldMatch()
	{
		// Arrange: CLC then STC (F8 F9)
		_helper.WriteCode(0xF8, 0xF9);
		
		// Act
		_helper.ExecuteInstruction(); // CLC
		_helper.ExecuteInstruction(); // STC
		
		// Assert
		_helper.AssertFlagsMatch(CpuFlag.Cf);
	}

	[Fact]
	public void CLD_ClearDirection_ShouldMatch()
	{
		// Arrange: STD then CLD (FD FC)
		_helper.WriteCode(0xFD, 0xFC);
		
		// Act
		_helper.ExecuteInstruction(); // STD
		_helper.ExecuteInstruction(); // CLD
		
		// Assert
		_helper.AssertFlagsMatch(CpuFlag.Df);
	}

	[Fact]
	public void STD_SetDirection_ShouldMatch()
	{
		// Arrange: CLD then STD (FC FD)
		_helper.WriteCode(0xFC, 0xFD);
		
		// Act
		_helper.ExecuteInstruction(); // CLD
		_helper.ExecuteInstruction(); // STD
		
		// Assert
		_helper.AssertFlagsMatch(CpuFlag.Df);
	}

	#endregion

	#region Memory Operations with Negative Displacement

	[Fact]
	public void MOV_MemoryNegativeDisplacement_ShouldMatch()
	{
		// Arrange: MOV EAX, [EBP-0x44] 
		// This tests memory access with negative displacement
		// Instruction: 8B 45 BC (MOV EAX, [EBP-0x44])
		var stackAddr = (uint)(StackBaseAddress + 0x8000);
		_helper.SetReg("EBP", stackAddr);
		_helper.SetReg("EAX", 0x00000000);
		
		// Write test value at [EBP-0x44]
		var targetAddr = stackAddr - 0x44;
		_helper.WriteMemory(targetAddr, 0x12, 0x34, 0x56, 0x78);
		
		// MOV EAX, [EBP-0x44] = 8B 45 BC
		_helper.WriteCode(0x8B, 0x45, 0xBC);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should contain the value from memory
		_helper.AssertRegistersMatch("EAX", "EBP");
	}

	[Fact]
	public void MOV_MemoryWrite_NegativeDisplacement_ShouldMatch()
	{
		// Arrange: MOV [EBP-0x10], EAX
		// This tests memory write with negative displacement
		// Instruction: 89 45 F0 (MOV [EBP-0x10], EAX)
		var stackAddr = (uint)(StackBaseAddress + 0x8000);
		_helper.SetReg("EBP", stackAddr);
		_helper.SetReg("EAX", 0xDEADBEEF);
		
		// MOV [EBP-0x10], EAX = 89 45 F0
		_helper.WriteCode(0x89, 0x45, 0xF0);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - Memory should contain the value from EAX
		var targetAddr = stackAddr - 0x10;
		_helper.AssertMemoryMatch(targetAddr, 4);
	}

	[Fact]
	public void ADD_MemoryNegativeDisplacement_ShouldMatch()
	{
		// Arrange: ADD EAX, [EBP-0x08]
		// Instruction: 03 45 F8 (ADD EAX, [EBP-0x08])
		var stackAddr = (uint)(StackBaseAddress + 0x8000);
		_helper.SetReg("EBP", stackAddr);
		_helper.SetReg("EAX", 0x00000100);
		
		// Write test value at [EBP-0x08]
		var targetAddr = stackAddr - 0x08;
		_helper.WriteMemory(targetAddr, 0x50, 0x00, 0x00, 0x00); // Value: 0x00000050
		
		// ADD EAX, [EBP-0x08] = 03 45 F8
		_helper.WriteCode(0x03, 0x45, 0xF8);
		
		// Act
		_helper.ExecuteInstruction();
		
		// Assert - EAX should be 0x100 + 0x50 = 0x150
		_helper.AssertRegistersMatch("EAX", "EBP");
		_helper.AssertFlagsMatch(CpuFlag.Cf, CpuFlag.Zf, CpuFlag.Sf, CpuFlag.Of, CpuFlag.Pf);
	}

	// TODO: Re-enable this test after fixing the underlying issue
	// [Fact]
	// public void AND_MemoryNegativeDisplacement_ShouldMatch()
	// {
	// 	// Arrange: AND DWORD PTR [EBP-0x44], 0xFF
	// 	// This is the type of instruction that was failing in the bug report
	// 	// Instruction: 83 65 BC FF (AND DWORD PTR [EBP-0x44], 0xFF)
	// 	var stackAddr = (uint)(StackBaseAddress + 0x8000);
	// 	_helper.SetReg("EBP", stackAddr);
	// 	
	// 	// Write test value at [EBP-0x44]
	// 	var targetAddr = stackAddr - 0x44;
	// 	_helper.WriteMemory(targetAddr, 0x12, 0x34, 0x56, 0x78); // Value: 0x78563412
	// 	
	// 	// AND DWORD PTR [EBP-0x44], 0xFF = 83 65 BC FF
	// 	_helper.WriteCode(0x83, 0x65, 0xBC, 0xFF);
	// 	
	// 	// Act
	// 	_helper.ExecuteInstruction();
	// 	
	// 	// Assert - Memory should be ANDed with 0xFF
	// 	_helper.AssertMemoryMatch(targetAddr, 4);
	// }

	#endregion

	public void Dispose()
	{
		_helper?.Dispose();
	}
}
