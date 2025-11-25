using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for string handling functions in User32
/// </summary>
[Trait("Category", "DllModuleTests")]
public class StringTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public StringTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void CharNextA_WithRegularString_AdvancesToNextCharacter()
    {
        // Arrange: Create a string "Hello" in memory
        var strPtr = _testEnv.WriteString("Hello");

        // Act: Call CharNextA on the first character
        var nextPtr = _testEnv.CallUser32Api("CHARNEXTA", strPtr);

        // Assert: The next pointer should point to 'e' (second character)
        Assert.Equal(strPtr + 1, nextPtr);
        Assert.Equal((byte)'e', _testEnv.Memory.Read8(nextPtr));
    }

    [Fact]
    public void CharNextA_WithMultipleAdvances_IteratesThroughString()
    {
        // Arrange: Create a string "ABC" in memory
        var strPtr = _testEnv.WriteString("ABC");

        // Act: Advance through the string character by character
        var ptr1 = _testEnv.CallUser32Api("CHARNEXTA", strPtr);      // A -> B
        var ptr2 = _testEnv.CallUser32Api("CHARNEXTA", ptr1);        // B -> C
        var ptr3 = _testEnv.CallUser32Api("CHARNEXTA", ptr2);        // C -> \0

        // Assert: Verify each pointer points to the expected character
        Assert.Equal((byte)'B', _testEnv.Memory.Read8(ptr1));
        Assert.Equal((byte)'C', _testEnv.Memory.Read8(ptr2));
        Assert.Equal((byte)0, _testEnv.Memory.Read8(ptr3)); // null terminator
    }

    [Fact]
    public void CharNextA_AtNullTerminator_ReturnsSamePointer()
    {
        // Arrange: Create a string "X" and get pointer to null terminator
        var strPtr = _testEnv.WriteString("X");
        var nullTermPtr = strPtr + 1; // Point to the null terminator

        // Act: Call CharNextA on the null terminator
        var resultPtr = _testEnv.CallUser32Api("CHARNEXTA", nullTermPtr);

        // Assert: Should return the same pointer when at null terminator
        Assert.Equal(nullTermPtr, resultPtr);
        Assert.Equal((byte)0, _testEnv.Memory.Read8(resultPtr));
    }

    [Fact]
    public void CharNextA_WithEmptyString_ReturnsSamePointer()
    {
        // Arrange: Create an empty string (just null terminator)
        var strPtr = _testEnv.WriteString("");

        // Act: Call CharNextA on the empty string
        var resultPtr = _testEnv.CallUser32Api("CHARNEXTA", strPtr);

        // Assert: Should return the same pointer for empty string
        Assert.Equal(strPtr, resultPtr);
        Assert.Equal((byte)0, _testEnv.Memory.Read8(resultPtr));
    }

    [Fact]
    public void CharNextA_WithNullPointer_ReturnsZero()
    {
        // Act: Call CharNextA with a null pointer
        var resultPtr = _testEnv.CallUser32Api("CHARNEXTA", 0);

        // Assert: Should return 0 for null pointer
        Assert.Equal(0u, resultPtr);
    }

    [Fact]
    public void CharNextA_IterateCompleteString_ReachesNullTerminator()
    {
        // Arrange: Create a string "Test" in memory
        var strPtr = _testEnv.WriteString("Test");
        var currentPtr = strPtr;

        // Act & Assert: Iterate through the string until we reach the null terminator
        // T
        Assert.Equal((byte)'T', _testEnv.Memory.Read8(currentPtr));
        currentPtr = _testEnv.CallUser32Api("CHARNEXTA", currentPtr);

        // e
        Assert.Equal((byte)'e', _testEnv.Memory.Read8(currentPtr));
        currentPtr = _testEnv.CallUser32Api("CHARNEXTA", currentPtr);

        // s
        Assert.Equal((byte)'s', _testEnv.Memory.Read8(currentPtr));
        currentPtr = _testEnv.CallUser32Api("CHARNEXTA", currentPtr);

        // t
        Assert.Equal((byte)'t', _testEnv.Memory.Read8(currentPtr));
        currentPtr = _testEnv.CallUser32Api("CHARNEXTA", currentPtr);

        // \0 (null terminator)
        Assert.Equal((byte)0, _testEnv.Memory.Read8(currentPtr));
        
        // Calling CharNextA on null terminator should return the same pointer
        var finalPtr = _testEnv.CallUser32Api("CHARNEXTA", currentPtr);
        Assert.Equal(currentPtr, finalPtr);
    }

    [Fact]
    public void WsprintfA_WithStringFormat_SubstitutesArguments()
    {
        // Arrange: Create format string "%s\%s" and argument strings
        var formatPtr = _testEnv.WriteString("%s\\%s");
        var arg1Ptr = _testEnv.WriteString("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs");
        var arg2Ptr = _testEnv.WriteString("UDS");
        var outputPtr = _testEnv.AllocateMemory(512);

        // Act: Call wsprintfA
        var length = _testEnv.CallUser32Api("WSPRINTFA", outputPtr, formatPtr, arg1Ptr, arg2Ptr);

        // Assert: Check the formatted result
        var result = _testEnv.ReadString(outputPtr);
        Assert.Equal(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\UDS", result);
        Assert.Equal((uint)result.Length, length);
    }

    [Fact]
    public void WsprintfA_WithIntegerFormats_FormatsCorrectly()
    {
        // Arrange: Create format string with various integer formats
        var formatPtr = _testEnv.WriteString("dec=%d, uint=%u, hex=%x, HEX=%X");
        var outputPtr = _testEnv.AllocateMemory(256);

        // Act: Call wsprintfA with integer arguments
        var length = _testEnv.CallUser32Api("WSPRINTFA", outputPtr, formatPtr, unchecked((uint)-42), 100u, 0xABu, 0xCDu);

        // Assert: Check the formatted result
        var result = _testEnv.ReadString(outputPtr);
        Assert.Equal("dec=-42, uint=100, hex=ab, HEX=CD", result);
        Assert.Equal((uint)result.Length, length);
    }

    [Fact]
    public void WsprintfA_WithCharFormat_FormatsCharacter()
    {
        // Arrange: Create format string with character format
        var formatPtr = _testEnv.WriteString("char=%c");
        var outputPtr = _testEnv.AllocateMemory(256);

        // Act: Call wsprintfA with character argument
        var length = _testEnv.CallUser32Api("WSPRINTFA", outputPtr, formatPtr, (uint)'A');

        // Assert: Check the formatted result
        var result = _testEnv.ReadString(outputPtr);
        Assert.Equal("char=A", result);
        Assert.Equal((uint)result.Length, length);
    }

    [Fact]
    public void WsprintfA_WithLiteralPercent_EscapesPercent()
    {
        // Arrange: Create format string with %% for literal %
        var formatPtr = _testEnv.WriteString("Progress: 50%%");
        var outputPtr = _testEnv.AllocateMemory(256);

        // Act: Call wsprintfA
        var length = _testEnv.CallUser32Api("WSPRINTFA", outputPtr, formatPtr);

        // Assert: Check the formatted result
        var result = _testEnv.ReadString(outputPtr);
        Assert.Equal("Progress: 50%", result);
        Assert.Equal((uint)result.Length, length);
    }

    [Fact]
    public void WsprintfA_WithNullStringPointer_PrintsNull()
    {
        // Arrange: Create format string with %s and null pointer
        var formatPtr = _testEnv.WriteString("value=%s");
        var outputPtr = _testEnv.AllocateMemory(256);

        // Act: Call wsprintfA with null string pointer
        var length = _testEnv.CallUser32Api("WSPRINTFA", outputPtr, formatPtr, 0u);

        // Assert: Check that (null) is printed
        var result = _testEnv.ReadString(outputPtr);
        Assert.Equal("value=(null)", result);
        Assert.Equal((uint)result.Length, length);
    }

    [Fact]
    public void WsprintfA_WithMixedFormats_FormatsAllCorrectly()
    {
        // Arrange: Create format string with mixed formats (mimics the actual setup.exe usage)
        var formatPtr = _testEnv.WriteString("%s\\%s.lnk");
        var path1Ptr = _testEnv.WriteString(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs");
        var path2Ptr = _testEnv.WriteString("Ignition");
        var outputPtr = _testEnv.AllocateMemory(512);

        // Act: Call wsprintfA
        var length = _testEnv.CallUser32Api("WSPRINTFA", outputPtr, formatPtr, path1Ptr, path2Ptr);

        // Assert: Check the formatted result
        var result = _testEnv.ReadString(outputPtr);
        Assert.Equal(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Ignition.lnk", result);
        Assert.Equal((uint)result.Length, length);
    }

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}
