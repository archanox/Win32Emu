using Config.Net;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Game library interface - machine-specific game library and watched folders
/// </summary>
public interface IGameLibrary
{
    [Option(Alias = "Games", DefaultValue = "[]")]
    string Games { get; set; }

    [Option(Alias = "WatchedFolders")]
    string[]? WatchedFolders { get; set; }
}
