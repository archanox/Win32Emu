using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Win32Emu.Tests.ReactOS;

/// <summary>
/// Result of running a ReactOS test executable
/// </summary>
public class ReactOSTestResult
{
	public int Total { get; set; }
	public int Passed { get; set; }
	public int Failed { get; set; }
	public int Skipped { get; set; }
	public bool AllPassed => Failed == 0 && !IsError;
	public string Summary { get; set; } = string.Empty;
	public List<string> FailureMessages { get; set; } = new();
	public string Output { get; set; } = string.Empty;
	public bool IsError { get; set; }
	public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Runs ReactOS test executables in Win32Emu and parses Wine test framework output
/// </summary>
public class ReactOSTestRunner
{
	private readonly string _apiTestsPath;
	private readonly ILogger? _logger;

	public ReactOSTestRunner(string? apiTestsPath = null, ILogger? logger = null)
	{
		// Default to EXEs/ApiTests relative to repository root
		_apiTestsPath = apiTestsPath ?? Path.Combine(
			GetRepositoryRoot(),
			"EXEs",
			"ApiTests"
		);
		_logger = logger;
	}

	/// <summary>
	/// Run a ReactOS test executable
	/// </summary>
	/// <param name="testExecutable">Name of the test executable (e.g., "user32_apitest.exe")</param>
	/// <param name="timeout">Optional timeout in seconds (default: 60)</param>
	/// <returns>Test result with parsed output</returns>
	public ReactOSTestResult Run(string testExecutable, int timeout = 60)
	{
		var testPath = Path.Combine(_apiTestsPath, testExecutable);

		if (!File.Exists(testPath))
		{
			return new ReactOSTestResult
			{
				IsError = true,
				ErrorMessage = $"Test executable not found: {testPath}"
			};
		}

		try
		{
			_logger?.LogInformation("Running ReactOS test: {TestExecutable}", testExecutable);

			// Capture console output
			var output = new StringBuilder();
			var originalOut = Console.Out;
			var originalError = Console.Error;

			using var outputWriter = new StringWriter(output);
			Console.SetOut(outputWriter);
			Console.SetError(outputWriter);

			try
			{
				// Create emulator instance
				using var emulator = new Emulator(logger: _logger);

				// Read executable bytes
				var executableBytes = File.ReadAllBytes(testPath);

				// Load the test executable
				emulator.LoadExecutableFromBytes(
					executableBytes,
					testExecutable,
					programArgs: null,
					debugMode: true,
					reservedMemoryMb: 256
				);

				// Run with timeout using Task and CancellationToken
				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));

				try
				{
					// Run asynchronously with cancellation support
					var runTask = Task.Run(() =>
					{
						emulator.Run();
					}, cts.Token);

					runTask.Wait(cts.Token);

					_logger?.LogInformation("Test completed successfully");
				}
				catch (OperationCanceledException)
				{
					return new ReactOSTestResult
					{
						IsError = true,
						ErrorMessage = $"Test timed out after {timeout} seconds"
					};
				}
				catch (AggregateException aex)
				{
					// Unwrap inner exception(s) from Task
					// Flatten to handle nested AggregateExceptions properly
					var flattened = aex.Flatten();
					var innerEx = flattened.InnerExceptions.FirstOrDefault() ?? aex;
					_logger?.LogError(innerEx, "Test execution failed: {TestExecutable}", testExecutable);
					return new ReactOSTestResult
					{
						IsError = true,
						ErrorMessage = $"Test execution failed: {innerEx.Message}"
					};
				}
			}
			finally
			{
				Console.SetOut(originalOut);
				Console.SetError(originalError);
			}

			var testOutput = output.ToString();
			_logger?.LogDebug("Test output:\n{Output}", testOutput);

			// Parse Wine test framework output
			return WineTestParser.Parse(testOutput);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Error running test: {TestExecutable}", testExecutable);
			return new ReactOSTestResult
			{
				IsError = true,
				ErrorMessage = $"Exception: {ex.Message}"
			};
		}
	}

	private static string GetRepositoryRoot()
	{
		// Walk up from current directory to find repository root
		var currentDir = Directory.GetCurrentDirectory();

		while (currentDir != null)
		{
			if (Directory.Exists(Path.Combine(currentDir, ".git")) ||
			    File.Exists(Path.Combine(currentDir, "Win32Emu.slnx")))
			{
				return currentDir;
			}

			currentDir = Directory.GetParent(currentDir)?.FullName;
		}

		// Fallback to current directory
		return Directory.GetCurrentDirectory();
	}
}
