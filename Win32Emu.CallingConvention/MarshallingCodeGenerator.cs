using System.Text;

namespace Win32Emu.CallingConvention;

/// <summary>
/// Generates marshalling code for Win32 API wrappers based on Reko XML definitions.
/// Reduces boilerplate by auto-generating parameter extraction and type conversion.
/// </summary>
public class MarshallingCodeGenerator
{
    private readonly Dictionary<string, string> _typeMapping = new()
    {
        // Windows types to C# types for internal use
        ["BOOL"] = "uint",
        ["DWORD"] = "uint",
        ["UINT"] = "uint",
        ["INT"] = "int",
        ["WORD"] = "ushort",
        ["BYTE"] = "byte",
        ["LONG"] = "int",
        ["HANDLE"] = "uint",
        ["HWND"] = "uint",
        ["HINSTANCE"] = "uint",
        ["HMODULE"] = "uint",
        ["HDC"] = "uint",
        ["HBITMAP"] = "uint",
        ["HICON"] = "uint",
        ["HCURSOR"] = "uint",
        ["HMENU"] = "uint",
        ["HBRUSH"] = "uint",
        ["HPEN"] = "uint",
        ["HFONT"] = "uint",
        ["HRGN"] = "uint",
        ["LPVOID"] = "uint",
        ["LPCVOID"] = "uint",
        ["PVOID"] = "uint",
        ["LPSTR"] = "uint",
        ["LPCSTR"] = "uint",
        ["LPWSTR"] = "uint",
        ["LPCWSTR"] = "uint",
        ["SIZE_T"] = "uint",
    };
    
    /// <summary>
    /// Generate a complete wrapper method for a Win32 API.
    /// </summary>
    public string GenerateWrapper(ApiSignature signature)
    {
        var sb = new StringBuilder();
        
        // Generate method summary
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// {signature.Name} - Auto-generated wrapper from {signature.DllName}");
        sb.AppendLine($"        /// Calling convention: {signature.CallingConvention}");
        sb.AppendLine($"        /// </summary>");
        
        // Generate method signature
        var returnType = MapType(signature.ReturnType);
        sb.Append($"        public {returnType} {signature.Name}(");
        
        var paramList = new List<string>();
        for (int i = 0; i < signature.Parameters.Count; i++)
        {
            var param = signature.Parameters[i];
            var paramType = MapType(param.Type);
            paramList.Add($"{paramType} {param.Name}");
        }
        sb.AppendLine(string.Join(", ", paramList) + ")");
        sb.AppendLine("        {");
        
        // Generate parameter extraction based on calling convention
        sb.AppendLine(GenerateParameterExtraction(signature));
        
        // Generate implementation stub
        sb.AppendLine($"            _logger.LogWarning(\"{signature.Name} called but not fully implemented\");");
        sb.AppendLine();
        sb.AppendLine("            // TODO: Implement actual API logic");
        
        // Generate return statement
        if (returnType != "void")
        {
            var defaultReturn = GetDefaultReturnValue(signature.ReturnType);
            sb.AppendLine($"            return {defaultReturn};");
        }
        
        sb.AppendLine("        }");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate parameter extraction code based on calling convention.
    /// </summary>
    private string GenerateParameterExtraction(ApiSignature signature)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("            // Parameter extraction and validation");
        
        foreach (var param in signature.Parameters)
        {
            if (param.IsStringPointer)
            {
                // Generate string extraction
                var encoding = param.Type.Contains("W") ? "Unicode" : "ANSI";
                sb.AppendLine($"            // {param.Name}: {param.Type} ({encoding} string pointer)");
                
                if (param.Type.Contains("W"))
                {
                    sb.AppendLine($"            var {param.Name}Str = _memory?.ReadWString({param.Name}) ?? string.Empty;");
                }
                else
                {
                    sb.AppendLine($"            var {param.Name}Str = _memory?.ReadCString({param.Name}) ?? string.Empty;");
                }
            }
            else if (param.IsPointer)
            {
                // Generate pointer validation
                sb.AppendLine($"            // {param.Name}: {param.Type} (pointer, address=0x{{{param.Name}:X8}})");
            }
            else
            {
                // Generate value parameter logging
                sb.AppendLine($"            // {param.Name}: {param.Type} (value={{{param.Name}}})");
            }
        }
        
        sb.AppendLine();
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate a parameter reader helper based on calling convention.
    /// </summary>
    public string GenerateParameterReader(ApiSignature signature)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"        // Auto-generated parameter reader for {signature.Name}");
        sb.AppendLine($"        // Convention: {signature.CallingConvention}");
        
