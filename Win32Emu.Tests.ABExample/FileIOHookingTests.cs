using System;
using System.Runtime.InteropServices;
using System.Text;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.ABExample;

/// <summary>
/// Advanced hook-based A-B testing for file I/O operations.
/// Demonstrates hooking CreateFileA and comparing behavior between Win32Emu and native Windows.
/// This is a comprehensive example of the pattern requested in the GitHub issue.
/// </summary>
public class FileIOHookingTests : HookingABTestBase
{
	// Use shared constants
	private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(Win32Constants.INVALID_HANDLE_VALUE);

	// Delegate matching CreateFileA signature
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Ansi)]
	private delegate IntPtr CreateFileADelegate(
		string lpFileName,
		uint dwDesiredAccess,
		uint dwShareMode,
		IntPtr lpSecurityAttributes,
		uint dwCreationDisposition,
		uint dwFlagsAndAttributes,
		IntPtr hTemplateFile
	);

	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
	private delegate bool CloseHandleDelegate(IntPtr hObject);

	// Store original functions
	private readonly CreateFileADelegate? _originalCreateFileA;
	private readonly CloseHandleDelegate? _originalCloseHandle;

	// Track hook calls
	private int _createFileCallCount;
	private string? _lastFileName;

	public FileIOHookingTests()
	{
		if (_hookingAvailable)
		{
			// Get original functions
			_originalCreateFileA = GetOriginalFunction<CreateFileADelegate>("kernel32.dll", "CreateFileA");
			_originalCloseHandle = GetOriginalFunction<CloseHandleDelegate>("kernel32.dll", "CloseHandle");

			// Create hooks
			CreateHook("kernel32.dll", "CreateFileA", new CreateFileADelegate(CreateFileAHook));
			CreateHook("kernel32.dll", "CloseHandle", new CloseHandleDelegate(CloseHandleHook));
		}
	}

	/// <summary>
	/// Hook handler for CreateFileA that captures call details.
	/// </summary>
	private IntPtr CreateFileAHook(
		string lpFileName,
		uint dwDesiredAccess,
		uint dwShareMode,
		IntPtr lpSecurityAttributes,
		uint dwCreationDisposition,
		uint dwFlagsAndAttributes,
		IntPtr hTemplateFile)
	{
		// Track the call
		_createFileCallCount++;
		_lastFileName = lpFileName;

		// Capture parameters
		CaptureHookData("CreateFileA.FileName", lpFileName);
		CaptureHookData("CreateFileA.DesiredAccess", dwDesiredAccess);
		CaptureHookData("CreateFileA.CreationDisposition", dwCreationDisposition);

		// Call the original function
		var result = _originalCreateFileA?.Invoke(
			lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
			dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile
		) ?? INVALID_HANDLE_VALUE;

		// Capture the result
		CaptureHookData("CreateFileA.Result", result);
		CaptureHookData("CreateFileA.IsValid", result != INVALID_HANDLE_VALUE);

		return result;
	}

	/// <summary>
	/// Hook handler for CloseHandle.
	/// </summary>
	private bool CloseHandleHook(IntPtr hObject)
	{
		var result = _originalCloseHandle?.Invoke(hObject) ?? false;
		CaptureHookData("CloseHandle.Result", result);
		return result;
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Advanced")]
	[Trait("Function", "CreateFileA")]
	public void CreateFileA_WithHooking_ValidatesHandleCreation()
	{
		// Skip if hooking is not available
		if (!_hookingAvailable)
		{
			return;
		}

		// Arrange
		using var testEnv = new TestEnvironment();
		var testFileName = "test_ab_file.txt";
		var fileNamePtr = testEnv.WriteString(testFileName);

		// Act - Win32Emu implementation
		var win32EmuHandle = testEnv.CallKernel32Api(
			"CREATEFILEA",
			fileNamePtr,
			Win32Constants.GENERIC_WRITE,
			0u, // dwShareMode
			0u, // lpSecurityAttributes (NULL)
			Win32Constants.CREATE_ALWAYS,
			Win32Constants.FILE_ATTRIBUTE_NORMAL,
			0u // hTemplateFile (NULL)
		);

		// Act - Native implementation (triggers hook)
		var tempPath = System.IO.Path.GetTempPath();
		var nativeTestFile = System.IO.Path.Combine(tempPath, testFileName);
		IntPtr? nativeHandle = null;

		if (_originalCreateFileA != null)
		{
			nativeHandle = _originalCreateFileA.Invoke(
				nativeTestFile,
				Win32Constants.GENERIC_WRITE,
				0,
				IntPtr.Zero,
				Win32Constants.CREATE_ALWAYS,
				Win32Constants.FILE_ATTRIBUTE_NORMAL,
				IntPtr.Zero
			);

			// Verify hook was called
			Assert.Equal(1, _createFileCallCount);
			Assert.Equal(nativeTestFile, _lastFileName);
		}

		// Assert - Compare results
		Assert.NotEqual(0u, win32EmuHandle); // Win32Emu should return valid handle

		if (nativeHandle.HasValue)
		{
			var nativeIsValid = nativeHandle.Value != INVALID_HANDLE_VALUE;
			Assert.True(nativeIsValid); // Native should also return valid handle

			// Both should succeed in creating the file
			var capturedIsValid = GetCapturedData<bool>("CreateFileA.IsValid");
			Assert.True(capturedIsValid);

			// Clean up native handle
			_originalCloseHandle?.Invoke(nativeHandle.Value);

			// Clean up temp file
			try
			{
				System.IO.File.Delete(nativeTestFile);
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Advanced")]
	[Trait("Function", "CreateFileA")]
	public void CreateFileA_InvalidFile_BehaviorMatchesNative()
	{
		// Skip if hooking is not available
		if (!_hookingAvailable)
		{
			return;
		}

		// Arrange
		using var testEnv = new TestEnvironment();
		var invalidFileName = "Z:\\invalid\\path\\file.txt"; // Should fail
		var fileNamePtr = testEnv.WriteString(invalidFileName);

		// Act - Win32Emu implementation
		var win32EmuHandle = testEnv.CallKernel32Api(
			"CREATEFILEA",
			fileNamePtr,
			Win32Constants.GENERIC_READ,
			0u,
			0u, // lpSecurityAttributes (NULL)
			Win32Constants.OPEN_EXISTING,
			Win32Constants.FILE_ATTRIBUTE_NORMAL,
			0u // hTemplateFile (NULL)
		);

		// Act - Native implementation (triggers hook)
		IntPtr? nativeHandle = null;
		if (_originalCreateFileA != null)
		{
			nativeHandle = _originalCreateFileA.Invoke(
				invalidFileName,
				Win32Constants.GENERIC_READ,
				0,
				IntPtr.Zero,
				Win32Constants.OPEN_EXISTING,
				Win32Constants.FILE_ATTRIBUTE_NORMAL,
				IntPtr.Zero
			);
		}

		// Assert - Both should fail (return INVALID_HANDLE_VALUE)
		// Win32Emu returns 0 for invalid handles
		Assert.Equal(0u, win32EmuHandle);

		if (nativeHandle.HasValue)
		{
			// Native returns INVALID_HANDLE_VALUE (-1)
			Assert.Equal(INVALID_HANDLE_VALUE, nativeHandle.Value);

			// Both indicate failure (different representation)
			var capturedIsValid = GetCapturedData<bool>("CreateFileA.IsValid");
			Assert.False(capturedIsValid);
		}
	}
}

/// <summary>
/// Demonstrates hooking GetTempPathA for path comparison.
/// </summary>
public class TempPathHookingTests : HookingABTestBase
{
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Ansi)]
	private delegate uint GetTempPathADelegate(uint nBufferLength, byte[] lpBuffer);

	private readonly GetTempPathADelegate? _originalGetTempPathA;

	public TempPathHookingTests()
	{
		if (_hookingAvailable)
		{
			_originalGetTempPathA = GetOriginalFunction<GetTempPathADelegate>("kernel32.dll", "GetTempPathA");
			CreateHook("kernel32.dll", "GetTempPathA", new GetTempPathADelegate(GetTempPathAHook));
		}
	}

	private uint GetTempPathAHook(uint nBufferLength, byte[] lpBuffer)
	{
		var result = _originalGetTempPathA?.Invoke(nBufferLength, lpBuffer) ?? 0;

		if (result > 0 && result <= nBufferLength)
		{
			var path = Encoding.ASCII.GetString(lpBuffer, 0, (int)result - 1); // -1 to exclude null terminator
			CaptureHookData("GetTempPathA.Path", path);
			CaptureHookData("GetTempPathA.Length", result);
		}

		return result;
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Example")]
	[Trait("Function", "GetTempPathA")]
	public void GetTempPathA_WithHooking_ReturnsValidPath()
	{
		// Skip if hooking is not available
		if (!_hookingAvailable)
		{
			return;
		}

		// Arrange
		using var testEnv = new TestEnvironment();
		const uint bufferSize = 260; // MAX_PATH
		var bufferPtr = testEnv.AllocateMemory((int)bufferSize);

		// Act - Win32Emu implementation
		var win32EmuResult = testEnv.CallKernel32Api("GETTEMPPATHA", bufferSize, bufferPtr);
		var win32EmuPath = testEnv.ReadString(bufferPtr);

		// Act - Native implementation (triggers hook)
		string? nativePath = null;
		uint? nativeLength = null;

		if (_originalGetTempPathA != null)
		{
			var nativeBuffer = new byte[bufferSize];
			nativeLength = _originalGetTempPathA.Invoke(bufferSize, nativeBuffer);
			nativePath = GetCapturedData<string>("GetTempPathA.Path");
		}

		// Assert
		Assert.True(win32EmuResult > 0, "Win32Emu should return a valid length");
		Assert.NotEmpty(win32EmuPath);

		if (nativePath != null && nativeLength.HasValue)
		{
			// Both should return valid paths
			Assert.True(nativeLength.Value > 0);
			Assert.NotEmpty(nativePath);

			// Paths may differ between Win32Emu and native Windows
			// Win32Emu may use a different temp directory
			// Just verify both are valid
		}
	}
}
