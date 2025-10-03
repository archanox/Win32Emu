using Config.Net;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Service for managing application configuration with persistence
/// </summary>
public class ConfigurationService
{
    private readonly IAppConfiguration _config;
    private readonly string _configFilePath;

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

        _configFilePath = Path.Combine(win32EmuDir, "config.json");

        // Build the configuration using Config.Net with JSON file storage
        _config = new ConfigurationBuilder<IAppConfiguration>()
            .UseJsonFile(_configFilePath)
            .Build();
    }

    /// <summary>
    /// Get the emulator configuration
    /// </summary>
    public EmulatorConfiguration GetEmulatorConfiguration()
    {
        return MapToEmulatorConfiguration(_config);
    }

    /// <summary>
    /// Save the emulator configuration
    /// </summary>
    public void SaveEmulatorConfiguration(EmulatorConfiguration configuration)
    {
        ApplyEmulatorConfiguration(_config, configuration);
    }

    /// <summary>
    /// Maps IAppConfiguration to EmulatorConfiguration
    /// </summary>
    private EmulatorConfiguration MapToEmulatorConfiguration(IAppConfiguration config)
    {
        return new EmulatorConfiguration
        {
            RenderingBackend = config.RenderingBackend,
            ResolutionScaleFactor = config.ResolutionScaleFactor,
            ReservedMemoryMb = config.ReservedMemoryMb,
            WindowsVersion = config.WindowsVersion,
            EnableDebugMode = config.EnableDebugMode
        };
    }

    /// <summary>
    /// Applies EmulatorConfiguration to IAppConfiguration
    /// </summary>
    private void ApplyEmulatorConfiguration(IAppConfiguration config, EmulatorConfiguration emulatorConfig)
    {
        config.RenderingBackend = emulatorConfig.RenderingBackend;
        config.ResolutionScaleFactor = emulatorConfig.ResolutionScaleFactor;
        config.ReservedMemoryMb = emulatorConfig.ReservedMemoryMb;
        config.WindowsVersion = emulatorConfig.WindowsVersion;
        config.EnableDebugMode = emulatorConfig.EnableDebugMode;
    }

    /// <summary>
    /// Get the list of games from configuration
    /// </summary>
    public Game[] GetGames()
    {
        return System.Text.Json.JsonSerializer.Deserialize<Game[]>(_config.Games) ?? [];
    }

    /// <summary>
    /// Save the list of games to configuration
    /// </summary>
    public void SaveGames(IEnumerable<Game> games)
    {
        _config.Games = System.Text.Json.JsonSerializer.Serialize(games);
    }

    /// <summary>
    /// Get the list of watched folders
    /// </summary>
    public string[] GetWatchedFolders()
    {
        return _config.WatchedFolders ?? [];
    }

    /// <summary>
    /// Save the list of watched folders
    /// </summary>
    public void SaveWatchedFolders(IEnumerable<string> folders)
    {
        _config.WatchedFolders = folders.ToArray();
    }

    /// <summary>
    /// Get the configuration file path for display purposes
    /// </summary>
    public string ConfigFilePath => _configFilePath;
}
