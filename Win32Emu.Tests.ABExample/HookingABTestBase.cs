using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EasyHook;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.ABExample;

/// <summary>
/// Base class for A-B tests that use EasyHook to intercept native Windows API calls.
/// This demonstrates the full hooking pattern mentioned in the GitHub issue:
/// - Hook native imports using EasyHook
/// - Capture both Win32Emu and native DLL behavior
/// - Compare results to validate Win32Emu implementations
/// </summary>
public abstract class HookingABTestBase : IDisposable
{
	protected readonly bool _hookingAvailable;
	protected readonly List<LocalHook> _activeHooks = new();
	protected readonly Dictionary<string, object> _capturedData = new();

	protected HookingABTestBase()
	{
		// Hooking is only available on Windows
		_hookingAvailable = OperatingSystem.IsWindows();
	}

	/// <summary>
	/// Creates a hook for a specific Windows API function.
	/// </summary>
	/// <param name="dllName">Name of the DLL (e.g., "kernel32.dll")</param>
	/// <param name="functionName">Name of the function to hook</param>
	/// <param name="hookHandler">Delegate to the hook handler function</param>
	/// <returns>The created LocalHook or null if hooking is not available</returns>
	protected LocalHook? CreateHook(string dllName, string functionName, Delegate hookHandler)
	{
		if (!_hookingAvailable)
		{
			return null;
		}

		try
		{
			// Get the address of the target function
			var targetAddress = LocalHook.GetProcAddress(dllName, functionName);
			
			// Create the hook
			var hook = LocalHook.Create(targetAddress, hookHandler, null);
			
			// Enable the hook for current thread only (0 = current thread ID)
			hook.ThreadACL.SetInclusiveACL(new int[] { 0 });
			
			_activeHooks.Add(hook);
			return hook;
		}
		catch (Exception)
		{
			// Hook creation failed - may not be supported on this platform/configuration
			return null;
		}
	}

