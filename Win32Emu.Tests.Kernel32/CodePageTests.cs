using Xunit;
using Win32Emu.Tests.Infrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for codepage-related Kernel32 functions (GetACP, GetOEMCP)
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class CodePageTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public CodePageTests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void GetACP_DefaultValue_ReturnsUtf8()
	{
		// Act
		var acp = _testEnv.CallKernel32Api("GETACP");

		// Assert - Default ANSI code page should be UTF-8 (65001)
		Assert.Equal(65001u, acp);
	}

	[Fact]
	public void GetOEMCP_DefaultValue_ReturnsOem437()
	{
		// Act
		var oemcp = _testEnv.CallKernel32Api("GETOEMCP");

		// Assert - Default OEM code page should be OEM US (437)
		Assert.Equal(437u, oemcp);
	}

	[Fact]
	public void GetACP_CustomValue_ReturnsConfiguredValue()
	{
		// Arrange - Set custom ANSI code page (Western European)
		_testEnv.ProcessEnv.AnsiCodePage = CodePage.WestEurope;

		// Act
		var acp = _testEnv.CallKernel32Api("GETACP");

		// Assert
		Assert.Equal(1252u, acp);
	}

	[Fact]
	public void GetOEMCP_CustomValue_ReturnsConfiguredValue()
	{
		// Arrange - Set custom OEM code page (Multilingual Latin I)
		_testEnv.ProcessEnv.OemCodePage = CodePage.OemMultilingualLatinI;

		// Act
		var oemcp = _testEnv.CallKernel32Api("GETOEMCP");

		// Assert
		Assert.Equal(850u, oemcp);
	}

	[Fact]
	public void GetACP_Japanese_ReturnsCorrectValue()
	{
		// Arrange - Set Japanese code page
		_testEnv.ProcessEnv.AnsiCodePage = CodePage.Japan;

		// Act
		var acp = _testEnv.CallKernel32Api("GETACP");

		// Assert
		Assert.Equal(932u, acp);
	}

	[Fact]
	public void GetOEMCP_Russian_ReturnsCorrectValue()
	{
		// Arrange - Set Russian OEM code page
		_testEnv.ProcessEnv.OemCodePage = CodePage.Ibm866;

		// Act
		var oemcp = _testEnv.CallKernel32Api("GETOEMCP");

		// Assert
		Assert.Equal(866u, oemcp);
	}

	public void Dispose()
	{
		_testEnv?.Dispose();
	}
}
