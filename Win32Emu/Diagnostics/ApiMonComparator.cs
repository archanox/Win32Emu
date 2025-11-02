using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Diagnostics;

/// <summary>
/// Compares emulated API behavior against real Windows behavior captured by API Monitor.
/// Helps identify where emulation diverges from expected behavior.
/// </summary>
public class ApiMonComparator
{
	private readonly ILogger _logger;
	private readonly List<ApiMonRecord> _expectedCalls = new();
	private readonly List<ApiCallRecord> _actualCalls = new();

	public ApiMonComparator(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
	}

	/// <summary>
	/// Load expected behavior from API Monitor CSV log file
	/// </summary>
	public bool LoadExpectedBehavior(string csvPath)
	{
		try
		{
			if (!File.Exists(csvPath))
			{
				_logger.LogError("API Monitor CSV file not found: {Path}", csvPath);
				return false;
			}

			var lines = File.ReadAllLines(csvPath);
			_logger.LogInformation("Loading API Monitor log from {Path} ({LineCount} lines)", csvPath, lines.Length);

			var recordCount = 0;
			for (var i = 1; i < lines.Length; i++) // Skip header
			{
				try
				{
					var record = ParseApiMonCsvLine(lines[i]);
					if (record != null)
					{
						_expectedCalls.Add(record);
						recordCount++;
					}
				}
				catch (FormatException ex)
				{
					_logger.LogWarning(ex, "Failed to parse CSV line {LineNumber} due to format error", i + 1);
				}
				catch (ArgumentException ex)
				{
					_logger.LogWarning(ex, "Failed to parse CSV line {LineNumber} due to argument error", i + 1);
				}
				catch (IndexOutOfRangeException ex)
				{
					_logger.LogWarning(ex, "Failed to parse CSV line {LineNumber} due to index error", i + 1);
				}
			}

			_logger.LogInformation("Loaded {RecordCount} API Monitor records", recordCount);
			return recordCount > 0;
		}
		catch (IOException ex)
		{
			_logger.LogError(ex, "IO error while loading API Monitor CSV: {Message}", ex.Message);
			return false;
		}
		catch (UnauthorizedAccessException ex)
		{
			_logger.LogError(ex, "Access denied while loading API Monitor CSV: {Message}", ex.Message);
			return false;
		}
		catch (FormatException ex)
		{
			_logger.LogError(ex, "Format error while loading API Monitor CSV: {Message}", ex.Message);
			return false;
		}
	}

	/// <summary>
	/// Add an actual API call from the emulator
	/// </summary>
	public void RecordActualCall(ApiCallRecord call)
	{
		_actualCalls.Add(call);
	}

	/// <summary>
	/// Compare expected vs actual behavior and generate a report
	/// </summary>
	public string GenerateComparisonReport()
	{
		var sb = new StringBuilder();
		sb.AppendLine("API Behavior Comparison Report");
		sb.AppendLine("=" + new string('=', 79));
		sb.AppendLine();
		sb.AppendLine($"Expected API calls (API Monitor): {_expectedCalls.Count:N0}");
		sb.AppendLine($"Actual API calls (Emulated):     {_actualCalls.Count:N0}");
		sb.AppendLine();

		// Find divergence point
		var divergenceIndex = FindDivergencePoint();
		if (divergenceIndex >= 0)
		{
			sb.AppendLine($"Behavior diverges at call #{divergenceIndex + 1}:");
			sb.AppendLine("-" + new string('-', 79));

			if (divergenceIndex < _expectedCalls.Count)
			{
				sb.AppendLine($"Expected: {FormatApiMonRecord(_expectedCalls[divergenceIndex])}");
			}
			else
			{
				sb.AppendLine("Expected: (end of log)");
			}

			if (divergenceIndex < _actualCalls.Count)
			{
				sb.AppendLine($"Actual:   {FormatApiCallRecord(_actualCalls[divergenceIndex])}");
			}
			else
			{
				sb.AppendLine("Actual:   (emulation stopped)");
			}

			sb.AppendLine();

			// Show context (previous 5 calls)
			sb.AppendLine("Context (previous 5 calls):");
			sb.AppendLine("-" + new string('-', 79));
			var contextStart = Math.Max(0, divergenceIndex - 5);
			for (var i = contextStart; i < divergenceIndex; i++)
			{
				sb.AppendLine($"  [{i + 1}] Expected: {FormatApiMonRecord(_expectedCalls[i])}");
				if (i < _actualCalls.Count)
				{
					sb.AppendLine($"      Actual:   {FormatApiCallRecord(_actualCalls[i])}");
				}
			}
			sb.AppendLine();
		}
		else
		{
			sb.AppendLine("✓ Behavior matches expected pattern");
			sb.AppendLine();
		}

		// API call frequency comparison
		sb.AppendLine("API Call Frequency Comparison:");
		sb.AppendLine("-" + new string('-', 79));
		var expectedFreq = GetApiFrequency(_expectedCalls);
		var actualFreq = GetApiFrequency(_actualCalls);

		var allApis = expectedFreq.Keys.Union(actualFreq.Keys).OrderBy(k => k).ToList();
		sb.AppendLine($"{"API",-40} {"Expected",12} {"Actual",12} {"Diff",12}");
		sb.AppendLine(new string('-', 80));

		foreach (var api in allApis.Take(30)) // Show top 30
		{
			var expected = expectedFreq.GetValueOrDefault(api, 0);
			var actual = actualFreq.GetValueOrDefault(api, 0);
			var diff = actual - expected;
			var diffStr = diff >= 0 ? $"+{diff}" : diff.ToString();

			sb.AppendLine($"{api,-40} {expected,12:N0} {actual,12:N0} {diffStr,12}");
		}

		sb.AppendLine();

		// Missing APIs
		var missingApis = expectedFreq.Keys.Except(actualFreq.Keys).ToList();
		if (missingApis.Any())
		{
			sb.AppendLine($"Missing APIs (called in real Windows but not in emulator): {missingApis.Count}");
			sb.AppendLine("-" + new string('-', 79));
			foreach (var api in missingApis.Take(20))
			{
				var count = expectedFreq[api];
				sb.AppendLine($"  {api,-50} (called {count} times)");
			}
			sb.AppendLine();
		}

		// Extra APIs
		var extraApis = actualFreq.Keys.Except(expectedFreq.Keys).ToList();
		if (extraApis.Any())
		{
			sb.AppendLine($"Extra APIs (called in emulator but not in real Windows): {extraApis.Count}");
			sb.AppendLine("-" + new string('-', 79));
			foreach (var api in extraApis.Take(20))
			{
				var count = actualFreq[api];
				sb.AppendLine($"  {api,-50} (called {count} times)");
			}
			sb.AppendLine();
		}

		return sb.ToString();
	}

