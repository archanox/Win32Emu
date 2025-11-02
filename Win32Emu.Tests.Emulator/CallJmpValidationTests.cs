using Microsoft.Extensions.Logging;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Win32Emu.Tests.Emulator.TestInfrastructure;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for CALL and JMP instruction target validation
/// These tests verify that suspicious call/jump targets are detected and logged
/// </summary>
public class CallJmpValidationTests : IDisposable
{
	private readonly CpuTestHelper _helper;
	private readonly TestLogger _logger;

	public CallJmpValidationTests(ITestOutputHelper output)
	{
		_logger = new TestLogger(output);
		_helper = new CpuTestHelper();
	}

	[Fact]
	public void CALL_Register_WithLowAddress_ShouldLogWarning()
	{
		// Arrange: CALL EBX (FF D3) with EBX containing a suspiciously low address
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, _logger);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("ESP", 0x00100000);
		cpu.SetRegister("EBX", 0x00001000); // Suspiciously low address
		
		// Write CALL EBX instruction
		memory.Write8(0x00400000, 0xFF); // CALL r/m32
		memory.Write8(0x00400001, 0xD3); // ModRM: 11 010 011 = register EBX

		// Act
		cpu.SingleStep(memory);

		// Assert
		Assert.Contains(_logger.Logs, log => 
			log.Contains("CALL") && 
			log.Contains("0x00001000") && 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00001000u, cpu.GetEip()); // EIP should still be set to the target
	}

	[Fact]
	public void CALL_Memory_WithLowAddress_ShouldLogWarning()
	{
		// Arrange: CALL [EBX] with memory containing a suspiciously low address
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, _logger);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("ESP", 0x00100000);
		cpu.SetRegister("EBX", 0x00450000);
		
		// Write low address to memory
		memory.Write32(0x00450000, 0x00002000); // Suspiciously low address
		
		// Write CALL [EBX] instruction
		memory.Write8(0x00400000, 0xFF); // CALL r/m32
		memory.Write8(0x00400001, 0x13); // ModRM: 00 010 011 = memory indirect through EBX

		// Act
		cpu.SingleStep(memory);

		// Assert
		Assert.Contains(_logger.Logs, log => 
			log.Contains("CALL") && 
			log.Contains("0x00002000") && 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00002000u, cpu.GetEip()); // EIP should still be set to the target
	}

	[Fact]
	public void CALL_Register_WithValidAddress_ShouldNotLogWarning()
	{
		// Arrange: CALL EBX with EBX containing a valid address
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, _logger);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("ESP", 0x00100000);
		cpu.SetRegister("EBX", 0x00401000); // Valid address

		// Write CALL EBX instruction
		memory.Write8(0x00400000, 0xFF); // CALL r/m32
		memory.Write8(0x00400001, 0xD3); // ModRM: 11 010 011 = register EBX

		// Act
		cpu.SingleStep(memory);

		// Assert
		Assert.DoesNotContain(_logger.Logs, log => 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00401000u, cpu.GetEip());
	}

	[Fact]
	public void JMP_Register_WithLowAddress_ShouldLogWarning()
	{
		// Arrange: JMP EAX with EAX containing a suspiciously low address
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, _logger);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("EAX", 0x00003000); // Suspiciously low address
		
		// Write JMP EAX instruction
		memory.Write8(0x00400000, 0xFF); // JMP r/m32
		memory.Write8(0x00400001, 0xE0); // ModRM: 11 100 000 = register EAX

		// Act
		cpu.SingleStep(memory);

		// Assert
		Assert.Contains(_logger.Logs, log => 
			log.Contains("JMP") && 
			log.Contains("0x00003000") && 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00003000u, cpu.GetEip());
	}

	[Fact]
	public void JMP_Memory_WithLowAddress_ShouldLogWarning()
	{
		// Arrange: JMP [ECX] with memory containing a suspiciously low address
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, _logger);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("ECX", 0x00450000);
		
		// Write low address to memory
		memory.Write32(0x00450000, 0x00004000); // Suspiciously low address
		
		// Write JMP [ECX] instruction
		memory.Write8(0x00400000, 0xFF); // JMP r/m32
		memory.Write8(0x00400001, 0x21); // ModRM: 00 100 001 = memory indirect through ECX

		// Act
		cpu.SingleStep(memory);

		// Assert
		Assert.Contains(_logger.Logs, log => 
			log.Contains("JMP") && 
			log.Contains("0x00004000") && 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00004000u, cpu.GetEip());
	}

	[Fact]
	public void CALL_Register_WithZeroAddress_ShouldNotLogWarning()
	{
		// Arrange: CALL EBX with EBX = 0 (NULL pointer, explicitly allowed)
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, _logger);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("ESP", 0x00100000);
		cpu.SetRegister("EBX", 0x00000000); // NULL pointer

		// Write CALL EBX instruction
		memory.Write8(0x00400000, 0xFF); // CALL r/m32
		memory.Write8(0x00400001, 0xD3); // ModRM: 11 010 011 = register EBX

		// Act
		cpu.SingleStep(memory);

		// Assert - NULL is explicitly allowed to avoid false positives
		Assert.DoesNotContain(_logger.Logs, log => 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00000000u, cpu.GetEip());
	}

	public void Dispose()
	{
		_helper?.Dispose();
		GC.SuppressFinalize(this);
	}
}

/// <summary>
/// Simple test logger that captures log messages for assertions
/// </summary>
internal class TestLogger : ILogger
{
	private readonly ITestOutputHelper _output;
	public List<string> Logs { get; } = new();

	public TestLogger(ITestOutputHelper output)
	{
		_output = output;
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		var message = formatter(state, exception);
		Logs.Add(message);
		_output.WriteLine($"[{logLevel}] {message}");
	}
}
