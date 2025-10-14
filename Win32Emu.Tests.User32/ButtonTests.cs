using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for button control functionality
/// </summary>
public class ButtonTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public ButtonTests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void Button_WM_LBUTTONUP_ShouldSendWM_COMMAND_ToParent()
	{
		// Arrange - Create a parent window
		var wndClassAddr = _testEnv.WriteWndClassA(
			className: "TestClass",
			wndProc: 0x00401000
		);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

		var classNamePtr = _testEnv.WriteString("TestClass");
		var titlePtr = _testEnv.WriteString("Test Window");

		var parentHwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
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

		Assert.NotEqual(0u, parentHwnd);

		// Create a button control
		var buttonClassPtr = _testEnv.WriteString("BUTTON");
		var buttonTextPtr = _testEnv.WriteString("Test Button");
		const uint controlId = 1001;

		var buttonHwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,                  // dwExStyle
			buttonClassPtr,     // lpClassName
			buttonTextPtr,      // lpWindowName
			0x50000000,         // dwStyle (WS_CHILD | WS_VISIBLE)
			10,                 // x
			10,                 // y
			100,                // nWidth
			30,                 // nHeight
			parentHwnd,         // hWndParent
			controlId,          // hMenu (control ID for child windows)
			0,                  // hInstance
			0                   // lpParam
		);

		Assert.NotEqual(0u, buttonHwnd);

		// Act - Send WM_LBUTTONDOWN and WM_LBUTTONUP to the button
		_testEnv.CallUser32Api("POSTMESSAGEA", buttonHwnd, 0x0201, 0x0001, 0); // WM_LBUTTONDOWN
		_testEnv.CallUser32Api("POSTMESSAGEA", buttonHwnd, 0x0202, 0, 0);      // WM_LBUTTONUP

		// Process the messages through GetMessage/DispatchMessage loop
		var msgStructPtr = _testEnv.AllocateMemory(28); // sizeof(MSG)

		// Get and dispatch WM_LBUTTONDOWN
		var result1 = _testEnv.CallUser32Api("GETMESSAGEA", msgStructPtr, 0, 0, 0);
		Assert.NotEqual(0u, result1); // Should return non-zero (not WM_QUIT)
		_testEnv.CallUser32Api("DISPATCHMESSAGEA", msgStructPtr);

		// Get and dispatch WM_LBUTTONUP (this should trigger WM_COMMAND to parent)
		var result2 = _testEnv.CallUser32Api("GETMESSAGEA", msgStructPtr, 0, 0, 0);
		Assert.NotEqual(0u, result2); // Should return non-zero (not WM_QUIT)
		_testEnv.CallUser32Api("DISPATCHMESSAGEA", msgStructPtr);

		// Now check if WM_COMMAND was posted to the parent
		var result3 = _testEnv.CallUser32Api("GETMESSAGEA", msgStructPtr, 0, 0, 0);
		Assert.NotEqual(0u, result3); // Should return non-zero for WM_COMMAND

		// Read the message from the structure
		var hwnd = _testEnv.Memory.Read32(msgStructPtr + 0);
		var message = _testEnv.Memory.Read32(msgStructPtr + 4);
		var wParam = _testEnv.Memory.Read32(msgStructPtr + 8);
		var lParam = _testEnv.Memory.Read32(msgStructPtr + 12);

		// Assert - Verify WM_COMMAND was sent to parent
		Assert.Equal(parentHwnd, hwnd); // Message should be for parent window
		Assert.Equal(0x0111u, message); // WM_COMMAND = 0x0111

		// wParam: HIWORD = notification code (BN_CLICKED = 0), LOWORD = control ID
		var notificationCode = wParam >> 16;
		var receivedControlId = wParam & 0xFFFF;

		Assert.Equal(0u, notificationCode); // BN_CLICKED = 0
		Assert.Equal(controlId, receivedControlId); // Control ID should match
		Assert.Equal(buttonHwnd, lParam); // lParam should be button HWND
	}

	[Fact]
	public void Button_BM_CLICK_ShouldSendWM_COMMAND_ToParent()
	{
		// Arrange - Create a parent window
		var wndClassAddr = _testEnv.WriteWndClassA(
			className: "TestClass",
			wndProc: 0x00401000
		);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

		var classNamePtr = _testEnv.WriteString("TestClass");
		var titlePtr = _testEnv.WriteString("Test Window");

		var parentHwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
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

		Assert.NotEqual(0u, parentHwnd);

		// Create a button control
		var buttonClassPtr = _testEnv.WriteString("BUTTON");
		var buttonTextPtr = _testEnv.WriteString("Test Button");
		const uint controlId = 1002;

		var buttonHwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,                  // dwExStyle
			buttonClassPtr,     // lpClassName
			buttonTextPtr,      // lpWindowName
			0x50000000,         // dwStyle (WS_CHILD | WS_VISIBLE)
			10,                 // x
			10,                 // y
			100,                // nWidth
			30,                 // nHeight
			parentHwnd,         // hWndParent
			controlId,          // hMenu (control ID for child windows)
			0,                  // hInstance
			0                   // lpParam
		);

		Assert.NotEqual(0u, buttonHwnd);

		// Act - Send BM_CLICK message to the button
		_testEnv.CallUser32Api("POSTMESSAGEA", buttonHwnd, 0x00F1, 0, 0); // BM_CLICK = 0x00F1

		// Process the messages through GetMessage/DispatchMessage loop
		var msgStructPtr = _testEnv.AllocateMemory(28); // sizeof(MSG)

		// Get and dispatch BM_CLICK
		var result1 = _testEnv.CallUser32Api("GETMESSAGEA", msgStructPtr, 0, 0, 0);
		Assert.NotEqual(0u, result1); // Should return non-zero (not WM_QUIT)
		_testEnv.CallUser32Api("DISPATCHMESSAGEA", msgStructPtr);

		// Now check if WM_COMMAND was posted to the parent
		var result2 = _testEnv.CallUser32Api("GETMESSAGEA", msgStructPtr, 0, 0, 0);
		Assert.NotEqual(0u, result2); // Should return non-zero for WM_COMMAND

		// Read the message from the structure
		var hwnd = _testEnv.Memory.Read32(msgStructPtr + 0);
		var message = _testEnv.Memory.Read32(msgStructPtr + 4);
		var wParam = _testEnv.Memory.Read32(msgStructPtr + 8);
		var lParam = _testEnv.Memory.Read32(msgStructPtr + 12);

		// Assert - Verify WM_COMMAND was sent to parent
		Assert.Equal(parentHwnd, hwnd); // Message should be for parent window
		Assert.Equal(0x0111u, message); // WM_COMMAND = 0x0111

		// wParam: HIWORD = notification code (BN_CLICKED = 0), LOWORD = control ID
		var notificationCode = wParam >> 16;
		var receivedControlId = wParam & 0xFFFF;

		Assert.Equal(0u, notificationCode); // BN_CLICKED = 0
		Assert.Equal(controlId, receivedControlId); // Control ID should match
		Assert.Equal(buttonHwnd, lParam); // lParam should be button HWND
	}

	public void Dispose()
	{
		_testEnv?.Dispose();
	}
}
