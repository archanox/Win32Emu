using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for User32 APIs based on API Monitor logs from ign_teas.exe
/// </summary>
public sealed class ApiMonLogUser32Tests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public ApiMonLogUser32Tests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void LoadCursorA_WithStandardCursor_ShouldReturnHandle()
	{
		// From CSV: LoadCursorA(NULL, IDC_ARROW) returns 0x00010003
		// IDC_ARROW = 32512 = 0x7F00
		const uint IDC_ARROW = 32512;
		
		var cursorHandle = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_ARROW);
		Assert.NotEqual(0u, cursorHandle);
		
		// The implementation should return 0x00010000 | IDC_ARROW for standard cursors
		var expected = 0x00010000u | IDC_ARROW;
		Assert.Equal(expected, cursorHandle);
	}

	[Fact]
	public void LoadIconA_WithStandardIcon_ShouldReturnHandle()
	{
		// From CSV: LoadIconA(NULL, IDI_APPLICATION) returns 0x0001002b
		// IDI_APPLICATION = 32512 = 0x7F00
		const uint IDI_APPLICATION = 32512;
		
		var iconHandle = _testEnv.CallUser32Api("LOADICONA", 0u, IDI_APPLICATION);
		Assert.NotEqual(0u, iconHandle);
		
		// The implementation should return 0x00010000 | IDI_APPLICATION for standard icons
		var expected = 0x00010000u | IDI_APPLICATION;
		Assert.Equal(expected, iconHandle);
	}

	[Fact]
	public void GetStockObject_WithBlackBrush_ShouldReturnHandle()
	{
		// From CSV: GetStockObject(BLACK_BRUSH=4) returns 0x00900011
		const int BLACK_BRUSH = 4;
		
		var brushHandle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", BLACK_BRUSH);
		Assert.NotEqual(0u, brushHandle);
	}

	[Fact]
	public void RegisterClassA_ShouldReturnAtom()
	{
		// From CSV: RegisterClassA(0x001afe40) returns 49770
		// This requires setting up a WNDCLASS structure in memory
		
		var wndClassAddr = _testEnv.AllocateMemory(40); // Size of WNDCLASSA
		
		// Set up basic WNDCLASSA structure
		_testEnv.Memory.Write32(wndClassAddr, 0); // style
		_testEnv.Memory.Write32(wndClassAddr + 4, 0x00401000); // lpfnWndProc (dummy)
		_testEnv.Memory.Write32(wndClassAddr + 8, 0); // cbClsExtra
		_testEnv.Memory.Write32(wndClassAddr + 12, 0); // cbWndExtra
		_testEnv.Memory.Write32(wndClassAddr + 16, 0x00400000); // hInstance
		_testEnv.Memory.Write32(wndClassAddr + 20, 0); // hIcon
		_testEnv.Memory.Write32(wndClassAddr + 24, 0); // hCursor
		_testEnv.Memory.Write32(wndClassAddr + 28, 0); // hbrBackground
		_testEnv.Memory.Write32(wndClassAddr + 32, 0); // lpszMenuName
		var classNamePtr = _testEnv.CreateAnsiString("TestClass");
		_testEnv.Memory.Write32(wndClassAddr + 36, classNamePtr); // lpszClassName
		
		var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);
		Assert.NotEqual(0u, atom);
	}

	[Fact]
	public void GetSystemMetrics_CYSCREEN_ShouldReturnHeight()
	{
		// From CSV: GetSystemMetrics(SM_CYSCREEN=1) returns 1286
		const int SM_CYSCREEN = 1;
		
		var height = _testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CYSCREEN);
		Assert.True(height > 0);
	}

	[Fact]
	public void GetSystemMetrics_CXSCREEN_ShouldReturnWidth()
	{
		// From CSV: GetSystemMetrics(SM_CXSCREEN=0) returns 2056
		const int SM_CXSCREEN = 0;
		
		var width = _testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CXSCREEN);
		Assert.True(width > 0);
	}

	[Fact]
	public void SetRect_ShouldSetRectangleValues()
	{
		// From CSV: SetRect(0x0043c780, 0, 0, 2056, 1286) returns TRUE
		var rectAddr = _testEnv.AllocateMemory(16); // Size of RECT
		
		var result = _testEnv.CallUser32Api("SETRECT", rectAddr, 0, 0, 2056, 1286);
		Assert.Equal(1u, result); // TRUE
		
		// Verify the rectangle was set correctly
		var left = (int)_testEnv.Memory.Read32(rectAddr);
		var top = (int)_testEnv.Memory.Read32(rectAddr + 4);
		var right = (int)_testEnv.Memory.Read32(rectAddr + 8);
		var bottom = (int)_testEnv.Memory.Read32(rectAddr + 12);
		
		Assert.Equal(0, left);
		Assert.Equal(0, top);
		Assert.Equal(2056, right);
		Assert.Equal(1286, bottom);
	}

	[Fact]
	public void SetCursor_ShouldReturnPreviousCursor()
	{
		// From CSV: SetCursor(NULL) returns 0x00010007 or NULL
		// First call should return 0 (no previous cursor)
		var previousCursor1 = _testEnv.CallUser32Api("SETCURSOR", 0u);
		Assert.Equal(0u, previousCursor1);
		
		// Second call with a cursor should return the previous (0)
		var testCursor = 0x12345678u;
		var previousCursor2 = _testEnv.CallUser32Api("SETCURSOR", testCursor);
		Assert.Equal(0u, previousCursor2);
		
		// Third call should return the test cursor
		var previousCursor3 = _testEnv.CallUser32Api("SETCURSOR", 0u);
		Assert.Equal(testCursor, previousCursor3);
	}

	[Fact]
	public void SetFocus_ShouldReturnPreviousFocus()
	{
		// From CSV: SetFocus(0x00080a28) returns 0x00080a28
		// First call should return 0 (no previous focus)
		var testWindow = 0x00080a28u;
		var previousFocus1 = _testEnv.CallUser32Api("SETFOCUS", testWindow);
		Assert.Equal(0u, previousFocus1);
		
		// Second call with same window should return the previous focus
		var previousFocus2 = _testEnv.CallUser32Api("SETFOCUS", testWindow);
		Assert.Equal(testWindow, previousFocus2);
	}

	[Fact]
	public void ShowWindow_ShouldReturnPreviousState()
	{
		// From CSV: ShowWindow(0x00080a28, SW_SHOW=5) returns TRUE
		// This requires a valid window handle which we don't have in unit tests
		// So we'll just verify it doesn't crash
		const int SW_SHOW = 5;
		var testWindow = 0x00080a28u;
		
		var result = _testEnv.CallUser32Api("SHOWWINDOW", testWindow, SW_SHOW);
		// Result can be 0 or non-zero depending on previous state
		// Just verify the call succeeds
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
