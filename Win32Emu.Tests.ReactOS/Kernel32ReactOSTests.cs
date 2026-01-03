using Microsoft.Extensions.Logging;

namespace Win32Emu.Tests.ReactOS;

/// <summary>
/// ReactOS tests for Kernel32.dll API functions
/// These tests run ReactOS test executables directly in Win32Emu
/// Tests cover functions used by ign_teas.exe including:
/// - Memory management (HeapCreate, HeapAlloc, HeapFree, VirtualAlloc)
/// - File I/O (CreateFileA, ReadFile, CloseHandle, GetFileType, SetFilePointer)
/// - Process/Environment (GetVersion, GetCommandLineA, GetEnvironmentStringsW, GetStartupInfoA)
/// - Module management (GetModuleHandleA, GetModuleFileNameA, GetProcAddress)
/// - String conversion (WideCharToMultiByte)
/// - CPU features (IsProcessorFeaturePresent)
/// - Legacy APIs (SetHandleCount, GetACP, GetCPInfo, FreeEnvironmentStringsW)
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Category", "ReactOSTests")]
[Trait("Module", "Kernel32")]
public class Kernel32ReactOSTests : IDisposable
{
	private readonly ReactOSTestRunner _runner;
	private readonly ILogger<Kernel32ReactOSTests> _logger;

	public Kernel32ReactOSTests()
	{
		// Create logger for test output
		using var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Information);
		});

		_logger = loggerFactory.CreateLogger<Kernel32ReactOSTests>();
		_runner = new ReactOSTestRunner(logger: _logger);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Run Kernel32 Wine test suite
	/// Wine tests are comprehensive integration tests from the Wine project
	/// </summary>
	[Theory(Skip = "ReactOS/Wine tests may trigger emulator issues. Run manually to validate Kernel32 implementation. Tests are informational and failures don't block PRs.")]
	[InlineData("kernel32_winetest.exe", "Kernel32 Wine Test")]
	[Trait("Function", "Kernel32_WineTests")]
	public void Kernel32_WineTests_ShouldExecute(string executable, string testName)
	{
		// Run the specified Kernel32 test suite
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

	/// <summary>
	/// Run Kernel32 ReactOS API test suite
	/// ReactOS API tests are focused unit tests from the ReactOS project
	/// </summary>
	[Theory(Skip = "ReactOS/Wine tests may trigger emulator issues. Run manually to validate Kernel32 implementation. Tests are informational and failures don't block PRs.")]
	[InlineData("kernel32_apitest.exe", "Kernel32 API Test")]
	[Trait("Function", "Kernel32_ApiTests")]
	public void Kernel32_ApiTests_ShouldExecute(string executable, string testName)
	{
		// Run the specified Kernel32 test suite
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
