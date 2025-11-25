using Win32Emu.Win32;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

[Trait("Category", "DllModuleTests")]
public class DllModuleExportInfoTests
{
	[Fact]
	public void IsExportImplemented_ShouldReturnTrue_ForImplementedExport()
	{
		// Arrange & Act
		var isImplemented = DllModuleExportInfo.IsExportImplemented("DPLAYX.DLL", "DirectPlayCreate");

		// Assert
		Assert.True(isImplemented);
	}

	[Fact]
	public void IsExportImplemented_ShouldReturnFalse_ForNonExistentExport()
	{
		// Arrange & Act
		var isImplemented = DllModuleExportInfo.IsExportImplemented("DPLAYX.DLL", "NonExistentFunction");

		// Assert
		Assert.False(isImplemented);
	}

	[Fact]
	public void GetAllExports_ShouldReturnExportsWithAttributes()
	{
		// Arrange & Act
		var exports = DllModuleExportInfo.GetAllExports("DPLAYX.DLL");

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
		var isImplemented = DllModuleExportInfo.IsExportImplemented("dplayx.dll", "DIRECTPLAYCREATE");

		// Assert
		Assert.True(isImplemented);
	}

	[Fact]
	public void GetForwardedExport_ShouldReturnNull_ForNonForwardedExport()
	{
		// Arrange & Act
		var forwardedTo = DllModuleExportInfo.GetForwardedExport("DPLAYX.DLL", "DirectPlayCreate");

		// Assert
		Assert.Null(forwardedTo);
	}

	[Fact]
	public void GetForwardedExport_ShouldReturnNull_ForNonExistentExport()
	{
		// Arrange & Act
		var forwardedTo = DllModuleExportInfo.GetForwardedExport("DPLAYX.DLL", "NonExistentFunction");

		// Assert
		Assert.Null(forwardedTo);
	}

	[Fact]
	public void GetForwardedExport_ShouldReturnForwardingTarget_ForForwardedExport()
	{
		// Arrange & Act
		var forwardedTo = DllModuleExportInfo.GetForwardedExport("KERNEL32.DLL", "GetVersionEx");

		// Assert
		Assert.NotNull(forwardedTo);
		Assert.Equal("KERNELBASE.GetVersionEx", forwardedTo);
	}

	[Fact]
	public void IsExportStub_ShouldReturnTrue_ForStubExport()
	{
		// Arrange & Act
		var isStub = DllModuleExportInfo.IsExportStub("USER32.DLL", "ShowCursor");

		// Assert
		Assert.True(isStub);
	}

	[Fact]
	public void IsExportStub_ShouldReturnFalse_ForFullyImplementedExport()
	{
		// Arrange & Act
		var isStub = DllModuleExportInfo.IsExportStub("KERNEL32.DLL", "GetProcAddress");

		// Assert
		Assert.False(isStub);
	}

	[Fact]
	public void IsExportStub_ShouldReturnFalse_ForNonExistentExport()
	{
		// Arrange & Act
		var isStub = DllModuleExportInfo.IsExportStub("KERNEL32.DLL", "NonExistentFunction");

		// Assert
		Assert.False(isStub);
	}

	[Fact]
	public void IsExportStub_ShouldBeCaseInsensitive()
	{
		// Arrange & Act
		var isStub = DllModuleExportInfo.IsExportStub("user32.dll", "SHOWCURSOR");

		// Assert
		Assert.True(isStub);
	}
}
