using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Win32Emu.Tests.Emulator.SingleStepTests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tools.TestReporter;

/// <summary>
/// Tool to generate SingleStep CPU test reports for GitHub Pages
/// </summary>
class Program
{
	static int Main(string[] args)
	{
		var testDataPath = args.Length > 0 ? args[0] : "Win32Emu.Tests.Emulator/TestData/SingleStepTests";
		var outputPath = args.Length > 1 ? args[1] : "test-results";
		var maxTestsPerFile = 100;
		if (args.Length > 2)
		{
			if (!int.TryParse(args[2], out maxTestsPerFile) || maxTestsPerFile <= 0)
			{
				Console.Error.WriteLine($"Error: Invalid value for maxTestsPerFile: '{args[2]}'. Please provide a positive integer.");
				return 1;
			}
		}

		Console.WriteLine($"SingleStep CPU Test Reporter");
		Console.WriteLine($"============================");
		Console.WriteLine($"Test Data Path: {testDataPath}");
		Console.WriteLine($"Output Path: {outputPath}");
		Console.WriteLine($"Max Tests Per File: {maxTestsPerFile}");
		Console.WriteLine();

		// Create output directory
		Directory.CreateDirectory(outputPath);

		// Find all MOO test files
		if (!Directory.Exists(testDataPath))
		{
			Console.Error.WriteLine($"Error: Test data path does not exist: {testDataPath}");
			return 1;
		}

		var testFiles = Directory.GetFiles(testDataPath, "*.MOO.gz")
			.OrderBy(f => f)
			.Select(f => Path.GetFileName(f))
			.ToList();

		if (!testFiles.Any())
		{
			Console.Error.WriteLine($"Error: No test files found in {testDataPath}");
			return 1;
		}

		Console.WriteLine($"Found {testFiles.Count} test files");
		Console.WriteLine();

		// Run tests and collect results
		var runner = new SingleStepTestRunner(NullLogger.Instance);
		var report = new TestReport
		{
			GeneratedAt = DateTime.UtcNow,
			MaxTestsPerFile = maxTestsPerFile,
			FileResults = new List<FileTestResult>()
		};

		foreach (var fileName in testFiles)
		{
			Console.WriteLine($"Processing {fileName}...");
			var filePath = Path.Combine(testDataPath, fileName);
			
			try
			{
				var mooFile = MooFileParser.Parse(filePath);
				var fileResult = new FileTestResult
				{
					FileName = fileName,
					TotalTestsAvailable = mooFile.Tests.Count,
					TestsRun = Math.Min(maxTestsPerFile, mooFile.Tests.Count),
					TestResults = new List<IndividualTestResult>()
				};

				var testCount = Math.Min(maxTestsPerFile, mooFile.Tests.Count);
				
				for (var i = 0; i < testCount; i++)
				{
					var test = mooFile.Tests[i];
					var result = runner.ExecuteTest(test);
					
					fileResult.TestResults.Add(new IndividualTestResult
					{
						TestIndex = i,
						TestName = test.Name,
						Passed = result.Success,
						ExecutionError = result.ExecutionError,
						RegisterMismatches = result.RegisterMismatches.Select(r => new RegisterMismatchInfo
						{
							RegisterName = r.RegisterName,
							Expected = r.Expected,
							Actual = r.Actual
						}).ToList(),
						MemoryMismatchCount = result.MemoryMismatches.Count
					});
				}

				fileResult.PassedCount = fileResult.TestResults.Count(t => t.Passed);
				fileResult.FailedCount = fileResult.TestResults.Count(t => !t.Passed);
				
				report.FileResults.Add(fileResult);
				Console.WriteLine($"  {fileResult.PassedCount}/{fileResult.TestsRun} passed");
			}
			catch (Exception ex)
			{
				// Rethrow fatal exceptions
				if (ex is OutOfMemoryException || ex is StackOverflowException || ex is ThreadAbortException)
					throw;
				
				Console.Error.WriteLine($"  Error processing {fileName}: {ex.Message}");
			}
		}

		// Calculate totals
		report.TotalFiles = report.FileResults.Count;
		report.TotalTestsRun = report.FileResults.Sum(f => f.TestsRun);
		report.TotalPassed = report.FileResults.Sum(f => f.PassedCount);
		report.TotalFailed = report.FileResults.Sum(f => f.FailedCount);

		Console.WriteLine();
		Console.WriteLine("Summary:");
		Console.WriteLine($"  Files: {report.TotalFiles}");
		Console.WriteLine($"  Tests Run: {report.TotalTestsRun}");
		Console.WriteLine($"  Passed: {report.TotalPassed} ({(report.TotalTestsRun > 0 ? (100.0 * report.TotalPassed / report.TotalTestsRun).ToString("F1") : "0.0")}%)");
		Console.WriteLine($"  Failed: {report.TotalFailed} ({(report.TotalTestsRun > 0 ? (100.0 * report.TotalFailed / report.TotalTestsRun).ToString("F1") : "0.0")}%)");
		Console.WriteLine();

		// Save JSON report
		var jsonPath = Path.Combine(outputPath, "test-results.json");
		var jsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		var json = JsonSerializer.Serialize(report, jsonOptions);
		File.WriteAllText(jsonPath, json);
		Console.WriteLine($"JSON report saved to: {jsonPath}");

		// Generate HTML report
		var htmlPath = Path.Combine(outputPath, "index.html");
		GenerateHtmlReport(report, htmlPath);
		Console.WriteLine($"HTML report saved to: {htmlPath}");

		return 0;
	}

