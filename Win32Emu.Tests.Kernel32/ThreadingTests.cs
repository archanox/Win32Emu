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
    public void GetCurrentThread_ShouldReturnPseudoHandle()
    {
        // Act
        var threadHandle = _testEnv.CallKernel32Api("GETCURRENTTHREAD");

        // Assert - Should return pseudo-handle (0xFFFFFFFE = -2 as unsigned)
        Assert.Equal(0xFFFFFFFEu, threadHandle);
    }

    [Fact]
    public void GetProcessAffinityMask_ShouldReturnSingleProcessorMask()
    {
        // Arrange
        var processAffinityMaskAddr = _testEnv.AllocateMemory(4);
        var systemAffinityMaskAddr = _testEnv.AllocateMemory(4);
        var processHandle = 0xFFFFFFFFu; // Current process pseudo-handle

        // Act
        var result = _testEnv.CallKernel32Api("GETPROCESSAFFINITYMASK", 
            processHandle, 
            processAffinityMaskAddr, 
            systemAffinityMaskAddr);

        // Assert - Function should succeed
        Assert.Equal(1u, result); // TRUE = 1

        // Assert - Both masks should be 0x1 (single processor)
        var processAffinityMask = _testEnv.Memory.Read32(processAffinityMaskAddr);
        var systemAffinityMask = _testEnv.Memory.Read32(systemAffinityMaskAddr);
        
        Assert.Equal(0x00000001u, processAffinityMask);
        Assert.Equal(0x00000001u, systemAffinityMask);
    }

    [Fact]
    public void GetProcessAffinityMask_WithNullPointers_ShouldReturnFalse()
    {
        // Arrange
        var processHandle = 0xFFFFFFFFu;

        // Act - Both pointers null
        var result = _testEnv.CallKernel32Api("GETPROCESSAFFINITYMASK", 
            processHandle, 
            0u, 
            0u);

        // Assert - Function should fail
        Assert.Equal(0u, result); // FALSE = 0
    }

    [Fact]
    public void GetSystemInfo_ShouldFillStructure()
    {
        // Arrange
        const uint SYSTEM_INFO_SIZE = 36; // Size of SYSTEM_INFO structure
        var systemInfoAddr = _testEnv.AllocateMemory(SYSTEM_INFO_SIZE);

        // Act
        _testEnv.CallKernel32Api("GETSYSTEMINFO", systemInfoAddr);

        // Assert - Read and verify structure fields
        var processorArchitecture = _testEnv.Memory.Read16(systemInfoAddr + 0);
        var reserved = _testEnv.Memory.Read16(systemInfoAddr + 2);
        var pageSize = _testEnv.Memory.Read32(systemInfoAddr + 4);
        var minAddress = _testEnv.Memory.Read32(systemInfoAddr + 8);
        var maxAddress = _testEnv.Memory.Read32(systemInfoAddr + 12);
        var activeProcessorMask = _testEnv.Memory.Read32(systemInfoAddr + 16);
        var numberOfProcessors = _testEnv.Memory.Read32(systemInfoAddr + 20);
        var processorType = _testEnv.Memory.Read32(systemInfoAddr + 24);
        var allocationGranularity = _testEnv.Memory.Read32(systemInfoAddr + 28);
        var processorLevel = _testEnv.Memory.Read16(systemInfoAddr + 32);
        var processorRevision = _testEnv.Memory.Read16(systemInfoAddr + 34);

        // Verify values match our emulated Pentium system
        Assert.Equal(0, processorArchitecture); // PROCESSOR_ARCHITECTURE_INTEL
        Assert.Equal(0, reserved);
        Assert.Equal(4096u, pageSize); // 4KB pages
        Assert.Equal(0x00010000u, minAddress); // 64KB
        Assert.Equal(0x7FFEFFFFu, maxAddress); // 2GB - 64KB
        Assert.Equal(0x00000001u, activeProcessorMask); // Single CPU
        Assert.Equal(1u, numberOfProcessors); // One processor
        Assert.Equal(586u, processorType); // Pentium (586)
        Assert.Equal(65536u, allocationGranularity); // 64KB
        Assert.Equal(5, processorLevel); // Family 5 (Pentium)
        Assert.Equal(0x0101, processorRevision); // Model 1, Stepping 1
    }

    [Fact]
    public void SetThreadAffinityMask_ShouldReturnPreviousMask()
    {
        // Arrange
        var threadHandle = 0xFFFFFFFEu; // Current thread pseudo-handle
        var newAffinityMask = 0x00000001u; // Processor 0

        // Act
        var previousMask = _testEnv.CallKernel32Api("SETTHREADAFFINITYMASK", 
            threadHandle, 
            newAffinityMask);

        // Assert - Should return previous affinity mask (also processor 0)
        Assert.Equal(0x00000001u, previousMask);
    }

    [Fact]
    public void SetThreadAffinityMask_WithZeroMask_ShouldReturnZero()
    {
        // Arrange
        var threadHandle = 0xFFFFFFFEu;
        var zeroMask = 0u;

        // Act
        var result = _testEnv.CallKernel32Api("SETTHREADAFFINITYMASK", 
            threadHandle, 
            zeroMask);

        // Assert - Should fail (return 0)
        Assert.Equal(0u, result);
    }

    [Fact]
    public void SetThreadAffinityMask_WithInvalidMask_ShouldReturnZero()
    {
        // Arrange
        var threadHandle = 0xFFFFFFFEu;
        var invalidMask = 0x00000002u; // Processor 1 doesn't exist in single-processor system

        // Act
        var result = _testEnv.CallKernel32Api("SETTHREADAFFINITYMASK", 
            threadHandle, 
            invalidMask);

        // Assert - Should fail (return 0)
        Assert.Equal(0u, result);
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

    [Fact]
    public void SetThreadPriority_ShouldSucceed()
    {
        // Arrange
        var threadHandle = _testEnv.CallKernel32Api("GETCURRENTTHREAD");
        const int THREAD_PRIORITY_ABOVE_NORMAL = 1;

        // Act
        var result = _testEnv.CallKernel32Api("SETTHREADPRIORITY", threadHandle, (uint)THREAD_PRIORITY_ABOVE_NORMAL);

        // Assert
        Assert.Equal(1u, result); // TRUE = 1
    }

    [Fact]
    public void GetThreadPriority_ShouldReturnSetValue()
    {
        // Arrange
        var threadHandle = _testEnv.CallKernel32Api("GETCURRENTTHREAD");
        const int THREAD_PRIORITY_HIGHEST = 2;
        
        // Set priority first
        _testEnv.CallKernel32Api("SETTHREADPRIORITY", threadHandle, (uint)THREAD_PRIORITY_HIGHEST);

        // Act
        var priority = (int)_testEnv.CallKernel32Api("GETTHREADPRIORITY", threadHandle);

        // Assert
        Assert.Equal(THREAD_PRIORITY_HIGHEST, priority);
    }

    [Fact]
    public void GetThreadPriority_DefaultPriority_ShouldBeNormal()
    {
        // Arrange
        var threadHandle = _testEnv.CallKernel32Api("GETCURRENTTHREAD");

        // Act
        var priority = (int)_testEnv.CallKernel32Api("GETTHREADPRIORITY", threadHandle);

        // Assert
        Assert.Equal(0, priority); // THREAD_PRIORITY_NORMAL = 0
    }

    [Fact]
    public void InterlockedCompareExchange_WhenEqual_ShouldExchange()
    {
        // Arrange
        var valueAddr = _testEnv.AllocateMemory(4);
        const uint initialValue = 100;
        const uint exchangeValue = 200;
        const uint comparand = 100;
        
        _testEnv.Memory.Write32(valueAddr, initialValue);

        // Act
        var result = _testEnv.CallKernel32Api("INTERLOCKEDCOMPAREEXCHANGE", 
            valueAddr, exchangeValue, comparand);

        // Assert
        Assert.Equal(initialValue, result); // Should return initial value
        var newValue = _testEnv.Memory.Read32(valueAddr);
        Assert.Equal(exchangeValue, newValue); // Should have exchanged the value
    }

    [Fact]
    public void InterlockedCompareExchange_WhenNotEqual_ShouldNotExchange()
    {
        // Arrange
        var valueAddr = _testEnv.AllocateMemory(4);
        const uint initialValue = 100;
        const uint exchangeValue = 200;
        const uint comparand = 50; // Different from initial value
        
        _testEnv.Memory.Write32(valueAddr, initialValue);

        // Act
        var result = _testEnv.CallKernel32Api("INTERLOCKEDCOMPAREEXCHANGE", 
            valueAddr, exchangeValue, comparand);

        // Assert
        Assert.Equal(initialValue, result); // Should return initial value
        var newValue = _testEnv.Memory.Read32(valueAddr);
        Assert.Equal(initialValue, newValue); // Should NOT have exchanged the value
    }

    [Fact]
    public void WaitForMultipleObjects_WithNoObjects_ShouldFail()
    {
        // Arrange
        var handlesAddr = _testEnv.AllocateMemory(4);

        // Act
        var result = _testEnv.CallKernel32Api("WAITFORMULTIPLEOBJECTS", 
            0u,           // count = 0 (invalid)
            handlesAddr,  // handles array
            0u,           // bWaitAll = FALSE
            0u);          // dwMilliseconds = 0 (no wait)

        // Assert
        Assert.Equal(0xFFFFFFFFu, result); // WAIT_FAILED
    }

    [Fact]
    public void WaitForMultipleObjects_WithTooManyObjects_ShouldFail()
    {
        // Arrange
        var handlesAddr = _testEnv.AllocateMemory(4);

        // Act
        var result = _testEnv.CallKernel32Api("WAITFORMULTIPLEOBJECTS", 
            65u,          // count = 65 (> MAXIMUM_WAIT_OBJECTS)
            handlesAddr,  // handles array
            0u,           // bWaitAll = FALSE
            0u);          // dwMilliseconds = 0 (no wait)

        // Assert
        Assert.Equal(0xFFFFFFFFu, result); // WAIT_FAILED
    }

    [Fact]
    public void WaitForMultipleObjects_WaitAny_WithSignaledEvent_ShouldReturnImmediately()
    {
        // Arrange - Create two events, one signaled, one not
        var event1 = _testEnv.CallKernel32Api("CREATEEVENTA", 0u, 0u, 0u, 0u); // Manual reset, not signaled
        var event2 = _testEnv.CallKernel32Api("CREATEEVENTA", 0u, 0u, 1u, 0u); // Manual reset, signaled
        
        var handlesAddr = _testEnv.AllocateMemory(8);
        _testEnv.Memory.Write32(handlesAddr, event1);
        _testEnv.Memory.Write32(handlesAddr + 4, event2);

        // Act
        var result = _testEnv.CallKernel32Api("WAITFORMULTIPLEOBJECTS", 
            2u,           // count = 2
            handlesAddr,  // handles array
            0u,           // bWaitAll = FALSE (wait for any)
            0u);          // dwMilliseconds = 0 (no wait)

        // Assert
        Assert.Equal(1u, result); // WAIT_OBJECT_0 + 1 (second event is signaled)
    }

    [Fact]
    public void WaitForMultipleObjects_WaitAll_WithAllSignaled_ShouldReturnImmediately()
    {
        // Arrange - Create two signaled events
        var event1 = _testEnv.CallKernel32Api("CREATEEVENTA", 0u, 0u, 1u, 0u); // Manual reset, signaled
        var event2 = _testEnv.CallKernel32Api("CREATEEVENTA", 0u, 0u, 1u, 0u); // Manual reset, signaled
        
        var handlesAddr = _testEnv.AllocateMemory(8);
        _testEnv.Memory.Write32(handlesAddr, event1);
        _testEnv.Memory.Write32(handlesAddr + 4, event2);

        // Act
        var result = _testEnv.CallKernel32Api("WAITFORMULTIPLEOBJECTS", 
            2u,           // count = 2
            handlesAddr,  // handles array
            1u,           // bWaitAll = TRUE (wait for all)
            0u);          // dwMilliseconds = 0 (no wait)

        // Assert
        Assert.Equal(0u, result); // WAIT_OBJECT_0 (all objects signaled)
    }

    [Fact]
    public void WaitForMultipleObjects_WaitAll_WithOneNotSignaled_ShouldTimeout()
    {
        // Arrange - Create two events, one signaled, one not
        var event1 = _testEnv.CallKernel32Api("CREATEEVENTA", 0u, 0u, 1u, 0u); // Manual reset, signaled
        var event2 = _testEnv.CallKernel32Api("CREATEEVENTA", 0u, 0u, 0u, 0u); // Manual reset, not signaled
        
        var handlesAddr = _testEnv.AllocateMemory(8);
        _testEnv.Memory.Write32(handlesAddr, event1);
        _testEnv.Memory.Write32(handlesAddr + 4, event2);

        // Act
        var result = _testEnv.CallKernel32Api("WAITFORMULTIPLEOBJECTS", 
            2u,           // count = 2
            handlesAddr,  // handles array
            1u,           // bWaitAll = TRUE (wait for all)
            0u);          // dwMilliseconds = 0 (no wait)

        // Assert
        Assert.Equal(0x102u, result); // WAIT_TIMEOUT (not all objects signaled)
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
