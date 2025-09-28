using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for environment variable functions like GetEnvironmentStringsW
/// </summary>
public class EnvironmentTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public EnvironmentTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void GetEnvironmentStringsW_ShouldReturnValidPointer()
    {
        // Act
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");

        // Assert
        Assert.NotEqual(0u, envStringsPtr); // Should return a valid pointer
    }

    [Fact]
    public void GetEnvironmentStringsW_ShouldReturnProperlyFormattedEnvironmentBlock()
    {
        // Act
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");

        // Assert
        Assert.NotEqual(0u, envStringsPtr);

        // Read and verify the environment strings format
        var environmentStrings = ReadEnvironmentStringsFromMemory(envStringsPtr);

        // Should contain some default environment variables
        Assert.True(environmentStrings.Any(s => s.StartsWith("PATH=")), "Should contain PATH environment variable");
        Assert.True(environmentStrings.Any(s => s.StartsWith("WINDIR=")), "Should contain WINDIR environment variable");
        Assert.True(environmentStrings.Any(s => s.StartsWith("SYSTEMROOT=")), "Should contain SYSTEMROOT environment variable");
        Assert.True(environmentStrings.Any(s => s.StartsWith("TEMP=")), "Should contain TEMP environment variable");
        Assert.True(environmentStrings.Any(s => s.StartsWith("USERNAME=")), "Should contain USERNAME environment variable");
    }

    [Fact]
    public void GetEnvironmentStringsW_ShouldUseEmulatedVariablesNotSystemVariables()
    {
        // Act
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");

        // Assert
        Assert.NotEqual(0u, envStringsPtr);

        var environmentStrings = ReadEnvironmentStringsFromMemory(envStringsPtr);

        // Should contain emulated values, not real system values
        Assert.Contains("COMPUTERNAME=WIN32EMU", environmentStrings);
        Assert.Contains("USERNAME=User", environmentStrings);
        Assert.Contains("USERDOMAIN=WIN32EMU", environmentStrings);
        Assert.Contains("WINDIR=C:\\WINDOWS", environmentStrings);
    }

    [Fact]
    public void GetEnvironmentStringsW_ShouldReturnDoubleNullTerminated()
    {
        // Act
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");

        // Assert
        Assert.NotEqual(0u, envStringsPtr);

        // Find the end of the environment block by looking for double null termination
        var addr = envStringsPtr;
        var foundDoubleNull = false;
        var maxIterations = 1000; // Safety check to prevent infinite loop
        var iterations = 0;

        while (iterations < maxIterations)
        {
            var wchar1 = _testEnv.Memory.Read16(addr);
            var wchar2 = _testEnv.Memory.Read16(addr + 2);
            
            if (wchar1 == 0 && wchar2 == 0)
            {
                foundDoubleNull = true;
                break;
            }
            
            addr += 2;
            iterations++;
        }

        Assert.True(foundDoubleNull, "Environment strings block should be double-null terminated");
    }

    [Fact] 
    public void GetEnvironmentStringsW_MultipleCallsShouldReturnValidPointers()
    {
        // Act - Call multiple times
        var envStringsPtr1 = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");
        var envStringsPtr2 = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");

        // Assert - Both calls should return valid (potentially different) pointers
        Assert.NotEqual(0u, envStringsPtr1);
        Assert.NotEqual(0u, envStringsPtr2);
        
        // Content should be the same even if pointers are different
        var env1 = ReadEnvironmentStringsFromMemory(envStringsPtr1);
        var env2 = ReadEnvironmentStringsFromMemory(envStringsPtr2);
        
        Assert.Equal(env1.Count, env2.Count);
        foreach (var envVar in env1)
        {
            Assert.Contains(envVar, env2);
        }
    }

    /// <summary>
    /// Helper method to read environment strings from memory and parse them into a list
    /// </summary>
    private List<string> ReadEnvironmentStringsFromMemory(uint ptr)
    {
        var environmentStrings = new List<string>();
        var addr = ptr;
        
        while (true)
        {
            // Read a null-terminated Unicode string
            var envString = ReadUnicodeString(addr);
            
            if (string.IsNullOrEmpty(envString))
            {
                // Empty string means we've reached the end
                break;
            }
            
            environmentStrings.Add(envString);
            
            // Move to next string (current string length * 2 bytes per char + 2 bytes for null terminator)
            addr += (uint)((envString.Length + 1) * 2);
        }
        
        return environmentStrings;
    }

    /// <summary>
    /// Helper method to read a null-terminated Unicode string from memory
    /// </summary>
    private string ReadUnicodeString(uint addr)
    {
        var chars = new List<char>();
        var currentAddr = addr;
        
        while (true)
        {
            var wchar = _testEnv.Memory.Read16(currentAddr);
            if (wchar == 0) break;
            
            chars.Add((char)wchar);
            currentAddr += 2;
        }
        
        return new string(chars.ToArray());
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}