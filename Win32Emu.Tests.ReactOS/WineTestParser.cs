using System.Text.RegularExpressions;

namespace Win32Emu.Tests.ReactOS;

/// <summary>
/// Parses Wine test framework output format
/// </summary>
public static class WineTestParser
{
	// Wine test output patterns
	// Examples:
	//   file.c:123: Test succeeded
	//   file.c:456: Test failed: expected X, got Y
	//   file.c:789: Tests skipped: reason
	//   Summary: 45 tests executed (43 passed, 2 failed, 0 skipped)

	private static readonly Regex TestFailedRegex = new(
		@"^[^:]+:\d+:\s+Test failed:",
		RegexOptions.Compiled | RegexOptions.Multiline
	);

	private static readonly Regex TestSkippedRegex = new(
		@"^[^:]+:\d+:\s+Tests? skipped:",
		RegexOptions.Compiled | RegexOptions.Multiline
	);

	private static readonly Regex SummaryRegex = new(
		@"^(\w+):\s+(\d+)\s+tests?\s+executed.*?\((\d+)\s+passed,\s+(\d+)\s+failed,\s+(\d+)\s+skipped\)",
		RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase
	);

	private static readonly Regex AlternateSummaryRegex = new(
		@"(\d+)\s+tests?\s+executed.*?(\d+)\s+marked as failed",
		RegexOptions.Compiled | RegexOptions.IgnoreCase
	);

	public static ReactOSTestResult Parse(string output)
	{
		var result = new ReactOSTestResult
		{
			Output = output
		};

		if (string.IsNullOrWhiteSpace(output))
		{
			result.IsError = true;
			result.ErrorMessage = "No output captured from test";
			return result;
		}

		// Try to parse summary line first
		var summaryMatch = SummaryRegex.Match(output);

		if (summaryMatch.Success)
		{
			// Parse from Wine test summary format
			result.Total = int.Parse(summaryMatch.Groups[2].Value);
			result.Passed = int.Parse(summaryMatch.Groups[3].Value);
			result.Failed = int.Parse(summaryMatch.Groups[4].Value);
			result.Skipped = int.Parse(summaryMatch.Groups[5].Value);
			result.Summary = $"{result.Passed}/{result.Total} tests passed, {result.Failed} failed, {result.Skipped} skipped";
		}
		else
		{
			// Try alternate format
			var altMatch = AlternateSummaryRegex.Match(output);

			if (altMatch.Success)
			{
				result.Total = int.Parse(altMatch.Groups[1].Value);
				result.Failed = int.Parse(altMatch.Groups[2].Value);
				result.Passed = result.Total - result.Failed;
				result.Skipped = 0;
				result.Summary = $"{result.Passed}/{result.Total} tests passed, {result.Failed} failed";
			}
			else
			{
				// Count failures and skips manually
				result.Failed = TestFailedRegex.Matches(output).Count;
				result.Skipped = TestSkippedRegex.Matches(output).Count;

				// Estimate total from output (rough heuristic)
				// If we have failures but no summary, we can't determine exact total
				if (result.Failed > 0 || result.Skipped > 0)
				{
					result.Total = result.Failed + result.Skipped;
					result.Passed = 0; // Unknown
					result.Summary = $"At least {result.Failed} failed, {result.Skipped} skipped (no summary found)";
				}
				else
				{
					// No failures detected, assume all passed
					result.Total = 1;
					result.Passed = 1;
					result.Failed = 0;
					result.Skipped = 0;
					result.Summary = "Test appears to have passed (no failures detected)";
				}
			}
		}

		// Extract failure messages
		var lines = output.Split('\n');
		var failedLines = lines.Where(line => TestFailedRegex.IsMatch(line));
		foreach (var line in failedLines)
		{
			result.FailureMessages.Add(line.Trim());
		}

		return result;
	}
}
