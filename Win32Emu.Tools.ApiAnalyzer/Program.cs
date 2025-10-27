using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Win32Emu.Tools.ApiAnalyzer;

/// <summary>
/// Analyzes Win32 API coverage by comparing Reko XML definitions with Win32Emu implementations.
/// This tool demonstrates the value of integrating Reko's comprehensive API definitions.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Win32Emu API Coverage Analyzer");
        Console.WriteLine("================================");
        Console.WriteLine();
        Console.WriteLine("This proof-of-concept tool demonstrates how Reko's API definitions");
        Console.WriteLine("could be used to analyze and improve Win32Emu's API coverage.");
        Console.WriteLine();

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Win32Emu.Tools.ApiAnalyzer <reko-xml-dir> <win32emu-modules-dir>");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  Win32Emu.Tools.ApiAnalyzer \\tmp\\reko\\src\\Environments\\Windows Win32Emu\\Win32\\Modules");
            Console.WriteLine();
            Console.WriteLine("To get Reko definitions:");
            Console.WriteLine("  git clone https://github.com/uxmal/reko.git /tmp/reko");
            return;
        }

        var rekoXmlDir = args[0];
        var win32EmuModulesDir = args[1];

        if (!Directory.Exists(rekoXmlDir))
        {
            Console.WriteLine($"Error: Reko XML directory not found: {rekoXmlDir}");
            return;
        }

        if (!Directory.Exists(win32EmuModulesDir))
        {
            Console.WriteLine($"Error: Win32Emu modules directory not found: {win32EmuModulesDir}");
            return;
        }

        var analyzer = new ApiCoverageAnalyzer(rekoXmlDir, win32EmuModulesDir);
        analyzer.Analyze();
    }
}

/// <summary>
/// Analyzes API coverage between Reko definitions and Win32Emu implementations.
/// </summary>
class ApiCoverageAnalyzer
{
    private readonly string _rekoXmlDir;
    private readonly string _win32EmuModulesDir;
    
    // Maps DLL name to list of API names defined in Reko
    private readonly Dictionary<string, HashSet<string>> _rekoApis = new();
    
    // Maps DLL name to list of API names implemented in Win32Emu
    private readonly Dictionary<string, HashSet<string>> _win32EmuApis = new();

    public ApiCoverageAnalyzer(string rekoXmlDir, string win32EmuModulesDir)
    {
        _rekoXmlDir = rekoXmlDir;
        _win32EmuModulesDir = win32EmuModulesDir;
    }

    public void Analyze()
    {
        Console.WriteLine("Step 1: Parsing Reko XML API definitions...");
        ParseRekoXmlFiles();
        
        Console.WriteLine("Step 2: Analyzing Win32Emu module implementations...");
        ParseWin32EmuModules();
        
        Console.WriteLine();
        Console.WriteLine("Step 3: Generating Coverage Report...");
        Console.WriteLine();
        GenerateReport();
    }

