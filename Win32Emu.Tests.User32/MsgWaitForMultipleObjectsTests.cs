using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for MsgWaitForMultipleObjects function in User32
/// </summary>
[Trait("Category", "DllModuleTests")]
public class MsgWaitForMultipleObjectsTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public MsgWaitForMultipleObjectsTests()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}

	[Fact]
	public void MsgWaitForMultipleObjects_WithNoHandles_ShouldIndicateMessageAvailable()
	{
		// Arrange - No handles, waiting for messages
		// Act
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			0u,    // nCount = 0 (no handles)
			0u,    // pHandles = NULL
			0u,    // fWaitAll = FALSE
			100u,  // dwMilliseconds = 100ms
			0xFFu);// dwWakeMask = QS_ALLINPUT

		// Assert - Should return WAIT_OBJECT_0 indicating message available
		Assert.Equal(0u, result); // WAIT_OBJECT_0
	}

	[Fact]
	public void MsgWaitForMultipleObjects_WithNoHandlesAndZeroTimeout_ShouldTimeout()
	{
		// Arrange - No handles, zero timeout
		// Act
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			0u,    // nCount = 0 (no handles)
			0u,    // pHandles = NULL
			0u,    // fWaitAll = FALSE
			0u,    // dwMilliseconds = 0 (no wait)
			0u);   // dwWakeMask = 0 (no input events)

		// Assert - Should return WAIT_TIMEOUT
		Assert.Equal(0x102u, result); // WAIT_TIMEOUT
	}

	[Fact]
	public void MsgWaitForMultipleObjects_WithTooManyHandles_ShouldFail()
	{
		// Arrange - More than MAXIMUM_WAIT_OBJECTS
		var handlesAddr = _testEnv.AllocateMemory(4 * 65);

		// Act
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			65u,          // nCount = 65 (exceeds maximum of 64)
			handlesAddr,  // pHandles
			0u,           // fWaitAll = FALSE
			0u,           // dwMilliseconds = 0
			0u);          // dwWakeMask = 0

		// Assert - Should return WAIT_FAILED
		Assert.Equal(0xFFFFFFFFu, result); // WAIT_FAILED
	}

	[Fact]
	public void MsgWaitForMultipleObjects_WithNullHandlesAndNonZeroCount_ShouldFail()
	{
		// Arrange - NULL handle array with non-zero count
		// Act
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			2u,    // nCount = 2
			0u,    // pHandles = NULL (invalid)
			0u,    // fWaitAll = FALSE
			0u,    // dwMilliseconds = 0
			0u);   // dwWakeMask = 0

		// Assert - Should return WAIT_FAILED
		Assert.Equal(0xFFFFFFFFu, result); // WAIT_FAILED
	}

	[Fact]
	public void MsgWaitForMultipleObjects_WaitAny_WithSignaledEvent_ShouldReturnImmediately()
	{
		// Arrange - Create two events using SynchronizationManager, one signaled, one not
		var syncManager = _testEnv.ProcessEnv.SynchronizationManager;
		Assert.NotNull(syncManager);

		var event1 = syncManager.CreateEvent(manualReset: false, initialState: false, name: null, out _); // Not signaled
		var event2 = syncManager.CreateEvent(manualReset: false, initialState: true, name: null, out _);  // Signaled

		var handlesAddr = _testEnv.AllocateMemory(8);
		_testEnv.Memory.Write32(handlesAddr, event1);
		_testEnv.Memory.Write32(handlesAddr + 4, event2);

		// Act
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			2u,           // nCount = 2
			handlesAddr,  // pHandles
			0u,           // fWaitAll = FALSE (wait for any)
			0u,           // dwMilliseconds = 0 (no wait)
			0u);          // dwWakeMask = 0

		// Assert - Should return WAIT_OBJECT_0 + 1 (second event is signaled)
		Assert.Equal(1u, result); // WAIT_OBJECT_0 + 1
	}

	[Fact]
	public void MsgWaitForMultipleObjects_WaitAll_WithAllSignaled_ShouldReturnImmediately()
	{
		// Arrange - Create two signaled events
		var syncManager = _testEnv.ProcessEnv.SynchronizationManager;
		Assert.NotNull(syncManager);

		var event1 = syncManager.CreateEvent(manualReset: false, initialState: true, name: null, out _); // Signaled
		var event2 = syncManager.CreateEvent(manualReset: false, initialState: true, name: null, out _); // Signaled

		var handlesAddr = _testEnv.AllocateMemory(8);
		_testEnv.Memory.Write32(handlesAddr, event1);
		_testEnv.Memory.Write32(handlesAddr + 4, event2);

		// Act
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			2u,           // nCount = 2
			handlesAddr,  // pHandles
			1u,           // fWaitAll = TRUE (wait for all)
			0u,           // dwMilliseconds = 0 (no wait)
			0u);          // dwWakeMask = 0

		// Assert - Should return WAIT_OBJECT_0 (all objects signaled)
		Assert.Equal(0u, result); // WAIT_OBJECT_0
	}

	[Fact]
	public void MsgWaitForMultipleObjects_WaitAll_WithOneNotSignaled_ShouldTimeout()
	{
		// Arrange - Create two events, one signaled, one not
		var syncManager = _testEnv.ProcessEnv.SynchronizationManager;
		Assert.NotNull(syncManager);

		var event1 = syncManager.CreateEvent(manualReset: false, initialState: true, name: null, out _);  // Signaled
		var event2 = syncManager.CreateEvent(manualReset: false, initialState: false, name: null, out _); // Not signaled

		var handlesAddr = _testEnv.AllocateMemory(8);
		_testEnv.Memory.Write32(handlesAddr, event1);
		_testEnv.Memory.Write32(handlesAddr + 4, event2);

		// Act
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			2u,           // nCount = 2
			handlesAddr,  // pHandles
			1u,           // fWaitAll = TRUE (wait for all)
			0u,           // dwMilliseconds = 0 (no wait)
			0u);          // dwWakeMask = 0

		// Assert - Should return WAIT_TIMEOUT
		Assert.Equal(0x102u, result); // WAIT_TIMEOUT
	}

	[Fact]
	public void MsgWaitForMultipleObjects_WithInvalidHandle_ShouldFail()
	{
		// Arrange - Create array with invalid handle
		var handlesAddr = _testEnv.AllocateMemory(4);
		_testEnv.Memory.Write32(handlesAddr, 0xDEADBEEFu); // Invalid handle

		// Act
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			1u,           // nCount = 1
			handlesAddr,  // pHandles with invalid handle
			0u,           // fWaitAll = FALSE
			0u,           // dwMilliseconds = 0
			0u);          // dwWakeMask = 0

		// Assert - Should return WAIT_FAILED
		Assert.Equal(0xFFFFFFFFu, result); // WAIT_FAILED
	}

	[Fact]
	public void MsgWaitForMultipleObjects_WithWakeMask_ShouldReturnMessageIndex()
	{
		// Arrange - Create one not signaled event
		var syncManager = _testEnv.ProcessEnv.SynchronizationManager;
		Assert.NotNull(syncManager);

		var event1 = syncManager.CreateEvent(manualReset: false, initialState: false, name: null, out _); // Not signaled

		var handlesAddr = _testEnv.AllocateMemory(4);
		_testEnv.Memory.Write32(handlesAddr, event1);

		// Act - Wait with wake mask set (indicating we want to check for messages)
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			1u,           // nCount = 1
			handlesAddr,  // pHandles
			0u,           // fWaitAll = FALSE
			100u,         // dwMilliseconds = 100ms
			0xFFu);       // dwWakeMask = QS_ALLINPUT (check for messages)

		// Assert - Should return WAIT_OBJECT_0 + nCount (indicating message available)
		// In our simplified implementation, if dwWakeMask is set and nothing is signaled, we return message index
		Assert.Equal(1u, result); // WAIT_OBJECT_0 + 1 (message available)
	}

	[Fact]
	public void MsgWaitForMultipleObjects_SingleSignaledEvent_ShouldReturnFirstIndex()
	{
		// Arrange - Create one signaled event
		var syncManager = _testEnv.ProcessEnv.SynchronizationManager;
		Assert.NotNull(syncManager);

		var event1 = syncManager.CreateEvent(manualReset: false, initialState: true, name: null, out _); // Signaled

		var handlesAddr = _testEnv.AllocateMemory(4);
		_testEnv.Memory.Write32(handlesAddr, event1);

		// Act
		var result = _testEnv.CallUser32Api("MSGWAITFORMULTIPLEOBJECTS",
			1u,           // nCount = 1
			handlesAddr,  // pHandles
			0u,           // fWaitAll = FALSE
			0u,           // dwMilliseconds = 0
			0u);          // dwWakeMask = 0

		// Assert - Should return WAIT_OBJECT_0 (first object signaled)
		Assert.Equal(0u, result); // WAIT_OBJECT_0
	}
}
