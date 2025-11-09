using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using DiscUtils.Registry;
using Win32Emu.VirtualFileSystem;
using Win32EmuRegistryHive = Win32Emu.Win32.Registry.RegistryHive;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Manages per-game registry hives that are stored within the virtual disk.
/// Registry hives live at C:\Windows\System32\config\ inside the virtual disk.
/// </summary>
public class GameRegistryService
{
	private readonly ILogger _logger;
	private readonly Dictionary<string, (DiskVirtualFileSystem vfs, Win32EmuRegistryHive hive)> _loadedHives = new();

	public GameRegistryService(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
	}

	/// <summary>
	/// Gets or creates a registry hive for a specific game by accessing the virtual disk.
	/// </summary>
	/// <param name="virtualDiskPath">Path to the game's virtual disk file (VHD/VMDK/VHDX)</param>
	/// <returns>A RegistryHive instance for this game</returns>
	public Win32EmuRegistryHive GetOrCreateGameRegistry(string virtualDiskPath)
	{
		if (_loadedHives.TryGetValue(virtualDiskPath, out var existing))
		{
			return existing.hive;
		}

		if (!File.Exists(virtualDiskPath))
		{
			throw new FileNotFoundException($"Virtual disk not found: {virtualDiskPath}");
		}

		try
		{
			// Open the virtual disk
			var vfs = new DiskVirtualFileSystem(virtualDiskPath, _logger);
			
			// Create a registry hive that uses this VFS
			// The registry will access files at paths like C:\Windows\System32\config\SYSTEM
			var hive = new Win32EmuRegistryHive(vfs, _logger);
			
			_loadedHives[virtualDiskPath] = (vfs, hive);
			_logger.LogInformation("[GameRegistry] Loaded registry from virtual disk: {Path}", virtualDiskPath);
			
			return hive;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[GameRegistry] Failed to load registry from virtual disk: {Path}", virtualDiskPath);
			throw;
		}
	}

	/// <summary>
	/// Gets environment variables from a game's registry.
	/// </summary>
	public Dictionary<string, string> GetEnvironmentVariables(string virtualDiskPath)
	{
		var result = new Dictionary<string, string>();
		var hive = GetOrCreateGameRegistry(virtualDiskPath);

		try
		{
			// Try user environment variables first
			var userEnvHandle = hive.OpenKey("HKEY_CURRENT_USER\\Environment");
			if (userEnvHandle != 0)
			{
				var valueNames = hive.EnumerateValueNames(userEnvHandle);
				foreach (var valueName in valueNames)
				{
					if (hive.QueryValue(userEnvHandle, valueName, out var value, out _))
					{
						result[valueName] = value?.ToString() ?? string.Empty;
					}
				}
				hive.CloseKey(userEnvHandle);
			}

			// Also get system environment variables
			var systemEnvHandle = hive.OpenKey("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment");
			if (systemEnvHandle != 0)
			{
				var valueNames = hive.EnumerateValueNames(systemEnvHandle);
				foreach (var valueName in valueNames)
				{
					// User variables override system variables
					if (!result.ContainsKey(valueName))
					{
						if (hive.QueryValue(systemEnvHandle, valueName, out var value, out _))
						{
							result[valueName] = value?.ToString() ?? string.Empty;
						}
					}
				}
				hive.CloseKey(systemEnvHandle);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[GameRegistry] Failed to get environment variables");
		}

		return result;
	}

	/// <summary>
	/// Sets environment variables in a game's registry (user hive).
	/// </summary>
	public void SetEnvironmentVariables(string virtualDiskPath, Dictionary<string, string> environmentVariables)
	{
		var hive = GetOrCreateGameRegistry(virtualDiskPath);

		try
		{
			var userEnvHandle = hive.CreateKey("HKEY_CURRENT_USER\\Environment");
			if (userEnvHandle != 0)
			{
				// Clear existing values first
				var existingValues = hive.EnumerateValueNames(userEnvHandle);
				foreach (var valueName in existingValues)
				{
					hive.DeleteValue(userEnvHandle, valueName);
				}

				// Set new values
				foreach (var (key, value) in environmentVariables)
				{
					hive.SetValue(userEnvHandle, key, value, RegistryValueType.String);
				}

				hive.CloseKey(userEnvHandle);
				_logger.LogInformation("[GameRegistry] Set {Count} environment variables in virtual disk", environmentVariables.Count);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[GameRegistry] Failed to set environment variables");
		}
	}

	/// <summary>
	/// Closes and disposes a game's registry hive and VFS.
	/// </summary>
	public void CloseGameRegistry(string virtualDiskPath)
	{
		if (_loadedHives.Remove(virtualDiskPath, out var loaded))
		{
			loaded.hive.Dispose();
			loaded.vfs.Dispose();
			_logger.LogInformation("[GameRegistry] Closed registry for virtual disk: {Path}", virtualDiskPath);
		}
	}

	/// <summary>
	/// Disposes all loaded registry hives and VFS instances.
	/// </summary>
	public void Dispose()
	{
		foreach (var (vfs, hive) in _loadedHives.Values)
		{
			hive.Dispose();
			vfs.Dispose();
		}
		_loadedHives.Clear();
	}
}
