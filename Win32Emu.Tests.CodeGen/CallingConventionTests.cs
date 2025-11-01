using Win32Emu.CallingConvention;
using Xunit;

namespace Win32Emu.Tests.CodeGen;

public class CallingConventionTests
{
    [Fact]
    public void ParseTypeDefinitions_ShouldParseStructs()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<ApiMonitor>
    <Module Name=""Test.dll"">
        <Variable Name=""TEST_STRUCT"" Type=""Struct"">
            <Field Type=""DWORD"" Name=""dwValue"" />
            <Field Type=""LPCSTR"" Name=""lpString"" />
        </Variable>
    </Module>
</ApiMonitor>";
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);
        
        try
        {
            // Act
            var parser = new RekoXmlApiParser();
            var types = parser.ParseTypeDefinitions(tempFile);
            
            // Assert
            Assert.Single(types.Structs);
            Assert.Equal("TEST_STRUCT", types.Structs[0].Name);
            Assert.Equal(2, types.Structs[0].Fields.Count);
            Assert.Equal("dwValue", types.Structs[0].Fields[0].Name);
            Assert.Equal("DWORD", types.Structs[0].Fields[0].Type);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void ParseTypeDefinitions_ShouldParseCallbacks()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<ApiMonitor>
    <Module Name=""Test.dll"">
        <Variable Name=""WNDPROC"" Type=""Alias"" Base=""LPVOID"" />
        <Variable Name=""MSGBOXCALLBACK"" Type=""Alias"" Base=""LPVOID"" />
    </Module>
</ApiMonitor>";
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);
        
        try
        {
            // Act
            var parser = new RekoXmlApiParser();
            var types = parser.ParseTypeDefinitions(tempFile);
            
            // Assert
            Assert.Equal(2, types.Callbacks.Count);
            Assert.Contains(types.Callbacks, c => c.Name == "WNDPROC");
            Assert.Contains(types.Callbacks, c => c.Name == "MSGBOXCALLBACK");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void ParseTypeDefinitions_ShouldParseAliases()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<ApiMonitor>
    <Module Name=""Test.dll"">
        <Variable Name=""HDESK"" Type=""Alias"" Base=""HANDLE"" />
    </Module>
</ApiMonitor>";
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);
        
        try
        {
            // Act
            var parser = new RekoXmlApiParser();
            var types = parser.ParseTypeDefinitions(tempFile);
            
            // Assert
            Assert.Single(types.Aliases);
            Assert.Equal("HDESK", types.Aliases[0].Name);
            Assert.Equal("HANDLE", types.Aliases[0].BaseType);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void ParseTypeDefinitions_ShouldParseEnums()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<ApiMonitor>
    <Module Name=""Test.dll"">
        <Variable Name=""MY_ENUM"" Type=""Alias"" Base=""DWORD"">
            <Enum>
                <Set Name=""VALUE_ONE"" Value=""1"" />
                <Set Name=""VALUE_TWO"" Value=""2"" />
            </Enum>
        </Variable>
    </Module>
</ApiMonitor>";
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);
        
        try
        {
            // Act
            var parser = new RekoXmlApiParser();
            var types = parser.ParseTypeDefinitions(tempFile);
            
            // Assert
            Assert.Single(types.Aliases);
            Assert.NotNull(types.Aliases[0].EnumValues);
            Assert.Equal(2, types.Aliases[0].EnumValues!.Count);
            Assert.Equal("1", types.Aliases[0].EnumValues["VALUE_ONE"]);
            Assert.Equal("2", types.Aliases[0].EnumValues["VALUE_TWO"]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void GenerateStructDefinition_ShouldCreateValidCSharpCode()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var structDef = new StructDefinition
        {
            Name = "TEST_STRUCT",
            Fields = new List<StructField>
            {
                new() { Name = "dwValue", Type = "DWORD" },
                new() { Name = "lpString", Type = "LPCSTR" }
            }
        };
        
        // Act
        var code = generator.GenerateStructDefinition(structDef);
        
        // Assert
        Assert.Contains("struct TEST_STRUCT", code);
        Assert.Contains("[StructLayout(LayoutKind.Sequential)]", code);
        Assert.Contains("public uint dwValue", code);
        Assert.Contains("public uint lpString", code);
    }
    
    [Fact]
    public void GenerateCallbackDelegate_ShouldCreateValidDelegate()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var callback = new CallbackDefinition
        {
            Name = "WNDPROC",
            BaseType = "LPVOID"
        };
        
        // Act
        var code = generator.GenerateCallbackDelegate(callback);
        
        // Assert
        Assert.Contains("delegate", code);
        Assert.Contains("WNDPROC", code);
    }
    
    [Fact]
    public void GenerateDocumentation_ShouldIncludeAllParameters()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var signature = new ApiSignature
        {
            Name = "TestFunc",
            DllName = "test.dll",
            ReturnType = "DWORD",
            Parameters = new List<ApiParameter>
            {
                new() { Name = "param1", Type = "DWORD" },
                new() { Name = "param2", Type = "LPCSTR" }
            }
        };
        
        // Act
        var docs = generator.GenerateDocumentation(signature, "Test Category");
        
        // Assert
        Assert.Contains("<summary>", docs);
        Assert.Contains("TestFunc", docs);
        Assert.Contains("<param name=\"param1\">", docs);
        Assert.Contains("<param name=\"param2\">", docs);
        Assert.Contains("<returns>DWORD</returns>", docs);
        Assert.Contains("Test Category", docs);
    }
    
    [Fact]
    public void GenerateUnitTest_ShouldCreateValidTest()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var signature = new ApiSignature
        {
            Name = "TestFunc",
            Parameters = new List<ApiParameter>
            {
                new() { Name = "param1", Type = "DWORD" }
            }
        };
        
        // Act
        var test = generator.GenerateUnitTest(signature);
        
        // Assert
        Assert.Contains("[Fact]", test);
        Assert.Contains("Test_TestFunc_Basic", test);
        Assert.Contains("// Arrange", test);
        Assert.Contains("// Act", test);
        Assert.Contains("// Assert", test);
        Assert.Contains("var param1", test);
    }
    
    [Fact]
    public void GenerateValidationReport_ShouldDetectMismatches()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var expected = new ApiSignature
        {
            Name = "TestFunc",
            ReturnType = "DWORD",
            Parameters = new List<ApiParameter>
            {
                new() { Name = "param1", Type = "DWORD" },
                new() { Name = "param2", Type = "LPCSTR" }
            }
        };
        
        var actual = new ApiSignature
        {
            Name = "TestFunc",
            ReturnType = "BOOL",  // Different return type
            Parameters = new List<ApiParameter>
            {
                new() { Name = "param1", Type = "DWORD" }  // Missing param2
            }
        };
        
        // Act
        var report = generator.GenerateValidationReport(expected, actual);
        
        // Assert
        Assert.Contains("INVALID", report);
        Assert.Contains("Return type mismatch", report);
        Assert.Contains("Parameter count mismatch", report);
    }
    
    [Fact]
    public void GenerateValidationReport_ShouldPassForMatchingSignatures()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var signature = new ApiSignature
        {
            Name = "TestFunc",
            ReturnType = "DWORD",
            Parameters = new List<ApiParameter>
            {
                new() { Name = "param1", Type = "DWORD" }
            }
        };
        
        // Act
        var report = generator.GenerateValidationReport(signature, signature);
        
        // Assert
        Assert.Contains("VALID", report);
        Assert.DoesNotContain("mismatch", report, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void GenerateTypeAlias_ShouldCreateEnumForEnumTypes()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var alias = new TypeAlias
        {
            Name = "MY_ENUM",
            BaseType = "DWORD",
            EnumValues = new Dictionary<string, string>
            {
                ["VALUE_ONE"] = "1",
                ["VALUE_TWO"] = "2"
            }
        };
        
        // Act
        var code = generator.GenerateTypeAlias(alias);
        
        // Assert
        Assert.Contains("enum MY_ENUM", code);
        Assert.Contains("VALUE_ONE = 1", code);
        Assert.Contains("VALUE_TWO = 2", code);
    }
    
    [Fact]
    public void GenerateTypeAlias_ShouldCreateUsingForSimpleAliases()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var alias = new TypeAlias
        {
            Name = "HDESK",
            BaseType = "HANDLE"
        };
        
        // Act
        var code = generator.GenerateTypeAlias(alias);
        
        // Assert
        Assert.Contains("using HDESK", code);
    }
    
    [Fact]
    public void ParseTypeDefinitions_ShouldParseComInterfaces()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<ApiMonitor>
    <Module Name=""Test.dll"">
        <Variable Name=""ITestInterface"" Type=""Interface"" />
        <Variable Name=""ICustomInterface"" Type=""Interface"" Base=""IUnknown"" />
    </Module>
