namespace Win32Emu.Tools.Tui.Models;

public class GameEntry
{
	public string Id { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public string ExecutablePath { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? Developer { get; set; }
	public string? Publisher { get; set; }
	public string? Genre { get; set; }
	public int? ReleaseYear { get; set; }
	public DateTime AddedDate { get; set; } = DateTime.Now;
	public DateTime? LastPlayed { get; set; }
	public int PlayCount { get; set; } = 0;
	
	public string? PreferredBackend { get; set; }
	public bool EnableDebugMode { get; set; } = false;
	public Dictionary<string, string> CustomSettings { get; set; } = new();

	public override string ToString()
	{
		return $"{Title} ({ReleaseYear?.ToString() ?? "Unknown"})";
	}
}
