# ReactOS Test Porting Guide

This guide explains how to port ReactOS User32/Kernel32 API tests to xUnit tests in Win32Emu.

## Overview

ReactOS provides comprehensive Win32 API tests written in C using the Wine Test Framework. These tests can be ported to C# xUnit tests to provide:

1. **Native C# test experience** - Better IDE integration and debugging
2. **Direct API testing** - No PE loading overhead
3. **Documentation** - Tests show expected API behavior
4. **Regression prevention** - Catch breaks in API implementations

## Test Status

**Status**: Optional (non-blocking in CI)

Ported tests are marked with:
- `[Trait("Category", "DllModuleTests")]` - Optional in CI
- `[Trait("Source", "ReactOS")]` - Tracks test origin

Test failures document current implementation differences and are expected. See [README.Tests.md](../../README.Tests.md) for CI policy.

## Porting Process

### 1. Find ReactOS Test Source

ReactOS tests are at: https://github.com/reactos/reactos/tree/master/modules/rostests/apitests/user32

Example test files:
- `GetPeekMessage.c` - Simple message queue tests
- `SetScrollRange.c` - Scroll range validation
- `GetSetWindowInt.c` - Window extra data tests
- `GetClassInfo.c` - Window class tests

### 2. Analyze Test Structure

ReactOS tests use Wine Test Framework macros:

```c
START_TEST(GetPeekMessage)
{
    HWND hWnd = CreateWindowExW(...);
    ok(hWnd != INVALID_HANDLE_VALUE, "\n");
    ok(DestroyWindow(hWnd), "\n");
    
    SetLastError(DNS_ERROR_RCODE_NXRRSET);
    ok(GetMessage(&msg, hWnd, 0, 0) == -1, "\n");
    ok(GetLastError() == ERROR_INVALID_WINDOW_HANDLE, "...");
}
```

Key macros:
- `ok(condition, message)` → `Assert.Equal/NotEqual()`
- `SetLastError(value)` → `_testEnv.CallKernel32Api("SETLASTERROR", value)`
- `GetLastError()` → `_testEnv.CallKernel32Api("GETLASTERROR")`

### 3. Create C# Test Method

Convert to xUnit pattern:

```csharp
[Fact]
public void GetMessage_WithInvalidWindowHandle_ShouldReturnMinusOne()
{
    // Arrange - Create and destroy window to get invalid handle
    var classNamePtr = _testEnv.WriteString("EDIT");
    var titlePtr = _testEnv.WriteString("test");
    
    var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
        0, classNamePtr, titlePtr, 0x00000000,
        0, 0, 0, 0, 0, 0, 0x00400000, 0
    );
    
    _testEnv.CallUser32Api("DESTROYWINDOW", hwnd);
    var msgPtr = _testEnv.AllocateMemory(28); // sizeof(MSG)

    // Act
    _testEnv.CallKernel32Api("SETLASTERROR", 0xDEADBEEF);
    var result = _testEnv.CallUser32Api("GETMESSAGEA", msgPtr, hwnd, 0, 0);

    // Assert
    Assert.Equal(unchecked((uint)-1), result); // -1
    const uint ERROR_INVALID_WINDOW_HANDLE = 1400;
    Assert.Equal(ERROR_INVALID_WINDOW_HANDLE, _testEnv.CallKernel32Api("GETLASTERROR"));
}
```

### 4. Add Attribution

Include attribution comments:

```csharp
#region GetMessage/PeekMessage Tests
// Ported from: rostests/apitests/user32/GetPeekMessage.c
// Original copyright: Thomas Faber <thomas.faber@reactos.org>

[Fact]
public void GetMessage_WithInvalidWindowHandle_ShouldReturnMinusOne()
{
    // ...
}
#endregion
```

## Porting Patterns

### Pattern 1: Simple Assertion Test

**ReactOS C:**
```c
ok(GetVersion() == 0x80000A04, "Version = %lx\n", GetVersion());
```

