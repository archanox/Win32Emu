namespace Win32Emu.Gui.Models;

public class Game
{
    public string Title { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public string? Description { get; set; }
    public DateTime? LastPlayed { get; set; }
    public int TimesPlayed { get; set; }
}
