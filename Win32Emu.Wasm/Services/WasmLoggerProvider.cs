using Microsoft.Extensions.Logging;

namespace Win32Emu.Wasm.Services;

/// <summary>
/// Custom logger provider that routes log messages from backend components to the debug console in the web frontend.
/// This allows debug logs from WasmRenderingBackend, WasmAudioBackend, WasmInputBackend, and other components
/// to appear in the UI debug panel instead of only the browser console.
/// </summary>
public sealed class WasmLoggerProvider : ILoggerProvider
{
	private readonly WasmDebugLogService _debugLogService;

	/// <summary>
	/// Creates a new WasmLoggerProvider.
	/// </summary>
	/// <param name="debugLogService">The debug log service to forward log messages to.</param>
	public WasmLoggerProvider(WasmDebugLogService debugLogService)
	{
		_debugLogService = debugLogService ?? throw new ArgumentNullException(nameof(debugLogService));
	}

	public ILogger CreateLogger(string categoryName)
	{
		return new WasmLogger(categoryName, _debugLogService);
	}

	public void Dispose()
	{
		// Nothing to dispose
	}

	private sealed class WasmLogger : ILogger
	{
		private readonly string _categoryName;
		private readonly WasmDebugLogService _debugLogService;

		public WasmLogger(string categoryName, WasmDebugLogService debugLogService)
		{
			_categoryName = categoryName;
			_debugLogService = debugLogService;
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			var message = formatter(state, exception);
			if (exception != null)
			{
				message = $"{message}\n{exception}";
			}

			_debugLogService.AddLog(_categoryName, logLevel, message);
		}
	}
}
