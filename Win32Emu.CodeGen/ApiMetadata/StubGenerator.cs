using System.Text;

namespace Win32Emu.CodeGen.ApiMetadata;

/// <summary>
/// Generates C# stub implementations for missing Win32 APIs
/// </summary>
public class StubGenerator
{
    /// <summary>
    /// Generate stub methods for missing APIs in a DLL
    /// </summary>
    /// <param name="dllName">Name of the DLL (e.g., "KERNEL32.DLL")</param>
    /// <param name="missingApis">List of missing API names</param>
    /// <param name="xmlDefinitions">Optional API definitions from XML (for better signatures)</param>
    /// <returns>Generated C# code</returns>
    public static string GenerateStubs(string dllName, List<string> missingApis, Dictionary<string, ApiDefinition>? xmlDefinitions = null)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("// Auto-generated stubs for missing APIs");
        sb.AppendLine($"// DLL: {dllName}");
        sb.AppendLine($"// Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        
        foreach (var apiName in missingApis.OrderBy(a => a))
        {
            var stub = GenerateStubMethod(apiName, xmlDefinitions?.GetValueOrDefault(apiName));
            sb.AppendLine(stub);
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate a stub method for a single API
    /// </summary>
    private static string GenerateStubMethod(string apiName, ApiDefinition? definition)
    {
        var sb = new StringBuilder();
        
        // Generate method signature
        sb.Append("[DllModuleExport]");
        sb.AppendLine();
        sb.Append("public uint ");
        sb.Append(apiName);
        sb.Append("(");
        
        // Add parameters if we have definition
        if (definition != null && definition.Parameters.Count > 0)
        {
            var paramStrings = new List<string>();
            for (int i = 0; i < definition.Parameters.Count; i++)
            {
                var param = definition.Parameters[i];
                var csharpType = MapWin32TypeToCSharp(param.Type);
                paramStrings.Add($"{csharpType} {param.Name}");
            }
            sb.Append(string.Join(", ", paramStrings));
        }
        
        sb.AppendLine(")");
        sb.AppendLine("{");
        
        // Add logging
        if (definition != null && definition.Parameters.Count > 0)
        {
            var paramNames = string.Join(", ", definition.Parameters.Select(p => $"{p.Name}={{" + p.Name + "}}"));
            sb.AppendLine($"    Diagnostics.Diagnostics.LogWarn($\"Stub called: {apiName}({paramNames})\");");
        }
        else
        {
            sb.AppendLine($"    Diagnostics.Diagnostics.LogWarn(\"Stub called: {apiName}()\");");
        }
        
        // Add TODO comment
        sb.AppendLine($"    // TODO: Implement {apiName}");
        
        // Return default value
        var returnType = definition?.ReturnType ?? "DWORD";
        var defaultReturn = GetDefaultReturnValue(returnType);
        sb.AppendLine($"    return {defaultReturn}; // {returnType} default");
        
        sb.Append("}");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Map Win32 type to C# type for method signatures
    /// </summary>
    private static string MapWin32TypeToCSharp(string win32Type)
    {
        // Remove const and pointer markers for analysis
        var cleanType = win32Type.Replace("const", "").Trim();
        var isPointer = cleanType.Contains("*");
        cleanType = cleanType.Replace("*", "").Trim();
        
        // If it's a pointer type, return uint* (generic pointer in our emulator)
        if (isPointer)
        {
            return "uint"; // We use uint for all pointers in the emulator
        }
        
        return cleanType.ToUpperInvariant() switch
        {
            // Exact matches
            "VOID" => "void",
            "BOOL" => "uint",
            "BYTE" => "uint",
            "WORD" => "uint",
            "DWORD" => "uint",
            "INT" => "uint",
            "UINT" => "uint",
            "LONG" => "uint",
            "ULONG" => "uint",
            "SHORT" => "uint",
            "USHORT" => "uint",
            
            // Handles and pointers (all are uint in our emulator)
            "HANDLE" => "uint",
            "HWND" => "uint",
            "HDC" => "uint",
            "HINSTANCE" => "uint",
            "HMODULE" => "uint",
            "HGDIOBJ" => "uint",
            "HBITMAP" => "uint",
            "HICON" => "uint",
            "HCURSOR" => "uint",
            "HMENU" => "uint",
            "HBRUSH" => "uint",
            "HPEN" => "uint",
            "HFONT" => "uint",
            
            // 64-bit types
            "LONGLONG" => "ulong",
            "ULONGLONG" => "ulong",
            "INT64" => "ulong",
            "UINT64" => "ulong",
            "__INT64" => "ulong",
            "LARGE_INTEGER" => "ulong",
            
            // Default to uint for unknown types
            _ => "uint"
        };
    }
    
    /// <summary>
    /// Get default return value for a Win32 type
    /// </summary>
    private static string GetDefaultReturnValue(string returnType)
    {
        var cleanType = returnType.Replace("*", "").Replace("const", "").Trim();
        
        return cleanType.ToUpperInvariant() switch
        {
            "VOID" => "0",
            "BOOL" => "0", // FALSE
            "LONGLONG" or "ULONGLONG" or "INT64" or "UINT64" or "__INT64" or "LARGE_INTEGER" => "0UL",
            _ => "0"
        };
    }
    
    /// <summary>
    /// Generate a complete module class with stubs for all missing APIs
    /// </summary>
    /// <param name="moduleName">Module name (e.g., "Advapi32Module")</param>
    /// <param name="dllName">DLL name (e.g., "ADVAPI32.DLL")</param>
    /// <param name="missingApis">List of missing API names</param>
    /// <param name="xmlDefinitions">Optional API definitions from XML</param>
    /// <returns>Complete C# class file</returns>
    public static string GenerateModuleClass(string moduleName, string dllName, List<string> missingApis, Dictionary<string, ApiDefinition>? xmlDefinitions = null)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("using Win32Emu.Cpu;");
        sb.AppendLine("using Win32Emu.Memory;");
        sb.AppendLine();
        sb.AppendLine("namespace Win32Emu.Win32.Modules;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {dllName} module implementation");
        sb.AppendLine("/// Auto-generated stub methods");
        sb.AppendLine($"/// Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {moduleName} : BaseModule");
        sb.AppendLine("{");
        sb.AppendLine($"    public override string Name => \"{dllName}\";");
        sb.AppendLine();
        
        // Generate all stub methods
        foreach (var apiName in missingApis.OrderBy(a => a))
        {
            var stub = GenerateStubMethod(apiName, xmlDefinitions?.GetValueOrDefault(apiName));
            
            // Indent the method
            var lines = stub.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.Append("    ");
                }
                sb.AppendLine(line.TrimEnd());
            }
            sb.AppendLine();
        }
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }
}
