using AsmResolver.PE;
using AsmResolver.PE.File;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Service for extracting metadata from PE (Portable Executable) files
/// </summary>
public static class PeMetadataService
{
    /// <summary>
    /// Get PE metadata from an executable file
    /// </summary>
    public static PeMetadata? GetMetadata(string executablePath)
    {
        if (!File.Exists(executablePath))
        {
            return null;
        }

        try
        {
            var image = PEImage.FromFile(executablePath);
            var pe = image.PEFile;
            if (pe == null)
            {
                return null;
            }

            var opt = pe.OptionalHeader;
            var fileHeader = pe.FileHeader;
            
            if (opt == null || fileHeader == null)
            {
                return null;
            }

            var fileInfo = new FileInfo(executablePath);
            var compiledDate = GetCompilationDate(fileHeader);
            var machineType = GetMachineType(fileHeader.Machine);
            var (minOs, minOsVersion) = GetMinimumOs(opt);
            var imports = GetImports(image);

            return new PeMetadata
            {
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                DateTimeCompiled = compiledDate,
                MachineType = machineType,
                MinimumOs = minOs,
                MinimumOsVersion = minOsVersion,
                Imports = imports
            };
        }
        catch
        {
            return null;
        }
    }

    private static DateTime? GetCompilationDate(FileHeader fileHeader)
    {
        try
        {
            // TimeDateStamp is seconds since Unix epoch (Jan 1, 1970)
            var timestamp = fileHeader.TimeDateStamp;
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(timestamp);
        }
        catch
        {
            return null;
        }
    }

    private static string GetMachineType(MachineType machine)
    {
        return machine switch
        {
            MachineType.I386 => "Intel 386 or later processors and compatible processors",
            MachineType.Amd64 => "x64 (AMD64/Intel 64)",
            MachineType.Arm => "ARM little endian",
            MachineType.Arm64 => "ARM64 little endian",
            MachineType.Thumb => "ARM or Thumb",
            MachineType.R4000 => "MIPS R4000",
            _ => $"Unknown (0x{(ushort)machine:X4})"
        };
    }

    private static (string MinOs, string MinOsVersion) GetMinimumOs(OptionalHeader optionalHeader)
    {
        var majorVersion = optionalHeader.MajorOperatingSystemVersion;
        var minorVersion = optionalHeader.MinorOperatingSystemVersion;
        
        // Map Windows versions
        var (osName, osVersion) = (majorVersion, minorVersion) switch
        {
            (3, 10) => ("Windows NT 3.1", "3.10"),
            (3, 50) => ("Windows NT 3.5", "3.50"),
            (3, 51) => ("Windows NT 3.51", "3.51"),
            (4, 0) => ("Windows 95 / NT 4.0", "4.00"),
            (5, 0) => ("Windows 2000", "5.00"),
            (5, 1) => ("Windows XP", "5.01"),
            (5, 2) => ("Windows Server 2003 / XP x64", "5.02"),
            (6, 0) => ("Windows Vista / Server 2008", "6.00"),
            (6, 1) => ("Windows 7 / Server 2008 R2", "6.01"),
            (6, 2) => ("Windows 8 / Server 2012", "6.02"),
            (6, 3) => ("Windows 8.1 / Server 2012 R2", "6.03"),
            (10, 0) => ("Windows 10 / 11", "10.0"),
            _ => ("Unknown", $"{majorVersion}.{minorVersion:D2}")
        };

        return (osName, osVersion);
    }

    private static List<PeImport> GetImports(PEImage image)
    {
        var imports = new List<PeImport>();
        
        try
        {
            if (image.Imports == null)
            {
                return imports;
            }

            foreach (var module in image.Imports)
            {
                var dllName = module.Name ?? "Unknown";
                
                foreach (var symbol in module.Symbols)
                {
                    var functionName = symbol.Name ?? $"Ordinal_{symbol.Hint}";
                    
                    imports.Add(new PeImport
                    {
                        DllName = dllName,
                        FunctionName = functionName,
                        Ordinal = symbol.Hint
                    });
                }
            }
        }
        catch
        {
            // Return empty list on error
        }

        return imports;
    }
}

/// <summary>
/// Metadata extracted from a PE file
/// </summary>
public class PeMetadata
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime? DateTimeCompiled { get; set; }
    public string MachineType { get; set; } = string.Empty;
    public string MinimumOs { get; set; } = string.Empty;
    public string MinimumOsVersion { get; set; } = string.Empty;
    public List<PeImport> Imports { get; set; } = new();
}

/// <summary>
/// Represents an import from a DLL
/// </summary>
public class PeImport
{
    public string DllName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public ushort Ordinal { get; set; }
    
    /// <summary>
    /// Whether this import is implemented in the emulator
    /// </summary>
    public bool IsImplemented { get; set; }
}
