namespace Win32Emu.Gui.Models;

/// <summary>
/// Represents game ratings from different rating organizations
/// </summary>
public class GameRating
{
    /// <summary>
    /// ESRB rating (e.g., "E", "T", "M")
    /// </summary>
    public string? Esrb { get; set; }

    /// <summary>
    /// PEGI rating (e.g., "3", "7", "12", "16", "18")
    /// </summary>
    public string? Pegi { get; set; }

    /// <summary>
    /// USK rating (e.g., "0", "6", "12", "16", "18")
    /// </summary>
    public string? Usk { get; set; }

    /// <summary>
    /// ACB rating (e.g., "G", "PG", "M", "MA15+", "R18+")
    /// </summary>
    public string? Acb { get; set; }
}
