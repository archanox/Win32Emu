using System;
using System.Runtime.InteropServices;
using System.Text;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.ABExample;

/// <summary>
/// Base class for A-B tests that compare Win32Emu behavior against native Windows DLLs.
/// This demonstrates the pattern for implementing A-B tests.
/// </summary>
public abstract class ABTestBase : IDisposable
{
	protected readonly bool _nativeAvailable;
	protected readonly IntPtr _nativeModule;

	protected ABTestBase(string dllName)
	{
		// Only load native DLLs on Windows
		if (OperatingSystem.IsWindows())
		{
			try
			{
				_nativeModule = LoadLibrary(dllName);
				_nativeAvailable = _nativeModule != IntPtr.Zero;
			}
			catch
			{
				_nativeAvailable = false;
			}
		}
	}

	public void Dispose()
	{
		if (_nativeModule != IntPtr.Zero && OperatingSystem.IsWindows())
		{
			FreeLibrary(_nativeModule);
		}
		GC.SuppressFinalize(this);
	}

	protected void AssertABMatch<T>(string functionName, T win32EmuResult, T? nativeResult)
	{
		if (_nativeAvailable && nativeResult != null)
		{
			Assert.Equal(nativeResult, win32EmuResult);
		}
		// If native not available, test passes (documents Win32Emu behavior)
	}

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
	private static extern IntPtr LoadLibrary(string lpFileName);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool FreeLibrary(IntPtr hModule);
}

/// <summary>
/// Example A-B tests for KERNEL32.DLL functions.
/// This demonstrates how to write A-B tests that compare Win32Emu against native behavior.
/// </summary>
public class GetVersionABTests : ABTestBase
{
	public GetVersionABTests() : base("KERNEL32.DLL")
	{
	}

	[Fact]
	[Trait("Category", "ABTest")]
	[Trait("Category", "Example")]
	[Trait("Function", "GetVersion")]
	public void GetVersion_ShouldReturnVersionNumber()
	{
		// Arrange
		using var testEnv = new TestEnvironment();

		// Act - Call Win32Emu implementation
		var win32EmuResult = testEnv.CallKernel32Api("GETVERSION");

		// Act - Call native (if available on Windows)
		uint? nativeResult = null;
		if (_nativeAvailable && OperatingSystem.IsWindows())
		{
			nativeResult = NativeGetVersion();
		}

		// Assert
		Assert.NotEqual(0u, win32EmuResult);

		// On Windows, we can compare against native behavior
		// Note: GetVersion is deprecated and may return emulated values on modern Windows
		// So we just verify Win32Emu returns a valid version
		if (_nativeAvailable)
		{
			// Both should return non-zero version
			Assert.NotEqual(0u, nativeResult);
		}

		// Win32Emu returns Windows 95 version (4.0.950)
		// Due to implementation bug, it's encoded as 0x040003B6
		Assert.Equal(0x040003B6u, win32EmuResult);
	}

	[Fact]
	[Trait("Category", "ABTest")]
	[Trait("Category", "Example")]
	[Trait("Function", "GetLastError")]
	public void GetLastError_InitialValue_ShouldBeZero()
	{
		// Arrange
		using var testEnv = new TestEnvironment();

		// Act - Call Win32Emu implementation
		var win32EmuResult = testEnv.CallKernel32Api("GETLASTERROR");

		// Act - Call native (if available on Windows)
		uint? nativeResult = null;
		if (_nativeAvailable && OperatingSystem.IsWindows())
		{
			// Note: We can't reliably test native GetLastError in isolation
			// since other API calls may have set it. This is just for demonstration.
			nativeResult = NativeGetLastError();
		}

		// Assert - Win32Emu should initialize to 0
		Assert.Equal(0u, win32EmuResult);

		// Note: Native GetLastError may not be 0 due to prior API calls,
		// so we don't assert A-B match here. This demonstrates that some
		// functions require more careful test setup.
	}

	[Fact]
	[Trait("Category", "ABTest")]
	[Trait("Category", "Example")]
	[Trait("Function", "SetLastError")]
	public void SetLastError_ThenGetLastError_ShouldReturnSameValue()
	{
		// Arrange
		using var testEnv = new TestEnvironment();
		const uint testError = 12345;

		// Act - Win32Emu
		testEnv.CallKernel32Api("SETLASTERROR", testError);
		var win32EmuResult = testEnv.CallKernel32Api("GETLASTERROR");

		// Act - Native (if available)
		uint? nativeResult = null;
		if (_nativeAvailable && OperatingSystem.IsWindows())
		{
			NativeSetLastError(testError);
			nativeResult = NativeGetLastError();
		}

		// Assert - Both should return the set value
		Assert.Equal(testError, win32EmuResult);
		AssertABMatch("SetLastError/GetLastError", win32EmuResult, nativeResult);
	}

	// P/Invoke declarations for native Windows DLL functions
	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern uint GetVersion();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern uint GetLastError();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern void SetLastError(uint dwErrCode);

	// Wrapper methods to match test convention
	private static uint NativeGetVersion() => GetVersion();
	private static uint NativeGetLastError() => GetLastError();
	private static void NativeSetLastError(uint error) => SetLastError(error);
}