	static void GenerateHtmlReport(TestReport report, string outputPath)
	{
		var passRate = report.TotalTestsRun > 0 ? 100.0 * report.TotalPassed / report.TotalTestsRun : 0;
		
		var html = new StringBuilder();
		html.Append($@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Win32Emu SingleStep CPU Test Results</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            max-width: 1400px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 10px;
            margin-bottom: 30px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }}
        .header h1 {{
            margin: 0 0 10px 0;
            font-size: 2.5em;
        }}
        .header p {{
            margin: 5px 0;
            opacity: 0.9;
        }}
        .summary {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }}
        .stat-card {{
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .stat-card h3 {{
            margin: 0 0 10px 0;
            color: #666;
            font-size: 0.9em;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }}
        .stat-card .value {{
            font-size: 2.5em;
            font-weight: bold;
            color: #333;
        }}
        .stat-card .subtitle {{
            color: #999;
            font-size: 0.9em;
            margin-top: 5px;
        }}
        .pass-rate {{
            color: {(passRate >= 75 ? "#10b981" : passRate >= 50 ? "#f59e0b" : "#ef4444")};
        }}
        table {{
            width: 100%;
            background: white;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            border-collapse: collapse;
        }}
        th {{
            background: #f8f9fa;
            padding: 15px;
            text-align: left;
            font-weight: 600;
            color: #333;
            border-bottom: 2px solid #dee2e6;
        }}
        td {{
            padding: 12px 15px;
            border-bottom: 1px solid #f0f0f0;
        }}
        tr:hover {{
            background-color: #f8f9fa;
        }}
        .file-name {{
            font-family: 'Courier New', monospace;
            font-weight: 500;
            color: #667eea;
        }}
        .pass-badge, .fail-badge {{
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 0.85em;
            font-weight: 600;
        }}
        .pass-badge {{
            background-color: #d1fae5;
            color: #065f46;
        }}
        .fail-badge {{
            background-color: #fee2e2;
            color: #991b1b;
        }}
        .progress-bar {{
            width: 200px;
            height: 8px;
            background-color: #e5e7eb;
            border-radius: 4px;
            overflow: hidden;
            display: inline-block;
            vertical-align: middle;
        }}
        .progress-fill {{
            height: 100%;
            background: linear-gradient(90deg, #10b981 0%, #059669 100%);
            transition: width 0.3s ease;
        }}
        .filter-controls {{
            background: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .filter-controls input {{
            padding: 10px 15px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 1em;
            width: 300px;
        }}
        .footer {{
            margin-top: 40px;
            text-align: center;
            color: #666;
            font-size: 0.9em;
        }}
        .details-link {{
            color: #667eea;
            text-decoration: none;
            font-weight: 500;
        }}
        .details-link:hover {{
            text-decoration: underline;
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🖥️ Win32Emu SingleStep CPU Test Results</h1>
        <p>Hardware conformance tests from <a href=""https://github.com/SingleStepTests/80386"" style=""color: white;"">SingleStepTests/80386</a></p>
        <p>Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
    </div>

    <div class=""summary"">
        <div class=""stat-card"">
            <h3>Total Files</h3>
            <div class=""value"">{report.TotalFiles}</div>
            <div class=""subtitle"">Test suites</div>
        </div>
        <div class=""stat-card"">
            <h3>Tests Run</h3>
            <div class=""value"">{report.TotalTestsRun:N0}</div>
            <div class=""subtitle"">{(report.MaxTestsPerFile == int.MaxValue ? "All available tests" : $"{report.MaxTestsPerFile} per file")}</div>
        </div>
        <div class=""stat-card"">
            <h3>Passed</h3>
            <div class=""value"" style=""color: #10b981;"">{report.TotalPassed:N0}</div>
            <div class=""subtitle"">{passRate:F1}% success rate</div>
        </div>
        <div class=""stat-card"">
            <h3>Failed</h3>
            <div class=""value"" style=""color: #ef4444;"">{report.TotalFailed:N0}</div>
            <div class=""subtitle"">{(report.TotalTestsRun > 0 ? 100.0 - passRate : 0):F1}% failure rate</div>
        </div>
    </div>

    <div class=""filter-controls"">
        <input type=""text"" id=""searchInput"" placeholder=""🔍 Search test files..."" onkeyup=""filterTable()"">
    </div>

    <table id=""resultsTable"">
        <thead>
            <tr>
                <th>Test File</th>
                <th>Tests Run</th>
                <th>Passed</th>
                <th>Failed</th>
                <th>Pass Rate</th>
                <th>Progress</th>
            </tr>
        </thead>
        <tbody>
");

		foreach (var file in report.FileResults.OrderBy(f => f.FileName))
		{
			var filePassRate = file.TestsRun > 0 ? 100.0 * file.PassedCount / file.TestsRun : 0;
			var statusBadge = file.PassedCount == file.TestsRun ? "pass-badge" : "fail-badge";

			html.Append($@"
            <tr>
                <td><span class=""file-name"">{file.FileName}</span></td>
                <td>{file.TestsRun} / {file.TotalTestsAvailable}</td>
                <td style=""color: #10b981; font-weight: 600;"">{file.PassedCount}</td>
                <td style=""color: #ef4444; font-weight: 600;"">{file.FailedCount}</td>
                <td><span class=""{statusBadge}"">{filePassRate:F1}%</span></td>
                <td>
                    <div class=""progress-bar"">
                        <div class=""progress-fill"" style=""width: {filePassRate}%""></div>
                    </div>
                </td>
            </tr>
");
		}

		html.Append(@"
        </tbody>
    </table>

    <div class=""footer"">
        <p>
            Win32Emu - Windows 32-bit PE Executable Emulator<br>
            <a href=""https://github.com/archanox/Win32Emu"" class=""details-link"">View on GitHub</a> | 
            <a href=""test-results.json"" class=""details-link"">Download JSON Data</a>
        </p>
    </div>

    <script>
        function filterTable() {
            var input = document.getElementById('searchInput');
            var filter = input.value.toUpperCase();
            var table = document.getElementById('resultsTable');
            var tr = table.getElementsByTagName('tr');

            for (var i = 1; i < tr.length; i++) {
                var td = tr[i].getElementsByTagName('td')[0];
                if (td) {
                    var txtValue = td.textContent || td.innerText;
                    if (txtValue.toUpperCase().indexOf(filter) > -1) {
                        tr[i].style.display = '';
                    } else {
                        tr[i].style.display = 'none';
                    }
                }
            }
        }
    </script>
</body>
</html>");

		File.WriteAllText(outputPath, html.ToString());
	}
}

/// <summary>
/// Complete test report
/// </summary>
public class TestReport
{
	public DateTime GeneratedAt { get; set; }
	public int MaxTestsPerFile { get; set; }
	public int TotalFiles { get; set; }
	public int TotalTestsRun { get; set; }
	public int TotalPassed { get; set; }
	public int TotalFailed { get; set; }
	public List<FileTestResult> FileResults { get; set; } = new();
}

/// <summary>
/// Test results for a single MOO file
/// </summary>
public class FileTestResult
{
	public string FileName { get; set; } = string.Empty;
	public int TotalTestsAvailable { get; set; }
	public int TestsRun { get; set; }
	public int PassedCount { get; set; }
	public int FailedCount { get; set; }
	public List<IndividualTestResult> TestResults { get; set; } = new();
}

/// <summary>
/// Individual test result
/// </summary>
public class IndividualTestResult
{
	public int TestIndex { get; set; }
	public string TestName { get; set; } = string.Empty;
	public bool Passed { get; set; }
	public string? ExecutionError { get; set; }
	public List<RegisterMismatchInfo> RegisterMismatches { get; set; } = new();
	public int MemoryMismatchCount { get; set; }
}

/// <summary>
/// Register mismatch information
/// </summary>
public class RegisterMismatchInfo
{
	public string RegisterName { get; set; } = string.Empty;
	public uint Expected { get; set; }
	public uint Actual { get; set; }
}
