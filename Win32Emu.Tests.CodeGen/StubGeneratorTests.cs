using Win32Emu.CodeGen.ApiMetadata;
using Xunit;

namespace Win32Emu.Tests.CodeGen;

public class StubGeneratorTests
{
    [Fact]
    public void GenerateStubs_ShouldCreateMethodStubs()
    {
        // Arrange
        var missingApis = new List<string> { "GetVersion", "GetProcAddress", "LoadLibraryA" };
        
        // Act
        var code = StubGenerator.GenerateStubs("KERNEL32.DLL", missingApis);
        
        // Assert
        Assert.Contains("[DllModuleExport]", code);
        Assert.Contains("public uint GetVersion()", code);
        Assert.Contains("public uint GetProcAddress()", code);
        Assert.Contains("public uint LoadLibraryA()", code);
        Assert.Contains("Diagnostics.Diagnostics.LogWarn", code);
        Assert.Contains("TODO: Implement", code);
    }
    
    [Fact]
    public void GenerateStubs_WithApiDefinitions_ShouldIncludeParameters()
    {
        // Arrange
        var missingApis = new List<string> { "CreateFileA" };
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
        Assert.Contains("lpFileName={lpFileName}", code);
    }
    
    [Fact]
    public void GenerateModuleClass_ShouldCreateCompleteClass()
    {
        // Arrange
        var missingApis = new List<string> { "DirectInput8Create", "DllCanUnloadNow" };
        
        // Act
        var code = StubGenerator.GenerateModuleClass("DInput8Module", "DINPUT8.DLL", missingApis);
        
        // Assert
        Assert.Contains("using Win32Emu.Cpu;", code);
        Assert.Contains("using Win32Emu.Memory;", code);
        Assert.Contains("namespace Win32Emu.Win32.Modules;", code);
        Assert.Contains("public class DInput8Module : BaseModule", code);
        Assert.Contains("public override string Name => \"DINPUT8.DLL\";", code);
        Assert.Contains("public uint DirectInput8Create()", code);
        Assert.Contains("public uint DllCanUnloadNow()", code);
    }
    
    [Fact]
    public void GenerateStubs_ShouldSortApisByName()
    {
        // Arrange
        var missingApis = new List<string> { "ZFunction", "AFunction", "MFunction" };
        
        // Act
        var code = StubGenerator.GenerateStubs("TEST.DLL", missingApis);
        
        // Assert
        var aIndex = code.IndexOf("public uint AFunction()");
        var mIndex = code.IndexOf("public uint MFunction()");
        var zIndex = code.IndexOf("public uint ZFunction()");
        
        Assert.True(aIndex < mIndex);
        Assert.True(mIndex < zIndex);
    }
    
    [Fact]
    public void GenerateStubs_EmptyList_ShouldReturnOnlyHeader()
    {
        // Arrange
        var missingApis = new List<string>();
        
        // Act
        var code = StubGenerator.GenerateStubs("EMPTY.DLL", missingApis);
        
        // Assert
        Assert.Contains("Auto-generated stubs", code);
        Assert.Contains("EMPTY.DLL", code);
        Assert.DoesNotContain("[DllModuleExport]", code);
    }
}
