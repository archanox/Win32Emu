using Xunit;
using Win32Emu.Tests.Infrastructure;
using Win32Emu.Win32.Modules;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for MSVCRT locale and error handling functions
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class MsvcrtLocaleFunctionsTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly MsvcrtModule _msvcrt;

	public MsvcrtLocaleFunctionsTests()
	{
		_testEnv = new TestEnvironment(initializeDispatcher: true);
		_msvcrt = new MsvcrtModule(_testEnv.ProcessEnv, 0x00400000, _testEnv.PeLoader, NullLogger.Instance);
		_testEnv.Dispatcher!.RegisterModule(_msvcrt);
	}

	[Fact]
	public void MbCurMax_ShouldReturnPointerToValue()
	{
		// Arrange - no parameters needed

		// Act - call __mb_cur_max
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success = _msvcrt.TryInvokeUnsafe("__mb_cur_max", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "__mb_cur_max should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read the value at the pointer - should be 1 for C locale
		var value = _testEnv.Memory.Read32(returnValue);
		Assert.Equal(1u, value);
	}

	[Fact]
	public void MbCurMax_ShouldReturnSamePointerOnMultipleCalls()
	{
		// Arrange - no parameters needed

		// Act - call __mb_cur_max twice
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success1 = _msvcrt.TryInvokeUnsafe("__mb_cur_max", _testEnv.Cpu, _testEnv.Memory, out var ptr1);
		
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success2 = _msvcrt.TryInvokeUnsafe("__mb_cur_max", _testEnv.Cpu, _testEnv.Memory, out var ptr2);

		// Assert - should return the same pointer both times
		Assert.True(success1 && success2, "__mb_cur_max should be implemented");
		Assert.Equal(ptr1, ptr2);
	}

	[Fact]
	public void Errno_ShouldReturnPointerToValue()
	{
		// Arrange - no parameters needed

		// Act - call _errno
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success = _msvcrt.TryInvokeUnsafe("_errno", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "_errno should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read the value at the pointer - should be 0 initially (no error)
		var value = _testEnv.Memory.Read32(returnValue);
		Assert.Equal(0u, value);
	}

	[Fact]
	public void Errno_ShouldAllowSettingValue()
	{
		// Arrange - get errno pointer
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success = _msvcrt.TryInvokeUnsafe("_errno", _testEnv.Cpu, _testEnv.Memory, out var errnoPtr);
		Assert.True(success, "_errno should be implemented");

		// Act - write an error value to errno
		var errorCode = 22; // EINVAL
		_testEnv.Memory.Write32(errnoPtr, (uint)errorCode);

		// Assert - read it back
		var readValue = _testEnv.Memory.Read32(errnoPtr);
		Assert.Equal((uint)errorCode, readValue);
	}

	[Fact]
	public void Fputc_WithStdout_ShouldSucceed()
	{
		// Arrange - get stdout pointer and character
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var iobSuccess = _msvcrt.TryInvokeUnsafe("__p__iob", _testEnv.Cpu, _testEnv.Memory, out var iobPtr);
		Assert.True(iobSuccess, "__p__iob should be implemented");
		
		var stdoutPtr = iobPtr + 32; // stdout is at offset 32
		var character = 'A';

		// Act - call fputc
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, character, stdoutPtr);
		var success = _msvcrt.TryInvokeUnsafe("fputc", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "fputc should be implemented");
		Assert.Equal((uint)character, returnValue); // Should return the character written
	}

	[Fact]
	public void Fputc_WithStderr_ShouldSucceed()
	{
		// Arrange - get stderr pointer and character
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var iobSuccess = _msvcrt.TryInvokeUnsafe("__p__iob", _testEnv.Cpu, _testEnv.Memory, out var iobPtr);
		Assert.True(iobSuccess, "__p__iob should be implemented");
		
		var stderrPtr = iobPtr + 64; // stderr is at offset 64
		var character = 'X';

		// Act - call fputc
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, character, stderrPtr);
		var success = _msvcrt.TryInvokeUnsafe("fputc", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "fputc should be implemented");
		Assert.Equal((uint)character, returnValue); // Should return the character written
	}

	[Fact]
	public void Localeconv_ShouldReturnPointerToStructure()
	{
		// Arrange - no parameters needed

		// Act - call localeconv
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success = _msvcrt.TryInvokeUnsafe("localeconv", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "localeconv should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read decimal_point pointer (first field)
		var decimalPointPtr = _testEnv.Memory.Read32(returnValue);
		Assert.NotEqual(0u, decimalPointPtr);
		
		// Read the decimal point string - should be "."
		var decimalPoint = _testEnv.ProcessEnv.ReadAnsiString(decimalPointPtr);
		Assert.Equal(".", decimalPoint);
	}

	[Fact]
	public void Localeconv_ShouldReturnSamePointerOnMultipleCalls()
	{
		// Arrange - no parameters needed

		// Act - call localeconv twice
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success1 = _msvcrt.TryInvokeUnsafe("localeconv", _testEnv.Cpu, _testEnv.Memory, out var ptr1);
		
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory);
		var success2 = _msvcrt.TryInvokeUnsafe("localeconv", _testEnv.Cpu, _testEnv.Memory, out var ptr2);

		// Assert - should return the same pointer both times
		Assert.True(success1 && success2, "localeconv should be implemented");
		Assert.Equal(ptr1, ptr2);
	}

	[Fact]
	public void Setlocale_WithNull_ShouldReturnCurrentLocale()
	{
		// Arrange - NULL locale pointer
		var localePtr = 0u;

		// Act - call setlocale with NULL
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, localePtr);
		var success = _msvcrt.TryInvokeUnsafe("setlocale", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "setlocale should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read the locale string - should be "C"
		var locale = _testEnv.ProcessEnv.ReadAnsiString(returnValue);
		Assert.Equal("C", locale);
	}

	[Fact]
	public void Setlocale_WithCLocale_ShouldSucceed()
	{
		// Arrange - "C" locale string
		var localePtr = _testEnv.ProcessEnv.WriteAnsiString("C\0");

		// Act - call setlocale with "C"
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, localePtr);
		var success = _msvcrt.TryInvokeUnsafe("setlocale", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "setlocale should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read the locale string - should be "C"
		var locale = _testEnv.ProcessEnv.ReadAnsiString(returnValue);
		Assert.Equal("C", locale);
	}

	[Fact]
	public void Setlocale_WithEmptyString_ShouldSucceed()
	{
		// Arrange - empty locale string
		var localePtr = _testEnv.ProcessEnv.WriteAnsiString("\0");

		// Act - call setlocale with ""
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, localePtr);
		var success = _msvcrt.TryInvokeUnsafe("setlocale", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "setlocale should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read the locale string - should be "C"
		var locale = _testEnv.ProcessEnv.ReadAnsiString(returnValue);
		Assert.Equal("C", locale);
	}

	[Fact]
	public void Setlocale_WithUnsupportedLocale_ShouldReturnNull()
	{
		// Arrange - unsupported locale string
		var localePtr = _testEnv.ProcessEnv.WriteAnsiString("fr_FR.UTF-8\0");

		// Act - call setlocale with unsupported locale
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, localePtr);
		var success = _msvcrt.TryInvokeUnsafe("setlocale", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "setlocale should be implemented");
		Assert.Equal(0u, returnValue); // Should return NULL for unsupported locale
	}

	[Fact]
	public void Strerror_WithZero_ShouldReturnNoError()
	{
		// Arrange - error number 0
		var errnum = 0u;

		// Act - call strerror
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, errnum);
		var success = _msvcrt.TryInvokeUnsafe("strerror", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "strerror should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read the error message string
		var errorMsg = _testEnv.ProcessEnv.ReadAnsiString(returnValue);
		Assert.Equal("No error", errorMsg);
	}

	[Fact]
	public void Strerror_WithEINVAL_ShouldReturnInvalidArgument()
	{
		// Arrange - error number EINVAL (22)
		var errnum = 22u;

		// Act - call strerror
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, errnum);
		var success = _msvcrt.TryInvokeUnsafe("strerror", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "strerror should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read the error message string
		var errorMsg = _testEnv.ProcessEnv.ReadAnsiString(returnValue);
		Assert.Equal("Invalid argument", errorMsg);
	}

	[Fact]
	public void Strerror_WithEACCES_ShouldReturnPermissionDenied()
	{
		// Arrange - error number EACCES (13)
		var errnum = 13u;

		// Act - call strerror
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, errnum);
		var success = _msvcrt.TryInvokeUnsafe("strerror", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "strerror should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read the error message string
		var errorMsg = _testEnv.ProcessEnv.ReadAnsiString(returnValue);
		Assert.Equal("Permission denied", errorMsg);
	}

	[Fact]
	public void Strerror_WithUnknownError_ShouldReturnGenericMessage()
	{
		// Arrange - unknown error number
		var errnum = 999u;

		// Act - call strerror
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, errnum);
		var success = _msvcrt.TryInvokeUnsafe("strerror", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "strerror should be implemented");
		Assert.NotEqual(0u, returnValue); // Should return a valid pointer
		
		// Read the error message string
		var errorMsg = _testEnv.ProcessEnv.ReadAnsiString(returnValue);
		Assert.Equal("Error 999", errorMsg);
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
