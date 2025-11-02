# GitHub Copilot Instructions for Win32Emu

## Project Overview

Win32Emu is a Windows 32-bit PE executable emulator written in C# (.NET 9) for running classic Windows games and applications on modern systems (Windows, Linux, macOS on x86 and ARM). It provides full x86 CPU emulation with JIT compilation, Windows API emulation (Kernel32, User32, DirectDraw, DirectInput, DirectSound, etc.), and cross-platform multimedia support through pluggable backends (SDL3, GLFW, Vulkan, Metal).

## Coding Standards

### Language and Framework
- Use C# with .NET 9 features
- Follow .editorconfig settings (tabs for indentation, tab width = 4)
- Use `var` for local variables when type is apparent or for built-in types
- Prefer block-scoped namespaces over file-scoped
- Use expression-bodied members for properties, indexers, and lambdas
- Avoid expression-bodied methods, constructors, and operators

### Logging
- **NEVER use `Console.WriteLine` for logging** - Use `ILogger` instance instead
- Example: `_logger.LogInformation("Message with {Parameter}", value);`
- Use structured logging with named parameters for better observability
- OpenTelemetry is integrated for logging, metrics, and tracing

### Naming Conventions
- Follow standard C# naming conventions (PascalCase for public members, camelCase for private fields)
- Use descriptive names that clearly indicate purpose
- For Win32 API emulation, match the original Win32 API naming (e.g., `CreateFileA`, `GetModuleHandleA`)

### Code Organization
- Keep related functionality together in logical namespaces
- Use source generators for code generation tasks (see Win32Emu.Generators)
- Place emulated Win32 DLLs in their own modules (e.g., Win32Emu.Kernel32, Win32Emu.User32)

## Testing Requirements

### Test Structure
The project uses multiple test projects organized by component:
- `Win32Emu.Tests.Emulator` - CPU emulator conformance tests (required for CI)
- `Win32Emu.Tests.Kernel32` - Kernel32.dll API tests (optional for CI)
- `Win32Emu.Tests.User32` - User32.dll API tests (optional for CI)
- `Win32Emu.Tests.Gui` - GUI component tests
- `Win32Emu.Tests.CodeGen` - Code generation tests

### Test Guidelines
- **Core emulator tests are REQUIRED** - failures block PRs (CPU, memory, instruction execution)
- **Win32 DLL module tests are OPTIONAL** - failures don't block PRs (enables test-driven development)
- Each test should be independent and isolated
- Use descriptive test names that explain the scenario (e.g., `GetVersion_ReturnsWindows95_WhenVersionIs950`)
- Include both positive and negative test cases
- Use `TestEnvironment` class for consistent test setup
- Document any deviations from expected Win32 behavior with comments
- Mark tests with appropriate categories/traits

### Test Utilities
- Use `MockCpu` to simulate CPU without full emulation
- Use `TestEnvironment` for complete test setup (memory, CPU, process environment)
- Use memory helper utilities for string handling and memory access in tests

### Running Tests
```bash
dotnet test                              # All tests
dotnet test Win32Emu.Tests.Kernel32      # Specific project
dotnet test --filter "BasicFunctionsTests"  # Specific category
```

## Security Practices

### General Security
- Validate all external input (PE files, user input, etc.)
- Never expose internal memory addresses or sensitive data in logs
- Use safe memory operations - bounds checking is critical
- Be careful with pointer arithmetic and memory access in emulated code

### Code Analysis
- SonarCloud and Codacy are integrated for code quality
- Qodana is used for additional static analysis
- Address security warnings before merging

## Build and CI/CD

### Building
```bash
dotnet restore
dotnet build --configuration Release
```

### Solution Structure
- Solution file: `Win32Emu.slnx` (XML-based solution format)
- All projects use .NET 9
- Projects include emulator core, DLL modules, tools, tests, and GUI

### CI Requirements
- All builds must succeed
- Core emulator tests must pass (CPU, memory, instruction tests)
- Win32 DLL module tests are informational only
- Code must build on Linux (Ubuntu latest) in CI

## Project-Specific Conventions

### Win32 API Emulation
- Match Win32 API signatures exactly (types, calling conventions, return values)
- Use `[DllExport]` attribute for emulated Win32 functions
- Document parameter meanings and return values
- Handle both ANSI (A suffix) and Unicode (W suffix) versions where applicable
- Set `LastError` appropriately using `env.LastError = ErrorCode.xxx`

### CPU Emulation
- Support x86 instruction sets: 8086, 286, 386, 486, Pentium
- Use hardware intrinsics (SSE, AVX, NEON) when available for performance
- Fall back to software implementation when intrinsics unavailable
- Implement CPUID to report accurate host CPU capabilities
- All CPU instructions should update flags correctly (CF, ZF, SF, OF, PF, AF)