**C# xUnit:**
```csharp
[Fact]
public void GetVersion_ShouldReturnWindows95Version()
{
    var result = _testEnv.CallKernel32Api("GETVERSION");
    Assert.Equal(0x80000A04u, result);
}
```

### Pattern 2: Error Code Testing

**ReactOS C:**
```c
SetLastError(0xdeadbeef);
result = SetScrollRange(hScroll, SB_CTL, INT_MIN, INT_MAX, FALSE);
ok(result == FALSE, "...");
ok(GetLastError() == ERROR_INVALID_SCROLLBAR_RANGE, "...");
```

**C# xUnit:**
```csharp
[Fact]
public void SetScrollRange_WithInvalidRange_ShouldFail()
{
    _testEnv.CallKernel32Api("SETLASTERROR", 0xdeadbeef);
    var result = _testEnv.CallUser32Api("SETSCROLLRANGE", hScroll, SB_CTL, 
        (uint)int.MinValue, (uint)int.MaxValue, 0);
    
    Assert.Equal(0u, result); // FALSE
    const uint ERROR_INVALID_SCROLLBAR_RANGE = 1448;
    Assert.Equal(ERROR_INVALID_SCROLLBAR_RANGE, _testEnv.CallKernel32Api("GETLASTERROR"));
}
```

### Pattern 3: Parameterized Tests

**ReactOS C:**
```c
struct { INT nMin; INT nMax; BOOL result; } tests[] = {
    {  0,         0,    TRUE },
    {  0,   INT_MAX,    TRUE },
    { -1,   INT_MAX,   FALSE },
};

for (i = 0; i < sizeof(tests) / sizeof(tests[0]); i++)
{
    success = SetScrollRange(hScroll, SB_CTL, tests[i].nMin, tests[i].nMax, FALSE);
    ok(success == tests[i].result, "...");
}
```

**C# xUnit:**
```csharp
[Theory]
[InlineData(0, 0, true)]
[InlineData(0, int.MaxValue, true)]
[InlineData(-1, int.MaxValue, false)]
public void SetScrollRange_WithVariousRanges_ShouldValidateCorrectly(
    int nMin, int nMax, bool shouldSucceed)
{
    var success = _testEnv.CallUser32Api("SETSCROLLRANGE", 
        hScroll, SB_CTL, (uint)nMin, (uint)nMax, 0);
    
    if (shouldSucceed)
        Assert.NotEqual(0u, success);
    else
        Assert.Equal(0u, success);
}
```

### Pattern 4: Structure Testing

**ReactOS C:**
```c
WNDCLASSEXW wcex;
memset(&wcex, 0xab, sizeof(wcex));
result = GetClassInfoExW(GetModuleHandle(NULL), (LPCWSTR)WC_DESKTOP, &wcex);
ok(result == WC_DESKTOP, "...");
ok(wcex.cbSize == 0xabababab, "...");  // Not modified
ok(wcex.style == 0x8, "...");
```

**C# xUnit:**
```csharp
[Fact]
public void GetClassInfoExW_Desktop_ShouldReturnCorrectClassInfo()
{
    const uint WC_DESKTOP = 0x8001;
    var wcexPtr = _testEnv.AllocateMemory(48); // sizeof(WNDCLASSEXW)
    
    // Fill with pattern
    for (uint i = 0; i < 48; i++)
        _testEnv.Memory.Write8(wcexPtr + i, 0xab);
    
    var result = _testEnv.CallUser32Api("GETCLASSINFOEXA",
        0x00400000, WC_DESKTOP, wcexPtr);
    
    Assert.Equal(WC_DESKTOP, result);
    Assert.Equal(0xabababab, _testEnv.Memory.Read32(wcexPtr + 0)); // cbSize not modified
    Assert.Equal(0x8u, _testEnv.Memory.Read32(wcexPtr + 4)); // style
}
```

## TestEnvironment Helpers

