using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for threading and TLS (Thread Local Storage) functions
/// </summary>
public sealed class ThreadingTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public ThreadingTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void GetCurrentThreadId_ShouldReturnNonZero()
    {
        // Act
        var threadId = _testEnv.CallKernel32Api("GETCURRENTTHREADID");

        // Assert
        Assert.NotEqual(0u, threadId);
        Assert.Equal(1u, threadId); // Main thread should have ID 1
    }

    [Fact]
    public void TlsAlloc_ShouldReturnValidIndex()
    {
        // Act
        var tlsIndex = _testEnv.CallKernel32Api("TLSALLOC");

        // Assert
        Assert.Equal(0u, tlsIndex); // First TLS index should be 0
    }

    [Fact]
    public void TlsSetValue_And_TlsGetValue_ShouldWork()
    {
        // Arrange
        var tlsIndex = _testEnv.CallKernel32Api("TLSALLOC");
        var testValue = 0x12345678u;

        // Act - Set value
        var setResult = _testEnv.CallKernel32Api("TLSSETVALUE", tlsIndex, testValue);
        
        // Assert - Set should succeed
        Assert.Equal(1u, setResult); // TRUE = 1

        // Act - Get value
        var getValue = _testEnv.CallKernel32Api("TLSGETVALUE", tlsIndex);

        // Assert - Should get same value back
        Assert.Equal(testValue, getValue);
    }

    [Fact]
    public void TlsGetValue_OnUnsetIndex_ShouldReturnZero()
    {
        // Arrange
        var tlsIndex = _testEnv.CallKernel32Api("TLSALLOC");

        // Act - Get value without setting it first
        var getValue = _testEnv.CallKernel32Api("TLSGETVALUE", tlsIndex);

        // Assert - Should return 0 for unset value
        Assert.Equal(0u, getValue);
    }

    [Fact]
    public void TlsSetValue_OnInvalidIndex_ShouldReturnFalse()
    {
        // Arrange
        var invalidIndex = 999u;
        var testValue = 0x12345678u;

        // Act
        var setResult = _testEnv.CallKernel32Api("TLSSETVALUE", invalidIndex, testValue);

        // Assert - Should fail (FALSE = 0)
        Assert.Equal(0u, setResult);
    }

    [Fact]
    public void TlsFree_ShouldWork()
    {
        // Arrange
        var tlsIndex = _testEnv.CallKernel32Api("TLSALLOC");

        // Act
        var freeResult = _testEnv.CallKernel32Api("TLSFREE", tlsIndex);

        // Assert - Should succeed
        Assert.Equal(1u, freeResult); // TRUE = 1
    }

    [Fact]
    public void TlsFree_OnInvalidIndex_ShouldReturnFalse()
    {
        // Arrange
        var invalidIndex = 999u;

        // Act
        var freeResult = _testEnv.CallKernel32Api("TLSFREE", invalidIndex);

        // Assert - Should fail (FALSE = 0)
        Assert.Equal(0u, freeResult);
    }

    [Fact]
    public void CreateThread_ShouldReturnValidHandle()
    {
        // Arrange
        var stackSize = 0x8000u;
        var startAddress = 0x00401000u; // Some arbitrary address
        var parameter = 0u;
        var creationFlags = 0u;
        var threadIdPtr = 0u;

        // Act
        var threadHandle = _testEnv.CallKernel32Api("CREATETHREAD", 
            0u, // lpThreadAttributes
            stackSize,
            startAddress,
            parameter,
            creationFlags,
            threadIdPtr
        );

        // Assert
        Assert.NotEqual(0u, threadHandle);
    }

    [Fact]
    public void MultipleTlsAlloc_ShouldReturnDifferentIndices()
    {
        // Act
        var tlsIndex1 = _testEnv.CallKernel32Api("TLSALLOC");
        var tlsIndex2 = _testEnv.CallKernel32Api("TLSALLOC");
        var tlsIndex3 = _testEnv.CallKernel32Api("TLSALLOC");

        // Assert
        Assert.NotEqual(tlsIndex1, tlsIndex2);
        Assert.NotEqual(tlsIndex2, tlsIndex3);
        Assert.NotEqual(tlsIndex1, tlsIndex3);
    }

    [Fact]
    public void TlsValues_ShouldBeIndependent()
    {
        // Arrange
        var tlsIndex1 = _testEnv.CallKernel32Api("TLSALLOC");
        var tlsIndex2 = _testEnv.CallKernel32Api("TLSALLOC");
        var value1 = 0x11111111u;
        var value2 = 0x22222222u;

        // Act - Set different values for different TLS indices
        _testEnv.CallKernel32Api("TLSSETVALUE", tlsIndex1, value1);
        _testEnv.CallKernel32Api("TLSSETVALUE", tlsIndex2, value2);

        // Assert - Each TLS index should retain its own value
        var getValue1 = _testEnv.CallKernel32Api("TLSGETVALUE", tlsIndex1);
        var getValue2 = _testEnv.CallKernel32Api("TLSGETVALUE", tlsIndex2);

        Assert.Equal(value1, getValue1);
        Assert.Equal(value2, getValue2);
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
