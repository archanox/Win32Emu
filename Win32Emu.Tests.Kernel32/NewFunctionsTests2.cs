using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for newly implemented Kernel32 functions (16-bit thunking, string validation, charset)
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class NewFunctionsTests2 : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public NewFunctionsTests2()
	{
		_testEnv = new TestEnvironment();
	}

	#region 16-bit Thunking Function Tests

	[Fact]
	public void FT_Exit4_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_EXIT4");

		// Assert - These are stubs for 16-bit compatibility
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Exit8_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_EXIT8");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Exit12_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_EXIT12");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Exit16_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_EXIT16");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Exit20_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_EXIT20");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Exit24_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_EXIT24");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Exit28_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_EXIT28");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Exit32_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_EXIT32");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Exit48_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_EXIT48");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Prolog_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_PROLOG");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void FT_Thunk_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("FT_THUNK");

		// Assert
		Assert.Equal(0u, result);
	}

	#endregion

	#region Memory Mapping Function Tests

	[Fact]
	public void SMapLS_ShouldReturnSameAddress()
	{
		// Arrange
		const uint testAddress = 0x12345678;

		// Act
		var result = _testEnv.CallKernel32Api("SMAPLS", testAddress);

		// Assert - In flat 32-bit mode, should return address unchanged
		Assert.Equal(testAddress, result);
	}

	[Fact]
	public void SUnMapLS_ShouldReturnZero()
	{
		// Arrange
		const uint testAddress = 0x12345678;

		// Act
		var result = _testEnv.CallKernel32Api("SUNMAPLS", testAddress);

		// Assert - No-op, returns 0
		Assert.Equal(0u, result);
	}

	[Fact]
	public void MapLS_ShouldReturnSameAddress()
	{
		// Arrange
		const uint testAddress = 0x87654321;

		// Act
		var result = _testEnv.CallKernel32Api("MAPLS", testAddress);

		// Assert - In flat 32-bit mode, should return address unchanged
		Assert.Equal(testAddress, result);
	}

	[Fact]
	public void MapSL_ShouldReturnSameAddress()
	{
		// Arrange
		const uint testAddress = 0xABCDEF00;

		// Act
		var result = _testEnv.CallKernel32Api("MAPSL", testAddress);

		// Assert - In flat 32-bit mode, should return address unchanged
		Assert.Equal(testAddress, result);
	}

	[Fact]
	public void MapHInstLS_ShouldReturnSameHandle()
	{
		// Arrange
		const uint testHandle = 0x00400000;

		// Act
		var result = _testEnv.CallKernel32Api("MAPHINSTLS", testHandle);

		// Assert
		Assert.Equal(testHandle, result);
	}

	[Fact]
	public void MapHInstLS_PN_ShouldReturnHandle_WhenPointerValid()
	{
		// Arrange
		const uint testHandle = 0x00400000;
		var phInst = _testEnv.AllocateMemory(4);
		_testEnv.Memory.Write32(phInst, testHandle);

		// Act
		var result = _testEnv.CallKernel32Api("MAPHINSTLS_PN", phInst);

		// Assert
		Assert.Equal(testHandle, result);
	}

	[Fact]
	public void MapHInstLS_PN_ShouldReturnZero_WhenPointerNull()
	{
		// Act
		var result = _testEnv.CallKernel32Api("MAPHINSTLS_PN", 0u);

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void MapSLFix_ShouldReturnSameAddress()
	{
		// Arrange
		const uint testAddress = 0x11223344;

		// Act
		var result = _testEnv.CallKernel32Api("MAPSLFIX", testAddress);

		// Assert
		Assert.Equal(testAddress, result);
	}

	[Fact]
	public void UnMapSLFixArray_ShouldReturnZero()
	{
		// Arrange
		const uint cSelectors = 5;
		var lpSelectors = _testEnv.AllocateMemory(cSelectors * 4);

		// Act
		var result = _testEnv.CallKernel32Api("UNMAPSLFIXARRAY", cSelectors, lpSelectors);

		// Assert - No-op, returns 0
		Assert.Equal(0u, result);
	}

	[Fact]
	public void SMapLS_IP_EBP_8_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("SMAPLS_IP_EBP_8");

		// Assert - Stub implementation
		Assert.Equal(0u, result);
	}

	[Fact]
	public void SUnMapLS_IP_EBP_8_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("SUNMAPLS_IP_EBP_8");

		// Assert - Stub implementation
		Assert.Equal(0u, result);
	}

	#endregion

	#region Thunk Callback Function Tests

	[Fact]
	public void K32Thk1632Prolog_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("K32THK1632PROLOG");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void K32Thk1632Epilog_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("K32THK1632EPILOG");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void Callback16_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("CALLBACK16");

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void Callback20_ShouldReturnZero()
	{
		// Act
		var result = _testEnv.CallKernel32Api("CALLBACK20");

		// Assert
		Assert.Equal(0u, result);
	}

	#endregion

	#region String and Character Set Function Tests

	[Fact]
	public void IsBadStringPtrA_ShouldReturnTrue_WhenPointerIsNull()
	{
		// Act
		var result = _testEnv.CallKernel32Api("ISBADSTRINGPTRA", 0u, 100u);

		// Assert - NULL pointer is bad
		Assert.Equal(1u, result); // TRUE
	}

	[Fact]
	public void IsBadStringPtrA_ShouldReturnFalse_WhenPointerIsValid()
	{
		// Arrange
		var testString = _testEnv.WriteString("Hello World");

		// Act
		var result = _testEnv.CallKernel32Api("ISBADSTRINGPTRA", testString, 100u);

		// Assert - Valid pointer is good
		Assert.Equal(0u, result); // FALSE
	}

	[Fact]
	public void IsBadStringPtrA_ShouldReturnFalse_WhenMaxLengthIsZero()
	{
		// Arrange
		var testString = _testEnv.WriteString("Test");

		// Act - ucchMax = 0 means check until null terminator
		var result = _testEnv.CallKernel32Api("ISBADSTRINGPTRA", testString, 0u);

		// Assert
		Assert.Equal(0u, result); // FALSE - valid string
	}

	[Fact]
	public void IsDBCSLeadByteEx_ShouldReturnFalse_ForAnsiCodePage()
	{
		// Arrange
		const uint CP_ACP = 0;   // ANSI code page
		const uint testByte = 0x41; // 'A'

		// Act
		var result = _testEnv.CallKernel32Api("ISDBCSLEADBYTEEX", CP_ACP, testByte);

		// Assert - ANSI is not DBCS
		Assert.Equal(0u, result); // FALSE
	}

	[Fact]
	public void IsDBCSLeadByteEx_ShouldReturnTrue_ForShiftJisLeadByte()
	{
		// Arrange
		const uint CP_SHIFTJIS = 932;
		const uint leadByte = 0x81; // Valid Shift-JIS lead byte

		// Act
		var result = _testEnv.CallKernel32Api("ISDBCSLEADBYTEEX", CP_SHIFTJIS, leadByte);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	[Fact]
	public void IsDBCSLeadByteEx_ShouldReturnFalse_ForShiftJisNonLeadByte()
	{
		// Arrange
		const uint CP_SHIFTJIS = 932;
		const uint nonLeadByte = 0x41; // ASCII 'A' - not a lead byte

		// Act
		var result = _testEnv.CallKernel32Api("ISDBCSLEADBYTEEX", CP_SHIFTJIS, nonLeadByte);

		// Assert
		Assert.Equal(0u, result); // FALSE
	}

	[Fact]
	public void IsDBCSLeadByteEx_ShouldReturnTrue_ForGbkLeadByte()
	{
		// Arrange
		const uint CP_GBK = 936; // Simplified Chinese
		const uint leadByte = 0xB0; // Valid GBK lead byte

		// Act
		var result = _testEnv.CallKernel32Api("ISDBCSLEADBYTEEX", CP_GBK, leadByte);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	[Fact]
	public void IsDBCSLeadByteEx_ShouldReturnTrue_ForKoreanLeadByte()
	{
		// Arrange
		const uint CP_KOREAN = 949;
		const uint leadByte = 0xC0; // Valid Korean lead byte

		// Act
		var result = _testEnv.CallKernel32Api("ISDBCSLEADBYTEEX", CP_KOREAN, leadByte);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	[Fact]
	public void IsDBCSLeadByteEx_ShouldReturnTrue_ForBig5LeadByte()
	{
		// Arrange
		const uint CP_BIG5 = 950; // Traditional Chinese
		const uint leadByte = 0xA4; // Valid Big5 lead byte

		// Act
		var result = _testEnv.CallKernel32Api("ISDBCSLEADBYTEEX", CP_BIG5, leadByte);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	#endregion

	public void Dispose()
	{
		_testEnv?.Dispose();
	}
}
