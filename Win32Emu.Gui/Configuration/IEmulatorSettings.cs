using Config.Net;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Emulator settings interface - portable settings that can be carried across machines
/// </summary>
public interface IEmulatorSettings
{
    [Option(Alias = "RenderingBackend", DefaultValue = "Software")]
    string RenderingBackend { get; set; }

    [Option(Alias = "ResolutionScaleFactor", DefaultValue = 1)]
    int ResolutionScaleFactor { get; set; }

    [Option(Alias = "ReservedMemoryMB", DefaultValue = 256)]
    int ReservedMemoryMb { get; set; }

    [Option(Alias = "WindowsVersion", DefaultValue = "Windows 95")]
    string WindowsVersion { get; set; }

    [Option(Alias = "EnableDebugMode", DefaultValue = false)]
    bool EnableDebugMode { get; set; }

    [Option(Alias = "PerGameSettings", DefaultValue = "{}")]
    string PerGameSettings { get; set; }
}
