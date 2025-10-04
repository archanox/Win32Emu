using Config.Net;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Service for managing application configuration with persistence
/// </summary>
public class ConfigurationService
{
    private readonly IEmulatorSettings _settings;
    private readonly IGameLibrary _library;
    private readonly IAppConfiguration _legacyConfig;
    private readonly string _settingsFilePath;
    private readonly string _libraryFilePath;
    private readonly string _legacyConfigFilePath;

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

        // Build the new split configuration files
        _settings = new ConfigurationBuilder<IEmulatorSettings>()
            .UseJsonFile(_settingsFilePath)
            .Build();

        _library = new ConfigurationBuilder<IGameLibrary>()
            .UseJsonFile(_libraryFilePath)
            .Build();

        // Keep legacy config for migration
        _legacyConfig = new ConfigurationBuilder<IAppConfiguration>()
            .UseJsonFile(_legacyConfigFilePath)
            .Build();

        // Migrate from legacy config if new files don't exist
        MigrateLegacyConfigIfNeeded();
    }

    /// <summary>
    /// Get the emulator configuration
    /// </summary>
    public EmulatorConfiguration GetEmulatorConfiguration()
    {
        return MapToEmulatorConfiguration(_settings);
    }

    /// <summary>
    /// Get the emulator configuration for a specific game (with per-game overrides applied)
    /// </summary>
    public EmulatorConfiguration GetEmulatorConfiguration(string gameExecutablePath)
    {
        var config = GetEmulatorConfiguration();
        var perGameSettings = GetPerGameSettings();

        if (perGameSettings.TryGetValue(gameExecutablePath, out var gameSettings))
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
        ApplyEmulatorConfiguration(_settings, configuration);
    }

    /// <summary>
    /// Save per-game emulator settings
    /// </summary>
    public void SaveGameSettings(string gameExecutablePath, GameSettings gameSettings)
    {
        var perGameSettings = GetPerGameSettings();
        perGameSettings[gameExecutablePath] = gameSettings;
        _settings.PerGameSettings = System.Text.Json.JsonSerializer.Serialize(perGameSettings);
    }

    /// <summary>
    /// Get per-game settings for a specific game
    /// </summary>
    public GameSettings? GetGameSettings(string gameExecutablePath)
    {
        var perGameSettings = GetPerGameSettings();
        return perGameSettings.TryGetValue(gameExecutablePath, out var settings) ? settings : null;
    }

    /// <summary>
    /// Remove per-game settings for a specific game
    /// </summary>
    public void RemoveGameSettings(string gameExecutablePath)
    {
        var perGameSettings = GetPerGameSettings();
        if (perGameSettings.Remove(gameExecutablePath))
        {
            _settings.PerGameSettings = System.Text.Json.JsonSerializer.Serialize(perGameSettings);
        }
    }

    /// <summary>
    /// Maps IEmulatorSettings to EmulatorConfiguration
    /// </summary>
    private EmulatorConfiguration MapToEmulatorConfiguration(IEmulatorSettings settings)
    {
        return new EmulatorConfiguration
        {
            RenderingBackend = settings.RenderingBackend,
            ResolutionScaleFactor = settings.ResolutionScaleFactor,
            ReservedMemoryMb = settings.ReservedMemoryMb,
            WindowsVersion = settings.WindowsVersion,
            EnableDebugMode = settings.EnableDebugMode
        };
    }

    /// <summary>
    /// Applies EmulatorConfiguration to IEmulatorSettings
    /// </summary>
    private void ApplyEmulatorConfiguration(IEmulatorSettings settings, EmulatorConfiguration emulatorConfig)
    {
        settings.RenderingBackend = emulatorConfig.RenderingBackend;
        settings.ResolutionScaleFactor = emulatorConfig.ResolutionScaleFactor;
        settings.ReservedMemoryMb = emulatorConfig.ReservedMemoryMb;
        settings.WindowsVersion = emulatorConfig.WindowsVersion;
        settings.EnableDebugMode = emulatorConfig.EnableDebugMode;
    }

    /// <summary>
    /// Get all per-game settings
    /// </summary>
    private Dictionary<string, GameSettings> GetPerGameSettings()
    {
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, GameSettings>>(_settings.PerGameSettings) 
            ?? new Dictionary<string, GameSettings>();
    }

    /// <summary>
    /// Get the list of games from configuration
    /// </summary>
    public Game[] GetGames()
    {
        return System.Text.Json.JsonSerializer.Deserialize<Game[]>(_library.Games) ?? [];
    }

    /// <summary>
    /// Save the list of games to configuration
    /// </summary>
    public void SaveGames(IEnumerable<Game> games)
    {
        _library.Games = System.Text.Json.JsonSerializer.Serialize(games);
    }

    /// <summary>
    /// Get the list of watched folders
    /// </summary>
    public string[] GetWatchedFolders()
    {
        return _library.WatchedFolders ?? [];
    }

    /// <summary>
    /// Save the list of watched folders
    /// </summary>
    public void SaveWatchedFolders(IEnumerable<string> folders)
    {
        _library.WatchedFolders = folders.ToArray();
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
            // Migrate emulator settings
            _settings.RenderingBackend = _legacyConfig.RenderingBackend;
            _settings.ResolutionScaleFactor = _legacyConfig.ResolutionScaleFactor;
            _settings.ReservedMemoryMb = _legacyConfig.ReservedMemoryMb;
            _settings.WindowsVersion = _legacyConfig.WindowsVersion;
            _settings.EnableDebugMode = _legacyConfig.EnableDebugMode;

            // Migrate game library
            _library.Games = _legacyConfig.Games;
            _library.WatchedFolders = _legacyConfig.WatchedFolders;
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
