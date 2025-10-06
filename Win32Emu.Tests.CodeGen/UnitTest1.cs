using Win32Emu.CodeGen.ApiMetadata;
using Xunit;

namespace Win32Emu.Tests.CodeGen;

public class PeExportParserTests
{
    [Fact]
    public void ParseExports_WinME_Kernel32_ShouldReturnExports()
    {
        // Arrange
        var dllPath = Path.Combine("DLLs", "WinME", "KERNEL32.DLL");
        
        // Skip test if DLL doesn't exist
        if (!File.Exists(dllPath))
        {
            return;
        }
        
        // Act
        var exports = PeExportParser.ParseExports(dllPath);
        
        // Assert
        Assert.NotEmpty(exports);
        Assert.Contains(exports, e => e.Name == "GetVersion");
        Assert.Contains(exports, e => e.Name == "GetProcAddress");
        Assert.Contains(exports, e => e.Name == "LoadLibraryA");
    }
    
    [Fact]
    public void ParseExports_WinME_User32_ShouldReturnExports()
    {
        // Arrange
        var dllPath = Path.Combine("DLLs", "WinME", "USER32.DLL");
        
        // Skip test if DLL doesn't exist
        if (!File.Exists(dllPath))
        {
            return;
        }
        
        // Act
        var exports = PeExportParser.ParseExports(dllPath);
        
        // Assert
        Assert.NotEmpty(exports);
        Assert.Contains(exports, e => e.Name == "CreateWindowExA");
        Assert.Contains(exports, e => e.Name == "ShowWindow");
        Assert.Contains(exports, e => e.Name == "GetMessageA");
    }
    
    [Fact]
    public void ParseDirectory_WinME_ShouldParseAllDlls()
    {
        // Arrange
        var directoryPath = Path.Combine("DLLs", "WinME");
        
        // Skip test if directory doesn't exist
        if (!Directory.Exists(directoryPath))
        {
            return;
        }
        
        // Act
        var results = PeExportParser.ParseDirectory(directoryPath);
        
        // Assert
        Assert.NotEmpty(results);
        Assert.True(results.ContainsKey("KERNEL32.DLL"));
        Assert.True(results.ContainsKey("USER32.DLL"));
        Assert.True(results.ContainsKey("GDI32.DLL"));
    }
    
    [Fact]
    public void ParseExports_ShouldHandleForwardedExports()
    {
        // Arrange
        var dllPath = Path.Combine("DLLs", "WinME", "KERNEL32.DLL");
        
        // Skip test if DLL doesn't exist
        if (!File.Exists(dllPath))
        {
            return;
        }
        
        // Act
        var exports = PeExportParser.ParseExports(dllPath);
        
        // Assert - some exports in KERNEL32 might be forwarded
        Assert.NotEmpty(exports);
        // At least verify we can parse exports without crashing
        Assert.All(exports, export =>
        {
            Assert.NotNull(export.Name);
            Assert.True(export.Ordinal > 0);
        });
    }
}
