using DiscUtils.Iso9660;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.VirtualFileSystem;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

public class DiskVirtualFileSystemTests : IDisposable
{
	private readonly string _testDir;

	public DiskVirtualFileSystemTests()
	{
		_testDir = Path.Combine(Path.GetTempPath(), $"DiskVFSTests_{Guid.NewGuid():N}");
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
		var diskPath = Path.Combine(_testDir, "nonexistent.vhd");

		// Act & Assert
		Assert.Throws<FileNotFoundException>(() => new DiskVirtualFileSystem(diskPath));
	}

	[Fact]
	public void Constructor_WithUnsupportedExtension_ThrowsException()
	{
		// Arrange
		var diskPath = Path.Combine(_testDir, "test.unsupported");
		File.WriteAllText(diskPath, "dummy content");

		// Act & Assert
		Assert.Throws<NotSupportedException>(() => new DiskVirtualFileSystem(diskPath));
	}

	[Fact]
	public void MountISO_WithValidFile_IsReadOnly()
	{
		// Arrange
		var isoPath = Path.Combine(_testDir, "test.iso");

		// Create a simple ISO file
		var builder = new CDBuilder
		{
			UseJoliet = true,
			VolumeIdentifier = "TEST"
		};
		builder.AddFile("readme.txt", System.Text.Encoding.UTF8.GetBytes("Hello from ISO"));
		builder.Build(isoPath);

		// Act
		using var vfs = new DiskVirtualFileSystem(isoPath, NullLogger.Instance);

		// Assert
		Assert.NotNull(vfs);
		Assert.True(vfs.FileExists("/readme.txt"));
		
		// ISO should be read-only
		Assert.Throws<InvalidOperationException>(() => 
			vfs.OpenFile("/newfile.txt", VfsFileMode.Create, VfsFileAccess.Write));
	}

	[Fact]
	public void CopyDirectoryIn_ToReadOnlyDisk_ThrowsException()
	{
		// Arrange
		var isoPath = Path.Combine(_testDir, "test.iso");
		var builder = new CDBuilder { UseJoliet = true, VolumeIdentifier = "TEST" };
		builder.Build(isoPath);

		using var vfs = new DiskVirtualFileSystem(isoPath, NullLogger.Instance);

		var sourceDir = Path.Combine(_testDir, "source");
		Directory.CreateDirectory(sourceDir);
		File.WriteAllText(Path.Combine(sourceDir, "file.txt"), "test");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => vfs.CopyDirectoryIn(sourceDir, "/"));
	}

	[Fact]
	public void ToWindowsPath_ConvertsToWindowsStyle()
	{
		// Arrange
		var isoPath = Path.Combine(_testDir, "test.iso");
		var builder = new CDBuilder { UseJoliet = true, VolumeIdentifier = "TEST" };
		builder.Build(isoPath);

		using var vfs = new DiskVirtualFileSystem(isoPath, NullLogger.Instance);

		// Act & Assert
		Assert.Equal(@"C:\test.txt", vfs.ToWindowsPath("/test.txt"));
		Assert.Equal(@"C:\subdir\file.txt", vfs.ToWindowsPath("/subdir/file.txt"));
	}

	[Fact]
	public void GetFiles_WithISOFileSystem_ReturnsFiles()
	{
		// Arrange
		var isoPath = Path.Combine(_testDir, "test.iso");
		var builder = new CDBuilder { UseJoliet = true, VolumeIdentifier = "TEST" };
		builder.AddFile("file1.txt", System.Text.Encoding.UTF8.GetBytes("content"));
		builder.AddFile("file2.doc", System.Text.Encoding.UTF8.GetBytes("content"));
		builder.AddFile("readme.txt", System.Text.Encoding.UTF8.GetBytes("content"));
		builder.Build(isoPath);

		using var vfs = new DiskVirtualFileSystem(isoPath, NullLogger.Instance);

		// Act
		var txtFiles = vfs.GetFiles("/", "*.txt");

		// Assert
		Assert.Equal(2, txtFiles.Length);
		Assert.Contains("file1.txt", txtFiles);
		Assert.Contains("readme.txt", txtFiles);
	}

	[Fact]
	public void Create_VhdDisk_SuccessfullyCreatesAndFormats()
	{
		// Arrange
		var diskPath = Path.Combine(_testDir, "new.vhd");

		// Act
		using (var vfs = DiskVirtualFileSystem.Create(diskPath, DiskFormat.Vhd, 10 * 1024 * 1024, NullLogger.Instance))
		{
			// Assert
			Assert.NotNull(vfs);
			Assert.False(vfs.IsReadOnly);
		}
		
		// Verify the file was created
		Assert.True(File.Exists(diskPath));
	}

	[Fact]
	public void Create_VhdxDisk_SuccessfullyCreatesAndFormats()
	{
		// Arrange
		var diskPath = Path.Combine(_testDir, "new.vhdx");

		// Act
		using (var vfs = DiskVirtualFileSystem.Create(diskPath, DiskFormat.Vhdx, 10 * 1024 * 1024, NullLogger.Instance))
		{
			// Assert
			Assert.NotNull(vfs);
			Assert.False(vfs.IsReadOnly);
		}
		
		// Verify the file was created
		Assert.True(File.Exists(diskPath));
	}

	[Fact]
	public void Create_VhdDisk_CanBeReopenedAfterCreation()
	{
		// Arrange
		var diskPath = Path.Combine(_testDir, "reopenable.vhd");

		// Act
		using (var vfs1 = DiskVirtualFileSystem.Create(diskPath, DiskFormat.Vhd, 10 * 1024 * 1024, NullLogger.Instance))
		{
			Assert.NotNull(vfs1);
		}
		
		// Verify we can reopen the disk after creation
		using (var vfs2 = new DiskVirtualFileSystem(diskPath, NullLogger.Instance))
		{
			Assert.NotNull(vfs2);
			Assert.False(vfs2.IsReadOnly);
		}
	}

	[Fact]
	public void ReadFile_FromISO_ReturnsCorrectContent()
	{
		// Arrange
		var isoPath = Path.Combine(_testDir, "test.iso");
		var builder = new CDBuilder { UseJoliet = true, VolumeIdentifier = "TEST" };
		var expectedContent = "Hello from ISO file!";
		builder.AddFile("test.txt", System.Text.Encoding.UTF8.GetBytes(expectedContent));
		builder.Build(isoPath);

		using var vfs = new DiskVirtualFileSystem(isoPath, NullLogger.Instance);

		// Act
		using var handle = vfs.OpenFile("/test.txt", VfsFileMode.Open, VfsFileAccess.Read);
		Assert.NotNull(handle);
		
		var buffer = new byte[100];
		var bytesRead = handle.Read(buffer, 0, buffer.Length);
		var actualContent = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

		// Assert
		Assert.Equal(expectedContent, actualContent);
	}
}
