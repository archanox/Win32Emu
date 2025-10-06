namespace Win32Emu.CodeGen.ApiMetadata;

/// <summary>
/// Database for storing and querying API metadata from various sources
/// </summary>
public class MetadataDatabase
{
    private readonly Dictionary<string, DllMetadata> _dllMetadata = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Add exports from PE DLL parsing
    /// </summary>
    public void AddPeExports(string dllName, List<ExportedFunction> exports)
    {
        if (!_dllMetadata.TryGetValue(dllName, out var metadata))
        {
            metadata = new DllMetadata(dllName);
            _dllMetadata[dllName] = metadata;
        }
        
        metadata.PeExports.AddRange(exports);
    }
    
    /// <summary>
    /// Add implemented API from StdCallMeta (generated from source)
    /// </summary>
    public void AddImplementedApi(string dllName, string functionName, int argBytes)
    {
        if (!_dllMetadata.TryGetValue(dllName, out var metadata))
        {
            metadata = new DllMetadata(dllName);
            _dllMetadata[dllName] = metadata;
        }
        
        metadata.ImplementedApis[functionName] = argBytes;
    }
    
    /// <summary>
    /// Get metadata for a specific DLL
    /// </summary>
    public DllMetadata? GetDllMetadata(string dllName)
    {
        return _dllMetadata.TryGetValue(dllName, out var metadata) ? metadata : null;
    }
    
    /// <summary>
    /// Get all DLL names in the database
    /// </summary>
    public IEnumerable<string> GetAllDllNames()
    {
        return _dllMetadata.Keys;
    }
    
    /// <summary>
    /// Generate a coverage report comparing implemented vs available APIs
    /// </summary>
    public CoverageReport GenerateCoverageReport()
    {
        var report = new CoverageReport();
        
        foreach (var (dllName, metadata) in _dllMetadata.OrderBy(kvp => kvp.Key))
        {
            var dllReport = new DllCoverageReport
            {
                DllName = dllName,
                TotalExports = metadata.PeExports.Count,
                ImplementedCount = metadata.ImplementedApis.Count,
                MissingApis = metadata.PeExports
                    .Where(e => !metadata.ImplementedApis.ContainsKey(e.Name))
                    .Select(e => e.Name)
                    .OrderBy(n => n)
                    .ToList(),
                ImplementedApis = metadata.ImplementedApis.Keys.OrderBy(k => k).ToList()
            };
            
            report.DllReports[dllName] = dllReport;
            report.TotalExports += dllReport.TotalExports;
            report.TotalImplemented += dllReport.ImplementedCount;
        }
        
        return report;
    }
}

/// <summary>
/// Metadata for a single DLL
/// </summary>
public class DllMetadata
{
    public DllMetadata(string name)
    {
        Name = name;
    }
    
    public string Name { get; }
    
    /// <summary>
    /// Exports parsed from the PE DLL file
    /// </summary>
    public List<ExportedFunction> PeExports { get; } = new();
    
    /// <summary>
    /// APIs implemented in our emulator (function name -> arg bytes)
    /// </summary>
    public Dictionary<string, int> ImplementedApis { get; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Coverage report showing implemented vs missing APIs
/// </summary>
public class CoverageReport
{
    public Dictionary<string, DllCoverageReport> DllReports { get; } = new(StringComparer.OrdinalIgnoreCase);
    public int TotalExports { get; set; }
    public int TotalImplemented { get; set; }
    
    public double CoveragePercentage => TotalExports > 0 ? (TotalImplemented * 100.0 / TotalExports) : 0;
}

/// <summary>
/// Coverage report for a single DLL
/// </summary>
public class DllCoverageReport
{
    public required string DllName { get; init; }
    public int TotalExports { get; init; }
    public int ImplementedCount { get; init; }
    public List<string> MissingApis { get; init; } = new();
    public List<string> ImplementedApis { get; init; } = new();
    
    public double CoveragePercentage => TotalExports > 0 ? (ImplementedCount * 100.0 / TotalExports) : 0;
}
