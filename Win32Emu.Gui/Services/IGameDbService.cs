using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Interface for game database operations
/// </summary>
public interface IGameDbService
{
    /// <summary>
    /// Find a game entry by matching executable hashes
    /// </summary>
    /// <param name="executablePath">Path to the executable file</param>
    /// <returns>Matching game database entry, or null if not found</returns>
    GameDbEntry? FindGameByExecutable(string executablePath);

    /// <summary>
    /// Get all games in the database
    /// </summary>
    /// <returns>List of all game entries</returns>
    IEnumerable<GameDbEntry> GetAllGames();

    /// <summary>
    /// Get a game by its ID
    /// </summary>
    /// <param name="id">Game ID</param>
    /// <returns>Game entry, or null if not found</returns>
    GameDbEntry? GetGameById(string id);

    /// <summary>
    /// Reload the game database from disk
    /// </summary>
    void Reload();
}
