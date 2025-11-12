using Win32Emu.CodeGen.ApiMetadata;
using Xunit;

namespace Win32Emu.Tests.CodeGen;

public class StubGeneratorTests
{
    [Fact]
    public void GenerateStubs_ShouldCreateMethodStubs()
    {
        // Arrange
        var missingApis = new List<ExportedFunction>
        {
	        new ExportedFunction("GetVersion", 1, null, 0x00001234),
	        new ExportedFunction("GetProcAddress", 2, null, 0x00005678),
	        new ExportedFunction("LoadLibraryA", 3, null, 0x00009ABC)
        };
        
        // Act
        var code = StubGenerator.GenerateStubs("KERNEL32.DLL", missingApis);
        
        // Assert
        Assert.Contains("[DllModuleExport", code);
        Assert.Contains("public uint GetVersion()", code);
        Assert.Contains("public uint GetProcAddress()", code);
        Assert.Contains("public uint LoadLibraryA()", code);
        Assert.Contains("_logger.LogWarning", code);
        Assert.Contains("TODO: Implement", code);
    }
    
    [Fact]
    public void GenerateStubs_WithApiDefinitions_ShouldIncludeParameters()
    {
        // Arrange
        var missingApis = new List<ExportedFunction> { new ExportedFunction("CreateFileA", 1, null, 0x00001000) };
        var definitions = new Dictionary<string, ApiDefinition>
        {
            ["CreateFileA"] = new ApiDefinition(
                "CreateFileA",
                "HANDLE",
                new List<ApiParameter>
                {
                    new("lpFileName", "LPCSTR"),
                    new("dwDesiredAccess", "DWORD"),
                    new("dwShareMode", "DWORD")
                },
                12
            )
        };
        
        // Act
        var code = StubGenerator.GenerateStubs("KERNEL32.DLL", missingApis, definitions);
        
        // Assert
        Assert.Contains("public uint CreateFileA(uint lpFileName, uint dwDesiredAccess, uint dwShareMode)", code);
        Assert.Contains("lpFileName", code);
        Assert.Contains("dwDesiredAccess", code);
        Assert.Contains("dwShareMode", code);
    }
    
    [Fact]
    public void GenerateModuleClass_ShouldCreateCompleteClass()
    {
        // Arrange
        var missingApis = new List<ExportedFunction>
        {
	        new ExportedFunction("DirectInput8Create", 1, null, 0x00001000),
	        new ExportedFunction("DllCanUnloadNow", 2, null, 0x00002000)
        };
        
        // Act
        var code = StubGenerator.GenerateModuleClass("DInput8Module", "DINPUT8.DLL", missingApis);
        
        // Assert
        Assert.Contains("using Win32Emu.Cpu;", code);
        Assert.Contains("using Win32Emu.Memory;", code);
        Assert.Contains("using Microsoft.Extensions.Logging;", code);
        Assert.Contains("namespace Win32Emu.Win32.Modules;", code);
        Assert.Contains("public class DInput8Module : BaseModule", code);
        Assert.Contains("public override string Name => \"DINPUT8.DLL\";", code);
        Assert.Contains("public uint DirectInput8Create()", code);
        Assert.Contains("public uint DllCanUnloadNow()", code);
    }
    
    [Fact]
    public void GenerateStubs_ShouldSortApisByOrdinal()
    {
        // Arrange
        var missingApis = new List<ExportedFunction>
        {
	        new ExportedFunction("ZFunction", 1, null, 0x00001000),
	        new ExportedFunction("AFunction", 2, null, 0x00002000),
	        new ExportedFunction("MFunction", 3, null, 0x00003000)
        };
        
        // Act
        var code = StubGenerator.GenerateStubs("TEST.DLL", missingApis);
        
        // Assert
        var aIndex = code.IndexOf("public uint AFunction()", StringComparison.InvariantCulture);
        var mIndex = code.IndexOf("public uint MFunction()", StringComparison.InvariantCulture);
        var zIndex = code.IndexOf("public uint ZFunction()", StringComparison.InvariantCulture);
        
        Assert.True(zIndex < aIndex, $"ZFunction (ordinal 1) should come before AFunction (ordinal 2). zIndex={zIndex}, aIndex={aIndex}");
        Assert.True(aIndex < mIndex, $"AFunction (ordinal 2) should come before MFunction (ordinal 3). aIndex={aIndex}, mIndex={mIndex}");
    }
    
    [Fact]
    public void GenerateStubs_EmptyList_ShouldReturnOnlyHeader()
    {
        // Arrange
        var missingApis = new List<ExportedFunction>();
        
        // Act
        var code = StubGenerator.GenerateStubs("EMPTY.DLL", missingApis);
        
        // Assert
        Assert.Contains("Auto-generated stubs", code);
        Assert.Contains("EMPTY.DLL", code);
        Assert.DoesNotContain("[DllModuleExport]", code);
    }
    
    [Fact]
    public void GenerateStubs_MultipleVersions_ShouldGenerateMultipleAttributes()
    {
        // Arrange
        var exports = new List<ExportedFunction>
        {
            new ExportedFunction("TestFunction", 1, null, 0x00001000, "4.90.0.3000"), // WinME
            new ExportedFunction("TestFunction", 1, null, 0x00002000, "5.1.2600.6532")  // WinXP
        };
        
        // Act
        var code = StubGenerator.GenerateStubs("TEST.DLL", exports);
        
        // Assert
        // Should have two [DllModuleExport] attributes for the same function
        var attributeCount = code.Split("[DllModuleExport(1,").Length - 1;
        Assert.Equal(2, attributeCount);
        Assert.Contains("entryPoint: 0x00001000", code);
        Assert.Contains("entryPoint: 0x00002000", code);
        Assert.Contains("Version = \"4.90.0.3000\"", code);
        Assert.Contains("Version = \"5.1.2600.6532\"", code);
    }
    
    [Fact]
    public void GenerateStubs_DecoratedNames_ShouldGenerateExportNameField()
    {
        // Arrange
        var exports = new List<ExportedFunction>
        {
            new ExportedFunction("_grDepthBufferMode@4", 1, null, 0x00001000)
        };
        
        // Act
        var code = StubGenerator.GenerateStubs("GLIDE2X.DLL", exports);
        
        // Assert
        Assert.Contains("ExportName = \"_grDepthBufferMode@4\"", code);
        Assert.Contains("public uint grDepthBufferMode()", code);
    }
}
