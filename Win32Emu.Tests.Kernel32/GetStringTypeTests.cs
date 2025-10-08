using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Comprehensive tests for GetStringTypeA and GetStringTypeW functions
/// These are critical for C runtime string parsing
/// </summary>
public class GetStringTypeTests
{
    [Fact]
    public void GetStringTypeA_WithDigits_ShouldSetDigitFlag()
    {
        var env = new TestEnvironment();
        var testStr = "0123456789";
        var strAddr = env.WriteString(testStr);
        var resultAddr = env.Malloc(testStr.Length * 2); // 2 bytes per char

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u,             // locale (ignored)
            1u,             // CT_CTYPE1
            strAddr,        // string pointer
            testStr.Length, // length
            resultAddr      // output buffer
        );

        Assert.Equal(1u, result); // TRUE
        
        // CT_CTYPE1_DIGIT = 0x0004
        // CT_CTYPE1_XDIGIT = 0x0080
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0004) != 0, $"Digit flag not set for '{testStr[i]}'");
            Assert.True((charType & 0x0080) != 0, $"Hex digit flag not set for '{testStr[i]}'");
        }
    }

    [Fact]
    public void GetStringTypeA_WithUppercase_ShouldSetUpperAndAlphaFlags()
    {
        var env = new TestEnvironment();
        var testStr = "ABCDEF";
        var strAddr = env.WriteString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u, 1u, strAddr, testStr.Length, resultAddr);

        Assert.Equal(1u, result);

        // CT_CTYPE1_UPPER = 0x0001, CT_CTYPE1_ALPHA = 0x0100, CT_CTYPE1_XDIGIT = 0x0080
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0001) != 0, $"Upper flag not set for '{testStr[i]}'");
            Assert.True((charType & 0x0100) != 0, $"Alpha flag not set for '{testStr[i]}'");
            Assert.True((charType & 0x0080) != 0, $"Hex digit flag not set for '{testStr[i]}'");
        }
    }

    [Fact]
    public void GetStringTypeA_WithLowercase_ShouldSetLowerAndAlphaFlags()
    {
        var env = new TestEnvironment();
        var testStr = "abcdef";
        var strAddr = env.WriteString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u, 1u, strAddr, testStr.Length, resultAddr);

        Assert.Equal(1u, result);

        // CT_CTYPE1_LOWER = 0x0002, CT_CTYPE1_ALPHA = 0x0100, CT_CTYPE1_XDIGIT = 0x0080
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0002) != 0, $"Lower flag not set for '{testStr[i]}'");
            Assert.True((charType & 0x0100) != 0, $"Alpha flag not set for '{testStr[i]}'");
            Assert.True((charType & 0x0080) != 0, $"Hex digit flag not set for '{testStr[i]}'");
        }
    }

    [Fact]
    public void GetStringTypeA_WithSpaces_ShouldSetSpaceAndBlankFlags()
    {
        var env = new TestEnvironment();
        var testStr = " \t";
        var strAddr = env.WriteString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u, 1u, strAddr, testStr.Length, resultAddr);

        Assert.Equal(1u, result);

        // CT_CTYPE1_SPACE = 0x0008, CT_CTYPE1_BLANK = 0x0040
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0008) != 0, $"Space flag not set for char {i}");
            Assert.True((charType & 0x0040) != 0, $"Blank flag not set for char {i}");
        }
    }

    [Fact]
    public void GetStringTypeA_WithNewlines_ShouldSetSpaceButNotBlankFlag()
    {
        var env = new TestEnvironment();
        var testStr = "\n\r\f\v";
        var strAddr = env.WriteString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u, 1u, strAddr, testStr.Length, resultAddr);

        Assert.Equal(1u, result);

        // CT_CTYPE1_SPACE = 0x0008, CT_CTYPE1_BLANK = 0x0040
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0008) != 0, $"Space flag not set for char {i}");
            Assert.True((charType & 0x0040) == 0, $"Blank flag should NOT be set for char {i}");
        }
    }

    [Fact]
    public void GetStringTypeA_WithPunctuation_ShouldSetPunctFlag()
    {
        var env = new TestEnvironment();
        var testStr = "!@#$%^&*()";
        var strAddr = env.WriteString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u, 1u, strAddr, testStr.Length, resultAddr);

        Assert.Equal(1u, result);

        // CT_CTYPE1_PUNCT = 0x0010
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0010) != 0, $"Punct flag not set for '{testStr[i]}'");
        }
    }

    [Fact]
    public void GetStringTypeA_WithControlChars_ShouldSetCntrlFlag()
    {
        var env = new TestEnvironment();
        var testStr = "\x00\x01\x1F\x7F"; // null, SOH, unit separator, DEL
        var strAddr = env.WriteString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u, 1u, strAddr, testStr.Length, resultAddr);

        Assert.Equal(1u, result);

        // CT_CTYPE1_CNTRL = 0x0020
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0020) != 0, $"Control flag not set for char {i} (0x{(int)testStr[i]:X2})");
        }
    }

    [Fact]
    public void GetStringTypeA_WithNegativeLength_ShouldCalculateLength()
    {
        var env = new TestEnvironment();
        var testStr = "Test";
        var strAddr = env.WriteString(testStr + "\0"); // Add null terminator
        var resultAddr = env.Malloc(10 * 2); // Buffer for up to 10 chars

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u,      // locale
            1u,      // CT_CTYPE1
            strAddr, // string pointer
            -1,      // cchSrc = -1 means calculate from null terminator
            resultAddr);

        Assert.Equal(1u, result);

        // Should have processed 4 characters (not including null terminator)
        // Verify first character is correct
        var firstCharType = env.Memory.Read16(resultAddr);
        Assert.True((firstCharType & 0x0001) != 0); // 'T' should be uppercase
    }

    [Fact]
    public void GetStringTypeA_WithInvalidInfoType_ShouldReturnFalse()
    {
        var env = new TestEnvironment();
        var testStr = "Test";
        var strAddr = env.WriteString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u,
            99u,     // Invalid dwInfoType (only 1 is supported)
            strAddr,
            testStr.Length,
            resultAddr);

        Assert.Equal(0u, result); // FALSE
    }

    [Fact]
    public void GetStringTypeA_WithNullPointer_ShouldReturnFalse()
    {
        var env = new TestEnvironment();
        var resultAddr = env.Malloc(10 * 2);

        var result = env.Call("KERNEL32.DLL", "GetStringTypeA",
            0u,
            1u,
            0u,      // NULL string pointer
            5,
            resultAddr);

        Assert.Equal(0u, result); // FALSE
    }

    [Fact]
    public void GetStringTypeW_WithDigits_ShouldSetDigitFlag()
    {
        var env = new TestEnvironment();
        var testStr = "0123456789";
        var strAddr = env.WriteUnicodeString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeW",
            0u,             // locale
            1u,             // CT_CTYPE1
            strAddr,        // wide string pointer
            testStr.Length, // length in characters
            resultAddr);

        Assert.Equal(1u, result);

        // CT_CTYPE1_DIGIT = 0x0004
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0004) != 0, $"Digit flag not set for '{testStr[i]}'");
        }
    }

    [Fact]
    public void GetStringTypeW_WithUppercase_ShouldSetUpperAndAlphaFlags()
    {
        var env = new TestEnvironment();
        var testStr = "ABCDEF";
        var strAddr = env.WriteUnicodeString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeW",
            0u, 1u, strAddr, testStr.Length, resultAddr);

        Assert.Equal(1u, result);

        // CT_CTYPE1_UPPER = 0x0001, CT_CTYPE1_ALPHA = 0x0100
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0001) != 0, $"Upper flag not set for '{testStr[i]}'");
            Assert.True((charType & 0x0100) != 0, $"Alpha flag not set for '{testStr[i]}'");
        }
    }

    [Fact]
    public void GetStringTypeW_WithSpaces_ShouldSetSpaceFlag()
    {
        var env = new TestEnvironment();
        var testStr = " \t";
        var strAddr = env.WriteUnicodeString(testStr);
        var resultAddr = env.Malloc((uint)(testStr.Length * 2));

        var result = env.Call("KERNEL32.DLL", "GetStringTypeW",
            0u, 1u, strAddr, testStr.Length, resultAddr);

        Assert.Equal(1u, result);

        // CT_CTYPE1_SPACE = 0x0008
        for (int i = 0; i < testStr.Length; i++)
        {
            var charType = env.Memory.Read16(resultAddr + (uint)(i * 2));
            Assert.True((charType & 0x0008) != 0, $"Space flag not set for char {i}");
        }
    }

    [Fact]
    public void GetStringTypeW_WithNegativeLength_ShouldCalculateLength()
    {
        var env = new TestEnvironment();
        var testStr = "Test";
        var strAddr = env.WriteUnicodeString(testStr); // WriteUnicodeString adds null terminator
        var resultAddr = env.Malloc(10 * 2);

        var result = env.Call("KERNEL32.DLL", "GetStringTypeW",
            0u, 1u, strAddr, -1, resultAddr);

        Assert.Equal(1u, result);

        // Verify first character is correct
        var firstCharType = env.Memory.Read16(resultAddr);
        Assert.True((firstCharType & 0x0001) != 0); // 'T' should be uppercase
    }
}
