using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator.TestInfrastructure;

/// <summary>
/// Shared test logger infrastructure for capturing log output across tests
/// </summary>
public static class TestLoggerHelper
{
	/// <summary>
	/// Creates a logger factory that captures logs to a concurrent bag and outputs to xUnit
	/// </summary>
	public static (ILoggerFactory factory, ConcurrentBag<string> logMessages) CreateTestLoggerFactory(
		ITestOutputHelper output,
		LogLevel minimumLevel = LogLevel.Debug)
	{
		var logMessages = new ConcurrentBag<string>();
		var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddProvider(new TestLoggerProvider((category, level, message) =>
			{
				var logEntry = $"[{level}] [{category}] {message}";
				logMessages.Add(logEntry);
				output.WriteLine(logEntry);
			}));
			builder.SetMinimumLevel(minimumLevel);
		});

		return (loggerFactory, logMessages);
	}
}

/// <summary>
/// Test logger provider for capturing log output
/// </summary>
internal class TestLoggerProvider : ILoggerProvider
{
	private readonly Action<string, LogLevel, string> _logAction;

	public TestLoggerProvider(Action<string, LogLevel, string> logAction)
	{
		_logAction = logAction;
	}

	public ILogger CreateLogger(string categoryName) => new TestLogger(categoryName, _logAction);

	public void Dispose() { }
}

/// <summary>
/// Test logger for capturing log output
/// </summary>
internal class TestLogger : ILogger
{
	private readonly string _categoryName;
	private readonly Action<string, LogLevel, string> _logAction;

	public TestLogger(string categoryName, Action<string, LogLevel, string> logAction)
	{
		_categoryName = categoryName;
		_logAction = logAction;
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		var message = formatter(state, exception);
		_logAction(_categoryName, logLevel, message);
	}
}
