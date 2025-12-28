using Microsoft.Extensions.Logging;
using Win32Emu.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests to verify that executable-specific workarounds are applied correctly
/// </summary>
public class ExecutableWorkaroundsTests
{
	private readonly ITestOutputHelper _output;

	public ExecutableWorkaroundsTests(ITestOutputHelper output)
	{
		_output = output;
	}

	/// <summary>
	/// Helper method to find the repository root by looking for Win32Emu.slnx
	/// </summary>
	private static string? FindRepositoryRoot()
	{
		var currentDir = Directory.GetCurrentDirectory();
		var repoRoot = currentDir;
		
		// Navigate up until we find the .slnx file
		while (repoRoot != null && !File.Exists(Path.Combine(repoRoot, "Win32Emu.slnx")))
		{
			var parent = Directory.GetParent(repoRoot);
			if (parent == null)
			{
				break;
			}
			repoRoot = parent.FullName;
		}
		
		return repoRoot;
	}

	[Fact]
	public void IgnTeas_ShouldHaveDebugEnvironmentVariableEnabled()
	{
		// Arrange
		var repoRoot = FindRepositoryRoot();
		
		var exePath = Path.Combine(repoRoot!, "EXEs", "ign_teas", "IGN_TEAS.EXE");
		
		if (!File.Exists(exePath))
		{
			_output.WriteLine($"Test executable not found at: {exePath}");
			return; // Skip test if executable is not available
		}

		var logMessages = new List<string>();
		
		using var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddProvider(new TestLoggerProvider(logMessages));
			builder.SetMinimumLevel(LogLevel.Debug);
		});
		
		var logger = loggerFactory.CreateLogger<Win32Emu.Emulator>();

		// Act
		using var emulator = new Win32Emu.Emulator(logger: logger);
		emulator.LoadExecutable(exePath, debugMode: false, reservedMemoryMb: 256);

		// Assert
		_output.WriteLine("=== Captured Log Messages ===");
		foreach (var message in logMessages)
		{
			_output.WriteLine(message);
		}

		// Verify that IGN_TEAS_DEBUG was automatically enabled
		Assert.Contains(logMessages, msg => msg.Contains("IGN_TEAS.EXE detected - enabled IGN_TEAS_DEBUG environment variable"));
		
		// Verify that the environment variable was set
		var envValue = emulator.Environment?.GetEnvironmentVariable("IGN_TEAS_DEBUG");
		Assert.NotNull(envValue);
		Assert.Equal("1", envValue);
		
		_output.WriteLine($"IGN_TEAS_DEBUG environment variable value: {envValue}");
	}

	[Fact]
	public void IgnTeas_ShouldNotLogEnvironmentVariableNotFound()
	{
		// Arrange
		var repoRoot = FindRepositoryRoot();
		
		var exePath = Path.Combine(repoRoot!, "EXEs", "ign_teas", "IGN_TEAS.EXE");
		
		if (!File.Exists(exePath))
		{
			_output.WriteLine($"Test executable not found at: {exePath}");
			return; // Skip test if executable is not available
		}

		var logMessages = new List<string>();
		
		using var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddProvider(new TestLoggerProvider(logMessages));
			builder.SetMinimumLevel(LogLevel.Debug);
		});
		
		var logger = loggerFactory.CreateLogger<Win32Emu.Emulator>();

		// Act
		using var emulator = new Win32Emu.Emulator(logger: logger);
		emulator.LoadExecutable(exePath, debugMode: false, reservedMemoryMb: 256);
		
		// Try to get the environment variable (should not log "not found")
		var envValue = emulator.Environment?.GetEnvironmentVariable("IGN_TEAS_DEBUG");

		// Assert
		_output.WriteLine("=== Captured Log Messages ===");
		foreach (var message in logMessages)
		{
			_output.WriteLine(message);
		}

		// Verify that we DON'T see the "not found" log message
		Assert.DoesNotContain(logMessages, msg => msg.Contains("GetEnvironmentVariable: 'IGN_TEAS_DEBUG' not found"));
		
		// Verify that the variable is set
		Assert.NotNull(envValue);
		Assert.Equal("1", envValue);
		
		_output.WriteLine($"Test passed - no 'not found' log spam detected");
	}

	/// <summary>
	/// Test logger provider that captures log messages to a list
	/// </summary>
	private class TestLoggerProvider : ILoggerProvider
	{
		private readonly List<string> _logMessages;

		public TestLoggerProvider(List<string> logMessages)
		{
			_logMessages = logMessages;
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new TestLogger(_logMessages);
		}

		public void Dispose()
		{
		}
	}

	/// <summary>
	/// Test logger that captures log messages to a list
	/// </summary>
	private class TestLogger : ILogger
	{
		private readonly List<string> _logMessages;

		public TestLogger(List<string> logMessages)
		{
			_logMessages = logMessages;
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			var message = formatter(state, exception);
			_logMessages.Add(message);
		}
	}
}
