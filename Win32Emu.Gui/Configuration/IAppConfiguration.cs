using Config.Net;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Application configuration interface for emulator settings
/// </summary>
public interface IAppConfiguration
{
    [Option(Alias = "RenderingBackend", DefaultValue = "Software")]
    string RenderingBackend { get; set; }

    [Option(Alias = "ResolutionScaleFactor", DefaultValue = 1)]
    int ResolutionScaleFactor { get; set; }

    [Option(Alias = "ReservedMemoryMB", DefaultValue = 256)]
    int ReservedMemoryMB { get; set; }

    [Option(Alias = "WindowsVersion", DefaultValue = "Windows 95")]
    string WindowsVersion { get; set; }

    [Option(Alias = "EnableDebugMode", DefaultValue = false)]
    bool EnableDebugMode { get; set; }

    [Option(Alias = "GamesJson", DefaultValue = "[]")]
    string GamesJson { get; set; }

    [Option(Alias = "WatchedFolders", DefaultValue = "")]
    string WatchedFolders { get; set; }
}
