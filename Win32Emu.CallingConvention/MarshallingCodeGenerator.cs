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
}
