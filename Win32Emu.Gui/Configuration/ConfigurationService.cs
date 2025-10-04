using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Service for managing application configuration with persistence
/// </summary>
public class ConfigurationService
{
    private readonly string _settingsFilePath;
    private readonly string _libraryFilePath;
    private readonly string _legacyConfigFilePath;
    private EmulatorSettings _settings;
    private GameLibrary _library;

    public ConfigurationService()
    {
        // Get the application data directory for cross-platform storage
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var win32EmuDir = Path.Combine(appDataDir, "Win32Emu");

        // Ensure the directory exists
        if (!Directory.Exists(win32EmuDir))
        {
            Directory.CreateDirectory(win32EmuDir);
        }

        _settingsFilePath = Path.Combine(win32EmuDir, "settings.json");
        _libraryFilePath = Path.Combine(win32EmuDir, "library.json");
        _legacyConfigFilePath = Path.Combine(win32EmuDir, "config.json");

        // Load configuration
        _settings = LoadSettings();
        _library = LoadLibrary();

        // Migrate from legacy config if new files don't exist
        MigrateLegacyConfigIfNeeded();
    }

    private EmulatorSettings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new EmulatorSettings();
        }

        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            return JsonSerializer.Deserialize<EmulatorSettings>(json) ?? new EmulatorSettings();
        }
        catch
        {
            return new EmulatorSettings();
        }
    }

    private GameLibrary LoadLibrary()
    {
        if (!File.Exists(_libraryFilePath))
        {
            return new GameLibrary();
        }

        try
        {
            var json = File.ReadAllText(_libraryFilePath);
            return JsonSerializer.Deserialize<GameLibrary>(json) ?? new GameLibrary();
        }
        catch
        {
            return new GameLibrary();
        }
    }

    private void SaveSettings()
    {
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsFilePath, json);
    }

    private void SaveLibrary()
    {
        var json = JsonSerializer.Serialize(_library, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_libraryFilePath, json);
    }

    /// <summary>
    /// Get the emulator configuration
    /// </summary>
    public EmulatorConfiguration GetEmulatorConfiguration()
    {
        return new EmulatorConfiguration
        {
            RenderingBackend = _settings.RenderingBackend,
            ResolutionScaleFactor = _settings.ResolutionScaleFactor,
            ReservedMemoryMb = _settings.ReservedMemoryMB,
            WindowsVersion = _settings.WindowsVersion,
            EnableDebugMode = _settings.EnableDebugMode
        };
    }

    /// <summary>
    /// Get the emulator configuration for a specific game (with per-game overrides applied)
    /// </summary>
    public EmulatorConfiguration GetEmulatorConfiguration(string gameExecutablePath)
    {
        var config = GetEmulatorConfiguration();

        if (_settings.PerGameSettings.TryGetValue(gameExecutablePath, out var gameSettings))
        {
            // Apply per-game overrides
            if (gameSettings.RenderingBackend != null)
                config.RenderingBackend = gameSettings.RenderingBackend;
            if (gameSettings.ResolutionScaleFactor != null)
                config.ResolutionScaleFactor = gameSettings.ResolutionScaleFactor.Value;
            if (gameSettings.ReservedMemoryMb != null)
                config.ReservedMemoryMb = gameSettings.ReservedMemoryMb.Value;
            if (gameSettings.WindowsVersion != null)
                config.WindowsVersion = gameSettings.WindowsVersion;
            if (gameSettings.EnableDebugMode != null)
                config.EnableDebugMode = gameSettings.EnableDebugMode.Value;
        }

        return config;
    }

    /// <summary>
    /// Save the emulator configuration
    /// </summary>
    public void SaveEmulatorConfiguration(EmulatorConfiguration configuration)
    {
        _settings.RenderingBackend = configuration.RenderingBackend;
        _settings.ResolutionScaleFactor = configuration.ResolutionScaleFactor;
        _settings.ReservedMemoryMB = configuration.ReservedMemoryMb;
        _settings.WindowsVersion = configuration.WindowsVersion;
        _settings.EnableDebugMode = configuration.EnableDebugMode;
        SaveSettings();
    }

    /// <summary>
    /// Save per-game emulator settings
    /// </summary>
    public void SaveGameSettings(string gameExecutablePath, GameSettings gameSettings)
    {
        _settings.PerGameSettings[gameExecutablePath] = gameSettings;
        SaveSettings();
    }

    /// <summary>
    /// Get per-game settings for a specific game
    /// </summary>
    public GameSettings? GetGameSettings(string gameExecutablePath)
    {
        return _settings.PerGameSettings.TryGetValue(gameExecutablePath, out var settings) ? settings : null;
    }

    /// <summary>
    /// Remove per-game settings for a specific game
    /// </summary>
    public void RemoveGameSettings(string gameExecutablePath)
    {
        if (_settings.PerGameSettings.Remove(gameExecutablePath))
        {
            SaveSettings();
        }
    }

    /// <summary>
    /// Get the list of games from configuration
    /// </summary>
    public Game[] GetGames()
    {
        return _library.Games.ToArray();
    }

    /// <summary>
    /// Save the list of games to configuration
    /// </summary>
    public void SaveGames(IEnumerable<Game> games)
    {
        _library.Games = games.ToList();
        SaveLibrary();
    }

    /// <summary>
    /// Get the list of watched folders
    /// </summary>
    public string[] GetWatchedFolders()
    {
        return _library.WatchedFolders.ToArray();
    }

    /// <summary>
    /// Save the list of watched folders
    /// </summary>
    public void SaveWatchedFolders(IEnumerable<string> folders)
    {
        _library.WatchedFolders = folders.ToList();
        SaveLibrary();
    }

    /// <summary>
    /// Migrate from legacy config.json to split files if needed
    /// </summary>
    private void MigrateLegacyConfigIfNeeded()
    {
        // Check if legacy config exists and new files don't
        if (File.Exists(_legacyConfigFilePath) && 
            (!File.Exists(_settingsFilePath) || !File.Exists(_libraryFilePath)))
        {
            try
            {
                var json = File.ReadAllText(_legacyConfigFilePath);
                var legacyConfig = JsonSerializer.Deserialize<AppConfiguration>(json);

                if (legacyConfig != null)
                {
                    // Migrate emulator settings
                    _settings.RenderingBackend = legacyConfig.RenderingBackend;
                    _settings.ResolutionScaleFactor = legacyConfig.ResolutionScaleFactor;
                    _settings.ReservedMemoryMB = legacyConfig.ReservedMemoryMB;
                    _settings.WindowsVersion = legacyConfig.WindowsVersion;
                    _settings.EnableDebugMode = legacyConfig.EnableDebugMode;

                    // Migrate game library
                    _library.Games = legacyConfig.Games;
                    _library.WatchedFolders = legacyConfig.WatchedFolders;

                    // Save the migrated configuration
                    SaveSettings();
                    SaveLibrary();
                }
            }
            catch
            {
                // If migration fails, keep the default values
            }
        }
    }

    /// <summary>
    /// Get the settings file path for display purposes
    /// </summary>
    public string SettingsFilePath => _settingsFilePath;

    /// <summary>
    /// Get the library file path for display purposes
    /// </summary>
    public string LibraryFilePath => _libraryFilePath;

    /// <summary>
    /// Get the configuration file path for display purposes (legacy)
    /// </summary>
    [Obsolete("Use SettingsFilePath or LibraryFilePath instead")]
    public string ConfigFilePath => _legacyConfigFilePath;
}
