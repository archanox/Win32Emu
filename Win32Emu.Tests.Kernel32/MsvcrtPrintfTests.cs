using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32.Modules;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Loader;
using System.Text;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for MSVCRT printf family functions
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class MsvcrtPrintfTests : IDisposable
{
	private readonly TestEnvironmentWithStdout _testEnv;
	private readonly MsvcrtModule _msvcrt;

	public MsvcrtPrintfTests()
	{
		_testEnv = new TestEnvironmentWithStdout();
		_msvcrt = new MsvcrtModule(_testEnv.ProcessEnv, 0x00400000, _testEnv.PeLoader, NullLogger.Instance);
		_testEnv.Dispatcher.RegisterModule(_msvcrt);
	}

	[Fact]
	public void Printf_WithSimpleString_ShouldOutputToStdout()
	{
		// Arrange - simple string with no format specifiers
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Hello, World!\0");

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, 0u);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Hello, World!", _testEnv.StdOutput);
		Assert.Equal(13u, returnValue); // Length of output string
	}

	[Fact]
	public void Printf_WithStringFormatSpecifier_ShouldFormatCorrectly()
	{
		// Arrange - format string with %s
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Name: %s\0");
		var namePtr = _testEnv.ProcessEnv.WriteAnsiString("Alice\0");
		
		// Build va_list manually at a known memory location
		var vaListPtr = 0x10000000u;
		_testEnv.Memory.Write32(vaListPtr, namePtr);

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, vaListPtr);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Name: Alice", _testEnv.StdOutput);
	}

	[Fact]
	public void Printf_WithIntegerFormatSpecifier_ShouldFormatCorrectly()
	{
		// Arrange - format string with %d
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Count: %d\0");
		
		// Build va_list with integer value
		var vaListPtr = 0x10000000u;
		_testEnv.Memory.Write32(vaListPtr, 42);

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, vaListPtr);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Count: 42", _testEnv.StdOutput);
	}

	[Fact]
	public void Printf_WithMultipleFormatSpecifiers_ShouldFormatCorrectly()
	{
		// Arrange - format string with multiple specifiers
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("%s: %d items\0");
		var namePtr = _testEnv.ProcessEnv.WriteAnsiString("Cart\0");
		
		// Build va_list with string pointer and integer
		var vaListPtr = 0x10000000u;
		_testEnv.Memory.Write32(vaListPtr, namePtr);
		_testEnv.Memory.Write32(vaListPtr + 4, 5);

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, vaListPtr);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Cart: 5 items", _testEnv.StdOutput);
	}

	[Fact]
	public void Printf_WithHexFormatSpecifier_ShouldFormatCorrectly()
	{
		// Arrange - format string with %x
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Address: 0x%x\0");
		
		// Build va_list with hex value
		var vaListPtr = 0x10000000u;
		_testEnv.Memory.Write32(vaListPtr, 0xDEADBEEF);

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, vaListPtr);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Address: 0xdeadbeef", _testEnv.StdOutput);
	}

	[Fact]
	public void Printf_WithNullStringPointer_ShouldOutputNull()
	{
		// Arrange - format string with %s and NULL pointer
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Value: %s\0");
		
		// Build va_list with NULL pointer
		var vaListPtr = 0x10000000u;
		_testEnv.Memory.Write32(vaListPtr, 0);

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, vaListPtr);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Value: (null)", _testEnv.StdOutput);
	}

	[Fact]
	public void Printf_WithPercentPercent_ShouldOutputLiteralPercent()
	{
		// Arrange - format string with %%
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Progress: 50%%\0");

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, 0u);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Progress: 50%", _testEnv.StdOutput);
	}

	[Fact]
	public void Printf_WithTrailingPercent_ShouldHandleGracefully()
	{
		// Arrange - format string ending with %
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Value: %\0");

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, 0u);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Value: %", _testEnv.StdOutput);
	}

	[Fact]
	public void Vfprintf_WithSimpleString_ShouldOutputToStdout()
	{
		// Arrange
		var streamPtr = 0x1u; // Stub stream pointer (ignored in implementation)
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Test message\0");
		var vaListPtr = 0u; // No arguments needed

		// Act - call vfprintf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, streamPtr, formatPtr, vaListPtr);
		var success = _msvcrt.TryInvokeUnsafe("vfprintf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "vfprintf should be implemented");
		Assert.Equal("Test message", _testEnv.StdOutput);
		Assert.Equal(12u, returnValue);
	}

	[Fact]
	public void Vfprintf_WithFormatSpecifiers_ShouldFormatCorrectly()
	{
		// Arrange
		var streamPtr = 0x1u;
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Result: %d\0");
		
		// Build va_list
		var vaListPtr = 0x10000000u;
		_testEnv.Memory.Write32(vaListPtr, 123);

		// Act - call vfprintf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, streamPtr, formatPtr, vaListPtr);
		var success = _msvcrt.TryInvokeUnsafe("vfprintf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "vfprintf should be implemented");
		Assert.Equal("Result: 123", _testEnv.StdOutput);
	}

	[Fact]
	public void Printf_WithCharFormatSpecifier_ShouldFormatCorrectly()
	{
		// Arrange - format string with %c
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Letter: %c\0");
		
		// Build va_list with character value (stored as 32-bit int)
		var vaListPtr = 0x10000000u;
		_testEnv.Memory.Write32(vaListPtr, (uint)'X');

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, vaListPtr);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Letter: X", _testEnv.StdOutput);
	}

	[Fact]
	public void Printf_WithUnsignedFormatSpecifier_ShouldFormatCorrectly()
	{
		// Arrange - format string with %u
		var formatPtr = _testEnv.ProcessEnv.WriteAnsiString("Value: %u\0");
		
		// Build va_list with unsigned value
		var vaListPtr = 0x10000000u;
		_testEnv.Memory.Write32(vaListPtr, 4294967295u); // Max uint32

		// Act - call printf
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, formatPtr, vaListPtr);
		var success = _msvcrt.TryInvokeUnsafe("printf", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "printf should be implemented");
		Assert.Equal("Value: 4294967295", _testEnv.StdOutput);
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}

