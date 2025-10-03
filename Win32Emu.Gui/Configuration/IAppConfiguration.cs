using Config.Net;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Application configuration interface for emulator settings
/// </summary>
public interface IAppConfiguration
{
    [Option(Alias = "Emulator.RenderingBackend", DefaultValue = "Software")]
    string RenderingBackend { get; set; }

    [Option(Alias = "Emulator.ResolutionScaleFactor", DefaultValue = 1)]
    int ResolutionScaleFactor { get; set; }

    [Option(Alias = "Emulator.ReservedMemoryMB", DefaultValue = 256)]
    int ReservedMemoryMB { get; set; }

    [Option(Alias = "Emulator.WindowsVersion", DefaultValue = "Windows 95")]
    string WindowsVersion { get; set; }

    [Option(Alias = "Emulator.EnableDebugMode", DefaultValue = false)]
    bool EnableDebugMode { get; set; }

    [Option(Alias = "Library.GamesJson", DefaultValue = "[]")]
    string GamesJson { get; set; }

    [Option(Alias = "Library.WatchedFolders", DefaultValue = "")]
    string WatchedFolders { get; set; }
}
