using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Emulator settings - portable settings that can be carried across machines
/// </summary>
public class EmulatorSettings
{
    public string RenderingBackend { get; set; } = "Software";
    public int ResolutionScaleFactor { get; set; } = 1;
    public int ReservedMemoryMB { get; set; } = 256;
    public string WindowsVersion { get; set; } = "Windows 95";
    public bool EnableDebugMode { get; set; } = false;
    public bool EnableGdbServer { get; set; } = false;
    public int GdbServerPort { get; set; } = 1234;
    public bool GdbPauseOnStart { get; set; } = true;
    
    /// <summary>
    /// Per-game settings keyed by SHA256 hash of the executable file
    /// </summary>
    public Dictionary<string, GameSettings> PerGameSettings { get; set; } = new();
}