/// <summary>
/// Test environment with stdout capturing capability
/// </summary>
internal class TestEnvironmentWithStdout : IDisposable
{
	private readonly StringBuilder _stdoutCapture = new StringBuilder();
	private readonly TestHost _host;

	public VirtualMemory Memory { get; }
	public MockCpu Cpu { get; }
	public ProcessEnvironment ProcessEnv { get; }
	public PeImageLoader PeLoader { get; }
	public Win32Dispatcher Dispatcher { get; }
	public string StdOutput => _stdoutCapture.ToString();

	public TestEnvironmentWithStdout()
	{
		Memory = new VirtualMemory();
		Cpu = new MockCpu();
		_host = new TestHost(_stdoutCapture);
		ProcessEnv = new ProcessEnvironment(Memory, host: _host, logger: NullLogger.Instance);
		PeLoader = new PeImageLoader(Memory, NullLogger.Instance);
		
		// Initialize main thread
		ProcessEnv.InitializeMainThread(Cpu);
		
		// Create dispatcher
		Dispatcher = new Win32Dispatcher(NullLogger.Instance);
		
		// Initialize process environment
		ProcessEnv.InitializeStrings("test.exe", []);
	}

	public void Dispose()
	{
		// Cleanup if needed
	}

	/// <summary>
	/// Simple test host that captures stdout
	/// </summary>
	private class TestHost : IEmulatorHost
	{
		private readonly StringBuilder _stdout;

		public TestHost(StringBuilder stdout)
		{
			_stdout = stdout;
		}

		public void OnDebugOutput(string message, DebugLevel level)
		{
			// Ignore debug output in tests
		}

		public void OnStdOutput(string output)
		{
			_stdout.Append(output);
		}

		public void OnWindowCreate(WindowCreateInfo info)
		{
		}

		public Task<int> OnDialogCreate(DialogCreateInfo info)
		{
			return Task.FromResult(0);
		}

		public void OnDialogEnd(uint dialogHandle, int result)
		{
		}

		public int OnMessageBox(MessageBoxInfo info)
		{
			return 1; // IDOK
		}

		public void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text)
		{
		}

		public void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData)
		{
		}

		public void OnDialogControlEnabledChanged(uint dialogHandle, int controlId, bool enabled)
		{
		}

		public void OnDisplayUpdate(DisplayUpdateInfo info)
		{
		}

		public Task<string?> OnBrowseForFolder(string? title, string? rootPath)
		{
			return Task.FromResult<string?>(null);
		}

		public Task<string?> OnOpenFileDialog(string? title, string? filter, string? initialDirectory)
		{
			return Task.FromResult<string?>(null);
		}

		public Task<string?> OnSaveFileDialog(string? title, string? filter, string? initialDirectory)
		{
			return Task.FromResult<string?>(null);
		}

		public void OnWindowTitleChanged(uint windowHandle, string title)
		{
		}

		public void OnControlVisibilityChanged(uint dialogHandle, int controlId, bool visible)
		{
		}
	}
}
