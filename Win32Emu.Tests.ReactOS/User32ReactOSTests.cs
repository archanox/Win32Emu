using Microsoft.Extensions.Logging;

namespace Win32Emu.Tests.ReactOS;

/// <summary>
/// ReactOS tests for User32.dll API functions
/// These tests run ReactOS test executables directly in Win32Emu
/// </summary>
[Trait("Module", "User32")]
public class User32ReactOSTests : IDisposable
{
	private readonly ReactOSTestRunner _runner;
	private readonly ILogger<User32ReactOSTests> _logger;

	public User32ReactOSTests()
	{
		// Create logger for test output
		using var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Information);
		});

		_logger = loggerFactory.CreateLogger<User32ReactOSTests>();
		_runner = new ReactOSTestRunner(logger: _logger);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	[Theory]
	[InlineData("user32_apitest.exe", "User32 API Test", false)]
	[InlineData("user32_dynamic_apitest.exe", "User32 Dynamic Test", true)]
	[InlineData("user32_apitest_menuui.exe", "User32 Menu UI Test", true)]
	[InlineData("user32_winetest.exe", "User32 Wine Test", true)]
	[Trait("Function", "User32_Tests")]
	public void User32_ReactOSTests_ShouldExecute(string executable, string testName, bool isOptional)
	{
		// Run the specified User32 test suite
		var result = _runner.Run(executable, timeout: 120);

		// Output test results
		_logger.LogInformation("{TestName} Results: {Summary}", testName, result.Summary);

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
		Assert.False(result.IsError, result.ErrorMessage);
		
		// Log the results for tracking
		_logger.LogInformation(
			"{TestName}: {Passed} passed, {Failed} failed, {Skipped} skipped out of {Total} total",
			testName, result.Passed, result.Failed, result.Skipped, result.Total
		);
	}
}
