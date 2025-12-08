using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32.Modules;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for MSVCRT string and I/O functions
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class MsvcrtStringAndIoTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly MsvcrtModule _msvcrt;

	public MsvcrtStringAndIoTests()
	{
		_testEnv = new TestEnvironment();
		_msvcrt = new MsvcrtModule(_testEnv.ProcessEnv, 0x00400000, _testEnv.PeLoader, NullLogger.Instance);
		_testEnv.Dispatcher.RegisterModule(_msvcrt);
	}

	[Fact]
	public void Strcmp_WithEqualStrings_ShouldReturnZero()
	{
		// Arrange - allocate two equal strings
		var str1Ptr = _testEnv.ProcessEnv.WriteAnsiString("hello\0");
		var str2Ptr = _testEnv.ProcessEnv.WriteAnsiString("hello\0");

		// Act - call strcmp
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, str1Ptr, str2Ptr);
		var success = _msvcrt.TryInvokeUnsafe("strcmp", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "strcmp should be implemented");
		Assert.Equal(0u, returnValue); // Equal strings return 0
	}

	[Fact]
	public void Strcmp_WithFirstStringLess_ShouldReturnNegative()
	{
		// Arrange - str1 < str2
		var str1Ptr = _testEnv.ProcessEnv.WriteAnsiString("abc\0");
		var str2Ptr = _testEnv.ProcessEnv.WriteAnsiString("xyz\0");

		// Act - call strcmp
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, str1Ptr, str2Ptr);
		var success = _msvcrt.TryInvokeUnsafe("strcmp", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "strcmp should be implemented");
		// Convert to signed int for comparison
		var result = unchecked((int)returnValue);
		Assert.True(result < 0, "First string less than second should return negative value");
	}

	[Fact]
	public void Strcmp_WithFirstStringGreater_ShouldReturnPositive()
	{
		// Arrange - str1 > str2
		var str1Ptr = _testEnv.ProcessEnv.WriteAnsiString("xyz\0");
		var str2Ptr = _testEnv.ProcessEnv.WriteAnsiString("abc\0");

		// Act - call strcmp
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, str1Ptr, str2Ptr);
		var success = _msvcrt.TryInvokeUnsafe("strcmp", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "strcmp should be implemented");
		// Convert to signed int for comparison
		var result = unchecked((int)returnValue);
		Assert.True(result > 0, "First string greater than second should return positive value");
	}

	[Fact]
	public void Fpreset_ShouldSucceed()
	{
		// Arrange - no parameters needed

		// Act - call _fpreset
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success = _msvcrt.TryInvokeUnsafe("_fpreset", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_fpreset should be implemented");
		Assert.Equal(0u, returnValue); // _fpreset returns void (0)
	}

	[Fact]
	public void SetInvalidParameterHandler_ShouldReturnOldHandler()
	{
		// Arrange - provide a handler address
		var handlerPtr = 0x12345678u;

		// Act - call _set_invalid_parameter_handler
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, handlerPtr);
		var success = _msvcrt.TryInvokeUnsafe("_set_invalid_parameter_handler", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_set_invalid_parameter_handler should be implemented");
		// Should return old handler (we return 0 since we don't track it)
		Assert.Equal(0u, returnValue);
	}

	[Fact]
	public void Fflush_WithValidStream_ShouldSucceed()
	{
		// Arrange - provide a stream pointer (we stub this, so any value works)
		var streamPtr = 0x10000000u;

		// Act - call fflush
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, streamPtr);
		var success = _msvcrt.TryInvokeUnsafe("fflush", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "fflush should be implemented");
		Assert.Equal(0u, returnValue); // Success returns 0
	}

	[Fact]
	public void Fflush_WithNullStream_ShouldSucceed()
	{
		// Arrange - NULL stream should flush all streams
		var streamPtr = 0u;

		// Act - call fflush
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, streamPtr);
		var success = _msvcrt.TryInvokeUnsafe("fflush", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "fflush should handle NULL stream");
		Assert.Equal(0u, returnValue); // Success returns 0
	}

	[Fact]
	public void Setvbuf_WithFullBuffering_ShouldSucceed()
	{
		// Arrange - setvbuf(stream, buffer, _IOFBF, size)
		var streamPtr = 0x10000000u;
		var bufferPtr = 0x20000000u;
		var mode = 0; // _IOFBF = full buffering
		var size = 1024u;

		// Act - call setvbuf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, streamPtr, bufferPtr, (uint)mode, size);
		var success = _msvcrt.TryInvokeUnsafe("setvbuf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "setvbuf should be implemented");
		Assert.Equal(0u, returnValue); // Success returns 0
	}

	[Fact]
	public void Setvbuf_WithNoBuffering_ShouldSucceed()
	{
		// Arrange - setvbuf(stream, NULL, _IONBF, 0)
		var streamPtr = 0x10000000u;
		var bufferPtr = 0u; // NULL
		var mode = 2; // _IONBF = no buffering
		var size = 0u;

		// Act - call setvbuf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, streamPtr, bufferPtr, (uint)mode, size);
		var success = _msvcrt.TryInvokeUnsafe("setvbuf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "setvbuf should handle no buffering mode");
		Assert.Equal(0u, returnValue); // Success returns 0
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