### String Writing

```csharp
// ANSI string
var strPtr = _testEnv.WriteString("Hello");

// Unicode string
var wstrPtr = _testEnv.WriteStringW("Hello");
```

### Memory Allocation

```csharp
var ptr = _testEnv.AllocateMemory(size);
_testEnv.Memory.Write32(ptr, value);
var value = _testEnv.Memory.Read32(ptr);
```

### API Calling

```csharp
// User32 API
var result = _testEnv.CallUser32Api("CREATEWINDOWA", arg1, arg2, ...);

// Kernel32 API (for error testing)
_testEnv.CallKernel32Api("SETLASTERROR", 0xdeadbeef);
var error = _testEnv.CallKernel32Api("GETLASTERROR");
```

### Structure Writing

```csharp
// Write WNDCLASSA structure
var wndClassPtr = _testEnv.WriteWndClassA(
    className: "MyClass",
    wndProc: 0x00401000,
    cbClsExtra: 0,
    cbWndExtra: 8
);
```

## Error Constants

Common Win32 error codes:

```csharp
const uint ERROR_SUCCESS = 0;
const uint ERROR_INVALID_HANDLE = 6;
const uint ERROR_INVALID_PARAMETER = 87;
const uint ERROR_INVALID_WINDOW_HANDLE = 1400;
const uint ERROR_INVALID_INDEX = 1413;
const uint ERROR_INVALID_SCROLLBAR_RANGE = 1448;
```

## Best Practices

### 1. Test Independence
Each test should be self-contained:
```csharp
public void Dispose()
{
    _testEnv.Dispose();
    GC.SuppressFinalize(this);
}
```

### 2. Descriptive Names
Use clear method names:
- Good: `GetMessage_WithInvalidWindowHandle_ShouldReturnMinusOne`
- Bad: `TestGetMessage`

### 3. Arrange/Act/Assert
Structure tests clearly:
```csharp
// Arrange - Setup
var hwnd = CreateTestWindow();

// Act - Execute
var result = _testEnv.CallUser32Api("SENDMESSAGEA", hwnd, WM_CLOSE, 0, 0);

// Assert - Verify
Assert.Equal(0u, result);
```

### 4. Comment Complex Tests
Explain non-obvious behavior:
```csharp
// Offset 1 overlaps with offset 0 because we're writing WORD values
// into overlapping byte positions in the window extra data
Assert.Equal(0x12u, _testEnv.CallUser32Api("SETWINDOWWORD", hwnd, 1, 0x2345));
```

### 5. Handle Test Failures
Tests document current implementation:
```csharp
// NOTE: This test currently fails because User32 doesn't implement
// GetClassInfoExA for system classes. This documents expected behavior.
[Fact]
public void GetClassInfoExW_Desktop_ShouldReturnCorrectClassInfo()
{
    // ...
}
```

## Example: Complete Test Port

**ReactOS C (SetScrollRange.c):**
```c
START_TEST(SetScrollRange)
{
    struct { INT nMin; INT nMax; BOOL result; } tests[] = {
        {  0,         0,    TRUE },
        {  0,   INT_MAX,    TRUE },
    };
    HWND hScroll = CreateWindowExW(0, L"SCROLLBAR", NULL, 0, 0, 0, 0, 0, NULL, NULL, NULL, NULL);
    
    for (i = 0; i < sizeof(tests) / sizeof(tests[0]); i++)
    {
        SetScrollRange(hScroll, SB_CTL, 123, 456, FALSE);
        SetLastError(0xdeaff00d);
        success = SetScrollRange(hScroll, SB_CTL, tests[i].nMin, tests[i].nMax, FALSE);
        GetScrollRange(hScroll, SB_CTL, &newMin, &newMax);
        
        if (tests[i].result)
        {
            ok(success == TRUE, "...");
            ok(newMin == tests[i].nMin, "...");
            ok(newMax == tests[i].nMax, "...");
        }
        else
        {
            ok(success == FALSE, "...");
            ok(error == ERROR_INVALID_SCROLLBAR_RANGE, "...");
            ok(newMin == 123, "...");
            ok(newMax == 456, "...");
        }
    }
    DestroyWindow(hScroll);
}
```

