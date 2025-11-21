# A/B Testing Guide for Win32Emu

This guide explains how to use EasyHook-based A/B testing to validate Win32Emu's implementation against real Windows DLLs.

## Overview

A/B testing compares:
- **A**: Win32Emu's Win32 API implementations
- **B**: Native Windows DLL behavior (via EasyHook interception)

This is especially useful for:
1. Validating that Win32Emu behaves like real Windows
2. Finding implementation differences
3. Debugging game compatibility issues
4. Regression testing

## When to Use Each Approach

### Direct P/Invoke Testing
**Use for:**
- Simple, stateless functions (GetVersion, GetTickCount)
- Quick validation of function return values
- Functions with no side effects

**Example:**
```csharp
public class SimpleABTests : ABTestBase
{
    public SimpleABTests() : base("KERNEL32.DLL") { }
    
    [Fact]
    public void GetVersion_MatchesNative()
    {
        using var testEnv = new TestEnvironment();
        var win32EmuResult = testEnv.CallKernel32Api("GETVERSION");
        
        uint? nativeResult = null;
        if (_nativeAvailable)
        {
            nativeResult = NativeGetVersion();
        }
        
        AssertABMatch("GetVersion", win32EmuResult, nativeResult);
    }
    
    [DllImport("kernel32.dll")]
    private static extern uint GetVersion();
    
    private static uint NativeGetVersion() => GetVersion();
}
```

### EasyHook-Based Testing
**Use for:**
- File I/O operations (CreateFile, ReadFile, WriteFile)
- Memory management (VirtualAlloc, HeapAlloc)
- Registry operations
- Window creation and messaging
- Complex stateful APIs
- API call sequences

**Example:**
```csharp
public class FileIOHookTests : HookingABTestBase
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Ansi)]
    private delegate IntPtr CreateFileADelegate(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );
    
    private CreateFileADelegate? _originalCreateFileA;
    
    public FileIOHookTests()
    {
        if (_hookingAvailable)
        {
            _originalCreateFileA = GetOriginalFunction<CreateFileADelegate>("kernel32.dll", "CreateFileA");
            CreateHook("kernel32.dll", "CreateFileA", new CreateFileADelegate(CreateFileAHook));
        }
    }
    
    private IntPtr CreateFileAHook(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile)
    {
        CaptureHookData("CreateFileA.FileName", lpFileName);
        CaptureHookData("CreateFileA.DesiredAccess", dwDesiredAccess);
        
        var result = _originalCreateFileA?.Invoke(
            lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile
        ) ?? new IntPtr(-1);
        
        CaptureHookData("CreateFileA.Result", result);
        return result;
    }
    
    [Fact]
    public void CreateFileA_BehaviorMatchesNative()
    {
        using var testEnv = new TestEnvironment();
        var testFile = "test.txt";
        var fileNamePtr = testEnv.WriteString(testFile);
        
        // Win32Emu call
        var win32EmuHandle = testEnv.CallKernel32Api(
            "CREATEFILEA",
            fileNamePtr,
            0x80000000u, // GENERIC_READ
            0u, 0u, 3u, 0x80u, 0u
        );
        
        // Native call (triggers hook)
        IntPtr? nativeHandle = null;
        if (_hookingAvailable && _originalCreateFileA != null)
        {
            var tempPath = System.IO.Path.GetTempPath();
            nativeHandle = _originalCreateFileA.Invoke(
                System.IO.Path.Combine(tempPath, testFile),
                0x80000000,
                0,
                IntPtr.Zero,
                3,
                0x80,
                IntPtr.Zero
            );
        }
        
        // Compare results
        Assert.NotEqual(0u, win32EmuHandle);
        if (nativeHandle.HasValue && nativeHandle.Value != new IntPtr(-1))
        {
            // Both should succeed
            Assert.True(true);
        }
    }
}
```

## Step-by-Step: Adding A/B Tests for a New Function

### Step 1: Identify the Function
Let's say you want to test `GetSystemTime` from KERNEL32.DLL.

### Step 2: Choose Your Approach
`GetSystemTime` writes to a SYSTEMTIME structure, so we'll use hooking to capture both input and output.

