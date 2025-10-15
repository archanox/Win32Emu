using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.VirtualFileSystem;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Integration tests for VFS with Kernel32 File I/O APIs
/// </summary>
public class VfsIntegrationTests : IDisposable
{
	private readonly string _testBaseDir;
	private readonly string _testOverlayDir;
	private readonly TestEnvironment _testEnv;

	public VfsIntegrationTests()
	{
		// Create temporary test directories
		_testBaseDir = Path.Combine(Path.GetTempPath(), "VfsIntTest_Base_" + Guid.NewGuid().ToString("N"));
		_testOverlayDir = Path.Combine(Path.GetTempPath(), "VfsIntTest_Overlay_" + Guid.NewGuid().ToString("N"));

		Directory.CreateDirectory(_testBaseDir);
		Directory.CreateDirectory(_testOverlayDir);

		// Create test files in base directory
		File.WriteAllText(Path.Combine(_testBaseDir, "config.ini"), "[Settings]\nVolume=100\n");
		File.WriteAllText(Path.Combine(_testBaseDir, "savegame.dat"), "PlayerName=Test\nScore=1000\n");

		// Initialize test environment and VFS
		_testEnv = new TestEnvironment();
		_testEnv.ProcessEnv.InitializeVirtualFileSystem(_testBaseDir, _testOverlayDir);
	}

	public void Dispose()
	{
		_testEnv.Dispose();

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
	public void CreateFileA_ReadExisting_ShouldReadFromBase()
	{
		// First verify the file can be found by VFS directly
		var vfs = _testEnv.ProcessEnv.VirtualFileSystem;
		Assert.NotNull(vfs);
		Assert.True(vfs.FileExists("config.ini"), "VFS should find config.ini");

		// Arrange
		var pathAddr = _testEnv.WriteString("config.ini");

		// Act - Open file for reading (GENERIC_READ, OPEN_EXISTING)
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", pathAddr, 0x80000000u, 0, 0, 3, 0, 0);

		// Assert
		Assert.NotEqual(0xFFFFFFFFu, handle); // Should not be INVALID_HANDLE_VALUE

		// Read file content - allocate buffer in emulated memory
		var bufferAddr = _testEnv.ProcessEnv.SimpleAlloc(100);
		var bytesReadAddr = _testEnv.ProcessEnv.SimpleAlloc(4);

		var readResult = _testEnv.CallKernel32Api("READFILE", handle, bufferAddr, 100, bytesReadAddr, 0);
		Assert.Equal(1u, readResult); // TRUE

		// Verify we read some bytes
		var bytesRead = _testEnv.Memory.Read32(bytesReadAddr);
		Assert.True(bytesRead > 0);

		// Close handle
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);
	}

	[Fact]
	public void CreateFileA_WriteExisting_ShouldCopyToOverlay()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString("savegame.dat");

		// Act - Open file for writing (GENERIC_READ | GENERIC_WRITE, OPEN_EXISTING)
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", pathAddr, 0xC0000000u, 0, 0, 3, 0, 0);

		// Assert
		Assert.NotEqual(0xFFFFFFFFu, handle);

		// Write new content
		var newContent = "PlayerName=NewPlayer\nScore=2000\n";
		var contentAddr = _testEnv.WriteString(newContent);
		var bytesWrittenAddr = 0x100100u;

		var writeResult = _testEnv.CallKernel32Api("WRITEFILE", handle, contentAddr, (uint)newContent.Length, 
			bytesWrittenAddr, 0);
		Assert.Equal(1u, writeResult); // TRUE

