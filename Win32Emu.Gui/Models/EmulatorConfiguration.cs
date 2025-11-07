namespace Win32Emu.Gui.Models;

public class EmulatorConfiguration
{
	public string RenderingBackend { get; set; } = "SDL";
	public string InputBackend { get; set; } = "SDL";
	public string CpuBackend { get; set; } = "IcedCPU";
	public int ResolutionScaleFactor { get; set; } = 1;
	public int ReservedMemoryMb { get; set; } = 256;
	public string WindowsVersion { get; set; } = "Windows 95";
	public bool EnableDebugMode { get; set; }
	public bool EnableGdbServer { get; set; }
	public int GdbServerPort { get; set; } = 1234;
	public bool GdbPauseOnStart { get; set; } = true;
	
	// Instruction Analyzer Settings
	public bool EnableInstructionAnalyzer { get; set; }
	public bool EnableLegacyInstructionDecoding { get; set; }
	
	// OpenTelemetry Settings
	public bool EnableOpenTelemetry { get; set; }
	public bool UseConsoleExporter { get; set; }
	public bool UseOtlpExporter { get; set; }
	public string OtlpEndpoint { get; set; } = "http://localhost:4317";
	
	// File Logging Settings
	public bool EnableFileLogging { get; set; }
	public string? LogFileDirectory { get; set; }
	
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
	/// Virtual disk format to use (VHD, VHDX, VMDK)
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
}