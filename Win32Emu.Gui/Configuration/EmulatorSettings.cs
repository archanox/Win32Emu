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
    public Dictionary<string, GameSettings> PerGameSettings { get; set; } = new();
}
