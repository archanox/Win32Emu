using System.CommandLine;
using Win32Emu.CodeGen.ApiMetadata;

namespace Win32Emu.CodeGen;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Win32Emu Code Generation Tools - API Metadata Parser and Validator");
        
        // Command: analyze-dlls
        var analyzeDllsCommand = new Command("analyze-dlls", "Analyze PE DLL exports and compare with implemented APIs");
        var dllDirOption = new Option<string>(
            "--dll-dir",
            description: "Directory containing DLLs to analyze (e.g., DLLs/WinME)",
            getDefaultValue: () => "DLLs/WinME"
        );
        var outputOption = new Option<string?>(
            "--output",
            description: "Output file for the report (optional, defaults to console)"
        );
        
        analyzeDllsCommand.AddOption(dllDirOption);
        analyzeDllsCommand.AddOption(outputOption);
        analyzeDllsCommand.SetHandler(AnalyzeDlls, dllDirOption, outputOption);
        
        // Command: parse-xml
        var parseXmlCommand = new Command("parse-xml", "Parse API Monitor XML files");
        var xmlDirOption = new Option<string>(
            "--xml-dir",
            description: "Directory containing API Monitor XML files"
        );
        
        parseXmlCommand.AddOption(xmlDirOption);
        parseXmlCommand.SetHandler(ParseXml, xmlDirOption);
        
        // Command: coverage-report
        var coverageCommand = new Command("coverage-report", "Generate API coverage report");
        var winmeOption = new Option<string>(
            "--winme",
            description: "Path to WinME DLLs directory",
            getDefaultValue: () => "DLLs/WinME"
        );
        var winxpOption = new Option<string>(
            "--winxp",
            description: "Path to WinXP DLLs directory",
            getDefaultValue: () => "DLLs/WinXP"
        );
        var reportOutputOption = new Option<string?>(
            "--output",
            description: "Output file for the report (optional, defaults to console)"
        );
        var assemblyOption = new Option<string?>(
            "--assembly",
            description: "Path to Win32Emu.dll to extract implemented APIs (optional)"
        );
        
        coverageCommand.AddOption(winmeOption);
        coverageCommand.AddOption(winxpOption);
        coverageCommand.AddOption(reportOutputOption);
        coverageCommand.AddOption(assemblyOption);
        coverageCommand.SetHandler(GenerateCoverageReport, winmeOption, winxpOption, reportOutputOption, assemblyOption);
        
        // Command: generate-stubs
        var generateStubsCommand = new Command("generate-stubs", "Generate C# stub methods for APIs");
        var dllNameOption = new Option<string>(
            "--dll",
            description: "DLL name to generate stubs for (e.g., ADVAPI32.DLL)"
        ) { IsRequired = true };
        var stubOutputOption = new Option<string>(
            "--output",
            description: "Output file for generated stubs",
            getDefaultValue: () => "GeneratedStubs.cs"
        );
        var moduleClassOption = new Option<bool>(
            "--module-class",
            description: "Generate a complete module class instead of just methods",
            getDefaultValue: () => false
        );
        
        generateStubsCommand.AddOption(dllNameOption);
        generateStubsCommand.AddOption(stubOutputOption);
        generateStubsCommand.AddOption(moduleClassOption);
        generateStubsCommand.SetHandler(GenerateStubs, dllNameOption, stubOutputOption, moduleClassOption);
        
        rootCommand.AddCommand(analyzeDllsCommand);
        rootCommand.AddCommand(parseXmlCommand);
        rootCommand.AddCommand(coverageCommand);
        rootCommand.AddCommand(generateStubsCommand);
        
        return await rootCommand.InvokeAsync(args);
    }
    
    static void AnalyzeDlls(string dllDir, string? output)
    {
        Console.WriteLine($"Analyzing DLLs in: {dllDir}");
        Console.WriteLine();
        
        var exports = PeExportParser.ParseDirectory(dllDir);
        
        Console.WriteLine();
        Console.WriteLine("Summary:");
        Console.WriteLine($"Total DLLs: {exports.Count}");
        Console.WriteLine($"Total Exports: {exports.Sum(kvp => kvp.Value.Count)}");
        
        if (output != null)
        {
            WriteExportReport(exports, output);
            Console.WriteLine($"\nReport written to: {output}");
        }
    }
    
    static void ParseXml(string xmlDir)
    {
        Console.WriteLine($"Parsing API Monitor XML files in: {xmlDir}");
        Console.WriteLine();
        
        var apis = XmlParser.ParseDirectory(xmlDir);
        
        Console.WriteLine();
        Console.WriteLine("Summary:");
        Console.WriteLine($"Total XML files: {apis.Count}");
        Console.WriteLine($"Total API definitions: {apis.Sum(kvp => kvp.Value.Count)}");
    }
    
    static void GenerateCoverageReport(string winmePath, string winxpPath, string? output, string? assemblyPath)
    {
        Console.WriteLine("Generating API Coverage Report");
        Console.WriteLine("==============================");
        Console.WriteLine();
        
        var db = new MetadataDatabase();
        
        // Parse WinME DLLs
        Console.WriteLine($"Parsing WinME DLLs from: {winmePath}");
        var winmeExports = PeExportParser.ParseDirectory(winmePath);
        foreach (var (dllName, exports) in winmeExports)
        {
            db.AddPeExports(dllName, exports);
        }
        
        // Parse WinXP DLLs
        Console.WriteLine($"Parsing WinXP DLLs from: {winxpPath}");
        var winxpExports = PeExportParser.ParseDirectory(winxpPath);
        foreach (var (dllName, exports) in winxpExports)
        {
            // Merge with WinME exports
            var metadata = db.GetDllMetadata(dllName);
            if (metadata != null)
            {
                // Add any exports that are only in WinXP
                foreach (var export in exports)
                {
                    if (!metadata.PeExports.Any(e => e.Name == export.Name))
                    {
                        metadata.PeExports.Add(export);
                    }
                }
            }
            else
            {
                db.AddPeExports(dllName, exports);
            }
        }
        
        Console.WriteLine();
        
        // Extract implemented APIs from assembly if provided
        if (!string.IsNullOrEmpty(assemblyPath))
        {
            Console.WriteLine($"Extracting implemented APIs from: {assemblyPath}");
            var implementedApis = ImplementedApiExtractor.ExtractFromAssembly(assemblyPath);
            
            foreach (var (dllName, apis) in implementedApis)
            {
                Console.WriteLine($"  Found {apis.Count} implemented APIs in {dllName}");
                foreach (var (funcName, argBytes) in apis)
                {
                    db.AddImplementedApi(dllName, funcName, argBytes);
                }
            }
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("Note: To show implemented APIs, provide --assembly path to Win32Emu.dll");
            Console.WriteLine("      Currently showing DLL exports only.");
            Console.WriteLine();
        }
        
        // Generate report
        var report = db.GenerateCoverageReport();
        
        var reportText = FormatCoverageReport(report);
        
        if (output != null)
        {
            File.WriteAllText(output, reportText);
            Console.WriteLine($"\nReport written to: {output}");
        }
        else
        {
            Console.WriteLine(reportText);
        }
    }
    
    static string FormatCoverageReport(CoverageReport report)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("API Coverage Report");
        sb.AppendLine("===================");
        sb.AppendLine();
        sb.AppendLine($"Overall Coverage: {report.TotalImplemented}/{report.TotalExports} ({report.CoveragePercentage:F1}%)");
        sb.AppendLine();
        
        foreach (var (dllName, dllReport) in report.DllReports.OrderByDescending(kvp => kvp.Value.TotalExports))
        {
            sb.AppendLine($"{dllName}");
            sb.AppendLine($"  Exports: {dllReport.TotalExports}");
            sb.AppendLine($"  Implemented: {dllReport.ImplementedCount} ({dllReport.CoveragePercentage:F1}%)");
            
            if (dllReport.ImplementedApis.Count > 0)
            {
                sb.AppendLine($"  Implemented APIs: {string.Join(", ", dllReport.ImplementedApis.Take(5))}...");
            }
            
            if (dllReport.MissingApis.Count > 0)
            {
                sb.AppendLine($"  Sample Missing APIs: {string.Join(", ", dllReport.MissingApis.Take(5))}...");
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    static void WriteExportReport(Dictionary<string, List<ExportedFunction>> exports, string outputPath)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("PE DLL Export Analysis");
        sb.AppendLine("======================");
        sb.AppendLine();
        
        foreach (var (dllName, funcs) in exports.OrderBy(kvp => kvp.Key))
        {
            sb.AppendLine($"{dllName} ({funcs.Count} exports)");
            sb.AppendLine("---");
            
            foreach (var func in funcs.OrderBy(f => f.Ordinal))
            {
                var forwarded = func.ForwardedTo != null ? $" -> {func.ForwardedTo}" : "";
                sb.AppendLine($"  {func.Ordinal,4}: {func.Name}{forwarded}");
            }
            
            sb.AppendLine();
        }
        
        File.WriteAllText(outputPath, sb.ToString());
    }
    
    static void GenerateStubs(string dllName, string output, bool moduleClass)
    {
        Console.WriteLine($"Generating stubs for {dllName}");
        Console.WriteLine("===============================");
        Console.WriteLine();
        
        // Parse API Monitor XML to get function signatures
        var xmlApiMonPath = Path.Combine("ApiMon XMLs", "Windows");
        var xmlModuleName = Path.GetFileNameWithoutExtension(dllName);
        
        // Try case-insensitive search for the XML file
        string? xmlPath = null;
        if (Directory.Exists(xmlApiMonPath))
        {
            var xmlFiles = Directory.GetFiles(xmlApiMonPath, "*.xml");
            xmlPath = xmlFiles.FirstOrDefault(f => 
                string.Equals(Path.GetFileNameWithoutExtension(f), xmlModuleName, StringComparison.OrdinalIgnoreCase));
        }
        
        Dictionary<string, ApiDefinition>? xmlDefinitions = null;
        if (xmlPath != null && File.Exists(xmlPath))
        {
            Console.WriteLine($"Loading API definitions from {xmlPath}");
            var apis = XmlParser.ParseApiMonitorXml(xmlPath);
            
            // Group by name and take the first definition (API Monitor XMLs can have duplicates for W/A versions)
            xmlDefinitions = apis
                .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            Console.WriteLine($"Loaded {xmlDefinitions.Count} API definitions");
        }
        else
        {
            Console.WriteLine($"Warning: API Monitor XML not found for {xmlModuleName}");
        }
        Console.WriteLine();
        
        // Group exports by DLL name across different versions
        var allExports = new List<ExportedFunction>();
        
        // Check all DLL directories
        var dllDirectories = new[] { "DLLs/WinME", "DLLs/WinXP" };
        foreach (var dllDir in dllDirectories)
        {
            if (!Directory.Exists(dllDir))
                continue;
            
            // Case-insensitive search for the DLL file
            var dllFiles = Directory.GetFiles(dllDir, "*", SearchOption.TopDirectoryOnly);
            var dllPath = dllFiles.FirstOrDefault(f => 
                string.Equals(Path.GetFileName(f), dllName, StringComparison.OrdinalIgnoreCase));
                
            if (dllPath != null && File.Exists(dllPath))
            {
                Console.WriteLine($"Parsing {dllPath}...");
                var exports = PeExportParser.ParseExports(dllPath);
                allExports.AddRange(exports);
                Console.WriteLine($"  Found {exports.Count} exports");
            }
        }
        
        if (allExports.Count == 0)
        {
            Console.WriteLine($"Error: No exports found for {dllName}");
            return;
        }
        
        Console.WriteLine($"Total exports across all versions: {allExports.Count}");
        Console.WriteLine();
        
        // Generate stubs for ALL exports (not just missing ones)
        string code;
        if (moduleClass)
        {
            var moduleName = Path.GetFileNameWithoutExtension(dllName) + "Module";
            code = StubGenerator.GenerateModuleClass(moduleName, dllName, allExports, xmlDefinitions);
        }
        else
        {
            code = StubGenerator.GenerateStubs(dllName, allExports, xmlDefinitions);
        }
        
        File.WriteAllText(output, code);
        Console.WriteLine($"Stubs written to: {output}");
        Console.WriteLine($"Total lines: {code.Split('\n').Length}");
    }
}
