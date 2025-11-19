using Win32Emu.Win32;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for ordinal-based function resolution
/// </summary>
public class OrdinalResolutionTests
{
	[Fact]
	public void ResolveOrdinalExport_ShouldReturnMethodName_ForValidOrdinal()
	{
		// Arrange - DPLAYX.DLL ordinal 1 should resolve to DirectPlayCreate
		var dllName = "DPLAYX.DLL";
		var ordinalExport = "ORDINAL_1";

		// Act
		var resolvedName = DllModuleExportInfo.ResolveOrdinalExport(dllName, ordinalExport);

		// Assert
		Assert.Equal("DirectPlayCreate", resolvedName);
	}

	[Fact]
	public void ResolveOrdinalExport_ShouldReturnOriginalName_WhenNotOrdinalFormat()
	{
		// Arrange
		var dllName = "DPLAYX.DLL";
		var functionName = "DirectPlayCreate";

		// Act
		var resolvedName = DllModuleExportInfo.ResolveOrdinalExport(dllName, functionName);

		// Assert
		Assert.Equal("DirectPlayCreate", resolvedName);
	}

	[Fact]
	public void ResolveOrdinalExport_ShouldReturnOriginalName_ForUnknownOrdinal()
	{
		// Arrange - ordinal 999 does not exist
		var dllName = "DPLAYX.DLL";
		var ordinalExport = "ORDINAL_999";

		// Act
		var resolvedName = DllModuleExportInfo.ResolveOrdinalExport(dllName, ordinalExport);

		// Assert - should return original when ordinal not found
		Assert.Equal("ORDINAL_999", resolvedName);
	}

	[Fact]
	public void ResolveOrdinalExport_ShouldBeCaseInsensitive()
	{
		// Arrange - test lowercase "ordinal_1"
		var dllName = "DPLAYX.DLL";
		var ordinalExport = "ordinal_1";

		// Act
		var resolvedName = DllModuleExportInfo.ResolveOrdinalExport(dllName, ordinalExport);

		// Assert
		Assert.Equal("DirectPlayCreate", resolvedName);
	}

	[Fact]
	public void ResolveOrdinalExport_ShouldWorkForDifferentOrdinals()
	{
		// Arrange - test ordinal 2 and 4
		var dllName = "DPLAYX.DLL";

		// Act & Assert
		Assert.Equal("DirectPlayEnumerateA", DllModuleExportInfo.ResolveOrdinalExport(dllName, "ORDINAL_2"));
		Assert.Equal("DirectPlayLobbyCreateA", DllModuleExportInfo.ResolveOrdinalExport(dllName, "ORDINAL_4"));
	}

	[Fact]
	public void IsExportImplemented_ShouldWorkWithOrdinalExport()
	{
		// Arrange - test that IsExportImplemented works with ordinal-based export names
		var dllName = "DPLAYX.DLL";
		var ordinalExport = "ORDINAL_1";

		// Act
		var isImplemented = DllModuleExportInfo.IsExportImplemented(dllName, ordinalExport);

		// Assert - should resolve ordinal and find the implementation
		Assert.True(isImplemented);
	}

	[Fact]
	public void IsExportStub_ShouldWorkWithOrdinalExport()
	{
		// Arrange - DPLAYX functions are stubs
		var dllName = "DPLAYX.DLL";
		var ordinalExport = "ORDINAL_1";

		// Act
		var isStub = DllModuleExportInfo.IsExportStub(dllName, ordinalExport);

		// Assert - DirectPlayCreate is marked as IsStub = true
		Assert.True(isStub);
	}

	[Fact]
	public void TryGetArgBytes_ShouldWorkWithOrdinalExport()
	{
		// Arrange - DirectPlayCreate (ordinal 1) has 3 uint32 parameters = 12 bytes
		var dllName = "DPLAYX.DLL";
		var ordinalExport = "ORDINAL_1";

		// Act
		var success = StdCallMeta.TryGetArgBytes(dllName, ordinalExport, out var argBytes);

		// Assert
		Assert.True(success);
		Assert.Equal(12, argBytes); // 3 parameters * 4 bytes each
	}
}
