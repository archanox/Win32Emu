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
        
        rootCommand.AddCommand(analyzeDllsCommand);
        rootCommand.AddCommand(parseXmlCommand);
        rootCommand.AddCommand(coverageCommand);
        
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
}
