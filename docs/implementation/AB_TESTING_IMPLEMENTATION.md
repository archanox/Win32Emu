# Implementation Summary: A-B Testing for Win32 API Functions

## Overview

This implementation addresses the request to "do use case number 3" from NATIVE_DLL_ANALYSIS.md with A-B testing capabilities. Use case #3 is about test-driven development where native DLL exports serve as a specification for implementing Win32 API functions.

## What is A-B Testing?

In the context of Win32Emu:
- **A**: Win32Emu's implementation of a Win32 API function
- **B**: Native Windows DLL implementation of the same function

A-B testing means running both implementations with identical inputs and comparing the results to ensure Win32Emu matches native Windows behavior exactly.

## What Was Implemented

### 1. Win32Emu.Tools.TestGenerator

A command-line tool that automates test creation:

**Features:**
- Reads missing functions from NativeDllAnalyzer JSON output
- Generates xUnit test scaffolding for missing/stub functions
- Creates platform-aware test infrastructure
- Includes proper traits for filtering and categorization
- Limits generation to avoid overwhelming (first 10 functions per DLL)

**Usage:**
```bash
dotnet run --project Win32Emu.Tools.TestGenerator \
  docs/pages/missing-functions.json \
  Win32Emu.Tests.Generated \
  KERNEL32.DLL
```

**Generates:**
- `README.md` - Documentation for generated tests
- `ABTestBase.cs` - Base class for A-B comparison tests
- `NativeDllLoader.cs` - Platform-aware native DLL loader
- `{DllName}ABTests.cs` - Test files with scaffolding

### 2. A-B Testing Infrastructure

**ABTestBase Class:**
- Handles platform detection (Windows vs Linux/macOS)
- Loads native DLLs on Windows via P/Invoke
- Gracefully degrades on non-Windows platforms
- Provides comparison helpers

**NativeDllLoader Class:**
- Uses Windows LoadLibrary/GetProcAddress APIs
- Only available on Windows
- Throws PlatformNotSupportedException on other platforms

**Platform Behavior:**
- **Windows**: Full A-B comparison against native DLLs
- **Linux/macOS**: Tests run Win32Emu only, native comparison skipped
- **CI/CD**: Tests pass on all platforms without blocking builds

### 3. Win32Emu.Tests.ABExample

A complete example project demonstrating the workflow:

**Example Tests:**
- `GetVersion` - Shows basic A-B comparison
- `GetLastError` - Shows state management
- `SetLastError/GetLastError` - Shows function interaction

**Documentation:**
- Step-by-step workflow guide
- Platform-specific behavior explanation
- Best practices and tips
- Integration with test generator

## Workflow Example

### Step 1: Analyze Native DLLs
```bash
# Generate API status
dotnet run --project Win32Emu.Tools.ApiStatusGenerator docs/pages/api-status.json

# Analyze native DLLs
dotnet run --project Win32Emu.Tools.NativeDllAnalyzer \
  DLLs/WinME \
  docs/pages/api-status.json \
  docs/pages/missing-functions.json
```

### Step 2: Generate Test Scaffolding
```bash
dotnet run --project Win32Emu.Tools.TestGenerator \
  docs/pages/missing-functions.json \
  Win32Emu.Tests.Generated \
  KERNEL32.DLL
```

This creates test stubs like:
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

### Step 3: Implement Function in Win32Emu

Add implementation in appropriate module (e.g., Win32Emu.Kernel32):
```csharp
[DllExport("GetTempPathA", CallingConvention = CallingConvention.Winapi)]
public static uint GetTempPathA(EmulatorEnvironment env, uint nBufferLength, IntPtr lpBuffer)
{
    // Implementation
}
```

### Step 4: Update Test

Replace TODO with actual test:
```csharp
[Fact]
[Trait("Category", "ABTest")]
[Trait("Function", "GetTempPathA")]
public void GetTempPathA_ShouldMatchNativeBehavior()
{
    // Arrange
    using var testEnv = new TestEnvironment();
    var bufferSize = 260u;
    var bufferPtr = testEnv.AllocateMemory((int)bufferSize);
    
    // Act - Win32Emu
    var win32EmuResult = testEnv.CallKernel32Api("GETTEMPPATHA", bufferSize, bufferPtr);
    
    // Act - Native (if on Windows)
    if (_nativeAvailable)
    {
        var nativeBuffer = new byte[bufferSize];
        var nativeResult = NativeGetTempPathA(bufferSize, nativeBuffer);
        
        // Assert - Compare
        AssertABMatch("GetTempPathA", win32EmuResult, nativeResult);
    }
}
```

