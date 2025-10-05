namespace Win32Emu.Gui.Models;

/// <summary>
/// Represents a developer in the game database
/// </summary>
public class Developer
{
    /// <summary>
    /// Unique identifier for the developer
    /// </summary>
    public Guid Id { get; set; } = Guid.Empty;

    /// <summary>
    /// Developer name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or history
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional website URL
    /// </summary>
    public string? WebsiteUrl { get; set; }
}
