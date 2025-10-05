namespace Win32Emu.Gui.Models;

/// <summary>
/// Represents a genre in the game database
/// </summary>
public class Genre
{
    /// <summary>
    /// Unique identifier for the genre
    /// </summary>
    public Guid Id { get; set; } = Guid.Empty;

    /// <summary>
    /// Genre name (e.g., "Racing", "Action", "RPG")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the genre
    /// </summary>
    public string? Description { get; set; }
}
