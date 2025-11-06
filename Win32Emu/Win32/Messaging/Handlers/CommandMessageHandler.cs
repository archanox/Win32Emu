using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Messaging.Handlers
{
	/// <summary>
	/// Handles WM_COMMAND messages asynchronously
	/// </summary>
	public class CommandMessageHandler : IMessageHandler<CommandMessage>
	{
		private readonly ILogger _logger;

		public CommandMessageHandler(ILogger? logger = null)
		{
			_logger = logger ?? NullLogger.Instance;
		}

		public async Task<uint> HandleAsync(CommandMessage message, CancellationToken cancellationToken = default)
		{
			await Task.Run(() =>
			{
				_logger.LogInformation(
					"[CommandMessageHandler] Handling WM_COMMAND for window 0x{Hwnd:X8}, ControlId={ControlId}, NotificationCode={NotificationCode}",
					message.Hwnd, message.ControlId, message.NotificationCode);
			
				// Default handling - specific command handlers would be registered separately
			}, cancellationToken);
		
			return 0;
		}
	}
}