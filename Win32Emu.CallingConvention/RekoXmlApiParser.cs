using System.Xml.Linq;

namespace Win32Emu.CallingConvention;

/// <summary>
/// Represents a Win32 API parameter parsed from Reko XML definitions.
/// </summary>
public class ApiParameter
{
    /// <summary>
    /// Parameter name (e.g., "lpFileName", "dwDesiredAccess")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Parameter type (e.g., "LPCSTR", "DWORD", "HANDLE")
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether parameter is passed via stack
    /// </summary>
    public bool IsStackParameter { get; set; }
    
    /// <summary>
    /// Stack offset size in bytes (typically 4 for 32-bit)
    /// </summary>
    public int StackSize { get; set; }
    
    /// <summary>
    /// Register name if parameter is passed in register (e.g., "ecx", "edx")
    /// </summary>
    public string? RegisterName { get; set; }
    
    /// <summary>
    /// Whether this is a pointer type (LPSTR, LPCSTR, LPVOID, etc.)
    /// </summary>
    public bool IsPointer => Type.StartsWith("LP") || Type.EndsWith("*") || Type == "PVOID";
    
    /// <summary>
    /// Whether this is a string pointer type (LPSTR, LPCSTR, LPWSTR, LPCWSTR)
    /// </summary>
    public bool IsStringPointer => Type is "LPSTR" or "LPCSTR" or "LPWSTR" or "LPCWSTR";
}

/// <summary>
/// Represents a Win32 API signature parsed from Reko XML definitions.
/// </summary>
public class ApiSignature
{
    /// <summary>
    /// API name (e.g., "CreateFileA", "MessageBoxA")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// DLL name (e.g., "kernel32.dll", "user32.dll")
    /// </summary>
    public string DllName { get; set; } = string.Empty;
    
    /// <summary>
    /// Return type (e.g., "HANDLE", "BOOL", "DWORD")
    /// </summary>
    public string ReturnType { get; set; } = string.Empty;
    
    /// <summary>
    /// Return register (typically "eax" for 32-bit)
    /// </summary>
    public string ReturnRegister { get; set; } = "eax";
    
    /// <summary>
    /// List of parameters in order
    /// </summary>
    public List<ApiParameter> Parameters { get; set; } = new();
    
    /// <summary>
    /// Calling convention (inferred from parameter passing)
    /// </summary>
    public Win32CallingConvention CallingConvention { get; set; } = Win32CallingConvention.Stdcall;
}

/// <summary>
/// Parser for Reko XML API definition files.
/// Extracts API signatures for use in calling convention standardization.
/// </summary>
public class RekoXmlApiParser
{
    /// <summary>
    /// Parse a Reko XML file and extract all API signatures.
    /// </summary>
    /// <param name="xmlPath">Path to the Reko XML file (e.g., kernel32.xml)</param>
    /// <returns>List of API signatures</returns>
    public List<ApiSignature> ParseXmlFile(string xmlPath)
    {
        var signatures = new List<ApiSignature>();
        
        try
        {
            var doc = XDocument.Load(xmlPath);
            var dllName = Path.GetFileNameWithoutExtension(xmlPath);
            
            // Normalize DLL name
            if (!dllName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                dllName = dllName + ".dll";
            }
            
            var procedures = doc.Descendants()
                .Where(e => e.Name.LocalName == "procedure");
            
            foreach (var procedure in procedures)
            {
                var signature = ParseProcedure(procedure, dllName);
                if (signature != null)
                {
                    signatures.Add(signature);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to parse {xmlPath}: {ex.Message}");
        }
        
        return signatures;
    }
    
    private ApiSignature? ParseProcedure(XElement procedure, string dllName)
    {
        var name = procedure.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(name))
            return null;
        
        var signature = new ApiSignature
        {
            Name = name,
            DllName = dllName
        };
        
        var signatureElement = procedure.Element(procedure.Name.Namespace + "signature");
        if (signatureElement == null)
            return signature;
        
        // Parse return type
        var returnElement = signatureElement.Element(signatureElement.Name.Namespace + "return");
        if (returnElement != null)
        {
            var typeElement = returnElement.Element(returnElement.Name.Namespace + "type");
            if (typeElement != null)
            {
                signature.ReturnType = typeElement.Value;
            }
            
            var regElement = returnElement.Element(returnElement.Name.Namespace + "reg");
            if (regElement != null)
            {
                signature.ReturnRegister = regElement.Value;
            }
        }
        
        // Parse parameters
        var args = signatureElement.Elements(signatureElement.Name.Namespace + "arg");
        foreach (var arg in args)
        {
            var param = ParseParameter(arg);
            if (param != null)
            {
                signature.Parameters.Add(param);
            }
        }
        
        // Infer calling convention from parameter passing
        signature.CallingConvention = InferCallingConvention(signature.Parameters);
        
        return signature;
    }
    
    private ApiParameter? ParseParameter(XElement arg)
    {
        var name = arg.Attribute("name")?.Value;
        if (string.IsNullOrEmpty(name))
            return null;
        
        var param = new ApiParameter
        {
            Name = name
        };
        
        // Parse type
        var typeElement = arg.Element(arg.Name.Namespace + "type");
        if (typeElement != null)
        {
            param.Type = typeElement.Value;
        }
        
        // Parse stack parameter
        var stackElement = arg.Element(arg.Name.Namespace + "stack");
        if (stackElement != null)
        {
            param.IsStackParameter = true;
            
            var sizeAttr = stackElement.Attribute("size");
            if (sizeAttr != null && int.TryParse(sizeAttr.Value, out var size))
            {
                param.StackSize = size;
            }
        }
        
        // Parse register parameter
        var regElement = arg.Element(arg.Name.Namespace + "reg");
        if (regElement != null)
        {
            param.RegisterName = regElement.Value;
        }
        
        return param;
    }
    
    private Win32CallingConvention InferCallingConvention(List<ApiParameter> parameters)
    {
        // Check if any parameters are in ECX/EDX (fastcall or thiscall)
        var registerParams = parameters.Where(p => p.RegisterName != null).ToList();
        
        if (registerParams.Any())
        {
            var firstReg = registerParams.First().RegisterName?.ToLower();
            if (firstReg == "ecx" && registerParams.Count == 1)
            {
                // First param in ECX only - likely thiscall
                return Win32CallingConvention.Thiscall;
            }
            else if (registerParams.Any(p => p.RegisterName?.ToLower() is "ecx" or "edx"))
            {
                // Multiple register params - fastcall
                return Win32CallingConvention.Fastcall;
            }
        }
        
        // Default to stdcall for Win32 APIs
        return Win32CallingConvention.Stdcall;
    }
}
