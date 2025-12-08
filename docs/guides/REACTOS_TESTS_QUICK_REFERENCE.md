# ReactOS Test Quick Reference

## For Developers: Using ReactOS Tests

### Run Tests

```bash
# All ReactOS tests
dotnet test Win32Emu.Tests.ReactOS

# Specific module
dotnet test --filter "Module=Kernel32"
dotnet test --filter "Module=User32"

# Specific function
dotnet test --filter "Function=GetModuleFileName"
```

### Add New Test

1. **Compile ReactOS test to PE:**
   ```bash
   ./scripts/build-reactos-tests.sh kernel32 GetModuleFileName
   ```

2. **Add to test class:**
   ```csharp
   [Fact]
   [Trait("Function", "GetModuleFileName")]
   public void GetModuleFileName_ReactOSTest()
   {
       var result = _runner.Run("kernel32_GetModuleFileName.exe");
       Assert.True(result.AllPassed, result.Summary);
   }
   ```

3. **Run:**
   ```bash
   dotnet test --filter "Function=GetModuleFileName"
   ```

### Understand Test Failures

```
✗ GetModuleFileName_ReactOSTest
  38/45 tests passed, 7 failed, 0 skipped
  
  Failures:
  - kernel32_test.c:123: Test failed: Expected 0, got 5
  - kernel32_test.c:456: Test failed: Buffer not filled
```

**Action:**
1. Check which specific assertions failed
2. Look at ReactOS source to understand expected behavior
3. Fix Win32Emu implementation
4. Re-run test

### Debug Failed Test

```bash
# Run test executable directly in Win32Emu
dotnet run --project Win32Emu -- tests/reactos/kernel32_GetModuleFileName.exe

# With debug output
dotnet run --project Win32Emu -- --debug tests/reactos/kernel32_GetModuleFileName.exe
```

## For Maintainers: Integration Status

### Current Status
- ✅ Research complete
- ✅ Documentation created
- ⏳ Implementation pending

### What's Ready
- Integration strategy defined
- Developer documentation complete
- CI/CD approach planned

### What's Next
1. Implement `ReactOSTestRunner` class
2. Implement `WineTestParser` class
3. Create `Win32Emu.Tests.ReactOS` project
4. Compile initial test set
5. Add to CI pipeline

### Documents
- [Research](../research/REACTOS_TEST_INTEGRATION.md)
- [Implementation Plan](../implementation/REACTOS_TEST_INTEGRATION_PLAN.md)
- [Summary](../testing/REACTOS_TEST_INTEGRATION_SUMMARY.md)

## Test Categories

| Category | Count | Description |
|----------|-------|-------------|
| Kernel32 | ~60 | Process, memory, file I/O, modules |
| User32 | ~80 | Windows, messages, input, drawing |
| GDI32 | ~40 | Graphics, fonts, bitmaps |
| AdvAPI32 | ~30 | Registry, security, services |
| Shell32 | ~20 | Shell operations, file dialogs |
| Others | ~100 | Various DLLs |

## Quick Links

- [ReactOS API Tests](https://github.com/reactos/reactos/tree/master/modules/rostests/apitests)
- [Wine Test Framework](https://wiki.winehq.org/Wine_Testing_Framework)
- [Win32Emu Test Strategy](../../README.Tests.md)

## Example: Implementing API with ReactOS Test

**Goal:** Implement `GetTempPathA` API

**Steps:**

1. **Find test:**
   ```bash
   # Search ReactOS repo
   find external/reactos -name "*GetTempPath*"
   ```

2. **Compile test:**
   ```bash
   ./scripts/build-reactos-tests.sh kernel32 GetTempPath
   ```

3. **Add test (it will fail):**
   ```csharp
   [Fact]
   public void GetTempPath_ReactOSTest()
   {
       var result = _runner.Run("kernel32_GetTempPath.exe");
       Assert.True(result.AllPassed, result.Summary);
   }
   ```

4. **Run test to see failures:**
   ```bash
   dotnet test --filter "Function=GetTempPath"
   # Output: 0/15 tests passed
   ```

5. **Implement API:**
   ```csharp
   [DllModuleExport(34)]
   public uint GetTempPathA(EmulatorEnvironment env, uint nBufferLength, uint lpBuffer)
   {
       // Implementation here
   }
   ```

6. **Re-run test:**
   ```bash
   dotnet test --filter "Function=GetTempPath"
   # Output: 12/15 tests passed (getting better!)
   ```

7. **Fix issues and iterate:**
   - Read failure messages
   - Check ReactOS test source
   - Fix implementation
   - Re-run until 15/15 passed

8. **Done!** API validated against ReactOS tests.

## Common Patterns

### Pattern 1: Simple API Test
```csharp
[Fact]
[Trait("Module", "Kernel32")]
[Trait("Function", "GetVersion")]
public void GetVersion_ReactOSTest()
{
    var result = _runner.Run("kernel32_GetVersion.exe");
    Assert.True(result.AllPassed, result.Summary);
}
```

### Pattern 2: Known Failure
```csharp
[Fact(Skip = "CreateThread not fully implemented")]
[Trait("Category", "KnownFailure")]
public void CreateThread_ReactOSTest()
{
    // Test skipped until CreateThread is complete
}
```

### Pattern 3: Partial Implementation
```csharp
[Fact]
[Trait("Status", "PartialImplementation")]
public void CreateProcess_ReactOSTest()
{
    var result = _runner.Run("kernel32_CreateProcess.exe");
    
    // Not all tests pass yet, but track progress
    Assert.InRange(result.Passed, 30, result.Total);
}
```

## Wine Test Output Format

**Input (from test executable):**
```
kernel32_test.c:123: Test succeeded
kernel32_test.c:456: Test failed: expected 0, got 5
kernel32_test.c:789: Tests skipped: feature not available
Summary: 45 tests executed (43 passed, 2 failed, 0 skipped)
```

**Parsed Result:**
```csharp
result.Total = 45
result.Passed = 43
result.Failed = 2
result.Skipped = 0
result.AllPassed = false
result.Summary = "43/45 tests passed, 2 failed, 0 skipped"
result.FailureMessages = [
    "kernel32_test.c:456: Test failed: expected 0, got 5"
]
```

## Tips

### Tip 1: Start Simple
Begin with basic tests like GetVersion before complex ones like CreateProcess.

### Tip 2: Read Test Source
ReactOS test source shows expected behavior and edge cases.

### Tip 3: Track Progress
Use test results to measure implementation progress over time.

### Tip 4: Mark Known Issues
Use `[Fact(Skip = "reason")]` for known unimplemented features.

### Tip 5: Use for TDD
Write test first (it fails), implement API, test passes.

## Support

- **Issues:** GitHub Issues
- **Documentation:** See "Documents" section above
- **Community:** GitHub Discussions

---

**Status:** Documentation complete, awaiting implementation

**Last Updated:** 2025-12-08
