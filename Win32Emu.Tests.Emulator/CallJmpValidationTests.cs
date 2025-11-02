using Microsoft.Extensions.Logging;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for CALL and JMP instruction target validation
/// These tests verify that suspicious call/jump targets are detected and logged
/// </summary>
public class CallJmpValidationTests
{
	[Fact]
	public void CALL_Register_WithLowAddress_ShouldLogWarning()
	{
		// Arrange: CALL EBX (FF D3) with EBX containing a suspiciously low address
		var output = new TestOutputHelper();
		var logger = new TestLogger(output);
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, logger);
		
		var originalEip = 0x00400000u;
		var originalEsp = 0x00100000u;
		cpu.SetEip(originalEip);
		cpu.SetRegister("ESP", originalEsp);
		cpu.SetRegister("EBX", 0x00001000); // Suspiciously low address
		
		// Write CALL EBX instruction (2 bytes)
		memory.Write8(0x00400000, 0xFF); // CALL r/m32
		memory.Write8(0x00400001, 0xD3); // ModRM: 11 010 011 = register EBX

		// Act
		cpu.SingleStep(memory);

		// Assert
		Assert.Contains(logger.Logs, log => 
			log.Contains("CALL") && 
			log.Contains("0x00001000") && 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00001000u, cpu.GetEip()); // EIP should still be set to the target
		
		// Verify CALL semantics: return address pushed to stack
		var expectedReturnAddress = originalEip + 2; // 2-byte instruction
		var expectedEsp = originalEsp - 4; // ESP decremented by 4
		Assert.Equal(expectedEsp, cpu.GetRegister("ESP"));
		Assert.Equal(expectedReturnAddress, memory.Read32(expectedEsp));
	}

	[Fact]
	public void CALL_Memory_WithLowAddress_ShouldLogWarning()
	{
		// Arrange: CALL [EBX] with memory containing a suspiciously low address
		var output = new TestOutputHelper();
		var logger = new TestLogger(output);
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, logger);
		
		var originalEip = 0x00400000u;
		var originalEsp = 0x00100000u;
		cpu.SetEip(originalEip);
		cpu.SetRegister("ESP", originalEsp);
		cpu.SetRegister("EBX", 0x00450000);
		
		// Write low address to memory
		memory.Write32(0x00450000, 0x00002000); // Suspiciously low address
		
		// Write CALL [EBX] instruction (2 bytes)
		memory.Write8(0x00400000, 0xFF); // CALL r/m32
		memory.Write8(0x00400001, 0x13); // ModRM: 00 010 011 = memory indirect through EBX

		// Act
		cpu.SingleStep(memory);

		// Assert
		Assert.Contains(logger.Logs, log => 
			log.Contains("CALL") && 
			log.Contains("0x00002000") && 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00002000u, cpu.GetEip()); // EIP should still be set to the target
		
		// Verify CALL semantics: return address pushed to stack
		var expectedReturnAddress = originalEip + 2; // 2-byte instruction
		var expectedEsp = originalEsp - 4; // ESP decremented by 4
		Assert.Equal(expectedEsp, cpu.GetRegister("ESP"));
		Assert.Equal(expectedReturnAddress, memory.Read32(expectedEsp));
	}

	[Fact]
	public void CALL_Register_WithValidAddress_ShouldNotLogWarning()
	{
		// Arrange: CALL EBX with EBX containing a valid address
		var output = new TestOutputHelper();
		var logger = new TestLogger(output);
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, logger);
		
		var originalEip = 0x00400000u;
		var originalEsp = 0x00100000u;
		cpu.SetEip(originalEip);
		cpu.SetRegister("ESP", originalEsp);
		cpu.SetRegister("EBX", 0x00401000); // Valid address

		// Write CALL EBX instruction (2 bytes)
		memory.Write8(0x00400000, 0xFF); // CALL r/m32
		memory.Write8(0x00400001, 0xD3); // ModRM: 11 010 011 = register EBX

		// Act
		cpu.SingleStep(memory);

		// Assert
		Assert.DoesNotContain(logger.Logs, log => 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00401000u, cpu.GetEip());
		
		// Verify CALL semantics: return address pushed to stack
		var expectedReturnAddress = originalEip + 2; // 2-byte instruction
		var expectedEsp = originalEsp - 4; // ESP decremented by 4
		Assert.Equal(expectedEsp, cpu.GetRegister("ESP"));
		Assert.Equal(expectedReturnAddress, memory.Read32(expectedEsp));
	}

	[Fact]
	public void JMP_Register_WithLowAddress_ShouldLogWarning()
	{
		// Arrange: JMP EAX with EAX containing a suspiciously low address
		var output = new TestOutputHelper();
		var logger = new TestLogger(output);
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, logger);
		
		cpu.SetEip(0x00400000);
		cpu.SetRegister("EAX", 0x00003000); // Suspiciously low address
		
		// Write JMP EAX instruction
		memory.Write8(0x00400000, 0xFF); // JMP r/m32
		memory.Write8(0x00400001, 0xE0); // ModRM: 11 100 000 = register EAX

		// Act
		cpu.SingleStep(memory);

		// Assert
		Assert.Contains(logger.Logs, log => 
			log.Contains("JMP") && 
			log.Contains("0x00003000") && 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00003000u, cpu.GetEip());
	}

	[Fact]
	public void JMP_Memory_WithLowAddress_ShouldLogWarning()
	{
		// Arrange: JMP [ECX] with memory containing a suspiciously low address
		var output = new TestOutputHelper();
		var logger = new TestLogger(output);
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, logger);
		
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
		Assert.Contains(logger.Logs, log => 
			log.Contains("JMP") && 
			log.Contains("0x00004000") && 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00004000u, cpu.GetEip());
	}

	[Fact]
	public void CALL_Register_WithZeroAddress_ShouldNotLogWarning()
	{
		// Arrange: CALL EBX with EBX = 0 (NULL pointer, explicitly allowed)
		var output = new TestOutputHelper();
		var logger = new TestLogger(output);
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, logger);
		
		var originalEip = 0x00400000u;
		var originalEsp = 0x00100000u;
		cpu.SetEip(originalEip);
		cpu.SetRegister("ESP", originalEsp);
		cpu.SetRegister("EBX", 0x00000000); // NULL pointer

		// Write CALL EBX instruction (2 bytes)
		memory.Write8(0x00400000, 0xFF); // CALL r/m32
		memory.Write8(0x00400001, 0xD3); // ModRM: 11 010 011 = register EBX

		// Act
		cpu.SingleStep(memory);

		// Assert - NULL is explicitly allowed to avoid false positives
		Assert.DoesNotContain(logger.Logs, log => 
			log.Contains("suspiciously low"));
		Assert.Equal(0x00000000u, cpu.GetEip());
		
		// Verify CALL semantics: return address pushed to stack
		var expectedReturnAddress = originalEip + 2; // 2-byte instruction
		var expectedEsp = originalEsp - 4; // ESP decremented by 4
		Assert.Equal(expectedEsp, cpu.GetRegister("ESP"));
		Assert.Equal(expectedReturnAddress, memory.Read32(expectedEsp));
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

/// <summary>
/// Simple implementation of ITestOutputHelper for use without xUnit test context
/// </summary>
internal class TestOutputHelper : ITestOutputHelper
{
	private readonly List<string> _output = new();

	public void WriteLine(string message)
	{
		_output.Add(message);
		Console.WriteLine(message);
	}

	public void WriteLine(string format, params object[] args)
	{
		WriteLine(string.Format(format, args));
	}
}

