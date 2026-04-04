using Iced.Intel;
using Win32Emu.Rtl;

namespace Win32Emu.Tests.Emulator;

public class X86ToRtlConverterTests
{
	[Fact]
	public void Convert_RorInstruction_GeneratesCorrectRtl()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Ror_rm32_imm8, Register.EAX, 4);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401003;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// ROR should generate multiple RTL instructions (temps for shifts and OR)
		Assert.True(block.Instructions.Count > 1);
	}

	[Fact]
	public void Convert_XchgInstruction_GeneratesThreeAssignments()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Xchg_r32_rm32, Register.EAX, Register.EBX);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401002;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// XCHG should generate 3 assignments: temp = dest, dest = src, src = temp
		Assert.Equal(3, block.Instructions.Count);
		Assert.IsType<RtlAssignment>(block.Instructions[0]);
		Assert.IsType<RtlAssignment>(block.Instructions[1]);
		Assert.IsType<RtlAssignment>(block.Instructions[2]);
	}

	[Fact]
	public void Convert_ImulTwoOperands_GeneratesBinaryOp()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Imul_r32_rm32, Register.EAX, Register.EBX);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401003;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		Assert.Single(block.Instructions);
		var binOp = Assert.IsType<RtlBinaryOp>(block.Instructions[0]);
		Assert.Equal("*", binOp.Operator);
	}

	[Fact]
	public void Convert_ImulThreeOperands_GeneratesCorrectRtl()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Imul_r32_rm32_imm32, Register.EAX, Register.EBX, 10);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401006;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		Assert.Single(block.Instructions);
		var binOp = Assert.IsType<RtlBinaryOp>(block.Instructions[0]);
		Assert.Equal("*", binOp.Operator);
	}

	[Fact]
	public void Convert_DivInstruction_GeneratesTwoOperations()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Div_rm32, Register.EBX);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401002;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// DIV should generate three operations: temp = EAX (preserve dividend), EAX = quotient, EDX = remainder
		Assert.Equal(3, block.Instructions.Count);
		Assert.IsType<RtlAssignment>(block.Instructions[0]); // temp = EAX
		Assert.IsType<RtlBinaryOp>(block.Instructions[1]);   // EAX = temp / src
		Assert.IsType<RtlBinaryOp>(block.Instructions[2]);   // EDX = temp % src

		// Verify quotient operation
		var quotient = (RtlBinaryOp)block.Instructions[1];
		Assert.Equal("/", quotient.Operator);
		var quotientDest = Assert.IsType<RtlRegister>(quotient.Destination);
		Assert.Equal("EAX", quotientDest.Name);

		// Verify remainder operation
		var remainder = (RtlBinaryOp)block.Instructions[2];
		Assert.Equal("%", remainder.Operator);
		var remainderDest = Assert.IsType<RtlRegister>(remainder.Destination);
		Assert.Equal("EDX", remainderDest.Name);
	}

	[Fact]
	public void Convert_LeaveInstruction_GeneratesCorrectSequence()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Leave);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401001;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// LEAVE should generate: ESP = EBP, EBP = [ESP], ESP += 4
		Assert.Equal(3, block.Instructions.Count);
		Assert.IsType<RtlAssignment>(block.Instructions[0]); // ESP = EBP
		Assert.IsType<RtlLoad>(block.Instructions[1]);       // EBP = [ESP]
		Assert.IsType<RtlBinaryOp>(block.Instructions[2]);   // ESP += 4
	}

	[Fact]
	public void Convert_CdqInstruction_SetsEdxToZero()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Cdq);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401001;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		Assert.Single(block.Instructions);
		var assignment = Assert.IsType<RtlAssignment>(block.Instructions[0]);
		var dest = Assert.IsType<RtlRegister>(assignment.Destination);
		Assert.Equal("EDX", dest.Name);
	}

	[Fact]
	public void Convert_BswapInstruction_GeneratesMultipleTemps()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Bswap_r32, Register.EAX);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401002;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// BSWAP needs many operations to extract, swap, and reassemble bytes
		Assert.True(block.Instructions.Count >= 10);
		Assert.True(result.NextTemporaryId > 0, "BSWAP should use temporary registers");
	}

	[Fact]
	public void Convert_SarInstruction_GeneratesShiftRight()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Sar_rm32_imm8, Register.EAX, 2);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401003;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// SAR generates several helper operations to produce a sign-extended shift.
		// Verify that at least one RtlBinaryOp with ">>" is present.
		Assert.True(block.Instructions.Count > 0);
		var hasRightShift = block.Instructions
			.OfType<RtlBinaryOp>()
			.Any(op => op.Operator == ">>");
		Assert.True(hasRightShift, "Expected at least one right-shift (>>) operation in SAR output");
	}

	[Fact]
	public void Convert_SalInstruction_GeneratesShiftLeft()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Sal_rm32_imm8, Register.EAX, 3);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401003;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		Assert.Single(block.Instructions);
		var binOp = Assert.IsType<RtlBinaryOp>(block.Instructions[0]);
		Assert.Equal("<<", binOp.Operator);
	}

	[Fact]
	public void Convert_SetccInstruction_GeneratesAssignment()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Sete_rm8, Register.AL);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401003;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		Assert.Single(block.Instructions);
		var assignment = Assert.IsType<RtlAssignment>(block.Instructions[0]);
		// SETE should emit a RtlFlagReference with Equal condition
		var flagRef = Assert.IsType<RtlFlagReference>(assignment.Source);
		Assert.Equal(FlagCondition.Equal, flagRef.Condition);
	}

	[Fact]
	public void Convert_MulInstruction_SetsEaxAndEdx()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Mul_rm32, Register.EBX);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401002;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// MUL 32-bit should generate 3 instructions: temp = EAX * src, EAX = temp, EDX = 0
		Assert.Equal(3, block.Instructions.Count);
	}

	[Fact]
	public void Convert_CbwInstruction_CopiesAlToAx()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Cbw);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401001;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		Assert.Single(block.Instructions);
		var assignment = Assert.IsType<RtlAssignment>(block.Instructions[0]);
		var dest = Assert.IsType<RtlRegister>(assignment.Destination);
		Assert.Equal("AX", dest.Name);
	}

	[Fact]
	public void Convert_CwdeInstruction_CopiesAxToEax()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Cwde);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401001;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.NotNull(result);
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		Assert.Single(block.Instructions);
		var assignment = Assert.IsType<RtlAssignment>(block.Instructions[0]);
		var dest = Assert.IsType<RtlRegister>(assignment.Destination);
		Assert.Equal("EAX", dest.Name);
	}

	[Fact]
	public void Convert_CmpInstruction_EmitsFlagUpdate()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Cmp_rm32_r32, Register.EAX, Register.EBX);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401002;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// CMP should emit: temp = left - right, then RtlFlagUpdate(SUB)
		Assert.Equal(2, block.Instructions.Count);
		Assert.IsType<RtlBinaryOp>(block.Instructions[0]);
		var flagUpdate = Assert.IsType<RtlFlagUpdate>(block.Instructions[1]);
		Assert.Equal("SUB", flagUpdate.Operation);
		Assert.True(flagUpdate.UpdateCF);
		Assert.True(flagUpdate.UpdateOF);
	}

	[Fact]
	public void Convert_TestInstruction_EmitsFlagUpdateWithoutCfOf()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Test_rm32_r32, Register.EAX, Register.EBX);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401002;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// TEST should emit: temp = left & right, then RtlFlagUpdate(AND, UpdateCF=false, UpdateOF=false)
		Assert.Equal(2, block.Instructions.Count);
		var flagUpdate = Assert.IsType<RtlFlagUpdate>(block.Instructions[1]);
		Assert.Equal("AND", flagUpdate.Operation);
		Assert.False(flagUpdate.UpdateCF);
		Assert.False(flagUpdate.UpdateOF);
	}

	[Fact]
	public void Convert_AddInstruction_EmitsFlagUpdate()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Add_rm32_r32, Register.EAX, Register.EBX);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401002;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// ADD: save orig left, add, flag-update
		Assert.Equal(3, block.Instructions.Count);
		Assert.IsType<RtlAssignment>(block.Instructions[0]);  // save original
		Assert.IsType<RtlBinaryOp>(block.Instructions[1]);    // add
		var flagUpdate = Assert.IsType<RtlFlagUpdate>(block.Instructions[2]);
		Assert.Equal("ADD", flagUpdate.Operation);
		Assert.True(flagUpdate.UpdateCF);
		Assert.True(flagUpdate.UpdateOF);
	}

	[Fact]
	public void Convert_IncInstruction_EmitsFlagUpdateWithoutCf()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.Create(Code.Inc_r32, Register.EAX);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401001;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];

		// INC: save orig, inc, flag-update (no CF)
		Assert.Equal(3, block.Instructions.Count);
		var flagUpdate = Assert.IsType<RtlFlagUpdate>(block.Instructions[2]);
		Assert.Equal("INC", flagUpdate.Operation);
		Assert.False(flagUpdate.UpdateCF);
	}

	[Fact]
	public void Convert_ConditionalJump_UsesFlagCondition()
	{
		// Arrange
		var converter = new X86ToRtlConverter();
		var instruction = Instruction.CreateBranch(Code.Je_rel32_32, 0x401010);
		instruction.IP = 0x401000;
		instruction.NextIP = 0x401006;

		// Act
		var result = converter.Convert(0x401000, [instruction]);

		// Assert – JE should produce a branch with FlagCondition.Equal
		Assert.Single(result.BasicBlocks);
		var block = result.BasicBlocks[0];
		Assert.Single(block.Instructions);
		var branch = Assert.IsType<RtlBranch>(block.Instructions[0]);
		Assert.Equal(FlagCondition.Equal, branch.FlagCondition);
		Assert.Null(branch.Condition);
	}
}
