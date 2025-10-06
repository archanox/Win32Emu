using Win32Emu.CodeGen.ApiMetadata;
using Xunit;

namespace Win32Emu.Tests.CodeGen;

public class MetadataDatabaseTests
{
    [Fact]
    public void AddPeExports_ShouldStoreExports()
    {
        // Arrange
        var db = new MetadataDatabase();
        var exports = new List<ExportedFunction>
        {
            new("GetVersion", 1, null),
            new("GetProcAddress", 2, null),
            new("LoadLibraryA", 3, null)
        };
        
        // Act
        db.AddPeExports("KERNEL32.DLL", exports);
        
        // Assert
        var metadata = db.GetDllMetadata("KERNEL32.DLL");
        Assert.NotNull(metadata);
        Assert.Equal(3, metadata.PeExports.Count);
    }
    
    [Fact]
    public void AddImplementedApi_ShouldStoreImplementation()
    {
        // Arrange
        var db = new MetadataDatabase();
        
        // Act
        db.AddImplementedApi("KERNEL32.DLL", "GetVersion", 0);
        db.AddImplementedApi("KERNEL32.DLL", "GetProcAddress", 8);
        
        // Assert
        var metadata = db.GetDllMetadata("KERNEL32.DLL");
        Assert.NotNull(metadata);
        Assert.Equal(2, metadata.ImplementedApis.Count);
        Assert.Equal(0, metadata.ImplementedApis["GetVersion"]);
        Assert.Equal(8, metadata.ImplementedApis["GetProcAddress"]);
    }
    
    [Fact]
    public void GenerateCoverageReport_ShouldCalculateCorrectly()
    {
        // Arrange
        var db = new MetadataDatabase();
        var exports = new List<ExportedFunction>
        {
            new("GetVersion", 1, null),
            new("GetProcAddress", 2, null),
            new("LoadLibraryA", 3, null),
            new("FreeLibrary", 4, null)
        };
        db.AddPeExports("KERNEL32.DLL", exports);
        db.AddImplementedApi("KERNEL32.DLL", "GetVersion", 0);
        db.AddImplementedApi("KERNEL32.DLL", "GetProcAddress", 8);
        
        // Act
        var report = db.GenerateCoverageReport();
        
        // Assert
        Assert.Equal(4, report.TotalExports);
        Assert.Equal(2, report.TotalImplemented);
        Assert.Equal(50.0, report.CoveragePercentage);
        
        var dllReport = report.DllReports["KERNEL32.DLL"];
        Assert.Equal(4, dllReport.TotalExports);
        Assert.Equal(2, dllReport.ImplementedCount);
        Assert.Equal(50.0, dllReport.CoveragePercentage);
        Assert.Contains("LoadLibraryA", dllReport.MissingApis);
        Assert.Contains("FreeLibrary", dllReport.MissingApis);
    }
    
    [Fact]
    public void GetAllDllNames_ShouldReturnAllNames()
    {
        // Arrange
        var db = new MetadataDatabase();
        db.AddPeExports("KERNEL32.DLL", new List<ExportedFunction>());
        db.AddPeExports("USER32.DLL", new List<ExportedFunction>());
        
        // Act
        var names = db.GetAllDllNames().ToList();
        
        // Assert
        Assert.Equal(2, names.Count);
        Assert.Contains("KERNEL32.DLL", names);
        Assert.Contains("USER32.DLL", names);
    }
}
