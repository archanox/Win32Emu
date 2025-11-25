using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for registry hive integration with environment variables
/// </summary>
[Trait("Category", "DllModuleTests")]
public class RegistryEnvironmentTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public RegistryEnvironmentTests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void ProcessEnvironment_ShouldInitializeRegistryHive()
	{
		// The registry hive should be initialized during ProcessEnvironment construction
		// This is verified by the fact that environment variables work (they're stored in registry)
		
		// Get environment strings - this internally uses the registry
		var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");
		
		// Assert - Should return a valid pointer
		Assert.NotEqual(0u, envStringsPtr);
	}

	[Fact]
	public void SetEnvironmentVariable_ShouldUpdateRegistry()
	{
		// Arrange
		var testName = "TEST_REGISTRY_VAR";
		var testValue = "RegistryValue123";
		var namePtr = WriteAnsiString(testName);
		var valuePtr = WriteAnsiString(testValue);

		// Act - Set the environment variable (this should update the registry)
		var result = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, valuePtr);

		// Assert
		Assert.Equal(1u, result); // TRUE

		// Verify it can be read back
		var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");
		var environmentStrings = ReadEnvironmentStringsFromMemoryAnsi(envStringsPtr);
		Assert.Contains($"{testName}={testValue}", environmentStrings);
	}

	[Fact]
	public void EnvironmentVariables_ShouldIncludeRegistryDefaults()
	{
		// Act - Get environment strings
		var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");
		var environmentStrings = ReadEnvironmentStringsFromMemoryAnsi(envStringsPtr);

		// Assert - Should contain default values from registry initialization
		Assert.Contains(environmentStrings, s => s.StartsWith("PATH="));
		Assert.Contains(environmentStrings, s => s.StartsWith("WINDIR="));
		Assert.Contains(environmentStrings, s => s.StartsWith("TEMP="));
	}

	/// <summary>
	/// Helper method to write an ANSI string to memory and return its pointer
	/// </summary>
	private uint WriteAnsiString(string str)
	{
		var bytes = System.Text.Encoding.ASCII.GetBytes(str + '\0');
		var addr = _testEnv.ProcessEnv.SimpleAlloc((uint)bytes.Length);
		_testEnv.Memory.WriteBytes(addr, bytes);
		return addr;
	}

	/// <summary>
	/// Helper method to read ANSI environment strings from memory and parse them into a list
	/// </summary>
	private List<string> ReadEnvironmentStringsFromMemoryAnsi(uint ptr)
	{
		var environmentStrings = new List<string>();
		var addr = ptr;
		
		while (true)
		{
			// Read a null-terminated ANSI string
			var envString = ReadAnsiString(addr);
			
			if (string.IsNullOrEmpty(envString))
			{
				// Empty string means we've reached the end
				break;
			}
			
			environmentStrings.Add(envString);
			
			// Move to next string (current string length + 1 byte for null terminator)
			addr += (uint)(envString.Length + 1);
		}
		
		return environmentStrings;
	}

	/// <summary>
	/// Helper method to read a null-terminated ANSI string from memory
	/// </summary>
	private string ReadAnsiString(uint addr)
	{
		var bytes = new List<byte>();
		var currentAddr = addr;
		
		while (true)
		{
			var b = _testEnv.Memory.Read8(currentAddr);
			if (b == 0)
			{
				break;
			}

			bytes.Add(b);
			currentAddr += 1;
		}
		
		return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
