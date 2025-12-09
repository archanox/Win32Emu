using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32.Modules;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for newly implemented MSVCRT functions
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class MsvcrtNewFunctionsTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly MsvcrtModule _msvcrt;

	public MsvcrtNewFunctionsTests()
	{
		_testEnv = new TestEnvironment();
		_msvcrt = new MsvcrtModule(_testEnv.ProcessEnv, 0x00400000, _testEnv.PeLoader, NullLogger.Instance);
		_testEnv.Dispatcher.RegisterModule(_msvcrt);
	}

	[Fact]
	public void Atoi_WithValidNumber_ShouldReturnInteger()
	{
		// Arrange - allocate string "123"
		var strPtr = _testEnv.ProcessEnv.WriteAnsiString("123\0");

		// Act - call atoi
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, strPtr);
		var success = _msvcrt.TryInvokeUnsafe("atoi", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "atoi should be implemented");
		Assert.Equal(123, unchecked((int)returnValue));
	}

	[Fact]
	public void Atoi_WithNegativeNumber_ShouldReturnNegativeInteger()
	{
		// Arrange - allocate string "-456"
		var strPtr = _testEnv.ProcessEnv.WriteAnsiString("-456\0");

		// Act - call atoi
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, strPtr);
		var success = _msvcrt.TryInvokeUnsafe("atoi", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "atoi should be implemented");
		Assert.Equal(-456, unchecked((int)returnValue));
	}

	[Fact]
	public void Atoi_WithInvalidString_ShouldReturnZero()
	{
		// Arrange - allocate string "abc"
		var strPtr = _testEnv.ProcessEnv.WriteAnsiString("abc\0");

		// Act - call atoi
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, strPtr);
		var success = _msvcrt.TryInvokeUnsafe("atoi", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "atoi should be implemented");
		Assert.Equal(0, unchecked((int)returnValue));
	}

	[Fact]
	public void Strcpy_WithValidString_ShouldCopyString()
	{
		// Arrange - allocate source string and destination buffer
		var srcPtr = _testEnv.ProcessEnv.WriteAnsiString("Hello World\0");
		var destPtr = _testEnv.ProcessEnv.HeapAlloc(0, 50);

		// Act - call strcpy
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, destPtr, srcPtr);
		var success = _msvcrt.TryInvokeUnsafe("strcpy", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "strcpy should be implemented");
		Assert.Equal(destPtr, returnValue); // Should return destination
		var copied = _testEnv.ProcessEnv.ReadAnsiString(destPtr);
		Assert.Equal("Hello World", copied);
	}

	[Fact]
	public void Strdup_WithValidString_ShouldAllocateAndCopy()
	{
		// Arrange - allocate string
		var strPtr = _testEnv.ProcessEnv.WriteAnsiString("Test String\0");

		// Act - call _strdup
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, strPtr);
		var success = _msvcrt.TryInvokeUnsafe("_strdup", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_strdup should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return allocated pointer
		Assert.NotEqual(strPtr, returnValue); // Should be different pointer
		var duplicated = _testEnv.ProcessEnv.ReadAnsiString(returnValue);
		Assert.Equal("Test String", duplicated);
	}

	[Fact]
	public void Fwrite_WithValidParameters_ShouldSucceed()
	{
		// Arrange - allocate buffer and stream
		var bufferPtr = _testEnv.ProcessEnv.WriteAnsiString("data\0");
		var streamPtr = 0x10000000u;
		var size = 1u;
		var count = 4u;

		// Act - call fwrite
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, bufferPtr, size, count, streamPtr);
		var success = _msvcrt.TryInvokeUnsafe("fwrite", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "fwrite should be implemented");
		Assert.Equal(count, returnValue); // Should return number of items written
	}

	[Fact]
	public void Onexit_WithValidFunction_ShouldReturnFunction()
	{
		// Arrange - function pointer
		var funcPtr = 0x12345678u;

		// Act - call _onexit
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, funcPtr);
		var success = _msvcrt.TryInvokeUnsafe("_onexit", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_onexit should be implemented");
		Assert.Equal(funcPtr, returnValue); // Should return function pointer
	}

	[Fact]
	public void DllOnexit_WithValidFunction_ShouldReturnFunction()
	{
		// Arrange - function pointer and table pointers
		var funcPtr = 0x12345678u;
		var pbegin = 0x20000000u;
		var pend = 0x20000100u;

		// Act - call __dllonexit
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, funcPtr, pbegin, pend);
		var success = _msvcrt.TryInvokeUnsafe("__dllonexit", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "__dllonexit should be implemented");
		Assert.Equal(funcPtr, returnValue); // Should return function pointer
	}

	[Fact]
	public void Lock_ShouldSucceed()
	{
		// Arrange - lock number
		var locknum = 5;

		// Act - call _lock
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, unchecked((uint)locknum));
		var success = _msvcrt.TryInvokeUnsafe("_lock", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_lock should be implemented");
		Assert.Equal(0u, returnValue); // void function returns 0
	}

	[Fact]
	public void Unlock_ShouldSucceed()
	{
		// Arrange - lock number
		var locknum = 5;

		// Act - first lock, then unlock
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, unchecked((uint)locknum));
		_msvcrt.TryInvokeUnsafe("_lock", _testEnv.Cpu, _testEnv.Memory, out _);
		
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, unchecked((uint)locknum));
		var success = _msvcrt.TryInvokeUnsafe("_unlock", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_unlock should be implemented");
		Assert.Equal(0u, returnValue); // void function returns 0
	}

	[Fact]
	public void Initenv_ShouldReturnPointer()
	{
		// Arrange - no parameters needed

		// Act - call __initenv
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success = _msvcrt.TryInvokeUnsafe("__initenv", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "__initenv should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return non-null pointer
	}

	[Fact]
	public void LconvInit_ShouldSucceed()
	{
		// Arrange - no parameters needed

		// Act - call __lconv_init
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success = _msvcrt.TryInvokeUnsafe("__lconv_init", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "__lconv_init should be implemented");
		Assert.Equal(0u, returnValue); // Success returns 0
	}

	[Fact]
	public void Iob_ShouldReturnPointer()
	{
		// Arrange - no parameters needed

		// Act - call _iob
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success = _msvcrt.TryInvokeUnsafe("_iob", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_iob should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return non-null pointer
	}

	[Fact]
	public void Vsnprintf_WithValidParameters_ShouldFormatString()
	{
		// Arrange - allocate buffer and format string
		var bufferPtr = _testEnv.ProcessEnv.HeapAlloc(0, 50);
		var count = 50u;
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Hello\0");
		var argsPtr = 0u; // No args for this simple test

		// Act - call _vsnprintf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, bufferPtr, count, formatPtr, argsPtr);
		var success = _msvcrt.TryInvokeUnsafe("_vsnprintf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_vsnprintf should be implemented");
		var result = unchecked((int)returnValue);
		Assert.True(result >= 0, "Should return non-negative length");
		var written = _testEnv.ProcessEnv.ReadAnsiString(bufferPtr);
		Assert.Equal("Hello", written);
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
