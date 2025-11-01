using System.Xml.Linq;

namespace Win32Emu.CallingConvention;

/// <summary>
/// Represents a field in a struct definition.
/// </summary>
public class StructField
{
    /// <summary>
    /// Field name (e.g., "cbSize", "dwMajorVersion")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Field type (e.g., "DWORD", "LPCTSTR")
    /// </summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents a struct definition parsed from XML.
/// </summary>
public class StructDefinition
{
    /// <summary>
    /// Struct name (e.g., "MSGBOXPARAMS", "DLLVERSIONINFO")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Fields in the struct
    /// </summary>
    public List<StructField> Fields { get; set; } = new();
}

/// <summary>
/// Represents a callback/delegate type definition.
/// </summary>
public class CallbackDefinition
{
    /// <summary>
    /// Callback type name (e.g., "WNDPROC", "MSGBOXCALLBACK")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Base type if it's an alias (e.g., "LPVOID")
    /// </summary>
    public string? BaseType { get; set; }
    
    /// <summary>
    /// Return type for the callback
    /// </summary>
    public string? ReturnType { get; set; }
    
    /// <summary>
    /// Parameters for the callback
    /// </summary>
    public List<ApiParameter> Parameters { get; set; } = new();
}

/// <summary>
/// Represents a type alias definition.
/// </summary>
public class TypeAlias
{
    /// <summary>
    /// Alias name (e.g., "HDESK", "SIZE_T")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Base type (e.g., "HANDLE", "DWORD")
    /// </summary>
    public string BaseType { get; set; } = string.Empty;
    
    /// <summary>
    /// Enum values if this is an enum type
    /// </summary>
    public Dictionary<string, string>? EnumValues { get; set; }
}

/// <summary>
/// Represents a COM interface definition.
/// </summary>
public class ComInterfaceDefinition
{
    /// <summary>
    /// Interface name (e.g., "IUnknown", "IDispatch")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Base interface (e.g., "IUnknown")
    /// </summary>
    public string? BaseInterface { get; set; }
    
    /// <summary>
    /// Interface methods (if defined in XML)
    /// </summary>
    public List<ApiSignature> Methods { get; set; } = new();
    
    /// <summary>
    /// GUID if specified
    /// </summary>
    public string? Guid { get; set; }
}

/// <summary>
/// Represents all type definitions parsed from an XML file.
/// </summary>
public class TypeDefinitions
{
    /// <summary>
    /// Struct definitions
    /// </summary>
    public List<StructDefinition> Structs { get; set; } = new();
    
    /// <summary>
    /// Callback/delegate definitions
    /// </summary>
    public List<CallbackDefinition> Callbacks { get; set; } = new();
    
    /// <summary>
    /// COM interface definitions
    /// </summary>
    public List<ComInterfaceDefinition> ComInterfaces { get; set; } = new();
    
    /// <summary>
    /// Type aliases
    /// </summary>
    public List<TypeAlias> Aliases { get; set; } = new();
}

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
    