### Step 3: Create the Test Class
```csharp
using System;
using System.Runtime.InteropServices;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.ABExample;

public class GetSystemTimeHookTests : HookingABTestBase
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }
    
    [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = false)]
    private delegate void GetSystemTimeDelegate(ref SYSTEMTIME lpSystemTime);
    
    private GetSystemTimeDelegate? _originalGetSystemTime;
    
    public GetSystemTimeHookTests()
    {
        if (_hookingAvailable)
        {
            _originalGetSystemTime = GetOriginalFunction<GetSystemTimeDelegate>("kernel32.dll", "GetSystemTime");
            CreateHook("kernel32.dll", "GetSystemTime", new GetSystemTimeDelegate(GetSystemTimeHook));
        }
    }
    
    private void GetSystemTimeHook(ref SYSTEMTIME lpSystemTime)
    {
        _originalGetSystemTime?.Invoke(ref lpSystemTime);
        CaptureHookData("GetSystemTime.Year", lpSystemTime.wYear);
        CaptureHookData("GetSystemTime.Month", lpSystemTime.wMonth);
    }
    
    [Fact]
    [Trait("Category", "HookTest")]
    [Trait("Function", "GetSystemTime")]
    public void GetSystemTime_ReturnsValidTime()
    {
        using var testEnv = new TestEnvironment();
        
        // Allocate memory for SYSTEMTIME structure
        var stPtr = testEnv.AllocateMemory(16); // sizeof(SYSTEMTIME)
        
        // Call Win32Emu
        testEnv.CallKernel32Api("GETSYSTEMTIME", stPtr);
        
        // Read results
        var year = testEnv.Memory.Read16(stPtr);
        var month = testEnv.Memory.Read16(stPtr + 2);
        
        // Call native (triggers hook)
        if (_hookingAvailable && _originalGetSystemTime != null)
        {
            var nativeTime = new SYSTEMTIME();
            _originalGetSystemTime.Invoke(ref nativeTime);
            
            var capturedYear = GetCapturedData<ushort>("GetSystemTime.Year");
            var capturedMonth = GetCapturedData<ushort>("GetSystemTime.Month");
            
            // Verify both implementations return reasonable values
            Assert.True(year >= 1900 && year <= 3000);
            Assert.True(month >= 1 && month <= 12);
            Assert.True(capturedYear >= 1900 && capturedYear <= 3000);
            Assert.True(capturedMonth >= 1 && capturedMonth <= 12);
        }
    }
}
```

### Step 4: Run the Test
```bash
dotnet test Win32Emu.Tests.ABExample --filter "Function=GetSystemTime"
```

## Testing a Game with A/B Validation

Here's how to use A/B testing to validate game compatibility:

### Scenario: Game Uses CreateFileA to Load Assets

1. **Create the Hook Test:**
```csharp
public class GameAssetLoadingTests : HookingABTestBase
{
    private readonly List<string> _filesOpened = new();
    
    [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Ansi)]
    private delegate IntPtr CreateFileADelegate(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );
    
    private CreateFileADelegate? _originalCreateFileA;
    
    public GameAssetLoadingTests()
    {
        if (_hookingAvailable)
        {
            _originalCreateFileA = GetOriginalFunction<CreateFileADelegate>("kernel32.dll", "CreateFileA");
            CreateHook("kernel32.dll", "CreateFileA", new CreateFileADelegate(CreateFileAHook));
        }
    }
    
    private IntPtr CreateFileAHook(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile)
    {
        // Track all files opened
        _filesOpened.Add(lpFileName);
        
        var result = _originalCreateFileA?.Invoke(
            lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile
        ) ?? new IntPtr(-1);
        
        CaptureHookData($"CreateFileA.{lpFileName}", result != new IntPtr(-1));
        return result;
    }
    
    [Fact]
    public void Game_OpensRequiredAssets()
    {
        // This test would run the game and verify it opens expected files
        using var testEnv = new TestEnvironment();
        
        // Simulate game opening config.ini
        var configPath = testEnv.WriteString("config.ini");
        var handle = testEnv.CallKernel32Api(
            "CREATEFILEA",
            configPath,
            0x80000000u, // GENERIC_READ
            0u, 0u, 3u, 0x80u, 0u
        );
        
        Assert.NotEqual(0u, handle);
        
        if (_hookingAvailable)
        {
            // Verify native Windows would also open the file successfully
            Assert.True(_filesOpened.Count > 0);
        }
    }
}
```

2. **Run the Game in Win32Emu:**
```bash
Win32Emu.Gui --nogui game.exe
```

3. **Compare behavior:**
   - Check which files Win32Emu tried to open
   - Check which files native Windows opened
   - Verify both behave the same way