### Memory Management
- Emulated memory is managed through `IMemory` interface
- Support multiple allocation types: `GlobalAlloc`, `HeapAlloc`, `VirtualAlloc`
- Implement proper handle management for Win32 objects
- Track allocated memory and handles for proper cleanup

### Backend System
Pluggable backends for cross-platform support:
- **SDL** (default) - SDL3-CS for rendering, audio, and input
- **GLFW** - Silk.NET.GLFW + OpenGL alternative
- **Vulkan** - Silk.NET.Vulkan (uses MoltenVK on macOS)
- **Metal** - SharpMetal for macOS hardware acceleration
- **Software** - CPU-only rendering, no GPU required

Configuration via:
- CLI: `--backend SDL|GLFW|Vulkan|Metal|Software`
- Environment: `WIN32EMU_BACKEND=SDL`
- Code: `BackendFactory.CurrentBackendType = BackendType.SDL`

### Message Dispatching
- Use `MessageDispatcher` for type-safe Win32 message handling
- Register handlers with lambda-based, zero-allocation approach
- Create strongly-typed message classes for different message types
- See `docs/implementation/MESSAGE_DISPATCHER_IMPLEMENTATION.md` for details

### Documentation
- Maintain implementation docs in `docs/implementation/`
- Create user guides in `docs/guides/`
- Include usage examples in `docs/examples/`
- Update README files when adding major features
- Document breaking changes and migration guides

### JIT and Performance
- JIT-compiled code is cached to disk for faster subsequent runs
- Support precompilation for improved startup performance
- Use .NET intrinsics for CPU instruction acceleration (SSE, AVX, NEON)
- See `docs/implementation/JIT_CACHE_IMPLEMENTATION.md`

### Debugging Features
The emulator includes extensive debugging capabilities:
- Enhanced debugging mode (`--debug`)
- Interactive debugger (`--interactive-debug`)
- GDB server for Ghidra/IDA integration (`--gdb-server`)
- Remote file I/O when VFS is initialized
- OpenTelemetry for observability (`--telemetry-console` or `--telemetry-otlp`)

## Common Patterns

### Creating New Win32 API Functions
```csharp
[DllExport("MyFunction", CallingConvention = CallingConvention.Winapi)]
public static uint MyFunction(EmulatorEnvironment env, uint param1, IntPtr param2)
{
    // Validate parameters
    if (param2 == IntPtr.Zero)
    {
        env.LastError = ErrorCode.ERROR_INVALID_PARAMETER;
        return 0;
    }
    
    // Implementation
    _logger.LogDebug("MyFunction called with {Param1}", param1);
    
    // Return success
    return 1;
}
```

### Error Handling
```csharp
try
{
    // Implementation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error in MyFunction");
    env.LastError = ErrorCode.ERROR_GEN_FAILURE;
    return 0;
}
```

### Adding Tests
```csharp
[Fact]
public void MyFunction_ReturnsExpectedValue_WhenParametersAreValid()
{
    // Arrange
    using var testEnv = new TestEnvironment();
    var param1 = 42u;
    
    // Act
    var result = MyModule.MyFunction(testEnv.Environment, param1, IntPtr.Zero);
    
    // Assert
    Assert.Equal(expectedValue, result);
}
```

## Dependencies

### Approved Package Ecosystems
- NuGet packages only
- Prefer packages from Microsoft, .NET Foundation, or well-maintained open-source projects
- For multimedia: SDL3-CS (preferred), Silk.NET, SharpMetal
- For testing: xUnit
- For logging: Microsoft.Extensions.Logging, OpenTelemetry

### Adding New Dependencies
- Check if the functionality is already available in existing dependencies
- Consider package maintenance status and community support
- Update project files and document why the dependency is needed
- Run security checks on new dependencies

## Additional Resources

- Main README: `/README.md`
- Test Strategy: `/README.Tests.md`
- Backend Migration: `/docs/implementation/SILK_NET_MIGRATION.md`
- CPU Intrinsics: `/docs/implementation/INTRINSICS.md`
- Message Dispatcher: `/docs/implementation/MESSAGE_DISPATCHER_IMPLEMENTATION.md`
- Debugging Guide: `/docs/guides/DEBUGGING_GUIDE.md`
- OpenTelemetry: `/docs/guides/OPENTELEMETRY_USAGE.md`

## When Working on Issues

1. Read relevant documentation in `/docs` before making changes
2. Follow existing patterns in the codebase
3. Add or update tests as appropriate (DLL tests are optional, core tests are required)
4. Update documentation if adding new features
5. Test on multiple backends if changing rendering/audio/input code
6. Verify builds and tests pass before submitting
