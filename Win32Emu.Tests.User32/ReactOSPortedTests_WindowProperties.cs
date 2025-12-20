using System.Runtime.InteropServices;
using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests ported from ReactOS User32 API test suite - Window Properties and Coordinates
/// Source: https://github.com/reactos/reactos/tree/master/modules/rostests/apitests/user32
/// Focus: GetClientRect, GetWindowRect, ClientToScreen, AdjustWindowRectEx, SetWindowPos
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Source", "ReactOS")]
public class ReactOSPortedTests_WindowProperties : IDisposable
{
	private readonly TestEnvironment _testEnv;

	// Structure sizes
	private static readonly int RectSize = Marshal.SizeOf<NativeTypes.RECT>();
	private static readonly int PointSize = Marshal.SizeOf<NativeTypes.POINT>();
	
	// Win32 constants
	private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
	private const uint WS_POPUP = 0x80000000;
	private const uint WS_BORDER = 0x00800000;
	private const uint WS_CAPTION = 0x00C00000;
	private const uint WS_THICKFRAME = 0x00040000;
	private const uint WS_EX_CLIENTEDGE = 0x00000200;
	private const uint ERROR_INVALID_WINDOW_HANDLE = 1400;

	// SetWindowPos flags
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_NOMOVE = 0x0002;
	private const uint SWP_NOZORDER = 0x0004;
	private const uint SWP_SHOWWINDOW = 0x0040;
	private const uint SWP_HIDEWINDOW = 0x0080;

