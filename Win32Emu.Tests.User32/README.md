# Win32Emu.Tests.User32

## Template for User32.dll Testing

This project serves as a template for testing User32.dll emulation in Win32Emu.

### Setup Instructions

1. **Copy Test Infrastructure**: Copy the `TestInfrastructure` directory from `Win32Emu.Tests.Kernel32` to this project
2. **Adapt TestEnvironment**: Modify `TestEnvironment.cs` to use a User32Module instead of Kernel32Module
3. **Create Test Classes**: Follow the pattern from Kernel32 tests:
   - WindowTests - CreateWindow, DestroyWindow, ShowWindow, etc.
   - MessageTests - PostMessage, SendMessage, PeekMessage, etc.
   - InputTests - GetAsyncKeyState, mouse functions, etc.

### Example Test Structure

```csharp
public class WindowTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public WindowTests()
    {
        _testEnv = new TestEnvironment(); // Modified for User32
    }

    [Fact]
    public void CreateWindowA_WithValidParameters_ShouldReturnWindowHandle()
    {
        // Test implementation
    }

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}
```

### Prerequisites

Before implementing User32 tests:
1. Ensure User32Module exists in the main Win32Emu project
2. Implement the User32 API functions you want to test
3. Follow the same patterns established in Win32Emu.Tests.Kernel32

### Reference

Use `Win32Emu.Tests.Kernel32` as a reference for:
- Test infrastructure setup
- Test naming conventions
- MockCpu usage patterns
- Memory and string handling utilities