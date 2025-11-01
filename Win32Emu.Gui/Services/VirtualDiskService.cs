using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.VirtualFileSystem;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Service for managing virtual disk files for games
/// </summary>
public class VirtualDiskService
{
	private readonly EmulatorConfiguration _configuration;
	private readonly ILogger _logger;

	public VirtualDiskService(EmulatorConfiguration configuration, ILogger? logger = null)
	{
		_configuration = configuration;
		_logger = logger ?? NullLogger.Instance;
	}

	/// <summary>
	/// Get or create a virtual disk for the specified game
	/// </summary>
	/// <param name="game">The game to get/create a disk for</param>
	/// <param name="gameSettings">Per-game settings (optional)</param>
	/// <returns>Path to the virtual disk file</returns>
	public string GetOrCreateVirtualDisk(Game game, GameSettings? gameSettings = null)
	{
		// Check if virtual disk is explicitly disabled for this game
		if (gameSettings?.UseVirtualDisk == false)
		{
			throw new InvalidOperationException("Virtual disk is disabled for this game");
		}

		// Use explicit path if provided
		if (!string.IsNullOrEmpty(gameSettings?.VirtualDiskPath))
		{
			var explicitPath = gameSettings.VirtualDiskPath;
			if (File.Exists(explicitPath))
			{
				_logger.LogInformation("[VirtualDisk] Using existing disk: {Path}", explicitPath);
				return explicitPath;
			}
			
			_logger.LogWarning("[VirtualDisk] Specified disk path does not exist: {Path}. Will auto-create.", explicitPath);
		}

		// Auto-create disk path
		var diskDir = GetVirtualDisksDirectory();
		Directory.CreateDirectory(diskDir);

		// Use game title (sanitized) as the disk filename
		var sanitizedTitle = SanitizeFileName(game.Title);
		var format = _configuration.VirtualDiskFormat?.ToLowerInvariant() ?? "vhd";
		var diskPath = Path.Combine(diskDir, $"{sanitizedTitle}.{format}");

		// If disk doesn't exist, we'll need to create it
		if (!File.Exists(diskPath))
		{
			_logger.LogInformation("[VirtualDisk] Virtual disk does not exist, will be created on first use: {Path}", diskPath);
			
			// Note: Actual creation happens when mounting, as we use external tools
			// The disk will be created by mounting logic
		}
		else
		{
			_logger.LogInformation("[VirtualDisk] Using existing disk: {Path}", diskPath);
		}

		return diskPath;
	}

	/// <summary>
	/// Get the directory where virtual disks are stored
	/// </summary>
	public string GetVirtualDisksDirectory()
	{
		if (!string.IsNullOrEmpty(_configuration.VirtualDisksDirectory))
		{
			return _configuration.VirtualDisksDirectory;
		}

		// Default to a "VirtualDisks" folder next to the emulator configuration
		var appDataDir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Win32Emu",
			"VirtualDisks");

		_configuration.VirtualDisksDirectory = appDataDir;
		return appDataDir;
	}

	/// <summary>
	/// Check if a virtual disk should be used for the given game
	/// </summary>
	public bool ShouldUseVirtualDisk(Game game, GameSettings? gameSettings = null)
	{
		// Check per-game setting first
		if (gameSettings?.UseVirtualDisk.HasValue == true)
		{
			return gameSettings.UseVirtualDisk.Value;
		}

		// Fall back to global default
		return _configuration.UseVirtualDiskByDefault;
	}

	/// <summary>
	/// Prepare a virtual disk for a game (create and populate if needed)
	/// </summary>
	public async Task<string> PrepareVirtualDiskAsync(Game game, GameSettings? gameSettings = null)
	{
		var diskPath = GetOrCreateVirtualDisk(game, gameSettings);

		// If disk doesn't exist and we have a source directory, we need to create and populate it
		if (!File.Exists(diskPath) && !string.IsNullOrEmpty(gameSettings?.VirtualDiskSourceDirectory))
		{
			await Task.Run(() =>
			{
				var sourceDir = gameSettings.VirtualDiskSourceDirectory;
				if (!Directory.Exists(sourceDir))
				{
					_logger.LogWarning("[VirtualDisk] Source directory does not exist: {Path}", sourceDir);
					return;
				}

				// Note: Disk creation with DiscUtils.Create is not implemented
				// User must create the disk manually using external tools
				_logger.LogWarning("[VirtualDisk] Disk creation is not automated. Please create the disk manually using tools like qemu-img or VBoxManage");
				_logger.LogInformation("[VirtualDisk] Example: qemu-img create -f vhd {DiskPath} {SizeMb}M", 
					diskPath, gameSettings.VirtualDiskSizeMb ?? _configuration.DefaultVirtualDiskSizeMb);
			});
		}

		return diskPath;
	}

	/// <summary>
	/// Delete the virtual disk for a game
	/// </summary>
	public void DeleteVirtualDisk(Game game)
	{
		var diskDir = GetVirtualDisksDirectory();
		var sanitizedTitle = SanitizeFileName(game.Title);
		
		// Try all supported formats
		var formats = new[] { "vhd", "vhdx", "vmdk" };
		foreach (var format in formats)
		{
			var diskPath = Path.Combine(diskDir, $"{sanitizedTitle}.{format}");
			if (File.Exists(diskPath))
			{
				File.Delete(diskPath);
				_logger.LogInformation("[VirtualDisk] Deleted virtual disk: {Path}", diskPath);
				return;
			}
		}

		_logger.LogWarning("[VirtualDisk] No virtual disk found for game: {Title}", game.Title);
	}

	private static string SanitizeFileName(string fileName)
	{
		var invalid = Path.GetInvalidFileNameChars();
		var sanitized = string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
		return string.IsNullOrWhiteSpace(sanitized) ? "game" : sanitized;
	}
}
