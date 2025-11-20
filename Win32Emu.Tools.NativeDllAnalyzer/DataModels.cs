namespace Win32Emu.Tools.NativeDllAnalyzer;

/// <summary>
/// Complete analysis report comparing native DLL exports with Win32Emu implementations.
/// </summary>
public class NativeDllAnalysisReport
{
	public string AnalyzedAt { get; set; } = "";
	public string DllDirectory { get; set; } = "";
	public AnalysisSummary Summary { get; set; } = new();
	public List<DllAnalysis> Dlls { get; set; } = new();
}

/// <summary>
/// Summary statistics for the entire analysis.
/// </summary>
public class AnalysisSummary
{
	public int TotalDllsAnalyzed { get; set; }
	public int TotalNativeExports { get; set; }
	public int TotalImplemented { get; set; }
	public int TotalStubs { get; set; }
	public int TotalMissing { get; set; }
	public double ImplementationPercentage { get; set; }
}

/// <summary>
/// Analysis results for a single DLL.
/// </summary>
public class DllAnalysis
{
	public string DllName { get; set; } = "";
	public List<ExportInfo> NativeExports { get; set; } = new();
	public List<string> ImplementedFunctions { get; set; } = new();
	public List<string> StubFunctions { get; set; } = new();
	public List<string> MissingFunctions { get; set; } = new();
	public List<string> ExtraImplementations { get; set; } = new();
	public double CoveragePercentage { get; set; }
}

/// <summary>
/// Information about a native DLL export.
/// </summary>
public class ExportInfo
{
	public string Name { get; set; } = "";
	public uint Ordinal { get; set; }
}
