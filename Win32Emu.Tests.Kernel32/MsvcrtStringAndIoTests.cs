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
		const int IOFBF = 0; // Full buffering
		var streamPtr = 0x10000000u;
		var bufferPtr = 0x20000000u;
		var mode = IOFBF;
		var size = 1024u;

		// Act - call setvbuf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, streamPtr, bufferPtr, unchecked((uint)mode), size);
		var success = _msvcrt.TryInvokeUnsafe("setvbuf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "setvbuf should be implemented");
		Assert.Equal(0u, returnValue); // Success returns 0
	}

	[Fact]
	public void Setvbuf_WithNoBuffering_ShouldSucceed()
	{
		// Arrange - setvbuf(stream, NULL, _IONBF, 0)
		const int IONBF = 2; // No buffering
		var streamPtr = 0x10000000u;
		var bufferPtr = 0u; // NULL
		var mode = IONBF;
		var size = 0u;

		// Act - call setvbuf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, streamPtr, bufferPtr, unchecked((uint)mode), size);
		var success = _msvcrt.TryInvokeUnsafe("setvbuf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "setvbuf should handle no buffering mode");
		Assert.Equal(0u, returnValue); // Success returns 0
	}

	[Fact]
	public void Strnicmp_WithEqualStrings_ShouldReturnZero()
	{
		// Arrange - allocate two equal strings (case-insensitive)
		var str1Ptr = _testEnv.ProcessEnv.WriteAnsiString("Hello\0");
		var str2Ptr = _testEnv.ProcessEnv.WriteAnsiString("HELLO\0");
		var count = 5u;

		// Act - call _strnicmp
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, str1Ptr, str2Ptr, count);
		var success = _msvcrt.TryInvokeUnsafe("_strnicmp", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_strnicmp should be implemented");
		Assert.Equal(0u, returnValue); // Equal strings (case-insensitive) return 0
	}

	[Fact]
	public void Strnicmp_WithFirstStringLess_ShouldReturnNegative()
	{
		// Arrange - str1 < str2 (case-insensitive)
		var str1Ptr = _testEnv.ProcessEnv.WriteAnsiString("abc\0");
		var str2Ptr = _testEnv.ProcessEnv.WriteAnsiString("XYZ\0");
		var count = 3u;

		// Act - call _strnicmp
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, str1Ptr, str2Ptr, count);
		var success = _msvcrt.TryInvokeUnsafe("_strnicmp", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_strnicmp should be implemented");
		var result = unchecked((int)returnValue);
		Assert.True(result < 0, "First string less than second (case-insensitive) should return negative value");
	}

	[Fact]
	public void Strnicmp_WithFirstStringGreater_ShouldReturnPositive()
	{
		// Arrange - str1 > str2 (case-insensitive)
		var str1Ptr = _testEnv.ProcessEnv.WriteAnsiString("XYZ\0");
		var str2Ptr = _testEnv.ProcessEnv.WriteAnsiString("abc\0");
		var count = 3u;

		// Act - call _strnicmp
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, str1Ptr, str2Ptr, count);
		var success = _msvcrt.TryInvokeUnsafe("_strnicmp", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_strnicmp should be implemented");
		var result = unchecked((int)returnValue);
		Assert.True(result > 0, "First string greater than second (case-insensitive) should return positive value");
	}

	[Fact]
	public void Strnicmp_WithPartialMatch_ShouldCompareUpToCount()
	{
		// Arrange - strings differ after count characters
		var str1Ptr = _testEnv.ProcessEnv.WriteAnsiString("Hello World\0");
		var str2Ptr = _testEnv.ProcessEnv.WriteAnsiString("HELLO THERE\0");
		var count = 5u; // Compare only first 5 characters

		// Act - call _strnicmp
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, str1Ptr, str2Ptr, count);
		var success = _msvcrt.TryInvokeUnsafe("_strnicmp", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_strnicmp should be implemented");
		Assert.Equal(0u, returnValue); // First 5 characters match (case-insensitive)
	}

	[Fact]
	public void Strnicmp_WithCountLargerThanStrings_ShouldCompareFullStrings()
	{
		// Arrange - count is larger than string length
		var str1Ptr = _testEnv.ProcessEnv.WriteAnsiString("Hi\0");
		var str2Ptr = _testEnv.ProcessEnv.WriteAnsiString("HI\0");
		var count = 100u; // Much larger than string length

		// Act - call _strnicmp
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, str1Ptr, str2Ptr, count);
		var success = _msvcrt.TryInvokeUnsafe("_strnicmp", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_strnicmp should be implemented");
		Assert.Equal(0u, returnValue); // Strings are equal (case-insensitive)
	}

	[Fact]
	public void Strnicmp_WithZeroCount_ShouldReturnZero()
	{
		// Arrange - count is 0
		var str1Ptr = _testEnv.ProcessEnv.WriteAnsiString("abc\0");
		var str2Ptr = _testEnv.ProcessEnv.WriteAnsiString("xyz\0");
		var count = 0u; // Compare 0 characters

		// Act - call _strnicmp
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, str1Ptr, str2Ptr, count);
		var success = _msvcrt.TryInvokeUnsafe("_strnicmp", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_strnicmp should be implemented");
		Assert.Equal(0u, returnValue); // Comparing 0 characters always returns 0
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
