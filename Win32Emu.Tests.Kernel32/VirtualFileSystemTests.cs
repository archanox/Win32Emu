using Win32Emu.VirtualFileSystem;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for Virtual File System functionality
/// </summary>
public class VirtualFileSystemTests : IDisposable
{
	private readonly string _testBaseDir;
	private readonly string _testOverlayDir;
	private readonly LayeredVirtualFileSystem _vfs;

	public VirtualFileSystemTests()
	{
		// Create temporary test directories
		_testBaseDir = Path.Combine(Path.GetTempPath(), "VfsTest_Base_" + Guid.NewGuid().ToString("N"));
		_testOverlayDir = Path.Combine(Path.GetTempPath(), "VfsTest_Overlay_" + Guid.NewGuid().ToString("N"));

		Directory.CreateDirectory(_testBaseDir);
		Directory.CreateDirectory(_testOverlayDir);

		// Create test files in base directory
		File.WriteAllText(Path.Combine(_testBaseDir, "readonly.txt"), "Original content");
		File.WriteAllText(Path.Combine(_testBaseDir, "tomodify.txt"), "Original content");

		_vfs = new LayeredVirtualFileSystem(_testBaseDir, _testOverlayDir);
	}

	public void Dispose()
	{
		// Clean up test directories
		try
		{
			if (Directory.Exists(_testBaseDir))
				Directory.Delete(_testBaseDir, true);
			if (Directory.Exists(_testOverlayDir))
				Directory.Delete(_testOverlayDir, true);
		}
		catch
		{
			// Ignore cleanup errors
		}
	}

	[Fact]
	public void FileExists_WithFileInBase_ShouldReturnTrue()
	{
		// Act
		var exists = _vfs.FileExists("readonly.txt");

		// Assert
		Assert.True(exists);
	}

	[Fact]
	public void FileExists_WithNonExistentFile_ShouldReturnFalse()
	{
		// Act
		var exists = _vfs.FileExists("nonexistent.txt");

		// Assert
		Assert.False(exists);
	}

	[Fact]
	public void OpenFile_ForRead_ShouldReadFromBase()
	{
		// Act
		using var handle = _vfs.OpenFile("readonly.txt", VfsFileMode.Open, VfsFileAccess.Read);

		// Assert
		Assert.NotNull(handle);

		var buffer = new byte[100];
		var bytesRead = handle.Read(buffer, 0, buffer.Length);
		var content = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

		Assert.Equal("Original content", content);
	}

	[Fact]
	public void OpenFile_ForWrite_ShouldCopyToOverlay()
	{
		// Act - Write to a file that exists in base
		using (var handle = _vfs.OpenFile("tomodify.txt", VfsFileMode.Open, VfsFileAccess.Write))
		{
			Assert.NotNull(handle);

			var newContent = System.Text.Encoding.UTF8.GetBytes("Modified content");
			handle.Write(newContent, 0, newContent.Length);
		}

		// Assert - Base file should be unchanged
		var baseContent = File.ReadAllText(Path.Combine(_testBaseDir, "tomodify.txt"));
		Assert.Equal("Original content", baseContent);

		// Overlay should have the modified file
		var overlayPath = Path.Combine(_testOverlayDir, "tomodify.txt");
		Assert.True(File.Exists(overlayPath));
	}

	[Fact]
	public void OpenFile_NewFile_ShouldCreateInOverlay()
	{
		// Act
		using (var handle = _vfs.OpenFile("newfile.txt", VfsFileMode.Create, VfsFileAccess.Write))
		{
			Assert.NotNull(handle);

			var content = System.Text.Encoding.UTF8.GetBytes("New file content");
			handle.Write(content, 0, content.Length);
		}

		// Assert - File should not exist in base
		Assert.False(File.Exists(Path.Combine(_testBaseDir, "newfile.txt")));

		// File should exist in overlay
		var overlayPath = Path.Combine(_testOverlayDir, "newfile.txt");
		Assert.True(File.Exists(overlayPath));

		var fileContent = File.ReadAllText(overlayPath);
		Assert.Equal("New file content", fileContent);
	}

	[Fact]
	public void DeleteFile_WithFileInBase_ShouldSucceed()
	{
		// Act
		var success = _vfs.DeleteFile("readonly.txt");

		// Assert
		Assert.True(success);

		// Base file should still exist (read-only)
		Assert.True(File.Exists(Path.Combine(_testBaseDir, "readonly.txt")));
	}

