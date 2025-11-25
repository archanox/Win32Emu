using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for async callback implementations (EnumWindows, SetTimer, SetWindowsHookEx, timeSetEvent)
/// </summary>
[Trait("Category", "DllModuleTests")]
public class AsyncCallbackTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public AsyncCallbackTests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void SetTimer_WithValidParameters_ShouldReturnTimerId()
	{
		// Arrange
		var hWnd = 0x12345678u;
		var nIDEvent = 1u;
		var uElapse = 100u;
		var lpTimerFunc = 0x00401000u; // Mock callback address

		// Act
		var result = _testEnv.CallUser32Api("SETTIMER", hWnd, nIDEvent, uElapse, lpTimerFunc);

		// Assert
		Assert.Equal(nIDEvent, result); // Should return the provided timer ID
	}

	[Fact]
	public void SetTimer_WithZeroTimerId_ShouldAllocateNewTimerId()
	{
		// Arrange
		var hWnd = 0x12345678u;
		var nIDEvent = 0u; // Request auto-allocation
		var uElapse = 100u;
		var lpTimerFunc = 0x00401000u; // Mock callback address

		// Act
		var result = _testEnv.CallUser32Api("SETTIMER", hWnd, nIDEvent, uElapse, lpTimerFunc);

		// Assert
		Assert.NotEqual(0u, result); // Should return a non-zero timer ID
	}

	[Fact]
	public void KillTimer_ForExistingTimer_ShouldSucceed()
	{
		// Arrange - Create a timer first
		var hWnd = 0x12345678u;
		var nIDEvent = 1u;
		var uElapse = 100u;
		var lpTimerFunc = 0x00401000u;
		var timerId = _testEnv.CallUser32Api("SETTIMER", hWnd, nIDEvent, uElapse, lpTimerFunc);

		// Act - Kill the timer
		var result = _testEnv.CallUser32Api("KILLTIMER", hWnd, timerId);

		// Assert
		Assert.Equal(1u, result); // TRUE - success
	}

	[Fact]
	public void KillTimer_ForNonExistentTimer_ShouldStillSucceed()
	{
		// Arrange
		var hWnd = 0x12345678u;
		var nonExistentTimerId = 9999u;

		// Act
		var result = _testEnv.CallUser32Api("KILLTIMER", hWnd, nonExistentTimerId);

		// Assert
		Assert.Equal(1u, result); // TRUE - should succeed even for non-existent timer
	}

	[Fact]
	public void SetWindowsHookExA_WithValidParameters_ShouldReturnHookHandle()
	{
		// Arrange
		var idHook = 5u; // WH_CBT (cast to uint for API)
		var lpfn = 0x00401000u; // Mock hook procedure address
		var hMod = 0u;
		var dwThreadId = 0u; // Current thread

		// Act
		var result = _testEnv.CallUser32Api("SETWINDOWSHOOKEXA", idHook, lpfn, hMod, dwThreadId);

		// Assert
		Assert.NotEqual(0u, result); // Should return a non-zero hook handle
	}

	[Fact]
	public void SetWindowsHookExA_WithNullCallback_ShouldReturnNull()
	{
		// Arrange
		var idHook = 5u; // WH_CBT (cast to uint for API)
		var lpfn = 0u; // NULL callback
		var hMod = 0u;
		var dwThreadId = 0u;

		// Act
		var result = _testEnv.CallUser32Api("SETWINDOWSHOOKEXA", idHook, lpfn, hMod, dwThreadId);

		// Assert
		Assert.Equal(0u, result); // NULL - failure due to null callback
	}

	[Fact]
	public void UnhookWindowsHookEx_ForExistingHook_ShouldSucceed()
	{
		// Arrange - Install a hook first
		var idHook = 5u;
		var lpfn = 0x00401000u;
		var hookHandle = _testEnv.CallUser32Api("SETWINDOWSHOOKEXA", idHook, lpfn, 0u, 0u);

		// Act - Remove the hook
		var result = _testEnv.CallUser32Api("UNHOOKWINDOWSHOOKEX", hookHandle);

		// Assert
		Assert.Equal(1u, result); // TRUE - success
	}

	[Fact]
	public void UnhookWindowsHookEx_ForNonExistentHook_ShouldStillSucceed()
	{
		// Arrange
		var nonExistentHook = 0xDEADBEEFu;

		// Act
		var result = _testEnv.CallUser32Api("UNHOOKWINDOWSHOOKEX", nonExistentHook);

		// Assert
		Assert.Equal(1u, result); // TRUE - should succeed even for non-existent hook
	}

	[Fact]
	public void EnumWindows_WithNoWindows_ShouldReturnSuccess()
	{
		// Arrange - No windows created
		var lpEnumFunc = 0x00401000u; // Mock callback address
		var lParam = 0x12345678u;

		// Act
		var result = _testEnv.CallUser32Api("ENUMWINDOWS", lpEnumFunc, lParam);

		// Assert
		Assert.Equal(1u, result); // TRUE - success (no windows to enumerate)
	}

	[Fact]
	public void EnumWindows_WithNullCallback_ShouldReturnSuccess()
	{
		// Arrange
		var lpEnumFunc = 0u; // NULL callback
		var lParam = 0u;

		// Act
		var result = _testEnv.CallUser32Api("ENUMWINDOWS", lpEnumFunc, lParam);

		// Assert
		Assert.Equal(1u, result); // TRUE - success (null callback is handled gracefully)
	}

	[Fact]
	public void TimeSetEvent_WithValidParameters_ShouldReturnTimerId()
	{
		// Arrange
		var uDelay = 100u;
		var uResolution = 10u;
		var lpTimeProc = 0x00401000u; // Mock callback address
		var dwUser = 0x12345678u;
		var fuEvent = 0u; // TIME_ONESHOT

		// Act
		var result = _testEnv.CallWinMmApi("TIMESETEVENT", uDelay, uResolution, lpTimeProc, dwUser, fuEvent);

		// Assert
		Assert.NotEqual(0u, result); // Should return a non-zero timer ID
	}

	[Fact]
	public void TimeSetEvent_WithNullCallback_ShouldReturnNull()
	{
		// Arrange
		var uDelay = 100u;
		var uResolution = 10u;
		var lpTimeProc = 0u; // NULL callback
		var dwUser = 0u;
		var fuEvent = 0u;

		// Act
		var result = _testEnv.CallWinMmApi("TIMESETEVENT", uDelay, uResolution, lpTimeProc, dwUser, fuEvent);

		// Assert
		Assert.Equal(0u, result); // NULL - failure due to null callback
	}

	[Fact]
	public void TimeKillEvent_ForExistingTimer_ShouldSucceed()
	{
		// Arrange - Create a timer first
		var uDelay = 100u;
		var uResolution = 10u;
		var lpTimeProc = 0x00401000u;
		var timerId = _testEnv.CallWinMmApi("TIMESETEVENT", uDelay, uResolution, lpTimeProc, 0u, 0u);

		// Act - Kill the timer
		var result = _testEnv.CallWinMmApi("TIMEKILLEVENT", timerId);

		// Assert
		Assert.Equal(0u, result); // TIMERR_NOERROR
	}

	[Fact]
	public void TimeKillEvent_ForNonExistentTimer_ShouldStillSucceed()
	{
		// Arrange
		var nonExistentTimerId = 0xDEADBEEFu;

		// Act
		var result = _testEnv.CallWinMmApi("TIMEKILLEVENT", nonExistentTimerId);

		// Assert
		Assert.Equal(0u, result); // TIMERR_NOERROR - should succeed even for non-existent timer
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
