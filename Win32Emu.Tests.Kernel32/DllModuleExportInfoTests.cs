using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

public class DllModuleExportInfoTests
{
	[Fact]
	public void IsExportImplemented_ShouldReturnTrue_ForImplementedExport()
	{
		// Arrange
		var moduleType = typeof(DPlayXModule);

		// Act
		var isImplemented = DllModuleExportInfo.IsExportImplemented(moduleType, "DirectPlayCreate");

		// Assert
		Assert.True(isImplemented);
	}

	[Fact]
	public void IsExportImplemented_ShouldReturnFalse_ForNonExistentExport()
	{
		// Arrange
		var moduleType = typeof(DPlayXModule);

		// Act
		var isImplemented = DllModuleExportInfo.IsExportImplemented(moduleType, "NonExistentFunction");

		// Assert
		Assert.False(isImplemented);
	}

	[Fact]
	public void GetAllExports_ShouldReturnExportsWithAttributes()
	{
		// Arrange
		var moduleType = typeof(DPlayXModule);

		// Act
		var exports = DllModuleExportInfo.GetAllExports(moduleType);

		// Assert
		Assert.NotEmpty(exports);
		Assert.Contains("DirectPlayCreate", exports.Keys);
		Assert.Contains("DirectPlayEnumerateA", exports.Keys);
		Assert.Equal(1u, exports["DirectPlayCreate"]);
		Assert.Equal(2u, exports["DirectPlayEnumerateA"]);
	}

	[Fact]
	public void GetExportAttributes_ShouldReturnAttributes_ForExportWithAttribute()
	{
		// Arrange
		var moduleType = typeof(DPlayXModule);

		// Act
		var attributes = DllModuleExportInfo.GetExportAttributes(moduleType, "DirectPlayCreate");

		// Assert
		Assert.NotEmpty(attributes);
		Assert.Equal(1u, attributes[0].Ordinal);
	}

	[Fact]
	public void GetExportAttributes_ShouldReturnEmpty_ForNonExistentExport()
	{
		// Arrange
		var moduleType = typeof(DPlayXModule);

		// Act
		var attributes = DllModuleExportInfo.GetExportAttributes(moduleType, "NonExistentFunction");

		// Assert
		Assert.Empty(attributes);
	}

	[Fact]
	public void IsExportImplemented_ShouldBeCaseInsensitive()
	{
		// Arrange
		var moduleType = typeof(DPlayXModule);

		// Act
		var isImplemented = DllModuleExportInfo.IsExportImplemented(moduleType, "DIRECTPLAYCREATE");

		// Assert
		Assert.True(isImplemented);
	}
}
