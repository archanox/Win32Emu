namespace Win32Emu.Gui.Models;

/// <summary>
/// Represents an executable file with multiple hash types for identification
/// </summary>
public class GameExecutable
{
    /// <summary>
    /// Name of the executable file (e.g., "ign_win.exe")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// MD5 hash of the executable
    /// </summary>
    public string? Md5 { get; set; }

    /// <summary>
    /// SHA-1 hash of the executable
    /// </summary>
    public string? Sha1 { get; set; }

    /// <summary>
    /// SHA-256 hash of the executable
    /// </summary>
    public string? Sha256 { get; set; }
}