**C# xUnit (ReactOSPortedTests.cs):**
```csharp
#region SetScrollRange Tests
// Ported from: rostests/apitests/user32/SetScrollRange.c
// Original copyright: Thomas Faber <thomas.faber@reactos.org>

[Theory]
[InlineData(0, 0, true)]
[InlineData(0, int.MaxValue, true)]
[InlineData(-1, int.MaxValue, false)]
public void SetScrollRange_WithVariousRanges_ShouldValidateCorrectly(
    int nMin, int nMax, bool shouldSucceed)
{
    // Arrange - Create a scrollbar control
    var classNamePtr = _testEnv.WriteString("SCROLLBAR");
    var hScroll = _testEnv.CallUser32Api("CREATEWINDOWEXA",
        0, classNamePtr, 0, 0x00000000, 0, 0, 0, 0, 0, 0, 0, 0);
    Assert.NotEqual(0u, hScroll);

    // Set initial values
    const int SB_CTL = 2;
    _testEnv.CallUser32Api("SETSCROLLRANGE", hScroll, SB_CTL, 123, 456, 0);

    // Act
    _testEnv.CallKernel32Api("SETLASTERROR", 0xdeaff00d);
    var success = _testEnv.CallUser32Api("SETSCROLLRANGE", 
        hScroll, SB_CTL, (uint)nMin, (uint)nMax, 0);

    var minPtr = _testEnv.AllocateMemory(4);
    var maxPtr = _testEnv.AllocateMemory(4);
    _testEnv.CallUser32Api("GETSCROLLRANGE", hScroll, SB_CTL, minPtr, maxPtr);
    var newMin = (int)_testEnv.Memory.Read32(minPtr);
    var newMax = (int)_testEnv.Memory.Read32(maxPtr);

    // Assert
    if (shouldSucceed)
    {
        Assert.NotEqual(0u, success);
        Assert.Equal(nMin, newMin);
        Assert.Equal(nMax, newMax);
    }
    else
    {
        Assert.Equal(0u, success);
        const uint ERROR_INVALID_SCROLLBAR_RANGE = 1448;
        Assert.Equal(ERROR_INVALID_SCROLLBAR_RANGE, _testEnv.CallKernel32Api("GETLASTERROR"));
        Assert.Equal(123, newMin);
        Assert.Equal(456, newMax);
    }

    // Cleanup
    _testEnv.CallUser32Api("DESTROYWINDOW", hScroll);
}
#endregion
```

## Tips

1. **Start Simple** - Port simple tests first (GetVersion, GetLastError)
2. **Read ReactOS Source** - Understand test intent and edge cases
3. **Use Existing Helpers** - TestEnvironment has many useful methods
4. **Document Failures** - Tests that fail document implementation gaps
5. **Mark as Optional** - Use `[Trait("Category", "DllModuleTests")]`

## References

- [ReactOS API Tests](https://github.com/reactos/reactos/tree/master/modules/rostests/apitests)
- [Wine Test Framework](https://wiki.winehq.org/Wine_Testing_Framework)
- [Win32Emu Test Strategy](../../README.Tests.md)
- [ReactOS Test Integration Research](../research/REACTOS_TEST_INTEGRATION.md)

## See Also

- `Win32Emu.Tests.User32/ReactOSPortedTests.cs` - Example ported tests
- `Win32Emu.Tests.User32/TestInfrastructure/TestEnvironment.cs` - Test helpers
- `Win32Emu.Tests.Kernel32/` - Similar Kernel32 test patterns

---

**Last Updated:** 2025-12-14  
**Status:** Initial porting complete (4 test groups, 12 test methods)