    /// <summary>
    /// Parse type definitions (structs, callbacks, aliases) from ApiMonitor XML files.
    /// </summary>
    /// <param name="xmlPath">Path to the ApiMonitor XML file</param>
    /// <returns>Type definitions found in the file</returns>
    public TypeDefinitions ParseTypeDefinitions(string xmlPath)
    {
        var definitions = new TypeDefinitions();
        
        try
        {
            var doc = XDocument.Load(xmlPath);
            
            // Parse Variable elements which contain struct, alias, and callback definitions
            var variables = doc.Descendants()
                .Where(e => e.Name.LocalName == "Variable");
            
            foreach (var variable in variables)
            {
                var name = variable.Attribute("Name")?.Value;
                var type = variable.Attribute("Type")?.Value;
                
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type))
                    continue;
                
                switch (type)
                {
                    case "Struct":
                        var structDef = ParseStructDefinition(variable, name);
                        if (structDef != null)
                            definitions.Structs.Add(structDef);
                        break;
                    
                    case "Interface":
                        var interfaceDef = ParseComInterface(variable, name);
                        if (interfaceDef != null)
                            definitions.ComInterfaces.Add(interfaceDef);
                        break;
                    
                    case "Alias":
                        var aliasDef = ParseTypeAlias(variable, name);
                        if (aliasDef != null)
                        {
                            // Check if it's a callback/function pointer
                            if (IsCallbackType(name, aliasDef.BaseType))
                            {
                                var callback = new CallbackDefinition
                                {
                                    Name = name,
                                    BaseType = aliasDef.BaseType
                                };
                                definitions.Callbacks.Add(callback);
                            }
                            else
                            {
                                definitions.Aliases.Add(aliasDef);
                            }
                        }
                        break;
                    
                    case "Pointer":
                        // Pointer types are typically handled as aliases
                        var baseType = variable.Attribute("Base")?.Value;
                        if (!string.IsNullOrEmpty(baseType))
                        {
                            var pointerAlias = new TypeAlias
                            {
                                Name = name,
                                BaseType = baseType + "*"
                            };
                            definitions.Aliases.Add(pointerAlias);
                        }
                        break;
                }
            }
        }
        catch (System.IO.IOException ex)
        {
            Console.WriteLine($"Warning: IO error while reading {xmlPath}: {ex.Message}");
        }
        catch (System.Xml.XmlException ex)
        {
            Console.WriteLine($"Warning: XML parse error in {xmlPath}: {ex.Message}");
        }
        
        return definitions;
    }
    
    private StructDefinition? ParseStructDefinition(XElement variable, string name)
    {
        var structDef = new StructDefinition { Name = name };
        
        var fields = variable.Elements()
            .Where(e => e.Name.LocalName == "Field");
        
        foreach (var field in fields)
        {
            var fieldName = field.Attribute("Name")?.Value;
            var fieldType = field.Attribute("Type")?.Value;
            
            if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(fieldType))
            {
                structDef.Fields.Add(new StructField
                {
                    Name = fieldName,
                    Type = fieldType
                });
            }
        }
        
        return structDef.Fields.Count > 0 ? structDef : null;
    }
    
    private TypeAlias? ParseTypeAlias(XElement variable, string name)
    {
        var baseType = variable.Attribute("Base")?.Value;
        if (string.IsNullOrEmpty(baseType))
            return null;
        
        var alias = new TypeAlias
        {
            Name = name,
            BaseType = baseType
        };
        
        // Parse enum values if present
        var enumElement = variable.Element(variable.Name.Namespace + "Enum");
        if (enumElement != null)
        {
            alias.EnumValues = new Dictionary<string, string>();
            
            var sets = enumElement.Elements()
                .Where(e => e.Name.LocalName == "Set");
            
            foreach (var set in sets)
            {
                var setName = set.Attribute("Name")?.Value;
                var setValue = set.Attribute("Value")?.Value;
                
                if (!string.IsNullOrEmpty(setName) && !string.IsNullOrEmpty(setValue))
                {
                    alias.EnumValues[setName] = setValue;
                }
            }
        }
        
        return alias;
    }
    
    private ComInterfaceDefinition? ParseComInterface(XElement variable, string name)
    {
        var interfaceDef = new ComInterfaceDefinition { Name = name };
        
        // Look for base interface attribute
        var baseAttr = variable.Attribute("Base");
        if (baseAttr != null)
        {
            interfaceDef.BaseInterface = baseAttr.Value;
        }
        
        // Look for GUID attribute
        var guidAttr = variable.Attribute("Guid");
        if (guidAttr != null)
        {
            interfaceDef.Guid = guidAttr.Value;
        }
        
        // Look for method definitions (if present in XML)
        var methods = variable.Elements()
            .Where(e => e.Name.LocalName == "Method");
        
        foreach (var method in methods)
        {
            var methodName = method.Attribute("Name")?.Value;
            if (!string.IsNullOrEmpty(methodName))
            {
                // Parse method signature similar to API signatures
                var methodSig = new ApiSignature
                {
                    Name = methodName,
                    CallingConvention = Win32CallingConvention.Thiscall // COM uses thiscall
                };
                
                // Parse return type
                var returnElement = method.Element(method.Name.Namespace + "Return");
                if (returnElement != null)
                {
                    var typeElement = returnElement.Element(returnElement.Name.Namespace + "Type");
                    if (typeElement != null)
                    {
                        methodSig.ReturnType = typeElement.Value;
                    }
                }
                
                // Parse parameters
                var paramElements = method.Elements()
                    .Where(e => e.Name.LocalName == "Param");
                
                foreach (var param in paramElements)
                {
                    var paramName = param.Attribute("Name")?.Value;
                    var paramType = param.Attribute("Type")?.Value;
                    
                    if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(paramType))
                    {
                        methodSig.Parameters.Add(new ApiParameter
                        {
                            Name = paramName,
                            Type = paramType,
                            IsStackParameter = true
                        });
                    }
                }
                
                interfaceDef.Methods.Add(methodSig);
            }
        }
        
        return interfaceDef;
    }
    
    private bool IsCallbackType(string name, string? baseType)
    {
        // Heuristic: callback types often have specific naming patterns
        if (name.EndsWith("PROC", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("CALLBACK", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("PFN", StringComparison.OrdinalIgnoreCase) ||
            (name.StartsWith("LP", StringComparison.OrdinalIgnoreCase) && name.EndsWith("PROC", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
        
        // Also check if base type is LPVOID which is common for callbacks
        return baseType == "LPVOID" && (name.Contains("CALLBACK", StringComparison.OrdinalIgnoreCase) || name.Contains("PROC", StringComparison.OrdinalIgnoreCase));
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
