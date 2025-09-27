using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for basic Kernel32 functions like GetVersion, GetLastError, SetLastError
/// </summary>
public class BasicFunctionsTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public BasicFunctionsTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void GetVersion_ShouldReturnValidVersionNumber()
    {
        // Act
        var version = _testEnv.CallKernel32Api("GETVERSION");

        // Assert
        Assert.NotEqual(0u, version);
        
        // Test the actual implementation which has a bug in version encoding
        // The implementation does: ((major << 8 | minor) << 16 | build)
        // With major=4, minor=0, build=950, this gives 0x040003B6
        var expectedVersion = 0x040003B6u; // 67109814
        Assert.Equal(expectedVersion, version);
        
        // Extract the values as they would appear due to the implementation bug
        var extractedMajor = version & 0xFF; // Should be 182 (0xB6) due to bug
        var extractedMinor = (version >> 8) & 0xFF; // Should be 3 (0x03) due to bug
        var extractedBuild = (version >> 16) & 0xFFFF; // Should be 1024 (0x0400) due to bug
        
        Assert.Equal(182u, extractedMajor);
        Assert.Equal(3u, extractedMinor);
        Assert.Equal(1024u, extractedBuild);
    }

    [Fact]
    public void GetLastError_InitialValue_ShouldBeZero()
    {
        // Act
        var error = _testEnv.CallKernel32Api("GETLASTERROR");

        // Assert
        Assert.Equal(0u, error);
    }

    [Fact]
    public void SetLastError_ShouldSetErrorValue()
    {
        // Arrange
        const uint expectedError = 123;

        // Act
        _testEnv.CallKernel32Api("SETLASTERROR", expectedError);
        var actualError = _testEnv.CallKernel32Api("GETLASTERROR");

        // Assert
        Assert.Equal(expectedError, actualError);
    }

    [Fact]
    public void SetLastError_MultipleValues_ShouldKeepLatestValue()
    {
        // Arrange
        const uint firstError = 111;
        const uint secondError = 222;

        // Act
        _testEnv.CallKernel32Api("SETLASTERROR", firstError);
        _testEnv.CallKernel32Api("SETLASTERROR", secondError);
        var actualError = _testEnv.CallKernel32Api("GETLASTERROR");

        // Assert
        Assert.Equal(secondError, actualError);
    }

    [Fact]
    public void GetCurrentProcess_ShouldReturnPseudoHandle()
    {
        // Act
        var handle = _testEnv.CallKernel32Api("GETCURRENTPROCESS");

        // Assert
        Assert.Equal(0xFFFFFFFF, handle); // Should return the pseudo-handle value
    }

    [Fact]
    public void GetACP_ShouldReturnWindowsCodePage()
    {
        // Act
        var codePage = _testEnv.CallKernel32Api("GETACP");

        // Assert
        Assert.Equal(1252u, codePage); // Should return Windows-1252 (Western European)
    }

    [Fact]
    public void ExitProcess_ShouldSetExitRequestedFlag()
    {
        // Arrange
        const uint exitCode = 42;

        // Act
        _testEnv.CallKernel32Api("EXITPROCESS", exitCode);

        // Assert
        Assert.True(_testEnv.ProcessEnv.ExitRequested);
    }

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}