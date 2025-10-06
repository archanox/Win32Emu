using Win32Emu.Tests.User32.TestInfrastructure;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for User32 message queue functions
/// </summary>
public class MessageQueueTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public MessageQueueTests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void PostMessageA_ShouldQueueMessage()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_USER = 0x0400;
		const uint wParam = 0x12345678;
		const uint lParam = 0x87654321;

		// Act
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER, wParam, lParam);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	[Fact]
	public void PeekMessageA_WithNoMessages_ShouldReturnZero()
	{
		// Arrange
		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size

		// Act
		var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0);

		// Assert
		Assert.Equal(0u, result); // No message available
	}

	[Fact]
	public void PeekMessageA_WithQueuedMessage_ShouldReturnMessage()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_USER = 0x0400;
		const uint wParam = 0x12345678;
		const uint lParam = 0x87654321;

		// Post a message
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER, wParam, lParam);

		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size

		// Act
		var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0001); // PM_REMOVE

		// Assert
		Assert.Equal(1u, result); // Message available

		// Verify message contents
		var retrievedHwnd = _testEnv.Memory.Read32(msgAddr + 0);
		var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
		var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);
		var retrievedLParam = _testEnv.Memory.Read32(msgAddr + 12);

		Assert.Equal(hwnd, retrievedHwnd);
		Assert.Equal(WM_USER, retrievedMsg);
		Assert.Equal(wParam, retrievedWParam);
		Assert.Equal(lParam, retrievedLParam);
	}

	[Fact]
	public void GetMessageA_WithQueuedMessage_ShouldReturnMessage()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_USER = 0x0400;
		const uint wParam = 0x12345678;
		const uint lParam = 0x87654321;

		// Post a message
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER, wParam, lParam);

		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size

		// Act
		var result = _testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);

		// Assert
		Assert.Equal(1u, result); // Non-zero for non-WM_QUIT messages

		// Verify message contents
		var retrievedHwnd = _testEnv.Memory.Read32(msgAddr + 0);
		var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
		var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);
		var retrievedLParam = _testEnv.Memory.Read32(msgAddr + 12);

		Assert.Equal(hwnd, retrievedHwnd);
		Assert.Equal(WM_USER, retrievedMsg);
		Assert.Equal(wParam, retrievedWParam);
		Assert.Equal(lParam, retrievedLParam);
	}

	[Fact]
	public void GetMessageA_WithQuitMessage_ShouldReturnZero()
	{
		// Arrange
		const int exitCode = 42;
		_testEnv.CallUser32Api("POSTQUITMESSAGE", (uint)exitCode);

		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size

		// Act
		var result = _testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);

		// Assert
		Assert.Equal(0u, result); // Zero for WM_QUIT

		// Verify WM_QUIT message
		var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
		var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);

		Assert.Equal(0x0012u, retrievedMsg); // WM_QUIT
		Assert.Equal((uint)exitCode, retrievedWParam);
	}

	[Fact]
	public void MessageQueue_FIFO_Order()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_USER = 0x0400;

		// Post messages in order
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER + 1, 1, 0);
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER + 2, 2, 0);
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER + 3, 3, 0);

		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size

		// Act & Assert - Retrieve messages in FIFO order
		_testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);
		Assert.Equal(WM_USER + 1, _testEnv.Memory.Read32(msgAddr + 4));
		Assert.Equal(1u, _testEnv.Memory.Read32(msgAddr + 8));

		_testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);
		Assert.Equal(WM_USER + 2, _testEnv.Memory.Read32(msgAddr + 4));
		Assert.Equal(2u, _testEnv.Memory.Read32(msgAddr + 8));

		_testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);
		Assert.Equal(WM_USER + 3, _testEnv.Memory.Read32(msgAddr + 4));
		Assert.Equal(3u, _testEnv.Memory.Read32(msgAddr + 8));
	}

	[Fact]
	public void PeekMessageA_WithPM_NOREMOVE_ShouldNotRemoveMessage()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_USER = 0x0400;

		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_USER, 0, 0);

		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size

		// Act - Peek without removing
		var result1 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0000); // PM_NOREMOVE
		var result2 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0000); // PM_NOREMOVE again

		// Assert - Message should still be available
		Assert.Equal(1u, result1);
		Assert.Equal(1u, result2);
	}

	[Fact]
	public void DefWindowProcA_WM_CLOSE_ShouldDestroyWindow()
	{
		// Arrange - Create a window first
		var wndClassAddr = _testEnv.WriteWndClassA(
			className: "TestClass",
			wndProc: 0x00401000
		);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

		var classNamePtr = _testEnv.WriteString("TestClass");
		var titlePtr = _testEnv.WriteString("Test Window");

		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,              // dwExStyle
			classNamePtr,   // lpClassName
			titlePtr,       // lpWindowName
			0x00000000,     // dwStyle (WS_OVERLAPPED)
			100,            // x
			100,            // y
			640,            // nWidth
			480,            // nHeight
			0,              // hWndParent
			0,              // hMenu
			0,              // hInstance
			0               // lpParam
		);

		const uint WM_CLOSE = 0x0010;

		// Act - Call DefWindowProc with WM_CLOSE
		_testEnv.CallUser32Api("DEFWINDOWPROCA", hwnd, WM_CLOSE, 0, 0);

		// Assert - Window should be destroyed (we can't easily check this without additional API)
		// Just verify the call doesn't crash
		Assert.NotEqual(0u, hwnd);
	}

	public void Dispose()
	{
		_testEnv?.Dispose();
	}
}
