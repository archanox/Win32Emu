using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for GetStringTypeW blank flag handling bug fix.
/// This test verifies that space and tab characters correctly receive both Space and Blank flags.
/// 
/// Bug: GetStringTypeW had an else-if chain that prevented space and tab from receiving
/// both CT_CTYPE1_SPACE and CT_CTYPE1_BLANK flags. The second check for blank was unreachable.
/// </summary>
[Trait("Category", "DllModuleTests")]
public class GetStringTypeWBlankFlagTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public GetStringTypeWBlankFlagTests()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}

	[Fact]
	public void GetStringTypeW_WithSpace_ShouldHaveBothSpaceAndBlankFlags()
	{
		// Arrange
		const ushort ctCtype1Space = 0x0008;
		const ushort ctCtype1Blank = 0x0040;

		// Write a Unicode string containing a space character
		const uint stringAddr = 0x1000;
		_testEnv.Memory.Write16(stringAddr, (ushort)' '); // Space character
		_testEnv.Memory.Write16(stringAddr + 2, 0); // Null terminator

		// Prepare output buffer for character types
		const uint outputAddr = 0x2000;

		// Act - Call GetStringTypeW
		// Parameters: dwInfoType, lpSrcStr, cchSrc, lpCharType (no Locale parameter for GetStringTypeW)
		_testEnv.CallKernel32Api("GETSTRINGTYPEW", 1, stringAddr, unchecked((uint)-1), outputAddr);

		// Assert
		var charType = _testEnv.Memory.Read16(outputAddr);

		// Space should have BOTH Space and Blank flags set
		Assert.True((charType & ctCtype1Space) != 0, "Space character should have CT_CTYPE1_SPACE flag");
		Assert.True((charType & ctCtype1Blank) != 0, "Space character should have CT_CTYPE1_BLANK flag");
	}

	[Fact]
	public void GetStringTypeW_WithTab_ShouldHaveBothSpaceAndBlankFlags()
	{
		// Arrange
		const ushort ctCtype1Space = 0x0008;
		const ushort ctCtype1Blank = 0x0040;

		// Write a Unicode string containing a tab character
		const uint stringAddr = 0x1000;
		_testEnv.Memory.Write16(stringAddr, (ushort)'\t'); // Tab character
		_testEnv.Memory.Write16(stringAddr + 2, 0); // Null terminator

		// Prepare output buffer for character types
		const uint outputAddr = 0x2000;

		// Act - Call GetStringTypeW
		// Parameters: dwInfoType, lpSrcStr, cchSrc, lpCharType (no Locale parameter for GetStringTypeW)
		_testEnv.CallKernel32Api("GETSTRINGTYPEW", 1, stringAddr, unchecked((uint)-1), outputAddr);

		// Assert
		var charType = _testEnv.Memory.Read16(outputAddr);

		// Tab should have BOTH Space and Blank flags set
		Assert.True((charType & ctCtype1Space) != 0, "Tab character should have CT_CTYPE1_SPACE flag");
		Assert.True((charType & ctCtype1Blank) != 0, "Tab character should have CT_CTYPE1_BLANK flag");
	}

	[Fact]
	public void GetStringTypeW_WithNewline_ShouldHaveSpaceButNotBlankFlag()
	{
		// Arrange
		const ushort ctCtype1Space = 0x0008;
		const ushort ctCtype1Blank = 0x0040;

		// Write a Unicode string containing a newline character
		const uint stringAddr = 0x1000;
		_testEnv.Memory.Write16(stringAddr, (ushort)'\n'); // Newline character
		_testEnv.Memory.Write16(stringAddr + 2, 0); // Null terminator

		// Prepare output buffer for character types
		const uint outputAddr = 0x2000;

		// Act - Call GetStringTypeW
		// Parameters: dwInfoType, lpSrcStr, cchSrc, lpCharType (no Locale parameter for GetStringTypeW)
		_testEnv.CallKernel32Api("GETSTRINGTYPEW", 1, stringAddr, unchecked((uint)-1), outputAddr);

		// Assert
		var charType = _testEnv.Memory.Read16(outputAddr);

		// Newline should have Space flag but NOT Blank flag
		Assert.True((charType & ctCtype1Space) != 0, "Newline character should have CT_CTYPE1_SPACE flag");
		Assert.False((charType & ctCtype1Blank) != 0, "Newline character should NOT have CT_CTYPE1_BLANK flag");
	}

	[Fact]
	public void GetStringTypeW_WithCarriageReturn_ShouldHaveSpaceButNotBlankFlag()
	{
		// Arrange
		const ushort ctCtype1Space = 0x0008;
		const ushort ctCtype1Blank = 0x0040;

		// Write a Unicode string containing a carriage return character
		const uint stringAddr = 0x1000;
		_testEnv.Memory.Write16(stringAddr, (ushort)'\r'); // Carriage return character
		_testEnv.Memory.Write16(stringAddr + 2, 0); // Null terminator

		// Prepare output buffer for character types
		const uint outputAddr = 0x2000;

		// Act - Call GetStringTypeW
		// Parameters: dwInfoType, lpSrcStr, cchSrc, lpCharType (no Locale parameter for GetStringTypeW)
		_testEnv.CallKernel32Api("GETSTRINGTYPEW", 1, stringAddr, unchecked((uint)-1), outputAddr);

		// Assert
		var charType = _testEnv.Memory.Read16(outputAddr);

		// Carriage return should have Space flag but NOT Blank flag
		Assert.True((charType & ctCtype1Space) != 0, "Carriage return character should have CT_CTYPE1_SPACE flag");
		Assert.False((charType & ctCtype1Blank) != 0, "Carriage return character should NOT have CT_CTYPE1_BLANK flag");
	}

	[Fact]
	public void GetStringTypeW_WithMixedWhitespace_ShouldClassifyCorrectly()
	{
		// Arrange
		const ushort ctCtype1Space = 0x0008;
		const ushort ctCtype1Blank = 0x0040;

		// Write a Unicode string with various whitespace characters: " \t\n\r"
		const uint stringAddr = 0x1000;
		_testEnv.Memory.Write16(stringAddr, (ushort)' ');
		_testEnv.Memory.Write16(stringAddr + 2, (ushort)'\t');
		_testEnv.Memory.Write16(stringAddr + 4, (ushort)'\n');
		_testEnv.Memory.Write16(stringAddr + 6, (ushort)'\r');
		_testEnv.Memory.Write16(stringAddr + 8, 0); // Null terminator

		// Prepare output buffer for character types
		const uint outputAddr = 0x2000;

		// Act - Call GetStringTypeW with explicit length
		// Parameters: dwInfoType, lpSrcStr, cchSrc, lpCharType (no Locale parameter for GetStringTypeW)
		_testEnv.CallKernel32Api("GETSTRINGTYPEW", 1, stringAddr, 4, outputAddr);

		// Assert
		var spaceType = _testEnv.Memory.Read16(outputAddr);
		var tabType = _testEnv.Memory.Read16(outputAddr + 2);
		var newlineType = _testEnv.Memory.Read16(outputAddr + 4);
		var crType = _testEnv.Memory.Read16(outputAddr + 6);

		// Space: both space and blank
		Assert.True((spaceType & ctCtype1Space) != 0, "Space should have Space flag");
		Assert.True((spaceType & ctCtype1Blank) != 0, "Space should have Blank flag");

		// Tab: both space and blank
		Assert.True((tabType & ctCtype1Space) != 0, "Tab should have Space flag");
		Assert.True((tabType & ctCtype1Blank) != 0, "Tab should have Blank flag");

		// Newline: space but not blank
		Assert.True((newlineType & ctCtype1Space) != 0, "Newline should have Space flag");
		Assert.False((newlineType & ctCtype1Blank) != 0, "Newline should NOT have Blank flag");

		// Carriage return: space but not blank
		Assert.True((crType & ctCtype1Space) != 0, "CR should have Space flag");
		Assert.False((crType & ctCtype1Blank) != 0, "CR should NOT have Blank flag");
	}
}
