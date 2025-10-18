using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for threading and TLS (Thread Local Storage) functions
/// </summary>
public sealed class ThreadingTests : IDisposable
{
    private readonly TestEnvironment _testEnv;
    
    // Constants for CRITICAL_SECTION structure
    private const uint CRITICAL_SECTION_SIZE = 24;
    private const uint CRITICAL_SECTION_UNLOCKED = unchecked((uint)-1);

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

    [Fact]
    public void InitializeCriticalSection_ShouldInitializeStructure()
    {
        // Arrange - Allocate memory for CRITICAL_SECTION (24 bytes)
        var criticalSectionAddr = _testEnv.AllocateMemory(CRITICAL_SECTION_SIZE);

        // Act - Initialize the critical section
        _testEnv.CallKernel32Api("INITIALIZECRITICALSECTION", criticalSectionAddr);

        // Assert - Verify structure is properly initialized
        var lockCount = _testEnv.Memory.Read32(criticalSectionAddr + 4);
        var recursionCount = _testEnv.Memory.Read32(criticalSectionAddr + 8);
        var owningThread = _testEnv.Memory.Read32(criticalSectionAddr + 12);

        Assert.Equal(CRITICAL_SECTION_UNLOCKED, lockCount); // -1 means unlocked
        Assert.Equal(0u, recursionCount); // Initially 0
        Assert.Equal(0u, owningThread); // Initially NULL
    }

    [Fact]
    public void EnterCriticalSection_ShouldAcquireLock()
    {
        // Arrange
        var criticalSectionAddr = _testEnv.AllocateMemory(CRITICAL_SECTION_SIZE);
        _testEnv.CallKernel32Api("INITIALIZECRITICALSECTION", criticalSectionAddr);

        // Act - Enter critical section
        _testEnv.CallKernel32Api("ENTERCRITICALSECTION", criticalSectionAddr);

        // Assert - Verify the lock is acquired
        var lockCount = _testEnv.Memory.Read32(criticalSectionAddr + 4);
        var recursionCount = _testEnv.Memory.Read32(criticalSectionAddr + 8);
        var owningThread = _testEnv.Memory.Read32(criticalSectionAddr + 12);

        Assert.Equal(0u, lockCount); // 0 means locked once
        Assert.Equal(1u, recursionCount); // First entry
        Assert.NotEqual(0u, owningThread); // Should be owned by a thread
    }

    [Fact]
    public void EnterCriticalSection_MultipleTimes_ShouldIncrementRecursion()
    {
        // Arrange
        var criticalSectionAddr = _testEnv.AllocateMemory(CRITICAL_SECTION_SIZE);
        _testEnv.CallKernel32Api("INITIALIZECRITICALSECTION", criticalSectionAddr);

        // Act - Enter critical section multiple times
        _testEnv.CallKernel32Api("ENTERCRITICALSECTION", criticalSectionAddr);
        _testEnv.CallKernel32Api("ENTERCRITICALSECTION", criticalSectionAddr);
        _testEnv.CallKernel32Api("ENTERCRITICALSECTION", criticalSectionAddr);

        // Assert - Verify recursion count
        var recursionCount = _testEnv.Memory.Read32(criticalSectionAddr + 8);
        Assert.Equal(3u, recursionCount); // Entered 3 times
    }

    [Fact]
    public void LeaveCriticalSection_ShouldReleaseLock()
    {
        // Arrange
        var criticalSectionAddr = _testEnv.AllocateMemory(CRITICAL_SECTION_SIZE);
        _testEnv.CallKernel32Api("INITIALIZECRITICALSECTION", criticalSectionAddr);
        _testEnv.CallKernel32Api("ENTERCRITICALSECTION", criticalSectionAddr);

        // Act - Leave critical section
        _testEnv.CallKernel32Api("LEAVECRITICALSECTION", criticalSectionAddr);

        // Assert - Verify the lock is released
        var lockCount = _testEnv.Memory.Read32(criticalSectionAddr + 4);
        var recursionCount = _testEnv.Memory.Read32(criticalSectionAddr + 8);
        var owningThread = _testEnv.Memory.Read32(criticalSectionAddr + 12);

        Assert.Equal(CRITICAL_SECTION_UNLOCKED, lockCount); // -1 means unlocked
        Assert.Equal(0u, recursionCount); // Back to 0
        Assert.Equal(0u, owningThread); // No longer owned
    }

    [Fact]
    public void LeaveCriticalSection_WithRecursion_ShouldDecrementCount()
    {
        // Arrange
        var criticalSectionAddr = _testEnv.AllocateMemory(CRITICAL_SECTION_SIZE);
        _testEnv.CallKernel32Api("INITIALIZECRITICALSECTION", criticalSectionAddr);
        _testEnv.CallKernel32Api("ENTERCRITICALSECTION", criticalSectionAddr);
        _testEnv.CallKernel32Api("ENTERCRITICALSECTION", criticalSectionAddr);
        _testEnv.CallKernel32Api("ENTERCRITICALSECTION", criticalSectionAddr);

        // Act - Leave once
        _testEnv.CallKernel32Api("LEAVECRITICALSECTION", criticalSectionAddr);

        // Assert - Still locked but recursion decremented
        var recursionCount = _testEnv.Memory.Read32(criticalSectionAddr + 8);
        var owningThread = _testEnv.Memory.Read32(criticalSectionAddr + 12);

        Assert.Equal(2u, recursionCount); // Decremented from 3 to 2
        Assert.NotEqual(0u, owningThread); // Still owned
    }

    [Fact]
    public void DeleteCriticalSection_ShouldClearStructure()
    {
        // Arrange
        var criticalSectionAddr = _testEnv.AllocateMemory(CRITICAL_SECTION_SIZE);
        _testEnv.CallKernel32Api("INITIALIZECRITICALSECTION", criticalSectionAddr);

        // Act - Delete critical section
        _testEnv.CallKernel32Api("DELETECRITICALSECTION", criticalSectionAddr);

        // Assert - Verify structure is cleared
        for (uint i = 0; i < CRITICAL_SECTION_SIZE; i++)
        {
            var value = _testEnv.Memory.Read8(criticalSectionAddr + i);
            Assert.Equal(0, value);
        }
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
