using AsmResolver.PE;
using AsmResolver.PE.Exports;

namespace Win32Emu.CodeGen.ApiMetadata;

/// <summary>
/// Parser for extracting export tables from PE DLL files
/// </summary>
public class PeExportParser
{
    /// <summary>
    /// Parse exports from a PE DLL file
    /// </summary>
    /// <param name="dllPath">Path to the DLL file</param>
    /// <returns>List of exported functions with their ordinals</returns>
    public static List<ExportedFunction> ParseExports(string dllPath)
    {
        var exports = new List<ExportedFunction>();
        
        try
        {
            var image = PEImage.FromFile(dllPath);
            var exportDirectory = image.Exports;
            
            if (exportDirectory == null)
            {
                Console.WriteLine($"Warning: No export directory found in {Path.GetFileName(dllPath)}");
                return exports;
            }

            // Extract version from PE resources
            string? version = ExtractFileVersion(dllPath);
            
            foreach (var export in exportDirectory.Entries)
            {
                // Get entry point RVA if not forwarded
                uint? entryPoint = export.IsForwarder ? null : export.Address?.Rva;
                
                // Skip forwarded exports for now (they don't have actual implementations in this DLL)
                if (export.IsByName && export.Name != null)
                {
                    exports.Add(new ExportedFunction(
                        export.Name,
                        export.Ordinal,
                        export.IsForwarder ? export.ForwarderName : null,
                        entryPoint,
                        version
                    ));
                }
                else if (!export.IsByName)
                {
                    // Export by ordinal only
                    exports.Add(new ExportedFunction(
                        $"Ordinal_{export.Ordinal}",
                        export.Ordinal,
                        export.IsForwarder ? export.ForwarderName : null,
                        entryPoint,
                        version
                    ));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing {Path.GetFileName(dllPath)}: {ex.Message}");
        }
        
        return exports;
    }

    /// <summary>
    /// Extract file version from PE resources
    /// TODO: Implement proper version extraction using AsmResolver API
    /// For now, returns null - version info will not be populated
    /// </summary>
    private static string? ExtractFileVersion(string dllPath)
    {
        // Version extraction would require deeper integration with AsmResolver
        // For now, we'll leave this as a TODO and return null
        // The Version field will still be part of the attribute but won't be populated yet
        return null;
    }
    
    /// <summary>
    /// Parse all DLL files in a directory
    /// </summary>
    /// <param name="directoryPath">Path to directory containing DLLs</param>
    /// <returns>Dictionary mapping DLL name to list of exports</returns>
    public static Dictionary<string, List<ExportedFunction>> ParseDirectory(string directoryPath)
    {
        var results = new Dictionary<string, List<ExportedFunction>>(StringComparer.OrdinalIgnoreCase);
        
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Warning: Directory not found: {directoryPath}");
            return results;
        }
        
        var dllFiles = Directory.GetFiles(directoryPath, "*.dll", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(directoryPath, "*.DLL", SearchOption.TopDirectoryOnly))
            .Distinct();
        
        foreach (var dllPath in dllFiles)
        {
            var dllName = Path.GetFileName(dllPath).ToUpperInvariant();
            var exports = ParseExports(dllPath);
            results[dllName] = exports;
            
            Console.WriteLine($"Parsed {dllName}: {exports.Count} exports");
        }
        
        return results;
    }
}

/// <summary>
/// Represents an exported function from a PE DLL
/// </summary>
/// <param name="Name">Function name</param>
/// <param name="Ordinal">Export ordinal number</param>
/// <param name="ForwardedTo">If this is a forwarded export, the target (e.g., "KERNELBASE.GetVersion")</param>
/// <param name="EntryPoint">RVA of the function entry point (null if forwarded)</param>
/// <param name="Version">DLL version string (e.g., "4.90.0.3000")</param>
public record ExportedFunction(string Name, uint Ordinal, string? ForwardedTo, uint? EntryPoint = null, string? Version = null);
