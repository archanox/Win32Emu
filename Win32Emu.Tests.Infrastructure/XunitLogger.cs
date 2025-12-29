using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Infrastructure;

/// <summary>
/// Logger implementation that writes to xUnit test output
/// </summary>
public class XunitLogger : ILogger
{
	private readonly ITestOutputHelper _output;
	private readonly LogLevel _minLevel;

	public XunitLogger(ITestOutputHelper output, LogLevel minLevel = LogLevel.Information)
	{
		_output = output;
		_minLevel = minLevel;
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
		Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
		{
			return;
		}

		var message = formatter(state, exception);
		var prefix = logLevel switch
		{
			LogLevel.Trace => "[TRACE]",
			LogLevel.Debug => "[DEBUG]",
			LogLevel.Information => "[INFO]",
			LogLevel.Warning => "[WARN]",
			LogLevel.Error => "[ERROR]",
			LogLevel.Critical => "[CRITICAL]",
			_ => "[LOG]"
		};

		_output.WriteLine($"{prefix} {message}");
		
		if (exception != null)
		{
			_output.WriteLine(exception.ToString());
		}
	}
}
