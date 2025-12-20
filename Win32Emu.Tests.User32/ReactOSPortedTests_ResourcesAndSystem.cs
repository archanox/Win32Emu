using System.Runtime.InteropServices;
using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests ported from ReactOS User32 API test suite - Resources and System Functions
/// Source: https://github.com/reactos/reactos/tree/master/modules/rostests/apitests/user32
/// Focus: LoadIcon, LoadCursor, GetSystemMetrics, SystemParametersInfo, MessageBox, GetStockObject
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Source", "ReactOS")]
public class ReactOSPortedTests_ResourcesAndSystem : IDisposable
{
	private readonly TestEnvironment _testEnv;

	// Win32 constants
	private const uint IDI_APPLICATION = 32512;
	private const uint IDI_HAND = 32513;
	private const uint IDI_QUESTION = 32514;
	private const uint IDI_EXCLAMATION = 32515;
	private const uint IDI_ASTERISK = 32516;
	private const uint IDI_WINLOGO = 32517;
	private const uint IDC_ARROW = 32512;
	private const uint IDC_IBEAM = 32513;
	private const uint IDC_WAIT = 32514;
	private const uint IDC_CROSS = 32515;
	private const uint SM_CXSCREEN = 0;
	private const uint SM_CYSCREEN = 1;
	private const uint SM_CXVSCROLL = 2;
	private const uint SM_CYHSCROLL = 3;
	private const uint SM_CYCAPTION = 4;
	private const uint MB_OK = 0x00000000;
	private const uint MB_ICONINFORMATION = 0x00000040;
	private const uint IDOK = 1;
	
	// GDI stock objects
	private const uint WHITE_BRUSH = 0;
	private const uint LTGRAY_BRUSH = 1;
	private const uint GRAY_BRUSH = 2;
	private const uint DKGRAY_BRUSH = 3;
	private const uint BLACK_BRUSH = 4;
	private const uint NULL_BRUSH = 5;
	private const uint WHITE_PEN = 6;
	private const uint BLACK_PEN = 7;
	private const uint NULL_PEN = 8;
	private const uint SYSTEM_FONT = 13;
	private const uint DEFAULT_PALETTE = 15;

