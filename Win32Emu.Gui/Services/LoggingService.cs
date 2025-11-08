using Microsoft.Extensions.Logging;
using Win32Emu.Gui.Models;
using Win32Emu.Logging;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Centralized logging service for the GUI application.
/// This service manages the application-wide logger factory and ensures
/// logging is available from the very start of the application lifecycle.
/// </summary>
public sealed class LoggingService : IDisposable
{
	private readonly ILoggerFactory _loggerFactory;
	private bool _disposed;

	public LoggingService(EmulatorConfiguration configuration)
	{
		// Create the logger factory using the static LoggerFactory.Create method
		_loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
		{
			builder
				.AddConsole()
				.SetMinimumLevel(configuration.EnableDebugMode ? LogLevel.Debug : LogLevel.Information);

			// Add file logging if enabled
			if (configuration.EnableFileLogging)
			{
				try
				{
					// Generate a log file path for the GUI session
					var logFileName = $"win32emu_gui_{DateTime.UtcNow:yyyyMMdd_HHmmss}.log";
					var logFilePath = string.IsNullOrEmpty(configuration.LogFileDirectory)
						? Path.Combine(Directory.GetCurrentDirectory(), logFileName)
						: Path.Combine(configuration.LogFileDirectory, logFileName);

					builder.AddFileLogging(logFilePath);

					// Log to console that file logging is enabled
					Console.WriteLine($"GUI logging enabled: {logFilePath}");
				}
				catch (Exception ex)
				{
					// If we can't enable file logging, just log to console
					Console.WriteLine($"Warning: Could not enable file logging: {ex.Message}");
				}
			}
		});
	}

	/// <summary>
	/// Get the logger factory for creating loggers
	/// </summary>
	public ILoggerFactory LoggerFactory => _loggerFactory;

	/// <summary>
	/// Create a logger for the specified type
	/// </summary>
	public ILogger<T> CreateLogger<T>() => _loggerFactory.CreateLogger<T>();

	/// <summary>
	/// Create a logger with the specified category name
	/// </summary>
	public ILogger CreateLogger(string categoryName) => _loggerFactory.CreateLogger(categoryName);

	public void Dispose()
	{
		if (!_disposed)
		{
			_loggerFactory.Dispose();
			_disposed = true;
		}
	}
}
