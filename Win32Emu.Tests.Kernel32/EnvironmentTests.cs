using Xunit;
using System.Text;
using Win32Emu.Tests.Infrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for environment variable functions like GetEnvironmentStringsW
/// </summary>
[Trait("Category", "DllModuleTests")]
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

    [Fact]
    public void FreeEnvironmentStringsW_WithValidPointer_ShouldReturnTrue()
    {
        // Arrange - Get environment strings
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");
        Assert.NotEqual(0u, envStringsPtr);

        // Act - Free the environment strings
        var result = _testEnv.CallKernel32Api("FREEENVIRONMENTSTRINGSW", envStringsPtr);

        // Assert - Should return TRUE (1)
        Assert.Equal(1u, result);
    }

    [Fact]
    public void FreeEnvironmentStringsW_WithNullPointer_ShouldReturnFalse()
    {
        // Act - Try to free a null pointer
        var result = _testEnv.CallKernel32Api("FREEENVIRONMENTSTRINGSW", 0u);

        // Assert - Should return FALSE (0) for null pointer
        Assert.Equal(0u, result);
    }

    [Fact]
    public void FreeEnvironmentStringsW_MultipleCallsWithSamePointer_ShouldSucceed()
    {
        // Arrange - Get environment strings
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");
        Assert.NotEqual(0u, envStringsPtr);

        // Act - Free the same pointer multiple times
        var result1 = _testEnv.CallKernel32Api("FREEENVIRONMENTSTRINGSW", envStringsPtr);
        var result2 = _testEnv.CallKernel32Api("FREEENVIRONMENTSTRINGSW", envStringsPtr);

        // Assert - Both calls should succeed (in our simple implementation)
        Assert.Equal(1u, result1);
        Assert.Equal(1u, result2);
    }
    
    [Fact]
    public void GetEnvironmentStringsA_ShouldReturnValidPointer()
    {
        // Act
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");

        // Assert
        Assert.NotEqual(0u, envStringsPtr); // Should return a valid pointer
    }

    [Fact]
    public void GetEnvironmentStringsA_ShouldReturnProperlyFormattedEnvironmentBlock()
    {
        // Act
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");

        // Assert
        Assert.NotEqual(0u, envStringsPtr);

        // Read and verify the environment strings format (ANSI)
        var environmentStrings = ReadEnvironmentStringsFromMemoryAnsi(envStringsPtr);

        // Should contain some default environment variables
        Assert.True(environmentStrings.Any(s => s.StartsWith("PATH=")), "Should contain PATH environment variable");
        Assert.True(environmentStrings.Any(s => s.StartsWith("WINDIR=")), "Should contain WINDIR environment variable");
        Assert.True(environmentStrings.Any(s => s.StartsWith("SYSTEMROOT=")), "Should contain SYSTEMROOT environment variable");
        Assert.True(environmentStrings.Any(s => s.StartsWith("TEMP=")), "Should contain TEMP environment variable");
        Assert.True(environmentStrings.Any(s => s.StartsWith("USERNAME=")), "Should contain USERNAME environment variable");
    }

    [Fact]
    public void GetEnvironmentStringsA_ShouldUseEmulatedVariablesNotSystemVariables()
    {
        // Act
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");

        // Assert
        Assert.NotEqual(0u, envStringsPtr);

        var environmentStrings = ReadEnvironmentStringsFromMemoryAnsi(envStringsPtr);

        // Should contain emulated values, not real system values
        Assert.Contains("COMPUTERNAME=WIN32EMU", environmentStrings);
        Assert.Contains("USERNAME=User", environmentStrings);
        Assert.Contains("USERDOMAIN=WIN32EMU", environmentStrings);
        Assert.Contains("WINDIR=C:\\WINDOWS", environmentStrings);
    }

    [Fact]
    public void GetEnvironmentStringsA_ShouldReturnDoubleNullTerminated()
    {
        // Act
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");

        // Assert
        Assert.NotEqual(0u, envStringsPtr);

        // Find the end of the environment block by looking for double null termination (ANSI)
        var addr = envStringsPtr;
        var foundDoubleNull = false;
        var maxIterations = 1000; // Safety check to prevent infinite loop
        var iterations = 0;

        while (iterations < maxIterations)
        {
            var byte1 = _testEnv.Memory.Read8(addr);
            var byte2 = _testEnv.Memory.Read8(addr + 1);
            
            if (byte1 == 0 && byte2 == 0)
            {
                foundDoubleNull = true;
                break;
            }
            
            addr += 1;
            iterations++;
        }

        Assert.True(foundDoubleNull, "Environment strings block should be double-null terminated");
    }

    [Fact]
    public void FreeEnvironmentStringsW_ShouldReturnTrue()
    {
        // Arrange
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSW");
        Assert.NotEqual(0u, envStringsPtr);

        // Act
        var result = _testEnv.CallKernel32Api("FREEENVIRONMENTSTRINGSW", envStringsPtr);

        // Assert
        Assert.Equal(1u, result); // Should return TRUE (1)
    }

    [Fact]
    public void FreeEnvironmentStringsA_ShouldReturnTrue()
    {
        // Arrange
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");
        Assert.NotEqual(0u, envStringsPtr);

        // Act
        var result = _testEnv.CallKernel32Api("FREEENVIRONMENTSTRINGSA", envStringsPtr);

        // Assert
        Assert.Equal(1u, result); // Should return TRUE (1)
    }

    [Fact]
    public void FreeEnvironmentStringsA_WithNullPointer_ShouldReturnFalse()
    {
        // Act - Try to free a null pointer
        var result = _testEnv.CallKernel32Api("FREEENVIRONMENTSTRINGSA", 0u);

        // Assert - Should return FALSE (0) for null pointer
        Assert.Equal(0u, result);
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

    [Fact]
    public void GetEnvironmentStrings_WithoutSuffix_ShouldReturnValidPointer()
    {
        // Act - Call GetEnvironmentStrings without A or W suffix
        // Per Windows API convention, this should map to the ANSI version
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGS");

        // Assert
        Assert.NotEqual(0u, envStringsPtr); // Should return a valid pointer
    }

    [Fact]
    public void GetEnvironmentStrings_ShouldBehaveLikeAnsiVersion()
    {
        // Act - Call both GetEnvironmentStrings and GetEnvironmentStringsA
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGS");
        var envStringsAPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");

        // Assert - Both should return valid pointers
        Assert.NotEqual(0u, envStringsPtr);
        Assert.NotEqual(0u, envStringsAPtr);
        
        // The content should be ANSI strings (single-byte characters)
        // Read a few characters to verify they're ANSI
        var firstChar = _testEnv.Memory.Read8(envStringsPtr);
        Assert.True(firstChar > 0 && firstChar < 128, "First character should be ASCII");
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
            if (wchar == 0)
            {
	            break;
            }

            chars.Add((char)wchar);
            currentAddr += 2;
        }
        
        return new string(chars.ToArray());
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
        
        return Encoding.ASCII.GetString(bytes.ToArray());
    }

    [Fact]
    public void GetEnvironmentVariableA_ExistingVariable_ShouldReturnValue()
    {
        // Arrange - PATH should exist in the default environment
        var namePtr = WriteAnsiString("PATH");
        var bufferSize = 1024u;
        var bufferPtr = _testEnv.ProcessEnv.SimpleAlloc(bufferSize);

        // Act
        var result = _testEnv.CallKernel32Api("GETENVIRONMENTVARIABLEA", namePtr, bufferPtr, bufferSize);

        // Assert - Should return length of value (excluding null terminator)
        Assert.True(result > 0, "Should return length of environment variable value");
        
        // Read the value from buffer
        var value = ReadAnsiString(bufferPtr);
        Assert.NotEmpty(value);
        Assert.Equal(result, (uint)value.Length);
    }

    [Fact]
    public void GetEnvironmentVariableA_NonExistentVariable_ShouldReturnZeroAndSetError()
    {
        // Arrange
        var namePtr = WriteAnsiString("NONEXISTENT_VAR_12345");
        var bufferSize = 1024u;
        var bufferPtr = _testEnv.ProcessEnv.SimpleAlloc(bufferSize);

        // Act
        var result = _testEnv.CallKernel32Api("GETENVIRONMENTVARIABLEA", namePtr, bufferPtr, bufferSize);

        // Assert
        Assert.Equal(0u, result);
        Assert.Equal(203u, _testEnv.ProcessEnv.LastError); // ERROR_ENVVAR_NOT_FOUND
    }

    [Fact]
    public void GetEnvironmentVariableA_WithNullBuffer_ShouldReturnRequiredSize()
    {
        // Arrange - PATH should exist
        var namePtr = WriteAnsiString("PATH");

        // Act - Call with NULL buffer
        var result = _testEnv.CallKernel32Api("GETENVIRONMENTVARIABLEA", namePtr, 0u, 0u);

        // Assert - Should return required buffer size (including null terminator)
        Assert.True(result > 0, "Should return required buffer size");
    }

    [Fact]
    public void GetEnvironmentVariableA_WithInsufficientBuffer_ShouldReturnRequiredSize()
    {
        // Arrange - PATH should exist and be longer than 5 characters
        var namePtr = WriteAnsiString("PATH");
        var smallBufferSize = 5u;
        var smallBufferPtr = _testEnv.ProcessEnv.SimpleAlloc(smallBufferSize);

        // Act
        var result = _testEnv.CallKernel32Api("GETENVIRONMENTVARIABLEA", namePtr, smallBufferPtr, smallBufferSize);

        // Assert - Should return required buffer size (which should be > 5)
        Assert.True(result > smallBufferSize, "Should return required buffer size which is greater than provided buffer");
    }

    [Fact]
    public void GetEnvironmentVariableA_SetAndGetNewVariable_ShouldWork()
    {
        // Arrange - Set a new variable
        var testName = "TEST_VAR_123";
        var testValue = "TestValue456";
        var namePtr = WriteAnsiString(testName);
        var valuePtr = WriteAnsiString(testValue);
        
        var setResult = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, valuePtr);
        Assert.Equal(1u, setResult);

        // Act - Get the variable
        var bufferSize = 1024u;
        var bufferPtr = _testEnv.ProcessEnv.SimpleAlloc(bufferSize);
        var getResult = _testEnv.CallKernel32Api("GETENVIRONMENTVARIABLEA", namePtr, bufferPtr, bufferSize);

        // Assert
        Assert.Equal((uint)testValue.Length, getResult);
        var retrievedValue = ReadAnsiString(bufferPtr);
        Assert.Equal(testValue, retrievedValue);
    }

    [Fact]
    public void GetEnvironmentVariableA_DeletedVariable_ShouldReturnZero()
    {
        // Arrange - Set and then delete a variable
        var testName = "TEST_VAR_DELETE";
        var testValue = "InitialValue";
        var namePtr = WriteAnsiString(testName);
        var valuePtr = WriteAnsiString(testValue);
        
        var setResult = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, valuePtr);
        Assert.Equal(1u, setResult);
        
        var deleteResult = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, 0u);
        Assert.Equal(1u, deleteResult);

        // Act - Try to get the deleted variable
        var bufferSize = 1024u;
        var bufferPtr = _testEnv.ProcessEnv.SimpleAlloc(bufferSize);
        var getResult = _testEnv.CallKernel32Api("GETENVIRONMENTVARIABLEA", namePtr, bufferPtr, bufferSize);

        // Assert
        Assert.Equal(0u, getResult);
        Assert.Equal(203u, _testEnv.ProcessEnv.LastError); // ERROR_ENVVAR_NOT_FOUND
    }

    [Fact]
    public void GetEnvironmentVariableA_WithEmptyName_ShouldReturnZero()
    {
        // Arrange
        var emptyNamePtr = WriteAnsiString("");
        var bufferSize = 1024u;
        var bufferPtr = _testEnv.ProcessEnv.SimpleAlloc(bufferSize);

        // Act
        var result = _testEnv.CallKernel32Api("GETENVIRONMENTVARIABLEA", emptyNamePtr, bufferPtr, bufferSize);

        // Assert
        Assert.Equal(0u, result);
        Assert.Equal(203u, _testEnv.ProcessEnv.LastError); // ERROR_ENVVAR_NOT_FOUND
    }

    [Fact]
    public void SetEnvironmentVariableA_ShouldSetVirtualizedVariable()
    {
        // Arrange - Create string pointers for name and value
        var testName = "TEST_VAR_NEW";
        var testValue = "TestValue123";
        var namePtr = WriteAnsiString(testName);
        var valuePtr = WriteAnsiString(testValue);

        // Capture OS environment before the call
        var osValueBefore = Environment.GetEnvironmentVariable(testName);

        // Act - Call SetEnvironmentVariableA
        var result = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, valuePtr);

        // Assert - Should return TRUE (1)
        Assert.Equal(1u, result);

        // Verify the OS environment was NOT modified
        var osValueAfter = Environment.GetEnvironmentVariable(testName);
        Assert.Equal(osValueBefore, osValueAfter);

        // Verify the virtualized environment was modified by checking GetEnvironmentStringsA
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");
        var environmentStrings = ReadEnvironmentStringsFromMemoryAnsi(envStringsPtr);
        Assert.Contains($"{testName}={testValue}", environmentStrings);
    }

    [Fact]
    public void SetEnvironmentVariableA_ShouldDeleteVirtualizedVariable()
    {
        // Arrange - First set a variable
        var testName = "TEST_VAR_DELETE";
        var testValue = "InitialValue";
        var namePtr = WriteAnsiString(testName);
        var valuePtr = WriteAnsiString(testValue);
        
        var setResult = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, valuePtr);
        Assert.Equal(1u, setResult);

        // Capture OS environment before deletion
        var osValueBefore = Environment.GetEnvironmentVariable(testName);

        // Act - Delete the variable by passing NULL (0) for value
        var deleteResult = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, 0u);

        // Assert - Should return TRUE (1)
        Assert.Equal(1u, deleteResult);

        // Verify the OS environment was NOT modified
        var osValueAfter = Environment.GetEnvironmentVariable(testName);
        Assert.Equal(osValueBefore, osValueAfter);

        // Verify the variable was removed from virtualized environment
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");
        var environmentStrings = ReadEnvironmentStringsFromMemoryAnsi(envStringsPtr);
        Assert.DoesNotContain(environmentStrings, s => s.StartsWith($"{testName}="));
    }

    [Fact]
    public void SetEnvironmentVariableA_ShouldUpdateExistingVirtualizedVariable()
    {
        // Arrange - Set initial value
        var testName = "TEST_VAR_UPDATE";
        var initialValue = "InitialValue";
        var updatedValue = "UpdatedValue";
        var namePtr = WriteAnsiString(testName);
        var initialValuePtr = WriteAnsiString(initialValue);
        
        var setResult = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, initialValuePtr);
        Assert.Equal(1u, setResult);

        // Capture OS environment before update
        var osValueBefore = Environment.GetEnvironmentVariable(testName);

        // Act - Update the variable
        var updatedValuePtr = WriteAnsiString(updatedValue);
        var updateResult = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, updatedValuePtr);

        // Assert - Should return TRUE (1)
        Assert.Equal(1u, updateResult);

        // Verify the OS environment was NOT modified
        var osValueAfter = Environment.GetEnvironmentVariable(testName);
        Assert.Equal(osValueBefore, osValueAfter);

        // Verify the virtualized environment has the updated value
        var envStringsPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");
        var environmentStrings = ReadEnvironmentStringsFromMemoryAnsi(envStringsPtr);
        Assert.Contains($"{testName}={updatedValue}", environmentStrings);
        Assert.DoesNotContain($"{testName}={initialValue}", environmentStrings);
    }

    [Fact]
    public void SetEnvironmentVariableA_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange - Create a pointer to an empty string
        var emptyNamePtr = WriteAnsiString("");
        var valuePtr = WriteAnsiString("SomeValue");

        // Act - Try to set a variable with an empty name
        var result = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", emptyNamePtr, valuePtr);

        // Assert - Should return FALSE (0)
        Assert.Equal(0u, result);
    }

    [Fact]
    public void SetEnvironmentVariableA_ShouldNotAffectOtherVirtualizedVariables()
    {
        // Arrange - Get existing environment before changes
        var beforePtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");
        var beforeEnv = ReadEnvironmentStringsFromMemoryAnsi(beforePtr);
        
        // Act - Set a new variable
        var testName = "TEST_VAR_ISOLATED";
        var testValue = "IsolatedValue";
        var namePtr = WriteAnsiString(testName);
        var valuePtr = WriteAnsiString(testValue);
        var result = _testEnv.CallKernel32Api("SETENVIRONMENTVARIABLEA", namePtr, valuePtr);

        // Assert - Should return TRUE (1)
        Assert.Equal(1u, result);

        // Verify other variables are unchanged
        var afterPtr = _testEnv.CallKernel32Api("GETENVIRONMENTSTRINGSA");
        var afterEnv = ReadEnvironmentStringsFromMemoryAnsi(afterPtr);
        
        // All previous variables should still exist
        foreach (var envVar in beforeEnv)
        {
            Assert.Contains(envVar, afterEnv);
        }
        
        // New variable should exist
        Assert.Contains($"{testName}={testValue}", afterEnv);
        
        // Should have exactly one more variable
        Assert.Equal(beforeEnv.Count + 1, afterEnv.Count);
    }

    /// <summary>
    /// Helper method to write an ANSI string to memory and return its pointer
    /// </summary>
    private uint WriteAnsiString(string str)
    {
        var bytes = Encoding.ASCII.GetBytes(str + '\0');
        var addr = _testEnv.ProcessEnv.SimpleAlloc((uint)bytes.Length);
        _testEnv.Memory.WriteBytes(addr, bytes);
        return addr;
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}