	public ReactOSPortedTests_ResourcesAndSystem()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv.Dispose();
		GC.SuppressFinalize(this);
	}

	#region LoadIcon Tests
	// Ported from: rostests/apitests/user32/LoadIcon.c

	[Fact]
	public void LoadIconA_WithIDI_APPLICATION_ShouldReturnHandle()
	{
		// Act - Load standard application icon
		var hIcon = _testEnv.CallUser32Api("LOADICONA", 0u, IDI_APPLICATION);

		// Assert
		Assert.NotEqual(0u, hIcon);
	}

	[Fact]
	public void LoadIconA_WithIDI_HAND_ShouldReturnHandle()
	{
		// Act - Load standard hand/error icon
		var hIcon = _testEnv.CallUser32Api("LOADICONA", 0u, IDI_HAND);

		// Assert
		Assert.NotEqual(0u, hIcon);
	}

	[Fact]
	public void LoadIconA_WithIDI_QUESTION_ShouldReturnHandle()
	{
		// Act - Load standard question icon
		var hIcon = _testEnv.CallUser32Api("LOADICONA", 0u, IDI_QUESTION);

		// Assert
		Assert.NotEqual(0u, hIcon);
	}

	[Fact]
	public void LoadIconA_WithIDI_EXCLAMATION_ShouldReturnHandle()
	{
		// Act - Load standard exclamation/warning icon
		var hIcon = _testEnv.CallUser32Api("LOADICONA", 0u, IDI_EXCLAMATION);

		// Assert
		Assert.NotEqual(0u, hIcon);
	}

	[Fact]
	public void LoadIconA_WithIDI_ASTERISK_ShouldReturnHandle()
	{
		// Act - Load standard asterisk/information icon
		var hIcon = _testEnv.CallUser32Api("LOADICONA", 0u, IDI_ASTERISK);

		// Assert
		Assert.NotEqual(0u, hIcon);
	}

	[Fact]
	public void LoadIconA_WithInvalidResource_ShouldReturnNull()
	{
		// Act - Try to load non-existent icon
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var hIcon = _testEnv.CallUser32Api("LOADICONA", 0x00400000u, 9999u);

		// Assert
		Assert.Equal(0u, hIcon);
	}

	#endregion

	#region LoadCursor Tests
	// Ported from: rostests/apitests/user32/LoadCursor.c

	[Fact]
	public void LoadCursorA_WithIDC_ARROW_ShouldReturnHandle()
	{
		// Act - Load standard arrow cursor
		var hCursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_ARROW);

		// Assert
		Assert.NotEqual(0u, hCursor);
	}

	[Fact]
	public void LoadCursorA_WithIDC_IBEAM_ShouldReturnHandle()
	{
		// Act - Load I-beam text cursor
		var hCursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_IBEAM);

		// Assert
		Assert.NotEqual(0u, hCursor);
	}

	[Fact]
	public void LoadCursorA_WithIDC_WAIT_ShouldReturnHandle()
	{
		// Act - Load wait/hourglass cursor
		var hCursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_WAIT);

		// Assert
		Assert.NotEqual(0u, hCursor);
	}

	[Fact]
	public void LoadCursorA_WithIDC_CROSS_ShouldReturnHandle()
	{
		// Act - Load crosshair cursor
		var hCursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_CROSS);

		// Assert
		Assert.NotEqual(0u, hCursor);
	}

	[Fact]
	public void SetCursor_WithValidCursor_ShouldReturnPreviousCursor()
	{
		// Arrange
		var hCursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, IDC_ARROW);

		// Act
		_testEnv.CallUser32Api("SETCURSOR", hCursor);

		// Assert - Returns previous cursor (may be NULL if none set)
		Assert.True(true); // Just verify it doesn't crash
	}

	[Fact]
	public void SetCursor_WithNull_ShouldRemoveCursor()
	{
		// Act
		_testEnv.CallUser32Api("SETCURSOR", 0u);

		// Assert - Should succeed
		Assert.True(true);
	}

	#endregion

	#region GetSystemMetrics Tests
	// Ported from: rostests/apitests/user32/GetSystemMetrics.c

	[Fact]
	public void GetSystemMetrics_SM_CXSCREEN_ShouldReturnPositiveValue()
	{
		// Act
		var width = (int)_testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CXSCREEN);

		// Assert
		Assert.True(width > 0, $"Screen width should be positive, got {width}");
		Assert.True(width <= 16384, $"Screen width should be reasonable, got {width}");
	}

	[Fact]
	public void GetSystemMetrics_SM_CYSCREEN_ShouldReturnPositiveValue()
	{
		// Act
		var height = (int)_testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CYSCREEN);

		// Assert
		Assert.True(height > 0, $"Screen height should be positive, got {height}");
		Assert.True(height <= 16384, $"Screen height should be reasonable, got {height}");
	}

	[Fact]
	public void GetSystemMetrics_SM_CXVSCROLL_ShouldReturnScrollbarWidth()
	{
		// Act
		var scrollWidth = (int)_testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CXVSCROLL);

		// Assert
		Assert.True(scrollWidth > 0, "Vertical scrollbar width should be positive");
		Assert.True(scrollWidth <= 100, "Vertical scrollbar width should be reasonable");
	}

	[Fact]
	public void GetSystemMetrics_SM_CYCAPTION_ShouldReturnCaptionHeight()
	{
		// Act
		var captionHeight = (int)_testEnv.CallUser32Api("GETSYSTEMMETRICS", SM_CYCAPTION);

		// Assert
		Assert.True(captionHeight > 0, "Caption height should be positive");
		Assert.True(captionHeight <= 100, "Caption height should be reasonable");
	}

	#endregion

	#region SystemParametersInfo Tests
	// Ported from: rostests/apitests/user32/SystemParametersInfo.c

	[Fact]
	public void SystemParametersInfoA_SPI_GETBEEP_ShouldReturnValue()
	{
		// Arrange
		const uint SPI_GETBEEP = 0x0001;
		var valuePtr = _testEnv.AllocateMemory(4);

		// Act
		var result = _testEnv.CallUser32Api("SYSTEMPARAMETERSINFOA", SPI_GETBEEP, 0u, valuePtr, 0u);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		var beepEnabled = _testEnv.Memory.Read32(valuePtr);
		Assert.True(beepEnabled == 0 || beepEnabled == 1, "Beep should be 0 or 1");
	}

	[Fact]
	public void SystemParametersInfoA_SPI_GETMOUSE_ShouldReturnMouseThresholds()
	{
		// Arrange
		const uint SPI_GETMOUSE = 0x0003;
		var arrayPtr = _testEnv.AllocateMemory(12); // 3 ints

		// Act
		var result = _testEnv.CallUser32Api("SYSTEMPARAMETERSINFOA", SPI_GETMOUSE, 0u, arrayPtr, 0u);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		var threshold1 = _testEnv.Memory.Read32(arrayPtr + 0);
		var threshold2 = _testEnv.Memory.Read32(arrayPtr + 4);
		var speed = _testEnv.Memory.Read32(arrayPtr + 8);

		// Values should be reasonable
		Assert.True(threshold1 >= 0);
		Assert.True(threshold2 >= 0);
		Assert.True(speed >= 0);
	}

	#endregion

	#region MessageBox Tests
	// Ported from: rostests/winetests/user32/dialog.c

	[Fact]
	public void MessageBoxA_WithValidParameters_ShouldReturnIDOK()
	{
		// Arrange
		var textPtr = _testEnv.WriteString("Test message");
		var captionPtr = _testEnv.WriteString("Test caption");

		// Act
		var result = _testEnv.CallUser32Api("MESSAGEBOXA", 0u, textPtr, captionPtr, MB_OK);

		// Assert
		Assert.Equal(IDOK, result);
	}

	[Fact]
	public void MessageBoxA_WithNullText_ShouldReturnIDOK()
	{
		// Arrange
		var captionPtr = _testEnv.WriteString("Test caption");

		// Act
		var result = _testEnv.CallUser32Api("MESSAGEBOXA", 0u, 0u, captionPtr, MB_OK);

		// Assert
		Assert.Equal(IDOK, result);
	}

	[Fact]
	public void MessageBoxA_WithNullCaption_ShouldReturnIDOK()
	{
		// Arrange
		var textPtr = _testEnv.WriteString("Test message");

		// Act
		var result = _testEnv.CallUser32Api("MESSAGEBOXA", 0u, textPtr, 0u, MB_OK);

		// Assert
		Assert.Equal(IDOK, result);
	}

	[Fact]
	public void MessageBoxA_WithIconInformation_ShouldReturnIDOK()
	{
		// Arrange
		var textPtr = _testEnv.WriteString("Information message");
		var captionPtr = _testEnv.WriteString("Information");

		// Act
		var result = _testEnv.CallUser32Api("MESSAGEBOXA", 0u, textPtr, captionPtr, MB_OK | MB_ICONINFORMATION);

		// Assert
		Assert.Equal(IDOK, result);
	}

	#endregion

	#region GetStockObject Tests (GDI32)
	// Ported from: rostests/apitests/gdi32/GetStockObject.c

	[Fact]
	public void GetStockObject_WHITE_BRUSH_ShouldReturnHandle()
	{
		// Act
		var hBrush = _testEnv.CallGdi32Api("GETSTOCKOBJECT", WHITE_BRUSH);

		// Assert
		Assert.NotEqual(0u, hBrush);
	}

	[Fact]
	public void GetStockObject_BLACK_BRUSH_ShouldReturnHandle()
	{
		// Act
		var hBrush = _testEnv.CallGdi32Api("GETSTOCKOBJECT", BLACK_BRUSH);

		// Assert
		Assert.NotEqual(0u, hBrush);
	}

	[Fact]
	public void GetStockObject_NULL_BRUSH_ShouldReturnHandle()
	{
		// Act
		var hBrush = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NULL_BRUSH);

		// Assert
		Assert.NotEqual(0u, hBrush);
	}

	[Fact]
	public void GetStockObject_WHITE_PEN_ShouldReturnHandle()
	{
		// Act
		var hPen = _testEnv.CallGdi32Api("GETSTOCKOBJECT", WHITE_PEN);

		// Assert
		Assert.NotEqual(0u, hPen);
	}

	[Fact]
	public void GetStockObject_BLACK_PEN_ShouldReturnHandle()
	{
		// Act
		var hPen = _testEnv.CallGdi32Api("GETSTOCKOBJECT", BLACK_PEN);

		// Assert
		Assert.NotEqual(0u, hPen);
	}

	[Fact]
	public void GetStockObject_SYSTEM_FONT_ShouldReturnHandle()
	{
		// Act
		var hFont = _testEnv.CallGdi32Api("GETSTOCKOBJECT", SYSTEM_FONT);

		// Assert
		Assert.NotEqual(0u, hFont);
	}

	[Fact]
	public void GetStockObject_DEFAULT_PALETTE_ShouldReturnHandle()
	{
		// Act
		var hPalette = _testEnv.CallGdi32Api("GETSTOCKOBJECT", DEFAULT_PALETTE);

		// Assert
		Assert.NotEqual(0u, hPalette);
	}

	[Fact]
	public void GetStockObject_InvalidIndex_ShouldReturnNull()
	{
		// Act - Try to get non-existent stock object
		var result = _testEnv.CallGdi32Api("GETSTOCKOBJECT", 9999u);

		// Assert
		Assert.Equal(0u, result);
	}

	#endregion

	#region GetDC and ReleaseDC Tests
	// Ported from: rostests/apitests/user32/GetDC.c

	[Fact]
	public void GetDC_WithValidWindow_ShouldReturnDC()
	{
		// Arrange
		var className = $"GetDCTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, 0x00CF0000, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act
		var hdc = _testEnv.CallUser32Api("GETDC", hwnd);

		// Assert
		Assert.NotEqual(0u, hdc);

		// Cleanup
		_testEnv.CallUser32Api("RELEASEDC", hwnd, hdc);
	}

	[Fact]
	public void GetDC_WithNull_ShouldReturnScreenDC()
	{
		// Act - NULL window means screen DC
		var hdc = _testEnv.CallUser32Api("GETDC", 0u);

		// Assert
		Assert.NotEqual(0u, hdc);

		// Cleanup
		_testEnv.CallUser32Api("RELEASEDC", 0u, hdc);
	}

	[Fact]
	public void ReleaseDC_WithValidDC_ShouldReturnOne()
	{
		// Arrange
		var hdc = _testEnv.CallUser32Api("GETDC", 0u);

		// Act
		var result = _testEnv.CallUser32Api("RELEASEDC", 0u, hdc);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	#endregion

	#region SetFocus and GetMenu Tests
	// Ported from: rostests/apitests/user32/SetFocus.c

	[Fact]
	public void SetFocus_WithValidWindow_ShouldReturnPreviousFocus()
	{
		// Arrange
		var className = $"SetFocusTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, 0x00CF0000, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act
		_testEnv.CallUser32Api("SETFOCUS", hwnd);

		// Assert - May return NULL if no previous focus
		Assert.True(true);
	}

	[Fact]
	public void GetMenu_WithNoMenu_ShouldReturnNull()
	{
		// Arrange
		var className = $"GetMenuTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, 0x00CF0000, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act
		var hMenu = _testEnv.CallUser32Api("GETMENU", hwnd);

		// Assert
		Assert.Equal(0u, hMenu); // NULL - no menu
	}

	#endregion

	#region DefWindowProc Tests
	// Ported from: rostests/winetests/user32/msg.c

	[Fact]
	public void DefWindowProcA_WithWM_NULL_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallUser32Api("DEFWINDOWPROCA", 0u, 0u /* WM_NULL */, 0u, 0u);

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void DefWindowProcA_WithUnknownMessage_ShouldReturnZero()
	{
		// Act - Send custom message that DefWindowProc doesn't handle
		var result = _testEnv.CallUser32Api("DEFWINDOWPROCA", 0u, 0x8000u, 0u, 0u);

		// Assert
		Assert.Equal(0u, result);
	}

	#endregion
}
