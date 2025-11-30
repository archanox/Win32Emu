using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for User32 window management functions
/// </summary>
[Trait("Category", "DllModuleTests")]
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
            (uint)NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
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
            (uint)NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
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
            (uint)NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
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
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );
        var hwnd2 = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
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

        const uint cwUsedefault = 0x80000000;

        // Act - using CW_USEDEFAULT should still create a valid window
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0,              // dwExStyle
            classNamePtr,   // lpClassName
            titlePtr,       // lpWindowName
            (uint)NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
            cwUsedefault,  // x
            cwUsedefault,  // y
            cwUsedefault,  // width
            cwUsedefault,  // height
            0,              // hWndParent
            0,              // hMenu
            0,              // hInstance
            0               // lpParam
        );

        // Assert
        Assert.NotEqual(0u, hwnd);
    }

    [Fact]
    public void CreateWindowExA_WithAtom_ShouldResolveClassName()
    {
        // Arrange - Register a window class and get its atom
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "AtomTestClass",
            wndProc: 0x00401000
        );
        var atom = _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);
        Assert.NotEqual(0u, atom);

        var titlePtr = _testEnv.WriteString("Test Window");

        // Act - Use the atom instead of a string pointer for the class name
        // When HIWORD(lpClassName) == 0, it's treated as an atom
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0,              // dwExStyle
            atom,           // lpClassName (using atom instead of string pointer)
            titlePtr,       // lpWindowName
            (uint)NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
            100,            // x
            100,            // y
            640,            // width
            480,            // height
            0,              // hWndParent
            0,              // hMenu
            0,              // hInstance
            0               // lpParam
        );

        // Assert - Window should be created successfully using the atom
        Assert.NotEqual(0u, hwnd);
    }

    [Fact]
    public void ClientToScreen_WithValidPoint_ShouldReturnTrue()
    {
        // Arrange
        uint hwnd = 0x00010000;
        var lpPoint = _testEnv.AllocateMemory(8); // POINT structure
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
        var lpRect = _testEnv.AllocateMemory(16); // RECT structure

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
        var lpRect = _testEnv.AllocateMemory(16);

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
        var lpRect = _testEnv.AllocateMemory(16);

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
    public void UpdateWindow_WithValidWindow_ShouldReturnTrue()
    {
        // Arrange - Create a valid window first
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");
        
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        // Act
        var result = _testEnv.CallUser32Api("UPDATEWINDOW", hwnd);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void UpdateWindow_WithInvalidWindow_ShouldReturnFalse()
    {
        // Arrange - Use an invalid window handle
        uint invalidHwnd = 0x99999999;

        // Act
        var result = _testEnv.CallUser32Api("UPDATEWINDOW", invalidHwnd);

        // Assert
        Assert.Equal(0u, result); // FALSE for invalid window
    }

    [Fact]
    public void SetWindowLongA_ShouldStoreValue()
    {
        // Arrange - Create a window
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");
        
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        // Act - Set user data (GWL_USERDATA = -21)
        var previousValue = _testEnv.CallUser32Api("SETWINDOWLONGA", hwnd, unchecked((uint)-21), 0x12345678u);

        // Assert
        Assert.Equal(0u, previousValue); // Should return 0 (no previous value)
    }

    [Fact]
    public void GetWindowLongA_ShouldRetrieveValue()
    {
        // Arrange - Create a window and set a value
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");
        
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        _testEnv.CallUser32Api("SETWINDOWLONGA", hwnd, unchecked((uint)-21), 0x12345678u);

        // Act - Get user data (GWL_USERDATA = -21)
        var value = _testEnv.CallUser32Api("GETWINDOWLONGA", hwnd, unchecked((uint)-21));

        // Assert
        Assert.Equal(0x12345678u, value);
    }

    [Fact]
    public void SetWindowLongA_ShouldReturnPreviousValue()
    {
        // Arrange
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");
        
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        // Set initial value
        _testEnv.CallUser32Api("SETWINDOWLONGA", hwnd, unchecked((uint)-21), 0x11111111u);

        // Act - Set a new value
        var previousValue = _testEnv.CallUser32Api("SETWINDOWLONGA", hwnd, unchecked((uint)-21), 0x22222222u);

        // Assert
        Assert.Equal(0x11111111u, previousValue);
        
        // Verify new value is set
        var currentValue = _testEnv.CallUser32Api("GETWINDOWLONGA", hwnd, unchecked((uint)-21));
        Assert.Equal(0x22222222u, currentValue);
    }

    [Fact]
    public void GetWindowLongA_Style_ShouldReturnWindowStyle()
    {
        // Arrange - Create a window with specific style
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");
        
        var testStyle = (uint)NativeTypes.WindowStyle.WS_OVERLAPPED | (uint)NativeTypes.WindowStyle.WS_CAPTION;
        
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, testStyle,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        // Act - Get style (GWL_STYLE = -16)
        var style = _testEnv.CallUser32Api("GETWINDOWLONGA", hwnd, unchecked((uint)-16));

        // Assert
        Assert.Equal(testStyle, style);
    }

    [Fact]
    public void PeekMessageA_ShouldReturnZeroWhenNoMessage()
    {
        // Arrange
        var lpMsg = _testEnv.AllocateMemory(28); // MSG structure

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
        var lpText = _testEnv.WriteString("Test message");
        var lpCaption = _testEnv.WriteString("Test caption");

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
            (uint)NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
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

    [Fact]
    public void ShowCursor_WithTrueParameter_ShouldReturnPositiveValue()
    {
        // Act
        var result = _testEnv.CallUser32Api("SHOWCURSOR", 1);

        // Assert
        // ShowCursor returns the new display count, which should be >= 0
        Assert.True((int)result >= 0);
    }

    [Fact]
    public void ShowCursor_WithFalseParameter_ShouldReturnZero()
    {
        // Act
        var result = _testEnv.CallUser32Api("SHOWCURSOR", 0);

        // Assert
        Assert.Equal(0, (int)result);
    }

    [Fact]
    public void RegisterWindowMessageA_WithValidString_ShouldReturnMessageId()
    {
        // Arrange
        var messageNamePtr = _testEnv.WriteString("MyCustomMessage");

        // Act
        var messageId = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", messageNamePtr);

        // Assert
        Assert.NotEqual(0u, messageId);
        Assert.True(messageId >= 0xC000 && messageId <= 0xFFFF, 
            $"Message ID should be in range 0xC000-0xFFFF, but was 0x{messageId:X4}");
    }

    [Fact]
    public void RegisterWindowMessageA_SameMessageTwice_ShouldReturnSameId()
    {
        // Arrange
        var messageNamePtr1 = _testEnv.WriteString("MyCustomMessage");
        var messageNamePtr2 = _testEnv.WriteString("MyCustomMessage");

        // Act
        var messageId1 = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", messageNamePtr1);
        var messageId2 = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", messageNamePtr2);

        // Assert
        Assert.NotEqual(0u, messageId1);
        Assert.Equal(messageId1, messageId2); // Same message name should return same ID
    }

    [Fact]
    public void RegisterWindowMessageA_DifferentMessages_ShouldReturnDifferentIds()
    {
        // Arrange
        var messageNamePtr1 = _testEnv.WriteString("FirstMessage");
        var messageNamePtr2 = _testEnv.WriteString("SecondMessage");

        // Act
        var messageId1 = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", messageNamePtr1);
        var messageId2 = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", messageNamePtr2);

        // Assert
        Assert.NotEqual(0u, messageId1);
        Assert.NotEqual(0u, messageId2);
        Assert.NotEqual(messageId1, messageId2); // Different messages should have different IDs
    }

    [Fact]
    public void RegisterWindowMessageA_WithNullPointer_ShouldReturnZero()
    {
        // Act
        var messageId = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", 0);

        // Assert
        Assert.Equal(0u, messageId);
    }

    [Fact]
    public void RegisterWindowMessageA_WithEmptyString_ShouldReturnZero()
    {
        // Arrange
        var messageNamePtr = _testEnv.WriteString("");

        // Act
        var messageId = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", messageNamePtr);

        // Assert
        Assert.Equal(0u, messageId);
    }

    [Fact]
    public void RegisterWindowMessageA_CaseInsensitive_ShouldReturnSameId()
    {
        // Arrange
        var messageNamePtr1 = _testEnv.WriteString("MyMessage");
        var messageNamePtr2 = _testEnv.WriteString("MYMESSAGE");
        var messageNamePtr3 = _testEnv.WriteString("mymessage");

        // Act
        var messageId1 = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", messageNamePtr1);
        var messageId2 = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", messageNamePtr2);
        var messageId3 = _testEnv.CallUser32Api("REGISTERWINDOWMESSAGEA", messageNamePtr3);

        // Assert
        Assert.NotEqual(0u, messageId1);
        Assert.Equal(messageId1, messageId2); // Same message name (different case) should return same ID
        Assert.Equal(messageId1, messageId3); // Same message name (different case) should return same ID
    }

    [Fact]
    public void CreateWindowExA_ShouldSendWmCreateWmSizeAndWmMove()
    {
        // Arrange
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        const int x = 100;
        const int y = 150;
        const int width = 640;
        const int height = 480;

        // Act
        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0,              // dwExStyle
            classNamePtr,   // lpClassName
            titlePtr,       // lpWindowName
            (uint)NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle
            x,              // x
            y,              // y
            width,          // width
            height,         // height
            0,              // hWndParent
            0,              // hMenu
            0,              // hInstance
            0               // lpParam
        );

        // Assert - window was created
        Assert.NotEqual(0u, hwnd);

        // Assert - WM_CREATE message should be queued
        var msgAddr1 = _testEnv.AllocateMemory(28); // MSG structure size
        var result1 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr1, 0, 0, 0, 0x0001); // PM_REMOVE
        Assert.Equal(1u, result1);
        var msg1 = _testEnv.Memory.Read32(msgAddr1 + 4);
        Assert.Equal(0x0001u, msg1); // WM_CREATE

        // Assert - WM_SIZE message should be queued
        var msgAddr2 = _testEnv.AllocateMemory(28); // MSG structure size
        var result2 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr2, 0, 0, 0, 0x0001); // PM_REMOVE
        Assert.Equal(1u, result2);
        var msg2 = _testEnv.Memory.Read32(msgAddr2 + 4);
        var wParam2 = _testEnv.Memory.Read32(msgAddr2 + 8);
        var lParam2 = _testEnv.Memory.Read32(msgAddr2 + 12);
        Assert.Equal(0x0005u, msg2); // WM_SIZE
        Assert.Equal(0u, wParam2); // SIZE_RESTORED
        Assert.Equal((uint)((height << 16) | (width & 0xFFFF)), lParam2); // MAKELONG(width, height)

        // Assert - WM_MOVE message should be queued
        var msgAddr3 = _testEnv.AllocateMemory(28); // MSG structure size
        var result3 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr3, 0, 0, 0, 0x0001); // PM_REMOVE
        Assert.Equal(1u, result3);
        var msg3 = _testEnv.Memory.Read32(msgAddr3 + 4);
        var wParam3 = _testEnv.Memory.Read32(msgAddr3 + 8);
        var lParam3 = _testEnv.Memory.Read32(msgAddr3 + 12);
        Assert.Equal(0x0003u, msg3); // WM_MOVE
        Assert.Equal(0u, wParam3);
        Assert.Equal((uint)((y << 16) | (x & 0xFFFF)), lParam3); // MAKELONG(x, y)
    }

    [Fact]
    public void ShowWindow_ShouldSendWmActivateAppWhenWindowBecomesVisible()
    {
        // Arrange - create a window
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0,              // dwExStyle
            classNamePtr,   // lpClassName
            titlePtr,       // lpWindowName
            (uint)NativeTypes.WindowStyle.WS_OVERLAPPED, // dwStyle (not visible initially)
            100,            // x
            100,            // y
            640,            // width
            480,            // height
            0,              // hWndParent
            0,              // hMenu
            0,              // hInstance
            0               // lpParam
        );

        // Clear the message queue (WM_CREATE, WM_SIZE, WM_MOVE)
        var dummyMsg = _testEnv.AllocateMemory(28);
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE

        // Act - show the window
        _testEnv.CallUser32Api("SHOWWINDOW", hwnd, 1); // SW_SHOWNORMAL

        // Assert - WM_ACTIVATEAPP message should be queued
        var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size
        var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0001); // PM_REMOVE
        Assert.Equal(1u, result);
        var retrievedHwnd = _testEnv.Memory.Read32(msgAddr + 0);
        var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
        var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);
        var retrievedLParam = _testEnv.Memory.Read32(msgAddr + 12);
        Assert.Equal(hwnd, retrievedHwnd);
        Assert.Equal(0x001Cu, retrievedMsg); // WM_ACTIVATEAPP
        Assert.Equal(1u, retrievedWParam); // TRUE - window is being activated
        Assert.Equal(0u, retrievedLParam); // Thread ID (0 for simplicity)
    }

    [Fact]
    public void ShowWindow_ShouldSendWmActivateAppWhenWindowBecomesHidden()
    {
        // Arrange - create a visible window
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0,              // dwExStyle
            classNamePtr,   // lpClassName
            titlePtr,       // lpWindowName
            (uint)NativeTypes.WindowStyle.WS_OVERLAPPED | (uint)NativeTypes.WindowStyle.WS_VISIBLE, // dwStyle - visible
            100,            // x
            100,            // y
            640,            // width
            480,            // height
            0,              // hWndParent
            0,              // hMenu
            0,              // hInstance
            0               // lpParam
        );

        // Clear the message queue (WM_CREATE, WM_SIZE, WM_MOVE)
        var dummyMsg = _testEnv.AllocateMemory(28);
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE

        // Act - hide the window
        _testEnv.CallUser32Api("SHOWWINDOW", hwnd, 0); // SW_HIDE

        // Assert - WM_ACTIVATEAPP message should be queued
        var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size
        var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0001); // PM_REMOVE
        Assert.Equal(1u, result);
        var retrievedHwnd = _testEnv.Memory.Read32(msgAddr + 0);
        var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
        var retrievedWParam = _testEnv.Memory.Read32(msgAddr + 8);
        var retrievedLParam = _testEnv.Memory.Read32(msgAddr + 12);
        Assert.Equal(hwnd, retrievedHwnd);
        Assert.Equal(0x001Cu, retrievedMsg); // WM_ACTIVATEAPP
        Assert.Equal(0u, retrievedWParam); // FALSE - window is being deactivated
        Assert.Equal(0u, retrievedLParam); // Thread ID (0 for simplicity)
    }

    [Fact]
    public void DestroyWindow_WithValidWindow_ShouldReturnTrue()
    {
        // Arrange - Create a window first
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        // Act
        var result = _testEnv.CallUser32Api("DESTROYWINDOW", hwnd);

        // Assert
        Assert.Equal(1u, result); // TRUE - window destroyed successfully
    }

    [Fact]
    public void DestroyWindow_WithInvalidWindow_ShouldReturnFalse()
    {
        // Arrange - Use an invalid window handle
        uint invalidHwnd = 0x99999999;

        // Act
        var result = _testEnv.CallUser32Api("DESTROYWINDOW", invalidHwnd);

        // Assert
        Assert.Equal(0u, result); // FALSE - invalid window
    }

    [Fact]
    public void DestroyWindow_ShouldSendWmDestroyMessage()
    {
        // Arrange - Create a window
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        // Clear the message queue (WM_CREATE, WM_SIZE, WM_MOVE)
        var dummyMsg = _testEnv.AllocateMemory(28);
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE

        // Act - Destroy the window
        _testEnv.CallUser32Api("DESTROYWINDOW", hwnd);

        // Assert - WM_DESTROY message should be queued
        var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size
        var result = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0001); // PM_REMOVE
        Assert.Equal(1u, result); // Message should be available
        var retrievedHwnd = _testEnv.Memory.Read32(msgAddr + 0);
        var retrievedMsg = _testEnv.Memory.Read32(msgAddr + 4);
        Assert.Equal(hwnd, retrievedHwnd);
        Assert.Equal(0x0002u, retrievedMsg); // WM_DESTROY
    }

    [Fact]
    public void DestroyWindow_ShouldSendWmNcDestroyMessage()
    {
        // Arrange - Create a window
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        // Clear the message queue (WM_CREATE, WM_SIZE, WM_MOVE)
        var dummyMsg = _testEnv.AllocateMemory(28);
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE
        _testEnv.CallUser32Api("PEEKMESSAGEA", dummyMsg, 0, 0, 0, 0x0001); // PM_REMOVE

        // Act - Destroy the window
        _testEnv.CallUser32Api("DESTROYWINDOW", hwnd);

        // Assert - Both WM_DESTROY and WM_NCDESTROY messages should be queued
        var msgAddr = _testEnv.AllocateMemory(28); // MSG structure size
        
        // First message should be WM_DESTROY
        var result1 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0001); // PM_REMOVE
        Assert.Equal(1u, result1);
        var msg1 = _testEnv.Memory.Read32(msgAddr + 4);
        Assert.Equal(0x0002u, msg1); // WM_DESTROY

        // Second message should be WM_NCDESTROY
        var result2 = _testEnv.CallUser32Api("PEEKMESSAGEA", msgAddr, 0, 0, 0, 0x0001); // PM_REMOVE
        Assert.Equal(1u, result2);
        var msg2 = _testEnv.Memory.Read32(msgAddr + 4);
        Assert.Equal(0x0082u, msg2); // WM_NCDESTROY
    }

    [Fact]
    public void DestroyWindow_CalledTwice_ShouldFailSecondTime()
    {
        // Arrange - Create and destroy a window
        var wndClassAddr = _testEnv.WriteWndClassA(
            className: "TestClass",
            wndProc: 0x00401000
        );
        _testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

        var classNamePtr = _testEnv.WriteString("TestClass");
        var titlePtr = _testEnv.WriteString("Test Window");

        var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
            0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
            100, 100, 640, 480, 0, 0, 0, 0
        );

        var result1 = _testEnv.CallUser32Api("DESTROYWINDOW", hwnd);

        // Act - Try to destroy the same window again
        var result2 = _testEnv.CallUser32Api("DESTROYWINDOW", hwnd);

        // Assert
        Assert.Equal(1u, result1); // First destroy should succeed
        Assert.Equal(0u, result2); // Second destroy should fail
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

    public Task<int> OnDialogCreate(DialogCreateInfo info) => Task.FromResult(2);
    
    public void OnDialogEnd(uint dialogHandle, int result)
    {
        // Mock implementation - no-op
    }

    public int OnMessageBox(MessageBoxInfo info)
    {
        // Mock implementation - return IDOK
        return 1;
    }

    public void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text) { }
    public void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData) { }
    public void OnDialogControlEnabledChanged(uint dialogHandle, int controlId, bool enabled) { }
    public void OnDisplayUpdate(DisplayUpdateInfo info) { }
    public Task<string?> OnBrowseForFolder(string? title, string? rootPath) => Task.FromResult<string?>(null);
		public Task<string?> OnOpenFileDialog(string? title, string? filter, string? initialDirectory) => Task.FromResult<string?>(null);
		public Task<string?> OnSaveFileDialog(string? title, string? filter, string? initialDirectory) => Task.FromResult<string?>(null);
    public void OnWindowTitleChanged(uint windowHandle, string title) { }
public void OnControlVisibilityChanged(uint dialogHandle, int controlId, bool visible) { }
}
