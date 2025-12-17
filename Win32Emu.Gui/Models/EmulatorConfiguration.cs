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
	
	// CPU Emulation Settings
	/// <summary>
	/// Force 32-bit operand size for stack operations (PUSH/POP/CALL/RET) in 32-bit mode,
	/// ignoring operand-size override prefix (0x66). Improves Win32 compatibility but may
	/// break Win16 or mixed-mode code. Default: true for Win32 compatibility.
	/// </summary>
	public bool Force32BitStackOps { get; set; } = true;
	
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
	/// Default size for auto-created virtual disks in MB.
	/// Minimum 1024 MB (1 GB) recommended to ensure FAT32 instead of FAT16.
	/// FAT32 provides better compatibility: 255-char paths, 4GB max file size.
	/// </summary>
	public int DefaultVirtualDiskSizeMb { get; set; } = 1024;
	
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
}