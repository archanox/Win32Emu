# Win32Emu.Tests.ABExample

Example project demonstrating A-B testing for Win32 API implementations using EasyHook.

## Purpose

This project demonstrates two complementary approaches for A-B testing:

1. **Direct P/Invoke Comparison** - Call both Win32Emu and native APIs directly
2. **EasyHook-Based Hooking** (NEW) - Intercept native API calls in real-time

Both approaches validate that Win32Emu implementations match native Windows DLL behavior.

## What is A-B Testing?

- **A**: Win32Emu's implementation
- **B**: Native Windows DLL behavior (via P/Invoke or EasyHook)

The tests run both implementations with identical inputs and compare results to ensure Win32Emu matches Windows behavior.

## Two Testing Approaches

### Approach 1: Direct P/Invoke (Simple)
- Directly call native Windows APIs via P/Invoke
- Compare results with Win32Emu
- Good for simple, stateless functions
- Example: `GetVersionABTests.cs`

### Approach 2: EasyHook Hooking (Advanced)
- Use EasyHook to intercept native API calls
- Capture parameters and return values in real-time
- Better for complex, stateful, or context-dependent APIs
- Example: `HookingABTestBase.cs`, `FileIOHookingTests.cs`

#### Why Use Hooking?

Hooking provides several advantages:

1. **Real-time Interception**: Capture actual API call behavior as it happens
2. **State Tracking**: Monitor sequences of API calls and their interactions
3. **Parameter Validation**: Verify that Win32Emu passes correct parameters
4. **Side Effect Detection**: Detect unexpected system-level side effects
5. **Context Awareness**: Test APIs in their natural calling context

This aligns with the GitHub issue goal: "Hook native imports and validate behavior of our implementation of functions against real DLLs."

## Hook-Based Testing Examples

### Example 1: Simple Function Hooking (GetVersion)

```csharp
public class GetVersionHookingABTests : HookingABTestBase
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
    private delegate uint GetVersionDelegate();
    
    private GetVersionDelegate? _originalGetVersion;
    
    public GetVersionHookingABTests()
    {
        if (_hookingAvailable)
        {
            _originalGetVersion = GetOriginalFunction<GetVersionDelegate>("kernel32.dll", "GetVersion");
            CreateHook("kernel32.dll", "GetVersion", new GetVersionDelegate(GetVersionHook));
        }
    }
    
    private uint GetVersionHook()
    {
        var nativeResult = _originalGetVersion?.Invoke() ?? 0;
        CaptureHookData("GetVersion.Native", nativeResult);
        return nativeResult;
    }
    
    [Fact]
    public void GetVersion_WithHooking_ShouldMatchNativeBehavior()
    {
        using var testEnv = new TestEnvironment();
        var win32EmuResult = testEnv.CallKernel32Api("GETVERSION");
        
        // Trigger native call (intercepted by hook)
        uint? nativeResult = null;
        if (_hookingAvailable && _originalGetVersion != null)
        {
            _originalGetVersion.Invoke();
            nativeResult = GetCapturedData<uint>("GetVersion.Native");
        }
        
        Assert.NotEqual(0u, win32EmuResult);
        AssertABMatch("GetVersion", win32EmuResult, nativeResult);
    }
}
```

### Example 2: File I/O Hooking (CreateFileA)

```csharp
public class FileIOHookingTests : HookingABTestBase
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
    
    public FileIOHookingTests()
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
        // Capture parameters
        CaptureHookData("CreateFileA.FileName", lpFileName);
        CaptureHookData("CreateFileA.DesiredAccess", dwDesiredAccess);
        
        // Call original
        var result = _originalCreateFileA?.Invoke(
            lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes,
            dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile
        ) ?? new IntPtr(-1);
        
        // Capture result
        CaptureHookData("CreateFileA.Result", result);
        return result;
    }
    
    [Fact]
    public void CreateFileA_WithHooking_ValidatesHandleCreation()
    {
        // Test implementation that compares Win32Emu vs hooked native behavior
        // See FileIOHookingTests.cs for full example
    }
}
```

## Running Hook-Based Tests

```bash
# Run all hooking tests (Windows only)
dotnet test --filter "Category=HookTest"

# Run specific hooking test
dotnet test --filter "Function=CreateFileA&Category=HookTest"

# On Linux/macOS, hooking tests are automatically skipped
dotnet test Win32Emu.Tests.ABExample
```