    private void ParseRekoXmlFiles()
    {
        var xmlFiles = Directory.GetFiles(_rekoXmlDir, "*.xml")
            .Where(f => !f.Contains("characteristics") && !f.Contains("windows32") && !f.Contains("windows64"))
            .ToList();

        foreach (var xmlFile in xmlFiles)
        {
            try
            {
                var doc = XDocument.Load(xmlFile);
                var dllName = Path.GetFileNameWithoutExtension(xmlFile);
                
                // Normalize DLL names (e.g., "kernel32" -> "kernel32.dll")
                if (!dllName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    dllName = dllName + ".dll";
                }

                var procedures = doc.Descendants()
                    .Where(e => e.Name.LocalName == "procedure")
                    .Select(e => e.Attribute("name")?.Value)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Cast<string>()
                    .ToHashSet();

                if (procedures.Count > 0)
                {
                    _rekoApis[dllName.ToLower()] = procedures;
                    Console.WriteLine($"  Loaded {procedures.Count} APIs from {Path.GetFileName(xmlFile)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Failed to parse {Path.GetFileName(xmlFile)}: {ex.Message}");
            }
        }
    }

    private void ParseWin32EmuModules()
    {
        var moduleFiles = Directory.GetFiles(_win32EmuModulesDir, "*Module.cs");

        foreach (var moduleFile in moduleFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(moduleFile);
                
                // Extract DLL name from module file name (e.g., "Kernel32Module" -> "kernel32.dll")
                string dllName;
                if (fileName.EndsWith("Module", StringComparison.OrdinalIgnoreCase))
                {
                    // Remove "Module" suffix and add .dll extension
                    var baseName = fileName.Substring(0, fileName.Length - 6);
                    dllName = baseName.ToLower() + ".dll";
                }
                else
                {
                    continue; // Skip files that don't follow the Module naming convention
                }

                var content = File.ReadAllText(moduleFile);
                
                // Look for [DllModuleExport(...)] followed by public methods
                // Pattern handles both [DllModuleExport] and [DllModuleExport(params)]
                var exportPattern = new Regex(@"\[DllModuleExport(?:\([^\]]*\))?\][\s\r\n]+(?:\[DllModuleExport(?:\([^\]]*\))?\][\s\r\n]+)*(?:\/\/.*[\r\n]+)*\s*public\s+\w+\s+(\w+)\s*\(");
                var matches = exportPattern.Matches(content);
                
                var apis = matches
                    .Select(m => m.Groups[1].Value)
                    .ToHashSet();

                if (apis.Count > 0)
                {
                    _win32EmuApis[dllName.ToLower()] = apis;
                    Console.WriteLine($"  Found {apis.Count} APIs in {Path.GetFileName(moduleFile)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Failed to parse {Path.GetFileName(moduleFile)}: {ex.Message}");
            }
        }
    }

    private void GenerateReport()
    {
        var allDlls = _rekoApis.Keys.Union(_win32EmuApis.Keys).OrderBy(k => k).ToList();
        
        int totalRekoApis = 0;
        int totalImplementedApis = 0;
        int totalMissingApis = 0;

        foreach (var dll in allDlls)
        {
            _rekoApis.TryGetValue(dll, out var rekoApis);
            _win32EmuApis.TryGetValue(dll, out var win32EmuApis);

            if (rekoApis == null || rekoApis.Count == 0)
                continue;

            win32EmuApis ??= new HashSet<string>();

            var implemented = rekoApis.Intersect(win32EmuApis, StringComparer.OrdinalIgnoreCase).ToList();
            var missing = rekoApis.Except(win32EmuApis, StringComparer.OrdinalIgnoreCase).ToList();
            var extra = win32EmuApis.Except(rekoApis, StringComparer.OrdinalIgnoreCase).ToList();

            totalRekoApis += rekoApis.Count;
            totalImplementedApis += implemented.Count;
            totalMissingApis += missing.Count;

            var coverage = rekoApis.Count > 0 ? (implemented.Count * 100.0 / rekoApis.Count) : 0;

            Console.WriteLine($"{dll}:");
            Console.WriteLine($"  Total APIs in Reko: {rekoApis.Count}");
            Console.WriteLine($"  Implemented in Win32Emu: {implemented.Count} ({coverage:F1}%)");
            Console.WriteLine($"  Missing: {missing.Count}");
            
            if (extra.Count > 0)
            {
                Console.WriteLine($"  Extra (not in Reko): {extra.Count}");
            }

            if (missing.Count > 0 && missing.Count <= 10)
            {
                Console.WriteLine($"  Missing APIs: {string.Join(", ", missing.OrderBy(x => x))}");
            }
            else if (missing.Count > 10)
            {
                Console.WriteLine($"  Sample missing APIs: {string.Join(", ", missing.OrderBy(x => x).Take(10))}...");
            }

            Console.WriteLine();
        }

        Console.WriteLine("Overall Summary");
        Console.WriteLine("===============");
        Console.WriteLine($"Total APIs in Reko definitions: {totalRekoApis}");
        Console.WriteLine($"Total implemented in Win32Emu: {totalImplementedApis}");
        Console.WriteLine($"Total missing: {totalMissingApis}");
        
        if (totalRekoApis > 0)
        {
            var overallCoverage = totalImplementedApis * 100.0 / totalRekoApis;
            Console.WriteLine($"Overall coverage: {overallCoverage:F1}%");
        }

        Console.WriteLine();
        Console.WriteLine("Analysis Complete!");
        Console.WriteLine();
        Console.WriteLine("Next Steps:");
        Console.WriteLine("1. Review missing APIs - which ones are used by target applications?");
        Console.WriteLine("2. Prioritize implementation based on usage frequency");
        Console.WriteLine("3. Use Reko XML as specification when implementing new APIs");
        Console.WriteLine("4. Consider auto-generating stub implementations from XML");
    }
}
