using System.Runtime.InteropServices;
using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests ported from ReactOS User32 API test suite
/// Source: https://github.com/reactos/reactos/tree/master/modules/rostests/apitests/user32
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Source", "ReactOS")]
public class ReactOSPortedTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	// Structure size constants using Marshal.SizeOf per coding guidelines
	private static readonly int MsgSize = Marshal.SizeOf<NativeTypes.MSG>();
	private static readonly int WndClassExASize = Marshal.SizeOf<NativeTypes.WNDCLASSEXA>();
	
	// WNDCLASSEXA field offsets (based on NativeTypes.WNDCLASSEXA structure)
	private const int WndClassExA_cbSize = 0;
	private const int WndClassExA_style = 4;
	private const int WndClassExA_lpfnWndProc = 8;
	private const int WndClassExA_cbClsExtra = 12;
	private const int WndClassExA_cbWndExtra = 16;
	private const int WndClassExA_hInstance = 20;
	private const int WndClassExA_hIcon = 24;
	private const int WndClassExA_hCursor = 28;
	private const int WndClassExA_hbrBackground = 32;
	private const int WndClassExA_lpszMenuName = 36;
	private const int WndClassExA_lpszClassName = 40;
	private const int WndClassExA_hIconSm = 44;
	
	// Test fixture values for SetScrollRange tests
	private const int TestInitialMin = 123;
	private const int TestInitialMax = 456;
	
	// Memory allocation sizes
	private const int PointerSize = 4;  // 32-bit pointers

	public ReactOSPortedTests()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv.Dispose();
		GC.SuppressFinalize(this);
	}

	#region GetMessage/PeekMessage Tests
	// Ported from: rostests/apitests/user32/GetPeekMessage.c
	// Original copyright: Thomas Faber <thomas.faber@reactos.org>

	[Fact]
	public void GetMessage_WithInvalidWindowHandle_ShouldReturnMinusOne()
	{
		// Arrange - Create a window and then destroy it to get invalid handle
		var classNamePtr = _testEnv.WriteString("EDIT");
		var titlePtr = _testEnv.WriteString("test");
		
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,              // dwExStyle
			classNamePtr,   // lpClassName
			titlePtr,       // lpWindowName
			0x00000000,     // dwStyle
			0,              // x (CW_USEDEFAULT)
			0,              // y (CW_USEDEFAULT)
			0,              // nWidth (CW_USEDEFAULT)
			0,              // nHeight (CW_USEDEFAULT)
			0,              // hWndParent
			0,              // hMenu
			0,              // hInstance
			0               // lpParam
		);

		// Destroy the window to make handle invalid
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);

		// Allocate MSG structure
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0xDEADBEEF); // DNS_ERROR_RCODE_NXRRSET equivalent
		var result = _testEnv.CallUser32Api("GETMESSAGEA", msgPtr, hwnd, 0, 0);

		// Assert
		Assert.Equal(unchecked((uint)-1), result); // Should return -1
		const uint ERROR_INVALID_WINDOW_HANDLE = 1400;
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, _testEnv.CallKernel32Api("GETLASTERROR"));
	}

	[Fact]
	public void PeekMessage_WithInvalidWindowHandle_ShouldReturnZero()
	{
		// Arrange - Create a window and then destroy it to get invalid handle
		var classNamePtr = _testEnv.WriteString("EDIT");
		var titlePtr = _testEnv.WriteString("test");

		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,              // dwExStyle
			classNamePtr,   // lpClassName
			titlePtr,       // lpWindowName
			0x00000000,     // dwStyle
			0,              // x
			0,              // y
			0,              // nWidth
			0,              // nHeight
			0,              // hWndParent
			0,              // hMenu
			0x00400000,  // hInstance
			0               // lpParam
		);

		// Destroy the window to make handle invalid
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);

		// Allocate MSG structure
		var msgPtr = _testEnv.AllocateMemory((uint)MsgSize);

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0xDEADBEEF);
		const uint PM_NOREMOVE = 0x0000;
		var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgPtr, hwnd, 0, 0, PM_NOREMOVE);

		// Assert
		Assert.Equal(0u, result); // Should return 0
		const uint ERROR_INVALID_WINDOW_HANDLE = 1400;
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, _testEnv.CallKernel32Api("GETLASTERROR"));
	}

	#endregion

	#region SetScrollRange Tests
	// Ported from: rostests/apitests/user32/SetScrollRange.c
	// Original copyright: Thomas Faber <thomas.faber@reactos.org>

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(0, int.MaxValue, true)]
	[InlineData(-1, int.MaxValue, false)]
	[InlineData(int.MinValue, int.MaxValue, false)]
	[InlineData(int.MinValue, 0, false)]
	[InlineData(int.MinValue, -1, true)]
	public void SetScrollRange_WithVariousRanges_ShouldValidateCorrectly(int nMin, int nMax, bool shouldSucceed)
	{
		// Arrange - Create a scrollbar control
		var classNamePtr = _testEnv.WriteString("SCROLLBAR");
		
		var hScroll = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,              // dwExStyle
			classNamePtr,   // lpClassName
			0,              // lpWindowName (NULL)
			0x00000000,     // dwStyle
			0, 0, 0, 0,     // x, y, width, height
			0,              // hWndParent
			0,              // hMenu
			0,              // hInstance
			0               // lpParam
		);

		Assert.NotEqual(0u, hScroll);

		// Set initial values to known state
		const int SB_CTL = 2;
		_testEnv.CallUser32Api("SETSCROLLRANGE", hScroll, SB_CTL, TestInitialMin, TestInitialMax, 0 /* FALSE */);

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0xdeaff00d);
		var success = _testEnv.CallUser32Api("SETSCROLLRANGE", hScroll, SB_CTL, (uint)nMin, (uint)nMax, 0 /* FALSE */);

		// Get the new range to verify
		var minPtr = _testEnv.AllocateMemory(PointerSize);
		var maxPtr = _testEnv.AllocateMemory(PointerSize);
		_testEnv.CallUser32Api("GETSCROLLRANGE", hScroll, SB_CTL, minPtr, maxPtr);
		var newMin = (int)_testEnv.Memory.Read32(minPtr);
		var newMax = (int)_testEnv.Memory.Read32(maxPtr);

		// Assert
		if (shouldSucceed)
		{
			Assert.NotEqual(0u, success); // TRUE
			Assert.Equal(nMin, newMin);
			Assert.Equal(nMax, newMax);
		}
		else
		{
			Assert.Equal(0u, success); // FALSE
			const uint ERROR_INVALID_SCROLLBAR_RANGE = 1448;
			Assert.Equal(ERROR_INVALID_SCROLLBAR_RANGE, _testEnv.CallKernel32Api("GETLASTERROR"));
			Assert.Equal(TestInitialMin, newMin); // Should remain unchanged
			Assert.Equal(TestInitialMax, newMax); // Should remain unchanged
		}

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hScroll);
	}

	#endregion

	#region GetSetWindowInt Tests
	// Ported from: rostests/apitests/user32/GetSetWindowInt.c
	// Original copyright: Timo Kreuzer <timo.kreuzer@reactos.org>

	[Fact]
	public void SetWindowWord_GetWindowWord_ShouldStoreAndRetrieveValues()
	{
		// Arrange - Register a window class with cbWndExtra = 5
		var wndClassPtr = _testEnv.WriteWndClassA(
			className: "ProTestClass1",
			wndProc: 0x00401000,
			cbClsExtra: 1,
			cbWndExtra: 5
		);

		var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);
		Assert.NotEqual(0u, atom);

		var classNamePtr = _testEnv.WriteString("ProTestClass1");
		var titlePtr = _testEnv.WriteString("WindowTitle");

		const uint WS_POPUP = 0x80000000;
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr,   // lpClassName
			titlePtr,       // lpWindowName
			WS_POPUP,       // dwStyle
			0, 0, 0, 0,     // x, y, width, height (CW_USEDEFAULT)
			0,              // hWndParent
			0,              // hMenu
			0x00400000,  // hInstance
			0               // lpParam
		);

		Assert.NotEqual(0u, hwnd);

		// Act & Assert - Test SetWindowWord/GetWindowWord with overlapping offsets
		_testEnv.CallKernel32Api("SETLASTERROR", 0xdeadbeef);

		// Offset 0
		Assert.Equal(0u, _testEnv.CallUser32Api("SETWINDOWWORD", hwnd, 0, 0x1234));
		Assert.Equal(0x1234u, _testEnv.CallUser32Api("GETWINDOWWORD", hwnd, 0));

		// Offset 1 - overlaps with previous value
		Assert.Equal(0x12u, _testEnv.CallUser32Api("SETWINDOWWORD", hwnd, 1, 0x2345));
		Assert.Equal(0x2345u, _testEnv.CallUser32Api("GETWINDOWWORD", hwnd, 1));

		// Offset 2
		Assert.Equal(0x23u, _testEnv.CallUser32Api("SETWINDOWWORD", hwnd, 2, 0x3456));
		Assert.Equal(0x3456u, _testEnv.CallUser32Api("GETWINDOWWORD", hwnd, 2));

		// Offset 3
		Assert.Equal(0x34u, _testEnv.CallUser32Api("SETWINDOWWORD", hwnd, 3, 0x4567));
		Assert.Equal(0x4567u, _testEnv.CallUser32Api("GETWINDOWWORD", hwnd, 3));

		// Offset 4 - out of bounds (cbWndExtra = 5, valid offsets are 0-3)
		Assert.Equal(0xdeadbeef, _testEnv.CallKernel32Api("GETLASTERROR")); // Should not have changed yet

		const uint ERROR_INVALID_INDEX = 1413;
		Assert.Equal(0u, _testEnv.CallUser32Api("SETWINDOWWORD", hwnd, 4, 0x5678));
		Assert.Equal(ERROR_INVALID_INDEX, _testEnv.CallKernel32Api("GETLASTERROR"));

		_testEnv.CallKernel32Api("SETLASTERROR", 0xdeadbeef);
		Assert.Equal(0u, _testEnv.CallUser32Api("GETWINDOWWORD", hwnd, 4));
		Assert.Equal(ERROR_INVALID_INDEX, _testEnv.CallKernel32Api("GETLASTERROR"));

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
	}

	[Fact]
	public void SetWindowLong_GetWindowLong_ShouldStoreAndRetrieveValues()
	{
		// Arrange - Register a window class with cbWndExtra = 5
		var wndClassPtr = _testEnv.WriteWndClassA(
			className: "ProTestClass2",
			wndProc: 0x00401000,
			cbClsExtra: 1,
			cbWndExtra: 5
		);

		var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);
		Assert.NotEqual(0u, atom);

		var classNamePtr = _testEnv.WriteString("ProTestClass2");
		var titlePtr = _testEnv.WriteString("WindowTitle");

		const uint WS_POPUP = 0x80000000;
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr,
			titlePtr,
			WS_POPUP,
			0, 0, 0, 0,
			0, 0,
			0x00400000,
			0
		);

		Assert.NotEqual(0u, hwnd);

		// Act & Assert - Test SetWindowLong/GetWindowLong
		_testEnv.CallKernel32Api("SETLASTERROR", 0xdeadbeef);

		// Offset 0
		Assert.Equal(0x67564534u, _testEnv.CallUser32Api("SETWINDOWLONGA", hwnd, 0, 0x12345678));
		Assert.Equal(0x12345678u, _testEnv.CallUser32Api("GETWINDOWLONGA", hwnd, 0));

		// Offset 1 - overlaps with previous value
		Assert.Equal(0x45123456u, _testEnv.CallUser32Api("SETWINDOWLONGA", hwnd, 1, 0x23456789));
		Assert.Equal(0x23456789u, _testEnv.CallUser32Api("GETWINDOWLONGA", hwnd, 1));

		// Offset 2 - out of bounds (cbWndExtra = 5, need 4 bytes, so valid offsets are 0-1)
		Assert.Equal(0xdeadbeef, _testEnv.CallKernel32Api("GETLASTERROR"));

		const uint ERROR_INVALID_INDEX = 1413;
		Assert.Equal(0u, _testEnv.CallUser32Api("SETWINDOWLONGA", hwnd, 2, 0x3456789a));
		Assert.Equal(ERROR_INVALID_INDEX, _testEnv.CallKernel32Api("GETLASTERROR"));

		_testEnv.CallKernel32Api("SETLASTERROR", 0xdeadbeef);
		Assert.Equal(0u, _testEnv.CallUser32Api("GETWINDOWLONGA", hwnd, 2));
		Assert.Equal(ERROR_INVALID_INDEX, _testEnv.CallKernel32Api("GETLASTERROR"));

		// Cleanup
		_testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
	}

	#endregion

	#region GetClassInfo Tests
	// Ported from: rostests/apitests/user32/GetClassInfo.c
	// Original copyright: Timo Kreuzer <timo.kreuzer@reactos.org>

	[Fact]
	public void GetClassInfoExW_Desktop_ShouldReturnCorrectClassInfo()
	{
		// Arrange
		const uint WC_DESKTOP = 0x8001; // System class atom for desktop
		var wcexPtr = _testEnv.AllocateMemory((uint)WndClassExASize);
		
		// Fill with pattern to detect unmodified fields
		for (uint i = 0; i < WndClassExASize; i++)
		{
			_testEnv.Memory.Write8(wcexPtr + i, 0xab);
		}

		// Act
		var result = _testEnv.CallUser32Api("GETCLASSINFOEXA",
			0x00400000,
			WC_DESKTOP,
			wcexPtr
		);

		// Assert
		Assert.Equal(WC_DESKTOP, result); // Returns the atom on success

		// Read the structure fields using offset constants
		var cbSize = _testEnv.Memory.Read32(wcexPtr + WndClassExA_cbSize);
		var style = _testEnv.Memory.Read32(wcexPtr + WndClassExA_style);
		var lpfnWndProc = _testEnv.Memory.Read32(wcexPtr + WndClassExA_lpfnWndProc);
		var cbClsExtra = _testEnv.Memory.Read32(wcexPtr + WndClassExA_cbClsExtra);
		var hInstance = _testEnv.Memory.Read32(wcexPtr + WndClassExA_hInstance);
		var hIcon = _testEnv.Memory.Read32(wcexPtr + WndClassExA_hIcon);
		var hCursor = _testEnv.Memory.Read32(wcexPtr + WndClassExA_hCursor);
		var lpszMenuName = _testEnv.Memory.Read32(wcexPtr + WndClassExA_lpszMenuName);
		var lpszClassName = _testEnv.Memory.Read32(wcexPtr + WndClassExA_lpszClassName);
		var hIconSm = _testEnv.Memory.Read32(wcexPtr + WndClassExA_hIconSm);

		// cbSize should not be modified
		Assert.Equal(0xabababab, cbSize);

		// Desktop class style should be CS_GLOBALCLASS (0x8)
		Assert.Equal(0x8u, style);

		// lpfnWndProc should be non-null
		Assert.NotEqual(0u, lpfnWndProc);

		// Desktop has no extra class or window bytes
		Assert.Equal(0u, cbClsExtra);

		// hInstance should be the module handle
		Assert.Equal(0x00400000u, hInstance);

		// Desktop has no icon
		Assert.Equal(0u, hIcon);

		// Cursor should be set
		Assert.NotEqual(0u, hCursor);

		// Menu name should be NULL
		Assert.Equal(0u, lpszMenuName);

		// Class name should be the desktop atom
		Assert.Equal(WC_DESKTOP, lpszClassName);

		// Small icon should be NULL
		Assert.Equal(0u, hIconSm);
	}

	[Fact]
	public void GetClassInfoExW_CustomClass_ShouldReturnCorrectClassInfo()
	{
		// Arrange - Register a custom class
		var wcexPtr = _testEnv.AllocateMemory((uint)WndClassExASize);
		
		// Set up WNDCLASSEXW structure using offset constants
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_cbSize, (uint)WndClassExASize);
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_style, 0x1);
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_lpfnWndProc, 0x00401000); // DefWindowProc
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_cbClsExtra, 1);
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_cbWndExtra, 5);
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_hInstance, 0x00400000);
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_hIcon, 0);
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_hCursor, 0);
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_hbrBackground, 0);
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_lpszMenuName, 0);
		
		var classNamePtr = _testEnv.WriteStringW("ProTestClass3");
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_lpszClassName, classNamePtr);
		_testEnv.Memory.Write32(wcexPtr + WndClassExA_hIconSm, 0);

		// Register the class
		var atom = _testEnv.CallUser32Api("REGISTERCLASSEXW", wcexPtr);
		Assert.NotEqual(0u, atom);

		// Fill wcex with pattern to test what gets modified
		for (uint i = 0; i < WndClassExASize; i++)
		{
			_testEnv.Memory.Write8(wcexPtr + i, 0xab);
		}

		// Act - Get class info by atom
		var result = _testEnv.CallUser32Api("GETCLASSINFOEXA",
			0x00400000,
			atom,
			wcexPtr
		);

		// Assert
		Assert.Equal(atom, result);

		// Read back the structure using offset constants
		var cbSize = _testEnv.Memory.Read32(wcexPtr + WndClassExA_cbSize);
		var style = _testEnv.Memory.Read32(wcexPtr + WndClassExA_style);
		var lpfnWndProc = _testEnv.Memory.Read32(wcexPtr + WndClassExA_lpfnWndProc);
		var cbClsExtra = _testEnv.Memory.Read32(wcexPtr + WndClassExA_cbClsExtra);
		var cbWndExtra = _testEnv.Memory.Read32(wcexPtr + WndClassExA_cbWndExtra);
		var hInstance = _testEnv.Memory.Read32(wcexPtr + WndClassExA_hInstance);
		var hIcon = _testEnv.Memory.Read32(wcexPtr + WndClassExA_hIcon);
		var lpszMenuName = _testEnv.Memory.Read32(wcexPtr + WndClassExA_lpszMenuName);
		var lpszClassName = _testEnv.Memory.Read32(wcexPtr + WndClassExA_lpszClassName);
		var hIconSm = _testEnv.Memory.Read32(wcexPtr + WndClassExA_hIconSm);

		// cbSize should not be modified
		Assert.Equal(0xabababab, cbSize);

		// Verify the class parameters match what we registered
		Assert.Equal(0x1u, style);
		Assert.Equal(0x00401000u, lpfnWndProc);
		Assert.Equal(1u, cbClsExtra);
		Assert.Equal(5u, cbWndExtra);
		Assert.Equal(0x00400000u, hInstance);
		Assert.Equal(0u, hIcon);
		Assert.Equal(0u, lpszMenuName);
		Assert.Equal(atom, lpszClassName); // Class name is returned as atom
		Assert.Equal(0u, hIconSm);
	}

	#endregion
}