### Step 5: Run Tests
```bash
# Run specific test
dotnet test --filter "Function=GetTempPathA"

# Run all A-B tests
dotnet test --filter "Category=ABTest"
```

### Step 6: Verify Coverage
```bash
# Re-analyze to see improvement
dotnet run --project Win32Emu.Tools.NativeDllAnalyzer \
  DLLs/WinME \
  docs/pages/api-status.json
```

## Benefits

1. **Test-Driven Development**
   - Write tests before implementation
   - Tests document expected behavior
   - Clear success criteria

2. **Behavior Validation**
   - Ensures Win32Emu matches Windows exactly
   - Catches subtle differences in behavior
   - Validates edge cases and error conditions

3. **Regression Prevention**
   - Tests catch breaking changes
   - Automated validation in CI/CD
   - Safe refactoring

4. **Cross-Platform Support**
   - Works on Windows with full A-B comparison
   - Works on Linux/macOS with Win32Emu validation
   - Tests don't block non-Windows builds

5. **Documentation**
   - Tests serve as usage examples
   - Document expected behavior
   - Show correct parameter usage

## Testing Results

✅ **All tests passing on Linux CI**
- 3 example A-B tests implemented
- Tests gracefully handle missing native DLLs
- Platform detection works correctly
- No CI/CD blocking

✅ **Test generator working**
- Successfully generates scaffolding from analysis
- Creates proper test structure
- Includes all necessary infrastructure

✅ **Code quality**
- No build errors or warnings in new code
- Code review feedback addressed
- Follows project conventions

## Files Added

```
Win32Emu.Tools.TestGenerator/
├── Win32Emu.Tools.TestGenerator.csproj
├── Program.cs (main generator logic)
├── DataModels.cs (data structures)
└── README.md (tool documentation)

Win32Emu.Tests.ABExample/
├── Win32Emu.Tests.ABExample.csproj
├── GetVersionABTests.cs (example tests)
└── README.md (workflow guide)

docs/
└── NATIVE_DLL_ANALYSIS.md (updated with test generator section)
```

## Documentation Updates

1. **NATIVE_DLL_ANALYSIS.md**
   - Added Win32Emu.Tools.TestGenerator section
   - Updated use case #3 with A-B testing details
   - Added complete workflow example
   - Updated component numbering

2. **Win32Emu.Tools.TestGenerator/README.md**
   - Tool purpose and usage
   - Generated file structure
   - Workflow examples
   - Platform support details
   - Best practices

3. **Win32Emu.Tests.ABExample/README.md**
   - Complete step-by-step guide
   - Example implementations
   - Platform behavior explanation
   - Integration with test generator

## Integration with Existing Tools

This implementation builds on existing infrastructure:

1. **Win32Emu.Tools.ApiStatusGenerator**
   - Generates API implementation status
   - Used as input for analyzer

2. **Win32Emu.Tools.NativeDllAnalyzer**
   - Compares native DLLs with Win32Emu
   - Generates missing functions report
   - Used as input for test generator

3. **Test Infrastructure**
   - Reuses TestEnvironment from Win32Emu.Tests.Kernel32
   - Follows existing test patterns
   - Compatible with CI/CD setup

## Future Enhancements

Possible improvements:
- Auto-generate test parameters from function signatures
- Support for Wine on Linux for native DLL testing
- Generate tests for already-implemented functions
- Automatic comparison of complex return types
- Integration with GitHub Issues for missing functions
- Historical tracking of test coverage

## Conclusion

This implementation fully addresses use case #3 from NATIVE_DLL_ANALYSIS.md by:

1. ✅ Automating test generation from native DLL exports
2. ✅ Enabling A-B testing to validate implementations
3. ✅ Supporting test-driven development workflow
4. ✅ Working cross-platform without blocking CI/CD
5. ✅ Providing comprehensive documentation and examples

The implementation is production-ready, tested, and documented. Developers can now use this workflow to systematically implement and validate Win32 API functions against native Windows behavior.