## Debugging with A/B Tests

When a game doesn't work in Win32Emu:

1. **Add hooks for the failing APIs**
2. **Capture parameters and return values**
3. **Compare with native Windows behavior**
4. **Find the difference**
5. **Fix Win32Emu implementation**
6. **Re-run test to verify fix**

### Example: Debugging File Not Found

```csharp
[Fact]
public void DebugFileNotFound()
{
    using var testEnv = new TestEnvironment();
    
    // Game tries to open "DATA\\LEVEL1.DAT"
    var path = testEnv.WriteString("DATA\\LEVEL1.DAT");
    var handle = testEnv.CallKernel32Api(
        "CREATEFILEA",
        path,
        0x80000000u, // GENERIC_READ
        0u, 0u, 3u, 0x80u, 0u
    );
    
    // Check if Win32Emu failed
    if (handle == 0)
    {
        var error = testEnv.CallKernel32Api("GETLASTERROR");
        // ERROR_FILE_NOT_FOUND = 2
        // ERROR_PATH_NOT_FOUND = 3
        
        // Compare with native behavior
        if (_hookingAvailable && _originalCreateFileA != null)
        {
            var nativeHandle = _originalCreateFileA.Invoke(
                "DATA\\LEVEL1.DAT",
                0x80000000,
                0,
                IntPtr.Zero,
                3,
                0x80,
                IntPtr.Zero
            );
            
            // If native succeeds but Win32Emu fails, there's a bug!
            if (nativeHandle != new IntPtr(-1))
            {
                Assert.Fail("Native Windows can open file, but Win32Emu cannot!");
            }
        }
    }
}
```

## Best Practices

### 1. Test Incrementally
Start with simple functions, then move to complex ones.

### 2. Use Traits for Organization
```csharp
[Trait("Category", "HookTest")]
[Trait("DLL", "KERNEL32")]
[Trait("Function", "CreateFileA")]
[Trait("Complexity", "High")]
```

### 3. Document Differences
If Win32Emu intentionally differs from Windows, document it:
```csharp
[Fact]
public void GetVersion_ReturnsWindows95()
{
    // Win32Emu always returns Windows 95 version
    // This is intentional for game compatibility
    var result = testEnv.CallKernel32Api("GETVERSION");
    Assert.Equal(0x040003B6u, result); // Windows 95
}
```

### 4. Handle Platform Differences
Always check `_hookingAvailable` before using hooks:
```csharp
if (_hookingAvailable && _originalFunc != null)
{
    // Hook-based testing
}
else
{
    // Just test Win32Emu behavior
}
```

### 5. Clean Up Resources
Always dispose hooks and handles:
```csharp
public class MyTests : HookingABTestBase
{
    // Hooks are automatically cleaned up in base class Dispose()
}

[Fact]
public void TestWithCleanup()
{
    using var testEnv = new TestEnvironment();
    // ... test code ...
    // TestEnvironment.Dispose() is called automatically
}
```

## Running Tests

```bash
# All A/B tests
dotnet test Win32Emu.Tests.ABExample

# Only hooking tests
dotnet test Win32Emu.Tests.ABExample --filter "Category=HookTest"

# Specific function
dotnet test Win32Emu.Tests.ABExample --filter "Function=CreateFileA"

# Specific DLL
dotnet test Win32Emu.Tests.ABExample --filter "DLL=KERNEL32"
```

## Troubleshooting

### Hook Creation Fails
If `CreateHook` fails:
1. Verify you're on Windows (hooks only work on Windows)
2. Check that the function name is correct
3. Verify the delegate signature matches the API
4. Run as Administrator if needed

### Tests Pass on Windows but Fail on Linux
This is expected! Hooking only works on Windows. Tests should gracefully skip hooking on other platforms:
```csharp
if (!_hookingAvailable)
{
    // Skip or use alternative validation
    return;
}
```

### Captured Data is Null
If `GetCapturedData` returns null:
1. Verify the hook was called
2. Check that `CaptureHookData` was called in the hook handler
3. Verify the key name matches exactly

## See Also

- [EasyHook Documentation](https://easyhook.github.io/)
- [Win32 API Reference](https://docs.microsoft.com/en-us/windows/win32/api/)
- [Win32Emu.Tests.ABExample README](../Win32Emu.Tests.ABExample/README.md)
- [README.Tests.md](../README.Tests.md)
