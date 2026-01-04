using Microsoft.Extensions.Logging;

namespace Win32Emu.Tests.ReactOS;

/// <summary>
/// ReactOS tests for User32.dll API functions
/// These tests run ReactOS test executables directly in Win32Emu
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Category", "ReactOSTests")]
[Trait("Module", "User32")]
public class User32ReactOSTests : IDisposable
{
	private readonly ReactOSTestRunner _runner;
	private readonly ILogger<User32ReactOSTests> _logger;
	private readonly ILoggerFactory _loggerFactory;

	public User32ReactOSTests()
	{
		// Create logger factory and logger for test output
		_loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Information);
		});

		_logger = _loggerFactory.CreateLogger<User32ReactOSTests>();
		_runner = new ReactOSTestRunner(logger: _logger);
	}

	public void Dispose()
	{
		_loggerFactory.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Run User32 test suites (Wine and ReactOS API tests)
	/// These tests run ReactOS test executables directly in Win32Emu
	/// 
	/// NOTE: _initterm callback execution has been implemented using ExecuteCallback method.
	/// This allows C runtime initializers to be called properly, initializing global state
	/// and test framework structures.
	/// </summary>
	[Theory]
	[InlineData("user32_apitest.exe", "User32 API Test")]
	[InlineData("user32_dynamic_apitest.exe", "User32 Dynamic Test")]
	[InlineData("user32_apitest_menuui.exe", "User32 Menu UI Test")]
	[InlineData("user32_winetest.exe", "User32 Wine Test", Skip = "Large Wine test suite - run manually for comprehensive validation")]
	[Trait("Function", "User32_Tests")]
	public void User32_ReactOSTests_ShouldExecute(string executable, string testName)
	{
		// Run the specified User32 test suite
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
