using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Legacy application configuration (for migration from config.json)
/// </summary>
public class AppConfiguration
{
    public string RenderingBackend { get; set; } = "Software";
    public int ResolutionScaleFactor { get; set; } = 1;
    public int ReservedMemoryMB { get; set; } = 256;
    public string WindowsVersion { get; set; } = "Windows 95";
    public bool EnableDebugMode { get; set; } = false;
    public List<Game> Games { get; set; } = new();
    public List<string> WatchedFolders { get; set; } = new();
}
