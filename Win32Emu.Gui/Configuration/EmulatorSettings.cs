using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Configuration;

/// <summary>
/// Emulator settings - portable settings that can be carried across machines
/// </summary>
public class EmulatorSettings
{
    public string RenderingBackend { get; set; } = "GLFW";
    public string InputBackend { get; set; } = "GLFW"; // SDL or GLFW
    public string CpuBackend { get; set; } = "IcedCPU"; // IcedCPU, JitCPU, or Unicorn
    public int ResolutionScaleFactor { get; set; } = 1;
    public int ReservedMemoryMB { get; set; } = 256;
    public string WindowsVersion { get; set; } = "Windows 95";
    public bool EnableDebugMode { get; set; } = false;
    public bool EnableGdbServer { get; set; } = false;
    public int GdbServerPort { get; set; } = 1234;
    public bool GdbPauseOnStart { get; set; } = true;
    
    // Instruction Analyzer Settings
    public bool EnableInstructionAnalyzer { get; set; } = false;
    public bool EnableLegacyInstructionDecoding { get; set; } = false;
    
    // OpenTelemetry Settings
    public bool EnableOpenTelemetry { get; set; } = false;
    public bool UseConsoleExporter { get; set; } = false;
    public bool UseOtlpExporter { get; set; } = false;
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
    
    // Virtual Disk Settings
    /// <summary>
    /// Enable virtual disk by default for all games (can be overridden per-game)
    /// </summary>
    public bool UseVirtualDiskByDefault { get; set; } = true;
    
    /// <summary>
    /// Default size for auto-created virtual disks in MB
    /// </summary>
    public int DefaultVirtualDiskSizeMb { get; set; } = 512;
    
    /// <summary>
    /// Virtual disk format to use (VHD, VHDX)
    /// </summary>
    public string VirtualDiskFormat { get; set; } = "VHD";
    
    /// <summary>
    /// Directory where virtual disks are stored
    /// </summary>
    public string? VirtualDisksDirectory { get; set; }
    
    /// <summary>
    /// Per-game settings keyed by SHA256 hash of the executable file
    /// </summary>
    public Dictionary<string, GameSettings> PerGameSettings { get; set; } = new();

    /// <summary>
    /// Controller configurations for physical controllers
    /// Key: physical controller name or ID
    /// </summary>
    public Dictionary<string, ControllerConfiguration> ControllerConfigurations { get; set; } = new();
}
