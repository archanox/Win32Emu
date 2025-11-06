using Iced.Intel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Cpu.Iced;

/// <summary>
/// Provides detailed analysis of x86 instructions using the iced library's InstructionInfo API.
/// This enables advanced debugging and instruction introspection capabilities.
/// </summary>
public class InstructionAnalyzer
{
	private readonly ILogger _logger;
	private readonly InstructionInfoFactory _infoFactory;
	private readonly Formatter _formatter;

	public InstructionAnalyzer(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
		_infoFactory = new InstructionInfoFactory();
		
		// Use NasmFormatter for consistent, readable output
		_formatter = new NasmFormatter();
		_formatter.Options.DigitSeparator = "`";
		_formatter.Options.FirstOperandCharIndex = 10;
	}

	/// <summary>
	/// Gets detailed information about an instruction including read/written registers and memory.
	/// </summary>
	public InstructionInfo GetInstructionInfo(in Instruction instruction)
	{
		return _infoFactory.GetInfo(instruction);
	}

	/// <summary>
	/// Formats an instruction to a human-readable string for debugging.
	/// </summary>
	public string FormatInstruction(in Instruction instruction)
	{
		var output = new StringOutput();
		_formatter.Format(instruction, output);
		return output.ToStringAndReset();
	}

	/// <summary>
	/// Formats an instruction with its address for debugging.
	/// </summary>
	public string FormatInstructionWithAddress(in Instruction instruction)
	{
		var output = new StringOutput();
		_formatter.Format(instruction, output);
		return $"{instruction.IP:X8} {output.ToStringAndReset()}";
	}

	/// <summary>
	/// Gets a detailed analysis of an instruction including all read/written registers and memory.
	/// </summary>
	public InstructionAnalysis AnalyzeInstruction(in Instruction instruction)
	{
		var info = _infoFactory.GetInfo(instruction);
		var analysis = new InstructionAnalysis
		{
			FormattedInstruction = FormatInstruction(instruction),
			Address = instruction.IP,
			Length = instruction.Length,
			Mnemonic = instruction.Mnemonic.ToString(),
			OpCodeString = instruction.OpCode.ToString()
		};

		// Collect read and written registers in a single pass
		foreach (var regInfo in info.GetUsedRegisters())
		{
			var access = regInfo.Access;
			if (access is OpAccess.Read or OpAccess.ReadWrite or OpAccess.ReadCondWrite)
			{
				analysis.ReadRegisters.Add(regInfo.Register.ToString());
			}
			
			if (access is OpAccess.Write or OpAccess.ReadWrite or OpAccess.CondWrite or OpAccess.ReadCondWrite)
			{
				analysis.WrittenRegisters.Add(regInfo.Register.ToString());
			}
		}

		// Collect memory accesses
		foreach (var memInfo in info.GetUsedMemory())
		{
			var memAccess = new MemoryAccess
			{
				Segment = memInfo.Segment.ToString(),
				Base = memInfo.Base.ToString(),
				Index = memInfo.Index.ToString(),
				Scale = memInfo.Scale,
				Displacement = memInfo.Displacement,
				Access = memInfo.Access.ToString()
			};
			analysis.MemoryAccesses.Add(memAccess);
		}

		return analysis;
	}

	/// <summary>
	/// Calculates the virtual address of a memory operand.
	/// </summary>
	public ulong? TryGetVirtualAddress(in Instruction instruction, int operand, 
		VATryGetRegisterValue getRegisterValue)
	{
		if (!instruction.TryGetVirtualAddress(operand, 0, out var address, getRegisterValue))
		{
			return null;
		}
		return address;
	}
}