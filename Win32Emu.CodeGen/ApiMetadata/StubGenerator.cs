using System.Text;
using System.Text.RegularExpressions;

namespace Win32Emu.CodeGen.ApiMetadata;

/// <summary>
/// Generates C# stub implementations for missing Win32 APIs
/// </summary>
public class StubGenerator
{
    /// <summary>
    /// Generate stub methods for APIs in a DLL, grouping by function name across versions
    /// </summary>
    /// <param name="dllName">Name of the DLL (e.g., "KERNEL32.DLL")</param>
    /// <param name="allExports">List of all exports (can include multiple versions)</param>
    /// <param name="xmlDefinitions">Optional API definitions from XML (for better signatures)</param>
    /// <returns>Generated C# code</returns>
    public static string GenerateStubs(string dllName,
	    List<ExportedFunction> allExports,
	    Dictionary<string, ApiDefinition>? xmlDefinitions = null)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("// Auto-generated stubs for APIs");
        sb.AppendLine($"// DLL: {dllName}");
        sb.AppendLine($"// Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        
        // Group exports by function name to handle multiple versions
        var groupedExports = allExports
            .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Min(e => e.Ordinal));
        
        foreach (var group in groupedExports)
        {
            var stub = GenerateStubMethod(dllName, group.ToList(), xmlDefinitions?.GetValueOrDefault(group.Key));
            sb.AppendLine(stub);
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Generate a stub method for a single API (potentially with multiple versions)
    /// </summary>
    private static string GenerateStubMethod(string dllName, List<ExportedFunction> exports, ApiDefinition? definition)
    {
        var sb = new StringBuilder();
        
        // Use the first export as the primary one
        var primaryExport = exports.First();
        
        // Generate multiple [DllModuleExport] attributes for different versions
        foreach (var export in exports.OrderBy(e => e.Version))
        {
            sb.Append($"[DllModuleExport({export.Ordinal}");
            
            if (export.EntryPoint.HasValue)
            {
                sb.Append($", entryPoint: 0x{export.EntryPoint.Value:X8}");
            }
            
            if (!string.IsNullOrEmpty(export.Version))
            {
                sb.Append($", Version = \"{export.Version}\"");
            }
            
            if (export.ForwardedTo != null)
            {
                sb.Append($", ForwardedTo = \"{export.ForwardedTo}\"");
            }
            
            // Check if the export name is not a valid C# identifier
            string csharpMethodName = MakeCSharpMethodName(export.Name);
            if (csharpMethodName != export.Name)
            {
                sb.Append($", ExportName = \"{export.Name}\"");
            }
            
            sb.Append(", IsStub = true)]");
            sb.AppendLine();
        }
        
        // Generate method signature
        string methodName = MakeCSharpMethodName(primaryExport.Name);
        sb.Append("public uint ");
        sb.Append(methodName);
        sb.Append("(");
        
        // Add parameters if we have definition
        var paramStrings = new List<string>();
        if (definition != null && definition.Parameters.Count > 0)
        {
            for (int i = 0; i < definition.Parameters.Count; i++)
            {
                var param = definition.Parameters[i];
                var csharpType = MapWin32TypeToCSharp(param.Type);
                paramStrings.Add($"{csharpType} {param.Name}");
            }
        }
        sb.Append(string.Join(", ", paramStrings));
        
        sb.AppendLine(")");
        sb.AppendLine("{");
        
        // Add logging with parameters
        string moduleName = Path.GetFileNameWithoutExtension(dllName);
        if (definition != null && definition.Parameters.Count > 0)
        {
            // Build format string and arguments for logging
            var logParams = new List<string>();
            var logArgs = new List<string>();
            
            for (int i = 0; i < definition.Parameters.Count; i++)
            {
                var param = definition.Parameters[i];
                var paramType = param.Type.ToUpperInvariant();
                
                // Determine the logging format based on parameter type
                if (paramType.Contains("HANDLE") || paramType.Contains("HWND") || 
                    paramType.Contains("HDC") || paramType.Contains("HMODULE") ||
                    paramType.Contains("PTR") || paramType.EndsWith("*"))
                {
                    logParams.Add($"{param.Name}=0x{{{param.Name}:X8}}");
                }
                else if (paramType.Contains("FLAGS") || paramType.StartsWith("DW"))
                {
                    logParams.Add($"{param.Name}=0x{{{param.Name}:X8}}");
                }
                else
                {
                    logParams.Add($"{param.Name}={{{param.Name}}}");
                }
                
                logArgs.Add(param.Name);
            }
            
            string logMessage = $"[{moduleName}] {methodName}: {string.Join(", ", logParams)}";
            string argsString = string.Join(", ", logArgs);
            sb.AppendLine($"    _logger.LogWarning(\"{logMessage}\", {argsString});");
        }
        else
        {
            sb.AppendLine($"    _logger.LogWarning(\"[{moduleName}] {methodName} called (stub)\");");
        }
        
        // Add TODO comment
        sb.AppendLine($"    // TODO: Implement {primaryExport.Name}");
        
        // Return default value
        var returnType = definition?.ReturnType ?? "DWORD";
        var defaultReturn = GetDefaultReturnValue(returnType);
        sb.AppendLine($"    return {defaultReturn}; // {returnType} default");
        
        sb.Append("}");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Convert a DLL export name to a valid C# method name
    /// </summary>
    private static string MakeCSharpMethodName(string exportName)
    {
        // Remove decorations like @4, @8, etc. (stdcall decorations)
        string name = Regex.Replace(exportName, @"@\d+$", "");
        
        // Remove leading underscores
        name = name.TrimStart('_');
        
        // Replace any remaining invalid characters with underscores
        name = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
        
        // Ensure it doesn't start with a digit
        if (char.IsDigit(name[0]))
        {
            name = "_" + name;
        }
        
        return name;
    }
    
    /// <summary>
    /// Map Win32 type to C# type for method signatures
    /// </summary>
    private static string MapWin32TypeToCSharp(string win32Type)
    {
        // Remove const and pointer markers for analysis
        var cleanType = win32Type.Replace("const", "").Trim();
        var isPointer = cleanType.Contains('*');
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
    /// Generate a complete module class with stubs for all APIs
    /// </summary>
    /// <param name="moduleName">Module name (e.g., "Advapi32Module")</param>
    /// <param name="dllName">DLL name (e.g., "ADVAPI32.DLL")</param>
    /// <param name="allExports">List of all exports (can include multiple versions)</param>
    /// <param name="xmlDefinitions">Optional API definitions from XML</param>
    /// <returns>Complete C# class file</returns>
    public static string GenerateModuleClass(string moduleName, string dllName, List<ExportedFunction> allExports, Dictionary<string, ApiDefinition>? xmlDefinitions = null)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("using Win32Emu.Cpu;");
        sb.AppendLine("using Win32Emu.Memory;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using Microsoft.Extensions.Logging.Abstractions;");
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
        sb.AppendLine("    private readonly ILogger _logger;");
        sb.AppendLine();
        sb.AppendLine($"    public {moduleName}(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)");
        sb.AppendLine("        : base(env, imageBase, peLoader)");
        sb.AppendLine("    {");
        sb.AppendLine("        _logger = logger ?? NullLogger.Instance;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    public override string Name => \"{dllName.ToUpperInvariant()}\";");
        sb.AppendLine();
        
        // Group exports by function name to handle multiple versions
        var groupedExports = allExports
            .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Min(e => e.Ordinal));
        
        // Generate all stub methods
        foreach (var group in groupedExports)
        {
            var stub = GenerateStubMethod(dllName, group.ToList(), xmlDefinitions?.GetValueOrDefault(group.Key));
            
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
