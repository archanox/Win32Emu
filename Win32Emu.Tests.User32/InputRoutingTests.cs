using Win32Emu.Tests.User32.TestInfrastructure;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for keyboard and mouse input routing to the Win32 message queue
/// </summary>
public class InputRoutingTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public InputRoutingTests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void PostMessage_WithKeyboardMessage_ShouldQueueCorrectly()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_KEYDOWN = 0x0100;
		const uint virtualKeyCode = 0x41; // 'A' key
		const uint lParam = 0x00000001; // Key data

		// Act
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_KEYDOWN, virtualKeyCode, lParam);

		// Assert
		Assert.Equal(1u, result); // TRUE

		// Verify message was queued
		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size
		var getMessage = _testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);

		Assert.Equal(1u, getMessage); // Non-zero for non-WM_QUIT messages

		var retrievedHwnd = _testEnv.Memory.Read32(msgAddr + 0);
		var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
		var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);
		var retrievedLParam = _testEnv.Memory.Read32(msgAddr + 12);

		Assert.Equal(hwnd, retrievedHwnd);
		Assert.Equal(WM_KEYDOWN, retrievedMsg);
		Assert.Equal(virtualKeyCode, retrievedWParam);
		Assert.Equal(lParam, retrievedLParam);
	}

	[Fact]
	public void PostMessage_WithMouseMessage_ShouldQueueCorrectly()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_LBUTTONDOWN = 0x0201;
		const uint wParam = 0x0001; // MK_LBUTTON
		const uint lParam = 0x00640032; // Position (50, 100) - HIWORD=100, LOWORD=50

		// Act
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_LBUTTONDOWN, wParam, lParam);

		// Assert
		Assert.Equal(1u, result); // TRUE

		// Verify message was queued
		var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size
		var getMessage = _testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);

		Assert.Equal(1u, getMessage); // Non-zero for non-WM_QUIT messages

		var retrievedHwnd = _testEnv.Memory.Read32(msgAddr + 0);
		var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
		var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);
		var retrievedLParam = _testEnv.Memory.Read32(msgAddr + 12);

		Assert.Equal(hwnd, retrievedHwnd);
		Assert.Equal(WM_LBUTTONDOWN, retrievedMsg);
		Assert.Equal(wParam, retrievedWParam);
		Assert.Equal(lParam, retrievedLParam);
	}

	[Fact]
	public void PostMessage_WithMultipleInputMessages_ShouldQueueInOrder()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_KEYDOWN = 0x0100;
		const uint WM_KEYUP = 0x0101;
		const uint virtualKeyA = 0x41; // 'A' key

		// Act - Post key down and key up
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_KEYDOWN, virtualKeyA, 0x00000001);
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_KEYUP, virtualKeyA, 0xC0000001);

		// Assert - Get first message (key down)
		var msgAddr = _testEnv.AllocateMemory(28);
		var result1 = _testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);
		Assert.Equal(1u, result1);

		var msg1 = _testEnv.Memory.Read32(msgAddr + 4);
		var wParam1 = _testEnv.Memory.Read32(msgAddr + 8);
		Assert.Equal(WM_KEYDOWN, msg1);
		Assert.Equal(virtualKeyA, wParam1);

		// Assert - Get second message (key up)
		var result2 = _testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);
		Assert.Equal(1u, result2);

		var msg2 = _testEnv.Memory.Read32(msgAddr + 4);
		var wParam2 = _testEnv.Memory.Read32(msgAddr + 8);
		Assert.Equal(WM_KEYUP, msg2);
		Assert.Equal(virtualKeyA, wParam2);
	}

	[Fact]
	public void PostMessage_WithMouseMoveMessage_ShouldIncludePosition()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_MOUSEMOVE = 0x0200;
		const uint wParam = 0x0000; // No buttons pressed
		const short xPos = 123;
		const short yPos = 456;
		uint lParam = (uint)((yPos << 16) | (xPos & 0xFFFF));

		// Act
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_MOUSEMOVE, wParam, lParam);

		// Assert
		Assert.Equal(1u, result); // TRUE

		// Verify message was queued
		var msgAddr = _testEnv.AllocateMemory(28);
		var getMessage = _testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);

		Assert.Equal(1u, getMessage);

		var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
		var retrievedLParam = _testEnv.Memory.Read32(msgAddr + 12);

		Assert.Equal(WM_MOUSEMOVE, retrievedMsg);
		Assert.Equal(lParam, retrievedLParam);

		// Extract position from lParam
		short retrievedX = (short)(retrievedLParam & 0xFFFF);
		short retrievedY = (short)((retrievedLParam >> 16) & 0xFFFF);

		Assert.Equal(xPos, retrievedX);
		Assert.Equal(yPos, retrievedY);
	}

	[Fact]
	public void PostMessage_WithCharMessage_ShouldQueueCharCode()
	{
		// Arrange
		const uint hwnd = 0x00010000;
		const uint WM_CHAR = 0x0102;
		const uint charCode = 0x61; // 'a'
		const uint lParam = 0x00000001;

		// Act
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_CHAR, charCode, lParam);

		// Assert
		Assert.Equal(1u, result); // TRUE

		// Verify message was queued
		var msgAddr = _testEnv.AllocateMemory(28);
		var getMessage = _testEnv.CallUser32Api("GETMESSAGEA", msgAddr, 0, 0, 0);

		Assert.Equal(1u, getMessage);

		var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
		var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);

		Assert.Equal(WM_CHAR, retrievedMsg);
		Assert.Equal(charCode, retrievedWParam);
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
