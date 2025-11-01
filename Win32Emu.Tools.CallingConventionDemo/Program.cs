using Win32Emu.CallingConvention;

namespace Win32Emu.Tools.CallingConventionDemo;

/// <summary>
/// Demonstrates Reko-based calling convention standardization (Integration Opportunity #4).
/// Generates marshalling code from Reko XML API definitions to reduce boilerplate.
/// Now includes: structs, callbacks, COM interfaces, validation, docs, and tests.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Win32Emu Calling Convention Standardization Demo");
        Console.WriteLine("==================================================");
        Console.WriteLine();
        Console.WriteLine("This tool demonstrates Integration Opportunity #4:");
        Console.WriteLine("Using Reko's XML definitions to standardize calling conventions");
        Console.WriteLine("and auto-generate parameter marshalling code.");
        Console.WriteLine();

        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Win32Emu.Tools.CallingConventionDemo <xml-file> [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --api <name>           Generate code for specific API");
            Console.WriteLine("  --structs              Show struct definitions");
            Console.WriteLine("  --callbacks            Show callback delegates");
            Console.WriteLine("  --validation           Show validation examples");
            Console.WriteLine("  --docs                 Show documentation generation");
            Console.WriteLine("  --tests                Show unit test generation");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  # Generate code for all APIs");
            Console.WriteLine("  Win32Emu.Tools.CallingConventionDemo kernel32.xml");
            Console.WriteLine();
            Console.WriteLine("  # Generate code for specific API");
            Console.WriteLine("  Win32Emu.Tools.CallingConventionDemo user32.xml --api MessageBoxA");
            Console.WriteLine();
            Console.WriteLine("  # Show struct definitions");
            Console.WriteLine("  Win32Emu.Tools.CallingConventionDemo Common.xml --structs");
            Console.WriteLine();
            Console.WriteLine("  # Show all features");
            Console.WriteLine("  Win32Emu.Tools.CallingConventionDemo user32.xml --api MessageBoxA --docs --tests");
            return;
        }

        var xmlFile = args[0];
        string? apiFilter = null;
        bool showStructs = false;
        bool showCallbacks = false;
        bool showValidation = false;
        bool showDocs = false;
        bool showTests = false;

        // Parse command line arguments
        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--api":
                    if (i + 1 < args.Length)
                        apiFilter = args[++i];
                    break;
                case "--structs":
                    showStructs = true;
                    break;
                case "--callbacks":
                    showCallbacks = true;
                    break;
                case "--validation":
                    showValidation = true;
                    break;
                case "--docs":
                    showDocs = true;
                    break;
                case "--tests":
                    showTests = true;
                    break;
            }
        }

        if (!File.Exists(xmlFile))
        {
            Console.WriteLine($"Error: File not found: {xmlFile}");
            return;
        }

        Console.WriteLine($"Parsing: {Path.GetFileName(xmlFile)}");
        Console.WriteLine();

        var parser = new RekoXmlApiParser();
        var signatures = parser.ParseXmlFile(xmlFile);
        var typeDefinitions = parser.ParseTypeDefinitions(xmlFile);

        // Show type definitions if requested
        if (showStructs && typeDefinitions.Structs.Count > 0)
        {
            ShowStructDefinitions(typeDefinitions.Structs);
        }

        if (showCallbacks && typeDefinitions.Callbacks.Count > 0)
        {
            ShowCallbackDefinitions(typeDefinitions.Callbacks);
        }

        if (signatures.Count == 0)
        {
            Console.WriteLine("No API signatures found in file.");
            return;
        }

        Console.WriteLine($"Found {signatures.Count} API signatures");
        Console.WriteLine();

        // Filter by API name if specified
        if (!string.IsNullOrEmpty(apiFilter))
        {
            signatures = signatures
                .Where(s => s.Name.Equals(apiFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (signatures.Count == 0)
            {
                Console.WriteLine($"No APIs found matching '{apiFilter}'");
                return;
            }
        }

        var generator = new MarshallingCodeGenerator();

        foreach (var signature in signatures.Take(5)) // Show first 5 for demo
        {
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine($"API: {signature.Name}");
            Console.WriteLine($"DLL: {signature.DllName}");
            Console.WriteLine($"Calling Convention: {signature.CallingConvention}");
            Console.WriteLine($"Return: {signature.ReturnType} (in {signature.ReturnRegister})");
            Console.WriteLine($"Parameters: {signature.Parameters.Count}");
            
            foreach (var param in signature.Parameters)
            {
                var location = param.IsStackParameter ? "stack" : $"register {param.RegisterName}";
                Console.WriteLine($"  - {param.Name}: {param.Type} ({location})");
            }
            
            Console.WriteLine();
            
            if (showDocs)
            {
                Console.WriteLine("Generated Documentation:");
                Console.WriteLine(generator.GenerateDocumentation(signature, "System Services"));
                Console.WriteLine();
            }
            
            Console.WriteLine("Generated Parameter Reader:");
            Console.WriteLine(generator.GenerateParameterReader(signature));
            Console.WriteLine();
            
            if (!string.IsNullOrEmpty(apiFilter)) // Full wrapper if specific API requested
            {
                Console.WriteLine("Generated Wrapper Method:");
                Console.WriteLine(generator.GenerateWrapper(signature));
                Console.WriteLine();
            }
            
            if (showTests)
            {
                Console.WriteLine("Generated Unit Test:");
                Console.WriteLine(generator.GenerateUnitTest(signature));
                Console.WriteLine();
            }
            
            if (showValidation)
            {
                Console.WriteLine("Validation Report:");
                Console.WriteLine(generator.GenerateValidationReport(signature, signature));
                Console.WriteLine();
            }
        }

        if (signatures.Count > 5 && apiFilter == null)
        {
            Console.WriteLine($"... and {signatures.Count - 5} more APIs");
            Console.WriteLine();
            Console.WriteLine("Tip: Specify --api <name> to see full wrapper code generation");
        }

        Console.WriteLine();
        Console.WriteLine("Benefits of Calling Convention Standardization:");
        Console.WriteLine("================================================");
        Console.WriteLine("✓ Reduces boilerplate - No manual parameter extraction");
        Console.WriteLine("✓ Type safety - Proper type mapping from Windows types");
        Console.WriteLine("✓ Convention awareness - Handles stdcall, fastcall, thiscall correctly");
        Console.WriteLine("✓ Automatic string marshalling - Detects and handles LPSTR/LPWSTR");
        Console.WriteLine("✓ Consistency - All APIs follow same pattern");
        Console.WriteLine("✓ Maintainability - Easy to update when Reko definitions change");
        
        if (typeDefinitions.Structs.Count > 0 || typeDefinitions.Callbacks.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("New Features:");
            Console.WriteLine("=============");
            if (typeDefinitions.Structs.Count > 0)
                Console.WriteLine($"✓ Struct definitions - {typeDefinitions.Structs.Count} structs parsed");
            if (typeDefinitions.Callbacks.Count > 0)
                Console.WriteLine($"✓ Callback delegates - {typeDefinitions.Callbacks.Count} callbacks identified");
            if (showDocs)
                Console.WriteLine("✓ Documentation generation - XML docs with categories");
            if (showTests)
                Console.WriteLine("✓ Unit test generation - xUnit test templates");
            if (showValidation)
                Console.WriteLine("✓ Validation mode - Signature checking");
        }
    }
    
    static void ShowStructDefinitions(List<StructDefinition> structs)
    {
        Console.WriteLine("Struct Definitions:");
        Console.WriteLine("===================");
        Console.WriteLine();
        
        var generator = new MarshallingCodeGenerator();
        foreach (var structDef in structs.Take(5))
        {
            Console.WriteLine(generator.GenerateStructDefinition(structDef));
            Console.WriteLine();
        }
        
        if (structs.Count > 5)
        {
            Console.WriteLine($"... and {structs.Count - 5} more structs");
            Console.WriteLine();
        }
    }
    
    static void ShowCallbackDefinitions(List<CallbackDefinition> callbacks)
    {
        Console.WriteLine("Callback Delegates:");
        Console.WriteLine("===================");
        Console.WriteLine();
        
        var generator = new MarshallingCodeGenerator();
        foreach (var callback in callbacks.Take(5))
        {
            Console.WriteLine(generator.GenerateCallbackDelegate(callback));
            Console.WriteLine();
        }
        
        if (callbacks.Count > 5)
        {
            Console.WriteLine($"... and {callbacks.Count - 5} more callbacks");
            Console.WriteLine();
        }
    }
}
