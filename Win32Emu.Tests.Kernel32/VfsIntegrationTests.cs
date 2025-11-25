using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.VirtualFileSystem;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Integration tests for VFS with Kernel32 File I/O APIs using virtual disks
/// </summary>
[Trait("Category", "DllModuleTests")]
public class VfsIntegrationTests : IDisposable
{
	private readonly string _testVhdPath;
	private readonly TestEnvironment _testEnv;
	private readonly DiskVirtualFileSystem _diskVfs;

	public VfsIntegrationTests()
	{
		// Create temporary VHD for testing
		_testVhdPath = Path.Combine(Path.GetTempPath(), "VfsIntTest_" + Guid.NewGuid().ToString("N") + ".vhd");
		
		// Create VHD with test files
		const long vhdSize = 50 * 1024 * 1024; // 50 MB
		using (var vfs = DiskVirtualFileSystem.Create(_testVhdPath, DiskFormat.Vhd, vhdSize))
		{
			// Create directory structure
			vfs.CreateDirectory(@"\testdir");
			
			// Create test files
			var configHandle = vfs.OpenFile(@"\config.ini", VfsFileMode.Create, VfsFileAccess.Write);
			if (configHandle != null)
			{
				using (configHandle)
				{
					var configContent = System.Text.Encoding.ASCII.GetBytes("[Settings]\nVolume=100\n");
					configHandle.Write(configContent, 0, configContent.Length);
				}
			}
			
			var saveHandle = vfs.OpenFile(@"\savegame.dat", VfsFileMode.Create, VfsFileAccess.Write);
			if (saveHandle != null)
			{
				using (saveHandle)
				{
					var saveContent = System.Text.Encoding.ASCII.GetBytes("PlayerName=Test\nScore=1000\n");
					saveHandle.Write(saveContent, 0, saveContent.Length);
				}
			}
		}
		
		// Open VHD and initialize test environment
		_diskVfs = new DiskVirtualFileSystem(_testVhdPath);
		_testEnv = new TestEnvironment();
		_testEnv.ProcessEnv.InitializeVirtualFileSystem(_diskVfs);
	}

	public void Dispose()
	{
		_testEnv.Dispose();
		_diskVfs.Dispose();

		// Clean up VHD file
		try
		{
			if (File.Exists(_testVhdPath))
				File.Delete(_testVhdPath);
		}
		catch
		{
			// Ignore cleanup errors
		}
	}

	[Fact]
	public void CreateFileA_ReadExisting_ShouldReadFromVHD()
	{
		// First verify the file can be found by VFS directly
		var vfs = _testEnv.ProcessEnv.VirtualFileSystem;
		Assert.NotNull(vfs);
		Assert.True(vfs.FileExists(@"\config.ini"), "VFS should find config.ini");

		// Arrange
		var pathAddr = _testEnv.WriteString(@"C:\config.ini");

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
	public void CreateFileA_WriteExisting_ShouldUpdateInVHD()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString(@"C:\savegame.dat");

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
	}

	[Fact]
	public void CreateFileA_CreateNew_ShouldCreateInVHD()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString(@"C:\newfile.txt");

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