        switch (signature.CallingConvention)
        {
            case Win32CallingConvention.Stdcall:
                sb.AppendLine("        // Stdcall: All parameters on stack, right-to-left push");
                for (int i = 0; i < signature.Parameters.Count; i++)
                {
                    var param = signature.Parameters[i];
                    var accessor = GetStackAccessor(param);
                    sb.AppendLine($"        var {param.Name} = a.{accessor}({i}); // {param.Type}");
                }
                break;
                
            case Win32CallingConvention.Fastcall:
                sb.AppendLine("        // Fastcall: First two params in ECX/EDX, rest on stack");
                int stackIndex = 0;
                for (int i = 0; i < signature.Parameters.Count; i++)
                {
                    var param = signature.Parameters[i];
                    if (i == 0 && param.RegisterName?.ToLower() == "ecx")
                    {
                        sb.AppendLine($"        var {param.Name} = cpu.GetRegister(\"ECX\"); // {param.Type} (register)");
                    }
                    else if (i == 1 && param.RegisterName?.ToLower() == "edx")
                    {
                        sb.AppendLine($"        var {param.Name} = cpu.GetRegister(\"EDX\"); // {param.Type} (register)");
                    }
                    else
                    {
                        var accessor = GetStackAccessor(param);
                        sb.AppendLine($"        var {param.Name} = a.{accessor}({stackIndex}); // {param.Type} (stack)");
                        stackIndex++;
                    }
                }
                break;
                
            case Win32CallingConvention.Thiscall:
                sb.AppendLine("        // Thiscall: 'this' pointer in ECX, rest on stack");
                for (int i = 0; i < signature.Parameters.Count; i++)
                {
                    var param = signature.Parameters[i];
                    if (i == 0)
                    {
                        sb.AppendLine($"        var {param.Name} = cpu.GetRegister(\"ECX\"); // {param.Type} (this pointer in ECX)");
                    }
                    else
                    {
                        var accessor = GetStackAccessor(param);
                        sb.AppendLine($"        var {param.Name} = a.{accessor}({i - 1}); // {param.Type}");
                    }
                }
                break;
        }
        
