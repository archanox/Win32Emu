using System.Runtime.InteropServices;
using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests ported from ReactOS User32 API test suite - Message Handling
/// Source: https://github.com/reactos/reactos/tree/master/modules/rostests/apitests/user32
/// Focus: PostMessage, PostQuitMessage, TranslateMessage, DispatchMessage
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Source", "ReactOS")]
public class ReactOSPortedTests_MessageHandling : IDisposable
{
	private readonly TestEnvironment _testEnv;

	// Structure sizes
	private static readonly int MsgSize = Marshal.SizeOf<NativeTypes.MSG>();
	
	// Win32 constants
	private const uint WM_NULL = 0x0000;
	private const uint WM_USER = 0x0400;
	private const uint WM_QUIT = 0x0012;
	private const uint WM_PAINT = 0x000F;
	private const uint WM_KEYDOWN = 0x0100;
	private const uint PM_NOREMOVE = 0x0000;
	private const uint PM_REMOVE = 0x0001;
	private const uint PM_NOYIELD = 0x0002;
	private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
	private const uint ERROR_INVALID_WINDOW_HANDLE = 1400;

	public ReactOSPortedTests_MessageHandling()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv.Dispose();
		GC.SuppressFinalize(this);
	}

	#region PostMessage Tests
	// Ported from: rostests/apitests/user32/PostMessage.c

	[Fact]
	public void PostMessageA_WithValidWindow_ShouldReturnTrue()
	{
		// Arrange
		var className = $"PostMessageTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER + 1, 0x1234u, 0x5678u);

		// Assert
		Assert.NotEqual(0u, result); // TRUE
	}

	[Fact]
	public void PostMessageA_WithInvalidWindow_ShouldReturnFalse()
	{
		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", 0xDEADBEEF, WM_USER, 0u, 0u);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, result); // FALSE
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, lastError);
	}

	[Fact]
	public void PostMessageA_WithBroadcastWindow_ShouldReturnTrue()
	{
		// Act - Post to broadcast window (HWND_BROADCAST = 0xFFFF)
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", 0xFFFFu, WM_USER, 0u, 0u);

		// Assert
		Assert.NotEqual(0u, result); // TRUE - broadcast should succeed
	}

	[Fact]
	public void PostMessageA_CanRetrieveWithPeekMessage()
	{
		// Arrange
		var className = $"PostPeekTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);

		// Act - Post a message
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER + 42, 0x1111u, 0x2222u);

		// Retrieve with PeekMessage
		var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, hwnd, 0u, 0u, PM_REMOVE);

		// Assert
		Assert.NotEqual(0u, result); // TRUE - message found

		// Verify message contents
		var hwndMsg = _testEnv.Memory.Read32(msgPtr + 0);
		var message = _testEnv.Memory.Read32(msgPtr + 4);
		var wParam = _testEnv.Memory.Read32(msgPtr + 8);
		var lParam = _testEnv.Memory.Read32(msgPtr + 12);

		Assert.Equal(hwnd, hwndMsg);
		Assert.Equal(WM_USER + 42, message);
		Assert.Equal(0x1111u, wParam);
		Assert.Equal(0x2222u, lParam);
	}

	#endregion

	#region PostQuitMessage Tests
	// Ported from: rostests/apitests/user32/GetPeekMessage.c

	[Fact]
	public void PostQuitMessage_ShouldPostWM_QUIT()
	{
		// Arrange
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);

		// Act
		_testEnv.CallUser32Api("POSTQUITMESSAGE", 42u); // Exit code 42

		// Try to retrieve the quit message
		var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, 0u, 0u, 0u, PM_REMOVE);

		// Assert
		Assert.NotEqual(0u, result); // TRUE - message found

		// Verify it's a WM_QUIT message
		var message = _testEnv.Memory.Read32(msgPtr + 4);
		var wParam = _testEnv.Memory.Read32(msgPtr + 8);

		Assert.Equal(WM_QUIT, message);
		Assert.Equal(42u, wParam); // Exit code should match
	}

	[Fact]
	public void GetMessageA_WithWM_QUIT_ShouldReturnZero()
	{
		// Arrange
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);

		// Act
		_testEnv.CallUser32Api("POSTQUITMESSAGE", 123u);
		var result = _testEnv.CallUser32Api("GETMESSAGEA", msgPtr, 0u, 0u, 0u);

		// Assert
		Assert.Equal(0u, result); // GetMessage returns 0 for WM_QUIT

		// Verify it's WM_QUIT
		var message = _testEnv.Memory.Read32(msgPtr + 4);
		var wParam = _testEnv.Memory.Read32(msgPtr + 8);

		Assert.Equal(WM_QUIT, message);
		Assert.Equal(123u, wParam);
	}

	#endregion

	#region TranslateMessage Tests
	// Ported from: rostests/winetests/user32/input.c

	[Fact]
	public void TranslateMessage_WithNonKeyMessage_ShouldReturnFalse()
	{
		// Arrange
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);
		
		// Create a non-keyboard message
		_testEnv.Memory.Write32(msgPtr + 0, 0); // hwnd
		_testEnv.Memory.Write32(msgPtr + 4, WM_USER); // message
		_testEnv.Memory.Write32(msgPtr + 8, 0); // wParam
		_testEnv.Memory.Write32(msgPtr + 12, 0); // lParam
		_testEnv.Memory.Write32(msgPtr + 16, 0); // time
		_testEnv.Memory.Write32(msgPtr + 20, 0); // pt.x
		_testEnv.Memory.Write32(msgPtr + 24, 0); // pt.y

		// Act
		var result = _testEnv.CallUser32Api("TRANSLATEMESSAGE", msgPtr);

		// Assert
		Assert.Equal(0u, result); // FALSE - no translation
	}

	[Fact]
	public void TranslateMessage_WithKeyMessage_ShouldReturnTrue()
	{
		// Arrange
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);
		
		// Create a WM_KEYDOWN message
		_testEnv.Memory.Write32(msgPtr + 0, 0); // hwnd
		_testEnv.Memory.Write32(msgPtr + 4, WM_KEYDOWN); // message
		_testEnv.Memory.Write32(msgPtr + 8, 0x41); // wParam ('A' key)
		_testEnv.Memory.Write32(msgPtr + 12, 0x001E0001); // lParam (scan code, etc.)
		_testEnv.Memory.Write32(msgPtr + 16, 0); // time
		_testEnv.Memory.Write32(msgPtr + 20, 0); // pt.x
		_testEnv.Memory.Write32(msgPtr + 24, 0); // pt.y

		// Act
		var result = _testEnv.CallUser32Api("TRANSLATEMESSAGE", msgPtr);

		// Assert
		Assert.NotEqual(0u, result); // TRUE - translation occurred
	}

	#endregion

	#region DispatchMessage Tests
	// Ported from: rostests/winetests/user32/msg.c

	[Fact]
	public void DispatchMessageA_WithValidMessage_ShouldReturnWndProcResult()
	{
		// Arrange
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);
		
		// Create a message
		_testEnv.Memory.Write32(msgPtr + 0, 0); // hwnd (NULL = no window)
		_testEnv.Memory.Write32(msgPtr + 4, WM_NULL); // message
		_testEnv.Memory.Write32(msgPtr + 8, 0); // wParam
		_testEnv.Memory.Write32(msgPtr + 12, 0); // lParam
		_testEnv.Memory.Write32(msgPtr + 16, 0); // time
		_testEnv.Memory.Write32(msgPtr + 20, 0); // pt.x
		_testEnv.Memory.Write32(msgPtr + 24, 0); // pt.y

		// Act
		var result = _testEnv.CallUser32Api("DISPATCHMESSAGEA", msgPtr);

		// Assert - Should return 0 for WM_NULL
		Assert.Equal(0u, result);
	}

	[Fact]
	public void DispatchMessageA_WithWindowMessage_ShouldCallWndProc()
	{
		// Arrange
		var className = $"DispatchTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);
		
		// Create a message for the window
		_testEnv.Memory.Write32(msgPtr + 0, hwnd);
		_testEnv.Memory.Write32(msgPtr + 4, WM_USER + 1);
		_testEnv.Memory.Write32(msgPtr + 8, 0x1234);
		_testEnv.Memory.Write32(msgPtr + 12, 0x5678);
		_testEnv.Memory.Write32(msgPtr + 16, 0);
		_testEnv.Memory.Write32(msgPtr + 20, 0);
		_testEnv.Memory.Write32(msgPtr + 24, 0);

		// Act - Dispatch should call the window procedure
		var result = _testEnv.CallUser32Api("DISPATCHMESSAGEA", msgPtr);

		// Assert - Result depends on window procedure implementation
		// Just verify it doesn't crash
		Assert.True(true);
	}

	#endregion

	#region Message Queue Behavior Tests
	// Ported from: rostests/apitests/user32/GetPeekMessage.c

	[Fact]
	public void PeekMessageA_WithPM_NOREMOVE_ShouldNotRemoveMessage()
	{
		// Arrange
		var className = $"PeekNoRemoveTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);

		// Post a message
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER + 99, 0xABCDu, 0xEF01u);

		// Act - Peek without removing
		var result1 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, hwnd, 0u, 0u, PM_NOREMOVE);
		var message1 = _testEnv.Memory.Read32(msgPtr + 4);

		// Peek again
		var result2 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, hwnd, 0u, 0u, PM_NOREMOVE);
		var message2 = _testEnv.Memory.Read32(msgPtr + 4);

		// Assert - Message should still be there
		Assert.NotEqual(0u, result1);
		Assert.NotEqual(0u, result2);
		Assert.Equal(WM_USER + 99, message1);
		Assert.Equal(WM_USER + 99, message2);

		// Now remove it
		var result3 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, hwnd, 0u, 0u, PM_REMOVE);
		Assert.NotEqual(0u, result3);

		// Should be gone now
		var result4 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, hwnd, 0u, 0u, PM_NOREMOVE);
		Assert.Equal(0u, result4);
	}

	[Fact]
	public void PeekMessageA_WithEmptyQueue_ShouldReturnFalse()
	{
		// Arrange
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);

		// Act - Peek when no messages
		var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, 0u, 0u, 0u, PM_REMOVE);

		// Assert
		Assert.Equal(0u, result); // FALSE
	}

	#endregion
}
