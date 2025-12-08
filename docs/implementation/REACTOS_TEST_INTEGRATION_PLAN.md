# Using ReactOS Tests in Win32Emu

**For:** Win32Emu Developers  
**Last Updated:** 2025-12-08

## Quick Start

ReactOS tests provide comprehensive validation for Win32 API implementations. This guide shows you how to use them.

### Running Tests

```bash
# Run all ReactOS tests
dotnet test Win32Emu.Tests.ReactOS

# Run tests for specific module
dotnet test Win32Emu.Tests.ReactOS --filter "Module=Kernel32"

# Run test for specific function
dotnet test Win32Emu.Tests.ReactOS --filter "Function=GetModuleFileName"
```

### Understanding Results

Tests output in xUnit format with additional ReactOS test details:

```
✓ GetModuleFileName_ReactOSTest
  45/45 tests passed, 0 failed, 0 skipped

✗ CreateProcess_ReactOSTest
  38/45 tests passed, 7 failed, 0 skipped
  
  Failures:
  - kernel32_test.c:123: Test failed: Expected 0, got 5
  - kernel32_test.c:456: Test failed: Process not created
```

## Adding New Tests

### Step 1: Compile ReactOS Test

```bash
# Option A: Use build script
./scripts/build-reactos-tests.sh

# Option B: Manual compilation
i686-w64-mingw32-gcc \
    -o tests/reactos/kernel32_MyFunction.exe \
    external/reactos/modules/rostests/apitests/kernel32/MyFunction.c \
    -I external/reactos/modules/rostests/apitests/include \
    -I external/reactos/sdk/include \
    -lkernel32 -luser32
```

### Step 2: Add Test to Suite

**File:** `Win32Emu.Tests.ReactOS/Kernel32ReactOSTests.cs`

```csharp
[Fact]
[Trait("Function", "MyFunction")]
public void MyFunction_ReactOSTest()
{
    var result = _runner.Run("kernel32_MyFunction.exe");
    Assert.False(result.IsError, result.ErrorMessage);
    Assert.True(result.AllPassed, result.Summary);
}
```

### Step 3: Run and Debug

```bash
# Run the new test
dotnet test --filter "Function=MyFunction"
```

If the test fails, the output will show which specific assertions failed in the ReactOS test.

## Debugging Failed Tests

### Step 1: Run Test Manually

```bash
# Navigate to test directory
cd tests/reactos

# Run Win32Emu with the test executable
dotnet run --project ../../Win32Emu/Win32Emu.csproj -- kernel32_MyFunction.exe
```

### Step 2: Examine Output

Look for Wine test framework output:
- Lines with "Test failed" show assertion failures
- File:line references point to source in ReactOS repo
- Compare expected vs actual values

### Step 3: Fix Implementation

1. Identify the failing API in Win32Emu (e.g., `Win32Emu.Kernel32`)
2. Compare with ReactOS expectations
3. Fix the implementation
4. Re-run the test

### Step 4: Iterate

```bash
# Fix -> Test -> Repeat
vim Win32Emu.Kernel32/Kernel32Module.cs
dotnet test --filter "Function=MyFunction"
```

## Common Issues

### Test Executable Not Found

```
ERROR: Test executable not found: tests/reactos/kernel32_MyFunction.exe
```

**Solution:** Compile the test first
```bash
./scripts/build-reactos-tests.sh
```

### Test Crashes Win32Emu

```
ERROR: Unhandled exception in emulator
```

**Causes:**
- Unimplemented API called by test
- CPU emulation bug
- Memory corruption

**Solution:**
1. Run with `--debug` flag to see which API is called
2. Check logs for last API before crash
3. Implement missing API or fix bug

### All Tests Fail

```
38/45 tests passed, 7 failed, 0 skipped
```

**This is normal!** Win32Emu may not implement 100% of Windows APIs yet.

**What to do:**
1. Check if failures are in known unimplemented APIs
2. Focus on high-priority failures
3. Mark as known failures in test attributes

### Test Hangs

```
Test running for > 30 seconds...
```

**Causes:**
- Infinite loop in emulator
- Waiting for input/event that never comes
- Deadlock

**Solution:**
1. Add timeout to test: `[Fact(Timeout = 10000)]` (10 seconds)
2. Debug with interactive debugger
3. Check for blocking operations

## Best Practices

### 1. Start with Simple Tests

Begin with basic API tests:
- GetVersion
- GetModuleFileName
- String operations

Then move to complex tests:
- CreateProcess
- Window management
- Threading

### 2. Mark Expected Failures

If a test is known to fail due to unimplemented features:

```csharp
[Fact(Skip = "CreateThread not fully implemented yet")]
[Trait("Category", "KnownFailure")]
public void CreateThread_ReactOSTest()
{
    // Test will be skipped
}
```

### 3. Group Related Tests

Organize tests by module and functionality:

```csharp
public class Kernel32_MemoryTests
{
    [Fact] public void GlobalAlloc_ReactOSTest() { ... }
    [Fact] public void HeapAlloc_ReactOSTest() { ... }
    [Fact] public void VirtualAlloc_ReactOSTest() { ... }
}
```

### 4. Document Test Behavior

Add comments explaining what the test validates:

