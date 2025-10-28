using Win32Emu.CallingConvention;

namespace Win32Emu.Tools.CallingConventionDemo;

/// <summary>
/// Demonstrates Reko-based calling convention standardization (Integration Opportunity #4).
/// Generates marshalling code from Reko XML API definitions to reduce boilerplate.
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
            Console.WriteLine("Usage: Win32Emu.Tools.CallingConventionDemo <reko-xml-file> [api-name]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  # Generate code for all APIs in kernel32.xml");
            Console.WriteLine("  Win32Emu.Tools.CallingConventionDemo /tmp/reko/src/Environments/Windows/kernel32.xml");
            Console.WriteLine();
            Console.WriteLine("  # Generate code for specific API");
            Console.WriteLine("  Win32Emu.Tools.CallingConventionDemo /tmp/reko/src/Environments/Windows/user32.xml MessageBoxA");
            Console.WriteLine();
            Console.WriteLine("To get Reko definitions:");
            Console.WriteLine("  git clone https://github.com/uxmal/reko.git /tmp/reko");
            return;
        }

        var xmlFile = args[0];
        var apiFilter = args.Length > 1 ? args[1] : null;

        if (!File.Exists(xmlFile))
        {
            Console.WriteLine($"Error: File not found: {xmlFile}");
            return;
        }

        Console.WriteLine($"Parsing: {Path.GetFileName(xmlFile)}");
        Console.WriteLine();

        var parser = new RekoXmlApiParser();
        var signatures = parser.ParseXmlFile(xmlFile);

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
            Console.WriteLine("Generated Parameter Reader:");
            Console.WriteLine(generator.GenerateParameterReader(signature));
            Console.WriteLine();
            
            if (args.Length > 1) // Full wrapper if specific API requested
            {
                Console.WriteLine("Generated Wrapper Method:");
                Console.WriteLine(generator.GenerateWrapper(signature));
                Console.WriteLine();
            }
        }

        if (signatures.Count > 5 && apiFilter == null)
        {
            Console.WriteLine($"... and {signatures.Count - 5} more APIs");
            Console.WriteLine();
            Console.WriteLine("Tip: Specify an API name to see full wrapper code generation");
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
        Console.WriteLine();
        Console.WriteLine("Next Steps:");
        Console.WriteLine("1. Integrate into Win32Emu module generation pipeline");
        Console.WriteLine("2. Create source generators for automatic code generation");
        Console.WriteLine("3. Add validation to detect signature mismatches");
        Console.WriteLine("4. Extend to support COM interfaces and callbacks");
    }
}
