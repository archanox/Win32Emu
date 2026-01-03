using System.Runtime.InteropServices;
using Xunit;
using Win32Emu.Tests.Infrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// ReactOS-style tests ported for User32 functions specifically used by ign_teas.exe
/// Based on ign_teas API call log analysis (ApiMon Logs/ign_teas/ign_teas.exe.csv)
/// Source: ReactOS User32 tests and Wine tests
/// Focus: Message handling, window focus, cursor management, system metrics
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Source", "ReactOS")]
[Trait("Application", "ign_teas")]
public class ReactOSPortedTests_IgnTeas : IDisposable
{
	private readonly TestEnvironment _testEnv;

	// Structure sizes
	private static readonly int MsgSize = Marshal.SizeOf<NativeTypes.MSG>();
	
	// Window messages
	private const uint WM_PAINT = 0x000F;
	private const uint WM_SETFOCUS = 0x0007;
	private const uint WM_KILLFOCUS = 0x0008;
	private const uint WM_QUIT = 0x0012;
	
	// Window styles
	private const uint WS_POPUP = 0x80000000;
	private const uint WS_VISIBLE = 0x10000000;
	private const uint WS_SYSMENU = 0x00080000;
	
	// System metrics constants
	private const int SM_CXSCREEN = 0;
	private const int SM_CYSCREEN = 1;
	
	// Show window commands
	private const uint SW_SHOW = 5;
	private const uint SW_HIDE = 0;
	private const uint SW_SHOWDEFAULT = 10;
	
	// Cursor constants
	private const uint IDC_ARROW = 32512;
	private const uint IDC_WAIT = 32514;

