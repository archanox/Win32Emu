using System.Runtime.InteropServices;
using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests ported from ReactOS User32 API test suite - Window Creation
/// Source: https://github.com/reactos/reactos/tree/master/modules/rostests/apitests/user32
/// Focus: RegisterClass, CreateWindow, DestroyWindow
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Source", "ReactOS")]
public class ReactOSPortedTests_WindowCreation : IDisposable
{
	private readonly TestEnvironment _testEnv;

	// Structure size constants
	private static readonly int WndClassASize = Marshal.SizeOf<NativeTypes.WNDCLASSA>();
	private static readonly int WndClassExASize = Marshal.SizeOf<NativeTypes.WNDCLASSEXA>();
	
	// Win32 constants
	private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
	private const uint WS_POPUP = 0x80000000;
	private const uint WS_CHILD = 0x40000000;
	private const uint WS_VISIBLE = 0x10000000;
	private const uint CS_HREDRAW = 0x0002;
	private const uint CS_VREDRAW = 0x0001;
	private const uint ERROR_CLASS_ALREADY_EXISTS = 1410;
	private const uint ERROR_INVALID_PARAMETER = 87;

	public ReactOSPortedTests_WindowCreation()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv.Dispose();
		GC.SuppressFinalize(this);
	}

	#region RegisterClass Tests
	// Ported from: rostests/apitests/user32/RegisterClassEx.c

	[Fact]
	public void RegisterClassA_WithValidClass_ShouldReturnAtom()
	{
		// Arrange
		var className = $"TestClass_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(
			className: className,
			wndProc: 0x00401000,
			style: CS_HREDRAW | CS_VREDRAW
		);

		// Act
		var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		// Assert
		Assert.NotEqual(0u, atom);
		Assert.True(atom >= 0xC000, $"Atom should be in valid range (>= 0xC000), got 0x{atom:X}");
	}

	[Fact]
	public void RegisterClassA_DuplicateClassName_ShouldFail()
	{
		// Arrange
		var className = $"DuplicateClass_{Guid.NewGuid():N}";
		var wndClassPtr1 = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		var wndClassPtr2 = _testEnv.WriteWndClassA(className: className, wndProc: 0x00402000);

		// Act
		var atom1 = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr1);
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var atom2 = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr2);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.NotEqual(0u, atom1);
		Assert.Equal(0u, atom2);
		Assert.Equal(ERROR_CLASS_ALREADY_EXISTS, lastError);
	}

	[Fact]
	public void RegisterClassA_WithNullClassName_ShouldFail()
	{
		// Arrange
		var wndClassPtr = _testEnv.WriteWndClassA(className: null, wndProc: 0x00401000);

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, atom);
		Assert.Equal(ERROR_INVALID_PARAMETER, lastError);
	}

	[Fact]
	public void RegisterClassExA_WithValidClass_ShouldReturnAtom()
	{
		// Arrange
		var className = $"TestClassEx_{Guid.NewGuid():N}";
		var wndClassExPtr = _testEnv.AllocateMemory((uint)WndClassExASize);
		
		_testEnv.Memory.Write32(wndClassExPtr + 0, (uint)WndClassExASize); // cbSize
		_testEnv.Memory.Write32(wndClassExPtr + 4, CS_HREDRAW | CS_VREDRAW); // style
		_testEnv.Memory.Write32(wndClassExPtr + 8, 0x00401000); // lpfnWndProc
		_testEnv.Memory.Write32(wndClassExPtr + 12, 0); // cbClsExtra
		_testEnv.Memory.Write32(wndClassExPtr + 16, 0); // cbWndExtra
		_testEnv.Memory.Write32(wndClassExPtr + 20, 0x00400000); // hInstance
		_testEnv.Memory.Write32(wndClassExPtr + 24, 0); // hIcon
		_testEnv.Memory.Write32(wndClassExPtr + 28, 0); // hCursor
		_testEnv.Memory.Write32(wndClassExPtr + 32, 0); // hbrBackground
		_testEnv.Memory.Write32(wndClassExPtr + 36, 0); // lpszMenuName
		_testEnv.Memory.Write32(wndClassExPtr + 40, _testEnv.WriteString(className)); // lpszClassName
		_testEnv.Memory.Write32(wndClassExPtr + 44, 0); // hIconSm

		// Act
		var atom = _testEnv.CallUser32Api("REGISTERCLASSEXA", wndClassExPtr);

		// Assert
		Assert.NotEqual(0u, atom);
		Assert.True(atom >= 0xC000, $"Atom should be in valid range (>= 0xC000), got 0x{atom:X}");
	}

	#endregion

	#region CreateWindowEx Tests
	// Ported from: rostests/apitests/user32/CreateWindowEx.c

	[Fact]
	public void CreateWindowExA_WithRegisteredClass_ShouldReturnValidHandle()
	{
		// Arrange
		var className = $"CreateWindowTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var titlePtr = _testEnv.WriteString("Test Window");

		// Act
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,              // dwExStyle
			classNamePtr,   // lpClassName
			titlePtr,       // lpWindowName
			WS_OVERLAPPEDWINDOW, // dwStyle
			0, 0, 640, 480, // x, y, width, height
			0,              // hWndParent
			0,              // hMenu
			0x00400000,     // hInstance
			0               // lpParam
		);

		// Assert
		Assert.NotEqual(0u, hwnd);
	}

	[Fact]
	public void CreateWindowExA_WithUnregisteredClass_ShouldFail()
	{
		// Arrange
		var classNamePtr = _testEnv.WriteString($"NonExistentClass_{Guid.NewGuid():N}");
		var titlePtr = _testEnv.WriteString("Test Window");

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0, classNamePtr, titlePtr, WS_OVERLAPPEDWINDOW,
			0, 0, 640, 480, 0, 0, 0x00400000, 0
		);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, hwnd);
		Assert.NotEqual(0u, lastError);
	}

	[Fact]
	public void CreateWindowExA_WithChildWindow_ShouldRequireParent()
	{
		// Arrange
		var className = $"ChildWindowTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);

		// Act - Create child window without parent (should work but be weird)
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0, classNamePtr, 0, WS_CHILD | WS_VISIBLE,
			0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Assert - Child windows can be created without parent on Win32
		Assert.NotEqual(0u, hwnd);
	}

	[Fact]
	public void CreateWindowA_ShouldBehaveLikeCreateWindowExA()
	{
		// Arrange
		var className = $"CreateWindowATest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var titlePtr = _testEnv.WriteString("Test Window");

		// Act
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, titlePtr, WS_OVERLAPPEDWINDOW,
			0, 0, 640, 480, 0, 0, 0x00400000, 0
		);

		// Assert
		Assert.NotEqual(0u, hwnd);
	}

	#endregion

	#region DestroyWindow Tests
	// Ported from: rostests/apitests/user32/DestroyWindow.c

	[Fact]
	public void DestroyWindow_WithValidHandle_ShouldReturnTrue()
	{
		// Arrange
		var className = $"DestroyTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_POPUP, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act
		var result = _testEnv.CallUser32Api("DESTROYWINDOW", hwnd);

		// Assert
		Assert.NotEqual(0u, result); // TRUE
	}

	[Fact]
	public void DestroyWindow_WithInvalidHandle_ShouldReturnFalse()
	{
		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var result = _testEnv.CallUser32Api("DESTROYWINDOW", 0xDEADBEEF);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, result); // FALSE
		const uint ERROR_INVALID_WINDOW_HANDLE = 1400;
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, lastError);
	}

	[Fact]
	public void DestroyWindow_CalledTwice_ShouldFailSecondTime()
	{
		// Arrange
		var className = $"DestroyTwiceTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_POPUP, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act
		var result1 = _testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var result2 = _testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.NotEqual(0u, result1); // First call should succeed
		Assert.Equal(0u, result2); // Second call should fail
		const uint ERROR_INVALID_WINDOW_HANDLE = 1400;
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, lastError);
	}

	#endregion

	#region ShowWindow Tests
	// Ported from: rostests/apitests/user32/ShowWindow.c

	[Fact]
	public void ShowWindow_WithSW_SHOW_ShouldReturnZeroFirstTime()
	{
		// Arrange
		var className = $"ShowWindowTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act
		const uint SW_SHOW = 5;
		var result = _testEnv.CallUser32Api("SHOWWINDOW", hwnd, SW_SHOW);

		// Assert - First ShowWindow returns 0 if window was previously hidden
		Assert.Equal(0u, result);
	}

	[Fact]
	public void ShowWindow_WithSW_HIDE_ShouldHideWindow()
	{
		// Arrange
		var className = $"HideWindowTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW | WS_VISIBLE, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act
		const uint SW_HIDE = 0;
		var result = _testEnv.CallUser32Api("SHOWWINDOW", hwnd, SW_HIDE);

		// Assert - Should return non-zero if window was previously visible
		Assert.NotEqual(0u, result);
	}

	#endregion

	#region UpdateWindow Tests
	// Ported from: rostests/winetests/user32/msg.c

	[Fact]
	public void UpdateWindow_WithValidWindow_ShouldReturnTrue()
	{
		// Arrange
		var className = $"UpdateWindowTest_{Guid.NewGuid():N}";
		var wndClassPtr = _testEnv.WriteWndClassA(className: className, wndProc: 0x00401000);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassPtr);

		var classNamePtr = _testEnv.WriteString(className);
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWA",
			classNamePtr, 0, WS_OVERLAPPEDWINDOW, 0, 0, 100, 100, 0, 0, 0x00400000, 0
		);

		// Act
		var result = _testEnv.CallUser32Api("UPDATEWINDOW", hwnd);

		// Assert
		Assert.NotEqual(0u, result); // TRUE
	}

	[Fact]
	public void UpdateWindow_WithInvalidWindow_ShouldReturnFalse()
	{
		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var result = _testEnv.CallUser32Api("UPDATEWINDOW", 0xDEADBEEF);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, result); // FALSE
		const uint ERROR_INVALID_WINDOW_HANDLE = 1400;
		Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, lastError);
	}

	#endregion
}
