# Win32Emu.Tests.ReactOS

ReactOS test integration for Win32Emu - runs ReactOS API test executables to validate Win32 API implementations.

## Overview

This test project implements the ReactOS test integration strategy documented in `docs/research/REACTOS_TEST_INTEGRATION.md`. It runs ReactOS test executables (compiled with Wine test framework) directly in Win32Emu and parses the test output.

## Test Structure

- **ReactOSTestRunner** - Loads and executes ReactOS test executables in Win32Emu
- **WineTestParser** - Parses Wine test framework output format
- **User32ReactOSTests** - User32.dll API tests (primary focus)
- More test classes can be added for other DLLs

## Running Tests

```bash
# Run all ReactOS tests
dotnet test Win32Emu.Tests.ReactOS

# Run User32 tests only
dotnet test --filter "Module=User32"

# Run specific test
dotnet test --filter "Function=User32_ApiTest"
```

## Test Executables

Test executables are located in `EXEs/ApiTests/`:
- `user32_apitest.exe` - Main User32 API tests from ReactOS
- `user32_dynamic_apitest.exe` - Dynamic User32 tests
- `user32_apitest_menuui.exe` - Menu UI tests
- `user32_winetest.exe` - Wine project tests (reference)

## Test Behavior

These tests are marked with:
- `[assembly: Trait("Category", "DllModuleTests")]` - Optional in CI (non-blocking)
- `[assembly: Trait("Category", "ReactOSTests")]` - ReactOS test category

Tests will:
1. Load the ReactOS test executable in Win32Emu
2. Execute the test with a timeout (default: 120 seconds)
3. Capture console output
4. Parse Wine test framework output
5. Report results (passed/failed/skipped counts)

Tests do NOT assert all tests pass (many APIs may not be implemented yet). Instead, they:
- Report test results for tracking
- Fail only if there's an error loading/running the executable
- Provide visibility into implementation progress

## Expected Behavior

Since Win32Emu is still implementing Win32 APIs, it's normal for some tests to fail:
- ✅ Test executable loads and runs
- ✅ Test results are parsed and reported
- ⚠️ Some tests may fail (unimplemented APIs)
- ⚠️ Some tests may timeout (missing functionality)

The goal is to track progress over time as more APIs are implemented.

## Adding Tests for Other DLLs

To add tests for other DLLs (e.g., Kernel32, GDI32):

1. Create a new test class (e.g., `Kernel32ReactOSTests.cs`)
2. Use the same pattern as `User32ReactOSTests`
3. Reference the appropriate test executable (e.g., `kernel32_apitest.exe`)
4. Add `[Trait("Module", "Kernel32")]` to the class

Example:
```csharp
[Trait("Module", "Kernel32")]
public class Kernel32ReactOSTests : IDisposable
{
    private readonly ReactOSTestRunner _runner;
    
    public Kernel32ReactOSTests()
    {
        _runner = new ReactOSTestRunner();
    }
    
    [Fact]
    public void Kernel32_ApiTest_ShouldExecute()
    {
        var result = _runner.Run("kernel32_apitest.exe");
        Assert.False(result.IsError, result.ErrorMessage);
    }
}
```

## Debugging

To debug a failing test:

1. Run Win32Emu directly with the test executable:
   ```bash
   dotnet run --project Win32Emu -- EXEs/ApiTests/user32_apitest.exe
   ```

2. Enable debug logging in the test:
   ```csharp
   builder.SetMinimumLevel(LogLevel.Debug);
   ```

3. Check test output for specific failure messages

## Documentation

For more details, see:
- [Research Document](../docs/research/REACTOS_TEST_INTEGRATION.md) - Strategy and analysis
- [Implementation Plan](../docs/implementation/REACTOS_TEST_INTEGRATION_PLAN.md) - Developer guide
- [Quick Reference](../docs/guides/REACTOS_TESTS_QUICK_REFERENCE.md) - Command reference

## Status

- ✅ Infrastructure implemented (ReactOSTestRunner, WineTestParser)
- ✅ User32 tests integrated
- ⏳ Additional DLL tests (can be added as needed)

## Contributing

To improve ReactOS test integration:
1. Add tests for more DLLs
2. Improve Wine test output parsing
3. Add better error handling
4. Enhance test result reporting
