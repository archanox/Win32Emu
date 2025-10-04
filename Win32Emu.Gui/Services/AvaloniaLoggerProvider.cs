using Microsoft.Extensions.Logging;

namespace Win32Emu.Gui.Services
{
	/// <summary>
	/// Custom logger provider that routes log messages to the Avalonia GUI
	/// </summary>
	public sealed class AvaloniaLoggerProvider : ILoggerProvider
	{
		private readonly IGuiEmulatorHost _host;

		public AvaloniaLoggerProvider(IGuiEmulatorHost host)
		{
			_host = host;
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new AvaloniaLogger(categoryName, _host);
		}

		public void Dispose()
		{
			// Nothing to dispose
		}

		private sealed class AvaloniaLogger : ILogger
		{
			private readonly string _categoryName;
			private readonly IGuiEmulatorHost _host;

			public AvaloniaLogger(string categoryName, IGuiEmulatorHost host)
			{
				_categoryName = categoryName;
				_host = host;
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

				var debugLevel = logLevel switch
				{
					LogLevel.Trace => DebugLevel.Trace,
					LogLevel.Debug => DebugLevel.Debug,
					LogLevel.Information => DebugLevel.Info,
					LogLevel.Warning => DebugLevel.Warning,
					LogLevel.Error or LogLevel.Critical => DebugLevel.Error,
					_ => DebugLevel.Debug
				};

				_host.OnDebugOutput(message, debugLevel);
			}
		}
	}
}
