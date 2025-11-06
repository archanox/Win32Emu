namespace Win32Emu.Win32.Messaging;

/// <summary>
/// Async handler for a specific message type
/// </summary>
/// <typeparam name="TMessage">The message type this handler processes</typeparam>
public interface IMessageHandler<in TMessage> where TMessage : IMessage
{
	/// <summary>
	/// Handle the message asynchronously
	/// </summary>
	/// <param name="message">The message to handle</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of message processing</returns>
	Task<uint> HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}