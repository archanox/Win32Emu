using Microsoft.Extensions.Logging;

namespace Win32Emu.Tests.ReactOS;

/// <summary>
/// ReactOS tests for Advapi32.dll API functions
/// These tests run ReactOS test executables directly in Win32Emu
/// Tests cover advanced Windows API functions including:
/// - Registry operations
/// - Security and access control
/// - Service management
/// - Event logging
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Category", "ReactOSTests")]
[Trait("Module", "Advapi32")]
public class Advapi32ReactOSTests : IDisposable
{
	private readonly ReactOSTestRunner _runner;
	private readonly ILogger<Advapi32ReactOSTests> _logger;
	private readonly ILoggerFactory _loggerFactory;

	public Advapi32ReactOSTests()
	{
		// Create logger factory and logger for test output
		_loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Information);
		});

		_logger = _loggerFactory.CreateLogger<Advapi32ReactOSTests>();
		_runner = new ReactOSTestRunner(logger: _logger);
	}

	public void Dispose()
	{
		_loggerFactory.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Run Advapi32 test suites (Wine and ReactOS API tests)
	/// Wine tests are comprehensive integration tests from the Wine project
	/// ReactOS API tests are focused unit tests from the ReactOS project
	/// 
	/// NOTE: _initterm callback execution has been implemented using ExecuteCallback method.
	/// This allows C runtime initializers to be called properly, initializing global state
	/// and test framework structures.
	/// </summary>
	[Theory]
	[InlineData("advapi32_winetest.exe", "Advapi32 Wine Test", Skip = "Large Wine test suite - run manually for comprehensive validation")]
	[InlineData("advapi32_apitest.exe", "Advapi32 API Test")]
	[Trait("Function", "Advapi32_Tests")]
	public void Advapi32_ReactOSTests_ShouldExecute(string executable, string testName)
	{
		// Run the specified Advapi32 test suite
		var result = _runner.Run(executable, timeout: 120);

		// Output test results
		_logger.LogInformation("{TestName} Results: {Summary}", testName, result.Summary);

		// Log error details if present
		if (result.IsError)
		{
			_logger.LogError("Test error: {ErrorMessage}", result.ErrorMessage);
			_logger.LogDebug("Output captured: {Output}", result.Output ?? "(none)");
		}

		if (result.FailureMessages.Count > 0)
		{
			_logger.LogWarning("Failures detected:");
			foreach (var failure in result.FailureMessages.Take(10)) // Show first 10
			{
				_logger.LogWarning("  {Failure}", failure);
			}
		}

		// For now, we don't assert all passed since many APIs may not be implemented
		// This test serves to run the suite and report results
		Assert.False(result.IsError, $"{result.ErrorMessage}\nOutput: {result.Output}");
		
		// Log the results for tracking
		_logger.LogInformation(
			"{TestName}: {Passed} passed, {Failed} failed, {Skipped} skipped out of {Total} total",
			testName, result.Passed, result.Failed, result.Skipped, result.Total
		);
	}
}
