using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
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
    private readonly string _configDirectory;
    private EmulatorSettings _settings;
    private GameLibrary _library;

    public ConfigurationService()
    {
        // Get the application data directory for cross-platform storage
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _configDirectory = Path.Combine(appDataDir, "Win32Emu");

        // Ensure the directory exists
        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
        }

        _settingsFilePath = Path.Combine(_configDirectory, "settings.json");
        _libraryFilePath = Path.Combine(_configDirectory, "library.json");

        // Load configuration using Microsoft.Extensions.Configuration
        _settings = LoadSettings();
        _library = LoadLibrary();
    }

    /// <summary>
    /// Compute SHA256 hash of the executable path
    /// </summary>
    private static string ComputePathHash(string executablePath)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(executablePath));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private EmulatorSettings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new EmulatorSettings();
        }

        try
        {
            // Build configuration using Microsoft.Extensions.Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(_configDirectory)
                .AddJsonFile("settings.json", optional: true, reloadOnChange: false)
                .Build();

            // Use Get<T>() to leverage source generation
            var settings = configuration.Get<EmulatorSettings>();
            return settings ?? new EmulatorSettings();
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
            // Build configuration using Microsoft.Extensions.Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(_configDirectory)
                .AddJsonFile("library.json", optional: true, reloadOnChange: false)
                .Build();

            // Use Get<T>() to leverage source generation
            var library = configuration.Get<GameLibrary>();
            return library ?? new GameLibrary();
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
        var hash = ComputePathHash(gameExecutablePath);

        if (_settings.PerGameSettings.TryGetValue(hash, out var gameSettings))
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
        var hash = ComputePathHash(gameExecutablePath);
        _settings.PerGameSettings[hash] = gameSettings;
        _settings.GamePathMapping[hash] = gameExecutablePath;
        SaveSettings();
    }

    /// <summary>
    /// Get per-game settings for a specific game
    /// </summary>
    public GameSettings? GetGameSettings(string gameExecutablePath)
    {
        var hash = ComputePathHash(gameExecutablePath);
        return _settings.PerGameSettings.TryGetValue(hash, out var settings) ? settings : null;
    }

    /// <summary>
    /// Remove per-game settings for a specific game
    /// </summary>
    public void RemoveGameSettings(string gameExecutablePath)
    {
        var hash = ComputePathHash(gameExecutablePath);
        if (_settings.PerGameSettings.Remove(hash))
        {
            _settings.GamePathMapping.Remove(hash);
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
    /// Get the settings file path for display purposes
    /// </summary>
    public string SettingsFilePath => _settingsFilePath;

    /// <summary>
    /// Get the library file path for display purposes
    /// </summary>
    public string LibraryFilePath => _libraryFilePath;
}