		// Close handle
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);

		// Verify base file is unchanged
		var baseContent = File.ReadAllText(Path.Combine(_testBaseDir, "savegame.dat"));
		Assert.Equal("PlayerName=Test\nScore=1000\n", baseContent);

		// Verify overlay has modified file
		var overlayPath = Path.Combine(_testOverlayDir, "savegame.dat");
		Assert.True(File.Exists(overlayPath));
	}

	[Fact]
	public void CreateFileA_CreateNew_ShouldCreateInOverlay()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString("newfile.txt");

		// Act - Create new file (GENERIC_READ | GENERIC_WRITE, CREATE_ALWAYS)
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", pathAddr, 0xC0000000u, 0, 0, 2, 0, 0);

		// Assert
		Assert.NotEqual(0xFFFFFFFFu, handle);

		// Write content
		var content = "New file content";
		var contentAddr = _testEnv.WriteString(content);
		var bytesWrittenAddr = 0x100200u;

		_testEnv.CallKernel32Api("WRITEFILE", handle, contentAddr, (uint)content.Length, bytesWrittenAddr, 0);
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);

		// Verify file exists only in overlay
		Assert.False(File.Exists(Path.Combine(_testBaseDir, "newfile.txt")));
		Assert.True(File.Exists(Path.Combine(_testOverlayDir, "newfile.txt")));
	}

	[Fact]
	public void DeleteFileA_ShouldWorkWithVFS()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString("config.ini");

		// Act
		var result = _testEnv.CallKernel32Api("DELETEFILEA", pathAddr);

		// Assert
		Assert.Equal(1u, result); // TRUE

		// Base file should still exist
		Assert.True(File.Exists(Path.Combine(_testBaseDir, "config.ini")));
	}

	[Fact]
	public void MoveFileA_ShouldWorkWithVFS()
	{
		// Arrange - Create a file first
		var sourcePathAddr = _testEnv.WriteString("source.txt");
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", sourcePathAddr, 0xC0000000u, 0, 0, 2, 0, 0);
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);

		var destPathAddr = _testEnv.WriteString("destination.txt");

		// Act
		var result = _testEnv.CallKernel32Api("MOVEFILEA", sourcePathAddr, destPathAddr);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	[Fact]
	public void SetFilePointer_ShouldWorkWithVFS()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString("config.ini");
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", pathAddr, 0x80000000u, 0, 0, 3, 0, 0);

		// Act - Seek to position 5 from beginning
		var position = _testEnv.CallKernel32Api("SETFILEPOINTER", handle, 5, 0, 0); // FILE_BEGIN = 0

		// Assert
		Assert.Equal(5u, position);

		// Clean up
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);
	}

	[Fact]
	public void FlushFileBuffers_ShouldWorkWithVFS()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString("flush.txt");
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", pathAddr, 0xC0000000u, 0, 0, 2, 0, 0);

		// Write some content
		var contentAddr = _testEnv.WriteString("Flush test");
		_testEnv.CallKernel32Api("WRITEFILE", handle, contentAddr, 10, 0x100300, 0);

		// Act
		var result = _testEnv.CallKernel32Api("FLUSHFILEBUFFERS", handle);

		// Assert
		Assert.Equal(1u, result); // TRUE

		// Clean up
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);
	}

	[Fact]
	public void GetFileType_WithVFSHandle_ShouldReturnDiskType()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString("config.ini");
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", pathAddr, 0x80000000u, 0, 0, 3, 0, 0);

		// Act
		var fileType = _testEnv.CallKernel32Api("GETFILETYPE", handle);

		// Assert
		Assert.Equal(0x0001u, fileType); // FILE_TYPE_DISK

		// Clean up
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);
	}

	[Fact]
	public void VFS_IsolatesTwoGames_Independently()
	{
		// This test simulates two different game instances with separate VFS overlays
		
		// Game 1 - Modify config
		var game1OverlayDir = Path.Combine(Path.GetTempPath(), "Game1_Overlay_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(game1OverlayDir);

		var testEnv1 = new TestEnvironment();
		testEnv1.ProcessEnv.InitializeVirtualFileSystem(_testBaseDir, game1OverlayDir);

		var pathAddr1 = testEnv1.WriteString("config.ini");
		var handle1 = testEnv1.CallKernel32Api("CREATEFILEA", pathAddr1, 0xC0000000u, 0, 0, 3, 0, 0);
		var content1 = "Game1Config";
		var contentAddr1 = testEnv1.WriteString(content1);
		testEnv1.CallKernel32Api("WRITEFILE", handle1, contentAddr1, (uint)content1.Length, 0x100400, 0);
		testEnv1.CallKernel32Api("CLOSEHANDLE", handle1);

		// Game 2 - Modify same config differently
		var game2OverlayDir = Path.Combine(Path.GetTempPath(), "Game2_Overlay_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(game2OverlayDir);

		var testEnv2 = new TestEnvironment();
		testEnv2.ProcessEnv.InitializeVirtualFileSystem(_testBaseDir, game2OverlayDir);

		var pathAddr2 = testEnv2.WriteString("config.ini");
		var handle2 = testEnv2.CallKernel32Api("CREATEFILEA", pathAddr2, 0xC0000000u, 0, 0, 3, 0, 0);
		var content2 = "Game2Config";
		var contentAddr2 = testEnv2.WriteString(content2);
		testEnv2.CallKernel32Api("WRITEFILE", handle2, contentAddr2, (uint)content2.Length, 0x100400, 0);
		testEnv2.CallKernel32Api("CLOSEHANDLE", handle2);

		// Assert - Each game has its own modified version
		var game1ConfigPath = Path.Combine(game1OverlayDir, "config.ini");
		var game2ConfigPath = Path.Combine(game2OverlayDir, "config.ini");

		Assert.True(File.Exists(game1ConfigPath));
		Assert.True(File.Exists(game2ConfigPath));
		Assert.NotEqual(File.ReadAllText(game1ConfigPath), File.ReadAllText(game2ConfigPath));

		// Original base file should be unchanged
		var baseContent = File.ReadAllText(Path.Combine(_testBaseDir, "config.ini"));
		Assert.Equal("[Settings]\nVolume=100\n", baseContent);

		// Clean up
		testEnv1.Dispose();
		testEnv2.Dispose();
		Directory.Delete(game1OverlayDir, true);
		Directory.Delete(game2OverlayDir, true);
	}
}