## Workflow Example: Adding New A-B Tests

This example shows how to implement and test `GetTempPathA` from KERNEL32.DLL:

```bash
# Generate API status
dotnet run --project Win32Emu.Tools.ApiStatusGenerator docs/pages/api-status.json

# Analyze native DLLs
dotnet run --project Win32Emu.Tools.NativeDllAnalyzer DLLs/WinME docs/pages/api-status.json docs/pages/missing-functions.json
```

### Step 2: Generate Test Scaffolding

```bash
# Generate tests for KERNEL32
dotnet run --project Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.ABExample KERNEL32.DLL
```

This creates:
- `ABTestBase.cs` - Base class for A-B tests
- `NativeDllLoader.cs` - Native DLL loader for Windows
- `KERNEL32ABTests.cs` - Test file with scaffolding

### Step 3: Implement Function in Win32Emu

Example implementation in `Win32Emu.Kernel32/Kernel32.cs`:

```csharp
[DllExport("GetTempPathA", CallingConvention = CallingConvention.Winapi)]
public static uint GetTempPathA(EmulatorEnvironment env, uint nBufferLength, IntPtr lpBuffer)
{
    // Implementation here
    return result;
}
```

### Step 4: Update Generated Test

The generated test starts as:

```csharp
[Fact]
[Trait("Category", "ABTest")]
[Trait("Function", "GetTempPathA")]
public void GetTempPathA_ShouldMatchNativeBehavior()
{
    // TODO: Implement test
    Skip.If(true, "Test not yet implemented");
}
```

Update it to:

```csharp
[Fact]
[Trait("Category", "ABTest")]
[Trait("Function", "GetTempPathA")]
public void GetTempPathA_ShouldMatchNativeBehavior()
{
    // Arrange
    using var testEnv = new TestEnvironment();
    var bufferSize = 260u; // MAX_PATH
    var bufferPtr = testEnv.AllocateMemory((int)bufferSize);
    
    // Act - Call Win32Emu
    var win32EmuResult = testEnv.CallKernel32Api("GETTEMPPATHA", bufferSize, bufferPtr);
    var win32EmuPath = testEnv.ReadString(bufferPtr);
    
    // Act - Call native (if on Windows)
    if (_nativeAvailable)
    {
        var nativeBuffer = new byte[bufferSize];
        var nativeResult = NativeGetTempPathA(bufferSize, nativeBuffer);
        var nativePath = Encoding.ASCII.GetString(nativeBuffer).TrimEnd('\0');
        
        // Assert - Compare results
        AssertABMatch("GetTempPathA", win32EmuResult, nativeResult);
        AssertABMatch("GetTempPathA Path", win32EmuPath, nativePath);
    }
    else
    {
        // On Linux/macOS, just verify Win32Emu behavior
        Assert.True(win32EmuResult > 0);
        Assert.NotEmpty(win32EmuPath);
    }
}

[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
private static extern uint GetTempPathA(uint nBufferLength, byte[] lpBuffer);
```

### Step 5: Run Tests

```bash
# Run all A-B tests
dotnet test --filter "Category=ABTest"

# Run specific function test
dotnet test --filter "Function=GetTempPathA"
```

### Step 6: Verify Coverage

```bash
# Re-run analyzer to see coverage improvement
dotnet run --project Win32Emu.Tools.NativeDllAnalyzer DLLs/WinME docs/pages/api-status.json
```

## Benefits of Hook-Based Testing

1. **Comprehensive Validation** - Verify Win32Emu matches Windows behavior exactly
2. **Real-time Monitoring** - Capture API behavior as it happens
3. **Complex Scenario Testing** - Test stateful and context-dependent APIs
4. **Regression Prevention** - Catch breaking changes early
5. **Documentation** - Tests document expected Windows API behavior
6. **Cross-platform Friendly** - Gracefully skips hooking on Linux/macOS
7. **CI/CD Compatible** - Optional tests don't block builds

## Platform Behavior

### Windows
- Native DLL loading works via `LoadLibrary` / `NativeAPI.LoadLibrary`
- EasyHook can create local hooks for API interception
- Tests perform true A-B comparison with live hooking
- Validates Win32Emu matches native behavior exactly