</ApiMonitor>";
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xmlContent);
        
        try
        {
            // Act
            var parser = new RekoXmlApiParser();
            var types = parser.ParseTypeDefinitions(tempFile);
            
            // Assert
            Assert.Equal(2, types.ComInterfaces.Count);
            Assert.Contains(types.ComInterfaces, c => c.Name == "ITestInterface");
            Assert.Contains(types.ComInterfaces, c => c.Name == "ICustomInterface");
            var customInterface = types.ComInterfaces.First(c => c.Name == "ICustomInterface");
            Assert.Equal("IUnknown", customInterface.BaseInterface);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void GenerateComInterface_ShouldCreateValidInterface()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var interfaceDef = new ComInterfaceDefinition
        {
            Name = "ITestInterface",
            BaseInterface = "IUnknown",
            Guid = "{12345678-1234-1234-1234-123456789ABC}"
        };
        
        // Act
        var code = generator.GenerateComInterface(interfaceDef);
        
        // Assert
        Assert.Contains("interface ITestInterface", code);
        Assert.Contains("[ComImport]", code);
        Assert.Contains("[Guid(\"", code);
        Assert.Contains("IUnknown", code);
    }
    
    [Fact]
    public void GenerateComInterface_ShouldGenerateVTableWrapper()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var interfaceDef = new ComInterfaceDefinition
        {
            Name = "ITestInterface",
            Methods = new List<ApiSignature>
            {
                new() 
                { 
                    Name = "TestMethod",
                    ReturnType = "HRESULT",
                    Parameters = new List<ApiParameter>
                    {
                        new() { Name = "param1", Type = "DWORD" }
                    }
                }
            }
        };
        
        // Act
        var code = generator.GenerateComInterface(interfaceDef);
        
        // Assert
        Assert.Contains("ITestInterfaceVTable", code);
        Assert.Contains("vtable", code.ToLower());
        Assert.Contains("TestMethod", code);
    }
    
    [Fact]
    public void GenerateComInterface_WithNoMethods_ShouldGenerateEmptyInterface()
    {
        // Arrange
        var generator = new MarshallingCodeGenerator();
        var interfaceDef = new ComInterfaceDefinition
        {
            Name = "IEmptyInterface"
        };
        
        // Act
        var code = generator.GenerateComInterface(interfaceDef);
        
        // Assert
        Assert.Contains("interface IEmptyInterface", code);
        Assert.Contains("No methods defined", code);
    }
}
