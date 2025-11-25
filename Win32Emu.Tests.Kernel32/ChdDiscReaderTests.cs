using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.VirtualFileSystem;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

[Trait("Category", "DllModuleTests")]
public class ChdDiscReaderTests : IDisposable
{
	private readonly string _testDir;

	public ChdDiscReaderTests()
	{
		_testDir = Path.Combine(Path.GetTempPath(), $"ChdTests_{Guid.NewGuid():N}");
		Directory.CreateDirectory(_testDir);
	}

	public void Dispose()
	{
		if (Directory.Exists(_testDir))
		{
			Directory.Delete(_testDir, true);
		}
	}

	[Fact]
	public void Constructor_WithNonExistentFile_ThrowsException()
	{
		// Arrange
		var chdPath = Path.Combine(_testDir, "nonexistent.chd");

		// Act & Assert
		Assert.Throws<FileNotFoundException>(() => new ChdDiscReader(chdPath, NullLogger.Instance));
	}

	[Fact]
	public void Constructor_WithInvalidChdFile_CreatesReaderButIsNotValid()
	{
		// Arrange
		var chdPath = Path.Combine(_testDir, "invalid.chd");
		File.WriteAllText(chdPath, "This is not a valid CHD file");

		// Act
		using var reader = new ChdDiscReader(chdPath, NullLogger.Instance);

		// Assert
		Assert.NotNull(reader);
		Assert.False(reader.IsValid); // Should be invalid because file doesn't have CHD signature
	}

	[Fact]
	public void DiskVirtualFileSystem_WithInvalidChdExtension_ThrowsInvalidOperationException()
	{
		// Arrange
		var chdPath = Path.Combine(_testDir, "test.chd");
		File.WriteAllText(chdPath, "This is not a valid CHD file");

		// Act & Assert
		// DiskVirtualFileSystem should throw InvalidOperationException for invalid CHD files
		Assert.Throws<InvalidOperationException>(() => new DiskVirtualFileSystem(chdPath, NullLogger.Instance));
	}
}
