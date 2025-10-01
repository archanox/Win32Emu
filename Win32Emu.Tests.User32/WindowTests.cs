using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for User32 window management functions
/// </summary>
public class WindowTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public WindowTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void RegisterClassA_WithValidParameters_ShouldReturnAtom()
    {
        // Arrange
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );

        // Act
        var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        // Assert
        Assert.NotEqual(0u, atom);
    }

    [Fact]
    public void RegisterClassA_WithNullClassName_ShouldReturnZero()
    {
        // Arrange
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: null,
            wndProc: 0x00401000
        );

        // Act
        var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        // Assert
        Assert.Equal(0u, atom);
    }

    [Fact]
    public void RegisterClassA_WithNullPointer_ShouldReturnZero()
    {
        // Act
        var atom = _testEnv.CallUser32Api("REGISTERCLASSA", 0);

        // Assert
        Assert.Equal(0u, atom);
    }

    [Fact]
    public void RegisterClassA_SameClassTwice_ShouldFailSecondTime()
    {
        // Arrange
        var wndClassAddr1 = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        var wndClassAddr2 = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );

        // Act
        var atom1 = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr1);
        var atom2 = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr2);

        // Assert
        Assert.NotEqual(0u, atom1);
        Assert.Equal(0u, atom2); // Second registration should fail
    }

    [Fact]
    public void CreateWindowExA_WithRegisteredClass_ShouldReturnHandle()
    {
        // Arrange
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        // Act
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0,              // dwExStyle
            classNamePtr,   // lpClassName
            titlePtr,       // lpWindowName
            NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
            100,            // x
            100,            // y
            640,            // width
            480,            // height
            0,              // hWndParent
            0,              // hMenu
            0,              // hInstance
            0               // lpParam
        );

        // Assert
        Assert.NotEqual(0u, hwnd);
    }

    [Fact]
    public void CreateWindowExA_WithUnregisteredClass_ShouldReturnZero()
    {
        // Arrange
        var classNamePtr = _testEnv.WriteString("UnregisteredClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        // Act
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0,              // dwExStyle
            classNamePtr,   // lpClassName
            titlePtr,       // lpWindowName
            NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
            100,            // x
            100,            // y
            640,            // width
            480,            // height
            0,              // hWndParent
            0,              // hMenu
            0,              // hInstance
            0               // lpParam
        );

        // Assert
        Assert.Equal(0u, hwnd);
    }

    [Fact]
    public void CreateWindowExA_WithNullClassName_ShouldReturnZero()
    {
        // Act
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0,              // dwExStyle
            0,              // lpClassName (NULL)
            0,              // lpWindowName
            NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
            100,            // x
            100,            // y
            640,            // width
            480,            // height
            0,              // hWndParent
            0,              // hMenu
            0,              // hInstance
            0               // lpParam
        );

        // Assert
        Assert.Equal(0u, hwnd);
    }

    [Fact]
    public void CreateWindowExA_MultipleTimes_ShouldReturnDifferentHandles()
    {
        // Arrange
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        // Act
        var hwnd1 = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );
        var hwnd2 = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        // Assert
        Assert.NotEqual(0u, hwnd1);
        Assert.NotEqual(0u, hwnd2);
        Assert.NotEqual(hwnd1, hwnd2); // Different windows should have different handles
    }

    [Fact]
    public void CreateWindowExA_WithCwUseDefault_ShouldUseDefaultValues()
    {
        // Arrange
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        const uint CW_USEDEFAULT = 0x80000000;

        // Act - using CW_USEDEFAULT should still create a valid window
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0,              // dwExStyle
            classNamePtr,   // lpClassName
            titlePtr,       // lpWindowName
            NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
            CW_USEDEFAULT,  // x
            CW_USEDEFAULT,  // y
            CW_USEDEFAULT,  // width
            CW_USEDEFAULT,  // height
            0,              // hWndParent
            0,              // hMenu
            0,              // hInstance
            0               // lpParam
        );

        // Assert
        Assert.NotEqual(0u, hwnd);
    }

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}
