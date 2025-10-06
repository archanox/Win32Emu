using System.Xml.Linq;

namespace Win32Emu.CodeGen.ApiMetadata;

/// <summary>
/// Parser for API Monitor XML definition files
/// See: https://github.com/jozefizso/apimonitor
/// </summary>
public class XmlParser
{
    /// <summary>
    /// Parse an API Monitor XML file
    /// </summary>
    /// <param name="xmlPath">Path to the XML file (e.g., Kernel32.xml)</param>
    /// <returns>List of API definitions</returns>
    public static List<ApiDefinition> ParseApiMonitorXml(string xmlPath)
    {
        var apis = new List<ApiDefinition>();
        
        if (!File.Exists(xmlPath))
        {
            Console.WriteLine($"Warning: XML file not found: {xmlPath}");
            return apis;
        }
        
        try
        {
            var doc = XDocument.Load(xmlPath);
            var apiElements = doc.Descendants("Api");
            
            foreach (var apiElement in apiElements)
            {
                var name = apiElement.Attribute("Name")?.Value;
                if (string.IsNullOrEmpty(name))
                    continue;
                
                // Parse parameters
                var parameters = new List<ApiParameter>();
                var paramElements = apiElement.Descendants("Param");
                
                foreach (var paramElement in paramElements)
                {
                    var paramType = paramElement.Attribute("Type")?.Value ?? "DWORD";
                    var paramName = paramElement.Attribute("Name")?.Value ?? "param";
                    
                    parameters.Add(new ApiParameter(paramName, paramType));
                }
                
                // Calculate expected arg bytes (stdcall convention)
                int argBytes = CalculateArgBytes(parameters);
                
                var returnType = apiElement.Attribute("Return")?.Value ?? "DWORD";
                
                apis.Add(new ApiDefinition(name, returnType, parameters, argBytes));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing XML {Path.GetFileName(xmlPath)}: {ex.Message}");
        }
        
        return apis;
    }
    
    /// <summary>
    /// Parse all XML files in a directory
    /// </summary>
    /// <param name="directoryPath">Path to directory containing API Monitor XML files</param>
    /// <returns>Dictionary mapping module name to API definitions</returns>
    public static Dictionary<string, List<ApiDefinition>> ParseDirectory(string directoryPath)
    {
        var results = new Dictionary<string, List<ApiDefinition>>(StringComparer.OrdinalIgnoreCase);
        
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Warning: Directory not found: {directoryPath}");
            return results;
        }
        
        var xmlFiles = Directory.GetFiles(directoryPath, "*.xml", SearchOption.TopDirectoryOnly);
        
        foreach (var xmlPath in xmlFiles)
        {
            var moduleName = Path.GetFileNameWithoutExtension(xmlPath);
            var apis = ParseApiMonitorXml(xmlPath);
            results[moduleName] = apis;
            
            Console.WriteLine($"Parsed {moduleName}.xml: {apis.Count} API definitions");
        }
        
        return results;
    }
    
    /// <summary>
    /// Calculate argument bytes for stdcall convention
    /// All parameters are 4-byte aligned on x86
    /// </summary>
    private static int CalculateArgBytes(List<ApiParameter> parameters)
    {
        int total = 0;
        foreach (var param in parameters)
        {
            // In x86 stdcall, all parameters are pushed as 4-byte values
            // Even smaller types like BYTE or WORD are pushed as 4 bytes
            // 64-bit types like LONGLONG take 8 bytes
            total += GetTypeSize(param.Type);
        }
        return total;
    }
    
    /// <summary>
    /// Get the size in bytes for a Win32 type in stdcall convention
    /// </summary>
    private static int GetTypeSize(string type)
    {
        // Remove pointer/const modifiers
        var cleanType = type.Replace("*", "").Replace("const", "").Trim();
        
        return cleanType.ToUpperInvariant() switch
        {
            // 64-bit types
            "LONGLONG" or "ULONGLONG" or "INT64" or "UINT64" or "__INT64" or "LARGE_INTEGER" => 8,
            "DOUBLE" => 8,
            
            // Everything else is 4 bytes in x86 stdcall
            // This includes: DWORD, UINT, INT, LONG, ULONG, HANDLE, HWND, LPSTR, LPVOID, etc.
            _ => 4
        };
    }
}

/// <summary>
/// Represents an API definition from XML
/// </summary>
/// <param name="Name">Function name</param>
/// <param name="ReturnType">Return type (e.g., "BOOL", "DWORD")</param>
/// <param name="Parameters">List of parameters</param>
/// <param name="ArgBytes">Calculated argument bytes for stdcall</param>
public record ApiDefinition(string Name, string ReturnType, List<ApiParameter> Parameters, int ArgBytes);

/// <summary>
/// Represents a parameter in an API definition
/// </summary>
/// <param name="Name">Parameter name</param>
/// <param name="Type">Parameter type (e.g., "DWORD", "LPSTR")</param>
public record ApiParameter(string Name, string Type);
