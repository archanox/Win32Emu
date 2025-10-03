using Win32Emu.Win32;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

public class DllModuleExportInfoTests
{
	[Fact]
	public void IsExportImplemented_ShouldReturnTrue_ForImplementedExport()
	{
		// Arrange & Act
		var isImplemented = DllModuleExportInfo.IsExportImplemented("DPlayXModule", "DirectPlayCreate");

		// Assert
		Assert.True(isImplemented);
	}

	[Fact]
	public void IsExportImplemented_ShouldReturnFalse_ForNonExistentExport()
	{
		// Arrange & Act
		var isImplemented = DllModuleExportInfo.IsExportImplemented("DPlayXModule", "NonExistentFunction");

		// Assert
		Assert.False(isImplemented);
	}

	[Fact]
	public void GetAllExports_ShouldReturnExportsWithAttributes()
	{
		// Arrange & Act
		var exports = DllModuleExportInfo.GetAllExports("DPlayXModule");

		// Assert
		Assert.NotEmpty(exports);
		Assert.Contains("DirectPlayCreate", exports.Keys);
		Assert.Contains("DirectPlayEnumerateA", exports.Keys);
		Assert.Equal(1u, exports["DirectPlayCreate"]);
		Assert.Equal(2u, exports["DirectPlayEnumerateA"]);
	}

	[Fact]
	public void IsExportImplemented_ShouldBeCaseInsensitive()
	{
		// Arrange & Act
		var isImplemented = DllModuleExportInfo.IsExportImplemented("DPlayXModule", "DIRECTPLAYCREATE");

		// Assert
		Assert.True(isImplemented);
	}
}
