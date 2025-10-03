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

    [Fact]
    public void ClientToScreen_WithValidPoint_ShouldReturnTrue()
    {
        // Arrange
        uint hwnd = 0x00010000;
        uint lpPoint = _testEnv.AllocateMemory(8); // POINT structure
        _testEnv.Memory.Write32(lpPoint, 10);     // x
        _testEnv.Memory.Write32(lpPoint + 4, 20); // y

        // Act
        var result = _testEnv.CallUser32Api("CLIENTTOSCREEN", hwnd, lpPoint);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void SetRect_ShouldInitializeRectCorrectly()
    {
        // Arrange
        uint lpRect = _testEnv.AllocateMemory(16); // RECT structure

        // Act
        var result = _testEnv.CallUser32Api("SETRECT", lpRect, 10u, 20u, 100u, 200u);

        // Assert
        Assert.Equal(1u, result); // TRUE
        Assert.Equal(10u, _testEnv.Memory.Read32(lpRect));      // left
        Assert.Equal(20u, _testEnv.Memory.Read32(lpRect + 4));  // top
        Assert.Equal(100u, _testEnv.Memory.Read32(lpRect + 8)); // right
        Assert.Equal(200u, _testEnv.Memory.Read32(lpRect + 12)); // bottom
    }

    [Fact]
    public void GetClientRect_ShouldReturnDefaultRect()
    {
        // Arrange
        uint hwnd = 0x00010000;
        uint lpRect = _testEnv.AllocateMemory(16);

        // Act
        var result = _testEnv.CallUser32Api("GETCLIENTRECT", hwnd, lpRect);

        // Assert
        Assert.Equal(1u, result); // TRUE
        Assert.Equal(0u, _testEnv.Memory.Read32(lpRect));      // left = 0
        Assert.Equal(0u, _testEnv.Memory.Read32(lpRect + 4));  // top = 0
        Assert.Equal(640u, _testEnv.Memory.Read32(lpRect + 8)); // right = 640
        Assert.Equal(480u, _testEnv.Memory.Read32(lpRect + 12)); // bottom = 480
    }

    [Fact]
    public void GetWindowRect_ShouldReturnDefaultRect()
    {
        // Arrange
        uint hwnd = 0x00010000;
        uint lpRect = _testEnv.AllocateMemory(16);

        // Act
        var result = _testEnv.CallUser32Api("GETWINDOWRECT", hwnd, lpRect);

        // Assert
        Assert.Equal(1u, result); // TRUE
        Assert.NotEqual(0u, _testEnv.Memory.Read32(lpRect)); // Has non-zero values
    }

    [Fact]
    public void GetDC_ShouldReturnValidHandle()
    {
        // Arrange
        uint hwnd = 0x00010000;

        // Act
        var hdc = _testEnv.CallUser32Api("GETDC", hwnd);

        // Assert
        Assert.NotEqual(0u, hdc);
    }

    [Fact]
    public void ReleaseDC_ShouldReturnSuccess()
    {
        // Arrange
        uint hwnd = 0x00010000;
        var hdc = _testEnv.CallUser32Api("GETDC", hwnd);

        // Act
        var result = _testEnv.CallUser32Api("RELEASEDC", hwnd, hdc);

        // Assert
        Assert.Equal(1u, result); // Success
    }

    [Fact]
    public void GetSystemMetrics_ScreenWidth_ShouldReturnValue()
    {
        // Act - SM_CXSCREEN = 0
        var width = _testEnv.CallUser32Api("GETSYSTEMMETRICS", 0);

        // Assert
        Assert.True(width > 0);
    }

    [Fact]
    public void LoadIconA_ShouldReturnHandle()
    {
        // Act
        var hIcon = _testEnv.CallUser32Api("LOADICONA", 0u, 0u);

        // Assert
        Assert.NotEqual(0u, hIcon);
    }

    [Fact]
    public void LoadCursorA_ShouldReturnHandle()
    {
        // Act
        var hCursor = _testEnv.CallUser32Api("LOADCURSORA", 0u, 0u);

        // Assert
        Assert.NotEqual(0u, hCursor);
    }

    [Fact]
    public void UpdateWindow_ShouldReturnTrue()
    {
        // Arrange
        uint hwnd = 0x00010000;

        // Act
        var result = _testEnv.CallUser32Api("UPDATEWINDOW", hwnd);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void PeekMessageA_ShouldReturnZeroWhenNoMessage()
    {
        // Arrange
        uint lpMsg = _testEnv.AllocateMemory(28); // MSG structure

        // Act
        var result = _testEnv.CallUser32Api("PEEKMESSAGEA", lpMsg, 0u, 0u, 0u, 0u);

        // Assert
        Assert.Equal(0u, result); // No message available
    }

    [Fact]
    public void PostMessageA_ShouldReturnTrue()
    {
        // Arrange
        uint hwnd = 0x00010000;

        // Act
        var result = _testEnv.CallUser32Api("POSTMESSAGEA", hwnd, 0x0100u, 0u, 0u);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void MessageBoxA_ShouldReturnOK()
    {
        // Arrange
        uint lpText = _testEnv.WriteString("Test message");
        uint lpCaption = _testEnv.WriteString("Test caption");

        // Act
        var result = _testEnv.CallUser32Api("MESSAGEBOXA", 0u, lpText, lpCaption, 0u);

        // Assert
        Assert.Equal(1u, result); // IDOK
    }

    [Fact]
    public void CreateWindowExA_ShouldInvokeHostCallback()
    {
        // Arrange - Create a mock host to track callbacks
        var mockHost = new MockEmulatorHost();
        var testEnvWithHost = new TestEnvironment(mockHost);
        
        var wndClassAddr = testEnvWithHost.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        testEnvWithHost.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = testEnvWithHost.WriteString("TestClass");
        var titlePtr = testEnvWithHost.WriteString("Test Window");

        // Act
        var hwnd = testEnvWithHost.CallUser32Api("CREATEWINDOWEXA",
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
        Assert.True(mockHost.OnWindowCreateCalled, "OnWindowCreate should have been called");
        Assert.NotNull(mockHost.LastWindowInfo);
        Assert.Equal("Test Window", mockHost.LastWindowInfo?.Title);
        Assert.Equal(640, mockHost.LastWindowInfo?.Width);
        Assert.Equal(480, mockHost.LastWindowInfo?.Height);
        Assert.Equal("TestClass", mockHost.LastWindowInfo?.ClassName);
        
        testEnvWithHost.Dispose();
    }

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}

/// <summary>
/// Mock implementation of IEmulatorHost for testing
/// </summary>
internal class MockEmulatorHost : IEmulatorHost
{
    public bool OnWindowCreateCalled { get; private set; }
    public WindowCreateInfo? LastWindowInfo { get; private set; }
    
    public void OnDebugOutput(string message, DebugLevel level)
    {
        // No-op for testing
    }
    
    public void OnStdOutput(string output)
    {
        // No-op for testing
    }
    
    public void OnWindowCreate(WindowCreateInfo info)
    {
        OnWindowCreateCalled = true;
        LastWindowInfo = info;
    }
}
