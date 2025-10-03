using System.Text.Json;
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
        return new EmulatorConfiguration
        {
            RenderingBackend = _config.RenderingBackend,
            ResolutionScaleFactor = _config.ResolutionScaleFactor,
            ReservedMemoryMB = _config.ReservedMemoryMB,
            WindowsVersion = _config.WindowsVersion,
            EnableDebugMode = _config.EnableDebugMode
        };
    }

    /// <summary>
    /// Save the emulator configuration
    /// </summary>
    public void SaveEmulatorConfiguration(EmulatorConfiguration configuration)
    {
        _config.RenderingBackend = configuration.RenderingBackend;
        _config.ResolutionScaleFactor = configuration.ResolutionScaleFactor;
        _config.ReservedMemoryMB = configuration.ReservedMemoryMB;
        _config.WindowsVersion = configuration.WindowsVersion;
        _config.EnableDebugMode = configuration.EnableDebugMode;
    }

    /// <summary>
    /// Get the list of games from configuration
    /// </summary>
    public List<Game> GetGames()
    {
        try
        {
            var gamesJson = _config.GamesJson;
            if (string.IsNullOrWhiteSpace(gamesJson) || gamesJson == "[]")
            {
                return new List<Game>();
            }

            var games = JsonSerializer.Deserialize<List<Game>>(gamesJson);
            return games ?? new List<Game>();
        }
        catch
        {
            return new List<Game>();
        }
    }

    /// <summary>
    /// Save the list of games to configuration
    /// </summary>
    public void SaveGames(IEnumerable<Game> games)
    {
        var gamesJson = JsonSerializer.Serialize(games, new JsonSerializerOptions
        {
            WriteIndented = false
        });
        _config.GamesJson = gamesJson;
    }

    /// <summary>
    /// Get the list of watched folders
    /// </summary>
    public List<string> GetWatchedFolders()
    {
        var foldersStr = _config.WatchedFolders;
        if (string.IsNullOrWhiteSpace(foldersStr))
        {
            return new List<string>();
        }

        return foldersStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .ToList();
    }

    /// <summary>
    /// Save the list of watched folders
    /// </summary>
    public void SaveWatchedFolders(IEnumerable<string> folders)
    {
        _config.WatchedFolders = string.Join(";", folders);
    }

    /// <summary>
    /// Get the configuration file path for display purposes
    /// </summary>
    public string ConfigFilePath => _configFilePath;
}
