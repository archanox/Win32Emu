namespace Win32Emu.Gui.Models;

/// <summary>
/// Represents a game entry in the game database
/// </summary>
public class GameDbEntry
{
    /// <summary>
    /// Unique identifier for the game (e.g., Wikidata ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Wikidata key for scraping metadata (e.g., "Q2411602")
    /// </summary>
    public string? WikidataKey { get; set; }

    /// <summary>
    /// Official game title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Game genre(s)
    /// </summary>
    public List<string> Genres { get; set; } = new();

    /// <summary>
    /// Game developer(s)
    /// </summary>
    public List<string> Developers { get; set; } = new();

    /// <summary>
    /// Game publisher(s)
    /// </summary>
    public List<string> Publishers { get; set; } = new();

    /// <summary>
    /// Release date
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Game description/synopsis
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Languages the game is available in (ISO 639 language codes)
    /// </summary>
    public List<string> Languages { get; set; } = new();

    /// <summary>
    /// URL to game logo with transparency
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Game ratings from various organizations
    /// </summary>
    public GameRating? Ratings { get; set; }

    /// <summary>
    /// List of known executables for this game
    /// </summary>
    public List<GameExecutable> Executables { get; set; } = new();

    /// <summary>
    /// External database URLs for reference
    /// </summary>
    public Dictionary<string, string> ExternalUrls { get; set; } = new();

    /// <summary>
    /// Source of the data (e.g., "wikidata", "igdb", "user_submitted")
    /// </summary>
    public string? DataSource { get; set; }
}