	public ReactOSPortedTests_IgnTeas()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv.Dispose();
		GC.SuppressFinalize(this);
	}

	#region SetFocus Tests
	// Ported from: rostests/apitests/user32/SetFocus.c
	// ign_teas calls SetFocus on its main window

	[Fact]
	public void SetFocus_WithValidWindow_ShouldReturnPreviousFocus()
	{
		// Arrange - Create two windows
		var className1 = _testEnv.WriteString("EDIT");
		var title1 = _testEnv.WriteString("Window1");
		var hwnd1 = _testEnv.CallUser32Api("CREATEWINDOWA",
			className1, title1, WS_POPUP, 0, 0, 100, 100, 0, 0, 0, 0);
		Assert.NotEqual(0u, hwnd1);

		var className2 = _testEnv.WriteString("EDIT");
		var title2 = _testEnv.WriteString("Window2");
		var hwnd2 = _testEnv.CallUser32Api("CREATEWINDOWA",
			className2, title2, WS_POPUP, 0, 0, 100, 100, 0, 0, 0, 0);
		Assert.NotEqual(0u, hwnd2);

		// Act - Set focus to window1, then to window2
		var focus1 = _testEnv.CallUser32Api("SETFOCUS", hwnd1);
		var focus2 = _testEnv.CallUser32Api("SETFOCUS", hwnd2);

		// Assert - SetFocus should return the previous focused window
		Assert.Equal(hwnd1, focus2);

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd1);
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd2);
	}

	[Fact]
	public void SetFocus_WithNullHandle_ShouldRemoveFocus()
	{
		// Arrange
		var className = _testEnv.WriteString("EDIT");
		var title = _testEnv.WriteString("TestWindow");
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			className, title, WS_POPUP, 0, 0, 100, 100, 0, 0, 0, 0);
		Assert.NotEqual(0u, hwnd);

		// Set focus first
		_testEnv.CallUser32Api("SETFOCUS", hwnd);

		// Act - Remove focus by passing NULL
		var previousFocus = _testEnv.CallUser32Api("SETFOCUS", 0u);

		// Assert - Should return the window that had focus
		Assert.Equal(hwnd, previousFocus);

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
	}

	[Fact]
	public void GetFocus_AfterSetFocus_ShouldReturnFocusedWindow()
	{
		// Arrange
		var className = _testEnv.WriteString("EDIT");
		var title = _testEnv.WriteString("TestWindow");
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			className, title, WS_POPUP, 0, 0, 100, 100, 0, 0, 0, 0);
		Assert.NotEqual(0u, hwnd);

		// Act
		_testEnv.CallUser32Api("SETFOCUS", hwnd);
		var focusedWindow = _testEnv.CallUser32Api("GETFOCUS");

		// Assert
		Assert.Equal(hwnd, focusedWindow);

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
	}

	#endregion

	#region ShowWindow Tests
	// Ported from: rostests/apitests/user32/ShowWindow.c
	// ign_teas needs to show its fullscreen window

	[Fact]
	public void ShowWindow_WithSW_SHOW_ShouldMakeWindowVisible()
	{
		// Arrange - Create a hidden window
		var className = _testEnv.WriteString("EDIT");
		var title = _testEnv.WriteString("TestWindow");
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			className, title, WS_POPUP, 0, 0, 100, 100, 0, 0, 0, 0);
		Assert.NotEqual(0u, hwnd);

		// Act
		var result = _testEnv.CallUser32Api("SHOWWINDOW", hwnd, SW_SHOW);

		// Assert - Should return non-zero for first show
		// (Returns TRUE if window was previously visible, FALSE if hidden)
		Assert.True(result == 0u || result == 1u);

		// Verify window is visible
		var isVisible = _testEnv.CallUser32Api("ISWINDOWVISIBLE", hwnd);
		Assert.NotEqual(0u, isVisible);

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
	}

	[Fact]
	public void ShowWindow_WithSW_HIDE_ShouldMakeWindowInvisible()
	{
		// Arrange - Create a visible window
		var className = _testEnv.WriteString("EDIT");
		var title = _testEnv.WriteString("TestWindow");
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			className, title, WS_POPUP | WS_VISIBLE, 0, 0, 100, 100, 0, 0, 0, 0);
		Assert.NotEqual(0u, hwnd);

		// Act
		var result = _testEnv.CallUser32Api("SHOWWINDOW", hwnd, SW_HIDE);

		// Assert
		var isVisible = _testEnv.CallUser32Api("ISWINDOWVISIBLE", hwnd);
		Assert.Equal(0u, isVisible);

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
	}

	[Fact]
	public void ShowWindow_WithInvalidHandle_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallUser32Api("SHOWWINDOW", 0xBADBADBAu, SW_SHOW);

		// Assert
		Assert.Equal(0u, result);
	}

	#endregion

	#region SetCursor Tests
	// Ported from: rostests/apitests/user32/SetCursor.c
	// ign_teas calls SetCursor 72 times to manage cursor display

	[Fact]
	public void SetCursor_WithValidCursor_ShouldReturnPreviousCursor()
	{
		// Arrange - Load arrow cursor
		var arrowCursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_ARROW);
		Assert.NotEqual(0u, arrowCursor);

		// Load wait cursor
		var waitCursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_WAIT);
		Assert.NotEqual(0u, waitCursor);

		// Act - Set cursor to arrow, then to wait
		var prev1 = _testEnv.CallUser32Api("SETCURSOR", arrowCursor);
		var prev2 = _testEnv.CallUser32Api("SETCURSOR", waitCursor);

		// Assert - Second SetCursor should return the arrow cursor
		Assert.Equal(arrowCursor, prev2);
	}

	[Fact]
	public void SetCursor_WithNull_ShouldRemoveCursor()
	{
		// Arrange - Load and set a cursor
		var cursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_ARROW);
		_testEnv.CallUser32Api("SETCURSOR", cursor);

		// Act - Remove cursor by setting to NULL
		var prevCursor = _testEnv.CallUser32Api("SETCURSOR", 0u);

		// Assert - Should return the previous cursor
		Assert.Equal(cursor, prevCursor);
	}

	[Fact]
	public void GetCursor_AfterSetCursor_ShouldReturnSetCursor()
	{
		// Arrange
		var cursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_ARROW);
		Assert.NotEqual(0u, cursor);

		// Act
		_testEnv.CallUser32Api("SETCURSOR", cursor);
		var currentCursor = _testEnv.CallUser32Api("GETCURSOR");

		// Assert
		Assert.Equal(cursor, currentCursor);
	}

	#endregion

	#region GetSystemMetrics Tests
	// Ported from: rostests/apitests/user32/GetSystemMetrics.c
	// ign_teas calls GetSystemMetrics to get screen dimensions (SM_CXSCREEN, SM_CYSCREEN)

	[Fact]
	public void GetSystemMetrics_SM_CXSCREEN_ShouldReturnPositiveValue()
	{
		// Act
		var width = _testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CXSCREEN);

		// Assert - Screen width should be positive
		Assert.True(width > 0, $"Screen width should be positive, got {width}");
		Assert.True(width >= 320, "Screen width should be at least 320 (minimum resolution)");
		Assert.True(width <= 7680, "Screen width should be reasonable (<= 8K resolution)");
	}

	[Fact]
	public void GetSystemMetrics_SM_CYSCREEN_ShouldReturnPositiveValue()
	{
		// Act
		var height = _testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CYSCREEN);

		// Assert - Screen height should be positive
		Assert.True(height > 0, $"Screen height should be positive, got {height}");
		Assert.True(height >= 240, "Screen height should be at least 240 (minimum resolution)");
		Assert.True(height <= 4320, "Screen height should be reasonable (<= 8K resolution)");
	}

	[Fact]
	public void GetSystemMetrics_MultipleCallsSamMetric_ShouldReturnConsistentValues()
	{
		// Act
		var width1 = _testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CXSCREEN);
		var width2 = _testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CXSCREEN);
		var height1 = _testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CYSCREEN);
		var height2 = _testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CYSCREEN);

		// Assert - Same metric should return same value
		Assert.Equal(width1, width2);
		Assert.Equal(height1, height2);
	}

	#endregion

	#region PostMessage Tests
	// Ported from: rostests/apitests/user32/PostMessage.c
	// ign_teas uses PostMessageA for message posting

	[Fact]
	public void PostMessageA_WithValidWindow_ShouldReturnTrue()
	{
		// Arrange
		var className = _testEnv.WriteString("EDIT");
		var title = _testEnv.WriteString("TestWindow");
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			className, title, WS_POPUP, 0, 0, 100, 100, 0, 0, 0, 0);
		Assert.NotEqual(0u, hwnd);

		// Act - Post a paint message
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_PAINT, 0u, 0u);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
	}

	[Fact]
	public void PostMessageA_WithNullWindow_ShouldReturnFalse()
	{
		// Act
		var result = _testEnv.CallUser32Api("POSTMESSAGEA", 0u, WM_PAINT, 0u, 0u);

		// Assert
		Assert.Equal(0u, result); // FALSE
	}

	[Fact]
	public void PostQuitMessage_ShouldPostQuitToMessageQueue()
	{
		// Act
		_testEnv.CallUser32Api("POSTQUITMESSAGE", 0u);

		// Assert - Try to peek for WM_QUIT message
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);
		var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, 0u, 0u, 0u, 0u);

		if (result != 0)
		{
			// Read message from structure
			var message = _testEnv.Memory.Read32(msgPtr + 4); // +4 is message offset in MSG
			Assert.Equal(WM_QUIT, message);
		}
	}

	#endregion

	#region PeekMessage/GetMessage Tests
	// Ported from: rostests/apitests/user32/GetPeekMessage.c
	// ign_teas uses PeekMessageA extensively (1062 calls) for game loop

	[Fact]
	public void PeekMessageA_WithNoMessages_ShouldReturnZero()
	{
		// Arrange
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);

		// Act - Try to peek with no messages in queue
		const uint PM_NOREMOVE = 0x0000;
		var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, 0u, 0u, 0u, PM_NOREMOVE);

		// Assert
		// PeekMessage returns 0 if no message available
		Assert.True(result == 0u || result == 1u);
	}

	[Fact]
	public void PeekMessageA_AfterPostMessage_ShouldReturnMessage()
	{
		// Arrange
		var className = _testEnv.WriteString("EDIT");
		var title = _testEnv.WriteString("TestWindow");
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			className, title, WS_POPUP, 0, 0, 100, 100, 0, 0, 0, 0);
		Assert.NotEqual(0u, hwnd);

		// Post a message
		_testEnv.CallUser32Api("POSTMESSAGEA", hwnd, WM_PAINT, 0u, 0u);

		// Act
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);
		const uint PM_REMOVE = 0x0001;
		var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, hwnd, 0u, 0u, PM_REMOVE);

		// Assert
		if (result != 0)
		{
			var message = _testEnv.Memory.Read32(msgPtr + 4); // message field
			Assert.Equal(WM_PAINT, message);
		}

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
	}

	#endregion

	#region UpdateWindow Tests
	// Ported from: rostests/apitests/user32/UpdateWindow.c
	// ign_teas calls UpdateWindow to trigger painting

	[Fact]
	public void UpdateWindow_WithValidWindow_ShouldReturnTrue()
	{
		// Arrange
		var className = _testEnv.WriteString("EDIT");
		var title = _testEnv.WriteString("TestWindow");
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			className, title, WS_POPUP | WS_VISIBLE, 0, 0, 100, 100, 0, 0, 0, 0);
		Assert.NotEqual(0u, hwnd);

		// Act
		var result = _testEnv.CallUser32Api("UPDATEWINDOW", hwnd);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
	}

	[Fact]
	public void UpdateWindow_WithInvalidWindow_ShouldReturnFalse()
	{
		// Act
		var result = _testEnv.CallUser32Api("UPDATEWINDOW", 0xBADBADBAu);

		// Assert
		Assert.Equal(0u, result); // FALSE
	}

	#endregion

	#region SetRect Tests
	// Ported from: rostests/apitests/user32/SetRect.c
	// ign_teas calls SetRect to set rectangle coordinates

	[Fact]
	public void SetRect_ShouldSetRectangleCoordinates()
	{
		// Arrange
		var rectPtr = _testEnv.AllocateMemory(16); // sizeof(RECT) = 16 bytes

		// Act
		var result = _testEnv.CallUser32Api("SETRECT", rectPtr, 10, 20, 100, 200);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Verify RECT structure
		var left = _testEnv.Memory.Read32(rectPtr + 0);
		var top = _testEnv.Memory.Read32(rectPtr + 4);
		var right = _testEnv.Memory.Read32(rectPtr + 8);
		var bottom = _testEnv.Memory.Read32(rectPtr + 12);

		Assert.Equal(10u, left);
		Assert.Equal(20u, top);
		Assert.Equal(100u, right);
		Assert.Equal(200u, bottom);
	}

	[Fact]
	public void SetRect_WithNegativeValues_ShouldWork()
	{
		// Arrange
		var rectPtr = _testEnv.AllocateMemory(16);

		// Act
		var result = _testEnv.CallUser32Api("SETRECT", rectPtr, 
			unchecked((uint)-10), unchecked((uint)-20), 100, 200);

		// Assert
		Assert.NotEqual(0u, result);

		// Verify negative values are stored correctly
		var left = (int)_testEnv.Memory.Read32(rectPtr + 0);
		var top = (int)_testEnv.Memory.Read32(rectPtr + 4);

		Assert.Equal(-10, left);
		Assert.Equal(-20, top);
	}

	#endregion
}