### Linux / macOS
- Native DLL loading is not available (Windows-only)
- EasyHook hooking is not available (Windows-only)
- Tests automatically skip hooking-based assertions
- Tests still run Win32Emu implementation for behavior verification
- Tests don't fail CI/CD on non-Windows platforms

## Example Test Structure

This project includes multiple examples:

### Direct P/Invoke Tests
1. **GetVersionABTests.cs** - Simple P/Invoke comparison tests
   - `GetVersion` - Version number comparison
   - `GetLastError/SetLastError` - Error state management

### Hook-Based Tests  
2. **HookingABTestBase.cs** - Base infrastructure for hooking tests
   - `GetVersionHookingABTests` - Demonstrates simple function hooking
   - `LastErrorHookingABTests` - Demonstrates stateful API hooking

3. **FileIOHookingTests.cs** - Advanced file I/O hooking examples
   - `CreateFileA` with valid paths
   - `CreateFileA` with invalid paths (error handling)
   - `GetTempPathA` path comparison
   - Demonstrates parameter capture and validation

Each example shows:
- How to define function delegates
- How to create hooks with EasyHook
- How to capture and compare behavior
- How to handle platform differences (Windows vs Linux/macOS)

## Running the Example

```bash
# Build the project
dotnet build Win32Emu.Tests.ABExample

# Run tests (Windows: full A-B comparison, Linux/macOS: Win32Emu only)
dotnet test Win32Emu.Tests.ABExample

# Run with verbose output
dotnet test Win32Emu.Tests.ABExample -v normal
```

## Benefits

1. **Automated verification** - Tests ensure implementations match Windows
2. **Regression prevention** - Catch breaking changes early
3. **Documentation** - Tests document expected behavior
4. **Cross-platform** - Works on all platforms with appropriate behavior
5. **CI/CD friendly** - Optional tests don't block builds

## Best Practices

### General Guidelines
1. **Start simple** - Begin with stateless functions using P/Invoke
2. **Use hooking for complexity** - Switch to hooks for stateful or context-dependent APIs
3. **Test edge cases** - Include tests for error conditions and boundary cases
4. **Document differences** - If Win32Emu intentionally differs, document why
5. **Use test traits** - Tag tests by category, DLL, and implementation status
6. **Keep tests focused** - One test per function or scenario

### Hooking-Specific Guidelines
1. **Define accurate delegates** - Match Windows API signatures exactly
2. **Capture relevant data** - Store parameters and results for comparison
3. **Clean up hooks** - Always dispose hooks properly (use `using` or try-finally)
4. **Handle platform differences** - Check `_hookingAvailable` before hooking
5. **Call original functions** - Always invoke the original API in hook handlers
6. **Test in isolation** - Each hook should be independent and reusable

### What to Hook vs What to P/Invoke

**Use P/Invoke for:**
- Simple, stateless functions (GetVersion, GetTickCount)
- Functions with no side effects
- Read-only queries
- Quick validation tests

**Use Hooking for:**
- File I/O operations (CreateFile, ReadFile, WriteFile)
- Memory management (VirtualAlloc, HeapAlloc)
- Registry operations
- Window creation and messaging
- Complex multi-step operations
- Testing API call sequences

## See Also

- [EasyHook - Creating a Local Hook](https://easyhook.github.io/tutorials/createlocalhook.html) - Official EasyHook tutorial
- [EasyHook GitHub Repository](https://github.com/EasyHook/EasyHook) - Source code and examples
- [Win32Emu.Tools.TestGenerator](../Win32Emu.Tools.TestGenerator/README.md) - Generate test scaffolding
- [NATIVE_DLL_ANALYSIS.md](../docs/NATIVE_DLL_ANALYSIS.md) - Native DLL analysis documentation
- [README.Tests.md](../README.Tests.md) - Overall test strategy

## References

This implementation addresses the GitHub issue:
- **Issue**: A/B testing DLLs on Windows
- **Approach**: Use EasyHook to hook native imports and validate behavior
- **Goal**: Compare Win32Emu implementations against real Windows DLLs

The hooking approach provides comprehensive validation by intercepting actual Windows API calls in real-time, capturing their parameters and return values, and comparing them against Win32Emu's implementations.
