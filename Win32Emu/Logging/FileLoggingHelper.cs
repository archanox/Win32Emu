using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Win32Emu.Logging;

/// <summary>
/// Helper class for configuring file logging with MD5-based filenames
/// </summary>
public static class FileLoggingHelper
{
	/// <summary>
	/// Compute MD5 hash of a file
	/// </summary>
	/// <param name="filePath">Path to the file</param>
	/// <returns>Hexadecimal MD5 hash string (lowercase)</returns>
	public static string ComputeMd5(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"File not found: {filePath}");
		}

		using var stream = File.OpenRead(filePath);
		var hashBytes = MD5.HashData(stream);
		return Convert.ToHexString(hashBytes).ToLowerInvariant();
	}

	/// <summary>
	/// Generate a log file path based on the executable's MD5 hash
	/// </summary>
	/// <param name="executablePath">Path to the executable</param>
	/// <param name="logsDirectory">Optional directory for logs (defaults to current directory)</param>
	/// <returns>Full path to the log file</returns>
	public static string GenerateLogFilePath(string executablePath, string? logsDirectory = null)
	{
		var md5Hash = ComputeMd5(executablePath);
		var executableName = Path.GetFileNameWithoutExtension(executablePath);
		var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		
		// Format: <executableName>_<md5hash>_<timestamp>.log
		var logFileName = $"{executableName}_{md5Hash}_{timestamp}.log";
		
		if (string.IsNullOrEmpty(logsDirectory))
		{
			logsDirectory = Directory.GetCurrentDirectory();
		}
		
		// Create logs directory if it doesn't exist
		if (!Directory.Exists(logsDirectory))
		{
			Directory.CreateDirectory(logsDirectory);
		}
		
		return Path.Combine(logsDirectory, logFileName);
	}

	/// <summary>
	/// Add file logging to a logger factory builder
	/// </summary>
	/// <param name="builder">Logger factory builder</param>
	/// <param name="logFilePath">Path to the log file</param>
	/// <returns>The builder for chaining</returns>
	public static ILoggingBuilder AddFileLogging(this ILoggingBuilder builder, string logFilePath)
	{
		// Create a simple file logger that writes to the specified file
		builder.AddProvider(new FileLoggerProvider(logFilePath));
		return builder;
	}
}

/// <summary>
/// Simple file logger provider
/// </summary>
internal sealed class FileLoggerProvider : ILoggerProvider
{
	private readonly string _logFilePath;
	private readonly StreamWriter _writer;
	private readonly object _lock = new();

	public FileLoggerProvider(string logFilePath)
	{
		_logFilePath = logFilePath;
		// Open file in append mode with UTF-8 encoding
		_writer = new StreamWriter(logFilePath, append: true, encoding: System.Text.Encoding.UTF8)
		{
			AutoFlush = true
		};
	}

	public ILogger CreateLogger(string categoryName)
	{
		return new FileLogger(categoryName, _writer, _lock);
	}

	public void Dispose()
	{
		_writer?.Dispose();
	}
}

/// <summary>
/// Simple file logger implementation
/// </summary>
internal sealed class FileLogger : ILogger
{
	private readonly string _categoryName;
	private readonly StreamWriter _writer;
	private readonly object _lock;

	public FileLogger(string categoryName, StreamWriter writer, object lockObject)
	{
		_categoryName = categoryName;
		_writer = writer;
		_lock = lockObject;
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
	{
		return null;
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return logLevel != LogLevel.None;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
		{
			return;
		}

		var message = formatter(state, exception);
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		var logLevelString = GetLogLevelString(logLevel);

		lock (_lock)
		{
			_writer.WriteLine($"{timestamp} [{logLevelString}] {_categoryName}: {message}");
			if (exception != null)
			{
				_writer.WriteLine(exception.ToString());
			}
		}
	}

	private static string GetLogLevelString(LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Trace => "TRACE",
			LogLevel.Debug => "DEBUG",
			LogLevel.Information => "INFO",
			LogLevel.Warning => "WARN",
			LogLevel.Error => "ERROR",
			LogLevel.Critical => "CRITICAL",
			_ => logLevel.ToString().ToUpperInvariant()
		};
	}
}
