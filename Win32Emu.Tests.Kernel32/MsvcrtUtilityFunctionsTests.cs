using Win32Emu.Win32.Modules;
using Win32Emu.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for MSVCRT utility functions (rand, srand, system, sleep, search, etc.)
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class MsvcrtUtilityFunctionsTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly MsvcrtModule _msvcrt;

	public MsvcrtUtilityFunctionsTests()
	{
		_testEnv = new TestEnvironment();
		_msvcrt = new MsvcrtModule(_testEnv.ProcessEnv, 0x00400000, _testEnv.PeLoader, NullLogger.Instance);
	}

	public void Dispose()
	{
		_testEnv?.Dispose();
	}
	[Fact]
	public void Rand_ReturnsValueInRange()
	{
		// Arrange - nothing needed, using _testEnv and _msvcrt from constructor
		
		// Act - call rand() a few times
		var results = new int[10];
		for (int i = 0; i < results.Length; i++)
		{
			var success = _msvcrt.TryInvokeUnsafe("RAND", _testEnv.Cpu, _testEnv.Memory, out var result);
			Assert.True(success);
			results[i] = (int)result;
		}
		
		// Assert - all values should be in range 0-32767
		foreach (var value in results)
		{
			Assert.InRange(value, 0, 32767);
		}
		
		// Assert - values should be different (not all the same)
		Assert.True(results.Distinct().Count() > 1, "Random values should not all be the same");
	}
	
	[Fact]
	public void Srand_SeedsRandomNumberGenerator()
	{
		// Arrange - set up stack args for srand
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, 12345u);
		
		// Act - seed with a specific value and get some random numbers
		var success1 = _msvcrt.TryInvokeUnsafe("SRAND", _testEnv.Cpu, _testEnv.Memory, out _);
		Assert.True(success1);
		
		var success2 = _msvcrt.TryInvokeUnsafe("RAND", _testEnv.Cpu, _testEnv.Memory, out var first1);
		Assert.True(success2);
		
		var success3 = _msvcrt.TryInvokeUnsafe("RAND", _testEnv.Cpu, _testEnv.Memory, out var second1);
		Assert.True(success3);
		
		// Seed again with the same value
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, 12345u);
		var success4 = _msvcrt.TryInvokeUnsafe("SRAND", _testEnv.Cpu, _testEnv.Memory, out _);
		Assert.True(success4);
		
		var success5 = _msvcrt.TryInvokeUnsafe("RAND", _testEnv.Cpu, _testEnv.Memory, out var first2);
		Assert.True(success5);
		
		var success6 = _msvcrt.TryInvokeUnsafe("RAND", _testEnv.Cpu, _testEnv.Memory, out var second2);
		Assert.True(success6);
		
		// Assert - same seed should produce same sequence
		Assert.Equal(first1, first2);
		Assert.Equal(second1, second2);
	}
	
	[Fact]
	public void RandS_GeneratesRandomNumber()
	{
		// Arrange - use a fixed memory address
		var pval = 0x00100000u;
		_testEnv.Memory.Write32(pval, 0); // Initialize
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, pval);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("RAND_S", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(0u, result); // Success
		// Just verify the function completed without error
	}
	
	[Fact]
	public void RandS_ReturnsErrorForNullPointer()
	{
		// Arrange - pass NULL pointer
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, 0u);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("RAND_S", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(22u, result); // EINVAL
	}
	
	[Fact]
	public void System_ReturnsNonZeroForNullCommand()
	{
		// Arrange - pass NULL to check if command processor is available
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, 0u);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("SYSTEM", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(1u, result); // Command processor is available
	}
	
	[Fact]
	public void System_ReturnsSuccessForCommand()
	{
		// Arrange - write command string to fixed address
		var cmdAddr = 0x00100000u;
		var cmdBytes = System.Text.Encoding.ASCII.GetBytes("echo hello\0");
		_testEnv.Memory.WriteBytes(cmdAddr, cmdBytes);
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, cmdAddr);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("SYSTEM", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert - for safety, we don't actually execute commands, but return success
		Assert.True(success);
		Assert.Equal(0u, result);
	}
	
	[Fact]
	public void Wsystem_ReturnsNonZeroForNullCommand()
	{
		// Arrange - pass NULL to check if command processor is available
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, 0u);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_WSYSTEM", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(1u, result); // Command processor is available
	}
	
	[Fact]
	public void Sleep_AcceptsMilliseconds()
	{
		// Arrange - sleep for 10ms (doesn't actually sleep in emulator)
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, 10u);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_SLEEP", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert - no return value, just verify it doesn't crash
		Assert.True(success);
		Assert.Equal(0u, result);
	}
	
	[Fact]
	public void Beep_AcceptsFrequencyAndDuration()
	{
		// Arrange - beep at 440Hz for 100ms (doesn't actually beep in emulator)
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0, 440u);
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 1, 100u);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_BEEP", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert - no return value, just verify it doesn't crash
		Assert.True(success);
		Assert.Equal(0u, result);
	}
	
	[Fact]
	public void Tzset_InitializesTimezone()
	{
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_TZSET", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert - no return value, just verify it doesn't crash
		Assert.True(success);
		Assert.Equal(0u, result);
	}
	
	[Fact]
	public void PDaylight_ReturnsPointer()
	{
		// Act
		var success = _msvcrt.TryInvokeUnsafe("__P__DAYLIGHT", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert - should return non-zero pointer
		Assert.True(success);
		Assert.NotEqual(0u, result);
		
		// Read the daylight value (should be 0 or 1 depending on system timezone)
		var daylight = _testEnv.Memory.Read32(result);
		Assert.InRange(daylight, 0u, 1u);
	}
	
	[Fact]
	public void PTimezone_ReturnsPointer()
	{
		// Act
		var success = _msvcrt.TryInvokeUnsafe("__P__TIMEZONE", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert - should return non-zero pointer
		Assert.True(success);
		Assert.NotEqual(0u, result);
		
		// Read the timezone value (seconds west of UTC, typically -43200 to 50400)
		var timezone = (int)_testEnv.Memory.Read32(result);
		Assert.InRange(timezone, -43200, 50400); // Valid timezone range
	}
	
	[Fact]
	public void PDstbias_ReturnsPointer()
	{
		// Act
		var success = _msvcrt.TryInvokeUnsafe("__P__DSTBIAS", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert - should return non-zero pointer
		Assert.True(success);
		Assert.NotEqual(0u, result);
		
		// Read the dstbias value (typically -3600 for 1 hour DST, but can vary)
		var dstbias = (int)_testEnv.Memory.Read32(result);
		Assert.InRange(dstbias, -7200, 0); // DST bias is usually negative, up to 2 hours
	}
	
	[Fact]
	public void Lsearch_AddsElementWhenNotFound()
	{
		// Arrange - create an array with some elements
		var arraySize = 3u;
		var elementSize = 4u;
		var arrayPtr = _testEnv.ProcessEnv.HeapAlloc(0, arraySize * elementSize + elementSize); // Extra space for new element
		var numPtr = _testEnv.ProcessEnv.HeapAlloc(0, 4);
		
		// Initialize array with values: [10, 20, 30]
		_testEnv.Memory.Write32(arrayPtr + 0, 10u);
		_testEnv.Memory.Write32(arrayPtr + 4, 20u);
		_testEnv.Memory.Write32(arrayPtr + 8, 30u);
		_testEnv.Memory.Write32(numPtr, arraySize);
		
		// Key to search for (not in array)
		var keyPtr = _testEnv.ProcessEnv.HeapAlloc(0, 4);
		_testEnv.Memory.Write32(keyPtr, 40u);
		
		// Dummy comparison function (we can't actually call it)
		var comparePtr = 0x12345678u;
		
		// Act - pass all arguments at once
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, keyPtr, arrayPtr, numPtr, elementSize, comparePtr);
		var success = _msvcrt.TryInvokeUnsafe("_LSEARCH", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert - should return pointer to new element (at index 3)
		Assert.True(success);
		Assert.Equal(arrayPtr + 12, result);
		
		// Verify the new element was added
		var newValue = _testEnv.Memory.Read32(result);
		Assert.Equal(40u, newValue);
		
		// Verify count was incremented
		var newCount = _testEnv.Memory.Read32(numPtr);
		Assert.Equal(4u, newCount);
	}
}