        return sb.ToString();
    }
    
    private string GetStackAccessor(ApiParameter param)
    {
        if (param.IsStringPointer)
        {
            return "Lpstr";
        }
        else if (param.Type == "INT")
        {
            return "Int32";
        }
        else
        {
            // Default to UInt32 for most Win32 types
            return "UInt32";
        }
    }
    
    private string MapType(string winType)
    {
        if (_typeMapping.TryGetValue(winType, out var csharpType))
        {
            return csharpType;
        }
        
        // Default to uint for unknown types (conservative approach)
        return "uint";
    }
    
    private string GetDefaultReturnValue(string returnType)
    {
        return returnType switch
        {
            "BOOL" => "0 // FALSE",
            "HANDLE" or "HWND" or "HDC" or "HINSTANCE" or "HMODULE" => "0 // NULL handle",
            "LPVOID" or "LPSTR" or "LPCSTR" => "0 // NULL pointer",
            "void" => "",
            _ => "0"
        };
    }
    
    /// <summary>
    /// Generate C# struct definition from parsed struct metadata.
    /// </summary>
    public string GenerateStructDefinition(StructDefinition structDef)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// {structDef.Name} - Auto-generated from XML definition");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    [StructLayout(LayoutKind.Sequential)]");
        sb.AppendLine($"    public struct {structDef.Name}");
        sb.AppendLine("    {");
        
        foreach (var field in structDef.Fields)
        {
            var csharpType = MapType(field.Type);
            sb.AppendLine($"        public {csharpType} {field.Name};");
        }
        
        sb.AppendLine("    }");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate C# delegate definition for callbacks.
    /// </summary>
    public string GenerateCallbackDelegate(CallbackDefinition callback)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// {callback.Name} - Auto-generated callback delegate");
        sb.AppendLine($"    /// </summary>");
        
        var returnType = callback.ReturnType != null ? MapType(callback.ReturnType) : "uint";
        sb.Append($"    public delegate {returnType} {callback.Name}(");
        
        var paramList = new List<string>();
        foreach (var param in callback.Parameters)
        {
            var paramType = MapType(param.Type);
            paramList.Add($"{paramType} {param.Name}");
        }
        
        sb.Append(string.Join(", ", paramList));
        sb.AppendLine(");");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate C# type alias or enum.
    /// </summary>
    public string GenerateTypeAlias(TypeAlias alias)
    {
        var sb = new StringBuilder();
        
        if (alias.EnumValues != null && alias.EnumValues.Count > 0)
        {
            // Generate enum
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {alias.Name} - Auto-generated enum");
            sb.AppendLine($"    /// </summary>");
            
            var baseEnumType = MapType(alias.BaseType);
            sb.AppendLine($"    public enum {alias.Name} : {baseEnumType}");
            sb.AppendLine("    {");
            
            foreach (var (name, value) in alias.EnumValues)
            {
                sb.AppendLine($"        {name} = {value},");
            }
            
            sb.AppendLine("    }");
        }
        else
        {
            // Generate using directive or type alias
            var csharpType = MapType(alias.BaseType);
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {alias.Name} - Alias for {alias.BaseType}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    using {alias.Name} = {csharpType};");
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate validation code to check API signature against XML definition.
    /// </summary>
    public string GenerateValidationReport(ApiSignature signature, ApiSignature? actualSignature)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"Validation Report for {signature.Name}");
        sb.AppendLine("=".PadRight(50, '='));
        
        if (actualSignature == null)
        {
            sb.AppendLine("Status: NOT IMPLEMENTED");
            return sb.ToString();
        }
        
        var issues = new List<string>();
        
        // Check parameter count
        if (signature.Parameters.Count != actualSignature.Parameters.Count)
        {
            issues.Add($"Parameter count mismatch: Expected {signature.Parameters.Count}, got {actualSignature.Parameters.Count}");
        }
        
        // Check return type
        if (signature.ReturnType != actualSignature.ReturnType)
        {
            issues.Add($"Return type mismatch: Expected {signature.ReturnType}, got {actualSignature.ReturnType}");
        }
        
        // Check parameter types
        for (int i = 0; i < Math.Min(signature.Parameters.Count, actualSignature.Parameters.Count); i++)
        {
            var expected = signature.Parameters[i];
            var actual = actualSignature.Parameters[i];
            
            if (expected.Type != actual.Type)
            {
                issues.Add($"Parameter {i} ({expected.Name}) type mismatch: Expected {expected.Type}, got {actual.Type}");
            }
        }
        
        if (issues.Count == 0)
        {
            sb.AppendLine("Status: VALID ✓");
        }
        else
        {
            sb.AppendLine("Status: INVALID ✗");
            sb.AppendLine("\nIssues:");
            foreach (var issue in issues)
            {
                sb.AppendLine($"  - {issue}");
            }
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate XML documentation comments from API signature.
    /// </summary>
    public string GenerateDocumentation(ApiSignature signature, string? category = null)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("        /// <summary>");
        sb.AppendLine($"        /// {signature.Name}");
        if (!string.IsNullOrEmpty(category))
        {
            sb.AppendLine($"        /// Category: {category}");
        }
        sb.AppendLine($"        /// </summary>");
        
        foreach (var param in signature.Parameters)
        {
            sb.AppendLine($"        /// <param name=\"{param.Name}\">{param.Type}</param>");
        }
        
        sb.AppendLine($"        /// <returns>{signature.ReturnType}</returns>");
        sb.AppendLine($"        /// <remarks>");
        sb.AppendLine($"        /// DLL: {signature.DllName}");
        sb.AppendLine($"        /// Calling Convention: {signature.CallingConvention}");
        sb.AppendLine($"        /// </remarks>");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate unit test template for an API.
    /// </summary>
    public string GenerateUnitTest(ApiSignature signature)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"    [Fact]");
        sb.AppendLine($"    public void Test_{signature.Name}_Basic()");
        sb.AppendLine("    {");
        sb.AppendLine("        // Arrange");
        sb.AppendLine("        var module = CreateModule();");
        
        // Generate mock parameter values
        foreach (var param in signature.Parameters)
        {
            var mockValue = GetMockValue(param);
            sb.AppendLine($"        var {param.Name} = {mockValue};");
        }
        
        sb.AppendLine();
        sb.AppendLine("        // Act");
        sb.Append($"        var result = module.{signature.Name}(");
        sb.Append(string.Join(", ", signature.Parameters.Select(p => p.Name)));
        sb.AppendLine(");");
        
        sb.AppendLine();
        sb.AppendLine("        // Assert");
        sb.AppendLine("        // TODO: Add assertions");
        sb.AppendLine($"        Assert.NotNull(result);");
        sb.AppendLine("    }");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate C# COM interface definition with vtable dispatch.
    /// </summary>
    public string GenerateComInterface(ComInterfaceDefinition interfaceDef)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// {interfaceDef.Name} - Auto-generated COM interface");
        if (!string.IsNullOrEmpty(interfaceDef.Guid))
        {
            sb.AppendLine($"    /// GUID: {interfaceDef.Guid}");
        }
        sb.AppendLine($"    /// </summary>");
        
        if (!string.IsNullOrEmpty(interfaceDef.Guid))
        {
            sb.AppendLine($"    [Guid(\"{interfaceDef.Guid}\")]");
        }
        sb.AppendLine($"    [ComImport]");
        sb.AppendLine($"    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]");
        
        var baseInterface = !string.IsNullOrEmpty(interfaceDef.BaseInterface) 
            ? interfaceDef.BaseInterface 
            : "IUnknown";
        
        sb.AppendLine($"    public interface {interfaceDef.Name} : {baseInterface}");
        sb.AppendLine("    {");
        
        // Generate method declarations
        if (interfaceDef.Methods.Count > 0)
        {
            foreach (var method in interfaceDef.Methods)
            {
                sb.AppendLine($"        /// <summary>");
                sb.AppendLine($"        /// {method.Name}");
                sb.AppendLine($"        /// </summary>");
                
                var returnType = MapType(method.ReturnType);
                sb.Append($"        {returnType} {method.Name}(");
                
                var paramList = new List<string>();
                foreach (var param in method.Parameters)
                {
                    var paramType = MapType(param.Type);
                    paramList.Add($"{paramType} {param.Name}");
                }
                
                sb.Append(string.Join(", ", paramList));
                sb.AppendLine(");");
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("        // No methods defined - extend manually");
            sb.AppendLine("        // Add interface methods here based on COM specification");
        }
        
        sb.AppendLine("    }");
        sb.AppendLine();
        
        // Generate vtable wrapper helper
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Vtable dispatch helper for {interfaceDef.Name}");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public class {interfaceDef.Name}VTable");
        sb.AppendLine("    {");
        sb.AppendLine("        private readonly uint _thisPtr;");
        sb.AppendLine("        private readonly uint _vtablePtr;");
        sb.AppendLine();
        sb.AppendLine($"        public {interfaceDef.Name}VTable(uint thisPtr, uint vtablePtr)");
        sb.AppendLine("        {");
        sb.AppendLine("            _thisPtr = thisPtr;");
        sb.AppendLine("            _vtablePtr = vtablePtr;");
        sb.AppendLine("        }");
        sb.AppendLine();
        
        // Generate vtable dispatch methods
        if (interfaceDef.Methods.Count > 0)
        {
            int methodIndex = 0;
            foreach (var method in interfaceDef.Methods)
            {
                sb.AppendLine($"        /// <summary>");
                sb.AppendLine($"        /// Dispatch {method.Name} via vtable[{methodIndex}]");
                sb.AppendLine($"        /// </summary>");
                
                var returnType = MapType(method.ReturnType);
                sb.Append($"        public {returnType} {method.Name}(");
                
                var paramList = new List<string>();
                foreach (var param in method.Parameters)
                {
                    var paramType = MapType(param.Type);
                    paramList.Add($"{paramType} {param.Name}");
                }
                
                sb.Append(string.Join(", ", paramList));
                sb.AppendLine(")");
                sb.AppendLine("        {");
                sb.AppendLine($"            // TODO: Read function pointer from vtable[{methodIndex}]");
                sb.AppendLine($"            // TODO: Call function with _thisPtr as first parameter");
                sb.AppendLine($"            throw new NotImplementedException(\"Vtable dispatch for {method.Name} not implemented\");");
                sb.AppendLine("        }");
                sb.AppendLine();
                
                methodIndex++;
            }
        }
        
        sb.AppendLine("    }");
        
        return sb.ToString();
    }
    
    private string GetMockValue(ApiParameter param)
    {
        if (param.IsStringPointer)
        {
            return "0x1000u // Mock string pointer";
        }
        else if (param.IsPointer)
        {
            return "0x2000u // Mock pointer";
        }
        else if (param.Type == "BOOL")
        {
            return "1u // TRUE";
        }
        else if (param.Type.StartsWith("H")) // Handles
        {
            return "0x3000u // Mock handle";
        }
        else
        {
            return "0u";
        }
    }
}
