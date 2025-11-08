using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.Services;
using Xunit;

namespace Win32Emu.Tests.Gui;

public class VirtualDiskServiceTests : IDisposable
{
	private readonly string _testDirectory;
	private readonly EmulatorConfiguration _configuration;
	private readonly VirtualDiskService _service;

	public VirtualDiskServiceTests()
	{
		// Create temporary directory for test VHDs
		_testDirectory = Path.Combine(Path.GetTempPath(), $"Win32EmuTests_{Guid.NewGuid()}");
		Directory.CreateDirectory(_testDirectory);

		_configuration = new EmulatorConfiguration
		{
			VirtualDisksDirectory = _testDirectory,
			DefaultVirtualDiskSizeMb = 50, // Small size for testing
			VirtualDiskFormat = "VHD"
		};

		_service = new VirtualDiskService(_configuration, NullLogger.Instance);
	}

	public void Dispose()
	{
		// Clean up test directory
		if (Directory.Exists(_testDirectory))
		{
			try
			{
				Directory.Delete(_testDirectory, recursive: true);
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}

	[Fact]
	public void GetOrCreateVirtualDisk_CreatesNewDisk_WhenDiskDoesNotExist()
	{
		// Arrange
		var game = new Game
		{
			Title = "TestGame",
			ExecutablePath = "/tmp/test.exe"
		};

		// Act
		var diskPath = _service.GetOrCreateVirtualDisk(game);

		// Assert
		Assert.NotNull(diskPath);
		Assert.True(File.Exists(diskPath));
		Assert.EndsWith(".vhd", diskPath);
	}

	[Fact]
	public void GetOrCreateVirtualDisk_ReusesExistingDisk_WhenDiskExists()
	{
		// Arrange
		var game = new Game
		{
			Title = "TestGame",
			ExecutablePath = "/tmp/test.exe"
		};

		// Create the disk first time
		var diskPath1 = _service.GetOrCreateVirtualDisk(game);
		var firstCreationTime = File.GetCreationTimeUtc(diskPath1);

		// Wait a bit to ensure creation time would differ
		Thread.Sleep(100);

		// Act - try to create again
		var diskPath2 = _service.GetOrCreateVirtualDisk(game);
		var secondCreationTime = File.GetCreationTimeUtc(diskPath2);

		// Assert
		Assert.Equal(diskPath1, diskPath2);
		Assert.Equal(firstCreationTime, secondCreationTime); // Same file, not recreated
	}

	[Fact]
	public void GetVirtualDisksDirectory_ReturnsConfiguredDirectory()
	{
		// Act
		var directory = _service.GetVirtualDisksDirectory();

		// Assert
		Assert.Equal(_testDirectory, directory);
	}

	[Fact]
	public void ShouldUseVirtualDisk_ReturnsTrue_WhenEnabledByDefault()
	{
		// Arrange
		var game = new Game
		{
			Title = "TestGame",
			ExecutablePath = "/tmp/test.exe"
		};

		// Act
		var shouldUse = _service.ShouldUseVirtualDisk(game);

		// Assert - should be true by default according to EmulatorConfiguration
		Assert.True(shouldUse);
	}

	[Fact]
	public void DeleteVirtualDisk_RemovesDiskFile_WhenExists()
	{
		// Arrange
		var game = new Game
		{
			Title = "TestGameToDelete",
			ExecutablePath = "/tmp/test.exe"
		};

		// Create the disk
		var diskPath = _service.GetOrCreateVirtualDisk(game);
		Assert.True(File.Exists(diskPath));

		// Act
		_service.DeleteVirtualDisk(game);

		// Assert
		Assert.False(File.Exists(diskPath));
	}
}
