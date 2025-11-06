using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Messaging.Handlers
{
	/// <summary>
	/// Handles WM_CLOSE messages asynchronously
	/// </summary>
	public class CloseMessageHandler : IMessageHandler<CloseMessage>
	{
		private readonly ProcessEnvironment _env;
		private readonly ILogger _logger;

		public CloseMessageHandler(ProcessEnvironment env, ILogger? logger = null)
		{
			_env = env;
			_logger = logger ?? NullLogger.Instance;
		}

		public async Task<uint> HandleAsync(CloseMessage message, CancellationToken cancellationToken = default)
		{
			await Task.Run(() =>
			{
				_logger.LogInformation("[CloseMessageHandler] Handling WM_CLOSE for window 0x{Hwnd:X8}", message.Hwnd);
			
				// Default behavior: destroy the window
				_env.DestroyWindow(message.Hwnd);
			}, cancellationToken);
		
			return 0;
		}
	}
}