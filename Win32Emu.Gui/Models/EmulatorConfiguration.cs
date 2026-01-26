namespace Win32Emu.Gui.Models;

public class EmulatorConfiguration
{
	public string RenderingBackend { get; set; } = "SDL";
	public string InputBackend { get; set; } = "SDL";
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
	
	/// <summary>
	/// Force interpreter mode even on desktop platforms (disables JIT compilation).
	/// Useful for debugging or when JIT compilation causes issues. Default: false (JIT enabled on desktop).
	/// </summary>
	public bool ForceInterpreterMode { get; set; } = false;
	
	// Codepage Settings
	/// <summary>
	/// Default ANSI code page (CP_ACP) used by the emulator.
	/// Common values: 1252 (Western European), 932 (Japanese), 936 (Chinese Simplified),
	/// 949 (Korean), 950 (Chinese Traditional), 1251 (Cyrillic), 65001 (UTF-8).
	/// Default: 65001 (UTF-8) for maximum compatibility.
	/// </summary>
	public uint DefaultAnsiCodePage { get; set; } = 65001; // UTF-8
	
	/// <summary>
	/// Default OEM code page (CP_OEMCP) used by the emulator.
	/// Common values: 437 (US), 850 (Multilingual Latin I), 852 (Latin II).
	/// Default: 437 (IBM PC US).
	/// </summary>
	public uint DefaultOemCodePage { get; set; } = 437; // IBM PC US
	
	// OpenTelemetry Settings
	public bool EnableOpenTelemetry { get; set; }
	public bool UseConsoleExporter { get; set; }
	public bool UseOtlpExporter { get; set; }
	public string OtlpEndpoint { get; set; } = "http://localhost:4317";
	
	// MCP (Model Context Protocol) Debugging Settings
	/// <summary>
	/// Enable MCP server for AI-assisted debugging
	/// </summary>
	public bool EnableMcpServer { get; set; } = false;
	
	/// <summary>
	/// Automatically start MCP server when emulator is launched
	/// </summary>
	public bool AutoStartMcpServer { get; set; } = false;
	
	/// <summary>
	/// Use HTTP transport for MCP server (required for Visual Studio integration).
	/// If false, uses STDIO transport (for command-line AI tools).
	/// </summary>
	public bool McpUseHttpTransport { get; set; } = true;
	
	/// <summary>
	/// Port for MCP HTTP server (default: 5111)
	/// </summary>
	public int McpHttpPort { get; set; } = 5111;
	
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