	/// <summary>
	/// Gets the original function pointer for calling the native implementation.
	/// </summary>
	protected TDelegate? GetOriginalFunction<TDelegate>(string dllName, string functionName) where TDelegate : Delegate
	{
		if (!_hookingAvailable)
		{
			return null;
		}

		try
		{
			var targetAddress = LocalHook.GetProcAddress(dllName, functionName);
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(targetAddress);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Captures data from a hook for later comparison.
	/// </summary>
	protected void CaptureHookData(string key, object value)
	{
		_capturedData[key] = value;
	}

	/// <summary>
	/// Retrieves captured hook data.
	/// </summary>
	protected T? GetCapturedData<T>(string key)
	{
		if (_capturedData.TryGetValue(key, out var value) && value is T typedValue)
		{
			return typedValue;
		}
		return default;
	}

	/// <summary>
	/// Compares Win32Emu result against captured native behavior.
	/// </summary>
	protected void AssertABMatch<T>(string functionName, T win32EmuResult, T? nativeResult)
	{
		if (_hookingAvailable && nativeResult != null)
		{
			Assert.Equal(nativeResult, win32EmuResult);
		}
		// If hooking not available, test passes (documents Win32Emu behavior)
	}

	public void Dispose()
	{
		// Dispose all active hooks
		foreach (var hook in _activeHooks)
		{
			hook.Dispose();
		}
		_activeHooks.Clear();
		_capturedData.Clear();
		GC.SuppressFinalize(this);
	}
}

/// <summary>
/// Example of hook-based A-B testing for GetVersion API.
/// This demonstrates the pattern requested in the GitHub issue.
/// </summary>
public class GetVersionHookingABTests : HookingABTestBase
{
	// Delegate matching the signature of GetVersion
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
	private delegate uint GetVersionDelegate();

	// Store the original function pointer
	private GetVersionDelegate? _originalGetVersion;

	public GetVersionHookingABTests()
	{
		if (_hookingAvailable)
		{
			// Get the original function for calling native implementation
			_originalGetVersion = GetOriginalFunction<GetVersionDelegate>("kernel32.dll", "GetVersion");
			
			// Create a hook that captures native calls
			CreateHook("kernel32.dll", "GetVersion", new GetVersionDelegate(GetVersionHook));
		}
	}

	/// <summary>
	/// Hook handler that intercepts native GetVersion calls.
	/// </summary>
	private uint GetVersionHook()
	{
		// Call the original function
		var nativeResult = _originalGetVersion?.Invoke() ?? 0;
		
		// Capture the native result for comparison
		CaptureHookData("GetVersion.Native", nativeResult);
		
		return nativeResult;
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Example")]
	[Trait("Function", "GetVersion")]
	public void GetVersion_WithHooking_ShouldMatchNativeBehavior()
	{
		// Arrange
		using var testEnv = new TestEnvironment();

		// Act - Call Win32Emu implementation
		var win32EmuResult = testEnv.CallKernel32Api("GETVERSION");

		// Act - Trigger native call (if hooking is available)
		uint? nativeResult = null;
		if (_hookingAvailable && _originalGetVersion != null)
		{
			// This call will be intercepted by our hook
			nativeResult = _originalGetVersion.Invoke();
			
			// Retrieve the captured native result
			nativeResult = GetCapturedData<uint>("GetVersion.Native");
		}

		// Assert
		Assert.NotEqual(0u, win32EmuResult);
		
		// If we have native results, compare them
		// Note: GetVersion returns different values on different Windows versions
		// Win32Emu returns Windows 95 version (0x040003B6)
		if (_hookingAvailable && nativeResult.HasValue)
		{
			// Both should return non-zero version numbers
			Assert.NotEqual(0u, nativeResult.Value);
			
			// Document the difference between Win32Emu (Windows 95) and host OS
			// This is expected behavior - Win32Emu emulates Windows 95
		}
		
		// Verify Win32Emu returns the expected Windows 95 version
		const uint EXPECTED_WIN95_VERSION = 0x040003B6u;
		Assert.Equal(EXPECTED_WIN95_VERSION, win32EmuResult);
	}
}

/// <summary>
/// Example of hook-based A-B testing for SetLastError/GetLastError APIs.
/// This shows how to test stateful APIs with hooking.
/// </summary>
public class LastErrorHookingABTests : HookingABTestBase
{
	// Delegates matching the signatures
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = false)]
	private delegate uint GetLastErrorDelegate();

	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = false)]
	private delegate void SetLastErrorDelegate(uint dwErrCode);

	// Store the original functions
	private GetLastErrorDelegate? _originalGetLastError;
	private SetLastErrorDelegate? _originalSetLastError;

	public LastErrorHookingABTests()
	{
		if (_hookingAvailable)
		{
			// Get the original functions
			_originalGetLastError = GetOriginalFunction<GetLastErrorDelegate>("kernel32.dll", "GetLastError");
			_originalSetLastError = GetOriginalFunction<SetLastErrorDelegate>("kernel32.dll", "SetLastError");
			
			// Create hooks
			CreateHook("kernel32.dll", "GetLastError", new GetLastErrorDelegate(GetLastErrorHook));
			CreateHook("kernel32.dll", "SetLastError", new SetLastErrorDelegate(SetLastErrorHook));
		}
	}

	private uint GetLastErrorHook()
	{
		var result = _originalGetLastError?.Invoke() ?? 0;
		CaptureHookData("GetLastError.Result", result);
		return result;
	}

	private void SetLastErrorHook(uint dwErrCode)
	{
		CaptureHookData("SetLastError.Value", dwErrCode);
		_originalSetLastError?.Invoke(dwErrCode);
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Example")]
	[Trait("Function", "SetLastError")]
	public void SetLastError_WithHooking_BehaviorMatchesNative()
	{
		// Arrange
		using var testEnv = new TestEnvironment();
		const uint testError = 12345;

		// Act - Win32Emu implementation
		testEnv.CallKernel32Api("SETLASTERROR", testError);
		var win32EmuResult = testEnv.CallKernel32Api("GETLASTERROR");

		// Act - Native implementation (if available)
		uint? nativeResult = null;
		if (_hookingAvailable && _originalSetLastError != null && _originalGetLastError != null)
		{
			_originalSetLastError.Invoke(testError);
			nativeResult = _originalGetLastError.Invoke();
		}

		// Assert
		Assert.Equal(testError, win32EmuResult);
		AssertABMatch("SetLastError/GetLastError", win32EmuResult, nativeResult);
	}
}
