# Win32Emu.Tools.TestGenerator

A tool that generates A-B test scaffolding from native DLL analysis reports, enabling test-driven development for Win32 API implementations.

## Purpose

This tool implements **Use Case #3** from `NATIVE_DLL_ANALYSIS.md`: Test-driven development using native DLL exports as specification, with A-B testing to compare Win32Emu behavior against native Windows DLLs.

## What is A-B Testing?

A-B testing in this context means:
- **A**: Win32Emu's implementation of a Win32 API function
- **B**: Native Windows DLL implementation of the same function

The generated tests call both implementations with the same inputs and compare the results to ensure Win32Emu matches native Windows behavior.

## How It Works

1. **Reads native DLL analysis** - Parses the JSON report from NativeDllAnalyzer
2. **Generates test scaffolding** - Creates xUnit test files for missing/stub functions
3. **Creates infrastructure** - Provides base classes for A-B comparison
4. **Platform-aware** - Tests work on Windows (with native DLLs) and Linux/macOS (Win32Emu only)

## Usage

```bash
# Generate tests for all DLLs with missing functions
dotnet run --project Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.Generated

# Generate tests for specific DLL
dotnet run --project Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.Generated KERNEL32.DLL
```

## Generated Files

The tool generates:

1. **README.md** - Documentation for the generated tests
2. **ABTestBase.cs** - Base class for A-B comparison tests
3. **NativeDllLoader.cs** - Helper for loading native Windows DLLs (Windows only)
4. **{DllName}ABTests.cs** - Test files for each DLL with missing/stub functions

## Generated Test Structure

Each test follows this pattern:

```csharp
[Fact]
[Trait("Category", "ABTest")]
[Trait("Category", "NeedsImplementation")]
[Trait("Function", "FunctionName")]
public void FunctionName_ShouldMatchNativeBehavior()
{
    // TODO: Implement test
    // 1. Setup test parameters
    // 2. Call Win32Emu implementation
    // 3. Call native DLL implementation (if available)
    // 4. Compare results using AssertABMatch
    
    Skip.If(true, "Test not yet implemented");
}
```

## Workflow

1. **Generate missing functions report**:
   ```bash
   dotnet run --project Win32Emu.Tools.ApiStatusGenerator docs/pages/api-status.json
   dotnet run --project Win32Emu.Tools.NativeDllAnalyzer DLLs/WinME docs/pages/api-status.json docs/pages/missing-functions.json
   ```

2. **Generate test scaffolding**:
   ```bash
   dotnet run --project Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.Generated
   ```

3. **Implement a function** in Win32Emu (e.g., in Win32Emu.Kernel32)

4. **Update the test**:
   - Remove `Skip.If(...)` attribute
   - Add proper test parameters
   - Add assertions to verify behavior
   - Run test to ensure it passes

5. **Verify A-B match**:
   - On Windows: Test compares Win32Emu vs native DLL
   - On Linux/macOS: Test only validates Win32Emu behavior

## Platform Support

### Windows
- Can load native Windows DLLs via P/Invoke
- Tests perform true A-B comparison
- Validates Win32Emu matches native behavior

### Linux / macOS
- Native DLL loading is not available
- Tests only run Win32Emu implementation
- Still useful for documenting expected behavior
- Prevents tests from blocking CI/CD

## Test Categories

Generated tests include traits for filtering:

- `Category=ABTest` - All A-B comparison tests
- `Category=NeedsImplementation` - Function not yet implemented
- `Category=Stub` - Function has stub implementation
- `Function={name}` - Specific function name

## Running Generated Tests

```bash
# Run all generated tests
dotnet test --filter "Category=ABTest"

# Run tests for specific DLL
dotnet test --filter "FullyQualifiedName~Kernel32ABTests"

# Run only missing function tests
dotnet test --filter "Category=NeedsImplementation"

# Run only stub function tests
dotnet test --filter "Category=Stub"
```

## Benefits

1. **Test-Driven Development** - Write tests before implementation
2. **Behavior Documentation** - Tests document expected Win32 API behavior
3. **Regression Prevention** - Ensure implementations don't break over time
4. **Cross-Platform** - Tests work on all platforms, with A-B on Windows
5. **Incremental Progress** - Add implementations one function at a time

## Example Workflow

```bash
# 1. Generate tests for KERNEL32
dotnet run --project Win32Emu.Tools.TestGenerator docs/pages/missing-functions.json Win32Emu.Tests.Generated KERNEL32.DLL

# 2. Implement GetTempPathA in Win32Emu.Kernel32
# ... code changes ...

# 3. Update the test in Win32Emu.Tests.Generated/Kernel32ABTests.cs
# Remove Skip, add test parameters, add assertions

# 4. Run the test
dotnet test --filter "Function=GetTempPathA"

# 5. Test validates Win32Emu matches native behavior
```

## Integration with CI/CD

- Generated tests are **optional** (won't block PRs)
- Same policy as other Win32 DLL module tests
- Allows test-driven development without breaking builds
- Test results visible in CI for tracking progress

## Limitations

1. **Manual test implementation** - Generated tests are stubs that need completion
2. **Windows-only A-B** - True comparison only works on Windows
3. **No signature validation** - Doesn't verify parameter types/counts
4. **Limited generation** - Generates first 10 functions per DLL to avoid overwhelming

## Future Enhancements

- [ ] Auto-generate test parameters from function signatures
- [ ] Support for different parameter combinations
- [ ] Automatic comparison of complex return types
- [ ] Integration with Wine on Linux for A-B testing
- [ ] Generate tests for already-implemented functions
- [ ] Support for different test frameworks (NUnit, MSTest)

## See Also

- [NATIVE_DLL_ANALYSIS.md](../../docs/NATIVE_DLL_ANALYSIS.md) - Native DLL analysis feature
- [README.Tests.md](../../README.Tests.md) - Test strategy documentation
- [Win32Emu.Tools.NativeDllAnalyzer](../Win32Emu.Tools.NativeDllAnalyzer/) - Native DLL analyzer tool
- [Win32Emu.Tools.ApiStatusGenerator](../Win32Emu.Tools.ApiStatusGenerator/) - API status generator

## Contributing

To improve test generation:

1. Enhance the test scaffolding templates
2. Add better parameter inference
3. Improve cross-platform support
4. Add more detailed assertions
5. Generate documentation from tests
