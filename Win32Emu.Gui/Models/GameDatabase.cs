namespace Win32Emu.Gui.Models;

/// <summary>
/// Root object for the game database file
/// </summary>
public class GameDatabase
{
    /// <summary>
    /// Version of the database schema
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// List of game entries in the database
    /// </summary>
    public List<GameDbEntry> Games { get; set; } = new();
}
