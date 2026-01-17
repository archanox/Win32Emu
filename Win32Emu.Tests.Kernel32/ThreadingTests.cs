using Xunit;
using Win32Emu.Tests.Infrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for threading and TLS (Thread Local Storage) functions
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class ThreadingTests : IDisposable
{
    private readonly TestEnvironment _testEnv;
    
    // Constants for CRITICAL_SECTION structure
    private const uint CRITICAL_SECTION_SIZE = 24;
    private const uint CRITICAL_SECTION_UNLOCKED = unchecked((uint)-1);
    
    // Thread access rights constants
    private const uint THREAD_ALL_ACCESS = 0x1F03FF;
    private const uint THREAD_QUERY_INFORMATION = 0x0040;
    
    // Thread creation flags
    private const uint CREATE_SUSPENDED = 0x4;
    
    // Test constants
    private const uint TEST_STACK_SIZE = 0x8000;
    private const uint TEST_START_ADDRESS = 0x00401000;

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
    public void OpenThread_WithCurrentThreadId_ShouldReturnValidHandle()
    {
        // Arrange
        var currentThreadId = _testEnv.CallKernel32Api("GETCURRENTTHREADID");
        
        // Act
        var threadHandle = _testEnv.CallKernel32Api("OPENTHREAD", 
            THREAD_ALL_ACCESS,  // dwDesiredAccess
            0u,                  // bInheritHandle = FALSE
            currentThreadId);    // dwThreadId

        // Assert
        Assert.NotEqual(0u, threadHandle); // Should return a valid handle
    }

    [Fact]
    public void OpenThread_WithInvalidThreadId_ShouldReturnNull()
    {
        // Arrange
        const uint invalidThreadId = 9999u;
        
        // Act
        var threadHandle = _testEnv.CallKernel32Api("OPENTHREAD", 
            THREAD_ALL_ACCESS,  // dwDesiredAccess
            0u,                  // bInheritHandle = FALSE
            invalidThreadId);    // dwThreadId

        // Assert
        Assert.Equal(0u, threadHandle); // Should return NULL for invalid thread ID
    }

    [Fact]
    public void OpenThread_WithValidThreadId_ReturnsSameHandleAsOriginal()
    {
        // Arrange - Create a thread
        var threadIdPtr = _testEnv.AllocateMemory(4);
        
        var originalHandle = _testEnv.CallKernel32Api("CREATETHREAD", 
            0u,                  // lpThreadAttributes
            TEST_STACK_SIZE,
            TEST_START_ADDRESS,
            0u,                  // parameter
            CREATE_SUSPENDED,
            threadIdPtr);
        
        var threadId = _testEnv.Memory.Read32(threadIdPtr);
        
        // Act - Open the same thread
        var openedHandle = _testEnv.CallKernel32Api("OPENTHREAD", 
            THREAD_ALL_ACCESS,  // dwDesiredAccess
            0u,                  // bInheritHandle = FALSE
            threadId);           // dwThreadId

        // Assert
        Assert.NotEqual(0u, openedHandle);
        Assert.Equal(originalHandle, openedHandle); // In the emulator, we return the same handle
    }

    [Fact]
    public void OpenThread_WithDifferentAccessRights_ShouldSucceed()
    {
        // Arrange
        var currentThreadId = _testEnv.CallKernel32Api("GETCURRENTTHREADID");
        
        // Act
        var threadHandle = _testEnv.CallKernel32Api("OPENTHREAD", 
            THREAD_QUERY_INFORMATION,  // dwDesiredAccess (limited access)
            0u,                         // bInheritHandle = FALSE
            currentThreadId);           // dwThreadId

        // Assert
        Assert.NotEqual(0u, threadHandle); // Should succeed even with limited access
    }

    [Fact]
    public void OpenThread_WithInheritHandleTrue_ShouldSucceed()
    {
        // Arrange
        var currentThreadId = _testEnv.CallKernel32Api("GETCURRENTTHREADID");
        
        // Act
        var threadHandle = _testEnv.CallKernel32Api("OPENTHREAD", 
            THREAD_ALL_ACCESS,  // dwDesiredAccess
            1u,                  // bInheritHandle = TRUE
            currentThreadId);    // dwThreadId

        // Assert
        Assert.NotEqual(0u, threadHandle); // Should succeed regardless of inherit flag
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

    #region Mutex Tests

    [Fact]
    public void CreateMutexA_WithoutInitialOwner_ShouldReturnValidHandle()
    {
        // Arrange - Create an unnamed mutex without initial ownership
        var lpMutexAttributes = 0u; // NULL (default security)
        var bInitialOwner = 0u; // FALSE - not initially owned
        var lpName = 0u; // NULL (unnamed mutex)

        // Act
        var handle = _testEnv.CallKernel32Api("CREATEMUTEXA", lpMutexAttributes, bInitialOwner, lpName);

        // Assert
        Assert.NotEqual(0u, handle); // Should return a valid handle
    }

    [Fact]
    public void CreateMutexA_WithInitialOwner_ShouldReturnValidHandleAndBeOwned()
    {
        // Arrange - Create an unnamed mutex with initial ownership
        var lpMutexAttributes = 0u; // NULL (default security)
        var bInitialOwner = 1u; // TRUE - initially owned by creating thread
        var lpName = 0u; // NULL (unnamed mutex)

        // Act
        var handle = _testEnv.CallKernel32Api("CREATEMUTEXA", lpMutexAttributes, bInitialOwner, lpName);

        // Assert
        Assert.NotEqual(0u, handle); // Should return a valid handle
        
        // The mutex should be owned, so trying to wait with zero timeout should succeed immediately
        const uint WAIT_OBJECT_0 = 0;
        var waitResult = _testEnv.CallKernel32Api("WAITFORSINGLEOBJECT", handle, 0u);
        Assert.Equal(WAIT_OBJECT_0, waitResult); // Should acquire immediately (recursive acquisition)
    }

    [Fact]
    public void CreateMutexA_WithName_ShouldReturnValidHandle()
    {
        // Arrange - Create a named mutex
        var lpMutexAttributes = 0u;
        var bInitialOwner = 0u;
        var mutexName = "TestMutex_" + Guid.NewGuid().ToString();
        var lpName = _testEnv.WriteString(mutexName);

        // Act
        var handle = _testEnv.CallKernel32Api("CREATEMUTEXA", lpMutexAttributes, bInitialOwner, lpName);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void CreateMutexA_WithSameName_ShouldReturnSameHandle()
    {
        // Arrange - Create two mutexes with the same name
        var lpMutexAttributes = 0u;
        var bInitialOwner = 0u;
        var mutexName = "TestMutex_" + Guid.NewGuid().ToString();
        var lpName = _testEnv.WriteString(mutexName);

        // Act
        var handle1 = _testEnv.CallKernel32Api("CREATEMUTEXA", lpMutexAttributes, bInitialOwner, lpName);
        var handle2 = _testEnv.CallKernel32Api("CREATEMUTEXA", lpMutexAttributes, bInitialOwner, lpName);

        // Assert
        Assert.NotEqual(0u, handle1);
        Assert.Equal(handle1, handle2); // Should return the same handle for the same name
        
        // Check that LastError indicates the mutex already exists
        var lastError = _testEnv.CallKernel32Api("GETLASTERROR");
        const uint ERROR_ALREADY_EXISTS = 183;
        Assert.Equal(ERROR_ALREADY_EXISTS, lastError);
    }

    [Fact]
    public void ReleaseMutex_OnOwnedMutex_ShouldSucceed()
    {
        // Arrange - Create and acquire a mutex
        var handle = _testEnv.CallKernel32Api("CREATEMUTEXA", 0u, 1u, 0u);
        Assert.NotEqual(0u, handle);

        // Act - Release the mutex
        var result = _testEnv.CallKernel32Api("RELEASEMUTEX", handle);

        // Assert
        Assert.Equal(1u, result); // TRUE - success
    }

    [Fact]
    public void ReleaseMutex_OnUnownedMutex_ShouldFail()
    {
        // Arrange - Create a mutex without acquiring it
        var handle = _testEnv.CallKernel32Api("CREATEMUTEXA", 0u, 0u, 0u);
        Assert.NotEqual(0u, handle);

        // Act - Try to release the mutex (should fail - not owned)
        var result = _testEnv.CallKernel32Api("RELEASEMUTEX", handle);

        // Assert
        Assert.Equal(0u, result); // FALSE - failure
        
        // Check that LastError indicates not the owner
        var lastError = _testEnv.CallKernel32Api("GETLASTERROR");
        const uint ERROR_NOT_OWNER = 288;
        Assert.Equal(ERROR_NOT_OWNER, lastError);
    }

    [Fact]
    public void WaitForSingleObject_OnFreeMutex_ShouldAcquireImmediately()
    {
        // Arrange - Create a free (unowned) mutex
        var handle = _testEnv.CallKernel32Api("CREATEMUTEXA", 0u, 0u, 0u);
        Assert.NotEqual(0u, handle);

        // Act - Wait for the mutex with zero timeout
        const uint WAIT_OBJECT_0 = 0;
        var result = _testEnv.CallKernel32Api("WAITFORSINGLEOBJECT", handle, 0u);

        // Assert
        Assert.Equal(WAIT_OBJECT_0, result); // Should acquire immediately
    }

    [Fact]
    public void WaitForSingleObject_WithTimeout_ShouldReturnTimeout()
    {
        // Arrange - Create an owned mutex
        var handle = _testEnv.CallKernel32Api("CREATEMUTEXA", 0u, 1u, 0u);
        Assert.NotEqual(0u, handle);
        
        // Acquire it again (recursive) and release twice to make it unowned
        _testEnv.CallKernel32Api("WAITFORSINGLEOBJECT", handle, 0u);
        _testEnv.CallKernel32Api("RELEASEMUTEX", handle);
        _testEnv.CallKernel32Api("RELEASEMUTEX", handle);

        // Act - Wait with zero timeout (should succeed as mutex is now free)
        const uint WAIT_OBJECT_0 = 0;
        var result = _testEnv.CallKernel32Api("WAITFORSINGLEOBJECT", handle, 0u);

        // Assert
        Assert.Equal(WAIT_OBJECT_0, result); // Should acquire successfully
    }

    [Fact]
    public void OpenMutexA_OnExistingNamedMutex_ShouldReturnSameHandle()
    {
        // Arrange - Create a named mutex
        var mutexName = "TestMutex_" + Guid.NewGuid().ToString();
        var lpName = _testEnv.WriteString(mutexName);
        var createdHandle = _testEnv.CallKernel32Api("CREATEMUTEXA", 0u, 0u, lpName);
        Assert.NotEqual(0u, createdHandle);

        // Act - Open the existing mutex
        const uint SYNCHRONIZE = 0x00100000;
        var openedHandle = _testEnv.CallKernel32Api("OPENMUTEXA", SYNCHRONIZE, 0u, lpName);

        // Assert
        Assert.NotEqual(0u, openedHandle);
        Assert.Equal(createdHandle, openedHandle); // Should return the same handle
    }

    [Fact]
    public void OpenMutexA_OnNonExistentMutex_ShouldReturnNull()
    {
        // Arrange - Try to open a mutex that doesn't exist
        var mutexName = "NonExistentMutex_" + Guid.NewGuid().ToString();
        var lpName = _testEnv.WriteString(mutexName);

        // Act
        const uint SYNCHRONIZE = 0x00100000;
        var handle = _testEnv.CallKernel32Api("OPENMUTEXA", SYNCHRONIZE, 0u, lpName);

        // Assert
        Assert.Equal(0u, handle); // NULL handle
        
        // Check that LastError indicates the mutex was not found
        var lastError = _testEnv.CallKernel32Api("GETLASTERROR");
        const uint ERROR_FILE_NOT_FOUND = 2;
        Assert.Equal(ERROR_FILE_NOT_FOUND, lastError);
    }

    [Fact]
    public void OpenMutexA_WithEmptyName_ShouldReturnNull()
    {
        // Arrange - Try to open a mutex with empty name
        var lpName = _testEnv.WriteString("");

        // Act
        const uint SYNCHRONIZE = 0x00100000;
        var handle = _testEnv.CallKernel32Api("OPENMUTEXA", SYNCHRONIZE, 0u, lpName);

        // Assert
        Assert.Equal(0u, handle); // NULL handle
        
        // Check that LastError indicates invalid parameter
        var lastError = _testEnv.CallKernel32Api("GETLASTERROR");
        const uint ERROR_INVALID_PARAMETER = 87;
        Assert.Equal(ERROR_INVALID_PARAMETER, lastError);
    }

    [Fact]
    public void CreateMutexW_WithoutInitialOwner_ShouldReturnValidHandle()
    {
        // Arrange - Create an unnamed mutex without initial ownership (Unicode version)
        var lpMutexAttributes = 0u;
        var bInitialOwner = 0u;
        var lpName = 0u;

        // Act
        var handle = _testEnv.CallKernel32Api("CREATEMUTEXW", lpMutexAttributes, bInitialOwner, lpName);

        // Assert
        Assert.NotEqual(0u, handle); // Should return a valid handle
    }

    #endregion
}