	public ReactOSPortedTests_WindowProperties()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv.Dispose();
		GC.SuppressFinalize(this);
	}

	#region GetClientRect Tests
	// Ported from: rostests/apitests/user32/GetClientRect.c

	[Fact]
	public void GetClientRect_WithValidWindow_ShouldReturnTrue()
	{
		// Arrange
		var className = $"GetClientRectTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW, 100, 100, 640, 480, 0, 0, 0x00400000, 0
		);

		var rectPtr = _testEnv.AllocateMemory((uint)RectSize);

		// Act
		var result = _testEnv.CallUser32Api("GETCLIENTRECT", hwnd, rectPtr);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Read the RECT structure
		var left = (int)_testEnv.Memory.Read32(rectPtr + 0);
		var top = (int)_testEnv.Memory.Read32(rectPtr + 4);
		var right = (int)_testEnv.Memory.Read32(rectPtr + 8);
		var bottom = (int)_testEnv.Memory.Read32(rectPtr + 12);

		// Client rect should start at 0,0
		Assert.Equal(0, left);
		Assert.Equal(0, top);
		// And have some positive dimensions
		Assert.True(right > 0);
		Assert.True(bottom > 0);
	}

	[Fact]
	public void GetClientRect_WithInvalidWindow_ShouldReturnFalse()
	{
		// Arrange
		var rectPtr = _testEnv.AllocateMemory((uint)RectSize);

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var result = _testEnv.CallUser32Api("GETCLIENTRECT", 0xDEADBEEF, rectPtr);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, result); // FALSE
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, lastError);
	}

	#endregion

	#region GetWindowRect Tests
	// Ported from: rostests/apitests/user32/GetWindowRect.c

	[Fact]
	public void GetWindowRect_WithValidWindow_ShouldReturnScreenCoordinates()
	{
		// Arrange
		var className = $"GetWindowRectTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_POPUP, 100, 150, 640, 480, 0, 0, 0x00400000, 0
		);

		var rectPtr = _testEnv.AllocateMemory((uint)RectSize);

		// Act
		var result = _testEnv.CallUser32Api("GETWINDOWRECT", hwnd, rectPtr);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Read the RECT structure
		var left = (int)_testEnv.Memory.Read32(rectPtr + 0);
		var top = (int)_testEnv.Memory.Read32(rectPtr + 4);
		var right = (int)_testEnv.Memory.Read32(rectPtr + 8);
		var bottom = (int)_testEnv.Memory.Read32(rectPtr + 12);

		// Window rect should be in screen coordinates
		Assert.Equal(100, left);
		Assert.Equal(150, top);
		Assert.Equal(100 + 640, right);
		Assert.Equal(150 + 480, bottom);
	}

	[Fact]
	public void GetWindowRect_WithInvalidWindow_ShouldReturnFalse()
	{
		// Arrange
		var rectPtr = _testEnv.AllocateMemory((uint)RectSize);

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var result = _testEnv.CallUser32Api("GETWINDOWRECT", 0xDEADBEEF, rectPtr);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, result); // FALSE
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, lastError);
	}

	#endregion

	#region ClientToScreen Tests
	// Ported from: rostests/apitests/user32/ClientToScreen.c

	[Fact]
	public void ClientToScreen_WithValidWindow_ShouldTransformCoordinates()
	{
		// Arrange
		var className = $"ClientToScreenTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_POPUP, 100, 150, 640, 480, 0, 0, 0x00400000, 0
		);

		var pointPtr = _testEnv.AllocateMemory((uint)PointSize);
		_testEnv.Memory.Write32(pointPtr + 0, 50); // x = 50
		_testEnv.Memory.Write32(pointPtr + 4, 75); // y = 75

		// Act
		var result = _testEnv.CallUser32Api("CLIENTTOSCREEN", hwnd, pointPtr);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Read transformed coordinates
		var screenX = (int)_testEnv.Memory.Read32(pointPtr + 0);
		var screenY = (int)_testEnv.Memory.Read32(pointPtr + 4);

		// Client (50, 75) should become screen (150, 225)
		Assert.Equal(150, screenX);
		Assert.Equal(225, screenY);
	}

	[Fact]
	public void ClientToScreen_WithInvalidWindow_ShouldReturnFalse()
	{
		// Arrange
		var pointPtr = _testEnv.AllocateMemory((uint)PointSize);
		_testEnv.Memory.Write32(pointPtr + 0, 50);
		_testEnv.Memory.Write32(pointPtr + 4, 75);

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var result = _testEnv.CallUser32Api("CLIENTTOSCREEN", 0xDEADBEEF, pointPtr);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, result); // FALSE
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, lastError);
	}

	[Fact]
	public void ScreenToClient_WithValidWindow_ShouldTransformCoordinates()
	{
		// Arrange
		var className = $"ScreenToClientTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_POPUP, 100, 150, 640, 480, 0, 0, 0x00400000, 0
		);

		var pointPtr = _testEnv.AllocateMemory((uint)PointSize);
		_testEnv.Memory.Write32(pointPtr + 0, 150); // screen x = 150
		_testEnv.Memory.Write32(pointPtr + 4, 225); // screen y = 225

		// Act
		var result = _testEnv.CallUser32Api("SCREENTOCLIENT", hwnd, pointPtr);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Read transformed coordinates
		var clientX = (int)_testEnv.Memory.Read32(pointPtr + 0);
		var clientY = (int)_testEnv.Memory.Read32(pointPtr + 4);

		// Screen (150, 225) should become client (50, 75)
		Assert.Equal(50, clientX);
		Assert.Equal(75, clientY);
	}

	#endregion

	#region AdjustWindowRectEx Tests
	// Ported from: rostests/apitests/user32/AdjustWindowRectEx.c

	[Fact]
	public void AdjustWindowRectEx_WithPopupStyle_ShouldNotChangeRect()
	{
		// Arrange
		var rectPtr = _testEnv.AllocateMemory((uint)RectSize);
		_testEnv.Memory.Write32(rectPtr + 0, 0);   // left
		_testEnv.Memory.Write32(rectPtr + 4, 0);   // top
		_testEnv.Memory.Write32(rectPtr + 8, 640); // right
		_testEnv.Memory.Write32(rectPtr + 12, 480); // bottom

		// Act
		var result = _testEnv.CallUser32Api("ADJUSTWINDOWRECTEX", rectPtr, WS_POPUP, 0u /* no menu */, 0u /* no ex style */);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Read adjusted rect
		var left = (int)_testEnv.Memory.Read32(rectPtr + 0);
		var top = (int)_testEnv.Memory.Read32(rectPtr + 4);
		var right = (int)_testEnv.Memory.Read32(rectPtr + 8);
		var bottom = (int)_testEnv.Memory.Read32(rectPtr + 12);

		// Popup window has no frame, so rect should be unchanged
		Assert.Equal(0, left);
		Assert.Equal(0, top);
		Assert.Equal(640, right);
		Assert.Equal(480, bottom);
	}

	[Fact]
	public void AdjustWindowRectEx_WithBorder_ShouldExpandRect()
	{
		// Arrange
		var rectPtr = _testEnv.AllocateMemory((uint)RectSize);
		_testEnv.Memory.Write32(rectPtr + 0, 0);
		_testEnv.Memory.Write32(rectPtr + 4, 0);
		_testEnv.Memory.Write32(rectPtr + 8, 640);
		_testEnv.Memory.Write32(rectPtr + 12, 480);

		// Act
		var result = _testEnv.CallUser32Api("ADJUSTWINDOWRECTEX", rectPtr, WS_BORDER, 0u, 0u);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Read adjusted rect
		var left = (int)_testEnv.Memory.Read32(rectPtr + 0);
		var top = (int)_testEnv.Memory.Read32(rectPtr + 4);
		var right = (int)_testEnv.Memory.Read32(rectPtr + 8);
		var bottom = (int)_testEnv.Memory.Read32(rectPtr + 12);

		// Border adds pixels on all sides
		Assert.True(left < 0, "Left should be negative to account for border");
		Assert.True(top < 0, "Top should be negative to account for border");
		Assert.True(right > 640, "Right should be expanded to account for border");
		Assert.True(bottom > 480, "Bottom should be expanded to account for border");
	}

	#endregion

	#region SetWindowPos Tests
	// Ported from: rostests/apitests/user32/SetWindowPos.c

	[Fact]
	public void SetWindowPos_WithValidParameters_ShouldReturnTrue()
	{
		// Arrange
		var className = $"SetWindowPosTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act - Move window to new position
		var result = _testEnv.CallUser32Api("SETWINDOWPOS",
			hwnd,
			0u, // hWndInsertAfter (HWND_TOP)
			200u, 300u, // x, y
			100u, 100u, // cx, cy
			SWP_NOZORDER // flags
		);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Verify new position
		var rectPtr = _testEnv.AllocateMemory((uint)RectSize);
		_testEnv.CallUser32Api("GETWINDOWRECT", hwnd, rectPtr);
		var left = (int)_testEnv.Memory.Read32(rectPtr + 0);
		var top = (int)_testEnv.Memory.Read32(rectPtr + 4);

		Assert.Equal(200, left);
		Assert.Equal(300, top);
	}

	[Fact]
	public void SetWindowPos_WithSWP_NOMOVE_ShouldNotMoveWindow()
	{
		// Arrange
		var className = $"SetWindowPosNoMoveTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW, 100, 150, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act - Try to move but with SWP_NOMOVE flag
		var result = _testEnv.CallUser32Api("SETWINDOWPOS",
			hwnd, 0u, 500u, 600u, 100u, 100u, SWP_NOMOVE | SWP_NOZORDER
		);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Verify position unchanged
		var rectPtr = _testEnv.AllocateMemory((uint)RectSize);
		_testEnv.CallUser32Api("GETWINDOWRECT", hwnd, rectPtr);
		var left = (int)_testEnv.Memory.Read32(rectPtr + 0);
		var top = (int)_testEnv.Memory.Read32(rectPtr + 4);

		Assert.Equal(100, left); // Should remain at original position
		Assert.Equal(150, top);
	}

	[Fact]
	public void SetWindowPos_WithInvalidWindow_ShouldReturnFalse()
	{
		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var result = _testEnv.CallUser32Api("SETWINDOWPOS",
			0xDEADBEEF, 0u, 0u, 0u, 100u, 100u, SWP_NOZORDER
		);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, result); // FALSE
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, lastError);
	}

	#endregion

	#region SetRect Tests
	// Ported from: rostests/winetests/user32/win.c

	[Fact]
	public void SetRect_ShouldInitializeRectCorrectly()
	{
		// Arrange
		var rectPtr = _testEnv.AllocateMemory((uint)RectSize);

		// Act
		var result = _testEnv.CallUser32Api("SETRECT", rectPtr, 10, 20, 100, 200);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		var left = (int)_testEnv.Memory.Read32(rectPtr + 0);
		var top = (int)_testEnv.Memory.Read32(rectPtr + 4);
		var right = (int)_testEnv.Memory.Read32(rectPtr + 8);
		var bottom = (int)_testEnv.Memory.Read32(rectPtr + 12);

		Assert.Equal(10, left);
		Assert.Equal(20, top);
		Assert.Equal(100, right);
		Assert.Equal(200, bottom);
	}

	#endregion
}
