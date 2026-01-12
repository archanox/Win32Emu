using Xunit;
using Win32Emu.Tests.Infrastructure;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for message dispatching with WndProc callbacks
/// </summary>
[Trait("Category", "DllModuleTests")]
public class MessageDispatchTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public MessageDispatchTests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void DispatchMessageA_ShouldCallWndProc_ForCustomMessage()
	{
		// Arrange - Register a window class with a WndProc
		const uint wndProcAddress = 0x00401000;
		var wndClassAddr = _testEnv.WriteWndClassA(
			className: "TestMessageClass",
			wndProc: wndProcAddress
		);
		
		var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);
		Assert.NotEqual(0u, atom); // Verify registration succeeded

		// Create a window with this class
		var classNamePtr = _testEnv.WriteString("TestMessageClass");
		var titlePtr = _testEnv.WriteString("Test Window");
		
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,              // dwExStyle
			classNamePtr,   // lpClassName
			titlePtr,       // lpWindowName
			0,              // dwStyle
			0, 0, 0, 0,     // position and size
			0,              // hWndParent
			0,              // hMenu
			0,              // hInstance
			0               // lpParam
		);
		
		Assert.NotEqual(0u, hwnd); // Verify window creation succeeded

		// Set up a simple WndProc that returns a specific value when called
		// We'll write assembly code that checks the message and returns a value
		const uint WM_CUSTOM_TEST = 0x0464; // WM_USER + 100
		const uint expectedReturnValue = 0x12345678;
		
		// Write a simple WndProc stub that checks for our custom message
		// For simplicity, we'll just return a constant value
		var wndProcCode = new byte[]
		{
			0xB8, 0x78, 0x56, 0x34, 0x12,  // mov eax, 0x12345678
			0xC2, 0x10, 0x00                // ret 16  (stdcall cleanup)
		};
		_testEnv.Memory.WriteBytes(wndProcAddress, wndProcCode);

		// Post a custom message
		const uint wParam = 123;
		const uint lParam = 456;
		var postResult = _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_CUSTOM_TEST, wParam, lParam);
		Assert.Equal(1u, postResult); // TRUE

		// Retrieve the message
		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size
		var peekResult = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, hwnd, 0, 0, 0x0001); // PM_REMOVE
		Assert.Equal(1u, peekResult); // TRUE - message available

		// Verify message contents
		var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
		var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);
		var retrievedLParam = _testEnv.Memory.Read32(msgAddr + 12);
		
		Assert.Equal(WM_CUSTOM_TEST, retrievedMsg);
		Assert.Equal(wParam, retrievedWParam);
		Assert.Equal(lParam, retrievedLParam);

		// Act - Dispatch the message
		var dispatchResult = _testEnv.CallUser32Api("DISPATCHMESSAGEA", msgAddr);

		// Assert - The WndProc should have been called and returned our expected value
		Assert.Equal(expectedReturnValue, dispatchResult);
	}

	[Fact]
	public void PostQuitMessage_ShouldMakeWM_QUIT_AvailableInPeekMessage()
	{
		// Arrange
		const int exitCode = 42;

		// Act - Post quit message
		_testEnv.CallUser32Api("POSTQUITMESSAGE", (uint)exitCode);

		// Assert - PeekMessage should return WM_QUIT
		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size
		var peekResult = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0001); // PM_REMOVE

		Assert.Equal(1u, peekResult); // TRUE - message available

		// Verify WM_QUIT message
		var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
		var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);

		Assert.Equal(0x0012u, retrievedMsg); // WM_QUIT
		Assert.Equal((uint)exitCode, retrievedWParam);
	}

	[Fact]
	public void PeekMessageA_WithEmptyQueue_ShouldReturnFalse()
	{
		// Arrange
		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size

		// Act - Try to peek when queue is empty
		var peekResult = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0001); // PM_REMOVE

		// Assert - Should return FALSE (no message available)
		Assert.Equal(0u, peekResult);
	}

	[Fact]
	public void PostMessageA_ToInvalidWindow_ShouldReturnFalseAndSetLastError()
	{
		// Arrange
		const uint invalidHwnd = 0xDEADBEEF;
		const uint WM_USER = 0x0400;

		// Act
		var postResult = _testEnv.CallUser32Api("POSTMESSAGEA", invalidHwnd, WM_USER, 0, 0);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, postResult); // FALSE
		Assert.Equal(1400u, lastError); // ERROR_INVALID_WINDOW_HANDLE
	}

	[Fact]
	public void PostMessageA_ToNullWindow_ShouldSucceed()
	{
		// Arrange
		const uint nullHwnd = 0;
		const uint WM_USER = 0x0400;

		// Act - Post to null window (broadcast to all windows in thread)
		var postResult = _testEnv.CallUser32Api("POSTMESSAGEA", nullHwnd, WM_USER, 0, 0);

		// Assert - Should succeed
		Assert.Equal(1u, postResult); // TRUE
	}

	public void Dispose()
	{
		_testEnv?.Dispose();
	}
}
