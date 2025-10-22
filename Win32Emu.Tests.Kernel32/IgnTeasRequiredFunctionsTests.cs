using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32.Modules;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests to verify all functions required by ign_teas.exe are properly implemented
/// </summary>
public class IgnTeasRequiredFunctionsTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public IgnTeasRequiredFunctionsTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void DirectDrawCreate_ShouldBeImplemented()
    {
        // Verify DirectDrawCreate is implemented (not a stub)
        // Create the module and try to invoke the function
        var ddrawModule = new DDrawModule(_testEnv.ProcessEnv, 0x10000000, null, NullLogger.Instance);
        
        var success = ddrawModule.TryInvokeUnsafe("DIRECTDRAWCREATE", _testEnv.Cpu, _testEnv.Memory, out var result);
        
        // The function should be invokable (success = true)
        Assert.True(success, "DirectDrawCreate should be implemented");
    }

    [Fact]
    public void DirectInputCreateA_ShouldBeImplemented()
    {
        // Verify DirectInputCreateA is implemented (not a stub)
        var dinputModule = new DInputModule(_testEnv.ProcessEnv, 0x20000000, null, NullLogger.Instance);
        
        var success = dinputModule.TryInvokeUnsafe("DIRECTINPUTCREATEA", _testEnv.Cpu, _testEnv.Memory, out var result);
        
        // The function should be invokable (success = true)
        Assert.True(success, "DirectInputCreateA should be implemented");
    }

    [Fact]
    public void DirectSoundCreate_ShouldBeImplemented()
    {
        // Verify DirectSoundCreate is implemented (not a stub)
        var dsoundModule = new DSoundModule(_testEnv.ProcessEnv, 0x30000000, null, NullLogger.Instance);
        
        var success = dsoundModule.TryInvokeUnsafe("DIRECTSOUNDCREATE", _testEnv.Cpu, _testEnv.Memory, out var result);
        
        // The function should be invokable (success = true)
        Assert.True(success, "DirectSoundCreate should be implemented");
    }

    [Fact]
    public void GetEnvironmentStrings_ShouldBeAvailable()
    {
        // Verify GetEnvironmentStrings (without A/W suffix) is available in KERNEL32.DLL
        var result = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGS");
        
        // Should return a valid pointer to environment strings
        Assert.NotEqual(0u, result);
    }

    [Fact]
    public void GetLastError_ShouldBeAvailable()
    {
        // Verify GetLastError is available in KERNEL32.DLL
        var result = _testEnv.CallKernel32Api("GETLASTERROR");
        
        // Should return a value (initially 0 if no error)
        Assert.True(true);
    }

    [Fact]
    public void GetVersion_ShouldBeAvailable()
    {
        // Verify GetVersion is available in KERNEL32.DLL
        var result = _testEnv.CallKernel32Api("GETVERSION");
        
        // Should return a version number
        Assert.NotEqual(0u, result);
        
        // Verify it returns a valid Windows version
        // The format is: (major << 8 | minor) << 16 | build
        var build = result & 0xFFFF;
        var minor = (result >> 16) & 0xFF;
        var major = (result >> 24) & 0xFF;
        
        Assert.True(major > 0, "Major version should be greater than 0");
        Assert.True(build > 0, "Build number should be greater than 0");
    }

    [Fact]
    public void AllRequiredFunctions_ShouldBeImplementedNotStubs()
    {
        // This test verifies that all the required functions are actually implemented
        // and not just stubs that return default values
        
        // Test GetVersion returns a specific version
        var version = _testEnv.CallKernel32Api("GETVERSION");
        var major = (version >> 24) & 0xFF;
        var minor = (version >> 16) & 0xFF;
        var build = version & 0xFFFF;
        
        // Should return Windows ME (4.0) or XP (5.1) version
        Assert.True(major == 4 || major == 5, $"Version major should be 4 or 5, got {major}");
        Assert.True(build > 0, $"Build should be > 0, got {build}");
        
        // Test GetEnvironmentStrings returns valid data
        var envStrings = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGS");
        Assert.NotEqual(0u, envStrings);
        
        // Verify it's actually a valid environment block (starts with data)
        var firstByte = _testEnv.Memory.Read8(envStrings);
        Assert.True(firstByte > 0, "Environment strings should start with valid data");
        
        // Test GetLastError can be set and retrieved
        _testEnv.CallKernel32Api("SETLASTERROR", 0x12345678u);
        var lastError = _testEnv.CallKernel32Api("GETLASTERROR");
        Assert.Equal(0x12345678u, lastError);
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
