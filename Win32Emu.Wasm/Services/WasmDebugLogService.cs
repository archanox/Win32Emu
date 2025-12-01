using Microsoft.Extensions.Logging;

namespace Win32Emu.Wasm.Services;

/// <summary>
/// Service that collects debug log messages from various components (backends, emulator, etc.)
/// and makes them available to the UI for display in the debug console.
/// This service acts as a central hub for log messages in the WASM environment.
/// </summary>
public sealed class WasmDebugLogService
{
	/// <summary>
	/// Event fired when a new log message is received.
	/// </summary>
	public event EventHandler<WasmDebugLogEventArgs>? LogReceived;

	/// <summary>
	/// Adds a log message to the debug log service.
	/// </summary>
	/// <param name="categoryName">The logger category name (usually the class name).</param>
	/// <param name="logLevel">The log level.</param>
	/// <param name="message">The formatted log message.</param>
	public void AddLog(string categoryName, LogLevel logLevel, string message)
	{
		LogReceived?.Invoke(this, new WasmDebugLogEventArgs(categoryName, logLevel, message));
	}
}

/// <summary>
/// Event arguments for log messages.
/// </summary>
public sealed class WasmDebugLogEventArgs : EventArgs
{
	public string CategoryName { get; }
	public LogLevel LogLevel { get; }
	public string Message { get; }
	public DateTime Timestamp { get; }

	public WasmDebugLogEventArgs(string categoryName, LogLevel logLevel, string message)
	{
		CategoryName = categoryName;
		LogLevel = logLevel;
		Message = message;
		Timestamp = DateTime.UtcNow;
	}

	/// <summary>
	/// Formats the log message for display in the UI.
	/// </summary>
	public string ToFormattedString()
	{
		var levelIndicator = LogLevel switch
		{
			LogLevel.Trace => "TRC",
			LogLevel.Debug => "DBG",
			LogLevel.Information => "INF",
			LogLevel.Warning => "WRN",
			LogLevel.Error => "ERR",
			LogLevel.Critical => "CRT",
			_ => "???"
		};

		// Extract just the class name from the full category (e.g., "Win32Emu.Wasm.Backend.WasmRenderingBackend" -> "WasmRenderingBackend")
		var shortCategory = CategoryName.Contains('.') 
			? CategoryName.Substring(CategoryName.LastIndexOf('.') + 1)
			: CategoryName;

		return $"[{Timestamp:HH:mm:ss}] [{levelIndicator}] [{shortCategory}] {Message}";
	}
}
