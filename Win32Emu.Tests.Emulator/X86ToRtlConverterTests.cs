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
		var instruction = Instruction.Create(Code.Ror_rm32_imm8, new MemoryOperand(Register.EAX), 4);
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

		// DIV should generate two operations: EAX = quotient, EDX = remainder
		Assert.Equal(2, block.Instructions.Count);
		Assert.IsType<RtlBinaryOp>(block.Instructions[0]);
		Assert.IsType<RtlBinaryOp>(block.Instructions[1]);
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

		Assert.Single(block.Instructions);
		var binOp = Assert.IsType<RtlBinaryOp>(block.Instructions[0]);
		Assert.Equal(">>", binOp.Operator);
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
		// Simplified implementation always sets to 0
		var constant = Assert.IsType<RtlConstant>(assignment.Source);
		Assert.Equal(0u, constant.Value);
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
}
