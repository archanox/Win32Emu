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
			
			_logger.LogInformation("[VirtualDisk] Specified disk path does not exist, will create: {Path}", explicitPath);
			// Create the disk at the explicit path
			CreateVirtualDisk(explicitPath, gameSettings);
			return explicitPath;
		}

		// Auto-create disk path
		var diskDir = GetVirtualDisksDirectory();
		if (string.IsNullOrWhiteSpace(diskDir))
		{
			throw new InvalidOperationException("Virtual disks directory is null or empty");
		}
		Directory.CreateDirectory(diskDir);

		// Use game title (sanitized) as the disk filename
		var sanitizedTitle = SanitizeFileName(game.Title);
		if (string.IsNullOrWhiteSpace(sanitizedTitle))
		{
			throw new InvalidOperationException($"Cannot create virtual disk: game title '{game.Title}' resulted in empty filename");
		}
		var format = _configuration.VirtualDiskFormat?.ToLowerInvariant() ?? "vhd";
		var diskPath = Path.Combine(diskDir, $"{sanitizedTitle}.{format}");

		// If disk doesn't exist, create it
		if (!File.Exists(diskPath))
		{
			_logger.LogInformation("[VirtualDisk] Creating new virtual disk: {Path}", diskPath);
			CreateVirtualDisk(diskPath, gameSettings);
		}
		else
		{
			_logger.LogInformation("[VirtualDisk] Using existing disk: {Path}", diskPath);
		}

		return diskPath;
	}

	/// <summary>
	/// Creates a new virtual disk file
	/// </summary>
	private void CreateVirtualDisk(string diskPath, GameSettings? gameSettings)
	{
		var format = GetDiskFormatFromPath(diskPath);
		var sizeBytes = (gameSettings?.VirtualDiskSizeMb ?? _configuration.DefaultVirtualDiskSizeMb) * 1024L * 1024L;

		_logger.LogInformation("[VirtualDisk] Creating {Format} disk at {Path} with size {SizeMb} MB", 
			format, diskPath, sizeBytes / 1024 / 1024);

		// Use DiskVirtualFileSystem.Create to create and format the disk
		using (DiskVirtualFileSystem.Create(diskPath, format, sizeBytes, _logger))
		{
			// Disk created and formatted successfully
		}
	}

	/// <summary>
	/// Determines the disk format from the file extension
	/// </summary>
	private DiskFormat GetDiskFormatFromPath(string diskPath)
	{
		var extension = Path.GetExtension(diskPath).ToLowerInvariant();
		return extension switch
		{
			".vhd" => DiskFormat.Vhd,
			".vhdx" => DiskFormat.Vhdx,
			_ => DiskFormat.Vhd // Default to VHD
		};
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

		// Default to a "VirtualDisks" folder in LocalApplicationData
		var appDataDir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Win32Emu",
			"VirtualDisks");

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
		return await Task.Run(() =>
		{
			var diskPath = GetOrCreateVirtualDisk(game, gameSettings);

			// If we have a source directory, populate the disk
			if (!string.IsNullOrEmpty(gameSettings?.VirtualDiskSourceDirectory))
			{
				var sourceDir = gameSettings.VirtualDiskSourceDirectory;
				if (Directory.Exists(sourceDir))
				{
					_logger.LogInformation("[VirtualDisk] Will populate disk with source directory: {SourceDir}", sourceDir);
					// The actual copying will be done by EmulatorService after mounting
				}
				else
				{
					_logger.LogWarning("[VirtualDisk] Source directory does not exist: {Path}", sourceDir);
				}
			}

			return diskPath;
		});
	}

	/// <summary>
	/// Install a game directory into a virtual disk and return the VHD path to the executable
	/// </summary>
	/// <param name="game">The game being installed</param>
	/// <param name="sourceExecutablePath">Path to the executable on the host filesystem</param>
	/// <param name="gameSettings">Optional per-game settings</param>
	/// <returns>Tuple of (VHD file path, path to executable within VHD)</returns>
	public async Task<(string DiskPath, string VhdExecutablePath)> InstallGameToVirtualDiskAsync(
		Game game, 
		string sourceExecutablePath,
		GameSettings? gameSettings = null)
	{
		return await Task.Run(() =>
		{
			if (!File.Exists(sourceExecutablePath))
			{
				throw new FileNotFoundException($"Source executable not found: {sourceExecutablePath}");
			}

			// Get the source directory (the directory containing the executable)
			var sourceDir = Path.GetDirectoryName(sourceExecutablePath);
			if (string.IsNullOrEmpty(sourceDir))
			{
				throw new InvalidOperationException($"Could not determine directory for executable: {sourceExecutablePath}");
			}

			// Get the folder name to use as the installation directory in VHD
			var folderName = Path.GetFileName(sourceDir);
			if (string.IsNullOrWhiteSpace(folderName))
			{
				folderName = "game";
			}

			// Get the executable filename
			var executableName = Path.GetFileName(sourceExecutablePath);

			// Create or get the virtual disk for this game
			var diskPath = GetOrCreateVirtualDisk(game, gameSettings);

			_logger.LogInformation("[VirtualDisk] Installing game from {SourceDir} to virtual disk {DiskPath}", 
				sourceDir, diskPath);

			// Open the disk and copy the game directory
			using (var diskVfs = new DiskVirtualFileSystem(diskPath, _logger))
			{
				if (diskVfs.IsReadOnly)
				{
					throw new InvalidOperationException("Cannot install game to read-only virtual disk");
				}

				// Copy the entire source directory to the root of the VHD with the folder name
				var targetPath = $"/{folderName}";
				_logger.LogInformation("[VirtualDisk] Copying directory to VHD path: {TargetPath}", targetPath);
				
				diskVfs.CopyDirectoryIn(sourceDir, targetPath);
				
				_logger.LogInformation("[VirtualDisk] Successfully installed game to virtual disk");
			}

			// Return the disk path and the VHD executable path
			var vhdExecutablePath = $"C:\\{folderName}\\{executableName}";
			return (diskPath, vhdExecutablePath);
		});
	}

	/// <summary>
	/// Delete the virtual disk for a game
	/// </summary>
	public void DeleteVirtualDisk(Game game)
	{
		var diskDir = GetVirtualDisksDirectory();
		if (string.IsNullOrWhiteSpace(diskDir))
		{
			throw new InvalidOperationException("Virtual disks directory is null or empty");
		}
		
		var sanitizedTitle = SanitizeFileName(game.Title);
		if (string.IsNullOrWhiteSpace(sanitizedTitle))
		{
			_logger.LogWarning("[VirtualDisk] Cannot delete disk for game with empty title: {Title}", game.Title);
			return;
		}
		
		// Try all supported formats
		var formats = new[] { "vhd", "vhdx" };
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