	[Fact]
	public void DeleteFile_WithFileInOverlay_ShouldRemoveFromOverlay()
	{
		// Arrange - Create a file in overlay first
		using (var handle = _vfs.OpenFile("overlayfile.txt", VfsFileMode.Create, VfsFileAccess.Write))
		{
			var content = System.Text.Encoding.UTF8.GetBytes("Overlay content");
			handle!.Write(content, 0, content.Length);
		}

		// Act
		var success = _vfs.DeleteFile("overlayfile.txt");

		// Assert
		Assert.True(success);
		Assert.False(File.Exists(Path.Combine(_testOverlayDir, "overlayfile.txt")));
	}

	[Fact]
	public void MoveFile_ShouldMoveInOverlay()
	{
		// Arrange - Create a file first
		using (var handle = _vfs.OpenFile("source.txt", VfsFileMode.Create, VfsFileAccess.Write))
		{
			var content = System.Text.Encoding.UTF8.GetBytes("Source content");
			handle!.Write(content, 0, content.Length);
		}

		// Act
		var success = _vfs.MoveFile("source.txt", "destination.txt");

		// Assert
		Assert.True(success);
		Assert.False(_vfs.FileExists("source.txt"));
		Assert.True(_vfs.FileExists("destination.txt"));
	}

	[Fact]
	public void GetFiles_ShouldReturnFilesFromBothLayers()
	{
		// Arrange - Add file to overlay
		using (var handle = _vfs.OpenFile("overlayonly.txt", VfsFileMode.Create, VfsFileAccess.Write))
		{
			var content = System.Text.Encoding.UTF8.GetBytes("Overlay only");
			handle!.Write(content, 0, content.Length);
		}

		// Act
		var files = _vfs.GetFiles(".", "*.txt");

		// Assert - Should include files from both base and overlay
		Assert.Contains("readonly.txt", files);
		Assert.Contains("tomodify.txt", files);
		Assert.Contains("overlayonly.txt", files);
	}

	[Fact]
	public void Seek_ShouldSetPosition()
	{
		// Arrange
		using var handle = _vfs.OpenFile("readonly.txt", VfsFileMode.Open, VfsFileAccess.Read);
		Assert.NotNull(handle);

		// Act
		var position = handle.Seek(5, SeekOrigin.Begin);

		// Assert
		Assert.Equal(5, position);
		Assert.Equal(5, handle.Position);
	}

	[Fact]
	public void SetLength_ShouldTruncateFile()
	{
		// Arrange
		using var handle = _vfs.OpenFile("truncate.txt", VfsFileMode.Create, VfsFileAccess.ReadWrite);
		Assert.NotNull(handle);

		var content = System.Text.Encoding.UTF8.GetBytes("Long content that will be truncated");
		handle.Write(content, 0, content.Length);

		// Act
		handle.Seek(0, SeekOrigin.Begin);
		handle.SetLength(4);

		// Assert
		var buffer = new byte[100];
		var bytesRead = handle.Read(buffer, 0, buffer.Length);
		Assert.Equal(4, bytesRead);
	}

	[Fact]
	public void PathNormalization_ShouldHandleWindowsPaths()
	{
		// Act - Try to create file with Windows-style path
		using (var handle = _vfs.OpenFile(@"C:\test\file.txt", VfsFileMode.Create, VfsFileAccess.Write))
		{
			Assert.NotNull(handle);
			var content = System.Text.Encoding.UTF8.GetBytes("Test");
			handle.Write(content, 0, content.Length);
		}

		// Assert - Should normalize path and create in overlay
		var overlayPath = Path.Combine(_testOverlayDir, "test", "file.txt");
		Assert.True(File.Exists(overlayPath));
	}

	[Fact]
	public void ToWindowsPath_WithPathUnderBase_ShouldReturnVirtualizedPath()
	{
		// Arrange
		var realPath = Path.Combine(_testBaseDir, "readonly.txt");

		// Act
		var windowsPath = _vfs.ToWindowsPath(realPath);

		// Assert
		Assert.Equal(@"C:\readonly.txt", windowsPath);
	}

	[Fact]
	public void ToWindowsPath_WithSubdirectoryPath_ShouldReturnVirtualizedPath()
	{
		// Arrange - Create a subdirectory structure
		var subDir = Path.Combine(_testBaseDir, "subdir");
		Directory.CreateDirectory(subDir);
		var filePath = Path.Combine(subDir, "test.dat");
		File.WriteAllText(filePath, "test");

		// Act
		var windowsPath = _vfs.ToWindowsPath(filePath);

		// Assert
		Assert.Equal(@"C:\subdir\test.dat", windowsPath);
	}

	[Fact]
	public void ToWindowsPath_WithPathOutsideBase_ShouldReturnOriginal()
	{
		// Arrange
		var outsidePath = Path.Combine(Path.GetTempPath(), "outside.txt");

		// Act
		var result = _vfs.ToWindowsPath(outsidePath);

		// Assert
		Assert.Equal(outsidePath, result);
	}
}
