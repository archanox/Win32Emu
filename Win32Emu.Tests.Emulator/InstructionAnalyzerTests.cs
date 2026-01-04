using Iced.Intel;
using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for instruction analysis capabilities in JitCpu when running in interpreter mode.
/// These tests verify that JitCpu can provide instruction-level debugging information
/// similar to the deprecated IcedCpu, fulfilling the requirement in ICEDCPU_DEPRECATION.md
/// to implement instruction analysis in JitCpu's interpreter mode.
/// </summary>
public class InstructionAnalyzerTests
{
	[Fact]
	public void InstructionAnalyzer_FormatsInstruction()
	{
		// Arrange
		var memory = new VirtualMemory(0x10000);
		// Force interpreter mode and enable instruction analyzer
		var cpu = new JitCpu(memory, logger: null, enableInstructionAnalyzer: true, forceInterpreterMode: true);
		
		// Write a simple MOV instruction: mov eax, ebx
		memory.Write8(0x1000, 0x89); // opcode
		memory.Write8(0x1001, 0xD8); // ModR/M byte
		
		cpu.SetEip(0x1000);
		
		// Act
		var formatted = cpu.FormatCurrentInstruction();
		
		// Assert
		Assert.NotNull(formatted);
		Assert.Contains("mov", formatted.ToLower());
	}

	[Fact]
	public void InstructionAnalyzer_AnalyzesInstruction()
	{
		// Arrange
		var memory = new VirtualMemory(0x10000);
		// Force interpreter mode and enable instruction analyzer
		var cpu = new JitCpu(memory, logger: null, enableInstructionAnalyzer: true, forceInterpreterMode: true);
		
		// Write: mov eax, ebx (89 D8)
		memory.Write8(0x1000, 0x89);
		memory.Write8(0x1001, 0xD8);
		
		cpu.SetEip(0x1000);
		
		// Act
		var analysis = cpu.AnalyzeCurrentInstruction();
		
		// Assert
		Assert.NotNull(analysis);
		Assert.Equal("Mov", analysis.Mnemonic);
		Assert.Equal(2, analysis.Length);
		
		// MOV reads from EBX and writes to EAX
		Assert.Contains("EBX", analysis.ReadRegisters);
		Assert.Contains("EAX", analysis.WrittenRegisters);
	}

	[Fact]
	public void InstructionAnalyzer_DetectsMemoryAccess()
	{
		// Arrange
		var memory = new VirtualMemory(0x10000);
		// Force interpreter mode and enable instruction analyzer
		var cpu = new JitCpu(memory, logger: null, enableInstructionAnalyzer: true, forceInterpreterMode: true);
		
		// Write: mov eax, [ebx] (8B 03)
		memory.Write8(0x1000, 0x8B);
		memory.Write8(0x1001, 0x03);
		
		cpu.SetEip(0x1000);
		
		// Act
		var analysis = cpu.AnalyzeCurrentInstruction();
		
		// Assert
		Assert.NotNull(analysis);
		Assert.Single(analysis.MemoryAccesses);
		
		var memAccess = analysis.MemoryAccesses[0];
		Assert.Equal("EBX", memAccess.Base);
		Assert.Contains("Read", memAccess.Access);
	}

	[Fact]
	public void InstructionAnalyzer_NotEnabledReturnsNull()
	{
		// Arrange
		var memory = new VirtualMemory(0x10000);
		// Force interpreter mode but don't enable instruction analyzer
		var cpu = new JitCpu(memory, logger: null, enableInstructionAnalyzer: false, forceInterpreterMode: true);
		
		// Write a simple instruction
		memory.Write8(0x1000, 0x90); // NOP
		cpu.SetEip(0x1000);
		
		// Act
		var analysis = cpu.AnalyzeCurrentInstruction();
		
		// Assert
		Assert.Null(analysis);
	}

	[Fact]
	public void InstructionAnalyzer_FormatCurrentInstruction_ThrowsWhenNotEnabled()
	{
		// Arrange
		var memory = new VirtualMemory(0x10000);
		// Force interpreter mode but don't enable instruction analyzer
		var cpu = new JitCpu(memory, logger: null, enableInstructionAnalyzer: false, forceInterpreterMode: true);
		
		// Write a simple instruction
		memory.Write8(0x1000, 0x90); // NOP
		cpu.SetEip(0x1000);
		
		// Act & Assert
		var ex = Assert.Throws<InvalidOperationException>(() => cpu.FormatCurrentInstruction());
		Assert.Contains("Instruction analyzer is not enabled", ex.Message);
	}

	[Fact]
	public void DecoderOptions_CanBeSet()
	{
		// Arrange - Test that decoder options can be passed without error
		var memory = new VirtualMemory(0x10000);
		var options = DecoderOptions.MPX | DecoderOptions.Cyrix;
		
		// Act - Should not throw
		var cpu = new JitCpu(memory, logger: null, decoderOptions: options, forceInterpreterMode: true);
		
		// Assert
		Assert.NotNull(cpu);
	}

	[Fact]
	public void InstructionAnalyzer_CanBeRetrieved()
	{
		// Arrange
		var memory = new VirtualMemory(0x10000);
		var cpuWithAnalyzer = new JitCpu(memory, logger: null, enableInstructionAnalyzer: true, forceInterpreterMode: true);
		var cpuWithoutAnalyzer = new JitCpu(memory, logger: null, enableInstructionAnalyzer: false, forceInterpreterMode: true);
		
		// Act
		var analyzerEnabled = cpuWithAnalyzer.GetInstructionAnalyzer();
		var analyzerDisabled = cpuWithoutAnalyzer.GetInstructionAnalyzer();
		
		// Assert
		Assert.NotNull(analyzerEnabled);
		Assert.Null(analyzerDisabled);
	}

	[Fact]
	public void InstructionAnalyzer_FormatsWithAddress()
	{
		// Arrange
		var memory = new VirtualMemory(0x00500000);  // Larger memory to accommodate address
		// Force interpreter mode and enable instruction analyzer
		var cpu = new JitCpu(memory, logger: null, enableInstructionAnalyzer: true, forceInterpreterMode: true);
		
		// Write: nop (90)
		memory.Write8(0x00401000, 0x90);
		cpu.SetEip(0x00401000);
		
		// Act
		var formatted = cpu.FormatCurrentInstruction();
		
		// Assert
		Assert.NotNull(formatted);
		Assert.Contains("00401000", formatted);
	}
}
