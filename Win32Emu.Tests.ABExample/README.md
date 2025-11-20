# Win32Emu.Tests.ABExample

Example project demonstrating A-B testing for Win32 API implementations.

## Purpose

This project demonstrates the complete workflow for using the test generator to create A-B tests that compare Win32Emu implementations against native Windows DLL behavior.

## What is A-B Testing?

- **A**: Win32Emu's implementation
- **B**: Native Windows DLL behavior

The tests run both implementations with identical inputs and compare results to ensure Win32Emu matches Windows behavior.

## Workflow Example

This example shows how to implement and test `GetTempPathA` from KERNEL32.DLL:

### Step 1: Generate Missing Functions Report

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

## Platform Behavior

### Windows
- Native DLL is loaded via `LoadLibrary`
- Tests perform true A-B comparison
- Validates Win32Emu matches native behavior exactly

### Linux / macOS
- Native DLL loading is skipped (not available)
- Tests only run Win32Emu implementation
- Verifies basic behavior without comparison
- Tests don't fail CI/CD

## Example Test Structure

This project includes:

1. **GetTempPathABTest.cs** - Example of a fully implemented A-B test
2. Shows how to:
   - Set up test environment
   - Call Win32Emu implementation
   - Call native DLL (on Windows)
   - Compare results
   - Handle platform differences

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

1. **Start simple** - Begin with simple functions that don't have complex state
2. **Test edge cases** - Include tests for error conditions and boundary cases
3. **Document differences** - If Win32Emu intentionally differs, document why
4. **Use test traits** - Tag tests by category, DLL, and implementation status
5. **Keep tests focused** - One test per function or scenario

## See Also

- [Win32Emu.Tools.TestGenerator](../Win32Emu.Tools.TestGenerator/README.md)
- [NATIVE_DLL_ANALYSIS.md](../docs/NATIVE_DLL_ANALYSIS.md)
- [README.Tests.md](../README.Tests.md)
