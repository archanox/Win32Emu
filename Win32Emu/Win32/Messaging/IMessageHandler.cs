namespace Win32Emu.Win32.Messaging;

/// <summary>
/// Represents a message type that can be handled
/// </summary>
public interface IMessage
{
	/// <summary>
	/// The window handle this message is for
	/// </summary>
	uint Hwnd { get; }
	
	/// <summary>
	/// The message identifier
	/// </summary>
	uint Message { get; }
	
	/// <summary>
	/// Additional message-specific information
	/// </summary>
	uint WParam { get; }
	
	/// <summary>
	/// Additional message-specific information
	/// </summary>
	uint LParam { get; }
}

/// <summary>
/// Handler for a specific message type
/// </summary>
/// <typeparam name="TMessage">The message type this handler processes</typeparam>
public interface IMessageHandler<in TMessage> where TMessage : IMessage
{
	/// <summary>
	/// Handle the message
	/// </summary>
	/// <param name="message">The message to handle</param>
	/// <returns>The result of message processing</returns>
	uint Handle(TMessage message);
}

/// <summary>
/// Async handler for a specific message type
/// </summary>
/// <typeparam name="TMessage">The message type this handler processes</typeparam>
public interface IAsyncMessageHandler<in TMessage> where TMessage : IMessage
{
	/// <summary>
	/// Handle the message asynchronously
	/// </summary>
	/// <param name="message">The message to handle</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of message processing</returns>
	Task<uint> HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for Win32 messages
/// </summary>
public record Win32Message(uint Hwnd, uint Message, uint WParam, uint LParam) : IMessage;