		// Verify file exists in VHD
		Assert.True(_diskVfs.FileExists(@"\newfile.txt"));
	}

	[Fact]
	public void DeleteFileA_ShouldWorkWithVFS()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString(@"C:\config.ini");

		// Act
		var result = _testEnv.CallKernel32Api("DELETEFILEA", pathAddr);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	[Fact]
	public void MoveFileA_ShouldWorkWithVFS()
	{
		// Arrange - Create a file first
		var sourcePathAddr = _testEnv.WriteString(@"C:\source.txt");
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", sourcePathAddr, 0xC0000000u, 0, 0, 2, 0, 0);
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);

		var destPathAddr = _testEnv.WriteString(@"C:\destination.txt");

		// Act
		var result = _testEnv.CallKernel32Api("MOVEFILEA", sourcePathAddr, destPathAddr);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	[Fact]
	public void SetFilePointer_ShouldWorkWithVFS()
	{
		// Arrange
		var pathAddr = _testEnv.WriteString(@"C:\config.ini");
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
		var pathAddr = _testEnv.WriteString(@"C:\flush.txt");
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
		var pathAddr = _testEnv.WriteString(@"C:\config.ini");
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", pathAddr, 0x80000000u, 0, 0, 3, 0, 0);

		// Act
		var fileType = _testEnv.CallKernel32Api("GETFILETYPE", handle);

		// Assert
		Assert.Equal(0x0001u, fileType); // FILE_TYPE_DISK

		// Clean up
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);
	}

	[Fact]
	public void VFS_IsolatesTwoGames_UsingDifferentVHDs()
	{
		// This test simulates two different game instances with separate VHDs
		
		// Game 1 - Create VHD and modify config
		var game1VhdPath = Path.Combine(Path.GetTempPath(), "Game1_" + Guid.NewGuid().ToString("N") + ".vhd");
		const long vhdSize = 50 * 1024 * 1024;
		
		using (var game1Vfs = DiskVirtualFileSystem.Create(game1VhdPath, DiskFormat.Vhd, vhdSize))
		{
			var configHandle = game1Vfs.OpenFile(@"\config.ini", VfsFileMode.Create, VfsFileAccess.Write);
			if (configHandle != null)
			{
				using (configHandle)
				{
					var content = System.Text.Encoding.ASCII.GetBytes("Game1Config");
					configHandle.Write(content, 0, content.Length);
				}
			}
		}

		// Game 2 - Create separate VHD and modify config differently
		var game2VhdPath = Path.Combine(Path.GetTempPath(), "Game2_" + Guid.NewGuid().ToString("N") + ".vhd");
		
		using (var game2Vfs = DiskVirtualFileSystem.Create(game2VhdPath, DiskFormat.Vhd, vhdSize))
		{
			var configHandle = game2Vfs.OpenFile(@"\config.ini", VfsFileMode.Create, VfsFileAccess.Write);
			if (configHandle != null)
			{
				using (configHandle)
				{
					var content = System.Text.Encoding.ASCII.GetBytes("Game2Config");
					configHandle.Write(content, 0, content.Length);
				}
			}
		}

		// Assert - Each game has its own config in separate VHDs
		using (var game1Vfs = new DiskVirtualFileSystem(game1VhdPath))
		{
			Assert.True(game1Vfs.FileExists(@"\config.ini"));
			var handle = game1Vfs.OpenFile(@"\config.ini", VfsFileMode.Open, VfsFileAccess.Read);
			Assert.NotNull(handle);
			handle?.Dispose();
		}
		
		using (var game2Vfs = new DiskVirtualFileSystem(game2VhdPath))
		{
			Assert.True(game2Vfs.FileExists(@"\config.ini"));
			var handle = game2Vfs.OpenFile(@"\config.ini", VfsFileMode.Open, VfsFileAccess.Read);
			Assert.NotNull(handle);
			handle?.Dispose();
		}

		// Clean up
		File.Delete(game1VhdPath);
		File.Delete(game2VhdPath);
	}

	[Fact]
	public void CreateFileA_RelativePath_ShouldResolveRelativeToCurrentDirectory()
	{
		// Arrange - Create a subdirectory with a file in the VHD
		_diskVfs.CreateDirectory(@"\data");
		var dataHandle = _diskVfs.OpenFile(@"\data\test.txt", VfsFileMode.Create, VfsFileAccess.Write);
		if (dataHandle != null)
		{
			using (dataHandle)
			{
				var content = System.Text.Encoding.ASCII.GetBytes("Test data content");
				dataHandle.Write(content, 0, content.Length);
			}
		}

		// Set current directory to C:\
		_testEnv.ProcessEnv.CurrentDirectory = @"C:\";

		// Act - Open file using relative path
		var relativePathAddr = _testEnv.WriteString(@"data\test.txt");
		var handle = _testEnv.CallKernel32Api("CREATEFILEA", relativePathAddr, 0x80000000u, 0, 0, 3, 0, 0);

		// Assert
		Assert.NotEqual(0xFFFFFFFFu, handle); // Should not be INVALID_HANDLE_VALUE

		// Read file content to verify it's the correct file
		var bufferAddr = _testEnv.ProcessEnv.SimpleAlloc(100);
		var bytesReadAddr = _testEnv.ProcessEnv.SimpleAlloc(4);
		var readResult = _testEnv.CallKernel32Api("READFILE", handle, bufferAddr, 100, bytesReadAddr, 0);
		Assert.Equal(1u, readResult); // TRUE

		// Verify content
		var bytesRead = _testEnv.Memory.Read32(bytesReadAddr);
		Assert.True(bytesRead > 0);

		// Close handle
		_testEnv.CallKernel32Api("CLOSEHANDLE", handle);
	}
}
