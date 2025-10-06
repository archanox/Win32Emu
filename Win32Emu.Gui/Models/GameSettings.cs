namespace Win32Emu.Gui.Models;

/// <summary>
/// Per-game emulator settings override
/// </summary>
public class GameSettings
{
    public string? RenderingBackend { get; set; }
    public int? ResolutionScaleFactor { get; set; }
    public int? ReservedMemoryMb { get; set; }
    public string? WindowsVersion { get; set; }
    public bool? EnableDebugMode { get; set; }
    
    /// <summary>
    /// Environment variables for this game (KEY=value pairs)
    /// </summary>
    public Dictionary<string, string>? EnvironmentVariables { get; set; }
    
    /// <summary>
    /// Command-line arguments to pass to the game
    /// </summary>
    public string? ProgramArguments { get; set; }
}
