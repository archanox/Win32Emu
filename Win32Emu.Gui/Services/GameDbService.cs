using System.Text.Json;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Service for managing the game database with support for readonly database and user overrides
/// </summary>
public class GameDbService : IGameDbService
{
    private readonly string _gameDbFilePath;
    private readonly string _userOverridesFilePath;
    private GameDatabase _gameDatabase = new();
    private GameDatabase _userOverrides = new();

    public GameDbService()
    {
        // Path to the readonly gamedb.json shipped with the emulator
        var appDirectory = AppContext.BaseDirectory;
        _gameDbFilePath = Path.Combine(appDirectory, "gamedb.json");

        // Path to user overrides in AppData
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDirectory = Path.Combine(appDataDir, "Win32Emu");
        
        // Ensure the directory exists
        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        _userOverridesFilePath = Path.Combine(configDirectory, "gamedb-overrides.json");

        // Load databases
        LoadDatabases();
    }

    /// <summary>
    /// Load both the readonly game database and user overrides
    /// </summary>
    private void LoadDatabases()
    {
        // Load readonly game database
        if (File.Exists(_gameDbFilePath))
        {
            try
            {
                var json = File.ReadAllText(_gameDbFilePath);
                _gameDatabase = JsonSerializer.Deserialize<GameDatabase>(json) ?? new GameDatabase();
            }
            catch
            {
                // If parsing fails, use empty database
                _gameDatabase = new GameDatabase();
            }
        }

        // Load user overrides
        if (File.Exists(_userOverridesFilePath))
        {
            try
            {
                var json = File.ReadAllText(_userOverridesFilePath);
                _userOverrides = JsonSerializer.Deserialize<GameDatabase>(json) ?? new GameDatabase();
            }
            catch
            {
                // If parsing fails, use empty overrides
                _userOverrides = new GameDatabase();
            }
        }
    }

    /// <summary>
    /// Find a game entry by matching executable hashes
    /// </summary>
    public GameDbEntry? FindGameByExecutable(string executablePath)
    {
        if (!File.Exists(executablePath))
        {
            return null;
        }

        try
        {
            // Compute all hashes for the executable
            var (md5, sha1, sha256) = HashUtility.ComputeAllHashes(executablePath);

            // Check user overrides first (they take precedence)
            var entry = FindByHashes(_userOverrides.Games, md5, sha1, sha256);
            if (entry != null)
            {
                return entry;
            }

            // Check readonly database
            entry = FindByHashes(_gameDatabase.Games, md5, sha1, sha256);
            return entry;
        }
        catch
        {
            // If hash computation fails, return null
            return null;
        }
    }

    /// <summary>
    /// Find a game by matching any of the provided hashes
    /// </summary>
    private static GameDbEntry? FindByHashes(List<GameDbEntry> games, string md5, string sha1, string sha256)
    {
        foreach (var game in games)
        {
            foreach (var executable in game.Executables)
            {
                // Match on any available hash
                if (!string.IsNullOrEmpty(executable.Md5) && 
                    executable.Md5.Equals(md5, StringComparison.OrdinalIgnoreCase))
                {
                    return game;
                }

                if (!string.IsNullOrEmpty(executable.Sha1) && 
                    executable.Sha1.Equals(sha1, StringComparison.OrdinalIgnoreCase))
                {
                    return game;
                }

                if (!string.IsNullOrEmpty(executable.Sha256) && 
                    executable.Sha256.Equals(sha256, StringComparison.OrdinalIgnoreCase))
                {
                    return game;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Get all games in the database (combines readonly and user overrides)
    /// </summary>
    public IEnumerable<GameDbEntry> GetAllGames()
    {
        // Use a dictionary to deduplicate by ID, with user overrides taking precedence
        var gamesById = new Dictionary<string, GameDbEntry>();

        // Add games from readonly database
        foreach (var game in _gameDatabase.Games)
        {
            gamesById[game.Id] = game;
        }

        // Add/override with user overrides
        foreach (var game in _userOverrides.Games)
        {
            gamesById[game.Id] = game;
        }

        return gamesById.Values;
    }

    /// <summary>
    /// Get a game by its ID
    /// </summary>
    public GameDbEntry? GetGameById(string id)
    {
        // Check user overrides first
        var entry = _userOverrides.Games.FirstOrDefault(g => g.Id == id);
        if (entry != null)
        {
            return entry;
        }

        // Check readonly database
        return _gameDatabase.Games.FirstOrDefault(g => g.Id == id);
    }

    /// <summary>
    /// Reload the game database from disk
    /// </summary>
    public void Reload()
    {
        LoadDatabases();
    }
}
