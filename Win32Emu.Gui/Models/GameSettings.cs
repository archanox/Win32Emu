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
    /// Command-line arguments to pass to the game
    /// </summary>
    public string? ProgramArguments { get; set; }
    
    /// <summary>
    /// Enable virtual disk for this game (uses a VHD file as C: drive)
    /// </summary>
    public bool? UseVirtualDisk { get; set; }
    
    /// <summary>
    /// Path to the virtual disk file (VHD/VHDX). If null, one will be auto-created.
    /// </summary>
    public string? VirtualDiskPath { get; set; }
    
    /// <summary>
    /// Size of the virtual disk in MB (used when auto-creating). Default: 512 MB
    /// </summary>
    public int? VirtualDiskSizeMb { get; set; }
    
    /// <summary>
    /// Source directory to copy into the virtual disk on first run (e.g., game installation folder)
    /// </summary>
    public string? VirtualDiskSourceDirectory { get; set; }
}