	private int FindDivergencePoint()
	{
		var maxComparisons = Math.Min(_expectedCalls.Count, _actualCalls.Count);

		for (var i = 0; i < maxComparisons; i++)
		{
			var expected = _expectedCalls[i];
			var actual = _actualCalls[i];

			// Compare API names (module + function)
			var expectedApi = $"{expected.Module}.{expected.Api}";
			var actualApi = $"{actual.ModuleName}.{actual.FunctionName}";

			if (!string.Equals(expectedApi, actualApi, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}

		// If we got here and counts differ, divergence is at the shorter list's end
		if (_expectedCalls.Count != _actualCalls.Count)
		{
			return maxComparisons;
		}

		// No divergence found
		return -1;
	}

	private Dictionary<string, int> GetApiFrequency(List<ApiMonRecord> records)
	{
		return records
			.GroupBy(r => $"{r.Module}.{r.Api}")
			.ToDictionary(g => g.Key, g => g.Count());
	}

	private Dictionary<string, int> GetApiFrequency(List<ApiCallRecord> records)
	{
		return records
			.GroupBy(r => $"{r.ModuleName}.{r.FunctionName}")
			.ToDictionary(g => g.Key, g => g.Count());
	}

	private static string FormatApiMonRecord(ApiMonRecord record)
	{
		return $"{record.Module}.{record.Api}(...) = {record.ReturnValue ?? "(none)"}";
	}

	private static string FormatApiCallRecord(ApiCallRecord record)
	{
		return $"{record.ModuleName}.{record.FunctionName}(...) = {record.ReturnValue ?? "(none)"}";
	}

	private static ApiMonRecord? ParseApiMonCsvLine(string line)
	{
		// API Monitor CSV format:
		// #,Time of Day,Thread,Module,API,Return Value,Error,Duration

		// Simple CSV parsing (handles quoted fields)
		var fields = ParseCsvLine(line);
		if (fields.Count < 6)
		{
			return null;
		}

		return new ApiMonRecord
		{
			CallNumber = int.TryParse(fields[0], out var n) ? n : 0,
			TimeOfDay = fields[1],
			Thread = fields[2],
			Module = fields[3],
			Api = fields[4],
			ReturnValue = fields[5],
			Error = fields.Count > 6 ? fields[6] : null,
			Duration = fields.Count > 7 ? fields[7] : null
		};
	}

	private static List<string> ParseCsvLine(string line)
	{
		var fields = new List<string>();
		var inQuotes = false;
		var fieldStart = 0;

		for (var i = 0; i < line.Length; i++)
		{
			var c = line[i];

			if (c == '"')
			{
				inQuotes = !inQuotes;
			}
			else if (c == ',' && !inQuotes)
			{
				fields.Add(line[fieldStart..i].Trim(' ', '"'));
				fieldStart = i + 1;
			}
		}

		// Add last field
		if (fieldStart < line.Length)
		{
			fields.Add(line[fieldStart..].Trim(' ', '"'));
		}

		return fields;
	}
}

/// <summary>
/// Record from API Monitor CSV log
/// </summary>
public class ApiMonRecord
{
	public int CallNumber { get; init; }
	public string TimeOfDay { get; init; } = string.Empty;
	public string Thread { get; init; } = string.Empty;
	public string Module { get; init; } = string.Empty;
	public string Api { get; init; } = string.Empty;
	public string? ReturnValue { get; init; }
	public string? Error { get; init; }
	public string? Duration { get; init; }
}
