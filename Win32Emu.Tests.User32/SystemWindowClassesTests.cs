using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;
using Xunit;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for system window classes that are pre-registered by the OS.
/// These classes should be available without explicit registration via RegisterClassA.
/// </summary>
[Trait("Category", "DllModuleTests")]
public class SystemWindowClassesTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public SystemWindowClassesTests()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv?.Dispose();
	}

	/// <summary>
	/// Verifies that all standard system window classes are pre-registered.
	/// According to Microsoft documentation, these classes should be available for all processes:
	/// https://learn.microsoft.com/en-us/windows/win32/winmsg/about-window-classes#system-classes
	/// </summary>
	[Theory]
	[InlineData("BUTTON")]
	[InlineData("EDIT")]
	[InlineData("STATIC")]
	[InlineData("LISTBOX")]
	[InlineData("COMBOBOX")]
	[InlineData("SCROLLBAR")]
	[InlineData("MDICLIENT")]
	public void SystemWindowClass_ShouldBePreRegistered(string className)
	{
		// Act - Verify class is registered without calling RegisterClassA
		var isRegistered = _testEnv.ProcessEnv.IsWindowClassRegistered(className);

		// Assert
		Assert.True(isRegistered, $"System window class '{className}' should be pre-registered");
	}

	/// <summary>
	/// Verifies that windows can be created using system window classes without explicit registration.
	/// </summary>
	[Theory]
	[InlineData("BUTTON", "Test Button")]
	[InlineData("EDIT", "Test Edit")]
	[InlineData("STATIC", "Test Static")]
	[InlineData("LISTBOX", "Test ListBox")]
	[InlineData("COMBOBOX", "Test ComboBox")]
	[InlineData("SCROLLBAR", "Test ScrollBar")]
	[InlineData("MDICLIENT", "Test MDIClient")]
	public void CreateWindowExA_WithSystemClass_ShouldSucceedWithoutRegistration(string className, string windowName)
	{
		// Arrange
		var classNamePtr = _testEnv.WriteString(className);
		var windowNamePtr = _testEnv.WriteString(windowName);

		// Act - Create window without calling RegisterClassA first
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,                  // dwExStyle
			classNamePtr,       // lpClassName
			windowNamePtr,      // lpWindowName
			0x00000000,         // dwStyle (WS_OVERLAPPED)
			0,                  // x
			0,                  // y
			100,                // width
			100,                // height
			0,                  // hWndParent
			0,                  // hMenu
			0,                  // hInstance
			0                   // lpParam
		);

		// Assert
		Assert.NotEqual(0u, hwnd);

		// Verify window info
		var windowInfo = _testEnv.ProcessEnv.GetWindow(hwnd);
		Assert.NotNull(windowInfo);
		var info = windowInfo.Value;
		Assert.Equal(className, info.ClassName);
		Assert.Equal(windowName, info.WindowName);
	}

	/// <summary>
	/// Verifies that system window classes have valid window procedure addresses.
	/// </summary>
	[Theory]
	[InlineData("BUTTON")]
	[InlineData("EDIT")]
	[InlineData("STATIC")]
	[InlineData("LISTBOX")]
	[InlineData("COMBOBOX")]
	[InlineData("SCROLLBAR")]
	[InlineData("MDICLIENT")]
	public void SystemWindowClass_ShouldHaveValidWindowProcedure(string className)
	{
		// Act
		var classInfo = _testEnv.ProcessEnv.GetWindowClass(className);

		// Assert
		Assert.NotNull(classInfo);
		var info = classInfo.Value;
		Assert.NotEqual(0u, info.WndProc);
		
		// Verify it's a standard control window procedure marker
		Assert.True(ProcessEnvironment.IsStandardControlWndProc(info.WndProc),
			$"Window procedure for '{className}' should be a standard control marker");
	}

	/// <summary>
	/// Verifies that all system window classes are case-insensitive.
	/// Windows accepts class names in any case and stores them as provided.
	/// </summary>
	[Theory]
	[InlineData("button")]
	[InlineData("Button")]
	[InlineData("BUTTON")]
	[InlineData("edit")]
	[InlineData("EDIT")]
	[InlineData("static")]
	[InlineData("listbox")]
	[InlineData("combobox")]
	[InlineData("scrollbar")]
	[InlineData("mdiclient")]
	public void SystemWindowClass_ShouldBeCaseInsensitive(string inputClassName)
	{
		// Arrange
		var classNamePtr = _testEnv.WriteString(inputClassName);
		var windowNamePtr = _testEnv.WriteString("Test Window");

		// Act - Create window with different case
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,                  // dwExStyle
			classNamePtr,       // lpClassName
			windowNamePtr,      // lpWindowName
			0x00000000,         // dwStyle
			0, 0, 100, 100,
			0, 0, 0, 0
		);

		// Assert - Window should be created successfully regardless of case
		Assert.NotEqual(0u, hwnd);

		var windowInfo = _testEnv.ProcessEnv.GetWindow(hwnd);
		Assert.NotNull(windowInfo);
		// Class name is stored as provided (preserving the input case)
		var nonNullWindowInfo = windowInfo.Value;
		Assert.Equal(inputClassName, nonNullWindowInfo.ClassName);
	}

	/// <summary>
	/// Verifies that a BUTTON control can be created as a child window.
	/// This is a common use case for buttons in dialogs and forms.
	/// </summary>
	[Fact]
	public void Button_CanBeCreatedAsChildWindow()
	{
		// Arrange - Create parent window first
		var parentClassPtr = _testEnv.WriteString("ParentClass");
		var parentTitlePtr = _testEnv.WriteString("Parent");
		
		// Register parent class
		var wndClassAddr = _testEnv.WriteWndClassA(className: "ParentClass", wndProc: 0x00400000);
		var parentAtom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);
		Assert.NotEqual(0u, parentAtom);

		var parentHwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0, parentClassPtr, parentTitlePtr,
			(uint)NativeTypes.WindowStyle.WS_OVERLAPPEDWINDOW,
			0, 0, 400, 300,
			0, 0, 0, 0
		);
		Assert.NotEqual(0u, parentHwnd);

		// Act - Create button as child
		var buttonClassPtr = _testEnv.WriteString("BUTTON");
		var buttonTextPtr = _testEnv.WriteString("Click Me");

		var buttonHwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0,                                          // dwExStyle
			buttonClassPtr,                             // lpClassName
			buttonTextPtr,                              // lpWindowName
			(uint)NativeTypes.WindowStyle.WS_CHILD |         // WS_CHILD
			(uint)NativeTypes.WindowStyle.WS_VISIBLE,        // WS_VISIBLE
			10, 10, 100, 30,                           // position and size
			parentHwnd,                                 // parent window
			1,                                          // control ID
			0, 0
		);

		// Assert
		Assert.NotEqual(0u, buttonHwnd);

		var buttonInfo = _testEnv.ProcessEnv.GetWindow(buttonHwnd);
		Assert.NotNull(buttonInfo);
		Assert.Equal("BUTTON", buttonInfo!.Value.ClassName);
		Assert.Equal("Click Me", buttonInfo!.Value.WindowName);
		Assert.Equal(parentHwnd, buttonInfo!.Value.Parent);
	}

	/// <summary>
	/// Verifies that attempting to re-register a system window class fails.
	/// System classes cannot be overridden by applications.
	/// </summary>
	[Fact]
	public void RegisterClassA_WithSystemClassName_ShouldFail()
	{
		// Arrange - Try to register BUTTON class
		var wndClassAddr = _testEnv.WriteWndClassA(className: "BUTTON", wndProc: 0x00400000);

		// Act - Attempt to register
		var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

		// Assert - Should fail because BUTTON is already registered
		Assert.Equal(0u, atom);
	}
}