```csharp
/// <summary>
/// Validates GetModuleFileName behavior:
/// - Returns full path to executable
/// - Handles NULL module handle (current executable)
/// - Properly handles buffer size limits
/// </summary>
[Fact]
public void GetModuleFileName_ReactOSTest() { ... }
```

### 5. Use Test Output for Implementation

When implementing a new API, check ReactOS test to understand:
- Expected behavior
- Edge cases
- Error conditions
- Return values

## Integration with Development Workflow

### When Implementing New API

1. **Find ReactOS test** for the API
2. **Compile and run** the test (it will fail)
3. **Implement API** in Win32Emu
4. **Run test** to validate
5. **Iterate** until passing

### When Fixing Bugs

1. **Check if ReactOS test exists** for the buggy API
2. **Run test** to reproduce
3. **Fix bug**
4. **Verify test passes**

### When Refactoring

1. **Run full ReactOS test suite** before refactoring
2. **Record pass/fail status**
3. **Refactor code**
4. **Run tests again**
5. **Ensure no regressions** (same or better pass rate)

## CI/CD Integration

ReactOS tests run on every PR but are **non-blocking**:
- Tests run automatically
- Results are reported in PR
- Failures don't block merge (many APIs are unimplemented)
- Use results to track progress

### Viewing CI Results

In your PR, check the "ReactOS Tests" workflow:
- Green checkmark: Tests ran successfully (may have expected failures)
- View logs to see pass/fail breakdown
- Download test artifacts for detailed results

## Performance Considerations

### Test Execution Time

ReactOS tests can be slow because they:
- Load full PE executable
- Run complete emulator stack
- Test comprehensive scenarios

**Tips:**
- Run subset of tests during development
- Use `--filter` to run specific tests
- Parallelize test execution in CI

### Resource Usage

Tests may consume significant:
- Memory (emulator instances)
- CPU (JIT compilation)
- Disk (PE loading)

**Tips:**
- Close other applications during testing
- Use release builds for faster execution
- Consider test timeouts

## Advanced Usage

### Custom Test Runner Options

```csharp
var runner = new ReactOSTestRunner();
runner.Timeout = TimeSpan.FromMinutes(5);
runner.CaptureDebugOutput = true;
runner.EmulatorOptions = new EmulatorOptions
{
    EnableLogging = true,
    LogLevel = LogLevel.Debug
};

var result = runner.Run("kernel32_MyFunction.exe");
```

### Analyzing Test Results Programmatically

```csharp
var result = runner.Run("kernel32_MyFunction.exe");

// Check specific metrics
Console.WriteLine($"Pass rate: {result.Passed}/{result.Total}");
Console.WriteLine($"Failures: {result.Failed}");

// Examine failure messages
foreach (var failure in result.FailureMessages)
{
    Console.WriteLine($"  - {failure}");
}

// Check for specific error patterns
if (result.FailureMessages.Any(f => f.Contains("Access Violation")))
{
    Console.WriteLine("Memory access issue detected!");
}
```

### Creating Test Collections

Group slow tests together:

```csharp
[Collection("SlowTests")]
public class Kernel32_ProcessTests
{
    // These tests run sequentially, not in parallel
    [Fact] public void CreateProcess_ReactOSTest() { ... }
    [Fact] public void TerminateProcess_ReactOSTest() { ... }
}
```

## Contributing

### Adding More ReactOS Tests

1. Browse ReactOS test repository
2. Identify valuable tests for Win32Emu
3. Compile and add to test suite
4. Submit PR with new tests

### Improving Test Runner

Ideas for contributions:
- Better error messages
- Performance optimizations
- More detailed result parsing
- Test result visualization

### Documentation

Help improve:
- This guide
- Troubleshooting tips
- Common failure patterns
- Implementation examples

## Resources

- [ReactOS Test Integration Research](../research/REACTOS_TEST_INTEGRATION.md)
- [Implementation Plan](../implementation/REACTOS_TEST_INTEGRATION_PLAN.md)
- [ReactOS API Tests Source](https://github.com/reactos/reactos/tree/master/modules/rostests/apitests)
- [Wine Test Framework Documentation](https://wiki.winehq.org/Wine_Testing_Framework)

## FAQ

**Q: Why don't all tests pass?**  
A: Win32Emu is still implementing Win32 APIs. Tests help track progress.

**Q: Should I fix all failing tests?**  
A: Focus on high-priority APIs first. Mark others as known failures.

**Q: Can I run tests on Linux/macOS?**  
A: Yes! Win32Emu is cross-platform. Tests work everywhere.

**Q: How do I know which APIs to implement?**  
A: Run ReactOS tests to see which APIs are most commonly needed.

**Q: What if a test is flaky?**  
A: Mark with `[Trait("Category", "Flaky")]` and investigate the cause.

**Q: Can I modify ReactOS tests?**  
A: You can, but it's better to fix Win32Emu to match Windows behavior.

## Support

- **Issues:** Report bugs in Win32Emu GitHub issues
- **Discussions:** Ask questions in GitHub Discussions
- **Discord:** Join Win32Emu community Discord (if available)

---

**Next Steps:**
1. Run your first ReactOS test
2. Implement a new API guided by tests
3. Contribute new tests to the suite

Happy testing! 🎉
