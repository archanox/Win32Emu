namespace Win32Emu.Gui.Models;

/// <summary>
/// Represents a publisher in the game database
/// </summary>
public class Publisher
{
    /// <summary>
    /// Unique identifier for the publisher
    /// </summary>
    public Guid Id { get; set; } = Guid.Empty;

    /// <summary>
    /// Publisher name
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
