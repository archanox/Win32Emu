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

	[Fact]
	[Trait("Function", "User32_ApiTest")]
	public void User32_ApiTest_ShouldExecute()
	{
		// Run the main User32 API test suite
		var result = _runner.Run("user32_apitest.exe", timeout: 120);

		// Output test results
		_logger.LogInformation("User32 API Test Results: {Summary}", result.Summary);

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
			"User32 Tests: {Passed} passed, {Failed} failed, {Skipped} skipped out of {Total} total",
			result.Passed, result.Failed, result.Skipped, result.Total
		);
	}

	[Fact]
	[Trait("Function", "User32_Dynamic")]
	[Trait("Status", "Optional")]
	public void User32_DynamicTests_ShouldExecute()
	{
		// Run the User32 dynamic API tests
		var result = _runner.Run("user32_dynamic_apitest.exe", timeout: 120);

		_logger.LogInformation("User32 Dynamic API Test Results: {Summary}", result.Summary);

		if (result.FailureMessages.Count > 0)
		{
			_logger.LogWarning("Failures detected:");
			foreach (var failure in result.FailureMessages.Take(10))
			{
				_logger.LogWarning("  {Failure}", failure);
			}
		}

		Assert.False(result.IsError, result.ErrorMessage);

		_logger.LogInformation(
			"User32 Dynamic Tests: {Passed} passed, {Failed} failed, {Skipped} skipped out of {Total} total",
			result.Passed, result.Failed, result.Skipped, result.Total
		);
	}

	[Fact]
	[Trait("Function", "User32_MenuUI")]
	[Trait("Status", "Optional")]
	public void User32_MenuUITests_ShouldExecute()
	{
		// Run the User32 menu UI tests
		var result = _runner.Run("user32_apitest_menuui.exe", timeout: 120);

		_logger.LogInformation("User32 Menu UI Test Results: {Summary}", result.Summary);

		if (result.FailureMessages.Count > 0)
		{
			_logger.LogWarning("Failures detected:");
			foreach (var failure in result.FailureMessages.Take(10))
			{
				_logger.LogWarning("  {Failure}", failure);
			}
		}

		Assert.False(result.IsError, result.ErrorMessage);

		_logger.LogInformation(
			"User32 Menu UI Tests: {Passed} passed, {Failed} failed, {Skipped} skipped out of {Total} total",
			result.Passed, result.Failed, result.Skipped, result.Total
		);
	}

	[Fact]
	[Trait("Function", "User32_WineTest")]
	[Trait("Status", "Optional")]
	public void User32_WineTest_ShouldExecute()
	{
		// Run the Wine test suite for User32
		// Wine tests provide comprehensive coverage and are equally valuable
		var result = _runner.Run("user32_winetest.exe", timeout: 120);

		_logger.LogInformation("User32 Wine Test Results: {Summary}", result.Summary);

		if (result.FailureMessages.Count > 0)
		{
			_logger.LogWarning("Failures detected:");
			foreach (var failure in result.FailureMessages.Take(10))
			{
				_logger.LogWarning("  {Failure}", failure);
			}
		}

		Assert.False(result.IsError, result.ErrorMessage);

		_logger.LogInformation(
			"User32 Wine Tests: {Passed} passed, {Failed} failed, {Skipped} skipped out of {Total} total",
			result.Passed, result.Failed, result.Skipped, result.Total
		);
	}
}
