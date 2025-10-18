namespace Win32Emu.Gui.Models;

public class EmulatorConfiguration
{
	public string RenderingBackend { get; set; } = "SDL";
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
}