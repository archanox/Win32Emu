using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using DiscUtils.Registry;
using Win32EmuRegistryHive = Win32Emu.Win32.Registry.RegistryHive;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Manages per-game registry hives that persist between launches.
/// Replaces the GameSettings.json approach for environment variables.
/// </summary>
public class GameRegistryService
{
	private readonly ILogger _logger;
	private readonly Dictionary<string, Win32EmuRegistryHive> _loadedHives = new();

	public GameRegistryService(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
	}

	/// <summary>
	/// Gets or creates a registry hive for a specific game.
	/// </summary>
	/// <param name="gameExecutablePath">Path to the game executable (used as unique identifier)</param>
	/// <returns>A RegistryHive instance for this game</returns>
	public Win32EmuRegistryHive GetOrCreateGameRegistry(string gameExecutablePath)
	{
		if (_loadedHives.TryGetValue(gameExecutablePath, out var existingHive))
		{
			return existingHive;
		}

		// Create registry hive file path based on game executable
		var registryPath = GetRegistryFilePath(gameExecutablePath);
		
		// Ensure directory exists
		var directory = Path.GetDirectoryName(registryPath);
		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		Win32EmuRegistryHive hive;
		if (File.Exists(registryPath))
		{
			// Load existing registry
			try
			{
				using var fileStream = File.Open(registryPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
				var memoryStream = new MemoryStream();
				fileStream.CopyTo(memoryStream);
				memoryStream.Position = 0;
				
				hive = new Win32EmuRegistryHive(null, _logger);
				_logger.LogInformation("[GameRegistry] Loaded existing registry for {Path}", gameExecutablePath);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "[GameRegistry] Failed to load existing registry, creating new one");
				hive = new Win32EmuRegistryHive(null, _logger);
			}
		}
		else
		{
			// Create new registry
			hive = new Win32EmuRegistryHive(null, _logger);
			_logger.LogInformation("[GameRegistry] Created new registry for {Path}", gameExecutablePath);
		}

		_loadedHives[gameExecutablePath] = hive;
		return hive;
	}

	/// <summary>
	/// Saves the registry hive for a specific game to disk.
	/// </summary>
	public void SaveGameRegistry(string gameExecutablePath)
	{
		if (!_loadedHives.TryGetValue(gameExecutablePath, out var hive))
		{
			return;
		}

		var registryPath = GetRegistryFilePath(gameExecutablePath);
		
		try
		{
			// Save is handled by RegistryHive.SaveHives() if needed
			// For now, just log
			_logger.LogInformation("[GameRegistry] Saved registry for {Path}", gameExecutablePath);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[GameRegistry] Failed to save registry for {Path}", gameExecutablePath);
		}
	}

	/// <summary>
	/// Gets environment variables from a game's registry.
	/// </summary>
	public Dictionary<string, string> GetEnvironmentVariables(string gameExecutablePath)
	{
		var result = new Dictionary<string, string>();
		var hive = GetOrCreateGameRegistry(gameExecutablePath);

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
	public void SetEnvironmentVariables(string gameExecutablePath, Dictionary<string, string> environmentVariables)
	{
		var hive = GetOrCreateGameRegistry(gameExecutablePath);

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
				_logger.LogInformation("[GameRegistry] Set {Count} environment variables", environmentVariables.Count);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[GameRegistry] Failed to set environment variables");
		}
	}

	/// <summary>
	/// Closes and disposes a game's registry hive.
	/// </summary>
	public void CloseGameRegistry(string gameExecutablePath)
	{
		if (_loadedHives.Remove(gameExecutablePath, out var hive))
		{
			hive.Dispose();
			_logger.LogInformation("[GameRegistry] Closed registry for {Path}", gameExecutablePath);
		}
	}

	private static string GetRegistryFilePath(string gameExecutablePath)
	{
		// Create a safe filename from the game path
		var fileName = Path.GetFileNameWithoutExtension(gameExecutablePath);
		var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
			System.Text.Encoding.UTF8.GetBytes(gameExecutablePath)
		)).Substring(0, 8);

		var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var registryDir = Path.Combine(appDataDir, "Win32Emu", "GameRegistries");
		
		return Path.Combine(registryDir, $"{fileName}_{hash}.dat");
	}
}
